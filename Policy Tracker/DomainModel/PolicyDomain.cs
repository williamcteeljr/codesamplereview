using PolicyTracker.DomainModel.Brokers;
using PolicyTracker.DomainModel.Common;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.DomainModel.PostNotice;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web.Mvc;

namespace PolicyTracker.DomainModel.Policy
{
    public enum SubmissionType { NewBusiness, Renewal, Endorsement }
    public enum ProductLines { NONE, AG, AL, COM, COR, MP, PB, AGE, WC, PROP }
    public enum AircraftPolicyTypes { CA, AG, AA, AVC, RAL }

    //public class ProductLine : StringEnum
    //{
    //    public static ProductLine AG = new ProductLine() { Value = "1", DisplayText = "Agriculture" };
    //    public static ProductLine AL = new ProductLine() { Value = "2", DisplayText = "Airlines" };
    //    public static ProductLine COM = new ProductLine() { Value = "3", DisplayText = "Commercial" };
    //    public static ProductLine COR = new ProductLine() { Value = "4", DisplayText = "Corporate" };
    //    public static ProductLine MP = new ProductLine() { Value = "5", DisplayText = "Manufactured Products" };
    //    public static ProductLine PB = new ProductLine() { Value = "6", DisplayText = "Pleasure & Business" };
    //    public static ProductLine AGE = new ProductLine() { Value = "7", DisplayText = "Airports & Govt Entity" };
    //    public static ProductLine WC = new ProductLine() { Value = "8", DisplayText = "Workers Comp" };
    //    public static ProductLine PROP = new ProductLine() { Value = "9", DisplayText = "Property" };


    //    private ProductLine(string val, string text) : base(val, text) { }
    //    private ProductLine() { }
    //}

    public class RiskStatus : StringEnum
    {
        public static RiskStatus SUBMISSION = new RiskStatus() { Value = "Submission", DisplayText = "Submission" };
        public static RiskStatus DECLINED = new RiskStatus() { Value = "Declined", DisplayText = "Declined" };
        public static RiskStatus QUOTE = new RiskStatus() { Value = "Quote", DisplayText = "Quote" };
        public static RiskStatus BOUND = new RiskStatus() { Value = "Bound", DisplayText = "Bound" };
        public static RiskStatus CANCELED = new RiskStatus() { Value = "Canceled", DisplayText = "Canceled" };
        public static RiskStatus ISSUED = new RiskStatus() { Value = "Issued", DisplayText = "Issued" };
        public static RiskStatus LOST = new RiskStatus() { Value = "Lost", DisplayText = "Lost" };
        public static RiskStatus INVOLVED = new RiskStatus() { Value = "Already Involved", DisplayText = "Already Involved" };

        private RiskStatus(string val, string text) : base(val, text) { }
        private RiskStatus() { }
    }

    public class ProductLine : BaseEntity
    {
        public int ProductLineId { get; set; }
        public string Name { get; set; }
    }

    public class Product : BaseEntity
    {
        public int ProductId { get; set; }
        public string Prefix { get; set; }
        public string Description { get; set; }
        public int ProductLineId { get; set; }
        public string QuoteType { get; set; }
        public bool IsActive { get; set; }

        public string DisplayText
        {
            get { return Prefix + " - " + Description; }
        }
    }

    public class Risk : AuditedEntity
    {
        public int Id { get; set; }
        [Required(ErrorMessage="Product Line is Required")]
        public int ProductLine { get; set; }
        public string QuoteType { get; set; }
        public int ControlNumber { get; set; }
        public string Prefix { get; set; }
        public string PolicyNumber { get; set; }
        //Policy Suffix required constraints added - WTeel
        [Required]
        [MaxLength(2, ErrorMessage = "Min and Max Length of 2 numbers allowed")]
        [RegularExpression(@"^[00-9]{2,2}$",
         ErrorMessage = "Min and Max Length of 2 numbers allowed")]
        [Range(00, 99,
        ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public string PolicySuffix { get; set; }
        public string CompletedWizardId { get; set; }
        [Required]
        public string Status { get; set; }
        public int CreatedById { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int UpdatedById { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool isMidTerm { get; set; }
        public int UnderwriterId { get; set; }
        public string StatusReason { get; set; }
        public string StatusMarket { get; set; }
        public int ParentID { get; set; }
        public int QuoteSubmissionTypeID { get; set; }
        [Required(ErrorMessage="ImageRight Id is Required")]
        public string ImageRightId { get; set; }
        [Required]
        public int UnderwriterAssistantId { get; set; }
        public int RenewalUnderwriterId { get; set; }
        [Required(ErrorMessage="Branch is Required")]
        public string Branch { get; set; }
        public bool IsTargetAccount { get; set; }

        public DateTime? ApplicationDate { get; set; }
        public int ClassofBusinessGroupCodesId { get; set; }
        public bool PaidInInstallments { get; set; }
        [Required(ErrorMessage="Risk Effective Date is Required")]
        public DateTime? EffectiveDate { get; set; }
        [Required(ErrorMessage="Risk Expiration Date is Required")]
        public DateTime? ExpirationDate { get; set; }
        public float APLossRatio { get; set; }
        public bool TRIALiability { get; set; }
        public bool WARLiability { get; set; }
        [Required]
        public float Commission { get; set; }
        public string BrokerComments { get; set; }
        public string BrokerStatus { get; set; }
        public bool IsCommissionVisible { get; set; }
        public float TriaPercentage { get; set; }
        public float WarPercentage { get; set; }
        [Required]
        public string AgencyID { get; set; }
        public string AgentName { get; set; }
        public string AgentEmail { get; set; }
        public DateTime? QuotedDate { get; set; }
        [Required(ErrorMessage = "Agent is Required. Please select an Agent.")]
        public int AgentId { get; set; }
        public string PurposeOfUse { get; set; }
        public string AirportId { get; set; }
        public string LeadInsurer { get; set; }
        public decimal ORAPercent { get; set; }
        public int Market { get; set; }
        public bool AppReceived { get; set; }
        public bool IsMexicanPolicy { get; set; }
        public bool IsNet { get; set; }

        //AVC Specific fields that were copied from QuoteAVCPolicyLevel
        public string AirportCategory { get; set; }
        public string AirportName { get; set; }
        public int AS400PolicyUseCodeId { get; set; }
        public float AVLossRatio { get; set; }
        public string DecPageCSL { get; set; }
        public string DecPageLiabLimit { get; set; }
        public string DecPageSubLiabLimit { get; set; }
        public string DecPageSubLimitType { get; set; }
        public int DecUse { get; set; }
        public bool Reporter { get; set; }
        public int RenewalQuoteId { get; set; }
        public bool IsDoNotRenew { get; set; }
        public DateTime? CancelDate { get; set; }
        public bool HasFailedOFAC { get; set; }

        // -- Risk Named Insured Information -- \\
        public string InsuredType { get; set; }
        public string FirstName { get; set; }
        public string MiddleInitial { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
        [RegularExpression(@"^\(?([0-9]{2})\)?[-. ]?([0-9]{7})$", ErrorMessage = "The FEIN number must start with 2 digits, a dash, followed by a max of 7 digits.")]
        public String Fein { get; set; }
        public string DoingBusinessAs { get; set; }
        public string StreetAddress1 { get; set; }
        public string StreetAddress2 { get; set; }
        [Required(ErrorMessage = "Named Insured Address City is Required")]
        public string City { get; set; }
        [Required(ErrorMessage = "Named Insured Address State is Required")]
        public string State { get; set; }
        public string Zip { get; set; }
        public int RenewalOf { get; set; }
        public string FullPolicyNumber { get
            { return Prefix + PolicyNumber + PolicySuffix; }
        }
        public int Rating { get; set; }

        //Related Entities
        public RiskPremiumInfo PremiumInfo { get; set; }
        public RiskInstallmentInfo InstallmentInfo { get; set; }
        public RiskWorkersCompInfo WorkersCompInfo { get; set; }
        public NamedInsured NamedInsured { get; set; }
        public Broker Broker { get; set; }
        public List<Aircraft> Aircrafts { get; set; }
        public List<RiskNote> Notes { get; set; }
        public RiskNote FirstNote { get; set; }
        public IEnumerable<string> FAANumbers { get; set; }
        public DateTime ProcessDate { get; set; } // Same as CreatedDate?
        public List<RiskPayment> Payments { get; set; }

        //Non Data Stored Fields
        public string Name
        {
            get { return (!String.IsNullOrEmpty(FirstName)) ? FirstName + " " + LastName : CompanyName; }
        }
        public bool IsCompany
        {
            get { return (!String.IsNullOrEmpty(CompanyName)); }
        }
        public bool IsRenewal
        {
            get
            {
                var result = false;

                if (ProductLine == (int)ProductLines.WC)
                {
                    if (!String.IsNullOrEmpty(Convert.ToString(PolicySuffix)) && Convert.ToInt32(PolicySuffix) > 0)
                        result = true;
                }
                else if(!String.IsNullOrEmpty(Convert.ToString(PolicySuffix)) && Convert.ToInt32(PolicySuffix) > 1)
                    result = true;

                return result;
            }
        }

        public bool UpdateClientInfo { get; set; }

        //property that contains the Main Insured Locations we will display in listbox
        public int[] MainLocationsIds { get; set; }
        public int[] MainLocationsSelectedItemsIds { get; set; }
        public IEnumerable<SelectListItem> MainLocationsOptionList { get; set; }
        public IEnumerable<SelectListItem> MainLocationsSelectedOptionList { get; set; }
        public List<WCPnLocationDomain> MainLocationsLocations { get; set; }

        public Risk()
        {
            PolicySuffix = "00";
            Status = RiskStatus.SUBMISSION.Value;
            NamedInsured = new NamedInsured();
            AgentId = 0;
        }
    }

    public class AVCRiskPolicyInfo : BaseEntity
    {
        public int QuotePolicyId { get; set; }
        public int QuoteId { get; set; }
        public int ClassofBusinessGroupCodesId { get; set; }
        public int AS400PolicyUseCodeId { get; set; }
        public bool PaidInInstallments { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public float AVLossRatio { get; set; }
        public bool Reporter { get; set; }
        public float Commission { get; set; }
        public string AirportCode { get; set; }
        public string AirportName { get; set; }
        public string AirportCategory { get; set; }
        public int CreatedById { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int UpdatedById { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string BrokerComments { get; set; }
        public string BrokerStatus { get; set; }
        public string DecPageLiabLimit { get; set; }
        public string DecPageCSL { get; set; }
        public string DecPageSubLiabLimit { get; set; }
        public string DecPageSubLimitType { get; set; }
        public string AgencyID { get; set; }
        public string AgentName { get; set; }
        public string AgentEmail { get; set; }
        public int DecUse { get; set; }
    }

    public class RiskWorkersCompInfo : BaseEntity
    {
        public int Id { get; set; }
        public int RiskId { get; set; }
        public string ProgramType { get; set; }
        public string AccountDescription { get; set; }
        public decimal ScheduledRating { get; set; }
        public decimal ExperienceModifier { get; set; }
        public decimal Payroll { get; set; }
        public decimal ExpiringPayroll { get; set; }
        public decimal PayrollChange { get; set; }
        public decimal NetRate { get; set; }
        public decimal ExpiringRate { get; set; }
        public decimal NetRateChange { get; set; }
        public bool IsPassengerCarrying { get; set; }
    }

    public class RiskAudit : BaseEntity
    {
        public int AuditId { get; set; }
        public int RiskId { get; set; }
        [Required]
        public DateTime? AuditDate { get; set; }
        [Required]
        public long Amount { get; set; }
    }

    public class RiskPremiumInfo : BaseEntity
    {
        public int Id { get; set; }
        public int RiskId { get; set; }
        public decimal DepositPremium { get; set; }
        public decimal ExpiredAnnualizedPremium { get; set; }
        public decimal ExpiringEarnedPremium { get; set; }
        public decimal PremiumRenewalChange { get; set; }
        public string PremiumChangeReasonCode { get; set; }
        public decimal AnnualizedPremium { get; set; }
        public decimal InceptionPremium { get; set; }
        public bool IsMinDepositPercentage { get; set; }
        public float DepositPercentage { get; set; }
        public decimal EarnedPremium { get; set; }
        public decimal WrittenPremium { get; set; }
        public decimal ExpiringWrittenPremium { get; set; }
    }

    public class RiskInstallmentInfo : BaseEntity
    {
        public int Id { get; set; }
        public int RiskId { get; set; }
        public bool IsPaidInInstallments { get; set; }
        public bool IsReporter { get; set; }
        public decimal EstimatedPremPerMonth { get; set; }
        public decimal ActualPremPerMonth { get; set; }
        public int PaymentTerms { get; set; }
    }

    public class RiskNote : BaseEntity
    {
        public int Id { get; set; }
        public int RiskId { get; set; }
        public string Comment { get; set; }
        public int CreatedById { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int UpdatedById { get; set; }
        public DateTime? ImpactDate { get; set; }
        public bool ImpactsPremium { get; set; }
    }

    public class PurposeOfUse : BaseEntity
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
    }

    public class NamedInsured : BaseEntity
    {
        [Required]
        public int ControlNumber { get; set; }
        public string AgencyID { get; set; }
        public DateTime? Date { get; set; }
        public string InsuredType { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
        [RegularExpression(@"^\(?([0-9]{2})\)?[-. ]?([0-9]{7})$", ErrorMessage = "The FEIN number must start with 2 digits, a dash, followed by a max of 7 digits.")]
        public String Fein { get; set; }
        public string DoingBusinessAs { get; set; }
        public string CompanyWebsite { get; set; }
        public string StreetAddress1 { get; set; }
        public string StreetAddress2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public bool isRenewal { get; set; }
        public string ExPolicyNumberAVC { get; set; }
        public string ExPolicyNumberAP { get; set; }
        public int Underwriter { get; set; }
        public string IssuingCompany { get; set; }
        public bool WorkersCompSB { get; set; }
        public bool CorporateSB { get; set; }
        public bool PBSupportBusiness { get; set; }
        public bool AirportMunicipalitiesSB { get; set; }
        public string WorkersCompSBNotes { get; set; }
        public string CorporateSBNotes { get; set; }
        public string PBSupportBusinessNotes { get; set; }
        public string AirportMunicipalitiesSBNotes { get; set; }
        public string BusinessDescription { get; set; }
        public string YearsInBusiness { get; set; }
        public bool NewBusiness { get; set; }
        public float ExAPAnnualPremium { get; set; }
        public float ExAVCAnnualPremium { get; set; }
        public string ProducerComments { get; set; }
        public int ClassofBusinessGroupCodesId { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedById { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int UpdatedById { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public float StateTaxRate { get; set; }
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
        [PhoneNumber]
        public string PhoneNumber { get; set; }
        public bool IsFBOClient { get; set; }
        public string MiddleInitial { get; set; }

        ////multiselect list box item collection objects
        //public int[] Id { get; set; }
        //public int[] SelectedItemsIds { get; set; }
        //public IEnumerable<SelectListItem> OptionList { get; set; }
        //public IEnumerable<SelectListItem> SelectedOptionList { get; set; }
        //public List<WCPnLocationDomain> Locations { get; set; }

        //Related Entities
        public string Name
        {
            get { return (!String.IsNullOrEmpty(FirstName)) ? FirstName + " " + LastName : CompanyName; }
        }
        public bool IsCompany
        {
            get { return (!String.IsNullOrEmpty(CompanyName)); }
        }
    }

    public class WorkingListItem : BaseEntity
    {
        public int RiskId { get; set; }
        public string Branch { get; set; }
        public string ImageRightId { get; set; }
        public string ProductLine { get; set; }
        public string PolicyNumber { get; set; }
        public int ControlNumber { get; set; }
        public string Status { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string AgencyID { get; set; }
        public string AgencyName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
        public string Name { get; set; }
        public int UnderwriterId { get; set; }
        public int UnderwriterAssistantId { get; set; }
        public string UW { get; set; }
        public string UA { get; set; }
        public string RUW { get; set; }
        public decimal AnnualizedPremium { get; set; }
        public bool IsRenewal { get; set; }
        public string QuoteType { get; set; }
        public bool HasNotes { get; set; }
        public bool AppReceived { get; set; }
        public string ProgramType { get; set; }
        public string AgentName { get; set; }
    }

    public class WorkingListGridColumn : StringEnum
    {
        public static WorkingListGridColumn RiskId = new WorkingListGridColumn() { Value = "RiskId", DisplayText = "Risk Id" };
        public static WorkingListGridColumn Branch = new WorkingListGridColumn() { Value = "Branch", DisplayText = "Branch" };
        public static WorkingListGridColumn ImageRightId = new WorkingListGridColumn() { Value = "ImageRightId", DisplayText = "ImageRight Id" };
        public static WorkingListGridColumn ProductLine = new WorkingListGridColumn() { Value = "ProductLine", DisplayText = "Product Line" };
        public static WorkingListGridColumn PolicyNumber = new WorkingListGridColumn() { Value = "PolicyNumber", DisplayText = "Policy Number" };
        public static WorkingListGridColumn ControlNumber = new WorkingListGridColumn() { Value = "ControlNumber", DisplayText = "ControlNumber" };
        public static WorkingListGridColumn Status = new WorkingListGridColumn() { Value = "Status", DisplayText = "Status" };
        public static WorkingListGridColumn EffectiveDate = new WorkingListGridColumn() { Value = "EffectiveDate", DisplayText = "Effective Date" };
        public static WorkingListGridColumn ExpirationDate = new WorkingListGridColumn() { Value = "ExpirationDate", DisplayText = "Expiration Date" };
        public static WorkingListGridColumn CreatedDate = new WorkingListGridColumn() { Value = "CreatedDate", DisplayText = "Created Date" };
        public static WorkingListGridColumn AgencyID = new WorkingListGridColumn() { Value = "AgencyID", DisplayText = "Agency Id" };
        public static WorkingListGridColumn AgencyName = new WorkingListGridColumn() { Value = "AgencyName", DisplayText = "Agency Name" };
        public static WorkingListGridColumn Name = new WorkingListGridColumn() { Value = "Name", DisplayText = "Named Insured" };
        public static WorkingListGridColumn UW = new WorkingListGridColumn() { Value = "UW", DisplayText = "Underwriter" };
        public static WorkingListGridColumn UA = new WorkingListGridColumn() { Value = "UA", DisplayText = "Underwriter Assistant" };
        public static WorkingListGridColumn AnnualizedPremium = new WorkingListGridColumn() { Value = "AnnualizedPremium", DisplayText = "Annualized Premium" };
        public static WorkingListGridColumn IsRenewal = new WorkingListGridColumn() { Value = "IsRenewal", DisplayText = "IsRenewal" };
        public static WorkingListGridColumn HasNotes = new WorkingListGridColumn() { Value = "HasNotes", DisplayText = "HasNotes" };
        public static WorkingListGridColumn AppReceived = new WorkingListGridColumn() { Value = "AppReceived", DisplayText = "Application Received" };
        public static WorkingListGridColumn ProgramType = new WorkingListGridColumn() { Value = "ProgramType", DisplayText = "Program Type" };
        public static WorkingListGridColumn AgentName = new WorkingListGridColumn() { Value = "AgentName", DisplayText = "Agent Name" };

        private WorkingListGridColumn(string val, string text) : base(val, text) { }
        private WorkingListGridColumn() { }
    }

    public class WorkingListGridConfig : BaseEntity
    {
        public int ConfigId { get; set; }
        public string Name { get; set; }
        public int UserId { get; set; }
        public string Columns { get; set; }
        public IEnumerable<string> GridColumns {
            get { return (Columns != null) ? Columns.Split(',') : Enumerable.Empty<string>(); }
            set { Columns = String.Join(",", value); }
        }
    }

    public class Aircraft : BaseEntity
    {
        public int Id { get; set; }
        [Required]
        public int QuoteId { get; set; }
        [Required]
        public string FAANo { get; set; }
        public string Year { get; set; }
        public string Make { get; set; }
        public long ModelID { get; set; }
        public string MaxSeats { get; set; }
        public int CrewSeats { get; set; }
        public int PassSeats { get; set; }
        public float Value { get; set; }
        [Required]
        public string AirportID { get; set; }
        public int ClassofBusinessGroupCodesId { get; set; }
        public string EngineType { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public float HullPrem { get; set; }
        public float LiabPrem { get; set; }
        public float AnnualPrem { get; set; }
        public float WrittenPrem { get; set; }
        public float HullFactor { get; set; }
        public float LiabFactor { get; set; }
        public int CreatedById { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int UpdatedById { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsIncluded { get; set; }
        [Required]
        public string PurposeOfUse { get; set; }
        public string ModelName { get; set; }

        public AircraftLiability Liability { get; set; }
    }

    public class AircraftLiability : BaseEntity
    {
        public int LiabilityId { get; set; }
        public int AircraftId { get; set; }
        public float Limit { get; set; }
        public bool isCSL { get; set; }
        public float Sublimit { get; set; }
        public string PerPersonPerPass { get; set; }
        public bool PassExcl { get; set; }
        public float ExpiringLiabRate { get; set; }
        public float LiabBookRate { get; set; }
        public float Modifier { get; set; }
        public float LiabPrem { get; set; }
        public float ExpiringLiabPrem { get; set; }
        public bool isWarLiab { get; set; }
        public bool isTRIALiab { get; set; }
        public string WarLimit { get; set; }
        public float WarRate { get; set; }
        public float TRIARate { get; set; }
        public float WarLiabPrem { get; set; }
        public float TRIALiabPrem { get; set; }
        public float TotalLiabPrem { get; set; }
        public bool isAttached { get; set; }
        public int CreatedById { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int UpdatedById { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public float TotalAnnualLiabPrem { get; set; }
        public float WrittenPrem { get; set; }
        public bool isLiabOnly { get; set; }
    }

    public class NamedInsuredAircraft : BaseEntity
    {
        public int QuoteId { get; set; }
        public int QuoteAircraftId { get; set; }
        public string FAANo { get; set; }
        public int ControlNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Year { get; set; }
        public string Make { get; set; }
        public long ModelID { get; set; }
        public string AirportID { get; set; }
    }

    public class RiskLog : BaseEntity
    {
        public long Id { get; set; }
        public int RiskId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public DateTime? ActionDate { get; set; }
        public string LogAction { get; set; }
    }

    public class StatusReason : BaseEntity
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }
        public bool IsActive { get; set; }
        public string ReasonDesc { get; set; }
    }

    public class RiskGraph : BaseEntity
    {
        public int RiskId { get; set; }
        public string Branch { get; set; }
        public string ImageRightId { get; set; }
        public string ProductLine { get; set; }
        public string PolicyNumber { get; set; }
        public int ControlNumber { get; set; }
        public string Status { get; set; }
        public DateTime EffectiveDate { get; set; }
        public int EffectiveMonth { get; set; }
        public int EffectiveYear { get; set; }
        public DateTime ExpirationDate { get; set; }
        public int ExpirationMonth { get; set; }
        public int ExpirationYear { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string AgencyID { get; set; }
        public string AgencyName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
        public string Name { get; set; }
        public int UnderwriterId { get; set; }
        public int RenewalUnderwriterId { get; set; }
        public int UnderwriterAssistantId { get; set; }
        public string UW { get; set; }
        public string UA { get; set; }
        public decimal InceptionPremium { get; set; }
        public decimal AnnualizedPremium { get; set; }
        public decimal WrittenPremium { get; set; }
        public decimal EarnedPremium { get; set; }
        public decimal ExpiringEarnedPremium { get; set; }
        public decimal ExpiringWrittenPremium { get; set; }
        public decimal ExpiredAnnualizedPremium { get; set; }
        public bool IsRenewal { get; set; }
        public bool IsTargetAccount { get; set; }
        public bool IsPaidInInstallments { get; set; }
        public bool IsReporter { get; set; }
        public decimal DepositPremium { get; set; }
        public decimal ExpiringDepositPremium { get; set; }
        public string Prefix { get; set; }
        public string QuoteType { get; set; }
        public decimal Payroll { get; set; }
        public string ProgramType { get; set; }
        public string AccountDescription { get; set; }
        public decimal ScheduledRating { get; set; }
        public decimal ExpirienceModifier { get; set; }
        public decimal ExpiringPayroll { get; set; }
        public bool AppReceived { get; set; }
        public string Market { get; set; }
        public string PurposeOfUse { get; set; }


        public DateTime DateRangeCategory { get; set; }
        public string DateRangeGroup { get; set; }
        public string Timeframe { get; set; }
        public string BusinessType
        {
            get { return (IsRenewal) ? "Renewal" : "New Business"; }
        }
        public string DisplayEffectiveDate
        {
            get { return EffectiveDate.ToShortDateString(); }
        }
    }

    public class RiskPayment : BaseEntity
    {
        public long PaymentId { get; set; }
        public int RiskId { get; set; }
        [Required]
        public decimal AnticipatedAmount { get; set; }
        public decimal ActualAmount { get; set; }
        public DateTime? InvoicedDate { get; set; }
        [Required]
        public DateTime? DueDate { get; set; }
        public string ProcessedBy { get; set; }
        public DateTime? ReportReceived { get; set; }
        public int DueDateMonth { get { return DueDate.Value.Month; } }
        public int DueDateYear { get { return DueDate.Value.Year; } }
    }

    public class RiskLoction : BaseEntity
    {
        public int RiskLocationId { get; set; }
        public int RiskId { get; set; }
        public string AirportID { get; set; }
        public string CategoryID { get; set; }
        public string Occupancy { get; set; }
        public string PortionOccupied { get; set; }
        public bool AnyLocation { get; set; }
        public int LocationMultipler { get; set; }
        public float Modifier { get; set; }
        public float TotalPremium { get; set; }
        public int CreatedById { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int UpdatedById { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public bool IsIncluded { get; set; }
    }

    public class MonthlyBudget : BaseEntity
    {
        public int BudgetId { get; set; }
        public string ProductLine { get; set; }
        public string Branch { get; set; }
        public decimal Amount { get; set; }
        public int DateMonth { get; set; }
        public int DateYear { get; set; }
    }

    public class RiskEndorsement : BaseEntity
    {
        public int Id { get; set; }
        public int RiskId { get; set; }
        public int Record_ID { get; set; }
        public bool IsCarryOverforRenewal { get; set; }
        public bool IsCertificateRequired { get; set; }
        public string RateType { get; set; }
        public float Premium { get; set; }
        public bool IsCompleted { get; set; }
        public string Notes { get; set; }
        public int CreatedById { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int UpdatedById { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsNet { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public bool IsRequired { get; set; }
        public int intQuoteNumber { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public float ProRataAmount { get; set; }
        public string AirportCode { get; set; }
        public bool COI { get; set; }
        public int DaysNOC { get; set; }
        public int NonPayment { get; set; }
        public string LevelType { get; set; }
        public string AssignedQuoteAircraftID { get; set; }
        public string AssignedPilotID { get; set; }
        public string Edition { get; set; }
        public bool IsDoesNotWillAlso { get; set; }
        public bool IsIncludingExcluding { get; set; }
        public bool IsAdditionalReturn { get; set; }
        public string ClassCodeLevel { get; set; }
        public int QuoteAircraftLossPayeeID { get; set; }
        public string ActionType { get; set; }
        public string strNNumber { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public int DeleteAssignedQuoteEndorsementId { get; set; }
        public float MidtermAnnualPremium { get; set; }
        public bool IsAddedMidterm { get; set; }
        public int PreHourAmount { get; set; }
        public string ManualFormAndEdition { get; set; }
        public string PurposeOfFlight { get; set; }
        public bool IsIssueCOI { get; set; }
        public long IssueCOIQuoteAircraftID { get; set; }
        public string COIHolderIs { get; set; }
        public DateTime? IssuedDate { get; set; }
        public bool IsFullyEarned { get; set; }
        public float InvoicedPremium { get; set; }

        public RiskEndorsement()
        {
            IsCarryOverforRenewal = false;
        }
    }

    public class WorkCompProgramType : StringEnum
    {
        public static WorkCompProgramType GUARANTEED_COST = new WorkCompProgramType() { Value = "GC", DisplayText = "Guaranteed Cost" };
        public static WorkCompProgramType NBAA = new WorkCompProgramType() { Value = "NBAA", DisplayText = "NBAA" };
        public static WorkCompProgramType SSDIV = new WorkCompProgramType() { Value = "SSDIV", DisplayText = "SSDIV" };
        public static WorkCompProgramType DBA = new WorkCompProgramType() { Value = "DBA", DisplayText = "DBA" };
        public static WorkCompProgramType RET = new WorkCompProgramType() { Value = "RET", DisplayText = "Retro" };
        public static WorkCompProgramType LD = new WorkCompProgramType() { Value = "LD", DisplayText = "Large Deductible" };

        private WorkCompProgramType(string val, string text) : base(val, text) { }
        private WorkCompProgramType() { }
    }

    public class WorkCompAccountDesc : StringEnum
    {
        public static WorkCompAccountDesc AA = new WorkCompAccountDesc() { Value = "AA", DisplayText = "Airport Authority" };
        public static WorkCompAccountDesc ALS = new WorkCompAccountDesc() { Value = "ALS", DisplayText = "Airline and/or Services" };
        public static WorkCompAccountDesc CGO = new WorkCompAccountDesc() { Value = "CGO", DisplayText = "Cargo Operations" };
        public static WorkCompAccountDesc FBO = new WorkCompAccountDesc() { Value = "FBO", DisplayText = "FBO" };
        public static WorkCompAccountDesc FBOFI = new WorkCompAccountDesc() { Value = "FBOFI", DisplayText = "FBO & Flight Instruction" };
        public static WorkCompAccountDesc IR = new WorkCompAccountDesc() { Value = "IR", DisplayText = "Instruction & Rental Only" };
        public static WorkCompAccountDesc IAC = new WorkCompAccountDesc() { Value = "IAC", DisplayText = "Turbine IA/Charter" };
        public static WorkCompAccountDesc MRO = new WorkCompAccountDesc() { Value = "MRO", DisplayText = "Man/Repair Org" };
        public static WorkCompAccountDesc NOC = new WorkCompAccountDesc() { Value = "NOC", DisplayText = "Not Otherwise Classified" };
        public static WorkCompAccountDesc O135 = new WorkCompAccountDesc() { Value = "O135", DisplayText = "Other 135 Ops" };
        public static WorkCompAccountDesc RW = new WorkCompAccountDesc() { Value = "RW", DisplayText = "Rotorwing" };

        private WorkCompAccountDesc(string val, string text) : base(val, text) { }
        private WorkCompAccountDesc() { }
    }

}
