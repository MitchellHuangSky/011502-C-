
using System;
using System.Collections.Generic;
using System.Linq;
using POSv01.Domain.Entities;
using POSv01.Infrastructure;
using POSv01.ViewModels;

namespace POSv01.Services
{
    public sealed class CheckoutService
    {
        private readonly PosDbContext _db;
        private readonly InventoryService _inventory;
        private readonly bool _allowNegativeStock;

        public CheckoutService(PosDbContext db, InventoryService inventory, bool allowNegativeStock = true)
        {
            _db = db;
            _inventory = inventory;
            _allowNegativeStock = allowNegativeStock;
        }

        public Sale Checkout(IReadOnlyList<CartItemViewModel> cartItems, PaymentMethod method, decimal paidAmount,
            string? clerkName = null, string? memberCode = null)
        {
            if (cartItems == null || cartItems.Count == 0)
                throw new InvalidOperationException("購物車是空的。");

            var subtotal = cartItems.Sum(i => i.LineTotal);
            var total = subtotal;

            if (method == PaymentMethod.Cash)
            {
                if (paidAmount < total)
                    throw new InvalidOperationException("現金不足，無法結帳。");
            }
            else
            {
                paidAmount = total;
            }

            var sale = new Sale
            {
                SaleNo = GenerateSaleNo(),
                CreatedAt = DateTime.UtcNow,
                PaymentMethod = method,
                Subtotal = subtotal,
                Total = total,
                PaidAmount = paidAmount,
                ChangeAmount = paidAmount - total,
                ClerkName = clerkName,
                MemberCode = memberCode
            };

            foreach (var ci in cartItems)
            {
                var p = ci.Product;
                sale.Items.Add(new SaleItem
                {
                    ProductId = p.Id,
                    Barcode = p.Barcode,
                    Name = p.Name,
                    UnitPrice = p.UnitPrice,
                    Quantity = ci.Quantity,
                    LineTotal = ci.LineTotal
                });
            }

            // 交易 + 庫存：同一個 SaveChanges
            _db.Sales.Add(sale);

            _inventory.ApplySale(
                sale.SaleNo,
                sale.Items.Select(x => (x.ProductId, x.Quantity)).ToList(),
                allowNegativeStock: _allowNegativeStock
            );

            _db.SaveChanges();
            return sale;
        }

        private static string GenerateSaleNo()
        {
            var ts = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            var rnd = Random.Shared.Next(1000, 9999);
            return $"{ts}-{rnd}";
        }
    }





    /*
    /// <summary>
    /// 結帳：把購物車寫成 Sale + SaleItem。
    /// </summary>
    public class CheckoutService
    {


        private readonly PosDbContext _db;        
        
        //public CheckoutService(PosDbContext db)
        //{
        //    _db = db;
        //}
        
        public CheckoutService(PosDbContext db) => _db = db;

        public Sale Checkout(
            IReadOnlyList<CartItemViewModel> cartItems,
            PaymentMethod paymentMethod,
            decimal paidAmount)
        {
            if (cartItems == null || cartItems.Count == 0)
            {
                throw new InvalidOperationException("購物車是空的。");
            }

            var subtotal = cartItems.Sum(i => i.LineTotal);
            var total = subtotal; // 先不做折扣/促銷，之後再擴充

            if (paymentMethod == PaymentMethod.Cash)
            {
                if (paidAmount < total)
                {
                    throw new InvalidOperationException("現金不足，無法結帳。");
                }
            }
            else
            {
                // 卡/行動支付：視為剛好付清
                paidAmount = total;
            }

            var change = paidAmount - total;

            var sale = new Sale
            {
                SaleNo = GenerateSaleNo(),
                CreatedAt = DateTime.UtcNow,
                PaymentMethod = paymentMethod,
                Subtotal = subtotal,
                Total = total,
                PaidAmount = paidAmount,
                ChangeAmount = change
            };

            foreach (var ci in cartItems)
            {
                var p = ci.Product;
                sale.Items.Add(new SaleItem
                {
                    ProductId = p.Id,
                    Barcode = p.Barcode,
                    Name = p.Name,
                    UnitPrice = p.UnitPrice,
                    Quantity = ci.Quantity,
                    LineTotal = ci.LineTotal
                });
            }

            _db.Sales.Add(sale);
            _db.SaveChanges();

            return sale;
        }

        private static string GenerateSaleNo()
        {
            // 交易單號：yyyyMMddHHmmssfff-xxxx
            var ts = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            var rnd = Random.Shared.Next(1000, 9999);
            return $"{ts}-{rnd}";
        }


    }

    */
}