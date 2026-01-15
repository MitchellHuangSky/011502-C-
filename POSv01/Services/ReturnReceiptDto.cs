using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSv01.Services
{
    public sealed class ReturnItemRow
    {
        public string Barcode { get; set; } = "";
        public string Name { get; set; } = "";
        public decimal UnitPrice { get; set; }
        public decimal Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }

    public sealed class ReturnReceiptDto
    {
        public int ReturnId { get; set; }
        public string ReturnNo { get; set; } = "";
        public DateTime CreatedAtLocal { get; set; }

        // ✅ ReceiptPreviewForm 會用到
        public string OriginalSaleNo { get; set; } = "";
        public string? MemberCode { get; set; }
        public string? ClerkName { get; set; }

        public decimal TotalRefund { get; set; }
        public List<ReturnItemRow> Items { get; set; } = new();
    }
}
