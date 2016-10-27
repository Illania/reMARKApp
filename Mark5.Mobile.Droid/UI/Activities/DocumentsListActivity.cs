//
// Project: Mark5.Mobile.Droid
// File: DocumentsListActivity.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common.BusMesseges;
using Mark5.Mobile.Droid.Ui.Fragments;
using TinyMessenger;

namespace Mark5.Mobile.Droid.Ui.Activities
{

    [Activity]
    public class DocumentsListActivity : AppCompatActivity
    {

        public const string FolderIntentKey = "Folder_fc733ef0-68cb-4412-9255-cf128602f176";

        Toolbar toolbar;

        DocumentsListFragment dlf;

        TinyMessageSubscriptionToken readStatusToken;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(DocumentsListActivity)}...");

            SetTitle(Resource.String.documents);
            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var folder = SerializationUtils.Deserialize<Folder>(Intent.Extras.GetString(FolderIntentKey));
                var ft = SupportFragmentManager.BeginTransaction();
                dlf = new DocumentsListFragment
                {
                    Folder = folder
                };
                ft.Replace(Resource.Id.fragment_container, dlf, dlf.GenerateTag());
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(DocumentsListActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(DocumentsListActivity)}");
            }

            readStatusToken = PlatformConfig.MessengerHub.Subscribe<DocumentPreviewReadStatusChangedMessage>(m =>
            {
                if (dlf != null && m.Sender != dlf && dlf.Folder.Id == m.FolderId)
                {
                    dlf.UpdateReadStatus(m);
                }
            });
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (readStatusToken != null) PlatformConfig.MessengerHub.Unsubscribe<DocumentPreviewReadStatusChangedMessage>(readStatusToken);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
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

