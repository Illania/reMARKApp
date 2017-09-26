using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Model.HubMessages;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using Mark5.Mobile.IOS.Utilities;
using TinyMessenger;
using UIKit;
using Mark5.Mobile.Common.Utilities.Extensions;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class NotificationsListViewController : AbstractTableViewController
    {
        readonly ObjectType[] objectTypes;

        UIBarButtonItem markAsReadItem;

        bool refreshing;

        Func<Notification, bool> unreadFilter = (n) => !n.IsRead;

        TinyMessageSubscriptionToken newNotificationsMessageToken;

        public NotificationsListViewController(ObjectType[] objectTypes)
        {
            this.objectTypes = objectTypes;
        }

        #region UIViewController overrides

        public override void LoadView()
        {
            base.LoadView();

            InitializeNavigationBar();
            InitializeView();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            if (NavigationController != null)
                NavigationController.NavigationBar.PrefersLargeTitles = true;
            NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Automatic;

            RestorationIdentifier = nameof(NotificationsListViewController);
            RestorationClass = Class;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            InitializeHandlers();

            if (TableView?.IndexPathForSelectedRow != null)
                TableView.DeselectRow(TableView.IndexPathForSelectedRow, true);

            if (TableView?.IndexPathsForSelectedRows?.Length > 0)
                foreach (var selectedIndexPath in TableView?.IndexPathsForSelectedRows)
                    TableView.DeselectRow(selectedIndexPath, true);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info($"{nameof(NotificationsListViewController)} appeared");

            newNotificationsMessageToken?.Dispose();
            newNotificationsMessageToken = CommonConfig.MessengerHub.Subscribe<NewNotificationsMessage>(msg =>
            {
                BeginInvokeOnMainThread(() =>
                {
                    RefreshData();
                });
            });

            RefreshData();

            NSOperationQueue.MainQueue.AddOperation(() =>
            {
                var ni = NavigationItem;

                if (ParentViewController != null && ParentViewController is UIViewController && !(ParentViewController is UINavigationController))
                    ni = ParentViewController?.NavigationItem;

                ni.SearchController = null;
            });
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            newNotificationsMessageToken?.Dispose();

            DeinitializeHandlers();
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning($"{nameof(NotificationsListViewController)} received memory warning!");

            ((DataSource)TableView.Source)?.Reset();

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        public override void Recycle()
        {
            base.Recycle();

            ((DataSource)TableView.Source)?.Reset();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (CommonConfig.Logger.IsDebugEnabled())
                CommonConfig.Logger.Debug("Disposed");
        }

        #endregion

        #region Initialize/deinitialize

        void InitializeNavigationBar()
        {
            NavigationItem.Title = Localization.GetString("notifications");

            markAsReadItem = new UIBarButtonItem
            {
                Image = UIImage.FromBundle(Path.Combine("icons", "markasread.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
                Enabled = false
            };
            NavigationItem.SetRightBarButtonItem(markAsReadItem, false);
        }

        void InitializeView()
        {
            RefreshControl = new UIRefreshControl();

            TableView.Source = new DataSource(this, TableView);
            TableView.RefreshControl = RefreshControl;
            TableView.EstimatedRowHeight = NotificationsTableViewCell.Height;
        }

        void InitializeHandlers()
        {
            if (markAsReadItem != null)
                markAsReadItem.Clicked += MarkAsReadItem_Clicked;

            RefreshControl.ValueChanged += RefreshControl_ValueChanged;
        }

        void DeinitializeHandlers()
        {
            if (markAsReadItem != null)
                markAsReadItem.Clicked -= MarkAsReadItem_Clicked;

            RefreshControl.ValueChanged -= RefreshControl_ValueChanged;
        }

        #endregion

        #region NavigationBar handlers

        async void MarkAsReadItem_Clicked(object sender, EventArgs e)
        {
            try
            {
                await Managers.NotificationsManager.MarkAllAsRead();
                RefreshData();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Marking notifications as read failed", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        #endregion

        #region Refreshing

        void RefreshControl_ValueChanged(object sender, EventArgs e) => RefreshData();

        async void RefreshData()
        {
            if (refreshing)
                return;

            refreshing = true;
            RefreshControl.ValueChanged -= RefreshControl_ValueChanged;

            CommonConfig.Logger.Info($"Refreshing list of notifications");

            try
            {
                var notifications = await Managers.NotificationsManager.GetNotificationsAsync(DeviceType.IOS, PlatformConfig.Preferences.PushNotificationToken);
                notifications = notifications.Where(n => objectTypes.Contains(n.ObjectType)).ToList();
                ((DataSource)TableView.Source).SetItems(notifications, PlatformConfig.Preferences.HideReadNotifications ? unreadFilter : null);

                markAsReadItem.Enabled = notifications.Any(n => !n.IsRead);

                Integration.ClearNotificationBadge();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Could not refresh list of notifications", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);
            }

            RefreshControl.EndRefreshing();
            RefreshControl.ValueChanged += RefreshControl_ValueChanged;

            refreshing = false;
        }

        #endregion

        #region List handlers

        public async void NotificationSelected(Notification notification, NSIndexPath row)
        {
            await Managers.NotificationsManager.MarkAsRead(notification);
            TableView.ReloadRows(new[] { row }, UITableViewRowAnimation.Fade);

            switch (notification.ObjectType)
            {
                case ObjectType.Document:
                    PresentDocumentViewController(notification.ObjectId, notification.Guid);
                    break;
            }
        }

        void PresentDocumentViewController(int documentId, Guid notificationGuid)
        {
            var vc = new DocumentViewController
            {
                Modal = true
            };
            vc.SetRefreshDataOnAppear();
            vc.SetData(documentId, notificationGuid);

            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        #endregion

        #region DataSource

        class DataSource : UITableViewSource
        {

            readonly WeakReference<NotificationsListViewController> viewControllerWeakReference;
            readonly WeakReference<UITableView> tableViewWeakReference;

            public List<Notification> Items { get; } = new List<Notification>();
            bool loading = true;

            public DataSource(NotificationsListViewController viewController, UITableView tableView)
            {
                viewControllerWeakReference = viewController.Wrap();
                tableViewWeakReference = tableView.Wrap();
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as WaitTableViewCell ?? WaitTableViewCell.Create();

                if (Items.Count < 1)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                    emptyCell.Initialize(Localization.GetString("no_notifications"));
                    return emptyCell;
                }

                var n = Items[indexPath.Row];

                var cell = tableView.DequeueReusableCell(NotificationsTableViewCell.Key) as NotificationsTableViewCell ?? NotificationsTableViewCell.Create();
                cell.Initialize(n);

                return cell;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                if (tableViewWeakReference.Unwrap()?.CellAt(indexPath).SelectionStyle == UITableViewCellSelectionStyle.None)
                    return;

                var n = Items[indexPath.Row];
                viewControllerWeakReference.Unwrap()?.NotificationSelected(n, indexPath);
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading)
                    return 1;

                if (Items.Count < 1)
                    return 1;

                return Items.Count;
            }

            public void SetItems(List<Notification> notifications, Func<Notification, bool> filter = null)
            {
                loading = false;

                Items.Clear();

                if (filter == null)
                    Items.AddRange(notifications);
                else
                    Items.AddRange(notifications.Where(filter).ToList());

                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            public void Reset()
            {
                loading = true;

                Items.Clear();
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }
        }

        #endregion

    }
}