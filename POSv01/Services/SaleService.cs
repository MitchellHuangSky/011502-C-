using POSv01.Domain.Entities;
using POSv01.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;


namespace POSv01.Services
{
    /// 處理銷售相關操作（目前：用條碼找商品）。
    /// 之後會擴充到「建立銷售、扣庫存」等。
    public class SaleService
    {
        private readonly PosDbContext _dbContext;

        public SaleService(PosDbContext dbContext)
        {
            // 用來存取資料庫（Products 等）
            _dbContext = dbContext;
        }

        /// <summary>
        /// 用條碼查詢商品，若找不到就回傳 null。
        /// </summary>
        public Product? FindProductByBarcode(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
            {
                return null;
            }

            // 只找還在上架的商品（IsActive = true）
            return _dbContext.Products
                .FirstOrDefault(p => p.Barcode == barcode && p.IsActive);
        }
    }
}
