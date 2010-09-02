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
using System.Windows.Navigation;
using GENIVisuals.models;
using System.Windows.Browser;

namespace GENIVisuals
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();

            if (HtmlPage.Document.QueryString.ContainsKey("slice"))
                sliceBox.Text = HtmlPage.Document.QueryString["slice"];
            if (HtmlPage.Document.QueryString.ContainsKey("dbHost"))
                hostBox.Text = HtmlPage.Document.QueryString["dbHost"];
            if (HtmlPage.Document.QueryString.ContainsKey("dbUser"))
                userBox.Text = HtmlPage.Document.QueryString["dbUser"];
            if (HtmlPage.Document.QueryString.ContainsKey("dbName"))
                dbNameBox.Text = HtmlPage.Document.QueryString["dbName"];
            if (HtmlPage.Document.QueryString.ContainsKey("debugServer"))
                DebugServerTextBox.Text = HtmlPage.Document.QueryString["debugServer"];
        }

        // Executes when the user navigates to this page.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void attachButton_Click(object sender, RoutedEventArgs e)
        {
            SessionParameters parameters = new SessionParameters
            {
                slice = sliceBox.Text,
                dbHost = hostBox.Text,
                dbUser = userBox.Text,
                dbPassword = passwordBox.Password,
                dbName = dbNameBox.Text,
                useDebugServer = (useDebugServerCheckBox.IsChecked == true),
                debugServer = DebugServerTextBox.Text,
                useBogusData = (useBogusDataCheckBox.IsChecked == true)
            };
            PageSwitcher ps = this.Parent as PageSwitcher;
            MainPage mp = new MainPage(parameters);
            ps.Navigate(mp);
        }

    }
}
