using Microsoft.VisualStudio.TestTools.UnitTesting;
using PolicyTracker.BusinessServices;
using PolicyTracker.DomainModel.Common;
using PolicyTracker.DomainModel.Policy;
using System;

namespace UnitTesting.Policy
{
    [TestClass]
    public class RenewalTesting : TransactionalUnitTest
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
            ServiceLocator.PolicySvc.GenerateRenewals();
        }

        /// <summary>
        /// Premium Entity For the Base Test Risk
        /// </summary>
        protected static RiskPremiumInfo _PREMIUM_INFO = new RiskPremiumInfo()
        {
            Id = 1,
            RiskId = 1
        };

        /// <summary>
        /// Named Insured Entity for the base Test Risk
        /// </summary>
        protected static NamedInsured _NAMED_INSURED = new NamedInsured()
        {
            ControlNumber = 1,
            //AgencyId
            InsuredType = InsuredType.CORP.Value,
            CompanyName = "BRODO OF THE SHIRE LLC.",
            City = "The Shire",
            State = "GA"
        };

        /// <summary>
        /// Missing Fields Needed to complete per test include...
        ///     ProductLine,
        ///     Prefix,
        ///     PolicyNumber,
        ///     PolicySuffix,
        ///     Status,
        ///     AgencyId,
        ///     AgentId
        ///     Purpose Of Use (If Applicable),
        ///     Airport (If Applicable)
        /// </summary>
        protected static Risk _RISK = new Risk() 
        { 
            Id = 1,
            ControlNumber = 1,
            CreatedDate = DateTime.Now,
            UnderwriterId = 1,
            ImageRightId = "IR01",
            UnderwriterAssistantId = 2,
            RenewalUnderwriterId = 1,
            Branch = Branch.ATL.Value,
            EffectiveDate = DateTime.Now,
            ExpirationDate = DateTime.Now.AddMonths(12),

            PremiumInfo = _PREMIUM_INFO,
            NamedInsured =  _NAMED_INSURED
        };
    }
}
