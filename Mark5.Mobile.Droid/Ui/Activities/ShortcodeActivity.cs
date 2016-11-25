//
// Project: Mark5.Mobile.Droid
// File: ShortcodeActivity.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Activities
{

    [Activity]
    public class ShortcodeActivity : BaseAppCompatActivity
    {

        public const string FolderIdIntentKey = "FolderId_e3f108c2-2b12-458c-ae4b-195c4036d334";
        public const string FolderIntentKey = "Folder_fc733ef0-68cb-4412-9255-cf128602f176";
        public const string SearchIdIntentKey = "SearchId_17ad833a-b475-4835-8f81-87fe435ed75d";
        public const string ShortcodeIdIntentKey = "ShortcodeId_3b7133eb-aa8c-44e9-be83-e984c5c43967";
        public const string ShortcodePreviewIntentKey = "ShortcodePreview_0bd291a4-22a5-431c-ad6e-4c8b273eeb98";
        public const string ReadOnlyModeIntentKey = "ReadOnlyMode_5676137e-22ab-4bf9-bff5-de812892c121";
        public const string NotificationGuidIntentKey = "NotificationGuid_f1cdbdf5-3f62-4545-ae60-8acfd6a5c750";

        const string sfFragmentTagKey = "fragmentTagKey";
        string sfFragmentTag;

        Toolbar toolbar;

        ShortcodeFragment sf;

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
                sf = new ShortcodeFragment();

                if (Intent.HasExtra(FolderIdIntentKey))
                    sf.FolderId = Intent.Extras.GetInt(FolderIdIntentKey);

                if (Intent.HasExtra(FolderIntentKey))
                    sf.Folder = SerializationUtils.Deserialize<Folder>(Intent.Extras.GetString(FolderIntentKey));

                if (Intent.HasExtra(SearchIdIntentKey))
                    sf.SearchId = Intent.Extras.GetInt(SearchIdIntentKey);

                if (Intent.HasExtra(ShortcodeIdIntentKey))
                    sf.ShortcodeId = Intent.Extras.GetInt(ShortcodeIdIntentKey);

                if (Intent.HasExtra(ShortcodePreviewIntentKey))
                    sf.ShortcodePreview = SerializationUtils.Deserialize<ShortcodePreview>(Intent.Extras.GetString(ShortcodePreviewIntentKey));

                if (Intent.HasExtra(ReadOnlyModeIntentKey))
                    sf.ReadOnlyMode = Intent.Extras.GetBoolean(ReadOnlyModeIntentKey);

                if (Intent.HasExtra(NotificationGuidIntentKey))
                    sf.NotificationGuid = SerializationUtils.Deserialize<Guid>(Intent.Extras.GetString(NotificationGuidIntentKey));

                sf.CloseRequest = OnBackPressed;

                var ft = SupportFragmentManager.BeginTransaction();
                sfFragmentTag = sf.GenerateTag();
                ft.Replace(Resource.Id.fragment_container, sf, sfFragmentTag);
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(ShortcodeActivity)}");
            }
            else
            {
                sfFragmentTag = savedInstanceState.GetString(sfFragmentTagKey);
                sf = SupportFragmentManager.FindFragmentByTag(sfFragmentTag) as ShortcodeFragment;

                CommonConfig.Logger.Info($"Restored {nameof(ShortcodeActivity)}");
            }
        }
    }
}

