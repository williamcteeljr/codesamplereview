using DevExpress.Web.Mvc;
using PolicyTracker.DTO;
using PolicyTracker.Platform;
using PolicyTracker.BusinessServices;
using PolicyTracker.DomainModel.Common;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.DomainModel.Policy;
using PolicyTracker.DomainModel.Reports;
using PolicyTracker.DomainModel.Security;
using PolicyTracker.Filters;
using PolicyTracker.Platform.Security;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace PolicyTracker.Controllers
{
    [NoCache]
    public class DashboardController : BaseController
    {
        public ActionResult Index()
        { 
            return PartialView();
        }
        
        public ActionResult Filters()
        {
            ViewBag.Underwriters = ServiceLocator.EntityService.GetList<User>();
            ViewBag.Branches = StringEnum.GetAll<Branch>();
            ViewBag.ProductLines = ServiceLocator.EntityService.GetList<ProductLine>();
            return PartialView();
        }

        public FilterSet CleanFilters(FilterSet filters)
        {
            filters.UnderwriterIds = filters.UnderwriterIds.Where(x => !String.IsNullOrEmpty(x)).ToArray();
            filters.Branches = filters.Branches.Where(x => !String.IsNullOrEmpty(x)).ToArray();
            filters.ProductLines = filters.ProductLines.Where(x => !String.IsNullOrEmpty(x)).ToArray();
            return filters;
        }

        public ActionResult ProductLineInforceTotals()
        {
            var data = ServiceLocator.ReportingService.GetProductLineInforceAmounts();
            var results = new List<object>();
            var colors = new[] { "#F7464A", "#46BFBD", "#FDB45C", "#949FB1", "#4D5360", "#5cb85c", "#337ab7", "#FFFF66", "#d9edf7" };
            var i = 0;

            foreach (var pl in data)
            {
                var label = new StringBuilder(pl.ProductLine);
                label.Append(": ");
                if (pl.TotalInforce != null) label.Append(pl.TotalInforce.ToString("C0"));

                var dataPoint = new
                {
                    value = pl.TotalInforce,
                    color = colors[i],
                    highlight = "",
                    label = label.ToString()
                };

                results.Add(dataPoint);
                i++;
            }

            return Json(results, JsonRequestBehavior.AllowGet);
        }

        public JsonResult InforcePolicyCount(string branch = null)
        {
            var data = ServiceLocator.ReportingService.GetUWInforcePolicyCount(branch);

            return Json(data);
        }

        [HttpPost]
        public ActionResult Dashboard(FilterSet filters = null)
        {
            if (filters == null)
            {
                filters = new FilterSet();
                if (SecurityManager.InRole(new[] { ORA.SecurityRole.UW.Value }))
                    filters.UnderwriterIds = new[] { SessionManager.GetCurrentSession().User.UserId.ToString() };
                else if (SecurityManager.InRole(new[] { ORA.SecurityRole.BM.Value }))
                    filters.Branches = new[] { SessionManager.GetCurrentSession().User.BranchID };
                else if (SecurityManager.InRole(new[] { ORA.SecurityRole.PLM.Value }))
                {
                    var productLine = ServiceLocator.EntityService.GetInstance<ProductLine>(new PropertyFilter("ProductLineId", SessionManager.GetCurrentSession().User.ProductLine));
                    string productLineName = productLine != null ? productLine.Name : null;
                    if (!String.IsNullOrEmpty(productLineName)) filters.ProductLines = new[] { productLineName };
                }
            }
            else
            {
                filters = CleanFilters(filters);
            }

            ViewBag.RenewalChart = GetRenewalData(filters);
            ViewBag.RenewalRetentionGauge = GetRenewalRententionData(filters);
            ViewBag.GrowthData = GetGrowthData(filters);
            ViewBag.RatioData = GetRatioData(filters);
            ViewBag.Filters = filters;

            return PartialView();
        }

        public ActionResult Dashboard()
        {
            FilterSet filters = new FilterSet();

            if (SecurityManager.InRole(new[] { ORA.SecurityRole.UW.Value }))
                filters.UnderwriterIds = new[] { SessionManager.GetCurrentSession().User.UserId.ToString() };
            else if (SecurityManager.InRole(new[] { ORA.SecurityRole.BM.Value }))
                filters.Branches = new[] { SessionManager.GetCurrentSession().User.BranchID };
            else if (SecurityManager.InRole(new[] { ORA.SecurityRole.PLM.Value }))
            {
                var productLine = ServiceLocator.EntityService.GetInstance<ProductLine>(new PropertyFilter("ProductLineId", SessionManager.GetCurrentSession().User.ProductLine));
                string productLineName = productLine != null ? productLine.Name : null;
                if (!String.IsNullOrEmpty(productLineName)) filters.ProductLines = new[] { productLineName };
            }
            
            ViewBag.RenewalChart = GetRenewalData(filters);
            ViewBag.RenewalRetentionGauge = GetRenewalRententionData(filters);
            ViewBag.GrowthData = GetGrowthData(filters);
            ViewBag.RatioData = GetRatioData(filters);
            ViewBag.Filters = filters;

            return PartialView();
        }

        public dynamic GetRatioData(FilterSet dbFilters, bool isRenewal = false)
        {
            var result = new ExpandoObject() as dynamic;

            var filters = new List<PropertyFilter>()
            {
                new PropertyFilter("EffectiveDate", PropertyFilter.Comparator.GreaterEquals, DateTime.Now.AddDays((DateTime.Now.Day * -1) + 1)),
                new PropertyFilter("Status", PropertyFilter.Comparator.NotEquals, RiskStatus.INVOLVED.Value ),
                new PropertyFilter("IsRenewal", isRenewal)
            };

            if (dbFilters.UnderwriterIds.Length > 0)
                filters.Add(new PropertyFilter("UnderwriterId", PropertyFilter.Comparator.In, dbFilters.UnderwriterIds));
            if (dbFilters.ProductLines.Length > 0)
                filters.Add(new PropertyFilter("ProductLine", PropertyFilter.Comparator.In, dbFilters.ProductLines));
            if (dbFilters.Branches.Length > 0)
                filters.Add(new PropertyFilter("Branch", PropertyFilter.Comparator.In, dbFilters.Branches));

            var data = ServiceLocator.EntityService.GetList<RiskGraph>(filters);

            decimal numberSubmitted = data.Count();
            decimal numberQuoted = data.Where(x => x.Status == RiskStatus.QUOTE.Value).Count();
            decimal numberIssued = data.Where(x => x.Status == RiskStatus.BOUND.Value || x.Status == RiskStatus.ISSUED.Value).Count();

            result.QuoteRatio = (numberSubmitted != 0) ? Math.Round(numberQuoted / numberSubmitted * 100) : 0;
            result.HitRatio = (numberQuoted != 0) ? Math.Round(numberIssued / numberQuoted * 100) : 0;
            result.WrittenRatio = (numberSubmitted != 0) ? Math.Round(numberIssued / numberSubmitted * 100) : 0;

            return result;
        }

        public List<dynamic> GetGrowthData(FilterSet dbFilters)
        {
            var results = new List<dynamic>();
            var filters = new List<PropertyFilter>();

            if (dbFilters.Branches.Length > 0) filters.Add(new PropertyFilter("Branch", PropertyFilter.Comparator.In, dbFilters.Branches));

            filters.Add(new PropertyFilter("EffectiveDate", PropertyFilter.Comparator.GreaterEquals, DateTime.Now.AddMonths(-24).ToShortDateString()));
            filters.Add(new PropertyFilter("Status", PropertyFilter.Comparator.In, new[] { RiskStatus.BOUND.Value, RiskStatus.ISSUED.Value, RiskStatus.CANCELED.Value }));

            var data = ServiceLocator.EntityService.GetList<RiskGraph>(filters);
            var productLines = ServiceLocator.EntityService.GetList<ProductLine>();
            var priorTweleveStartDate = DateTime.Today.AddMonths(-12);
            var baseLine = 50;

            foreach (var pl in productLines)
            {
                var pld = data.Where(x => x.ProductLine == pl.Name);
                var result = new ExpandoObject() as dynamic;
                
                result.Productline = pl.Name;
                result.CurrentCancelled = pld.Where(x => x.EffectiveDate >= priorTweleveStartDate && x.Status == RiskStatus.CANCELED.Value).Sum(x => x.WrittenPremium);
                result.Current = pld.Where(x => x.EffectiveDate >= priorTweleveStartDate && (x.Status == RiskStatus.BOUND.Value || x.Status == RiskStatus.ISSUED.Value)).Sum(x => x.AnnualizedPremium);
                result.CurrentAmount = result.CurrentCancelled + result.Current;

                result.PriorCancelled = pld.Where(x => x.EffectiveDate < priorTweleveStartDate && x.Status == RiskStatus.CANCELED.Value).Sum(x => x.WrittenPremium);
                result.Prior = pld.Where(x => x.EffectiveDate < priorTweleveStartDate && (x.Status == RiskStatus.BOUND.Value || x.Status == RiskStatus.ISSUED.Value)).Sum(x => x.AnnualizedPremium);
                result.PriorAmount = result.PriorCancelled + result.Prior;

                result.Growth = result.PriorAmount - result.CurrentAmount;
                var diff = (result.PriorAmount > 0) ? result.CurrentAmount / result.PriorAmount  : 0;
                result.GrowthPercent = diff * baseLine;

                results.Add(result);
            }

            return results;
        }

        /// <summary>
        /// Renewal Renetion Grading system ( <85% = bad, 86%-93% = OK, > 94% = Good)
        /// </summary>
        /// <returns></returns>
        public dynamic GetRenewalRententionData(FilterSet dbFilters)
        {
            var filters = new List<PropertyFilter>()
            {
                new PropertyFilter("EffectiveDate", DateTime.Now.AddMonths(-12), DateTime.Now),
                new PropertyFilter("IsRenewal", true),
                new PropertyFilter("Status", PropertyFilter.Comparator.In, new[] { RiskStatus.LOST.Value, RiskStatus.BOUND.Value, RiskStatus.ISSUED.Value, RiskStatus.CANCELED.Value })
            };

            if (dbFilters.UnderwriterIds.Length > 0)
                filters.Add(new PropertyFilter("UnderwriterId", PropertyFilter.Comparator.In, dbFilters.UnderwriterIds));
            if (dbFilters.ProductLines.Length > 0)
                filters.Add(new PropertyFilter("ProductLine", PropertyFilter.Comparator.In, dbFilters.ProductLines));
            if (dbFilters.Branches.Length > 0)
                filters.Add(new PropertyFilter("Branch", PropertyFilter.Comparator.In, dbFilters.Branches));

            var data = ServiceLocator.EntityService.GetList<RiskGraph>(filters);
            var obj = new ExpandoObject() as dynamic;

            var totalRenewals = data.Count();
            var totalRenewalValue = data.Sum(x => x.ExpiredAnnualizedPremium);
            var totalRetained = data.Where(x => x.Status == RiskStatus.BOUND.Value || x.Status == RiskStatus.ISSUED.Value).Count();
            var totalRetainedValue = data.Where(x => x.Status == RiskStatus.BOUND.Value || x.Status == RiskStatus.ISSUED.Value).Sum(x => x.AnnualizedPremium);

            var totalRetainedAsPercent = (totalRenewals != 0) ? ((Convert.ToDecimal(totalRetained) / Convert.ToDecimal(totalRenewals))) * 100 : 0;
            var ValueRetainedAsPercent = (totalRenewalValue != 0) ? (totalRetainedValue / totalRenewalValue) * 100 : 0; ;

            obj.TotalRetainedAsPercent = totalRetainedAsPercent.ToString("N0");
            obj.ValueRetainedAsPercent = ValueRetainedAsPercent.ToString("N0");
            obj.TotalRetainedGrade = (totalRetainedAsPercent <= 85) ? "danger" : (totalRetainedAsPercent > 85 && totalRetainedAsPercent <= 93) ? "warning" : "success";
            obj.TotalRetainedBadgeGrade = (totalRetainedAsPercent <= 85) ? "bg-red" : (totalRetainedAsPercent > 85 && totalRetainedAsPercent <= 93) ? "bg-yellow" : "bg-green";
            obj.TotalValueRetainedGrade = (ValueRetainedAsPercent <= 85) ? "danger" : (ValueRetainedAsPercent > 85 && ValueRetainedAsPercent <= 93) ? "warning" : "success";
            obj.TotalValueRetainedBadgeGrade = (ValueRetainedAsPercent <= 85) ? "bg-red" : (ValueRetainedAsPercent > 85 && ValueRetainedAsPercent <= 93) ? "bg-yellow" : "bg-green";

            return obj;
        }

        public List<dynamic> GetRenewalData(FilterSet dbFilters)
        {
            var timeframes = new[] { 
                new {
                    TimeFrame = "Current Month",
                    DisplayText = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Now.Month),
                    Month = DateTime.Now.Month,
                    Year = DateTime.Now.Year
                }, 
                new {
                    TimeFrame = "Next Month",
                    DisplayText = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Now.AddMonths(1).Month),
                    Month = DateTime.Now.AddMonths(1).Month,
                    Year = DateTime.Now.AddMonths(1).Year
                },
                new {
                    TimeFrame = "Month After Next",
                    DisplayText = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Now.AddMonths(2).Month),
                    Month = DateTime.Now.AddMonths(2).Month,
                    Year = DateTime.Now.AddMonths(2).Year
                },
            };

            var list = new List<dynamic>();

            foreach (var timeframe in timeframes)
            {
                var filters = new List<PropertyFilter>()
                {
                    new PropertyFilter("EffectiveMonth", timeframe.Month),
                    new PropertyFilter("EffectiveYear", timeframe.Year),
                    new PropertyFilter("IsRenewal", true)
                };
                
                if (dbFilters.UnderwriterIds.Length > 0)
                    filters.Add(new PropertyFilter("UnderwriterId", PropertyFilter.Comparator.In, dbFilters.UnderwriterIds));
                if (dbFilters.ProductLines.Length > 0)
                    filters.Add(new PropertyFilter("ProductLine", PropertyFilter.Comparator.In, dbFilters.ProductLines));
                if (dbFilters.Branches.Length > 0)
                    filters.Add(new PropertyFilter("Branch", PropertyFilter.Comparator.In, dbFilters.Branches));

                var tfd = ServiceLocator.EntityService.GetList<RiskGraph>(filters);

                var obj = new ExpandoObject() as dynamic;

                obj.DisplayText = timeframe.DisplayText;
                obj.TimeFrame = timeframe.TimeFrame;
                obj.QuoteNumber = tfd.Where(x => x.Status == RiskStatus.QUOTE.Value).Count();
                obj.QuoteValue = tfd.Where(x => x.Status == RiskStatus.QUOTE.Value).Sum(x => x.AnnualizedPremium);
                obj.IssuedNumber = tfd.Where(x => x.Status == RiskStatus.ISSUED.Value || x.Status == RiskStatus.BOUND.Value || x.Status == RiskStatus.CANCELED.Value).Count();
                obj.IssuedValue = tfd.Where(x => x.Status == RiskStatus.ISSUED.Value || x.Status == RiskStatus.BOUND.Value || x.Status == RiskStatus.CANCELED.Value).Sum(x => x.AnnualizedPremium); ;
                obj.LostNumber = tfd.Where(x => x.Status == RiskStatus.LOST.Value).Count();
                obj.LostValue = tfd.Where(x => x.Status == RiskStatus.LOST.Value).Sum(x => x.AnnualizedPremium);
                obj.SubmissionNumber = tfd.Where(x => x.Status == RiskStatus.SUBMISSION.Value).Count();

                list.Add(obj);
            }

            return list;
        }

        public ActionResult StatChartFilters()
        {
            ViewBag.Underwriters = ServiceLocator.RiskService.GetUnderwriters();
            return PartialView();
        }

        [HttpGet]
        public ActionResult StatsChart()
        {
            var model = ServiceLocator.ReportingService.GetStatChartData();

            return PartialView(model);
        }

        public List<StatChart> GetStatChart()
        {
            var filters = new List<PropertyFilter>();
            var statFilters = new FilterSet();
            statFilters.Branches = CheckBoxListExtension.GetSelectedValues<string>("StatBranches");
            statFilters.ProductLines = CheckBoxListExtension.GetSelectedValues<string>("StatProductLines");
            statFilters.UnderwriterIds = CheckBoxListExtension.GetSelectedValues<string>("StatUnderwriterIds");
            var brokers = Request.Form["BrokerFilter_List"];

            if (!String.IsNullOrEmpty(brokers))
                statFilters.Brokers = brokers.Split(new[] { ", " }, StringSplitOptions.None);
            if (statFilters.Branches.Length > 0)
                filters.Add(new PropertyFilter("Branch", PropertyFilter.Comparator.In, statFilters.Branches));
            if (statFilters.ProductLines.Length > 0)
                filters.Add(new PropertyFilter("ProductLine", PropertyFilter.Comparator.In, statFilters.ProductLines));
            if (statFilters.UnderwriterIds.Length > 0)
                filters.Add(new PropertyFilter("UW", PropertyFilter.Comparator.In, statFilters.UnderwriterIds));
            if (statFilters.Brokers != null && statFilters.Brokers.Length > 0)
                filters.Add(new PropertyFilter("AgencyID", PropertyFilter.Comparator.In, statFilters.Brokers));

            var model = ServiceLocator.ReportingService.GetStatChartData(filters);

            return model;
        }

        public ActionResult BrokerStatFilter()
        {
            return PartialView();
        }

        [HttpPost]
        public ActionResult FilteredStatsChart()
        {
            var model = GetStatChart();
            return PartialView("~/Views/Dashboard/StatsChart.cshtml", model);
        }
    }
}
