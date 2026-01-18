using POSv01.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;


namespace POSv01.UI
{
    //簡單庫存調整 UI
    public sealed class InventoryAdjustForm : Form
    {
        private readonly ProductService _productService;
        private readonly InventoryService _inventoryService;

        private readonly TextBox _txtKeyword = new() { Left = 16, Top = 16, Width = 260, PlaceholderText = "條碼/品名" };
        private readonly Button _btnSearch = new() { Left = 284, Top = 14, Width = 80, Text = "搜尋" };
        private readonly ListBox _lst = new() { Left = 16, Top = 48, Width = 348, Height = 220 };

        private readonly TextBox _txtDelta = new() { Left = 16, Top = 280, Width = 160, Text = "0" };
        private readonly TextBox _txtReason = new() { Left = 184, Top = 280, Width = 180, Text = "調整" };

        private readonly Button _btnApply = new() { Left = 16, Top = 320, Width = 100, Text = "套用" };
        private readonly Button _btnClose = new() { Left = 264, Top = 320, Width = 100, Text = "關閉" };

        public InventoryAdjustForm(ProductService productService, InventoryService inventoryService)
        {
            _productService = productService;
            _inventoryService = inventoryService;

            Text = "庫存調整/進貨";
            Width = 400;
            Height = 420;
            StartPosition = FormStartPosition.CenterParent;

            Controls.AddRange(new Control[] { _txtKeyword, _btnSearch, _lst, _txtDelta, _txtReason, _btnApply, _btnClose });

            _btnSearch.Click += (_, _) => LoadProducts();
            _txtKeyword.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Enter) { LoadProducts(); e.Handled = true; e.SuppressKeyPress = true; }
            };

            _btnApply.Click += (_, _) => Apply();
            _btnClose.Click += (_, _) => Close();

            LoadProducts();
        }

        private void LoadProducts()
        {
            var list = _productService.GetActiveProducts(_txtKeyword.Text.Trim());
            _lst.Items.Clear();
            foreach (var p in list)
            {
                _lst.Items.Add(new Item(p.Id, $"{p.Barcode}  {p.Name}  ${p.UnitPrice:0.##}"));
            }
            if (_lst.Items.Count > 0) _lst.SelectedIndex = 0;
        }

        private void Apply()
        {
            if (_lst.SelectedItem is not Item it)
            {
                MessageBox.Show("請先選商品。");
                return;
            }

            if (!decimal.TryParse(_txtDelta.Text.Trim(), out var delta))
            {
                MessageBox.Show("加減數量格式錯誤。");
                return;
            }

            var reason = _txtReason.Text.Trim();
            if (string.IsNullOrWhiteSpace(reason)) reason = "Adjust";

            try
            {
                _inventoryService.Adjust(it.ProductId, delta, reason);
                MessageBox.Show("已套用。");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "失敗");
            }
        }

        private sealed record Item(int ProductId, string Text)
        {
            public override string ToString() => Text;
        }
    }
}
