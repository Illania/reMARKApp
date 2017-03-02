//
// Project: Mark5.Mobile.IOS
// File: DocumentsSearchResultsViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;
using Mark5.Mobile.IOS.Utilities;
using ObjCRuntime;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{

    public class DocumentsSearchResultsViewController : AbstractViewController, IPrimaryViewController, IUIGestureRecognizerDelegate
    {

        public SearchDocumentsCriteria Criteria { get; set; }

        UIBarButtonItem exitEditItem;
        UIBarButtonItem editItem;

        UITableView documentsTableView;

        #region UIViewController overrides

        public override void LoadView()
        {
            base.LoadView();

            InitializeNavigationBar();
            InitializeView();
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

            CommonConfig.Logger.Info($"{nameof(DocumentsListViewController)} appeared");

            var ds = (DataSource)documentsTableView.Source;
            if (ds.Empty)
                RefreshData();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeinitializeHandlers();
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning($"{nameof(DocumentsListViewController)} received memory warning!");

            var ds = documentsTableView?.Source as DataSource;
            ds?.Reset();

            base.DidReceiveMemoryWarning();
        }

        #endregion

        #region Initialization

        void InitializeNavigationBar()
        {
            exitEditItem = new UIBarButtonItem(UIBarButtonSystemItem.Done);
            editItem = new UIBarButtonItem(UIBarButtonSystemItem.Edit);
        }

        void InitializeView()
        {
            AutomaticallyAdjustsScrollViewInsets = true;

            documentsTableView = new UITableView();
            documentsTableView.ClipsToBounds = false;
#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
            documentsTableView.Source = new DataSource(this, documentsTableView, Localization.GetString("no_documents_found"), PlatformConfig.Preferences.CompactDocumentsList);
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
            documentsTableView.RowHeight = UITableView.AutomaticDimension;
            documentsTableView.EstimatedRowHeight = DocumentsTableViewCell.Height;
            documentsTableView.AllowsSelectionDuringEditing = false;
            documentsTableView.AllowsMultipleSelectionDuringEditing = true;
            documentsTableView.TranslatesAutoresizingMaskIntoConstraints = false;
            View.AddSubview(documentsTableView);
            View.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(documentsTableView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1f, 0f),
                    NSLayoutConstraint.Create(documentsTableView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1f, 0f),
                    NSLayoutConstraint.Create(documentsTableView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1f, 0f),
                    NSLayoutConstraint.Create(documentsTableView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1f, 0f)
                });

            var longPressRecognizer = new UILongPressGestureRecognizer(this, new Selector("longPressed:"))
            {
                MinimumPressDuration = 1f,
                Delegate = this
            };
            documentsTableView.AddGestureRecognizer(longPressRecognizer);
        }

        void InitializeNavigationBarTitle()
        {
            NavigationItem.Title = Localization.GetString("search_results");
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

        #region Actions

        public void DocumentSelected(DocumentPreview documentPreview)
        {
            if (documentsTableView.Editing)
            {
                return;
            }

            if (SplitViewController != null && !SplitViewController.Collapsed)
            {
                var ds = (DataSource)documentsTableView.Source;

                var nc = ((UINavigationController)SplitViewController.ViewControllers[1]);
                nc.PopToViewController(nc.ViewControllers[0], false);

                var vc = (DocumentViewController)nc.ViewControllers[0];

                if (vc.isShowingDocumentWithId(documentPreview.Id))
                    return;

                vc.HidesBottomBarWhenPushed = false;

                vc.ClearData();
                vc.SetData(documentPreview, ds.GetNextDocumentPreview, ds.GetPreviousDocumentPreview);
                vc.RefreshData();
            }
            else
            {
                var ds = (DataSource)documentsTableView.Source;

                var vc = new DocumentViewController();
                vc.SetData(documentPreview, ds.GetNextDocumentPreview, ds.GetPreviousDocumentPreview);
                vc.SetRefreshDataOnAppear();

                NavigationController.PushViewController(vc, true);
            }
        }

        [Export("longPressed:")]
        public void LongPressed(UILongPressGestureRecognizer recognizer)
        {
            if (documentsTableView.Editing) return;

            StartEditing();

            var point = recognizer.LocationInView(documentsTableView);
            var indexPath = documentsTableView.IndexPathForRowAtPoint(point);

            documentsTableView.SelectRow(indexPath, true, UITableViewScrollPosition.None);
        }

        void StartEditing()
        {
            documentsTableView.SetEditing(true, true);
            NavigationItem.SetRightBarButtonItem(exitEditItem, true);
            NavigationItem.SetLeftBarButtonItem(editItem, true);
        }

        void ExitEditItem_Clicked(object sender, EventArgs e) => EndEditing();

        void EndEditing()
        {
            documentsTableView.SetEditing(false, true);
            NavigationItem.SetLeftBarButtonItem(NavigationItem.BackBarButtonItem, true);
        }

        void EditItem_Clicked(object sender, EventArgs e)
        {
            if (documentsTableView.IndexPathsForSelectedRows == null || documentsTableView.IndexPathsForSelectedRows.Length < 1) return;

            var eas = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);

            var rows = documentsTableView.IndexPathsForSelectedRows.ToArray();
            var selectedDocuments = rows.Select(ip => ((DataSource)documentsTableView.Source).Items[ip.Row]).ToList();

            if (selectedDocuments.Any(dp => !dp.IsReadByCurrent))
                eas.AddAction(UIAlertAction.Create(Localization.GetString("mark_as_read"), UIAlertActionStyle.Default, a => { MarkAsRead(selectedDocuments, rows); EndEditing(); }));

            if (selectedDocuments.Any(dp => dp.IsReadByCurrent))
                eas.AddAction(UIAlertAction.Create(Localization.GetString("mark_as_unread"), UIAlertActionStyle.Default, a => { MarkAsUnread(selectedDocuments, rows); EndEditing(); }));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_worktray"), UIAlertActionStyle.Default, a => { CopyToWorktray(selectedDocuments); EndEditing(); }));
            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_folder"), UIAlertActionStyle.Default, a => { CopyToFolder(selectedDocuments); EndEditing(); }));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("set_priority"), UIAlertActionStyle.Default, a => ShowPriorityActionSheet(selectedDocuments, (UIBarButtonItem)sender)));

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator
                || ServerConfig.SystemSettings.DocumentsModuleInfo.Permissions.DeleteAllowed
                || selectedDocuments.All(dp => dp.Direction == DocumentDirection.Draft))
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete"), UIAlertActionStyle.Destructive, a => Delete(selectedDocuments)));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, a => exitEditItem.Enabled = true));

            if (eas.PopoverPresentationController != null)
                eas.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate((UIBarButtonItem)sender);

            PresentViewController(eas, true, null);
        }

        void CopyToWorktray(DocumentPreview selectedDocument) => CopyToWorktray(new List<DocumentPreview> { selectedDocument });

        void CopyToWorktray(List<DocumentPreview> selectedDocuments)
        {
            var vc = new CopyToWorktrayViewController { BusinessEntities = selectedDocuments.Cast<IBusinessEntity>().ToList() };
            NavigationController.PresentViewController(new NavigationController(vc), true, null);
        }

        void CopyToFolder(DocumentPreview selectedDocument) => CopyToFolder(new List<DocumentPreview> { selectedDocument });

        void CopyToFolder(List<DocumentPreview> selectedDocument)
        {
            var vc = new CopyMoveToFolderListViewController(selectedDocument.Cast<IBusinessEntity>().ToList());
            NavigationController.PresentViewController(new NavigationController(vc), true, null);
        }

        void MarkAsRead(DocumentPreview selectedDocument, NSIndexPath row) => MarkAsRead(new List<DocumentPreview> { selectedDocument }, new[] { row });

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void MarkAsRead(List<DocumentPreview> selectedDocuments, NSIndexPath[] rows)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            CommonConfig.Logger.Info($"Attempting to mark as read [documentPreviews={selectedDocuments.Count}]...");

            try
            {
                await Managers.DocumentsManager.SetDocumentsReadStatusAsync(selectedDocuments, true);
                documentsTableView.ReloadRows(rows, UITableViewRowAnimation.Automatic);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Marking as read failed [documentPreviews.Count={selectedDocuments.Count}]", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        void MarkAsUnread(DocumentPreview documentPreview, NSIndexPath row) => MarkAsUnread(new List<DocumentPreview> { documentPreview }, new[] { row });

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void MarkAsUnread(List<DocumentPreview> documentPreviews, NSIndexPath[] rows)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            CommonConfig.Logger.Info($"Attempting to mark as unread [documentPreviews={documentPreviews.Count}]...");

            try
            {
                await Managers.DocumentsManager.SetDocumentsReadStatusAsync(documentPreviews, false);
                documentsTableView.ReloadRows(rows, UITableViewRowAnimation.Automatic);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Marking as unread failed [documentPreviews.Count={documentPreviews.Count}]", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void ShowPriorityActionSheet(List<DocumentPreview> selectedDocuments, UIBarButtonItem barButtonItem)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            var priorities = new List<Priority> { Priority.Low, Priority.Normal, Priority.Urgent };
            var priorityStrings = priorities.Select(p => UI.PriorityString(p));
            var result = await Dialogs.ShowListDialogAsync(this, Localization.GetString("select_priority"), priorityStrings.ToArray(), barButtonItem);

            if (result < 0)
                return;

            var priority = priorities[result];

            await SetPriority(selectedDocuments, priority);
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void ShowPriorityActionSheet(DocumentPreview selectedDocument, UITableView tv, UITableViewCell cell)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            var priorities = new List<Priority> { Priority.Low, Priority.Normal, Priority.Urgent };
            var priorityStrings = priorities.Select(p => UI.PriorityString(p));
            var result = await Dialogs.ShowListDialogAsync(this, Localization.GetString("select_priority"), priorityStrings.ToArray(), tv, cell);

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

                CommonConfig.Logger.Error($"Error while setting priority for documents]", ex);
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }


        void Delete(DocumentPreview selectedDocument) => Delete(new List<DocumentPreview> { selectedDocument });

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void Delete(List<DocumentPreview> selectedDocuments)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
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
                CommonConfig.Logger.Info($"Attempting to delete documents]");

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

        void RemoveDocumentsFromList(IEnumerable<int> ids)
        {
            var ds = (DataSource)documentsTableView.Source;
            ds.RemoveItems(ids.ToList());

            if (SplitViewController != null && !SplitViewController.Collapsed)
            {
                var nc = (UINavigationController)SplitViewController.ViewControllers[1];
                var vc = (DocumentViewController)nc.ViewControllers[0];
                if (ids.Select(id => vc.isShowingDocumentWithId(id)).Any(v => v))
                {
                    vc.ClearData();
                }
            }
        }

        void UpdatePriorityForDocument(IEnumerable<int> ids)
        {
            if (SplitViewController != null && !SplitViewController.Collapsed)
            {
                var nc = (UINavigationController)SplitViewController.ViewControllers[1];
                var vc = (DocumentViewController)nc.ViewControllers[0];
                if (ids.Select(id => vc.isShowingDocumentWithId(id)).Any(v => v))
                {
                    vc.UpdatePriority();
                }
            }
        }

        void DoShowMoreActionSheet(NSIndexPath indexPath, DocumentPreview selectedDocument)
        {
            var eas = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_worktray"), UIAlertActionStyle.Default, a => { CopyToWorktray(selectedDocument); EndEditing(); }));
            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_folder"), UIAlertActionStyle.Default, a => { CopyToFolder(selectedDocument); EndEditing(); }));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("set_priority"), UIAlertActionStyle.Default, a => ShowPriorityActionSheet(selectedDocument, documentsTableView, documentsTableView.CellAt(indexPath))));

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator
                || ServerConfig.SystemSettings.DocumentsModuleInfo.Permissions.DeleteAllowed
                || selectedDocument.Direction == DocumentDirection.Draft)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete"), UIAlertActionStyle.Destructive, a => Delete(selectedDocument)));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, null));

            if (eas.PopoverPresentationController != null)
                eas.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate(documentsTableView, documentsTableView.CellAt(indexPath));

            PresentViewController(eas, true, null);
        }

        #endregion

        #region Refreshing

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void RefreshData()
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            try
            {
                CommonConfig.Logger.Info($"Refreshing documents list... [criteria={Criteria}]");

                var results = await Managers.SearchManager.SearchDocumentsAsync(Criteria);

                var ds = (DataSource)documentsTableView.Source;
                ds.AppendItems(results);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Could not refresh documents list [criteria={Criteria}]", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);

                NavigationController?.PopViewController(true);
            }
        }

        #endregion

        class DataSource : UITableViewSource, IDisposable
        {

            public bool Empty
            {
                get
                {
                    return documentPreviewsInView.Count < 1;
                }
            }

            public List<DocumentPreview> Items
            {
                get
                {
                    return documentPreviewsInView;
                }
            }

            DocumentsSearchResultsViewController viewController;
            UITableView documentsTableView;
            readonly string emptyText;
            readonly bool compact;

            bool loading = true;
            List<DocumentPreview> documentPreviewsInView = new List<DocumentPreview>(1000);

            public DataSource(DocumentsSearchResultsViewController viewController, UITableView documentsTableView, string emptyText, bool compact)
            {
                this.viewController = viewController;
                this.documentsTableView = documentsTableView;
                this.emptyText = emptyText;
                this.compact = compact;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                {
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as WaitTableViewCell ?? WaitTableViewCell.Create();
                }

                if (documentPreviewsInView.Count < 1)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                    emptyCell.Initialize(emptyText);
                    return emptyCell;
                }

                var dp = documentPreviewsInView[indexPath.Row];

                if (dp.Direction == DocumentDirection.External)
                {
                    var cell = tableView.DequeueReusableCell(ExternalDocumentsTableViewCell.Key) as ExternalDocumentsTableViewCell ?? ExternalDocumentsTableViewCell.Create();
                    cell.Initialize(dp);
                    return cell;
                }

                if (compact)
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

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading)
                    return 1;

                if (documentPreviewsInView.Count < 1)
                    return 1;

                return documentPreviewsInView.Count;
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                if (documentPreviewsInView.Count > 0 && documentPreviewsInView[indexPath.Row]?.Direction == DocumentDirection.External)
                    return ExternalDocumentsTableViewCell.Height;

                return compact ? DocumentsCompactTableViewCell.Height : DocumentsTableViewCell.Height;
            }

            public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
            {
                return true;
            }

            public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
            {
                var actions = new List<UITableViewRowAction>();

                var documentPreview = documentPreviewsInView[indexPath.Row];

                var moreAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("more"), (a, ip) => { viewController.DoShowMoreActionSheet(indexPath, documentPreview); });
                moreAction.BackgroundColor = Theme.Blue;
                actions.Add(moreAction);

                var copyToWorktrayAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("copy_to_worktray"), (a, ip) => { viewController.CopyToWorktray(documentPreview); viewController.EndEditing(); });
                copyToWorktrayAction.BackgroundColor = Theme.DarkBlue;
                actions.Add(copyToWorktrayAction);

                if (documentPreview.IsReadByCurrent)
                {
                    var markAsUnreadAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("mark_as_unread"), (a, ip) => { viewController.MarkAsUnread(documentPreview, indexPath); viewController.EndEditing(); });
                    markAsUnreadAction.BackgroundColor = Theme.Brown;
                    actions.Add(markAsUnreadAction);
                }
                else
                {
                    var markAsReadAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("mark_as_read"), (a, ip) => { viewController.MarkAsRead(documentPreview, indexPath); viewController.EndEditing(); });
                    markAsReadAction.BackgroundColor = Theme.Brown;
                    actions.Add(markAsReadAction);
                }

                return actions.ToArray();
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var dp = documentPreviewsInView[indexPath.Row];
                viewController.DocumentSelected(dp);
            }

            public void AppendItems(List<DocumentPreview> documentPreviews)
            {
                loading = false;

                documentPreviewsInView.AddRange(documentPreviews);
                documentsTableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Automatic);
            }

            public void RemoveItems(List<int> documentIds)
            {
                var indices = documentPreviewsInView.Select((d, i) => new { d, i }).Where(x => documentIds.Contains(x.d.Id)).Select(x => x.i).ToList();
                indices.OrderByDescending(i => i).ForEach(documentPreviewsInView.RemoveAt);

                if (!documentPreviewsInView.Any())
                {
                    documentsTableView.ReloadData();
                }
                else
                {
                    //TODO same problem as in the non search version, need to figure it out
                    documentsTableView.ReloadData();
                }
            }

            public DocumentPreview GetNextDocumentPreview(DocumentPreview documentPreview,
                                              out bool previousDocumentAvailable,
                                              out bool nextDocumentAvailable,
                                              bool scrollToDocument = false)
            {
                var currentDocumentRow = documentPreviewsInView.IndexOf(d => d.Id == documentPreview.Id);
                if (currentDocumentRow < 0)
                {
                    previousDocumentAvailable = false;
                    nextDocumentAvailable = false;
                    return null;
                }

                var nextDocumentRow = currentDocumentRow + 1;
                previousDocumentAvailable = true;
                nextDocumentAvailable = nextDocumentRow < documentPreviewsInView.Count - 1;

                return documentPreviewsInView.ElementAtOrDefault(nextDocumentRow);
            }

            public DocumentPreview GetPreviousDocumentPreview(DocumentPreview documentPreview,
                                                              out bool previousDocumentAvailable,
                                                              out bool nextDocumentAvailable,
                                                              bool scrollToDocument = false)
            {
                var currentDocumentRow = documentPreviewsInView.IndexOf(d => d.Id == documentPreview.Id);
                if (currentDocumentRow < 0)
                {
                    previousDocumentAvailable = false;
                    nextDocumentAvailable = false;
                    return null;
                }

                var previousDocumentRow = currentDocumentRow - 1;
                previousDocumentAvailable = previousDocumentRow > 0;
                nextDocumentAvailable = previousDocumentRow < documentPreviewsInView.Count - 1;

                return documentPreviewsInView.ElementAtOrDefault(previousDocumentRow);
            }

            public void Reset()
            {
                loading = true;

                documentPreviewsInView.Clear();
                documentsTableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Automatic);
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                viewController = null;
                documentsTableView = null;
                documentPreviewsInView = null;
            }
        }
    }
}
