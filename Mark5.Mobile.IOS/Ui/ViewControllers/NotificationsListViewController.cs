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
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Model.HubMessages;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using TinyMessenger;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{

    public class NotificationsListViewController : AbstractViewController
    {
        
        readonly ModuleType moduleType;

        UIBarButtonItem markAsReadItem;

        UIRefreshControl refreshControl;
        UITableView tableView;

        bool refreshing;

        TinyMessageSubscriptionToken newNotificationsMessageToken;

        public NotificationsListViewController(ModuleType moduleType)
        {
            this.moduleType = moduleType;
        }

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

            if (tableView?.IndexPathForSelectedRow != null)
                tableView.DeselectRow(tableView.IndexPathForSelectedRow, true);

            if (tableView?.IndexPathsForSelectedRows?.Length > 0)
                foreach (var selectedIndexPath in tableView?.IndexPathsForSelectedRows)
                    tableView.DeselectRow(selectedIndexPath, true);

            ReachabilityBar.Attach(View, tableView, (float)NavigationController.BottomLayoutGuide.Length);
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public override async void ViewDidAppear(bool animated)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info($"{nameof(NotificationsListViewController)} appeared");

            if (newNotificationsMessageToken != null)
            {
                PlatformConfig.MessengerHub.Unsubscribe<NewNotificationsMessage>(newNotificationsMessageToken);
                newNotificationsMessageToken = null;
            }
#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
            newNotificationsMessageToken = PlatformConfig.MessengerHub.Subscribe<NewNotificationsMessage>(msg => InvokeOnMainThread(async () => await RefreshData()));
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void

            var ds = (DataSource)tableView.Source;
            await RefreshData();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            if (newNotificationsMessageToken != null)
            {
                PlatformConfig.MessengerHub.Unsubscribe<NewNotificationsMessage>(newNotificationsMessageToken);
                newNotificationsMessageToken = null;
            }

            DeinitializeHandlers();
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning($"{nameof(NotificationsListViewController)} received memory warning!");

            var ds = tableView?.Source as DataSource;
            ds?.Reset();

            base.DidReceiveMemoryWarning();
        }

        void InitializeNavigationBar()
        {
            markAsReadItem = new UIBarButtonItem();
            markAsReadItem.Title = Localization.GetString("mark_as_read");
            markAsReadItem.Enabled = false;
            NavigationItem.SetRightBarButtonItem(markAsReadItem, false);
        }

        void InitializeView()
        {
            AutomaticallyAdjustsScrollViewInsets = true;

            tableView = new UITableView();
            tableView.ClipsToBounds = false;
            tableView.Source = new DataSource(this, tableView, Localization.GetString("no_notifications"));
            tableView.AllowsSelection = true;
            tableView.RowHeight = UITableView.AutomaticDimension;
            tableView.EstimatedRowHeight = 60f;
            tableView.TranslatesAutoresizingMaskIntoConstraints = false;
            View.AddSubview(tableView);
            View.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1f, 0f),
                    NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1f, 0f),
                    NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1f, 0f),
                    NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1f, 0f)
                });

            refreshControl = new UIRefreshControl();
            refreshControl.BackgroundColor = UIColor.White;
            tableView.AddSubview(refreshControl);
        }

        void InitializeNavigationBarTitle()
        {
            NavigationItem.Title = Localization.GetString("notifications");
        }

        void InitializeHandlers()
        {
            if (markAsReadItem != null)
                markAsReadItem.Clicked += MarkAsReadItem_Clicked;

            if (refreshControl != null)
                refreshControl.ValueChanged += RefreshControl_ValueChanged;
        }

        void DeinitializeHandlers()
        {
            if (markAsReadItem != null)
                markAsReadItem.Clicked -= MarkAsReadItem_Clicked;

            if (refreshControl != null)
                refreshControl.ValueChanged -= RefreshControl_ValueChanged;
        }

        async void MarkAsReadItem_Clicked(object sender, EventArgs e)
        {
            try
            {
                var ds = (DataSource)tableView.Source;
                await Managers.NotificationsManager.MarkAsRead(ds.Items);
                ds.Reload();

                markAsReadItem.Enabled = false;
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Marking notifications as read failed", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        async void RefreshControl_ValueChanged(object sender, EventArgs e) => await RefreshData();

        async Task RefreshData()
        {
            if (refreshing) return;

            refreshing = true;
            refreshControl.ValueChanged -= RefreshControl_ValueChanged;

            CommonConfig.Logger.Info($"Refreshing list of notifications");

            try
            {
                var notifications = await Managers.NotificationsManager.GetNotificationsAsync(DeviceType.IOS, PlatformConfig.Preferences.PushNotificationToken);
                var ds = (DataSource)tableView.Source;
                ds.SetItems(notifications);

                markAsReadItem.Enabled = notifications.Any(n => !n.IsRead);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Could not refresh list of notifications", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);
            }

            refreshControl.EndRefreshing();
            refreshControl.ValueChanged += RefreshControl_ValueChanged;

            refreshing = false;
        }

        public void NotificationSelected(Notification notification)
        {
            switch (notification.ObjectType)
            {
                case ObjectType.Document:
                    PresentDocumentViewController(notification.ObjectId);
                    break;
                case ObjectType.Contact:

                    PresentContactViewController(notification.ObjectId);
                    break;
                case ObjectType.Shortcode:

                    PresentShortcodeViewController(notification.ObjectId);
                    break;
            }
        }

        public void PresentDocumentViewController(int documentId)
        {
            var vc = new DocumentViewController();
            vc.Modal = true;
            vc.SetRefreshDataOnAppear();
            vc.SetData(documentId);

            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        public void PresentContactViewController(int contactId)
        {
            var vc = new ContactViewController();
            vc.Modal = true;
            vc.SetRefreshDataOnAppear();
            vc.SetData(contactId);

            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        public void PresentShortcodeViewController(int shortcodeId)
        {
            var vc = new ShortcodeViewController();
            vc.Modal = true;
            vc.SetRefreshDataOnAppear();
            vc.SetData(shortcodeId);

            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
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

            public List<Notification> Items
            {
                get
                {
                    return notificationsInView;
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

                var cell = tableView.DequeueReusableCell(NotificationsTableViewCell.Key) as NotificationsTableViewCell ?? NotificationsTableViewCell.Create();
                cell.Initialize(n);

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

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                if (tableView.CellAt(indexPath).SelectionStyle == UITableViewCellSelectionStyle.None) return;

                var n = notificationsInView[indexPath.Row];
                viewController.NotificationSelected(n);
            }

            public void SetItems(List<Notification> systemUsers)
            {
                loading = false;

                notificationsInView.Clear();
                notificationsInView.AddRange(systemUsers);

                tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            public void Reload()
            {
                tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            public void Reset()
            {
                loading = true;

                notificationsInView.Clear();
                tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
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
