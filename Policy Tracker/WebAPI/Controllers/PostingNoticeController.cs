using PolicyTracker.BusinessServices;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.DomainModel.Policy;
using PolicyTracker.WebAPI.Filters;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebAPI.Controllers
{
    public class PostingNoticeController : BaseController
    {
        [HttpGet]
        [ActionName("SendPostingNotice")]
        public string SendPostingNotice(int RiskId, string TypeOfPayLoad, int SentById, int? LocationId = null, int? AdditionalInsuredId = null)
        {
            string ConfirmationCode = PostingNoticeService.SendPostingNotice(TypeOfPayLoad, RiskId, SentById, LocationId, AdditionalInsuredId);
            return ConfirmationCode;
        }


        #region Additional Insured Data
        //Delete Additional Named Insured
        [HttpDelete]
        [UseTransaction]
        [ActionName("DeleteAdditionalInsured")]
        public HttpResponseMessage DeleteAdditionalInsured(int id)
        {
            ServiceLocator.UIPostingNoticeService.DeleteAdditionalInsured(id);
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }

        [HttpGet]
        [ActionName("GetAdditionalNamedInsured")]
        public dynamic GetAdditionalNamedInsured(String id, int pageSize, int pageNumber = 1, string sortProperty = null, string sortOrder = null)
        {
            return base.GetPaginatedList<AdditionalNamedInsured>(pageSize, pageNumber, sortProperty, sortOrder, new PropertyFilter("QuoteId", id));
        }

        #endregion

        #region Additional Locations Data

        //Delete Additional Locations
        [HttpDelete]
        [UseTransaction]
        [ActionName("DeleteAdditionalLocations")]
        public HttpResponseMessage DeleteAdditionalLocations(int id)
        {
            ServiceLocator.UIPostingNoticeService.DeleteAdditionalLocations(id);
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }

        [HttpGet]
        [ActionName("GetAdditionalLocations")]
        public dynamic GetAdditionalLocations(String id, int pageSize, int pageNumber = 1, string sortProperty = null, string sortOrder = null)
        {
            return base.GetPaginatedList<Locations>(pageSize, pageNumber, sortProperty, sortOrder, new PropertyFilter("QuoteId", id));
        }

        #endregion

        #region Exposure/Hazard Group functions

        //Delete Hazard Group
        [HttpDelete]
        [UseTransaction]
        [ActionName("DeleteHazardGroup")]
        public HttpResponseMessage DeleteHazardGroup(int id)
        {
            ServiceLocator.UIPostingNoticeService.DeleteHazardGroup(id);
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }

        [HttpGet]
        [ActionName("GetHazardGroupDetails")]
        public dynamic GetHazardGroupDetails(String id, int pageSize, int pageNumber = 1, string sortProperty = null, string sortOrder = null)
        {
            return base.GetPaginatedList<Exposure>(pageSize, pageNumber, sortProperty, sortOrder, new PropertyFilter("QuoteId", id));
        }

        #endregion

        #region PNConfirmation Grid View

        [HttpGet]
        [ActionName("GetPNConfirmationGrid")]
        public dynamic GetPNConfirmationGrid(String id, int pageSize, int pageNumber = 1, string sortProperty = null, string sortOrder = null)
        {
            return base.GetPaginatedList<PNConfirmation>(pageSize, pageNumber, sortProperty, sortOrder, new PropertyFilter("QuoteId", id));
        }

        #endregion


    }

}
