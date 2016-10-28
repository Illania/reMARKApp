//
// Project: Mark5.Mobile.Droid
// File: WithAttachmentsOnlySearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public class WithAttachmentsOnlySearchView : AbstractCheckboxSearchView<SearchDocumentsCriteria>
    {

        public WithAttachmentsOnlySearchView(Context context)
            : base(context)
        {
            TitleTextView.SetText(Resource.String.search_with_attachments_only);
        }

        public override void FromCriteria(SearchDocumentsCriteria criteria)
        {
            Checkbox.Checked = criteria.HavingAttachmentsOnly;
        }

        public override void ToCriteria(SearchDocumentsCriteria criteria)
        {
            criteria.HavingAttachmentsOnly = Checkbox.Checked;
        }
    }
}
