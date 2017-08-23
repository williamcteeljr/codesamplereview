using PolicyTracker.DomainModel.Framework;
using PolicyTracker.DomainModel.Security;
using PolicyTracker.Platform.UOW;
using System;
using System.Collections.Generic;
using System.Text;

namespace PolicyTracker.DataAccess.Security
{
    public class UserEntityDAO : BaseDAO<User>
    {
        public UserEntityDAO()
            : base("User", "UW_Base_App", defaultOrderFilter: new OrderFilter("FirstName", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("UserId", "UserId", true, true);
            AddColumnMapping("FirstName", "FirstName");
            AddColumnMapping("LastName", "LastName");
            AddColumnMapping("NickName", "NickName");
            AddColumnMapping("PersonalEmail", "PersonalEmail");
            AddColumnMapping("WorkEmail", "WorkEmail");
            AddColumnMapping("WorkPhone", "WorkPhone");
            AddColumnMapping("MobilePhone", "MobilePhone");
            AddColumnMapping("OtherPhone", "OtherPhone");
            AddColumnMapping("Fax", "Fax");
            AddColumnMapping("Country", "Country");
            AddColumnMapping("Address1", "Address1");
            AddColumnMapping("Address2", "Address2");
            AddColumnMapping("City", "City");
            AddColumnMapping("State", "State");
            AddColumnMapping("PostalCode", "PostalCode");
            AddColumnMapping("Active", "Active");
            AddColumnMapping("Photo", "Photo");
            AddColumnMapping("BusinessTitle", "BusinessTitle");
            AddColumnMapping("CompanyName", "CompanyName");
            AddColumnMapping("IsLockedOut", "IsLockedOut");
            AddColumnMapping("LockedOutTime", "LockedOutTime");
            AddColumnMapping("IsAdmin", "IsAdmin");
            AddColumnMapping("IsSuperAdmin", "IsSuperAdmin");
            AddColumnMapping("UserName", "UserName");
            AddColumnMapping("Password", "Password");
            AddColumnMapping("CreatedById", "CreatedById");
            AddColumnMapping("CreatedDate", "CreatedDate");
            AddColumnMapping("UpdatedById", "UpdatedById");
            AddColumnMapping("UpdatedDate", "UpdatedDate");
            AddColumnMapping("BranchID", "BranchID");
            AddColumnMapping("SignatureImage", "SignatureImage");
            AddColumnMapping("IsShowPhotoOnEmail", "IsShowPhotoOnEmail");
            AddColumnMapping("defaultSystemTabBaseId", "defaultSystemTabBaseId");
            AddColumnMapping("defaultSystemTabRibbonId", "defaultSystemTabRibbonId");
            AddColumnMapping("PortalHostAPIID", "PortalHostAPIID");
            AddColumnMapping("Initials", "Initials");
            AddColumnMapping("ProductLine", "ProductLine");
            Initialize();
        }

        public IEnumerable<User> GetUnderwriters(UnitOfWork uow, string roleName, string branch = null)
        {
            var sql = new StringBuilder("SELECT	U.UserId, U.UserName, U.FirstName, U.LastName, U.FirstName + ' ' + U.LastName as Name");
            sql.Append(" FROM	[User] U");
            sql.Append(" JOIN [UserRoles] UR on UR.UserId = U.UserId");
            sql.Append(" JOIN [GroupRole] GR on GR.RoleId = UR.RoleId");
            sql.Append(String.Format(" WHERE	GR.RoleName = '{0}'", roleName));
            sql.Append(" ORDER BY U.FirstName");
            if (!String.IsNullOrEmpty(branch)) sql.Append(String.Format(" AND U.BranchID = '{0}'", branch));
            var result = Query<User>(uow, sql.ToString());
            return result;
        }

        public IEnumerable<User> GetUnderwritingAssistants(UnitOfWork uow, string roleName, string branch = null)
        {
            var sql = new StringBuilder("SELECT	U.UserId, U.UserName, U.FirstName, U.LastName, U.FirstName + ' ' + U.LastName as Name");
            sql.Append(" FROM	[User] U");
            sql.Append(" JOIN [UserRoles] UR on UR.UserId = U.UserId");
            sql.Append(" JOIN [GroupRole] GR on GR.RoleId = UR.RoleId");
            sql.Append(String.Format(" WHERE	GR.RoleName = '{0}'", roleName));
            sql.Append(" ORDER BY U.FirstName");
            if (!String.IsNullOrEmpty(branch)) sql.Append(String.Format(" AND U.BranchID = '{0}'", branch));
            var result = Query<User>(uow, sql.ToString());
            return result;
        }
    }

    public class UserGraphDAO : BaseDAO<UserGraph>
    {
        public UserGraphDAO()
            : base("Users", "UW_Base_App", defaultOrderFilter: new OrderFilter("LastName", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("UserId", "UserId");
            AddColumnMapping("FirstName", "FirstName");
            AddColumnMapping("LastName", "LastName");
            AddColumnMapping("UserRoleId", "UserRoleId");
            AddColumnMapping("RoleName", "RoleName");
            AddColumnMapping("GroupName", "GroupName");
            Initialize();
        }
    }

    public class UserRoleAssignmentsDAO : BaseDAO<UserRoleAssignment>
    {
        public UserRoleAssignmentsDAO()
            : base("UserRoles", "UW_Base_App", defaultOrderFilter: new OrderFilter("UserRoleId", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("UserRoleId", "UserRoleId", true, true);
            AddColumnMapping("UserId", "UserId");
            AddColumnMapping("RoleId", "RoleId");
            AddColumnMapping("CreatedById", "CreatedById");
            AddColumnMapping("CreatedDate", "CreatedDate");
            AddColumnMapping("UpdatedById", "UpdatedById");
            AddColumnMapping("UpdatedDate", "UpdatedDate");
            Initialize();
        }
    }

    public class SecurityGroupDAO : BaseDAO<SecurityGroup>
    {
        public SecurityGroupDAO()
            : base("Group", "UW_Base_App", defaultOrderFilter: new OrderFilter("GroupId", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("GroupId", "GroupId", true, true);
            AddColumnMapping("GroupName", "GroupName");
            AddColumnMapping("Description", "Description");
            AddColumnMapping("IsActive", "IsActive");
            AddColumnMapping("CreatedById", "CreatedById");
            AddColumnMapping("CreatedDate", "CreatedDate");
            AddColumnMapping("UpdatedById", "UpdatedById");
            AddColumnMapping("UpdatedDate", "UpdatedDate");
            Initialize();
        }
    }

    public class SecurityGroupRoleDAO : BaseDAO<SecurityGroupRole>
    {
        public SecurityGroupRoleDAO()
            : base("GroupRole", "UW_Base_App", defaultOrderFilter: new OrderFilter("RoleId", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("RoleId", "RoleId", true, true);
            AddColumnMapping("GroupId", "GroupId");
            AddColumnMapping("RoleName", "RoleName");
            AddColumnMapping("Description", "Description");
            AddColumnMapping("IsActive", "IsActive");
            AddColumnMapping("IsAutoAssignment", "IsAutoAssignment");
            AddColumnMapping("CreatedById", "CreatedById");
            AddColumnMapping("CreatedDate", "CreatedDate");
            AddColumnMapping("UpdatedById", "UpdatedById");
            AddColumnMapping("UpdatedDate", "UpdatedDate");
            Initialize();
        }
    }

    public class SecurityResourceDAO : BaseDAO<SecurityResource>
    {
        public SecurityResourceDAO()
            : base("SecurityResource", "UW_Base_App", defaultOrderFilter: new OrderFilter("Id", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("Id", "ResourceId", true, true);
            AddColumnMapping("ResourcePath", "ResourcePath");
            AddColumnMapping("Name", "Name");
            Initialize();
        }
    }

    public class SecurityAccessDAO : BaseDAO<SecurityAccess>
    {
        public SecurityAccessDAO()
            : base("SecurityAccess", "UW_Base_App", defaultOrderFilter: new OrderFilter("Id", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("Id", "Id", true, true);
            AddColumnMapping("RoleId", "RoleId");
            AddColumnMapping("ResourceId", "ResourceId");
            AddColumnMapping("Read", "Read");
            AddColumnMapping("Write", "Write");
            AddColumnMapping("Delete", "Delete");
            Initialize();
        }

        public IEnumerable<dynamic> GetUserSecurityPrivileges(UnitOfWork uow, long userId)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT U.UserId, UR.RoleId, RoleName, G.GroupId, G.GroupName, SR.ResourcePath, SR.Name, SA.[Read], SA.Write, SA.[Delete]");
            sql.Append(" FROM UW_Base_App.dbo.[User] U");
            sql.Append(" JOIN UW_Base_App.dbo.UserRoles UR ON U.UserId = UR.UserId");
            sql.Append(" JOIN UW_Base_App.dbo.GroupRole GR On GR.RoleId = UR.RoleId");
            sql.Append(" JOIN UW_Base_App.dbo.[Group] G ON G.GroupId = GR.GroupId");
            sql.Append(" JOIN UW_Base_App.dbo.SecurityAccess SA ON SA.RoleId = GR.RoleId");
            sql.Append(" JOIN UW_Base_App.dbo.SecurityResource SR ON SA.ResourceId = SR.ResourceId");
            sql.Append(" WHERE U.UserId = @UserId");
            var results = Query<dynamic>(uow, sql.ToString(), new { UserId = userId });
            return results;
        }

        public IEnumerable<dynamic> GetSecurityAccesses(UnitOfWork uow)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" SELECT	SA.Id, GR.RoleName, SR.ResourcePath, SR.Name, SA.[Read], SA.Write, SA.[Delete]");
            sql.Append(" FROM	SecurityAccess SA");
            sql.Append(" JOIN GroupRole GR ON GR.RoleId = SA.RoleId");
            sql.Append(" JOIN SecurityResource SR ON SR.ResourceId = SA.ResourceId");
            var results = Query<dynamic>(uow, sql.ToString());
            return results;
        }
    }
}
