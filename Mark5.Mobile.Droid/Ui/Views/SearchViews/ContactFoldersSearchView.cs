//
// Project: Mark5.Mobile.Droid
// File: ContactFoldersSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public class ContactFoldersSearchView : AbstractFoldersSearchView<SearchContactsCriteria>
    {

        public ContactFoldersSearchView(Context context)
            : base(context)
        {
        }

        public override void FromCriteria(SearchContactsCriteria criteria)
        {
            // TODO
        }

        public override void ToCriteria(SearchContactsCriteria criteria)
        {
            criteria.FiledInFolderFolderType = FiledInFolderFolderType.Any;
        }
    }
}
