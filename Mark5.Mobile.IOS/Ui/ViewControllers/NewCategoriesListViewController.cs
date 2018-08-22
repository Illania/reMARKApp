using UIKit;
using Foundation;
using System;
using System.Linq;
using System.Threading;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using Mark5.Mobile.IOS.Utilities;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class NewCategoriesListViewController : AbstractTableViewController, IUISearchResultsUpdating
    {
        BusinessEntityPreview BusinessEntityPreview { get; set; }
        List<Category> categories = new List<Category>();

        UIBarButtonItem cancelBtnItem;
        UIBarButtonItem saveBtnItem;
        UISearchController searchController;

        CancellationTokenSource searchCancellationTokenSource;
        readonly List<CancellationTokenSource> searchCancellationTokenSourceList = new List<CancellationTokenSource>();

        public override void LoadView()
        {
            base.LoadView();

            if (BusinessEntityPreview != null)
                CommonConfig.UsageAnalytics.LogEvent(new OpenCategoriesEvent(BusinessEntityPreview.ModuleType));

            InitializeView();
            InitializeNavigationBar();
            InitializeSearchBar();
        }

        public NewCategoriesListViewController(BusinessEntityPreview businessEntityPreview)
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

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (Integration.IsRunningAtLeast(11))
            {
                if (NavigationController != null)
                    NavigationController.NavigationBar.PrefersLargeTitles = true;
                NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Automatic;
            }
        }

        public override async void ViewDidAppear(bool animated)
        {
            if (((DataSource)TableView.Source).Empty)
                await RefreshDataAsync();

            if (Integration.IsRunningAtLeast(11))
            {
                NavigationItem.SearchController = searchController;
            }
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

        void InitializeView()
        {
            TableView.Source = new DataSource(this, TableView);
            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 40f;
            TableView.AllowsSelection = true;
        }

        public async Task RefreshDataAsync()
        {
            if (BusinessEntityPreview is DocumentPreview documentPreview)
            {
                ((DataSource)TableView.Source).SetItems(DataSource.Section.Selected, categories);
                var allAvailableCategories = await Managers.DocumentsManager.GetAllCategoriesAsync();

                var availableCategories = allAvailableCategories.Where(x => !categories.Any(y => y.Guid == x.Guid)).ToList();
                ((DataSource)TableView.Source).SetItems(DataSource.Section.Available, availableCategories);
            }

            if (BusinessEntityPreview is ContactPreview contactPreview)
            {
                ((DataSource)TableView.Source).SetItems(DataSource.Section.Selected, categories);
                var allAvailableCategories = await Managers.ContactsManager.GetAllCategoriesAsync();
                var availableCategories = allAvailableCategories.Where(x => !categories.Contains(x)).ToList();
                ((DataSource)TableView.Source).SetItems(DataSource.Section.Available, availableCategories);
            }

            if(searchController != null)
                searchController.Active = false;
        }

        protected virtual void CategorySelected(Category category)
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

            #pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            RefreshDataAsync();
            #pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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

                if (BusinessEntityPreview is DocumentPreview documentPreview)
                {
                    ((DataSource)TableView.Source).SetItems(DataSource.Section.Selected, documentPreview.Categories);
                    var allAvailableCategories = await Managers.DocumentsManager.GetAllCategoriesAsync();
                    searchResultCategories = allAvailableCategories.Where(x => x.Name.Contains(searchText) && !categories.Any(y => y.Guid == x.Guid)).ToList();
                }

                if (BusinessEntityPreview is ContactPreview contactPreview)
                {
                    ((DataSource)TableView.Source).SetItems(DataSource.Section.Selected, contactPreview.Categories);
                    var allAvailableCategories = await Managers.ContactsManager.GetAllCategoriesAsync();
                    searchResultCategories = allAvailableCategories.Where(x => x.Name.Contains(searchText) && !categories.Any(y => y.Guid == x.Guid)).ToList();
                }

                if (cancellationToken.IsCancellationRequested)
                    return;

                dataSource?.SetSearchCategories(searchResultCategories, searchText);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error(ex);
            }
        }

        protected class SearchDataSource : UITableViewSource
        {
            public bool Empty => items.Count < 1;
            public List<Category> Items => items.ToList();

            string searchQuery = String.Empty;

            readonly WeakReference<NewCategoriesListViewController> viewControllerWeakReference;
            readonly WeakReference<UITableView> tableViewWeakReference;
            readonly List<Category> items = new List<Category>();

            bool loading = true;

            public SearchDataSource(NewCategoriesListViewController viewController, UITableView tableView)
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
                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return loading || Empty ? 1 : (nint)items.Count;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var category = items[indexPath.Row];
                viewControllerWeakReference.Unwrap()?.CategorySelected(category);
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

        #region Navigation bar related
        void InitializeNavigationBar()
        {
            Title = Localization.GetString("categories");
            cancelBtnItem = new UIBarButtonItem(UIBarButtonSystemItem.Cancel, CancelBtnItem_Clicked);
            NavigationItem.SetLeftBarButtonItem(cancelBtnItem, false);

            saveBtnItem = new UIBarButtonItem(UIBarButtonSystemItem.Save, SaveBtnItem_Clicked);

            NavigationItem.SetRightBarButtonItem(saveBtnItem, false);
        }

        void CancelBtnItem_Clicked(object sender, EventArgs e)
        {
            if (BusinessEntityPreview is DocumentPreview documentPreview && categories.Equals(documentPreview.Categories) || BusinessEntityPreview is ContactPreview contactPreview && categories.Equals(contactPreview.Categories))
            {
                DismissViewController(true, null);
            }
            else
            {
                var alertController = UIAlertController.Create(Localization.GetString("changes_not_saved"), Localization.GetString("changes_not_saved_description"), UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create(Localization.GetString("ok"), UIAlertActionStyle.Default, x => DismissViewController(true, null)));
                alertController.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, null));
                PresentViewController(alertController, true, null);
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
            readonly WeakReference<NewCategoriesListViewController> viewControllerWeakReference;

            public DataSource(NewCategoriesListViewController newCategoriesListViewController, UITableView tableView)
            {
                tableViewWeakReference = tableView.Wrap();
                viewControllerWeakReference = newCategoriesListViewController.Wrap();

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
                return cell;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var selectedCategory = items[indexPath.LongSection][indexPath.Row];
                viewControllerWeakReference.Unwrap()?.CategorySelected(selectedCategory);
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

            public void Reload()
            {
                tableViewWeakReference.Unwrap()?.BeginUpdates();
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(Section.Available), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(Section.Selected), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.EndUpdates();
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
                    return Localization.GetString("available");

                if (section == Section.Selected)
                    return Localization.GetString("selected");

                return "";
            }

            public override void WillDisplayHeaderView(UITableView tableView, UIView headerView, nint section) => headerView.ApplyTheme();

        }
        #endregion
    }
}
