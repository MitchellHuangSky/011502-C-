using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.ServerSentEvents;
using System.Text;

namespace POSv01.Services
{
    /// <summary>
    /// 查詢/畫面用資料（不要拿來當 EF Entity）
    /// </summary>
    public class SaleDto
    {
        public int Id { get; set; }
        public string SaleNo { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public string PaymentMethod { get; set; } = string.Empty;

        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal ChangeAmount { get; set; }

        public string? ClerkName { get; set; }
        public string? MemberCode { get; set; }
    }

}