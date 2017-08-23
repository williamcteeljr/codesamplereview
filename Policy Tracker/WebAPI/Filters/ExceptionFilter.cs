using PolicyTracker.Platform.Logging;
using PolicyTracker.BusinessServices;
using PolicyTracker.Platform.Caching;
using PolicyTracker.Platform.UOW;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;

namespace PolicyTracker.WebAPI.Filters
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HandleExceptionAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            if (context == null) return;
            string controllerName = context.ActionContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            string actionName = context.ActionContext.ActionDescriptor.ActionName;
            Exception exception = context.Exception;

            // If there is an existing unit of work, close it.
            UnitOfWork uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            if (uow != null)
            {
                LogManager.Log(LogLevel.WARN, "There was an open unit of work detected in [WebAPI] during execution of "
                    + "Action [{0}] on Controller [{1}]", actionName, controllerName);
                uow.Finish();
                CacheManager.RequestCache.RemoveValue(uow.Context);
            }

            // Specific Check for Validation Exception
            if (exception is ValidationRulesException)
            {
                LogManager.Log(LogLevel.DEBUG, "A Validation Rule Exception occurred in [WebAPI] during execution of "
                    + "Action [{0}] on Controller [{1}]", actionName, controllerName);
                OnValidationRulesException(context, exception as ValidationRulesException);
            }
            // Unchecked Exception
            else
            {
                LogManager.Log(LogLevel.ERROR, "An exception of type [" + exception.GetType().Name + "] occurred in [WebAPI] during execution of Action ["
                    + actionName + "] on Controller [" + controllerName + "]" + System.Environment.NewLine + exception.Message);

                //actionExecutedContext.ExceptionHandled = true;
                context.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        private void OnValidationRulesException(HttpActionExecutedContext context, ValidationRulesException ex)
        {
            // Serialize Validation Errors into List of Key/Value Pairs
            var errors = new Dictionary<string, IEnumerable<string>>();
            foreach (ValidationResult vr in ex.Errors)
            {
                foreach (string key in vr.MemberNames)
                {
                    errors[key] = ex.Errors.Select(e => e.ErrorMessage);
                }
            }

            // Return 400 with Errors
            context.Response = context.Request.CreateResponse(HttpStatusCode.BadRequest, errors);
        }
    }
}