using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;

namespace WpfKinectSkeleton
{
    
    public partial class ResultWindow : Window
    {
        public ExamData examData { get; set; }

        public ResultWindow( ExamData _data ) : this() {
            
            this.examData = _data;
            showColumnChart();
        }

        public ResultWindow()
        {
            InitializeComponent();
        }


        internal void ClearLines()
        {
            var lgc = new Collection<IPlotterElement>();
            foreach (var x in plotter.Children)
            {
                if (x is LineGraph || x is ElementMarkerPointsGraph)
                    lgc.Add(x);
            }

            foreach (var x in lgc)
            {
                plotter.Children.Remove(x);
            }
        }

        internal void showColumnChart()
        {

            ClearLines();


            List<int> xAxisSource = new List<int>();
            List<double> yAxisSource = new List<double>();

            int i = 0;
            
            foreach (JointData joint in this.examData.Data) 
            {
                //xAxisSource[i] = joint.DataTime;
                xAxisSource.Add(i);
                yAxisSource.Add(joint.Y);
                i++;
            }

            var xEnumSrc = new EnumerableDataSource<int>(xAxisSource);
            var yEnumSrc = new EnumerableDataSource<double>(yAxisSource);

            //set the mappings

            xEnumSrc.SetXMapping(x => x);
            yEnumSrc.SetYMapping(y => y);

            //combine into CompositeDataSource
            CompositeDataSource compositeSource = new CompositeDataSource(xEnumSrc, yEnumSrc);

            //draw the graph

            plotter.AddLineGraph(compositeSource);

        }

    }
}




