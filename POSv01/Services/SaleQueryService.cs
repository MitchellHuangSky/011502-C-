using POSv01.Domain.Entities;
using POSv01.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

using System.Linq;
using Microsoft.EntityFrameworkCore;



namespace POSv01.Services
{
    public sealed partial class SaleQueryService
    {


        private static (DateTime fromUtc, DateTime toUtcExclusive) ToUtcRange(DateTime fromLocalDate, DateTime toLocalDate)
        {
            var startLocal = DateTime.SpecifyKind(fromLocalDate.Date, DateTimeKind.Local);
            var endLocalExclusive = DateTime.SpecifyKind(toLocalDate.Date.AddDays(1), DateTimeKind.Local);
            return (startLocal.ToUniversalTime(), endLocalExclusive.ToUniversalTime());
        }

        public IReadOnlyList<Return> SearchReturns(DateTime fromLocalDate, DateTime toLocalDate, string? keyword)
        {
            var (fromUtc, toUtc) = ToUtcRange(fromLocalDate, toLocalDate);

            var q = _db.Returns
                .AsNoTracking()
                .Include(r => r.Sale)
                .Where(r => r.CreatedAt >= fromUtc && r.CreatedAt < toUtc);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                q = q.Where(r =>
                r.ReturnNo.Contains(keyword) ||
                (r.Sale != null && r.Sale.SaleNo.Contains(keyword)));
            }

            return q.OrderByDescending(r => r.CreatedAt).ToList();
        }

        public IReadOnlyList<ReturnItem> GetReturnItems(int returnId)
        {
            return _db.ReturnItems
                .AsNoTracking()
                .Where(i => i.ReturnId == returnId)
                .OrderBy(i => i.Id)
                .ToList();
        }

        public Return GetReturn(int returnId)
        {
            return _db.Returns
                .AsNoTracking()
                .Include(r => r.Sale)
                .Include(r => r.Items)
                .First(r => r.Id == returnId);
        }


        private readonly PosDbContext _db;

        public SaleQueryService(PosDbContext db) => _db = db;

        public IReadOnlyList<SaleRow> SearchSales(DateTime? from, DateTime? to, string? keyword, int limit = 500)
        {
            var q = _db.Sales.AsNoTracking();

            if (from.HasValue)
            {
                var f = from.Value.Date;
                q = q.Where(s => s.CreatedAt >= f);
            }

            if (to.HasValue)
            {
                var t = to.Value.Date.AddDays(1);
                q = q.Where(s => s.CreatedAt < t);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                q = q.Where(s => s.SaleNo.Contains(keyword));
            }

            return q.OrderByDescending(s => s.CreatedAt)
                .Take(limit)
                .Select(s => new SaleRow
                {
                    Id = s.Id,
                    SaleNo = s.SaleNo,
                    CreatedAt = s.CreatedAt,
                    PaymentMethod = s.PaymentMethod,
                    Total = s.Total,
                    PaidAmount = s.PaidAmount,
                    ChangeAmount = s.ChangeAmount
                })
                .ToList();
        }

        public IReadOnlyList<SaleItemRow> GetSaleItems(int saleId)
        {
            return _db.SaleItems.AsNoTracking()
                .Where(i => i.SaleId == saleId)
                .OrderBy(i => i.Id)
                .Select(i => new SaleItemRow
                {
                    Barcode = i.Barcode,
                    Name = i.Name,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity,
                    LineTotal = i.LineTotal
                })
                .ToList();
        }

        public ReceiptDto GetReceipt(int saleId)
        {
            var sale = _db.Sales.AsNoTracking()
                .FirstOrDefault(s => s.Id == saleId);

            if (sale is null)
            {
                throw new InvalidOperationException("找不到交易。");
            }

            var items = GetSaleItems(saleId);

            return new ReceiptDto
            {
                SaleId = sale.Id,
                SaleNo = sale.SaleNo,
                CreatedAtLocal = sale.CreatedAt.ToLocalTime(),
                PaymentMethod = sale.PaymentMethod,
                Subtotal = sale.Subtotal,
                Total = sale.Total,
                PaidAmount = sale.PaidAmount,
                ChangeAmount = sale.ChangeAmount,
                Items = items.ToList()
            };
        }
    }

    public sealed class SaleRow
    {
        public int Id { get; set; }
        public string SaleNo { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public decimal Total { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal ChangeAmount { get; set; }
    }

    public sealed class SaleItemRow
    {
        public string Barcode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }

    public sealed class ReceiptDto
    {
        public int SaleId { get; set; }
        public string SaleNo { get; set; } = string.Empty;
        public DateTime CreatedAtLocal { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal ChangeAmount { get; set; }
        public List<SaleItemRow> Items { get; set; } = new();
    }
}
