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

        public const string FolderIntentKey = "Folder_88a33f0b-ebbf-4eed-b33d-49fba4f43f15";
        public const string SearchIdIntentKey = "SearchId_7634b0db-2217-4f5b-90a8-903ed1782e77";
        public const string ContactPreviewIntentKey = "ContactPreview_0da27d12-4d29-4f44-8dbf-2e28d7f93aae";
        public const string ReadOnlyModeIntentKey = "ReadOnlyMode_660e0fd1-17df-46f2-a4c2-44dacb9f0a76";

        const string cfFragmentTagKey = "fragmentTagKey";
        string cfFragmentTag;

        ContactHeaderView toolbarHeaderView;
        ContactHeaderView floatHeaderView;

        ContactFragment cf;

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
                var folder = SerializationUtils.Deserialize<Folder>(Intent.Extras.GetString(FolderIntentKey));
                var searchId = Intent.Extras.GetInt(SearchIdIntentKey);
                var contactPreview = SerializationUtils.Deserialize<ContactPreview>(Intent.Extras.GetString(ContactPreviewIntentKey));
                var readOnlyMode = Intent.Extras.GetBoolean(ReadOnlyModeIntentKey);
                var ft = SupportFragmentManager.BeginTransaction();
                cf = new ContactFragment
                {
                    Folder = folder,
                    SearchId = searchId,
                    ContactPreview = contactPreview,
                    CloseRequest = OnBackPressed,
                    ReadOnlyMode = readOnlyMode
                };
                cfFragmentTag = cf.GenerateTag();
                ft.Replace(Resource.Id.fragment_container, cf, cfFragmentTag);
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(ContactActivity)}");
            }
            else
            {
                cfFragmentTag = savedInstanceState.GetString(cfFragmentTagKey);
                cf = SupportFragmentManager.FindFragmentByTag(cfFragmentTag) as ContactFragment;

                CommonConfig.Logger.Info($"Restored {nameof(ContactActivity)}");
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutString(cfFragmentTagKey, cfFragmentTag);
            base.OnSaveInstanceState(outState);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Android.Content.Intent data)
        {
            if (resultCode == Result.Ok && cf != null)
            {
                if (requestCode == ContactFragment.RequestCodes.CommentsRequest)
                {
                    var comments = SerializationUtils.Deserialize<List<Comment>>(data.GetStringExtra(CommentsListActivity.CommentsResultKey));
                    cf.UpdateComments(comments);
                }
                else if (requestCode == ContactFragment.RequestCodes.CategoriesRequest)
                {
                    var categories = SerializationUtils.Deserialize<List<Category>>(data.GetStringExtra(CategoriesListActivity.CategoriesResultKey));
                    cf.UpdateCategories(categories);
                }
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
