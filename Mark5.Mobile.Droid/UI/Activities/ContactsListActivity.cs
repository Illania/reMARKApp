//
// Project: Mark5.Mobile.Droid
// File: ContactsListActivity.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Fragments;
using TinyMessenger;

namespace Mark5.Mobile.Droid.Ui.Activities
{

    [Activity]
    public class ContactsListActivity : BaseAppCompatActivity
    {
        public const string FolderIntentKey = "Folder_fc733ef0-68cb-4412-9255-cf128602f176";

        Toolbar toolbar;
        ContactsListFragment clf;

        TinyMessageSubscriptionToken categoriesToken;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(ContactsListActivity)}...");

            SetTitle(Resource.String.contacts);
            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var folder = SerializationUtils.Deserialize<Folder>(Intent.Extras.GetString(FolderIntentKey));
                var ft = SupportFragmentManager.BeginTransaction();
                clf = new ContactsListFragment
                {
                    Folder = folder
                };
                ft.Replace(Resource.Id.fragment_container, clf, clf.GenerateTag());
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(ContactsListActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(ContactsListActivity)}");
            }

            categoriesToken = PlatformConfig.MessengerHub.Subscribe<ContactPreviewCategoriesChangedMessage>(m =>
            {
                if (clf != null && m.Sender != clf)
                {
                    clf.UpdateCategories(m);
                }
            });
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

