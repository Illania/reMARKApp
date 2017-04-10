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
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Activities
{

    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class DocumentActivity : BaseAppCompatActivity
    {

        public const string FolderIdIntentKey = "FolderId_4bd29db4-c529-48a2-bf8f-8f1a96ed60b5";
        public const string FolderIntentKey = "Folder_fc733ef0-68cb-4412-9255-cf128602f176";
        public const string DocumentIdIntentKey = "DocumentId_690fc3d6-ae73-4f5e-844a-06bdc44b6747";
        public const string DocumentPreviewIntentKey = "DocumentPreview_0bd291a4-22a5-431c-ad6e-4c8b273eeb98";
        public const string NotificationGuidIntentKey = "NotificationGuid_0473a08d-5f96-4acd-924a-6d160a23cdf2";

        Toolbar toolbar;

        Folder folder;
        List<int> documentIds;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(DocumentActivity)}...");

            OverridePendingTransition(Resource.Animation.enter_from_right, Resource.Animation.exit_to_left_half);

            SetTitle(Resource.String.document);
            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var df = new DocumentFragment();

                if (Intent.HasExtra(FolderIdIntentKey))
                    df.FolderId = Intent.Extras.GetInt(FolderIdIntentKey);

                if (Intent.HasExtra(FolderIntentKey))
                {
                    folder = SerializationUtils.Deserialize<Folder>(Intent.Extras.GetString(FolderIntentKey));
                    df.Folder = folder;
                }

                if (Intent.HasExtra(DocumentIdIntentKey))
                    df.DocumentId = Intent.Extras.GetInt(DocumentIdIntentKey);

                if (Intent.HasExtra(DocumentPreviewIntentKey))
                    df.DocumentPreview = SerializationUtils.Deserialize<DocumentPreview>(Intent.Extras.GetString(DocumentPreviewIntentKey));

                if (Intent.HasExtra(NotificationGuidIntentKey))
                    df.NotificationGuid = SerializationUtils.Deserialize<Guid>(Intent.Extras.GetString(NotificationGuidIntentKey));

                df.CloseRequest = OnBackPressed;

                var ft = SupportFragmentManager.BeginTransaction();
                ft.Replace(Resource.Id.fragment_container, df, df.GenerateTag());
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(DocumentActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(DocumentActivity)}");
            }

            documentIds = await Managers.DocumentsManager.GetDocumentsIdAsync(folder);
        }

        public bool HasPrevious(int documentId) //TODO check if ordered in the query
        {
            return GetPreviousId(documentId) != null;
        }

        public bool HasNext(int documentId)
        {
            return GetNextId(documentId) != null;
        }

        public void GoToPrevious(int documentId)
        {
            var previousId = GetPreviousId(documentId);
            if (previousId == null)
                return;

            var df = new DocumentFragment
            {
                Folder = folder,
                DocumentId = previousId,
                CloseRequest = OnBackPressed,
            };

            var ft = SupportFragmentManager.BeginTransaction();
            ft.Replace(Resource.Id.fragment_container, df, df.GenerateTag());
            ft.Commit();
        }

        public void GoToNext(int documentId)
        {
            var nextId = GetNextId(documentId);
            if (nextId == null)
                return;

            var df = new DocumentFragment
            {
                Folder = folder,
                DocumentId = nextId,
                CloseRequest = OnBackPressed,
            };

            var ft = SupportFragmentManager.BeginTransaction();
            ft.Replace(Resource.Id.fragment_container, df, df.GenerateTag());
            ft.Commit();
        }

        int? GetNextId(int documentId)
        {
            var documentIndex = documentIds.FindIndex(d => d == documentId);
            if (documentIndex < documentIds.Count - 1)
            {
                return documentIds[documentIndex + 1];
            }

            return null;
        }

        int? GetPreviousId(int documentId)
        {
            var documentIndex = documentIds.FindIndex(d => d == documentId);
            if (documentIndex > 0)
            {
                return documentIds[documentIndex - 1];
            }

            return null;
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.enter_from_left_half, Resource.Animation.exit_to_right);
        }

    }
}

