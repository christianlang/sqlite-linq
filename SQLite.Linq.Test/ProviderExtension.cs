using IQToolkit.Data.Common;
using IQToolkit.Data.SQLite;

namespace SQLite.Linq.Test
{
    public static class ProviderExtension
    {
        public static SQLiteQueryProvider New(this SQLiteQueryProvider provider, SQLiteConnection connection, QueryMapping mapping, QueryPolicy policy)
        {
            return new SQLiteQueryProvider(connection, mapping, policy);
        }

        public static SQLiteQueryProvider New(this SQLiteQueryProvider provider, SQLiteConnection connection)
        {
            var n = provider.New(connection, provider.Mapping, provider.Policy);
            n.Log = provider.Log;
            return n;
        }

        public static SQLiteQueryProvider New(this SQLiteQueryProvider provider, QueryMapping mapping)
        {
            var n = provider.New(provider.Connection, mapping, provider.Policy);
            n.Log = provider.Log;
            return n;
        }

        public static SQLiteQueryProvider New(this SQLiteQueryProvider provider, QueryPolicy policy)
        {
            var n = provider.New(provider.Connection, provider.Mapping, policy);
            n.Log = provider.Log;
            return n;
        }
    }
}
