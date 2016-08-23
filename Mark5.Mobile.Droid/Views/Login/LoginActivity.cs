//
// Project: Mark5.Mobile.Droid
// File: LoginActivity.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common.Authenticator;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Views.Common;
using Mark5.Mobile.Droid.Views.Main;
using Xamarin;

namespace Mark5.Mobile.Droid.Views.Login
{

    [Activity]
    public class LoginActivity : AppCompatActivity
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

            Title = "MARK5";
            SetContentView(Resource.Layout.Login);

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

            Task.Run(async () =>
            {
                var ci = await authenticator.GetConnectionInfoAsync();

                usernameEditText.Text = ci?.Username;
                passwordEditText.Text = string.Empty;
                hostnameEditText.Text = ci?.Hostname;
                portEditText.Text = ci?.Port.ToString();
                sslSpinner.SetSelection((int?)ci?.SslMode ?? 0);

                loginButton.Enabled = true;
            });
        }

        async void LoginButton_Click(object sender, EventArgs e)
        {
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
                    usernameEditText.Error = "Username is not valid.";
                    errors = true;
                }
                if (!Validator.IsPasswordValid(password))
                {
                    passwordEditText.Error = "Password is not valid.";
                    errors = true;
                }
                if (!Validator.IsHostNameValid(hostname))
                {
                    hostnameEditText.Error = "Hostname is not valid.";
                    errors = true;
                }
                if (!Validator.IsPortValid(port))
                {
                    portEditText.Error = "Port is not valid.";
                    errors = true;
                }

                if (errors)
                {
                    return;
                }

                if (sslMode == SslMode.AllowSelfSigned && !await Dialogs.ShowYesNoDialog(this, "Warning", "You chose to accept certificates. This may decrease security of the connection. Do you want to continue?"))
                {
                    return;
                }
                if (sslMode == SslMode.Off && !await Dialogs.ShowYesNoDialog(this, "Warning", "You chose to turn off SSL. Connection will be unsecure. Do you want to continue?"))
                {
                    return;
                }

                switch (sslMode)
                {
                    case SslMode.AllowSelfSigned:
                        PlatformConfig.SSLCertificateVerificationManager.EnableSelfSignedCertificates();
                        break;
                    default:
                        PlatformConfig.SSLCertificateVerificationManager.DisableSelfSignedCertificates();
                        break;
                }

                var ci = await authenticator.AuthenticateAsync(username, password, sslMode, hostname, int.Parse(port));

                Managers.Initialize(ci);
                PlatformConfig.ReachabilityBroadcastReceiver.Register();

                var ss = await Managers.SystemManager.GetSystemSettingsAsync();

                Insights.Identify($"{ci.Username}@{ci.SslMode},{ci.Hostname}:{ci.Port}", new Dictionary<string, string>
                {
                    [Insights.Traits.FirstName] = ss?.UserInfo?.User?.FirstName,
                    [Insights.Traits.LastName] = ss?.UserInfo?.User?.LastName,
                    ["System Administrator"] = (ss?.UserInfo?.IsSystemAdministrator ?? false) ? "Yes" : "No"
                });

                await authenticator.SaveConnectionInfoAsync(ci);

                StartActivity(new Intent(this, typeof(MainActivity)));
                Finish();
            }
            catch (Exception ex)
            {
                await Dialogs.ShowConfirmDialog(this, "Error", ex.Message);
            }
        }
    }
}

