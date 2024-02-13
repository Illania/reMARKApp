using UIKit;
using Foundation;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Manager;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.Common.Utilities.Extensions;
using reMark.Mobile.IOS.Ui.Common;
using reMark.Mobile.IOS.Ui.TableViewCells;
using reMark.Mobile.IOS.Utilities;

namespace reMark.Mobile.IOS.Ui.ViewControllers
{
    public class PresetCategoryViewController : AbstractTableViewController, IUISearchResultsUpdating
    {
       
        BusinessEntityPreview BusinessEntityPreview { get; set; }
        List<Category> availableCategories = new List<Category>();
        UISearchController searchController;

        CancellationTokenSource searchCancellationTokenSource;
        readonly List<CancellationTokenSource> searchCancellationTokenSourceList = new List<CancellationTokenSource>();

        public PresetCategoryViewController() : base(UITableViewStyle.Grouped)
        {


        }

        public override void LoadView()
        {
            base.LoadView();

            if (BusinessEntityPreview != null)
                CommonConfig.UsageAnalytics.LogEvent(new OpenCategoriesEvent(BusinessEntityPreview.ModuleType));

            InitializeView();
            InitializeNavigationBar();
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

            if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
                ModalInPresentation = true;
        }

        public override async void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            if (((DataSource)TableView.Source).Empty)
                await RefreshDataAsync();

            if (Integration.IsRunningAtLeast(11))
                NavigationItem.SearchController = searchController;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
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

            searchCancellationTokenSource?.Dispose();
            searchCancellationTokenSource = null;
            searchCancellationTokenSourceList.ForEach(cts => cts?.Dispose());
            searchCancellationTokenSourceList.Clear();

            ((DataSource)TableView.Source)?.Reset();

            searchController.SearchResultsUpdater = null;
            searchController = null;
        }

        #region Initializing/deinitializing
        void InitializeView()
        {
            TableView.Source = new DataSource(this, TableView);
            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 40f;
            TableView.AllowsSelection = true;
            TableView.AllowsMultipleSelection = false;
        }

        void InitializeNavigationBar()
        {
            Title = Localization.GetString("categories");
 
        }
        #endregion

        public async Task RefreshDataAsync()
        {
            try
            {
                availableCategories = await Managers.DocumentsManager.GetAllCategoriesAsync();

            }
            catch (Exception ex)
            {
                await Dialogs.ShowErrorAlertAsync(this, ex);
            }

            ReloadTable();
        }


        public void ReloadTable()
        {
            if (searchController != null)
                searchController.Active = false;

            ((DataSource)TableView.Source).SetItems(DataSource.Section.Available, availableCategories);

            TableView.ReloadData();
        }



        #region Search related
        protected virtual void InitializeSearchBar()
        {
            DefinesPresentationContext = true;
            var searchResultsController = new UITableViewController();
            var searchResultsDataSource = new SearchDataSource(this, searchResultsController.TableView);
            searchResultsController.TableView.Source = searchResultsDataSource;
            searchResultsController.TableView.EstimatedRowHeight = 50f;
            searchResultsController.TableView.RowHeight = UITableView.AutomaticDimension;

            searchController = new UISearchController(searchResultsController)
            {
                HidesNavigationBarDuringPresentation = true,
                DimsBackgroundDuringPresentation = true,
                ObscuresBackgroundDuringPresentation = true,
                SearchResultsUpdater = this
            };

            searchController.SearchBar.Placeholder = Localization.GetString("filter");

            if (!Integration.IsRunningAtLeast(11))
            {
                TableView.TableHeaderView = searchController.SearchBar;
            }
        }

        void IUISearchResultsUpdating.UpdateSearchResultsForSearchController(UISearchController searchController)
        {
            var searchText = searchController.SearchBar.Text;

            if (!searchController.Active || string.IsNullOrWhiteSpace(searchText))
            {
                searchCancellationTokenSourceList.ForEach(cts => cts?.Cancel());
                searchCancellationTokenSourceList.Clear();
                var dataSource = ((UITableViewController)searchController.SearchResultsController).TableView.Source;
                ((SearchDataSource)dataSource)?.Reset();
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
                DoSearchCategories(searchText, searchCancellationTokenSource.Token);
            }

        }

        async void DoSearchCategories(string searchText, CancellationToken cancellationToken)
        {
            try
            {
                var tableViewController = searchController?.SearchResultsController as UITableViewController;
                var dataSource = tableViewController?.TableView?.Source as SearchDataSource;
                dataSource?.Reset();

                await Task.Delay(200);

                if (cancellationToken.IsCancellationRequested)
                    return;

                var searchResultCategories = new List<Category>();

                searchResultCategories = availableCategories.Where(x => x.Name.ToLower().Contains(searchText.ToLower())).ToList();

                if (cancellationToken.IsCancellationRequested)
                    return;

                dataSource?.SetSearchCategories(searchResultCategories, searchText);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while searching.", ex);
            }
        }
        #endregion

   


        #region DataSource
        class DataSource : UITableViewSource
        {
            public static class Section
            {
                public static readonly nint Available = 0;
            }

            readonly Dictionary<nint, List<Category>> items;

            public bool Empty => items.All(kv => kv.Value.Count < 1);

            bool[] loading;

            NSIndexPath selectedBefore;

            readonly WeakReference<UITableView> tableViewWeakReference;
            readonly WeakReference<PresetCategoryViewController> viewControllerWeakReference;

            public DataSource(PresetCategoryViewController presetViewController, UITableView tableView)
            {
                tableViewWeakReference = tableView.Wrap();
                viewControllerWeakReference = presetViewController.Wrap();

                loading = new[] { true, true, true };

                items = new Dictionary<nint, List<Category>>
                {
                    [Section.Available] = new List<Category>()
                };
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading[indexPath.LongSection])
                    return tableView.DequeueReusableCell(WaitTableViewCell.DefaultId) as WaitTableViewCell ?? new WaitTableViewCell();

                if (items[indexPath.LongSection].Count < 1)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.DefaultId) as EmptyTableViewCell ?? new EmptyTableViewCell();
                    emptyCell.Initialize(Localization.GetString("no_categories"));
                    return emptyCell;
                }

                var cell = tableView.DequeueReusableCell(CategoriesTableViewCell.DefaultId) as CategoriesTableViewCell ?? new CategoriesTableViewCell();

                cell.Initialize(items[indexPath.LongSection][indexPath.Row]);

                cell.Accessory = UITableViewCellAccessory.None;


                if (items[indexPath.LongSection][indexPath.Row].Id == PlatformConfig.Preferences.PresetCategoryId)
                {
                    cell.Accessory = UITableViewCellAccessory.Checkmark;
                    selectedBefore = indexPath;
                }
                else cell.Accessory = UITableViewCellAccessory.None;

                return cell;
            }


            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                tableView.AllowsSelection = false; // to prevent exceptions when 'quickly' tapping rows
                tableView.BeginUpdates();

                //get selected category
                var category = items[indexPath.LongSection][indexPath.Row];

                if (indexPath != null)
                {

                    UITableViewCell cellNow = tableView.CellAt(indexPath);//currently selected cell

                    if (selectedBefore != null)
                    {
                        UITableViewCell cellOld = tableView.CellAt(selectedBefore);
                        if (selectedBefore != indexPath && cellOld!=null)
                        {

                            cellOld.Accessory = UITableViewCellAccessory.None;
                            tableView.DeselectRow(selectedBefore, true);
                        }
                    }

                     cellNow.Accessory = UITableViewCellAccessory.Checkmark;
                     selectedBefore = indexPath;

                     PlatformConfig.Preferences.PresetCategoryId = category.Id;
                }

                tableView.EndUpdates();
                tableView.AllowsSelection = true;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return items[section].Count;
            }

            public void SetItems(nint section, List<Category> categories)
            {
                items[section].Clear();
                items[section].AddRange(categories.OrderBy(c => c.Name));
                loading[section] = false;
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(section), UITableViewRowAnimation.Fade);
            }

            public override nint NumberOfSections(UITableView tableView) => items.Keys.Count;

            public void Reset()
            {
                for (var i = 0; i < loading.Length; i++)
                    loading[i] = true;

                foreach (var kv in items)
                    kv.Value.Clear();

                tableViewWeakReference.Unwrap()?.BeginUpdates();
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(Section.Available), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.EndUpdates();
            }

            public List<Category> GetCategoriesInSection(nint section) => items[section];

            public int GetItemsInSection(nint section) => items[section].Count;

            public override string TitleForHeader(UITableView tableView, nint section)
            {
                if (section == Section.Available)
                    return Localization.GetString("select_categories");

                return "";
            }

            public override void WillDisplayHeaderView(UITableView tableView, UIView headerView, nint section) => headerView.ApplyTheme();


        }

        class SearchDataSource : UITableViewSource
        {
            public bool Empty => items.Count < 1;
            public List<Category> Items => items.ToList();

            string searchQuery = String.Empty;

            readonly WeakReference<PresetCategoryViewController> viewControllerWeakReference;
            readonly WeakReference<UITableView> tableViewWeakReference;
            readonly List<Category> items = new List<Category>();

            bool loading = true;

            public SearchDataSource(PresetCategoryViewController viewController, UITableView tableView)
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
                    emptyCell.Initialize(Localization.GetString("no_categories"));
                    return emptyCell;
                }

                var cell = tableView.DequeueReusableCell(CategoriesTableViewCell.DefaultId) as CategoriesTableViewCell ?? new CategoriesTableViewCell();
                cell.Initialize(items[indexPath.Row]);
                cell.Accessory = UITableViewCellAccessory.None;

                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return loading || Empty ? 1 : (nint)items.Count;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var category = items[indexPath.Row];
                PlatformConfig.Preferences.PresetCategoryId = category.Id;
                viewControllerWeakReference.Unwrap()?.ReloadTable();
            }

            public void SetSearchCategories(List<Category> categories, string searchText)
            {
                if ((items != null) && searchQuery.Equals(searchText))
                {
                    items.Union(categories, new CategoryComparer()).ToList();
                    tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                }
                else
                {
                    items.Clear();
                    items.AddRange(categories);
                    loading = false;
                    tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                }

                searchQuery = searchText;
            }

            public void Reset()
            {
                loading = true;
                items.Clear();
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }
        }
        #endregion
    }
}
