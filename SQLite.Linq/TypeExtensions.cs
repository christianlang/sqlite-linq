using System;

namespace SQLite.Linq
{
    public static class TypeExtensions
    {
        public static TypeCode GetTypeCode(this Type type)
        {
            return Type.GetTypeCode(type);
        }
    }
}
