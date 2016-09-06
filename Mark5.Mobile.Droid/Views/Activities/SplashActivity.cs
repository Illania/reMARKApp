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
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Authenticator;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Views.Common;

namespace Mark5.Mobile.Droid.Views.Activity
{

    [Activity(Label = "MARK5",
              MainLauncher = true,
              Icon = "@mipmap/ic_icon",
              Theme = "@style/mark5Splash",
              ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
              ScreenOrientation = ScreenOrientation.Portrait)]
    public class SplashActivity : BaseAppCompatActivity
    {

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
                    Managers.DocumentsManager.MaxToFetch = PlatformConfig.Preferences.DocumentsToDownload;
                    Managers.DocumentsManager.DocumentBodyTypeRequest = PlatformConfig.Preferences.DocumentBodyRequestType;
                    var policies = Managers.DownloadManager.DownloadPolicies;
                    policies[ObjectType.Document] = new DownloadFoldersPolicy();
                    if (PlatformConfig.Preferences.SynchroniseContacts)
                    {
                        policies[ObjectType.Contact] = new DownloadAllPolicy();
                    }
                    if (PlatformConfig.Preferences.SynchroniseShortcodes)
                    {
                        policies[ObjectType.Shortcode] = new DownloadAllPolicy();
                    }

                    if (await Managers.CleanUpManager.IsCleanUpNecessary(PlatformConfig.Preferences.CleanCacheIntervalDays))
                    {
                        await Managers.CleanUpManager.CleanUp();
                    }

                    await Managers.DownloadManager.Start();
                    await Managers.OutgoingDocumentsManager.Start();
                    PlatformConfig.ReachabilityBroadcastReceiver.Register();

                    RunOnUiThreadIfNecessary(() => StartActivity(new Intent(this, typeof(MainActivity))));
                }
                else
                {
                    RunOnUiThreadIfNecessary(() => StartActivity(new Intent(this, typeof(LoginActivity))));
                }

                Finish();
            });
        }
    }
}

