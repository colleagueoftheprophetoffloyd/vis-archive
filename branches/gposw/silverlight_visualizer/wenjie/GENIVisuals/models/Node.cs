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
using System.ComponentModel;
using System.Json;

namespace GENIVisuals.models
{
    public class Node : INotifyPropertyChanged
    {
        private int id;
        private string name;
        private double latitude;
        private double longitude;
        private string type;
        private string icon;
        private int load;

        public event PropertyChangedEventHandler PropertyChanged;

        public Node(JsonValue nodeJson)
        {
            // Parse node content out of JSON
            if (nodeJson["id"] != null)
                id = Convert.ToInt32(nodeJson["id"].ToString().Replace('"', ' ').Trim());
            if (nodeJson["name"] != null)
                name = nodeJson["name"].ToString().Replace('"', ' ').Trim();
            if (nodeJson["latitude"] != null)
                latitude = Convert.ToDouble(nodeJson["latitude"].ToString().Replace('"', ' ').Trim());
            if (nodeJson["longitude"] != null)
                longitude = Convert.ToDouble(nodeJson["longitude"].ToString().Replace('"', ' ').Trim());
            if (nodeJson["type"] != null)
                type = nodeJson["type"].ToString().Replace('"', ' ').Trim();
            if (nodeJson["icon"] != null)
                icon = nodeJson["icon"].ToString().Replace('"', ' ').Trim();

        }

        public int Id
        {
            get { return id; }
            set
            {
                id = value;
                NotifyPropertyChanged("Id");
            }
        }

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                NotifyPropertyChanged("Name");
            }
        }

        public double Latitude
        {
            get { return latitude; }
            set
            {
                latitude = value;
                NotifyPropertyChanged("Latitude");
            }
        }

        public double Longitude
        {
            get { return longitude; }
            set
            {
                longitude = value;
                NotifyPropertyChanged("Longitude");
            }
        }

        public string Type
        {
            get { return type; }
            set
            {
                type = value;
                NotifyPropertyChanged("Type");
            }
        }

        public string Icon
        {
            get { return icon; }
            set
            {
                icon = value;
                NotifyPropertyChanged("Icon");
            }
        }

        public int Load
        {
            get { return load; }
            set
            {
                load = value;
                NotifyPropertyChanged("Load");
            }
        }

        // Per suggestion in
        // http://www.silverlight.net/learn/tutorials/databinding-cs
        public void NotifyPropertyChanged(string PropertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this,
                                new PropertyChangedEventArgs(PropertyName));
            }
        }
    }
}
