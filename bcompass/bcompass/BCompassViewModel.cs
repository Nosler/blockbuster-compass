using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;
using System.Diagnostics;

namespace bcompass
{
    public class BCompassViewModel : INotifyPropertyChanged
    {
        static Location blockbuster = new Location(44.067365, -121.303486);

        Location currentLocation = new Location(0, 0);
        double distanceFromBBm = 0;
        double distanceFromBBk = 0;
        double direcitonToBB = 0;

        public BCompassViewModel()
        {
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
                    distanceFromBBm = Location.CalculateDistance(currentLocation, blockbuster, DistanceUnits.Miles);
                    distanceFromBBk = Location.CalculateDistance(currentLocation, blockbuster, DistanceUnits.Kilometers);
                    direcitonToBB = CalculateDirectionToBB(hMagNorth);
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
            return 0;
        }

        // -------- THE UI UPDATING ZONE --------

        private void updateCurrentLocation()
        {
            Debug.Write(currentLocation);
        }

        private void updateDistanceFromBBk()
        {
            Debug.Write(distanceFromBBk);
        }

        private void updateDistanceFromBBm()
        {
            Debug.Write(distanceFromBBm);
        }

        private void updateDirectionToBB()
        {
            Debug.Write(direcitonToBB);
        }
        // ----------------------------------------

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
