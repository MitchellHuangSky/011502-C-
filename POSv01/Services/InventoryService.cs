using POSv01.Domain.Entities;
using POSv01.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;


namespace POSv01.Services
{
    public sealed class InventoryService
    {
        private readonly PosDbContext _db;

        public InventoryService(PosDbContext db)
        {
            _db = db;
        }

        public Inventory GetOrCreate(int productId)
        {
            var inv = _db.Inventories.SingleOrDefault(x => x.ProductId == productId);
            if (inv != null) return inv;

            inv = new Inventory { ProductId = productId, QtyOnHand = 0, UpdatedAt = DateTime.UtcNow };
            _db.Inventories.Add(inv);
            _db.SaveChanges();
            return inv;
        }

        public void Adjust(int productId, decimal delta, string reason, string refNo = "Manual")
        {
            var inv = GetOrCreate(productId);
            inv.QtyOnHand += delta;
            inv.UpdatedAt = DateTime.UtcNow;

            _db.InventoryTxns.Add(new InventoryTxn
            {
                TxnType = InventoryTxnType.Adjust,
                ProductId = productId,
                QtyDelta = delta,
                Reason = reason ?? string.Empty,
                RefNo = refNo ?? "Manual",
                CreatedAt = DateTime.UtcNow
            });

            _db.SaveChanges();
        }

        public void ApplySale(string saleNo, IReadOnlyList<(int productId, decimal qty)> lines, bool allowNegativeStock)
        {
            foreach (var (productId, qty) in lines)
            {
                var inv = GetOrCreate(productId);

                var after = inv.QtyOnHand - qty;
                if (!allowNegativeStock && after < 0)
                {
                    throw new InvalidOperationException($"庫存不足：ProductId={productId} 現有={inv.QtyOnHand:0.##} 需扣={qty:0.##}");
                }

                inv.QtyOnHand = after;
                inv.UpdatedAt = DateTime.UtcNow;

                _db.InventoryTxns.Add(new InventoryTxn
                {
                    TxnType = InventoryTxnType.Sale,
                    ProductId = productId,
                    QtyDelta = -qty,
                    RefNo = saleNo,
                    Reason = "Sale",
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        public void ApplyReturn(string returnNo, IReadOnlyList<(int productId, decimal qty)> lines)
        {
            foreach (var (productId, qty) in lines)
            {
                var inv = GetOrCreate(productId);
                inv.QtyOnHand += qty;
                inv.UpdatedAt = DateTime.UtcNow;

                _db.InventoryTxns.Add(new InventoryTxn
                {
                    TxnType = InventoryTxnType.Return,
                    ProductId = productId,
                    QtyDelta = qty,
                    RefNo = returnNo,
                    Reason = "Return",
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
    }
}
