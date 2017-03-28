//
// Project: Mark5.Mobile.Droid
// File: PickLineListFragment.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class PickLineListFragment : RetainableStateFragment
    {

        RecyclerView recyclerView;
        CategoriesListAdapter adapter;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Android.OS.Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(PickLineListFragment)}]");

            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkerblue)));


            return rootView;
        }

        public override string GenerateTag()
        {
            return $"{nameof(PickLineListFragment)} ]";
        }

        class CategoriesListAdapter : RecyclerView.Adapter
        {
            readonly List<Category> categoriesInView = new List<Category>(200);
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
                var c = categoriesInView[position];
                var cvh = holder as CategoryViewHolder;

                cvh.ItemView.SetOnClickListener(new ActionOnClickListener(() => ItemClicked(this, c)));

                cvh.Selected = selectedCategoriesInView.ContainsKey(c.Id);
                cvh.Name = c.Name;
                cvh.Description = c.Description;
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.search_list_item_categories, parent, false);
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

        }

        class CategoryViewHolder : RecyclerView.ViewHolder
        {
            bool selected;

            public string Name
            {
                set
                {
                    nameTextView.Text = value;
                    nameTextView.SetTextAppearanceCompat(nameTextView.Context, Selected ? Resource.Style.searchCategorySelected : Resource.Style.searchCategory);
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
                        descriptionTextView.SetTextAppearanceCompat(nameTextView.Context, Selected ? Resource.Style.searchCategorySubtitleSelected : Resource.Style.searchCategorySubtitle);
                    }
                }
            }


            public bool Selected
            {
                set
                {
                    selected = value;
                    selectedOverlay.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
                }
                get
                {
                    return selected;
                }
            }

            readonly AppCompatTextView nameTextView;
            readonly AppCompatTextView descriptionTextView;

            readonly View selectedOverlay;

            public CategoryViewHolder(View itemView) : base(itemView)
            {
                nameTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_category_name);
                descriptionTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_categoty_description);
                selectedOverlay = itemView.FindViewById<View>(Resource.Id.selected_overlay);
            }
        }


    }


}
