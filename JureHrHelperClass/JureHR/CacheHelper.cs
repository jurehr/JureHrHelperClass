using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using System.Collections.Generic;

namespace JureHR
{
    /// <summary>
    /// Cache Management
    /// </summary>
    public class CacheHelper
    {
        private CacheHelper() { }

        //>> Based on Factor = 5 default value

        /// <summary>
        /// Day cache factor
        /// </summary>
        public static readonly int DayFactor = 17280;

        /// <summary>
        /// Hours cache factor
        /// </summary>
        public static readonly int HourFactor = 720;

        /// <summary>
        /// Sub-cache factor
        /// </summary>
        public static readonly int MinuteFactor = 12;

        /// <summary>
        /// Seconds cache factor
        /// </summary>
        public static readonly double SecondFactor = 0.2;

        private static readonly Cache cache;

        private static int Factor = 5;

        /// <summary>
        /// Reset
        /// </summary>
        /// <param name="cacheFactor"></param>
        public static void ReSetFactor(int cacheFactor)
        {
            Factor = cacheFactor;
        }

        /// <summary>
        /// Static initializer should ensure we only have to look up the current cache
        /// instance once.        
        /// </summary>
        static CacheHelper()
        {
            HttpContext context = HttpContext.Current;
            if (context != null)
            {
                cache = context.Cache;
            }
            else
            {
                cache = HttpRuntime.Cache;
            }
        }

        /// <summary>
        /// Static initializer should ensure we only have to look up the current cache
        /// </summary>
        public static void Clear()
        {
            IDictionaryEnumerator CacheEnum = cache.GetEnumerator();
            ArrayList al = new ArrayList();
            while (CacheEnum.MoveNext())
            {
                al.Add(CacheEnum.Key);
            }

            foreach (string key in al)
            {
                cache.Remove(key);
            }

        }

        /// <summary>
        /// Remove items whose key can match the given regular expression pattern string 
        /// </summary>
        /// <param name="pattern">mode</param>
        public static void RemoveByPattern(string pattern)
        {
            IDictionaryEnumerator CacheEnum = cache.GetEnumerator();
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
            while (CacheEnum.MoveNext())
            {
                if (regex.IsMatch(CacheEnum.Key.ToString()))
                    cache.Remove(CacheEnum.Key.ToString());
            }
        }

        /// <summary>
        /// Remove items whose key contains the given sub keyword string.
        /// </summary>
        /// <param name="subKeyWord"></param>
        public static object RemoveBySubKey(string subKeyWord)
        {
            IDictionaryEnumerator CacheEnum = cache.GetEnumerator();
            while (CacheEnum.MoveNext())
            {
                if (CacheEnum.Key.ToString().IndexOf(subKeyWord) != -1)
                    return cache.Remove(CacheEnum.Key.ToString());
            }
            return null;
        }

        /// <summary>
        /// Remove items whose key can match the given regular expression pattern string 
        /// </summary>
        /// <param name="key">key</param>
        public static void Remove(string key)
        {
            cache.Remove(key);
        }

        /// <summary>
        /// Object is loaded into the Cache
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="obj">object </param>
        public static void Insert(string key, object obj)
        {
            Insert(key, obj, null, 1);
        }

        /// <summary>
        /// Object is loaded into the Cache, additional cache dependency information
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="obj">object</param>
        /// <param name="dep">cache dependencies</param>
        public static void Insert(string key, object obj, CacheDependency dep)
        {
            Insert(key, obj, dep, MinuteFactor * 3);
        }

        /// <summary>
        /// Object is loaded into the Cache, additional expiration time information
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="obj">object</param>
        /// <param name="seconds">cache time (in seconds)</param>
        public static void Insert(string key, object obj, int seconds)
        {
            Insert(key, obj, null, seconds);
        }

        /// <summary>
        /// Object is loaded into the Cache, additional expiration time information and priority
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="obj">object</param>
        /// <param name="seconds">cache time (in seconds)</param>
        /// <param name="priority"> priority</param>
        public static void Insert(string key, object obj, int seconds, CacheItemPriority priority)
        {
            Insert(key, obj, null, seconds, priority);
        }

        /// <summary>
        /// Object is loaded into the Cache, additional cache dependency and expiration time (the number of seconds after expired)
        /// (Default priority is Normal)
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="obj">object</param>
        /// <param name="dep">cache dependencies</param>
        /// <param name="seconds">cache time (in seconds)</param>
        public static void Insert(string key, object obj, CacheDependency dep, int seconds)
        {
            Insert(key, obj, dep, seconds, CacheItemPriority.Normal);
        }

        /// <summary>
        /// Object is loaded into the Cache, additional cache dependency and expiration time expires (after how many seconds) and priority
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="obj">object</param>
        /// <param name="dep">cache dependencies</param>
        /// <param name="seconds">cache time (in seconds)</param>
        /// <param name="priority">priority</param>
        public static void Insert(string key, object obj, CacheDependency dep, int seconds, CacheItemPriority priority)
        {
            if (obj != null)
            {
                cache.Insert(key, obj, dep, DateTime.Now.AddSeconds(Factor * seconds), TimeSpan.Zero, priority, null);
            }

        }

        /// <summary>
        /// Object to the cache and ignore the priority
        /// </summary>
        /// <param Name="key"> key </param>
        /// <param Name="obj"> object </param>
        /// <param Name="second"> time </param>
        public static void MicroInsert(string key, object obj, int second)
        {
            if (obj != null)
            {
                cache.Insert(key, obj, null, DateTime.Now.AddSeconds(Factor * second), TimeSpan.Zero);
            }
        }

        /// <summary>
        /// Object to the cache and the expiration time is set to the maximum
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="obj">object</param>
        public static void Max(string key, object obj)
        {
            Max(key, obj, null);
        }

        /// <summary>
        /// Object to the cache and the expiration time is set to the maximum, the additional cache dependency information
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="obj">object</param>
        /// <param name="dep">cache dependencies</param>
        public static void Max(string key, object obj, CacheDependency dep)
        {
            if (obj != null)
            {
                cache.Insert(key, obj, dep, DateTime.MaxValue, TimeSpan.Zero, CacheItemPriority.AboveNormal, null);
            }
        }

        /// <summary>
        /// Insert a persistent cache
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="obj">object</param>
        public static void Permanent(string key, object obj)
        {
            Permanent(key, obj, null);
        }

        /// <summary>
        /// Insert persistent cache, additional cache dependency
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="obj">object</param>
        /// <param name="dep">cache dependencies</param>
        public static void Permanent(string key, object obj, CacheDependency dep)
        {
            if (obj != null)
            {
                cache.Insert(key, obj, dep, DateTime.MaxValue, TimeSpan.Zero, CacheItemPriority.NotRemovable, null);
            }
        }

        /// <summary>
        /// Button to get the cached object
        /// </summary>
        /// <param name="key">key</param>
        /// <returns></returns>
        public static object Get(string key)
        {
            return cache[key];
        }

        /// <summary>
        /// GetAllKeyValues
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, object> GetAllKeyValues()
        {
            Dictionary<string, object> dicts = new Dictionary<string, object>();

            foreach (DictionaryEntry dict in cache)
            {
                dicts.Add(dict.Key.ToString(), dict.Value);
            }
            return dicts;
        }

        /// <summary>
        /// GetAllKeys
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAllKeys()
        {
            List<string> list = new List<string>();

            foreach (DictionaryEntry dict in cache)
            {
                list.Add(dict.Key.ToString());
            }
            return list;
        }

        /// <summary>
        /// Return int of seconds * SecondFactor
        /// </summary>
        public static int SecondFactorCalculate(int seconds)
        {
            // Insert method below takes integer seconds, so we have to round any fractional values
            return Convert.ToInt32(Math.Round((double)seconds * SecondFactor));
        }

    }
}
