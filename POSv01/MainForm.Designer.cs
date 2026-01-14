namespace POSv01
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtBarcode = new TextBox();
            label1 = new Label();
            gridCart = new DataGridView();
            lblTotal = new Label();
            btnClear = new Button();
            btnCash = new Button();
            btnCard = new Button();
            btnMobile = new Button();
            label2 = new Label();
            label3 = new Label();
            btnRemoveSelected = new Button();
            button1 = new Button();
            ((System.ComponentModel.ISupportInitialize)gridCart).BeginInit();
            SuspendLayout();
            // 
            // txtBarcode
            // 
            txtBarcode.Location = new Point(82, 6);
            txtBarcode.Name = "txtBarcode";
            txtBarcode.Size = new Size(160, 23);
            txtBarcode.TabIndex = 0;
            txtBarcode.KeyDown += txtBarcode_KeyDown;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(124, 253);
            label1.Name = "label1";
            label1.Size = new Size(55, 15);
            label1.TabIndex = 1;
            label1.Text = "總金額：";
            // 
            // gridCart
            // 
            gridCart.BackgroundColor = SystemColors.ActiveCaption;
            gridCart.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridCart.Location = new Point(23, 41);
            gridCart.Name = "gridCart";
            gridCart.Size = new Size(660, 206);
            gridCart.TabIndex = 2;
            // 
            // lblTotal
            // 
            lblTotal.AutoSize = true;
            lblTotal.Location = new Point(185, 253);
            lblTotal.Name = "lblTotal";
            lblTotal.Size = new Size(103, 15);
            lblTotal.TabIndex = 3;
            lblTotal.Text = "（用來顯示金額）";
            // 
            // btnClear
            // 
            btnClear.Location = new Point(23, 253);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(75, 23);
            btnClear.TabIndex = 4;
            btnClear.Text = "清空";
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += btnClear_Click;
            // 
            // btnCash
            // 
            btnCash.Location = new Point(143, 327);
            btnCash.Name = "btnCash";
            btnCash.Size = new Size(75, 23);
            btnCash.TabIndex = 5;
            btnCash.Text = "現金";
            btnCash.UseVisualStyleBackColor = true;
            btnCash.Click += btnCash_Click;
            // 
            // btnCard
            // 
            btnCard.Location = new Point(143, 369);
            btnCard.Name = "btnCard";
            btnCard.Size = new Size(75, 23);
            btnCard.TabIndex = 6;
            btnCard.Text = "信用卡";
            btnCard.UseVisualStyleBackColor = true;
            btnCard.Click += btnCard_Click;
            // 
            // btnMobile
            // 
            btnMobile.Location = new Point(143, 398);
            btnMobile.Name = "btnMobile";
            btnMobile.Size = new Size(75, 23);
            btnMobile.TabIndex = 7;
            btnMobile.Text = "行動支付";
            btnMobile.UseVisualStyleBackColor = true;
            btnMobile.Click += btnMobile_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(33, 9);
            label2.Name = "label2";
            label2.Size = new Size(43, 15);
            label2.TabIndex = 8;
            label2.Text = "條碼：";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(97, 279);
            label3.Name = "label3";
            label3.Size = new Size(55, 15);
            label3.TabIndex = 9;
            label3.Text = "移除選取";
            // 
            // btnRemoveSelected
            // 
            btnRemoveSelected.Location = new Point(158, 275);
            btnRemoveSelected.Name = "btnRemoveSelected";
            btnRemoveSelected.Size = new Size(75, 23);
            btnRemoveSelected.TabIndex = 10;
            btnRemoveSelected.Text = "移除選取";
            btnRemoveSelected.UseVisualStyleBackColor = true;
            btnRemoveSelected.Click += btnRemoveSelected_Click;
            // 
            // button1
            // 
            button1.Location = new Point(275, 12);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 11;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            button1.Click += btnPickProducts_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(button1);
            Controls.Add(btnRemoveSelected);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(btnMobile);
            Controls.Add(btnCard);
            Controls.Add(btnCash);
            Controls.Add(btnClear);
            Controls.Add(lblTotal);
            Controls.Add(gridCart);
            Controls.Add(label1);
            Controls.Add(txtBarcode);
            Name = "MainForm";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)gridCart).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtBarcode;
        private Label label1;
        private DataGridView gridCart;
        private Label lblTotal;
        private Button btnClear;
        private Button btnCash;
        private Button btnCard;
        private Button btnMobile;
        private Label label2;
        private Label label3;
        private Button btnRemoveSelected;
        private Button button1;
    }
}
