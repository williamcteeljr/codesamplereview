using PolicyTracker.DataAccess;
using PolicyTracker.DataAccess.AircraftDataAccess;
using PolicyTracker.DataAccess.Policy;
using PolicyTracker.DomainModel;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.DomainModel.Policy;
using PolicyTracker.Platform.UOW;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PolicyTracker.BusinessServices
{
    public class AircraftSvc
    {
        public void ValidateAircraft(List<ValidationResult> valResults, Aircraft ac)
        {
            var risk = ServiceLocator.EntityService.GetInstance<Risk>(new PropertyFilter("Id", ac.QuoteId));

            // Check if the ID of the aircraft record is != 0 to ensure we are not dealing with an aircraft record being created from Risk Entry (Clearance). 
            // This check only applies after the risk has been entered and the UA/UWs are working on it.
            if (ac.Id != 0 && risk.Prefix == "CA" &&  ac.Liability.Limit == 0)
                valResults.Add(new ValidationResult("Aircraft on Corporate policies must include a liability limit.", new[] { "Liability.Limit" }));
        }

        public Aircraft SaveAircraft(Aircraft ac)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var valResults = new List<ValidationResult>();

            ValidateAircraft(valResults, ac);

            if (valResults.Count > 0) throw new ValidationRulesException(valResults);
            ac.FAANo = ac.FAANo.ToUpper();
            //Un-Comment to start removing the N from the front of tail numbers.
            //if (ac.FAANo.StartsWith("N")) ac.FAANo = ac.FAANo.Remove(0, 1);

            if (ac.Id == 0)
            {
                DAOFactory.GetDAO<AircraftDAO>().Create(uow, ac);
                if (ac.Liability != null)
                {
                    ac.Liability.AircraftId = ac.Id;
                    DAOFactory.GetDAO<AircraftLiabilityDAO>().Create(uow, ac.Liability);
                }
            }
            else
            {
                DAOFactory.GetDAO<AircraftDAO>().Update(uow, ac);
                if (ac.Liability != null && ac.Liability.LiabilityId == 0) DAOFactory.GetDAO<AircraftLiabilityDAO>().Create(uow, ac.Liability);
                else if (ac.Liability != null && ac.Liability.LiabilityId != 0) DAOFactory.GetDAO<AircraftLiabilityDAO>().Update(uow, ac.Liability);
            }

            return ac;
        }

        public void DeleteAircraft(int riskId, int acId)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var ac = DAOFactory.GetDAO<AircraftDAO>().GetInstance(uow, new PropertyFilter[] { new PropertyFilter("QuoteId", riskId), new PropertyFilter("Id", acId)});
            ac.IsIncluded = false;
            DAOFactory.GetDAO<AircraftDAO>().Update(uow, ac);
        }

        public IEnumerable<AircraftYear> GetYears()
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var results = DAOFactory.GetDAO<AircraftYearsDAO>().GetYears(uow);
            return results;
        }

        public IEnumerable<AircraftCrossRef> GetMakes(string year)
        {
            var uow = UnitOfWorkFactory.CreateUnitOfWork(UnitOfWorkFactory._BLUEBOOK_CONTEXT);
            var results = DAOFactory.GetDAO<AircraftCrossRefDAO>().GetMake(uow, year);
            uow.Finish();
            return results;
        }

        public IEnumerable<AircraftReference> GetModels(string make = "")
        {
            var uow = UnitOfWorkFactory.CreateUnitOfWork(UnitOfWorkFactory._BLUEBOOK_CONTEXT);
            if (make == null) make = String.Empty;
            var filters = new List<PropertyFilter>();
            filters.Add(new PropertyFilter("MFR", PropertyFilter.Comparator.Like, make));
            var results = DAOFactory.GetDAO<AircraftReferenceDAO>().GetList(uow, filters);
            uow.Finish();
            return results;
        }

        public Aircraft GetNewAircraftForRisk(int riskId)
        {
            var ac = new Aircraft() { QuoteId = riskId };
            var policyInfo = ServiceLocator.EntityService.GetInstance<Risk>(new PropertyFilter("Id", riskId));
            ac.PurposeOfUse = policyInfo.PurposeOfUse;
            ac.AirportID = policyInfo.AirportId;
            ac.Liability = new AircraftLiability();
            ac.IsIncluded = true;

            return ac;
        }

        public Aircraft GetAircraft(int aircraftId)
        {
            var ac = ServiceLocator.EntityService.GetInstance<Aircraft>(new PropertyFilter("Id", aircraftId));
            var acLiability = ServiceLocator.EntityService.GetInstance<AircraftLiability>(new PropertyFilter("AircraftId", aircraftId));
            if (acLiability == null) acLiability = new AircraftLiability() { AircraftId = aircraftId };
            ac.Liability = acLiability;
            return ac;
        }

        public object LookupTailNumber(string tailNumber)
        {
            object result;

            if (tailNumber.StartsWith("N")) tailNumber = tailNumber.Remove(0, 1);
            var tailNumberRef = ServiceLocator.EntityService.GetInstance<TailNumber>(new PropertyFilter("FAA_Number", tailNumber));
            var aircraftRef = (tailNumberRef != null) ? ServiceLocator.EntityService.GetInstance<AircraftReference>(new PropertyFilter("CODE", tailNumberRef.MFR_MDL_CODE)) : new AircraftReference();

            if (tailNumberRef != null && aircraftRef != null)
                result = new { Year = tailNumberRef.YEAR_MFR, Make = aircraftRef.MFR, Model = aircraftRef.MODEL };
            else
                result = new { Year = "", Make = "", Model = "" };

            return result;
        }
    }
}