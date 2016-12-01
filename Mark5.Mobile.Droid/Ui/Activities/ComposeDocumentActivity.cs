//
// Project: Mark5.Mobile.Droid
// File: ComposeDocumentActivity.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//

using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
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
        const string PreviousDocumentDirectionIntentKey = "PreviousDocumentDirectionIntent_edefdcd2-764f-439d-891b-178b8de29333";
        const string OutgoingDocumentGuidIntentKey = "OutgoingDocumentGuidIntent_7901fa2b-f096-4e9e-82b9-5aeae9f39d05";

        const string cdfFragmentTagKey = "fragmentTagKey";
        string cdfFragmentTag;

        public static Intent CreateIntent(Context context, DocumentCreationModeFlag creationModeFlag, DocumentDirection previousDocumentDirection, int? precedingDocumentId = null,
                                          int? precedingDocumentFolderId = null, Guid outgoingDocumentGuid = default(Guid))
        {
            var intent = new Intent(context, typeof(ComposeDocumentActivity));
            intent.PutExtra(CreationModeFlagIntentKey, (int)creationModeFlag);
            intent.PutExtra(PreviousDocumentDirectionIntentKey, (int)previousDocumentDirection);
            if (outgoingDocumentGuid != default(Guid))
            {
                intent.PutExtra(OutgoingDocumentGuidIntentKey, outgoingDocumentGuid.ToString());
            }
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
                cdf = new ComposeDocumentFragment();

                if (Intent.HasExtra(CreationModeFlagIntentKey))
                    cdf.CreationModeFlag = (DocumentCreationModeFlag)Intent.Extras.GetInt(CreationModeFlagIntentKey);

                if (Intent.HasExtra(PreviousDocumentDirectionIntentKey))
                    cdf.PreviousDocumentDirection = (DocumentDirection)Intent.Extras.GetInt(PreviousDocumentDirectionIntentKey);

                if (Intent.HasExtra(PreviousDocumentIdIntentKey))
                    cdf.PreviousDocumentId = Intent.Extras.GetInt(PreviousDocumentIdIntentKey);

                if (Intent.HasExtra(PreviousDocumentFolderIdIntentKey))
                    cdf.PreviousDocumentFolderId = Intent.Extras.GetInt(PreviousDocumentFolderIdIntentKey);

                if (Intent.HasExtra(OutgoingDocumentGuidIntentKey))
                {
                    cdf.OutgoingDocumentGuid = new Guid(Intent.Extras.GetString(OutgoingDocumentGuidIntentKey));
                    cdf.LocalDocument = true;
                }
                else
                {
                    cdf.LocalDocument = false;
                }

                cdfFragmentTag = cdf.GenerateTag();

                var ft = SupportFragmentManager.BeginTransaction();
                ft.Replace(Resource.Id.fragment_container, cdf, cdfFragmentTag);
                ft.Commit();

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
            cdf.AskIfShouldSave();
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == ComposeDocumentFragment.AttachmentRequestCode && resultCode == Result.Ok)
            {
                cdf.HandleLocalAttachment(data);
            }
        }

    }
}
