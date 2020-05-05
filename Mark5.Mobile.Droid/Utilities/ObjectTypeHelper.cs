using Java.Lang;

namespace Mark5.Mobile.Droid.Utilities
{
    public static class ObjectTypeHelper
    {
        /// <summary>
        ///     Cast Java Object type to .Net Type T
        /// </summary>
        /// <param name="obj">Java object</param>
        /// <typeparam name="T">.Net Type</typeparam>
        /// <returns>obj casted to T type</returns>
        public static T Cast<T>(this Object obj) where T : class
        {
            var propertyInfo = obj.GetType().GetProperty("Instance");
            return propertyInfo == null ? null : propertyInfo.GetValue(obj, null) as T;
        }
    }
}