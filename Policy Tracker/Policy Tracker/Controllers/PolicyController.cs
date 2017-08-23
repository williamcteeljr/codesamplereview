using DevExpress.Web.Mvc;

using PolicyTracker.BusinessServices;
using PolicyTracker.DataAccess;
using PolicyTracker.DataAccess.WCPostingNotice;
using PolicyTracker.DomainModel;
using PolicyTracker.DomainModel.Brokers;
using PolicyTracker.DomainModel.Common;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.DomainModel.Policy;
using PolicyTracker.DomainModel.Security;
using PolicyTracker.Filters;
using PolicyTracker.Platform.Security;
using PolicyTracker.Platform.UOW;
using PolicyTracker.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Text;
using PolicyTracker.DomainModel.PostNotice;

namespace PolicyTracker.Controllers
{
    public class PolicyController : BaseController
    {
        /// <summary>
        /// Build the necessary view data for the Risk Entry and Risk Edit Forms.
        /// </summary>
        /// <param name="risk">Risk being entered</param>
        private void RiskViewData(Risk risk)
        {
            var productLines = ServiceLocator.EntityService.GetList<ProductLine>();

            ViewBag.ProductLines = productLines;
            ViewBag.Products = ServiceLocator.EntityService.GetList<Product>(new PropertyFilter("IsActive", true));
            ViewBag.Branches = StringEnum.GetAll<Branch>();
            ViewBag.States = ServiceLocator.BrokerSvc.GetStates();
            ViewBag.AircraftUses = ServiceLocator.EntityService.GetList<PurposeOfUse>(new PropertyFilter("IsActive", true));
            ViewBag.Statuses = new List<RiskStatus>() { RiskStatus.INVOLVED, RiskStatus.SUBMISSION };
            ViewBag.Underwriters = ServiceLocator.RiskService.GetUnderwriters();
            ViewBag.Agents = ServiceLocator.BrokerSvc.GetAgentsForBroker(risk.AgencyID);
        }

        /// <summary>
        /// Build the Additional View data elements required for the Risk Edit Form. This is not necessary for Risk Entry due to the simplified amount of entry points in the Risk Entry/Submission Process
        /// </summary>
        /// <param name="risk">Risk Being Edited</param>
        private void RiskEditFormViewData(Risk risk)
        {
            var productLines = ServiceLocator.EntityService.GetList<ProductLine>();
            ViewBag.WCProductLineId = productLines.Where(x => x.Name == "Workers Comp").First().ProductLineId;
            //Filter by Status Unsuccessful due to how Ravent initially setup this table. I wanted to use it instead of creating a new one.
            var markets = ServiceLocator.EntityService.GetList<Market>(new PropertyFilter("Status", PropertyFilter.Comparator.Equals, "Unsuccessful")).ToList();
            markets.Insert(0, new Market() { Id = 0, CompanyName = "" });
            ViewBag.Markets = markets;
            ViewBag.StatusReasons = ServiceLocator.PolicySvc.GetStatusReasons(risk.Status);
            ViewBag.WCPrograms = StringEnum.GetAll<WorkCompProgramType>();
            ViewBag.WCAccountDescs = StringEnum.GetAll<WorkCompAccountDesc>();

            var broker = ServiceLocator.EntityService.GetInstance<Broker>(new PropertyFilter("AgencyID", risk.AgencyID));
            var brokerMailing = ServiceLocator.EntityService.GetInstance<BrokerMailingAddress>(new PropertyFilter("AgencyID", risk.AgencyID));
            ViewBag.Broker = (broker == null) ? new Broker() { AgencyName = "", State = "" } : broker;
            ViewBag.BrokerMailing = (brokerMailing == null) ? new BrokerMailingAddress() { State = "" } : brokerMailing;

            ViewBag.IsLicensed = ServiceLocator.BrokerSvc.IsBrokerLicensedAlert(risk);
            ViewBag.Agents = ServiceLocator.BrokerSvc.GetAgentsForBroker(risk.AgencyID);
            ViewBag.Assistants = ServiceLocator.RiskService.GetUnderwritingAssistants();

            ViewBag.Statuses = StringEnum.GetAll<RiskStatus>().ToList();
        }

        /// <summary>
        /// Gets the availble list of statuses based on the current status of the Risk being entered/edited. Currently we allow any status if the risk is being edited to make things easier.
        /// </summary>
        /// <param name="status">Current Risk Status</param>
        /// <param name="statuses">List of Availble statuses</param>
        /// <param name="isEdit">Is Risk Being Edited</param>
        /// <returns>List of availble status choices</returns>
        public List<RiskStatus> GetStatusList(string status, List<RiskStatus> statuses, bool isEdit)
        {
            switch (status)
            {
                case "Submission":
                    statuses = new List<RiskStatus>() { RiskStatus.INVOLVED, RiskStatus.SUBMISSION, RiskStatus.DECLINED };
                    break;
                default:
                    statuses = StringEnum.GetAll<RiskStatus>().ToList();
                    break;
            }
            if (isEdit) statuses = StringEnum.GetAll<RiskStatus>().ToList();
            return statuses;
        }

        public ActionResult ClearanceSearch()
        {
            return PartialView();
        }

        public ActionResult RiskPreview(int riskId, string context)
        {
            var risk = ServiceLocator.RiskService.GetRisk(riskId);
            ViewBag.ProductLine = ServiceLocator.EntityService.GetInstance<ProductLine>(new PropertyFilter("ProductLineId", risk.ProductLine));
            var uw = ServiceLocator.EntityService.GetInstance<User>(new PropertyFilter("UserId", risk.UnderwriterId));
            ViewBag.UWName = (uw != null) ? uw.Name : "NOT ASSIGNED";
            var relatedRisk = ServiceLocator.EntityService.GetInstance<Risk>(new PropertyFilter("Id", risk.ParentID));
            ViewBag.RelatedRisk = relatedRisk;
            var broker = ServiceLocator.EntityService.GetInstance<BrokerMailingAddress>(new PropertyFilter("AgencyID", risk.AgencyID));
            ViewBag.BrokerState = (broker != null) ? broker.State : "";
            ViewBag.Logs = ServiceLocator.EntityService.GetList<RiskLog>(new PropertyFilter("RiskId", riskId));
            //risk.Aircrafts = ServiceLocator.EntityService.GetList<Aircraft>(new PropertyFilter("QuoteId", risk.Id)).ToList();
            if (relatedRisk != null)
            {
                var relatedUW = ServiceLocator.EntityService.GetInstance<User>(new PropertyFilter("UserId", relatedRisk.UnderwriterId));
                ViewBag.RealtedRiskUW = (relatedUW != null) ? relatedUW.Name : "";
            }
            ViewBag.PreviewParent = context;
            return PartialView(risk);
        }

        [HttpGet]
        public ActionResult RiskEntryForm(int controlNumber = 0)
        {
            var model = ServiceLocator.RiskService.GetNewRisk(controlNumber);
            RiskViewData(model);
            return PartialView(model);
        }

        [HttpPost]
        [UseTransaction]
        public ActionResult RiskEntryForm(Risk entity)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (entity.Id != 0) entity = ServiceLocator.RiskService.GetRisk(entity.Id);
                    entity = WebUtils.FormDataToModel<Risk>(entity, Request.Form);

                    if (entity.Id != 0)
                        ServiceLocator.PolicySvc.EntrySaveExisting(entity);
                    else
                        ServiceLocator.PolicySvc.SaveNewRisk(entity);
                    ModelState.Clear();
                }
                catch (ValidationRulesException ex)
                {
                    ProcessValidationException(ex);
                }
            }

            RiskViewData(entity);
            return PartialView(entity);
        }

        public ActionResult NoteForm(int riskId, int noteId = 0)
        {
            var note = new RiskNote() { RiskId = riskId };
            if (noteId != 0) note = ServiceLocator.EntityService.GetInstance<RiskNote>(new PropertyFilter[] { new PropertyFilter("RiskId", riskId), new PropertyFilter("Id", noteId) });
            return PartialView(note);
        }

        [HttpPost]
        [UseTransaction]
        public ActionResult NoteForm(RiskNote entity)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    entity = BindRequestToEntity<RiskNote>(entity, new PropertyFilter("Id", entity.Id));
                    ServiceLocator.RiskService.SaveNote(entity);
                    ModelState.Clear();
                }
                catch (ValidationRulesException ex)
                {
                    ProcessValidationException(ex);
                }
            }

            return PartialView(entity);
        }

        public ActionResult CommercialRatingApp(int id)
        {
            var model = ServiceLocator.EntityService.GetInstance<Risk>(new PropertyFilter("Id", id));
            ServiceLocator.PolicySvc.PrepareRiskForCommApp(model);
            return PartialView(model);
        }

        [HttpGet]
        public ActionResult RiskEditForm(int id)
        {
            var model = ServiceLocator.RiskService.GetRisk(id);

            //get collection of Locations for List Box View in Main Insured WC OpitionList
            List<WCPnLocationDomain> locations = new List<WCPnLocationDomain>();
            locations = WCPNServiceDAOCalls.View.getAlllocations(id);
            model.MainLocationsOptionList = new MultiSelectList(locations, "Id", "Name");

            //Load Selected Mapped Data
            //Get all assigned Locations mapped to QuoteId, AdditionalNamedInsuredId
            List<WCPnLocationDomain> selectedlocations = new List<WCPnLocationDomain>();
            selectedlocations = WCPNServiceDAOCalls.View.getMainInsuredLocations(id);
            //Add location items to int Array List
            var itemList = new List<int>();
            for (int i = 0; i < selectedlocations.Count; i++)
            {
                itemList.Add(selectedlocations[i].Id);
            }
            model.MainLocationsSelectedItemsIds = itemList.ToArray();

            RiskViewData(model);
            RiskEditFormViewData(model);


            return PartialView(model);
        }

        public ActionResult RiskPremiumContent(int id)
        {
            var model = ServiceLocator.EntityService.GetInstance<RiskPremiumInfo>(new PropertyFilter("RiskId", id));
            ViewBag.PremChangeReasons = ServiceLocator.EntityService.GetList<Reason>(new PropertyFilter("ActionType", "PC"));
            return PartialView(model);
        }

        public ActionResult RiskPremiumContentPartial(RiskPremiumInfo rpi)
        {
            ViewBag.PremChangeReasons = ServiceLocator.EntityService.GetList<Reason>(new PropertyFilter("ActionType", "PC"));
            return PartialView("~/Views/Policy/RiskPremiumContent.cshtml", rpi);
        }

        public ActionResult RiskInstallmentContent(int id)
        {
            var model = ServiceLocator.EntityService.GetInstance<Risk>(new PropertyFilter("Id", id));
            model.InstallmentInfo = ServiceLocator.EntityService.GetInstance<RiskInstallmentInfo>(new PropertyFilter("RiskId", id));
            ViewBag.PaymentTerms = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
            return PartialView(model);
        }

        public ActionResult RiskEditInstallmentContent(Risk risk)
        {
            ViewBag.PaymentTerms = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
            return PartialView("~/Views/Policy/RiskInstallmentContent.cshtml", risk);
        }

        [HttpPost]
        public ActionResult ResetMainLocations(int RiskId)
        {
            var model = ServiceLocator.RiskService.GetRisk(RiskId);

            //Step 1 Delete all records that are assigned to the riskid = QuoteID that has Null or Zero
            UIWCPNServiceDAOCalls.View.DeleteMappingBindingByQuoteID(RiskId);

            //get collection of Locations for List Box View in Main Insured WC OpitionList
            List<WCPnLocationDomain> locations = new List<WCPnLocationDomain>();
            locations = WCPNServiceDAOCalls.View.getAlllocations(RiskId);
            model.MainLocationsOptionList = new MultiSelectList(locations, "Id", "Name");

            //Load Selected Mapped Data
            //Get all assigned Locations mapped to QuoteId, AdditionalNamedInsuredId
            locations = new List<WCPnLocationDomain>();
            locations = WCPNServiceDAOCalls.View.getMainInsuredLocations(RiskId);
            //Add location items to int Array List
            if (locations.Count > 0)
            {
                var itemList = new List<int>();
                for (int i = 0; i < locations.Count; i++)
                {
                    itemList.Add(locations[i].Id);
                }
                model.MainLocationsSelectedItemsIds = itemList.ToArray();
            }
            return PartialView(model.MainLocationsOptionList);
        }

        [HttpPost]
        [UseTransaction]
        public ActionResult RiskEditForm(Risk entity)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //Get all selected Locations form List Box
                    if (entity.MainLocationsSelectedItemsIds != null)
                    {
                        //Step 1 Delete all records that are assigned to the riskid = QuoteID that has Null or Zero
                        UIWCPNServiceDAOCalls.View.DeleteMappingBindingByQuoteID(entity.Id);

                        for (int z = 0; z < entity.MainLocationsSelectedItemsIds.Length; z++)
                        {
                            //Step 2 Insert a new location into the Mapping Table
                            UIWCPNServiceDAOCalls.View.InsertMappingBindingMain(entity.Id, Convert.ToInt32(entity.MainLocationsSelectedItemsIds[z]));
                        }
                    }
                    else
                    {
                        //Step 1 Delete all records that are not selected
                        UIWCPNServiceDAOCalls.View.DeleteMappingBindingByQuoteID(entity.Id);
                    }

                    if (entity.Id != 0) entity = ServiceLocator.RiskService.GetRisk(entity.Id);
                    entity = WebUtils.FormDataToModel<Risk>(entity, Request.Form);
                    ServiceLocator.PolicySvc.UpdateRisk(entity);
                    if (entity.UpdateClientInfo) ServiceLocator.NamedInsuredService.UpdateNamedInsured(entity.Id);
                    

                    //ModelState.Clear();

                    //get collection of Locations for List Box View in Main Insured WC OpitionList
                    List<WCPnLocationDomain> locations = new List<WCPnLocationDomain>();
                    locations = WCPNServiceDAOCalls.View.getAlllocations(entity.Id);
                    entity.MainLocationsOptionList = new MultiSelectList(locations, "Id", "Name");

                    //Load Selected Mapped Data
                    //Get all assigned Locations mapped to QuoteId, AdditionalNamedInsuredId
                    locations = new List<WCPnLocationDomain>();
                    locations = WCPNServiceDAOCalls.View.getMainInsuredLocations(entity.Id);
                    //Add location items to int Array List
                    if (locations.Count > 0)
                    {
                        var itemList = new List<int>();
                        for (int i = 0; i < locations.Count; i++)
                        {
                            itemList.Add(locations[i].Id);
                        }
                        entity.MainLocationsSelectedItemsIds = itemList.ToArray();
                    }
                }
                catch (ValidationRulesException ex)
                {
                    ProcessValidationException(ex);
                }
            }

            RiskViewData(entity);
            RiskEditFormViewData(entity);
            return PartialView(entity);
        }

        [HttpDelete]
        public void DeleteRisk(int id)
        {
            ServiceLocator.RiskService.DeleteRisk(id);
        }

        /// <summary>
        /// Fetch pick list & other data needed to create or edit an aircraft.
        /// </summary>
        public void AircraftEditFormViewData()
        {
            List<int> years = new List<int>();
            for (var i = DateTime.Now.Year; i >= 1930; i--)
            {
                years.Add(i);
            }

            ViewBag.Years = years;
            ViewBag.AircraftUses = ServiceLocator.EntityService.GetList<PurposeOfUse>(new PropertyFilter("IsActive", true));
            ViewBag.Engines = ServiceLocator.EntityService.GetList<EngineType>();
        }

        [HttpGet]
        public ActionResult AircraftEditForm(int riskId, int id = 0)
        {
            Aircraft ac = null;

            if (id != 0)
                ac = ServiceLocator.AircraftSvc.GetAircraft(id);

            if (ac == null)
                ac = ServiceLocator.AircraftSvc.GetNewAircraftForRisk(riskId);

            AircraftEditFormViewData();

            return PartialView(ac);
        }

        [HttpPost]
        [UseTransaction]
        public ActionResult AircraftEditForm(Aircraft entity)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (entity.Id != 0) entity = ServiceLocator.AircraftSvc.GetAircraft(entity.Id);
                    entity = WebUtils.FormDataToModel<Aircraft>(entity, Request.Form);
                    ServiceLocator.AircraftSvc.SaveAircraft(entity);
                    ModelState.Clear();
                }
                catch (ValidationRulesException ex)
                {
                    ProcessValidationException(ex);
                }
            }

            AircraftEditFormViewData();
            return PartialView(entity);
        }

        public ActionResult PostingNoticeConfirmation()
        {
            return PartialView();
        }

        public ActionResult AdditionalInsuredEndorsement()
        {
            return PartialView();
        }

        public ActionResult AdditionalLocationsEndorsement()
        {
            return PartialView();
        }

        //[IgnoreModelStateErrors]
        //public ActionResult AdditionalLocationsAirportComboBox(Risk model = null)
        //{
        //    if (model == null)
        //    {
        //        model = new Risk();
        //    }
        //    return PartialView(model);
        //}

        public JsonResult PostingNoticeSubmission(string Suffix, string RiskId)
        {
            var user = SessionManager.GetCurrentSession().User.UserId;

            string TypeOfPayLoad = "RENEWAL";
            if (Suffix == "0")
            {
                TypeOfPayLoad = "NEW";
            }

            //Post Notice Web Api Service
            string ConfirmationCode = PostingNoticeService.SendPostingNotice(TypeOfPayLoad, Convert.ToInt32(RiskId), Convert.ToInt32(user));
            Dictionary<string, object> listData = new Dictionary<string, object>();
            listData.Add("Code", ConfirmationCode);
            var jsonReturn = Json(listData);
            return jsonReturn;
        }

        public JsonResult PostingNoticeEndorsedLocationSubmission(int LocationId = 0, String RiskId = "0")
        {
            //get current active user session
            var user = SessionManager.GetCurrentSession().User.UserId;

            //transaction type/payload
            string TypeOfPayLoad = "EN";

            var location = new Locations();

            if (LocationId != 0)
            {
                location = ServiceLocator.EntityService.GetInstance<Locations>(new PropertyFilter("LocationId", LocationId));
            }

            //Post Notice Web Api Service
            string ConfirmationCode = PostingNoticeService.SendPostingNotice(TypeOfPayLoad, Convert.ToInt32(RiskId), Convert.ToInt32(user), LocationId);
            Dictionary<string, object> listData = new Dictionary<string, object>();
            listData.Add("Code", ConfirmationCode);
            var jsonReturn = Json(listData);
            return jsonReturn;
        }

        public JsonResult PostingNoticeEndorsedInsuredSubmission(int InsuredId = 0, String RiskId = "0")
        {
            //get current active user session
            var user = SessionManager.GetCurrentSession().User.UserId;

            //transaction type/payload
            string TypeOfPayLoad = "EN";

            var aNI = new AdditionalNamedInsured();

            if (InsuredId != 0)
            {
                aNI = ServiceLocator.EntityService.GetInstance<AdditionalNamedInsured>(new PropertyFilter("AdditionalNamedInsuredId", InsuredId));
            }

            //Post Notice Web Api Service
            string ConfirmationCode = PostingNoticeService.SendPostingNotice(TypeOfPayLoad, Convert.ToInt32(RiskId), Convert.ToInt32(user), null, InsuredId);
            Dictionary<string, object> listData = new Dictionary<string, object>();
            listData.Add("Code", ConfirmationCode);
            var jsonReturn = Json(listData);
            return jsonReturn;
        }

        #region Posting Notice Confirmation Logs

        public void PNConfirmationViewData()
        {

            var pNConfirmation = new PNConfirmation();
            var risk = new Risk();

            ViewBag.PolicyNumber = risk.FullPolicyNumber;
            // = loc.PolicyNumber;
            ViewBag.ID = pNConfirmation.PNConfirmationId;
            ViewBag.QuoteId = pNConfirmation.QuoteId;
            ViewBag.Status = pNConfirmation.Status;
            ViewBag.SubmissionType = pNConfirmation.SubmissionType;
            ViewBag.ConfirmationCode = pNConfirmation.ConfirmationCode;
            ViewBag.ErrorMsg = pNConfirmation.ErrorMessage;
            ViewBag.Submissions = pNConfirmation.Submission;
            ViewBag.Response = pNConfirmation.Response;
            ViewBag.SubmittedBy = pNConfirmation.SentById;
            ViewBag.SubmittedDate = pNConfirmation.SentDate;
        }

        [HttpGet]
        public ActionResult PNConfirmationView(int pNConfirmationId = 0, int quoteId = 0)
        {
            PNConfirmation pNC = null;

            if (pNConfirmationId != 0)
            {
                pNC = ServiceLocator.EntityService.GetInstance<PNConfirmation>(new PropertyFilter("PNConfirmationId", pNConfirmationId));
            }
            if (pNC == null)
            {
                pNC = ServiceLocator.UIPostingNoticeService.GetPNConfirmationById(quoteId);
            }
            PNConfirmationViewData();
            return PartialView(pNC);
        }

        #endregion

        #region Additional Named Insured Views

        //AdditionalNamedInsuredEditViewData
        public void AdditionalNamedInsuredViewData()
        {
            var aNI = new AdditionalNamedInsured();

            var risk = new Risk();

            ViewBag.PolicyNumber = risk.FullPolicyNumber;

            ViewBag.InsuredID = aNI.AdditionalNamedInsuredId;
            ViewBag.QuoteId = ServiceLocator.EntityService.GetList<Locations>(new PropertyFilter("QuoteId", true));
            ViewBag.FEIN = aNI.FEIN;
            ViewBag.CompanyName = aNI.CompanyName;
            ViewBag.StreetAddress = aNI.StreetAddress1;
            ViewBag.StreetAddress2 = aNI.StreetAddress2;
            ViewBag.City = aNI.City;
            ViewBag.State = aNI.State;
            ViewBag.Zip = aNI.Zip;
            ViewBag.EmployeeCount = aNI.EmployeeCount;
            ViewBag.CreatedDate = aNI.CreatedDate;
            ViewBag.UpdatedDate = aNI.UpdatedDate;


        }

        //AdditionalNamedInsuredEdit [Get]
        [HttpGet]
        public ActionResult AdditionalNamedInsuredEdit(int quoteId = 0, int aniID = 0)
        {
            AdditionalNamedInsured aNI = null;

            if (aniID != 0)
            {
                aNI = ServiceLocator.EntityService.GetInstance<AdditionalNamedInsured>(new PropertyFilter("AdditionalNamedInsuredId", aniID));
            }
            if (aNI == null)
            {
                aNI = ServiceLocator.UIPostingNoticeService.GetAdditionalNamedInsuredById(quoteId);
            }

            //get collection of Locations for List Box View in Additional Named Insured
            //save each location in the locations property
            List<WCPnLocationDomain> locations = new List<WCPnLocationDomain>();
            locations = WCPNServiceDAOCalls.View.getAlllocations(quoteId);
            aNI.OptionList = new MultiSelectList(locations, "Id", "Name");

            //Load Selected Mapped Data
            //Get all assigned Locations mapped to QuoteId, AdditionalNamedInsuredId
            locations = new List<WCPnLocationDomain>();
            locations = UIWCPNServiceDAOCalls.View.SelectMappingBindingByID(aNI.AdditionalNamedInsuredId);
            //Add location items to int Array List
            var itemList = new List<int>();
            for(int i = 0; i < locations.Count; i++)
            {
                itemList.Add(locations[i].Id);
            }
            aNI.SelectedItemsIds = itemList.ToArray();

            AdditionalNamedInsuredViewData();
            return PartialView(aNI);
        }

        //AdditionalNamedInsuredEdit [Post]
        [HttpPost]
        [UseTransaction]
        public ActionResult AdditionalNamedInsuredEdit(AdditionalNamedInsured entity)
        {
            AdditionalNamedInsured insuredEntity = new AdditionalNamedInsured();

            if (ModelState.IsValid)
            {
                try
                {
                    //if (entity.AdditionalNamedInsuredId != 0) entity = ServiceLocator.EntityService.GetInstance<AdditionalNamedInsured>(new PropertyFilter("AdditionalNamedInsuredId", entity.AdditionalNamedInsuredId));
                    //entity = WebUtils.FormDataToModel<AdditionalNamedInsured>(entity, Request.Form);
                    insuredEntity = ServiceLocator.UIPostingNoticeService.SaveNewAdditionalInsured(entity);
                    //ModelState.Clear();
                }
                catch (ValidationRulesException ex)
                {

                    ProcessValidationException(ex);
                }
            }

            //AdditionalNamedInsuredViewData();
            return PartialView(insuredEntity);
        }


        #endregion

        //[IgnoreModelStateErrors]
        //public ActionResult LocationsEditAirportComboBox(Risk model = null)
        //{
        //    if (model == null)
        //    {
        //        model = new Risk();
        //    }
        //    return PartialView(model);
        //}

        #region Additional Locations Views

        //AdditionalLocationsEditViewData
        public void AdditionalLocationsEditViewData()
        {
            var loc = new Locations();

            var risk = new Risk();

            ViewBag.PolicyNumber = risk.FullPolicyNumber;
            // = loc.PolicyNumber;
            ViewBag.QuoteId = ServiceLocator.EntityService.GetList<Locations>(new PropertyFilter("QuoteId", true));
            ViewBag.ControlNumber = loc.ControlNumber;
            ViewBag.NamedInsured = loc.CompanyName;
            ViewBag.Name2 = loc.Name2;
            ViewBag.StreetAddress = loc.StreetAddress1;
            ViewBag.StreetAddress2 = loc.StreetAddress2;
            ViewBag.City = loc.City;
            ViewBag.State = loc.State;
            ViewBag.Zip = loc.ZipCode;
            ViewBag.AirportID = loc.AirportID;
            ViewBag.Quantity = loc.Quantity;
        }

        //AdditionalLocationsEdit [Get]
        [HttpGet]
        public ActionResult AdditionalLocationsEdit(int locationId = 0, int quoteId = 0)
        {
            var location = new Locations();

            if (locationId != 0)
            {
                location = ServiceLocator.EntityService.GetInstance<Locations>(new PropertyFilter("LocationId", locationId));
            }
            if (location == null)
            {
                location = ServiceLocator.UIPostingNoticeService.GetNewLocationById(quoteId);
            }
            AdditionalLocationsEditViewData();
            return PartialView(location);
        }

        //AdditionalLocationsEdit [Post]
        [HttpPost]
        [UseTransaction]
        public ActionResult AdditionalLocationsEdit(Locations entity)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    AddInsuredLocationMapping mapping = new AddInsuredLocationMapping();
                    if (entity.ControlNumber != 0) entity = ServiceLocator.EntityService.GetInstance<Locations>(new PropertyFilter("LocationId", entity.LocationId));
                    entity = WebUtils.FormDataToModel<Locations>(entity, Request.Form);
                    ServiceLocator.UIPostingNoticeService.SaveNewAdditionalLocations(entity);
                    ModelState.Clear();
                }
                catch (ValidationRulesException ex)
                {

                    ProcessValidationException(ex);
                }
            }

            AdditionalLocationsEditViewData();
            return PartialView(entity);
        }

        #endregion

        #region Hazard Group/Exposure Views

        public void HazardGroupDetailsEditViewData()
        {
            var exposure = new Exposure();

            var risk = new Risk();

            ViewBag.PolicyNumber = risk.FullPolicyNumber;
            ViewBag.QuoteId = ServiceLocator.EntityService.GetList<Exposure>(new PropertyFilter("QuoteId", true));
            ViewBag.HazardState = exposure.HazardState;
            ViewBag.Class = exposure.Class;
            ViewBag.ClassCodeID = exposure.ClassCodeID;
            ViewBag.EmployeeCount = exposure.EmployeeCount;
            ViewBag.EmployeePayroll = exposure.EmployeePayroll;
            ViewBag.EmployeePremium = exposure.EmployeePremium;
            ViewBag.EmployeeTypeID = exposure.EmployeeTypeID;
            ViewBag.ExposureID = exposure.ExposureID;
            ViewBag.HazardGroupCode = exposure.HazardGroupCode;
            ViewBag.LocationID = exposure.LocationID;
            ViewBag.StatePremium = exposure.StatePremium;
        }

        [HttpPost]
        [UseTransaction]
        public ActionResult HazardGroupDetailsEdit(Exposure entity)
        {
            Exposure exposureEntity = new Exposure();

            try
            {
                exposureEntity = ServiceLocator.UIPostingNoticeService.SaveNewHazardGroup(entity);
            }
            catch (ValidationRulesException ex)
            {

                ProcessValidationException(ex);
            }

            AdditionalLocationsEditViewData();
            return PartialView(exposureEntity);
        }

        //Hazard Group Partial View
        [HttpGet]
        public ActionResult HazardGroupDetailsEdit(int exposureId = 0, int quoteId = 0)
        {
            var exposure = new Exposure();

            if (exposureId != 0)
            {
                exposure = ServiceLocator.EntityService.GetInstance<Exposure>(new PropertyFilter("ExposureID", exposureId));
            }
            if (exposure == null)
            {
                exposure = ServiceLocator.UIPostingNoticeService.GetHazardGroupById(quoteId);
            }

            ////get collection of Locations for List Box View in Additional Named Insured
            ////save each location in the locations property
            //List<WCPnLocationDomain> locations = new List<WCPnLocationDomain>();
            //locations = WCPNServiceDAOCalls.View.getAlllocations(quoteId);
            //exposure.ExpOptionList = new MultiSelectList(locations, "Id", "Name");

            ////Load Selected Mapped Data
            ////Get all assigned Locations mapped to QuoteId, AdditionalNamedInsuredId
            //locations = new List<WCPnLocationDomain>();
            //locations = UIWCPNServiceDAOCalls.View.SelectLocationByExposureId(exposureId);
            ////Add location items to int Array List
            //var itemList = new List<int>();
            //for (int i = 0; i < locations.Count; i++)
            //{
            //    itemList.Add(locations[i].Id);
            //}
            //exposure.ExpSelectedItemsIds = itemList.ToArray();

            HazardGroupDetailsEditViewData();
            return PartialView(exposure);
        }

        #endregion

        public ActionResult RiskAuditEditForm(int riskId, int auditId = 0)
        {
            var audit = new RiskAudit() { RiskId = riskId, AuditDate = DateTime.Now };
            if (auditId != 0) audit = ServiceLocator.EntityService.GetInstance<RiskAudit>(new PropertyFilter[] { new PropertyFilter("RiskId", riskId), new PropertyFilter("AuditId", auditId) });
            return PartialView(audit);
        }

        [HttpPost]
        [UseTransaction]
        public ActionResult RiskAuditEditForm(RiskAudit entity)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    entity = BindRequestToEntity<RiskAudit>(entity, new PropertyFilter("AuditId", entity.AuditId));
                    ServiceLocator.PolicySvc.SaveRiskAudit(entity);
                    ModelState.Clear();
                }
                catch (ValidationRulesException ex)
                {
                    ProcessValidationException(ex);
                }
            }

            return PartialView(entity);
        }

        public ActionResult CloneAndRelateRisk(int riskId)
        {
            var risk = ServiceLocator.PolicySvc.CloneRisk(riskId);
            RiskViewData(risk);
            return PartialView("RiskEntryForm", risk);
        }

        public ActionResult DeclineRiskForm(int id)
        {
            var model = ServiceLocator.EntityService.GetInstance<Risk>(new PropertyFilter("Id", id));
            //Set the status of the risk to declined here since we are skipping the user selecting to decline from a dropdown.
            model.Status = "Declined";
            return PartialView(model);
        }

        #region Installment Payments
        public ActionResult InstallmentPaymentsGrid(int riskId)
        {
            var payments = ServiceLocator.EntityService.GetList<RiskPayment>(new PropertyFilter("RiskId", riskId));
            ViewBag.RiskId = riskId;
            return PartialView(payments);
        }

        [ValidateInput(false)]
        public ActionResult BatchInstallmentPayments(MVCxGridViewBatchUpdateValues<RiskPayment, int> payments)
        {
            foreach (var payment in payments.Insert)
            {
                if (payments.IsValid(payment))
                {
                    payment.RiskId = Convert.ToInt32(Request.Params["RiskId"]);
                    ServiceLocator.PolicySvc.SavePayment(payment);
                }
            }
            foreach (var payment in payments.Update)
            {
                if (payments.IsValid(payment))
                {
                    payment.RiskId = Convert.ToInt32(Request.Params["RiskId"]);
                    ServiceLocator.PolicySvc.SavePayment(payment);
                }
            }
            foreach (var paymentId in payments.DeleteKeys)
            {
                ServiceLocator.PolicySvc.DeletePayment(paymentId);
            }

            ViewBag.RiskId = Convert.ToInt32(Request.Params["RiskId"]);
            return PartialView("InstallmentPaymentsGrid", ServiceLocator.EntityService.GetList<RiskPayment>(new PropertyFilter("RiskId", Request.Params["RiskId"])));
        }
        #endregion

        #region Endorsements
        public ActionResult EndorsementsGrid(int riskId)
        {
            var payments = ServiceLocator.EntityService.GetList<RiskEndorsement>(new PropertyFilter("RiskId", riskId));
            var risk = ServiceLocator.EntityService.GetInstance<Risk>(new PropertyFilter("Id", riskId));
            ViewBag.RiskId = riskId;
            //ViewBag.IsEditable = (risk.QuoteType != "AP" && risk.QuoteType != "AVC");
            return PartialView(payments);
        }

        [ValidateInput(false)]
        public ActionResult BatchEndorsements(MVCxGridViewBatchUpdateValues<RiskEndorsement, int> endorsements)
        {
            foreach (var endorsement in endorsements.Insert)
            {
                if (endorsements.IsValid(endorsement))
                {
                    endorsement.RiskId = Convert.ToInt32(Request.Params["RiskId"]);
                    ServiceLocator.PolicySvc.CreateEndorsement(endorsement);
                }
            }
            foreach (var endorsement in endorsements.Update)
            {
                if (endorsements.IsValid(endorsement))
                {
                    endorsement.RiskId = Convert.ToInt32(Request.Params["RiskId"]);
                    ServiceLocator.PolicySvc.UpdateEndorsement(endorsement);
                }
            }
            foreach (var endorsementId in endorsements.DeleteKeys)
            {
                ServiceLocator.PolicySvc.DeleteEndorsement(endorsementId);
            }

            ViewBag.RiskId = Convert.ToInt32(Request.Params["RiskId"]);
            var risk = ServiceLocator.EntityService.GetInstance<Risk>(new PropertyFilter("Id", Request.Params["RiskId"]));
            ViewBag.IsEditable = (risk.QuoteType != "AP" && risk.QuoteType != "AVC");

            return PartialView("EndorsementsGrid", ServiceLocator.EntityService.GetList<RiskEndorsement>(new PropertyFilter("RiskId", Request.Params["RiskId"])));
        }
        #endregion

        /// <summary>
        /// Aiport Locations grid for policy types dealing with airports.
        /// </summary>
        /// <param name="riskId">Risk Id related to the airports.</param>
        /// <returns></returns>
        public ActionResult RiskLocationsGrid(int riskId)
        {
            var locations = ServiceLocator.EntityService.GetList<RiskLoction>(new PropertyFilter("RiskId", riskId));
            ViewBag.RiskId = riskId;
            return PartialView(locations);
        }

        [HttpPost]
        public ActionResult DeclineRiskForm(Risk entity)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    entity = ServiceLocator.RiskService.GetRisk(entity.Id);
                    entity = WebUtils.FormDataToModel<Risk>(entity, Request.Form);
                    ServiceLocator.PolicySvc.UpdateRisk(entity);
                    ModelState.Clear();
                }
                catch (ValidationRulesException ex)
                {
                    ProcessValidationException(ex);
                }
            }

            return PartialView(entity);
        }

        public string GetBrokerCommission(string agencyId = "", int productLine = 0, string suffix = null, string prefix = null)
        {
            suffix = (suffix == null || String.IsNullOrEmpty(suffix)) ? "00" : suffix;
            var renewal = Convert.ToInt32(suffix);
            var commissions = ServiceLocator.EntityService.GetInstance<Commission>(new PropertyFilter("AgencyID", agencyId));
            var commission = 0.0m;
            bool isRenewal = (renewal > 1);

            if (commissions != null)
            {
                switch (productLine)
                {
                    case (int)ProductLines.AG:
                        commission = (!isRenewal) ? commissions.NewAG : commissions.ReNewAG;
                        break;
                    case (int)ProductLines.AL:
                        //commission = (!isRenewal) ? commissions.NewAirportCom : commissions.RenewAirportCom;
                        break;
                    case (int)ProductLines.COR:
                        commission = (!isRenewal) ? commissions.NewCorp : commissions.RenewCorp;
                        break;
                    case (int)ProductLines.MP:
                        commission = (!isRenewal) ? commissions.NewManProd : commissions.RenewManProd;
                        break;
                    case (int)ProductLines.PB:
                        commission = (!isRenewal) ? commissions.NewPB : commissions.RenewPB;
                        break;
                    case (int)ProductLines.PROP:
                        //commission = (!isRenewal) ? commissions. : commissions.RenewPB;
                        break;
                    case (int)ProductLines.WC:
                        commission = (!isRenewal) ? commissions.NewWC : commissions.RenewWC;
                        break;
                    default:
                        commission = 0.0m;
                        break;
                }

                if (productLine == (int)ProductLines.AGE)
                {
                    if (prefix == "HL") commission = (!isRenewal) ? commissions.NewHL : commissions.RenewHL;
                    else commission = (!isRenewal) ? commissions.NewAirportNonCo : commissions.RenewAirportNonCo;
                }

                if (productLine == (int)ProductLines.COM)
                {
                    if (prefix == "AP") commission = (!isRenewal) ? commissions.NewComm : commissions.RenewComm;
                    else if (prefix == "AVC") commission = (!isRenewal) ? commissions.NewAirportCom : commissions.RenewAirportCom;
                }
            }

            return Convert.ToString(commission);
        }

        public bool IsBrokerLicensedForState(string agencyId, string state, string riskId)
        {
            return ServiceLocator.BrokerSvc.IsBrokerLicensed(agencyId, state, Convert.ToInt32(riskId), true);
        }

        #region Combo & Grid Actions
        [IgnoreModelStateErrors]
        public ActionResult BrokerComboBox(Risk model = null)
        {
            return PartialView(model);
        }

        [IgnoreModelStateErrors]
        public ActionResult AircraftMakeComboBox(Aircraft model)
        {
            var year = (Request.Params["year"] == null) ? "" : Request.Params["year"];
            if (model == null) model = new Aircraft();
            model.Year = year;
            return PartialView(model);
        }

        [IgnoreModelStateErrors]
        public ActionResult AircraftModelComboBox(Aircraft model)
        {
            var year = (Request.Params["year"] == null) ? "" : Request.Params["year"];
            var make = (Request.Params["make"] == null) ? "" : Request.Params["make"];
            if (model == null) model = new Aircraft();
            model.Year = year;
            model.Make = make;
            return PartialView(model);
        }

        [IgnoreModelStateErrors]
        public ActionResult AirportComboBox(Aircraft model = null)
        {
            return PartialView(model);
        }

        [IgnoreModelStateErrors]
        public ActionResult RiskEditAirportComboBox(Risk model = null)
        {
            if (model == null)
            {
                model = new Risk();
            }
            return PartialView(model);
        }
        #endregion
    }

    //public class PolicyDTO
    //{
    //    public int ExposureID { get; set; }
    //    public Locations LocationID { get; set; }
    //    public String HazardState { get; set; }
    //    public Decimal EmployeePremium { get; set; }
    //    public Decimal EmployeePayroll { get; set; }
    //    public int EmployeeCount { get; set; }
    //    public Decimal StatePremium { get; set; }
    //    public WCEmployeeType EmployeeTypeID { get; set; }
    //    public WCEmployeeType EmployeeTypeName { get; set; }
    //    public ClassCodes ClassCodeID { get; set; }
    //    public ClassCodes Class { get; set; }
    //    public ClassCodes HazardGroupCode { get; set; }
    //    public Locations CompanyName { get; set; }
    //}
}
