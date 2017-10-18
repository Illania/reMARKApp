using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Model.HubMessages;
using Mark5.Mobile.Common.Model.HubMessages;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;
using TinyMessenger;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.ShortcodesList
{
    public abstract class AbstractShortcodesListViewController : AbstractTableViewController, IPrimaryViewController, IUISearchResultsUpdating
    {
        protected readonly bool DisableRowActions;

        public Folder Folder { get; set; }

        protected UIBarButtonItem ExitEditItem;
        protected UIBarButtonItem EditItem;
        protected UIBarButtonItem LeftButton;
        protected UIBarButtonItem RightButton;

        bool refreshing;
        UISearchController searchController;
        protected CancellationTokenSource searchCancellationTokenSource;
        readonly List<CancellationTokenSource> searchCancellationTokenSourceList = new List<CancellationTokenSource>();

        CancellationTokenSource cts;

        TinyMessageSubscriptionToken shortcodeChangedToken;
        TinyMessageSubscriptionToken removedFromFolderToken;
        TinyMessageSubscriptionToken movedFromFolderToken;
        TinyMessageSubscriptionToken deletedToken;

        protected AbstractShortcodesListViewController(bool disableRowActions)
        {
            DisableRowActions = disableRowActions;
        }

        #region UIViewController overrides

        public override void LoadView()
        {
            base.LoadView();

            InitializeNavigationBar();
            InitializeView();
            InitializeSearchBar();
            SubscribeToMessages();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (NavigationController != null)
                NavigationController.NavigationBar.PrefersLargeTitles = true;
            NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Automatic;

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

            if (((DataSource)TableView.Source).Empty)
                RefreshData();

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

            cts?.Cancel();

            if (searchController != null && searchController.Active)
                searchController.Active = false;
        }

        public override void WillMoveToParentViewController(UIViewController parent)
        {
            base.WillMoveToParentViewController(parent);

            if (parent == null && SplitViewController != null && !SplitViewController.Collapsed)
            {
                var nc = (UINavigationController)SplitViewController.ViewControllers[1];
                nc.PopToRootViewController(false);

                var vc = (ShortcodeViewController)nc.ViewControllers[0];
                vc.ClearData();
            }
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning("Received memory warning!");

            ((DataSource)TableView.Source)?.Reset();
            UnsubscribeFromMessages();

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        protected override void Recycle()
        {
            base.Recycle();

            ExitEditItem = null;
            EditItem = null;
            LeftButton = null;
            RightButton = null;

            UnsubscribeFromMessages();

            searchCancellationTokenSource?.Dispose();
            searchCancellationTokenSource = null;
            searchCancellationTokenSourceList.ForEach(cts => cts?.Dispose());
            searchCancellationTokenSourceList.Clear();

            TableView.GestureRecognizers.ForEach(TableView.RemoveGestureRecognizer);
            ((DataSource)TableView.Source)?.Reset();

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
            NavigationItem.Title = Folder.Name;

            ExitEditItem = new UIBarButtonItem(UIBarButtonSystemItem.Done);
            EditItem = new UIBarButtonItem(UIBarButtonSystemItem.Edit);
        }

        void InitializeView()
        {
            RefreshControl = new UIRefreshControl();

            TableView.Source = new DataSource(this, TableView, DisableRowActions, Localization.GetString("folder_empty"));
            TableView.RefreshControl = RefreshControl;
            TableView.AllowsMultipleSelectionDuringEditing = true;

            TableView.AddGestureRecognizer(new UILongPressGestureRecognizer(ShortcodePreviewLongPressed));
        }

        void InitializeSearchBar()
        {
            DefinesPresentationContext = true;

            var searchResultsController = new UITableViewController();
            var searchResultsDataSource = new DataSource(this, searchResultsController.TableView, DisableRowActions, Localization.GetString("no_matching_shortcodes"));
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
            if (ExitEditItem != null)
                ExitEditItem.Clicked += ExitEditItem_Clicked;

            if (EditItem != null)
                EditItem.Clicked += EditItem_Clicked;

            RefreshControl.ValueChanged += RefreshControl_ValueChanged;
        }

        protected virtual void DeinitializeHandlers()
        {
            if (ExitEditItem != null)
                ExitEditItem.Clicked -= ExitEditItem_Clicked;

            if (EditItem != null)
                EditItem.Clicked -= EditItem_Clicked;

            RefreshControl.ValueChanged -= RefreshControl_ValueChanged;
        }

        #endregion

        #region Subscribe/unsubscribe

        void SubscribeToMessages()
        {
            shortcodeChangedToken = CommonConfig.MessengerHub.Subscribe<EntityPreviewChangedMessage>(HandleShortcodeChanged, m => m.EntityPreview.ObjectType == ObjectType.Shortcode);
            removedFromFolderToken = CommonConfig.MessengerHub.Subscribe<EntityRemovedFromFolderMessage>(HandleRemovedFromFolder, m => m.ObjectType == ObjectType.Shortcode);
            movedFromFolderToken = CommonConfig.MessengerHub.Subscribe<EntityMovedFromFolderMessage>(HandleMovedFromFolder, m => m.ObjectType == ObjectType.Shortcode);
            deletedToken = CommonConfig.MessengerHub.Subscribe<EntityRemovedMessage>(HandleDeleted, m => m.ObjectType == ObjectType.Shortcode);
        }

        void UnsubscribeFromMessages()
        {
            shortcodeChangedToken?.Dispose();
            removedFromFolderToken?.Dispose();
            movedFromFolderToken?.Dispose();
            deletedToken?.Dispose();
        }

        #endregion

        #region NavigationBar handlerster

        void ExitEditItem_Clicked(object sender, EventArgs e) => EndEditing();

        void EditItem_Clicked(object sender, EventArgs e)
        {
            if (TableView.IndexPathsForSelectedRows == null || TableView.IndexPathsForSelectedRows.Length < 1)
                return;

            var eas = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            var d = new PopoverPresentationControllerDelegate((UIBarButtonItem)sender);

            var rows = TableView.IndexPathsForSelectedRows.ToArray();
            var selectedShortcodes = rows.Select(ip => ((DataSource)TableView.Source).FindItemAtIndexPath(ip)).ToList();

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_worktray"),
                UIAlertActionStyle.Default,
                a =>
                {
                    CopyToWorktray(selectedShortcodes);
                    EndEditing();
                }));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_folder"),
                UIAlertActionStyle.Default,
                a =>
                {
                    CopyToFolder(selectedShortcodes);
                    EndEditing();
                }));

            if (Folder.InternalType == FolderInternalType.FilterView || Folder.InternalType == FolderInternalType.Static || Folder.InternalType == FolderInternalType.Worktray)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("move_to_folder"),
                    UIAlertActionStyle.Default,
                    a =>
                    {
                        MoveToFolder(selectedShortcodes);
                        EndEditing();
                    }));

            if (Folder.InternalType == FolderInternalType.FilterView || Folder.InternalType == FolderInternalType.Static || Folder.InternalType == FolderInternalType.Worktray)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete_from_folder"), UIAlertActionStyle.Default, a => RemoveFromFolder(selectedShortcodes, d)));

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator || ServerConfig.SystemSettings.ShortcodesModuleInfo.Permissions.DeleteAllowed)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete"), UIAlertActionStyle.Destructive, a => Delete(selectedShortcodes, d)));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, a => ExitEditItem.Enabled = true));

            if (eas.PopoverPresentationController != null)
                eas.PopoverPresentationController.Delegate = d;

            ExitEditItem.Enabled = false;
            PresentViewController(eas, true, null);
        }

        #endregion

        #region Refreshing

        void RefreshControl_ValueChanged(object sender, EventArgs e) => RefreshData(forceClear: true);

        async void RefreshData(int startRowId = -1, bool forceClear = false)
        {
            if (refreshing)
                return;

            refreshing = true;
            RefreshControl.ValueChanged -= RefreshControl_ValueChanged;

            if (forceClear && await Managers.FoldersManager.IsSavedFolderOfflineInfo(Folder))
            {
                var result = await Dialogs.ShowYesNoCancelAlertAsync(this,
                                                                      Localization.GetString("folder_offline_title"),
                                                                      Localization.GetString("folder_offline_message"),
                                                                      Localization.GetString("folder_offline_go_online"),
                                                                      Localization.GetString("folder_offline_redownload"),
                                                                      Localization.GetString("cancel"));

                if (result == 1)
                    await Managers.FoldersManager.RemoveSavedFolderInfo(Folder);
                if (result == 0)
                {
                    var vc = new DownloadViewController { Folder = Folder };
                    PresentViewController(new NavigationController(vc, UIModalPresentationStyle.FormSheet), true, null);
                    await vc.Result;
                }
                if (result == -1)
                {
                    RefreshControl.EndRefreshing();
                    RefreshControl.ValueChanged += RefreshControl_ValueChanged;

                    refreshing = false;

                    return;
                }
            }

            CommonConfig.Logger.Info($"Refreshing shortcodes list [folder={Folder?.Name}, startRowId={startRowId}, forceClear={forceClear}]");

            cts?.Cancel();
            cts = new CancellationTokenSource();

            if (forceClear)
                ((DataSource)TableView.Source).Reset();

            var sourceType = await Managers.FoldersManager.IsSavedFolderOfflineInfo(Folder) ? SourceType.Local : SourceType.Auto;

            Managers.ShortcodesManager.GetAllShortcodePreviews(Folder,
                sps =>
                {
                    InvokeOnMainThread(() =>
                    {
                        ((DataSource)TableView.Source).AppendItems(sps);
                    });
                },
                () =>
                {
                    RefreshControl.EndRefreshing();
                    RefreshControl.ValueChanged += RefreshControl_ValueChanged;

                    refreshing = false;

                    CommonConfig.Logger.Info($"Refresh finished");
                },
                async ex =>
                {
                    RefreshControl.EndRefreshing();
                    RefreshControl.ValueChanged += RefreshControl_ValueChanged;

                    refreshing = false;

                    CommonConfig.Logger.Error($"Could not refresh shortcodes [folder={Folder?.Name}, startRowId={startRowId}, forceClear={forceClear}]", ex);

                    await Dialogs.ShowErrorAlertAsync(this, ex);

                    NavigationController?.PopViewController(true);
                },
                startRowId,
                cts.Token,
                sourceType);
        }

        #endregion

        #region List handlers

        public virtual void ShortcodeSelected(UITableView tableView, NSIndexPath indexPath, ShortcodePreview shortcodePreview)
        {
        }

        public void ShortcodePreviewLongPressed(UILongPressGestureRecognizer recognizer)
        {
            if (TableView.Editing || ((DataSource)TableView.Source).Empty)
                return;

            StartEditing();

            var point = recognizer.LocationInView(TableView);
            var indexPath = TableView.IndexPathForRowAtPoint(point);

            if (!TableView.CellAt(indexPath)?.UserInteractionEnabled ?? true)
                return;

            TableView.SelectRow(indexPath, true, UITableViewScrollPosition.None);
        }

        #endregion

        #region Actions

        void ShowMoreActionSheet(NSIndexPath indexPath, ShortcodePreview selectedShortcode)
        {
            var eas = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            var d = new PopoverPresentationControllerDelegate(TableView, TableView.CellAt(indexPath));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_folder"),
                UIAlertActionStyle.Default,
                a =>
                {
                    CopyToFolder(selectedShortcode);
                    EndEditing();
                }));

            if (Folder.InternalType == FolderInternalType.FilterView || Folder.InternalType == FolderInternalType.Static || Folder.InternalType == FolderInternalType.Worktray)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("move_to_folder"),
                    UIAlertActionStyle.Default,
                    a =>
                    {
                        MoveToFolder(selectedShortcode);
                        EndEditing();
                    }));

            if (Folder.InternalType == FolderInternalType.FilterView || Folder.InternalType == FolderInternalType.Static || Folder.InternalType == FolderInternalType.Worktray)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete_from_folder"), UIAlertActionStyle.Default, a => RemoveFromFolder(selectedShortcode, d)));

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator || ServerConfig.SystemSettings.ShortcodesModuleInfo.Permissions.DeleteAllowed)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete"), UIAlertActionStyle.Destructive, a => Delete(selectedShortcode, d)));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, a => EndEditing()));

            if (eas.PopoverPresentationController != null)
                eas.PopoverPresentationController.Delegate = d;

            PresentViewController(eas, true, null);
        }

        void RemoveFromFolder(ShortcodePreview selectedShortcode, UIPopoverPresentationControllerDelegate d) =>
            RemoveFromFolder(new List<ShortcodePreview> { selectedShortcode }, d);

        async void RemoveFromFolder(List<ShortcodePreview> selectedShortcodes, UIPopoverPresentationControllerDelegate d)
        {
            var result = await Dialogs.ShowDestructiveActionSheetAsync(this, Localization.GetString("delete_from_folder"), d);
            if (!result)
            {
                EndEditing();
                return;
            }

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("deleting_from_folder___"));

            try
            {
                CommonConfig.Logger.Info($"Attempting to remove shortcodes from folder [folderId={Folder.Id}]");

                await Managers.CommonActionsManager.RemoveFromFolder(selectedShortcodes.Cast<IBusinessEntity>().ToList(), Folder);

                RemoveShortcodesFromList(selectedShortcodes.Select(s => s.Id));
                EndEditing();

                dismissAction();
            }
            catch (Exception ex)
            {
                EndEditing();
                dismissAction();

                CommonConfig.Logger.Error($"Error while removing shortcodes from folder [folderId={Folder.Id}]", ex);
                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        void Delete(ShortcodePreview selectedShortcode, UIPopoverPresentationControllerDelegate d) =>
            Delete(new List<ShortcodePreview> { selectedShortcode }, d);

        async void Delete(List<ShortcodePreview> selectedShortcodes, UIPopoverPresentationControllerDelegate d)
        {
            var result = await Dialogs.ShowDestructiveActionSheetAsync(this, Localization.GetString("delete"), d);
            if (!result)
            {
                EndEditing();
                return;
            }

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("deleting___"));

            try
            {
                CommonConfig.Logger.Info($"Attempting to delete shortcodes]");

                await Managers.CommonActionsManager.Delete(selectedShortcodes.Cast<IBusinessEntity>().ToList());

                RemoveShortcodesFromList(selectedShortcodes.Select(s => s.Id));
                EndEditing();

                dismissAction();
            }
            catch (Exception ex)
            {
                EndEditing();
                dismissAction();

                CommonConfig.Logger.Error($"Error while deleting shortcodes", ex);
                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        void CopyToWorktray(ShortcodePreview shortcodePreview) =>
            CopyToWorktray(new List<ShortcodePreview> { shortcodePreview });

        void CopyToWorktray(List<ShortcodePreview> shortcodePreviews)
        {
            var vc = new CopyToWorktrayViewController
            {
                BusinessEntities = shortcodePreviews.Cast<IBusinessEntity>().ToList()
            };
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        void CopyToFolder(ShortcodePreview shortcodePreview) =>
            CopyToFolder(new List<ShortcodePreview> { shortcodePreview });

        void CopyToFolder(List<ShortcodePreview> shortcodePreviews)
        {
            var vc = new CopyMoveToFolderListViewController(ModuleType.Shortcodes, shortcodePreviews.Cast<IBusinessEntity>().ToList());
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        void MoveToFolder(ShortcodePreview shortcodePreview) =>
            MoveToFolder(new List<ShortcodePreview> { shortcodePreview });

        void MoveToFolder(List<ShortcodePreview> shortcodePreviews)
        {
            var vc = new CopyMoveToFolderListViewController(ModuleType.Shortcodes, shortcodePreviews.Cast<IBusinessEntity>().ToList(), Folder);
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
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
                ((DataSource)dataSource)?.Reset();
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

                DoSearchShortcodes(searchText, searchCancellationTokenSource.Token);
            }
        }

        async void DoSearchShortcodes(string searchText, CancellationToken ct)
        {
            var tableViewController = searchController?.SearchResultsController as UITableViewController;
            var dataSource = tableViewController?.TableView?.Source as DataSource;
            dataSource?.Reset();

            await Task.Delay(500);

            if (ct.IsCancellationRequested)
                return;

            var ds = (DataSource)TableView.Source;
            var filteredShortcodes = ds.Items.Where(sp => MatchesQuery(sp, searchText)).ToList();

            if (ct.IsCancellationRequested)
                return;

            dataSource?.AppendItems(filteredShortcodes);
        }

        static bool MatchesQuery(ShortcodePreview sp, string query)
        {
            if (sp.Name?.ContainsCaseInsensitive(query) ?? false)
                return true;

            if (sp.Description?.ContainsCaseInsensitive(query) ?? false)
                return true;

            return false;
        }

        #endregion

        #region Messages handlers

        void HandleShortcodeChanged(EntityPreviewChangedMessage m) => UpdateShortcodeOnList((ShortcodePreview)m.EntityPreview);

        void HandleRemovedFromFolder(EntityRemovedFromFolderMessage m) => RemoveShortcodesFromList(m.EntitiesId);

        void HandleMovedFromFolder(EntityMovedFromFolderMessage m) => RemoveShortcodesFromList(m.EntitiesId);

        void HandleDeleted(EntityRemovedMessage m) => RemoveShortcodesFromList(m.EntitiesId);

        #endregion

        #region Utilities

        void StartEditing()
        {
            TableView.SetEditing(true, true);
            NavigationItem.SetRightBarButtonItem(ExitEditItem, true);
            NavigationItem.SetLeftBarButtonItem(EditItem, true);

            searchController.SearchBar.UserInteractionEnabled = false;
            searchController.SearchBar.Alpha = .5f;

            if (SplitViewController != null && !SplitViewController.Collapsed)
            {
                var nc = (UINavigationController)SplitViewController.ViewControllers[1];
                nc.PopToRootViewController(false);

                var vc = (ShortcodeViewController)nc.ViewControllers[0];
                vc.ClearData();
            }
        }

        void EndEditing()
        {
            TableView.SetEditing(false, true);
            NavigationItem.SetRightBarButtonItem(null, true);
            NavigationItem.SetLeftBarButtonItem(NavigationItem.BackBarButtonItem, true);

            searchController.SearchBar.UserInteractionEnabled = true;
            searchController.SearchBar.Alpha = 1f;
        }

        void UpdateShortcodeOnList(ShortcodePreview sp)
        {
            if (searchController.Active)
            {
                var tableViewController = searchController?.SearchResultsController as UITableViewController;
                var dataSource = tableViewController?.TableView?.Source as DataSource;
                dataSource?.UpdateItem(sp);
            }

            ((DataSource)TableView.Source).UpdateItem(sp);
        }

        void RemoveShortcodesFromList(IEnumerable<int> ids)
        {
            BeginInvokeOnMainThread(() =>
            {
                if (searchController.Active)
                {
                    var tableViewController = searchController?.SearchResultsController as UITableViewController;
                    var dataSource = tableViewController?.TableView?.Source as DataSource;
                    dataSource?.RemoveItems(ids);
                }

                ((DataSource)TableView.Source).RemoveItems(ids);

                if (SplitViewController != null && !SplitViewController.Collapsed)
                {
                    var nc = (UINavigationController)SplitViewController.ViewControllers[1];
                    var vc = (ShortcodeViewController)nc.ViewControllers[0];
                    if (ids.Select(id => vc.IsShowingShortcodeWithId(id)).Any(v => v))
                        vc.ClearData();
                }
            });
        }

        #endregion

        protected class DataSource : UITableViewSource
        {
            public bool Empty => !items.SelectMany(v => v).Any();
            public IEnumerable<ShortcodePreview> Items => items.SelectMany(i => i);

            readonly WeakReference<AbstractShortcodesListViewController> viewControllerWeakReference;
            readonly WeakReference<UITableView> tableViewWeakReference;
            readonly bool disableRowActions;
            readonly string emptyText;

            bool loading = true;
            readonly List<List<ShortcodePreview>> items = new List<List<ShortcodePreview>>(25);

            public DataSource(AbstractShortcodesListViewController viewController, UITableView tableView, bool disableRowActions, string emptyText)
            {
                viewControllerWeakReference = viewController.Wrap();
                tableViewWeakReference = tableView.Wrap();
                this.disableRowActions = disableRowActions;
                this.emptyText = emptyText;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                    return tableView.DequeueReusableCell(WaitTableViewCell.DefaultId) as WaitTableViewCell ?? new WaitTableViewCell();

                if (Empty)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.DefaultId) as EmptyTableViewCell ?? new EmptyTableViewCell();
                    emptyCell.Initialize(emptyText);
                    return emptyCell;
                }

                var cp = items[indexPath.Section][indexPath.Row];

                var cell = tableView.DequeueReusableCell(ShortcodesTableViewCell.Key) as ShortcodesTableViewCell ?? ShortcodesTableViewCell.Create();
                cell.Initialize(cp);
                return cell;
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath) => ShortcodesTableViewCell.Height;

            public override nint NumberOfSections(UITableView tableView)
            {
                if (loading || Empty)
                    return 1;

                return items.Count;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading || Empty)
                    return 1;

                return items[(int)section].Count;
            }

            public override string[] SectionIndexTitles(UITableView tableView) => items.SelectMany(i => i)
                                                                                       .Select(cp => cp.Name.SafeSubstring(0, 1).ToUpper())
                                                                                       .Distinct()
                                                                                       .ToArray();

            public override nint SectionFor(UITableView tableView, string title, nint atIndex)
            {
                for (var section = 0; section < items.Count; section++)
                {
                    var row = items[section].FindIndex(cp => cp.Name.SafeSubstring(0, 1).ToUpper() == title);
                    if (row >= 0)
                    {
                        tableView.ScrollToRow(NSIndexPath.FromRowSection(row, section), UITableViewScrollPosition.Top, true);
                        break;
                    }
                }

                return -1;
            }

            public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath) => !disableRowActions && (tableView.CellAt(indexPath)?.UserInteractionEnabled ?? false);

            public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
            {
                var actions = new List<UITableViewRowAction>();

                var shortcodePreview = items[indexPath.Section][indexPath.Row];

                var copyToWorktrayAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Default,
                    Localization.GetString("copy_to_worktray_ml"),
                    (a, ip) =>
                    {
                        viewControllerWeakReference.Unwrap()?.CopyToWorktray(shortcodePreview);
                        viewControllerWeakReference.Unwrap()?.EndEditing();
                    });
                copyToWorktrayAction.BackgroundColor = Theme.DarkBlue;
                actions.Add(copyToWorktrayAction);

                var moreAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Default,
                                                             Localization.GetString("more"),
                                                             (a, ip) =>
                {
                    viewControllerWeakReference.Unwrap()?.ShowMoreActionSheet(indexPath, shortcodePreview);
                });
                moreAction.BackgroundColor = Theme.DarkerBlue;
                actions.Add(moreAction);

                return actions.ToArray();
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                if (tableView.Editing)
                    return;

                var sp = items[indexPath.Section][indexPath.Row];
                viewControllerWeakReference.Unwrap()?.ShortcodeSelected(tableView, indexPath, sp);
            }

            public void AppendItems(IEnumerable<ShortcodePreview> shortcodePreviews)
            {
                loading = false;

                var count = items.Count;
                var isInputListPopulated = shortcodePreviews.Any();

                if (isInputListPopulated)
                    items.Add(shortcodePreviews.ToList());

                if (count == 0)
                    tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                else if (isInputListPopulated)
                    tableViewWeakReference.Unwrap()?.InsertSections(NSIndexSet.FromIndex(items.Count - 1), UITableViewRowAnimation.Fade);
            }

            public void RemoveItems(IEnumerable<int> shortcodeIds)
            {
                tableViewWeakReference.Unwrap()?.BeginUpdates();

                var indexPaths = shortcodeIds.Select(id => FindItemIndexPath(id)).Where(idx => idx != null).OrderByDescending(idx => idx.Section).ThenByDescending(idx => idx.Row).ToList();
                foreach (var indexPath in indexPaths)
                {
                    items[indexPath.Section].RemoveAt(indexPath.Row);
                    if (!items[indexPath.Section].Any())
                    {
                        items.RemoveAt(indexPath.Section);
                        if (items.Count == 0)
                            tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                        else
                            tableViewWeakReference.Unwrap()?.DeleteSections(NSIndexSet.FromIndex(indexPath.Section), UITableViewRowAnimation.Automatic);
                    }
                    else
                        tableViewWeakReference.Unwrap()?.DeleteRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);
                }

                tableViewWeakReference.Unwrap()?.EndUpdates();
            }

            public void UpdateItem(ShortcodePreview sp)
            {
                var indexPath = FindItemIndexPath(sp);
                if (indexPath != null)
                {
                    items[indexPath.Section][indexPath.Row] = sp;
                    tableViewWeakReference.Unwrap()?.ReloadRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);
                }
            }

            public void Reset()
            {
                loading = true;

                items.Clear();

                var sectionsCount = tableViewWeakReference.Unwrap()?.NumberOfSections() ?? 0;

                tableViewWeakReference.Unwrap()?.BeginUpdates();
                if (sectionsCount > 1)
                    tableViewWeakReference.Unwrap()?.DeleteSections(NSIndexSet.FromNSRange(new NSRange(1, sectionsCount - 1)), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.EndUpdates();
            }

            public ShortcodePreview FindItemAtIndexPath(NSIndexPath indexPath) => items[indexPath.Section][indexPath.Row];

            public NSIndexPath FindItemIndexPath(ShortcodePreview sp) => FindItemIndexPath(sp.Id);

            public NSIndexPath FindItemIndexPath(int id)
            {
                for (var section = 0; section < items.Count; section++)
                    for (var row = 0; row < items[section].Count; row++)
                        if (items[section][row].Id == id)
                            return NSIndexPath.FromRowSection(row, section);

                return null;
            }
        }
    }
}