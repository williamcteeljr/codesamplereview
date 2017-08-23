using PolicyTracker.WebAPI.Filters;
using PolicyTracker.BusinessServices;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.Utilities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;

namespace WebAPI.Controllers
{
    [AuthenticateSession]
    [HandleException]
    [UnitOfWork]
    public abstract class BaseController : ApiController
    {
        protected OrderFilter GetOrderFilter(string sortProperty, string sortOrder)
        {
            if (String.IsNullOrEmpty(sortProperty)) return null;
            if (sortOrder == null || sortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase))
            {
                return new OrderFilter(sortProperty, OrderFilter.Comparator.Ascending);
            }
            else
            {
                return new OrderFilter(sortProperty, OrderFilter.Comparator.Descending);
            }
        }

        protected PaginatedList<T> GetPaginatedList<T>(int pageSize, int pageNumber, string sortProperty, string sortOrder, PropertyFilter filter) where T : BaseEntity
        {
            var criteria = GetPaginationCriteria<T>(pageSize, pageNumber, sortProperty, sortOrder);
            if (filter != null) criteria.Filters.Add(filter);
            var result = ServiceLocator.EntityService.GetPaginatedList<T>(criteria);
            return result;
        }

        protected PaginatedList<T> GetPaginatedList<T>(int pageSize, int pageNumber, string sortProperty, string sortOrder, IEnumerable<PropertyFilter> filters = null) where T : BaseEntity
        {
            var criteria = GetPaginationCriteria<T>(pageSize, pageNumber, sortProperty, sortOrder);
            if (filters != null) criteria.Filters.AddRange(filters);
            var result = ServiceLocator.EntityService.GetPaginatedList<T>(criteria);
            return result;
        }

        protected PaginatedList<dynamic> GetPaginatedList<T>(string[] propertySet, int pageSize, int pageNumber, string sortProperty, string sortOrder, PropertyFilter filter = null) where T : BaseEntity
        {
            var criteria = GetPaginationCriteria<T>(pageSize, pageNumber, sortProperty, sortOrder);
            if (filter != null) criteria.Filters.Add(filter);
            var result = ServiceLocator.EntityService.GetPaginatedList<T>(propertySet, criteria);
            return result;
        }

        protected PaginationCriteria GetPaginationCriteria<T>(int pageSize, int pageNumber, string sortProperty, string sortOrder)
        {
            var orderFilter = GetOrderFilter(sortProperty, sortOrder);
            PaginationCriteria criteria = new PaginationCriteria(pageSize, pageNumber, orderFilter);
            criteria.Filters = WebAPIUtilities.ConvertRequestParamsToPropertyFilters(typeof(T), Request.GetQueryNameValuePairs());
            return criteria;
        }
    }
}
