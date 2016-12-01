//
// Project: Mark5.Mobile.Droid
// File: DocumentFoldersSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public class DocumentFoldersSearchView : AbstractFoldersSearchView<SearchDocumentsCriteria>
    {

        public DocumentFoldersSearchView(Context context)
            : base(context)
        {
            Click += (sender, e) =>
            {
                var i = new Intent(context, typeof(FolderListSelectionActivity));
                i.PutExtra(FolderListSelectionActivity.ModeIntentKey, (int)FolderListSelectionActivity.ModeType.Picker);
                i.PutExtra(FolderListSelectionActivity.ModuleIntentKey, SerializationUtils.Serialize(ModuleType.Documents));
                context.StartActivity(i);
            };
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
