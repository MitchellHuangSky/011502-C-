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
        // 設計模式時會 return，這些欄位會沒初始化，所以用 null! 安全消除 CS8618
        private PosDbContext _dbContext = null!;
        private SaleService _saleService = null!;
        private ProductService _productService = null!;
        private InventoryService _inventoryService = null!;
        private CheckoutService _checkoutService = null!;
        private SaleQueryService _saleQueryService = null!;
        private ReturnService _returnService = null!;
        private ReturnQueryService _returnQueryService = null!;

        private readonly BindingList<CartItemViewModel> _cartItems = new();


        //private readonly PosDbContext _dbContext;
        //private readonly SaleService _saleService;
        //private readonly ProductService _productService;
        //private readonly InventoryService _inventoryService;
        //private readonly CheckoutService _checkoutService;

        //private readonly SaleQueryService _saleQueryService;
        //private readonly ReturnService _returnService;
        //private readonly ReturnQueryService _returnQueryService;



        //private readonly BindingList<CartItemViewModel> _cartItems =
        //    new BindingList<CartItemViewModel>();


        private Button _btnPickProducts = null!;
        private Button _btnOrderLookup = null!;
        private Button _btnReturn = null!;
        private Button _btnHelp = null!;

        private Button _btnReturnDrop = null!;
        private ContextMenuStrip _returnMenu = null!;

        //private ContextMenuStrip _helpMenu = null!;
        //private ContextMenuStrip _productMenu = null!;
        //private ContextMenuStrip _inventoryMenu = null!;
        //private ContextMenuStrip _checkoutMenu = null!;
        //private ContextMenuStrip _saleMenu = null!;
        //private ContextMenuStrip _reportMenu = null!;



        public MainForm()
        {
            InitializeComponent();
            // ✅ 超商風格（設計/執行都能跑，不碰 DB）
            ApplyConvenienceStoreTheme();

            // ✅ 設計工具打開畫面時，不跑 DB 初始化（避免 Designer 報錯）
            if (IsDesignTime()) return;

            // ✅ 只有正式執行才跑 DB / Service 初始化
            InitRuntime();
        }


        // =========================================================
        // ✅ DesignTime 判斷（constructor 內 DesignMode 不可靠）
        // =========================================================
        private static bool IsDesignTime()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        }

        // =========================================================
        // ✅ 強制事件只綁一次（修 btnRemoveSelected_Click 觸發兩次）
        // =========================================================
        private static void HookClickOnce(Button btn, EventHandler handler)
        {
            // 不管被綁幾次，都先清掉
            for (int i = 0; i < 8; i++)
                btn.Click -= handler;

            btn.Click += handler;
        }

        private void InitRuntime()
        {
            //  事件統一在這裡綁（Designer 綁到也沒關係，我們會先清掉再綁一次）
            HookClickOnce(btnClear, btnClear_Click);
            HookClickOnce(btnCash, btnCash_Click);
            HookClickOnce(btnCard, btnCard_Click);
            HookClickOnce(btnMobile, btnMobile_Click);
            HookClickOnce(btnRemoveSelected, btnRemoveSelected_Click);

            gridCart.CellEndEdit -= GridCart_CellEndEdit;
            gridCart.CellEndEdit += GridCart_CellEndEdit;

            gridCart.KeyDown -= gridCart_KeyDown;
            gridCart.KeyDown += gridCart_KeyDown;

            // ✅ DB 初始化
            _dbContext = new PosDbContext();

            try
            {
                _dbContext.Database.Migrate();
            }
            catch
            {
                _dbContext.Database.EnsureCreated();
            }

            // 最終保險：缺欄位就補上（你有這支就保留）
            SchemaRepair.EnsureProductsHasImagePath(_dbContext);

            _saleService = new SaleService(_dbContext);
            _productService = new ProductService(_dbContext);

            _inventoryService = new InventoryService(_dbContext);
            _checkoutService = new CheckoutService(_dbContext, _inventoryService);

            _saleQueryService = new SaleQueryService(_dbContext);
            _returnService = new ReturnService(_dbContext, _inventoryService);
            _returnQueryService = new ReturnQueryService(_dbContext);

            InitializeCartGrid();
            AddFeatureButtons();

            UpdateTotal();
            txtBarcode.Focus();
        }





        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (!IsDesignTime())
                _dbContext?.Dispose();

            base.OnFormClosed(e);
        }

        // =========================================================
        // ✅ 超商版：右側快捷按鈕區（商品 / 查訂單 / 退貨(下拉) / 流程）
        // =========================================================
        private void AddFeatureButtons()
        {
            flpQuick.Controls.Clear();

            int w = 220;
            int h = 52;

            Button MakeQuick(string text)
            {
                var b = new Button
                {
                    Text = text,
                    Width = w,
                    Height = h,
                    Font = new Font("Microsoft JhengHei UI", 12.5f, FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat,
                    Margin = new Padding(0, 0, 0, 12),
                    BackColor = Color.White
                };
                b.FlatAppearance.BorderSize = 1;
                return b;
            }

            _btnPickProducts = MakeQuick("商品");
            _btnOrderLookup = MakeQuick("查訂單");
            _btnHelp = MakeQuick("流程");

            // ✅ 退貨：主按鈕 + 下拉
            int dropW = 40;
            var pnlReturn = new Panel { Width = w, Height = h, Margin = new Padding(0, 0, 0, 12) };

            _btnReturn = new Button
            {
                Left = 0,
                Top = 0,
                Width = w - dropW,
                Height = h,
                Text = "退貨",
                Font = new Font("Microsoft JhengHei UI", 12.5f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White
            };
            _btnReturn.FlatAppearance.BorderSize = 1;

            _btnReturnDrop = new Button
            {
                Left = w - dropW,
                Top = 0,
                Width = dropW,
                Height = h,
                Text = "▾",
                Font = new Font("Microsoft JhengHei UI", 12.5f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White
            };
            _btnReturnDrop.FlatAppearance.BorderSize = 1;

            pnlReturn.Controls.Add(_btnReturn);
            pnlReturn.Controls.Add(_btnReturnDrop);

            // 商品：連續點選加入（不關閉）
            _btnPickProducts.Click += (_, _) =>
            {
                using var form = new ProductsForm(_productService, p => AddProductToCart(p));
                form.CloseOnPick = false;

                // ✅ 視窗位置最佳化：靠右、靠近主視窗、不超出螢幕
                form.StartPosition = FormStartPosition.Manual;
                var wa = Screen.FromControl(this).WorkingArea;

                int x = Math.Min(wa.Right - form.Width - 10, this.Right - form.Width - 10);
                int y = Math.Max(wa.Top + 20, this.Top + 60);

                form.Location = new Point(Math.Max(wa.Left + 10, x), y);

                form.ShowDialog(this);
                txtBarcode.Focus();
            };

            // 查訂單：預設今天 + 可查退貨
            _btnOrderLookup.Click += (_, _) =>
            {
                using var form = new OrderLookupForm(_saleQueryService, _returnQueryService);
                form.ShowDialog(this);
                txtBarcode.Focus();
            };

            // 退貨預設：今日退單（打開退貨 tab）
            void OpenTodayReturns()
            {
                using var form = new OrderLookupForm(_saleQueryService, _returnQueryService, defaultToReturns: true);
                form.ShowDialog(this);
                txtBarcode.Focus();
            }

            // 新增退貨
            void OpenCreateReturn()
            {
                using var form = new ReturnForm(_returnService, _returnQueryService);
                form.ShowDialog(this);
                txtBarcode.Focus();
            }

            _btnReturn.Click += (_, _) => OpenTodayReturns();

            _returnMenu = new ContextMenuStrip();
            _returnMenu.Items.Add("今日退單", null, (_, _) => OpenTodayReturns());
            _returnMenu.Items.Add("新增退貨（輸入原單號）", null, (_, _) => OpenCreateReturn());

            _btnReturnDrop.Click += (_, _) =>
            {
                _returnMenu.Show(_btnReturnDrop, 0, _btnReturnDrop.Height);
            };

            _btnHelp.Click += (_, _) =>
            {
                using var form = new PosHelpForm();
                form.ShowDialog(this);
                txtBarcode.Focus();
            };

            flpQuick.Controls.Add(_btnPickProducts);
            flpQuick.Controls.Add(_btnOrderLookup);
            flpQuick.Controls.Add(pnlReturn);
            flpQuick.Controls.Add(_btnHelp);
        }



        // ====== 付款按鈕（結帳）======
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



        // ✅ 只刪「目前那筆」（不管你選幾筆，都只刪 CurrentRow）
        private void btnRemoveSelected_Click(object? sender, EventArgs e)
        {
            RemoveCurrentItemOnly();
            txtBarcode.Focus();
        }

        private void gridCart_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                RemoveCurrentItemOnly();
                e.Handled = true;
            }
        }

        private void RemoveCurrentItemOnly()
        {
            if (gridCart.CurrentRow?.DataBoundItem is not CartItemViewModel item) return;

            int idx = gridCart.CurrentRow.Index;

            _cartItems.Remove(item);
            gridCart.Refresh();
            UpdateTotal();

            // ✅ 刪完自動選下一筆（方便連續刪）
            if (gridCart.Rows.Count > 0)
            {
                int next = Math.Min(idx, gridCart.Rows.Count - 1);
                gridCart.CurrentCell = gridCart.Rows[next].Cells[0];

                gridCart.ClearSelection();
                gridCart.Rows[next].Selected = true;
            }
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







        private void RemoveSelectedItems()
        {
            var targets = gridCart.SelectedRows
                .Cast<DataGridViewRow>()
                .Select(r => r.DataBoundItem as CartItemViewModel)
                .Where(x => x != null)
                .ToList();

            // 沒有 SelectedRows 就用 CurrentRow
            if (targets.Count == 0 && gridCart.CurrentRow?.DataBoundItem is CartItemViewModel one)
                targets.Add(one);

            if (targets.Count == 0) return;

            foreach (var it in targets)
                _cartItems.Remove(it!);

            gridCart.Refresh();
            UpdateTotal();

            // ✅ 保持選取（選下一筆）
            if (gridCart.Rows.Count > 0)
                gridCart.CurrentCell = gridCart.Rows[Math.Min(0, gridCart.Rows.Count - 1)].Cells[0];
        }

        private void RemoveSelectedItem()
        {
            if (gridCart.CurrentRow?.DataBoundItem is not CartItemViewModel item) return;
            _cartItems.Remove(item);
            gridCart.Refresh();
            UpdateTotal();
        }
        // =========================================================
        // ✅ 超商主題（不要寫在 Designer.cs，Designer 會報錯）
        // =========================================================
        private void ApplyConvenienceStoreTheme()
        {
            Text = "POSv01 超商收銀台";
            Font = new Font("Microsoft JhengHei UI", 10f);
            BackColor = Color.WhiteSmoke;

            // 條碼輸入更像 POS
            txtBarcode.Font = new Font("Microsoft JhengHei UI", 14f, FontStyle.Bold);

            // Grid
            gridCart.BackgroundColor = Color.White;
            gridCart.BorderStyle = BorderStyle.FixedSingle;
            gridCart.RowHeadersVisible = false;
            gridCart.EnableHeadersVisualStyles = false;
            gridCart.ColumnHeadersDefaultCellStyle.BackColor = Color.Gainsboro;
            gridCart.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft JhengHei UI", 10f, FontStyle.Bold);
            gridCart.DefaultCellStyle.Font = new Font("Microsoft JhengHei UI", 10f, FontStyle.Regular);
            gridCart.DefaultCellStyle.SelectionBackColor = Color.LightGoldenrodYellow;
            gridCart.DefaultCellStyle.SelectionForeColor = Color.Black;

            // 總金額加大
            lblTotal.Font = new Font("Microsoft JhengHei UI", 22f, FontStyle.Bold);
            lblTotal.ForeColor = Color.DarkRed;

            lblTotalTitle.Font = new Font("Microsoft JhengHei UI", 12f, FontStyle.Bold);

            // 付款按鈕大顆
            StylePayButton(btnCash, "現金結帳");
            StylePayButton(btnCard, "信用卡");
            StylePayButton(btnMobile, "行動支付");

            // 清空 / 移除
            StyleSmallButton(btnClear, "清空");
            StyleSmallButton(btnRemoveSelected, "移除一筆");
        }

        private void StylePayButton(Button b, string text)
        {
            b.Text = text;
            b.Width = 240;
            b.Height = 62;
            b.Font = new Font("Microsoft JhengHei UI", 14f, FontStyle.Bold);
            b.FlatStyle = FlatStyle.Flat;
            b.BackColor = Color.White;
            b.FlatAppearance.BorderSize = 1;
        }

        private void StyleSmallButton(Button b, string text)
        {
            b.Text = text;
            b.Height = 34;
            b.Font = new Font("Microsoft JhengHei UI", 10.5f, FontStyle.Bold);
            b.FlatStyle = FlatStyle.Flat;
            b.BackColor = Color.White;
            b.FlatAppearance.BorderSize = 1;
        }




        private void btnHistory_Click(object sender, EventArgs e)
        {
            using var form = new SalesHistoryForm(new SaleQueryService(_dbContext));
            form.ShowDialog(this);
            txtBarcode.Focus();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

    }
}
