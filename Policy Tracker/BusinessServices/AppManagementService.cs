using Microsoft.VisualBasic.FileIO;
using PolicyTracker.DataAccess;
using PolicyTracker.DataAccess.Brokers;
using PolicyTracker.DataAccess.Policy;
using PolicyTracker.DataAccess.Security;
using PolicyTracker.DomainModel.Brokers;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.DomainModel.Policy;
using PolicyTracker.DomainModel.Security;
using PolicyTracker.Platform.Logging;
using PolicyTracker.Platform.UOW;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace PolicyTracker.BusinessServices
{
    public class AppManagementService 
    {
        public SecurityResource SaveSecurityResource(SecurityResource resource)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();

            if (resource.Id == 0)
                DAOFactory.GetDAO<SecurityResourceDAO>().Create(uow, resource);
            else
                DAOFactory.GetDAO<SecurityResourceDAO>().Update(uow, resource);

            return resource;
        }

        public SecurityAccess SaveSecurityAccess(SecurityAccess access)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();

            if (access.Id == 0)
                DAOFactory.GetDAO<SecurityAccessDAO>().Create(uow, access);
            else
                DAOFactory.GetDAO<SecurityAccessDAO>().Update(uow, access);

            return access;
        }

        public MonthlyBudget SaveMonthlyBudget(MonthlyBudget budget)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();

            if (budget.BudgetId == 0)
                DAOFactory.GetDAO<MonthlyBudgetsDAO>().Create(uow, budget);
            else
                DAOFactory.GetDAO<MonthlyBudgetsDAO>().Update(uow, budget);

            return budget;
        }

        public void DeleteMonthlyBudget(int id)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            DAOFactory.GetDAO<MonthlyBudgetsDAO>().Delete(uow, new PropertyFilter("BudgetId", id)); 
        }

        public BrokerAssignment SaveBrokerAssignment(BrokerAssignment assignment)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var underwriter = ServiceLocator.EntityService.GetInstance<User>(new PropertyFilter("UserId", assignment.UserId));
            assignment.UserName = underwriter.UserName;

            if (assignment.AssignmentId == 0)
                DAOFactory.GetDAO<BrokerAssignmentDAO>().Create(uow, assignment);
            else
                DAOFactory.GetDAO<BrokerAssignmentDAO>().Update(uow, assignment);

            return assignment;
        }

        public Product SaveProduct(Product product)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();

            if (product.ProductId == 0)
                DAOFactory.GetDAO<ProductsDAO>().Create(uow, product);
            else
                DAOFactory.GetDAO<ProductsDAO>().Update(uow, product);

            return product;
        }

        public PurposeOfUse SavePurposeOfUse(PurposeOfUse purpose)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();

            if (purpose.Id == 0)
                DAOFactory.GetDAO<PurposeOfUseDAO>().Create(uow, purpose);
            else
                DAOFactory.GetDAO<PurposeOfUseDAO>().Update(uow, purpose);

            return purpose;
        }

        public void DeleteBrokerAssignment(int id)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            DAOFactory.GetDAO<BrokerAssignmentDAO>().Delete(uow, new PropertyFilter("AssignmentId", id));
        }

        public void DeletePurposeOfUse(int id)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            DAOFactory.GetDAO<PurposeOfUseDAO>().Delete(uow, new PropertyFilter("Id", id));
        }

        public void DeleteProduct(int id)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            DAOFactory.GetDAO<ProductsDAO>().Delete(uow, new PropertyFilter("ProductId", id));
        }

        public void DeleteSecurityResource(int id)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();

            List<ValidationResult> valResults = new List<ValidationResult>();
            if (ServiceLocator.EntityService.Exists<SecurityAccess>(new PropertyFilter("ResourceId", id)))
                valResults.Add(new ValidationResult("Resource has associated access records.", new[] { "errors" }));

            if (valResults.FirstOrDefault() != null) throw new ValidationRulesException(valResults);

            DAOFactory.GetDAO<SecurityResourceDAO>().Delete(uow, new PropertyFilter("Id", id));
        }

        public void DeleteSecurityAccess(int id)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            DAOFactory.GetDAO<SecurityAccessDAO>().Delete(uow, new PropertyFilter("Id", id));
        }

        public StatusReason SaveReason(StatusReason reason)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();

            if (reason.Id == 0)
                DAOFactory.GetDAO<StatusReasonDAO>().Create(uow, reason);
            else
                DAOFactory.GetDAO<StatusReasonDAO>().Update(uow, reason);

            return reason;
        }

        public void DeleteReason(int id)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            DAOFactory.GetDAO<StatusReasonDAO>().Delete(uow, new PropertyFilter("Id", id));
        }

        public void BrokerAssignmentImport(string filePath, int productLineStartCol, int productLineEndCol)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var plStart = productLineStartCol;
            var plEnd = productLineEndCol;
            var productLines = ServiceLocator.EntityService.GetList<ProductLine>();
            var rowCount = 0; //Prevent importing header row.

            TextFieldParser parser = new TextFieldParser(filePath);
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");

            while (!parser.EndOfData)
            {
                string[] row = parser.ReadFields();

                if (row[plEnd + 1].Split(' ').Length > 1 && rowCount > 0)
                {
                    try
                    {
                        for (var i = plStart; i <= plEnd; i++)
                        {
                            var pl = row[i].ToUpper();

                            if (!String.IsNullOrEmpty(pl))
                            {
                                int plId = 0;

                                switch (pl)
                                {
                                    case "AGRICULTURE":
                                        plId = productLines.Where(x => x.Name == "Agriculture").First().ProductLineId;
                                        break;
                                    case "CORPORATE":
                                        plId = productLines.Where(x => x.Name == "Corporate").First().ProductLineId;
                                        break;
                                    case "MANUFACTURED PRODUCTS":
                                        plId = productLines.Where(x => x.Name == "Manufactured Products").First().ProductLineId;
                                        break;
                                    case "AIRPORT":
                                        plId = productLines.Where(x => x.Name == "Airports & Govt Entity").First().ProductLineId;
                                        break;
                                    case "COMMERCIAL":
                                        plId = productLines.Where(x => x.Name == "Commercial").First().ProductLineId;
                                        break;
                                    case "SR":
                                        plId = productLines.Where(x => x.Name == "Airports & Govt Entity").First().ProductLineId;
                                        break;
                                    case "WC":
                                        plId = productLines.Where(x => x.Name == "Workers Comp").First().ProductLineId;
                                        break;
                                }

                                //var name = row[plEnd + 1].Split(' ');
                                var initials = row[plEnd + 2];

                                //var user = ServiceLocator.EntityService.GetInstance<User>(new PropertyFilter[] { new PropertyFilter("FirstName", name[0]), new PropertyFilter("LastName", name[1]) });
                                var user = ServiceLocator.EntityService.GetInstance<User>(new PropertyFilter("Initials", initials));

                                if (user != null)
                                {
                                    var assignment = new BrokerAssignment()
                                    {
                                        BrokerCode = row[1],
                                        ProductLineId = plId,
                                        UserName = user.UserName,
                                        UserId = user.UserId
                                    };
                                    var exists = ServiceLocator.EntityService.Exists<BrokerAssignment>(new[] { new PropertyFilter("BrokerCode", assignment.BrokerCode), new PropertyFilter("UserId", assignment.UserId), new PropertyFilter("ProductLineId", assignment.ProductLineId) });
                                    if (!exists)
                                    {
                                        DAOFactory.GetDAO<BrokerAssignmentDAO>().Create(uow, assignment);
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                }
                rowCount++;
            }
            parser.Close();
        }

        public void MergeDuplicateNamedInsuredByPolicyNumber()
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var filters = new List<PropertyFilter>();
            var policies = DAOFactory.GetDAO<RiskDAO>().GetUniquePolicies(uow);
            
            foreach (var pol in policies)
            {
                filters = new List<PropertyFilter>()
                {
                    new PropertyFilter("Prefix", pol.Prefix),
                    new PropertyFilter("PolicyNumber", pol.PolicyNumber)
                };

                var policyBunch = DAOFactory.GetDAO<RiskDAO>().GetList(uow, filters);

                if(policyBunch.Count() > 0)
                {
                    try
                    {
                        var controlNumbersToDelete = new List<int>();
                        var newestIterationOfPolicy = policyBunch.OrderByDescending(x => Convert.ToInt16(x.PolicySuffix)).First();
                        var newestNamedInsured = DAOFactory.GetDAO<NamedInsuredDAO>().GetInstance(uow, new PropertyFilter("ControlNumber", newestIterationOfPolicy.ControlNumber));

                        foreach (var policy in policyBunch)
                        {
                            if (policy.ControlNumber != newestNamedInsured.ControlNumber)
                            {
                                controlNumbersToDelete.Add(policy.ControlNumber);
                                policy.ControlNumber = newestNamedInsured.ControlNumber;
                            }
                        }

                        DAOFactory.GetDAO<RiskDAO>().Update(uow, policyBunch);
                        DAOFactory.GetDAO<NamedInsuredDAO>().Delete(uow, new PropertyFilter("ControlNumber", PropertyFilter.Comparator.In, controlNumbersToDelete));
                    }
                    catch (Exception ex)
                    {
                        LogManager.Log(LogLevel.ERROR, "NIM|" + pol.PolicyNumber + " " + ex.Message);
                    }
                    
                }
            }
        }
    }
}
