using System;
using System.Text;
using IQToolkit.Data.Common;
using SQLite;
using SQLite.Linq;

namespace IQToolkit.Data.SQLite
{
    public class SQLiteTypeSystem : DbTypeSystem
    {
        public override SqlDbType GetSqlType(string typeName)
        {
            if (string.Compare(typeName, "TEXT", StringComparison.OrdinalIgnoreCase) == 0 ||
                string.Compare(typeName, "CHAR", StringComparison.OrdinalIgnoreCase) == 0 ||
                string.Compare(typeName, "CLOB", StringComparison.OrdinalIgnoreCase) == 0 ||
                string.Compare(typeName, "VARYINGCHARACTER", StringComparison.OrdinalIgnoreCase) == 0 ||
                string.Compare(typeName, "NATIONALVARYINGCHARACTER", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return SqlDbType.VarChar;
            }
            else if (string.Compare(typeName, "INT", StringComparison.OrdinalIgnoreCase) == 0 ||
                string.Compare(typeName, "INTEGER", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return SqlDbType.BigInt;
            }
            else if (string.Compare(typeName, "BLOB", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return SqlDbType.Binary;
            }
            else if (string.Compare(typeName, "BOOLEAN", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return SqlDbType.Bit;
            }
            else if (string.Compare(typeName, "NUMERIC", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return SqlDbType.Decimal;
            }
            else
            {
                return base.GetSqlType(typeName);
            }
        }

        public override string GetVariableDeclaration(QueryType type, bool suppressSize)
        {
            StringBuilder sb = new StringBuilder();
            DbQueryType sqlType = (DbQueryType)type;
            SqlDbType sqlDbType = sqlType.SqlDbType;

            switch (sqlDbType)
            {
                case SqlDbType.BigInt:
                case SqlDbType.SmallInt:
                case SqlDbType.Int:
                case SqlDbType.TinyInt:
                    sb.Append("INTEGER");
                    break;
                case SqlDbType.Bit:
                    sb.Append("BOOLEAN");
                    break;
                case SqlDbType.SmallDateTime:
                    sb.Append("DATETIME");
                    break;
                case SqlDbType.Char:
                case SqlDbType.NChar:
                    sb.Append("CHAR");
                    if (type.Length > 0 && !suppressSize)
                    {
                        sb.Append("(");
                        sb.Append(type.Length);
                        sb.Append(")");
                    }
                    break;
                case SqlDbType.Variant:
                case SqlDbType.Binary:
                case SqlDbType.Image:
                case SqlDbType.UniqueIdentifier: //There is a setting to make it string, look at later
                    sb.Append("BLOB");
                    if (type.Length > 0 && !suppressSize)
                    {
                        sb.Append("(");
                        sb.Append(type.Length);
                        sb.Append(")");
                    }
                    break;
                case SqlDbType.Xml:
                case SqlDbType.NText:
                case SqlDbType.NVarChar:
                case SqlDbType.Text:
                case SqlDbType.VarBinary:
                case SqlDbType.VarChar:
                    sb.Append("TEXT");
                    if (type.Length > 0 && !suppressSize)
                    {
                        sb.Append("(");
                        sb.Append(type.Length);
                        sb.Append(")");
                    }
                    break;
                case SqlDbType.Decimal:
                case SqlDbType.Money:
                case SqlDbType.SmallMoney:
                    sb.Append("NUMERIC");
                    if (type.Precision != 0)
                    {
                        sb.Append("(");
                        sb.Append(type.Precision);
                        sb.Append(")");
                    }
                    break;
                case SqlDbType.Float:
                case SqlDbType.Real:
                    sb.Append("FLOAT");
                    if (type.Precision != 0)
                    {
                        sb.Append("(");
                        sb.Append(type.Precision);
                        sb.Append(")");
                    }
                    break;
                case SqlDbType.Date:
                case SqlDbType.DateTime:
                case SqlDbType.Timestamp:
                default:
                    sb.Append(sqlDbType);
                    break;
            }
            return sb.ToString();
        }
    }
}
