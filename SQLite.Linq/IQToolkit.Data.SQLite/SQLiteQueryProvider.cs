using IQToolkit.Data.Common;
using SQLite;

namespace IQToolkit.Data.SQLite
{
    public class SQLiteQueryProvider : DbEntityProvider
    {
        public SQLiteQueryProvider(SQLiteConnection connection, QueryMapping mapping, QueryPolicy policy)
            : base(connection, SQLiteLanguage.Default, mapping, policy)
        {
        }

        public static string GetConnectionString(string databaseFile)
        {
            return string.Format("Data Source={0};", databaseFile);
        }

        public static string GetConnectionString(string databaseFile, string password)
        {
            return string.Format("Data Source={0};Password={1};", databaseFile, password);
        }

        public static string GetConnectionString(string databaseFile, bool failIfMissing)
        {
            return string.Format("Data Source={0};FailIfMissing={1};", databaseFile, failIfMissing ? bool.TrueString : bool.FalseString);
        }

        public static string GetConnectionString(string databaseFile, string password, bool failIfMissing)
        {
            return string.Format("Data Source={0};Password={1};FailIfMissing={2};", databaseFile, password, failIfMissing ? bool.TrueString : bool.FalseString);
        }

        public override DbEntityProvider New(SQLiteConnection connection, QueryMapping mapping, QueryPolicy policy)
        {
            return new SQLiteQueryProvider(connection, mapping, policy);
        }

        protected override QueryExecutor CreateExecutor()
        {
            return new Executor(this);
        }

        new class Executor : DbEntityProvider.Executor
        {
            readonly SQLiteQueryProvider provider;

            public Executor(SQLiteQueryProvider provider)
                : base(provider)
            {
                this.provider = provider;
            }

            protected override SQLiteCommand GetCommand(QueryCommand query, object[] paramValues)
            {
                var cmd = provider.Connection.CreateCommand(query.CommandText);
                this.SetParameterValues(query, cmd, paramValues);
                return cmd;
            }
        }
    }
}
