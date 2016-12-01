//
// Project: Mark5.Mobile.Droid
// File: DocumentsListActivity.cs
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
    public class DocumentsListActivity : BaseAppCompatActivity
    {

        public const string FolderIntentKey = "Folder_fc733ef0-68cb-4412-9255-cf128602f176";

        Toolbar toolbar;

        DocumentsListFragment dlf;

        TinyMessageSubscriptionToken readStatusToken;
        TinyMessageSubscriptionToken priorityToken;
        TinyMessageSubscriptionToken categoriesToken;
        TinyMessageSubscriptionToken commentCountToken;
        TinyMessageSubscriptionToken entityMovedFromFolderToken;
        TinyMessageSubscriptionToken entityRemovedFromFolderToken;
        TinyMessageSubscriptionToken entityRemovedToken;

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
                    Folder = folder,
                    CloseRequest = OnBackPressed
                };
                ft.Replace(Resource.Id.fragment_container, dlf, dlf.GenerateTag());
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(DocumentsListActivity)}");
            }
            else
            {
                dlf = (DocumentsListFragment)SupportFragmentManager.FindFragmentById(Resource.Id.fragment_container);
                CommonConfig.Logger.Info($"Restored {nameof(DocumentsListActivity)}");
            }

            readStatusToken = PlatformConfig.MessengerHub.Subscribe<DocumentPreviewReadStatusChangedMessage>(dlf.UpdateReadStatus, m => dlf != null && m.Sender != dlf);
            priorityToken = PlatformConfig.MessengerHub.Subscribe<DocumentPreviewPriorityChangedMessage>(dlf.UpdatePriority, m => dlf != null && m.Sender != dlf);
            categoriesToken = PlatformConfig.MessengerHub.Subscribe<DocumentPreviewCategoriesChangedMessage>(dlf.UpdateCategories, m => dlf != null && m.Sender != dlf);
            commentCountToken = PlatformConfig.MessengerHub.Subscribe<DocumentPreviewCommentCountChangedMessage>(dlf.UpdateCommentsCount, m => dlf != null && m.Sender != dlf);
            entityMovedFromFolderToken = PlatformConfig.MessengerHub.Subscribe<EntityMovedFromFolderMessage>(dlf.UpdateMovedFromFolderEntities, m => dlf != null && m.Sender != dlf && dlf.Folder.Id == m.FromFolderId && m.ObjectType == ObjectType.Document);
            entityRemovedFromFolderToken = PlatformConfig.MessengerHub.Subscribe<EntityRemovedFromFolderMessage>(dlf.UpdateRemovedFromFolderEntities, m => dlf != null && m.Sender != dlf && dlf.Folder.Id == m.FromFolderId && m.ObjectType == ObjectType.Document);
            entityRemovedToken = PlatformConfig.MessengerHub.Subscribe<EntityRemovedMessage>(dlf.UpdateRemovedEntities, m => dlf != null && m.Sender != dlf && m.ObjectType == ObjectType.Document);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            readStatusToken?.Dispose();
            priorityToken?.Dispose();
            commentCountToken?.Dispose();
            categoriesToken?.Dispose();
            entityMovedFromFolderToken?.Dispose();
            entityRemovedFromFolderToken?.Dispose();
            entityRemovedToken?.Dispose();
        }
    }
}

