using Hangfire;
using Hangfire.SqlServer;
using PolicyTracker.Filters;
using System;

namespace PolicyTracker.EventProcessing
{
    public static class HangFire
    {
        public static SqlServerStorageOptions  SqlOptions = new SqlServerStorageOptions()
        {
            QueuePollInterval = TimeSpan.FromMinutes(1) // Default value
        };

        public static BackgroundJobServerOptions ServerOptions = new BackgroundJobServerOptions()
        {
            Queues = new[] { "policytracker" }
        };

        public static DashboardOptions DashboardOptions = new DashboardOptions()
        {
            AuthorizationFilters = new[] { new AuthenticateUserForHangFireAttribute() }
        };

        public static void Initialize()
        {
            //Create Recurring Jobs on App Start
            //RecurringJob.AddOrUpdate("GenerateRenewals", () => Events.GenerateRenewals(), Cron.Daily);
        }
    }
}