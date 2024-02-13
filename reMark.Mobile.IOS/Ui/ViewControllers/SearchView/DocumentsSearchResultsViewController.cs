using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Extensions;
using reMark.Mobile.Common.Manager;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Model.HubMessages;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.Common.Utilities.Extensions;
using reMark.Mobile.IOS.Ui.Common;
using reMark.Mobile.IOS.Ui.TableViewCells;
using reMark.Mobile.IOS.Ui.ViewControllers.DocumentView;
using reMark.Mobile.IOS.Ui.ViewControllers.FoldersList;
using reMark.Mobile.IOS.Utilities;
using TinyMessenger;
using UIKit;

namespace reMark.Mobile.IOS.Ui.ViewControllers.SearchView
{
    public class DocumentsSearchResultsViewController : AbstractTableViewController, IPrimaryViewController,
        IUIGestureRecognizerDelegate, IUIViewControllerRestoration, IUISearchResultsUpdating
    {
        public SearchDocumentsCriteria Criteria { get; set; }

        UIBarButtonItem exitEditItem;
        UIBarButtonItem editItem;
        UIBarButtonItem closeItem;
        UIBarButtonItem closeLeftButton;
        UIBarButtonItem backBarButton;

        TinyMessageSubscriptionToken readStatusChangedToken;
        TinyMessageSubscriptionToken commentsCountChangedToken;
        TinyMessageSubscriptionToken categoriesChangedToken;
        TinyMessageSubscriptionToken goToDocumentToken;
        TinyMessageSubscriptionToken deletedToken;

        DocumentsSearchResultsFilterController searchResultsController;
        UISearchController searchController;

        CancellationTokenSource searchCancellationTokenSource;
        readonly List<CancellationTokenSource> searchCancellationTokenSourceList = new List<CancellationTokenSource>();
        string lastSearchQuery;


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

            RestorationIdentifier = nameof(DocumentsSearchResultsViewController);
            RestorationClass = Class;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (Integration.IsRunningAtLeast(11))
            {
                if (NavigationController != null)
                    NavigationController.NavigationBar.PrefersLargeTitles = false;
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
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info("Appeared");

            if (((DocumentsSearchResultsDataSource)TableView.Source).Empty)
                RefreshData();

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
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning("Received memory warning!");

            ((DocumentsSearchResultsDataSource)TableView.Source)?.Reset();

            UnsubscribeFromMessages();

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        protected override void Recycle()
        {
            base.Recycle();

            TableView.GestureRecognizers.ForEach(TableView.RemoveGestureRecognizer);
            ((DocumentsSearchResultsDataSource)TableView.Source)?.Reset();

            searchController.SearchResultsUpdater = null;
            searchController = null;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (CommonConfig.Logger.IsDebugEnabled())
                CommonConfig.Logger.Debug("Disposed");
        }

        void SubscribeToMessages()
        {
            readStatusChangedToken = CommonConfig.MessengerHub.Subscribe<DocumentPreviewReadStatusChangedMessage>(ReadStatusChangedHandler);
            commentsCountChangedToken = CommonConfig.MessengerHub.Subscribe<EntityPreviewCommentCountChangedMessage>(CommentsCountChangedHandler, m => m.ObjectType == ObjectType.Document);
            categoriesChangedToken = CommonConfig.MessengerHub.Subscribe<EntityCategoriesChangedMessage>(CategoriesChangedHandler, m => m.ObjectType == ObjectType.Document);
            goToDocumentToken = CommonConfig.MessengerHub.Subscribe<GoToDocumentMessage>(GoToDocumentHandler);
            deletedToken = CommonConfig.MessengerHub.Subscribe<EntityRemovedMessage>(DeletedHandler, m => m.ObjectType == ObjectType.Document);
        }

        void UnsubscribeFromMessages()
        {
            readStatusChangedToken?.Dispose();
            commentsCountChangedToken?.Dispose();
            categoriesChangedToken?.Dispose();
            goToDocumentToken?.Dispose();
            deletedToken?.Dispose();
        }

        #endregion

        #region Initialize/deinitialize

        void InitializeNavigationBar()
        {
            NavigationItem.Title = Localization.GetString("search_results");

            exitEditItem = new UIBarButtonItem(UIBarButtonSystemItem.Done);
            editItem = new UIBarButtonItem(UIBarButtonSystemItem.Edit);

            closeItem = new UIBarButtonItem
            {
                Title = Localization.GetString("close")
            };

            backBarButton = new UIBarButtonItem();
            backBarButton.SetTitleTextAttributes(new UIStringAttributes() { ForegroundColor = Theme.DarkerBlue }, UIControlState.Normal);
            NavigationItem.BackBarButtonItem = backBarButton;

            if (!Integration.IsRunningAtLeast(13) && Integration.IsIPad())
                NavigationItem.SetRightBarButtonItem(closeItem, false);

            if(Integration.IsiOSApplicationOnMac())
            {
                closeLeftButton = new UIBarButtonItem
                {
                    Title = Localization.GetString("close")
                };
                NavigationItem.SetLeftBarButtonItem(closeLeftButton, false);
            }
    
        }

        void InitializeSearchBar()
        {
            DefinesPresentationContext = true;

            searchResultsController = new DocumentsSearchResultsFilterController()
            {
                DocumentSearchResultsController = this
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

        void InitializeView()
        {
            TableView.Source = new DocumentsSearchResultsDataSource(this, TableView, PlatformConfig.Preferences.CompactDocumentsList);
            TableView.AllowsMultipleSelectionDuringEditing = true;
            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 60f;

            TableView.AddGestureRecognizer(new UILongPressGestureRecognizer(DocumentPreviewLongPressed));
        }

        void InitializeHandlers()
        {
            if (closeItem != null)
                closeItem.Clicked += CloseItem_Clicked;

            if (closeLeftButton != null)
                closeLeftButton.Clicked += CloseItem_Clicked;

            InitializeEditModeHandlers();
        }

        void DeinitializeHandlers()
        {
            if (closeItem != null)
                closeItem.Clicked -= CloseItem_Clicked;

            if (closeLeftButton != null)
                closeLeftButton.Clicked -= CloseItem_Clicked;

            DeinitializeEditModeHandlers();
        }

        public void InitializeEditModeHandlers()
        {
            if (exitEditItem != null)
                exitEditItem.Clicked += ExitEditItem_Clicked;


            if (editItem != null)
                editItem.Clicked += EditItem_Clicked;
        }

        public void DeinitializeEditModeHandlers()
        {
            if (exitEditItem != null)
                exitEditItem.Clicked -= ExitEditItem_Clicked;

            if (editItem != null)
                editItem.Clicked -= EditItem_Clicked;
        }

        public void InitializeEditModeActions(EventHandler editItemHandler, EventHandler exitEditItemHandler)
        {
            if (exitEditItem != null)
                exitEditItem.Clicked += exitEditItemHandler;


            if (editItem != null)
                editItem.Clicked += editItemHandler;
        }

        public void DeinitializeEditModeActions(EventHandler editItemHandler, EventHandler exitEditItemHandler)
        {
            if (exitEditItem != null)
                exitEditItem.Clicked -= exitEditItemHandler;

            if (editItem != null)
                editItem.Clicked -= editItemHandler;
        }

        #endregion

        #region NavigationBar handlers
        public void SetExitEditItemEnabled(bool enabled)
        {
            exitEditItem.Enabled = enabled;
        }

        private void CloseItem_Clicked(object sender, EventArgs e)
        {
            DismissViewController(true, null);
        }

        void ExitEditItem_Clicked(object sender, EventArgs e) => EndEditing(TableView);

        void EditItem_Clicked(object sender, EventArgs e)
        {
            if (TableView.IndexPathsForSelectedRows == null || TableView.IndexPathsForSelectedRows.Length < 1)
                return;

            var eas = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            var d = new PopoverPresentationControllerDelegate((UIBarButtonItem)sender);

            var rows = TableView.IndexPathsForSelectedRows.ToArray();
            var selectedDocuments = rows.Select(ip => ((DocumentsSearchResultsDataSource)TableView.Source).Items[ip.Row]).ToList();

            if (selectedDocuments.Any(dp => !dp.IsReadByCurrent))
                eas.AddAction(UIAlertAction.Create(Localization.GetString("mark_as_read"),
                    UIAlertActionStyle.Default,
                    a =>
                    {
                        MarkAsRead(selectedDocuments, rows);
                        EndEditing(TableView);
                    }));

            if (selectedDocuments.Any(dp => dp.IsReadByCurrent))
                eas.AddAction(UIAlertAction.Create(Localization.GetString("mark_as_unread"),
                    UIAlertActionStyle.Default,
                    a =>
                    {
                        MarkAsUnread(selectedDocuments, rows);
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
                    var vc = new CopyMoveToFolderListViewController(ModuleType.Documents, selectedDocuments.Cast<IBusinessEntity>().ToList());
                    PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
                }));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("set_priority"), UIAlertActionStyle.Default,
                a => ShowPriorityActionSheet(selectedDocuments, (UIBarButtonItem)sender, rows)));

            if (ServerConfig.SystemSettings.DocumentsModuleInfo.Permissions.DeleteAllowed || selectedDocuments.All(dp => dp.Direction == DocumentDirection.Draft))
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete"), UIAlertActionStyle.Destructive, a => Delete(selectedDocuments, d)));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, a => exitEditItem.Enabled = true));

            if (eas.PopoverPresentationController != null)
                eas.PopoverPresentationController.Delegate = d;

            PresentViewController(eas, true, null);
        }

        #endregion

        #region Refreshing

        async void RefreshData()
        {
            try
            {
                CommonConfig.Logger.Info($"Refreshing documents list... [criteria={Criteria}]");

                var results = await Managers.SearchManager.SearchDocumentsAsync(Criteria);

                if (CommonConfig.Logger.IsDebugEnabled())
                    CommonConfig.Logger.Debug($"Retrieved {results.Count} items");

                ((DocumentsSearchResultsDataSource)TableView.Source).AppendItems(results);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Could not refresh documents list [criteria={Criteria}]", ex);

                await Dialogs.ShowErrorAlertAsync(this, ex);

                if (Integration.IsIPad())
                    DismissViewController(true, null);
                else
                    NavigationController?.PopViewController(true);
            }
        }

        #endregion

        #region IUISearchResultsUpdating
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
                ((DocumentsSearchResultsDataSource)dataSource)?.Reset();
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
        #endregion

        #region List handlers

        public void DocumentSelected(DocumentPreview documentPreview)
        {
            if (Integration.IsIPad())
            {
                var nc = (UINavigationController)SplitViewController.ViewControllers[1];
                nc.PopToViewController(nc.ViewControllers[0], false);

                var vc = (DocumentPageViewController)nc.ViewControllers[0];
                vc.DocumentPreviews = ((DocumentsSearchResultsDataSource)TableView.Source).Items;

                if (vc.IsShowingDocumentWithId(documentPreview.Id))
                    return;

                vc.HidesBottomBarWhenPushed = false;
                vc.SetPage(null, documentPreview, false);
            }        
            else
            {
                var vc = new DocumentViewController(Integration.IsIPad());
                vc.SetData(documentPreview, true);
                vc.SetRefreshDataOnAppear();
                NavigationController.PushViewController(vc, true);
            }
        }

        public void DocumentPreviewLongPressed(UILongPressGestureRecognizer recognizer)
        {
            if (TableView.Editing || ((DocumentsSearchResultsDataSource)TableView.Source).Empty)
                return;

            StartEditing(TableView);

            var point = recognizer.LocationInView(TableView);
            var indexPath = TableView.IndexPathForRowAtPoint(point);

            if (indexPath == null)
                return;

            if (!TableView.CellAt(indexPath)?.UserInteractionEnabled ?? true)
                return;

            TableView.SelectRow(indexPath, true, UITableViewScrollPosition.None);
        }

        #endregion

        #region Actions

        public void ShowMoreActionSheet(NSIndexPath indexPath, DocumentPreview selectedDocument)
        {
            var eas = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            var d = new PopoverPresentationControllerDelegate(TableView, TableView.CellAt(indexPath));

            if (ServerConfig.SystemSettings.DocumentsModuleInfo.WorktrayEnabled ?? true)
            {
                eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_worktray"),
                                                   UIAlertActionStyle.Default,
                                                   a =>
                {
                    this.CopyToWorktray(selectedDocument);
                    EndEditing(TableView);
                }));
            }

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_folder"),
                UIAlertActionStyle.Default,
                a =>
                {
                    this.CopyToFolder(selectedDocument);
                    EndEditing(TableView);
                }));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("set_priority"),
                UIAlertActionStyle.Default,
                a => ShowPriorityActionSheet(selectedDocument, TableView, TableView.CellAt(indexPath), new[] {indexPath})));

            if (ServerConfig.SystemSettings.DocumentsModuleInfo.Permissions.DeleteAllowed || selectedDocument.Direction == DocumentDirection.Draft)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete"), UIAlertActionStyle.Destructive, a => Delete(selectedDocument, d)));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, null));

            if (eas.PopoverPresentationController != null)
                eas.PopoverPresentationController.Delegate = d;

            PresentViewController(eas, true, null);
        }

        public void MarkAsRead(DocumentPreview selectedDocument, NSIndexPath row) =>
            MarkAsRead(new List<DocumentPreview> { selectedDocument }, new[] { row });

        public async void MarkAsRead(List<DocumentPreview> selectedDocuments, NSIndexPath[] rows)
        {
            CommonConfig.Logger.Info($"Attempting to mark as read [documentPreviews={selectedDocuments.Count}]...");

            try
            {
                CommonConfig.UsageAnalytics.LogEvent(new SetReadStatusEvent(selectedDocuments.Count));

                await Managers.DocumentsManager.SetDocumentsReadStatusAsync(selectedDocuments, true);
                var updatedItems = selectedDocuments.Select(d => d.Id);
                ((DocumentsSearchResultsDataSource)(TableView.Source)).UpdateItems(updatedItems);
                ((DocumentsSearchResultsDataSource)((UITableViewController)searchController?.SearchResultsController)?.TableView?.Source)?.UpdateItems(updatedItems);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Marking as read failed [documentPreviews.Count={selectedDocuments.Count}]", ex);

                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }


        public async void MarkAsRead(List<DocumentPreview> documentPreviews)
        {
            CommonConfig.Logger.Info($"Attempting to mark as read [documentPreviews={documentPreviews.Count}]...");

            try
            {
                CommonConfig.UsageAnalytics.LogEvent(new SetReadStatusEvent(documentPreviews.Count));

                await Managers.DocumentsManager.SetDocumentsReadStatusAsync(documentPreviews, true);

                if (PlatformConfig.Preferences.SyncUserActivities)
                    await Managers.DocumentsManager.ExecuteUserActivity(UserActivityType.Read, documentPreviews);

                var updatedItems = documentPreviews.Select(d => d.Id);
                ((DocumentsSearchResultsDataSource)TableView.Source).UpdateItems(updatedItems);
                ((DocumentsSearchResultsDataSource)((UITableViewController)searchController?.SearchResultsController)?.TableView?.Source)?.UpdateItems(updatedItems);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Marking as read failed [documentPreviews.Count={documentPreviews.Count}]", ex);

                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        public void MarkAsUnread(DocumentPreview documentPreview, NSIndexPath row) =>
            MarkAsUnread(new List<DocumentPreview> { documentPreview }, new[] { row });

        public async void MarkAsUnread(List<DocumentPreview> documentPreviews, NSIndexPath[] rows)
        {
            CommonConfig.Logger.Info($"Attempting to mark as unread [documentPreviews={documentPreviews.Count}]...");

            try
            {
                CommonConfig.UsageAnalytics.LogEvent(new SetReadStatusEvent(documentPreviews.Count));

                await Managers.DocumentsManager.SetDocumentsReadStatusAsync(documentPreviews, false);
                var updatedItems = documentPreviews.Select(d => d.Id);
                ((DocumentsSearchResultsDataSource)TableView.Source).UpdateItems(updatedItems);
                ((DocumentsSearchResultsDataSource)((UITableViewController)searchController?.SearchResultsController)?.TableView?.Source)?.UpdateItems(updatedItems);

            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Marking as unread failed [documentPreviews.Count={documentPreviews.Count}]", ex);

                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        public async void ShowPriorityActionSheet(List<DocumentPreview> selectedDocuments, UIBarButtonItem barButtonItem, NSIndexPath[] rows)
        {
            var priorities = new List<Priority> { Priority.Low, Priority.Normal, Priority.Urgent };
            var priorityStrings = priorities.Select(p => UI.PrettyPriorityString(p));
            var result = await Dialogs.ShowListActionSheetAsync(this, priorityStrings.ToArray(), barButtonItem);

            if (result < 0)
                return;

            var priority = priorities[result];

            await SetPriority(selectedDocuments, priority, rows);
        }

        async void ShowPriorityActionSheet(DocumentPreview selectedDocument, UITableView tv, UITableViewCell cell, NSIndexPath[] rows)
        {
            var priorities = new List<Priority> { Priority.Low, Priority.Normal, Priority.Urgent };
            var priorityStrings = priorities.Select(p => UI.PrettyPriorityString(p));
            var result = await Dialogs.ShowListActionSheetAsync(this, priorityStrings.ToArray(), tv, cell);

            if (result < 0)
                return;

            var priority = priorities[result];
            await SetPriority(new List<DocumentPreview> { selectedDocument }, priority, rows);
        }

        async Task SetPriority(List<DocumentPreview> selectedDocuments, Priority priority, NSIndexPath[] rows)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("setting_priority___"));

            try
            {
                CommonConfig.Logger.Info($"Attempting to setting priority for documents");
                await Managers.DocumentsManager.SetDocumentsPriorityAsync(selectedDocuments, priority);
                var updatedItems = selectedDocuments.Select(d => d.Id);
                ((DocumentsSearchResultsDataSource)TableView.Source).UpdateItems(updatedItems);
                ((DocumentsSearchResultsDataSource)((UITableViewController)searchController?.SearchResultsController)?.TableView?.Source)?.UpdateItems(updatedItems);
                dismissAction();
                EndEditing(TableView);

            }
            catch (Exception ex)
            { 
                dismissAction();
                EndEditing(TableView);
                CommonConfig.Logger.Error($"Error while setting priority for documents", ex);
                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }


        void Delete(DocumentPreview selectedDocument, UIPopoverPresentationControllerDelegate d) =>
            Delete(new List<DocumentPreview> { selectedDocument }, d);

        public async void Delete(List<DocumentPreview> selectedDocuments, UIPopoverPresentationControllerDelegate d)
        {
            var result = await Dialogs.ShowDestructiveActionSheetAsync(this, Localization.GetString("delete"), d);
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

        #endregion

        #region Message handlers

        void RemoveDocumentsFromList(IEnumerable<int> ids)
        {
            BeginInvokeOnMainThread(() =>
            {
                ((DocumentsSearchResultsDataSource)TableView.Source).RemoveItems(ids.ToList());
            });
        }

        void ReadStatusChangedHandler(DocumentPreviewReadStatusChangedMessage message)
        {
            BeginInvokeOnMainThread(() =>
            {
                var ds = TableView.Source as DocumentsSearchResultsDataSource;
                var index = ds.Items.FindIndex(dp => dp.Id == message.DocumentPreviewId);

                if (index >= 0)
                {
                    var documentPreview = ds.Items[index];
                    documentPreview.IsReadByCurrent = message.IsReadByCurrent;
                    documentPreview.IsReadByAnyone = message.IsReadByAnyone;

                    var selectedRow = TableView.IndexPathForSelectedRow;

                    TableView.ReloadRows(new[] { NSIndexPath.FromRowSection(index, 0) }, UITableViewRowAnimation.Fade);

                    if (selectedRow != null)
                        TableView.SelectRow(selectedRow, false, UITableViewScrollPosition.None);
                }
            });
        }

        void CommentsCountChangedHandler(EntityPreviewCommentCountChangedMessage message)
        {
            BeginInvokeOnMainThread(() =>
            {
                var ds = TableView.Source as DocumentsSearchResultsDataSource;
                var index = ds.Items.FindIndex(dp => dp.Id == message.EntityId);

                if (index >= 0)
                {
                    var documentPreview = ds.Items[index];
                    documentPreview.CommentsCount = message.CommentsCount;

                    var selectedRow = TableView.IndexPathForSelectedRow;

                    TableView.ReloadRows(new[] { NSIndexPath.FromRowSection(index, 0) }, UITableViewRowAnimation.Fade);

                    if (selectedRow != null)
                        TableView.SelectRow(selectedRow, false, UITableViewScrollPosition.None);
                }
            });
        }

        void CategoriesChangedHandler(EntityCategoriesChangedMessage message)
        {
            BeginInvokeOnMainThread(() =>
            {
                var ds = TableView.Source as DocumentsSearchResultsDataSource;
                var index = ds.Items.FindIndex(dp => dp.Id == message.EntityId);

                if (index >= 0)
                {
                    var documentPreview = ds.Items[index];
                    documentPreview.Categories.Clear();
                    documentPreview.Categories.AddRange(message.Categories);

                    var selectedRow = TableView.IndexPathForSelectedRow;

                    TableView.ReloadRows(new[] { NSIndexPath.FromRowSection(index, 0) }, UITableViewRowAnimation.Fade);

                    if (selectedRow != null)
                        TableView.SelectRow(selectedRow, false, UITableViewScrollPosition.None);
                }

            });
        }

        void GoToDocumentHandler(GoToDocumentMessage message)
        {
            BeginInvokeOnMainThread(() =>
            {
                var index = ((DocumentsSearchResultsDataSource)TableView.Source).Items.FindIndex(dp => dp.Id == message.DocumentId);

                if (index >= 0)
                {
                    TableView.SelectRow(NSIndexPath.FromRowSection(index, 0), true, UITableViewScrollPosition.None);
                    TableView.ScrollToRow(NSIndexPath.FromRowSection(index, 0), UITableViewScrollPosition.None, true);
                }
            });
        }

        void DeletedHandler(EntityRemovedMessage m) => RemoveDocumentsFromList(m.EntitiesId);

        #endregion

        #region Utilities

        public void StartEditing(UITableView tableView)
        {
            tableView.SetEditing(true, true);
            NavigationItem.SetRightBarButtonItem(exitEditItem, true);
            NavigationItem.SetLeftBarButtonItem(editItem, true);
        }

        public void EndEditing(UITableView tableView)
        {
            tableView.SetEditing(false, true);
            NavigationItem.SetRightBarButtonItem(null, true);
            NavigationItem.SetLeftBarButtonItem(null, true);
            NavigationItem.HidesBackButton = false;
            NavigationItem.BackBarButtonItem = backBarButton;

            if (!Integration.IsRunningAtLeast(13) && Integration.IsIPad())
                NavigationItem.SetRightBarButtonItem(closeItem, false);
        }

        #endregion

        #region State restoration

        public override void EncodeRestorableState(NSCoder coder)
        {
            base.EncodeRestorableState(coder);
            coder.Encode(Serializer.SerializeToByteArray(Criteria), "criteria");
        }

        public override void DecodeRestorableState(NSCoder coder)
        {
            base.DecodeRestorableState(coder);
            Criteria = Serializer.DeserializeFromByteArray<SearchDocumentsCriteria>(coder.DecodeBytes("criteria"));
        }

        [Export("viewControllerWithRestorationIdentifierPath:coder:")]
        public static UIViewController Restore(string[] identifierComponents, NSCoder coder)
        {
            return new DocumentsSearchResultsViewController();
        }
        #endregion

    }
}