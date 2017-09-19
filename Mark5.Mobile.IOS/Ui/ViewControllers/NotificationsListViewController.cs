using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
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
        readonly ObjectType[] objectTypes;

        UIBarButtonItem markAsReadItem;

        UIRefreshControl refreshControl;
        UITableView tableView;

        bool refreshing;

        TinyMessageSubscriptionToken newNotificationsMessageToken;

        Func<Notification, bool> unreadFilter = (n) => !n.IsRead;

        public NotificationsListViewController(ObjectType[] objectTypes)
        {
            this.objectTypes = objectTypes;
        }

        public override void LoadView()
        {
            base.LoadView();

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

            InitializeHandlers();

            if (tableView?.IndexPathForSelectedRow != null)
                tableView.DeselectRow(tableView.IndexPathForSelectedRow, true);

            if (tableView?.IndexPathsForSelectedRows?.Length > 0)
                foreach (var selectedIndexPath in tableView?.IndexPathsForSelectedRows)
                    tableView.DeselectRow(selectedIndexPath, true);

            ReachabilityBar.Attach(View, tableView, (float)NavigationController.BottomLayoutGuide.Length, UITextAlignment.Left);
        }

        public override async void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info($"{nameof(NotificationsListViewController)} appeared");

            newNotificationsMessageToken?.Dispose();
            newNotificationsMessageToken = CommonConfig.MessengerHub.Subscribe<NewNotificationsMessage>(msg =>
            {
                BeginInvokeOnMainThread(async () =>
                {
                    await RefreshData();
                });
            });

            var ds = (DataSource)tableView.Source;
            await RefreshData();
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

            var ds = tableView?.Source as DataSource;
            ds?.Reset();

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        void InitializeNavigationBar()
        {
            NavigationController.NavigationBar.PrefersLargeTitles = true;
            NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Automatic;
            NavigationItem.Title = Localization.GetString("notifications");

            markAsReadItem = new UIBarButtonItem();
            markAsReadItem.Image = UIImage.FromBundle(Path.Combine("icons", "markasread.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            markAsReadItem.Enabled = false;
            NavigationItem.SetRightBarButtonItem(markAsReadItem, false);
        }

        void InitializeView()
        {
            refreshControl = new UIRefreshControl();

            tableView = new UITableView();
            tableView.InsetsContentViewsToSafeArea = true;
            tableView.Source = new DataSource(this, tableView, Localization.GetString("no_notifications"));
            tableView.RefreshControl = refreshControl;
            tableView.AllowsSelection = true;
            tableView.RowHeight = UITableView.AutomaticDimension;
            tableView.EstimatedRowHeight = 60f;
            View = tableView;
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
                await Managers.NotificationsManager.MarkAllAsRead();
                await RefreshData();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Marking notifications as read failed", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        async void RefreshControl_ValueChanged(object sender, EventArgs e)
        {
            await RefreshData();
        }

        async Task RefreshData()
        {
            if (refreshing)
                return;

            refreshing = true;
            refreshControl.ValueChanged -= RefreshControl_ValueChanged;

            CommonConfig.Logger.Info($"Refreshing list of notifications");

            try
            {
                var notifications = await Managers.NotificationsManager.GetNotificationsAsync(DeviceType.IOS, PlatformConfig.Preferences.PushNotificationToken);
                notifications = notifications.Where(n => objectTypes.Contains(n.ObjectType)).ToList();
                var ds = (DataSource)tableView.Source;
                ds.SetItems(notifications, PlatformConfig.Preferences.HideReadNotifications ? unreadFilter : null);

                markAsReadItem.Enabled = notifications.Any(n => !n.IsRead);

                ResetNotificationsBadge();
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

        void ResetNotificationsBadge()
        {
            UIApplication.SharedApplication.ApplicationIconBadgeNumber = 1;
            UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;
        }

        public async void NotificationSelected(Notification notification, NSIndexPath row)
        {
            await Managers.NotificationsManager.MarkAsRead(notification);
            tableView.ReloadRows(new[] { row }, UITableViewRowAnimation.Fade);

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
            vc.Modal = true;
            vc.SetRefreshDataOnAppear();
            vc.SetData(documentId, notificationGuid);

            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        class DataSource : UITableViewSource, IDisposable
        {
            public bool Empty => Items.Count < 1;

            public List<Notification> Items { get; private set; } = new List<Notification>();

            NotificationsListViewController viewController;
            UITableView tableView;
            string emptyText;

            bool loading = true;

            public DataSource(NotificationsListViewController viewController, UITableView tableView, string emptyText)
            {
                this.viewController = viewController;
                this.tableView = tableView;
                this.emptyText = emptyText;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as WaitTableViewCell ?? WaitTableViewCell.Create();

                if (Items.Count < 1)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                    emptyCell.Initialize(emptyText);
                    return emptyCell;
                }

                var n = Items[indexPath.Row];

                var cell = tableView.DequeueReusableCell(NotificationsTableViewCell.Key) as NotificationsTableViewCell ?? NotificationsTableViewCell.Create();
                cell.Initialize(n);

                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading)
                    return 1;

                if (Items.Count < 1)
                    return 1;

                return Items.Count;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                if (tableView.CellAt(indexPath).SelectionStyle == UITableViewCellSelectionStyle.None)
                    return;

                var n = Items[indexPath.Row];
                viewController.NotificationSelected(n, indexPath);
            }

            public void SetItems(List<Notification> notifications, Func<Notification, bool> filter = null)
            {
                loading = false;

                Items.Clear();

                if (filter == null)
                    Items.AddRange(notifications);
                else
                    Items.AddRange(notifications.Where(filter).ToList());

                tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            public void Reset()
            {
                loading = true;

                Items.Clear();
                tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                viewController = null;
                tableView = null;
                Items = null;
            }
        }
    }
}