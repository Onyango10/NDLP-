using NDLP_Project.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace NDLP_Project.Controllers
{
    public class BaseApiController: ApiController
    {
        protected IHttpActionResult StandardNotFound(string errorCode, string message)
        {
            var error = new ApiErrorResponseDto(errorCode, message);
            return Content(HttpStatusCode.NotFound, error);
        }
        protected IHttpActionResult StandardBadRequest(string errorCode, string message, IEnumerable<string> details = null)
        {
            var error = new ApiErrorResponseDto(errorCode, message, details);
            return Content(HttpStatusCode.BadRequest, error);
        }
        protected IHttpActionResult StandardConflict(string errorCode, string message)
        {
            var error = new ApiErrorResponseDto(errorCode, message);
            return Content(HttpStatusCode.Conflict, error);
        }
        protected IHttpActionResult StandardValidationError(ModelStateDictionary modelState)
        {
            var errors = modelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? e.Exception?.Message : e.ErrorMessage)
                .Where(msg => !string.IsNullOrWhiteSpace(msg))
                .ToList();
            var error = new ApiErrorResponseDto("VALIDATION_FAILED", "One or more validation errors occurred.", errors);
            return Content(HttpStatusCode.BadRequest, error);
        }
    }
}