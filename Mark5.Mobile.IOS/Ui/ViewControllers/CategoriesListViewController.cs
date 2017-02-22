//
// Project: Mark5.Mobile.IOS
// File: CategoriesListViewController.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
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

        bool refreshingEntityCategories;
        bool refreshingAvailableCategories;

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
            var dataSource = new DataSource(this);
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

                RefreshAvailableCategories(availableCategories);
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

        void RefreshAvailableCategories(List<Category> availableCategories)
        {

        }

        #endregion

        #region Event handlers

        void DismissButtonItem_Clicked(object sender, EventArgs e)
        {

        }

        void CancelButtomItem_Clicked(object sender, EventArgs e)
        {

        }

        void EditModeButtonItem_Clicked(object sender, EventArgs e)
        {

        }

        void ExitEditModeButtonItem_Clicked(object sender, EventArgs e)
        {

        }

        #endregion


        class DataSource : UITableViewSource
        {
            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                throw new NotImplementedException();
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                throw new NotImplementedException();
            }
        }

    }
}
