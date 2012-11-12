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

namespace Laserboard
{
    public partial class Form1 : Form
    {
        //Forms
        Form2 frm_webcam = new Form2();
        Form2 frm_transformed = new Form2();
        Form2 frm_filtered = new Form2();
        Form2 frm_laser = new Form2();

        //Constants
        const int N_CHESSFIELDS_X = 8;
        const int N_CHESSFIELDS_Y = 6;
        const int OFFSET_CHESSBOARD = 7;
        const string FILE_TEST = @"..\..\files\Screenshot.png";

        //Variables
        Image<Gray, Byte> Image_chessboard;
        Image<Bgr, Byte> Image_webcam;
        Image<Bgr, Byte> Image_transformed;
        Image<Gray, Byte> Image_filtered;
        Image<Bgr, Byte> Image_laser;
        Graphics Drawings;
        Capture Webcam;
        bool Calibrated_perspective = false;
        bool Calibrated_laser = false;
        bool Marking_spot = false;
        bool Mouse_down = false;
        Rectangle Spot;
        Hsv Color_spot;

        HomographyMatrix Transformation_matrix;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            frm_webcam.Text = "Webcam";
            frm_webcam.Show();
            frm_transformed.Text = "Transformed";
            frm_transformed.Show();
            frm_filtered.Text = "Filtered";
            frm_filtered.Show();
            frm_laser.Text = "Laser";
            //frm_laser.box_image.SizeMode = PictureBoxSizeMode.CenterImage;
            frm_laser.Show();


            //Positions
            if (frm_webcam.Location.X + frm_webcam.Width + frm_transformed.Width < Screen.PrimaryScreen.Bounds.Width)
            {
                //frm_transformed.StartPosition = FormStartPosition.Manual;
                frm_transformed.Location = new Point(frm_webcam.Location.X + frm_webcam.Width, frm_webcam.Location.Y);
            }
            if (frm_transformed.Location.X + frm_transformed.Width + frm_filtered.Width < Screen.PrimaryScreen.Bounds.Width)
            {
                //frm_filtered.StartPosition = FormStartPosition.Manual;
                frm_filtered.Location = new Point(frm_transformed.Location.X + frm_transformed.Width, frm_transformed.Location.Y);
            }

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
                lbl_info.Text = "Webcam not found. Using testmode";
                Calibrated_perspective = true;
                Application.Idle += new EventHandler(Testmode);
            }
        }

        private void btn_recalibrate_perspective_Click(object sender, EventArgs e)
        {
            Calibrated_perspective = Calibrate_perspective();
        }

        private bool Calibrate_perspective()
        {
            if (Image_chessboard == null)
            {//Chessboard-image not loaded yet
                //Load (with same size as original)
                Image_chessboard = new Image<Gray, Byte>(Properties.Resources.Chessboard).Resize(Image_webcam.Width, Image_webcam.Height, Emgu.CV.CvEnum.INTER.CV_INTER_AREA);
            }

            //Display chessboard
            box_final.BackColor = Color.Black;
            box_final.SizeMode = PictureBoxSizeMode.CenterImage;
            box_final.Image = Image_chessboard.Resize(box_final.Width - 2 * OFFSET_CHESSBOARD, box_final.Height - 2 * OFFSET_CHESSBOARD, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC).ToBitmap();

            //Get corner-points of original and captured chessboard
            Size size_p = new Size(N_CHESSFIELDS_X - 1, N_CHESSFIELDS_Y - 1);
            Emgu.CV.CvEnum.CALIB_CB_TYPE calibrations = Emgu.CV.CvEnum.CALIB_CB_TYPE.ADAPTIVE_THRESH | Emgu.CV.CvEnum.CALIB_CB_TYPE.NORMALIZE_IMAGE | Emgu.CV.CvEnum.CALIB_CB_TYPE.FILTER_QUADS;
            PointF[] corners_dst = CameraCalibration.FindChessboardCorners(Image_chessboard, size_p, calibrations);
            PointF[] corners_src = CameraCalibration.FindChessboardCorners(Image_webcam.Convert<Gray, Byte>(), size_p, calibrations);
            if (corners_src == null || corners_dst == null) return false; //Chessboard not found

            //Get matrix for transformation
            Transformation_matrix = CameraCalibration.FindHomography(corners_src, corners_dst, Emgu.CV.CvEnum.HOMOGRAPHY_METHOD.DEFAULT, 1);

            //Clear box_final
            box_final.BackColor = Color.Black;
            box_final.SizeMode = PictureBoxSizeMode.StretchImage;
            box_final.Image = null;

            return true; //Successful
        }

        private void btn_calibrate_laser_Click(object sender, EventArgs e)
        {
            btn_calibrate_laser.Enabled = false;
            btn_recalibrate_perspective.Enabled = false;

            //Start marking mode
            box_final.Image = Image_transformed.ToBitmap();
            box_final.Cursor = Cursors.Cross;
            Marking_spot = true;

            //-> Rest is done in box_final_MouseDown(), box_final_MouseMove() and box_final_MouseUp()
        }

        private void box_final_MouseDown(object sender, MouseEventArgs e)
        {
            if (!Marking_spot) return; //Not in marking mode

            Mouse_down = true;

            //Set start position
            Spot.Location = e.Location;
        }

        private Rectangle Norm_rectangle(Rectangle rect)
        {
            //  p1 ___
            //    |   |
            //    '---'p2
            Point p1, p2;
            Rectangle output = new Rectangle();

            //Get origin points
            p1 = rect.Location;
            p2 = new Point(rect.X + rect.Width, rect.Y + rect.Height);

            //Recalculate points
            if (p1.X > p2.X)
            {
                output.X = p2.X;
                output.Width = p1.X - p2.X;
            }
            else
            {
                output.X = p1.X;
                output.Width = p2.X - p1.X;
            }
            if (p1.Y > p2.Y)
            {
                output.Y = p2.Y;
                output.Height = p1.Y - p2.Y;
            }
            else
            {
                output.Y = p1.Y;
                output.Height = p2.Y - p1.Y;
            }

            return output;
        }

        private void box_final_MouseMove(object sender, MouseEventArgs e)
        {
            if (!Marking_spot) return; //Not in marking mode
            if (!Mouse_down) return;

            //Clear
            //Drawings.Clear(box_final.BackColor);
            //box_final.Image = Image_transformed.ToBitmap();
            Drawings.DrawImage(Image_transformed.Resize(box_final.Width, box_final.Height, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC).ToBitmap(), 0, 0);

            //Set size with current position
            Spot.Width = e.X - Spot.X;
            Spot.Height = e.Y - Spot.Y;

            Drawings.DrawRectangle(Pens.White, Norm_rectangle(Spot));
        }

        private void box_final_MouseUp(object sender, MouseEventArgs e)
        {
            if (!Marking_spot) return; //Not in marking mode

            Mouse_down = false;

            //Get scale factors
            float factor_x = (float)Image_transformed.Width / box_final.Width;
            float factor_y = (float)Image_transformed.Height / box_final.Height;
            Spot.X = (int)(factor_x * Spot.X);
            Spot.Y = (int)(factor_y * Spot.Y);
            Spot.Width = (int)(factor_x * Spot.Width);
            Spot.Height = (int)(factor_y * Spot.Height);

            //Get average color (HSV) of the spot
            Color_spot = Image_transformed.GetSubRect(Norm_rectangle(Spot)).Convert<Hsv, Byte>().GetAverage();
            //Reset spot position and size
            Spot = new Rectangle();

            //Stop marking mode
            box_final.Image = null;
            Drawings.Clear(box_final.BackColor);
            box_final.Cursor = Cursors.Default;
            Marking_spot = false;

            btn_calibrate_laser.Enabled = true;
            btn_recalibrate_perspective.Enabled = true;
            Calibrated_laser = true;
        }

        private void Testmode(object sender, EventArgs e)
        {
            if (Marking_spot) return; //In marking mode

            if (!File.Exists(FILE_TEST))
            {
                lbl_info.Text = "Webcam and test file not found.";
                return;
            }

            frm_webcam.box_image.BackColor = Color.Gray;

            //Load test image
            Image_transformed = new Image<Bgr, Byte>(FILE_TEST);

            //Clear box_final
            box_final.Image = null;
            box_final.BackColor = Color.Black;

            btn_calibrate_laser.Enabled = true;

            if (Calibrated_laser)
            {
                Filter();
                Draw(Find_point());
            }

            //Display images
            if (Image_webcam != null) frm_webcam.box_image.Image = Image_webcam.ToBitmap();
            if (Image_transformed != null) frm_transformed.box_image.Image = Image_transformed.ToBitmap();
            if (Image_filtered != null) frm_filtered.box_image.Image = Image_filtered.ToBitmap();
            if (Image_laser != null) frm_laser.box_image.Image = Image_laser.ToBitmap();

            //Simulate 30Fps
            System.Threading.Thread.Sleep(33);
        }

        private void Show_cam(object sender, EventArgs e)
        {
            if (Marking_spot) return; //In marking mode

            //Load  webcam image
            Image_webcam = Webcam.QueryFrame();

            if (Calibrated_perspective)
            {
                //Transform image
                Bgr color_outside = new Bgr(Color.Red); //Detect/change later
                Image_transformed = Image_webcam.WarpPerspective(Transformation_matrix, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC, Emgu.CV.CvEnum.WARP.CV_WARP_FILL_OUTLIERS, color_outside);

                btn_calibrate_laser.Enabled = true;

                if (Calibrated_laser)
                {
                    Filter();
                    Draw(Find_point());
                }
            }
            else
            {
                Calibrated_perspective = Calibrate_perspective();
                if (Calibrated_perspective) btn_recalibrate_perspective.Enabled = true;
            }

            //Display images
            if (Image_webcam != null) frm_webcam.box_image.Image = Image_webcam.ToBitmap();
            if (Image_transformed != null) frm_transformed.box_image.Image = Image_transformed.ToBitmap();
            if (Image_filtered != null) frm_filtered.box_image.Image = Image_filtered.ToBitmap();
            if (Image_laser != null) frm_laser.box_image.Image = Image_laser.ToBitmap();
        }

        private void Filter()
        {
            //Create thresholds
            Hsv threshold_lower = new Hsv(Color_spot.Hue - 25, 100, 100);
            Hsv threshold_higher = new Hsv(Color_spot.Hue + 25, 240, 240);

            //Blur image and find colors between thresholds
            Image_filtered = Image_transformed.Convert<Hsv, Byte>().SmoothBlur(20, 20).InRange(threshold_lower, threshold_higher);

            //Reduce size of the spot
            Image_filtered = Image_filtered.Erode(4);
        }

        private Rectangle Find_point()
        {
            float circle_x;
            float circle_y;
            float diameter;

            //Find Circles
            CircleF[] circles = Image_filtered.HoughCircles(
            new Gray(180), //The higher threshold of the two passed to Canny edge detector (the lower one will be twice smaller)
            new Gray(6), //Accumulator threshold at the center detection stage
            1.0, //Resolution of the accumulator used to detect centers of the circles
            10.0, //Min distance
            2, //Min radius
            20 //Max radius
            )[0]; //Get the circles from the first channel

            //Calculate coordinates and diameter of first circle
            circle_x = circles[0].Center.X - circles[0].Radius;
            circle_y = circles[0].Center.Y - circles[0].Radius;
            diameter = 2 * circles[0].Radius;

            if (circles.Length > 0)
            {
                //Get subpicture with laser
                Image_laser = Image_transformed.GetSubRect(new Rectangle((int)circle_x, (int)circle_y, (int)diameter, (int)diameter));
                //Return circle as Rectangle
                return new Rectangle((int)circle_x, (int)circle_y, (int)diameter, (int)diameter);
            }

            //No circle
            return new Rectangle((int)circle_x, (int)circle_y, (int)diameter, (int)diameter);
        }

        private void Draw(Rectangle circle)
        {
            //Clear image
            Drawings.Clear(box_final.BackColor);

            if (circle.X == -1 && circle.Y == -1 && circle.Width == -1 && circle.Height == -1) return; //No circle

            Pen pen_circle = new Pen(Color.DarkBlue, 3);

            //Get scale factors
            float circle_x = circle.X;
            float circle_y = circle.Y;
            float factor_x = (float)box_final.Width / Image_filtered.Width;
            float factor_y = (float)box_final.Height / Image_filtered.Height;

            //Convert coordinates for picturebox box_final
            circle_x *= factor_x;
            circle_y *= factor_y;

            lbl_info.Text = circle_x.ToString() + " " + circle_y.ToString();

            Drawings.DrawEllipse(pen_circle, circle_x, circle_y, circle.Width + pen_circle.Width, circle.Height + pen_circle.Width);

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

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Dispose();
        }
    }
}