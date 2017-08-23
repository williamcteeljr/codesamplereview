using PolicyTracker.DomainModel.Framework;
using System;
using System.Collections.Generic;

namespace PolicyTracker.DomainModel.Reports
{
    public class NewBusinessFlow : BaseEntity
    {
        public string Branch { get; set; }
        public string ProductLine { get; set; }
        public string UW { get; set; }
        public int UnderwriterId { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public int EffectiveMonth { get; set; }
        public int EffectiveYear { get; set; }
        public int EffectiveDay { get; set; }
        public string RiskStatus { get; set; }
        public int Total { get; set; }
        public decimal TotalPremium { get; set; }

        public int LowYearSubmissions { get; set; }
        public int UpperYearSubmissions { get; set; }
        public int SubmissionVariance { get; set; }

        public int LowYearQuotes { get; set; }
        public int UpperYearQuotes { get; set; }
        public int QuoteVariance { get; set; }

        public int LowYearPolicies { get; set; }
        public int UpperYearPolicies { get; set; }
        public int PolicyVariance { get; set; }
        
        public decimal LowYearQuoteRatio { get; set; }
        public decimal UpperYearQuoteRatio { get; set; }
        public decimal QuoteRatioVariance { get; set; }
        
        public decimal LowYearHitRatio { get; set; }
        public decimal UpperYearHitRatio { get; set; }
        public decimal HitRatioVariance { get; set; }

        public decimal LowYearWrittenRatio { get; set; }
        public decimal UpperYearWrittenRatio { get; set; }
        public decimal WrittenRatioVariance { get; set; }

        public decimal LowYearQuotedPremium { get; set; }
        public decimal UpperYearQuotedPremium { get; set; }
        public decimal QuotedPremiumVariance { get; set; }

        public decimal LowYearWrittenPremium { get; set; }
        public decimal UpperYearWrittenPremium { get; set; }
        public decimal WrittenPremiumVariance { get; set; }
    }

    public class PredictivePremiumDataPoint : BaseEntity
    {
        public string Branch { get; set; }
        public string ProductLine { get; set; }
        public string Status { get; set; }
        public int UnderwriterId { get; set; }
        public string UW { get; set; }
        public bool IsRenewal { get; set; }
        public int EffectiveMonth { get; set; }
        public int EffectiveYear { get; set; }
        public int ExpirationMonth { get; set; }
        public int ExpirationYear { get; set; }
        public decimal Premium { get; set; }
        public bool Renewed { get; set; }
        public decimal RenewedPremium { get; set; }

        public decimal NBQuotedPremium { get; set; }
        public int TotalNBQuoted { get; set; }
        public decimal NBWrittenPremium { get; set; }
        public int TotalWritten { get; set; }
        public decimal PremiumAvailableForRenewal { get; set; }
        public int TotalAvailable { get; set; }
        public decimal PremiumRenewed { get; set; }
        public int TotalRenewed { get; set; }
        public decimal PredictedPremium { get; set; }
    }

    public class ProductLineMonthlyDetail
    {
        //New Business
        public int NBTotal { get; set; }
        public int NBBoundTotal { get; set; }
        public decimal NBTotalBookedPremium { get; set; }
        public decimal NBTotalWrittenPremium { get; set; }
        public int NBTotalUnsuccessful { get; set; }
        public decimal NBTotalUnsuccessfulPremium { get; set; }
        public int NBQuotedTotal { get; set; }
        public decimal NBOutstandingPremium { get; set; }
        public int NBDeclinedTotal { get; set; }
        public decimal NBDeclinedPremium { get; set; }
        public decimal NBHitRatio { get; set; }
        public decimal NBHitRatioPremium { get; set; }
        public decimal NewBusinessAuditAmount { get; set; }

        //Renewals
        public int RETotal { get; set; }
        public int REBoundTotal { get; set; }
        public decimal RETotalBookedPremium { get; set; }
        public decimal RETotalWrittenPremium { get; set; }
        public int RETotalUnsuccessful { get; set; }
        public decimal RETotalUnsuccessfulPremium { get; set; }
        public int REQuotedTotal { get; set; }
        public decimal REOutstandingPremium { get; set; }
        public int REDeclinedTotal { get; set; }
        public decimal REDeclinedPremium { get; set; }
        public decimal REExpiringBookedPremium { get; set; }
        public decimal REExpiringWrittenPremium { get; set; }
        public decimal REExpiringPremiumLost { get; set; }
        public decimal REPolicyRetentionRatio { get; set; }
        public decimal REPremiumRetentionRatio { get; set; }
        public decimal REHitRatio { get; set; }
        public decimal REHitRatioPremium { get; set; }
        public decimal RenewalAuditAmount { get; set; }
        public decimal RenewalNetRateChange { get; set; }

        //Summary
        public List<dynamic> priorOutstanding { get; set; }
        public decimal TotalBooked { get; set; }
        public int TotalInstallments { get; set; }
        public int PriorInstallments { get; set; }
        public decimal TotalAuditAndEndorsementAmount { get; set; }
        public decimal TotalBookedPlusInstallments { get; set; }
        public decimal Budget { get; set; }
        public decimal OverUnderPercent { get; set; }
        public decimal YTDActual { get; set; }
        public decimal YTDBudget { get; set; }
        public decimal YTDOverUnderBudgetPercent { get; set; }
    }

    public class StatChart
    {
        public string Timeframe { get; set; }
        public decimal HitRatio { get; set; }
        public int NumberWritten { get; set; }
        public decimal WrittenPercent { get; set; }
        public int NumberQuoted { get; set; }
        public decimal QuotedPercent { get; set; }
        public int NumberSubmissions { get; set; }

        public decimal NewBusinessAmount { get; set; }
        public int NumberNewBusiness { get; set; }
        public decimal AccountRetentionPercent { get; set; }
        public decimal GrowthAmount { get; set; }
        public decimal GrowthPercent { get; set; }
        public int TotalInforce { get; set; }
    }
}
