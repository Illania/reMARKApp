using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.HubMessages;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Fragments;
using TinyMessenger;

namespace Mark5.Mobile.Droid.Ui.Activities
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class DocumentsListActivity : BaseAppCompatActivity
    {
        public const string FolderIntentKey = "Folder_fc733ef0-68cb-4412-9255-cf128602f176";

        const string dlfFragmentTagKey = "DocumentsListFragmentTagKey";
        const string dtulfFragmentTagKey = "DocumentsToUploadListFragmentTagKey";

        Toolbar toolbar;

        DocumentsListFragment dlf;
        DocumentsToUploadListFragment odlf;

        TinyMessageSubscriptionToken readStatusToken;
        TinyMessageSubscriptionToken priorityToken;
        TinyMessageSubscriptionToken categoriesToken;
        TinyMessageSubscriptionToken commentCountToken;
        TinyMessageSubscriptionToken entityMovedFromFolderToken;
        TinyMessageSubscriptionToken entityRemovedFromFolderToken;
        TinyMessageSubscriptionToken entityRemovedToken;

        string dlfFragmentTag;
        string dtuFragmentTag;

        public static Intent CreateIntent(Context context, Folder folder)
        {
            var intent = new Intent(context, typeof(DocumentsListActivity));
            intent.PutExtra(FolderIntentKey,Serializer.Serialize(folder));

            return intent;
        }

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
                var folder = Serializer.Deserialize<Folder>(Intent.Extras.GetString(FolderIntentKey));
                var ft = SupportFragmentManager.BeginTransaction();
                if (folder.Local && folder.Id == Folder.LocalRootForModule(ModuleType.Documents).SubFolders[0].Id)
                {
                    (odlf, dtuFragmentTag) = DocumentsToUploadListFragment.NewInstance();
                    ft.Replace(Resource.Id.fragment_container, odlf, dtuFragmentTag);
                }
                else
                {
                    (dlf, dlfFragmentTag) = DocumentsListFragment.NewInstance(folder);
                    ft.Replace(Resource.Id.fragment_container, dlf, dlfFragmentTag);
                }
                ft.Commit();

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

                dtuFragmentTag = savedInstanceState.GetString(dtulfFragmentTagKey);
                if (!string.IsNullOrEmpty(dtuFragmentTag))
                {
                    odlf = SupportFragmentManager.FindFragmentByTag(dtuFragmentTag) as DocumentsToUploadListFragment;
                    CommonConfig.Logger.Info($"Reassigned {nameof(DocumentsToUploadListFragment)}");
                }

                CommonConfig.Logger.Info($"Restored {nameof(DocumentsListActivity)}");
            }

            if (dlf != null)
            {
                readStatusToken = CommonConfig.MessengerHub.Subscribe<DocumentPreviewReadStatusChangedMessage>(dlf.UpdateReadStatus, m => dlf != null && m.Sender != dlf);
                priorityToken = CommonConfig.MessengerHub.Subscribe<DocumentPreviewPriorityChangedMessage>(dlf.UpdatePriority, m => dlf != null && m.Sender != dlf);
                categoriesToken = CommonConfig.MessengerHub.Subscribe<EntityCategoriesChangedMessage>(dlf.UpdateCategories, m => dlf != null && m.Sender != dlf && m.ObjectType == ObjectType.Document);
                commentCountToken = CommonConfig.MessengerHub.Subscribe<EntityPreviewCommentCountChangedMessage>(dlf.UpdateCommentsCount, m => dlf != null && m.Sender != dlf && m.ObjectType == ObjectType.Document);
                entityMovedFromFolderToken = CommonConfig.MessengerHub.Subscribe<EntityMovedFromFolderMessage>(dlf.UpdateMovedFromFolderEntities, m => dlf != null && m.Sender != dlf && dlf.Folder.Id == m.FromFolderId && m.ObjectType == ObjectType.Document);
                entityRemovedFromFolderToken = CommonConfig.MessengerHub.Subscribe<EntityRemovedFromFolderMessage>(dlf.UpdateRemovedFromFolderEntities, m => dlf != null && m.Sender != dlf && dlf.Folder.Id == m.FromFolderId && m.ObjectType == ObjectType.Document);
                entityRemovedToken = CommonConfig.MessengerHub.Subscribe<EntityRemovedMessage>(dlf.UpdateRemovedEntities, m => dlf != null && m.Sender != dlf && m.ObjectType == ObjectType.Document);
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
            outState.PutString(dtulfFragmentTagKey, dtuFragmentTag);

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