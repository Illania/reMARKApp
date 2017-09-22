using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.DataAccess.Exceptions;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentView;
using UIKit;
using Mark5.Mobile.Common.Utilities.Extensions;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList
{
    public abstract class AbstractFoldersListViewController : AbstractTableViewController, IPrimaryViewController, IUISearchResultsUpdating
    {
        protected virtual bool LoadRemoteFromCache { get; }

        protected readonly Folder ParentFolder;
        protected readonly bool IsRootOfFoldersList;
        protected readonly bool DisableRowActions;
        protected readonly bool DisableNavigationBarActions;
        protected readonly bool DisableSearch;

        protected UIBarButtonItem EditModeItem;
        protected UIBarButtonItem ComposeDocumentItem;
        protected UIBarButtonItem CreateContactItem;

        UISearchController searchController;
        CancellationTokenSource searchCancellationTokenSource;
        readonly List<CancellationTokenSource> searchCancellationTokenSourceList = new List<CancellationTokenSource>();

        protected AbstractFoldersListViewController(ModuleType module, bool disableRowActions, bool disableNavigationBarActions, bool disableSearch)
            : base(UITableViewStyle.Grouped)
        {
            IsRootOfFoldersList = true;
            ParentFolder = Folder.RootForModule(module);
            DisableRowActions = disableRowActions;
            DisableNavigationBarActions = disableNavigationBarActions;
            DisableSearch = disableSearch;
        }

        /// <summary>
        ///     This constructor MUST NOT be public!
        /// </summary>
        protected AbstractFoldersListViewController(Folder folder, bool disableRowActions, bool disableNavigationBarActions, bool disableSearch)
            : base(UITableViewStyle.Plain)
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

            NavigationController.NavigationBar.PrefersLargeTitles = true;
            NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Automatic;
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

            ReachabilityBar.Attach(View, TableView, (float)NavigationController.BottomLayoutGuide.Length, UITextAlignment.Left);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info($"{nameof(AbstractFoldersListViewController)} appeared");

            if (((TableView?.Source as GrouppedDataSource)?.Empty ?? false)
                || ((TableView?.Source as DataSource)?.Empty ?? false))
                RefreshData();
            else if (TableView?.Source as GrouppedDataSource != null)
                QuickRefreshData();

            NSOperationQueue.MainQueue.AddOperation(() =>
            {
                var ni = NavigationItem;

                if (ParentViewController != null && ParentViewController is UIViewController && !(ParentViewController is UINavigationController))
                    ni = ParentViewController?.NavigationItem;

                if (ni.SearchController == null)
                    ni.SearchController = searchController;
            });
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeinitializeHandlers();

            if (NavigationController != null && NavigationController.NavigationBarHidden)
                NavigationController.SetNavigationBarHidden(false, true);

            if (searchController != null && searchController.Active)
                searchController.Active = false;
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

        public override void Recycle()
        {
            base.Recycle();

            (TableView.Source as DataSource)?.Reset();
            (TableView.Source as GrouppedDataSource)?.Reset();
            searchController = null;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (CommonConfig.Logger.IsDebugEnabled())
                CommonConfig.Logger.Debug("Disposed");
        }

        #endregion

        #region Initialize/deinitialize

        protected virtual void InitializeNavigationBar()
        {
            if (IsRootOfFoldersList)
                switch (ParentFolder.Module)
                {
                    case ModuleType.Documents:
                        NavigationItem.Title = Localization.GetString("documents");
                        break;
                    case ModuleType.Contacts:
                        NavigationItem.Title = Localization.GetString("contacts");
                        break;
                    case ModuleType.Shortcodes:
                        NavigationItem.Title = Localization.GetString("shortcodes");
                        break;
                    default:
                        NavigationItem.Title = " ";
                        break;
                }
            else
                NavigationItem.Title = ParentFolder.Name;

            if (DisableNavigationBarActions)
                return;

            if (ParentFolder.Module == ModuleType.Documents)
            {
                ComposeDocumentItem = new UIBarButtonItem
                {
                    Image = UIImage.FromBundle(Path.Combine("icons", "compose.png"))
                };
                NavigationItem.SetRightBarButtonItem(ComposeDocumentItem, false);

                if (IsRootOfFoldersList)
                {
                    EditModeItem = new UIBarButtonItem
                    {
                        Title = Localization.GetString("edit"),
                        Enabled = false
                    };
                    NavigationItem.SetLeftBarButtonItem(EditModeItem, false);
                }
            }

            if (ParentFolder.Module == ModuleType.Contacts && ServerConfig.SystemSettings.ContactsModuleInfo.Permissions.CreateAllowed)
            {
                CreateContactItem = new UIBarButtonItem
                {
                    Image = UIImage.FromBundle(Path.Combine("icons", "add_contact.png"))
                };
                NavigationItem.SetRightBarButtonItem(CreateContactItem, false);
            }

            if (ParentFolder.Module == ModuleType.Contacts || ParentFolder.Module == ModuleType.Shortcodes)
            {
                if (IsRootOfFoldersList)
                {
                    EditModeItem = new UIBarButtonItem
                    {
                        Title = Localization.GetString("edit"),
                        Enabled = false
                    };
                    NavigationItem.SetLeftBarButtonItem(EditModeItem, false);
                }
            }
        }

        protected virtual void InitializeView()
        {
            RefreshControl = new UIRefreshControl();

            TableView.Source = IsRootOfFoldersList
                ? new GrouppedDataSource(this, TableView, ParentFolder.Module, DisableRowActions) as UITableViewSource
                : new DataSource(this, TableView, ParentFolder.Module, DisableRowActions) as UITableViewSource;
            TableView.RefreshControl = RefreshControl;
        }

        protected virtual void InitializeSearchBar()
        {
            DefinesPresentationContext = true;

            var searchResultsController = new UITableViewController();
            var searchResultsDataSource = new SearchDataSource(this, searchResultsController.TableView);
            searchResultsController.TableView.Source = searchResultsDataSource;

            searchController = new UISearchController(searchResultsController)
            {
                HidesNavigationBarDuringPresentation = true,
                DimsBackgroundDuringPresentation = true,
                ObscuresBackgroundDuringPresentation = true,
                SearchResultsUpdater = this
            };
            searchController.SearchBar.Placeholder = Localization.GetString("filter");
        }

        protected virtual void InitializeHandlers()
        {
            if (ComposeDocumentItem != null)
                ComposeDocumentItem.Clicked += ComposeDocumentItem_Clicked;

            if (EditModeItem != null)
                EditModeItem.Clicked += EditModeItem_Clicked;

            if (CreateContactItem != null)
                CreateContactItem.Clicked += CreateContactItem_Clicked;

            RefreshControl.ValueChanged += RefreshControl_ValueChanged;
        }

        protected virtual void DeinitializeHandlers()
        {
            if (ComposeDocumentItem != null)
                ComposeDocumentItem.Clicked -= ComposeDocumentItem_Clicked;

            if (EditModeItem != null)
                EditModeItem.Clicked -= EditModeItem_Clicked;

            if (CreateContactItem != null)
                CreateContactItem.Clicked -= CreateContactItem_Clicked;

            RefreshControl.ValueChanged -= RefreshControl_ValueChanged;
        }

        #endregion

        #region NavigationBar handlers

        void ComposeDocumentItem_Clicked(object sender, EventArgs e)
        {
            var vc = new ComposeDocumentViewController
            {
                DocumentCreationModeFlag = DocumentCreationModeFlag.New
            };

            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        async void CreateContactItem_Clicked(object sender, EventArgs e)
        {
            var choices = new[] { Localization.GetString("company"), Localization.GetString("department"), Localization.GetString("person") };
            var choice = await Dialogs.ShowListDialogAsync(this, Localization.GetString("add_contact"), choices, CreateContactItem);

            if (choice >= 0)
            {
                ContactType type = ContactType.None;
                switch (choice)
                {
                    case 0:
                        type = ContactType.Company;
                        break;
                    case 1:
                        type = ContactType.Department;
                        break;
                    case 2:
                        type = ContactType.Person;
                        break;
                }

                var vc = new AddEditContactViewController
                {
                    CreationModeFlag = ContactCreationModeFlag.New,
                    ContactType = type,
                };

                PresentViewController(new NavigationController(vc), true, null);
            }
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void EditModeItem_Clicked(object sender, EventArgs e)
        {
            EditModeItem.Clicked -= EditModeItem_Clicked;

            if (TableView.Editing)
            {
                EditModeItem.Title = Localization.GetString("edit");
                TableView.SetEditing(false, true);

                try
                {
                    var gds = (GrouppedDataSource)TableView.Source;
                    await Managers.FoldersManager.SetFavoriteFoldersAsync(ParentFolder.Module, gds.GetFolders(GrouppedDataSource.Section.Favorites));
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error("Could not save favorite folders order", ex);

                    await Dialogs.ShowErrorDialogAsync(this, ex);
                }

                if (searchController != null)
                {
                    searchController.SearchBar.UserInteractionEnabled = true;
                    searchController.SearchBar.Alpha = 1f;
                }
            }
            else
            {
                EditModeItem.Title = Localization.GetString("done");
                TableView.SetEditing(true, true);

                if (searchController != null)
                {
                    searchController.SearchBar.UserInteractionEnabled = false;
                    searchController.SearchBar.Alpha = .5f;
                }
            }

            EditModeItem.Clicked += EditModeItem_Clicked;
        }

        #endregion

        #region Refreshing

        void RefreshControl_ValueChanged(object sender, EventArgs e) => RefreshData(true);

        async void RefreshData(bool forceRefresh = false)
        {
            RefreshControl.ValueChanged -= RefreshControl_ValueChanged;

            CommonConfig.Logger.Info($"Refreshing folders list [parentFolder={ParentFolder}]");

            try
            {
                List<Folder> remoteFolders = null;
                if (!forceRefresh && ParentFolder.HasSubFolders && ParentFolder.SubFolders != null && ParentFolder.SubFolders.Count > 0)
                    remoteFolders = ParentFolder.SubFolders;
                else
                {
                    if (LoadRemoteFromCache)
                        try
                        {
                            remoteFolders = await Managers.FoldersManager.GetFoldersAsync(ParentFolder, sourceType: SourceType.Local);
                        }
                        catch (DataNotFoundException)
                        {
                            //Do nothing
                        }
                        catch (Exception ex)
                        {
                            CommonConfig.Logger.Error($"Could not retrieve folders from cache only [parentFolder={ParentFolder}]", ex);
                        }

                    if (remoteFolders == null)
                        remoteFolders = await Managers.FoldersManager.GetFoldersAsync(ParentFolder);
                }

                if (IsRootOfFoldersList)
                {
                    var gds = (GrouppedDataSource)TableView.Source;

                    var favorites = await Managers.FoldersManager.GetFavoriteFoldersAsync(ParentFolder.Module);

                    if (ParentFolder.Module == ModuleType.Documents)
                        gds.SetFolders(GrouppedDataSource.Section.Local, Folder.LocalRootForModule(ModuleType.Documents).SubFolders);

                    gds.SetFolders(GrouppedDataSource.Section.Favorites, favorites);
                    gds.SetFolders(GrouppedDataSource.Section.Folders, remoteFolders);

                    if (EditModeItem != null)
                        EditModeItem.Enabled = gds.ItemsInSection(GrouppedDataSource.Section.Favorites) > 0;
                }
                else
                {
                    var ds = (DataSource)TableView.Source;
                    ds.SetFolders(remoteFolders);

                    CommonConfig.Logger.Info($"Refreshed folders list [parentFolder={ParentFolder}]");
                }

                await Task.Delay(150); // Let animations finish
                RefreshFoldersInfo();
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

        async void QuickRefreshData()
        {
            try
            {
                var gds = (GrouppedDataSource)TableView.Source;
                var currentFavorites = gds.GetFolders(GrouppedDataSource.Section.Favorites);
                var favorites = await Managers.FoldersManager.GetFavoriteFoldersAsync(ParentFolder.Module);

                var favortiesSame = currentFavorites.Count == favorites.Count
                                                    && currentFavorites.All(f => favorites.Any(f2 => f.Id == f2.Id));

                if (!favortiesSame)
                    gds.SetFolders(GrouppedDataSource.Section.Favorites, favorites);

                await Task.Delay(150); // Let animations finish
                RefreshFoldersInfo();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Could not refresh favorites [parentFolder={ParentFolder}]", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);

                if (!IsRootOfFoldersList)
                    NavigationController?.PopViewController(true);
            }
        }

        async void RefreshFoldersInfo()
        {
            if (IsRootOfFoldersList)
            {
                var gds = (GrouppedDataSource)TableView.Source;
                var ids = gds.FoldersInViewIds;

                var favoritesStatus = new SortedDictionary<int, bool>();
                var syncStatus = new SortedDictionary<int, bool>();

                foreach (var id in ids)
                {
                    favoritesStatus[id] = await Managers.FoldersManager.IsFolderFavouriteAsync(ParentFolder.Module, id);
                    syncStatus[id] = await Managers.FoldersManager.IsSavedFolderOfflineInfo(ParentFolder.Module, id);
                }

                if (!gds.FavoriteStatus.SequenceEqual(favoritesStatus) || !gds.SyncStatus.SequenceEqual(syncStatus))
                {
                    gds.FavoriteStatus = favoritesStatus;
                    gds.SyncStatus = syncStatus;
                    gds.Reload();
                }
            }
            else
            {
                var ds = (DataSource)TableView.Source;
                var ids = ds.FoldersInViewIds;

                var favoritesStatus = new SortedDictionary<int, bool>();
                var syncStatus = new SortedDictionary<int, bool>();

                foreach (var id in ids)
                {
                    favoritesStatus[id] = await Managers.FoldersManager.IsFolderFavouriteAsync(ParentFolder.Module, id);
                    syncStatus[id] = await Managers.FoldersManager.IsSavedFolderOfflineInfo(ParentFolder.Module, id);
                }

                if (!ds.FavoriteStatus.SequenceEqual(favoritesStatus) || !ds.SyncStatus.SequenceEqual(syncStatus))
                {
                    ds.FavoriteStatus = favoritesStatus;
                    ds.SyncStatus = syncStatus;
                    ds.Reload();
                }
            }
        }

        #endregion

        #region List handlers

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

        #region Actions

        public async void AddToFavorites(Folder folder)
        {
            try
            {
                await Managers.FoldersManager.AddFavoriteFolderAsync(folder.Module, folder);

                if (TableView.Source is GrouppedDataSource gds)
                {
                    gds.FavoriteStatus[folder.Id] = true;

                    var favorites = await Managers.FoldersManager.GetFavoriteFoldersAsync(folder.Module);
                    gds.SetFolders(GrouppedDataSource.Section.Favorites, favorites);

                    var indexPaths = gds.GetIndexPaths(folder.Id);
                    TableView.ReloadRows(indexPaths, UITableViewRowAnimation.Fade);

                    if (EditModeItem != null)
                        EditModeItem.Enabled = gds.ItemsInSection(GrouppedDataSource.Section.Favorites) > 0;
                }

                if (TableView.Source is DataSource ds)
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

        public async void RemoveFromFavorites(Folder folder)
        {
            try
            {
                await Managers.FoldersManager.RemoveFavoriteFolderAsync(folder.Module, folder);

                if (TableView.Source is GrouppedDataSource gds)
                {
                    gds.FavoriteStatus[folder.Id] = false;

                    var favorites = await Managers.FoldersManager.GetFavoriteFoldersAsync(folder.Module);
                    gds.SetFolders(GrouppedDataSource.Section.Favorites, favorites);

                    var indexPaths = gds.GetIndexPaths(folder.Id);
                    TableView.ReloadRows(indexPaths, UITableViewRowAnimation.Fade);

                    if (EditModeItem != null)
                        EditModeItem.Enabled = gds.ItemsInSection(GrouppedDataSource.Section.Favorites) > 0;
                }

                if (TableView.Source is DataSource ds)
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

        public async void EnableNotifications(Folder folder)
        {
            try
            {
                await Managers.NotificationsManager.SetFoldersNotificationsAsync(DeviceType.IOS,
                    PlatformConfig.Preferences.PushNotificationToken,
                    folder.Module,
                    new List<Folder>
                    {
                        folder
                    },
                    true);

                if (TableView.Source is GrouppedDataSource gds)
                {
                    var folders = gds.GetFolders(folder.Id);
                    folders.ForEach(f => f.Subscribed = true);
                    var indexPaths = gds.GetIndexPaths(folder.Id);
                    TableView.ReloadRows(indexPaths, UITableViewRowAnimation.Fade);
                }

                if (TableView.Source is DataSource ds)
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

        public async void DisableNotifications(Folder folder)
        {
            try
            {
                await Managers.NotificationsManager.SetFoldersNotificationsAsync(DeviceType.IOS,
                    PlatformConfig.Preferences.PushNotificationToken,
                    folder.Module,
                    new List<Folder> { folder },
                    false);

                if (TableView.Source is GrouppedDataSource gds)
                {
                    var folders = gds.GetFolders(folder.Id);
                    folders.ForEach(f => f.Subscribed = false);
                    var indexPaths = gds.GetIndexPaths(folder.Id);
                    TableView.ReloadRows(indexPaths, UITableViewRowAnimation.Fade);
                }

                if (TableView.Source is DataSource ds)
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

        public async void EnableSync(Folder folder)
        {
            try
            {
                await Managers.FoldersManager.AddSavedFolderInfo(folder);

                if (TableView.Source is GrouppedDataSource gds)
                {
                    gds.SyncStatus[folder.Id] = true;

                    var indexPaths = gds.GetIndexPaths(folder.Id);
                    TableView.ReloadRows(indexPaths, UITableViewRowAnimation.Fade);
                }

                if (TableView.Source is DataSource ds)
                {
                    ds.SyncStatus[folder.Id] = true;

                    var indexPaths = ds.GetIndexPaths(folder.Id);
                    TableView.ReloadRows(indexPaths, UITableViewRowAnimation.Fade);
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Could not enabled sync for folder", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        public async void DisableSync(Folder folder)
        {
            try
            {
                await Managers.FoldersManager.RemoveSavedFolderInfo(folder);

                if (TableView.Source is GrouppedDataSource gds)
                {
                    gds.SyncStatus[folder.Id] = false;

                    var indexPaths = gds.GetIndexPaths(folder.Id);
                    TableView.ReloadRows(indexPaths, UITableViewRowAnimation.Fade);
                }

                if (TableView.Source is DataSource ds)
                {
                    ds.SyncStatus[folder.Id] = false;

                    var indexPaths = ds.GetIndexPaths(folder.Id);
                    TableView.ReloadRows(indexPaths, UITableViewRowAnimation.Fade);
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Could not disable sync for folder", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        public void SaveOffline(Folder folder)
        {
            NavigationController.PresentViewController(new NavigationController(new DownloadViewController { Folder = folder.ShallowCopy() }, UIModalPresentationStyle.FormSheet), true, null);
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

                var dataSource = ((UITableViewController)searchController.SearchResultsController).TableView.Source;
                ((SearchDataSource)dataSource).Reset();
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

        async void DoSearchFolders(string searchText, CancellationToken cancellationToken)
        {
            try
            {
                var tableViewController = searchController?.SearchResultsController as UITableViewController;
                var dataSource = tableViewController?.TableView?.Source as SearchDataSource;
                dataSource?.Reset();

                await Task.Delay(500);

                if (cancellationToken.IsCancellationRequested)
                    return;

                var root = Folder.RootForModule(ParentFolder.Module);

                var resultList = new List<Folder>();
                await Task.Run(() => SearchRecursively(root, searchText, resultList, cancellationToken));

                if (cancellationToken.IsCancellationRequested)
                    return;

                dataSource?.SetFolders(resultList);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error(ex);
            }
        }

        void SearchRecursively(Folder folder, string searchText, List<Folder> resultList, CancellationToken ct)
        {
            if (folder.SubFolders == null || folder.SubFolders.Count < 1)
                return;

            foreach (var subFolder in folder.SubFolders)
            {
                if (ct.IsCancellationRequested)
                    return;

                if (subFolder.Name.IndexOf(searchText, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    resultList.Add(subFolder);

                SearchRecursively(subFolder, searchText, resultList, ct);
            }
        }

        #endregion

        #region DataSources

        protected class GrouppedDataSource : UITableViewSource
        {
            public static class Section
            {
                public static readonly nint Favorites = 0;
                public static readonly nint Folders = 1;
                public static readonly nint Local = 2;
            }

            public bool Empty => foldersInView.All(kv => kv.Value.Count < 1);
            public int[] FoldersInViewIds => foldersInView.SelectMany(kv => kv.Value).Select(f => f.Id).Distinct().ToArray();

            public SortedDictionary<int, bool> FavoriteStatus { get; set; } = new SortedDictionary<int, bool>();
            public SortedDictionary<int, bool> SyncStatus { get; set; } = new SortedDictionary<int, bool>();

            readonly WeakReference<AbstractFoldersListViewController> viewControllerWeakReference;
            readonly WeakReference<UITableView> tableViewWeakReference;
            readonly ModuleType module;
            readonly bool disableRowActions;

            bool[] loading;
            readonly Dictionary<nint, List<Folder>> foldersInView;

            public GrouppedDataSource(AbstractFoldersListViewController viewController, UITableView tableView, ModuleType module, bool disableRowActions)
            {
                viewControllerWeakReference = viewController.Wrap();
                tableViewWeakReference = tableView.Wrap();

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
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as WaitTableViewCell ?? WaitTableViewCell.Create();

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
                SyncStatus.TryGetValue(f.Id, out bool folderIsCached);

                cell.Initialize(f, folderIsCached);
                if (viewControllerWeakReference.Unwrap()?.ShouldDisableFolder(f) ?? false)
                    cell.Disable();

                return cell;
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath) => FoldersTableViewCell.Height;

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading[section])
                    return 1;

                if (foldersInView[section].Count < 1)
                    return 1;

                return foldersInView[section].Count;
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                return foldersInView.Keys.Count;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var f = foldersInView[indexPath.LongSection][indexPath.Row];

                if (viewControllerWeakReference.Unwrap()?.ShouldDisableFolder(f) ?? false)
                    return;

                viewControllerWeakReference.Unwrap()?.FolderSelected(f);
            }

            public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
            {
                var f = foldersInView[indexPath.LongSection][indexPath.Row];
                viewControllerWeakReference?.Unwrap().FolderDeselected(f);
            }

            void Cell_ExpandCollapseClicked(object sender, Folder f)
            {
                viewControllerWeakReference.Unwrap()?.FolderExpand(f);
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
                return tableView.Editing ? indexPath.LongSection == Section.Favorites && foldersInView[Section.Favorites].Count > 0 : indexPath.LongSection != Section.Local;
            }

            public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
            {
                if (disableRowActions)
                    return new UITableViewRowAction[0];

                if (indexPath.Section == Section.Local)
                    return new UITableViewRowAction[0];

                var f = foldersInView[indexPath.LongSection][indexPath.Row];
                var actions = new List<UITableViewRowAction>();

                if (FavoriteStatus.ContainsKey(f.Id))
                    if (FavoriteStatus[f.Id])
                    {
                        var action = UITableViewRowAction.Create(UITableViewRowActionStyle.Default,
                            Localization.GetString("remove_from_favorites"),
                            (a, ip) =>
                            {
                                viewControllerWeakReference.Unwrap()?.RemoveFromFavorites(foldersInView[ip.LongSection][ip.Row]);
                                tableView.SetEditing(false, true);
                            });
                        action.BackgroundColor = Theme.Brown;
                        actions.Add(action);
                    }
                    else
                    {
                        var action = UITableViewRowAction.Create(UITableViewRowActionStyle.Default,
                            Localization.GetString("add_to_favorites"),
                            (a, ip) =>
                            {
                                viewControllerWeakReference.Unwrap()?.AddToFavorites(foldersInView[ip.LongSection][ip.Row]);
                                tableView.SetEditing(false, true);
                            });
                        action.BackgroundColor = Theme.Brown;
                        actions.Add(action);
                    }

                if (module == ModuleType.Documents)
                {
                    if (SyncStatus.ContainsKey(f.Id))
                        if (SyncStatus[f.Id])
                        {
                            var action = UITableViewRowAction.Create(UITableViewRowActionStyle.Default,
                                Localization.GetString("disable_sync"),
                                (a, ip) =>
                                {
                                    viewControllerWeakReference.Unwrap()?.DisableSync(foldersInView[ip.LongSection][ip.Row]);
                                    tableView.SetEditing(false, true);
                                });
                            action.BackgroundColor = Theme.DarkBlue;
                            actions.Add(action);
                        }
                        else
                        {
                            var action = UITableViewRowAction.Create(UITableViewRowActionStyle.Default,
                                Localization.GetString("enable_sync"),
                                (a, ip) =>
                                {
                                    viewControllerWeakReference.Unwrap()?.EnableSync(foldersInView[ip.LongSection][ip.Row]);
                                    tableView.SetEditing(false, true);
                                });
                            action.BackgroundColor = Theme.DarkBlue;
                            actions.Add(action);
                        }

                    if (f.Subscribed)
                    {
                        var action = UITableViewRowAction.Create(UITableViewRowActionStyle.Default,
                            Localization.GetString("disable_notifications"),
                            (a, ip) =>
                            {
                                viewControllerWeakReference.Unwrap()?.DisableNotifications(foldersInView[ip.LongSection][ip.Row]);
                                tableView.SetEditing(false, true);
                            });
                        action.BackgroundColor = Theme.DarkerBlue;
                        actions.Add(action);
                    }
                    else
                    {
                        var action = UITableViewRowAction.Create(UITableViewRowActionStyle.Default,
                            Localization.GetString("enable_notifications"),
                            (a, ip) =>
                            {
                                viewControllerWeakReference.Unwrap()?.EnableNotifications(foldersInView[ip.LongSection][ip.Row]);
                                tableView.SetEditing(false, true);
                            });
                        action.BackgroundColor = Theme.DarkerBlue;
                        actions.Add(action);
                    }
                }

                if (module == ModuleType.Contacts || module == ModuleType.Shortcodes)
                {
                    var action = UITableViewRowAction.Create(UITableViewRowActionStyle.Default,
                        Localization.GetString("save_offline"),
                        (a, ip) =>
                        {
                            viewControllerWeakReference.Unwrap()?.SaveOffline(foldersInView[ip.LongSection][ip.Row]);
                            tableView.SetEditing(false, true);
                        });
                    action.BackgroundColor = Theme.DarkBlue;
                    actions.Add(action);
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

            public void SetFolders(nint section, List<Folder> folders)
            {
                foldersInView[section].Clear();
                foldersInView[section].AddRange(folders);
                loading[section] = false;
                tableViewWeakReference?.Unwrap().ReloadSections(NSIndexSet.FromIndex(section), UITableViewRowAnimation.Fade);
            }

            public List<Folder> GetFolders(nint section)
            {
                return foldersInView[section];
            }

            public void Reload()
            {
                tableViewWeakReference?.Unwrap().BeginUpdates();
                if (module == ModuleType.Documents)
                    tableViewWeakReference?.Unwrap().ReloadSections(NSIndexSet.FromIndex(Section.Local), UITableViewRowAnimation.None);

                tableViewWeakReference?.Unwrap().ReloadSections(NSIndexSet.FromIndex(Section.Favorites), UITableViewRowAnimation.None);
                tableViewWeakReference?.Unwrap().ReloadSections(NSIndexSet.FromIndex(Section.Folders), UITableViewRowAnimation.None);
                tableViewWeakReference?.Unwrap().EndUpdates();
            }

            public void Reset()
            {
                for (var i = 0; i < loading.Length; i++)
                    loading[i] = true;

                foreach (var kv in foldersInView)
                    kv.Value.Clear();

                tableViewWeakReference?.Unwrap().BeginUpdates();
                if (module == ModuleType.Documents)
                    tableViewWeakReference?.Unwrap().ReloadSections(NSIndexSet.FromIndex(Section.Local), UITableViewRowAnimation.Fade);

                tableViewWeakReference?.Unwrap().ReloadSections(NSIndexSet.FromIndex(Section.Favorites), UITableViewRowAnimation.Fade);
                tableViewWeakReference?.Unwrap().ReloadSections(NSIndexSet.FromIndex(Section.Folders), UITableViewRowAnimation.Fade);
                tableViewWeakReference?.Unwrap().EndUpdates();
            }

            public Folder[] GetFolders(int folderId)
            {
                var folders = new List<Folder>();
                foreach (var folderInView in foldersInView)
                    for (var i = 0; i < folderInView.Value.Count; i++)
                    {
                        var f = folderInView.Value[i];
                        if (f.Id == folderId)
                            folders.Add(f);
                    }

                return folders.ToArray();
            }

            public int ItemsInSection(nint section)
            {
                return foldersInView[section].Count;
            }

            public NSIndexPath[] GetIndexPaths(int folderId)
            {
                var indexPaths = new List<NSIndexPath>();
                foreach (var folderInView in foldersInView)
                    for (var i = 0; i < folderInView.Value.Count; i++)
                    {
                        var f = folderInView.Value[i];
                        if (f.Id == folderId)
                            indexPaths.Add(NSIndexPath.FromRowSection(i, folderInView.Key));
                    }

                return indexPaths.ToArray();
            }
        }

        protected class DataSource : UITableViewSource, IDisposable
        {
            public bool Empty => foldersInView.Count < 1;
            public int[] FoldersInViewIds => foldersInView.Select(f => f.Id).Distinct().ToArray();

            public SortedDictionary<int, bool> FavoriteStatus { get; set; } = new SortedDictionary<int, bool>();
            public SortedDictionary<int, bool> SyncStatus { get; set; } = new SortedDictionary<int, bool>();

            readonly WeakReference<AbstractFoldersListViewController> viewControllerWeakReference;
            readonly WeakReference<UITableView> tableViewWeakReference;
            readonly ModuleType module;
            readonly bool disableRowActions;

            bool loading = true;
            readonly List<Folder> foldersInView = new List<Folder>();

            public DataSource(AbstractFoldersListViewController viewController, UITableView tableView, ModuleType module, bool disableRowActions)
            {
                viewControllerWeakReference = viewController.Wrap();
                tableViewWeakReference = tableView.Wrap();
                this.module = module;
                this.disableRowActions = disableRowActions;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as WaitTableViewCell ?? WaitTableViewCell.Create();

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
                SyncStatus.TryGetValue(f.Id, out bool folderIsCached);

                cell.Initialize(f, folderIsCached);

                if (viewControllerWeakReference.Unwrap()?.ShouldDisableFolder(f) ?? false)
                    cell.Disable();

                return cell;
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath) => FoldersTableViewCell.Height;

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

                if (viewControllerWeakReference.Unwrap()?.ShouldDisableFolder(f) ?? false)
                    return;

                viewControllerWeakReference.Unwrap()?.FolderSelected(f);
            }

            public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
            {
                var f = foldersInView[indexPath.Row];
                viewControllerWeakReference.Unwrap()?.FolderDeselected(f);
            }

            void Cell_ExpandCollapseClicked(object sender, Folder f)
            {
                viewControllerWeakReference.Unwrap()?.FolderExpand(f);
            }

            public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
            {
                if (disableRowActions)
                    return new UITableViewRowAction[0];

                var f = foldersInView[indexPath.Row];
                var actions = new List<UITableViewRowAction>();

                if (FavoriteStatus.ContainsKey(f.Id))
                    if (FavoriteStatus[f.Id])
                    {
                        var action = UITableViewRowAction.Create(UITableViewRowActionStyle.Default,
                            Localization.GetString("remove_from_favorites"),
                            (a, ip) =>
                            {
                                viewControllerWeakReference.Unwrap()?.RemoveFromFavorites(foldersInView[ip.Row]);
                                tableView.SetEditing(false, true);
                            });
                        action.BackgroundColor = Theme.Brown;
                        actions.Add(action);
                    }
                    else
                    {
                        var action = UITableViewRowAction.Create(UITableViewRowActionStyle.Default,
                            Localization.GetString("add_to_favorites"),
                            (a, ip) =>
                            {
                                viewControllerWeakReference.Unwrap()?.AddToFavorites(foldersInView[ip.Row]);
                                tableView.SetEditing(false, true);
                            });
                        action.BackgroundColor = Theme.Brown;
                        actions.Add(action);
                    }

                if (module == ModuleType.Documents)
                {
                    if (SyncStatus.ContainsKey(f.Id))
                        if (SyncStatus[f.Id])
                        {
                            var action = UITableViewRowAction.Create(UITableViewRowActionStyle.Default,
                                Localization.GetString("disable_sync"),
                                (a, ip) =>
                                {
                                    viewControllerWeakReference.Unwrap()?.DisableSync(foldersInView[ip.Row]);
                                    tableView.SetEditing(false, true);
                                });
                            action.BackgroundColor = Theme.DarkBlue;
                            actions.Add(action);
                        }
                        else
                        {
                            var action = UITableViewRowAction.Create(UITableViewRowActionStyle.Default,
                                Localization.GetString("enable_sync"),
                                (a, ip) =>
                                {
                                    viewControllerWeakReference.Unwrap()?.EnableSync(foldersInView[ip.Row]);
                                    tableView.SetEditing(false, true);
                                });
                            action.BackgroundColor = Theme.DarkBlue;
                            actions.Add(action);
                        }

                    if (f.Subscribed)
                    {
                        var action = UITableViewRowAction.Create(UITableViewRowActionStyle.Default,
                            Localization.GetString("disable_notifications"),
                            (a, ip) =>
                            {
                                viewControllerWeakReference.Unwrap()?.DisableNotifications(foldersInView[ip.Row]);
                                tableView.SetEditing(false, true);
                            });
                        action.BackgroundColor = Theme.DarkerBlue;
                        actions.Add(action);
                    }
                    else
                    {
                        var action = UITableViewRowAction.Create(UITableViewRowActionStyle.Default,
                            Localization.GetString("enable_notifications"),
                            (a, ip) =>
                            {
                                viewControllerWeakReference.Unwrap()?.EnableNotifications(foldersInView[ip.Row]);
                                tableView.SetEditing(false, true);
                            });
                        action.BackgroundColor = Theme.DarkerBlue;
                        actions.Add(action);
                    }
                }

                if (module == ModuleType.Contacts || module == ModuleType.Shortcodes)
                {
                    var action = UITableViewRowAction.Create(UITableViewRowActionStyle.Default,
                        Localization.GetString("save_offline"),
                        (a, ip) =>
                        {
                            viewControllerWeakReference.Unwrap()?.SaveOffline(foldersInView[ip.Row]);
                            tableView.SetEditing(false, true);
                        });
                    action.BackgroundColor = Theme.DarkBlue;
                    actions.Add(action);
                }

                return actions.ToArray();
            }

            public void SetFolders(List<Folder> folders)
            {
                foldersInView.Clear();
                foldersInView.AddRange(folders);
                loading = false;

                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            public void Reload()
            {
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.None);
            }

            public void Reset()
            {
                loading = true;

                foldersInView.Clear();
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            public Folder[] GetFolders(int folderId)
            {
                var folders = new List<Folder>();
                for (var i = 0; i < foldersInView.Count; i++)
                {
                    var f = foldersInView[i];
                    if (f.Id == folderId)
                        folders.Add(f);
                }

                return folders.ToArray();
            }

            public NSIndexPath[] GetIndexPaths(int folderId)
            {
                var indexPaths = new List<NSIndexPath>();
                for (var i = 0; i < foldersInView.Count; i++)
                {
                    var f = foldersInView[i];
                    if (f.Id == folderId)
                        indexPaths.Add(NSIndexPath.FromRowSection(i, 0));
                }

                return indexPaths.ToArray();
            }
        }

        protected class SearchDataSource : UITableViewSource
        {
            public bool Empty => foldersInView.Count < 1;

            readonly WeakReference<AbstractFoldersListViewController> viewControllerWeakReference;
            readonly WeakReference<UITableView> tableViewWeakReference;

            bool loading = true;
            readonly List<Folder> foldersInView = new List<Folder>();

            public SearchDataSource(AbstractFoldersListViewController viewController, UITableView tableView)
            {
                viewControllerWeakReference = viewController.Wrap();
                tableViewWeakReference = tableView.Wrap();
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as WaitTableViewCell ?? WaitTableViewCell.Create();

                if (foldersInView.Count < 1)
                {
                    var emptyCell = tableView.DequeueReusableCell(WaitTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                    emptyCell.Initialize(Localization.GetString("no_folders_found"));
                    return emptyCell;
                }

                var cell = tableView.DequeueReusableCell(FoldersSearchResultsTableViewCell.Key) as FoldersSearchResultsTableViewCell ?? FoldersSearchResultsTableViewCell.Create();

                var f = foldersInView[indexPath.Row];
                cell.Initialize(f);
                if (viewControllerWeakReference.Unwrap()?.ShouldDisableFolder(f) ?? false)
                    cell.Disable();

                return cell;
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath) => FoldersSearchResultsTableViewCell.Height;

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

                if (viewControllerWeakReference.Unwrap()?.ShouldDisableFolder(f) ?? false)
                    return;

                viewControllerWeakReference.Unwrap()?.FolderSelected(f);
            }

            public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
            {
                var f = foldersInView[indexPath.Row];
                viewControllerWeakReference.Unwrap()?.FolderDeselected(f);
            }

            public void SetFolders(List<Folder> folders)
            {
                foldersInView.Clear();
                foldersInView.AddRange(folders);
                loading = false;
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            public void Reset()
            {
                loading = true;
                foldersInView.Clear();
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }
        }

        #endregion
    }
}