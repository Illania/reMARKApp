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
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{

    public class SettingsViewController : AppSettingsViewController, ISettingsDelegate
    {

        const string Value1CellId = "Value1CellId";

        const string UsernameKey = "username";
        const string LocalTemplateKey = "localTemplate";
        const string ServerAddressKey = "serverAddress";
        const string SslEnabledKey = "sslEnabled";
        const string VersionKey = "version";
        const string CreateSystemReportKey = "createSystemReport";
        const string SendFeedbackKey = "sendFeedback";
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

            RefreshHiddenSettings();
            NSNotificationCenter.DefaultCenter.AddObserver(new NSString(InAppSettingsKit.SettingsStore.AppSettingChangedNotification), n => RefreshHiddenSettings());
        }

        public override nfloat GetHeightForFooter(UITableView tableView, nint section)
        {
            var footerText = SettingsReader.GetFooterText(section);

            if (string.IsNullOrWhiteSpace(footerText)) return 0.0f;

            var width = tableView.Frame.Width - tableView.LayoutMargins.Left - tableView.LayoutMargins.Right;

            var attributes = new UIStringAttributes();
            attributes.Font = Theme.DefaultFont;
            var size = new NSString(footerText).GetBoundingRect(new CGSize(width, nfloat.MaxValue), NSStringDrawingOptions.UsesLineFragmentOrigin, attributes, null);

            return size.Height + 10.0f;
        }

        [Export("tableView:cellForSpecifier:")]
        public virtual UITableViewCell GetCellForSpecifier(UITableView tableView, SettingsSpecifier specifier)
        {
            if (specifier.Key == UsernameKey)
            {
                var cell = tableView.DequeueReusableCell(Value1CellId) ?? new UITableViewCell(UITableViewCellStyle.Value1, Value1CellId);

                cell.TextLabel.Text = specifier.Title;
                cell.DetailTextLabel.Text = Managers.ActiveConnectionInfo?.Username;
                cell.DetailTextLabel.TextColor = UIColor.Gray;
                cell.DetailTextLabel.Font = UIFont.SystemFontOfSize(17.0f);

                return cell;
            }

            if (specifier.Key == ServerAddressKey)
            {
                var cell = tableView.DequeueReusableCell(Value1CellId) ?? new UITableViewCell(UITableViewCellStyle.Value1, Value1CellId);

                var ci = Managers.ActiveConnectionInfo;

                cell.TextLabel.Text = specifier.Title;
                cell.DetailTextLabel.Text = ci?.Hostname + ":" + ci?.Port;
                cell.DetailTextLabel.TextColor = UIColor.Gray;
                cell.DetailTextLabel.Font = UIFont.SystemFontOfSize(17.0f);

                return cell;
            }

            if (specifier.Key == SslEnabledKey)
            {
                var cell = tableView.DequeueReusableCell(Value1CellId) ?? new UITableViewCell(UITableViewCellStyle.Value1, Value1CellId);

                var sslOff = Managers.ActiveConnectionInfo?.SslMode != SslMode.Off;

                cell.TextLabel.Text = specifier.Title;
                cell.DetailTextLabel.Text = sslOff ? Localization.GetString("enabled") : Localization.GetString("disabled");
                cell.DetailTextLabel.TextColor = sslOff ? UIColor.Gray : Theme.Brown;
                cell.DetailTextLabel.Font = sslOff ? UIFont.SystemFontOfSize(17.0f) : UIFont.BoldSystemFontOfSize(17.0f);

                return cell;
            }

            if (specifier.Key == VersionKey)
            {
                var cell = tableView.DequeueReusableCell(Value1CellId) ?? new UITableViewCell(UITableViewCellStyle.Value1, Value1CellId);

                cell.TextLabel.Text = specifier.Title;
                cell.DetailTextLabel.Text = string.Format("{0} ({1})", NSBundle.MainBundle.InfoDictionary["CFBundleShortVersionString"], NSBundle.MainBundle.InfoDictionary["CFBundleVersion"]);
                cell.DetailTextLabel.TextColor = UIColor.Gray;
                cell.DetailTextLabel.Font = UIFont.SystemFontOfSize(17.0f);

                return cell;
            }

            return null;
        }

        [Export("tableView:heightForSpecifier:")]
        public virtual nfloat GetHeightForSpecifier(UITableView tableView, SettingsSpecifier specifier)
        {
            switch (specifier.Key)
            {
                case UsernameKey:
                case ServerAddressKey:
                case SslEnabledKey:
                case VersionKey:
                    return 44.0f;
                default:
                    return 0.0f;
            }
        }

        [Export("settingsViewController:buttonTappedForSpecifier:")]
        public virtual void ButtonTappedForSpecifier(AppSettingsViewController sender, SettingsSpecifier specifier)
        {
            if (specifier.Key == CreateSystemReportKey)
            {
                
            }

            if (specifier.Key == SendFeedbackKey)
            {
                
            }

            if (specifier.Key == UpdateConfigKey)
            {
                
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

        void RefreshHiddenSettings()
        {
            SetHiddenKeys(PlatformConfig.Preferences.UseTemplate == Preferences.TemplateUsageMode.Local || PlatformConfig.Preferences.UseTemplate == Preferences.TemplateUsageMode.AlwaysAsk ? null : new[] { LocalTemplateKey }, false);
        }
    }
}
