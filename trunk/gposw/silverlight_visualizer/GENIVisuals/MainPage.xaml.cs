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
        private SessionParameters m_parameters;
        private string m_URIParams = "";
        WebClient m_webClient = new WebClient();
        private string m_phpBase;


        // All the {visuals, nodes, links} we care about.
        private Collection<Visual> m_visuals = new Collection<Visual>();
        private Dictionary<string, Node> m_nodesDic = new Dictionary<string, Node>();
        private Dictionary<string, Link> m_linksDic = new Dictionary<string, Link>();

        private Dictionary<string, Node> m_nlrNodesDic = new Dictionary<string,Node>();
        private Dictionary<string, Node> m_i2NodesDic = new Dictionary<string,Node>();
        
        // Objects associated with a particular visual.
        private Dictionary<Visual, VisualElements> m_elementsDic =
            new Dictionary<Visual, VisualElements>();

        // Keep a chart for each object so that multiple line plots share same surface.
        private Dictionary<Object, Chart> m_chartsDic = new Dictionary<Object, Chart>();

        // The list of all visuals for updating.
        private Queue<Visual> m_updateQueue = new Queue<Visual>();
        private Dictionary<Object, Point> m_nodeOffsetsDic = new Dictionary<Object, Point>();
        private Dictionary<Point, List<Object>> m_overlappedObjectsDic = new Dictionary<Point, List<Object>>();
        private Dictionary<Object, UIElement> m_mapObjectsDic = new Dictionary<Object, UIElement>();
        // Map from data sources (nodes, links) to their associated visuals.
        Dictionary<Object, List<Visual>> m_visualsForSourceDic = new Dictionary<Object, List<Visual>>();

        private MapLayer m_overlayLayer = null;
        private Random m_random = new Random();

        double m_radius = 10;
        double m_radiusForRearrangement = 40;

        public MainPage(SessionParameters myparams)
        {
            InitializeComponent();
            m_parameters = myparams;
            if (m_parameters.topologyVisuals.Count == 0)
            {
                // Remember session parameters
                m_parameters.makePeriodicQuery = true;
#if DEBUG
                m_parameters.useDebugServer = true;
                m_parameters.debugServer = "http://ganel.gpolab.bbn.com:17380";
                m_parameters.slice = "SmartRE15Sep";
                m_parameters.dbHost = "ganel.gpolab.bbn.com";
                m_parameters.dbUser = "wzeng";
                m_parameters.dbPassword = "wzeng";
                m_parameters.dbName = "wzeng";
#endif
                // Gather up parameters to pass to PHP scripts.
                if ((m_parameters.slice != null) && (m_parameters.slice != ""))
                {
                    if (m_URIParams == "")
                        m_URIParams = "?slice=" + m_parameters.slice;
                    else
                        m_URIParams += "&slice=" + m_parameters.slice;
                }
                if ((m_parameters.dbHost != null) && (m_parameters.dbHost != ""))
                {
                    if (m_URIParams == "")
                        m_URIParams = "?server=" + m_parameters.dbHost;
                    else
                        m_URIParams += "&server=" + m_parameters.dbHost;
                }
                if ((m_parameters.dbUser != null) && (m_parameters.dbUser != ""))
                {
                    if (m_URIParams == "")
                        m_URIParams = "?dbUsername=" + m_parameters.dbUser;
                    else
                        m_URIParams += "&dbUsername=" + m_parameters.dbUser;
                }
                if ((m_parameters.dbPassword != null) && (m_parameters.dbPassword != ""))
                {
                    if (m_URIParams == "")
                        m_URIParams = "?dbPassword=" + m_parameters.dbPassword;
                    else
                        m_URIParams += "&dbPassword=" + m_parameters.dbPassword;
                }
                if ((m_parameters.dbName != null) && (m_parameters.dbName != ""))
                {
                    if (m_URIParams == "")
                        m_URIParams = "?db=" + m_parameters.dbName;
                    else
                        m_URIParams += "&db=" + m_parameters.dbName;
                }

                // Figure out base URI for PHP scripts.

                if (m_parameters.useDebugServer)
                {
                    m_phpBase = m_parameters.debugServer + "/GENIVisuals/bin/php/";
                }
                else
                {
                    string myURI = Application.Current.Host.Source.ToString();
                    m_phpBase = myURI.Substring(0, myURI.IndexOf("ClientBin")) + "bin/php/";
                }

                string uri = m_phpBase + "get_nodes.php" + m_URIParams;
                // Get list of nodes from PHP script.
                m_webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(wc_DownloadStringCompleted);
                m_webClient.DownloadStringAsync(new Uri(uri));
            }
            else
            {
                this.m_visuals = m_parameters.topologyVisuals;
                this.m_nodesDic = m_parameters.topologyNodes;
                this.m_linksDic = m_parameters.topologyLinks;
                DisplayVisuals();
                SetupDataUpdates();
                m_updateQueue.Enqueue(null); // setup status updates
            }
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

                    m_webClient.DownloadStringAsync(new Uri(m_phpBase + "get_links.php" + m_URIParams));
                }
                else if (resultType == "links")
                {
                    LoadLinks(completeResult);
                    m_webClient.DownloadStringAsync(new Uri(m_phpBase + "get_visuals.php" + m_URIParams));
                }
                else if (resultType == "visuals")
                {
                    LoadVisuals(completeResult);
                    DisplayVisuals();
                    SetupDataUpdates();
                    m_updateQueue.Enqueue(null); // setup status updates

                    if (m_parameters.useBogusData)
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

                sliceLabel.Content = m_parameters.slice;
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
            m_nodesDic.Clear();

            // Loop over list of nodes.
            JsonArray nodesJson = (JsonArray)completeResult["results"];
            foreach (JsonValue nodeJson in nodesJson)
            {
                // Parse node content out of JSON
                Node thisNode = new Node(nodeJson);
                m_nodesDic[thisNode.Name] = thisNode;
            }

            foreach (string name in m_nodesDic.Keys)
            {
                if (name.StartsWith("NLR"))
                {
                    m_nlrNodesDic.Add(name, m_nodesDic[name]);
                }
                else if (name.StartsWith("I2"))
                {
                    m_i2NodesDic.Add(name, m_nodesDic[name]);
                }
            }
        }


        // Parse link content out of JSON.
        private void LoadLinks(JsonValue completeResult)
        {
            // Forget what we know about links.
            m_linksDic.Clear();

            // Loop over list of links.
            JsonArray linksJson = (JsonArray)completeResult["results"];
            foreach (JsonValue linkJson in linksJson)
            {
                Link thisLink = new Link(linkJson);
                m_linksDic[thisLink.name] = thisLink;
            }
        }

        // Parse visual content out of JSON.
        private void LoadVisuals(JsonValue completeResult)
        {
            m_visuals.Clear();

            // Loop over list of visuals.
            JsonArray visualsJson = (JsonArray)completeResult["results"];
            foreach (JsonValue visualJson in visualsJson)
            {
                Visual thisVisual = new Visual(visualJson);
                m_visuals.Add(thisVisual);
            }

            foreach (Node node in m_nlrNodesDic.Values)
            {
                Visual vis = new Visual();
                vis.objName = node.Name;
                vis.objType = "node";
                vis.infoType = "label";
                bool shldAdd = true;
                foreach (Visual v in m_visuals)
                {
                    if (vis.objName == v.objName)
                    {
                        shldAdd = false;
                    }
                }
                if (shldAdd)
                {
                    m_visuals.Add(vis);
                }
            }

            foreach (Node node in m_i2NodesDic.Values)
            {
                Visual vis = new Visual();
                vis.objName = node.Name;
                vis.objType = "node";
                vis.infoType = "label";
                bool shldAdd = true;
                foreach (Visual v in m_visuals)
                {
                    if (vis.objName == v.objName)
                    {
                        shldAdd = false;
                    }
                }
                if (shldAdd)
                {
                    m_visuals.Add(vis);
                }
            }

        }

        private double Distance(Point a, Point b)
        {
            return Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }


        //TODO: if not all nodes in the topology map has a renderAdvice or
        //there are more than 9 nodes, we should fall back to the auto-organize scheme.
        private Point FindOffset(object obj)
        {
            if (m_parameters.topologyVisuals.Count == 0)
            {
                return m_nodeOffsetsDic[obj];
            }
            else
            {
                string renderAdvice = m_visualsForSourceDic[obj][0].renderAdvice;
                double mapHeight = this.ActualHeight;
                double mapWidth = this.ActualWidth;
                double verticalOfst = mapHeight / 3;
                double horizontalOfst = mapWidth / 3;
                Point ofst = new Point(0, 0);
                if (renderAdvice != null)
                {
                    if (renderAdvice.ToLower() == "north")
                    {
                        ofst.Y -= verticalOfst;
                    }
                    else if (renderAdvice.ToLower() == "south")
                    {
                        ofst.Y += verticalOfst;
                    }
                    else if (renderAdvice.ToLower() == "west")
                    {
                        ofst.X -= horizontalOfst;
                    }
                    else if (renderAdvice.ToLower() == "east")
                    {
                        ofst.X += horizontalOfst;
                    }
                    else if (renderAdvice.ToLower() == "northeast")
                    {
                        ofst.X += horizontalOfst;
                        ofst.Y -= verticalOfst;
                    }
                    else if (renderAdvice.ToLower() == "northwest")
                    {
                        ofst.X -= horizontalOfst;
                        ofst.Y -= verticalOfst;
                    }
                    else if (renderAdvice.ToLower() == "southeast")
                    {
                        ofst.X += horizontalOfst;
                        ofst.Y += verticalOfst;
                    }
                    else if (renderAdvice.ToLower() == "southwest")
                    {
                        ofst.X -= horizontalOfst;
                        ofst.Y += verticalOfst;
                    }
                    else if (renderAdvice.ToLower() == "center")
                    {
                        //do nothing. used (0,0) as the offset.
                    }
                    else
                    {
                        //TODO: unknown render adivce.
                    }
                }

                return ofst;
            }

        }

        //
        // Parse visuals content out of JSON and remember the
        // requested visual display parameters.
        //
        private void UpdateVisuals(bool createControl)
        {
            m_overlappedObjectsDic.Clear();
            m_nodeOffsetsDic.Clear();

            //initialize the offs
            Point zeroOffset = new Point(0, 0);
            foreach (Node n in m_nodesDic.Values)
            {
                m_nodeOffsetsDic[n] = zeroOffset;
            }

            //when a map is used only as a topology map, follow the specified position-advice retrieved
            //from the database, do not organize them. m_parameters.topologyVisuals.Count == 0 indicates
            //that the map is NOT used as a topology map.
            if (m_parameters.topologyVisuals.Count == 0)
            {
                //group ojbects that are close to each other
                foreach (Object obj in m_visualsForSourceDic.Keys)
                {
                    string objType = m_visualsForSourceDic[obj][0].objType;
                    string objName = m_visualsForSourceDic[obj][0].objName;
                    string infoType = m_visualsForSourceDic[obj][0].infoType;
                    Location location = GetLocation(objType, objName);
                    if (objType == "node")
                    {
                        Point newPoint = sliceMap.LocationToViewportPoint(location);
                        //if this new obejct is within a circle of a radius of 20 of an previous object,
                        //add it to the area of the previous object
                        bool added = false;
                        foreach (Point point in m_overlappedObjectsDic.Keys)
                        {
                            if (Distance(newPoint, point) <= m_radiusForRearrangement)
                            {
                                m_overlappedObjectsDic[point].Add(obj);
                                added = true;
                                continue;
                            }
                        }
                        //otherwise, make it a new point in the overlappedObjects
                        if (!added)
                        {
                            List<Object> newList = new List<object>();
                            newList.Add(obj);
                            m_overlappedObjectsDic.Add(newPoint, newList);
                        }
                    }
                }
#if DEBUG
                infoLabel.Content = m_overlappedObjectsDic.Count;
#endif

                //for each group of objects that are nearby, spread them over a circle with a radius of 20 around their geometric center.
                foreach (Point point in m_overlappedObjectsDic.Keys)
                {
                    List<Object> objects = m_overlappedObjectsDic[point];
                    if (objects.Count >= 2)
                    {
                        double avgX = 0, avgY = 0;
                        for (int i = 0; i < objects.Count; i++)
                        {
                            Object oj = objects[i];
                            string objType = m_visualsForSourceDic[oj][0].objType;
                            string objName = m_visualsForSourceDic[oj][0].objName;
                            Point p = sliceMap.LocationToViewportPoint(GetLocation(objType, objName));
                            m_nodeOffsetsDic[oj] = p;
                            avgX += p.X;
                            avgY += p.Y;
                        }

                        avgX = avgX / objects.Count;
                        avgY = avgY / objects.Count;

                        //(avgX, avgY) is the center of polygon formed by the nearby objects;
                        double step = 2 * Math.PI / objects.Count;
                        double radiusOffset = 0;
                        if (objects.Count == 2)
                        {
                            radiusOffset = Math.PI / 4;
                        }
                        else if (objects.Count == 4)
                        {
                            radiusOffset = Math.PI / 6;
                        }

                        for (int i = 0; i < objects.Count; i++)
                        {
                            Point offset = new Point();
                            double newX, newY;
                            Object oj = objects[i];
                            Point p = m_nodeOffsetsDic[oj];
                            newX = avgX + m_radius * objects.Count * Math.Cos(step * i + radiusOffset);
                            newY = avgY + m_radius * objects.Count * Math.Sin(step * i + radiusOffset);
                            offset.X = newX - p.X;
                            offset.Y = newY - p.Y;
                            m_nodeOffsetsDic[oj] = offset;
                        }
                    }
                }
            }

            //update the offsets of the UIElements on the map
            foreach (Object obj in m_visualsForSourceDic.Keys)
            {
                string objType = m_visualsForSourceDic[obj][0].objType;
                string objName = m_visualsForSourceDic[obj][0].objName;
                Location location = GetLocation(objType, objName);

                if (createControl)
                {
                    StackPanel panel = null;
                    Point offset = new Point(0, 0);

                    foreach (Visual vis in m_visualsForSourceDic[obj])
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
                                (vis.infoType == "lineGraph") || (vis.infoType == "amChart"))
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
                                m_updateQueue.Enqueue(vis);
                            m_elementsDic[vis].SetProperty(VisualElements.StatusAnimationTargetProperty, control);
                        }
                    }

                    if (panel != null)
                    {
                        Point ofst = FindOffset(obj);
                        m_overlayLayer.AddChild(panel, location, ofst);
                        m_mapObjectsDic.Add(obj, panel);
                    }
                }
                else
                {
                    if (m_mapObjectsDic.Keys.Contains(obj) && m_nodeOffsetsDic.Keys.Contains(obj))
                    {
                        UIElement el = m_mapObjectsDic[obj];
                        if (el != null)
                        {
                            Point ofst = FindOffset(obj);
                            m_overlayLayer.Children.Remove(el);
                            m_overlayLayer.AddChild(el, location, ofst);
                            m_overlayLayer.UpdateLayout();
                        }
                    }
                }
            }
        }

        private void DisplayVisuals()
        {
            m_visualsForSourceDic.Clear();
            m_elementsDic.Clear();
            m_updateQueue.Clear();

            // Loop over list of visuals.  Group visuals associated with
            // same data source together in a dictionary.
            foreach (Visual thisVisual in m_visuals)
            {
                m_elementsDic[thisVisual] = new VisualElements();
                Object dataSource = null;
                if (thisVisual.objType == "node")
                    dataSource = m_nodesDic[thisVisual.objName];
                else if (thisVisual.objType == "link")
                    dataSource = m_linksDic[thisVisual.objName];

                if (dataSource != null)
                {
                    if (!m_visualsForSourceDic.ContainsKey(dataSource))
                        m_visualsForSourceDic[dataSource] = new List<Visual>();
                    m_visualsForSourceDic[dataSource].Add(thisVisual);
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
            if (m_overlayLayer == null)
            {
                m_overlayLayer = new MapLayer();
                //overlayLayer.SizeChanged += new SizeChangedEventHandler(UpdateVisuals);
                sliceMap.Children.Add(m_overlayLayer);
            }
            //update the visuals with (createControl == true)
            //future updates will not need to create the controls again
            UpdateVisuals(true);
        }


        //
        // Parse numeric data out of JSON and use to update visual.
        //
        private void LoadData(JsonValue completeResult, Visual vis)
        {
            Stat myStat = m_elementsDic[vis].GetProperty(VisualElements.StatisticsProperty) as Stat;

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
            m_updateQueue.Enqueue(vis);
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
                foreach (Visual vis in m_visuals)
                    if (vis.statusHandle == info.Handle)
                    {
                        StatusInfo oldStatusInfo = m_elementsDic[vis].GetProperty(VisualElements.StatusProperty) as StatusInfo;
                        string oldStatus = "";
                        if (oldStatusInfo != null)
                            oldStatus = oldStatusInfo.Status;
                        m_elementsDic[vis].SetProperty(VisualElements.StatusProperty, info);

                        if (oldStatus != info.Status)
                            UpdateStoryboard(vis);
                    }

            // Requeue the query for later update.
            // *** Sleazy to use null Visual for this purpose.
            m_updateQueue.Enqueue(null);
        }


        //
        // Make a new storyboard to animate status display.
        //
        private void UpdateStoryboard(Visual vis)
        {
            VisualElements info = m_elementsDic[vis];
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
                    m_overlayLayer.Children.Remove(element);



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
                UIElement control = m_elementsDic[vis].GetProperty(VisualElements.StatusAnimationTargetProperty) as UIElement;
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
                UIElement control = m_elementsDic[vis].GetProperty(VisualElements.StatusAnimationTargetProperty) as UIElement;
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
                UIElement control = m_elementsDic[vis].GetProperty(VisualElements.StatusAnimationTargetProperty) as UIElement;
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
            MapPolyline pl = m_elementsDic[vis].GetProperty(VisualElements.LinkPolyLineProperty) as MapPolyline;
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
                m_overlayLayer.AddChild(ball, startLoc, offset);
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
            Link thisLink = m_linksDic[objName];
            Location sourceLoc = new Location(m_nodesDic[thisLink.sourceNode].Latitude,
                                                m_nodesDic[thisLink.sourceNode].Longitude);
            Location destLoc = new Location(m_nodesDic[thisLink.destNode].Latitude,
                                            m_nodesDic[thisLink.destNode].Longitude);
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
                Node thisNode = m_nodesDic[objName];
                return new Location(thisNode.Latitude, thisNode.Longitude);
            }
            else if (objType == "link")
            {
                Link thisLink = m_linksDic[objName];
                Location sourceLoc = new Location(m_nodesDic[thisLink.sourceNode].Latitude,
                                                  m_nodesDic[thisLink.sourceNode].Longitude);
                Location destLoc = new Location(m_nodesDic[thisLink.destNode].Latitude,
                                                m_nodesDic[thisLink.destNode].Longitude);
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
                obj = m_nodesDic[objectName];
            }
            else if (vis.objType == "link")
            {
                objectName = vis.objName;
                obj = m_linksDic[objectName];
            }

            if (obj == null)
                return null;


            // *** For goodness sake, please refactor me so
            // *** that we're not stuck with this big if statement
            // *** and all this logic in one place.

            // Is it a label?
            if (vis.infoType == "label")
            {
                Label label = new Label();
                label.Content = objectName;

                if (objectName.StartsWith("NLR")) {
                    label.Background = new SolidColorBrush(Colors.Yellow);
                    label.Foreground = new SolidColorBrush(Colors.Black);
                }
                else if (objectName.StartsWith("I2"))
                {
                    label.Background = new SolidColorBrush(Colors.Green);
                    label.Foreground = new SolidColorBrush(Colors.Black);
                }
                else
                {
                    label.Background = new SolidColorBrush(Colors.DarkGray);
                    label.Foreground = new SolidColorBrush(Colors.White);
                }
                
                control = label;
                m_elementsDic[vis].SetProperty(VisualElements.DataSourceLabelProperty, label);
            }
            else if (vis.infoType == "zoomButton")
            {
                //TODO: need a different logic for button on a topology map
                Button button = new Button();
                button.Click += new RoutedEventHandler(labelButtonClick);
                button.Content = objectName;
                control = button;
            }
            else if (vis.infoType == "amChart")
            {
                //TODO: need a different logic for button on a topology map
                Button button = new Button();
                button.Click += new RoutedEventHandler(amChartButtonClick);
                button.Content = objectName;
                LinearGradientBrush myLinearGradientBrush = new LinearGradientBrush();
                myLinearGradientBrush.StartPoint = new Point(0,0);
                myLinearGradientBrush.EndPoint = new Point(1,1);
                GradientStop stop = new GradientStop();
                stop.Color = Colors.Red;
                stop.Offset = 0.25;
                myLinearGradientBrush.GradientStops.Add(stop);

                button.Background = myLinearGradientBrush;
                control = button;
            }
            // Is it an icon?
            else if ((vis.infoType == "icon") &&
                     (vis.objType == "node"))
            {
                Image image = new Image();
                image.Height = 50;
                image.Width = 50;
                string iconString = m_nodesDic[vis.objName].Icon;
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
                m_elementsDic[vis].SetProperty(VisualElements.LinkPolyLineProperty, control);
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
                    if (m_chartsDic.ContainsKey(obj) && m_chartsDic[obj] != null)
                        ch = m_chartsDic[obj];
                    else
                    {
                        ch = new Chart();
                        m_chartsDic[obj] = ch;
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
                    m_elementsDic[vis].SetProperty(VisualElements.StatisticsProperty, myStat);
            }
            // Other types of visual are unknown.
            else
            {
                infoLabel.Content = "Unknown visual type.";
                throw new NotImplementedException();
            }


            return control;
        }

        void amChartButtonClick(object sender, RoutedEventArgs e)
        {            // Find the associated visual (gotta be a better way),
            // so that we can use the advice alist.
            // TODO: learn how to pass the visual along with e
            Visual vis = null;
            foreach (Visual thisVis in m_visuals)
            {
                if (m_elementsDic.ContainsKey(thisVis) &&
                    m_elementsDic[thisVis].GetProperty(VisualElements.StatusAnimationTargetProperty) == sender)
                {
                    vis = thisVis;
                }
            }

            String objName = ((Button)sender).Content.ToString();

            FloatableWindow fw = new FloatableWindow();
            fw.ResizeMode = ResizeMode.CanResize;

            ChartPage newpage = new ChartPage();
            newpage.Title = objName;

            fw.Height = 500;
            fw.Width = 800;

            Location center = GetLocation(vis.objType, vis.objName);

            fw.Title = m_parameters.slice;
            fw.Content = newpage;

            fw.SetValue(Grid.RowProperty, 1);
            fw.SetValue(Grid.ColumnProperty, 1);
            fw.SetValue(Grid.RowSpanProperty, 1);
            fw.SetValue(Grid.ColumnSpanProperty, 1);

            // Position popup.
            fw.ParentLayoutRoot = mapCanvas;
            Point centerPoint = sliceMap.LocationToViewportPoint(center);
            fw.Show(centerPoint.X, centerPoint.Y);
        }


        //
        // ** Experimental **
        //
        void labelButtonClick(object sender, RoutedEventArgs e)
        {
            // Find the associated visual (gotta be a better way),
            // so that we can use the advice alist.
            // TODO: learn how to pass the visual along with e
            Visual vis = null;
            foreach (Visual thisVis in m_visuals)
            {
                if (m_elementsDic.ContainsKey(thisVis) &&
                    m_elementsDic[thisVis].GetProperty(VisualElements.StatusAnimationTargetProperty) == sender)
                {
                    vis = thisVis;
                }
            }
            // error if vis is null;

            // Make a new map window in a floatable child window.
            SessionParameters newParams = new SessionParameters(m_parameters);

            newParams.makePeriodicQuery = false;
            
            String objName = ((Button)sender).Content.ToString();
            List<Object> minimapNodes = new List<Object>();
            List<Object> minimapLinks = new List<Object>();
            Collection<Visual> minimapVisuals = new Collection<Visual>();

            if (m_nodesDic.Keys.Contains(objName))
            {
                Node node = m_nodesDic[objName];
                //find all the ndoes that are at the same coordinates
                foreach (Node n in m_nodesDic.Values)
                {
                    if (n.Latitude == node.Latitude && n.Longitude == n.Longitude)
                    {
                        minimapNodes.Add(n);
                    }
                }
                //find all the links that are incident to the colocated nodes
                foreach (Link link in m_linksDic.Values)
                {
                    foreach (Object n in minimapNodes)
                    {
                        //only edges with both ends inside the topology map are added.
                        if (minimapNodes.Contains(m_nodesDic[link.destNode]) && minimapNodes.Contains(m_nodesDic[link.sourceNode]))
                        {
                            minimapLinks.Add(link);
                        }
                    }
                }
                //find all the visuals that are associated to the found links and nodes
                foreach (Object obj in m_visualsForSourceDic.Keys)
                {
                    if (minimapNodes.Contains(obj))
                    {
                        foreach (Visual v in m_visualsForSourceDic[obj])
                        {
                            minimapVisuals.Add(v);
                        }
                    }
                    else if (minimapLinks.Contains(obj))
                    {
                        foreach (Visual v in m_visualsForSourceDic[obj])
                        {
                            minimapVisuals.Add(v);
                        }
                    }
                }
                newParams.topologyVisuals = minimapVisuals;

                foreach (Object oj in minimapNodes)
                {
                    Node n = (Node)oj;
                    if (!newParams.topologyNodes.Keys.Contains(n.Name))
                    {
                        newParams.topologyNodes.Add(n.Name, n);
                    }
                }

                foreach (Object oj in minimapLinks)
                {
                    Link l = (Link)oj;
                    if (!newParams.topologyLinks.Keys.Contains(l.name))
                    {
                        newParams.topologyLinks.Add(l.name, l);
                    }
                }
            }

            FloatableWindow fw = new FloatableWindow();
            fw.ResizeMode = ResizeMode.CanResize;

            fw.Height = 250;
            fw.Width = 250;

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
            if (m_parameters.makePeriodicQuery)
            {
                DispatcherTimer dt = new DispatcherTimer();
                if (m_parameters.useDebugServer)
                    dt.Interval = new TimeSpan(0, 0, 0, 0, 500); // dy, hr, min, sec, ms
                else
                    dt.Interval = new TimeSpan(0, 0, 0, 0, 250); // dy, hr, min, sec, ms
                dt.Tick += new EventHandler(RequestNextUpdate);
                dt.Start();
            }
        }


        // Send a data query for the next item in the queue.
        private void RequestNextUpdate(object sender, EventArgs e)
        {
            if (m_webClient.IsBusy || (m_updateQueue.Count <= 0))
                return;

            Visual vis = m_updateQueue.Dequeue();

            // Build URI for query.
            string scriptName = "";
            string scriptParams = "";
            Uri queryURI = null;

            // Status queries use get_status.php; data use get_data.php
            // *** Not pretty:  status query is indicated by null visualization
            if (vis == null)
            {
                scriptName = "get_status.php";
                if ((m_URIParams != null) && (m_URIParams != ""))
                    scriptParams = m_URIParams;
            }
            else
            {
                scriptName = "get_data.php";
                string statQuery = vis.statQuery;
                if ((m_URIParams != null) && (m_URIParams != ""))
                    scriptParams = m_URIParams + "&statQuery=" + statQuery;
                else
                    scriptParams = "?statQuery=" + statQuery;
            }

            // Issue query for data needed.             

            queryURI = new Uri(m_phpBase + scriptName + scriptParams);
            m_webClient.DownloadStringAsync(queryURI, vis);
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
            foreach (Visual vis in m_elementsDic.Keys)
            {
                Stat thisStat = m_elementsDic[vis].GetProperty(VisualElements.StatisticsProperty) as Stat;

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
                        thisStat.addValue(now, m_random.Next((int)minValue, (int)maxValue));
                    }
                    else
                    {
                        double newValue = thisStat.currentValue -
                                          ((maxValue - minValue) / 2.0) +
                                          m_random.Next((int)minValue, (int)maxValue);
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
            UpdateVisuals(false);
            foreach (Visual vis in m_elementsDic.Keys) // why not "in visuals"?
                UpdateStoryboard(vis);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
