using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using Xamarin.Essentials;

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
         
        public void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public BCompassViewModel()
        {
            this.PrintMessages();


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

        public string[] BuildMessagesFromDistance((string, double)[] messages, double distance)
        {
            var returnArray = new string[] { };
            foreach((string, double) row in messages)
            {
                Console.WriteLine(row.Item1);
            }

            return returnArray;
            
        }

        public void PrintMessages()
        {
            var data = new (string, double)[] {
                ("Blockbuster!!!", 0),
                ("Blockbuster is waiting for you!", 0),
                ("A trip to Blockbuster is fun for the whole family!", 0),
                ("Have you ever heard of this cool place called Blockbuster?", 0),
                ("Fun fact: you can get movies at blockbuster!", 0),
                ("Overdue tapes at your place? Better make it to your nearest Blockbuster!", 0),
                ("Make it a blockbuster hike!", 0),
                ("You should go to Blockbuster.", 0),
                ("Kellyanne loved going to Blockbuster. I miss her so much.", 0),
                ("Don't forget to return your tapes!", 0),
                ("Looking for the hottest new tapes and DVDs? You can find them at your local Blockbuster!", 0),
                ("Not sure what you’d like to get from Blockbuster yet? We have a call-gorithm!", 0),
                ("Don’t forget: Blockbuster gets new releases 30 days before Netflix!", 0),
                ("Have you thought about going to Blockbuster today?", 0),
                ("Who needs slow streaming? With Blockbuster, you can get real DVDs, real fast!", 0),
                ("Get some tapes and keep em’ for a week!", 0),
                ("When the world ends, and the internet streams no more, we’ll still be here.", 0),
                ("Looking for a video? Blockbuster has it! We have over ten thousand videos!", 0),
                ("Wow! What a Difference!", 0.0568182f),
                ("You made it to Blockbuster!", 0.0568182f),
                ("You’re at your nearest Blockbuster!", 0.0568182f),
                ("Wow! It’s Blockbuster!", 0.0568182f),
                ("You’re at Blockbuster! Wow, what a difference!", 0.0568182f),
                ("Wow, you're almost at Blockbuster!", 10f),
                ("You’re so close to Blockbuster! Your blockbuster night could be right around the corner!", 10f),
                ("You’re really close to Blockbuster! I can smell the popcorn already!", 10f),
                ("Blockbuster is right around the corner! Have you thought about what you’d like to watch?", 10f),
                ("You’re so close to Blockbuster! What a difference!", 10f),
                ("Nearly there! Be a friend and rewind once you reach the end!", 10f),
                ("There’s a Blockbuster near you!", 10f),
                ("You better get moving, you're quite a ways away from Blockbuster!", 3000f),
                ("You’re so close, and yet so far from your nearest Blockbuster! Make sure your tapes aren’t overdue!", 3000f),
                ("Feeling a little far from Blockbuster? Go a little crazy and consider swinging by!", 3000f),
                ("Head on over to Blockbuster video, and you’ll see just what a difference!", 3000f)
            };
            this.BuildMessagesFromDistance(data, 20.0);
        }
    }
}
