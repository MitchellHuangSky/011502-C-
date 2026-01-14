using POSv01.Services;
using System;
using System.Linq;
using System.Windows.Forms;

namespace POSv01.UI
{
    public sealed class OrderLookupForm : Form
    {
        private readonly SaleQueryService _q;

        private readonly TextBox _txtSaleNo = new() { Left = 16, Top = 16, Width = 260, PlaceholderText = "輸入交易單號" };
        private readonly Button _btnSearch = new() { Left = 284, Top = 14, Width = 80, Text = "查詢" };
        private readonly ListBox _list = new() { Left = 16, Top = 50, Width = 460, Height = 420 };
        private readonly Button _btnReceipt = new() { Left = 384, Top = 14, Width = 92, Text = "收據" };

        private int? _saleId;

        public OrderLookupForm(SaleQueryService q)
        {
            _q = q;

            Text = "查訂單";
            Width = 520;
            Height = 540;
            StartPosition = FormStartPosition.CenterParent;

            Controls.AddRange(new Control[] { _txtSaleNo, _btnSearch, _btnReceipt, _list });

            _btnSearch.Click += (_, _) => LoadSale();
            _btnReceipt.Click += (_, _) => ShowReceipt();

            _txtSaleNo.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    LoadSale();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            };
        }

        private void LoadSale()
        {
            _saleId = null;
            _list.Items.Clear();

            var key = _txtSaleNo.Text.Trim();
            if (string.IsNullOrWhiteSpace(key)) return;

            // 用 SearchSales 找出該單號的 Id（最簡單）
            var row = _q.SearchSales(null, null, key, limit: 50).FirstOrDefault(s => s.SaleNo == key);
            if (row == null)
            {
                MessageBox.Show("找不到交易");
                return;
            }

            _saleId = row.Id;

            var receipt = _q.GetReceipt(row.Id);
            foreach (var it in receipt.Items)
            {
                _list.Items.Add($"{it.Name}  {it.Quantity:0.##} x {it.UnitPrice:0.##} = {it.LineTotal:0.##}");
            }

            _list.Items.Add($"---------------------------");
            _list.Items.Add($"總額: {receipt.Total:0.##} / 付款: {receipt.PaymentMethod}");
        }

        private void ShowReceipt()
        {
            if (_saleId == null)
            {
                MessageBox.Show("請先查詢交易");
                return;
            }

            var dto = _q.GetReceipt(_saleId.Value);
            using var preview = new ReceiptPreviewForm(dto);
            preview.ShowDialog(this);
        }
    }
}
