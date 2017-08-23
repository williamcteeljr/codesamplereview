using Microsoft.VisualStudio.TestTools.UnitTesting;
using PolicyTracker.Platform.Logging;
using PolicyTracker.DomainModel.Brokers;
using System.Collections.Generic;

namespace UnitTesting
{
    [TestClass]
    public class cSharpTesting : AuthenticatedUserTest
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

        
    }
}
