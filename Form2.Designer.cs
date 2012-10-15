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
            this.box_image = new System.Windows.Forms.PictureBox();
            this.sfd_screenshot = new System.Windows.Forms.SaveFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.box_image)).BeginInit();
            this.SuspendLayout();
            // 
            // box_image
            // 
            this.box_image.BackColor = System.Drawing.Color.Black;
            this.box_image.Dock = System.Windows.Forms.DockStyle.Fill;
            this.box_image.Location = new System.Drawing.Point(0, 0);
            this.box_image.Name = "box_image";
            this.box_image.Size = new System.Drawing.Size(284, 262);
            this.box_image.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.box_image.TabIndex = 1;
            this.box_image.TabStop = false;
            this.box_image.Click += new System.EventHandler(this.box_image_Click);
            this.box_image.MouseEnter += new System.EventHandler(this.box_image_MouseEnter);
            // 
            // sfd_screenshot
            // 
            this.sfd_screenshot.DefaultExt = "png";
            this.sfd_screenshot.FileName = "Screenshot";
            this.sfd_screenshot.Filter = "PNG-Image|*.png";
            this.sfd_screenshot.Title = "Save screenshot";
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.box_image);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "Form2";
            this.Text = "Form2";
            ((System.ComponentModel.ISupportInitialize)(this.box_image)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.PictureBox box_image;
        private System.Windows.Forms.SaveFileDialog sfd_screenshot;

    }
}