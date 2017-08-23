using PolicyTracker.DomainModel.Framework;
using PolicyTracker.DomainModel.Policy;
using PolicyTracker.Platform.UOW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolicyTracker.DataAccess.Policy
{
    public class MonthlyBudgetsDAO : BaseDAO<MonthlyBudget>
    {
        public MonthlyBudgetsDAO()
            : base("MonthlyBudgets", "UW_Base_App", defaultOrderFilter: new OrderFilter("BudgetId", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("BudgetId", "BudgetId", true, true);
            AddColumnMapping("ProductLine", "ProductLine");
            AddColumnMapping("Branch", "Branch");
            AddColumnMapping("Amount", "Amount");
            AddColumnMapping("DateMonth", "DateMonth");
            AddColumnMapping("DateYear", "DateYear");
            Initialize();
        }
    }

    public class ProductLinesDAO : BaseDAO<ProductLine>
    {
        public ProductLinesDAO()
            : base("ProductLines", "UW_Base_App", defaultOrderFilter: new OrderFilter("ProductLineId", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("ProductLineId", "ProductLineId", true, true);
            AddColumnMapping("Name", "ProductLine");
            Initialize();
        }
    }

    public class ProductsDAO : BaseDAO<Product>
    {
        public ProductsDAO()
            : base("Products", "UW_Base_App", defaultOrderFilter: new OrderFilter("ProductId", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("ProductId", "ProductId", true, true);
            AddColumnMapping("Prefix", "Prefix");
            AddColumnMapping("Description", "Product");
            AddColumnMapping("ProductLineId", "ProductLineId");
            AddColumnMapping("QuoteType", "QuoteType");
            AddColumnMapping("IsActive", "IsActive");
            Initialize();
        }
    }

    public class RiskDAO : BaseDAO<Risk>
    {
        public RiskDAO()
            : base("QuoteLookup", "UW_Base_App", defaultOrderFilter: new OrderFilter("Id", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("Id", "QuoteId", true, true);
            AddColumnMapping("QuoteType", "QuoteType");
            AddColumnMapping("ControlNumber", "ControlNumber");
            AddColumnMapping("PolicyNumber", "PolicyNumber");
            AddColumnMapping("PolicySuffix", "PolicySuffix");
            AddColumnMapping("CompletedWizardId", "CompletedWizardId");
            AddColumnMapping("Status", "Status");
            AddColumnMapping("CreatedById", "CreatedById");
            AddColumnMapping("CreatedDate", "CreatedDate");
            AddColumnMapping("UpdatedById", "UpdatedById");
            AddColumnMapping("UpdatedDate", "UpdatedDate");
            AddColumnMapping("isMidTerm", "isMidTerm");
            AddColumnMapping("UnderwriterId", "UnderwriterId");
            AddColumnMapping("StatusReason", "StatusReason");
            AddColumnMapping("StatusMarket", "StatusMarket");
            AddColumnMapping("ParentID", "ParentID");
            AddColumnMapping("QuoteSubmissionTypeID", "QuoteSubmissionTypeID");
            AddColumnMapping("ImageRightId", "ImageRightId");
            AddColumnMapping("UnderwriterAssistantId", "UnderwriterAssistantId");
            AddColumnMapping("RenewalUnderwriterId", "RenewalUnderwriterId");
            AddColumnMapping("ProductLine", "ProductLine");
            AddColumnMapping("Branch", "Branch");
            AddColumnMapping("IsTargetAccount", "IsTargetAccount");
            AddColumnMapping("Prefix", "Prefix");

            AddColumnMapping("ApplicationDate", "ApplicationDate");
            AddColumnMapping("ClassofBusinessGroupCodesId", "ClassofBusinessGroupCodesId");
            AddColumnMapping("PaidInInstallments", "PaidInInstallments");
            AddColumnMapping("EffectiveDate", "EffectiveDate");
            AddColumnMapping("ExpirationDate", "ExpirationDate");
            AddColumnMapping("APLossRatio", "APLossRatio");
            AddColumnMapping("TRIALiability", "TRIALiability");
            AddColumnMapping("WARLiability", "WARLiability");
            AddColumnMapping("Commission", "Commission");
            AddColumnMapping("BrokerComments", "BrokerComments");
            AddColumnMapping("BrokerStatus", "BrokerStatus");
            AddColumnMapping("IsCommissionVisible", "IsCommissionVisible");
            AddColumnMapping("TriaPercentage", "TriaPercentage");
            AddColumnMapping("WarPercentage", "WarPercentage");
            AddColumnMapping("AgencyID", "AgencyID");
            AddColumnMapping("AgentName", "AgentName");
            AddColumnMapping("AgentEmail", "AgentEmail");
            AddColumnMapping("QuotedDate", "QuotedDate");
            AddColumnMapping("AgentId", "AgentId");
            AddColumnMapping("PurposeOfUse", "PurposeOfUse");
            AddColumnMapping("AirportId", "AirportCode");
            AddColumnMapping("LeadInsurer", "LeadInsurer");
            AddColumnMapping("ORAPercent", "ORAPercent");
            AddColumnMapping("Market", "Market");
            AddColumnMapping("AppReceived", "AppReceived");
            AddColumnMapping("IsMexicanPolicy", "IsMexicanPolicy");
            AddColumnMapping("IsNet", "IsNet");

            //AVC Specific fields that were copied from QuoteAVCPolicyLevel
            AddColumnMapping("AirportCategory", "AirportCategory");
            AddColumnMapping("AirportName", "AirportName");
            AddColumnMapping("AS400PolicyUseCodeId", "AS400PolicyUseCodeId");
            AddColumnMapping("AVLossRatio", "AVLossRatio");
            AddColumnMapping("DecPageCSL", "DecPageCSL");
            AddColumnMapping("DecPageLiabLimit", "DecPageLiabLimit");
            AddColumnMapping("DecPageSubLiabLimit", "DecPageSubLiabLimit");
            AddColumnMapping("DecPageSubLimitType", "DecPageSubLimitType");
            AddColumnMapping("DecUse", "DecUse");
            AddColumnMapping("Reporter", "Reporter");
            AddColumnMapping("RenewalQuoteId", "RenewalQuoteId");
            AddColumnMapping("IsDoNotRenew", "IsDoNotRenew");
            AddColumnMapping("CancelDate", "CancelDate");
            AddColumnMapping("HasFailedOFAC", "HasFailedOFAC");

            AddColumnMapping("InsuredType", "InsuredType");
            AddColumnMapping("FirstName", "FirstName");
            AddColumnMapping("LastName", "LastName");
            AddColumnMapping("CompanyName", "CompanyName");
            AddColumnMapping("Fein", "Fein");
            AddColumnMapping("StreetAddress1", "StreetAddress1");
            AddColumnMapping("StreetAddress2", "StreetAddress2");
            AddColumnMapping("City", "City");
            AddColumnMapping("State", "State");
            AddColumnMapping("Zip", "Zip");
            AddColumnMapping("DoingBusinessAs", "DoingBusinessAs");
            AddColumnMapping("MiddleInitial", "MiddleInitial");
            AddColumnMapping("RenewalOf", "RenewalOf");
            AddColumnMapping("Rating", "Rating");

            var relatedTables = new List<ORTableRelation>();
            relatedTables.Add(new ORTableRelation() { Table = "RiskPremiumInfo", Property = "Id", RelationProperty = "RiskId" });
            relatedTables.Add(new ORTableRelation() { Table = "RiskInstallmentInfo", Property = "Id", RelationProperty = "RiskId" });

            ORCatalog.AddJoinMetadata(new ORJoinMetadata() { EntityName = "Risk", RelatedTables = relatedTables });

            Initialize();
        }

        public IEnumerable<dynamic> GetUniquePolicies(UnitOfWork uow)
        {
            //var sql = new StringBuilder("SELECT	*");
            //sql.Append(" FROM	(	SELECT	CASE WHEN CompanyName <> '' THEN CompanyName ELSE FirstName + ' ' + LastName END AS NAME, PREFIX, POLICYNUMBER");
            //sql.Append("            FROM	QuoteNamedInsured JOIN QuoteLookUp ON QuoteNamedInsured.ControlNumber = QuoteLookUp.ControlNumber ");
            //sql.Append("        ) TBL ");
            //sql.Append(" WHERE	Prefix <> '' AND PolicyNumber <> '' ");
            //sql.Append(" GROUP BY NAME, Prefix, PolicyNumber ");

            var sql = new StringBuilder("SELECT Prefix, PolicyNumber FROM QuoteLookUp WHERE	Prefix <> '' AND PolicyNumber <> ''");
            //sql.Append(string.Format(" AND Prefix = '{0}' AND PolicyNumber = '{1}'", "CAV", "017089"));
            sql.Append(" GROUP BY Prefix, PolicyNumber HAVING COUNT(PREFIX) > 1");

            var result = Query<dynamic>(uow, sql.ToString());
            return result;
        }

        /// <summary>
        /// Get a List of the RiskIds that are expiring in 90 days to generate renewals
        /// </summary>
        /// <param name="uow">Unit of Work</param>
        /// <returns>List of RiskIds</returns>
        public IEnumerable<dynamic> GetExpiringRisks(UnitOfWork uow)
        {
            var sql = new StringBuilder("SELECT QL.QuoteId as RiskId, QL.PolicyNumber, QL.RenewalPolicyNumber, QL.[Status], QL.ExpirationDate, QL.RenewalQuoteId, QL.[IsDoNotRenew]");
            sql.Append(" FROM (Select QuoteId, ExpirationDate, RenewalQuoteId, [Status], IsDoNotRenew, Prefix + PolicyNumber + PolicySuffix as PolicyNumber, ");
            sql.Append(" Prefix + PolicyNumber + " +
					" case when len(Cast((CAST(PolicySuffix as int) + 1) as varchar(3))) = 1 then '0' + Cast((CAST(PolicySuffix as int) + 1) as varchar(3)) " +
					" else Cast((CAST(PolicySuffix as int) + 1) as varchar(3)) end as RenewalPolicyNumber from QuoteLookUp) QL ");
            sql.Append(" left outer join (select Prefix + PolicyNumber + PolicySuffix as PolicyNumber from QuoteLookUp) PN on PN.PolicyNumber = QL.RenewalPolicyNumber");
            sql.Append(" WHERE ((Month(ExpirationDate) <= Month(DATEADD(MM, 3, getdate())) AND YEAR(ExpirationDate) <= YEAR(DATEADD(MM, 3, getdate()))) or (ExpirationDate < DATEADD(s, -1, DATEADD(mm, DATEDIFF(m, 0, GETDATE()) + 4, 0))))");
            sql.Append(" AND (RenewalQuoteId IS NULL OR RenewalQuoteId = 0)");
            sql.Append(" AND QL.[Status] = 'Issued'");
            sql.Append(" AND QL.[IsDoNotRenew] = 0 and ExpirationDate > getdate()");
            //Added and expirationdate > getdate() so that we wouldn't generate a renewal for something that has already passed. This is really only to stop old 
            //policies from being generated when a broker is being moved from no new business/renewals to being allowed to do new business and renewals with us again.

            var result = Query<dynamic>(uow, sql.ToString());
            return result;
        }

        /// <summary>
        /// Gets the summary of product line Policy count. For policies that are not expired and inforce.
        /// </summary>
        /// <param name="uow">Unit of Work</param>
        /// <param name="branch">Limit to a single Branch of business</param>
        /// <returns>List of Product Line and Total Premium Value</returns>
        public IEnumerable<dynamic> GetProductLinePolicyCounts(UnitOfWork uow, string branch = null)
        {
            var sql = new StringBuilder("SELECT PL.ProductLine, COUNT(QL.QuoteId) TotalRisks");
            sql.Append(" FROM QuoteLookUp QL JOIN ProductLines PL ON PL.ProductLineId = QL.ProductLine");
            if (!String.IsNullOrEmpty(branch)) sql.Append(String.Format(" WHERE branch = '{0}'", branch));
            sql.Append(" and ExpirationDate > GETDATE() and QL.[Status] in ('Issued', 'Bound')");
            sql.Append(" GROUP BY PL.ProductLine");

            var results = Query<dynamic>(uow, sql.ToString());

            return results;
        }

        /// <summary>
        /// Gets the summary of product line premiums. For policies that are not expired and inforce.
        /// </summary>
        /// <param name="uow">Unit of Work</param>
        /// <param name="branch">Limit to a single Branch of business</param>
        /// <returns>List of Product Line and Total Count</returns>
        public IEnumerable<dynamic> GetProductLinePolicyPremiums(UnitOfWork uow, string branch = null)
        {
            var sql = new StringBuilder("SELECT PL.ProductLine, SUM(RPI.AnnualizedPremium) TotalPremium");
            sql.Append(" FROM QuoteLookUp QL JOIN ProductLines PL ON PL.ProductLineId = QL.ProductLine");
            sql.Append(" JOIN RiskPremiumInfo RPI ON RPI.RiskId = QL.QuoteId");
            if (!String.IsNullOrEmpty(branch)) sql.Append(String.Format(" WHERE branch = '{0}'", branch));
            sql.Append(" and ExpirationDate > GETDATE() and QL.[Status] in ('Issued', 'Bound')");
            sql.Append(" GROUP BY PL.ProductLine");

            var results = Query<dynamic>(uow, sql.ToString());

            return results;
        }

        /// <summary>
        /// Checks the OFAC (Office of Foreign Assets Control) SDN (Specially Designated Nationals List) for any hits on the name passed. Endures we are not doing business with undesirables.
        /// </summary>
        /// <param name="uow">Unit of Work</param>
        /// <param name="name">Name to check against the List</param>
        /// <returns>List of Hits</returns>
        public IEnumerable<string> CheckOFACSDN(UnitOfWork uow, string name)
        {
            //name = name.Replace("\'", "''''").Trim();
            name = name.Trim();
            var results = ExecuteStoredProcedureList<string>(uow, "AS400_OFAC_Check", new { InsuredName = name, TSQL = String.Empty });
            return results;
        }
    }

    public class AVCRiskPolicyInfoDAO : BaseDAO<AVCRiskPolicyInfo>
    {
        public AVCRiskPolicyInfoDAO()
            : base("QuoteAVCPolicyLevel", "UW_Base_App", defaultOrderFilter: new OrderFilter("QuotePolicyId", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("QuotePolicyId", "QuotePolicyId", true, true);
            AddColumnMapping("RiskId", "QuoteId");
            AddColumnMapping("ClassofBusinessGroupCodesId", "ClassofBusinessGroupCodesId");
            AddColumnMapping("AS400PolicyUseCodeId", "AS400PolicyUseCodeId");
            AddColumnMapping("PaidInInstallments", "PaidInInstallments");
            AddColumnMapping("EffectiveDate", "EffectiveDate");
            AddColumnMapping("ExpirationDate", "ExpirationDate");
            AddColumnMapping("AVLossRatio", "AVLossRatio");
            AddColumnMapping("Reporter", "Reporter");
            AddColumnMapping("Commission", "Commission");
            AddColumnMapping("AirportCode", "AirportCode");
            AddColumnMapping("AirportName", "AirportName");
            AddColumnMapping("AirportCategory", "AirportCategory");
            AddColumnMapping("CreatedById", "CreatedById");
            AddColumnMapping("CreatedDate", "CreatedDate");
            AddColumnMapping("UpdatedById", "UpdatedById");
            AddColumnMapping("UpdatedDate", "UpdatedDate");
            AddColumnMapping("BrokerComments", "BrokerComments");
            AddColumnMapping("BrokerStatus", "BrokerStatus");
            AddColumnMapping("DecPageLiabLimit", "DecPageLiabLimit");
            AddColumnMapping("DecPageCSL", "DecPageCSL");
            AddColumnMapping("DecPageSubLiabLimit", "DecPageSubLiabLimit");
            AddColumnMapping("DecPageSubLimitType", "DecPageSubLimitType");
            AddColumnMapping("AgencyID", "AgencyID");
            AddColumnMapping("AgentName", "AgentName");
            AddColumnMapping("AgentEmail", "AgentEmail");
            AddColumnMapping("DecUse", "DecUse");
            Initialize();
        }
    }

    public class RiskWorkersCompInfoDAO : BaseDAO<RiskWorkersCompInfo>
    {
        public RiskWorkersCompInfoDAO()
            : base("RiskWorkersCompInfo", "UW_Base_App", defaultOrderFilter: new OrderFilter("Id", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("Id", "Id", true, true);
            AddColumnMapping("RiskId", "RiskId");
            AddColumnMapping("ProgramType", "ProgramType");
            AddColumnMapping("AccountDescription", "AccountDescription");
            AddColumnMapping("ScheduledRating", "ScheduledRating");
            AddColumnMapping("ExperienceModifier", "ExpirienceModifier");
            AddColumnMapping("Payroll", "Payroll");
            AddColumnMapping("ExpiringPayroll", "ExpiringPayroll");
            AddColumnMapping("PayrollChange", "PayrollChange");
            AddColumnMapping("NetRate", "NetRate");
            AddColumnMapping("ExpiringRate", "ExpiringRate");
            AddColumnMapping("NetRateChange", "NetRateChange");
            AddColumnMapping("IsPassengerCarrying", "IsPassengerCarrying");
            Initialize();
        }
    }

    public class RiskAuditDAO : BaseDAO<RiskAudit>
    {
        public RiskAuditDAO()
            : base("RiskAudits", "UW_Base_App", defaultOrderFilter: new OrderFilter("AuditId", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("AuditId", "AuditId", true, true);
            AddColumnMapping("RiskId", "RiskId");
            AddColumnMapping("AuditDate", "AuditDate");
            AddColumnMapping("Amount", "Amount");
            Initialize();
        }

        public IEnumerable<dynamic> GetAuditAmountByBranch(UnitOfWork uow, DateTime startDate, DateTime endDate)
        {
            var sql = new StringBuilder();
            sql.Append("SELECT	QL.Branch, SUM(Amount) Amount");
            sql.Append(" FROM	RiskAudits RA");
            sql.Append(" JOIN QuoteLookUp QL ON QL.QuoteId = RA.RiskId");
            sql.Append(String.Format(" WHERE	AuditDate BETWEEN '{0}' AND '{1}'", startDate, endDate));
            sql.Append(" GROUP BY QL.BRANCH");

            var results = Query<dynamic>(uow, sql.ToString());
            return results;
        }

        public IEnumerable<dynamic> GetAuditAmount(UnitOfWork uow, int year, string branch = null, int underwriterId = 0)
        {
            var sql = new StringBuilder();
            sql.Append("SELECT	QL.Branch, Amount, MONTH(AuditDate) AuditMonth, YEAR(AuditDate) AuditYear");
            sql.Append(" FROM	RiskAudits RA");
            sql.Append(" JOIN QuoteLookUp QL ON QL.QuoteId = RA.RiskId");
            sql.Append(String.Format(" WHERE Year(AuditDate) = {0}", year));

            if (!string.IsNullOrEmpty(branch))
                sql.Append(String.Format(" and QL.Branch = '{0}'", branch));
            if (underwriterId != 0)
                sql.Append(String.Format(" and QL.UnderwriterId = '{0}'", underwriterId));

            var results = Query<dynamic>(uow, sql.ToString());

            return results;
        }
    }

    public class RiskPremiumInfoDAO : BaseDAO<RiskPremiumInfo>
    {
        public RiskPremiumInfoDAO()
            : base("RiskPremiumInfo", "UW_Base_App", defaultOrderFilter: new OrderFilter("Id", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("Id", "Id", true, true);
            AddColumnMapping("RiskId", "RiskId");
            AddColumnMapping("DepositPremium", "DepositPremium");
            AddColumnMapping("ExpiredAnnualizedPremium", "ExpiredAnnualizedPremium");
            AddColumnMapping("ExpiringEarnedPremium", "ExpiringEarnedPremium");
            AddColumnMapping("PremiumRenewalChange", "PremiumRenewalChange");
            AddColumnMapping("PremiumChangeReasonCode", "PremiumChangeReasonCode");
            AddColumnMapping("AnnualizedPremium", "AnnualizedPremium");
            AddColumnMapping("IsMinDepositPercentage", "IsMinDepositPercentage");
            AddColumnMapping("DepositPercentage", "DepositPercentage");
            AddColumnMapping("EarnedPremium", "EarnedPremium");
            AddColumnMapping("WrittenPremium", "WrittenPremium");
            AddColumnMapping("ExpiringWrittenPremium", "ExpiringWrittenPremium");
            AddColumnMapping("InceptionPremium", "InceptionPremium");
            Initialize();
        }
    }

    public class RiskInstallmentInfoDAO : BaseDAO<RiskInstallmentInfo>
    {
        public RiskInstallmentInfoDAO()
            : base("RiskInstallmentInfo", "UW_Base_App", defaultOrderFilter: new OrderFilter("Id", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("Id", "Id", true, true);
            AddColumnMapping("RiskId", "RiskId");
            AddColumnMapping("IsPaidInInstallments", "IsPaidInInstallments");
            AddColumnMapping("IsReporter", "IsReporter");
            AddColumnMapping("EstimatedPremPerMonth", "EstimatedPremPerMonth");
            AddColumnMapping("ActualPremPerMonth", "ActualPremPerMonth");
            AddColumnMapping("PaymentTerms", "PaymentTerms");
            Initialize();
        }
    }

    public class NamedInsuredDAO : BaseDAO<NamedInsured>
    {
        public NamedInsuredDAO()
            : base("QuoteNamedInsured", "UW_Base_App", defaultOrderFilter: new OrderFilter("ControlNumber", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("ControlNumber", "ControlNumber", true, true);
            AddColumnMapping("AgencyID", "AgencyID");
            AddColumnMapping("Date", "Date");
            AddColumnMapping("InsuredType", "InsuredType");
            AddColumnMapping("FirstName", "FirstName");
            AddColumnMapping("LastName", "LastName");
            AddColumnMapping("CompanyName", "CompanyName");
            AddColumnMapping("Fein", "Fein");
            AddColumnMapping("CompanyWebsite", "CompanyWebsite");
            AddColumnMapping("StreetAddress1", "StreetAddress1");
            AddColumnMapping("StreetAddress2", "StreetAddress2");
            AddColumnMapping("City", "City");
            AddColumnMapping("State", "State");
            AddColumnMapping("Zip", "Zip");
            AddColumnMapping("isRenewal", "isRenewal");
            AddColumnMapping("ExPolicyNumberAVC", "ExPolicyNumberAVC");
            AddColumnMapping("ExPolicyNumberAP", "ExPolicyNumberAP");
            AddColumnMapping("Underwriter", "Underwriter");
            AddColumnMapping("IssuingCompany", "IssuingCompany");
            AddColumnMapping("WorkersCompSB", "WorkersCompSB");
            AddColumnMapping("CorporateSB", "CorporateSB");
            AddColumnMapping("PBSupportBusiness", "PBSupportBusiness");
            AddColumnMapping("AirportMunicipalitiesSB", "AirportMunicipalitiesSB");
            AddColumnMapping("WorkersCompSBNotes", "WorkersCompSBNotes");
            AddColumnMapping("CorporateSBNotes", "CorporateSBNotes");
            AddColumnMapping("PBSupportBusinessNotes", "PBSupportBusinessNotes");
            AddColumnMapping("AirportMunicipalitiesSBNotes", "AirportMunicipalitiesSBNotes");
            AddColumnMapping("BusinessDescription", "BusinessDescription");
            AddColumnMapping("YearsInBusiness", "YearsInBusiness");
            AddColumnMapping("NewBusiness", "NewBusiness");
            AddColumnMapping("ExAPAnnualPremium", "ExAPAnnualPremium");
            AddColumnMapping("ExAVCAnnualPremium", "ExAVCAnnualPremium");
            AddColumnMapping("ProducerComments", "ProducerComments");
            AddColumnMapping("ClassofBusinessGroupCodesId", "ClassofBusinessGroupCodesId");
            AddColumnMapping("IsDeleted", "IsDeleted");
            AddColumnMapping("CreatedById", "CreatedById");
            AddColumnMapping("CreatedDate", "CreatedDate");
            AddColumnMapping("UpdatedById", "UpdatedById");
            AddColumnMapping("UpdatedDate", "UpdatedDate");
            AddColumnMapping("StateTaxRate", "StateTaxRate");
            AddColumnMapping("Email", "Email");
            AddColumnMapping("PhoneNumber", "PhoneNumber");
            AddColumnMapping("IsFBOClient", "FBOClient");
            AddColumnMapping("DoingBusinessAs", "DoingBusinessAs");
            AddColumnMapping("MiddleInitial", "MiddleInitial");
            Initialize();
        }
    }

    public class WorkingListDAO : BaseDAO<WorkingListItem>
    {
        public WorkingListDAO()
            : base("PolicyTrackerWorkingList", "UW_Base_App", defaultOrderFilter: new OrderFilter("RiskId", OrderFilter.Comparator.Descending))
        {
            AddColumnMapping("RiskId", "QuoteId");
            AddColumnMapping("Branch", "Branch");
            AddColumnMapping("ImageRightId", "ImageRightId");
            AddColumnMapping("ProductLine", "ProductLine");
            AddColumnMapping("PolicyNumber", "PolicyNumber");
            AddColumnMapping("ControlNumber", "ControlNumber");
            AddColumnMapping("Status", "Status");
            AddColumnMapping("EffectiveDate", "EffectiveDate");
            AddColumnMapping("ExpirationDate", "ExpirationDate");
            AddColumnMapping("CreatedDate", "CreatedDate");
            AddColumnMapping("AgencyID", "AgencyID");
            AddColumnMapping("AgencyName", "AgencyName");
            AddColumnMapping("FirstName", "FirstName");
            AddColumnMapping("LastName", "LastName");
            AddColumnMapping("CompanyName", "CompanyName");
            AddColumnMapping("Name", "Name");
            AddColumnMapping("UnderwriterId", "UnderwriterId");
            AddColumnMapping("UnderwriterAssistantId", "UnderwriterAssistantId");
            AddColumnMapping("UW", "UW");
            AddColumnMapping("UA", "UA");
            AddColumnMapping("RUW", "RUW");
            AddColumnMapping("AnnualizedPremium", "AnnualizedPremium");
            AddColumnMapping("IsRenewal", "IsRenewal");
            AddColumnMapping("QuoteType", "QuoteType");
            AddColumnMapping("HasNotes", "HasNotes");
            AddColumnMapping("AppReceived", "AppReceived");
            AddColumnMapping("ProgramType", "ProgramType");
            AddColumnMapping("AgentName", "AgentName");
            //AddColumnMapping("Comment", "Comment");
            //AddColumnMapping("RowId", "RowId");
            Initialize();
        }
    }

    public class RiskNotesDAO : BaseDAO<RiskNote>
    {
        public RiskNotesDAO()
            : base("QuoteNotes", "UW_Base_App", defaultOrderFilter: new OrderFilter("Id", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("Id", "QuoteNoteId", true, true);
            AddColumnMapping("RiskId", "QuoteId");
            AddColumnMapping("Comment", "Comment");
            AddColumnMapping("CreatedById", "CreatedById");
            AddColumnMapping("CreatedDate", "CreatedDate");
            AddColumnMapping("UpdatedDate", "UpdatedDate");
            AddColumnMapping("UpdatedById", "UpdatedById");
            AddColumnMapping("ImpactDate", "ImpactDate"); 
            AddColumnMapping("ImpactsPremium", "ImpactsPremium");
            Initialize();
        }

        public IEnumerable<dynamic> GetImpactNotesForDates(UnitOfWork uow, DateTime startDate, DateTime endDate, int productLineId)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT	U.FirstName + ' ' + U.LastName Name, Comment, QN.CreatedDate, ImpactDate, ImpactsPremium, PL.ProductLine");
            sql.Append(" FROM	QuoteNotes QN");
            sql.Append(" JOIN QuoteLookUp QL ON QL.QuoteId = QN.QuoteId ");
            sql.Append(" JOIN ProductLines PL ON PL.ProductLineId = QL.ProductLine");
            sql.Append(" JOIN [USER] U ON U.UserId = QN.CreatedById");
            sql.Append(String.Format(" WHERE ImpactDate BETWEEN '{0}' AND '{1}'", startDate, endDate));
            if (productLineId != 0)
                sql.Append(String.Format(" AND PL.ProductLineId = {0}", productLineId));

            return Query<dynamic>(uow, sql.ToString());
        }
    }

    public class AircraftDAO : BaseDAO<Aircraft>
    {
        public AircraftDAO()
            : base("QuoteAVCAircraft", "UW_Base_App", defaultOrderFilter: new OrderFilter("Id", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("Id", "QuoteAircraftId", true, true);
            AddColumnMapping("QuoteId", "QuoteId");
            AddColumnMapping("FAANo", "FAANo");
            AddColumnMapping("Year", "Year");
            AddColumnMapping("Make", "Make");
            AddColumnMapping("ModelID", "ModelID");
            AddColumnMapping("MaxSeats", "MaxSeats");
            AddColumnMapping("CrewSeats", "CrewSeats");
            AddColumnMapping("PassSeats", "PassSeats");
            AddColumnMapping("Value", "Value");
            AddColumnMapping("AirportID", "AirportID");
            AddColumnMapping("ClassofBusinessGroupCodesId", "ClassofBusinessGroupCodesId");
            AddColumnMapping("EngineType", "EngineType");
            AddColumnMapping("EffectiveDate", "EffectiveDate");
            AddColumnMapping("ExpirationDate", "ExpirationDate");
            AddColumnMapping("HullPrem", "HullPrem");
            AddColumnMapping("LiabPrem", "LiabPrem");
            AddColumnMapping("AnnualPrem", "AnnualPrem");
            AddColumnMapping("WrittenPrem", "WrittenPrem");
            AddColumnMapping("HullFactor", "HullFactor");
            AddColumnMapping("LiabFactor", "LiabFactor");
            AddColumnMapping("CreatedById", "CreatedById");
            AddColumnMapping("CreatedDate", "CreatedDate");
            AddColumnMapping("UpdatedById", "UpdatedById");
            AddColumnMapping("UpdatedDate", "UpdatedDate");
            AddColumnMapping("IsIncluded", "IsIncluded");
            AddColumnMapping("PurposeOfUse", "PurposeOfUse");
            AddColumnMapping("ModelName", "ModelName");
            Initialize();
        }
    }

    public class AircraftLiabilityDAO : BaseDAO<AircraftLiability>
    {
        public AircraftLiabilityDAO()
            : base("QuoteAVCLiability", "UW_Base_App", defaultOrderFilter: new OrderFilter("LiabilityId", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("LiabilityId", "QuoteLiabID", true, true);
            AddColumnMapping("AircraftId", "QuoteAircraftId");
            AddColumnMapping("Limit", "Limit");
            AddColumnMapping("isCSL", "isCSL");
            AddColumnMapping("Sublimit", "Sublimit");
            AddColumnMapping("PerPersonPerPass", "PerPersonPerPass");
            AddColumnMapping("PassExcl", "PassExcl");
            AddColumnMapping("ExpiringLiabRate", "ExpiringLiabRate");
            AddColumnMapping("LiabBookRate", "LiabBookRate");
            AddColumnMapping("Modifier", "Modifier");
            AddColumnMapping("LiabPrem", "LiabPrem");
            AddColumnMapping("ExpiringLiabPrem", "ExpiringLiabPrem");
            AddColumnMapping("isWarLiab", "isWarLiab");
            AddColumnMapping("isTRIALiab", "isTRIALiab");
            AddColumnMapping("WarLimit", "WarLimit");
            AddColumnMapping("WarRate", "WarRate");
            AddColumnMapping("TRIARate", "TRIARate");
            AddColumnMapping("WarLiabPrem", "WarLiabPrem");
            AddColumnMapping("TRIALiabPrem", "TRIALiabPrem");
            AddColumnMapping("TotalLiabPrem", "TotalLiabPrem");
            AddColumnMapping("isAttached", "isAttached");
            AddColumnMapping("CreatedById", "CreatedById");
            AddColumnMapping("CreatedDate", "CreatedDate");
            AddColumnMapping("UpdatedById", "UpdatedById");
            AddColumnMapping("UpdatedDate", "UpdatedDate");
            AddColumnMapping("EffectiveDate", "EffectiveDate");
            AddColumnMapping("ExpirationDate", "ExpirationDate");
            AddColumnMapping("TotalAnnualLiabPrem", "TotalAnnualLiabPrem");
            AddColumnMapping("WrittenPrem", "WrittenPrem");
            AddColumnMapping("isLiabOnly", "isLiabOnly");
            Initialize();
        }
    }

    public class PurposeOfUseDAO : BaseDAO<PurposeOfUse>
    {
        public PurposeOfUseDAO()
            : base("PurposeOfUse", "UW_Base_App", defaultOrderFilter: new OrderFilter("Id", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("Id", "Id", true, true);
            AddColumnMapping("Code", "Code");
            AddColumnMapping("Name", "Name");
            AddColumnMapping("IsActive", "IsActive");
            Initialize();
        }
    }

    public class RiskLogDAO : BaseDAO<RiskLog>
    {
        public RiskLogDAO()
            : base("RiskLog", "UW_Base_App", defaultOrderFilter: new OrderFilter("Id", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("Id", "Id", true, true);
            AddColumnMapping("RiskId", "RiskId");
            AddColumnMapping("UserId", "UserId");
            AddColumnMapping("UserName", "UserName");
            AddColumnMapping("ActionDate", "ActionDate");
            AddColumnMapping("LogAction", "LogAction");
            Initialize();
        }
    }

    public class RiskPaymentDAO : BaseDAO<RiskPayment>
    {
        public RiskPaymentDAO()
            : base("RiskPayments", "UW_Base_App", defaultOrderFilter: new OrderFilter("DueDate", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("PaymentId", "PaymentId", true, true);
            AddColumnMapping("RiskId", "RiskId");
            AddColumnMapping("AnticipatedAmount", "AnticipatedAmount");
            AddColumnMapping("ActualAmount", "ActualAmount");
            AddColumnMapping("InvoicedDate", "InvoicedDate");
            AddColumnMapping("DueDate", "DueDate");
            AddColumnMapping("ProcessedBy", "ProcessedBy");
            AddColumnMapping("ReportReceived", "ReportReceived");
            Initialize();
        }

        public IEnumerable<dynamic> GetProductLinePayments(UnitOfWork uow, string productLine, int year, int month = 0, bool isYearSummary = false)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT	RP.RiskId ,RP.AnticipatedAmount ,PL.ProductLine, QL.Branch, QL.UnderwriterId");
            sql.Append(" FROM " + SourceDB + ".dbo.[QuoteLookUp] QL");
            sql.Append(" JOIN " + SourceDB + ".dbo.[RiskPayments] RP ON RP.RiskId = QL.QuoteId");
            sql.Append(" JOIN " + SourceDB + ".dbo.[ProductLines] PL ON PL.ProductLineId = QL.ProductLine");
            sql.Append(" WHERE	QL.[Status] in ('Bound', 'Issued')");
            sql.Append(String.Format(" AND PL.ProductLine = '{0}'", productLine));
            sql.Append(String.Format(" AND YEAR(RP.DueDate) = '{0}'", year));
            if (month != 0 && !isYearSummary)
                sql.Append(String.Format(" AND MONTH(RP.DueDate) = '{0}'", month));
            else if (month != 0 && isYearSummary)
                sql.Append(String.Format(" AND MONTH(RP.DueDate) <= '{0}'", month));
            var results = Query<dynamic>(uow, sql.ToString());
            return results;
        }

        public IEnumerable<dynamic> GetForecastingInstallments(UnitOfWork uow, List<RiskGraph> possibleRenewalInstallmentPolicies, int startingMonth, int endingMonth)
        {
            StringBuilder sql = new StringBuilder();
            int totalRenewals = possibleRenewalInstallmentPolicies.Count();

            sql.Append(" SELECT	RP.RiskId, RGV.PolicyNumber, RP.AnticipatedAmount ,PL.ProductLine, RGV.Branch, DueDate, MONTH(DueDate) MonthDue, YEAR(DueDate) YearDue, YEAR(EffectiveDate) PolicyEffectiveYear, [Status] ");
            sql.Append(" FROM RiskGraphView RGV ");
            sql.Append(" JOIN [RiskPayments] RP ON RP.RiskId = RGV.RiskId ");
            sql.Append(" JOIN [ProductLines] PL ON PL.ProductLine = RGV.ProductLine ");
            sql.Append(String.Format(" WHERE RGV.[Status] in ('{0}', '{1}') ", RiskStatus.BOUND.Value, RiskStatus.ISSUED.Value));
            sql.Append(String.Format(" AND MONTH(DueDate) between {0} AND {1} ", startingMonth, endingMonth));
            sql.Append(" and PolicyNumber in ( ");
            for (var i = 0; i < totalRenewals; i++)
            {
                sql.Append(String.Format(" '{0}' ", possibleRenewalInstallmentPolicies[i].PolicyNumber));
                if (i != totalRenewals - 1) sql.Append(",");
            }
            sql.Append(" )");
            sql.Append(" ORDER BY DueDate Desc");

            var results = Query<dynamic>(uow, sql.ToString());
            return results;
        }
    }

    public class NamedInsuredAircraftDAO : BaseDAO<NamedInsuredAircraft>
    {
        public NamedInsuredAircraftDAO()
            : base("NamedInsuredAircraft", "UW_Base_App", defaultOrderFilter: new OrderFilter("QuoteAircraftId", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("QuoteId", "QuoteId", true, true);
            AddColumnMapping("QuoteAircraftId", "QuoteAircraftId");
            AddColumnMapping("FAANo", "FAANo");
            AddColumnMapping("ControlNumber", "ControlNumber");
            AddColumnMapping("FirstName", "FirstName");
            AddColumnMapping("LastName", "LastName");
            AddColumnMapping("Year", "Year");
            AddColumnMapping("Make", "Make");
            AddColumnMapping("ModelID", "ModelID");
            AddColumnMapping("AirportID", "AirportID");
            Initialize();
        }
    }

    public class StatusReasonDAO : BaseDAO<StatusReason>
    {
        public StatusReasonDAO()
            : base("QuoteStatusReason", "UW_Base_App", defaultOrderFilter: new OrderFilter("Id", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("Id", "StatusReasonId", true, true);
            AddColumnMapping("Status", "Status");
            AddColumnMapping("Reason", "Reason");
            AddColumnMapping("IsActive", "IsActive");
            AddColumnMapping("ReasonDesc", "ReasonDesc");
            Initialize();
        }
    }

    public class RiskGraphDAO : BaseDAO<RiskGraph>
    {
        public RiskGraphDAO()
            : base("RiskGraphView", "UW_Base_App", defaultOrderFilter: new OrderFilter("RiskId", OrderFilter.Comparator.Descending))
        {
            AddColumnMapping("RiskId", "RiskId");
            AddColumnMapping("Branch", "Branch");
            AddColumnMapping("ImageRightId", "ImageRightId");
            AddColumnMapping("ProductLine", "ProductLine");
            AddColumnMapping("PolicyNumber", "PolicyNumber");
            AddColumnMapping("ControlNumber", "ControlNumber");
            AddColumnMapping("Status", "Status");
            AddColumnMapping("EffectiveDate", "EffectiveDate");
            AddColumnMapping("EffectiveMonth", "EffectiveMonth");
            AddColumnMapping("EffectiveYear", "EffectiveYear");
            AddColumnMapping("ExpirationDate", "ExpirationDate");
            AddColumnMapping("ExpirationMonth", "ExpirationMonth");
            AddColumnMapping("ExpirationYear", "ExpirationYear");
            AddColumnMapping("CreatedDate", "CreatedDate");
            AddColumnMapping("AgencyID", "AgencyID");
            AddColumnMapping("AgencyName", "AgencyName");
            AddColumnMapping("FirstName", "FirstName");
            AddColumnMapping("LastName", "LastName");
            AddColumnMapping("CompanyName", "CompanyName");
            AddColumnMapping("Name", "Name");
            AddColumnMapping("UnderwriterId", "UnderwriterId");
            AddColumnMapping("RenewalUnderwriterId", "RenewalUnderwriterId");
            AddColumnMapping("UnderwriterAssistantId", "UnderwriterAssistantId");
            AddColumnMapping("UW", "UW");
            AddColumnMapping("UA", "UA");
            AddColumnMapping("AnnualizedPremium", "AnnualizedPremium");
            AddColumnMapping("WrittenPremium", "WrittenPremium");
            AddColumnMapping("EarnedPremium", "EarnedPremium");
            AddColumnMapping("ExpiringEarnedPremium", "ExpiringEarnedPremium");
            AddColumnMapping("ExpiredAnnualizedPremium", "ExpiredAnnualizedPremium");
            AddColumnMapping("ExpiringWrittenPremium", "ExpiringWrittenPremium");
            AddColumnMapping("IsRenewal", "IsRenewal");
            AddColumnMapping("IsTargetAccount", "IsTargetAccount");
            AddColumnMapping("IsPaidInInstallments", "IsPaidInInstallments");
            AddColumnMapping("IsReporter", "IsReporter");
            AddColumnMapping("DepositPremium", "DepositPremium");
            AddColumnMapping("ExpiringDepositPremium", "ExpiringDepositPremium");
            AddColumnMapping("Prefix", "Prefix");
            AddColumnMapping("QuoteType", "QuoteType");
            AddColumnMapping("Payroll", "Payroll");
            AddColumnMapping("ProgramType", "ProgramType");
            AddColumnMapping("AccountDescription", "AccountDescription");
            AddColumnMapping("ScheduledRating", "ScheduledRating");
            AddColumnMapping("ExpirienceModifier", "ExpirienceModifier");
            AddColumnMapping("ExpiringPayroll", "ExpiringPayroll");
            AddColumnMapping("AppReceived", "AppReceived");
            AddColumnMapping("Market", "Market");
            AddColumnMapping("InceptionPremium", "InceptionPremium");
            AddColumnMapping("PurposeOfUse", "PurposeOfUse");
            Initialize();
        }

        public IEnumerable<dynamic> GetUWInforcePolicyCount(UnitOfWork uow, string branch = null)
        {
            var sql = new StringBuilder("select COUNT(*) TotalInforce, UW, Branch from RiskGraphView where ExpirationDate > GETDATE() and status in ('Bound', 'Issued')");
            if (branch != null)
                sql.Append(" and Branch = '" + branch + "'");
            sql.Append(" group by UW, Branch");
            var result = Query<dynamic>(uow, sql.ToString());
            return result;
        }

        public IEnumerable<dynamic> GetProductLineInforceAmounts(UnitOfWork uow)
        {
            var sql = new StringBuilder("SELECT FLOOR(SUM(annualizedPremium)) TotalInforce, ProductLine");
            sql.Append(" FROM RiskGraphView ");
            sql.Append(" WHERE ExpirationDate > GETDATE() AND STATUS IN ('Bound', 'Issued')");
            sql.Append(" GROUP BY ProductLine ");
            var result = Query<dynamic>(uow, sql.ToString());
            return result;
        }
    }

    public class RiskLocationDAO : BaseDAO<RiskLoction>
    {
        public RiskLocationDAO()
            : base("QuoteLocations", "UW_Base_App", defaultOrderFilter: new OrderFilter("RiskLocationId", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("RiskLocationId", "QuoteLocationId", true, true);
            AddColumnMapping("RiskId", "QuoteId");
            AddColumnMapping("AirportID", "AirportID");
            AddColumnMapping("CategoryID", "CategoryID");
            AddColumnMapping("Occupancy", "Occupancy");
            AddColumnMapping("PortionOccupied", "PortionOccupied");
            AddColumnMapping("AnyLocation", "AnyLocation");
            AddColumnMapping("LocationMultipler", "LocationMultipler");
            AddColumnMapping("Modifier", "Modifier");
            AddColumnMapping("TotalPremium", "TotalPremium");
            AddColumnMapping("CreatedById", "CreatedById");
            AddColumnMapping("CreatedDate", "CreatedDate");
            AddColumnMapping("UpdatedById", "UpdatedById");
            AddColumnMapping("UpdatedDate", "UpdatedDate");
            AddColumnMapping("EffectiveDate", "EffectiveDate");
            AddColumnMapping("ExpirationDate", "ExpirationDate");
            AddColumnMapping("IsIncluded", "IsIncluded");
            Initialize();
        }
    }

    public class RiskEndorsementDAO : BaseDAO<RiskEndorsement>
    {
        public RiskEndorsementDAO()
            : base("QuoteEndorsement", "UW_Base_App", defaultOrderFilter: new OrderFilter("Id", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("Id", "QuoteEndorsementId", true, true);
            AddColumnMapping("RiskId", "QuoteId");
            AddColumnMapping("Record_ID", "Record_ID");
            AddColumnMapping("IsCarryOverforRenewal", "IsCarryOverforRenewal");
            AddColumnMapping("IsCertificateRequired", "IsCertificateRequired");
            AddColumnMapping("RateType", "RateType");
            AddColumnMapping("Premium", "Premium");
            AddColumnMapping("IsCompleted", "IsCompleted");
            AddColumnMapping("Notes", "Notes");
            AddColumnMapping("CreatedById", "CreatedById");
            AddColumnMapping("CreatedDate", "CreatedDate");
            AddColumnMapping("UpdatedById", "UpdatedById");
            AddColumnMapping("UpdatedDate", "UpdatedDate");
            AddColumnMapping("IsNet", "IsNet");
            AddColumnMapping("EffectiveDate", "EffectiveDate");
            AddColumnMapping("ExpirationDate", "ExpirationDate");
            AddColumnMapping("IsRequired", "IsRequired");
            AddColumnMapping("intQuoteNumber", "intQuoteNumber");
            AddColumnMapping("Code", "Code");
            AddColumnMapping("Description", "Description");
            AddColumnMapping("ProRataAmount", "ProRataAmount");
            AddColumnMapping("AirportCode", "AirportCode");
            AddColumnMapping("COI", "COI");
            AddColumnMapping("DaysNOC", "DaysNOC");
            AddColumnMapping("NonPayment", "NonPayment");
            AddColumnMapping("LevelType", "LevelType");
            AddColumnMapping("AssignedQuoteAircraftID", "AssignedQuoteAircraftID");
            AddColumnMapping("AssignedPilotID", "AssignedPilotID");
            AddColumnMapping("Edition", "Edition");
            AddColumnMapping("IsDoesNotWillAlso", "IsDoesNotWillAlso");
            AddColumnMapping("IsIncludingExcluding", "IsIncludingExcluding");
            AddColumnMapping("IsAdditionalReturn", "IsAdditionalReturn");
            AddColumnMapping("ClassCodeLevel", "ClassCodeLevel");
            AddColumnMapping("QuoteAircraftLossPayeeID", "QuoteAircraftLossPayeeID");
            AddColumnMapping("ActionType", "ActionType");
            AddColumnMapping("strNNumber", "strNNumber");
            AddColumnMapping("ExpiredDate", "ExpiredDate");
            AddColumnMapping("DeleteAssignedQuoteEndorsementId", "DeleteAssignedQuoteEndorsementId");
            AddColumnMapping("MidtermAnnualPremium", "MidtermAnnualPremium");
            AddColumnMapping("IsAddedMidterm", "IsAddedMidterm");
            AddColumnMapping("PreHourAmount", "PreHourAmount");
            AddColumnMapping("ManualFormAndEdition", "ManualFormAndEdition");
            AddColumnMapping("PurposeOfFlight", "PurposeOfFlight");
            AddColumnMapping("IsIssueCOI", "IsIssueCOI");
            AddColumnMapping("IssueCOIQuoteAircraftID", "IssueCOIQuoteAircraftID");
            AddColumnMapping("COIHolderIs", "COIHolderIs");
            AddColumnMapping("IssuedDate", "IssuedDate");
            AddColumnMapping("IsFullyEarned", "IsFullyEarned");
            AddColumnMapping("InvoicedPremium", "InvoicedPremium");
            Initialize();
        }

        public IEnumerable<dynamic> GetProductLineEndorsementAmountForDates(
            UnitOfWork uow, int year, string productLine, string branch = null, int underwriterId = 0)
        {
            var sql = new StringBuilder("SELECT	Premium, QL.Branch, Year(QE.EffectiveDate) EffectiveYear, MONTH(QE.EffectiveDate) EffectiveMonth");
            sql.Append(" FROM	QuoteEndorsement QE");
            sql.Append(" join QuoteLookUp QL on ql.QuoteId = QE.QuoteId");
            sql.Append(" join ProductLines PL on PL.ProductLineId = QL.ProductLine");
            sql.Append(String.Format(" WHERE YEAR(QE.EffectiveDate) = {0} ", year));
            sql.Append(String.Format(" AND PL.ProductLine = '{0}'", productLine));

            if (!String.IsNullOrEmpty(branch))
                sql.Append(String.Format(" AND QL.Branch = '{0}'", branch));
            if (underwriterId != 0)
                sql.Append(String.Format(" AND QL.UnderwriterId = '{0}'", underwriterId));

            var results = Query<dynamic>(uow, sql.ToString());

            return results;
        }

        public IEnumerable<dynamic> GetProductLineEndorsementAmountForDatesByBranch(UnitOfWork uow, DateTime startDate, DateTime endDate, string productLine)
        {
            var sql = new StringBuilder("SELECT Branch, ISNULL(SUM(Premium), 0) Amount FROM QuoteEndorsement, QuoteLookup");
            sql.Append(String.Format(" WHERE QuoteEndorsement.QuoteId = QuoteLookUp.QuoteId and QuoteEndorsement.EffectiveDate BETWEEN '{0}' AND '{1}'", startDate.ToShortDateString(), endDate.ToShortDateString()));
            sql.Append(String.Format(" AND QuoteEndorsement.QuoteId IN (SELECT QuoteId FROM QuoteLookUp JOIN ProductLines PL ON PL.ProductLineId = QuoteLookUp.ProductLine where PL.ProductLine = '{0}')", productLine));
            sql.Append(" GROUP BY Branch ");
            var results = Query<dynamic>(uow, sql.ToString());
            return results;
        }
    }

    public class GridConfigDAO : BaseDAO<WorkingListGridConfig>
    {
        public GridConfigDAO()
            : base("GridConfig", "UW_Base_App", defaultOrderFilter: new OrderFilter("ConfigId", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("ConfigId", "ConfigId", true, true);
            AddColumnMapping("Name", "Name");
            AddColumnMapping("UserId", "UserId");
            AddColumnMapping("Columns", "GridColumns");
            Initialize();
        }
    }
}
