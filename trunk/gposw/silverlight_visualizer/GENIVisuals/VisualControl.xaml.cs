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
        // Process any rendering instructions from the provided Alist
        // and add to the parent UIElement provided.
        //
        public virtual void processAttributes(Alist attributes, UIElement parent)
        {
            Alist result = new Alist();

            foreach (string attributeName in attributes.attributes.Keys)
            {
                string value = attributes.GetValue(attributeName);
                switch (attributeName.ToLower())
                {
                    default:
                        // ignore unknown attributes?
                        break;
                }
            }

        }

        public VisualControl()
        {
            InitializeComponent();
        }
    }
}
