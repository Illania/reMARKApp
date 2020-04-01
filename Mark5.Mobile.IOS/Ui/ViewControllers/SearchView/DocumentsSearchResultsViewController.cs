using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.HubMessages;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;
using Mark5.Mobile.IOS.Utilities;
using TinyMessenger;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.SearchView
{
    public class DocumentsSearchResultsViewController : AbstractTableViewController, IPrimaryViewController, IUIGestureRecognizerDelegate, IUIViewControllerRestoration
    {
        public SearchDocumentsCriteria Criteria { get; set; }

        UIBarButtonItem exitEditItem;
        UIBarButtonItem editItem;

        TinyMessageSubscriptionToken readStatusChangedToken;
        TinyMessageSubscriptionToken commentsCountChangedToken;
        TinyMessageSubscriptionToken categoriesChangedToken;
        TinyMessageSubscriptionToken goToDocumentToken;

        #region UIViewController overrides

        public override void LoadView()
        {
            base.LoadView();

            InitializeNavigationBar();
            InitializeView();
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
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info("Appeared");

            if (((DataSource)TableView.Source).Empty)
                RefreshData();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeinitializeHandlers();
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

            TableView.GestureRecognizers.ForEach(TableView.RemoveGestureRecognizer);
            ((DataSource)TableView.Source)?.Reset();
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
            goToDocumentToken = CommonConfig.MessengerHub.Subscribe<GoToDocumentMessage>(HandleAction);
        }

        void UnsubscribeFromMessages()
        {
            readStatusChangedToken?.Dispose();
            commentsCountChangedToken?.Dispose();
            categoriesChangedToken?.Dispose();
            goToDocumentToken?.Dispose();
        }

        #endregion

        #region Initialize/deinitialize

        void InitializeNavigationBar()
        {
            NavigationItem.Title = Localization.GetString("search_results");

            exitEditItem = new UIBarButtonItem(UIBarButtonSystemItem.Done);
            editItem = new UIBarButtonItem(UIBarButtonSystemItem.Edit);
        }

        void InitializeView()
        {
            TableView.Source = new DataSource(this, TableView, PlatformConfig.Preferences.CompactDocumentsList);
            TableView.AllowsMultipleSelectionDuringEditing = true;
            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 60f;

            TableView.AddGestureRecognizer(new UILongPressGestureRecognizer(DocumentPreviewLongPressed));
        }

        void InitializeHandlers()
        {
            if (exitEditItem != null)
                exitEditItem.Clicked += ExitEditItem_Clicked;

            if (editItem != null)
                editItem.Clicked += EditItem_Clicked;
        }

        void DeinitializeHandlers()
        {
            if (exitEditItem != null)
                exitEditItem.Clicked -= ExitEditItem_Clicked;

            if (editItem != null)
                editItem.Clicked -= EditItem_Clicked;
        }

        #endregion

        #region NavigationBar handlers

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

            if (ServerConfig.SystemSettings.DocumentsModuleInfo.WorktrayEnabled ?? true)
            {
                eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_worktray"),
                                                   UIAlertActionStyle.Default,
                                                   a =>
                {
                    CopyToWorktray(selectedDocuments);
                    EndEditing();
                }));
            }

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_folder"),
                UIAlertActionStyle.Default,
                a =>
                {
                    var vc = new CopyMoveToFolderListViewController(ModuleType.Documents, selectedDocuments.Cast<IBusinessEntity>().ToList());
                    PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
                }));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("set_priority"), UIAlertActionStyle.Default, a => ShowPriorityActionSheet(selectedDocuments, (UIBarButtonItem)sender)));

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

                ((DataSource)TableView.Source).AppendItems(results);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Could not refresh documents list [criteria={Criteria}]", ex);

                await Dialogs.ShowErrorAlertAsync(this, ex);

                NavigationController?.PopViewController(true);
            }
        }

        #endregion

        #region List handlers

        public void DocumentSelected(DocumentPreview documentPreview)
        {
            if (Integration.IsIPad())
            {
                var nc = (UINavigationController)SplitViewController.ViewControllers[1];
                nc.PopToViewController(nc.ViewControllers[0], false);

                var vc = (DocumentPageViewController)nc.ViewControllers[0];
                vc.DocumentPreviews = ((DataSource)TableView.Source).Items;

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
            if (TableView.Editing || ((DataSource)TableView.Source).Empty)
                return;

            StartEditing();

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

        void ShowMoreActionSheet(NSIndexPath indexPath, DocumentPreview selectedDocument)
        {
            var eas = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            var d = new PopoverPresentationControllerDelegate(TableView, TableView.CellAt(indexPath));

            if (ServerConfig.SystemSettings.DocumentsModuleInfo.WorktrayEnabled ?? true)
            {
                eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_worktray"),
                                                   UIAlertActionStyle.Default,
                                                   a =>
                {
                    CopyToWorktray(selectedDocument);
                    EndEditing();
                }));
            }

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_folder"),
                UIAlertActionStyle.Default,
                a =>
                {
                    CopyToFolder(selectedDocument);
                    EndEditing();
                }));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("set_priority"), UIAlertActionStyle.Default, a => ShowPriorityActionSheet(selectedDocument, TableView, TableView.CellAt(indexPath))));

            if (ServerConfig.SystemSettings.DocumentsModuleInfo.Permissions.DeleteAllowed || selectedDocument.Direction == DocumentDirection.Draft)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete"), UIAlertActionStyle.Destructive, a => Delete(selectedDocument, d)));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, null));

            if (eas.PopoverPresentationController != null)
                eas.PopoverPresentationController.Delegate = d;

            PresentViewController(eas, true, null);
        }

        void CopyToWorktray(DocumentPreview selectedDocument) =>
            CopyToWorktray(new List<DocumentPreview> { selectedDocument });

        void CopyToWorktray(List<DocumentPreview> selectedDocuments)
        {
            var vc = new CopyToWorktrayViewController
            {
                BusinessEntities = selectedDocuments.Cast<IBusinessEntity>().ToList()
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

        void MarkAsRead(DocumentPreview selectedDocument, NSIndexPath row) =>
            MarkAsRead(new List<DocumentPreview> { selectedDocument }, new[] { row });

        async void MarkAsRead(List<DocumentPreview> selectedDocuments, NSIndexPath[] rows)
        {
            CommonConfig.Logger.Info($"Attempting to mark as read [documentPreviews={selectedDocuments.Count}]...");

            try
            {
                CommonConfig.UsageAnalytics.LogEvent(new SetReadStatusEvent(selectedDocuments.Count));

                await Managers.DocumentsManager.SetDocumentsReadStatusAsync(selectedDocuments, true);
                TableView.ReloadRows(rows, UITableViewRowAnimation.Fade);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Marking as read failed [documentPreviews.Count={selectedDocuments.Count}]", ex);

                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        void MarkAsUnread(DocumentPreview documentPreview, NSIndexPath row) =>
            MarkAsUnread(new List<DocumentPreview> { documentPreview }, new[] { row });

        async void MarkAsUnread(List<DocumentPreview> documentPreviews, NSIndexPath[] rows)
        {
            CommonConfig.Logger.Info($"Attempting to mark as unread [documentPreviews={documentPreviews.Count}]...");

            try
            {
                CommonConfig.UsageAnalytics.LogEvent(new SetReadStatusEvent(documentPreviews.Count));

                await Managers.DocumentsManager.SetDocumentsReadStatusAsync(documentPreviews, false);
                TableView.ReloadRows(rows, UITableViewRowAnimation.Fade);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Marking as unread failed [documentPreviews.Count={documentPreviews.Count}]", ex);

                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        async void ShowPriorityActionSheet(List<DocumentPreview> selectedDocuments, UIBarButtonItem barButtonItem)
        {
            var priorities = new List<Priority> { Priority.Low, Priority.Normal, Priority.Urgent };
            var priorityStrings = priorities.Select(p => UI.PrettyPriorityString(p));
            var result = await Dialogs.ShowListActionSheetAsync(this, priorityStrings.ToArray(), barButtonItem);

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

        async Task SetPriority(List<DocumentPreview> selectedDocuments, Priority priority)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("setting_priority___"));

            try
            {
                CommonConfig.Logger.Info($"Attempting to setting priority for documents");
                await Managers.DocumentsManager.SetDocumentsPriorityAsync(selectedDocuments, priority);

                EndEditing();

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

        #endregion

        #region Message handlers

        void RemoveDocumentsFromList(IEnumerable<int> ids)
        {
            BeginInvokeOnMainThread(() =>
            {
                ((DataSource)TableView.Source).RemoveItems(ids.ToList());
            });
        }

        void ReadStatusChangedHandler(DocumentPreviewReadStatusChangedMessage message)
        {
            BeginInvokeOnMainThread(() =>
            {
                var ds = TableView.Source as DataSource;
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
                var ds = TableView.Source as DataSource;
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
                var ds = TableView.Source as DataSource;
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

        void HandleAction(GoToDocumentMessage message)
        {
            BeginInvokeOnMainThread(() =>
            {
                var index = ((DataSource)TableView.Source).Items.FindIndex(dp => dp.Id == message.DocumentId);

                if (index >= 0)
                {
                    TableView.SelectRow(NSIndexPath.FromRowSection(index, 0), true, UITableViewScrollPosition.None);
                    TableView.ScrollToRow(NSIndexPath.FromRowSection(index, 0), UITableViewScrollPosition.None, true);
                }
            });
        }

        #endregion

        #region Utilities

        void StartEditing()
        {
            TableView.SetEditing(true, true);
            NavigationItem.SetRightBarButtonItem(exitEditItem, true);
            NavigationItem.SetLeftBarButtonItem(editItem, true);
        }

        void EndEditing()
        {
            TableView.SetEditing(false, true);
            NavigationItem.SetLeftBarButtonItem(NavigationItem.BackBarButtonItem, true);
        }

        #endregion

        #region DataSource

        class DataSource : UITableViewSource
        {
            public bool Empty => Items.Count < 1;
            public List<DocumentPreview> Items { get; } = new List<DocumentPreview>(1000);

            readonly WeakReference<DocumentsSearchResultsViewController> viewControllerWeakReference;
            readonly WeakReference<UITableView> tableViewWeakReference;
            readonly bool compactList;

            bool loading = true;

            public DataSource(DocumentsSearchResultsViewController viewController, UITableView tableView, bool compactList)
            {
                viewControllerWeakReference = viewController.Wrap();
                tableViewWeakReference = tableView.Wrap();
                this.compactList = compactList;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                    return tableView.DequeueReusableCell(WaitTableViewCell.DefaultId) as WaitTableViewCell ?? new WaitTableViewCell();

                if (Empty)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.DefaultId) as EmptyTableViewCell ?? new EmptyTableViewCell();
                    emptyCell.Initialize(Localization.GetString("no_documents_found"));
                    return emptyCell;
                }

                var dp = Items[indexPath.Row];

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

            public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading || Empty)
                    return false;

                return true;
            }

            public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
            {
                var actions = new List<UITableViewRowAction>();

                if (indexPath.Row < 0 || indexPath.Row >= Items.Count)
                    return actions.ToArray();

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

                if (ServerConfig.SystemSettings.DocumentsModuleInfo.WorktrayEnabled ?? true)
                {
                    var copyToWorktrayAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Default,
                                                                           Localization.GetString("copy_to_worktray_ml"),
                                                                           (a, ip) =>
                    {
                        viewControllerWeakReference.Unwrap()?.CopyToWorktray(documentPreview);
                        viewControllerWeakReference.Unwrap()?.EndEditing();
                    });
                    copyToWorktrayAction.BackgroundColor = Theme.DarkBlue;
                    actions.Add(copyToWorktrayAction);
                }

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

            public void AppendItems(List<DocumentPreview> documentPreviews)
            {
                loading = false;

                Items.AddRange(documentPreviews);
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            public void RemoveItems(List<int> documentIds)
            {
                var indices = Items.Select((d, i) => new { d, i })
                                   .Where(x => documentIds.Contains(x.d.Id))
                                   .Select(x => x.i)
                                   .ToList();
                indices.OrderByDescending(i => i).ForEach(Items.RemoveAt);

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

            public void Reset()
            {
                loading = true;

                Items.Clear();
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }
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