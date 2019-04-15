namespace TestBarcoded
{
    partial class Form1
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.Target = new System.Windows.Forms.NumericUpDown();
            this.Height = new System.Windows.Forms.NumericUpDown();
            this.DPI = new System.Windows.Forms.NumericUpDown();
            this.Xdim = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.HumanReadable = new System.Windows.Forms.TextBox();
            this.labFontSize = new System.Windows.Forms.Label();
            this.labWidth = new System.Windows.Forms.Label();
            this.labHeight = new System.Windows.Forms.Label();
            this.labDPI = new System.Windows.Forms.Label();
            this.labXdim = new System.Windows.Forms.Label();
            this.labFontChanged = new System.Windows.Forms.Label();
            this.labHeightChanged = new System.Windows.Forms.Label();
            this.labDPIChanged = new System.Windows.Forms.Label();
            this.LabXdimChanged = new System.Windows.Forms.Label();
            this.symbology = new System.Windows.Forms.ComboBox();
            this.showencoding = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Target)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Height)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DPI)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Xdim)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.White;
            this.pictureBox1.Location = new System.Drawing.Point(225, 93);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(1424, 726);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.PictureBox1_Click);
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(1803, 162);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(449, 28);
            this.comboBox1.TabIndex = 2;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(1828, 344);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(361, 72);
            this.button1.TabIndex = 3;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(1803, 215);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(72, 26);
            this.numericUpDown1.TabIndex = 5;
            this.numericUpDown1.Value = new decimal(new int[] {
            8,
            0,
            0,
            0});
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(1803, 48);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(449, 26);
            this.textBox1.TabIndex = 6;
            this.textBox1.Text = "abcdefghijklmnopqrstuvwxyz";
            // 
            // Target
            // 
            this.Target.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.Target.Location = new System.Drawing.Point(1803, 532);
            this.Target.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.Target.Name = "Target";
            this.Target.Size = new System.Drawing.Size(120, 26);
            this.Target.TabIndex = 7;
            this.Target.Value = new decimal(new int[] {
            200,
            0,
            0,
            0});
            // 
            // Height
            // 
            this.Height.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.Height.Location = new System.Drawing.Point(2008, 531);
            this.Height.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.Height.Name = "Height";
            this.Height.Size = new System.Drawing.Size(120, 26);
            this.Height.TabIndex = 8;
            this.Height.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // DPI
            // 
            this.DPI.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.DPI.Location = new System.Drawing.Point(1803, 670);
            this.DPI.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.DPI.Name = "DPI";
            this.DPI.Size = new System.Drawing.Size(120, 26);
            this.DPI.TabIndex = 9;
            this.DPI.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // Xdim
            // 
            this.Xdim.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.Xdim.Location = new System.Drawing.Point(2008, 669);
            this.Xdim.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.Xdim.Name = "Xdim";
            this.Xdim.Size = new System.Drawing.Size(120, 26);
            this.Xdim.TabIndex = 10;
            this.Xdim.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(1803, 506);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 20);
            this.label1.TabIndex = 11;
            this.label1.Text = "Target width";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(1803, 644);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 20);
            this.label2.TabIndex = 12;
            this.label2.Text = "DPI";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(2008, 505);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 20);
            this.label3.TabIndex = 13;
            this.label3.Text = "Height";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(2008, 643);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(45, 20);
            this.label4.TabIndex = 14;
            this.label4.Text = "Xdim";
            // 
            // HumanReadable
            // 
            this.HumanReadable.Location = new System.Drawing.Point(1807, 93);
            this.HumanReadable.Name = "HumanReadable";
            this.HumanReadable.Size = new System.Drawing.Size(445, 26);
            this.HumanReadable.TabIndex = 15;
            this.HumanReadable.Text = "abcdefghijklmnopqrstuvwxyz";
            // 
            // labFontSize
            // 
            this.labFontSize.AutoSize = true;
            this.labFontSize.Location = new System.Drawing.Point(1803, 256);
            this.labFontSize.Name = "labFontSize";
            this.labFontSize.Size = new System.Drawing.Size(51, 20);
            this.labFontSize.TabIndex = 16;
            this.labFontSize.Text = "label5";
            // 
            // labWidth
            // 
            this.labWidth.AutoSize = true;
            this.labWidth.Location = new System.Drawing.Point(1803, 574);
            this.labWidth.Name = "labWidth";
            this.labWidth.Size = new System.Drawing.Size(51, 20);
            this.labWidth.TabIndex = 17;
            this.labWidth.Text = "label6";
            // 
            // labHeight
            // 
            this.labHeight.AutoSize = true;
            this.labHeight.Location = new System.Drawing.Point(2013, 574);
            this.labHeight.Name = "labHeight";
            this.labHeight.Size = new System.Drawing.Size(51, 20);
            this.labHeight.TabIndex = 18;
            this.labHeight.Text = "label7";
            // 
            // labDPI
            // 
            this.labDPI.AutoSize = true;
            this.labDPI.Location = new System.Drawing.Point(1803, 723);
            this.labDPI.Name = "labDPI";
            this.labDPI.Size = new System.Drawing.Size(51, 20);
            this.labDPI.TabIndex = 19;
            this.labDPI.Text = "label8";
            // 
            // labXdim
            // 
            this.labXdim.AutoSize = true;
            this.labXdim.Location = new System.Drawing.Point(2004, 723);
            this.labXdim.Name = "labXdim";
            this.labXdim.Size = new System.Drawing.Size(51, 20);
            this.labXdim.TabIndex = 20;
            this.labXdim.Text = "label9";
            // 
            // labFontChanged
            // 
            this.labFontChanged.AutoSize = true;
            this.labFontChanged.Location = new System.Drawing.Point(1803, 276);
            this.labFontChanged.Name = "labFontChanged";
            this.labFontChanged.Size = new System.Drawing.Size(60, 20);
            this.labFontChanged.TabIndex = 21;
            this.labFontChanged.Text = "label10";
            // 
            // labHeightChanged
            // 
            this.labHeightChanged.AutoSize = true;
            this.labHeightChanged.Location = new System.Drawing.Point(2013, 594);
            this.labHeightChanged.Name = "labHeightChanged";
            this.labHeightChanged.Size = new System.Drawing.Size(60, 20);
            this.labHeightChanged.TabIndex = 22;
            this.labHeightChanged.Text = "label10";
            // 
            // labDPIChanged
            // 
            this.labDPIChanged.AutoSize = true;
            this.labDPIChanged.Location = new System.Drawing.Point(1803, 743);
            this.labDPIChanged.Name = "labDPIChanged";
            this.labDPIChanged.Size = new System.Drawing.Size(60, 20);
            this.labDPIChanged.TabIndex = 23;
            this.labDPIChanged.Text = "label10";
            // 
            // LabXdimChanged
            // 
            this.LabXdimChanged.AutoSize = true;
            this.LabXdimChanged.Location = new System.Drawing.Point(2008, 743);
            this.LabXdimChanged.Name = "LabXdimChanged";
            this.LabXdimChanged.Size = new System.Drawing.Size(60, 20);
            this.LabXdimChanged.TabIndex = 24;
            this.LabXdimChanged.Text = "label10";
            // 
            // symbology
            // 
            this.symbology.FormattingEnabled = true;
            this.symbology.Items.AddRange(new object[] {
            "Code128ABC",
            "Code128BAC",
            "Code128AB",
            "Code128BA",
            "Code39",
            "Code39C",
            "Code39Full",
            "Code39FullC"});
            this.symbology.Location = new System.Drawing.Point(1983, 215);
            this.symbology.Name = "symbology";
            this.symbology.Size = new System.Drawing.Size(241, 28);
            this.symbology.TabIndex = 25;
            // 
            // showencoding
            // 
            this.showencoding.AutoSize = true;
            this.showencoding.Location = new System.Drawing.Point(1983, 256);
            this.showencoding.Name = "showencoding";
            this.showencoding.Size = new System.Drawing.Size(146, 24);
            this.showencoding.TabIndex = 26;
            this.showencoding.Text = "Show Encoding";
            this.showencoding.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(2441, 1052);
            this.Controls.Add(this.showencoding);
            this.Controls.Add(this.symbology);
            this.Controls.Add(this.LabXdimChanged);
            this.Controls.Add(this.labDPIChanged);
            this.Controls.Add(this.labHeightChanged);
            this.Controls.Add(this.labFontChanged);
            this.Controls.Add(this.labXdim);
            this.Controls.Add(this.labDPI);
            this.Controls.Add(this.labHeight);
            this.Controls.Add(this.labWidth);
            this.Controls.Add(this.labFontSize);
            this.Controls.Add(this.HumanReadable);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Xdim);
            this.Controls.Add(this.DPI);
            this.Controls.Add(this.Height);
            this.Controls.Add(this.Target);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.pictureBox1);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Target)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Height)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DPI)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Xdim)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.NumericUpDown Target;
        private System.Windows.Forms.NumericUpDown Height;
        private System.Windows.Forms.NumericUpDown DPI;
        private System.Windows.Forms.NumericUpDown Xdim;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox HumanReadable;
        private System.Windows.Forms.Label labFontSize;
        private System.Windows.Forms.Label labWidth;
        private System.Windows.Forms.Label labHeight;
        private System.Windows.Forms.Label labDPI;
        private System.Windows.Forms.Label labXdim;
        private System.Windows.Forms.Label labFontChanged;
        private System.Windows.Forms.Label labHeightChanged;
        private System.Windows.Forms.Label labDPIChanged;
        private System.Windows.Forms.Label LabXdimChanged;
        private System.Windows.Forms.ComboBox symbology;
        private System.Windows.Forms.CheckBox showencoding;
    }
}

