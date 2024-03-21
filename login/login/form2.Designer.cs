namespace login
{
    partial class form2
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
            refresh = new Button();
            create = new Button();
            listBox1 = new ListBox();
            join = new Button();
            watch = new Button();
            SuspendLayout();
            // 
            // refresh
            // 
            refresh.Location = new Point(683, 202);
            refresh.Name = "refresh";
            refresh.Size = new Size(94, 29);
            refresh.TabIndex = 0;
            refresh.Text = "refresh";
            refresh.UseVisualStyleBackColor = true;
            refresh.Click += refresh_Click;
            // 
            // create
            // 
            create.Location = new Point(339, 372);
            create.Name = "create";
            create.Size = new Size(94, 29);
            create.TabIndex = 1;
            create.Text = "create";
            create.UseVisualStyleBackColor = true;
            create.Click += create_Click;
            // 
            // listBox1
            // 
            listBox1.FormattingEnabled = true;
            listBox1.Location = new Point(30, 65);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(619, 264);
            listBox1.TabIndex = 3;
            listBox1.Click += listBox1_Click;
            // 
            // join
            // 
            join.Location = new Point(58, 372);
            join.Name = "join";
            join.Size = new Size(94, 29);
            join.TabIndex = 4;
            join.Text = "join";
            join.UseVisualStyleBackColor = true;
            join.Click += join_Click;
            // 
            // watch
            // 
            watch.Location = new Point(640, 372);
            watch.Name = "watch";
            watch.Size = new Size(94, 29);
            watch.TabIndex = 5;
            watch.Text = "watch";
            watch.UseVisualStyleBackColor = true;
            watch.Click += watch_Click;
            // 
            // form2
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 451);
            Controls.Add(watch);
            Controls.Add(join);
            Controls.Add(listBox1);
            Controls.Add(create);
            Controls.Add(refresh);
            Name = "form2";
            Text = "form2";
            Load += form2_Load;
            ResumeLayout(false);
        }

        #endregion

        private Button refresh;
        private Button create;
        private ListBox listBox1;
        private Button join;
        private Button watch;
    }
}