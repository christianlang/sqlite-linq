// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System;
using System.Collections.Generic;

namespace IQToolkit.Data.Common
{
    public interface ICreateExecutor
    {
        QueryExecutor CreateExecutor();
    }

    public abstract class QueryExecutor
    {
        // called from compiled execution plan
        public abstract int RowsAffected { get; }
        public abstract object Convert(object value, Type type);
        public abstract IEnumerable<T> Execute<T>(QueryCommand command, Func<FieldReader, T> fnProjector, MappingEntity entity, object[] paramValues);
        public abstract IEnumerable<T> ExecuteDeferred<T>(QueryCommand query, Func<FieldReader, T> fnProjector, MappingEntity entity, object[] paramValues);
        public abstract int ExecuteCommand(QueryCommand query, object[] paramValues);
    }
}