using System;
using CoreGraphics;
using Foundation;
using InAppSettingsKit;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Common.CallId;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class SettingsViewController : AbstractAppSettingsViewController, ISettingsDelegate
    {
        const string UseServerTimezoneKey = "UseServerTimezone";
        const string CreateSystemReportKey = "createSystemReport";
        const string DocumentBodyRequestTypeKey = "DocumentBodyRequestType";
        const string DocumentsToDownloadKey = "DocumentsToDownload";
        const string CallerIdentificationEnabled = "CallerIdentificationEnabled";
        const string LocalTemplateKey = "localTemplate";
        const string LogoutKey = "logout";
        const string OpenSettingsAppKey = "openSettingsApp";
        const string SendFeedbackKey = "sendFeedback";
        const string ServerAddressKey = "serverAddress";
        const string SslEnabledKey = "sslEnabled";
        const string SynchroniseContactsKey = "SynchroniseContacts";
        const string SynchroniseShortcodesKey = "SynchroniseShortcodes";
        const string UpdateConfigKey = "updateConfig";
        const string UsernameKey = "username";
        const string UseTemplateKey = "UseTemplate";
        const string VersionKey = "version";

        public SettingsViewController()
        {
            File = "Root.inApp";
            ShowDoneButton = false;
            NeverShowPrivacySettings = false;
            ShowCreditsFooter = false;
            Delegate = this;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NSNotificationCenter.DefaultCenter.AddObserver(new NSString(InAppSettingsKit.SettingsStore.AppSettingChangedNotification), SettingsChanged);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            CommonConfig.UsageAnalytics.LogEvent(new OpenSettingsEvent());

            if (Integration.IsRunningAtLeast(11))
            {
                if (NavigationController != null)
                    NavigationController.NavigationBar.PrefersLargeTitles = true;
                NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Automatic;

                TableView.InsetsContentViewsToSafeArea = true;
            }

            RefreshHiddenSettings();
        }

        public override void WillDisplayHeaderView(UITableView tableView, UIView headerView, nint section) => headerView.ApplyTheme();

        public override nfloat GetHeightForFooter(UITableView tableView, nint section)
        {
            var footerText = SettingsReader.GetFooterText(section);

            if (string.IsNullOrWhiteSpace(footerText))
                return 0f;

            var width = tableView.Frame.Width - tableView.LayoutMargins.Left - tableView.LayoutMargins.Right;
            var size = new NSString(footerText).GetBoundingRect(new CGSize(width, nfloat.MaxValue),
                                                                NSStringDrawingOptions.UsesLineFragmentOrigin,
                                                                new UIStringAttributes { Font = Theme.DefaultFont },
                                                                null);

            return size.Height + 10f;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = base.GetCell(tableView, indexPath);

            if (cell.TextLabel != null)
                cell.TextLabel.Font = Theme.DefaultFont;
            if (cell.DetailTextLabel != null)
                cell.DetailTextLabel.Font = Theme.DefaultLightFont;

            return cell;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            var specifier = SettingsReader.GetSpecifier(indexPath);
            if (specifier.Type == "PSMultiValueSpecifier")
            {
                var vc = new CustomSpecifierValuesViewController
                {
                    CurrentSpecifier = specifier,
                    SettingsReader = SettingsReader,
                    SettingsStore = SettingsStore
                };
                vc.View.TintColor = View.TintColor;
                NavigationController.PushViewController(vc, true);
                return;
            }

            base.RowSelected(tableView, indexPath);
        }

        [Export("tableView:cellForSpecifier:")]
        public virtual UITableViewCell GetCellForSpecifier(UITableView tableView, SettingsSpecifier specifier)
        {
            if (specifier.Key == CallerIdentificationEnabled)
            {
                var cell = (CallIdTableViewCell)tableView.DequeueReusableCell(CallIdTableViewCell.Key);
                if (cell == null)
                {
                    cell = new CallIdTableViewCell(async () =>
                    {
                        if (cell.Toggled)
                        {
                            await Dialogs.ShowConfirmAlertAsync(this, Localization.GetString("enable_callid_extension_title"), Localization.GetString("enable_callid_extension_message"));
                        }
                    });
                }
                cell.Toggled = CallIdExtensionUtilities.IsCallIdExtensionEnabled().Result;

                return cell;
            }

            if (specifier.Key == LocalTemplateKey)
            {
                var cell = (EditTextViewCell)tableView.DequeueReusableCell(EditTextViewCell.Key);
                if (cell == null)
                {
                    cell = new EditTextViewCell();
                    cell.ContentChanged += (sender, e) => PlatformConfig.Preferences.LocalTemplate = cell.Content;
                }
                cell.Content = PlatformConfig.Preferences.LocalTemplate;

                return cell;
            }

            if (specifier.Key == UsernameKey)
            {
                var ci = Managers.ActiveConnectionInfo;

                var cell = tableView.DequeueReusableCell("cell") ?? UITableViewCellUtilities.CreateWithSideText("cell");
                cell.TextLabel.Text = specifier.Title;
                cell.DetailTextLabel.Text = ci?.Username;
                cell.DetailTextLabel.TextColor = Theme.DarkGray;
                return cell;
            }

            if (specifier.Key == ServerAddressKey)
            {
                var ci = Managers.ActiveConnectionInfo;

                var cell = tableView.DequeueReusableCell("cell") ?? UITableViewCellUtilities.CreateWithSideText("cell");
                cell.TextLabel.Text = specifier.Title;
                cell.DetailTextLabel.Text = ci?.Hostname + ":" + ci?.Port;
                cell.DetailTextLabel.TextColor = Theme.DarkGray;
                return cell;
            }

            if (specifier.Key == SslEnabledKey)
            {
                var ci = Managers.ActiveConnectionInfo;
                var sslEnabled = ci?.SslMode != SslMode.Off;

                var cell = tableView.DequeueReusableCell("cell") ?? UITableViewCellUtilities.CreateWithSideText("cell");
                cell.TextLabel.Text = specifier.Title;
                cell.DetailTextLabel.Text = sslEnabled ? Localization.GetString("enabled") : Localization.GetString("disabled");
                cell.DetailTextLabel.TextColor = sslEnabled ? Theme.DarkGray : Theme.Brown;
                return cell;
            }

            if (specifier.Key == VersionKey)
            {
                var cell = tableView.DequeueReusableCell("cell") ?? UITableViewCellUtilities.CreateWithSideText("cell");
                cell.TextLabel.Text = specifier.Title;
                cell.DetailTextLabel.Text = $"{NSBundle.MainBundle.InfoDictionary["CFBundleShortVersionString"]} ({NSBundle.MainBundle.InfoDictionary["CFBundleVersion"]})";
                cell.DetailTextLabel.TextColor = Theme.DarkGray;
                return cell;
            }

            return null;
        }

        [Export("tableView:heightForSpecifier:")]
        public virtual nfloat GetHeightForSpecifier(UITableView tableView, SettingsSpecifier specifier)
        {
            switch (specifier.Key)
            {
                case LocalTemplateKey:
                    return 150f;
                case UsernameKey:
                case ServerAddressKey:
                case SslEnabledKey:
                case CallerIdentificationEnabled:
                    return 44f;
                case VersionKey:
                    return 44f;
                default:
                    return 0f;
            }
        }

        [Export("settingsViewController:buttonTappedForSpecifier:")]
        public virtual async void ButtonTappedForSpecifier(AppSettingsViewController sender, SettingsSpecifier specifier)
        {
            if (specifier.Key == SendFeedbackKey)
            {
                try
                {
                    if (!SystemReportCollector.CanMailReport)
                    {
                        await Dialogs.ShowConfirmAlertAsync(this, Localization.GetString("cannot_mail_report_title"), Localization.GetString("cannot_mail_report_content"));
                        return;
                    }

                    var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("creating_system_report___"));

                    var report = await SystemReportCollector.CreateFullReportAsync();

                    dismissAction();

                    var mrc = SystemReportCollector.CreateMailReportController(report);
                    PresentViewController(mrc, true, null);
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error("Could not mail system report", ex);

                    Dialogs.ShowErrorAlert(this, ex);
                }

                return;
            }

            if (specifier.Key == CreateSystemReportKey)
            {
                try
                {
                    var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("creating_system_report___"));

                    var report = await SystemReportCollector.CreateFullReportAsync();

                    dismissAction();

                    var src = SystemReportCollector.CreateShareReportController(report);
                    if (src.PopoverPresentationController != null)
                        src.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate(sender.TableView, sender.TableView.CellAt(sender.SettingsReader.GetIndexPath(specifier.Key)));
                    PresentViewController(src, true, null);
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error("Could not share system report", ex);

                    Dialogs.ShowErrorAlert(this, ex);
                }

                return;
            }

            if (specifier.Key == UpdateConfigKey)
            {
                CommonConfig.UsageAnalytics.LogEvent(new SettingsUpdateSystemConfigurationEvent());

                var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("updating_config___"));

                try
                {
                    var ss = await Managers.SystemManager.GetSystemSettingsAsync(SourceType.Remote);
                    ServerConfig.SystemSettings = ss;

                    await Managers.SystemManager.GetSystemUsersDepartmentsAsync(SourceType.Remote);

                    dismissAction();
                }
                catch (Exception ex)
                {
                    dismissAction();

                    CommonConfig.Logger.Error("Could not retrieve system settings!", ex);

                    await Dialogs.ShowErrorAlertAsync(this, ex);
                }

                return;
            }

            if (specifier.Key == LogoutKey)
            {
                CommonConfig.UsageAnalytics.LogEvent(new SettingsLogOutEvent());

                var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("logging_out___"));

                try
                {
                    if (!string.IsNullOrWhiteSpace(PlatformConfig.Preferences.PushNotificationToken))
                        await Managers.NotificationsManager.UnSubscribe(DeviceType.IOS, PlatformConfig.Preferences.PushNotificationToken);
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error(ex);
                }

                PlatformConfig.Preferences.ResetOnLaunch = true;

                dismissAction();

                Dialogs.ShowBlockingAlert(this, Localization.GetString("please_restart"));

                return;
            }

            if (specifier.Key == OpenSettingsAppKey)
                UIApplication.SharedApplication.OpenUrl(new NSUrl(UIApplication.OpenSettingsUrlString), new NSDictionary(), null);
        }

        public void SettingsViewControllerDidEnd(AppSettingsViewController sender)
        {
            // Nothing to do
        }

        async void SettingsChanged(NSNotification n)
        {
            var key = n.Object.ToString();

            if (key == UseServerTimezoneKey)
                await Dialogs.ShowConfirmAlertAsync(this, Localization.GetString("restart_required_title"), Localization.GetString("restart_required_content"));

            if (key == DocumentsToDownloadKey)
            {
                Managers.DocumentsManager.MaxToFetch = PlatformConfig.Preferences.DocumentsToDownload;
                return;
            }

            if (key == DocumentBodyRequestTypeKey)
            {
                Managers.DocumentsManager.DocumentBodyTypeRequest = PlatformConfig.Preferences.DocumentBodyRequestType;
                Managers.NotificationsManager.DocumentBodyTypeRequest = PlatformConfig.Preferences.DocumentBodyRequestType;
                Managers.SearchManager.DocumentBodyTypeRequest = PlatformConfig.Preferences.DocumentBodyRequestType;
                return;
            }

            if (key == UseTemplateKey)
                RefreshHiddenSettings();
        }

        void RefreshHiddenSettings()
        {
            SetHiddenKeys(PlatformConfig.Preferences.UseTemplate == Preferences.TemplateUsageMode.Local || PlatformConfig.Preferences.UseTemplate == Preferences.TemplateUsageMode.AlwaysAsk
                          ? null
                          : new[] { LocalTemplateKey }, false);
        }

        class CustomSpecifierValuesViewController : SpecifierValuesViewController
        {
            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = base.GetCell(tableView, indexPath);

                if (cell.TextLabel != null)
                    cell.TextLabel.Font = Theme.DefaultFont;
                if (cell.DetailTextLabel != null)
                    cell.DetailTextLabel.Font = Theme.DefaultLightFont;

                return cell;
            }
        }
    }
}