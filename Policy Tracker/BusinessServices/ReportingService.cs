using PolicyTracker.BusinessServices.Risks;
using PolicyTracker.DataAccess;
using PolicyTracker.DataAccess.Policy;
using PolicyTracker.DomainModel.Common;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.DomainModel.Policy;
using PolicyTracker.DomainModel.Reports;
using PolicyTracker.Platform.UOW;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;

namespace PolicyTracker.BusinessServices
{
    public class ReportingService
    {
        public IEnumerable<dynamic> GetUWInforcePolicyCount(string branch = null)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var result = DAOFactory.GetDAO<RiskGraphDAO>().GetUWInforcePolicyCount(uow, branch);
            return result;
        }

        public IEnumerable<dynamic> GetProductLineInforceAmounts()
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var result = DAOFactory.GetDAO<RiskGraphDAO>().GetProductLineInforceAmounts(uow);
            return result;
        }

        public List<RiskGraph> GetUnResolvedRenewalsReportData()
        {
            var orderFilters = new List<OrderFilter>();
            var filters = new List<PropertyFilter>();

            orderFilters.Add(new OrderFilter("Status"));
            orderFilters.Add(new OrderFilter("Branch"));
            orderFilters.Add(new OrderFilter("EffectiveDate"));

            filters.Add(new PropertyFilter("Status", PropertyFilter.Comparator.In, new[] { RiskStatus.SUBMISSION.DisplayText, RiskStatus.QUOTE.Value, RiskStatus.BOUND.Value }));
            filters.Add(new PropertyFilter("EffectiveDate", PropertyFilter.Comparator.Less, DateTime.Now.ToShortDateString()));

            var results = ServiceLocator.EntityService.GetList<RiskGraph>(filters, orderFilters).ToList();
            return results;
        }

        public List<RiskGraph> GetTargetAccountsReportData()
        {
            var filters = new List<PropertyFilter>();
            filters.Add(new PropertyFilter("IsTargetAccount", true));
            return ServiceLocator.EntityService.GetList<RiskGraph>(filters).ToList();;
        }

        public List<RiskGraph> GetMonthlyExpiringAccounts(int month)
        {
            var filters = new List<PropertyFilter>()
            {
                new PropertyFilter("EffectiveMonth", month),
                new PropertyFilter("EffectiveYear", DateTime.Now.Year),
                new PropertyFilter("Status", PropertyFilter.Comparator.NotIn, new[] { RiskStatus.INVOLVED.Value }),
                new PropertyFilter("IsRenewal", true)
            };

            var risks = ServiceLocator.EntityService.GetList<RiskGraph>(filters).ToList();

            return risks;
        }

        #region NBF By Branch
        public List<NewBusinessFlow> GetNBFByBranch(int year, int month, int underwriterId = 0)
        {
            var priorYear = year - 1;
            var results = new List<NewBusinessFlow>();

            var filters = new List<PropertyFilter>();
            filters.Add(new PropertyFilter("EffectiveYear", PropertyFilter.Comparator.In, new[] { year, priorYear }));
            filters.Add(new PropertyFilter("EffectiveMonth", month));
            if (underwriterId != 0)
                filters.Add(new PropertyFilter("UnderwriterId", underwriterId));

            var NBFData = ServiceLocator.EntityService.GetList<NewBusinessFlow>(filters, new OrderFilter("RiskStatus", OrderFilter.Comparator.Descending)).ToList();
            
            var groups = from row in NBFData group row by new { row.Branch } into grp select grp.First();

            foreach (var group in groups)
            {
                results.Add(NBFCalculateForBranch(year, priorYear, NBFData, false, group.Branch));
            }

            return results;
        }

        public List<NewBusinessFlow> GetYTDNBFByBranch(int year, string underwriter)
        {
            var priorYear = year - 1;
            var results = new List<NewBusinessFlow>();
            var commonFilters = new List<PropertyFilter>();
            var filters = new List<PropertyFilter>()
            {
                new PropertyFilter("EffectiveYear", PropertyFilter.Comparator.In, new[] { year, priorYear })
            };

            if (year == DateTime.Now.Year)
                filters.Add(new PropertyFilter("EffectiveDate", new DateTime(DateTime.Now.Year, 1, 1).ToShortDateString(), DateTime.Now.ToShortDateString()));
            if (!String.IsNullOrEmpty(underwriter))
                filters.Add(new PropertyFilter("UW", underwriter));

            filters.ForEach(x => commonFilters.Add(
                new PropertyFilter(x.PropertyName, x.Operand, x.Value, 1)
                {
                    PropertyName = x.PropertyName, Group = 1, Operand = x.Operand, Value = x.Value, Value2 = x.Value2
                }));
            commonFilters.RemoveAll(x => x.PropertyName == "EffectiveDate");
            commonFilters.Add(new PropertyFilter("EffectiveDate", new DateTime(DateTime.Now.AddYears(-1).Year, 1, 1).ToShortDateString(), DateTime.Now.AddYears(-1).ToShortDateString(), 1));
            filters.AddRange(commonFilters);

            var NBFData = ServiceLocator.EntityService.GetList<NewBusinessFlow>(filters).ToList();
            var groups = from row in NBFData group row by new { row.Branch } into grp select grp.First();

            foreach (var group in groups)
            {
                results.Add(NBFCalculateForBranch(year, priorYear, NBFData, true, group.Branch));
            }
            
            return results;
        }

        public NewBusinessFlow NBFCalculateForBranch(int year, int priorYear, IEnumerable<NewBusinessFlow> data, bool isYTD, string branch = null)
        {
            NewBusinessFlow row = new NewBusinessFlow();
            IEnumerable<NewBusinessFlow> monthBranchData;

            if (branch != null)
            {
                monthBranchData = data.Where(x => x.Branch == branch);
                row.Branch = branch;
            } 
            else
            {
                monthBranchData = data;
                row.Branch = "* GRAND TOTAL *";
            }
            

            row.LowYearSubmissions = monthBranchData.Where(x => x.EffectiveYear == priorYear && x.RiskStatus == "Submissions").Sum(x => x.Total);
            row.UpperYearSubmissions = monthBranchData.Where(x => x.EffectiveYear == year && x.RiskStatus == "Submissions").Sum(x => x.Total);
            row.SubmissionVariance = row.UpperYearSubmissions - row.LowYearSubmissions;

            row.LowYearQuotes = monthBranchData.Where(x => x.EffectiveYear == priorYear && x.RiskStatus == "Quotes").Sum(x => x.Total);
            row.UpperYearQuotes = monthBranchData.Where(x => x.EffectiveYear == year && x.RiskStatus == "Quotes").Sum(x => x.Total);
            row.QuoteVariance = row.UpperYearQuotes - row.LowYearQuotes;

            row.LowYearPolicies = monthBranchData.Where(x => x.EffectiveYear == priorYear && x.RiskStatus == "Policies").Sum(x => x.Total);
            row.UpperYearPolicies = monthBranchData.Where(x => x.EffectiveYear == year && x.RiskStatus == "Policies").Sum(x => x.Total);
            row.PolicyVariance = row.UpperYearPolicies - row.LowYearPolicies;

            row.LowYearQuoteRatio = (row.LowYearSubmissions != 0) ? (Convert.ToDecimal(row.LowYearQuotes) / Convert.ToDecimal(row.LowYearSubmissions)) * 100 : 0;
            row.UpperYearQuoteRatio = (row.UpperYearSubmissions != 0) ? (Convert.ToDecimal(row.UpperYearQuotes) / Convert.ToDecimal(row.UpperYearSubmissions)) * 100 : 0;
            row.QuoteRatioVariance = row.UpperYearQuoteRatio - row.LowYearQuoteRatio;

            row.LowYearHitRatio = (row.LowYearQuotes != 0) ? (Convert.ToDecimal(row.LowYearPolicies) / Convert.ToDecimal(row.LowYearQuotes)) * 100 : 0;
            row.UpperYearHitRatio = (row.UpperYearQuotes != 0) ? (Convert.ToDecimal(row.UpperYearPolicies) / Convert.ToDecimal(row.UpperYearQuotes)) * 100 : 0;
            row.HitRatioVariance = row.UpperYearHitRatio - row.LowYearHitRatio;

            row.LowYearWrittenRatio = (row.LowYearSubmissions != 0) ? (Convert.ToDecimal(row.LowYearPolicies) / Convert.ToDecimal(row.LowYearSubmissions)) * 100 : 0;
            row.UpperYearWrittenRatio = (row.UpperYearSubmissions != 0) ? (Convert.ToDecimal(row.UpperYearPolicies) / Convert.ToDecimal(row.UpperYearSubmissions)) * 100 : 0;
            row.WrittenRatioVariance = row.UpperYearWrittenRatio - row.LowYearWrittenRatio;

            if (!isYTD)
            {
                row.LowYearQuotedPremium = monthBranchData.Where(x => x.EffectiveYear == priorYear && x.RiskStatus == "Quotes").Sum(x => x.TotalPremium);
                row.UpperYearQuotedPremium = monthBranchData.Where(x => x.EffectiveYear == year && x.RiskStatus == "Quotes").Sum(x => x.TotalPremium);
                row.QuotedPremiumVariance = row.UpperYearQuotedPremium - row.LowYearQuotedPremium;
            }

            row.LowYearWrittenPremium = monthBranchData.Where(x => x.EffectiveYear == priorYear && x.RiskStatus == "Policies").Sum(x => x.TotalPremium);
            row.UpperYearWrittenPremium = monthBranchData.Where(x => x.EffectiveYear == year && x.RiskStatus == "Policies").Sum(x => x.TotalPremium);
            row.WrittenPremiumVariance = row.UpperYearWrittenPremium - row.LowYearWrittenPremium;

            return row;
        }
        #endregion

        #region NBF By Branch & UW/Branch & Month
        public List<NewBusinessFlow> GetNBFByBranchAndUW(int year, int month, int underwriterId = 0)
        {
            var priorYear = year - 1;
            var results = new List<NewBusinessFlow>();

            var filters = new List<PropertyFilter>();
            filters.Add(new PropertyFilter("EffectiveYear", PropertyFilter.Comparator.In, new[] { year, priorYear }));
            filters.Add(new PropertyFilter("EffectiveMonth", month));
            if (underwriterId != 0)
                filters.Add(new PropertyFilter("UnderwriterId", underwriterId));

            var NBFData = ServiceLocator.EntityService.GetList<NewBusinessFlow>(filters, new OrderFilter("RiskStatus", OrderFilter.Comparator.Descending)).ToList();

            var groups = from row in NBFData group row by new { row.Branch, row.UW } into grp select grp.First();

            foreach (var group in groups)
            {
                NewBusinessFlow row = new NewBusinessFlow();
                IEnumerable<NewBusinessFlow> monthBranchData = NBFData.Where(x => x.UW == group.UW && x.Branch == group.Branch);
                row.Branch = group.Branch;
                row.UW = group.UW;

                row.LowYearSubmissions = monthBranchData.Where(x => x.EffectiveYear == priorYear && x.RiskStatus == "Submissions").Sum(x => x.Total);
                row.UpperYearSubmissions = monthBranchData.Where(x => x.EffectiveYear == year && x.RiskStatus == "Submissions").Sum(x => x.Total);
                row.SubmissionVariance = row.UpperYearSubmissions - row.LowYearSubmissions;

                row.LowYearQuotes = monthBranchData.Where(x => x.EffectiveYear == priorYear && x.RiskStatus == "Quotes").Sum(x => x.Total);
                row.UpperYearQuotes = monthBranchData.Where(x => x.EffectiveYear == year && x.RiskStatus == "Quotes").Sum(x => x.Total);
                row.QuoteVariance = row.UpperYearQuotes - row.LowYearQuotes;

                row.LowYearPolicies = monthBranchData.Where(x => x.EffectiveYear == priorYear && x.RiskStatus == "Policies").Sum(x => x.Total);
                row.UpperYearPolicies = monthBranchData.Where(x => x.EffectiveYear == year && x.RiskStatus == "Policies").Sum(x => x.Total);
                row.PolicyVariance = row.UpperYearPolicies - row.LowYearPolicies;

                row.LowYearQuoteRatio = (row.LowYearSubmissions != 0) ? (Convert.ToDecimal(row.LowYearQuotes) / Convert.ToDecimal(row.LowYearSubmissions)) * 100 : 0;
                row.UpperYearQuoteRatio = (row.UpperYearSubmissions != 0) ? (Convert.ToDecimal(row.UpperYearQuotes) / Convert.ToDecimal(row.UpperYearSubmissions)) * 100 : 0;
                row.QuoteRatioVariance = row.UpperYearQuoteRatio - row.LowYearQuoteRatio;

                row.LowYearHitRatio = (row.LowYearQuotes != 0) ? (Convert.ToDecimal(row.LowYearPolicies) / Convert.ToDecimal(row.LowYearQuotes)) * 100 : 0;
                row.UpperYearHitRatio = (row.UpperYearQuotes != 0) ? (Convert.ToDecimal(row.UpperYearPolicies) / Convert.ToDecimal(row.UpperYearQuotes)) * 100 : 0;
                row.HitRatioVariance = row.UpperYearHitRatio - row.LowYearHitRatio;

                row.LowYearWrittenRatio = (row.LowYearSubmissions != 0) ? (Convert.ToDecimal(row.LowYearPolicies) / Convert.ToDecimal(row.LowYearSubmissions)) * 100 : 0;
                row.UpperYearWrittenRatio = (row.UpperYearSubmissions != 0) ? (Convert.ToDecimal(row.UpperYearPolicies) / Convert.ToDecimal(row.UpperYearSubmissions)) * 100 : 0;
                row.WrittenRatioVariance = row.UpperYearWrittenRatio - row.LowYearWrittenRatio;

                row.LowYearQuotedPremium = monthBranchData.Where(x => x.EffectiveYear == priorYear && x.RiskStatus == "Quotes").Sum(x => x.TotalPremium);
                row.UpperYearQuotedPremium = monthBranchData.Where(x => x.EffectiveYear == year && x.RiskStatus == "Quotes").Sum(x => x.TotalPremium);
                row.QuotedPremiumVariance = row.UpperYearQuotedPremium - row.LowYearQuotedPremium;

                row.LowYearWrittenPremium = monthBranchData.Where(x => x.EffectiveYear == priorYear && x.RiskStatus == "Policies").Sum(x => x.TotalPremium);
                row.UpperYearWrittenPremium = monthBranchData.Where(x => x.EffectiveYear == year && x.RiskStatus == "Policies").Sum(x => x.TotalPremium);
                row.WrittenPremiumVariance = row.UpperYearWrittenPremium - row.LowYearWrittenPremium;

                results.Add(row);
            }

            return results;
        }

        public List<NewBusinessFlow> GetYTDNBFByBranchAndMonth(int year, string underwriter)
        {
            var priorYear = year - 1;
            var results = new List<NewBusinessFlow>();

            var filters = new List<PropertyFilter>()
            {
                new PropertyFilter("EffectiveYear", PropertyFilter.Comparator.In, new[] { year, priorYear })
            };

            if (year == DateTime.Now.Year)
            {
                filters.Add(new PropertyFilter("EffectiveMonth", PropertyFilter.Comparator.LessEquals, DateTime.Now.Month));
                filters.Add(new PropertyFilter("EffectiveDay", PropertyFilter.Comparator.LessEquals, DateTime.Now.Day));
            }
            if (!String.IsNullOrEmpty(underwriter))
                filters.Add(new PropertyFilter("UW", underwriter));

            var NBFData = ServiceLocator.EntityService.GetList<NewBusinessFlow>(filters).ToList();
            
            var groups = from row in NBFData group row by new { row.Branch, row.EffectiveMonth } into grp select grp.First();

            foreach (var group in groups)
            {
                results.Add(NBFCalculateForBranchAndMonth(year, priorYear, NBFData, group.EffectiveMonth,  group.Branch, true));
            }

            return results;
        }

        public NewBusinessFlow NBFCalculateForBranchAndMonth(int year, int priorYear, IEnumerable<NewBusinessFlow> data, int month, string branch, bool isYTD)
        {
            NewBusinessFlow row = new NewBusinessFlow();
            IEnumerable<NewBusinessFlow> monthBranchData = data.Where(x => x.EffectiveMonth == month && x.Branch == branch);
            row.EffectiveMonth = month;
            row.Branch = branch;

            row.LowYearSubmissions = monthBranchData.Where(x => x.EffectiveYear == priorYear && x.RiskStatus == "Submissions" && x.EffectiveMonth == month).Sum(x => x.Total);
            row.UpperYearSubmissions = monthBranchData.Where(x => x.EffectiveYear == year && x.RiskStatus == "Submissions" && x.EffectiveMonth == month).Sum(x => x.Total);
            row.SubmissionVariance = row.UpperYearSubmissions - row.LowYearSubmissions;

            row.LowYearQuotes = monthBranchData.Where(x => x.EffectiveYear == priorYear && x.RiskStatus == "Quotes" && x.EffectiveMonth == month).Sum(x => x.Total);
            row.UpperYearQuotes = monthBranchData.Where(x => x.EffectiveYear == year && x.RiskStatus == "Quotes" && x.EffectiveMonth == month).Sum(x => x.Total);
            row.QuoteVariance = row.UpperYearQuotes - row.LowYearQuotes;

            row.LowYearPolicies = monthBranchData.Where(x => x.EffectiveYear == priorYear && x.RiskStatus == "Policies" && x.EffectiveMonth == month).Sum(x => x.Total);
            row.UpperYearPolicies = monthBranchData.Where(x => x.EffectiveYear == year && x.RiskStatus == "Policies" && x.EffectiveMonth == month).Sum(x => x.Total);
            row.PolicyVariance = row.UpperYearPolicies - row.LowYearPolicies;

            row.LowYearQuoteRatio = (row.LowYearSubmissions != 0) ? (Convert.ToDecimal(row.LowYearQuotes) / Convert.ToDecimal(row.LowYearSubmissions)) * 100 : 0;
            row.UpperYearQuoteRatio = (row.UpperYearSubmissions != 0) ? (Convert.ToDecimal(row.UpperYearQuotes) / Convert.ToDecimal(row.UpperYearSubmissions)) * 100 : 0;
            row.QuoteRatioVariance = row.UpperYearQuoteRatio - row.LowYearQuoteRatio;

            row.LowYearHitRatio = (row.LowYearQuotes != 0) ? (Convert.ToDecimal(row.LowYearPolicies) / Convert.ToDecimal(row.LowYearQuotes)) * 100 : 0;
            row.UpperYearHitRatio = (row.UpperYearQuotes != 0) ? (Convert.ToDecimal(row.UpperYearPolicies) / Convert.ToDecimal(row.UpperYearQuotes)) * 100 : 0;
            row.HitRatioVariance = row.UpperYearHitRatio - row.LowYearHitRatio;

            row.LowYearWrittenRatio = (row.LowYearSubmissions != 0) ? (Convert.ToDecimal(row.LowYearPolicies) / Convert.ToDecimal(row.LowYearSubmissions)) * 100 : 0;
            row.UpperYearWrittenRatio = (row.UpperYearSubmissions != 0) ? (Convert.ToDecimal(row.UpperYearPolicies) / Convert.ToDecimal(row.UpperYearSubmissions)) * 100 : 0;
            row.WrittenRatioVariance = row.UpperYearWrittenRatio - row.LowYearWrittenRatio;

            if (!isYTD)
            {
                row.LowYearQuotedPremium = monthBranchData.Where(x => x.EffectiveYear == priorYear && x.RiskStatus == "Quotes" && x.EffectiveMonth == month).Sum(x => x.TotalPremium);
                row.UpperYearQuotedPremium = monthBranchData.Where(x => x.EffectiveYear == year && x.RiskStatus == "Quotes" && x.EffectiveMonth == month).Sum(x => x.TotalPremium);
                row.QuotedPremiumVariance = row.UpperYearQuotedPremium - row.LowYearQuotedPremium;
            }

            row.LowYearWrittenPremium = monthBranchData.Where(x => x.EffectiveYear == priorYear && x.RiskStatus == "Policies" && x.EffectiveMonth == month).Sum(x => x.TotalPremium);
            row.UpperYearWrittenPremium = monthBranchData.Where(x => x.EffectiveYear == year && x.RiskStatus == "Policies" && x.EffectiveMonth == month).Sum(x => x.TotalPremium);
            row.WrittenPremiumVariance = row.UpperYearWrittenPremium - row.LowYearWrittenPremium;

            return row;
        }
        #endregion

        #region NBF By Product Line
        public List<NewBusinessFlow> GetNBFByProductLine(int year, int month, int underwriterId = 0)
        {
            var priorYear = year - 1;
            var results = new List<NewBusinessFlow>();

            var filters = new List<PropertyFilter>();
            filters.Add(new PropertyFilter("EffectiveYear", PropertyFilter.Comparator.In, new[] { year, priorYear }));
            filters.Add(new PropertyFilter("EffectiveMonth", month));
            if (underwriterId != 0)
                filters.Add(new PropertyFilter("UnderwriterId", underwriterId));

            var NBFData = ServiceLocator.EntityService.GetList<NewBusinessFlow>(filters, new OrderFilter("RiskStatus", OrderFilter.Comparator.Descending)).ToList();

            var groups = from row in NBFData group row by new { row.ProductLine } into grp select grp.First();

            foreach (var group in groups)
            {
                results.Add(NBFCalculateForProductLine(year, priorYear, NBFData, false, group.ProductLine));
            }

            return results;
        }

        public List<NewBusinessFlow> GetYTDNewBusinessFlowReportProductLineData(int year, string underwriter)
        {
            var priorYear = year - 1;
            var results = new List<NewBusinessFlow>();

            var filters = new List<PropertyFilter>()
            {
                new PropertyFilter("EffectiveYear", PropertyFilter.Comparator.In, new[] { year, priorYear })
            };

            if (year == DateTime.Now.Year)
            {
                filters.Add(new PropertyFilter("EffectiveMonth", PropertyFilter.Comparator.LessEquals, DateTime.Now.Month));
                filters.Add(new PropertyFilter("EffectiveDay", PropertyFilter.Comparator.LessEquals, DateTime.Now.Day));
            }
                
            if (!String.IsNullOrEmpty(underwriter))
                filters.Add(new PropertyFilter("UW", underwriter));

            var NBFData = ServiceLocator.EntityService.GetList<NewBusinessFlow>(filters).ToList();

            var groups = from row in NBFData group row by new { row.ProductLine } into grp select grp.First();

            foreach (var group in groups)
            {
                results.Add(NBFCalculateForProductLine(year, priorYear, NBFData, true, group.ProductLine));
            }

            return results;
        }

        public NewBusinessFlow NBFCalculateForProductLine(int year, int priorYear, IEnumerable<NewBusinessFlow> data, bool isYTD, string productLine = null)
        {
            NewBusinessFlow row = new NewBusinessFlow();
            IEnumerable<NewBusinessFlow> dataSubset;

            if(productLine != null)
            {
                dataSubset = data.Where(x => x.ProductLine == productLine);
                row.ProductLine = productLine;
            }
            else
            {
                dataSubset = data;
                row.ProductLine = "* GRAND TOTAL *";
            }

            row.LowYearSubmissions = dataSubset.Where(x => x.EffectiveYear == priorYear && x.RiskStatus == "Submissions").Sum(x => x.Total);
            row.UpperYearSubmissions = dataSubset.Where(x => x.EffectiveYear == year && x.RiskStatus == "Submissions").Sum(x => x.Total);
            row.SubmissionVariance = row.UpperYearSubmissions - row.LowYearSubmissions;

            row.LowYearQuotes = dataSubset.Where(x => x.EffectiveYear == priorYear && x.RiskStatus == "Quotes").Sum(x => x.Total);
            row.UpperYearQuotes = dataSubset.Where(x => x.EffectiveYear == year && x.RiskStatus == "Quotes").Sum(x => x.Total);
            row.QuoteVariance = row.UpperYearQuotes - row.LowYearQuotes;

            row.LowYearPolicies = dataSubset.Where(x => x.EffectiveYear == priorYear && x.RiskStatus == "Policies").Sum(x => x.Total);
            row.UpperYearPolicies = dataSubset.Where(x => x.EffectiveYear == year && x.RiskStatus == "Policies").Sum(x => x.Total);
            row.PolicyVariance = row.UpperYearPolicies - row.LowYearPolicies;

            row.LowYearQuoteRatio = (row.LowYearSubmissions != 0) ? (Convert.ToDecimal(row.LowYearQuotes) / Convert.ToDecimal(row.LowYearSubmissions)) * 100 : 0;
            row.UpperYearQuoteRatio = (row.UpperYearSubmissions != 0) ? (Convert.ToDecimal(row.UpperYearQuotes) / Convert.ToDecimal(row.UpperYearSubmissions)) * 100 : 0;
            row.QuoteRatioVariance = row.UpperYearQuoteRatio - row.LowYearQuoteRatio;

            row.LowYearHitRatio = (row.LowYearQuotes != 0) ? (Convert.ToDecimal(row.LowYearPolicies) / Convert.ToDecimal(row.LowYearQuotes)) * 100 : 0;
            row.UpperYearHitRatio = (row.UpperYearQuotes != 0) ? (Convert.ToDecimal(row.UpperYearPolicies) / Convert.ToDecimal(row.UpperYearQuotes)) * 100 : 0;
            row.HitRatioVariance = row.UpperYearHitRatio - row.LowYearHitRatio;

            row.LowYearWrittenRatio = (row.LowYearSubmissions != 0) ? (Convert.ToDecimal(row.LowYearPolicies) / Convert.ToDecimal(row.LowYearSubmissions)) * 100 : 0;
            row.UpperYearWrittenRatio = (row.UpperYearSubmissions != 0) ? (Convert.ToDecimal(row.UpperYearPolicies) / Convert.ToDecimal(row.UpperYearSubmissions)) * 100 : 0;
            row.WrittenRatioVariance = row.UpperYearWrittenRatio - row.LowYearWrittenRatio;

            if (!isYTD)
            {
                row.LowYearQuotedPremium = dataSubset.Where(x => x.EffectiveYear == priorYear && x.RiskStatus == "Quotes").Sum(x => x.TotalPremium);
                row.UpperYearQuotedPremium = dataSubset.Where(x => x.EffectiveYear == year && x.RiskStatus == "Quotes").Sum(x => x.TotalPremium);
                row.QuotedPremiumVariance = row.UpperYearQuotedPremium - row.LowYearQuotedPremium;
            }

            row.LowYearWrittenPremium = dataSubset.Where(x => x.EffectiveYear == priorYear && x.RiskStatus == "Policies").Sum(x => x.TotalPremium);
            row.UpperYearWrittenPremium = dataSubset.Where(x => x.EffectiveYear == year && x.RiskStatus == "Policies").Sum(x => x.TotalPremium);
            row.WrittenPremiumVariance = row.UpperYearWrittenPremium - row.LowYearWrittenPremium;

            return row;
        }
        #endregion

        public List<PredictivePremiumDataPoint> GetPredictivePremiumReportData(int month)
        {
            var filters = new List<PropertyFilter>();
            filters.Add(new PropertyFilter("EffectiveMonth", PropertyFilter.Comparator.Equals, month));
            filters.Add(new PropertyFilter("EffectiveYear", PropertyFilter.Comparator.Equals, DateTime.Now.Year));
            filters.Add(new PropertyFilter("ExpirationMonth", PropertyFilter.Comparator.Equals, month, 1));
            filters.Add(new PropertyFilter("ExpirationYear", PropertyFilter.Comparator.Equals, DateTime.Now.Year, 1));
            var data = ServiceLocator.EntityService.GetList<PredictivePremiumDataPoint>(filters);
            var reportData = new List<PredictivePremiumDataPoint>();

            //var g = NBFData.GroupBy(
            var groups = from row in data
                         group row by new
                         {
                             row.Branch,
                             row.ProductLine,
                             row.UW
                         } into grp
                         select grp.First();

            foreach (var group in groups)
            {
                PredictivePremiumDataPoint row = new PredictivePremiumDataPoint();
                IEnumerable<PredictivePremiumDataPoint> groupData = data.Where(x => x.Branch == group.Branch && x.UW == group.UW && x.ProductLine == group.ProductLine);
                row.Branch = group.Branch;
                row.ProductLine = group.ProductLine;
                row.UW = group.UW;

                row.NBQuotedPremium = groupData.Where(x => x.EffectiveMonth == month && x.EffectiveYear == DateTime.Now.Year && x.Status == "Quote" || x.Status == "Submission" && !x.IsRenewal).Sum(x => x.Premium);
                row.TotalNBQuoted = groupData.Where(x => x.EffectiveMonth == month && x.EffectiveYear == DateTime.Now.Year && x.Status == "Quote" || x.Status == "Submission" && !x.IsRenewal).Count();
                row.NBWrittenPremium = groupData.Where(x => x.EffectiveMonth == month && x.EffectiveYear == DateTime.Now.Year && x.Status == "Bound" || x.Status == "Issued" && !x.IsRenewal).Sum(x => x.Premium);
                row.TotalWritten = groupData.Where(x => x.EffectiveMonth == month && x.EffectiveYear == DateTime.Now.Year && x.Status == "Bound" || x.Status == "Issued" && !x.IsRenewal).Count();

                row.PremiumAvailableForRenewal = groupData.Where(x => x.ExpirationMonth == month && x.ExpirationYear == DateTime.Now.Year && !x.Renewed).Sum(x => x.Premium);
                row.TotalAvailable = groupData.Where(x => x.ExpirationMonth == month && x.ExpirationYear == DateTime.Now.Year && !x.Renewed).Count();
                row.PremiumRenewed = groupData.Where(x => x.ExpirationMonth == month && x.ExpirationYear == DateTime.Now.Year && x.Renewed).Sum(x => x.RenewedPremium);
                row.TotalRenewed = groupData.Where(x => x.ExpirationMonth == month && x.ExpirationYear == DateTime.Now.Year && x.Renewed).Count();
                
                row.PredictedPremium = (row.NBQuotedPremium * .2m) + row.NBWrittenPremium + row.PremiumAvailableForRenewal + row.PremiumRenewed;

                reportData.Add(row);
            }

            return reportData;
        }

        /// <summary>
        /// Builds the necessary property filters for fetching risks for the product line console. These filters should be able to fetch all of the currently existing records that the Policy Tracker
        /// will have already created through the Generate Renewals process. The filters should get both the new and renewal records.
        /// </summary>
        /// <param name="productLine">Product Line</param>
        /// <param name="year">Year</param>
        /// <param name="month">Month</param>
        /// <param name="underwriter">Underwriter Id</param>
        /// <returns></returns>
        private List<PropertyFilter> GetProductLineDetailFilters(string productLine, int year, int month = 0, int underwriter = 0, string branch = null)
        {
            bool isYearSummary = (month == 0);
            var filters = new List<PropertyFilter>()
            {
                new PropertyFilter("ProductLine", productLine),
                new PropertyFilter("EffectiveYear", year)
            };

            if (underwriter != 0)
                filters.Add(new PropertyFilter("UnderwriterId", underwriter));

            if (!String.IsNullOrEmpty(branch))
                filters.Add(new PropertyFilter("Branch", branch));

            if (isYearSummary) //Year Summary
            {
                //Current Year
                if (year == DateTime.Now.Year)
                {
                    if (DateTime.Now.AddMonths(3).Year != year || DateTime.Now.AddMonths(3).Month == 12) // We are within 3 months of next year. Therefore all records have been generated for the year
                        filters.Add(new PropertyFilter("EffectiveMonth", PropertyFilter.Comparator.LessEquals, 12));
                    else // Not all records have been generated yet this year. Only get what has been generated
                        filters.Add(new PropertyFilter("EffectiveMonth", PropertyFilter.Comparator.LessEquals, DateTime.Now.AddMonths(3).Month));
                }
                // Future or past year. If a past year everything is simple all the records needed will exist.
                else
                {
                    if(year < DateTime.Now.Year) //Past Year
                        filters.Add(new PropertyFilter("EffectiveMonth", PropertyFilter.Comparator.LessEquals, 12));
                    else //Future year
                    {
                        //We are within 3 months of the next year so some risk records will already exist.
                        if (DateTime.Now.AddMonths(3).Year == year)
                        {
                            filters.Add(new PropertyFilter("EffectiveMonth", PropertyFilter.Comparator.LessEquals, DateTime.Now.AddMonths(3).Month));
                        }
                    }
                }
            }
            else // For a specific Month
            {
                filters.Add(new PropertyFilter("EffectiveMonth", PropertyFilter.Comparator.LessEquals, month));
            }

            return filters;
        }

        public dynamic GetWorkloadStatistics(string productLine, int year)
        {
            var results = new List<dynamic>();
            List<PropertyFilter> filters = GetProductLineDetailFilters(productLine, year);
            List<RiskGraph> risks = ServiceLocator.EntityService.GetList<RiskGraph>(filters).ToList();
            List<RiskGraph> additionalForecastingRisks = new List<RiskGraph>();

            #region Additional Risk Selection Logic
            if (DateTime.Now.Year == year) // Displaying the current year
            {
                // We are within 3 months of the next year so all records for this year have been generated
                if (DateTime.Now.AddMonths(3).Year != year || DateTime.Now.AddMonths(3).Month == 12) { }
                else
                {
                    DateTime startDate = new DateTime(year - 1, DateTime.Now.AddMonths(4).Month, 1);
                    DateTime endDate = new DateTime(year - 1, 12, DateTime.DaysInMonth(year - 1, 12));

                    filters.RemoveAll(x => x.PropertyName == "EffectiveYear");
                    filters.RemoveAll(x => x.PropertyName == "EffectiveMonth");
                    filters.Add(new PropertyFilter("EffectiveDate", PropertyFilter.Comparator.Between, startDate, endDate));
                    filters.Add(new PropertyFilter("Status", RiskStatus.ISSUED.Value));

                    additionalForecastingRisks = ServiceLocator.EntityService.GetList<RiskGraph>(filters).ToList();
                    additionalForecastingRisks.Select(o => { o.Status = RiskStatus.SUBMISSION.Value; o.InceptionPremium = o.AnnualizedPremium; return o; }).ToList(); // Set all future stubs to submission so they dont get counted as bound.
                    risks.AddRange(additionalForecastingRisks);
                }
            }
            else // Displaying either a past or future year
            {
                // We are displaying a past year. Therefore all records will exist that we need
                if (year < DateTime.Now.Year) { }
                else // We are displaying a future year
                {
                    //We are within 3 months of the following year so we have generated at least some of next years renewals.
                    //This means we will need the records from the current year for the remaining months that have not already been generated.
                    if (DateTime.Now.AddMonths(3).Year == year)
                    {
                        DateTime startDate = new DateTime(year - 1, DateTime.Now.AddMonths(4).Month, 1);
                        DateTime endDate = new DateTime(year - 1, 12, DateTime.DaysInMonth(year - 1, 12));

                        filters.RemoveAll(x => x.PropertyName == "EffectiveYear");
                        filters.RemoveAll(x => x.PropertyName == "EffectiveMonth");
                        filters.Add(new PropertyFilter("EffectiveDate", PropertyFilter.Comparator.Between, startDate, endDate));
                        filters.Add(new PropertyFilter("Status", RiskStatus.ISSUED.Value));

                        additionalForecastingRisks = ServiceLocator.EntityService.GetList<RiskGraph>(filters).ToList();
                        additionalForecastingRisks.Select(o => { o.Status = RiskStatus.SUBMISSION.Value; o.InceptionPremium = o.AnnualizedPremium; return o; }).ToList(); // Set all future stubs to submission so they dont get counted as bound.
                        risks.AddRange(additionalForecastingRisks);
                    }
                    //We are not within 3 months of the next year so no records have been generated yet for the future year.
                    //This means we will need records from the current year, and maybe the prior year. Depending on how close are to the end of the current year.
                    else
                    {
                        //We have generated all the records for the current year. Therefore we have everything we need to forecast next year without looking back at the prior year to fill in the gaps.
                        if (DateTime.Now.AddMonths(3).Month == 12)
                        {
                            DateTime startDate = new DateTime(year - 1, 1, 1);
                            DateTime endDate = new DateTime(year - 1, 12, DateTime.DaysInMonth(year - 1, 12));

                            filters.RemoveAll(x => x.PropertyName == "EffectiveYear");
                            filters.RemoveAll(x => x.PropertyName == "EffectiveMonth");
                            filters.Add(new PropertyFilter("EffectiveDate", PropertyFilter.Comparator.Between, startDate, endDate));
                            filters.Add(new PropertyFilter("Status", RiskStatus.ISSUED.Value));

                            additionalForecastingRisks = ServiceLocator.EntityService.GetList<RiskGraph>(filters).ToList();
                            additionalForecastingRisks.Select(o => { o.Status = RiskStatus.SUBMISSION.Value; o.InceptionPremium = o.AnnualizedPremium; return o; }).ToList(); // Set all future stubs to submission so they dont get counted as bound.
                            risks.AddRange(additionalForecastingRisks);
                        }
                        /*  We are not within 3 months of the end of the year. This means we will have to get some records from the current year, and some records from the prior year to make
                            up a full year. */
                        else
                        {
                            //Getting all the risks that have been generated so far this year
                            DateTime startDate = new DateTime(year - 1, 1, 1);
                            DateTime endDate = new DateTime(year - 1, DateTime.Now.AddMonths(3).Month, DateTime.DaysInMonth(year - 1, DateTime.Now.AddMonths(3).Month));

                            filters.RemoveAll(x => x.PropertyName == "EffectiveYear");
                            filters.RemoveAll(x => x.PropertyName == "EffectiveMonth");
                            filters.Add(new PropertyFilter("EffectiveDate", PropertyFilter.Comparator.Between, startDate, endDate));
                            filters.Add(new PropertyFilter("Status", RiskStatus.ISSUED.Value));

                            additionalForecastingRisks = ServiceLocator.EntityService.GetList<RiskGraph>(filters).ToList();
                            additionalForecastingRisks.Select(o => { o.Status = RiskStatus.SUBMISSION.Value; o.InceptionPremium = o.AnnualizedPremium; return o; }).ToList(); // Set all future stubs to submission so they dont get counted as bound.

                            //Now we need to get the remaining risks from the prior year
                            startDate = endDate.AddYears(-1).AddMonths(1);
                            filters.RemoveAll(x => x.PropertyName == "EffectiveDate");
                            filters.Add(new PropertyFilter("EffectiveDate", PropertyFilter.Comparator.Between, startDate, new DateTime(startDate.Year, 12, DateTime.DaysInMonth(startDate.Year, 12))));
                            var twoYearOldForecastingRisks = ServiceLocator.EntityService.GetList<RiskGraph>(filters);
                            twoYearOldForecastingRisks.Select(o => { o.Status = RiskStatus.SUBMISSION.Value; o.InceptionPremium = o.AnnualizedPremium; return o; }).ToList(); // Set all future stubs to submission so they dont get counted as bound.

                            additionalForecastingRisks.AddRange(twoYearOldForecastingRisks);
                            risks.AddRange(additionalForecastingRisks);
                        }
                    }
                }
            }
            #endregion

            IEnumerable<IGrouping<int, RiskGraph>> months = risks.GroupBy(risk => risk.EffectiveMonth, risk => risk).OrderBy(x => x.Key);

            foreach (IGrouping<int, RiskGraph> monthGroup in months)
            {
                var result = new ExpandoObject() as dynamic;
                result.Month = monthGroup.Key;
                result.MonthText = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(monthGroup.Key);
                IEnumerable<IGrouping<string, RiskGraph>> underwriters = monthGroup.GroupBy(risk => risk.UW, risk => risk);
                var underwriterResults = new List<dynamic>();

                int totalEstimatedWorkload = 0;
                int totalEstimatedWorkloadPremium = 0;
                int totalBoundPremium = 0;

                foreach (IGrouping<string, RiskGraph> uw in underwriters)
                {
                    var uwResult = new ExpandoObject() as dynamic;
                    uwResult.Name = uw.Key;
                    uwResult.NewBusiness = uw.Count(x => !x.IsRenewal);
                    uwResult.NewBusinessBound = uw.Where(x => !x.IsRenewal && x.Status == RiskStatus.ISSUED.Value).Count();
                    uwResult.Renewals = uw.Count(x => x.IsRenewal);
                    uwResult.RenewalsBound = uw.Where(x => x.IsRenewal && x.Status == RiskStatus.ISSUED.Value).Count();
                    uwResult.Total = uwResult.NewBusiness + uwResult.Renewals;
                    totalEstimatedWorkload += uwResult.Total;
                    
                    uwResult.NewPremium = uw.Where(x => !x.IsRenewal).Sum(x => x.AnnualizedPremium);
                    uwResult.NewPremiumBound = uw.Where(x => !x.IsRenewal && x.Status == RiskStatus.ISSUED.Value).Sum(x => x.InceptionPremium);
                    uwResult.RenewalPremium = uw.Where(x => x.IsRenewal).Sum(x => x.AnnualizedPremium);
                    uwResult.RenewalPremiumBound = uw.Where(x => x.IsRenewal && x.Status == RiskStatus.ISSUED.Value).Sum(x => x.InceptionPremium);
                    uwResult.TotalPremium = uwResult.NewPremium + uwResult.RenewalPremium;
                    totalEstimatedWorkloadPremium += uwResult.TotalPremium;
                    totalBoundPremium += (uwResult.RenewalPremiumBound + uwResult.NewPremiumBound);

                    underwriterResults.Add(uwResult);
                }
                result.Underwriters = underwriterResults;

                result.TotalEstimatedWorkload = totalEstimatedWorkload;
                result.TotalEstimatedWorkloadPremium = totalEstimatedWorkloadPremium;
                result.TotalWrittenPremium = totalBoundPremium;

                results.Add(result);
            }

            return results;
        }

        private bool WasIssued(string status)
        {
            return status == RiskStatus.ISSUED.Value || status == RiskStatus.BOUND.Value || status == RiskStatus.CANCELED.Value;
        }

        public ProductLineMonthlyDetail GetProductLineDetail( string productLine, int year, int month = 0, int underwriter = 0, string branch = null)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var detail = new ProductLineMonthlyDetail();
            bool isYearSummary = (month == 0);
            var firstDayOfYear = new DateTime(year, 1, 1);
            var firstDayOfDetailDateRange = (month == 0) ? firstDayOfYear : new DateTime(year, month, 1);
            var lastDayOfDetailDateRange = (month == 0) ? new DateTime(year, 12, DateTime.DaysInMonth(year, 12)) : new DateTime(year, month, DateTime.DaysInMonth(year, month));
            var firstDayOfMonth = (month != 0) ? new DateTime(year, month, 1) : new DateTime(1337, 1, 1);
            List<RiskGraph> additionalForecastingRisks = new List<RiskGraph>();

            List<PropertyFilter> filters = GetProductLineDetailFilters(productLine, year, month, underwriter, branch);
            var risks = ServiceLocator.EntityService.GetList<RiskGraph>(filters).ToList();

            var newBizRisks = risks.Where(x => !x.IsRenewal).ToList();
            var nbRiskForPeriod = (isYearSummary) ? newBizRisks : newBizRisks.Where(x => !x.IsRenewal && x.EffectiveMonth == month);
            var renewalRisks = risks.Where(x => x.IsRenewal).ToList();
            var reRiskForPeriod = (isYearSummary) ? renewalRisks.ToList() : renewalRisks.Where(x => x.IsRenewal && x.EffectiveMonth == month).ToList();
            
            #region Additional Risk Selection Logic
            if (isYearSummary) // Displaying a full year
            {
                if (DateTime.Now.Year == year) // Displaying the current year
                {
                    // We are within 3 months of the next year so all records for this year have been generated
                    if (DateTime.Now.AddMonths(3).Year != year || DateTime.Now.AddMonths(3).Month == 12) { }
                    else
                    {
                        DateTime startDate = new DateTime(year - 1, DateTime.Now.AddMonths(4).Month, 1);
                        DateTime endDate = new DateTime(year - 1, 12, DateTime.DaysInMonth(year - 1, 12));

                        filters.RemoveAll(x => x.PropertyName == "EffectiveYear");
                        filters.RemoveAll(x => x.PropertyName == "EffectiveMonth");
                        filters.Add(new PropertyFilter("EffectiveDate", PropertyFilter.Comparator.Between, startDate, endDate));
                        filters.Add(new PropertyFilter("Status", RiskStatus.ISSUED.Value));

                        additionalForecastingRisks = ServiceLocator.EntityService.GetList<RiskGraph>(filters).ToList();
                        additionalForecastingRisks.Select(o => { o.Status = RiskStatus.SUBMISSION.Value; o.InceptionPremium = o.AnnualizedPremium; return o; }).ToList(); // Set all future stubs to submission so they dont get counted as bound.
                        reRiskForPeriod.AddRange(additionalForecastingRisks);
                    }
                }
                else // Displaying either a past or future year
                {
                    // We are displaying a past year. Therefore all records will exist that we need
                    if (year < DateTime.Now.Year) { } 
                    else // We are displaying a future year
                    {
                        //We are within 3 months of the following year so we have generated at least some of next years renewals.
                        //This means we will need the records from the current year for the remaining months that have not already been generated.
                        if (DateTime.Now.AddMonths(3).Year == year)
                        {
                            DateTime startDate = new DateTime(year - 1, DateTime.Now.AddMonths(4).Month, 1);
                            DateTime endDate = new DateTime(year - 1, 12, DateTime.DaysInMonth(year - 1, 12));

                            filters.RemoveAll(x => x.PropertyName == "EffectiveYear");
                            filters.RemoveAll(x => x.PropertyName == "EffectiveMonth");
                            filters.Add(new PropertyFilter("EffectiveDate", PropertyFilter.Comparator.Between, startDate, endDate));
                            filters.Add(new PropertyFilter("Status", RiskStatus.ISSUED.Value));

                            additionalForecastingRisks = ServiceLocator.EntityService.GetList<RiskGraph>(filters).ToList();
                            additionalForecastingRisks.Select(o => { o.Status = RiskStatus.SUBMISSION.Value; o.InceptionPremium = o.AnnualizedPremium; return o; }).ToList(); // Set all future stubs to submission so they dont get counted as bound.
                            reRiskForPeriod.AddRange(additionalForecastingRisks);
                        }
                        //We are not within 3 months of the next year so no records have been generated yet for the future year.
                        //This means we will need records from the current year, and maybe the prior year. Depending on how close are to the end of the current year.
                        else
                        {
                            //We have generated all the records for the current year. Therefore we have everything we need to forecast next year without looking back at the prior year to fill in the gaps.
                            if (DateTime.Now.AddMonths(3).Month == 12)
                            {
                                DateTime startDate = new DateTime(year - 1, 1, 1);
                                DateTime endDate = new DateTime(year - 1, 12, DateTime.DaysInMonth(year - 1, 12));

                                filters.RemoveAll(x => x.PropertyName == "EffectiveYear");
                                filters.RemoveAll(x => x.PropertyName == "EffectiveMonth");
                                filters.Add(new PropertyFilter("EffectiveDate", PropertyFilter.Comparator.Between, startDate, endDate));
                                filters.Add(new PropertyFilter("Status", RiskStatus.ISSUED.Value));

                                additionalForecastingRisks = ServiceLocator.EntityService.GetList<RiskGraph>(filters).ToList();
                                additionalForecastingRisks.Select(o => { o.Status = RiskStatus.SUBMISSION.Value; o.InceptionPremium = o.AnnualizedPremium; return o; }).ToList(); // Set all future stubs to submission so they dont get counted as bound.
                                reRiskForPeriod.AddRange(additionalForecastingRisks);
                            }
                            /*  We are not within 3 months of the end of the year. This means we will have to get some records from the current year, and some records from the prior year to make
                                up a full year. */
                            else
                            {
                                //Getting all the risks that have been generated so far this year
                                DateTime startDate = new DateTime(year - 1, 1, 1);
                                DateTime endDate = new DateTime(year - 1, DateTime.Now.AddMonths(3).Month, DateTime.DaysInMonth(year - 1, DateTime.Now.AddMonths(3).Month));

                                filters.RemoveAll(x => x.PropertyName == "EffectiveYear");
                                filters.RemoveAll(x => x.PropertyName == "EffectiveMonth");
                                filters.Add(new PropertyFilter("EffectiveDate", PropertyFilter.Comparator.Between, startDate, endDate));
                                filters.Add(new PropertyFilter("Status", RiskStatus.ISSUED.Value));

                                additionalForecastingRisks = ServiceLocator.EntityService.GetList<RiskGraph>(filters).ToList();
                                additionalForecastingRisks.Select(o => { o.Status = RiskStatus.SUBMISSION.Value; o.InceptionPremium = o.AnnualizedPremium; return o; }).ToList(); // Set all future stubs to submission so they dont get counted as bound.

                                //Now we need to get the remaining risks from the prior year
                                startDate = endDate.AddYears(-1).AddMonths(1);
                                filters.RemoveAll(x => x.PropertyName == "EffectiveDate");
                                filters.Add(new PropertyFilter("EffectiveDate", PropertyFilter.Comparator.Between, startDate, new DateTime(startDate.Year, 12, DateTime.DaysInMonth(startDate.Year, 12))));
                                var twoYearOldForecastingRisks = ServiceLocator.EntityService.GetList<RiskGraph>(filters);
                                twoYearOldForecastingRisks.Select(o => { o.Status = RiskStatus.SUBMISSION.Value; o.InceptionPremium = o.AnnualizedPremium; return o; }).ToList(); // Set all future stubs to submission so they dont get counted as bound.

                                additionalForecastingRisks.AddRange(twoYearOldForecastingRisks);
                                reRiskForPeriod.AddRange(additionalForecastingRisks);
                            }
                        }
                    }
                }
            }
            //Month Summary
            else
            {
                //Current Year
                if (year == DateTime.Now.Year)
                {
                    if (month <= DateTime.Now.AddMonths(3).Month || DateTime.Now.AddMonths(3).Year > DateTime.Now.Year) // All records will have been already generated
                    {
                    } 
                    else //Future month we have not generated any records for
                    {
                        DateTime startDate = new DateTime(year - 1, DateTime.Now.AddMonths(4).Month, 1);
                        DateTime endDate = new DateTime(year - 1, month, DateTime.DaysInMonth(year - 1, month));

                        filters.RemoveAll(x => x.PropertyName == "EffectiveYear");
                        filters.RemoveAll(x => x.PropertyName == "EffectiveMonth");
                        filters.Add(new PropertyFilter("EffectiveDate", PropertyFilter.Comparator.Between, startDate, endDate));
                        filters.Add(new PropertyFilter("Status", RiskStatus.ISSUED.Value));

                        additionalForecastingRisks = ServiceLocator.EntityService.GetList<RiskGraph>(filters).ToList();
                        additionalForecastingRisks.Select(o => { o.Status = RiskStatus.SUBMISSION.Value; o.InceptionPremium = o.AnnualizedPremium; return o; }).ToList(); // Set all future stubs to submission so they dont get counted as bound.

                        renewalRisks.AddRange(additionalForecastingRisks.Where(x => x.EffectiveMonth != month));
                        reRiskForPeriod.AddRange(additionalForecastingRisks.Where(x => x.EffectiveMonth == month));
                    }
                }
                //Past or future Year
                else
                {
                    if (year < DateTime.Now.Year) { } //Past Year Do nothing
                    else //Future Year
                    {
                        if (DateTime.Now.AddMonths(3).Year == year) //We have generated some records already
                        {
                            if (DateTime.Now.AddMonths(3).Month >= month) { } //We have already generated all the records.
                            else
                            {
                                DateTime startDate = new DateTime(year - 1, DateTime.Now.AddMonths(4).Month, 1);
                                DateTime endDate = new DateTime(year - 1, month, DateTime.DaysInMonth(year - 1, month));

                                filters.RemoveAll(x => x.PropertyName == "EffectiveYear");
                                filters.RemoveAll(x => x.PropertyName == "EffectiveMonth");
                                filters.Add(new PropertyFilter("EffectiveDate", PropertyFilter.Comparator.Between, startDate, endDate));
                                filters.Add(new PropertyFilter("Status", RiskStatus.ISSUED.Value));

                                additionalForecastingRisks = ServiceLocator.EntityService.GetList<RiskGraph>(filters).ToList();
                                additionalForecastingRisks.Select(o => { o.Status = RiskStatus.SUBMISSION.Value; o.InceptionPremium = o.AnnualizedPremium; return o; }).ToList(); // Set all future stubs to submission so they dont get counted as bound.
                                renewalRisks.AddRange(additionalForecastingRisks.Where(x => x.EffectiveMonth != month));
                                reRiskForPeriod.AddRange(additionalForecastingRisks.Where(x => x.EffectiveMonth == month));
                            }
                        }
                        else //No records have been generated yet for this year
                        {
                            DateTime startDate = new DateTime(year - 1, 1, 1);
                            DateTime endDate = new DateTime(year - 1, month, DateTime.DaysInMonth(year - 1, month));

                            filters.RemoveAll(x => x.PropertyName == "EffectiveYear");
                            filters.RemoveAll(x => x.PropertyName == "EffectiveMonth");
                            filters.Add(new PropertyFilter("EffectiveDate", PropertyFilter.Comparator.Between, startDate, endDate));
                            filters.Add(new PropertyFilter("Status", RiskStatus.ISSUED.Value));

                            additionalForecastingRisks = ServiceLocator.EntityService.GetList<RiskGraph>(filters).ToList();
                            additionalForecastingRisks.Select(o => { o.Status = RiskStatus.SUBMISSION.Value; o.InceptionPremium = o.AnnualizedPremium; return o; }).ToList(); // Set all future stubs to submission so they dont get counted as bound.
                            //reRiskForPeriod.AddRange(additionalForecastingRisks);
                            renewalRisks.AddRange(additionalForecastingRisks.Where(x => x.EffectiveMonth != month));
                            reRiskForPeriod.AddRange(additionalForecastingRisks.Where(x => x.EffectiveMonth == month));
                        }
                    }
                }
            }
            #endregion

            detail.NBTotal = nbRiskForPeriod.Count();
            detail.RETotal = reRiskForPeriod.Count();

            detail.NBBoundTotal = nbRiskForPeriod.Where(x => WasIssued(x.Status)).Count();
            detail.REBoundTotal = reRiskForPeriod.Where(x => WasIssued(x.Status)).Count();

            //-- Total Booked + Deposits ** --//
            detail.NBTotalBookedPremium += nbRiskForPeriod.Where(x => WasIssued(x.Status) && !x.IsPaidInInstallments).Sum(x => x.InceptionPremium);
            detail.NBTotalBookedPremium += nbRiskForPeriod.Where(x => WasIssued(x.Status) && x.IsPaidInInstallments).Sum(x => x.DepositPremium);

            if (!isYearSummary)
            {
                detail.YTDActual += newBizRisks.Where(x => WasIssued(x.Status) && !x.IsPaidInInstallments).Sum(x => x.InceptionPremium);
                detail.YTDActual += newBizRisks.Where(x => WasIssued(x.Status) && x.IsPaidInInstallments).Sum(x => x.DepositPremium);
            }

            detail.RETotalBookedPremium += reRiskForPeriod.Where(x => WasIssued(x.Status) && !x.IsPaidInInstallments).Sum(x => x.InceptionPremium);
            detail.RETotalBookedPremium += reRiskForPeriod.Where(x => WasIssued(x.Status) && x.IsPaidInInstallments).Sum(x => x.DepositPremium);

            if (!isYearSummary)
            {
                detail.YTDActual += renewalRisks.Where(x => WasIssued(x.Status) && !x.IsPaidInInstallments).Sum(x => x.InceptionPremium);
                detail.YTDActual += renewalRisks.Where(x => WasIssued(x.Status) && x.IsPaidInInstallments).Sum(x => x.DepositPremium);
            }
            detail.TotalBooked = detail.RETotalBookedPremium + detail.NBTotalBookedPremium;
            //-- End Total Booked + Deposits //

            //var withInstallments = new RiskGraph();
            //if (withInstallments.IsPaidInInstallments)
            //{
            //    withInstallments.DepositPremium = detail.RETotalWrittenPremium;
            //}

            detail.NBTotalWrittenPremium = nbRiskForPeriod.Where(x => WasIssued(x.Status)).Sum(x => x.WrittenPremium);
            detail.RETotalWrittenPremium = reRiskForPeriod.Where(x => WasIssued(x.Status)).Sum(x => x.WrittenPremium);

            //detail.RETotalBookedPremium = detail.RETotalWrittenPremium;
            // totalExpiringWrittenPremium, totalRenewalPayroll, totalExpiringPayroll - Used for Renewal Net Rate Change Calc
            var totalExpiringWrittenPremium = reRiskForPeriod.Where(x => WasIssued(x.Status)).Sum(x => x.ExpiringWrittenPremium);
            var totalRenewalPayroll = reRiskForPeriod.Where(x => WasIssued(x.Status)).Sum(x => x.Payroll);
            var totalExpiringPayroll = reRiskForPeriod.Where(x => WasIssued(x.Status)).Sum(x => x.ExpiringPayroll);

            //-- Total UnSuccessfull Count & Premium --//
            detail.NBTotalUnsuccessful = nbRiskForPeriod.Where(x => x.Status == RiskStatus.LOST.DisplayText).Count();
            detail.RETotalUnsuccessful = reRiskForPeriod.Where(x => x.Status == RiskStatus.LOST.DisplayText).Count();
            detail.NBTotalUnsuccessfulPremium = nbRiskForPeriod.Where(x => x.Status == RiskStatus.LOST.DisplayText).Sum(x => x.ExpiringWrittenPremium);
            detail.RETotalUnsuccessfulPremium = reRiskForPeriod.Where(x => x.Status == RiskStatus.LOST.DisplayText).Sum(x => x.ExpiringWrittenPremium);
            //-- End Total UnSuccessfull Count & Premium --//

            detail.NBQuotedTotal = nbRiskForPeriod.Where(x => x.Status == RiskStatus.QUOTE.Value).Count();
            detail.REQuotedTotal = reRiskForPeriod.Where(x => x.Status == RiskStatus.QUOTE.Value).Count();

            detail.NBOutstandingPremium = nbRiskForPeriod.Where(x => x.Status == RiskStatus.QUOTE.DisplayText || x.Status == RiskStatus.SUBMISSION.DisplayText).Sum(x => x.WrittenPremium);
            detail.REOutstandingPremium = reRiskForPeriod.Where(x => (x.Status == RiskStatus.QUOTE.DisplayText || x.Status == RiskStatus.SUBMISSION.DisplayText) && x.IsPaidInInstallments).Sum(x => x.DepositPremium);
            detail.REOutstandingPremium += reRiskForPeriod.Where(x => (x.Status == RiskStatus.QUOTE.DisplayText || x.Status == RiskStatus.SUBMISSION.DisplayText) && !x.IsPaidInInstallments).Sum(x => x.WrittenPremium);
            //
            if (!isYearSummary)
            {
                IEnumerable<IGrouping<int, RiskGraph>> priorOutstandingMonths = renewalRisks.Where(x => x.EffectiveMonth < month && (x.Status == RiskStatus.QUOTE.DisplayText || x.Status == RiskStatus.SUBMISSION.DisplayText)).GroupBy(risk => risk.EffectiveMonth, risk => risk);
                var priorOutstandingAmounts = new List<dynamic>();

                foreach (IGrouping<int, RiskGraph> monthSet in priorOutstandingMonths)
                {
                    var outstandingEntry = new ExpandoObject() as dynamic;
                    outstandingEntry.month = monthSet.Key;
                    outstandingEntry.amount = monthSet.Where(x => x.IsPaidInInstallments).Sum(x => x.DepositPremium);
                    outstandingEntry.amount += monthSet.Where(x => !x.IsPaidInInstallments).Sum(x => x.WrittenPremium);
                    priorOutstandingAmounts.Add(outstandingEntry);
                }

                detail.priorOutstanding = priorOutstandingAmounts;                    
                //detail.priorOutstanding += renewalRisks.Where(x => x.EffectiveMonth < month && (x.Status == RiskStatus.QUOTE.DisplayText || x.Status == RiskStatus.SUBMISSION.DisplayText) && x.IsPaidInInstallments).Sum(x => x.DepositPremium);
                //detail.priorOutstanding += renewalRisks.Where(x => x.EffectiveMonth < month && (x.Status == RiskStatus.QUOTE.DisplayText || x.Status == RiskStatus.SUBMISSION.DisplayText) && !x.IsPaidInInstallments).Sum(x => x.InceptionPremium);
            }

            detail.NBDeclinedTotal = nbRiskForPeriod.Where(x => x.Status == RiskStatus.DECLINED.DisplayText).Count();
            detail.NBDeclinedPremium = nbRiskForPeriod.Where(x => x.Status == RiskStatus.DECLINED.DisplayText).Sum(x => x.AnnualizedPremium);
            detail.REDeclinedTotal = reRiskForPeriod.Where(x => x.Status == RiskStatus.DECLINED.DisplayText).Count();
            detail.REDeclinedPremium = reRiskForPeriod.Where(x => x.Status == RiskStatus.DECLINED.DisplayText).Sum(x => x.AnnualizedPremium);

            detail.REExpiringBookedPremium = reRiskForPeriod.Where(x => x.IsPaidInInstallments).Sum(x => x.DepositPremium) + reRiskForPeriod.Where(x => !x.IsPaidInInstallments).Sum(x => x.WrittenPremium);
            detail.REExpiringWrittenPremium = reRiskForPeriod.Sum(x => x.ExpiringWrittenPremium);
            detail.REExpiringPremiumLost = reRiskForPeriod.Where(x => x.Status == RiskStatus.LOST.DisplayText).Sum(x => x.ExpiredAnnualizedPremium);

            detail.REPolicyRetentionRatio = (detail.REBoundTotal + detail.RETotalUnsuccessful > 0) ? (Convert.ToDecimal(detail.REBoundTotal) / (Convert.ToDecimal(detail.REBoundTotal) + Convert.ToDecimal(detail.RETotalUnsuccessful))) * 100 : 0;
            detail.REPremiumRetentionRatio = (detail.REExpiringBookedPremium > 0) ? Math.Round((detail.RETotalBookedPremium / detail.REExpiringBookedPremium) * 100, 2) : 0;
            detail.NBHitRatio = (detail.NBBoundTotal + detail.NBTotalUnsuccessful > 0) ? Math.Round(Convert.ToDecimal(detail.NBBoundTotal) / (detail.NBBoundTotal + detail.NBTotalUnsuccessful) * 100, 2) : 0m;
            detail.NBHitRatioPremium = (detail.NBTotalWrittenPremium + detail.NBTotalUnsuccessfulPremium > 0) ? Math.Round(detail.NBTotalWrittenPremium / (detail.NBTotalWrittenPremium + detail.NBTotalUnsuccessfulPremium) * 100, 2) : 0;

            //== Summary Detail ===========================================//
            
            //var endorsements = DAOFactory.GetDAO<RiskEndorsementDAO>().GetProductLineEndorsementAmountForDatesByBranch(uow, firstDayOfDetailDateRange, lastDayOfDetailDateRange, productLine);
            filters.Clear();

            if (productLine.ToLower() == "workers comp")
            {
                var audits = DAOFactory.GetDAO<RiskAuditDAO>().GetAuditAmount(uow, year, branch, underwriter) as List<dynamic>;
                detail.YTDActual += audits.Where(x => x.AuditMonth <= month).Sum(x => Convert.ToInt32(x.Amount));
                detail.TotalAuditAndEndorsementAmount += (!isYearSummary) ? audits.Where(x => x.AuditMonth == month).Sum(x => Convert.ToInt32(x.Amount))
                    : audits.Sum(x => Convert.ToInt32(x.Amount));
            }          

            var endorsements = DAOFactory.GetDAO<RiskEndorsementDAO>().GetProductLineEndorsementAmountForDates(uow, year, productLine, branch, underwriter);
            detail.YTDActual += endorsements.Where(x => x.EffectiveMonth <= month).Sum(x => Convert.ToInt32(x.Premium));
            detail.TotalAuditAndEndorsementAmount += (!isYearSummary) ? endorsements.Where(x => x.EffectiveMonth == month).Sum(x => Convert.ToInt32(x.Premium))
                : endorsements.Sum(x => Convert.ToInt32(x.Premium));

            #region If the month being shown has not been generated yet we will show the prior years installment amount for reference.
            DateTime lastGen = DateTime.Now.AddMonths(3);
            DateTime lastRenewalGenerationDate = new DateTime(lastGen.Year, lastGen.Month, DateTime.DaysInMonth(lastGen.Year, lastGen.Month));
            if (!isYearSummary)
            {
                if (new DateTime(year, month, 1) > lastRenewalGenerationDate)
                    detail.PriorInstallments = DAOFactory.GetDAO<RiskPaymentDAO>().GetProductLinePayments(uow, productLine, year - 1, month).Sum(x => Convert.ToInt32(x.AnticipatedAmount));
            }
            else
            {
                if (DateTime.Now.Month < 9 || year > DateTime.Now.Year)
                    detail.PriorInstallments = DAOFactory.GetDAO<RiskPaymentDAO>().GetProductLinePayments(uow, productLine, year - 1, month).Sum(x => Convert.ToInt32(x.AnticipatedAmount));
            }
            #endregion

            #region Installments & Reporter Payments
            var payments = DAOFactory.GetDAO<RiskPaymentDAO>().GetProductLinePayments(uow, productLine, year, month);
            detail.TotalInstallments += (String.IsNullOrEmpty(branch)) ? payments.Sum(x => Convert.ToInt32(x.AnticipatedAmount)) : payments.Where(x => x.Branch == branch).Sum(x => Convert.ToInt32(x.AnticipatedAmount));

            if (!isYearSummary) //Single Month
            {
                var periodPayments = DAOFactory.GetDAO<RiskPaymentDAO>().GetProductLinePayments(uow, productLine, year, month, true);
                if (underwriter != 0 && !String.IsNullOrEmpty(branch))
                    detail.YTDActual += periodPayments.Where(x => x.UnderwriterId == underwriter && x.Branch == branch).Sum(x => Convert.ToInt32(x.AnticipatedAmount));
                else if (!String.IsNullOrEmpty(branch))
                    detail.YTDActual += periodPayments.Where(x => x.Branch == branch).Sum(x => Convert.ToInt32(x.AnticipatedAmount));
                else if (underwriter != 0)
                    detail.YTDActual += periodPayments.Where(x => x.UnderwriterId == underwriter).Sum(x => Convert.ToInt32(x.AnticipatedAmount));
                else
                    detail.YTDActual += periodPayments.Sum(x => Convert.ToInt32(x.AnticipatedAmount));
            }
            #endregion

            detail.TotalBookedPlusInstallments += detail.TotalBooked
                                                + detail.TotalInstallments
                                                + detail.TotalAuditAndEndorsementAmount;

            if (isYearSummary)
            {
                var budgetFilters = new List<PropertyFilter>() { new PropertyFilter("ProductLine", productLine), new PropertyFilter("DateYear", year) };
                if (!String.IsNullOrEmpty(branch)) budgetFilters.Add(new PropertyFilter("Branch", branch));
                var yearlyBudgets = ServiceLocator.EntityService.GetList<MonthlyBudget>(budgetFilters);

                // If the budgets for next year are not out yet use the current years budget.
                if (yearlyBudgets.Count() == 0 && year > DateTime.Now.Year)
                {
                    budgetFilters.RemoveAll(x => x.PropertyName == "DateYear");
                    budgetFilters.Add(new PropertyFilter("DateYear", year - 1));
                    yearlyBudgets = ServiceLocator.EntityService.GetList<MonthlyBudget>(budgetFilters);
                }
                
                var yearlyBudgetAmount = (yearlyBudgets != null) ? yearlyBudgets.Sum(x => x.Amount) : 0;
                var budgetMonth = (year == DateTime.Now.Year && month != 0) ? DateTime.Now.Month : 12;
                var ytdBudgetAmount = (yearlyBudgets != null) ? yearlyBudgets.Where(x => x.DateMonth <= budgetMonth).Sum(x => x.Amount) : 0;
                detail.Budget = ytdBudgetAmount;
                detail.YTDBudget = ytdBudgetAmount;
            }
            else
            {
                var monthlyBudgetFilters = new List<PropertyFilter>()
                {
                    new PropertyFilter("ProductLine", productLine),
                    new PropertyFilter("DateYear", year),
                    new PropertyFilter("DateMonth", PropertyFilter.Comparator.LessEquals, month)
                };
                if (!String.IsNullOrEmpty(branch)) monthlyBudgetFilters.Add(new PropertyFilter("Branch", branch));
                

                var monthlyBudgets = ServiceLocator.EntityService.GetList<MonthlyBudget>(monthlyBudgetFilters);
                if (monthlyBudgets.Count() == 0 && year > DateTime.Now.Year)
                {
                    monthlyBudgetFilters.RemoveAll(x => x.PropertyName == "DateYear");
                    monthlyBudgetFilters.Add(new PropertyFilter("DateYear", year - 1));
                    monthlyBudgets = ServiceLocator.EntityService.GetList<MonthlyBudget>(monthlyBudgetFilters);
                }
                detail.Budget = (monthlyBudgets != null) ? monthlyBudgets.Where(x => x.DateMonth == month).Sum(x => x.Amount) : 0;
                detail.YTDBudget = (monthlyBudgets != null) ? monthlyBudgets.Sum(x => x.Amount) : 0;
            }
            
            detail.OverUnderPercent = (detail.Budget != 0) ? Math.Round(((detail.TotalBookedPlusInstallments / detail.Budget) - 1) * 100, 2) : 0;

            if (isYearSummary)
                detail.YTDActual = detail.TotalBookedPlusInstallments;

            //var priorYearDetail = GetPriorYearDeail(productLine, year, month, underwriter);
            detail.YTDOverUnderBudgetPercent = (detail.YTDBudget != 0) ?  Math.Round(((detail.YTDActual/ detail.YTDBudget) - 1) * 100, 2) : 0;

            if (totalRenewalPayroll != 0 && totalExpiringPayroll != 0)
                detail.RenewalNetRateChange = ((detail.RETotalWrittenPremium / (totalRenewalPayroll / 100)) / (totalExpiringWrittenPremium / (totalExpiringPayroll / 100))) - 1;

            return detail;
        }

        public List<dynamic> GetBranchSummary(string productLine, int year, int month = 0)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var results = new List<dynamic>();
            var branches = StringEnum.GetAll<Branch>();
            List<string> territories = new List<string>();
            var firstDayOfYear = new DateTime(year, 1, 1);
            var firstDayOfDetailDateRange = (month == 0) ? firstDayOfYear : new DateTime(year, month, 1);
            var lastDayOfDetailDateRange = (month == 0) ? new DateTime(year, 12, DateTime.DaysInMonth(year, 12)) : new DateTime(year, month, DateTime.DaysInMonth(year, month));
            var firstDayOfMonth = (month != 0) ? new DateTime(year, month, 1) : new DateTime(1337, 1, 1);

            foreach (var branch in branches)
                territories.Add(branch.Value);

            var filters = new List<PropertyFilter>()
            {
                new PropertyFilter("ProductLine", productLine),
                new PropertyFilter("EffectiveYear", year)
            };

            if (month != 0)
                filters.Add(new PropertyFilter("EffectiveMonth", month));

            var data = ServiceLocator.EntityService.GetList<RiskGraph>(filters);
            IEnumerable<dynamic> audits = new List<dynamic>();
            if (productLine.ToLower() == "workers comp")
                audits = DAOFactory.GetDAO<RiskAuditDAO>().GetAuditAmountByBranch(uow, firstDayOfDetailDateRange, lastDayOfDetailDateRange);

            var endorsements = DAOFactory.GetDAO<RiskEndorsementDAO>().GetProductLineEndorsementAmountForDatesByBranch(uow, firstDayOfDetailDateRange, lastDayOfDetailDateRange, productLine);

            foreach (var branch in territories)
            {
                var detail = new ExpandoObject() as dynamic;
                detail.Territory = branch;
                var riskIdsForBranch = data.Where(x => x.Branch == branch).Select(x => x.RiskId);

                var renewalSet =
                    data.Where(
                        x =>
                            x.Branch == branch && x.IsRenewal &&
                            WasIssued(x.Status));

                var renewalDeposits = renewalSet.Where(x => x.IsPaidInInstallments).Sum(x => x.DepositPremium);
                detail.RenewalWrittenPremium = renewalSet.Where(x => !x.IsPaidInInstallments).Sum(x => x.InceptionPremium) + renewalDeposits;

                detail.ExpiringBookedPremium = data.Where(x => x.Branch == branch && x.IsRenewal && x.IsPaidInInstallments).Sum(x => x.DepositPremium) + data.Where(x => x.Branch == branch && x.IsRenewal && !x.IsPaidInInstallments).Sum(x => x.WrittenPremium);

                detail.NewBusinessWritten = data.Where(x => x.Branch == branch && !x.IsRenewal && (x.Status == RiskStatus.BOUND.DisplayText || x.Status == RiskStatus.ISSUED.DisplayText)).Sum(x => x.InceptionPremium);

                var newBusinessDeposits = data.Where(x => x.Branch == branch && !x.IsRenewal && (x.Status == RiskStatus.BOUND.Value || x.Status == RiskStatus.ISSUED.Value) && x.IsPaidInInstallments).Sum(x => x.DepositPremium);
                detail.NewBusinessBookedPremium = data.Where(x => x.Branch == branch && !x.IsRenewal && (x.Status == RiskStatus.BOUND.DisplayText || x.Status == RiskStatus.ISSUED.DisplayText) && !x.IsPaidInInstallments).Sum(x => x.InceptionPremium) + newBusinessDeposits;

                var branchAudits = audits.Where(x => x.Branch == branch).FirstOrDefault();
                detail.AuditPremium = branchAudits != null ? branchAudits.Amount : 0;

                var branchEndorsements = endorsements.Where(x => x.Branch == branch).FirstOrDefault();
                detail.EndorsementPremium = branchEndorsements != null ? branchEndorsements.Amount : 0;

                var payments = DAOFactory.GetDAO<RiskPaymentDAO>().GetProductLinePayments(uow, productLine, year, month);
                detail.Installments = (payments != null) ? payments.Where(x => x.Branch == branch).Sum(x => Convert.ToInt32(x.AnticipatedAmount)) : 0;

                detail.Total = detail.RenewalWrittenPremium + detail.NewBusinessBookedPremium + detail.Installments + detail.AuditPremium + Convert.ToDecimal(detail.EndorsementPremium);
                results.Add(detail);
            }

            var totalDetail = new ExpandoObject() as dynamic;
            totalDetail.Territory = "Totals";
            totalDetail.RenewalWrittenPremium = results.Sum(x => Convert.ToInt32(x.RenewalWrittenPremium));
            totalDetail.ExpiringBookedPremium = results.Sum(x => Convert.ToInt32(x.ExpiringBookedPremium));
            totalDetail.NewBusinessWritten = results.Sum(x => Convert.ToInt32(x.NewBusinessWritten));
            totalDetail.NewBusinessBookedPremium = results.Sum(x => Convert.ToInt32(x.NewBusinessBookedPremium));
            totalDetail.AuditPremium = results.Sum(x => Convert.ToInt32(x.AuditPremium));
            totalDetail.EndorsementPremium = results.Sum(x => Convert.ToInt32(x.EndorsementPremium));
            totalDetail.Installments = results.Sum(x => Convert.ToInt32(x.Installments));
            totalDetail.Total = results.Sum(x => Convert.ToInt32(x.Total));
            results.Add(totalDetail);

            return results;
        }

        public List<StatChart> GetStatChartData(List<PropertyFilter> filters = null)
        {
            List<StatChart> results = new List<StatChart>();
            if (filters == null) filters = new List<PropertyFilter>()
            {
                new PropertyFilter("EffectiveDate", DateTime.Now.AddMonths(-24).ToShortDateString(), DateTime.Now.ToShortDateString())
            };
            
            var timeframes = new[] { 
                new { TimeFrame = "Last 90D", LowerRangeVar = DateTime.Now.AddDays(-90), UpperRangeVar = DateTime.Now }, 
                new { TimeFrame = "Last 180D", LowerRangeVar = DateTime.Now.AddDays(-180), UpperRangeVar = DateTime.Now },
                new { TimeFrame = "Current Month", LowerRangeVar = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1), UpperRangeVar = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).AddDays(-1) },
                new { TimeFrame = "Rolling 12M", LowerRangeVar = DateTime.Now.AddMonths(-12), UpperRangeVar = DateTime.Now },
                new { TimeFrame = "Prior Rolling 12M", LowerRangeVar = DateTime.Now.AddMonths(-24), UpperRangeVar = DateTime.Now.AddMonths(-12) },
                new { TimeFrame = "Calendar YTD", LowerRangeVar = new DateTime(DateTime.Now.Year, 1, 1), UpperRangeVar = DateTime.Now }
            };

            //TODO: Add a filter to prevent any data prior to the GoLive date to prevent skewed #s
            var data = ServiceLocator.EntityService.GetList<RiskGraph>(filters).ToList();
            var totalInforceTimeFrames = new[] { "Rolling 12M", "Prior Rolling 12M", "Calender YTD" };

            foreach (var timeframe in timeframes)
            {
                var tfd = data.Where(x => x.EffectiveDate >= timeframe.LowerRangeVar && x.EffectiveDate <= timeframe.UpperRangeVar);
                var sc = new StatChart();

                sc.Timeframe = timeframe.TimeFrame;
                sc.NumberWritten = tfd.Where(x => !x.IsRenewal && (x.Status == RiskStatus.BOUND.Value || x.Status == RiskStatus.ISSUED.Value || x.Status == RiskStatus.CANCELED.Value)).Count();
                sc.NumberQuoted = tfd.Where(x => !x.IsRenewal && (x.Status == RiskStatus.QUOTE.Value || x.Status == RiskStatus.BOUND.Value || x.Status == RiskStatus.ISSUED.Value || x.Status == RiskStatus.CANCELED.Value || x.Status == RiskStatus.LOST.Value)).Count();
                sc.NumberSubmissions = tfd.Where(x => !x.IsRenewal && x.Status != RiskStatus.INVOLVED.Value).Count();

                sc.HitRatio = (sc.NumberQuoted > 0) ? (Convert.ToDecimal(sc.NumberWritten) / Convert.ToDecimal(sc.NumberQuoted)) * 100 : 0;
                sc.WrittenPercent = (sc.NumberSubmissions > 0) ? (Convert.ToDecimal(sc.NumberWritten) / Convert.ToDecimal(sc.NumberSubmissions)) * 100 : 0;
                sc.QuotedPercent = (sc.NumberSubmissions > 0) ? (Convert.ToDecimal(sc.NumberQuoted) / Convert.ToDecimal(sc.NumberSubmissions)) * 100 : 0;

                sc.NewBusinessAmount = tfd.Where(x => !x.IsRenewal && (x.Status == RiskStatus.BOUND.DisplayText || x.Status == RiskStatus.ISSUED.DisplayText)).Sum(x => x.AnnualizedPremium);
                sc.NumberNewBusiness = tfd.Where(x => !x.IsRenewal && (x.Status == RiskStatus.BOUND.DisplayText || x.Status == RiskStatus.ISSUED.DisplayText)).Count();

                var renewalLostCount = tfd.Where(x => x.IsRenewal && x.Status == RiskStatus.LOST.Value).Count();
                var renewalRetained = tfd.Where(x => x.IsRenewal && (x.Status == RiskStatus.BOUND.Value || x.Status == RiskStatus.ISSUED.Value || x.Status == RiskStatus.QUOTE.Value || x.Status == RiskStatus.SUBMISSION.Value)).Count();
                sc.AccountRetentionPercent = ((renewalLostCount + renewalRetained) > 0) ? Math.Round((Convert.ToDecimal(renewalRetained) / Convert.ToDecimal((renewalLostCount + renewalRetained))) * 100, 2) : 0;

                var renewalGrowthAmount = tfd.Where(x => x.IsRenewal && (x.Status == RiskStatus.BOUND.DisplayText || x.Status == RiskStatus.ISSUED.DisplayText)).Sum(x => x.AnnualizedPremium - x.ExpiredAnnualizedPremium);
                var nbGrowth = tfd.Where(x => !x.IsRenewal && (x.Status == RiskStatus.BOUND.DisplayText || x.Status == RiskStatus.ISSUED.DisplayText)).Sum(x => x.AnnualizedPremium);
                var expiringAmount = tfd.Where(x => x.IsRenewal && (x.Status == RiskStatus.BOUND.DisplayText || x.Status == RiskStatus.ISSUED.DisplayText)).Sum(x => x.ExpiredAnnualizedPremium);

                sc.GrowthAmount = (renewalGrowthAmount + nbGrowth);
                sc.GrowthPercent = (expiringAmount != 0) ? (sc.GrowthAmount / expiringAmount) * 100 : 0;

                if (totalInforceTimeFrames.Contains(timeframe.TimeFrame))
                {
                    sc.TotalInforce = tfd.Where(x => x.Status == RiskStatus.ISSUED.Value).Count();
                }
                
                results.Add(sc);
            }
            
            return results;
        }
    }
}