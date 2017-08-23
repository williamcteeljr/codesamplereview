using PolicyTracker.Platform.Logging;
using PolicyTracker.Platform.Caching;
using PolicyTracker.Platform.Security;
using PolicyTracker.Platform.UOW;
using System;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace PolicyTracker.WebAPI.Filters
{
    [AttributeUsage(AttributeTargets.Class)]
    public class UnitOfWorkAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext context)
        {
            string controllerName = context.ActionDescriptor.ControllerDescriptor.ControllerName;
            string actionName = context.ActionDescriptor.ActionName;
            UserSession session = SessionManager.GetCurrentSession();
            UnitOfWork uow = UnitOfWorkFactory.CreateUnitOfWork();
            //LogManager.Log(LogLevel.DEBUG, "Started Unit of Work in [WebAPI] for Action [{0}] on Controller [{1}]", actionName, controllerName);
            CacheManager.RequestCache.SetValue(UnitOfWorkFactory._UWBASE_CONTEXT, uow);
        }

        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            string controllerName = context.ActionContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            string actionName = context.ActionContext.ActionDescriptor.ActionName;
            UnitOfWork uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            uow.Finish();
            //LogManager.Log(LogLevel.DEBUG, "Finished Unit of Work in [WebAPI] for Action [{0}] on Controller [{1}]", actionName, controllerName);
            CacheManager.RequestCache.RemoveValue(uow.Context);
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class UseTransactionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext context)
        {
            string controllerName = context.ActionDescriptor.ControllerDescriptor.ControllerName;
            string actionName = context.ActionDescriptor.ActionName;
            UnitOfWork uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            uow.BeginTransaction();
            LogManager.Log(LogLevel.DEBUG, "Started Transaction in [WebAPI] for Action [{0}] on Controller [{1}]", actionName, controllerName);
        }

        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            string controllerName = context.ActionContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            string actionName = context.ActionContext.ActionDescriptor.ActionName;
            UnitOfWork uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            if (context.Exception == null)
            {
                uow.CommitTransaction();
                LogManager.Log(LogLevel.DEBUG, "Committed Transaction in [WebAPI] for Action [{0}] on Controller [{1}]", actionName, controllerName);
            }
            else
            {
                uow.RollBackTransaction();
                LogManager.Log(LogLevel.DEBUG, "Rolled Back Transaction in [WebAPI] for Action [{0}] on Controller [{1}]", actionName, controllerName);
            }
        }
    }
}