//
// Project: Mark5.Mobile.Droid
// File: MustHaveCategoriesSearchView.cs
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

    public abstract class AbstractMustHaveCategoriesSearchView<T> : AbstractSearchView<T>
    {

        readonly AppCompatTextView title;
        readonly AppCompatTextView subtitle;

        protected AbstractMustHaveCategoriesSearchView(Context context)
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

            title = new AppCompatTextView(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            title.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            title.SetText(Resource.String.search_must_have_categories);
            AddView(title);

            subtitle = new AppCompatTextView(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            subtitle.SetText(Resource.String.search_must_have_categories);
            subtitle.SetTextAppearanceCompat(context, Resource.Style.fontSmallLight);
            AddView(subtitle);
        }
    }
}
