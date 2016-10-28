//
// Project: Mark5.Mobile.Droid
// File: AbstractEditTextSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Support.Design.Widget;
using Android.Views;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public abstract class AbstractEditTextSearchView<T> : AbstractSearchView<T>
    {

        protected readonly TextInputEditText EditText;

        protected AbstractEditTextSearchView(Context context)
            : base(context)
        {
            Orientation = Vertical;
            SetPadding(DistanceLarge, DistanceNormal, DistanceLarge, DistanceNormal);

            var textInputLayout = new TextInputLayout(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                {
                    LeftMargin = -DistanceSmall
                }
            };
            AddView(textInputLayout);

            EditText = new TextInputEditText(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            EditText.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            textInputLayout.AddView(EditText);
        }
    }
}
