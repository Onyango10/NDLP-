# NDLP Account Queries API — Technical Specification & Documentation

Welcome to the technical documentation for the **NDLP Account Queries System**. This document provides full API contracts, security controls, parameter definitions, error codes, and architectural mapping for all 19 tickets.

---

## 1. System Architecture & Ticket Mapping

```
Client / Consumer (Mobile, Web, Integration Partners)
   │
   ▼
[Security & Observability Layer]
   ├── DelegatingHandler: Structured Logging, Duration, PII Masking (Ticket 12)
   ├── Security Headers: X-Correlation-ID, X-Frame-Options, X-Content-Type-Options (Ticket 17)
   └── Role-Based Authorization & IDOR Protection (Ticket 14)
   │
   ▼
[API Controller Layer]
   ├── CustomerAccountsController (Tickets 1, 2, 7, 9)
   └── AccountQueriesController (Tickets 3, 6, 16)
   │
   ▼
[Validation & Business Rules Layer]
   ├── BaseApiController: Standard Error Envelopes (Ticket 10)
   └── AccountValidationService: Ownership, Format, Balance, Status, Permissions (Tickets 8, 11)
   │
   ▼
[Data Access & Transaction Layer]
   ├── AccountDbContext & SQL Server Connection (Ticket 5)
   ├── Database Transactions: ACID Atomicity & Rollbacks (Ticket 15)
   └── AuditService: tbl_AccountAuditLogs Immutable Ledger (Ticket 13)
```

### Complete Ticket-to-Code Traceability Table

| Ticket # | Ticket Name | Implementation Location |
| :--- | :--- | :--- |
| **Ticket 1** | Basic GET Account API | `Controllers/CustomerAccountsController.cs` (`GetAccount`) |
| **Ticket 2** | Account List API | `Controllers/CustomerAccountsController.cs` (`GetAccounts`) |
| **Ticket 3** | Create Account Query API | `Controllers/AccountQueriesController.cs` (`CreateQuery`) |
| **Ticket 4** | Account & Query DTOs | `DTOs/` (`CustomerAccountDto`, `AccountQueryRequestDto`, `AccountTransactionDto`, etc.) |
| **Ticket 5** | MSSQL Data Access | `Data/AccountDbContext.cs`, `Models/Entities/`, `Web.config` |
| **Ticket 6** | Update Account Query API | `Controllers/AccountQueriesController.cs` (`UpdateQuery`) |
| **Ticket 7** | Account Status API | `Controllers/CustomerAccountsController.cs` (`UpdateAccountStatus`) |
| **Ticket 8** | Account Validation | `Services/AccountValidationService.cs` |
| **Ticket 9** | Statement Search & Pagination | `Controllers/CustomerAccountsController.cs` (`GetAccountStatement`), `DTOs/PagedResultDto.cs` |
| **Ticket 10** | Standard API Errors | `Controllers/BaseApiController.cs`, `DTOs/ApiErrorResponseDto.cs` |
| **Ticket 11** | Account Business Rules | `Services/AccountValidationService.cs` |
| **Ticket 12** | Structured Logging & Correlation ID | `Handlers/StructuredLoggingHandler.cs`, `App_Start/WebApiConfig.cs` |
| **Ticket 13** | Audit Trail | `Models/Entities/AccountAuditLog.cs`, `Services/AuditService.cs` |
| **Ticket 14** | Authentication & Authorization | `Controllers/CustomerAccountsController.cs` (`IsAuthorizedForAccount`) |
| **Ticket 15** | Database Transactions | `Controllers/AccountQueriesController.cs` (`BeginTransaction` / `Commit` / `Rollback`) |
| **Ticket 16** | Duplicate Request Protection | `Controllers/AccountQueriesController.cs` (Idempotency check, HTTP 409 Conflict) |
| **Ticket 17** | API Security Hardening | Strict DTOs, Security Headers in `Handlers/StructuredLoggingHandler.cs` |
| **Ticket 18** | Postman Test Suite | `Account_Queries_Postman_Collection.json` |
| **Ticket 19** | API Documentation | `API_DOCUMENTATION.md` |

---

## 2. Global Request & Response Headers

### Request Headers
| Header | Type | Description |
| :--- | :--- | :--- |
| `Content-Type` | `string` | Must be `application/json` for POST and PUT requests. |
| `X-Correlation-ID` | `string` | *(Optional)* Unique tracing UUID provided by the client. If omitted, the server automatically generates one. |

### Response Headers
| Header | Description |
| :--- | :--- |
| `X-Correlation-ID` | The tracing ID for this request; included in all responses for support tracking. |
| `X-Content-Type-Options` | `nosniff` (Prevents MIME-type confusion attacks). |
| `X-Frame-Options` | `DENY` (Prevents clickjacking). |

---

## 3. Endpoints Specification

### 3.1 Account Details
`GET /api/accounts/{accountIdentifier}`
- **Description:** Retrieves complete account information by Account Number or GUID ID.
- **Route Parameters:**
  - `accountIdentifier` *(string, required)*: The customer account number (e.g. `01001234567`) or system GUID.
- **Responses:**
  - `200 OK`: Returns `CustomerAccountDto`.
  - `400 Bad Request`: Account number missing or malformed.
  - `403 Forbidden`: User is not authorized to access this customer's account.
  - `404 Not Found`: Account does not exist or has been soft-deleted.

---

### 3.2 Account List & Filtering
`GET /api/accounts?type={type}&status={status}`
- **Description:** Retrieves a collection of customer accounts with optional filters.
- **Query Parameters:**
  - `type` *(string, optional)*: Filter by account type (`Savings`, `Current`, `Loan`).
  - `status` *(string, optional)*: Filter by account status (`Active`, `Dormant`, `Frozen`, `Blocked`).
- **Responses:**
  - `200 OK`: Returns `List<CustomerAccountDto>`. (Returns `[]` if no accounts match).

---

### 3.3 Account Statement & Paginated Search
`GET /api/accounts/{accountNumber}/statement`
- **Description:** Retrieves a paginated list of transaction history ordered newest first.
- **Query Parameters:**
  - `pageNumber` *(int, default: 1)*: Target page number.
  - `pageSize` *(int, default: 20, max: 100)*: Number of transactions per page.
  - `startDate` *(date, optional)*: Filter transactions >= start date (`YYYY-MM-DD`).
  - `endDate` *(date, optional)*: Filter transactions <= end date (`YYYY-MM-DD`).
  - `minAmount` *(decimal, optional)*: Filter transactions >= amount.
  - `maxAmount` *(decimal, optional)*: Filter transactions <= amount.
  - `transactionType` *(string, optional)*: `"DEBIT"` or `"CREDIT"`.
- **Response `200 OK`:**
  ```json
  {
    "pageNumber": 1,
    "pageSize": 20,
    "totalRecords": 85,
    "totalPages": 5,
    "items": [
      {
        "transactionReference": "TXN-20260907-001",
        "transactionDate": "2026-09-07T14:30:00Z",
        "valueDate": "2026-09-07T14:30:00Z",
        "transactionType": "CREDIT",
        "amount": 25000.00,
        "currency": "KES",
        "runningBalance": 145000.00,
        "description": "Salary Deposit",
        "channel": "WEB",
        "status": "POSTED"
      }
    ]
  }
  ```

---

### 3.4 Submit Account Query Request
`POST /api/account-queries`
- **Description:** Submits a customer inquiry/service request with duplicate protection.
- **Request Body (`AccountQueryRequestDto`):**
  ```json
  {
    "accountNumber": "01001234567",
    "customerId": "CUST-1001",
    "queryType": "BALANCE_INQUIRY",
    "purpose": "Customer requested official balance statement for visa application.",
    "requestReference": "CLIENT-REF-99882"
  }
  ```
- **Responses:**
  - `201 Created`: Returns `AccountQueryResponseDto`.
  - `400 Bad Request`: Validation failure or account does not match customer ID.
  - `409 Conflict`: Duplicate request detected (Request reference already exists).

---

### 3.5 Update Account Query Request
`PUT /api/account-queries/{id}`
- **Description:** Updates permitted fields (`status`, `resolutionNotes`, `purpose`) on an open query.
- **Responses:**
  - `200 OK`: Returns updated `AccountQueryResponseDto`.
  - `400 Bad Request`: Request is already `COMPLETED` or `REJECTED` (immutable).
  - `404 Not Found`: Query ID not found.

---

### 3.6 Update Account Status
`PUT /api/accounts/{accountNumber}/status`
- **Description:** Flags, freezes, activates, or deactivates an eligible account.
- **Request Body (`AccountStatusUpdateDto`):**
  ```json
  {
    "status": "FROZEN",
    "reason": "Temporary freeze requested by AML fraud compliance."
  }
  ```
- **Eligibility Rules:**
  - Cannot alter a permanently deactivated/closed account.
  - Cannot deactivate an account with negative balance (`BookBalance < 0`) or active funds holds (`HoldAmount > 0`).

---

## 4. Standardized Error Response Envelope

All `4xx` and `5xx` error responses adhere to the unified contract:

```json
{
  "errorCode": "ACCOUNT_FROZEN",
  "message": "Account '01001234567' is currently FROZEN. Cannot proceed with query submission.",
  "errors": [],
  "timestamp": "2026-09-08T08:55:00Z"
}
```

### Complete Error Code Directory

| Error Code | HTTP Status | Trigger Condition |
| :--- | :--- | :--- |
| `VALIDATION_FAILED` | 400 | Data annotation or model binding rules failed (e.g. missing required field). |
| `INVALID_ACCOUNT_NUMBER` | 400 | Account number is null, empty, or whitespace. |
| `INVALID_ACCOUNT_LENGTH` | 400 | Account number is shorter than 5 or longer than 20 characters. |
| `INVALID_ACCOUNT_FORMAT` | 400 | Account number contains illegal characters (only alphanumeric and hyphens allowed). |
| `UNAUTHORIZED_ACCOUNT_ACCESS` | 400 / 403 | Customer ID in request does not match the account's registered owner. |
| `FORBIDDEN` | 403 | Authenticated user is not authorized to access this account. |
| `ACCOUNT_NOT_FOUND` | 404 | Account does not exist in the ledger or is soft-deleted (`IsDeleted = 1`). |
| `ACCOUNT_CLOSED` | 400 | Account is marked `CLOSED` or `DEACTIVATED`. |
| `ACCOUNT_FROZEN` | 400 | Account is marked `FROZEN` or `BLOCKED` by compliance. |
| `INSUFFICIENT_FUNDS` | 400 | Available balance is less than the requested transaction or fee amount. |
| `INVALID_QUERY_TYPE` | 400 | Query type is not in permitted list (`BALANCE_INQUIRY`, `STATEMENT_QUERY`, `ACCOUNT_DETAILS`, `STATUS_CHECK`). |
| `QUERY_NOT_PERMITTED_FOR_ACCOUNT_TYPE` | 400 | The specific query type is forbidden for this account classification (e.g. statement on loan accounts). |
| `DUPLICATE_REQUEST` | 409 | `RequestReference` was already submitted previously (Idempotency violation). |
| `IMMUTABLE_QUERY` | 400 | Attempting to update a query request that has already been `COMPLETED` or `REJECTED`. |
| `TRANSACTION_FAILED` | 400 / 500 | Database transaction failed and was rolled back. |
