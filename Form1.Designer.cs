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
            this.btn_calibrate_laser = new System.Windows.Forms.Button();
            this.btn_recalibrate_perspective = new System.Windows.Forms.Button();
            this.lbl_info = new System.Windows.Forms.Label();
            this.box_final = new System.Windows.Forms.PictureBox();
            this.lbl_Cam_not_found = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.box_final)).BeginInit();
            this.SuspendLayout();
            // 
            // btn_calibrate_laser
            // 
            this.btn_calibrate_laser.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_calibrate_laser.Enabled = false;
            this.btn_calibrate_laser.Location = new System.Drawing.Point(12, 387);
            this.btn_calibrate_laser.Name = "btn_calibrate_laser";
            this.btn_calibrate_laser.Size = new System.Drawing.Size(81, 23);
            this.btn_calibrate_laser.TabIndex = 13;
            this.btn_calibrate_laser.Text = "Calibrate laser";
            this.btn_calibrate_laser.UseVisualStyleBackColor = true;
            this.btn_calibrate_laser.Click += new System.EventHandler(this.btn_calibrate_laser_Click);
            // 
            // btn_recalibrate_perspective
            // 
            this.btn_recalibrate_perspective.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_recalibrate_perspective.Enabled = false;
            this.btn_recalibrate_perspective.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_recalibrate_perspective.Location = new System.Drawing.Point(99, 387);
            this.btn_recalibrate_perspective.Name = "btn_recalibrate_perspective";
            this.btn_recalibrate_perspective.Size = new System.Drawing.Size(127, 23);
            this.btn_recalibrate_perspective.TabIndex = 12;
            this.btn_recalibrate_perspective.Text = "Recalibrate perspective";
            this.btn_recalibrate_perspective.UseVisualStyleBackColor = true;
            this.btn_recalibrate_perspective.Click += new System.EventHandler(this.btn_recalibrate_perspective_Click);
            // 
            // lbl_info
            // 
            this.lbl_info.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lbl_info.AutoSize = true;
            this.lbl_info.Location = new System.Drawing.Point(232, 392);
            this.lbl_info.Name = "lbl_info";
            this.lbl_info.Size = new System.Drawing.Size(25, 13);
            this.lbl_info.TabIndex = 11;
            this.lbl_info.Text = "Info";
            // 
            // box_final
            // 
            this.box_final.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.box_final.BackColor = System.Drawing.Color.Black;
            this.box_final.Cursor = System.Windows.Forms.Cursors.Default;
            this.box_final.Location = new System.Drawing.Point(12, 12);
            this.box_final.Name = "box_final";
            this.box_final.Size = new System.Drawing.Size(492, 369);
            this.box_final.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.box_final.TabIndex = 10;
            this.box_final.TabStop = false;
            this.box_final.MouseDown += new System.Windows.Forms.MouseEventHandler(this.box_final_MouseDown);
            this.box_final.MouseMove += new System.Windows.Forms.MouseEventHandler(this.box_final_MouseMove);
            this.box_final.MouseUp += new System.Windows.Forms.MouseEventHandler(this.box_final_MouseUp);
            // 
            // lbl_Cam_not_found
            // 
            this.lbl_Cam_not_found.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lbl_Cam_not_found.AutoSize = true;
            this.lbl_Cam_not_found.BackColor = System.Drawing.Color.Black;
            this.lbl_Cam_not_found.ForeColor = System.Drawing.Color.White;
            this.lbl_Cam_not_found.Location = new System.Drawing.Point(197, 191);
            this.lbl_Cam_not_found.Name = "lbl_Cam_not_found";
            this.lbl_Cam_not_found.Size = new System.Drawing.Size(98, 13);
            this.lbl_Cam_not_found.TabIndex = 14;
            this.lbl_Cam_not_found.Text = "Webcam not found";
            this.lbl_Cam_not_found.Visible = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(516, 422);
            this.Controls.Add(this.lbl_Cam_not_found);
            this.Controls.Add(this.btn_calibrate_laser);
            this.Controls.Add(this.btn_recalibrate_perspective);
            this.Controls.Add(this.lbl_info);
            this.Controls.Add(this.box_final);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(532, 460);
            this.Name = "Form1";
            this.Text = "Laserboard";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.box_final)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_calibrate_laser;
        private System.Windows.Forms.Button btn_recalibrate_perspective;
        private System.Windows.Forms.Label lbl_info;
        private System.Windows.Forms.PictureBox box_final;
        private System.Windows.Forms.Label lbl_Cam_not_found;
    }
}

