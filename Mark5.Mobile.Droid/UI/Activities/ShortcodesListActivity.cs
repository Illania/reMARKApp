//
// Project: Mark5.Mobile.Droid
// File: ShortcodesListActivity.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Common.HubMessages;
using Mark5.Mobile.Droid.Ui.Fragments;
using TinyMessenger;

namespace Mark5.Mobile.Droid.Ui.Activities
{

    [Activity]
    public class ShortcodesListActivity : BaseAppCompatActivity
    {

        public const string FolderIntentKey = "Folder_fc733ef0-68cb-4412-9255-cf128602f176";

        Toolbar toolbar;
        ShortcodesListFragment slf;

        TinyMessageSubscriptionToken entityMovedFromFolderToken;
        TinyMessageSubscriptionToken entityRemovedFromFolderToken;
        TinyMessageSubscriptionToken entityRemovedToken;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(ShortcodesListActivity)}...");

            SetTitle(Resource.String.shortcodes);
            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var folder = SerializationUtils.Deserialize<Folder>(Intent.Extras.GetString(FolderIntentKey));
                var ft = SupportFragmentManager.BeginTransaction();
                slf = new ShortcodesListFragment
                {
                    Folder = folder,
                    CloseRequest = OnBackPressed
                };
                ft.Replace(Resource.Id.fragment_container, slf, slf.GenerateTag());
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(ShortcodesListActivity)}");
            }
            else
            {
                slf = (ShortcodesListFragment)SupportFragmentManager.FindFragmentById(Resource.Id.fragment_container);
                CommonConfig.Logger.Info($"Restored {nameof(ShortcodesListActivity)}");
            }

            entityMovedFromFolderToken = PlatformConfig.MessengerHub.Subscribe<EntityMovedFromFolderMessage>(slf.UpdateMovedEntities, m => slf != null && m.Sender != slf && slf.Folder.Id == m.FromFolderId && m.ObjectType == ObjectType.Shortcode);
            entityRemovedFromFolderToken = PlatformConfig.MessengerHub.Subscribe<EntityRemovedFromFolderMessage>(slf.UpdateRemovedFromFolderEntities, m => slf != null && m.Sender != slf && slf.Folder.Id == m.FromFolderId && m.ObjectType == ObjectType.Shortcode);
            entityRemovedToken = PlatformConfig.MessengerHub.Subscribe<EntityRemovedMessage>(slf.UpdateRemovedEntities, m => slf != null && m.Sender != slf && m.ObjectType == ObjectType.Shortcode);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            entityMovedFromFolderToken?.Dispose();
            entityRemovedFromFolderToken?.Dispose();
            entityRemovedToken?.Dispose();
        }
    }
}

