using System;
using System.Linq;
using System.Reflection;

namespace SQLite.Linq
{
    public static class TypeExtensions
    {
        public static bool IsAssignableFrom(this Type type, Type from)
        {
            return type.GetTypeInfo().IsAssignableFrom(from.GetTypeInfo());
        }

        public static ConstructorInfo GetConstructor(this Type type, Type[] types)
        {
            return type.GetTypeInfo().DeclaredConstructors.First(
                c => c.GetParameters().Select(param => param.ParameterType).SequenceEqual(types));
        }

        public static FieldInfo[] GetFields(this Type type)
        {
            var fields = type.GetTypeInfo().DeclaredFields;

            var baseType = type.GetTypeInfo().BaseType;
            if (baseType != null)
            {
                fields = fields.Union(GetFields(baseType));
            }

            return fields.ToArray();
        }

        public static PropertyInfo[] GetProperties(this Type type)
        {
            var properties = type.GetTypeInfo().DeclaredProperties;

            var baseType = type.GetTypeInfo().BaseType;
            if (baseType != null)
            {
                properties = properties.Union(GetProperties(baseType));
            }

            return properties.ToArray();
        }

        public static MethodInfo[] GetMethods(this Type type)
        {
            var methods = type.GetTypeInfo().DeclaredMethods;

            var baseType = type.GetTypeInfo().BaseType;
            if (baseType != null)
            {
                methods = methods.Union(GetMethods(baseType));
            }

            return methods.ToArray();
        }

        public static TypeCode GetTypeCode(this Type type)
        {
            if (type == null)
                return TypeCode.Empty;

            if (typeof(bool).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                return TypeCode.Boolean;

            if (typeof(char).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                return TypeCode.Char;

            if (typeof(sbyte).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                return TypeCode.SByte;

            if (typeof(byte).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                return TypeCode.Byte;

            if (typeof(short).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                return TypeCode.Int16;

            if (typeof(ushort).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                return TypeCode.UInt16;

            if (typeof(int).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                return TypeCode.Int32;

            if (typeof(uint).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                return TypeCode.UInt32;

            if (typeof(long).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                return TypeCode.Int64;

            if (typeof(ulong).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                return TypeCode.UInt64;

            if (typeof(float).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                return TypeCode.Single;

            if (typeof(double).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                return TypeCode.Double;

            if (typeof(decimal).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                return TypeCode.Decimal;

            if (typeof(DateTime).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                return TypeCode.DateTime;

            if (typeof(string).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                return TypeCode.String;

            return TypeCode.Object;
        }
    }
}
