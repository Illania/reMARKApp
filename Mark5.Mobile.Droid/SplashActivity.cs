//
// Project: Mark5.Mobile.Droid
// File: SplashActivity.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Mark5.Mobile.Common.Authenticator;
using Mark5.Mobile.Droid.Views.Login;
using Xamarin;
using Mark5.Mobile.Droid.Views.Main;
using Android.Support.V7.App;

namespace Mark5.Mobile.Droid
{

    [Activity(Label = "MARK5",
              MainLauncher = true,
              Icon = "@mipmap/icon",
              NoHistory = true,
              ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
              ScreenOrientation = ScreenOrientation.Portrait)]
    public class SplashActivity : AppCompatActivity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Splash);

            Insights.Track($"Creating Splash activity...");

            Task.Run(async () =>
            {
                if (await AuthenticatorFactory.Create().IsAuthenticatedAsync())
                {
                    StartActivity(new Intent(this, typeof(MainActivity)));
                }
                else
                {
                    StartActivity(new Intent(this, typeof(LoginActivity)));
                }
            });
        }
    }
}

