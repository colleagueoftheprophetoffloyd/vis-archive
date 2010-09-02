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
    public class Link
    {
        public int id;
        public string name;
        public string sourceNode;
        public string destNode;

        public Link(JsonValue linkJson)
        {
            // Parse link content out of JSON.

            if (linkJson["id"] != null)
                id = Convert.ToInt32(linkJson["id"].ToString().Replace('"', ' ').Trim());
            if (linkJson["name"] != null)
                name = linkJson["name"].ToString().Replace('"', ' ').Trim();
            if (linkJson["sourceNode"] != null)
                sourceNode = linkJson["sourceNode"].ToString().Replace('"', ' ').Trim();
            if (linkJson["destNode"] != null)
                destNode = linkJson["destNode"].ToString().Replace('"', ' ').Trim();
        }
    }
}
