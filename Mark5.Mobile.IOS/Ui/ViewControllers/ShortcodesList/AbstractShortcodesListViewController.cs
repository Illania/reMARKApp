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
using Mark5.Mobile.IOS.Model.HubMessages;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;
using ObjCRuntime;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.ShortcodesList
{
    public abstract class AbstractShortcodesListViewController : AbstractViewController, IPrimaryViewController, IUISearchResultsUpdating, IUIGestureRecognizerDelegate
    {
        protected readonly bool DisableRowActions;

        public Folder Folder { get; set; }

        protected UIBarButtonItem ExitEditItem;
        protected UIBarButtonItem EditItem;

        protected UIRefreshControl RefreshControl;
        protected UITableView TableView;
        protected UISearchController SearchController;
        protected UITableViewController SearchResultsController;
        protected DataSource SearchResultsDataSource;

        protected CancellationTokenSource searchCancellationTokenSource;
        readonly List<CancellationTokenSource> searchCancellationTokenSourceList = new List<CancellationTokenSource>();

        bool refreshing;

        protected CancellationTokenSource cts;

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

            ReachabilityBar.Attach(View, TableView, (float) NavigationController.BottomLayoutGuide.Length, UITextAlignment.Left);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info($"{nameof(AbstractShortcodesListViewController)} appeared");

            var ds = (DataSource) TableView.Source;
            if (ds.Empty)
                RefreshData();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeinitializeHandlers();

            cts?.Cancel();
        }

        public override void WillMoveToParentViewController(UIViewController parent)
        {
            base.WillMoveToParentViewController(parent);

            if (parent == null && SplitViewController != null && !SplitViewController.Collapsed)
            {
                var nc = (UINavigationController) SplitViewController.ViewControllers[1];
                nc.PopToRootViewController(false);

                var vc = (ShortcodeViewController) nc.ViewControllers[0];
                vc.ClearData();
            }
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning($"{nameof(AbstractShortcodesListViewController)} received memory warning!");

            var ds = TableView?.Source as DataSource;
            ds?.Reset();

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        #endregion

        #region Initialization

        void InitializeNavigationBar()
        {
            ExitEditItem = new UIBarButtonItem(UIBarButtonSystemItem.Done);
            EditItem = new UIBarButtonItem(UIBarButtonSystemItem.Edit);
        }

        void InitializeView()
        {
            AutomaticallyAdjustsScrollViewInsets = true;

            TableView = new UITableView
            {
                ClipsToBounds = false,
                AllowsSelectionDuringEditing = false,
                AllowsMultipleSelectionDuringEditing = true,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            TableView.Source = new DataSource(this, TableView, Localization.GetString("folder_empty"), DisableRowActions);
            View.AddSubview(TableView);
            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(TableView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(TableView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(TableView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(TableView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1f, 0f)
            });

            var longPressRecognizer = new UILongPressGestureRecognizer(this, new Selector("longPressed:"))
            {
                MinimumPressDuration = 1f,
                Delegate = this
            };
            TableView.AddGestureRecognizer(longPressRecognizer);

            RefreshControl = new UIRefreshControl
            {
                BackgroundColor = UIColor.White
            };
            TableView.AddSubview(RefreshControl);
        }

        void InitializeSearchBar()
        {
            DefinesPresentationContext = true;

            SearchResultsController = new UITableViewController();
            SearchResultsDataSource = new DataSource(this, SearchResultsController.TableView, Localization.GetString("no_matching_shortcodes"), DisableRowActions);
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

        void SubscribeToMessages()
        {
            PlatformConfig.MessengerHub.Subscribe<EntityRemovedFromFolderMessage>(HandleRemovedFromFolder, m => m.ObjectType == ObjectType.Shortcode);
            PlatformConfig.MessengerHub.Subscribe<EntityMovedFromFolderMessage>(HandleMovedFromFolder, m => m.ObjectType == ObjectType.Shortcode);
            PlatformConfig.MessengerHub.Subscribe<EntityDeletedMessage>(HandleDeleted, m => m.ObjectType == ObjectType.Shortcode);
        }

        void InitializeNavigationBarTitle()
        {
            UIView.AnimationsEnabled = false;
            NavigationItem.Title = Folder.Name;
            NavigationItem.Prompt = null;
            UIView.AnimationsEnabled = true;
        }

        void InitializeHandlers()
        {
            if (ExitEditItem != null)
                ExitEditItem.Clicked += ExitEditItem_Clicked;

            if (EditItem != null)
                EditItem.Clicked += EditItem_Clicked;

            if (RefreshControl != null)
                RefreshControl.ValueChanged += RefreshControl_ValueChanged;
        }

        void DeinitializeHandlers()
        {
            if (ExitEditItem != null)
                ExitEditItem.Clicked -= ExitEditItem_Clicked;

            if (EditItem != null)
                EditItem.Clicked -= EditItem_Clicked;

            if (RefreshControl != null)
                RefreshControl.ValueChanged -= RefreshControl_ValueChanged;
        }

        #endregion

        #region Actions

        public virtual void ShortcodeSelected(UITableView tableView, ShortcodePreview shortcodePreview)
        {

        }

        [Export("longPressed:")]
        public void LongPressed(UILongPressGestureRecognizer recognizer)
        {
            if (TableView.Editing)
                return;

            StartEditing();

            var point = recognizer.LocationInView(TableView);
            var indexPath = TableView.IndexPathForRowAtPoint(point);

            TableView.SelectRow(indexPath, true, UITableViewScrollPosition.None);
        }

        void StartEditing()
        {
            TableView.SetEditing(true, true);
            NavigationItem.SetRightBarButtonItem(ExitEditItem, true);
            NavigationItem.SetLeftBarButtonItem(EditItem, true);

            SearchController.SearchBar.UserInteractionEnabled = false;
            SearchController.SearchBar.Alpha = .5f;

            if (SplitViewController != null && !SplitViewController.Collapsed)
            {
                var nc = (UINavigationController) SplitViewController.ViewControllers[1];
                nc.PopToRootViewController(false);

                var vc = (ShortcodeViewController) nc.ViewControllers[0];
                vc.ClearData();
            }
        }

        void ExitEditItem_Clicked(object sender, EventArgs e)
        {
            EndEditing();
        }

        void EndEditing()
        {
            TableView.SetEditing(false, true);
            NavigationItem.SetRightBarButtonItem(null, true);
            NavigationItem.SetLeftBarButtonItem(NavigationItem.BackBarButtonItem, true);

            SearchController.SearchBar.UserInteractionEnabled = true;
            SearchController.SearchBar.Alpha = 1f;
        }

        void EditItem_Clicked(object sender, EventArgs e)
        {
            if (TableView.IndexPathsForSelectedRows == null || TableView.IndexPathsForSelectedRows.Length < 1)
                return;

            var eas = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);

            var rows = TableView.IndexPathsForSelectedRows.ToArray();
            var selectedShortcodes = rows.Select(ip => ((DataSource) TableView.Source).FindItemAtIndexPath(ip)).ToList();

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
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete_from_folder"), UIAlertActionStyle.Default, a => RemoveFromFolder(selectedShortcodes)));

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator || ServerConfig.SystemSettings.ShortcodesModuleInfo.Permissions.DeleteAllowed)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete"), UIAlertActionStyle.Destructive, a => Delete(selectedShortcodes)));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, a => ExitEditItem.Enabled = true));

            if (eas.PopoverPresentationController != null)
                eas.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate((UIBarButtonItem) sender);

            ExitEditItem.Enabled = false;
            PresentViewController(eas, true, null);
        }

        #endregion

        #region Refreshing

        void RefreshControl_ValueChanged(object sender, EventArgs e)
        {
            RefreshData(forceClear: true);
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void RefreshData(int startRowId = -1, bool forceClear = false)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            if (refreshing)
                return;

            refreshing = true;
            RefreshControl.ValueChanged -= RefreshControl_ValueChanged;

            if (forceClear && await Managers.FoldersManager.IsSavedFolderOfflineInfo(Folder))
            {
                var result = await Dialogs.ShowYesNoCancelDialogAsync(this,
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
                    NavigationController.PresentViewController(new NavigationController(vc, UIModalPresentationStyle.FormSheet), true, null);
                    await vc.DidDisappear;
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
            {
                var ds = (DataSource) TableView.Source;
                ds.Reset();
            }

            var sourceType = await Managers.FoldersManager.IsSavedFolderOfflineInfo(Folder) ? SourceType.Local : SourceType.Auto;

            Managers.ShortcodesManager.GetAllShortcodePreviews(Folder,
                sps =>
                {
                    InvokeOnMainThread(() =>
                    {
                        var ds = (DataSource) TableView.Source;
                        ds.AppendItems(sps);
                    });
                },
                () =>
                {
                    RefreshControl.EndRefreshing();
                    RefreshControl.ValueChanged += RefreshControl_ValueChanged;

                    refreshing = false;

                    CommonConfig.Logger.Info($"Refresh finished");
#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
                },
                async ex =>
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
                {
                    RefreshControl.EndRefreshing();
                    RefreshControl.ValueChanged += RefreshControl_ValueChanged;

                    refreshing = false;

                    CommonConfig.Logger.Error($"Could not refresh shortcodes [folder={Folder?.Name}, startRowId={startRowId}, forceClear={forceClear}]", ex);

                    await Dialogs.ShowErrorDialogAsync(this, ex);

                    NavigationController?.PopViewController(true);
                },
                startRowId,
                cts.Token,
                sourceType);
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

                DoSearchShortcodes(searchText, searchCancellationTokenSource.Token);
            }
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void DoSearchShortcodes(string searchText, CancellationToken ct)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            SearchResultsDataSource.Reset();

            await Task.Delay(500);

            if (ct.IsCancellationRequested)
                return;

            var ds = (DataSource) TableView.Source;
            var filteredShortcodes = ds.Items.Where(sp => MatchesQuery(sp, searchText)).ToList();

            if (ct.IsCancellationRequested)
                return;

            SearchResultsDataSource.AppendItems(filteredShortcodes);
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

        #region Actions

        void RemoveFromFolder(ShortcodePreview selectedShortcode)
        {
            RemoveFromFolder(new List<ShortcodePreview>
            {
                selectedShortcode
            });
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void RemoveFromFolder(List<ShortcodePreview> selectedShortcodes)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            var result = await Dialogs.ShowYesNoDialogAsync(this, Localization.GetString("delete_from_folder"), Localization.GetString("confirm_delete_from_folder_shortcodes"));

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
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        void Delete(ShortcodePreview selectedShortcode)
        {
            Delete(new List<ShortcodePreview>
            {
                selectedShortcode
            });
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void Delete(List<ShortcodePreview> selectedShortcodes)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            var result = await Dialogs.ShowYesNoDialogAsync(this, Localization.GetString("delete"), Localization.GetString("confirm_delete_shortcodes"));

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
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        void CopyToWorktray(ShortcodePreview shortcodePreview)
        {
            CopyToWorktray(new List<ShortcodePreview>
            {
                shortcodePreview
            });
        }

        void CopyToWorktray(List<ShortcodePreview> shortcodePreviews)
        {
            var vc = new CopyToWorktrayViewController
            {
                BusinessEntities = shortcodePreviews.Cast<IBusinessEntity>().ToList()
            };
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        void CopyToFolder(ShortcodePreview shortcodePreview)
        {
            CopyToFolder(new List<ShortcodePreview>
            {
                shortcodePreview
            });
        }

        void CopyToFolder(List<ShortcodePreview> shortcodePreviews)
        {
            var vc = new CopyMoveToFolderListViewController(shortcodePreviews.Cast<IBusinessEntity>().ToList());
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        void MoveToFolder(ShortcodePreview shortcodePreview)
        {
            MoveToFolder(new List<ShortcodePreview>
            {
                shortcodePreview
            });
        }

        void MoveToFolder(List<ShortcodePreview> shortcodePreviews)
        {
            var vc = new CopyMoveToFolderListViewController(shortcodePreviews.Cast<IBusinessEntity>().ToList(), Folder);
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        void DoShowMoreActionSheet(NSIndexPath indexPath, ShortcodePreview selectedShortcode)
        {
            var eas = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);

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
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete_from_folder"), UIAlertActionStyle.Default, a => RemoveFromFolder(selectedShortcode)));

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator || ServerConfig.SystemSettings.ShortcodesModuleInfo.Permissions.DeleteAllowed)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete"), UIAlertActionStyle.Destructive, a => Delete(selectedShortcode)));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, a => EndEditing()));

            if (eas.PopoverPresentationController != null)
                eas.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate(TableView, TableView.CellAt(indexPath));

            PresentViewController(eas, true, null);
        }

        #endregion

        #region Messages handlers

        void HandleRemovedFromFolder(EntityRemovedFromFolderMessage m)
        {
            RemoveShortcodesFromList(m.EntitiesId);
        }

        void HandleMovedFromFolder(EntityMovedFromFolderMessage m)
        {
            RemoveShortcodesFromList(m.EntitiesId);
        }

        void HandleDeleted(EntityDeletedMessage m)
        {
            RemoveShortcodesFromList(m.EntitiesId);
        }

        #endregion

        #region Utilities

        void RemoveShortcodesFromList(IEnumerable<int> ids)
        {
            if (SearchController.Active)
                SearchResultsDataSource.RemoveItems(ids.ToList());

            var ds = (DataSource) TableView.Source;
            ds.RemoveItems(ids.ToList());
            if (SplitViewController != null && !SplitViewController.Collapsed)
            {
                var nc = (UINavigationController) SplitViewController.ViewControllers[1];
                var vc = (ShortcodeViewController) nc.ViewControllers[0];
                if (ids.Select(id => vc.IsShowingShortcodeWithId(id)).Any(v => v))
                    vc.ClearData();
            }
        }

        #endregion

        protected class DataSource : UITableViewSource, IDisposable
        {
            public bool Empty { get { return !shortcodePreviewsInView.SelectMany(v => v).Any(); } }

            public IEnumerable<ShortcodePreview> Items { get { return shortcodePreviewsInView.SelectMany(i => i); } }

            AbstractShortcodesListViewController viewController;
            UITableView tableView;
            readonly string emptyText;

            bool loading = true;
            bool disableRowActions;
            List<List<ShortcodePreview>> shortcodePreviewsInView = new List<List<ShortcodePreview>>(25);

            public DataSource(AbstractShortcodesListViewController viewController, UITableView tableView, string emptyText, bool disableRowActions)
            {
                this.viewController = viewController;
                this.tableView = tableView;
                this.emptyText = emptyText;
                this.disableRowActions = disableRowActions;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as WaitTableViewCell ?? WaitTableViewCell.Create();

                if (!shortcodePreviewsInView.SelectMany(v => v).Any())
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                    emptyCell.Initialize(emptyText);
                    return emptyCell;
                }

                var cp = shortcodePreviewsInView[indexPath.Section][indexPath.Row];

                var cell = tableView.DequeueReusableCell(ShortcodesTableViewCell.Key) as ShortcodesTableViewCell ?? ShortcodesTableViewCell.Create();
                cell.Initialize(cp);

                return cell;
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                if (loading)
                    return 1;

                if (!shortcodePreviewsInView.SelectMany(v => v).Any())
                    return 1;

                return shortcodePreviewsInView.Count;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading)
                    return 1;

                if (!shortcodePreviewsInView.SelectMany(v => v).Any())
                    return 1;

                return shortcodePreviewsInView[(int) section].Count;
            }

            public override string[] SectionIndexTitles(UITableView tableView)
            {
                return shortcodePreviewsInView.SelectMany(i => i).Select(cp => cp.Name.SafeSubstring(0, 1).ToUpper()).Distinct().ToArray();
            }

            public override nint SectionFor(UITableView tableView, string title, nint atIndex)
            {
                for (var section = 0; section < shortcodePreviewsInView.Count; section++)
                {
                    var row = shortcodePreviewsInView[section].FindIndex(cp => cp.Name.SafeSubstring(0, 1).ToUpper() == title);
                    if (row >= 0)
                    {
                        tableView.ScrollToRow(NSIndexPath.FromRowSection(row, section), UITableViewScrollPosition.Top, true);
                        break;
                    }
                }

                return -1;
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return ShortcodesTableViewCell.Height;
            }

            public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
            {
                return !disableRowActions;
            }

            public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
            {
                var actions = new List<UITableViewRowAction>();

                var shortcodePreview = shortcodePreviewsInView[indexPath.Section][indexPath.Row];

                var moreAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("more"), (a, ip) => { viewController.DoShowMoreActionSheet(indexPath, shortcodePreview); });
                moreAction.BackgroundColor = Theme.DarkerBlue;
                actions.Add(moreAction);

                var copyToWorktrayAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Default,
                    Localization.GetString("copy_to_worktray_ml"),
                    (a, ip) =>
                    {
                        viewController.CopyToWorktray(shortcodePreview);
                        viewController.EndEditing();
                    });
                copyToWorktrayAction.BackgroundColor = Theme.DarkBlue;
                actions.Add(copyToWorktrayAction);

                return actions.ToArray();
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                if (tableView.Editing)
                    return;

                var cp = shortcodePreviewsInView[indexPath.Section][indexPath.Row];
                viewController.ShortcodeSelected(tableView, cp);
            }

            public void AppendItems(List<ShortcodePreview> shortcodePreviews)
            {
                loading = false;

                var count = shortcodePreviewsInView.Count;
                var isInputListPopulated = shortcodePreviews.Any();

                if (isInputListPopulated)
                    shortcodePreviewsInView.Add(shortcodePreviews);

                if (count == 0)
                    tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                else if (isInputListPopulated)
                    tableView.InsertSections(NSIndexSet.FromIndex(shortcodePreviewsInView.Count - 1), UITableViewRowAnimation.Fade);
            }

            public void RemoveItems(List<int> shortcodeIds)
            {
                tableView.BeginUpdates();

                var indexPaths = shortcodeIds.Select(id => FindItemIndexPath(id)).Where(idx => idx != null).OrderByDescending(idx => idx.Section).ThenByDescending(idx => idx.Row).ToList();
                foreach (var indexPath in indexPaths)
                {
                    shortcodePreviewsInView[indexPath.Section].RemoveAt(indexPath.Row);
                    if (!shortcodePreviewsInView[indexPath.Section].Any())
                    {
                        shortcodePreviewsInView.RemoveAt(indexPath.Section);
                        if (shortcodePreviewsInView.Count == 0)
                            tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                        else
                            tableView.DeleteSections(NSIndexSet.FromIndex(indexPath.Section), UITableViewRowAnimation.Automatic);
                    }
                    else
                    {
                        tableView.DeleteRows(new NSIndexPath[]
                            {
                                indexPath
                            },
                            UITableViewRowAnimation.Automatic);
                    }
                }

                tableView.EndUpdates();
            }

            public void Reset()
            {
                loading = true;

                var count = shortcodePreviewsInView.Count;

                shortcodePreviewsInView.Clear();

                tableView.BeginUpdates();
                tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);

                if (count > 1)
                    tableView.DeleteSections(NSIndexSet.FromNSRange(new NSRange(1, count - 1)), UITableViewRowAnimation.Fade);

                tableView.EndUpdates();
            }

            public NSIndexPath FindItemIndexPath(ShortcodePreview sp)
            {
                return FindItemIndexPath(sp.Id);
            }

            public NSIndexPath FindItemIndexPath(int id)
            {
                for (var section = 0; section < shortcodePreviewsInView.Count; section++)
                    for (var row = 0; row < shortcodePreviewsInView[section].Count; row++)
                        if (shortcodePreviewsInView[section][row].Id == id)
                            return NSIndexPath.FromRowSection(row, section);

                return null;
            }

            public ShortcodePreview FindItemAtIndexPath(NSIndexPath indexPath)
            {
                return shortcodePreviewsInView[indexPath.Section][indexPath.Row];
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                viewController = null;
                tableView = null;
                shortcodePreviewsInView = null;
            }
        }
    }
}