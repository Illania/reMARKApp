//
// Project: Mark5.Mobile.Droid
// File: DocumentFoldersSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public class DocumentFoldersSearchView : FoldersSearchView<SearchDocumentsCriteria>
    {

        public DocumentFoldersSearchView(Context context)
            : base(context)
        {
        }

        public override void FromCriteria(SearchDocumentsCriteria criteria)
        {
            // TODO
        }

        public override void ToCriteria(SearchDocumentsCriteria criteria)
        {
            criteria.FiledInFolderFolderType = FiledInFolderFolderType.Any;
        }
    }
}
