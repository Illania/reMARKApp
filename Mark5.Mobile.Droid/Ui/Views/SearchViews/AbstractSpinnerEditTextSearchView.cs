//
// Project: Mark5.Mobile.Droid
// File: AbstractSpinnerEditTextSearchView.cs
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

    public abstract class AbstractSpinnerEditTextSearchView<T> : AbstractSearchView<T>
    {

        protected readonly AppCompatSpinner Spinner;
        protected readonly AppCompatEditText EditText;

        protected AbstractSpinnerEditTextSearchView(Context context)
            : base(context)
        {
            Orientation = Vertical;
            SetPadding(DistanceLarge, DistanceNormal, DistanceLarge, DistanceNormal);

            Spinner = new AppCompatSpinner(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
            };
            AddView(Spinner);

            EditText = new AppCompatEditText(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                {
                    LeftMargin = -DistanceSmall
                }
            };
            EditText.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            EditText.SetHint(Resource.String.type_search_query);
            AddView(EditText);
        }
    }
}
