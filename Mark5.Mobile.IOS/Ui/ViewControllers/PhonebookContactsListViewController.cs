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

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class PhonebookContactsListViewController : AbstractTableViewController, IUISearchResultsUpdating
    {
        readonly TaskCompletionSource<Recipient> tcs = new TaskCompletionSource<Recipient>();
        public Task<Recipient> Result => tcs.Task;

        UIBarButtonItem exitItem;

        UISearchController searchController;
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

            if (NavigationController != null)
                NavigationController.NavigationBar.PrefersLargeTitles = true;
            NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Automatic;

            InitializeHandlers();
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info("Appeared");

            if (((DataSource)TableView.Source).Empty)
                RefreshData();

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
            NavigationItem.Title = Localization.GetString("phonebook_contacts_title");

            exitItem = new UIBarButtonItem(UIBarButtonSystemItem.Cancel);
            NavigationItem.SetLeftBarButtonItem(exitItem, true);
        }

        void InitializeView()
        {
            TableView.Source = new DataSource(this, TableView, Localization.GetString("phonebook_contacts_empty"));
            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 20f;
        }

        void InitializeSearchBar()
        {
            DefinesPresentationContext = true;

            var searchResultsController = new UITableViewController();
            var searchResultsDataSource = new DataSource(this, searchResultsController.TableView, Localization.GetString("no_phonebook_matching"));
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

        void RefreshData()
        {
            Task.Run(() =>
            {
                return CommonConfig.Phonebook.GetPhonebookContacts();
            }).ContinueWith(async t =>
            {
                if (t.IsFaulted)
                {
                    var ex = t.Exception.InnerException;
                    CommonConfig.Logger.Error($"Error while retrieving phonebook contacts", ex);
                    await Dialogs.ShowErrorAlertAsync(this, ex);
                    tcs.SetResult(null);
                    return;
                }

                if (t.Result == null)
                {
                    await Dialogs.ShowConfirmAlertAsync(this, Localization.GetString("phonebook_contacts_no_access_title"),
                                                         Localization.GetString("phonebook_contacts_no_access_content"));
                    tcs.SetResult(null);
                }
                else
                    ((DataSource)TableView.Source).SetItems(t.Result.OrderBy(c => c.Name).ToList());
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        void ExitItem_Clicked(object sender, EventArgs e)
        {
            tcs.TrySetResult(null);
            DismissViewController(true, null);
        }

        public void PhonebookAddressSelected(Recipient pb, UITableViewCell cell)
        {
            tcs.TrySetResult(pb);
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

                DoSearchContacts(searchText, searchCancellationTokenSource.Token);
            }
        }

        async void DoSearchContacts(string searchText, CancellationToken ct)
        {
            var tableViewController = searchController?.SearchResultsController as UITableViewController;
            var dataSource = tableViewController?.TableView?.Source as DataSource;
            dataSource?.Reset();

            await Task.Delay(500);

            if (ct.IsCancellationRequested)
                return;

            var ds = (DataSource)TableView.Source;
            var filteredContacts = ds.Items.Where(cp => MatchesQuery(cp, searchText)).ToList();

            if (ct.IsCancellationRequested)
                return;

            dataSource?.SetItems(filteredContacts);
        }

        static bool MatchesQuery(Recipient cp, string query)
        {
            if (cp.Name?.ContainsCaseInsensitive(query) ?? false)
                return true;

            if (cp.Address?.ContainsCaseInsensitive(query) ?? false)
                return true;

            return false;
        }

        class DataSource : UITableViewSource
        {
            public bool Empty => !items.SelectMany(v => v).Any();
            public List<Recipient> Items => items.SelectMany(v => v).ToList();

            readonly WeakReference<PhonebookContactsListViewController> viewControllerWeakReference;
            readonly WeakReference<UITableView> tableViewWeakReference;
            readonly string emptyText;

            bool loading = true;
            readonly List<List<Recipient>> items = new List<List<Recipient>>();

            public DataSource(PhonebookContactsListViewController viewController, UITableView tableView, string emptyText)
            {
                viewControllerWeakReference = viewController.Wrap();
                tableViewWeakReference = tableView.Wrap();
                this.emptyText = emptyText;
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

                var pbc = items[indexPath.Section][indexPath.Row];

                if (string.IsNullOrWhiteSpace(pbc.Name))
                {
                    var cell = tableView.DequeueReusableCell("cell1") ?? UITableViewCellUtilities.CreateDefault("cell1");
                    cell.TextLabel.Text = pbc.Address;
                    return cell;
                }
                else
                {
                    var cell = tableView.DequeueReusableCell("cell2") ?? UITableViewCellUtilities.CreateWithSubtitle("cell2");
                    cell.TextLabel.Text = pbc.Name;
                    cell.DetailTextLabel.Text = pbc.Address;
                    return cell;
                }
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                if (loading || Empty)
                    return 1;

                return items.Count;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading || Empty)
                    return 1;

                return items[(int)section].Count;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var ra = items[indexPath.Section][indexPath.Row];
                viewControllerWeakReference.Unwrap()?.PhonebookAddressSelected(ra, tableView.CellAt(indexPath));
            }

            public override string[] SectionIndexTitles(UITableView tableView) => items.Select(i => i.First()?.Name.SafeSubstring(0, 1).ToUpper())
                                                                                       .ToArray();

            public void SetItems(List<Recipient> phonebookContacts)
            {
                loading = false;

                items.Clear();
                items.AddRange(phonebookContacts.GroupBy(cp => cp.Name.SafeSubstring(0, 1)).Select(s => s.ToList()));

                var sectionsCount = items.Count;

                tableViewWeakReference.Unwrap()?.BeginUpdates();
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                if (sectionsCount > 1)
                    tableViewWeakReference.Unwrap()?.InsertSections(NSIndexSet.FromNSRange(new NSRange(1, sectionsCount - 1)), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.EndUpdates();
            }

            public void Reset()
            {
                loading = true;

                var sectionsCount = tableViewWeakReference.Unwrap()?.NumberOfSections() ?? 0;

                items.Clear();

                tableViewWeakReference.Unwrap()?.BeginUpdates();
                if (sectionsCount > 1)
                    tableViewWeakReference.Unwrap()?.DeleteSections(NSIndexSet.FromNSRange(new NSRange(1, sectionsCount - 1)), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.EndUpdates();
            }
        }
    }
}
