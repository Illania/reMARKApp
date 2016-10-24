//
// Project: Mark5.Mobile.Droid
// File: SubjectMessageSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;
using Android.Content;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common.Model;
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
            SetPadding(DistanceLarge, DistanceLarge, DistanceLarge, DistanceNormal);

            var subjectMessageAdapter = ArrayAdapter.CreateFromResource(context, Resource.Array.search_subject_message, Android.Resource.Layout.SimpleSpinnerItem);
            subjectMessageAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);

            subjectMessageSpinner = new AppCompatSpinner(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Adapter = subjectMessageAdapter
            };
            subjectMessageSpinner.SetSelection(0);
            AddView(subjectMessageSpinner);

            subjectMessageField = new AppCompatEditText(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
            };
            subjectMessageField.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            subjectMessageField.SetHint(Resource.String.type_search_query);
            AddView(subjectMessageField);
        }

        public override void SetFromCriteria(SearchDocumentsCriteria criteria)
        {
            subjectMessageSpinner.SetSelection((int)criteria.SubjectMessageClause);
            subjectMessageField.Text = criteria.SubjectMessageField;
        }

        public override void UpdateCriteria(SearchDocumentsCriteria criteria)
        {
            criteria.SubjectMessageClause = (SubjectMessageClause)subjectMessageSpinner.SelectedItemPosition;
            criteria.SubjectMessageField = subjectMessageField.Text;
        }
    }
}
