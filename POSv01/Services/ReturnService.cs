using POSv01.Domain.Entities;
using POSv01.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace POSv01.Services
{
    public sealed class ReturnService
    {
        private readonly PosDbContext _db;
        private readonly InventoryService _inventory;

        public ReturnService(PosDbContext db, InventoryService inventory)
        {
            _db = db;
            _inventory = inventory;
        }

        public Sale? FindSaleBySaleNo(string saleNo)
        {
            saleNo = (saleNo ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(saleNo)) return null;

            return _db.Sales
                .Include(s => s.Items)
                .AsNoTracking()
                .FirstOrDefault(s => s.SaleNo == saleNo);
        }

        public Return CreateReturn(string saleNo, IReadOnlyList<(int productId, decimal qty)> returnLines,
            string? clerkName = null, string? memberCode = null)
        {
            var sale = _db.Sales.Include(s => s.Items).FirstOrDefault(s => s.SaleNo == saleNo);
            if (sale == null) throw new InvalidOperationException("找不到原交易。");

            if (returnLines.Count == 0) throw new InvalidOperationException("沒有退貨品項。");

            // 驗證：退貨數量不得超過原購買數
            var saleQty = sale.Items.ToDictionary(x => x.ProductId, x => x.Quantity);
            foreach (var (pid, qty) in returnLines)
            {
                if (qty <= 0) throw new InvalidOperationException("退貨數量需大於 0。");
                if (!saleQty.TryGetValue(pid, out var bought)) throw new InvalidOperationException("退貨商品不在原交易內。");
                if (qty > bought) throw new InvalidOperationException("退貨數量不可超過原購買數量。");
            }

            var ret = new Return
            {
                ReturnNo = GenerateReturnNo(),
                SaleId = sale.Id,
                CreatedAt = DateTime.UtcNow,
                ClerkName = clerkName,
                MemberCode = memberCode
            };

            decimal totalRefund = 0;
            foreach (var (pid, qty) in returnLines)
            {
                var si = sale.Items.First(x => x.ProductId == pid);
                var line = si.UnitPrice * qty;
                totalRefund += line;

                ret.Items.Add(new ReturnItem
                {
                    ProductId = pid,
                    Barcode = si.Barcode,
                    Name = si.Name,
                    UnitPrice = si.UnitPrice,
                    Quantity = qty,
                    LineTotal = line
                });
            }

            ret.TotalRefund = totalRefund;

            _db.Returns.Add(ret);

            _inventory.ApplyReturn(ret.ReturnNo, returnLines);

            _db.SaveChanges();
            return ret;
        }

        private static string GenerateReturnNo()
        {
            var ts = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            var rnd = Random.Shared.Next(1000, 9999);
            return $"R{ts}-{rnd}";
        }
    }
}
