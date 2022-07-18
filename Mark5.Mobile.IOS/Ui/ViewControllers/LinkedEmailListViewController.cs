using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using UIKit;
using Mark5.Mobile.IOS.Utilities;
using Mark5.Mobile.Common.Manager;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class LinkedEmailListViewController : AbstractTableViewController, IUISearchResultsUpdating
    {
        readonly TaskCompletionSource<Recipient> tcs = new();
        public Task<Recipient> Result => tcs.Task;

        public Folder Folder { get; set; }
        public Contact Contact { get; set; }
        public ContactPreview ContactPreview { get; set; }

        UIBarButtonItem exitItem;

        UISearchController searchController;
        CancellationTokenSource searchCancellationTokenSource;
        readonly List<CancellationTokenSource> searchCancellationTokenSourceList = new();

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

            if (Integration.IsRunningAtLeast(11))
            {
                if (NavigationController != null)
                    NavigationController.NavigationBar.PrefersLargeTitles = true;
                NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Automatic;
            }

            InitializeHandlers();
        }

        public override async void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info("Appeared");

            if (((DataSource)TableView.Source).Empty)
                await RefreshData();


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

            if (searchController != null && searchController.Active)
                searchController.Active = false;
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning("Received memory warning!");

            ((DataSource)TableView.Source)?.Reset();

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        protected override void Recycle()
        {
            base.Recycle();

            exitItem = null;

            searchCancellationTokenSource?.Dispose();
            searchCancellationTokenSource = null;
            searchCancellationTokenSourceList.ForEach(cts => cts?.Dispose());
            searchCancellationTokenSourceList.Clear();

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

        void InitializeNavigationBar()
        {
            NavigationItem.Title = ContactPreview.Name + Localization.GetString("contacts_list");
            exitItem = new UIBarButtonItem(UIBarButtonSystemItem.Cancel);
            NavigationItem.SetLeftBarButtonItem(exitItem, true);
        }

        void InitializeView()
        {
            TableView.Source = new DataSource(this, TableView);
            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 20f;
        }

        void InitializeSearchBar()
        {
            DefinesPresentationContext = true;

            var searchResultsController = new UITableViewController();
            var searchResultsDataSource = new DataSource(this, searchResultsController.TableView);
            searchResultsController.TableView.Source = searchResultsDataSource;
            searchResultsController.TableView.EstimatedRowHeight = 20f;
            searchResultsController.TableView.RowHeight = UITableView.AutomaticDimension;

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
            if (exitItem != null)
                exitItem.Clicked += ExitItem_Clicked;
        }

        void DeinitializeHandlers()
        {
            if (exitItem != null)
                exitItem.Clicked -= ExitItem_Clicked;
        }

        async Task RefreshData()
        { 
            if(Contact == null)
                Contact = await Managers.ContactsManager.GetContactAsync(Folder, ContactPreview.Id);
            if (Contact.CommunicationAddresses.Any())
                ((DataSource)TableView.Source).SetItems(Contact.CommunicationAddresses);
        }

        void ExitItem_Clicked(object sender, EventArgs e)
        {
            tcs.TrySetResult(null);
            DismissViewController(true, null);
        }

        public void EmailAddressSelected(CommunicationAddress ca)
        {
            tcs.SetResult(new Recipient(ContactPreview.Name, ca.Address, RecipientType.Contact, ContactPreview.Id));
            var vc = NavigationController?.PopToRootViewController(false);
            DismissViewController(true, null);
        }


        void IUISearchResultsUpdating.UpdateSearchResultsForSearchController(UISearchController searchController)
        {
            var searchText = searchController.SearchBar.Text;

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

                DoSearchAddress(searchText, searchCancellationTokenSource.Token);
            }
        }

        async void DoSearchAddress(string searchText, CancellationToken ct)
        {
            var tableViewController = searchController?.SearchResultsController as UITableViewController;
            var dataSource = tableViewController?.TableView?.Source as DataSource;
            dataSource?.Reset();

            await Task.Delay(500);

            if (ct.IsCancellationRequested)
                return;

            var ds = (DataSource)TableView.Source;
            var filteredAddresses = ds.Items.Where(cp => MatchesQuery(cp, searchText)).ToList();

            if (ct.IsCancellationRequested)
                return;

            dataSource?.SetItems(filteredAddresses);
        }

        static bool MatchesQuery(CommunicationAddress ca, string query)
        {
            if (ca.Address?.ContainsCaseInsensitive(query) ?? false)
                return true;

            return false;
        }



        class DataSource : UITableViewSource
        {
            public bool Empty => !Items.Any();

            readonly WeakReference<LinkedEmailListViewController> viewControllerWeakReference;
            readonly WeakReference<UITableView> tableViewWeakReference;

            bool loading = true;
            public List<CommunicationAddress> Items = new();

            public DataSource(LinkedEmailListViewController viewController, UITableView tableView)
            {
                viewControllerWeakReference = viewController.Wrap();
                tableViewWeakReference = tableView.Wrap();
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                    return tableView.DequeueReusableCell(WaitTableViewCell.DefaultId) as WaitTableViewCell ?? new WaitTableViewCell();

                if (Empty)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.DefaultId) as EmptyTableViewCell ?? new EmptyTableViewCell();
                    emptyCell.Initialize(Localization.GetString("recent_addresses_empty"));
                    return emptyCell;
                }

                var ca = Items[indexPath.Row];
                var cell = tableView.DequeueReusableCell("cell") ?? UITableViewCellUtilities.CreateDefault("cell");
                cell.TextLabel.Text = ca.Address;
                return cell;

            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading || Empty)
                    return 1;

                return Items.Count;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var ca = Items[indexPath.Row];
                viewControllerWeakReference.Unwrap()?.EmailAddressSelected(ca);
            }

            public void SetItems(List<CommunicationAddress> ca)
            {
                loading = false;

                Items.AddRange(ca);

                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }


            public void Reset()
            {
                loading = true;

                var count = Items.Count;

                Items.Clear();

                tableViewWeakReference.Unwrap()?.BeginUpdates();
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.EndUpdates();
            }
        }


         
    }
}
