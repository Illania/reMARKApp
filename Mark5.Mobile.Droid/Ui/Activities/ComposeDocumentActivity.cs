using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Activities
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class ComposeDocumentActivity : AppCompatActivity
    {
        Toolbar toolbar;
        ComposeDocumentFragment cdf;

        const string CreationModeFlagIntentKey = "CreationModeFlagIntent_290d1383-175d-4e2d-8f5e-ca899baff3f7";
        const string PreviousDocumentIdIntentKey = "PreviousDocumentIdIntent_a2066147-a27b-454f-bc5c-03e6b8266697";
        const string PreviousDocumentFolderIdIntentKey = "PreviousDocumentFolderIdIntent_ac0d9a31-2ddc-497b-8fbe-7fd5a51b2257";
        const string PreviousDocumentDirectionIntentKey = "PreviousDocumentDirectionIntent_edefdcd2-764f-439d-891b-178b8de29333";
        const string OutgoingDocumentGuidIntentKey = "OutgoingDocumentGuidIntent_7901fa2b-f096-4e9e-82b9-5aeae9f39d05";
        const string PreconfiguredEmailToAddressesIntentKey = "PreconfiguredEmailToAddressesIntent_25ff402c-268e-477c-890c-80d68e60ab01";
        const string PreconfiguredEmailCcAddressesIntentKey = "PreconfiguredEmailCcAddressesIntent_051636c4-f032-4736-9d05-b9c0427bba5b";
        const string PreconfiguredEmailBccAddressesIntentKey = "PreconfiguredEmailBccAddressesIntent_c7d5b5ce-497c-460d-bcbd-331b3e01b656";
        const string CopyToNewOptionsIntentKey = "CopyToNewOptionsIntent_f298d024-4df0-431d-ad3d-1834eb0dede0";

        const string cdfFragmentTagKey = "fragmentTagKey";
        string cdfFragmentTag;

        public static Intent CreateIntent(Context context,
                                          DocumentCreationModeFlag creationModeFlag,
                                          DocumentDirection previousDocumentDirection,
                                          int? precedingDocumentId = null,
                                          int? precedingDocumentFolderId = null,
                                          Guid outgoingDocumentGuid = default(Guid),
                                          List<string> preconfiguredEmailToAddresses = null,
                                          List<string> preconfiguredEmailCcAddresses = null,
                                          List<string> preconfiguredEmailBccAddresses = null,
                                          CopyToNewOption copyToNewOptions = CopyToNewOption.None)
        {
            var intent = new Intent(context, typeof(ComposeDocumentActivity));
            intent.PutExtra(CreationModeFlagIntentKey, (int)creationModeFlag);
            intent.PutExtra(PreviousDocumentDirectionIntentKey, (int)previousDocumentDirection);
            intent.PutExtra(CopyToNewOptionsIntentKey, (int)copyToNewOptions);

            if (precedingDocumentId != null)
                intent.PutExtra(PreviousDocumentIdIntentKey, precedingDocumentId.Value);

            if (precedingDocumentFolderId != null)
                intent.PutExtra(PreviousDocumentFolderIdIntentKey, precedingDocumentFolderId.Value);

            if (outgoingDocumentGuid != default(Guid))
                intent.PutExtra(OutgoingDocumentGuidIntentKey, outgoingDocumentGuid.ToString());

            if (preconfiguredEmailToAddresses != null)
                intent.PutExtra(PreconfiguredEmailToAddressesIntentKey, preconfiguredEmailToAddresses.ToArray());

            if (preconfiguredEmailCcAddresses != null)
                intent.PutExtra(PreconfiguredEmailCcAddressesIntentKey, preconfiguredEmailCcAddresses.ToArray());

            if (preconfiguredEmailBccAddresses != null)
                intent.PutExtra(PreconfiguredEmailBccAddressesIntentKey, preconfiguredEmailBccAddresses.ToArray());

            return intent;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(ComposeDocumentActivity)}...");

            OverridePendingTransition(Resource.Animation.slide_up, Resource.Animation.no_change);

            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                cdf = new ComposeDocumentFragment();

                if (Intent.HasExtra(CreationModeFlagIntentKey))
                    cdf.CreationModeFlag = (DocumentCreationModeFlag) Intent.Extras.GetInt(CreationModeFlagIntentKey);

                if (Intent.HasExtra(PreviousDocumentDirectionIntentKey))
                    cdf.PreviousDocumentDirection = (DocumentDirection) Intent.Extras.GetInt(PreviousDocumentDirectionIntentKey);

                if (Intent.HasExtra(PreviousDocumentIdIntentKey))
                    cdf.PreviousDocumentId = Intent.Extras.GetInt(PreviousDocumentIdIntentKey);

                if (Intent.HasExtra(PreviousDocumentFolderIdIntentKey))
                    cdf.PreviousDocumentFolderId = Intent.Extras.GetInt(PreviousDocumentFolderIdIntentKey);

                if (Intent.HasExtra(PreconfiguredEmailToAddressesIntentKey))
                    cdf.PreconfiguredEmailToAddresses = Intent.Extras.GetStringArray(PreconfiguredEmailToAddressesIntentKey);

                if (Intent.HasExtra(PreconfiguredEmailCcAddressesIntentKey))
                    cdf.PreconfiguredEmailCcAddresses = Intent.Extras.GetStringArray(PreconfiguredEmailCcAddressesIntentKey);

                if (Intent.HasExtra(PreconfiguredEmailBccAddressesIntentKey))
                    cdf.PreconfiguredEmailBccAddresses = Intent.Extras.GetStringArray(PreconfiguredEmailBccAddressesIntentKey);

                if (Intent.HasExtra(CopyToNewOptionsIntentKey))
                    cdf.CopyToNewOption = (CopyToNewOption)Intent.Extras.GetInt(CopyToNewOptionsIntentKey);

                if (Intent.HasExtra(OutgoingDocumentGuidIntentKey))
                {
                    cdf.OutgoingDocumentGuid = new Guid(Intent.Extras.GetString(OutgoingDocumentGuidIntentKey));
                    cdf.LocalDocument = true;
                }
                else
                {
                    cdf.LocalDocument = false;
                }

                cdf.CloseRequest = OnBackPressed;

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

        public override void Finish()
        {
            base.Finish();

            cdf?.DeleteAutoSavedDocument();

            OverridePendingTransition(Resource.Animation.no_change, Resource.Animation.slide_down);
        }
    }
}