using PolicyTracker.DomainModel.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolicyTracker.DomainModel.PostNotice
{
    public class WCPnPolicyDomain : BaseEntity
    {
        public string number { get; set; } //Policy Number
        public string effectivedate { get; set; }
        public string expirationdate { get; set; }
        public string reportclaimbycontacting { get; set; }
        public string insconame { get; set; }
        public string naicnumber { get; set; }
        public string inscoaddress1 { get; set; }
        public string inscocity { get; set; }
        public string inscostate { get; set; }
        public string inscopostalcode { get; set; }
        public string inscophone { get; set; }

        //Broker Info
        public string brokerorgname { get; set; }
        public string brokerorgrepname { get; set; }
        public string brokerorgaddress1 { get; set; }
        public string brokerorgaddress2 { get; set; }
        public string brokerorgreptitle { get; set; }
        public string brokerorgcity { get; set; }
        public string brokerorgstate { get; set; }
        public string brokerorgpostalcode { get; set; }


        //Claim Admin
        public string claimadminorgname { get; set; }
        public string claimadminorgaddress1 { get; set; }
        public string claimadminorgaddress2 { get; set; }
        public string claimadminorgcity { get; set; }
        public string claimadminorgstate { get; set; }
        public string claimadminorgpostalcode { get; set; }
        public string claimadminrepname { get; set; }
        public string claimadminreptitle { get; set; }
        public string claimadminrepphone { get; set; }
        public string claimadminrepemail { get; set; }

        //Location Info
        public List<WCPnLocationDomain> location { get; set; }
    }
}
