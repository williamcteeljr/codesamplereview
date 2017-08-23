using PolicyTracker.BusinessServices;
using PolicyTracker.DomainModel.Common;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.DomainModel.Policy;
using PolicyTracker.DomainModel.Security;
using PolicyTracker.DTO;
using PolicyTracker.Filters;
using PolicyTracker.Platform.Security;
using PolicyTracker.Utilities;
using System;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

namespace PolicyTracker.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult UserWorkingList()
        {
            return View("Index");
        }

        public ActionResult WorkingList(string gridConfig = null)
        {
            ViewBag.UserName = SessionManager.GetCurrentSession().User.Name;
            ViewBag.UserId = SessionManager.GetCurrentSession().User.UserId;
            ViewBag.UserGridConfig = gridConfig;
            ViewBag.IsUA = SecurityManager.InRole(new[] { ConfigurationManager.AppSettings["AssistantRoleName"] });
            ViewBag.Underwriters = ServiceLocator.RiskService.GetUnderwriters();
            return PartialView();
        }

        public ActionResult GridConfigList()
        {
            ViewBag.GridConfigs = ServiceLocator.EntityService.GetList<WorkingListGridConfig>(new PropertyFilter("UserId", SessionManager.GetCurrentSession().User.UserId));
            return PartialView();
        }

        #region Working List Custom User Filters
        [HttpGet]
        public ActionResult UserFilters()
        {
            var filters = ServiceLocator.EntityService.GetList<UserFilter>(new PropertyFilter("UserId", SessionManager.GetCurrentSession().User.UserId));

            foreach (var f in filters)
            {
                if (f.MonthFilter > 0)
                {
                    var expStatements = f.Expression.Split(new[] { " AND ", " And ", " and "  }, StringSplitOptions.None);
                    var statements = expStatements.Where(x => !x.Contains("[EffectiveDate]")).ToList();

                    var month = DateTime.Now.AddMonths(f.MonthFilter - 1).Month;
                    var firstDayOfMonth = new DateTime(DateTime.Now.Year, month, 1);
                    var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                    statements.Add("[EffectiveDate] >= #" + firstDayOfMonth.ToShortDateString() + "#");
                    statements.Add("[EffectiveDate] <= #" + lastDayOfMonth.ToShortDateString() + "#");

                    f.Expression = String.Join(" AND ", statements.ToArray());
                }
                else if (f.MonthRangeFilter > 0)
                {
                    var expStatements = f.Expression.Split(new[] { "AND" }, StringSplitOptions.None);
                }
                
            }

            ViewBag.MyFilters = filters;
            return PartialView();
        }

        [HttpGet]
        public ActionResult SaveFilterSettings(string exp)
        {
            var filter = new UserFilter();
            filter.UserId = SessionManager.GetCurrentSession().User.UserId;
            filter.Expression = exp;
            return PartialView(filter);
        }

        [HttpPost]
        [UseTransaction]
        public ActionResult SaveFilterSettings(UserFilter entity)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //entity = BindRequestToEntity<Aircraft>(entity, new PropertyFilter("Id", entity.Id));
                    ServiceLocator.SystemSvc.SaveUserFilter(entity);
                    ModelState.Clear();
                }
                catch (ValidationRulesException ex)
                {
                    ProcessValidationException(ex);
                }
            }

            return PartialView(entity);
        }

        [HttpDelete]
        [UseTransaction]
        public ActionResult DeleteFilterSetting(int userId, string filterName)
        {
            ServiceLocator.SystemSvc.DeleteUserFilter(userId, filterName);
            ViewBag.MyFilters = ServiceLocator.EntityService.GetList<UserFilter>(new PropertyFilter("UserId", SessionManager.GetCurrentSession().User.UserId));
            return PartialView("UserFilters");
        }
        #endregion

        #region Quick Assignment Popup
        public ActionResult QuickAssign(int id)
        {
            var risk = ServiceLocator.EntityService.GetInstance<Risk>(new PropertyFilter("Id", id));
            var model = new UnderwriterQuickAssign()
            {
                Id = risk.Id,
                UnderwriterId = risk.UnderwriterId,
                UnderwriterAssistantId = risk.UnderwriterAssistantId
            };
            ViewBag.Underwriters = ServiceLocator.RiskService.GetUnderwriters();
            ViewBag.Assistants = ServiceLocator.RiskService.GetUnderwritingAssistants();

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult QuickAssign(UnderwriterQuickAssign model)
        {
            string branch = null;

            if (ModelState.IsValid)
            {
                try
                {
                    var risk = ServiceLocator.RiskService.UpdateUnderwriterAssignment(model.Id, model.UnderwriterId, model.UnderwriterAssistantId);
                    branch = risk.Branch;
                    ModelState.Clear();
                }
                catch (ValidationRulesException ex)
                {
                    ProcessValidationException(ex);
                }
            }

            ViewBag.Underwriters = ServiceLocator.EntityService.GetList<User>(new PropertyFilter("BranchID", branch));

            return PartialView(model);
        }
        #endregion

        public ActionResult GridConfig()
        {
            return PartialView("~/Views/Home/WorkingListConfig/Index.cshtml");
        }

        public ActionResult ConfigSelection()
        {
            ViewBag.Configurations = ServiceLocator.EntityService.GetList<WorkingListGridConfig>(new PropertyFilter("UserId", SessionManager.GetCurrentSession().User.UserId));
            return PartialView("~/Views/Home/WorkingListConfig/ConfigSelection.cshtml");
        }

        public ActionResult GridConfigForm(int configId = 0)
        {
            var model = new WorkingListGridConfig() { UserId = SessionManager.GetCurrentSession().User.UserId };
            if (configId != 0) model = ServiceLocator.EntityService.GetInstance<WorkingListGridConfig>(new PropertyFilter("ConfigId", configId));

            var availableColumns = StringEnum.GetAll<WorkingListGridColumn>().ToList();
            if (model.GridColumns != null)
                availableColumns = availableColumns.Where(x => !model.GridColumns.Contains(x.Value)).ToList();
            ViewBag.AvailableColumns = availableColumns;

            ModelState.Clear();

            return PartialView("~/Views/Home/WorkingListConfig/ConfigForm.cshtml", model);
        }

        [HttpPost]
        public ActionResult GridConfigForm(WorkingListGridConfig entity)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ServiceLocator.WorkingListGridSvc.SaveConfig(entity);
                    ModelState.Clear();
                    entity = new WorkingListGridConfig() { UserId = SessionManager.GetCurrentSession().User.UserId };
                }
                catch (ValidationRulesException ex)
                {
                    ProcessValidationException(ex);
                }
            }
            var availableColumns = StringEnum.GetAll<WorkingListGridColumn>().ToList();
            if (entity.GridColumns != null)
                availableColumns = availableColumns.Where(x => !entity.GridColumns.Contains(x.Value)).ToList();
            ViewBag.AvailableColumns = availableColumns;
            return PartialView("~/Views/Home/WorkingListConfig/ConfigForm.cshtml", entity);
        }
    }
}
