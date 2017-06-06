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
using FastScrollRecycler;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Common.HubMessages;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid
{
    public class EditCategoriesListFragment : RetainableStateFragment, MenuItemCompat.IOnActionExpandListener, SearchView.IOnQueryTextListener
    {
        public BusinessEntityPreview BusinessEntityPreview { get; set; }
        public Action CloseRequest { get; set; }

        CategoriesListAdapter CurrentAdapter
        {
            get { return (CategoriesListAdapter) recyclerView.GetAdapter(); }
        }

        SwipeRefreshLayout refreshLayout;
        RecyclerView recyclerView;
        SearchView searchView;
        CategoriesListAdapter adapter;
        CategoriesListAdapter searchAdapter;
        AppCompatButton saveButton;

        readonly Dictionary<int, Category> selectedCategories = new Dictionary<int, Category>();

        readonly Handler searchHandler = new Handler();

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(EditCategoriesListFragment)} [businessEntity.id={BusinessEntityPreview?.Id}, businessEntity.objectType={BusinessEntityPreview?.ObjectType}]");

            var rootView = inflater.Inflate(Resource.Layout.list_with_button, container, false);

            refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            refreshLayout.SetColorSchemeResources(Resource.Color.blue, Resource.Color.darkerblue);
            refreshLayout.Enabled = false;

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));

            adapter = new CategoriesListAdapter(selectedCategories);
            adapter.ItemClicked += Adapter_ItemClicked;
            recyclerView.SetAdapter(adapter);

            searchAdapter = new CategoriesListAdapter(selectedCategories);
            searchAdapter.ItemClicked += Adapter_ItemClicked;

            saveButton = rootView.FindViewById<AppCompatButton>(Resource.Id.button);
            saveButton.Text = GetString(Resource.String.save);
            saveButton.Click += SaveButton_Click;

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity) Activity).SupportActionBar.Title = GetString(Resource.String.categories);
            ((AppCompatActivity) Activity).SupportActionBar.Subtitle = null;

            CommonConfig.Logger.Info($"Created {nameof(EditCategoriesListFragment)} [businessEntity.id={BusinessEntityPreview?.Id}, businessEntity.objectType={BusinessEntityPreview?.ObjectType}]");
        }

        public override async void OnResume()
        {
            base.OnResume();

            if (adapter.ItemCount < 1)
            {
                CommonConfig.Logger.Info($"Refreshing {nameof(EditCategoriesListFragment)} [businessEntity.id={BusinessEntityPreview?.Id}, businessEntity.objectType={BusinessEntityPreview?.ObjectType}]");
                await RefreshData();
            }

            UpdateControls();
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.menu_main, menu);

            var filterItem = menu.FindItem(Resource.Id.action_filter);
            MenuItemCompat.SetOnActionExpandListener(filterItem, this);
            searchView = (SearchView) MenuItemCompat.GetActionView(filterItem);
            searchView.QueryHint = GetString(Resource.String.filter);
            searchView.SetOnQueryTextListener(this);
        }

        void Adapter_ItemClicked(object sender, Category e)
        {
            ToggleSelected(e);
            UpdateControls();
        }

        async void SaveButton_Click(object sender, EventArgs e)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.updating_categories, Resource.String.please_wait);

            try
            {
                switch (BusinessEntityPreview.ObjectType)
                {
                    case ObjectType.Document:
                        var documentPreview = BusinessEntityPreview as DocumentPreview;
                        await Managers.DocumentsManager.SetCategoriesAsync(documentPreview, selectedCategories.Values.ToList());
                        PlatformConfig.MessengerHub.Publish(new DocumentPreviewCategoriesChangedMessage(this, documentPreview.Id, documentPreview.Categories));
                        break;
                    case ObjectType.Contact:
                        var contactPreview = BusinessEntityPreview as ContactPreview;
                        await Managers.ContactsManager.SetCategoriesAsync(contactPreview, selectedCategories.Values.ToList());
                        PlatformConfig.MessengerHub.Publish(new ContactPreviewCategoriesChangedMessage(this, contactPreview.Id, contactPreview.Categories));
                        break;
                    default:
                        throw new ArgumentException("Invalid BusinessEntityPreview!");
                }

                dismissAction();
                if (CloseRequest != null)
                    CloseRequest();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Update of categories failed", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        #region Refresh methods

        async Task RefreshData()
        {
            try
            {
                CommonConfig.Logger.Info($"Refresh running...");

                refreshLayout.Refreshing = true;

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
            finally
            {
                refreshLayout.Refreshing = false;

                CommonConfig.Logger.Info($"Refresh finished");
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
            if (!IsAdded || IsDetached || IsRemoving)
                return;

            if (selectedCategories.Count < 1)
            {
                ((AppCompatActivity) Activity).SupportActionBar.Title = GetString(Resource.String.select_categories);
                ((AppCompatActivity) Activity).SupportActionBar.Subtitle = null;
            }
            else
            {
                ((AppCompatActivity) Activity).SupportActionBar.Title = Resources.GetQuantityString(Resource.Plurals.categories_selected, selectedCategories.Count, selectedCategories.Count);
                ((AppCompatActivity) Activity).SupportActionBar.Subtitle = null;
            }
        }

        #endregion

        #region Filtering

        bool MenuItemCompat.IOnActionExpandListener.OnMenuItemActionExpand(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_filter)
            {
                recyclerView.SwapAdapter(searchAdapter, true);
                (this as SearchView.IOnQueryTextListener).OnQueryTextChange(string.Empty);
                return true;
            }

            return false;
        }

        bool MenuItemCompat.IOnActionExpandListener.OnMenuItemActionCollapse(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_filter)
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
                    searchAdapter.ReplaceItems(adapter.Items);
                else
                    searchAdapter.ReplaceItems(adapter.Items.Where(dp => MatchesQuery(dp, newText)).ToList());
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
                AvailableCategories = adapter.Items
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var clfs = restoredState as AvailableCategoriesListFragmentState;
            if (clfs != null)
            {
                BusinessEntityPreview = clfs.BusinessEntityPreview;
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
            return $"{nameof(EditCategoriesListFragment)} [businessEntity.id={BusinessEntityPreview.Id}, businessEntity.objectType={BusinessEntityPreview.ObjectType}]";
        }

        class AvailableCategoriesListFragmentState : IRetainableState
        {
            public BusinessEntityPreview BusinessEntityPreview { get; set; }

            public Dictionary<int, Category> SelectedCategories { get; set; }

            public List<Category> AvailableCategories { get; set; }
        }

        #endregion

        #region RecyclerView Adapter/ViewHolder

        class CategoriesListAdapter : RecyclerView.Adapter, ISectionedAdapter
        {
            readonly List<Category> categoriesInView = new List<Category>();
            readonly Dictionary<int, Category> selectedCategoriesInView;

            public override int ItemCount
            {
                get { return categoriesInView.Count; }
            }

            public List<Category> Items
            {
                get { return categoriesInView; }
            }

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

            string ISectionedAdapter.GetSectionName(int position)
            {
                return categoriesInView[position].Name?.SafeSubstring(0, 1)?.ToUpper() ?? "";
            }
        }

        class CategoryViewHolder : RecyclerView.ViewHolder
        {
            public string Name
            {
                set { nameTextView.Text = value; }
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
                set { selectedOverlay.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }
            }

            readonly View colorImageView;
            readonly AppCompatTextView nameTextView;
            readonly AppCompatTextView descriptionTextView;
            readonly View selectedOverlay;

            public CategoryViewHolder(View itemView)
                : base(itemView)
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