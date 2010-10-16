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

namespace GENIVisuals.models
{
    public class Alist
    {
        public Dictionary<string, string> attributes;

        public Alist()
        {
            attributes = new Dictionary<string, string>();
        }

        // Initialize from string of format:
        // "attribute1=value1,attribute2=value2"
        public Alist(string alistString)
        {
            attributes = new Dictionary<string, string>();

            if ((alistString == null) || (alistString == ""))
                return;

            foreach (string pairString in alistString.Split(','))
            {
                string[] parts = pairString.Trim().Split('=');
                if (parts.Length == 2)
                    attributes[parts[0].Trim()] = parts[1].Trim();
            }
        }

        // Return the value associated with the specified attribute.
        // Return null (not exception) if no value associated.
        public string GetValue(string attribute)
        {
            if (attributes.ContainsKey(attribute))
                return attributes[attribute];
            else
                return null;
        }

        public void SetValue(string attribute, string value)
        {
            attributes[attribute] = value;
        }

        public void ClearValue(string attribute)
        {
            attributes.Remove(attribute);
        }

        // Clear all entries.
        public void Clear()
        {
            attributes.Clear();
        }

    }
}
