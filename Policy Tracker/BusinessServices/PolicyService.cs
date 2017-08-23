using PolicyTracker.Platform;
using PolicyTracker.Platform.Logging;
using PolicyTracker.DataAccess;
using PolicyTracker.DataAccess.Brokers;
using PolicyTracker.DataAccess.Policy;
using PolicyTracker.DataAccess.Security;
using PolicyTracker.DomainModel.Brokers;
using PolicyTracker.DomainModel.Common;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.DomainModel.Policy;
using PolicyTracker.DomainModel.Security;
using PolicyTracker.Platform.Security;
using PolicyTracker.Platform.UOW;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;
using System.Configuration;
using System.Text;
using PolicyTracker.DomainModel;
using BusinessServices;
using PolicyTracker.DomainModel.PostNotice;

namespace PolicyTracker.BusinessServices
{
    public class PolicySvc
    {
        public void GenerateRenewals()
        {
            LogManager.Log(LogLevel.DEBUG, String.Format("Generating Renewals for risks expiring in 90 days from {0}", DateTime.Now.ToShortDateString()));
            UnitOfWork uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var expiringIn90 = DAOFactory.GetDAO<RiskDAO>().GetExpiringRisks(uow);

            foreach(var risk in expiringIn90)
            {
                GenerateRenewal(risk.RiskId);
            }
        }

        public void GenerateRenewal(int riskId)
        {
            UnitOfWork uow = UnitOfWorkFactory.GetActiveUnitOfWork();

            try
            {
                LogManager.Log(LogLevel.DEBUG, String.Format("Generating Renewal for Risk: {0}", riskId));
                
                Risk policy = (Risk)ServiceLocator.RiskService.GetRisk(riskId);
                Broker broker = ServiceLocator.EntityService.GetInstance<Broker>(new PropertyFilter("AgencyID", policy.AgencyID));

                if (broker.IsMidTermOnly)
                {
                    LogManager.Log(LogLevel.INFO, $"Skipped policy {policy.Prefix}{policy.PolicyNumber}{policy.PolicySuffix} because the broker is marked IsMidTermOnly");
                }
                else
                {
                    Risk renewal = (Risk)policy.Copy();
                    IEnumerable<RiskEndorsement> policyCarryOverEndorsements = ServiceLocator.EntityService.GetList<RiskEndorsement>(new[] { new PropertyFilter("RiskId", riskId), new PropertyFilter("IsCarryOverforRenewal", true) });
                    IEnumerable<RiskPayment> policyPayments = ServiceLocator.EntityService.GetList<RiskPayment>(new PropertyFilter("RiskId", riskId));
                    IEnumerable<Aircraft> aircraft = ServiceLocator.EntityService.GetList<Aircraft>(new PropertyFilter("QuoteId", policy.Id)).ToList();

                    renewal.Id = 0;
                    renewal.EffectiveDate = policy.ExpirationDate;
                    renewal.CreatedDate = DateTime.Now;
                    /* 
                     * Per Gary Churchill:
                     * When setting the expiration date on a renewal simply set it 12 months or 1 year from the effective date. This is the most common policy time span which should cover most policies.
                     * If the policy is not a 12 month policy the Underwriter assistant or Underwriter will need to update the expiration date accordingly to handle any 6, 18, or other varied time periods.
                    */
                    renewal.ExpirationDate = renewal.EffectiveDate.Value.AddMonths(12);
                    renewal.PolicySuffix = GetRenewalPolicySuffix(policy.PolicySuffix);
                    renewal.ImageRightId = policy.ImageRightId;
                    renewal.Status = RiskStatus.SUBMISSION.Value;
                    renewal.Market = 0; //The Market is Old Republic now so it is not necessary on renewals.
                    renewal.ParentID = policy.Id;

                    //renewal.PremiumInfo.InceptionPremium = policy.PremiumInfo.AnnualizedPremium;
                    renewal.PremiumInfo.InceptionPremium = policy.PremiumInfo.AnnualizedPremium;
                    renewal.PremiumInfo.AnnualizedPremium = policy.PremiumInfo.AnnualizedPremium;
                    renewal.PremiumInfo.ExpiredAnnualizedPremium = renewal.PremiumInfo.AnnualizedPremium;
                    renewal.PremiumInfo.WrittenPremium = renewal.PremiumInfo.AnnualizedPremium;
                    renewal.PremiumInfo.ExpiringEarnedPremium = policy.PremiumInfo.EarnedPremium;
                    renewal.PremiumInfo.ExpiringWrittenPremium = policy.PremiumInfo.WrittenPremium;
                    renewal.PremiumInfo.DepositPremium = policy.PremiumInfo.DepositPremium;
                    renewal.RenewalOf = policy.Id;

                    renewal.UnderwriterId = policy.RenewalUnderwriterId;
                    renewal.RenewalUnderwriterId = policy.RenewalUnderwriterId;

                    if (renewal.ProductLine == (int)ProductLines.WC)
                        renewal.ApplicationDate = null; // WC requires a new App each year so reset this to null so it will show no app has been sent yet.

                    //Save Each record and assign the Risk Id to each related record.
                    DAOFactory.GetDAO<RiskDAO>().Create(uow, renewal);
                    policy.RenewalQuoteId = renewal.Id;
                    Save(policy);

                    ServiceLocator.RiskService.LogRiskAction(renewal.Id, String.Format("Generated as renewal from policy {0}", policy.Id));
                    LogManager.Log(LogLevel.DEBUG, String.Format("Renewal Created from Risk: {0}. Renewal Risk Id is {1}", policy.Id, renewal.Id));

                    renewal.WorkersCompInfo.RiskId = renewal.Id;
                    DAOFactory.GetDAO<RiskWorkersCompInfoDAO>().Create(uow, renewal.WorkersCompInfo);

                    renewal.InstallmentInfo.RiskId = renewal.Id;
                    DAOFactory.GetDAO<RiskInstallmentInfoDAO>().Create(uow, renewal.InstallmentInfo);
                    foreach (var payment in policyPayments)
                    {
                        payment.DueDate = payment.DueDate.Value.AddMonths(12); //Assumes the policy is on 12 month terms (Again per Gary Churchill above)
                                                                               //payment.InvoicedDate = payment.InvoicedDate.Value.AddMonths(12); //Assumes the policy is on 12 month terms (Again per Gary Churchill above)
                        payment.ActualAmount = 0;
                        payment.RiskId = renewal.Id;
                        DAOFactory.GetDAO<RiskPaymentDAO>().Create(uow, payment);
                    }

                    renewal.PremiumInfo.RiskId = renewal.Id;
                    DAOFactory.GetDAO<RiskPremiumInfoDAO>().Create(uow, renewal.PremiumInfo);

                    //renewal.aircrafts is null... Need to handle this.
                    foreach (var ac in aircraft)
                    {
                        var renewAC = (Aircraft)ac.Copy();
                        renewAC.QuoteId = renewal.Id;
                        DAOFactory.GetDAO<AircraftDAO>().Create(uow, renewAC);
                    }

                    // Creating Carry Over For Renewal Endorsements
                    foreach (var endo in policyCarryOverEndorsements)
                    {
                        var renewalEndorsement = (RiskEndorsement)endo.Copy();
                        renewalEndorsement.RiskId = renewal.Id;
                        DAOFactory.GetDAO<RiskEndorsementDAO>().Create(uow, renewalEndorsement);
                    }

                    ServiceLocator.RiskService.LogRiskAction(policy.Id, "Generated renewal for policy");
                }
            }
            catch (Exception e)
            {
                LogManager.Log(LogLevel.ERROR, e.Message);
            }
        }

        /// <summary>
        /// Increment Suffix for renewing policies
        /// </summary>
        /// <param name="suffix">Policy Suffix</param>
        /// <returns>Incremented String Suffix</returns>
        public string GetRenewalPolicySuffix(string suffix = "00")
        {
            int num;

            if (int.TryParse(suffix, out num))
            {
                suffix = (num + 1).PolicySuffixZeroFill();
            }
            else
            {
                //Failed to Convert Suffix to int
            }

            return suffix;
        }

        /// <summary>
        /// Setting Values on the risk object that are stanard no matter what stage the risk is in, and no matter what the product line is.
        /// </summary>
        /// <param name="risk">Risk Entity being created or edited</param>
        public void RiskDefaults(Risk risk)
        {
            if (!String.IsNullOrEmpty(risk.ImageRightId)) risk.ImageRightId = risk.ImageRightId.ToUpper();
            if (risk.Commission > 0) risk.IsNet = false;

            var product = ServiceLocator.EntityService.GetInstance<Product>(new PropertyFilter("Prefix", risk.Prefix));
            risk.QuoteType = (product != null) ? product.QuoteType : String.Empty;

            risk.InsuredType = ((!String.IsNullOrEmpty(risk.CompanyName))) ? InsuredType.CORP.Value : InsuredType.INDV.Value;

            //Workers Comp Specific Operations.
            if ((risk.ProductLine == (int)ProductLines.WC) && (risk.PolicySuffix != "00"))
            {
                risk.AppReceived = false; //Apps are required for all new WC risks so we should default this.
            }

            if (risk.QuoteSubmissionTypeID == 0)
            {
                if (Convert.ToInt32(risk.PolicySuffix) == 1)
                    risk.QuoteSubmissionTypeID = (int)SubmissionType.NewBusiness;
                else
                    risk.QuoteSubmissionTypeID = (int)SubmissionType.Renewal;
            }
        }

        /// <summary>
        /// Sets the agent name and email on the risk based on the currently assigned Agent Id. If no agent is selected defaulted Agent information will be set.
        /// </summary>
        /// <param name="risk"></param>
        /// <param name="agentId"></param>
        public void SetAgentInfo(Risk risk, int agentId)
        {
            var agent = ServiceLocator.EntityService.GetInstance<Agent>(new[] { new PropertyFilter("AgencyID", risk.AgencyID), new PropertyFilter("IndID", risk.AgentId) });
            if (agent == null)
            {
                agent = ORA.DefaultAgent;
                risk.AgentId = agent.IndID;
            }
            risk.AgentName = agent.FirstName.ToUpper() + " " + agent.LastName.ToUpper();
            risk.AgentEmail = agent.UserEmail;
        }

        /// <summary>
        /// Generic validations that apply when entering a risk for the first time, or editing risks that have already been created.
        /// </summary>
        /// <param name="risk">The risk being validated</param>
        public void ValidateRisk(List<ValidationResult> valResults, Risk risk)
        {
            if (!String.IsNullOrEmpty(risk.LeadInsurer) && risk.ORAPercent == 0)
                valResults.Add(new ValidationResult("Must enter ORA's insuring percent since policy is vertical", new[] { "PolicyInfo.ORAPercent" }));

            if (risk.Status == RiskStatus.QUOTE.Value && String.IsNullOrEmpty(risk.Prefix))
                valResults.Add(new ValidationResult("Must Select a Policy Type/Prefix when Quoting a Policy", new[] { "Prefix" }));

            if (String.IsNullOrEmpty(risk.FirstName) && String.IsNullOrEmpty(risk.CompanyName))
                valResults.Add(new ValidationResult("The Named insured must at least have a first name, or a company name.", new[] { "FirstName" }));

            var productLine = ServiceLocator.EntityService.GetInstance<ProductLine>(new PropertyFilter("ProductLineId", risk.ProductLine));



            ////Market requirement if risk status is not Submission or Already Involved
            //if (risk.Market == 0 && risk.Status != RiskStatus.SUBMISSION.Value || risk.Status != RiskStatus.INVOLVED.Value)
            //    valResults.Add(new ValidationResult("A market is required for this policy status", new[] { "Market" }));

            if (risk.Status == RiskStatus.SUBMISSION.Value)
            {
                if (risk.ProcessDate == null) valResults.Add(new ValidationResult("ProcessDate Required", new[] { "ProcessDate" }));
                //TODO: Talk to Gary about the market field and what the default should be. Some users said new purchase was incorrect.
                //if (risk.Market == 0) valResults.Add(new ValidationResult("Market Required", new[] { "Market" }));
                if (String.IsNullOrEmpty(risk.AgencyID)) valResults.Add(new ValidationResult("Broker Required", new[] { "AgencyID" }));
                if (risk.UnderwriterId == 0) valResults.Add(new ValidationResult("Underwriter Required", new[] { "UnderwriterId" }));

                if (Enum.IsDefined(typeof(AircraftPolicyTypes), risk.Prefix))
                {
                    if (String.IsNullOrEmpty(risk.AirportId))
                        valResults.Add(new ValidationResult(String.Format("Policies for Product Line {0} require an Aiport", productLine.Name), new[] { "AirportId" }));
                }
            }

            //Require Market value if status is Already Involved
            if (risk.Status == RiskStatus.INVOLVED.Value)
            {
                if (risk.ProcessDate == null) valResults.Add(new ValidationResult("ProcessDate Required", new[] { "ProcessDate" }));
                //TODO: Talk to Gary about the market field and what the default should be. Some users said new purchase was incorrect.
                //if (risk.Market == 0) valResults.Add(new ValidationResult("Market Required", new[] { "Market" }));
                if (String.IsNullOrEmpty(risk.AgencyID)) valResults.Add(new ValidationResult("Broker Required", new[] { "AgencyID" }));
                if (risk.UnderwriterId == 0) valResults.Add(new ValidationResult("Underwriter Required", new[] { "UnderwriterId" }));

                if (Enum.IsDefined(typeof(AircraftPolicyTypes), risk.Prefix))
                {
                    if (String.IsNullOrEmpty(risk.AirportId))
                        valResults.Add(new ValidationResult(String.Format("Policies for Product Line {0} require an Aiport", productLine.Name), new[] { "AirportId" }));
                }
            }

            if (risk.Status == RiskStatus.QUOTE.Value)
            {
                //Market requirement if risk status is not Submission or Already Involved
                if (risk.Market == 0 && ((risk.Status != RiskStatus.SUBMISSION.Value) || (risk.Status != RiskStatus.INVOLVED.Value)))
                    valResults.Add(new ValidationResult("A market is required for this policy status", new[] { "Market" }));

                QuoteStatusValidations(valResults, risk);
            }

            if (risk.Status == RiskStatus.DECLINED.Value)
            {
                //Market requirement if risk status is not Submission or Already Involved
                if (risk.Market == 0 && ((risk.Status != RiskStatus.SUBMISSION.Value) || (risk.Status != RiskStatus.INVOLVED.Value)))
                    valResults.Add(new ValidationResult("A market is required for this policy status", new[] { "Market" }));
                DeclineStatusValidations(valResults, risk);
            }

            if (risk.Status == RiskStatus.BOUND.Value)
            {
                //Market requirement if risk status is not Submission or Already Involved
                if (risk.Market == 0 && ((risk.Status != RiskStatus.SUBMISSION.Value) || (risk.Status != RiskStatus.INVOLVED.Value)))
                    valResults.Add(new ValidationResult("A market is required for this policy status", new[] { "Market" }));

                QuoteStatusValidations(valResults, risk);
                BoundStatusValidations(valResults, risk);
            }

            if (risk.Status == RiskStatus.ISSUED.Value)
            {
                //Market requirement if risk status is not Submission or Already Involved
                if (risk.Market == 0 && ((risk.Status != RiskStatus.SUBMISSION.Value) || (risk.Status != RiskStatus.INVOLVED.Value)))
                    valResults.Add(new ValidationResult("A market is required for this policy status", new[] { "Market" }));

                QuoteStatusValidations(valResults, risk);
                BoundStatusValidations(valResults, risk);
                IssuedStatusValidations(valResults, risk);
            }

            if (risk.Status == RiskStatus.LOST.Value)
            {
                //Market requirement if risk status is not Submission or Already Involved
                if (risk.Market == 0 && ((risk.Status != RiskStatus.SUBMISSION.Value) || (risk.Status != RiskStatus.INVOLVED.Value)))
                    valResults.Add(new ValidationResult("A market is required for this policy status", new[] { "Market" }));

                QuoteStatusValidations(valResults, risk);
                LostStatusValidations(valResults, risk);
            }

            if (risk.Status == RiskStatus.CANCELED.Value)
            {
                //Market requirement if risk status is not Submission or Already Involved
                if (risk.Market == 0 && ((risk.Status != RiskStatus.SUBMISSION.Value) || (risk.Status != RiskStatus.INVOLVED.Value)))
                    valResults.Add(new ValidationResult("A market is required for this policy status", new[] { "Market" }));

                QuoteStatusValidations(valResults, risk);
                BoundStatusValidations(valResults, risk);
                IssuedStatusValidations(valResults, risk);
                CancelledStatusValidations(valResults, risk);
            }
        }

        public void QuoteStatusValidations(List<ValidationResult> valResults, Risk risk)
        {
            if (Enum.IsDefined(typeof(AircraftPolicyTypes), risk.Prefix))
            {
                var riskAircraft = ServiceLocator.EntityService.GetList<Aircraft>(new PropertyFilter("QuoteId", risk.Id));
                if (riskAircraft.FirstOrDefault() == null)
                    valResults.Add(new ValidationResult("Policies of this type require aircraft records to be moved beyond status 'Submission'", new[] { "Aircraft" }));
            }
            //Purpose of Use Requirement only for specific policy types
            //Airport Requirement only for specific policy types
            if (risk.PremiumInfo.AnnualizedPremium == 0)
                valResults.Add(new ValidationResult("Annualized Premium Required for Quotes.", new[] { "PremiumInfo.AnnualizedPremium" }));
                
            //How to require Brokerage agent when sometimes it will be unassigned

            if (!risk.IsNet && risk.Commission == 0)
                valResults.Add(new ValidationResult("Commission must be indicated as net to allow for 0% commission", new[] { "PolicyInfo.Commission" }));

            if ((!risk.InstallmentInfo.IsPaidInInstallments && !risk.InstallmentInfo.IsReporter) && risk.PremiumInfo.DepositPremium != 0)
                valResults.Add(new ValidationResult("Cannot have a deposit premium when the risk is not marked as paid in installments", new[] { "PremiumInfo.DepositPremium" }));
        }

        public void DeclineStatusValidations(List<ValidationResult> valResults, Risk risk)
        {
            if (String.IsNullOrEmpty(risk.StatusReason)) valResults.Add(new ValidationResult("Declined Status Reason Required", new[] { "StatusReason" }));

            //Purpose of Use Requirement only for specific policy types
            //Airport Requirement only for specific policy types
        }

        public void BoundStatusValidations(List<ValidationResult> valResults, Risk risk)
        {
            if (risk.PremiumInfo.WrittenPremium == 0) valResults.Add(new ValidationResult("Written Premium cannot be $0", new[] { "PremiumInfo.WrittenPremium" }));
            //Cant Require Earned.
            if (String.IsNullOrEmpty(risk.StreetAddress1)) valResults.Add(new ValidationResult("Named Insured Street Address is Required", new[] { "StreetAddress1" }));
            if (String.IsNullOrEmpty(risk.Zip)) valResults.Add(new ValidationResult("Named Insured Zip is Required", new[] { "Zip" }));
        }

        public void CancelledStatusValidations(List<ValidationResult> valResults, Risk risk)
        {
            if (String.IsNullOrEmpty(risk.StatusReason)) valResults.Add(new ValidationResult("Cancelled Status Reason Required", new[] { "StatusReason" }));
        }

        public void IssuedStatusValidations(List<ValidationResult> valResults, Risk risk)
        {
            if (String.IsNullOrEmpty(risk.Prefix))
                valResults.Add(new ValidationResult("A policy prefix (product) is required to issue a policy", new[] { "Prefix" }));

            if (String.IsNullOrEmpty(risk.PolicyNumber))
                valResults.Add(new ValidationResult("A policy number is required to issue a policy", new[] { "PolicyNumber" }));

            if (String.IsNullOrEmpty(risk.PolicySuffix))
                valResults.Add(new ValidationResult("A policy suffix is required to issue a policy", new[] { "PolicySuffix" }));

            var productLine = ServiceLocator.EntityService.GetInstance<ProductLine>(new PropertyFilter("ProductLineId", risk.ProductLine));
            if ((int)ProductLines.WC == risk.ProductLine && Convert.ToInt16(risk.PolicySuffix) > 0)
            {
                if (risk.PremiumInfo.ExpiredAnnualizedPremium == 0) valResults.Add(new ValidationResult("Expired Annual Prem Required for Renewals", new[] { "PremiumInfo.ExpiredAnnualizedPremium" }));
                if (risk.PremiumInfo.ExpiringWrittenPremium == 0) valResults.Add(new ValidationResult("Expired Written  Prem Required for Renewals", new[] { "PremiumInfo.WrittenPremium" }));
            }
            else if ((int)ProductLines.WC != risk.ProductLine && Convert.ToInt16(risk.PolicySuffix) > 1)
            {
                if (risk.PremiumInfo.ExpiredAnnualizedPremium == 0) valResults.Add(new ValidationResult("Expired Annual Prem Required for Renewals", new[] { "PremiumInfo.ExpiredAnnualizedPremium" }));
                if (risk.PremiumInfo.ExpiringWrittenPremium == 0) valResults.Add(new ValidationResult("Expired Written  Prem Required for Renewals", new[] { "PremiumInfo.WrittenPremium" }));
            }

            if (risk.RenewalUnderwriterId == 0)
                valResults.Add(new ValidationResult("A Renewal Underwriter is Required to Issue a policy", new[] { "RenewalUnderwriterId" }));

            //TODO: Premium Change and Premium Change Reason checking
        }

        public void LostStatusValidations(List<ValidationResult> valResults, Risk risk)
        {
            if (String.IsNullOrEmpty(risk.StatusReason)) valResults.Add(new ValidationResult("Lost Status Reason Required", new[] { "StatusReason" }));
        }

        /// <summary>
        /// Creates a new Risk record and either creates or updates associated entities. Only used from the Risk Entry Screen (Clearance Workflow)
        /// </summary>
        /// <param name="risk">Risk being created</param>
        /// <returns>Risk Created</returns>
        public Risk SaveNewRisk(Risk risk, bool isValidating = true)
        {
            var sessionUser = SessionManager.GetCurrentSession().User;
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var valResults = new List<ValidationResult>();

            LogManager.Log(LogLevel.DEBUG, String.Format("[User: {0}] Saving New Risk", sessionUser.UserName));
            // <Defaults For All Risks> =============================================================================================================================================================
            RiskDefaults(risk);
            SetAgentInfo(risk, risk.AgentId);
            risk.RenewalUnderwriterId = risk.UnderwriterId;
            // </Defaults For All Risks> ============================================================================================================================================================

            if (isValidating)
            {
                LogManager.Log(LogLevel.DEBUG, String.Format("[User: {0}] Validating New Risk", sessionUser.UserName));
                // <New Risk Only Validations> =============================================================================================================================================================
                var ofacHits = ServiceLocator.PolicySvc.CheckOFACSDNListForName(risk.Name);
                if (ofacHits.Count() > 0 && risk.Id == 0)
                {
                    valResults.Add(new ValidationResult("The Named Insured was found on the OFAC SDN list. Unable to create this risk. Compliance has been notified.", new[] { "NamedInsured.CompanyName" }));
                    OFACSDNIssueNotification(risk.Name);
                    risk.HasFailedOFAC = true;
                }
                // </New Risk Only Validations> ============================================================================================================================================================

                ValidateRisk(valResults, risk);

                if (risk.Status == RiskStatus.DECLINED.Value && String.IsNullOrEmpty(risk.FirstNote.Comment))
                    valResults.Add(new ValidationResult("Must enter a note if declining the risk", new[] { "FirstNote" }));

                if (valResults.Count > 0) throw new ValidationRulesException(valResults);
            }

            if (isValidating)
            {
                if (risk.ProductLine == 8) risk.PolicySuffix = "00";
                else risk.PolicySuffix = "01";
            }


            //Step 1 - Save the Named Insured Info. We will need the control # from this for the risk.
            LogManager.Log(LogLevel.DEBUG, String.Format("[User: {0}] Saving Named Insured of New Risk", sessionUser.UserName));
            risk.NamedInsured.AgencyID = risk.AgencyID;
            if (risk.NamedInsured.ControlNumber == 0)
            {
                risk.NamedInsured.DoingBusinessAs = risk.DoingBusinessAs;
                risk.NamedInsured.FirstName = risk.FirstName;
                risk.NamedInsured.LastName = risk.LastName;
                risk.NamedInsured.CompanyName = risk.CompanyName;
                risk.NamedInsured.StreetAddress1 = risk.StreetAddress1;
                risk.NamedInsured.StreetAddress2 = risk.StreetAddress2;
                risk.NamedInsured.City = risk.City;
                risk.NamedInsured.State = risk.State;
                risk.NamedInsured.Zip = risk.Zip;
                risk.NamedInsured.MiddleInitial = risk.MiddleInitial;
                risk.NamedInsured.InsuredType = risk.InsuredType;
                risk.NamedInsured.Fein = risk.Fein;

                DAOFactory.GetDAO<NamedInsuredDAO>().Create(uow, risk.NamedInsured);
            }
            else
            {
                //DAOFactory.GetDAO<NamedInsuredDAO>().Update(uow, risk.NamedInsured);
            }

            //Step 2 - Save the Risk.
            LogManager.Log(LogLevel.DEBUG, String.Format("[User: {0}] Saving Risk Record of New Risk", sessionUser.UserName));
            risk.ControlNumber = risk.NamedInsured.ControlNumber;
            risk.Broker = ServiceLocator.EntityService.GetInstance<Broker>(new PropertyFilter("AgencyID", risk.AgencyID));
            risk.BrokerStatus = (risk.Broker != null && risk.Broker.PlatinumProducer) ? BrokerCommissionStatus.PLATINUM.Value : BrokerCommissionStatus.BASE.Value;
            risk.RenewalUnderwriterId = risk.UnderwriterId;
            risk.ExpirationDate = risk.EffectiveDate.Value.AddYears(1);

            //Workers Comp Specific Operations.
            if ((risk.ProductLine == (int)ProductLines.WC) && risk.PolicySuffix == "00")
            {
                risk.AppReceived = true; //Apps are required for all new WC risks so we should default this.
            }
            else
            {
                risk.AppReceived = false;
            }

            DAOFactory.GetDAO<RiskDAO>().Create(uow, risk);

            //Save Each FAA # Entered as a new aircraft
            LogManager.Log(LogLevel.DEBUG, String.Format("[User: {0}] Saving Aircraft Record(s) of New Risk {1}", sessionUser.UserName, risk.Id));
            if (risk.FAANumbers != null && risk.FAANumbers.Count() > 0)
            {
                foreach (var faaNo in risk.FAANumbers)
                {
                    var acRef = ServiceLocator.AircraftSvc.LookupTailNumber(faaNo) as dynamic;

                    var aircraft = new Aircraft()
                    {
                        QuoteId = risk.Id,
                        FAANo = faaNo,
                        PurposeOfUse = risk.PurposeOfUse,
                        AirportID = risk.AirportId,
                        Year = acRef.Year,
                        Make = acRef.Make,
                        ModelName = acRef.Model,
                        IsIncluded = true
                        
                    };

                    ServiceLocator.AircraftSvc.SaveAircraft(aircraft);
                }
            }

            //Step 4 - Save Premium Info
            LogManager.Log(LogLevel.DEBUG, String.Format("[User: {0}] Saving Premium Info of New Risk {1}", sessionUser.UserName, risk.Id));
            risk.PremiumInfo = new RiskPremiumInfo() { RiskId = risk.Id };
            DAOFactory.GetDAO<RiskPremiumInfoDAO>().Create(uow, risk.PremiumInfo);

            //Step 5 - Save Installment Info
            LogManager.Log(LogLevel.DEBUG, String.Format("[User: {0}] Saving Installment of New Risk {1}", sessionUser.UserName, risk.Id));
            risk.InstallmentInfo = new RiskInstallmentInfo() { RiskId = risk.Id };
            DAOFactory.GetDAO<RiskInstallmentInfoDAO>().Create(uow, risk.InstallmentInfo);

            //Step 6 - Save WorkersComp Info
            LogManager.Log(LogLevel.DEBUG, String.Format("[User: {0}] Saving Workers Comp Info of New Risk {1}", sessionUser.UserName, risk.Id));
            risk.WorkersCompInfo = new RiskWorkersCompInfo() { RiskId = risk.Id };
            DAOFactory.GetDAO<RiskWorkersCompInfoDAO>().Create(uow, risk.WorkersCompInfo);

            //Step 7 - Save the Initial Risk Note if one was entered
            if (!String.IsNullOrEmpty(risk.FirstNote.Comment))
            {
                risk.FirstNote.CreatedById = SessionManager.GetCurrentSession().User.UserId;
                risk.FirstNote.CreatedDate = DateTime.Now;
                risk.FirstNote.RiskId = risk.Id;
                DAOFactory.GetDAO<RiskNotesDAO>().Create(uow, risk.FirstNote);
            }

            //DAOFactory.GetDAO<NamedInsuredDAO>().Update(uow, risk.NamedInsured);

            ServiceLocator.RiskService.LogRiskAction(risk.Id, "Created");

            // <Check Compliance Licensing>
            if (isValidating)
            {
                var airport = ServiceLocator.EntityService.GetInstance<Airport>(new PropertyFilter("AirportID", risk.AirportId));
                var state = (Enum.IsDefined(typeof(AircraftPolicyTypes), risk.Prefix)) ? airport.State : risk.State;
                var IsLicensed = ServiceLocator.BrokerSvc.IsBrokerLicensed(risk.AgencyID, state, risk.Id, true);
              
                    
            }
            //</Check Compliance Licensing>
            LogManager.Log(LogLevel.DEBUG, String.Format("[User: {0}] Completed Saving New Risk {1}", sessionUser.UserName, risk.Id));

            return risk;
        }

        public Risk Save(Risk risk)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();


            if (risk.NamedInsured.ControlNumber != 0)
            {

                DAOFactory.GetDAO<RiskDAO>().Update(uow, risk);

                DAOFactory.GetDAO<NamedInsuredDAO>().Update(uow, risk.NamedInsured);

                if (risk.PremiumInfo.Id != 0)
                {
                    DAOFactory.GetDAO<RiskPremiumInfoDAO>().Update(uow, risk.PremiumInfo);
                }
                else
                {
                    DAOFactory.GetDAO<RiskPremiumInfoDAO>().Create(uow, risk.PremiumInfo);
                }

                if (risk.InstallmentInfo.Id != 0)
                {
                    DAOFactory.GetDAO<RiskInstallmentInfoDAO>().Update(uow, risk.InstallmentInfo);
                }
                else
                {
                    DAOFactory.GetDAO<RiskInstallmentInfoDAO>().Create(uow, risk.InstallmentInfo);
                }

                if (risk.WorkersCompInfo.Id != 0)
                {
                    DAOFactory.GetDAO<RiskWorkersCompInfoDAO>().Update(uow, risk.WorkersCompInfo);
                }
                else
                {
                    DAOFactory.GetDAO<RiskWorkersCompInfoDAO>().Create(uow, risk.WorkersCompInfo);
                }
            }

            return risk;
        }

        /// <summary>
        /// Used to save an already existing risk on the entry form when the user uses the save button isntead of the save and close button. Since the entry form contains less inputs
        /// the save process doesn't do all the same validations (ie. workers comp, etc.).
        /// </summary>
        /// <param name="risk"></param>
        /// <returns></returns>
        public Risk EntrySaveExisting(Risk risk)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var valResults = new List<ValidationResult>();

            //Validate the risk entry
            ValidateRisk(valResults, risk);
            Save(risk);

            return risk;
        }

        /// <summary>
        /// Validations specific to the Workers Comp Product Line. Should only be called when saving a Workers Comp Product Line policy.
        /// </summary>
        /// <param name="valResults">List of Validation Results</param>
        /// <param name="r">Risk being validated</param>
        public void ValidateRiskForWorkersComp(List<ValidationResult> valResults, Risk r, Risk currRisk)
        {
            if (r.Status != RiskStatus.SUBMISSION.Value && r.Status != RiskStatus.INVOLVED.Value)
            {
                //if (!r.IsRenewal)
                //{
                //    if (r.Market == 0)
                //        valResults.Add(new ValidationResult("Must select a market", new string[] { "Market" }));
                //}

                if (r.IsDoNotRenew != currRisk.IsDoNotRenew)
                {
                    if (!SecurityManager.HasAccess("/ProductLineManager/Risk", SecurityManager.ResourcePrivilege.Write))
                        valResults.Add(new ValidationResult("Only a Product Line Manager can mark a risk as Do Not Renew", new string[] { "IsDoNotRenew" }));
                }

                if (r.PremiumInfo.WrittenPremium == 0)
                    valResults.Add(new ValidationResult("Must enter a written premium amount", new string[] { "PremiumInfo.WrittenPremium" }));

                if (String.IsNullOrEmpty(r.WorkersCompInfo.AccountDescription))
                    valResults.Add(new ValidationResult("Must select an account description", new string[] { "WorkersCompInfo.AccountDescription" }));

                if (r.InstallmentInfo.IsPaidInInstallments && r.PremiumInfo.DepositPremium == 0)
                    valResults.Add(new ValidationResult("Must have a deposit premium amount if the risk is marked paid in installments.", new[] { "PremiumInfo.DepositPremium" }));
            }

            if (r.Status == RiskStatus.DECLINED.Value)
            {
                //Annual Prem is already required for all statuses quote and beyond. Set this here so that it is also required for declined for workers comp.
                if (r.PremiumInfo.AnnualizedPremium == 0)
                    valResults.Add(new ValidationResult("Must enter an annualized premium amount", new string[] { "PremiumInfo.AnnualizedPremium" }));
            }
        }

        /// <summary>
        /// Updates the risk.
        /// </summary>
        /// <param name="risk">Current risk being updated</param>
        /// <returns>Risk with updated changes.</returns>
        public Risk UpdateRisk(Risk risk)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var dbRisk = ServiceLocator.RiskService.GetRisk(risk.Id);
            var valResults = new List<ValidationResult>();

            // Validate Risk ============
            ValidateRisk(valResults, risk);
            if (risk.ProductLine == (int)ProductLines.WC) 
                ValidateRiskForWorkersComp(valResults, risk, dbRisk);

            if (valResults.Count > 0) throw new ValidationRulesException(valResults);
            // End Validations ===========

            RiskDefaults(risk);
            UpdateNamedInsuredCurrentBroker(ref risk, dbRisk);
            if (dbRisk.Status != risk.Status && risk.Status == RiskStatus.ISSUED.Value) IssuePolicy(risk);
            Save(risk);
            LogRiskHistory(risk, dbRisk);
            ServiceLocator.RiskService.LogRiskAction(risk.Id, "Updated");

            return risk;
        }

        public void UpdateNamedInsuredCurrentBroker(ref Risk risk, Risk dbRisk)
        {
            var newestRiskId = ServiceLocator.EntityService.GetTopNList<Risk>(1,
                new PropertyFilter("ControlNumber", risk.NamedInsured.ControlNumber),
                new[] { new OrderFilter("Id", OrderFilter.Comparator.Descending) }
                ).FirstOrDefault();

            if (risk.NamedInsured.AgencyID != risk.AgencyID && risk.Id == newestRiskId.Id) risk.NamedInsured.AgencyID = risk.AgencyID;

            if (dbRisk.AgencyID != risk.AgencyID)
            {
                var broker = ServiceLocator.EntityService.GetInstance<Broker>(new PropertyFilter("AgencyID", risk.AgencyID));
                risk.BrokerStatus = (broker.PlatinumProducer) ? BrokerCommissionStatus.PLATINUM.Value : BrokerCommissionStatus.BASE.Value;
            }
            if (dbRisk.AgentId != risk.AgentId) SetAgentInfo(risk, risk.AgentId);
        }

        private void LogRiskHistory(Risk risk, Risk dbRisk)
        {
            if (risk.Status != dbRisk.Status)
            {
                ServiceLocator.RiskService.LogRiskAction(risk.Id, String.Format("Moved to status {0}", risk.Status));
            }
        }

        public Risk IssuePolicy(Risk policy)
        {
            policy.ImageRightId = policy.Prefix + policy.PolicyNumber;
            policy.Status = RiskStatus.ISSUED.Value;
            policy.PremiumInfo.InceptionPremium = policy.PremiumInfo.AnnualizedPremium;

            return policy;
        }

        public Risk CancelPolicy(Risk policy)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();

            policy.Status = RiskStatus.CANCELED.Value;
            ServiceLocator.RiskService.LogRiskAction(policy.Id, "Policy Moved to Cancelled Status");

            if (policy.RenewalQuoteId != 0)
            {
                Risk policyRenewal = ServiceLocator.EntityService.GetInstance<Risk>(new PropertyFilter("Id", policy.RenewalQuoteId));
                DAOFactory.GetDAO<RiskDAO>().Delete(uow, policyRenewal);
            }

            return policy;
        }

        public Risk CloneRisk(int id)
        {
            var risk = ServiceLocator.RiskService.GetRisk(id);
            var newRisk = (Risk)risk.Copy();

            newRisk.Id = 0;
            newRisk.ParentID = risk.Id;
            newRisk.Status = "Already Involved";

            return newRisk;
        }
        
        public RiskPayment SavePayment(RiskPayment payment)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            RiskInstallmentInfo riskInstallmentInfo = DAOFactory.GetDAO<RiskInstallmentInfoDAO>().GetInstance(uow, new PropertyFilter("RiskId", payment.RiskId));
            Risk risk;

            if (payment.PaymentId != 0)
            {
                DAOFactory.GetDAO<RiskPaymentDAO>().Update(uow, payment);
                ServiceLocator.RiskService.LogRiskAction(payment.RiskId, String.Format("Payment {0} updated", payment.PaymentId));
            }
            else
            {
                DAOFactory.GetDAO<RiskPaymentDAO>().Create(uow, payment);
                riskInstallmentInfo.PaymentTerms += 1;
                ServiceLocator.RiskService.LogRiskAction(payment.RiskId, String.Format("Payment {0} created", payment.PaymentId));
            }

            risk = DAOFactory.GetDAO<RiskDAO>().GetInstance(uow, new PropertyFilter("Id", payment.RiskId));
            var months = Math.Round(risk.ExpirationDate.Value.Subtract(risk.EffectiveDate.Value).Days / (365.25 / 12));
            var riskPayments = DAOFactory.GetDAO<RiskPaymentDAO>().GetList(uow, new PropertyFilter("RiskId", payment.RiskId));

            riskInstallmentInfo.EstimatedPremPerMonth = Math.Round(riskPayments.Sum(x => x.AnticipatedAmount) / Convert.ToDecimal(months), 2);
            riskInstallmentInfo.ActualPremPerMonth = Math.Round(riskPayments.Sum(x => x.ActualAmount) / Convert.ToDecimal(months), 2); ;

            DAOFactory.GetDAO<RiskInstallmentInfoDAO>().Update(uow, riskInstallmentInfo);

            return payment;
        }

        public void DeletePayment(int paymentId)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var riskPayment = DAOFactory.GetDAO<RiskPaymentDAO>().GetInstance(uow, new PropertyFilter("PaymentId", paymentId));
            var riskInstallInfo = DAOFactory.GetDAO<RiskInstallmentInfoDAO>().GetInstance(uow, new PropertyFilter("RiskId", riskPayment.RiskId));
            riskInstallInfo.PaymentTerms -= 1;

            DAOFactory.GetDAO<RiskPaymentDAO>().Delete(uow, riskPayment);
            ServiceLocator.RiskService.LogRiskAction(riskPayment.RiskId, String.Format("Payment {0} deleted", paymentId));
            DAOFactory.GetDAO<RiskInstallmentInfoDAO>().Update(uow, riskInstallInfo);
        }

        public RiskEndorsement CreateEndorsement(RiskEndorsement endorsement)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            RiskPremiumInfo riskPremium;

            DAOFactory.GetDAO<RiskEndorsementDAO>().Create(uow, endorsement);
            ServiceLocator.RiskService.LogRiskAction(endorsement.RiskId, String.Format("Endorsement {0} created", endorsement.Code));
            riskPremium = DAOFactory.GetDAO<RiskPremiumInfoDAO>().GetInstance(uow, new PropertyFilter("RiskId", endorsement.RiskId));
            riskPremium.AnnualizedPremium += Convert.ToDecimal(endorsement.Premium);
            riskPremium.WrittenPremium += Convert.ToDecimal(endorsement.InvoicedPremium);
            DAOFactory.GetDAO<RiskPremiumInfoDAO>().Update(uow, riskPremium);

            return endorsement;
        }

        public RiskEndorsement UpdateEndorsement(RiskEndorsement endorsement)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            RiskPremiumInfo riskPremium;
            
            var origEndo = DAOFactory.GetDAO<RiskEndorsementDAO>().GetInstance(uow, new PropertyFilter("Id", endorsement.Id));
            DAOFactory.GetDAO<RiskEndorsementDAO>().Update(uow, endorsement);
            riskPremium = DAOFactory.GetDAO<RiskPremiumInfoDAO>().GetInstance(uow, new PropertyFilter("RiskId", endorsement.RiskId));

            if (origEndo.Premium != endorsement.Premium)
                riskPremium.AnnualizedPremium += (Convert.ToDecimal(endorsement.Premium) - Convert.ToDecimal(origEndo.Premium));
            if (origEndo.InvoicedPremium != endorsement.InvoicedPremium)
                riskPremium.WrittenPremium += (Convert.ToDecimal(endorsement.InvoicedPremium) - Convert.ToDecimal(origEndo.InvoicedPremium));

            DAOFactory.GetDAO<RiskPremiumInfoDAO>().Update(uow, riskPremium);

            return endorsement;
        }

        public void DeleteEndorsement(int endorsementId)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var riskEndorsement = DAOFactory.GetDAO<RiskEndorsementDAO>().GetInstance(uow, new PropertyFilter("Id", endorsementId));
            var riskPremium = DAOFactory.GetDAO<RiskPremiumInfoDAO>().GetInstance(uow, new PropertyFilter("RiskId", riskEndorsement.RiskId));
            riskPremium.AnnualizedPremium -= Convert.ToDecimal(riskEndorsement.Premium);
            riskPremium.WrittenPremium -= Convert.ToDecimal(riskEndorsement.InvoicedPremium);

            DAOFactory.GetDAO<RiskEndorsementDAO>().Delete(uow, riskEndorsement);
            ServiceLocator.RiskService.LogRiskAction(riskEndorsement.RiskId, String.Format("Endorsement {0} deleted", riskEndorsement.Code));
            DAOFactory.GetDAO<RiskPremiumInfoDAO>().Update(uow, riskPremium);
        }

        /// <summary>
        /// Used by the reason ComboBox to get reasons for the current selected status
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public List<StatusReason> GetStatusReasons(string status)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var reasons = DAOFactory.GetDAO<StatusReasonDAO>().GetList(uow, new PropertyFilter("Status", status)).ToList();
            return reasons;
        }

        public List<User> GetUWsForBranch(string branch)
        {
            var uow = UnitOfWorkFactory.CreateUnitOfWork(UnitOfWorkFactory._UWBASE_CONTEXT);
            var results = DAOFactory.GetDAO<UserEntityDAO>().GetList(uow, new PropertyFilter("BranchID", branch), new OrderFilter("FirstName")).ToList();
            results.Insert(0, new User() { UserId = 0, FirstName = "* UNASSIGNED *", BranchID = "NONE" });
            uow.Finish();
            return results;
        }

        public IEnumerable<ProductLine> GetProductLines()
        {
            var uow = UnitOfWorkFactory.CreateUnitOfWork(UnitOfWorkFactory._UWBASE_CONTEXT);
            var results = DAOFactory.GetDAO<ProductLinesDAO>().GetList(uow);
            uow.Finish();
            return results;
        }

        public Risk PrepareRiskForCommApp(Risk risk)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();

            if (risk.Status == RiskStatus.SUBMISSION.Value)
            {
                risk.Status = RiskStatus.QUOTE.Value;
                DAOFactory.GetDAO<RiskDAO>().Update(uow, risk);
            }

            return risk;
        }

        public RiskAudit SaveRiskAudit(RiskAudit audit)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();

            if (audit.AuditId != 0) DAOFactory.GetDAO<RiskAuditDAO>().Update(uow, audit);
            else DAOFactory.GetDAO<RiskAuditDAO>().Create(uow, audit);

            return audit;
        }

        public void DeleteAudit(int auditId)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            DAOFactory.GetDAO<RiskAuditDAO>().Delete(uow, new PropertyFilter("AuditId", auditId));
        }

        /// <summary>
        /// Checks the OFAC (Office of Foreign Assets Control) SDN (Specially Designated Nationals List) for any hits on the name passed. Endures we are not doing business with undesirables.
        /// </summary>
        /// <param name="name">Name to check against the List</param>
        /// <returns>List of Hits</returns>
        public IEnumerable<string> CheckOFACSDNListForName(string name)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            return DAOFactory.GetDAO<RiskDAO>().CheckOFACSDN(uow, name);
        }

        public void OFACSDNIssueNotification(string namedInsured)
        {
            var mail = new MailMessage();
            var smtp = new SmtpClient();

            mail.From = new MailAddress(ConfigurationManager.AppSettings["MailFrom"]);
            mail.To.Add(ConfigurationManager.AppSettings["ComplianceLicensing"]);
            mail.Subject = "OFAC SDN Failure";
            mail.IsBodyHtml = true;

            var body = new StringBuilder(String.Format("<h3>OFAC SDN Issue In Policy Tracker For Named Insured {0}</h3>", namedInsured));
            var user = SessionManager.GetCurrentSession().User;
            body.Append(String.Format("<p>{0} {1} attempted to enter the risk</p>", user.FirstName, user.LastName));
            body.Append(String.Format("<p>Please notify {0} ({1}) how to proceed.</p>", user.Name, user.WorkEmail));

            mail.Body = body.ToString();
            smtp.Send(mail);
        }
    }
}