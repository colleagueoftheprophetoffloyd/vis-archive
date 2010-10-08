using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Controls.DataVisualization.Charting;

namespace GENIVisuals
{
    public partial class ValueHistory : VisualControl
    {
        public LineSeries AddLine()
        {
            LineSeries result = new LineSeries();
            Chart.Series.Add(result);
            return result;
        }

        public ValueHistory()
        {
            InitializeComponent();
        }
    }
}
