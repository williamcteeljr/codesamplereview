using PolicyTracker.BusinessServices;
using PolicyTracker.DomainModel.Brokers;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.DomainModel.Policy;
using PolicyTracker.WebAPI.Filters;
using PolicyTracker.Platform.Logging;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebAPI.Controllers
{
    public class AppManagementController : BaseController
    {
        [HttpGet]
        [ActionName("GetProducts")]
        public PaginatedList<Product> GetProducts(int pageSize, int pageNumber = 1, string sortProperty = null, string sortOrder = null)
        {
            return base.GetPaginatedList<Product>(pageSize, pageNumber, sortProperty, sortOrder);
        }

        [HttpDelete]
        [UseTransaction]
        [ActionName("DeleteProduct")]
        public HttpResponseMessage DeleteProduct(int id)
        {
            ServiceLocator.AppManagementService.DeleteProduct(id);
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }

        [HttpGet]
        [ActionName("GetBudgets")]
        public PaginatedList<MonthlyBudget> GetBudgets(int pageSize, int pageNumber = 1, string sortProperty = null, string sortOrder = null)
        {
            return base.GetPaginatedList<MonthlyBudget>(pageSize, pageNumber, sortProperty, sortOrder);
        }

        [HttpDelete]
        [UseTransaction]
        [ActionName("DeleteMonthlyBudget")]
        public HttpResponseMessage DeleteMonthlyBudget(int id)
        {
            ServiceLocator.AppManagementService.DeleteMonthlyBudget(id);
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }

        [HttpGet]
        [ActionName("GetBrokerAssignments")]
        public PaginatedList<BrokerAssignment> GetBrokerAssignments(int pageSize, int pageNumber = 1, string sortProperty = null, string sortOrder = null)
        {
            var results = base.GetPaginatedList<BrokerAssignment>(pageSize, pageNumber, sortProperty, sortOrder);
            var productLines = ServiceLocator.EntityService.GetList<ProductLine>(new PropertyFilter("ProductLineId", PropertyFilter.Comparator.In, results.Results.Select(x => x.ProductLineId).Distinct()));

            foreach (var productLine in results.Results)
            {
                productLine.ProductLine = productLines.Where(x => x.ProductLineId == productLine.ProductLineId).First().Name;
            }

            return results;
        }

        [HttpDelete]
        [UseTransaction]
        [ActionName("DeleteBrokerAssignment")]
        public HttpResponseMessage DeleteBrokerAssignment(int id)
        {
            ServiceLocator.AppManagementService.DeleteBrokerAssignment(id);
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }

        [HttpGet]
        [ActionName("GetProductLines")]
        public PaginatedList<ProductLine> GetProductLines(int pageSize, int pageNumber = 1, string sortProperty = null, string sortOrder = null)
        {
            return base.GetPaginatedList<ProductLine>(pageSize, pageNumber, sortProperty, sortOrder);
        }

        [HttpGet]
        [ActionName("GetPurposesOfUse")]
        public PaginatedList<PurposeOfUse> GetPurposesOfUse(int pageSize, int pageNumber = 1, string sortProperty = null, string sortOrder = null)
        {
            return base.GetPaginatedList<PurposeOfUse>(pageSize, pageNumber, sortProperty, sortOrder);
        }

        [HttpDelete]
        [UseTransaction]
        [ActionName("DeletePurposeOfUse")]
        public HttpResponseMessage DeletePurposeOfUse(int id)
        {
            ServiceLocator.AppManagementService.DeletePurposeOfUse(id);
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }

        [HttpGet]
        [ActionName("GetReasons")]
        public PaginatedList<StatusReason> GetReasons(int pageSize, int pageNumber = 1, string sortProperty = null, string sortOrder = null)
        {
            return base.GetPaginatedList<StatusReason>(pageSize, pageNumber, sortProperty, sortOrder);
        }

        [HttpDelete]
        [UseTransaction]
        [ActionName("DeleteReason")]
        public HttpResponseMessage DeleteReason(int id)
        {
            ServiceLocator.AppManagementService.DeleteReason(id);
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }

        [HttpGet]
        [ActionName("LogJavascriptError")]
        public HttpResponseMessage LogJavascriptError(string errorMsg, string processName)
        {
            LogManager.Log(LogLevel.ERROR, string.Format("*Javascript Error* [{0}] {1}", processName, errorMsg));
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }
    }
}
