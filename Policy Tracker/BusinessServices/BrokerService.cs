using DevExpress.Web.Mvc;
using PolicyTracker.DataAccess;
using PolicyTracker.DataAccess.Brokers;
using PolicyTracker.DomainModel;
using PolicyTracker.DomainModel.Brokers;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.DomainModel.Policy;
using PolicyTracker.DomainModel.Security;
using PolicyTracker.Platform.Caching;
using PolicyTracker.Platform.Logging;
using PolicyTracker.Platform.UOW;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace PolicyTracker.BusinessServices.Brokers
{
    public class BrokerSvc : BaseService
    {
        /// <summary>
        /// Used by the DevExpress Broker Combo box on the risk entry and edit screens. This gets the results for the combo box based on what was typed in. It then stores the results in the request cache
        /// so when the following DevExpress method calls GetGridDataSimple (Part of BaseService) we dont have to hit the database again for the same results.
        /// </summary>
        /// <param name="e">DevExpress Object</param>
        public override void GetDataRowCount(GridViewCustomBindingGetDataRowCountArgs e)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();

            var criteria = GetPaginationCriteria<Broker>(e);
            var results = DAOFactory.GetDAO<BrokerDAO>().GetPaginatedList(uow, criteria);

            e.DataRowCount = results.TotalResults;

            CacheManager.RequestCache.SetValue("GridData", results.Results);
        }

        /// <summary>
        /// Gets the list of agents working at the specified brokerage.
        /// </summary>
        /// <param name="agencyId">Broker Agency Code</param>
        /// <returns>List of Agents</returns>
        public static List<Agent> GetAgentsForAgency(string agencyId)
        {
            var uow = UnitOfWorkFactory.CreateUnitOfWork();
            var brokers = DAOFactory.GetDAO<AgentDAO>().GetList(uow, new PropertyFilter("AgencyID", agencyId)).ToList();
            brokers.Insert(0, new Agent() { AgencyID = agencyId, IndID = 0, FirstName = "UNASSIGNED" });
            uow.Finish();
            return brokers;
        }

        public IEnumerable<Agent> GetAgentsForBroker(string brokerCode)
        {
            var agents = ServiceLocator.EntityService.GetList<Agent>(new PropertyFilter("AgencyID", brokerCode)).ToList();
            agents.Insert(0, new Agent() { AgencyID = brokerCode, IndID = 0, FirstName = "UNASSIGNED" });
            return agents;
        }

        /// <summary>
        /// Gets a list of all available states from the Broker Database State table. Mostly used for state dropdown pick lists.
        /// </summary>
        /// <returns>List of States</returns>
        public IEnumerable<State> GetStates()
        {
            return ServiceLocator.EntityService.GetList<State>();
        }

        public IEnumerable<dynamic> GetTopTenBrokersByPremium(string underwriterId = null, string branch = null)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var results = DAOFactory.GetDAO<BrokerDAO>().GetTopTenBrokersByPremium(uow, underwriterId, branch);
            return results;
        }

        public IEnumerable<dynamic> GetTopTenBrokersByCount(string underwriterId = null, string branch = null)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var results = DAOFactory.GetDAO<BrokerDAO>().GetTopTenBrokersByCount(uow, underwriterId, branch);
            return results;
        }

        /// <summary>
        /// Sends an email to compliance about a broker state licesning Issue. This is initiated via a button on the Risk Entry adn Edit Screens.
        /// </summary>
        /// <param name="riskId">Id of the Risk being worked on. Will be 0 for new submissions</param>
        /// <param name="agencyId">Broker Agency Code</param>
        /// <param name="state">State of the Aiport</param>
        public void LicenseIssueNotification(int riskId, string agencyId, string state, bool isAgencyLicensed, bool IsAgentLicensed)
        {
            var broker = ServiceLocator.EntityService.GetInstance<Broker>(new PropertyFilter("AgencyID", agencyId));
            Risk risk = ServiceLocator.EntityService.GetInstance<Risk>(new PropertyFilter("Id", riskId));
            User user = (risk != null && risk.UnderwriterId != 0) ? ServiceLocator.EntityService.GetInstance<User>(new PropertyFilter("UserId", risk.UnderwriterId)) : null;
            var body = new StringBuilder(String.Format("<h3>Licensing Issue In Policy Tracker For Risk {0}</h3>", riskId));

            var mail = new MailMessage();
            var smtp = new SmtpClient();

            //Send Agency Licensed Email - If not Licensed
            if (!isAgencyLicensed)
            {
                try
                {

                    body.Append(String.Format("Agency {0} - {1} is not currently licensed in the state of {2}.", agencyId, broker.AgencyName, state));
                    mail.From = new MailAddress(ConfigurationManager.AppSettings["MailFrom"]);
                    mail.To.Add(ConfigurationManager.AppSettings["ComplianceLicensing"]);
                    if (user != null && !String.IsNullOrEmpty(user.WorkEmail))
                        mail.CC.Add(user.WorkEmail);
                    mail.Subject = "Agency State Licensing Issue (Policy Tracker)";
                    mail.Body = body.ToString();
                    mail.IsBodyHtml = true;
                    smtp.Send(mail);
                    smtp.Dispose();
                }
                catch (Exception ex)
                {
                    LogManager.Log(LogLevel.ERROR, "SMTP SERVER ISSUE. Agency License issue notification not sent to compliance. ::: " + ex.Message);
                }
            }

            //Send Agent Licensed Email - If not Licensed
            if (!IsAgentLicensed)
            {
                try
                {
                    body.Append(String.Format("Agent {0} - {1} is not currently licensed in the state of {2}.", agencyId, broker.AgencyName, state));
                    mail.From = new MailAddress(ConfigurationManager.AppSettings["MailFrom"]);
                    mail.To.Add(ConfigurationManager.AppSettings["ComplianceLicensing"]);
                    if (user != null && !String.IsNullOrEmpty(user.WorkEmail))
                        mail.CC.Add(user.WorkEmail);
                    mail.Subject = "Agent State Licensing Issue (Policy Tracker)";
                    mail.IsBodyHtml = true;
                    mail.Body = body.ToString();
                    smtp.Send(mail);
                    smtp.Dispose();
                }
                catch (Exception ex)
                {
                    LogManager.Log(LogLevel.ERROR, "SMTP SERVER ISSUE. Agent License issue notification not sent to compliance. ::: " + ex.Message);
                }
            }
        }

        //Get Alert Status Value for Broker Licensed
        public bool IsBrokerLicensedAlert(Risk risk)
        {
            var airport = ServiceLocator.EntityService.GetInstance<Airport>(new PropertyFilter("AirportID", risk.AirportId));
            var state = (Enum.IsDefined(typeof(AircraftPolicyTypes), risk.Prefix) && airport != null) ? airport.State : risk.NamedInsured.State;

            //Don't Send email
            return IsBrokerLicensed(risk.AgencyID, state, risk.Id, false);
        }

        /// <summary>
        /// Checks to see if the broker is licensed to do business in the state of the airport. Licensing information is stored in the Broker Database and updated by compliance.
        /// If either the Broker Agency Code or Aiport State is not supplied then return true to not incorrectly warn the user.
        /// </summary>
        /// <param name="agencyId">Broker Agency Code</param>
        /// <param name="state">Airport State Code</param>
        /// <returns>Boolean</returns>
        public bool IsBrokerLicensed(Risk risk)
        {
            var airport = ServiceLocator.EntityService.GetInstance<Airport>(new PropertyFilter("AirportID", risk.AirportId));
            var state = (Enum.IsDefined(typeof(AircraftPolicyTypes), risk.Prefix) && airport != null) ? airport.State : risk.NamedInsured.State;
            
            //Send Email
            return IsBrokerLicensed(risk.AgencyID, state, risk.Id, true);
        }

        public bool IsBrokerLicensed(string agencyId, string state, int RiskId, bool SendEmail)
        {
            var IsAgencyLicensed = true;
            var IsAgentLicensed = true;
            var isLicensed = true;

            if (!String.IsNullOrEmpty(agencyId) && !String.IsNullOrEmpty(state))
            {
                var filters = new List<PropertyFilter>()
                {
                    new PropertyFilter("AgencyID", agencyId),
                    new PropertyFilter("LicenseState", state),
                    new PropertyFilter("ExpirationDate", PropertyFilter.Comparator.GreaterEquals, DateTime.Now)
                };
                //Check Broker License
                var brokerLicensed = ServiceLocator.EntityService.Exists<BrokerLicense>(filters);

                if (!brokerLicensed)
                {
                    IsAgencyLicensed = false;
                    isLicensed = false;
                }

                //Check Agent License 
                var agentLicensed = ServiceLocator.EntityService.Exists<AgentLicense>(filters);
                if (!agentLicensed)
                {
                    IsAgentLicensed = false;
                    isLicensed = false;
                }
            }

            //Send Agency License Check Email
            if (SendEmail)
            {
                ServiceLocator.BrokerSvc.LicenseIssueNotification(RiskId, agencyId, state, IsAgencyLicensed, IsAgentLicensed);
            }

            return isLicensed;
        }

        /// <summary>
        /// Auto Select the underwrtier to assign to the new risk.
        /// Use case tests are in PoicyServiceTesting.cs
        /// </summary>
        /// <param name="broker">Agency assigned to risk</param>
        /// <param name="productLineId">Id of the product line for risk</param>
        /// <returns></returns>
        public BrokerAssignment GetBrokerAssignment(string broker, int productLineId)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            BrokerAssignment assignment = null;
            var assignments = ServiceLocator.EntityService.GetList<BrokerAssignment>(new[] { new PropertyFilter("BrokerCode", broker), new PropertyFilter("ProductLineId", productLineId) }).ToList();

            //Check if there are any assignments for this broker/pl 
            if (assignments.Count() > 0)
            {
                //More than one person is assigned to receive risks for this broker/pl
                if (assignments.Count() > 1)
                {
                    var lastAssigned = assignments.Where(x => x.LastAssigned).FirstOrDefault();
                    var lastAssignedIndex = assignments.FindLastIndex(x => x.LastAssigned);

                    //Hasent met frequency yet
                    if (lastAssigned != null && lastAssigned.Frequency > 0 && lastAssigned.Tempo < lastAssigned.Frequency)
                    {
                        assignment = lastAssigned;
                        assignment.Tempo++;
                    }
                    //Assignment tempo has reached frequency so we need to move to the next uw in the list of assignments
                    else
                    {
                        lastAssigned.Tempo = 0; //reset tempo for next go around.
                        lastAssigned.LastAssigned = false;
                        // +1 to account for 0 based index
                        if ((lastAssignedIndex + 1) == assignments.Count()) assignment = assignments.FirstOrDefault();
                        else assignment = assignments[lastAssignedIndex + 1];

                        if (assignment.Frequency > 0)
                            assignment.Tempo++;
                    }

                    DAOFactory.GetDAO<BrokerAssignmentDAO>().Update(uow, lastAssigned);
                }
                else
                    assignment = assignments.FirstOrDefault();

                assignment.LastAssigned = true;
                DAOFactory.GetDAO<BrokerAssignmentDAO>().Update(uow, assignment);
            }

            return assignment;
        }
    }

    /// <summary>
    /// Used only by the DevExpress Broker ComboBox to return results for the Autofill feature.
    /// </summary>
    public class BrokerComboService : BaseComboBoxService
    {
        public override object GetByFilter(DevExpress.Web.ListEditItemsRequestedByFilterConditionEventArgs args)
        {
            var uow = UnitOfWorkFactory.CreateUnitOfWork(UnitOfWorkFactory._BROKER_CONTEXT);
            PaginationCriteria criteria = new PaginationCriteria(args.BeginIndex, args.EndIndex, args.Filter, new OrderFilter("AgencyID"));
            criteria.Filters.Add(new PropertyFilter("AgencyID", PropertyFilter.Comparator.StartsWith, args.Filter));
            criteria.Filters.Add(new PropertyFilter("AgencyName", PropertyFilter.Comparator.StartsWith, args.Filter, 1));
            criteria.Filters.Add(new PropertyFilter("Active", true));
            criteria.Filters.Add(new PropertyFilter("Active", PropertyFilter.Comparator.Equals, true, 1));
            var results = (!String.IsNullOrEmpty(args.Filter)) ? DAOFactory.GetDAO<BrokerDAO>().GetPaginatedList(uow, criteria) : new PaginatedList<Broker>();

            //Get Broker Mailing Address information for the returned broker results so we can attach the brokers State to the results to display in the combobox dropdown.
            if (results.Results != null)
            {
                var brokerMailingAddrs = DAOFactory.GetDAO<BrokerMailingAddressDAO>().GetList(uow, new PropertyFilter("AgencyID", PropertyFilter.Comparator.In, results.Results.Select(x => x.AgencyID)));

                foreach (var broker in results.Results)
                {
                    var brokerMailing = brokerMailingAddrs.Where(x => x.AgencyID == broker.AgencyID).FirstOrDefault();
                    broker.State = brokerMailing != null ? brokerMailing.State : "";
                }
            }

            uow.Finish();
            return results.Results;
        }

        public override object GetByValue(DevExpress.Web.ListEditItemRequestedByValueEventArgs args)
        {
            var uow = UnitOfWorkFactory.CreateUnitOfWork();
            var broker = DAOFactory.GetDAO<BrokerDAO>().GetList(uow, new PropertyFilter("AgencyID", args.Value)).FirstOrDefault();
            var result = (broker == null) ? null : new[] { broker };
            uow.Finish();
            return result;
        }
    }

    public class AgentComboService : BaseComboBoxService
    {
        public override object GetByFilter(DevExpress.Web.ListEditItemsRequestedByFilterConditionEventArgs args)
        {
            var uow = UnitOfWorkFactory.CreateUnitOfWork();
            PaginationCriteria criteria = new PaginationCriteria(args.BeginIndex, args.EndIndex, args.Filter, new OrderFilter("AgencyID"));
            criteria.Filters.Add(new PropertyFilter("AgencyID", PropertyFilter.Comparator.StartsWith, args.Filter));
            var results = DAOFactory.GetDAO<BrokerDAO>().GetPaginatedList(uow, criteria);
            uow.Finish();
            return results.Results;
        }

        public override object GetByValue(DevExpress.Web.ListEditItemRequestedByValueEventArgs args)
        {
            var uow = UnitOfWorkFactory.CreateUnitOfWork();
            var result = DAOFactory.GetDAO<BrokerDAO>().GetInstance(uow, new PropertyFilter("AgencyID", args.Value));
            uow.Finish();
            return result;
        }
    }
}
