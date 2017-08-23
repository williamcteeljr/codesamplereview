using DevExpress.Web;
using DevExpress.Web.Mvc;
using PolicyTracker.DataAccess;
using PolicyTracker.DataAccess.Policy;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.DomainModel.Policy;
using PolicyTracker.Platform.Caching;
using PolicyTracker.Platform.Security;
using PolicyTracker.Platform.UOW;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PolicyTracker.BusinessServices
{
    public class WorkingListGridSvc : BaseService
    {
        public override void GetDataRowCount(GridViewCustomBindingGetDataRowCountArgs e)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork(UnitOfWorkFactory._UWBASE_CONTEXT);
            var userId = SessionManager.GetCurrentSession().User.UserId;

            var criteria = GetPaginationCriteria<WorkingListItem>(e);
            //criteria.Filters = GetFiltersForNestedObject(criteria.Filters);
            var results = DAOFactory.GetDAO<WorkingListDAO>().GetPaginatedList(uow, criteria);
            
            e.DataRowCount = results.TotalResults;
            CacheManager.RequestCache.SetValue(userId + "GridData", results.Results);
            SessionManager.SetSessionValue("LastWorkingListModel", e, true);
        }

        public IEnumerable<WorkingListItem> GetWorkingListExportData(GridViewCustomBindingGetDataRowCountArgs dxModel)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var criteria = GetPaginationCriteria<WorkingListItem>(dxModel);
            var results = DAOFactory.GetDAO<WorkingListDAO>().GetList(uow, criteria.Filters, criteria.OrderFilters);
            return results;
        }

        public List<WorkingListItem> GetWorkingList(IList<GridViewDataColumn> columns = null)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork(UnitOfWorkFactory._BLUEBOOK_CONTEXT);
            var orderFilter = (columns != null) ? GetOrderFilterForExport(columns) : null;
            var filters = (columns != null) ? ServiceUtils.GetFiltersForExport(columns) : null;
            var results = DAOFactory.GetDAO<WorkingListDAO>().GetList(uow, filters, orderFilter).ToList();
            return results;
        }

        public List<PropertyFilter> GetFiltersForNestedObject(List<PropertyFilter> filters, string objName = null)
        {
            List<PropertyFilter> objFilters = new List<PropertyFilter>();

            if (objName == null || String.IsNullOrEmpty(objName))
            {
                objFilters = filters.Where(f => f.PropertyName.Contains('.')).ToList();
            }
            else
            {
                objFilters = filters.Where(f => f.PropertyName.Contains(objName)).ToList();
                var objNameInFilter = objName + ".";
                foreach (var filter in objFilters)
                {
                    filter.PropertyName = filter.PropertyName.Replace(objNameInFilter, "");
                }
            }
            return objFilters;
        }

        public WorkingListGridConfig SaveConfig(WorkingListGridConfig config)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();

            if (config.ConfigId == 0)
                DAOFactory.GetDAO<GridConfigDAO>().Create(uow, config);
            else
                DAOFactory.GetDAO<GridConfigDAO>().Update(uow, config);
            return config;
        }

        public WorkingListGridConfig GetUserGridConfiguration(string name)
        {
            var uow = UnitOfWorkFactory.CreateUnitOfWork(UnitOfWorkFactory._UWBASE_CONTEXT);
            var filters = new List<PropertyFilter>();
            filters.AddRange(new[] { new PropertyFilter("Name", name), new PropertyFilter("UserId", SessionManager.GetCurrentSession().User.UserId) });
            var result = DAOFactory.GetDAO<GridConfigDAO>().GetInstance(uow, filters);
            return result;
        }
    }
}
