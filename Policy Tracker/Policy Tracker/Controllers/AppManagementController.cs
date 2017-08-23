using PolicyTracker.BusinessServices;
using PolicyTracker.DomainModel.Brokers;
using PolicyTracker.DomainModel.Common;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.DomainModel.Policy;
using PolicyTracker.DomainModel.Security;
using PolicyTracker.Platform.Caching;
using PolicyTracker.Platform.Security;
using PolicyTracker.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Text;
using DomainModel.View_Models;
using PolicyTracker.Platform.Logging;

namespace PolicyTracker.Controllers
{
    public class AppManagementController : BaseController
    {
        public ActionResult Index()
        {
            return PartialView();
        }

        public ActionResult DailyReports()
        {
            return PartialView();
        }
        public ActionResult Security()
        {
            return PartialView();
        }
        
        public ActionResult RiskNamedInsuredSwitch()
        {
            return PartialView(new AlterNamedInsuredViewModel());
        }

        public ActionResult SessionManager()
        {
            ViewBag.Sessions = CacheManager.SessionCache.Sessions;
            return PartialView();
        }

        public HttpStatusCodeResult ClearSession(string id)
        {
            var result = new HttpStatusCodeResult(200);
            try
            {
                Platform.Security.SessionManager.EndSession(id);
            }
            catch (Exception ex)
            {
                result = new HttpStatusCodeResult(400);
                LogManager.Log(LogLevel.ERROR, ex.Message);
            }
            return result;
        }

        public ActionResult MonthlyBudgets()
        {
            return PartialView();
        }

        public void MonthlyBudgetViewData()
        {
            ViewBag.ProductLines = ServiceLocator.EntityService.GetList<ProductLine>();
            ViewBag.Branches = StringEnum.GetAll<Branch>();
            ViewBag.Months = StringEnum.GetAll<Month>();
            ViewBag.Years = new[] { 2013, 2014, 2015, 2016, 2017, 2018, 2019, 2020 };
            
            var startYear = DateTime.Now.AddYears(-2).Year;
            var years = new List<int>();
            for (int i = startYear; i < startYear + 10; i++)
            {
                years.Add(i);
            }
            ViewBag.Years = years;
        }

        public ActionResult EditMonthlyBudget(int budgetId = 0)
        {
            var budget = new MonthlyBudget();
            if (budgetId != 0) budget = ServiceLocator.EntityService.GetInstance<MonthlyBudget>(new PropertyFilter("BudgetId", budgetId));
            MonthlyBudgetViewData();
            return PartialView(budget);
        }

        [HttpPost]
        [UseTransaction]
        public ActionResult EditMonthlyBudget(MonthlyBudget entity)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    entity = BindRequestToEntity<MonthlyBudget>(entity, new PropertyFilter("BudgetId", entity.BudgetId));
                    ServiceLocator.AppManagementService.SaveMonthlyBudget(entity);
                    ModelState.Clear();
                }
                catch (ValidationRulesException ex)
                {
                    ProcessValidationException(ex);
                }
            }
            MonthlyBudgetViewData();
            return PartialView(entity);
        }

        public ActionResult BrokerAssignments()
        {
            return PartialView();
        }

        public ActionResult EditBrokerAssignment(int assignmentId = 0)
        {
            var brokerAssignment = new BrokerAssignment();
            if (assignmentId != 0) brokerAssignment = ServiceLocator.EntityService.GetInstance<BrokerAssignment>(new PropertyFilter("AssignmentId", assignmentId));
            ViewBag.Users = ServiceLocator.EntityService.GetList<User>();
            ViewBag.ProductLines = ServiceLocator.EntityService.GetList<ProductLine>();
            return PartialView(brokerAssignment);
        }

        [HttpPost]
        [UseTransaction]
        public ActionResult EditBrokerAssignment(BrokerAssignment entity)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    entity = BindRequestToEntity<BrokerAssignment>(entity, new PropertyFilter("AssignmentId", entity.AssignmentId));
                    ServiceLocator.AppManagementService.SaveBrokerAssignment(entity);
                    ModelState.Clear();
                }
                catch (ValidationRulesException ex)
                {
                    ProcessValidationException(ex);
                }
            }


            ViewBag.Users = ServiceLocator.EntityService.GetList<User>();
            ViewBag.ProductLines = ServiceLocator.EntityService.GetList<ProductLine>();

            return PartialView(entity);
        }

        public ActionResult Products()
        {
            return PartialView();
        }

        public ActionResult EditProduct(int productId = 0)
        {
            var product = new Product();
            if (productId != 0) product = ServiceLocator.EntityService.GetInstance<Product>(new PropertyFilter("ProductId", productId));
            return PartialView(product);
        }

        [HttpPost]
        [UseTransaction]
        public ActionResult EditProduct(Product entity)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    entity = BindRequestToEntity<Product>(entity, new PropertyFilter("ProductId", entity.ProductId));
                    ServiceLocator.AppManagementService.SaveProduct(entity);
                    ModelState.Clear();
                }
                catch (ValidationRulesException ex)
                {
                    ProcessValidationException(ex);
                }
            }

            return PartialView(entity);
        }

        public ActionResult PurposesOfUse()
        {
            return PartialView();
        }

        public ActionResult EditPurposeOfUse(int purposeId = 0)
        {
            var purpose = new PurposeOfUse();
            if (purposeId != 0) purpose = ServiceLocator.EntityService.GetInstance<PurposeOfUse>(new PropertyFilter("Id", purposeId));
            return PartialView(purpose);
        }

        [HttpPost]
        [UseTransaction]
        public ActionResult EditPurposeOfUse(PurposeOfUse entity)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    entity = BindRequestToEntity<PurposeOfUse>(entity, new PropertyFilter("Id", entity.Id));
                    ServiceLocator.AppManagementService.SavePurposeOfUse(entity);
                    ModelState.Clear();
                }
                catch (ValidationRulesException ex)
                {
                    ProcessValidationException(ex);
                }
            }

            return PartialView(entity);
        }

        public ActionResult Reasons()
        {
            return PartialView();
        }

        public ActionResult EditReason(int reasonId = 0)
        {
            var reason = new StatusReason();
            if (reasonId != 0) reason = ServiceLocator.EntityService.GetInstance<StatusReason>(new PropertyFilter("Id", reasonId));
            ViewBag.Statuses = StringEnum.GetAll<RiskStatus>().ToList();
            return PartialView(reason);
        }

        [HttpPost]
        [UseTransaction]
        public ActionResult EditReason(StatusReason entity)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    entity = BindRequestToEntity<StatusReason>(entity, new PropertyFilter("Id", entity.Id));
                    ServiceLocator.AppManagementService.SaveReason(entity);
                    ModelState.Clear();
                }
                catch (ValidationRulesException ex)
                {
                    ProcessValidationException(ex);
                }
            }

            ViewBag.Statuses = StringEnum.GetAll<RiskStatus>().ToList();

            return PartialView(entity);
        }

        /// <summary>
        /// View the latest Error, Performance, and App logs
        /// </summary>
        /// <returns></returns>
        public ActionResult Logs()
        {
            var dirPath = Server.MapPath(String.Empty);
            var virtualPath = System.Web.HttpRuntime.AppDomainAppVirtualPath;
            var physicalPath = System.Web.Hosting.HostingEnvironment.MapPath(virtualPath);

            var appLogs = Directory.GetFiles(physicalPath, "PolicyTracker.*");
            var errorLogs = Directory.GetFiles(physicalPath, "PolicyTracker-Errors*");
            var performanceLogs = Directory.GetFiles(physicalPath, "PolicyTracker-Performance*");

            List<object> appLogList = new List<object>();

            foreach (var file in appLogs)
            {
                var pos = file.LastIndexOf("\\");
                appLogList.Add(new { Name = "", Path = file });
            }

            ViewBag.AppLogs = appLogs;
            ViewBag.ErrorLogs = errorLogs;
            ViewBag.PerformanceLogs = performanceLogs;

            return View();
        }

        public ActionResult GetLogFile(string file)
        {
            var virtualPath = System.Web.HttpRuntime.AppDomainAppVirtualPath;
            var physicalPath = System.Web.Hosting.HostingEnvironment.MapPath(virtualPath);

            var pos = file.LastIndexOf("\\");
            if (file.Last() == '\\') pos += 1;
            var fileName = file.Substring(pos, file.Length - pos);
            var filePath = physicalPath + fileName;
            string content = String.Empty;
            string test = String.Empty;

            try
            {
                test = System.IO.File.ReadAllText(filePath);
                using (var stream = new StreamReader(filePath))
                {
                    content = stream.ReadToEnd();
                }
            }
            catch (Exception exc)
            {
                LogManager.Log(LogLevel.ERROR, exc.Message);
                return Content("Error Retrieving file Content\n " + exc.Message);
            }

            return Content(test);
        }

        public ActionResult SecurityResourceForm(int id = 0)
        {
            var resource = new SecurityResource();
            if (id != 0) resource = ServiceLocator.EntityService.GetInstance<SecurityResource>(new PropertyFilter("Id", id));
            return PartialView(resource);
        }

        [HttpPost]
        [UseTransaction]
        public ActionResult SecurityResourceForm(SecurityResource entity)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    entity = BindRequestToEntity<SecurityResource>(entity, new PropertyFilter("Id", entity.Id));
                    ServiceLocator.AppManagementService.SaveSecurityResource(entity);
                    ModelState.Clear();
                }
                catch (ValidationRulesException ex)
                {
                    ProcessValidationException(ex);
                }
            }

            return PartialView(entity);
        }

        public ActionResult SecurityAccessForm(int id = 0)
        {
            var access = new SecurityAccess();
            if (id != 0) access = ServiceLocator.EntityService.GetInstance<SecurityAccess>(new PropertyFilter("Id", id));
            ViewBag.Roles = ServiceLocator.EntityService.GetList<SecurityGroupRole>();
            ViewBag.Resources = ServiceLocator.EntityService.GetList<SecurityResource>();
            return PartialView(access);
        }

        [HttpPost]
        [UseTransaction]
        public ActionResult SecurityAccessForm(SecurityAccess entity)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    entity = BindRequestToEntity<SecurityAccess>(entity, new PropertyFilter("Id", entity.Id));
                    ServiceLocator.AppManagementService.SaveSecurityAccess(entity);
                    ModelState.Clear();
                }
                catch (ValidationRulesException ex)
                {
                    ProcessValidationException(ex);
                }
            }
            ViewBag.Roles = ServiceLocator.EntityService.GetList<SecurityGroupRole>();
            ViewBag.Resources = ServiceLocator.EntityService.GetList<SecurityResource>();
            return PartialView(entity);
        }
    }
}
