/*
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

            // 用 SearchSales 找出該單號的 Id
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
*/

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using POSv01.Services;

namespace POSv01.UI
{
    public sealed class OrderLookupForm : Form
    {
        private readonly SaleQueryService _saleQuery;
        private readonly ReturnQueryService _returnQuery;

        private readonly DateTimePicker _dtFrom = new() { Width = 140 };
        private readonly DateTimePicker _dtTo = new() { Width = 140 };
        private readonly TextBox _txtKeyword = new() { Width = 180, PlaceholderText = "單號關鍵字(可空)" };
        private readonly Button _btnSearch = new() { Text = "查詢", Width = 80 };

        private readonly TabControl _tabs = new() { Dock = DockStyle.Fill };
        private readonly DataGridView _gridSales = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false };
        private readonly DataGridView _gridReturns = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false };

        private BindingList<SaleRow> _sales = new();
        private BindingList<ReturnRowVm> _returns = new();

        public OrderLookupForm(SaleQueryService saleQuery, ReturnQueryService returnQuery, bool defaultToReturns = false)
        {
            _saleQuery = saleQuery;
            _returnQuery = returnQuery;

            Text = "查訂單 / 退貨單";
            Width = 980;
            Height = 650;
            StartPosition = FormStartPosition.CenterParent;

            var top = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 44,
                Padding = new Padding(10, 8, 10, 8),
                AutoSize = false
            };

            top.Controls.Add(new Label { Text = "起日", AutoSize = true, Padding = new Padding(0, 6, 0, 0) });
            top.Controls.Add(_dtFrom);
            top.Controls.Add(new Label { Text = "迄日", AutoSize = true, Padding = new Padding(10, 6, 0, 0) });
            top.Controls.Add(_dtTo);
            top.Controls.Add(_txtKeyword);
            top.Controls.Add(_btnSearch);

            Controls.Add(_tabs);
            Controls.Add(top);

            // Tabs
            var tabSales = new TabPage("訂單(交易)");
            var tabReturns = new TabPage("退貨單");
            tabSales.Controls.Add(_gridSales);
            tabReturns.Controls.Add(_gridReturns);
            _tabs.TabPages.Add(tabSales);
            _tabs.TabPages.Add(tabReturns);

            InitSalesGrid();
            InitReturnsGrid();

            _btnSearch.Click += (_, _) => Reload();

            _gridSales.CellDoubleClick += (_, e) =>
            {
                if (e.RowIndex < 0) return;
                if (_gridSales.Rows[e.RowIndex].DataBoundItem is not SaleRow row) return;

                var receipt = _saleQuery.GetReceipt(row.Id);
                using var preview = new ReceiptPreviewForm(receipt, autoPreview: true);
                preview.ShowDialog(this);
            };

            _gridReturns.CellDoubleClick += (_, e) =>
            {
                if (e.RowIndex < 0) return;
                if (_gridReturns.Rows[e.RowIndex].DataBoundItem is not ReturnRowVm row) return;

                var dto = _returnQuery.GetReturnReceipt(row.Id);
                using var preview = new ReceiptPreviewForm(dto, autoPreview: true);
                preview.ShowDialog(this);
            };

            // ✅ 預設今天
            _dtFrom.Value = DateTime.Today;
            _dtTo.Value = DateTime.Today;

            Shown += (_, _) =>
            {
                if (defaultToReturns) _tabs.SelectedIndex = 1;
                Reload();
            };
        }

        private void InitSalesGrid()
        {
            _gridSales.Columns.Clear();
            _gridSales.AllowUserToAddRows = false;
            _gridSales.MultiSelect = false;
            _gridSales.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            _gridSales.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(SaleRow.SaleNo), HeaderText = "單號", Width = 220 });
            _gridSales.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(SaleRow.CreatedAt), HeaderText = "時間(UTC)", Width = 160 });
            _gridSales.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(SaleRow.PaymentMethod), HeaderText = "付款", Width = 90 });
            _gridSales.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(SaleRow.Total), HeaderText = "總額", Width = 90, DefaultCellStyle = { Format = "0.##" } });

            _gridSales.DataSource = _sales;
        }

        private void InitReturnsGrid()
        {
            _gridReturns.Columns.Clear();
            _gridReturns.AllowUserToAddRows = false;
            _gridReturns.MultiSelect = false;
            _gridReturns.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            _gridReturns.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ReturnRowVm.ReturnNo), HeaderText = "退貨單號", Width = 220 });
            _gridReturns.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ReturnRowVm.CreatedAtLocal), HeaderText = "時間(本機)", Width = 160 });
            _gridReturns.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ReturnRowVm.OriginalSaleNo), HeaderText = "原單號", Width = 220 });
            _gridReturns.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ReturnRowVm.TotalRefund), HeaderText = "退款", Width = 90, DefaultCellStyle = { Format = "0.##" } });

            _gridReturns.DataSource = _returns;
        }

        private void Reload()
        {
            var from = _dtFrom.Value.Date;
            var to = _dtTo.Value.Date;
            if (to < from)
            {
                MessageBox.Show("迄日不可小於起日");
                return;
            }

            var keyword = _txtKeyword.Text?.Trim();

            var sales = _saleQuery.SearchSales(from, to, keyword).ToList();
            _sales = new BindingList<SaleRow>(sales);
            _gridSales.DataSource = _sales;

            var returns = _saleQuery.SearchReturns(from, to, keyword)
                .Select(r => new ReturnRowVm
                {
                    Id = r.Id,
                    ReturnNo = r.ReturnNo,
                    CreatedAtLocal = r.CreatedAt.ToLocalTime(),
                    OriginalSaleNo = r.Sale?.SaleNo ?? "",
                    TotalRefund = r.TotalRefund
                })
                .ToList();

            _returns = new BindingList<ReturnRowVm>(returns);
            _gridReturns.DataSource = _returns;
        }

        private sealed class ReturnRowVm
        {
            public int Id { get; set; }
            public string ReturnNo { get; set; } = "";
            public DateTime CreatedAtLocal { get; set; }
            public string OriginalSaleNo { get; set; } = "";
            public decimal TotalRefund { get; set; }
        }
    }
}
