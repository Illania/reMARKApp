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
using Mark5.Mobile.Droid.Ui.Common.BusMesseges;
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
        OutgoingDocumentsListFragment odlf;

        TinyMessageSubscriptionToken readStatusToken;
        TinyMessageSubscriptionToken categoriesToken;
        TinyMessageSubscriptionToken commentCountToken;
        TinyMessageSubscriptionToken entityMovedToken;

        const string dlfFragmentTagKey = "DocumentsListFragmentTagKey";
        string dlfFragmentTag = string.Empty;

        const string odlfFragmentTagKey = "OutgoingDocumentsListFragmentTagKey";
        string odlfFragmentTag = string.Empty;

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

            readStatusToken = PlatformConfig.MessengerHub.Subscribe<DocumentPreviewReadStatusChangedMessage>(m =>
            {
                if (dlf != null && m.Sender != dlf && dlf.Folder.Id == m.FolderId)
                {
                    dlf.UpdateReadStatus(m);
                }
            });

            categoriesToken = PlatformConfig.MessengerHub.Subscribe<DocumentPreviewCategoriesChangedMessage>(m =>
            {
                if (dlf != null && m.Sender != dlf)
                {
                    dlf.UpdateCategories(m);
                }
            });

            commentCountToken = PlatformConfig.MessengerHub.Subscribe<DocumentPreviewCommentCountChangedMessage>(m =>
            {
                if (dlf != null && m.Sender != dlf)
                {
                    dlf.UpdateCommentsCount(m);
                }
            });

            entityMovedToken = PlatformConfig.MessengerHub.Subscribe<EntityMovedFromFolderMessage>(m =>
           {
               if (dlf != null && m.Sender != dlf && dlf.Folder.Id == m.FromFolderId && m.ObjectType == ObjectType.Document)
               {
                   dlf.RemoveMovedEntities(m);
               }
           });
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
            commentCountToken?.Dispose();
            categoriesToken?.Dispose();
            entityMovedToken?.Dispose();
        }
    }
}

