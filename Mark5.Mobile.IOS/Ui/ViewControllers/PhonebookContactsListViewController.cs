using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class PhonebookContactsListViewController : AbstractViewController, IUISearchResultsUpdating
    {
        readonly TaskCompletionSource<Recipient> tcs = new TaskCompletionSource<Recipient>();
        public Task<Recipient> Task => tcs.Task;

        UIBarButtonItem exitEditItem;
        UITableView tableView;

        UITableViewController searchResultsController;
        DataSource searchResultsDataSource;
        UISearchController searchController;

        CancellationTokenSource cts;

        CancellationTokenSource searchCancellationTokenSource;
        readonly List<CancellationTokenSource> searchCancellationTokenSourceList = new List<CancellationTokenSource>();

        #region UIViewControllerOverrides

        public override void LoadView()
        {
            base.LoadView();

            InitializeNavigationBar();
            InitializeNavigationBarTitle();
            InitializeView();
            InitializeSearchBar();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            InitializeHandlers();
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info("Appeared");

            var ds = (DataSource) tableView.Source;
            if (ds.Empty)
                RefreshData();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeinitializeHandlers();

            cts?.Cancel();
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning("Received memory warning!");

            var ds = tableView?.Source as DataSource;
            ds?.Reset();

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        #endregion

        #region Initialization

        void InitializeNavigationBar()
        {
            exitEditItem = new UIBarButtonItem(UIBarButtonSystemItem.Cancel);
            NavigationItem.SetLeftBarButtonItem(exitEditItem, true);
        }

        void InitializeView()
        {
            AutomaticallyAdjustsScrollViewInsets = true;

            tableView = new UITableView();
            tableView.ClipsToBounds = false;
            tableView.Source = new DataSource(this, tableView, Localization.GetString("phonebook_contacts_empty"));
            tableView.AllowsSelectionDuringEditing = false;
            tableView.AllowsMultipleSelectionDuringEditing = true;
            tableView.TranslatesAutoresizingMaskIntoConstraints = false;
            tableView.EstimatedRowHeight = 44f;
            tableView.RowHeight = UITableView.AutomaticDimension;
            View.AddSubview(tableView);
            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1f, 0f)
            });
        }

        void InitializeNavigationBarTitle()
        {
            UIView.AnimationsEnabled = false;
            NavigationItem.Title = Localization.GetString("phonebook_contacts_title");
            NavigationItem.Prompt = null;
            UIView.AnimationsEnabled = true;
        }

        void InitializeSearchBar()
        {
            DefinesPresentationContext = true;

            searchResultsController = new UITableViewController();
            searchResultsDataSource = new DataSource(this, searchResultsController.TableView, Localization.GetString("no_phonebook_matching"));
            searchResultsController.TableView.Source = searchResultsDataSource;
            searchResultsController.TableView.EstimatedRowHeight = 44f;
            searchResultsController.TableView.RowHeight = UITableView.AutomaticDimension;

            searchController = new UISearchController(searchResultsController)
            {
                HidesNavigationBarDuringPresentation = true,
                DimsBackgroundDuringPresentation = true,
                ObscuresBackgroundDuringPresentation = true,
                SearchResultsUpdater = this
            };
            searchController.SearchBar.Placeholder = Localization.GetString("filter");

            tableView.TableHeaderView = searchController.SearchBar;
        }

        void InitializeHandlers()
        {
            if (exitEditItem != null)
                exitEditItem.Clicked += ExitEditItem_Clicked;
        }

        void DeinitializeHandlers()
        {
            if (exitEditItem != null)
                exitEditItem.Clicked -= ExitEditItem_Clicked;
        }

        #endregion

        #region Refreshing

        void RefreshData()
        {
            cts?.Cancel();
            cts = new CancellationTokenSource();

            List<Recipient> contacts = null;

            System.Threading.Tasks.Task.Run(() =>
              {
                  contacts = CommonConfig.Phonebook.GetPhonebookContacts();
              }).ContinueWith(t =>
           {
               InvokeOnMainThread(async () =>
               {
                   if (t.IsFaulted)
                   {
                       var ex = t.Exception.InnerException;
                       CommonConfig.Logger.Error($"Error while retrieving phonebook contacts", ex);
                       await Dialogs.ShowErrorDialogAsync(this, ex);
                       tcs.SetResult(null);
                   }

                   if (contacts == null)
                   {
                       await Dialogs.ShowConfirmDialogAsync(this, Localization.GetString("phonebook_contacts_no_access_title"),
                                                            Localization.GetString("phonebook_contacts_no_access_content"));
                       tcs.SetResult(null);
                   }
                   else
                   {
                       var ds = (DataSource)tableView.Source;
                       ds.SetItems(contacts.OrderBy(c => c.Name).ToList());
                   }
               });

           });
        }

        #endregion

        #region Actions

        public void PhonebookAddressSelected(Recipient pb, UITableViewCell cell)
        {
            tcs.SetResult(pb);
        }

        #endregion

        #region Event Handlers

        void ExitEditItem_Clicked(object sender, EventArgs e)
        {
            tcs.SetResult(null);
        }

        #endregion

        #region Filter

        public void UpdateSearchResultsForSearchController(UISearchController searchController)
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

                DoSearchContacts(searchText, searchCancellationTokenSource.Token);
            }
        }

        async void DoSearchContacts(string searchText, CancellationToken ct)
        {
            searchResultsDataSource.Reset();

            await System.Threading.Tasks.Task.Delay(500);

            if (ct.IsCancellationRequested)
                return;

            var ds = (DataSource) tableView.Source;
            var filteredContacts = ds.Items.Where(cp => MatchesQuery(cp, searchText)).ToList();

            if (ct.IsCancellationRequested)
                return;

            searchResultsDataSource.SetItems(filteredContacts);
        }

        static bool MatchesQuery(Recipient cp, string query)
        {
            if (cp.Name?.ContainsCaseInsensitive(query) ?? false)
                return true;

            if (cp.Address?.ContainsCaseInsensitive(query) ?? false)
                return true;

            return false;
        }

        #endregion

        class DataSource : UITableViewSource, IDisposable
        {
            public bool Empty => !phonebookContactsInView.SelectMany(v => v).Any();

            public List<Recipient> Items => phonebookContactsInView.SelectMany(v => v).ToList();

            PhonebookContactsListViewController viewController;
            UITableView tableView;
            readonly string emptyText;

            List<List<Recipient>> phonebookContactsInView = new List<List<Recipient>>(25);

            bool loading = true;

            public DataSource(PhonebookContactsListViewController viewController, UITableView tableView, string emptyText)
            {
                this.viewController = viewController;
                this.tableView = tableView;
                this.emptyText = emptyText;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as WaitTableViewCell ?? WaitTableViewCell.Create();

                if (Empty)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                    emptyCell.Initialize(emptyText);
                    return emptyCell;
                }

                var ra = phonebookContactsInView[indexPath.Section][indexPath.Row];

                var cell = tableView.DequeueReusableCell(SuggestionsTableViewCell.Key) as SuggestionsTableViewCell ?? SuggestionsTableViewCell.Create();
                cell.Initialize(ra);

                return cell;
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                if (loading || Empty)
                    return 1;

                return phonebookContactsInView.Count;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading || Empty)
                    return 1;

                return phonebookContactsInView[(int) section].Count;
            }

            public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
            {
                return false;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                if (tableView.Editing)
                    return;

                var ra = phonebookContactsInView[indexPath.Section][indexPath.Row];
                viewController.PhonebookAddressSelected(ra, tableView.CellAt(indexPath));
            }

            public override string[] SectionIndexTitles(UITableView tableView)
            {
                return phonebookContactsInView.Select(i => i.First()?.Name.SafeSubstring(0, 1).ToUpper()).ToArray();
            }

            public void SetItems(List<Recipient> phonebookContacts)
            {
                loading = false;

                phonebookContactsInView = phonebookContacts.GroupBy(cp => cp.Name.SafeSubstring(0, 1)).Select(s => s.ToList()).ToList();
                tableView.ReloadData();
            }

            public void Reset()
            {
                loading = true;

                var count = phonebookContactsInView.Count;

                phonebookContactsInView.Clear();
                tableView.ReloadData();
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                viewController = null;
                tableView = null;
                phonebookContactsInView = null;
            }
        }
    }
}
