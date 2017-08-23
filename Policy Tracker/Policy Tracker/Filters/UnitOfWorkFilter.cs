using PolicyTracker.Platform.Caching;
using PolicyTracker.Platform.UOW;
using System;
using System.Web;
using System.Web.Mvc;

namespace PolicyTracker.Filters
{
    [AttributeUsage(AttributeTargets.Class)]
    public class UnitOfWorkAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            string controllerName = (string)context.RouteData.Values["controller"];
            string actionName = (string)context.RouteData.Values["action"];
            //UserSession session = SessionManager.GetCurrentSession();

            UnitOfWork baseUOW = UnitOfWorkFactory.CreateUnitOfWork();
            // brokerUOW = UnitOfWorkFactory.CreateUnitOfWork(UnitOfWorkFactory._BROKER_CONTEXT);
            
            //LogManager.Log(LogLevel.DEBUG, "Started Unit of Work in [WebApplication] for Action [{0}] on Controller [{1}]", actionName, controllerName);
            CacheManager.RequestCache.SetValue(UnitOfWorkFactory._UWBASE_CONTEXT, baseUOW);
            //CacheManager.RequestCache.SetValue(UnitOfWorkFactory._BROKER_CONTEXT, brokerUOW);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            string controllerName = (string)context.RouteData.Values["controller"];
            string actionName = (string)context.RouteData.Values["action"];

            UnitOfWork uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            uow.Finish();
            CacheManager.RequestCache.RemoveValue(uow.Context);

            //uow = UnitOfWorkFactory.GetActiveUnitOfWork(UnitOfWorkFactory._BROKER_CONTEXT);
            //uow.Finish();
            //CacheManager.RequestCache.RemoveValue(uow.Context);

            //LogManager.Log(LogLevel.DEBUG, "Finished Unit of Work in [WebApplication] for Action [{0}] on Controller [{1}]", actionName, controllerName);
            
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class UseTransactionAttribute : ActionFilterAttribute
    {
        public UseTransactionAttribute()
            : base()
        {
            Order = 3;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            string controllerName = (string)context.RouteData.Values["controller"];
            string actionName = (string)context.RouteData.Values["action"];
            UnitOfWork uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            uow.BeginTransaction();
            //LogManager.Log(LogLevel.DEBUG, "Started Transaction in [WebApplication] for Action [{0}] on Controller [{1}]", actionName, controllerName);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            string controllerName = (string)context.RouteData.Values["controller"];
            string actionName = (string)context.RouteData.Values["action"];
            UnitOfWork uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            if (context.Exception == null)
            {
                uow.CommitTransaction();
                //LogManager.Log(LogLevel.DEBUG, "Committed Transaction in [WebApplication] for Action [{0}] on Controller [{1}]", actionName, controllerName);
            }
            else
            {
                uow.RollBackTransaction();
                //LogManager.Log(LogLevel.DEBUG, "Rolled Back Transaction in [WebApplication] for Action [{0}] on Controller [{1}]", actionName, controllerName);
            }
        }
    }
}