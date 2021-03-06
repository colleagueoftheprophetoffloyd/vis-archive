﻿using System;
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
using AmCharts.Windows.QuickCharts;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace GENIVisuals
{
    public partial class ChartPage : UserControl
    {
        public ChartPage()
        {
            InitializeComponent();
        }

        private ObservableCollection<SampleDataItem> m_pageViewData;
        public ObservableCollection<SampleDataItem> PageViewData { get { return m_pageViewData; } }

        private bool m_pause = false;
        private double m_updateInterval = 500;

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SampleDataLayer pageViewDataLayer = SampleDataLayer.GetDataLayer(null);
            //SampleDataLayer pageViewDataLayer = SampleDataLayer.GetDataLayer("AmQuickChartsDemoSL.MultipleChartsDemo.pageviewsvisitors.csv");
            m_pageViewData = pageViewDataLayer.Data;

            DispatcherTimer dt = new DispatcherTimer();
            int min = (int)(m_updateInterval / (1000 * 60));
            int sec = (int)(m_updateInterval / 1000) - min * 60;
            int ms = (int)m_updateInterval - min * 60 * 1000 - sec * 1000;
            dt.Interval = new TimeSpan(0, 0, min, sec, ms); // dy, hr, min, sec, ms
            dt.Tick += new EventHandler(RequestNextUpdate);
            dt.Start();

            DataContext = this;
        }

        private void RequestNextUpdate(object sender, EventArgs e)
        {
            Random rand = new Random();
            double txThroughput = rand.NextDouble() * 100;
            double rxThroughput = rand.NextDouble() * 100;
            string timeStr = DateTime.Now.ToShortTimeString();
            this.AddDataPoint(txThroughput, rxThroughput, timeStr);
        }

        public string Title {
            get { return m_title.Text; } 
            set { m_title.Text = value; } 
        }

        public string Graph1Title
        {
            get { return lineGraph1.Title; }
            set { m_title.Text = value; } 
        }

        public string Graph2Title
        {
            get { return lineGraph2.Title; }
            set { m_title.Text = value; }
        }

        public double UpdateFrequency
        {
            get { return m_updateInterval; }
            set { m_updateInterval = value; }
        }

        public void AddDataPoint(double rxThroughput, double txThroughput, string timestamp)
        {
            if (!m_pause)
            {
                SampleDataItem item = new SampleDataItem();
                Random rand = new Random();
                item.TxThroughput = txThroughput;
                item.RxThroughput = rxThroughput;
                item.Category = timestamp;
                if (m_pageViewData.Count > 20)
                {
                    m_pageViewData.RemoveAt(0);
                }
                m_pageViewData.Add(item);
            }
        }

        private void pauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (!m_pause)
            {
                this.pauseButton.Content = "Resume";
                m_pause = true;
            }
            else
            {
                this.pauseButton.Content = "Pause";
                m_pause = false;
            }

        }
    }
}
