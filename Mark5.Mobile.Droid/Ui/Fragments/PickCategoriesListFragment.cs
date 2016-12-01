//
// Project: Mark5.Mobile.Droid
// File: EditCategoriesListFragment.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Graphics.Drawables;
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
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid
{

    public class PickCategoriesListFragment : RetainableStateFragment, MenuItemCompat.IOnActionExpandListener, SearchView.IOnQueryTextListener
    {

        public ObjectType ObjectType { get; set; }
        public Action<List<Category>> CloseRequest { get; set; }

        CategoriesListAdapter CurrentAdapter
        {
            get { return (CategoriesListAdapter)recyclerView.GetAdapter(); }
        }

        SwipeRefreshLayout refreshLayout;
        RecyclerView recyclerView;
        SearchView searchView;
        CategoriesListAdapter adapter;
        CategoriesListAdapter searchAdapter;
        AppCompatButton selectButton;

        readonly Dictionary<int, Category> selectedCategories = new Dictionary<int, Category>();

        readonly Handler searchHandler = new Handler();

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(PickCategoriesListFragment)} [objectType={ObjectType}]");

            var rootView = inflater.Inflate(Resource.Layout.list_with_button, container, false);

            refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            refreshLayout.SetColorSchemeResources(Resource.Color.lightbrown, Resource.Color.brown);
            refreshLayout.Enabled = false;

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));

            adapter = new CategoriesListAdapter(selectedCategories);
            adapter.ItemClicked += Adapter_ItemClicked;
            recyclerView.SetAdapter(adapter);

            searchAdapter = new CategoriesListAdapter(selectedCategories);
            searchAdapter.ItemClicked += Adapter_ItemClicked;

            selectButton = rootView.FindViewById<AppCompatButton>(Resource.Id.button);
            selectButton.Text = GetString(Resource.String.select);
            selectButton.Click += SaveButton_Click;

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.categories);

            CommonConfig.Logger.Info($"Created {nameof(PickCategoriesListFragment)} [objectType={ObjectType}]");
        }

        public override async void OnResume()
        {
            base.OnResume();

            if (adapter.ItemCount < 1)
            {
                CommonConfig.Logger.Info($"Refreshing {nameof(PickCategoriesListFragment)} [objectType={ObjectType}]");
                await RefreshData();
            }

            UpdateControls();
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.menu_main, menu);

            var searchItem = menu.FindItem(Resource.Id.action_search);
            MenuItemCompat.SetOnActionExpandListener(searchItem, this);
            searchView = (SearchView)MenuItemCompat.GetActionView(searchItem);
            searchView.QueryHint = GetString(Resource.String.filter);
            searchView.SetOnQueryTextListener(this);
        }

        void Adapter_ItemClicked(object sender, Category e)
        {
            ToggleSelected(e);
            UpdateControls();
        }

        void SaveButton_Click(object sender, EventArgs e)
        {
            if (CloseRequest != null) CloseRequest(selectedCategories.Values.ToList());
        }

        #region Refresh methods

        async Task RefreshData()
        {
            try
            {
                CommonConfig.Logger.Info($"Refresh running...");

                refreshLayout.Refreshing = true;

                List<Category> availableCategories;
                switch (ObjectType)
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

                adapter.SetItems(availableCategories);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while retrieving available categories [objectType={ObjectType}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
            finally
            {
                refreshLayout.Refreshing = false;

                CommonConfig.Logger.Info($"Refresh finished");
            }
        }

        #endregion

        #region Private methods

        void ToggleSelected(Category category)
        {
            var isSelected = selectedCategories.ContainsKey(category.Id);
            if (isSelected)
            {
                selectedCategories.Remove(category.Id);
            }
            else
            {
                selectedCategories.Add(category.Id, category);
            }

            var position = CurrentAdapter.GetPosition(category);
            if (position >= 0)
            {
                CurrentAdapter.NotifyItemChanged(position);
            }
        }

        void UpdateControls()
        {
            if (!IsAdded || IsDetached || IsRemoving) return;

            if (selectedCategories.Count < 1)
            {
                ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.select_categories);
            }
            else
            {
                ((AppCompatActivity)Activity).SupportActionBar.Title = Resources.GetQuantityString(Resource.Plurals.categories_selected, selectedCategories.Count, selectedCategories.Count);
            }
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
                SelectedCategories = selectedCategories,
                AvailableCategories = adapter.Items
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var clfs = restoredState as AvailableCategoriesListFragmentState;
            if (clfs != null)
            {
                selectedCategories.Clear();
                foreach (var kv in clfs.SelectedCategories)
                {
                    selectedCategories.Add(kv.Key, kv.Value);
                }
                adapter.SetItems(clfs.AvailableCategories);
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(PickCategoriesListFragment)} [objectType={ObjectType}]";
        }

        class AvailableCategoriesListFragmentState : IRetainableState
        {

            public Dictionary<int, Category> SelectedCategories { get; set; }

            public List<Category> AvailableCategories { get; set; }
        }

        #endregion

        #region RecyclerView Adapter/ViewHolder

        class CategoriesListAdapter : RecyclerView.Adapter
        {

            readonly List<Category> categoriesInView = new List<Category>();
            readonly Dictionary<int, Category> selectedCategoriesInView;

            public override int ItemCount { get { return categoriesInView.Count; } }
            public List<Category> Items { get { return categoriesInView; } }

            public event EventHandler<Category> ItemClicked = delegate { };

            public CategoriesListAdapter(Dictionary<int, Category> selectedCategoriesInView)
            {
                this.selectedCategoriesInView = selectedCategoriesInView;
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var category = categoriesInView[position];
                var viewHolder = holder as CategoryViewHolder;

                viewHolder.ItemView.SetOnClickListener(new ActionOnClickListener(() => ItemClicked(this, category)));

                viewHolder.Name = category.Name;
                viewHolder.HexColor = category.HexColor;
                viewHolder.Description = category.Description;

                viewHolder.Selected = selectedCategoriesInView.ContainsKey(category.Id);
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_categories, parent, false);
                return new CategoryViewHolder(itemView);
            }

            public void SetItems(List<Category> categories)
            {
                var count = categoriesInView.Count;
                categoriesInView.AddRange(categories.OrderBy(c => c.Name));
                NotifyItemRangeInserted(count, categories.Count);
            }

            public void Clear()
            {
                var count = categoriesInView.Count;
                categoriesInView.Clear();
                NotifyItemRangeRemoved(0, count);
            }

            public void ReplaceItems(List<Category> items)
            {
                Clear();
                SetItems(items);
            }

            public int GetPosition(Category category)
            {
                return categoriesInView.FindIndex(c => c.Id == category.Id);
            }
        }

        class CategoryViewHolder : RecyclerView.ViewHolder
        {

            public string Name
            {
                set
                {
                    nameTextView.Text = value;
                }
            }

            public string Description
            {
                set
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        descriptionTextView.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        descriptionTextView.Visibility = ViewStates.Visible;
                        descriptionTextView.Text = value;
                    }
                }
            }

            public string HexColor
            {
                set
                {
                    var gd = new GradientDrawable();
                    gd.SetShape(ShapeType.Oval);
                    gd.SetStroke(ConversionUtils.ConvertDpToPixels(1), Color.Black);
                    gd.SetColor(Color.ParseColor(value));

                    colorImageView.Background = gd;
                }
            }

            public bool Selected
            {
                set
                {
                    selectedOverlay.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
                }
            }

            readonly View colorImageView;
            readonly AppCompatTextView nameTextView;
            readonly AppCompatTextView descriptionTextView;
            readonly View selectedOverlay;

            public CategoryViewHolder(View itemView) : base(itemView)
            {
                nameTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_category_name);
                descriptionTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_categoty_description);
                colorImageView = itemView.FindViewById<View>(Resource.Id.list_item_category_color);
                selectedOverlay = itemView.FindViewById<View>(Resource.Id.selected_overlay);
            }
        }

        #endregion

    }
}
