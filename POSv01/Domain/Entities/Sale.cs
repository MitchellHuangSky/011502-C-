using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.ServerSentEvents;
using System.Text;

namespace POSv01.Domain.Entities
{
    /// <summary>
    /// 交易主檔（一次結帳一筆）。
    /// </summary>
    //public class Sale
        
    public partial class Sale
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(32)]
        public string SaleNo { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public PaymentMethod PaymentMethod { get; set; }

        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }

        public decimal PaidAmount { get; set; }
        public decimal ChangeAmount { get; set; }

        public List<SaleItem> Items { get; set; } = new();

        [MaxLength(64)]
        public string? ClerkName { get; set; }

        [MaxLength(64)]
        public string? MemberCode { get; set; }
    }

    
}