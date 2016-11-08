//
// Project: Mark5.Mobile.Droid
// File: CategoriesListFragment.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.OS;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid
{
    public class AvailableCategoriesListFragment : RetainableStateFragment, MenuItemCompat.IOnActionExpandListener, SearchView.IOnQueryTextListener
    {
        public BusinessEntityPreview BusinessEntityPreview
        {
            get;
            set;
        }

        RecyclerView recyclerView;
        SearchView searchView;

        CategoriesListFragment.CategoriesListAdapter adapter;
        CategoriesListFragment.CategoriesListAdapter searchAdapter;

        Dictionary<int, Category> selectedCategories = new Dictionary<int, Category>();

        readonly Handler searchHandler = new Handler();

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(AvailableCategoriesListFragment)} [businessEntity.id={BusinessEntityPreview?.Id}, businessEntity.objectType={BusinessEntityPreview?.ObjectType}]");

            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            var refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            refreshLayout.Enabled = false;

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));

            adapter = new CategoriesListFragment.CategoriesListAdapter(true);
            adapter.SelectedCategoriesInView = selectedCategories;
            recyclerView.SetAdapter(adapter);

            searchAdapter = new CategoriesListFragment.CategoriesListAdapter(true);
            searchAdapter.SelectedCategoriesInView = selectedCategories;

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.categories);

            CommonConfig.Logger.Info($"Created {nameof(AvailableCategoriesListFragment)} [businessEntity.id={BusinessEntityPreview?.Id}, businessEntity.objectType={BusinessEntityPreview?.ObjectType}]");
        }

        public override async void OnResume()
        {
            base.OnResume();

            if (adapter.ItemCount < 1)
            {
                CommonConfig.Logger.Info($"Refreshing {nameof(AvailableCategoriesListFragment)} [businessEntity.id={BusinessEntityPreview?.Id}, businessEntity.objectType={BusinessEntityPreview?.ObjectType}]");
                await RefreshData();
            }
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.menu_main, menu);

            var item = menu.Add(Menu.None, 10, 10, Resource.String.done);
            item.SetShowAsAction(ShowAsAction.Always);

            var searchItem = menu.FindItem(Resource.Id.action_search);
            MenuItemCompat.SetOnActionExpandListener(searchItem, this);
            searchView = (SearchView)MenuItemCompat.GetActionView(searchItem);
            searchView.QueryHint = GetString(Resource.String.filter);
            searchView.SetOnQueryTextListener(this);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == 10)
            {
                UpdateEntityCategories();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        void UpdateEntityCategories()
        {
            if (!adapter.CategoriesModified)
            {
                Activity.OnBackPressed();
                return;
            }

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.updating_categories, Resource.String.please_wait);

            Task.Run(async () =>
            {
                switch (BusinessEntityPreview.ObjectType)
                {
                    case ObjectType.Document:
                        var documentPreview = BusinessEntityPreview as DocumentPreview;
                        await Managers.DocumentsManager.SetCategoriesAsync(documentPreview, selectedCategories.Values.ToList());
                        break;
                    case ObjectType.Contact:
                        var contactPreview = BusinessEntityPreview as ContactPreview;
                        await Managers.ContactsManager.SetCategoriesAsync(contactPreview, selectedCategories.Values.ToList());
                        break;
                    default:
                        throw new ArgumentException("The business entity provided does not have categories in the model");
                }
            }).ContinueWith(async t =>
                    {
                        dismissAction();
                        if (t.IsFaulted)
                        {
                            CommonConfig.Logger.Error($"Update of categories failed", t.Exception.InnerException);
                            await Dialogs.ShowErrorDialogAsync(Activity, t.Exception.InnerException);
                        }
                        else
                        {
                            switch (BusinessEntityPreview.ObjectType)
                            {
                                case ObjectType.Document:
                                    var documentPreview = BusinessEntityPreview as DocumentPreview;
                                    PlatformConfig.MessengerHub.Publish(new DocumentPreviewCategoriesChangedMessage(this, documentPreview.Id, documentPreview.Categories));
                                    break;
                                case ObjectType.Contact:
                                    var contactPreview = BusinessEntityPreview as ContactPreview;
                                    PlatformConfig.MessengerHub.Publish(new ContactPreviewCategoriesChangedMessage(this, contactPreview.Id, contactPreview.Categories));
                                    break;
                                default:
                                    throw new ArgumentException("The business entity provided does not have categories in the model");
                            }

                            Activity.OnBackPressed();
                        }
                    }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        #region Refresh methods

        async Task RefreshData()
        {
            try
            {
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

                RefreshView(availableCategories);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while retrieving available categories [businessEntity.id={BusinessEntityPreview.Id}, businessEntity.objectType={BusinessEntityPreview.ObjectType}]", ex);
                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        void RefreshView(List<Category> availableCategories)
        {
            if (selectedCategories.Count == 0)
            {
                switch (BusinessEntityPreview.ObjectType)
                {
                    case ObjectType.Document:
                        var documentPreview = BusinessEntityPreview as DocumentPreview;
                        documentPreview.Categories.ForEach(c => selectedCategories.Add(c.Id, c));
                        break;
                    case ObjectType.Contact:
                        var contactPreview = BusinessEntityPreview as ContactPreview;
                        contactPreview.Categories.ForEach(c => selectedCategories.Add(c.Id, c));
                        break;
                    default:
                        throw new ArgumentException("The business entity provided does not have categories in the model");
                }
            }

            adapter.SetItems(availableCategories);
        }

        #endregion

        #region Filtering

        bool MenuItemCompat.IOnActionExpandListener.OnMenuItemActionExpand(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_search)
            {
                recyclerView.SwapAdapter(searchAdapter, true);
                return true;
            }

            return false;
        }

        bool MenuItemCompat.IOnActionExpandListener.OnMenuItemActionCollapse(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_search)
            {
                searchHandler.RemoveCallbacksAndMessages(null);
                searchAdapter.Clear();
                recyclerView.SwapAdapter(adapter, true);
                return true;
            }

            return false;
        }

        bool SearchView.IOnQueryTextListener.OnQueryTextChange(string newText)
        {
            searchHandler.RemoveCallbacksAndMessages(null);
            searchHandler.PostDelayed(() =>
            {
                if (string.IsNullOrWhiteSpace(newText))
                {
                    searchAdapter.Clear();
                }
                else
                {
                    searchAdapter.ReplaceItems(adapter.Items.Where(dp => MatchesQuery(dp, newText)).ToList());
                }
            }, 500);
            return false;
        }

        bool SearchView.IOnQueryTextListener.OnQueryTextSubmit(string newText)
        {
            return false;
        }

        static bool MatchesQuery(Category c, string query)
        {
            if (c.Name.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                return true;
            }
            if (c.Description.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Retained State

        public override IRetainableState OnRetainInstanceState()
        {
            return new AvailableCategoriesListFragmentState
            {
                BusinessEntityPreview = BusinessEntityPreview,
                SelectedCategories = selectedCategories,
                AvailableCategories = adapter.Items,
                CategoriesModified = adapter.CategoriesModified,
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var clfs = restoredState as AvailableCategoriesListFragmentState;
            if (clfs != null)
            {
                BusinessEntityPreview = clfs.BusinessEntityPreview;
                selectedCategories = clfs.SelectedCategories;
                adapter.SelectedCategoriesInView = selectedCategories;
                adapter.SetItems(clfs.AvailableCategories);
                adapter.CategoriesModified = clfs.CategoriesModified;
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(AvailableCategoriesListFragment)} [businessEntity.id={BusinessEntityPreview.Id}, businessEntity.objectType={BusinessEntityPreview.ObjectType}]";
        }

        class AvailableCategoriesListFragmentState : IRetainableState
        {
            public BusinessEntityPreview BusinessEntityPreview { get; set; }
            public Dictionary<int, Category> SelectedCategories { get; set; }
            public List<Category> AvailableCategories { get; set; }
            public bool CategoriesModified { get; set; }

        }

        #endregion

    }
}
