namespace Laserboard
{
    partial class Form2
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form2));
            this.box_Image = new System.Windows.Forms.PictureBox();
            this.sfd_screenshot = new System.Windows.Forms.SaveFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.box_Image)).BeginInit();
            this.SuspendLayout();
            // 
            // box_Image
            // 
            this.box_Image.BackColor = System.Drawing.Color.Black;
            this.box_Image.Dock = System.Windows.Forms.DockStyle.Fill;
            this.box_Image.Location = new System.Drawing.Point(0, 0);
            this.box_Image.Name = "box_Image";
            this.box_Image.Size = new System.Drawing.Size(284, 212);
            this.box_Image.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.box_Image.TabIndex = 1;
            this.box_Image.TabStop = false;
            this.box_Image.Click += new System.EventHandler(this.box_image_Click);
            this.box_Image.MouseEnter += new System.EventHandler(this.box_image_MouseEnter);
            // 
            // sfd_screenshot
            // 
            this.sfd_screenshot.DefaultExt = "png";
            this.sfd_screenshot.FileName = "Screenshot";
            this.sfd_screenshot.Filter = "PNG-Image|*.png";
            this.sfd_screenshot.Title = "Save screenshot";
            this.sfd_screenshot.FileOk += new System.ComponentModel.CancelEventHandler(this.sfd_Screenshot_FileOk);
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 212);
            this.Controls.Add(this.box_Image);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form2";
            this.ShowInTaskbar = false;
            this.Text = "Form2";
            this.ResizeBegin += new System.EventHandler(this.Form2_ResizeBegin);
            this.ResizeEnd += new System.EventHandler(this.Form2_ResizeEnd);
            ((System.ComponentModel.ISupportInitialize)(this.box_Image)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.PictureBox box_Image;
        private System.Windows.Forms.SaveFileDialog sfd_screenshot;

    }
}