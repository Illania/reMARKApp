//
// Project: Mark5.Mobile.Droid
// File: LoginActivity.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Authenticator;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Services;
using Mark5.Mobile.Droid.Views.Common;

namespace Mark5.Mobile.Droid.Views.Activity
{

    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class LoginActivity : BaseAppCompatActivity
    {

        TextInputEditText usernameEditText;
        TextInputEditText passwordEditText;
        TextInputEditText hostnameEditText;
        TextInputEditText portEditText;
        AppCompatSpinner sslSpinner;
        AppCompatButton loginButton;

        IAuthenticator authenticator;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTitle(Resource.String.app_name);
            SetContentView(Resource.Layout.activity_login);

            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            usernameEditText = FindViewById<TextInputEditText>(Resource.Id.username_edit_text);
            usernameEditText.TextChanged += (sender, e) => usernameEditText.Error = null;
            passwordEditText = FindViewById<TextInputEditText>(Resource.Id.password_edit_text);
            passwordEditText.TextChanged += (sender, e) => passwordEditText.Error = null;
            hostnameEditText = FindViewById<TextInputEditText>(Resource.Id.hostname_edit_text);
            hostnameEditText.TextChanged += (sender, e) => hostnameEditText.Error = null;
            portEditText = FindViewById<TextInputEditText>(Resource.Id.port_edit_text);
            portEditText.TextChanged += (sender, e) => portEditText.Error = null;
            sslSpinner = FindViewById<AppCompatSpinner>(Resource.Id.ssl_spinner);
            var sslSpinnerAdapter = Android.Widget.ArrayAdapter.CreateFromResource(this, Resource.Array.ssl_modes, Android.Resource.Layout.SimpleSpinnerItem);
            sslSpinnerAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            sslSpinner.Adapter = sslSpinnerAdapter;
            loginButton = FindViewById<AppCompatButton>(Resource.Id.login_button);
            loginButton.Enabled = false;
            loginButton.Click += LoginButton_Click;

            authenticator = AuthenticatorFactory.Create();

            CommonConfig.Logger.Info($"Created {nameof(LoginActivity)}");
        }

        protected override void OnStart()
        {
            base.OnStart();

            CommonConfig.Logger.Info($"Starting {nameof(LoginActivity)}...");

            Task.Run(async () =>
            {
                return await authenticator.GetConnectionInfoAsync();
            }).ContinueWith(t =>
            {
                var ci = t.Result;

                usernameEditText.Text = ci?.Username;
                passwordEditText.Text = string.Empty;
                hostnameEditText.Text = ci?.Hostname;
                portEditText.Text = ci?.Port.ToString();
                sslSpinner.SetSelection((int?)ci?.SslMode ?? 0);

                loginButton.Enabled = true;
            }, TaskScheduler.FromCurrentSynchronizationContext());

            CommonConfig.Logger.Info($"Started {nameof(LoginActivity)}");
        }

        async void LoginButton_Click(object sender, EventArgs e)
        {
            CommonConfig.Logger.Info($"Attempting login...");

            Action dismissAction = null;

            try
            {
                var username = usernameEditText.Text;
                var password = passwordEditText.Text;
                var hostname = hostnameEditText.Text;
                var port = portEditText.Text;
                var sslMode = (SslMode)sslSpinner.SelectedItemPosition;

                var errors = false;
                if (!Validator.IsUsernameValid(username))
                {
                    CommonConfig.Logger.Info($"Invalid username was entered: {username}");

                    usernameEditText.Error = GetText(Resource.String.username_invalid);
                    errors = true;
                }
                if (!Validator.IsPasswordValid(password))
                {
                    CommonConfig.Logger.Info($"Invalid password was entered: {password}");

                    passwordEditText.Error = GetText(Resource.String.passowrd_invalid);
                    errors = true;
                }
                if (!Validator.IsHostNameValid(hostname))
                {
                    CommonConfig.Logger.Info($"Invalid hostname was entered: {hostname}");

                    hostnameEditText.Error = GetText(Resource.String.hostname_invalid);
                    errors = true;
                }
                if (!Validator.IsPortValid(port))
                {
                    CommonConfig.Logger.Info($"Invalid port was entered: {port}");

                    portEditText.Error = GetText(Resource.String.port_invalid);
                    errors = true;
                }

                if (errors)
                {
                    return;
                }

                if (sslMode == SslMode.AllowSelfSigned && !await Dialogs.ShowYesNoDialogAsync(this, Resource.String.warning, Resource.String.ssl_accept_selfsigned_warning))
                {
                    return;
                }

                if (sslMode == SslMode.Off && !await Dialogs.ShowYesNoDialogAsync(this, Resource.String.warning, Resource.String.ssl_off_warning))
                {
                    return;
                }

                CommonConfig.Logger.Info("Logging in...");

                dismissAction = Dialogs.ShowInfiniteProgressDialog(this, Resource.String.logging_in, Resource.String.please_wait);

                switch (sslMode)
                {
                    case SslMode.AllowSelfSigned:
                        PlatformConfig.SSLCertificateVerificationManager.EnableSelfSignedCertificates();
                        break;
                    default:
                        PlatformConfig.SSLCertificateVerificationManager.DisableSelfSignedCertificates();
                        break;
                }


                CommonConfig.Logger.Info("Authenticating...");

                var ci = await authenticator.AuthenticateAsync(username, password, sslMode, hostname, int.Parse(port));

                CommonConfig.Logger.Info($"Authenticated - saving connection info {ci}...");

                await authenticator.SaveConnectionInfoAsync(ci);

                CommonConfig.Logger.Info($"Initializing {nameof(Managers)}...");

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

                CommonConfig.Logger.Info($"Starting {nameof(IDownloadManager)} and {nameof(IOutgoingDocumentsManager)}...");

                await Managers.DownloadManager.Start();
                await Managers.OutgoingDocumentsManager.Start();

                CommonConfig.Logger.Info($"Registering {nameof(ReachabilityBroadcastReceiver)}...");

                await CommonConfig.ReachabilityService.Refresh();
                PlatformConfig.ReachabilityBroadcastReceiver.Register();

                CommonConfig.Logger.Info("Retrieving system settings...");

                await Managers.SystemManager.GetSystemSettingsAsync();

                CommonConfig.Logger.Info($"Logged in - will present {nameof(MainActivity)}");

                StartActivity(new Intent(this, typeof(MainActivity)));
                Finish();
            }
            catch (Exception ex)
            {
                if (dismissAction != null)
                {
                    dismissAction();
                }

                CommonConfig.Logger.Error("Log in failed", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }
    }

}

