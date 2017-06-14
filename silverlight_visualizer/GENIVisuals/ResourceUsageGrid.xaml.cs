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

namespace GENIVisuals
{
    public partial class ResourceUsageGrid : VisualControl
    {
        private const int _numColumns = 10;

        private string _resourceName = "";
        private List<Border> _itemBorders = new List<Border>();

        public static readonly DependencyProperty TotalProperty =
         DependencyProperty.RegisterAttached(
             "Total",
             typeof(int),
             typeof(ResourceUsageGrid),
             new PropertyMetadata(0, new PropertyChangedCallback(OnTotalChanged)));

        public static readonly DependencyProperty InUseProperty =
         DependencyProperty.RegisterAttached(
             "InUse",
             typeof(int),
             typeof(ResourceUsageGrid),
             new PropertyMetadata(0, new PropertyChangedCallback(OnInUseChanged)));

        //public static readonly DependencyProperty CurrentStatusProperty =
        //    DependencyProperty.RegisterAttached(
        //        "CurrentStatus",
        //        typeof(string),
        //        typeof(ResourceUsageGrid),
        //        new PropertyMetadata(null, new PropertyChangedCallback(OnCurrentStatusChanged)));

        //public string CurrentStatus
        //{
        //    get
        //    {
        //        return (string)GetValue(CurrentStatusProperty);
        //    }
        //    set
        //    {
        //        SetValue(CurrentStatusProperty, value);
        //    }
        //}

        //private static void OnCurrentStatusChanged(object sender, DependencyPropertyChangedEventArgs args)
        //{
        //    ResourceUsageGrid grid = sender as ResourceUsageGrid;
        //    string newStatus = grid.CurrentStatus;

        //    if (!canDisplay(newStatus))
        //        newStatus = statusNames[0];

        //    if (grid.activeStoryboard != null)
        //        grid.activeStoryboard.Stop();
        //    grid.activeStoryboard = grid.StoryboardForStatus(newStatus);
        //    if (grid.activeStoryboard != null)
        //        grid.activeStoryboard.Begin();
        //}

        //private static string[] statusNames = 
        //{
        //    "normal",
        //    "alert",
        //    "throb",
        //    "rainbow"
        //};
        //private static List<string> displayableStatusList = new List<string>(statusNames);

        //private Storyboard activeStoryboard = null;

        //public static Boolean canDisplay(string status)
        //{
        //    return displayableStatusList.Contains(status);
        //}

        //private Storyboard StoryboardForStatus(string status)
        //{
        //    if (status == "alert")
        //        return AlertStoryboard;
        //    if (status == "throb")
        //        return ThrobStoryboard;
        //    if (status == "rainbow")
        //        return RainbowStoryboard;

        //    return null;
        //}





        public ResourceUsageGrid()
        {
            // Required to initialize variables
            InitializeComponent();
            BuildGrid();
            RefreshGrid();
        }

        public string ResourceName
        {
            get { return _resourceName; }
            set
            {
                if (_resourceName != value)
                {
                    _resourceName = value;
                    NameBlock.Text = _resourceName;
                }
            }
        }


        public int Total
        {
            get
            {
                return (int)GetValue(TotalProperty);
            }
            set
            {
                SetValue(TotalProperty, value);
                BuildGrid();
                RefreshGrid();
            }
        }


        public int InUse
        {
            get
            {
                return (int)GetValue(InUseProperty);
            }
            set
            {
                SetValue(InUseProperty, value);
                RefreshGrid();
            }
        }


        public void BuildGrid()
        {
            int numRows = (Total + _numColumns - 1) / _numColumns;

            ResourceGrid.RowDefinitions.Clear();
            for (int row = 0; row < numRows; row++)
            {
                RowDefinition rowDef = new RowDefinition();
                ResourceGrid.RowDefinitions.Add(rowDef);
            }

            ResourceGrid.ColumnDefinitions.Clear();
            for (int col = 0; col < _numColumns; col++)
            {
                ColumnDefinition colDef = new ColumnDefinition();
                ResourceGrid.ColumnDefinitions.Add(colDef);
            }

            _itemBorders.Clear();
            for (int row = 0; row < numRows; row++)
            {
                for (int col = 0; col < _numColumns; col++)
                {
                    Rectangle rect = new Rectangle();
                    rect.SetValue(Grid.RowProperty, row);
                    rect.SetValue(Grid.ColumnProperty, col);
                    rect.SetValue(Rectangle.FillProperty, GreyFill);

                    Border border = new Border();
                    border.SetValue(Grid.RowProperty, row);
                    border.SetValue(Grid.ColumnProperty, col);
                    border.SetValue(Border.BorderBrushProperty, BlackBrush);
                    border.BorderThickness = new Thickness(1.0);
                    border.CornerRadius = new CornerRadius(5.0);

                    ResourceGrid.Children.Add(rect);
                    ResourceGrid.Children.Add(border);
                    _itemBorders.Add(border);
                }
            }
        }

        public void RefreshGrid()
        {
            int numRows = (Total + _numColumns - 1) / _numColumns;

            for (int row = 0; row < numRows; row++)
            {
                for (int col = 0; col < _numColumns; col++)
                {
                    int index = row * _numColumns + col;
                    Border border = _itemBorders[index];
                    if (index < InUse)
                        border.SetValue(Border.BackgroundProperty, InUseFill);
                    else if (index < Total)
                        border.SetValue(Border.BackgroundProperty, AvailableFill);
                    else
                        border.SetValue(Border.BackgroundProperty, GreyFill);
                }
            }
        }

        private static void OnTotalChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            ResourceUsageGrid grid = sender as ResourceUsageGrid;

            grid.BuildGrid();
        }

        private static void OnInUseChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            ResourceUsageGrid grid = sender as ResourceUsageGrid;

            grid.RefreshGrid();
        }
    }
}
