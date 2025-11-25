using System;
using System.Web;
using System.Web.Caching;

namespace ValiModern.Helpers
{
    /// <summary>
    /// Centralized cache helper to optimize database queries
    /// </summary>
    public static class CacheHelper
    {
        // Cache keys
        public const string KEY_CATEGORIES = "Cache_Categories";
        public const string KEY_BRANDS = "Cache_Brands";
        public const string KEY_LAYOUT_CATEGORIES = "Cache_Layout_Categories";

        /// <summary>
        /// Get item from cache or execute factory function and cache the result (for reference types)
        /// </summary>
        public static T GetOrSet<T>(string key, Func<T> factory, int durationMinutes = 10) where T : class
        {
            var cache = HttpRuntime.Cache;
            var cached = cache[key] as T;

            if (cached != null)
            {
                return cached;
            }

            // Execute factory to get data
            var data = factory();

            if (data != null)
            {
                cache.Insert(
                    key,
                    data,
                    null,
                    DateTime.Now.AddMinutes(durationMinutes),
                    Cache.NoSlidingExpiration,
                    CacheItemPriority.Normal,
                    null
                );
            }

            return data;
        }

        /// <summary>
        /// Get value type from cache or execute factory function and cache the result (for value types like int, bool, etc.)
        /// </summary>
        public static T GetOrSetValue<T>(string key, Func<T> factory, int durationMinutes = 10) where T : struct
        {
            var cache = HttpRuntime.Cache;
            var cached = cache[key];

            if (cached != null && cached is T)
            {
                return (T)cached;
            }

            // Execute factory to get data
            var data = factory();

            // Cache the value (boxed)
            cache.Insert(
                key,
                data,
                null,
                DateTime.Now.AddMinutes(durationMinutes),
                Cache.NoSlidingExpiration,
                CacheItemPriority.Normal,
                null
            );

            return data;
        }

        /// <summary>
        /// Remove item from cache
        /// </summary>
        public static void Remove(string key)
        {
            HttpRuntime.Cache.Remove(key);
        }

        /// <summary>
        /// Remove all category-related cache when categories are modified
        /// </summary>
        public static void InvalidateCategoryCache()
        {
            Remove(KEY_CATEGORIES);
            Remove(KEY_LAYOUT_CATEGORIES);
        }

        /// <summary>
        /// Remove brand cache when brands are modified
        /// </summary>
        public static void InvalidateBrandCache()
        {
            Remove(KEY_BRANDS);
        }

        /// <summary>
        /// Clear all application cache
        /// </summary>
        public static void ClearAll()
        {
            var cache = HttpRuntime.Cache;
            var enumerator = cache.GetEnumerator();
            
            while (enumerator.MoveNext())
            {
                cache.Remove(enumerator.Key.ToString());
            }
        }
    }
}
