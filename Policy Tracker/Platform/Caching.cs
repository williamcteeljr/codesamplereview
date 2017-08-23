using PolicyTracker.Platform.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Security;

namespace PolicyTracker.Platform.Caching
{
    public class CacheManager
    {
        public static readonly string USER_SESSION_KEY = "PolicyTrackerUserSession";
        public static readonly string MESSAGES_KEY = "Messages";

        public class GlobalCache
        {
            private static ObjectCache _CacheInstance = new MemoryCache("GlobalCache");
            private static CacheItemPolicy _Policy = new CacheItemPolicy() { Priority = CacheItemPriority.NotRemovable };

            public static Int32 ItemCount
            {
                get { return _CacheInstance.Count(); }
            }

            public static void SetValue(string key, Object value)
            {
                _CacheInstance.Set(key, value, _Policy);
            }

            public static Object GetValue(string key)
            {
                return _CacheInstance.Get(key);
            }
        }

        public class SessionCache
        {
            private static ObjectCache _CacheInstance = new MemoryCache("SessionCache");
            private static CacheItemPolicy _Policy = new CacheItemPolicy() { Priority = CacheItemPriority.Default, SlidingExpiration = TimeSpan.FromMinutes(80) };

            public static long SessionCount
            {
                get { return _CacheInstance.GetCount(); }
            }

            public static IEnumerable<KeyValuePair<string, object>> Sessions
            {
                get { return _CacheInstance as IEnumerable<KeyValuePair<string, object>>; }
            }

            public static void Initialize(string sessionId)
            {
                IDictionary<string, object> sessionValues = new Dictionary<string, object>();
                _CacheInstance.Set(sessionId, sessionValues, _Policy);
                LogManager.Log(LogLevel.DEBUG, "Session Cache Policy Silding Expiration Set To: {0}", new[] { _Policy.SlidingExpiration.ToString() });
            }

            public static void Clear(string sessionId)
            {
                _CacheInstance.Remove(sessionId);
            }

            public static void SetValue(string sessionId, string key, object value, bool overWrite = false)
            {
                var sessionValues = (IDictionary<String, Object>)_CacheInstance.Get(sessionId);
                if (sessionValues != null)
                {
                    if (sessionValues.ContainsKey(key) && overWrite)
                    {
                        sessionValues.Remove(key);
                        sessionValues.Add(key, value);
                    }
                    else if (!sessionValues.ContainsKey(key))
                    {
                        sessionValues.Add(key, value);
                    }
                }
            }

            public static void RemoveValue(string sessionId, string key)
            {
                var sessionValues = (IDictionary<String, Object>)_CacheInstance.Get(sessionId);
                if (sessionValues != null)
                {
                    sessionValues.Remove(key);
                }
            }

            public static object GetValue(string sessionId, string key)
            {
                object result = null;
                var sessionValues = (IDictionary<String, Object>)_CacheInstance.Get(sessionId);
                if (sessionValues != null)
                {
                    result = (sessionValues.ContainsKey(key)) ? sessionValues[key] : null;
                }

                return result;
            }
        }

        public class RequestCache
        {
            public static object GetUserSession()
            {
                return GetValue(USER_SESSION_KEY);
            }

            public static void SetValue(string key, object value)
            {
                HttpContext.Current.Items[key] = value;
            }

            public static object GetValue(string key)
            {
                return (HttpContext.Current == null) ? null : HttpContext.Current.Items[key];
            }

            public static void RemoveValue(string key)
            {
                HttpContext.Current.Items.Remove(key);
            }
        }
    }
}
