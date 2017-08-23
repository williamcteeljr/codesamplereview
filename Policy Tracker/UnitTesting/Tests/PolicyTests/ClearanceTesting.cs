using Microsoft.VisualStudio.TestTools.UnitTesting;
using PolicyTracker.BusinessServices;
using PolicyTracker.DomainModel.Common;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.DomainModel.Policy;
using System;
using System.Linq;

namespace UnitTesting.Policy
{
    [TestClass]
    public class ClearanceTesting : AuthenticatedUserTest
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
        public void RiskEntryNewNamedInsured()
        {
            // Set Control # to 0 to indicate creating a risk for a new named insured not already in the system.
            var controlNumber = 0;
            // Get a new Risk model from the Policy Service passing the control number
            var risk = ServiceLocator.RiskService.GetNewRisk(controlNumber);

            // Fetch required pick lists for data entry.
            var productLines = ServiceLocator.EntityService.GetList<ProductLine>();
            // =======================================================================

            // Mimic the input of the minimum required fields for creating a new Risk.
            risk.ImageRightId = "IR00001";
            risk.Branch = Branch.ATL.Value;
            risk.ProductLine = productLines.First().ProductLineId;

            
            risk.NamedInsured.CompanyName = "BRODO OF THE SHIRE";
            risk.NamedInsured.City = "THE SHIRE";
            risk.NamedInsured.State = "GA";

            risk.AgencyID = "SE9999";
            // =======================================================================

            ServiceLocator.PolicySvc.SaveNewRisk(risk);
        }
    }
}
