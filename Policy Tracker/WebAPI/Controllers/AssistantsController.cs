using PolicyTracker.DomainModel.Framework;
using PolicyTracker.DomainModel.Policy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebAPI.Controllers
{
    public class AssistantsController : BaseController
    {
        [HttpGet]
        [ActionName("LookupRisks")]
        public PaginatedList<RiskGraph> LookupRisks(int pageSize, int pageNumber = 1, string sortProperty = null, string sortOrder = null)
        {
            return base.GetPaginatedList<RiskGraph>(pageSize, pageNumber, sortProperty, sortOrder);
        }
    }
}
