using System;
using System.Collections.Generic;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Fragments;
using Newtonsoft.Json;

namespace Mark5.Mobile.Droid.Ui.Activities
{
    [Android.App.Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class SwipeDocumentActivity : BaseAppCompatActivity, ViewPager.IOnPageChangeListener
    {
        public const string FolderIdIntentKey = "FolderId_4bd29db4-c529-48a2-bf8f-8f1a96ed60b5";
        public const string FolderIntentKey = "Folder_fc733ef0-68cb-4412-9255-cf128602f176";
        public const string DocumentIdIntentKey = "DocumentId_690fc3d6-ae73-4f5e-844a-06bdc44b6747";
        public const string DocumentPreviewIntentKey = "DocumentPreview_0bd291a4-22a5-431c-ad6e-4c8b273eeb98";
        public const string NotificationGuidIntentKey = "NotificationGuid_0473a08d-5f96-4acd-924a-6d160a23cdf2";

        const int MaxNeighbours = 500;

        int initialPosition;

        Toolbar toolbar;
        ViewPager pager;

        SwipeDocumentActivityState state;

        public static Intent CreateIntent(Context context, Folder folder, DocumentPreview documentPreview)
        {
            var intent = new Intent(context, typeof(SwipeDocumentActivity));
            intent.PutExtra(FolderIntentKey, Serializer.Serialize(folder));

            if (documentPreview != null)
            {
                if (IsDocumentPreviewTooBig(documentPreview))
                    intent.PutExtra(DocumentIdIntentKey, documentPreview.Id);
                else
                    intent.PutExtra(DocumentPreviewIntentKey, Serializer.Serialize(documentPreview));
            }

            return intent;
        }

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(SwipeDocumentActivity)}...");

            OverridePendingTransition(Resource.Animation.enter_from_right, Resource.Animation.exit_to_left_half);

            Title = string.Empty;
            SetContentView(Resource.Layout.base_layout_pager);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            pager = FindViewById<ViewPager>(Resource.Id.pager);
            pager.OffscreenPageLimit = 1;
            pager.AddOnPageChangeListener(this);

            if (savedInstanceState == null)
            {
                var activityState = new SwipeDocumentActivityState();

                if (Intent.HasExtra(FolderIdIntentKey))
                    activityState.FolderId = Intent.Extras.GetInt(FolderIdIntentKey);

                if (Intent.HasExtra(FolderIntentKey))
                    activityState.Folder = Serializer.Deserialize<Folder>(Intent.Extras.GetString(FolderIntentKey));

                activityState.CloseRequest = OnBackPressed;

                var mainFragmentState = new DocumentFragmentState();

                if (Intent.HasExtra(DocumentIdIntentKey))
                    mainFragmentState.DocumentId = Intent.Extras.GetInt(DocumentIdIntentKey);

                if (Intent.HasExtra(DocumentPreviewIntentKey))
                    mainFragmentState.DocumentPreview = Serializer.Deserialize<DocumentPreview>(Intent.Extras.GetString(DocumentPreviewIntentKey));

                if (Intent.HasExtra(NotificationGuidIntentKey))
                    mainFragmentState.NotificationGuid = Serializer.Deserialize<Guid>(Intent.Extras.GetString(NotificationGuidIntentKey));

                activityState.FragmentStates.Add(mainFragmentState);
                activityState.Position = 0;

                CommonConfig.Logger.Info($"Created {nameof(SwipeDocumentActivity)}");

                var initialDocumentId = mainFragmentState?.DocumentPreview?.Id ?? mainFragmentState.DocumentId;

                if (activityState.Folder != null && initialDocumentId > 0)
                    try
                    {
                        var previousIds = await Managers.DocumentsManager.GetNeighbourDocumentsIdAsync(activityState.Folder, initialDocumentId.Value, true, false, MaxNeighbours);
                        previousIds.Reverse();

                        foreach (var previousId in previousIds)
                        {
                            activityState.FragmentStates.Insert(0,
                                new DocumentFragmentState
                                {
                                    DocumentId = previousId
                                });
                            activityState.Position++;
                        }

                        var nextIds = await Managers.DocumentsManager.GetNeighbourDocumentsIdAsync(activityState.Folder, initialDocumentId.Value, false, true, MaxNeighbours);

                        foreach (var nextId in nextIds)
                            activityState.FragmentStates.Add(new DocumentFragmentState
                            {
                                DocumentId = nextId
                            });
                    }
                    catch (Exception ex)
                    {
                        CommonConfig.Logger.Error("Error while retrieveing neighbour documents", ex);
                    }

                state = activityState;
                pager.Adapter = new PagerAdapter(SupportFragmentManager, activityState);

                initialPosition = activityState.Position;

                pager.SetCurrentItem(initialPosition, false);

                // We need to call OnPageSelected manually due to a possible bug in ViewPager
                if (pager.CurrentItem == 0)
                    pager.Post(() => ((ViewPager.IOnPageChangeListener)this).OnPageSelected(pager.CurrentItem));
            }
            else
            {
                var activityState = Serializer.Deserialize<SwipeDocumentActivityState>(savedInstanceState.GetString("state"));
                activityState.CloseRequest = OnBackPressed;

                state = activityState;
                pager.Adapter = new PagerAdapter(SupportFragmentManager, activityState);
                pager.SetCurrentItem(activityState.Position, false);

                // We need to call OnPageSelected manually due to a possible bug in ViewPager
                if (pager.CurrentItem == 0)
                    pager.Post(() => ((ViewPager.IOnPageChangeListener)this).OnPageSelected(pager.CurrentItem));

                CommonConfig.Logger.Info($"Restored {nameof(SwipeDocumentActivity)}");
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutString("state", Serializer.Serialize(state));

            base.OnSaveInstanceState(outState);
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.enter_from_left_half, Resource.Animation.exit_to_right);
        }

        static bool IsDocumentPreviewTooBig(DocumentPreview documentPreview)
        {
            //Ballpark calculation to avoid TransactionTooLargeException
            //https://developer.android.com/reference/android/os/TransactionTooLargeException

            var len1 = documentPreview.AddressesString.Length;
            var len2 = documentPreview.Subject.Length;
            var len3 = documentPreview.CategoriesString.Length;
            var len4 = documentPreview.Preview.Length;

            var sizeInKiB = (len1 + len2 + len3 + len4) * 2 / 1024;

            return sizeInKiB > 200;
        }

        void ViewPager.IOnPageChangeListener.OnPageScrollStateChanged(int state)
        {
            // Nothing to do
        }

        void ViewPager.IOnPageChangeListener.OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
        {
            // Nothing to do
        }

        void ViewPager.IOnPageChangeListener.OnPageSelected(int position)
        {
            if (position != initialPosition)
                CommonConfig.UsageAnalytics.LogEvent(new DocumentQuickSwitchEvent());

            var dp = state.FragmentStates[position].DocumentPreview;
            if (dp?.Direction == DocumentDirection.External)
                CommonConfig.UsageAnalytics.LogEvent(new OpenDocumentEvent(true));
            else
                CommonConfig.UsageAnalytics.LogEvent(new OpenDocumentEvent(false));

            state.Position = position;
        }

        class PagerAdapter : FragmentStatePagerAdapter
        {
            public override int Count => state.FragmentStates.Count;

            readonly SwipeDocumentActivityState state;

            public PagerAdapter(FragmentManager fm, SwipeDocumentActivityState state)
                : base(fm)
            {
                this.state = state;
            }

            public override Fragment GetItem(int position)
            {
                var s = state;
                var fd = state.FragmentStates[position];

                var df = DocumentFragment.NewInstance(s.Folder, s.FolderId, fd.DocumentPreview, fd.DocumentId, fd.NotificationGuid).fragment;

                return df;
            }
        }

        class SwipeDocumentActivityState
        {
            public int? FolderId { get; set; }
            public Folder Folder { get; set; }

            public int Position { get; set; } = -1;
            public List<DocumentFragmentState> FragmentStates { get; } = new List<DocumentFragmentState>();

            [JsonIgnore]
            public Action CloseRequest { get; set; }
        }

        class DocumentFragmentState
        {
            public int? DocumentId { get; set; }
            public DocumentPreview DocumentPreview { get; set; }

            public Guid NotificationGuid { get; set; }
        }
    }
}