//
// Project: Mark5.Mobile.Droid
// File: ComposeDocumentActivity.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//

using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Activities
{
    [Activity]
    public class ComposeDocumentActivity : AppCompatActivity
    {
        Toolbar toolbar;
        ComposeDocumentFragment cdf;

        const string CreationModeFlagIntentKey = "CreationModeFlagIntent_290d1383-175d-4e2d-8f5e-ca899baff3f7";
        const string DocumentPreviewIntentKey = "DocumentPreviewIntent_d3e7ce92-3b0d-4d6b-882b-88bf4ba7bf24";
        const string DocumentIntentKey = "DocumentIntent_a2066147-a27b-454f-bc5c-03e6b8266697";
        const string PrecedingDocumentIdIntentKey = "PrecedingDocumentIdIntent_1a6f3c5c-f54c-43c9-a9ce-8041fdcad7c5";
        const string PrecedingDocumentFolderIdIntentKey = "PrecedingDocumentFolderIdIntent_ac0d9a31-2ddc-497b-8fbe-7fd5a51b2257";

        public static Intent CreateIntent(Context context, DocumentCreationModeFlag creationModeFlag, DocumentPreview documentPreview = null, Document document = null,
                                         int? precedingDocumentId = null, int? precedingDocumentFolderId = null)
        {
            var intent = new Intent(context, typeof(ComposeDocumentActivity));
            intent.PutExtra(CreationModeFlagIntentKey, (int)creationModeFlag);
            if (documentPreview != null)
            {
                intent.PutExtra(DocumentPreviewIntentKey, SerializationUtils.Serialize(documentPreview));
            }
            if (document != null)
            {
                intent.PutExtra(DocumentIntentKey, SerializationUtils.Serialize(document));
            }
            if (precedingDocumentId != null)
            {
                intent.PutExtra(PrecedingDocumentIdIntentKey, precedingDocumentId.Value);
            }
            if (precedingDocumentId != null)
            {
                intent.PutExtra(PrecedingDocumentFolderIdIntentKey, precedingDocumentFolderId.Value);
            }

            return intent;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(ComposeDocumentActivity)}...");

            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var creationModeFlag = (DocumentCreationModeFlag)Intent.Extras.GetInt(CreationModeFlagIntentKey);
                var documentPreview = Intent.HasExtra(DocumentPreviewIntentKey) ? SerializationUtils.Deserialize<DocumentPreview>(Intent.Extras.GetString(DocumentPreviewIntentKey)) : null;
                var document = Intent.HasExtra(DocumentIntentKey) ? SerializationUtils.Deserialize<Document>(Intent.Extras.GetString(DocumentIntentKey)) : null;
                var precedingDocumentId = Intent.HasExtra(PrecedingDocumentIdIntentKey) ? (int?)Intent.Extras.GetInt(PrecedingDocumentIdIntentKey) : null;
                var precedingDocumentFolderId = Intent.HasExtra(PrecedingDocumentFolderIdIntentKey) ? (int?)Intent.Extras.GetInt(PrecedingDocumentFolderIdIntentKey) : null;

                var ft = SupportFragmentManager.BeginTransaction();
                cdf = new ComposeDocumentFragment
                {
                    CreationModeFlag = creationModeFlag,
                    DocumentPreview = documentPreview,
                    Document = document,
                    PrecedingDocumentId = precedingDocumentId,
                    PrecedingDocumentFolderId = precedingDocumentFolderId,
                };
                ft.Replace(Resource.Id.fragment_container, cdf, cdf.GenerateTag());
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(ComposeDocumentActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(ComposeDocumentActivity)}");
            }
        }

        public override bool OnOptionsItemSelected(Android.Views.IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
            {
                OnBackPressed();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

    }
}
