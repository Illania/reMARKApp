//
// Project: Mark5.Mobile.IOS
// File: AbstractFoldersListViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList
{

    public abstract class AbstractFoldersListViewController : ViewController, IUISearchResultsUpdating
    {

        protected readonly Folder Folder;
        protected readonly bool IsRootOfFoldersList;
        protected readonly bool DisableRowActions;
        protected readonly bool DisableNavigationBarActions;
        protected readonly bool DisableSearch;

        protected UIBarButtonItem EditModeItem;
        protected UIBarButtonItem ComposeDocumentItem;

        protected UIRefreshControl RefreshControl;
        protected UITableView FoldersTableView;
        protected UISearchController SearchController;
        protected UITableViewController SearchResultsController;
        protected SearchDataSource SearchResultsDataSource;

        protected CancellationTokenSource searchCancellationTokenSource;
        protected readonly List<CancellationTokenSource> searchCancellationTokenSourceList = new List<CancellationTokenSource>();

        protected AbstractFoldersListViewController(ModuleType module, bool disableRowActions, bool disableNavigationBarActions, bool disableSearch)
        {
            IsRootOfFoldersList = true;
            Folder = Folder.RootForModule(module);
            DisableRowActions = disableRowActions;
            DisableNavigationBarActions = disableNavigationBarActions;
            DisableSearch = disableSearch;
        }

        /// <summary>
        /// This constructor MUST NOT be public!
        /// </summary>
        protected AbstractFoldersListViewController(Folder folder, bool disableRowActions, bool disableNavigationBarActions, bool disableSearch)
        {
            IsRootOfFoldersList = false;
            Folder = folder;
            DisableRowActions = disableRowActions;
            DisableNavigationBarActions = disableNavigationBarActions;
            DisableSearch = disableSearch;
        }

        #region UIViewController overrides

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

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            RefreshData();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            ClearNavigationBarTitle();
            DeinitializeHandlers();
        }

        public override void DidReceiveMemoryWarning()
        {
            var ds = FoldersTableView?.DataSource as DataSource;
            ds?.Reset();

            var gds = FoldersTableView?.DataSource as GrouppedDataSource;
            gds?.Reset();

            base.DidReceiveMemoryWarning();
        }

        #endregion

        #region Initialize/deinitialize

        void InitializeNavigationBarTitle()
        {
            if (IsRootOfFoldersList)
            {
                NavigationItem.Title = Localization.GetString("documents");
            }
            else
            {
                NavigationItem.Title = Folder.Name;
                NavigationItem.Prompt = Localization.GetString("documents");
            }
        }

        void ClearNavigationBarTitle()
        {
            if (IsRootOfFoldersList)
            {
                NavigationItem.Title = Localization.GetString("back");
            }
        }

        void InitializeNavigationBar()
        {
            if (DisableNavigationBarActions) return;

            if (Folder.Module == ModuleType.Documents)
            {
                ComposeDocumentItem = new UIBarButtonItem();
                ComposeDocumentItem.Image = UIImage.FromBundle(Path.Combine("icons", "compose.png"));
                NavigationItem.SetRightBarButtonItem(ComposeDocumentItem, false);

                if (IsRootOfFoldersList)
                {
                    EditModeItem = new UIBarButtonItem();
                    EditModeItem.Title = Localization.GetString("edit");
                    NavigationItem.SetLeftBarButtonItem(EditModeItem, false);
                }

                return;
            }

            if (Folder.Module == ModuleType.Contacts || Folder.Module == ModuleType.Shortcodes)
            {
                if (IsRootOfFoldersList)
                {
                    EditModeItem = new UIBarButtonItem();
                    EditModeItem.Title = Localization.GetString("edit");
                    NavigationItem.SetLeftBarButtonItem(EditModeItem, false);
                }

                return;
            }

            throw new ArgumentException(nameof(Folder.Module));
        }

        void InitializeView()
        {
            AutomaticallyAdjustsScrollViewInsets = true;

            FoldersTableView = new UITableView(CGRect.Empty, UITableViewStyle.Grouped);
            FoldersTableView.ClipsToBounds = false;
            if (IsRootOfFoldersList)
                FoldersTableView.Source = new GrouppedDataSource(this, FoldersTableView, Folder.Module, DisableRowActions);
            else
                FoldersTableView.Source = new DataSource(this, FoldersTableView, DisableRowActions);
            FoldersTableView.AllowsSelectionDuringEditing = false;
            FoldersTableView.TranslatesAutoresizingMaskIntoConstraints = false;
            View.AddSubview(FoldersTableView);
            View.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(FoldersTableView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(FoldersTableView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(FoldersTableView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(FoldersTableView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1.0f, 0.0f)
                });

            RefreshControl = new UIRefreshControl();
            RefreshControl.BackgroundColor = UIColor.White;
            RefreshControl.AttributedTitle = Localization.GetNSAttributedString("pull_to_refresh");
            FoldersTableView.AddSubview(RefreshControl);
        }

        void InitializeSearchBar()
        {
            DefinesPresentationContext = true;

            SearchResultsController = new UITableViewController();
            SearchResultsDataSource = new SearchDataSource(this, SearchResultsController.TableView);
            SearchResultsController.TableView.Source = SearchResultsDataSource;

            SearchController = new UISearchController(SearchResultsController)
            {
                HidesNavigationBarDuringPresentation = true,
                DimsBackgroundDuringPresentation = true,
                ObscuresBackgroundDuringPresentation = true,
                SearchResultsUpdater = this
            };
            SearchController.SearchBar.Placeholder = Localization.GetString("filter");

            FoldersTableView.TableHeaderView = SearchController.SearchBar;
        }

        void InitializeHandlers()
        {
            if (ComposeDocumentItem != null)
                ComposeDocumentItem.Clicked += ComposeDocumentItem_Clicked;

            if (EditModeItem != null)
                EditModeItem.Clicked += EditModeItem_Clicked;

            RefreshControl.ValueChanged += RefreshControl_ValueChanged;
        }

        void DeinitializeHandlers()
        {
            if (ComposeDocumentItem != null)
                ComposeDocumentItem.Clicked -= ComposeDocumentItem_Clicked;

            if (EditModeItem != null)
                EditModeItem.Clicked -= EditModeItem_Clicked;

            RefreshControl.ValueChanged -= RefreshControl_ValueChanged;
        }

        #endregion

        #region NavigationBar handlers

        void ComposeDocumentItem_Clicked(object sender, EventArgs e)
        {
            // TODO
        }

        async void EditModeItem_Clicked(object sender, EventArgs e)
        {
            EditModeItem.Clicked -= EditModeItem_Clicked;

            if (FoldersTableView.Editing)
            {
                EditModeItem.Title = Localization.GetString("edit");
                FoldersTableView.SetEditing(false, true);

                try
                {
                    var gds = FoldersTableView.Source as GrouppedDataSource;
                    await Managers.FoldersManager.SetFavoriteFoldersAsync(Folder.Module, gds.GetFavoriteFolders());
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error("Could not save favorite folders order", ex);

                    await Dialogs.ShowErrorDialogAsync(this, ex);
                }
            }
            else
            {
                EditModeItem.Title = Localization.GetString("done");
                FoldersTableView.SetEditing(true, true);
            }

            EditModeItem.Clicked += EditModeItem_Clicked;
        }

        #endregion

        #region Refreshing

        void RefreshControl_ValueChanged(object sender, EventArgs e) => RefreshData(true);

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void RefreshData(bool forceRefresh = false)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            RefreshControl.ValueChanged -= RefreshControl_ValueChanged;

            try
            {
                if (IsRootOfFoldersList)
                {
                    var ds = FoldersTableView.Source as GrouppedDataSource;

                    var favorites = await Managers.FoldersManager.GetFavoriteFoldersAsync(Folder.Module);

                    List<Folder> folders;
                    if (!forceRefresh && Folder.HasSubFolders && Folder.SubFolders != null && Folder.SubFolders.Count > 0)
                    {
                        folders = await Managers.FoldersManager.GetFoldersAsync(Folder, 3, SourceType.Local);
                    }
                    else
                    {
                        folders = await Managers.FoldersManager.GetFoldersAsync(Folder, 3);
                    }

                    var favoriteIds = favorites.Select(f => f.Id);
                    var folderIds = folders.Select(f => f.Id);
                    var allIds = favoriteIds.Union(folderIds).Distinct();

                    var favoritesStatus = new Dictionary<int, bool>();
                    var offlineStatus = new Dictionary<int, bool>();

                    foreach (var id in allIds)
                    {
                        favoritesStatus[id] = await Managers.FoldersManager.IsFolderFavouriteAsync(Folder.Module, id);
                        offlineStatus[id] = await Managers.FoldersManager.IsFolderOfflineAsync(Folder.Module, id);
                    }

                    ds.FavoriteStatus = favoritesStatus;
                    ds.CachingStatus = offlineStatus;

                    ds.SetFolders(GrouppedDataSource.Section.Favorites, favorites);
                    ds.SetFolders(GrouppedDataSource.Section.Local, Folder.LocalRootForModule(ModuleType.Documents).SubFolders);
                    ds.SetFolders(GrouppedDataSource.Section.Folders, folders);
                }
                else
                {
                    var ds = FoldersTableView.Source as DataSource;

                    List<Folder> folders;
                    if (!forceRefresh && Folder.HasSubFolders && Folder.SubFolders != null && Folder.SubFolders.Count > 0)
                    {
                        folders = Folder.SubFolders;
                    }
                    else
                    {
                        folders = await Managers.FoldersManager.GetFoldersAsync(Folder);
                    }

                    var folderIds = folders.Select(f => f.Id).Distinct();

                    var favoritesStatus = new Dictionary<int, bool>();
                    var offlineStatus = new Dictionary<int, bool>();

                    foreach (var id in folderIds)
                    {
                        favoritesStatus[id] = await Managers.FoldersManager.IsFolderFavouriteAsync(Folder.Module, id);
                        offlineStatus[id] = await Managers.FoldersManager.IsFolderOfflineAsync(Folder.Module, id);
                    }

                    ds.FavoriteStatus = favoritesStatus;
                    ds.CachingStatus = offlineStatus;

                    ds.SetFolders(folders);
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Could not load folders", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);
            }

            RefreshControl.EndRefreshing();
            RefreshControl.ValueChanged += RefreshControl_ValueChanged;
        }

        #endregion

        #region FoldersList handlers

        protected virtual void FolderSelected(Folder folder)
        {
        }

        protected virtual void FolderDeselected(Folder folder)
        {
        }

        protected virtual void FolderExpand(Folder folder)
        {
        }

        #endregion

        #region Action handlers

        public async void AddToFavorites(Folder folder)
        {
            await Managers.FoldersManager.AddFavoriteFolderAsync(folder.Module, folder);

            var gds = FoldersTableView.Source as GrouppedDataSource;
            if (gds != null)
            {
                gds.FavoriteStatus[folder.Id] = true;

                var favorites = await Managers.FoldersManager.GetFavoriteFoldersAsync(folder.Module);
                gds.SetFolders(GrouppedDataSource.Section.Favorites, favorites);

                var indexPaths = gds.GetIndexPaths(folder.Id);
                FoldersTableView.ReloadRows(indexPaths, UITableViewRowAnimation.Automatic);
            }

            var ds = FoldersTableView.Source as DataSource;
            if (ds != null)
            {
                ds.FavoriteStatus[folder.Id] = true;

                var indexPaths = ds.GetIndexPaths(folder.Id);
                FoldersTableView.ReloadRows(indexPaths, UITableViewRowAnimation.Automatic);
            }
        }

        public async void RemoveFromFavorites(Folder folder)
        {
            await Managers.FoldersManager.RemoveFavoriteFolderAsync(folder.Module, folder);

            var gds = FoldersTableView.Source as GrouppedDataSource;
            if (gds != null)
            {
                gds.FavoriteStatus[folder.Id] = false;

                var favorites = await Managers.FoldersManager.GetFavoriteFoldersAsync(folder.Module);
                gds.SetFolders(GrouppedDataSource.Section.Favorites, favorites);

                var indexPaths = gds.GetIndexPaths(folder.Id);
                FoldersTableView.ReloadRows(indexPaths, UITableViewRowAnimation.Automatic);
            }

            var ds = FoldersTableView.Source as DataSource;
            if (ds != null)
            {
                ds.FavoriteStatus[folder.Id] = false;

                var indexPaths = ds.GetIndexPaths(folder.Id);
                FoldersTableView.ReloadRows(indexPaths, UITableViewRowAnimation.Automatic);
            }
        }

        public async void EnableNotifications(Folder folder)
        {
            await Managers.NotificationsManager.SetFoldersNotificationsAsync(DeviceType.Android, PlatformConfig.Preferences.PushNotificationToken, folder.Module, new List<Folder>{ folder }, true);

            var gds = FoldersTableView.Source as GrouppedDataSource;
            if (gds != null)
            {
                var folders = gds.GetFolders(folder.Id);
                folders.ForEach(f => f.Subscribed = true);
                var indexPaths = gds.GetIndexPaths(folder.Id);
                FoldersTableView.ReloadRows(indexPaths, UITableViewRowAnimation.Automatic);
            }

            var ds = FoldersTableView.Source as DataSource;
            if (ds != null)
            {
                var folders = ds.GetFolders(folder.Id);
                folders.ForEach(f => f.Subscribed = true);
                var indexPaths = ds.GetIndexPaths(folder.Id);
                FoldersTableView.ReloadRows(indexPaths, UITableViewRowAnimation.Automatic);
            }
        }

        public async void DisableNotifications(Folder folder)
        {
            await Managers.NotificationsManager.SetFoldersNotificationsAsync(DeviceType.Android, PlatformConfig.Preferences.PushNotificationToken, folder.Module, new List<Folder> { folder }, false);

            var gds = FoldersTableView.Source as GrouppedDataSource;
            if (gds != null)
            {
                var folders = gds.GetFolders(folder.Id);
                folders.ForEach(f => f.Subscribed = false);
                var indexPaths = gds.GetIndexPaths(folder.Id);
                FoldersTableView.ReloadRows(indexPaths, UITableViewRowAnimation.Automatic);
            }

            var ds = FoldersTableView.Source as DataSource;
            if (ds != null)
            {
                var folders = ds.GetFolders(folder.Id);
                folders.ForEach(f => f.Subscribed = false);
                var indexPaths = ds.GetIndexPaths(folder.Id);
                FoldersTableView.ReloadRows(indexPaths, UITableViewRowAnimation.Automatic);
            }
        }

        public async void EnableCaching(Folder folder)
        {
            await Managers.FoldersManager.AddOfflineFolderAsync(folder.Module, folder);

            var gds = FoldersTableView.Source as GrouppedDataSource;
            if (gds != null)
            {
                gds.CachingStatus[folder.Id] = true;

                var indexPaths = gds.GetIndexPaths(folder.Id);
                FoldersTableView.ReloadRows(indexPaths, UITableViewRowAnimation.Automatic);
            }

            var ds = FoldersTableView.Source as DataSource;
            if (ds != null)
            {
                ds.CachingStatus[folder.Id] = true;

                var indexPaths = ds.GetIndexPaths(folder.Id);
                FoldersTableView.ReloadRows(indexPaths, UITableViewRowAnimation.Automatic);
            }
        }

        public async void DisableCaching(Folder folder)
        {
            await Managers.FoldersManager.RemoveOfflineFolderAsync(folder.Module, folder);

            var gds = FoldersTableView.Source as GrouppedDataSource;
            if (gds != null)
            {
                gds.CachingStatus[folder.Id] = false;

                var indexPaths = gds.GetIndexPaths(folder.Id);
                FoldersTableView.ReloadRows(indexPaths, UITableViewRowAnimation.Automatic);
            }

            var ds = FoldersTableView.Source as DataSource;
            if (ds != null)
            {
                ds.CachingStatus[folder.Id] = false;

                var indexPaths = ds.GetIndexPaths(folder.Id);
                FoldersTableView.ReloadRows(indexPaths, UITableViewRowAnimation.Automatic);
            }
        }

        #endregion

        #region Searching

        void IUISearchResultsUpdating.UpdateSearchResultsForSearchController(UISearchController searchController)
        {
            var searchText = searchController.SearchBar.Text;

            if (!searchController.Active || string.IsNullOrWhiteSpace(searchText))
            {
                searchCancellationTokenSourceList.ForEach(cts => cts?.Cancel());
                searchCancellationTokenSourceList.Clear();
                SearchResultsDataSource.Reset();
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

                DoSearchFolders(searchText, searchCancellationTokenSource.Token);
            }
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void DoSearchFolders(string searchText, CancellationToken cancellationToken)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            try
            {
                await Task.Delay(500);

                if (cancellationToken.IsCancellationRequested) return;

                var root = Folder.RootForModule(Folder.Module);
                var flattenedFolders = root.SubFolders
                                             .Flatten(f => f.SubFolders)
                                             .Where(f => f.Name.IndexOf(searchText, StringComparison.CurrentCultureIgnoreCase) >= 0)
                                             .OrderBy(f => f.Name)
                                             .ToList();

                if (cancellationToken.IsCancellationRequested) return;

                SearchResultsDataSource.SetFolders(flattenedFolders);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error(ex);
            }
        }

        #endregion

        #region DataSources

        protected class DataSource : UITableViewSource, IDisposable
        {

            public bool Empty
            {
                get
                {
                    return foldersInView.Count < 1;
                }
            }

            public Dictionary<int, bool> FavoriteStatus { get; set; }
            public Dictionary<int, bool> CachingStatus { get; set; }

            AbstractFoldersListViewController viewController;
            UITableView tableView;
            readonly bool disableRowActions;

            bool loading;
            List<Folder> foldersInView;

            public DataSource(AbstractFoldersListViewController viewController, UITableView tableView, bool disableRowActions)
            {
                this.viewController = viewController;
                this.tableView = tableView;
                this.disableRowActions = disableRowActions;

                loading = true;
                foldersInView = new List<Folder>();
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                {
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as WaitTableViewCell ?? WaitTableViewCell.Create();
                }

                if (foldersInView.Count < 1)
                {
                    var emptyCell = tableView.DequeueReusableCell(WaitTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                    emptyCell.Initialize(Localization.GetString("no_folders_in_section"));
                    return emptyCell;
                }

                var cell = tableView.DequeueReusableCell(FoldersTableViewCell.Key) as FoldersTableViewCell;
                if (cell == null)
                {
                    cell = FoldersTableViewCell.Create();
                    cell.ExpandCollapseClicked += Cell_ExpandCollapseClicked;
                }

                var f = foldersInView[indexPath.Row];
                var folderIsCached = false;
                CachingStatus.TryGetValue(f.Id, out folderIsCached);

                cell.Initialize(f, false, folderIsCached);

                return cell;
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return 44f;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading)
                {
                    return 1;
                }

                if (foldersInView.Count < 1)
                {
                    return 1;
                }

                return foldersInView.Count;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var f = foldersInView[indexPath.Row];
                viewController.FolderSelected(f);
            }

            public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
            {
                var f = foldersInView[indexPath.Row];
                viewController.FolderDeselected(f);
            }

            void Cell_ExpandCollapseClicked(object sender, Folder f)
            {
                viewController.FolderExpand(f);
            }

            public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
            {
                if (disableRowActions) return new UITableViewRowAction[0];

                var f = foldersInView[indexPath.Row];

                var actions = new List<UITableViewRowAction>();

                if (FavoriteStatus[f.Id])
                {
                    var action = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("remove_from_favorites"), (a, ip) => { viewController.RemoveFromFavorites(foldersInView[ip.Row]); tableView.SetEditing(false, true); });
                    action.BackgroundColor = Theme.Brown;
                    actions.Add(action);
                }
                else
                {
                    var action = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("add_to_favorites"), (a, ip) => { viewController.AddToFavorites(foldersInView[ip.Row]); tableView.SetEditing(false, true); });
                    action.BackgroundColor = Theme.Brown;
                    actions.Add(action);
                }

                if (CachingStatus[f.Id])
                {
                    var action = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("disable_caching"), (a, ip) => { viewController.DisableCaching(foldersInView[ip.Row]); tableView.SetEditing(false, true); });
                    action.BackgroundColor = Theme.DarkBlue;
                    actions.Add(action);
                }
                else
                {
                    var action = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("enable_caching"), (a, ip) => { viewController.EnableCaching(foldersInView[ip.Row]); tableView.SetEditing(false, true); });
                    action.BackgroundColor = Theme.DarkBlue;
                    actions.Add(action);
                }

                if (f.Subscribed)
                {
                    var action = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("disable_notifications"), (a, ip) => { viewController.DisableNotifications(foldersInView[ip.Row]); tableView.SetEditing(false, true); });
                    action.BackgroundColor = Theme.Blue;
                    actions.Add(action);
                }
                else
                {
                    var action = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("enable_notifications"), (a, ip) => { viewController.EnableNotifications(foldersInView[ip.Row]); tableView.SetEditing(false, true); });
                    action.BackgroundColor = Theme.Blue;
                    actions.Add(action);
                }

                return actions.ToArray();
            }

            protected override void Dispose(bool disposing)
            {
                viewController = null;
                tableView = null;

                loading = true;
                foldersInView = null;
                FavoriteStatus = null;
                CachingStatus = null;
            }

            public void SetFolders(List<Folder> folders)
            {
                foldersInView.Clear();
                foldersInView.AddRange(folders);
                loading = false;
                tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Automatic);
            }

            public void Reset()
            {
                loading = true;

                foldersInView.Clear();
                
                tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Automatic);
            }

            public Folder[] GetFolders(int folderId)
            {
                var folders = new List<Folder>();
                for (int i = 0; i < foldersInView.Count; i++)
                {
                    var f = foldersInView[i];
                    if (f.Id == folderId)
                    {
                        folders.Add(f);
                    }
                }

                return folders.ToArray();
            }

            public NSIndexPath[] GetIndexPaths(int folderId)
            {
                var indexPaths = new List<NSIndexPath>();
                for (int i = 0; i < foldersInView.Count; i++)
                {
                    var f = foldersInView[i];
                    if (f.Id == folderId)
                    {
                        indexPaths.Add(NSIndexPath.FromRowSection(i, 0));
                    }
                }

                return indexPaths.ToArray();
            }
        }

        protected class GrouppedDataSource : UITableViewSource, IDisposable
        {

            public static class Section
            {
                public static readonly nint Favorites = 0;
                public static readonly nint Folders = 1;
                public static readonly nint Local = 2;
            }

            public bool Empty
            {
                get
                {
                    return foldersInView.All(kv => kv.Value.Count < 1);
                }
            }

            public Dictionary<int, bool> FavoriteStatus { get; set; }
            public Dictionary<int, bool> CachingStatus { get; set; }

            AbstractFoldersListViewController viewController;
            UITableView tableView;
            readonly bool disableRowActions;

            bool[] loading;
            Dictionary<nint, List<Folder>> foldersInView;

            public GrouppedDataSource(AbstractFoldersListViewController viewController, UITableView tableView, ModuleType module, bool disableRowActions)
            {
                this.viewController = viewController;
                this.tableView = tableView;
                this.disableRowActions = disableRowActions;

                if (module == ModuleType.Documents)
                {
                    loading = new[] { true, true, true };
                    foldersInView = new Dictionary<nint, List<Folder>>
                    {
                        [Section.Favorites] = new List<Folder>(),
                        [Section.Folders] = new List<Folder>(),
                        [Section.Local] = new List<Folder>()
                    };

                    return;
                }

                if (module == ModuleType.Contacts || module == ModuleType.Shortcodes)
                {
                    loading = new[] { true, true };
                    foldersInView = new Dictionary<nint, List<Folder>>
                    {
                        [Section.Favorites] = new List<Folder>(),
                        [Section.Folders] = new List<Folder>()
                    };

                    return;
                }

                throw new ArgumentException(nameof(module));
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading[indexPath.LongSection])
                {
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as WaitTableViewCell ?? WaitTableViewCell.Create();
                }

                if (foldersInView[indexPath.LongSection].Count < 1)
                {
                    var emptyCell = tableView.DequeueReusableCell(WaitTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();

                    if (indexPath.LongSection == Section.Favorites)
                    {
                        emptyCell.Initialize(Localization.GetString("no_folders_in_favorites"));
                    }
                    else
                    {
                        emptyCell.Initialize(Localization.GetString("no_folders_in_section"));
                    }

                    return emptyCell;
                }

                var cell = tableView.DequeueReusableCell(FoldersTableViewCell.Key) as FoldersTableViewCell;
                if (cell == null)
                {
                    cell = FoldersTableViewCell.Create();
                    cell.ExpandCollapseClicked += Cell_ExpandCollapseClicked;
                }

                var f = foldersInView[indexPath.LongSection][indexPath.Row];
                var sectionIsFavorites = Section.Favorites == indexPath.LongSection;
                var folderIsCached = false;
                CachingStatus.TryGetValue(f.Id, out folderIsCached);

                cell.Initialize(f, sectionIsFavorites, folderIsCached);

                return cell;
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return 44f;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading[section])
                {
                    return 1;
                }

                if (foldersInView[section].Count < 1)
                {
                    return 1;
                }

                return foldersInView[section].Count;
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                return foldersInView.Keys.Count;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var f = foldersInView[indexPath.LongSection][indexPath.Row];
                viewController.FolderSelected(f);
            }

            public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
            {
                var f = foldersInView[indexPath.LongSection][indexPath.Row];
                viewController.FolderDeselected(f);
            }

            void Cell_ExpandCollapseClicked(object sender, Folder f)
            {
                viewController.FolderExpand(f);
            }

            public override string TitleForHeader(UITableView tableView, nint section)
            {
                if (section == Section.Favorites)
                    return Localization.GetString("favorites").ToUpper(CultureInfo.CurrentCulture);

                if (section == Section.Folders)
                    return Localization.GetString("folders").ToUpper(CultureInfo.CurrentCulture);

                if (section == Section.Local)
                    return Localization.GetString("local_folders").ToUpper(CultureInfo.CurrentCulture);

                throw new ArgumentException(nameof(section));
            }

            public override bool ShouldIndentWhileEditing(UITableView tableView, NSIndexPath indexPath)
            {
                return false;
            }

            public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
            {
                if (tableView.Editing)
                {
                    return indexPath.LongSection == Section.Favorites;
                }
                else
                {
                    return indexPath.LongSection != Section.Local;
                }
            }
            
            public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
            {
                if (disableRowActions) return new UITableViewRowAction[0];

                var f = foldersInView[indexPath.LongSection][indexPath.Row];

                var actions = new List<UITableViewRowAction>();

                if (indexPath.Section == Section.Favorites || indexPath.Section == Section.Folders)
                {
                    if (FavoriteStatus[f.Id])
                    {
                        var action = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("remove_from_favorites"), (a, ip) => { viewController.RemoveFromFavorites(foldersInView[ip.LongSection][ip.Row]); tableView.SetEditing(false, true); });
                        action.BackgroundColor = Theme.Brown;
                        actions.Add(action);
                    }
                    else
                    {
                        var action = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("add_to_favorites"), (a, ip) => { viewController.AddToFavorites(foldersInView[ip.LongSection][ip.Row]); tableView.SetEditing(false, true); });
                        action.BackgroundColor = Theme.Brown;
                        actions.Add(action);
                    }

                    if (CachingStatus[f.Id])
                    {
                        var action = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("disable_caching"), (a, ip) => { viewController.DisableCaching(foldersInView[ip.LongSection][ip.Row]); tableView.SetEditing(false, true); });
                        action.BackgroundColor = Theme.DarkBlue;
                        actions.Add(action);
                    }
                    else
                    {
                        var action = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("enable_caching"), (a, ip) => { viewController.EnableCaching(foldersInView[ip.LongSection][ip.Row]); tableView.SetEditing(false, true); });
                        action.BackgroundColor = Theme.DarkBlue;
                        actions.Add(action);
                    }

                    if (f.Subscribed)
                    {
                        var action = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("disable_notifications"), (a, ip) => { viewController.DisableNotifications(foldersInView[ip.LongSection][ip.Row]); tableView.SetEditing(false, true); });
                        action.BackgroundColor = Theme.Blue;
                        actions.Add(action);
                    }
                    else
                    {
                        var action = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("enable_notifications"), (a, ip) => { viewController.EnableNotifications(foldersInView[ip.LongSection][ip.Row]); tableView.SetEditing(false, true); });
                        action.BackgroundColor = Theme.Blue;
                        actions.Add(action);
                    }
                }

                return actions.ToArray();
            }

            public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return tableView.Editing ? UITableViewCellEditingStyle.None : UITableViewCellEditingStyle.Delete;
            }

            public override bool CanMoveRow(UITableView tableView, NSIndexPath indexPath)
            {
                return indexPath.Section == Section.Favorites;
            }

            public override NSIndexPath CustomizeMoveTarget(UITableView tableView, NSIndexPath sourceIndexPath, NSIndexPath proposedIndexPath)
            {
                return proposedIndexPath.Section == Section.Favorites ? proposedIndexPath : sourceIndexPath;
            }

            public override void MoveRow(UITableView tableView, NSIndexPath sourceIndexPath, NSIndexPath destinationIndexPath)
            {
                var oldRow = sourceIndexPath.Row;
                var newRow = destinationIndexPath.Row;

                var rowToMove = foldersInView[Section.Favorites][oldRow];
                foldersInView[Section.Favorites].RemoveAt(oldRow);
                foldersInView[Section.Favorites].Insert(newRow, rowToMove);
            }

            protected override void Dispose(bool disposing)
            {
                viewController = null;
                tableView = null;

                loading = null;
                foldersInView = null;
                FavoriteStatus = null;
                CachingStatus = null;
            }

            public void SetFolders(nint section, List<Folder> folders)
            {
                foldersInView[section].Clear();
                foldersInView[section].AddRange(folders);
                loading[section] = false;
                tableView.ReloadSections(NSIndexSet.FromIndex(section), UITableViewRowAnimation.Automatic);
            }

            public void Reset()
            {
                for (var i = 0; i < loading.Length; i++)
                    loading[i] = true;

                foreach (var kv in foldersInView)
                    kv.Value.Clear();

                tableView.ReloadSections(NSIndexSet.FromIndex(Section.Favorites), UITableViewRowAnimation.Automatic);
                tableView.ReloadSections(NSIndexSet.FromIndex(Section.Folders), UITableViewRowAnimation.Automatic);
                tableView.ReloadSections(NSIndexSet.FromIndex(Section.Local), UITableViewRowAnimation.Automatic);
            }

            public Folder[] GetFolders(int folderId)
            {
                var folders = new List<Folder>();
                foreach (var folderInView in foldersInView)
                {
                    for (int i = 0; i < folderInView.Value.Count; i++)
                    {
                        var f = folderInView.Value[i];
                        if (f.Id == folderId)
                        {
                            folders.Add(f);
                        }
                    }
                }
                return folders.ToArray();
            }

            public List<Folder> GetFavoriteFolders()
            {
                return foldersInView[Section.Favorites].Select(f => f.ShallowCopy()).ToList();
            }

            public NSIndexPath[] GetIndexPaths(int folderId)
            {
                var indexPaths = new List<NSIndexPath>();
                foreach (var folderInView in foldersInView)
                {
                    for (int i = 0; i < folderInView.Value.Count; i++)
                    {
                        var f = folderInView.Value[i];
                        if (f.Id == folderId)
                        {
                            indexPaths.Add(NSIndexPath.FromRowSection(i, folderInView.Key));
                        }
                    }
                }
                return indexPaths.ToArray();
            }
        }

        protected class SearchDataSource : UITableViewSource, IDisposable
        {

            public bool Empty
            {
                get
                {
                    return foldersInView.Count < 1;
                }
            }

            AbstractFoldersListViewController viewController;
            UITableView tableView;

            bool loading;
            List<Folder> foldersInView;

            public SearchDataSource(AbstractFoldersListViewController viewController, UITableView tableView)
            {
                this.tableView = tableView;
                this.viewController = viewController;

                loading = true;
                foldersInView = new List<Folder>();
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                {
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as WaitTableViewCell ?? WaitTableViewCell.Create();
                }

                if (foldersInView.Count < 1)
                {
                    var emptyCell = tableView.DequeueReusableCell(WaitTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                    emptyCell.Initialize(Localization.GetString("no_folders_found"));
                    return emptyCell;
                }

                var cell = tableView.DequeueReusableCell(FoldersSearchResultsTableViewCell.Key) as FoldersSearchResultsTableViewCell ?? FoldersSearchResultsTableViewCell.Create();

                cell.Initialize(foldersInView[indexPath.Row]);

                return cell;
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return 44f;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading)
                {
                    return 1;
                }

                if (foldersInView.Count < 1)
                {
                    return 1;
                }

                return foldersInView.Count;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var f = foldersInView[indexPath.Row];
                viewController.FolderSelected(f);
            }

            public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
            {
                var f = foldersInView[indexPath.Row];
                viewController.FolderDeselected(f);
            }

            protected override void Dispose(bool disposing)
            {
                viewController = null;
                tableView = null;

                loading = true;
                foldersInView = null;
            }

            public void SetFolders(List<Folder> folders)
            {
                foldersInView.Clear();
                foldersInView.AddRange(folders);
                loading = false;
                tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Automatic);
            }

            public void Reset()
            {
                loading = true;
                foldersInView.Clear();
                tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Automatic);
            }
        }

    #endregion

    }
}
