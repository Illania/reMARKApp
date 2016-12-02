//
// Project: Mark5.Mobile.Droid
// File: CategoriesSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;
using Android.Content;
using Android.Support.V4.App;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public abstract class AbstractCategoriesSearchView<T> : AbstractSearchView<T>
    {

        public const int ViewId = 791;

        public static class RequestCodes
        {
            public const int CategoriesRequest = 1337;
        }

        readonly AppCompatTextView categoriesTitle;
        readonly AppCompatTextView categoriesSubtitle;

        protected ObjectType ObjectType;

        protected readonly List<int> SelectedCategories = new List<int>();

        protected AbstractCategoriesSearchView(Context context, Fragment fragment)
            : base(context)
        {
            Id = ViewId;

            Orientation = Vertical;
            SetPadding(DistanceLarge, DistanceNormal, DistanceLarge, DistanceNormal);

            var typedArray = Context.ObtainStyledAttributes(new int[] { Resource.Attribute.selectableItemBackground });
            SetBackgroundResource(typedArray.GetResourceId(0, 0));
            typedArray.Recycle();

            Clickable = true;
            Click += (sender, e) =>
            {
                var i = new Intent(context, typeof(PickCategoriesListActivity));
                i.PutExtra(PickCategoriesListActivity.ObjectTypeIntentKey, (int)ObjectType);
                i.PutExtra(PickCategoriesListActivity.PreselectedCategoryIdsIntentKey, SelectedCategories.ToArray());
                fragment.StartActivityForResult(i, RequestCodes.CategoriesRequest);
            };

            categoriesTitle = new AppCompatTextView(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            categoriesTitle.SetText(Resource.String.search_categories);
            categoriesTitle.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            AddView(categoriesTitle);

            categoriesSubtitle = new AppCompatTextView(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            categoriesSubtitle.SetText(Resource.String.search_categories_none);
            categoriesSubtitle.SetTextAppearanceCompat(context, Resource.Style.fontSmallLight);
            AddView(categoriesSubtitle);

            UpdateSubtitle();
        }

        public void SetSelectedCategoryIds(List<int> categoryIds)
        {
            SelectedCategories.Clear();
            SelectedCategories.AddRange(categoryIds);

            UpdateSubtitle();
        }

        protected void UpdateSubtitle()
        {
            if (SelectedCategories.Count > 0)
            {
                categoriesSubtitle.Text = Resources.GetQuantityString(Resource.Plurals.search_categories_selected, SelectedCategories.Count, SelectedCategories.Count);
            }
            else
            {
                categoriesSubtitle.SetText(Resource.String.search_categories_none);
            }
        }
    }
}
