//
// Project: Mark5.Mobile.Droid
// File: SplashActivity.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Mark5.Mobile.Common.Authenticator;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Xamarin;
using UK.CO.Chrisjenx.Calligraphy;

namespace Mark5.Mobile.Droid.Views.Activity
{

    [Activity(Label = "MARK5",
              MainLauncher = true,
              Icon = "@mipmap/ic_icon",
              Theme = "@style/mark5Splash",
              ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
              ScreenOrientation = ScreenOrientation.Portrait)]
    public class SplashActivity : AppCompatActivity
    {

        protected override void AttachBaseContext(Context @base)
        {
            base.AttachBaseContext(CalligraphyContextWrapper.Wrap(@base));
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_splash);

            var uiOptions = (int)Window.DecorView.SystemUiVisibility;
            uiOptions |= (int)SystemUiFlags.Immersive;
            uiOptions |= (int)SystemUiFlags.HideNavigation;
            Window.DecorView.SystemUiVisibility = (StatusBarVisibility)uiOptions;
        }

        protected override void OnResume()
        {
            base.OnResume();

            Task.Run(async () =>
            {
                var authenticator = AuthenticatorFactory.Create();
                if (await authenticator.IsAuthenticatedAsync())
                {
                    var ci = await authenticator.GetConnectionInfoAsync();

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

                    var ss = await Managers.SystemManager.GetSystemSettingsAsync(SourceType.Local);

                    Insights.Identify($"{ci.Username}@{ci.SslMode},{ci.Hostname}:{ci.Port}", new Dictionary<string, string>
                    {
                        [Insights.Traits.FirstName] = ss?.UserInfo?.User?.FirstName,
                        [Insights.Traits.LastName] = ss?.UserInfo?.User?.LastName,
                        ["System Administrator"] = (ss?.UserInfo?.IsSystemAdministrator ?? false) ? "Yes" : "No"
                    });

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

