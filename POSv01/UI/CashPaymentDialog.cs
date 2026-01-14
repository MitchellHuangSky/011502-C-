using System;
using System.Windows.Forms;

namespace POSv01.UI
{
    public sealed class CashPaymentDialog : Form
    {
        private readonly decimal _total;
        private readonly TextBox _txtPaid = new() { Left = 120, Top = 16, Width = 180 };
        private readonly Label _lblTotal = new() { Left = 20, Top = 18, Width = 280 };

        public decimal PaidAmount { get; private set; }

        public CashPaymentDialog(decimal total)
        {
            _total = total;

            Text = "現金收款";
            Width = 340;
            Height = 150;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            _lblTotal.Text = $"應收：{_total:0.##}";
            Controls.Add(_lblTotal);

            Controls.Add(new Label { Left = 20, Top = 50, Width = 90, Text = "收款金額" });
            _txtPaid.Top = 46;
            _txtPaid.Text = total.ToString("0.##");
            Controls.Add(_txtPaid);

            var btnOk = new Button { Left = 120, Top = 78, Width = 80, Text = "確定" };
            var btnCancel = new Button { Left = 220, Top = 78, Width = 80, Text = "取消" };

            btnOk.Click += (_, _) =>
            {
                if (!decimal.TryParse(_txtPaid.Text.Trim(), out var paid))
                {
                    MessageBox.Show("金額格式錯誤");
                    return;
                }
                if (paid < _total)
                {
                    MessageBox.Show("現金不足");
                    return;
                }

                PaidAmount = paid;
                DialogResult = DialogResult.OK;
                Close();
            };

            btnCancel.Click += (_, _) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            Controls.Add(btnOk);
            Controls.Add(btnCancel);

            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }
    }
}
