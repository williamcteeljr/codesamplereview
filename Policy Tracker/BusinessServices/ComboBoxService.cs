using PolicyTracker.DataAccess;
using PolicyTracker.DataAccess.AircraftDataAccess;
using PolicyTracker.DomainModel;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.Platform.UOW;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PolicyTracker.BusinessServices.ComboBox
{
    public class AirportComboSvc : BaseComboBoxService
    {
        public override object GetByFilter(DevExpress.Web.ListEditItemsRequestedByFilterConditionEventArgs args)
        {
            var uow = UnitOfWorkFactory.CreateUnitOfWork();
            PaginationCriteria criteria = new PaginationCriteria(args.BeginIndex, args.EndIndex, args.Filter, new OrderFilter("AirportID"));
            criteria.Filters.Add(new PropertyFilter("AirportID", PropertyFilter.Comparator.StartsWith, args.Filter));
            criteria.Filters.Add(new PropertyFilter("AirportName", PropertyFilter.Comparator.StartsWith, args.Filter, 1));
            var results = (!String.IsNullOrEmpty(args.Filter)) ? DAOFactory.GetDAO<AirportsDAO>().GetPaginatedList(uow, criteria) : new PaginatedList<Airport>();

            uow.Finish();
            return results.Results;
        }

        public override object GetByValue(DevExpress.Web.ListEditItemRequestedByValueEventArgs args)
        {
            var uow = UnitOfWorkFactory.CreateUnitOfWork();
            var airport = DAOFactory.GetDAO<AirportsDAO>().GetList(uow, new PropertyFilter("AirportID", args.Value)).FirstOrDefault();
            var result = (airport == null) ? null : new[] { airport };
            uow.Finish();
            return result;
        }
    }
}
