//
// Project: 
// File: CategoriesListFragment.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//

using System.Collections.Generic;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid
{
    public class CategoriesListFragment : RetainableStateFragment
    {
        public BusinessEntityPreview BusinessEntityPreview
        {
            get;
            set;
        }

        RecyclerView recyclerView;
        LinearLayoutCompat linearLayout;

        CategoriesListAdapter adapter;
        CategoriesListAdapter searchAdapter;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(CategoriesListFragment)} [businessEntity.id={BusinessEntityPreview.Id}, businessEntity.objectType={BusinessEntityPreview.ObjectType}]");

            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            var refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            refreshLayout.Enabled = false;

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));

            adapter = new CategoriesListAdapter();
            recyclerView.SetAdapter(adapter);

            searchAdapter = new CategoriesListAdapter();

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

            RefreshView();
        }

        void RefreshView()
        {

        }

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

        class CategoriesListAdapter : RecyclerView.Adapter
        {
            List<Category> categories;

            public override int ItemCount { get { return categories.Count; } }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var category = categories[position];
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
                    var sd = new ShapeDrawable(new OvalShape());
                    sd.Paint.Color = Color.ParseColor(value);

                    colorImageView.Background = sd;
                }
            }

            readonly View colorImageView;
            readonly AppCompatTextView nameTextView;
            readonly AppCompatTextView descriptionTextView;

            public CategoryViewHolder(View itemView) : base(itemView)
            {
                nameTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_category_name);
                descriptionTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_categoty_description);
                colorImageView = itemView.FindViewById<View>(Resource.Id.list_item_category_color);
            }
        }

        #endregion
    }
}
