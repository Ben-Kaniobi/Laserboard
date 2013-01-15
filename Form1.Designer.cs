namespace Laserboard
{
    partial class Form1
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.box_Final = new System.Windows.Forms.PictureBox();
            this.lbl_Info = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.box_Final)).BeginInit();
            this.SuspendLayout();
            // 
            // box_Final
            // 
            this.box_Final.BackColor = System.Drawing.Color.Black;
            this.box_Final.Dock = System.Windows.Forms.DockStyle.Fill;
            this.box_Final.Location = new System.Drawing.Point(0, 0);
            this.box_Final.Name = "box_Final";
            this.box_Final.Size = new System.Drawing.Size(784, 562);
            this.box_Final.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.box_Final.TabIndex = 10;
            this.box_Final.TabStop = false;
            this.box_Final.SizeChanged += new System.EventHandler(this.box_Size_or_position_changed);
            this.box_Final.MouseDown += new System.Windows.Forms.MouseEventHandler(this.box_final_MouseDown);
            this.box_Final.MouseMove += new System.Windows.Forms.MouseEventHandler(this.box_final_MouseMove);
            this.box_Final.MouseUp += new System.Windows.Forms.MouseEventHandler(this.box_final_MouseUp);
            // 
            // lbl_Info
            // 
            this.lbl_Info.BackColor = System.Drawing.Color.Black;
            this.lbl_Info.Dock = System.Windows.Forms.DockStyle.Top;
            this.lbl_Info.ForeColor = System.Drawing.Color.White;
            this.lbl_Info.Location = new System.Drawing.Point(0, 0);
            this.lbl_Info.Name = "lbl_Info";
            this.lbl_Info.Size = new System.Drawing.Size(784, 16);
            this.lbl_Info.TabIndex = 11;
            this.lbl_Info.Text = "Info";
            this.lbl_Info.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 562);
            this.Controls.Add(this.lbl_Info);
            this.Controls.Add(this.box_Final);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(400, 300);
            this.Name = "Form1";
            this.Text = "Laserboard";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.LocationChanged += new System.EventHandler(this.box_Size_or_position_changed);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyUp);
            ((System.ComponentModel.ISupportInitialize)(this.box_Final)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox box_Final;
        private System.Windows.Forms.Label lbl_Info;
    }
}

