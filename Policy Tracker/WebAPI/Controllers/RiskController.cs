using PolicyTracker.WebAPI.Filters;
using PolicyTracker.BusinessServices;
using PolicyTracker.DomainModel.Brokers;
using PolicyTracker.DomainModel.Common;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.DomainModel.Policy;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DomainModel.View_Models;

namespace WebAPI.Controllers
{
    public class RiskController : BaseController
    {
        [HttpGet]
        [ActionName("GetRisk")]
        public RiskGraph GetRisk(int id)
        {
            return ServiceLocator.EntityService.GetInstance<RiskGraph>(new PropertyFilter("RiskId", id));
        }

        [HttpGet]
        [ActionName("GetRisks")]
        public PaginatedList<RiskGraph> GetRisks(int id, int pageSize, int pageNumber = 1, string sortProperty = null, string sortOrder = null)
        {
            return base.GetPaginatedList<RiskGraph>(pageSize, pageNumber, sortProperty, sortOrder, new PropertyFilter("ControlNumber", id));
        }

        [HttpGet]
        [ActionName("GetNotes")]
        public IEnumerable<RiskNote> GetNotes(int id)
        {
            return ServiceLocator.EntityService.GetList<RiskNote>(new PropertyFilter("RiskId", id));
        }

        [HttpGet]
        [ActionName("GetInstallments")]
        public PaginatedList<RiskPayment> GetInstallments(int id, int pageSize, int pageNumber = 1, string sortProperty = null, string sortOrder = null)
        {
            return base.GetPaginatedList<RiskPayment>(pageSize, pageNumber, sortProperty, sortOrder, new PropertyFilter("RiskId", id));
        }

        [HttpGet]
        [ActionName("GetTargetAccounts")]
        public List<RiskGraph> GetTargetAccounts()
        {
            return ServiceLocator.ReportingService.GetTargetAccountsReportData();
        }

        [HttpGet]
        [ActionName("GetMonthlyExpiringAccounts")]
        public IEnumerable<RiskGraph> GetMonthlyExpiringAccounts()
        {
            return ServiceLocator.ReportingService.GetMonthlyExpiringAccounts(DateTime.Now.Month);
        }

        [HttpGet]
        [ActionName("GetAircraft")]
        public dynamic GetAircraft(int id)
        {
            var result = new ExpandoObject() as dynamic;
            var filters = new List<PropertyFilter>()
            {
                new PropertyFilter("QuoteId", id)
            };
            var results = ServiceLocator.EntityService.GetList<Aircraft>(filters);

            var limits = ServiceLocator.EntityService.GetList<AircraftLiability>(new PropertyFilter("AircraftId", PropertyFilter.Comparator.In, results.Select(x => x.Id)));

            foreach (var ac in results)
            {
                ac.Liability = limits.Where(x => x.AircraftId == ac.Id).FirstOrDefault();
            }

            result.Results = results;
            result.TotalCount = results.Count();

            return result;
        }

        [HttpGet]
        [ActionName("GetNotes")]
        public PaginatedList<RiskNote> GetNotes(int id, int pageSize, int pageNumber = 1, string sortProperty = null, string sortOrder = null)
        {
            return base.GetPaginatedList<RiskNote>(pageSize, pageNumber, sortProperty, sortOrder, new PropertyFilter("RiskId", id));
        }

        [HttpGet]
        [ActionName("GetAudits")]
        public PaginatedList<RiskAudit> GetAudits(int id, int pageSize, int pageNumber = 1, string sortProperty = null, string sortOrder = null)
        {
            return base.GetPaginatedList<RiskAudit>(pageSize, pageNumber, sortProperty, sortOrder, new PropertyFilter("RiskId", id));
        }

        [HttpGet]
        [ActionName("GetProductLines")]
        public IEnumerable<ProductLine> GetProductLines()
        {
            return ServiceLocator.EntityService.GetList<ProductLine>();
        }

        [HttpGet]
        [ActionName("getPurposesOfUse")]
        public IEnumerable<PurposeOfUse> getPurposesOfUse()
        {
            return ServiceLocator.EntityService.GetList<PurposeOfUse>();
        }

        [HttpDelete]
        [UseTransaction]
        [ActionName("DeleteAudit")]
        public HttpResponseMessage DeleteAudit(int id)
        {
            ServiceLocator.PolicySvc.DeleteAudit(id);
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }

        [HttpDelete]
        [UseTransaction]
        [ActionName("DeleteNote")]
        public HttpResponseMessage DeleteNote(int id)
        {
            ServiceLocator.RiskService.DeleteNote(id);
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }

        [HttpGet]
        [ActionName("GetBranches")]
        public IEnumerable<Branch> GetBranches()
        {
            return StringEnum.GetAll<Branch>();
        }

        [HttpGet]
        [ActionName("GetStatusReasons")]
        public List<StatusReason> GetStatusReasons(string status)
        {
            return ServiceLocator.PolicySvc.GetStatusReasons(status);
        }

        [HttpGet]
        [ActionName("GetBrokerAssignment")]
        public BrokerAssignment GetBrokerAssignment(string broker, int productLine)
        {
            return ServiceLocator.BrokerSvc.GetBrokerAssignment(broker, productLine);
        }

        [HttpGet]
        [ActionName("GetUnResolvedRisks")]
        public IEnumerable<RiskGraph> GetUnResolvedRisks()
        {
            return ServiceLocator.ReportingService.GetUnResolvedRenewalsReportData();
        }

        /// <summary>
        /// Runs the process to generate new renewal records for policies 90 days from expiring.
        /// </summary>
        /// <returns>Http Response Message</returns>
        [HttpGet]
        [ActionName("CreateRenewals")]
        public HttpResponseMessage GetCreateRenewals()
        {
            ServiceLocator.PolicySvc.GenerateRenewals();
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Checks the OFAC (Office of Foreign Assets Control) SDN (Specially Designated Nationals List) for any hits on the name passed. Endures we are not doing business with undesirables.
        /// </summary>
        /// <param name="name">Name to check against the List</param>
        /// <returns>List of Hits</returns>
        [HttpGet]
        [ActionName("CheckOFACSDN")]
        public dynamic GetCheckOFACSDN(string name)
        {
            var result = new ExpandoObject() as dynamic;
            var hits = ServiceLocator.PolicySvc.CheckOFACSDNListForName(name);
            result.Results = hits;
            result.TotalHits = hits.Count();
            return result;
        }
    }

    public class InstallmentsController : BaseController
    {
        [HttpGet]
        [ActionName("InforceInstallments")]
        public List<dynamic> InforceInstallments()
        {
            var filters = new List<PropertyFilter>();
            filters.Add(new PropertyFilter("ExpirationDate", PropertyFilter.Comparator.GreaterEquals, DateTime.Now));
            filters.Add(new PropertyFilter("IsPaidInInstallments", true));

            var policies = ServiceLocator.EntityService.GetList<RiskGraph>(new[] { "RiskId", "PolicyNumber", "UW", "Name", "AgencyName", "Branch", "ProductLine", "IsPaidInInstallments", "IsReporter" }, filters).ToList();
            var installments = ServiceLocator.EntityService.GetList<RiskPayment>(new PropertyFilter("RiskId", PropertyFilter.Comparator.In, policies.Select(x => x.RiskId)));

            foreach (var policy in policies)
            {
                var policyInstallments = new List<RiskPayment>();
                policyInstallments.AddRange(installments.Where(x => x.RiskId == policy.RiskId));
                policy.Installments = policyInstallments;
            }

            return policies;
        }

        [HttpGet]
        [ActionName("InforceReporters")]
        public List<dynamic> InforceReporters()
        {
            var filters = new List<PropertyFilter>();
            filters.Add(new PropertyFilter("ExpirationDate", PropertyFilter.Comparator.GreaterEquals, DateTime.Now));
            filters.Add(new PropertyFilter("IsReporter", true));

            var policies = ServiceLocator.EntityService.GetList<RiskGraph>(new[] { "RiskId", "PolicyNumber", "UW", "Name", "AgencyName", "Branch", "ProductLine", "IsPaidInInstallments", "IsReporter" }, filters).ToList();
            var installments = ServiceLocator.EntityService.GetList<RiskPayment>(new PropertyFilter("RiskId", PropertyFilter.Comparator.In, policies.Select(x => x.RiskId)));

            foreach (var policy in policies)
            {
                var policyInstallments = new List<RiskPayment>();
                policyInstallments.AddRange(installments.Where(x => x.RiskId == policy.RiskId));
                policy.Installments = policyInstallments;
            }

            return policies;
        }

        [HttpGet]
        [ActionName("Unpaid")]
        public List<dynamic> unpaid()
        {
            var filters = new List<PropertyFilter>();
            filters.Add(new PropertyFilter("ActualAmount", PropertyFilter.Comparator.Equals, 0));
            var payments = ServiceLocator.EntityService.GetList<RiskPayment>(filters);
            var policies = ServiceLocator.EntityService.GetList<RiskGraph>(new PropertyFilter("RiskId", PropertyFilter.Comparator.In, payments.Select(x => x.RiskId)));
            var results = new List<dynamic>();

            foreach (var p in payments)
            {
                var payment = new ExpandoObject() as dynamic;
                var policy = policies.Where(x => x.RiskId == p.RiskId).First();

                payment.PolicyNumber = policy.PolicyNumber;
                payment.AgencyName = policy.AgencyName;
                payment.Insured = policy.Name;
                if (!String.IsNullOrEmpty(policy.UA)) payment.UA = policy.UA;
                if (!String.IsNullOrEmpty(policy.UW)) payment.UW = policy.UW;
                payment.AnticipatedAmount = p.AnticipatedAmount;
                payment.ActualAmount = p.ActualAmount;
                payment.InvoicedDate = p.InvoicedDate;
                payment.DueDate = p.DueDate;
                payment.ReportReceived = p.ReportReceived;
                payment.DueDateMonth = p.DueDateMonth;
                payment.DueDateYear = p.DueDateYear;
                payment.Branch = policy.Branch;
                payment.Type = (policy.IsReporter) ? "Reporter" : (policy.IsPaidInInstallments) ? "Installments" : "";
                payment.ProductLine = policy.ProductLine;
                results.Add(payment);
            }

            return results;
        }

        [HttpPost]
        [Route("policytracker/api/risk/changeNamedInsured")]
        [ActionName("RiskNamedInsuredSwitch")]
        public HttpResponseMessage ChangeNamedInsured(AlterNamedInsuredViewModel model)
        {
            ServiceLocator.RiskService.ChangeRiskNamedInsured(model);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
