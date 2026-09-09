using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using NDLP_Project.Data;
using NDLP_Project.DTOs;
using NDLP_Project.Models.Entities;
using NDLP_Project.Services;

namespace NDLP_Project.Controllers
{
    [RoutePrefix("api/account-queries")]
    public class AccountQueriesController : BaseApiController // <-- Inherits BaseApiController (Ticket 10)
    {
        private readonly AccountDbContext _db = new AccountDbContext();
        private readonly AccountValidationService _validationService = new AccountValidationService(); // <-- (Tickets 8 & 11)

        // =========================================================================
        // TICKET 3, 8, 10, 11, 15, 16: POST api/account-queries
        // =========================================================================
        [HttpPost]
        [Route("")]
        public IHttpActionResult CreateQuery([FromBody] AccountQueryRequestDto requestDto)
        {
            // 1. Validate payload using Ticket 10 standard format
            if (requestDto == null)
            {
                return StandardBadRequest("EMPTY_REQUEST", "Request body cannot be null.");
            }

            if (!ModelState.IsValid)
            {
                return StandardValidationError(ModelState);
            }

            // 2. [TICKET 8] Validate Account Number Format
            var accFormatCheck = _validationService.ValidateAccountNumber(requestDto.AccountNumber);
            if (!accFormatCheck.IsSuccess)
            {
                return StandardBadRequest(accFormatCheck.ErrorCode, accFormatCheck.ErrorMessage);
            }

            // 3. Locate the account
            var account = _db.CustomerAccounts
                .FirstOrDefault(a => a.AccountNumber == requestDto.AccountNumber.Trim() && !a.IsDeleted);

            if (account == null)
            {
                return StandardNotFound("ACCOUNT_NOT_FOUND", $"Account '{requestDto.AccountNumber}' does not exist or is inactive.");
            }

            // 4. [TICKET 8 & 11] Validate Ownership
            var ownershipCheck = _validationService.ValidateOwnership(account, requestDto.CustomerId);
            if (!ownershipCheck.IsSuccess)
            {
                return StandardBadRequest(ownershipCheck.ErrorCode, ownershipCheck.ErrorMessage);
            }

            // 5. [TICKET 8 & 11] Validate Account Status
            var statusCheck = _validationService.ValidateActiveStatus(account, "query submission");
            if (!statusCheck.IsSuccess)
            {
                return StandardBadRequest(statusCheck.ErrorCode, statusCheck.ErrorMessage);
            }

            // 6. [TICKET 8 & 11] Validate Permitted Query Types for this account
            var queryTypeCheck = _validationService.ValidatePermittedQueryType(account.AccountType, requestDto.QueryType);
            if (!queryTypeCheck.IsSuccess)
            {
                return StandardBadRequest(queryTypeCheck.ErrorCode, queryTypeCheck.ErrorMessage);
            }

            // 7. [TICKET 16] Duplicate Request Protection
            if (!string.IsNullOrWhiteSpace(requestDto.RequestReference))
            {
                bool isDuplicate = _db.AccountQueryRequests
                    .Any(q => q.RequestReference == requestDto.RequestReference.Trim());

                if (isDuplicate)
                {
                    return StandardConflict("DUPLICATE_REQUEST",
                        $"A query with reference '{requestDto.RequestReference}' already exists.");
                }
            }
            else
            {
                requestDto.RequestReference = $"REQ-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}";
            }

            // 8. [TICKET 15] Atomic Database Transaction
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var queryEntity = new AccountQueryRequest
                    {
                        Id = Guid.NewGuid(),
                        RequestReference = requestDto.RequestReference.Trim(),
                        AccountNumber = account.AccountNumber,
                        CustomerId = account.CustomerId,
                        QueryType = requestDto.QueryType.Trim().ToUpper(),
                        Status = "PENDING",
                        Purpose = requestDto.Purpose,
                        RequestedBy = User?.Identity?.Name ?? "SystemUser",
                        RequestedDate = DateTime.UtcNow
                    };

                    _db.AccountQueryRequests.Add(queryEntity);
                    _db.SaveChanges();

                    // If multiple DB records were involved, they would be saved here before commit
                    transaction.Commit();

                    var responseDto = MapToResponseDto(queryEntity);
                    return Content(HttpStatusCode.Created, responseDto);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return StandardBadRequest("TRANSACTION_FAILED", "Failed to process query transaction: " + ex.Message);
                }
            }
        }

        // =========================================================================
        // TICKET 6: PUT api/account-queries/{id}
        // =========================================================================
        [HttpPut]
        [Route("{id:guid}")]
        public IHttpActionResult UpdateQuery(Guid id, [FromBody] AccountQueryUpdateDto updateDto)
        {
            if (updateDto == null)
            {
                return StandardBadRequest("EMPTY_REQUEST", "Update payload cannot be null.");
            }

            if (!ModelState.IsValid)
            {
                return StandardValidationError(ModelState);
            }

            var query = _db.AccountQueryRequests.Find(id);
            if (query == null)
            {
                return StandardNotFound("QUERY_NOT_FOUND", $"Account query with ID '{id}' was not found.");
            }

            if (query.Status == "COMPLETED" || query.Status == "REJECTED")
            {
                return StandardBadRequest("IMMUTABLE_QUERY", $"Cannot update a query request that has already been {query.Status}.");
            }

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    query.Status = updateDto.Status.Trim().ToUpper();

                    if (!string.IsNullOrWhiteSpace(updateDto.ResolutionNotes))
                    {
                        query.ResolutionNotes = updateDto.ResolutionNotes.Trim();
                    }

                    if (!string.IsNullOrWhiteSpace(updateDto.Purpose))
                    {
                        query.Purpose = updateDto.Purpose.Trim();
                    }

                    query.UpdatedBy = User?.Identity?.Name ?? "SystemUser";
                    query.UpdatedDate = DateTime.UtcNow;

                    _db.SaveChanges();
                    transaction.Commit();

                    return Ok(MapToResponseDto(query));
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return StandardBadRequest("UPDATE_FAILED", "Failed to update query transaction: " + ex.Message);
                }
            }
        }

        // =========================================================================
        // GET api/account-queries/{id}
        // =========================================================================
        [HttpGet]
        [Route("{id:guid}")]
        public IHttpActionResult GetQueryById(Guid id)
        {
            var query = _db.AccountQueryRequests.Find(id);
            if (query == null)
            {
                return StandardNotFound("QUERY_NOT_FOUND", $"Account query with ID '{id}' was not found.");
            }

            return Ok(MapToResponseDto(query));
        }

        private AccountQueryResponseDto MapToResponseDto(AccountQueryRequest entity)
        {
            if (entity == null) return null;

            return new AccountQueryResponseDto
            {
                Id = entity.Id,
                RequestReference = entity.RequestReference,
                AccountNumber = entity.AccountNumber,
                CustomerId = entity.CustomerId,
                QueryType = entity.QueryType,
                Status = entity.Status,
                Purpose = entity.Purpose,
                RequestedBy = entity.RequestedBy,
                RequestedDate = entity.RequestedDate,
                ResolutionNotes = entity.ResolutionNotes
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}