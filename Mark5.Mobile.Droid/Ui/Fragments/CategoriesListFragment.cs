using System;
using System.Collections.Generic;
using System.Linq;
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
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid
{
    public class CategoriesListFragment : RetainableStateFragment, MenuItemCompat.IOnActionExpandListener, SearchView.IOnQueryTextListener
    {
        public List<Category> Categories => adapter.Items;

        BusinessEntityPreview businessEntityPreview;
        Action closeRequest;

        RecyclerView recyclerView;
        SearchView searchView;

        CategoriesListAdapter adapter;
        CategoriesListAdapter searchAdapter;

        readonly Handler searchHandler = new Handler();

        public CategoriesListFragment(BusinessEntityPreview businessEntityPreview, Action closeRequest)
        {
            this.businessEntityPreview = businessEntityPreview;
            this.closeRequest = closeRequest;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(CategoriesListFragment)} [businessEntity.id={businessEntityPreview?.Id}, businessEntity.objectType={businessEntityPreview?.ObjectType}]");

            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            var emptyView = rootView.FindViewById<AppCompatTextView>(Resource.Id.empty_view);
            emptyView.SetText(Resource.String.no_categories);

            var refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            refreshLayout.Enabled = false;

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));

            adapter = new CategoriesListAdapter();
            adapter.RegisterAdapterDataObserver(new LambdaEmptyAdapterObserver(() =>
            {
                if (recyclerView.GetAdapter() != adapter)
                    return;

                emptyView.Visibility = adapter.ItemCount < 1 ? ViewStates.Visible : ViewStates.Gone;
                recyclerView.Visibility = adapter.ItemCount > 0 ? ViewStates.Visible : ViewStates.Gone;
            }));
            recyclerView.SetAdapter(adapter);

            searchAdapter = new CategoriesListAdapter();

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity) Activity).SupportActionBar.Title = GetString(Resource.String.categories);
            ((AppCompatActivity) Activity).SupportActionBar.Subtitle = null;

            CommonConfig.Logger.Info($"Created {nameof(CategoriesListFragment)} [businessEntity.id={businessEntityPreview?.Id}, businessEntity.objectType={businessEntityPreview?.ObjectType}]");
        }

        public override void OnResume()
        {
            base.OnResume();

            if (adapter.ItemCount < 1)
            {
                CommonConfig.Logger.Info($"Refreshing {nameof(CategoriesListFragment)} [businessEntity.id={businessEntityPreview?.Id}, businessEntity.objectType={businessEntityPreview?.ObjectType}]");
                RefreshView();
            }
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.menu_main, menu);

            var item = menu.Add(Menu.None, 10, 10, Resource.String.edit);
            item.SetShowAsAction(ShowAsAction.Always);

            var filterItem = menu.FindItem(Resource.Id.action_filter);
            MenuItemCompat.SetOnActionExpandListener(filterItem, this);
            searchView = (SearchView) MenuItemCompat.GetActionView(filterItem);
            searchView.QueryHint = GetString(Resource.String.filter);
            searchView.SetOnQueryTextListener(this);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == 10)
            {
                var clf = new EditCategoriesListFragment(businessEntityPreview, closeRequest);

                var ft = ((AppCompatActivity) Activity).SupportFragmentManager.BeginTransaction();
                ft.SetCustomAnimations(Resource.Animation.fade_in, Resource.Animation.fade_out, Resource.Animation.fade_in, Resource.Animation.fade_out);
                ft.Replace(Resource.Id.fragment_container, clf, clf.GenerateTag());
                ft.AddToBackStack(null);
                ft.Commit();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        void RefreshView()
        {
            switch (businessEntityPreview.ObjectType)
            {
                case ObjectType.Document:
                    var documentPreview = businessEntityPreview as DocumentPreview;
                    adapter.SetItems(documentPreview.Categories);
                    break;
                case ObjectType.Contact:
                    var contactPreview = businessEntityPreview as ContactPreview;
                    adapter.SetItems(contactPreview.Categories);
                    break;
                default:
                    throw new ArgumentException("The business entity provided does not have categories in the model");
            }
        }


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
                },
                500);
            return false;
        }

        bool SearchView.IOnQueryTextListener.OnQueryTextSubmit(string newText)
        {
            return false;
        }

        static bool MatchesQuery(Category c, string query)
        {
            if (c.Name.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0)
                return true;
            if (c.Description.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0)
                return true;

            return false;
        }

        #endregion

        #region Retained State

        public override IRetainableState OnRetainInstanceState()
        {
            return new CategoriesListFragmentState
            {
                BusinessEntityPreview = businessEntityPreview
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var clfs = restoredState as CategoriesListFragmentState;
            if (clfs != null)
                businessEntityPreview = clfs.BusinessEntityPreview;
        }

        public override string GenerateTag()
        {
            return $"{nameof(CategoriesListFragment)} [businessEntity.id={businessEntityPreview.Id}, businessEntity.objectType={businessEntityPreview.ObjectType}]";
        }

        class CategoriesListFragmentState : IRetainableState
        {
            public BusinessEntityPreview BusinessEntityPreview { get; set; }
        }

        #endregion

        #region RecyclerView Adapter/ViewHolder

        class CategoriesListAdapter : RecyclerView.Adapter, ISectionedAdapter
        {
            public override int ItemCount => Items.Count;

            public List<Category> Items { get; } = new List<Category>();

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var category = Items[position];
                var viewHolder = holder as CategoryViewHolder;

                viewHolder.Name = category.Name;
                viewHolder.HexColor = category.HexColor;
                viewHolder.Description = category.Description;
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_categories, parent, false);
                return new CategoryViewHolder(itemView);
            }

            public void SetItems(List<Category> categories)
            {
                var count = Items.Count;
                Items.AddRange(categories.OrderBy(c => c.Name));
                NotifyItemRangeInserted(count, categories.Count);
            }

            public void Clear()
            {
                var count = Items.Count;
                Items.Clear();
                NotifyItemRangeRemoved(0, count);
            }

            public void ReplaceItems(List<Category> items)
            {
                Clear();
                SetItems(items);
            }

            string ISectionedAdapter.GetSectionName(int position)
            {
                return Items[position].Name?.SafeSubstring(0, 1)?.ToUpper() ?? "";
            }
        }

        class CategoryViewHolder : RecyclerView.ViewHolder
        {
            public string Name { set => nameTextView.Text = value; }

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
                    gd.SetStroke(Conversion.ConvertDpToPixels(1), Color.Black);
                    gd.SetColor(Color.ParseColor(value));

                    colorImageView.Background = gd;
                }
            }

            public bool Selected { set => selectedOverlay.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }

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