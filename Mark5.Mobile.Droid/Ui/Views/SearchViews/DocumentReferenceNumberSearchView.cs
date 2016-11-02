//
// Project: Mark5.Mobile.Droid
// File: ReferenceNumberSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public class DocumentReferenceNumberSearchView : AbstractEditTextSearchView<SearchDocumentsCriteria>
    {

        public DocumentReferenceNumberSearchView(Context context)
            : base(context)
        {
            EditText.SetHint(Resource.String.search_document_reference_number);
        }

        public override void FromCriteria(SearchDocumentsCriteria criteria)
        {
            EditText.Text = criteria.Reference;
        }

        public override void ToCriteria(SearchDocumentsCriteria criteria)
        {
            criteria.Reference = EditText.Text;
        }
    }
}
