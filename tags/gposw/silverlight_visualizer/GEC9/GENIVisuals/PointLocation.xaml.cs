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
    public partial class PointLocation : VisualControl
    {

        protected override void SetStatus(string status)
        {
            string newStatus = status;

            if (!canDisplay(newStatus))
                newStatus = statusNames[0];

            if (newStatus == "active")
            {
                StarPath.Fill = ActiveFillBrush;
                StarScale.ScaleX = 1.5;
                StarScale.ScaleY = 1.5;
            }
            else
            {
                StarPath.Fill = FillBrush;
                StarScale.ScaleX = 1.0;
                StarScale.ScaleY = 1.0;
            }

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
            "rainbow",
            "active"
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

            return null;
        }


        public PointLocation()
        {
            InitializeComponent();

            Storyboard.SetTarget(AlertAnimation, StarPath);
            Storyboard.SetTargetProperty(AlertAnimation, new PropertyPath("(Fill).(SolidColorBrush.Color)"));

            Storyboard.SetTarget(RainbowAnimation, StarPath);
            Storyboard.SetTargetProperty(RainbowAnimation, new PropertyPath("(Fill).(SolidColorBrush.Color)"));
        }
    }
}
