using PolicyTracker.DomainModel.Framework;
using System;

namespace PolicyTracker.DomainModel
{
    public class TailNumber : BaseEntity
    {
        public int Id { get; set; }
        public string FAA_Number { get; set; }
        public string SERIAL_NUMBER { get; set; }
        public string MFR_MDL_CODE { get; set; }
        public string ENG_MFR_MDL { get; set; }
        public string YEAR_MFR { get; set; }
        public int TYPE_REGISTRANT { get; set; }
        public string NAME { get; set; }
        public string STREET { get; set; }
        public string STREET2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public int Zip { get; set; }
        public string REGION { get; set; }
        public int COUNTY { get; set; }
        public string COUNTRY { get; set; }
        public int LAST_ACTION_DATE { get; set; }
        public int CERT_ISSUE_DATE { get; set; }
        public string CERTIFICATION { get; set; }
        public int TYPE_AIRCRAFT { get; set; }
        public int TYPE_ENGINE { get; set; }
        public string STATUS_CODE { get; set; }
        public int MODE_S_CODE { get; set; }
        public string FRACT_OWNER { get; set; }
        public string AIR_WORTH_DATE { get; set; }
        public string OtherName1 { get; set; }
        public string OtherName2 { get; set; }
        public string OtherName3 { get; set; }
        public string OtherName4 { get; set; }
        public string OtherName5 { get; set; }
        public int EXPIRATION_DATE { get; set; }
        public int UNIQUE_ID { get; set; }
        public string KIT_MFR { get; set; }
        public string KIT_MODEL { get; set; }
        public string MODE_S_CODE_HEX { get; set; }
    }

    public class AircraftYear : BaseEntity
    {
        public string Suffix { get; set; }
        public string Year { get; set; }
    }

    public class EngineType : BaseEntity
    {
        public string EngineTypeCode { get; set; }
        public string EngineTypeDesc { get; set; }
    }

    public class AircraftLookup : BaseEntity
    {
        public int RiskId { get; set; }
        public string FAANo { get; set; }
        public string Year { get; set; }
        public string Make { get; set; }
        public string ModelName { get; set; }
        public string PurposeOfUse { get; set; }
        public string AirportID { get; set; }
        public string ProductLine { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public int EffectiveMonth { get; set; }
        public int EffectiveYear { get; set; }
        public string EngineType { get; set; }
        public string NamedInsured { get; set; }
        public bool IsRenewal { get; set; }
        public float Value { get; set; }
        public float HullPrem { get; set; }
        public float LiabPrem { get; set; }
        public float HullTriaPrem { get; set; }
        public float HullWarPrem { get; set; }
        public float Limit { get; set; }
        public bool isCSL { get; set; }
        public float AnnualPrem { get; set; }
        public string AgencyName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string Branch { get; set; }
        public string Status { get; set; }
        public string UW { get; set; }
        public float HullRate
        {
            get { return (Value != 0) ? Convert.ToSingle(HullPrem) / (Convert.ToSingle(Value / 100)) : 0; }
        }
        public int YearNumber
        {
            get { return (String.IsNullOrEmpty(Year)) ? 0 : Convert.ToInt16(Year); }
        }
        public string DisplayEffectiveDate
        {
            get { return EffectiveDate.Value.ToShortDateString(); }
        }
    }
        
    public class AircraftReference : BaseEntity
    {
        public string CODE { get; set; }
        public string MFR { get; set; }
        public string MODEL { get; set; }
        public int TYPEACFT { get; set; }
        public int ACCAT { get; set; }
        public int TYPEENG { get; set; }
        public int BUILDCERTIND { get; set; }
        public int NOENG { get; set; }
        public string ACWEIGHT { get; set; }
        public int NOSEATS { get; set; }
        public int ID { get; set; }
        public int SPEED { get; set; }
        public string FULLNAME { get; set; }
        public string AS400LinkCode { get; set; }
        public int intClassCodeID { get; set; }
        public string HullRateSurcharge { get; set; }
        public string LiabilityRateSurcharge { get; set; }
        public int AltTWClassCodeID { get; set; }
        public string AltTWAS400LinkCode { get; set; }
        public int AltSEAClassCodeID { get; set; }
        public bool IsHullBody { get; set; }
        public bool IsDefaultToOverride { get; set; }
        public bool IsExperimental { get; set; }
        public string FAA { get; set; }
    }

    public class AircraftCrossRef : BaseEntity
    {
        public long ModelID { get; set; }
        public string ModelName { get; set; }
        public string PAMModelName { get; set; }
        public string MfgID { get; set; }
        public string MfgOrigName { get; set; }
        public string PamMfg { get; set; }
        public string Year { get; set; }
        public string AS400Code { get; set; }
        public string Make { get; set; }
        public string PAMMake { get; set; }
    }

    public class Airport : BaseEntity
    {
        public int ID { get; set; }
        public string AirportName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string State { get; set; }
        public string Zipcode { get; set; }
        public string AirportID { get; set; }
        public string TypeOwner { get; set; }
        public string AirportUse { get; set; }
        public string TypeAirport { get; set; }
        public float RunwayCount { get; set; }
        public float HelipadCount { get; set; }
        public float OtherRunways { get; set; }
        public float TotalRunways { get; set; }
        public float LongRWLength { get; set; }
        public float LongRWWidth { get; set; }
        public string LongRWSurf { get; set; }
        public float CommOps { get; set; }
        public float AirTaxiOps { get; set; }
        public float GALocalOps { get; set; }
        public float GAItinerantOps { get; set; }
        public float MilitaryOps { get; set; }
        public float TotalOps { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public float Elevation { get; set; }
    }
}
