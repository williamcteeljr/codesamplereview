using PolicyTracker.DomainModel.Framework;
using PolicyTracker.DomainModel.Other;
using PolicyTracker.DomainModel.Reports;

namespace PolicyTracker.DataAccess.Other
{
    public class DataImportDAO : BaseDAO<DataImport>
    {
        public DataImportDAO()
            : base("QuotePRTDataImport", "UW_Base_App", defaultOrderFilter: new OrderFilter("ID", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("ID", "ID", true, true);
            AddColumnMapping("Company", "Company");
            AddColumnMapping("Prefix", "Prefix");
            AddColumnMapping("PolicyNumber", "PolicyNumber");
            AddColumnMapping("Suffix", "Suffix");
            AddColumnMapping("DateInserted", "DateInserted");
            AddColumnMapping("WrittenPremium", "WrittenPremium");
            AddColumnMapping("AnnualPremium", "AnnualPremium");
            AddColumnMapping("IsImported", "IsImported");
            AddColumnMapping("CreatedOn", "CreatedOn");
            Initialize();
        }
    }
}
