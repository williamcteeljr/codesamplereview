//using Microsoft.VisualBasic.FileIO;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using PolicyTracker.BusinessServices;
//using PolicyTracker.DataAccess;
//using PolicyTracker.DataAccess.Other;
//using PolicyTracker.DataAccess.Policy;
//using PolicyTracker.DataAccess.Security;
//using PolicyTracker.DomainModel.Common;
//using PolicyTracker.DomainModel.Framework;
//using PolicyTracker.DomainModel.Policy;
//using PolicyTracker.Platform.Logging;
//using PolicyTracker.Platform.UOW;
//using System;
//using System.Collections.Generic;
//using System.Data.OleDb;
//using System.Dynamic;
//using System.Globalization;
//using System.Linq;
//using System.Text;
//using System.Text.RegularExpressions;

//namespace UnitTesting
//{
//    [TestClass]
//    public class DataImport : NonTransactionalUnitTest
//    {
//        #region Class Setup/Test Initialization
//        [ClassInitialize]
//        public static void Setup(TestContext context)
//        {
//            Login();
//        }

//        [ClassCleanup]
//        public static void TearDown()
//        {
//            Logout();
//        }
//        #endregion

//        [TestMethod]
//        public void ImportBrokerAssignments()
//        {
//            var assignmentFiles = new List<dynamic> { 
//                new { FilePath = @"C:\Testing\broker-assignment\wcBrokerAssignment.csv", Start = 5, End = 5 }
//            };

//            foreach (var file in assignmentFiles)
//            {
//                ServiceLocator.AppManagementService.BrokerAssignmentImport(file.FilePath, file.Start, file.End);
//            }
//        }

//        /// <summary>
//        /// Import Prospect Records from the AS 400 Prospect database.
//        /// </summary>
//        [TestMethod]
//        public void ImportProspects()
//        {
//            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
//            var clients = new List<NamedInsured>();
//            var risks = new List<Risk>();
//            var rowCount = 0;
//            bool specificRow = true;
//            var row = 3610 - 1;

//            TextFieldParser parser = new TextFieldParser(@"C:\Testing\Last90Prospect.csv");
//            parser.TextFieldType = FieldType.Delimited;
//            parser.SetDelimiters(",");


//            while (!parser.EndOfData)
//            {
//                string[] fields = parser.ReadFields();

//                //Processing row
//                if (rowCount > 0)
//                {
//                    if (!specificRow || (specificRow && rowCount == row))
//                    {
//                        try
//                        {
//                            var risk = ServiceLocator.RiskService.GetNewRisk();
//                            risk.PremiumInfo = new RiskPremiumInfo();
//                            risk.FirstNote = new RiskNote();

//                            risk.CreatedDate = DateTime.ParseExact(fields[1], "yyyyMMdd", CultureInfo.InvariantCulture);
//                            risk.EffectiveDate = DateTime.ParseExact(fields[1], "yyyyMMdd", CultureInfo.InvariantCulture);
//                            risk.ExpirationDate = Convert.ToDateTime(risk.EffectiveDate).AddMonths(12);

//                            risk.AgencyID = fields[2];

//                            var name = fields[4].Split(' ');

//                            if (name != null && name.Length > 0)
//                            {
//                                if (name.Length > 2 || name.Length == 1)
//                                {
//                                    var companyName = new StringBuilder();
//                                    for (int i = 0; i < name.Length; i++)
//                                    {
//                                        if (i > 0) companyName.Append(" ");
//                                        companyName.Append(name[i]);
//                                    }
//                                    risk.NamedInsured.CompanyName = companyName.ToString();
//                                }
//                                else
//                                {
//                                    risk.NamedInsured.FirstName = name[0];
//                                    risk.NamedInsured.LastName = (name.Length > 1 && !String.IsNullOrEmpty(name[1]) ? name[1] : "");
//                                }
//                            }

//                            risk.NamedInsured.Date = DateTime.ParseExact(fields[1], "yyyyMMdd", CultureInfo.InvariantCulture);
//                            //risk.NamedInsured.StreetAddress1 = fields[5];
//                            risk.ImageRightId = fields[6];
//                            risk.NamedInsured.City = fields[7];
//                            risk.NamedInsured.State = fields[8];
//                            risk.NamedInsured.Zip = fields[9];
//                            if (!String.IsNullOrEmpty(fields[10])) risk.NamedInsured.Zip = risk.NamedInsured.Zip + " - " + fields[10];

//                            risk.FAANumbers = new[] { fields[13] };
//                            risk.AirportId = fields[14];
//                            risk.PurposeOfUse = fields[18];

//                            switch (fields[21])
//                            {
//                                case "P":
//                                    risk.Status = RiskStatus.SUBMISSION.DisplayText;
//                                    break;
//                                case "Q":
//                                    risk.Status = RiskStatus.QUOTE.DisplayText;
//                                    break;
//                                case "D":
//                                    risk.Status = RiskStatus.DECLINED.DisplayText;
//                                    break;
//                            }

//                            switch (fields[19])
//                            {
//                                case "A":
//                                    risk.Branch = Branch.ATL.Value;
//                                    break;
//                                case "S":
//                                    risk.Branch = Branch.SEA.Value;
//                                    break;
//                                case "Y":
//                                    risk.Branch = Branch.NYC.Value;
//                                    break;
//                                case "I":
//                                    risk.Branch = Branch.CHI.Value;
//                                    break;
//                                case "D":
//                                    risk.Branch = Branch.DAL.Value;
//                                    break;
//                            }

//                            //risk.PremiumInfo.AnnualizedPremium = Convert.ToDecimal(fields[22]);
//                            risk.AgentId = (Regex.Replace(fields[23], @"[^\d]", String.Empty) == String.Empty) ? 0 : Convert.ToInt32(Regex.Replace(fields[23], @"[^\d]", String.Empty));
//                            if (fields[17].Length > 0) risk.FirstNote.Comment = fields[17];
//                            if (fields[24].Length > 0 && fields[17].Length > 0)
//                            {
//                                if (risk.FirstNote.Comment.Length > 0) risk.FirstNote.Comment += " ";
//                                risk.FirstNote.Comment += fields[24];
//                            }

//                            var uw = DAOFactory.GetDAO<UserEntityDAO>().GetInstance(uow, new PropertyFilter("Initials", fields[25]));
//                            if (uw != null) risk.UnderwriterId = uw.UserId;

//                            var clearer = DAOFactory.GetDAO<UserEntityDAO>().GetInstance(uow, new PropertyFilter("Initials", fields[20]));
//                            if (clearer != null) risk.CreatedById = clearer.UserId;

//                            ServiceLocator.PolicySvc.SaveNewRisk(risk, false);
//                        }
//                        catch (Exception e)
//                        {
//                            if (e is ValidationRulesException)
//                            {
//                                LogManager.Log(LogLevel.ERROR, String.Format("[PROSPECT IMPORT ERROR] - [ProspectKey: {0}] - Record was not imported.", fields[0]));
//                            }
//                            else
//                            {
//                                LogManager.Log(LogLevel.ERROR, String.Format("[PROSPECT IMPORT ERROR] - [ProspectKey: {0}] - Record was not imported.", fields[0]));
//                            }
//                        }
//                    }
//                }

//                rowCount++;
//            }
//            parser.Close();
//        }

//        [TestMethod]
//        public void UpdateProspectEffectiveDates()
//        {
//            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
//            var clients = new List<NamedInsured>();
//            var risks = new List<Risk>();
//            var rowCount = 0;
//            bool specificRow = false;
//            var row = 2 - 1;

//            TextFieldParser parser = new TextFieldParser(@"C:\Testing\Last90Prospect(WithEffective).csv");
//            parser.TextFieldType = FieldType.Delimited;
//            parser.SetDelimiters(",");


//            while (!parser.EndOfData)
//            {
//                string[] fields = parser.ReadFields();

//                //Processing row
//                if (rowCount > 0)
//                {
//                    if ((!specificRow || (specificRow && rowCount == row)) && !String.IsNullOrEmpty(fields[5]))
//                    {
//                        try
//                        {
//                            var length = (fields[5].Length > 30) ? 30 : fields[5].Length;
//                            var imageRightId = fields[5].Substring(0, length);
//                            var risk = ServiceLocator.EntityService.GetInstance<Risk>(new PropertyFilter("ImageRightId", imageRightId));

//                            if (risk != null && fields[25].Length > 0)
//                            {
//                                DateTime effectiveDate = DateTime.ParseExact(fields[25], "yyyyMMdd", CultureInfo.InvariantCulture);
//                                DateTime expirationDate = effectiveDate.AddYears(1);
                                
//                                risk.EffectiveDate = effectiveDate;
//                                risk.ExpirationDate = expirationDate;
//                                DAOFactory.GetDAO<RiskDAO>().Update(uow, risk);
//                            }
//                        }
//                        catch (Exception e)
//                        {
//                            LogManager.Log(LogLevel.ERROR, String.Format("[PROSPECT IMPORT ERROR] - [ProspectKey: {0}] - Record was not imported.", fields[0]));
//                            LogManager.Log(LogLevel.ERROR, e.Message);
//                        }
//                    }
//                }

//                rowCount++;
//            }
//            parser.Close();
//        }

//        /// <summary>
//        /// Import user initials to update the users table. Initials are used during the policy import process from the AS 400 to match up who the assinged underwriter is for the risk.
//        /// </summary>
//        [TestMethod]
//        public void ImportUserInitials()
//        {
//            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
//            var users = DAOFactory.GetDAO<UserEntityDAO>().GetList(uow);


//            TextFieldParser parser = new TextFieldParser(@"C:\Testing\InitialsImport.csv");
//            parser.TextFieldType = FieldType.Delimited;
//            parser.SetDelimiters(",");
//            while (!parser.EndOfData)
//            {
//                string[] fields = parser.ReadFields();
                
//                try
//                {
//                    var user = users.Where(x => x.Name == fields[1]).FirstOrDefault();
                    
//                    if (user != null)
//                    {
//                        user.Initials = fields[0];
//                        DAOFactory.GetDAO<UserEntityDAO>().Update(uow, user);
//                    }
//                }
//                catch (Exception e)
//                {
//                    LogManager.Log(LogLevel.WARN, e.Message);
//                }
//            }
//        }
//    }

//    [TestClass]
//    public class AS400PolicyImporter : NonTransactionalUnitTest
//    {
//        // -- Connection Strings ----------------------------------------------------------------------------------------------------------------------------------------------------------
//        public static string AS400ConnectionString = "Provider=SQLOLEDB;Data Source=172.17.11.17;Integrated Security=SSPI;Initial Catalog=UW_Base_App;User ID=dev;Password=Willowoak23";
//        public static string NewBiConnectionString = "Provider=SQLOLEDB;Data Source=orabiprod;Integrated Security=SSPI;Initial Catalog=FSBI_DW";
//        public static string ProductionBiConnectionString = "Provider=SQLOLEDB;Data Source=orabi;Integrated Security=SSPI;Initial Catalog=PAMWarehouse";
//        // -- End Connection Strings ------------------------------------------------------------------------------------------------------------------------------------------------------

//        public enum BIServers { Prod, New };

//        app_code.LogWriter log = new app_code.LogWriter("");

//        #region Class Setup/Test Initialization
//        [ClassInitialize]
//        public static void Setup(TestContext context)
//        {
//            Login();
//        }

//        [ClassCleanup]
//        public static void TearDown()
//        {
//            Logout();
//        }
//        #endregion

//        [TestMethod]
//        public void AssignPoliciesToUnderwriters()
//        {
//            log.LogWrite("Underwriter Assignment Started");

//            var totalAssignedPolicies = 0;
//            var totalErrors = 0;
//            var totalUnAssignedPolicies = 0;

//            using (OleDbConnection AS400Connection = new OleDbConnection(AS400ConnectionString))
//            {
//                AS400Connection.Open();
//                var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
//                var AS400Policy = new ExpandoObject() as dynamic;

//                var filters = new List<PropertyFilter>()
//                {
//                    new PropertyFilter("UnderwriterId", 0),
//                    new PropertyFilter("PolicyNumber", PropertyFilter.Comparator.NotEquals, "")
//                };

//                var unAssignedRisks = DAOFactory.GetDAO<RiskDAO>().GetTopNList(uow, 1000, filters);

//                foreach (var risk in unAssignedRisks)
//                {
//                    var policySQL = AS400PolicyInstanceSQL(risk.Prefix, risk.PolicyNumber, risk.PolicySuffix);
//                    OleDbDataReader policyReader = AS400DataReader(policySQL, AS400Connection);

//                    while (policyReader.Read())
//                    {
//                        try
//                        {
//                            AS400Policy = new ExpandoObject() as dynamic;

//                            AS400Policy.Company = policyReader.GetString(0);
//                            AS400Policy.Prefix = policyReader.GetString(1);
//                            AS400Policy.PolicyNumber = Convert.ToInt32(policyReader[2]).ToString("000000");
//                            AS400Policy.Suffix = Convert.ToInt32(policyReader[3]).ToString("00");
//                            AS400Policy.Status = Convert.ToChar(policyReader[4]);
//                            AS400Policy.UnderwriterInitials = policyReader[5];

//                            if (!String.IsNullOrEmpty(AS400Policy.UnderwriterInitials))
//                            {
//                                var uw = DAOFactory.GetDAO<UserEntityDAO>().GetInstance(uow, new PropertyFilter("Initials", AS400Policy.UnderwriterInitials));
//                                if (uw != null)
//                                {
//                                    risk.UnderwriterId = uw.UserId;
//                                    DAOFactory.GetDAO<RiskDAO>().Update(uow, risk);
//                                    totalAssignedPolicies++;
//                                }
//                            }
//                        }
//                        catch (Exception ex)
//                        {
//                            totalErrors++;
//                            var errorLog = String.Format("ERROR [Policy #: {0}{1}{2}]: {3} + {4}", AS400Policy.Prefix, AS400Policy.PolicyNumber, AS400Policy.Suffix, ex, ex.StackTrace);
//                            log.LogWrite(errorLog);
//                        }
//                        finally
//                        {
//                            totalUnAssignedPolicies++;
//                        }
//                    }
//                    policyReader.Close();

//                    var logStatement = String.Format("Total Assignments: {0}", totalAssignedPolicies);
//                    log.LogWrite(logStatement);
//                    logStatement = String.Format("Total Errors: {0}", totalErrors);
//                    log.LogWrite(logStatement);
//                    logStatement = String.Format("Total Un-Assigned Policies: {0}", totalUnAssignedPolicies);
//                    log.LogWrite(logStatement);
//                }
//            }
//        }

//        [TestMethod]
//        public void migrate400Data()
//        {
//            //Set which BI Server to query for policy premium data
//            var biServer = BIServers.Prod;
//            // Determines if premiums are pulled from the BI Warehouse or the AS400
//            bool setPremiumsFromBI = false;

//            int lineNo = 0;
//            int totalPolicies = 0;
//            int totalPoliciesWithErrors = 0;
//            int totalPoliciesInserted = 0;
//            bool isFirstAirport = false;

//            try
//            {
//                log.LogWrite("Starting retrieval");

//                using (OleDbConnection AS400Connection = new OleDbConnection(AS400ConnectionString))
//                {
//                    AS400Connection.Open();

//                    var policySQL = DUP002PF();
//                    OleDbDataReader policyReader = AS400DataReader(policySQL, AS400Connection);

//                    app_code.DUP002PF dup002pf = new app_code.DUP002PF();

//                    while (policyReader.Read())
//                    {
//                        PolicyTracker.DomainModel.Other.DataImport dataImport = null;
//                        var uow = UnitOfWorkFactory.GetActiveUnitOfWork();

//                        try
//                        {
//                            string company = policyReader.GetString(0);
//                            string prefix = policyReader.GetString(1);
//                            string policyNumber = Convert.ToInt32(policyReader[2]).ToString("000000");
//                            string suffix = Convert.ToInt32(policyReader[3]).ToString("00");
//                            dataImport = GetDataImport(company, prefix, policyNumber, suffix);
//                            if (dataImport == null || !dataImport.IsImported)
//                            {
//                                dup002pf = new app_code.DUP002PF();
//                                if (dataImport == null) dataImport = InsertImportRecord(company, prefix, policyNumber, suffix);
//                                string strInstallPayPlan = "";
//                                string policyLevelPurposeOfUse = "";
//                                string airportName = "";
//                                string aiportCode = "";
//                                bool isFirstAircraft = true;
//                                isFirstAirport = false;

//                                var risk = ServiceLocator.RiskService.GetNewRisk();
//                                risk.FirstNote = new RiskNote();
//                                risk.Aircrafts = new List<Aircraft>();
//                                risk.PremiumInfo = new RiskPremiumInfo();
//                                risk.CreatedById = -1;//Using to by pass logic for premium

//                                #region Mapping Data Reader Columns to DUP002PF (Risk/Policy) Entity
//                                dup002pf.company = policyReader.GetString(0);
//                                dup002pf.symbol = policyReader.GetString(1);
//                                dup002pf.policyNumber = Convert.ToInt32(policyReader[2]).ToString("000000");
//                                dup002pf.suffix = Convert.ToInt32(policyReader[3]).ToString("00");
//                                dup002pf.policyStatus = Convert.ToChar(policyReader[4]);
//                                dup002pf.issueDate = Convert.ToInt32(policyReader[5]);
//                                dup002pf.lastTrans = policyReader.GetString(6).Trim();
//                                dup002pf.transDate = Convert.ToInt32(policyReader[7]);
//                                dup002pf.accountName = policyReader.GetString(8).Trim();
//                                dup002pf.addressLine2 = policyReader.GetString(9).Trim();
//                                dup002pf.addressLine3 = policyReader.GetString(10).Trim();
//                                dup002pf.city = policyReader.GetString(11).Trim();
//                                dup002pf.state = policyReader.GetString(12);
//                                dup002pf.zip1 = policyReader.GetString(13);
//                                dup002pf.zip2 = policyReader.GetString(14);
//                                dup002pf.coinCode = policyReader.GetString(15);

//                                strInstallPayPlan = policyReader.GetString(16);
//                                if (strInstallPayPlan.Trim() != "")
//                                {
//                                    dup002pf.installPayPlan = strInstallPayPlan;
//                                }

//                                dup002pf.prodCode = policyReader.GetString(17);
//                                dup002pf.cancelDate = Convert.ToInt32(policyReader[18]);
//                                dup002pf.reinstateDate = Convert.ToInt32(policyReader[19]);
//                                dup002pf.reinProcessDate = Convert.ToInt32(policyReader[20]);
//                                dup002pf.branch = Convert.ToChar(policyReader[21]);
//                                dup002pf.effYear = Convert.ToInt32(policyReader[22]);
//                                dup002pf.effMonth = Convert.ToInt32(policyReader[23]);
//                                dup002pf.effDay = Convert.ToInt32(policyReader[24]);
//                                dup002pf.expYear = Convert.ToInt32(policyReader[25]);
//                                dup002pf.expMonth = Convert.ToInt32(policyReader[26]);
//                                dup002pf.expDay = Convert.ToInt32(policyReader[27]);
//                                dup002pf.term = Convert.ToInt32(policyReader[28]);
//                                dup002pf.billCode = policyReader.GetString(29);
//                                dup002pf.auditFreq = Convert.ToChar(policyReader[30]);
//                                dup002pf.underwritter = policyReader.GetString(31);
//                                dup002pf.doNotRenew = Convert.ToChar(policyReader[32]);
//                                dup002pf.homeOfficeIndicator = Convert.ToChar(policyReader[33]);
//                                dup002pf.prodComm = Convert.ToInt32(policyReader[34]);
//                                dup002pf.pamComm = Convert.ToInt32(policyReader[35]);
//                                dup002pf.coComm = Convert.ToInt32(policyReader[36]);
//                                dup002pf.treatyCod = Convert.ToChar(policyReader[37]);
//                                dup002pf.treatyYear = Convert.ToInt32(policyReader[38]);
//                                dup002pf.aggLimit = policyReader.GetString(39);
//                                dup002pf.aggDeduct = policyReader.GetString(40);
//                                dup002pf.minimumPremium = Convert.ToInt32(policyReader[41]);
//                                dup002pf.depositPremium = Convert.ToInt32(policyReader[42]);
//                                dup002pf.renewalOfSymbol = policyReader.GetString(43);
//                                dup002pf.renewalOfPolNum = Convert.ToInt32(policyReader[44]);
//                                dup002pf.renewalOfSuffix = Convert.ToInt32(policyReader[45]);
//                                dup002pf.occDeduct = policyReader.GetString(46);
//                                dup002pf.secondProdCode = policyReader.GetString(47);
//                                dup002pf.secondProdComm = Convert.ToInt32(policyReader[48]);
//                                dup002pf.hullValue = Convert.ToInt32(policyReader[49]);
//                                dup002pf.totalNumAircraft = Convert.ToInt32(policyReader[50]);
//                                dup002pf.nameLine = policyReader.GetString(51).Trim();
//                                dup002pf.annualPrem = Convert.ToInt32(policyReader[52]);
//                                dup002pf.canCode = Convert.ToChar(policyReader[53]);
//                                dup002pf.noCamtDue = Convert.ToInt32(policyReader[54]);
//                                #endregion

//                                #region Risk
//                                //Name Insured
//                                var name = dup002pf.accountName.Split(' ');

//                                if (name != null && name.Length > 0)
//                                {
//                                    if (name.Length > 2 || name.Length == 1)
//                                    {
//                                        risk.NamedInsured.CompanyName = dup002pf.accountName;
//                                    }
//                                    else if (name.Length == 2)
//                                    {
//                                        risk.NamedInsured.FirstName = name[0];
//                                        risk.NamedInsured.LastName = (!String.IsNullOrEmpty(name[1]) ? name[1] : "");
//                                    }
//                                }
//                                //End Name Insured
//                                var productPrefix = ServiceLocator.EntityService.GetInstance<Product>(new PropertyFilter("Prefix", dup002pf.symbol));
//                                risk.QuoteType = (productPrefix != null) ? productPrefix.QuoteType : String.Empty;
//                                risk.Prefix = dup002pf.symbol.Trim();
//                                risk.PolicyNumber = dup002pf.policyNumber;
//                                risk.PolicySuffix = dup002pf.suffix;
//                                risk.NamedInsured.StreetAddress1 = dup002pf.addressLine2;
//                                risk.NamedInsured.StreetAddress2 = dup002pf.addressLine3;
//                                risk.NamedInsured.City = dup002pf.city;
//                                risk.NamedInsured.State = dup002pf.state;
//                                risk.NamedInsured.Zip = dup002pf.zip1;


//                                if (!String.IsNullOrEmpty(dup002pf.zip2) && dup002pf.zip2 != "    ") risk.NamedInsured.Zip = risk.NamedInsured.Zip + " - " + dup002pf.zip2;
//                                var uw = DAOFactory.GetDAO<UserEntityDAO>().GetInstance(uow, new PropertyFilter("Initials", dup002pf.underwritter));
//                                if (uw != null) risk.UnderwriterId = uw.UserId;

//                                #region Setting Risk Branch
//                                switch (dup002pf.branch.ToString())
//                                {
//                                    case "A":
//                                        risk.Branch = Branch.ATL.Value;
//                                        break;
//                                    case "S":
//                                        risk.Branch = Branch.SEA.Value;
//                                        break;
//                                    case "Y":
//                                        risk.Branch = Branch.NYC.Value;
//                                        break;
//                                    case "I":
//                                        risk.Branch = Branch.CHI.Value;
//                                        break;
//                                    case "D":
//                                        risk.Branch = Branch.DAL.Value;
//                                        break;
//                                }
//                                #endregion

//                                #region Set Policy Status
//                                switch (dup002pf.policyStatus.ToString())
//                                {
//                                    case "A":
//                                        risk.Status = RiskStatus.ISSUED.DisplayText;
//                                        break;
//                                    case "C":
//                                        risk.Status = RiskStatus.CANCELED.DisplayText;
//                                        break;
//                                    case "P":
//                                        risk.Status = RiskStatus.SUBMISSION.DisplayText;
//                                        break;
//                                    case "Q":
//                                        risk.Status = RiskStatus.QUOTE.DisplayText;
//                                        break;
//                                    case "D":
//                                        risk.Status = RiskStatus.DECLINED.DisplayText;
//                                        break;
//                                }
//                                #endregion


//                                risk.AgencyID = dup002pf.prodCode;
//                                risk.EffectiveDate = Convert.ToDateTime(dup002pf.effYear + "-" + dup002pf.effMonth + "-" + dup002pf.effDay);
//                                risk.ExpirationDate = Convert.ToDateTime(dup002pf.expYear + "-" + dup002pf.expMonth + "-" + dup002pf.expDay);
//                                if (dup002pf.prodComm == 0)
//                                    risk.IsNet = true;
//                                risk.Commission = dup002pf.prodComm;
//                                risk.ImageRightId = risk.Prefix + risk.PolicyNumber;

//                                // ======= SAVING AND CREATING RISK RECORD ============== \\
//                                ServiceLocator.PolicySvc.SaveNewRisk(risk, false);

//                                // ======= LOOPING THROUGH AIRCRAFT RECORD(S) TO ADD TO RISK.AIRCRAFT LIST ============== \\
//                                #region Adding Aircraft to Risks
//                                var acSQL = DUP009PF(dup002pf.symbol, dup002pf.policyNumber, dup002pf.suffix);
//                                OleDbDataReader dup009pfReader = AS400DataReader(acSQL, AS400Connection);

//                                app_code.DUP009PF dup009pf = new app_code.DUP009PF();

//                                while (dup009pfReader.Read())
//                                {
//                                    dup009pf = new app_code.DUP009PF();
//                                    int hullPremium = 0;

//                                    dup009pf.faaNum = dup009pfReader.GetString(0);
//                                    dup009pf.planeYear = Convert.ToInt32(dup009pfReader[1]);
//                                    dup009pf.planeMake = dup009pfReader.GetString(2);
//                                    dup009pf.airportNo = dup009pfReader.GetString(3);
//                                    dup009pf.hullValue = Convert.ToInt32(dup009pfReader[4]);
//                                    dup009pf.PlaneUse = dup009pfReader.GetString(5);
//                                    dup009pf.avRiskNumber = dup009pfReader.GetString(6);

//                                    var modelSQL = DEP006PF(dup009pf.planeMake.Trim().ToUpper());
//                                    OleDbDataReader dep006pfReader = AS400DataReader(modelSQL, AS400Connection);

//                                    while (dep006pfReader.Read())
//                                    {
//                                        dup009pf.dep006PlaneModel = dep006pfReader.GetString(0).Trim();
//                                    }
//                                    dep006pfReader.Close();

//                                    #region Coverage
//                                    //DIN
//                                    var coverageSQL = DUP012PFDIN(dup002pf.symbol, dup002pf.policyNumber, dup002pf.suffix, dup009pf.avRiskNumber);
//                                    OleDbDataReader dup012pfReader = AS400DataReader(coverageSQL, AS400Connection);
//                                    app_code.DUP012PF dup012pf = new app_code.DUP012PF();
//                                    while (dup012pfReader.Read())
//                                    {
//                                        dup012pf = new app_code.DUP012PF();

//                                        dup012pf.coverageId = dup012pfReader.GetString(0);
//                                        dup012pf.perLimit = dup012pfReader.GetString(1);
//                                        dup012pf.occLimit = dup012pfReader.GetString(2);
//                                        dup012pf.premium = Convert.ToInt32(dup012pfReader[3]);
//                                    }
//                                    dup012pfReader.Close();

//                                    //F
//                                    var FCoverageSQL = DUP012PFF(dup002pf.symbol, dup002pf.policyNumber, dup002pf.suffix, dup009pf.avRiskNumber);
//                                    OleDbDataReader dup012pffReader = AS400DataReader(FCoverageSQL, AS400Connection);
//                                    app_code.DUP012PF dup012pff = new app_code.DUP012PF();
//                                    while (dup012pffReader.Read())
//                                    {
//                                        dup012pff = new app_code.DUP012PF();

//                                        dup012pff.coverageId = dup012pffReader.GetString(0);
//                                        dup012pff.perLimit = dup012pffReader.GetString(1);
//                                        dup012pff.occLimit = dup012pffReader.GetString(2);
//                                        hullPremium = Convert.ToInt32(dup012pffReader[3]);
//                                    }
//                                    dup012pffReader.Close();
//                                    #endregion

//                                    //Adding Aircraft to Risk List
//                                    risk.Aircrafts.Add(new Aircraft()
//                                    {
//                                        QuoteId = risk.Id,
//                                        FAANo = dup009pf.faaNum,
//                                        Year = dup009pf.planeYear.ToString(),
//                                        //Make = dup009pf.planeMake,
//                                        //AirportID = dup009pf.airportNo,
//                                        Value = dup009pf.hullValue,
//                                        PurposeOfUse = dup009pf.PlaneUse,
//                                        HullPrem = hullPremium

//                                    });

//                                    //Set the use of the first plane to the policy level purpose of use field. This is used for setting the correct product line on AV (Pleasure & Business policies)
//                                    policyLevelPurposeOfUse = dup009pf.PlaneUse;
//                                    isFirstAircraft = false;
//                                }
//                                dup009pfReader.Close();

//                                risk.ProductLine = GetProductLine(dup002pf.symbol, policyLevelPurposeOfUse);

//                                if (dup002pf.symbol.Trim().ToUpper() == "AV")
//                                {
//                                    if (risk.Aircrafts.Count >= 1)
//                                    {
//                                        foreach (Aircraft a in risk.Aircrafts)
//                                        {

//                                            if (a.PurposeOfUse.Trim().ToUpper() == "PB" || a.PurposeOfUse.Trim().ToUpper() == "PF")
//                                            {
//                                            }
//                                            else
//                                            {
                                                
//                                            }
//                                        }

//                                    }
//                                }

//                                #endregion

//                                //if (allPlanesPB == true)
//                                //{
//                                //    ServiceLocator.PolicySvc.DeleteRisk(risk.Id);
//                                //    DAOFactory.GetDAO<NamedInsuredDAO>().Delete(uow, risk.NamedInsured);
//                                //}
//                                //else
//                                //{

//                                //Airport Risk
//                                var airportSQL = DUP010PF(dup002pf.symbol, dup002pf.policyNumber, dup002pf.suffix);
//                                OleDbDataReader dup010pfReader = AS400DataReader(airportSQL, AS400Connection);
//                                while (dup010pfReader.Read())
//                                {
//                                    dup009pf.dup010AirportName = dup010pfReader.GetString(0);
//                                    dup009pf.dup010AiportCode = dup010pfReader.GetString(1);
//                                    if (isFirstAirport == false)
//                                    {
//                                        policyLevelPurposeOfUse = dup009pf.PlaneUse;
//                                        if (dup009pf.dup010AirportName != null)
//                                        {
//                                            airportName = dup009pf.dup010AirportName.Trim();
//                                        }
//                                        if (dup009pf.dup010AiportCode != null)
//                                        {
//                                            aiportCode = dup009pf.dup010AiportCode.Trim();
//                                        }
//                                        isFirstAirport = true;
//                                    }
//                                }
//                                dup010pfReader.Close();

//                                // ======= SAVING AND CREATING AIRCRAFT RECORD(S) ============== \\
//                                if (risk.Aircrafts.Count > 0)
//                                {
//                                    foreach (var aicraft in risk.Aircrafts)
//                                    {
//                                        DAOFactory.GetDAO<AircraftDAO>().Create(uow, aicraft);
//                                    }
//                                }

//                                // ======= UPDATING AND SAVING RISK PREMIUM INFO RECORD ============== \\
//                                // RiskPremiumInfo was already created by the SaveNewRisk Process      \\
//                                risk.PremiumInfo.DepositPremium = dup002pf.depositPremium;

//                                // ======= Setting Risk Premium Values ============== \\
//                                if (!setPremiumsFromBI)
//                                {
//                                    risk.PremiumInfo.AnnualizedPremium = dup002pf.annualPrem;
//                                    risk.PremiumInfo.WrittenPremium = dup002pf.annualPrem;
//                                }
//                                // Update premium with annualized and written premium from the BI
//                                else
//                                {
//                                    var BIConnectionString = (biServer == BIServers.New) ? NewBiConnectionString : ProductionBiConnectionString;
//                                    using (OleDbConnection BIConnection = new OleDbConnection(BIConnectionString))
//                                    {
//                                        BIConnection.Open();
//                                        var premiumSQL = (biServer == BIServers.New) ?
//                                            BINewServerPremiumSQL(dup002pf.symbol, dup002pf.policyNumber, dup002pf.suffix)
//                                            : BIProductionServerPremiumSQL(dup002pf.symbol, dup002pf.policyNumber, dup002pf.suffix);
//                                        OleDbDataReader biReader = BIDataReader(premiumSQL, BIConnection);

//                                        while (biReader.Read())
//                                        {
//                                            risk.PremiumInfo.WrittenPremium = Convert.ToDecimal(biReader.GetDecimal(0));
//                                            risk.PremiumInfo.AnnualizedPremium = Convert.ToDecimal(biReader.GetDecimal(1));
//                                            dataImport.WrittenPremium = Convert.ToInt32(risk.PremiumInfo.WrittenPremium);
//                                            dataImport.AnnualPremium = Convert.ToInt32(risk.PremiumInfo.AnnualizedPremium);
//                                        }

//                                        biReader.Close();
//                                        BIConnection.Close();
//                                    }
//                                }

//                                //Check if Renewal and set expired amounts equal to annual and written (Setting expired by simple assumption to make the import process easier)
//                                if (risk.ProductLine == (int)ProductLines.WC && Convert.ToInt16(risk.PolicySuffix) > 0)
//                                {
//                                    risk.PremiumInfo.ExpiredAnnualizedPremium = risk.PremiumInfo.AnnualizedPremium;
//                                    risk.PremiumInfo.ExpiringWrittenPremium = risk.PremiumInfo.WrittenPremium;
//                                }
//                                if (risk.ProductLine != (int)ProductLines.WC && Convert.ToInt16(risk.PolicySuffix) > 1)
//                                {
//                                    risk.PremiumInfo.ExpiredAnnualizedPremium = risk.PremiumInfo.AnnualizedPremium;
//                                    risk.PremiumInfo.ExpiringWrittenPremium = risk.PremiumInfo.WrittenPremium;
//                                }

//                                DAOFactory.GetDAO<RiskPremiumInfoDAO>().Update(uow, risk.PremiumInfo);

//                                //Update Purpose of use and Productline
//                                risk.PurposeOfUse = policyLevelPurposeOfUse;
//                                risk.AirportName = airportName;
//                                risk.AirportId = aiportCode;

//                                DAOFactory.GetDAO<RiskDAO>().Update(uow, risk);

//                                // ======= SAVING AND CREATING AIRCRAFT RECORD(S) ============== \\
//                                if (risk.ProductLine == (int)ProductLines.WC)
//                                {
//                                    risk.WorkersCompInfo.ProgramType = GetProgramType(dup002pf.symbol);
//                                    DAOFactory.GetDAO<RiskWorkersCompInfoDAO>().Update(uow, risk.WorkersCompInfo);
//                                }

//                                dataImport.AnnualPremium = (int)risk.PremiumInfo.AnnualizedPremium;
//                                dataImport.WrittenPremium = (int)risk.PremiumInfo.WrittenPremium;
//                                UpdateImportRecord(dataImport);

//                                totalPoliciesInserted += 1;
//                            }

//                            lineNo += 1;
//                        }
//                        catch (Exception ex)
//                        {
//                            log.LogWrite("Error at record: " + lineNo + " policy : " + dup002pf.symbol + dup002pf.policyNumber + " " +
//                                dup002pf.suffix + " error " +
//                                ex + " " + ex.StackTrace);
//                            totalPoliciesWithErrors += 1;

//                        }
//                        finally
//                        {
//                            totalPolicies += 1;
//                        }
//                        #endregion
//                    }
//                    policyReader.Close();
//                }
//            }
//            catch (Exception ex)
//            {
//                log.LogWrite("Error " + ex + " " + ex.StackTrace);
//            }

//            log.LogWrite("Summary: Total Policies " + totalPolicies + " Total Policies Inserted " + totalPoliciesInserted + " Total Policies That Experienced Errors " + totalPoliciesWithErrors);

//        }

//        #region SQL Queries for AS400 and BI Warehouse
//        //Policy
//        private string AS400PolicyInstanceSQL(string prefix, string policyNumber, string suffix)
//        {
//            StringBuilder sql = new StringBuilder();

//            sql.Append("SELECT  P_COMPANY,P_SYMBOL,P_P_NUMBER,P_SUFFIX,POLICY_STATUS, UNDERWRITER");
//            sql.Append(@" FROM	openquery([400Connect], 'SELECT P_COMPANY,P_SYMBOL, P_P_NUMBER, P_SUFFIX, POLICY_STATUS, UNDERWRITER 
//                          FROM PAMDATA.DUP002PF ");
//            sql.Append(String.Format(" WHERE	P_COMPANY = ''OR'' AND P_SYMBOL = ''{0}'' AND P_P_NUMBER = {1} AND P_SUFFIX = {2} ')", prefix, policyNumber, suffix));

//            return sql.ToString();
//        }

//        //All Policies
//        private string DUP002PF()
//        {
//            int date = 20160401;
//            int endDate = 20160430;
//            StringBuilder sql = new StringBuilder();

//            //Un-Comment to Select only top 300 rows.
//            //sql.Append("SELECT TOP 500 P_COMPANY,P_SYMBOL,P_P_NUMBER,P_SUFFIX,POLICY_STATUS,ISSUE_DATE,LAST_TRANS,P_TRANS_DATE,ACCOUNT_NAME,LINE_2,LINE_3,CITY,P_STATE,ZIP_1,ZIP_2");
//            sql.Append("SELECT P_COMPANY,P_SYMBOL,P_P_NUMBER,P_SUFFIX,POLICY_STATUS,ISSUE_DATE,LAST_TRANS,P_TRANS_DATE,ACCOUNT_NAME,LINE_2,LINE_3,CITY,P_STATE,ZIP_1,ZIP_2");

//            sql.Append(@",COINCODE,INSTALL_PAY_PLAN_ALPHA,PROD_CODE,CANCEL_DATE,REINSTATE_DATE,REIN_PROCESS_DATE,BRANCH,P_EFF_YEAR
//                       ,P_EFF_MONTH,P_EFF_DAY, P_EXP_YEAR, P_EXP_MONT,P_EXP_DAY,TERM,BILL_CODE,AUDIT_FREQ,UNDERWRITER,DO_NOT_RENEW
//                       ,HOME_OFFICE_INDICATOR,PROD_COMM,PAM_COMM,CO_COMM,TREATY_CODE,TREATY_YR,AGG_LIMIT, AGG_DEDUCT
//                       ,MINIMUM_PREM,DEPOSIT_PREM,P_RENEWAL_OF_SYMBOL,P_RENEWAL_OF_POL_NUM,P_RENEWAL_OF_SUFFIX,P_OCC_DEDUCT
//                       ,SECOND_PROD_CODE,SECOND_PROD_COMM,AGG_HULL_VALUE,ISNULL(P_TOTAL_NUM_AIRCRAFT,0) AS P_TOTAL_NUM_AIRCRAFT,ALPHA_NAME_LINE,P_ANNUAL_PREM
//                       ,PCANCODE,ISNULL( PNOCAMTDUE, 0) AS PNOCAMTDUE  
//                       FROM openquery([400Connect], 'SELECT P_COMPANY,P_SYMBOL, P_P_NUMBER, P_SUFFIX, POLICY_STATUS, ISSUE_DATE, LAST_TRANS, P_TRANS_DATE, ACCOUNT_NAME, LINE_2, LINE_3, CITY, P_STATE, ZIP_1, ZIP_2
//                       , COINCODE, INSTALL_PAY_PLAN_ALPHA, PROD_CODE,CANCEL_DATE, REINSTATE_DATE, REIN_PROCESS_DATE,BRANCH,P_EFF_YEAR, P_EFF_MONTH, P_EFF_DAY, P_EXP_YEAR, P_EXP_MONT, P_EXP_DAY, TERM,BILL_CODE, AUDIT_FREQ, UNDERWRITER, DO_NOT_RENEW
//                       , HOME_OFFICE_INDICATOR, PROD_COMM, PAM_COMM, CO_COMM, TREATY_CODE, TREATY_YR,AGG_LIMIT, AGG_DEDUCT, MINIMUM_PREM, DEPOSIT_PREM, P_RENEWAL_OF_SYMBOL, P_RENEWAL_OF_POL_NUM, P_RENEWAL_OF_SUFFIX, P_OCC_DEDUCT
//                       , SECOND_PROD_CODE, SECOND_PROD_COMM, AGG_HULL_VALUE, NULLIF(P_TOTAL_NUM_AIRCRAFT, 0 ) AS P_TOTAL_NUM_AIRCRAFT, ALPHA_NAME_LINE, P_ANNUAL_PREM
//                       , PCANCODE,NULLIF(PNOCAMTDUE, 0) AS PNOCAMTDUE 
//                       FROM PAMDATA.DUP002PF ");
//            // -- PRT Restrictions ---------------------------------------------------------------------------------------
//            sql.Append(String.Format(" WHERE P_EFF_YEAR || DIGITS(P_EFF_MONTH) || DIGITS(P_EFF_DAY) >= {0} ", date));
//            sql.Append(String.Format(" AND P_EFF_YEAR || DIGITS(P_EFF_MONTH) || DIGITS(P_EFF_DAY) <=  {0} ", endDate));
//            sql.Append(" AND P_COMPANY = ''OR'' AND P_SYMBOL != ''PB'' AND POLICY_STATUS IN (''A'',''C'') ");

//            // -- BI Additional Restrictions ------------------------------------------------------------------------------
//            sql.Append(" AND CANCEL_DATE != (P_EFF_YEAR || DIGITS(P_EFF_MONTH) || P_EFF_DAY) ");

//            //For Specific Year and Month
//            //sql.Append(" AND P_EFF_YEAR = 2016  ");
//            //sql.Append(" AND P_EFF_MONTH = 1 ");

//            sql.Append(" AND CANCEL_PROCESS_DATE != ISSUE_DATE ')");

//            // Remove Comment Below to test against a single record
//            //sql.Append("WHERE	P_COMPANY = 'OR' AND P_SYMBOL = 'CAN' AND P_P_NUMBER = 43938 AND P_SUFFIX = '0'");

//            //Excludes Workers Comp Policies
//            //sql.Append("WHERE P_SYMBOL NOT IN ('CAD','CAN','CAV','CAW','CDB', 'CAL', 'CAR') ");

//            //Include Workers Comp Policies ONLY
//            //sql.Append("WHERE P_SYMBOL IN ('CAD','CAN','CAV','CAW','CDB', 'CAL', 'CAR') ");

//            sql.Append("WHERE UNDERWRITER = 'BLM' and p_p_number not in (select cast(policynumber as int) from QuoteLookUp where UnderwriterId = 73 and EffectiveDate >= '04-01-2016' and EffectiveDate <= '04-30-2016' ) order by p_suffix asc");

//            return sql.ToString();
//        }

//        //Aircraft risk
//        private string DUP009PF(string symbol, string policyNumber, string suffix)
//        {
//            StringBuilder sql = new StringBuilder();

//            sql.Append("SELECT AV_FAA_NUMBER, ISNULL(AV_PLANE_YEAR,0) AS AV_PLANE_YEAR,AV_PLANE_MAKE, AV_AIRPORT_NO, AV_HULL_VALUE, AV_PLANE_USE, AV_RISK_NUMBER");
//            sql.Append(" FROM OPENQUERY([400CONNECT], 'SELECT AV_FAA_NUMBER, NULLIF(AV_PLANE_YEAR,0) AV_PLANE_YEAR, AV_PLANE_MAKE, AV_AIRPORT_NO, AV_HULL_VALUE, AV_PLANE_USE, AV_RISK_NUMBER");
//            sql.Append(" FROM PAMDATA.DUP009PF ");
//            sql.Append(String.Format(" WHERE P_SYMBOL =''{0}'' AND P_P_NUMBER = {1} AND P_SUFFIX = {2} ')", symbol, policyNumber, suffix));

//            return sql.ToString();
//        }

//        //Coverage
//        private string DUP012PFDIN(string symbol, string policyNumber, string suffix, string avRiskNumber)
//        {
//            StringBuilder sql = new StringBuilder();

//            sql.Append("SELECT * FROM OPENQUERY( [400CONNECT], 'SELECT COVERAGE_ID, PER_LIMIT, OCC_LIMIT, PREMIUM FROM PAMDATA.DUP012PF");
//            sql.Append(String.Format(" WHERE P_SYMBOL =''{0}''" + " AND P_P_NUMBER = {1} AND P_SUFFIX = {2}", symbol, policyNumber, suffix));
//            sql.Append(String.Format(" AND COVERAGE_ID = ''DIN'' AND RISK_NUM = {0} AND COV_EXP_YEAR = 0')", avRiskNumber));

//            return sql.ToString();
//        }

//        private string DUP012PFF(string symbol, string policyNumber, string suffix, string avRiskNumber)
//        {
//            StringBuilder sql = new StringBuilder();

//            sql.Append("SELECT * FROM OPENQUERY([400CONNECT], 'SELECT COVERAGE_ID, PER_LIMIT, OCC_LIMIT, PREMIUM FROM PAMDATA.DUP012PF");
//            sql.Append(String.Format(" WHERE P_SYMBOL = ''{0}''" + " AND P_P_NUMBER = {1}", symbol, policyNumber));
//            sql.Append(String.Format(" AND P_SUFFIX = {0} AND COVERAGE_ID = ''F'' AND RISK_NUM = {1} AND COV_EXP_YEAR = 0')", suffix, avRiskNumber));

//            return sql.ToString();
//        }

//        //Get model
//        private string DEP006PF(string make)
//        {
//            StringBuilder sql = new StringBuilder();
//            sql.Append(String.Format("SELECT TOP 1 * FROM OPENQUERY([400CONNECT], 'SELECT ET_TYPE_DESCRIPTION FROM PAMDATA.DEP006PF WHERE ET_KEY = ''06{0}'' ')", make));
//            return sql.ToString();
//        }

//        //Get Airport name
//        private string DUP010PF(string symbol, string policyNumber, string suffix)
//        {
//            string query;
//            query = "SELECT * FROM openquery([400Connect], 'SELECT ap_risk_description, ap_risk_airport_no " +
//    "from pamdata.DUP010PF where p_symbol =''" + symbol + "''" + " and p_p_number = " +
//    policyNumber + " and p_suffix = " + suffix + " order by risk_num desc')";
//            return query;
//        }

//        /// <summary>
//        /// Builds SQL Query string to get premiums from the new BI Data Warehouse
//        /// When using this function make sure that the BI Connection String is set to ORABIPROD
//        /// </summary>
//        /// <param name="policyPrefix"></param>
//        /// <param name="policyNumber"></param>
//        /// <param name="policySuffix"></param>
//        /// <returns></returns>
//        private string BINewServerPremiumSQL(string policyPrefix, string policyNumber, string policySuffix)
//        {
//            StringBuilder sql = new StringBuilder();

//            sql.Append("SELECT ISNULL(SUM(FC.WRTN_PREM_AMT), 0) AS WRITTENPREMIUM, ISNULL(SUM(DC.COV_ANN_PREM),0) AS ANNUALPREMIUM ");
//            sql.Append("FROM [FSBI_DW].[DBO].FACT_POLICYCOVERAGE FC ");
//            sql.Append(" INNER JOIN DIM_POLICY DP ON FC.POLICY_ID = DP.POLICY_ID ");
//            sql.Append(" LEFT JOIN DIM_COVERAGEEXTENSION DC ON FC.COVERAGE_UNIQUEID = DC.COV_UNIQUEID ");
//            sql.Append(String.Format(" WHERE	DP.POL_POLICYNUMBERPREFIX = '{0}' ", policyPrefix));
//            sql.Append(String.Format(" AND DP.POL_POLICYNUMBER = '{0}'", policyNumber));
//            sql.Append(String.Format(" AND DP.POL_POLICYNUMBERSUFFIX = '03' AND WRTN_PREM_AMT <> 0", policySuffix));

//            return sql.ToString();
//        }

//        /// <summary>
//        /// Builds SQL Query string to get premiums from the current in production BI Data Warehouse.
//        /// When using this function make sure that the Static BI Connection String property () is set to ORABI
//        /// </summary>
//        /// <param name="policyPrefix"></param>
//        /// <param name="policyNumber"></param>
//        /// <param name="policySuffix"></param>
//        /// <returns></returns>
//        private string BIProductionServerPremiumSQL(string policyPrefix, string policyNumber, string policySuffix)
//        {
//            StringBuilder sql = new StringBuilder();

//            sql.Append("SELECT ISNULL(SUM(FC.WRTN_PREM_AMT), 0) AS WRITTENPREMIUM, ISNULL(SUM(DC.COV_ANN_PREM),0) AS ANNUALPREMIUM ");
//            sql.Append("FROM [PAMWAREHOUSE].[DBO].FACT_POLICYCOVERAGE FC ");
//            sql.Append(" INNER JOIN DIM_POLICY DP ON FC.POLICY_ID = DP.POLICY_ID ");
//            sql.Append(" LEFT JOIN DIM_COVERAGEEXTENSION DC ON FC.COVERAGEEXTENSION_ID = DC.COVERAGEEXTENSION_ID  ");
//            sql.Append(String.Format(" WHERE	DP.POL_POLICYNUMBERPREFIX = '{0}' ", policyPrefix));
//            sql.Append(String.Format(" AND DP.POL_POLICYNUMBER = '{0}'", policyNumber));
//            sql.Append(String.Format(" AND DP.POL_POLICYNUMBERSUFFIX = '03' AND WRTN_PREM_AMT <> 0", policySuffix));

//            return sql.ToString();
//        }
//        #endregion

//        /// <summary>
//        /// 400 Data Reader
//        /// </summary>
//        /// <param name="query">SQL Select Query</param>
//        /// <returns>Data Reader Record Set Produced From Query</returns>
//        public OleDbDataReader AS400DataReader(string query, OleDbConnection conn)
//        {
//            //OleDbConnection conn = new OleDbConnection("Provider=SQLOLEDB;Data Source=172.17.11.17;Integrated Security=SSPI;Initial Catalog=UW_Base_App;User ID=dev;Password=Willowoak23");
//            OleDbCommand catCMD = conn.CreateCommand();
//            catCMD.CommandText = query;
//            OleDbDataReader reader = catCMD.ExecuteReader();
//            return reader;
//        }

//        /// <summary>
//        /// BI Tool Data Reader
//        /// </summary>
//        /// <param name="query">SQL Select Query</param>
//        /// <returns>Data Reader Record Set Produced From Query</returns>
//        public OleDbDataReader BIDataReader(string query, OleDbConnection conn)
//        {
//            OleDbCommand catCMD = conn.CreateCommand();
//            catCMD.CommandText = query;
//            OleDbDataReader reader = catCMD.ExecuteReader();
//            return reader;
//        }

//        /// <summary>
//        /// Retuns the Product Line Id based on the Risk Prefix and Purpose of Use
//        /// </summary>
//        /// <param name="prefix">Risk Prefix (Policy Type)</param>
//        /// <param name="use">Aircraft Use Code</param>
//        /// <returns>Product Line Id</returns>
//        private int GetProductLine(string prefix, string use)
//        {
//            int productLine = 0;

//            switch (prefix.Trim())
//            {
//                case "AA":
//                    productLine = 1;
//                    break;
//                case "AP":
//                    productLine = 3;
//                    break;
//                case "AV":
//                    if (use.Trim() == "PB")
//                    {
//                        productLine = 6;
//                    }
//                    else if (use.Trim() == "CP")
//                    {
//                        productLine = 4;
//                    }
//                    break;
//                case "AVC":
//                    productLine = 3;
//                    break;
//                case "AVL":
//                    productLine = 6;
//                    break;
//                case "CA":
//                    productLine = 4;
//                    break;
//                case "CAD":
//                    productLine = 8;
//                    break;
//                case "CAL":
//                    productLine = 8;
//                    break;
//                case "CAN":
//                    productLine = 8;
//                    break;
//                case "CAR":
//                    productLine = 8;
//                    break;
//                case "CAV":
//                    productLine = 8;
//                    break;
//                case "CDB":
//                    productLine = 8;
//                    break;
//                case "CR":
//                    productLine = 0;
//                    break;
//                case "DNX":
//                    productLine = 0;
//                    break;
//                case "HL":
//                    productLine = 7;
//                    break;
//                case "MP":
//                    productLine = 5;
//                    break;
//                case "PB":
//                    productLine = 6;
//                    break;
//                case "PMA":
//                    productLine = 9;
//                    break;
//                case "PR":
//                    productLine = 7;
//                    break;
//                case "RAL":
//                    productLine = 2;
//                    break;
//                default:
//                    break;
//            }
//            return productLine;
//        }

//        /// <summary>
//        /// Returns the correct Workers Comp Program type based on the Workers Comp Policy Type
//        /// </summary>
//        /// <param name="prefix">Risk Prefix (Policy Type)</param>
//        /// <returns>Workers Comp Program Type</returns>
//        private string GetProgramType(string prefix)
//        {
//            string programType = string.Empty;

//            switch (prefix.Trim())
//            {
//                case "CAN":
//                    programType = "NBAA";
//                    break;
//                case "CAV":
//                    programType = "GC";
//                    break;
//                case "CAD":
//                    programType = "SSDIV";
//                    break;
//                case "CAR":
//                    programType = "RET";
//                    break;
//                case "CAL":
//                    programType = "LD";
//                    break;
//                case "CDB":
//                    programType = "DBA";
//                    break;

//                default:
//                    break;
//            }
//            return programType;
//        }

//        /// <summary>
//        /// Checks to see if the policy record being imported has alraedy been successfully imported before.
//        /// </summary>
//        /// <param name="company">Company Code</param>
//        /// <param name="prefix">Risk Prefix (Policy Type)</param>
//        /// <param name="policyNumber">Policy Number</param>
//        /// <param name="suffix">Policy Suffix</param>
//        /// <returns>If Has Been Successfully Imported</returns>
//        private PolicyTracker.DomainModel.Other.DataImport GetDataImport(string company, string prefix, string policyNumber, string suffix)
//        {
//            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();

//            var filters = new List<PropertyFilter>();
//            filters.Add(new PropertyFilter("Company", company));
//            filters.Add(new PropertyFilter("Prefix", prefix));
//            filters.Add(new PropertyFilter("PolicyNumber", policyNumber));
//            filters.Add(new PropertyFilter("Suffix", suffix));

//            var dataImport = DAOFactory.GetDAO<DataImportDAO>().GetInstance(uow, filters);

//            return dataImport;
//        }

//        private PolicyTracker.DomainModel.Other.DataImport InsertImportRecord(string company, string prefix, string policyNumber, string suffix)
//        {
//            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();

//            var rec = new PolicyTracker.DomainModel.Other.DataImport()
//            {
//                Company = company,
//                Prefix = prefix,
//                PolicyNumber = policyNumber,
//                Suffix = suffix,
//                CreatedOn = DateTime.Now
//            };

//            DAOFactory.GetDAO<DataImportDAO>().Create(uow, rec);

//            return rec;
//        }

//        private void UpdateImportRecord(PolicyTracker.DomainModel.Other.DataImport rec)
//        {
//            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
//            rec.DateInserted = DateTime.Now;
//            rec.IsImported = true;
//            DAOFactory.GetDAO<DataImportDAO>().Update(uow, rec);
//        }
//    }

//    [TestClass]
//    public class WCFlowDataImporter : TransactionalUnitTest
//    {
//        #region Class Setup/Test Initialization
//        [ClassInitialize]
//        public static void Setup(TestContext context)
//        {
//            Login();
//        }

//        [ClassCleanup]
//        public static void TearDown()
//        {
//            Logout();
//        }
//        #endregion

//        app_code.LogWriter log = new app_code.LogWriter("");

//        [TestMethod]
//        public void ImportNewBusiness()
//        {
//            log.LogWrite("workers Comp Flow Data Import Started [New Business]");

//            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
//            var totalImported = 0;
//            var totalErrors = 0;
//            bool isSingleRowTest = false;
//            var specificRow = 151 + 1;

//            TextFieldParser parser = new TextFieldParser(@"C:\testing\wc-data-import\2016_NewBusiness_April_corrected.csv");
//            parser.TextFieldType = FieldType.Delimited;
//            parser.SetDelimiters(",");

//            while (!parser.EndOfData)
//            {
//                var fileRow = parser.LineNumber;
//                string[] fields = parser.ReadFields();

//                //Processing row
//                if (fileRow >= 1)
//                {
//                    if ((!isSingleRowTest || (isSingleRowTest && fileRow == specificRow)) && fields[9] != "Bound")
//                    {
//                        try
//                        {
//                            var uw = DAOFactory.GetDAO<UserEntityDAO>().GetInstance(uow, new PropertyFilter("Initials", fields[2]));
//                            var uwId = (uw == null) ? 0 : uw.UserId;
//                            var namedInsured = new NamedInsured()
//                            {
//                                CompanyName = fields[5],
//                                City = "DataImport",
//                                State = fields[10].ToUpper()
//                            };

//                            DateTime effectiveDate;
//                            DateTime createdDate;
//                            var prefix = StringEnum.GetAll<PolicyPrefixMap>().Where(x => x.Value == fields[1].ToUpper()).FirstOrDefault();
//                            var status = StringEnum.GetAll<StatusMap>().Where(x => x.Value == fields[9].ToUpper()).FirstOrDefault();

//                            var risk = new Risk()
//                            {
//                                Branch = fields[0],
//                                UnderwriterId = uwId,
//                                EffectiveDate = (DateTime.TryParse(fields[3], out effectiveDate)) ? effectiveDate : DateTime.Parse("1753-01-01"),
//                                CreatedDate = (DateTime.TryParse(fields[4], out createdDate)) ? createdDate : DateTime.Parse("1753-01-01"),
//                                AgencyID = fields[7].ToUpper(),
//                                Status = (status != null) ? status.DisplayText : "",
//                                FirstNote = new RiskNote(),
//                                ProductLine = 8,
//                                Prefix = (prefix != null) ? prefix.DisplayText : "",
//                            };

//                            risk.NamedInsured = namedInsured;

//                            risk = ServiceLocator.PolicySvc.SaveNewRisk(risk, false);

//                            var note = new RiskNote()
//                            {
//                                CreatedById = uwId,
//                                CreatedDate = (DateTime.TryParse(fields[4], out createdDate)) ? createdDate : DateTime.Parse("1753-01-01"),
//                                Comment = fields[16],
//                                RiskId = risk.Id
//                            };

//                            //Try to ignore any weird defaults or 0s in the comment field.
//                            if (note.Comment.Length > 1)
//                                DAOFactory.GetDAO<RiskNotesDAO>().Create(uow, note);

//                            risk.WorkersCompInfo.AccountDescription = GetAccountDescription(fields[6]);
//                            risk.WorkersCompInfo.ScheduledRating = (Regex.Replace(fields[11], @"[^0-9.]", String.Empty) == String.Empty) ? 0 : Convert.ToDecimal(Regex.Replace(fields[11], @"[^0-9.]", String.Empty));
//                            risk.WorkersCompInfo.ExperienceModifier = (Regex.Replace(fields[12].Split(' ').Last(), @"[^0-9.]", String.Empty) == String.Empty) ? 0 : Convert.ToDecimal(Regex.Replace(fields[12].Split(' ').Last(), @"[^0-9.]", String.Empty));

//                            var programType = StringEnum.GetAll<ProgramTypeMap>().Where(x => x.Value == fields[1].ToUpper()).FirstOrDefault();
//                            risk.WorkersCompInfo.ProgramType = (programType != null) ? programType.DisplayText : "";

//                            risk.PremiumInfo.AnnualizedPremium = (Regex.Replace(fields[13], @"[^\d]", String.Empty) == String.Empty) ? 0 : Convert.ToDecimal(Regex.Replace(fields[13], @"[^\d]", String.Empty));
//                            risk.PremiumInfo.DepositPremium = (Regex.Replace(fields[14], @"[^\d]", String.Empty) == String.Empty) ? 0 : Convert.ToDecimal(Regex.Replace(fields[14], @"[^\d]", String.Empty));

//                            if (risk.PremiumInfo.DepositPremium > 0) risk.InstallmentInfo.IsPaidInInstallments = true;

//                            ServiceLocator.PolicySvc.Save(risk);
//                            totalImported++;
//                        }
//                        catch (Exception ex)
//                        {
//                            log.LogWrite("[Row#: " + fileRow + "] Erro = " + ex.Message + "}");
//                            totalErrors++;
//                        }
//                        finally
//                        {
//                        }
//                    }
//                }
//            }

//            log.LogWrite(String.Format("WC Flow Data Import Complete. Total Records Imported [ {0} ], Total Errors [ {1} ]", totalImported, totalErrors));

//            parser.Close();
//        }

//        [TestMethod]
//        public void ImportRenewals()
//        {
//            log.LogWrite("workers Comp Flow Data Import Started [Renewals]");

//            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
//            var totalImported = 0;
//            var totalErrors = 0;
//            bool isSingleRowTest = false;
//            var specificRow = 90;

//            TextFieldParser parser = new TextFieldParser(@"C:\testing\wc-data-import\2015_Renewals.csv");
//            parser.TextFieldType = FieldType.Delimited;
//            parser.SetDelimiters(",");

//            while (!parser.EndOfData)
//            {
//                var fileRow = parser.LineNumber;
//                string[] fields = parser.ReadFields();

//                //Processing row
//                if (fileRow >= 1)
//                {
//                    if ((!isSingleRowTest || (isSingleRowTest && fileRow == specificRow)) && fields[9] == "Non-Renewed")
//                    {
//                        try
//                        {
//                            var uw = DAOFactory.GetDAO<UserEntityDAO>().GetInstance(uow, new PropertyFilter("Initials", fields[2]));

//                            var namedInsured = new NamedInsured()
//                            {
//                                CompanyName = fields[5],
//                                City = "DataImport",
//                                State = fields[10].ToUpper()
//                            };

//                            DateTime effectiveDate;
//                            var prefix = StringEnum.GetAll<PolicyPrefixMap>().Where(x => x.Value == fields[1].ToUpper()).FirstOrDefault();
//                            var status = StringEnum.GetAll<StatusMap>().Where(x => x.Value == fields[9].ToUpper()).FirstOrDefault();

//                            var risk = new Risk()
//                            {
//                                Branch = fields[0],
//                                UnderwriterId = uw != null ? uw.UserId : 0,
//                                EffectiveDate = (DateTime.TryParse(fields[3], out effectiveDate)) ? effectiveDate : DateTime.Parse("1753-01-01"),
//                                CreatedDate = DateTime.Now,
//                                AgencyID = fields[7].ToUpper(),
//                                Status = (status != null) ? status.DisplayText : "",
//                                FirstNote = new RiskNote(),
//                                ProductLine = 8,
//                                Prefix = (prefix != null) ? prefix.DisplayText : "",
//                                PolicyNumber = fields[4].Split(' ')[1],
//                                PolicySuffix = fields[4].Split(' ')[2],
//                                ImageRightId = (prefix != null) ? prefix.DisplayText + fields[4].Split(' ')[1] : "" + fields[4].Split(' ')[1]
//                            };

//                            risk.NamedInsured = namedInsured;

//                            risk = ServiceLocator.PolicySvc.SaveNewRisk(risk, false);

//                            var note = new RiskNote()
//                            {
//                                CreatedById = uw != null ? uw.UserId : 0,
//                                CreatedDate = DateTime.Now,
//                                Comment = fields[16],
//                                RiskId = risk.Id
//                            };

//                            //Try to ignore any weird defaults or 0s in the comment field.
//                            if (note.Comment.Length > 1)
//                                DAOFactory.GetDAO<RiskNotesDAO>().Create(uow, note);

//                            risk.WorkersCompInfo.AccountDescription = GetAccountDescription(fields[6]);
//                            risk.WorkersCompInfo.ScheduledRating = (Regex.Replace(fields[11], @"[^0-9.]", String.Empty) == String.Empty) ? 0 : Convert.ToDecimal(Regex.Replace(fields[11], @"[^0-9.]", String.Empty));
//                            risk.WorkersCompInfo.ExperienceModifier = (Regex.Replace(fields[12].Split(' ').Last(), @"[^0-9.]", String.Empty) == String.Empty) ? 0 : Convert.ToDecimal(Regex.Replace(fields[12].Split(' ').Last(), @"[^0-9.]", String.Empty));

//                            var programType = StringEnum.GetAll<ProgramTypeMap>().Where(x => x.Value == fields[1]).FirstOrDefault();
//                            risk.WorkersCompInfo.ProgramType = (programType != null) ? programType.DisplayText : "";

//                            risk.WorkersCompInfo.ExpiringPayroll = (Regex.Replace(fields[15], @"[^\d]", String.Empty) == String.Empty) ? 0 : Convert.ToDecimal(Regex.Replace(fields[15], @"[^\d]", String.Empty));
//                            risk.WorkersCompInfo.Payroll = (Regex.Replace(fields[19], @"[^\d]", String.Empty) == String.Empty) ? 0 : Convert.ToDecimal(Regex.Replace(fields[19], @"[^\d]", String.Empty));

//                            risk.PremiumInfo.ExpiringWrittenPremium = (Regex.Replace(fields[13], @"[^\d]", String.Empty) == String.Empty) ? 0 : Convert.ToDecimal(Regex.Replace(fields[13], @"[^\d]", String.Empty));
//                            risk.PremiumInfo.AnnualizedPremium = (Regex.Replace(fields[17], @"[^\d]", String.Empty) == String.Empty) ? 0 : Convert.ToDecimal(Regex.Replace(fields[17], @"[^\d]", String.Empty));
//                            risk.PremiumInfo.WrittenPremium = risk.PremiumInfo.AnnualizedPremium;
//                            risk.PremiumInfo.DepositPremium = (Regex.Replace(fields[18], @"[^\d]", String.Empty) == String.Empty) ? 0 : Convert.ToDecimal(Regex.Replace(fields[18], @"[^\d]", String.Empty));

//                            if (risk.PremiumInfo.DepositPremium == risk.PremiumInfo.AnnualizedPremium)
//                                risk.PremiumInfo.DepositPremium = 0;

//                            if (risk.PremiumInfo.DepositPremium > 0) risk.InstallmentInfo.IsPaidInInstallments = true;

//                            ServiceLocator.PolicySvc.Save(risk);
//                            totalImported++;
//                        }
//                        catch (Exception ex)
//                        {
//                            log.LogWrite("[Row#: " + fileRow + "] Erro = " + ex.Message + "}");
//                            totalErrors++;
//                        }
//                        finally
//                        {
//                        }
//                    }
//                }
//            }

//            log.LogWrite(String.Format("WC Flow Data Import Complete. Total Records Imported [ {0} ], Total Errors [ {1} ]", totalImported, totalErrors));

//            parser.Close();
//        }

//        [TestMethod]
//        public void ImportRenewalsDepositUpdates()
//        {
//            log.LogWrite("Workers Deposit Updates");

//            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
//            var totalImported = 0;
//            var totalErrors = 0;
//            bool isSingleRowTest = false;
//            var specificRow = 1;

//            TextFieldParser parser = new TextFieldParser(@"C:\testing\wc-data-import\2016_Renewals.csv");
//            parser.TextFieldType = FieldType.Delimited;
//            parser.SetDelimiters(",");

//            while (!parser.EndOfData)
//            {
//                var fileRow = parser.LineNumber;
//                string[] fields = parser.ReadFields();

//                //Processing row
//                if (fileRow >= 1)
//                {
//                    if ((!isSingleRowTest || (isSingleRowTest && fileRow == specificRow)) && fields[9] == "Bound")
//                    {
//                        try
//                        {
//                            var prefix = StringEnum.GetAll<PolicyPrefixMap>().Where(x => x.Value == fields[1].ToUpper()).FirstOrDefault();

//                            var filters = new List<PropertyFilter>()
//                            {
//                                new PropertyFilter("Prefix", (prefix != null) ? prefix.DisplayText : ""),
//                                new PropertyFilter("PolicyNumber", fields[4].Split(' ')[1])
//                            };

//                            var risks = ServiceLocator.EntityService.GetList<Risk>(filters);

//                            var newest = risks.OrderByDescending(x => x.PolicySuffix).FirstOrDefault();

//                            if (newest != null)
//                            {
//                                var riskPremiumInfo = ServiceLocator.EntityService.GetInstance<RiskPremiumInfo>(new PropertyFilter("RiskId", newest.Id));
//                                var written = (Regex.Replace(fields[17], @"[^\d]", String.Empty) == String.Empty) ? 0 : Convert.ToDecimal(Regex.Replace(fields[17], @"[^\d]", String.Empty));
//                                var deposit = (Regex.Replace(fields[18], @"[^\d]", String.Empty) == String.Empty) ? 0 : Convert.ToDecimal(Regex.Replace(fields[18], @"[^\d]", String.Empty));

//                                riskPremiumInfo.DepositPremium = deposit;
//                                if (riskPremiumInfo.DepositPremium > 0)
//                                {
//                                    var riskInstallmentInfo = ServiceLocator.EntityService.GetInstance<RiskInstallmentInfo>(new PropertyFilter("RiskId", newest.Id));
//                                    riskInstallmentInfo.IsPaidInInstallments = true;
//                                    DAOFactory.GetDAO<RiskInstallmentInfoDAO>().Update(uow, riskInstallmentInfo);
//                                }

//                                DAOFactory.GetDAO<RiskPremiumInfoDAO>().Update(uow, riskPremiumInfo);
//                            }
                            
//                            totalImported++;
//                        }
//                        catch (Exception ex)
//                        {
//                            log.LogWrite("[Row#: " + fileRow + "] Erro = " + ex.Message + "}");
//                            totalErrors++;
//                        }
//                        finally
//                        {
//                        }
//                    }
//                }
//            }

//            log.LogWrite(String.Format("WC Flow Data Import Complete. Total Records Imported [ {0} ], Total Errors [ {1} ]", totalImported, totalErrors));

//            parser.Close();
//        }

//        [TestMethod]
//        public void ImportInstallments()
//        {
//            log.LogWrite("workers Comp Installment Import Started");

//            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
//            var totalImported = 0;
//            var totalErrors = 0;
//            bool isSingleRowTest = false;
//            var specificRow = 165;

//            TextFieldParser parser = new TextFieldParser(@"C:\testing\wc-data-import\Installments.csv");
//            parser.TextFieldType = FieldType.Delimited;
//            parser.SetDelimiters(",");

//            while (!parser.EndOfData)
//            {
//                var fileRow = parser.LineNumber;
//                string[] fields = parser.ReadFields();

//                //Processing row
//                if (fileRow >= 1)
//                {
//                    if ((!isSingleRowTest || (isSingleRowTest && fileRow == specificRow)))
//                    {
//                        try
//                        {
//                            var policyNumber = fields[0].Split(' ');
//                            var filters = new List<PropertyFilter>()
//                            {
//                                new PropertyFilter("Prefix", policyNumber[0]),
//                                new PropertyFilter("PolicyNumber", policyNumber[1]),
//                                new PropertyFilter("PolicySuffix", policyNumber[2])
//                            };
//                            var risk = DAOFactory.GetDAO<RiskDAO>().GetInstance(uow, filters);

//                            if (risk != null)
//                            {
//                                risk.InstallmentInfo = DAOFactory.GetDAO<RiskInstallmentInfoDAO>().GetInstance(uow, new PropertyFilter("RiskId", risk.Id));
//                                risk.InstallmentInfo.IsPaidInInstallments = true;

//                                var payment = new RiskPayment()
//                                {
//                                    RiskId = risk.Id,
//                                    AnticipatedAmount = (Regex.Replace(fields[1], @"[^\d]", String.Empty) == String.Empty) ? 0 : Convert.ToDecimal(Regex.Replace(fields[1], @"[^\d]", String.Empty)),
//                                    ActualAmount = (Regex.Replace(fields[1], @"[^\d]", String.Empty) == String.Empty) ? 0 : Convert.ToDecimal(Regex.Replace(fields[1], @"[^\d]", String.Empty)),
//                                    DueDate = DateTime.Parse(fields[2]),
//                                    InvoicedDate = DateTime.Parse(fields[2])
//                                };

//                                DAOFactory.GetDAO<RiskPaymentDAO>().Create(uow, payment);
//                                DAOFactory.GetDAO<RiskInstallmentInfoDAO>().Update(uow, risk.InstallmentInfo);
//                            }

//                            totalImported++;
//                        }
//                        catch (Exception ex)
//                        {
//                            log.LogWrite("[Row#: " + fileRow + "] Erro = " + ex.Message + "}");
//                            totalErrors++;
//                        }
//                        finally
//                        {
//                        }
//                    }
//                }
//            }

//            log.LogWrite(String.Format("WC Installment Import Complete. Total Records Imported [ {0} ], Total Errors [ {1} ]", totalImported, totalErrors));

//            parser.Close();
//        }

//        private string GetAccountDescription(string desc)
//        {
//            var indicator = desc.Split(' ').First();
//            var result = String.Empty;

//            switch (indicator)
//            {
//                case "IAC":
//                    result = WorkCompAccountDesc.IAC.Value;
//                    break;
//                case "MRO":
//                    result = WorkCompAccountDesc.MRO.Value;
//                    break;
//                case "FBO":
//                    result = WorkCompAccountDesc.FBO.Value;
//                    break;
//                case "O135":
//                    result = WorkCompAccountDesc.O135.Value;
//                    break;
//                case "NOC":
//                    result = WorkCompAccountDesc.NOC.Value;
//                    break;
//                case "IR":
//                    result = WorkCompAccountDesc.IR.Value;
//                    break;
//                case "ALS":
//                    result = WorkCompAccountDesc.ALS.Value;
//                    break;
//                case "AA":
//                    result = WorkCompAccountDesc.AA.Value;
//                    break;
//                case "RW":
//                    result = WorkCompAccountDesc.RW.Value;
//                    break;
//            }

//            return result;
//        }

//        private class StatusMap : StringEnum
//        {
//            public static StatusMap UNS = new StatusMap() { Value = "Unsuccessful".ToUpper(), DisplayText = "Lost" };
//            public static StatusMap DEC = new StatusMap() { Value = "Declined".ToUpper(), DisplayText = "Declined" };
//            public static StatusMap REV = new StatusMap() { Value = "Reviewing".ToUpper(), DisplayText = "Submission" };
//            public static StatusMap QUO = new StatusMap() { Value = "Quoted".ToUpper(), DisplayText = "Quote" };
//            public static StatusMap NON = new StatusMap() { Value = "Non-Renewed".ToUpper(), DisplayText = "Lost" };

//            private StatusMap(string val, string text) : base(val, text) { }
//            private StatusMap() { }
//        }

//        private class ProgramTypeMap : StringEnum
//        {
//            public static ProgramTypeMap GUA = new ProgramTypeMap() { Value = "Guaranteed Cost".ToUpper(), DisplayText = "GC" };
//            public static ProgramTypeMap NBA = new ProgramTypeMap() { Value = "NBAA", DisplayText = "NBAA" };
//            public static ProgramTypeMap SSD = new ProgramTypeMap() { Value = "SSDIV", DisplayText = "SSDIV" };

//            private ProgramTypeMap(string val, string text) : base(val, text) { }
//            private ProgramTypeMap() { }
//        }

//        private class PolicyPrefixMap : StringEnum
//        {
//            public static PolicyPrefixMap GUA = new PolicyPrefixMap() { Value = "Guaranteed Cost".ToUpper(), DisplayText = "CAV" };
//            public static PolicyPrefixMap NBA = new PolicyPrefixMap() { Value = "NBAA", DisplayText = "CAN" };
//            public static PolicyPrefixMap SSD = new PolicyPrefixMap() { Value = "SSDIV", DisplayText = "CAD" };
//            public static PolicyPrefixMap DBA = new PolicyPrefixMap() { Value = "DBA", DisplayText = "CDB" };
//            public static PolicyPrefixMap LRG = new PolicyPrefixMap() { Value = "Large Ded".ToUpper(), DisplayText = "LD" };
//            public static PolicyPrefixMap LD = new PolicyPrefixMap() { Value = "Large Deductible".ToUpper(), DisplayText = "LD" };

//            private PolicyPrefixMap(string val, string text) : base(val, text) { }
//            private PolicyPrefixMap() { }
//        }
//    }
//}