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
        const int SIZE_CHESSFIELD = 100;
        const string FILE_CHESSBOARD = @"..\..\files\Chessboard.png";
        const string FILE_TEST = @"..\..\files\Chessboard 2 Capture.jpg";
        const string FILE_TEST2 = @"..\..\files\Test_Image_2.png";

        //Variables
        Image<Bgr, Byte> image_original;
        Image<Bgr, Byte> image_transformed;
        Image<Gray, Byte> image_filtered;
        Graphics drawings;
        Capture webcam;
        bool calibrated = false;

        HomographyMatrix t_matrix;

        public frm_Main()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            lbl_info.Text = "";

            //Create graphics to draw on box_final
            drawings = box_final.CreateGraphics();
            drawings.SmoothingMode = SmoothingMode.AntiAlias;

            //Make sure files exist
            if (!File.Exists(FILE_CHESSBOARD) || !File.Exists(FILE_TEST) || !File.Exists(FILE_TEST2))
            {
                lbl_info.Text = "Image not found";
            }
            else
            {
                try
                {
                    //Capture webcam
                    webcam = new Capture();
                }
                catch
                {
                    lbl_info.Text = "Image not found";
                }

                Application.Idle += new EventHandler(Show_cam);
            }
        }

        private void Show_cam(object sender, EventArgs e)
        {
            if (!calibrated)
            {
                //Load image and display webcam in box_original
                image_original = new Image<Bgr, byte>(FILE_TEST);
                //image_original = webcam.QueryFrame();
                box_original.Image = image_original.ToBitmap();

                btn_calibrate.Enabled = true;
            }
            else
            {
                //Get red
                image_filtered = image_transformed.SmoothBlur(5, 5).InRange(new Bgr(Color.DarkRed), new Bgr(Color.FromArgb(255, 100, 100)));
                box_filtered.Image = image_filtered.ToBitmap();

                //Find_circles();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Calibrate_perspective();
        }

        private void Calibrate_perspective()
        {
            //Load and display chessboard image for calibration
            Image<Gray, Byte> image_chessboard = new Image<Gray, byte>(FILE_CHESSBOARD);
            box_final.Image = image_chessboard.ToBitmap();

            //Get corner-points of original and captured chessboard
            Size size_p = new Size(N_CHESSFIELDS_X - 1, N_CHESSFIELDS_Y - 1);
            Emgu.CV.CvEnum.CALIB_CB_TYPE calibrations = Emgu.CV.CvEnum.CALIB_CB_TYPE.ADAPTIVE_THRESH | Emgu.CV.CvEnum.CALIB_CB_TYPE.NORMALIZE_IMAGE | Emgu.CV.CvEnum.CALIB_CB_TYPE.FILTER_QUADS;
            PointF[] corners_dst = CameraCalibration.FindChessboardCorners(image_chessboard, size_p, calibrations);
            PointF[] corners_src = CameraCalibration.FindChessboardCorners(image_original.Convert<Gray, Byte>(), size_p, calibrations);

            //Create matrix for transformation
            t_matrix = CameraCalibration.FindHomography(corners_src, corners_dst, Emgu.CV.CvEnum.HOMOGRAPHY_METHOD.DEFAULT, 1);

            //Transform and resize image
            image_transformed = image_original.WarpPerspective(t_matrix, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC, Emgu.CV.CvEnum.WARP.CV_WARP_FILL_OUTLIERS, new Bgr(Color.Green));
            Rectangle sub_image = new Rectangle(0, 0, Convert.ToInt32(corners_dst[corners_dst.Length - 1].X + SIZE_CHESSFIELD), Convert.ToInt32(corners_dst[corners_dst.Length - 1].Y + SIZE_CHESSFIELD));
            image_transformed = image_transformed.GetSubRect(sub_image).Resize(image_original.Size.Width, image_original.Size.Height, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);

            //lbl_info.Text = image_original.Size.Width + "-" + image_original.Size.Height + " " + image_transformed.Size.Width + "-" + image_transformed.Size.Height;

            //Display in box_transformed
            box_transformed.Image = image_transformed.ToBitmap();

            //Clear box_final
            box_final.Image = null;
            box_final.BackColor = Color.Black;

            calibrated = true;
        }

        private void Find_circles()
        {
            //Clear image
            //drawings.DrawImage(box_final.Image, 0, 0); //maybe backgroundimage

            //Find Circles
            CircleF[] circles = image_filtered.HoughCircles(
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
                Pen pen_circle = new Pen(Color.Red, 3);
                drawings.DrawEllipse(pen_circle, circles[0].Center.X - circles[0].Radius, circles[0].Center.Y - circles[0].Radius, circles[0].Radius * 2, circles[0].Radius * 2);
            }

            /*//int circle_number = 0;
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

        private void frm_Pointboard_FormClosing(object sender, FormClosingEventArgs e)
        {
            Dispose();
        }
    }
}
