using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace POSv01.Domain.Entities
{
    public class InventoryTxn
    {
        public int Id { get; set; }

        public InventoryTxnType TxnType { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public decimal QtyDelta { get; set; }

        [MaxLength(64)]
        public string RefNo { get; set; } = string.Empty; // SaleNo / ReturnNo / Manual

        [MaxLength(128)]
        public string Reason { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
