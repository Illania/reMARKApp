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
using Android.Provider;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Webkit;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Activities
{
    [Activity]
    public class ComposeDocumentActivity : AppCompatActivity
    {
        Toolbar toolbar;
        ComposeDocumentFragment cdf;

        const string CreationModeFlagIntentKey = "CreationModeFlagIntent_290d1383-175d-4e2d-8f5e-ca899baff3f7";
        const string PreviousDocumentIdIntentKey = "PreviousDocumentIdIntent_a2066147-a27b-454f-bc5c-03e6b8266697";
        const string PreviousDocumentFolderIdIntentKey = "PreviousDocumentFolderIdIntent_ac0d9a31-2ddc-497b-8fbe-7fd5a51b2257";

        const string cdfFragmentTagKey = "fragmentTagKey";
        string cdfFragmentTag;

        public static Intent CreateIntent(Context context, DocumentCreationModeFlag creationModeFlag, int? precedingDocumentId = null,
                                          int? precedingDocumentFolderId = null)
        {
            var intent = new Intent(context, typeof(ComposeDocumentActivity));
            intent.PutExtra(CreationModeFlagIntentKey, (int)creationModeFlag);
            if (precedingDocumentId != null)
            {
                intent.PutExtra(PreviousDocumentIdIntentKey, precedingDocumentId.Value);
            }
            if (precedingDocumentFolderId != null)
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
                var previousDocumentId = Intent.HasExtra(PreviousDocumentIdIntentKey) ? (int?)Intent.Extras.GetInt(PreviousDocumentIdIntentKey) : null;
                var previousDocumentFolderId = Intent.HasExtra(PreviousDocumentFolderIdIntentKey) ? (int?)Intent.Extras.GetInt(PreviousDocumentFolderIdIntentKey) : null;

                var ft = SupportFragmentManager.BeginTransaction();
                cdf = new ComposeDocumentFragment
                {
                    CreationModeFlag = creationModeFlag,
                    PreviousDocumentId = previousDocumentId,
                    PreviousDocumentFolderId = previousDocumentFolderId,
                };
                cdfFragmentTag = cdf.GenerateTag();
                ft.Replace(Resource.Id.fragment_container, cdf, cdfFragmentTag);
                ft.Commit();

                if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadContacts) != Android.Content.PM.Permission.Granted)
                {
                    ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.ReadContacts }, 383);
                }

                CommonConfig.Logger.Info($"Created {nameof(ComposeDocumentActivity)}");
            }
            else
            {
                cdfFragmentTag = savedInstanceState.GetString(cdfFragmentTagKey);
                cdf = SupportFragmentManager.FindFragmentByTag(cdfFragmentTag) as ComposeDocumentFragment;
                CommonConfig.Logger.Info($"Restored {nameof(ComposeDocumentActivity)}");
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutString(cdfFragmentTagKey, cdfFragmentTag);
            base.OnSaveInstanceState(outState);
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

        public override void OnBackPressed()
        {
            cdf.AskIfShouldSaveAsDraft();
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == ComposeDocumentFragment.AttachmentRequestCode)
            {
                if (resultCode != Result.Ok)
                {
                    //TODO error to the user?
                }

                var uri = data.Data;

                var cursor = ContentResolver.Query(uri, null, null, null, null);
                var nameIndex = cursor.GetColumnIndex(OpenableColumns.DisplayName);
                var sizeIndex = cursor.GetColumnIndex(OpenableColumns.Size);
                cursor.MoveToFirst();

                var name = cursor.GetString(nameIndex);
                var size = cursor.GetLong(sizeIndex);
                var mimeType = ContentResolver.GetType(uri);
                var extension = MimeTypeMap.Singleton.GetExtensionFromMimeType(mimeType);

                var stream = ContentResolver.OpenInputStream(uri);
                var a = stream.Position;

                var attachment = new Attachment
                {
                    Filename = name,
                    Extension = extension,
                    Size = (int)size,
                    Stream = stream,
                };

                cdf.LoadAttachment(attachment);
            }
        }

    }
}
