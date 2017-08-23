using PolicyTracker.DomainModel.Brokers;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.Platform.UOW;
using System;
using System.Collections.Generic;
using System.Text;

namespace PolicyTracker.DataAccess.Brokers
{
    public class BrokerDAO : BaseDAO<Broker>
    {
        public BrokerDAO()
            : base("BrokerInfo", "BrokerDB", defaultOrderFilter: new OrderFilter("AgencyID", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("AgencyID", "AgencyID");
            AddColumnMapping("AgencyName", "AgencyName");
            AddColumnMapping("DBAName", "DBAName");
            AddColumnMapping("PhoneNumber", "PhoneNumber");
            AddColumnMapping("FaxNumber", "FaxNumber");
            AddColumnMapping("BranchNumber", "BranchNumber");
            AddColumnMapping("DoNotWrite", "DoNotWrite");
            AddColumnMapping("Active", "Active");
            AddColumnMapping("InactiveReasonCode", "InactiveReasonCode");
            AddColumnMapping("AgencyAgreement", "AgencyAgreement");
            AddColumnMapping("PBAgreement", "PBAgreement");
            AddColumnMapping("AdditionalNames1", "AdditionalNames1");
            AddColumnMapping("AdditionalNames2", "AdditionalNames2");
            AddColumnMapping("AddtionalNames3", "AddtionalNames3");
            AddColumnMapping("comments", "comments");
            AddColumnMapping("EmailAddress1", "EmailAddress1");
            AddColumnMapping("IntialEntryDate", "IntialEntryDate");
            AddColumnMapping("LastUpdate", "LastUpdate");
            AddColumnMapping("PlatinumProducer", "PlatinumProducer");
            AddColumnMapping("PlatinumPlusProducer", "PlatinumPlusProducer");
            AddColumnMapping("TerritoryID", "TerritoryID");
            AddColumnMapping("EO", "EO");
            AddColumnMapping("EOExpireDate", "EOExpireDate");
            AddColumnMapping("PAMBranch", "PAMBranch");
            AddColumnMapping("CreatedBy", "CreatedBy");
            AddColumnMapping("UpdatedBy", "UpdatedBy");
            AddColumnMapping("IsNewBusinessRenewalQuoteAllowed", "IsNewBusinessRenewalQuoteAllowed");
            Initialize();
        }

        public IEnumerable<dynamic> GetTopTenBrokersByPremium(UnitOfWork uow, string underwriterId = null, string branch = null)
        {
            var sql = new StringBuilder("SELECT TOP 10");
            if (!String.IsNullOrEmpty(underwriterId)) sql.Append(" UnderwriterId,");
            sql.Append(" BI.AgencyID, BI.AgencyName, SUM (RPI.AnnualizedPremium) TotalPremium");
            sql.Append(" FROM QuoteLookUp QL JOIN RiskPremiumInfo RPI ON RPI.RiskId = QL.QuoteId");
            sql.Append(" JOIN BROKERDB.DBO.BrokerInfo BI ON BI.AgencyID = QL.AgencyID");

            sql.Append(" WHERE  QL.[Status] in ('Issued', 'Bound')");
            sql.Append(" and ExpirationDate > GETDATE()");
            if (!string.IsNullOrEmpty(underwriterId))
                sql.Append($" and UnderwriterId = {underwriterId}");
            if (!string.IsNullOrEmpty(branch))
                sql.Append($" and Branch = '{branch}'");

            sql.Append(" GROUP BY");
            if (!String.IsNullOrEmpty(underwriterId)) sql.Append(" UnderwriterId,");
            sql.Append(" BI.AgencyID, BI.AgencyName");
            sql.Append(" ORDER BY TotalPremium DESC");

            var results = Query<dynamic>(uow, sql.ToString());

            return results;
        }

        public IEnumerable<dynamic> GetTopTenBrokersByCount(UnitOfWork uow, string underwriterId = null, string branch = null)
        {
            var sql = new StringBuilder("SELECT TOP 10 ");
            if (!String.IsNullOrEmpty(underwriterId)) sql.Append(" UnderwriterId, ");
            sql.Append(" QL.AgencyID, BI.AgencyName, COUNT(QL.QuoteId) TotalRisks");
            sql.Append(" FROM QuoteLookUp QL JOIN BrokerDB.DBO.BrokerInfo BI ON BI.AgencyID = QL.AgencyID");

            sql.Append(" WHERE  QL.[Status] in ('Issued', 'Bound')");
            sql.Append(" and ExpirationDate > GETDATE()");
            if (!string.IsNullOrEmpty(underwriterId))
                sql.Append($" and UnderwriterId = {underwriterId}");
            if (!string.IsNullOrEmpty(branch))
                sql.Append($" and Branch = '{branch}'");

            sql.Append(" GROUP BY ");
            if (!String.IsNullOrEmpty(underwriterId)) sql.Append(" UnderwriterId,");
            sql.Append(" QL.AgencyID, BI.AgencyName");
            sql.Append(" ORDER BY TotalRisks DESC");

            var results = Query<dynamic>(uow, sql.ToString());

            return results;
        }
    }

    public class BrokerMailingAddressDAO : BaseDAO<BrokerMailingAddress>
    {
        public BrokerMailingAddressDAO()
            : base("BrokerMailingAddress", "BrokerDB", defaultOrderFilter: new OrderFilter("AgencyID", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("AgencyID", "AgencyID");
            AddColumnMapping("Street1", "Street1");
            AddColumnMapping("Street2", "Street2");
            AddColumnMapping("City", "City");
            AddColumnMapping("State", "State");
            AddColumnMapping("Zip", "Zip");
            AddColumnMapping("Zip4", "Zip4");
            Initialize();
        }
    }

    public class BrokerPhysicalAddressDAO : BaseDAO<BrokerPhysicalAddress>
    {
        public BrokerPhysicalAddressDAO()
            : base("BrokerPhysicalAddress", "BrokerDB", defaultOrderFilter: new OrderFilter("AgencyID", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("AgencyID", "AgencyID");
            AddColumnMapping("Street1", "Street1");
            AddColumnMapping("Street2", "Street2");
            AddColumnMapping("City", "City");
            AddColumnMapping("State", "State");
            AddColumnMapping("Zip", "Zip");
            Initialize();
        }
    }

    public class BrokerComissionDAO : BaseDAO<Commission>
    {
        public BrokerComissionDAO()
            : base("BrokerCommission", "BrokerDB", defaultOrderFilter: new OrderFilter("AgencyID", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("AgencyID", "AgencyID");
            AddColumnMapping("NewAG", "NewAG");
            AddColumnMapping("NewAirportCom", "NewAirportCom");
            AddColumnMapping("NewAirportNonCo", "NewAirportNonCo");
            AddColumnMapping("NewComm", "NewComm");
            AddColumnMapping("NewCorp", "NewCorp");
            AddColumnMapping("NewHL", "NewHL");
            AddColumnMapping("NewWC", "NewWC");
            AddColumnMapping("NewPB", "NewPB");
            AddColumnMapping("NewManProd", "NewManProd");
            AddColumnMapping("ReNewAG", "ReNewAG");
            AddColumnMapping("RenewAirportCom", "RenewAirportCom");
            AddColumnMapping("RenewAirportNonCo", "RenewAirportNonCo");
            AddColumnMapping("RenewComm", "RenewComm");
            AddColumnMapping("RenewCorp", "RenewCorp");
            AddColumnMapping("RenewHL", "RenewHL");
            AddColumnMapping("RenewWC", "RenewWC");
            AddColumnMapping("RenewPB", "RenewPB");
            AddColumnMapping("RenewManProd", "RenewManProd");
            AddColumnMapping("Comments", "Comments");
            AddColumnMapping("LastUpdated", "LastUpdated");
            Initialize();
        }
    }

    public class AgentDAO : BaseDAO<Agent>
    {
        public AgentDAO()
            : base("IndividualInfo", "BrokerDB", defaultOrderFilter: new OrderFilter("IndID", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("IndID", "IndID");
            AddColumnMapping("AgencyID", "AgencyID");
            AddColumnMapping("FirstName", "FirstName");
            AddColumnMapping("MiddleInitial", "MiddleInitial");
            AddColumnMapping("LastName", "LastName");
            AddColumnMapping("Suffix", "Suffix");
            AddColumnMapping("IsActive", "IsActive");
            AddColumnMapping("ActiveInactiveCode", "ActiveInactiveCode");
            AddColumnMapping("Principle", "Principle");
            AddColumnMapping("Title", "Title");
            AddColumnMapping("RoleID", "RoleID");
            AddColumnMapping("UserEmail", "UserEmail");
            AddColumnMapping("Phone", "Phone");
            AddColumnMapping("NPNCode", "NPNCode");
            AddColumnMapping("SSN", "SSN");
            AddColumnMapping("DOB", "DOB");
            AddColumnMapping("comments", "Comments");
            AddColumnMapping("UpdateDate", "UpdateDate");
            AddColumnMapping("EntryDate", "EntryDate");
            AddColumnMapping("TerminationDate", "TerminationDate");
            AddColumnMapping("BackgroundCheckDate", "BackgroundCheckDate");
            AddColumnMapping("UpdatedBy", "UpdatedBy");
            AddColumnMapping("EnteredBy", "EnteredBy");
            Initialize();
        }
    }

    public class StateDAO : BaseDAO<State>
    {
        public StateDAO()
            : base("State", "BrokerDB", defaultOrderFilter: new OrderFilter("StateCode", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("StateCode", "StateCode");
            AddColumnMapping("StateName", "StateName");
            Initialize();
        }
    }

    public class BrokerLicenseDAO : BaseDAO<BrokerLicense>
    {
        public BrokerLicenseDAO()
            : base("BrokerLic", "BrokerDB", defaultOrderFilter: new OrderFilter("Id", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("BrokerLicID", "BrokerLicID", true, true);
            AddColumnMapping("AgencyID", "AgencyID");
            AddColumnMapping("licenseNumber", "licenseNumber");
            AddColumnMapping("LastUpdated", "LastUpdated");
            AddColumnMapping("LicenseType", "LicenseType");
            AddColumnMapping("LicenseState", "LicenseState");
            AddColumnMapping("Appointed", "Appointed");
            AddColumnMapping("ExpirationDate", "ExpirationDate");
            AddColumnMapping("PerpetualLicense", "PerpetualLicense");
            Initialize();
        }
    }

    public class AgentLicenseDAO : BaseDAO<AgentLicense>
    {
        public AgentLicenseDAO()
            : base("IndLic", "BrokerDB", defaultOrderFilter: new OrderFilter("Id", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("IndLicID", "IndLicID", true, true);
            AddColumnMapping("AgentId", "IndID");
            AddColumnMapping("AgencyID", "AgencyID");
            AddColumnMapping("licenseNumber", "licenseNumber");
            AddColumnMapping("LastUpdated", "LastUpdated");
            AddColumnMapping("LicenseType", "LicenseType");
            AddColumnMapping("LicenseState", "LicenseState");
            AddColumnMapping("Appointed", "Appointed");
            AddColumnMapping("ExpirationDate", "ExpirationDate");
            AddColumnMapping("PerpetualLicense", "PerpetualLicense");
            Initialize();
        }
    }

    public class BrokerAssignmentDAO : BaseDAO<BrokerAssignment>
    {
        public BrokerAssignmentDAO()
            : base("BrokerAssignment", "UW_Base_App", defaultOrderFilter: new OrderFilter("AssignmentId", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("AssignmentId", "AssignmentId", true, true);
            AddColumnMapping("BrokerCode", "BrokerCode");
            AddColumnMapping("UserId", "UserId");
            AddColumnMapping("UserName", "UserName");
            AddColumnMapping("ProductLineId", "ProductLineId");
            AddColumnMapping("LastAssigned", "LastAssigned");
            AddColumnMapping("Tempo", "Tempo");
            AddColumnMapping("Frequency", "Frequency");
            Initialize();
        }
    }
}