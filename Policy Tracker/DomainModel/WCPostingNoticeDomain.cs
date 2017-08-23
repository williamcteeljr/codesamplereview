using PolicyTracker.DomainModel.Framework;
using PolicyTracker.DomainModel.PostNotice;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace PolicyTracker.DomainModel.Policy
{
    public class UWBranch : BaseEntity
    {
        public String branchIDe { get; set; }
        public String branchDescription { get; set; }
        public String companyName { get; set; }
        public String contactName { get; set; }
        public String officePhone { get; set; }
        public String mobilePhone { get; set; }
        public String email { get; set; }
        public String address1 { get; set; }
        public String address2 { get; set; }
        public String city { get; set; }
        public String state { get; set; }
        public String postalcode { get; set; }
    }

    public class AdditionalNamedInsured : BaseEntity
    {
        public int AdditionalNamedInsuredId { get; set; }
        public int QuoteId { get; set; }
        [Required(ErrorMessage = "The FEIN is required.  Must start with 2 digits, a dash, followed by a max of 7 digits.")]
        [RegularExpression(@"^\(?([0-9]{2})\)?[-. ]?([0-9]{7})$", ErrorMessage = "The FEIN number must start with 2 digits, a dash, followed by a max of 7 digits.")]
        public String FEIN { get; set; }
        [Required(ErrorMessage = "The Company is required")]
        [MaxLength(50, ErrorMessage = "Company Name has a 50 character limit, including spaces.")]
        public String CompanyName { get; set; }
        public String Name2 { get; set; }
        //[Required(ErrorMessage = "The Address is required.")]
        public String StreetAddress1 { get; set; }
        public String StreetAddress2 { get; set; }
        //[Required(ErrorMessage = "The City is required.")]
        public String City { get; set; }
        //[Required(ErrorMessage = "The State abbreviation is required.")]
        //[MaxLength(2, ErrorMessage = "State has a 2 character limit, including spaces.")]
        public String State { get; set; }
        //[Required(ErrorMessage = "The Zip Code is required.")]
        //[RegularExpression(@"^(?!00000)[0-9]{5,5}$", ErrorMessage = "error Message")]
        public String Zip { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public String EmployeeCount { get; set; }

        //property that contains the Locations we will display in listbox
        public int[] Id { get; set; }
        public int[] SelectedItemsIds { get; set; }
        public IEnumerable<SelectListItem> OptionList { get; set; }
        public IEnumerable<SelectListItem> SelectedOptionList { get; set; }
        public List<WCPnLocationDomain> Locations { get; set; }
        //property used to receive the ID's of the locations
        // IEnumerable<String> SelectedLocations { get; set; }
    }

    public class Locations : BaseEntity
    {
        public int LocationId { get; set; }
        public int QuoteId { get; set; }
        public int AdditionalNamedInsuredId { get; set; }
        public int ControlNumber { get; set; }
        public String PolicyNumber { get; set; }
        [Required(ErrorMessage = "The Company is required.")]
        [MaxLength(50, ErrorMessage = "Company Name has a 50 character limit, including spaces.")]
        public String CompanyName { get; set; }
        public String Name2 { get; set; }
        [Required(ErrorMessage = "The Street Address is required.")]
        public String StreetAddress1 { get; set; }
        public String StreetAddress2 { get; set; }
        [Required(ErrorMessage = "The City is required.")]
        public String City { get; set; }
        [Required(ErrorMessage = "The State abbreviation is required.")]
        [MinLength(2, ErrorMessage ="State has a 2 character minimum limit, including spaces.")]
        [MaxLength(2, ErrorMessage = "State has a 2 character maximum limit, including spaces.")]
        public String State { get; set; }
        [Required(ErrorMessage = "The Zip Code is required.")]
        [RegularExpression(@"^\d{5}(?:[-\s]\d{4})?$", ErrorMessage = "The Zip code must have at least 5 digits with optional 4 digits ")]
        public String ZipCode { get; set; }
        public String AirportID { get; set; }
        [Required(ErrorMessage = "The Employee Count is required.")]
        public String EmployeeCount { get; set; }
        [Required(ErrorMessage = "The Quantity is required and must be at least 1.")]
        [Range(1, 1000, ErrorMessage = "The Quantity requires at least 1 print quantity.")]
        //[RegularExpression(@"^1\d{1-1000}$", ErrorMessage = "The Quantity must be at least 1 or more.")]
        public int Quantity { get; set; }
    }

    public class WCEmployeeType : BaseEntity
    {
        public int EmployeeTypeId { get; set; }
        public String EmployeeTypeName { get; set; }
    }

    public class EndorsedPolicies : BaseEntity
    {
        public int EP_ID { get; set; }
        public int QuoteId { get; set; }
        public int PolicyNumber { get; set; }
        public Boolean PostingNoticeSent { get; set; }
    }

    public class ClassCodes : BaseEntity
    {
        public int ClassCodeID { get; set; }
        public int Class { get; set; }
        public String HazardGroupCode { get; set; }
        public WCEmployeeType EmployeeTypeID { get; set; }
    }

    public class Exposure : BaseEntity
    {
        public int ExposureID { get; set; }
        public int LocationID { get; set; }
        public int QuoteId { get; set; }
        public int EmployeeTypeID { get; set; }
        public int ClassCodeID { get; set; }
        [Required(ErrorMessage = "The Class Code is required.")]
        public String Class { get; set; }
        [Required(ErrorMessage = "The Employee Type Name is required.")]
        public String EmployeeTypeName { get; set; }
        [Required(ErrorMessage = "The Hazard Group Code is required.")]
        [MaxLength(1, ErrorMessage = "The Hazard Group Code has a 1 character limit, including spaces.")]
        public String HazardGroupCode { get; set; }
        [Required(ErrorMessage = "The Hazard State is required.")]
        [MaxLength(2, ErrorMessage = "Hazard State has a 2 character limit, including spaces.")]
        public String HazardState { get; set; }
        [Required(ErrorMessage = "The Premium Amount is required.")]
        public float EmployeePremium { get; set; }
        [Required(ErrorMessage = "The Payroll Amount is required.")]
        public float EmployeePayroll { get; set; }
        [Required(ErrorMessage = "The Employee Count is required.")]
        public String EmployeeCount { get; set; }
        [Required(ErrorMessage = "The State Premium Amount is required.")]
        public float StatePremium { get; set; }
        public String PolicyNumber { get; set; }

        //property objects that contain the Locations we will display in listbox
        public int[] ExpId { get; set; }
        public int[] ExpSelectedItemsIds { get; set; }
        public IEnumerable<SelectListItem> ExpOptionList { get; set; }
        public IEnumerable<SelectListItem> ExpSelectedOptionList { get; set; }
        public List<WCPnLocationDomain> ExpLocations { get; set; }
    }

    public class PNConfirmation : BaseEntity
    {
        public int PNConfirmationId { get; set; }
        public int QuoteId { get; set; }
        public String SubmissionType { get; set; }
        public String Status { get; set; }
        public String Submission { get; set; }
        public String ConfirmationCode { get; set; }
        public String Response { get; set; }
        public int TotalLocationCount { get; set; }
        public String ErrorMessage { get; set; }
        public int SentById { get; set; }
        public DateTime SentDate { get; set; }
        public DateTime ResponseDate { get; set; }

    }

    public class PNClaimsAdmin : BaseEntity
    {
        public int PNClaimsAdminId { get; set; }
        public String ClaimState { get; set; }
        public String ClaimCompanyName { get; set; }
        public String StreetAddress1 { get; set; }
        public String StreetAddress2 { get; set; }
        public String City { get; set; }
        public String State { get; set; }
        public int ZipCode { get; set; }
        public int PhoneNumber { get; set; }
        public String BranchManagerName { get; set; }
        public String BranchManagerEmail { get; set; }
        public int PNClaimResolutionMgrId { get; set; }
        public String SupervisorName { get; set; }
        public int SupvPhoneNumber { get; set; }
        public String SupvEmail { get; set; }
        public String ServiceRep { get; set; }
        public int ServiceRepPhone { get; set; }
        public String ServiceRepEmail { get; set; }
    }

    public class AddInsuredLocationMapping : BaseEntity
    {
        public int MappingId { get; set; }
        public int AdditionalNamedInsuredId { get; set; }
        public int LocationId { get; set; }
        public int QuoteId { get; set; }
    }
}
