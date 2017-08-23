//using Hangfire;
//using Microsoft.Owin;
//using Owin;
//using PolicyTracker.EventProcessing;

//[assembly: OwinStartup(typeof(PolicyTracker.Startup))]

//namespace PolicyTracker
//{
//    public class Startup
//    {
//        public void Configuration(IAppBuilder app)
//        {
//            // Map Dashboard to the `http://<your-app>/hangfire` URL.
//            app.UseHangfireDashboard("/policytracker/hangfire", HangFire.DashboardOptions);
//            app.UseHangfireServer(HangFire.ServerOptions);
//        }
//    }
//}