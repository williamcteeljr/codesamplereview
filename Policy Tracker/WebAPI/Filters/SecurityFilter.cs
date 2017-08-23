using PolicyTracker.Platform.Logging;
using PolicyTracker.Platform.Security;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace PolicyTracker.WebAPI.Filters
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AuthenticateSessionAttribute : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            CookieHeaderValue cookie = actionContext.Request.Headers.GetCookies(SessionManager.SESSIONID_TOKEN).FirstOrDefault();
            bool authorized = (cookie != null);
            if (!authorized) LogManager.Log(LogLevel.DEBUG, "{0} - Cookie {1} was not found in request", this.GetType().Name, SessionManager.SESSIONID_TOKEN);
            if (authorized)
            {
                string sessionId = cookie[SessionManager.SESSIONID_TOKEN].Value;
                authorized = SessionManager.IsValidSession(sessionId);
                if (!authorized) LogManager.Log(LogLevel.DEBUG, "{0} - Session {1} was not found", this.GetType().Name, sessionId);
                if (authorized)
                {
                    UserSession session = SessionManager.GetSession(sessionId);
                    SessionManager.SetCurrentSession(session);
                }

                // Check for Missing Resource Authorization
                //var resourceAuthorizationAttributes = actionContext.ActionDescriptor.GetCustomAttributes<ResourceAuthorizationAttribute>();
                //if (resourceAuthorizationAttributes.Count == 0) LogManager.Log(LogLevel.WARN, "No Resource Authorization found in [WebApplication]"
                //    + " for Action [{0}] on Controller [{1}]", actionContext.ActionDescriptor.ActionName, actionContext.ActionDescriptor.ControllerDescriptor.ControllerName);
            }

            if (!authorized)
            {
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
        }
    }

    //[AttributeUsage(AttributeTargets.Method)]
    //public class ResourceAuthorizationAttribute : AuthorizationFilterAttribute
    //{
    //    public string Resource { get; set; }
    //    public ResourcePrivilege Privilege { get; set; }

    //    public override void OnAuthorization(HttpActionContext actionContext)
    //    {
    //        var authorized = SecurityManager.HasAccess(Resource, Privilege);
    //        if (!authorized)
    //        {
    //            actionContext.Response = new HttpResponseMessage(HttpStatusCode.Forbidden);
    //        }
    //    }
    //}
}