using System;
using System.Threading.Tasks;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class ConnectionDiagnosticsViewController : AbstractViewController
    {
        private UILabel deviceStatusDescription;
        private UILabel serviceStatusDescription;

        public override void LoadView()
        {
            base.LoadView();

            Title = Localization.GetString("diagnostics_title");
            View.BackgroundColor = UIColor.GroupTableViewBackgroundColor;

            UIScrollView scrollView = new UIScrollView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = View.BackgroundColor,
                ShowsVerticalScrollIndicator = false,
                ShowsHorizontalScrollIndicator = false,
                ScrollEnabled = true
            };

            View.AddConstraints(new[]
            {
                scrollView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                scrollView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                scrollView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                scrollView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });

            View.AddSubview(scrollView);

            UILabel deviceStatusTitle = new UILabel
            {
                Font = Theme.DefaultLightBoldFont,
                TextColor = Theme.DarkerBlue,
                LineBreakMode = UILineBreakMode.WordWrap,
                Lines = 0,
                BackgroundColor = Theme.Clear,
                Text = Localization.GetString("diagnostics_device_status"),
                TextAlignment = UITextAlignment.Left,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            View.AddConstraints(new[]
            {
                deviceStatusTitle.LeadingAnchor.ConstraintEqualTo(View.ReadableContentGuide.LeadingAnchor),
                deviceStatusTitle.TrailingAnchor.ConstraintEqualTo(View.ReadableContentGuide.TrailingAnchor),
                deviceStatusTitle.TopAnchor.ConstraintEqualTo(scrollView.TopAnchor,20f)
            });

            scrollView.AddSubview(deviceStatusTitle);

            deviceStatusDescription = new UILabel
            {
                Font = Theme.DefaultFont,
                TextColor = Theme.DarkBlue,
                LineBreakMode = UILineBreakMode.WordWrap,
                Lines = 0,
                BackgroundColor = Theme.Clear,
                Text = "",
                TextAlignment = UITextAlignment.Left,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            View.AddConstraints(new[]
            {
                deviceStatusDescription.LeadingAnchor.ConstraintEqualTo(deviceStatusTitle.LeadingAnchor),
                deviceStatusDescription.TrailingAnchor.ConstraintEqualTo(deviceStatusTitle.TrailingAnchor),
                deviceStatusDescription.TopAnchor.ConstraintEqualTo(deviceStatusTitle.BottomAnchor,20f)
            });

            scrollView.AddSubview(deviceStatusDescription);

            UILabel serviceStatusTitle = new UILabel()
            {
                Font = Theme.DefaultLightBoldFont,
                TextColor = Theme.DarkerBlue,
                LineBreakMode = UILineBreakMode.WordWrap,
                Lines = 0,
                BackgroundColor = Theme.Clear,
                Text = Localization.GetString("diagnostics_service_status"),
                TextAlignment = UITextAlignment.Left,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            View.AddConstraints(new[]
            {
                serviceStatusTitle.LeadingAnchor.ConstraintEqualTo(deviceStatusTitle.LeadingAnchor),
                serviceStatusTitle.TrailingAnchor.ConstraintEqualTo(deviceStatusTitle.TrailingAnchor),
                serviceStatusTitle.TopAnchor.ConstraintEqualTo(deviceStatusDescription.BottomAnchor,20f)
            });

            scrollView.AddSubview(serviceStatusTitle);

            serviceStatusDescription = new UILabel()
            {
                Font = Theme.DefaultLightFont,
                TextColor = Theme.DarkBlue,
                LineBreakMode = UILineBreakMode.WordWrap,
                Lines = 0,
                BackgroundColor = Theme.Clear,
                TextAlignment = UITextAlignment.Left,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            scrollView.AddConstraints(new[]
            {
                serviceStatusDescription.LeadingAnchor.ConstraintEqualTo(deviceStatusTitle.LeadingAnchor),
                serviceStatusDescription.TrailingAnchor.ConstraintEqualTo(deviceStatusTitle.TrailingAnchor),
                serviceStatusDescription.TopAnchor.ConstraintEqualTo(serviceStatusTitle.BottomAnchor,20f)
            });

            scrollView.AddSubview(serviceStatusDescription);

            UIButton refreshButton = new UIButton
            {
                TintColor = Theme.DarkerBlue,
                BackgroundColor = UIColor.GroupTableViewBackgroundColor,
                ClipsToBounds = true,
                ContentEdgeInsets = new UIEdgeInsets(14f, 14f, 14f, 14f),
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            refreshButton.SetTitle(Localization.GetString("diagnostics_refresh").ToUpper(), UIControlState.Normal);
            refreshButton.SetTitleColor(Theme.DarkBlue, UIControlState.Normal);
            refreshButton.SetTitleColor(Theme.DarkGray, UIControlState.Highlighted);
            refreshButton.TitleLabel.Font = Theme.DefaultFont;
            refreshButton.Layer.CornerRadius = 4f;
            refreshButton.TouchUpInside += RefreshButton_TouchUpInside;
            refreshButton.Layer.BorderWidth = 1f;
            refreshButton.Layer.BorderColor = Theme.DarkBlue.CGColor;

            scrollView.AddConstraints(new[]
            {
                refreshButton.LeadingAnchor.ConstraintGreaterThanOrEqualTo(deviceStatusTitle.LeadingAnchor),
                refreshButton.CenterXAnchor.ConstraintEqualTo(scrollView.CenterXAnchor),
                refreshButton.TrailingAnchor.ConstraintGreaterThanOrEqualTo(deviceStatusTitle.TrailingAnchor),
                refreshButton.TopAnchor.ConstraintEqualTo(serviceStatusDescription.BottomAnchor, 40f),
                refreshButton.BottomAnchor.ConstraintGreaterThanOrEqualTo(scrollView.BottomAnchor, -40f)
            });

            scrollView.AddSubview(refreshButton);

            StartDiagnostics();
        }

        private void RefreshButton_TouchUpInside(object sender, EventArgs e)
        {
            StartDiagnostics();
        }

        private void StartDiagnostics()
        {

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("diagnostics_progress"));

            Task.Run(async () =>
            {
                var diagnosticsModel = await CommonConfig.Reachability.ConnectionDiagnostics();

                var ci = Managers.ActiveConnectionInfo;
                var url = $"{(ci.SslMode == SslMode.Off ? "http" : "https")}://{ci.Hostname}:{ci.Port}/app3";

                var connectionStatus = "";

                switch (diagnosticsModel.Status)
                {
                    case ConnectionDiagnosticModel.ConnectionStatus.Stable:
                        connectionStatus = Localization.GetString("diagnostics_stable");
                        break;
                    case ConnectionDiagnosticModel.ConnectionStatus.Unstable:
                        connectionStatus = Localization.GetString("diagnostics_unstable");
                        break;
                    case ConnectionDiagnosticModel.ConnectionStatus.Bad:
                        connectionStatus = Localization.GetString("diagnostics_bad");
                        break;
                    default:
                        connectionStatus = Localization.GetString("diagnostics_broken");
                        break;
                }

                InvokeOnMainThread(() =>
                {
                    string mobileConnectionStatus = CommonConfig.Reachability.IsMobileDataEnabled() ? Localization.GetString("diagnostics_connected") : Localization.GetString("diagnostics_not_connected");
                    string wifiConnectionStatus = CommonConfig.Reachability.IsWifiConnected() ? Localization.GetString("diagnostics_connected") : Localization.GetString("diagnostics_not_connected");

                    deviceStatusDescription.Text = "";
                    serviceStatusDescription.Text = "";

                    deviceStatusDescription.Text += $"{ Localization.GetString("diagnostics_mobile_connection") } : { mobileConnectionStatus }";
                    deviceStatusDescription.Text += $"\r\n{ Localization.GetString("diagnostics_wifi_connection") } : { wifiConnectionStatus }";
                    serviceStatusDescription.Text += $"{ Localization.GetString("diagnostics_service_address") } : { url }";

                    if (diagnosticsModel.Error == ConnectionDiagnosticModel.ErrorCode.None)
                    {
                        serviceStatusDescription.Text += $"\r\n{ Localization.GetString("diagnostics_successfull_requests") } : { diagnosticsModel.SuccessfulRequestCount }";
                        serviceStatusDescription.Text += $"\r\n{ Localization.GetString("diagnostics_failed_requests") } :  { diagnosticsModel.FailedRequestCount }";
                        serviceStatusDescription.Text += $"\r\n{ Localization.GetString("diagnostics_avg_time") } : { diagnosticsModel.AverageElapsedTimeInSeconds } { Localization.GetString("diagnostics_sec") }";
                    }

                    serviceStatusDescription.Text += $"\r\n{ Localization.GetString("diagnostics_connection_status") } : " + connectionStatus;

                    if (diagnosticsModel.Status == ConnectionDiagnosticModel.ConnectionStatus.Bad || diagnosticsModel.Status == ConnectionDiagnosticModel.ConnectionStatus.Broken)
                        serviceStatusDescription.Text += $"\r\n\r\n{ Localization.GetString("diagnostics_bad_broken_description") }";

                    dismissAction.Invoke();
                });
            });
        }
    }
}
