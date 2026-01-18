using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using POSv01.Domain.Entities;
using System.IO;

namespace POSv01.Infrastructure
{
    /// 負責連線 SQLite，並把 Entity 映射成資料表。
    public sealed class PosDbContext : DbContext
    {

        public PosDbContext() { }
        public PosDbContext(DbContextOptions<PosDbContext> options) : base(options) { } // ✅修掉 1 參數建構子錯誤

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Sale> Sales => Set<Sale>();
        public DbSet<SaleItem> SaleItems => Set<SaleItem>();


        public DbSet<Return> Returns => Set<Return>();
        public DbSet<ReturnItem> ReturnItems => Set<ReturnItem>();


        public DbSet<Inventory> Inventories => Set<Inventory>();
        public DbSet<InventoryTxn> InventoryTxns => Set<InventoryTxn>();
       
     


       

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured) return;

            // 先用最簡單，等你要改路徑再接 AppSettings
            // optionsBuilder.UseSqlite("Data Source=pos.db");
            optionsBuilder.UseSqlite(DbPathProvider.GetConnectionString());
        }

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<Product>(e =>
            {
                e.ToTable("Products");
                e.HasKey(x => x.Id);
                e.Property(x => x.Barcode).HasMaxLength(64).IsRequired();
                e.Property(x => x.Name).HasMaxLength(128).IsRequired();
                e.Property(x => x.ImagePath).HasMaxLength(256);
                e.HasIndex(x => x.Barcode).IsUnique();
            });

            b.Entity<Sale>(e =>
            {
                e.ToTable("Sales");
                e.HasKey(x => x.Id);
                e.Property(x => x.SaleNo).HasMaxLength(32).IsRequired();
                e.HasIndex(x => x.SaleNo).IsUnique();
                e.Property(x => x.MemberCode).HasMaxLength(64);
                e.Property(x => x.ClerkName).HasMaxLength(64);
            });

            b.Entity<SaleItem>(e =>
            {
                e.ToTable("SaleItems");
                e.HasKey(x => x.Id);
                e.Property(x => x.Barcode).HasMaxLength(64).IsRequired();
                e.Property(x => x.Name).HasMaxLength(128).IsRequired();

                e.HasOne(x => x.Sale)
                    .WithMany(s => s.Items)
                    .HasForeignKey(x => x.SaleId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Product)
                    .WithMany()
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            b.Entity<Return>(e =>
            {
                e.ToTable("Returns");
                e.HasKey(x => x.Id);
                e.Property(x => x.ReturnNo).HasMaxLength(32).IsRequired();
                e.HasIndex(x => x.ReturnNo).IsUnique();
                e.Property(x => x.MemberCode).HasMaxLength(64);
                e.Property(x => x.ClerkName).HasMaxLength(64);

                e.HasOne(x => x.Sale)
                    .WithMany()
                    .HasForeignKey(x => x.SaleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            b.Entity<ReturnItem>(e =>
            {
                e.ToTable("ReturnItems");
                e.HasKey(x => x.Id);
                e.Property(x => x.Barcode).HasMaxLength(64).IsRequired();
                e.Property(x => x.Name).HasMaxLength(128).IsRequired();

                e.HasOne(x => x.Return)
                    .WithMany(r => r.Items)
                    .HasForeignKey(x => x.ReturnId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Product)
                    .WithMany()
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }



        /*
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
            */
    }
}