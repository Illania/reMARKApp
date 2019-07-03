using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using Android.Net;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Common
{
    public class ConnectionDiagnosticsFragment : BaseFragment
    {
        AppCompatTextView deviceStatustDetails;
        AppCompatTextView serviceStatustDetails;
        NestedScrollView scrollView;
        readonly int padding = 40;

        public static (ConnectionDiagnosticsFragment fragment, string tag) NewInstance()
        {
            var fragment = new ConnectionDiagnosticsFragment();
            var tag = $"{nameof(ConnectionDiagnosticsFragment)}";

            return (fragment, tag);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var rootView = inflater.Inflate(Resource.Layout.linear_layout_base, container, false);
            LinearLayoutCompat linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);
            scrollView = rootView.FindViewById<NestedScrollView>(Resource.Id.scroll_view);
            scrollView.Visibility = ViewStates.Visible;

            var deviceStatusTitle = new AppCompatTextView(Activity)
            {
                Gravity = GravityFlags.CenterVertical
            };
            deviceStatusTitle.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));
            deviceStatusTitle.SetTextAppearanceCompat(Activity, Resource.Style.fontLargeBold);
            deviceStatusTitle.Text = GetString(Resource.String.diagnostics_device_status);
            deviceStatusTitle.SetPadding(0, 0, 0, padding);
            linearLayout.AddView(deviceStatusTitle, new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));

            deviceStatustDetails = new AppCompatTextView(Activity)
            {
                Gravity = GravityFlags.Top | GravityFlags.Left | GravityFlags.Start
            };

            deviceStatustDetails.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));
            deviceStatustDetails.SetTextAppearanceCompat(Activity, Resource.Style.fontPrimaryLight);
            deviceStatustDetails.SetLineSpacing(16, 1.10f);
            linearLayout.AddView(deviceStatustDetails, new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));

            var serviceStatusTitle = new AppCompatTextView(Activity)
            {
                Gravity = GravityFlags.CenterVertical
            };
            serviceStatusTitle.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));
            serviceStatusTitle.SetTextAppearanceCompat(Activity, Resource.Style.fontLargeBold);
            serviceStatusTitle.SetPadding(0, padding, 0, padding);
            serviceStatusTitle.Text = GetString(Resource.String.diagnostics_service_status);
            linearLayout.AddView(serviceStatusTitle, new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));

            serviceStatustDetails = new AppCompatTextView(Activity)
            {
                Gravity = GravityFlags.Top | GravityFlags.Left | GravityFlags.Start
            };
            serviceStatustDetails.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));
            serviceStatustDetails.SetTextAppearanceCompat(Activity, Resource.Style.fontPrimaryLight);
            serviceStatustDetails.SetLineSpacing(16, 1.10f);

            serviceStatustDetails.SetPadding(0, 0, 0, padding);

            linearLayout.AddView(serviceStatustDetails, new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));

            Button diagnosticsBtn = new Button(Activity)
            {
                Text = GetString(Resource.String.diagnostics_refresh_diagnostics)
            };
            diagnosticsBtn.Click += DiagnosticsBtn_Click;

            linearLayout.AddView(diagnosticsBtn, new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));

            linearLayout.SetPadding(padding, padding, padding, padding);
            linearLayout.DescendantFocusability = DescendantFocusability.BeforeDescendants;
            linearLayout.FocusableInTouchMode = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            StartDiagnostics();
        }

        private void DiagnosticsBtn_Click(object sender, System.EventArgs e)
        {
            StartDiagnostics();
        }

        private void StartDiagnostics()
        {
            System.Action dismissAction = null;

            Activity.RunOnUiThread(() =>
            {
                dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.please_wait, Resource.String.diagnostics_progress_text);
            });

            Task.Run(async () =>
            {

                ConnectionDiagnosticModel diagnosticsModel = await CommonConfig.Reachability.ConnectionDiagnostics();

                var cm = (ConnectivityManager)Application.Context.GetSystemService(Android.Content.Context.ConnectivityService);
                var networkInfo = cm.ActiveNetworkInfo;
                var ci = Managers.ActiveConnectionInfo;
                var url = $"{(ci.SslMode == SslMode.Off ? "http" : "https")}://{ci.Hostname}:{ci.Port}/app3";

                var connectionStatus = "";

                switch (diagnosticsModel.Status)
                {
                    case ConnectionDiagnosticModel.ConnectionStatus.Stable:
                        connectionStatus = GetString(Resource.String.diagnostics_stable);
                        break;
                    case ConnectionDiagnosticModel.ConnectionStatus.Bad:
                        connectionStatus = GetString(Resource.String.diagnostics_bad);
                        break;
                    case ConnectionDiagnosticModel.ConnectionStatus.Unstable:
                        connectionStatus = GetString(Resource.String.diagnostics_unstable);
                        break;
                    default:
                        connectionStatus = GetString(Resource.String.diagnostics_broken);
                        break;
                }

                Activity.RunOnUiThread(() =>
                {
                    deviceStatustDetails.Text = "";
                    serviceStatustDetails.Text = "";

                    string mobileConnectionStatus = CommonConfig.Reachability.IsMobileDataEnabled() ? GetString(Resource.String.diagnostics_connected) : GetString(Resource.String.diagnostics_not_connected);
                    string wifiConnectionStatus = CommonConfig.Reachability.IsWifiConnected() ? GetString(Resource.String.diagnostics_connected) : GetString(Resource.String.diagnostics_not_connected);

                    deviceStatustDetails.Text += $"{GetString(Resource.String.diagnostics_mobile_data)} : " + mobileConnectionStatus;
                    deviceStatustDetails.Text += $"\r\n{GetString(Resource.String.diagnostics_wi_fi)} : " + wifiConnectionStatus;
                    deviceStatustDetails.Text += $"\r\n{GetString(Resource.String.diagnostics_network_name)} : { networkInfo?.ExtraInfo }";
                    serviceStatustDetails.Text += $"{GetString(Resource.String.diagnostics_server_address)} : { url }";

                    if (diagnosticsModel.Error == ConnectionDiagnosticModel.ErrorCode.None)
                    {
                        serviceStatustDetails.Text += $"\r\n{GetString(Resource.String.diagnostics_successfull_requests)} : { diagnosticsModel.SuccessfullRequestCount }";
                        serviceStatustDetails.Text += $"\r\n{GetString(Resource.String.diagnostics_failed_requests)} : { diagnosticsModel.FailedRequestCount }";
                        serviceStatustDetails.Text += $"\r\n{GetString(Resource.String.diagnostics_avg_request_time)} : { diagnosticsModel.AverageEllapsedTimeInSeconds } {GetString(Resource.String.diagnostics_sec)}";
                    }

                    serviceStatustDetails.Text += $"\r\n{GetString(Resource.String.diagnostics_connection_status)} : " + connectionStatus;

                    dismissAction?.Invoke();
                });
            });
        }
    }
}
