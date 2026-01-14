using System.Windows.Forms;

namespace POSv01.UI
{
    public sealed class PosHelpForm : Form
    {
        public PosHelpForm()
        {
            Text = "POS 操作流程";
            Width = 520;
            Height = 420;
            StartPosition = FormStartPosition.CenterParent;

            var box = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Vertical,
                Text =
@"POS 操作流程

1) 加入商品
- 掃條碼後按 Enter
- 或點「商品」開啟商品照片卡，點一下加入購物車

2) 修改/移除
- 直接修改購物車「數量」
- Delete 或「移除選取」刪除該列
- 「清空」清掉購物車

3) 結帳
- 現金：輸入收款金額 → 完成
- 信用卡/行動支付：先用模擬完成
- 結帳成功後自動跳出『收據預覽』

4) 查訂單
- 點「查訂單」→ 輸入交易單號 → 顯示品項
- 可按「收據」重印/預覽

5) 退貨
- 點「退貨」→ 輸入原交易單號 → 填退貨數量 → 送出
- 成功後顯示『退貨單預覽』"
            };

            Controls.Add(box);
        }
    }
}
