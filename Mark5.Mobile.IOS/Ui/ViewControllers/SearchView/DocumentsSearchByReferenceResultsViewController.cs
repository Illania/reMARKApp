using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.SearchView
{
    public class DocumentsSearchByReferenceResultsViewController : AbstractTableViewController, IPrimaryViewController,
        IUIGestureRecognizerDelegate, IUIViewControllerRestoration, IUISearchResultsUpdating
    {
        public SearchDocumentsCriteria Criteria { get; set; }

        protected readonly TaskCompletionSource<int> tcs = new();
        public Task<int> Result => tcs.Task;

        UIBarButtonItem closeItem;

        DocumentsSearchByReferenceResultsFilterController searchResultsController;
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
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            RestorationIdentifier = nameof(DocumentsSearchByReferenceResultsViewController);
            RestorationClass = Class;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
                ModalInPresentation = true;

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

            if (((DocumentsSearchByReferenceResultsDataSource)TableView.Source).Empty)
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

            ((DocumentsSearchByReferenceResultsDataSource)TableView.Source)?.Reset();

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        protected override void Recycle()
        {
            base.Recycle();

            TableView.GestureRecognizers.ForEach(TableView.RemoveGestureRecognizer);
            ((DocumentsSearchByReferenceResultsDataSource)TableView.Source)?.Reset();

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
            NavigationItem.Title = Localization.GetString("search_results");

            closeItem = new UIBarButtonItem
            {
                Title = Localization.GetString("close")
            };

            NavigationItem.SetRightBarButtonItem(closeItem, false);
    
        }

        void InitializeSearchBar()
        {
            DefinesPresentationContext = true;

            searchResultsController = new DocumentsSearchByReferenceResultsFilterController()
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
            TableView.Source = new DocumentsSearchByReferenceResultsDataSource(this, TableView, PlatformConfig.Preferences.CompactDocumentsList);
            TableView.AllowsMultipleSelectionDuringEditing = true;
            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 60f;
        }

        void InitializeHandlers()
        {
            if (closeItem != null)
                closeItem.Clicked += CloseItem_Clicked;
        }

        void DeinitializeHandlers()
        {
            if (closeItem != null)
                closeItem.Clicked -= CloseItem_Clicked;

        }


        #endregion

        #region NavigationBar handlers

        private void CloseItem_Clicked(object sender, EventArgs e)
        {
            tcs.SetCanceled();
            DismissViewController(true, null);
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

                ((DocumentsSearchByReferenceResultsDataSource)TableView.Source).AppendItems(results);
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
                ((DocumentsSearchByReferenceResultsDataSource)dataSource)?.Reset();
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
           tcs.SetResult(documentPreview.Id);
           DismissViewController(false, null);
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