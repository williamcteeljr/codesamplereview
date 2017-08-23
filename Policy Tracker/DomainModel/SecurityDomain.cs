using PolicyTracker.DomainModel.Framework;
using System;
using System.Collections.Generic;

namespace PolicyTracker.DomainModel.Security
{
    public class SecurityRole : StringEnum
    {
        public static SecurityRole UW = new SecurityRole() { Value = "Underwriter", DisplayText = "Underwriter" };
        public static SecurityRole UA = new SecurityRole() { Value = "Underwriter Assistant", DisplayText = "Underwriter Assistant" };
        public static SecurityRole BM = new SecurityRole() { Value = "Branch Manager", DisplayText = "Branch Manager" };
        public static SecurityRole PM = new SecurityRole() { Value = "Product Line Manager", DisplayText = "Product Line Manager" };
        public static SecurityRole ADMIN = new SecurityRole() { Value = "Admin", DisplayText = "Admin" };
        public static SecurityRole EXEC = new SecurityRole() { Value = "Exec", DisplayText = "Exec" };

        private SecurityRole(string val, string text) : base(val, text) { }
        private SecurityRole() { }
    }

    public class User : BaseEntity
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }
        public string PersonalEmail { get; set; }
        public string WorkEmail { get; set; }
        public string WorkPhone { get; set; }
        public string MobilePhone { get; set; }
        public string OtherPhone { get; set; }
        public string Fax { get; set; }
        public string Country { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public bool Active { get; set; }
        public string Photo { get; set; }
        public string BusinessTitle { get; set; }
        public string CompanyName { get; set; }
        public bool IsLockedOut { get; set; }
        public DateTime? LockedOutTime { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsSuperAdmin { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int CreatedById { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int UpdatedById { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string BranchID { get; set; }
        public string SignatureImage { get; set; }
        public bool IsShowPhotoOnEmail { get; set; }
        public int defaultSystemTabBaseId { get; set; }
        public int defaultSystemTabRibbonId { get; set; }
        public int PortalHostAPIID { get; set; }
        public string Initials { get; set; }
        public int ProductLine { get; set; }

        //Related Entities
        //public IEnumerable<UserRoleAssignment> RoleAssignments { get; set; }
        public IEnumerable<SecurityGroupRole> SecurityRoles { get; set; }
        public IEnumerable<SecurityGroup> SecurityGroups { get; set; }

        //Facade Properties
        public string Name
        {
            get { return FirstName + " " + LastName; }
        }
        public string UWDisplayName
        {
            get { return FirstName + " " + LastName + " [" + BranchID + "]"; }
        }
    }

    public class UserGraph : BaseEntity
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int UserRoleId { get; set; }
        public string RoleName { get; set; }
        public string GroupName { get; set; }
    }

    public class UserRoleAssignment : BaseEntity
    {
        public int UserRoleId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public int CreatedById { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int UpdatedById { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class SecurityGroup : BaseEntity
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int CreatedById { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int UpdatedById { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class SecurityGroupRole : BaseEntity
    {
        public int RoleId { get; set; }
        public int GroupId { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public bool IsAutoAssignment { get; set; }
        public int CreatedById { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int UpdatedById { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class SecurityResource : BaseEntity
    {
        public int Id { get; set; }
        public string ResourcePath { get; set; }
        public string Name { get; set; }
    }

    public class SecurityAccess : BaseEntity
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public int ResourceId { get; set; }
        public bool Read { get; set; }
        public bool Write { get; set; }
        public bool Delete { get; set; }

        //Facade Properties
        public string RoleName { get; set; }
        public string ResourceName { get; set; }
    }
}
