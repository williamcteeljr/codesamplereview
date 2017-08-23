using PolicyTracker.DataAccess;
using PolicyTracker.DataAccess.WCPostingNotice;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.DomainModel.Policy;
using PolicyTracker.Platform.UOW;
using System;
using System.Xml;
using System.Configuration;
using System.Collections;
using DynaFormWrapper;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using PolicyTracker.DomainModel.PostNotice;

namespace PolicyTracker.BusinessServices
{
    public class UIPostingNoticeService
    {
        #region Additional Named Insured Functions

        //Delete Additional Insured
        public void DeleteAdditionalInsured(int id = 0)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var nI = DAOFactory.GetDAO<AdditionalNamedInsuredDAO>().GetInstance(uow, new PropertyFilter[] { new PropertyFilter("AdditionalNamedInsuredId", id) });

            DAOFactory.GetDAO<AdditionalNamedInsuredDAO>().Delete(uow, nI);
        }

        //Save New Additional Insured
        public AdditionalNamedInsured SaveNewAdditionalInsured(AdditionalNamedInsured aNI)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var location = new Locations();
            if (aNI.AdditionalNamedInsuredId == 0)
            {
                //Insert Additional Named Insured Data
                List<AdditionalNamedInsured> _List = UIWCPNServiceDAOCalls.View.InsertAdditionalNamedInsured(aNI.QuoteId, aNI.FEIN, aNI.CompanyName, aNI.Name2, aNI.StreetAddress1
                    , aNI.StreetAddress2, aNI.City, aNI.State, aNI.Zip, aNI.EmployeeCount, aNI.CreatedDate, aNI.UpdatedDate);

                //Get Newly Created Additional Named Insured ID
                int ID = 0;
                if (_List.Count > 0)
                {
                    //Get First Index Only
                    ID = _List[0].AdditionalNamedInsuredId;
                }

                //Get all selected Locations form List Box
                if (aNI.SelectedItemsIds != null)
                {
                    foreach (object obj in aNI.SelectedItemsIds)
                    {
                        //Insert a new location into the Mapping Table
                        UIWCPNServiceDAOCalls.View.InsertMappingBinding(aNI.QuoteId, ID, Convert.ToInt32(obj));
                    }
                }
            }

            if (aNI.AdditionalNamedInsuredId != 0)
            {
                //Update Additional Named Insured Data
                List<AdditionalNamedInsured> _LocList = UIWCPNServiceDAOCalls.View.UpdateAdditionalNamedInsured(aNI.AdditionalNamedInsuredId, aNI.QuoteId, aNI.FEIN, aNI.CompanyName,
                    aNI.Name2, aNI.StreetAddress1, aNI.StreetAddress2, aNI.City, aNI.State, aNI.Zip, aNI.EmployeeCount, aNI.UpdatedDate);

                //Delete all mapped records assigned to AdditionalNamedIsured by ID
                UIWCPNServiceDAOCalls.View.DeleteMappingBindingByID(aNI.AdditionalNamedInsuredId);


                //Loop all selected Locations
                if (_LocList[0].AdditionalNamedInsuredId != 0)
                {
                    if (aNI.SelectedItemsIds != null)
                    {
                        foreach (object obj in aNI.SelectedItemsIds)
                        {
                            //Insert a new location into the Mapping Table
                            UIWCPNServiceDAOCalls.View.InsertMappingBinding(aNI.QuoteId, aNI.AdditionalNamedInsuredId, Convert.ToInt32(obj));
                        }
                    }
                }
            }

            //Reload Option List Data
            List<WCPnLocationDomain> locations = new List<WCPnLocationDomain>();
            locations = WCPNServiceDAOCalls.View.getAlllocations(aNI.QuoteId);
            aNI.OptionList = new MultiSelectList(locations, "Id", "Name");

            //Load Selected Mapped Data
            //Get all assigned Locations mapped to QuoteId, AdditionalNamedInsuredId
            locations = new List<WCPnLocationDomain>();
            locations = UIWCPNServiceDAOCalls.View.SelectMappingBindingByID(aNI.AdditionalNamedInsuredId);
            //Add location items to int Array List
            var itemList = new List<int>();
            for (int i = 0; i < locations.Count; i++)
            {
                itemList.Add(locations[i].Id);
            }
            aNI.SelectedItemsIds = itemList.ToArray();

            return aNI;
        }

        public AdditionalNamedInsured GetAdditionalNamedInsuredById(int quoteId)
        {
            var aNI = new AdditionalNamedInsured() { QuoteId = quoteId };
            return aNI;
        }

        public AdditionalNamedInsured GetAdditionalNamedInsured(int addNI)
        {

            var aNI = ServiceLocator.EntityService.GetInstance<AdditionalNamedInsured>(new PropertyFilter("AdditionalNamedInsuredId", addNI));
            return aNI;
        }


        #endregion

        #region Locations Functions

        //SaveNewAdditionalLocations
        public Locations SaveNewAdditionalLocations(Locations locations)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();

            if (locations.LocationId == 0)
            {
                DAOFactory.GetDAO<LocationsDAO>().Create(uow, locations);
            }
            if (locations.LocationId != 0)
            {
                DAOFactory.GetDAO<LocationsDAO>().Update(uow, locations);
            }

            return locations;
        }

        //DeleteAdditionalLocations
        public void DeleteAdditionalLocations(int id = 0)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var loc = DAOFactory.GetDAO<LocationsDAO>().GetInstance(uow, new PropertyFilter[] { new PropertyFilter("LocationId", id) });

            DAOFactory.GetDAO<LocationsDAO>().Delete(uow, loc);
        }

        //GetLocation
        public Locations GetLocation(int locationId)
        {
            var location = ServiceLocator.EntityService.GetInstance<Locations>(new PropertyFilter("LocationId", locationId));
            return location;
        }

        //GetLocationById
        public Locations GetNewLocationById(int id)
        {
            var location = new Locations() { QuoteId = id };
            return location;
        }

        #endregion

        #region Hazard Group functions

        public Exposure GetHazardGroupById(int id)
        {
            var exposure = new Exposure() { QuoteId = id };
            return exposure;
        }

        public Exposure GetHazardGroup(int exposureId)
        {
            var exposure = ServiceLocator.EntityService.GetInstance<Exposure>(new PropertyFilter("ExposureID", exposureId));
            return exposure;
        }

        public Exposure SaveNewHazardGroup(Exposure exposure)
        {
            //Create new exposure/Hazard Group or update existing insured validating by control number           
            if (exposure.ExposureID == 0)
            {

                //Insert Hazard Group Data
                List<Exposure> _List = UIWCPNServiceDAOCalls.View.InsertExposure(exposure.LocationID, exposure.QuoteId,
                    exposure.Class, exposure.HazardGroupCode, exposure.HazardState, exposure.EmployeePremium,
                    exposure.EmployeeTypeName, exposure.EmployeePayroll, exposure.EmployeeCount,
                    exposure.StatePremium, exposure.PolicyNumber);

            }
            else if(exposure.ExposureID != 0)
            {
                //Update Method
                List<Exposure> _LocList = UIWCPNServiceDAOCalls.View.UpdateExposure(
                    exposure.ExposureID, exposure.LocationID, exposure.QuoteId, exposure.Class, exposure.HazardGroupCode, exposure.HazardState,
                    exposure.EmployeePremium, exposure.EmployeeTypeName, exposure.EmployeePayroll, exposure.EmployeeCount, exposure.StatePremium,
                    exposure.PolicyNumber);
            }

            ////Reload Option List Data
            //List<WCPnLocationDomain> locations = new List<WCPnLocationDomain>();
            //locations = WCPNServiceDAOCalls.View.getAlllocations(exposure.QuoteId);
            //exposure.ExpOptionList = new MultiSelectList(locations, "Id", "Name");

            ////Load Selected Location Data
            ////Get all assigned Locations mapped to QuoteId, AdditionalNamedInsuredId
            //locations = new List<WCPnLocationDomain>();
            //locations = UIWCPNServiceDAOCalls.View.SelectLocationByExposureId(exposure.ExposureID);
            ////Add location items to int Array List
            //var itemList = new List<int>();
            //for (int i = 0; i < locations.Count; i++)
            //{
            //    itemList.Add(locations[i].Id);
            //    break;//kill loop
            //}
            ////Set Model Value
            //exposure.ExpSelectedItemsIds = itemList.ToArray();

            return exposure;
        }

        public void DeleteHazardGroup(int exposureId)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var exposure = DAOFactory.GetDAO<ExposureDAO>().GetInstance(uow, new PropertyFilter[] { new PropertyFilter("ExposureID", exposureId) });
            DAOFactory.GetDAO<ExposureDAO>().Delete(uow, exposure);
        }


        #endregion

        #region PNConfirmation

        public PNConfirmation GetPNConfirmationById(int pNConfirmationID)
        {
            var pNC = new PNConfirmation() { PNConfirmationId = pNConfirmationID };
            return pNC;
        }

        #endregion
    }
    public class UIWCPNServiceDAOCalls
    {
        public class View
        {
            #region Mapping Table Functions

            public static List<WCPnLocationDomain> SelectMappingBindingByID(int additionalNamedInsuredId)
            {
                var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
                var dao = DAOFactory.GetDAO<AddInsuredLocationMappingDAO>();
                var results = dao.SelectMappingBindingByID(uow, additionalNamedInsuredId).ToList();
                return results;
            }

            public static List<AddInsuredLocationMapping> InsertMappingBinding(int quoteId, int insuredId, int locationId)
            {
                var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
                var dao = DAOFactory.GetDAO<AddInsuredLocationMappingDAO>();
                var results = dao.InsertMappingBinding(uow, quoteId.ToString(), insuredId, locationId).ToList();
                return results;
            }

            public static List<AddInsuredLocationMapping> InsertMappingBindingMain(int quoteId, int locationId)
            {
                var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
                var dao = DAOFactory.GetDAO<AddInsuredLocationMappingDAO>();
                var results = dao.InsertMappingBindingMain(uow, quoteId.ToString(), locationId).ToList();
                return results;
            }

            public static List<AddInsuredLocationMapping> UpdateMappingBinding(int insuredId, int locationId, int QuoteId)
            {
                var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
                var dao = DAOFactory.GetDAO<AddInsuredLocationMappingDAO>();
                var results = dao.UpdateMappingBinding(uow, insuredId, locationId, QuoteId).ToList();
                return results;
            }

            public static List<AddInsuredLocationMapping> DeleteMappingBindingByID(int additionalNamedInsuredId)
            {
                var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
                var dao = DAOFactory.GetDAO<AddInsuredLocationMappingDAO>();
                var results = dao.DeleteMappingBindingByID(uow, additionalNamedInsuredId).ToList();
                return results;
            }

            public static List<AddInsuredLocationMapping> DeleteMappingBindingByQuoteID(int quoteId)
            {
                var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
                var dao = DAOFactory.GetDAO<AddInsuredLocationMappingDAO>();
                var results = dao.DeleteMappingBindingByQuoteID(uow, quoteId).ToList();
                return results;
            }

            #endregion

            #region Insert/Update Additional Named Insured

            public static List<AdditionalNamedInsured> InsertAdditionalNamedInsured(int quoteId, String fein,
            String companyName, String name2, String address1, String address2, String city, String state, String zip, String employeeCount,
            DateTime createdDate, DateTime updatedDate)
            {
                var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
                var dao = DAOFactory.GetDAO<AdditionalNamedInsuredDAO>();
                var results = dao.InsertAdditionalNamedInsured(uow, quoteId, fein,
                    companyName, name2, address1, address2, city, state, zip, employeeCount,
                    createdDate, updatedDate).ToList();
                return results;
            }

            public static List<AdditionalNamedInsured> UpdateAdditionalNamedInsured(int AdditionalNamedInsuredId, int quoteId, String fein,
            String companyName, String name2, String address1, String address2, String city, String state, String zip, String employeeCount,
            DateTime updatedDate)
            {
                var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
                var dao = DAOFactory.GetDAO<AdditionalNamedInsuredDAO>();
                var results = dao.UpdateAdditionalNamedInsured(uow, AdditionalNamedInsuredId, quoteId, fein,
                    companyName, name2, address1, address2, city, state, zip, employeeCount,
                    updatedDate).ToList();
                return results;
            }

            #endregion

            #region Insert/Update Exposure/Hazard Group
                        
            public static List<Exposure> InsertExposure(int locationId, int quoteId, /*int employeeTypeId,
            int classCodeId,*/ String classCode, String hazardGroupCode, String hazardState, float employeePremium,
            String employeeTypeName, float employeePayroll, String employeeCount,
            float statePremium, String policyNumber)
            {
                var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
                var dao = DAOFactory.GetDAO<ExposureDAO>();
                var results = dao.InsertExposure(uow, locationId, quoteId, /*employeeTypeId,
                    classCodeId,*/ classCode, hazardGroupCode, hazardState, employeePremium,
                    employeeTypeName, employeePayroll, employeeCount,
                    statePremium, policyNumber).ToList();
                return results;
            }

            public static List<Exposure> UpdateExposure(int exposureId, int locationId, int quoteId, /*int employeeTypeId,
            int classCodeId,*/ String classCode, String hazardGroupCode, String hazardState, float employeePremium,
            String employeeTypeName, float employeePayroll, String employeeCount, float statePremium, String policyNumber)
            {
                var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
                var dao = DAOFactory.GetDAO<ExposureDAO>();
                var results = dao.UpdateExposure(uow, exposureId, locationId, quoteId, /*employeeTypeId,
                    classCodeId,*/ classCode, hazardGroupCode, hazardState, employeePremium, employeeTypeName, employeePayroll, employeeCount,
                    statePremium, policyNumber).ToList();
                return results;
            }

            #endregion

            public static List<WCPnLocationDomain> SelectLocationByExposureId(int exposureId)
            {
                var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
                var dao = DAOFactory.GetDAO<ExposureDAO>();
                var results = dao.SelectLocationByExposureId(uow, exposureId).ToList();
                return results;
            }

        }
    }



}
