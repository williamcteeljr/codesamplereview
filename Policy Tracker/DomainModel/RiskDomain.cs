using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.Risks
{
    public class BaseRisk 
    {
        public int RiskId { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }

    public class WorkersCompRisk : BaseRisk
    {
        public int Payroll { get; set; }
    }

    public class AgricultureRisk : BaseRisk
    {
        public int ChemicalLimit { get; set; }
    }
}
