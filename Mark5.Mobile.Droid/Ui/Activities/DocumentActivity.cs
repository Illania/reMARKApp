using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Analytics;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Activities
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class DocumentActivity : BaseAppCompatActivity
    {
        public const string FailedDocumentToUploadGuidIntentKey = "FailedDocumentToUploadGuid_d76eb08a-1873-49f2-8e91-b6bc80417ccf";
        public const string FolderIdIntentKey = "FolderId_4bd29db4-c529-48a2-bf8f-8f1a96ed60b5";
        public const string FolderIntentKey = "Folder_fc733ef0-68cb-4412-9255-cf128602f176";
        public const string DocumentIdIntentKey = "DocumentId_690fc3d6-ae73-4f5e-844a-06bdc44b6747";
        public const string DocumentPreviewIntentKey = "DocumentPreview_0bd291a4-22a5-431c-ad6e-4c8b273eeb98";
        public const string NotificationGuidIntentKey = "NotificationGuid_0473a08d-5f96-4acd-924a-6d160a23cdf2";

        Toolbar toolbar;

        public static Intent CreateIntent(Context context, string failedDocumentToUploadGuid = null, int? folderId = null, int? documentId = null, string documentPreview = null, string notificationGuid = null)
        {
            var intent = new Intent(context, typeof(DocumentActivity));

            if (failedDocumentToUploadGuid != null)
                intent.PutExtra(FailedDocumentToUploadGuidIntentKey, failedDocumentToUploadGuid);

            if (folderId != null)
                intent.PutExtra(FolderIdIntentKey, folderId.Value);

            if (documentId != null)
                intent.PutExtra(DocumentIdIntentKey, documentId.Value);

            if (documentPreview != null)
                intent.PutExtra(DocumentPreviewIntentKey, Serializer.Serialize(documentPreview));

            if (notificationGuid != null)
                intent.PutExtra(NotificationGuidIntentKey, notificationGuid);

            return intent;
        }

        protected override void OnCreate(Bundle savedInstanceState)
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
                Guid? failedDocumentToUploadGuid = null;
                int? folderId = null;
                Folder folder = null;
                int? documentId = null;
                DocumentPreview documentPreview = null;
                Guid? notificationGuid = null;

                if (Intent.HasExtra(FailedDocumentToUploadGuidIntentKey))
                    failedDocumentToUploadGuid = Guid.Parse(Intent.Extras.GetString(FailedDocumentToUploadGuidIntentKey));

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

                if (documentPreview?.Direction == DocumentDirection.External)
                    CommonConfig.UsageAnalytics.LogEvent(new OpenDocumentEvent(true));
                else
                    CommonConfig.UsageAnalytics.LogEvent(new OpenDocumentEvent(false));

                var (df, tag) = DocumentFragment.NewInstance(folder, folderId, documentPreview, documentId, notificationGuid, failedDocumentToUploadGuid);

                var ft = SupportFragmentManager.BeginTransaction();
                ft.Replace(Resource.Id.fragment_container, df, tag);
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(DocumentActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(DocumentActivity)}");
            }
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.enter_from_left_half, Resource.Animation.exit_to_right);
        }
    }
}