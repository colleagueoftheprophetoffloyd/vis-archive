using System;
using System.Net;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace GENIVisuals.models
{
    public class SessionParameters
    {
        public SessionParameters(SessionParameters copy)
        {
            slice = copy.slice;
            dbHost = copy.dbHost;
            dbUser = copy.dbUser;
            dbPassword = copy.dbPassword;
            dbName = copy.dbName;
            useDebugServer = copy.useDebugServer;
            debugServer = copy.debugServer;
            useBogusData = copy.useBogusData;
            makePeriodicQuery = copy.makePeriodicQuery;
            topologyVisuals = new List<Visual>(copy.topologyVisuals);
        }

        public SessionParameters() {
            topologyVisuals = new List<Visual>();
        }

        public string slice { get; set; }
        public string dbHost { get; set; }
        public string dbUser { get; set; }
        public string dbPassword { get; set; }
        public string dbName { get; set; }
        public bool useDebugServer { get; set; }
        public string debugServer { get; set; }
        public bool useBogusData { get; set; }
        public bool makePeriodicQuery { get; set; }
        public List<Visual> topologyVisuals { get; set; }
    }
}
