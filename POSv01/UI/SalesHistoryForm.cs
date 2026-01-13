using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using POSv01.Domain.Entities;
using POSv01.Services;

namespace POSv01
{
    public sealed class SalesHistoryForm : Form
    {
        private readonly SaleQueryService _svc;

        private readonly DateTimePicker _dtFrom = new() { Format = DateTimePickerFormat.Short, Width = 120 };
        private readonly DateTimePicker _dtTo = new() { Format = DateTimePickerFormat.Short, Width = 120 };
        private readonly TextBox _txtKeyword = new() { Width = 180, PlaceholderText = "單號關鍵字" };
        private readonly Button _btnSearch = new() { Text = "搜尋", Width = 80 };
        private readonly Button _btnToday = new() { Text = "今天", Width = 80 };

        // 上半部：交易/退貨兩個清單
        private readonly TabControl _tabs = new() { Dock = DockStyle.Fill };
        private readonly TabPage _tabSales = new("交易");
        private readonly TabPage _tabReturns = new("退貨");

        private readonly DataGridView _gridSales = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false };
        private readonly DataGridView _gridReturns = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false };

        // 下半部：明細（共用）
        private readonly DataGridView _gridItems = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false };

        private readonly BindingList<SaleRowVm> _sales = new();
        private readonly BindingList<ReturnRowVm> _returns = new();
        private readonly BindingList<LineItemVm> _items = new();

        // 右鍵選單
        private readonly ContextMenuStrip _menuSales = new();
        private readonly ContextMenuStrip _menuReturns = new();

        public SalesHistoryForm(SaleQueryService svc)
        {
            _svc = svc;

            Text = "交易 / 退貨 查詢";
            Width = 1100;
            Height = 700;
            StartPosition = FormStartPosition.CenterParent;

            var top = BuildTopPanel();
            var split = BuildSplit();

            Controls.Add(split);
            Controls.Add(top);

            InitSalesGrid();
            InitReturnsGrid();
            InitItemsGrid();
            InitContextMenus();

            _btnSearch.Click += (_, _) => LoadData();
            _btnToday.Click += (_, _) =>
            {
                var today = DateTime.Today;
                _dtFrom.Value = today;
                _dtTo.Value = today;
                LoadData();
            };

            _tabs.SelectedIndexChanged += (_, _) => LoadSelectedItems();

            _gridSales.SelectionChanged += (_, _) => { if (IsSalesTab) LoadSelectedItems(); };
            _gridReturns.SelectionChanged += (_, _) => { if (!IsSalesTab) LoadSelectedItems(); };

            _gridSales.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) OpenPreview(autoPreview: false); };
            _gridReturns.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) OpenPreview(autoPreview: false); };

            _dtFrom.Value = DateTime.Today;
            _dtTo.Value = DateTime.Today;
            Shown += (_, _) => LoadData();
        }

        private bool IsSalesTab => _tabs.SelectedTab == _tabSales;

        private Panel BuildTopPanel()
        {
            var p = new Panel { Dock = DockStyle.Top, Height = 52, Padding = new Padding(10) };

            var lblFrom = new Label { Text = "起", AutoSize = true, Top = 16, Left = 10 };
            _dtFrom.Left = 30; _dtFrom.Top = 12;

            var lblTo = new Label { Text = "迄", AutoSize = true, Top = 16, Left = 165 };
            _dtTo.Left = 185; _dtTo.Top = 12;

            _txtKeyword.Left = 320; _txtKeyword.Top = 12;
            _btnSearch.Left = 510; _btnSearch.Top = 10;
            _btnToday.Left = 595; _btnToday.Top = 10;

            p.Controls.Add(lblFrom);
            p.Controls.Add(_dtFrom);
            p.Controls.Add(lblTo);
            p.Controls.Add(_dtTo);
            p.Controls.Add(_txtKeyword);
            p.Controls.Add(_btnSearch);
            p.Controls.Add(_btnToday);

            return p;
        }

        private SplitContainer BuildSplit()
        {
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 330
            };

            _tabSales.Controls.Add(_gridSales);
            _tabReturns.Controls.Add(_gridReturns);
            _tabs.TabPages.Add(_tabSales);
            _tabs.TabPages.Add(_tabReturns);

            split.Panel1.Controls.Add(_tabs);
            split.Panel2.Controls.Add(_gridItems);
            return split;
        }

        private void InitSalesGrid()
        {
            _gridSales.AllowUserToAddRows = false;
            _gridSales.MultiSelect = false;
            _gridSales.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            _gridSales.Columns.Clear();
            _gridSales.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(SaleRowVm.CreatedAt), HeaderText = "時間", Width = 160 });
            _gridSales.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(SaleRowVm.SaleNo), HeaderText = "單號", Width = 200 });
            _gridSales.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(SaleRowVm.PaymentMethodText), HeaderText = "付款", Width = 90 });
            _gridSales.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(SaleRowVm.Total), HeaderText = "總額", Width = 90, DefaultCellStyle = { Format = "0.##" } });
            _gridSales.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(SaleRowVm.PaidAmount), HeaderText = "實收", Width = 90, DefaultCellStyle = { Format = "0.##" } });
            _gridSales.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(SaleRowVm.ChangeAmount), HeaderText = "找零", Width = 90, DefaultCellStyle = { Format = "0.##" } });

            _gridSales.DataSource = _sales;
        }

        private void InitReturnsGrid()
        {
            _gridReturns.AllowUserToAddRows = false;
            _gridReturns.MultiSelect = false;
            _gridReturns.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            _gridReturns.Columns.Clear();
            _gridReturns.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ReturnRowVm.CreatedAt), HeaderText = "時間", Width = 160 });
            _gridReturns.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ReturnRowVm.ReturnNo), HeaderText = "退貨單號", Width = 220 });
            _gridReturns.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ReturnRowVm.OriginalSaleNo), HeaderText = "原單號", Width = 220 });
            _gridReturns.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ReturnRowVm.TotalRefund), HeaderText = "退款", Width = 90, DefaultCellStyle = { Format = "0.##" } });
            _gridReturns.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ReturnRowVm.ClerkName), HeaderText = "店員", Width = 100 });
            _gridReturns.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(ReturnRowVm.MemberCode), HeaderText = "會員", Width = 120 });

            _gridReturns.DataSource = _returns;
        }

        private void InitItemsGrid()
        {
            _gridItems.AllowUserToAddRows = false;
            _gridItems.MultiSelect = false;
            _gridItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            _gridItems.Columns.Clear();
            _gridItems.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(LineItemVm.Barcode), HeaderText = "條碼", Width = 160 });
            _gridItems.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(LineItemVm.Name), HeaderText = "品名", Width = 340 });
            _gridItems.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(LineItemVm.UnitPrice), HeaderText = "單價", Width = 90, DefaultCellStyle = { Format = "0.##" } });
            _gridItems.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(LineItemVm.Quantity), HeaderText = "數量", Width = 90, DefaultCellStyle = { Format = "0.##" } });
            _gridItems.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(LineItemVm.LineTotal), HeaderText = "小計", Width = 90, DefaultCellStyle = { Format = "0.##" } });

            _gridItems.DataSource = _items;
        }

        private void InitContextMenus()
        {
            // 右鍵時先選到那一列
            _gridSales.CellMouseDown += Grid_CellMouseDown_SelectRow;
            _gridReturns.CellMouseDown += Grid_CellMouseDown_SelectRow;

            _menuSales.Items.Add("複製單號", null, (_, _) => CopyCurrentNo());
            _menuSales.Items.Add("重印（預覽列印）", null, (_, _) => OpenPreview(autoPreview: true));
            _gridSales.ContextMenuStrip = _menuSales;

            _menuReturns.Items.Add("複製單號", null, (_, _) => CopyCurrentNo());
            _menuReturns.Items.Add("重印（預覽列印）", null, (_, _) => OpenPreview(autoPreview: true));
            _gridReturns.ContextMenuStrip = _menuReturns;
        }

        private void Grid_CellMouseDown_SelectRow(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            var grid = (DataGridView)sender!;
            if (e.RowIndex < 0) return;

            grid.ClearSelection();
            grid.CurrentCell = grid.Rows[e.RowIndex].Cells[0];
            grid.Rows[e.RowIndex].Selected = true;
        }

        private void CopyCurrentNo()
        {
            if (IsSalesTab)
            {
                if (_gridSales.CurrentRow?.DataBoundItem is not SaleRowVm s) return;
                Clipboard.SetText(s.SaleNo ?? "");
            }
            else
            {
                if (_gridReturns.CurrentRow?.DataBoundItem is not ReturnRowVm r) return;
                Clipboard.SetText(r.ReturnNo ?? "");
            }
        }

        private void LoadData()
        {
            var from = _dtFrom.Value.Date;
            var to = _dtTo.Value.Date;
            if (to < from)
            {
                MessageBox.Show("日期區間不正確。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var keyword = _txtKeyword.Text.Trim();

            // 交易
            var salesRows = _svc.SearchSales(from, to, keyword);
            _sales.Clear();
            foreach (var r in salesRows)
                _sales.Add(new SaleRowVm(r));

            // 退貨（你要在 SaleQueryService 補 SearchReturns）
            var returnRows = _svc.SearchReturns(from, to, keyword);
            _returns.Clear();
            foreach (var r in returnRows)
                _returns.Add(new ReturnRowVm(r));

            _items.Clear();

            // 預設選第一筆（交易優先，沒有交易才選退貨）
            if (_sales.Count > 0)
            {
                _tabs.SelectedTab = _tabSales;
                SelectFirstRow(_gridSales);
            }
            else if (_returns.Count > 0)
            {
                _tabs.SelectedTab = _tabReturns;
                SelectFirstRow(_gridReturns);
            }
        }

        private static void SelectFirstRow(DataGridView grid)
        {
            if (grid.Rows.Count <= 0) return;
            grid.ClearSelection();
            grid.CurrentCell = grid.Rows[0].Cells[0];
            grid.Rows[0].Selected = true;
        }

        private void LoadSelectedItems()
        {
            _items.Clear();

            if (IsSalesTab)
            {
                if (_gridSales.CurrentRow?.DataBoundItem is not SaleRowVm sale) return;

                var rows = _svc.GetSaleItems(sale.Id);
                foreach (var r in rows)
                    _items.Add(new LineItemVm(r.Barcode, r.Name, r.UnitPrice, r.Quantity, r.LineTotal));
            }
            else
            {
                if (_gridReturns.CurrentRow?.DataBoundItem is not ReturnRowVm ret) return;

                // 你要在 SaleQueryService 補 GetReturnItems
                var rows = _svc.GetReturnItems(ret.Id);
                foreach (var r in rows)
                    _items.Add(new LineItemVm(r.Barcode, r.Name, r.UnitPrice, r.Quantity, r.LineTotal));
            }
        }

        private void OpenPreview(bool autoPreview)
        {
            if (IsSalesTab)
            {
                if (_gridSales.CurrentRow?.DataBoundItem is not SaleRowVm sale) return;

                var receipt = _svc.GetReceipt(sale.Id);
                using var form = new ReceiptPreviewForm(receipt, autoPreview: autoPreview);
                form.ShowDialog(this);
            }
            else
            {
                if (_gridReturns.CurrentRow?.DataBoundItem is not ReturnRowVm ret) return;

                var entity = _svc.GetReturn(ret.Id);
                using var form = new ReceiptPreviewForm(entity, autoPreview: autoPreview);
                form.ShowDialog(this);
            }
        }

        private static string ToPaymentText(PaymentMethod m) => m switch
        {
            PaymentMethod.Cash => "現金",
            PaymentMethod.Card => "信用卡",
            PaymentMethod.Mobile => "行動支付",
            _ => m.ToString()
        };

        // ===== VMs =====

        private sealed class SaleRowVm
        {
            public int Id { get; }
            public DateTime CreatedAt { get; }
            public string SaleNo { get; }
            public string PaymentMethodText { get; }
            public decimal Total { get; }
            public decimal PaidAmount { get; }
            public decimal ChangeAmount { get; }

            public SaleRowVm(SaleRow row)
            {
                Id = row.Id;
                CreatedAt = row.CreatedAt.ToLocalTime();
                SaleNo = row.SaleNo;
                PaymentMethodText = ToPaymentText(row.PaymentMethod);
                Total = row.Total;
                PaidAmount = row.PaidAmount;
                ChangeAmount = row.ChangeAmount;
            }
        }

        private sealed class ReturnRowVm
        {
            public int Id { get; }
            public DateTime CreatedAt { get; }
            public string ReturnNo { get; }
            public string OriginalSaleNo { get; }
            public decimal TotalRefund { get; }
            public string ClerkName { get; }
            public string MemberCode { get; }

            public ReturnRowVm(Return r)
            {
                Id = r.Id;
                CreatedAt = r.CreatedAt.ToLocalTime();
                ReturnNo = r.ReturnNo ?? "";
                OriginalSaleNo = r.Sale?.SaleNo ?? "";   // 退貨對應原交易單號
                TotalRefund = r.TotalRefund;
                ClerkName = r.ClerkName ?? "";
                MemberCode = r.MemberCode ?? "";
            }
        }

        private sealed class LineItemVm
        {
            public string Barcode { get; }
            public string Name { get; }
            public decimal UnitPrice { get; }
            public decimal Quantity { get; }
            public decimal LineTotal { get; }

            public LineItemVm(string? barcode, string? name, decimal unitPrice, decimal quantity, decimal lineTotal)
            {
                Barcode = barcode ?? "";
                Name = name ?? "";
                UnitPrice = unitPrice;
                Quantity = quantity;
                LineTotal = lineTotal;
            }


        }

    }
}
