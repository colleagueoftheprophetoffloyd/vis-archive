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


namespace GENIVisuals.models
{
    public class Visual
    {
        public string name { get; set; }
        public string sliceName { get; set; }
        public string subSlice { get; set; }
        public int sequence { get; set; } // Order in which to display items
        public string infoType { get; set; } // Type of visual (status, numeric)
        public string objType { get; set; } // Type of data source (node, link)
        public string objName { get; set; } // ID of data source
        public string statType { get; set; } // Name of statistic
        public int statHistory { get; set; }
        public Nullable<double> minValue { get; set; }
        public Nullable<double> maxValue { get; set; }
        public string statQuery { get; set; }
        public string statusHandle { get; set; }
        public Alist renderAttributes { get; set; }

        public Visual(JsonValue visualJson)
        {
            // Parse visual content out of JSON
            // Ignoring id field from database.

            if (visualJson["sliceName"] != null)
                sliceName = ((string)visualJson["sliceName"]).Trim();
            if (visualJson["name"] != null)
                name = ((string)visualJson["name"]).Trim();
            if (visualJson["subSlice"] != null)
                subSlice = ((string) visualJson["subSlice"]).Trim();
            if (visualJson["sequence"] != null)
                sequence = Convert.ToInt32((string) visualJson["sequence"]);
            if (visualJson["infoType"] != null)
                infoType = ((string) visualJson["infoType"]).Trim();
            if (visualJson["objType"] != null)
                objType = ((string) visualJson["objType"]).Trim();
            if (visualJson["objName"] != null)
                objName = ((string) visualJson["objName"]).Trim();
            if (visualJson["statType"] != null)
                statType = ((string) visualJson["statType"]).Trim();
            if (visualJson["statHistory"] != null)
                statHistory = Convert.ToInt32((string) visualJson["statHistory"]);
            minValue = null;
            if (visualJson["minValue"] != null)
            {
                string strVal = ((string) visualJson["minValue"]).Trim();
                if (strVal != "null")
                    minValue = Convert.ToDouble(strVal);
            }
            maxValue = null;
            if (visualJson["maxValue"] != null)
            {
                string strVal = ((string) visualJson["maxValue"]).Trim();
                if (strVal != "null")
                    maxValue = Convert.ToDouble(strVal);
            }
            if (visualJson["statQuery"] != null)
                statQuery = ((string) visualJson["statQuery"]).Trim();
            if (visualJson["statusHandle"] != null)
                statusHandle = ((string) visualJson["statusHandle"]).Trim();
            if (visualJson["renderAttributes"] != null)
                renderAttributes = new Alist(((string) visualJson["renderAttributes"]).Trim());
            else
                renderAttributes = new Alist();
        }
    }
}
