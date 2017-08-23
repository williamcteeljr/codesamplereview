using Microsoft.VisualStudio.TestTools.UnitTesting;
using PolicyTracker.BusinessServices;
using PolicyTracker.DomainModel.Common;
using PolicyTracker.DomainModel.Policy;
using System;

namespace UnitTesting.Policy
{
    [TestClass]
    public class ReportServiceTests : TransactionalUnitTest
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
        public void CalculateMonthsBetweenDates()
        {
            var results = ServiceLocator.ReportingService.GetWorkloadStatistics("Workers Comp", 2016);
        }
    }
}
