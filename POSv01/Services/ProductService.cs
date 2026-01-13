using Microsoft.EntityFrameworkCore;
using POSv01.Domain.Entities;
using POSv01.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.Linq;




namespace POSv01.Services
{
    /// <summary>
    /// 商品資料存取 + 新增商品（含照片複製到 images/ 並存相對路徑）。
    /// </summary>
    public class ProductService
    {
        private readonly PosDbContext _db;

        public ProductService(PosDbContext db) => _db = db;

        public IReadOnlyList<Product> GetActiveProducts(string? keyword = null)
        {
            var q = _db.Products.AsNoTracking().Where(p => p.IsActive);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                q = q.Where(p => p.Barcode.Contains(keyword) || p.Name.Contains(keyword));
            }

            return q.OrderBy(p => p.Name).ToList();
        }

        public void AddProduct(string barcode, string name, decimal unitPrice, string? sourceImagePath)
        {
            barcode = (barcode ?? "").Trim();
            name = (name ?? "").Trim();

            if (string.IsNullOrWhiteSpace(barcode)) throw new ArgumentException("條碼不可空白");
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("名稱不可空白");
            if (unitPrice < 0) throw new ArgumentException("單價不可小於 0");
            if (_db.Products.Any(p => p.Barcode == barcode)) throw new InvalidOperationException("條碼已存在");

            var relImagePath = CopyImageToImagesFolder(sourceImagePath);

            _db.Products.Add(new Product
            {
                Barcode = barcode,
                Name = name,
                UnitPrice = unitPrice,
                IsActive = true,
                ImagePath = relImagePath,
                CreatedAt = DateTime.UtcNow
            });

            _db.SaveChanges();
        }

        private static string? CopyImageToImagesFolder(string? sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath)) return null;

            var imagesDir = DbPathProvider.GetImagesDir();
            var ext = Path.GetExtension(sourcePath);
            var fileName = $"{Guid.NewGuid():N}{ext}";
            var destFullPath = Path.Combine(imagesDir, fileName);

            File.Copy(sourcePath, destFullPath, overwrite: true);

            // 存相對於 DB 根資料夾的相對路徑
            return Path.Combine("images", fileName);
        }
    }
}