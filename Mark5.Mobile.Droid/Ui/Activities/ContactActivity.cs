//
// Project: Mark5.Mobile.Droid
// File: ContactViewActivity.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Fragments;
using Mark5.Mobile.Droid.Ui.Views.ContactViews;

namespace Mark5.Mobile.Droid.Ui.Activities
{

    [Activity]
    public class ContactActivity : BaseAppCompatActivity
    {

        public const string FolderIdIntentKey = "FolderId_35678826-1e66-4e81-9f6a-68b758712338";
        public const string FolderIntentKey = "Folder_88a33f0b-ebbf-4eed-b33d-49fba4f43f15";
        public const string SearchIdIntentKey = "SearchId_7634b0db-2217-4f5b-90a8-903ed1782e77";
        public const string ContactIdIntentKey = "ContactId_248178bc-e0e4-4ca2-aad5-ffaed65514e5";
        public const string ContactPreviewIntentKey = "ContactPreview_0da27d12-4d29-4f44-8dbf-2e28d7f93aae";
        public const string ReadOnlyModeIntentKey = "ReadOnlyMode_660e0fd1-17df-46f2-a4c2-44dacb9f0a76";
        public const string NotificationGuidIntentKey = "NotificationGuid_d0224832-22e3-481b-9c0d-78b361a57691";

        ContactHeaderView toolbarHeaderView;
        ContactHeaderView floatHeaderView;

        Toolbar toolbar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(ContactActivity)}...");

            SetContentView(Resource.Layout.base_layout_contact);

            toolbar = FindViewById<Toolbar>(Resource.Id.collapsing_toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.Title = string.Empty;

            var appBarLayout = FindViewById<AppBarLayout>(Resource.Id.collapsing_appbar);
            toolbarHeaderView = FindViewById<ContactHeaderView>(Resource.Id.toolbar_header_view);
            floatHeaderView = FindViewById<ContactHeaderView>(Resource.Id.float_header_view);

            appBarLayout.AddOnOffsetChangedListener(new AppBarListener(toolbarHeaderView));

            if (savedInstanceState == null)
            {
                var cf = new ContactFragment();

                if (Intent.HasExtra(FolderIdIntentKey))
                    cf.FolderId = Intent.Extras.GetInt(FolderIdIntentKey);

                if (Intent.HasExtra(FolderIntentKey))
                    cf.Folder = SerializationUtils.Deserialize<Folder>(Intent.Extras.GetString(FolderIntentKey));

                if (Intent.HasExtra(SearchIdIntentKey))
                    cf.SearchId = Intent.Extras.GetInt(SearchIdIntentKey);

                if (Intent.HasExtra(ContactIdIntentKey))
                    cf.ContactId = Intent.Extras.GetInt(ContactIdIntentKey);

                if (Intent.HasExtra(ContactPreviewIntentKey))
                    cf.ContactPreview = SerializationUtils.Deserialize<ContactPreview>(Intent.Extras.GetString(ContactPreviewIntentKey));

                if (Intent.HasExtra(ReadOnlyModeIntentKey))
                    cf.ReadOnlyMode = Intent.Extras.GetBoolean(ReadOnlyModeIntentKey);

                if (Intent.HasExtra(NotificationGuidIntentKey))
                    cf.NotificationGuid = SerializationUtils.Deserialize<Guid>(Intent.Extras.GetString(NotificationGuidIntentKey));

                cf.CloseRequest = OnBackPressed;

                var ft = SupportFragmentManager.BeginTransaction();
                ft.Replace(Resource.Id.fragment_container, cf, cf.GenerateTag());
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(ContactActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(ContactActivity)}");
            }
        }

        public void SetTitles(string title, string subtitle)
        {
            toolbarHeaderView.SetTitles(title, subtitle);
            floatHeaderView.SetTitles(title, subtitle);
        }

        class AppBarListener : Java.Lang.Object, AppBarLayout.IOnOffsetChangedListener
        {
            readonly ContactHeaderView toolbarHeaderView;

            public AppBarListener(ContactHeaderView headerView)
            {
                toolbarHeaderView = headerView;
            }

            public void OnOffsetChanged(AppBarLayout appBarLayout, int verticalOffset)
            {
                float percentage = Math.Abs(verticalOffset) / (float)appBarLayout.TotalScrollRange;

                toolbarHeaderView.Visibility = percentage < 1f ? ViewStates.Gone : ViewStates.Visible;
            }
        }

    }
}
