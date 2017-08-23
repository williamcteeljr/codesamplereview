using PolicyTracker.BusinessServices;
using PolicyTracker.DomainModel;
using PolicyTracker.DomainModel.Common;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.DomainModel.Policy;
using PolicyTracker.Platform.Security;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebAPI.Controllers
{
    public class ConsoleController : BaseController
    {
        [HttpGet]
        [ActionName("ProductLineConsole")]
        public dynamic ProductLineConsole(string pl, int year = 0, int month = 0, int uwId = 0, string branch = null)
        {
            var results = new ExpandoObject() as dynamic;
            results.data = ServiceLocator.ReportingService.GetProductLineDetail(pl, year, month, uwId, branch);
            results.BranchSummary = ServiceLocator.ReportingService.GetBranchSummary(pl, year, month);

            var filters = new List<PropertyFilter>() { new PropertyFilter("ProductLine", pl) };
            DateTime lastGen = DateTime.Now.AddMonths(3);
            DateTime lastRenewalGenerationDate = new DateTime(lastGen.Year, lastGen.Month, DateTime.DaysInMonth(lastGen.Year, lastGen.Month));

            if (month != 0)
            {
                filters.Add(new PropertyFilter("EffectiveMonth", month));

                
                if (new DateTime(year, month, 1) > lastRenewalGenerationDate)
                    filters.Add(new PropertyFilter("EffectiveYear", year - 1));
                else
                    filters.Add(new PropertyFilter("EffectiveYear", year));
            }

            if (uwId != 0)
            {
                filters.Add(new PropertyFilter("UnderwriterId", uwId));
                var renewalUnderwriterFilters = new List<PropertyFilter>();
                foreach (var filter in filters)
                    renewalUnderwriterFilters.Add(new PropertyFilter(filter.PropertyName, PropertyFilter.Comparator.Equals, filter.Value, 1));
                renewalUnderwriterFilters.Add(new PropertyFilter("RenewalUnderwriterId", PropertyFilter.Comparator.Equals, uwId, 1));
                filters.AddRange(renewalUnderwriterFilters);
            }

            if (month != 0)
            {
                IEnumerable<RiskGraph> risks = ServiceLocator.EntityService.GetList<RiskGraph>(filters);
                List<RiskGraph> renewals = risks.Where(x => x.IsRenewal).OrderBy(x => x.EffectiveDate).ToList();
                if (month != 0)
                {
                    if (new DateTime(year, month, 1) > lastRenewalGenerationDate)
                        renewals.AddRange(risks.Where(x => !x.IsRenewal && x.Status == RiskStatus.ISSUED.Value));
                }

                IEnumerable<RiskGraph> newBusiness = risks.Where(x => !x.IsRenewal).OrderBy(x => x.EffectiveDate);

                #region Get Cancelled Risks
                if (year == DateTime.Now.Year)
                {
                    filters.RemoveAll(x => x.PropertyName == "EffectiveYear");

                    if (uwId != 0)
                    {
                        filters.AddRange(new[] { new PropertyFilter("EffectiveYear", year - 1), new PropertyFilter("EffectiveYear", PropertyFilter.Comparator.Equals, year - 1, 1) });
                        filters.AddRange(new[] { new PropertyFilter("Status", RiskStatus.CANCELED.Value), new PropertyFilter("Status", PropertyFilter.Comparator.Equals, RiskStatus.CANCELED.Value, 1) });
                    }
                    else
                    {
                        filters.Add(new PropertyFilter("EffectiveYear", year - 1));
                        filters.Add(new PropertyFilter("Status", RiskStatus.CANCELED.Value));
                    }

                    var cancelledRisks = ServiceLocator.EntityService.GetList<RiskGraph>(filters);
                    cancelledRisks.Select(o => { o.ExpiringWrittenPremium = o.WrittenPremium; o.ExpiredAnnualizedPremium = o.AnnualizedPremium; o.WrittenPremium = 0; o.AnnualizedPremium = 0; o.InceptionPremium = 0; return o; }).ToList();
                    renewals.AddRange(cancelledRisks);
                }
                #endregion

                results.Renewals = renewals;
                results.NewBusiness = newBusiness;
            }
            
            #region Impact Notes
            DateTime startDate = (month != 0) ? new DateTime(year, month, 1) : new DateTime(year, 1, 1);
            DateTime endDate = (month != 0) ? new DateTime(year, month, DateTime.DaysInMonth(year, month)) : new DateTime(year, 12, DateTime.DaysInMonth(year, 12));
            var productLineId = ServiceLocator.EntityService.GetInstance<ProductLine>(new PropertyFilter("Name", pl)).ProductLineId;
            results.ImpactNotes = ServiceLocator.ConsoleService.GetImpactNotes(startDate, endDate, productLineId);
            #endregion

            return results;
        }

        [HttpGet]
        [ActionName("GetUnderwriterStats")]
        public dynamic GetUnderwriterStats(string underwriterId, string branch = null)
        {
            var result = new ExpandoObject() as dynamic;

            var data = GetCounts(underwriterId, branch);
            result.BusinessTypeCount = data.BusinessTypeCount;
            result.StatusCountByMonth = data.StatusCountByMonth;

            var riskStatusCounts = GetRiskStatusCounts(underwriterId, branch);
            result.RiskStatusCounts = riskStatusCounts;

            //quote hit and written ratio for UI data binding
            var ratios = GetRatios(underwriterId, branch);
            result.QuoteRatio = ratios.QuoteRatio;
            result.HitRatio = ratios.HitRatio;
            result.WrittenRatio = ratios.WrittenRatio;

            return result;
        }

        /// <summary>
        /// Gets the sum/count of the risks handled by either an underwriter, branch, or entire company
        /// </summary>
        /// <param name="underwriterId"></param>
        /// <param name="branch"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetRiskStatusCounts")]
        public dynamic GetRiskStatusCounts(string underwriterId = null, string branch = null, string productLine = null)
        {
            var result = new ExpandoObject() as dynamic;
            var filters = new List<PropertyFilter>()
            {
                new PropertyFilter("EffectiveDate", PropertyFilter.Comparator.Between, new DateTime(DateTime.Now.Year, 1, 1), DateTime.Now)
            };

            if (!String.IsNullOrEmpty(underwriterId))
                filters.Add(new PropertyFilter("UnderwriterId", underwriterId));
            if (!String.IsNullOrEmpty(branch))
                filters.Add(new PropertyFilter("Branch", branch));
            if (!String.IsNullOrEmpty(productLine))
                filters.Add(new PropertyFilter("ProductLine", productLine));

            var risks = ServiceLocator.EntityService.GetList<RiskGraph>(filters).ToList();
            //submissions, declined, quotes, issues, lost, canceled counts
            result.Submissions = risks.Where(x => x.IsRenewal == false && x.Status != RiskStatus.INVOLVED.Value).Count();
            result.Declined = risks.Where(x => x.IsRenewal == false && x.Status == RiskStatus.DECLINED.Value).Count();

            result.Quotes = risks.Where(x =>
                x.IsRenewal == false
                && x.Status == RiskStatus.QUOTE.Value).Count();

            result.Issued = risks.Where(x =>
                x.IsRenewal == true &&(
                x.Status == RiskStatus.ISSUED.Value
                || x.Status == RiskStatus.BOUND.Value
            )).Count();

            result.Lost = risks.Where(x => x.IsRenewal == true && x.Status == RiskStatus.LOST.Value).Count();

            result.Cancelled = risks.Where(x => x.Status == RiskStatus.CANCELED.Value).Count();

            //Get Total Inforce
            filters = new List<PropertyFilter>()
            {
                new PropertyFilter("Status", RiskStatus.ISSUED.Value),
                new PropertyFilter("ExpirationDate", PropertyFilter.Comparator.GreaterEquals, DateTime.Now)
            };

            if (!String.IsNullOrEmpty(underwriterId))
                filters.Add(new PropertyFilter("UnderwriterId", underwriterId));
            if (!String.IsNullOrEmpty(branch))
                filters.Add(new PropertyFilter("Branch", branch));
            if (!String.IsNullOrEmpty(productLine))
                filters.Add(new PropertyFilter("ProductLine", productLine));

            result.TotalInforce = ServiceLocator.EntityService.Count<RiskGraph>(filters);

            return result;
        }

        [HttpGet]
        [ActionName("GetRatios")]
        public dynamic GetRatios(string underwriterId = null, string branch = null)
        {
            return ServiceLocator.ConsoleService.GetRatios(underwriterId, branch);
        }

        
        [HttpGet]
        [ActionName("GetCounts")]
        public dynamic GetCounts(string underwriterId = null, string branch = null)
        {
            var results = new ExpandoObject() as dynamic;
            var filters = new List<PropertyFilter>()
            {
                new PropertyFilter("EffectiveMonth", PropertyFilter.Comparator.LessEquals, DateTime.Now.Month),
                new PropertyFilter("EffectiveYear", DateTime.Now.Year),
            };

            if (!String.IsNullOrEmpty(underwriterId))
                filters.Add(new PropertyFilter("UnderwriterId", underwriterId));
            if (!String.IsNullOrEmpty(branch))
                filters.Add(new PropertyFilter("Branch", branch));

            var risks = ServiceLocator.EntityService.GetList<RiskGraph>(filters).ToList();

            var groups = from row in risks
                         group row by new
                         {
                             row.EffectiveMonth
                         } into grp
                         select grp.First();

            groups = groups.OrderBy(x => x.EffectiveMonth).ToList();

            var businessTypeResults = new List<dynamic>();

            foreach (var group in groups)
            {
                var result = new ExpandoObject() as dynamic;
                var riskSet = risks.Where(x => x.EffectiveMonth == group.EffectiveMonth);

                result.Month = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(group.EffectiveMonth);
                result.NewBusiness = riskSet.Count(x => !x.IsRenewal && (x.Status == RiskStatus.ISSUED.Value || x.Status == RiskStatus.BOUND.Value));
                result.NewBusinessValue = riskSet.Where(x => !x.IsRenewal && (x.Status == RiskStatus.ISSUED.Value || x.Status == RiskStatus.BOUND.Value)).Sum(x => x.AnnualizedPremium);
                result.Renewals = riskSet.Count(x => x.IsRenewal && (x.Status == RiskStatus.ISSUED.Value || x.Status == RiskStatus.BOUND.Value));
                result.RenewalsValue = riskSet.Where(x => x.IsRenewal && (x.Status == RiskStatus.ISSUED.Value || x.Status == RiskStatus.BOUND.Value)).Sum(x => x.AnnualizedPremium);
                businessTypeResults.Add(result);
            }
            results.BusinessTypeCount = businessTypeResults;

            var statusCountByMonthResults = new List<dynamic>();

            foreach (var group in groups)
            {
                var result = new ExpandoObject() as dynamic;
                var riskSet = risks.Where(x => x.EffectiveMonth == group.EffectiveMonth);

                result.Month = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(group.EffectiveMonth);
                result.Submissions = riskSet.Count(x => x.EffectiveMonth == group.EffectiveMonth && x.Status != RiskStatus.INVOLVED.Value);
                result.Declines = riskSet.Where(x => x.EffectiveMonth == group.EffectiveMonth && x.Status == RiskStatus.DECLINED.Value).Count();
                result.Quotes = riskSet.Where(x => x.EffectiveMonth == group.EffectiveMonth && x.Status == RiskStatus.QUOTE.Value).Count();
                result.Issued = risks.Where(x => x.EffectiveMonth == group.EffectiveMonth && ( x.IsRenewal == true &&(x.Status == RiskStatus.ISSUED.Value || x.Status == RiskStatus.BOUND.Value))).Count();
                result.Loses = riskSet.Where(x => x.EffectiveMonth == group.EffectiveMonth && x.Status == RiskStatus.LOST.Value).Count();
                result.Cancelled = riskSet.Where(x => x.EffectiveMonth == group.EffectiveMonth && x.Status == RiskStatus.CANCELED.Value).Count();
                
                statusCountByMonthResults.Add(result);
            }
            results.StatusCountByMonth = statusCountByMonthResults;

            return results;
        }

        [HttpGet]
        [ActionName("GetTopBrokers")]
        public dynamic GetTopBrokers(string underwriterId = null, string branch = null)
        {
            var result = new ExpandoObject() as dynamic;

            result.TopTenCount = ServiceLocator.BrokerSvc.GetTopTenBrokersByCount(underwriterId, branch);
            result.TopTenPremium = ServiceLocator.BrokerSvc.GetTopTenBrokersByPremium(underwriterId, branch);

            return result;
        }

        [HttpGet]
        [ActionName("GetProductLineData")]
        public dynamic GetProductLineData(string branch = null)
        {
            var result = new ExpandoObject() as dynamic;

            result.ProductLineCounts = ServiceLocator.ConsoleService.GetProductLinePolicyCounts(branch);
            result.ProductLinePremiums = ServiceLocator.ConsoleService.GetProductLinePolicyPremiums(branch);

            return result;
        }

        [HttpGet]
        [ActionName("GetBranchUnderwriterWorkload")]
        public IEnumerable<dynamic> GetBranchUnderwriterWorkload(string branch = null)
        {
            return ServiceLocator.ReportingService.GetUWInforcePolicyCount(branch);
        }

        [HttpGet]
        [ActionName("GetBudgetStats")]
        public IEnumerable<dynamic> GetBudgetStats(bool isYearly = false)
        {
            return ServiceLocator.ConsoleService.GetBudgetData(isYearly);
        }

        [HttpGet]
        [ActionName("GetGrowthData")]
        public IEnumerable<dynamic> GetGrowthData()
        {
            var results = new List<dynamic>();
            var filters = new List<PropertyFilter>();

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

                result.ProductLine = pl.Name;
                result.CurrentCancelled = pld.Where(x => x.EffectiveDate >= priorTweleveStartDate && x.Status == RiskStatus.CANCELED.Value).Sum(x => x.WrittenPremium);
                result.Current = pld.Where(x => x.EffectiveDate >= priorTweleveStartDate && (x.Status == RiskStatus.BOUND.Value || x.Status == RiskStatus.ISSUED.Value)).Sum(x => x.WrittenPremium);
                result.CurrentAmount = result.CurrentCancelled + result.Current;

                result.PriorCancelled = pld.Where(x => x.EffectiveDate < priorTweleveStartDate && x.Status == RiskStatus.CANCELED.Value).Sum(x => x.WrittenPremium);
                result.Prior = pld.Where(x => x.EffectiveDate < priorTweleveStartDate && (x.Status == RiskStatus.BOUND.Value || x.Status == RiskStatus.ISSUED.Value)).Sum(x => x.WrittenPremium);
                result.PriorAmount = result.PriorCancelled + result.Prior;

                result.Growth = result.PriorAmount - result.CurrentAmount;
                var diff = (result.PriorAmount > 0) ? result.CurrentAmount / result.PriorAmount : 0;
                result.GrowthPercent = diff * baseLine;

                results.Add(result);
            }

            return results;
        }

        /// <summary>
        /// Renewal Renetion Grading system ( <85% = bad, 86%-93% = OK, > 94% = Good)
        /// </summary>
        /// <returns></returns>
        /// [HttpGet]
        [ActionName("GetRenewalRetentionData")]
        public dynamic GetRenewalRetentionData(string productLine = null)
        {
            var filters = new List<PropertyFilter>();

            filters.Add(new PropertyFilter("EffectiveDate", DateTime.Now.AddMonths(-12), DateTime.Now));
            filters.Add(new PropertyFilter("IsRenewal", true));
            if(!String.IsNullOrEmpty(productLine))
                filters.Add(new PropertyFilter("ProductLine", productLine));

            //Only Count renewals in a final disposition
            filters.Add(new PropertyFilter("Status", PropertyFilter.Comparator.In, new[] { RiskStatus.LOST.Value, RiskStatus.BOUND.Value, RiskStatus.ISSUED.Value, RiskStatus.CANCELED.Value }));
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

        [HttpGet]
        [ActionName("GetTopTenQuotedProspects")]
        public PaginatedList<RiskGraph> GetTopTenQuotedProspects(int pageSize, int pageNumber = 1, string sortProperty = null, string sortOrder = null)
        {
            var filters = new List<PropertyFilter>();

            filters.Add(new PropertyFilter("IsRenewal", false));
            filters.Add(new PropertyFilter("Status", RiskStatus.QUOTE.Value));

            return base.GetPaginatedList<RiskGraph>(10, pageNumber, "AnnualizedPremium", "desc", filters);
        }

        [HttpGet]
        [ActionName("GetTopTwentyOutstandingQuotes")]
        public PaginatedList<RiskGraph> GetTopTenOutstandingQuotes(int pageSize, int pageNumber = 1, string sortProperty = null, string sortOrder = null)
        {
            var filters = new List<PropertyFilter>();
            filters.Add(new PropertyFilter("Status", RiskStatus.QUOTE.Value));

            return base.GetPaginatedList<RiskGraph>(pageSize, pageNumber, "AnnualizedPremium", "desc", filters);
        }

        [HttpGet]
        [ActionName("GetTopTwentyPolicies")]
        public PaginatedList<RiskGraph> GetTopTwentyPolicies(int pageSize, int pageNumber = 1, string sortProperty = null, string sortOrder = null)
        {
            var filters = new List<PropertyFilter>();

            filters.Add(new PropertyFilter("ExpirationDate", PropertyFilter.Comparator.GreaterEquals, DateTime.Now));
            filters.Add(new PropertyFilter("Status", PropertyFilter.Comparator.In, new[] { RiskStatus.BOUND.Value, RiskStatus.ISSUED.Value }));

            return base.GetPaginatedList<RiskGraph>(pageSize, pageNumber, "AnnualizedPremium", "desc", filters);
        }

        [HttpGet]
        [ActionName("GetPremiumImpactNotes")]
        public IEnumerable<dynamic> GetPremiumImpactNotes(DateTime startDate, DateTime endDate, int productLineId = 0)
        {
            return ServiceLocator.ConsoleService.GetImpactNotes(startDate, endDate, productLineId);
        }

        [HttpGet]
        [ActionName("GetWorkloadStatistics")]
        public List<dynamic> GetWorkloadStatistics(string productLine, int year)
        {
            return ServiceLocator.ReportingService.GetWorkloadStatistics(productLine, year);
        }
        [HttpGet]
        [ActionName("UnderwriterWorkloadRisksForProductLineForMonth")]
        public IEnumerable<RiskGraph> UnderwriterWorkloadRisksForProductLineForMonth(string underwriter, int year, int month, string productLine)
        {
            DateTime lastGeneratedRenewalDate = new DateTime(DateTime.Now.AddMonths(3).Year, DateTime.Now.AddMonths(3).Month, DateTime.DaysInMonth(DateTime.Now.AddMonths(3).Year, DateTime.Now.AddMonths(3).Month));
            DateTime firstDayOfSelecteMonth = new DateTime(year, month, DateTime.DaysInMonth(year, month));

            if (firstDayOfSelecteMonth > lastGeneratedRenewalDate)
                year -= 1;

            var filters = new List<PropertyFilter>()
            {
                new PropertyFilter("UW", underwriter),
                new PropertyFilter("EffectiveYear", year),
                new PropertyFilter("EffectiveMonth", month),
                new PropertyFilter("ProductLine", productLine),
                new PropertyFilter("Status", PropertyFilter.Comparator.In, new[] { RiskStatus.ISSUED.Value, RiskStatus.BOUND.Value })
            };

            return ServiceLocator.EntityService.GetList<RiskGraph>(filters);
        }

        [HttpGet]
        [ActionName("GetImpactfulEvents")]
        public IEnumerable<dynamic> GetImpactfulEvents(int productLineId)
        {
            return ServiceLocator.ConsoleService.GetImpactNotes(DateTime.Now.AddMonths(-12), DateTime.Now, productLineId);
        }
    }

    public class ClearanceController : BaseController
    {
        [HttpGet]
        [ActionName("GetClients")]
        public PaginatedList<NamedInsured> GetClients(int pageSize, int pageNumber = 1, string sortProperty = null, string sortOrder = null)
        {
            /*
             * Filter Groups
             *  0: common
             *  1: company name
             *  2: doing business as
             *  3: last name
             *  4: risk record
             */

            var criteria = base.GetPaginationCriteria<NamedInsured>(pageSize, pageNumber, sortProperty, sortOrder);
            var postValues = Request.GetQueryNameValuePairs();

            //Set to null if Name since the DB table doesn't actually have a field called name. Name is a generic to support either first and last or company
            if (sortProperty == "Name") sortProperty = null;

            var nameFilter = postValues.Where(x => x.Key == "Name").FirstOrDefault();
            var faaFilter = postValues.Where(x => x.Key == "FAANo").FirstOrDefault();
            var policyNumber = postValues.Where(x => x.Key == "PolicyNumber").FirstOrDefault();

            if (faaFilter.Key != null)
            {
                var tailNumber = (faaFilter.Value.StartsWith("N")) ? faaFilter.Value.Remove(0, 1) : faaFilter.Value;
                var aircraftFilters = new List<PropertyFilter>()
                {
                    new PropertyFilter(faaFilter.Key, PropertyFilter.Comparator.StartsWith, tailNumber),
                    new PropertyFilter(faaFilter.Key, PropertyFilter.Comparator.StartsWith, tailNumber.Insert(0, "N"), 1)
                };
                var aircraft = ServiceLocator.EntityService.GetList<NamedInsuredAircraft>(aircraftFilters);
                var controlNumbers = new List<int>();
                if (aircraft == null)
                    controlNumbers.Add(0);
                else
                    foreach (var ac in aircraft)
                        controlNumbers.Add(ac.ControlNumber);

                criteria.Filters.Add(new PropertyFilter("ControlNumber", PropertyFilter.Comparator.In, controlNumbers));
            }

            if (policyNumber.Key != null)
            {
                var risks = ServiceLocator.EntityService.GetList<RiskGraph>(new PropertyFilter(policyNumber.Key, PropertyFilter.Comparator.StartsWith, policyNumber.Value));
                var controlNumbers = (risks != null && risks.Count() > 0) ? risks.Select(x => x.ControlNumber) : new[] { 0 };
                criteria.Filters.Add(new PropertyFilter("ControlNumber", PropertyFilter.Comparator.In, controlNumbers));
            }

            //Remove the Name filter from the grid if it exists because this is a helper property only ment to combine the first and last names.
            criteria.Filters = criteria.Filters.Where(x => x.PropertyName != "Name").ToList();

            if (nameFilter.Key != null)
            {
                var nameArray = nameFilter.Value.Split(' ');
                var firstName = nameArray.First();
                var lastName = (nameArray.Length > 1) ? nameFilter.Value.Split(' ').Last() : null;
                var companyName = nameFilter.Value;
                var commonFilters = criteria.Filters.Take(criteria.Filters.Count()).ToList();
                var controlNumbers = GetControlNumbersFromRisks(firstName, lastName, nameFilter.Value);
                int maxGroup = 0;

                var firstNameFilterGroup = new List<PropertyFilter>();
                var lastNameFilterGroup = new List<PropertyFilter>();
                var personNameFilterGroup = new List<PropertyFilter>();
                var companyFilterGroup = new List<PropertyFilter>();
                var dbaFilterGroup = new List<PropertyFilter>();

                // If the name array length is only 1 then we lookup by first name, last name, and company name, doing business as
                if (nameArray.Length == 1)
                {
                    commonFilters.ForEach(x => firstNameFilterGroup.Add(new PropertyFilter(x.PropertyName, x.Operand, x.Value, 1)));
                    firstNameFilterGroup.Add(new PropertyFilter("FirstName", PropertyFilter.Comparator.StartsWith, firstName, 1));

                    commonFilters.ForEach(x => lastNameFilterGroup.Add(new PropertyFilter(x.PropertyName, x.Operand, x.Value, 2)));
                    lastNameFilterGroup.Add(new PropertyFilter("LastName", PropertyFilter.Comparator.Like, firstName, 2));

                    commonFilters.ForEach(x => companyFilterGroup.Add(new PropertyFilter(x.PropertyName, x.Operand, x.Value, 3)));
                    companyFilterGroup.Add(new PropertyFilter("CompanyName", PropertyFilter.Comparator.Like, companyName, 3));

                    commonFilters.ForEach(x => dbaFilterGroup.Add(new PropertyFilter(x.PropertyName, x.Operand, x.Value, 4)));
                    dbaFilterGroup.Add(new PropertyFilter("DoingBusinessAs", PropertyFilter.Comparator.Like, companyName, 4));
                    maxGroup = 4;
                }
                // If the name array length is greater than 2 we look up by company name and first and last combined. We will take the last array element as the last name and first array element as the first name
                else if (nameArray.Length > 1)
                {
                    commonFilters.ForEach(x => personNameFilterGroup.Add(new PropertyFilter(x.PropertyName, x.Operand, x.Value, 1)));
                    personNameFilterGroup.Add(new PropertyFilter("FirstName", PropertyFilter.Comparator.Like, firstName, 1));
                    personNameFilterGroup.Add(new PropertyFilter("LastName", PropertyFilter.Comparator.Like, lastName, 1));

                    commonFilters.ForEach(x => companyFilterGroup.Add(new PropertyFilter(x.PropertyName, x.Operand, x.Value, 2)));
                    companyFilterGroup.Add(new PropertyFilter("CompanyName", PropertyFilter.Comparator.Like, companyName, 2));

                    commonFilters.ForEach(x => dbaFilterGroup.Add(new PropertyFilter(x.PropertyName, x.Operand, x.Value, 3)));
                    dbaFilterGroup.Add(new PropertyFilter("DoingBusinessAs", PropertyFilter.Comparator.Like, companyName, 3));
                    maxGroup = 3;
                }

                criteria.Filters.AddRange(firstNameFilterGroup);
                criteria.Filters.AddRange(lastNameFilterGroup);
                criteria.Filters.AddRange(personNameFilterGroup);
                criteria.Filters.AddRange(companyFilterGroup);
                criteria.Filters.AddRange(dbaFilterGroup);

                //Lookup names directly from risk records.
                if (controlNumbers.FirstOrDefault() != 0) criteria.Filters.Add(new PropertyFilter("ControlNumber", PropertyFilter.Comparator.In, controlNumbers, maxGroup + 1));
            }

            var result = ServiceLocator.EntityService.GetPaginatedList<NamedInsured>(criteria);

            result.Results.OrderBy(x => x.FirstName).OrderBy(x => x.LastName).OrderBy(x => x.CompanyName);

            return result;
        }

        public IEnumerable<int> GetControlNumbersFromRisks(string firstName, string lastName, string companyName)
        {
            List<PropertyFilter> filters = new List<PropertyFilter>();
            filters.Add(new PropertyFilter("FirstName", PropertyFilter.Comparator.Like, firstName));
            if (!string.IsNullOrEmpty(lastName))
                filters.Add(new PropertyFilter("LastName", PropertyFilter.Comparator.Like, lastName));

            filters.Add(new PropertyFilter("CompanyName", PropertyFilter.Comparator.Like, companyName, 1));
            filters.Add(new PropertyFilter("DoingBusinessAs", PropertyFilter.Comparator.Like, companyName, 2));

            IEnumerable<Risk> risks = ServiceLocator.EntityService.GetList<Risk>(filters);

            return risks.Select(x => x.ControlNumber);
        }
    }
    
    public class UserController : BaseController
    {
        [HttpGet]
        [ActionName("GetCurrentUser")]
        public object GetCurrentUser()
        {
            var user = SessionManager.GetCurrentSession().User;
            return new {
                Branch = StringEnum.GetAll<Branch>().Where(x => x.Value == user.BranchID).First(),
                UserId = user.UserId,
                UserName = user.UserName,
                FirstName = user.FirstName, LastName = user.LastName, Name = user.FirstName + ' ' + user.LastName,
                ProductLine = user.ProductLine
            };
        }

        [HttpGet]
        [ActionName("GetWorkingListConfigurations")]
        public PaginatedList<WorkingListGridConfig> GetWorkingListConfigurations(int pageSize, int pageNumber = 1, string sortProperty = null, string sortOrder = null)
        {
            return base.GetPaginatedList<WorkingListGridConfig>(pageSize, pageNumber, sortProperty, sortOrder, new PropertyFilter("UserId", SessionManager.GetCurrentSession().User.UserId));
        }

        [HttpDelete]
        [ActionName("DeleteGridConfig")]
        public HttpResponseMessage DeleteGridConfig(int id)
        {
            ServiceLocator.SystemSvc.DeleteUserGridConfig(id);
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }

        [HttpGet]
        [ActionName("GetUnderwriters")]
        public IEnumerable<dynamic> GetUnderwriters()
        {
            return ServiceLocator.RiskService.GetUnderwriters();
        }
    }

    public class ComplianceController : BaseController
    {
        /// <summary>
        /// Data Tranfer Object to support a PUT or POST method type for the necessary data points needed to send a Licensing Email to compliance.
        /// </summary>
        public class LicenseNotificationDTO
        {
            public string AgencyId { get; set; }
            public string State { get; set; }
            public int RiskId { get; set; }
        }

        /// <summary>
        /// Issue an email to the licenseing department to let them know there is an issue with the brokers state licensing.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpGet]
        [AcceptVerbs("GET")]
        [ActionName("LicenseNotification")]
        public HttpResponseMessage LicenseNotification(string state, string agencyId, int riskId, string airportId)
        {
            //Check to send the correct agency or agent email notification
            var IsLicensed = ServiceLocator.BrokerSvc.IsBrokerLicensed(agencyId, state, riskId, true);
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }
    }
}