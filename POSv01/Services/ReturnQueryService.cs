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
        }
    }
}
