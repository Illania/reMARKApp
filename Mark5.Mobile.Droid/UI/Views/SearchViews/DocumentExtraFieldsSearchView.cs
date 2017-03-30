//
// Project: Mark5.Mobile.Droid
// File: DocumentExtraFieldsSearchView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public class DocumentExtraFieldsSearchView : AbstractEditableTextSearchView<SearchDocumentsCriteria>
    {
        public DocumentExtraFieldsSearchView(Android.Content.Context context)
            : base(context, Resource.String.search_document_extra_fields)
        {
        }

        public override void Refresh()
        {
            SetText(Criteria.ExtraFields);
        }

        public override void UpdateCriteria()
        {
            Criteria.ExtraFields = GetText();
        }
    }
}
