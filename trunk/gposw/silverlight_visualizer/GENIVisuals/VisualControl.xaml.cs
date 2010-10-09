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

        public models.Visual ParentVisual { get; set; }

        private Storyboard activeStoryboard = null;
        private static void OnCurrentStatusChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            VisualControl control = sender as VisualControl;
            string newStatus = control.CurrentStatus;

            if (! control.canDisplay(newStatus))
                newStatus = statusNames[0];

            if (control.activeStoryboard != null)
                control.activeStoryboard.Stop();
            control.activeStoryboard = control.StoryboardForStatus(newStatus);
            if (control.activeStoryboard != null)
                control.activeStoryboard.Begin();
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
        // Update provided container object as appropriate (if non-null),
        // and return a Point indicating offset information.
        //
        public virtual Point processAttributes(Alist attributes, VisualControl container)
        {
            Point offset = new Point(-Width/2, -Height/2);    // Default offset is to center object

            foreach (string attributeName in attributes.attributes.Keys)
            {
                string value = attributes.GetValue(attributeName);
                switch (attributeName.ToLower())
                {
                    // Container attributes.

                    case "opacity":
                        Opacity = Convert.ToDouble(value);
                        break;

                    case "background":
                        Color bgColor = GetThisColor(value);
                        Background = new SolidColorBrush(bgColor);
                        break;


                    // Offset attributes.

                    case "xoffset":
                        offset.X += Convert.ToInt32(value);
                        break;

                    case "yoffset":
                        offset.Y += Convert.ToInt32(value);
                        break;

                    case "alignment":
                        switch (value.ToLower())
                        {
                            case "center":
                                // No change, this is default.
                                break;

                            case "top":
                                offset.Y += Height / 2;
                                break;

                            case "bottom":
                                offset.Y -= Height / 2;
                                break;

                            case "left":
                                offset.X += Width / 2;
                                break;

                            case "right":
                                offset.X -= Width / 2;
                                break;

                            case "topleft":
                                offset.Y += Height / 2;
                                offset.X += Width / 2;
                                break;

                            case "bottomleft":
                                offset.Y -= Height / 2;
                                offset.X += Width / 2;
                                break;

                            case "topright":
                                offset.Y += Height / 2;
                                offset.X -= Width / 2;
                                break;

                            case "bottomright":
                                offset.Y -= Height / 2;
                                offset.X -= Height / 2;
                                break;
                        }
                        break; // end case "alignment"

                    default:
                        // ignore unknown attributes?
                        break;
                }
            }

            if (double.IsNaN(offset.X) ||
                double.IsInfinity(offset.X))
            {
                offset.X = 0;
            }

            if (double.IsNaN(offset.Y) ||
                double.IsInfinity(offset.Y))
            {
                offset.Y = 0;
            }

            return offset;
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
