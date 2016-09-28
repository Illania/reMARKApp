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
            linearLayout.AddView(new AttachmentsView(Context));
            linearLayout.AddView(new Divider(Context));
            linearLayout.AddView(new ContentView(Context));

            HasOptionsMenu = true;

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

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            if (!DocumentPreview.IsReadByCurrent)
            {
                menu.Add(Menu.None, 10, 10, Resource.String.mark_as_read);
            }

            if (DocumentPreview.IsReadByCurrent)
            {
                menu.Add(Menu.None, 11, 11, Resource.String.marks_as_unread);
            }

            menu.Add(Menu.None, 20, 20, Resource.String.reply);
            menu.Add(Menu.None, 21, 21, Resource.String.reply_all);
            menu.Add(Menu.None, 22, 22, Resource.String.forward);
            menu.Add(Menu.None, 30, 30, Resource.String.copy_to_worktray);
            menu.Add(Menu.None, 40, 40, Resource.String.copy_to_folder);

            if (Folder.InternalType == FolderInternalType.FilterView
                || Folder.InternalType == FolderInternalType.Static
                || Folder.InternalType == FolderInternalType.Worktray)
            {
                menu.Add(Menu.None, 41, 41, Resource.String.move_to_folder);
            }

            menu.Add(Menu.None, 50, 50, Resource.String.categories);
            menu.Add(Menu.None, 60, 60, Resource.String.comments);
            menu.Add(Menu.None, 70, 70, Resource.String.actions);
            menu.Add(Menu.None, 80, 80, Resource.String.links);

            if (Folder.InternalType == FolderInternalType.FilterView
                || Folder.InternalType == FolderInternalType.Static
                || Folder.InternalType == FolderInternalType.Worktray)
            {
                menu.Add(Menu.None, 90, 90, Resource.String.delete_from_folder);
            }

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator
                || ServerConfig.SystemSettings.DocumentsModuleInfo.Permissions.DeleteAllowed)
            {
                menu.Add(Menu.None, 91, 91, Resource.String.delete);
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            return base.OnOptionsItemSelected(item);
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
                var dv = linearLayout.GetChildAt(i) as DocumentView;
                if (dv != null)
                {
                    dv.DocumentPreview = DocumentPreview;
                    dv.Document = Document;
                    dv.RefreshView();

                    var d = linearLayout.GetChildAt(i + 1) as Divider;
                    if (d != null)
                    {
                        d.Visibility = dv.Visibility;
                        i++;
                    }
                }
            }

            linearLayout.Invalidate();
            linearLayout.RequestLayout();
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
