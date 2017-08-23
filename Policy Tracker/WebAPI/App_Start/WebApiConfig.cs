using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace WebAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Formatters.Remove(config.Formatters.XmlFormatter);
            config.MapHttpAttributeRoutes();

            // Unsecured Route
            config.Routes.MapHttpRoute(
                name: "Unsecured",
                routeTemplate: "api/security/{action}",
                defaults: new { controller = "Security", action = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "SecuredAction",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "SecuredNestedAction",
                routeTemplate: "policytracker/api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "SecuredGeneralAction",
                routeTemplate: "api/{controller}/action/{action}/{actionId}",
                defaults: new { action = RouteParameter.Optional, actionId = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "SecuredEntityAction",
                routeTemplate: "api/{controller}/{id}/{action}/{actionId}/{subaction}/{subactionId}",
                defaults: new
                {
                    id = RouteParameter.Optional,
                    action = RouteParameter.Optional,
                    actionId = RouteParameter.Optional,
                    subaction = RouteParameter.Optional,
                    subactionId = RouteParameter.Optional
                }
            );

            config.Routes.MapHttpRoute(
                name: "SendPostingNotice",
                routeTemplate: "api/{controller}/{action}/{RiskId}/{TypeOfPayLoad}",
                defaults: new { action = RouteParameter.Optional }
            );

            // This is necessary only when Web API will be deployed stand-alone
            //config.MessageHandlers.Add(new PerformanceHandler());

            // Allow JSON Serialization via Querystring Parameter
            config.Formatters.JsonFormatter.MediaTypeMappings.Add(new QueryStringMapping("json", "true", "application/json"));
            // Remove the XML Formatter so default is JSON
            //config.Formatters.Remove(config.Formatters.XmlFormatter);
        }
    }
}
