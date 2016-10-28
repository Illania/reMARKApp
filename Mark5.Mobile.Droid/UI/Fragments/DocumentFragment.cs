//
// Project: Mark5.Mobile.Droid
// File: DocumentFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Common.BusMesseges;
using Mark5.Mobile.Droid.Ui.Views.Common;
using Mark5.Mobile.Droid.Ui.Views.DocumentViews;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{

    public class DocumentFragment : RetainableStateFragment
    {
        public static class RequestCodes
        {
            public static int CategoriesRequest = 2;
        }

        const int LargeAttachmentSizeInBytes = 20 * 1024 * 1024; // 20MB

        public int? FolderId { get; set; }

        public Folder Folder { get; set; }

        public int? DocumentId { get; set; }

        public DocumentPreview DocumentPreview { get; set; }

        public Document Document { get; set; }

        public Action CloseRequest { get; set; }

        ProgressBar progress;
        ScrollView scrollView;
        LinearLayoutCompat linearLayout;

        CancellationTokenSource setReadStatusCancellationTokenSource;

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
            linearLayout.AddView(new PriorityView(Context));
            linearLayout.AddView(new Divider(Context));
            var av = new AttachmentsView(Context);
            av.AttachmentClicked += AttachmentsView_AttachmentClicked;
            av.AttachmentLongClicked += AttachmentsView_AttachmentLongClicked;
            linearLayout.AddView(av);
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

            if (!IsAdded || IsDetached || IsRemoving) return;

            MarkAsReadIfNecessary();
        }

        public override void OnDestroyedByUser()
        {
            base.OnDestroyedByUser();

            setReadStatusCancellationTokenSource?.Cancel();
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

            menu.Add(Menu.None, 50, 50, Resource.String.set_priority);
            menu.Add(Menu.None, 60, 60, Resource.String.categories);
            menu.Add(Menu.None, 70, 90, Resource.String.comments);
            menu.Add(Menu.None, 80, 80, Resource.String.actions);
            menu.Add(Menu.None, 90, 90, Resource.String.links);

            if (Folder.InternalType == FolderInternalType.FilterView
                || Folder.InternalType == FolderInternalType.Static
                || Folder.InternalType == FolderInternalType.Worktray)
            {
                menu.Add(Menu.None, 100, 100, Resource.String.delete_from_folder);
            }

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator
                || ServerConfig.SystemSettings.DocumentsModuleInfo.Permissions.DeleteAllowed)
            {
                menu.Add(Menu.None, 101, 101, Resource.String.delete);
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == 80)
            {
                var i = new Intent(Activity, typeof(ObjectActionsActivity));
                i.PutExtra(ObjectActionsActivity.BusinessEntityTypeIntentKey, SerializationUtils.Serialize(DocumentPreview.GetType()));
                i.PutExtra(ObjectActionsActivity.BusinessEntityIntentKey, SerializationUtils.Serialize(DocumentPreview));
                StartActivity(i);

                return true;
            }

            if (item.ItemId == 90)
            {
                var i = new Intent(Activity, typeof(ObjectLinksActivity));
                i.PutExtra(ObjectLinksActivity.BusinessEntityTypeIntentKey, SerializationUtils.Serialize(DocumentPreview.GetType()));
                i.PutExtra(ObjectLinksActivity.BusinessEntityIntentKey, SerializationUtils.Serialize(DocumentPreview));
                StartActivity(i);

                return true;
            }

            if (item.ItemId == 60)
            {
                var i = new Intent(Activity, typeof(CategoriesListActivity));
                i.PutExtra(CategoriesListActivity.BusinessEntityPreviewIntentKey, SerializationUtils.Serialize(DocumentPreview));
                Activity.StartActivityForResult(i, RequestCodes.CategoriesRequest);

                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        async void AttachmentsView_AttachmentClicked(object sender, AttachmentDescription attachmentDescription)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.opening_attachment, Resource.String.please_wait);

            try
            {
                var path = await Managers.DocumentsManager.GetAttachmentAsync(attachmentDescription, Document, Folder, false, SourceType.Local);
                if (string.IsNullOrWhiteSpace(path))
                {
                    if (attachmentDescription.SizeInBytes > LargeAttachmentSizeInBytes
                        && PlatformConfig.Preferences.LargeAttachmentWarning
                        && Integration.IsConnectedToMeteredConnection()
                        && !await Dialogs.ShowYesNoDialogAsync(Context, Resource.String.warning, Resource.String.large_attachment))
                    {
                        dismissAction();
                        return;
                    }

                    path = await Managers.DocumentsManager.GetAttachmentAsync(attachmentDescription, Document, Folder, false, SourceType.Remote);
                }

                var uri = FileProvider.GetUriForFile(Context, Context.PackageName + ".fileprovider", new Java.IO.File(path));
                var mimeType = Context.ContentResolver.GetType(uri);

                var openFileIntent = new Intent(Intent.ActionView);
                openFileIntent.SetDataAndType(uri, mimeType);
                openFileIntent.AddFlags(ActivityFlags.NewTask);
                openFileIntent.AddFlags(ActivityFlags.GrantReadUriPermission);
                Context.StartActivity(openFileIntent);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Failed to view attachment [document.Id={Document.Id}, attachment.Id={attachmentDescription.Id}, attachment.Name={attachmentDescription.Name}", ex);

                dismissAction();
                await Dialogs.ShowErrorDialogAsync(Context, ex);
            }
            finally
            {
                dismissAction();
            }
        }

        async void AttachmentsView_AttachmentLongClicked(object sender, AttachmentDescription attachmentDescription)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.opening_attachment, Resource.String.please_wait);

            try
            {
                var path = await Managers.DocumentsManager.GetAttachmentAsync(attachmentDescription, Document, Folder, false, SourceType.Local);
                if (string.IsNullOrWhiteSpace(path))
                {
                    if (attachmentDescription.SizeInBytes > LargeAttachmentSizeInBytes
                        && PlatformConfig.Preferences.LargeAttachmentWarning
                        && Integration.IsConnectedToMeteredConnection()
                        && !await Dialogs.ShowYesNoDialogAsync(Context, Resource.String.warning, Resource.String.large_attachment))
                    {
                        dismissAction();
                        return;
                    }

                    path = await Managers.DocumentsManager.GetAttachmentAsync(attachmentDescription, Document, Folder, false, SourceType.Remote);
                }

                var uri = FileProvider.GetUriForFile(Context, Context.PackageName + ".fileprovider", new Java.IO.File(path));
                var mimeType = Context.ContentResolver.GetType(uri);

                ShareCompat.IntentBuilder.From(Activity).SetType(mimeType).SetStream(uri).StartChooser();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Failed to share attachment [document.Id={Document.Id}, attachment.Id={attachmentDescription.Id}, attachment.Name={attachmentDescription.Name}", ex);

                dismissAction();
                await Dialogs.ShowErrorDialogAsync(Context, ex);
            }
            finally
            {
                dismissAction();
            }
        }

        async Task RefreshData()
        {
            try
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
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Downloading document failed [folder.name={Folder?.Name}, folder.id={FolderId ?? Folder?.Id}, documentId={DocumentId ?? DocumentPreview?.Id}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);

                if (CloseRequest != null) CloseRequest();
            }
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

        void RefreshView<T>() where T : DocumentView
        {
            progress.Visibility = ViewStates.Gone;
            scrollView.Visibility = ViewStates.Visible;

            for (var i = 0; i < linearLayout.ChildCount; i++)
            {
                var dv = linearLayout.GetChildAt(i) as T;
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

        void MarkAsReadIfNecessary()
        {
            setReadStatusCancellationTokenSource?.Cancel();
            setReadStatusCancellationTokenSource = new CancellationTokenSource();

            Task.Run(async () =>
            {
                var f = Folder;
                var d = Document;
                var dp = DocumentPreview;
                var token = setReadStatusCancellationTokenSource.Token;

                try
                {
                    if (dp.IsReadByCurrent)
                    {
                        return;
                    }

                    var delaySeconds = PlatformConfig.Preferences.MarkAsReadDelaySeconds;
                    if (delaySeconds < 0) return;

                    await Task.Delay(delaySeconds * 1000);

                    if (token.IsCancellationRequested) return;
                    await Managers.DocumentsManager.SetDocumentReadStatusAsync(dp, d, true, ServerConfig.SystemSettings.UserInfo.User);

                    Activity?.RunOnUiThread(() =>
                    {
                        if (token.IsCancellationRequested) return;
                        if (!IsAdded || IsDetached || IsRemoving) return;

                        RefreshView<RecipentsView>();
                        PlatformConfig.MessengerHub.Publish(new DocumentPreviewReadStatusChangedMessage(this, f.Id, dp.Id, dp.IsReadByCurrent, dp.IsReadByAnyone));
                    });
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error($"Marking document as read failed [folder.name={f?.Name}, folder.id={f?.Id}, documentPreviewId={dp?.Id}]", ex);
                }
            });
        }

        public void UpdateCategories(List<Category> categories)
        {
            DocumentPreview?.Categories.Clear();
            DocumentPreview?.Categories.AddRange(categories);
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
