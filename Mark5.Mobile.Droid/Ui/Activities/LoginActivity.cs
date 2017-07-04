using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Authenticator;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Service;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Activities
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
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

            CommonConfig.Logger.Info($"Creating {nameof(LoginActivity)}...");

            OverridePendingTransition(Resource.Animation.fade_in, Resource.Animation.fade_out);

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
            sslSpinner.Adapter = CustomArrayAdapter.CreateWithLeftPaddingMatchingEditText(this, Resource.Array.ssl_modes, Android.Resource.Layout.SimpleSpinnerItem, Resource.Layout.support_simple_spinner_dropdown_item);
            loginButton = FindViewById<AppCompatButton>(Resource.Id.login_button);
            loginButton.Click += LoginButton_Click;

            authenticator = AuthenticatorFactory.Create();

            if (savedInstanceState != null)
            {
                usernameEditText.Text = savedInstanceState.GetString("username");
                passwordEditText.Text = savedInstanceState.GetString("password");
                hostnameEditText.Text = savedInstanceState.GetString("hostname");
                portEditText.Text = savedInstanceState.GetString("port");
                sslSpinner.SetSelection(savedInstanceState.GetInt("ssl"));
            }

            CommonConfig.Logger.Info($"Created {nameof(LoginActivity)}");
        }

        protected override void OnStart()
        {
            base.OnStart();

            CommonConfig.Logger.Info($"Started {nameof(LoginActivity)}");
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            outState.PutString("username", usernameEditText.Text);
            outState.PutString("password", passwordEditText.Text);
            outState.PutString("hostname", hostnameEditText.Text);
            outState.PutString("port", portEditText.Text);
            outState.PutInt("ssl", sslSpinner.SelectedItemPosition);
        }

        protected override void OnRestoreInstanceState(Bundle savedInstanceState)
        {
            base.OnRestoreInstanceState(savedInstanceState);

            usernameEditText.Text = savedInstanceState.GetString("username");
            passwordEditText.Text = savedInstanceState.GetString("password");
            hostnameEditText.Text = savedInstanceState.GetString("hostname");
            portEditText.SetSelection(savedInstanceState.GetInt("ssl"));
        }

        static class MenuItemActions
        {
            public const int SystemReport = 10;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            menu.Add(Menu.None, MenuItemActions.SystemReport, MenuItemActions.SystemReport, Resource.String.create_system_report);

            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == MenuItemActions.SystemReport)
            {
                var dismissAction = Dialogs.ShowInfiniteProgressDialog(this, Resource.String.dialog_creating_report, Resource.String.please_wait);

                Task.Run(() => { return SystemReportCollector.CreateFullReport(); })
                    .ContinueWith(t =>
                        {
                            dismissAction();

                            if (!t.IsFaulted)
                                StartActivity(SystemReportCollector.CreateShareReportIntent(this, t.Result));
                        },
                        TaskScheduler.FromCurrentSynchronizationContext());

                return true;
            }

            return false;
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
                var sslMode = (SslMode) sslSpinner.SelectedItemPosition;

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

                    passwordEditText.Error = GetText(Resource.String.password_invalid);
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
                    return;

                if (sslMode == SslMode.AllowSelfSigned && !await Dialogs.ShowYesNoDialogAsync(this, Resource.String.warning, Resource.String.ssl_accept_selfsigned_warning))
                    return;

                if (sslMode == SslMode.Off && !await Dialogs.ShowYesNoDialogAsync(this, Resource.String.warning, Resource.String.ssl_off_warning))
                    return;

                CommonConfig.Logger.Info($"Logging in... [username={username}, hostname={hostname}, port={port}, ssl={sslMode}]");

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
                Managers.NotificationsManager.DocumentBodyTypeRequest = PlatformConfig.Preferences.DocumentBodyRequestType;
                Managers.SearchManager.DocumentBodyTypeRequest = PlatformConfig.Preferences.DocumentBodyRequestType;

                CommonConfig.Logger.Info("Retrieving system settings...");

                ServerConfig.SystemSettings = await Managers.SystemManager.GetSystemSettingsAsync();

                CommonConfig.Logger.Info($"Starting {nameof(IDocumentsDownloadService)} and {nameof(IDocumentsUploadService)}...");

                await Managers.DocumentsDownloadManager.Start();
                await Managers.OutgoingDocumentsManager.Start();

                CommonConfig.Logger.Info($"Refreshing reachability status...");
                await CommonConfig.Reachability.Refresh();

                CommonConfig.Logger.Info($"Registering {nameof(ReachabilityBroadcastReceiver)}...");
                PlatformConfig.ReachabilityBroadcastReceiver.Register();

                CommonConfig.Logger.Info($"Logged in - will present {nameof(MainActivity)}");

                StartActivity(new Intent(this, typeof(MainActivity)));
                Finish();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error("Log in failed - main exception", ex);

                if (ex.InnerException != null)
                    CommonConfig.Logger.Error("Log in failed - inner exception", ex.InnerException);

                await Dialogs.ShowConfirmDialogAsync(this, Resource.String.log_in_failed_title, Resource.String.log_in_failed_message);
            }
        }
    }
}