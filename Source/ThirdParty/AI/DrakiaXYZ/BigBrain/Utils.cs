using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;

namespace DrakiaXYZ.BigBrain
{
    internal class Utils
    {
        public static FieldInfo GetFieldByType(Type classType, Type fieldType)
        {
            return AccessTools.GetDeclaredFields(classType).FirstOrDefault(
                x => fieldType.IsAssignableFrom(x.FieldType) || (x.FieldType.IsGenericType && fieldType.IsGenericType && fieldType.GetGenericTypeDefinition().IsAssignableFrom(x.FieldType.GetGenericTypeDefinition())));
        }

        public static string GetPropertyNameByType(Type classType, Type propertyType)
        {
            return AccessTools.GetDeclaredProperties(classType).FirstOrDefault(
                x => propertyType.IsAssignableFrom(x.PropertyType) || (x.PropertyType.IsGenericType && propertyType.IsGenericType && propertyType.GetGenericTypeDefinition().IsAssignableFrom(x.PropertyType.GetGenericTypeDefinition())))?.Name;
        }
    }
}
