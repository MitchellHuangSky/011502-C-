using Microsoft.EntityFrameworkCore;
using POSv01.Domain.Entities;
using POSv01.Infrastructure;
using POSv01.Services;
using POSv01.UI;
using POSv01.ViewModels;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace POSv01
{
    public partial class MainForm : Form
    {
        private readonly PosDbContext _dbContext;
        private readonly SaleService _saleService;
        private readonly ProductService _productService;
        private readonly InventoryService _inventoryService;
        private readonly CheckoutService _checkoutService;

        private readonly SaleQueryService _saleQueryService;
        private readonly ReturnService _returnService;
        private readonly ReturnQueryService _returnQueryService;



        private readonly BindingList<CartItemViewModel> _cartItems =
            new BindingList<CartItemViewModel>();

        private Button _btnPickProducts = null!;
        private Button _btnOrderLookup = null!;
        private Button _btnReturn = null!;
        private Button _btnHelp = null!;



        public MainForm()
        {
            InitializeComponent();

            btnClear.Click += btnClear_Click;
            btnCash.Click += btnCash_Click;
            btnCard.Click += btnCard_Click;
            btnMobile.Click += btnMobile_Click;
            btnRemoveSelected.Click += btnRemoveSelected_Click;

            gridCart.CellEndEdit += GridCart_CellEndEdit;
            gridCart.KeyDown += gridCart_KeyDown;

            _dbContext = new PosDbContext();

            // 1) migrations 套用（如果你沒有 migrations，會進 catch 改用 EnsureCreated）
            try
            {
                _dbContext.Database.Migrate();
            }
            catch
            {
                _dbContext.Database.EnsureCreated();
            }

            // 2) 最終保險：缺欄位就補上（避免 migrations 狀態亂掉時還能跑）
            //    若你沒有 SchemaRepair 這支類別，就先把這行註解掉。
            SchemaRepair.EnsureProductsHasImagePath(_dbContext);

            _saleService = new SaleService(_dbContext);
            _productService = new ProductService(_dbContext);

            _inventoryService = new InventoryService(_dbContext);
            _checkoutService = new CheckoutService(_dbContext, _inventoryService);

            _dbContext.Database.EnsureCreated();

           
            _saleQueryService = new SaleQueryService(_dbContext);
            _returnService = new ReturnService(_dbContext, _inventoryService);
            _returnQueryService = new ReturnQueryService(_dbContext);


            btnClear.Click += btnClear_Click;
            btnCash.Click += btnCash_Click;
            btnCard.Click += btnCard_Click;
            btnMobile.Click += btnMobile_Click;
            btnRemoveSelected.Click += btnRemoveSelected_Click;

            gridCart.CellEndEdit += GridCart_CellEndEdit;
            gridCart.KeyDown += gridCart_KeyDown;

            InitializeCartGrid();
            AddFeatureButtons();
            UpdateTotal();
            txtBarcode.Focus();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _dbContext.Dispose();
            base.OnFormClosed(e);
        }
        // ====== POS 流程功能按鈕（動態加入） ======
        private void AddFeatureButtons()
        {
            // 右側空位加四顆按鈕
            var x = 700;
            var y = 40;
            var w = 90;
            var h = 28;
            var gap = 8;

            _btnPickProducts = new Button { Left = x, Top = y, Width = w, Height = h, Text = "商品" };
            _btnOrderLookup = new Button { Left = x, Top = y + (h + gap) * 1, Width = w, Height = h, Text = "查訂單" };
            _btnReturn = new Button { Left = x, Top = y + (h + gap) * 2, Width = w, Height = h, Text = "退貨" };
            _btnHelp = new Button { Left = x, Top = y + (h + gap) * 3, Width = w, Height = h, Text = "流程" };

            _btnPickProducts.Click += (_, _) =>
            {
                using var form = new ProductsForm(_productService, p => AddProductToCart(p));
                form.CloseOnPick = true;
                form.ShowDialog(this);
                txtBarcode.Focus();
            };

            _btnOrderLookup.Click += (_, _) =>
            {
                using var form = new OrderLookupForm(_saleQueryService);
                form.ShowDialog(this);
                txtBarcode.Focus();
            };

            _btnReturn.Click += (_, _) =>
            {
                using var form = new ReturnForm(_returnService, _returnQueryService);
                form.ShowDialog(this);
                txtBarcode.Focus();
            };

            _btnHelp.Click += (_, _) =>
            {
                using var form = new PosHelpForm();
                form.ShowDialog(this);
                txtBarcode.Focus();
            };

            Controls.Add(_btnPickProducts);
            Controls.Add(_btnOrderLookup);
            Controls.Add(_btnReturn);
            Controls.Add(_btnHelp);
        }




        // ====== 付款按鈕（Designer 綁事件到這三個） ======
        private void btnCash_Click(object? sender, EventArgs e) => Checkout(PaymentMethod.Cash);
        private void btnCard_Click(object? sender, EventArgs e) => Checkout(PaymentMethod.Card);
        private void btnMobile_Click(object? sender, EventArgs e) => Checkout(PaymentMethod.Mobile);

        private void Checkout(PaymentMethod method)
        {
            if (_cartItems.Count == 0)
            {
                MessageBox.Show("購物車是空的。", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var total = _cartItems.Sum(i => i.LineTotal);
            decimal paidAmount = total;

            if (method == PaymentMethod.Cash)
            {
                using var dlg = new CashPaymentDialog(total);
                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return;

                paidAmount = dlg.PaidAmount;
            }
            else
            {
                // 卡/行動支付：先用「模擬完成」
                paidAmount = total;
            }

            try
            {
                // ✅ 注意：CheckoutService 請回傳 Sale（Entity）
                var sale = _checkoutService.Checkout(_cartItems.ToList(), method, paidAmount);


                // ✅ 顯示收據（用 SaleQueryService 組 ReceiptDto）
                var receipt = _saleQueryService.GetReceipt(sale.Id);
                using (var preview = new ReceiptPreviewForm(receipt))
                {
                    preview.ShowDialog(this);
                }


                // ✅ 成功：清空購物車
                _cartItems.Clear();
                gridCart.Refresh();
                UpdateTotal();
                txtBarcode.Focus();

                MessageBox.Show(
                    $"交易完成\n單號：{sale.SaleNo}\n付款：{sale.PaymentMethod}\n總額：{sale.Total:0.##}\n找零：{sale.ChangeAmount:0.##}",
                    "完成",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "結帳失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }






        // ====== 購物車操作 ======
        private void btnClear_Click(object? sender, EventArgs e)
        {
            _cartItems.Clear();
            gridCart.Refresh();
            UpdateTotal();
            txtBarcode.Focus();
        }

        private void btnRemoveSelected_Click(object? sender, EventArgs e)
        {
            RemoveSelectedItem();
            txtBarcode.Focus();
        }

        private void gridCart_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                RemoveSelectedItem();
                e.Handled = true;
            }
        }

        private void RemoveSelectedItem()
        {
            if (gridCart.CurrentRow?.DataBoundItem is not CartItemViewModel item) return;
            _cartItems.Remove(item);
            gridCart.Refresh();
            UpdateTotal();
        }

        private void AddProductToCart(Product product)
        {
            var existing = _cartItems.FirstOrDefault(i => i.Product.Id == product.Id);
            if (existing != null) existing.Quantity += 1;
            else _cartItems.Add(new CartItemViewModel(product));

            gridCart.Refresh();
            UpdateTotal();
        }

        private void AddProductToCart(string barcode)
        {
            var product = _saleService.FindProductByBarcode(barcode);
            if (product == null)
            {
                MessageBox.Show($"找不到商品，條碼：{barcode}", "查無商品",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            AddProductToCart(product);
        }

        private void InitializeCartGrid()
        {
            gridCart.AutoGenerateColumns = false;
            gridCart.AllowUserToAddRows = false;
            gridCart.MultiSelect = false;
            gridCart.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            gridCart.DataSource = _cartItems;
            gridCart.Columns.Clear();

            gridCart.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Barcode", HeaderText = "條碼", ReadOnly = true, Width = 140 });
            gridCart.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Name", HeaderText = "商品名稱", ReadOnly = true, Width = 280 });
            gridCart.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "UnitPrice", HeaderText = "單價", ReadOnly = true, Width = 90, DefaultCellStyle = { Format = "0.##" } });
            gridCart.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Quantity", HeaderText = "數量", ReadOnly = false, Width = 90, DefaultCellStyle = { Format = "0.##" } });
            gridCart.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "LineTotal", HeaderText = "小計", ReadOnly = true, Width = 90, DefaultCellStyle = { Format = "0.##" } });
        }

        private void UpdateTotal()
        {
            var total = _cartItems.Sum(i => i.LineTotal);
            lblTotal.Text = total.ToString("0.##");
        }

        private void txtBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;

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
            if (e.RowIndex < 0) return;
            if (gridCart.Columns[e.ColumnIndex].DataPropertyName != "Quantity") return;

            var row = gridCart.Rows[e.RowIndex];
            if (row.DataBoundItem is not CartItemViewModel item) return;

            var cellValue = row.Cells[e.ColumnIndex].Value?.ToString() ?? "0";
            if (!decimal.TryParse(cellValue, out var qty) || qty < 0) qty = 0;

            if (qty == 0) _cartItems.Remove(item);
            else item.Quantity = qty;

            gridCart.Refresh();
            UpdateTotal();
        }














        private void btnPickProducts_Click(object sender, EventArgs e)
        {
            using var form = new ProductsForm(
                _productService,
                product => AddProductToCart(product) // 點照片卡片就加到購物車
            );

            form.ShowDialog(this);
            txtBarcode.Focus();
        }



        //private void AddProductToCart(string barcode)
        //{
        //    var product = _saleService.FindProductByBarcode(barcode);
        //    if (product == null)
        //    {
        //        MessageBox.Show($"找不到商品，條碼：{barcode}", "查無商品",
        //            MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //        return;
        //    }

        //    AddProductToCart(product);
        //}



      


      
        
        

        private void btnHistory_Click(object sender, EventArgs e)
        {
            using var form = new SalesHistoryForm(new SaleQueryService(_dbContext));
            form.ShowDialog(this);
            txtBarcode.Focus();
        }





      
        // btnCash / btnCard / btnMobile 之後會接 Checkout 邏輯。

    }
}
