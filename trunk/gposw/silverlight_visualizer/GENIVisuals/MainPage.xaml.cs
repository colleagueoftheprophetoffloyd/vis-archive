using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;
using System.Windows.Controls.DataVisualization.Charting;
using Microsoft.Maps.MapControl;
using Microsoft.Maps.MapControl.Common;
using System.Collections.ObjectModel;
using GENIVisuals.models;
using GENIVisuals.viewClasses;
using System.Json;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;

namespace GENIVisuals
{
    public partial class MainPage : UserControl
    {
        private SessionParameters parameters;
        private string URIParams = "";
        WebClient wc = new WebClient();
        private string phpBase;


        // All the {visuals, nodes, links} we care about.
        private Collection<Visual> visuals = new Collection<Visual>();
        private Dictionary<string, Node> nodes = new Dictionary<string, Node>();
        private Dictionary<string, Link> links = new Dictionary<string, Link>();


        // Objects associated with a particular visual.
        private Dictionary<Visual, VisualElements> elements =
            new Dictionary<Visual, VisualElements>();

        // Keep a chart for each object so that multiple line plots share same surface.
        private Dictionary<Object, Chart> charts = new Dictionary<Object, Chart>();

        // The list of all visuals for updating.
        private Queue<Visual> updateQueue = new Queue<Visual>();
        private Dictionary<Object, Point> nodePoints = new Dictionary<Object, Point>();
        private Dictionary<Point, List<Object>> overlappedObjects = new Dictionary<Point, List<Object>>();
        private Dictionary<Object, UIElement> mapObjects = new Dictionary<Object, UIElement>();
        // Map from data sources (nodes, links) to their associated visuals.
        Dictionary<Object, List<Visual>> visualsForSource = new Dictionary<Object, List<Visual>>();

        private MapLayer overlayLayer = null;
        private Random myRandom = new Random();


        public MainPage(SessionParameters myparams)
        {
            InitializeComponent();

            // Remember session parameters
            parameters = myparams;

#if DEBUG
            parameters.useDebugServer = true;
            parameters.debugServer = "http://ganel.gpolab.bbn.com:17380";
            parameters.slice = "SmartRE15Sep";
            parameters.dbHost = "ganel.gpolab.bbn.com";
            parameters.dbUser = "wzeng";
            parameters.dbPassword = "wzeng";
            parameters.dbName = "wzeng";
#endif
            // Gather up parameters to pass to PHP scripts.
            if ((parameters.slice != null) && (parameters.slice != ""))
            {
                if (URIParams == "")
                    URIParams = "?slice=" + parameters.slice;
                else
                    URIParams += "&slice=" + parameters.slice;
            }
            if ((parameters.dbHost != null) && (parameters.dbHost != ""))
            {
                if (URIParams == "")
                    URIParams = "?server=" + parameters.dbHost;
                else
                    URIParams += "&server=" + parameters.dbHost;
            }
            if ((parameters.dbUser != null) && (parameters.dbUser != ""))
            {
                if (URIParams == "")
                    URIParams = "?dbUsername=" + parameters.dbUser;
                else
                    URIParams += "&dbUsername=" + parameters.dbUser;
            }
            if ((parameters.dbPassword != null) && (parameters.dbPassword != ""))
            {
                if (URIParams == "")
                    URIParams = "?dbPassword=" + parameters.dbPassword;
                else
                    URIParams += "&dbPassword=" + parameters.dbPassword;
            }
            if ((parameters.dbName != null) && (parameters.dbName != ""))
            {
                if (URIParams == "")
                    URIParams = "?db=" + parameters.dbName;
                else
                    URIParams += "&db=" + parameters.dbName;
            }

            // Figure out base URI for PHP scripts.

            if (parameters.useDebugServer)
            {
                phpBase = parameters.debugServer + "/GENIVisuals/bin/php/";
            }
            else
            {
                string myURI = Application.Current.Host.Source.ToString();
                phpBase = myURI.Substring(0, myURI.IndexOf("ClientBin")) + "bin/php/";
            }

            string uri = phpBase + "get_nodes.php" + URIParams;
            // Get list of nodes from PHP script.
            wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(wc_DownloadStringCompleted);
            wc.DownloadStringAsync(new Uri(uri));
        }

        void wc_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null && e.Result != "")
            {
                JsonValue completeResult = JsonPrimitive.Parse(e.Result);
                string resultType = completeResult["returnType"].ToString().Replace('"', ' ').Trim();
                if (resultType == "nodes")
                {
                    LoadNodes(completeResult);
                    wc.DownloadStringAsync(new Uri(phpBase + "get_links.php" + URIParams));
                }
                else if (resultType == "links")
                {
                    LoadLinks(completeResult);
                    wc.DownloadStringAsync(new Uri(phpBase + "get_visuals.php" + URIParams));
                }
                else if (resultType == "visuals")
                {
                    LoadVisuals(completeResult);
                    DisplayVisuals();
                    SetupDataUpdates();
                    updateQueue.Enqueue(null); // setup status updates

                    if (parameters.useBogusData)
                        SetupBogusDataUpdates();
                }
                else if (resultType == "data")
                {
                    Visual vis = e.UserState as Visual;
                    if (vis != null)
                        LoadData(completeResult, vis);
                }
                else if (resultType == "status")
                {
                    LoadStatus(completeResult);
                    //UpdateVisuals();
                }

                sliceLabel.Content = parameters.slice;
            }
            else
            {
                infoLabel.Content = e.Error;
            }
        }


        // Parse node content out of JSON.
        private void LoadNodes(JsonValue completeResult)
        {
            // Forget what we know about nodes.
            nodes.Clear();

            // Loop over list of nodes.
            JsonArray nodesJson = (JsonArray)completeResult["results"];
            foreach (JsonValue nodeJson in nodesJson)
            {
                // Parse node content out of JSON
                Node thisNode = new Node(nodeJson);
                nodes[thisNode.Name] = thisNode;
            }
        }


        // Parse link content out of JSON.
        private void LoadLinks(JsonValue completeResult)
        {
            // Forget what we know about links.
            links.Clear();

            // Loop over list of links.
            JsonArray linksJson = (JsonArray)completeResult["results"];
            foreach (JsonValue linkJson in linksJson)
            {
                Link thisLink = new Link(linkJson);
                links[thisLink.name] = thisLink;
            }
        }

        // Parse visual content out of JSON.
        private void LoadVisuals(JsonValue completeResult)
        {
            visuals.Clear();

            // Loop over list of visuals.
            JsonArray visualsJson = (JsonArray)completeResult["results"];
            foreach (JsonValue visualJson in visualsJson)
            {
                Visual thisVisual = new Visual(visualJson);
                visuals.Add(thisVisual);
            }
        }

        private double distance(Point a, Point b)
        {
            return Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }

        //
        // Parse visuals content out of JSON and remember the
        // requested visual display parameters.
        //
        private void UpdateVisuals()
        {
            overlappedObjects.Clear();
            nodePoints.Clear();

            //group ojbects that are close to each other
            foreach (Object obj in visualsForSource.Keys)
            {
                string objType = visualsForSource[obj][0].objType;
                string objName = visualsForSource[obj][0].objName;
                Location location = GetLocation(objType, objName);
                if (objType == "node")
                {
                    Point newPoint = sliceMap.LocationToViewportPoint(location);
                    //if this new obejct is within a circle of a radius of 20 of an previous object,
                    //add it to the area of the previous object
                    bool added = false;
                    foreach (Point point in overlappedObjects.Keys)
                    {
                        if (distance(newPoint, point) <= 30)
                        {
                            overlappedObjects[point].Add(obj);
                            added = true;
                            continue;
                        }
                    }
                    //otherwise, make it a new point in the overlappedObjects
                    if (!added)
                    {
                        List<Object> newList = new List<object>();
                        newList.Add(obj);
                        overlappedObjects.Add(newPoint, newList);
                    }
                }
            }

            infoLabel.Content = overlappedObjects.Count;

            //for each group of objects that are nearby, spread them over a circle with a radius of 20 around their geometric center.
            foreach (Point point in overlappedObjects.Keys)
            {
                List<Object> objects = overlappedObjects[point];
                if (objects.Count >= 2)
                {
                    double avgX = 0, avgY = 0;
                    for (int i = 0; i < objects.Count; i++)
                    {
                        Object oj = objects[i];
                        string objType = visualsForSource[oj][0].objType;
                        string objName = visualsForSource[oj][0].objName;
                        Point p = sliceMap.LocationToViewportPoint(GetLocation(objType, objName));
                        nodePoints[oj] = p;
                        avgX += p.X;
                        avgY += p.Y;
                    }

                    avgX = avgX / objects.Count;
                    avgY = avgY / objects.Count;

                    //(avgX, avgY) is the center of polygon formed by the nearby objects;
                    double step = 2 * Math.PI / objects.Count;
                    for (int i = 0; i < objects.Count; i++)
                    {
                        Point offset = new Point();
                        double newX, newY;
                        Object oj = objects[i];
                        Point p = nodePoints[oj];
                        newX = avgX + 10 * objects.Count * Math.Cos(step * i + Math.PI / 4);
                        newY = avgY + 10 * objects.Count * Math.Sin(step * i + Math.PI / 4);
                        offset.X = newX - p.X;
                        offset.Y = newY - p.Y;
                        nodePoints[oj] = offset;
                    }
                }
            }

            // Build controls for the data sources and data.
            foreach (Object obj in visualsForSource.Keys)
            {
                string objType = visualsForSource[obj][0].objType;
                string objName = visualsForSource[obj][0].objName;

                Location location = GetLocation(objType, objName);
                if (mapObjects.Keys.Contains(obj) && nodePoints.Keys.Contains(obj)) {
                    UIElement el = mapObjects[obj];
                    if (el != null)
                    {
                        overlayLayer.Children.Remove(el);
                        overlayLayer.AddChild(el, location, nodePoints[obj]);
                        overlayLayer.UpdateLayout();
                    }
                }
            }
        }

        private void DisplayVisuals()
        {
            visualsForSource.Clear();
            elements.Clear();
            updateQueue.Clear();
            overlappedObjects.Clear();
            nodePoints.Clear();

            // Loop over list of visuals.  Group visuals associated with
            // same data source together in a dictionary.
            foreach (Visual thisVisual in visuals)
            {
                elements[thisVisual] = new VisualElements();
                Object dataSource = null;
                if (thisVisual.objType == "node")
                    dataSource = nodes[thisVisual.objName];
                else if (thisVisual.objType == "link")
                    dataSource = links[thisVisual.objName];

                if (dataSource != null)
                {
                    if (!visualsForSource.ContainsKey(dataSource))
                        visualsForSource[dataSource] = new List<Visual>();
                    visualsForSource[dataSource].Add(thisVisual);
                }
            }

            // **** Experimental: Add blank layer
            if (false)
            {
                MapTileLayer blankLayer = new MapTileLayer();
                string myURI = Application.Current.Host.Source.ToString();
                string imageBase = myURI.Substring(0, myURI.IndexOf("ClientBin")) + "images/";
                string imageSourceFormat = imageBase + "blankTile{quadkey}.png";
                TileSource blankTileSource = new TileSource();
                blankTileSource.UriFormat = imageSourceFormat;
                blankLayer.Opacity = 1.0;
                blankLayer.Visibility = Visibility.Visible;
                if (!sliceMap.Children.Contains(blankLayer))
                    sliceMap.Children.Add(blankLayer);
            }

            // Add a layer for drawing overlays (labels, graphs, etc.)
            if (overlayLayer == null)
            {
                overlayLayer = new MapLayer();
                //overlayLayer.SizeChanged += new SizeChangedEventHandler(UpdateVisuals);
                sliceMap.Children.Add(overlayLayer);
            }

            //group ojbects that are close to each other
            foreach (Object obj in visualsForSource.Keys)
            {
                string objType = visualsForSource[obj][0].objType;
                string objName = visualsForSource[obj][0].objName;
                Location location = GetLocation(objType, objName);
                if (objType == "node")
                {
                    Point newPoint = sliceMap.LocationToViewportPoint(location);
                    //if this new obejct is within a circle of a radius of 20 of an previous object,
                    //add it to the area of the previous object
                    bool added = false;
                    foreach (Point point in overlappedObjects.Keys)
                    {
                        if (distance(newPoint, point) <= 30)
                        {
                            overlappedObjects[point].Add(obj);
                            added = true;
                            continue;
                        }
                    }
                    //otherwise, make it a new point in the overlappedObjects
                    if (!added)
                    {
                        List<Object> newList = new List<object>();
                        newList.Add(obj);
                        overlappedObjects.Add(newPoint, newList);
                    }
                }
            }

            infoLabel.Content = overlappedObjects.Count;

            //for each group of objects that are nearby, spread them over a circle with a radius of 20 around their geometric center.
            foreach (Point point in overlappedObjects.Keys)
            {
                List<Object> objects = overlappedObjects[point];
                if (objects.Count >= 2)
                {
                    double avgX = 0, avgY = 0;
                    for (int i = 0; i < objects.Count; i++)
                    {
                        Object oj = objects[i];
                        string objType = visualsForSource[oj][0].objType;
                        string objName = visualsForSource[oj][0].objName;
                        Point p = sliceMap.LocationToViewportPoint(GetLocation(objType, objName));
                        nodePoints[oj] = p;
                        avgX += p.X;
                        avgY += p.Y;
                    }

                    avgX = avgX / objects.Count;
                    avgY = avgY / objects.Count;

                    //(avgX, avgY) is the center of polygon formed by the nearby objects;
                    double step = 2 * Math.PI / objects.Count;
                    for (int i = 0; i < objects.Count; i++)
                    {
                        Point offset = new Point();
                        double newX, newY;
                        Object oj = objects[i];
                        Point p = nodePoints[oj];
                        newX = avgX + 10 * objects.Count * Math.Cos(step * i + Math.PI / 4);
                        newY = avgY + 10 * objects.Count * Math.Sin(step * i + Math.PI / 4);
                        offset.X = newX - p.X;
                        offset.Y = newY - p.Y;
                        nodePoints[oj] = offset;
                    }
                }
            }

            // Build controls for the data sources and data.
            foreach (Object obj in visualsForSource.Keys)
            {
                string objType = visualsForSource[obj][0].objType;
                string objName = visualsForSource[obj][0].objName;
                StackPanel panel = null;
                Point offset = new Point(0, 0);

                foreach (Visual vis in visualsForSource[obj])
                {
                    UIElement control = MakeVisControl(vis);
                    if (vis.positionAdvice.GetValue("XOffset") != null)
                        offset.X = Convert.ToInt32(vis.positionAdvice.GetValue("XOffset"));
                    if (vis.positionAdvice.GetValue("YOffset") != null)
                        offset.Y = Convert.ToInt32(vis.positionAdvice.GetValue("YOffset"));

                    if (control != null)
                    {
                        if ((vis.infoType == "label") ||
                            (vis.infoType == "zoomButton") ||
                            (vis.infoType == "icon") ||
                            (vis.infoType == "scalar") ||
                            (vis.infoType == "lineGraph"))
                        {
                            if (panel == null)
                            {
                                panel = new StackPanel();
                                panel.Opacity = 0.7;
                                panel.Background = new SolidColorBrush(Colors.LightGray);
                            }
                            panel.Children.Add(control);
                        }
                        else if (vis.infoType == "arc")
                        {
                            sliceMap.Children.Add(control);
                        }

                        if ((vis.statQuery != null) && (vis.statQuery != ""))
                            updateQueue.Enqueue(vis);
                        elements[vis].SetProperty(VisualElements.StatusAnimationTargetProperty, control);
                    }
                }

                if (panel != null)
                {
                    Location location = GetLocation(objType, objName);
                    overlayLayer.AddChild(panel, location, nodePoints[obj]);
                    mapObjects.Add(obj, panel);
                }
            }
        }


        //
        // Parse numeric data out of JSON and use to update visual.
        //
        private void LoadData(JsonValue completeResult, Visual vis)
        {
            Stat myStat = elements[vis].GetProperty(VisualElements.StatisticsProperty) as Stat;

            if (myStat != null)
            {
                // Load the data pairs.
                JsonArray dataJson = (JsonArray)completeResult["results"];
                foreach (JsonValue pairJson in dataJson)
                {
                    DateTime time = DateTime.MinValue;
                    double value = 0.0;

                    if (pairJson["time"] != null)
                        time = Convert.ToDateTime(pairJson["time"].ToString().Replace('"', ' ').Trim());
                    if (pairJson["value"] != null)
                        value = Convert.ToDouble(pairJson["value"].ToString().Replace('"', ' ').Trim());

                    // Update value.
                    if (myStat.history > 0)
                        myStat.addValue(time, value);
                    else
                        myStat.currentValue = value;
                }
            }

            // Requeue the query for later update.
            updateQueue.Enqueue(vis);
        }


        //
        // Parse status data out of JSON and use to update affected visuals.
        //
        private void LoadStatus(JsonValue completeResult)
        {
            Collection<StatusInfo> allStatusInfo = new Collection<StatusInfo>();

            // Loop over list of visuals.
            JsonArray statusJson = (JsonArray)completeResult["results"];
            foreach (JsonValue thisStatusJson in statusJson)
                allStatusInfo.Add(new StatusInfo(thisStatusJson));

            foreach (StatusInfo info in allStatusInfo)
                foreach (Visual vis in visuals)
                    if (vis.statusHandle == info.Handle)
                    {
                        StatusInfo oldStatusInfo = elements[vis].GetProperty(VisualElements.StatusProperty) as StatusInfo;
                        string oldStatus = "";
                        if (oldStatusInfo != null)
                            oldStatus = oldStatusInfo.Status;
                        elements[vis].SetProperty(VisualElements.StatusProperty, info);

                        if (oldStatus != info.Status)
                            UpdateStoryboard(vis);
                    }

            // Requeue the query for later update.
            // *** Sleazy to use null Visual for this purpose.
            updateQueue.Enqueue(null);
        }


        //
        // Make a new storyboard to animate status display.
        //
        private void UpdateStoryboard(Visual vis)
        {
            VisualElements info = elements[vis];
            if (info == null)
                return;
            Storyboard oldSb = info.GetProperty(VisualElements.StoryboardProperty) as Storyboard;
            List<UIElement> oldElementList = info.GetProperty(VisualElements.AnimationElementsProperty) as List<UIElement>;
            StatusInfo statusInfo = info.GetProperty(VisualElements.StatusProperty) as StatusInfo;
            string status = "";
            if (statusInfo != null)
                status = statusInfo.Status;


            // Clear out any existing animation.
            info.ClearProperty(VisualElements.StoryboardProperty);
            info.ClearProperty(VisualElements.AnimationElementsProperty);
            if (oldSb != null)
                oldSb.Stop();
            if (oldElementList != null)
                foreach (UIElement element in oldElementList)
                    overlayLayer.Children.Remove(element);



            // Build new animation.
            // This cries out for refactoring.
            Storyboard newSb = null;
            List<UIElement> newElementList = null;
            if (status == "alert")
            {
                newSb = new Storyboard();

                // Build an animation.
                ColorAnimation animation = new ColorAnimation();
                animation.Duration = new Duration(TimeSpan.FromSeconds(0.2));
                animation.From = Colors.LightGray;
                animation.To = Colors.Red;
                animation.AutoReverse = true;
                animation.RepeatBehavior = RepeatBehavior.Forever;
                animation.FillBehavior = FillBehavior.Stop; // Does this mean anything, given above line?

                // Attach to storyboard.
                newSb.Children.Add(animation);

                // Attach to control
                UIElement control = elements[vis].GetProperty(VisualElements.StatusAnimationTargetProperty) as UIElement;
                if (control != null)
                {
                    Storyboard.SetTarget(animation, control);
                    Storyboard.SetTargetProperty(animation, new PropertyPath("(Background).(SolidColorBrush.Color)"));
                }
            }
            else if (status == "rainbow")
            {
                newSb = new Storyboard();

                // Build an animation.
                ColorAnimation animation = new ColorAnimation();
                animation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                animation.From = Colors.Red;
                animation.To = Colors.Purple;
                animation.AutoReverse = false;
                animation.RepeatBehavior = RepeatBehavior.Forever;
                animation.FillBehavior = FillBehavior.Stop; // Does this mean anything, given above line?

                // Attach to storyboard.
                newSb.Children.Add(animation);

                // Attach to control
                UIElement control = elements[vis].GetProperty(VisualElements.StatusAnimationTargetProperty) as UIElement;
                if (control != null)
                {
                    Storyboard.SetTarget(animation, control);
                    Storyboard.SetTargetProperty(animation, new PropertyPath("(Background).(SolidColorBrush.Color)"));
                }
            }
            else if (status == "throb")
            {
                newSb = new Storyboard();

                // Build an animation.
                DoubleAnimation animation = new DoubleAnimation();
                animation.Duration = new Duration(TimeSpan.FromSeconds(0.2));
                animation.From = 1.0;
                animation.To = 0.0;
                animation.AutoReverse = true;
                animation.RepeatBehavior = RepeatBehavior.Forever;
                animation.FillBehavior = FillBehavior.Stop; // Does this mean anything, given above line?

                // Attach to storyboard.
                newSb.Children.Add(animation);

                // Attach to control
                UIElement control = elements[vis].GetProperty(VisualElements.StatusAnimationTargetProperty) as UIElement;
                if (control != null)
                {
                    Storyboard.SetTarget(animation, control);
                    Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity"));
                }
            }
            else if ((vis.objType == "link") &&
                     (vis.infoType == "arc") &&
                     ((status == "forward") || (status == "backward") || (status == "both")))
            {
                newSb = new Storyboard();
                newElementList = new List<UIElement>();

                if ((status == "forward") || (status == "both"))
                {
                    List<UIElement> balls = MakeBalls(newSb, vis, "forward");
                    foreach (UIElement ball in balls)
                        newElementList.Add(ball);
                }

                if ((status == "backward") || (status == "both"))
                {
                    List<UIElement> balls = MakeBalls(newSb, vis, "backward");
                    foreach (UIElement ball in balls)
                        newElementList.Add(ball);
                }
            }

            // Start storyboard and remember for later cleanup.
            if (newSb != null)
            {
                info.SetProperty(VisualElements.StoryboardProperty, newSb);
                if (newElementList != null)
                    info.SetProperty(VisualElements.AnimationElementsProperty,
                                     newElementList);
                newSb.Begin();
            }
        }


        //
        // Build an animation that follows the arc of the specified polyline.
        // Shift points by the specified offset (for centering).
        // If direction is "backward" then follow the arc backward.
        //
        private PointAnimationUsingKeyFrames ArcAnimation(MapPolyline pl, Point offset, string direction)
        {
            PointAnimationUsingKeyFrames animation = new PointAnimationUsingKeyFrames();
            animation.Duration = new Duration(TimeSpan.FromMilliseconds(2000.0));
            animation.AutoReverse = false;
            animation.RepeatBehavior = RepeatBehavior.Forever;
            animation.FillBehavior = FillBehavior.Stop;

            IEnumerable<Location> locations = pl.Locations;
            if (direction == "backward")
                locations = pl.Locations.Reverse();

            // One key frame for each segment(skip first point).
            Point startPoint = sliceMap.LocationToViewportPoint(locations.First());
            int numPoints = pl.Locations.Count();
            double frameDuration = 2000.0 / Convert.ToDouble(numPoints - 1);

            for (int index = 0; index < numPoints; index++)
            {
                Location loc = locations.ElementAt(index);
                LinearPointKeyFrame keyFrame = new LinearPointKeyFrame();
                animation.KeyFrames.Add(keyFrame);
                KeyTime time = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(index * frameDuration));
                keyFrame.KeyTime = time;
                Point point = sliceMap.LocationToViewportPoint(loc);
                keyFrame.Value = new Point(point.X - startPoint.X + offset.X,
                                           point.Y - startPoint.Y + offset.Y);
            }
            return animation;

        }


        //
        // Helper function for making balls for link animation.
        // Return a list of created UI objects.
        //
        private List<UIElement> MakeBalls(Storyboard sb, Visual vis, string direction)
        {
            List<UIElement> result = new List<UIElement>();
            MapPolyline pl = elements[vis].GetProperty(VisualElements.LinkPolyLineProperty) as MapPolyline;
            Location startLoc;

            if ((pl == null) || (pl.Locations == null) || (pl.Locations.Count() == 0))
                return null;

            if (direction == "backward")
                startLoc = pl.Locations.Last();
            else
                startLoc = pl.Locations.First();

            // Make moving balls
            Point offset = new Point(-5, -5); // Offset to center of balls
            for (int i = 0; i < 3; i++)
            {
                // Attach animation to storyboard.
                PointAnimationUsingKeyFrames animation = ArcAnimation(pl, offset, direction);
                animation.BeginTime = new TimeSpan(0, 0, 0, 0, 500 * i); // ms
                sb.Children.Add(animation);

                // Attach a moving ball.
                Ellipse ball = new Ellipse();
                ball.Fill = new SolidColorBrush(Colors.White);
                ball.Width = 10;
                ball.Height = 10;
                Storyboard.SetTarget(animation, ball);
                Storyboard.SetTargetProperty(animation, new PropertyPath(MapLayer.PositionOffsetProperty));
                overlayLayer.AddChild(ball, startLoc, offset);
                result.Add(ball);

                //*** Experiment (delete me)
                if (false)
                {
                    // Build an animation.
                    ColorAnimation animation2 = new ColorAnimation();
                    animation2.Duration = new Duration(TimeSpan.FromSeconds(2.0));
                    animation2.From = Colors.Red;
                    animation2.To = Colors.Blue;
                    animation2.AutoReverse = false;
                    animation2.RepeatBehavior = RepeatBehavior.Forever;
                    sb.Children.Add(animation2);
                    Storyboard.SetTarget(animation2, ball);
                    Storyboard.SetTargetProperty(animation2, new PropertyPath("(Fill).(SolidColorBrush.Color)"));
                }
                //*** End experiment
            }
            return result;
        }


        //
        // Make an arc to represent a link on map.
        //
        private MapPolyline MakeArc(Visual visual, string objType, string objName)
        {
            // Only links have arcs.
            if (objType != "link")
                return null;

            // Get link info.
            Link thisLink = links[objName];
            Location sourceLoc = new Location(nodes[thisLink.sourceNode].Latitude,
                                                nodes[thisLink.sourceNode].Longitude);
            Location destLoc = new Location(nodes[thisLink.destNode].Latitude,
                                            nodes[thisLink.destNode].Longitude);
            if ((sourceLoc == null) || (destLoc == null))
                return null;

            // Build polyline.
            string renderAdvice = "";
            if (visual.renderAdvice != null)
                renderAdvice = visual.renderAdvice.ToLower();
            MapPolyline pl = new MapPolyline();
            //pl.Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
            pl.Stroke = linkBrush;
            pl.StrokeThickness = 5;
            pl.Opacity = 0.5;
            int segs;
            double bendFactor, normalOffset;

            if ((renderAdvice == "cw") ||
                (renderAdvice == "clockwise"))
            {
                segs = 50;
                normalOffset = Math.PI / 2.0;
                bendFactor = 0.1;
            }
            else if ((renderAdvice == "ccw") ||
                        (renderAdvice == "counterclockwise"))
            {
                segs = 50;
                normalOffset = -1.0 * Math.PI / 2.0;
                bendFactor = 0.1;
            }
            else
            {
                segs = 1;
                normalOffset = 0.0;
                bendFactor = 0.0;
            }

            LocationCollection arcPoints = new LocationCollection();
            Point sPoint = sliceMap.LocationToViewportPoint(sourceLoc);
            Point dPoint = sliceMap.LocationToViewportPoint(destLoc);
            double xDist = dPoint.X - sPoint.X;
            double yDist = dPoint.Y - sPoint.Y;
            double length = Math.Sqrt(xDist * xDist + yDist * yDist);
            double normalAngle = Math.Atan2(yDist, xDist) + normalOffset;
            double normalDist = bendFactor * length;
            for (int i = 0; i <= segs; i++)
            {
                double t = Convert.ToDouble(i) / Convert.ToDouble(segs);
                double offset = Math.Sin(Math.PI * t) * normalDist;
                Point interp = new Point(t * xDist + sPoint.X + offset * Math.Cos(normalAngle),
                                            t * yDist + sPoint.Y + offset * Math.Sin(normalAngle));
                arcPoints.Add(sliceMap.ViewportPointToLocation(interp));
            }
            pl.Locations = arcPoints;
            return pl;
        }


        //
        // Return a location for the object (node, link).
        //
        private Location GetLocation(string objType, string objName)
        {
            // Currently only understand nodes and links.
            if (objType == "node")
            {
                Node thisNode = nodes[objName];
                return new Location(thisNode.Latitude, thisNode.Longitude);
            }
            else if (objType == "link")
            {
                Link thisLink = links[objName];
                Location sourceLoc = new Location(nodes[thisLink.sourceNode].Latitude,
                                                  nodes[thisLink.sourceNode].Longitude);
                Location destLoc = new Location(nodes[thisLink.destNode].Latitude,
                                                nodes[thisLink.destNode].Longitude);
                Point sPoint = sliceMap.LocationToViewportPoint(sourceLoc);
                Point dPoint = sliceMap.LocationToViewportPoint(destLoc);
                Point middle = new Point((sPoint.X + dPoint.X) / 2.0,
                                         (sPoint.Y + dPoint.Y) / 2.0);
                return sliceMap.ViewportPointToLocation(middle);
            }
            return null;
        }


        //
        // Create the UI Control that presents data for the requested visualization.
        //
        private UIElement MakeVisControl(Visual vis)
        {
            Object obj = null;
            string objectName = "";
            UIElement control = null;

            // Find the associated object.
            // We understand nodes and links.
            if (vis.objType == "node")
            {
                objectName = vis.objName;
                obj = nodes[objectName];
            }
            else if (vis.objType == "link")
            {
                objectName = vis.objName;
                obj = links[objectName];
            }

            if (obj == null)
                return null;


            // *** For goodness sake, please refactor me so
            // *** that we're not stuck with this big if statement
            // *** and all this logic in one place.

            // Is is a label?
            if (vis.infoType == "label")
            {
                Label label = new Label();
                label.Content = objectName;
                label.Background = new SolidColorBrush(Colors.DarkGray);
                label.Foreground = new SolidColorBrush(Colors.White);
                control = label;
                elements[vis].SetProperty(VisualElements.DataSourceLabelProperty, label);
            }
            else if (vis.infoType == "zoomButton")
            {
                Button button = new Button();
                button.Click += new RoutedEventHandler(labelButtonClick);
                button.Content = objectName;
                control = button;
            }
            // Is it an icon?
            else if ((vis.infoType == "icon") &&
                     (vis.objType == "node"))
            {
                Image image = new Image();
                image.Height = 50;
                image.Width = 50;
                string iconString = nodes[vis.objName].Icon;
                if ((iconString != null) && (iconString != ""))
                {
                    Uri imageSourceURI = new Uri(iconString);
                    image.Source = new System.Windows.Media.Imaging.BitmapImage(imageSourceURI);
                }
                else
                {
                    string myURI = Application.Current.Host.Source.ToString();
                    string imageBase = myURI.Substring(0, myURI.IndexOf("ClientBin")) + "images/";
                    Uri imageSourceURI = new Uri(imageBase + "smile.png");
                    image.Source = new System.Windows.Media.Imaging.BitmapImage(imageSourceURI);
                }
                control = image;
            }
            // Is it an arc (links only)?
            else if (vis.infoType == "arc")
            {
                MapPolyline arc;
                arc = MakeArc(vis, vis.objType, vis.objName);
                control = arc;
                elements[vis].SetProperty(VisualElements.LinkPolyLineProperty, control);
            }
            // Is it a statitics graph?
            else if ((vis.infoType == "scalar") || (vis.infoType == "lineGraph"))
            {
                // Make a new Stat object to hold data
                Stat myStat = new Stat();
                myStat.statType = vis.statType;
                myStat.history = vis.statHistory;

                // Use a progress bar for scalar values or a line graph for time series.
                // Nest it in a horizontal stack panel with a label.
                if (vis.infoType == "scalar")
                {
                    StackPanel panel = new StackPanel();
                    panel.Orientation = Orientation.Horizontal;

                    Label label = new Label();
                    label.Content = vis.statType;

                    ProgressBar pb = new ProgressBar();
                    pb.Width = 100;
                    pb.Height = 20;
                    if (vis.minValue.HasValue)
                        pb.Minimum = vis.minValue.Value;
                    if (vis.maxValue.HasValue)
                        pb.Maximum = vis.maxValue.Value;
                    Binding bind = new Binding("currentValue");
                    bind.Source = myStat;
                    pb.SetBinding(ProgressBar.ValueProperty, bind);

                    panel.Children.Add(label);
                    panel.Children.Add(pb);
                    control = panel;
                }

                if (vis.infoType == "lineGraph")
                {
                    // Make a chart to plot line graphs.
                    // Remember and reuse chart for future
                    // line graphs for this object.
                    Chart ch = null;
                    if (charts.ContainsKey(obj) && charts[obj] != null)
                        ch = charts[obj];
                    else
                    {
                        ch = new Chart();
                        charts[obj] = ch;
                        ch.Width = 300;
                        ch.Height = 200;
                        ch.LegendStyle = NoLegendStyle;
                    }

                    // Create a line series
                    LineSeries line = new LineSeries();
                    line.SetValue(LineSeries.TitleProperty, myStat.statType);
                    ch.Series.Add(line);

                    // Bind data
                    Binding bind = new Binding("values");
                    bind.Source = myStat;
                    line.SetBinding(LineSeries.ItemsSourceProperty, bind);
                    line.IndependentValuePath = "Key";
                    line.DependentValuePath = "Value";

                    // Set axis properties.  *** Rewrite this search
                    DateTimeAxis timeAxis = null;
                    foreach (Axis thisAxis in ch.Axes)
                        if (thisAxis.Name == "time")
                            timeAxis = thisAxis as DateTimeAxis;
                    if (timeAxis == null)
                    {
                        timeAxis = new DateTimeAxis()
                        {
                            Orientation = AxisOrientation.X,
                            AxisLabelStyle = TimeLabelStyle,
                            Name = "time"
                        };
                        ch.Axes.Add(timeAxis);
                    }

                    control = ch;
                }

                if (control != null)
                    elements[vis].SetProperty(VisualElements.StatisticsProperty, myStat);
            }
            // Other types of visual are unknown.
            else
            {
                infoLabel.Content = "Unknown visual type.";
                throw new NotImplementedException();
            }


            return control;
        }


        //
        // ** Experimental **
        //
        // Callback for a click on a zoomButton object.
        // Create a floatable child window with a new map.
        //
        // TODO: See if this can be done without the (relatively)
        // heavyweight effect of a new MainPage.  Also, seems
        // that I'm overwriting main page parameters (for instance
        // with the zoomButton's slice name) sometimes.
        //
        void labelButtonClick(object sender, RoutedEventArgs e)
        {
            // Find the associated visual (gotta be a better way),
            // so that we can use the advice alist.
            // TODO: learn how to pass the visual along with e
            Visual vis = null;
            foreach (Visual thisVis in visuals)
            {
                if (elements.ContainsKey(thisVis) &&
                    elements[thisVis].GetProperty(VisualElements.StatusAnimationTargetProperty) == sender)
                {
                    vis = thisVis;
                }
            }
            // error if vis is null;
            
            // Make a new map window in a floatable child window.
            SessionParameters newParams = new SessionParameters();
            newParams = parameters; // TODO: I need a shallow copy here to avoid overwrite.
            FloatableWindow fw = new FloatableWindow();
            fw.ResizeMode = ResizeMode.CanResize;
            fw.Height = 200;
            fw.Width = 200;

            MainPage mp = new MainPage(newParams);
            Location center = GetLocation(vis.objType, vis.objName);
            double targetZoomLevel = 2.0;

            // Accept any changes to defaults from advice.
            Alist advice = vis.positionAdvice; // Really the wrong name for this.
            if (advice != null)
            {
                // zoomTarget says which slice to display in popup
                string zoomTarget = advice.GetValue("zoomTarget");
                if (zoomTarget != null)
                    newParams.slice = zoomTarget;

                // width
                string width = advice.GetValue("width");
                if (width != null)
                    fw.Width = Convert.ToInt32(width);

                // height
                string height = advice.GetValue("height");
                if (height != null)
                    fw.Height = Convert.ToInt32(height);

                // latitude
                string latitude = advice.GetValue("latitude");
                if (latitude != null)
                    center.Latitude = Convert.ToDouble(latitude);

                // longitude
                string longitude = advice.GetValue("longitude");
                if (longitude != null)
                    center.Longitude = Convert.ToDouble(longitude);

                // zoomLevel
                string zoomLevel = advice.GetValue("zoomLevel");
                if (zoomLevel != null)
                    targetZoomLevel = Convert.ToDouble(zoomLevel);
            }
            fw.Title = newParams.slice;
            mp.sliceMap.SetView(center, targetZoomLevel);

            // Don't need decorations in child window.
            Grid lr = mp.LayoutRoot;
            lr.Children.Remove(mp.image1);
            lr.Children.Remove(mp.infoLabel);
            lr.Children.Remove(mp.sliceLabel);
            mp.sliceMap.SetValue(Grid.RowProperty, 0);
            mp.sliceMap.SetValue(Grid.ColumnProperty, 0);
            mp.sliceMap.SetValue(Grid.RowSpanProperty, 3);
            mp.sliceMap.SetValue(Grid.ColumnSpanProperty, 3);
            fw.Content = mp;

            fw.SetValue(Grid.RowProperty, 1);
            fw.SetValue(Grid.ColumnProperty, 1);
            fw.SetValue(Grid.RowSpanProperty, 1);
            fw.SetValue(Grid.ColumnSpanProperty, 1);

            // Position popup.
            fw.ParentLayoutRoot = mapCanvas;
            Point centerPoint = sliceMap.LocationToViewportPoint(center);
            fw.Show(centerPoint.X, centerPoint.Y);
        }



        // Just register for timer event.
        // Don't make data queries any more frequently than this timer.
        private void SetupDataUpdates()
        {
            DispatcherTimer dt = new DispatcherTimer();
            if (parameters.useDebugServer)
                dt.Interval = new TimeSpan(0, 0, 0, 0, 500); // dy, hr, min, sec, ms
            else
                dt.Interval = new TimeSpan(0, 0, 0, 0, 250); // dy, hr, min, sec, ms
            dt.Tick += new EventHandler(RequestNextUpdate);
            dt.Start();
        }


        // Send a data query for the next item in the queue.
        private void RequestNextUpdate(object sender, EventArgs e)
        {
            if (wc.IsBusy || (updateQueue.Count <= 0))
                return;

            Visual vis = updateQueue.Dequeue();

            // Build URI for query.
            string scriptName = "";
            string scriptParams = "";
            Uri queryURI = null;

            // Status queries use get_status.php; data use get_data.php
            // *** Not pretty:  status query is indicated by null visualization
            if (vis == null)
            {
                scriptName = "get_status.php";
                if ((URIParams != null) && (URIParams != ""))
                    scriptParams = URIParams;
            }
            else
            {
                scriptName = "get_data.php";
                string statQuery = vis.statQuery;
                if ((URIParams != null) && (URIParams != ""))
                    scriptParams = URIParams + "&statQuery=" + statQuery;
                else
                    scriptParams = "?statQuery=" + statQuery;
            }

            // Issue query for data needed.             

            queryURI = new Uri(phpBase + scriptName + scriptParams);
            wc.DownloadStringAsync(queryURI, vis);
        }


        // Just register for timer event.
        private void SetupBogusDataUpdates()
        {
            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = new TimeSpan(0, 0, 0, 1, 0); // dy, hr, min, sec, ms
            dt.Tick += new EventHandler(UpdateWithBogusData);
            dt.Start();
        }

        // Put in random data to get some action on the screen.
        private void UpdateWithBogusData(object sender, EventArgs e)
        {
            foreach (Visual vis in elements.Keys)
            {
                Stat thisStat = elements[vis].GetProperty(VisualElements.StatisticsProperty) as Stat;

                if (thisStat != null)
                {
                    double minValue = 0.0;
                    double maxValue = 100.0;

                    if (vis.minValue.HasValue)
                        minValue = vis.minValue.Value;
                    if (vis.maxValue.HasValue)
                        maxValue = vis.maxValue.Value;

                    if (thisStat.history > 0)
                    {
                        DateTime now = DateTime.Now;
                        thisStat.addValue(now, myRandom.Next((int)minValue, (int)maxValue));
                    }
                    else
                    {
                        double newValue = thisStat.currentValue -
                                          ((maxValue - minValue) / 2.0) +
                                          myRandom.Next((int)minValue, (int)maxValue);
                        if (newValue > maxValue)
                            newValue = maxValue;
                        if (newValue < minValue)
                            newValue = minValue;
                        thisStat.currentValue = newValue;
                    }
                }
            }
        }

        // If map view changes, need to refresh all animations.
        private void sliceMap_ViewChangeEnd(object sender, MapEventArgs e)
        {
            UpdateVisuals();
            foreach (Visual vis in elements.Keys) // why not "in visuals"?
                UpdateStoryboard(vis);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
