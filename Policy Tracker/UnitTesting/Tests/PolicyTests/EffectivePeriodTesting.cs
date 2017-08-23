using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTesting.Policy
{
    [TestClass]
    public class EffectivePeriodTesting : AuthenticatedUserTest
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

        /// <summary>
        /// Figures what the renewal effective/expiration should be in a normal use case with non special date cases or exceptions.
        /// </summary>
        [TestMethod]
        public void Fgiure12MonthEffectiveNoLeapYears()
        {
            var effectiveDate = DateTime.Parse("3/25/2013");
            var expirationDate = DateTime.Parse("3/25/2014");
            
            var dateDiff = expirationDate - effectiveDate;

            var renewalEffectiveDate = expirationDate;
            var renewalExpirationDate = expirationDate.AddMonths(12);

            Assert.AreEqual(DateTime.Parse("3/25/2015"), renewalExpirationDate);
        }

        /// <summary>
        /// Figures what the renewal effective/expiration should be when the expiration date is falling in a leap year.
        /// </summary>
        [TestMethod]
        public void Figure12MonthEffectivePeriodEndingInLeapYear()
        {
            var effectiveDate = DateTime.Parse("3/25/2014");
            var expirationDate = DateTime.Parse("3/25/2015");

            var dateDiff = expirationDate - effectiveDate;

            var renewalEffectiveDate = expirationDate;
            var renewalExpirationDate = expirationDate.AddMonths(12);

            Assert.AreEqual(DateTime.Parse("3/25/2016"), renewalExpirationDate);
        }

        /// <summary>
        /// Figures what the renewal effective/expiration should be when the effecitve date falls in a leap year.
        /// </summary>
        [TestMethod]
        public void Figure12MonthEffectivePeriodStartingInLeapYear()
        {
            var effectiveDate = DateTime.Parse("3/25/2016");
            var expirationDate = DateTime.Parse("3/25/2017");

            var dateDiff = expirationDate - effectiveDate;

            var renewalEffectiveDate = expirationDate;
            var renewalExpirationDate = expirationDate.AddMonths(12);

            Assert.AreEqual(DateTime.Parse("3/25/2018"), renewalExpirationDate);
        }

        /// <summary>
        /// Testing the setting of effective/Expiration on a renewal when the effective date in Feb 29th. The following year doesn't have a Feb 29th so it should set the effective date of the 
        /// renewal to Feb 28th. From the on the new effective/expiration should remain Feb 28th.
        /// </summary>
        [TestMethod]
        public void Figure12MonthPeriodStartingFebruary29()
        {
            var effectiveDate = DateTime.Parse("2/29/2016");
            var expirationDate = effectiveDate.AddYears(1);
            
            var renewalEffectiveDate = expirationDate;
            var renewalExpirationDate = expirationDate.AddMonths(12);

            Assert.AreEqual(expirationDate, renewalEffectiveDate);
            Assert.AreEqual(DateTime.Parse("2/28/2018"), renewalExpirationDate);

            renewalEffectiveDate = renewalExpirationDate;
            renewalExpirationDate = renewalEffectiveDate.AddMonths(12);

            Assert.AreEqual(DateTime.Parse("2/28/2018"), renewalEffectiveDate);
            Assert.AreEqual(DateTime.Parse("2/28/2019"), renewalExpirationDate);
        }

        [TestMethod]
        public void CalculateMonthsBetweenDates()
        {
            var startDate = DateTime.Parse("2015/01/01");
            var endDate = DateTime.Parse("2016/01/01");

            Assert.AreEqual(Math.Round(endDate.Subtract(startDate).Days / (365.25 / 12)), 12);

            startDate = DateTime.Parse("2015/01/01");
            endDate = DateTime.Parse("2015/04/12");

            Assert.AreEqual(Math.Round(endDate.Subtract(startDate).Days / (365.25 / 12)), 3);

            startDate = DateTime.Parse("2015/01/01");
            endDate = DateTime.Parse("2015/04/20");

            Assert.AreEqual(Math.Round(endDate.Subtract(startDate).Days / (365.25 / 12)), 4);
        }
    }
}
