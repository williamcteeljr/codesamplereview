using PolicyTracker.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Policy_Tracker.Controllers
{
    public class AssistantsController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult RiskLookup()
        {
            return PartialView();
        }

        public ActionResult Payments()
        {
            return PartialView();
        }

        public ActionResult Endorsements()
        {
            return PartialView();
        }
    }
}
