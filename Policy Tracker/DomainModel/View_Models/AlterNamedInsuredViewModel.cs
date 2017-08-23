using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DomainModel.View_Models
{
    public class AlterNamedInsuredViewModel
    {
        public int RiskId { get; set; }
        public int NewNamedInsuredId { get; set; }
        /// <summary>
        /// Indicates if the named insured Id previously assigned to the risk should be deleted
        /// </summary>
        public bool ShouldDeleteOldNamedInsured { get; set; }
    }
}