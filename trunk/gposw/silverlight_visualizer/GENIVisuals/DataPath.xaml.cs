using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace GENIVisuals
{
    public partial class DataPath : VisualControl
    {
        public static readonly DependencyProperty WaypointsProperty =
        DependencyProperty.RegisterAttached(
            "Waypoints",
            typeof(PointCollection),
            typeof(DataPath),
            new PropertyMetadata(null, new PropertyChangedCallback(OnWaypointsChanged)));

        public PointCollection Waypoints
        {
            get
            {
                return (PointCollection)GetValue(WaypointsProperty);
            }

            set
            {
                SetValue(WaypointsProperty, value);
                //UpdatePathData();
            }
        }

        private static void OnWaypointsChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            DataPath path = sender as DataPath;
            path.UpdatePathData();
        }


        private void UpdatePathData()
        {
            PathFigure fig = new PathFigure();
            PolyLineSegment polyline = new PolyLineSegment();
            foreach (Point point in Waypoints)
                polyline.Points.Add(point);
            fig.Segments.Add(polyline);
            FigureCollection.Clear();
            FigureCollection.Add(fig);
        }

        private static string[] statusNames = 
        {
            "normal",
            "hidden",
            "alert",
            "throb",
            "rainbow",
            "forward",
            "backward"
        };
        private static List<string> displayableStatusList = new List<string>(statusNames);

        protected override List<string> getDisplayableStatusList()
        {
            return displayableStatusList;
        }

        public override Boolean canDisplay(string status)
        {
            List<string> displayableStatusList = getDisplayableStatusList();
            return displayableStatusList.Contains(status);
        }

        protected override Storyboard StoryboardForStatus(string status)
        {
            if (status == "hidden")
                return HiddenStoryboard;
            if (status == "alert")
                return AlertStoryboard;
            if (status == "throb")
                return ThrobStoryboard;
            if (status == "rainbow")
                return RainbowStoryboard;
            if (status == "forward")
                return ForwardStoryboard;
            if (status == "backward")
                return BackwardStoryboard;

            return null;
        }

        public DataPath()
        {
            InitializeComponent();
        }
    }
}
