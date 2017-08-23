using PolicyTracker.BusinessServices;
using PolicyTracker.Platform.Logging;
using PolicyTracker.Platform.Security;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Mvc;
//using Microsoft.Owin;

namespace PolicyTracker.Filters
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AuthenticateBaseSessionAttribute : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            HttpCookie cookie = filterContext.HttpContext.Request.Cookies[SessionManager.SESSIONID_TOKEN];
            bool authorized = (cookie != null);
            if (!authorized)
                LogManager.Log(LogLevel.DEBUG, String.Format("Session Cookie {0} not sent with request", SessionManager.SESSIONID_TOKEN));

            if (authorized)
            {
                string sessionId = cookie.Value;
                authorized = SessionManager.IsValidSession(sessionId);

                if (authorized)
                {
                    UserSession session = SessionManager.GetSession(sessionId);
                    SessionManager.SetCurrentSession(session);
                    string controllerName = (string)filterContext.RouteData.Values["controller"];
                    string actionName = (string)filterContext.RouteData.Values["action"];
                    LogManager.Log(LogLevel.DEBUG, String.Format("Successfully Authroized Request from Session {0} for controller {1} action {2}", session.SessionId, controllerName, actionName));
                }
                else
                {
                    LogManager.Log(LogLevel.DEBUG, String.Format("SessionId {0} not found in list of valid SessionIds. Cookie Expired at {1}", sessionId, cookie.Expires));
                }
            }

            if (!authorized)
            {
                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
                LogManager.Log(LogLevel.DEBUG, "Request Denied. Session was invalid");
            }
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ResourceAuthorizationAttribute : System.Web.Http.Filters.AuthorizationFilterAttribute
    {
        public string Resource { get; set; }
        public PolicyTracker.BusinessServices.SecurityManager.ResourcePrivilege Privilege { get; set; }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var authorized = SecurityManager.HasAccess(Resource, Privilege);

            if (!authorized)
            {
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.Forbidden);
            }
        }
    }

    public class AuthenticateUserForHangFireAttribute : FilterAttribute, Hangfire.Dashboard.IAuthorizationFilter
    {
        public bool Authorize(IDictionary<string, object> owinEnvironment)
        {
            // In case you need an OWIN context, use the next line,
            // `OwinContext` class is the part of the `Microsoft.Owin` package.
            var context = new Microsoft.Owin.OwinContext(owinEnvironment);
            var sessionId = context.Request.Cookies[SessionManager.SESSIONID_TOKEN];

            bool authorized = (!String.IsNullOrEmpty(sessionId));

            if (authorized)
            {
                authorized = SessionManager.IsValidSession(sessionId);

                if (authorized)
                {
                    UserSession session = SessionManager.GetSession(sessionId);
                    SessionManager.SetCurrentSession(session);
                    authorized = (session.User.IsSuperAdmin);
                }
            }

            // Allow all authenticated users to see the Dashboard (potentially dangerous).
            return authorized;
        }
    }
}