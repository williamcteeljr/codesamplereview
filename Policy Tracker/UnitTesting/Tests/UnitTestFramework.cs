using Microsoft.VisualStudio.TestTools.UnitTesting;
using PolicyTracker.DomainModel.Security;
using PolicyTracker.Platform.Caching;
using PolicyTracker.Platform.Security;
using PolicyTracker.Platform.UOW;
using System;
using System.Web;

namespace UnitTesting
{
    [TestClass]
    public abstract class AuthenticatedUserTest
    {
        protected static UserSession _Session = null;

        public static void Login()
        {
            HttpContext.Current = new HttpContext(new HttpRequest("", "http://fakeuri.com", ""), new HttpResponse(null));
            PolicyTracker.BusinessServices.ApplicationManager.Initialize();
            var user = new User() { UserId = 38, FirstName = "Josh", LastName = "Lee" };
            UserSession newSession = new UserSession()
            {
                User = user,
                StartTime = DateTime.Now,
            };
            _Session = SessionManager.CreateSession(newSession);
        }

        public static void Logout()
        {
            //UserSession session = SessionManager.GetCurrentSession();
            //SessionManager.EndSession(session.SessionId);
            _Session = null;
        }
    }

    [TestClass]
    public abstract class TransactionalUnitTest : AuthenticatedUserTest
    {
        [TestInitialize]
        public void TestSetup()
        {
            // Mock HTTP Request
            HttpContext.Current = new HttpContext(new HttpRequest("", "http://fakeuri.com", ""), new HttpResponse(null));
            SessionManager.SetCurrentSession(_Session);

            // Mock UntiOfWorkFilter Behavior
            var session = SessionManager.GetCurrentSession();
            var uow = UnitOfWorkFactory.CreateUnitOfWork();
            uow.BeginTransaction();
            CacheManager.RequestCache.SetValue(UnitOfWorkFactory._UWBASE_CONTEXT, uow);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            // Mock UntiOfWorkFilter Behavior
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            uow.CommitTransaction();
            uow.Finish();
            CacheManager.RequestCache.RemoveValue(uow.Context);
        }
    }

    [TestClass]
    public abstract class NonTransactionalUnitTest : AuthenticatedUserTest
    {
        [TestInitialize]
        public void TestSetup()
        {
            // Mock HTTP Request
            HttpContext.Current = new HttpContext(new HttpRequest("", "http://fakeuri.com", ""), new HttpResponse(null));
            SessionManager.SetCurrentSession(_Session);

            // Mock UntiOfWorkFilter Behavior
            var session = SessionManager.GetCurrentSession();
            var uow = UnitOfWorkFactory.CreateUnitOfWork();
            //uow.BeginTransaction();
            CacheManager.RequestCache.SetValue(UnitOfWorkFactory._UWBASE_CONTEXT, uow);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            // Mock UntiOfWorkFilter Behavior
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            //uow.CommitTransaction();
            uow.Finish();
            CacheManager.RequestCache.RemoveValue(uow.Context);
        }
    }
}
