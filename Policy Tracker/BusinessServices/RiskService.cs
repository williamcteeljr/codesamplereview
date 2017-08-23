using PolicyTracker.DataAccess;
using PolicyTracker.DataAccess.Brokers;
using PolicyTracker.DataAccess.Policy;
using PolicyTracker.DataAccess.Security;
using PolicyTracker.DomainModel.Brokers;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.DomainModel.Policy;
using PolicyTracker.DomainModel.Security;
using PolicyTracker.Platform.Security;
using PolicyTracker.Platform.UOW;
using System;
using BusinessServices;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Text;
using DomainModel.View_Models;

namespace PolicyTracker.BusinessServices.Risks
{
    public class RiskService
    {
        public Risk UpdateUnderwriterAssignment(int riskId, int uwId, int uwAssistantId)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var risk = DAOFactory.GetDAO<RiskDAO>().GetInstance(uow, new PropertyFilter("Id", riskId));
            risk.UnderwriterId = uwId;
            risk.UnderwriterAssistantId = uwAssistantId;
            DAOFactory.GetDAO<RiskDAO>().Update(uow, risk);

            return risk;
        }

        /// <summary>
        /// Gets a risk from the database along with some related entities.
        /// </summary>
        /// <param name="id">RiskId (QuoteId) of risk</param>
        /// <returns>Risk</returns>
        public Risk GetRisk(int id)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();

            var risk = DAOFactory.GetDAO<RiskDAO>().GetInstance(uow, new PropertyFilter("Id", id));
            risk.PremiumInfo = DAOFactory.GetDAO<RiskPremiumInfoDAO>().GetInstance(uow, new PropertyFilter("RiskId", id)) ?? new RiskPremiumInfo() { RiskId = id };
            risk.InstallmentInfo = DAOFactory.GetDAO<RiskInstallmentInfoDAO>().GetInstance(uow, new PropertyFilter("RiskId", id)) ?? new RiskInstallmentInfo() { RiskId = id };
            risk.WorkersCompInfo = DAOFactory.GetDAO<RiskWorkersCompInfoDAO>().GetInstance(uow, new PropertyFilter("RiskId", id)) ?? new RiskWorkersCompInfo() { RiskId = id };
            risk.Notes = DAOFactory.GetDAO<RiskNotesDAO>().GetList(uow, new PropertyFilter("RiskId", id)).ToList();
            risk.NamedInsured = DAOFactory.GetDAO<NamedInsuredDAO>().GetInstance(uow, new PropertyFilter("ControlNumber", risk.ControlNumber));
            risk.Broker = DAOFactory.GetDAO<BrokerDAO>().GetInstance(uow, new PropertyFilter("AgencyID", risk.NamedInsured.AgencyID));

            return risk;
        }

        /// <summary>
        /// Builds and sets defaults for creating a new risk record.
        /// </summary>
        /// <param name="controlNumber"></param>
        /// <returns></returns>
        public Risk GetNewRisk(int controlNumber = 0)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var risk = new Risk();
            risk.Broker = new Broker();
            risk.NamedInsured = new NamedInsured();
            risk.PremiumInfo = new RiskPremiumInfo();
            risk.InstallmentInfo = new RiskInstallmentInfo();
            risk.WorkersCompInfo = new RiskWorkersCompInfo();

            if (controlNumber != 0)
            {
                var namedInsured = ServiceLocator.EntityService.GetInstance<NamedInsured>(new PropertyFilter("ControlNumber", controlNumber));
                risk.NamedInsured = namedInsured;
                risk.Broker = ServiceLocator.EntityService.GetInstance<Broker>(new PropertyFilter("AgencyID", risk.NamedInsured.AgencyID));
                risk.AgencyID = risk.NamedInsured.AgencyID;

                risk.FirstName = namedInsured.FirstName;
                risk.LastName = namedInsured.LastName;
                risk.CompanyName = namedInsured.CompanyName;
                risk.StreetAddress1 = namedInsured.StreetAddress1;
                risk.StreetAddress2 = namedInsured.StreetAddress2;
                risk.MiddleInitial = namedInsured.MiddleInitial ?? "";
                risk.DoingBusinessAs = namedInsured.DoingBusinessAs ?? "";
                risk.City = namedInsured.City;
                risk.State = namedInsured.State;
                risk.Zip = namedInsured.Zip;
            }
            else
            {
                risk.NamedInsured.IssuingCompany = "Old Republic Insurance Company";
                risk.NamedInsured.CreatedById = SessionManager.GetCurrentSession().User.UserId;
                risk.NamedInsured.CreatedDate = DateTime.Now;
            }

            //risk.Market = ServiceLocator.EntityService.GetList<Market>(new[] { new PropertyFilter("Status", PropertyFilter.Comparator.Equals, "Unsuccessful"), new PropertyFilter("CompanyName", "New Purchase") }).FirstOrDefault().Id;
            risk.CreatedDate = DateTime.Now;
            risk.CreatedById = SessionManager.GetCurrentSession().User.UserId;
            risk.Status = "Submission";
            risk.EffectiveDate = DateTime.Now;
            risk.ExpirationDate = risk.EffectiveDate.Value.AddMonths(12);

            return risk;
        }

        /// <summary>
        /// Gets a list of underwriters based on users assigned to the Underwriter Security Role
        /// </summary>
        /// <param name="branch">Branch of the business to scope underwriters list to.</param>
        /// <returns>List of underwriters</returns>
        public List<User> GetUnderwriters(string branch = null)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var roleName = ConfigurationManager.AppSettings["UnderwriterRoleName"];
            var results = DAOFactory.GetDAO<UserEntityDAO>().GetUnderwriters(uow, roleName, branch).ToList();
            results.Insert(0, new User() { UserId = 0, FirstName = "(None)", ProductLine = SessionManager.GetCurrentSession().User.ProductLine });
            return results;
        }

        /// <summary>
        /// Gets a list of underwriting assistants based on users assigned to the Underwriter Security Role
        /// </summary>
        /// <param name="branch">Branch of the business to scope underwriters list to.</param>
        /// <returns>List of underwriters</returns>
        public List<User> GetUnderwritingAssistants(string branch = null)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var roleName = ConfigurationManager.AppSettings["AssistantRoleName"];
            var results = DAOFactory.GetDAO<UserEntityDAO>().GetUnderwritingAssistants(uow, roleName, branch).ToList();
            results.Insert(0, new User() { UserId = 0, FirstName = "UNASSIGNED" });
            return results;
        }

        /// <summary>
        /// Used to save only the Risk Header record (QuoteLookup). Useful when you only need to update a small amount of information and dont need all related entities updated.
        /// </summary>
        /// <param name="risk">Risk Header being updated</param>
        /// <returns>Risk</returns>
        public Risk SaveRiskHeader(Risk risk)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            DAOFactory.GetDAO<RiskDAO>().Update(uow, risk);

            return risk;
        }

        public void DeleteRisk(int id)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            ServiceLocator.RiskService.LogRiskAction(id, "Deleted");
            DAOFactory.GetDAO<RiskDAO>().Delete(uow, new PropertyFilter("Id", id));
        }

        public void LogRiskAction(int riskId, string action)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();

            var log = new RiskLog();
            log.RiskId = riskId;
            log.UserId = SessionManager.GetCurrentSession().User.UserId;
            log.UserName = SessionManager.GetCurrentSession().User.UserName;
            log.ActionDate = DateTime.Now;
            log.LogAction = action;

            DAOFactory.GetDAO<RiskLogDAO>().Create(uow, log);
        }

        public RiskNote SaveNote(RiskNote note)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var currentUser = SessionManager.GetCurrentSession().User;
            var valResults = new List<ValidationResult>();

            if (note.Id != 0 && currentUser.UserId != note.CreatedById)
                valResults.Add(ValidationHelper.CreateGeneralError("Cannot delete a comment created by another user"));

            if (note.ImpactsPremium && note.ImpactDate == null)
                valResults.Add(new ValidationResult("Must include an impact date if a note impacts premium", new[] { "ImpactDate" }));

            if (valResults.Count() > 0) throw new ValidationRulesException(valResults);

            // Defaults
            if (!note.ImpactsPremium)
                note.ImpactDate = null;

            if (note.Id == 0)
            {
                note.CreatedById = SessionManager.GetCurrentSession().User.UserId;
                note.CreatedDate = DateTime.Now;
                DAOFactory.GetDAO<RiskNotesDAO>().Create(uow, note);
            }
            else
            {
                DAOFactory.GetDAO<RiskNotesDAO>().Update(uow, note);
            }

            return note;
        }

        public void DeleteNote(int noteId)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var note = DAOFactory.GetDAO<RiskNotesDAO>().GetInstance(uow, new PropertyFilter("Id", noteId));
            var currentUser = SessionManager.GetCurrentSession().User;

            var valResults = new List<ValidationResult>();

            if (currentUser.UserId != note.CreatedById)
            {
                valResults.Add(ValidationHelper.CreateGeneralError("Cannot delete a comment created by another user"));
            }

            if (valResults.Count > 0) throw new ValidationRulesException(valResults);

            DAOFactory.GetDAO<RiskNotesDAO>().Delete(uow, new PropertyFilter("Id", noteId));
        }

        public string GetPriorPolicyNumber(string policyNumber)
        {
            int startIndex = policyNumber.Length - 2;
            int suffix;
            string priorPolicySuffix;
            StringBuilder priorPolicyNumber = new StringBuilder();

            Int32.TryParse(policyNumber.Substring(startIndex, 2), out suffix);
            priorPolicySuffix = (suffix > 0) ? (suffix - 1).PolicySuffixZeroFill() : suffix.PolicySuffixZeroFill();
            priorPolicyNumber.Append(policyNumber.Substring(0, startIndex));
            priorPolicyNumber.Append(priorPolicySuffix);

            return priorPolicyNumber.ToString();
        }

        public void ChangeRiskNamedInsured(AlterNamedInsuredViewModel model)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var valResults = new List<ValidationResult>();
            var risk = DAOFactory.GetDAO<RiskDAO>().GetInstance(uow, new PropertyFilter("Id", model.RiskId));

            if (!DAOFactory.GetDAO<NamedInsuredDAO>().Exists(uow, new PropertyFilter("ControlNumber", model.NewNamedInsuredId)))
                valResults.Add(new ValidationResult("Control Number doesn't exist.", new[] { "Error" }));
            if (risk == null)
                valResults.Add(new ValidationResult("Risk Id doesn't exist.", new[] { "Error" }));
            if (risk != null && DAOFactory.GetDAO<RiskDAO>().Count(uow, new PropertyFilter("ControlNumber", risk.ControlNumber)) > 1)
                valResults.Add(new ValidationResult(@"The named insured currently assigned to this risk cannot be deleted. More than the current
                    risk record was found. Please un check the delete option", new[] { "Error" }));

            if (valResults.Count > 0) throw new ValidationRulesException(valResults);

            if (model.ShouldDeleteOldNamedInsured)
                DAOFactory.GetDAO<NamedInsuredDAO>().Delete(uow, new PropertyFilter("ControlNumber", risk.ControlNumber));

            risk.ControlNumber = model.NewNamedInsuredId;

            DAOFactory.GetDAO<RiskDAO>().Update(uow, risk);
        }
    }
}
