using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using POSv01.Services;

namespace POSv01
{
    public sealed class ReturnForm : Form
    {
        private readonly ReturnService _returnService;
        private readonly ReturnQueryService _returnQueryService;

        private readonly TextBox _txtSaleNo = new() { Left = 16, Top = 16, Width = 260, PlaceholderText = "輸入原交易單號" };
        private readonly Button _btnLoad = new() { Left = 284, Top = 14, Width = 80, Text = "查詢" };

        private readonly DataGridView _grid = new() { Left = 16, Top = 50, Width = 780, Height = 320, AutoGenerateColumns = false };
        private readonly Button _btnSubmit = new() { Left = 16, Top = 380, Width = 100, Text = "退貨" };
        private readonly Button _btnClose = new() { Left = 696, Top = 380, Width = 100, Text = "關閉" };

        private readonly BindingList<RowVm> _rows = new();
        private string _loadedSaleNo = string.Empty;

        public ReturnForm(ReturnService returnService, ReturnQueryService returnQueryService)
        {
            _returnService = returnService;
            _returnQueryService = returnQueryService;

            Text = "退貨";
            Width = 840;
            Height = 470;
            StartPosition = FormStartPosition.CenterParent;

            Controls.AddRange(new Control[] { _txtSaleNo, _btnLoad, _grid, _btnSubmit, _btnClose });

            InitGrid();

            _btnLoad.Click += (_, _) => LoadSale();
            _txtSaleNo.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    LoadSale();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            };

            _btnSubmit.Click += (_, _) => Submit();
            _btnClose.Click += (_, _) => Close();
        }

        private void InitGrid()
        {
            _grid.AllowUserToAddRows = false;
            _grid.MultiSelect = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            _grid.Columns.Clear();
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(RowVm.Barcode), HeaderText = "條碼", Width = 140, ReadOnly = true });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(RowVm.Name), HeaderText = "品名", Width = 260, ReadOnly = true });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(RowVm.UnitPrice), HeaderText = "單價", Width = 80, ReadOnly = true, DefaultCellStyle = { Format = "0.##" } });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(RowVm.BoughtQty), HeaderText = "原購買", Width = 80, ReadOnly = true, DefaultCellStyle = { Format = "0.##" } });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(RowVm.ReturnQty), HeaderText = "退貨數量", Width = 90, ReadOnly = false, DefaultCellStyle = { Format = "0.##" } });

            _grid.DataSource = _rows;
        }

        private void LoadSale()
        {
            var saleNo = _txtSaleNo.Text.Trim();
            var sale = _returnService.FindSaleBySaleNo(saleNo);

            _rows.Clear();
            _loadedSaleNo = string.Empty;

            if (sale == null)
            {
                MessageBox.Show("找不到交易。", "退貨", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _loadedSaleNo = sale.SaleNo;

            foreach (var it in sale.Items)
            {
                _rows.Add(new RowVm
                {
                    ProductId = it.ProductId,
                    Barcode = it.Barcode,
                    Name = it.Name,
                    UnitPrice = it.UnitPrice,
                    BoughtQty = it.Quantity,
                    ReturnQty = 0
                });
            }
        }

        private void Submit()
        {
            if (string.IsNullOrWhiteSpace(_loadedSaleNo))
            {
                MessageBox.Show("請先查詢交易。", "退貨", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var lines = new List<(int productId, decimal qty)>();
            foreach (var r in _rows)
            {
                if (r.ReturnQty <= 0) continue;

                if (r.ReturnQty > r.BoughtQty)
                {
                    MessageBox.Show($"退貨數量不可超過原購買：{r.Name}", "退貨", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                lines.Add((r.ProductId, r.ReturnQty));
            }

            if (lines.Count == 0)
            {
                MessageBox.Show("請輸入退貨數量。", "退貨", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var ret = _returnService.CreateReturn(_loadedSaleNo, lines);

                // ✅ 退貨單預覽 + 預覽列印/列印/另存/複製
                var dto = _returnQueryService.GetReturnReceipt(ret.Id);
                using var preview = new ReceiptPreviewForm(dto);
                preview.ShowDialog(this);

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "退貨失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private sealed class RowVm
        {
            public int ProductId { get; set; }
            public string Barcode { get; set; } = "";
            public string Name { get; set; } = "";
            public decimal UnitPrice { get; set; }
            public decimal BoughtQty { get; set; }
            public decimal ReturnQty { get; set; }
        }
    }
}
