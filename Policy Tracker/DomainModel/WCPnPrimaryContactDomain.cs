using PolicyTracker.DomainModel.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolicyTracker.DomainModel.PostNotice
{
    public class WCPnPrimaryContactDomain : BaseEntity
    {
        public String branchID { get; set; }
        public String branchDescription { get; set; }
        public String companyName { get; set; }
        public String contactName { get; set; }
        public String officePhone { get; set; }
        public String mobilePhone { get; set; }
        public String email { get; set; }
        public String streetAddress1 { get; set; }
        public String streetAddress2 { get; set; }
        public String city { get; set; }
        public String state { get; set; }
        public String zip { get; set; }
    }
}
