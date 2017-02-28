//
// Project: Mark5.Mobile.IOS
// File: AbstractMainViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Foundation;
using ObjCRuntime;
using UIKit;
using Mark5.Mobile.IOS.Utilities.Extensions;
using Mark5.Mobile.IOS.Ui.Common;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{

    public class AbstractMainViewController : UITabBarController
    {

        protected const string SearchTag = "search";
        protected const string DocumentTag = "document";
        protected const string ContactTag = "contact";
        protected const string ShortcodeTag = "shortcode";
        protected const string NotificationsTag = "notifications";
        protected const string SettingsTag = "settings";

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var tableView = MoreNavigationController.TopViewController.View as UITableView;
            if (tableView != null)
            {
                var ods = tableView.WeakDataSource;
                tableView.WeakDataSource = new DataSourceProxy(ods);
            }
        }

        class DataSourceProxy : UITableViewDataSource
        {
            
            readonly NSObject ods;

            public DataSourceProxy(NSObject ods)
            {
                this.ods = ods;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var sel = new Selector("tableView:cellForRowAtIndexPath:");

                if (!ods.RespondsToSelector(sel))
                    return null;

                var cell = ods.PerformSelector(sel, tableView, indexPath) as UITableViewCell;

                if (cell.TextLabel != null)
                    cell.TextLabel.Font = Theme.DefaultFont;
                if (cell.DetailTextLabel != null)
                    cell.DetailTextLabel.Font = Theme.DefaultLightFont;

                return cell;
            }

            public override nint RowsInSection(UITableView tableView, nint section)
            {
                var sel = new Selector("tableView:numberOfRowsInSection:");
                if (!ods.RespondsToSelector(sel))
                    return 0;

                return ods.PerformSelectorCustom(sel, tableView, (int)section);
            }
        }
    }
}
