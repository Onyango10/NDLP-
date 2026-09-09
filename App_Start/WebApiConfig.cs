using Microsoft.Owin.Security.OAuth;
using NDLP_Project.Handlers;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;

namespace NDLP_Project
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            // Configure Web API to use only bearer token authentication.
            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

            // Web API routes
            config.MapHttpAttributeRoutes();
            // [TICKET 12] Register our Structured Logging & Correlation ID HandlerB
            config.MessageHandlers.Add(new StructuredLoggingHandler());
            // Centralized Global Exception Handling
            config.Services.Replace(typeof(IExceptionHandler), new GlobalExceptionHandler());



            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
