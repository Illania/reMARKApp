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
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common.Authenticator;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Views.Main;
using Xamarin;

namespace Mark5.Mobile.Droid.Views.Login
{

    [Activity(Label = "MARK5", NoHistory = true)]
    public class LoginActivity : AppCompatActivity
    {

        AppCompatEditText usernameEditText;
        AppCompatEditText passwordEditText;
        AppCompatEditText hostnameEditText;
        AppCompatEditText portEditText;
        AppCompatButton loginButton;

        IAuthenticator authenticator;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Title = "Log in";
            SetContentView(Resource.Layout.Login);

            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            usernameEditText = FindViewById<AppCompatEditText>(Resource.Id.username_edit_text);
            usernameEditText.TextChanged += (sender, e) => usernameEditText.Error = null;
            passwordEditText = FindViewById<AppCompatEditText>(Resource.Id.password_edit_text);
            passwordEditText.TextChanged += (sender, e) => passwordEditText.Error = null;
            hostnameEditText = FindViewById<AppCompatEditText>(Resource.Id.hostname_edit_text);
            hostnameEditText.TextChanged += (sender, e) => hostnameEditText.Error = null;
            portEditText = FindViewById<AppCompatEditText>(Resource.Id.port_edit_text);
            portEditText.TextChanged += (sender, e) => portEditText.Error = null;
            loginButton = FindViewById<AppCompatButton>(Resource.Id.login_button);
            loginButton.Enabled = false;
            loginButton.Click += LoginButton_Click;

            authenticator = AuthenticatorFactory.Create();

            Insights.Track($"[{nameof(LoginActivity.OnCreate)}]");

            Task.Run(async () =>
            {
                var ci = await authenticator.GetConnectionInfoAsync();

                usernameEditText.Text = ci?.Username;
                passwordEditText.Text = string.Empty;
                hostnameEditText.Text = ci?.Hostname;
                portEditText.Text = ci?.Port.ToString();

                loginButton.Enabled = true;
            });
        }

        async void LoginButton_Click(object sender, EventArgs e)
        {

            var username = usernameEditText.Text;
            var password = passwordEditText.Text;
            var hostname = hostnameEditText.Text;
            var port = portEditText.Text;

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

            try
            {
                //await authenticator.AuthenticateAsync(username, password, true, hostname, int.Parse(port), DeviceType.Android, "test", "test");

                StartActivity(new Intent(this, typeof(MainActivity)));
            }
            catch (Exception ex)
            {

            }
        }
    }
}

