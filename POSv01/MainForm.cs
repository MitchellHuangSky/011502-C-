using POSv01.Domain.Entities;
using POSv01.Infrastructure;
using POSv01.Services;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using POSv01.ViewModels;

namespace POSv01
{
    public partial class MainForm : Form
    {
        private readonly PosDbContext _dbContext;
        private readonly SaleService _saleService;

        private readonly BindingList<CartItemViewModel> _cartItems =
            new BindingList<CartItemViewModel>();

       
  
        public MainForm()
        {
            InitializeComponent();

            _dbContext = new PosDbContext();
            _saleService = new SaleService(_dbContext);
            InitializeCartGrid();
            UpdateTotal();
            txtBarcode.Focus();
        }


        private void InitializeCartGrid()
        {
            gridCart.AutoGenerateColumns = false;
            gridCart.AllowUserToAddRows = false;
            gridCart.MultiSelect = false;
            gridCart.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            gridCart.DataSource = _cartItems;
            gridCart.Columns.Clear();

            gridCart.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Barcode",
                HeaderText = "條碼",
                ReadOnly = true,
                Width = 140
            });
            gridCart.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Name",
                HeaderText = "商品名稱",
                ReadOnly = true,
                Width = 280
            });
            gridCart.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "UnitPrice",
                HeaderText = "單價",
                ReadOnly = true,
                Width = 90,
                DefaultCellStyle = { Format = "0.##" }
            });
            gridCart.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = "數量",
                ReadOnly = false,
                Width = 90,
                DefaultCellStyle = { Format = "0.##" }
            });
            gridCart.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LineTotal",
                HeaderText = "小計",
                ReadOnly = true,
                Width = 90,
                DefaultCellStyle = { Format = "0.##" }
            });
        }


        private void UpdateTotal()
        {
            var total = _cartItems.Sum(i => i.LineTotal);
            lblTotal.Text = total.ToString("0.##");
        }

        private void AddProductToCart(string barcode)
        {
            var product = _saleService.FindProductByBarcode(barcode);
            if (product == null)
            {
                MessageBox.Show($"找不到商品，條碼：{barcode}",
                    "查無商品",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var existing = _cartItems.FirstOrDefault(i => i.Product.Id == product.Id);
            if (existing != null)
            {
                existing.Quantity += 1;
            }
            else
            {
                _cartItems.Add(new CartItemViewModel(product));
            }

            gridCart.Refresh();
            UpdateTotal();
        }




        private void txtBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            var barcode = txtBarcode.Text.Trim();
            if (!string.IsNullOrWhiteSpace(barcode))
            {
                AddProductToCart(barcode);
                txtBarcode.Clear();
            }

            txtBarcode.Focus();

            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        private void GridCart_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            if (gridCart.Columns[e.ColumnIndex].DataPropertyName != "Quantity")
            {
                return;
            }

            var row = gridCart.Rows[e.RowIndex];
            if (row.DataBoundItem is not CartItemViewModel item)
            {
                return;
            }

            var cellValue = row.Cells[e.ColumnIndex].Value?.ToString() ?? "0";

            if (!decimal.TryParse(cellValue, out var qty) || qty < 0)
            {
                qty = 0;
            }

            if (qty == 0)
            {
                _cartItems.Remove(item);
            }
            else
            {
                item.Quantity = qty;
            }


            gridCart.Refresh();
            UpdateTotal();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            _cartItems.Clear();
            UpdateTotal();
        }

        // btnCash / btnCard / btnMobile 之後會接 Checkout 邏輯。

    }
}
