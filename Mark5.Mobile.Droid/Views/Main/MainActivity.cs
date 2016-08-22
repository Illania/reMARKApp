//
// Project: Mark5.Mobile.Droid
// File: MainActivity.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Xamarin;

namespace Mark5.Mobile.Droid.Views.Main
{

    [Activity(Label = "MARK5")]
    public class MainActivity : AppCompatActivity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Main);

            Insights.Track($"[{nameof(MainActivity.OnCreate)}]");
        }
    }
}

