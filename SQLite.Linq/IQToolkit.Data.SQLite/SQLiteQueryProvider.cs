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
