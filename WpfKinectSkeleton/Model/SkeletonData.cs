using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.IO;
using System.Diagnostics;
using WpfKinectSkeleton.Model;

namespace WpfKinectSkeleton
{
    /*
    public partial class JointData 
    {
        public int JointDataID { get; set; }
        public TimeSpan _time { get; set; }
        public Joint _joint { get; set; }

        public JointData(TimeSpan elapsed, Joint joint) 
        {
            this._time = elapsed;
            this._joint = joint;
        }
    }
    */

    public partial class JointData 
    {
        public int JointDataID { get; set; }
        public TimeSpan DataTime { get; set; }
        public JointType JointType { get; set; }
        public JointTrackingState TrackingState { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public JointData(TimeSpan elapsed, JointType _type, JointTrackingState _state, double _x, double _y, double _z)
        {
            this.DataTime = elapsed;
            this.JointType = _type;
            this.TrackingState = _state;
            this.X = _x;
            this.Y = _y;
            this.Z = _z;
        }
    }

    public partial class ExamData
    {
        public virtual ICollection<JointData> Data { get; set; }
        private DateTime _time { get; set; }

        public ExamData()
        {
            this.Data = new HashSet<JointData>();
            this._time = DateTime.Now;
        }


        private KinectContext AddToContext(KinectContext context, JointData entity, int count, int commitCount, bool recreateContext)
        {
            context.Set<JointData>().Add(entity);

            if (count % commitCount == 0)
            {
                context.SaveChanges();
                if (recreateContext)
                {
                    context.Dispose();
                    context = new KinectContext();
                    context.Configuration.AutoDetectChangesEnabled = false;
                }
            }

            return context;
        }

        public void spillData()
        {
            
            string fullPath = System.AppDomain.CurrentDomain.BaseDirectory + "data.csv";

            StreamWriter coordinatesStream = new StreamWriter(fullPath);
            coordinatesStream.WriteLine("Time,JointType,TrackingState,Position.X,Position.Y,Position.Z");

            foreach (JointData joint in this.Data)
            {
                coordinatesStream.WriteLine(joint.DataTime + "," + joint.JointType + "," + joint.TrackingState +
                        "," + joint.X + "," + joint.Y + "," + joint.Z);
            }
            coordinatesStream.Close();

            /*
            KinectContext context = null;
            try
            {
                context = new KinectContext();
                context.Configuration.AutoDetectChangesEnabled = false;

                int count = 0;
                foreach (JointData joint in this.Data)
                {
                    ++count;
                    context = AddToContext(context, joint, count, 1000, true);
                }

                context.SaveChanges();
            }
            finally
            {
                if (context != null)
                    context.Dispose();
            }
             */
        }
    }
}
