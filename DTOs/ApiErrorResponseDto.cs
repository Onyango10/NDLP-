using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NDLP_Project.DTOs
{
    public class ApiErrorResponseDto
    {
        public string ErrorCode { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; }
        public DateTime Timestamp { get; set; }
        public ApiErrorResponseDto()
        {
            Errors = new List<string>();
            Timestamp = DateTime.UtcNow;
        }
        public ApiErrorResponseDto(string errorCode, string message, IEnumerable<string> errors = null)
        {
            ErrorCode = errorCode;
            Message = message;
            Timestamp = DateTime.UtcNow;
            Errors = errors != null ? new List<string>(errors) : new List<string>();
        }
    }
}