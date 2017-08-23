using Microsoft.VisualStudio.TestTools.UnitTesting;
using PolicyTracker.BusinessServices;

namespace UnitTesting.Policy
{
    [TestClass]
    public class NamedInsuredMerging : TransactionalUnitTest
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
        public void DebugLogTest()
        {
            ServiceLocator.AppManagementService.MergeDuplicateNamedInsuredByPolicyNumber();
        }
    }
}
