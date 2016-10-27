//
// Project: Mark5.Mobile.Droid
// File: ShortcodeActivity.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.App;
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
    public class ShortcodeActivity : AppCompatActivity
    {

        public const string FolderIntentKey = "Folder_fc733ef0-68cb-4412-9255-cf128602f176";
        public const string ShortcodePreviewIntentKey = "ShortcodePreview_0bd291a4-22a5-431c-ad6e-4c8b273eeb98";

        Toolbar toolbar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(ShortcodeActivity)}...");

            SetTitle(Resource.String.shortcode);
            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var folder = SerializationUtils.Deserialize<Folder>(Intent.Extras.GetString(FolderIntentKey));
                var shortcodePreview = SerializationUtils.Deserialize<ShortcodePreview>(Intent.Extras.GetString(ShortcodePreviewIntentKey));
                var ft = SupportFragmentManager.BeginTransaction();
                var df = new ShortcodeFragment
                {
                    Folder = folder,
                    ShortcodePreview = shortcodePreview,
                    CloseRequest = OnBackPressed
                };
                ft.Replace(Resource.Id.fragment_container, df, df.GenerateTag());
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(ShortcodeActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(ShortcodeActivity)}");
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

