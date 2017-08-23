using System;
using System.Collections.Generic;
using PolicyTracker.Platform.Caching;
using PolicyTracker.DomainModel.Security;

namespace PolicyTracker.Platform.Security
{
    public class UserSession
    {
        //public string Company { get; set; }
        //public string CompanyDatabase { get; set; }
        public string SessionId { get; internal set; }
        //public long UserId { get; set; }
        //public string Username { get; set; }
        //public string EncryptionKey { get; set; }
        public DateTime StartTime { get; set; }
        public User User { get; set; }

        public IEnumerable<UserPrivilege> SecurityAccess { get; set; }
    }

    public class UserPrivilege
    {
        public string ResourceId { get; set; }
        public string Resource { get; set; }
        public long GroupId { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string GroupName { get; set; }
        public bool HasAccess { get; set; }
        public bool HasCreate { get; set; }
        public bool HasEdit { get; set; }
        public bool HasDelete { get; set; }
    }

    public static class SessionManager
    {
        public static readonly string SESSIONID_TOKEN = "ptsessionid";

        public static void EndSession(string sessionId)
        {
            CacheManager.SessionCache.Clear(sessionId);
        }

        public static UserSession GetSession(string sessionId)
        {
            UserSession session = CacheManager.SessionCache.GetValue(sessionId, CacheManager.USER_SESSION_KEY) as UserSession;
            return session;
        }

        public static UserSession GetCurrentSession()
        {
            return CacheManager.RequestCache.GetValue(CacheManager.USER_SESSION_KEY) as UserSession;
        }

        public static string GetCurrentSessionId()
        {
            var session = GetCurrentSession();
            string sessionId = (session == null) ? null : session.SessionId;
            return sessionId;
        }

        public static UserSession GetExistingSession(int UserId)
        {
            var sessions = CacheManager.SessionCache.Sessions;
            foreach (var session in sessions)
            {
                var sessionValues = session.Value as IDictionary<string, object>;
                var userSession = sessionValues[CacheManager.USER_SESSION_KEY] as UserSession;
                if (userSession.User.UserId.Equals(UserId)) return userSession;
            }

            return null;
        }

        public static void SetCurrentSession(UserSession session)
        {
            CacheManager.RequestCache.SetValue(CacheManager.USER_SESSION_KEY, session);
        }

        public static bool IsValidSession(string sessionId)
        {
            UserSession session = GetSession(sessionId);
            return (session != null);
        }

        public static UserSession CreateSession(UserSession newSession)
        {
            String sessionId = Guid.NewGuid().ToString().Replace("-", "");
            newSession.SessionId = sessionId;
            CacheManager.SessionCache.Initialize(sessionId);
            CacheManager.SessionCache.SetValue(sessionId, CacheManager.USER_SESSION_KEY, newSession);
            SetCurrentSession(newSession);
            // TODO:  Store Session in Database??
            return newSession;
        }

        public static object GetSessionValue(string key)
        {
            return CacheManager.SessionCache.GetValue(GetCurrentSessionId(), key);
        }

        public static void SetSessionValue(string key, object value, bool overWrite = false)
        {
            CacheManager.SessionCache.SetValue(GetCurrentSessionId(), key, value, overWrite);
        }
    }
}
