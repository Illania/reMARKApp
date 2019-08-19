using UIKit;
using Foundation;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using Mark5.Mobile.IOS.Utilities;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class CategoriesListViewController : AbstractTableViewController, IUISearchResultsUpdating
    {
        BusinessEntityPreview BusinessEntityPreview { get; set; }
        List<Category> categories = new List<Category>();
        List<Category> allCategories = new List<Category>();
        UIBarButtonItem cancelBtnItem;
        UIBarButtonItem saveBtnItem;
        UISearchController searchController;

        CancellationTokenSource searchCancellationTokenSource;
        readonly List<CancellationTokenSource> searchCancellationTokenSourceList = new List<CancellationTokenSource>();

        public CategoriesListViewController(BusinessEntityPreview businessEntityPreview) : base(UITableViewStyle.Grouped)
        {
            this.BusinessEntityPreview = businessEntityPreview;
            if (businessEntityPreview is DocumentPreview documentPreview)
            {
                categories.AddRange(documentPreview.Categories);
            }

            if (businessEntityPreview is ContactPreview contactPreview)
            {
                categories.AddRange(contactPreview.Categories);
            }
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

            if (NavigationController != null)
                NavigationController.NavigationBar.PrefersLargeTitles = true;
            NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Automatic;
        }

        public override async void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            if (((DataSource)TableView.Source).Empty)
                await RefreshDataAsync();

            NavigationItem.SearchController = searchController;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            DeinitializeHandlers();
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

            cancelBtnItem = null;
            saveBtnItem = null;

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
        }

        void InitializeNavigationBar()
        {
            Title = Localization.GetString("categories");
            cancelBtnItem = new UIBarButtonItem(UIBarButtonSystemItem.Cancel);
            cancelBtnItem.Clicked += CancelBtnItem_Clicked;
            NavigationItem.SetLeftBarButtonItem(cancelBtnItem, false);
            saveBtnItem = new UIBarButtonItem(UIBarButtonSystemItem.Save);
            saveBtnItem.Clicked += SaveBtnItem_Clicked;
            NavigationItem.SetRightBarButtonItem(saveBtnItem, false);
        }

        protected virtual void DeinitializeHandlers()
        {
            if (cancelBtnItem != null)
                cancelBtnItem.Clicked -= CancelBtnItem_Clicked;

            if (saveBtnItem != null)
                saveBtnItem.Clicked -= SaveBtnItem_Clicked;
        }
        #endregion

        public async Task RefreshDataAsync()
        {
            try
            {
                allCategories = BusinessEntityPreview is DocumentPreview documentPreview
                    ? await Managers.DocumentsManager.GetAllCategoriesAsync()
                    : await Managers.ContactsManager.GetAllCategoriesAsync();
            }
            catch (Exception ex)
            {
                await Dialogs.ShowErrorAlertAsync(this, ex);
            }

            ReloadTable();
        }

        public void ReloadTable()
        {
            var availableCategories = new List<Category>();

            categories.Sort((x, y) => String.Compare(x.Name, y.Name, true));

            ((DataSource)TableView.Source).SetItems(DataSource.Section.Selected, categories);
            availableCategories = allCategories.Where(x => !categories.Contains(x)).ToList();

            availableCategories.Sort((x, y) => String.Compare(x.Name, y.Name, true));

            ((DataSource)TableView.Source).SetItems(DataSource.Section.Available, availableCategories);

            if (searchController != null)
                searchController.Active = false;

            TableView.ReloadData();
        }

        protected virtual void MoveCategory(Category category)
        {
            if (category == null)
                return;

            if (categories.Contains(category))
            {
                categories.Remove(category);
            }
            else
            {
                categories.Add(category);
            }
        }

        List<Category> GetAvailableCategories()
        {
            ((DataSource)TableView.Source).SetItems(DataSource.Section.Selected, categories);
            return allCategories.Where(x => !categories.Contains(x)).ToList();
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

                searchResultCategories = allCategories.Where(x => x.Name.ToLower().Contains(searchText.ToLower()) && !categories.Contains(x)).ToList();

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

        #region Navigation bar related
        async void CancelBtnItem_Clicked(object sender, EventArgs e)
        {
            if (BusinessEntityPreview is DocumentPreview documentPreview && categories.SequenceEqual(documentPreview.Categories)
              || BusinessEntityPreview is ContactPreview contactPreview && categories.SequenceEqual(contactPreview.Categories))
            {
                DismissViewController(true, null);
            }
            else
            {
                var response = await Dialogs.ShowYesNoAlertAsync(this, Localization.GetString("changes_not_saved"),
                                                                 Localization.GetString("changes_not_saved_description"),
                                                                 Localization.GetString("ok"),
                                                                 Localization.GetString("cancel"));
                if (response)
                    DismissViewController(true, null);
            }
        }

        async void SaveBtnItem_Clicked(object sender, EventArgs e)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("updating_categories___"));
            CommonConfig.Logger.Info($"Updating categories... [entity={BusinessEntityPreview}]");
            try
            {
                if (BusinessEntityPreview is DocumentPreview documentPreview)
                    await Managers.DocumentsManager.SetCategoriesAsync(documentPreview, categories);

                if (BusinessEntityPreview is ContactPreview contactPreview)
                    await Managers.ContactsManager.SetCategoriesAsync(contactPreview, categories);
                dismissAction();
                DismissViewController(true, null);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while updating categories [entity={BusinessEntityPreview}]", ex);

                dismissAction();

                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }
        #endregion

        #region DataSource
        class DataSource : UITableViewSource
        {
            public static class Section
            {
                public static readonly nint Selected = 0;
                public static readonly nint Available = 1;
            }

            readonly Dictionary<nint, List<Category>> items;

            public bool Empty => items.All(kv => kv.Value.Count < 1);

            bool[] loading;

            readonly WeakReference<UITableView> tableViewWeakReference;
            readonly WeakReference<CategoriesListViewController> viewControllerWeakReference;

            public DataSource(CategoriesListViewController categoriesListViewController, UITableView tableView)
            {
                tableViewWeakReference = tableView.Wrap();
                viewControllerWeakReference = categoriesListViewController.Wrap();

                loading = new[] { true, true };

                items = new Dictionary<nint, List<Category>>
                {
                    [Section.Selected] = new List<Category>(),
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
                if (indexPath.LongSection == Section.Selected)
                    cell.Accessory = UITableViewCellAccessory.Checkmark;

                return cell;
            }

            int MoveCategory(nint section, Category category)
            {

                if (category == null)
                    return -1;

                if (section == Section.Available)
                {
                    if (items[Section.Available].Contains(category))
                    {
                        items[Section.Available].Remove(category);
                        items[Section.Selected].Add(category);
                        items[Section.Selected].Sort((x, y) => String.Compare(x.Name, y.Name, true));
                        return items[Section.Selected].IndexOf(category);
                    }
                }

                if (section == Section.Selected)
                {
                    if (items[Section.Selected].Contains(category))
                    {
                        items[Section.Selected].Remove(category);
                        items[Section.Available].Add(category);
                        items[Section.Available].Sort((x, y) => String.Compare(x.Name, y.Name, true));
                        return items[Section.Available].IndexOf(category);
                    }
                }

                return -1;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                tableView.AllowsSelection = false; // to prevent exceptions when 'quickly' tapping rows
                tableView.BeginUpdates();
                var category = items[indexPath.LongSection][indexPath.Row];
                var newIndex = MoveCategory(indexPath.LongSection, category);
                viewControllerWeakReference.Unwrap()?.MoveCategory(category);

                // dont delete the row if it's the last element, because we are displaying "No categories" cell
                if (items[indexPath.LongSection].Count >= 1)
                {
                    tableView.DeleteRows(new NSIndexPath[] { NSIndexPath.Create(indexPath.LongSection, indexPath.Row) }, UITableViewRowAnimation.Middle);
                }

                if (indexPath.LongSection == Section.Selected)
                {
                    var availableCategories = viewControllerWeakReference.Unwrap()?.GetAvailableCategories();
                    if (items[Section.Available].Count <= 1)
                    {
                        tableView.ReloadSections(NSIndexSet.FromIndex(Section.Available), UITableViewRowAnimation.Middle);
                    }
                    else
                    {
                        tableView.InsertRows(new NSIndexPath[] { NSIndexPath.Create(Section.Available, newIndex) }, UITableViewRowAnimation.Middle);
                    }
                }
                else
                {
                    var count = items[Section.Selected].Count;
                    if (count <= 1)
                    {
                        tableView.ReloadSections(NSIndexSet.FromIndex(Section.Selected), UITableViewRowAnimation.Middle);
                    }
                    else
                    {
                        tableView.InsertRows(new NSIndexPath[] { NSIndexPath.Create(Section.Selected, newIndex) }, UITableViewRowAnimation.Middle);
                    }
                }

                tableView.EndUpdates();
                tableView.AllowsSelection = true;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading[section] || items[section].Count < 1)
                    return 1;

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
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(Section.Selected), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.EndUpdates();
            }

            public List<Category> GetCategoriesInSection(nint section) => items[section];

            public int GetItemsInSection(nint section) => items[section].Count;

            public override string TitleForHeader(UITableView tableView, nint section)
            {
                if (section == Section.Available)
                    return Localization.GetString("select_categories");

                if (section == Section.Selected)
                    return Localization.GetString("categories_added");

                return "";
            }

            public override void WillDisplayHeaderView(UITableView tableView, UIView headerView, nint section) => headerView.ApplyTheme();

        }

        class SearchDataSource : UITableViewSource
        {
            public bool Empty => items.Count < 1;
            public List<Category> Items => items.ToList();

            string searchQuery = String.Empty;

            readonly WeakReference<CategoriesListViewController> viewControllerWeakReference;
            readonly WeakReference<UITableView> tableViewWeakReference;
            readonly List<Category> items = new List<Category>();

            bool loading = true;

            public SearchDataSource(CategoriesListViewController viewController, UITableView tableView)
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
                viewControllerWeakReference.Unwrap()?.MoveCategory(category);
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
