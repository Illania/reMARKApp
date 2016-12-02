//
// Project: Mark5.Mobile.Droid
// File: DocumentsSearchCriteriaFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.Common;
using Mark5.Mobile.Droid.Ui.Views.SearchViews;
using System.Collections.Generic;

namespace Mark5.Mobile.Droid.Ui.Fragments
{

    public class DocumentsSearchCriteriaFragment : RetainableStateFragment
    {

        LinearLayoutCompat linearLayout;
        AppCompatButton searchButton;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(DocumentsSearchCriteriaFragment)}...");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_with_button, container, false);

            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);
            searchButton = rootView.FindViewById<AppCompatButton>(Resource.Id.button);

            linearLayout.AddView(new DocumentReferenceNumberSearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new DocumentFromToSearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new DocumentSubjectMessageSearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new DocumentReceivedDateRangeSearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new DocumentUnreadOnlySearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new DocumentDirectionsSearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new DocumentPrioritiesSearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new DocumentLinesSearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new DocumentCommentsSearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new DocumentAttachmentNamesSearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new DocumentSearchInAttachmentsSearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new DocumentWithAttachmentsOnlySearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new DocumentCategoriesSearchView(Context, this));
            linearLayout.AddView(new Divider(Context));

            if (ServerConfig.SystemSettings.DocumentsModuleInfo.HandledFieldEnabled)
            {
                linearLayout.AddView(new DocumentHandledSearchView(Context));
                linearLayout.AddView(new Divider(Context));
            }

            if (ServerConfig.SystemSettings.DocumentsModuleInfo.ExtraFieldInfos.Any())
            {
                linearLayout.AddView(new DocumentExtraFieldsSearchView(Context));
                linearLayout.AddView(new Divider(Context));
            }

            linearLayout.AddView(new DocumentPartialWordsSearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new MaxDocumentsSearchView(Context));

            searchButton.Text = GetString(Resource.String.search);
            searchButton.Click += (sender, e) =>
            {
                var i = new Intent(Activity, typeof(SearchResultsActivity));
                i.PutExtra(SearchResultsActivity.ModuleIntentKey, SerializationUtils.Serialize(ModuleType.Documents));
                i.PutExtra(SearchResultsActivity.CriteriaIntentKey, SerializationUtils.Serialize(GetCriteria()));
                StartActivity(i);
            };

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            //((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.search_documents);

            CommonConfig.Logger.Info($"Created {nameof(DocumentFragment)}");
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            if (resultCode == (int)Result.Ok && requestCode == AbstractCategoriesSearchView<SearchDocumentsCriteria>.RequestCodes.CategoriesRequest)
            {
                var ccsv = View.FindViewById<DocumentCategoriesSearchView>(AbstractCategoriesSearchView<SearchDocumentsCriteria>.ViewId);
                if (ccsv != null)
                {
                    var categories = SerializationUtils.Deserialize<List<Category>>(data.Extras.GetString(PickCategoriesListActivity.CategoriesResultKey));
                    ccsv.SetSelectedCategoryIds(categories.Select(c => c.Id).ToList());
                }
            }
        }

        SearchDocumentsCriteria GetCriteria()
        {
            var criteria = new SearchDocumentsCriteria();

            for (var i = 0; i < linearLayout.ChildCount; i++)
            {
                var dsv = linearLayout.GetChildAt(i) as AbstractSearchView<SearchDocumentsCriteria>;
                if (dsv != null)
                {
                    dsv.ToCriteria(criteria);
                }
            }

            return criteria;
        }

        void SetCriteria(SearchDocumentsCriteria criteria)
        {
            for (var i = 0; i < linearLayout.ChildCount; i++)
            {
                var dsv = linearLayout.GetChildAt(i) as AbstractSearchView<SearchDocumentsCriteria>;
                if (dsv != null)
                {
                    dsv.FromCriteria(criteria);
                }
            }
        }

        public override IRetainableState OnRetainInstanceState()
        {
            return new DocumentsSearchFragmentState { Criteria = GetCriteria() };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var dsfs = restoredState as DocumentsSearchFragmentState;
            if (dsfs != null)
            {
                SetCriteria(dsfs.Criteria);
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(DocumentsSearchCriteriaFragment)}";
        }

        class DocumentsSearchFragmentState : IRetainableState
        {

            public SearchDocumentsCriteria Criteria { get; set; }
        }
    }
}

