//
// Project: Mark5.Mobile.Droid
// File: DocumentAttachmentSearchView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public class DocumentAttachmentSearchView : AbstractEditableTextSearchView<SearchDocumentsCriteria>
    {
        public DocumentAttachmentSearchView(Android.Content.Context context, LinearLayoutCompat containerLayout)
            : base(context, Resource.String.search_document_attachment, containerLayout)
        {
        }

        public override void Refresh()
        {
            //TODO
        }

        public override void UpdateCriteria()
        {
            //TODO
        }
    }
}
