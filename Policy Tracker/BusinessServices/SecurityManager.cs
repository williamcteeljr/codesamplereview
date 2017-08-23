using PolicyTracker.Platform.Logging;
using PolicyTracker.DataAccess;
using PolicyTracker.DataAccess.Security;
using PolicyTracker.DomainModel.Framework;
using PolicyTracker.DomainModel.Security;
using PolicyTracker.Platform.Security;
using PolicyTracker.Platform.UOW;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PolicyTracker.BusinessServices
{
    public static class SecurityManager
    {
        public enum ResourcePrivilege
        {
            Read,
            Write,
            Delete
        }

        public static UserSession StartSession(User user)
        {
            LogManager.Log(LogLevel.INFO, String.Format("Initializing Session For {0} {1}", user.FirstName, user.LastName));

            var existingSession = SessionManager.GetExistingSession(user.UserId);
            if (existingSession != null)
            {
                SessionManager.SetCurrentSession(existingSession);
                return existingSession;
            }

            UserSession newSession = new UserSession()
            {
                User = user,
                StartTime = DateTime.Now,
            };

            var roleAssignments = ServiceLocator.EntityService.GetList<UserRoleAssignment>(new PropertyFilter("UserId", user.UserId));
            user.SecurityRoles = ServiceLocator.EntityService.GetList<SecurityGroupRole>(new PropertyFilter("RoleId", PropertyFilter.Comparator.In, roleAssignments.Select(x => x.RoleId).Distinct()));
            user.SecurityGroups = ServiceLocator.EntityService.GetList<SecurityGroup>(new PropertyFilter("GroupId", PropertyFilter.Comparator.In, user.SecurityRoles.Select(x => x.GroupId).Distinct()));
            
            var uow = UnitOfWorkFactory.CreateUnitOfWork();

            // Load User Security Privileges
            var privileges = DAOFactory.GetDAO<SecurityAccessDAO>().GetUserSecurityPrivileges(uow, user.UserId);
            if (privileges.Count() == 0)
            {
                //throw new AuthorizationException("Insufficient Access");
            }
            var userPrivileges = new List<UserPrivilege>();
            foreach (var priv in privileges)
            {
                var userPrivilege = new UserPrivilege()
                {
                    RoleId = priv.RoleId,
                    RoleName = priv.RoleName,
                    GroupId = priv.GroupId,
                    GroupName = priv.GroupName,
                    ResourceId = priv.Name,
                    Resource = priv.ResourcePath,
                    HasAccess = priv.Read,
                    HasCreate = priv.Write,
                    HasEdit = priv.Write,
                    HasDelete = priv.Delete
                };
                userPrivileges.Add(userPrivilege);
            }
            newSession.SecurityAccess = userPrivileges;
            uow.Finish();

            newSession = SessionManager.CreateSession(newSession);

            LogManager.Log(LogLevel.INFO, String.Format("SessionId for {0} {1} is {2}", user.FirstName, user.LastName, newSession.SessionId));

            return newSession;
        }

        public static bool HasAccess(string resourcePath, ResourcePrivilege access)
        {
            // Process the resource paths from most specific to least specific.
            // Stop when an explicit grant or deny has been assigned.
            // If there are conflicting permissions, least restrictive wins.
            UserSession session = SessionManager.GetCurrentSession();
            var currentPath = resourcePath;
            while (!String.IsNullOrEmpty(currentPath))
            {
                var rights = session.SecurityAccess.Where(x => x.Resource.Equals(currentPath));
                foreach (var right in rights)
                {
                    if (ResourcePrivilege.Read.Equals(access) && right.HasAccess) return true;
                    if (ResourcePrivilege.Write.Equals(access) && (right.HasCreate || right.HasEdit)) return true;
                    if (ResourcePrivilege.Delete.Equals(access) && right.HasDelete) return true;
                }

                // If the privilege was found and not granted, it was denied
                // If no privilege was found and we have reached "root" level, deny access
                if ((rights.Count() > 0) || currentPath.Equals("/"))
                {
                    LogManager.Log(LogLevel.DEBUG, "User [" + session.User.UserName + "] does not have " + Enum.GetName(typeof(ResourcePrivilege), access)
                        + " rights to resource [" + resourcePath + "]");
                    return false;
                }

                int nextPathIndex = currentPath.LastIndexOf('/');
                currentPath = (nextPathIndex > 0) ? currentPath.Substring(0, nextPathIndex) : "/";
            }

            // Failsafe to return false
            return false;
        }

        public static bool InGroup(IEnumerable<string> allowedGroups)
        {
            var user = SessionManager.GetCurrentSession().User;
            var groupCount = user.SecurityGroups.Count();
            var hasAccess = false;

            for(var i = 0; i < groupCount; i++)
            {
                var group = user.SecurityGroups.ElementAt(i);
                hasAccess = (allowedGroups.FirstOrDefault(x => x == group.GroupName) != null);
                if (hasAccess) i = groupCount;
            }

            return hasAccess;
        }

        public static bool InRole(IEnumerable<string> roles)
        {
            var user = SessionManager.GetCurrentSession().User;
            var roleCount = user.SecurityRoles.Count();
            var inRole = false;

            roles = roles.Select(r => r.ToUpper()).ToArray();

            for (var i = 0; i < roleCount; i++)
            {
                var role = user.SecurityRoles.ElementAt(i);
                inRole = (roles.FirstOrDefault(x => x == role.RoleName.ToUpper()) != null);
                if (inRole) i = roleCount;
            }

            return inRole;
        }
    }
}
