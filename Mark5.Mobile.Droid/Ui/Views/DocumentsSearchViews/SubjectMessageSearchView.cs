//
// Project: Mark5.Mobile.Droid
// File: SubjectMessageSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.DocumentsSearchViews
{

    public class SubjectMessageSearchView : DocumentsSearchView
    {

        readonly AppCompatSpinner subjectMessageSpinner;
        readonly AppCompatEditText subjectMessageField;

        public SubjectMessageSearchView(Context context)
            : base(context)
        {
            Orientation = Vertical;
            SetPadding(DistanceLarge, DistanceNormal, DistanceLarge, DistanceNormal);

            subjectMessageSpinner = new AppCompatSpinner(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Adapter = CustomArrayAdapter.CreateWithoutLeftPadding(context, Resource.Array.search_subject_message, Android.Resource.Layout.SimpleSpinnerItem, Resource.Layout.support_simple_spinner_dropdown_item)
            };
            subjectMessageSpinner.SetSelection(0);
            AddView(subjectMessageSpinner);

            subjectMessageField = new AppCompatEditText(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                {
                    LeftMargin = -DistanceSmall
                }
            };
            subjectMessageField.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            subjectMessageField.SetHint(Resource.String.type_search_query);
            AddView(subjectMessageField);
        }

        public override void FromCriteria(SearchDocumentsCriteria criteria)
        {
            subjectMessageSpinner.SetSelection((int)criteria.SubjectMessageClause);
            subjectMessageField.Text = criteria.SubjectMessageField;
        }

        public override void ToCriteria(SearchDocumentsCriteria criteria)
        {
            criteria.SubjectMessageClause = (SubjectMessageClause)subjectMessageSpinner.SelectedItemPosition;
            criteria.SubjectMessageField = subjectMessageField.Text;
        }
    }
}
