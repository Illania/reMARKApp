//
// Project: Mark5.Mobile.Droid
// File: ReferenceNumberSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.DocumentsSearchViews
{

    public class ReferenceNumberSearchView : DocumentsSearchView
    {

        readonly AppCompatEditText referenceNumberField;

        public ReferenceNumberSearchView(Context context)
            : base(context)
        {
            Orientation = Vertical;
            SetPadding(DistanceLarge, DistanceNormal, DistanceLarge, DistanceNormal);

            referenceNumberField = new AppCompatEditText(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                {
                    LeftMargin = -DistanceSmall
                }
            };
            referenceNumberField.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            referenceNumberField.SetHint(Resource.String.search_reference_number);
            AddView(referenceNumberField);
        }

        public override void SetFromCriteria(SearchDocumentsCriteria criteria)
        {
            referenceNumberField.Text = criteria.Reference;
        }

        public override void UpdateCriteria(SearchDocumentsCriteria criteria)
        {
            criteria.Reference = referenceNumberField.Text;
        }
    }
}
