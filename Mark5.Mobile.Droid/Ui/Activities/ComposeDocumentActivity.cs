using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Activities
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class ComposeDocumentActivity : AppCompatActivity
    {
        const string DocumentCreationModeFlagIntentKey = "DocumentCreationModeFlag_290d1383-175d-4e2d-8f5e-ca899baff3f7";
        const string CopyToNewOptionsIntentKey = "CopyToNewOptions_f298d024-4df0-431d-ad3d-1834eb0dede0";
        const string RestoreWorkingCopyIntentKey = "RestoreWorkingCopy_7c921825-0a4b-47e6-91b6-9c3d59a895e6";
        const string PreviousDocumentDirectionIntentKey = "PreviousDocumentDirection_edefdcd2-764f-439d-891b-178b8de29333";
        const string PreviousDocumentFolderIdIntentKey = "PreviousDocumentFolderId_ac0d9a31-2ddc-497b-8fbe-7fd5a51b2257";
        const string PreviousDocumentIdIntentKey = "PreviousDocumentId_a2066147-a27b-454f-bc5c-03e6b8266697";
        const string PreconfiguredEmailAddressesIntentKey = "PreconfiguredEmailAddresses_25ff402c-268e-477c-890c-80d68e60ab01";

        const string cdfFragmentTagKey = "fragmentTagKey";
        string cdfFragmentTag;

        Toolbar toolbar;
        ComposeDocumentFragment cdf;

        public static Intent CreateIntent(Context context,
                                          DocumentCreationModeFlag documentCreationModeFlag,
                                          CopyToNewOption copyToNewOptions,
                                          bool restoreWorkingCopy = false,
                                          DocumentDirection previousDocumentDirection = DocumentDirection.None,
                                          int? previousDocumentFolderId = null,
                                          int? previousDocumentId = null,
                                          Dictionary<DocumentAddressType, string[]> preconfiguredEmailAddresses = null)
        {
            var intent = new Intent(context, typeof(ComposeDocumentActivity));
            intent.PutExtra(DocumentCreationModeFlagIntentKey, (int)documentCreationModeFlag);
            intent.PutExtra(CopyToNewOptionsIntentKey, (int)copyToNewOptions);
            intent.PutExtra(RestoreWorkingCopyIntentKey, restoreWorkingCopy);

            if (previousDocumentDirection != DocumentDirection.None)
                intent.PutExtra(PreviousDocumentDirectionIntentKey, (int)previousDocumentDirection);

            if (previousDocumentFolderId != null)
                intent.PutExtra(PreviousDocumentFolderIdIntentKey, previousDocumentFolderId.Value);

            if (previousDocumentId != null)
                intent.PutExtra(PreviousDocumentIdIntentKey, previousDocumentId.Value);

            if (preconfiguredEmailAddresses != null)
                intent.PutExtra(PreconfiguredEmailAddressesIntentKey, Serializer.Serialize(preconfiguredEmailAddresses));

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
                cdf = ComposeDocumentFragment.NewInstance((DocumentCreationModeFlag)Intent.Extras.GetInt(DocumentCreationModeFlagIntentKey),
                                                          (CopyToNewOption)Intent.Extras.GetInt(CopyToNewOptionsIntentKey),
                                                          Intent.Extras.GetBoolean(RestoreWorkingCopyIntentKey));

                if (Intent.HasExtra(PreviousDocumentDirectionIntentKey))
                    cdf.PreviousDocumentDirection = (DocumentDirection)Intent.Extras.GetInt(PreviousDocumentDirectionIntentKey);
                
                if (Intent.HasExtra(PreviousDocumentFolderIdIntentKey))
                    cdf.PreviousDocumentFolderId = Intent.Extras.GetInt(PreviousDocumentFolderIdIntentKey);

                if (Intent.HasExtra(PreviousDocumentIdIntentKey))
                    cdf.PreviousDocumentId = Intent.Extras.GetInt(PreviousDocumentIdIntentKey);

                if (Intent.HasExtra(PreconfiguredEmailAddressesIntentKey))
                    cdf.PreconfiguredEmailAddresses = Serializer.Deserialize<Dictionary<DocumentAddressType, string[]>>(Intent.Extras.GetString(PreconfiguredEmailAddressesIntentKey));

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
            return base.OnOptionsItemSelected(item);
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.no_change, Resource.Animation.slide_down);
        }
    }
}