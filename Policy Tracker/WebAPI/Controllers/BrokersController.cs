using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PolicyTracker.DomainModel.Brokers;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.BusinessServices;

namespace WebAPI.Controllers
{
    public class BrokersController : BaseController
    {
        [HttpGet]
        public PaginatedList<Broker> Search(int pageNumber, int pageSize, string term)
        {
            var filters = new List<PropertyFilter>()
            {
                new PropertyFilter("AgencyID", PropertyFilter.Comparator.StartsWith, term),
                new PropertyFilter("AgencyName", PropertyFilter.Comparator.StartsWith, term, 1)
            };

            return base.GetPaginatedList<Broker>(25, 1, "AgencyID", "asc", filters);
        }

        [HttpGet]
        public Broker GetBroker(string brokerCode)
        {
            return ServiceLocator.EntityService.GetInstance<Broker>(new PropertyFilter("AgencyID", brokerCode));
        }

        [HttpGet]
        public IEnumerable<Agent> GetBrokerAgents(string brokerCode)
        {
            return ServiceLocator.BrokerSvc.GetAgentsForBroker(brokerCode);
        }
    }
}
