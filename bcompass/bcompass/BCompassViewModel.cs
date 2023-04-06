using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using Xamarin.Essentials;
using CsvHelper;

namespace bcompass
{
    public class BCompassViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        static Location blockBusterLoc = new Location(44.067365, -121.303486);

        Location currentLocation = new Location(0, 0);
        private string _distanceFromBBText = "";
        private double _compassRotation = 0;

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

        public void printHello()
        {
            Console.WriteLine("hello");
        }

        public Message[] ReadMessagesFromFile()
        {
            using (var reader = new StreamReader("m.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<DistanceMessage>();
                Console.Write(records);
            }
            return null;
        }
         

        public void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public BCompassViewModel()
        {
            var records = this.ReadMessagesFromFile();
            this.printHello();

            if (!Compass.IsMonitoring)
                Compass.Start(SensorSpeed.UI);
            Compass.ReadingChanged += Compass_ReadingChanged;
        }

        private void Compass_ReadingChanged(object sender, CompassChangedEventArgs e)
        {
            var data = e.Reading;
            UpdateLocation(data.HeadingMagneticNorth);

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

                    this.DistanceFromBBText = String.Format("{0:0.##} Miles from", dist);
                    this.CompassRotation = CalculateDirectionToBB(hMagNorth);
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

            hMagNorth = hMagNorth + 180;
            if (hMagNorth > 360)
                hMagNorth -= 360;

            double bbDirection = hMagBB - hMagNorth;

            Debug.Write(hMagBB - hMagNorth);
            return hMagBB;
        }
    }

    public abstract class Message
    {
        public string text;
        public void print()
        {
            Console.WriteLine(text);
        }
    }

    public class DistanceMessage : Message
    {
        public double maxDist;
    }
}
