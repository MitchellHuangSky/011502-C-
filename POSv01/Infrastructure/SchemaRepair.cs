// File: Infrastructure/SchemaRepair.cs
using System;
using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace POSv01.Infrastructure
{
    public static class SchemaRepair
    {
        /// <summary>
        /// 修復舊 DB：若 Products 缺少 ImagePath 欄位就補上。
        /// </summary>
        public static void EnsureProductsHasImagePath(DbContext db)
        {
            using var conn = db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            if (!TableExists(conn, "Products"))
            {
                return;
            }

            if (ColumnExists(conn, "Products", "ImagePath"))
            {
                return;
            }

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"ALTER TABLE ""Products"" ADD COLUMN ""ImagePath"" TEXT;";
            cmd.ExecuteNonQuery();
        }

        public static IReadOnlyList<string> GetProductColumns(DbContext db)
        {
            using var conn = db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            var cols = new List<string>();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"PRAGMA table_info(""Products"");";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                cols.Add(reader.GetString(1)); // name
            }

            return cols;
        }

        private static bool TableExists(IDbConnection conn, string tableName)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT 1 FROM sqlite_master WHERE type='table' AND name=$name LIMIT 1;";
            var p = cmd.CreateParameter();
            p.ParameterName = "$name";
            p.Value = tableName;
            cmd.Parameters.Add(p);
            return cmd.ExecuteScalar() != null;
        }

        private static bool ColumnExists(IDbConnection conn, string tableName, string columnName)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"PRAGMA table_info(""{tableName}"");";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var name = reader.GetString(1);
                if (string.Equals(name, columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
