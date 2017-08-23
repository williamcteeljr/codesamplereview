using PolicyTracker.DomainModel.Framework;
using PolicyTracker.DomainModel.PostNotice;
using PolicyTracker.Platform.UOW;
using System;
using System.Collections.Generic;
using System.Text;

namespace PolicyTracker.DataAccess.WCBindingPostingNotice
{
    public class WCPnPrimaryContactDomainDAO : BaseDAO<WCPnPrimaryContactDomain>
    {
        public WCPnPrimaryContactDomainDAO()
            : base("UWBranch", "UW_Base_App", defaultOrderFilter: new OrderFilter("BranchID", OrderFilter.Comparator.Descending))
        {
            AddColumnMapping("branchID", "BranchID", true, false);
            AddColumnMapping("branchDescription", "BranchDescription");
            AddColumnMapping("streetAddress1", "StreetAddress1");
            AddColumnMapping("streetAddress1", "StreetAddress2");
            AddColumnMapping("city", "City");
            AddColumnMapping("state", "State");
            AddColumnMapping("postalcode", "Zip");
            Initialize();
        }

        public IEnumerable<WCPnPrimaryContactDomain> getUWbranch(UnitOfWork uow, string BranchID)
        {
            var sql = new StringBuilder("Select * from UWbranch");
            sql.Append(String.Format(" Where BranchID = '{0}'", BranchID));
            var result = Query<WCPnPrimaryContactDomain>(uow, sql.ToString());
            return result;
        }
    }

    public class WCPnInsuredDomainDAO : BaseDAO<WCPnInsuredDomain>
    {
        public WCPnInsuredDomainDAO()
            : base("ClassofBusinessGroup", "UW_Base_App", defaultOrderFilter: new OrderFilter("ClassofBusinessGroupId", OrderFilter.Comparator.Descending))
        {
            AddColumnMapping("ClassofBusinessGroupId", "ClassofBusinessGroupId", true, true);
            AddColumnMapping("GroupName", "GroupName");
            Initialize();
        }


        public IEnumerable<WCPnInsuredDomain> getInsuredInformation(UnitOfWork uow, string RisKId)
        {
            var results = ExecuteStoredProcedureList<WCPnInsuredDomain>(uow, "PostNoticeGetInsuredInfo", new { QuoteId = RisKId });
            return results;
        }

        public IEnumerable<WCPnPolicyDomain> getPolicyInformation(UnitOfWork uow, string RisKId, string BranchId, string State)
        {
            var results = ExecuteStoredProcedureList<WCPnPolicyDomain>(uow, "PostNoticeGetPolicyInfo", new { QuoteId = RisKId, BranchID = BranchId, State = State });
            return results;
        }

        public IEnumerable<WCPnLocationDomain> getPolicyInsuredLocation(UnitOfWork uow, string RisKId)
        {
            var results = ExecuteStoredProcedureList<WCPnLocationDomain>(uow, "PostNoticeGetLocationInfo", new { QuoteId = RisKId });
            return results;
        }

        public IEnumerable<WCPnLocationDomain> getAllLocation(UnitOfWork uow, string RisKId)
        {
            var results = ExecuteStoredProcedureList<WCPnLocationDomain>(uow, "PostNoticeGetAllLocationByQuoteId", new { QuoteId = RisKId });
            return results;
        }

        public IEnumerable<WCPnLocationDomain> getMainInsuredLocation(UnitOfWork uow, string RisKId)
        {
            var results = ExecuteStoredProcedureList<WCPnLocationDomain>(uow, "PostNoticeGetMainInsuredLocation", new { QuoteId = RisKId });
            return results;
        }

        public IEnumerable<WCPnConfirmationDomain> InsertConfirmation(UnitOfWork uow, string RisKId, string SubmissionType, string Status, string Submission, string SentById)
        {
            var results = ExecuteStoredProcedureList<WCPnConfirmationDomain>(uow, "PostNoticeInsertConfirmation", new { QuoteId = RisKId, SubmissionType = SubmissionType, Status = Status, Submission = Submission, SentById = SentById });
            return results;
        }

        public IEnumerable<WCPnConfirmationDomain> UpdateConfirmation(UnitOfWork uow, string Id, string Status, string ConfirmationCode, string Response, string TotalLocationCount, string ErrorMessage, string ResponseDate, string Submission)
        {
            var results = ExecuteStoredProcedureList<WCPnConfirmationDomain>(uow, "PostNoticeUpdateConfirmation", new
            {
                Id = Id,
                Status = Status,
                ConfirmationCode = ConfirmationCode,
                Response = Response,
                TotalLocationCount = TotalLocationCount,
                ErrorMessage = ErrorMessage,
                ResponseDate = ResponseDate,
                Submission = Submission
            });
            return results;
        }

        public IEnumerable<WCPnConfirmationDomain> ErrorUpdateConfirmation(UnitOfWork uow, string Id, string Status, string ErrorMessage)
        {
            var results = ExecuteStoredProcedureList<WCPnConfirmationDomain>(uow, "PostNoticeErrorUpdateConfirmation", new
            {
                Id = Id,
                Status = Status,
                ErrorMessage = ErrorMessage
            });
            return results;
        }

        public IEnumerable<WCPnInsuredDomain> getAdditionalInsuredInformation(UnitOfWork uow, string RisKId)
        {
            var results = ExecuteStoredProcedureList<WCPnInsuredDomain>(uow, "PostNoticeGetAdditionalInsuredInfo", new { QuoteId = RisKId });
            return results;
        }

        public IEnumerable<WCPnLocationDomain> getAdditionalInsuredLocation(UnitOfWork uow, string AdditionalInsuredId)
        {
            var results = ExecuteStoredProcedureList<WCPnLocationDomain>(uow, "PostNoticeGetAdditionalInsuredLocationInfo", new { ADDITIONALNAMEDINSUREDID = AdditionalInsuredId });
            return results;
        }

        public IEnumerable<WCPnLocationDomain> getLocationById(UnitOfWork uow, string LocationId)
        {
            var results = ExecuteStoredProcedureList<WCPnLocationDomain>(uow, "PostNoticeGetLocationById", new { LocationId = LocationId });
            return results;

        }

        public IEnumerable<WCPnInsuredDomain> getAdditionalInsuredById(UnitOfWork uow, string AdditionalInsuredId)
        {
            var results = ExecuteStoredProcedureList<WCPnInsuredDomain>(uow, "PostNoticeAdditionalInsuredByIdInfo", new { ID = AdditionalInsuredId });
            return results;
        }
    }
}
