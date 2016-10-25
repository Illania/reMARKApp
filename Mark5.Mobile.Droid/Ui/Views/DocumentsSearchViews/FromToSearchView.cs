//
// Project: Mark5.Mobile.Droid
// File: FromToSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.DocumentsSearchViews
{

    public class FromToSearchView : DocumentsSearchView
    {

        readonly AppCompatSpinner fromToSpinner;
        readonly AppCompatEditText fromToField;

        public FromToSearchView(Context context)
            : base(context)
        {
            Orientation = Vertical;
            SetPadding(DistanceLarge, DistanceNormal, DistanceLarge, DistanceNormal);

            fromToSpinner = new AppCompatSpinner(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Adapter = CustomArrayAdapter.CreateWithoutLeftPadding(context, Resource.Array.search_from_to, Android.Resource.Layout.SimpleSpinnerItem, Resource.Layout.support_simple_spinner_dropdown_item)
            };
            fromToSpinner.SetSelection(2);
            AddView(fromToSpinner);

            fromToField = new AppCompatEditText(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                {
                    LeftMargin = -DistanceSmall
                }
            };
            fromToField.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            fromToField.SetHint(Resource.String.type_search_query);
            AddView(fromToField);
        }

        public override void SetFromCriteria(SearchDocumentsCriteria criteria)
        {
            fromToSpinner.SetSelection((int)criteria.FromToClause);
            fromToField.Text = criteria.FromToField;
        }

        public override void UpdateCriteria(SearchDocumentsCriteria criteria)
        {
            criteria.FromToClause = (FromToClause)fromToSpinner.SelectedItemPosition;
            criteria.FromToField = fromToField.Text;
        }
    }
}
