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
using PathAnimation;

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
        

        // Bookkeeping functions for hand-made animations.
        private List<DoubleAnimationUsingPath> activePathAnimations =
            new List<DoubleAnimationUsingPath>();
        private void BeginPathAnimations()
        {
            if (activePathAnimations != null)
                foreach (DoubleAnimationUsingPath anim in activePathAnimations)
                    anim.Begin();
        }
        private void StopPathAnimations()
        {
            if (activePathAnimations != null)
                foreach (DoubleAnimationUsingPath anim in activePathAnimations)
                    anim.Stop();
        }

        // Bookkeeping functions for objects attached to hand-made animations.
        private List<UIElement> activeAnimatedObjects = new List<UIElement>();
        private void AddActiveAnimatedObjects()
        {
            if (activeAnimatedObjects != null)
                foreach (UIElement element in activeAnimatedObjects)
                    if (! AnimationCanvas.Children.Contains(element))
                        AnimationCanvas.Children.Add(element);
        }
        private void RemoveActiveAnimatedObjects()
        {
            if (activeAnimatedObjects != null)
                foreach (UIElement element in activeAnimatedObjects)
                    AnimationCanvas.Children.Remove(element);
        }


        protected override void SetStatus(string status)
        {
            string newStatus = status;

            if (!canDisplay(newStatus))
                newStatus = statusNames[0];

            if (activeStoryboard != null)
                activeStoryboard.Stop();
            StopPathAnimations();
            activePathAnimations.Clear();
            RemoveActiveAnimatedObjects();
            activeAnimatedObjects.Clear();

            activeStoryboard = StoryboardForStatus(newStatus);
            if (activeStoryboard != null)
                activeStoryboard.Begin();
            else
            {
                SetupAnimationsForStatus(newStatus);
                AddActiveAnimatedObjects();
                BeginPathAnimations();
            }

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

        private static string[] storyboardStatusNames = 
        {
            "normal",
            "hidden",
            "alert",
            "throb",
            "rainbow",
            "forward",
            "backward"
        };

        private static string[] pathAnimationStatusNames = 
        {
           "violin"
        };

        private static string[] statusNames = 
        {
            "normal",
            "hidden",
            "alert",
            "throb",
            "rainbow",
            "forward",
            "backward",
            "violin"
        };
        private static List<string> storyboardStatusList = new List<string>(storyboardStatusNames);
        private static List<string> pathAnimationStatusList = new List<string>(pathAnimationStatusNames);
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

        protected void SetupAnimationsForStatus(string status)
        {
            if (status == "violin")
                SetupViolinAnimations();
        }

        private void SetupViolinAnimations()
        {
            Image violinImage = new Image();
            violinImage.Height = 100;
            violinImage.Width = 100;
            string myURI = Application.Current.Host.Source.ToString();
            string imageBase = myURI.Substring(0, myURI.IndexOf("ClientBin"));
            string uriString = imageBase + "images/Violin.png";
            Uri imageSourceURI = new Uri(uriString, UriKind.Absolute);
            violinImage.Source = new System.Windows.Media.Imaging.BitmapImage(imageSourceURI);
            activeAnimatedObjects.Add(violinImage);

            RotateTransform rotation = new RotateTransform();
            violinImage.RenderTransform = rotation;

            Storyboard sb = new Storyboard();
            DoubleAnimation animRot = new DoubleAnimation();
            animRot.BeginTime = TimeSpan.Zero;
            animRot.Duration = TimeSpan.FromSeconds(1.0);
            animRot.From = 0.0;
            animRot.To = 360.0;
            animRot.RepeatBehavior = RepeatBehavior.Forever;
            Storyboard.SetTarget(animRot, rotation);
            Storyboard.SetTargetProperty(animRot, new PropertyPath("Angle"));
            sb.Children.Add(animRot);
            activeStoryboard = sb;
            activeStoryboard.Begin();

            DoubleAnimationUsingPath animX;
            animX = new DoubleAnimationUsingPath();
            animX.BeginTime = TimeSpan.Zero;
            animX.Duration = TimeSpan.FromSeconds(2.0);
            animX.RepeatBehavior = RepeatBehavior.Forever;
            animX.PathGeometry = PathGeometry;
            animX.Source = PathAnimationSource.X;
            animX.Target = violinImage;
            animX.TargetProperty = new PropertyPath("(Canvas.Left)");
            animX.Tolerance = 30;

            DoubleAnimationUsingPath animY;
            animY = new DoubleAnimationUsingPath();
            animY.BeginTime = TimeSpan.Zero;
            animY.Duration = TimeSpan.FromSeconds(2.0);
            animX.RepeatBehavior = RepeatBehavior.Forever;
            animY.PathGeometry = PathGeometry;
            animY.Source = PathAnimationSource.Y;
            animY.Target = violinImage;
            animY.TargetProperty = new PropertyPath("(Canvas.Top)");
            animY.Tolerance = 30;

            activePathAnimations.Add(animX);
            activePathAnimations.Add(animY);

            activeStoryboard = ForwardStoryboard;
            activeStoryboard.Begin();
        }

        public DataPath()
        {
            InitializeComponent();
        }
    }
}
