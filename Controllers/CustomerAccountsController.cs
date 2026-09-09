using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using NDLP_Project.Data;
using NDLP_Project.DTOs;
using NDLP_Project.Models.Entities;

namespace NDLP_Project.Controllers
{
    [RoutePrefix("api/accounts")]
    public class CustomerAccountsController : ApiController
    {
        private readonly AccountDbContext _db = new AccountDbContext();

        // =========================================================================
        // TICKET 2: GET api/accounts?type=Savings&status=Active
        // =========================================================================
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAccounts([FromUri] string type = null, [FromUri] string status = null)
        {
            // 1. Start with non-deleted accounts
            IQueryable<CustomerAccount> query = _db.CustomerAccounts.Where(a => !a.IsDeleted);

            // 2. Apply optional AccountType filter (e.g. "Savings", "Current", "Loan")
            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(a => a.AccountType.ToLower() == type.Trim().ToLower());
            }

            // 3. Apply optional Status filter (e.g. "Active", "Dormant", "Frozen")
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(a => a.Status.ToLower() == status.Trim().ToLower());
            }

            // 4. Execute query against SQL Server
            var accounts = query.ToList();

            // 5. Map each entity to DTO using our helper method
            var responseList = accounts.Select(MapToDto).ToList();

            return Ok(responseList);
        }

        // =========================================================================
        // TICKET 1: GET api/accounts/{accountIdentifier}
        // =========================================================================
        [HttpGet]
        [Route("{accountIdentifier}")]
        public IHttpActionResult GetAccount(string accountIdentifier)
        {
            if (string.IsNullOrWhiteSpace(accountIdentifier))
            {
                return BadRequest("Account number or identifier must be provided.");
            }

            CustomerAccount account = null;

            if (Guid.TryParse(accountIdentifier, out Guid accountGuid))
            {
                account = _db.CustomerAccounts
                    .FirstOrDefault(a => a.Id == accountGuid && !a.IsDeleted);
            }
            else
            {
                account = _db.CustomerAccounts
                    .FirstOrDefault(a => a.AccountNumber == accountIdentifier && !a.IsDeleted);
            }

            if (account == null)
            {
                return NotFound();
            }

            // [TICKET 14] Verify authorization
            if (!IsAuthorizedForAccount(account))
            {
                return Content(System.Net.HttpStatusCode.Forbidden,
                    new ApiErrorResponseDto("FORBIDDEN", "You are not authorized to view this account."));
            }

            return Ok(MapToDto(account));
        }
        // =========================================================================
        // TICKET 9: GET api/accounts/{accountNumber}/statement
        // Paginated statement with date, amount, and transaction type filters
        // =========================================================================
        [HttpGet]
        [Route("{accountNumber}/statement")]
        public IHttpActionResult GetAccountStatement(
            string accountNumber,
            [FromUri] DateTime? startDate = null,
            [FromUri] DateTime? endDate = null,
            [FromUri] decimal? minAmount = null,
            [FromUri] decimal? maxAmount = null,
            [FromUri] string transactionType = null,
            [FromUri] int pageNumber = 1,
            [FromUri] int pageSize = 20)
        {
            // 1. Validate that the account exists
            var accountExists = _db.CustomerAccounts
                .Any(a => a.AccountNumber == accountNumber && !a.IsDeleted);

            if (!accountExists)
            {
                return NotFound();
            }

            // 2. Guardrails on pagination parameters
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100; // Safety cap

            // 3. Start IQueryable for this account's transactions
            IQueryable<AccountTransaction> query = _db.AccountTransactions
                .Where(t => t.AccountNumber == accountNumber);

            // 4. Apply optional filters
            if (startDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                // Include the full end-of-day for the end date
                var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(t => t.TransactionDate <= endOfDay);
            }

            if (minAmount.HasValue)
            {
                query = query.Where(t => t.Amount >= minAmount.Value);
            }

            if (maxAmount.HasValue)
            {
                query = query.Where(t => t.Amount <= maxAmount.Value);
            }

            if (!string.IsNullOrWhiteSpace(transactionType))
            {
                query = query.Where(t => t.TransactionType.ToLower() == transactionType.Trim().ToLower());
            }

            // 5. Count total records matching filters (translates to SELECT COUNT(*) in SQL)
            int totalRecords = query.Count();

            // 6. Apply Ordering and Pagination (translates to OFFSET ... FETCH NEXT in SQL)
            var transactions = query
                .OrderByDescending(t => t.TransactionDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // 7. Map to DTOs
            var transactionDtos = transactions.Select(MapToTransactionDto).ToList();

            // 8. Wrap into the paginated envelope
            var response = new PagedResultDto<AccountTransactionDto>(
                transactionDtos,
                totalRecords,
                pageNumber,
                pageSize);

            return Ok(response);
        }

        // Helper: Map AccountTransaction Entity -> AccountTransactionDto
        private AccountTransactionDto MapToTransactionDto(AccountTransaction t)
        {
            if (t == null) return null;

            return new AccountTransactionDto
            {
                TransactionReference = t.TransactionReference,
                TransactionDate = t.TransactionDate,
                ValueDate = t.ValueDate,
                TransactionType = t.TransactionType,
                Amount = t.Amount,
                Currency = t.Currency,
                RunningBalance = t.RunningBalance,
                Description = t.Description,
                Channel = t.Channel,
                Status = t.Status
            };
        }
        // =========================================================================
        // TICKET 7: PUT api/accounts/{accountNumber}/status
        // Deactivates or flags an account according to banking business rules
        // =========================================================================
        [HttpPut]
        [Route("{accountNumber}/status")]
        public IHttpActionResult UpdateAccountStatus(string accountNumber, [FromBody] AccountStatusUpdateDto statusDto)
        {
            // 1. Validate payload
            if (statusDto == null)
            {
                return BadRequest("Request body cannot be null.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 2. Validate permitted statuses
            var validStatuses = new[] { "ACTIVE", "DORMANT", "FROZEN", "BLOCKED", "DEACTIVATED" };
            string targetStatus = statusDto.Status.Trim().ToUpper();

            if (!validStatuses.Contains(targetStatus))
            {
                return BadRequest($"Invalid status '{statusDto.Status}'. Permitted values: {string.Join(", ", validStatuses)}.");
            }

            // 3. Locate the account
            var account = _db.CustomerAccounts
                .FirstOrDefault(a => a.AccountNumber == accountNumber && !a.IsDeleted);

            if (account == null)
            {
                return NotFound();
            }

            // 4. Business Rule: Cannot alter a permanently deactivated/closed account
            if (string.Equals(account.Status, "DEACTIVATED", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(account.Status, "CLOSED", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Cannot change the status of an account that has already been deactivated or closed.");
            }

            // 5. Business Rule: Cannot deactivate if account is overdrawn or has active holds
            if (targetStatus == "DEACTIVATED")
            {
                if (account.BookBalance < 0)
                {
                    return BadRequest($"Cannot deactivate account. Account has an outstanding overdrawn balance of {account.BookBalance} {account.Currency}.");
                }

                if (account.HoldAmount > 0)
                {
                    return BadRequest($"Cannot deactivate account. Account has active funds holds totaling {account.HoldAmount} {account.Currency}.");
                }

                account.IsActive = false; // Mark inactive
            }
            else if (targetStatus == "ACTIVE")
            {
                account.IsActive = true;
            }

            // 6. Update status and audit trail
            account.Status = targetStatus;
            account.ModifiedBy = User?.Identity?.Name ?? "SystemUser";
            account.ModifiedDate = DateTime.UtcNow;

            // 7. Save changes
            _db.SaveChanges();

            // 8. Return 200 OK with updated account details
            return Ok(MapToDto(account));
        }
        // Helper: [TICKET 14] Ensures users can only access their own accounts unless they are Teller/Admin
        private bool IsAuthorizedForAccount(CustomerAccount account)
        {
            if (account == null) return false;

            // 1. If caller is staff / admin, access is permitted
            if (User.IsInRole("Admin") || User.IsInRole("Teller") || User.IsInRole("Manager"))
            {
                return true;
            }

            // 2. If caller is anonymous/system user during local dev testing, permit
            if (string.IsNullOrEmpty(User?.Identity?.Name) || User.Identity.Name == "SystemUser")
            {
                return true;
            }

            // 3. For customer users: username must match the account's CustomerId
            return string.Equals(account.CustomerId, User.Identity.Name, StringComparison.OrdinalIgnoreCase);
        }
        // =========================================================================
        // REUSABLE HELPER: Map Entity -> DTO (DRY Principle)
        // =========================================================================
        private CustomerAccountDto MapToDto(CustomerAccount account)
        {
            if (account == null) return null;

            return new CustomerAccountDto
            {
                AccountId = account.Id.ToString(),
                AccountNumber = account.AccountNumber,
                CustomerId = account.CustomerId,
                CustomerName = account.CustomerName,
                AccountType = account.AccountType,
                Currency = account.Currency,
                BookBalance = account.BookBalance,
                AvailableBalance = account.AvailableBalance,
                HoldAmount = account.HoldAmount,
                OverdraftLimit = account.OverdraftLimit,
                Status = account.Status,
                BranchCode = account.BranchCode,
                BranchName = account.BranchName,
                Email = account.Email,
                PhoneNumber = account.PhoneNumber,
                NationalIdNumber = account.NationalIdNumber,
                DateOpened = account.CreatedDate
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