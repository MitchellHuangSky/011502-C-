using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using POSv01.Domain.Entities;
using System.IO;

namespace POSv01.Infrastructure
{
    /// 負責連線 SQLite，並把 Entity 映射成資料表。
    public class PosDbContext : DbContext
    {
        
        public DbSet<Product> Products => Set<Product>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "pos.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Barcode)
                .IsUnique();

            var seedTime = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);



            // 塞一筆測試商品，方便你掃條碼測試。
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Barcode = "471000000001",
                    Name = "測試商品A",
                    UnitPrice = 35m,
                    IsActive = true,
                    CreatedAt = seedTime
                },
                new Product
                {
                    Id = 2,
                    Barcode = "471000000002",
                    Name = "測試商品B",
                    UnitPrice = 25m,
                    IsActive = true,
                    CreatedAt = seedTime
                },
                new Product
                {
                    Id = 3,
                    Barcode = "471000000003",
                    Name = "測試商品C",
                    UnitPrice = 59m,
                    IsActive = true,
                    CreatedAt = seedTime
                },
                new Product
                {
                    Id = 4,
                    Barcode = "471000000004",
                    Name = "瓶裝水",
                    UnitPrice = 20m,
                    IsActive = true,
                    CreatedAt = seedTime
                },
                new Product
                {
                    Id = 5,
                    Barcode = "471000000005",
                    Name = "麵包",
                    UnitPrice = 45m,
                    IsActive = true,
                    CreatedAt = seedTime
                }
            );
        }
    }
}