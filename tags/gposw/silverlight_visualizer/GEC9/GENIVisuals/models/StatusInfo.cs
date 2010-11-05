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
    public class StatusInfo
    {
        private string _handle;
        private string _status;

        public event PropertyChangedEventHandler PropertyChanged;

        public StatusInfo(JsonValue statusInfoJson)
        {
            // Parse status content out of JSON
            if (statusInfoJson["handle"] != null)
                _handle = ((string) statusInfoJson["handle"]).Trim();
            if (statusInfoJson["status"] != null)
                _status = ((string) statusInfoJson["status"]).Trim();
        }

        public string Handle
        {
            get { return _handle; }
            set
            {
                _handle = value;
                NotifyPropertyChanged("Handle");
            }
        }


        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                NotifyPropertyChanged("Status");
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
