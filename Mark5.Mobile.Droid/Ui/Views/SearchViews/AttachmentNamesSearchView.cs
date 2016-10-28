//
// Project: Mark5.Mobile.Droid
// File: AttachmentNamesSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public class AttachmentNamesSearchView : AbstractEditTextSearchView<SearchDocumentsCriteria>
    {

        public AttachmentNamesSearchView(Context context)
            : base(context)
        {
            EditText.SetHint(Resource.String.search_attachment_names);
        }

        public override void FromCriteria(SearchDocumentsCriteria criteria)
        {
            EditText.Text = criteria.Comment;
        }

        public override void ToCriteria(SearchDocumentsCriteria criteria)
        {
            criteria.Comment = EditText.Text;
        }
    }
}
