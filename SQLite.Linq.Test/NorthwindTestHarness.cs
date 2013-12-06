// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using IQToolkit.Data.SQLite;

namespace SQLite.Linq.Test
{
    public class NorthwindTestHarness : TestHarness
    {
        protected Northwind db;

        protected void RunTests(Northwind db, string baselineFile, string newBaselineFile, bool executeQueries)
        {
            this.db = db;
            var provider = (SQLiteQueryProvider)db.Provider;
            base.RunTests(provider, baselineFile, newBaselineFile, executeQueries);
        }

        protected void RunTest(Northwind db, string baselineFile, bool executeQueries, string testName)
        {
            this.db = db;
            var provider = (SQLiteQueryProvider)db.Provider;
            base.RunTest(provider, baselineFile, executeQueries, testName);
        }
    }
}