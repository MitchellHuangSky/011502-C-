using System;
using System.Collections.Generic;
using System.Text;

namespace POSv01.Domain.Entities
{
    public enum InventoryTxnType
    {
        Purchase = 1,   // 進貨
        Sale = 2,       // 銷售扣庫存
        Adjust = 3,     // 人工調整
        Return = 4      // 退貨加回
    }
}
