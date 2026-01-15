using Microsoft.EntityFrameworkCore;
using POSv01.Domain.Entities;
using POSv01.Infrastructure;
using System;
using System.Linq;

namespace POSv01.Services
{
    public sealed class ReturnQueryService
    {
        private readonly PosDbContext _db;
        public ReturnQueryService(PosDbContext db) => _db = db;

        public ReturnReceiptDto GetReturnReceipt(int returnId)
        {
            var r = _db.Returns
                .AsNoTracking()
                .Include(x => x.Sale)
                .Include(x => x.Items)
                .FirstOrDefault(x => x.Id == returnId);

            if (r is null) throw new InvalidOperationException("找不到退貨單。");

            return new ReturnReceiptDto
            {
                ReturnId = r.Id,
                ReturnNo = r.ReturnNo,
                CreatedAtLocal = r.CreatedAt.ToLocalTime(),
                OriginalSaleNo = r.Sale?.SaleNo ?? "",
                MemberCode = r.MemberCode,
                ClerkName = r.ClerkName,
                TotalRefund = r.TotalRefund,
                Items = r.Items.Select(it => new ReturnItemRow
                {
                    Barcode = it.Barcode,
                    Name = it.Name,
                    UnitPrice = it.UnitPrice,
                    Quantity = it.Quantity,
                    LineTotal = it.LineTotal
                }).ToList()
            };
            /*
            public ReturnReceiptDto GetReturnReceipt(int returnId)
            {
                var ret = _db.Returns.AsNoTracking()
                    .Include(r => r.Sale)
                    .Include(r => r.Items)
                    .First(r => r.Id == returnId);

                return new ReturnReceiptDto
                {
                    ReturnId = ret.Id,
                    ReturnNo = ret.ReturnNo,
                    CreatedAtLocal = ret.CreatedAt.ToLocalTime(),
                    //SaleNo = ret.Sale?.SaleNo ?? "",
                    OriginalSaleNo = ret.Sale?.SaleNo ?? "",
                    MemberCode = ret.MemberCode,
                    ClerkName = ret.ClerkName,

                    TotalRefund = ret.TotalRefund,
                    Items = ret.Items.Select(i => new ReturnItemRow
                    {
                        Barcode = i.Barcode,
                        Name = i.Name,
                        UnitPrice = i.UnitPrice,
                        Quantity = i.Quantity,
                        LineTotal = i.LineTotal
                    }).ToList()
                };
            */
        }
    }
    /*

    public sealed class ReturnReceiptDto
    {
        public int ReturnId { get; set; }
        public string ReturnNo { get; set; } = "";
        public DateTime CreatedAtLocal { get; set; }

        // ✅ ReceiptPreviewForm 需要這三個
        public string OriginalSaleNo { get; set; } = "";
        public string? MemberCode { get; set; }
        public string? ClerkName { get; set; }

        public decimal TotalRefund { get; set; }
        public List<ReturnItemRow> Items { get; set; } = new();
    }

    public sealed class ReturnItemRow
    {
        public string Barcode { get; set; } = "";
        public string Name { get; set; } = "";
        public decimal UnitPrice { get; set; }
        public decimal Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }
    */
}
