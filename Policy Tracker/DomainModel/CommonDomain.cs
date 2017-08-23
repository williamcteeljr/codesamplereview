using PolicyTracker.DomainModel.Framework;
using System.ComponentModel.DataAnnotations;

namespace PolicyTracker.DomainModel.Common
{
    public class InsuredType : StringEnum
    {
        public static InsuredType INDV = new InsuredType() { Value = "Individual", DisplayText = "Individual" };
        public static InsuredType CORP = new InsuredType() { Value = "Corporation", DisplayText = "Corporation" };

        private InsuredType(string val, string text) : base(val, text) { }
        private InsuredType() { }
    }

    public class BrokerCommissionStatus : StringEnum
    {
        public static BrokerCommissionStatus BASE = new BrokerCommissionStatus() { Value = "Base", DisplayText = "Base" };
        public static BrokerCommissionStatus PLATINUM = new BrokerCommissionStatus() { Value = "Platinum", DisplayText = "Platinum" };

        private BrokerCommissionStatus(string val, string text) : base(val, text) { }
        private BrokerCommissionStatus() { }
    }

    public class PaymentTerm : StringEnum
    {
        public static PaymentTerm MONTHLY = new PaymentTerm() { Value = "Monthly", DisplayText = "Monthly" };
        public static PaymentTerm BIMONTHLY = new PaymentTerm() { Value = "BiMonthly", DisplayText = "Bi Monthly" };
        public static PaymentTerm QUARTERLY = new PaymentTerm() { Value = "Quarterly", DisplayText = "Quarterly" };

        private PaymentTerm(string val, string text) : base(val, text) { }
        private PaymentTerm() { }
    }

    public class Month : StringEnum
    {
        public static Month JANUARY = new Month() { Value = "1", DisplayText = "January" };
        public static Month FEBRUARY = new Month() { Value = "2", DisplayText = "February" };
        public static Month MARCH = new Month() { Value = "3", DisplayText = "March" };
        public static Month APRIL = new Month() { Value = "4", DisplayText = "April" };
        public static Month MAY = new Month() { Value = "5", DisplayText = "May" };
        public static Month JUNE = new Month() { Value = "6", DisplayText = "June" };
        public static Month JULY = new Month() { Value = "7", DisplayText = "July" };
        public static Month AUGUST = new Month() { Value = "8", DisplayText = "August" };
        public static Month SEPTEMBER = new Month() { Value = "9", DisplayText = "September" };
        public static Month OCTOBER = new Month() { Value = "10", DisplayText = "October" };
        public static Month NOVEMBER = new Month() { Value = "11", DisplayText = "November" };
        public static Month DECEMBER = new Month() { Value = "12", DisplayText = "December" };

        private Month(string val, string text) : base(val, text) { }
        private Month() { }
    }

    public class Branch : StringEnum
    {
        public static Branch ATL = new Branch() { Value = "ATL", DisplayText = "Atlanta" };
        public static Branch NYC = new Branch() { Value = "NYC", DisplayText = "New York" };
        public static Branch CHI = new Branch() { Value = "CHI", DisplayText = "Chicago" };
        public static Branch DAL = new Branch() { Value = "DAL", DisplayText = "Dallas" };
        public static Branch SEA = new Branch() { Value = "SEA", DisplayText = "Seattle" };

        private Branch(string val, string text) : base(val, text) { }
        private Branch() { }
    }

    public class DaysShowing : StringEnum
    {
        public static DaysShowing ALL = new DaysShowing() { Value = "", DisplayText = "All" };
        public static DaysShowing THIRTY = new DaysShowing() { Value = "30", DisplayText = "30 Days" };
        public static DaysShowing SIXTY = new DaysShowing() { Value = "60", DisplayText = "60 Days" };
        public static DaysShowing NINTY = new DaysShowing() { Value = "90", DisplayText = "90 Days" };

        private DaysShowing(string val, string text) : base(val, text) { }
        private DaysShowing() { }
    }

    public class PhoneNumber : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            return ValidationResult.Success;
        }
    }

    public class Market : BaseEntity
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public string CompanyName { get; set; }
    }

    public class Reason : BaseEntity
    {
        public int Id { get; set; }
        public string ActionType { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class UserFilter : BaseEntity
    {
        public int FilterId { get; set; }
        public int UserId { get; set; }
        [Required]
        public string Name { get; set; }
        public string Expression { get; set; }
        public int MonthFilter { get; set; }
        public int MonthRangeFilter { get; set; }
    }
}
