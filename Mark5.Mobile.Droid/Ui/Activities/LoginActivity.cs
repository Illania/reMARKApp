using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Firebase.Iid;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Authenticator;
using Mark5.Mobile.Common.Azure;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Azure;
using Mark5.Mobile.Common.Service;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Service;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;
using Mark5.Mobile.Droid.Utilities.DeviceReminder;
using Mark5.Mobile.Droid.Utilities.Workers;
using Mark5.ServiceReference.Exceptions;
using Microsoft.Identity.Client;

namespace Mark5.Mobile.Droid.Ui.Activities
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class LoginActivity : BaseAppCompatActivity
    {
        CancellationTokenSource cts;

        TextInputEditText usernameEditText;
        TextInputEditText passwordEditText;
        TextInputEditText hostnameEditText;
        TextInputEditText portEditText;
        AppCompatSpinner sslSpinner;
        AppCompatButton loginButton;
        AppCompatImageButton loginWithMicrosoftButton;

        IAuthenticator authenticator;
        ConnectionInfo retainedConnectionInfo;

        Action dismissAction;

        public static Intent CreateIntent(Context context)
        {
            var intent = new Intent(context, typeof(LoginActivity));
            return intent;
        }

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
            sslSpinner.Adapter = CustomArrayAdapter.CreateWithLeftPaddingMatchingEditText(this, Resource.Array.ssl_modes, Resource.Layout.login_spinner, Resource.Layout.support_simple_spinner_dropdown_item);
            loginButton = FindViewById<AppCompatButton>(Resource.Id.login_button);
            loginButton.Click += LoginButton_Click;
            loginWithMicrosoftButton = FindViewById<AppCompatImageButton>(Resource.Id.sign_microsoft_button);
            loginWithMicrosoftButton.Click += LoginWithMicrosoftButton_Click;

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

        protected override async void OnResume()
        {
            base.OnResume();

            await RefreshData();

            CommonConfig.Logger.Info($"Resumed {nameof(LoginActivity)}");
        }

        protected override void OnActivityResult(int requestCode,
                                         Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(requestCode,
                                                                                    resultCode,
                                                                                    data);
        }

        async Task RefreshData()
        {
            if (!string.IsNullOrEmpty(usernameEditText.Text + hostnameEditText.Text))
                return;

            retainedConnectionInfo = await authenticator.GetRetainedConnectionInfoAsync();
            if (retainedConnectionInfo != null)
            {
                usernameEditText.Text = retainedConnectionInfo.Username;
                hostnameEditText.Text = retainedConnectionInfo.Hostname;
                portEditText.Text = retainedConnectionInfo.Port.ToString();
                switch (retainedConnectionInfo.SslMode)
                {
                    case SslMode.On:
                        sslSpinner.SetSelection(0);
                        return;
                    case SslMode.AllowSelfSigned:
                        sslSpinner.SetSelection(1);
                        return;
                    case SslMode.Off:
                        sslSpinner.SetSelection(2);
                        return;
                }
            }
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

        async void LoginWithMicrosoftButton_Click(object sender, EventArgs e)
        {
            CommonConfig.Logger.Info($"Attempting login...");

            var btn = (AppCompatImageButton)sender;

            btn.Clickable = false;

            CancellationToken token;

            try
            {
                var microsoftAuthService = new MicrosoftAuthService();
                await microsoftAuthService.Authenticate(this);

                var azureUser = await microsoftAuthService.GetAzureUser();
                var endpointList = await microsoftAuthService.GetAzureEndpointInfoList();

                if (!endpointList.Any())
                    throw new Exception("No connection info was found on Azure");

                AzureEndpointInfo endpointInfo = null;

                if (endpointList.Count > 1)
                {
                    var cInfoNamesList = endpointList.Select(c => c.Name).ToArray();
                    var index = await Dialogs.ShowListDialog(this, "Select system to connect to", cInfoNamesList, false);
                    if (index == -1)
                        return;

                    endpointInfo = endpointList[index];
                }
                else
                    endpointInfo = endpointList.First();

                //We assume that all the connection details are correct (no need to validate or confirm hostname, port, SSL)
                var azureUserId = azureUser.Id;
                var hostname = endpointInfo.Hostname;
                var port = endpointInfo.Port;
                var sslMode = endpointInfo.UseSsl ? SslMode.On : SslMode.Off;

                SetSSLMode(sslMode);

                CommonConfig.Logger.Info($"Logging in with Azure Id... [azureUserId={azureUserId}, hostname={hostname}, port={port}, ssl={sslMode}]");

                cts = new CancellationTokenSource();
                token = cts.Token;

                dismissAction = Dialogs.ShowInfiniteProgressDialog(this, Resource.String.logging_in, Resource.String.please_wait, cts);

                CommonConfig.Logger.Info("Authenticating...");

                var ci = await authenticator.AuthenticateWithAzureAsync(azureUser, sslMode, hostname, port, token);

                await InitializeApplication(ci, token);
            }
            catch (Exception ex)
            {
                await ManageLoginException(ex, token, true);
            }
            finally
            {
                btn.Clickable = true;
            }
        }

        async void LoginButton_Click(object sender, EventArgs e)
        {
            CommonConfig.Logger.Info($"Attempting login...");

            var btn = (AppCompatButton)sender;

            btn.Clickable = false;

            CancellationToken token;

            try
            {
                var username = usernameEditText.Text;
                var password = passwordEditText.Text;
                var hostname = hostnameEditText.Text;
                var port = portEditText.Text;
                var sslMode = (SslMode)sslSpinner.SelectedItemPosition;

                var errors = ValidateInputs(username, password, hostname, port);
                if (errors)
                    return;

                await ConfirmSSLMode(sslMode);
                SetSSLMode(sslMode);

                CommonConfig.Logger.Info($"Logging in... [username={username}, hostname={hostname}, port={port}, ssl={sslMode}]");

                cts = new CancellationTokenSource();
                token = cts.Token;
                dismissAction = Dialogs.ShowInfiniteProgressDialog(this, Resource.String.logging_in, Resource.String.please_wait, cts);

                CommonConfig.Logger.Info("Authenticating...");

                var ci = await authenticator.AuthenticateAsync(username, password, sslMode, hostname, int.Parse(port), token);

                await InitializeApplication(ci, token);
            }
            catch (Exception ex)
            {
                await ManageLoginException(ex, token, false);
            }
            finally
            {
                btn.Clickable = true;
            }
        }

        async Task ConfirmSSLMode(SslMode sslMode)
        {
            if (sslMode == SslMode.AllowSelfSigned
                && !await Dialogs.ShowYesNoDialogAsync(this, Resource.String.warning, Resource.String.ssl_accept_selfsigned_warning))
                return;

            if (sslMode == SslMode.Off
                && !await Dialogs.ShowYesNoDialogAsync(this, Resource.String.warning, Resource.String.ssl_off_warning))
                return;
        }

        void SetSSLMode(SslMode sslMode)
        {
            switch (sslMode)
            {
                case SslMode.AllowSelfSigned:
                    PlatformConfig.SSLCertificateVerificationManager.EnableSelfSignedCertificates();
                    break;
                default:
                    PlatformConfig.SSLCertificateVerificationManager.DisableSelfSignedCertificates();
                    break;
            }
        }

        bool ValidateInputs(string username, string password, string hostname, string port)
        {
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

            return errors;
        }

        async Task InitializeApplication(ConnectionInfo ci, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                CommonConfig.Logger.Info($"Authentication was cancelled...");
                cts = null;
                return;
            }

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
            SystemSettingsWorker.Schedule();

            await Managers.SystemManager.GetSystemUsersDepartmentsAsync();

            CommonConfig.Logger.Info($"Starting services...");
            Services.DocumentsUploadService?.Start();
            Services.DocumentPreviewsDownloadService?.Start();
            Services.DocumentsDownloadService?.Start();
            Services.ActionSyncService?.Start();
            DeviceReminderWorker.Schedule();

            CommonConfig.Logger.Info($"Refreshing reachability status...");
            await CommonConfig.Reachability.Refresh();

            CommonConfig.Logger.Info($"Registering {nameof(ReachabilityMonitor)}...");
            PlatformConfig.ReachabilityMonitor.Register(ApplicationContext);

            CommonConfig.Logger.Info($"Logged in - will present {nameof(MainActivity)}");

            CommonConfig.UsageAnalytics.SetUserProperty(UserProperty.Hostname, ci.Hostname);
            CommonConfig.UsageAnalytics.SetUserProperty(UserProperty.SSL, ci.SslMode.ToString());

            if (!String.IsNullOrEmpty(ServerConfig.SystemSettings.SystemInfo.CustomerName))
                CommonConfig.UsageAnalytics.SetUserProperty(UserProperty.CustomerName, ServerConfig.SystemSettings.SystemInfo.CustomerName);

            PushNotificationsUtilities.CreateChannelIfNotExists(this);
            DeviceReminderBroadcastReceiver.CreateChannelIfNotExists(this);

            SendPushNotificationToken();

            StartActivity(MainActivity.CreateIntent(this));
            Finish();
        }

        async Task ManageLoginException(Exception ex, CancellationToken token, bool loginFromAzure)
        {
            if (token.IsCancellationRequested)
                return;

            dismissAction?.Invoke();

            CommonConfig.Logger.Error("Log in failed - main exception", ex);

            if (ex.InnerException != null)
                CommonConfig.Logger.Error("Log in failed - inner exception", ex.InnerException);

            if (Dialogs.IsAccessDisabled(ex))
                await Dialogs.ShowConfirmDialogAsync(this, Resource.String.log_in_failed_title, Resource.String.log_in_failed_message_access_disabled);
            else if (IsAcountLocked(ex))
                await Dialogs.ShowConfirmDialogAsync(this, Resource.String.log_in_failed_title, Resource.String.log_in_failed_message_account_locked);
            else
                await Dialogs.ShowConfirmDialogAsync(this, Resource.String.log_in_failed_title,
                    loginFromAzure ? Resource.String.log_in_failed_azure_message : Resource.String.log_in_failed_message);
        }

        public static bool IsAcountLocked(Exception ex)
        {
            if (ex is HttpAppServiceException httpEx)
            {
                var code = httpEx?.Detail?.Code;

                return code == AppServiceFaultCode.PasswordPolicyError;
            }

            return false;
        }

        void SendPushNotificationToken()
        {
            try
            {
                var token = FirebaseInstanceId.Instance.Token;

                if (string.IsNullOrEmpty(token))
                    return;

                if (CommonConfig.Logger.IsDebugEnabled())
                    CommonConfig.Logger.Debug($"Firebase token: {token}");

                PlatformConfig.Preferences.PushNotificationToken = token;

                if (Managers.ActiveConnectionInfo != null)
                {
                    CommonConfig.Logger.Info($"Sending Firebase token to service...");

                    Managers.NotificationsManager.Subscribe(DeviceType.Android, token).FireAndForget();
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while subscribing to push notifications after login", ex);
            }
        }

        static class MenuItemActions
        {
            public const int SystemReport = 10;
        }
    }
}
