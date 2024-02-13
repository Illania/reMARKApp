using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Manager;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Utilities.Extensions;
using reMark.Mobile.IOS.Ui.Common;
using reMark.Mobile.IOS.Ui.TableViewCells;
using reMark.Mobile.IOS.Utilities;
using reMark.Mobile.Common.Extensions;
using UIKit;
using AngleSharp.Dom;

namespace reMark.Mobile.IOS.Ui.ViewControllers
{
    public class SavedContactSearchesViewController: AbstractTableViewController, IUISearchResultsUpdating
    {
        readonly TaskCompletionSource<SavedContactsSearch> tcs = new TaskCompletionSource<SavedContactsSearch>();
        public Task<SavedContactsSearch> Result => tcs.Task;

        UIBarButtonItem cancelItem;
        UIBarButtonItem doneItem;

        public int DocumentId { get; set; }
        public string ReferenceNumber { get; set; }

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

            cancelItem= null;
            doneItem = null;

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
            NavigationItem.Title = Localization.GetString("saved_searches");
            cancelItem = new UIBarButtonItem(UIBarButtonSystemItem.Cancel);
            NavigationItem.SetLeftBarButtonItem(cancelItem, false);

            doneItem = new UIBarButtonItem(UIBarButtonSystemItem.Done)
            {
                Enabled = false
            };
            NavigationItem.SetRightBarButtonItem(doneItem, false);
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
            if (cancelItem != null)
                cancelItem.Clicked += CancelItem_Clicked;

            if (doneItem != null)
                doneItem.Clicked += DoneItem_Clicked;
        }

        void DeinitializeHandlers()
        {
            if (cancelItem != null)
                cancelItem.Clicked -= CancelItem_Clicked;

            if (doneItem != null)
                doneItem.Clicked -= DoneItem_Clicked;
        }

        async Task RefreshData()
        {
            var searches = await Managers.SearchManager.GetSavedContactsSearchesAsync();
            if(searches != null)
                ((DataSource)TableView.Source).SetItems(searches);
            doneItem.Enabled = true;
        }

        void CancelItem_Clicked(object sender, EventArgs e)
        {
            tcs.SetResult(null);
            DismissViewController(true, null);
        }

        void DoneItem_Clicked(object sender, EventArgs e)
        {
            tcs.SetResult(((DataSource)TableView.Source).SelectedSearch);
            DismissViewController(true, null);
        }

        public void RowSelected(SavedContactsSearch sds)
        {
            tcs.SetResult(((DataSource)TableView.Source).SelectedSearch);
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

                DoSearchSavedSearch(searchText, searchCancellationTokenSource.Token);
            }
        }

        async void DoSearchSavedSearch(string searchText, CancellationToken ct)
        {
            var tableViewController = searchController?.SearchResultsController as UITableViewController;
            var dataSource = tableViewController?.TableView?.Source as DataSource;
            dataSource?.Reset();

            await Task.Delay(500);

            if (ct.IsCancellationRequested)
                return;

            var ds = (DataSource)TableView.Source;
            var filteredSearches = ds.Items.Where(cp => MatchesQuery(cp, searchText)).ToList();

            if (ct.IsCancellationRequested)
                return;

            dataSource?.SetItems(filteredSearches);
        }

        static bool MatchesQuery(SavedContactsSearch sds, string query)
        {
            if (sds.Name?.ContainsCaseInsensitive(query) ?? false)
                return true;

            return false;
        }

        async void DeleteSavedSearch(SavedContactsSearch savedContactsSearch)
        {
            TableView.Editing = false;

            try
            {

                var deleteConfirmed = await Dialogs.ShowYesNoAlertAsync(this, Localization.GetString("confirm_saved_search_deletion_title"),
                                                                      string.Format(Localization.GetString("confirm_saved_search_deletion_content")));
                if (!deleteConfirmed)
                    return;

                var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("deleting_saved_search___"));
                await Managers.SearchManager.DeleteSavedSearchAsync(savedContactsSearch.Id);
                dismissAction();

                ((DataSource)TableView.Source).RemoveSavedSearch(savedContactsSearch);
                ((DataSource)TableView.Source).SortItems();

            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Failed to delete saved document search [Id={savedContactsSearch.Id}] "
                                             , ex.InnerException);
                await Dialogs.ShowErrorAlertAsync(this, ex.InnerException);
            }

        }


        class DataSource : UITableViewSource
        {
            public bool Empty => !Items.Any();

            UILabelScalable topLabel;

            readonly WeakReference<SavedContactSearchesViewController> viewControllerWeakReference;
            readonly WeakReference<UITableView> tableViewWeakReference;

            bool loading = true;
            public List<SavedContactsSearch> Items = new();

            public DataSource(SavedContactSearchesViewController viewController, UITableView tableView)
            {
                viewControllerWeakReference = viewController.Wrap();
                tableViewWeakReference = tableView.Wrap();
            }

            public SavedContactsSearch SelectedSearch
            {
                get
                {
                    var tableView = tableViewWeakReference.Unwrap();
                    if (tableView.IndexPathForSelectedRow == null || tableView.IndexPathForSelectedRow.Row < 0)
                        return null;
                    return Items[tableView.IndexPathForSelectedRow.Row];
                }
                set
                {
                    var selectedId = value.Id;
                    for (var i = 0; i < Items.Count; i++)
                        if (selectedId == Items[i].Id)
                        {
                            var ip = NSIndexPath.FromRowSection(i, 0);
                            tableViewWeakReference.Unwrap()?.SelectRow(ip, false, UITableViewScrollPosition.None);
                            RowSelected(tableViewWeakReference.Unwrap(), ip);
                        }
                }
            }


            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                    return tableView.DequeueReusableCell(WaitTableViewCell.DefaultId) as WaitTableViewCell ?? new WaitTableViewCell();

                if (Empty)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.DefaultId) as EmptyTableViewCell ?? new EmptyTableViewCell();
                    emptyCell.Initialize(Localization.GetString("saved_searches_list_empty"));
                    return emptyCell;
                }

                var savedSearch = Items[indexPath.Row];
                var cell = tableView.DequeueReusableCell("cell") ?? UITableViewCellUtilities.CreateDefault("cell");

                var leadingMarginGuide = new UILayoutGuide();
                cell.ContentView.AddLayoutGuide(leadingMarginGuide);
                cell.ContentView.ClipsToBounds = true;
                leadingMarginGuide.LeadingAnchor.ConstraintEqualTo(cell.ContentView.ReadableContentGuide.LeadingAnchor).Active = true;

                var leadingMarginWidthAnchor = leadingMarginGuide.WidthAnchor.ConstraintEqualTo(0f);
                leadingMarginWidthAnchor.SetIdentifier("leadingMarginWidth");
                leadingMarginWidthAnchor.Active = true;

                topLabel = new UILabelScalable
                {
                    Font = Theme.DefaultBoldFont.CustomFont(),
                    Lines = 1,
                    TranslatesAutoresizingMaskIntoConstraints = false
                };

                cell.ContentView.AddSubview(topLabel);
                cell.ContentView.AddConstraints(new[]
                {
                    topLabel.TopAnchor.ConstraintEqualTo(cell.ContentView.TopAnchor, 8f),
                    topLabel.LeadingAnchor.ConstraintEqualTo(leadingMarginGuide.TrailingAnchor, 15f + 8f),
                    topLabel.HeightAnchor.ConstraintGreaterThanOrEqualTo(Theme.MinimumLabelSize),
                    topLabel.TrailingAnchor.ConstraintEqualTo(cell.ContentView.ReadableContentGuide.TrailingAnchor),
                    topLabel.BottomAnchor.ConstraintEqualTo(cell.ContentView.BottomAnchor, -8f),
                });

                topLabel.Text = savedSearch.Name;
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
                var td = Items[indexPath.Row];
                viewControllerWeakReference.Unwrap()?.RowSelected(td);
            }

            public void RemoveSavedSearch(SavedContactsSearch savedContactsSearch)
            {
                var position = Items.FindIndex(c => c.Id == savedContactsSearch.Id);
                if (position >= 0)
                {
                    Items.RemoveAt(position);

                    if (Items.Count == 0)
                        tableViewWeakReference.Unwrap()?.ReloadRows(new[] { NSIndexPath.FromRowSection(position, 0) }, UITableViewRowAnimation.Fade);
                    else
                        tableViewWeakReference.Unwrap()?.DeleteRows(new[] { NSIndexPath.FromRowSection(position, 0) }, UITableViewRowAnimation.Fade);
                }
            }

            public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
            {

                var actions = new List<UITableViewRowAction>();

                if (indexPath.Row < 0 || indexPath.Row >= Items.Count)
                    return actions.ToArray();

                var savedContactsSearch = Items[indexPath.Row];


                var deleteAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Destructive,
                                                                Localization.GetString("delete"),
                                                                (a, ip) => viewControllerWeakReference.Unwrap()?.DeleteSavedSearch(savedContactsSearch));     
                deleteAction.BackgroundColor = Theme.DarkerBlue;
                actions.Add(deleteAction);
                return actions.ToArray();
            }

            public void SetItems(List<SavedContactsSearch> destinations)
            {
                loading = false;

                Items.AddRange(destinations);

                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }


            public void Reset()
            {
                loading = true;
                Items.Clear();

                tableViewWeakReference.Unwrap()?.BeginUpdates();
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.EndUpdates();
            }

            public void SortItems()
            {
                Items.Sort();
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }
        }

    }
}
