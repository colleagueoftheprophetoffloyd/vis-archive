using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Globalization;

namespace GENIVisuals
{
    public class SampleDataItem
    {
        public string Category { get; set; }
        private double[] _metrics = new double[2];
        public double RxThroughput { get { return _metrics[0]; } set { _metrics[0] = value; } }
        public double TxThroughput { get { return _metrics[1]; } set { _metrics[1] = value; } }

        public void SetMetric(int index, string stringValue)
        {
            CultureInfo ci = new CultureInfo("en-US");
            NumberFormatInfo nfi = ci.NumberFormat;
            _metrics[index] = double.Parse(stringValue, nfi);
        }
    }

    public class SampleDataLayer
    {
        private ObservableCollection<SampleDataItem> _data = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> Data
        { 
            get { return this._data; } 
        }

        public static SampleDataLayer GetDataLayer(string resourceName)
        {
            SampleDataLayer dataLayer = new SampleDataLayer();
            if (resourceName == null)
            {
                SampleDataItem di = new SampleDataItem();
                di.TxThroughput = 0;
                di.RxThroughput = 0;
                di.Category = "";
                dataLayer.Data.Add(di);
                return dataLayer;
            }

            // read data from embedded CSV flie
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                TextReader textReader = new StreamReader(assembly.GetManifestResourceStream(resourceName));
                string result = textReader.ReadToEnd();
                textReader.Close();
                // split lines and add data items
                string[] lines = result.Split('\n');
                foreach (string line in lines)
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        string[] values = line.Trim().Split(';');
                        SampleDataItem di = new SampleDataItem();
                        di.Category = values[0];
                        for (int i = 1; i < 2; i++)
                        {
                            di.SetMetric(i - 1, values[i]);
                        }

                        dataLayer._data.Add(di);
                    }
                }

                return dataLayer;
            }
            catch (Exception e)
            {
                return dataLayer;
            }
        }
    }
}
