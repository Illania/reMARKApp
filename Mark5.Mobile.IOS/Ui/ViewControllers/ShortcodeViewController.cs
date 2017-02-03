//
// Project: Mark5.Mobile.IOS
// File: ShortcodeViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    
    public class ShortcodeViewController : AbstractViewController, ISecondaryViewController
    {

        public bool Empty { get { return folderId == null && folder == null && shortcodeId == null && shortcodePreview == null && shortcode == null; } }

        int? folderId;
        Folder folder;

        int? shortcodeId;
        ShortcodePreview shortcodePreview;
        Shortcode shortcode;

        UITableView tableView;

        public ShortcodeViewController(int folderId, int shortcodeId)
        {
            this.folderId = folderId;
            this.shortcodeId = shortcodeId;
        }

        public ShortcodeViewController(Folder folder, ShortcodePreview shortcodePreview)
        {
            this.folder = folder;
            this.shortcodePreview = shortcodePreview;
        }

        public ShortcodeViewController(ShortcodePreview shortcodePreview)
        {
            this.shortcodePreview = shortcodePreview;
        }

        public override void LoadView()
        {
            base.LoadView();

            InitializeView();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            InitializeNavigationBarTitle();
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public override async void ViewDidAppear(bool animated)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info($"{nameof(ObjectLinksListViewController)} appeared");

            await RefreshData();
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning($"{nameof(ObjectLinksListViewController)} received memory warning!");

            var ds = tableView?.DataSource as DataSource;
            ds?.Clear();

            base.DidReceiveMemoryWarning();
        }

        void InitializeNavigationBarTitle()
        {
            NavigationItem.Title = shortcodePreview?.Name;
        }

        void InitializeView()
        {
            AutomaticallyAdjustsScrollViewInsets = true;

            tableView = new UITableView(CGRect.Empty, UITableViewStyle.Grouped);
            tableView.ClipsToBounds = false;
            tableView.Source = new DataSource(this, tableView);
            tableView.TranslatesAutoresizingMaskIntoConstraints = false;
            View.AddSubview(tableView);
            View.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1.0f, 0.0f)
                });
        }

        async Task RefreshData()
        {
            CommonConfig.Logger.Info("Loading shortcode...");

            var ds = (DataSource)tableView?.Source;

            try
            {
                ds.StartRefresh();

                if (folderId != null && shortcodeId != null)
                {
                    var swp = await Managers.ShortcodesManager.GetShortcodeWithPreviewAsync(folderId.Value, shortcodeId.Value);
                    shortcodePreview = swp.ShortcodePreview;
                    shortcode = swp.Shortcode;
                }

                if (folder != null && shortcodePreview != null)
                {
                    shortcode = await Managers.ShortcodesManager.GetShortcodeAsync(folder, shortcodePreview.Id);
                }

                if (folderId == null && folder == null && shortcodePreview != null)
                {
                    shortcode = await Managers.ShortcodesManager.GetShortcodeAsync(-1, shortcodePreview.Id);
                }

                var description = shortcodePreview.Description;
                var toAddresses = shortcode.Addresses.Where(da => da.AddressType == DocumentAddressType.To).OrderBy(da => da.Name).ThenBy(da => da.FullAddress).ToArray();
                var ccAddresses = shortcode.Addresses.Where(da => da.AddressType == DocumentAddressType.Cc).OrderBy(da => da.Name).ThenBy(da => da.FullAddress).ToArray();
                var bccAddresses = shortcode.Addresses.Where(da => da.AddressType == DocumentAddressType.Bcc).OrderBy(da => da.Name).ThenBy(da => da.FullAddress).ToArray();

                InitializeNavigationBarTitle();
                ds.EndRefresh(description, toAddresses, ccAddresses, bccAddresses);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Could not load shortcode", ex);

                ds.Clear();

                await Dialogs.ShowErrorDialogAsync(this, ex);

                NavigationController.DismissViewController(true, null);
            }
        }

        class DataSource : UITableViewSource, IDisposable
        {

            ShortcodeViewController viewController;
            UITableView tableView;

            bool empty = true;
            bool loading = true;

            string description = string.Empty;
            DocumentAddress[] toAddresses = new DocumentAddress[0];
            DocumentAddress[] ccAddresses = new DocumentAddress[0];
            DocumentAddress[] bccAddresses = new DocumentAddress[0];

            public DataSource(ShortcodeViewController viewController, UITableView tableView)
            {
                this.viewController = viewController;
                this.tableView = tableView;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (empty)
                    return null;

                if (loading)
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as WaitTableViewCell ?? WaitTableViewCell.Create();

                if (indexPath.Section == 0)
                {
                    var cell = new UITableViewCell(UITableViewCellStyle.Default, "cell");
                    cell.TextLabel.Text = description;
                    return cell;
                }

                if (indexPath.Section == 1)
                {
                    var cell = new UITableViewCell(UITableViewCellStyle.Default, "cell");
                    cell.TextLabel.Text = toAddresses[indexPath.Row].Address;
                    return cell;
                }
                if (indexPath.Section == 2)
                {
                    var cell = new UITableViewCell(UITableViewCellStyle.Default, "cell");
                    cell.TextLabel.Text = ccAddresses[indexPath.Row].Address;
                    return cell;
                }
                if (indexPath.Section == 3)
                {
                    var cell = new UITableViewCell(UITableViewCellStyle.Default, "cell");
                    cell.TextLabel.Text = bccAddresses[indexPath.Row].Address;
                    return cell;
                }

                return null;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (empty)
                    return 0;

                if (loading)
                    return 1;

                if (section == 0)
                    return 1;

                if (section == 1)
                    return toAddresses.Length;

                if (section == 2)
                    return ccAddresses.Length;

                if (section == 3)
                    return bccAddresses.Length;

                return 0;
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                if (empty)
                    return 0;

                if (loading)
                    return 1;

                return 4;
            }

            public override string TitleForHeader(UITableView tableView, nint section)
            {
                if (empty) return string.Empty;
                if (loading) return string.Empty;

                if (section == 0) return Localization.GetString("description");
                if (section == 1) return Localization.GetString("to");
                if (section == 2) return Localization.GetString("cc");
                if (section == 3) return Localization.GetString("bcc");

                return string.Empty;
            }

            public void StartRefresh()
            {
                empty = false;
                loading = true;

                tableView.ReloadData();
            }

            public void EndRefresh(string description, DocumentAddress[] toAddresses, DocumentAddress[] ccAddresses, DocumentAddress[] bccAddresses)
            {
                empty = false;
                loading = false;

                this.description = description;
                this.toAddresses = toAddresses;
                this.ccAddresses = ccAddresses;
                this.bccAddresses = bccAddresses;

                tableView.ReloadData();
            }

            public void Clear()
            {
                empty = true;
                loading = true;

                description = string.Empty;
                toAddresses = new DocumentAddress[0];
                ccAddresses = new DocumentAddress[0];
                bccAddresses = new DocumentAddress[0];

                tableView.ReloadData();
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                tableView = null;
                viewController = null;

                description = null;
                toAddresses = null;
                ccAddresses = null;
                bccAddresses = null;
            }
        }
    }
}
