//
// Project: Mark5.Mobile.Droid
// File: DocumentFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System.Threading.Tasks;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.DocumentViews;

namespace Mark5.Mobile.Droid.Ui.Fragments
{

    public class DocumentFragment : RetainableStateFragment
    {

        public int? FolderId { get; set; }

        public Folder Folder { get; set; }

        public int? DocumentId { get; set; }

        public DocumentPreview DocumentPreview { get; set; }

        public Document Document { get; set; }

        ProgressBar progress;
        ScrollView scrollView;
        LinearLayoutCompat linearLayout;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(DocumentFragment)} [folder.id={FolderId ?? Folder?.Id}, document.id={DocumentId ?? DocumentPreview?.Id ?? Document?.Id}...");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout, container, false);

            progress = rootView.FindViewById<ProgressBar>(Resource.Id.progress);
            scrollView = rootView.FindViewById<ScrollView>(Resource.Id.scroll_view);
            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);

            linearLayout.AddView(new SubjectView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new RecipentsView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new ContentView(Context));

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = string.Empty;

            CommonConfig.Logger.Info($"Created {nameof(DocumentFragment)} [folder.id={FolderId ?? Folder?.Id}, document.id={DocumentId ?? DocumentPreview?.Id ?? Document?.Id}]");
        }

        public override async void OnResume()
        {
            base.OnResume();

            await RefreshData();
        }

        async Task RefreshData()
        {
            if (DocumentId.HasValue && DocumentPreview == null && Document == null)
            {
                var container = await Managers.DocumentsManager.GetDocumentWithPreviewAsync(FolderId ?? Folder.Id, DocumentId.Value);
                DocumentPreview = container.DocumentPreview;
                Document = container.Document;
            }

            if (DocumentPreview != null && Document == null)
            {
                Document = await Managers.DocumentsManager.GetDocumentAsync(FolderId ?? Folder.Id, DocumentPreview.Id);
            }

            RefreshView();
        }

        void RefreshView()
        {
            progress.Visibility = ViewStates.Gone;
            scrollView.Visibility = ViewStates.Visible;

            for (var i = 0; i < linearLayout.ChildCount; i++)
            {
                var dv = linearLayout.GetChildAt(i) as IDocumentView;
                if (dv != null)
                {
                    dv.DocumentPreview = DocumentPreview;
                    dv.Document = Document;
                    dv.RefreshView();
                }
            }
        }

        public override IRetainableState OnRetainInstanceState()
        {
            return new DocumentFragmentState
            {
                Folder = Folder,
                DocumentId = DocumentId,
                DocumentPreview = DocumentPreview,
                Document = Document
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var dfs = restoredState as DocumentFragmentState;
            if (dfs != null)
            {
                Folder = dfs.Folder;
                DocumentId = dfs.DocumentId;
                DocumentPreview = dfs.DocumentPreview;
                Document = dfs.Document;
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(DocumentFragment)} [DocumentId={DocumentPreview?.Id ?? Document.Id}]";
        }

        class DocumentFragmentState : IRetainableState
        {

            public Folder Folder { get; set; }

            public int? DocumentId { get; set; }

            public DocumentPreview DocumentPreview { get; set; }

            public Document Document { get; set; }
        }
    }
}
