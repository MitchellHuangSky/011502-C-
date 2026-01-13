using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace POSv01.Infrastructure
{
    public static class MigrationRepair
    {
        /// <summary>
        /// 修復：舊資料庫用 EnsureCreated 建立，沒有 __EFMigrationsHistory。
        /// 若 Products 存在但 __EFMigrationsHistory 不存在，建立 history 並標記 Init 已套用。
        /// </summary>
        public static void RepairLegacyDbIfNeeded(DbContext db, string initMigrationId)
        {
            using var conn = db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            var hasProducts = TableExists(conn, "Products");
            var hasHistory = TableExists(conn, "__EFMigrationsHistory");

            if (!hasProducts || hasHistory)
            {
                return;
            }

            CreateHistoryTable(conn);
            InsertMigrationIfMissing(conn, initMigrationId, productVersion: "8.0.0");
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

        private static void CreateHistoryTable(IDbConnection conn)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText =
                """
                CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
                    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
                    "ProductVersion" TEXT NOT NULL
                );
                """;
            cmd.ExecuteNonQuery();
        }

        private static void InsertMigrationIfMissing(IDbConnection conn, string migrationId, string productVersion)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText =
                """
                INSERT OR IGNORE INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
                VALUES ($mid, $ver);
                """;

            var p1 = cmd.CreateParameter();
            p1.ParameterName = "$mid";
            p1.Value = migrationId;

            var p2 = cmd.CreateParameter();
            p2.ParameterName = "$ver";
            p2.Value = productVersion;

            cmd.Parameters.Add(p1);
            cmd.Parameters.Add(p2);

            cmd.ExecuteNonQuery();
        }
    }
}
