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
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Android.OS;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid
{
    public class CategoriesListFragment : RetainableStateFragment, View.IOnClickListener, SearchView.IOnQueryTextListener, SearchView.IOnCloseListener
    {
        public BusinessEntityPreview BusinessEntityPreview
        {
            get;
            set;
        }

        RecyclerView recyclerView;
        SearchView searchView;

        CategoriesListAdapter adapter;
        CategoriesListAdapter searchAdapter;

        readonly Handler searchHandler = new Handler();

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(CategoriesListFragment)} [businessEntity.id={BusinessEntityPreview.Id}, businessEntity.objectType={BusinessEntityPreview.ObjectType}]");

            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            var refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            refreshLayout.Enabled = false;

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));

            adapter = new CategoriesListAdapter(null);
            recyclerView.SetAdapter(adapter);

            searchAdapter = new CategoriesListAdapter(null);

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.categories);

            CommonConfig.Logger.Info($"Created {nameof(CategoriesListFragment)} [businessEntity.id={BusinessEntityPreview.Id}, businessEntity.objectType={BusinessEntityPreview.ObjectType}]");
        }

        public override void OnResume()
        {
            base.OnResume();

            if (adapter.ItemCount < 1)
            {
                CommonConfig.Logger.Info($"Refreshing {nameof(CategoriesListFragment)} [businessEntity.id={BusinessEntityPreview.Id}, businessEntity.objectType={BusinessEntityPreview.ObjectType}]");
                RefreshView();
            }
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.menu_main, menu);

            menu.Add(Menu.None, 10, 10, Resource.String.edit);

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
                var ft = Activity.SupportFragmentManager.BeginTransaction();
                var clf = new AvailableCategoriesListFragment
                {
                    BusinessEntityPreview = BusinessEntityPreview
                };
                ft.Replace(Resource.Id.fragment_container, clf, clf.GenerateTag());
                ft.AddToBackStack(null);
                ft.Commit();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        void RefreshView()
        {
            switch (BusinessEntityPreview.ObjectType)
            {
                case ObjectType.Document:
                    var documentPreview = BusinessEntityPreview as DocumentPreview;
                    adapter.AppendItems(documentPreview.Categories);
                    break;
                case ObjectType.Contact:
                    var contactPreview = BusinessEntityPreview as ContactPreview;
                    adapter.AppendItems(contactPreview.Categories);
                    break;
                default:
                    throw new ArgumentException("The business entity provided does not have categories in the model");
            }
        }

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
            return new CategoriesListFragmentState
            {
                BusinessEntityPreview = BusinessEntityPreview,
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var clfs = restoredState as CategoriesListFragmentState;
            if (clfs != null)
            {
                BusinessEntityPreview = clfs.BusinessEntityPreview;
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(CategoriesListFragment)} [businessEntity.id={BusinessEntityPreview.Id}, businessEntity.objectType={BusinessEntityPreview.ObjectType}]";
        }

        class CategoriesListFragmentState : IRetainableState
        {
            public BusinessEntityPreview BusinessEntityPreview { get; set; }
        }

        #endregion

        #region RecyclerView Adapter/ViewHolder

        public class CategoriesListAdapter : RecyclerView.Adapter
        {
            readonly List<Category> categoriesInView = new List<Category>();
            readonly Dictionary<int, Category> selectedCategoriesInView;
            readonly bool selectionEnabled;

            public override int ItemCount { get { return categoriesInView.Count; } }
            public List<Category> Items { get { return categoriesInView; } }

            public CategoriesListAdapter(Dictionary<int, Category> selectedCategoriesInView, bool selectionEnabled = false)
            {
                this.selectedCategoriesInView = selectedCategoriesInView ?? new Dictionary<int, Category>();
                this.selectionEnabled = selectionEnabled;
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var category = categoriesInView[position];
                var viewHolder = holder as CategoryViewHolder;

                if (selectionEnabled)
                {
                    viewHolder.ItemView.SetOnClickListener(new ActionOnClickListener(() => ToggleSelected(category)));
                }

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

            public void AppendItems(List<Category> categories)
            {
                var count = categoriesInView.Count;
                categoriesInView.AddRange(categories);
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
                AppendItems(items);
            }

            public bool IsSelected(Category category)
            {
                return selectedCategoriesInView.ContainsKey(category.Id);
            }

            public void ToggleSelected(Category category)
            {
                var isSelected = IsSelected(category);

                if (isSelected)
                {
                    selectedCategoriesInView.Remove(category.Id);
                }
                else
                {
                    selectedCategoriesInView.Add(category.Id, category);
                }

                NotifyItemChanged(GetPosition(category));
            }

            public int GetPosition(Category category)
            {
                return categoriesInView.FindIndex(c => c.Id == category.Id);
            }
        }

        public class CategoryViewHolder : RecyclerView.ViewHolder
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
                    var sd = new ShapeDrawable(new OvalShape());
                    sd.Paint.Color = Color.ParseColor(value);

                    colorImageView.Background = sd;
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
