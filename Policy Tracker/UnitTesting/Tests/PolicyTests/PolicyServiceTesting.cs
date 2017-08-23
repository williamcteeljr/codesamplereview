using CsvHelper;
using DomainModel.Risks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PolicyTracker.BusinessServices;
using PolicyTracker.DataAccess;
using PolicyTracker.DataAccess.Policy;
using PolicyTracker.DomainModel.Brokers;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.DomainModel.Policy;
using PolicyTracker.Platform.UOW;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UnitTesting.Policy
{
    [TestClass]
    public class PolicyServiceTesting : TransactionalUnitTest
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
        public void WorkingListExport()
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var exportData = ServiceLocator.EntityService.GetList<WorkingListItem>(new PropertyFilter("Status", RiskStatus.QUOTE.Value));
            var memoryStream = new MemoryStream();
            var writer = new CsvWriter(new StreamWriter(memoryStream));
            writer.WriteRecords(exportData);

            Assert.IsTrue(1 == 1);
        }

        [TestMethod]
        public void CheckOFAC()
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var resuts = DAOFactory.GetDAO<RiskDAO>().CheckOFACSDN(uow, "AGHAJANI, DAVOOD");
            var terrorCount = resuts.Count();
        }

        [TestMethod]
        public void TestUnderwriterAutoSelectMultiNoLast()
        {
            var assignments = new List<BrokerAssignment>()
                {
                    new BrokerAssignment() { AssignmentId = 1, BrokerCode = "SE9999", UserId = 1, ProductLineId = 3, LastAssigned = false },
                    new BrokerAssignment() { AssignmentId = 2, BrokerCode = "SE9999", UserId = 2, ProductLineId = 3, LastAssigned = false },
                    new BrokerAssignment() { AssignmentId = 3, BrokerCode = "SE9999", UserId = 3, ProductLineId = 3, LastAssigned = false },
                    new BrokerAssignment() { AssignmentId = 4, BrokerCode = "SE9999", UserId = 4, ProductLineId = 3, LastAssigned = false },
                };

            BrokerAssignment assignment = null;

            var lastAssigned = assignments.FindLastIndex(x => x.LastAssigned);
            if (lastAssigned == assignments.Count()) assignment = assignments.FirstOrDefault();
            else assignment = assignments[lastAssigned + 1];

            Assert.AreEqual(1, assignment.UserId);
        }

        [TestMethod]
        public void TestUnderwriterAutoSelectMultiStandard()
        {
            var assignments = new List<BrokerAssignment>()
                {
                    new BrokerAssignment() { AssignmentId = 1, BrokerCode = "SE9999", UserId = 1, ProductLineId = 3, LastAssigned = true },
                    new BrokerAssignment() { AssignmentId = 2, BrokerCode = "SE9999", UserId = 2, ProductLineId = 3, LastAssigned = false },
                    new BrokerAssignment() { AssignmentId = 3, BrokerCode = "SE9999", UserId = 3, ProductLineId = 3, LastAssigned = false },
                    new BrokerAssignment() { AssignmentId = 4, BrokerCode = "SE9999", UserId = 4, ProductLineId = 3, LastAssigned = false },
                };

            BrokerAssignment assignment = null;

            var lastAssigned = assignments.FindLastIndex(x => x.LastAssigned);
            if (lastAssigned == assignments.Count()) assignment = assignments.FirstOrDefault();
            else assignment = assignments[lastAssigned + 1];

            Assert.AreEqual(2, assignment.UserId);
        }

        [TestMethod]
        public void TestUnderwriterAutoSelectMultiRestart()
        {
            var assignments = new List<BrokerAssignment>()
                {
                    new BrokerAssignment() { AssignmentId = 1, BrokerCode = "SE9999", UserId = 1, ProductLineId = 3, LastAssigned = false },
                    new BrokerAssignment() { AssignmentId = 2, BrokerCode = "SE9999", UserId = 2, ProductLineId = 3, LastAssigned = false },
                    new BrokerAssignment() { AssignmentId = 3, BrokerCode = "SE9999", UserId = 3, ProductLineId = 3, LastAssigned = false },
                    new BrokerAssignment() { AssignmentId = 4, BrokerCode = "SE9999", UserId = 4, ProductLineId = 3, LastAssigned = true },
                };

            BrokerAssignment assignment = null;

            var lastAssigned = assignments.FindLastIndex(x => x.LastAssigned);
            // +1 to account for 0 based index
            if ((lastAssigned + 1) == assignments.Count()) assignment = assignments.FirstOrDefault();
            else assignment = assignments[lastAssigned + 1];

            Assert.AreEqual(1, assignment.UserId);
        }

        [TestMethod]
        public void TestGenerateRenewal()
        {
            UnitOfWork uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var expiringIn90 = DAOFactory.GetDAO<RiskDAO>().GetExpiringRisks(uow);
            var problemPolicy = expiringIn90.Where(x => x.RiskId == 65783).FirstOrDefault();

            Assert.AreNotEqual(null, problemPolicy);

            ServiceLocator.PolicySvc.GenerateRenewal(problemPolicy.RiskId);
        }
    }
}
