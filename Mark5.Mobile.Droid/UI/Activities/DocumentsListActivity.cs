//
// Project: Mark5.Mobile.Droid
// File: DocumentsListActivity.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using Android.App;
using Android.Content.PM;
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
    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class DocumentsListActivity : BaseAppCompatActivity
    {
        public const string FolderIntentKey = "Folder_fc733ef0-68cb-4412-9255-cf128602f176";

        Toolbar toolbar;

        DocumentsListFragment dlf;
        OutgoingDocumentsListFragment odlf;

        TinyMessageSubscriptionToken readStatusToken;
        TinyMessageSubscriptionToken priorityToken;
        TinyMessageSubscriptionToken categoriesToken;
        TinyMessageSubscriptionToken commentCountToken;
        TinyMessageSubscriptionToken entityMovedFromFolderToken;
        TinyMessageSubscriptionToken entityRemovedFromFolderToken;
        TinyMessageSubscriptionToken entityRemovedToken;

        const string dlfFragmentTagKey = "DocumentsListFragmentTagKey";
        string dlfFragmentTag = string.Empty;

        const string odlfFragmentTagKey = "OutgoingDocumentsListFragmentTagKey";
        string odlfFragmentTag = string.Empty;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(DocumentsListActivity)}...");

            OverridePendingTransition(Resource.Animation.enter_from_right, Resource.Animation.exit_to_left_half);

            SetTitle(Resource.String.documents);
            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var folder = SerializationUtils.Deserialize<Folder>(Intent.Extras.GetString(FolderIntentKey));
                var ft = SupportFragmentManager.BeginTransaction();
                if (folder.Local)
                {
                    odlf = new OutgoingDocumentsListFragment();
                    odlf.CloseRequest = OnBackPressed;
                    odlfFragmentTag = odlf.GenerateTag();
                    ft.Replace(Resource.Id.fragment_container, odlf, odlfFragmentTag);
                    ft.Commit();
                }
                else
                {
                    dlf = new DocumentsListFragment();
                    dlf.Folder = folder;
                    dlf.CloseRequest = OnBackPressed;
                    dlfFragmentTag = dlf.GenerateTag();
                    ft.Replace(Resource.Id.fragment_container, dlf, dlfFragmentTag);
                    ft.Commit();
                }

                CommonConfig.Logger.Info($"Created {nameof(DocumentsListActivity)}");
            }
            else
            {
                dlfFragmentTag = savedInstanceState.GetString(dlfFragmentTagKey);
                if (!string.IsNullOrEmpty(dlfFragmentTag))
                {
                    dlf = SupportFragmentManager.FindFragmentByTag(dlfFragmentTag) as DocumentsListFragment;
                    CommonConfig.Logger.Info($"Reassigned {nameof(DocumentsListFragment)}");
                }

                odlfFragmentTag = savedInstanceState.GetString(odlfFragmentTagKey);
                if (!string.IsNullOrEmpty(odlfFragmentTag))
                {
                    odlf = SupportFragmentManager.FindFragmentByTag(odlfFragmentTag) as OutgoingDocumentsListFragment;
                    CommonConfig.Logger.Info($"Reassigned {nameof(OutgoingDocumentsListFragment)}");
                }

                CommonConfig.Logger.Info($"Restored {nameof(DocumentsListActivity)}");
            }

            if (dlf != null)
            {
                readStatusToken = PlatformConfig.MessengerHub.Subscribe<DocumentPreviewReadStatusChangedMessage>(dlf.UpdateReadStatus, m => dlf != null && m.Sender != dlf);
                priorityToken = PlatformConfig.MessengerHub.Subscribe<DocumentPreviewPriorityChangedMessage>(dlf.UpdatePriority, m => dlf != null && m.Sender != dlf);
                categoriesToken = PlatformConfig.MessengerHub.Subscribe<DocumentPreviewCategoriesChangedMessage>(dlf.UpdateCategories, m => dlf != null && m.Sender != dlf);
                commentCountToken = PlatformConfig.MessengerHub.Subscribe<DocumentPreviewCommentCountChangedMessage>(dlf.UpdateCommentsCount, m => dlf != null && m.Sender != dlf);
                entityMovedFromFolderToken = PlatformConfig.MessengerHub.Subscribe<EntityMovedFromFolderMessage>(dlf.UpdateMovedFromFolderEntities, m => dlf != null && m.Sender != dlf && dlf.Folder.Id == m.FromFolderId && m.ObjectType == ObjectType.Document);
                entityRemovedFromFolderToken = PlatformConfig.MessengerHub.Subscribe<EntityRemovedFromFolderMessage>(dlf.UpdateRemovedFromFolderEntities, m => dlf != null && m.Sender != dlf && dlf.Folder.Id == m.FromFolderId && m.ObjectType == ObjectType.Document);
                entityRemovedToken = PlatformConfig.MessengerHub.Subscribe<EntityRemovedMessage>(dlf.UpdateRemovedEntities, m => dlf != null && m.Sender != dlf && m.ObjectType == ObjectType.Document);
            }
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.enter_from_left_half, Resource.Animation.exit_to_right);
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutString(dlfFragmentTagKey, dlfFragmentTag);
            outState.PutString(odlfFragmentTagKey, odlfFragmentTag);

            base.OnSaveInstanceState(outState);
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