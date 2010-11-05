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
using GENIVisuals.models;

namespace GENIVisuals
{
    public partial class ValueHistory : VisualControl
    {
        public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.RegisterAttached(
            "Mimimum",
            typeof(double?),
            typeof(ValueHistory),
            new PropertyMetadata(null));

        public double? Minimum
        {
            get
            {
                return (double?)GetValue(MinimumProperty);
            }
            set
            {
                SetValue(MinimumProperty, value);
                if (value.HasValue)
                {
                    LinearAxis axis = Chart.Axes[1] as LinearAxis;
                    axis.Minimum = value.Value;
                }
            }
        }


        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.RegisterAttached(
            "Maximum",
            typeof(double?),
            typeof(ValueHistory),
            new PropertyMetadata(null));

        public double? Maximum
        {
            get
            {
                return (double?)GetValue(MaximumProperty);
            }
            set
            {
                SetValue(MaximumProperty, value);
                if (value.HasValue)
                {
                    LinearAxis axis = Chart.Axes[1] as LinearAxis;
                    axis.Maximum = value.Value;
                }
            }
        }


        private static Color[] colors =
        {
            Colors.Blue,
            Colors.Red,
            Colors.Orange,
            Colors.Magenta,
            Colors.Black
        };
        private static int numColors = 5;

        private int numLines = 0;

        public LineSeries AddLine()
        {
            Brush brush = new SolidColorBrush(colors[numLines % numColors]);
            LineSeries result = new LineSeries();
            result.PolylineStyle = LineStyle;

            Style dpStyle = new Style(typeof(LineDataPoint));
            Setter opacitySetter = new Setter(OpacityProperty, 0.0);
            Setter bgSetter = new Setter(BackgroundProperty, brush);
            dpStyle.Setters.Add(opacitySetter);
            dpStyle.Setters.Add(bgSetter);
            result.DataPointStyle = dpStyle;

            numLines++;
            Chart.Series.Add(result);
            return result;
        }

        public ValueHistory()
        {
            InitializeComponent();
        }
    }
}
