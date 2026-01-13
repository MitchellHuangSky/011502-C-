using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace POSv01.Domain.Entities
{
    public class Return
    {
        public int Id { get; set; }

        [Required, MaxLength(32)]
        public string ReturnNo { get; set; } = string.Empty;

        public int SaleId { get; set; }
        public Sale Sale { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public decimal TotalRefund { get; set; }

        [MaxLength(64)]
        public string? ClerkName { get; set; }

        [MaxLength(64)]
        public string? MemberCode { get; set; }

        public List<ReturnItem> Items { get; set; } = new();
    }
}
