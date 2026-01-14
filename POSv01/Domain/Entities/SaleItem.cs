using System;
//using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
//using System.Text;

namespace POSv01.Domain.Entities
{
    /// <summary>
    /// 交易明細（對應購物車每一列）。
    /// </summary>
    public sealed class SaleItem
    {
        public int Id { get; set; }

        public int SaleId { get; set; }
        public Sale? Sale { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public string Barcode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public decimal UnitPrice { get; set; }
        public decimal Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }
}
