using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Laserboard
{
    public partial class Form2 : Form
    {
        int Width_old;
        int Height_old;

        public Form2()
        {
            InitializeComponent();
        }

        private void box_image_MouseEnter(object sender, EventArgs e)
        {
            //Handle cursor
            if (box_Image.Image != null)
            {
                Cursor = Cursors.Hand;
            }
            else
            {
                Cursor = Cursors.Default;
            }
        }

        private void box_image_Click(object sender, EventArgs e)
        {
            if (box_Image.Image != null)
            {
                sfd_screenshot.FileName = "Screenshot_" + Text;
                sfd_screenshot.ShowDialog();
            }
        }

        private void sfd_Screenshot_FileOk(object sender, CancelEventArgs e)
        {
            //Safe file
            box_Image.Image.Save(sfd_screenshot.FileName);
        }

        private void Form2_ResizeEnd(object sender, EventArgs e)
        {
            if (box_Image.Image == null) return;
            
            // Get aspect ratio of the image
            float aspect_ratio = (float)box_Image.Image.Width / box_Image.Image.Height;

            // Check which resize difference is larger, to decide if the height or the width sould be adjusted
            if (Math.Abs(Width - Width_old) >= Math.Abs(Height - Height_old))
            {
                // Calculate the height of the box and add the window-box difference
                Height = (int)(box_Image.Width / aspect_ratio) + (Height - box_Image.Height);
            }
            else
            {
                // Calculate the width of the box and add the window-box difference
                Width = (int)(box_Image.Height * aspect_ratio) + (Width - box_Image.Width);
            }
        }

        private void Form2_ResizeBegin(object sender, EventArgs e)
        {
            Width_old = Width;
            Height_old = Height;
        }
    }
}
