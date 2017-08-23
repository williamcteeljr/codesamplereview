using PolicyTracker.DomainModel.Framework;
using System;
using System.ComponentModel.DataAnnotations;

namespace PolicyTracker.DomainModel.Brokers
{
    public class Broker : BaseEntity
    {
        [Required]
        public string AgencyID { get; set; }
        public string AgencyName { get; set; }
        public string DBAName { get; set; }
        public string PhoneNumber { get; set; }
        public string FaxNumber { get; set; }
        public string BranchNumber { get; set; }
        public bool DoNotWrite { get; set; }
        public bool Active { get; set; }
        public string InactiveReasonCode { get; set; }
        public bool AgencyAgreement { get; set; }
        public bool PBAgreement { get; set; }
        public string AdditionalNames1 { get; set; }
        public string AdditionalNames2 { get; set; }
        public string AddtionalNames3 { get; set; }
        public string comments { get; set; }
        public string EmailAddress1 { get; set; }
        public DateTime IntialEntryDate { get; set; }
        public DateTime LastUpdate { get; set; }
        public bool PlatinumProducer { get; set; }
        public bool PlatinumPlusProducer { get; set; }
        public string TerritoryID { get; set; }
        public bool EO { get; set; }
        public DateTime EOExpireDate { get; set; }
        public string PAMBranch { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public Commission Commission { get; set; }
        private bool IsNewBusinessRenewalQuoteAllowed { get; set; }
        public bool IsMidTermOnly {
            get { return IsNewBusinessRenewalQuoteAllowed ? false : true; }
        }

        //Facade Properties
        public string Id
        {
            get { return AgencyID; }
        }
        public Agent Agent { get; set; }
        public string State { get; set; }
    }

    public class BrokerMailingAddress : BaseEntity
    {
        public string AgencyID { get; set; }
        public string Street1 { get; set; }
        public string Street2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Zip4 { get; set; }
    }

    public class BrokerPhysicalAddress : BaseEntity
    {
        public string AgencyID { get; set; }
        public string Street1 { get; set; }
        public string Street2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
    }

    public class Agent : BaseEntity
    {
        public int IndID { get; set; }
        public string AgencyID { get; set; }
        public string FirstName { get; set; }
        public string MiddleInitial { get; set; }
        public string LastName { get; set; }
        public string Suffix { get; set; }
        public bool IsActive { get; set; }
        public string ActiveInactiveCode { get; set; }
        public bool Principle { get; set; }
        public string Title { get; set; }
        public string RoleID { get; set; }
        public string UserEmail { get; set; }
        public string Phone { get; set; }
        public string NPNCode { get; set; }
        public string SSN { get; set; }
        public DateTime DOB { get; set; }
        public string Comments { get; set; }
        public DateTime UpdateDate { get; set; }
        public DateTime EntryDate { get; set; }
        public DateTime TerminationDate { get; set; }
        public DateTime BackgroundCheckDate { get; set; }
        public string UpdatedBy { get; set; }
        public string EnteredBy { get; set; }

        public string Name
        {
            get { return FirstName + ' ' + LastName; }
        }
    }

    public class State : BaseEntity
    {
        public string StateCode { get; set; }
        public string StateName { get; set; }
    }

    public class Commission : BaseEntity
    {
        public string AgencyID { get; set; }
        public decimal NewAG { get; set; }
        public decimal NewAirportCom { get; set; }
        public decimal NewAirportNonCo { get; set; }
        public decimal NewComm { get; set; }
        public decimal NewCorp { get; set; }
        public decimal NewHL { get; set; }
        public decimal NewWC { get; set; }
        public decimal NewPB { get; set; }
        public decimal NewManProd { get; set; }
        public decimal ReNewAG { get; set; }
        public decimal RenewAirportCom { get; set; }
        public decimal RenewAirportNonCo { get; set; }
        public decimal RenewComm { get; set; }
        public decimal RenewCorp { get; set; }
        public decimal RenewHL { get; set; }
        public decimal RenewWC { get; set; }
        public decimal RenewPB { get; set; }
        public decimal RenewManProd { get; set; }
        public string Comments { get; set; }
        public DateTime? LastUpdated { get; set; }
    }

    public class BrokerLicense : BaseEntity
    {
        public int BrokerLicID { get; set; }
        public string AgencyID { get; set; }
        public string licenseNumber { get; set; }
        public DateTime LastUpdated { get; set; }
        public string LicenseType { get; set; }
        public string LicenseState { get; set; }
        public string Appointed { get; set; }
        public DateTime ExpirationDate { get; set; }
        public bool PerpetualLicense { get; set; }
    }

    public class AgentLicense : BaseEntity
    {
        public int IndLicID { get; set; }
        public int AgentId { get; set; }
        public string AgencyID { get; set; }
        public string licenseNumber { get; set; }
        public DateTime LastUpdated { get; set; }
        public string LicenseType { get; set; }
        public string LicenseState { get; set; }
        public string Appointed { get; set; }
        public DateTime ExpirationDate { get; set; }
        public bool PerpetualLicense { get; set; }
    }

    public class BrokerAssignment : BaseEntity
    {
        public int AssignmentId { get; set; }
        [Required]
        public string BrokerCode { get; set; }
        [Required]
        public int UserId { get; set; }
        public string UserName { get; set; }
        [Required]
        public int ProductLineId { get; set; }
        public bool LastAssigned { get; set; }
        public string ProductLine { get; set; }
        public int Tempo { get; set; }
        public int Frequency { get; set; }
    }
}
