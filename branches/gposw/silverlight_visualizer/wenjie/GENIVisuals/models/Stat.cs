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
using System.Json;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Generic;


namespace GENIVisuals.models
{
    public class Stat : INotifyPropertyChanged
    {
        private string _statType;
        private int _history; // Number of samples of history (or zero for current value only)
        private ObservableCollection<KeyValuePair<DateTime, double>> _values =
            new ObservableCollection<KeyValuePair<DateTime, double>>(); // Valid only if _history is non-zero
        private double _currentValue; // Valid only if _history is zero.


        // Update values from content of JSON.
        public void UpdateWith(JsonValue dataJson)
        {
            throw new NotImplementedException();
        }

        public string statType
        {
            get { return _statType; }
            set
            {
                _statType = value;
                NotifyPropertyChanged("statType");
            }
        }

        public void clearValues()
        {
            _values.Clear();
            NotifyPropertyChanged("values");
        }

        public void addValue(DateTime time, double value)
        {
            // If there's already a value at this time, keep it (don't modify history).
            // *** This is probably wrong, but removing the point and replacing it crashes. ***
            foreach (KeyValuePair<DateTime, double> pair in _values)
                if (pair.Key.Equals(time))
                    return;

            // Add in new value.
            KeyValuePair<DateTime, double> newDatum = new KeyValuePair<DateTime, double>(time, value);
            _values.Add(newDatum);

            // Housekeeping: trim list of values and notify
            truncateValues();
            NotifyPropertyChanged("values");
        }

        private void truncateValues()
        {    
            int excess = _values.Count - _history;

            if (excess > 0)
            {
                for (int i = 0; i < excess; i++)
                {
                    DateTime earliest = DateTime.MaxValue;
                    KeyValuePair<DateTime, double> deleteMe = new KeyValuePair<DateTime, double>(earliest, 0.0);
                    foreach (KeyValuePair<DateTime, double> thisPair in _values)
                        if (thisPair.Key < earliest)
                        {
                            earliest = thisPair.Key;
                            deleteMe = thisPair;
                        }
                    _values.Remove(deleteMe);
                }
            }
        }

        public ObservableCollection<KeyValuePair<DateTime, double>> values
        {
            get { return _values; }
        }

        public int history
        {
            get { return _history; }
            set
            {
                _history = value;
                _values.Clear();
                _currentValue = 0.0;
                NotifyPropertyChanged("history");
                NotifyPropertyChanged("values");
                NotifyPropertyChanged("currentValue");
           }
        }

        public double currentValue
        {
            get { return _currentValue; }
            set
            {
                _currentValue = value;
                NotifyPropertyChanged("currentValue");
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
