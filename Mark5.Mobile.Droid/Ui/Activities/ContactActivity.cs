//
// Project: 
// File: ContactViewActivity.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
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
        public const string ContactPreviewIntentKey = "ContactPreview_0da27d12-4d29-4f44-8dbf-2e28d7f93aae";
        public const string FolderIntentKey = "Folder_88a33f0b-ebbf-4eed-b33d-49fba4f43f15";

        ContactHeaderView toolbarHeaderView;
        ContactHeaderView floatHeaderView;

        Toolbar toolbar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(ContactActivity)}...");

            SetContentView(Resource.Layout.base_layout_collapsing);

            toolbar = FindViewById<Toolbar>(Resource.Id.collapsing_toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            SupportActionBar.Title = "";

            var appBarLayout = FindViewById<AppBarLayout>(Resource.Id.collapsing_appbar);
            toolbarHeaderView = FindViewById<ContactHeaderView>(Resource.Id.toolbar_header_view);
            floatHeaderView = FindViewById<ContactHeaderView>(Resource.Id.float_header_view);

            appBarLayout.AddOnOffsetChangedListener(new AppBarListener(toolbarHeaderView));

            if (savedInstanceState == null)
            {
                var contactPreview = SerializationUtils.Deserialize<ContactPreview>(Intent.Extras.GetString(ContactPreviewIntentKey));
                var folder = SerializationUtils.Deserialize<Folder>(Intent.Extras.GetString(FolderIntentKey));

                var ft = SupportFragmentManager.BeginTransaction();
                var dlf = new ContactViewFragment
                {
                    ContactPreview = contactPreview,
                    Folder = folder,
                };
                ft.Replace(Resource.Id.fragment_container, dlf, dlf.GenerateTag());
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(ContactActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(ContactActivity)}");
            }
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


        public void SetTitles(string title, string subtitle)
        {
            RunOnUiThreadIfNecessary(() =>
            {
                toolbarHeaderView.SetTitles(title, subtitle);
                floatHeaderView.SetTitles(title, subtitle);
            });
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
