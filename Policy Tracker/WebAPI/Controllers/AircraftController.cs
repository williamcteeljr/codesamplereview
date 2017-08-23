using PolicyTracker.BusinessServices;
using PolicyTracker.DomainModel;
using PolicyTracker.DomainModel.Framework;
using System.Dynamic;
using System.Net.Http;
using System.Web.Http;
using System.Linq;
using System;
using System.Collections.Generic;
using PolicyTracker.DomainModel.Common;
using PolicyTracker.WebAPI.Filters;
using System.Net;

namespace WebAPI.Controllers
{
    public class AircraftController : BaseController
    {
        [HttpGet]
        [ActionName("SearchTailNumbers")]
        public PaginatedList<TailNumber> SearchTailNumbers(int pageSize, int pageNumber = 1, string id = "")
        {
            var criteria = GetPaginationCriteria<TailNumber>(pageSize, pageNumber, null, null);
            var faaNumber = criteria.Filters.Where(f => f.PropertyName == "FAA_Number").First().Value.ToString();

            criteria.Filters.RemoveAll(x => x.PropertyName == "FAA_Number");
            if (faaNumber.StartsWith("N")) faaNumber = faaNumber.Remove(0, 1);
            criteria.Filters.Add(new PropertyFilter("FAA_Number", PropertyFilter.Comparator.StartsWith, faaNumber));

            var results = ServiceLocator.EntityService.GetPaginatedList<TailNumber>(criteria);

            foreach (var tailNumber in results.Results)
            {
                tailNumber.FAA_Number = "N"+tailNumber.FAA_Number;
            }

            return results;
        }

        [HttpGet]
        [ActionName("GetAircraftForTailNumber")]
        public object GetAircraftForTailNumber(string tailNumber)
        {
            return ServiceLocator.AircraftSvc.LookupTailNumber(tailNumber);
        }

        [HttpGet]
        [ActionName("LookupAircraft")]
        public PaginatedList<AircraftLookup> LookupAircraft(int pageSize, int pageNumber = 1, string sortProperty = null, string sortOrder = null, string filter = null)
        {
            return base.GetPaginatedList<AircraftLookup>(pageSize, pageNumber, sortProperty, sortOrder);
        }

        [HttpGet]
        [ActionName("GetAircraftSummary")]
        public dynamic GetAircraftSummary(int pageSize, int pageNumber = 1, string sortProperty = null, string sortOrder = null)
        {
            var results = new ExpandoObject() as dynamic;
            var criteria = base.GetPaginationCriteria<AircraftLookup>(pageSize, pageNumber, sortProperty, sortOrder).Filters;
            
            var combined = ServiceLocator.EntityService.GetList<AircraftLookup>(criteria);
            var renewals = combined.Where(x => x.IsRenewal);
            var newbuisness = combined.Where(x => !x.IsRenewal);
            
            if (renewals.FirstOrDefault() != null)
            {
                results.Renewals = new ExpandoObject() as dynamic;
                results.Renewals.AverageHullPremium = renewals.Sum(x => x.HullPrem)/ renewals.Count();
                results.Renewals.AverageHullValue = renewals.Average(x => x.Value);
                results.Renewals.AverageHullRate = results.Renewals.AverageHullPremium / results.Renewals.AverageHullValue;
                results.Renewals.AveragePremiumPerUnit = renewals.Average(x => x.HullPrem + x.LiabPrem + x.HullTriaPrem + x.HullWarPrem);
                results.Renewals.AverageYearBuilt = Convert.ToInt16(renewals.Average(x => x.YearNumber));
            }

            if (newbuisness.FirstOrDefault() != null)
            {
                results.NewBusiness = new ExpandoObject() as dynamic;
                results.NewBusiness.AverageHullPremium = newbuisness.Sum(x => x.HullPrem) / newbuisness.Count();
                results.NewBusiness.AverageHullValue = newbuisness.Average(x => x.Value);
                results.NewBusiness.AverageHullRate = results.NewBusiness.AverageHullPremium / results.NewBusiness.AverageHullValue;
                results.NewBusiness.AveragePremiumPerUnit = newbuisness.Average(x => x.HullPrem + x.LiabPrem + x.HullTriaPrem + x.HullWarPrem);
                results.NewBusiness.AverageYearBuilt = Convert.ToInt16(newbuisness.Average(x => x.YearNumber));
            }

            if (combined.FirstOrDefault() != null)
            {
                results.Combined = new ExpandoObject() as dynamic;
                results.Combined.AverageHullPremium = combined.Sum(x => x.HullPrem) / combined.Count();
                results.Combined.AverageHullValue = combined.Average(x => x.Value);
                results.Combined.AverageHullRate = results.Combined.AverageHullPremium / results.Combined.AverageHullValue;
                results.Combined.AveragePremiumPerUnit = combined.Average(x => x.HullPrem + x.LiabPrem + x.HullTriaPrem + x.HullWarPrem);
                results.Combined.AverageYearBuilt = Convert.ToInt16(combined.Average(x => x.YearNumber));
            }

            var uses = from row in combined
                    group row by new
                    {
                        row.PurposeOfUse
                    } into grp
                    select grp.First();
            
            results.Uses = new List<dynamic>();
            foreach (var use in uses)
            {
                var useResult = new ExpandoObject() as dynamic;
                useResult.Name = (!String.IsNullOrEmpty(use.PurposeOfUse)) ? use.PurposeOfUse : "NONE";
                if (combined.FirstOrDefault() != null)
                {
                    var z = combined.Where(x => x.PurposeOfUse == use.PurposeOfUse).Count();
                    var y = combined.Count();
                    useResult.CombinedTotal = Math.Round(Convert.ToDecimal(z) / Convert.ToDecimal(y) * 100, 2);
                }
                if (renewals.FirstOrDefault() != null)
                    useResult.RenewalTotal = Math.Round(Convert.ToDecimal((renewals.Where(x => x.PurposeOfUse == use.PurposeOfUse).Count()) / Convert.ToDecimal(renewals.Count()) * 100), 2);
                if (newbuisness.FirstOrDefault() != null)
                    useResult.NewTotal = Math.Round(Convert.ToDecimal((newbuisness.Where(x => x.PurposeOfUse == use.PurposeOfUse).Count()) / Convert.ToDecimal(newbuisness.Count()) * 100), 2);

                useResult.CombinedCount = combined.Where(x => x.PurposeOfUse == use.PurposeOfUse).Count();
                useResult.RenewalCount = renewals.Where(x => x.PurposeOfUse == use.PurposeOfUse).Count();
                useResult.NewCount = newbuisness.Where(x => x.PurposeOfUse == use.PurposeOfUse).Count();

                results.Uses.Add(useResult);
            }

            var useGrandtotal = new ExpandoObject() as dynamic;
            useGrandtotal.Name = "Total";
            useGrandtotal.CombinedCount = combined.Count();
            useGrandtotal.RenewalCount = renewals.Count();
            useGrandtotal.NewCount = newbuisness.Count();
            results.Uses.Add(useGrandtotal);

            var branches = StringEnum.GetAll<Branch>();
            results.Branches = new List<dynamic>();
            foreach (var branch in branches)
            {
                var branchResult = new ExpandoObject() as dynamic;
                branchResult.Name = branch.DisplayText;
                branchResult.New = newbuisness.Where(x => x.Branch == branch.Value).Count();
                branchResult.Renewals = renewals.Where(x => x.Branch == branch.Value).Count();
                branchResult.Combined = branchResult.New + branchResult.Renewals;

                results.Branches.Add(branchResult);
            }

            var branchGrandtotal = new ExpandoObject() as dynamic;
            branchGrandtotal.Name = "Total";
            branchGrandtotal.Combined = useGrandtotal.CombinedCount;
            branchGrandtotal.Renewals = useGrandtotal.RenewalCount;
            branchGrandtotal.New = useGrandtotal.NewCount;
            results.Branches.Add(branchGrandtotal);


            results.TotalLiability = combined.Sum(x => x.LiabPrem);
            results.TotalPremium = combined.Sum(x => x.HullPrem + x.LiabPrem + x.HullTriaPrem + x.HullWarPrem);
            //TODO: Add a Expired prem field to QuoteAVCAircraft to track the prior prem.
            results.TotalExpired = 0;

            return results;
        }

        [HttpDelete]
        [UseTransaction]
        [ActionName("DeleteAircraft")]
        public HttpResponseMessage DeleteAircraft(int id, int riskId)
        {
            ServiceLocator.AircraftSvc.DeleteAircraft(riskId, id);
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }
    }
}
