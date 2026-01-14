using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace POSv01.Domain.Entities
{
    public class Return
    {
        public int Id { get; set; }

        public string ReturnNo { get; set; } = string.Empty;

        public int SaleId { get; set; }
        public Sale? Sale { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public decimal TotalRefund { get; set; }

        public string? ClerkName { get; set; }
        public string? MemberCode { get; set; }

        public List<ReturnItem> Items { get; set; } = new();
    }
}
