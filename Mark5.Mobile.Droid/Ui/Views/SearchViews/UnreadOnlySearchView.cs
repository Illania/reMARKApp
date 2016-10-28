//
// Project: Mark5.Mobile.Droid
// File: UnreadOnlySearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public class UnreadOnlySearchView : AbstractCheckboxSearchView<SearchDocumentsCriteria>
    {

        public UnreadOnlySearchView(Context context)
            : base(context)
        {
            TitleTextView.SetText(Resource.String.search_unread_only);
        }

        public override void FromCriteria(SearchDocumentsCriteria criteria)
        {
            Checkbox.Checked = criteria.UnreadOnly;
        }

        public override void ToCriteria(SearchDocumentsCriteria criteria)
        {
            criteria.UnreadOnly = Checkbox.Checked;
        }
    }
}
