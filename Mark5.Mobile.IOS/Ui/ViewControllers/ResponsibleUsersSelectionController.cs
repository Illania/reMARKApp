using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class ResponsibleUsersSelectionController : AbstractTableViewController
    {
        public List<int> PreselectedSystemUserIds { get; set; }

        readonly public TaskCompletionSource<List<SystemUser>> tcs = new TaskCompletionSource<List<SystemUser>>();
        public Task<List<SystemUser>> Result => tcs.Task;

        UIBarButtonItem cancelItem;
        UIBarButtonItem doneItem;

        public override void LoadView()
        {
            base.LoadView();

            InitializeNavigationBar();
            InitializeView();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (NavigationController != null)
                NavigationController.NavigationBar.PrefersLargeTitles = true;
            NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Automatic;

            InitializeHandlers();
        }

        public override async void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info("Appeared");

            if (((DataSource)TableView.Source).Empty)
                await RefreshData();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeinitializeHandlers();
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

            cancelItem = null;
            doneItem = null;

            ((DataSource)TableView.Source)?.Reset();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (CommonConfig.Logger.IsDebugEnabled())
                CommonConfig.Logger.Debug("Disposed");
        }

        void InitializeNavigationBar()
        {
            NavigationItem.Title = Localization.GetString("select_users");

            cancelItem = new UIBarButtonItem(UIBarButtonSystemItem.Cancel);
            NavigationItem.SetLeftBarButtonItem(cancelItem, false);

            doneItem = new UIBarButtonItem(UIBarButtonSystemItem.Done);
            NavigationItem.SetRightBarButtonItem(doneItem, false);
        }

        void InitializeView()
        {
            TableView.Source = new DataSource(this, TableView);
            TableView.AllowsMultipleSelection = true;
        }

        void InitializeHandlers()
        {
            if (cancelItem != null)
                cancelItem.Clicked += CancelItem_Clicked;

            if (doneItem != null)
                doneItem.Clicked += DoneItem_Clicked;
        }

        void DeinitializeHandlers()
        {
            if (cancelItem != null)
                cancelItem.Clicked -= CancelItem_Clicked;

            if (doneItem != null)
                doneItem.Clicked -= DoneItem_Clicked;
        }

        void CancelItem_Clicked(object sender, EventArgs e)
        {
            tcs.SetResult(null);
            DismissViewController(true, null);
        }

        void DoneItem_Clicked(object sender, EventArgs e)
        {
            var selectedUsers = ((DataSource)TableView.Source).SelectedUsers;
            tcs.SetResult(selectedUsers);
            DismissViewController(true, null);
        }

        async Task RefreshData()
        {
            CommonConfig.Logger.Info($"Refreshing list of users");

            try
            {
                var usersDepartments = await Managers.SystemManager.GetSystemUsersDepartmentsAsync();
                usersDepartments.Users.Add(ServerConfig.SystemSettings.UserInfo.User);
                ((DataSource)TableView.Source).SetItems(usersDepartments.Users);

                if (PreselectedSystemUserIds != null)
                    ((DataSource)TableView.Source).SelectedUserIds = PreselectedSystemUserIds;
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Could not refresh list of users", ex);

                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        class DataSource : UITableViewSource
        {
            public bool Empty => items.Count < 1;

            public List<int> SelectedUserIds
            {
                get
                {
                    var tableView = tableViewWeakReference.Unwrap();
                    if (tableView.IndexPathsForSelectedRows == null || tableView.IndexPathsForSelectedRows.Length < 1)
                        return new List<int>();
                    return tableView.IndexPathsForSelectedRows.Select(ip => items[ip.Row].Id).ToList();
                }
                set
                {
                    var selectedIds = value.ToHashSet();
                    for (var i = 0; i < items.Count; i++)
                        if (selectedIds.Contains(items[i].Id))
                        {
                            var ip = NSIndexPath.FromRowSection(i, 0);
                            tableViewWeakReference.Unwrap()?.SelectRow(ip, false, UITableViewScrollPosition.None);
                            RowSelected(tableViewWeakReference.Unwrap(), ip);
                        }
                }
            }

            public List<SystemUser> SelectedUsers
            {
                get
                {
                    var tableView = tableViewWeakReference.Unwrap();
                    if (tableView.IndexPathsForSelectedRows == null || tableView.IndexPathsForSelectedRows.Length < 1)
                        return new List<SystemUser>();
                    return tableView.IndexPathsForSelectedRows.Select(ip => items[ip.Row]).ToList();
                }
                set
                {
                    var selectedIds = value.Select(su => su.Id).ToHashSet();
                    for (var i = 0; i < items.Count; i++)
                        if (selectedIds.Contains(items[i].Id))
                        {
                            var ip = NSIndexPath.FromRowSection(i, 0);
                            tableViewWeakReference.Unwrap()?.SelectRow(ip, false, UITableViewScrollPosition.None);
                            RowSelected(tableViewWeakReference.Unwrap(), ip);
                        }
                }
            }

            readonly WeakReference<ResponsibleUsersSelectionController> viewControllerWeakReference;
            readonly WeakReference<UITableView> tableViewWeakReference;

            bool loading = true;
            readonly List<SystemUser> items = new List<SystemUser>();

            public DataSource(ResponsibleUsersSelectionController viewController, UITableView tableView)
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
                    emptyCell.Initialize(Localization.GetString("no_system_users"));
                    return emptyCell;
                }

                var su = items[indexPath.Row];

                var cell = tableView.DequeueReusableCell("cell") ?? UITableViewCellUtilities.CreateWithSubtitle("cell", UITableViewCellSelectionStyle.None);
                cell.TextLabel.Text = $"{su.FirstName} {su.LastName}";
                cell.DetailTextLabel.Text = su.Username;

                cell.Accessory = tableView.IndexPathsForSelectedRows != null && tableView.IndexPathsForSelectedRows.Contains(indexPath)
                    ? UITableViewCellAccessory.Checkmark
                    : UITableViewCellAccessory.None;

                return cell;
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath) => 50f;

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading || Empty)
                    return 1;

                return items.Count;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.CellAt(indexPath);
                if (cell == null)
                    return;
                
                cell.Accessory = UITableViewCellAccessory.Checkmark;
            }

            public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.CellAt(indexPath);
                if (cell == null)
                    return;
                
                cell.Accessory = UITableViewCellAccessory.None;
            }

            public void SetItems(List<SystemUser> systemUsers)
            {
                loading = false;

                items.Clear();
                items.AddRange(systemUsers.OrderBy(s => s.Username));

                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            public void Reset()
            {
                loading = true;

                items.Clear();
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }
        }
    }
}