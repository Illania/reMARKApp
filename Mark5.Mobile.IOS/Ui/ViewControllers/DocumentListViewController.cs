using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.HubMessages;
using Mark5.Mobile.Common.Service;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentView;
using Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;
using Mark5.Mobile.IOS.Utilities;
using TinyMessenger;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class DocumentsListViewController : AbstractTableViewController, IPrimaryViewController, IUISearchResultsUpdating, IUIViewControllerRestoration
    {
        const int AutoRefreshIntervalMs = 5 * 1000;

        public Folder Folder { get; set; }

        UIBarButtonItem composeDocumentItem;
        UIBarButtonItem exitEditItem;
        UIBarButtonItem editItem;

        bool refreshing;

        UISearchController searchController;
        CancellationTokenSource searchCancellationTokenSource;
        readonly List<CancellationTokenSource> searchCancellationTokenSourceList = new List<CancellationTokenSource>();
        string lastSearchQuery;

        AutoRefreshWorker autoRefreshWorker;
        Action newDocumentsAvailableAction;

        TinyMessageSubscriptionToken readStatusChangedToken;
        TinyMessageSubscriptionToken commentsCountChangedToken;
        TinyMessageSubscriptionToken categoriesChangedToken;
        TinyMessageSubscriptionToken removedFromFolderToken;
        TinyMessageSubscriptionToken movedFromFolderToken;
        TinyMessageSubscriptionToken deletedToken;

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

            NavigationItem.Title = Folder.Name;

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

            if (searchController.SearchResultsController is UITableViewController searchTableViewController)
            {
                if (searchTableViewController?.TableView?.IndexPathForSelectedRow != null)
                    searchTableViewController.TableView.DeselectRow(TableView.IndexPathForSelectedRow, true);

                if (searchTableViewController?.TableView?.IndexPathsForSelectedRows?.Length > 0)
                    foreach (var selectedIndexPath in searchTableViewController.TableView?.IndexPathsForSelectedRows)
                        searchTableViewController.TableView.DeselectRow(selectedIndexPath, true);
            }


            ReachabilityBar.Attach(this);
        }

        public override async void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info($"{nameof(DocumentsListViewController)} appeared");

            if (((DataSource)TableView.Source).Empty)
                await RefreshData();

            CommonConfig.Logger.Info($"Starting automatic refresh...");

            autoRefreshWorker?.Stop();
            autoRefreshWorker = new AutoRefreshWorker(AutoRefreshData, () => { return ((DataSource)TableView.Source).Items.FirstOrDefault(); }, AutoRefreshIntervalMs);
            autoRefreshWorker.Start();

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

            autoRefreshWorker?.Stop();
            autoRefreshWorker?.Dispose();
            autoRefreshWorker = null;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            ReachabilityBar.Detach(this);
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

            ((DataSource)TableView.Source)?.Reset();

            UnsubscribeFromMessages();

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        protected override void Recycle()
        {
            base.Recycle();

            composeDocumentItem = null;
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

        void InitializeNavigationBar()
        {
            composeDocumentItem = new UIBarButtonItem
            {
                Image = UIImage.FromBundle("Create")
            };
            NavigationItem.SetRightBarButtonItem(composeDocumentItem, false);

            exitEditItem = new UIBarButtonItem(UIBarButtonSystemItem.Done);
            editItem = new UIBarButtonItem(UIBarButtonSystemItem.Edit);
        }

        void InitializeView()
        {
            RefreshControl = new UIRefreshControl();

            TableView.Source = new DataSource(this, TableView, Localization.GetString("folder_empty"), PlatformConfig.Preferences.CompactDocumentsList);
            TableView.RefreshControl = RefreshControl;
            TableView.AllowsMultipleSelectionDuringEditing = true;
            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 60f;

            TableView.AddGestureRecognizer(new UILongPressGestureRecognizer(DocumentPreviewLongPressed));
        }

        void InitializeSearchBar()
        {
            DefinesPresentationContext = true;

            var searchResultsController = new UITableViewController();
            var searchResultsDataSource = new DataSource(this, searchResultsController.TableView, Localization.GetString("no_matching_documents"), PlatformConfig.Preferences.CompactDocumentsList);
            searchResultsController.TableView.Source = searchResultsDataSource;
            searchResultsController.TableView.EstimatedRowHeight = 60f;
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
                TableView.TableHeaderView = searchController.SearchBar;
        }

        void InitializeHandlers()
        {
            if (composeDocumentItem != null)
                composeDocumentItem.Clicked += ComposeDocumentItem_Clicked;

            if (exitEditItem != null)
                exitEditItem.Clicked += ExitEditItem_Clicked;

            if (editItem != null)
                editItem.Clicked += EditItem_Clicked;

            RefreshControl.ValueChanged += RefreshControl_ValueChanged;
        }

        void DeinitializeHandlers()
        {
            if (composeDocumentItem != null)
                composeDocumentItem.Clicked -= ComposeDocumentItem_Clicked;

            if (exitEditItem != null)
                exitEditItem.Clicked -= ExitEditItem_Clicked;

            if (editItem != null)
                editItem.Clicked -= EditItem_Clicked;

            RefreshControl.ValueChanged -= RefreshControl_ValueChanged;
        }

        #endregion

        #region Subscribe/unsubscribe

        void SubscribeToMessages()
        {
            readStatusChangedToken = CommonConfig.MessengerHub.Subscribe<DocumentPreviewReadStatusChangedMessage>(ReadStatusChangedHandler);
            commentsCountChangedToken = CommonConfig.MessengerHub.Subscribe<EntityPreviewCommentCountChangedMessage>(CommentsCountChangedHandler);
            categoriesChangedToken = CommonConfig.MessengerHub.Subscribe<EntityCategoriesChangedMessage>(CategoriesChangedHandler, m => m.ObjectType == ObjectType.Document);
            removedFromFolderToken = CommonConfig.MessengerHub.Subscribe<EntityRemovedFromFolderMessage>(HandleRemovedFromFolder, m => m.ObjectType == ObjectType.Document);
            movedFromFolderToken = CommonConfig.MessengerHub.Subscribe<EntityMovedFromFolderMessage>(HandleMovedFromFolder, m => m.ObjectType == ObjectType.Document);
            deletedToken = CommonConfig.MessengerHub.Subscribe<EntityRemovedMessage>(HandleDeleted, m => m.ObjectType == ObjectType.Document);
        }

        void UnsubscribeFromMessages()
        {
            readStatusChangedToken?.Dispose();
            commentsCountChangedToken?.Dispose();
            categoriesChangedToken?.Dispose();
            removedFromFolderToken?.Dispose();
            movedFromFolderToken?.Dispose();
            deletedToken?.Dispose();
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

        void ExitEditItem_Clicked(object sender, EventArgs e) => EndEditing();

        void EditItem_Clicked(object sender, EventArgs e)
        {
            if (TableView.IndexPathsForSelectedRows == null || TableView.IndexPathsForSelectedRows.Length < 1)
                return;

            var eas = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            var d = new PopoverPresentationControllerDelegate((UIBarButtonItem)sender);

            var rows = TableView.IndexPathsForSelectedRows.ToArray();
            var selectedDocuments = rows.Select(ip => ((DataSource)TableView.Source).Items[ip.Row]).ToList();

            if (selectedDocuments.Any(dp => !dp.IsReadByCurrent))
                eas.AddAction(UIAlertAction.Create(Localization.GetString("mark_as_read"),
                    UIAlertActionStyle.Default,
                    a =>
                    {
                        MarkAsRead(selectedDocuments);
                        EndEditing();
                    }));

            if (selectedDocuments.Any(dp => dp.IsReadByCurrent))
                eas.AddAction(UIAlertAction.Create(Localization.GetString("mark_as_unread"),
                    UIAlertActionStyle.Default,
                    a =>
                    {
                        MarkAsUnread(selectedDocuments);
                        EndEditing();
                    }));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_worktray"),
                UIAlertActionStyle.Default,
                a =>
                {
                    CopyToWorktray(selectedDocuments);
                    EndEditing();
                }));
            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_folder"),
                UIAlertActionStyle.Default,
                a =>
                {
                    CopyToFolder(selectedDocuments);
                    EndEditing();
                }));

            if (Folder.InternalType == FolderInternalType.FilterView || Folder.InternalType == FolderInternalType.Static || Folder.InternalType == FolderInternalType.Worktray)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("move_to_folder"),
                    UIAlertActionStyle.Default,
                    a =>
                    {
                        MoveToFolder(selectedDocuments);
                        EndEditing();
                    }));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("set_priority"), UIAlertActionStyle.Default, a => ShowPriorityActionSheet(selectedDocuments, (UIBarButtonItem)sender)));

            if (Folder.InternalType == FolderInternalType.FilterView || Folder.InternalType == FolderInternalType.Static || Folder.InternalType == FolderInternalType.Worktray)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete_from_folder"), UIAlertActionStyle.Default, a => RemoveFromFolder(selectedDocuments, d)));

            if (ServerConfig.SystemSettings.DocumentsModuleInfo.Permissions.DeleteAllowed || selectedDocuments.All(dp => dp.Direction == DocumentDirection.Draft))
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete"), UIAlertActionStyle.Destructive, a => Delete(selectedDocuments, d)));

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

        async Task RefreshData(int startId = -1, int endId = -1, bool forceClear = false)
        {
            if (refreshing)
                return;

            refreshing = true;
            RefreshControl.ValueChanged -= RefreshControl_ValueChanged;

            CommonConfig.Logger.Info($"Refreshing documents list [folder={Folder?.Name}, startId={startId}, endId={endId}, forceClear={forceClear}]");

            try
            {
                if (forceClear)
                    ((DataSource)TableView.Source)?.Reset();

                var documentPreviews = await Managers.DocumentsManager.GetDocumentPreviewsAsync(Folder, startId, endId);
                ((DataSource)TableView.Source).LoadMoreEnabled = documentPreviews.Count >= PlatformConfig.Preferences.DocumentsToDownload;
                CommonConfig.Logger.Info($"Enable load more documents set to {((DataSource)TableView.Source).LoadMoreEnabled}");

                Services.DocumentsDownloadService.Notify();

                ((DataSource)TableView.Source).AppendItems(documentPreviews);

                if (documentPreviews.Any())
                    newDocumentsAvailableAction?.Invoke();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Could not refresh folders [folder={Folder?.Name}, startId={startId}, endId={endId}, forceClear={forceClear}]", ex);

                await Dialogs.ShowErrorAlertAsync(this, ex);

                NavigationController?.PopViewController(true);
            }

            RefreshControl.EndRefreshing();
            RefreshControl.ValueChanged += RefreshControl_ValueChanged;

            refreshing = false;
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

                if (documents.Count > 0)
                {
                    CommonConfig.Logger.Info($"Received {documents?.Count} new documents");

                    Services.DocumentsDownloadService.Notify();

                    ((DataSource)TableView.Source).PrependItems(documents);

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

        void DocumentSelected(DocumentPreview documentPreview)
        {
            if (SplitViewController != null && !SplitViewController.Collapsed)
            {
                var nc = (UINavigationController)SplitViewController.ViewControllers[1];
                nc.PopToViewController(nc.ViewControllers[0], false);

                var vc = (DocumentPageViewController)nc.ViewControllers[0];

                if (vc.IsShowingDocumentWithId(documentPreview.Id))
                    return;

                vc.HidesBottomBarWhenPushed = false;
                vc.SetPage(Folder, documentPreview);
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
                        DocumentPreviews = ((DataSource)TableView.Source).Items
                    };
                    newDocumentsAvailableAction = null;
                    NavigationController.PushViewController(vc, true);
                }
            }
        }

        void DocumentPreviewLongPressed(UILongPressGestureRecognizer recognizer)
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

        void ShowMoreActionSheet(NSIndexPath indexPath, DocumentPreview selectedDocument, Folder folder)
        {
            var alertController = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            var d = new PopoverPresentationControllerDelegate(TableView, TableView.CellAt(indexPath));

            var availableActions = PlatformConfig.Preferences.GetAvailableSwipeActions();

            foreach (var item in availableActions)
            {
                switch (item.Action)
                {
                    case EmailSwipeAction.SwipeAction.MarkAsRead:

                        if (selectedDocument.IsReadByCurrent)
                        {
                            alertController.AddAction(UIAlertAction.Create(Localization.GetString("mark_as_unread"), UIAlertActionStyle.Default, a =>
                            {
                                MarkAsUnread(selectedDocument);
                                EndEditing();
                            }));
                        }
                        else
                        {
                            alertController.AddAction(UIAlertAction.Create(Localization.GetString("mark_as_read"), UIAlertActionStyle.Default, a =>
                            {
                                MarkAsRead(selectedDocument);
                                EndEditing();
                            }));
                        }
                        break;
                    case EmailSwipeAction.SwipeAction.CopyToWorkTray:
                        alertController.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_worktray"), UIAlertActionStyle.Default, a =>
                        {
                            CopyToWorktray(selectedDocument);
                            EndEditing();
                        }));
                        break;
                    case EmailSwipeAction.SwipeAction.Delete:
                        if (SwipeActionAllowed(item.Action, selectedDocument, folder))
                            alertController.AddAction(UIAlertAction.Create(Localization.GetString("delete"), UIAlertActionStyle.Destructive, a => Delete(selectedDocument, d)));

                        break;
                    case EmailSwipeAction.SwipeAction.Categories:
                        alertController.AddAction(UIAlertAction.Create(Localization.GetString("categories"),
                                                                       UIAlertActionStyle.Default,
                                                                       a =>
                                                                       {
                                                                           ShowCategories(selectedDocument);
                                                                           EndEditing();
                                                                       }));
                        break;
                    case EmailSwipeAction.SwipeAction.CopyToFolder:
                        alertController.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_folder"),
                                                                       UIAlertActionStyle.Default,
                                                                       a =>
                                                                       {
                                                                           CopyToFolder(selectedDocument);
                                                                           EndEditing();
                                                                       }));

                        break;
                    case EmailSwipeAction.SwipeAction.MoveToFolder:
                        if(SwipeActionAllowed(item.Action, selectedDocument, folder)) { 
                            alertController.AddAction(UIAlertAction.Create(Localization.GetString("move_to_folder"),
                                UIAlertActionStyle.Default,
                                a =>
                                {
                                    MoveToFolder(selectedDocument);
                                    EndEditing();
                                }));
                        }
                        break;
                    case EmailSwipeAction.SwipeAction.SetPriority:
                        alertController.AddAction(UIAlertAction.Create(Localization.GetString("set_priority"), UIAlertActionStyle.Default, a => ShowPriorityActionSheet(selectedDocument, TableView, TableView.CellAt(indexPath))));

                        break;
                    case EmailSwipeAction.SwipeAction.RemoveFromFolder:
                        if (SwipeActionAllowed(item.Action, selectedDocument, folder))
                        {
                            alertController.AddAction(UIAlertAction.Create(Localization.GetString("delete_from_folder"), UIAlertActionStyle.Default, a => RemoveFromFolder(selectedDocument, d)));
                        }
                        break;

                }
            }

            alertController.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, null));
            if (alertController.PopoverPresentationController != null)
                alertController.PopoverPresentationController.Delegate = d;

            PresentViewController(alertController, true, null);
        }

        async void ShowPriorityActionSheet(List<DocumentPreview> selectedDocuments, UIBarButtonItem barButtonItem)
        {
            var priorities = new[] { Priority.Low, Priority.Normal, Priority.Urgent };
            var priorityStrings = priorities.Select(p => UI.PrettyPriorityString(p)).ToArray();
            var result = await Dialogs.ShowListActionSheetAsync(this, priorityStrings, barButtonItem);

            if (result < 0)
                return;

            var priority = priorities[result];

            await SetPriority(selectedDocuments, priority);
        }

        async void ShowPriorityActionSheet(DocumentPreview selectedDocument, UITableView tv, UITableViewCell cell)
        {
            var priorities = new List<Priority> { Priority.Low, Priority.Normal, Priority.Urgent };
            var priorityStrings = priorities.Select(p => UI.PrettyPriorityString(p));
            var result = await Dialogs.ShowListActionSheetAsync(this, priorityStrings.ToArray(), tv, cell);

            if (result < 0)
                return;

            var priority = priorities[result];

            await SetPriority(new List<DocumentPreview> { selectedDocument }, priority);
        }

        async void ShowPriorityActionSheet(DocumentPreview selectedDocument, UITableView tv, NSIndexPath indexPath)
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

        async Task SetPriority(List<DocumentPreview> selectedDocuments, Priority priority)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("setting_priority___"));

            try
            {
                CommonConfig.Logger.Info($"Attempting to setting priority for documents");
                await Managers.DocumentsManager.SetDocumentsPriorityAsync(selectedDocuments, priority);

                EndEditing();

                UpdatePriorityForDocument(selectedDocuments.Select(d => d.Id));

                dismissAction();
            }
            catch (Exception ex)
            {
                EndEditing();
                dismissAction();

                CommonConfig.Logger.Error($"Error while setting priority for documents", ex);
                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        void RemoveFromFolder(DocumentPreview selectedDocument, UIPopoverPresentationControllerDelegate d) =>
            RemoveFromFolder(new List<DocumentPreview> { selectedDocument }, d);

        async void RemoveFromFolder(List<DocumentPreview> selectedDocuments, UIPopoverPresentationControllerDelegate d)
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
                CommonConfig.Logger.Info($"Attempting to remove documents from folder [folderId={Folder.Id}]");
                await Managers.CommonActionsManager.RemoveFromFolder(selectedDocuments.Cast<IBusinessEntity>().ToList(), Folder);

                RemoveDocumentsFromList(selectedDocuments.Select(s => s.Id));
                EndEditing();

                dismissAction();
            }
            catch (Exception ex)
            {
                EndEditing();
                dismissAction();

                CommonConfig.Logger.Error($"Error while removing documents from folder [folderId={Folder.Id}]", ex);
                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        void Delete(DocumentPreview selectedDocument, UIPopoverPresentationControllerDelegate d) =>
            Delete(new List<DocumentPreview> { selectedDocument }, d);

        async void Delete(List<DocumentPreview> selectedDocuments, UIPopoverPresentationControllerDelegate d)
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
                CommonConfig.Logger.Info($"Attempting to delete documents");

                await Managers.CommonActionsManager.Delete(selectedDocuments.Cast<IBusinessEntity>().ToList());

                RemoveDocumentsFromList(selectedDocuments.Select(s => s.Id));
                EndEditing();

                dismissAction();
            }
            catch (Exception ex)
            {
                EndEditing();
                dismissAction();

                CommonConfig.Logger.Error($"Error while deleting documents", ex);
                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        void ShowCategories(DocumentPreview selectedDocument)
        {
            var vc = new CategoriesListViewController
            {
                BusinessEntityPreview = selectedDocument
            };
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        void CopyToFolder(DocumentPreview selectedDocument) =>
            CopyToFolder(new List<DocumentPreview> { selectedDocument });

        void CopyToFolder(List<DocumentPreview> selectedDocument)
        {
            var vc = new CopyMoveToFolderListViewController(ModuleType.Documents, selectedDocument.Cast<IBusinessEntity>().ToList());
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        void MoveToFolder(DocumentPreview selectedDocument) =>
            MoveToFolder(new List<DocumentPreview> { selectedDocument });

        void MoveToFolder(List<DocumentPreview> selectedDocument)
        {
            var vc = new CopyMoveToFolderListViewController(ModuleType.Documents, selectedDocument.Cast<IBusinessEntity>().ToList(), Folder)
            {
                ModalPresentationStyle = UIModalPresentationStyle.PageSheet
            };
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        void CopyToWorktray(DocumentPreview selectedDocument) =>
            CopyToWorktray(new List<DocumentPreview> { selectedDocument });

        void CopyToWorktray(List<DocumentPreview> selectedDocuments)
        {
            var vc = new CopyToWorktrayViewController
            {
                BusinessEntities = selectedDocuments.Cast<IBusinessEntity>().ToList()
            };
            vc.ModalPresentationStyle = UIModalPresentationStyle.PageSheet;
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        void MarkAsRead(DocumentPreview documentPreview) =>
            MarkAsRead(new List<DocumentPreview> { documentPreview });

        async void MarkAsRead(List<DocumentPreview> documentPreviews)
        {
            CommonConfig.Logger.Info($"Attempting to mark as read [documentPreviews={documentPreviews.Count}]...");

            try
            {
                CommonConfig.UsageAnalytics.LogEvent(new SetReadStatusEvent(documentPreviews.Count));

                await Managers.DocumentsManager.SetDocumentsReadStatusAsync(documentPreviews, true);

                var updatedItems = documentPreviews.Select(d => d.Id);
                ((DataSource)TableView.Source).UpdateItems(updatedItems);
                ((DataSource)((UITableViewController)searchController?.SearchResultsController)?.TableView?.Source)?.UpdateItems(updatedItems);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Marking as read failed [documentPreviews.Count={documentPreviews.Count}]", ex);

                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        void MarkAsUnread(DocumentPreview documentPreview) =>
            MarkAsUnread(new List<DocumentPreview> { documentPreview });

        async void MarkAsUnread(List<DocumentPreview> documentPreviews)
        {
            CommonConfig.Logger.Info($"Attempting to mark as unread [documentPreviews={documentPreviews.Count}]...");

            try
            {
                CommonConfig.UsageAnalytics.LogEvent(new SetReadStatusEvent(documentPreviews.Count));

                await Managers.DocumentsManager.SetDocumentsReadStatusAsync(documentPreviews, false);

                var updatedItems = documentPreviews.Select(d => d.Id);
                ((DataSource)TableView.Source).UpdateItems(updatedItems);
                ((DataSource)((UITableViewController)searchController?.SearchResultsController)?.TableView?.Source)?.UpdateItems(updatedItems);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Marking as unread failed [documentPreviews.Count={documentPreviews.Count}]", ex);

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

                if (searchText == lastSearchQuery)
                    return;

                lastSearchQuery = searchText;

                DoSearchDocuments(searchText, searchCancellationTokenSource.Token);
            }
        }

        async void DoSearchDocuments(string searchText, CancellationToken ct)
        {
            var tableViewController = searchController?.SearchResultsController as UITableViewController;
            var dataSource = tableViewController?.TableView?.Source as DataSource;
            dataSource?.Reset();

            await Task.Delay(500);

            if (ct.IsCancellationRequested)
                return;

            var ds = (DataSource)TableView.Source;
            var filteredDocuments = ds.Items.Where(dp => MatchesQuery(dp, searchText)).ToList();

            if (ct.IsCancellationRequested)
                return;

            dataSource?.AppendItems(filteredDocuments);
        }

        static bool MatchesQuery(DocumentPreview dp, string query)
        {
#if DEBUG
            if (dp.Id.ToString() == query)
                return true;
#endif

            if (dp.ReferenceNumber?.ContainsCaseInsensitive(query) ?? false)
                return true;

            if (dp.Subject?.ContainsCaseInsensitive(query) ?? false)
                return true;

            if (dp.Preview?.ContainsCaseInsensitive(query) ?? false)
                return true;

            if (dp.Addresses.Any(da => da.Name?.ContainsCaseInsensitive(query) ?? false))
                return true;

            if (dp.Addresses.Any(da => da.Address?.ContainsCaseInsensitive(query) ?? false))
                return true;

            if (dp.Categories.Any(da => da.Name?.ContainsCaseInsensitive(query) ?? false))
                return true;

            if (dp.Creator?.ContainsCaseInsensitive(query) ?? false)
                return true;

            return false;
        }

        #endregion

        #region Messages handlers

        void ReadStatusChangedHandler(DocumentPreviewReadStatusChangedMessage message)
        {
            BeginInvokeOnMainThread(() =>
            {
                foreach (var tableView in new UITableView[] { TableView, ((UITableViewController)searchController?.SearchResultsController)?.TableView })
                {
                    if (tableView == null || tableView.Source == null)
                        continue;

                    var index = ((DataSource)tableView.Source).Items.FindIndex(dp => dp.Id == message.DocumentPreviewId);

                    if (index >= 0)
                    {
                        var documentPreview = ((DataSource)tableView.Source).Items[index];
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

        void CommentsCountChangedHandler(EntityPreviewCommentCountChangedMessage message)
        {
            BeginInvokeOnMainThread(() =>
            {
                foreach (var tableView in new UITableView[] { TableView, ((UITableViewController)searchController?.SearchResultsController)?.TableView })
                {
                    if (tableView == null || tableView.Source == null)
                        continue;

                    var index = ((DataSource)tableView.Source).Items.FindIndex(dp => dp.Id == message.EntityId);

                    if (index >= 0)
                    {
                        var documentPreview = ((DataSource)tableView.Source).Items[index];
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

                    var index = ((DataSource)tableView.Source).Items.FindIndex(dp => dp.Id == message.EntityId);

                    if (index >= 0)
                    {
                        var documentPreview = ((DataSource)tableView.Source).Items[index];
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

        #endregion

        #region Utilities
        static bool SwipeActionAllowed(EmailSwipeAction.SwipeAction action, DocumentPreview documentPreview, Folder folder)
        {
            switch (action)
            {
                case EmailSwipeAction.SwipeAction.MoveToFolder:
                    return folder.InternalType == FolderInternalType.FilterView || folder.InternalType == FolderInternalType.Static || folder.InternalType == FolderInternalType.Worktray;
                case EmailSwipeAction.SwipeAction.Delete:
                    return ServerConfig.SystemSettings.DocumentsModuleInfo.Permissions.DeleteAllowed || documentPreview.Direction == DocumentDirection.Draft;
                case EmailSwipeAction.SwipeAction.RemoveFromFolder:
                    return folder.InternalType == FolderInternalType.FilterView || folder.InternalType == FolderInternalType.Static || folder.InternalType == FolderInternalType.Worktray;
                default:
                    return true;
            }
        }

        void StartEditing()
        {
            if (SplitViewController != null && !SplitViewController.Collapsed)
            {
                var nc = (UINavigationController)SplitViewController.ViewControllers[1];
                nc.PopToRootViewController(false);

                var vc = (DocumentPageViewController)nc.ViewControllers[0];
                vc.ClearPage();
            }

            TableView.SetEditing(true, true);
            NavigationItem.SetRightBarButtonItem(exitEditItem, true);
            NavigationItem.SetLeftBarButtonItem(editItem, true);
        }

        void EndEditing()
        {
            TableView.SetEditing(false, true);
            NavigationItem.SetRightBarButtonItem(composeDocumentItem, false);
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
                    var dataSource = tableViewController?.TableView?.Source as DataSource;
                    dataSource?.RemoveItems(ids);
                }

                ((DataSource)TableView.Source).RemoveItems(ids);

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
                var ds = ((DataSource)TableView.Source);

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
                var ds = ((DataSource)TableView.Source);

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

        #region DataSource

        class DataSource : UITableViewSource
        {
            public bool Empty => Items.Count < 1;
            public List<DocumentPreview> Items { get; } = new List<DocumentPreview>(1000);
            public bool LoadMoreEnabled { get; set; }

            readonly WeakReference<DocumentsListViewController> viewControllerWeakReference;
            readonly WeakReference<UITableView> tableViewWeakReference;
            readonly string emptyText;
            readonly bool compactList;

            bool loading = true;

            public DataSource(DocumentsListViewController viewController, UITableView tableView, string emptyText, bool compactList)
            {
                viewControllerWeakReference = viewController.Wrap();
                tableViewWeakReference = tableView.Wrap();
                this.emptyText = emptyText;
                this.compactList = compactList;
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

                var dp = Items[indexPath.Row];

                if (LoadMoreEnabled && dp.Id == Items.Last().Id)
                {
                    CommonConfig.UsageAnalytics.LogEvent(new GetMoreDocumentsEvent());
                    AsyncHelpers.FireAndForget(viewControllerWeakReference.Unwrap()?.RefreshData(dp.Id));
                }

                if (dp.Direction == DocumentDirection.External)
                {
                    var cell = tableView.DequeueReusableCell(DocumentsTableViewCell.ExternalId) as DocumentsTableViewCell ?? new DocumentsTableViewCell(DocumentsTableViewCell.ExternalId);
                    cell.Initialize(dp);
                    return cell;
                }

                if (compactList)
                {
                    var cell = tableView.DequeueReusableCell(DocumentsTableViewCell.CompactId) as DocumentsTableViewCell ?? new DocumentsTableViewCell(DocumentsTableViewCell.CompactId);
                    cell.Initialize(dp);
                    return cell;
                }
                else
                {
                    var cell = tableView.DequeueReusableCell(DocumentsTableViewCell.DefaultId) as DocumentsTableViewCell ?? new DocumentsTableViewCell(DocumentsTableViewCell.DefaultId);
                    cell.Initialize(dp);
                    return cell;
                }
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading || Empty)
                    return 1;

                return Items.Count;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                if (tableView.Editing)
                    return;

                var dp = Items[indexPath.Row];
                viewControllerWeakReference.Unwrap()?.DocumentSelected(dp);
            }

            public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading || Empty)
                    return false;

                return true;
            }

            public void PrependItems(IEnumerable<DocumentPreview> documentPreviews)
            {
                loading = false;

                if (Empty)
                {
                    Items.InsertRange(0, documentPreviews);

                    tableViewWeakReference.Unwrap()?.BeginUpdates();

                    tableViewWeakReference.Unwrap()?.ReloadRows(new[] { NSIndexPath.FromRowSection(0, 0) }, UITableViewRowAnimation.Automatic);

                    if (documentPreviews.Count() > 1)
                    {
                        var indexes = Enumerable.Range(1, documentPreviews.Count() - 1).Select(i => NSIndexPath.FromRowSection(i, 0)).ToArray();
                        tableViewWeakReference.Unwrap()?.InsertRows(indexes, UITableViewRowAnimation.Fade);
                    }

                    tableViewWeakReference.Unwrap()?.EndUpdates();
                }
                else
                {
                    Items.InsertRange(0, documentPreviews);
                    var indexes = Enumerable.Range(0, documentPreviews.Count()).Select(i => NSIndexPath.FromRowSection(i, 0)).ToArray();
                    tableViewWeakReference.Unwrap()?.InsertRows(indexes, UITableViewRowAnimation.Fade);
                }

            }

            public void AppendItems(IEnumerable<DocumentPreview> documentPreviews)
            {
                loading = false;

                Items.AddRange(documentPreviews);
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            public void RemoveItems(IEnumerable<int> documentIds)
            {
                var indices = Items.Select((d, i) => new { d, i })
                                   .Where(x => documentIds.Contains(x.d.Id))
                                   .Select(x => x.i)
                                   .OrderByDescending(i => i)
                                   .ToArray();
                indices.ForEach(Items.RemoveAt);

                tableViewWeakReference.Unwrap()?.BeginUpdates();

                if (!Items.Any())
                    tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Automatic);
                else
                {
                    var indexPaths = indices.Select(i => NSIndexPath.FromRowSection(i, 0)).ToArray();
                    tableViewWeakReference.Unwrap()?.DeleteRows(indexPaths, UITableViewRowAnimation.Automatic);
                }

                tableViewWeakReference.Unwrap()?.EndUpdates();
            }

            public void UpdateItems(IEnumerable<int> documentPreviewIds)
            {
                if (documentPreviewIds == null || !documentPreviewIds.Any())
                    return;

                tableViewWeakReference.Unwrap()?.BeginUpdates();
                documentPreviewIds.ForEach(UpdateItem);
                tableViewWeakReference.Unwrap()?.EndUpdates();
            }

            public void UpdateItem(int documentPreviewId)
            {
                var documentRow = Items?.IndexOf(d => d.Id == documentPreviewId);
                if (documentRow < 0)
                    return;

                tableViewWeakReference.Unwrap()?.ReloadRows(new NSIndexPath[] { NSIndexPath.FromRowSection(documentRow.Value, 0) }, UITableViewRowAnimation.Fade);
            }

            public void Reset()
            {
                loading = true;

                Items.Clear();
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            public void ScrollTo(int row)
            {
                var selectedIndexPaths = tableViewWeakReference.Unwrap()?.IndexPathsForSelectedRows;
                if (selectedIndexPaths != null)
                    foreach (var indexPath in selectedIndexPaths)
                        tableViewWeakReference.Unwrap()?.DeselectRow(indexPath, true);

                tableViewWeakReference.Unwrap()?.SelectRow(NSIndexPath.FromRowSection(row, 0), true, UITableViewScrollPosition.Middle);
            }

            #region SwipeRelated

            class SwipeActionUIWrapper
            {
                public UITableViewRowAction Action { get; set; }
                public bool Disabled;
            }

            UIContextualAction BuildLeadingContextualAction(UITableView tableView, NSIndexPath indexPath)
            {
                var documentPreview = Items[indexPath.Row];

                EmailSwipeAction leadingAction = PlatformConfig.Preferences.EmailLeadingSwipeActions.First();

                var folder = viewControllerWeakReference.Unwrap()?.Folder;

                string title = SwipeActionTitle(leadingAction, documentPreview);

                var contextualAction = UIContextualAction.FromContextualActionStyle(UIContextualActionStyle.Normal, title, (someAction, view, success) =>
                {
                    OnSwipeActionClick(leadingAction, indexPath, documentPreview, folder, tableView);
                });

                if(SwipeActionAllowed(leadingAction.Action, documentPreview, folder)) {
                    contextualAction.BackgroundColor = Theme.LightBrown;
                } else {
                    contextualAction.BackgroundColor = Theme.LightGray;
                }

                return contextualAction;
            }

            public override UISwipeActionsConfiguration GetLeadingSwipeActionsConfiguration(UITableView tableView, NSIndexPath indexPath)
            {
                var leadingSwipe = UISwipeActionsConfiguration.FromActions(new UIContextualAction[] { BuildLeadingContextualAction(tableView, indexPath) });

                leadingSwipe.PerformsFirstActionWithFullSwipe = true;

                return leadingSwipe;
            }

            public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
            {
                var actionWrappers = new List<SwipeActionUIWrapper>();

                var documentPreview = Items[indexPath.Row];

                List<EmailSwipeAction> trailingSwipeActions = PlatformConfig.Preferences.EmailTrailingSwipeActions;

                trailingSwipeActions.Reverse();
                var folder = viewControllerWeakReference.Unwrap()?.Folder;

                foreach (EmailSwipeAction swipeAction in trailingSwipeActions)
                {
                    SwipeActionUIWrapper actionWrapper = new SwipeActionUIWrapper();

                    switch (swipeAction.Action)
                    {
                        case EmailSwipeAction.SwipeAction.MarkAsRead:
                            if (documentPreview.IsReadByCurrent)
                            {
                                actionWrapper.Action = UITableViewRowAction.Create(
                                    UITableViewRowActionStyle.Default,
                                    SwipeActionTitle(swipeAction, documentPreview),
                                    (a, ip) =>
                                    {
                                        OnSwipeActionClick(swipeAction, indexPath, documentPreview, folder, tableView);
                                    });
                            }
                            else
                            {
                                actionWrapper.Action = UITableViewRowAction.Create(
                                    UITableViewRowActionStyle.Default,
                                    SwipeActionTitle(swipeAction, documentPreview),
                                    (a, ip) =>
                                    {
                                        OnSwipeActionClick(swipeAction, indexPath, documentPreview, folder, tableView);
                                    });
                            }
                            break;
                        case EmailSwipeAction.SwipeAction.CopyToWorkTray:
                            actionWrapper.Action = UITableViewRowAction.Create(
                                UITableViewRowActionStyle.Default,
                                SwipeActionTitle(swipeAction, documentPreview),
                                (a, ip) =>
                                {
                                    OnSwipeActionClick(swipeAction, indexPath, documentPreview, folder, tableView);
                                });
                            break;
                        case EmailSwipeAction.SwipeAction.More:
                            actionWrapper.Action = UITableViewRowAction.Create(
                                UITableViewRowActionStyle.Default,
                                SwipeActionTitle(swipeAction, documentPreview),
                                (a, ip) =>
                                {
                                    OnSwipeActionClick(swipeAction, indexPath, documentPreview, folder, tableView);
                                });
                            break;
                        case EmailSwipeAction.SwipeAction.CopyToFolder:
                            actionWrapper.Action = UITableViewRowAction.Create(
                                UITableViewRowActionStyle.Default,
                                SwipeActionTitle(swipeAction, documentPreview),
                                (a, ip) =>
                                {
                                    OnSwipeActionClick(swipeAction, indexPath, documentPreview, folder, tableView);
                                });
                            break;
                        case EmailSwipeAction.SwipeAction.Categories:
                            actionWrapper.Action = UITableViewRowAction.Create(
                                UITableViewRowActionStyle.Default,
                                SwipeActionTitle(swipeAction, documentPreview),
                                (a, ip) =>
                                {
                                    OnSwipeActionClick(swipeAction, indexPath, documentPreview, folder, tableView);
                                });
                            break;
                        case EmailSwipeAction.SwipeAction.SetPriority:
                            actionWrapper.Action = UITableViewRowAction.Create(
                                UITableViewRowActionStyle.Default,
                                SwipeActionTitle(swipeAction, documentPreview),
                                (a, ip) =>
                                {
                                    OnSwipeActionClick(swipeAction, indexPath, documentPreview, folder, tableView);
                                });
                            break;
                        case EmailSwipeAction.SwipeAction.Delete:
                            actionWrapper.Action = UITableViewRowAction.Create(
                                UITableViewRowActionStyle.Default,
                                SwipeActionTitle(swipeAction, documentPreview),
                                (a, ip) =>
                                {
                                    OnSwipeActionClick(swipeAction, indexPath, documentPreview, folder, tableView);
                                });

                            actionWrapper.Disabled = !SwipeActionAllowed(swipeAction.Action, documentPreview, folder);
                            break;
                        case EmailSwipeAction.SwipeAction.MoveToFolder:
                            actionWrapper.Action = UITableViewRowAction.Create(
                                UITableViewRowActionStyle.Default,
                                SwipeActionTitle(swipeAction, documentPreview),
                                (a, ip) => { OnSwipeActionClick(swipeAction, indexPath, documentPreview, folder, tableView); });
                        
                            actionWrapper.Disabled = !SwipeActionAllowed(swipeAction.Action, documentPreview, folder);
                            break;
                        case EmailSwipeAction.SwipeAction.RemoveFromFolder:
                            actionWrapper.Action = UITableViewRowAction.Create(
                                UITableViewRowActionStyle.Default,
                                SwipeActionTitle(swipeAction, documentPreview),
                                (a, ip) =>
                                {
                                   OnSwipeActionClick(swipeAction, indexPath, documentPreview, folder, tableView);
                                });
                            actionWrapper.Disabled = !SwipeActionAllowed(swipeAction.Action, documentPreview, folder);
                            break;
                        default:
                            break;
                    }

                    actionWrappers.Add(actionWrapper);
                }

                for (int i = 0; i < actionWrappers.Count; i++)
                {
                    if (actionWrappers[i].Disabled)
                    {
                        actionWrappers[i].Action.BackgroundColor = Theme.LightGray;
                    }
                    else
                    {
                        if (i == 0)
                        {
                            actionWrappers[i].Action.BackgroundColor = Theme.Brown;
                        }
                        else if (i == 1)
                        {
                            actionWrappers[i].Action.BackgroundColor = Theme.DarkBlue;
                        }
                        else
                        {
                            actionWrappers[i].Action.BackgroundColor = Theme.DarkerBlue;
                        }
                    }
                }

                UITableViewRowAction[] returnActions = actionWrappers.Select(a => a.Action).ToArray();

                return returnActions;
            }


            string SwipeActionTitle(EmailSwipeAction swipeAction, DocumentPreview documentPreview)
            {
                switch (swipeAction.Action)
                {
                    case EmailSwipeAction.SwipeAction.MarkAsRead:
                        if (documentPreview.IsReadByCurrent)
                        {
                            return Localization.GetString("mark_as_unread_ml");
                        }
                        else
                        {
                            return Localization.GetString("mark_as_read_ml");
                        }

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
                    default:
                        CommonConfig.Logger.Error($"Missing implementation for EmailSwipeAction : {swipeAction.Action.ToString()}");
                        return "";
                }
            }

            void OnSwipeActionClick(EmailSwipeAction swipeAction, NSIndexPath indexPath, DocumentPreview documentPreview, Folder folder, UITableView tableView)
            {
                var popoverDelegate = new PopoverPresentationControllerDelegate(tableView, tableView.CellAt(indexPath));

                switch (swipeAction.Action)
                {
                    case EmailSwipeAction.SwipeAction.MarkAsRead:
                        if (documentPreview.IsReadByCurrent)
                        {
                            viewControllerWeakReference.Unwrap()?.MarkAsUnread(documentPreview);
                            viewControllerWeakReference.Unwrap()?.EndEditing();
                        }
                        else
                        {
                            viewControllerWeakReference.Unwrap()?.MarkAsRead(documentPreview);
                            viewControllerWeakReference.Unwrap()?.EndEditing();
                        }
                        break;
                    case EmailSwipeAction.SwipeAction.CopyToWorkTray:
                        viewControllerWeakReference.Unwrap()?.CopyToWorktray(documentPreview);
                        viewControllerWeakReference.Unwrap()?.EndEditing();
                        break;
                    case EmailSwipeAction.SwipeAction.More:
                        viewControllerWeakReference.Unwrap()?.ShowMoreActionSheet(indexPath, documentPreview, folder);
                        break;
                    case EmailSwipeAction.SwipeAction.CopyToFolder:
                        viewControllerWeakReference.Unwrap()?.CopyToFolder(documentPreview);
                        viewControllerWeakReference.Unwrap()?.EndEditing();
                        break;
                    case EmailSwipeAction.SwipeAction.Categories:
                        viewControllerWeakReference.Unwrap()?.ShowCategories(documentPreview);
                        viewControllerWeakReference.Unwrap()?.EndEditing();
                        break;
                    case EmailSwipeAction.SwipeAction.SetPriority:
                        viewControllerWeakReference.Unwrap()?.ShowPriorityActionSheet(documentPreview, tableView, indexPath);
                        break;
                    case EmailSwipeAction.SwipeAction.Delete:
                        if (SwipeActionAllowed(EmailSwipeAction.SwipeAction.Delete, documentPreview, folder))
                        {
                            viewControllerWeakReference.Unwrap()?.Delete(documentPreview, popoverDelegate);
                        }
                        break;
                    case EmailSwipeAction.SwipeAction.MoveToFolder:
                        if (SwipeActionAllowed(EmailSwipeAction.SwipeAction.Delete, documentPreview, folder))
                        {
                            viewControllerWeakReference.Unwrap()?.MoveToFolder(documentPreview);
                        }
                        break;

                    case EmailSwipeAction.SwipeAction.RemoveFromFolder:
                        viewControllerWeakReference.Unwrap()?.RemoveFromFolder(documentPreview, popoverDelegate);
                        break;
                    default:
                        CommonConfig.Logger.Error("Missed case for EmailSwipeAction : " + swipeAction.Action.ToString());
                        break;
                }
            }

            #endregion

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