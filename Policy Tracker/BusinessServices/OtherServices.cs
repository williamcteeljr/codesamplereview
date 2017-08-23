using PolicyTracker.DataAccess;
using PolicyTracker.DataAccess.Policy;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.DomainModel.Policy;
using PolicyTracker.Platform.UOW;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolicyTracker.BusinessServices
{
    public class ConsoleService
    {
        public IEnumerable<dynamic> GetImpactNotes(DateTime startDate, DateTime endDate, int productLineId = 0)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            return DAOFactory.GetDAO<RiskNotesDAO>().GetImpactNotesForDates(uow, startDate, endDate, productLineId);
        }

        public dynamic GetRatios(string underwriterId = null, string branch = null)
        {
            var result = new ExpandoObject() as dynamic;
            var filters = new List<PropertyFilter>()
            {
                new PropertyFilter("EffectiveDate", new DateTime(DateTime.Now.Year, 1, 1), DateTime.Now),
                new PropertyFilter("Status", PropertyFilter.Comparator.NotEquals, RiskStatus.INVOLVED.Value)
            };

            if (!String.IsNullOrEmpty(underwriterId))
                filters.Add(new PropertyFilter("UnderwriterId", underwriterId));
            if (!String.IsNullOrEmpty(branch))
                filters.Add(new PropertyFilter("Branch", branch));

            var risks = ServiceLocator.EntityService.GetList<RiskGraph>(filters).ToList();

            decimal numberSubmitted = risks.Where(x => x.IsRenewal == false).Count();

            decimal numberQuoted = risks.Where(
                x => x.IsRenewal == false
                && x.Status == RiskStatus.QUOTE.Value).Count();

            decimal numberIssued = risks.Where(
                x => x.IsRenewal == true
                && (x.Status == RiskStatus.BOUND.Value
                || x.Status == RiskStatus.ISSUED.Value)).Count();

            result.QuoteRatio = (numberSubmitted != 0) ? Math.Round(Convert.ToDecimal(numberQuoted) / Convert.ToDecimal(numberSubmitted) * 100) : 0;
            result.HitRatio = (numberQuoted != 0) ? Math.Round(Convert.ToDecimal(numberIssued) / Convert.ToDecimal(numberQuoted)  * 100) : 0;
            result.WrittenRatio = (numberSubmitted != 0) ? Math.Round(Convert.ToDecimal(numberIssued) / Convert.ToDecimal(numberSubmitted) * 100) : 0;

            return result;
        }

        public IEnumerable<dynamic> GetProductLinePolicyCounts(string branch = null)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var results = DAOFactory.GetDAO<RiskDAO>().GetProductLinePolicyCounts(uow, branch);
            return results;
        }

        public IEnumerable<dynamic> GetProductLinePolicyPremiums(string branch = null)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var results = DAOFactory.GetDAO<RiskDAO>().GetProductLinePolicyPremiums(uow, branch);
            return results;
        }

        public List<dynamic> GetBudgetData(bool isYearly = false)
        {
            var results = new List<dynamic>();

            var filters = new List<PropertyFilter>();
            var budgetFilters = new List<PropertyFilter>();
            int priorYear = DateTime.Now.AddYears(-1).Year;
            int month = !isYearly ? DateTime.Now.Month : 0;
            int year = DateTime.Now.Year;
            var productLines = ServiceLocator.EntityService.GetList<ProductLine>();

            foreach (var pl in productLines)
            {
                var result = new ExpandoObject() as dynamic;

                result.ProductLine = pl.Name;

                var priorDetail = ServiceLocator.ReportingService.GetProductLineDetail(pl.Name, priorYear, month);
                var currentDetail = ServiceLocator.ReportingService.GetProductLineDetail(pl.Name, year, month);

                result.Budget = currentDetail.Budget;

                result.Actual = priorDetail.TotalBookedPlusInstallments;

                result.Current = currentDetail.TotalBookedPlusInstallments;
                result.CurrentPercent = (result.Budget != 0) ? (result.Current / result.Budget) * 100 : 0;

                result.CurrentOutstanding = currentDetail.REOutstandingPremium;
                result.CurrentOutstandingPercent = (result.Budget != 0) ? (result.CurrentOutstanding / result.Budget) * 100 : 0;

                result.Total = result.Current + result.CurrentOutstanding;
                result.budgetPercent = 100 + currentDetail.OverUnderPercent;

                results.Add(result);
            }

            return results;
        }
    }
}
