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
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{

    public class SettingsViewController : AppSettingsViewController
    {

        public SettingsViewController()
        {
            File = "Root.inApp";
            ShowDoneButton = false;
            NeverShowPrivacySettings = false;
            ShowCreditsFooter = false;
        }

        public override nfloat GetHeightForFooter(UITableView tableView, nint section)
        {
            var footerText = SettingsReader.GetFooterText(section);

            if (string.IsNullOrWhiteSpace(footerText))
            {
                return 0.0f;
            }

            var width = tableView.Frame.Width - tableView.LayoutMargins.Left - tableView.LayoutMargins.Right;

            var attributes = new UIStringAttributes();
            attributes.Font = Theme.DefaultFont;
            var size = new NSString(footerText).GetBoundingRect(new CGSize(width, nfloat.MaxValue), NSStringDrawingOptions.UsesLineFragmentOrigin, attributes, null);

            return size.Height + 10.0f;
        }
    }
}
