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
using System.Collections.ObjectModel;

namespace GENIVisuals.models
{
    public class NodeStats
    {
        public ObservableCollection<NodeStat> data;

        public NodeStats()
        {
            data = new ObservableCollection<NodeStat>();

            data.Add(new NodeStat() { name = "Load", statValue = 0 });
        }

    }

    public class NodeStat : INotifyPropertyChanged
    {
        public string name { get; set; }

        private double _value;

        public double statValue
        {
            get {return _value;}
            set
            {
                _value = value;
                NotifyPropertyChanged("statValue");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string PropertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
            }
        }

    }
}
