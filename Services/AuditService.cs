using System;
using NDLP_Project.Data;
using NDLP_Project.Models.Entities;

namespace NDLP_Project.Services
{
    public class AuditService
    {
        private readonly AccountDbContext _db;

        public AuditService(AccountDbContext db)
        {
            _db = db;
        }

        public void RecordAudit(string accountNumber, string action, string performedBy, string details, string correlationId = null)
        {
            try
            {
                var audit = new AccountAuditLog
                {
                    Id = Guid.NewGuid(),
                    AccountNumber = accountNumber ?? "N/A",
                    Action = action.Trim().ToUpper(),
                    PerformedBy = !string.IsNullOrWhiteSpace(performedBy) ? performedBy : "SystemUser",
                    PerformedDate = DateTime.UtcNow,
                    CorrelationId = correlationId,
                    Details = details
                };

                _db.AccountAuditLogs.Add(audit);
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                // In production banking, audit failures are logged to fallback syslog/event logs
                System.Diagnostics.Trace.WriteLine("AUDIT RECORDING FAILED: " + ex.Message);
            }
        }
    }
}