//
// Project: Mark5.Mobile.Droid
// File: DocumentReferenceNumberSearchView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public class DocumentReferenceNumberSearchView : AbstractEditableTextSearchView<SearchDocumentsCriteria>
    {
        public DocumentReferenceNumberSearchView(Android.Content.Context context) : base(context, Resource.String.search_document_reference_number)
        {
        }

        public override void FromCriteria(SearchDocumentsCriteria criteria)
        {
            //TODO
        }

        public override void ToCriteria(SearchDocumentsCriteria criteria)
        {
            //TODO
        }
    }
}
