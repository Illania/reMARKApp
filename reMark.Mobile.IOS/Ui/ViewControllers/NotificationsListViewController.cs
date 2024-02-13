using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Foundation;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Extensions;
using reMark.Mobile.Common.Manager;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Model.HubMessages;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.Common.Utilities.Extensions;
using reMark.Mobile.IOS.Ui.Common;
using reMark.Mobile.IOS.Ui.TableViewCells;
using reMark.Mobile.IOS.Utilities;
using TinyMessenger;
using UIKit;
using DeviceType = reMark.Mobile.Common.Model.DeviceType;

namespace reMark.Mobile.IOS.Ui.ViewControllers
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

            if (objectTypes.Any())
                CommonConfig.UsageAnalytics.LogEvent(new OpenNotificationsEvent(objectTypes[0].Module()));

            InitializeNavigationBar();
            InitializeView();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            RestorationIdentifier = nameof(NotificationsListViewController);
            RestorationClass = Class;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (Integration.IsRunningAtLeast(11))
            {
                if (NavigationController != null)
                    NavigationController.NavigationBar.PrefersLargeTitles = true;
                NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Automatic;
            }

            InitializeHandlers();

            if (TableView?.IndexPathForSelectedRow != null)
                TableView.DeselectRow(TableView.IndexPathForSelectedRow, true);

            if (TableView?.IndexPathsForSelectedRows?.Length > 0)
                foreach (var selectedIndexPath in TableView?.IndexPathsForSelectedRows)
                    TableView.DeselectRow(selectedIndexPath, true);

            ReachabilityBar.Attach(this);
            SendStatusBanner.Attach(this);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info("Appeared");

            newNotificationsMessageToken?.Dispose();
            newNotificationsMessageToken = CommonConfig.MessengerHub.Subscribe<NewNotificationsReceivedMessage>(msg =>
            {
                BeginInvokeOnMainThread(() =>
                {
                    RefreshData();
                });
            });

            RefreshData();

            if (Integration.IsRunningAtLeast(11))
            {
                NSOperationQueue.MainQueue.AddOperation(() =>
                {
                    var ni = NavigationItem;

                    if (ParentViewController != null && ParentViewController is UIViewController && !(ParentViewController is UINavigationController))
                        ni = ParentViewController?.NavigationItem;

                    ni.SearchController = null;
                });
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            newNotificationsMessageToken?.Dispose();

            DeinitializeHandlers();
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            ReachabilityBar.Detach(this);
            SendStatusBanner.Detach(this);
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning("Received memory warning!");

            ((DataSource)TableView.Source)?.Reset();

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        protected override void Recycle()
        {
            base.Recycle();

            markAsReadItem = null;

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
                Image = UIImage.FromBundle("Mark-As-Read").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
                Enabled = false
            };
            NavigationItem.SetRightBarButtonItem(markAsReadItem, false);
        }

        void InitializeView()
        {
            RefreshControl = new UIRefreshControl();

            TableView.Source = new DataSource(this, TableView);
            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 50f;
            TableView.RefreshControl = RefreshControl;
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
            if (objectTypes.Any())
                CommonConfig.UsageAnalytics.LogEvent(new NotificationMarkAllAsReadEvent(objectTypes[0].Module()));

            try
            {
                if (ServerConfig.SystemSettings?.SystemInfo?.SetNotificationReadStatusAvailable != true)
                    await Managers.NotificationsManager.MarkAllAsRead(); 
                else
                {
                    await Managers.NotificationsManager?.SetNotificationReadStatusAsync(PlatformConfig.Preferences.PushNotificationToken,
                                     (((DataSource)TableView.Source)?.Items.Select(n => n.Guid)).ToList(), true);
                }
                Integration.ClearNotificationBadge();
                RefreshData();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Marking notifications as read failed", ex);

                await Dialogs.ShowErrorAlertAsync(this, ex);
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

                if (ServerConfig.SystemSettings?.SystemInfo?.SetNotificationReadStatusAvailable != true)
                    Integration.ClearNotificationBadge();

            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Could not refresh list of notifications", ex);

                await Dialogs.ShowErrorAlertAsync(this, ex);
            }

            RefreshControl.EndRefreshing();
            RefreshControl.ValueChanged += RefreshControl_ValueChanged;

            refreshing = false;
        }

        #endregion

        #region List handlers

        public async void NotificationSelected(Notification notification, NSIndexPath row)
        {
            CommonConfig.UsageAnalytics.LogEvent(new NotificationClickedEvent(notification.ObjectType.Module()));

            if (ServerConfig.SystemSettings?.SystemInfo?.SetNotificationReadStatusAvailable != true)
                await Managers.NotificationsManager.MarkAsRead(notification);
            else
            {
                if (!notification.IsRead)
                    Integration.DecreaseNotificationBadge();

                Managers.NotificationsManager?.SetNotificationReadStatusAsync(PlatformConfig.Preferences.PushNotificationToken, new List<Guid> { notification.Guid }, true);
                RefreshData();
            }
            
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
            var vc = new DocumentViewController();
            vc.SetRefreshDataOnAppear();
            vc.SetData(documentId, notificationGuid);
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        #endregion

        #region DataSource

        class DataSource : UITableViewSource
        {
            public bool Empty => Items.Count < 1;

            readonly WeakReference<NotificationsListViewController> viewControllerWeakReference;
            readonly WeakReference<UITableView> tableViewWeakReference;

            bool loading = true;
            public List<Notification> Items { get; set; } = new List<Notification>();

            public DataSource(NotificationsListViewController viewController, UITableView tableView)
            {
                viewControllerWeakReference = viewController.Wrap();
                tableViewWeakReference = tableView.Wrap();
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                    return tableView.DequeueReusableCell(WaitTableViewCell.DefaultId) as WaitTableViewCell ?? new WaitTableViewCell();

                if (Empty)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.DefaultId) as EmptyTableViewCell ?? new EmptyTableViewCell();
                    emptyCell.Initialize(Localization.GetString("no_notifications"));
                    return emptyCell;
                }

                var n = Items[indexPath.Row];

                var reuseIdentifier = n.Type == EventType.NewObjectCreated ? NotificationsTableViewCell.NewObjectCreatedId : NotificationsTableViewCell.DefaultId;

                var cell = tableView.DequeueReusableCell(reuseIdentifier) as NotificationsTableViewCell ?? new NotificationsTableViewCell(reuseIdentifier);
                cell.Initialize(n);

                return cell;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var n = Items[indexPath.Row];
                viewControllerWeakReference.Unwrap()?.NotificationSelected(n, indexPath);
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading || Empty)
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