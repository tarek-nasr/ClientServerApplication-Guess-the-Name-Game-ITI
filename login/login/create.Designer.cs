namespace login
{
    partial class create
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
            categoryselect = new ComboBox();
            select = new Button();
            SuspendLayout();
            // 
            // categoryselect
            // 
            categoryselect.FormattingEnabled = true;
            categoryselect.Items.AddRange(new object[] { "Colors", "Animals", "Cities" });
            categoryselect.Location = new Point(305, 152);
            categoryselect.Name = "categoryselect";
            categoryselect.Size = new Size(151, 28);
            categoryselect.TabIndex = 0;
            // 
            // select
            // 
            select.Location = new Point(330, 261);
            select.Name = "select";
            select.Size = new Size(94, 29);
            select.TabIndex = 1;
            select.Text = "select";
            select.UseVisualStyleBackColor = true;
            select.Click += select_Click;
            // 
            // create
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(select);
            Controls.Add(categoryselect);
            Name = "create";
            Text = "create";
            Load += create_Load;
            ResumeLayout(false);
        }

        #endregion

        private ComboBox categoryselect;
        private Button select;
    }
}