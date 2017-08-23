using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PolicyTracker.BusinessServices;
using System.Net;
using UnitTesting;

namespace WebAPI.Controllers.SendPostingNotice
{
    [TestClass()]
    public class PostingNoticeControllerTests : TransactionalUnitTest
    {
        #region Class Setup/Test Initialization
        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            try
            {
                Login();
            }
            catch(Exception Ex)
            {
                string Message = Ex.Message.ToString();
                string Stack = Ex.StackTrace.ToString();
                string innertStack = Ex.InnerException.ToString();
            }
        }

        [ClassCleanup]
        public static void TearDown()
        {
            Logout();
        }
        #endregion

        [TestMethod()]
        public void SendPostingNoticeTest()
        {
            //Submission Type: New, Renewal, Endorsded
            string TypeOfPayLoad = "EN";
            //Quote Id
            int RiskId = 89563;
            //Send By User Id
            int SentById = 139;
            //Optional Parameter
            int LocationId = 0;//5631;
            int AdditionalInsuredId = 10142;

            //Full Payload for NEW BUSINESS AND RENEWAL
            //string ConfirmationCode = PostingNoticeService.SendPostingNotice(TypeOfPayLoad, RiskId, SentById);

            ////Location Endorsed
            //ConfirmationCode = PostingNoticeService.SendPostingNotice(TypeOfPayLoad, RiskId, SentById, LocationId);

            ////Additional Insured Endorsed
           string ConfirmationCode = PostingNoticeService.SendPostingNotice(TypeOfPayLoad, RiskId, SentById, null, AdditionalInsuredId);

            Assert.AreNotEqual(ConfirmationCode, "0");

        }
    }
}