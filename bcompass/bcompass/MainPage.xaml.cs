﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace bcompass
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            BCompassViewModel bcvm = new BCompassViewModel();
            BindingContext = this;
            InitializeComponent();
        }

        void Options_Clicked(System.Object sender, System.EventArgs e)
        {
        }
    }
}
