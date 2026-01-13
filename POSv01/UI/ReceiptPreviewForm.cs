using System;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using POSv01.Domain.Entities;
using POSv01.Services;

// - 支援：銷售收據 ReceiptDto + 退貨單 ReturnReceiptDto
// - 功能：預覽列印 / 列印 / 另存TXT / 複製

namespace POSv01
{
    public sealed class ReceiptPreviewForm : Form
    {
        private readonly RichTextBox _rtb = new()
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            Font = new Font("Consolas", 10f),
            WordWrap = false
        };

        private readonly Button _btnPrintPreview = new() { Text = "預覽列印", Width = 100, Height = 30 };
        private readonly Button _btnPrint = new() { Text = "列印", Width = 80, Height = 30 };
        private readonly Button _btnSaveTxt = new() { Text = "另存TXT", Width = 90, Height = 30 };
        private readonly Button _btnCopy = new() { Text = "複製", Width = 80, Height = 30 };
        private readonly Button _btnClose = new() { Text = "關閉", Width = 80, Height = 30 };

        private readonly PrintDocument _printDoc = new();
        private string[] _printLines = Array.Empty<string>();
        private int _printLineIndex;

        private readonly string _defaultFileName;

        // ✅ 銷售收據
        public ReceiptPreviewForm(ReceiptDto saleReceipt, bool autoPreview = false)
    : this(
        title: "收據預覽",
        text: BuildSaleReceiptText(saleReceipt),
        defaultFileName: $"Receipt_{SafeName(saleReceipt.SaleNo)}_{saleReceipt.CreatedAtLocal:yyyyMMdd_HHmmss}.txt",
        autoPreview: autoPreview)
        { }


        // ✅ 退貨單
        public ReceiptPreviewForm(Return r, bool autoPreview = false)
            : this(
        title: "退貨單預覽",
        text: BuildReturnText(r),
        defaultFileName: $"Return_{SafeName(r.ReturnNo)}_{r.CreatedAt.ToLocalTime():yyyyMMdd_HHmmss}.txt",
        autoPreview: autoPreview)
        { }

        private static string BuildReturnText(Return r)
        {
            const int width = 32;
            static string Line(char c) => new string(c, width);
            static string Center(string s)
            {
                s ??= "";
                if (s.Length >= width) return s;
                var pad = (width - s.Length) / 2;
                return new string(' ', pad) + s;
            }

            var sb = new StringBuilder();
            sb.AppendLine(Center("POSv01 超商"));
            sb.AppendLine(Center("退貨單"));
            sb.AppendLine(Line('='));
            sb.AppendLine($"退貨單: {r.ReturnNo}");
            sb.AppendLine($"時間: {r.CreatedAt.ToLocalTime():yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"原單號: {r.Sale?.SaleNo ?? ""}");

            if (!string.IsNullOrWhiteSpace(r.MemberCode))
                sb.AppendLine($"會員: {r.MemberCode}");
            if (!string.IsNullOrWhiteSpace(r.ClerkName))
                sb.AppendLine($"店員: {r.ClerkName}");

            sb.AppendLine(Line('-'));

            foreach (var it in r.Items)
            {
                var name = (it.Name ?? "").Trim();
                if (name.Length > 16) name = name[..16];

                var qty = it.Quantity.ToString("0.##");
                var price = it.UnitPrice.ToString("0.##");
                var total = it.LineTotal.ToString("0.##");

                sb.AppendLine($"{name,-16}{qty,4}x{price,6}{total,6}");
            }

            sb.AppendLine(Line('-'));
            sb.AppendLine($"退款: {r.TotalRefund,24:0.##}");
            sb.AppendLine(Line('='));
            sb.AppendLine(Center("謝謝光臨"));
            return sb.ToString();
        }

        private readonly bool _autoPreview;




        // ✅ 退貨單
        public ReceiptPreviewForm(ReturnReceiptDto returnReceipt)
            : this(
                title: "退貨單預覽",
                text: BuildReturnReceiptText(returnReceipt),
                defaultFileName: $"Return_{SafeName(returnReceipt.ReturnNo)}_{returnReceipt.CreatedAtLocal:yyyyMMdd_HHmmss}.txt")
        { }

        // ✅ 通用
        public ReceiptPreviewForm(string title, string text, string defaultFileName, bool autoPreview = false)
        {
            _autoPreview = autoPreview;

            Text = title;
            Width = 580;
            Height = 780;
            StartPosition = FormStartPosition.CenterParent;

            _defaultFileName = string.IsNullOrWhiteSpace(defaultFileName) ? "receipt.txt" : defaultFileName;

            var top = new Panel { Dock = DockStyle.Top, Height = 52, Padding = new Padding(10) };

            _btnPrintPreview.Left = 10; _btnPrintPreview.Top = 10;
            _btnPrint.Left = 120; _btnPrint.Top = 10;
            _btnSaveTxt.Left = 210; _btnSaveTxt.Top = 10;
            _btnCopy.Left = 310; _btnCopy.Top = 10;
            _btnClose.Left = 400; _btnClose.Top = 10;

            top.Controls.AddRange(new Control[] { _btnPrintPreview, _btnPrint, _btnSaveTxt, _btnCopy, _btnClose });

            Controls.Add(_rtb);
            Controls.Add(top);

            _rtb.Text = text ?? string.Empty;

            _btnPrintPreview.Click += (_, _) => ShowPrintPreview();
            _btnPrint.Click += (_, _) => PrintReceipt();
            _btnSaveTxt.Click += (_, _) => SaveAsTxt();
            _btnCopy.Click += (_, _) => CopyToClipboard();
            _btnClose.Click += (_, _) => Close();

            _printDoc.PrintPage += PrintDoc_PrintPage;

            // ✅ 一開就跳預覽列印（重印用）
            Shown += (_, _) =>
            {
                if (_autoPreview)
                    BeginInvoke(new Action(() => ShowPrintPreview()));
            };
        }

        private string CurrentText => _rtb.Text ?? string.Empty;

        private void PreparePrint(string text)
        {
            _printLines = (text ?? string.Empty).Replace("\r\n", "\n").Split('\n');
            _printLineIndex = 0;
        }

        private void ShowPrintPreview()
        {
            try
            {
                PreparePrint(CurrentText);
                using var dlg = new PrintPreviewDialog
                {
                    Document = _printDoc,
                    Width = 1000,
                    Height = 800
                };
                dlg.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "預覽列印失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintReceipt()
        {
            try
            {
                PreparePrint(CurrentText);

                using var dlg = new PrintDialog
                {
                    Document = _printDoc,
                    UseEXDialog = true
                };
                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                _printDoc.Print();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "列印失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintDoc_PrintPage(object? sender, PrintPageEventArgs e)
        {
            using var font = new Font("Consolas", 9f);

            var g = e.Graphics ?? throw new InvalidOperationException("PrintPageEventArgs.Graphics is null");



            var left = e.MarginBounds.Left;
            var top = e.MarginBounds.Top;

            var lineHeight = (int)Math.Ceiling(font.GetHeight(e.Graphics) + 2);
            var y = top;

            while (_printLineIndex < _printLines.Length)
            {
                var line = _printLines[_printLineIndex];
                //e.Graphics.DrawString(line, font, Brushes.Black, left, y);

                g.DrawString(line, font, Brushes.Black, left, y);
                y += lineHeight;
                _printLineIndex++;

                if (y + lineHeight > e.MarginBounds.Bottom)
                {
                    e.HasMorePages = true;
                    return;
                }
            }

            e.HasMorePages = false;
        }

        private void CopyToClipboard()
        {
            try
            {
                Clipboard.SetText(CurrentText);
                MessageBox.Show("已複製到剪貼簿。", "Copy", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "複製失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveAsTxt()
        {
            try
            {
                using var sfd = new SaveFileDialog
                {
                    Title = "另存為 TXT",
                    Filter = "Text File|*.txt|All files|*.*",
                    FileName = _defaultFileName,
                    OverwritePrompt = true
                };

                if (sfd.ShowDialog(this) != DialogResult.OK) return;

                File.WriteAllText(sfd.FileName, CurrentText, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
                MessageBox.Show("已儲存。", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "儲存失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string SafeName(string? s)
        {
            s = (s ?? "").Trim();
            if (string.IsNullOrWhiteSpace(s)) return "no";
            foreach (var c in Path.GetInvalidFileNameChars())
                s = s.Replace(c, '_');
            return s;
        }

        private static string BuildSaleReceiptText(ReceiptDto r)
        {
            static string PayText(PaymentMethod m) => m switch
            {
                PaymentMethod.Cash => "現金",
                PaymentMethod.Card => "信用卡",
                PaymentMethod.Mobile => "行動支付",
                _ => m.ToString()
            };

            const int width = 32;
            static string Line(char c) => new string(c, width);
            static string Center(string s)
            {
                s ??= "";
                if (s.Length >= width) return s;
                var pad = (width - s.Length) / 2;
                return new string(' ', pad) + s;
            }

            var sb = new StringBuilder();
            sb.AppendLine(Center("POSv01 超商"));
            sb.AppendLine(Center("電子發票/收據"));
            sb.AppendLine(Line('='));
            sb.AppendLine($"單號: {r.SaleNo}");
            sb.AppendLine($"時間: {r.CreatedAtLocal:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"付款: {PayText(r.PaymentMethod)}");
            sb.AppendLine(Line('-'));

            foreach (var it in r.Items)
            {
                var name = (it.Name ?? "").Trim();
                if (name.Length > 16) name = name[..16];

                var qty = it.Quantity.ToString("0.##");
                var price = it.UnitPrice.ToString("0.##");
                var total = it.LineTotal.ToString("0.##");

                sb.AppendLine($"{name,-16}{qty,4}x{price,6}{total,6}");
            }

            sb.AppendLine(Line('-'));
            sb.AppendLine($"應收: {r.Total,24:0.##}");
            sb.AppendLine($"實收: {r.PaidAmount,24:0.##}");
            sb.AppendLine($"找零: {r.ChangeAmount,24:0.##}");
            sb.AppendLine(Line('='));
            sb.AppendLine(Center("謝謝光臨"));
            sb.AppendLine(Center("歡迎再度蒞臨"));
            return sb.ToString();
        }

        // ✅ 你報錯的就是這個：BuildReturnReceiptText 必須存在
        private static string BuildReturnReceiptText(ReturnReceiptDto r)
        {
            const int width = 32;
            static string Line(char c) => new string(c, width);
            static string Center(string s)
            {
                s ??= "";
                if (s.Length >= width) return s;
                var pad = (width - s.Length) / 2;
                return new string(' ', pad) + s;
            }

            var sb = new StringBuilder();
            sb.AppendLine(Center("POSv01 超商"));
            sb.AppendLine(Center("退貨單"));
            sb.AppendLine(Line('='));
            sb.AppendLine($"退貨單: {r.ReturnNo}");
            sb.AppendLine($"時間: {r.CreatedAtLocal:yyyy-MM-dd HH:mm:ss}");

            if (!string.IsNullOrWhiteSpace(r.OriginalSaleNo))
                sb.AppendLine($"原單號: {r.OriginalSaleNo}");
            if (!string.IsNullOrWhiteSpace(r.MemberCode))
                sb.AppendLine($"會員: {r.MemberCode}");
            if (!string.IsNullOrWhiteSpace(r.ClerkName))
                sb.AppendLine($"店員: {r.ClerkName}");

            sb.AppendLine(Line('-'));

            foreach (var it in r.Items)
            {
                var name = (it.Name ?? "").Trim();
                if (name.Length > 16) name = name[..16];

                var qty = it.Quantity.ToString("0.##");
                var price = it.UnitPrice.ToString("0.##");
                var total = it.LineTotal.ToString("0.##");

                sb.AppendLine($"{name,-16}{qty,4}x{price,6}{total,6}");
            }

            sb.AppendLine(Line('-'));
            sb.AppendLine($"退款: {r.TotalRefund,24:0.##}");
            sb.AppendLine(Line('='));
            sb.AppendLine(Center("謝謝光臨"));
            return sb.ToString();
        }
    }
}
