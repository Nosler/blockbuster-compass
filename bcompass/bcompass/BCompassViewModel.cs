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
        private string _distanceFromBBText = "";
        private double _compassRotation = 0;
        private string _displayMessage = "";
        private string _accReadingX = "";
        private string _accReadingY = "";
        private string _accReadingZ = "";

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

        public void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public BCompassViewModel()
        {
            if (!Compass.IsMonitoring)
                Compass.Start(SensorSpeed.Game);
            if (!Accelerometer.IsMonitoring)
                Accelerometer.Start(SensorSpeed.UI);

            Compass.ReadingChanged += Compass_ReadingChanged;
            Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;

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
                    this.DistanceFromBBText = String.Format("{0:0.##} Miles from", dist);
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

            // hMagNorth = hMagNorth;
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
                if (true)
                {
                    returnList.Add(row.Item1);
                }
            }
            return returnList;
        }

        public void SetMessage(double dist)
        {
            var data = new (string, double, double)[] {
                ("Blockbuster!!!", 0, 0),
                ("Blockbuster is waiting for you!", 0, 0),
                ("A trip to Blockbuster is fun for the whole family!", 0, 0),
                ("Have you heard of this cool place called Blockbuster?", 0, 0),
                ("Fun Fact: You can get Movies at your local Blockbuster!", 0, 0),
                ("Overdue tapes at your place? Better make it to your nearest Blockbuster!", 0, 0),
                ("Make it a Blockbuster Hike!", 0, 0),
                ("Go to Blockbuster!", 0, 0),
                ("Go to Blockbuster.", 0, 0),
                ("You should go to Blockbuster!", 0, 0),
                ("You should go to Blockbuster.", 0, 0),
                ("Kellyanne loved going to Blockbuster. I miss her so much.", 0, 0),
                ("Don't forget to return some videotapes!", 0, 0),
                ("Don't forget to return your tapes!", 0, 0),
                ("Looking for the hottest new tapes and DVDs? We have them at Blockbuster!", 0, 0),
                ("Not sure what to get from Blockbuster? We have a Call-gorithm!", 0, 0),
                ("Blockbuster gets new releases 30 days before Netflix!", 0, 0),
                ("Have you thought about Blockbuster today?", 0, 0),
                ("Hey! Blockbuster!", 0, 0),
                ("Did you know: Blockbuster!!!", 0, 0),
                ("Blockbuster? Blockbuster!", 0, 0),
                ("Are you ready for some Blockbuster?", 0, 0),
                ("Come to Blockbuster and you'll discover what a difference it makes!", 0, 0),
                ("Wow, what a difference! Blockbuster Video!", 0, 0),
                ("I have to return some video tapes.", 0, 0),
                ("You can find the widest selection of movies around at Blockbuster!", 0, 0),
                ("Who loves blockbuster? You do!", 0, 0),
                ("We don't track your location, but if you go to Hollywood Video, we can't make any promises.", 0, 0),
                ("B-L-O-C-K-B-U-S-T-E-R! What's that spell? Read it!", 0, 0),
                ("The future is now with Blockbuster Video!", 0, 0),
                ("Don't Bust Your Block, Block Those Busts! (note - figure out better phrase)", 0, 0),
                ("Be sure to go to Blockbuster, because you can't watch movies on your phone!", 0, 0),
                ("Blockbuster is here to stay!", 0, 0),
                ("With so many locations, it can be hard to find your closest store! Luckily, Blockbuster Compass is here for you!", 0, 0),
                ("Blockbuster is here to stay!", 0, 0),
                ("Guess what we're thinking of! (Here's a hint: It's Blockbuster!)", 0, 0),
                ("Have you been to your nearest Blockbuster lately?", 0, 0),
                ("What are movies? find out at Blockbuster!", 0, 0),
                ("Movies? Who needs 'em? You do! From Blockbuster!", 0, 0),
                ("Remember that commercial where the tapes all come to life and hang out at night? That doesn't really happen.", 0, 0),
                ("BustBlockter", 0, 0),
                ("Who needs slow streaming? With Blockbuster, you can get real DVDs, real fast!", 0, 0),
                ("Get some tapes and keep 'em for a week!", 0, 0),
                ("When the world ends and the internet streams no more, we'll still be here.", 0, 0),
                ("Looking for a video? Blockbuster has those!", 0, 0),
                ("We have over ten thousand videos!", 0, 0),
                ("Looking for a video? Blockbuster has it!", 0, 0),
                ("Video stores aren't going anywhere, so be sure you know where yours are!", 0, 0),
                ("Looking for a video? Blockbuster has it! We have over ten thousand videos!", 0, 0),
                ("Feeling a little far from Blockbuster? Go a little crazy and consider swinging by!", 3000, 9999),
                ("Head on over to Blockbuster video, and you’ll see just what a difference!", 3000, 9999)
            };
            List<string> messages = this.BuildMessagesFromDistance(data, dist);
            Random rnd = new Random();
            this.DisplayMessage = messages[rnd.Next(messages.Count)];
        }
    }
}
