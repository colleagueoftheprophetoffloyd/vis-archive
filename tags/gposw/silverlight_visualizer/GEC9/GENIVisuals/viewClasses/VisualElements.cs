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

    // For keeping track of the objects created on behalf
    // of displaying a particular visual.
    // Might be nice to carry types around and do some checking.
    public class VisualElements
    {
        private Dictionary<string, Object> elements;

        public VisualElements()
        {
            elements = new Dictionary<string, Object>();
        }

        // Return the object associated with the specified property.
        // Return null (not exception) if no value associated.
        public Object GetProperty(string property)
        {
            if (elements.ContainsKey(property))
                return elements[property];
            else
                return null;
        }

        // Set the object associated with the specified property.
        public void SetProperty(string property, Object obj)
        {
            elements[property] = obj;
        }

        // Clear the object associated with the specified property.
        public void ClearProperty(string property)
        {
            elements.Remove(property);
        }

        // Clear all entries.
        public void Clear()
        {
            elements.Clear();
        }


        // Constant values for properties.
        public const string StatisticsProperty = "statistics";
        public const string StatusProperty = "status";
        public const string StoryboardProperty = "storyboard";
        public const string AnimationElementsProperty = "animationElements";
        public const string DataSourceStatsContainerProperty = "statsContainer";
        public const string StatusAnimationTargetProperty = "statusAnimationTarget";
        public const string DataSourceLabelProperty = "dataSourceLabel";
        public const string LinkPolyLineProperty = "linkPolyLine";
    }
}
