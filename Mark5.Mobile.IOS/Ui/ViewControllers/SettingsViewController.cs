//
// Project: Mark5.Mobile.IOS
// File: SettingsViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using CoreGraphics;
using Foundation;
using InAppSettingsKit;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{

    public class SettingsViewController : AppSettingsViewController, ISettingsDelegate
    {

        const string Value1CellId = "Value1CellId";

        const string DocumentsToDownloadKey = "DocumentsToDownload";
        const string DocumentBodyRequestTypeKey = "DocumentBodyRequestType";
        const string SynchroniseContactsKey = "SynchroniseContacts";
        const string SynchroniseShortcodesKey = "SynchroniseShortcodes";
        const string UsernameKey = "username";
        const string UseTemplateKey = "UseTemplate";
        const string LocalTemplateKey = "localTemplate";
        const string ServerAddressKey = "serverAddress";
        const string SslEnabledKey = "sslEnabled";
        const string VersionKey = "version";
        const string LogoutKey = "logout";
        const string SendFeedbackKey = "sendFeedback";
        const string CreateSystemReportKey = "createSystemReport";
        const string UpdateConfigKey = "updateConfig";
        const string OpenSettingsAppKey = "openSettingsApp";

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

            RefreshHiddenSettings();
        }

        public override nfloat GetHeightForFooter(UITableView tableView, nint section)
        {
            var footerText = SettingsReader.GetFooterText(section);

            if (string.IsNullOrWhiteSpace(footerText)) return 0f;

            var width = tableView.Frame.Width - tableView.LayoutMargins.Left - tableView.LayoutMargins.Right;

            var attributes = new UIStringAttributes();
            attributes.Font = Theme.DefaultFont;
            var size = new NSString(footerText).GetBoundingRect(new CGSize(width, nfloat.MaxValue), NSStringDrawingOptions.UsesLineFragmentOrigin, attributes, null);

            return size.Height + 10f;
        }

        [Export("tableView:cellForSpecifier:")]
        public virtual UITableViewCell GetCellForSpecifier(UITableView tableView, SettingsSpecifier specifier)
        {
            if (specifier.Key == LocalTemplateKey)
            {
                var cell = (EditTextViewCell)tableView.DequeueReusableCell(EditTextViewCell.Key);
                if (cell == null)
                {
                    cell = (EditTextViewCell)EditTextViewCell.Nib.Instantiate(null, null)[0];
                    cell.ContentChanged += (sender, e) => PlatformConfig.Preferences.LocalTemplate = cell.Content;
                }
                cell.Content = PlatformConfig.Preferences.LocalTemplate;

                return cell;
            }

            if (specifier.Key == UsernameKey)
            {
                var cell = tableView.DequeueReusableCell(Value1CellId) ?? new UITableViewCell(UITableViewCellStyle.Value1, Value1CellId);

                cell.TextLabel.Text = specifier.Title;
                cell.DetailTextLabel.Text = Managers.ActiveConnectionInfo?.Username;
                cell.DetailTextLabel.TextColor = UIColor.Gray;
                cell.DetailTextLabel.Font = UIFont.SystemFontOfSize(17f);

                return cell;
            }

            if (specifier.Key == ServerAddressKey)
            {
                var cell = tableView.DequeueReusableCell(Value1CellId) ?? new UITableViewCell(UITableViewCellStyle.Value1, Value1CellId);

                var ci = Managers.ActiveConnectionInfo;

                cell.TextLabel.Text = specifier.Title;
                cell.DetailTextLabel.Text = ci?.Hostname + ":" + ci?.Port;
                cell.DetailTextLabel.TextColor = UIColor.Gray;
                cell.DetailTextLabel.Font = UIFont.SystemFontOfSize(17f);

                return cell;
            }

            if (specifier.Key == SslEnabledKey)
            {
                var cell = tableView.DequeueReusableCell(Value1CellId) ?? new UITableViewCell(UITableViewCellStyle.Value1, Value1CellId);

                var sslOff = Managers.ActiveConnectionInfo?.SslMode != SslMode.Off;

                cell.TextLabel.Text = specifier.Title;
                cell.DetailTextLabel.Text = sslOff ? Localization.GetString("enabled") : Localization.GetString("disabled");
                cell.DetailTextLabel.TextColor = sslOff ? UIColor.Gray : Theme.Brown;
                cell.DetailTextLabel.Font = sslOff ? UIFont.SystemFontOfSize(17f) : UIFont.BoldSystemFontOfSize(17f);

                return cell;
            }

            if (specifier.Key == VersionKey)
            {
                var cell = tableView.DequeueReusableCell(Value1CellId) ?? new UITableViewCell(UITableViewCellStyle.Value1, Value1CellId);

                cell.TextLabel.Text = specifier.Title;
                cell.DetailTextLabel.Text = string.Format("{0} ({1})", NSBundle.MainBundle.InfoDictionary["CFBundleShortVersionString"], NSBundle.MainBundle.InfoDictionary["CFBundleVersion"]);
                cell.DetailTextLabel.TextColor = UIColor.Gray;
                cell.DetailTextLabel.Font = UIFont.SystemFontOfSize(17f);

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
                case VersionKey:
                    return 44f;
                default:
                    return 0f;
            }
        }

        [Export("settingsViewController:buttonTappedForSpecifier:")]
        public virtual async void ButtonTappedForSpecifier(AppSettingsViewController sender, SettingsSpecifier specifier)
        {
            if (specifier.Key == LogoutKey)
            {
                var dismisAction = Dialogs.ShowInfiniteProgressDialog("logging_out___");

                try
                {
                    if (!string.IsNullOrWhiteSpace(PlatformConfig.Preferences.PushNotificationToken))
                    {
                        await Managers.NotificationsManager.UnSubscribe(DeviceType.IOS, PlatformConfig.Preferences.PushNotificationToken);
                    }
                }
                catch
                {
                }

                PlatformConfig.Preferences.ResetOnLaunch = true;

                dismisAction();

                Dialogs.ShowBlockingDialog(this, Localization.GetString("please_restart"));

                return;
            }

            if (specifier.Key == SendFeedbackKey)
            {
                // TODO

                return;
            }

            if (specifier.Key == CreateSystemReportKey)
            {
                var dismissAction = Dialogs.ShowInfiniteProgressDialog("creating_system_report___");

                var report = await SystemReportCollector.CreateFullReportAsync();

                dismissAction();

                var src = SystemReportCollector.CreateShareReportController(report);
                if (src.PopoverPresentationController != null) src.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate(sender.TableView, sender.TableView.CellAt(sender.SettingsReader.GetIndexPath(specifier.Key)));
                PresentViewController(src, true, null);

                return;
            }

            if (specifier.Key == UpdateConfigKey)
            {
                var dismissAction = Dialogs.ShowInfiniteProgressDialog("updating_config___");

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

                    await Dialogs.ShowErrorDialogAsync(this, ex);
                }

                return;
            }

            if (specifier.Key == OpenSettingsAppKey)
            {
                UIApplication.SharedApplication.OpenUrl(new NSUrl(UIApplication.OpenSettingsUrlString), new NSDictionary(), null);
            }
        }

        public void SettingsViewControllerDidEnd(AppSettingsViewController sender)
        {
            // Nothing to do
        }

        async void SettingsChanged(NSNotification n)
        {
            var key = n.Object.ToString();

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

            if (key == SynchroniseContactsKey)
            {
                if (PlatformConfig.Preferences.SynchroniseContacts)
                {
                    Managers.DownloadManager.DownloadPolicies[ObjectType.Contact] = new DownloadAllPolicy();
                }
                else
                {
                    Managers.DownloadManager.DownloadPolicies.Remove(ObjectType.Contact);

                    var result = await Dialogs.ShowYesNoDialogAsync(this, Localization.GetString("clear_contacts_cache_title"), Localization.GetString("clear_contacts_cache_summary"));
                    if (result)
                    {
                        var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("clearing_contacts_cache___"));

                        try
                        {
                            await Managers.CleanUpManager.ClearContactsCache();
                            await Managers.CleanUpManager.CleanUp(new[] { ModuleType.Contacts });

                            dismissAction();
                        }
                        catch (Exception ex)
                        {
                            dismissAction();

                            CommonConfig.Logger.Error("Could not clear contacts cache!", ex);

                            await Dialogs.ShowErrorDialogAsync(this, ex);
                        }
                    }
                }

                return;
            }

            if (key == SynchroniseShortcodesKey)
            {
                if (PlatformConfig.Preferences.SynchroniseShortcodes)
                {
                    Managers.DownloadManager.DownloadPolicies[ObjectType.Shortcode] = new DownloadAllPolicy();
                }
                else
                {
                    Managers.DownloadManager.DownloadPolicies.Remove(ObjectType.Shortcode);

                    var result = await Dialogs.ShowYesNoDialogAsync(this, Localization.GetString("clear_shortcodes_cache_title"), Localization.GetString("clear_shortcodes_cache_summary"));
                    if (result)
                    {
                        var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("clearing_shortcodes_cache___"));

                        try
                        {
                            await Managers.CleanUpManager.ClearShortcodeCache();
                            await Managers.CleanUpManager.CleanUp(new[] { ModuleType.Shortcodes });

                            dismissAction();
                        }
                        catch (Exception ex)
                        {
                            dismissAction();

                            CommonConfig.Logger.Error("Could not clear shortcodes cache!", ex);

                            await Dialogs.ShowErrorDialogAsync(this, ex);
                        }
                    }
                }

                return;
            }

            if (key == UseTemplateKey)
            {
                RefreshHiddenSettings();
            }
        }

        void RefreshHiddenSettings() => SetHiddenKeys(PlatformConfig.Preferences.UseTemplate == Preferences.TemplateUsageMode.Local || PlatformConfig.Preferences.UseTemplate == Preferences.TemplateUsageMode.AlwaysAsk ? null : new[] { LocalTemplateKey }, false);
    }
}
