using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

namespace WpfKinectSkeleton.Model
{
    class KinectContext : DbContext
    {
        public DbSet<JointData> jointData { get;set; }
    }
}
