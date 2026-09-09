using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace NDLP_Project.Handlers
{
    public class StructuredLoggingHandler : DelegatingHandler
    {
        public const string CorrelationHeader = "X-Correlation-ID";

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // 1. Extract or generate Correlation ID
            string correlationId;
            if (request.Headers.Contains(CorrelationHeader))
            {
                correlationId = request.Headers.GetValues(CorrelationHeader).FirstOrDefault();
            }
            else
            {
                correlationId = Guid.NewGuid().ToString("N");
            }

            // Store correlation ID in request properties for controllers/services to access
            request.Properties[CorrelationHeader] = correlationId;

            // 2. Start Stopwatch to measure duration
            var stopwatch = Stopwatch.StartNew();

            // 3. Process the request through the inner handler / controller
            HttpResponseMessage response = null;
            Exception capturedException = null;

            try
            {
                response = await base.SendAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                capturedException = ex;
                throw;
            }
            finally
            {
                stopwatch.Stop();

                // 4. Mask sensitive info in the URL path (e.g. account numbers)
                string maskedUri = MaskSensitiveUrl(request.RequestUri.PathAndQuery);

                int statusCode = response != null ? (int)response.StatusCode : 500;

                // 5. Structured Log Output (Can be read by tools like Seq, Datadog, or CloudWatch)
                string logMessage = string.Format(
                    "[LOG] Timestamp: {0:O} | CorrelationID: {1} | Method: {2} | Path: {3} | StatusCode: {4} | DurationMs: {5}ms",
                    DateTime.UtcNow,
                    correlationId,
                    request.Method.Method,
                    maskedUri,
                    statusCode,
                    stopwatch.ElapsedMilliseconds
                );

                Trace.WriteLine(logMessage);

                // 6. Append Correlation ID to outgoing response headers so client has reference
                if (response != null && !response.Headers.Contains(CorrelationHeader))
                {
                    response.Headers.Add(CorrelationHeader, correlationId);
                    // [TICKET 17] API Security Hardening: Anti-sniffing & Anti-clickjacking headers
                    if (response != null)
                    {
                        if (!response.Headers.Contains("X-Content-Type-Options"))
                        {
                            response.Headers.Add("X-Content-Type-Options", "nosniff");
                        }

                        if (!response.Headers.Contains("X-Frame-Options"))
                        {
                            response.Headers.Add("X-Frame-Options", "DENY");
                        }
                    }
                }
            }

            return response;
        }

        // Helper: Masks 6+ digit numbers in the URL to protect account numbers
        private string MaskSensitiveUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return url;

            // Replaces digits in account paths: e.g. /api/accounts/01001234567 -> /api/accounts/0100****567
            return Regex.Replace(url, @"\b(\d{4})\d+(\d{3})\b", "$1****$2");
        }
    }
}