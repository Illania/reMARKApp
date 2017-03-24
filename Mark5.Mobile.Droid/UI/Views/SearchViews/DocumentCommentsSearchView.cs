//
// Project: Mark5.Mobile.Droid
// File: DocumentCommentsSearchView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public class DocumentCommentsSearchView : AbstractEditableTextSearchView<SearchDocumentsCriteria>
    {
        public DocumentCommentsSearchView(Android.Content.Context context) : base(context, Resource.String.search_document_comments)
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
