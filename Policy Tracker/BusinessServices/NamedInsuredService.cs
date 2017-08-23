using PolicyTracker.DataAccess;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.Platform.UOW;
using PolicyTracker.DomainModel.DTO;
using PolicyTracker.DomainModel.Policy;
using PolicyTracker.DataAccess.Policy;

namespace PolicyTracker.BusinessServices
{
    public class NamedInsuredService
    {
        public void UpdateNamedInsured(int riskId)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var risk = ServiceLocator.EntityService.GetInstance<Risk>(new PropertyFilter("Id", riskId));
            NamedInsured namedInsured = ServiceLocator.EntityService.GetInstance<NamedInsured>(new PropertyFilter("ControlNumber", risk.ControlNumber));

            if (namedInsured != null)
            {
                namedInsured.FirstName = risk.FirstName;
                namedInsured.LastName = risk.LastName;
                namedInsured.CompanyName = risk.CompanyName;
                namedInsured.Fein = risk.Fein;
                namedInsured.StreetAddress1 = risk.StreetAddress1;
                namedInsured.StreetAddress2 = risk.StreetAddress2;
                namedInsured.MiddleInitial = risk.MiddleInitial ?? "";
                namedInsured.DoingBusinessAs = risk.DoingBusinessAs ?? "";
                namedInsured.City = risk.City;
                namedInsured.State = risk.State;
                namedInsured.Zip = risk.Zip;
                DAOFactory.GetDAO<NamedInsuredDAO>().Update(uow, namedInsured);
            }
        }
    }
}
