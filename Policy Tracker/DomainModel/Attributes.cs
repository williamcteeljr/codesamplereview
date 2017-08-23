using PolicyTracker.DomainModel.Policy;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.Attributes
{
    public class StatusRequiredProperties : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = (Risk)value;

            if (model.Status == RiskStatus.ISSUED.Value)
            {
            }

            return ValidationResult.Success;
        }
    }
}
