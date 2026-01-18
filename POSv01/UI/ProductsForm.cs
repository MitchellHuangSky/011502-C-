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



namespace POSv01.UI
{
    /// <summary>
    /// 商品照片選取視窗（Designer 已拉 UI）
    /// - 可滾動顯示商品卡
    /// - 搜尋（條碼/名稱）
    /// - 新增商品（含照片）
    /// - 點商品卡 → 回呼 MainForm 加入購物車
    /// 
  
    /// - 連續點選：點卡片就加 1
    /// - 批量加入：勾選 + 設定數量 + 加入選取
    /// - 搜尋（條碼/名稱）
    /// - 新增商品（含照片）
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

        // ✅ 新增：批量加入按鈕（如果 Designer 沒有，我會動態生成）
        private const string AddSelectedButtonName = "btnAddSelected";

        private TextBox _txtSearch = null!;
        private Button _btnSearch = null!;
        private Button _btnAdd = null!;
        private Button _btnAddSelected = null!;
        private Control _container = null!;


        private sealed class CardState
        {
            public Product Product = null!;
            public CheckBox Chk = null!;
            public NumericUpDown Qty = null!;
        }
        private readonly List<CardState> _cardStates = new();


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
            EnsureAddSelectedButton();
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
        private void EnsureAddSelectedButton()
        {
            // ✅ 如果 Designer 沒有 btnAddSelected，就動態補一顆
            _btnAddSelected = FindOptional<Button>(AddSelectedButtonName) ?? new Button
            {
                Name = AddSelectedButtonName,
                Text = "加入選取",
                Width = 90,
                Height = _btnAdd.Height
            };

            if (_btnAddSelected.Parent == null)
            {
                // 放在「新增商品」按鈕右邊
                _btnAddSelected.Left = _btnAdd.Right + 8;
                _btnAddSelected.Top = _btnAdd.Top;
                Controls.Add(_btnAddSelected);
                _btnAddSelected.BringToFront();
            }
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
            // ✅ 批量加入
            _btnAddSelected.Click += (_, _) => AddSelectedToCart();
        }
        private void AddSelectedToCart()
        {
            EnsureInjected();

            var selected = _cardStates.Where(x => x.Chk.Checked).ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show("請先勾選要加入的商品。", "加入選取", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            foreach (var s in selected)
            {
                var qty = (int)s.Qty.Value;
                if (qty <= 0) qty = 1;

                for (int i = 0; i < qty; i++)
                    _onPick!.Invoke(s.Product);
            }

            // ✅ 清除勾選（避免重複加）
            foreach (var s in selected)
            {
                s.Chk.Checked = false;
                s.Qty.Value = 1;
            }

            if (CloseOnPick)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
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
                //_cards.Add(card);
                _container.Controls.Add(card);
            }

            _container.ResumeLayout();
        }

        private Panel CreateProductCard(Product p)
        {
            var card = new Panel
            {
                Width = 180,
                Height = 240,
                Margin = new Padding(8),
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.Hand,
                BackColor = Color.White
            };

            var chk = new CheckBox
            {
                Left = 8,
                Top = 8,
                Width = 18,
                Height = 18
            };

            var qty = new NumericUpDown
            {
                Left = 120,
                Top = 6,
                Width = 50,
                Minimum = 1,
                Maximum = 999,
                Value = 1
            };

            var pic = new PictureBox
            {
                Left = 8,
                Top = 30,
                Width = 180 - 16,
                Height = 120,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White
            };

            var name = new Label
            {
                Left = 8,
                Top = 156,
                Width = 180 - 16,
                Height = 40,
                Text = p.Name,
                AutoEllipsis = true
            };

            var price = new Label
            {
                Left = 8,
                Top = 202,
                Width = 180 - 16,
                Height = 24,
                Text = $"$ {p.UnitPrice:0.##}",
                Font = new Font(Font.FontFamily, 10f, FontStyle.Bold)
            };

            SetImage(pic, p.ImagePath);

            // ✅ 連續點選：點卡片就直接加 1
            void PickOne()
            {
                EnsureInjected();
                _onPick!.Invoke(p);

                if (CloseOnPick)
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }

            // ✅ 避免點到 CheckBox / 數量時也觸發 Pick
            card.Click += (_, _) => PickOne();
            pic.Click += (_, _) => PickOne();
            name.Click += (_, _) => PickOne();
            price.Click += (_, _) => PickOne();

            card.Controls.Add(chk);
            card.Controls.Add(qty);
            card.Controls.Add(pic);
            card.Controls.Add(name);
            card.Controls.Add(price);

            _cardStates.Add(new CardState { Product = p, Chk = chk, Qty = qty });

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

        //private T? FindOptional<T>(string name) where T : Control
        //{
        //    return Controls.Find(name, true).FirstOrDefault() as T;
        //}

        private T? FindOptional<T>(string name) where T : Control
            => Controls.Find(name, true).FirstOrDefault() as T;


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