using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.AppCompat.View;
using AndroidX.AppCompat.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Activities
{
    [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class AutoReplyActivity: BaseAppCompatActivity
    {
        const string AutoReplyIntentKey = "AutoReply_4F18BD85-1832-4EAA-8A92-4AF927247572";
        const string arfFragmentTagKey = "fragmentTagKey";
        string arfFragmentTag;

        Toolbar toolbar;
        AutoReplyFragment arf;

        public static Intent CreateIntent(Context context, AutoReplyRule autoReplyRule)
        {
            var intent = new Intent(context, typeof(AutoReplyActivity));
            intent.PutExtra(AutoReplyIntentKey, Serializer.Serialize(autoReplyRule));
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
                AutoReplyRule autoReplyRule = null;


                if (Intent.HasExtra(AutoReplyIntentKey))
                    autoReplyRule = Serializer.Deserialize<AutoReplyRule>(Intent.Extras.GetString(AutoReplyIntentKey));

                
                (arf, arfFragmentTag) = AutoReplyFragment.NewInstance(autoReplyRule);

                var ft = SupportFragmentManager.BeginTransaction();
                ft.Replace(Resource.Id.fragment_container, arf, arfFragmentTag);
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(ComposeDocumentActivity)}");
            }
            else
            {
                arfFragmentTag = savedInstanceState.GetString(arfFragmentTagKey);
                arf = SupportFragmentManager.FindFragmentByTag(arfFragmentTag) as AutoReplyFragment;
                CommonConfig.Logger.Info($"Restored {nameof(AutoReplyActivity)}");
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutString(arfFragmentTagKey, arfFragmentTag);
            base.OnSaveInstanceState(outState);
        }

        public override void OnBackPressed()
        {
            arf?.AskIfShouldSave();
        }

        public override void OnActionModeStarted(Android.Views.ActionMode mode)
        {
            arf?.OnActionModeStarted();
            base.OnActionModeStarted(mode);
        }

        public override void OnActionModeFinished(Android.Views.ActionMode mode)
        {
            arf?.OnActionModeFinished();
            base.OnActionModeFinished(mode);
        }

        public override void OnSupportActionModeFinished(ActionMode mode)
        {
            arf?.OnActionModeFinished();
            base.OnSupportActionModeFinished(mode);
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.no_change, Resource.Animation.slide_down);
        }
    }
}

