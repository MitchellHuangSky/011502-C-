using POSv01.Infrastructure;
using System;
using System.Windows.Forms;


namespace POSv01
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // 確保資料庫與資料表存在，並套用 OnModelCreating 的設定與種子資料。
            using (var db = new PosDbContext())
            {
                db.Database.EnsureCreated();
            }

            Application.Run(new MainForm());
        }
    }
}