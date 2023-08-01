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
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace bcompass
{
    public class Message
    {
        public string Text { get; }
        public double Rarity { get; }
        public double MinDisplayDistance { get; }
        public double MaxDisplayDistance { get; }
        public double UnitMeasurement { get; }
        public string Disclaimer { get; }
        public bool Used { get; set; }
        public Message(string tex, double rar, double minDistance, double maxDistance, double measurement, string disc)
        {
            Text = tex;
            Rarity = rar;
            MinDisplayDistance = minDistance;
            MaxDisplayDistance = maxDistance;
            Disclaimer = disc;
            UnitMeasurement = measurement;
            Used = false;
        }
    }

    public class BCompassViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        static readonly Location BBLOCATION = new Location(44.067365, -121.303486);
        static readonly double KMCONSTANT = 1.609344;
        static string saveData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "savedata.txt");

        private Location currentLocation = new Location(0, 0);
        private double zReading;
        private double totalDistanceWalked;
        private double distanceFromBB;
        private int counter;

        private double _compassRotation;
        private string _distanceFromBBText;
        private string _totalWalkedText;
        private string _disclaimerText;
        private string _messageText;
        private bool _setToMiles;

        private string _accReadingX;
        private string _accReadingY;
        private string _accReadingZ;

        List<Message> messages = new List<Message> {
                new Message("Test1", 0, 0, 0, -1, ""),
                new Message("Test2", 0, 0, 0, -1, "1"),
                new Message("Test3", 0, 0, 0, -1, "2"),
                new Message("Test4", 0, 0, 0, -1, "3"),
                new Message("Test5", 0, 0, 0, -1, "4"),
                new Message("Test6", 0, 0, 0, -1, "5"),
                new Message("Test7", 0, 0, 0, -1, "6"),
                new Message("Test8", 0, 0, 0, -1, "7"),
                new Message("Test9", 0, 0, 0, -1, "8"),
        };

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
        public string TotalWalkedText
        {
            get => _totalWalkedText;
            set
            {
                if (_totalWalkedText != value)
                {
                    _totalWalkedText = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Message
        {
            get => _messageText;
            set
            {
                if (_messageText != value)
                {
                    _messageText = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Disclaimer
        {
            get => _disclaimerText;
            set
            {
                if (_disclaimerText != value)
                {
                    _disclaimerText = value;
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
            Compass.ReadingChanged += Compass_ReadingChanged;
            Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;

            if (File.Exists(saveData))
            {
                double travelled =  Double.Parse(File.ReadAllText(saveData));
                totalDistanceWalked = travelled;
                if (SetToMiles)
                {
                    TotalWalkedText = String.Format("{0:0.#} m Travelled to Blockbuster", travelled);
                }
                else
                {
                    TotalWalkedText = String.Format("{0:0.#} km Travelled to Blockbuster", travelled * KMCONSTANT);
                }
            }
            else
            {
                if (SetToMiles)
                {
                    TotalWalkedText = String.Format("{0:0.#} m Travelled to Blockbuster", 1110);
                }
                else
                {
                    TotalWalkedText = String.Format("{0:0.#} km Travelled to Blockbuster", 0111);
                }
            }
            double dist = Location.CalculateDistance(currentLocation, BBLOCATION, DistanceUnits.Miles);
            StartMessageUpdate();
            StartLocationUpdate();
        }
        
        private void Compass_ReadingChanged(object sender, CompassChangedEventArgs e)
        { 
            var data = e.Reading;
            if (zReading < 0)
            {
                CompassRotation = GetDirectionToBB(data.HeadingMagneticNorth) + 180.0;
            }
            else
            {
                CompassRotation = GetDirectionToBB(data.HeadingMagneticNorth);
            }
        }
        void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
        {
            var data = e.Reading;
            zReading = data.Acceleration.Z;
        }

        private double GetDirectionToBB(double hMagNorth)
        {
            double lat1 = currentLocation.Latitude * Math.PI / 180;
            double lon1 = currentLocation.Longitude * Math.PI / 180;
            double lat2 = BBLOCATION.Latitude * Math.PI / 180;
            double lon2 = BBLOCATION.Longitude * Math.PI / 180;

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


        public async void StartLocationUpdate()
        {
            await PeriodicLocationUpdate(TimeSpan.FromSeconds(9));
        }

        public async void StartMessageUpdate()
        {
            await PeriodicMessageUpdate(TimeSpan.FromSeconds(15));
        }


        public async Task PeriodicLocationUpdate(TimeSpan interval)
        {
            while (true)
            {
                await UpdateLocation();
                await Task.Delay(interval);
            }
        }
        public async Task PeriodicMessageUpdate(TimeSpan interval)
        {
            while (true)
            {
                await UpdateMessageDisplay();
                await Task.Delay(interval);
            }
        }

        public async Task UpdateMessageDisplay()
        {
            Random rnd = new Random();
            List<int> availableIndexes = new List<int>();
            double rarityThreashold = rnd.NextDouble();

        CheckAvailability:
            for (int i = 0; i < messages.Count-1; i++)
            {
                Message m = messages[i];
                if (!m.Used &&  (m.MinDisplayDistance == 0 && m.MaxDisplayDistance == 0) || (m.MinDisplayDistance <= distanceFromBB && m.MaxDisplayDistance >= distanceFromBB))
                {
                    if(m.Rarity > rarityThreashold)
                    {
                        availableIndexes.Add(i);
                    }
                }
            }

            if (availableIndexes.Count < 1)
            {
                if (rarityThreashold > 0)
                {
                    rarityThreashold -= .2;
                } 
                else
                {
                    foreach (Message m in messages)
                    {
                        m.Used = false;
                    }
                }
                goto CheckAvailability;
            }

            int index = rnd.Next(availableIndexes.Count);
            Message msg = messages[index];

            Message = msg.Text.Replace("{val}", (distanceFromBB * msg.UnitMeasurement).ToString());
            Disclaimer = msg.Disclaimer;
            messages[index].Used = true;
        }

        public async Task UpdateLocation()
        {
            try
            {
                var location = await Geolocation.GetLocationAsync();

                if (location != null)
                {
                    currentLocation = new Location(location.Latitude, location.Longitude);
                    double dist = Location.CalculateDistance(currentLocation, BBLOCATION, DistanceUnits.Miles);

                    if (dist < distanceFromBB)
                    {
                        totalDistanceWalked += (distanceFromBB - dist);
                        if (SetToMiles)
                        {
                            TotalWalkedText = String.Format("{0:0.#} m Travelled to Blockbuster", totalDistanceWalked);
                        }
                        else
                        {
                            TotalWalkedText = String.Format("{0:0.#} km Travelled to Blockbuster", totalDistanceWalked * KMCONSTANT);
                        }
                        File.WriteAllText(saveData, totalDistanceWalked.ToString());
                    }

                    distanceFromBB = dist;
                    if (SetToMiles)
                    {
                        DistanceFromBBText = String.Format("{0:0.##} m from", dist);
                    }
                    else
                    {
                        DistanceFromBBText = String.Format("{0:0.##} km from", dist * KMCONSTANT);
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
    }
}
