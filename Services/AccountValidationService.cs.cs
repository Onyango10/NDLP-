using NDLP_Project.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace NDLP_Project.Services
{
    public class BusinessRuleResult
    {
        public bool IsSuccess { get; private set; }
        public string ErrorCode { get; private set; }
        public string ErrorMessage { get; private set; }
        public static BusinessRuleResult Success() => new BusinessRuleResult { IsSuccess = true };
        public static BusinessRuleResult Failure(string code, string message) =>
            new BusinessRuleResult { IsSuccess = false, ErrorCode = code, ErrorMessage = message };
    }
    public class AccountValidationService
    {
        // 1. [TICKET 8] Validate Account Number Format
        public BusinessRuleResult ValidateAccountNumber(string accountNumber)
        {
            if (string.IsNullOrWhiteSpace(accountNumber))
            {
                return BusinessRuleResult.Failure("INVALID_ACCOUNT_NUMBER", "Account number cannot be empty.");
            }
            // Must be between 5 and 20 alphanumeric characters
            if (accountNumber.Length < 5 || accountNumber.Length > 20)
            {
                return BusinessRuleResult.Failure("INVALID_ACCOUNT_LENGTH", "Account number must be between 5 and 20 characters.");
            }
            if (!Regex.IsMatch(accountNumber, @"^[a-zA-Z0-9\-]+$"))
            {
                return BusinessRuleResult.Failure("INVALID_ACCOUNT_FORMAT", "Account number can only contain letters, digits, and hyphens.");
            }
            return BusinessRuleResult.Success();
        }
        // 2. [TICKET 8 & 11] Validate Account Ownership
        public BusinessRuleResult ValidateOwnership(CustomerAccount account, string requestingCustomerId)
        {
            if (account == null)
            {
                return BusinessRuleResult.Failure("ACCOUNT_NOT_FOUND", "Account does not exist.");
            }
            if (string.IsNullOrWhiteSpace(requestingCustomerId) ||
                !string.Equals(account.CustomerId, requestingCustomerId.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return BusinessRuleResult.Failure("UNAUTHORIZED_ACCOUNT_ACCESS",
                    $"Customer '{requestingCustomerId}' is not the authorized owner of account '{account.AccountNumber}'.");
            }
            return BusinessRuleResult.Success();
        }
        // 3. [TICKET 8 & 11] Validate Account Status (Active, Frozen, Dormant)
        public BusinessRuleResult ValidateActiveStatus(CustomerAccount account, string operationName = "this operation")
        {
            if (account == null)
            {
                return BusinessRuleResult.Failure("ACCOUNT_NOT_FOUND", "Account does not exist.");
            }
            if (account.IsDeleted || string.Equals(account.Status, "DEACTIVATED", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(account.Status, "CLOSED", StringComparison.OrdinalIgnoreCase))
            {
                return BusinessRuleResult.Failure("ACCOUNT_CLOSED", $"Account '{account.AccountNumber}' is closed or deactivated.");
            }
            if (string.Equals(account.Status, "FROZEN", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(account.Status, "BLOCKED", StringComparison.OrdinalIgnoreCase))
            {
                return BusinessRuleResult.Failure("ACCOUNT_FROZEN",
                    $"Account '{account.AccountNumber}' is currently {account.Status}. Cannot proceed with {operationName}.");
            }
            return BusinessRuleResult.Success();
        }
        // 4. [TICKET 11] Validate Available Balance
        public BusinessRuleResult ValidateAvailableBalance(CustomerAccount account, decimal requiredAmount)
        {
            if (account == null)
            {
                return BusinessRuleResult.Failure("ACCOUNT_NOT_FOUND", "Account does not exist.");
            }
            if (account.AvailableBalance < requiredAmount)
            {
                return BusinessRuleResult.Failure("INSUFFICIENT_FUNDS",
                    $"Insufficient available balance. Required: {requiredAmount:N2} {account.Currency}, Available: {account.AvailableBalance:N2} {account.Currency}.");
            }
            return BusinessRuleResult.Success();
        }
        // 5. [TICKET 8 & 11] Validate Permitted Query Types for Account Type
        public BusinessRuleResult ValidatePermittedQueryType(string accountType, string queryType)
        {
            var permittedQueries = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "BALANCE_INQUIRY", "STATEMENT_QUERY", "ACCOUNT_DETAILS", "STATUS_CHECK"
            };
            if (string.IsNullOrWhiteSpace(queryType) || !permittedQueries.Contains(queryType.Trim()))
            {
                return BusinessRuleResult.Failure("INVALID_QUERY_TYPE",
                    $"Query type '{queryType}' is not permitted. Allowed: {string.Join(", ", permittedQueries)}.");
            }
            // Example Rule: Loan accounts do not support standard statement queries
            if (string.Equals(accountType, "LOAN", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(queryType, "STATEMENT_QUERY", StringComparison.OrdinalIgnoreCase))
            {
                return BusinessRuleResult.Failure("QUERY_NOT_PERMITTED_FOR_ACCOUNT_TYPE",
                    "Statement query is not permitted on loan accounts. Please use Loan Schedule inquiry.");
            }
            return BusinessRuleResult.Success();
        }
    }
}