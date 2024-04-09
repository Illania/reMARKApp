using Foundation;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Extensions;
using reMark.Mobile.Common.Manager;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Model.HubMessages;
using reMark.Mobile.Common.Service;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.IOS.Ui.Common;
using reMark.Mobile.IOS.Ui.ViewControllers.ComposeDocumentView;
using reMark.Mobile.IOS.Ui.ViewControllers.DocumentView;
using reMark.Mobile.IOS.Utilities;
using TinyMessenger;
using UIKit;

namespace reMark.Mobile.IOS.Ui.ViewControllers
{
    public class DocumentsListViewController : AbstractTableViewController, IPrimaryViewController,
        IUISearchResultsUpdating, IUIViewControllerRestoration
    {
        const int AutoRefreshIntervalMs = 5 * 1000;

        public Folder Folder { get; set; }
        public bool DisableRowActions { get; set; }
        public bool OnlyShowExternalDocuments { get; set; }
        public bool OnlyShowUnreadDocuments {get; set;}

        public UITableViewController? SearchResultsController => 
            (UITableViewController?)(searchController?.SearchResultsController);

        UIBarButtonItem selectAllItem;
        UIBarButtonItem goToBookmarkItem;
        UIBarButtonItem exitEditItem;
        UIBarButtonItem editItem;
        SearchResultsController searchResultsController;

        bool refreshing;
        bool selectAllEnabled;

        protected UISearchController searchController;
        CancellationTokenSource searchCancellationTokenSource;
        readonly List<CancellationTokenSource> searchCancellationTokenSourceList = new List<CancellationTokenSource>();
        string lastSearchQuery;

        AutoRefreshWorker autoRefreshWorker;
        Action newDocumentsAvailableAction;

        TinyMessageSubscriptionToken priorityChangedToken;
        TinyMessageSubscriptionToken readStatusChangedToken;
        TinyMessageSubscriptionToken commentsCountChangedToken;
        TinyMessageSubscriptionToken categoriesChangedToken;
        TinyMessageSubscriptionToken removedFromFolderToken;
        TinyMessageSubscriptionToken movedFromFolderToken;
        TinyMessageSubscriptionToken deletedToken;
        TinyMessageSubscriptionToken goToDocumentToken;

        bool compactList = PlatformConfig.Preferences.CompactDocumentsList;

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

            RestorationIdentifier = nameof(DocumentsListViewController);
            RestorationClass = Class;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            NavigationItem.Title = Folder?.Name;

            if (Integration.IsRunningAtLeast(11))
            {
                if (NavigationController?.NavigationBar != null)
                    NavigationController.NavigationBar.PrefersLargeTitles = true;

                if (NavigationItem != null)
                    NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Automatic;
            }

            InitializeHandlers();

            if (TableView?.IndexPathForSelectedRow != null)
                TableView.DeselectRow(TableView.IndexPathForSelectedRow, true);

            if (TableView?.IndexPathsForSelectedRows?.Length > 0)
                foreach (var selectedIndexPath in TableView?.IndexPathsForSelectedRows)
                    TableView.DeselectRow(selectedIndexPath, true);

            if (searchController?.SearchResultsController is UITableViewController searchTableViewController)
            {
                if (searchTableViewController?.TableView?.IndexPathForSelectedRow != null)
                    searchTableViewController.TableView.DeselectRow(searchTableViewController?.TableView.IndexPathForSelectedRow, true);

                if (searchTableViewController?.TableView?.IndexPathsForSelectedRows?.Length > 0)
                    foreach (var selectedIndexPath in searchTableViewController.TableView?.IndexPathsForSelectedRows)
                        searchTableViewController.TableView.DeselectRow(selectedIndexPath, true);
            }

            ReachabilityBar.Attach(this);
            SendStatusBanner.Attach(this);
        }

        public override async void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info($"{nameof(DocumentsListViewController)} appeared");

            if (((DocumentListDataSource)TableView.Source).Empty)
                await RefreshData();


            CommonConfig.Logger.Info($"Starting automatic refresh...");

            autoRefreshWorker?.Stop();
            autoRefreshWorker = new AutoRefreshWorker(AutoRefreshData, () => {
                var items = ((DocumentListDataSource)TableView.Source).Items.ToArray();

                return items.Any()
                    ? MoreLinq.MoreEnumerable.MaxBy(items, i => i.Id).FirstOrDefault()
                    : default;
            },
                AutoRefreshIntervalMs);
            autoRefreshWorker.Start();

             //Avoid refresh control being stuck
            var endRefreshTimer = NSTimer.CreateRepeatingScheduledTimer(TimeSpan.FromSeconds(5.0), delegate {
                RefreshControl.EndRefreshing();
            });

            if (!Integration.IsRunningAtLeast(11))
                return;

            NSOperationQueue.MainQueue.AddOperation(() =>
            {
                var ni = NavigationItem;

                if (ParentViewController is UIViewController && !(ParentViewController is UINavigationController))
                    ni = ParentViewController?.NavigationItem;

                if (ni.SearchController == null)
                    ni.SearchController = searchController;
            });
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeinitializeHandlers();

            autoRefreshWorker?.Stop();
            autoRefreshWorker?.Dispose();
            autoRefreshWorker = null;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            ReachabilityBar.Detach(this);
            SendStatusBanner.Detach(this);
        }

        public override void WillMoveToParentViewController(UIViewController parent)
        {
            base.WillMoveToParentViewController(parent);

            if (parent == null && SplitViewController != null && !SplitViewController.Collapsed)
            {
                var nc = (UINavigationController)SplitViewController.ViewControllers[1];
                nc.PopToRootViewController(false);

                var vc = (DocumentPageViewController)nc.ViewControllers[0];
                vc.ClearPage();
            }
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning("Received memory warning!");

            autoRefreshWorker?.Stop();
            autoRefreshWorker = null;

            ((DocumentListDataSource)TableView.Source)?.Reset();

            UnsubscribeFromMessages();

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        protected override void Recycle()
        {
            base.Recycle();

            goToBookmarkItem = null;
            selectAllItem = null;
            exitEditItem = null;
            editItem = null;

            UnsubscribeFromMessages();

            autoRefreshWorker?.Stop();
            autoRefreshWorker = null;
            newDocumentsAvailableAction = null;

            searchCancellationTokenSource?.Dispose();
            searchCancellationTokenSource = null;
            searchCancellationTokenSourceList.ForEach(cts => cts?.Dispose());
            searchCancellationTokenSourceList.Clear();

            TableView.GestureRecognizers.ForEach(TableView.RemoveGestureRecognizer);
            ((DocumentListDataSource)TableView.Source)?.Reset();

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
            goToBookmarkItem = new UIBarButtonItem
            {
                Image = UIImage.FromBundle("Bookmark")
            };
            NavigationItem.SetRightBarButtonItem(goToBookmarkItem, false);


            if (!DisableRowActions)
            {
                selectAllItem = new UIBarButtonItem
                {
                    Image = UIImage.FromBundle("SelectAll")
                };

                exitEditItem = new UIBarButtonItem(UIBarButtonSystemItem.Done);
                editItem = new UIBarButtonItem(UIBarButtonSystemItem.Edit);
            }
        }

        public void ShowBookmarkButton() => NavigationItem.SetRightBarButtonItem(goToBookmarkItem, false);
        public void HideBookmarkButton() => NavigationItem.SetRightBarButtonItem(null, false);

        void InitializeView()
        {
            RefreshControl = new UIRefreshControl();
            TableView.Source = new DocumentListDataSource(this, TableView, Localization.GetString("folder_empty"), PlatformConfig.Preferences.CompactDocumentsList, DisableRowActions);
            TableView.RefreshControl = RefreshControl;
            TableView.AllowsMultipleSelectionDuringEditing = true;
            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 60f;

            TableView.AddGestureRecognizer(new UILongPressGestureRecognizer(DocumentPreviewLongPressed));
        }

        void InitializeSearchBar()
        {
            DefinesPresentationContext = true;

            searchResultsController = new SearchResultsController() {
                DisableRowActions = false,
                DocumentListViewController = this,
                Folder = Folder
            };

            searchController = new(searchResultsController)
            {
                HidesNavigationBarDuringPresentation = false,
                ObscuresBackgroundDuringPresentation = true,
                SearchResultsUpdater = this,
            };
            searchController.SearchBar.Placeholder = Localization.GetString("filter");

            if (!Integration.IsRunningAtLeast(11))
                TableView.TableHeaderView = searchController.SearchBar;
        }

        public override bool CanBecomeFirstResponder => true;
        public override UIKeyCommand[] KeyCommands {
            get
            {
                if (Integration.IsiOSApplicationOnMac())
                {
                    return new UIKeyCommand[] { UIKeyCommand.Create((NSString)" ", 0, new ObjCRuntime.Selector("spacebarPressed")) };
                }
                else
                    return base.KeyCommands;
            }
        }

        [Export("spacebarPressed")]
        private void OnSpacebarPressed()
        {
            var selectedRowIp = TableView.IndexPathsForSelectedRows?.FirstOrDefault();

            if (selectedRowIp == null)
                return;

            var selectedDocument = ((DocumentListDataSource)TableView.Source).Items[selectedRowIp.Row];
            MarkAsRead(selectedDocument);

            if (selectedRowIp.Row == ((DocumentListDataSource)TableView.Source).Items.Count - 1)
                return;

            var nextRowIndex = NSIndexPath.FromRowSection(selectedRowIp.Row + 1, 0);
            //if next row is not yet visible scroll it at the top of the view
            var nextRowScrollPosition = UITableViewScrollPosition.Top;
            //if row is visible don't scroll
            if (TableView.IndexPathsForVisibleRows.Contains(nextRowIndex))
                nextRowScrollPosition = UITableViewScrollPosition.None;

            TableView.SelectRow(nextRowIndex, true, nextRowScrollPosition);
            var dp = ((DocumentListDataSource)TableView.Source).Items[nextRowIndex.Row];
            DocumentSelected(dp);
        }

        void InitializeHandlers()
        {

            if (goToBookmarkItem != null)
                goToBookmarkItem.Clicked += GoToBookmarkItem_Clicked;

            InitializeEditModeHandlers();

            if (RefreshControl != null)
                RefreshControl.ValueChanged += RefreshControl_ValueChanged;
        }

        public void InitializeEditModeHandlers()
        {
            if (exitEditItem != null)
                exitEditItem.Clicked += ExitEditItem_Clicked;

            if (selectAllItem != null)
                selectAllItem.Clicked += SelectAllItem_Clicked;

            if (editItem != null)
                editItem.Clicked += EditItem_Clicked;
        }

        public void DeinitializeEditModeHandlers()
        {
            if (exitEditItem != null)
                exitEditItem.Clicked -= ExitEditItem_Clicked;

            if (selectAllItem != null)
                selectAllItem.Clicked -= SelectAllItem_Clicked;

            if (editItem != null)
                editItem.Clicked -= EditItem_Clicked;
        }

        void DeinitializeHandlers()
        {

            DeinitializeEditModeHandlers();

            if (RefreshControl != null)
                RefreshControl.ValueChanged -= RefreshControl_ValueChanged;
        }

        public void InitializeEditModeActions(EventHandler editItemHandler, EventHandler exitEditItemHandler, EventHandler selectAllItemHandler)
        {
            if (exitEditItem != null)
                exitEditItem.Clicked += exitEditItemHandler;

            if (selectAllItem != null)
                selectAllItem.Clicked += selectAllItemHandler;

            if (editItem != null)
                editItem.Clicked += editItemHandler;
        }

        public void DeinitializeEditModeActions(EventHandler editItemHandler, EventHandler exitEditItemHandler, EventHandler selectAllItemHandler)
        {
            if (exitEditItem != null)
                exitEditItem.Clicked -= exitEditItemHandler;

            if (selectAllItem != null)
                selectAllItem.Clicked -= selectAllItemHandler;

            if (editItem != null)
                editItem.Clicked -= editItemHandler;
        }



        #endregion

        #region Subscribe/unsubscribe

        void SubscribeToMessages()
        {
            readStatusChangedToken = CommonConfig.MessengerHub.Subscribe<DocumentPreviewReadStatusChangedMessage>(ReadStatusChangedHandler);
            priorityChangedToken = CommonConfig.MessengerHub.Subscribe<DocumentPreviewPriorityChangedMessage>(PriorityChangedHandler);
            commentsCountChangedToken = CommonConfig.MessengerHub.Subscribe<EntityPreviewCommentCountChangedMessage>(CommentsCountChangedHandler);
            categoriesChangedToken = CommonConfig.MessengerHub.Subscribe<EntityCategoriesChangedMessage>(CategoriesChangedHandler, m => m.ObjectType == ObjectType.Document);
            removedFromFolderToken = CommonConfig.MessengerHub.Subscribe<EntityRemovedFromFolderMessage>(HandleRemovedFromFolder, m => m.ObjectType == ObjectType.Document);
            movedFromFolderToken = CommonConfig.MessengerHub.Subscribe<EntityMovedFromFolderMessage>(HandleMovedFromFolder, m => m.ObjectType == ObjectType.Document);
            deletedToken = CommonConfig.MessengerHub.Subscribe<EntityRemovedMessage>(HandleDeleted, m => m.ObjectType == ObjectType.Document);
            goToDocumentToken = CommonConfig.MessengerHub.Subscribe<GoToDocumentMessage>(HandleAction);
        }

        void UnsubscribeFromMessages()
        {
            readStatusChangedToken?.Dispose();
            priorityChangedToken?.Dispose();
            commentsCountChangedToken?.Dispose();
            categoriesChangedToken?.Dispose();
            removedFromFolderToken?.Dispose();
            movedFromFolderToken?.Dispose();
            deletedToken?.Dispose();
            goToDocumentToken?.Dispose();
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

        void ExitEditItem_Clicked(object sender, EventArgs e) => EndEditing(TableView);

        void EditItem_Clicked(object sender, EventArgs e)
        {
            if (TableView.IndexPathsForSelectedRows == null || TableView.IndexPathsForSelectedRows.Length < 1)
                return;

            var eas = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            var d = new PopoverPresentationControllerDelegate((UIBarButtonItem)sender);

            var rows = TableView.IndexPathsForSelectedRows.ToArray();
            var selectedDocuments = rows.Select(ip => ((DocumentListDataSource)TableView.Source).Items[ip.Row]).ToList();

            if (selectedDocuments.Any(dp => !dp.IsReadByCurrent))
                eas.AddAction(UIAlertAction.Create(Localization.GetString("mark_as_read"),
                    UIAlertActionStyle.Default,
                    a =>
                    {
                        MarkAsRead(selectedDocuments);
                        EndEditing(TableView);
                    }));

            if (selectedDocuments.Any(dp => dp.IsReadByCurrent))
                eas.AddAction(UIAlertAction.Create(Localization.GetString("mark_as_unread"),
                    UIAlertActionStyle.Default,
                    a =>
                    {
                        MarkAsUnread(selectedDocuments);
                        EndEditing(TableView);
                    }));

            if (ServerConfig.SystemSettings.DocumentsModuleInfo.WorktrayEnabled ?? true)
            {
                eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_worktray"),
                                                   UIAlertActionStyle.Default,
                                                   a =>
                {
                    this.CopyToWorktray(selectedDocuments.Select(be => (IBusinessEntity)be).ToList());
                    EndEditing(TableView);
                }));
            }

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_folder"),
                UIAlertActionStyle.Default,
                a =>
                {
                    this.CopyToFolder(selectedDocuments.Select(be => (IBusinessEntity)be).ToList());
                    EndEditing(TableView);
                }));

            if (PlatformConfig.Preferences.EnableMoveToFolder
                && (Folder.InternalType == FolderInternalType.FilterView
                || Folder.InternalType == FolderInternalType.Static
                || Folder.InternalType == FolderInternalType.Worktray))
            {
                eas.AddAction(UIAlertAction.Create(Localization.GetString("move_to_folder"),
                      UIAlertActionStyle.Default,
                      a =>
                      {
                          this.MoveToFolder(selectedDocuments.Select(be => (IBusinessEntity)be).ToList(), Folder);
                          EndEditing(TableView);
                      }));
            }

            eas.AddAction(UIAlertAction.Create(Localization.GetString("set_priority"), UIAlertActionStyle.Default, a => ShowPriorityActionSheet(selectedDocuments, (UIBarButtonItem)sender)));

            if (Folder.InternalType == FolderInternalType.FilterView || Folder.InternalType == FolderInternalType.Static || Folder.InternalType == FolderInternalType.Worktray)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete_from_folder"),
                    UIAlertActionStyle.Default,
                    a => RemoveFromFolder(selectedDocuments, d)));

            if (DocumentsDeleteChecker.CanDeleteDocuments(selectedDocuments))
            {
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete"),
                    UIAlertActionStyle.Destructive,
                    a => Delete(selectedDocuments, d)));
            }

            if (ServerConfig.SystemSettings?.SystemInfo?.DelaySendAvailable == true && selectedDocuments.Any(dp => dp.TransmitStatus == TransmitStatus.Delayed))
            {
                eas.AddAction(UIAlertAction.Create(Localization.GetString("send_now"),
                   UIAlertActionStyle.Default,
                   a =>
                   {
                       ForceSend(selectedDocuments);
                       EndEditing(TableView);
                   }));

                eas.AddAction(UIAlertAction.Create(Localization.GetString("cancel_send"),
                  UIAlertActionStyle.Default,
                  a =>
                  {
                      CancelSend(selectedDocuments);
                      EndEditing(TableView);
                  }));
            }

            if (selectedDocuments.Count == 1)
            {
                if (!PlatformConfig.Preferences.HasBookmarkForFolder(Folder.Id, selectedDocuments.FirstOrDefault().Id))
                {
                    eas.AddAction(UIAlertAction.Create(
                                     Localization.GetString("add_bookmark"),
                                     UIAlertActionStyle.Default,
                                     a => AddBookmark(selectedDocuments.FirstOrDefault())));

                }
                else
                {
                    eas.AddAction(UIAlertAction.Create(
                                       Localization.GetString("remove_bookmark"),
                                       UIAlertActionStyle.Default,
                                       a => RemoveBookmark(selectedDocuments.FirstOrDefault())));
                }

                eas.AddAction(UIAlertAction.Create(
                                 Localization.GetString("set_preset_category"),
                                 UIAlertActionStyle.Default,
                                 a => AssignPresetCategory(selectedDocuments.FirstOrDefault())));
            }

            eas.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, a => exitEditItem.Enabled = true));

            if (eas.PopoverPresentationController != null)
                eas.PopoverPresentationController.Delegate = d;

            PresentViewController(eas, true, null);
        }

        void GoToBookmarkItem_Clicked(object sender, EventArgs e)
        {
            var bookmarkForFolderId = PlatformConfig.Preferences.GetBookmarkForFolder(Folder.Id);
            if (bookmarkForFolderId > 0)
            {
                var index = ((DocumentListDataSource)TableView.Source).Items.FindIndex(dp => dp.Id == bookmarkForFolderId);

                if (index >= 0)
                {
                    TableView.SelectRow(NSIndexPath.FromRowSection(index, 0), true, UITableViewScrollPosition.None);
                    TableView.ScrollToRow(NSIndexPath.FromRowSection(index, 0), UITableViewScrollPosition.None, true);
                }
            }
        }

        void SelectAllItem_Clicked(object sender, EventArgs e)
        {
            if (selectAllEnabled)
            {
                SelectAll(TableView);
            }
            else
            {
                DeselectAll(TableView);
            }

            selectAllEnabled = !selectAllEnabled;

        }

        public void SetExitEditItemEnabled(bool enabled)
        {
            exitEditItem.Enabled = enabled;
        }

        void SearchControllerExitEditItem_Clicked(object sender, EventArgs e) => EndEditing(TableView);

        void SearchControllerEditItem_Clicked(object sender, EventArgs e)
        {
            var tableView = searchResultsController.TableView;
            if (tableView.IndexPathsForSelectedRows == null || tableView.IndexPathsForSelectedRows.Length < 1)
                return;

            var eas = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            var d = new PopoverPresentationControllerDelegate((UIBarButtonItem)sender);

            var rows = tableView.IndexPathsForSelectedRows.ToArray();
            var selectedDocuments = rows.Select(ip => ((DocumentListDataSource)tableView.Source).Items[ip.Row]).ToList();

            if (selectedDocuments.Any(dp => !dp.IsReadByCurrent))
                eas.AddAction(UIAlertAction.Create(Localization.GetString("mark_as_read"),
                    UIAlertActionStyle.Default,
                    a =>
                    {
                        MarkAsRead(selectedDocuments);
                        EndEditing(TableView);
                    }));

            if (selectedDocuments.Any(dp => dp.IsReadByCurrent))
                eas.AddAction(UIAlertAction.Create(Localization.GetString("mark_as_unread"),
                    UIAlertActionStyle.Default,
                    a =>
                    {
                        MarkAsUnread(selectedDocuments);
                        EndEditing(TableView);
                    }));

            if (ServerConfig.SystemSettings.DocumentsModuleInfo.WorktrayEnabled ?? true)
            {
                eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_worktray"),
                                                   UIAlertActionStyle.Default,
                                                   a =>
                                                   {
                                                       this.CopyToWorktray(selectedDocuments.Select(be => (IBusinessEntity)be).ToList());
                                                       EndEditing(TableView);
                                                   }));
            }

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_folder"),
                UIAlertActionStyle.Default,
                a =>
                {
                    this.CopyToFolder(selectedDocuments.Select(be => (IBusinessEntity)be).ToList());
                    EndEditing(TableView);
                }));

            if (Folder.InternalType == FolderInternalType.FilterView || Folder.InternalType == FolderInternalType.Static || Folder.InternalType == FolderInternalType.Worktray)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("move_to_folder"),
                    UIAlertActionStyle.Default,
                    a =>
                    {
                        this.MoveToFolder(selectedDocuments.Select(be => (IBusinessEntity)be).ToList(), Folder);
                        EndEditing(TableView);
                    }));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("set_priority"), UIAlertActionStyle.Default, a => ShowPriorityActionSheet(selectedDocuments, (UIBarButtonItem)sender)));

            if (Folder.InternalType == FolderInternalType.FilterView || Folder.InternalType == FolderInternalType.Static || Folder.InternalType == FolderInternalType.Worktray)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete_from_folder"),
                    UIAlertActionStyle.Default,
                    a => RemoveFromFolder(selectedDocuments, d)));

            if (DocumentsDeleteChecker.CanDeleteDocuments(selectedDocuments))
            {
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete"),
                    UIAlertActionStyle.Destructive,
                    a => Delete(selectedDocuments, d)));
            }

            if (ServerConfig.SystemSettings?.SystemInfo?.DelaySendAvailable == true && selectedDocuments.Any(dp => dp.TransmitStatus == TransmitStatus.Delayed))
            {
                eas.AddAction(UIAlertAction.Create(Localization.GetString("send_now"),
                   UIAlertActionStyle.Default,
                   a =>
                   {
                       ForceSend(selectedDocuments);
                       EndEditing(TableView);
                   }));

                eas.AddAction(UIAlertAction.Create(Localization.GetString("cancel_send"),
                  UIAlertActionStyle.Default,
                  a =>
                  {
                      CancelSend(selectedDocuments);
                      EndEditing(TableView);
                  }));
            }


            eas.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, a => exitEditItem.Enabled = true));

            if (eas.PopoverPresentationController != null)
                eas.PopoverPresentationController.Delegate = d;

            PresentViewController(eas, true, null);
        }


        #endregion

        #region Refreshing

        async void RefreshControl_ValueChanged(object sender, EventArgs e)
        {
            CommonConfig.UsageAnalytics.LogEvent(new PullToRefreshEvent(false, ModuleType.Documents));
            await RefreshData(forceClear: true);
        }

        public async Task RefreshData(int startId = -1, int endId = -1, bool forceClear = false)
        {
            if (refreshing)
                return;

            refreshing = true;
            RefreshControl.ValueChanged -= RefreshControl_ValueChanged;

            CommonConfig.Logger.Info($"Refreshing documents list [folder={Folder?.Name}, startId={startId}, endId={endId}, forceClear={forceClear}]");

            try
            {
                if (forceClear)
                    ((DocumentListDataSource)TableView.Source)?.Reset();

                var documentPreviews = await Managers.DocumentsManager.GetDocumentPreviewsAsync(Folder, startId, endId);


                if (OnlyShowExternalDocuments)
                    documentPreviews = documentPreviews.FindAll(dp => dp.Direction == DocumentDirection.External);
                else if (OnlyShowUnreadDocuments)
                    documentPreviews = documentPreviews.FindAll(dp => dp.IsReadByCurrent == false);
                
                ((DocumentListDataSource)TableView.Source).LoadMoreEnabled = documentPreviews.Count >= PlatformConfig.Preferences.DocumentsToDownload;
                CommonConfig.Logger.Info($"Enable load more documents set to {((DocumentListDataSource)TableView.Source).LoadMoreEnabled}");

                Services.DocumentsDownloadService.Notify();

                if (PlatformConfig.Preferences.SortByDate)
                    ((DocumentListDataSource)TableView.Source).InsertItems(documentPreviews);
                else
                    ((DocumentListDataSource)TableView.Source).AppendItems(documentPreviews);

                if (documentPreviews.Any())
                    newDocumentsAvailableAction?.Invoke();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Could not refresh folders [folder={Folder?.Name}, startId={startId}, endId={endId}, forceClear={forceClear}]", ex);

                await Dialogs.ShowErrorAlertAsync(this, ex);

                NavigationController?.PopViewController(true);
            }
            finally
            {           
                RefreshControl.EndRefreshing();
                RefreshControl.ValueChanged += RefreshControl_ValueChanged;
                refreshing = false;
            }
        }

        async Task AutoRefreshData(int endId)
        {
            if (refreshing)
                return;

            refreshing = true;

            RefreshControl.Enabled = false;

            try
            {
                CommonConfig.Logger.Debug($"Attempting automatic refresh [endId={endId}, isBeingDismissed={IsBeingDismissed}]...");

                if (IsBeingDismissed)
                    return;

                CommonConfig.Logger.Debug($"Automatic refresh running...");

                var documents = await Managers.DocumentsManager.GetDocumentPreviewsAsync(Folder, endId: endId);

                if (OnlyShowUnreadDocuments)
                    documents = documents.FindAll(dp => dp.IsReadByCurrent == false);

                if (documents.Count > 0)
                {
                    CommonConfig.Logger.Info($"Received {documents?.Count} new documents");

                    Services.DocumentsDownloadService.Notify();

                    ((DocumentListDataSource)TableView.Source).PrependItems(documents);

                    newDocumentsAvailableAction?.Invoke();
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Automatic refresh failed [endId={endId}]", ex);
            }
            finally
            {
                CommonConfig.Logger.Debug($"Automatic refresh finished");
            }

            RefreshControl.Enabled = true;
            refreshing = false;
        }

        #endregion

        #region List handlers

        public virtual void DocumentSelected(DocumentPreview documentPreview)
        {
            if (SplitViewController != null && !SplitViewController.Collapsed)
            {
                var nc = (UINavigationController)SplitViewController.ViewControllers[1];
                nc.PopToViewController(nc.ViewControllers[0], false);

                var vc = (DocumentPageViewController)nc.ViewControllers[0];
                vc.DocumentPreviews = ((DocumentListDataSource)TableView.Source).Items;

                if (vc.IsShowingDocumentWithId(documentPreview.Id))
                    return;

                vc.HidesBottomBarWhenPushed = false;
                vc.SetPage(Folder, documentPreview, searchController.Active);
            }
            else
            {
                if (searchController.Active)
                {
                    var vc = new DocumentViewController();
                    vc.SetData(Folder, documentPreview);
                    vc.SetRefreshDataOnAppear();
                    newDocumentsAvailableAction = null;
                    NavigationController.PushViewController(vc, true);
                }
                else
                {
                    var vc = new DocumentPageViewController
                    {
                        Folder = Folder,
                        InitialDocumentPreview = documentPreview,
                        DocumentPreviews = ((DocumentListDataSource)TableView.Source).Items
                    };
                    newDocumentsAvailableAction = null;
                    NavigationController.PushViewController(vc, true);
                }
            }
        }

        void DocumentPreviewLongPressed(UILongPressGestureRecognizer recognizer)
        {
            if (TableView.Editing || ((DocumentListDataSource)TableView.Source).Empty || DisableRowActions)
                return;

            StartEditing(TableView);

            var point = recognizer.LocationInView(TableView);
            var indexPath = TableView.IndexPathForRowAtPoint(point);

            if (indexPath == null || (!TableView.CellAt(indexPath)?.UserInteractionEnabled ?? true))
                return;

            TableView.SelectRow(indexPath, true, UITableViewScrollPosition.None);
        }

        #endregion

        #region Actions

        public void ShowMoreActionSheet(NSIndexPath indexPath, DocumentPreview selectedDocument, Folder folder)
        {
            var alertController = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);

            var popoverDelegate = new PopoverPresentationControllerDelegate(TableView, TableView.CellAt(indexPath));

            List<EmailSwipeAction> availableSwipeActions = PlatformConfig.Preferences.GetAvailableSwipeActions();

            foreach (var swipeAction in availableSwipeActions)
            {
                var title = SwipeActionTitle(swipeAction.Action, selectedDocument);
                var actionAllowed = SwipeActionAllowed(swipeAction.Action, selectedDocument, folder);
                if (actionAllowed && swipeAction.Action != EmailSwipeAction.SwipeAction.More)
                {
                    UIAlertActionStyle actionStyle = swipeAction.Action == EmailSwipeAction.SwipeAction.More ? UIAlertActionStyle.Destructive : UIAlertActionStyle.Default;
                    alertController.AddAction(UIAlertAction.Create(title, actionStyle, a => OnSwipeActionClick(swipeAction, indexPath, selectedDocument, folder, TableView)));
                }
            }

            alertController.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, null));

            if (alertController.PopoverPresentationController != null)
                alertController.PopoverPresentationController.Delegate = popoverDelegate;

            PresentViewController(alertController, true, null);
        }

        public async void ShowPriorityActionSheet(List<DocumentPreview> selectedDocuments, UIBarButtonItem barButtonItem)
        {
            var priorities = new[] { Priority.Low, Priority.Normal, Priority.Urgent };
            var priorityStrings = priorities.Select(p => UI.PrettyPriorityString(p)).ToArray();
            var result = await Dialogs.ShowListActionSheetAsync(this, priorityStrings, barButtonItem);

            if (result < 0)
                return;

            var priority = priorities[result];

            await SetPriority(selectedDocuments, priority);
        }

        public async void ShowPriorityActionSheet(DocumentPreview selectedDocument, UITableView tv, UITableViewCell cell)
        {
            var priorities = new List<Priority> { Priority.Low, Priority.Normal, Priority.Urgent };
            var priorityStrings = priorities.Select(p => UI.PrettyPriorityString(p));
            var result = await Dialogs.ShowListActionSheetAsync(this, priorityStrings.ToArray(), tv, cell);

            if (result < 0)
                return;

            var priority = priorities[result];

            await SetPriority(new List<DocumentPreview> { selectedDocument }, priority);
        }

        public async void ShowPriorityActionSheet(DocumentPreview selectedDocument, UITableView tv, NSIndexPath indexPath)
        {

            var cell = TableView.CellAt(indexPath);
            var priorities = new List<Priority> { Priority.Low, Priority.Normal, Priority.Urgent };
            var priorityStrings = priorities.Select(p => UI.PrettyPriorityString(p));
            var result = await Dialogs.ShowListActionSheetAsync(this, priorityStrings.ToArray(), tv, cell);

            if (result < 0)
                return;

            var priority = priorities[result];

            await SetPriority(new List<DocumentPreview> { selectedDocument }, priority);
        }

        public async Task SetPriority(List<DocumentPreview> selectedDocuments, Priority priority)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("setting_priority___"));

            try
            {
                CommonConfig.Logger.Info($"Attempting to setting priority for documents");
                await Managers.DocumentsManager.SetDocumentsPriorityAsync(selectedDocuments, priority);

                var updatedItems = selectedDocuments.Select(d => d.Id);
                ((DocumentListDataSource)TableView.Source).UpdateItems(updatedItems);
                ((DocumentListDataSource)SearchResultsController?.TableView?.Source)?.UpdateItems(updatedItems);

                EndEditing(TableView);

                UpdatePriorityForDocument(selectedDocuments.Select(d => d.Id));

                dismissAction();
            }
            catch (Exception ex)
            {
                EndEditing(TableView);
                dismissAction();

                CommonConfig.Logger.Error($"Error while setting priority for documents", ex);
                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        public void SelectAll(UITableView tableView)
        {
            var dataSource = (DocumentListDataSource)tableView.Source;
            var currentSection = 0;
            var rowsInSection = dataSource.RowsInSection(null, currentSection);

            for (int i = 0; i < rowsInSection; i++)
            {
                var path = NSIndexPath.FromItemSection(i, currentSection);
                TableView.SelectRow(path, false, UITableViewScrollPosition.None);
            }

            selectAllItem.Image = UIImage.FromBundle("DeselectAll");
        }

        public void DeselectAll(UITableView tableView)
        {

            var dataSource = (DocumentListDataSource)TableView.Source;
            var currentSection = 0;
            var rowsInSection = dataSource.RowsInSection(null, currentSection);

            for (int i = 0; i < rowsInSection; i++)
            {
                var path = NSIndexPath.FromItemSection(i, currentSection);
                TableView.DeselectRow(path, false);
            }
            selectAllItem.Image = UIImage.FromBundle("SelectAll");
        }

        public void RemoveFromFolder(DocumentPreview selectedDocument, UIPopoverPresentationControllerDelegate d) =>
            RemoveFromFolder(new List<DocumentPreview> { selectedDocument }, d, true);

        public async void RemoveFromFolder(List<DocumentPreview> selectedDocuments, UIPopoverPresentationControllerDelegate d, bool fromSwipe = false)
        {
            if (fromSwipe && PlatformConfig.Preferences.ConfirmRemoveSwipe)
            {
                var result = await Dialogs.ShowDestructiveActionSheetAsync(this, Localization.GetString("delete_from_folder"), d);
                if (!result)
                {
                    EndEditing(TableView);
                    return;
                }
            }
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("deleting_from_folder___"));

            try
            {
                CommonConfig.Logger.Info($"Attempting to remove documents from folder [folderId={Folder.Id}]");
                await Managers.CommonActionsManager.RemoveFromFolder(selectedDocuments.Cast<IBusinessEntity>().ToList(), Folder);

                RemoveDocumentsFromList(selectedDocuments.Select(s => s.Id));
                EndEditing(TableView);

                dismissAction();
            }
            catch (Exception ex)
            {
                EndEditing(TableView);
                dismissAction();

                CommonConfig.Logger.Error($"Error while removing documents from folder [folderId={Folder.Id}]", ex);
                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        public void Delete(DocumentPreview selectedDocument, UIPopoverPresentationControllerDelegate d) =>
            Delete(new List<DocumentPreview> { selectedDocument }, d);

        public async void Delete(List<DocumentPreview> selectedDocuments, UIPopoverPresentationControllerDelegate d)
        {
            var result = await Dialogs.ShowDestructiveActionSheetAsync(this, Localization.GetString("delete"), d, Localization.GetString("confirm_deletion"));
            if (!result)
            {
                EndEditing(TableView);
                return;
            }

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("deleting___"));

            try
            {
                CommonConfig.Logger.Info($"Attempting to delete documents");

                await Managers.CommonActionsManager.Delete(selectedDocuments.Cast<IBusinessEntity>().ToList());

                RemoveDocumentsFromList(selectedDocuments.Select(s => s.Id));
                EndEditing(TableView);

                dismissAction();
            }
            catch (Exception ex)
            {
                EndEditing(TableView);
                dismissAction();

                CommonConfig.Logger.Error($"Error while deleting documents", ex);
                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        public void ShowCategories(DocumentPreview selectedDocument)
        {
            if (!ServerConfig.SystemSettings.SystemInfo.FavoriteCategoriesAvailable)
            {
                PresentViewController(new NavigationController(
                    new CategoriesListOldViewController(selectedDocument), UIModalPresentationStyle.PageSheet), true, null);

            }
            else
            {
                PresentViewController(new NavigationController(
                    new CategoriesListViewController(selectedDocument), UIModalPresentationStyle.PageSheet), true, null);
            }
        }


        public void MarkAsRead(DocumentPreview documentPreview) =>
            MarkAsRead(new List<DocumentPreview> { documentPreview });

        public virtual async void MarkAsRead(List<DocumentPreview> documentPreviews)
        {
            CommonConfig.Logger.Info($"Attempting to mark as read [documentPreviews={documentPreviews.Count}]...");

            try
            {
                CommonConfig.UsageAnalytics.LogEvent(new SetReadStatusEvent(documentPreviews.Count));

                await Managers.DocumentsManager.SetDocumentsReadStatusAsync(documentPreviews, true);

                if (PlatformConfig.Preferences.SyncUserActivities)
                    await Managers.DocumentsManager.ExecuteUserActivity(UserActivityType.Read, documentPreviews);

                var updatedItems = documentPreviews.Select(d => d.Id);
                ((DocumentListDataSource)TableView.Source).UpdateItems(updatedItems);
                ((DocumentListDataSource)SearchResultsController?.TableView?.Source)?.UpdateItems(updatedItems);

            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Marking as read failed [documentPreviews.Count={documentPreviews.Count}]", ex);

                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        public void MarkAsUnread(DocumentPreview documentPreview) =>
            MarkAsUnread(new List<DocumentPreview> { documentPreview });

        public async void MarkAsUnread(List<DocumentPreview> documentPreviews)
        {
            CommonConfig.Logger.Info($"Attempting to mark as unread [documentPreviews={documentPreviews.Count}]...");

            try
            {
                CommonConfig.UsageAnalytics.LogEvent(new SetReadStatusEvent(documentPreviews.Count));

                await Managers.DocumentsManager.SetDocumentsReadStatusAsync(documentPreviews, false);

                var updatedItems = documentPreviews.Select(d => d.Id);
                ((DocumentListDataSource)TableView.Source).UpdateItems(updatedItems);
                ((DocumentListDataSource)SearchResultsController?.TableView?.Source)?.UpdateItems(updatedItems);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Marking as unread failed [documentPreviews.Count={documentPreviews.Count}]", ex);

                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        public async void ForceSend(List<DocumentPreview> documentPreviews)
        {
            var delayedItems = documentPreviews.Where(i => i.TransmitStatus == TransmitStatus.Delayed).ToList();

            CommonConfig.Logger.Info($"Attempting to force send delayed items [businessEntities.Count={delayedItems.Count}]...");

            try
            {
                CommonConfig.UsageAnalytics.LogEvent(new ForceSendEvent());

                await Managers.DocumentsManager.ForceSendDocument(delayedItems);
                var updatedItemsIds = delayedItems.Select(d => d.Id);
                ((DocumentListDataSource)TableView.Source).UpdateItems(updatedItemsIds);
                ((DocumentListDataSource)SearchResultsController?.TableView?.Source)?.UpdateItems(updatedItemsIds);

                CommonConfig.Logger.Info($"Documents with IDs:{string.Join(",", delayedItems.Select(i => i.Id).ToList()).TrimEnd(',')} forced sent.");

                foreach (var item in delayedItems)
                    CommonConfig.MessengerHub.Publish(new DocumentUploadStatusChangedMessage(this, DocumentUploadStatusChangedMessage.Status.DocumentSent,
                        Guid.Empty, false));

            }
            catch (Exception ex)
            {

                CommonConfig.Logger.Error($"Force send failed [businessEntities.Count={delayedItems.Count}]", ex);

                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        public async void CancelSend(List<DocumentPreview> documentPreviews)
        {
            var delayedItems = documentPreviews.Where(i => i.TransmitStatus == TransmitStatus.Delayed).ToList();

            CommonConfig.Logger.Info($"Attempting to cancel send delayed items [businessEntities.Count={delayedItems.Count}]...");

            try
            {
                CommonConfig.UsageAnalytics.LogEvent(new CancelSendEvent());

                await Managers.DocumentsManager.CancelSendDocument(delayedItems);
                var updatedItemsIds = delayedItems.Select(d => d.Id);
                ((DocumentListDataSource)TableView.Source).UpdateItems(updatedItemsIds);
                ((DocumentListDataSource)SearchResultsController?.TableView?.Source)?.UpdateItems(updatedItemsIds);

                foreach (var item in delayedItems)
                    CommonConfig.MessengerHub.Publish(new DocumentUploadStatusChangedMessage(this, DocumentUploadStatusChangedMessage.Status.DocumentSendCancelled,
                        Guid.Empty, false));

            }
            catch (Exception ex)
            {

                CommonConfig.Logger.Error($"Cancel send failed [businessEntities.Count={delayedItems.Count}]", ex);

                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        #endregion

        #region Searching

        void IUISearchResultsUpdating.UpdateSearchResultsForSearchController(UISearchController searchController)
        {
            var searchText = searchController.SearchBar.Text;

            if (!searchController.Active)
                CommonConfig.UsageAnalytics.LogEvent(new FilterEvent(false, ModuleType.Documents));

            if (!searchController.Active || string.IsNullOrWhiteSpace(searchText))
            {
                searchCancellationTokenSourceList.ForEach(cts => cts?.Cancel());
                searchCancellationTokenSourceList.Clear();

                var dataSource = SearchResultsController?.TableView.Source;
                ((DocumentListDataSource)dataSource)?.Reset();
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

                if (searchText == lastSearchQuery)
                    return;

                lastSearchQuery = searchText;

                searchResultsController.SearchDocuments(searchText, searchCancellationTokenSource.Token);
            }
        }

        #endregion

        #region Messages handlers

        protected virtual void ReadStatusChangedHandler(DocumentPreviewReadStatusChangedMessage message)
        {
            BeginInvokeOnMainThread(() =>
            {
                foreach (var tableView in new UITableView[] { TableView, SearchResultsController?.TableView })
                {
                    if (tableView == null || tableView.Source == null)
                        continue;

                    var index = ((DocumentListDataSource)tableView.Source).Items.FindIndex(dp => dp.Id == message.DocumentPreviewId);

                    if (index >= 0)
                    {
                        var documentPreview = ((DocumentListDataSource)tableView.Source).Items[index];

                        documentPreview.IsReadByCurrent = message.IsReadByCurrent;
                        documentPreview.IsReadByAnyone = message.IsReadByAnyone;

                        var selectedRow = tableView.IndexPathForSelectedRow;

                        tableView.ReloadRows(new[] { NSIndexPath.FromRowSection(index, 0) }, UITableViewRowAnimation.Fade);

                        if (selectedRow != null)
                            tableView.SelectRow(selectedRow, false, UITableViewScrollPosition.None);
                    }

                }

            });

        }

        void PriorityChangedHandler(DocumentPreviewPriorityChangedMessage message)
        {
            BeginInvokeOnMainThread(() =>
            {
                foreach (var tableView in new UITableView[] { TableView, ((UITableViewController)searchController?.SearchResultsController)?.TableView })
                {
                    if (tableView == null || tableView.Source == null)
                        continue;

                    var index = ((DocumentListDataSource)tableView.Source).Items.FindIndex(dp => dp.Id == message.DocumentPreviewId);

                    if (index >= 0)
                    {
                        var documentPreview = ((DocumentListDataSource)tableView.Source).Items[index];
                        documentPreview.Priority = message.Priority;

                        var selectedRow = tableView.IndexPathForSelectedRow;

                        tableView.ReloadRows(new[] { NSIndexPath.FromRowSection(index, 0) }, UITableViewRowAnimation.Fade);

                        if (selectedRow != null)
                            tableView.SelectRow(selectedRow, false, UITableViewScrollPosition.None);
                    }

                }

            });

        }

        void CommentsCountChangedHandler(EntityPreviewCommentCountChangedMessage message)
        {
            BeginInvokeOnMainThread(() =>
            {
                foreach (var tableView in new UITableView[] { TableView, SearchResultsController?.TableView })
                {
                    if (tableView == null || tableView.Source == null)
                        continue;

                    var index = ((DocumentListDataSource)tableView.Source).Items.FindIndex(dp => dp.Id == message.EntityId);

                    if (index >= 0)
                    {
                        var documentPreview = ((DocumentListDataSource)tableView.Source).Items[index];
                        documentPreview.CommentsCount = message.CommentsCount;

                        var selectedRow = tableView.IndexPathForSelectedRow;

                        tableView.ReloadRows(new[] { NSIndexPath.FromRowSection(index, 0) }, UITableViewRowAnimation.Fade);

                        if (selectedRow != null)
                            tableView.SelectRow(selectedRow, false, UITableViewScrollPosition.None);
                    }
                }
            });
        }

        void CategoriesChangedHandler(EntityCategoriesChangedMessage message)
        {
            BeginInvokeOnMainThread(() =>
            {
                foreach (var tableView in new UITableView[] { TableView, ((UITableViewController)searchController?.SearchResultsController)?.TableView })
                {
                    if (tableView == null || tableView.Source == null)
                        continue;

                    var index = ((DocumentListDataSource)tableView.Source).Items.FindIndex(dp => dp.Id == message.EntityId);

                    if (index >= 0)
                    {
                        var documentPreview = ((DocumentListDataSource)tableView.Source).Items[index];
                        documentPreview.Categories.Clear();
                        documentPreview.Categories.AddRange(message.Categories);

                        var selectedRow = tableView.IndexPathForSelectedRow;

                        tableView.ReloadRows(new[] { NSIndexPath.FromRowSection(index, 0) }, UITableViewRowAnimation.Fade);

                        if (selectedRow != null)
                            tableView.SelectRow(selectedRow, false, UITableViewScrollPosition.None);
                    }
                }
            });
        }

        void HandleRemovedFromFolder(EntityRemovedFromFolderMessage m) => RemoveDocumentsFromList(m.EntitiesId);

        void HandleMovedFromFolder(EntityMovedFromFolderMessage m) => RemoveDocumentsFromList(m.EntitiesId);

        void HandleDeleted(EntityRemovedMessage m) => RemoveDocumentsFromList(m.EntitiesId);

        void HandleAction(GoToDocumentMessage message)
        {
            BeginInvokeOnMainThread(() =>
            {
                foreach (var tableView in new UITableView[] { TableView, SearchResultsController?.TableView })
                {
                    if (tableView == null || tableView.Source == null)
                        continue;

                    var index = ((DocumentListDataSource)tableView.Source).Items.FindIndex(dp => dp.Id == message.DocumentId);

                    if (index >= 0)
                    {
                        tableView.SelectRow(NSIndexPath.FromRowSection(index, 0), true, UITableViewScrollPosition.None);
                        tableView.ScrollToRow(NSIndexPath.FromRowSection(index, 0), UITableViewScrollPosition.None, true);
                    }
                }
            });
        }

        #endregion

        #region Utilities
        public static bool SwipeActionAllowed(EmailSwipeAction.SwipeAction action, DocumentPreview documentPreview, Folder folder)
        {
            switch (action)
            {
                case EmailSwipeAction.SwipeAction.CopyToWorkTray:
                    return ServerConfig.SystemSettings.DocumentsModuleInfo.WorktrayEnabled ?? true;
                case EmailSwipeAction.SwipeAction.MoveToFolder:
                    return folder.InternalType == FolderInternalType.FilterView || folder.InternalType == FolderInternalType.Static || folder.InternalType == FolderInternalType.Worktray;
                case EmailSwipeAction.SwipeAction.Delete:
                    return DocumentsDeleteChecker.CanDeleteDocuments(
                        new List<DocumentPreview> { documentPreview });
                case EmailSwipeAction.SwipeAction.RemoveFromFolder:
                    return folder.InternalType == FolderInternalType.FilterView || folder.InternalType == FolderInternalType.Static || folder.InternalType == FolderInternalType.Worktray;
                default:
                    return true;
            }
        }

        public void OnSwipeActionClick(EmailSwipeAction swipeAction, NSIndexPath indexPath, DocumentPreview documentPreview, Folder folder, UITableView tableView)
        {
            CommonConfig.UsageAnalytics.LogEvent(new SwipeActionUsedEvent());

            var popoverDelegate = new PopoverPresentationControllerDelegate(tableView, tableView.CellAt(indexPath));

            if (SwipeActionAllowed(swipeAction.Action, documentPreview, folder))
            {
                switch (swipeAction.Action)
                {
                    case EmailSwipeAction.SwipeAction.MarkAsRead:
                        if (documentPreview.IsReadByCurrent)
                        {
                            MarkAsUnread(documentPreview);
                            EndEditing(TableView);
                        }
                        else
                        {
                            MarkAsRead(documentPreview);
                            EndEditing(TableView);
                        }
                        break;
                    case EmailSwipeAction.SwipeAction.CopyToWorkTray:
                        this.CopyToWorktray(documentPreview);
                        EndEditing(TableView);
                        break;
                    case EmailSwipeAction.SwipeAction.More:
                        ShowMoreActionSheet(indexPath, documentPreview, folder);
                        break;
                    case EmailSwipeAction.SwipeAction.CopyToFolder:
                        this.CopyToFolder(documentPreview);
                        EndEditing(TableView);
                        break;
                    case EmailSwipeAction.SwipeAction.Categories:
                        ShowCategories(documentPreview);
                        EndEditing(TableView);
                        break;
                    case EmailSwipeAction.SwipeAction.SetPriority:
                        ShowPriorityActionSheet(documentPreview, tableView, indexPath);
                        break;
                    case EmailSwipeAction.SwipeAction.SetPresetCategory:
                        AssignPresetCategory(documentPreview);
                        break;
                    case EmailSwipeAction.SwipeAction.AddBookmark:
                        if (!PlatformConfig.Preferences.HasBookmarkForFolder(Folder.Id, documentPreview.Id))
                            AddBookmark(documentPreview);
                        else
                            RemoveBookmark(documentPreview);
                        break;
                    case EmailSwipeAction.SwipeAction.Delete:
                        Delete(documentPreview, popoverDelegate);
                        break;
                    case EmailSwipeAction.SwipeAction.MoveToFolder:
                        this.MoveToFolder(documentPreview, Folder);
                        break;
                    case EmailSwipeAction.SwipeAction.RemoveFromFolder:
                        RemoveFromFolder(documentPreview, popoverDelegate);
                        break;
                    case EmailSwipeAction.SwipeAction.DeliveryReport:
                        ShowDeliveryReport(documentPreview);
                        break;
                    default:
                        CommonConfig.Logger.Error("Missed case for EmailSwipeAction : " + swipeAction.Action.ToString());
                        break;
                }
            }
        }

        void ShowDeliveryReport(DocumentPreview dp)
        {
            var vc = new TransmitDestinationsViewController() { DocumentId = dp.Id, ReferenceNumber = dp.ReferenceNumber };
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }
       

        Category presetCategory;

        async void AssignPresetCategory(DocumentPreview documentPreview)
        {
            if (presetCategory == null)
            {
                var categories = await Managers.DocumentsManager.GetAllCategoriesAsync();
                presetCategory = categories.FirstOrDefault(c => c.Id == PlatformConfig.Preferences.PresetCategoryId);
                if (presetCategory == null)
                    return;
            }

            var oldCategories = documentPreview.Categories;
            var newCategories = oldCategories.Union(new List<Category> { presetCategory }).ToList();
            CommonConfig.Logger.Info($"Attempting to assign preset category " +
                                     $"[documentPreview.Id={documentPreview.Id}]...");

            try
            {
                await Managers.CommonActionsManager.SetCategoriesAsync(documentPreview, newCategories);
                ((DocumentListDataSource)TableView.Source)
                    .UpdateItems(new List<int> { documentPreview.Id });
                ((DocumentListDataSource)((UITableViewController)searchController?.SearchResultsController)
                    ?.TableView.Source)?.UpdateItems(new List<int> { documentPreview.Id });
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Assigning preset category for " +
                                          $"[documentPreview.Id={documentPreview.Id}] failed", ex);
                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        async void AddBookmark(DocumentPreview documentPreview)
        {
            
            CommonConfig.Logger.Info($"Attempting to add bookmark for folder Id= {Folder.Id} [documentPreview.Id={documentPreview.Id}]...");

            try
            {
                var previousBookmarkedDocId = PlatformConfig.Preferences.GetBookmarkForFolder(Folder.Id);
                var itemsToUpdate = new List<int> { documentPreview.Id };
                if (previousBookmarkedDocId > 0)
                    itemsToUpdate.Add(previousBookmarkedDocId);
                PlatformConfig.Preferences.SetBookmarkForFolder(Folder.Id, documentPreview.Id);
                ((DocumentListDataSource)TableView.Source).UpdateItems(itemsToUpdate);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Adding bookmark for folder Id= {Folder.Id} [documentPreview.Id={documentPreview.Id}] failed", ex);

                await Dialogs.ShowErrorAlertAsync(this, ex);
            }

        }

        async void RemoveBookmark(DocumentPreview documentPreview)
        {

            CommonConfig.Logger.Info($"Attempting to remove bookmark for folder Id= {Folder.Id} [documentPreview.Id={documentPreview.Id}]...");

            try
            {
                PlatformConfig.Preferences.RemoveBookmarkForFolder(Folder.Id);
                ((DocumentListDataSource)TableView.Source).UpdateItems(new List<int> { documentPreview.Id });
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Removing bookmark for folder Id= {Folder.Id} [documentPreview.Id={documentPreview.Id}] failed", ex);

                await Dialogs.ShowErrorAlertAsync(this, ex);
            }

        }

        public string SwipeActionTitle(EmailSwipeAction.SwipeAction swipeAction, DocumentPreview documentPreview)
        {
            switch (swipeAction)
            {
                case EmailSwipeAction.SwipeAction.MarkAsRead:
                    return documentPreview.IsReadByCurrent ? Localization.GetString("mark_as_unread_ml") : Localization.GetString("mark_as_read_ml");
                case EmailSwipeAction.SwipeAction.CopyToWorkTray:
                    return Localization.GetString("copy_to_worktray_ml");
                case EmailSwipeAction.SwipeAction.More:
                    return Localization.GetString("more");
                case EmailSwipeAction.SwipeAction.CopyToFolder:
                    return Localization.GetString("copy_to_folder");
                case EmailSwipeAction.SwipeAction.Categories:
                    return Localization.GetString("categories");
                case EmailSwipeAction.SwipeAction.SetPriority:
                    return Localization.GetString("set_priority");
                case EmailSwipeAction.SwipeAction.Delete:
                    return Localization.GetString("delete");
                case EmailSwipeAction.SwipeAction.MoveToFolder:
                    return Localization.GetString("move_to_folder");
                case EmailSwipeAction.SwipeAction.RemoveFromFolder:
                    return Localization.GetString("delete_from_folder");
                case EmailSwipeAction.SwipeAction.SetPresetCategory:
                    return Localization.GetString("set_preset_category");
                case EmailSwipeAction.SwipeAction.DeliveryReport:
                    return Localization.GetString("delivery_report");
                case EmailSwipeAction.SwipeAction.AddBookmark:
                    if(!PlatformConfig.Preferences.HasBookmarkForFolder(Folder.Id, documentPreview.Id))
                        return Localization.GetString("add_bookmark");
                    else
                        return Localization.GetString("remove_bookmark");
                default:
                    CommonConfig.Logger.Error($"Missing implementation for EmailSwipeAction : {swipeAction}");
                    return "";
            }
        }

        public void StartEditing(UITableView tableView)
        {
            if (SplitViewController != null && !SplitViewController.Collapsed)
            {
                var nc = (UINavigationController)SplitViewController.ViewControllers[1];
                nc.PopToRootViewController(false);

                var vc = (DocumentPageViewController)nc.ViewControllers[0];
                vc.ClearPage();
            }

            tableView.SetEditing(true, true);
            NavigationItem.SetRightBarButtonItems(new[] { exitEditItem, selectAllItem }, true);
            NavigationItem.SetLeftBarButtonItem(editItem, true);

            selectAllEnabled = true;
            selectAllItem.Image = UIImage.FromBundle("SelectAll");
        }

        public void EndEditing(UITableView tableView)
        {
            tableView.SetEditing(false, true);
            NavigationItem.SetRightBarButtonItems(new[] { goToBookmarkItem }, false);
            NavigationItem.SetLeftBarButtonItem(NavigationItem.BackBarButtonItem, true);
        }

        void UpdatePriorityForDocument(IEnumerable<int> ids)
        {
            BeginInvokeOnMainThread(() =>
            {
                if (SplitViewController != null && !SplitViewController.Collapsed)
                {
                    var nc = (UINavigationController)SplitViewController.ViewControllers[1];
                    var vc = (DocumentPageViewController)nc.ViewControllers[0];
                    if (ids.Select(id => vc.IsShowingDocumentWithId(id)).Any(v => v))
                        vc.UpdatePriority();
                }
            });
        }

        void RemoveDocumentsFromList(IEnumerable<int> ids)
        {
            BeginInvokeOnMainThread(() =>
            {
                if (searchController.Active)
                {
                    var tableViewController = searchController?.SearchResultsController as UITableViewController;
                    var dataSource = tableViewController?.TableView?.Source as DocumentListDataSource;
                    dataSource?.RemoveItems(ids);
                }

                ((DocumentListDataSource)TableView.Source).RemoveItems(ids);

                if (SplitViewController != null && !SplitViewController.Collapsed)
                {
                    var nc = (UINavigationController)SplitViewController.ViewControllers[1];
                    var vc = (DocumentPageViewController)nc.ViewControllers[0];
                    if (ids.Select(id => vc.IsShowingDocumentWithId(id)).Any(v => v))
                        vc.ClearPage();
                }
            });
        }

        DocumentPreview GetNextDocumentPreview(DocumentPreview documentPreview, out bool previousDocumentAvailable, out bool nextDocumentAvailable, bool scrollToDocument = false)
        {
            try
            {
                var ds = ((DocumentListDataSource)TableView.Source);

                var currentDocumentRow = ds.Items.IndexOf(d => d.Id == documentPreview.Id);
                if (currentDocumentRow < 0)
                {
                    previousDocumentAvailable = false;
                    nextDocumentAvailable = false;
                    return null;
                }

                var nextDocumentRow = currentDocumentRow + 1;
                previousDocumentAvailable = true;
                nextDocumentAvailable = nextDocumentRow < ds.Items.Count - 1;

                if (!nextDocumentAvailable && ds.LoadMoreEnabled)
                    AsyncHelpers.FireAndForget(RefreshData(ds.Items.Last().Id));

                if (scrollToDocument)
                    ds.ScrollTo(nextDocumentRow);

                return ds.Items.ElementAtOrDefault(nextDocumentRow);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Could not switch to next document", ex);

                previousDocumentAvailable = false;
                nextDocumentAvailable = false;
                return null;
            }
        }

        DocumentPreview GetPreviousDocumentPreview(DocumentPreview documentPreview, out bool previousDocumentAvailable, out bool nextDocumentAvailable, bool scrollToDocument = false)
        {
            try
            {
                var ds = ((DocumentListDataSource)TableView.Source);

                var currentDocumentRow = ds.Items.IndexOf(d => d.Id == documentPreview.Id);
                if (currentDocumentRow < 0)
                {
                    previousDocumentAvailable = false;
                    nextDocumentAvailable = false;
                    return null;
                }

                var previousDocumentRow = currentDocumentRow - 1;
                previousDocumentAvailable = previousDocumentRow > 0;
                nextDocumentAvailable = previousDocumentRow < ds.Items.Count - 1;

                if (scrollToDocument)
                    ds.ScrollTo(previousDocumentRow);

                return ds.Items.ElementAtOrDefault(previousDocumentRow);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Could not switch to previous document", ex);

                previousDocumentAvailable = false;
                nextDocumentAvailable = false;
                return null;
            }
        }


        #endregion

        #region Workers

        class AutoRefreshWorker : NSObject
        {
            CancellationTokenSource cts;

            Func<int, Task> work;
            Func<DocumentPreview> firstOrDefaultItem;
            readonly int intervalMs;

            readonly object lockObj = new object();

            public AutoRefreshWorker(Func<int, Task> work, Func<DocumentPreview> firstOrDefaultItem, int intervalMs)
            {
                this.work = work;
                this.firstOrDefaultItem = firstOrDefaultItem;
                this.intervalMs = intervalMs;
            }

            public void Start()
            {
                lock (lockObj)
                {
                    cts?.Cancel();
                    cts = new CancellationTokenSource();
                    Task.Run(async () =>
                    {
                        while (true)
                        {
                            await Task.Delay(intervalMs);
                            if (cts.IsCancellationRequested)
                                break;

                            await AsyncHelpers.InvokeOnMainThreadAsync(this, async () =>
                            {
                                var first = firstOrDefaultItem?.Invoke();
                                if (first != null)
                                {
                                    if (work != null)
                                        await work(first.Id);
                                }
                            });
                        }
                    });
                }
            }

            public void Stop()
            {
                lock (lockObj)
                    cts?.Cancel();
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                work = null;
                firstOrDefaultItem = null;
            }
        }

        #endregion

        #region State restoration

        public override void EncodeRestorableState(NSCoder coder)
        {
            base.EncodeRestorableState(coder);
            coder.Encode(Serializer.SerializeToByteArray(Folder.ShallowCopy()), "folder");
        }

        public override void DecodeRestorableState(NSCoder coder)
        {
            base.DecodeRestorableState(coder);
            Folder = Serializer.DeserializeFromByteArray<Folder>(coder.DecodeBytes("folder"));
        }

        [Export("viewControllerWithRestorationIdentifierPath:coder:")]
        public static UIViewController Restore(string[] identifierComponents, NSCoder coder)
        {
            return new DocumentsListViewController();
        }

        #endregion

    }
}
