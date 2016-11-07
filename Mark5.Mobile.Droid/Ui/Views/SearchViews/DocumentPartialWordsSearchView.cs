//
// Project: Mark5.Mobile.Droid
// File: PartialWordsSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public class DocumentPartialWordsSearchView : AbstractCheckboxSearchView<SearchDocumentsCriteria>
    {

        public DocumentPartialWordsSearchView(Context context)
            : base(context)
        {
            TitleTextView.SetText(Resource.String.search_document_partial);
        }

        public override void FromCriteria(SearchDocumentsCriteria criteria)
        {
            Checkbox.Checked = criteria.PartialWordSearch;
        }

        public override void ToCriteria(SearchDocumentsCriteria criteria)
        {
            criteria.PartialWordSearch = Checkbox.Checked;
        }
    }
}
