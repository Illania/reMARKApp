//
// Project: Mark5.Mobile.Droid
// File: SearchDocumentsActivity.cs
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

namespace Mark5.Mobile.Droid.Ui.Activities
{

    [Activity]
    public class SearchActivity : BaseAppCompatActivity
    {

        public const string ModuleIntentKey = "Module_d1dbd7d8-045d-48c0-b72c-618107935279";

        Toolbar toolbar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(SearchActivity)}...");

            SetTitle(Resource.String.search);
            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var type = SerializationUtils.Deserialize<ModuleType>(Intent.Extras.GetString(ModuleIntentKey));

                if (type == ModuleType.Documents)
                {
                    var ft = SupportFragmentManager.BeginTransaction();
                    var dlf = new DocumentsSearchFragment();
                    ft.Replace(Resource.Id.fragment_container, dlf, dlf.GenerateTag());
                    ft.Commit();
                }

                if (type == ModuleType.Contacts)
                {
                    var ft = SupportFragmentManager.BeginTransaction();
                    var dlf = new ContactsSearchFragment();
                    ft.Replace(Resource.Id.fragment_container, dlf, dlf.GenerateTag());
                    ft.Commit();
                }

                if (type == ModuleType.Shortcodes)
                {
                    var ft = SupportFragmentManager.BeginTransaction();
                    var dlf = new ShortcodesSearchFragment();
                    ft.Replace(Resource.Id.fragment_container, dlf, dlf.GenerateTag());
                    ft.Commit();
                }

                CommonConfig.Logger.Info($"Created {nameof(SearchActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(SearchActivity)}");
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
    }
}
