//
// Project: Mark5.Mobile.Droid
// File: SearchInAttachmentsSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public class SearchInAttachmentsSearchView : AbstractCheckboxSearchView<SearchDocumentsCriteria>
    {

        public SearchInAttachmentsSearchView(Context context)
            : base(context)
        {
            TitleTextView.SetText(Resource.String.search_in_attachments);
        }

        public override void FromCriteria(SearchDocumentsCriteria criteria)
        {
            Checkbox.Checked = criteria.SearchInAttachments;
        }

        public override void ToCriteria(SearchDocumentsCriteria criteria)
        {
            criteria.SearchInAttachments = Checkbox.Checked;
        }
    }
}
