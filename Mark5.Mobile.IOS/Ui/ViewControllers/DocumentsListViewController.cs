//
// Project: Mark5.Mobile.IOS
// File: DocumentsListViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;
using ObjCRuntime;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    
    public class DocumentsListViewController : AbstractViewController, IPrimaryViewController, IUISearchResultsUpdating, IUIGestureRecognizerDelegate
    {

        const int AutoRefreshIntervalMs = 5 * 1000; // 5 seconds

        public Folder Folder { get; set; }

        UIBarButtonItem composeDocumentItem;
        UIBarButtonItem exitEditItem;
        UIBarButtonItem editItem;

        UIRefreshControl refreshControl;
        UITableView documentsTableView;
        UISearchController searchController;
        UITableViewController searchResultsController;
        DataSource searchResultsDataSource;

        CancellationTokenSource searchCancellationTokenSource;
        readonly List<CancellationTokenSource> searchCancellationTokenSourceList = new List<CancellationTokenSource>();

        AutoRefreshWorker autoRefreshWorker;

        bool refreshing;

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

            InitializeNavigationBarTitle();
            InitializeHandlers();
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public override async void ViewDidAppear(bool animated)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info($"{nameof(DocumentsListViewController)} appeared");

            var ds = (DataSource)documentsTableView.Source;
            if (ds.Empty)
                await RefreshData();

            if (IsBeingDismissed) return;

            CommonConfig.Logger.Info($"Starting automatic refresh...");

            autoRefreshWorker?.Stop();
            autoRefreshWorker = new AutoRefreshWorker(AutoRefreshData, () => { return ds?.Items?.FirstOrDefault(); }, AutoRefreshIntervalMs);
            autoRefreshWorker.Start();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeinitializeHandlers();

            autoRefreshWorker?.Stop();
            autoRefreshWorker = null;
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
            composeDocumentItem = new UIBarButtonItem();
            composeDocumentItem.Image = UIImage.FromBundle(Path.Combine("icons", "compose.png"));
            NavigationItem.SetRightBarButtonItem(composeDocumentItem, false);

            exitEditItem = new UIBarButtonItem(UIBarButtonSystemItem.Done);
            editItem = new UIBarButtonItem(UIBarButtonSystemItem.Edit);
        }

        void InitializeView()
        {
            AutomaticallyAdjustsScrollViewInsets = true;

            documentsTableView = new UITableView();
            documentsTableView.ClipsToBounds = false;
#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void            documentsTableView.Source = new DataSource(this, documentsTableView, async (startId) => await RefreshData(startId), Localization.GetString("folder_empty"), PlatformConfig.Preferences.CompactDocumentsList);
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void            documentsTableView.RowHeight = UITableView.AutomaticDimension;
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

            refreshControl = new UIRefreshControl();
            refreshControl.BackgroundColor = UIColor.White;
            refreshControl.AttributedTitle = Localization.GetNSAttributedString("pull_to_refresh");
            documentsTableView.AddSubview(refreshControl);
        }

        void InitializeSearchBar()
        {
            DefinesPresentationContext = true;

            searchResultsController = new UITableViewController();
            searchResultsDataSource = new DataSource(this, searchResultsController.TableView, null, Localization.GetString("no_matching_documents"), PlatformConfig.Preferences.CompactDocumentsList);
            searchResultsController.TableView.Source = searchResultsDataSource;

            searchController = new UISearchController(searchResultsController)
            {
                HidesNavigationBarDuringPresentation = true,
                DimsBackgroundDuringPresentation = true,
                ObscuresBackgroundDuringPresentation = true,
                SearchResultsUpdater = this
            };
            searchController.SearchBar.Placeholder = Localization.GetString("filter");

            documentsTableView.TableHeaderView = searchController.SearchBar;
        }

        void InitializeNavigationBarTitle()
        {
            NavigationItem.Title = Folder.Name;
        }

        void InitializeHandlers()
        {
            if (composeDocumentItem != null)
                composeDocumentItem.Clicked += ComposeDocumentItem_Clicked;

            if (exitEditItem != null)
                exitEditItem.Clicked += ExitEditItem_Clicked;

            if (editItem != null)
                editItem.Clicked += EditItem_Clicked;
            
            if (refreshControl != null)
                refreshControl.ValueChanged += RefreshControl_ValueChanged;
        }

        void DeinitializeHandlers()
        {
            if (composeDocumentItem != null)
                composeDocumentItem.Clicked -= ComposeDocumentItem_Clicked;

            if (exitEditItem != null)
                exitEditItem.Clicked -= ExitEditItem_Clicked;

            if (editItem != null)
                editItem.Clicked -= EditItem_Clicked;

            if (refreshControl != null)
                refreshControl.ValueChanged -= RefreshControl_ValueChanged;
        }

        #endregion

        #region Actions

        public void DocumentSelected(DocumentPreview documentPreview)
        {
            // TODO
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

        void ComposeDocumentItem_Clicked(object sender, EventArgs e)
        {
            // TODO
        }

        void ExitEditItem_Clicked(object sender, EventArgs e) => EndEditing();

        void EndEditing()
        {
            documentsTableView.SetEditing(false, true);
            NavigationItem.SetRightBarButtonItem(composeDocumentItem, true);
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
            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_folder"), UIAlertActionStyle.Default, a =>
            {
                var vc = new CopyMoveToFolderListViewController(selectedDocuments.Cast<IBusinessEntity>().ToList());
                NavigationController.PresentViewController(new NavigationController(vc), true, null);
            }));

            if (Folder.InternalType == FolderInternalType.FilterView
                || Folder.InternalType == FolderInternalType.Static
                || Folder.InternalType == FolderInternalType.Worktray)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("move_to_folder"), UIAlertActionStyle.Default, a =>
            {
                var vc = new CopyMoveToFolderListViewController(selectedDocuments.Cast<IBusinessEntity>().ToList(), Folder);
                NavigationController.PresentViewController(new NavigationController(vc), true, null);
            }));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("set_priority"), UIAlertActionStyle.Default, null)); // TODO

            if (Folder.InternalType == FolderInternalType.FilterView
                || Folder.InternalType == FolderInternalType.Static
                || Folder.InternalType == FolderInternalType.Worktray)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete_from_folder"), UIAlertActionStyle.Default, null)); // TODO

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator
                || ServerConfig.SystemSettings.DocumentsModuleInfo.Permissions.DeleteAllowed
                || selectedDocuments.All(dp => dp.Direction == DocumentDirection.Draft))
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete"), UIAlertActionStyle.Destructive, null)); // TODO

            eas.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, null));

            if (eas.PopoverPresentationController != null)
                eas.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate((UIBarButtonItem)sender);

            PresentViewController(eas, true, null);
        }

        void CopyToWorktray(DocumentPreview documentPreview) => CopyToWorktray(new List<DocumentPreview> { documentPreview });

        void CopyToWorktray(List<DocumentPreview> documentPreviews)
        {
            var vc = new CopyToWorktrayViewController { BusinessEntities = documentPreviews.Cast<IBusinessEntity>().ToList() };
            NavigationController.PresentViewController(new NavigationController(vc), true, null);
        }

        void MarkAsRead(DocumentPreview documentPreview, NSIndexPath row) => MarkAsRead(new List<DocumentPreview> { documentPreview }, new[] { row });

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void        async void MarkAsRead(List<DocumentPreview> documentPreviews, NSIndexPath[] rows)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void        {
            CommonConfig.Logger.Info($"Attempting to mark as read [documentPreviews={documentPreviews.Count}]...");

            try
            {
                await Managers.DocumentsManager.SetDocumentsReadStatusAsync(documentPreviews, true);
                documentsTableView.ReloadRows(rows, UITableViewRowAnimation.Automatic);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Marking as read failed [documentPreviews.Count={documentPreviews.Count}]", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        void MarkAsUnread(DocumentPreview documentPreview, NSIndexPath row) => MarkAsUnread(new List<DocumentPreview> { documentPreview }, new[] { row });

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void        async void MarkAsUnread(List<DocumentPreview> documentPreviews, NSIndexPath[] rows)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void        {
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

        #endregion

        #region Refreshing

        async void RefreshControl_ValueChanged(object sender, EventArgs e) => await RefreshData(forceClear: true);

        async Task RefreshData(int startId = -1, int endId = -1, bool forceClear = false)
        {
            if (refreshing) return;

            refreshing = true;
            refreshControl.ValueChanged -= RefreshControl_ValueChanged;

            CommonConfig.Logger.Info($"Refreshing documents list [folder={Folder?.Name}, startId={startId}, endId={endId}, forceClear={forceClear}]");

            try
            {
                var ds = (DataSource)documentsTableView.Source;

                if (forceClear)
                    ds.Reset();

                var documentPreviews = await Managers.DocumentsManager.GetDocumentPreviewsAsync(Folder, startId, endId);
                ds.LoadMoreEnabled = documentPreviews.Count >= PlatformConfig.Preferences.DocumentsToDownload;
                CommonConfig.Logger.Info($"Enable load more documents set to {ds.LoadMoreEnabled}");

                Managers.DownloadManager.Notify(ObjectType.Document, Folder.Id);
                ds.AppendItems(documentPreviews);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Could not refresh folders [folder={Folder?.Name}, startId={startId}, endId={endId}, forceClear={forceClear}]", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);

                NavigationController?.PopViewController(true);
            }

            refreshControl.EndRefreshing();
            refreshControl.ValueChanged += RefreshControl_ValueChanged;

            refreshing = false;
        }

        async Task AutoRefreshData(int endId)
        {
            if (refreshing) return;

            refreshing = true;

            refreshControl.Enabled = false;

            try
            {
                CommonConfig.Logger.Debug($"Attempting automatic refresh [endId={endId}, isBeingDismissed={IsBeingDismissed}]...");

                if (IsBeingDismissed) return;

                CommonConfig.Logger.Debug($"Automatic refresh running...");

                var documents = await Managers.DocumentsManager.GetDocumentPreviewsAsync(Folder, endId: endId);

                if (documents.Count > 0)
                {
                    CommonConfig.Logger.Info($"Received {documents?.Count} new documents");

                    Managers.DownloadManager.Notify(ObjectType.Document, Folder.Id);

                    var ds = documentsTableView.Source as DataSource;
                    ds?.PrependItems(documents);
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

            refreshControl.Enabled = true;
            refreshing = false;
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
                searchResultsDataSource.Reset();
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

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void DoSearchDocuments(string searchText, CancellationToken ct)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            searchResultsDataSource.Reset();

            await Task.Delay(500);

            if (ct.IsCancellationRequested) return;

            var ds = (DataSource)documentsTableView.Source;
            var filteredDocuments = ds.Items.Where(dp => MatchesQuery(dp, searchText)).ToList();

            if (ct.IsCancellationRequested) return;

            searchResultsDataSource.AppendItems(filteredDocuments);
        }

        static bool MatchesQuery(DocumentPreview dp, string query)
        {
            if (dp.Subject.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0)
                return true;

            if (dp.Preview.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0)
                return true;

            if (dp.Addresses.Any(da => da.Name.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0))
                return true;

            if (dp.Addresses.Any(da => da.Address.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0))
                return true;

            if (dp.Categories.Any(da => da.Name.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0))
                return true;

            return false;
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

            public bool LoadMoreEnabled { get; set; }

            DocumentsListViewController viewController;
            UITableView documentsTableView;
            readonly Action<int> loadMoreAction;
            readonly string emptyText;
            readonly bool compact;

            bool loading = true;
            List<DocumentPreview> documentPreviewsInView = new List<DocumentPreview>(1000);

            public DataSource(DocumentsListViewController viewController, UITableView documentsTableView, Action<int> loadMoreAction, string emptyText, bool compact)
            {
                this.viewController = viewController;
                this.documentsTableView = documentsTableView;
                this.loadMoreAction = loadMoreAction;
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


                if (LoadMoreEnabled && loadMoreAction != null && dp.Id == documentPreviewsInView.Last().Id)
                    loadMoreAction(dp.Id);

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

                var moreAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("more"), (a, ip) => { viewController.EndEditing(); }); // TODO
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

            public void PrependItems(List<DocumentPreview> documentPreviews)
            {
                loading = false;

                documentPreviewsInView.InsertRange(0, documentPreviews);
                var indexes = Enumerable.Range(0, documentPreviews.Count).Select(i => NSIndexPath.FromRowSection(i, 0)).ToArray();
                documentsTableView.InsertRows(indexes, UITableViewRowAnimation.Automatic);
            }

            public void AppendItems(List<DocumentPreview> documentPreviews)
            {
                loading = false;

                documentPreviewsInView.AddRange(documentPreviews);
                documentsTableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Automatic);
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
                            if (cts.IsCancellationRequested) break;

                            var first = firstOrDefaultItem();
                            if (first != null)
#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void                                InvokeOnMainThread(async () => await work(first.Id));
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
                        }
                    });
                }
            }

            public void Stop()
            {
                lock (lockObj)
                    cts?.Cancel();
            }
        }
    }
}
