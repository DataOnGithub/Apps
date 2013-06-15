﻿using FSharp;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using NationalRail;
using System;
using System.Device.Location;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace UKTrains
{
    public partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!Settings.Get(Setting.LocationServicesEnabled) && !Settings.Get(Setting.LocationServicesPromptShown))
            {
                Settings.Set(Setting.LocationServicesPromptShown, true);
                var result = MessageBox.Show("This application uses your current location to improve the experience. Do you wish to give it permission to use your location?",
                                             "Location Services",
                                             MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    Settings.Set(Setting.LocationServicesEnabled, true);
                }
            }

            if (Settings.Get(Setting.LocationServicesEnabled))
            {
                var watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High)
                {
                    MovementThreshold = 20
                };

                watcher.PositionChanged += OnGeoPositionChanged;
                watcher.Start();
            }

            LoadStations();
        }

        private GeoCoordinate currentLocation;

        private void OnGeoPositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            bool previousEmpty = currentLocation == null || currentLocation.IsUnknown;
            currentLocation = e.Position.Location;
            if (previousEmpty)
            {
                LoadStations();
            }
        }

        private bool busy;

        private void OnRefreshButtonClick(object sender, EventArgs e)
        {
            if (busy)
            {
                return;
            }

            busy = true;
            LoadStations();
        }

        private void LoadStations()
        {
            nearestStations.ItemsSource = null;
            messageTextBlock.Text = null;
            messageTextBlock.Visibility = Visibility.Collapsed;
            if (!Settings.Get(Setting.LocationServicesEnabled))
            {
                messageTextBlock.Visibility = Visibility.Visible;
                messageTextBlock.Text = "Locations Services are disabled";
                busy = true;
            }
            else if (currentLocation == null || currentLocation.IsUnknown)
            {
                messageTextBlock.Visibility = Visibility.Visible;
                messageTextBlock.Text = "Acquiring location...";
                var indicator = new ProgressIndicator { IsVisible = true, IsIndeterminate = true, Text = "Acquiring Position..." };
                SystemTray.SetProgressIndicator(this, indicator);
                busy = true;
            }
            else
            {
                var gpsPosition = GeoUtils.LatLong.Create(currentLocation.Latitude, currentLocation.Longitude);
                LiveDepartures.getNearestStations(gpsPosition, 20).Display(
                    this,
                    "Loading stations...",
                    "You're outside of the UK",
                    messageTextBlock,
                    nearest => nearestStations.ItemsSource = nearest,
                    () => busy = false);
            }
        }

        private void OnSettingsButtonClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }

        private void OnStationButtonClick(object sender, RoutedEventArgs e)
        {
            var station = ((Tuple<string, Station>)((Button)sender).DataContext).Item2;
            NavigationService.Navigate(new Uri("/StationPage.xaml?stationCode=" + station.Code, UriKind.Relative));
        }

        private void OnRateAndReviewButtonClick(object sender, EventArgs e)
        {
            var task = new MarketplaceReviewTask();
            task.Show();
        }
    }
}