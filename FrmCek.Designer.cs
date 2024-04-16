
namespace REG2Publisher
{
    partial class FrmCek
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
            this.components = new System.ComponentModel.Container();
            this.text = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.lbl_tunggu = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // text
            // 
            this.text.Location = new System.Drawing.Point(7, 144);
            this.text.Name = "text";
            this.text.Size = new System.Drawing.Size(185, 15);
            this.text.TabIndex = 1;
            this.text.Text = "Menunggu Listeners merespon ..";
            this.text.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.text.Click += new System.EventHandler(this.label1_Click);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // lbl_tunggu
            // 
            this.lbl_tunggu.Location = new System.Drawing.Point(6, 159);
            this.lbl_tunggu.Name = "lbl_tunggu";
            this.lbl_tunggu.Size = new System.Drawing.Size(185, 15);
            this.lbl_tunggu.TabIndex = 2;
            this.lbl_tunggu.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lbl_tunggu.Click += new System.EventHandler(this.lbl_tunggu_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::REG2PubLink.Properties.Resources.loader;
            this.pictureBox1.Location = new System.Drawing.Point(6, 5);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(185, 136);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // FrmCek
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(197, 178);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.lbl_tunggu);
            this.Controls.Add(this.text);
            this.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FrmCek";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FrmCek";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.FrmCek_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label text;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label lbl_tunggu;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}