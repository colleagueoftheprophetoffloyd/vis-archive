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
                id = Convert.ToInt32((string) linkJson["id"]);
            if (linkJson["name"] != null)
                name = ((string) linkJson["name"]).Trim();
            if (linkJson["sourceNode"] != null)
                sourceNode = ((string) linkJson["sourceNode"]).Trim();
            if (linkJson["destNode"] != null)
                destNode = ((string) linkJson["destNode"]).Trim();
        }
    }
}
