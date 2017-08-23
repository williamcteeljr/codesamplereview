/******************************************************
 * Purpose:  This program loads the Claims input file
 *  into the RiskClaims database table UnderwritingExperience
 * 
 * 
 * 
 * Change Log
 * Date      Programmer        Description
 * 08/13/15  V. Felder         Intial Code
 ******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.VisualBasic.FileIO;
using System.Text.RegularExpressions;
using System.Data;
using ClaimsLoad.Library;
using System.Data.Entity;
using System.Data.OleDb;

namespace ClaimsLoad
{
    class Program
    {
        static string newFileRecievedDir = System.Configuration.ConfigurationManager.AppSettings["newFileRecievedDir"].ToString();
        static string archivedDir = System.Configuration.ConfigurationManager.AppSettings["archivedDir"].ToString();
        static string claimsFile = "";
        static int lineNo = 0;
        static int numberOflinesInserted, numberOflinesNotInsert, numberofLinesWithErrors, numberOfLines,
            standPremiumTotal, earnPremiumTotal, writPremiumTotal, claimCount, dividend, pdExpense, 
            pdLoss, incLoss = 0;
        static bool isTotalsMatched = false;
        static LogWriter log = new LogWriter("The workers comp claims load data file process has started...");
        static string summaryMessage = "";
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Checking if a new file was received...");
                bool isNewFile = NewClaimsFileRecieved();
                if (isNewFile)
                {
                    try
                    {
                        summaryMessage = "<b>Total Lines </b>" + (numberOfLines - 1) + " <b>Lines Inserted</b> " + numberOflinesInserted +
                            " <b>Lines Not Inserted</b> " + numberOflinesNotInsert + " <b>Lines w/Errors</b> " + numberofLinesWithErrors +
                            " <b>All Totals Matched</b> " + isTotalsMatched;
                        EmailHandler.SendEmail(summaryMessage, claimsFile);
                        Console.WriteLine("Email sent.");
                        log.LogWrite("Email sent.");
                    }
                    catch (Exception ex)
                    {
                        log.LogWrite("There was an issue sending the email the error was : " + ex);
                    }
                }
                else
                {
                    log.LogWrite("No file was received. ");

                }
            }
            catch (Exception ex)
            {
                log.LogWrite("There was an issue encountered in the main program the error was: " + ex);
            }
        }
        /// <summary>
        /// Check to see if new file was received
        /// </summary>
        /// <returns></returns>
        static bool NewClaimsFileRecieved()
        {

            bool isNewFile = false;
            int fileCount = Directory.GetFiles(newFileRecievedDir).Length;
            if (fileCount > 0)
            {
                Console.WriteLine("A new file was received...");
                log.LogWrite("A new file was received...");
                string[] files = Directory.GetFiles(newFileRecievedDir);
                foreach (string sFile in files)
                {
                    claimsFile = Path.GetFileName(sFile);
                    //Process only csv files
                    if (sFile.ToUpper().Contains("CSV"))
                    {
                        isNewFile = true;
                        //Empty Table First
                        try
                        {
                            Console.WriteLine("Truncating table");
                            log.LogWrite("Truncating table");
                            UW_Base_AppEntities context = new UW_Base_AppEntities();
                            context.Database.ExecuteSqlCommand("TRUNCATE TABLE [UW_Base_App].[dbo].[RiskClaims]");

                            //Process file contents
                            try
                            {
                                ProcessFileContents(sFile);

                            }
                            catch (Exception ex)
                            {
                                log.LogWrite("There was an issue encountered processing the file contents:" + ex);
                            }
                        }
                        catch (Exception ex)
                        {
                            log.LogWrite("There was an issue truncating the table:" + ex);
                        }
                        //Summary information the -1 is to remove the count for header
                        summaryMessage = "Summary: " + "Total Lines " + (numberOfLines - 1) + " Lines Inserted " + numberOflinesInserted +
                            " Lines Not Inserted " + numberOflinesNotInsert + " Lines w/Errors " + numberofLinesWithErrors +
                            " All Totals Matched " + isTotalsMatched ;
                        log.LogWrite(summaryMessage);
                        Console.WriteLine("The file " + claimsFile + " has been archived.");
                        if (archivedDir != "")
                        {
                            File.Move(sFile, archivedDir + claimsFile);
                        }
                    }
                }

            }
            return isNewFile;
        }

        /// <summary>
        /// Processing file contents
        /// </summary>
        /// <param name="file"></param>
        static void ProcessFileContents(string file)
        {
            Console.WriteLine("Processing file " + claimsFile + " contents...");
            log.LogWrite("Processing file " + claimsFile + " contents...");
            UnderwritingExperience uw = new UnderwritingExperience();
            TextFieldParser parser = new TextFieldParser(file);
            parser.TextFieldType = FieldType.Delimited;

            parser.SetDelimiters(",");
            while (!parser.EndOfData)
            {
                //Skip first two rows
                string[] fields = parser.ReadFields();
                
                if (lineNo > 1)
                {

                    try
                    {
                        uw.policyEffectiveDate = fields[0];
                        if (uw.policyEffectiveDate.Trim().ToUpper() != "TOTAL")
                        {
                            uw.prefix = fields[1].Substring(1, 3);
                            uw.policyNumber = fields[1].Substring(4, 6);
                            uw.suffix = fields[1].Substring(10, 2);
                            uw.name = fields[2];
                            uw.standard = Int32.Parse(fields[3]);
                            uw.earned = Int32.Parse(fields[4]);
                            uw.written = Int32.Parse(fields[5]);
                            uw.claimCount = Int32.Parse(fields[6]);
                            uw.dividend = Int32.Parse(fields[7]);
                            uw.pdExpenses = Int32.Parse(fields[8]);
                            uw.pdLoss = Int32.Parse(fields[9]);
                            uw.incLoss = Int32.Parse(fields[10]);
                        }
                        //Get totals
                        else
                        {
                            standPremiumTotal = Int32.Parse(fields[3]);
                            earnPremiumTotal = Int32.Parse(fields[4]);
                            writPremiumTotal = Int32.Parse(fields[5]);
                            claimCount = Int32.Parse(fields[6]);
                            dividend = Int32.Parse(fields[7]);
                            pdExpense = Int32.Parse(fields[8]);
                            pdLoss = Int32.Parse(fields[9]);
                            incLoss = Int32.Parse(fields[10]);
                        }

                    }
                    catch (Exception ex)
                    {
                        log.LogWrite("There was an issue inserting line: " + lineNo + "policy number: " +
                            uw.prefix + uw.policyNumber + uw.suffix + " the error was: " + ex);
                        numberofLinesWithErrors++;
                    }
                    //Write to database
                    try
                    {
                        if (uw.policyEffectiveDate.Trim().ToUpper() != "TOTAL")
                        {
                            insertData(uw);
                        }
                        numberOflinesInserted++;
                    }
                    catch (Exception ex)
                    {
                        log.LogWrite("There was an issue inserting line: " + lineNo + "policy number: " +
                            uw.prefix + uw.policyNumber + uw.suffix + " the error was: " + ex);
                        numberOflinesNotInsert++;
                    }
                   
                }//If
                lineNo++;
            }//While
            numberOfLines = lineNo;
            //Check if totals matched
            try
            {
                isTotalsMatched = IsTotalMatched(standPremiumTotal, earnPremiumTotal, writPremiumTotal, claimCount, dividend, pdExpense, pdLoss, incLoss);
            }
            catch (Exception ex)
            {
                log.LogWrite("There was a matching the totals the error was: " + ex);
            }
            parser.Close();
        }
        /// <summary>
        /// Matching the summed totals from the database to the values in total column
        /// </summary>
        /// <param name="standard"></param>
        /// <param name="earned"></param>
        /// <param name="written"></param>
        /// <param name="clmCnt"></param>
        /// <param name="div"></param>
        /// <param name="pdExp"></param>
        /// <param name="pdLs"></param>
        /// <param name="incLs"></param>
        /// <returns></returns>
        static bool IsTotalMatched(int standard, int earned, int written, int clmCnt, int div, int pdExp, int pdLs, int incLs )
        {
            bool totalsMatched = true;
            OleDbDataReader reader = DataReader(DataRead());
            while (reader.Read())
            {

                if (Convert.ToInt32(reader[0]) != standard)
                {
                    totalsMatched = false;
                }
                if ( Convert.ToInt32(reader[1]) != earned )
                {
                    totalsMatched = false;
                }
                if (Convert.ToInt32(reader[2]) != written)
                {
                    totalsMatched = false;
                }
                if (Convert.ToInt32(reader[3]) != clmCnt)
                {
                    totalsMatched = false;
                }
                if (Convert.ToInt32(reader[4]) != div)
                {
                    totalsMatched = false;
                }
                if (Convert.ToInt32(reader[5]) != pdExpense )
                {
                    totalsMatched = false;
                }
                if (Convert.ToInt32(reader[6]) != pdLs)
                {
                    totalsMatched = false;
                }
                if (Convert.ToInt32(reader[7]) != incLs )
                {
                    totalsMatched = false;
                }
            } 
            
            return totalsMatched;
        }
        /// <summary>
        /// Insert data into table
        /// </summary>
        /// <param name="record"></param>
        static void insertData(UnderwritingExperience record)
        {
            RiskClaim rc = new RiskClaim();
            rc.PolicyEffDate = DateTime.ParseExact(record.policyEffectiveDate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);// Convert.ToDateTime(record.policyEffectiveDate);
            rc.Prefix = record.prefix.Trim ();
            rc.PolicyNumber = record.policyNumber;
            rc.Suffix = record.suffix;
            rc.Name = record.name;
            rc.Standard = record.standard;
            rc.Earned = record.earned;
            rc.Written = record.written;
            rc.ClaimCount = record.claimCount;
            rc.Dividend = record.dividend;
            rc.PaidExpense = record.pdExpenses;
            rc.PaidLoss = record.pdLoss;
            rc.IncurredLoss = record.incLoss;

            using (var dbCtx = new UW_Base_AppEntities())
            {
                dbCtx.RiskClaims.Add(rc);
                dbCtx.SaveChanges();
            }
        }
        /// <summary>
        /// String to retrieved summed totals
        /// </summary>
        /// <returns></returns>
        static string DataRead()
        {
            string query;
            query = "SELECT SUM([Standard]) standard,SUM([Earned])earned,SUM([Written])written,SUM([ClaimCount])claimcount " +
	",SUM([Dividend])dividend,SUM([PaidExpense])paidexpense,SUM([PaidLoss])paidloss,SUM([IncurredLoss])inccurredRloss" +
    " FROM [UW_Base_App].[dbo].[RiskClaims]";
            return query;
        }
        /// <summary>
        /// Connection settings
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        static OleDbDataReader DataReader(string query)
        {
            OleDbConnection conn = new OleDbConnection(System .Configuration .ConfigurationManager .ConnectionStrings ["DB"].ConnectionString );

            OleDbCommand catCMD = conn.CreateCommand();
            catCMD.CommandText = query;

            conn.Open();

            OleDbDataReader myReader = catCMD.ExecuteReader();

            return myReader;
        }
    }
}
