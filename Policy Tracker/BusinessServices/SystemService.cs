using PolicyTracker.DataAccess;
using PolicyTracker.DataAccess.Policy;
using PolicyTracker.DomainModel.Common;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.DomainModel.Policy;
using PolicyTracker.Platform.UOW;
using System;
using System.Collections.Generic;

namespace PolicyTracker.BusinessServices
{
    public class SystemSvc
    {
        public UserFilter SaveUserFilter(UserFilter filter)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();

            var existsFilters = new List<PropertyFilter>();
            existsFilters.AddRange(new PropertyFilter[] {new PropertyFilter("UserId", filter.UserId), new PropertyFilter("Name", filter.Name)});
            var filterExistsForUser = DAOFactory.GetDAO<UserFilterDAO>().Exists(uow, existsFilters);
            if (filter.MonthFilter > 0) filter.MonthRangeFilter = 0;

            if (filterExistsForUser)
            {
                DAOFactory.GetDAO<UserFilterDAO>().Delete(uow, existsFilters);
            }

            DAOFactory.GetDAO<UserFilterDAO>().Create(uow, filter);
            return filter;
        }

        public void DeleteUserFilter(int userId, string name)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            if (userId != 0 && !String.IsNullOrEmpty(name))
            {
                var filter = ServiceLocator.EntityService.GetInstance<UserFilter>(new PropertyFilter[] {new PropertyFilter("UserId", userId), new PropertyFilter("Name", name)});
                DAOFactory.GetDAO<UserFilterDAO>().Delete(uow, filter); 
            }
        }

        public void DeleteUserGridConfig(int configId)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            DAOFactory.GetDAO<GridConfigDAO>().Delete(uow, new PropertyFilter("ConfigId", configId));
        }
    }
}
