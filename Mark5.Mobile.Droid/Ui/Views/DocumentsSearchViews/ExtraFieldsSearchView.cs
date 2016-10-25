//
// Project: Mark5.Mobile.Droid
// File: ExtraFieldsSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Support.Design.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.DocumentsSearchViews
{

    public class ExtraFieldsSearchView : DocumentsSearchView
    {

        readonly TextInputEditText extraFieldsField;

        public ExtraFieldsSearchView(Context context)
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

            extraFieldsField = new TextInputEditText(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            extraFieldsField.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            extraFieldsField.SetHint(Resource.String.search_extra_fields);
            textInputLayout.AddView(extraFieldsField);
        }

        public override void SetFromCriteria(SearchDocumentsCriteria criteria)
        {
            extraFieldsField.Text = criteria.Comment;
        }

        public override void UpdateCriteria(SearchDocumentsCriteria criteria)
        {
            criteria.Comment = extraFieldsField.Text;
        }
    }
}
