using PolicyTracker.WebAPI.Filters;
using PolicyTracker.BusinessServices;
using PolicyTracker.DomainModel.Brokers;
using PolicyTracker.DomainModel.Common;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.DomainModel.Policy;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PolicyTracker.DomainModel.DTO;
using PolicyTracker.DomainModel.Security;

namespace WebAPI.Controllers
{
    public class SecurityController : BaseController
    {
        [HttpGet]
        [ActionName("GetResources")]
        public PaginatedList<SecurityResource> GetResources(int pageSize, int pageNumber = 1, string sortProperty = null, string sortOrder = null)
        {
            return base.GetPaginatedList<SecurityResource>(pageSize, pageNumber, sortProperty, sortOrder);
        }

        [HttpGet]
        [ActionName("GetAccess")]
        public PaginatedList<SecurityAccess> GetAccess(int pageSize, int pageNumber = 1, string sortProperty = null, string sortOrder = null)
        {
            var criteria = base.GetPaginationCriteria<SecurityAccess>(pageSize, pageNumber, sortProperty, sortOrder);

            if (criteria.Filters.Where(x => x.PropertyName == "RoleName").FirstOrDefault() != null)
            {
                var securityRole = ServiceLocator.EntityService.GetInstance<SecurityGroupRole>(new PropertyFilter("RoleName", criteria.Filters.Where(x => x.PropertyName == "RoleName").FirstOrDefault().Value));
                criteria.Filters.Add(new PropertyFilter("RoleId", securityRole.RoleId));
            }
            if (criteria.Filters.Where(x => x.PropertyName == "ResourceName").FirstOrDefault() != null)
            {
                var securityResource = ServiceLocator.EntityService.GetInstance<SecurityResource>(new PropertyFilter("Name", criteria.Filters.Where(x => x.PropertyName == "ResourceName").FirstOrDefault().Value));
                if (securityResource != null)
                    criteria.Filters.Add(new PropertyFilter("ResourceId", securityResource.Id));
                else
                    criteria.Filters.Add(new PropertyFilter("ResourceId", 0));
            }
            criteria.Filters.RemoveAll(x => x.PropertyName == "RoleName" || x.PropertyName == "ResourceName");

            var results = ServiceLocator.EntityService.GetPaginatedList<SecurityAccess>(criteria);
            var roles = ServiceLocator.EntityService.GetList<SecurityGroupRole>(new PropertyFilter("RoleId", PropertyFilter.Comparator.In, results.Results.Select(x => x.RoleId)));
            var resources = ServiceLocator.EntityService.GetList<SecurityResource>(new PropertyFilter("Id", PropertyFilter.Comparator.In, results.Results.Select(x => x.ResourceId)));

            foreach(var result in results.Results)
            {
                result.RoleName = roles.Where(x => x.RoleId == result.RoleId).First().RoleName;
                result.ResourceName = resources.Where(x => x.Id == result.ResourceId).First().Name;
            }

            return results;
        }

        [HttpDelete]
        [UseTransaction]
        [ActionName("DeleteResource")]
        public HttpResponseMessage DeleteResource(int id)
        {
            ServiceLocator.AppManagementService.DeleteSecurityResource(id);
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }

        [HttpDelete]
        [UseTransaction]
        [ActionName("DeleteAccess")]
        public HttpResponseMessage DeleteAccess(int id)
        {
            ServiceLocator.AppManagementService.DeleteSecurityAccess(id);
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }
    }
}
