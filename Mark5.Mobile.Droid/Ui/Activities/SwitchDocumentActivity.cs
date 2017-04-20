//
// Project: Mark5.Mobile.Droid
// File: SwitchDocumentActivity.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class SwitchDocumentActivity : BaseAppCompatActivity
    {

        public const string FolderIdIntentKey = "FolderId_4bd29db4-c529-48a2-bf8f-8f1a96ed60b5";
        public const string FolderIntentKey = "Folder_fc733ef0-68cb-4412-9255-cf128602f176";
        public const string DocumentIdIntentKey = "DocumentId_690fc3d6-ae73-4f5e-844a-06bdc44b6747";
        public const string DocumentPreviewIntentKey = "DocumentPreview_0bd291a4-22a5-431c-ad6e-4c8b273eeb98";
        public const string NotificationGuidIntentKey = "NotificationGuid_0473a08d-5f96-4acd-924a-6d160a23cdf2";

        const int MaxNeighbours = 20;

        const string NeighbourDocumentIdsKey = "neighbourDocumentIdsKey";
        const string FolderKey = "folderKey";

        Folder folder;
        List<int> documentIds = new List<int>(MaxNeighbours * 3);

        Toolbar toolbar;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(SwitchDocumentActivity)}...");

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
                    df.Folder = folder = SerializationUtils.Deserialize<Folder>(Intent.Extras.GetString(FolderIntentKey));

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

                CommonConfig.Logger.Info($"Created {nameof(SwitchDocumentActivity)}");

                if (folder != null && df.DocumentPreview != null)
                {
                    try
                    {
                        documentIds.AddRange(await Managers.DocumentsManager.GetNeighbourDocumentsIdAsync(folder, df.DocumentPreview.Id, true, true, MaxNeighbours));
                    }
                    catch (Exception ex)
                    {
                        CommonConfig.Logger.Error("Error while retrieveing neighbour documents", ex);
                    }
                }
            }
            else
            {
                documentIds = SerializationUtils.Deserialize<List<int>>(savedInstanceState.GetString(NeighbourDocumentIdsKey));

                var serializedFolder = savedInstanceState.GetString(FolderKey);
                folder = !string.IsNullOrEmpty(serializedFolder) ? SerializationUtils.Deserialize<Folder>(serializedFolder) : null;

                CommonConfig.Logger.Info($"Restored {nameof(SwitchDocumentActivity)}");
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutString(NeighbourDocumentIdsKey, SerializationUtils.Serialize(documentIds));
            outState.PutString(FolderKey, folder != null ? SerializationUtils.Serialize(folder) : null);

            base.OnSaveInstanceState(outState);
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.enter_from_left_half, Resource.Animation.exit_to_right);
        }

        public async Task<bool> HasPrevious(int documentId)
        {
            if (folder == null)
                return false;

            var documentIndex = documentIds.FindIndex(d => d == documentId);
            if (documentIndex == -1)
            {
                return false;
            }

            if (documentIndex == 0)
            {
                try
                {
                    var previous = await Managers.DocumentsManager.GetNeighbourDocumentsIdAsync(folder, documentId, true, false, MaxNeighbours);
                    if (previous == null || !previous.Any())
                        return false;

                    documentIds.InsertRange(0, previous);
                    return true;
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error("Error while checking if previous document exists", ex);
                    return false;
                }
            }

            return true;
        }

        public async Task<bool> HasNext(int documentId)
        {
            if (folder == null)
                return false;

            var documentIndex = documentIds.FindIndex(d => d == documentId);
            if (documentIndex == -1)
            {
                return false;
            }

            if (documentIndex == documentIds.Count - 1)
            {
                try
                {
                    var next = await Managers.DocumentsManager.GetNeighbourDocumentsIdAsync(folder, documentId, false, true, MaxNeighbours);
                    if (next == null || !next.Any())
                        return false;

                    documentIds.AddRange(next);
                    return true;
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error("Error while checking if next document exists", ex);
                    return false;
                }
            }

            return true;
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
            ft.SetCustomAnimations(Resource.Animation.fade_in, Resource.Animation.fade_out);
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
            ft.SetCustomAnimations(Resource.Animation.fade_in, Resource.Animation.fade_out);
            ft.Replace(Resource.Id.fragment_container, df, df.GenerateTag());
            ft.Commit();
        }

        int? GetNextId(int documentId)
        {
            var documentIndex = documentIds.FindIndex(d => d == documentId);
            if (documentIndex < documentIds.Count - 1)
                return documentIds[documentIndex + 1];

            return null;
        }

        int? GetPreviousId(int documentId)
        {
            var documentIndex = documentIds.FindIndex(d => d == documentId);
            if (documentIndex > 0)
                return documentIds[documentIndex - 1];

            return null;
        }
    }
}
