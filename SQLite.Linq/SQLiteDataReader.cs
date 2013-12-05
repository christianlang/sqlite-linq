using System;
using Sqlite3Statement = System.IntPtr;

namespace SQLite
{
    public class SQLiteDataReader
    {
        private readonly SQLiteConnection _conn;
        private readonly IntPtr _stmt;

        public SQLiteDataReader(SQLiteConnection conn, Sqlite3Statement stmt)
        {
            _conn = conn;
            _stmt = stmt;
        }

        public int FieldCount
        {
            get { return SQLite3.ColumnCount(_stmt); }
        }

        public Type GetFieldType(int ordinal)
        {
            switch (SQLite3.ColumnType(_stmt, ordinal))
            {
                case SQLite3.ColType.Integer:
                    return typeof (int);
                case SQLite3.ColType.Float:
                    return typeof (double);
                case SQLite3.ColType.Text:
                    return typeof (string);
                case SQLite3.ColType.Blob:
                    return typeof (byte[]);
                case SQLite3.ColType.Null:
                    return typeof (object);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool IsDBNull(int ordinal)
        {
            return SQLite3.ColumnType(_stmt, ordinal) == SQLite3.ColType.Null;
        }

        public object GetValue(int ordinal)
        {
            switch (SQLite3.ColumnType(_stmt, ordinal))
            {
                case SQLite3.ColType.Integer:
                    return SQLite3.ColumnInt(_stmt, ordinal);
                case SQLite3.ColType.Float:
                    return SQLite3.ColumnDouble(_stmt, ordinal);
                case SQLite3.ColType.Text:
                    return SQLite3.ColumnString(_stmt, ordinal);
                case SQLite3.ColType.Blob:
                    return SQLite3.ColumnByteArray(_stmt, ordinal);
                case SQLite3.ColType.Null:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public byte GetByte(int ordinal)
        {
            return (byte)SQLite3.ColumnInt(_stmt, ordinal);
        }

        public char GetChar(int ordinal)
        {
            return (char)SQLite3.ColumnInt(_stmt, ordinal);
        }

        public DateTime GetDateTime(int ordinal)
        {
            if (_conn.StoreDateTimeAsTicks)
            {
                return new DateTime(SQLite3.ColumnInt64(_stmt, ordinal));
            }
            else
            {
                var text = SQLite3.ColumnString(_stmt, ordinal);
                return DateTime.Parse(text);
            }
        }

        public decimal GetDecimal(int ordinal)
        {
            return (decimal)SQLite3.ColumnDouble(_stmt, ordinal);
        }

        public double GetDouble(int ordinal)
        {
            return SQLite3.ColumnDouble(_stmt, ordinal);
        }

        public float GetFloat(int ordinal)
        {
            return (float)SQLite3.ColumnDouble(_stmt, ordinal);
        }

        public Guid GetGuid(int ordinal)
        {
            var text = SQLite3.ColumnString(_stmt, ordinal);
            return new Guid(text);
        }

        public short GetInt16(int ordinal)
        {
            return (short)SQLite3.ColumnInt(_stmt, ordinal);
        }

        public int GetInt32(int ordinal)
        {
            return SQLite3.ColumnInt(_stmt, ordinal);
        }

        public long GetInt64(int ordinal)
        {
            return SQLite3.ColumnInt64(_stmt, ordinal);
        }

        public string GetString(int ordinal)
        {
            return SQLite3.ColumnString(_stmt, ordinal);
        }

        public bool Read()
        {
            return SQLite3.Step(_stmt) == SQLite3.Result.Row;
        }

        public void Close()
        {
            SQLite3.Finalize(_stmt);
        }
    }
}
