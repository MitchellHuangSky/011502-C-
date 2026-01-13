using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace POSv01.Domain.Entities
{
    public class ReturnItem
    {
        public int Id { get; set; }

        public int ReturnId { get; set; }
        public Return Return { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        [MaxLength(64)]
        public string Barcode { get; set; } = string.Empty;

        [MaxLength(128)]
        public string Name { get; set; } = string.Empty;

        public decimal UnitPrice { get; set; }
        public decimal Quantity { get; set; }  // 退貨數量（正數）
        public decimal LineTotal { get; set; } // 退貨金額（正數）
    }
}
