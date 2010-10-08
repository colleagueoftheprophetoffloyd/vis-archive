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
