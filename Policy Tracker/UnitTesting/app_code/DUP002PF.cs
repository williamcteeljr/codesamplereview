using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTesting.app_code
{
    /// <summary>
    /// Policy / Risk Record
    /// </summary>
    class DUP002PF
    {
        public string company { get; set; }
        public string symbol { get; set; }
        public string policyNumber { get; set; }
        public string suffix { get; set; }
        public char policyStatus { get; set; }
        public int issueDate { get; set; }
        public string lastTrans { get; set; }
        public int transDate { get; set; }
        public string accountName { get; set; }
        public string addressLine2 { get; set; }
        public string addressLine3 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip1 { get; set; }
        public string zip2 { get; set; }
        public char pamLead { get; set; }
        public string coinCode { get; set; }
        public string installPayPlan { get; set; }
        public string prodCode { get; set; }
        public int cancelDate { get; set; }
        public int reinstateDate { get; set; }
        public int reinProcessDate { get; set; }
        public char branch { get; set; }
        public int effYear { get; set; }
        public int effMonth { get; set; }
        public int effDay { get; set; }
        public int expYear { get; set; }
        public int expMonth { get; set; }
        public int expDay { get; set; }
        public int term { get; set; }
        public string billCode { get; set; }
        public char auditFreq { get; set; }
        public string underwritter { get; set; }
        public char doNotRenew { get; set; }
        public char homeOfficeIndicator { get; set; }
        public int prodComm { get; set; }
        public int pamComm { get; set; }
        public int coComm { get; set; }
        public char treatyCod { get; set; }
        public int treatyYear { get; set; }
        public string aggLimit { get; set; }
        public string aggDeduct { get; set; }
        public int minimumPremium { get; set; }
        public int depositPremium { get; set; }
        public string renewalOfSymbol { get; set; }
        public int renewalOfPolNum { get; set; }
        public int renewalOfSuffix { get; set; }
        public string occDeduct { get; set; }
        public string secondProdCode { get; set; }
        public int secondProdComm { get; set; }
        public int hullValue { get; set; }
        public string originalProdCode { get; set; }
        public int totalNumAircraft { get; set; }
        public string nameLine { get; set; }
        public int annualPrem { get; set; }
        public char canCode { get; set; }
        public int noCamtDue { get; set; }
    }
}
