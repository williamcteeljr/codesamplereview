using Hangfire;
using PolicyTracker.BusinessServices;

namespace PolicyTracker.EventProcessing
{
    [Queue("policytracker")]
    public static class Events
    {
        public static void GenerateRenewals()
        {
            ServiceLocator.PolicySvc.GenerateRenewals();
        }
    }
}