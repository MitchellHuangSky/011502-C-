using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace POSv01.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(64)]
        public string Barcode { get; set; } = string.Empty;
        [Required]
        [MaxLength(128)]
        public string Name { get; set; } = string.Empty;

        public decimal UnitPrice { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? ImagePath { get; set; }

        //[掃條碼] → [用 Barcode 找 Product] → [顯示商品名稱 + 單價] → [加入購物車] → [結帳]
        //             │
        //             └─ 查詢來源：Domain.Entities.Product


    }
}
