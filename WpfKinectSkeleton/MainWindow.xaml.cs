using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using Coding4Fun.Kinect.Wpf;
using System.IO;
using System.Diagnostics;

namespace WpfKinectSkeleton
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensor Kinect;

        private bool bFlag = false;

        /// <summary>
        /// This array will store the Skeletons
        /// received with each SkeletonFrame
        /// </summary>
        private Skeleton[] skeletonData = new Skeleton[6];

        /// <summary>
        /// The skeletons drawn on the scene.
        /// </summary>
        private Dictionary<int, SkeletonDrawing> drawnSkeletons;

        /// <summary>
        /// Window Timers.
        /// </summary>
        DispatcherTimer _timer;
        Stopwatch sw;
        TimeSpan _time;

        ExamData CurrentExamData;    

        public MainWindow()
        {
            InitializeComponent();

            string message = "Get Ready...";
            _time = TimeSpan.FromSeconds(10);
            
            sw = new Stopwatch();

            _timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                tbTime.Text = message + _time.Seconds.ToString();
                if (_time == TimeSpan.Zero)
                {
                    _timer.Stop();
                    message = null;

                    if (!bFlag)
                    {
                        tbTime.Text = "GO!";
                        sw.Start();
                        // exam duration 15 sec
                        _time = TimeSpan.FromSeconds(15);
                        _timer.Start();
                    }
                    else 
                    {
                        sw.Stop();
                        tbTime.Text = String.Format("Completed (captured {0} events)!", CurrentExamData.Data.Count);
                        
                        // Save data to file
                        CurrentExamData.spillData();

                        // display graph
                        new ResultWindow(CurrentExamData).Show();
                        this.Close();
                    }
                    
                    bFlag = !bFlag;
                }
                _time = _time.Add(TimeSpan.FromSeconds(-1));
            }, Application.Current.Dispatcher);

            _timer.Start();

            drawnSkeletons = new Dictionary<int, SkeletonDrawing>();
            CurrentExamData = new ExamData();
            KinectSensorChooser.KinectSensorChanged += new DependencyPropertyChangedEventHandler(KinectSensorChooser_KinectSensorChanged);


        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        void KinectSensorChooser_KinectSensorChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                OpenKinect((KinectSensor)e.NewValue);
            }

            if (e.OldValue != null)
            {
                StopKinect((KinectSensor)e.OldValue);
            }
        }

        /// <summary>
        /// Stops the kinect.
        /// </summary>
        /// <param name="kinectSensor">The kinect sensor.</param>
        private void StopKinect(KinectSensor kinectSensor)
        {
            kinectSensor.Stop();
        }

        /// <summary>
        /// Initialize Kinect.
        /// </summary>
        /// <param name="newKinect">The new kinect.</param>
        private void OpenKinect(KinectSensor newKinect)
        {
            Kinect = newKinect;
            newKinect.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            // Activate the SkeletonStream with default smoothing.
            newKinect.SkeletonStream.Enable();
            // Subscribe to the SkeletonFrameReady event to know when data is available
            newKinect.SkeletonFrameReady += Kinect_SkeletonFrameReady;
            // Starts the sensor
            newKinect.Start();
            //Set the angle
            newKinect.ElevationAngle = Convert.ToInt32(0);
        }

        /// <summary>
        /// Handles the SkeletonFrameReady event of the newKinect control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Microsoft.Kinect.SkeletonFrameReadyEventArgs"/> instance containing the event data.</param>
        void Kinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            // use flag not to track movements, on timer flag will change to true.
            if (bFlag == false) return; 

            // Opens the received SkeletonFrame
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                // Skeleton frame might be null if we are too late or if the Kinect has been stopped
                if (skeletonFrame == null)
                    return;

                // Copies the data in a Skeleton array (6 items) 
                skeletonFrame.CopySkeletonDataTo(skeletonData);

                // Retrieves Skeleton objects with Tracked state
                var trackedSkeletons = skeletonData.Where(s => s.TrackingState == SkeletonTrackingState.Tracked);

                // By default, assume all the drawn skeletons are inactive
                foreach (SkeletonDrawing skeleton in drawnSkeletons.Values)
                    skeleton.Status = ActivityState.Inactive;

                foreach (Skeleton trackedSkeleton in trackedSkeletons)
                {
                    SkeletonDrawing skeletonDrawing;
                    // Checks if the tracked skeleton is already drawn.
                    if (!drawnSkeletons.ContainsKey(trackedSkeleton.TrackingId))
                    {
                        // If not, create a new drawing on our canvas
                        skeletonDrawing = new SkeletonDrawing(this.SkeletonCanvas);
                        drawnSkeletons.Add(trackedSkeleton.TrackingId, skeletonDrawing);
                    }
                    else
                    {
                        skeletonDrawing = drawnSkeletons[trackedSkeleton.TrackingId];
                    }
                
                    // Update the drawing
                    foreach (Joint joint in trackedSkeleton.Joints)
                    {

                        // Transforms a SkeletonPoint to a ColorImagePoint
                        //var colorPoint = Kinect.MapSkeletonPointToColor(joint.Position, Kinect.ColorStream.Format);
                        var colorPoint = Kinect.CoordinateMapper.MapSkeletonPointToColorPoint(joint.Position, Kinect.ColorStream.Format);
                        // Scale the ColorImagePoint position to the current window size
                        var point = new Point((int)colorPoint.X / 640.0 * this.ActualWidth, (int)colorPoint.Y / 480.0 * this.ActualHeight);
                        // update the position of that joint
                        skeletonDrawing.Update(joint.JointType, point, joint.Position.Z);

                        JointData data = new JointData(sw.Elapsed, joint.JointType, joint.TrackingState, joint.Position.X, joint.Position.Y, joint.Position.Z);
                        //CurrentExamData.Data.Add(new JointData(sw.Elapsed, joint));  
                        if (joint.JointType == JointType.ElbowLeft || joint.JointType == JointType.ElbowRight ) 
                            CurrentExamData.Data.Add(data);
                    }

                    skeletonDrawing.Status = ActivityState.Active;
                }


                foreach (SkeletonDrawing skeleton in drawnSkeletons.Values)
                {
                    // Erase all the still inactive drawing. It means they are not tracked anymore.
                    if (skeleton.Status == ActivityState.Inactive)
                        skeleton.Erase();
                }

                this.InvalidateVisual();
            }
        }

        /// <summary>
        /// Updates the specified drawn skeleton with the new positions
        /// </summary>
        /// <param name="skeleton">The skeleton source.</param>
        /// <param name="drawing">The target drawing.</param>
        private void Update(Skeleton skeleton, SkeletonDrawing drawing)
        {

        }
    }
}
