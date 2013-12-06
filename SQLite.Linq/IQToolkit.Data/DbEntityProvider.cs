// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using SQLite;

namespace IQToolkit.Data
{
    using Common;
    using Mapping;

    public class DbEntityProvider : EntityProvider
    {
        SQLiteConnection connection;

        int nConnectedActions = 0;
        bool actionOpenedConnection = false;

        public DbEntityProvider(SQLiteConnection connection, QueryLanguage language, QueryMapping mapping, QueryPolicy policy)
            : base(language, mapping, policy)
        {
            if (connection == null)
                throw new InvalidOperationException("Connection not specified");
            this.connection = connection;
        }

        public virtual SQLiteConnection Connection
        {
            get { return this.connection; }
        }

        public virtual DbEntityProvider New(SQLiteConnection connection, QueryMapping mapping, QueryPolicy policy)
        {
            return (DbEntityProvider)Activator.CreateInstance(this.GetType(), new object[] { connection, mapping, policy });
        }

        public virtual DbEntityProvider New(SQLiteConnection connection)
        {
            var n = New(connection, this.Mapping, this.Policy);
            n.Log = this.Log;
            return n;
        }

        public virtual DbEntityProvider New(QueryMapping mapping)
        {
            var n = New(this.Connection, mapping, this.Policy);
            n.Log = this.Log;
            return n;
        }

        public virtual DbEntityProvider New(QueryPolicy policy)
        {
            var n = New(this.Connection, this.Mapping, policy);
            n.Log = this.Log;
            return n;
        }

        protected bool ActionOpenedConnection
        {
            get { return this.actionOpenedConnection; }
        }

        protected void StartUsingConnection()
        {
            this.nConnectedActions++;
        }

        protected void StopUsingConnection()
        {
            System.Diagnostics.Debug.Assert(this.nConnectedActions > 0);
            this.nConnectedActions--;
            if (this.nConnectedActions == 0 && this.actionOpenedConnection)
            {
                this.connection.Close();
                this.actionOpenedConnection = false;
            }
        }

        public override void DoConnected(Action action)
        {
            this.StartUsingConnection();
            try
            {
                action();
            }
            finally
            {
                this.StopUsingConnection();
            }
        }

        public override void DoTransacted(Action action)
        {
            this.StartUsingConnection();
            try
            {
                action();
            }
            finally
            {
                this.StopUsingConnection();
            }
        }

        public override int ExecuteCommand(string commandText)
        {
            if (this.Log != null)
            {
                this.Log.WriteLine(commandText);
            }
            this.StartUsingConnection();
            try
            {
                var cmd = this.Connection.CreateCommand(commandText);
                return cmd.ExecuteNonQuery();
            }
            finally
            {
                this.StopUsingConnection();
            }
        }

        protected override QueryExecutor CreateExecutor()
        {
            return new Executor(this);
        }

        public class Executor : QueryExecutor
        {
            readonly DbEntityProvider provider;
            int rowsAffected;

            public Executor(DbEntityProvider provider)
            {
                this.provider = provider;
            }

            public DbEntityProvider Provider
            {
                get { return this.provider; }
            }

            public override int RowsAffected
            {
                get { return this.rowsAffected; }
            }

            protected virtual bool BufferResultRows
            {
                get { return false; }
            }

            protected bool ActionOpenedConnection
            {
                get { return this.provider.actionOpenedConnection; }
            }

            protected void StartUsingConnection()
            {
                this.provider.StartUsingConnection();
            }

            protected void StopUsingConnection()
            {
                this.provider.StopUsingConnection();
            }

            public override object Convert(object value, Type type)
            {
                if (value == null)
                {
                    return TypeHelper.GetDefault(type);
                }
                type = TypeHelper.GetNonNullableType(type);
                Type vtype = value.GetType();
                if (type != vtype)
                {
                    if (type.GetTypeInfo().IsEnum)
                    {
                        if (vtype == typeof(string))
                        {
                            return Enum.Parse(type, (string)value);
                        }
                        else
                        {
                            Type utype = Enum.GetUnderlyingType(type);
                            if (utype != vtype)
                            {
                                value = System.Convert.ChangeType(value, utype);
                            }
                            return Enum.ToObject(type, value);
                        }
                    }
                    return System.Convert.ChangeType(value, type);
                }
                return value;
            }

            public override IEnumerable<T> Execute<T>(QueryCommand command, Func<FieldReader, T> fnProjector, MappingEntity entity, object[] paramValues)
            {
                this.LogCommand(command, paramValues);
                this.StartUsingConnection();
                try
                {
                    var cmd = this.GetCommand(command, paramValues);
                    var reader = this.ExecuteReader(cmd);
                    var result = Project(reader, fnProjector, entity, true);
                    if (this.provider.ActionOpenedConnection)
                    {
                        result = result.ToList();
                    }
                    else
                    {
                        result = new EnumerateOnce<T>(result);
                    }
                    return result;
                }
                finally
                {
                    this.StopUsingConnection();
                }
            }

            protected virtual SQLiteDataReader ExecuteReader(SQLiteCommand command)
            {
                var reader = command.ExecuteReader();
                if (this.BufferResultRows)
                {
                    // use data table to buffer results
                    //var ds = new DataSet();
                    //ds.EnforceConstraints = false;
                    //var table = new DataTable();
                    //ds.Tables.Add(table);
                    //ds.EnforceConstraints = false;
                    //table.Load(reader);
                    //reader = table.CreateDataReader();
                }
                return reader;
            }

            protected virtual IEnumerable<T> Project<T>(SQLiteDataReader reader, Func<FieldReader, T> fnProjector, MappingEntity entity, bool closeReader)
            {
                var freader = new DbFieldReader(this, reader);
                try
                {
                    while (reader.Read())
                    {
                        yield return fnProjector(freader);
                    }
                }
                finally
                {
                    if (closeReader)
                    {
                        reader.Close();
                    }
                }
            }

            public override int ExecuteCommand(QueryCommand query, object[] paramValues)
            {
                this.LogCommand(query, paramValues);
                this.StartUsingConnection();
                try
                {
                    var cmd = this.GetCommand(query, paramValues);
                    this.rowsAffected = cmd.ExecuteNonQuery();
                    return this.rowsAffected;
                }
                finally
                {
                    this.StopUsingConnection();
                }
            }

            public override IEnumerable<T> ExecuteDeferred<T>(QueryCommand query, Func<FieldReader, T> fnProjector, MappingEntity entity, object[] paramValues)
            {
                this.LogCommand(query, paramValues);
                this.StartUsingConnection();
                try
                {
                    var cmd = this.GetCommand(query, paramValues);
                    var reader = this.ExecuteReader(cmd);
                    var freader = new DbFieldReader(this, reader);
                    try
                    {
                        while (reader.Read())
                        {
                            yield return fnProjector(freader);
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
                finally
                {
                    this.StopUsingConnection();
                }
            }

            /// <summary>
            /// Get an ADO command object initialized with the command-text and parameters
            /// </summary>
            /// <param name="commandText"></param>
            /// <param name="paramNames"></param>
            /// <param name="paramValues"></param>
            /// <returns></returns>
            protected virtual SQLiteCommand GetCommand(QueryCommand query, object[] paramValues)
            {
                // create command object (and fill in parameters)
                var cmd = this.provider.Connection.CreateCommand(query.CommandText);
                this.SetParameterValues(query, cmd, paramValues);
                return cmd;
            }

            protected virtual void SetParameterValues(QueryCommand query, SQLiteCommand command, object[] paramValues)
            {
                for (int i = 0, n = query.Parameters.Count; i < n; i++)
                {
                    this.AddParameter(command, query.Parameters[i], paramValues != null ? paramValues[i] : null);
                }
            }

            protected virtual void AddParameter(SQLiteCommand command, QueryParameter parameter, object value)
            {
                command.Bind("@" + parameter.Name, value);
            }

            protected virtual void LogMessage(string message)
            {
                if (this.provider.Log != null)
                {
                    this.provider.Log.WriteLine(message);
                }
            }

            /// <summary>
            /// Write a command & parameters to the log
            /// </summary>
            /// <param name="command"></param>
            /// <param name="paramValues"></param>
            protected virtual void LogCommand(QueryCommand command, object[] paramValues)
            {
                if (this.provider.Log != null)
                {
                    this.provider.Log.WriteLine(command.CommandText);
                    if (paramValues != null)
                    {
                        this.LogParameters(command, paramValues);
                    }
                    this.provider.Log.WriteLine();
                }
            }

            protected virtual void LogParameters(QueryCommand command, object[] paramValues)
            {
                if (this.provider.Log != null && paramValues != null)
                {
                    for (int i = 0, n = command.Parameters.Count; i < n; i++)
                    {
                        var p = command.Parameters[i];
                        var v = paramValues[i];

                        if (v == null)
                        {
                            this.provider.Log.WriteLine("-- {0} = NULL", p.Name);
                        }
                        else
                        {
                            this.provider.Log.WriteLine("-- {0} = [{1}]", p.Name, v);
                        }
                    }
                }
            }
        }

        protected class DbFieldReader : FieldReader
        {
            QueryExecutor executor;
            SQLiteDataReader reader;

            public DbFieldReader(QueryExecutor executor, SQLiteDataReader reader)
            {
                this.executor = executor;
                this.reader = reader;
                this.Init();
            }

            protected override int FieldCount
            {
                get { return this.reader.FieldCount; }
            }

            protected override Type GetFieldType(int ordinal)
            {
                return this.reader.GetFieldType(ordinal);
            }

            protected override bool IsDBNull(int ordinal)
            {
                return this.reader.IsDBNull(ordinal);
            }

            protected override T GetValue<T>(int ordinal)
            {
                return (T)this.executor.Convert(this.reader.GetValue(ordinal), typeof(T));
            }

            protected override Byte GetByte(int ordinal)
            {
                return this.reader.GetByte(ordinal);
            }

            protected override Char GetChar(int ordinal)
            {
                return this.reader.GetChar(ordinal);
            }

            protected override DateTime GetDateTime(int ordinal)
            {
                return this.reader.GetDateTime(ordinal);
            }

            protected override Decimal GetDecimal(int ordinal)
            {
                return this.reader.GetDecimal(ordinal);
            }

            protected override Double GetDouble(int ordinal)
            {
                return this.reader.GetDouble(ordinal);
            }

            protected override Single GetSingle(int ordinal)
            {
                return this.reader.GetFloat(ordinal);
            }

            protected override Guid GetGuid(int ordinal)
            {
                return this.reader.GetGuid(ordinal);
            }

            protected override Int16 GetInt16(int ordinal)
            {
                return this.reader.GetInt16(ordinal);
            }

            protected override Int32 GetInt32(int ordinal)
            {
                return this.reader.GetInt32(ordinal);
            }

            protected override Int64 GetInt64(int ordinal)
            {
                return this.reader.GetInt64(ordinal);
            }

            protected override String GetString(int ordinal)
            {
                return this.reader.GetString(ordinal);
            }
        }
    }
}
