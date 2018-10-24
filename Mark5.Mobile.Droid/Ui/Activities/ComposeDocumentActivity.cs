using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Activities
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class ComposeDocumentActivity : BaseAppCompatActivity
    {
        const string DocumentCreationModeFlagIntentKey = "DocumentCreationModeFlag_290d1383-175d-4e2d-8f5e-ca899baff3f7";
        const string CopyToNewOptionIntentKey = "CopyToNewOptions_f298d024-4df0-431d-ad3d-1834eb0dede0";
        const string RestoreWorkingCopyIntentKey = "RestoreWorkingCopy_7c921825-0a4b-47e6-91b6-9c3d59a895e6";
        const string PreviousDocumentDirectionIntentKey = "PreviousDocumentDirection_edefdcd2-764f-439d-891b-178b8de29333";
        const string PreviousDocumentFolderIdIntentKey = "PreviousDocumentFolderId_ac0d9a31-2ddc-497b-8fbe-7fd5a51b2257";
        const string PreviousDocumentIdIntentKey = "PreviousDocumentId_a2066147-a27b-454f-bc5c-03e6b8266697";
        const string PreconfiguredEmailAddressesIntentKey = "PreconfiguredEmailAddresses_25ff402c-268e-477c-890c-80d68e60ab01";
        const string PreconfiguredContentIntentKey = "PreconfiguredContent_78fe5450-4294-461b-b51a-41222f1b2b14";
        const string PreconfiguredSubjectIntentKey = "PreconfiguredSubject_831fc95b-a407-4cfe-a1fc-6bd12986286f";

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
                                          Dictionary<DocumentAddressType, string[]> preconfiguredEmailAddresses = null,
                                          string preconfiguredContent = null,
                                          string preconfiguredSubject = null)
        {
            var intent = new Intent(context, typeof(ComposeDocumentActivity));

            intent.PutExtra(DocumentCreationModeFlagIntentKey, (int)documentCreationModeFlag);
            intent.PutExtra(CopyToNewOptionIntentKey, (int)copyToNewOptions);
            intent.PutExtra(RestoreWorkingCopyIntentKey, restoreWorkingCopy);

            if (previousDocumentDirection != DocumentDirection.None)
                intent.PutExtra(PreviousDocumentDirectionIntentKey, (int)previousDocumentDirection);

            if (previousDocumentFolderId != null)
                intent.PutExtra(PreviousDocumentFolderIdIntentKey, previousDocumentFolderId.Value);

            if (previousDocumentId != null)
                intent.PutExtra(PreviousDocumentIdIntentKey, previousDocumentId.Value);

            if (preconfiguredEmailAddresses != null)
                intent.PutExtra(PreconfiguredEmailAddressesIntentKey, Serializer.Serialize(preconfiguredEmailAddresses));

            if (preconfiguredContent != null)
                intent.PutExtra(PreconfiguredContentIntentKey, preconfiguredContent);

            if (preconfiguredSubject != null)
                intent.PutExtra(PreconfiguredSubjectIntentKey, preconfiguredSubject);

            return intent;
        }

        public static Intent CreateShareReportIntent(Context context, string preconfiguredSubject, string preconfiguredContent)
        {
            var intent = new Intent(context, typeof(ComposeDocumentActivity));

            intent.PutExtra(ComposeDocumentActivity.DocumentCreationModeFlagIntentKey, (int)DocumentCreationModeFlag.New);
            intent.PutExtra(ComposeDocumentActivity.CopyToNewOptionIntentKey, (int)CopyToNewOption.None);
            intent.PutExtra(ComposeDocumentActivity.PreconfiguredEmailAddressesIntentKey, Serializer.Serialize(new Dictionary<DocumentAddressType, string[]>() { { DocumentAddressType.To, new string[] { "appfeedback@nordic-it.com" } } }));
            intent.PutExtra(ComposeDocumentActivity.PreconfiguredSubjectIntentKey, preconfiguredSubject);
            intent.PutExtra(ComposeDocumentActivity.PreconfiguredContentIntentKey, preconfiguredContent);

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
                DocumentCreationModeFlag? documentCreationMode = null;
                CopyToNewOption? copyToNewOption = null;
                bool? restoreWorkingCopy = null;
                DocumentDirection? previousDocumentDirection = null;
                int? previousDocumentFolderId = null;
                int? previousDocumentId = null;
                Dictionary<DocumentAddressType, string[]> preconfiguredEmailAddresses = null;
                string preconfiguredContent = null;
                string preconfiguredSubject = null;

                if (Intent.HasExtra(DocumentCreationModeFlagIntentKey))
                    documentCreationMode = (DocumentCreationModeFlag)Intent.Extras.GetInt(DocumentCreationModeFlagIntentKey);

                if (Intent.HasExtra(CopyToNewOptionIntentKey))
                    copyToNewOption = (CopyToNewOption)Intent.Extras.GetInt(CopyToNewOptionIntentKey);

                if (Intent.HasExtra(RestoreWorkingCopyIntentKey))
                    restoreWorkingCopy = Intent.Extras.GetBoolean(RestoreWorkingCopyIntentKey);

                if (Intent.HasExtra(PreviousDocumentDirectionIntentKey))
                    previousDocumentDirection = (DocumentDirection)Intent.Extras.GetInt(PreviousDocumentDirectionIntentKey);

                if (Intent.HasExtra(PreviousDocumentFolderIdIntentKey))
                    previousDocumentFolderId = Intent.Extras.GetInt(PreviousDocumentFolderIdIntentKey);

                if (Intent.HasExtra(PreviousDocumentIdIntentKey))
                    previousDocumentId = Intent.Extras.GetInt(PreviousDocumentIdIntentKey);

                if (Intent.HasExtra(PreconfiguredEmailAddressesIntentKey))
                    preconfiguredEmailAddresses = Serializer.Deserialize<Dictionary<DocumentAddressType, string[]>>(Intent.Extras.GetString(PreconfiguredEmailAddressesIntentKey));

                if (Intent.HasExtra(PreconfiguredContentIntentKey))
                    preconfiguredContent = Intent.Extras.GetString(PreconfiguredContentIntentKey);

                if (Intent.HasExtra(PreconfiguredSubjectIntentKey))
                    preconfiguredSubject = Intent.Extras.GetString(PreconfiguredSubjectIntentKey);

                (cdf, cdfFragmentTag) = ComposeDocumentFragment.NewInstance(documentCreationMode.Value,
                                                                            copyToNewOption,
                                                                            restoreWorkingCopy,
                                                                            previousDocumentDirection,
                                                                            previousDocumentFolderId,
                                                                            previousDocumentId,
                                                                            preconfiguredEmailAddresses,
                                                                            preconfiguredContent,
                                                                            preconfiguredSubject);

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

        public override void OnBackPressed()
        {
            cdf?.AskIfShouldSave();
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.no_change, Resource.Animation.slide_down);
        }
    }
}