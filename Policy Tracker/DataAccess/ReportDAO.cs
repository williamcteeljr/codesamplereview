using PolicyTracker.DomainModel.Framework;
using PolicyTracker.DomainModel.Reports;

namespace PolicyTracker.DataAccess.Reports
{
    public class NBFReportDAO : BaseDAO<NewBusinessFlow>
    {
        public NBFReportDAO()
            : base("NewBusinessFlowReport", "UW_Base_App", defaultOrderFilter: new OrderFilter("Branch", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("Branch", "Branch");
            AddColumnMapping("ProductLine", "ProductLine");
            AddColumnMapping("UW", "UW");
            AddColumnMapping("UnderwriterId", "UnderwriterId");
            AddColumnMapping("EffectiveDate", "EffectiveDate");
            AddColumnMapping("EffectiveMonth", "EffectiveMonth");
            AddColumnMapping("EffectiveYear", "EffectiveYear");
            AddColumnMapping("EffectiveDay", "EffectiveDay");
            AddColumnMapping("RiskStatus", "RiskStatus");
            AddColumnMapping("Total", "Total");
            AddColumnMapping("TotalPremium", "TotalPremium");
            Initialize();
        }
    }

    public class PredictivePremiumDAO : BaseDAO<PredictivePremiumDataPoint>
    {
        public PredictivePremiumDAO()
            : base("PredictivePremium", "UW_Base_App", defaultOrderFilter: new OrderFilter("Branch", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("Branch", "Branch");
            AddColumnMapping("ProductLine", "ProductLine");
            AddColumnMapping("Status", "Status");
            AddColumnMapping("UnderwriterId", "UnderwriterId");
            AddColumnMapping("UW", "UW");
            AddColumnMapping("IsRenewal", "IsRenewal");
            AddColumnMapping("EffectiveMonth", "EffectiveMonth");
            AddColumnMapping("EffectiveYear", "EffectiveYear");
            AddColumnMapping("ExpirationMonth", "ExpirationMonth");
            AddColumnMapping("ExpirationYear", "ExpirationYear");
            AddColumnMapping("Premium", "Premium");
            AddColumnMapping("RenewedPremium", "RenewedPremium");
            Initialize();
        }
    }
}
