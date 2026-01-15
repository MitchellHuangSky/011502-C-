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


        //-------

        private void InitializeComponent()
        {
            txtBarcode = new TextBox();
            lblBarcode = new Label();
            gridCart = new DataGridView();
            btnClear = new Button();
            btnRemoveSelected = new Button();
            lblTotalTitle = new Label();
            lblTotal = new Label();
            btnCash = new Button();
            btnCard = new Button();
            btnMobile = new Button();
            flpQuick = new FlowLayoutPanel();
            lblQuick = new Label();
            ((System.ComponentModel.ISupportInitialize)gridCart).BeginInit();
            SuspendLayout();
            // 
            // txtBarcode
            // 
            txtBarcode.Location = new Point(147, 24);
            txtBarcode.Margin = new Padding(4);
            txtBarcode.Name = "txtBarcode";
            txtBarcode.Size = new Size(481, 27);
            txtBarcode.TabIndex = 0;
            txtBarcode.KeyDown += txtBarcode_KeyDown;
            // 
            // lblBarcode
            // 
            lblBarcode.AutoSize = true;
            lblBarcode.Location = new Point(28, 28);
            lblBarcode.Margin = new Padding(4, 0, 4, 0);
            lblBarcode.Name = "lblBarcode";
            lblBarcode.Size = new Size(84, 19);
            lblBarcode.TabIndex = 1;
            lblBarcode.Text = "條碼輸入：";
            // 
            // gridCart
            // 
            gridCart.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridCart.Location = new Point(28, 73);
            gridCart.Margin = new Padding(4);
            gridCart.Name = "gridCart";
            gridCart.RowHeadersWidth = 51;
            gridCart.Size = new Size(951, 532);
            gridCart.TabIndex = 2;
            // 
            // btnClear
            // 
            btnClear.Location = new Point(28, 623);
            btnClear.Margin = new Padding(4);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(129, 38);
            btnClear.TabIndex = 3;
            btnClear.Text = "清空";
            btnClear.UseVisualStyleBackColor = true;
            // 
            // btnRemoveSelected
            // 
            btnRemoveSelected.Location = new Point(170, 623);
            btnRemoveSelected.Margin = new Padding(4);
            btnRemoveSelected.Name = "btnRemoveSelected";
            btnRemoveSelected.Size = new Size(154, 38);
            btnRemoveSelected.TabIndex = 4;
            btnRemoveSelected.Text = "移除一筆";
            btnRemoveSelected.UseVisualStyleBackColor = true;
            // 
            // lblTotalTitle
            // 
            lblTotalTitle.AutoSize = true;
            lblTotalTitle.Location = new Point(656, 630);
            lblTotalTitle.Margin = new Padding(4, 0, 4, 0);
            lblTotalTitle.Name = "lblTotalTitle";
            lblTotalTitle.Size = new Size(69, 19);
            lblTotalTitle.TabIndex = 5;
            lblTotalTitle.Text = "總金額：";
            // 
            // lblTotal
            // 
            lblTotal.AutoSize = true;
            lblTotal.Location = new Point(734, 621);
            lblTotal.Margin = new Padding(4, 0, 4, 0);
            lblTotal.Name = "lblTotal";
            lblTotal.Size = new Size(31, 19);
            lblTotal.TabIndex = 6;
            lblTotal.Text = "$ 0";
            // 
            // btnCash
            // 
            btnCash.Location = new Point(28, 684);
            btnCash.Margin = new Padding(4);
            btnCash.Name = "btnCash";
            btnCash.Size = new Size(309, 79);
            btnCash.TabIndex = 7;
            btnCash.Text = "現金結帳";
            btnCash.UseVisualStyleBackColor = true;
            // 
            // btnCard
            // 
            btnCard.Location = new Point(28, 773);
            btnCard.Margin = new Padding(4);
            btnCard.Name = "btnCard";
            btnCard.Size = new Size(309, 79);
            btnCard.TabIndex = 8;
            btnCard.Text = "信用卡";
            btnCard.UseVisualStyleBackColor = true;
            // 
            // btnMobile
            // 
            btnMobile.Location = new Point(28, 861);
            btnMobile.Margin = new Padding(4);
            btnMobile.Name = "btnMobile";
            btnMobile.Size = new Size(309, 79);
            btnMobile.TabIndex = 9;
            btnMobile.Text = "行動支付";
            btnMobile.UseVisualStyleBackColor = true;
            // 
            // flpQuick
            // 
            flpQuick.FlowDirection = FlowDirection.TopDown;
            flpQuick.Location = new Point(1016, 58);
            flpQuick.Margin = new Padding(4);
            flpQuick.Name = "flpQuick";
            flpQuick.Size = new Size(334, 882);
            flpQuick.TabIndex = 11;
            flpQuick.WrapContents = false;
            // 
            // lblQuick
            // 
            lblQuick.AutoSize = true;
            lblQuick.Location = new Point(1016, 28);
            lblQuick.Margin = new Padding(4, 0, 4, 0);
            lblQuick.Name = "lblQuick";
            lblQuick.Size = new Size(99, 19);
            lblQuick.TabIndex = 10;
            lblQuick.Text = "快捷功能區：";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1389, 975);
            Controls.Add(flpQuick);
            Controls.Add(lblQuick);
            Controls.Add(btnMobile);
            Controls.Add(btnCard);
            Controls.Add(btnCash);
            Controls.Add(lblTotal);
            Controls.Add(lblTotalTitle);
            Controls.Add(btnRemoveSelected);
            Controls.Add(btnClear);
            Controls.Add(gridCart);
            Controls.Add(lblBarcode);
            Controls.Add(txtBarcode);
            Margin = new Padding(4);
            Name = "MainForm";
            Text = "POSv01 超商收銀台";
            ((System.ComponentModel.ISupportInitialize)gridCart).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtBarcode;
        private Label lblBarcode;
        private DataGridView gridCart;
        private Button btnClear;
        private Button btnRemoveSelected;
        private Label lblTotalTitle;
        private Label lblTotal;
        private Button btnCash;
        private Button btnCard;
        private Button btnMobile;
        private FlowLayoutPanel flpQuick;
        private Label lblQuick;




        /*
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
            ((System.ComponentModel.ISupportInitialize)gridCart).BeginInit();
            SuspendLayout();
            // 
            // txtBarcode
            // 
            txtBarcode.Location = new Point(105, 8);
            txtBarcode.Margin = new Padding(4, 4, 4, 4);
            txtBarcode.Name = "txtBarcode";
            txtBarcode.Size = new Size(205, 27);
            txtBarcode.TabIndex = 0;
            txtBarcode.KeyDown += txtBarcode_KeyDown;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(159, 320);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(69, 19);
            label1.TabIndex = 1;
            label1.Text = "總金額：";
            // 
            // gridCart
            // 
            gridCart.BackgroundColor = SystemColors.ActiveCaption;
            gridCart.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridCart.Location = new Point(30, 52);
            gridCart.Margin = new Padding(4, 4, 4, 4);
            gridCart.Name = "gridCart";
            gridCart.RowHeadersWidth = 51;
            gridCart.Size = new Size(690, 261);
            gridCart.TabIndex = 2;
            // 
            // lblTotal
            // 
            lblTotal.AutoSize = true;
            lblTotal.Location = new Point(238, 320);
            lblTotal.Margin = new Padding(4, 0, 4, 0);
            lblTotal.Name = "lblTotal";
            lblTotal.Size = new Size(129, 19);
            lblTotal.TabIndex = 3;
            lblTotal.Text = "（用來顯示金額）";
            // 
            // btnClear
            // 
            btnClear.Location = new Point(30, 320);
            btnClear.Margin = new Padding(4, 4, 4, 4);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(96, 29);
            btnClear.TabIndex = 4;
            btnClear.Text = "清空";
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += btnClear_Click;
            // 
            // btnCash
            // 
            btnCash.Location = new Point(184, 414);
            btnCash.Margin = new Padding(4, 4, 4, 4);
            btnCash.Name = "btnCash";
            btnCash.Size = new Size(96, 29);
            btnCash.TabIndex = 5;
            btnCash.Text = "現金";
            btnCash.UseVisualStyleBackColor = true;
            // 
            // btnCard
            // 
            btnCard.Location = new Point(184, 467);
            btnCard.Margin = new Padding(4, 4, 4, 4);
            btnCard.Name = "btnCard";
            btnCard.Size = new Size(96, 29);
            btnCard.TabIndex = 6;
            btnCard.Text = "信用卡";
            btnCard.UseVisualStyleBackColor = true;
            // 
            // btnMobile
            // 
            btnMobile.Location = new Point(184, 504);
            btnMobile.Margin = new Padding(4, 4, 4, 4);
            btnMobile.Name = "btnMobile";
            btnMobile.Size = new Size(96, 29);
            btnMobile.TabIndex = 7;
            btnMobile.Text = "行動支付";
            btnMobile.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(42, 11);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(54, 19);
            label2.TabIndex = 8;
            label2.Text = "條碼：";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(125, 353);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(69, 19);
            label3.TabIndex = 9;
            label3.Text = "移除選取";
            // 
            // btnRemoveSelected
            // 
            btnRemoveSelected.Location = new Point(203, 348);
            btnRemoveSelected.Margin = new Padding(4, 4, 4, 4);
            btnRemoveSelected.Name = "btnRemoveSelected";
            btnRemoveSelected.Size = new Size(96, 29);
            btnRemoveSelected.TabIndex = 10;
            btnRemoveSelected.Text = "移除選取";
            btnRemoveSelected.UseVisualStyleBackColor = true;
            btnRemoveSelected.Click += btnRemoveSelected_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1029, 570);
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
            Margin = new Padding(4, 4, 4, 4);
            Name = "MainForm";
            Text = "Form1";
            Load += MainForm_Load;
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

        */

    }
}
