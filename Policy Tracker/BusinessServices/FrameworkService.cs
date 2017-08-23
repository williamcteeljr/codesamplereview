using DevExpress.Web;
using DevExpress.Web.Mvc;
using PolicyTracker.BusinessServices.Brokers;
using PolicyTracker.BusinessServices.ComboBox;
using PolicyTracker.BusinessServices.Risks;
using PolicyTracker.BusinessServices.Security;
using PolicyTracker.DataAccess;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.Platform.Caching;
using PolicyTracker.Platform.Security;
using PolicyTracker.Platform.UOW;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace PolicyTracker.BusinessServices
{
    public static class ApplicationManager
    {
        public static void Initialize()
        {
            DAOFactory.Initialize();
            //ServiceFactory.Initialize();
        }
    }

    #region Validations
    public class BusinessRulesException : Exception
    {
        public BusinessRulesException(IList<ValidationResult> validations)
        {
            Warnings = validations;
        }

        public BusinessRulesException(string message)
        {
            var result = new ValidationResult(message);
            Warnings = new List<ValidationResult>() { result };
        }

        public IList<ValidationResult> Warnings { get; private set; }
    }

    public class ValidationRulesException : Exception
    {
        public ValidationRulesException(IList<ValidationResult> validations)
        {
            Errors = validations;
        }

        public ValidationRulesException(string message)
        {
            var result = ValidationHelper.CreateGeneralError(message);
            Errors = new List<ValidationResult>() { result };
        }

        public IList<ValidationResult> Errors { get; private set; }
    }

    public static class ValidationHelper
    {
        public static List<ValidationResult> ValidateEntity(object instance)
        {
            var valResults = new List<ValidationResult>();
            var valContext = new ValidationContext(instance);
            Validator.TryValidateObject(instance, valContext, valResults, true);
            return valResults;
        }

        public static ValidationResult CreateGeneralError(string message)
        {
            return new ValidationResult(message, new string[] { "Errors" });
        }

        public static ValidationResult CreateGeneralError(string formattedMessage, object param0, object param1 = null)
        {
            if (param1 == null)
            {
                return new ValidationResult(String.Format(formattedMessage, param0), new string[] { "Errors" });
            }
            else
            {
                return new ValidationResult(String.Format(formattedMessage, param0, param1), new string[] { "Errors" });
            }
        }

        public static void SetRequestWarnings(List<ValidationResult> warnings)
        {
            CacheManager.RequestCache.SetValue("Warnings", warnings);
        }

        public static List<ValidationResult> GetRequestWarnings()
        {
            var result = (List<ValidationResult>)CacheManager.RequestCache.GetValue("Warnings");
            return result;
        }
    }
    #endregion

    //public abstract class BaseService
    //{
    //    public delegate object ComboFilterDelegate(ListEditItemsRequestedByFilterConditionEventArgs args);
    //    public delegate object ComboValueDelegate(ListEditItemRequestedByValueEventArgs args);
    //}

    public abstract class BaseComboBoxService
    {
        public ItemsRequestedByFilterConditionMethod ItemRequestByFilter { get; private set; }
        public ItemRequestedByValueMethod ItemRequestByValue { get; private set; }

        public BaseComboBoxService()
        {
            ItemRequestByFilter = new ItemsRequestedByFilterConditionMethod(GetByFilter);
            ItemRequestByValue = new ItemRequestedByValueMethod(GetByValue);
        }

        public abstract object GetByFilter(ListEditItemsRequestedByFilterConditionEventArgs args);
        public abstract object GetByValue(ListEditItemRequestedByValueEventArgs args);

        public static int GetPageSize(int beginIndex, int endIndex)
        {
            return endIndex - beginIndex;
        }

        public static int GetPageNumber(int endIndex, int pageSize)
        {
            return (endIndex + 1) / pageSize;
        }
    }

    #region BaseGridSvc [Common Service For Fetching Grid Data]
    public abstract class BaseService
    {
        public GridViewCustomBindingGetDataRowCountHandler GetRowCount { get; set; }
        public GridViewCustomBindingGetDataHandler GetDataSimple = new GridViewCustomBindingGetDataHandler(GetGridDataSimple);
        public GridViewCustomBindingGetSummaryValuesHandler GetDataAdvanced = new GridViewCustomBindingGetSummaryValuesHandler(GetGridDataAdvanced);

        public BaseService()
        {
            GetRowCount = new GridViewCustomBindingGetDataRowCountHandler(GetDataRowCount);
        }

        /// <summary>
        /// Returns the Sorting column when exporting from a grid.
        /// </summary>
        /// <param name="columns">ReadOnly Grid Column Collection </param>
        /// <returns>List of Order Filters for Data Sorting</returns>
        public static List<OrderFilter> GetOrderFilterForExport(IList<GridViewDataColumn> columns)
        {
            var orderFilters = new List<OrderFilter>();
            columns.OrderByDescending(x => x.SortIndex);
            foreach (var c in columns)
            {
                if (c.SortIndex > -1)
                {
                    var filter = new OrderFilter(c.FieldName, c.SortOrder.ToString());
                    orderFilters.Add(filter);
                }
            }
            return orderFilters;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static PaginationCriteria GetPaginationCriteria<T>(GridViewCustomBindingGetDataRowCountArgs e) where T : BaseEntity
        {
            var sortFilters = new List<OrderFilter>();

            foreach (var ordFilter in e.State.SortedColumns)
            {
                sortFilters.Add(new OrderFilter(ordFilter.FieldName, ordFilter.SortOrder.ToString()));
            }

            PaginationCriteria criteria = new PaginationCriteria(e.State.Pager.PageSize, (e.State.Pager.PageIndex + 1), sortFilters);

            List<PropertyFilter> filters = (e.State.IsFilterApplied) ? ServiceUtils.GetFiltersFromGrid<T>(e.State.Columns) : null;
            criteria.Filters = filters;

            return criteria;
        }

        public abstract void GetDataRowCount(GridViewCustomBindingGetDataRowCountArgs e);

        public static void GetGridDataSimple(GridViewCustomBindingGetDataArgs e)
        {
            var userId = SessionManager.GetCurrentSession().User.UserId;
            var x = CacheManager.RequestCache.GetValue(userId+"GridData") as IEnumerable<object>;
            e.Data = x;
            CacheManager.RequestCache.RemoveValue("GridData");
        }

        public static void GetGridDataAdvanced(GridViewCustomBindingGetSummaryValuesArgs e)
        {
            // Implement Method Body
        }
    }
    #endregion

    #region DXServiceLocator
    public static class DXServiceLocator
    {
        private static Dictionary<string, Object> _Instances = new Dictionary<string, object>();

        static DXServiceLocator()
        {
            _Instances.Add(typeof(BrokerComboService).Name, new BrokerComboService());
            _Instances.Add(typeof(AirportComboSvc).Name, new AirportComboSvc());
            _Instances.Add(typeof(AircraftMakeComboService).Name, new AircraftMakeComboService());
        }

        public static BrokerComboService BrokerComboService
        {
            get
            {
                return _Instances[typeof(BrokerComboService).Name] as BrokerComboService;
            }
        }

        public static AirportComboSvc AirportComboSvc
        {
            get
            {
                return _Instances[typeof(AirportComboSvc).Name] as AirportComboSvc;
            }
        }

        public static AircraftMakeComboService AircraftMakeComboService
        {
            get
            {
                return _Instances[typeof(AircraftMakeComboService).Name] as AircraftMakeComboService;
            }
        }
    }
    #endregion DXServiceLocator

    #region ServiceLocator
    public static class ServiceLocator
    {
        private static Dictionary<string, Object> _Instances = new Dictionary<string, object>();

        static ServiceLocator()
        {
            _Instances.Add(typeof(EntityService).Name, new EntityService());
            _Instances.Add(typeof(BrokerSvc).Name, new BrokerSvc());
            _Instances.Add(typeof(BlueBookSvc).Name, new BlueBookSvc());
            _Instances.Add(typeof(PolicySvc).Name, new PolicySvc());
            _Instances.Add(typeof(WorkingListGridSvc).Name, new WorkingListGridSvc());
            _Instances.Add(typeof(SecuritySvc).Name, new SecuritySvc());
            _Instances.Add(typeof(AircraftSvc).Name, new AircraftSvc());
            _Instances.Add(typeof(SystemSvc).Name, new SystemSvc());
            _Instances.Add(typeof(ReportingService).Name, new ReportingService());
            _Instances.Add(typeof(AppManagementService).Name, new AppManagementService());
            _Instances.Add(typeof(ConsoleService).Name, new ConsoleService());
            _Instances.Add(typeof(RiskService).Name, new RiskService());
            _Instances.Add(typeof(NamedInsuredService).Name, new NamedInsuredService());
            _Instances.Add(typeof(UIPostingNoticeService).Name, new UIPostingNoticeService());
        }

        public static NamedInsuredService NamedInsuredService
        {
            get
            {
                return _Instances[typeof(NamedInsuredService).Name] as NamedInsuredService;
            }
        }

        public static UIPostingNoticeService UIPostingNoticeService
        {
            get
            {
                return _Instances[typeof(UIPostingNoticeService).Name] as UIPostingNoticeService;
            }
        }

        public static RiskService RiskService
        {
            get
            {
                return _Instances[typeof(RiskService).Name] as RiskService;
            }
        }

        public static EntityService EntityService
        {
            get
            {
                return _Instances[typeof(EntityService).Name] as EntityService;
            }
        }

        public static ConsoleService ConsoleService
        {
            get
            {
                return _Instances[typeof(ConsoleService).Name] as ConsoleService;
            }
        }

        public static ReportingService ReportingService
        {
            get
            {
                return _Instances[typeof(ReportingService).Name] as ReportingService;
            }
        }

        public static AppManagementService AppManagementService
        {
            get
            {
                return _Instances[typeof(AppManagementService).Name] as AppManagementService;
            }
        }

        public static SystemSvc SystemSvc
        {
            get
            {
                return _Instances[typeof(SystemSvc).Name] as SystemSvc;
            }
        }

        public static WorkingListGridSvc WorkingListGridSvc
        {
            get
            {
                return _Instances[typeof(WorkingListGridSvc).Name] as WorkingListGridSvc;
            }
        }

        public static AircraftSvc AircraftSvc
        {
            get
            {
                return _Instances[typeof(AircraftSvc).Name] as AircraftSvc;
            }
        }

        public static SecuritySvc SecuritySvc
        {
            get
            {
                return _Instances[typeof(SecuritySvc).Name] as SecuritySvc;
            }
        }

        public static BrokerSvc BrokerSvc
        {
            get
            {
                return _Instances[typeof(BrokerSvc).Name] as BrokerSvc;
            }
        }

        public static BlueBookSvc BlueBookSvc
        {
            get
            {
                return _Instances[typeof(BlueBookSvc).Name] as BlueBookSvc;
            }
        }

        public static PolicySvc PolicySvc
        {
            get
            {
                return _Instances[typeof(PolicySvc).Name] as PolicySvc;
            }
        }
    }
    #endregion

    #region EntityService
    //public interface IEntityService
    //{
    //    void Create<T>(T newEntity) where T : BaseEntity;
    //    void Update<T>(T entity) where T : BaseEntity;
    //    void DeleteAll<T>() where T : BaseEntity;

    //    int Count<T>(PropertyFilter filter) where T : BaseEntity;
    //    int Count<T>(IEnumerable<PropertyFilter> filters = null) where T : BaseEntity;
    //    bool Exists<T>(PropertyFilter filter) where T : BaseEntity;
    //    bool Exists<T>(IEnumerable<PropertyFilter> filters = null) where T : BaseEntity;

    //    T GetInstance<T>(PropertyFilter filter) where T : BaseEntity;
    //    T GetInstance<T>(IEnumerable<PropertyFilter> filters = null) where T : BaseEntity;
    //    dynamic GetInstance<T>(IEnumerable<string> propertySet, PropertyFilter filter) where T : BaseEntity;
    //    dynamic GetInstance<T>(IEnumerable<string> propertySet, IEnumerable<PropertyFilter> filters = null) where T : BaseEntity;

    //    IEnumerable<T> GetList<T>(PropertyFilter filter, IEnumerable<OrderFilter> order = null) where T : BaseEntity;
    //    IEnumerable<T> GetList<T>(IEnumerable<PropertyFilter> filters = null, IEnumerable<OrderFilter> order = null) where T : BaseEntity;
    //    IEnumerable<T> GetTopNList<T>(long nbrRecords, IEnumerable<PropertyFilter> filters = null, IEnumerable<OrderFilter> order = null) where T : BaseEntity;
    //    IEnumerable<dynamic> GetList<T>(IEnumerable<string> propertySet, PropertyFilter filter, IEnumerable<OrderFilter> order = null) where T : BaseEntity;
    //    IEnumerable<dynamic> GetList<T>(IEnumerable<string> propertySet, IEnumerable<PropertyFilter> filters = null, IEnumerable<OrderFilter> order = null) where T : BaseEntity;

    //    PaginatedList<T> GetPaginatedList<T>(PaginationCriteria criteria = null) where T : BaseEntity;
    //    PaginatedList<dynamic> GetPaginatedList<T>(IEnumerable<string> propertySet, PaginationCriteria criteria = null) where T : BaseEntity;
    //}

    public class EntityService
    {
        public void Create<T>(T newEntity) where T : BaseEntity
        {
            UnitOfWork uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            DAOFactory.GetDAOForEntity<T>().Create(uow, newEntity);
        }

        public int Count<T>(PropertyFilter filter) where T : BaseEntity
        {
            List<PropertyFilter> filters = new List<PropertyFilter>() { filter };
            return Count<T>( filters);
        }

        public int Count<T>(IEnumerable<PropertyFilter> filters = null) where T : BaseEntity
        {
            UnitOfWork uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            int result = DAOFactory.GetDAOForEntity<T>().Count(uow, filters);
            return result;
        }

        public void DeleteAll<T>() where T : BaseEntity
        {
            UnitOfWork uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            DAOFactory.GetDAOForEntity<T>().Delete(uow, new PropertyFilter("RecordId", PropertyFilter.Comparator.GreaterEquals, 0));
        }

        public bool Exists<T>( PropertyFilter filter) where T : BaseEntity
        {
            List<PropertyFilter> filters = new List<PropertyFilter>() { filter };
            return Exists<T>(filters);
        }

        public bool Exists<T>(IEnumerable<PropertyFilter> filters = null) where T : BaseEntity
        {
            UnitOfWork uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            bool result = DAOFactory.GetDAOForEntity<T>().Exists(uow, filters);
            return result;
        }

        public T GetInstance<T>(PropertyFilter filter) where T : BaseEntity
        {
            List<PropertyFilter> filters = new List<PropertyFilter>() { filter };
            return GetInstance<T>(filters);
        }

        public T GetInstance<T>(IEnumerable<PropertyFilter> filters = null) where T : BaseEntity
        {
            UnitOfWork uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            T result = DAOFactory.GetDAOForEntity<T>().GetInstance(uow, filters);
            return result;
        }

        public dynamic GetInstance<T>(IEnumerable<string> propertySet, PropertyFilter filter) where T : BaseEntity
        {
            List<PropertyFilter> filters = new List<PropertyFilter>() { filter };
            return GetInstance<T>(propertySet, filters);
        }

        public dynamic GetInstance<T>(IEnumerable<string> propertySet, IEnumerable<PropertyFilter> filters = null) where T : BaseEntity
        {
            UnitOfWork uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            dynamic result = DAOFactory.GetDAOForEntity<T>().GetInstance(uow, propertySet, filters);
            return result;
        }

        public IEnumerable<T> GetList<T>(IEnumerable<PropertyFilter> filters, OrderFilter order) where T : BaseEntity
        {
            List<OrderFilter> orderFilters = new List<OrderFilter>() { order };
            return GetList<T>(filters, orderFilters);
        }

        public IEnumerable<T> GetList<T>(PropertyFilter filter, IEnumerable<OrderFilter> order = null) where T : BaseEntity
        {
            List<PropertyFilter> filters = new List<PropertyFilter>() { filter };
            return GetList<T>(filters, order);
        }

        public IEnumerable<T> GetList<T>(IEnumerable<PropertyFilter> filters = null, IEnumerable<OrderFilter> order = null) where T : BaseEntity
        {
            UnitOfWork uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            IEnumerable<T> results = DAOFactory.GetDAOForEntity<T>().GetList(uow, filters, order);
            return results;
        }

        public IEnumerable<dynamic> GetList<T>(IEnumerable<string> propertySet, PropertyFilter filter, IEnumerable<OrderFilter> order = null) where T : BaseEntity
        {
            List<PropertyFilter> filters = new List<PropertyFilter>() { filter };
            return GetList<T>(propertySet, filters, order);
        }

        public IEnumerable<dynamic> GetList<T>(IEnumerable<string> propertySet, IEnumerable<PropertyFilter> filters = null, IEnumerable<OrderFilter> order = null) where T : BaseEntity
        {
            UnitOfWork uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            IEnumerable<dynamic> results = DAOFactory.GetDAOForEntity<T>().GetList(uow, propertySet, filters, order);
            return results;
        }

        public IEnumerable<T> GetTopNList<T>(long nbrRecords, PropertyFilter filter = null, IEnumerable<OrderFilter> order = null) where T : BaseEntity
        {
            var filters = new[] { filter };
            return GetTopNList<T>(nbrRecords, filters, order);
        }

        public IEnumerable<T> GetTopNList<T>(long nbrRecords, IEnumerable<PropertyFilter> filters = null, IEnumerable<OrderFilter> order = null) where T : BaseEntity
        {
            UnitOfWork uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            IEnumerable<T> results = DAOFactory.GetDAOForEntity<T>().GetTopNList(uow, nbrRecords, filters, order);
            return results;
        }

        public PaginatedList<T> GetPaginatedList<T>(PaginationCriteria criteria = null) where T : BaseEntity
        {
            UnitOfWork uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            PaginatedList<T> results = DAOFactory.GetDAOForEntity<T>().GetPaginatedList(uow, criteria);
            return results;
        }

        public PaginatedList<dynamic> GetPaginatedList<T>(IEnumerable<string> propertySet, PaginationCriteria criteria = null) where T : BaseEntity
        {
            UnitOfWork uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            PaginatedList<dynamic> results = DAOFactory.GetDAOForEntity<T>().GetPaginatedList(uow, propertySet, criteria);
            return results;
        }

        public void Update<T>(T entity) where T : BaseEntity
        {
            UnitOfWork uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            DAOFactory.GetDAOForEntity<T>().Update(uow, entity);
        }
    }
    #endregion
}
