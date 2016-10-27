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
using Mark5.Mobile.Droid.Ui.Common.BusMesseges;

namespace Mark5.Mobile.Droid
{
    public class AvailableCategoriesListFragment : RetainableStateFragment, View.IOnClickListener, SearchView.IOnQueryTextListener, SearchView.IOnCloseListener
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
            CommonConfig.Logger.Info($"Creating {nameof(AvailableCategoriesListFragment)} [businessEntity.id={BusinessEntityPreview.Id}, businessEntity.objectType={BusinessEntityPreview.ObjectType}]");

            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            var refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            refreshLayout.Enabled = false;

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));

            adapter = new CategoriesListFragment.CategoriesListAdapter(selectedCategories, true);
            recyclerView.SetAdapter(adapter);

            searchAdapter = new CategoriesListFragment.CategoriesListAdapter(selectedCategories, true);

            HasOptionsMenu = true;

            ((BaseAppCompatActivity)Activity).SupportActionBar.SetDisplayHomeAsUpEnabled(false);

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
            searchView = (SearchView)MenuItemCompat.GetActionView(searchItem);
            searchView.QueryHint = GetString(Resource.String.filter);
            searchView.SetOnSearchClickListener(this);
            searchView.SetOnQueryTextListener(this);
            searchView.SetOnCloseListener(this);
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
            }).ContinueWith(t =>
                    {
                        dismissAction();
                        if (t.IsFaulted)
                        {
                            CommonConfig.Logger.Error($"Update of categories failed", t.Exception);
                            Dialogs.ShowErrorDialog(Activity, t.Exception);
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
                                    //await Managers.ContactsManager.SetCategoriesAsync(contactPreview, selectedCategories.Values.ToList());
                                    break;
                                default:
                                    throw new ArgumentException("The business entity provided does not have categories in the model");
                            }



                            Activity.RunOnUiThread(() => Activity.OnBackPressed());
                        } //TODO need to send info to the list
                          //Check what happens when we rotate

                    });
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

            adapter.AppendItems(availableCategories);
        }

        #endregion

        #region Search

        public void OnClick(View v)
        {
            if (v == searchView)
            {
                recyclerView.SwapAdapter(searchAdapter, true);
            }
        }

        public bool OnQueryTextChange(string newText)
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

        public bool OnQueryTextSubmit(string newText)
        {
            return false;
        }

        public bool OnClose()
        {
            searchHandler.RemoveCallbacksAndMessages(null);
            searchAdapter.Clear();
            recyclerView.SwapAdapter(adapter, true);
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
                BusinessEntityPreview = BusinessEntityPreview, //TODO need to save also the selected and the available 
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var clfs = restoredState as AvailableCategoriesListFragmentState;
            if (clfs != null)
            {
                BusinessEntityPreview = clfs.BusinessEntityPreview;
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(AvailableCategoriesListFragment)} [businessEntity.id={BusinessEntityPreview.Id}, businessEntity.objectType={BusinessEntityPreview.ObjectType}]";
        }

        class AvailableCategoriesListFragmentState : IRetainableState
        {
            public BusinessEntityPreview BusinessEntityPreview { get; set; }
        }

        #endregion

    }
}
