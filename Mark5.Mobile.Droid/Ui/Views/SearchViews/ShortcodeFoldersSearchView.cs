//
// Project: Mark5.Mobile.Droid
// File: ShortcodeFoldersSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public class ShortcodeFoldersSearchView : FoldersSearchView<SearchShortcodesCriteria>
    {

        public ShortcodeFoldersSearchView(Context context)
            : base(context)
        {
        }

        public override void FromCriteria(SearchShortcodesCriteria criteria)
        {
            // TODO
        }

        public override void ToCriteria(SearchShortcodesCriteria criteria)
        {
            criteria.FiledInFolderFolderType = FiledInFolderFolderType.Any;
        }
    }
}
