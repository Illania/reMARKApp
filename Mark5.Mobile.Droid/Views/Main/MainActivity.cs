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

namespace Mark5.Mobile.Droid.Views.Main
{

    [Activity]
    public class MainActivity : AppCompatActivity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetTitle(Resource.String.app_name);
            SetContentView(Resource.Layout.Main);
        }
    }
}

