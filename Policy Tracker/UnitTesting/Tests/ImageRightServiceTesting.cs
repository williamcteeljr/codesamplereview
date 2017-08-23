using Microsoft.VisualStudio.TestTools.UnitTesting;
using PolicyTracker.Platform.Logging;
using ServiceGateway.ImageRight;
using ServiceGateway.ImageRightServiceReference;
using System.Linq;

namespace UnitTesting
{
    [TestClass]
    public class ImageRightServiceTesting : AuthenticatedUserTest
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
        public void TestGetDrawer()
        {
            Drawer[] drawer = ImageRightServiceGateway.GetDrawers();
        }

        [TestMethod]
        public void TestGetFile()
        {
            Drawer drawer = ImageRightServiceGateway.GetDrawers().Where(x => x.Name == "Quote").FirstOrDefault();
            File file = ImageRightServiceGateway.GetFile(drawer, "265531OR", "Atlanta Agriculture");
        }

        [TestMethod]
        public void TestCreateFile()
        {
            Drawer drawer = ImageRightServiceGateway.GetDrawers().Where(x => x.Name == "Quote").FirstOrDefault();
            FileRef file = ImageRightServiceGateway.CreateFile(drawer);
        }
    }
}
