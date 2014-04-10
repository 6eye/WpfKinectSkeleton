using System.Collections.Generic;
using System.Linq;
using Microsoft.Kinect;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;

namespace WpfKinectSkeleton
{
    public class SkeletonBoneDrawing
    {
        public JointType Joint1 { get; set; }
        public JointType Joint2 { get; set; }
        public Point Point1 { get; set; }
        public Point Point2 { get; set; }

        public SkeletonBoneDrawing(JointType joint1, JointType joint2)
        {
            Joint1 = joint1;
            Joint2 = joint2;
        }
    }

    public enum ActivityState
    {
        Active,
        Inactive,
        Erased
    }

    public class SkeletonDrawing
    {
        private const int JOINT_WIDTH = 10;
        private const int HEAD_WIDTH = 30;
        private const int BONES_THICKNESS = 5;

        private Canvas canvas;

        private IDictionary<JointType, Ellipse> Joints;
        private IDictionary<SkeletonBoneDrawing, Line> Bones;

        public int TrackingId { get; set; }

        public ActivityState Status { get; set; }

        public SkeletonDrawing(Canvas canvas)
        {
            this.canvas = canvas;

            InitJoints();
            InitBones();
        }

        public void Erase()
        {
            Status = ActivityState.Erased;
            foreach (Ellipse ellipse in Joints.Values)
            {
                canvas.Children.Remove(ellipse);
            }
            foreach (Line line in Bones.Values)
            {
                canvas.Children.Remove(line);
            }
            Joints.Clear();
            Bones.Clear();
        }

        public void Update(JointType jointType, Point point, float distance = 2.0f)
        {
            if (Status == ActivityState.Erased)
            {
                InitJoints();
                InitBones();
            }

            var ellipse = Joints[jointType];

            // Scale the width
            ellipse.Width = ellipse.Height = JOINT_WIDTH * (2.0f / distance != 0 ? distance : 2.0f);

            Canvas.SetZIndex(ellipse, 5000 - (int)distance * 1000);

            Canvas.SetTop(Joints[jointType], (point.Y - Joints[jointType].Height / 2));
            Canvas.SetLeft(Joints[jointType], (point.X - Joints[jointType].Width / 2));

            foreach (SkeletonBoneDrawing boneDrawing in Bones.Keys.Where(b => b.Joint1 == jointType || b.Joint2 == jointType))
            {
                var line = Bones[boneDrawing];
                if (boneDrawing.Joint1 == jointType)
                {
                    line.X1 = point.X;
                    line.Y1 = point.Y;
                }
                else
                {
                    line.X2 = point.X;
                    line.Y2 = point.Y;
                }
            }         
        }

        #region Private methods

        private void InitBones()
        {
            Bones = new Dictionary<SkeletonBoneDrawing, Line>
            {
                { new SkeletonBoneDrawing(JointType.Head,JointType.ShoulderCenter), GenerateLine()},
                { new SkeletonBoneDrawing(JointType.ShoulderCenter,JointType.ShoulderLeft), GenerateLine()},
                { new SkeletonBoneDrawing(JointType.ShoulderCenter,JointType.ShoulderRight), GenerateLine()},
                { new SkeletonBoneDrawing(JointType.ShoulderCenter,JointType.Spine), GenerateLine()},
                { new SkeletonBoneDrawing(JointType.Spine,JointType.ShoulderLeft), GenerateLine()},
                { new SkeletonBoneDrawing(JointType.Spine,JointType.ShoulderRight), GenerateLine()},
                { new SkeletonBoneDrawing(JointType.Spine,JointType.HipCenter), GenerateLine()},
                { new SkeletonBoneDrawing(JointType.Spine,JointType.HipLeft), GenerateLine()},
                { new SkeletonBoneDrawing(JointType.Spine,JointType.HipRight), GenerateLine()},
                { new SkeletonBoneDrawing(JointType.HipCenter,JointType.HipLeft), GenerateLine()},
                { new SkeletonBoneDrawing(JointType.HipCenter,JointType.HipRight), GenerateLine()},
                
                { new SkeletonBoneDrawing(JointType.ShoulderLeft,JointType.ElbowLeft), GenerateLine()},
                { new SkeletonBoneDrawing(JointType.ElbowLeft,JointType.WristLeft), GenerateLine()},
                { new SkeletonBoneDrawing(JointType.WristLeft,JointType.HandLeft), GenerateLine()},
                
                { new SkeletonBoneDrawing(JointType.ShoulderRight,JointType.ElbowRight), GenerateLine()},
                { new SkeletonBoneDrawing(JointType.ElbowRight,JointType.WristRight), GenerateLine()},
                { new SkeletonBoneDrawing(JointType.WristRight,JointType.HandRight), GenerateLine()},
                
                { new SkeletonBoneDrawing(JointType.HipRight,JointType.KneeRight), GenerateLine()},
                { new SkeletonBoneDrawing(JointType.KneeRight,JointType.AnkleRight), GenerateLine()},
                { new SkeletonBoneDrawing(JointType.AnkleRight,JointType.FootRight), GenerateLine()},
                
                { new SkeletonBoneDrawing(JointType.HipLeft,JointType.KneeLeft), GenerateLine()},
                { new SkeletonBoneDrawing(JointType.KneeLeft,JointType.AnkleLeft), GenerateLine()},
                { new SkeletonBoneDrawing(JointType.AnkleLeft,JointType.FootLeft), GenerateLine()},
            };

            //foreach (Line line in Bones.Values)
            //    canvas.Children.Add(line);
        }

        private void InitJoints()
        {
            Joints = new Dictionary<JointType, Ellipse>()
            {
                  { JointType.Head,             GenerateEllipse(50)},
                  { JointType.AnkleLeft,        GenerateEllipse()},
                  { JointType.AnkleRight,       GenerateEllipse()},
                  { JointType.ElbowLeft,        GenerateEllipse()},
                  { JointType.ElbowRight,       GenerateEllipse()},
                  { JointType.FootLeft,         GenerateEllipse()},
                  { JointType.FootRight,        GenerateEllipse()},
                  { JointType.HandLeft,         GenerateEllipse()},
                  { JointType.HandRight,        GenerateEllipse()},
                  { JointType.HipCenter,        GenerateEllipse()},
                  { JointType.HipLeft,          GenerateEllipse()},
                  { JointType.HipRight,         GenerateEllipse()},
                  { JointType.KneeLeft,         GenerateEllipse()},
                  { JointType.KneeRight,        GenerateEllipse()},
                  { JointType.ShoulderCenter,   GenerateEllipse()},
                  { JointType.ShoulderRight,    GenerateEllipse()},
                  { JointType.ShoulderLeft,     GenerateEllipse()},
                  { JointType.Spine,            GenerateEllipse()},
                  { JointType.WristLeft,        GenerateEllipse()},
                  { JointType.WristRight,       GenerateEllipse()},
            };

            //foreach (Ellipse ellipse in Joints.Values)
            //    canvas.Children.Add(ellipse);
        }

        private Ellipse GenerateEllipse(int size = JOINT_WIDTH)
        {
            var ellipse = new Ellipse() { Width = size, Height = size, Fill = Brushes.Red };
            canvas.Children.Add(ellipse);
            return ellipse;
        }

        private Line GenerateLine()
        {
            var line = new Line() { StrokeThickness = BONES_THICKNESS, Fill = Brushes.Red , Stroke = Brushes.Black };
            canvas.Children.Add(line);
            return line;
        }

        #endregion
    }
}
