//
// Project: Mark5.Mobile.Droid
// File: DocumentActivity.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Activities
{

    [Activity]
    public class DocumentActivity : BaseAppCompatActivity
    {

        public const string FolderIntentKey = "Folder_fc733ef0-68cb-4412-9255-cf128602f176";
        public const string DocumentPreviewIntentKey = "DocumentPreview_0bd291a4-22a5-431c-ad6e-4c8b273eeb98";

        string fragmentTagKey = "fragmentTagKey";

        Toolbar toolbar;
        DocumentFragment df;
        string fragmentTag;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(DocumentActivity)}...");

            SetTitle(Resource.String.document);
            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var folder = SerializationUtils.Deserialize<Folder>(Intent.Extras.GetString(FolderIntentKey));
                var documentPreview = SerializationUtils.Deserialize<DocumentPreview>(Intent.Extras.GetString(DocumentPreviewIntentKey));
                var ft = SupportFragmentManager.BeginTransaction();
                df = new DocumentFragment
                {
                    Folder = folder,
                    DocumentPreview = documentPreview,
                    CloseRequest = OnBackPressed
                };
                fragmentTag = df.GenerateTag();
                ft.Replace(Resource.Id.fragment_container, df, fragmentTag);
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(DocumentActivity)}");
            }
            else
            {
                fragmentTag = savedInstanceState.GetString(fragmentTagKey);
                df = SupportFragmentManager.FindFragmentByTag(fragmentTag) as DocumentFragment;
                CommonConfig.Logger.Info($"Restored {nameof(DocumentActivity)}");
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Android.Content.Intent data)
        {
            if (resultCode == Result.Ok && df != null)
            {
                if (requestCode == DocumentFragment.RequestCodes.CommentsRequest)
                {
                    var comments = SerializationUtils.Deserialize<List<Comment>>(data.GetStringExtra(CommentsListActivity.CommentsResultKey));
                    df.UpdateComments(comments);
                }
                else if (requestCode == DocumentFragment.RequestCodes.CategoriesRequest)
                {
                    var categories = SerializationUtils.Deserialize<List<Category>>(data.GetStringExtra(CategoriesListActivity.CategoriesResultKey));
                    df.UpdateCategories(categories);
                }
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutString(fragmentTagKey, fragmentTag);
            base.OnSaveInstanceState(outState);
        }

        public override bool OnOptionsItemSelected(Android.Views.IMenuItem item)
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

