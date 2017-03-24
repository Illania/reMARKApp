//
// Project: Mark5.Mobile.Droid
// File: DocumentSubjectMessageSearchView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public class DocumentSubjectMessageSearchView : AbstractMultiSearchView<SearchDocumentsCriteria>
    {
        public DocumentSubjectMessageSearchView(Android.Content.Context context)
            : base(context, Resource.String.search_document_where, Resource.String.search_document_where_hint, Resource.Array.search_document_subject_message)
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
