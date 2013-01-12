using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Laserboard
{
    public partial class Form2 : Form
    {
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
    }
}
