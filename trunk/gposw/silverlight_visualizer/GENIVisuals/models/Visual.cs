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
        public string renderAdvice { get; set; }        
        public Alist positionAdvice { get; set; }

        public Visual(JsonValue visualJson)
        {
            // Parse visual content out of JSON
            // Ignoring id and sliceName fields from database.

            if (visualJson["sequence"] != null)
                sequence = Convert.ToInt32(visualJson["sequence"].ToString().Replace('"', ' ').Trim());
            if (visualJson["infoType"] != null)
                infoType = visualJson["infoType"].ToString().Replace('"', ' ').Trim();
            if (visualJson["objType"] != null)
                objType = visualJson["objType"].ToString().Replace('"', ' ').Trim();
            if (visualJson["objName"] != null)
                objName = visualJson["objName"].ToString().Replace('"', ' ').Trim();
            if (visualJson["statType"] != null)
                statType = visualJson["statType"].ToString().Replace('"', ' ').Trim();
            if (visualJson["statHistory"] != null)
                statHistory = Convert.ToInt32(visualJson["statHistory"].ToString().Replace('"', ' ').Trim());
            minValue = null;
            if (visualJson["minValue"] != null)
            {
                string strVal = visualJson["minValue"].ToString().Replace('"', ' ').Trim();
                if (strVal != "null")
                    minValue = Convert.ToDouble(strVal);
            }
            maxValue = null;
            if (visualJson["maxValue"] != null)
            {
                string strVal = visualJson["maxValue"].ToString().Replace('"', ' ').Trim();
                if (strVal != "null")
                    maxValue = Convert.ToDouble(strVal);
            }
            if (visualJson["statQuery"] != null)
                statQuery = visualJson["statQuery"].ToString().Replace('"', ' ').Trim();
            if (visualJson["statusHandle"] != null)
                statusHandle = visualJson["statusHandle"].ToString().Replace('"', ' ').Trim();
            if (visualJson["renderAdvice"] != null)
                renderAdvice = visualJson["renderAdvice"].ToString().Replace('"', ' ').Trim();
            if (visualJson["positionAdvice"] != null)
                positionAdvice = new Alist(visualJson["positionAdvice"]);
            else
                positionAdvice = new Alist();
        }
    }
}
