

// File: Infrastructure/PosDbContextFactory.cs
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace POSv01.Infrastructure
{
    public class PosDbContextFactory : IDesignTimeDbContextFactory<PosDbContext>
    {
        public PosDbContext CreateDbContext(string[] args)
        {
            //var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "pos.db");
            var dbPath = DbPathProvider.GetDbPath();


            var options = new DbContextOptionsBuilder<PosDbContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;

            return new PosDbContext(options);
        }

    }
}
