//
// Project: Mark5.Mobile.Droid
// File: DocumentActivity.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System;
using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Activities
{

    [Activity]
    public class DocumentActivity : BaseAppCompatActivity
    {

        public const string FolderIdIntentKey = "FolderId_4bd29db4-c529-48a2-bf8f-8f1a96ed60b5";
        public const string FolderIntentKey = "Folder_fc733ef0-68cb-4412-9255-cf128602f176";
        public const string SearchIdIntentKey = "SearchId_fe483a14-0042-4fe2-a887-c232b332a715";
        public const string DocumentIdIntentKey = "DocumentId_690fc3d6-ae73-4f5e-844a-06bdc44b6747";
        public const string DocumentPreviewIntentKey = "DocumentPreview_0bd291a4-22a5-431c-ad6e-4c8b273eeb98";
        public const string ReadOnlyModeIntentKey = "ReadOnlyMode_c23890cf-06fc-45d7-86c8-76c4c8027daf";
        public const string NotificationGuidIntentKey = "NotificationGuid_0473a08d-5f96-4acd-924a-6d160a23cdf2";

        const string dfFragmentTagKey = "fragmentTagKey";
        string dfFragmentTag;

        Toolbar toolbar;
        DocumentFragment df;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(DocumentActivity)}...");

            SetTitle(Resource.String.document);
            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                df = new DocumentFragment();

                if (Intent.HasExtra(FolderIdIntentKey))
                    df.FolderId = Intent.Extras.GetInt(FolderIdIntentKey);

                if (Intent.HasExtra(FolderIntentKey))
                    df.Folder = SerializationUtils.Deserialize<Folder>(Intent.Extras.GetString(FolderIntentKey));

                if (Intent.HasExtra(SearchIdIntentKey))
                    df.SearchId = Intent.Extras.GetInt(SearchIdIntentKey);

                if (Intent.HasExtra(DocumentIdIntentKey))
                    df.DocumentId = Intent.Extras.GetInt(DocumentIdIntentKey);

                if (Intent.HasExtra(DocumentPreviewIntentKey))
                    df.DocumentPreview = SerializationUtils.Deserialize<DocumentPreview>(Intent.Extras.GetString(DocumentPreviewIntentKey));

                if (Intent.HasExtra(ReadOnlyModeIntentKey))
                    df.ReadOnlyMode = Intent.Extras.GetBoolean(ReadOnlyModeIntentKey);

                if (Intent.HasExtra(NotificationGuidIntentKey))
                    df.NotificationGuid = SerializationUtils.Deserialize<Guid>(Intent.Extras.GetString(NotificationGuidIntentKey));

                df.CloseRequest = OnBackPressed;

                var ft = SupportFragmentManager.BeginTransaction();
                dfFragmentTag = df.GenerateTag();
                ft.Replace(Resource.Id.fragment_container, df, dfFragmentTag);
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(DocumentActivity)}");
            }
            else
            {
                dfFragmentTag = savedInstanceState.GetString(dfFragmentTagKey);
                df = SupportFragmentManager.FindFragmentByTag(dfFragmentTag) as DocumentFragment;
                CommonConfig.Logger.Info($"Restored {nameof(DocumentActivity)}");
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Android.Content.Intent data)
        {
            if (resultCode == Result.Ok && df != null)
            {
                if (requestCode == DocumentFragment.RequestCodes.CommentsRequest)
                {
                    var comments = SerializationUtils.Deserialize<List<Comment>>(data.GetStringExtra(CommentsListActivity.CommentsResultKey));
                    df.UpdateComments(comments);
                }
                else if (requestCode == DocumentFragment.RequestCodes.CategoriesRequest)
                {
                    var categories = SerializationUtils.Deserialize<List<Category>>(data.GetStringExtra(CategoriesListActivity.CategoriesResultKey));
                    df.UpdateCategories(categories);
                }
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutString(dfFragmentTagKey, dfFragmentTag);
            base.OnSaveInstanceState(outState);
        }
    }
}

