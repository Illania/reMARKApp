//
// Project: Mark5.Mobile.Droid
// File: CategoriesSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public abstract class AbstractCategoriesSearchView<T> : AbstractSearchView<T>
    {

        readonly AppCompatTextView categoriesTitle;
        readonly AppCompatTextView categoriesSubtitle;

        protected AbstractCategoriesSearchView(Context context)
            : base(context)
        {
            Orientation = Vertical;
            SetPadding(DistanceLarge, DistanceNormal, DistanceLarge, DistanceNormal);

            var typedArray = Context.ObtainStyledAttributes(new int[] { Resource.Attribute.selectableItemBackground });
            SetBackgroundResource(typedArray.GetResourceId(0, 0));
            typedArray.Recycle();

            Clickable = true;
            // TODO
            //Click += (sender, e) =>
            //{
            //};

            categoriesTitle = new AppCompatTextView(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            categoriesTitle.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            categoriesTitle.SetText(Resource.String.search_categories);
            AddView(categoriesTitle);

            categoriesSubtitle = new AppCompatTextView(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            categoriesSubtitle.SetText(Resource.String.search_categories);
            categoriesSubtitle.SetTextAppearanceCompat(context, Resource.Style.fontSmallLight);
            AddView(categoriesSubtitle);
        }
    }
}
