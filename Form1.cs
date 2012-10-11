using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO;
using System.Drawing.Drawing2D;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;

namespace Pointboard
{
    public partial class frm_Main : Form
    {
        //Constants
        const int N_CHESSFIELDS_X = 8;
        const int N_CHESSFIELDS_Y = 6;
        const int OFFSET_CHESSBOARD = 7;
        const string FILE_TEST = @"..\..\files\Test_image_black_red.png";
        const string FILE_TEST_2 = @"..\..\files\Screenshot.png";

        //Variables
        Image<Gray, Byte> Image_chessboard;
        Image<Bgr, Byte> Image_original;
        Image<Bgr, Byte> Image_transformed;
        Image<Gray, Byte> Image_filtered;
        Graphics Drawings;
        Capture Webcam;
        bool Calibrated = false;

        HomographyMatrix Transformation_matrix;

        public frm_Main()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            lbl_info.Text = "";

            //Create graphics to draw on box_final
            Drawings = box_final.CreateGraphics();
            Drawings.SmoothingMode = SmoothingMode.AntiAlias;
            
            try
            {
                //Capture Webcam
                Webcam = new Capture();
                Application.Idle += new EventHandler(Show_cam);
            }
            catch
            {
                Application.Idle += new EventHandler(Testmode);
                lbl_info.Text = "Webcam not found. Using testmode";
            }
        }

        private void btn_Calibrate_Click(object sender, EventArgs e)
        {
            Calibrated = Calibrate_perspective();
        }

        private bool Calibrate_perspective()
        {
            if (Image_chessboard == null)
            {//Chessboard-image not loaded yet
                //Load (with same size as original)
                Image_chessboard = new Image<Gray, byte>(Laserboard.Properties.Resources.Chessboard).Resize(Image_original.Width, Image_original.Height, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            }

            //Display
            box_final.BackColor = Color.Black;
            box_final.Image = Image_chessboard.Resize(box_final.Width - 2 * OFFSET_CHESSBOARD, box_final.Height - 2 * OFFSET_CHESSBOARD, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC).ToBitmap();

            //Get corner-points of original and captured chessboard
            Size size_p = new Size(N_CHESSFIELDS_X - 1, N_CHESSFIELDS_Y - 1);
            Emgu.CV.CvEnum.CALIB_CB_TYPE calibrations = Emgu.CV.CvEnum.CALIB_CB_TYPE.ADAPTIVE_THRESH | Emgu.CV.CvEnum.CALIB_CB_TYPE.NORMALIZE_IMAGE | Emgu.CV.CvEnum.CALIB_CB_TYPE.FILTER_QUADS;
            PointF[] corners_dst = CameraCalibration.FindChessboardCorners(Image_chessboard, size_p, calibrations);
            PointF[] corners_src = CameraCalibration.FindChessboardCorners(Image_original.Convert<Gray, Byte>(), size_p, calibrations);
            if (corners_src == null || corners_dst == null) return false; //Chessboard not found

            //Get matrix for transformation
            Transformation_matrix = CameraCalibration.FindHomography(corners_src, corners_dst, Emgu.CV.CvEnum.HOMOGRAPHY_METHOD.DEFAULT, 1);

            //Clear box_final
            box_final.Image = null;
            //box_final.BackColor = Color.Black;

            return true; //Successful
        }

        private void Testmode(object sender, EventArgs e)
        {
            box_original.BackColor = Color.Gray;

            //Load and display test image
            Image_transformed = new Image<Bgr, byte>(FILE_TEST_2);
            box_transformed.Image = Image_transformed.ToBitmap();

            //Clear box_final
            box_final.Image = null;
            box_final.BackColor = Color.Black;

            Filter();

            Find_point();

            //Simulate 30Fps
            System.Threading.Thread.Sleep(33);
        }

        private void Show_cam(object sender, EventArgs e)
        {
            //Load and display Webcam-image in box_original
            Image_original = Webcam.QueryFrame();
            box_original.Image = Image_original.ToBitmap();

            if (!Calibrated)
            {
                btn_Calibrate.Enabled = true;
                Calibrated = Calibrate_perspective();
            }
            else
            {
                //Transform and display image
                Bgr color_outside = new Bgr(Color.Red); //Detect/change later
                Image_transformed = Image_original.WarpPerspective(Transformation_matrix, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC, Emgu.CV.CvEnum.WARP.CV_WARP_FILL_OUTLIERS, color_outside);
                box_transformed.Image = Image_transformed.ToBitmap();

                Filter();

                Find_point();
            }
        }

        private void Filter()
        {
            //Get red
            Image_filtered = Image_transformed.SmoothBlur(5, 5).InRange(new Bgr(Color.DarkRed), new Bgr(Color.White));//Color.FromArgb(255, 100, 100)));
            box_filtered.Image = Image_filtered.ToBitmap();

            /*Rectangle rect = new Rectangle(298, 324, 30, 30);
            Hsv average = Image_transformed.GetSubRect(rect).Convert<Hsv, byte>().GetAverage();

            Hsv threshold_lower = new Hsv(average.Hue -20, average.Satuation -20, average.Value -20);
            Hsv threshold_higher = new Hsv(average.Hue +20, 255, 255);

            Image_filtered = Image_transformed.Convert<Hsv, byte>().InRange(threshold_lower, threshold_higher);
            box_filtered.Image = Image_filtered.ToBitmap();*/
        }

        private void btn_Test_Click(object sender, EventArgs e)
        {
            if(File.Exists(FILE_TEST))
            {
                box_final.Image = new Image<Bgr, byte>(FILE_TEST).ToBitmap();
            }
            else
            {
                lbl_info.Text = "Test-image not found";
            }
        }

        private void Find_point()
        {
            //Get scale factors
            Double factor_x = 1 - (Convert.ToDouble(box_original.Width) / Image_filtered.Width);
            Double factor_y = 1 - (Convert.ToDouble(box_original.Height) / Image_filtered.Height);

            //Clear image
            if (box_final.Image != null)
            {
                Drawings.DrawImage(box_final.Image, 0, 0);
            }
            else
            {
                Drawings.Clear(box_final.BackColor);
            }

            //Find Circles
            CircleF[] circles = Image_filtered.HoughCircles(
            new Gray(180), //The higher threshold of the two passed to Canny edge detector (the lower one will be twice smaller)
            new Gray(6), //Accumulator threshold at the center detection stage
            1.0, //Resolution of the accumulator used to detect centers of the circles
            10.0, //Min distance
            2, //Min radius
            20 //Max radius
            )[0]; //Get the circles from the first channel
            
            //Mark first circle
            if (circles.Length > 0)
            {
                //lbl_info.Text = circles[0].Center.X.ToString() + " " + circles[0].Center.Y.ToString();
                Pen pen_circle = new Pen(Color.Blue, 3);

                float radius = circles[0].Radius + pen_circle.Width;
                Rectangle rect = new Rectangle(Convert.ToInt32(factor_x * (circles[0].Center.X - circles[0].Radius)),
                    Convert.ToInt32(factor_y * (circles[0].Center.Y - circles[0].Radius)),
                    Convert.ToInt32(factor_x * radius * 2), Convert.ToInt32(factor_y * radius * 2));
                Drawings.DrawEllipse(pen_circle, rect);//(pen_circle, circles[0].Center.X - circles[0].Radius, circles[0].Center.Y - circles[0].Radius, radius * 2, radius * 2);
            }

            /*Mark multiple circles
            //int circle_number = 0;
            //lbl_info.Text = "";
            //foreach (CircleF circle in circles)
            {
                if (circle_number >= 3)
                {
                    lbl_info.Text += " +";
                    return;
                }
                circle_number++;
                lbl_info.Text += "(" + circle.Center.X + "; " + circle.Center.Y + ")  ";
                Grafik.DrawEllipse(circlepen, circle.Center.X - circle.Radius, circle.Center.Y - circle.Radius, circle.Radius * 2, circle.Radius * 2);
            }*/
        }

        private void box_filtered_Click(object sender, EventArgs e)
        {
            if (box_filtered.Image != null)
            {
                sfd_Screenshot.Tag = box_filtered;
                sfd_Screenshot.ShowDialog();
            }
        }

        private void box_transformed_Click(object sender, EventArgs e)
        {
            if (box_transformed.Image != null)
            {
                sfd_Screenshot.Tag = box_transformed;
                sfd_Screenshot.ShowDialog();
            }
        }

        private void box_original_Click(object sender, EventArgs e)
        {
            if (box_original.Image != null)
            {
                sfd_Screenshot.Tag = box_original;
                sfd_Screenshot.ShowDialog();
            }
        }

        private void sfd_Screenshot_FileOk(object sender, CancelEventArgs e)
        {
            if (sfd_Screenshot.Tag == box_filtered)
            {
                box_filtered.Image.Save(sfd_Screenshot.FileName);
            }
            else if (sfd_Screenshot.Tag == box_transformed)
            {
                box_transformed.Image.Save(sfd_Screenshot.FileName);
            }
            else if (sfd_Screenshot.Tag == box_original)
            {
                box_original.Image.Save(sfd_Screenshot.FileName);
            }
        }

        private void frm_Pointboard_FormClosing(object sender, FormClosingEventArgs e)
        {
            Dispose();
        }
    }
}
