using DevExpress.Web;
using DevExpress.Web.Mvc;
using PolicyTracker.DataAccess;
using PolicyTracker.DataAccess.AircraftDataAccess;
using PolicyTracker.DomainModel;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.Platform.Caching;
using PolicyTracker.Platform.UOW;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PolicyTracker.BusinessServices
{
    public class BlueBookSvc : BaseService
    {
        public override void GetDataRowCount(GridViewCustomBindingGetDataRowCountArgs e)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork(UnitOfWorkFactory._BLUEBOOK_CONTEXT);

            var criteria = GetPaginationCriteria<Airport>(e);
            var results = DAOFactory.GetDAO<AirportsDAO>().GetPaginatedList(uow, criteria);

            e.DataRowCount = results.TotalResults;

            CacheManager.RequestCache.SetValue("GridData", results.Results);
        }

        public List<Airport> GetAirports(IList<GridViewDataColumn> columns = null)
        {
            var uow = UnitOfWorkFactory.CreateUnitOfWork(UnitOfWorkFactory._BLUEBOOK_CONTEXT);
            var orderFilter = (columns != null) ? GetOrderFilterForExport(columns) : null;
            var filters = (columns != null) ? ServiceUtils.GetFiltersForExport(columns) : null;
            var results = DAOFactory.GetDAO<AirportsDAO>().GetList(uow, filters, orderFilter).ToList();
            uow.Finish();
            return results;
        }
    }

    public class AircraftMakeComboService : BaseComboBoxService
    {
        public override object GetByFilter(DevExpress.Web.ListEditItemsRequestedByFilterConditionEventArgs args)
        {
            var uow = UnitOfWorkFactory.CreateUnitOfWork(UnitOfWorkFactory._BLUEBOOK_CONTEXT);
            var aircraftList = (!String.IsNullOrEmpty(args.Filter)) ? DAOFactory.GetDAO<AircraftReferenceDAO>().GetList(uow, new PropertyFilter("MFR", PropertyFilter.Comparator.StartsWith, args.Filter)) : new List<AircraftReference>();
            var makeList = aircraftList.Select(x => x.MFR).ToList().Distinct();
            var endIndex = (makeList.Count() < args.EndIndex) ? makeList.Count() : args.EndIndex;
            makeList = makeList.ToList().GetRange(args.BeginIndex, endIndex);

            uow.Finish();
            return makeList;
        }

        public override object GetByValue(DevExpress.Web.ListEditItemRequestedByValueEventArgs args)
        {
            var uow = UnitOfWorkFactory.CreateUnitOfWork();
            var result = DAOFactory.GetDAO<AircraftReferenceDAO>().GetList(uow, new PropertyFilter("MFR", args.Value)).ToList();
            if (result.Count() == 0) result.Add(new AircraftReference());
            uow.Finish();
            return result.First().MFR;
        }
    }
}
