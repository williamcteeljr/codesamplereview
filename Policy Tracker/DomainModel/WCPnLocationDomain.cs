using PolicyTracker.DomainModel.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolicyTracker.DomainModel.PostNotice
{
    public class WCPnLocationDomain : BaseEntity
    {
        public int Id { get; set; }
        public string jurisdictionid { get; set; } //Jurisdiction State
        public string name { get; set; }
        public string address1 { get; set; }
        public string city { get; set; }
        public string State { get; set; }
        public string postalcode { get; set; }
        public string quantity { get; set; }
        public string employeecount { get; set; }
        public string locsequence { get; set; }

        //Override Claim Admin
        public string overrideclaimreportingaddress { get; set; }
        public string overrideclaimreportingname { get; set; }
        public string overrideclaimreportingaddress1 { get; set; }
        public string overrideclaimreportingaddress2 { get; set; }
        public string overrideclaimreportingcity { get; set; }
        public string overrideclaimreportingstate { get; set; }
        public string overrideclaimreportingpostalcode { get; set; }
        public string overrideclaimreportingrepname { get; set; }
        public string overrideclaimreportingreptitle { get; set; }
        public string overrideclaimreportingrepphone { get; set; }
        public string overrideclaimreportingrepemail { get; set; }

        //Virtual Sticker
        public string virtualstickertext { get; set; }
    }
}
