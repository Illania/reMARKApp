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
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentView;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList
{

    public abstract class AbstractFoldersListViewController : AbstractViewController, IPrimaryViewController, IUISearchResultsUpdating
    {

        protected readonly Folder ParentFolder;
        protected readonly bool IsRootOfFoldersList;
        protected readonly bool DisableRowActions;
        protected readonly bool DisableNavigationBarActions;
        protected readonly bool DisableSearch;

        protected UIBarButtonItem EditModeItem;
        protected UIBarButtonItem ComposeDocumentItem;

        protected UIRefreshControl RefreshControl;
        protected UITableView TableView;
        protected UISearchController SearchController;
        protected UITableViewController SearchResultsController;
        protected SearchDataSource SearchResultsDataSource;

        protected CancellationTokenSource searchCancellationTokenSource;
        protected readonly List<CancellationTokenSource> searchCancellationTokenSourceList = new List<CancellationTokenSource>();

        protected AbstractFoldersListViewController(ModuleType module, bool disableRowActions, bool disableNavigationBarActions, bool disableSearch)
        {
            IsRootOfFoldersList = true;
            ParentFolder = Folder.RootForModule(module);
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
            ParentFolder = folder;
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

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ExtendedLayoutIncludesOpaqueBars = true;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            InitializeNavigationBarTitle();
            InitializeHandlers();

            if (TableView?.IndexPathForSelectedRow != null)
                TableView.DeselectRow(TableView.IndexPathForSelectedRow, true);

            if (TableView?.IndexPathsForSelectedRows?.Length > 0)
                foreach (var selectedIndexPath in TableView?.IndexPathsForSelectedRows)
                    TableView.DeselectRow(selectedIndexPath, true);

            ReachabilityBar.Attach(View, TableView, (float)NavigationController.BottomLayoutGuide.Length, UITextAlignment.Left);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info($"{nameof(AbstractFoldersListViewController)} appeared");

            if (((TableView?.Source as GrouppedDataSource)?.Empty ?? false) || ((TableView?.Source as DataSource)?.Empty ?? false))
                RefreshData();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            ClearNavigationBarTitle();
            DeinitializeHandlers();

            if (NavigationController != null && NavigationController.NavigationBarHidden)
                NavigationController?.SetNavigationBarHidden(false, true);

            if (SearchController != null && SearchController.Active)
                SearchController.Active = false;
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning($"{nameof(AbstractFoldersListViewController)} received memory warning!");

            var ds = TableView?.Source as DataSource;
            ds?.Reset();

            var gds = TableView?.Source as GrouppedDataSource;
            gds?.Reset();

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        #endregion

        #region Initialize/deinitialize

        protected virtual void InitializeNavigationBarTitle()
        {
            Func<string> getTitle = () =>
            {
                switch (ParentFolder.Module)
                {
                    case ModuleType.Documents:
                        return Localization.GetString("documents");
                    case ModuleType.Contacts:
                        return Localization.GetString("contacts");
                    case ModuleType.Shortcodes:
                        return Localization.GetString("shortcodes");
                    default:
                        return " ";
                }
            };

            UIView.AnimationsEnabled = false;
            if (IsRootOfFoldersList)
            {
                NavigationItem.Title = getTitle();
                NavigationItem.Prompt = " ";
            }
            else
            {
                NavigationItem.Title = ParentFolder.Name;
                NavigationItem.Prompt = getTitle();
            }
            UIView.AnimationsEnabled = true;
        }

        protected virtual void ClearNavigationBarTitle()
        {
            if (IsRootOfFoldersList)
            {
                NavigationItem.Title = Localization.GetString("back");
            }
        }

        protected virtual void InitializeNavigationBar()
        {
            if (DisableNavigationBarActions) return;

            if (ParentFolder.Module == ModuleType.Documents)
            {
                ComposeDocumentItem = new UIBarButtonItem();
                ComposeDocumentItem.Image = UIImage.FromBundle(Path.Combine("icons", "compose.png"));
                NavigationItem.SetRightBarButtonItem(ComposeDocumentItem, false);

                if (IsRootOfFoldersList)
                {
                    EditModeItem = new UIBarButtonItem();
                    EditModeItem.Title = Localization.GetString("edit");
                    EditModeItem.Enabled = false;
                    NavigationItem.SetLeftBarButtonItem(EditModeItem, false);
                }
            }

            if (ParentFolder.Module == ModuleType.Contacts || ParentFolder.Module == ModuleType.Shortcodes || ParentFolder.Module == ModuleType.Calendar)
            {
                if (IsRootOfFoldersList)
                {
                    EditModeItem = new UIBarButtonItem();
                    EditModeItem.Title = Localization.GetString("edit");
                    EditModeItem.Enabled = false;
                    NavigationItem.SetLeftBarButtonItem(EditModeItem, false);
                }
            }
        }

        protected virtual void InitializeView()
        {
            AutomaticallyAdjustsScrollViewInsets = true;

            TableView = new UITableView(CGRect.Empty, UITableViewStyle.Grouped);
            TableView.ClipsToBounds = false;
            if (IsRootOfFoldersList)
                TableView.Source = new GrouppedDataSource(this, TableView, ParentFolder.Module, DisableRowActions);
            else
                TableView.Source = new DataSource(this, TableView, ParentFolder.Module, DisableRowActions);
            TableView.AllowsSelectionDuringEditing = false;
            TableView.TranslatesAutoresizingMaskIntoConstraints = false;
            View.AddSubview(TableView);
            View.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(TableView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1f, 0f),
                    NSLayoutConstraint.Create(TableView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1f, 0f),
                    NSLayoutConstraint.Create(TableView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1f, 0f),
                    NSLayoutConstraint.Create(TableView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1f, 0f)
                });

            RefreshControl = new UIRefreshControl();
            RefreshControl.BackgroundColor = UIColor.White;
            TableView.AddSubview(RefreshControl);
        }

        protected virtual void InitializeSearchBar()
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

            TableView.TableHeaderView = SearchController.SearchBar;
        }

        protected virtual void InitializeHandlers()
        {
            if (ComposeDocumentItem != null)
                ComposeDocumentItem.Clicked += ComposeDocumentItem_Clicked;

            if (EditModeItem != null)
                EditModeItem.Clicked += EditModeItem_Clicked;

            RefreshControl.ValueChanged += RefreshControl_ValueChanged;
        }

        protected virtual void DeinitializeHandlers()
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
            var vc = new ComposeDocumentViewController
            {
                CreationModeFlag = DocumentCreationModeFlag.New,
                PreviousDocumentDirection = DocumentDirection.None
            };

            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void EditModeItem_Clicked(object sender, EventArgs e)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            EditModeItem.Clicked -= EditModeItem_Clicked;

            if (TableView.Editing)
            {
                EditModeItem.Title = Localization.GetString("edit");
                TableView.SetEditing(false, true);

                try
                {
                    var gds = (GrouppedDataSource)TableView.Source;
                    await Managers.FoldersManager.SetFavoriteFoldersAsync(ParentFolder.Module, gds.GetFavoriteFolders());
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error("Could not save favorite folders order", ex);

                    await Dialogs.ShowErrorDialogAsync(this, ex);
                }

                SearchController.SearchBar.UserInteractionEnabled = true;
                SearchController.SearchBar.Alpha = 1f;
            }
            else
            {
                EditModeItem.Title = Localization.GetString("done");
                TableView.SetEditing(true, true);

                SearchController.SearchBar.UserInteractionEnabled = false;
                SearchController.SearchBar.Alpha = .5f;
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

            CommonConfig.Logger.Info($"Refreshing folders list [parentFolder={ParentFolder}]");

            try
            {
                if (IsRootOfFoldersList)
                {
                    var gds = (GrouppedDataSource)TableView.Source;

                    var favorites = await Managers.FoldersManager.GetFavoriteFoldersAsync(ParentFolder.Module);

                    List<Folder> folders;
                    if (!forceRefresh && ParentFolder.HasSubFolders && ParentFolder.SubFolders != null && ParentFolder.SubFolders.Count > 0)
                    {
                        folders = await Managers.FoldersManager.GetFoldersAsync(ParentFolder, 3, SourceType.Local);
                    }
                    else
                    {
                        folders = await Managers.FoldersManager.GetFoldersAsync(ParentFolder, 3);
                    }

                    var favoriteIds = favorites.Select(f => f.Id);
                    var folderIds = folders.Select(f => f.Id);
                    var allIds = favoriteIds.Union(folderIds).Distinct();

                    var favoritesStatus = new Dictionary<int, bool>();
                    var offlineStatus = new Dictionary<int, bool>();

                    foreach (var id in allIds)
                    {
                        favoritesStatus[id] = await Managers.FoldersManager.IsFolderFavouriteAsync(ParentFolder.Module, id);
                        offlineStatus[id] = await Managers.FoldersManager.IsFolderOfflineAsync(ParentFolder.Module, id);
                    }

                    gds.FavoriteStatus = favoritesStatus;
                    gds.CachingStatus = offlineStatus;

                    if (ParentFolder.Module == ModuleType.Documents)
                    {
                        gds.SetFolders(GrouppedDataSource.Section.Local, Folder.LocalRootForModule(ModuleType.Documents).SubFolders);
                    }
                    gds.SetFolders(GrouppedDataSource.Section.Favorites, favorites);
                    gds.SetFolders(GrouppedDataSource.Section.Folders, folders);

                    if (EditModeItem != null)
                        EditModeItem.Enabled = gds.ItemsInSection(GrouppedDataSource.Section.Favorites) > 0;
                }
                else
                {
                    var ds = (DataSource)TableView.Source;

                    List<Folder> folders;
                    if (!forceRefresh && ParentFolder.HasSubFolders && ParentFolder.SubFolders != null && ParentFolder.SubFolders.Count > 0)
                    {
                        folders = ParentFolder.SubFolders;
                    }
                    else
                    {
                        folders = await Managers.FoldersManager.GetFoldersAsync(ParentFolder);
                    }

                    var folderIds = folders.Select(f => f.Id).Distinct();

                    var favoritesStatus = new Dictionary<int, bool>();
                    var offlineStatus = new Dictionary<int, bool>();

                    foreach (var id in folderIds)
                    {
                        favoritesStatus[id] = await Managers.FoldersManager.IsFolderFavouriteAsync(ParentFolder.Module, id);
                        offlineStatus[id] = await Managers.FoldersManager.IsFolderOfflineAsync(ParentFolder.Module, id);
                    }

                    ds.FavoriteStatus = favoritesStatus;
                    ds.CachingStatus = offlineStatus;

                    ds.SetFolders(folders);

                    CommonConfig.Logger.Info($"Refreshed folders list [parentFolder={ParentFolder}]");
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Could not refresh folders [parentFolder={ParentFolder}]", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);

                if (!IsRootOfFoldersList)
                    NavigationController?.PopViewController(true);
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

        protected virtual bool ShouldDisableFolder(Folder folder)
        {
            return false;
        }

        #endregion

        #region Row actions handlers

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public async void AddToFavorites(Folder folder)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            try
            {
                await Managers.FoldersManager.AddFavoriteFolderAsync(folder.Module, folder);

                var gds = TableView.Source as GrouppedDataSource;
                if (gds != null)
                {
                    gds.FavoriteStatus[folder.Id] = true;

                    var favorites = await Managers.FoldersManager.GetFavoriteFoldersAsync(folder.Module);
                    gds.SetFolders(GrouppedDataSource.Section.Favorites, favorites);

                    var indexPaths = gds.GetIndexPaths(folder.Id);
                    TableView.ReloadRows(indexPaths, UITableViewRowAnimation.Fade);

                    if (EditModeItem != null)
                        EditModeItem.Enabled = gds.ItemsInSection(GrouppedDataSource.Section.Favorites) > 0;
                }

                var ds = TableView.Source as DataSource;
                if (ds != null)
                {
                    ds.FavoriteStatus[folder.Id] = true;

                    var indexPaths = ds.GetIndexPaths(folder.Id);
                    TableView.ReloadRows(indexPaths, UITableViewRowAnimation.Fade);
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Could not add folder to favorites", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public async void RemoveFromFavorites(Folder folder)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            try
            {
                await Managers.FoldersManager.RemoveFavoriteFolderAsync(folder.Module, folder);

                var gds = TableView.Source as GrouppedDataSource;
                if (gds != null)
                {
                    gds.FavoriteStatus[folder.Id] = false;

                    var favorites = await Managers.FoldersManager.GetFavoriteFoldersAsync(folder.Module);
                    gds.SetFolders(GrouppedDataSource.Section.Favorites, favorites);

                    var indexPaths = gds.GetIndexPaths(folder.Id);
                    TableView.ReloadRows(indexPaths, UITableViewRowAnimation.Fade);

                    if (EditModeItem != null)
                        EditModeItem.Enabled = gds.ItemsInSection(GrouppedDataSource.Section.Favorites) > 0;
                }

                var ds = TableView.Source as DataSource;
                if (ds != null)
                {
                    ds.FavoriteStatus[folder.Id] = false;

                    var indexPaths = ds.GetIndexPaths(folder.Id);
                    TableView.ReloadRows(indexPaths, UITableViewRowAnimation.Fade);
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Could not remote folder from favorites", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public async void EnableNotifications(Folder folder)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            try
            {
                await Managers.NotificationsManager.SetFoldersNotificationsAsync(DeviceType.Android, PlatformConfig.Preferences.PushNotificationToken, folder.Module, new List<Folder> { folder }, true);

                var gds = TableView.Source as GrouppedDataSource;
                if (gds != null)
                {
                    var folders = gds.GetFolders(folder.Id);
                    folders.ForEach(f => f.Subscribed = true);
                    var indexPaths = gds.GetIndexPaths(folder.Id);
                    TableView.ReloadRows(indexPaths, UITableViewRowAnimation.Fade);
                }

                var ds = TableView.Source as DataSource;
                if (ds != null)
                {
                    var folders = ds.GetFolders(folder.Id);
                    folders.ForEach(f => f.Subscribed = true);
                    var indexPaths = ds.GetIndexPaths(folder.Id);
                    TableView.ReloadRows(indexPaths, UITableViewRowAnimation.Fade);
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Could not enable notifications for folder", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public async void DisableNotifications(Folder folder)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            try
            {
                await Managers.NotificationsManager.SetFoldersNotificationsAsync(DeviceType.Android, PlatformConfig.Preferences.PushNotificationToken, folder.Module, new List<Folder> { folder }, false);

                var gds = TableView.Source as GrouppedDataSource;
                if (gds != null)
                {
                    var folders = gds.GetFolders(folder.Id);
                    folders.ForEach(f => f.Subscribed = false);
                    var indexPaths = gds.GetIndexPaths(folder.Id);
                    TableView.ReloadRows(indexPaths, UITableViewRowAnimation.Fade);
                }

                var ds = TableView.Source as DataSource;
                if (ds != null)
                {
                    var folders = ds.GetFolders(folder.Id);
                    folders.ForEach(f => f.Subscribed = false);
                    var indexPaths = ds.GetIndexPaths(folder.Id);
                    TableView.ReloadRows(indexPaths, UITableViewRowAnimation.Fade);
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Could not disable notifications for folder", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public async void EnableCaching(Folder folder)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            try
            {
                await Managers.FoldersManager.AddOfflineFolderAsync(folder.Module, folder);

                var gds = TableView.Source as GrouppedDataSource;
                if (gds != null)
                {
                    gds.CachingStatus[folder.Id] = true;

                    var indexPaths = gds.GetIndexPaths(folder.Id);
                    TableView.ReloadRows(indexPaths, UITableViewRowAnimation.Fade);
                }

                var ds = TableView.Source as DataSource;
                if (ds != null)
                {
                    ds.CachingStatus[folder.Id] = true;

                    var indexPaths = ds.GetIndexPaths(folder.Id);
                    TableView.ReloadRows(indexPaths, UITableViewRowAnimation.Fade);
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Could not enabled caching for folder", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public async void DisableCaching(Folder folder)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            try
            {
                await Managers.FoldersManager.RemoveOfflineFolderAsync(folder.Module, folder);

                var gds = TableView.Source as GrouppedDataSource;
                if (gds != null)
                {
                    gds.CachingStatus[folder.Id] = false;

                    var indexPaths = gds.GetIndexPaths(folder.Id);
                    TableView.ReloadRows(indexPaths, UITableViewRowAnimation.Fade);
                }

                var ds = TableView.Source as DataSource;
                if (ds != null)
                {
                    ds.CachingStatus[folder.Id] = false;

                    var indexPaths = ds.GetIndexPaths(folder.Id);
                    TableView.ReloadRows(indexPaths, UITableViewRowAnimation.Fade);
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Could not disable caching for folder", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);
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

                var root = Folder.RootForModule(ParentFolder.Module);
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
            readonly ModuleType module;
            readonly bool disableRowActions;

            bool loading;
            List<Folder> foldersInView;

            public DataSource(AbstractFoldersListViewController viewController, UITableView tableView, ModuleType module, bool disableRowActions)
            {
                this.viewController = viewController;
                this.tableView = tableView;
                this.module = module;
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
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
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

                cell.Initialize(f, folderIsCached);
                if (viewController.ShouldDisableFolder(f)) cell.Disable();

                return cell;
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return FoldersTableViewCell.Height;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading)
                    return 1;

                if (foldersInView.Count < 1)
                    return 1;

                return foldersInView.Count;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var f = foldersInView[indexPath.Row];

                if (viewController.ShouldDisableFolder(f)) return;

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

                if (module == ModuleType.Documents)
                {
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
                        action.BackgroundColor = Theme.DarkerBlue;
                        actions.Add(action);
                    }
                    else
                    {
                        var action = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("enable_notifications"), (a, ip) => { viewController.EnableNotifications(foldersInView[ip.Row]); tableView.SetEditing(false, true); });
                        action.BackgroundColor = Theme.DarkerBlue;
                        actions.Add(action);
                    }
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
                tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            public void Reset()
            {
                loading = true;

                foldersInView.Clear();

                tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
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
            readonly ModuleType module;
            readonly bool disableRowActions;

            bool[] loading;
            Dictionary<nint, List<Folder>> foldersInView;

            public GrouppedDataSource(AbstractFoldersListViewController viewController, UITableView tableView, ModuleType module, bool disableRowActions)
            {
                this.viewController = viewController;
                this.tableView = tableView;
                this.module = module;
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
                }

                if (module == ModuleType.Contacts || module == ModuleType.Shortcodes || module == ModuleType.Calendar)
                {
                    loading = new[] { true, true };
                    foldersInView = new Dictionary<nint, List<Folder>>
                    {
                        [Section.Favorites] = new List<Folder>(),
                        [Section.Folders] = new List<Folder>()
                    };
                }
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
                        emptyCell.Initialize(Localization.GetString("no_folders_in_favorites"));
                    else
                        emptyCell.Initialize(Localization.GetString("no_folders_in_section"));

                    return emptyCell;
                }

                var cell = tableView.DequeueReusableCell(FoldersTableViewCell.Key) as FoldersTableViewCell;
                if (cell == null)
                {
                    cell = FoldersTableViewCell.Create();
                    cell.ExpandCollapseClicked += Cell_ExpandCollapseClicked;
                }

                var f = foldersInView[indexPath.LongSection][indexPath.Row];
                var folderIsCached = false;
                CachingStatus.TryGetValue(f.Id, out folderIsCached);

                cell.Initialize(f, folderIsCached);
                if (viewController.ShouldDisableFolder(f)) cell.Disable();

                return cell;
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return FoldersTableViewCell.Height;
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

                if (viewController.ShouldDisableFolder(f)) return;

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

                return string.Empty;
            }

            public override void WillDisplayHeaderView(UITableView tableView, UIView headerView, nint section)
            {
                var v = headerView as UITableViewHeaderFooterView;
                if (v == null)
                    return;

                v.TextLabel.TextColor = Theme.DarkerBlue;
            }

            public override bool ShouldIndentWhileEditing(UITableView tableView, NSIndexPath indexPath)
            {
                return false;
            }

            public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
            {
                return tableView.Editing ? (indexPath.LongSection == Section.Favorites && foldersInView[Section.Favorites].Count > 0) : indexPath.LongSection != Section.Local;
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

                    if (module == ModuleType.Documents)
                    {
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
                            action.BackgroundColor = Theme.DarkerBlue;
                            actions.Add(action);
                        }
                        else
                        {
                            var action = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("enable_notifications"), (a, ip) => { viewController.EnableNotifications(foldersInView[ip.LongSection][ip.Row]); tableView.SetEditing(false, true); });
                            action.BackgroundColor = Theme.DarkerBlue;
                            actions.Add(action);
                        }
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
                tableView.ReloadSections(NSIndexSet.FromIndex(section), UITableViewRowAnimation.Fade);
            }

            public void Reset()
            {
                for (var i = 0; i < loading.Length; i++)
                    loading[i] = true;

                foreach (var kv in foldersInView)
                    kv.Value.Clear();

                tableView.BeginUpdates();
                if (module == ModuleType.Documents)
                {
                    tableView.ReloadSections(NSIndexSet.FromIndex(Section.Local), UITableViewRowAnimation.Fade);
                }

                tableView.ReloadSections(NSIndexSet.FromIndex(Section.Favorites), UITableViewRowAnimation.Fade);
                tableView.ReloadSections(NSIndexSet.FromIndex(Section.Folders), UITableViewRowAnimation.Fade);
                tableView.EndUpdates();
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

            public int ItemsInSection(nint section)
            {
                return foldersInView[section].Count;
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

                var f = foldersInView[indexPath.Row];
                cell.Initialize(f);
                if (viewController.ShouldDisableFolder(f)) cell.Disable();

                return cell;
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return FoldersSearchResultsTableViewCell.Height;
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

                if (viewController.ShouldDisableFolder(f)) return;

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
                tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            public void Reset()
            {
                loading = true;
                foldersInView.Clear();
                tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }
        }

        #endregion

    }
}
