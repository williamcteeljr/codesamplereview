using PolicyTracker.Controllers;
using PolicyTracker.BusinessServices;
using PolicyTracker.DomainModel.Common;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.DomainModel.Policy;
using PolicyTracker.DomainModel.Security;
using PolicyTracker.Platform.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Policy_Tracker.Controllers
{
    public class ConsoleController : BaseController
    {
        public ActionResult ProductLineMaster()
        {
            return View();
        }
        public ActionResult ProductLineConsole()
        {
            return View();
        }

        public ActionResult UnderwriterConsole()
        {
            return View();
        }

        public ActionResult BranchConsole()
        {
            return View();
        }

        public ActionResult CompanyConsole()
        {
            return View();
        }
    }
}
