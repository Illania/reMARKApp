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
        readonly TaskCompletionSource<Recipient> tcs = new TaskCompletionSource<Recipient>();
        public Task<Recipient> Result => tcs.Task;

        public Folder Folder { get; set; }
        public Contact Contact { get; set; }
        public ContactPreview ContactPreview { get; set; }

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

            if (Integration.IsRunningAtLeast(11))
            {
                if (NavigationController != null)
                    NavigationController.NavigationBar.PrefersLargeTitles = true;
                NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Automatic;
            }

            InitializeHandlers();
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info("Appeared");

            if (((DataSource)TableView.Source).Empty)
                RefreshData();


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
            TableView.Source = new DataSource(this, TableView, Localization.GetString("contacts_list_empty"));
            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 20f;
        }

        void InitializeSearchBar()
        {
            DefinesPresentationContext = true;

            var searchResultsController = new UITableViewController();
            var searchResultsDataSource = new DataSource(this, searchResultsController.TableView, Localization.GetString("no_contacts_found"));
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
                return Contact.Children.Union(new List<ContactPreview> { Contact.PrimaryPerson }).ToList().OrderBy(c=> c.Type);
            }).ContinueWith(async t =>
            {
                if (t.IsFaulted)
                {
                    var ex = t.Exception.InnerException;
                    CommonConfig.Logger.Error($"Error while retrieving contacts", ex);
                    await Dialogs.ShowErrorAlertAsync(this, ex);
                    tcs.SetResult(null);
                    return;
                }

                if (t.Result == null)
                {
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

        public async void LinkedContactSelected(ContactPreview cp, UITableViewCell cell)
        {
            var contact = await Managers.ContactsManager.GetContactAsync(Folder, cp.Id);
            var emailAddresses = contact.CommunicationAddresses.Where(ca => ca.Type == CommunicationAddressType.Email).Select(ca => ca.Address).ToArray();
            if (emailAddresses.Any())
            {
                var index = await Dialogs.ShowListActionSheetAsync(this, emailAddresses, TableView, cell);
                if (index < 0)
                    return;

                var address = emailAddresses[index];

                tcs.SetResult(new Recipient(ContactPreview.Name, address, RecipientType.Contact, ContactPreview.Id));
                DismissViewController(true, null);
            }
            else
                await Dialogs.ShowConfirmAlertAsync(this, Localization.GetString("no_email_addresses_title"), Localization.GetString("no_email_addresses_content"));

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

        static bool MatchesQuery(ContactPreview cp, string query)
        {
            if (cp.Name?.ContainsCaseInsensitive(query) ?? false)
                return true;

            return false;
        }

        class DataSource : UITableViewSource
        {
            public bool Empty => !items.SelectMany(v => v).Any();
            public List<ContactPreview> Items => items.SelectMany(v => v).ToList();

            readonly WeakReference<LinkedEmailListViewController> viewControllerWeakReference;
            readonly WeakReference<UITableView> tableViewWeakReference;
            readonly string emptyText;

            bool loading = true;
            readonly List<List<ContactPreview>> items = new List<List<ContactPreview>>();

            public DataSource(LinkedEmailListViewController viewController, UITableView tableView, string emptyText)
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

                var cp = items[indexPath.Section][indexPath.Row];
                string type;
                if (cp.Type == ContactType.Person)
                    type = Localization.GetString("person");
                else if (cp.Type == ContactType.Department)
                    type = Localization.GetString("department");
                else
                    type = Localization.GetString("company");
                var cell = tableView.DequeueReusableCell(ContactInfoTableViewCell.DefaultId) as ContactInfoTableViewCell ?? new ContactInfoTableViewCell();
                cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
                cell.SelectionStyle = UITableViewCellSelectionStyle.Default;
                cell.Initialize(type.ToUpper(), cp.Name);
                return cell;
 
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
                var cp = items[indexPath.Section][indexPath.Row];
                viewControllerWeakReference.Unwrap()?.LinkedContactSelected(cp, tableView.CellAt(indexPath));
            }

            public override string[] SectionIndexTitles(UITableView tableView) => items.Select(i => i.First()?.Name.SafeSubstring(0, 1).ToUpper())
                                                                                       .ToArray();

            public void SetItems(List<ContactPreview> phonebookContacts)
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
