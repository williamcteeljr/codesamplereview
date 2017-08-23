using Dapper;
using PolicyTracker.Platform.Logging;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.DomainModel.Policy;
using PolicyTracker.Platform.Security;
using PolicyTracker.Platform.UOW;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PolicyTracker.DataAccess
{
    public class DataAccessException : Exception
    {
        internal DataAccessException() : base() { }
        internal DataAccessException(string message) : base(message) { }
        internal DataAccessException(string message, Exception innerException) : base(message, innerException) { }
    }

    #region DapperDAO
    public abstract class DapperDAO
    {
        protected static readonly string _DB = "DBName";

        protected DapperDAO() { }

        protected int Count(UnitOfWork uow, string sql, object param = null)
        {
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append("SELECT COUNT(*)");
            sqlBuilder.Append(" FROM (").Append(sql).Append(") as VirtualTable");
            int count = Query<int>(uow, sqlBuilder.ToString(), param).SingleOrDefault();
            return count;
        }

        protected int Execute(UnitOfWork uow, string sql, object param = null)
        {
            try
            {
                Stopwatch timer = new Stopwatch();
                timer.Start();
                int rowsAffected = uow.Connection.Execute(sql, param, uow.Transaction);
                timer.Stop();
                var logString = new StringBuilder("[").Append(timer.ElapsedMilliseconds).Append("ms]  ").Append(sql).ToString();
                LogLevel level = (timer.ElapsedMilliseconds > 200) ? LogLevel.WARN : LogLevel.INFO;
                //LogManager.LogPerformance(level, logString);
                if (param is DynamicParameters) LogSQLParameters(param as DynamicParameters);
                if (rowsAffected == 0) LogManager.Log(LogLevel.WARN, "Executed SQL Statment Did Not Affect Any Rows");
                return rowsAffected;
            }
            catch (Exception ex)
            {
                if (uow.Connection.State == ConnectionState.Closed)
                {
                    throw new DataAccessException("The connection has been closed", ex);
                }

                LogManager.Log(LogLevel.ERROR, "The following SQL produced the error [{0}]" + Environment.NewLine + "{1}", ex.Message, sql);
                throw new DataAccessException("An error occurred during a data operation", ex);
            }
        }

        protected bool Exists(UnitOfWork uow, string sql, object param = null)
        {
            StringBuilder sqlBuilder = new StringBuilder("SELECT 1 WHERE EXISTS (").Append(sql).Append(")");
            int result = Query<int>(uow, sqlBuilder.ToString(), param).SingleOrDefault();
            return (result != 0);
        }

        public static U QueryStateless<U>(UnitOfWork uow, string sql)
        {
            U result = uow.Connection.Query<U>(sql: sql, transaction: uow.Transaction).SingleOrDefault();
            return result;
        }

        protected IEnumerable<U> Query<U>(UnitOfWork uow, string sql, object param = null)
        {
            try
            {
                Stopwatch timer = new Stopwatch();
                timer.Start();
                IEnumerable<U> results = uow.Connection.Query<U>(sql, param, uow.Transaction);
                timer.Stop();
                var logString = new StringBuilder("[").Append(timer.ElapsedMilliseconds).Append("ms]  ").Append(sql).ToString();
                LogLevel level = (timer.ElapsedMilliseconds > 200) ? LogLevel.WARN : LogLevel.INFO;
                //LogManager.LogPerformance(level, logString);
                var dapperParam = param as DynamicParameters;
                if (dapperParam != null)
                {
                    LogSQLParameters(dapperParam);
                }
                return results;
            }
            catch (Exception ex)
            {
                if (uow.Connection.State == ConnectionState.Closed)
                {
                    throw new DataAccessException("The connection has been closed", ex);
                }

                LogManager.Log(LogLevel.ERROR, "The following SQL produced the error [{0}]" + Environment.NewLine + "{1}", ex.Message, sql);
                throw new DataAccessException("An error occurred accessing the data", ex);
            }
        }

        protected IEnumerable<U> ExecuteStoredProcedureList<U>(UnitOfWork uow, string name, object param = null)
        {
            try
            {
                IEnumerable<U> result = uow.Connection.Query<U>(name, param, uow.Transaction, commandType: CommandType.StoredProcedure);
                var dapperParam = param as DynamicParameters;
                if (dapperParam != null)
                {
                    LogSQLParameters(dapperParam);
                }
                return result;
            }
            catch (Exception ex)
            {
                if (uow.Connection.State == ConnectionState.Closed)
                {
                    throw new DataAccessException("The connection has been closed", ex);
                }

                throw new DataAccessException("An error occurred accessing the data", ex);
            }
        }

        protected U ExecuteStoredProcedure<U>(UnitOfWork uow, string name, object param = null)
        {
            U result = ExecuteStoredProcedureList<U>(uow, name, param).FirstOrDefault();
            return result;
        }

        protected PaginatedList<U> GetPaginatedList<U>(UnitOfWork uow, string selectSql, string orderBySql, object param, int pageSize = 10, int pageNumber = 1)
        {
            // Get Record Count and Calculate Pagination Values
            int count = Count(uow, selectSql, param);
            pageSize = (pageSize <= 0) ? 10 : pageSize;
            pageNumber = (pageNumber <= 0) ? 1 : pageNumber;
            int startRow = ((pageNumber - 1) * pageSize) + 1;
            int endRow = startRow + pageSize - 1;
            int totalPages = (int)count / pageSize;
            if ((int)count % pageSize > 0) totalPages += 1;

            // Execute SQL and return PaginatedList
            // TODO:  If Count was 0, do we need to run Paginated Query??
            IEnumerable<U> searchResults = PaginatedQuery<U>(uow, selectSql, orderBySql, startRow, endRow, param);
            PaginatedList<U> list = new PaginatedList<U>() { CurrentPage = pageNumber, Results = searchResults, TotalPages = totalPages, TotalResults = count };
            return list;
        }

        protected IEnumerable<U> PaginatedQuery<U>(UnitOfWork uow, string selectSql, string orderBySql, int startRow, int endRow, object param)
        {
            // Build SQL Statement for Pagination (SQL Server 2008+)
            StringBuilder sqlBuilder = new StringBuilder();
            // TODO:  Need to Inject RowNumber into Select Statement (beginning or end)
            //selectSql = selectSql.Replace("SELECT ", new StringBuilder("SELECT row_number()").Append(" OVER (").Append(orderBySql).Append(") as RowNumber, ").ToString());
            selectSql = selectSql.Substring(6);
            selectSql = selectSql.Insert(0, "SELECT row_number() OVER (" + orderBySql + ") as RowNumber, ");
            sqlBuilder.Append("SELECT * FROM (").Append(selectSql).Append(") as PaginatedView ");
            sqlBuilder.Append("WHERE RowNumber BETWEEN ").Append(startRow).Append(" AND ").Append(endRow).Append(" ");
            sqlBuilder.Append("ORDER BY RowNumber");
            IEnumerable<U> searchResults = Query<U>(uow, sqlBuilder.ToString(), param);
            return searchResults;
        }

        private void LogSQLParameters(DynamicParameters dapperParams)
        {
            if (dapperParams == null || dapperParams.ParameterNames.Count() == 0) return;

            StringBuilder sb = new StringBuilder();
            foreach (var keyName in dapperParams.ParameterNames)
            {
                if (sb.Length != 0)
                {
                    sb.Append(", ");
                }
                sb.Append("@").Append(keyName).Append(" = ");
                object paramObj = dapperParams.GetParamValue(keyName);
                if (paramObj is IEnumerable && !(paramObj is string))
                {
                    sb.Append("[");
                    bool isFirst = true;
                    foreach (var obj in paramObj as IEnumerable)
                    {
                        if (!isFirst) sb.Append(",");
                        sb.Append(obj.ToString());
                        isFirst = false;
                    }
                    sb.Append("]");
                }
                else
                {
                    if (paramObj != null)
                        sb.Append(paramObj.ToString());
                    else
                        sb.Append("null");
                }
            }

            //LogManager.LogPerformance(LogLevel.INFO, "Parameters:  {" + sb.ToString() + "}");
        }
    }
    #endregion

    #region BaseDAO
    public abstract class BaseDAO<T> : DapperDAO where T : BaseEntity
    {
        protected static readonly string IdentitySQL = "SELECT CAST(SCOPE_IDENTITY() AS bigint)";

        protected string TableName { get; set; }
        protected string EscapedTableName { get; set; }
        protected string SourceDB { get; set; }
        protected OrderFilter DefaultOrderFilter { get; set; }
        protected string SelectAllSQL { get; private set; }
        protected string InsertAllSQL { get; private set; }
        protected string UpdateAllSQL { get; private set; }

        protected BaseDAO(string tableName, string sourceDB, bool identityIsPartOfKey = false, OrderFilter defaultOrderFilter = null)
        {
            if (defaultOrderFilter != null) DefaultOrderFilter = defaultOrderFilter;
            TableName = tableName;
            EscapedTableName = "[" + tableName + "]";
            SourceDB = sourceDB;
            //AddColumnMapping("RecordId", "RECNUM", identityIsPartOfKey, true);
            Type entityType = this.GetType().BaseType.GenericTypeArguments[0];
            if (typeof(T).IsSubclassOf(typeof(AuditedEntity)))
            {
                //AddColumnMapping("CreatedBy", "CREATED_BY");
                //AddColumnMapping("LastEdit", "LAST_EDIT");
                //AddColumnMapping("PriorEdit", "PRIOR_EDIT");
            }
        }

        public int Count(UnitOfWork uow, PropertyFilter filter)
        {
            List<PropertyFilter> filters = new List<PropertyFilter>() { filter };
            return Count(uow, filters);
        }

        public int Count(UnitOfWork uow, IEnumerable<PropertyFilter> filters = null)
        {
            StringBuilder sql = new StringBuilder("SELECT COUNT(*) FROM ").Append(GetSourceTableName(uow, TableName));
            if (filters != null)
            {
                sql.Append(BuildWhereClause(filters));
            }
            DynamicParameters dapperParams = ToDynamicParameters(filters);
            int count = base.Query<int>(uow, sql.ToString(), dapperParams).Single();
            return count;
        }

        public bool Exists(UnitOfWork uow, PropertyFilter filter)
        {
            var filters = new PropertyFilter[] { filter };
            return Exists(uow, filters);
        }

        public bool Exists(UnitOfWork uow, IEnumerable<PropertyFilter> filters = null)
        {
            var entityName = ORCatalog.GetEntityNameFromTable(TableName);
            var prop = ORCatalog.GetMetadataForEntity(entityName).Where(y => y.IsIdentityField).FirstOrDefault();
            StringBuilder sql = new StringBuilder("SELECT ").Append(prop.EscapedColumnName).Append(" FROM ").Append(GetSourceTableName(uow, TableName));
            if (filters != null)
            {
                sql.Append(BuildWhereClause(filters));
            }
            DynamicParameters dapperParams = ToDynamicParameters(filters);
            bool result = base.Exists(uow, sql.ToString(), dapperParams);
            return result;
        }

        private U GetInstance<U>(UnitOfWork uow, string selectSql, IEnumerable<PropertyFilter> filters)
        {
            // Build Complete Query
            StringBuilder sql = new StringBuilder(selectSql);
            sql.Append(" FROM ").Append(GetSourceTableName(uow, TableName));
            if (filters != null)
            {
                sql.Append(BuildWhereClause(filters));
            }

            // Execute Query and Return Results
            DynamicParameters dapperParams = ToDynamicParameters(filters);
            U result = base.Query<U>(uow, sql.ToString(), dapperParams).SingleOrDefault();
            return result;
        }

        private string GetColumnSQLForRelatedType<T>() where T : BaseEntity
        {
            string columnSQL = String.Empty;
            columnSQL = DAOFactory.GetDAOForEntity<T>().SelectAllSQL;
            columnSQL = columnSQL.Replace("SELECT ", "");
            return columnSQL;
        }

        public T GetInstanceGraph<T, U>(UnitOfWork uow, Func<T, U, T> map, PropertyFilter filter)
            where T : BaseEntity where U : BaseEntity
        {
            List<PropertyFilter> filters = new List<PropertyFilter>() { filter };
            return GetInstanceGraph<T, U>(uow, map, filters);
        }

        public T GetInstanceGraph<T, U, G>(UnitOfWork uow, Func<T, U, G, T> map, PropertyFilter filter)
            where T : BaseEntity where U : BaseEntity where G : BaseEntity
        {
            List<PropertyFilter> filters = new List<PropertyFilter>() { filter };
            return GetInstanceGraph<T, U, G>(uow, map, filters);
        }

        public T GetInstanceGraph<T, U, G, O>(UnitOfWork uow, Func<T, U, G, O, T> map, PropertyFilter filter)
            where T : BaseEntity where U : BaseEntity where G : BaseEntity where O : BaseEntity
        {
            List<PropertyFilter> filters = new List<PropertyFilter>() { filter };
            return GetInstanceGraph<T, U, G, O>(uow, map, filters);
        }

        public T GetInstanceGraph<T, U>(UnitOfWork uow, Func<T, U, T> map, List<PropertyFilter> filters)
            where T : BaseEntity where U : BaseEntity
        {
            var types = new[] { typeof(U) };
            DynamicParameters dapperParams = ToDynamicParameters(filters);

            StringBuilder sql = new StringBuilder(SelectAllSQL);
            sql.Append(", ").Append(GetColumnSQLForRelatedType<U>());
            sql.Append(" FROM ").Append(GetSourceTableName(uow, TableName));

            sql.Append(BuildJoinSQL<T>(TableName, types));

            if (filters != null) sql.Append(BuildWhereClause(filters));

            var data = uow.Connection.Query<T, U, T>(sql.ToString(), map, dapperParams, uow.Transaction);
            T result = (T)data.First();

            return result;
        }

        public T GetInstanceGraph<T, U, G>(UnitOfWork uow, Func<T, U, G, T> map, List<PropertyFilter> filters)
            where T : BaseEntity where U : BaseEntity where G : BaseEntity
        {
            var types = new[] { typeof(U), typeof(G) };
            DynamicParameters dapperParams = ToDynamicParameters(filters);

            StringBuilder sql = new StringBuilder(SelectAllSQL);
            sql.Append(", ").Append(GetColumnSQLForRelatedType<U>());
            sql.Append(", ").Append(GetColumnSQLForRelatedType<G>());
            sql.Append(" FROM ").Append(GetSourceTableName(uow, TableName));

            sql.Append(BuildJoinSQL<T>(TableName, types));

            if (filters != null) sql.Append(BuildWhereClause(filters));

            var data = uow.Connection.Query<T, U, G, T>(sql.ToString(), map, dapperParams, uow.Transaction);
            T result = (T)data.First();

            return result;
        }

        public T GetInstanceGraph<T, U, G, O>(UnitOfWork uow, Func<T, U, G, O, T> map, List<PropertyFilter> filters)
            where T : BaseEntity where U : BaseEntity where G : BaseEntity
        {
            var types = new[] { typeof(U), typeof(G), typeof(O) };
            DynamicParameters dapperParams = ToDynamicParameters(filters);

            StringBuilder sql = new StringBuilder(SelectAllSQL);
            sql.Append(", ").Append(GetColumnSQLForRelatedType<U>());
            sql.Append(", ").Append(GetColumnSQLForRelatedType<G>());
            sql.Append(" FROM ").Append(GetSourceTableName(uow, TableName));

            sql.Append(BuildJoinSQL<T>(TableName, types));

            if (filters != null) sql.Append(BuildWhereClause(filters));

            var data = uow.Connection.Query<T, U, G, O, T>(sql.ToString(), map, dapperParams, uow.Transaction);
            T result = (T)data.First();

            return result;
        }

        public IEnumerable<T> GetListGraph<T, U>(UnitOfWork uow, Func<T, U, T> map, PropertyFilter filter)
            where T : BaseEntity where U : BaseEntity
        {
            List<PropertyFilter> filters = new List<PropertyFilter>() { filter };
            return GetListGraph<T, U>(uow, map, filters);
        }

        public IEnumerable<T> GetListGraph<T, U>(UnitOfWork uow, Func<T, U, T> map, List<PropertyFilter> filters)
            where T : BaseEntity
            where U : BaseEntity
        {
            var types = new[] { typeof(U) };
            DynamicParameters dapperParams = ToDynamicParameters(filters);

            StringBuilder sql = new StringBuilder(SelectAllSQL);
            sql.Append(", ").Append(GetColumnSQLForRelatedType<U>());
            sql.Append(" FROM ").Append(GetSourceTableName(uow, TableName));

            sql.Append(BuildJoinSQL<T>(TableName, types));

            if (filters != null) sql.Append(BuildWhereClause(filters));

            IEnumerable<T> results = uow.Connection.Query<T, U, T>(sql.ToString(), map, dapperParams, uow.Transaction);

            return results;
        }

        public string BuildJoinSQL<T>(string entityTableName, Type[] types)
        {
            StringBuilder sql = new StringBuilder();
            var erm = ORCatalog.GetRelationalMetadataForEntity(typeof(T).Name);

            foreach (Type type in types)
            {
                //RM (Relationship Map)
                var rm = erm.RelatedTables.Where(x => x.Table == type.Name).FirstOrDefault();
                sql.Append(" JOIN ").Append(GetSourceTableName(rm.Table)).Append(" on ");
                var entityProp = ORCatalog.GetMetadataForProperty(typeof(T), rm.Property);
                var fkProp = ORCatalog.GetMetadataForProperty(type, rm.RelationProperty);
                sql.Append(GetSourceTableName(TableName) + "." + entityProp.EscapedColumnName + " = " + GetSourceTableName(rm.Table) + "." + fkProp.EscapedColumnName);
            }

            return sql.ToString();
        }

        public T GetInstance(UnitOfWork uow, PropertyFilter filter)
        {
            List<PropertyFilter> filters = new List<PropertyFilter>() { filter };
            return GetInstance(uow, filters);
        }

        public T GetInstance(UnitOfWork uow, IEnumerable<PropertyFilter> filters = null)
        {
            T result = GetInstance<T>(uow, SelectAllSQL, filters);
            if (result != null && result is ISelfTracking)
            {
                (result as ISelfTracking).AcceptChanges();
            }
            return result;
        }

        // Example:  GetInstance(uow, new string[] { "FirstName", "LastName" }, new PropertyFilter("Name", username));
        public dynamic GetInstance(UnitOfWork uow, IEnumerable<string> propertySet, PropertyFilter filter)
        {
            List<PropertyFilter> filters = new List<PropertyFilter>() { filter };
            return GetInstance(uow, propertySet, filters);
        }

        public dynamic GetInstance(UnitOfWork uow, IEnumerable<string> propertySet, IEnumerable<PropertyFilter> filters = null)
        {
            return GetInstance<dynamic>(uow, BuildSelectColumnSQL(propertySet), filters);
        }

        private IEnumerable<U> GetList<U>(UnitOfWork uow, string selectSql, IEnumerable<PropertyFilter> filters, IEnumerable<OrderFilter> order = null)
        {
            // Build Complete Query
            StringBuilder sql = new StringBuilder(selectSql);
            sql.Append(" FROM ").Append(GetSourceTableName(uow, TableName));
            if (filters != null)
            {
                sql.Append(BuildWhereClause(filters));
            }
            if (order == null && DefaultOrderFilter != null) order = new OrderFilter[] { DefaultOrderFilter };
            sql.Append(BuildOrderClause(order));

            // Execute Query and Return Results
            DynamicParameters dapperParams = ToDynamicParameters(filters);
            IEnumerable<U> results = base.Query<U>(uow, sql.ToString(), dapperParams);
            return results;
        }

        public IEnumerable<T> GetList(UnitOfWork uow, PropertyFilter filter, OrderFilter order)
        {
            List<PropertyFilter> filters = new List<PropertyFilter>() { filter };
            List<OrderFilter> orderFilter = new List<OrderFilter>() { order };
            return GetList(uow, filters, orderFilter);
        }

        public IEnumerable<T> GetList(UnitOfWork uow, PropertyFilter filter, IEnumerable<OrderFilter> order = null)
        {
            List<PropertyFilter> filters = new List<PropertyFilter>() { filter };
            return GetList(uow, filters, order);
        }

        public IEnumerable<T> GetList(UnitOfWork uow, IEnumerable<PropertyFilter> filters = null, IEnumerable<OrderFilter> order = null)
        {
            IEnumerable<T> results = GetList<T>(uow, SelectAllSQL, filters, order);
            if (typeof(ISelfTracking).IsAssignableFrom(typeof(T)))
            {
                foreach (T item in results)
                {
                    (item as ISelfTracking).AcceptChanges();
                }
            }
            return results;
        }

        public IEnumerable<T> GetTopNList(UnitOfWork uow, long nbrRecords, IEnumerable<PropertyFilter> filters = null, IEnumerable<OrderFilter> order = null)
        {
            var selectSql = SelectAllSQL.Replace("SELECT", "SELECT TOP " + nbrRecords);
            return GetList<T>(uow, selectSql, filters, order);
        }

        public IEnumerable<T> GetTopNList(UnitOfWork uow, long nbrRecords, PropertyFilter filter = null, IEnumerable<OrderFilter> order = null)
        {
            var filters = new List<PropertyFilter>() { filter };
            var selectSql = SelectAllSQL.Replace("SELECT", "SELECT TOP " + nbrRecords);
            return GetList<T>(uow, selectSql, filters, order);
        }

        // Example:  GetList(uow, new string[] { "FirstName", "LastName" });
        public IEnumerable<dynamic> GetList(UnitOfWork uow, IEnumerable<string> propertySet, PropertyFilter filter, IEnumerable<OrderFilter> order = null)
        {
            List<PropertyFilter> filters = new List<PropertyFilter>() { filter };
            return GetList(uow, propertySet, filters, order);
        }

        public IEnumerable<dynamic> GetList(UnitOfWork uow, IEnumerable<string> propertySet, IEnumerable<PropertyFilter> filters = null, IEnumerable<OrderFilter> order = null)
        {
            return GetList<dynamic>(uow, BuildSelectColumnSQL(propertySet), filters, order);
        }

        private PaginatedList<U> GetPaginatedList<U>(UnitOfWork uow, string selectSql, PaginationCriteria criteria = null)
        {
            //string selectSql, IEnumerable<PropertyFilter> filters, string sortProperty = null
            criteria = (criteria == null) ? PaginationCriteria.Default : criteria;

            // Build SQL Statement
            StringBuilder sqlBuilder = new StringBuilder(selectSql).Append(" FROM ").Append(GetSourceTableName(uow, TableName));
            if (criteria.Filters != null)
            {
                sqlBuilder.Append(BuildWhereClause(criteria.Filters));
            }

            // Build Order By Clause
            var orderFilters = (criteria.OrderFilters.FirstOrDefault() == null) ? new[] { DefaultOrderFilter } : criteria.OrderFilters;
            var orderBySqlBuilder = new StringBuilder();
            var totalFilters = orderFilters.Count();
            for (var i = 0; i < totalFilters; i++)
            {
                var ordFilter = orderFilters.ElementAt(i);
                var metadata = ORCatalog.GetMetadataForProperty(typeof(T), ordFilter.PropertyName);
                string order = (ordFilter.Operand.Equals(OrderFilter.Comparator.Ascending)) ? "ASC" : "DESC";
                if (i == 0) orderBySqlBuilder.Append("ORDER BY");
                orderBySqlBuilder.Append(" ").Append(metadata.EscapedColumnName).Append(" " + order);
                if (i < (totalFilters - 1) && totalFilters > 1) orderBySqlBuilder.Append(",");
            }

            string orderBySql = orderBySqlBuilder.ToString();

            // Execute and return paged result
            DynamicParameters dapperParams = ToDynamicParameters(criteria.Filters);
            PaginatedList<U> list = base.GetPaginatedList<U>(uow, sqlBuilder.ToString(), orderBySql, dapperParams, criteria.PageSize, criteria.PageNumber);
            return list;
        }

        public PaginatedList<T> GetPaginatedList(UnitOfWork uow, PaginationCriteria criteria = null)
        {
            PaginatedList<T> page = GetPaginatedList<T>(uow, SelectAllSQL, criteria);
            if (typeof(ISelfTracking).IsAssignableFrom(typeof(T)))
            {
                foreach (T item in page.Results)
                {
                    (item as ISelfTracking).AcceptChanges();
                }
            }
            return page;
        }

        // Example:  GetPaginatedList(uow, new string[] { "FirstName", "LastName" });
        public PaginatedList<dynamic> GetPaginatedList(UnitOfWork uow, IEnumerable<string> propertySet, PaginationCriteria criteria = null)
        {
            return GetPaginatedList<dynamic>(uow, BuildSelectColumnSQL(propertySet), criteria);
        }

        private void SetDefaults(T entity)
        {
            if (entity is ISelfTracking) return;

            var props = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var prop in props)
            {
                // Skip read-only methods (some facades)
                if (!prop.CanWrite) continue;

                if (prop.PropertyType == typeof(String))
                {
                    // Set String to String.Empty if null
                    if (prop.GetValue(entity) == null)
                    {
                        prop.SetValue(entity, String.Empty);
                    }
                }
                else if (prop.PropertyType == typeof(DateTime?))
                {
                    // Set DateTime to Response Default if null
                    //if (prop.GetValue(entity) == null)
                    //{
                    //    prop.SetValue(entity, Response4W.DefaultDate);
                    //}
                }
            }
        }

        public void Create(UnitOfWork uow, T entity)
        {
            if (entity == null) return;

            // Set auditing properties if applicable
            if (entity is AuditedEntity)
            {
                UserSession session = SessionManager.GetCurrentSession();
                var auditedEntity = entity as AuditedEntity;
                auditedEntity.CreatedBy = session.User.UserName;
                //auditedEntity.PriorEdit = Response4W.DefaultDate.ToString();
                auditedEntity.LastEdit = DateTime.Now.ToString();
                auditedEntity.SetPropertyChanged("CreatedBy");
                auditedEntity.SetPropertyChanged("LastEdit");
                auditedEntity.SetPropertyChanged("PriorEdit");
            }

            SetDefaults(entity);

            string createSql = (entity is ISelfTracking) ? BuildInsertSQL(entity.ChangedProperties) : InsertAllSQL;
            createSql = createSql.Replace("INSERT INTO " + TableName, "INSERT INTO " + GetSourceTableName(uow, TableName));
            StringBuilder sql = new StringBuilder(createSql);
            sql.Append("; ").Append(IdentitySQL).Append(";");

            var prop = ORCatalog.GetMetadataForEntity(entity.GetType().Name).Where(y => y.IsIdentityField).FirstOrDefault();
            PropertyInfo propertyInfo = entity.GetType().GetProperty(prop.PropertyName);
            var newId = base.Query<long>(uow, sql.ToString(), entity).Single();
            propertyInfo.SetValue(entity, Convert.ChangeType(newId, propertyInfo.PropertyType), null);

            //entity.RecordId = base.Query<long>(uow, sql.ToString(), entity).Single();
            if (entity is ISelfTracking)
            {
                (entity as ISelfTracking).AcceptChanges();
            }
        }

        public bool Update(UnitOfWork uow, T entity)
        {
            // For Testing Dapper Mappings
            //var typeMap = SqlMapper.GetTypeMap(typeof(T));

            if (entity == null) return false;
            if (entity is ISelfTracking && !entity.IsChanged) return false;

            // Set auditing properties if applicable
            if (entity is AuditedEntity)
            {
                var auditedEntity = entity as AuditedEntity;
                auditedEntity.PriorEdit = auditedEntity.LastEdit;
                auditedEntity.LastEdit = DateTime.Now.ToString();
                if (auditedEntity.LastEdit != null) auditedEntity.SetPropertyChanged("LastEdit");
                if (auditedEntity.PriorEdit != null) auditedEntity.SetPropertyChanged("PriorEdit");
            }

            string sql = (entity is ISelfTracking) ? BuildUpdateSQL(entity.ChangedProperties) : UpdateAllSQL;
            sql = sql.Replace("UPDATE " + TableName, "UPDATE " + GetSourceTableName(uow, TableName));
            int rowsAffected = base.Execute(uow, sql, entity);
            if (entity is ISelfTracking)
            {
                (entity as ISelfTracking).AcceptChanges();
            }
            return (rowsAffected == 1);
        }

        public bool Update(UnitOfWork uow, IEnumerable<T> entity)
        {
            int rowsAffected = 0;
            foreach (T obj in entity)
            {
                rowsAffected += (Update(uow, obj)) ? 1 : 0;
            }
            return (rowsAffected == entity.Count());
        }
        /*
                public bool Update(UnitOfWork uow, IEnumerable<PropertyFilter> newPropertyValues, IEnumerable<PropertyFilter> filters)
                {
                    // Validation to prevent accidental batch update
                    if (newPropertyValues == null || newPropertyValues.Count() == 0) return false;
                    if (filters == null || filters.Count() == 0) return false;

                    StringBuilder batchSql = new StringBuilder("UPDATE ").Append(TableName);
                    // TODO:  Refactor with code in BuildUpdateSQL
                    // Build Column Update Clause
                    StringBuilder valuesCaluse = new StringBuilder();
                    foreach (var propVal in newPropertyValues)
                    {
                        if (valuesCaluse.Length == 0)
                        {
                            valuesCaluse.Append(" SET ");
                        }
                        else
                        {
                            valuesCaluse.Append(", ");
                        }

                        valuesCaluse.Append(BuildFilterRestriction(propVal));
                    }

                    string whereClause = BuildWhereClause(filters);
                    batchSql.Append(valuesCaluse).Append(whereClause);
                    var allParams = ToDynamicParameters(newPropertyValues);
                    allParams.AddDynamicParams(ToDynamicParameters(filters));
                    int rowsAffected = base.Execute(uow, batchSql.ToString(), allParams);
                    return (rowsAffected == 1);
                }
        */
        public bool Delete(UnitOfWork uow, T entity)
        {
            bool deleted = false;
            var prop = ORCatalog.GetMetadataForEntity(entity.GetType().Name).Where(y => y.IsIdentityField).FirstOrDefault();

            if (prop != null)
            {
                PropertyInfo propertyInfo = entity.GetType().GetProperty(prop.PropertyName);
                var filter = new PropertyFilter(prop.PropertyName, propertyInfo.GetValue(entity));
                deleted = Delete(uow, filter);
            }
            return deleted;
        }

        public bool Delete(UnitOfWork uow, PropertyFilter filter)
        {
            List<PropertyFilter> filters = new List<PropertyFilter>() { filter };
            return Delete(uow, filters);
        }

        public bool Delete(UnitOfWork uow, IEnumerable<PropertyFilter> filters)
        {
            StringBuilder sql = new StringBuilder("DELETE FROM ").Append(GetSourceTableName(uow, TableName));
            sql.Append(BuildWhereClause(filters));
            DynamicParameters dapperParams = ToDynamicParameters(filters);
            int rowsAffected = base.Execute(uow, sql.ToString(), dapperParams);
            return (rowsAffected == 1);
        }

        protected void AddColumnMapping(string property, string column, bool isKeyField = false, bool isIdentityField = false)
        {
            var metadata = new ORMetadata()
            {
                EntityName = typeof(T).Name,
                PropertyName = property,
                TableName = TableName,
                ColumnName = column,
                IsKeyField = isKeyField,
                IsIdentityField = isIdentityField
            };
            ORCatalog.AddMetadata(metadata);
        }

        protected string GetSourceTableName(UnitOfWork uow, string dbTableName)
        {
            //string sourceDB = Response4W.GetSourceDatabase(uow.Context, dbTableName);
            //Replaces the original method which allows for specifying a different database to query if needed
            //string sourceDB = uow.Context;
            var fqDbTable = new StringBuilder("[").Append(SourceDB).Append("]").Append(".[dbo]").Append(".[").Append(dbTableName).Append("]").ToString();
            return fqDbTable;
        }

        protected string GetSourceTableName(string dbTableName)
        {
            var fqDbTable = new StringBuilder("[").Append(SourceDB).Append("]").Append(".[dbo]").Append(".[").Append(dbTableName).Append("]").ToString();
            return fqDbTable;
        }

        protected void Initialize()
        {
            // Build Static SQL Statements
            SelectAllSQL = BuildSelectColumnSQL(ORCatalog.GetMetadataForTable(TableName));
            InsertAllSQL = BuildInsertSQL(ORCatalog.GetMetadataForTable(TableName));
            UpdateAllSQL = BuildUpdateSQL(ORCatalog.GetMetadataForTable(TableName));
        }

        private string BuildFilterRestriction(PropertyFilter filter)
        {
            StringBuilder restriction = new StringBuilder();
            var metadata = ORCatalog.GetMetadataForProperty(typeof(T), filter.PropertyName);
            restriction.Append(metadata.EscapedColumnName);
            string filterPropertyName = null;
            string upperFilterName = null;
            string lowerFilterName = null;

            if (filter.Operand == PropertyFilter.Comparator.Between)
            {
                if (filter.Group > 0)
                {
                    lowerFilterName = filter.Group + "Lower" + filter.PropertyName;
                    upperFilterName = filter.Group + "Upper" + filter.PropertyName;
                }
                else
                {
                    lowerFilterName = "Lower" + filter.PropertyName;
                    upperFilterName = "Upper" + filter.PropertyName;
                }
            }
            else
            {
                filterPropertyName = (filter.Group != 0) ? filter.Group.ToString() + filter.PropertyName : filter.PropertyName;
            }

            switch (filter.Operand)
            {
                case PropertyFilter.Comparator.Greater:
                    restriction.Append(" > @").Append(filterPropertyName);
                    break;
                case PropertyFilter.Comparator.GreaterEquals:
                    restriction.Append(" >= @").Append(filterPropertyName);
                    break;
                case PropertyFilter.Comparator.Equals:
                    restriction.Append(" = @").Append(filterPropertyName);
                    break;
                case PropertyFilter.Comparator.NotEquals:
                    restriction.Append(" <> @").Append(filterPropertyName);
                    break;
                case PropertyFilter.Comparator.LessEquals:
                    restriction.Append(" <= @").Append(filterPropertyName);
                    break;
                case PropertyFilter.Comparator.Less:
                    restriction.Append(" < @").Append(filterPropertyName);
                    break;
                case PropertyFilter.Comparator.StartsWith:
                    restriction.Append(" LIKE '").Append(filter.Value.ToString().Replace("'", "''").ToString()).Append("%'");
                    break;
                case PropertyFilter.Comparator.Like:
                    restriction.Append(" LIKE '%").Append(filter.Value.ToString().Replace("'", "''").ToString()).Append("%'");
                    break;
                case PropertyFilter.Comparator.In:
                    restriction.Append(" IN @").Append(filterPropertyName);
                    break;
                case PropertyFilter.Comparator.NotIn:
                    restriction.Append(" NOT IN @").Append(filterPropertyName);
                    break;
                case PropertyFilter.Comparator.Between:
                    restriction.Append(" BETWEEN @").Append(lowerFilterName).Append(" AND @").Append(upperFilterName);
                    break;
                default:
                    restriction.Append(" = @").Append(filterPropertyName);
                    break;
            }

            return restriction.ToString();
        }

        private string BuildInsertSQL(IList<ORMetadata> entityMetadata)
        {
            // Build SQL from Mappings
            StringBuilder insertClause = new StringBuilder("INSERT INTO ").Append(TableName).Append("(");
            StringBuilder valuesCaluse = new StringBuilder("VALUES (");
            bool first = true;
            foreach (var metadata in entityMetadata)
            {
                // Skip Identity Fields for Insert
                if (metadata.IsIdentityField) continue;
                if (!first)
                {
                    insertClause.Append(", ");
                    valuesCaluse.Append(", ");
                }
                insertClause.Append(metadata.EscapedColumnName);
                valuesCaluse.Append("@").Append(metadata.PropertyName);
                first = false;
            }
            insertClause.Append(") ");
            valuesCaluse.Append(")");

            return insertClause.Append(valuesCaluse).ToString();
        }

        private string BuildInsertSQL(IEnumerable<string> propertySet)
        {
            IList<ORMetadata> propertyMetadata = GetMetadataForProperties(propertySet);
            return BuildInsertSQL(propertyMetadata);
        }

        private string BuildSelectColumnSQL(IList<ORMetadata> entityMetadata, string alias = null)
        {
            StringBuilder result = new StringBuilder();
            foreach (var metadata in entityMetadata)
            {
                if (result.Length != 0)
                {
                    result.Append(", ");
                }
                if (alias != null)
                {
                    result.Append(alias).Append(".");
                }
                result.Append(EscapedTableName).Append(".").Append(metadata.EscapedColumnName).Append(" as ").Append(metadata.EscapedPropertyName);
            }

            return result.Insert(0, "SELECT ").ToString();
        }

        private string BuildSelectColumnSQL(IEnumerable<string> propertySet)
        {
            IList<ORMetadata> propertyMetadata = GetMetadataForProperties(propertySet);
            return BuildSelectColumnSQL(propertyMetadata);
        }

        protected string BuildSelectColumnSQL(string alias)
        {
            IList<ORMetadata> propertyMetadata = ORCatalog.GetMetadataForTable(TableName);
            return BuildSelectColumnSQL(propertyMetadata, alias);
        }

        private string BuildUpdateSQL(IList<ORMetadata> entityMetadata)
        {
            // Build SQL from Mappings
            StringBuilder sql = new StringBuilder("UPDATE ").Append(TableName);
            StringBuilder valuesCaluse = new StringBuilder();
            // Build Column Update Clause
            foreach (var metadata in entityMetadata)
            {
                // Exclude Key Columns
                if (!metadata.IsKeyField && !metadata.IsIdentityField)
                {
                    if (valuesCaluse.Length == 0)
                    {
                        valuesCaluse.Append(" SET ");
                    }
                    else
                    {
                        valuesCaluse.Append(", ");
                    }

                    valuesCaluse.Append(metadata.EscapedColumnName).Append(" = @").Append(metadata.PropertyName);
                }
            }

            // Build Where Clause from Key/Identity Columns
            //IDictionary<string, object> keyColumns = _ColumnMappings.Where(p => p.Value.KeyField).ToDictionary(p => p.Key, p => (object)p.Value.ColumnName);
            IEnumerable<ORMetadata> keyMetadata = ORCatalog.GetMetadataForTable(TableName).Where(p => p.IsKeyField || p.IsIdentityField).ToList();
            List<PropertyFilter> filters = new List<PropertyFilter>();
            foreach (var metadata in keyMetadata)
            {
                filters.Add(new PropertyFilter(metadata.PropertyName, metadata.ColumnName));
            }
            string whereClause = BuildWhereClause(filters);

            sql.Append(valuesCaluse).Append(whereClause);
            return sql.ToString();
        }

        private string BuildUpdateSQL(IEnumerable<string> propertySet)
        {
            IList<ORMetadata> propertyMetadata = GetMetadataForProperties(propertySet);
            return BuildUpdateSQL(propertyMetadata);
        }

        private IList<ORMetadata> GetMetadataForProperties(IEnumerable<string> propertySet)
        {
            // Include Only Properties Specified
            IList<ORMetadata> propertyMetadata = new List<ORMetadata>();
            foreach (var prop in propertySet)
            {
                var metadata = ORCatalog.GetMetadataForProperty(typeof(T), prop);
                propertyMetadata.Add(metadata);
            }

            return propertyMetadata;
        }

        private string BuildWhereClause(IEnumerable<PropertyFilter> filters = null)
        {
            StringBuilder whereClause = new StringBuilder();
            IEnumerable<int> propertyGroups = filters.Select(x => x.Group).Distinct();

            for (int i = 0; i <= propertyGroups.Count(); i++)
            {
                var groupFilters = filters.Where(x => x.Group == i);

                if (groupFilters.Count() > 0)
                {
                    int currentFilterIndex = 0;
                    if (i > 0 && whereClause.Length > 0) whereClause.Append(" OR (");

                    foreach (var filter in groupFilters)
                    {
                        // If we dont find a matching Column, ignore the parameter
                        if (ORCatalog.GetMetadataForProperty(typeof(T), filter.PropertyName) == null)
                        {
                            LogManager.Log(LogLevel.INFO, filter.PropertyName + " not found on " + typeof(T).Name);
                            throw new DataAccessException(filter.PropertyName + " not found on " + typeof(T).Name);
                        }

                        if (whereClause.Length == 0)
                        {
                            whereClause.Append(" WHERE (");
                        }
                        else
                        {
                            if (currentFilterIndex != 0) whereClause.Append(" AND ");
                        }

                        whereClause.Append(BuildFilterRestriction(filter));
                        currentFilterIndex++;
                    }
                    whereClause.Append(" ) ");
                }
            }

            return whereClause.ToString();
        }

        private string BuildOrderClause(IEnumerable<OrderFilter> filters = null)
        {
            StringBuilder orderClause = new StringBuilder();
            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    var metadata = ORCatalog.GetMetadataForProperty(typeof(T), filter.PropertyName);
                    if (metadata == null)
                    {
                        throw new DataAccessException(filter.PropertyName + " not found on " + typeof(T).Name);
                    }

                    if (orderClause.Length == 0)
                    {
                        orderClause.Append(" ORDER BY ");
                    }
                    else
                    {
                        orderClause.Append(", ");
                    }

                    orderClause.Append(metadata.EscapedColumnName);
                    if (filter.Operand == OrderFilter.Comparator.Ascending) orderClause.Append(" ASC");
                    if (filter.Operand == OrderFilter.Comparator.Descending) orderClause.Append(" DESC");
                }
            }

            return orderClause.ToString();
        }

        private DynamicParameters ToDynamicParameters(IEnumerable<PropertyFilter> filters = null)
        {
            DynamicParameters results = new DynamicParameters();
            if (filters == null) return results;

            IDictionary<string, object> dictionary = new Dictionary<string, object>();

            foreach (var pf in filters)
            {
                if (pf.Operand == PropertyFilter.Comparator.Between)
                {
                    if (pf.Group == 0)
                    {
                        dictionary.Add("Lower" + pf.PropertyName, pf.Value);
                        dictionary.Add("Upper" + pf.PropertyName, pf.Value2);
                    }
                    else
                    {
                        dictionary.Add(Convert.ToString(pf.Group) + "Lower" + pf.PropertyName, pf.Value);
                        dictionary.Add(Convert.ToString(pf.Group) + "Upper" + pf.PropertyName, pf.Value2);
                    }
                }
                else
                {
                    if (pf.Group != 0)
                    {
                        dictionary.Add(Convert.ToString(pf.Group) + pf.PropertyName, pf.Value);
                    }
                    else
                    {
                        dictionary.Add(pf.PropertyName, pf.Value);
                    }
                }
            }
            results.AddDynamicParams(dictionary);
            return results;
        }

        #region Posty Notices DAO
        public IEnumerable<T> GetPnAdministrator(UnitOfWork uow, string BranchID, PropertyFilter filter = null, IEnumerable<OrderFilter> order = null)
        {
            var filters = new List<PropertyFilter>() { filter };
            var selectSql = SelectAllSQL.Replace("SELECT", "Select * from UWBranch where BranchID = '" + BranchID  + "'");
            return GetList<T>(uow, selectSql, filters, order);
        }
        #endregion

    }
    #endregion

    #region SingletonDAO
    //public abstract class SingletonDAO<T> : BaseDAO<T> where T : BaseEntity
    //{
    //    protected SingletonDAO(string tableName, bool identityIsPartOfKey = false) : base(tableName, identityIsPartOfKey) { }

    //    // Loads the Singleton into the Global Cache at Company Level
    //    public void Initialize(UnitOfWork uow)
    //    {
    //        var instance = GetInstance(uow);
    //        if (instance == null)
    //        {
    //            instance = GetList(uow).SingleOrDefault();
    //            if (instance == null)
    //            {
    //                LogManager.Log(LogLevel.WARN, "No " + typeof(T).Name + " for Context [" + uow.Context + "] was cached");
    //            }
    //            else
    //            {
    //                WriteToCache(uow, instance);
    //                LogManager.Log(LogLevel.INFO, "Caching " + typeof(T).Name + " for Context [" + uow.Context + "]");
    //            }
    //        }
    //    }

    //    // Retrieve the Singleton from the Global Cache at Company Level
    //    public T GetInstance(UnitOfWork uow)
    //    {
    //        string companyKey = GetContextCacheKey(uow);
    //        var instance = CacheManager.GlobalCache.GetValue(companyKey) as T;
    //        return instance;
    //    }

    //    // Write-thru to the DB and load cache
    //    public void Create(UnitOfWork uow, T entity)
    //    {
    //        if (GetInstance(uow) != null)
    //        {
    //            throw new DataAccessException(String.Format("Only one Entity of type [{0}] is allowed", entity.GetType().Name));
    //        }

    //        base.Create(uow, entity);
    //        WriteToCache(uow, entity);
    //    }

    //    // Write-thru to the DB and refresh cache
    //    public bool Update(UnitOfWork uow, T entity)
    //    {
    //        var result = base.Update(uow, entity);
    //        WriteToCache(uow, entity);
    //        return result;
    //    }

    //    protected void WriteToCache(UnitOfWork uow, T entity)
    //    {
    //        string companyKey = GetContextCacheKey(uow);
    //        CacheManager.GlobalCache.SetValue(companyKey, entity);
    //    }

    //    protected string GetContextCacheKey(UnitOfWork uow)
    //    {
    //        //string sourceContext = Response4W.GetSourceDatabase(uow.Context, TableName);
    //        //Replaces the original method which allows for specifying a different database to query if needed
    //        string sourceContext = _DB;
    //        string cacheKey = new StringBuilder(sourceContext).Append(":").Append(TableName).ToString();
    //        return cacheKey;
    //    }
    //}
    #endregion

    #region ORMetaData
    public class ORMetadata
    {
        public string TableName { get; internal set; }
        public string ColumnName { get; internal set; }
        public string EntityName { get; internal set; }
        public string PropertyName { get; internal set; }
        public bool IsIdentityField { get; internal set; }
        public bool IsKeyField { get; internal set; }

        public string TableColumnFormat
        {
            get { return new StringBuilder(TableName).Append(".").Append(ColumnName).ToString(); }
        }

        public string EscapedColumnName
        {
            get { return new StringBuilder("[").Append(ColumnName).Append("]").ToString(); }
        }

        public string EscapedPropertyName
        {
            get { return new StringBuilder("[").Append(PropertyName).Append("]").ToString(); }
        }

        public ORMetadata Relationship { get; internal set; }
    }
    #endregion

    public class ORTableRelation
    {
        public string Table { get; set; }
        public string Property { get; set; }
        public string RelationProperty { get; set; }
    }

    public class ORJoinMetadata
    {
        public string EntityName { get; set; }
        public List<ORTableRelation> RelatedTables { get; set; }
    }

    #region ORCatalog
    public static class ORCatalog
    {
        private static IList<ORMetadata> _Catalog = new List<ORMetadata>();
        private static IList<ORJoinMetadata> _JoinCatalog = new List<ORJoinMetadata>();

        public static void AddJoinMetadata(ORJoinMetadata metadata)
        {
            _JoinCatalog.Add(metadata);
        }

        public static ORJoinMetadata GetRelationalMetadataForEntity(string entityName)
        {
            return _JoinCatalog.Where(x => x.EntityName == entityName).SingleOrDefault();
        }

        public static void AddMetadata(ORMetadata metadata)
        {
            _Catalog.Add(metadata);
        }

        public static IList<ORMetadata> GetAllMetadata()
        {
            return _Catalog;
        }

        public static IList<ORMetadata> GetMetadataForTable(string tableName)
        {
            var results = _Catalog.Where(p => p.TableName.Equals(tableName)).ToList();
            return results;
        }

        public static string GetEntityNameFromTable(string tableName)
        {
            var result = _Catalog.First(x => x.TableName == tableName).EntityName;
            return result;
        }

        public static IList<ORMetadata> GetMetadataForEntity(string entityName)
        {
            var results = _Catalog.Where(p => p.EntityName.Equals(entityName)).ToList();
            return results;
        }

        public static ORMetadata GetMetadataForColumn(string tableName, string columnName)
        {
            var result = _Catalog.Where(p => p.TableName.Equals(tableName) && p.ColumnName.Equals(columnName)).SingleOrDefault();
            return result;
        }

        public static ORMetadata GetMetadataForProperty(Type type, string propertyName)
        {
            return GetMetadataForProperty(type.Name, propertyName);
        }

        public static ORMetadata GetMetadataForProperty(string entityName, string propertyName)
        {
            ORMetadata result = null;
            try
            {
                result = _Catalog.Where(p => p.EntityName.Equals(entityName) && p.PropertyName.Equals(propertyName)).SingleOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
            return result;
        }

        public static void Validate()
        {
            //Check for missing identities, key columns, etc...
            IList<string> tables = GetAllMetadata().Select(p => p.TableName).Distinct().ToList();
            //LogManager.Log(LogLevel.INFO, "Number of Database Tables:  " + tables.Count);
            foreach (var table in tables)
            {
                var metadata = GetMetadataForTable(table);
                int identityCount = metadata.Where(p => p.IsIdentityField).Count();
                int keyCount = metadata.Where(p => p.IsKeyField).Count();
                if (identityCount == 0)
                {
                    //LogManager.Log(LogLevel.ERROR, "Table [{0}] does not have an Identity Column", table);
                }
                if (keyCount == 0)
                {
                    //LogManager.Log(LogLevel.WARN, "Table [{0}] does not have an Key Column", table);
                }
                //LogManager.Log(LogLevel.INFO, "Table [{0}] has {1} columns mapped", table, metadata.Count());
            }
        }
    }
    #endregion

    #region DAOFactory
    public static class DAOFactory
    {
        private static Dictionary<string, object> _DAOClasses = new Dictionary<string, object>();

        public static void Initialize()
        {
            LogManager.Log(LogLevel.INFO, "Initializing Data Access Component");
            Assembly daoAssembly = Assembly.GetExecutingAssembly();
            foreach (Type t in daoAssembly.GetTypes())
            {
                // Assuming that all Concrete DAO classes extend from BaseDAO
                if (t.BaseType != null && t.BaseType.IsGenericType && !t.IsAbstract)
                {
                    Type entityType = t.BaseType.GenericTypeArguments[0];
                    _DAOClasses.Add(entityType.Name, Activator.CreateInstance(t));
                    //LogManager.Log(LogLevel.INFO, "Initialized " + t.FullName);
                }
            }

            ORCatalog.Validate();
        }

        public static T GetDAO<T>()
        {
            Type entityType = typeof(T).BaseType.GenericTypeArguments[0];
            T result = (T)_DAOClasses[entityType.Name];
            return result;
        }

        public static BaseDAO<T> GetDAOForEntity<T>() where T : BaseEntity
        {
            BaseDAO<T> result = _DAOClasses[typeof(T).Name] as BaseDAO<T>;
            return result;
        }
    }
    #endregion
}

#region Dapper
namespace Dapper
{
    partial class DynamicParameters
    {
        public object GetParamValue(string name)
        {
            return parameters[Clean(name)].Value;
        }
    }
}
#endregion
