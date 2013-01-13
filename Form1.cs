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
        const string STRING_NO_WEBCAM = "Webcam not found";         // String which is displayed if no webcam was found
        const string STRING_PAUSE_MODE = "[Paused]";                // String which is displayed while in pause mode
        const string STRING_CALIBRATION_INFO = "[L] Laser calibration    [P] Perspective recalibration    [F] Fullscreen toggle    [I] Info toggle";

        // Flags
        bool Ready_for_calibration_l;                               // Is set as soon as the transformed image is ready for the laser calibration (is never reset)
        bool Perspective_calibrated = false;                        // Indicates if perspective already is calibrated
        bool Laser_calibrated = false;                              // Indicates if laser already is calibrated
        bool Calibrating_laser = false;                             // Indicates if program currently is in laser calibration mode
        bool Pause_mode = false;                                    // Doesn't analyse the image if pause mode is active
        bool Mouse_down = false;                                    // Indicates if mouse button is down, used for laser calibration mode

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
        FormWindowState Last_window_state = FormWindowState.Normal; // Used to return from fullscreen to same window state as before entering fullscreen

        public Form1()
        {
            InitializeComponent();

            // Change parent of the label, needed for correct transparency
            lbl_Info.Parent = box_Final;
            lbl_Info.BackColor = Color.Transparent;

            // Add some eventhandlers
            GotFocus += new EventHandler(Form1_GotFocus);
            LostFocus += new EventHandler(Form1_LostFocus);
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

            // Clear info label text
            lbl_Info.Text = "";

            // Create graphics to draw on box_final
            Drawings = box_Final.CreateGraphics();
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
                lbl_Info.Text = STRING_NO_WEBCAM;
                // Change dock to whole window and text align to middle&center, so the message is in the middle of the window
                lbl_Info.Dock = DockStyle.Fill;
                lbl_Info.TextAlign = ContentAlignment.MiddleCenter;
            }
        }

        private bool Calibrate_perspective()
        {
            if (Image_chessboard == null)
            {
                // Chessboard-image not loaded yet
                // Load and scale to size of webcam image
                Image_chessboard = new Image<Gray, Byte>(Properties.Resources.Chessboard).Resize(Image_webcam.Width, Image_webcam.Height, Emgu.CV.CvEnum.INTER.CV_INTER_AREA);
            }

            // Remove Text
            lbl_Info.Text = "";

            // Display chessboard
            box_Final.BackColor = Color.White;
            box_Final.SizeMode = PictureBoxSizeMode.CenterImage;
            box_Final.Image = Image_chessboard.Resize(box_Final.Width - 2 * OFFSET_CHESSBOARD, box_Final.Height - 2 * OFFSET_CHESSBOARD, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC).ToBitmap();
            
            // Get corner points of original and captured chessboard
            Size size_p = new Size(N_CHESSFIELDS_X - 1, N_CHESSFIELDS_Y - 1);
            Emgu.CV.CvEnum.CALIB_CB_TYPE calibrations = Emgu.CV.CvEnum.CALIB_CB_TYPE.ADAPTIVE_THRESH | Emgu.CV.CvEnum.CALIB_CB_TYPE.NORMALIZE_IMAGE | Emgu.CV.CvEnum.CALIB_CB_TYPE.FILTER_QUADS;
            PointF[] corners_dst = CameraCalibration.FindChessboardCorners(Image_chessboard, size_p, calibrations);
            PointF[] corners_src = CameraCalibration.FindChessboardCorners(Image_webcam.Convert<Gray, Byte>(), size_p, calibrations);
            if (corners_src == null || corners_dst == null) return false; // Chessboard not found

            // Get matrix for transformation
            Transformation_matrix = CameraCalibration.FindHomography(corners_src, corners_dst, Emgu.CV.CvEnum.HOMOGRAPHY_METHOD.DEFAULT, 1);

            // Clear box_final
            box_Final.BackColor = Color.Black;
            box_Final.Image = null;
            Drawings.Clear(box_Final.BackColor);

            // Set size mode back to image stretch
            box_Final.SizeMode = PictureBoxSizeMode.StretchImage;

            return true; // Successful
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            switch(e.KeyCode)
            {
                case Keys.I: // Info

                    // Toggle info text
                    if (lbl_Info.Text == STRING_CALIBRATION_INFO)
                    {
                        lbl_Info.Text = "";
                    }
                    else
                    {
                        lbl_Info.Text = STRING_CALIBRATION_INFO;
                    }
                    break;

                case Keys.P: // Perspective recalibration

                    // Don't continue if laser calibration mode is active
                    if (Calibrating_laser) break;

                    // Reset calibration flag
                    Perspective_calibrated = false;
                    break;

                case Keys.L: // Laser calibration

                    // Don't continue if not yet ready
                    if (!Ready_for_calibration_l) break;
                    // Don't continue if laser calibration mode already is active
                    if(Calibrating_laser) break;

                    // Reset flag
                    Laser_calibrated = false;

                    // Start calibration mode
                    box_Final.Image = Image_transformed.ToBitmap();
                    Cursor.Show();
                    Calibrating_laser = true;
                    // -> Rest is done in box_final_MouseDown(), box_final_MouseMove() and box_final_MouseUp()
                    break;

                case Keys.F: // Toggle fullscreen

                    if (FormBorderStyle == FormBorderStyle.None)
                    {
                        // Switch fullscreen mode off
                        FormBorderStyle = FormBorderStyle.Sizable;
                        WindowState = Last_window_state;
                        TopMost = false;
                    }
                    else
                    {
                        // Remember window state and set to normal to avoid problems
                        Last_window_state = WindowState;
                        WindowState = FormWindowState.Normal;

                        // Switch fullscreen mode on
                        FormBorderStyle = FormBorderStyle.None;
                        WindowState = FormWindowState.Maximized;
                        TopMost = true;
                    }
                    break;

                case Keys.Escape:

                    // Stop calibration mode
                    box_Final.Image = null;
                    Drawings.Clear(box_Final.BackColor);
                    Cursor.Hide();
                    Calibrating_laser = false;
                    break;
            }
        }

        private void box_final_MouseDown(object sender, MouseEventArgs e)
        {
            if (!Calibrating_laser) return; // Not in calibration mode

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
            if (!Calibrating_laser) return; // Not in calibration mode
            if (!Mouse_down) return;

            // Display transformed image, so the user can select a spot for calibrating the laser
            Drawings.DrawImage(Image_transformed.Resize(box_Final.Width, box_Final.Height, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC).ToBitmap(), 0, 0);

            // Set size with current position
            Spot.Width = e.X - Spot.X;
            Spot.Height = e.Y - Spot.Y;

            Drawings.DrawRectangle(Pens.White, Norm_rectangle(Spot));
        }

        private void box_final_MouseUp(object sender, MouseEventArgs e)
        {
            if (!Calibrating_laser) return; // Not in calibration mode

            Mouse_down = false;

            // Get scale factors
            float factor_x = (float)Image_transformed.Width / box_Final.Width;
            float factor_y = (float)Image_transformed.Height / box_Final.Height;
            Spot.X = (int)(factor_x * Spot.X);
            Spot.Y = (int)(factor_y * Spot.Y);
            Spot.Width = (int)(factor_x * Spot.Width);
            Spot.Height = (int)(factor_y * Spot.Height);
            if (Spot.Width * Spot.Height <= 0) return; // Return if spot had no area

            // Get average color (HSV) of the spot
            Color_spot = Image_transformed.GetSubRect(Norm_rectangle(Spot)).Convert<Hsv, Byte>().GetAverage();
            // Reset spot position and size
            Spot = new Rectangle();

            // Stop calibration mode
            box_Final.Image = null;
            Drawings.Clear(box_Final.BackColor);
            Cursor.Hide();
            Calibrating_laser = false;

            // Set calibration successfully completed flag
            Laser_calibrated = true;
        }

        private void Show_cam(object sender, EventArgs e)
        {
            if (Calibrating_laser) return; // In calibration mode

            // Load  webcam image
            Image_webcam = Webcam.QueryFrame();

            if (Perspective_calibrated)
            {
                // Set color of areas that are out of camera view to black.
                //// To do / enhancement: Set the color to a specified color to detect and avoid out of view perspectives
                Bgr color_outside = new Bgr(Color.Black);

                // Transform image
                Image_transformed = Image_webcam.WarpPerspective(Transformation_matrix, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC, Emgu.CV.CvEnum.WARP.CV_WARP_FILL_OUTLIERS, color_outside);

                // The first time, display calibration info
                if (!Ready_for_calibration_l) lbl_Info.Text = STRING_CALIBRATION_INFO;

                Ready_for_calibration_l = true;

                if (Laser_calibrated)
                {
                    // Create binary image
                    Filter();

                    // Find point and draw, if analysing is't paused
                    if(!Pause_mode) Draw(Find_point());
                }
            }
            else
            {
                Perspective_calibrated = Calibrate_perspective();
            }

            // Display images
            if (Image_webcam != null) frm_webcam.box_Image.Image = Image_webcam.ToBitmap();
            if (Image_transformed != null) frm_transformed.box_Image.Image = Image_transformed.ToBitmap();
            if (Image_filtered != null) frm_filtered.box_Image.Image = Image_filtered.ToBitmap();
            if (Image_laser != null) frm_laser.box_Image.Image = Image_laser.ToBitmap();
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

            // Decrease size again a little, makes it smoother
            Image_filtered = Image_filtered.Erode(3);
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

        private float Distance(Point pt1, Point pt2)
        {
            // Calculate legs of right triangle
            int a = pt2.X - pt1.X;
            int b = pt2.Y - pt1.Y;

            // Return hypotenuse calculated with pythagoras
            return (float)Math.Sqrt(a * a + b * b);
        }

        private void Draw(Rectangle circle)
        {
            if (circle.Width >= 50 || circle.Height >= 50 || circle.X == -1 || circle.Y == -1 || circle.Width == -1 || circle.Height == -1) // Circle too big or no circle
            {
                // Reset point and return
                Point_old = new Point(-1, -1);
                return;
            }

            // Get scale factors
            float factor_x = (float)box_Final.Width / Image_filtered.Width;
            float factor_y = (float)box_Final.Height / Image_filtered.Height;

            // Create a point adjusted for the picturebox size
            Point circle_point = new Point((int)(circle.X * factor_x), (int)(circle.Y * factor_y));

            if (Point_old.X == -1 || Point_old.Y == -1 || Distance(Point_old, circle_point) > box_Final.Width / 4)
            {
                // Point_old not yet set, ordistance between the two points is grater than a quarter of the image width

                // Save First point
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

        private void Form1_GotFocus(object sender, EventArgs e)
        {
            // Resume image analysing
            Pause_mode = false;
            lbl_Info.Text = "";
        }

        private void Form1_LostFocus(object sender, EventArgs e)
        {
            // Pause image analysing
            Pause_mode = true;
            lbl_Info.Text = STRING_PAUSE_MODE;

            // If in fullscreen mode, turn it off
            if (FormBorderStyle == FormBorderStyle.None)
            {
                // Switch fullscreen mode off
                FormBorderStyle = FormBorderStyle.Sizable;
                WindowState = Last_window_state;
                TopMost = false;
            }
        }

        private void box_Final_SizeChanged(object sender, EventArgs e)
        {
            if (box_Final.Width * box_Final.Height <= 0) return; // Window minimised

            // Reset flag to start a new calibration
            Perspective_calibrated = false;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Dispose();
        }

        private void box_Final_MouseEnter(object sender, EventArgs e)
        {
            // Hide cursor on entering box_final, but not if in laser calibration mode
            if (Calibrating_laser) return;
            Cursor.Hide();
        }

        private void box_Final_MouseLeave(object sender, EventArgs e)
        {
            // Show cursor again on leaving box_final
            Cursor.Show();
        }
    }
}