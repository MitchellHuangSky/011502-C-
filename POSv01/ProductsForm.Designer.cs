namespace POSv01
{
    partial class ProductsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtSearch = new TextBox();
            btnSearch = new Button();
            btnAdd = new Button();
            flpProducts = new FlowLayoutPanel();
            SuspendLayout();
            // 
            // txtSearch
            // 
            txtSearch.Location = new Point(31, 30);
            txtSearch.Margin = new Padding(4);
            txtSearch.Multiline = true;
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(209, 23);
            txtSearch.TabIndex = 0;
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(258, 30);
            btnSearch.Margin = new Padding(4);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(96, 29);
            btnSearch.TabIndex = 1;
            btnSearch.Text = "搜尋";
            btnSearch.UseVisualStyleBackColor = true;
            // 
            // btnAdd
            // 
            btnAdd.Location = new Point(403, 30);
            btnAdd.Margin = new Padding(4);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(96, 29);
            btnAdd.TabIndex = 2;
            btnAdd.Text = "新增商品";
            btnAdd.UseVisualStyleBackColor = true;
            // 
            // flpProducts
            // 
            flpProducts.Location = new Point(31, 93);
            flpProducts.Margin = new Padding(4);
            flpProducts.Name = "flpProducts";
            flpProducts.Size = new Size(705, 402);
            flpProducts.TabIndex = 3;
            // 
            // ProductsForm
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1029, 746);
            Controls.Add(flpProducts);
            Controls.Add(btnAdd);
            Controls.Add(btnSearch);
            Controls.Add(txtSearch);
            Margin = new Padding(4);
            Name = "ProductsForm";
            Text = "ProductsForm";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtSearch;
        private Button btnSearch;
        private Button btnAdd;
        private FlowLayoutPanel flpProducts;
    }
}