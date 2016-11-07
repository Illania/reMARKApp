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

    public abstract class AbstractButtonSearchView<T> : AbstractSearchView<T>
    {

        protected readonly AppCompatTextView ButtonTitle;
        protected readonly AppCompatTextView ButtonSubtitle;

        protected AbstractButtonSearchView(Context context)
            : base(context)
        {
            Orientation = Vertical;
            SetPadding(DistanceLarge, DistanceNormal, DistanceLarge, DistanceNormal);

            var typedArray = Context.ObtainStyledAttributes(new int[] { Resource.Attribute.selectableItemBackground });
            SetBackgroundResource(typedArray.GetResourceId(0, 0));
            typedArray.Recycle();

            Clickable = true;

            ButtonTitle = new AppCompatTextView(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            ButtonTitle.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            AddView(ButtonTitle);

            ButtonSubtitle = new AppCompatTextView(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            ButtonSubtitle.SetTextAppearanceCompat(context, Resource.Style.fontSmallLight);
            AddView(ButtonSubtitle);

            UpdateSubtitle();
        }

        public abstract void UpdateSubtitle();
    }
}
