using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Nulah.VSIX.TaskTool.StandardLib.Models;

namespace Nulah.VSIX.TaskTool.StandardLib
{
    public static class NulahStandardLib
    {
        public static DateTime DateTimeNow()
        {
            return DateTime.UtcNow;
        }

        public static List<ReflectedTypeInfo> GetPropertiesForType<T>()
        {
            var publicProps = new List<ReflectedTypeInfo>();

            var t = typeof(T);

            var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            var props = t.GetProperties(bindingFlags);

            foreach (var prop in props)
            {
                var valueInfo = new ReflectedTypeInfo()
                {
                    IsNullableType = prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>),
                    IsPrivate = prop.PropertyType.IsPublic,
                    Name = prop.Name,
                };

                // Override strings to be a nullable type
                if (prop.PropertyType == typeof(string))
                {
                    valueInfo.IsNullableType = true;
                }

                // Override strings to be a nullable type
                if (prop.PropertyType == typeof(string))
                {
                    valueInfo.IsNullableType = true;
                    valueInfo.ValueType = typeof(string);
                }
                else if (valueInfo.IsNullableType)
                {
                    // If the value type is nullable, get its underlying type
                    valueInfo.ValueType = prop.PropertyType.GetGenericArguments()[0];
                }
                else
                {
                    valueInfo.ValueType = prop.PropertyType;
                }

                publicProps.Add(valueInfo);
            }

            return publicProps;
        }

        public static List<ReflectedTypeInfo> GetPropertiesForObject(object valueObject)
        {
            var publicProps = new List<ReflectedTypeInfo>();

            var t = valueObject.GetType();

            var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            var props = t.GetProperties(bindingFlags);

            foreach (var prop in props)
            {
                var valueInfo = new ReflectedTypeInfo()
                {
                    IsNullableType = prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>),
                    IsPrivate = prop.PropertyType.IsPublic,
                    Name = prop.Name,
                };

                // Override strings to be a nullable type
                if (prop.PropertyType == typeof(string))
                {
                    valueInfo.IsNullableType = true;
                    valueInfo.ValueType = typeof(string);
                }
                else if (valueInfo.IsNullableType)
                {
                    // If the value type is nullable, get its underlying type
                    valueInfo.ValueType = prop.PropertyType.GetGenericArguments()[0];
                }
                else
                {
                    valueInfo.ValueType = prop.PropertyType;
                }

                publicProps.Add(valueInfo);
            }

            return publicProps;
        }

        public static Dictionary<string, ReflectedValueInfo> GetPropertiesAndValuesForObject(object valueObject)
        {
            var publicProps = new Dictionary<string, ReflectedValueInfo>();

            var t = valueObject.GetType();

            var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            var props = t.GetProperties(bindingFlags);

            foreach (var prop in props)
            {
                var valueInfo = new ReflectedValueInfo()
                {
                    IsNullableType = prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>),
                    IsPrivate = prop.PropertyType.IsPublic,
                    Name = prop.Name,
                    Value = prop.GetValue(valueObject)
                };

                // Override strings to be a nullable type
                if (prop.PropertyType == typeof(string))
                {
                    valueInfo.IsNullableType = true;
                    valueInfo.ValueType = typeof(string);
                }
                else if (valueInfo.IsNullableType)
                {
                    // If the value type is nullable, get its underlying type
                    valueInfo.ValueType = prop.PropertyType.GetGenericArguments()[0];
                }
                else
                {
                    valueInfo.ValueType = prop.PropertyType;
                }

                if (valueInfo.Value == null)
                {
                    valueInfo.IsNull = true;
                }

                publicProps.Add(valueInfo.Name, valueInfo);
            }

            return publicProps;
        }
    }
}
