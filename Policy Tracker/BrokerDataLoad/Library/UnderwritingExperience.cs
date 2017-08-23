using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaimsLoad.Library
{
    class UnderwritingExperience
    {
        public int quoteId { get; set; }
        public string type { get; set; }
        public string line { get; set; }
        public string policyEffectiveDate { get; set; }
        public string prefix { get; set; }
        public string policyNumber { get; set; }
        public string suffix { get; set; }
        public string name { get; set; }
        public int standard { get; set; }
        public int earned { get; set; }
        public int written { get; set; }
        public int claimCount { get; set; }
        public int dividend { get; set; }
        public int pdExpenses { get; set; }
        public int pdLoss { get; set; }
        public int incLoss { get; set; }
        public decimal lossRatio { get; set; }
        public int count { get; set; }
    }
}
