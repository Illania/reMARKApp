//
// Project: ${Project}
// File: CopyToWorktrayViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{

    public class CopyToWorktrayViewController : AbstractViewController, IUISearchResultsUpdating
    {

        public List<IBusinessEntity> BusinessEntities { get; set; }

        UIBarButtonItem cancelItem;
        UIBarButtonItem doneItem;

        UITableView tableView;
        UISearchController searchController;
        UITableViewController searchResultsController;
        SearchDataSource searchResultsDataSource;

        CancellationTokenSource searchCancellationTokenSource;
        readonly List<CancellationTokenSource> searchCancellationTokenSourceList = new List<CancellationTokenSource>();

        public override void LoadView()
        {
            base.LoadView();

            InitializeNavigationBar();
            InitializeView();
            InitializeSearchBar();
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
            cancelItem = new UIBarButtonItem(UIBarButtonSystemItem.Cancel);
            NavigationItem.SetLeftBarButtonItem(cancelItem, false);

            doneItem = new UIBarButtonItem(UIBarButtonSystemItem.Done);
            doneItem.Enabled = false;
            NavigationItem.SetRightBarButtonItem(doneItem, false);
        }

        void InitializeView()
        {
            AutomaticallyAdjustsScrollViewInsets = true;

            tableView = new UITableView(CGRect.Empty, UITableViewStyle.Grouped);
            tableView.ClipsToBounds = false;
            tableView.Source = new DataSource(this, tableView, Localization.GetString("no_system_users"));
            tableView.AllowsSelection = true;
            tableView.AllowsMultipleSelection = true;
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

        void InitializeSearchBar()
        {
            DefinesPresentationContext = true;

            searchResultsController = new UITableViewController();
            searchResultsDataSource = new SearchDataSource(this, searchResultsController.TableView, Localization.GetString("no_matching_users"));
            searchResultsController.TableView.Source = searchResultsDataSource;
            searchResultsController.TableView.AllowsSelection = true;
            searchResultsController.TableView.AllowsMultipleSelection = true;

            searchController = new UISearchController(searchResultsController)
            {
                HidesNavigationBarDuringPresentation = true,
                DimsBackgroundDuringPresentation = true,
                ObscuresBackgroundDuringPresentation = true,
                SearchResultsUpdater = this
            };
            searchController.SearchBar.Placeholder = Localization.GetString("filter");

            tableView.TableHeaderView = searchController.SearchBar;
        }

        void InitializeNavigationBarTitle()
        {
            NavigationItem.Title = Localization.GetString("copy_to_worktray");
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
            var ds = (DataSource)tableView.Source;

            if (ds.IsOwnSelected)
            {
                var dismissAction = Dialogs.ShowInfiniteProgressDialog("copying_to_own_worktray___");

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

            var selectedUsers = ds.SelectedItems;
            if (selectedUsers.Count > 0)
            {
                var dismissAction = Dialogs.ShowInfiniteProgressDialog("copying_to_worktray___");

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
                var ds = (DataSource)tableView.Source;
                ds.SetItems(usersDepartments.Users);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Could not refresh list of users", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        void EnableDoneButton() => doneItem.Enabled = true;

        void DisableDoneButton() => doneItem.Enabled = false;

        public bool IsUserSelected(SystemUser systemUser)
        {
            var ds = (DataSource)tableView.Source;
            return ds.SelectedItems.Contains(systemUser, LambdaEqualityComparer<SystemUser>.Create(su => su.Id));
        }

        public void SelectUser(SystemUser systemUser)
        {
            var ds = (DataSource)tableView.Source;
            var ip = ds.IndexPathForItem(systemUser);

            if (ip != null)
            {
                tableView.SelectRow(ip, false, UITableViewScrollPosition.None);
            }
        }

        public void DeselectUser(SystemUser systemUser)
        {
            var ds = (DataSource)tableView.Source;
            var ip = ds.IndexPathForItem(systemUser);

            if (ip != null)
            {
                tableView.DeselectRow(ip, false);
            }
        }

        void IUISearchResultsUpdating.UpdateSearchResultsForSearchController(UISearchController searchController)
        {
            var searchText = searchController.SearchBar.Text;

            if (!searchController.Active || string.IsNullOrWhiteSpace(searchText))
            {
                searchCancellationTokenSourceList.ForEach(cts => cts?.Cancel());
                searchCancellationTokenSourceList.Clear();
                searchResultsDataSource.Reset();
            }
            else
            {
                if (searchCancellationTokenSource != null)
                {
                    searchCancellationTokenSource.Cancel();
                    searchCancellationTokenSourceList.Remove(searchCancellationTokenSource);
                    searchCancellationTokenSource = null;
                }

                searchCancellationTokenSource = new CancellationTokenSource();
                searchCancellationTokenSourceList.Add(searchCancellationTokenSource);

                DoSearchUsers(searchText, searchCancellationTokenSource.Token);
            }
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void DoSearchUsers(string searchText, CancellationToken ct)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            searchResultsDataSource.Reset();

            await Task.Delay(500);

            if (ct.IsCancellationRequested) return;

            var ds = (DataSource)tableView.Source;
            var filteredDocuments = ds.Items.Where(dp => MatchesQuery(dp, searchText)).ToList();

            if (ct.IsCancellationRequested) return;

            searchResultsDataSource.SetItems(filteredDocuments);
        }

        static bool MatchesQuery(SystemUser su, string query)
        {
            if (su.Username.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0)
                return true;

            if (su.FirstName.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0)
                return true;

            if (su.LastName.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0)
                return true;

            if (su.PatronymicName.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0)
                return true;

            return false;
        }

        class DataSource : UITableViewSource, IDisposable
        {

            public bool Empty
            {
                get
                {
                    return systemUsersInView.Count < 1;
                }
            }

            public bool IsOwnSelected
            {
                get
                {
                    return tableView.IndexPathsForSelectedRows.Contains(NSIndexPath.FromRowSection(0, 0));
                }
            }

            public List<SystemUser> SelectedItems
            {
                get
                {
                    if (tableView.IndexPathsForSelectedRows == null || tableView.IndexPathsForSelectedRows.Length < 1)
                        return new List<SystemUser>();

                    var rows = tableView.IndexPathsForSelectedRows.ToArray();
                    return rows.Select(ip => systemUsersInView[ip.Row]).ToList();
                }
            }

            public List<SystemUser> Items
            {
                get
                {
                    return systemUsersInView;
                }
            }

            CopyToWorktrayViewController viewController;
            UITableView tableView;
            string emptyText;

            bool loading = true;
            List<SystemUser> systemUsersInView = new List<SystemUser>();

            public DataSource(CopyToWorktrayViewController viewController, UITableView tableView, string emptyText)
            {
                this.viewController = viewController;
                this.tableView = tableView;
                this.emptyText = emptyText;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (indexPath.Section == 0)
                {
                    var ownCell = tableView.DequeueReusableCell("default") ?? new UITableViewCell(UITableViewCellStyle.Default, "default");
                    ownCell.TextLabel.Text = Localization.GetString("own_worktray");
                    ownCell.SelectionStyle = UITableViewCellSelectionStyle.None;

                    ownCell.Accessory = tableView.IndexPathsForSelectedRows != null && tableView.IndexPathsForSelectedRows.Contains(indexPath) ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;

                    return ownCell;
                }

                if (loading)
                {
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as WaitTableViewCell ?? WaitTableViewCell.Create();
                }

                if (systemUsersInView.Count < 1)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                    emptyCell.Initialize(emptyText);
                    return emptyCell;
                }

                var su = systemUsersInView[indexPath.Row];

                var cell = tableView.DequeueReusableCell("subtitle") ?? new UITableViewCell(UITableViewCellStyle.Subtitle, "subtitle");
                cell.TextLabel.Text = $"{su.FirstName} {su.LastName}";
                cell.DetailTextLabel.Text = su.Username;
                cell.SelectionStyle = UITableViewCellSelectionStyle.None;

                cell.Accessory = tableView.IndexPathsForSelectedRows != null && tableView.IndexPathsForSelectedRows.Contains(indexPath) ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;

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

            public override nint NumberOfSections(UITableView tableView)
            {
                return 2;
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return 44f;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.CellAt(indexPath);
                cell.Accessory = UITableViewCellAccessory.Checkmark;

                if (tableView.IndexPathsForSelectedRows != null && tableView.IndexPathsForSelectedRows.Length > 0)
                    viewController.EnableDoneButton();
                else
                    viewController.DisableDoneButton();
            }

            public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.CellAt(indexPath);
                cell.Accessory = UITableViewCellAccessory.None;

                if (tableView.IndexPathsForSelectedRows != null && tableView.IndexPathsForSelectedRows.Length > 0)
                    viewController.EnableDoneButton();
                else
                    viewController.DisableDoneButton();
            }

            public void SetItems(List<SystemUser> systemUsers)
            {
                loading = false;

                systemUsersInView.Clear();
                systemUsersInView.AddRange(systemUsers);

                tableView.ReloadSections(NSIndexSet.FromIndex(1), UITableViewRowAnimation.Automatic);
            }

            public void Reset()
            {
                loading = true;

                systemUsersInView.Clear();
                tableView.ReloadSections(NSIndexSet.FromIndex(1), UITableViewRowAnimation.Automatic);
            }

            public NSIndexPath IndexPathForItem(SystemUser systemUser)
            {
                for (var i = 0; i < systemUsersInView.Count; i++)
                    if (systemUsersInView[i].Id == systemUser.Id)
                        return NSIndexPath.FromRowSection(i, 0);

                return null;
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                viewController = null;
                tableView = null;
                systemUsersInView = null;
            }
        }

        class SearchDataSource : UITableViewSource, IDisposable
        {

            CopyToWorktrayViewController viewController;
            UITableView tableView;
            string emptyText;

            List<SystemUser> systemUsersInView = new List<SystemUser>();

            bool loading = true;

            public SearchDataSource(CopyToWorktrayViewController viewController, UITableView tableView, string emptyText)
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

                if (systemUsersInView.Count < 1)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                    emptyCell.Initialize(emptyText);
                    return emptyCell;
                }

                var su = systemUsersInView[indexPath.Row];

                var cell = tableView.DequeueReusableCell("subtitle") ?? new UITableViewCell(UITableViewCellStyle.Subtitle, "subtitle");
                cell.TextLabel.Text = $"{su.FirstName} {su.LastName}";
                cell.DetailTextLabel.Text = su.Username;
                cell.SelectionStyle = UITableViewCellSelectionStyle.None;

                cell.Accessory = tableView.IndexPathsForSelectedRows != null && tableView.IndexPathsForSelectedRows.Contains(indexPath) ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;

                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading)
                    return 1;

                if (systemUsersInView.Count < 1)
                    return 1;

                return systemUsersInView.Count;
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return 44f;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.CellAt(indexPath);
                cell.Accessory = UITableViewCellAccessory.Checkmark;

                viewController.SelectUser(systemUsersInView[indexPath.Row]);
            }

            public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.CellAt(indexPath);
                cell.Accessory = UITableViewCellAccessory.None;

                viewController.DeselectUser(systemUsersInView[indexPath.Row]);
            }

            public void SetItems(List<SystemUser> systemUsers)
            {
                loading = false;

                systemUsersInView.Clear();
                systemUsersInView.AddRange(systemUsers);

                tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Automatic);

                for (var i = 0; i < systemUsersInView.Count; i++)
                {
                    if (viewController.IsUserSelected(systemUsersInView[i]))
                    {
                        var cell = tableView.CellAt(NSIndexPath.FromRowSection(i, 0));
                        if (cell != null) cell.Accessory = UITableViewCellAccessory.Checkmark;
                        tableView.SelectRow(NSIndexPath.FromRowSection(i, 0), false, UITableViewScrollPosition.None);
                    }
                    else
                    {
                        var cell = tableView.CellAt(NSIndexPath.FromRowSection(i, 0));
                        if (cell != null) cell.Accessory = UITableViewCellAccessory.None;
                    }
                }
            }

            public void Reset()
            {
                loading = true;

                systemUsersInView.Clear();
                tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Automatic);
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                viewController = null;
                tableView = null;
                systemUsersInView = null;
            }
        }
    }
}
