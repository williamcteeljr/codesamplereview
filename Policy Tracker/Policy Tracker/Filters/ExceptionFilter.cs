using PolicyTracker.Platform.Logging;
using PolicyTracker.Platform.Caching;
using PolicyTracker.Platform.UOW;
using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using PolicyTracker.Platform.Security;

namespace PolicyTracker.Filters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class IgnoreModelStateErrorsAttribute : ActionFilterAttribute
    {
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class HandleModelStateErrorsAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var ignoreModelState = context.ActionDescriptor.GetCustomAttributes(typeof(IgnoreModelStateErrorsAttribute), false).Any();
            // ModelState.Invalid will be set to true by MVC Framework during binding
            // ModelState.Invalid will also be set to true if ValidationRulesException is processed in Controller Action
            if (!context.Controller.ViewData.ModelState.IsValid && !ignoreModelState)
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class HandleUncheckedExceptionAttribute : FilterAttribute, IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context == null) return;
            string controllerName = (string)context.RouteData.Values["controller"];
            string actionName = (string)context.RouteData.Values["action"];
            var session = SessionManager.GetCurrentSession();
            var userName = session.User != null ? session.User.FirstName + " " + session.User.LastName : "Non User";
            Exception exception = context.Exception;

            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();

            if (uow != null)
            {
                LogManager.Log(LogLevel.WARN, "There was an open unit of work detected in [WebApplication] during execution of " + "Action [{0}] on Controller [{1}]", actionName, controllerName);
                uow.Finish();
                CacheManager.RequestCache.RemoveValue(uow.Context);
            }

            LogManager.Log(LogLevel.ERROR, "[User: " + userName + "] An exception of type [" + exception.GetType().Name + "] occurred in [WebApplication] during execution of Action ["
                + actionName + "] on Controller [" + controllerName + "]" + System.Environment.NewLine + exception.Message);

            context.ExceptionHandled = true;
            //context.HttpContext.Response.Clear();
            //context.HttpContext.Response.StatusCode = 500;
            context.Result = new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
        }
    }
}