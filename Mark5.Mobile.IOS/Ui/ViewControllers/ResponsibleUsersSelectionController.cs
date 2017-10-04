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

        public override void Recycle()
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
        }

        void DoneItem_Clicked(object sender, EventArgs e)
        {
            var selectedUsers = ((DataSource)TableView.Source).SelectedItems;
            tcs.SetResult(selectedUsers);
        }

        async Task RefreshData()
        {
            CommonConfig.Logger.Info($"Refreshing list of users");

            try
            {
                var usersDepartments = await Managers.SystemManager.GetSystemUsersDepartmentsAsync();
                usersDepartments.Users.Add(ServerConfig.SystemSettings.UserInfo.User);
                ((DataSource)TableView.Source).SetItems(usersDepartments.Users, PreselectedSystemUserIds);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Could not refresh list of users", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        class DataSource : UITableViewSource
        {
            public bool Empty => systemUsersInView.Count < 1;
            public List<SystemUser> SelectedItems { get; set; } = new List<SystemUser>();

            readonly WeakReference<ResponsibleUsersSelectionController> viewControllerWeakReference;
            readonly WeakReference<UITableView> tableViewWeakReference;

            bool loading = true;
            readonly List<SystemUser> systemUsersInView = new List<SystemUser>();

            public DataSource(ResponsibleUsersSelectionController viewController, UITableView tableView)
            {
                viewControllerWeakReference = viewController.Wrap();
                tableViewWeakReference = tableView.Wrap();
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as WaitTableViewCell ?? WaitTableViewCell.Create();

                if (systemUsersInView.Count < 1)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                    emptyCell.Initialize(Localization.GetString("no_system_users"));
                    return emptyCell;
                }

                var su = systemUsersInView[indexPath.Row];

                var cell = tableView.DequeueReusableCell("cell") ?? UITableViewCellUtilities.CreateWithSubtitle("cell", UITableViewCellSelectionStyle.None);
                cell.TextLabel.Text = $"{su.FirstName} {su.LastName}";
                cell.DetailTextLabel.Text = su.Username;
                cell.Accessory = SelectedItems.Contains(su) ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;

                return cell;
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath) => 50f;

            public override void WillDisplay(UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
            {
                if (SelectedItems.Contains(systemUsersInView.ElementAtOrDefault(indexPath.Row)))
                    tableView.SelectRow(indexPath, false, UITableViewScrollPosition.None);
                else
                    tableView.DeselectRow(indexPath, false);
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading)
                    return 1;

                if (systemUsersInView.Count < 1)
                    return 1;

                return systemUsersInView.Count;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.CellAt(indexPath);
                cell.Accessory = UITableViewCellAccessory.Checkmark;
                SelectedItems.Add(systemUsersInView[indexPath.Row]);
            }

            public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.CellAt(indexPath);
                cell.Accessory = UITableViewCellAccessory.None;
                SelectedItems.Remove(systemUsersInView[indexPath.Row]);
            }

            public void SetItems(List<SystemUser> systemUsers, List<int> preselectedSystemUsersId)
            {
                loading = false;

                systemUsersInView.Clear();
                systemUsersInView.AddRange(systemUsers.OrderBy(s => s.Username));

                SelectedItems.Clear();
                SelectedItems.AddRange(systemUsersInView.Where(s => preselectedSystemUsersId.Contains(s.Id)));

                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            public void Reset()
            {
                loading = true;

                systemUsersInView.Clear();
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }
        }
    }
}