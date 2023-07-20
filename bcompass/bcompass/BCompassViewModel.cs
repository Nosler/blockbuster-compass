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
using System.Net.NetworkInformation;
using System.Security.Cryptography;

namespace bcompass
{
    public class Message
    {
        public string text { get; }
        public double rarity { get; }
        public double minDisplayDistance { get; }
        public double maxDisplayDistance { get; }
        public double unitMeasurement { get; }
        public string disclaimer { get; }
        public bool used { get; set; }
        public Message(string tex, double rar, double minDistance, double maxDistance, double measurement, string disc)
        {
            this.text = tex;
            this.rarity = rar;
            this.minDisplayDistance = minDistance;
            this.maxDisplayDistance = maxDistance;
            this.disclaimer = disc;
            this.unitMeasurement = measurement;
            this.used = false;
        }
    }

    public class BCompassViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        static Location blockBusterLoc = new Location(44.067365, -121.303486);

        private Location currentLocation = new Location(0, 0);
        private float zReading;

        DateTime nextMessageTime = DateTime.MinValue;
        private double distanceFromBB;

        private string _distanceFromBBText;
        private double _compassRotation;
        private string _displayMessage;
        private string _displayDisclaimer;
        private bool _setToMiles;

        private string _accReadingX;
        private string _accReadingY;
        private string _accReadingZ;

        List<Message> messages = new List<Message> { };

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
        public string DisplayDisclaimer
        {
            get => _displayDisclaimer;
            set
            {
                if (_displayDisclaimer != value)
                {
                    _displayDisclaimer = value;
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
            {
                Compass.Start(SensorSpeed.Game); 
            }

            if (!Accelerometer.IsMonitoring)
            {
                Accelerometer.Start(SensorSpeed.UI);
            }

            Compass.ReadingChanged += Compass_ReadingChanged;
            Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;

            this.messages = this.BuildMessagesList("beans");
            double dist = Location.CalculateDistance(currentLocation, blockBusterLoc, DistanceUnits.Miles);
        }

        private void Compass_ReadingChanged(object sender, CompassChangedEventArgs e)
        {
            var data = e.Reading;
            if (this.zReading < 0)
            {
                this.CompassRotation = CalculateDirectionToBB(data.HeadingMagneticNorth) + 180.0;
            }
            else
            {
                this.CompassRotation = CalculateDirectionToBB(data.HeadingMagneticNorth);
            }
            UpdateLocation(data.HeadingMagneticNorth);
        }
        void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
        {
            var data = e.Reading;
            //this.AccReadingX = ($"X   {data.Acceleration.X}");
            //this.AccReadingY = ($"Y   {data.Acceleration.Y}");
            //this.AccReadingZ = ($"Z   {data.Acceleration.Z}");
            this.zReading = data.Acceleration.Z;
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

                    if (DateTime.Now < this.nextMessageTime)
                    {
                        this.UpdateMessageDisplay(dist);
                        this.nextMessageTime = DateTime.Now.AddSeconds(10);
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

        public List<Message> BuildMessagesList(string filename)
        {
            return new List<Message> { new Message("Test", 0, 0, 0, -1, "") };
        }

        public void UpdateMessageDisplay(double dist)
        {
            List<int> availableIndexes = new List<int>();


            CheckAvailability:
                for (int i = 0; i < this.messages.Count; i++)
                {
                    Message m = this.messages[i];
                    if (!m.used && (m.minDisplayDistance <= dist && m.maxDisplayDistance >= dist))
                    {
                        availableIndexes.Add(i);
                    }
                }

            if (availableIndexes.Count < 1)
            {
                foreach (Message m in this.messages) 
                {
                    m.used = false;
                }
                goto CheckAvailability;
            }

            Random rnd = new Random();
            int index = rnd.Next(availableIndexes.Count);
            Message msg = this.messages[index];
            this.messages[index].used = true;
            
            this.DisplayMessage = msg.text.Replace("{val}", (dist * msg.unitMeasurement).ToString());
            this.DisplayDisclaimer = msg.disclaimer;
        }
    }
}
