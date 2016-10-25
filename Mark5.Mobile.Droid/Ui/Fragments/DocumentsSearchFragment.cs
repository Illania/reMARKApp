//
// Project: Mark5.Mobile.Droid
// File: DocumentsSearchFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.DocumentsSearchViews;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Views.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{

    public class DocumentsSearchFragment : RetainableStateFragment
    {

        ProgressBar progress;
        ScrollView scrollView;
        LinearLayoutCompat linearLayout;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(DocumentsSearchFragment)}...");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout, container, false);

            progress = rootView.FindViewById<ProgressBar>(Resource.Id.progress);
            scrollView = rootView.FindViewById<ScrollView>(Resource.Id.scroll_view);
            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);

            linearLayout.AddView(new ReferenceNumberSearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new FromToSearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new SubjectMessageSearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new DateRangeSearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new PartialWordsSearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new UnreadOnlySearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new DocumentDirectionsSearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new PrioritiesSearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new LinesSearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new CommentsSearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new AttachmentNamesSearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new SearchInAttachmentsSearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new WithAttachmentsOnlySearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new ProcessedSearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new CategoriesSearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new MustHaveCategoriesSearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new FoldersSearchView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new ExtraFieldsSearchView(Context));
            linearLayout.AddView(new Divider(Context));

            progress.Visibility = ViewStates.Gone;
            scrollView.Visibility = ViewStates.Visible;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.search_documents);

            CommonConfig.Logger.Info($"Created {nameof(DocumentFragment)}");
        }

        public override IRetainableState OnRetainInstanceState()
        {
            var criteria = new SearchDocumentsCriteria();

            for (var i = 0; i < linearLayout.ChildCount; i++)
            {
                var dsv = linearLayout.GetChildAt(i) as DocumentsSearchView;
                if (dsv != null)
                {
                    dsv.UpdateCriteria(criteria);
                }
            }

            return new DocumentsSearchFragmentState { Criteria = criteria };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var dsfs = restoredState as DocumentsSearchFragmentState;
            if (dsfs != null)
            {
                for (var i = 0; i < linearLayout.ChildCount; i++)
                {
                    var dsv = linearLayout.GetChildAt(i) as DocumentsSearchView;
                    if (dsv != null)
                    {
                        dsv.SetFromCriteria(dsfs.Criteria);
                    }
                }
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(DocumentsSearchFragment)}";
        }

        class DocumentsSearchFragmentState : IRetainableState
        {

            public SearchDocumentsCriteria Criteria { get; set; }
        }
    }
}

