//
// Project: Mark5.Mobile.IOS
// File: NotificationsListViewController.cs
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
    
    public class NotificationsListViewController : AbstractViewController
    {
        
        UITableView tableView;

        public override void LoadView()
        {
            base.LoadView();

            InitializeNavigationBar();
            InitializeView();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            InitializeNavigationBarTitle();
            InitializeHandlers();
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public override async void ViewDidAppear(bool animated)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info($"{nameof(CopyToWorktrayViewController)} appeared");

            var ds = (DataSource)tableView.Source;
            if (ds.Empty)
                await RefreshData();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeinitializeHandlers();
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning($"{nameof(CopyToWorktrayViewController)} received memory warning!");

            var ds = tableView?.DataSource as DataSource;
            ds?.Reset();

            base.DidReceiveMemoryWarning();
        }

        void InitializeNavigationBar()
        {
        }

        void InitializeView()
        {
        }

        void InitializeNavigationBarTitle()
        {
            NavigationItem.Title = Localization.GetString("notfications");
        }

        void InitializeHandlers()
        {
        }

        void DeinitializeHandlers()
        {
        }

        Task RefreshData()
        {
            throw new NotImplementedException();
        }

        class DataSource : UITableViewSource, IDisposable
        {

            public bool Empty
            {
                get
                {
                    return notificationsInView.Count < 1;
                }
            }

            NotificationsListViewController viewController;
            UITableView tableView;
            string emptyText;

            bool loading = true;
            List<Notification> notificationsInView = new List<Notification>();

            public DataSource(NotificationsListViewController viewController, UITableView tableView, string emptyText)
            {
                this.viewController = viewController;
                this.tableView = tableView;
                this.emptyText = emptyText;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                {
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as WaitTableViewCell ?? WaitTableViewCell.Create();
                }

                if (notificationsInView.Count < 1)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                    emptyCell.Initialize(emptyText);
                    return emptyCell;
                }

                var n = notificationsInView[indexPath.Row];

                var cell = tableView.DequeueReusableCell("default") ?? new UITableViewCell(UITableViewCellStyle.Default, "default");
                cell.TextLabel.Text = n.Message;

                cell.Accessory = tableView.IndexPathsForSelectedRows != null && tableView.IndexPathsForSelectedRows.Contains(indexPath) ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;

                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading)
                    return 1;

                if (notificationsInView.Count < 1)
                    return 1;

                return notificationsInView.Count;
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return 44f;
            }

            public void SetItems(List<Notification> systemUsers)
            {
                loading = false;

                notificationsInView.Clear();
                notificationsInView.AddRange(systemUsers);

                tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Automatic);
            }

            public void Reset()
            {
                loading = true;

                notificationsInView.Clear();
                tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Automatic);
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                viewController = null;
                tableView = null;
                notificationsInView = null;
            }
        }
    }
}
