//
// Project: Mark5.Mobile.Droid
// File: DocumentFromToSearchView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Android.Widget;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public class DocumentFromToSearchView : AbstractMultiSearchView<SearchDocumentsCriteria>
    {
        public DocumentFromToSearchView(Android.Content.Context context) :
            base(context, Resource.String.search_document_search_for, Resource.String.search_document_search_for_hint, Resource.Array.search_document_from_to)
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
