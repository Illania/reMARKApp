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
using Android.Support.V7.App;
using Mark5.Mobile.Common.Authenticator;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Views.Login;
using Mark5.Mobile.Droid.Views.Main;
using Xamarin;

namespace Mark5.Mobile.Droid.Views.Splash
{

    [Activity(Label = "MARK5",
              MainLauncher = true,
              Icon = "@mipmap/icon",
              ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
              ScreenOrientation = ScreenOrientation.Portrait)]
    public class SplashActivity : AppCompatActivity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Splash);

            Insights.Track($"Creating Splash activity...");
        }

        protected override void OnResume()
        {
            base.OnResume();

            Task.Run(async () =>
            {
                var auth = AuthenticatorFactory.Create();
                if (await auth.IsAuthenticatedAsync())
                {
                    var ci = await auth.GetConnectionInfoAsync();

                    switch (ci.SslMode)
                    {
                        case SslMode.AllowSelfSigned:
                            PlatformConfig.SSLCertificateVerificationManager.EnableSelfSignedCertificates();
                            break;
                        default:
                            PlatformConfig.SSLCertificateVerificationManager.DisableSelfSignedCertificates();
                            break;
                    }

                    Managers.Initialize(ci);
                    PlatformConfig.ReachabilityBroadcastReceiver.Register();

                    StartActivity(new Intent(this, typeof(MainActivity)));
                }
                else
                {
                    StartActivity(new Intent(this, typeof(LoginActivity)));
                }

                Finish();
            });
        }
    }
}

