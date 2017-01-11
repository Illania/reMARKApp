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
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    
    public class DocumentsListViewController : UIViewController, IPrimaryViewController, IUISearchResultsUpdating
    {

        const int AutoRefreshIntervalMs = 5 * 1000; // 5 seconds

        public Folder Folder { get; set; }

        UIBarButtonItem composeDocumentItem;

        UIRefreshControl refreshControl;
        UITableView documentsTableView;
        UISearchController searchController;
        UITableViewController searchResultsController;
        DataSource searchResultsDataSource;

        CancellationTokenSource searchCancellationTokenSource;
        readonly List<CancellationTokenSource> searchCancellationTokenSourceList = new List<CancellationTokenSource>();

        AutoRefreshWorker autoRefreshWorker;

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

            var ds = documentsTableView?.DataSource as DataSource;
            ds?.Reset();

            base.DidReceiveMemoryWarning();
        }

        void InitializeNavigationBar()
        {
            composeDocumentItem = new UIBarButtonItem();
            composeDocumentItem.Image = UIImage.FromBundle(Path.Combine("icons", "compose.png"));
            NavigationItem.SetRightBarButtonItem(composeDocumentItem, false);
        }

        void InitializeView()
        {
            AutomaticallyAdjustsScrollViewInsets = true;

            documentsTableView = new UITableView();
            documentsTableView.ClipsToBounds = false;
            documentsTableView.Source = new DataSource(documentsTableView, Localization.GetString("folder_empty"), PlatformConfig.Preferences.CompactDocumentsList);
            documentsTableView.RowHeight = UITableView.AutomaticDimension;
            documentsTableView.EstimatedRowHeight = 75f;
            documentsTableView.AllowsSelectionDuringEditing = false;
            documentsTableView.TranslatesAutoresizingMaskIntoConstraints = false;
            View.AddSubview(documentsTableView);
            View.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(documentsTableView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(documentsTableView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(documentsTableView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(documentsTableView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1.0f, 0.0f)
                });

            refreshControl = new UIRefreshControl();
            refreshControl.BackgroundColor = UIColor.White;
            refreshControl.AttributedTitle = Localization.GetNSAttributedString("pull_to_refresh");
            documentsTableView.AddSubview(refreshControl);
        }

        void InitializeSearchBar()
        {
            DefinesPresentationContext = true;

            searchResultsController = new UITableViewController();
            searchResultsDataSource = new DataSource(searchResultsController.TableView, Localization.GetString("no_matching_documents"), PlatformConfig.Preferences.CompactDocumentsList);
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

            refreshControl.ValueChanged += RefreshControl_ValueChanged;
        }

        void DeinitializeHandlers()
        {
            if (composeDocumentItem != null)
                composeDocumentItem.Clicked -= ComposeDocumentItem_Clicked;

            refreshControl.ValueChanged -= RefreshControl_ValueChanged;
        }

        async void RefreshControl_ValueChanged(object sender, EventArgs e) => await RefreshData(forceClear: true);

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async Task RefreshData(int startId = -1, int endId = -1, bool forceClear = false)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            refreshControl.ValueChanged -= RefreshControl_ValueChanged;

            CommonConfig.Logger.Info($"Refreshing documents list [folder={Folder?.Name}, startId={startId}, endId={endId}, forceClear={forceClear}]");

            try
            {
                var ds = (DataSource)documentsTableView.Source;

                if (forceClear)
                    ds.Reset();

                var documentPreviews = await Managers.DocumentsManager.GetDocumentPreviewsAsync(Folder, startId, endId);
                ds.EnableLoadMore = documentPreviews.Count >= PlatformConfig.Preferences.DocumentsToDownload;
                CommonConfig.Logger.Info($"Enable load more documents set to {ds.EnableLoadMore}");

                Managers.DownloadManager.Notify(ObjectType.Document, Folder.Id);
                ds.AppendItems(documentPreviews);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Could not refresh folders [folder={Folder?.Name}, startId={startId}, endId={endId}, forceClear={forceClear}]", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);
            }

            refreshControl.EndRefreshing();
            refreshControl.ValueChanged += RefreshControl_ValueChanged;
        }

        async Task AutoRefreshData(int endId)
        {
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
        }

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

        async void DoSearchDocuments(string searchText, CancellationToken ct)
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
            {
                return true;
            }
            if (dp.Preview.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                return true;
            }
            if (dp.Addresses.Any(da => da.Name.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0))
            {
                return true;
            }
            if (dp.Addresses.Any(da => da.Address.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0))
            {
                return true;
            }
            if (dp.Categories.Any(da => da.Name.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0))
            {
                return true;
            }

            return false;
        }

        void ComposeDocumentItem_Clicked(object sender, EventArgs e)
        {
            // TODO
        }

        class DataSource : UITableViewSource, IDisposable
        {

            static readonly nfloat Height = 100f;
            static readonly nfloat CompactHeight = 52f;
            static readonly nfloat ExternalHeight = 52f;

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

            public bool EnableLoadMore { get; set; }

            UITableView documentsTableView;
            readonly string emptyText;
            readonly bool compact;

            bool loading = true;
            List<DocumentPreview> documentPreviewsInView = new List<DocumentPreview>(1000);

            public DataSource(UITableView documentsTableView, string emptyText, bool compact)
            {
                this.emptyText = emptyText;
                this.documentsTableView = documentsTableView;
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
                    return ExternalHeight;

                return compact ? CompactHeight : Height;
            }

            public void PrependItems(List<DocumentPreview> documentPreviews)
            {
                loading = false;

                documentPreviewsInView.InsertRange(0, documentPreviews);
                var indexes = Enumerable.Range(0, documentPreviews.Count).Select(i => NSIndexPath.FromRowSection(i, 0)).ToArray();
                documentsTableView.ReloadRows(indexes, UITableViewRowAnimation.Automatic);
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

                documentPreviewsInView = null;
                documentsTableView = null;
            }
        }

        class AutoRefreshWorker
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
                            {
                                await work(first.Id);
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
    }
}
