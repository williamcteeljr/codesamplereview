using Hangfire;
using DevExpress.XtraReports.Security;
using PolicyTracker.EventProcessing;
using PolicyTracker.BusinessServices;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using WebAPI;
using System.Web.Http;

namespace PolicyTracker
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            //System.Web.Http.GlobalConfiguration.Configure();
            System.Web.Http.GlobalConfiguration.Configure(WebApiConfig.Register);
            
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ControllerBuilder.Current.DefaultNamespaces.Add("PolicyTracker.Controllers");

            ApplicationManager.Initialize();
            ModelBinders.Binders.DefaultBinder = new DevExpress.Web.Mvc.DevExpressEditorsBinder();
            DataAnnotationsModelValidatorProvider.AddImplicitRequiredAttributeForValueTypes = false;

            //Hangfire.GlobalConfiguration.Configuration.UseSqlServerStorage("DB", HangFire.SqlOptions);
            //HangFire.Initialize();
        }
    }
}
