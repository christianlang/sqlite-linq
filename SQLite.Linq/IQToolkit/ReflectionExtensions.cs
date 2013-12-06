// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System;
using System.Reflection;

namespace IQToolkit
{
    public static class ReflectionExtensions
    {
        public static object GetValue(this MemberInfo member, object instance)
        {
            if (member is PropertyInfo)
            {
                return ((PropertyInfo)member).GetValue(instance, null);
            }
            if (member is FieldInfo)
            {
                return ((FieldInfo)member).GetValue(instance);
            }
            throw new InvalidOperationException();
        }

        public static void SetValue(this MemberInfo member, object instance, object value)
        {
            if (member is PropertyInfo)
            {
                var pi = (PropertyInfo)member;
                pi.SetValue(instance, value, null);
            }
            else if (member is FieldInfo)
            {
                var fi = (FieldInfo) member;
                fi.SetValue(instance, value);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}
