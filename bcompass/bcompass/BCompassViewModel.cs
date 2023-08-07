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
using Xamarin.Forms;

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

        private FormattedString _distanceFromBBText;
        private string _totalWalkedText;
        private string _disclaimerText;
        private string _messageText;
        private double _messageFontSize;
        private double _compassRotation;
        private bool _setToMiles = true;

        List<Message> messages = new List<Message> {
                new Message("There’s a Blockbuster near you!", 0, 0, 0, -1, ""),
                new Message("Make it a Blockbuster Hike!", 0, 0, 0, -1, ""),
                new Message("Go to Blockbuster!", 0, 0, 0, -1, ""),
                new Message("You should go to Blockbuster!", 0, 0, 0, -1, ""),
                new Message("Wow, what a difference! Blockbuster Video!", 0, 0, 0, -1, ""),
                new Message("Make it a Blockbuster Night!", 0, 0, 0, -1, ""),
                new Message("Blockbuster!!!", 0, 0, 0, -1, ""),
                new Message("Who loves blockbuster? You do!", 0, 0, 0, -1, ""),
                new Message("You could always be closer to Blockbuster.",0,0,0,-1,""),
                new Message("Have you thought about Blockbuster today?",0,0,0,-1,""),
                new Message("Are you ready for some Blockbuster?",0,0,0,-1,""),
                new Message("The future is now with Blockbuster Video!",0,0,0,-1,""),
                new Message("What are movies? find out at Blockbuster!",0,0,0,-1,""),
                new Message("Movies? Who needs 'em? You do! From Blockbuster!",0,0,0,-1,""),
                new Message("Looking for a video? Blockbuster has those!",0,0,0,-1,""),
                new Message("Looking for a video? Blockbuster has it!",0,0,0,-1,""),
                new Message("Go to Blockbuster now!",0,0,0,-1,""),
                new Message("Hey! Blockbuster!",0,0,0,-1,""),
                new Message("Blockbuster? Blockbuster!",0,0,0,-1,""),
                new Message("Did you know: Blockbuster!!!",0,0,0,-1,""),
                new Message("We don't track your location, but if you go to Hollywood Video, we'll know.",0,0,0,-1,""),
                new Message("I have to return some video tapes.",0,0,0,-1,""),
                new Message("You MUST return your video tapes.",0,0,0,-1,""),
                new Message("Fun Fact: You can get Movies at your local Blockbuster!",0,0,0,-1,""),
                new Message("Remember that commercial where the tapes all come to life and hang out at night? That doesn't really happen.",0,0,0,-1,""),
                new Message("Blockbuster Blockbuster Blockbuster Blockbuster Blockbuster Blockbuster Blockbuster",0,0,0,-1,""),
                new Message("Go to Blockbuster.",0,0,0,-1,""),
                new Message("You're {val} Protozoas away! What a Fantastic Voyage!",0,0,0,0.0000000310686,""),
                new Message("You're {val} Clydesdales away from Blockbuster! Saddle up!",0,0,0,0.001136364,""),
                new Message("Ello Gov'na! You're the length of {val} Englands away from Blockbuster!",0,0,0,600,""),
                new Message("You're only {val} Soviet Unions (1962) away, comrade!",0,0,0,6800,""),
                new Message("Gong Shi! You're {val} Great Walls of China away from your movie night!",0,0,0,13171,""),
                new Message("Heng ha! You're just {val} Singapores away from Blockbuster!",0,0,0,31,""),
                new Message("Holy Moly! You're the length of {val} Vatican Cities away from Blockbuster!",0,0,0,0.6,""),
                new Message("You're {val} Schoolbusses away from Blockbuster! This won't be a normal feild trip!",0,0,0,0.00606061,""),
                new Message("Watch out for snakes! You're the length of {val} average corn snakes away from Blockbuster!",0,0,0,0.000757576,""),
                new Message("Beep Beep! You're only {val} car-lengths away from Blockbuster!",0,0,0,0.002784091,""),
                new Message("There's no rule that says a dog can't go to Blockbuster! you're the length of {val} adult Golden Retrievers away!",0,0,0,0.000662879,""),
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
        public FormattedString DistanceFromBBText
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

        public double MessageFontSize
        {
            get => _messageFontSize;
            set
            {
                if (_messageFontSize != value)
                {
                    _messageFontSize = value;
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
                    TotalWalkedText = String.Format("{0:f2} MI TRAVELLED", travelled);
                }
                else
                {
                    TotalWalkedText = String.Format("{0:f2} KM TRAVELLED", travelled * KMCONSTANT);
                }
            }
            else
            {
                if (SetToMiles)
                {
                    TotalWalkedText = String.Format("{0:f2} MI TRAVELLED", 0);
                }
                else
                {
                    TotalWalkedText = String.Format("{0:f2} KM TRAVELLED", 0);
                }
            }
            double dist = Location.CalculateDistance(currentLocation, BBLOCATION, DistanceUnits.Miles);
            StartLocationUpdate();
            StartMessageUpdate();
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
            await PeriodicLocationUpdate(TimeSpan.FromSeconds(8));
        }

        public async void StartMessageUpdate()
        {
            await PeriodicMessageUpdate(TimeSpan.FromSeconds(9));
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

        CheckAvailability:
            for (int i = 0; i < messages.Count-1; i++)
            {
                Message m = messages[i];
                if (!m.Used &&  (m.MinDisplayDistance == 0 && m.MaxDisplayDistance == 0) || (m.MinDisplayDistance <= distanceFromBB && m.MaxDisplayDistance >= distanceFromBB))
                {
                    availableIndexes.Add(i);
                }
            }

            if (availableIndexes.Count <= 1)
            {
                for (int i = 0; i < messages.Count - 1; i++)
                {
                    messages[i].Used = false;
                }
                goto CheckAvailability;
            }

            int index = rnd.Next(availableIndexes.Count);

            // Catch case where Distance-based message is picked before first distance reading is finished
            if (distanceFromBB == 0 && messages[index].Text.Contains("{val}"))
            {
                goto CheckAvailability;
            }
            
            double unitDistances = (distanceFromBB / messages[index].UnitMeasurement);

            if (unitDistances < 1)
            {
                Message = messages[index].Text.Replace("{val}", String.Format("{0:g7}", unitDistances));
            } else if (unitDistances < 1000)
            {
                Message = messages[index].Text.Replace("{val}", String.Format("{0:n6}", unitDistances));
            } else
            {
                Message = messages[index].Text.Replace("{val}", String.Format("{0:n0}", unitDistances));
            }
            Disclaimer = messages[index].Disclaimer;
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
                            TotalWalkedText = String.Format("{0:f2} MI TRAVELLED", totalDistanceWalked);
                        }
                        else
                        {
                            TotalWalkedText = String.Format("{0:f2} KM TRAVELLED", totalDistanceWalked * KMCONSTANT);
                        }
                        File.WriteAllText(saveData, totalDistanceWalked.ToString());
                    }

                    distanceFromBB = dist;
                    if (SetToMiles)
                    {
                        DistanceFromBBText = String.Format("{0:f2} mi from", dist);
                    }
                    else
                    {
                        DistanceFromBBText = String.Format("{0:f2} km from", dist * KMCONSTANT);
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
