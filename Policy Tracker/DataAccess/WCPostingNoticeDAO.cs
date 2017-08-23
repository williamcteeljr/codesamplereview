using PolicyTracker.DomainModel;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.DomainModel.Policy;
using PolicyTracker.DomainModel.PostNotice;
using PolicyTracker.Platform.UOW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolicyTracker.DataAccess.WCPostingNotice
{   
    public class LocationsDAO : BaseDAO<Locations>
    {
        public LocationsDAO()
            : base("Locations", "UW_Base_App", defaultOrderFilter: new OrderFilter("LocationId", OrderFilter.Comparator.Descending))
        {
            AddColumnMapping("LocationId", "LOCATION_ID", true, true);
            AddColumnMapping("QuoteId", "QUOTEID");
            AddColumnMapping("AdditionalNamedInsuredId", "ADDITIONALNAMEDINSUREDID");
            AddColumnMapping("PolicyNumber", "POLICYNUMBER");
            AddColumnMapping("CompanyName", "COMPANYNAME");
            AddColumnMapping("Name2", "NAME2");
            AddColumnMapping("StreetAddress1", "STREETADDRESS1");
            AddColumnMapping("StreetAddress2", "STREETADDRESS2");
            AddColumnMapping("City", "CITY");
            AddColumnMapping("State", "STATE");
            AddColumnMapping("ZipCode", "ZIPCODE");
            AddColumnMapping("AirportID", "AIRPORTID");
            AddColumnMapping("EmployeeCount", "EMPLOYEECOUNT");
            AddColumnMapping("Quantity", "QUANTITY");

            Initialize();
        }
    }

    public class AdditionalNamedInsuredDAO : BaseDAO<AdditionalNamedInsured>
    {
        public AdditionalNamedInsuredDAO()
            : base("AdditionalNamedInsured", "UW_Base_App", defaultOrderFilter: new OrderFilter("QuoteId", OrderFilter.Comparator.Descending))
        {
            AddColumnMapping("AdditionalNamedInsuredId", "ADDITIONALNAMEDINSUREDID", true, true);
            AddColumnMapping("QuoteId", "QUOTEID");
            AddColumnMapping("FEIN", "FEIN");
            AddColumnMapping("CompanyName", "COMPANYNAME");
            AddColumnMapping("Name2", "NAME2");
            AddColumnMapping("StreetAddress1", "STREETADDRESS1");
            AddColumnMapping("StreetAddress2", "STREETADDRESS2");
            AddColumnMapping("City", "CITY");
            AddColumnMapping("State", "STATE");
            AddColumnMapping("Zip", "ZIP");
            AddColumnMapping("EmployeeCount", "EMPLOYEECOUNT");
            AddColumnMapping("CreatedDate", "CREATEDDATE");
            AddColumnMapping("UpdatedDate", "UPDATEDDATE");
            
            Initialize();
        }

         public IEnumerable<AdditionalNamedInsured> UpdateAdditionalNamedInsured(UnitOfWork uow, int AdditionalNamedInsuredId, int quoteId, String fein, 
            String companyName, String name2,String address1, String address2, String city, String state, String zip, String employeeCount,
            DateTime updatedDate)
        {
            updatedDate = DateTime.Now;
            var results = ExecuteStoredProcedureList<AdditionalNamedInsured>(uow, "PostNoticeUpdateAdditionalNamedInsured",
                new
                {
                    AdditionalNamedInsuredId = AdditionalNamedInsuredId,
                    QuoteId = quoteId,
                    FEIN = fein,
                    CompanyName = companyName,
                    Name2 = name2,
                    StreetAddress1 = address1,
                    StreetAddress2 = address2,
                    City = city,
                    State = state,
                    Zip = zip,
                    EmployeeCount = employeeCount,                    
                    UpdatedDate = updatedDate
                });
            return results;
        }

        public IEnumerable<AdditionalNamedInsured> InsertAdditionalNamedInsured(UnitOfWork uow, int quoteId, String fein,
            String companyName, String name2, String address1, String address2, String city, String state, String zip, String employeeCount,
            DateTime createdDate, DateTime updatedDate)
        {
            createdDate = DateTime.Now;
            updatedDate = DateTime.Now;
            var results = ExecuteStoredProcedureList<AdditionalNamedInsured>(uow, "PostNoticeInsertAdditionalNamedInsured",
                new
                {
                    //AdditionalNamedInsuredId = AdditionalNamedInsuredId,
                    QuoteId = quoteId,
                    FEIN = fein,
                    CompanyName = companyName,
                    Name2 = name2,
                    StreetAddress1 = address1,
                    StreetAddress2 = address2,
                    City = city,
                    State = state,
                    Zip = zip,
                    EmployeeCount = employeeCount,
                    CreatedDate = createdDate,
                    UpdatedDate = updatedDate
                });
            return results;
        }
    }

    public class WCEmployeeTypeDAO : BaseDAO<WCEmployeeType>
    {
        public WCEmployeeTypeDAO()
            : base("WCEmployeeType", "UW_Base_App", defaultOrderFilter: new OrderFilter("EmployeeTypeName", OrderFilter.Comparator.Descending))
        {
            AddColumnMapping("EmployeeTypeId", "EMPLOYEETYPE_ID", true, true);
            AddColumnMapping("EmployeeTypeName", "EMPLOYEETYPENAME");

            Initialize();
        }
    }

    public class EndorsedPoliciesDAO : BaseDAO<EndorsedPolicies>
    {
        public EndorsedPoliciesDAO()
            : base("EndorsedPolicies", "UW_Base_App", defaultOrderFilter: new OrderFilter("PolicyNumber", OrderFilter.Comparator.Descending))
        {
            AddColumnMapping("EP_ID", "EP_ID");
            AddColumnMapping("QuoteId", "QUOTEID");
            AddColumnMapping("PolicyNumber", "POLICYNUMBER");
            AddColumnMapping("PostingNoticeSent", "POSTING_NOTICESENT");

            Initialize();
        }
    }

    public class ClassCodesDAO : BaseDAO<ClassCodes>
    {
        public ClassCodesDAO()
            : base("ClassCodes", "UW_Base_App", defaultOrderFilter: new OrderFilter("ClassCodeGroup", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("ClassCodeID", "CLASSCODE_ID", true, true);
            AddColumnMapping("Class", "CLASS");
            AddColumnMapping("HazardGroupCode", "HAZARDGROUPCODE");
            AddColumnMapping("EmployeeTypeID", "EMPLOYEETYPE_ID");

            Initialize();
        }
    }

    public class ExposureDAO : BaseDAO<Exposure>
    {
        public ExposureDAO()
            : base("WC_Exposure", "UW_Base_App", defaultOrderFilter: new OrderFilter("ExposureID", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("ExposureID", "EXPOSURE_ID", true, true);
            AddColumnMapping("LocationID", "LOCATION_ID");
            AddColumnMapping("QuoteId", "QUOTEID");
            AddColumnMapping("EmployeeTypeID", "EMPLOYEETYPE_ID");
            AddColumnMapping("ClassCodeID", "CLASSCODE_ID");
            AddColumnMapping("Class", "CLASS");
            AddColumnMapping("HazardGroupCode", "HAZARDGROUPCODE");
            AddColumnMapping("HazardState", "HAZARDSTATE");
            AddColumnMapping("EmployeePremium", "EMPLOYEE_PREMIUM");
            AddColumnMapping("EmployeePayroll", "EMPLOYEE_PAYROLL");
            AddColumnMapping("EmployeeCount", "EMPLOYEECOUNT");
            AddColumnMapping("StatePremium", "STATEPREMIUM");
            AddColumnMapping("PolicyNumber", "POLICYNUMBER");
            AddColumnMapping("EmployeeTypeName", "EMPLOYEETYPENAME");

            Initialize();
        }

        public IEnumerable<Exposure> UpdateExposure(UnitOfWork uow, int exposureId, int locationId, int quoteId, /*int employeeTypeId,
            int classCodeId,*/ String classCode, String hazardGroupCode, String hazardState, float employeePremium, 
            String employeeTypeName, float employeePayroll, String employeeCount, float statePremium, String policyNumber)
        {            
            var results = ExecuteStoredProcedureList<Exposure>(uow, "PostNoticeUpdateExposure",
                new
                {
                    ExposureID = exposureId,
                    LocationID = locationId,
                    QuoteId = quoteId,
                    //EmployeeTypeID = employeeTypeId,
                    //ClassCodeId = classCodeId,
                    Class = classCode,
                    HazardGroupCode = hazardGroupCode,
                    HazardState = hazardState,
                    EmployeePremium = employeePremium,
                    EmployeePayroll = employeePayroll,
                    EmployeeCount = employeeCount,
                    StatePremium = employeePremium,
                    EmployeeTypeName = employeeTypeName,
                    PolicyNumber = policyNumber
                });
            return results;
        }

        public IEnumerable<Exposure> InsertExposure(UnitOfWork uow, int locationId, int quoteId, /*int employeeTypeId,
            int classCodeId,*/ String classCode, String hazardGroupCode, String hazardState, float employeePremium, 
            String employeeTypeName, float employeePayroll, String employeeCount,
            float statePremium, String policyNumber)
        {            
            var results = ExecuteStoredProcedureList<Exposure>(uow, "PostNoticeInsertExposure",
                new
                {
                    LocationID = locationId,
                    QuoteId = quoteId,
                    //EmployeeTypeID = employeeTypeId,
                    //ClassCodeID = classCodeId,
                    Class = classCode,
                    HazardGroupCode = hazardGroupCode,
                    HazardState = hazardState,
                    EmployeePremium = employeePremium,
                    EmployeePayroll = employeePayroll,
                    EmployeeCount = employeeCount,
                    StatePremium = statePremium,
                    EmployeeTypeName = employeeTypeName,
                    PolicyNumber = policyNumber
                });
            return results;
        }

        public IEnumerable<WCPnLocationDomain> SelectLocationByExposureId(UnitOfWork uow, int ExposureID)
        {
            var results = ExecuteStoredProcedureList<WCPnLocationDomain>(uow, "PostNoticeSelectLocationByExposureId",
                new
                {
                    ExposureID = ExposureID
                });
            return results;
        }
    }

    public class PNConfirmationDAO : BaseDAO<PNConfirmation>
    {
        public PNConfirmationDAO()
        : base("PNConfirmation", "UW_Base_App", defaultOrderFilter: new OrderFilter("PNConfirmationId", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("PNConfirmationId", "PNCONFIRMATION_ID", true, true);
            AddColumnMapping("QuoteId", "QUOTEID");
            AddColumnMapping("SubmissionType", "SUBMISSIONTYPE");
            AddColumnMapping("Status", "STATUS");
            AddColumnMapping("Submission", "SUBMISSION");
            AddColumnMapping("ConfirmationCode", "CONFIRMATIONCODE");
            AddColumnMapping("Response", "RESPONSE");
            AddColumnMapping("TotalLocationCount", "TOTALLOCATIONCOUNT");
            AddColumnMapping("ErrorMessage", "ERRORMESSAGE");
            AddColumnMapping("SentById", "SENTBYID");
            AddColumnMapping("SentDate", "SENTDATE");           
            AddColumnMapping("ResponseDate", "RESPONSEDATE");
            Initialize();
        }
    }

    public class PNClaimsAdminDAO : BaseDAO<PNClaimsAdmin>
    {
        public PNClaimsAdminDAO()
        : base("PNClaimsAdmin", "UW_Base_App", defaultOrderFilter: new OrderFilter("PNCLAIMSADMIN_ID", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("PNClaimsAdminId", "PNCLAIMSADMIN_ID", true, true);
            AddColumnMapping("ClaimState", "CLAIMSTATE");
            AddColumnMapping("ClaimCompanyName", "NAME");
            AddColumnMapping("StreetAddress1", "STREETADDRESS1");
            AddColumnMapping("StreetAddress2", "STREETADDRESS2");
            AddColumnMapping("City", "CITY");
            AddColumnMapping("State", "STATE");
            AddColumnMapping("ZipCode", "ZIPCODE");
            AddColumnMapping("PhoneNumber", "PHONENUMBER");
            AddColumnMapping("BranchManagerName", "BRANCHMANAGERNAME");
            AddColumnMapping("BranchManagerEmail", "BRANCHMANAGEREMAIL");
            AddColumnMapping("PNClaimResolutionMgrId", "PNCLAIMRESOLUTIONMGR_ID");
            AddColumnMapping("SupervisorName", "SUPERVISORNAME");
            AddColumnMapping("SupvPhoneNumber", "SUPERVISORPHONE");
            AddColumnMapping("SupvEmail", "SUPERVISOREMAIL");
            AddColumnMapping("ServiceRep", "SERVICEREP");
            AddColumnMapping("ServiceRepPhone", "SERVICEREPPHONE");
            AddColumnMapping("ServiceRepEmail", "SERVICEREPEMAIL");
        }
    }

    public class AddInsuredLocationMappingDAO : BaseDAO<AddInsuredLocationMapping>
    {
        public AddInsuredLocationMappingDAO()
        : base("AddInsuredLocationMapping", "UW_Base_App", defaultOrderFilter: new OrderFilter("PNCLAIMSADMIN_ID", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("MappingId", "MAPPINGID", true, true);
            AddColumnMapping("AdditionalNamedInsuredId", "ADDITIONALNAMEDINSUREDID");
            AddColumnMapping("LocationId", "LOCATION_ID");
            AddColumnMapping("QuoteId", "QUOTEID");
        }

        public IEnumerable<AddInsuredLocationMapping> InsertMappingBinding(UnitOfWork uow, String quoteId, int AdditionalNamedInsuredId, int locationId)
        {
            var results = ExecuteStoredProcedureList<AddInsuredLocationMapping>(uow, "PostNoticeInsertMappingBinding", 
                new {
                    AdditionalNamedInsuredId = AdditionalNamedInsuredId,
                    LocationId = locationId,
                    QuoteId = quoteId
                    });
            return results;
        }

        public IEnumerable<AddInsuredLocationMapping> InsertMappingBindingMain(UnitOfWork uow, String quoteId, int locationId)
        {
            var results = ExecuteStoredProcedureList<AddInsuredLocationMapping>(uow, "PostNoticeInsertMappingBindingMain",
                new
                {
                    LocationId = locationId,
                    QuoteId = quoteId
                });
            return results;
        }

        public IEnumerable<AddInsuredLocationMapping> UpdateMappingBinding(UnitOfWork uow, int AdditionalNamedInsuredId, int LocationId, int QuoteId)
        {
            var results = ExecuteStoredProcedureList<AddInsuredLocationMapping>(uow, "PostNoticeUpdateMappingBinding", 
                new {                    
                    AdditionalNamedInsuredId = AdditionalNamedInsuredId,
                    LocationId = LocationId,
                    QuoteId = QuoteId
                });
            return results;
        }

        public IEnumerable<AddInsuredLocationMapping> DeleteMappingBindingByID(UnitOfWork uow, int AdditionalNamedInsuredId)
        {
            var results = ExecuteStoredProcedureList<AddInsuredLocationMapping>(uow, "PostNoticeDeleteMappingBindingByID",
                new
                {
                    AdditionalNamedInsuredId = AdditionalNamedInsuredId
                });
            return results;
        }

        public IEnumerable<AddInsuredLocationMapping> DeleteMappingBindingByQuoteID(UnitOfWork uow, int quoteId)
        {
            var results = ExecuteStoredProcedureList<AddInsuredLocationMapping>(uow, "PostNoticeDeleteMappingBindingByQuoteID",
                new
                {
                    QuoteId = quoteId
                });
            return results;
        }

        public IEnumerable<WCPnLocationDomain> SelectMappingBindingByID(UnitOfWork uow, int AdditionalNamedInsuredId)
        {
            var results = ExecuteStoredProcedureList<WCPnLocationDomain>(uow, "PostNoticeSelectMappingBindingByID",
                new
                {
                    AdditionalNamedInsuredId = AdditionalNamedInsuredId
                });
            return results;
        }

    }
}
