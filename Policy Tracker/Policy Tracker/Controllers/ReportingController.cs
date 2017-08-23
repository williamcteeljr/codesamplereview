using DevExpress.XtraReports.UI;
using Policy_Tracker.Reports;
using PolicyTracker.BusinessServices;
using PolicyTracker.DomainModel.Common;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.DomainModel.Policy;
using PolicyTracker.DomainModel.Security;
using PolicyTracker.Platform.Logging;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web.Mvc;

namespace PolicyTracker.Controllers
{
    public class ReportingController : BaseController
    {
        #region Report Lists/Lookups
        public ActionResult Home()
        {
            return View();
        }

        public ActionResult ReportListing()
        {
            return PartialView("~/Views/Reporting/Lists/ReportListing.cshtml");
        }

        public ActionResult AircraftLookup()
        {
            return PartialView("~/Views/Reporting/Aircraft/Lookup.cshtml");
        }

        public ActionResult Installments()
        {
            return PartialView("~/Views/Reporting/Lists/Installments.cshtml");
        }
        #endregion

        #region DevExpress Exportable Reports
        public ActionResult Index()
        {
            return View();
        }

        public BoundNotIssuedReport GetBoundNotIssuedReport()
        {
            var report = new BoundNotIssuedReport();
            report.DataSource = ServiceLocator.EntityService.GetList<RiskGraph>(new PropertyFilter("Status", RiskStatus.BOUND.DisplayText));
            return report;
        }

        public ActionResult BoundNotIssuedReport()
        {
            ViewData["Report"] = GetBoundNotIssuedReport();
            return PartialView();
        }

        public ActionResult ExportBoundNotIssuedReport()
        {
            var report = GetBoundNotIssuedReport();
            ViewData["Report"] = report;
            return DevExpress.Web.Mvc.DocumentViewerExtension.ExportTo(report);
        }

        public ActionResult UnResolvedRenewalsReport()
        {
            var report = new UnResolvedRenewalsReport();
            report.DataSource = ServiceLocator.ReportingService.GetUnResolvedRenewalsReportData();
            ViewData["Report"] = report;
            return PartialView();
        }

        public ActionResult ExportUnResolvedRenewalsReport()
        {
            var report = new UnResolvedRenewalsReport();
            report.DataSource = ServiceLocator.ReportingService.GetUnResolvedRenewalsReportData();
            return DevExpress.Web.Mvc.DocumentViewerExtension.ExportTo(report);
        }

        public ActionResult TargetAccountsReport()
        {
            var report = new TargetAccountReport();
            report.DataSource = ServiceLocator.ReportingService.GetTargetAccountsReportData();
            ViewData["Report"] = report;
            return PartialView();
        }

        public ActionResult ExportTargetAccountsReport()
        {
            var report = new TargetAccountReport();
            report.DataSource = ServiceLocator.ReportingService.GetTargetAccountsReportData();
            return DevExpress.Web.Mvc.DocumentViewerExtension.ExportTo(report);
        }

        public ActionResult InstallmentsReport()
        {
            var report = new InstallmentsReport();
            ViewData["Report"] = report;
            return PartialView();
        }

        public ActionResult ExportInstallmentsReport()
        {
            var report = new InstallmentsReport();
            return DevExpress.Web.Mvc.DocumentViewerExtension.ExportTo(report);
        }

        public ActionResult UnPaidInstallmentsByMonthReport()
        {
            var report = new UnPaidInstallmentsByMonthReport();
            ViewData["Report"] = report;
            return PartialView();
        }

        public ActionResult PendingPremiumReport()
        {
            var report = new PendingPremiumReport();
            ViewData["Report"] = report;
            return PartialView();
        }

        public ActionResult ExportPendingPremiumReport()
        {
            var report = new PendingPremiumReport();
            return DevExpress.Web.Mvc.DocumentViewerExtension.ExportTo(report);
        }

        public ActionResult ExhibitFReport()
        {
            var report = new ExhibitFReport();
            ViewData["Report"] = report;
            return PartialView();
        }

        public ActionResult ExportExhibitFReport()
        {
            var report = new ExhibitFReport();
            return DevExpress.Web.Mvc.DocumentViewerExtension.ExportTo(report);
        }

        public ActionResult ExhibitBGNEPIReport()
        {
            var report = new ExhibitBGNEPIReport();
            ViewData["Report"] = report;
            return PartialView();
        }

        public ActionResult ExportExhibitBGNEPIReport()
        {
            var report = new ExhibitBGNEPIReport();
            return DevExpress.Web.Mvc.DocumentViewerExtension.ExportTo(report);
        }

        public ActionResult ExhibitBPayrollReport()
        {
            var report = new ExhibitBPayrollReport();
            ViewData["Report"] = report;
            return PartialView();
        }

        public ActionResult ExportExhibitBPayrollReport()
        {
            var report = new ExhibitBPayrollReport();
            return DevExpress.Web.Mvc.DocumentViewerExtension.ExportTo(report);
        }

        public ActionResult ExhibitDExposureInfoReport()
        {
            var report = new ExhibitDExposureInfoReport();
            ViewData["Report"] = report;
            return PartialView();
        }

        public ActionResult ExportExhibitDExposureInfoReport()
        {
            var report = new ExhibitDExposureInfoReport();
            return DevExpress.Web.Mvc.DocumentViewerExtension.ExportTo(report);
        }

        public ActionResult ExhibitE_LLReport()
        {
            var report = new ExhibitE_LLReport();
            ViewData["Report"] = report;
            return PartialView();
        }

        public ActionResult ExportExhibitE_LLReport()
        {
            var report = new ExhibitE_LLReport();
            return DevExpress.Web.Mvc.DocumentViewerExtension.ExportTo(report);
        }

        public ActionResult PayrollByStAndCCReport()
        {
            var report = new PayrollByStAndCCReport();
            ViewData["Report"] = report;
            return PartialView();
        }

        public ActionResult ExportPayrollByStAndCCReport()
        {
            var report = new PayrollByStAndCCReport();
            return DevExpress.Web.Mvc.DocumentViewerExtension.ExportTo(report);
        }

        public ActionResult ExportUnPaidInstallmentsByMonthReport()
        {
            var report = new UnPaidInstallmentsByMonthReport();
            return DevExpress.Web.Mvc.DocumentViewerExtension.ExportTo(report);
        }

        public ActionResult NewBuisnessCountReport(bool exportReport = false)
        {
            var report = new RiskSubmissionCountReport();
            ViewData["Report"] = report;

            if (exportReport)
                return DevExpress.Web.Mvc.DocumentViewerExtension.ExportTo(report);

            return PartialView();
        }

        #region NBF Report
        public ActionResult NewBusinessFlow()
        {
            List<int> years = new List<int>();
            for (int i = DateTime.Now.Year; i >= DateTime.Now.Year-10; i--)
            {
                years.Add(i);
            }
            ViewBag.Years = years;
            ViewBag.Underwriters = ServiceLocator.RiskService.GetUnderwriters();
            ViewBag.Months = StringEnum.GetAll<Month>();
            return PartialView();
        }

        public NewBusinessFlowReport GetNBFReport(int year, int month, int underwriterId = 0)
        {
            var report = new NewBusinessFlowReport();
            var controls = report.AllControls<XRLabel>();
            controls.Where(x => x.Name == "YearLabel").First().Text = String.Format("Year: {0}", year);
            controls.Where(x => x.Name == "PriorLabel").First().Text = String.Format("Prior: {0}", year - 1);
            controls.Where(x => x.Name == "MonthLabel").First().Text = String.Format("Month: {0}", StringEnum.Parse<Month>(month.ToString()).DisplayText);
            var pivotControls = report.AllControls<XRPivotGrid>();

            var branchReport = report.Bands["BranchBand"] as DetailReportBand;
            branchReport.DataSource = ServiceLocator.ReportingService.GetNBFByBranch(year, month, underwriterId);
            var branchMonthReport = report.Bands["BranchUWBand"] as DetailReportBand;
            branchMonthReport.DataSource = ServiceLocator.ReportingService.GetNBFByBranchAndUW(year, month, underwriterId);
            var productLineReport = report.Bands["ProductLineBand"] as DetailReportBand;
            productLineReport.DataSource = ServiceLocator.ReportingService.GetNBFByProductLine(year, month, underwriterId);

            return report;
        }

        public ActionResult NewBusinessFlowReport(int Year, int Month, int UnderwriterId)
        {
            ViewBag.Year = Year;
            ViewBag.Month = Month;
            ViewBag.UnderwriterId = UnderwriterId;
            var report = GetNBFReport(Year, Month, UnderwriterId);
            ViewData["Report"] = report;
            return PartialView();
        }

        public ActionResult ExportNewBusinessFlowReport(int Year, int Month)
        {
            var report = GetNBFReport(Year, Month);
            return DevExpress.Web.Mvc.DocumentViewerExtension.ExportTo(report);
        }
        #endregion

        #region YTD New Business Flow Report
        public ActionResult YTDNewBusinessFlow()
        {
            List<int> years = new List<int>();
            for (int i = DateTime.Now.Year; i >= DateTime.Now.Year - 5; i--)
            {
                years.Add(i);
            }
            ViewBag.Years = years;
            ViewBag.Underwriters = ServiceLocator.EntityService.GetList<User>().ToList() as List<User>;
            return PartialView();
        }

        public NewBusinessFlowYTD GetYTDNBFReport(int year, string underwriter = null)
        {
            var report = new NewBusinessFlowYTD();
            var controls = report.AllControls<XRLabel>();
            controls.Where(x => x.Name == "YearFilter").First().Text = String.Format("Year: {0}", year);
            if (!String.IsNullOrEmpty(underwriter))
                controls.Where(x => x.Name == "UnderwriterFilter").First().Text = String.Format("Underwriter: {0}", underwriter);
            else
                controls.Where(x => x.Name == "UnderwriterFilter").First().Text = String.Format("Underwriter: {0}", "ALL");
            //report.DataSource = ServiceLocator.ReportingService.GetYTDNewBusinessFlowReportData(year, underwriter);

            var branchReport = report.Bands["BranchBand"] as DetailReportBand;
            branchReport.DataSource = ServiceLocator.ReportingService.GetYTDNBFByBranch(year, underwriter);
            var branchMonthReport = report.Bands["BranchMonthBand"] as DetailReportBand;
            branchMonthReport.DataSource = ServiceLocator.ReportingService.GetYTDNBFByBranchAndMonth(year, underwriter);
            var productLineReport = report.Bands["ProductLineBand"] as DetailReportBand;
            productLineReport.DataSource = ServiceLocator.ReportingService.GetYTDNewBusinessFlowReportProductLineData(year, underwriter);

            return report;
        }

        public ActionResult YTDNewBusinessFlowReport(int year, string underwriter = null)
        {
            ViewBag.Year = year;
            ViewBag.Underwriter = underwriter;
            var report = GetYTDNBFReport(year, underwriter);
            ViewData["Report"] = report;
            return PartialView();
        }

        public ActionResult ExportYTDNewBusinessFlowReport(int year, string underwriter = null)
        {
            var report = GetYTDNBFReport(year, underwriter);
            ViewData["Report"] = report;
            return DevExpress.Web.Mvc.DocumentViewerExtension.ExportTo(report);
        }
        #endregion

        #region Predictive Premium Report
        public ActionResult PredictivePremium()
        {
            ViewBag.Months = StringEnum.GetAll<Month>();
            return PartialView();
        }

        public PredictivePremiumReport GetPredictivePremiumReport(int month)
        {
            var report = new PredictivePremiumReport();
            var controls = report.AllControls<XRLabel>();
            controls.Where(x => x.Name == "MonthLbl").First().Text = String.Format("Month: {0}", CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month));
            report.DataSource = ServiceLocator.ReportingService.GetPredictivePremiumReportData(month);
            return report;
        }

        public ActionResult PredictivePremiumReport(int month)
        {
            ViewBag.Month = month;
            var report = GetPredictivePremiumReport(month);
            ViewData["Report"] = report;
            return PartialView();
        }

        public ActionResult ExportPredictivePremium(int month)
        {
            var report = GetPredictivePremiumReport(month);
            ViewData["Report"] = report;
            return DevExpress.Web.Mvc.DocumentViewerExtension.ExportTo(report);
        }
        #endregion

        #region Product Line Monthly Detail Report
        public ActionResult ProductLineMonthlyDetail()
        {
            ViewBag.Months = StringEnum.GetAll<Month>();
            ViewBag.ProductLines = ServiceLocator.EntityService.GetList<ProductLine>();
            return PartialView();
        }

        public ProductLineMonthlyDetail GetProductLineMonthlyDetailReport(string productLine, int month)
        {
            var report = new ProductLineMonthlyDetail();
            var controls = report.AllControls<XRLabel>();
            controls.Where(x => x.Name == "MonthVal").First().Text = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
            controls.Where(x => x.Name == "YearVal").First().Text = DateTime.Now.Year.ToString();
            controls.Where(x => x.Name == "ProductLineVal").First().Text = productLine;
            report.DataSource = ServiceLocator.ReportingService.GetProductLineDetail(productLine, DateTime.Now.Year, month);
            return report;
        }

        public ActionResult ProductLineMonthlyDetailReport(string productLine, int month)
        {
            ViewBag.Month = month;
            ViewBag.ProductLine = productLine;
            var report = GetProductLineMonthlyDetailReport(productLine, month);
            ViewData["Report"] = report;
            return PartialView();
        }

        public ActionResult ExportProductLineMonthlyDetailReport(string productLine, int month)
        {
            var report = GetProductLineMonthlyDetailReport(productLine, month);
            ViewData["Report"] = report;
            return DevExpress.Web.Mvc.DocumentViewerExtension.ExportTo(report);
        }
        #endregion
        #endregion

        [HttpGet]
        public HttpStatusCodeResult MonthlyExpiringAccounts()
        {
            string view = "~/Views/Reporting/EmailedReports/MonthlyExpiringAccounts.cshtml";
            string emailBody = null;
            var response = new HttpStatusCodeResult(200);

            var risks = ServiceLocator.ReportingService.GetMonthlyExpiringAccounts(DateTime.Now.Month);

            var orderedRisks = new List<RiskGraph>();

            orderedRisks.AddRange(risks.Where(x => x.Status == RiskStatus.SUBMISSION.Value));
            orderedRisks.AddRange(risks.Where(x => x.Status == RiskStatus.QUOTE.Value));
            orderedRisks.AddRange(risks.Where(x => x.Status == RiskStatus.BOUND.Value));
            orderedRisks.AddRange(risks.Where(x => x.Status == RiskStatus.ISSUED.Value));

            var summary = new ExpandoObject() as dynamic;

            summary.Issued = risks.Where(x => (x.Status == RiskStatus.ISSUED.Value || x.Status == RiskStatus.BOUND.Value) && !x.IsPaidInInstallments).Sum(x => x.AnnualizedPremium);
            summary.InstallmentReporterDeposits = risks.Where(x => (x.Status == RiskStatus.ISSUED.Value || x.Status == RiskStatus.BOUND.Value) && x.IsPaidInInstallments).Sum(x => x.DepositPremium);
            summary.Pending = risks.Where(x => x.Status != RiskStatus.ISSUED.Value && x.Status == RiskStatus.BOUND.Value && !x.IsPaidInInstallments).Sum(x => x.AnnualizedPremium);
            summary.PendingDeposits = risks.Where(x => x.Status != RiskStatus.ISSUED.Value && x.Status == RiskStatus.BOUND.Value && x.IsPaidInInstallments).Sum(x => x.DepositPremium);

            try
            {
                using (var sw = new StringWriter())
                {
                    var viewResult = ViewEngines.Engines.FindPartialView(this.ControllerContext, view);
                    var viewContext = new ViewContext(this.ControllerContext, viewResult.View, this.ViewData, this.TempData, sw);
                    viewContext.ViewBag.Risks = orderedRisks.Where(x => x.Status != RiskStatus.ISSUED.Value);
                    viewContext.ViewBag.Summary = summary;
                    viewResult.View.Render(viewContext, sw);
                    viewResult.ViewEngine.ReleaseView(this.ControllerContext, viewResult.View);

                    emailBody = sw.GetStringBuilder().ToString();
                }

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("oraReporting@NoMail.com");
                mail.To.Add("jlee@oraero.com");
                mail.Subject = "Monthly Expiring Accounts";
                mail.IsBodyHtml = true;
                mail.Body = emailBody;

                SmtpClient smtp = new SmtpClient();
                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                LogManager.Log(LogLevel.ERROR, String.Format("Error sending Monthly Expiring Accounts Report. [[ {0} ]]", ex.Message));
                response = new HttpStatusCodeResult(400);
            }

            return response;
        }
    }
}
