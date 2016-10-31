//
// Project: Mark5.Mobile.Droid
// File: SubjectMessageSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public class DocumentSubjectMessageSearchView : AbstractSpinnerEditTextSearchView<SearchDocumentsCriteria>
    {

        public DocumentSubjectMessageSearchView(Context context)
            : base(context)
        {
            Spinner.Adapter = CustomArrayAdapter.CreateWithoutLeftPadding(context, Resource.Array.search_document_subject_message, Android.Resource.Layout.SimpleSpinnerItem, Resource.Layout.support_simple_spinner_dropdown_item);
            Spinner.SetSelection(0);
        }

        public override void FromCriteria(SearchDocumentsCriteria criteria)
        {
            Spinner.SetSelection((int)criteria.SubjectMessageClause);
            EditText.Text = criteria.SubjectMessageField;
        }

        public override void ToCriteria(SearchDocumentsCriteria criteria)
        {
            criteria.SubjectMessageClause = (SubjectMessageClause)Spinner.SelectedItemPosition;
            criteria.SubjectMessageField = EditText.Text;
        }
    }
}
