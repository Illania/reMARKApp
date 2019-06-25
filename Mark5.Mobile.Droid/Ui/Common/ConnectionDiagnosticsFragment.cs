using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Common
{
    public class ConnectionDiagnosticsFragment : BaseFragment
    {
        AppCompatTextView textView;
        ProgressBar progressBar;
        NestedScrollView scrollView;

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
            StartDiagnostics();

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_with_progress, container, false);

            LinearLayoutCompat linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);
            scrollView = rootView.FindViewById<NestedScrollView>(Resource.Id.scroll_view);
            progressBar = rootView.FindViewById<ProgressBar>(Resource.Id.progress);

            scrollView.Visibility = ViewStates.Visible;
            progressBar.Visibility = ViewStates.Gone;

            textView = new AppCompatTextView(Activity)
            {
                Gravity = GravityFlags.CenterVertical
            };

            textView.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));
            textView.SetTextAppearanceCompat(Activity, Resource.Style.fontPrimary);

            linearLayout.AddView(textView, new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));

            Button diagnosticsBtn = new Button(Activity);
            diagnosticsBtn.Text = "Run Diagnostics";

            diagnosticsBtn.Click += DiagnosticsBtn_Click;

            linearLayout.AddView(diagnosticsBtn, new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));

            linearLayout.DescendantFocusability = DescendantFocusability.BeforeDescendants;
            linearLayout.FocusableInTouchMode = true;
            return rootView;
        }

        private void DiagnosticsBtn_Click(object sender, System.EventArgs e)
        {
            StartDiagnostics();
        }

        void StartDiagnostics()
        {
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
                        connectionStatus = "Stable";
                        break;
                    case ConnectionDiagnosticModel.ConnectionStatus.Bad:
                        connectionStatus = "Bad";
                        break;
                    case ConnectionDiagnosticModel.ConnectionStatus.Unstable:
                        connectionStatus = "Unstable";
                        break;
                    default:
                        connectionStatus = "Broken";
                        break;
                }

                scrollView.Visibility = ViewStates.Visible;
                progressBar.Visibility = ViewStates.Gone;

                Activity.RunOnUiThread(async () =>
                {
                    textView.Text = "";
                    textView.Text += "\r\nMobile connection : " + CommonConfig.Reachability.IsMobileDataEnabled();
                    textView.Text += "\r\nWifi connection : " + CommonConfig.Reachability.IsWifiConnected();
                    textView.Text += $"\r\nNetwork Name : {networkInfo.ExtraInfo}";
                    textView.Text += $"\r\nServer Address : { url }";
                    if (diagnosticsModel.Error == ConnectionDiagnosticModel.ErrorCode.None)
                    {
                        textView.Text += $"\r\nSuccessfull requests : { diagnosticsModel.SuccessfullRequestCount }";
                        textView.Text += $"\r\nAverage request time : { diagnosticsModel.AverageEllapsedTimeInSeconds } sec.";
                        textView.Text += $"\r\nFailed requests : { diagnosticsModel.FailedRequestCount }";
                        textView.Text += "\r\nConnection status : " + connectionStatus;
                    }
                });
            });
        }
    }
}
