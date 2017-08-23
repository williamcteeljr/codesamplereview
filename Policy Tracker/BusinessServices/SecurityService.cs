using PolicyTracker.DataAccess;
using PolicyTracker.DataAccess.Security;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.DomainModel.Security;
using PolicyTracker.Platform.UOW;
using System.Collections.Generic;

namespace PolicyTracker.BusinessServices.Security
{
    public class SecuritySvc
    {
        public User GetUser(string id)
        {
            var uow = UnitOfWorkFactory.GetActiveUnitOfWork();
            var user = DAOFactory.GetDAO<UserEntityDAO>().GetInstance(uow, new PropertyFilter("UserId", id));
            return user;
        }

        /// <summary>
        /// Used to get the user list for DX grids since the usual request UOW is not available.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<User> GetUsers()
        {
            var uow = UnitOfWorkFactory.CreateUnitOfWork(UnitOfWorkFactory._UWBASE_CONTEXT);
            var results = DAOFactory.GetDAO<UserEntityDAO>().GetList(uow);
            uow.Finish();
            return results;
        }
    }
}
