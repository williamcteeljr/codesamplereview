using CsvHelper;
using DevExpress.Web.Mvc;
using PolicyTracker.BusinessServices;
using PolicyTracker.DomainModel.Policy;
using PolicyTracker.DomainModel.Security;
using PolicyTracker.Platform.Caching;
using PolicyTracker.Platform.Security;
using PolicyTracker.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace PolicyTracker.Controllers
{
    public class WorkingListGridController : DXBaseGridController
    {
        public static WorkingListGridConfig _DEFAULT_SETUP = new WorkingListGridConfig()
        {
            Name = "Default Setup",
            Columns = "Status,ImageRightId,Name,UW,UA,AgencyName,AgencyID,EffectiveDate,ExpirationDate,Branch,ProductLine,PolicyNumber,AnnualizedPremium,RiskId,IsRenewal,HasNotes"
        };

        public WorkingListGridController()
        {
            _GRID = "WorkingListGrid";
        }

        private void WorkingListViewData()
        {
            ViewBag.Underwriters = ServiceLocator.EntityService.GetList<User>().OrderBy(x => x.Name);
        }

        public ActionResult WorkingListGrid(string gridConfig = null)
        {
            var gvm = GridViewExtension.GetViewModel(_GRID);
            if (gvm == null)
            {
                gvm = new GridViewModel();
                gvm = CreateGridViewModel("RiskId", new[] { "RiskId", "ImageRightId", "Status", "Name", "UW", "UA", "AgencyID", "EffectiveDate", "Branch", "ProductLine", "PolicyType", "PolicyNumber", "PolicySuffix", "UserId", "QuoteType", "ControlNumber" });
                //gvm.FilterExpression = "Contains([UW], '" + SessionManager.GetCurrentSession().User.Name + "')";
                //gvm.Columns[4].FilterExpression = "Contains([UW], '" + SessionManager.GetCurrentSession().User.Name + "')";
                gvm.Pager.PageSize = 100;
            }
            ViewBag.UserName = SessionManager.GetCurrentSession().User.Name;
            if (gvm.Pager.PageSize < MinPageSize) gvm.Pager.PageSize = MinPageSize;
            WorkingListViewData();

            ViewBag.ConfigName = gridConfig;

            return GetDataForGrid(gvm);
        }

        public override ActionResult Page(GridViewPagerState pager)
        {
            var gvm = GridViewExtension.GetViewModel(_GRID);
            var pageSizeChanged = (gvm.Pager.PageSize != pager.PageSize);
            WebUtils.BuildCustomArgs(Request.Params);
            gvm.ApplyPagingState(pager);
            if (gvm.Pager.PageSize < MinPageSize) gvm.Pager.PageSize = MinPageSize;
            if (pageSizeChanged) gvm.Pager.PageIndex = 0;
            WorkingListViewData();
            return GetDataForGrid(gvm);
        }

        public override ActionResult Sort(GridViewColumnState column, bool reset)
        {
            var viewModel = GridViewExtension.GetViewModel(_GRID);
            WebUtils.BuildCustomArgs(Request.Params);
            viewModel.ApplySortingState(column, reset);
            WorkingListViewData();
            return GetDataForGrid(viewModel);
        }

        public override ActionResult Filter(GridViewFilteringState filteringState)
        {
            var gvm = GridViewExtension.GetViewModel(_GRID);
            WebUtils.BuildCustomArgs(Request.Params);
            gvm.ApplyFilteringState(filteringState);
            //Reset the page index to the first page each time a new filter is applied. Otherwise the results get messed up if there aren't enough to make it to the current page the user is on.
            gvm.Pager.PageIndex = 0;
            WorkingListViewData();
            return GetDataForGrid(gvm);
        }

        public override PartialViewResult GetDataForGrid(GridViewModel gridViewModel)
        {
            gridViewModel.ProcessCustomBinding(ServiceLocator.WorkingListGridSvc.GetRowCount, ServiceLocator.WorkingListGridSvc.GetDataSimple);
            return PartialView("~/Views/Home/WorkingListGridView.cshtml", gridViewModel);
        }

        public PartialViewResult CustomPerformCallback()
        {
            var gvm = GridViewExtension.GetViewModel(_GRID);
            return GetDataForGrid(gvm);
        }
 
        public void Export(string filterExp = null)
        {
            var workingListModel = SessionManager.GetSessionValue("LastWorkingListModel") as GridViewCustomBindingGetDataRowCountArgs;
            var exportData = ServiceLocator.WorkingListGridSvc.GetWorkingListExportData(workingListModel);

            var memoryStream = new MemoryStream();

            var writer = new CsvWriter(new StreamWriter(memoryStream));
            writer.WriteRecords(exportData);

            memoryStream.Position = 0;
            Response.Clear();
            Response.AddHeader("Content-Disposition", "attachment; filename=WorkingList.csv");
            Response.ContentType = "text/csv";
            Response.Write(new StreamReader(memoryStream).ReadToEnd());
            Response.End();
        }
    }
}
