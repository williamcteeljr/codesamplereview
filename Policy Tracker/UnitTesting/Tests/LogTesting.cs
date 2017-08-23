using Microsoft.VisualStudio.TestTools.UnitTesting;
using PolicyTracker.BusinessServices;
using PolicyTracker.DomainModel.Policy;
using PolicyTracker.Platform.Logging;
using System;

namespace UnitTesting
{
    [TestClass]
    public class GeneralTesting : AuthenticatedUserTest
    {
        #region Class Setup/Test Initialization
        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            Login();
        }

        [ClassCleanup]
        public static void TearDown()
        {
            Logout();
        }
        #endregion

        [TestMethod]
        public void InfoLogTest()
        {
            var diff = DateTime.Now - DateTime.Parse("3-1-2015");
            LogManager.Log(LogLevel.INFO, "INFO TEST LOG ENTRY");
        }

        [TestMethod]
        public void DebugLogTest()
        {
            LogManager.Log(LogLevel.DEBUG, "DEBUG TEST LOG ENTRY");
        }

        [TestMethod]
        public void WarningLogTest()
        {
            LogManager.Log(LogLevel.WARN, "WARNING TEST LOG ENTRY");
        }

        [TestMethod]
        public void ErrorLogTest()
        {
            LogManager.Log(LogLevel.ERROR, "ERROR TEST LOG ENTRY");
        }

        [TestMethod]
        public void FatalLogTest()
        {
            LogManager.Log(LogLevel.FATAL, "FATAL TEST LOG ENTRY");
        }
    }
}
