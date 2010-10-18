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
            }
        }

        private static void OnWaypointsChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            DataPath path = sender as DataPath;
            path.UpdatePathData();
            path.SetStatus(path.CurrentStatus);
        }


        public static readonly DependencyProperty ThicknessProperty =
        DependencyProperty.RegisterAttached(
            "Thickness",
            typeof(double),
            typeof(DataPath),
            new PropertyMetadata(5.0, new PropertyChangedCallback(OnThicknessChanged)));

        public double Thickness
        {
            get
            {
                return (double)GetValue(ThicknessProperty);
            }

            set
            {
                SetValue(ThicknessProperty, value);
            }
        }

        private static void OnThicknessChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            DataPath path = sender as DataPath;
            path.RenderedPath.StrokeThickness = (double)path.GetValue(ThicknessProperty);
            path.SetStatus(path.CurrentStatus);
        }


        // Bookkeeping functions for hand-made storyboards.
        private List<Storyboard> activeAdditionalStoryboards =
            new List<Storyboard>();
        private void BeginAdditionalStoryboards()
        {
            if (activeAdditionalStoryboards != null)
                foreach (Storyboard sb in activeAdditionalStoryboards)
                    sb.Begin();
        }
        private void StopAdditionalStoryboards()
        {
            if (activeAdditionalStoryboards != null)
                foreach (Storyboard sb in activeAdditionalStoryboards)
                    sb.Stop();
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
                    if (!AnimationCanvas.Children.Contains(element))
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
            StopAdditionalStoryboards();
            activeAdditionalStoryboards.Clear();
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
                BeginAdditionalStoryboards();
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

        private PathGeometry GetReversePathGeometry()
        {
            PathGeometry geo = new PathGeometry();
            PathFigure fig = new PathFigure();
            PolyLineSegment polyline = new PolyLineSegment();
            foreach (Point point in Waypoints.Reverse())
            {
                polyline.Points.Add(point);
            }
            fig.Segments.Clear();
            fig.Segments.Add(polyline);
            geo.Figures.Add(fig);
            return geo;
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
            "violin",
            "violin-forward",
            "violin-backward",
            "puzzle",
            "puzzle-forward",
            "puzzle-backward",
            "balls-forward",
            "balls-backward",
            "balls-both"
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
            "violin",
            "violin-forward",
            "violin-backward",
            "puzzle",
            "puzzle-forward",
            "puzzle-backward",
            "balls-forward",
            "balls-backward",
            "balls-both"
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
                SetupViolinAnimations("forward", "normal");
            if (status == "violin-forward")
                SetupViolinAnimations("forward", "forward");
            if (status == "violin-backward")
                SetupViolinAnimations("backward", "backward");

            if (status == "puzzle")
                SetupPuzzleAnimations("forward", "normal");
            if (status == "puzzle-forward")
                SetupPuzzleAnimations("forward", "forward");
            if (status == "puzzle-backward")
                SetupPuzzleAnimations("backward", "backward");

            if (status == "balls-forward")
                SetupBallsAnimations("forward", "normal");
            if (status == "balls-backward")
                SetupBallsAnimations("backward", "normal");
            if (status == "balls-both")
                SetupBallsAnimations("both", "normal");
        }


        private void SetupImageAnimationAlongPath(Storyboard sb,
                                                  FrameworkElement element, 
                                                  string direction, 
                                                  Boolean rotate,
                                                  double delay)
        {
            const double pathDuration = 2.0;

            // Want to translate object to center it along path.
            TransformGroup tg = new TransformGroup();
            TranslateTransform translate = new TranslateTransform();
            translate.X = -1.0 * element.Width / 2.0;
            translate.Y = -1.0 * element.Height / 2.0;
            tg.Children.Add(translate);

            PathGeometry path;
            if (direction == "forward")
                path = PathGeometry;
            else
                path = GetReversePathGeometry();

            activeAnimatedObjects.Add(element);

            if (rotate)
            {
                RotateTransform rotation = new RotateTransform();
                tg.Children.Add(rotation);

                DoubleAnimation animRot = new DoubleAnimation();
                animRot.BeginTime = TimeSpan.FromSeconds(delay);
                animRot.Duration = TimeSpan.FromSeconds(pathDuration / 2.0);
                animRot.From = 0.0;
                animRot.To = 360.0;
                animRot.RepeatBehavior = RepeatBehavior.Forever;
                Storyboard.SetTarget(animRot, rotation);
                Storyboard.SetTargetProperty(animRot, new PropertyPath("Angle"));
                sb.Children.Add(animRot);
            }
            element.RenderTransform = tg;

            DoubleAnimationUsingPath animX;
            animX = new DoubleAnimationUsingPath();
            animX.BeginTime = TimeSpan.FromSeconds(delay);
            animX.Duration = TimeSpan.FromSeconds(pathDuration);
            animX.RepeatBehavior = RepeatBehavior.Forever;
            animX.PathGeometry = path;
            animX.Source = PathAnimationSource.X;
            animX.Target = element;
            animX.TargetProperty = new PropertyPath("(Canvas.Left)");
            animX.Tolerance = 30;

            DoubleAnimationUsingPath animY;
            animY = new DoubleAnimationUsingPath();
            animY.BeginTime = TimeSpan.FromSeconds(delay);
            animY.Duration = TimeSpan.FromSeconds(pathDuration);
            animY.RepeatBehavior = RepeatBehavior.Forever;
            animY.PathGeometry = path;
            animY.Source = PathAnimationSource.Y;
            animY.Target = element;
            animY.TargetProperty = new PropertyPath("(Canvas.Top)");
            animY.Tolerance = 30;

            activePathAnimations.Add(animX);
            activePathAnimations.Add(animY);
        }

        private void SetupViolinAnimations(string violinDir,
                                           string pathDir)
        {
            Image violinImage = new Image();
            violinImage.Height = 100;
            violinImage.Width = 100;
            string myURI = Application.Current.Host.Source.ToString();
            string imageBase = myURI.Substring(0, myURI.IndexOf("ClientBin"));
            string uriString = imageBase + "images/Violin.png";
            Uri imageSourceURI = new Uri(uriString, UriKind.Absolute);
            violinImage.Source = new System.Windows.Media.Imaging.BitmapImage(imageSourceURI);

            Storyboard sb = new Storyboard();
            SetupImageAnimationAlongPath(sb, violinImage, violinDir, true, 0.0);
            activeAdditionalStoryboards.Add(sb);
            BeginAdditionalStoryboards();

            if (pathDir == "forward")
                activeStoryboard = ForwardStoryboard;
            else if (pathDir == "backward")
                activeStoryboard = BackwardStoryboard;
            if (activeStoryboard != null)
                activeStoryboard.Begin();
        }

        private void SetupPuzzleAnimations(string puzzleDir,
                                           string pathDir)
        {
            Image puzzleImage = new Image();
            puzzleImage.Height = 100;
            puzzleImage.Width = 100;
            string myURI = Application.Current.Host.Source.ToString();
            string imageBase = myURI.Substring(0, myURI.IndexOf("ClientBin"));
            string uriString = imageBase + "images/puzzlePiece.png";
            Uri imageSourceURI = new Uri(uriString, UriKind.Absolute);
            puzzleImage.Source = new System.Windows.Media.Imaging.BitmapImage(imageSourceURI);

            Storyboard sb = new Storyboard();
            SetupImageAnimationAlongPath(sb, puzzleImage, puzzleDir, false, 0.0);
            activeAdditionalStoryboards.Add(sb);
            BeginAdditionalStoryboards();

            if (pathDir == "forward")
                activeStoryboard = ForwardStoryboard;
            else if (pathDir == "backward")
                activeStoryboard = BackwardStoryboard;
            if (activeStoryboard != null)
                activeStoryboard.Begin();
        }


        private Ellipse OneBall()
        {
            Ellipse ball = new Ellipse();
            ball.Stroke = new SolidColorBrush(Colors.Black);
            ball.StrokeThickness = Thickness;
            ball.Fill = new SolidColorBrush(Colors.White);
            ball.Width = Thickness * 4.0;
            ball.Height = Thickness * 4.0;

            return ball;
        }

        private void SetupBallsAnimations(string ballsDir,
                                          string pathDir)
        {
            const int numBalls = 4;
            Storyboard sb = new Storyboard();


            if ((ballsDir == "both") ||
                (ballsDir == "forward"))
            {
                for (int index = 0; index < numBalls; index++)
                    SetupImageAnimationAlongPath(sb, OneBall(), "forward", false, 0.5 * index);
            }

            if ((ballsDir == "both") ||
                (ballsDir == "backward"))
            {
                for (int index = 0; index < numBalls; index++)
                    SetupImageAnimationAlongPath(sb, OneBall(), "backward", false, 0.5 * index);
            }

            activeAdditionalStoryboards.Add(sb);
            BeginAdditionalStoryboards();

            if (pathDir == "forward")
                activeStoryboard = ForwardStoryboard;
            else if (pathDir == "backward")
                activeStoryboard = BackwardStoryboard;
            if (activeStoryboard != null)
                activeStoryboard.Begin();
        }


        public DataPath()
        {
            InitializeComponent();
        }
    }
}
