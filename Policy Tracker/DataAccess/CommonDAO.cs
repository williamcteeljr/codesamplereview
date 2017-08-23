using PolicyTracker.DomainModel.Common;
using PolicyTracker.DomainModel.Framework;

namespace PolicyTracker.DataAccess
{
    public class MarketDAO : BaseDAO<Market>
    {
        public MarketDAO()
            : base("QuoteStatusMarket", "UW_Base_App", defaultOrderFilter: new OrderFilter("CompanyName", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("Id", "MarketOptionId", true, true);
            AddColumnMapping("Status", "Status");
            AddColumnMapping("CompanyName", "Market");
            Initialize();
        }
    }

    public class ReasonDAO : BaseDAO<Reason>
    {
        public ReasonDAO()
            : base("Reasons", "UW_Base_App", defaultOrderFilter: new OrderFilter("Name", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("Id", "Id");
            AddColumnMapping("ActionType", "ActionType");
            AddColumnMapping("Code", "Code");
            AddColumnMapping("Name", "Name");
            Initialize();
        }
    }

    public class UserFilterDAO : BaseDAO<UserFilter>
    {
        public UserFilterDAO()
            : base("UserFilters", "UW_Base_App", defaultOrderFilter: new OrderFilter("FilterId", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("FilterId", "FilterId", true, true);
            AddColumnMapping("UserId", "UserId");
            AddColumnMapping("Name", "Name");
            AddColumnMapping("Expression", "Expression");
            AddColumnMapping("MonthFilter", "MonthFilter");
            AddColumnMapping("MonthRangeFilter", "MonthRangeFilter");
            Initialize();
        }
    }
}
