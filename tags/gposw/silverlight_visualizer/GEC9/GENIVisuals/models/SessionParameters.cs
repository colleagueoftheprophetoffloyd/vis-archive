using System;
using System.Net;
using System.Windows;
using System.Collections.ObjectModel;
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
        public string slice { get; set; }
        public string subSlice { get; set; }
        public string dbHost { get; set; }
        public string dbUser { get; set; }
        public string dbPassword { get; set; }
        public string dbName { get; set; }
        public bool useDebugServer { get; set; }
        public string debugServer { get; set; }
        public bool showDebugMessages { get; set; }
        public bool useBogusData { get; set; }

        
        public SessionParameters(SessionParameters original)
        {
            slice = original.slice;
            subSlice = original.subSlice;
            dbHost = original.dbHost;
            dbUser = original.dbUser;
            dbPassword = original.dbPassword;
            dbName = original.dbName;
            useDebugServer = original.useDebugServer;
            debugServer = original.debugServer;
            showDebugMessages = original.showDebugMessages;
            useBogusData = original.useBogusData;
        }

        public SessionParameters()
        {
        }

    }
}
