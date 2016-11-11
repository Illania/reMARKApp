//
// Project: Mark5.Mobile.Droid
// File: ComposeDocumentActivity.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//

using Android;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
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
        const string PreviousDocumentPreviewIntentKey = "PreviousDocumentPreviewIntent_d3e7ce92-3b0d-4d6b-882b-88bf4ba7bf24";
        const string PreviousDocumentIntentKey = "PreviousDocumentIntent_a2066147-a27b-454f-bc5c-03e6b8266697";
        const string PreviousDocumentFolderIdIntentKey = "PreviousDocumentFolderIdIntent_ac0d9a31-2ddc-497b-8fbe-7fd5a51b2257";

        public static Intent CreateIntent(Context context, DocumentCreationModeFlag creationModeFlag, DocumentPreview documentPreview = null, Document document = null,
                                         int? precedingDocumentId = null, int? precedingDocumentFolderId = null)
        {
            var intent = new Intent(context, typeof(ComposeDocumentActivity));
            intent.PutExtra(CreationModeFlagIntentKey, (int)creationModeFlag);
            if (documentPreview != null)
            {
                intent.PutExtra(PreviousDocumentPreviewIntentKey, SerializationUtils.Serialize(documentPreview));
            }
            if (document != null)
            {
                intent.PutExtra(PreviousDocumentIntentKey, SerializationUtils.Serialize(document));
            }
            if (precedingDocumentId != null)
            {
                intent.PutExtra(PreviousDocumentFolderIdIntentKey, precedingDocumentFolderId.Value);
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
                var previousDocumentPreview = Intent.HasExtra(PreviousDocumentPreviewIntentKey) ? SerializationUtils.Deserialize<DocumentPreview>(Intent.Extras.GetString(PreviousDocumentPreviewIntentKey)) : null;
                var previousDocument = Intent.HasExtra(PreviousDocumentIntentKey) ? SerializationUtils.Deserialize<Document>(Intent.Extras.GetString(PreviousDocumentIntentKey)) : null;
                var previousDocumentFolderId = Intent.HasExtra(PreviousDocumentFolderIdIntentKey) ? (int?)Intent.Extras.GetInt(PreviousDocumentFolderIdIntentKey) : null;

                var ft = SupportFragmentManager.BeginTransaction();
                cdf = new ComposeDocumentFragment
                {
                    CreationModeFlag = creationModeFlag,
                    PreviousDocumentPreview = previousDocumentPreview,
                    PreviousDocument = previousDocument,
                    PreviousDocumentFolderId = previousDocumentFolderId,
                };
                ft.Replace(Resource.Id.fragment_container, cdf, cdf.GenerateTag());
                ft.Commit();

                ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.ReadContacts }, 383);

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
