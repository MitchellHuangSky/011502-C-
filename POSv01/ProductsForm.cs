using POSv01.Domain.Entities;
using POSv01.Infrastructure;
using POSv01.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;



namespace POSv01
{
    /// <summary>
    /// 商品照片選取視窗（Designer 已拉 UI）
    /// - 可滾動顯示商品卡
    /// - 搜尋（條碼/名稱）
    /// - 新增商品（含照片）
    /// - 點商品卡 → 回呼 MainForm 加入購物車
    /// </summary>
    public partial class ProductsForm : Form
    {
        private ProductService? _productService;
        private Action<Product>? _onPick;

        // ✅ 讓 Designer 忽略此屬性（避免 WF1000）
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CloseOnPick { get; set; } = false;

        // === 你 Designer 控制項的 Name 若不同，就改這裡 ===
        private const string SearchTextBoxName = "txtSearch";
        private const string SearchButtonName = "btnSearch";
        private const string AddButtonName = "btnAdd";
        private const string FlowLayoutName = "flpProducts";
        private const string PanelName = "pnlProducts";

        private TextBox _txtSearch = null!;
        private Button _btnSearch = null!;
        private Button _btnAdd = null!;
        private Control _container = null!;

        private readonly List<Panel> _cards = new();

        // ✅ Designer 需要 parameterless ctor
        public ProductsForm()
        {
            InitializeComponent();
        }

        // ✅ 正常開啟用這個 ctor
        public ProductsForm(ProductService productService, Action<Product> onPick) : this()
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _onPick = onPick ?? throw new ArgumentNullException(nameof(onPick));

            BindControlsByName();
            WireEvents();
            ReloadProducts();
        }

        private void EnsureInjected()
        {
            if (_productService is null || _onPick is null)
            {
                throw new InvalidOperationException("ProductsForm 尚未注入服務。請用帶參數的建構子 ProductsForm(ProductService, Action<Product>) 開啟。");
            }
        }

        private void BindControlsByName()
        {
            _txtSearch = FindRequired<TextBox>(SearchTextBoxName);
            _btnSearch = FindRequired<Button>(SearchButtonName);
            _btnAdd = FindRequired<Button>(AddButtonName);

            var flp = FindOptional<FlowLayoutPanel>(FlowLayoutName);
            if (flp != null)
            {
                flp.AutoScroll = true;
                flp.WrapContents = true;
                flp.FlowDirection = FlowDirection.LeftToRight;
                _container = flp;
                return;
            }

            var pnl = FindOptional<Panel>(PanelName);
            if (pnl == null)
            {
                throw new InvalidOperationException(
                    $"ProductsForm 找不到容器：請在 Designer 放 FlowLayoutPanel(Name={FlowLayoutName}) 或 Panel(Name={PanelName}, AutoScroll=true)。");
            }

            pnl.AutoScroll = true;
            _container = pnl;
        }

        private void WireEvents()
        {
            _btnSearch.Click += (_, _) => ReloadProducts();

            _txtSearch.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    ReloadProducts();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            };

            _btnAdd.Click += (_, _) =>
            {
                EnsureInjected();
                using var dlg = new AddProductDialogLite(_productService!);
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    ReloadProducts();
                }
            };
        }

        private void ReloadProducts()
        {
            EnsureInjected();

            var products = _productService!.GetActiveProducts(_txtSearch.Text.Trim());

            _cards.Clear();
            _container.SuspendLayout();
            _container.Controls.Clear();

            foreach (var p in products)
            {
                var card = CreateProductCard(p);
                _cards.Add(card);
                _container.Controls.Add(card);
            }

            _container.ResumeLayout();
        }

        private Panel CreateProductCard(Product p)
        {
            var card = new Panel
            {
                Width = 170,
                Height = 220,
                Margin = new Padding(8),
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.Hand
            };

            var pic = new PictureBox
            {
                Left = 8,
                Top = 8,
                Width = 170 - 16,
                Height = 120,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White
            };

            var name = new Label
            {
                Left = 8,
                Top = 136,
                Width = 170 - 16,
                Height = 40,
                Text = p.Name,
                AutoEllipsis = true
            };

            var price = new Label
            {
                Left = 8,
                Top = 180,
                Width = 170 - 16,
                Height = 24,
                Text = $"$ {p.UnitPrice:0.##}"
            };

            SetImage(pic, p.ImagePath);

            void Pick()
            {
                EnsureInjected();
                _onPick!.Invoke(p);

                if (CloseOnPick)
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }

            card.Click += (_, _) => Pick();
            pic.Click += (_, _) => Pick();
            name.Click += (_, _) => Pick();
            price.Click += (_, _) => Pick();

            card.Controls.Add(pic);
            card.Controls.Add(name);
            card.Controls.Add(price);

            return card;
        }

        private static void SetImage(PictureBox pic, string? imagePath)
        {
            pic.Image = null;
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return;
            }

            // ✅ 圖片路徑以 DB 根目錄為基準（LocalAppData\POSv01）
            var dbDir = Path.GetDirectoryName(DbPathProvider.GetDbPath()) ?? AppContext.BaseDirectory;

            var fullPath = Path.IsPathRooted(imagePath)
                ? imagePath
                : Path.Combine(dbDir, imagePath);

            if (!File.Exists(fullPath))
            {
                return;
            }

            using var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var tmp = Image.FromStream(fs);
            pic.Image = new Bitmap(tmp);
        }

        private T FindRequired<T>(string name) where T : Control
        {
            var c = FindOptional<T>(name);
            if (c == null)
            {
                throw new InvalidOperationException($"找不到控制項 Name={name}（{typeof(T).Name}）。請在 Designer 設定 Name。");
            }

            return c;
        }

        private T? FindOptional<T>(string name) where T : Control
        {
            return Controls.Find(name, true).FirstOrDefault() as T;
        }

        // ✅ 內建新增商品（不用 Designer）
        private sealed class AddProductDialogLite : Form
        {
            private readonly ProductService _productService;

            private readonly TextBox _txtBarcode = new() { Left = 120, Top = 16, Width = 340 };
            private readonly TextBox _txtName = new() { Left = 120, Top = 51, Width = 340 };
            private readonly TextBox _txtPrice = new() { Left = 120, Top = 86, Width = 160, Text = "0" };
            private readonly TextBox _txtImage = new() { Left = 120, Top = 121, Width = 260, ReadOnly = true };

            public AddProductDialogLite(ProductService productService)
            {
                _productService = productService;

                Text = "新增商品";
                Width = 520;
                Height = 260;
                StartPosition = FormStartPosition.CenterParent;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;

                Controls.Add(new Label { Left = 20, Top = 20, Width = 90, Text = "條碼" });
                Controls.Add(_txtBarcode);
                Controls.Add(new Label { Left = 20, Top = 55, Width = 90, Text = "名稱" });
                Controls.Add(_txtName);
                Controls.Add(new Label { Left = 20, Top = 90, Width = 90, Text = "單價" });
                Controls.Add(_txtPrice);
                Controls.Add(new Label { Left = 20, Top = 125, Width = 90, Text = "照片" });
                Controls.Add(_txtImage);

                var btnBrowse = new Button { Left = 390, Top = 119, Width = 70, Text = "選擇" };
                btnBrowse.Click += (_, _) =>
                {
                    using var ofd = new OpenFileDialog
                    {
                        Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All files|*.*"
                    };
                    if (ofd.ShowDialog(this) == DialogResult.OK)
                    {
                        _txtImage.Text = ofd.FileName;
                    }
                };
                Controls.Add(btnBrowse);

                var btnOk = new Button { Left = 280, Top = 165, Width = 85, Text = "確定" };
                btnOk.Click += (_, _) =>
                {
                    if (!decimal.TryParse(_txtPrice.Text.Trim(), out var price))
                    {
                        MessageBox.Show("單價格式錯誤", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    try
                    {
                        _productService.AddProduct(_txtBarcode.Text, _txtName.Text, price, _txtImage.Text);
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "新增失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                var btnCancel = new Button { Left = 375, Top = 165, Width = 85, Text = "取消" };
                btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

                Controls.Add(btnOk);
                Controls.Add(btnCancel);

                AcceptButton = btnOk;
                CancelButton = btnCancel;
            }
        }
    }

}