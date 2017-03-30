//
// Project: Mark5.Mobile.Droid
// File: DocumentFromToSearchView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using Mark5.Mobile.Common.Model;

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
            Spinner.SetSelection((int)Criteria.FromToClause);
            BottomEditText.Text = Criteria.FromToField;
        }

        public override void UpdateCriteria()
        {
            Criteria.FromToClause = (FromToClause)Spinner.SelectedItemPosition;
            Criteria.FromToField = BottomEditText.Text;
        }
    }
}
