using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using PolicyTracker.Platform.Security;
using PolicyTracker.Platform.Caching;
using PolicyTracker.Platform.Logging;

namespace PolicyTracker.Platform.UOW
{
    public class UnitOfWork
    {
        internal UnitOfWork(IDbConnection con, string ctx)
        {
            Connection = con;
            Context = ctx;
            ContextCount = 1;
        }

        public string Context { get; private set; }
        internal int ContextCount { get; set; }
        public IDbConnection Connection { get; internal set; }
        public IDbTransaction Transaction { get; internal set; }
        private int TransactionCount { get; set; }

        public bool IsInTransaction
        {
            get { return TransactionCount > 0; }
        }

        public bool Finish()
        {
            ContextCount--;
            if (ContextCount == 0)
            {
                Connection.Close();
            }
            return (ContextCount == 0);
        }

        public void BeginTransaction()
        {
            Transaction = Connection.BeginTransaction();
            TransactionCount++;
        }

        public void RollBackTransaction()
        {
            Transaction.Rollback();
            TransactionCount = 0;
            Connection.Close();
        }

        public bool CommitTransaction()
        {
            if (TransactionCount == 0)
            {
                //throw new DataAccessException("There is not an active Transaction");
                return true;
            }

            if (TransactionCount > 1) return true;

            try
            {
                Transaction.Commit();
                Connection.Close();
                TransactionCount--;
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Log(LogLevel.FATAL, "The active transaction could not be committed");
                LogManager.Log(LogLevel.FATAL, ex.ToString());
                Transaction.Rollback();
                return false;
            }
            finally
            {
                //ReleaseCurrentTransaction();
            }
        }
    }

    public static class UnitOfWorkFactory
    {
        public static readonly string _POLICYTRACKER_CONTEXT = "PolicyTracker";
        public static readonly string _UWBASE_CONTEXT = "UW_Base_App";
        public static readonly string _BROKER_CONTEXT = "BrokerDB";
        public static readonly string _BLUEBOOK_CONTEXT = "BlueBook";

        //public static UnitOfWork CreateUnitOfWork(UserSession session)
        //{
        //    return CreateUnitOfWork(session.CompanyDatabase);
        //}

        public static UnitOfWork GetActiveUnitOfWork(string context = null)
        {
            //UserSession session = SessionManager.GetCurrentSession();
            if (context == null) context = UnitOfWorkFactory._UWBASE_CONTEXT;
            UnitOfWork uow = CacheManager.RequestCache.GetValue(context) as UnitOfWork;
            return uow;
        }

        public static UnitOfWork CreateUnitOfWork(string context = null)
        {
            // Determine the Context (use Default if null)
            if (context == null) context = _UWBASE_CONTEXT;

            // Get from Request Cache if available
            UnitOfWork uow = CacheManager.RequestCache.GetValue(context) as UnitOfWork;
            if (uow != null)
            {
                //throw new ApplicationException("There is already an Unit of Work for Context [" + context + "]");
                //uow.ContextCount++;
                return uow;
            }

            // Get Connection from Environment Configuration
            string environment = ConfigurationManager.AppSettings["Environment"];
            string connectionString = ConfigurationManager.ConnectionStrings["DB"].ConnectionString;
            //connectionString = String.Format(connectionString, "sa", "Response1");
            SqlConnection connection = new SqlConnection(connectionString);
            //LogManager.Log(LogLevel.INFO, "Retrieving Connection from Pool (Context = " + context + ")");
            connection.Open();

            // Set Connection Context [OBSOLETE: Prepending the Tablenames with DB names makes this un-necessary]
            //SqlCommand databaseCommand = new SqlCommand("USE " + context, connection);
            //databaseCommand.ExecuteNonQuery();

            // Return new Unit of Work
            uow = new UnitOfWork(connection, context);
            return uow;
        }
    }
}
