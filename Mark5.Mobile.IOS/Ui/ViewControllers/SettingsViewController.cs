using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using InAppSettingsKit;
using LocalAuthentication;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Authenticator;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Common.CallId;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using Mark5.Mobile.IOS.Utilities;
using Mark5.Mobile.IOS.Utilities.Extensions;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    //Documentation : https://github.com/futuretap/InAppSettingsKit

    public class SettingsViewController : AbstractAppSettingsViewController, ISettingsDelegate
    {
        const string UseServerTimezoneKey = "UseServerTimezone";
        const string CreateSystemReportKey = "createSystemReport";
        const string DocumentBodyRequestTypeKey = "DocumentBodyRequestType";
        const string DocumentsToDownloadKey = "DocumentsToDownload";
        const string CallerIdentificationEnabled = "CallerIdentificationEnabled";
        const string AuthorizationIntervalKey = "AuthorizationInterval";
        const string LocalTemplateKey = "localTemplate";
        const string LogoutKey = "logout";
        const string OpenSettingsAppKey = "openSettingsApp";
        const string SendFeedbackKey = "sendFeedback";
        const string ServerAddressKey = "serverAddress";
        const string SslEnabledKey = "sslEnabled";
        const string UpdateConfigKey = "updateConfig";
        const string UsernameKey = "username";
        const string UseTemplateKey = "UseTemplate";
        const string VersionKey = "version";
        const string EmailSwipeActionsKey = "EmailSwipeActions";
        const string SyncFavoriteFoldersKey = "SyncFavoriteFolders";
        const string SyncFavoriteFoldersGroupKey = "SyncFavoriteFoldersGroup";
        const string ConnectionDiagnosticsKey = "ConnectionDiagnostics";

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
                TableView.CellLayoutMarginsFollowReadableWidth = true;
                TableView.InsetsContentViewsToSafeArea = true;
            }

            // Remove TabBarItem title
            Title = "";
            NavigationItem.Title = "Settings";

            RefreshHiddenSettings();
        }

        public override void WillDisplayHeaderView(UITableView tableView, UIView headerView, nint section) => headerView.ApplyTheme();

        public override void WillDisplayFooterView(UITableView tableView, UIView footerView, nint section)
        {
            if (footerView is UITableViewHeaderFooterView footer)
            {
                var text = footer.TextLabel.Text ?? string.Empty;
                footer.TextLabel.Text = null;
                footer.TextLabel.AttributedText = new NSAttributedString(text, new UIStringAttributes { Font = Theme.DefaultFont.WithRelativeSize(-2f) });
            }
        }

        public override nfloat GetHeightForFooter(UITableView tableView, nint section)
        {
            var footerText = SettingsReader.GetFooterText(section);

            if (string.IsNullOrWhiteSpace(footerText))
                return 0f;

            var width = tableView.Frame.Width - tableView.LayoutMargins.Left - tableView.LayoutMargins.Right;
            var size = new NSString(footerText).GetBoundingRect(new CGSize(width, nfloat.MaxValue),
                                                                NSStringDrawingOptions.UsesLineFragmentOrigin,
                                                                new UIStringAttributes { Font = Theme.DefaultFont.WithRelativeSize(-2f) },
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

        public override async void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            var specifier = SettingsReader.GetSpecifier(indexPath);

            if (specifier.Key == ConnectionDiagnosticsKey)
            {
                var diagnosticsVC = new ConnectionDiagnosticsViewController();
                NavigationController.PushViewController(diagnosticsVC, true);
                return;
            }

            if (specifier.Key == EmailSwipeActionsKey)
            {
                var swipeActionVC = new SwipeActionViewController();
                NavigationController.PushViewController(swipeActionVC, true);
                return;
            }

            if (specifier.Type == "PSMultiValueSpecifier")
            {
                if (specifier.Key == AuthorizationIntervalKey)
                {
                    if (!new LAContext().CanEvaluatePolicy(LAPolicy.DeviceOwnerAuthentication, out var error))
                    {
                        await Dialogs.ShowConfirmAlertAsync(this, Localization.GetString("auth_cant_evaluate_policy_title"),
                                                            Localization.GetString("auth_cant_evaluate_policy_content"));
                    }
                }

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
                var cell = tableView.DequeueReusableCell("cell") ?? UITableViewCellUtilities.CreateWithSideText("cell");
                cell.TextLabel.Text = specifier.Title;
                try
                {
                    if (CallIdExtensionUtilities.IsCallIdExtensionEnabled().Result)
                        cell.DetailTextLabel.Text = "Enabled";
                    else
                        cell.DetailTextLabel.Text = "Disabled";

                    cell.DetailTextLabel.TextColor = Theme.DarkGray;
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error("Call ID extension not available exception ", ex);
                }
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

            if (specifier.Key == EmailSwipeActionsKey)
            {
                var cell = tableView.DequeueReusableCell("cell") ?? UITableViewCellUtilities.CreateWithSideText("cell");
                cell.TextLabel.Text = specifier.Title;
                cell.DetailTextLabel.Text = "";
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
                    var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("creating_system_report___"));

                    var report = await SystemReportCollector.CreateFullReportAsync();

                    dismissAction();

                    var sendWithReMARK = await Dialogs.ShowYesNoAlertAsync(this, Localization.GetString("send_with_mark5_title"), Localization.GetString("send_feedback_with_mark5_content"));

                    if (sendWithReMARK)
                    {
                        var cvc = SystemReportCollector.CreateShareFeedbackComposeDocumentViewController(report);
                        PresentViewController(new NavigationController(cvc, UIModalPresentationStyle.PageSheet), true, null);
                    }
                    else
                    {
                        if (!SystemReportCollector.CanMailReport)
                        {
                            await Dialogs.ShowConfirmAlertAsync(this, Localization.GetString("cannot_mail_report_title"), Localization.GetString("cannot_mail_report_content"));
                            return;
                        }
                        PresentViewController(SystemReportCollector.CreateMailFeedbackController(report), true, null);
                    }
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

                    var sendWithReMARK = await Dialogs.ShowYesNoAlertAsync(this, Localization.GetString("send_with_mark5_title"), Localization.GetString("send_report_with_mark5_content"));

                    if (sendWithReMARK)
                    {
                        var cvc = SystemReportCollector.CreateShareReportComposeDocumentViewController(report);
                        PresentViewController(new NavigationController(cvc, UIModalPresentationStyle.PageSheet), true, null);
                    }
                    else
                    {
                        var src = SystemReportCollector.CreateShareReportController(report);
                        if (src.PopoverPresentationController != null)
                            src.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate(sender.TableView, sender.TableView.CellAt(sender.SettingsReader.GetIndexPath(specifier.Key)));
                        PresentViewController(src, true, null);
                    }
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
                    CommonConfig.Logger.Error("Error while unsubscribing during log out!", ex);
                }

                PlatformConfig.Preferences.ResetOnLaunch = true;

                await AuthenticatorFactory.Create().RetainConnectionInfoAsync();

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

            if (key == SyncFavoriteFoldersKey)
                await HandleFavoriteSync();

            if (key == UseTemplateKey)
                RefreshHiddenSettings();
        }

        async Task HandleFavoriteSync()
        {
            if (!PlatformConfig.Preferences.SyncFavoriteFoldersEnabled)
                return;

            try
            {
                var response = await Managers.FoldersManager.GetServiceFavoriteFoldersAsync(retain: false);
                if (response.ModuleFavoriteFolders == null)
                    await Managers.FoldersManager.UpdateServiceFavoriteFoldersAsync();
                else
                {
                    var selectedOption = await Dialogs.ShowListActionSheetWithTitleAsync(this, new string[] { Localization.GetString("sync_fav_folders_use_server"), Localization.GetString("sync_fav_folders_use_device") }, View, Localization.GetString("sync_fav_folders_action_title"), $"{Localization.GetString("sync_fav_folders_action_description")} : {response.UpdatedAt.ToLongDateString()}");

                    if (selectedOption == 0)
                    {
                        foreach (var mff in response.ModuleFavoriteFolders)
                            await Managers.FoldersManager.SetFavoriteFoldersAsync(mff.ModuleType, mff.Folders);

                        var availableModules = new List<ModuleType> { ModuleType.Shortcodes, ModuleType.Contacts, ModuleType.Documents };
                        await Managers.FoldersManager.ClearFavoritesAsync(availableModules.Except(response.ModuleFavoriteFolders.Select(mf => mf.ModuleType)).ToList());
                    }
                    else if (selectedOption == 1)
                        await Managers.FoldersManager.UpdateServiceFavoriteFoldersAsync();
                    else
                    {
                        PlatformConfig.Preferences.SyncFavoriteFoldersEnabled = false;
                        return;
                    }
                }

                PlatformConfig.Preferences.SyncFavoriteFoldersEnabled = true;
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while shynchonizing favorite folders", ex);
                PlatformConfig.Preferences.SyncFavoriteFoldersEnabled = false;
                Dialogs.ShowErrorAlert(this, new Exception(Localization.GetString("sync_error_general")));
            }
        }

        void RefreshHiddenSettings()
        {
            List<string> hiddenKeys = new List<string>();

            if (ServerConfig.SystemSettings?.SystemInfo?.SyncFavoritesAvailable != true)
            {
                hiddenKeys.Add(SyncFavoriteFoldersGroupKey);
                hiddenKeys.Add(SyncFavoriteFoldersKey);
            }

            if (PlatformConfig.Preferences.UseTemplate != Preferences.TemplateUsageMode.Local && PlatformConfig.Preferences.UseTemplate != Preferences.TemplateUsageMode.AlwaysAsk)
                hiddenKeys.Add(LocalTemplateKey);

            SetHiddenKeys(hiddenKeys.ToArray(), false);
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