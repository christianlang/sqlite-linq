// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System;
using IQToolkit.Data.Common;
using IQToolkit.Data.Mapping;
using IQToolkit.Data.SQLite;

namespace SQLite.Linq.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var provider = new SQLiteQueryProvider(
                new SQLiteConnection("Northwind.db3"),
                new AttributeMapping(typeof (NorthwindWithAttributes)),
                QueryPolicy.Default);

            //provider.Log = Console.Out;

            var db = new Northwind(provider);

            NorthwindTranslationTests.Run(db, true);
            NorthwindExecutionTests.Run(db);
            NorthwindCUDTests.Run(db);
            MultiTableTests.Run(new MultiTableContext(provider.New(new AttributeMapping(typeof(MultiTableContext)))));
            NorthwindPerfTests.Run(db, "TestStandardQuery");

            Console.ReadLine();
        }
    }
}
