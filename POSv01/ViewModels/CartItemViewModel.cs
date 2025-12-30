using POSv01.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace POSv01.ViewModels
{
    /// 給購物車 DataGridView 使用的一列資料。
    
    public class CartItemViewModel
    {
        public Product Product { get; }

        public decimal Quantity { get; set; }

        public decimal UnitPrice => Product.UnitPrice;

        public decimal LineTotal => UnitPrice * Quantity;

        public string Barcode => Product.Barcode;

        public string Name => Product.Name;

        public CartItemViewModel(Product product, decimal quantity = 1)
        {
            Product = product;
            Quantity = quantity;
        }
    }
}
