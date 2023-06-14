using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Xamarin.Essentials;
using System.Linq;

namespace bcompass
{
    public class BCompassViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        static Location blockBusterLoc = new Location(44.067365, -121.303486);

        Location currentLocation = new Location(0, 0);
        private double distanceFromBB;
        private string _distanceFromBBText;
        private double _compassRotation;
        private string _displayMessage;
        private string _accReadingX;
        private string _accReadingY;
        private string _accReadingZ;
        private bool _setToMiles;
        public string DistanceFromBBText
        {
            get => _distanceFromBBText;
            set
            {
                if (_distanceFromBBText != value)
                {
                    _distanceFromBBText = value;
                    OnPropertyChanged();
                }
            }
        }

        public string DisplayMessage
        {
            get => _displayMessage;
            set
            {
                if (_displayMessage != value)
                {
                    _displayMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public double CompassRotation
        {
            get => _compassRotation;
            set
            {
                if (_compassRotation != value)
                {
                    _compassRotation = value;
                    OnPropertyChanged();
                }
            }
        }

        public string AccReadingX
        {
            get => _accReadingX;
            set
            {
                if (_accReadingX != value)
                {
                    _accReadingX = value;
                    OnPropertyChanged();
                }
            }
        }

        public string AccReadingY
        {
            get => _accReadingY;
            set
            {
                if (_accReadingY != value)
                {
                    _accReadingY = value;
                    OnPropertyChanged();
                }
            }
        }
        public string AccReadingZ
        {
            get => _accReadingZ;
            set
            {
                if (_accReadingZ != value)
                {
                    _accReadingZ = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool SetToMiles
        {
            get => _setToMiles;
            set
            {
                if (_setToMiles != value)
                {
                    _setToMiles = value;
                    OnPropertyChanged();
                }
            }
        }

        public void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public BCompassViewModel()
        {
            if (!Compass.IsMonitoring)
                Compass.Start(SensorSpeed.Game);
            /*if (!Accelerometer.IsMonitoring)
                Accelerometer.Start(SensorSpeed.UI);*/

            Compass.ReadingChanged += Compass_ReadingChanged;
            //Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;

            double dist = Location.CalculateDistance(currentLocation, blockBusterLoc, DistanceUnits.Miles);
            this.SetMessage(dist);
        }

        private void Compass_ReadingChanged(object sender, CompassChangedEventArgs e)
        {
            var data = e.Reading;
            this.CompassRotation = CalculateDirectionToBB(data.HeadingMagneticNorth);
            UpdateLocation(data.HeadingMagneticNorth);
        }
        void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
        {
            var data = e.Reading;
            this.AccReadingX = ($"X   {data.Acceleration.X}");
            this.AccReadingY = ($"Y   {data.Acceleration.Y}");
            this.AccReadingZ = ($"Z   {data.Acceleration.Z}");
        }
        private async void UpdateLocation(double hMagNorth)
        {
            try
            {
                var location = await Geolocation.GetLocationAsync();

                if (location != null)
                {
                    currentLocation = new Location(location.Latitude, location.Longitude);
                    double dist = Location.CalculateDistance(currentLocation, blockBusterLoc, DistanceUnits.Miles);

                    this.distanceFromBB = dist;
                    if (this.SetToMiles)
                    {
                        this.DistanceFromBBText = String.Format("{0:0.##} Miles from", dist);
                    }
                    else
                    {
                        this.DistanceFromBBText = String.Format("{0:0.##} KM from", dist * 1.609344);
                    }
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                Console.Write("Unable to get Location - Feature not supported: " + fnsEx);
            }
            catch (FeatureNotEnabledException fneEx)
            {
                Console.Write("Unable to get Location - Feature not available: " + fneEx);
            }
            catch (PermissionException pEx)
            {
                Console.Write("Unable to get Location - Permission Error: " + pEx);
            }
            catch (Exception ex)
            {
                Console.Write("Unable to get Location: " + ex);
            }
        }

        private double CalculateDirectionToBB(double hMagNorth)
        {
            Vector2 bbPath = Vector2.Normalize(new Vector2((float)(blockBusterLoc.Longitude - currentLocation.Longitude), (float)(blockBusterLoc.Latitude - currentLocation.Latitude)));

            double lat1 = currentLocation.Latitude * Math.PI / 180;
            double lon1 = currentLocation.Longitude * Math.PI / 180;
            double lat2 = blockBusterLoc.Latitude * Math.PI / 180;
            double lon2 = blockBusterLoc.Longitude * Math.PI / 180;

            double deltaLon = lon2 - lon1;
            double x = Math.Cos(lat2) * Math.Sin(deltaLon);
            double y = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(deltaLon);
            double theta = Math.Atan2(x, y);
            while (theta < 0)
                theta += 2 * Math.PI;

            double hMagBB = theta * 180 / Math.PI;

            if (hMagNorth > 360)
                hMagNorth -= 360;

            double bbDirection = hMagBB - hMagNorth;

            return bbDirection;
        }

        public List<string> BuildMessagesFromDistance((string, double, double)[] messages, double distance)
        {
            List<string> returnList = new List<string>();
            Console.WriteLine(messages);
            foreach((string, double, double) row in messages)
            {
                if ((row.Item2 == 0 && row.Item3 == 0) ||
                    (row.Item2 <= distanceFromBB && row.Item3 >= distanceFromBB))
                {
                    returnList.Add(row.Item1);
                }
            }
            return returnList;
        }

        public void SetMessage(double dist)
        {
            var data = new (string, double, double)[] {
                ("Blockbuster!!!", 0, 0)
            };
            List<string> messages = this.BuildMessagesFromDistance(data, dist);
            Random rnd = new Random();
            this.DisplayMessage = messages[rnd.Next(messages.Count)];
        }
    }
}
