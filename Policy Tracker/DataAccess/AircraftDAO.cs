using PolicyTracker.DomainModel;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.Platform.UOW;
using System.Collections.Generic;
using System.Text;

namespace PolicyTracker.DataAccess.AircraftDataAccess
{
    public class AircraftLookupDAO : BaseDAO<AircraftLookup>
    {
        public AircraftLookupDAO()
            : base("AircraftLookup", "UW_Base_App", defaultOrderFilter: new OrderFilter("CreatedDate", OrderFilter.Comparator.Descending))
        {
            AddColumnMapping("RiskId", "QuoteId");
            AddColumnMapping("FAANo", "FAANo", true);
            AddColumnMapping("Year", "YEAR");
            AddColumnMapping("Make", "Make");
            AddColumnMapping("ModelName", "ModelName");
            AddColumnMapping("PurposeOfUse", "PurposeOfUse");
            AddColumnMapping("AirportID", "AirportID");
            AddColumnMapping("ProductLine", "ProductLine");
            AddColumnMapping("EffectiveDate", "EffectiveDate");
            AddColumnMapping("EffectiveMonth", "EffectiveMonth");
            AddColumnMapping("EffectiveYear", "EffectiveYear");
            AddColumnMapping("EngineType", "EngineType");
            AddColumnMapping("NamedInsured", "NamedInsured");
            AddColumnMapping("IsRenewal", "IsRenewal");
            AddColumnMapping("Value", "Value");
            AddColumnMapping("HullPrem", "HullPrem");
            AddColumnMapping("LiabPrem", "LiabPrem");
            AddColumnMapping("HullTriaPrem", "HullTriaPrem");
            AddColumnMapping("HullWarPrem", "HullWarPrem");
            AddColumnMapping("Limit", "Limit");
            AddColumnMapping("isCSL", "isCSL");
            AddColumnMapping("AnnualPrem", "AnnualPrem");
            AddColumnMapping("AgencyName", "AgencyName");
            AddColumnMapping("CreatedDate", "CreatedDate");
            AddColumnMapping("Branch", "Branch");
            AddColumnMapping("Status", "Status");
            AddColumnMapping("UW", "UW");
            Initialize();
        }
    }

    public class AircraftReferenceDAO : BaseDAO<AircraftReference>
    {
        public AircraftReferenceDAO()
            : base("ACFTREF", "BlueBook", defaultOrderFilter: new OrderFilter("CODE", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("CODE", "CODE");
            AddColumnMapping("MFR", "MFR");
            AddColumnMapping("MODEL", "MODEL");
            AddColumnMapping("TYPEACFT", "TYPE-ACFT");
            AddColumnMapping("ACCAT", "AC-CAT");
            AddColumnMapping("TYPEENG", "TYPE-ENG");
            AddColumnMapping("BUILDCERTIND", "BUILD-CERT-IND");
            AddColumnMapping("NOENG", "NO-ENG");
            AddColumnMapping("ACWEIGHT", "AC-WEIGHT");
            AddColumnMapping("NOSEATS", "NO-SEATS");
            AddColumnMapping("ID", "ID");
            AddColumnMapping("SPEED", "SPEED");
            AddColumnMapping("FULLNAME", "FULLNAME");
            AddColumnMapping("AS400LinkCode", "AS400LinkCode");
            AddColumnMapping("intClassCodeID", "intClassCodeID");
            AddColumnMapping("HullRateSurcharge", "HullRateSurcharge");
            AddColumnMapping("LiabilityRateSurcharge", "LiabilityRateSurcharge");
            AddColumnMapping("AltTWClassCodeID", "AltTWClassCodeID");
            AddColumnMapping("AltTWAS400LinkCode", "AltTWAS400LinkCode");
            AddColumnMapping("AltSEAClassCodeID", "AltSEAClassCodeID");
            AddColumnMapping("IsHullBody", "IsHullBody");
            AddColumnMapping("IsDefaultToOverride", "IsDefaultToOverride");
            AddColumnMapping("IsExperimental", "IsExperimental");
            AddColumnMapping("FAA", "FAA");
            Initialize();
        }
    }

    public class EngineTypeDAO : BaseDAO<EngineType>
    {
        public EngineTypeDAO()
            : base("AVCEngineTypes", "UW_Base_App", defaultOrderFilter: new OrderFilter("EngineTypeDesc", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("EngineTypeCode", "EngineTypeCode", true);
            AddColumnMapping("EngineTypeDesc", "EngineTypeDesc");
            Initialize();
        }
    }

    public class AircraftYearsDAO : BaseDAO<AircraftYear>
    {
        public AircraftYearsDAO()
            : base("AircraftYears", "BlueBook", defaultOrderFilter: new OrderFilter("Suffix", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("Suffix", "Year");
            AddColumnMapping("Year", "Exact_Year");
            Initialize();
        }

        public IEnumerable<AircraftYear> GetYears(UnitOfWork uow)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT DISTINCT [Exact_Year] Year, [aircraftYears].[Year] Suffix");
            sql.Append(" FROM " + SourceDB + ".dbo.[aircraftYears]");
            sql.Append(" INNER JOIN " + SourceDB + ".dbo.[AircraftValue] on [AircraftValue].[Year] = [aircraftYears].[Year]");
            var results = Query<AircraftYear>(uow, sql.ToString());
            return results;
        }
    }

    public class AircraftCrossRefDAO : BaseDAO<AircraftCrossRef>
    {
        public AircraftCrossRefDAO()
            : base("AircraftYears", "BlueBook", defaultOrderFilter: new OrderFilter("Suffix", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("ModelID", "ModelID", true);
            AddColumnMapping("ModelName", "ModelName");
            AddColumnMapping("PAMModelName", "PAMModelName");
            AddColumnMapping("MfgID", "MfgID");
            AddColumnMapping("MfgOrigName", "MfgOrigName");
            AddColumnMapping("PamMfg", "PamMfg");
            AddColumnMapping("Year", "Year");
            AddColumnMapping("AS400Code", "AS400Code");
            AddColumnMapping("Make", "Make");
            AddColumnMapping("PAMMake", "PAMMake");
            Initialize();
        }

        public IEnumerable<AircraftCrossRef> GetMake(UnitOfWork uow, string year)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT DISTINCT [PAMMake], [Make] FROM " + SourceDB + ".dbo.[AircraftXRefLookup] WHERE ([Year] = @Year)");
            var results = Query<AircraftCrossRef>(uow, sql.ToString(), new { Year = year });
            return results;
        }

        public IEnumerable<AircraftCrossRef> GetModel(UnitOfWork uow, string year, string make)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT [ModelID], [ModelName], [PAMModelName] FROM " + SourceDB + ".dbo.[AircraftXRefLookup] WHERE (([Make] = @Make) AND ([Year] = @Year))");
            var results = Query<AircraftCrossRef>(uow, sql.ToString(), new { Year = year, Make = make });
            return results;
        }
    }

    public class AirportsDAO : BaseDAO<Airport>
    {
        public AirportsDAO()
            : base("Airports", "BlueBook", defaultOrderFilter: new OrderFilter("ID", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("ID", "ID");
            AddColumnMapping("AirportName", "AirportName");
            AddColumnMapping("Address", "Address");
            AddColumnMapping("City", "City");
            AddColumnMapping("County", "County");
            AddColumnMapping("State", "State");
            AddColumnMapping("Zipcode", "Zipcode");
            AddColumnMapping("AirportID", "AirportID");
            AddColumnMapping("TypeOwner", "TypeOwner");
            AddColumnMapping("AirportUse", "AirportUse");
            AddColumnMapping("TypeAirport", "TypeAirport");
            AddColumnMapping("RunwayCount", "RunwayCount");
            AddColumnMapping("HelipadCount", "HelipadCount");
            AddColumnMapping("OtherRunways", "OtherRunways");
            AddColumnMapping("TotalRunways", "TotalRunways");
            AddColumnMapping("LongRWLength", "LongRWLength");
            AddColumnMapping("LongRWWidth", "LongRWWidth");
            AddColumnMapping("LongRWSurf", "LongRWSurf");
            AddColumnMapping("CommOps", "CommOps");
            AddColumnMapping("AirTaxiOps", "AirTaxiOps");
            AddColumnMapping("GALocalOps", "GALocalOps");
            AddColumnMapping("GAItinerantOps", "GAItinerantOps");
            AddColumnMapping("MilitaryOps", "MilitaryOps");
            AddColumnMapping("TotalOps", "TotalOps");
            AddColumnMapping("Latitude", "Latitude");
            AddColumnMapping("Longitude", "Longitude");
            AddColumnMapping("Elevation", "Elevation");
            Initialize();
        }
    }

    public class TailNumberDAO : BaseDAO<TailNumber>
    {
        public TailNumberDAO()
            : base("NNumbers", "BlueBook", defaultOrderFilter: new OrderFilter("Id", OrderFilter.Comparator.Ascending))
        {
            AddColumnMapping("Id", "ID", true, true);
            AddColumnMapping("FAA_Number", "N-NUMBER");
            AddColumnMapping("SERIAL_NUMBER", "SERIAL NUMBER");
            AddColumnMapping("MFR_MDL_CODE", "MFR MDL CODE");
            AddColumnMapping("ENG_MFR_MDL", "ENG MFR MDL");
            AddColumnMapping("YEAR_MFR", "YEAR MFR");
            AddColumnMapping("TYPE_REGISTRANT", "TYPE REGISTRANT");
            AddColumnMapping("NAME", "NAME");
            AddColumnMapping("STREET", "STREET");
            AddColumnMapping("STREET2", "STREET2");
            AddColumnMapping("City", "CITY");
            AddColumnMapping("State", "STATE");
            AddColumnMapping("Zip", "ZIP CODE");
            AddColumnMapping("REGION", "REGION");
            AddColumnMapping("COUNTY", "COUNTY");
            AddColumnMapping("COUNTRY", "COUNTRY");
            AddColumnMapping("LAST_ACTION_DATE", "LAST ACTION DATE");
            AddColumnMapping("CERT_ISSUE_DATE", "CERT ISSUE DATE");
            AddColumnMapping("CERTIFICATION", "CERTIFICATION");
            AddColumnMapping("TYPE_AIRCRAFT", "TYPE AIRCRAFT");
            AddColumnMapping("TYPE_ENGINE", "TYPE ENGINE");
            AddColumnMapping("STATUS_CODE", "STATUS CODE");
            AddColumnMapping("MODE_S_CODE", "MODE S CODE");
            AddColumnMapping("FRACT_OWNER", "FRACT OWNER");
            AddColumnMapping("AIR_WORTH_DATE", "AIR WORTH DATE");
            AddColumnMapping("OtherName1", "OTHER NAMES(1)");
            AddColumnMapping("OtherName2", "OTHER NAMES(2)");
            AddColumnMapping("OtherName3", "OTHER NAMES(3)");
            AddColumnMapping("OtherName4", "OTHER NAMES(4)");
            AddColumnMapping("OtherName5", "OTHER NAMES(5)");
            AddColumnMapping("EXPIRATION_DATE", "EXPIRATION DATE");
            AddColumnMapping("UNIQUE_ID", "UNIQUE ID");
            AddColumnMapping("KIT_MFR", "KIT MFR");
            AddColumnMapping("KIT_MODEL", "KIT MODEL");
            AddColumnMapping("MODE_S_CODE_HEX", "MODE S CODE HEX");
            Initialize();
        }
    }
}
