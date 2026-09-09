using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;
using NDLP_Project.DTOs;

namespace NDLP_Project.Handlers
{
    public class GlobalExceptionHandler : ExceptionHandler
    {
        public override void Handle(ExceptionHandlerContext context)
        {
            // 1. Extract Correlation ID from request properties (set by StructuredLoggingHandler)
            string correlationId = "UNKNOWN";
            if (context.Request.Properties.ContainsKey(StructuredLoggingHandler.CorrelationHeader))
            {
                correlationId = context.Request.Properties[StructuredLoggingHandler.CorrelationHeader]?.ToString();
            }

            // 2. Securely log full exception details on the server (Trace / Logger)
            Trace.TraceError(
                "[GLOBAL ERROR] CorrelationID: {0} | Exception: {1} | Message: {2} | StackTrace: {3}",
                correlationId,
                context.Exception.GetType().FullName,
                context.Exception.Message,
                context.Exception.StackTrace
            );

            // 3. Build standardized error envelope (hiding sensitive internal details)
            var errorResponse = new ApiErrorResponseDto(
                "INTERNAL_SERVER_ERROR",
                "An unexpected error occurred while processing your request. Please contact support with Correlation ID: " + correlationId
            );

            // 4. Return HTTP 500 Internal Server Error with JSON body
            var response = context.Request.CreateResponse(HttpStatusCode.InternalServerError, errorResponse);

            // Ensure Correlation ID is on the response headers
            if (!response.Headers.Contains(StructuredLoggingHandler.CorrelationHeader))
            {
                response.Headers.Add(StructuredLoggingHandler.CorrelationHeader, correlationId);
            }

            context.Result = new ResponseMessageResult(response);
        }

        // Ensure this handler processes all exceptions across the entire pipeline
        public override bool ShouldHandle(ExceptionHandlerContext context)
        {
            return true;
        }
    }
}