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
using Mark5.Mobile.Common.Service;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Model.HubMessages;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentView;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;
using Mark5.Mobile.IOS.Utilities;
using ObjCRuntime;
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

        AutoRefreshWorker autoRefreshWorker;
        Action newDocumentsAvailableAction;

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

            NavigationController.NavigationBar.PrefersLargeTitles = true;
            NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Automatic;

            RestorationIdentifier = nameof(DocumentsListViewController);
            RestorationClass = Class;
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

            autoRefreshWorker?.Stop();
            autoRefreshWorker = null;

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

                var vc = (DocumentViewController)nc.ViewControllers[0];
                vc.ClearData();
            }
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning($"{nameof(DocumentsListViewController)} received memory warning!");

            autoRefreshWorker?.Stop();
            autoRefreshWorker = null;

            ((DataSource)TableView.Source).Reset();

            UnsubscribeFromMessages();

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        public override void Recycle()
        {
            base.Recycle();

            ((DataSource)TableView.Source)?.Reset();
            TableView.GestureRecognizers.ForEach(TableView.RemoveGestureRecognizer);
            UnsubscribeFromMessages();
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
            NavigationItem.Title = Folder.Name;

            composeDocumentItem = new UIBarButtonItem
            {
                Image = UIImage.FromBundle(Path.Combine("icons", "compose.png"))
            };
            NavigationItem.SetRightBarButtonItem(composeDocumentItem, false);

            exitEditItem = new UIBarButtonItem(UIBarButtonSystemItem.Done);
            editItem = new UIBarButtonItem(UIBarButtonSystemItem.Edit);
        }

        void InitializeView()
        {
            RefreshControl = new UIRefreshControl();

            TableView.Source = new DataSource(this, TableView, Localization.GetString("folder_empty"));
            TableView.RefreshControl = RefreshControl;
            TableView.AllowsMultipleSelectionDuringEditing = true;

            TableView.AddGestureRecognizer(new UILongPressGestureRecognizer(DocumentPreviewLongPressed));
        }

        void InitializeSearchBar()
        {
            DefinesPresentationContext = true;

            var searchResultsController = new UITableViewController();
            var searchResultsDataSource = new DataSource(this, searchResultsController.TableView, Localization.GetString("no_matching_documents"));
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
            commentsCountChangedToken = CommonConfig.MessengerHub.Subscribe<DocumentPreviewCommentsCountChangedMessage>(CommentsCountChangedHandler);
            categoriesChangedToken = CommonConfig.MessengerHub.Subscribe<EntityCategoriesChangedMessage>(CategoriesChangedHandler, m => m.ObjectType == ObjectType.Document);
            removedFromFolderToken = CommonConfig.MessengerHub.Subscribe<EntityRemovedFromFolderMessage>(HandleRemovedFromFolder, m => m.ObjectType == ObjectType.Document);
            movedFromFolderToken = CommonConfig.MessengerHub.Subscribe<EntityMovedFromFolderMessage>(HandleMovedFromFolder, m => m.ObjectType == ObjectType.Document);
            deletedToken = CommonConfig.MessengerHub.Subscribe<EntityDeletedMessage>(HandleDeleted, m => m.ObjectType == ObjectType.Document);
        }

        void UnsubscribeFromMessages()
        {
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

            var rows = TableView.IndexPathsForSelectedRows.ToArray();
            var selectedDocuments = rows.Select(ip => ((DataSource)TableView.Source).Items[ip.Row]).ToList();

            if (selectedDocuments.Any(dp => !dp.IsReadByCurrent))
                eas.AddAction(UIAlertAction.Create(Localization.GetString("mark_as_read"),
                    UIAlertActionStyle.Default,
                    a =>
                    {
                        MarkAsRead(selectedDocuments, rows);
                        EndEditing();
                    }));

            if (selectedDocuments.Any(dp => dp.IsReadByCurrent))
                eas.AddAction(UIAlertAction.Create(Localization.GetString("mark_as_unread"),
                    UIAlertActionStyle.Default,
                    a =>
                    {
                        MarkAsUnread(selectedDocuments, rows);
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
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete_from_folder"), UIAlertActionStyle.Default, a => RemoveFromFolder(selectedDocuments)));

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator || ServerConfig.SystemSettings.DocumentsModuleInfo.Permissions.DeleteAllowed || selectedDocuments.All(dp => dp.Direction == DocumentDirection.Draft))
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete"), UIAlertActionStyle.Destructive, a => Delete(selectedDocuments)));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, a => exitEditItem.Enabled = true));

            if (eas.PopoverPresentationController != null)
                eas.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate((UIBarButtonItem)sender);

            PresentViewController(eas, true, null);
        }

        #endregion

        #region Refreshing

        async void RefreshControl_ValueChanged(object sender, EventArgs e) => await RefreshData(forceClear: true);

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
                    ((DataSource)TableView.Source).Reset();

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

                await Dialogs.ShowErrorDialogAsync(this, ex);

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

                var vc = (DocumentViewController)nc.ViewControllers[0];

                if (vc.IsShowingDocumentWithId(documentPreview.Id))
                    return;

                vc.HidesBottomBarWhenPushed = false;

                vc.ClearData();
                vc.ReadStatusUpdated -= DocumentViewController_ReadStatusUpdated;
                vc.ReadStatusUpdated += DocumentViewController_ReadStatusUpdated;

                if (!searchController.Active)
                {
                    vc.SetData(Folder, documentPreview, GetNextDocumentPreview, GetPreviousDocumentPreview);
                    newDocumentsAvailableAction = vc.RefreshNavigationBar;
                }
                else
                {
                    vc.SetData(Folder, documentPreview);
                    newDocumentsAvailableAction = null;
                }

                vc.RefreshData();
            }
            else
            {
                var vc = new DocumentViewController();
                vc.ReadStatusUpdated += DocumentViewController_ReadStatusUpdated;
                vc.OnComplete = () => vc.ReadStatusUpdated -= DocumentViewController_ReadStatusUpdated;
                if (!searchController.Active)
                    vc.SetData(Folder, documentPreview, GetNextDocumentPreview, GetPreviousDocumentPreview);
                else
                    vc.SetData(Folder, documentPreview);
                vc.SetRefreshDataOnAppear();

                newDocumentsAvailableAction = vc.RefreshNavigationBar;
                NavigationController.PushViewController(vc, true);
            }
        }

        void DocumentPreviewLongPressed(UILongPressGestureRecognizer recognizer)
        {
            if (TableView.Editing)
                return;

            StartEditing();

            var point = recognizer.LocationInView(TableView);
            var indexPath = TableView.IndexPathForRowAtPoint(point);

            TableView.SelectRow(indexPath, true, UITableViewScrollPosition.None);
        }

        #endregion

        #region Actions

        void ShowMoreActionSheet(NSIndexPath indexPath, DocumentPreview selectedDocument)
        {
            var eas = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);

            eas.AddAction(UIAlertAction.Create(Localization.GetString("categories"),
                UIAlertActionStyle.Default,
                a =>
                {
                    ShowCategories(selectedDocument);
                    EndEditing();
                }));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("reply"),
                UIAlertActionStyle.Default,
                a =>
                {
                    Respond(selectedDocument, DocumentCreationModeFlag.Reply);
                    EndEditing();
                }));
            eas.AddAction(UIAlertAction.Create(Localization.GetString("reply_all"),
                UIAlertActionStyle.Default,
                a =>
                {
                    Respond(selectedDocument, DocumentCreationModeFlag.ReplyAll);
                    EndEditing();
                }));
            eas.AddAction(UIAlertAction.Create(Localization.GetString("forward"),
                UIAlertActionStyle.Default,
                a =>
                {
                    Respond(selectedDocument, DocumentCreationModeFlag.Forward);
                    EndEditing();
                }));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_folder"),
                UIAlertActionStyle.Default,
                a =>
                {
                    CopyToFolder(selectedDocument);
                    EndEditing();
                }));

            if (Folder.InternalType == FolderInternalType.FilterView || Folder.InternalType == FolderInternalType.Static || Folder.InternalType == FolderInternalType.Worktray)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("move_to_folder"),
                    UIAlertActionStyle.Default,
                    a =>
                    {
                        MoveToFolder(selectedDocument);
                        EndEditing();
                    }));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("set_priority"), UIAlertActionStyle.Default, a => ShowPriorityActionSheet(selectedDocument, TableView, TableView.CellAt(indexPath))));

            if (Folder.InternalType == FolderInternalType.FilterView || Folder.InternalType == FolderInternalType.Static || Folder.InternalType == FolderInternalType.Worktray)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete_from_folder"), UIAlertActionStyle.Default, a => RemoveFromFolder(selectedDocument)));

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator || ServerConfig.SystemSettings.DocumentsModuleInfo.Permissions.DeleteAllowed || selectedDocument.Direction == DocumentDirection.Draft)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete"), UIAlertActionStyle.Destructive, a => Delete(selectedDocument)));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, null));

            if (eas.PopoverPresentationController != null)
                eas.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate(TableView, TableView.CellAt(indexPath));

            PresentViewController(eas, true, null);
        }

        async void ShowPriorityActionSheet(List<DocumentPreview> selectedDocuments, UIBarButtonItem barButtonItem)
        {
            var priorities = new[] { Priority.Low, Priority.Normal, Priority.Urgent };
            var priorityStrings = priorities.Select(p => UI.PrettyPriorityString(p)).ToArray();
            var result = await Dialogs.ShowListDialogAsync(this, Localization.GetString("select_priority"), priorityStrings, barButtonItem);

            if (result < 0)
                return;

            var priority = priorities[result];

            await SetPriority(selectedDocuments, priority);
        }

        async void ShowPriorityActionSheet(DocumentPreview selectedDocument, UITableView tv, UITableViewCell cell)
        {
            var priorities = new List<Priority>
            {
                Priority.Low,
                Priority.Normal,
                Priority.Urgent
            };
            var priorityStrings = priorities.Select(p => UI.PrettyPriorityString(p));
            var result = await Dialogs.ShowListDialogAsync(this, Localization.GetString("select_priority"), priorityStrings.ToArray(), tv, cell);

            if (result < 0)
                return;

            var priority = priorities[result];

            await SetPriority(new List<DocumentPreview>
                {
                    selectedDocument
                },
                priority);
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
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        void RemoveFromFolder(DocumentPreview selectedDocument)
        {
            RemoveFromFolder(new List<DocumentPreview>
            {
                selectedDocument
            });
        }

        async void RemoveFromFolder(List<DocumentPreview> selectedDocuments)
        {
            var result = await Dialogs.ShowYesNoDialogAsync(this, Localization.GetString("delete_from_folder"), Localization.GetString("confirm_delete_from_folder_documents"));

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
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        void Delete(DocumentPreview selectedDocument)
        {
            Delete(new List<DocumentPreview>
            {
                selectedDocument
            });
        }

        async void Delete(List<DocumentPreview> selectedDocuments)
        {
            var result = await Dialogs.ShowYesNoDialogAsync(this, Localization.GetString("delete"), Localization.GetString("confirm_delete_documents"));

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
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        void Respond(DocumentPreview documentPreview, DocumentCreationModeFlag creationModeFlag)
        {
            var vc = new ComposeDocumentViewController
            {
                DocumentCreationModeFlag = creationModeFlag,
                PreviousDocumentDirection = documentPreview.Direction,
                PreviousDocumentFolderId = Folder.Id,
                PreviousDocumentId = documentPreview.Id
            };

            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        void ShowCategories(DocumentPreview selectedDocument)
        {
            var vc = new CategoriesListViewController
            {
                BusinessEntityPreview = selectedDocument
            };
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        void CopyToFolder(DocumentPreview selectedDocument)
        {
            CopyToFolder(new List<DocumentPreview>
            {
                selectedDocument
            });
        }

        void CopyToFolder(List<DocumentPreview> selectedDocument)
        {
            var vc = new CopyMoveToFolderListViewController(selectedDocument.Cast<IBusinessEntity>().ToList());
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        void MoveToFolder(DocumentPreview selectedDocument)
        {
            MoveToFolder(new List<DocumentPreview>
            {
                selectedDocument
            });
        }

        void MoveToFolder(List<DocumentPreview> selectedDocument)
        {
            var vc = new CopyMoveToFolderListViewController(selectedDocument.Cast<IBusinessEntity>().ToList(), Folder)
            {
                ModalPresentationStyle = UIModalPresentationStyle.PageSheet
            };
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        void CopyToWorktray(DocumentPreview selectedDocument)
        {
            CopyToWorktray(new List<DocumentPreview>
            {
                selectedDocument
            });
        }

        void CopyToWorktray(List<DocumentPreview> selectedDocuments)
        {
            var vc = new CopyToWorktrayViewController
            {
                BusinessEntities = selectedDocuments.Cast<IBusinessEntity>().ToList()
            };
            vc.ModalPresentationStyle = UIModalPresentationStyle.PageSheet;
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        void MarkAsRead(DocumentPreview documentPreview, NSIndexPath row) =>
            MarkAsRead(new List<DocumentPreview> { documentPreview }, new[] { row });

        async void MarkAsRead(List<DocumentPreview> documentPreviews, NSIndexPath[] rows)
        {
            CommonConfig.Logger.Info($"Attempting to mark as read [documentPreviews={documentPreviews.Count}]...");

            try
            {
                await Managers.DocumentsManager.SetDocumentsReadStatusAsync(documentPreviews, true);
                TableView.ReloadRows(rows, UITableViewRowAnimation.Fade);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Marking as read failed [documentPreviews.Count={documentPreviews.Count}]", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        void MarkAsUnread(DocumentPreview documentPreview, NSIndexPath row) =>
            MarkAsUnread(new List<DocumentPreview> { documentPreview }, new[] { row });

        async void MarkAsUnread(List<DocumentPreview> documentPreviews, NSIndexPath[] rows)
        {
            CommonConfig.Logger.Info($"Attempting to mark as unread [documentPreviews={documentPreviews.Count}]...");

            try
            {
                await Managers.DocumentsManager.SetDocumentsReadStatusAsync(documentPreviews, false);
                TableView.ReloadRows(rows, UITableViewRowAnimation.Fade);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Marking as unread failed [documentPreviews.Count={documentPreviews.Count}]", ex);

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

                var dataSource = ((UITableViewController)searchController.SearchResultsController).TableView.Source;
                ((DataSource)dataSource).Reset();
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

            var filteredDocuments = dataSource?.Items.Where(dp => MatchesQuery(dp, searchText)).ToList();

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

        void HandleRemovedFromFolder(EntityRemovedFromFolderMessage m) => RemoveDocumentsFromList(m.EntitiesId);

        void HandleMovedFromFolder(EntityMovedFromFolderMessage m) => RemoveDocumentsFromList(m.EntitiesId);

        void HandleDeleted(EntityDeletedMessage m) => RemoveDocumentsFromList(m.EntitiesId);

        #endregion

        #region Event handlers

        void DocumentViewController_ReadStatusUpdated(object sender, ReadStatusUpdatedEventArgs e)
        {
            BeginInvokeOnMainThread(() =>
            {
                var selectedRow = TableView.IndexPathForSelectedRow;

                ((DataSource)TableView.Source).UpdateItem(e.DocumentPreview.Id);
                TableView.ReloadData();

                if (selectedRow != null)
                    TableView.SelectRow(selectedRow, false, UITableViewScrollPosition.None);
            });
        }

        void CommentsCountChangedHandler(DocumentPreviewCommentsCountChangedMessage message)
        {
            BeginInvokeOnMainThread(() =>
            {
                var index = ((DataSource)TableView.Source).Items.FindIndex(dp => dp.Id == message.DocumentPreviewId);

                if (index >= 0)
                {
                    var documentPreview = ((DataSource)TableView.Source).Items[index];
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
                var index = ((DataSource)TableView.Source).Items.FindIndex(dp => dp.Id == message.EntityId);

                if (index >= 0)
                {
                    var documentPreview = ((DataSource)TableView.Source).Items[index];
                    documentPreview.Categories.Clear();
                    documentPreview.Categories.AddRange(message.Categories);

                    var selectedRow = TableView.IndexPathForSelectedRow;

                    TableView.ReloadRows(new[] { NSIndexPath.FromRowSection(index, 0) }, UITableViewRowAnimation.Fade);

                    if (selectedRow != null)
                        TableView.SelectRow(selectedRow, false, UITableViewScrollPosition.None);
                }
            });
        }

        #endregion

        #region Utilities

        void StartEditing()
        {
            if (SplitViewController != null && !SplitViewController.Collapsed)
            {
                var nc = (UINavigationController)SplitViewController.ViewControllers[1];
                nc.PopToRootViewController(false);

                var vc = (DocumentViewController)nc.ViewControllers[0];
                vc.ClearData();
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
            if (SplitViewController != null && !SplitViewController.Collapsed)
            {
                var nc = (UINavigationController)SplitViewController.ViewControllers[1];
                var vc = (DocumentViewController)nc.ViewControllers[0];
                if (ids.Select(id => vc.IsShowingDocumentWithId(id)).Any(v => v))
                    vc.UpdatePriority();
            }
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
                    var vc = (DocumentViewController)nc.ViewControllers[0];
                    if (ids.Select(id => vc.IsShowingDocumentWithId(id)).Any(v => v))
                        vc.ClearData();
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
            public List<DocumentPreview> Items { get; private set; } = new List<DocumentPreview>(1000);
            public bool LoadMoreEnabled { get; set; }
            public bool CompactList { get; set; }

            readonly WeakReference<DocumentsListViewController> viewControllerWeakReference;
            readonly WeakReference<UITableView> tableViewWeakReference;
            readonly string emptyText;

            bool loading = true;

            public DataSource(DocumentsListViewController viewController, UITableView tableView, string emptyText)
            {
                viewControllerWeakReference = viewController.Wrap();
                tableViewWeakReference = tableView.Wrap();
                this.emptyText = emptyText;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as WaitTableViewCell ?? WaitTableViewCell.Create();

                if (Items.Count < 1)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                    emptyCell.Initialize(emptyText);
                    return emptyCell;
                }

                var dp = Items[indexPath.Row];

                if (LoadMoreEnabled && dp.Id == Items.Last().Id)
                    AsyncHelpers.FireAndForget(viewControllerWeakReference.Unwrap()?.RefreshData(dp.Id));

                if (dp.Direction == DocumentDirection.External)
                {
                    var cell = tableView.DequeueReusableCell(ExternalDocumentsTableViewCell.Key) as ExternalDocumentsTableViewCell ?? ExternalDocumentsTableViewCell.Create();
                    cell.Initialize(dp);
                    return cell;
                }

                if (CompactList)
                {
                    var cell = tableView.DequeueReusableCell(DocumentsCompactTableViewCell.Key) as DocumentsCompactTableViewCell ?? DocumentsCompactTableViewCell.Create();
                    cell.Initialize(dp);
                    return cell;
                }
                else
                {
                    var cell = tableView.DequeueReusableCell(DocumentsTableViewCell.Key) as DocumentsTableViewCell ?? DocumentsTableViewCell.Create();
                    cell.Initialize(dp);
                    return cell;
                }
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                if (Items.Count > 0 && Items[indexPath.Row]?.Direction == DocumentDirection.External)
                    return ExternalDocumentsTableViewCell.Height;

                return CompactList ? DocumentsCompactTableViewCell.Height : DocumentsTableViewCell.Height;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading)
                    return 1;

                if (Items.Count < 1)
                    return 1;

                return Items.Count;
            }

            public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath) => true;

            public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
            {
                var actions = new List<UITableViewRowAction>();

                var documentPreview = Items[indexPath.Row];

                if (documentPreview.IsReadByCurrent)
                {
                    var markAsUnreadAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Default,
                        Localization.GetString("mark_as_unread_ml"),
                        (a, ip) =>
                        {
                            viewControllerWeakReference.Unwrap()?.MarkAsUnread(documentPreview, indexPath);
                            viewControllerWeakReference.Unwrap()?.EndEditing();
                        });
                    markAsUnreadAction.BackgroundColor = Theme.Brown;
                    actions.Add(markAsUnreadAction);
                }
                else
                {
                    var markAsReadAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Default,
                        Localization.GetString("mark_as_read_ml"),
                        (a, ip) =>
                        {
                            viewControllerWeakReference.Unwrap()?.MarkAsRead(documentPreview, indexPath);
                            viewControllerWeakReference.Unwrap()?.EndEditing();
                        });
                    markAsReadAction.BackgroundColor = Theme.Brown;
                    actions.Add(markAsReadAction);
                }

                var copyToWorktrayAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Default,
                    Localization.GetString("copy_to_worktray_ml"),
                    (a, ip) =>
                    {
                        viewControllerWeakReference.Unwrap()?.CopyToWorktray(documentPreview);
                        viewControllerWeakReference.Unwrap()?.EndEditing();
                    });
                copyToWorktrayAction.BackgroundColor = Theme.DarkBlue;
                actions.Add(copyToWorktrayAction);

                var moreAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Default,
                    Localization.GetString("more"),
                    (a, ip) =>
                    {
                        viewControllerWeakReference.Unwrap()?.ShowMoreActionSheet(indexPath, documentPreview);
                    });
                moreAction.BackgroundColor = Theme.DarkerBlue;
                actions.Add(moreAction);

                return actions.ToArray();
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                if (tableView.Editing)
                    return;

                var dp = Items[indexPath.Row];
                viewControllerWeakReference.Unwrap()?.DocumentSelected(dp);
            }

            public void PrependItems(IEnumerable<DocumentPreview> documentPreviews)
            {
                loading = false;

                Items.InsertRange(0, documentPreviews);
                var indexes = Enumerable.Range(0, documentPreviews.Count()).Select(i => NSIndexPath.FromRowSection(i, 0)).ToArray();
                tableViewWeakReference.Unwrap()?.InsertRows(indexes, UITableViewRowAnimation.Fade);
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

            public void UpdateItem(int documentPreviewId)
            {
                var documentRow = Items.IndexOf(d => d.Id == documentPreviewId);
                if (documentRow < 0)
                    return;

                tableViewWeakReference.Unwrap()?.ReloadRows(new NSIndexPath[] { NSIndexPath.FromRowSection(documentRow, 0) }, UITableViewRowAnimation.Fade);
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
        }

        #endregion

        #region Workers

        class AutoRefreshWorker : NSObject
        {
            CancellationTokenSource cts;

            readonly Func<int, Task> work;
            readonly Func<DocumentPreview> firstOrDefaultItem;
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

                            var first = firstOrDefaultItem();
                            if (first != null)
                            {
                                await AsyncHelpers.InvokeOnMainThreadAsync(this, async () =>
                                {
                                    await work(first.Id);
                                });
                            }
                        }
                    });
                }
            }

            public void Stop()
            {
                lock (lockObj)
                {
                    cts?.Cancel();
                }
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