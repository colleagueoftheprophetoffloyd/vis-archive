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
        private string postData = "";
        private string phpBase;

        // These are associated with managing
        // PHP downloads from database.
        private string previousNodeResults = null;
        private string previousLinkResults = null;
        private string previousVisualResults = null;
        private string previousStatusResults = null;
        private Alist previousMapAttributes = new Alist();


        // All the {visuals, nodes, links} we care about.
        private List<Visual> visuals = new List<Visual>();
        private Dictionary<string, Node> nodes = new Dictionary<string, Node>();
        private Dictionary<string, Link> links = new Dictionary<string, Link>();

        
        // Objects associated with a particular visual.
        // Someday should probably bundle these up into a single thing.
        private Dictionary<string, VisualControl> controls =
            new Dictionary<string, VisualControl>();
        private Dictionary<Visual, Stat> stats =
            new Dictionary<Visual, Stat>();
                
        // Keep a value history for each object so that multiple line plots share same surface.
        private Dictionary<Object, ValueHistory> graphs = new Dictionary<Object, ValueHistory>();


        // The list of all visuals for updating.
        private Queue<Visual> updateQueue = new Queue<Visual>();


        private Random myRandom = new Random();

        static string[] visualTypeNames = 
        {
            "label",
            "zoomButton",
            "icon",
            "scalar",
            "usageGrid",
            "scalarText",
            "lineGraph",
            "arc",
            "point"
        };
        static string[] updatedVisualTypeNames =
        {
            "label",
            "zoomButton",
            "icon",
            "scalar",
            "usageGrid",
            "scalarText",
            "lineGraph",
            "arc",
            "point"
        };
        static string[] statisticsVisualTypeNames =
        {
            "scalar",
            "usageGrid",
            "scalarText",
            "lineGraph"
        };
        static string[] nonOverlayVisualTypeNames =
        {
            "arc"
        };
        static List<string> allVisualTypes = new List<string>(visualTypeNames);
        static List<string> updatedVisualTypes = new List<string>(updatedVisualTypeNames);
        static List<string> statisticsVisualTypes = new List<string>(statisticsVisualTypeNames);
        static List<string> nonOverlayVisualTypes = new List<string>(nonOverlayVisualTypeNames);

        public MainPage(SessionParameters myparams)
        {
            InitializeComponent();

            // Remember session parameters
            parameters = myparams;

            //if ((parameters.subSlice == null) ||
            //    (parameters.subSlice == ""))
            if (true)
            {
                InializeBaseConfiguration();
            }
            else
            {
                InitializePopupConfiguration();
            }
        }




        private void InializeBaseConfiguration()
        {
            // Gather up parameters to pass to PHP scripts.
            List<string> postDataItems = new List<string>();
            if ((parameters.slice != null) && (parameters.slice != ""))
                postDataItems.Add("slice=" + parameters.slice);
            if ((parameters.dbHost != null) && (parameters.dbHost != ""))
                postDataItems.Add("server=" + parameters.dbHost);
           if ((parameters.dbUser != null) && (parameters.dbUser != ""))
                postDataItems.Add("dbUsername=" + parameters.dbUser);
           if ((parameters.dbPassword != null) && (parameters.dbPassword != ""))
                postDataItems.Add("dbPassword=" + parameters.dbPassword);
            if ((parameters.dbName != null) && (parameters.dbName != ""))
                postDataItems.Add("db=" + parameters.dbName);
            postData = string.Join("&", postDataItems);
            
            // Figure out base URI for PHP scripts.
            if (parameters.useDebugServer)
            {
                phpBase = parameters.debugServer + "/GENIVisualsTesting/bin/php/";
            }
            else
            {
                string myURI = Application.Current.Host.Source.ToString();
                phpBase = myURI.Substring(0, myURI.IndexOf("ClientBin")) + "bin/php/";
            }

            // Get information from DB.
            WebClient wc = new WebClient();
            wc.Encoding = System.Text.Encoding.UTF8;
            wc.Headers["Content-type"] = "application/x-www-form-urlencoded";
            wc.UploadStringCompleted += new UploadStringCompletedEventHandler(wc_UploadStringCompleted);
            wc.UploadStringAsync(new Uri(phpBase + "get_all.php"), "POST", postData);
        }


        private JsonValue resultsOfType(JsonValue mixedResults,
                                        string resultType)
        {
            if (mixedResults.ContainsKey("results"))
            {
                JsonArray allResults = (JsonArray)mixedResults["results"];
                if (allResults != null)
                {
                    foreach (JsonValue resultSet in allResults)
                    {
                        if (resultSet.ContainsKey("returnType") &&
                            resultSet["returnType"] == resultType)
                        {
                            return resultSet;
                        }
                    }
                }
            }

            return null;
        }


        void wc_UploadStringCompleted(object sender, UploadStringCompletedEventArgs e)
        {
            if (e.Error == null && e.Result != "")
            {
                JsonValue completeResult = JsonPrimitive.Parse(e.Result);
                string resultType = completeResult["returnType"].ToString().Replace('"', ' ').Trim();

                if (resultType == "mixed")
                {
                    Boolean changedSomething = false;

                    JsonValue nodeResults = resultsOfType(completeResult, "nodes");
                    if ((nodeResults != null) &&
                            (nodeResults.ToString() != previousNodeResults))
                    {
                        LoadNodes(nodeResults);
                        previousNodeResults = nodeResults.ToString();
                        changedSomething = true;
                    }

                    JsonValue linkResults = resultsOfType(completeResult, "links");
                    if ((linkResults != null) &&
                        (linkResults.ToString() != previousLinkResults))
                    {
                        LoadLinks(linkResults);
                        previousLinkResults = linkResults.ToString();
                        changedSomething = true;
                    }

                    JsonValue visualResults = resultsOfType(completeResult, "visuals");
                    if ((visualResults != null) &&
                        (changedSomething ||
                         (visualResults.ToString() != previousVisualResults)))
                    {
                        LoadVisuals(visualResults);
                        previousVisualResults = visualResults.ToString();
                        changedSomething = true;
                        DisplayVisuals();
                        SetupDataUpdates();

                        if (parameters.useBogusData)
                            SetupBogusDataUpdates();
                    }

                    JsonValue statusResults = resultsOfType(completeResult, "status");
                    if ((statusResults != null) &&
                        (changedSomething ||
                         (statusResults.ToString() != previousStatusResults)))
                    {
                        LoadStatus(statusResults);
                        previousStatusResults = statusResults.ToString();
                    }
                    updateQueue.Enqueue(null); // Queue up next status update
                }
                else if (resultType == "data")
                {
                    Visual vis = e.UserState as Visual;
                    if (vis != null)
                        LoadData(completeResult, vis);
                }

                sliceLabel.Content = parameters.slice;
            }
            else
            {
                infoLabel.Content = e.Error;
            }
        }


        private void InitializePopupConfiguration()
        {
            DisplayVisuals();
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
            // Forget what we know about visuals.
            visuals.Clear();

            // Loop over list of visuals.
            JsonArray visualsJson = (JsonArray)completeResult["results"];
            foreach (JsonValue visualJson in visualsJson)
            {
                Visual thisVisual = new Visual(visualJson);
                visuals.Add(thisVisual);
            }
        }


        //
        // Parse visuals content out of JSON and remember the
        // requested visual display parameters.
        //
        private void DisplayVisuals()
        {
            // Clear out display information and map overlay.
            controls.Clear();
            stats.Clear();
            graphs.Clear();
            updateQueue.Clear();
            OverlayLayer.Children.Clear();

            // Map from data sources (nodes, links)
            // to Stackers (containers for visuals associated with one object).
            Dictionary<Object, Stacker> stackers = new Dictionary<object, Stacker>();

            // Loop over list of visuals.  Group visuals associated with
            // same data source together in a dictionary.
            visuals.Sort();
            foreach (Visual vis in visuals)
            {
                if (inMySubSlice(vis))
                {
                    if (vis.infoType == "map")
                    {
                        Alist newMapAttributes = vis.renderAttributes;
                        if (!newMapAttributes.Equals(previousMapAttributes))
                        {
                            previousMapAttributes = newMapAttributes;
                            SetMapParameters(newMapAttributes);
                        }
                    }
                    else
                    {
                        Object dataSource = FindObject(vis.objType, vis.objName);
                        string title = vis.objName;
                        VisualControl control = MakeVisControl(vis);
                        control.ParentVisual = vis;

                        if (vis.renderAttributes.GetValue("Title") != null)
                            title = vis.renderAttributes.GetValue("Title");

                        if (control != null)
                        {
                            if (nonOverlayVisualTypes.Contains(vis.infoType))
                            {
                                Location location = GetSourceLocation(vis.objName);
                                control.anchorLocation = location;
                                control.processAttributes(vis.renderAttributes, null);
                                Point offset = new Point(control.anchorOffset.X + control.alignmentOffset.X,
                                                         control.anchorOffset.Y + control.alignmentOffset.Y);
                                OverlayLayer.AddChild(control, location, offset);
                            }
                            else
                            {
                                Stacker stacker;
                                if (stackers.ContainsKey(dataSource))
                                    stacker = stackers[dataSource];
                                else
                                {
                                    stacker = new Stacker();
                                    stackers[dataSource] = stacker;
                                    stacker.ParentVisual = vis;
                                }
                                if (!stacker.Panel.Children.Contains(control))
                                    stacker.Panel.Children.Add(control);
                                Location location = GetLocation(vis.objType, vis.objName);
                                control.anchorLocation = location;
                                control.processAttributes(vis.renderAttributes, stacker);
                                stacker.anchorLocation = control.anchorLocation;
                                stacker.anchorOffset = control.anchorOffset;
                                stacker.alignmentOffset = control.alignmentOffset; // This is probably wrong. ***
                            }

                            if ((vis.statQuery != null) && (vis.statQuery != ""))
                                updateQueue.Enqueue(vis);

                            controls[vis.name] = control;
                        }
                    }
                }
            }

            foreach (Object obj in stackers.Keys) {
                Stacker stacker = stackers[obj];
                Visual vis = stacker.ParentVisual;
                Location location = stacker.anchorLocation;
                Point offset = new Point(stacker.anchorOffset.X + stacker.alignmentOffset.X,
                                         stacker.anchorOffset.Y + stacker.alignmentOffset.Y);
                OverlayLayer.AddChild(stacker, location, offset);
            }
        }

        
        // Subslices match if they have the same name, or are both null/empty.
        private bool inMySubSlice(Visual vis)
        {
            string mySubslice = "";
            string visSubslice = "";

            if (vis == null)
                return false;

            if (parameters.subSlice != null)
                mySubslice = parameters.subSlice;
            if (vis.subSlice != null)
                visSubslice = vis.subSlice;

            return (mySubslice == visSubslice);
        }


        //
        // Parse numeric data out of JSON and use to update visual.
        //
        private void LoadData(JsonValue completeResult, Visual vis)
        {
            if (stats.ContainsKey(vis))
            {
                Stat myStat = stats[vis];

                // Load the data pairs.
                JsonArray dataJson = (JsonArray) completeResult["results"];
                foreach (JsonValue pairJson in dataJson)
                {
                    DateTime time = DateTime.MinValue;
                    double value = 0.0;

                    if (pairJson["time"] != null)
                        time = Convert.ToDateTime((string) pairJson["time"]);
                    if (pairJson["value"] != null)
                        value = Convert.ToDouble((string) pairJson["value"]);

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
                    if ((vis.statusHandle == info.Handle) &&
                        (controls.ContainsKey(vis.name)))
                    {
                        VisualControl control = controls[vis.name];
                        if (control.CurrentStatus != info.Status)
                            control.CurrentStatus = info.Status;
                    }

            // Requeue the query for later update.
            // *** Sleazy to use null Visual for this purpose.
            updateQueue.Enqueue(null);
        }


        //
        // Return the underlying object (node or link) named by
        // the provided type and name.  Show an error and return
        // null on failure.
        //
        private Object FindObject(string objType, string objName)
        {
            if (objType == "node")
            {
                if (nodes.ContainsKey(objName))
                    return nodes[objName];
            }
            else if (objType == "link")
            {
                if (links.ContainsKey(objName))
                    return links[objName];
            }

            ShowError(String.Format("FindObject couldn't find object {0} of type {1}.",
                                    objName, objType));
            return null;
        }


        //
        // Return a location for the object (node, link).
        //
        private Location GetLocation(string objType, string objName)
        {
            // Currently only understand nodes and links.
            if (objType == "node")
            {
                Node thisNode = FindObject(objType, objName) as Node;
                if (thisNode != null)
                    return new Location(thisNode.Latitude, thisNode.Longitude);
                else
                    return null;
            }
            else if (objType == "link")
            {
                Link thisLink = FindObject(objType, objName) as Link;
                if (thisLink == null)
                    return null;
                Node sourceNode = FindObject("node", thisLink.sourceNode) as Node;
                Node destNode = FindObject("node", thisLink.destNode) as Node;
                if ((sourceNode == null) || (destNode == null))
                    return null;

                Location sourceLoc = new Location(sourceNode.Latitude, sourceNode.Longitude);
                Location destLoc = new Location(destNode.Latitude, destNode.Longitude);
                Point sPoint = sliceMap.LocationToViewportPoint(sourceLoc);
                Point dPoint = sliceMap.LocationToViewportPoint(destLoc);
                Point middle = new Point((sPoint.X + dPoint.X) / 2.0,
                                         (sPoint.Y + dPoint.Y) / 2.0);
                return sliceMap.ViewportPointToLocation(middle);
            }
            else
            {
                ShowError(String.Format("Internal error: unknown object {0} of type {1} in GetLocation.",
                                        objName, objType));
                return null;
            }
        }

        // Find the location of the source node of a link.
        private Location GetSourceLocation(string linkName)
        {
            Link thisLink = FindObject("link", linkName) as Link;
            if (thisLink == null)
                return null;

            Node sourceNode = FindObject("node", thisLink.sourceNode) as Node;
            if (sourceNode == null)
                return null;

            Location sourceLoc = new Location(sourceNode.Latitude, sourceNode.Longitude);
            return sourceLoc;
        }


        //
        // Helper functions for PointsForPath.  Return pixel offset and
        // visual location parts in a point spec in a datapath.
        // Example:
        // vis1+(-100;50) (means X,Y offset of 1100,50 from location of vis1)
        //
        private Point OffsetPart(string spec)
        {
            Point result = new Point(0, 0);

            string [] parts = spec.Split('+');
            if (parts.Length == 2)
            {
                char[] parens = { '(', ')' };
                string offsetString = parts[1].Trim(parens);
                string[] coords = offsetString.Split(';');

                if (coords.Length == 2)
                {
                    result.X = Convert.ToDouble(coords[0]);
                    result.Y = Convert.ToDouble(coords[1]);
                }
            }
            return result;
        }

        private string VisPart(string spec)
        {
            string visName = spec.Split('+')[0];

            if ((visName != null) && (visName != ""))
                return visName;
            else
                return null;
        }


        //
        // Return a point collection of waypoints for the given data path.
        //
        private PointCollection PointsForPath(Visual vis)
        {

            // Get the list of visual locations that the path follows.
            // Use the list in "datapath" attribute.
            List<Location> datapathLocations = new List<Location>();
            List<Point> datapathOffsets = new List<Point>();
            string datapath = vis.renderAttributes.GetValue("datapath");
            if (datapath != null)
            {
                foreach (string waypointSpec in datapath.Split(':'))
                {
                    Location loc;
                    Point locOffset;

                    string visName = VisPart(waypointSpec);
                    if (controls.ContainsKey(visName))
                    {
                        VisualControl control = controls[visName];
                        loc = control.anchorLocation;
                        locOffset = control.anchorOffset;
                    }
                    else
                    {
                        loc = null;
                        locOffset = new Point(0, 0);
                    }

                    if (loc != null)
                    {
                        datapathLocations.Add(loc);
                        Point additionalOffset = OffsetPart(waypointSpec);
                        Point offset = new Point(locOffset.X + additionalOffset.X,
                                                 locOffset.Y + additionalOffset.Y);
                        datapathOffsets.Add(offset);                                                        
                    }
                }
            }

            // Didn't find a good datapath.
            if (datapathLocations.Count() < 2)
            {
                ShowError(String.Format("Missing or bad datapath attribute for arc {0}.",
                                        vis.name));
                return null;
            }

            // Convert from (lat,lon) to pixel coordinates, using the initial
            // point as origin.
            Location firstLoc = datapathLocations[0];
            Point firstPoint = sliceMap.LocationToViewportPoint(firstLoc);
            PointCollection result = new PointCollection();

            for (int i=0; i < datapathLocations.Count; i++)
            {
                Location thisLoc = datapathLocations[i];
                Point offset = datapathOffsets[i];
                Point thisPoint = sliceMap.LocationToViewportPoint(thisLoc);
                result.Add(new Point(thisPoint.X + offset.X - firstPoint.X,
                                     thisPoint.Y + offset.Y - firstPoint.Y));
            }
            return result;
        }




        //
        // Create the UI Control that presents data for the requested visualization.
        //
        private VisualControl MakeVisControl(Visual vis)
        {
            Object obj = null;
            string objectName = "";
            VisualControl control = null;

            // Find the associated object.
            // We understand nodes and links.
            if ((vis.objType == "node") && nodes.ContainsKey(vis.objName))
            {
                objectName = vis.objName;
                obj = nodes[objectName];
            }
            else if ((vis.objType == "link") && links.ContainsKey(vis.objName))
            {
                objectName = vis.objName;
                obj = links[objectName];
            }

            if (obj == null)
            {
                ShowError(String.Format("MakeVisControl couldn't find object {0} of type {1} for visual of type {2}.",
                                        vis.objName, vis.objType, vis.infoType));
                return null;
            }


            // *** For goodness sake, please refactor me so
            // *** that we're not stuck with this big if statement
            // *** and all this logic in one place.

            // Is is a label?
            if (vis.infoType == "label")
            {
                TextLabel label = new TextLabel();
                label.Width = 100;
                label.Height = 20;
                string labelContent = vis.renderAttributes.GetValue("text");
                if ((labelContent != null) && (labelContent != ""))
                    label.Label.Content = labelContent;
                else
                    label.Label.Content = objectName;
                control = label;
            }
            else if (vis.infoType == "zoomButton")
            {
                ZoomButton zoomButton = new ZoomButton();
                zoomButton.Button.Click += new RoutedEventHandler(zoomButtonClick);
                zoomButton.Button.Content = objectName;
                control = zoomButton;
            }
            // Is it an icon?
            else if ((vis.infoType == "icon") &&
                     (vis.objType == "node"))
            {
                IconFrame frame = new IconFrame();
                frame.Height = 50;
                frame.Width = 50;
                frame.Background = new SolidColorBrush(Colors.Black);

                string iconString = nodes[vis.objName].Icon;
                if ((iconString != null) && (iconString != ""))
                {
                    string uriString = null;

                    if (iconString.StartsWith("images/") &&
                        (!iconString.Substring("images/".Length).Contains('/')))
                    {
                        string myURI = Application.Current.Host.Source.ToString();
                        string imageBase = myURI.Substring(0, myURI.IndexOf("ClientBin"));
                        uriString = imageBase + iconString;
                    }
                    else
                        uriString = iconString;
                    Uri imageSourceURI = new Uri(uriString, UriKind.Absolute);
                    frame.FramedImage.Source = new System.Windows.Media.Imaging.BitmapImage(imageSourceURI);
                }

                if (frame.FramedImage.Source == null)
                {
                    ShowError(String.Format("MakeVisControl: couldn't build icon at location {0}.",
                                            iconString));
                }
                control = frame;
            }
            // Is it a point?
            else if (vis.infoType == "point")
            {
                PointLocation loc = new PointLocation();
                loc.Height = 40;
                loc.Width = 40;
                control = loc;
            }
            // Is it an arc (links only)?
            else if (vis.infoType == "arc")
            {
                DataPath arc = new DataPath();
                string thicknessSpec = vis.renderAttributes.GetValue("thickness");
                if ((thicknessSpec != null) && (thicknessSpec != ""))
                    arc.Thickness = Convert.ToDouble(thicknessSpec);
                arc.Waypoints = PointsForPath(vis);
                control = arc;
            }
            // Is it a statitics graph?
            else if (statisticsVisualTypes.Contains(vis.infoType))
            {
                // Make a new Stat object to hold data
                Stat myStat = new Stat();
                myStat.statType = vis.statType;
                myStat.history = vis.statHistory;

                // Use a progress bar for scalar values or a line graph for time series.
                // Nest it in a horizontal stack panel with a label.
                if (vis.infoType == "scalar")
                {
                    ScalarValue scalar = new ScalarValue();
                    scalar.Width = 200;
                    scalar.Height = 40;
                    scalar.Label.Content = vis.statType;
                    if (vis.minValue.HasValue)
                        scalar.ProgressBar.Minimum = vis.minValue.Value;
                    if (vis.maxValue.HasValue)
                        scalar.ProgressBar.Maximum = vis.maxValue.Value;

                    Binding bind = new Binding("currentValue");
                    bind.Source = myStat;
                    scalar.ProgressBar.SetBinding(ProgressBar.ValueProperty, bind);
                    control = scalar;
                }

                if (vis.infoType == "usageGrid")
                {
                    ResourceUsageGrid grid = new ResourceUsageGrid();
                    grid.Width = 100;
                    grid.Height = 100;
                    grid.NameBlock.Text = vis.statType;
                    grid.Total = 100;
                    if (vis.maxValue.HasValue)
                        grid.Total = Convert.ToInt32(vis.maxValue.Value);

                    Binding bind = new Binding("currentValue");
                    bind.Source = myStat;
                    grid.SetBinding(ResourceUsageGrid.InUseProperty, bind);
                    control = grid;
                }

                if (vis.infoType == "scalarText")
                {
                    LabelPair pair = new LabelPair();
                    pair.Label.Content = vis.statType;

                    Binding bind = new Binding("currentValue");
                    bind.Source = myStat;
                    pair.Value.SetBinding(Label.ContentProperty, bind);
                    control = pair;
                }

                if (vis.infoType == "lineGraph")
                {
                    // Make a chart to plot line graphs.
                    // Remember and reuse chart for future
                    // line graphs for this object.
                    ValueHistory graph = null;
                    if (graphs.ContainsKey(obj) && graphs[obj] != null)
                        graph = graphs[obj];
                    else
                    {
                        graph = new ValueHistory();
                        graph.Width = 300;
                        graph.Height = 200;
                        graphs[obj] = graph;
                    }

                    // Create a line series
                    LineSeries line = graph.AddLine();
                    line.SetValue(LineSeries.TitleProperty, myStat.statType);
                    if (vis.minValue.HasValue)
                        graph.Minimum = vis.minValue;
                    if (vis.maxValue.HasValue)
                        graph.Maximum = vis.maxValue;

                    // Bind data
                    Binding bind = new Binding("values");
                    bind.Source = myStat;
                    line.SetBinding(LineSeries.ItemsSourceProperty, bind);
                    line.IndependentValuePath = "Key";
                    line.DependentValuePath = "Value";

                    control = graph;
                }

                if (control != null)
                    stats[vis] = myStat;
            }
            // Other types of visual are unknown.
            else
            {
                ShowError("Configuration requests a visual of unknown type: " + vis.infoType);
            }

            return control;
        }


        //
        // Event routine for clicks on zoomButton objects.
        // Display a floating popup window.
        //
        void zoomButtonClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Visual vis = null;
            foreach (Visual thisVis in visuals)
            {
                if ((thisVis.infoType == "zoomButton") &&
                    (thisVis.objName == (string) button.Content))
                {
                    vis = thisVis;
                }
            }

            if (vis == null)
            {
                ShowError("Couldn't find associated visual for zoom button.");
                return;
            }

            ZoomButton zb = controls[vis.name] as ZoomButton;


            // Make a new map window in a floatable child window.
            SessionParameters newParams = new SessionParameters(parameters);
            FloatableWindow fw = new FloatableWindow();
            fw.ResizeMode = ResizeMode.CanResize;
            fw.Height = 200;
            fw.Width = 200;

            MainPage mp = new MainPage(newParams);
            Location center = GetLocation(vis.objType, vis.objName);
            double targetZoomLevel = 2.0;

            // Accept any changes to defaults.
            Alist advice = vis.renderAttributes;
            if (advice != null)
            {
                // zoomTarget says which subslice to display in popup
                string zoomTarget = advice.GetValue("zoomTarget");
                if (zoomTarget != null)
                    newParams.subSlice = zoomTarget;

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

            if ((newParams.subSlice != null) &&
                (newParams.subSlice != ""))
                fw.Title = newParams.subSlice;
            else
                fw.Title = newParams.slice;
            mp.sliceMap.SetView(center, targetZoomLevel);

            // Don't need decorations in child window.
            Grid lr = mp.LayoutRoot;
            lr.Children.Remove(mp.image1);
            lr.Children.Remove(mp.infoLabel);
            lr.Children.Remove(mp.sliceLabel);
            mp.sliceMap.LogoVisibility = Visibility.Collapsed;
            mp.sliceMap.CopyrightVisibility = Visibility.Collapsed;
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
            //if (wc.IsBusy || (updateQueue.Count <= 0))
            if (updateQueue.Count <= 0)
                return;

            WebClient wc = new WebClient();
            wc.Encoding = System.Text.Encoding.UTF8;
            wc.Headers["Content-type"] = "application/x-www-form-urlencoded";
            wc.UploadStringCompleted += new UploadStringCompletedEventHandler(wc_UploadStringCompleted);

            Visual vis = updateQueue.Dequeue();

            // Build URI for query.
            string scriptName = "";

            // Status queries use get_status.php; data use get_data.php
            // *** Not pretty:  status query is indicated by null visualization
            if (vis == null)
            {
                scriptName = "get_all.php"; // Try using combined script.
                Uri queryURI = new Uri(phpBase + scriptName);
                wc.UploadStringAsync(queryURI, "POST", postData);
            }
            else
            {
                scriptName = "get_data.php";
                string statQuery = vis.statQuery;

                // Issue query for data needed.     
                string fullPostData = "statQuery=" + statQuery;
                if ((postData != null) && (postData != ""))
                    fullPostData = postData + "&" + fullPostData;
                Uri queryURI = new Uri(phpBase + scriptName);
                wc.UploadStringAsync(queryURI, "POST", fullPostData, vis);
            }
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
            foreach (Visual vis in visuals)
            {
               if (stats.ContainsKey(vis))
                {
                    Stat thisStat = stats[vis];
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


        private void SetMapParameters(Alist advice)
        {
            Location mapCenter = new Location(0.0, 0.0);
            double targetZoomLevel = 4;

            // -- Basic view parameters: center and zoom

            // latitude
            string centerLat = advice.GetValue("centerLat");
            if (centerLat != null)
                mapCenter.Latitude = Convert.ToDouble(centerLat);

            // longitude
            string centerLon= advice.GetValue("centerLon");
            if (centerLon != null)
                mapCenter.Longitude = Convert.ToDouble(centerLon);

            // zoomLevel
            string zoomLevel = advice.GetValue("zoomLevel");
            if (zoomLevel != null)
                targetZoomLevel = Convert.ToDouble(zoomLevel);
                        
            sliceMap.SetView(mapCenter, targetZoomLevel);


            // Map appearance: arial (default), road, black
            string mapMode = advice.GetValue("mapMode");
            if (mapMode == "road")
                sliceMap.Mode = new Microsoft.Maps.MapControl.RoadMode();
            if (mapMode == "black")
                BlackLayer.Opacity = 1;            
        }

        // If map view changes, may need to refresh some visuals.
        private void sliceMap_ViewChange(object sender, MapEventArgs e)
        {
            // *** Currently only arcs need redrawing, but should be a property or list of some kind.
            foreach (Visual vis in visuals)
            {
                if (inMySubSlice(vis) &&
                    (vis.infoType == "arc") &&
                    (controls.ContainsKey(vis.name)))
                {
                    DataPath arc = controls[vis.name] as DataPath;
                    if (OverlayLayer.Children.Contains(arc))
                        OverlayLayer.Children.Remove(arc);
                    arc.Waypoints = PointsForPath(vis);

                    Location location = GetSourceLocation(vis.objName);
                    arc.anchorLocation = location;
                    arc.processAttributes(vis.renderAttributes, null);
                    Point offset = new Point(arc.anchorOffset.X + arc.alignmentOffset.X,
                                             arc.anchorOffset.Y + arc.alignmentOffset.Y);
                    OverlayLayer.AddChild(arc, location, offset);
                }
            }
        }


        // Display an error popup with the message provided.
        private void ShowError(string message)
        {
            ErrorWindow win = new ErrorWindow();
            win.ErrorTextBlock.Text = message;
            win.Show();
        }
    }
}
