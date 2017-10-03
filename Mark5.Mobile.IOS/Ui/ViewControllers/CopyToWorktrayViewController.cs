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
    public class CopyToWorktrayViewController : AbstractTableViewController
    {
        public List<IBusinessEntity> BusinessEntities { get; set; }

        UIBarButtonItem cancelItem;
        UIBarButtonItem doneItem;

        public CopyToWorktrayViewController()
            : base(UITableViewStyle.Grouped)
        {
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

            if (NavigationController != null)
                NavigationController.NavigationBar.PrefersLargeTitles = true;
            NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Automatic;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

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
            NavigationItem.Title = Localization.GetString("copy_to_worktray");

            cancelItem = new UIBarButtonItem(UIBarButtonSystemItem.Cancel);
            NavigationItem.SetLeftBarButtonItem(cancelItem, false);

            doneItem = new UIBarButtonItem(UIBarButtonSystemItem.Done)
            {
                Enabled = false
            };
            NavigationItem.SetRightBarButtonItem(doneItem, false);
        }

        void InitializeView()
        {
            TableView.Source = new DataSource(this, TableView);
            TableView.AllowsSelection = true;
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
            NavigationController.DismissViewController(true, null);
        }

        async void DoneItem_Clicked(object sender, EventArgs e)
        {
            if (((DataSource)TableView.Source).IsOwnSelected)
            {
                var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("copying_to_own_worktray___"));

                try
                {
                    await Managers.CommonActionsManager.CopyToWorktray(BusinessEntities);
                    dismissAction();
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error("Could not copy to own worktray", ex);

                    dismissAction();
                    await Dialogs.ShowErrorDialogAsync(this, ex);
                }
            }

            var selectedUsers = ((DataSource)TableView.Source).SelectedItems;
            if (selectedUsers.Count > 0)
            {
                var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("copying_to_worktray___"));

                try
                {
                    await Managers.CommonActionsManager.CopyToUserWorktray(BusinessEntities, selectedUsers);
                    dismissAction();
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error("Could not copy to users' worktrays", ex);

                    dismissAction();
                    await Dialogs.ShowErrorDialogAsync(this, ex);
                }
            }

            NavigationController.DismissViewController(true, null);
        }

        async Task RefreshData()
        {
            CommonConfig.Logger.Info($"Refreshing list of users");

            try
            {
                var usersDepartments = await Managers.SystemManager.GetSystemUsersDepartmentsAsync();
                ((DataSource)TableView.Source).SetItems(usersDepartments.Users);

                var firstItemIndexPath = NSIndexPath.FromRowSection(0, 0);
                TableView.SelectRow(firstItemIndexPath, false, UITableViewScrollPosition.None);
                ((DataSource)TableView.Source).RowSelected(TableView, firstItemIndexPath);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Could not refresh list of users", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        void EnableDoneButton() => doneItem.Enabled = true;

        void DisableDoneButton() => doneItem.Enabled = false;

        class DataSource : UITableViewSource
        {
            public bool Empty => systemUsersInView.Count < 1;
            public bool IsOwnSelected => tableViewWeakReference.Unwrap()?.IndexPathsForSelectedRows.Contains(NSIndexPath.FromRowSection(0, 0)) ?? false;

            public List<SystemUser> SelectedItems
            {
                get
                {
                    var tableView = tableViewWeakReference.Unwrap();

                    if (tableView.IndexPathsForSelectedRows == null || tableView.IndexPathsForSelectedRows.Length < 1)
                        return new List<SystemUser>();

                    var rows = tableView.IndexPathsForSelectedRows.Where(indexPath => indexPath.Section != 0).ToArray();
                    return rows.Select(ip => systemUsersInView[ip.Row]).ToList();
                }
            }

            readonly WeakReference<CopyToWorktrayViewController> viewControllerWeakReference;
            readonly WeakReference<UITableView> tableViewWeakReference;

            bool loading = true;
            readonly List<SystemUser> systemUsersInView = new List<SystemUser>(50);

            public DataSource(CopyToWorktrayViewController viewController, UITableView tableView)
            {
                viewControllerWeakReference = viewController.Wrap();
                tableViewWeakReference = tableView.Wrap();
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (indexPath.Section == 0)
                {
                    var ownCell = tableView.DequeueReusableCell("ownCell") ?? UITableViewCellUtilities.CreateDefault("ownCell", UITableViewCellSelectionStyle.None);
                    ownCell.TextLabel.Text = Localization.GetString("own_worktray");
                    ownCell.Accessory = tableView.IndexPathsForSelectedRows != null && tableView.IndexPathsForSelectedRows.Contains(indexPath)
                        ? UITableViewCellAccessory.Checkmark
                        : UITableViewCellAccessory.None;

                    return ownCell;
                }

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
                cell.Accessory = tableView.IndexPathsForSelectedRows != null && tableView.IndexPathsForSelectedRows.Contains(indexPath)
                    ? UITableViewCellAccessory.Checkmark
                    : UITableViewCellAccessory.None;

                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (section == 0)
                    return 1;

                if (loading)
                    return 1;

                if (systemUsersInView.Count < 1)
                    return 1;

                return systemUsersInView.Count;
            }

            public override nint NumberOfSections(UITableView tableView) => 2;

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath) => 50f;

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.CellAt(indexPath);
                cell.Accessory = UITableViewCellAccessory.Checkmark;

                if (tableView.IndexPathsForSelectedRows != null && tableView.IndexPathsForSelectedRows.Length > 0)
                    viewControllerWeakReference.Unwrap()?.EnableDoneButton();
                else
                    viewControllerWeakReference.Unwrap()?.DisableDoneButton();
            }

            public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.CellAt(indexPath);
                cell.Accessory = UITableViewCellAccessory.None;

                if (tableView.IndexPathsForSelectedRows != null && tableView.IndexPathsForSelectedRows.Length > 0)
                    viewControllerWeakReference.Unwrap()?.EnableDoneButton();
                else
                    viewControllerWeakReference.Unwrap()?.DisableDoneButton();
            }

            public void SetItems(List<SystemUser> systemUsers)
            {
                loading = false;

                systemUsersInView.Clear();
                systemUsersInView.AddRange(systemUsers);

                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(1), UITableViewRowAnimation.Fade);
            }

            public void Reset()
            {
                loading = true;

                systemUsersInView.Clear();
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(1), UITableViewRowAnimation.Fade);
            }
        }
    }
}