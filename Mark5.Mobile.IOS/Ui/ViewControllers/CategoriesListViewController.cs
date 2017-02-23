//
// Project: Mark5.Mobile.IOS
// File: CategoriesListViewController.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Model.HubMessages;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class CategoriesListViewController : AbstractMainViewController, IUISearchResultsUpdating
    {
        public BusinessEntityPreview BusinessEntityPreview
        {
            get; set;
        }

        UIBarButtonItem dismissButtonItem;
        UIBarButtonItem cancelButtonItem;
        UIBarButtonItem editModeButtonItem;
        UIBarButtonItem exitEditModeButtonItem;

        UITableView categoriesListView;
        UISearchController searchController;
        UITableViewController searchResultsController;

        CancellationTokenSource searchCancellationTokenSource;
        readonly List<CancellationTokenSource> searchCancellationTokenSourceList = new List<CancellationTokenSource>();

        SearchDataSource searchDataSource;

        List<Category> availableCategories = new List<Category>();
        List<Category> assignedCategories = new List<Category>();
        List<Category> selectedCategories = new List<Category>();

        public CategoriesListViewController()
        {
            Title = Localization.GetString("categories");
        }

        #region Lifecycle overrides

        public override void LoadView()
        {
            base.LoadView();

            InitializeNavigationBar();
            InitializeCategoriesListView();
            InitializeSearchBar();
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public override async void ViewWillAppear(bool animated)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            base.ViewWillAppear(animated);

            InitializeHandlers();

            await RefreshData();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeInitializeHandlers();
        }
        #endregion

        #region Init methods

        void InitializeNavigationBar()
        {
            dismissButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Done);
            NavigationItem.SetLeftBarButtonItem(dismissButtonItem, false);

            cancelButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Cancel);

            editModeButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Edit);
            editModeButtonItem.Enabled = false;
            NavigationItem.SetRightBarButtonItem(editModeButtonItem, false);

            exitEditModeButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Done);
        }

        void InitializeCategoriesListView()
        {
            //AutomaticallyAdjustsScrollViewInsets = true; //TODO need to understand why it does not work
            EdgesForExtendedLayout = UIRectEdge.None;

            categoriesListView = new UITableView();
            var dataSource = new DataSource(categoriesListView, Localization.GetString("no_categories"), selectedCategories, availableCategories, assignedCategories);
            categoriesListView.Source = dataSource;
            categoriesListView.CellLayoutMarginsFollowReadableWidth = false;
            categoriesListView.AllowsSelection = false;
            categoriesListView.AllowsSelectionDuringEditing = true;
            categoriesListView.AllowsMultipleSelection = false;
            categoriesListView.AllowsMultipleSelectionDuringEditing = true;
            categoriesListView.TranslatesAutoresizingMaskIntoConstraints = false;
            categoriesListView.ClipsToBounds = false;
            View.AddSubview(categoriesListView);
            View.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(categoriesListView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(categoriesListView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(categoriesListView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(categoriesListView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1.0f, 0.0f),
                });
        }

        void InitializeSearchBar()
        {
            DefinesPresentationContext = true;

            searchResultsController = new UITableViewController();
            searchResultsController.TableView.CellLayoutMarginsFollowReadableWidth = false;
            searchResultsController.TableView.AllowsSelection = true;
            searchResultsController.TableView.AllowsSelectionDuringEditing = true;
            searchResultsController.TableView.AllowsMultipleSelection = false;
            searchResultsController.TableView.AllowsMultipleSelectionDuringEditing = true;
            searchDataSource = new SearchDataSource(searchResultsController.TableView, Localization.GetString("no_matching_categories"), selectedCategories, availableCategories);
            searchResultsController.TableView.Source = searchDataSource;

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
            if (dismissButtonItem != null)
                dismissButtonItem.Clicked += DismissButtonItem_Clicked;

            if (cancelButtonItem != null)
                cancelButtonItem.Clicked += CancelButtomItem_Clicked;

            if (editModeButtonItem != null)
                editModeButtonItem.Clicked += EditModeButtonItem_Clicked;

            if (exitEditModeButtonItem != null)
                exitEditModeButtonItem.Clicked += ExitEditModeButtonItem_Clicked;
        }

        void DeInitializeHandlers()
        {
            if (dismissButtonItem != null)
                dismissButtonItem.Clicked -= DismissButtonItem_Clicked;

            if (cancelButtonItem != null)
                cancelButtonItem.Clicked -= CancelButtomItem_Clicked;

            if (editModeButtonItem != null)
                editModeButtonItem.Clicked -= EditModeButtonItem_Clicked;

            if (exitEditModeButtonItem != null)
                exitEditModeButtonItem.Clicked -= ExitEditModeButtonItem_Clicked;
        }

        #endregion

        #region Refresh methods

        async Task RefreshData()
        {
            RefreshAssignedCategories();
            await RefreshAvailableCategories();
        }

        async Task RefreshAvailableCategories()
        {
            try
            {
                CommonConfig.Logger.Info($"Refresh running...");

                switch (BusinessEntityPreview.ObjectType)
                {
                    case ObjectType.Document:
                        availableCategories.AddRange(await Managers.DocumentsManager.GetAllCategoriesAsync());
                        break;
                    case ObjectType.Contact:
                        availableCategories.AddRange(await Managers.ContactsManager.GetAllCategoriesAsync());
                        break;
                    default:
                        throw new ArgumentException("The business entity provided does not have categories in the model");
                }

                editModeButtonItem.Enabled = true;
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while retrieving available categories [businessEntity.id={BusinessEntityPreview.Id}, businessEntity.objectType={BusinessEntityPreview.ObjectType}]", ex);
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
            finally
            {
                CommonConfig.Logger.Info($"Refresh finished");
            }
        }

        void RefreshAssignedCategories()
        {
            switch (BusinessEntityPreview.ObjectType)
            {
                case ObjectType.Document:
                    assignedCategories.AddRange((BusinessEntityPreview as DocumentPreview).Categories);
                    break;
                case ObjectType.Contact:
                    assignedCategories.AddRange((BusinessEntityPreview as ContactPreview).Categories);
                    break;
                default:
                    throw new ArgumentException("The business entity provided does not have categories in the model");
            }

            var ds = categoriesListView.Source as DataSource;
            ds.RefreshAssignedCategories();
            categoriesListView.ReloadData();
        }

        #endregion

        #region Event handlers

        void DismissButtonItem_Clicked(object sender, EventArgs e)
        {
            DismissViewController(true, null);
        }

        void CancelButtomItem_Clicked(object sender, EventArgs e)
        {
            UpdateAfterExitEditMode();
        }

        void EditModeButtonItem_Clicked(object sender, EventArgs e)
        {
            NavigationItem.SetLeftBarButtonItem(cancelButtonItem, true);
            NavigationItem.SetRightBarButtonItem(exitEditModeButtonItem, true);

            categoriesListView.TableHeaderView = searchController.SearchBar;

            var ds = categoriesListView.Source as DataSource;
            ds.EditingWillBegin();
            categoriesListView.SetEditing(true, true);
            searchResultsController.TableView.SetEditing(true, true);
            ds.EditingDidBegin();
        }

        async void ExitEditModeButtonItem_Clicked(object sender, EventArgs e)
        {
            var ds = categoriesListView.Source as DataSource;

            if (ds.CategoriesChanged)
            {
                CommonConfig.Logger.Info(string.Format("Categories changed - will update. [entity={0}]", BusinessEntityPreview));

                var categoriesToAssign = ds.SelectedCategories;

                var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("updating_categories___"));

                try
                {
                    switch (BusinessEntityPreview.ObjectType)
                    {
                        case ObjectType.Document:
                            var documentPreview = BusinessEntityPreview as DocumentPreview;
                            await Managers.DocumentsManager.SetCategoriesAsync(documentPreview, categoriesToAssign.ToList());
                            PlatformConfig.MessengerHub.Publish(new EntityCategoriesChangedMessage(this, documentPreview.Id, ObjectType.Document, documentPreview.Categories)); //TODO probably we don't need categories
                            break;
                        case ObjectType.Contact:
                            var contactPreview = BusinessEntityPreview as ContactPreview;
                            await Managers.ContactsManager.SetCategoriesAsync(contactPreview, categoriesToAssign.ToList());
                            PlatformConfig.MessengerHub.Publish(new EntityCategoriesChangedMessage(this, contactPreview.Id, ObjectType.Contact, contactPreview.Categories)); //TODO probably we don't need categories
                            break;
                        default:
                            throw new ArgumentException("Invalid BusinessEntityPreview!");
                    }

                    RefreshAssignedCategories();
                    UpdateAfterExitEditMode();
                }
                catch (Exception ex)
                {
                    dismissAction();
                    CommonConfig.Logger.Error($"Error while updating categories [entity={BusinessEntityPreview}]", ex);
                    await Dialogs.ShowErrorDialogAsync(this, ex);
                }
            }
            else
            {
                CommonConfig.Logger.Info(string.Format($"Categories did not change - exiting edit mode. [entity={BusinessEntityPreview}]"));

                UpdateAfterExitEditMode();
            }
        }

        void UpdateAfterExitEditMode()
        {
            NavigationItem.SetLeftBarButtonItem(dismissButtonItem, true);
            NavigationItem.SetRightBarButtonItem(editModeButtonItem, true);

            categoriesListView.TableHeaderView = null;

            var ds = categoriesListView.Source as DataSource;
            ds.EditingWillEnd();
            categoriesListView.SetEditing(false, true);
            searchResultsController.TableView.SetEditing(false, true);
            ds.EditingDidEnd();
        }

        #endregion

        #region Search Results

        public void UpdateSearchResultsForSearchController(UISearchController searchController)
        {
            var searchText = searchController.SearchBar.Text;

            if (!searchController.Active || string.IsNullOrWhiteSpace(searchText))
            {
                searchCancellationTokenSourceList.ForEach(t => t?.Cancel());
                searchCancellationTokenSourceList.Clear();
                searchDataSource.Reset();
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

                DoSearchCategory(searchText, searchCancellationTokenSource);
            }
        }

        async void DoSearchCategory(string searchText, CancellationTokenSource ct)
        {
            searchDataSource.Reset();

            await Task.Delay(100);

            if (ct.IsCancellationRequested) return;

            searchDataSource.RefreshData(searchText);
            searchResultsController.TableView.ReloadData();
        }

        #endregion

        class DataSource : UITableViewSource
        {
            public bool CategoriesChanged
            {
                get;
                private set;
            }

            public bool Empty
            {
                get
                {
                    return !sectionIndexes.Any() || !categoriesInView.Any(); //TODO wht this?
                }
            }

            public List<Category> SelectedCategories
            {
                get
                {
                    return selectedCategories.ToList();
                }
            }

            readonly UITableView tableView;

            List<string> sectionIndexes;
            Dictionary<string, List<Category>> categoriesInView;

            readonly List<Category> assignedCategories;
            List<Category> availableCategories;
            readonly List<Category> selectedCategories;
            readonly string emptyText;

            public DataSource(UITableView tableView, string emptyText, List<Category> selectedCategories, List<Category> availableCategories, List<Category> assignedCategories)
            {
                this.tableView = tableView;
                this.emptyText = emptyText;

                sectionIndexes = new List<string>();
                categoriesInView = new Dictionary<string, List<Category>>();
                this.assignedCategories = assignedCategories;
                this.availableCategories = availableCategories;
                this.selectedCategories = selectedCategories;
            }

            #region UITableViewDataSource implementation

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (Empty)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                    emptyCell.Initialize(emptyText);
                    return emptyCell;
                }

                var cell = tableView.DequeueReusableCell(CategoriesTableViewCell.Key) as CategoriesTableViewCell ?? CategoriesTableViewCell.Create();
                cell.Initialize(categoriesInView[sectionIndexes[indexPath.Section]][indexPath.Row]);
                return cell;
            }

            public override void WillDisplay(UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
            {
                if (tableView.Editing)
                {
                    var categoryListViewCell = cell as CategoriesTableViewCell;
                    if (categoryListViewCell != null)
                    {
                        if (selectedCategories.Contains(categoryListViewCell.Category))
                        {
                            tableView.SelectRow(indexPath, false, UITableViewScrollPosition.None);
                        }
                        else
                        {
                            tableView.DeselectRow(indexPath, false);
                        }
                    }
                }
                else
                {
                    tableView.DeselectRow(indexPath, false);
                }
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                return Empty ? 1 : sectionIndexes.Count;

            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return Empty ? 1 : categoriesInView[sectionIndexes[(int)section]].Count;

            }

            public override string[] SectionIndexTitles(UITableView tableView)
            {
                if (Empty)
                {
                    return new[] { string.Empty };
                }

                return sectionIndexes.Count < 5 ? null : sectionIndexes.ToArray();
            }

            public override string TitleForHeader(UITableView tableView, nint section)
            {
                return Empty ? string.Empty : sectionIndexes[(int)section];
            }

            public override nint SectionFor(UITableView tableView, string title, nint atIndex)
            {
                return atIndex;
            }

            public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
            {
                return true;
            }

            #endregion

            #region UITableViewDelegate implementation

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return 44.0f;
            }

            public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return UITableViewCellEditingStyle.None;
            }

            public override NSIndexPath WillSelectRow(UITableView tableView, NSIndexPath indexPath)
            {
                CategoriesChanged |= tableView.Editing;

                return tableView.Editing ? indexPath : null;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                if (Empty)
                {
                    return;
                }

                var cell = tableView.CellAt(indexPath) as CategoriesTableViewCell;
                if (cell != null)
                {
                    selectedCategories.Add(cell.Category);
                }
            }

            public override NSIndexPath WillDeselectRow(UITableView tableView, NSIndexPath indexPath)
            {
                CategoriesChanged |= tableView.Editing;

                return tableView.Editing ? indexPath : null;
            }

            public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.CellAt(indexPath) as CategoriesTableViewCell;
                if (cell != null)
                {
                    selectedCategories.Remove(cell.Category);
                }
            }

            // This method is implemented only to workaround a bug in Xamarin.iOS or iOS 8.1.2 on
            // iPhone 6 and 6 Plus related to AppeareanceWhenContainedIn method. The rest is described
            // in Theme.cs file.
            public override void WillDisplayHeaderView(UITableView tableView, UIView headerView, nint section)
            {
                var tableViewHeaderFooterView = headerView as UITableViewHeaderFooterView;
                if (tableViewHeaderFooterView != null)
                {
                    tableViewHeaderFooterView.TextLabel.TextColor = Theme.TintColor;
                }
            }

            #endregion

            public void RefreshAssignedCategories()
            {
                categoriesInView = assignedCategories.GroupBy(c => c.Name.SafeSubstring(0, 1).ToUpper(CultureInfo.CurrentCulture)).ToDictionary(g => g.Key, g => g.OrderBy(c => c.Name).ToList(), StringComparer.OrdinalIgnoreCase);
                sectionIndexes = categoriesInView.Keys.OrderBy(i => i).ToList();
            }

            public void EditingWillBegin()
            {
                sectionIndexes.Clear();
                categoriesInView.Clear();

                categoriesInView = availableCategories.GroupBy(c => c.Name.SafeSubstring(0, 1).ToUpper(CultureInfo.CurrentCulture)).ToDictionary(g => g.Key, g => g.OrderBy(c => c.Name).ToList(), StringComparer.OrdinalIgnoreCase);
                sectionIndexes = categoriesInView.Keys.OrderBy(i => i).ToList();

                UIView.TransitionNotify(tableView, 0.25d, UIViewAnimationOptions.TransitionCrossDissolve, tableView.ReloadData, null);
            }

            public void EditingDidBegin()
            {
                selectedCategories.AddRange(assignedCategories);

                for (var i = 0; i < sectionIndexes.Count; i++)
                {
                    for (var j = 0; j < categoriesInView[sectionIndexes[i]].Count; j++)
                    {
                        var indexPath = NSIndexPath.FromRowSection(j, i);
                        var cell = tableView.CellAt(indexPath) as CategoriesTableViewCell;
                        if (cell != null && selectedCategories.Contains(cell.Category))
                        {
                            tableView.SelectRow(indexPath, false, UITableViewScrollPosition.None);
                        }
                    }
                }
            }

            public void EditingWillEnd()
            {
                sectionIndexes.Clear();
                categoriesInView.Clear();

                foreach (var category in assignedCategories)
                {
                    var key = category.Name.SafeSubstring(0, 1).ToUpper(CultureInfo.CurrentCulture);
                    if (categoriesInView.ContainsKey(key))
                    {
                        categoriesInView[key].Add(category);
                    }
                    else
                    {
                        var categories = new List<Category>();
                        categories.Add(category);
                        categoriesInView.Add(key, categories);
                    }
                }

                sectionIndexes = categoriesInView.Keys.OrderBy(i => i).ToList();

                UIView.TransitionNotify(tableView, 0.25d, UIViewAnimationOptions.TransitionCrossDissolve, tableView.ReloadData, null);
            }

            public void EditingDidEnd()
            {
                selectedCategories.Clear();

                foreach (var indexPath in tableView.IndexPathsForVisibleRows)
                {
                    tableView.DeselectRow(indexPath, false);
                }

                CategoriesChanged = false;
            }

            //public NSIndexPath IndexPathForCategory(Category category) //TODO check if necessary
            //{
            //    var index = 0;
            //    var section = 0;

            //    foreach (var entry in categoriesInView)
            //    {
            //        if (entry.Value.Contains(category))
            //        {
            //            index = entry.Value.IndexOf(category);
            //            section = sectionIndexes.IndexOf(entry.Key);
            //        }
            //    }

            //    return NSIndexPath.FromItemSection(index, section);
            //}

            //public void SelectCategory(Category category) //TODO check if they are used
            //{
            //    CategoriesChanged |= tableView.Editing;
            //    tableView.SelectRow(IndexPathForCategory(category), true, UITableViewScrollPosition.None);
            //    selectedCategories.Add(category);
            //}

            //public void DeselectCategory(Category category)
            //{
            //    CategoriesChanged |= tableView.Editing;
            //    tableView.DeselectRow(IndexPathForCategory(category), true);
            //    selectedCategories.Remove(category);
            //}

        }

        class SearchDataSource : UITableViewSource
        {
            public bool Empty
            {
                get
                {
                    return !sectionIndexes.Any() || !categoriesInView.Any();
                }
            }

            readonly UITableView tableView;

            List<string> sectionIndexes;
            Dictionary<string, List<Category>> categoriesInView;

            List<Category> selectedCategories;
            List<Category> availableCategories;
            readonly string emptyText;

            public SearchDataSource(UITableView tableView, string emptyText, List<Category> selectedCategories, List<Category> availableCategories)
            {
                this.tableView = tableView;
                this.emptyText = emptyText;
                this.selectedCategories = selectedCategories;
                this.availableCategories = availableCategories;

                sectionIndexes = new List<string>();
                categoriesInView = new Dictionary<string, List<Category>>();
            }

            #region UITableViewDataSource implementation

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (Empty)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                    emptyCell.Initialize(emptyText);
                    return emptyCell;
                }

                var cell = tableView.DequeueReusableCell(CategoriesTableViewCell.Key) as CategoriesTableViewCell ?? CategoriesTableViewCell.Create();
                cell.Initialize(categoriesInView[sectionIndexes[indexPath.Section]][indexPath.Row]);
                return cell;
            }

            public override void WillDisplay(UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
            {
                if (tableView.Editing)
                {
                    var categoryListViewCell = cell as CategoriesTableViewCell;
                    if (categoryListViewCell != null)
                    {
                        if (selectedCategories.FindIndex(c => c.Id == categoryListViewCell.Category.Id) >= 0)
                        {
                            tableView.SelectRow(indexPath, false, UITableViewScrollPosition.None);
                        }
                        else
                        {
                            tableView.DeselectRow(indexPath, false);
                        }
                    }
                }
                else
                {
                    tableView.DeselectRow(indexPath, false);
                }
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                return Empty ? 1 : sectionIndexes.Count;

            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return Empty ? 1 : categoriesInView[sectionIndexes[(int)section]].Count;

            }

            public override string[] SectionIndexTitles(UITableView tableView)
            {
                if (Empty)
                {
                    return new[] { string.Empty };
                }

                return sectionIndexes.Count < 5 ? null : sectionIndexes.ToArray();
            }

            public override string TitleForHeader(UITableView tableView, nint section)
            {
                return Empty ? string.Empty : sectionIndexes[(int)section];
            }

            public override nint SectionFor(UITableView tableView, string title, nint atIndex)
            {
                return atIndex;
            }

            public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
            {
                return !Empty;
            }

            #endregion

            #region UITableViewDelegate implementation

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return 44.0f;
            }

            public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return UITableViewCellEditingStyle.None;
            }

            public override NSIndexPath WillSelectRow(UITableView tableView, NSIndexPath indexPath)
            {
                return tableView.Editing ? indexPath : null;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                if (Empty)
                {
                    return;
                }

                var cell = tableView.CellAt(indexPath) as CategoriesTableViewCell;
                if (cell != null)
                {
                    //selectedCategories.Add(cell.Category); //TODO
                }
            }

            public override NSIndexPath WillDeselectRow(UITableView tableView, NSIndexPath indexPath)
            {
                return tableView.Editing ? indexPath : null;
            }

            public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.CellAt(indexPath) as CategoriesTableViewCell;
                if (cell != null)
                {
                    //selectedCategories.Remove(cell.Category); //TODO
                }
            }

            // This method is implemented only to workaround a bug in Xamarin.iOS or iOS 8.1.2 on
            // iPhone 6 and 6 Plus related to AppeareanceWhenContainedIn method. The rest is described
            // in Theme.cs file.
            public override void WillDisplayHeaderView(UITableView tableView, UIView headerView, nint section)
            {
                var tableViewHeaderFooterView = headerView as UITableViewHeaderFooterView;
                if (tableViewHeaderFooterView != null)
                {
                    tableViewHeaderFooterView.TextLabel.TextColor = Theme.TintColor;
                }
            }

            #endregion

            #region Public methods

            public void RefreshData(string searchText)
            {
                var matchingCategories = availableCategories.FindAll(c => MatchesQuery(c, searchText));
                var assignedCategoriesId = new HashSet<int>(selectedCategories.Select(c => c.Id));

                sectionIndexes = matchingCategories.Select(c => c.Name.SafeSubstring(0, 1).ToUpper(CultureInfo.CurrentCulture)).Distinct().OrderBy(i => i).ToList();
                categoriesInView = matchingCategories.GroupBy(c => c.Name.SafeSubstring(0, 1).ToUpper(CultureInfo.CurrentCulture))
                                                     .ToDictionary(g => g.Key, g => g.OrderBy(c => c.Name).ToList(), StringComparer.OrdinalIgnoreCase);
            }

            public void Reset()
            {
                sectionIndexes.Clear();
                categoriesInView.Clear();
            }

            #endregion

            #region Utility

            bool MatchesQuery(Category category, string query)
            {
                return category.Name.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0;
            }

            #endregion
        }

        public class CategoryComparer : IEqualityComparer<Category>
        {
            public bool Equals(Category x, Category y)
            {
                return x.Id == y.Id;
            }

            public int GetHashCode(Category obj)
            {
                return obj.Id.GetHashCode();
            }
        }

    }
}
