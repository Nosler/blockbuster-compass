using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using blockbustercompass.Services;
using blockbustercompass.Views;

namespace blockbustercompass
{
    public partial class App : Application
    {

        public App ()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            MainPage = new AppShell();
        }

        protected override void OnStart ()
        {
        }

        protected override void OnSleep ()
        {
        }

        protected override void OnResume ()
        {
        }
    }
}

