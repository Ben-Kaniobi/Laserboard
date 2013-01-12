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
        // Forms
        Form2 frm_webcam = new Form2();
        Form2 frm_transformed = new Form2();
        Form2 frm_filtered = new Form2();
        Form2 frm_laser = new Form2();

        // Constants
        const int N_CHESSFIELDS_X = 8;                              // Horizontal number of fields on chessboard
        const int N_CHESSFIELDS_Y = 6;                              // Vertical number of fields on chessboard
        const int OFFSET_CHESSBOARD = 7;                            // Width of black frame around chessboard
        const string FILE_TEST = @"..\..\files\Screenshot.png";     // Testfile when no webcam was found

        // Flags
        bool Calibrated_perspective = false;                        // Indicates if perspective already is calibrated
        bool Calibrated_laser = false;                              // Indicates if laser already is calibrated
        bool Marking_spot = false;                                  // Indicates if program currently is in laser marking mode (selecting spot for calibrating laser)
        bool Mouse_down = false;                                    // Indicates if mouse button is down, used for laser marking mode

        // Variables
        Capture Webcam;                                             // Webcam
        Image<Gray, Byte> Image_chessboard;                         // Image with chessboard for calibration
        Image<Bgr, Byte> Image_webcam;                              // Image with current webcam frame
        Image<Bgr, Byte> Image_transformed;                         // Image with current webcam frame, transformed
        Image<Gray, Byte> Image_filtered;                           // Image with current webcam frame, transformed and filtered
        Image<Bgr, Byte> Image_laser;                               // Image with current laser spot
        HomographyMatrix Transformation_matrix;                     // Calculated matrix for perspective transformation
        Rectangle Spot;                                             // Selected laserspot
        Hsv Color_spot;                                             // Average color of selected spot
        Graphics Drawings;                                          // Graphics used to draw circles
        Point Point_old;                                            // Points used to draw sublines
        Pen Pen_laser = new Pen(Color.DarkBlue, 3);                 // Pen that draws the lines

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
            frm_laser.Show();


            // Align secondary windows side by side
            if (frm_webcam.Location.X + frm_webcam.Width + frm_transformed.Width < Screen.PrimaryScreen.Bounds.Width)
            {
                frm_transformed.Location = new Point(frm_webcam.Location.X + frm_webcam.Width, frm_webcam.Location.Y);
            }
            if (frm_transformed.Location.X + frm_transformed.Width + frm_filtered.Width < Screen.PrimaryScreen.Bounds.Width)
            {
                frm_filtered.Location = new Point(frm_transformed.Location.X + frm_transformed.Width, frm_transformed.Location.Y);
            }
            if (frm_filtered.Location.X + frm_filtered.Width + frm_laser.Width < Screen.PrimaryScreen.Bounds.Width)
            {
                frm_laser.Location = new Point(frm_filtered.Location.X + frm_filtered.Width, frm_filtered.Location.Y);
            }

            lbl_info.Text = "";

            // Create graphics to draw on box_final
            Drawings = box_final.CreateGraphics();
            Drawings.SmoothingMode = SmoothingMode.AntiAlias;

            try
            {
                // Capture Webcam
                Webcam = new Capture();
                Application.Idle += new EventHandler(Show_cam);
            }
            catch
            {
                // Webcam not found, display an error message
                lbl_Cam_not_found.Show();
            }
        }

        private void btn_recalibrate_perspective_Click(object sender, EventArgs e)
        {
            Calibrated_perspective = false;
        }

        private bool Calibrate_perspective()
        {
            if (Image_chessboard == null)
            {
                // Chessboard-image not loaded yet
                // Load and scale to size of webcam image
                Image_chessboard = new Image<Gray, Byte>(Properties.Resources.Chessboard).Resize(Image_webcam.Width, Image_webcam.Height, Emgu.CV.CvEnum.INTER.CV_INTER_AREA);
            }

            // Display chessboard
            box_final.BackColor = Color.Black;
            box_final.SizeMode = PictureBoxSizeMode.CenterImage;
            box_final.Image = Image_chessboard.Resize(box_final.Width - 2 * OFFSET_CHESSBOARD, box_final.Height - 2 * OFFSET_CHESSBOARD, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC).ToBitmap();
            
            // Get corner points of original and captured chessboard
            Size size_p = new Size(N_CHESSFIELDS_X - 1, N_CHESSFIELDS_Y - 1);
            Emgu.CV.CvEnum.CALIB_CB_TYPE calibrations = Emgu.CV.CvEnum.CALIB_CB_TYPE.ADAPTIVE_THRESH | Emgu.CV.CvEnum.CALIB_CB_TYPE.NORMALIZE_IMAGE | Emgu.CV.CvEnum.CALIB_CB_TYPE.FILTER_QUADS;
            PointF[] corners_dst = CameraCalibration.FindChessboardCorners(Image_chessboard, size_p, calibrations);
            PointF[] corners_src = CameraCalibration.FindChessboardCorners(Image_webcam.Convert<Gray, Byte>(), size_p, calibrations);
            if (corners_src == null || corners_dst == null) return false; // Chessboard not found

            // Get matrix for transformation
            Transformation_matrix = CameraCalibration.FindHomography(corners_src, corners_dst, Emgu.CV.CvEnum.HOMOGRAPHY_METHOD.DEFAULT, 1);

            // Clear box_final
            box_final.BackColor = Color.Black;
            box_final.Image = null;
            Drawings.Clear(box_final.BackColor);

            // Set size mode back to image stretch
            box_final.SizeMode = PictureBoxSizeMode.StretchImage;

            return true; // Successful
        }

        private void btn_calibrate_laser_Click(object sender, EventArgs e)
        {
            btn_calibrate_laser.Enabled = false;
            btn_recalibrate_perspective.Enabled = false;

            // Start marking mode
            box_final.Image = Image_transformed.ToBitmap();
            box_final.Cursor = Cursors.Cross;
            Marking_spot = true;

            // -> Rest is done in box_final_MouseDown(), box_final_MouseMove() and box_final_MouseUp()
        }

        private void box_final_MouseDown(object sender, MouseEventArgs e)
        {
            if (!Marking_spot) return; // Not in marking mode

            Mouse_down = true;

            // Set start position
            Spot.Location = e.Location;
        }

        private Rectangle Norm_rectangle(Rectangle rect)
        {
            //  p1 ___
            //    |   |
            //    '---'p2
            Point p1, p2;
            Rectangle output = new Rectangle();

            // Get origin points
            p1 = rect.Location;
            p2 = new Point(rect.X + rect.Width, rect.Y + rect.Height);

            // Recalculate points
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
            if (!Marking_spot) return; // Not in marking mode
            if (!Mouse_down) return;

            // Display transformed image, so the user can select a spot for calibrating the laser
            Drawings.DrawImage(Image_transformed.Resize(box_final.Width, box_final.Height, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC).ToBitmap(), 0, 0);

            // Set size with current position
            Spot.Width = e.X - Spot.X;
            Spot.Height = e.Y - Spot.Y;

            Drawings.DrawRectangle(Pens.White, Norm_rectangle(Spot));
        }

        private void box_final_MouseUp(object sender, MouseEventArgs e)
        {
            if (!Marking_spot) return; // Not in marking mode

            Mouse_down = false;

            // Get scale factors
            float factor_x = (float)Image_transformed.Width / box_final.Width;
            float factor_y = (float)Image_transformed.Height / box_final.Height;
            Spot.X = (int)(factor_x * Spot.X);
            Spot.Y = (int)(factor_y * Spot.Y);
            Spot.Width = (int)(factor_x * Spot.Width);
            Spot.Height = (int)(factor_y * Spot.Height);
            if (Spot.Width * Spot.Height <= 0) return; // Return if spot had no area

            // Get average color (HSV) of the spot
            Color_spot = Image_transformed.GetSubRect(Norm_rectangle(Spot)).Convert<Hsv, Byte>().GetAverage();
            // Reset spot position and size
            Spot = new Rectangle();

            // Stop marking mode
            box_final.Image = null;
            Drawings.Clear(box_final.BackColor);
            box_final.Cursor = Cursors.Default;
            Marking_spot = false;

            btn_calibrate_laser.Enabled = true;
            btn_recalibrate_perspective.Enabled = true;
            Calibrated_laser = true;
        }

        private void Show_cam(object sender, EventArgs e)
        {
            if (Marking_spot) return; // In marking mode

            // Load  webcam image
            Image_webcam = Webcam.QueryFrame();

            if (Calibrated_perspective)
            {
                // Transform image
                Bgr color_outside = new Bgr(Color.Red); // Detect/change later
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

            // Display images
            if (Image_webcam != null) frm_webcam.box_image.Image = Image_webcam.ToBitmap();
            if (Image_transformed != null) frm_transformed.box_image.Image = Image_transformed.ToBitmap();
            if (Image_filtered != null) frm_filtered.box_image.Image = Image_filtered.ToBitmap();
            if (Image_laser != null) frm_laser.box_image.Image = Image_laser.ToBitmap();
        }

        private void Filter()
        {
            // Create thresholds
            Hsv threshold_lower = new Hsv(Color_spot.Hue - 25, 100, 100);
            Hsv threshold_higher = new Hsv(Color_spot.Hue + 25, 240, 240);

            // Blur image and find colors between thresholds
            Image_filtered = Image_transformed.Convert<Hsv, Byte>().SmoothBlur(20, 20).InRange(threshold_lower, threshold_higher);

            // Increase size of the spot and remove possible hole where color was too bright
            Image_filtered = Image_filtered.Dilate(5);
        }

        private Rectangle Find_point()
        {
            int circle_x;
            int circle_y;
            int diameter;
            Rectangle rect;

            // Find Circles
            CircleF[] circles = Image_filtered.HoughCircles(
            new Gray(180),      // The higher threshold of the two passed to Canny edge detector (the lower one will be twice smaller)
            new Gray(6),        // Accumulator threshold at the center detection stage
            1.0,                // Resolution of the accumulator used to detect centers of the circles
            10.0,               // Min distance
            2,                  // Min radius
            20                  // Max radius
            )[0];               // Get the circles from the first channel

            if (circles.Length > 0)
            {
                // Calculate coordinates and diameter of first circle
                circle_x = (int)circles[0].Center.X - (int)circles[0].Radius;
                circle_y = (int)circles[0].Center.Y - (int)circles[0].Radius;
                diameter = 2 * (int)circles[0].Radius;

                // Create a rectangle that is 10 bigger on each side
                rect = new Rectangle(circle_x - 10, circle_y - 10, diameter + 20, diameter + 20);

                // Make sure the whole rectangle is inside the image
                if (rect.X + rect.Width > Image_transformed.Width) rect.Width = Image_transformed.Width - rect.X; // Width max. to right image border
                if (rect.Y + rect.Height > Image_transformed.Height) rect.Height = Image_transformed.Height - rect.Y; // Height max. to bottom image bordor
                if (rect.X < 0)
                {
                    rect.Width += rect.X; // Reduce width by the same amount that X is too small
                    rect.X = 0;
                }
                if (rect.Y < 0)
                {
                    rect.Height += rect.Y; // Reduce height by the same amount that Y is too small
                    rect.Y = 0;
                }

                // Get subpicture with laser
                Image_laser = Image_transformed.GetSubRect(rect);

                // Stretch image 10 times because it's not possible to set interpolation method for picturebox
                Image_laser = Image_laser.Resize(10, Emgu.CV.CvEnum.INTER.CV_INTER_AREA); // Stretch with interpolation mode area, so pixels are visible

                // Return circle as Rectangle
                return new Rectangle(circle_x, circle_y, diameter, diameter);
            }
            // No circle
            return new Rectangle(-1, -1, -1, -1);
        }

        private void Draw(Rectangle circle)
        {
            if (circle.X == -1 || circle.Y == -1 || circle.Width == -1 || circle.Height == -1) // No circle
            {
                // Reset point and return
                Point_old = new Point(-1, -1);
                return;
            }

            // Get scale factors
            float factor_x = (float)box_final.Width / Image_filtered.Width;
            float factor_y = (float)box_final.Height / Image_filtered.Height;

            // Create a point adjusted for the picturebox size
            Point circle_point = new Point((int)(circle.X * factor_x), (int)(circle.Y * factor_y));

            ////Drawings.DrawEllipse(pen_circle, circle_x, circle_y, circle.Width + pen_circle.Width, circle.Height + pen_circle.Width);

            if (Point_old.X == -1 || Point_old.Y == -1)
            {
                // Fist point
                Point_old = circle_point;
            }
            else
            {
                // Draw line from last point to current point
                Drawings.DrawLine(Pen_laser, Point_old, circle_point);

                // Save current point
                Point_old = circle_point;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Dispose();
        }
    }
}