using POSv01.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

using POSv01.Domain.Entities;


namespace POSv01.Services
{
    public sealed class ReturnQueryService
    {
        private readonly PosDbContext _db;

        public ReturnQueryService(PosDbContext db)
        {
            _db = db;
        }


        public IReadOnlyList<ReturnRow> SearchReturns(DateTime fromDate, DateTime toDate, string? keyword)
        {
            var from = fromDate.Date;
            var toExclusive = toDate.Date.AddDays(1);

            var q = _db.Returns.AsNoTracking()
                .Include(r => r.Sale)
                .Where(r => r.CreatedAt >= from && r.CreatedAt < toExclusive);

            keyword = (keyword ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                q = q.Where(r =>
                    r.ReturnNo.Contains(keyword) ||
                    (r.Sale != null && r.Sale.SaleNo.Contains(keyword)) ||
                    (r.MemberCode != null && r.MemberCode.Contains(keyword)) ||
                    (r.ClerkName != null && r.ClerkName.Contains(keyword)));
            }

            return q.OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReturnRow
                {
                    Id = r.Id,
                    ReturnNo = r.ReturnNo,
                    OriginalSaleNo = r.Sale != null ? r.Sale.SaleNo : string.Empty,
                    CreatedAt = r.CreatedAt,
                    TotalRefund = r.TotalRefund,
                    MemberCode = r.MemberCode,
                    ClerkName = r.ClerkName
                })
                .ToList();
        }

        public IReadOnlyList<ReturnItemRow> GetReturnItems(int returnId)
        {
            return _db.ReturnItems.AsNoTracking()
                .Where(i => i.ReturnId == returnId)
                .OrderBy(i => i.Id)
                .Select(i => new ReturnItemRow
                {
                    Barcode = i.Barcode,
                    Name = i.Name,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity,
                    LineTotal = i.LineTotal
                })
                .ToList();
        }

        public ReturnReceiptDto GetReturnReceipt(int returnId)
        {
            var ret = _db.Returns.AsNoTracking()
                .Include(r => r.Sale)
                .Include(r => r.Items)
                .FirstOrDefault(r => r.Id == returnId);

            if (ret is null)
            {
                throw new InvalidOperationException("找不到退貨單。");
            }

            return new ReturnReceiptDto
            {
                ReturnId = ret.Id,
                ReturnNo = ret.ReturnNo,
                CreatedAtLocal = ret.CreatedAt.ToLocalTime(),
                OriginalSaleNo = ret.Sale?.SaleNo ?? string.Empty,
                TotalRefund = ret.TotalRefund,
                ClerkName = ret.ClerkName,
                MemberCode = ret.MemberCode,
                Items = ret.Items
                    .OrderBy(i => i.Id)
                    .Select(i => new ReturnReceiptItemDto
                    {
                        Barcode = i.Barcode,
                        Name = i.Name,
                        UnitPrice = i.UnitPrice,
                        Quantity = i.Quantity,
                        LineTotal = i.LineTotal
                    })
                    .ToList()
            };
        }
    }

    public sealed class ReturnRow
    {
        public int Id { get; set; }
        public string ReturnNo { get; set; } = string.Empty;
        public string OriginalSaleNo { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public decimal TotalRefund { get; set; }
        public string? MemberCode { get; set; }
        public string? ClerkName { get; set; }
    }

    public sealed class ReturnItemRow
    {
        public string Barcode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }

    

public sealed class ReturnReceiptDto
    {
        public int ReturnId { get; set; }
        public string ReturnNo { get; set; } = string.Empty;
        public DateTime CreatedAtLocal { get; set; }
        public string OriginalSaleNo { get; set; } = string.Empty;
        public decimal TotalRefund { get; set; }
        public string? ClerkName { get; set; }
        public string? MemberCode { get; set; }
        public List<ReturnReceiptItemDto> Items { get; set; } = new();
    }

    public sealed class ReturnReceiptItemDto
    {
        public string Barcode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }
}
