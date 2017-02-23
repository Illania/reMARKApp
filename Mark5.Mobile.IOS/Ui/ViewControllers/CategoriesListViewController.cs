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
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class CategoriesListViewController : AbstractMainViewController
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

        public CategoriesListViewController()
        {
            Title = Localization.GetString("categories");
        }

        #region Lifecycle overrides

        public override void LoadView()
        {
            base.LoadView();

            InitNavigationBar();
            InitCategoriesListView();
            InitSearchBar();
        }

        public override async void ViewWillAppear(bool animated)
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

        void InitNavigationBar()
        {
            dismissButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Done);
            NavigationItem.SetLeftBarButtonItem(dismissButtonItem, false);

            cancelButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Cancel);

            editModeButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Edit);
            editModeButtonItem.Enabled = false;
            NavigationItem.SetRightBarButtonItem(editModeButtonItem, false);

            exitEditModeButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Done);
        }

        void InitCategoriesListView()
        {
            AutomaticallyAdjustsScrollViewInsets = true;

            categoriesListView = new UITableView();
            var dataSource = new DataSource(categoriesListView, ""); //TODO correct
            categoriesListView.Source = dataSource;
            categoriesListView.CellLayoutMarginsFollowReadableWidth = false;
            categoriesListView.AllowsSelection = false;
            categoriesListView.AllowsSelectionDuringEditing = true;
            categoriesListView.AllowsMultipleSelection = false;
            categoriesListView.AllowsMultipleSelectionDuringEditing = true;
            categoriesListView.TranslatesAutoresizingMaskIntoConstraints = false;
            View.AddSubview(categoriesListView);
            View.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(categoriesListView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(categoriesListView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(categoriesListView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(categoriesListView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1.0f, 0.0f),
                });
        }

        void InitSearchBar()
        {
            DefinesPresentationContext = true;

            searchResultsController = new UITableViewController();
            searchResultsController.TableView.CellLayoutMarginsFollowReadableWidth = false;
            searchResultsController.TableView.AllowsSelection = true;
            searchResultsController.TableView.AllowsSelectionDuringEditing = true;
            searchResultsController.TableView.AllowsMultipleSelection = false;
            searchResultsController.TableView.AllowsMultipleSelectionDuringEditing = true;
            //categoriesListSearchViewSource = new CategoriesListSearchViewSource(this, searchResultsController.TableView, categoriesListViewSource);
            //searchResultsController.TableView.Source = categoriesListSearchViewSource; //TODO

            searchController = new UISearchController(searchResultsController)
            {
                HidesNavigationBarDuringPresentation = true,
                DimsBackgroundDuringPresentation = true,
                ObscuresBackgroundDuringPresentation = true,
                //SearchResultsUpdater = new CategoriesListSearchResultsUpdater(categoriesListSearchViewSource), //TODO
            };
            searchController.SearchBar.Placeholder = "Filter";

            categoriesListView.TableHeaderView = searchController.SearchBar;
        }

        void InitializeHandlers()
        {
            //TODO finish
            dismissButtonItem.Clicked += DismissButtonItem_Clicked;
            cancelButtonItem.Clicked += CancelButtomItem_Clicked;
            editModeButtonItem.Clicked += EditModeButtonItem_Clicked;
            exitEditModeButtonItem.Clicked += ExitEditModeButtonItem_Clicked;
        }

        void DeInitializeHandlers()
        { }

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

                List<Category> availableCategories;

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

                var ds = categoriesListView.Source as DataSource;
                ds.RefreshData(null, availableCategories);

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

            var ds = categoriesListView.Source as DataSource;
            ds.RefreshData(assignedCategories, null);
            ds.ReloadData();
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

            var ds = categoriesListView.Source as DataSource;
            ds.EditingWillBegin();
            categoriesListView.SetEditing(true, true);
            searchResultsController.TableView.SetEditing(true, true);
            ds.EditingDidBegin();
        }

        async void ExitEditModeButtonItem_Clicked(object sender, EventArgs e)
        {
            var ds = categoriesListView.Source as DataSource;

            if (ds.DidCategoriesChanged)
            {
                CommonConfig.Logger.Info(string.Format("Categories changed - will update. [entity={0}]", BusinessEntityPreview));

                var categoriesToAssign = ds.GetChangedUnassignedCategories(); //TODO need to put the right one

                var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("updating_categories___"));

                try
                {
                    switch (BusinessEntityPreview.ObjectType)
                    {
                        case ObjectType.Document:
                            var documentPreview = BusinessEntityPreview as DocumentPreview;
                            await Managers.DocumentsManager.SetCategoriesAsync(documentPreview, categoriesToAssign.ToList());
                            PlatformConfig.MessengerHub.Publish(new DocumentPreviewCategoriesChangedMessage(this, documentPreview.Id, documentPreview.Categories));
                            break;
                        case ObjectType.Contact:
                            var contactPreview = BusinessEntityPreview as ContactPreview;
                            await Managers.ContactsManager.SetCategoriesAsync(contactPreview, categoriesToAssign.ToList());
                            PlatformConfig.MessengerHub.Publish(new ContactPreviewCategoriesChangedMessage(this, contactPreview.Id, contactPreview.Categories));
                            break;
                        default:
                            throw new ArgumentException("Invalid BusinessEntityPreview!");
                    }
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

            var ds = categoriesListView.Source as DataSource;
            ds.EditingWillEnd();
            categoriesListView.SetEditing(false, true);
            searchResultsController.TableView.SetEditing(false, true);
            ds.EditingDidEnd();
        }

        #endregion


        class DataSource : UITableViewSource
        {
            public bool DidCategoriesChanged
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

            public List<Category> AssignedCategories
            {
                get
                {
                    return assignedCategories.ToList();
                }
            }

            public List<Category> AvailableCategories
            {
                get
                {
                    return availableCategories.ToList();
                }
            }

            readonly UITableView tableView;

            List<string> sectionIndexes;
            Dictionary<string, List<Category>> categoriesInView;

            readonly List<Category> assignedCategories;
            List<Category> availableCategories;
            readonly HashSet<Category> selectedCategories;
            readonly string emptyText;

            public bool FirstLoad
            {
                get;
                set;
            }

            public DataSource(UITableView tableView, string emptyText)
            {
                this.tableView = tableView;
                this.emptyText = emptyText;

                FirstLoad = true;

                sectionIndexes = new List<string>();
                categoriesInView = new Dictionary<string, List<Category>>();
                assignedCategories = new List<Category>();
                availableCategories = new List<Category>();
                selectedCategories = new HashSet<Category>();
            }

            #region UITableViewDataSource implementation

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (FirstLoad)
                {
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as WaitTableViewCell ?? WaitTableViewCell.Create();
                }

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
                DidCategoriesChanged |= tableView.Editing;

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
                DidCategoriesChanged |= tableView.Editing;

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

            public void ReloadData()
            {
                tableView.ReloadData();
            }

            public void RefreshData(List<Category> assignedCategories, List<Category> availableCategories)
            {
                FirstLoad = false;

                if (assignedCategories != null)
                {
                    this.assignedCategories.Clear();
                    this.assignedCategories.AddRange(assignedCategories);
                }
                if (availableCategories != null)
                {
                    this.availableCategories.Clear();
                    this.availableCategories.AddRange(availableCategories);
                }

                this.availableCategories = this.availableCategories.Except(this.assignedCategories, new CategoryComparer()).OrderBy(c => c.Name).ToList();

                var categories = new List<Category>();

                InvokeOnMainThread(() =>
                    {
                        if (!tableView.Editing)
                        {
                            categories.AddRange(this.assignedCategories);
                        }
                        else
                        {
                            categories.AddRange(this.assignedCategories.Union(this.availableCategories).ToList());
                        }
                    });

                sectionIndexes = categories.Select(c => c.Name.SafeSubstring(0, 1).ToUpper(CultureInfo.CurrentCulture)).Distinct().OrderBy(i => i).ToList();
                categoriesInView = categories.GroupBy(c => c.Name.SafeSubstring(0, 1).ToUpper(CultureInfo.CurrentCulture)).ToDictionary(g => g.Key, g => g.OrderBy(c => c.Name).ToList(), StringComparer.OrdinalIgnoreCase);
            }

            public void EditingWillBegin()
            {
                sectionIndexes.Clear();
                categoriesInView.Clear();

                var categoriesToAdd = assignedCategories.Union(availableCategories).OrderBy(c => c.Name).ToList();

                foreach (var category in categoriesToAdd)
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

                UIView.Transition(tableView, 0.25d, UIViewAnimationOptions.TransitionCrossDissolve, tableView.ReloadData, null); //TODO
            }

            public void EditingDidBegin()
            {
                selectedCategories.UnionWith(assignedCategories);

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

                UIView.Transition(tableView, 0.25d, UIViewAnimationOptions.TransitionCrossDissolve, tableView.ReloadData, null);
            }

            public void EditingDidEnd()
            {
                selectedCategories.Clear();

                foreach (var indexPath in tableView.IndexPathsForVisibleRows)
                {
                    tableView.DeselectRow(indexPath, false);
                }

                DidCategoriesChanged = false;
            }

            public List<Category> GetChangedAssignedCategories()
            {
                return assignedCategories.Except(SelectedCategories).ToList();
            }

            public List<Category> GetChangedUnassignedCategories()
            {
                return availableCategories.Intersect(SelectedCategories).ToList();
            }

            public NSIndexPath IndexPathForCategory(Category category)
            {
                var index = 0;
                var section = 0;

                foreach (var entry in categoriesInView)
                {
                    if (entry.Value.Contains(category))
                    {
                        index = entry.Value.IndexOf(category);
                        section = sectionIndexes.IndexOf(entry.Key);
                    }
                }

                return NSIndexPath.FromItemSection(index, section);
            }

            public void SelectCategory(Category category)
            {
                DidCategoriesChanged |= tableView.Editing;
                tableView.SelectRow(IndexPathForCategory(category), true, UITableViewScrollPosition.None);
                selectedCategories.Add(category);
            }

            public void DeselectCategory(Category category)
            {
                DidCategoriesChanged |= tableView.Editing;
                tableView.DeselectRow(IndexPathForCategory(category), true);
                selectedCategories.Remove(category);
            }
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
