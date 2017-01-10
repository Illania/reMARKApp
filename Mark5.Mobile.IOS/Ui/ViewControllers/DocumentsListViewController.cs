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
        SearchDataSource searchResultsDataSource;

        CancellationTokenSource searchCancellationTokenSource;
        readonly List<CancellationTokenSource> searchCancellationTokenSourceList = new List<CancellationTokenSource>();

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

            await RefreshData();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeinitializeHandlers();
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning($"{nameof(DocumentsListViewController)} received memory warning!");

            var ds = documentsTableView?.DataSource as DataSource;
            ds?.Clear();

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
            documentsTableView.Source = new DataSource(documentsTableView);
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
            searchResultsDataSource = new SearchDataSource();
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
                    ds.Clear();

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
            refreshControl.ValueChanged -= RefreshControl_ValueChanged;
        }

        public void UpdateSearchResultsForSearchController(UISearchController searchController)
        {
            throw new NotImplementedException();
        }

        void ComposeDocumentItem_Clicked(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        class DataSource : UITableViewSource, IDisposable
        {

            static readonly nfloat Height = 100f;
            static readonly nfloat CompactHeight = 100f;

            public bool Empty
            {
                get
                {
                    return documentPreviewsInView.Count < 1;
                }
            }

            public bool EnableLoadMore { get; set; }

            UITableView documentsTableView;

            bool loading;
            List<DocumentPreview> documentPreviewsInView;
            
            public DataSource(UITableView documentsTableView)
            {
                this.documentsTableView = documentsTableView;

                loading = true;
                documentPreviewsInView = new List<DocumentPreview>(1000);
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
                    emptyCell.Initialize(Localization.GetString("folder_empty"));
                    return emptyCell;
                }

                var cell = tableView.DequeueReusableCell(DocumentsTableViewCell.Key) as DocumentsTableViewCell ?? DocumentsTableViewCell.Create();
                var dp = documentPreviewsInView[indexPath.Row];
                cell.Initialize(dp);

                return cell;
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
                return Height;
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

            public void Clear()
            {
                loading = false;

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

        class SearchDataSource : UITableViewSource, IDisposable
        {
            
            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                throw new NotImplementedException();
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                throw new NotImplementedException();
            }
        }
    }
}
