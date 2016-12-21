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

        protected bool isRootOfFoldersList;
        protected Folder folder { get; private set; }

        protected UIBarButtonItem EditModeItem;
        protected UIBarButtonItem ComposeDocumentItem;

        protected UIRefreshControl RefreshControl;
        protected UITableView FoldersListView;
        protected UISearchController SearchController;
        protected UITableViewController SearchResultsController;
        protected SearchDataSource SearchResultsDataSource;

        protected CancellationTokenSource searchCancellationTokenSource;
        protected readonly List<CancellationTokenSource> searchCancellationTokenSourceList = new List<CancellationTokenSource>();

        protected AbstractFoldersListViewController(ModuleType module)
        {
            isRootOfFoldersList = true;
            folder = Folder.RootForModule(module);
        }

        protected AbstractFoldersListViewController(Folder folder)
        {
            isRootOfFoldersList = false;
            this.folder = folder;
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
            var ds = FoldersListView?.DataSource as GrouppedDataSource;
            ds?.Reset();

            base.DidReceiveMemoryWarning();
        }

        #endregion

        #region Initialize/deinitialize

        void InitializeNavigationBarTitle()
        {
            if (isRootOfFoldersList)
            {
                NavigationItem.Title = Localization.GetString("documents");
            }
            else
            {
                NavigationItem.Title = folder.Name;
                NavigationItem.Prompt = Localization.GetString("documents");
            }
        }

        void ClearNavigationBarTitle()
        {
            if (isRootOfFoldersList)
            {
                NavigationItem.Title = Localization.GetString("back");
            }
        }

        void InitializeNavigationBar()
        {
            if (folder.Module == ModuleType.Documents)
            {
                ComposeDocumentItem = new UIBarButtonItem();
                ComposeDocumentItem.Image = UIImage.FromBundle(Path.Combine("icons", "compose.png"));
                NavigationItem.SetRightBarButtonItem(ComposeDocumentItem, false);

                if (isRootOfFoldersList)
                {
                    EditModeItem = new UIBarButtonItem();
                    EditModeItem.Title = Localization.GetString("edit");
                    NavigationItem.SetLeftBarButtonItem(EditModeItem, false);
                }

                return;
            }

            if (folder.Module == ModuleType.Contacts || folder.Module == ModuleType.Shortcodes)
            {
                if (isRootOfFoldersList)
                {
                    EditModeItem = new UIBarButtonItem();
                    EditModeItem.Title = Localization.GetString("edit");
                    NavigationItem.SetLeftBarButtonItem(EditModeItem, false);
                }

                return;
            }

            throw new ArgumentException(nameof(folder.Module));
        }

        void InitializeView()
        {
            AutomaticallyAdjustsScrollViewInsets = true;

            FoldersListView = new UITableView(CGRect.Empty, UITableViewStyle.Grouped);
            FoldersListView.ClipsToBounds = false;
            if (isRootOfFoldersList)
                FoldersListView.Source = new GrouppedDataSource(this, FoldersListView, folder.Module);
            else
                FoldersListView.Source = new DataSource(this, FoldersListView);
            FoldersListView.AllowsSelectionDuringEditing = false;
            FoldersListView.TranslatesAutoresizingMaskIntoConstraints = false;
            View.AddSubview(FoldersListView);
            View.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(FoldersListView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(FoldersListView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(FoldersListView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(FoldersListView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1.0f, 0.0f)
                });

            RefreshControl = new UIRefreshControl();
            RefreshControl.BackgroundColor = UIColor.White;
            RefreshControl.AttributedTitle = Localization.GetNSAttributedString("pull_to_refresh");
            FoldersListView.AddSubview(RefreshControl);
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

            FoldersListView.TableHeaderView = SearchController.SearchBar;
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

        void EditModeItem_Clicked(object sender, EventArgs e)
        {
            // TODO
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
                if (isRootOfFoldersList)
                {
                    var ds = FoldersListView.Source as GrouppedDataSource;

                    var favorites = await Managers.FoldersManager.GetFavoriteFoldersAsync(folder.Module);

                    List<Folder> folders;
                    if (!forceRefresh && folder.HasSubFolders && folder.SubFolders != null && folder.SubFolders.Count > 0)
                    {
                        folders = folder.SubFolders;
                    }
                    else
                    {
                        folders = await Managers.FoldersManager.GetFoldersAsync(folder, 3);
                    }

                    var favoriteIds = favorites.Select(f => f.Id);
                    var folderIds = folders.Select(f => f.Id);
                    var allIds = favoriteIds.Union(folderIds).Distinct();

                    var favoritesStatus = new Dictionary<int, bool>();
                    var offlineStatus = new Dictionary<int, bool>();

                    foreach (var id in allIds)
                    {
                        favoritesStatus[id] = await Managers.FoldersManager.IsFolderFavouriteAsync(folder.Module, id);
                        offlineStatus[id] = await Managers.FoldersManager.IsFolderOfflineAsync(folder.Module, id);
                    }

                    ds.FavoriteStatus = favoritesStatus;
                    ds.OfflineStatus = offlineStatus;

                    ds.SetFolders(GrouppedDataSource.Section.Favorites, favorites);
                    ds.SetFolders(GrouppedDataSource.Section.Local, Folder.LocalRootForModule(ModuleType.Documents).SubFolders);
                    ds.SetFolders(GrouppedDataSource.Section.Folders, folders);
                }
                else
                {
                    var ds = FoldersListView.Source as DataSource;

                    List<Folder> folders;
                    if (!forceRefresh && folder.HasSubFolders && folder.SubFolders != null && folder.SubFolders.Count > 0)
                    {
                        folders = folder.SubFolders;
                    }
                    else
                    {
                        folders = await Managers.FoldersManager.GetFoldersAsync(folder);
                    }

                    var folderIds = folders.Select(f => f.Id).Distinct();

                    var favoritesStatus = new Dictionary<int, bool>();
                    var offlineStatus = new Dictionary<int, bool>();

                    foreach (var id in folderIds)
                    {
                        favoritesStatus[id] = await Managers.FoldersManager.IsFolderFavouriteAsync(folder.Module, id);
                        offlineStatus[id] = await Managers.FoldersManager.IsFolderOfflineAsync(folder.Module, id);
                    }

                    ds.FavoriteStatus = favoritesStatus;
                    ds.OfflineStatus = offlineStatus;

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

                var root = Folder.RootForModule(folder.Module);
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
            public Dictionary<int, bool> OfflineStatus { get; set; }

            AbstractFoldersListViewController viewController;
            UITableView tableView;

            bool loading;
            List<Folder> foldersInView;

            public DataSource(AbstractFoldersListViewController viewController, UITableView tableView)
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
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                }

                var cell = tableView.DequeueReusableCell(FoldersListViewCell.Key) as FoldersListViewCell;
                if (cell == null)
                {
                    cell = FoldersListViewCell.Create();
                    cell.ExpandCollapseClicked += Cell_ExpandCollapseClicked;
                }

                var f = foldersInView[indexPath.Row];
                var isFavorite = false;
                FavoriteStatus.TryGetValue(f.Id, out isFavorite);
                var isOffline = false;
                OfflineStatus.TryGetValue(f.Id, out isOffline);

                cell.Initialize(f, isFavorite, isOffline);

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
            public Dictionary<int, bool> OfflineStatus { get; set; }

            AbstractFoldersListViewController viewController;
            UITableView tableView;

            bool[] loading;
            Dictionary<nint, List<Folder>> foldersInView;

            public GrouppedDataSource(AbstractFoldersListViewController viewController, UITableView tableView, ModuleType module)
            {
                this.tableView = tableView;
                this.viewController = viewController;

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
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                }

                var cell = tableView.DequeueReusableCell(FoldersListViewCell.Key) as FoldersListViewCell;
                if (cell == null)
                {
                    cell = FoldersListViewCell.Create();
                    cell.ExpandCollapseClicked += Cell_ExpandCollapseClicked;
                }

                var f = foldersInView[indexPath.LongSection][indexPath.Row];
                var isFavorite = false;
                FavoriteStatus.TryGetValue(f.Id, out isFavorite);
                var isOffline = false;
                OfflineStatus.TryGetValue(f.Id, out isOffline);

                cell.Initialize(f, isFavorite, isOffline);

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

            public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
            {
                var actions = new List<UITableViewRowAction>();

                if (indexPath.Section == Section.Favorites)
                {
                    var rff = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("remove_from_favorites"), (arg1, arg2) => { });
                    rff.BackgroundColor = Theme.Brown;
                    actions.Add(rff);
                }

                if (indexPath.Section == Section.Folders)
                {
                    var rff = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("remove_from_favorites"), (arg1, arg2) => { });
                    rff.BackgroundColor = Theme.Brown;
                    actions.Add(rff);

                    var ec = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("enable_caching"), (arg1, arg2) => { });
                    ec.BackgroundColor = Theme.DarkBlue;
                    actions.Add(ec);

                    var n = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("notify"), (arg1, arg2) => { });
                    n.BackgroundColor = Theme.Blue;
                    actions.Add(n);
                }

                return actions.ToArray();
            }

            protected override void Dispose(bool disposing)
            {
                viewController = null;
                tableView = null;

                loading = null;
                foldersInView = null;
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
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                }

                var cell = tableView.DequeueReusableCell(SearchFoldersTableViewCell.Key) as SearchFoldersTableViewCell ?? SearchFoldersTableViewCell.Create();

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
