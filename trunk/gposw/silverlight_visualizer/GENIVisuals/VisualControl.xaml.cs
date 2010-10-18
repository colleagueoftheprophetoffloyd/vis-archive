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
using System.Reflection;
using Microsoft.Maps.MapControl;
using GENIVisuals.models;

namespace GENIVisuals
{

    public partial class VisualControl : UserControl
    {

        public static readonly DependencyProperty CurrentStatusProperty =
                DependencyProperty.RegisterAttached(
                    "CurrentStatus",
                    typeof(string),
                    typeof(VisualControl),
                    new PropertyMetadata(null, new PropertyChangedCallback(OnCurrentStatusChanged)));

        public string CurrentStatus
        {
            get
            {
                return (string)GetValue(CurrentStatusProperty);
            }
            set
            {
                SetValue(CurrentStatusProperty, value);
            }
        }

        // The visual on whose behalf control is displayed.
        public models.Visual ParentVisual { get; set; }

        // Next three members are about the positioning of the object on the map.
        // 
        // Anchor location names the (lat,lon) location that anchors the object.
        // Anchor offset is a user-provided offset from that point.
        // Alignment offset is an internally-computed offset used to align the
        // object according to user request.
        //
        // So, a control of size (100,100) that the user has asked to be centered
        // at an offset of (200,300) from 23N, 75W might have an anchor location
        // of (23, -75), an anchor offset of (200,300), and an alignment offset
        // of (-50,-50) to cause it to be centered.
        public Location anchorLocation { get; set; }
        public Point anchorOffset { get; set; }
        public Point alignmentOffset { get; set; }

        protected Storyboard activeStoryboard = null;
        private static void OnCurrentStatusChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            VisualControl control = sender as VisualControl;
            string newStatus = control.CurrentStatus;
            control.SetStatus(newStatus);
        }

        protected virtual void SetStatus(string status)
        {
            string newStatus = status;

            if (! canDisplay(newStatus))
                newStatus = statusNames[0];

            if (activeStoryboard != null)
                activeStoryboard.Stop();
            activeStoryboard = StoryboardForStatus(newStatus);
            if (activeStoryboard != null)
                activeStoryboard.Begin();
        }

        private static string[] statusNames = 
        {
            "normal",
            "hidden",
            "alert",
            "throb",
            "rainbow"
        };
        private static List<string> displayableStatusList = new List<string>(statusNames);

        protected virtual List<string> getDisplayableStatusList()
        {
            return displayableStatusList;
        }

        public virtual Boolean canDisplay(string status)
        {
            List<string> displayableStatusList = getDisplayableStatusList();
            return displayableStatusList.Contains(status);
        }

        protected virtual Storyboard StoryboardForStatus(string status)
        {
            if (status == "hidden")
                return HiddenStoryboard;
            if (status == "alert")
                return AlertStoryboard;
            if (status == "throb")
                return ThrobStoryboard;
            if (status == "rainbow")
                return RainbowStoryboard;

            return null;
        }


        //
        // Process any rendering instructions from the provided Alist.
        //
        // Attributes may be applied directly to the VisualControl, but
        // more likely apply to the container in which it's held or
        // indicate its placement on the map.
        //
        // Update provided container object as appropriate (if non-null).
        //
        // Update alignmentOffset with offset information.
        //
        public virtual void processAttributes(Alist attributes, VisualControl container)
        {
            string value;

            // Check height and width first, because
            // they influence later items.

            value = attributes.GetValue("width");
            if (value != null)
            {
                Width = Convert.ToDouble(value);
            }

            value = attributes.GetValue("height");
            if (value != null)
            {
                Height = Convert.ToDouble(value);
            }

           
            // Initialize alignment offset to center object.
            Point newAlignOffset = new Point(-Width/2, -Height/2);
            if (double.IsNaN(newAlignOffset.X) ||
                double.IsInfinity(newAlignOffset.X))
            {
                newAlignOffset.X = 0;
            }

            if (double.IsNaN(newAlignOffset.Y) ||
                double.IsInfinity(newAlignOffset.Y))
            {
                newAlignOffset.Y = 0;
            }
            

            // Container attributes.

            value = attributes.GetValue("opacity");
            if (value != null) {
                Opacity = Convert.ToDouble(value);
            }

            value = attributes.GetValue("background");
            if (value != null) {
                Color bgColor = GetThisColor(value);
                Background = new SolidColorBrush(bgColor);
            }

            value = attributes.GetValue("alignment");
            if (value != null)
            {
                switch (value.ToLower())
                {
                    case "center":
                        // No change, this is default.
                        break;

                    case "top":
                        newAlignOffset.Y += Height / 2;
                        break;

                    case "bottom":
                        newAlignOffset.Y -= Height / 2;
                        break;

                    case "left":
                        newAlignOffset.X += Width / 2;
                        break;

                    case "right":
                        newAlignOffset.X -= Width / 2;
                        break;

                    case "topleft":
                        newAlignOffset.Y += Height / 2;
                        newAlignOffset.X += Width / 2;
                        break;

                    case "bottomleft":
                        newAlignOffset.Y -= Height / 2;
                        newAlignOffset.X += Width / 2;
                        break;

                    case "topright":
                        newAlignOffset.Y += Height / 2;
                        newAlignOffset.X -= Width / 2;
                        break;

                    case "bottomright":
                        newAlignOffset.Y -= Height / 2;
                        newAlignOffset.X -= Height / 2;
                        break;
                }
            }

            if (double.IsNaN(newAlignOffset.X) ||
                double.IsInfinity(newAlignOffset.X))
            {
                newAlignOffset.X = 0;
            }

            if (double.IsNaN(newAlignOffset.Y) ||
                double.IsInfinity(newAlignOffset.Y))
            {
                newAlignOffset.Y = 0;
            }


            // Explicit offset attributes.

            Point newAnchorOffset = new Point(0, 0);
            value = attributes.GetValue("xoffset");
            if (value != null)
            {
                newAnchorOffset.X += Convert.ToInt32(value);
            }

            value = attributes.GetValue("yoffset");
            if (value != null)
            {
                newAnchorOffset.Y += Convert.ToInt32(value);
            }

            anchorOffset = newAnchorOffset;
            alignmentOffset = newAlignOffset;
        }


        public Color GetThisColor(string colorString)
        {
            Type colorType = (typeof(System.Windows.Media.Colors));
            if (colorType.GetProperty(colorString) != null)
            {
                object o = colorType.InvokeMember(colorString, BindingFlags.GetProperty, null, null, null); 
                if (o != null)
                {
                    return (Color)o;
                }

            }
            return Colors.Black;
        }


        public VisualControl()
        {
            InitializeComponent();
        }
    }
}
