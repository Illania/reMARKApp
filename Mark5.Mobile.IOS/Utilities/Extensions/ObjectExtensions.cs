using System;
using System.Collections.Generic;
using System.Reflection;

namespace Mark5.Mobile.IOS.Utilities
{
    public static class ObjectExtensions
    {
        static readonly MethodInfo CloneMethod = typeof(Object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);

        public static Object UniversalDeepCopy(this Object originalObject)
        {
            return InternalCopy(originalObject, new Dictionary<Object, Object>(new ReferenceEqualityComparer()));
        }

        public static T UniversalDeepCopy<T>(this T original)
        {
            return (T) UniversalDeepCopy((Object) original);
        }

        #region Private methods

        static Object InternalCopy(Object originalObject, IDictionary<Object, Object> visited)
        {
            if (originalObject == null)
                return null;
            var typeToReflect = originalObject.GetType();
            if (IsPrimitive(typeToReflect))
                return originalObject;
            if (visited.ContainsKey(originalObject))
                return visited[originalObject];
            if (typeof(Delegate).IsAssignableFrom(typeToReflect))
                return null;
            var cloneObject = CloneMethod.Invoke(originalObject, null);
            if (typeToReflect.IsArray)
            {
                var arrayType = typeToReflect.GetElementType();
                if (IsPrimitive(arrayType))
                {
                    var clonedArray = (Array) cloneObject;
                    for (var i = 0; i < clonedArray.Length; i++)
                        clonedArray.SetValue(InternalCopy(clonedArray.GetValue(i), visited), i);
                }
            }
            visited.Add(originalObject, cloneObject);
            CopyFields(originalObject, visited, cloneObject, typeToReflect);
            RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect);
            return cloneObject;
        }

        static void CopyFields(object originalObject, IDictionary<object, object> visited, object cloneObject, IReflect typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy, Func<FieldInfo, bool> filter = null)
        {
            foreach (var fieldInfo in typeToReflect.GetFields(bindingFlags))
            {
                if (filter != null && !filter(fieldInfo))
                    continue;
                if (IsPrimitive(fieldInfo.FieldType))
                    continue;
                var originalFieldValue = fieldInfo.GetValue(originalObject);
                var clonedFieldValue = InternalCopy(originalFieldValue, visited);
                fieldInfo.SetValue(cloneObject, clonedFieldValue);
            }
        }

        static void RecursiveCopyBaseTypePrivateFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect)
        {
            if (typeToReflect.BaseType != null)
            {
                RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect.BaseType);
                CopyFields(originalObject, visited, cloneObject, typeToReflect.BaseType, BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate);
            }
        }

        static bool IsPrimitive(this Type type)
        {
            return type == typeof(String) || type.IsValueType & type.IsPrimitive;
        }

        #endregion
    }

    class ReferenceEqualityComparer : EqualityComparer<Object>
    {
        public override bool Equals(object x, object y)
        {
            return ReferenceEquals(x, y);
        }

        public override int GetHashCode(object obj)
        {
            return obj == null ? 0 : obj.GetHashCode();
        }
    }
}