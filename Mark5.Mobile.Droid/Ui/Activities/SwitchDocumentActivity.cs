using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Analytics;
using Mark5.Mobile.Common.Manager;
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

        public static Intent CreateIntent(Context context, int? folderId = null, Folder folder = null, int? documentId = null, DocumentPreview documentPreview = null, Guid? guid = null)
        {
            var intent = new Intent(context, typeof(SwitchDocumentActivity));

            if (folderId != null)
                intent.PutExtra(FolderIdIntentKey, folderId.Value);

            if (folder != null)
                intent.PutExtra(FolderIntentKey, Serializer.Serialize(folder));

            if (documentId != null)
                intent.PutExtra(DocumentIdIntentKey, documentId.Value);

            if (documentPreview != null)
                intent.PutExtra(DocumentPreviewIntentKey, Serializer.Serialize(documentPreview));

            if (guid != null)
                intent.PutExtra(NotificationGuidIntentKey, Serializer.Serialize(guid.Value));

            return intent;
        }

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
                int? folderId = null;
                Folder folder = null;
                int? documentId = null;
                DocumentPreview documentPreview = null;
                Guid? notificationGuid = null;

                if (Intent.HasExtra(FolderIdIntentKey))
                    folderId = Intent.Extras.GetInt(FolderIdIntentKey);

                if (Intent.HasExtra(FolderIntentKey))
                    folder = Serializer.Deserialize<Folder>(Intent.Extras.GetString(FolderIntentKey));

                if (Intent.HasExtra(DocumentIdIntentKey))
                    documentId = Intent.Extras.GetInt(DocumentIdIntentKey);

                if (Intent.HasExtra(DocumentPreviewIntentKey))
                    documentPreview = Serializer.Deserialize<DocumentPreview>(Intent.Extras.GetString(DocumentPreviewIntentKey));

                if (Intent.HasExtra(NotificationGuidIntentKey))
                    notificationGuid = Serializer.Deserialize<Guid>(Intent.Extras.GetString(NotificationGuidIntentKey));

                var (df, tag) = DocumentFragment.NewInstance(folder, folderId, documentPreview, documentId, notificationGuid);

                var ft = SupportFragmentManager.BeginTransaction();
                ft.Replace(Resource.Id.fragment_container, df, tag);
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(SwitchDocumentActivity)}");

                if (folder != null && documentPreview != null)
                    try
                    {
                        documentIds.AddRange(await Managers.DocumentsManager.GetNeighbourDocumentsIdAsync(folder, documentPreview.Id, true, true, MaxNeighbours));
                    }
                    catch (Exception ex)
                    {
                        CommonConfig.Logger.Error("Error while retrieveing neighbour documents", ex);
                    }
            }
            else
            {
                documentIds = Serializer.Deserialize<List<int>>(savedInstanceState.GetString(NeighbourDocumentIdsKey));

                var serializedFolder = savedInstanceState.GetString(FolderKey);
                folder = !string.IsNullOrEmpty(serializedFolder) ? Serializer.Deserialize<Folder>(serializedFolder) : null;

                CommonConfig.Logger.Info($"Restored {nameof(SwitchDocumentActivity)}");
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutString(NeighbourDocumentIdsKey, Serializer.Serialize(documentIds));
            outState.PutString(FolderKey, folder != null ? Serializer.Serialize(folder) : null);

            base.OnSaveInstanceState(outState);
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.enter_from_left_half, Resource.Animation.exit_to_right);
        }

        public void GoToPrevious(int documentId)
        {
            AnalyticsManager.LogEvent(new DocumentQuickSwitchEvent());

            var previousId = GetPreviousId(documentId);
            if (previousId == null)
                return;

            var (df, tag) = DocumentFragment.NewInstance(folder, docId: previousId);

            var ft = SupportFragmentManager.BeginTransaction();

            ft.SetCustomAnimations(Resource.Animation.fade_in, Resource.Animation.fade_out);
            ft.Replace(Resource.Id.fragment_container, df, tag);
            ft.Commit();
        }

        public void GoToNext(int documentId)
        {
            AnalyticsManager.LogEvent(new DocumentQuickSwitchEvent());

            var nextId = GetNextId(documentId);
            if (nextId == null)
                return;

            var (df, tag) = DocumentFragment.NewInstance(folder, docId: nextId);

            var ft = SupportFragmentManager.BeginTransaction();
            ft.SetCustomAnimations(Resource.Animation.fade_in, Resource.Animation.fade_out);
            ft.Replace(Resource.Id.fragment_container, df, tag);
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

        public async Task<bool> HasPrevious(int documentId)
        {
            if (folder == null)
                return false;

            var documentIndex = documentIds.FindIndex(d => d == documentId);
            if (documentIndex == -1)
                return false;

            if (documentIndex == 0)
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

            return true;
        }

        public async Task<bool> HasNext(int documentId)
        {
            if (folder == null)
                return false;

            var documentIndex = documentIds.FindIndex(d => d == documentId);
            if (documentIndex == -1)
                return false;

            if (documentIndex == documentIds.Count - 1)
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

            return true;
        }
    }
}