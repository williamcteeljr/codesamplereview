using DevExpress.Web.Mvc;
using DevExpress.XtraPrinting;
using PolicyTracker.Filters;
using PolicyTracker.Platform.Utilities;
using PolicyTracker.BusinessServices;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace PolicyTracker.Controllers
{
    [AuthenticateBaseSession]
    [HandleUncheckedException]
    [HandleModelStateErrors(Order = 1)]
    [UnitOfWork(Order = 2)]
    [NoCache]
    public abstract class BaseController : Controller 
    {
        protected void ProcessValidationException(ValidationRulesException ex)
        {
            if (ex == null) return;

            foreach (ValidationResult vr in ex.Errors)
            {
                foreach (string key in vr.MemberNames)
                {
                    ModelState.AddModelError(key, vr.ErrorMessage);
                }
            }
        }

        protected void ProcessBusinessRulesException()
        {
            var warnings = ValidationHelper.GetRequestWarnings();

            var result = new List<string>();

            if (warnings != null)
            {
                foreach (ValidationResult vr in warnings)
                {
                    result.Add(vr.ErrorMessage);
                }

                ViewBag.Warnings = result;
            }
        }

        //protected T BindRequestToEntity<T>(T entity) where T : BaseEntity
        //{
        //    return BindRequestToEntity<T>(entity, PropertyFilter.RecordId(entity.RecordId));
        //}

        protected T BindRequestToEntity<T>(T entity, object value) where T : BaseEntity
        {
            return BindRequestToEntity(entity, new PropertyFilter("Property", value));
        }

        protected T BindRequestToEntity<T>(T entity, PropertyFilter uniqueFilter, bool isNestedObject = false) where T : BaseEntity
        {
            // If we are updating an existing entity, fetch and replace values from request
            var prop = PolicyTracker.DataAccess.ORCatalog.GetMetadataForEntity(entity.GetType().Name).Where(y => y.IsKeyField).FirstOrDefault();
            PropertyInfo property = ObjectUtils.GetNestedProperty(entity, prop.PropertyName);
            Type propType = property.PropertyType;

            if (Convert.ToString(property.GetValue(entity, null)) == Convert.ToString(uniqueFilter.Value))
            {
                var existingEntity = ServiceLocator.EntityService.GetInstance<T>(uniqueFilter);
                if (existingEntity != null)
                {
                    entity = WebUtils.FormDataToModel<T>(existingEntity, Request.Form, isNestedObject);
                }
            }

            return entity;
        }
    }

    #region DXBaseGridController
    public abstract class DXBaseGridController : BaseController
    {
        protected string _GRID { get; set; }
        protected GridViewSettings GridViewSettings { get; set; }
        public static int MinPageSize = 20;

        public abstract PartialViewResult GetDataForGrid(GridViewModel gridViewModel);

        protected GridViewModel CreateGridViewModel(string keyFieldName, IEnumerable<string> columns)
        {
            var viewModel = new GridViewModel();
            viewModel.KeyFieldName = keyFieldName;
            foreach (var c in columns)
            {
                viewModel.Columns.Add(c);
            }
            return viewModel;
        }

        //Paging
        public virtual ActionResult Page(GridViewPagerState pager)
        {
            var viewModel = GridViewExtension.GetViewModel(_GRID);
            viewModel.ApplyPagingState(pager);
            return GetDataForGrid(viewModel);
        }

        //Sorting
        public virtual ActionResult Sort(GridViewColumnState column, bool reset)
        {
            var viewModel = GridViewExtension.GetViewModel(_GRID);
            viewModel.ApplySortingState(column, reset);
            return GetDataForGrid(viewModel);
        }

        // Filtering
        public virtual ActionResult Filter(GridViewFilteringState filteringState)
        {
            var viewModel = GridViewExtension.GetViewModel(_GRID);
            viewModel.ApplyFilteringState(filteringState);
            return GetDataForGrid(viewModel);
        }

        // EXPORTING PROPERTIES AND METHODS
        public delegate ActionResult GridViewExportMethod(GridViewSettings settings, object dataObject);
        private static Dictionary<GridViewExportFormat, GridViewExportMethod> _ExportFormatsInfo;
        //Xlsx, Rtf,
        public enum GridViewExportFormat { None, Pdf, Xls, Csv }

        static Dictionary<GridViewExportFormat, GridViewExportMethod> CreateExportFormatsInfo()
        {
            return new Dictionary<GridViewExportFormat, GridViewExportMethod> {
                { GridViewExportFormat.Pdf, GridViewExtension.ExportToPdf },
                //{ GridViewExportFormat.Rtf, GridViewExtension.ExportToRtf },
                {
                    GridViewExportFormat.Xls,
                    (settings, data) => GridViewExtension.ExportToXls(settings, data, new XlsExportOptionsEx { ExportType = DevExpress.Export.ExportType.WYSIWYG })
                },
                //{ 
                //    GridViewExportFormat.Xlsx,
                //    (settings, data) => GridViewExtension.ExportToXlsx(settings, data, new XlsxExportOptionsEx { ExportType = DevExpress.Export.ExportType.WYSIWYG })
                //},
                {
                    GridViewExportFormat.Csv,
                    (settings, data) => GridViewExtension.ExportToCsv(settings, data, new CsvExportOptionsEx { ExportType = DevExpress.Export.ExportType.WYSIWYG })
                }
            };
        }

        public static Dictionary<GridViewExportFormat, GridViewExportMethod> ExportFormatsInfo
        {
            get
            {
                if (_ExportFormatsInfo == null) _ExportFormatsInfo = CreateExportFormatsInfo();
                return _ExportFormatsInfo;
            }
        }

        protected GridViewExportFormat GetExportFormat(string type)
        {
            var exptFormat = ExportFormatsInfo.Keys.Where(x => x.ToString() == type).DefaultIfEmpty(GridViewExportFormat.None).First();
            return exptFormat;
        }
    }
    #endregion

    public abstract class JQBaseGridController : BaseController
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

        protected PaginatedList<T> GetPaginatedList<T>(PaginationCriteria criteria) where T : BaseEntity
        {
            var result = ServiceLocator.EntityService.GetPaginatedList<T>(criteria);
            return result;
        }

        protected PaginatedList<T> GetPaginatedList<T>(int pageSize, int pageNumber, string sortProperty, string sortOrder, PropertyFilter filter) where T : BaseEntity
        {
            var filters = new PropertyFilter[] { filter };
            return GetPaginatedList<T>( pageSize, pageNumber, sortProperty, sortOrder, filters);
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
            var properties = WebUtils.FormToList(Request.Form);
            criteria.Filters = WebUtils.ConvertRequestParamsToPropertyFilters(typeof(T), properties);
            return criteria;
        }

        protected IEnumerable<T> GetTopNList<T>(long nbrRecords, IEnumerable<PropertyFilter> filters = null, IEnumerable<OrderFilter> order = null) where T : BaseEntity
        {
            var result = ServiceLocator.EntityService.GetTopNList<T>(nbrRecords, filters, order);
            return result;
        }
    }
}