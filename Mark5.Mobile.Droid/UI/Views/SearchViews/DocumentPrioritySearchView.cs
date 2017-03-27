//
// Project: Mark5.Mobile.Droid
// File: DocumentPrioritySearchView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System.Collections.Generic;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public class DocumentPrioritySearchView : AbstractDropdownSearchView<SearchDocumentsCriteria, Priority>
    {
        public DocumentPrioritySearchView(Android.Content.Context context)
            : base(context, Resource.String.search_document_priorities, Resource.String.search_document_priorities_none_selected)
        {
            Values = new List<Priority> { Priority.Urgent, Priority.Normal, Priority.Low };
        }

        protected override void ClickAction()
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
