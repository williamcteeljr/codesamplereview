using PolicyTracker.DataAccess;
using PolicyTracker.Platform.UOW;
using System;
using System.Configuration;
using System.Collections;
using DynaFormWrapper;
using PolicyTracker.DataAccess.WCBindingPostingNotice;
using System.Collections.Generic;
using PolicyTracker.DomainModel.PostNotice;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Dynamic;
using Newtonsoft.Json;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;
using PolicyTracker.DomainModel.Policy;
using System.Web;

namespace PolicyTracker.BusinessServices
{
    public class PostingNoticeService
    {
        //Send Post to Ferg Tech - Server
        protected static string svr2 = System.Configuration.ConfigurationManager.AppSettings["valserver"];

        public static string SendPostingNotice(string TypeOfPayLoad, int RiskId, int SentById, int? LocationId = null, int? AdditionalInsuredId = null)
        {
            XmlDocument xmlDoc;
            int confirmationId = 0;
            string ConfirmationCode = "0";
            try
            {
                // ------------------------------------------------------------------------------------------------- \\
                // *** XML creation will pull data from the database  *** \\
                // ------------------------------------------------------------------------------------------------- \\

                #region  STEP 1: Insert Confirmation Record
                List<WCPnConfirmationDomain> confirmation = new List<WCPnConfirmationDomain>();
                confirmation = WCPNServiceDAOCalls.View.InsertConfirmation(RiskId, TypeOfPayLoad, "In-Progress", "", SentById.ToString());
                if (confirmation.Count > 0)
                {
                    confirmationId = Convert.ToInt32(confirmation[0].PNCONFIRMATION_ID);
                }
                #endregion

                #region  STEP 2: Build posting notices data model needed to build the XML Document
                WCGlobalPostingNoticesDomain wcg = new WCGlobalPostingNoticesDomain();
                if (string.IsNullOrEmpty(LocationId.ToString()) && string.IsNullOrEmpty(AdditionalInsuredId.ToString()))
                {
                    wcg = (WCGlobalPostingNoticesDomain)WCPNServiceDAOCalls.View.BuildPayLoad(RiskId, TypeOfPayLoad);
                }
                else if (string.IsNullOrEmpty(LocationId.ToString()) && !string.IsNullOrEmpty(AdditionalInsuredId.ToString()))
                {
                    //Endorse Additional Insured
                    wcg = (WCGlobalPostingNoticesDomain)WCPNServiceDAOCalls.View.BuildAdditionalInsuredPayLoad(RiskId, TypeOfPayLoad, Convert.ToInt32(AdditionalInsuredId));
                }
                else if (!string.IsNullOrEmpty(LocationId.ToString()) && string.IsNullOrEmpty(AdditionalInsuredId.ToString()))
                {
                    //Endorse Location
                    wcg = (WCGlobalPostingNoticesDomain)WCPNServiceDAOCalls.View.BuildLocationPayLoad(RiskId, TypeOfPayLoad, Convert.ToInt32(LocationId));
                }
                #endregion

                #region  STEP 3: Send New or Renewal Domain Model to XML Builder
                xmlDoc = new XmlDocument();
                //New Business or Renewal
                string xml = WCPNXmlHander.XmlBuilder.PNFullPayloadXMLDoc(wcg).InnerXml.ToString();
                xml = xml.Replace("\\", "");
                xmlDoc.LoadXml(xml);
                #endregion

                #region  STEP 4: Update Confirmaiton Record with Inner XML
                confirmation = new List<WCPnConfirmationDomain>();
                confirmation = WCPNServiceDAOCalls.View.UpdateConfirmation(RiskId, "In-Progress", "0", "", "0", "", DateTime.Now.ToString(), xmlDoc.InnerXml.ToString());

                #endregion

                #region  STEP 5: Initiate the card-creation process via a DynaForm web service call
                ConfirmationCode = callVSwrapper(xmlDoc, svr2.ToString(), RiskId, TypeOfPayLoad, "In-Progress", SentById.ToString(), confirmationId.ToString());
                #endregion

            }
            catch (Exception X)
            {
                onException(X, confirmationId);
            }

            return ConfirmationCode;
        }

        private static string callVSwrapper(XmlDocument xmldoc, string trgSvr, int RiskId, string SubmissionType, string Status, string SentById, string Id)
        {
            // Image array list
            ArrayList alImages = new ArrayList();

            // Start the clock...
            DateTime dTs = new DateTime(DateTime.Now.Ticks);
            string strXMLrs = string.Empty;

            // Define the data needed to make a successful web-service request - see Web.config for current definitions
            WrapperData wD = new WrapperData();

            wD.xmlSrc = xmldoc.InnerXml;                                    // The DynaForm compliant XML file as a string
            wD.cmpStr = ConfigurationManager.AppSettings["comp"];          // Company str; req'd for web-service (ws) authentication
            wD.tokTim = ConfigurationManager.AppSettings["time"];          // Time-stamp str; req'd for ws authentication
            wD.clntTk = ConfigurationManager.AppSettings["toke"];          // Client-based sec tok - req'd for ws authentication
            wD.trgSvr = trgSvr;                                             // The code specifying which server to target for request

            // Create a new instance of the DyanFormWrapper class (DynaFormWrapper.dll)
            DynaFormWrapper.CallWSE objVWS = new DynaFormWrapper.CallWSE();

            // Call the DynaForm wrapper which will issue the web service request
            strXMLrs = objVWS.callValetService(wD);

            //Format Response Message
            string message = string.Format(strXMLrs, System.Environment.NewLine);

            //Parse Response XML
            XDocument doc = XDocument.Parse(message);
            string jsonText = JsonConvert.SerializeXNode(doc);
            JObject MetaData = JObject.Parse(jsonText);
            string ErrorMessage = string.Empty;
            string totallocationcount = "0";

            //Confirmation Code
            string ConfirmationCode = MetaData["VWS-Response"]["Transactions"]["Transaction"]["confirmationcode"].ToString();
            if (ConfirmationCode.ToString() == "0")
            {
                //Update Status
                Status = "Error";
                //Get Error Message
                string errorMeta = MetaData["VWS-Response"]["Transactions"]["Transaction"]["Errors"]["error"].ToString();
                //Read Json with JsonTextReader
                JsonTextReader reader = new JsonTextReader(new StringReader(errorMeta));
                while (reader.Read())
                {
                    if (reader.Value != null)
                    {
                        if (reader.Value.ToString().Contains("Line"))
                        {
                            ErrorMessage += reader.Value + " ";
                        }
                    }
                }
                //Format Error Message
                ErrorMessage = onErrorMessageException(ErrorMessage.ToString());
            }
            else
            {
                //Update Status
                Status = "Complete";

                //Get Total Location Count
                totallocationcount = MetaData["VWS-Response"]["Transactions"]["Transaction"]["totallocationcount"].ToString();
            }

            //Update Confirmation
            List<WCPnConfirmationDomain> confirmation = new List<WCPnConfirmationDomain>();
            confirmation = WCPNServiceDAOCalls.View.UpdateConfirmation(Convert.ToInt32(Id), Status, ConfirmationCode, message, totallocationcount, ErrorMessage, DateTime.Now.ToString(), xmldoc.InnerXml);

            //Return Confirmation Code
            return ConfirmationCode.ToString();
        }

        // Exception handler
        private static void onException(Exception X, int ConfirmationId)
        {
            if (string.Compare(X.Message, "Thread was being aborted.") != 0)   //Ignore the thread abort exception :: thrown by Response.End()
            {
                //Provide error information here...
                string errorDIVS = "<div style=\"font-family:verdana;font-size:8pt;width:415px;border:1px solid black;padding:5px;\";>";

                string errorHTML = "<HTML><BODY>!";
                errorHTML += "$<br/><br/><b>@</b><br/><br/><b>^</b><br/><br/>#</div></BODY></HTML>";

                string errorMSSG = "An error occurred in the application. Please review the error message below and report ";
                errorMSSG += "it to your application administrator.";

                errorHTML = errorHTML.Replace("!", errorDIVS);
                errorHTML = errorHTML.Replace("$", errorMSSG);
                errorHTML = errorHTML.Replace("@", X.Message);
                errorHTML = errorHTML.Replace("^", X.StackTrace);

                //Update Confirmation With Error Message
                List<WCPnConfirmationDomain> confirmation = new List<WCPnConfirmationDomain>();
                confirmation = WCPNServiceDAOCalls.View.ErrorUpdateConfirmation(ConfirmationId, "Error", errorHTML);
            }
        }
        private static string onErrorMessageException(string expection)
        {
            string errorHTML = string.Empty;
            if (string.Compare(expection, "Thread was being aborted.") != 0)   //Ignore the thread abort exception :: thrown by Response.End()
            {
                //Provide error information here...
                string errorDIVS = "<div style=\"font-family:verdana;font-size:8pt;width:415px;border:1px solid black;padding:5px;\";>";

                errorHTML = "<HTML><BODY>!";
                errorHTML += "$<br/><br/><b>@</b><br/><br/><b>^</b><br/><br/>#</div></BODY></HTML>";

                string errorMSSG = "An error occurred in the application. Please review the error message below and report ";
                errorMSSG += "it to your application administrator.";

                errorHTML = errorHTML.Replace("!", errorDIVS);
                errorHTML = errorHTML.Replace("$", errorMSSG);
                errorHTML = errorHTML.Replace("@", expection);
                errorHTML = errorHTML.Replace("^", expection);
            }
            return errorHTML;
        }
    }

    public class WCPNServiceDAOCalls
    {
        public class View
        {
            //Build New Business and Renewal Posting Data Model
            public static WCGlobalPostingNoticesDomain BuildPayLoad(int RiskID, string TypeOfPayLoad)
            {
                //Declare Object Entity
                WCGlobalPostingNoticesDomain WCGPND = new WCGlobalPostingNoticesDomain();
                WCPnPrimaryContactDomain uwb = new WCPnPrimaryContactDomain();
                WCPnPolicyDomain policies = new WCPnPolicyDomain();
                List<WCPnLocationDomain> locations = new List<WCPnLocationDomain>();
                List<WCPnPolicyDomain> pol = new List<WCPnPolicyDomain>();
                string transactiontype = TypeOfPayLoad.ToUpper() == "NEW" ? "NB" : "RN";
                List<WCPnInsuredDomain> insured = new List<WCPnInsuredDomain>();

                //Set the Version
                WCGPND.version = "041708.1100v0";

                #region Step 1: Set Primary Contact Information
                List<WCPnPrimaryContactDomain> pc = new List<WCPnPrimaryContactDomain>();
                pc = WCPNServiceDAOCalls.View.getUWBranchLocation("ATL");
                if (pc.Count > 0)
                {
                    uwb.companyName = "Old Republic Aerospace, Inc.";
                    uwb.contactName = System.Configuration.ConfigurationManager.AppSettings["valtechCont"];  //Technical Contact
                    uwb.streetAddress1 = string.IsNullOrEmpty(pc[0].streetAddress1) ? "1990 Vaughn Road" : pc[0].streetAddress1.ToString();
                    uwb.streetAddress2 = string.IsNullOrEmpty(pc[0].streetAddress2) ? "Suite 350" : pc[0].streetAddress2.ToString();
                    uwb.city = string.IsNullOrEmpty(pc[0].city) ? "Kennesaw" : pc[0].city.ToString();
                    uwb.state = string.IsNullOrEmpty(pc[0].state) ? "GA" : pc[0].state.ToString();
                    uwb.zip = string.IsNullOrEmpty(pc[0].zip) ? "30144" : pc[0].zip.ToString();
                    uwb.mobilePhone = "(770) 590-4950";
                    uwb.officePhone = "(770) 590-4950";
                    uwb.email = System.Configuration.ConfigurationManager.AppSettings["valtechEmail"].Trim();
                }

                //Add Primary Contact to Global object
                WCGPND.primaryContact = uwb;
                #endregion

                //Get Main Insured Only if location are selected
                locations = getMainInsuredLocations(RiskID);
                if (locations.Count > 0)
                {
                    #region Step 2: Set Named Insured and Shipping info
                    insured = getInsuredInformmation(RiskID);
                    if (insured.Count > 0)
                    {
                        for (int i = 0; i < insured.Count; i++)
                        {
                            //Add Insured Data
                            string orgname1 = string.IsNullOrEmpty(insured[i].orgname1) ? "" : insured[i].orgname1.ToString();
                            string fein = string.IsNullOrEmpty(insured[i].fein) ? "" : insured[i].fein.ToString();
                            string StreetAddress1 = string.IsNullOrEmpty(insured[i].StreetAddress1) ? "" : insured[i].StreetAddress1.ToString();
                            string StreetAddress2 = string.IsNullOrEmpty(insured[i].StreetAddress2) ? "" : insured[i].StreetAddress2.ToString();
                            string City = string.IsNullOrEmpty(insured[i].City) ? "" : insured[i].City.ToString();
                            string State = string.IsNullOrEmpty(insured[i].State) ? "" : insured[i].State.ToString();
                            string Zip = string.IsNullOrEmpty(insured[i].Zip) ? "" : insured[i].Zip.ToString();
                            string shiptoinsuredaddr = string.IsNullOrEmpty(insured[i].shiptoinsuredaddr) ? "" : insured[i].shiptoinsuredaddr.ToString();
                            string shipmentmethod = string.IsNullOrEmpty(insured[i].shipmentmethod) ? "" : insured[i].shipmentmethod.ToString();
                            string AgencyName = string.IsNullOrEmpty(insured[i].AgencyName) ? "" : insured[i].AgencyName.ToString();
                            string brokerStreet1 = string.IsNullOrEmpty(insured[i].brokerStreet1) ? "" : insured[i].brokerStreet1.ToString();
                            string brokerStreet2 = string.IsNullOrEmpty(insured[i].brokerStreet2) ? "" : insured[i].brokerStreet2.ToString();
                            string brokerCity = string.IsNullOrEmpty(insured[i].brokerCity) ? "" : insured[i].brokerCity.ToString();
                            string brokerZip = string.IsNullOrEmpty(insured[i].brokerZip) ? "" : insured[i].brokerZip.ToString();
                            string brokerState = string.IsNullOrEmpty(insured[i].brokerState) ? "" : insured[i].brokerState.ToString();
                            string coverletterid = string.IsNullOrEmpty(insured[i].coverletterid) ? "" : insured[i].coverletterid.ToString();


                            //Get Policy Info
                            if (!string.IsNullOrEmpty(State))
                            {
                                pol = getPolicyInformmation(RiskID, "ATL", State.Trim());
                            }

                            //Add Insured to Global Object
                            WCGPND.Insured.Add(new WCPnInsuredDomain(transactiontype, orgname1, fein, StreetAddress1,
                                StreetAddress2, City, State, Zip, shiptoinsuredaddr, shipmentmethod, pol, locations,
                                AgencyName, brokerStreet1, brokerStreet2, brokerCity, brokerZip, brokerState, coverletterid));

                        }
                    }
                    #endregion
                }

                //Get Additional Insured Only
                #region Step 3: Get Additional Insureds only
                List<WCPnInsuredDomain> additionalInsured = new List<WCPnInsuredDomain>();
                additionalInsured = getAdditionalInsuredInformation(RiskID);
                if (additionalInsured.Count > 0)
                {
                    for (int i = 0; i < additionalInsured.Count; i++)
                    {
                        //Add Additional Insured Data
                        string ID = additionalInsured[i].ID.ToString();
                        string orgname1 = string.IsNullOrEmpty(additionalInsured[i].orgname1) ? "" : additionalInsured[i].orgname1.ToString();
                        string fein = string.IsNullOrEmpty(additionalInsured[i].fein) ? "" : additionalInsured[i].fein.ToString();
                        string StreetAddress1 = string.IsNullOrEmpty(additionalInsured[i].StreetAddress1) ? "" : additionalInsured[i].StreetAddress1.ToString();
                        string StreetAddress2 = string.IsNullOrEmpty(additionalInsured[i].StreetAddress2) ? "" : additionalInsured[i].StreetAddress2.ToString();
                        string City = string.IsNullOrEmpty(additionalInsured[i].City) ? "" : additionalInsured[i].City.ToString();
                        string State = string.IsNullOrEmpty(additionalInsured[i].State) ? "" : additionalInsured[i].State.ToString();
                        string Zip = string.IsNullOrEmpty(additionalInsured[i].Zip) ? "" : additionalInsured[i].Zip.ToString();
                        string shiptoadditionalInsuredaddr = string.IsNullOrEmpty(additionalInsured[i].shiptoinsuredaddr) ? "" : additionalInsured[i].shiptoinsuredaddr.ToString();
                        string shipmentmethod = string.IsNullOrEmpty(additionalInsured[i].shipmentmethod) ? "" : additionalInsured[i].shipmentmethod.ToString();
                        string AgencyName = string.IsNullOrEmpty(additionalInsured[i].AgencyName) ? "" : additionalInsured[i].AgencyName.ToString();
                        string brokerStreet1 = string.IsNullOrEmpty(additionalInsured[i].brokerStreet1) ? "" : additionalInsured[i].brokerStreet1.ToString();
                        string brokerStreet2 = string.IsNullOrEmpty(additionalInsured[i].brokerStreet2) ? "" : additionalInsured[i].brokerStreet2.ToString();
                        string brokerCity = string.IsNullOrEmpty(additionalInsured[i].brokerCity) ? "" : additionalInsured[i].brokerCity.ToString();
                        string brokerZip = string.IsNullOrEmpty(additionalInsured[i].brokerZip) ? "" : additionalInsured[i].brokerZip.ToString();
                        string brokerState = string.IsNullOrEmpty(additionalInsured[i].brokerState) ? "" : additionalInsured[i].brokerState.ToString();
                        string coverletterid = string.IsNullOrEmpty(additionalInsured[i].coverletterid) ? "" : additionalInsured[i].coverletterid.ToString();

                        //Get Policy Info
                        if (!string.IsNullOrEmpty(State))
                        {
                            pol = getPolicyInformmation(RiskID, "ATL", State.Trim());
                        }

                        //Get additional Insured Location Info
                        locations = getAdditionallocationInformmation(Convert.ToInt32(ID));

                        //Add Additional Insured to Global Object
                        WCGPND.Insured.Add(new WCPnInsuredDomain(transactiontype, orgname1, fein, StreetAddress1,
                            StreetAddress2, City, State, Zip, shiptoadditionalInsuredaddr, shipmentmethod, pol, locations,
                            AgencyName, brokerStreet1, brokerStreet2, brokerCity, brokerZip, brokerState, coverletterid));
                    }
                }
                #endregion

                return WCGPND;
            }

            public static WCGlobalPostingNoticesDomain BuildLocationPayLoad(int RiskID, string TypeOfPayLoad, int LocationId)
            {
                //Declare Object Entity
                WCGlobalPostingNoticesDomain WCGPND = new WCGlobalPostingNoticesDomain();
                WCPnPrimaryContactDomain uwb = new WCPnPrimaryContactDomain();
                WCPnPolicyDomain policies = new WCPnPolicyDomain();
                List<WCPnLocationDomain> locations = new List<WCPnLocationDomain>();
                List<WCPnPolicyDomain> pol = new List<WCPnPolicyDomain>();
                string transactiontype = "EN";

                //Set the Version
                WCGPND.version = "041708.1100v0";

                #region Step 1: Set Primary Contact Information
                List<WCPnPrimaryContactDomain> pc = new List<WCPnPrimaryContactDomain>();
                pc = WCPNServiceDAOCalls.View.getUWBranchLocation("ATL");
                if (pc.Count > 0)
                {
                    uwb.companyName = "Old Republic Aerospace, Inc.";
                    uwb.contactName = System.Configuration.ConfigurationManager.AppSettings["valtechCont"];  //Technical Contact
                    uwb.streetAddress1 = string.IsNullOrEmpty(pc[0].streetAddress1) ? "1990 Vaughn Road" : pc[0].streetAddress1.ToString();
                    uwb.streetAddress2 = string.IsNullOrEmpty(pc[0].streetAddress2) ? "Suite 350" : pc[0].streetAddress2.ToString();
                    uwb.city = string.IsNullOrEmpty(pc[0].city) ? "Kennesaw" : pc[0].city.ToString();
                    uwb.state = string.IsNullOrEmpty(pc[0].state) ? "GA" : pc[0].state.ToString();
                    uwb.zip = string.IsNullOrEmpty(pc[0].zip) ? "30144" : pc[0].zip.ToString();
                    uwb.mobilePhone = "(770) 590-4950";
                    uwb.officePhone = "(770) 590-4950";
                    uwb.email = System.Configuration.ConfigurationManager.AppSettings["valtechEmail"].Trim();
                }

                //Add Primary Contact to Global object
                WCGPND.primaryContact = uwb;
                #endregion

                #region Step 2: Set Named Insured and Shipping info
                List<WCPnInsuredDomain> insured = new List<WCPnInsuredDomain>();
                insured = getInsuredInformmation(RiskID);
                if (insured.Count > 0)
                {
                    for (int i = 0; i < insured.Count; i++)
                    {
                        //Add Insured Data
                        string orgname1 = string.IsNullOrEmpty(insured[i].orgname1) ? "" : insured[i].orgname1.ToString();
                        string fein = string.IsNullOrEmpty(insured[i].fein) ? "" : insured[i].fein.ToString();
                        string StreetAddress1 = string.IsNullOrEmpty(insured[i].StreetAddress1) ? "" : insured[i].StreetAddress1.ToString();
                        string StreetAddress2 = string.IsNullOrEmpty(insured[i].StreetAddress2) ? "" : insured[i].StreetAddress2.ToString();
                        string City = string.IsNullOrEmpty(insured[i].City) ? "" : insured[i].City.ToString();
                        string State = string.IsNullOrEmpty(insured[i].State) ? "" : insured[i].State.ToString();
                        string Zip = string.IsNullOrEmpty(insured[i].Zip) ? "" : insured[i].Zip.ToString();
                        string shiptoinsuredaddr = string.IsNullOrEmpty(insured[i].shiptoinsuredaddr) ? "" : insured[i].shiptoinsuredaddr.ToString();
                        string shipmentmethod = string.IsNullOrEmpty(insured[i].shipmentmethod) ? "" : insured[i].shipmentmethod.ToString();
                        string AgencyName = string.IsNullOrEmpty(insured[i].AgencyName) ? "" : insured[i].AgencyName.ToString();
                        string brokerStreet1 = string.IsNullOrEmpty(insured[i].brokerStreet1) ? "" : insured[i].brokerStreet1.ToString();
                        string brokerStreet2 = string.IsNullOrEmpty(insured[i].brokerStreet2) ? "" : insured[i].brokerStreet2.ToString();
                        string brokerCity = string.IsNullOrEmpty(insured[i].brokerCity) ? "" : insured[i].brokerCity.ToString();
                        string brokerZip = string.IsNullOrEmpty(insured[i].brokerZip) ? "" : insured[i].brokerZip.ToString();
                        string brokerState = string.IsNullOrEmpty(insured[i].brokerState) ? "" : insured[i].brokerState.ToString();
                        string coverletterid = string.IsNullOrEmpty(insured[i].coverletterid) ? "" : insured[i].coverletterid.ToString();

                        //Get Policy Info
                        if (!string.IsNullOrEmpty(State))
                        {
                            pol = getPolicyInformmation(RiskID, "ATL", State.Trim());
                        }

                        //Get Location Info By Id
                        locations = getLocationById(LocationId);

                        //Add Insured to Global Object
                        WCGPND.Insured.Add(new WCPnInsuredDomain(transactiontype, orgname1, fein, StreetAddress1,
                            StreetAddress2, City, State, Zip, shiptoinsuredaddr, shipmentmethod, pol, locations,
                            AgencyName, brokerStreet1, brokerStreet2, brokerCity, brokerZip, brokerState, coverletterid));

                    }
                }


                #endregion

                return WCGPND;
            }

            public static WCGlobalPostingNoticesDomain BuildAdditionalInsuredPayLoad(int RiskID, string TypeOfPayLoad, int AdditionalInsuredId)
            {
                //Declare Object Entity
                WCGlobalPostingNoticesDomain WCGPND = new WCGlobalPostingNoticesDomain();
                WCPnPrimaryContactDomain uwb = new WCPnPrimaryContactDomain();
                WCPnPolicyDomain policies = new WCPnPolicyDomain();
                List<WCPnLocationDomain> locations = new List<WCPnLocationDomain>();
                List<WCPnPolicyDomain> pol = new List<WCPnPolicyDomain>();
                string transactiontype = "EN";

                //Set the Version
                WCGPND.version = "041708.1100v0";

                #region Step 1: Set Primary Contact Information
                List<WCPnPrimaryContactDomain> pc = new List<WCPnPrimaryContactDomain>();
                pc = WCPNServiceDAOCalls.View.getUWBranchLocation("ATL");
                if (pc.Count > 0)
                {
                    uwb.companyName = "Old Republic Aerospace, Inc.";
                    uwb.contactName = System.Configuration.ConfigurationManager.AppSettings["valtechCont"];  //Technical Contact
                    uwb.streetAddress1 = string.IsNullOrEmpty(pc[0].streetAddress1) ? "1990 Vaughn Road" : pc[0].streetAddress1.ToString();
                    uwb.streetAddress2 = string.IsNullOrEmpty(pc[0].streetAddress2) ? "Suite 350" : pc[0].streetAddress2.ToString();
                    uwb.city = string.IsNullOrEmpty(pc[0].city) ? "Kennesaw" : pc[0].city.ToString();
                    uwb.state = string.IsNullOrEmpty(pc[0].state) ? "GA" : pc[0].state.ToString();
                    uwb.zip = string.IsNullOrEmpty(pc[0].zip) ? "30144" : pc[0].zip.ToString();
                    uwb.mobilePhone = "(770) 590-4950";
                    uwb.officePhone = "(770) 590-4950";
                    uwb.email = System.Configuration.ConfigurationManager.AppSettings["valtechEmail"].Trim();
                }

                //Add Primary Contact to Global object
                WCGPND.primaryContact = uwb;
                #endregion

                #region Step 2: Get Additional Insureds only
                List<WCPnInsuredDomain> insured = new List<WCPnInsuredDomain>();
                List<WCPnInsuredDomain> additionalInsured = new List<WCPnInsuredDomain>();
                additionalInsured = getAdditionalInsuredById(AdditionalInsuredId);
                if (additionalInsured.Count > 0)
                {
                    for (int i = 0; i < additionalInsured.Count; i++)
                    {
                        //Add Additional Insured Data
                        string ID = additionalInsured[i].ID.ToString();
                        string orgname1 = string.IsNullOrEmpty(additionalInsured[i].orgname1) ? "" : additionalInsured[i].orgname1.ToString();
                        string fein = string.IsNullOrEmpty(additionalInsured[i].fein) ? "" : additionalInsured[i].fein.ToString();
                        string StreetAddress1 = string.IsNullOrEmpty(additionalInsured[i].StreetAddress1) ? "" : additionalInsured[i].StreetAddress1.ToString();
                        string StreetAddress2 = string.IsNullOrEmpty(additionalInsured[i].StreetAddress2) ? "" : additionalInsured[i].StreetAddress2.ToString();
                        string City = string.IsNullOrEmpty(additionalInsured[i].City) ? "" : additionalInsured[i].City.ToString();
                        string State = string.IsNullOrEmpty(additionalInsured[i].State) ? "" : additionalInsured[i].State.ToString();
                        string Zip = string.IsNullOrEmpty(additionalInsured[i].Zip) ? "" : additionalInsured[i].Zip.ToString();
                        string shiptoadditionalInsuredaddr = string.IsNullOrEmpty(additionalInsured[i].shiptoinsuredaddr) ? "" : additionalInsured[i].shiptoinsuredaddr.ToString();
                        string shipmentmethod = string.IsNullOrEmpty(additionalInsured[i].shipmentmethod) ? "" : additionalInsured[i].shipmentmethod.ToString();
                        string AgencyName = string.IsNullOrEmpty(additionalInsured[i].AgencyName) ? "" : additionalInsured[i].AgencyName.ToString();
                        string brokerStreet1 = string.IsNullOrEmpty(additionalInsured[i].brokerStreet1) ? "" : additionalInsured[i].brokerStreet1.ToString();
                        string brokerStreet2 = string.IsNullOrEmpty(additionalInsured[i].brokerStreet2) ? "" : additionalInsured[i].brokerStreet2.ToString();
                        string brokerCity = string.IsNullOrEmpty(additionalInsured[i].brokerCity) ? "" : additionalInsured[i].brokerCity.ToString();
                        string brokerZip = string.IsNullOrEmpty(additionalInsured[i].brokerZip) ? "" : additionalInsured[i].brokerZip.ToString();
                        string brokerState = string.IsNullOrEmpty(additionalInsured[i].brokerState) ? "" : additionalInsured[i].brokerState.ToString();
                        string coverletterid = string.IsNullOrEmpty(additionalInsured[i].coverletterid) ? "" : additionalInsured[i].coverletterid.ToString();

                        //Get Policy Info
                        if (!string.IsNullOrEmpty(State))
                        {
                            pol = getPolicyInformmation(RiskID, "ATL", State.Trim());
                        }

                        //Get additional Insured Location Info
                        locations = getAdditionallocationInformmation(Convert.ToInt32(ID));

                        //Add Additional Insured to Global Object
                        WCGPND.Insured.Add(new WCPnInsuredDomain(transactiontype, orgname1, fein, StreetAddress1,
                            StreetAddress2, City, State, Zip, shiptoadditionalInsuredaddr, shipmentmethod, pol, locations,
                            AgencyName, brokerStreet1, brokerStreet2, brokerCity, brokerZip, brokerState, coverletterid));

                    }
                }
                #endregion

                return WCGPND;
            }

            public static List<WCPnPrimaryContactDomain> getUWBranchLocation(string BranchID)
            {
                var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
                var dao = DAOFactory.GetDAO<WCPnPrimaryContactDomainDAO>();
                var results = dao.getUWbranch(uow, BranchID).ToList();
                return results;
            }

            public static List<WCPnInsuredDomain> getInsuredInformmation(int RiskId)
            {
                var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
                var dao = DAOFactory.GetDAO<WCPnInsuredDomainDAO>();
                var results = dao.getInsuredInformation(uow, RiskId.ToString()).ToList();
                return results;
            }

            public static List<WCPnPolicyDomain> getPolicyInformmation(int RiskId, string BranchId, string State)
            {
                var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
                var dao = DAOFactory.GetDAO<WCPnInsuredDomainDAO>();
                var results = dao.getPolicyInformation(uow, RiskId.ToString(), BranchId.ToString(), State).ToList();
                return results;
            }

            public static List<WCPnLocationDomain> getPolicylocationInformmation(int RiskId)
            {
                var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
                var dao = DAOFactory.GetDAO<WCPnInsuredDomainDAO>();
                var results = dao.getPolicyInsuredLocation(uow, RiskId.ToString()).ToList();
                return results;
            }

            public static List<WCPnLocationDomain> getAlllocations(int RiskId)
            {
                var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
                var dao = DAOFactory.GetDAO<WCPnInsuredDomainDAO>();
                var results = dao.getAllLocation(uow, RiskId.ToString()).ToList();
                return results;
            }

            public static List<WCPnConfirmationDomain> InsertConfirmation(int RiskId, string SubmissionType, string Status, string Submission, string SentById)
            {
                var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
                var dao = DAOFactory.GetDAO<WCPnInsuredDomainDAO>();
                var results = dao.InsertConfirmation(uow, RiskId.ToString(), SubmissionType, Status, Submission, SentById).ToList();
                return results;
            }

            public static List<WCPnConfirmationDomain> UpdateConfirmation(int Id, string Status, string ConfirmationCode, string Response, string TotalLocationCount, string ErrorMessage, string ResponseDate, string Submission)
            {
                var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
                var dao = DAOFactory.GetDAO<WCPnInsuredDomainDAO>();
                var results = dao.UpdateConfirmation(uow, Id.ToString(), Status, ConfirmationCode, Response, TotalLocationCount, ErrorMessage, ResponseDate, Submission).ToList();
                return results;
            }

            public static List<WCPnConfirmationDomain> ErrorUpdateConfirmation(int Id, string Status, string ErrorMessage)
            {
                var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
                var dao = DAOFactory.GetDAO<WCPnInsuredDomainDAO>();
                var results = dao.ErrorUpdateConfirmation(uow, Id.ToString(), Status, ErrorMessage).ToList();
                return results;
            }

            public static List<WCPnInsuredDomain> getAdditionalInsuredInformation(int RiskId)
            {
                var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
                var dao = DAOFactory.GetDAO<WCPnInsuredDomainDAO>();
                var results = dao.getAdditionalInsuredInformation(uow, RiskId.ToString()).ToList();
                return results;
            }

            public static List<WCPnLocationDomain> getAdditionallocationInformmation(int AdditionalInsuredId)
            {
                var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
                var dao = DAOFactory.GetDAO<WCPnInsuredDomainDAO>();
                var results = dao.getAdditionalInsuredLocation(uow, AdditionalInsuredId.ToString()).ToList();
                return results;
            }

            public static List<WCPnLocationDomain> getLocationById(int LocationId)
            {
                var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
                var dao = DAOFactory.GetDAO<WCPnInsuredDomainDAO>();
                var results = dao.getLocationById(uow, LocationId.ToString()).ToList();
                return results;
            }

            public static List<WCPnInsuredDomain> getAdditionalInsuredById(int AdditionalInsuredId)
            {
                var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
                var dao = DAOFactory.GetDAO<WCPnInsuredDomainDAO>();
                var results = dao.getAdditionalInsuredById(uow, AdditionalInsuredId.ToString()).ToList();
                return results;
            }

            public static List<WCPnLocationDomain> getMainInsuredLocations(int RiskId)
            {
                var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
                var dao = DAOFactory.GetDAO<WCPnInsuredDomainDAO>();
                var results = dao.getMainInsuredLocation(uow, RiskId.ToString()).ToList();
                return results;
            }
        }
    }

    public class WCPNXmlHander
    {
        public class XmlBuilder
        {
            public static XmlDocument PNFullPayloadXMLDoc(WCGlobalPostingNoticesDomain wcg)
            {

                //Create XML Document
                XmlDocument doc = new XmlDocument();

                XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "utf-8", null);
                XmlElement root = doc.DocumentElement;
                doc.InsertBefore(xmlDeclaration, root);

                #region Create Root Node

                XmlElement PostingNoticeInfo = doc.CreateElement(String.Empty, "postingnoticeinfo", String.Empty);
                doc.AppendChild(PostingNoticeInfo);

                #endregion

                #region Create <Version> Element
                if (!string.IsNullOrEmpty(wcg.version))
                {
                    XmlElement versionElement = doc.CreateElement("version");
                    XmlText versionText = doc.CreateTextNode(wcg.version.ToString().Trim());
                    versionElement.AppendChild(versionText);
                    PostingNoticeInfo.AppendChild(versionElement);
                }
                #endregion

                #region PrimaryContact Elements
                //Group Main Node
                XmlElement PrimaryContact = doc.CreateElement("primarycontact");
                PostingNoticeInfo.AppendChild(PrimaryContact);

                #region Create <CompanyName> Element
                if (!string.IsNullOrEmpty(wcg.primaryContact.companyName))
                {
                    XmlElement CompanyNameElement = doc.CreateElement("companyname");
                    XmlText CompanyNameText = doc.CreateTextNode(wcg.primaryContact.companyName.ToString().Trim());
                    CompanyNameElement.AppendChild(CompanyNameText);
                    PrimaryContact.AppendChild(CompanyNameElement);
                }

                #endregion

                #region Create <ContactName> Element
                if (!string.IsNullOrEmpty(wcg.primaryContact.contactName))
                {
                    XmlElement ContactNameElement = doc.CreateElement("contactname");
                    XmlText ContactNameText = doc.CreateTextNode(wcg.primaryContact.contactName.ToString().Trim());
                    ContactNameElement.AppendChild(ContactNameText);
                    PrimaryContact.AppendChild(ContactNameElement);
                }
                #endregion

                #region Create <Address1> Element
                if (!string.IsNullOrEmpty(wcg.primaryContact.streetAddress1))
                {
                    XmlElement Address1Element = doc.CreateElement("address1");
                    XmlText Address1Text = doc.CreateTextNode(wcg.primaryContact.streetAddress1.ToString().Trim());
                    Address1Element.AppendChild(Address1Text);
                    PrimaryContact.AppendChild(Address1Element);
                }
                #endregion

                #region Create <Address2> Element
                if (!string.IsNullOrEmpty(wcg.primaryContact.streetAddress2))
                {
                    XmlElement Address2Element = doc.CreateElement("address2");
                    XmlText Address2Text = doc.CreateTextNode(wcg.primaryContact.streetAddress2.ToString().Trim());
                    Address2Element.AppendChild(Address2Text);
                    PrimaryContact.AppendChild(Address2Element);
                }
                #endregion

                #region Create <City> Element
                if (!string.IsNullOrEmpty(wcg.primaryContact.city))
                {
                    XmlElement cityElement = doc.CreateElement("city");
                    XmlText cityText = doc.CreateTextNode(wcg.primaryContact.city.ToString().Trim());
                    cityElement.AppendChild(cityText);
                    PrimaryContact.AppendChild(cityElement);
                }
                #endregion

                #region Create <State> Element
                if (!string.IsNullOrEmpty(wcg.primaryContact.state))
                {
                    XmlElement stateElement = doc.CreateElement("state");
                    XmlText stateText = doc.CreateTextNode(wcg.primaryContact.state.ToString().Trim());
                    stateElement.AppendChild(stateText);
                    PrimaryContact.AppendChild(stateElement);
                }
                #endregion

                #region Create <PostalCode> Element
                if (!string.IsNullOrEmpty(wcg.primaryContact.zip))
                {
                    XmlElement postalcodeElement = doc.CreateElement("postalcode");
                    XmlText postalcodeText = doc.CreateTextNode(wcg.primaryContact.zip.ToString().Trim());
                    postalcodeElement.AppendChild(postalcodeText);
                    PrimaryContact.AppendChild(postalcodeElement);
                }
                #endregion

                #region Create <OfficePhone> Element
                XmlElement officephoneElement = doc.CreateElement("officephone");
                XmlText officephoneText = doc.CreateTextNode(string.IsNullOrEmpty(wcg.primaryContact.officePhone) ? "(000) 000-0000" : wcg.primaryContact.officePhone.ToString().Trim());
                officephoneElement.AppendChild(officephoneText);
                PrimaryContact.AppendChild(officephoneElement);
                #endregion

                #region Create <MobilePhone> Element
                XmlElement mobilephoneElement = doc.CreateElement("mobilephone");
                XmlText mobilephoneText = doc.CreateTextNode(string.IsNullOrEmpty(wcg.primaryContact.mobilePhone) ? "(000) 000-0000" : wcg.primaryContact.mobilePhone.ToString().Trim());
                mobilephoneElement.AppendChild(mobilephoneText);
                PrimaryContact.AppendChild(mobilephoneElement);
                #endregion

                #region Create <Email> Element
                XmlElement emailElement = doc.CreateElement("email");
                XmlText emailText = doc.CreateTextNode(string.IsNullOrEmpty(wcg.primaryContact.email) ? "" : wcg.primaryContact.email.ToString().Trim());
                emailElement.AppendChild(emailText);
                PrimaryContact.AppendChild(emailElement);
                #endregion
                #endregion

                #region Insured Elements
                //Parent
                XmlElement Insureds = doc.CreateElement("insureds");
                PostingNoticeInfo.AppendChild(Insureds);
                //Sub Parent
                XmlElement Insured = doc.CreateElement("insured");

                #region  Get All Insured Information - Insured Section
                if (wcg.Insured.Count > 0)
                {


                    for (int i = 0; i < wcg.Insured.Count; i++)
                    {
                        if (i > 0)
                        {
                            Insured = doc.CreateElement("insured");
                        }

                        #region Create <TransactionType> Child of Insured Element
                        if (!string.IsNullOrEmpty(wcg.Insured[i].transactiontype))
                        {
                            XmlElement TransactionTypeElement = doc.CreateElement("transactiontype");
                            XmlText TransactionTypeText = doc.CreateTextNode(wcg.Insured[i].transactiontype.ToString().Trim());
                            TransactionTypeElement.AppendChild(TransactionTypeText);
                            Insured.AppendChild(TransactionTypeElement);
                        }
                        #endregion

                        #region Create <OrgName1> Child of Insured Element
                        if (!string.IsNullOrEmpty(wcg.Insured[i].orgname1))
                        {
                            XmlElement OrgName1Element = doc.CreateElement("orgname1");
                            XmlText OrgName1Text = doc.CreateTextNode(wcg.Insured[i].orgname1.ToString().Trim());
                            OrgName1Element.AppendChild(OrgName1Text);
                            Insured.AppendChild(OrgName1Element);
                        }
                        #endregion

                        #region Create <FEIN>  Child of Insured Element
                        if (!string.IsNullOrEmpty(wcg.Insured[i].fein))
                        {
                            XmlElement FeinElement = doc.CreateElement("fein");
                            XmlText FeinText = doc.CreateTextNode(wcg.Insured[i].fein.ToString().Trim());
                            FeinElement.AppendChild(FeinText);
                            Insured.AppendChild(FeinElement);
                        }
                        #endregion

                        #region Create <InsuredAddress1> Child of Insured Element
                        if (!string.IsNullOrEmpty(wcg.Insured[i].StreetAddress1))
                        {
                            XmlElement address1Element = doc.CreateElement("address1");
                            XmlText address1Text = doc.CreateTextNode(wcg.Insured[i].StreetAddress1.ToString().Trim());
                            address1Element.AppendChild(address1Text);
                            Insured.AppendChild(address1Element);
                        }
                        #endregion

                        #region Create <InsuredAddress2> Child of Insured Element
                        if (!string.IsNullOrEmpty(wcg.Insured[i].StreetAddress2))
                        {
                            XmlElement address2Element = doc.CreateElement("address2");
                            XmlText address2Text = doc.CreateTextNode(wcg.Insured[i].StreetAddress2.ToString().Trim());
                            address2Element.AppendChild(address2Text);
                            Insured.AppendChild(address2Element);
                        }
                        #endregion

                        #region Create <City> Child of Insured Element
                        if (!string.IsNullOrEmpty(wcg.Insured[i].City))
                        {
                            XmlElement cityElement = doc.CreateElement("city");
                            XmlText cityText = doc.CreateTextNode(wcg.Insured[i].City.ToString().Trim());
                            cityElement.AppendChild(cityText);
                            Insured.AppendChild(cityElement);
                        }
                        #endregion

                        #region Create <InsuredState> Child of Insured Element
                        if (!string.IsNullOrEmpty(wcg.Insured[i].State))
                        {
                            XmlElement stateElement = doc.CreateElement("state");
                            XmlText stateText = doc.CreateTextNode(wcg.Insured[i].State.ToString().Trim());
                            stateElement.AppendChild(stateText);
                            Insured.AppendChild(stateElement);
                        }
                        #endregion

                        #region Create <PostalCode> Child of Insured Element
                        if (!string.IsNullOrEmpty(wcg.Insured[i].Zip))
                        {
                            XmlElement postalcodeElement = doc.CreateElement("postalcode");
                            XmlText postalcodeText = doc.CreateTextNode(wcg.Insured[i].Zip.ToString().Trim());
                            postalcodeElement.AppendChild(postalcodeText);
                            Insured.AppendChild(postalcodeElement);
                        }
                        #endregion

                        //Sub Add - Insured Section for Every Insured Item
                        Insureds.AppendChild(Insured);

                        #region ShippingInfo Elements
                        XmlElement ShippingInfo = doc.CreateElement("shippinginfo");
                        Insured.AppendChild(ShippingInfo);

                        #region Create <shiptoinsuredaddr> Child of ShippingInfo Element
                        if (!string.IsNullOrEmpty(wcg.Insured[i].shiptoinsuredaddr))
                        {
                            XmlElement shiptoinsuredaddrElement = doc.CreateElement("shiptoinsuredaddr");
                            XmlText shiptoinsuredaddrText = doc.CreateTextNode(wcg.Insured[i].shiptoinsuredaddr.ToString().Trim());
                            shiptoinsuredaddrElement.AppendChild(shiptoinsuredaddrText);
                            ShippingInfo.AppendChild(shiptoinsuredaddrElement);
                        }
                        #endregion

                        #region Create <shipmentmethod> Child of ShippingInfo Element
                        if (!string.IsNullOrEmpty(wcg.Insured[i].shipmentmethod))
                        {
                            XmlElement shipmentmethodElement = doc.CreateElement("shipmentmethod");
                            XmlText shipmentmethodText = doc.CreateTextNode(wcg.Insured[i].shipmentmethod.ToString().Trim());
                            shipmentmethodElement.AppendChild(shipmentmethodText);
                            ShippingInfo.AppendChild(shipmentmethodElement);
                        }
                        #endregion

                        #region Create <coverletterid> Child of ShippingInfo Element
                        if (!string.IsNullOrEmpty(wcg.Insured[i].coverletterid))
                        {
                            XmlElement coverletteridElement = doc.CreateElement("coverletterid");
                            XmlText coverletteridText = doc.CreateTextNode(wcg.Insured[i].coverletterid.ToString().Trim());
                            coverletteridElement.AppendChild(coverletteridText);
                            ShippingInfo.AppendChild(coverletteridElement);
                        }
                        #endregion

                        #region ShipTo Elements
                        XmlElement shipto = doc.CreateElement("shipto");
                        ShippingInfo.AppendChild(shipto);

                        #region Create <companyname> Child of ShipTo Element
                        if (!string.IsNullOrEmpty(wcg.Insured[i].AgencyName))
                        {
                            XmlElement companynameElement = doc.CreateElement("companyname");
                            XmlText companynameText = doc.CreateTextNode(wcg.Insured[i].AgencyName.ToString().Trim());
                            companynameElement.AppendChild(companynameText);
                            shipto.AppendChild(companynameElement);
                        }
                        #endregion

                        #region Create <address1> Child of ShipTo Element
                        if (!string.IsNullOrEmpty(wcg.Insured[i].brokerStreet1))
                        {
                            XmlElement brokerStreet1Element = doc.CreateElement("address1");
                            XmlText brokerStreet1Text = doc.CreateTextNode(wcg.Insured[i].brokerStreet1.ToString().Trim());
                            brokerStreet1Element.AppendChild(brokerStreet1Text);
                            shipto.AppendChild(brokerStreet1Element);
                        }
                        #endregion

                        #region Create <address2> Child of ShipTo Element
                        if (!string.IsNullOrEmpty(wcg.Insured[i].brokerStreet2.Trim()))
                        {
                            XmlElement brokerStreet2Element = doc.CreateElement("address2");
                            XmlText brokerStreet2Text = doc.CreateTextNode(wcg.Insured[i].brokerStreet2.ToString().Trim());
                            brokerStreet2Element.AppendChild(brokerStreet2Text);
                            shipto.AppendChild(brokerStreet2Element);
                        }
                        #endregion

                        #region Create <city> Child of ShipTo Element
                        if (!string.IsNullOrEmpty(wcg.Insured[i].brokerCity.Trim()))
                        {
                            XmlElement brokerCityElement = doc.CreateElement("city");
                            XmlText brokerCityText = doc.CreateTextNode(wcg.Insured[i].brokerCity.ToString().Trim());
                            brokerCityElement.AppendChild(brokerCityText);
                            shipto.AppendChild(brokerCityElement);
                        }
                        #endregion

                        #region Create <state> Child of ShipTo Element
                        if (!string.IsNullOrEmpty(wcg.Insured[i].brokerState.Trim()))
                        {
                            XmlElement stateElement = doc.CreateElement("state");
                            XmlText stateText = doc.CreateTextNode(wcg.Insured[i].brokerState.ToString().Trim());
                            stateElement.AppendChild(stateText);
                            shipto.AppendChild(stateElement);
                        }
                        #endregion

                        #region Create <postalcode> Child of ShipTo Element
                        if (!string.IsNullOrEmpty(wcg.Insured[i].brokerZip))
                        {
                            XmlElement postalcodeElement = doc.CreateElement("postalcode");
                            XmlText postalcodeText = doc.CreateTextNode(wcg.Insured[i].brokerZip.ToString().Trim());
                            postalcodeElement.AppendChild(postalcodeText);
                            shipto.AppendChild(postalcodeElement);
                        }
                        #endregion
                        #endregion

                        #region Policy Elements
                        //Policy Parent
                        XmlElement Policies = doc.CreateElement("policies");
                        Insured.AppendChild(Policies);

                        XmlElement Policy = doc.CreateElement("policy");

                        for (int z = 0; z < wcg.Insured[i].policy.Count; z++)
                        {
                            if (z > 0)
                            {
                                Policy = doc.CreateElement("policy");
                            }

                            #region Create <number> Child of Policy Element
                            if (!string.IsNullOrEmpty(wcg.Insured[i].policy[z].number))
                            {
                                XmlElement numberElement = doc.CreateElement("number");
                                XmlText numberText = doc.CreateTextNode(wcg.Insured[i].policy[z].number.ToString().Trim());
                                numberElement.AppendChild(numberText);
                                Policy.AppendChild(numberElement);
                            }
                            #endregion

                            #region Create <effectivedate> Child of Policy Element
                            if (!string.IsNullOrEmpty(wcg.Insured[i].policy[z].effectivedate))
                            {
                                XmlElement effectivedateElement = doc.CreateElement("effectivedate");
                                XmlText effectivedateText = doc.CreateTextNode(wcg.Insured[i].policy[z].effectivedate.ToString().Trim());
                                effectivedateElement.AppendChild(effectivedateText);
                                Policy.AppendChild(effectivedateElement);
                            }

                            #endregion

                            #region Create <expirationdate> Child of Policy Element
                            if (!string.IsNullOrEmpty(wcg.Insured[i].policy[z].effectivedate))
                            {
                                XmlElement expirationdateElement = doc.CreateElement("expirationdate");
                                XmlText expirationdateText = doc.CreateTextNode(wcg.Insured[i].policy[z].expirationdate.ToString().Trim());
                                expirationdateElement.AppendChild(expirationdateText);
                                Policy.AppendChild(expirationdateElement);
                            }
                            #endregion

                            #region Create <ReportClaimByContacting> Child of Policy Element
                            if (!string.IsNullOrEmpty(wcg.Insured[i].policy[z].reportclaimbycontacting))
                            {
                                XmlElement reportclaimbycontactingElement = doc.CreateElement("reportclaimbycontacting");
                                XmlText reportclaimbycontactingText = doc.CreateTextNode(wcg.Insured[i].policy[z].reportclaimbycontacting.ToString().Trim());
                                reportclaimbycontactingElement.AppendChild(reportclaimbycontactingText);
                                Policy.AppendChild(reportclaimbycontactingElement);
                            }
                            #endregion

                            //CARRIER INFO

                            #region Create <insconame> Child of Policy Element
                            if (!string.IsNullOrEmpty(wcg.Insured[i].policy[z].insconame))
                            {
                                XmlElement insconameElement = doc.CreateElement("insconame");
                                XmlText insconameText = doc.CreateTextNode(wcg.Insured[i].policy[z].insconame.ToString().Trim());
                                insconameElement.AppendChild(insconameText);
                                Policy.AppendChild(insconameElement);
                            }
                            #endregion

                            #region Create <naicnumber> Child of Policy Element
                            if (!string.IsNullOrEmpty(wcg.Insured[i].policy[z].naicnumber))
                            {
                                XmlElement naicnumberElement = doc.CreateElement("naicnumber");
                                XmlText insconameText = doc.CreateTextNode(wcg.Insured[i].policy[z].naicnumber.ToString().Trim());
                                naicnumberElement.AppendChild(insconameText);
                                Policy.AppendChild(naicnumberElement);
                            }
                            #endregion

                            #region Create <inscoaddress1> Child of Policy Element
                            if (!string.IsNullOrEmpty(wcg.Insured[i].policy[z].inscoaddress1))
                            {
                                XmlElement inscoaddress1Element = doc.CreateElement("inscoaddress1");
                                XmlText inscoaddress1Text = doc.CreateTextNode(wcg.Insured[i].policy[z].inscoaddress1.ToString().Trim());
                                inscoaddress1Element.AppendChild(inscoaddress1Text);
                                Policy.AppendChild(inscoaddress1Element);
                            }

                            #endregion

                            #region Create <InsCoCity> Child of Policy Element
                            if (!string.IsNullOrEmpty(wcg.Insured[i].policy[z].inscocity))
                            {
                                XmlElement inscocityElement = doc.CreateElement("inscocity");
                                XmlText inscocityText = doc.CreateTextNode(wcg.Insured[i].policy[z].inscocity.ToString().Trim());
                                inscocityElement.AppendChild(inscocityText);
                                Policy.AppendChild(inscocityElement);
                            }
                            #endregion

                            #region Create <inscostate> Child of Policy Element
                            if (!string.IsNullOrEmpty(wcg.Insured[i].policy[z].inscostate))
                            {
                                XmlElement inscostateElement = doc.CreateElement("inscostate");
                                XmlText inscostateText = doc.CreateTextNode(wcg.Insured[i].policy[z].inscostate.ToString().Trim());
                                inscostateElement.AppendChild(inscostateText);
                                Policy.AppendChild(inscostateElement);
                            }
                            #endregion

                            #region Create <inscopostalcode> Child of Policy Element
                            if (!string.IsNullOrEmpty(wcg.Insured[i].policy[z].inscopostalcode))
                            {
                                XmlElement inscopostalcodeElement = doc.CreateElement("inscopostalcode");
                                XmlText inscopostalcodeText = doc.CreateTextNode(wcg.Insured[i].policy[z].inscopostalcode.ToString().Trim());
                                inscopostalcodeElement.AppendChild(inscopostalcodeText);
                                Policy.AppendChild(inscopostalcodeElement);
                            }
                            #endregion

                            #region Create <inscophone> Child of Policy Element
                            if (!string.IsNullOrEmpty(wcg.Insured[i].policy[z].inscophone))
                            {
                                XmlElement inscophoneElement = doc.CreateElement("inscophone");
                                XmlText inscophoneText = doc.CreateTextNode(wcg.Insured[i].policy[z].inscophone.ToString().Trim());
                                inscophoneElement.AppendChild(inscophoneText);
                                Policy.AppendChild(inscophoneElement);
                            }
                            #endregion

                            //Broker INFO
                            #region Create <brokerorgname> Child of Policy Element
                            if (!string.IsNullOrEmpty(wcg.Insured[i].policy[z].brokerorgname))
                            {
                                XmlElement brokerorgnameElement = doc.CreateElement("brokerorgname");
                                XmlText brokerorgnameText = doc.CreateTextNode(wcg.Insured[i].policy[z].brokerorgname.ToString().Trim());
                                brokerorgnameElement.AppendChild(brokerorgnameText);
                                Policy.AppendChild(brokerorgnameElement);
                            }
                            #endregion

                            #region Create <brokerorgrepname> Child of Policy Element
                            if (!string.IsNullOrEmpty(wcg.Insured[i].policy[z].brokerorgrepname))
                            {
                                XmlElement brokerorgrepnameElement = doc.CreateElement("brokerorgrepname");
                                XmlText brokerorgrepnameText = doc.CreateTextNode(wcg.Insured[i].policy[z].brokerorgrepname.ToString().Trim());
                                brokerorgrepnameElement.AppendChild(brokerorgrepnameText);
                                Policy.AppendChild(brokerorgrepnameElement);
                            }
                            #endregion

                            #region Create <brokerorgaddress1> Child of Policy Element
                            if (!string.IsNullOrEmpty(wcg.Insured[i].policy[z].brokerorgaddress1))
                            {
                                XmlElement brokerorgaddress1Element = doc.CreateElement("brokerorgaddress1");
                                XmlText brokerorgaddress1Text = doc.CreateTextNode(wcg.Insured[i].policy[z].brokerorgaddress1.ToString().Trim());
                                brokerorgaddress1Element.AppendChild(brokerorgaddress1Text);
                                Policy.AppendChild(brokerorgaddress1Element);
                            }
                            #endregion

                            #region Create <brokerorgreptitle> Child of Policy Element
                            if (!string.IsNullOrEmpty(wcg.Insured[i].policy[z].brokerorgreptitle.Trim()))
                            {
                                XmlElement brokerorgreptitle = doc.CreateElement("brokerorgreptitle");
                                XmlText brokerorgreptitleText = doc.CreateTextNode(wcg.Insured[i].policy[z].brokerorgreptitle.ToString().Trim());
                                brokerorgreptitle.AppendChild(brokerorgreptitleText);
                                Policy.AppendChild(brokerorgreptitle);
                            }
                            #endregion

                            #region Create <brokerorgcity> Child of Policy Element
                            if (!string.IsNullOrEmpty(wcg.Insured[i].policy[z].brokerorgcity.Trim()))
                            {
                                XmlElement brokerorgcityElement = doc.CreateElement("brokerorgcity");
                                XmlText brokerorgcityText = doc.CreateTextNode(wcg.Insured[i].policy[z].brokerorgcity.ToString().Trim());
                                brokerorgcityElement.AppendChild(brokerorgcityText);
                                Policy.AppendChild(brokerorgcityElement);
                            }
                            #endregion

                            #region Create <brokerorgstate> Child of Policy Element
                            if (!string.IsNullOrEmpty(wcg.Insured[i].policy[z].brokerorgstate))
                            {
                                XmlElement brokerorgstateElement = doc.CreateElement("brokerorgstate");
                                XmlText brokerorgstateText = doc.CreateTextNode(wcg.Insured[i].policy[z].brokerorgstate.ToString().Trim());
                                brokerorgstateElement.AppendChild(brokerorgstateText);
                                Policy.AppendChild(brokerorgstateElement);
                            }
                            #endregion

                            #region Create <brokerorgpostalcode> Child of Policy Element
                            if (!string.IsNullOrEmpty(wcg.Insured[i].policy[z].brokerorgpostalcode))
                            {
                                XmlElement brokerorgpostalcodeElement = doc.CreateElement("brokerorgpostalcode");
                                XmlText brokerorgpostalcodeText = doc.CreateTextNode(wcg.Insured[i].policy[z].brokerorgpostalcode.ToString().Trim());
                                brokerorgpostalcodeElement.AppendChild(brokerorgpostalcodeText);
                                Policy.AppendChild(brokerorgpostalcodeElement);
                            }
                            #endregion

                            //TAP INFO / Admin Info

                            #region Create <claimadminorgname> Child of Policy Element
                            if (!string.IsNullOrEmpty(wcg.Insured[i].policy[z].claimadminorgname))
                            {
                                XmlElement claimadminorgnameElement = doc.CreateElement("claimadminorgname");
                                XmlText claimadminorgnameText = doc.CreateTextNode(wcg.Insured[i].policy[z].claimadminorgname.ToString().Trim());
                                claimadminorgnameElement.AppendChild(claimadminorgnameText);
                                Policy.AppendChild(claimadminorgnameElement);
                            }
                            #endregion

                            #region Create <claimadminorgaddress1> Child of Policy Element
                            if (!string.IsNullOrEmpty(wcg.Insured[i].policy[z].claimadminorgaddress1))
                            {
                                XmlElement claimadminorgaddress1Element = doc.CreateElement("claimadminorgaddress1");
                                XmlText claimadminorgaddress1Text = doc.CreateTextNode(wcg.Insured[i].policy[z].claimadminorgaddress1.ToString().Trim());
                                claimadminorgaddress1Element.AppendChild(claimadminorgaddress1Text);
                                Policy.AppendChild(claimadminorgaddress1Element);
                            }
                            #endregion

                            #region Create <claimadminorgaddress2> Child of Policy Element
                            if (!string.IsNullOrEmpty(wcg.Insured[i].policy[z].claimadminorgaddress2))
                            {
                                XmlElement claimadminorgaddress2Element = doc.CreateElement("claimadminorgaddress2");
                                XmlText claimadminorgaddress2Text = doc.CreateTextNode(wcg.Insured[i].policy[z].claimadminorgaddress2.ToString().Trim());
                                claimadminorgaddress2Element.AppendChild(claimadminorgaddress2Text);
                                Policy.AppendChild(claimadminorgaddress2Element);
                            }
                            #endregion

                            #region Create <claimadminorgcity> Child of Policy Element
                            if (!string.IsNullOrEmpty(wcg.Insured[i].policy[z].claimadminorgcity))
                            {
                                XmlElement claimadminorgcityElement = doc.CreateElement("claimadminorgcity");
                                XmlText claimadminorgcityText = doc.CreateTextNode(wcg.Insured[i].policy[z].claimadminorgcity.ToString().Trim());
                                claimadminorgcityElement.AppendChild(claimadminorgcityText);
                                Policy.AppendChild(claimadminorgcityElement);
                            }
                            #endregion

                            #region Create <claimadminorgstate> Child of Policy Element
                            if (!string.IsNullOrEmpty(wcg.Insured[i].policy[z].claimadminorgstate))
                            {
                                XmlElement claimadminorgstateElement = doc.CreateElement("claimadminorgstate");
                                XmlText claimadminorgstateText = doc.CreateTextNode(wcg.Insured[i].policy[z].claimadminorgstate.ToString().Trim());
                                claimadminorgstateElement.AppendChild(claimadminorgstateText);
                                Policy.AppendChild(claimadminorgstateElement);
                            }
                            #endregion

                            #region Create <claimadminorgpostalcode> Child of Policy Element
                            if (!string.IsNullOrEmpty(wcg.Insured[i].policy[z].claimadminorgpostalcode))
                            {
                                XmlElement claimadminorgpostalcodeElement = doc.CreateElement("claimadminorgpostalcode");
                                XmlText claimadminorgpostalcodeText = doc.CreateTextNode(wcg.Insured[i].policy[z].claimadminorgpostalcode.ToString().Trim());
                                claimadminorgpostalcodeElement.AppendChild(claimadminorgpostalcodeText);
                                Policy.AppendChild(claimadminorgpostalcodeElement);
                            }
                            #endregion

                            #region Create <claimadminrepname> Child of Policy Element
                            if (!string.IsNullOrEmpty(wcg.Insured[i].policy[z].claimadminrepname))
                            {
                                XmlElement claimadminrepnameElement = doc.CreateElement("claimadminrepname");
                                XmlText claimadminrepnameText = doc.CreateTextNode(wcg.Insured[i].policy[z].claimadminrepname.ToString().Trim());
                                claimadminrepnameElement.AppendChild(claimadminrepnameText);
                                Policy.AppendChild(claimadminrepnameElement);
                            }
                            #endregion

                            #region Create <claimadminreptitle> Child of Policy Element
                            if (!string.IsNullOrEmpty(wcg.Insured[i].policy[z].claimadminreptitle))
                            {
                                XmlElement claimadminreptitleElement = doc.CreateElement("claimadminreptitle");
                                XmlText claimadminreptitleText = doc.CreateTextNode(wcg.Insured[i].policy[z].claimadminreptitle.ToString().Trim());
                                claimadminreptitleElement.AppendChild(claimadminreptitleText);
                                Policy.AppendChild(claimadminreptitleElement);
                            }
                            #endregion

                            #region Create <claimadminrepphone> Child of Policy Element
                            if (!string.IsNullOrEmpty(wcg.Insured[i].policy[z].claimadminrepphone))
                            {
                                XmlElement claimadminrepphoneElement = doc.CreateElement("claimadminrepphone");
                                XmlText claimadminrepphoneText = doc.CreateTextNode(wcg.Insured[i].policy[z].claimadminrepphone.ToString().Trim());
                                claimadminrepphoneElement.AppendChild(claimadminrepphoneText);
                                Policy.AppendChild(claimadminrepphoneElement);
                            }
                            #endregion

                            #region Create <claimadminrepemail> Child of Policy Element
                            if (!string.IsNullOrEmpty(wcg.Insured[i].policy[z].claimadminrepemail))
                            {
                                XmlElement claimadminrepemailElement = doc.CreateElement("claimadminrepemail");
                                XmlText claimadminrepemailText = doc.CreateTextNode(wcg.Insured[i].policy[z].claimadminrepemail.ToString().Trim());
                                claimadminrepemailElement.AppendChild(claimadminrepemailText);
                                Policy.AppendChild(claimadminrepemailElement);
                            }
                            #endregion

                            #region Create <virtualstickerbgcolor> Child of Policy Element
                            {
                                XmlElement virtualstickerbgcolorElement = doc.CreateElement("virtualstickerbgcolor");
                                XmlText virtualStickerText = doc.CreateTextNode("#FFFF00");
                                virtualstickerbgcolorElement.AppendChild(virtualStickerText);
                                Policy.AppendChild(virtualstickerbgcolorElement);
                            }
                            #endregion


                            //LOCATION INFO
                            if (wcg.Insured[i].location.Count > 0)
                            {
                                #region Create <Locations> Child of Policy Element

                                XmlElement Locations = doc.CreateElement("locations");
                                Policy.AppendChild(Locations);

                                #endregion

                                for (int l = 0; l < wcg.Insured[i].location.Count; l++)
                                {
                                    #region Create <Location> Child of Locations Element
                                    XmlElement Location = doc.CreateElement("location");
                                    Locations.AppendChild(Location);
                                    #endregion

                                    #region Location Child Nodes
                                    #region Create <JurisdictionId> Child of Location Element
                                    XmlElement JurisdictionIdElement = doc.CreateElement("jurisdictionid");
                                    XmlText JurisdictionIdText = doc.CreateTextNode(wcg.Insured[i].location[l].jurisdictionid.ToString().Trim());
                                    JurisdictionIdElement.AppendChild(JurisdictionIdText);
                                    Location.AppendChild(JurisdictionIdElement);
                                    #endregion

                                    #region Create <Name> Child of Location Element
                                    XmlElement NameElement = doc.CreateElement("name");
                                    XmlText NameText = doc.CreateTextNode(wcg.Insured[i].location[l].name.ToString().Trim());
                                    NameElement.AppendChild(NameText);
                                    Location.AppendChild(NameElement);
                                    #endregion

                                    #region Create <LocationAddress1> Child of Location Element
                                    if (!string.IsNullOrEmpty(wcg.Insured[i].location[l].address1))
                                    {
                                        XmlElement LocationAddress1Element = doc.CreateElement("address1");
                                        XmlText LocationAddress1Text = doc.CreateTextNode(wcg.Insured[i].location[l].address1.ToString().Trim());
                                        LocationAddress1Element.AppendChild(LocationAddress1Text);
                                        Location.AppendChild(LocationAddress1Element);
                                    }
                                    #endregion

                                    #region Create <LocationCity> Child of Location Element
                                    XmlElement LocationCityElement = doc.CreateElement("city");
                                    XmlText LocationCityText = doc.CreateTextNode(wcg.Insured[i].location[l].city.ToString().Trim());
                                    LocationCityElement.AppendChild(LocationCityText);
                                    Location.AppendChild(LocationCityElement);
                                    #endregion

                                    #region Create <LocationState> Child of Location Element
                                    XmlElement LocationStateElement = doc.CreateElement("state");
                                    XmlText LocationStateText = doc.CreateTextNode(wcg.Insured[i].location[l].State.ToString().Trim());
                                    LocationStateElement.AppendChild(LocationStateText);
                                    Location.AppendChild(LocationStateElement);
                                    #endregion

                                    #region Create <LocationPostCode> Child of Location Element
                                    XmlElement LocationPostCodeElement = doc.CreateElement("postalcode");
                                    XmlText LocationPostCodeText = doc.CreateTextNode(wcg.Insured[i].location[l].postalcode.ToString().Trim());
                                    LocationPostCodeElement.AppendChild(LocationPostCodeText);
                                    Location.AppendChild(LocationPostCodeElement);
                                    #endregion

                                    #region Create <quantity> Child of Location Element
                                    if (!string.IsNullOrEmpty(wcg.Insured[i].location[l].quantity))
                                    {
                                        XmlElement quantityElement = doc.CreateElement("quantity");
                                        XmlText quantityText = doc.CreateTextNode(wcg.Insured[i].location[l].quantity.ToString().Trim());
                                        quantityElement.AppendChild(quantityText);
                                        Location.AppendChild(quantityElement);
                                    }
                                    #endregion

                                    #region Create <employeecount> Child of Location Element
                                    if (!string.IsNullOrEmpty(wcg.Insured[i].location[l].employeecount))
                                    {
                                        XmlElement employeecountElement = doc.CreateElement("employeecount");
                                        XmlText employeecountText = doc.CreateTextNode(wcg.Insured[i].location[l].employeecount.ToString().Trim());
                                        employeecountElement.AppendChild(employeecountText);
                                        Location.AppendChild(employeecountElement);
                                    }
                                    #endregion

                                    #region Create <LocSequence> Child of Location Element
                                    if (!string.IsNullOrEmpty(wcg.Insured[i].location[l].locsequence))
                                    {
                                        XmlElement LocSequencElement = doc.CreateElement("locsequence");
                                        XmlText LocSequencText = doc.CreateTextNode(wcg.Insured[i].location[l].locsequence.ToString().Trim());
                                        LocSequencElement.AppendChild(LocSequencText);
                                        Location.AppendChild(LocSequencElement);
                                    }
                                    #endregion

                                    #region Create <virtualstickertext> Child of Location Element
                                    if (!string.IsNullOrEmpty(wcg.Insured[i].location[l].virtualstickertext))
                                    {
                                        XmlElement virtualstickertextElement = doc.CreateElement("virtualstickertext");
                                        XmlText virtualStickerText = doc.CreateTextNode(wcg.Insured[i].location[l].virtualstickertext);
                                        virtualstickertextElement.AppendChild(virtualStickerText);
                                        Location.AppendChild(virtualstickertextElement);
                                    }
                                    #endregion
                                    #endregion

                                    #region Override Claim Child Nodes
                                    #region Create <overrideclaimreportingaddress> Child of Location Element
                                    XmlElement overrideclaimreportingaddressElement = doc.CreateElement("overrideclaimreportingaddress");
                                    XmlText overrideclaimreportingaddressText = doc.CreateTextNode(wcg.Insured[i].location[l].overrideclaimreportingaddress.ToString().Trim());
                                    overrideclaimreportingaddressElement.AppendChild(overrideclaimreportingaddressText);
                                    Location.AppendChild(overrideclaimreportingaddressElement);
                                    #endregion

                                    #region Create <overrideclaimreportingname> Child of Location Element
                                    XmlElement overrideclaimreportingnameElement = doc.CreateElement("overrideclaimreportingname");
                                    XmlText overrideclaimreportingnameText = doc.CreateTextNode(wcg.Insured[i].location[l].overrideclaimreportingname.ToString().Trim());
                                    overrideclaimreportingnameElement.AppendChild(overrideclaimreportingnameText);
                                    Location.AppendChild(overrideclaimreportingnameElement);
                                    #endregion

                                    #region Create <overrideclaimreportingaddress1> Child of Location Element
                                    if (!string.IsNullOrEmpty(wcg.Insured[i].location[l].overrideclaimreportingaddress1))
                                    {
                                        XmlElement overrideclaimreportingaddress1Element = doc.CreateElement("overrideclaimreportingaddress1");
                                        XmlText overrideclaimreportingaddress1Text = doc.CreateTextNode(wcg.Insured[i].location[l].overrideclaimreportingaddress1.ToString().Trim());
                                        overrideclaimreportingaddress1Element.AppendChild(overrideclaimreportingaddress1Text);
                                        Location.AppendChild(overrideclaimreportingaddress1Element);
                                    }
                                    #endregion

                                    #region Create <overrideclaimreportingaddress2> Child of Location Element
                                    if (!string.IsNullOrEmpty(wcg.Insured[i].location[l].overrideclaimreportingaddress2))
                                    {
                                        XmlElement overrideclaimreportingaddress2Element = doc.CreateElement("overrideclaimreportingaddress2");
                                        XmlText overrideclaimreportingaddress2Text = doc.CreateTextNode(wcg.Insured[i].location[l].overrideclaimreportingaddress2.ToString().Trim());
                                        overrideclaimreportingaddress2Element.AppendChild(overrideclaimreportingaddress2Text);
                                        Location.AppendChild(overrideclaimreportingaddress2Element);
                                    }
                                    #endregion

                                    #region Create <overrideclaimreportingcity> Child of Location Element
                                    if (!string.IsNullOrEmpty(wcg.Insured[i].location[l].overrideclaimreportingcity))
                                    {
                                        XmlElement overrideclaimreportingcityElement = doc.CreateElement("overrideclaimreportingcity");
                                        XmlText overrideclaimreportingcityText = doc.CreateTextNode(wcg.Insured[i].location[l].overrideclaimreportingcity.ToString().Trim());
                                        overrideclaimreportingcityElement.AppendChild(overrideclaimreportingcityText);
                                        Location.AppendChild(overrideclaimreportingcityElement);
                                    }
                                    #endregion

                                    #region Create <overrideclaimreportingstate> Child of Location Element
                                    if (!string.IsNullOrEmpty(wcg.Insured[i].location[l].overrideclaimreportingstate))
                                    {
                                        XmlElement overrideclaimreportingstateElement = doc.CreateElement("overrideclaimreportingstate");
                                        XmlText overrideclaimreportingstateText = doc.CreateTextNode(wcg.Insured[i].location[l].overrideclaimreportingstate.ToString().Trim());
                                        overrideclaimreportingstateElement.AppendChild(overrideclaimreportingstateText);
                                        Location.AppendChild(overrideclaimreportingstateElement);
                                    }
                                    #endregion

                                    #region Create <overrideclaimreportingpostalcode> Child of Location Element
                                    if (!string.IsNullOrEmpty(wcg.Insured[i].location[l].overrideclaimreportingpostalcode))
                                    {
                                        XmlElement overrideclaimreportingpostalcodeElement = doc.CreateElement("overrideclaimreportingpostalcode");
                                        XmlText overrideclaimreportingpostalcodeText = doc.CreateTextNode(wcg.Insured[i].location[l].overrideclaimreportingpostalcode.ToString().Trim());
                                        overrideclaimreportingpostalcodeElement.AppendChild(overrideclaimreportingpostalcodeText);
                                        Location.AppendChild(overrideclaimreportingpostalcodeElement);
                                    }
                                    #endregion

                                    #region Create <overrideclaimreportingrepname> Child of Location Element
                                    if (!string.IsNullOrEmpty(wcg.Insured[i].location[l].overrideclaimreportingrepname))
                                    {
                                        XmlElement overrideclaimreportingrepnameElement = doc.CreateElement("overrideclaimreportingrepname");
                                        XmlText overrideclaimreportingrepnameText = doc.CreateTextNode(wcg.Insured[i].location[l].overrideclaimreportingrepname.ToString().Trim());
                                        overrideclaimreportingrepnameElement.AppendChild(overrideclaimreportingrepnameText);
                                        Location.AppendChild(overrideclaimreportingrepnameElement);
                                    }
                                    #endregion

                                    #region Create <overrideclaimreportingreptitle> Child of Location Element
                                    if (!string.IsNullOrEmpty(wcg.Insured[i].location[l].overrideclaimreportingreptitle))
                                    {
                                        XmlElement overrideclaimreportingreptitleElement = doc.CreateElement("overrideclaimreportingreptitle");
                                        XmlText overrideclaimreportingreptitleText = doc.CreateTextNode(wcg.Insured[i].location[l].overrideclaimreportingreptitle.ToString().Trim());
                                        overrideclaimreportingreptitleElement.AppendChild(overrideclaimreportingreptitleText);
                                        Location.AppendChild(overrideclaimreportingreptitleElement);
                                    }
                                    #endregion

                                    #region Create <overrideclaimreportingrepphone> Child of Location Element
                                    if (!string.IsNullOrEmpty(wcg.Insured[i].location[l].overrideclaimreportingrepphone))
                                    {
                                        XmlElement overrideclaimreportingrepphoneElement = doc.CreateElement("overrideclaimreportingrepphone");
                                        XmlText overrideclaimreportingrepphoneText = doc.CreateTextNode(wcg.Insured[i].location[l].overrideclaimreportingrepphone.ToString().Trim());
                                        overrideclaimreportingrepphoneElement.AppendChild(overrideclaimreportingrepphoneText);
                                        Location.AppendChild(overrideclaimreportingrepphoneElement);
                                    }
                                    #endregion

                                    #region Create <overrideclaimreportingrepemail> Child of Location Element
                                    if (!string.IsNullOrEmpty(wcg.Insured[i].location[l].overrideclaimreportingrepemail))
                                    {
                                        XmlElement overrideclaimreportingrepemailElement = doc.CreateElement("overrideclaimreportingrepemail");
                                        XmlText overrideclaimreportingrepemailText = doc.CreateTextNode(wcg.Insured[i].location[l].overrideclaimreportingrepemail.ToString().Trim());
                                        overrideclaimreportingrepemailElement.AppendChild(overrideclaimreportingrepemailText);
                                        Location.AppendChild(overrideclaimreportingrepemailElement);
                                    }
                                    #endregion

                                    #endregion

                                }
                            }

                            Policies.AppendChild(Policy);
                        }
                        #endregion

                        #endregion
                    }
                }
                #endregion

                #endregion


                return doc;
            }
        }
    }

}
