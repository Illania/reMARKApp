//
// Project: Mark5.Mobile.Droid
// File: DocumentAttachmentSearchView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public class DocumentAttachmentSearchView : AbstractEditableTextSearchView<SearchDocumentsCriteria>
    {
        public DocumentAttachmentSearchView(Android.Content.Context context) : base(context, Resource.String.search_document_attachment)
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
