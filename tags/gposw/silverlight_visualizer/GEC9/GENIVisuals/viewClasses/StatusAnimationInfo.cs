using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace GENIVisuals.viewClasses
{
    public class StatusAnimationInfo
    {
        public Storyboard storyboard { get; set; }
        public List<UIElement> elements { get; set; }

        public StatusAnimationInfo()
        {
            elements = new List<UIElement>();
        }
    }
}
