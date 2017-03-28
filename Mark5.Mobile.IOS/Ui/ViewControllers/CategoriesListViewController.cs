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

    public class CategoriesListViewController : AbstractViewController, IUISearchResultsUpdating
    {

        public BusinessEntityPreview BusinessEntityPreview { get; set; }

        UIBarButtonItem dismissButtonItem;
        UIBarButtonItem cancelButtonItem;
        UIBarButtonItem editModeButtonItem;
        UIBarButtonItem exitEditModeButtonItem;

        UITableView tableView;
        UISearchController searchController;
        UITableViewController searchResultsController;

        CancellationTokenSource searchCancellationTokenSource;
        readonly List<CancellationTokenSource> searchCancellationTokenSourceList = new List<CancellationTokenSource>();

        SearchDataSource searchDataSource;
        DataSource dataSource;

        List<Category> availableCategories = new List<Category>();

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

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ExtendedLayoutIncludesOpaqueBars = true;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            InitializeHandlers();
            
            ReachabilityBar.Attach(View, tableView, (float)NavigationController.BottomLayoutGuide.Length);
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public override async void ViewDidAppear(bool animated)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            base.ViewDidAppear(animated);

            var ds = (DataSource)tableView.Source;
            if (ds.Empty)
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
            AutomaticallyAdjustsScrollViewInsets = true;

            tableView = new UITableView();
            tableView.Source = dataSource = new DataSource(tableView, Localization.GetString("no_categories")); ;
            tableView.AllowsSelection = false;
            tableView.AllowsSelectionDuringEditing = true;
            tableView.AllowsMultipleSelection = false;
            tableView.AllowsMultipleSelectionDuringEditing = true;
            tableView.ClipsToBounds = false;
            tableView.TranslatesAutoresizingMaskIntoConstraints = false;
            View.AddSubview(tableView);
            View.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1f, 0f),
                    NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1f, 0f),
                    NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1f, 0f),
                    NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1f, 0f),
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
            searchResultsController.TableView.Source = searchDataSource = new SearchDataSource(Localization.GetString("no_matching_categories"), this);

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
                        availableCategories = await Managers.DocumentsManager.GetAllCategoriesAsync();
                        break;
                    case ObjectType.Contact:
                        availableCategories = await Managers.ContactsManager.GetAllCategoriesAsync();
                        break;
                    default:
                        throw new ArgumentException("The business entity provided does not have categories in the model");
                }

                editModeButtonItem.Enabled = true;
                dataSource.RefreshAvailableCategories(availableCategories);
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
            List<Category> assignedCategories;

            switch (BusinessEntityPreview.ObjectType)
            {
                case ObjectType.Document:
                    assignedCategories = (BusinessEntityPreview as DocumentPreview).Categories;
                    break;
                case ObjectType.Contact:
                    assignedCategories = (BusinessEntityPreview as ContactPreview).Categories;
                    break;
                default:
                    throw new ArgumentException("The business entity provided does not have categories in the model");
            }

            dataSource.RefreshAssignedCategories(assignedCategories);
            tableView.ReloadData();
        }

        #endregion

        #region Event handlers

        void DismissButtonItem_Clicked(object sender, EventArgs e) => DismissViewController(true, null);

        void CancelButtomItem_Clicked(object sender, EventArgs e) => UpdateAfterExitEditMode();

        void EditModeButtonItem_Clicked(object sender, EventArgs e)
        {
            NavigationItem.SetLeftBarButtonItem(cancelButtonItem, true);
            NavigationItem.SetRightBarButtonItem(exitEditModeButtonItem, true);

            tableView.TableHeaderView = searchController.SearchBar;

            dataSource.EditingWillBegin();
            tableView.SetEditing(true, true);
            searchResultsController.TableView.SetEditing(true, true);
        }

        async void ExitEditModeButtonItem_Clicked(object sender, EventArgs e)
        {
            if (dataSource.CategoriesChanged)
            {
                CommonConfig.Logger.Info(string.Format("Categories changed - will update. [entity={0}]", BusinessEntityPreview));

                var categoriesToAssign = dataSource.SelectedCategories;

                var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("updating_categories___"));

                try
                {
                    switch (BusinessEntityPreview.ObjectType)
                    {
                        case ObjectType.Document:
                            var documentPreview = BusinessEntityPreview as DocumentPreview;
                            await Managers.DocumentsManager.SetCategoriesAsync(documentPreview, categoriesToAssign);
                            PlatformConfig.MessengerHub.Publish(new EntityCategoriesChangedMessage(this, documentPreview.Id, ObjectType.Document, categoriesToAssign));
                            break;
                        case ObjectType.Contact:
                            var contactPreview = BusinessEntityPreview as ContactPreview;
                            await Managers.ContactsManager.SetCategoriesAsync(contactPreview, categoriesToAssign);
                            PlatformConfig.MessengerHub.Publish(new EntityCategoriesChangedMessage(this, contactPreview.Id, ObjectType.Contact, categoriesToAssign));
                            break;
                        default:
                            throw new ArgumentException("Invalid BusinessEntityPreview!");
                    }

                    dataSource.RefreshAssignedCategories(categoriesToAssign);
                    tableView.ReloadData();

                    UpdateAfterExitEditMode();

                    dismissAction();
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error($"Error while updating categories [entity={BusinessEntityPreview}]", ex);
                    
                    dismissAction();

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

            tableView.TableHeaderView = null;

            dataSource.EditingWillEnd();
            tableView.SetEditing(false, true);
            searchResultsController.TableView.SetEditing(false, true);
        }

        #endregion

        #region Search 

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

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void        async void DoSearchCategory(string searchText, CancellationTokenSource ct)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            searchDataSource.Reset();

            await Task.Delay(100);

            if (ct.IsCancellationRequested) return;

            var matchingCategories = availableCategories.FindAll(c => MatchesQuery(c, searchText));

            searchDataSource.RefreshData(matchingCategories, dataSource.SelectedCategories);
            searchResultsController.TableView.ReloadData();
        }

        bool MatchesQuery(Category category, string query)
        {
            return category.Name.ContainsCaseInsensitive(query);
        }

        public void SearchCategorySelected(Category category) => dataSource.SelectCategory(category);

        public void SearchCategoryDeselected(Category category) => dataSource.DeselectCategory(category);

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
                    return !sectionIndexes.Any() || !categoriesInView.Any();
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

            public DataSource(UITableView tableView, string emptyText)
            {
                this.tableView = tableView;
                this.emptyText = emptyText;

                sectionIndexes = new List<string>();
                categoriesInView = new Dictionary<string, List<Category>>();
                assignedCategories = new List<Category>();
                availableCategories = new List<Category>();
                selectedCategories = new List<Category>();
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

            public override void WillDisplayHeaderView(UITableView tableView, UIView headerView, nint section)
            {
                var v = headerView as UITableViewHeaderFooterView;
                if (v == null)
                    return;

                v.TextLabel.TextColor = Theme.DarkerBlue;
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
                return 44f;
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
                    selectedCategories.RemoveAll(c => c.Id == cell.Category.Id);
                }
            }

            #endregion

            #region Public methods

            public void RefreshAssignedCategories(List<Category> categories)
            {
                if (categories != null)
                {
                    assignedCategories.Clear();
                    assignedCategories.AddRange(categories);
                }

                categoriesInView = assignedCategories.GroupBy(c => c.Name.SafeSubstring(0, 1).ToUpper(CultureInfo.CurrentCulture)).ToDictionary(g => g.Key, g => g.OrderBy(c => c.Name).ToList(), StringComparer.OrdinalIgnoreCase);
                sectionIndexes = categoriesInView.Keys.OrderBy(i => i).ToList();
            }

            public void RefreshAvailableCategories(List<Category> categories)
            {
                if (categories != null)
                {
                    availableCategories.Clear();
                    availableCategories.AddRange(categories);
                }
            }

            public void EditingWillBegin()
            {
                sectionIndexes.Clear();
                categoriesInView.Clear();

                categoriesInView = availableCategories.GroupBy(c => c.Name.SafeSubstring(0, 1).ToUpper(CultureInfo.CurrentCulture)).ToDictionary(g => g.Key, g => g.OrderBy(c => c.Name).ToList(), StringComparer.OrdinalIgnoreCase);
                sectionIndexes = categoriesInView.Keys.OrderBy(i => i).ToList();

                UIView.TransitionNotify(tableView, 0.25d, UIViewAnimationOptions.TransitionCrossDissolve, tableView.ReloadData, null);

                selectedCategories.AddRange(assignedCategories);

            }

            public void EditingWillEnd()
            {
                sectionIndexes.Clear();
                categoriesInView.Clear();

                categoriesInView = assignedCategories.GroupBy(c => c.Name.SafeSubstring(0, 1).ToUpper(CultureInfo.CurrentCulture)).ToDictionary(g => g.Key, g => g.OrderBy(c => c.Name).ToList(), StringComparer.OrdinalIgnoreCase);
                sectionIndexes = categoriesInView.Keys.OrderBy(i => i).ToList();

                UIView.TransitionNotify(tableView, 0.25d, UIViewAnimationOptions.TransitionCrossDissolve, tableView.ReloadData, null);

                selectedCategories.Clear();

                foreach (var indexPath in tableView.IndexPathsForVisibleRows)
                {
                    tableView.DeselectRow(indexPath, false);
                }

                CategoriesChanged = false;
            }

            public NSIndexPath IndexPathForCategory(Category category)
            {
                var index = 0;
                var section = 0;

                foreach (var keyValuePair in categoriesInView)
                {
                    index = keyValuePair.Value.FindIndex(c => c.Id == category.Id);
                    if (index >= 0)
                    {
                        section = sectionIndexes.IndexOf(keyValuePair.Key);
                        return NSIndexPath.FromItemSection(index, section);
                    }
                }

                return null;
            }

            public void SelectCategory(Category category)
            {
                var categoryIndexPath = IndexPathForCategory(category);
                if (categoryIndexPath != null)
                {
                    CategoriesChanged |= tableView.Editing;
                    tableView.SelectRow(categoryIndexPath, true, UITableViewScrollPosition.None);
                    selectedCategories.Add(category);
                }
            }

            public void DeselectCategory(Category category)
            {
                var categoryIndexPath = IndexPathForCategory(category);
                if (categoryIndexPath != null)
                {
                    CategoriesChanged |= tableView.Editing;
                    tableView.DeselectRow(categoryIndexPath, true);
                    selectedCategories.RemoveAll(c => c.Id == category.Id);
                }
            }

            #endregion
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

            List<string> sectionIndexes;
            Dictionary<string, List<Category>> categoriesInView;

            List<Category> selectedCategories;

            readonly string emptyText;
            readonly CategoriesListViewController vc;

            public SearchDataSource(string emptyText, CategoriesListViewController vc)
            {
                this.emptyText = emptyText;
                this.vc = vc;

                selectedCategories = new List<Category>();
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
                var categoryListViewCell = cell as CategoriesTableViewCell;
                if (categoryListViewCell != null)
                {
                    if (selectedCategories.FindIndex(c => categoryListViewCell.Category.Id == c.Id) >= 0)
                    {
                        tableView.SelectRow(indexPath, false, UITableViewScrollPosition.None);
                    }
                    else
                    {
                        tableView.DeselectRow(indexPath, false);
                    }
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

            public override void WillDisplayHeaderView(UITableView tableView, UIView headerView, nint section)
            {
                var v = headerView as UITableViewHeaderFooterView;
                if (v == null)
                    return;

                v.TextLabel.TextColor = Theme.DarkerBlue;
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
                return 44f;
            }

            public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return UITableViewCellEditingStyle.None;
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
                    vc.SearchCategorySelected(cell.Category);
                }
            }

            public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.CellAt(indexPath) as CategoriesTableViewCell;
                if (cell != null)
                {
                    selectedCategories.Remove(cell.Category);
                    vc.SearchCategoryDeselected(cell.Category);
                }
            }

            #endregion

            #region Public methods

            public void RefreshData(List<Category> filteredCategories, List<Category> selectedCategories)
            {
                this.selectedCategories = selectedCategories;

                sectionIndexes = filteredCategories.Select(c => c.Name.SafeSubstring(0, 1).ToUpper(CultureInfo.CurrentCulture)).Distinct().OrderBy(i => i).ToList();
                categoriesInView = filteredCategories.GroupBy(c => c.Name.SafeSubstring(0, 1).ToUpper(CultureInfo.CurrentCulture))
                                                     .ToDictionary(g => g.Key, g => g.OrderBy(c => c.Name).ToList(), StringComparer.OrdinalIgnoreCase);
            }

            public void Reset()
            {
                sectionIndexes.Clear();
                categoriesInView.Clear();
                selectedCategories.Clear();
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
