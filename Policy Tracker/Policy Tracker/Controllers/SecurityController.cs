using PolicyTracker.Platform.Logging;
using PolicyTracker.BusinessServices;
using PolicyTracker.Filters;
using PolicyTracker.Platform.Security;
using System;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using PolicyTracker.DomainModel.Security;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.DomainModel.Policy;

namespace PolicyTracker.Controllers
{
    [UnitOfWork]
    public class SecurityController : Controller
    {
        // Sets/Checks the user session and app domain selection from the UW Base Toolbar. Will reroute to the necessary action based on the appDomain selection from the toolbar.
        public ActionResult InitializeSession()
        {
            /* UID = User Id
             * PID = Portal Id
             * STRID = System Tab Ribbion Id (aka. The specific domain area of the app they want to jump to)
             */

            var userId = string.Empty;
            var portalId = string.Empty;
            var appDomainSelection = string.Empty;

            if ((ConfigurationManager.AppSettings["Environment"] == "Local"))
            {
                //Local
                userId = ServiceLocator.EntityService.GetInstance<User>(new PropertyFilter("UserName", "wteel")).UserId.ToString();
            }
            else
            {
                //Prod
                userId = Request.QueryString["UID"] == null ? "118" : Request.QueryString["UID"];
                portalId = Request.QueryString["PID"] == null ? "1" : Request.QueryString["PID"];
                appDomainSelection = Request.QueryString["STRID"] == null ? "27" : Request.QueryString["STRID"];
            }

            try
            {
                var user = ServiceLocator.SecuritySvc.GetUser(userId);
                if (user == null) throw new Exception();

                // Check if the user has access to the App at all
                //SecurityManager.HasAccess("/PolicyTracker", SecurityManager.ResourcePrivilege.Read);

                UserSession newSession = SecurityManager.StartSession(user);
                HttpCookie cookie = new HttpCookie(SessionManager.SESSIONID_TOKEN, newSession.SessionId);
                cookie.Path = "/";
                cookie.Expires = DateTime.Now.AddDays(1);
                Response.SetCookie(cookie);

                if (SecurityManager.InRole(new[] { "Exec" }))
                    return RedirectToAction("CompanyConsole", "Console");
                else if (SecurityManager.InRole(new[] { "Branch Manager" }))
                    return RedirectToAction("BranchConsole", "Console");
                else if (SecurityManager.InRole(new[] { "Product Line Manager" }) || user.ProductLine == (int)ProductLines.WC)
                    return RedirectToAction("ProductLineMaster", "Console");
                else
                    return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                LogManager.Log(LogLevel.INFO, ex.Message);
                return PartialView("Index");
            }
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SessionExpired()
        {
            return PartialView();
        }
    }
}
