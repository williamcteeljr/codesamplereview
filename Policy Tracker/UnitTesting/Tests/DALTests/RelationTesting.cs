using Microsoft.VisualStudio.TestTools.UnitTesting;
using PolicyTracker.DataAccess;
using PolicyTracker.DataAccess.Policy;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.DomainModel.Policy;
using PolicyTracker.Platform.UOW;
using System;
using System.Linq;
using System.Collections.Generic;
using PolicyTracker.BusinessServices;

namespace UnitTesting.DAL
{
    [TestClass]
    public class GeneralTesting : NonTransactionalUnitTest
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
        public void SingleJoinTest()
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var dao = DAOFactory.GetDAO<RiskDAO>();
            var risk = dao.GetInstanceGraph<Risk, RiskPremiumInfo>(uow, (entity, relatedEntity) => { entity.PremiumInfo = relatedEntity; return entity; }, new PropertyFilter("Id", 60598));
            
            Assert.AreEqual(10000, risk.PremiumInfo.AnnualizedPremium);
        }

        [TestMethod]
        public void DoubleJoinTest()
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var dao = DAOFactory.GetDAO<RiskDAO>();
            Func<Risk, RiskPremiumInfo, RiskInstallmentInfo, Risk> map = (entity, premiums, installments) => { entity.PremiumInfo = premiums; entity.InstallmentInfo = installments; return entity; };
            var risk = dao.GetInstanceGraph<Risk, RiskPremiumInfo, RiskInstallmentInfo>(uow, map, new PropertyFilter("Id", 60598));

            Assert.AreEqual(20, risk.InstallmentInfo.EstimatedPremPerMonth);
        }

        [TestMethod]
        public void GetListGraphTest()
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var dao = DAOFactory.GetDAO<RiskDAO>();
            Func<Risk, RiskPremiumInfo, Risk> map = (entity, premiums) => { entity.PremiumInfo = premiums; return entity; };
            var risks = dao.GetListGraph<Risk, RiskPremiumInfo>(uow, map, new PropertyFilter("Branch", "DAL"));

            Assert.IsTrue(risks.Count() > 0);
        }

        [TestMethod]
        public void TestLargeDataSet()
        {
            var risks = ServiceLocator.EntityService.GetList<Risk>();

            Assert.AreEqual(1, 1);
        }
    }
}
