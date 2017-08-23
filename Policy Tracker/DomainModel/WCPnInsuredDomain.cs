using PolicyTracker.DomainModel.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolicyTracker.DomainModel.PostNotice
{
    public class WCPnInsuredDomain : BaseEntity
    { 
        //Insured Transcation Type
        public String transactiontype { get; set; }

        //Insured Info
        public string ID { get; set; }
        public string orgname1 { get; set; }
        public string fein { get; set; }
        public string StreetAddress1 { get; set; }
        public string StreetAddress2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string AgencyName { get; set; }
        public string brokerStreet1 { get; set; }

        //Shipping Info/Broker Info
        public string brokerStreet2 { get; set; }
        public string brokerCity { get; set; }
        public string brokerZip { get; set; }
        public string brokerState { get; set; }
        public string shiptoinsuredaddr { get; set; }
        public string shipmentmethod { get; set; }
        public string coverletterid { get; set; }


        //Policy Info
        public List<WCPnPolicyDomain> policy { get; set; }

        //Location Info
        public List<WCPnLocationDomain> location { get; set; }

        public WCPnInsuredDomain() { }

        public WCPnInsuredDomain(string _transactiontype, string _orgname1, string _fein, string _StreetAddress1, string _StreetAddress2, string _City, 
            string _State, string _Zip, string _shiptoinsuredaddr, string _shipmentmethod, List<WCPnPolicyDomain> _policy, List<WCPnLocationDomain> _location, 
            string _AgencyName, string _BrokerStreet1, string _BrokerStreet2, string _BrokerCity, string _BrokerZip, string _BrokerState, string _CoverletterId)
        {
            transactiontype = _transactiontype;
            orgname1 = _orgname1;
            fein = _fein;
            StreetAddress1 = _StreetAddress1;
            StreetAddress2 = _StreetAddress2;
            City = _City;
            State = _State;
            Zip = _Zip;
            shiptoinsuredaddr = _shiptoinsuredaddr;
            shipmentmethod = _shipmentmethod;
            policy = _policy;
            location = _location;
            AgencyName = _AgencyName;
            brokerStreet1 = _BrokerStreet1;
            brokerStreet2 = _BrokerStreet2;
            brokerCity = _BrokerCity;
            brokerZip = _BrokerZip;
            coverletterid = _CoverletterId;
            brokerState = _BrokerState;
        }

    }
}
