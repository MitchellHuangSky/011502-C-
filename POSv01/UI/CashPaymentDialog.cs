using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace POSv01.UI

{
    /// <summary>
    /// 現金收款輸入視窗（輸入實收）。
    /// </summary>
    public sealed class CashPaymentDialog : Form
    {
        private readonly decimal _total;

        private readonly TextBox _txtPaid = new();
        private readonly Label _lblTotal = new();
        private readonly Label _lblHint = new();
        private readonly Button _btnOk = new();
        private readonly Button _btnCancel = new();

        public decimal PaidAmount { get; private set; }

        public CashPaymentDialog(decimal total)
        {
            _total = total;

            Text = "現金收款";
            Width = 360;
            Height = 200;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            _lblTotal.Left = 16;
            _lblTotal.Top = 16;
            _lblTotal.Width = 300;
            _lblTotal.Text = $"應收：{_total:0.##}";

            _lblHint.Left = 16;
            _lblHint.Top = 46;
            _lblHint.Width = 300;
            _lblHint.Text = "實收金額：";

            _txtPaid.Left = 16;
            _txtPaid.Top = 70;
            _txtPaid.Width = 300;
            _txtPaid.Text = _total.ToString("0.##", CultureInfo.InvariantCulture);

            _btnOk.Left = 150;
            _btnOk.Top = 110;
            _btnOk.Width = 80;
            _btnOk.Text = "確定";
            _btnOk.Click += (_, _) => TryOk();

            _btnCancel.Left = 236;
            _btnCancel.Top = 110;
            _btnCancel.Width = 80;
            _btnCancel.Text = "取消";
            _btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.Add(_lblTotal);
            Controls.Add(_lblHint);
            Controls.Add(_txtPaid);
            Controls.Add(_btnOk);
            Controls.Add(_btnCancel);

            AcceptButton = _btnOk;
            CancelButton = _btnCancel;
        }

        private void TryOk()
        {
            if (!decimal.TryParse(_txtPaid.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var paid))
            {
                MessageBox.Show("實收金額格式錯誤。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (paid < _total)
            {
                MessageBox.Show("實收金額不足。", "不足", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            PaidAmount = paid;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
