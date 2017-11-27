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
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentView;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

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
        protected UIBarButtonItem CreateShortcodeItem;

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
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info("Appeared");

            if (((TableView?.Source as GrouppedDataSource)?.Empty ?? false)
                || ((TableView?.Source as DataSource)?.Empty ?? false))
                RefreshData();
            else if (TableView?.Source as GrouppedDataSource != null)
                QuickRefreshData();

            if (Integration.IsRunningAtLeast(11))
            {
                NSOperationQueue.MainQueue.AddOperation(() =>
                {
                    var ni = NavigationItem;

                    if (ParentViewController != null && ParentViewController is UIViewController && !(ParentViewController is UINavigationController))
                        ni = ParentViewController?.NavigationItem;

                    if (ni.SearchController == null)
                        ni.SearchController = searchController;
                });
            }
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
            CommonConfig.Logger.Warning("Received memory warning!");

            (TableView.Source as DataSource)?.Reset();
            (TableView.Source as GrouppedDataSource)?.Reset();

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        protected override void Recycle()
        {
            base.Recycle();

            EditModeItem = null;
            ComposeDocumentItem = null;
            CreateContactItem = null;
            CreateShortcodeItem = null;

            searchCancellationTokenSource?.Dispose();
            searchCancellationTokenSource = null;
            searchCancellationTokenSourceList.ForEach(cts => cts?.Dispose());
            searchCancellationTokenSourceList.Clear();

            (TableView.Source as DataSource)?.Reset();
            (TableView.Source as GrouppedDataSource)?.Reset();

            searchController.SearchResultsUpdater = null;
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
                    Image = UIImage.FromBundle(Path.Combine("icons", "create.png"))
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
                    Image = UIImage.FromBundle(Path.Combine("icons", "create.png"))
                };
                NavigationItem.SetRightBarButtonItem(CreateContactItem, false);
            }

            if (ParentFolder.Module == ModuleType.Shortcodes && ServerConfig.SystemSettings.ShortcodesModuleInfo.Permissions.CreateAllowed)
            {
                CreateShortcodeItem = new UIBarButtonItem
                {
                    Image = UIImage.FromBundle(Path.Combine("icons", "create.png"))
                };
                NavigationItem.SetRightBarButtonItem(CreateShortcodeItem, false);
            }

            if (ParentFolder.Module == ModuleType.Contacts || ParentFolder.Module == ModuleType.Shortcodes || ParentFolder.Module == ModuleType.Calendar)
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
            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 50f;
            TableView.RefreshControl = RefreshControl;
        }

        protected virtual void InitializeSearchBar()
        {
            DefinesPresentationContext = true;

            var searchResultsController = new UITableViewController();
            var searchResultsDataSource = new SearchDataSource(this, searchResultsController.TableView);
            searchResultsController.TableView.Source = searchResultsDataSource;
            searchResultsController.TableView.EstimatedRowHeight = 50f;
            searchResultsController.TableView.RowHeight = UITableView.AutomaticDimension;

            searchController = new UISearchController(searchResultsController)
            {
                HidesNavigationBarDuringPresentation = true,
                DimsBackgroundDuringPresentation = true,
                ObscuresBackgroundDuringPresentation = true,
                SearchResultsUpdater = this
            };
            searchController.SearchBar.Placeholder = Localization.GetString("filter");

            if (!Integration.IsRunningAtLeast(11))
            {
                TableView.TableHeaderView = searchController.SearchBar;
            }
        }

        protected virtual void InitializeHandlers()
        {
            if (ComposeDocumentItem != null)
                ComposeDocumentItem.Clicked += ComposeDocumentItem_Clicked;

            if (EditModeItem != null)
                EditModeItem.Clicked += EditModeItem_Clicked;

            if (CreateContactItem != null)
                CreateContactItem.Clicked += CreateContactItem_Clicked;

            if (CreateShortcodeItem != null)
                CreateShortcodeItem.Clicked += CreateShortcodeItem_Clicked;

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

            if (CreateShortcodeItem != null)
                CreateShortcodeItem.Clicked -= CreateShortcodeItem_Clicked;

            RefreshControl.ValueChanged -= RefreshControl_ValueChanged;
        }

        #endregion

        #region NavigationBar handlers

        void ComposeDocumentItem_Clicked(object sender, EventArgs e)
        {
            var vc = new ComposeDocumentViewController2
            {
                DocumentCreationModeFlag = DocumentCreationModeFlag.New
            };
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        void CreateShortcodeItem_Clicked(object sender, EventArgs e)
        {
            var vc = new AddEditShortcodeViewController
            {
                CreationModeFlag = ShortcodeCreationModeFlag.New,
            };
            PresentViewController(new NavigationController(vc), true, null);
        }

        async void CreateContactItem_Clicked(object sender, EventArgs e)
        {
            var choice = await Dialogs.ShowListActionSheetAsync(this,
                                                           new[] { Localization.GetString("add_company"), Localization.GetString("add_department"), Localization.GetString("add_person") },
                                                           CreateContactItem);
            if (choice < 0)
                return;

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
                    await Managers.FoldersManager.SetFavoriteFoldersAsync(ParentFolder.Module, gds.GetFoldersInSection(GrouppedDataSource.Section.Favorites));
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error("Could not save favorite folders order", ex);

                    await Dialogs.ShowErrorAlertAsync(this, ex);
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

        void RefreshControl_ValueChanged(object sender, EventArgs e)
        {
            CommonConfig.UsageAnalytics.LogEvent(new PullToRefreshEvent(true, ParentFolder.Module));
            RefreshData(true);
        }

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
                        EditModeItem.Enabled = gds.GetItemsInSection(GrouppedDataSource.Section.Favorites) > 0;
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

                await Dialogs.ShowErrorAlertAsync(this, ex);

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
                var currentFavorites = gds.GetFoldersInSection(GrouppedDataSource.Section.Favorites);
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

                await Dialogs.ShowErrorAlertAsync(this, ex);

                if (!IsRootOfFoldersList)
                    NavigationController?.PopViewController(true);
            }
        }

        async void RefreshFoldersInfo()
        {
            if (IsRootOfFoldersList)
            {
                var gds = (GrouppedDataSource)TableView.Source;
                var ids = gds.ItemsIds;

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
                var ids = ds.Itemids;

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

        protected virtual void FolderSelected(Folder folder, bool isFromFavorite)
        {
            if (folder == null)
                return;

            if (folder.IsOutgoing)
                CommonConfig.UsageAnalytics.LogEvent(new OpenOutgoingFolderEvent());
            else
                CommonConfig.UsageAnalytics.LogEvent(new OpenFolderEvent(folder.Module, isFromFavorite));
        }

        protected virtual void FolderDeselected(Folder folder)
        {
        }

        protected virtual void FolderExpand(Folder folder)
        {
            if (folder == null)
                return;

            CommonConfig.UsageAnalytics.LogEvent(new ExpandFolderEvent(folder.Module));
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
                CommonConfig.UsageAnalytics.LogEvent(new SetFolderFavoriteEvent(folder.Module, 1));

                await Managers.FoldersManager.AddFavoriteFolderAsync(folder.Module, folder);

                if (TableView.Source is GrouppedDataSource gds)
                {
                    gds.FavoriteStatus[folder.Id] = true;

                    var favorites = await Managers.FoldersManager.GetFavoriteFoldersAsync(folder.Module);
                    gds.SetFolders(GrouppedDataSource.Section.Favorites, favorites);

                    var indexPaths = gds.GetIndexPaths(folder.Id);
                    TableView.ReloadRows(indexPaths, UITableViewRowAnimation.Fade);

                    if (EditModeItem != null)
                        EditModeItem.Enabled = gds.GetItemsInSection(GrouppedDataSource.Section.Favorites) > 0;
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

                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        public async void RemoveFromFavorites(Folder folder)
        {
            try
            {
                CommonConfig.UsageAnalytics.LogEvent(new SetFolderFavoriteEvent(folder.Module, 1));

                await Managers.FoldersManager.RemoveFavoriteFolderAsync(folder.Module, folder);

                if (TableView.Source is GrouppedDataSource gds)
                {
                    gds.FavoriteStatus[folder.Id] = false;

                    var favorites = await Managers.FoldersManager.GetFavoriteFoldersAsync(folder.Module);
                    gds.SetFolders(GrouppedDataSource.Section.Favorites, favorites);

                    var indexPaths = gds.GetIndexPaths(folder.Id);
                    TableView.ReloadRows(indexPaths, UITableViewRowAnimation.Fade);

                    if (EditModeItem != null)
                        EditModeItem.Enabled = gds.GetItemsInSection(GrouppedDataSource.Section.Favorites) > 0;
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

                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        public async void EnableNotifications(Folder folder)
        {
            try
            {
                CommonConfig.UsageAnalytics.LogEvent(new SetFolderNotifyEvent(folder.Module, 1));

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

                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        public async void DisableNotifications(Folder folder)
        {
            try
            {
                CommonConfig.UsageAnalytics.LogEvent(new SetFolderNotifyEvent(folder.Module, 1));

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

                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        public async void EnableSync(Folder folder)
        {
            try
            {
                CommonConfig.UsageAnalytics.LogEvent(new SetFolderSyncEvent(folder.Module, 1));

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

                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        public async void DisableSync(Folder folder)
        {
            try
            {
                CommonConfig.UsageAnalytics.LogEvent(new SetFolderSyncEvent(folder.Module, 1));

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

                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        public void SaveOffline(Folder folder)
        {
            PresentViewController(new NavigationController(new DownloadViewController { Folder = folder.ShallowCopy() }, UIModalPresentationStyle.FormSheet), true, null);
        }

        #endregion

        #region Searching

        void IUISearchResultsUpdating.UpdateSearchResultsForSearchController(UISearchController searchController)
        {
            var searchText = searchController.SearchBar.Text;

            if (!searchController.Active)
                CommonConfig.UsageAnalytics.LogEvent(new FilterEvent(true, ParentFolder.Module));

            if (!searchController.Active || string.IsNullOrWhiteSpace(searchText))
            {
                searchCancellationTokenSourceList.ForEach(cts => cts?.Cancel());
                searchCancellationTokenSourceList.Clear();

                var dataSource = ((UITableViewController)searchController.SearchResultsController).TableView.Source;
                ((SearchDataSource)dataSource)?.Reset();
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

            public bool Empty => items.All(kv => kv.Value.Count < 1);
            public int[] ItemsIds => items.SelectMany(kv => kv.Value).Select(f => f.Id).Distinct().ToArray();

            public SortedDictionary<int, bool> FavoriteStatus { get; set; } = new SortedDictionary<int, bool>();
            public SortedDictionary<int, bool> SyncStatus { get; set; } = new SortedDictionary<int, bool>();

            readonly WeakReference<AbstractFoldersListViewController> viewControllerWeakReference;
            readonly WeakReference<UITableView> tableViewWeakReference;
            readonly ModuleType module;
            readonly bool disableRowActions;

            bool[] loading;
            readonly Dictionary<nint, List<Folder>> items;

            public GrouppedDataSource(AbstractFoldersListViewController viewController, UITableView tableView, ModuleType module, bool disableRowActions)
            {
                viewControllerWeakReference = viewController.Wrap();
                tableViewWeakReference = tableView.Wrap();

                this.module = module;
                this.disableRowActions = disableRowActions;

                if (module == ModuleType.Documents)
                {
                    loading = new[] { true, true, true };
                    items = new Dictionary<nint, List<Folder>>
                    {
                        [Section.Favorites] = new List<Folder>(),
                        [Section.Folders] = new List<Folder>(),
                        [Section.Local] = new List<Folder>()
                    };
                }

                if (module == ModuleType.Contacts || module == ModuleType.Shortcodes || module == ModuleType.Calendar)
                {
                    loading = new[] { true, true };
                    items = new Dictionary<nint, List<Folder>>
                    {
                        [Section.Favorites] = new List<Folder>(),
                        [Section.Folders] = new List<Folder>()
                    };
                }
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading[indexPath.LongSection])
                    return tableView.DequeueReusableCell(WaitTableViewCell.DefaultId) as WaitTableViewCell ?? new WaitTableViewCell();

                if (items[indexPath.LongSection].Count < 1)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.DefaultId) as EmptyTableViewCell ?? new EmptyTableViewCell();

                    if (indexPath.LongSection == Section.Favorites)
                        emptyCell.Initialize(Localization.GetString("no_folders_in_favorites"));
                    else
                        emptyCell.Initialize(Localization.GetString("no_folders_in_section"));

                    return emptyCell;
                }

                var cell = tableView.DequeueReusableCell(FoldersTableViewCell.DefaultId) as FoldersTableViewCell;
                if (cell == null)
                {
                    cell = new FoldersTableViewCell
                    {
                        ExpandGestureRecognizer = new UITapGestureRecognizer(ExpandCollapse)
                    };
                }

                var f = items[indexPath.LongSection][indexPath.Row];
                SyncStatus.TryGetValue(f.Id, out bool folderIsCached);

                cell.Initialize(f, folderIsCached);
                if (viewControllerWeakReference.Unwrap()?.ShouldDisableFolder(f) ?? false)
                    cell.Disable();

                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading[section] || items[section].Count < 1)
                    return 1;

                return items[section].Count;
            }

            public override nint NumberOfSections(UITableView tableView) => items.Keys.Count;

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                if (tableView.Editing)
                    return;

                var f = items[indexPath.LongSection][indexPath.Row];

                if (viewControllerWeakReference.Unwrap()?.ShouldDisableFolder(f) ?? false)
                    return;

                viewControllerWeakReference.Unwrap()?.FolderSelected(f, indexPath.LongSection == 0);
            }

            public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
            {
                if (tableView.Editing)
                    return;

                var f = items[indexPath.LongSection][indexPath.Row];

                if (viewControllerWeakReference.Unwrap()?.ShouldDisableFolder(f) ?? false)
                    return;

                viewControllerWeakReference?.Unwrap().FolderDeselected(f);
            }

            void ExpandCollapse(UITapGestureRecognizer g)
            {
                var viewLocation = g.View.Bounds.Location;
                var location = tableViewWeakReference.Unwrap()?.ConvertPointFromView(viewLocation, g.View);
                if (location == null)
                    return;

                var indexPath = tableViewWeakReference.Unwrap()?.IndexPathForRowAtPoint(location.Value);
                if (indexPath == null)
                    return;

                var f = items[indexPath.LongSection][indexPath.Row];

                if (viewControllerWeakReference.Unwrap()?.ShouldDisableFolder(f) ?? false)
                    return;

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

                return null;
            }

            public override void WillDisplayHeaderView(UITableView tableView, UIView headerView, nint section) => headerView.ApplyTheme();

            public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
            {
                if (disableRowActions)
                    return false;

                if (tableView.Editing)
                    return indexPath.LongSection == Section.Favorites;

                return indexPath.LongSection != Section.Local;
            }

            public override bool ShouldIndentWhileEditing(UITableView tableView, NSIndexPath indexPath) => false;

            public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return tableView.Editing ? UITableViewCellEditingStyle.None : UITableViewCellEditingStyle.Delete;
            }

            public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
            {
                var f = items[indexPath.LongSection][indexPath.Row];
                var actions = new List<UITableViewRowAction>();

                if (FavoriteStatus.ContainsKey(f.Id))
                    if (FavoriteStatus[f.Id])
                    {
                        var action = UITableViewRowAction.Create(UITableViewRowActionStyle.Default,
                            Localization.GetString("remove_from_favorites"),
                            (a, ip) =>
                            {
                                viewControllerWeakReference.Unwrap()?.RemoveFromFavorites(items[ip.LongSection][ip.Row]);
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
                                viewControllerWeakReference.Unwrap()?.AddToFavorites(items[ip.LongSection][ip.Row]);
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
                                    viewControllerWeakReference.Unwrap()?.DisableSync(items[ip.LongSection][ip.Row]);
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
                                    viewControllerWeakReference.Unwrap()?.EnableSync(items[ip.LongSection][ip.Row]);
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
                                viewControllerWeakReference.Unwrap()?.DisableNotifications(items[ip.LongSection][ip.Row]);
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
                                viewControllerWeakReference.Unwrap()?.EnableNotifications(items[ip.LongSection][ip.Row]);
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
                            viewControllerWeakReference.Unwrap()?.SaveOffline(items[ip.LongSection][ip.Row]);
                            tableView.SetEditing(false, true);
                        });
                    action.BackgroundColor = Theme.DarkBlue;
                    actions.Add(action);
                }

                return actions.ToArray();
            }

            public override bool CanMoveRow(UITableView tableView, NSIndexPath indexPath) => indexPath.Section == Section.Favorites;

            public override NSIndexPath CustomizeMoveTarget(UITableView tableView, NSIndexPath sourceIndexPath, NSIndexPath proposedIndexPath)
            {
                return proposedIndexPath.Section == Section.Favorites ? proposedIndexPath : sourceIndexPath;
            }

            public override void MoveRow(UITableView tableView, NSIndexPath sourceIndexPath, NSIndexPath destinationIndexPath)
            {
                var oldRow = sourceIndexPath.Row;
                var newRow = destinationIndexPath.Row;

                var rowToMove = items[Section.Favorites][oldRow];
                items[Section.Favorites].RemoveAt(oldRow);
                items[Section.Favorites].Insert(newRow, rowToMove);
            }

            public void SetFolders(nint section, List<Folder> folders)
            {
                items[section].Clear();
                items[section].AddRange(folders);
                loading[section] = false;
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(section), UITableViewRowAnimation.Fade);
            }

            public void Reload()
            {
                tableViewWeakReference.Unwrap()?.BeginUpdates();
                if (module == ModuleType.Documents)
                    tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(Section.Local), UITableViewRowAnimation.None);

                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(Section.Favorites), UITableViewRowAnimation.None);
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(Section.Folders), UITableViewRowAnimation.None);
                tableViewWeakReference.Unwrap()?.EndUpdates();
            }

            public void Reset()
            {
                for (var i = 0; i < loading.Length; i++)
                    loading[i] = true;

                foreach (var kv in items)
                    kv.Value.Clear();

                tableViewWeakReference.Unwrap()?.BeginUpdates();
                if (module == ModuleType.Documents)
                    tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(Section.Local), UITableViewRowAnimation.Fade);

                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(Section.Favorites), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(Section.Folders), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.EndUpdates();
            }

            public List<Folder> GetFoldersInSection(nint section) => items[section];

            public int GetItemsInSection(nint section) => items[section].Count;

            public Folder[] GetFolders(int folderId)
            {
                var folders = new List<Folder>();
                foreach (var item in items)
                    for (var i = 0; i < item.Value.Count; i++)
                    {
                        var f = item.Value[i];
                        if (f.Id == folderId)
                            folders.Add(f);
                    }

                return folders.ToArray();
            }

            public NSIndexPath[] GetIndexPaths(int folderId)
            {
                var indexPaths = new List<NSIndexPath>();
                foreach (var item in items)
                    for (var i = 0; i < item.Value.Count; i++)
                    {
                        var f = item.Value[i];
                        if (f.Id == folderId)
                            indexPaths.Add(NSIndexPath.FromRowSection(i, item.Key));
                    }

                return indexPaths.ToArray();
            }
        }

        protected class DataSource : UITableViewSource, IDisposable
        {
            public bool Empty => items.Count < 1;
            public int[] Itemids => items.Select(f => f.Id).Distinct().ToArray();

            public SortedDictionary<int, bool> FavoriteStatus { get; set; } = new SortedDictionary<int, bool>();
            public SortedDictionary<int, bool> SyncStatus { get; set; } = new SortedDictionary<int, bool>();

            readonly WeakReference<AbstractFoldersListViewController> viewControllerWeakReference;
            readonly WeakReference<UITableView> tableViewWeakReference;
            readonly ModuleType module;
            readonly bool disableRowActions;

            bool loading = true;
            readonly List<Folder> items = new List<Folder>();

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
                    return tableView.DequeueReusableCell(WaitTableViewCell.DefaultId) as WaitTableViewCell ?? new WaitTableViewCell();

                if (Empty)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.DefaultId) as EmptyTableViewCell ?? new EmptyTableViewCell();
                    emptyCell.Initialize(Localization.GetString("no_folders_in_section"));
                    return emptyCell;
                }

                var cell = tableView.DequeueReusableCell(FoldersTableViewCell.DefaultId) as FoldersTableViewCell;
                if (cell == null)
                {
                    cell = new FoldersTableViewCell
                    {
                        ExpandGestureRecognizer = new UITapGestureRecognizer(ExpandCollapse)
                    };
                }

                var f = items[indexPath.Row];
                SyncStatus.TryGetValue(f.Id, out bool folderIsCached);

                cell.Initialize(f, folderIsCached);

                if (viewControllerWeakReference.Unwrap()?.ShouldDisableFolder(f) ?? false)
                    cell.Disable();

                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading || Empty)
                    return 1;

                return items.Count;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                if (tableView.Editing)
                    return;

                var f = items[indexPath.Row];

                if (viewControllerWeakReference.Unwrap()?.ShouldDisableFolder(f) ?? false)
                    return;

                viewControllerWeakReference.Unwrap()?.FolderSelected(f, indexPath.LongSection == 0);
            }

            public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
            {
                if (tableView.Editing)
                    return;

                var f = items[indexPath.Row];

                if (viewControllerWeakReference.Unwrap()?.ShouldDisableFolder(f) ?? false)
                    return;

                viewControllerWeakReference.Unwrap()?.FolderDeselected(f);
            }

            void ExpandCollapse(UITapGestureRecognizer g)
            {
                var viewLocation = g.View.Bounds.Location;
                var location = tableViewWeakReference.Unwrap()?.ConvertPointFromView(viewLocation, g.View);
                if (location == null)
                    return;

                var indexPath = tableViewWeakReference.Unwrap()?.IndexPathForRowAtPoint(location.Value);
                if (indexPath == null)
                    return;

                var f = items[indexPath.Row];

                if (viewControllerWeakReference.Unwrap()?.ShouldDisableFolder(f) ?? false)
                    return;

                viewControllerWeakReference.Unwrap()?.FolderExpand(f);
            }

            public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath) => !disableRowActions && (tableView.CellAt(indexPath)?.UserInteractionEnabled ?? false);

            public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
            {
                var f = items[indexPath.Row];
                var actions = new List<UITableViewRowAction>();

                if (FavoriteStatus.ContainsKey(f.Id))
                    if (FavoriteStatus[f.Id])
                    {
                        var action = UITableViewRowAction.Create(UITableViewRowActionStyle.Default,
                            Localization.GetString("remove_from_favorites"),
                            (a, ip) =>
                            {
                                viewControllerWeakReference.Unwrap()?.RemoveFromFavorites(items[ip.Row]);
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
                                viewControllerWeakReference.Unwrap()?.AddToFavorites(items[ip.Row]);
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
                                    viewControllerWeakReference.Unwrap()?.DisableSync(items[ip.Row]);
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
                                    viewControllerWeakReference.Unwrap()?.EnableSync(items[ip.Row]);
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
                                viewControllerWeakReference.Unwrap()?.DisableNotifications(items[ip.Row]);
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
                                viewControllerWeakReference.Unwrap()?.EnableNotifications(items[ip.Row]);
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
                            viewControllerWeakReference.Unwrap()?.SaveOffline(items[ip.Row]);
                            tableView.SetEditing(false, true);
                        });
                    action.BackgroundColor = Theme.DarkBlue;
                    actions.Add(action);
                }

                return actions.ToArray();
            }

            public void SetFolders(List<Folder> folders)
            {
                items.Clear();
                items.AddRange(folders);
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

                items.Clear();
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            public Folder[] GetFolders(int folderId)
            {
                var folders = new List<Folder>();
                for (var i = 0; i < items.Count; i++)
                {
                    var f = items[i];
                    if (f.Id == folderId)
                        folders.Add(f);
                }

                return folders.ToArray();
            }

            public NSIndexPath[] GetIndexPaths(int folderId)
            {
                var indexPaths = new List<NSIndexPath>();
                for (var i = 0; i < items.Count; i++)
                {
                    var f = items[i];
                    if (f.Id == folderId)
                        indexPaths.Add(NSIndexPath.FromRowSection(i, 0));
                }

                return indexPaths.ToArray();
            }
        }

        protected class SearchDataSource : UITableViewSource
        {
            public bool Empty => items.Count < 1;

            readonly WeakReference<AbstractFoldersListViewController> viewControllerWeakReference;
            readonly WeakReference<UITableView> tableViewWeakReference;

            bool loading = true;
            readonly List<Folder> items = new List<Folder>();

            public SearchDataSource(AbstractFoldersListViewController viewController, UITableView tableView)
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
                    emptyCell.Initialize(Localization.GetString("no_folders_found"));
                    return emptyCell;
                }

                var f = items[indexPath.Row];

                var cell = tableView.DequeueReusableCell(FoldersSearchResultsTableViewCell.DefaultId) as FoldersSearchResultsTableViewCell ?? new FoldersSearchResultsTableViewCell();
                cell.Initialize(f);

                if (viewControllerWeakReference.Unwrap()?.ShouldDisableFolder(f) ?? false)
                    cell.Disable();

                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading || Empty)
                    return 1;

                return items.Count;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                if (tableView.Editing)
                    return;

                var f = items[indexPath.Row];

                if (viewControllerWeakReference.Unwrap()?.ShouldDisableFolder(f) ?? false)
                    return;

                viewControllerWeakReference.Unwrap()?.FolderSelected(f, indexPath.LongSection == 0);
            }

            public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
            {
                if (tableView.Editing)
                    return;

                var f = items[indexPath.Row];

                if (viewControllerWeakReference.Unwrap()?.ShouldDisableFolder(f) ?? false)
                    return;

                viewControllerWeakReference.Unwrap()?.FolderDeselected(f);
            }

            public void SetFolders(List<Folder> folders)
            {
                items.Clear();
                items.AddRange(folders);
                loading = false;
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            public void Reset()
            {
                loading = true;
                items.Clear();
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }
        }

        #endregion

    }
}