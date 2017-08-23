using PolicyTracker.DomainModel.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolicyTracker.DomainModel.PostNotice
{
    public class WCGlobalPostingNoticesDomain
    {
        public String version { get; set; }

        //Primary Contact
        public WCPnPrimaryContactDomain primaryContact { get; set; }

        //Insured
        private List<WCPnInsuredDomain> insured = new List<WCPnInsuredDomain>();
        public List<WCPnInsuredDomain> Insured { get { return insured; } set { insured = value; } }

    }
}
