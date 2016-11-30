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
using Android.App;
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
using Mark5.Mobile.Droid.Ui.Common.HubMessages;
using Mark5.Mobile.Droid.Ui.Views.Common;
using Mark5.Mobile.Droid.Ui.Views.DocumentViews;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{

    public class DocumentFragment : RetainableStateFragment
    {

        public static class RequestCodes
        {
            public static int CommentsRequest = 1;
            public static int CategoriesRequest = 2;
        }

        const int LargeAttachmentSizeInBytes = 20 * 1024 * 1024; // 20MB

        public int? FolderId { get; set; }
        public Folder Folder { get; set; }
        public int SearchId { get; set; }
        public int? DocumentId { get; set; }
        public DocumentPreview DocumentPreview { get; set; }
        public Document Document { get; set; }
        public bool ReadOnlyMode { get; set; } = false;
        public Action CloseRequest { get; set; }
        public Guid NotificationGuid { get; set; }

        ProgressBar progress;
        ScrollView scrollView;
        LinearLayoutCompat linearLayout;

        CancellationTokenSource setReadStatusCancellationTokenSource;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(DocumentFragment)} [folder.id={FolderId ?? Folder?.Id}, searchId={SearchId}, document.id={DocumentId ?? DocumentPreview?.Id ?? Document?.Id}, readOnlyMode={ReadOnlyMode}]...");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_with_progress, container, false);

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

            CommonConfig.Logger.Info($"Created {nameof(DocumentFragment)} [folder.id={FolderId ?? Folder?.Id}, searchId={SearchId}, document.id={DocumentId ?? DocumentPreview?.Id ?? Document?.Id}, readOnlyMode={ReadOnlyMode}]");
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

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            if (resultCode == (int)Result.Ok)
            {
                if (requestCode == RequestCodes.CommentsRequest)
                {
                    var comments = SerializationUtils.Deserialize<List<Comment>>(data.GetStringExtra(CommentsListActivity.CommentsResultKey));
                    UpdateComments(comments);
                }
                else if (requestCode == RequestCodes.CategoriesRequest)
                {
                    var categories = SerializationUtils.Deserialize<List<Category>>(data.GetStringExtra(CategoriesListActivity.CategoriesResultKey));
                    UpdateCategories(categories);
                }
            }
        }

        static class MenuItemActions
        {
            public const int MarkAsRead = 10;
            public const int MarkAsUnread = 11;
            public const int Reply = 20;
            public const int ReplyAll = 21;
            public const int Forward = 22;
            public const int CopyToWorktray = 30;
            public const int CopyToFolder = 40;
            public const int MoveToFolder = 41;
            public const int SetPriority = 50;
            public const int Categories = 60;
            public const int Comments = 70;
            public const int Actions = 80;
            public const int Links = 90;
            public const int DeleteFromFolder = 100;
            public const int Delete = 101;
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            if (ReadOnlyMode) return;
            if (DocumentPreview == null) return;

            if (!DocumentPreview.IsReadByCurrent)
            {
                menu.Add(Menu.None, MenuItemActions.MarkAsRead, MenuItemActions.MarkAsRead, Resource.String.mark_as_read);
            }

            if (DocumentPreview.IsReadByCurrent)
            {
                menu.Add(Menu.None, MenuItemActions.MarkAsUnread, MenuItemActions.MarkAsUnread, Resource.String.marks_as_unread);
            }

            menu.Add(Menu.None, MenuItemActions.Reply, MenuItemActions.Reply, Resource.String.reply);
            menu.Add(Menu.None, MenuItemActions.ReplyAll, MenuItemActions.ReplyAll, Resource.String.reply_all);
            menu.Add(Menu.None, MenuItemActions.Forward, MenuItemActions.Forward, Resource.String.forward);
            menu.Add(Menu.None, MenuItemActions.CopyToWorktray, MenuItemActions.CopyToWorktray, Resource.String.copy_to_worktray);
            menu.Add(Menu.None, MenuItemActions.CopyToFolder, MenuItemActions.CopyToFolder, Resource.String.copy_to_folder);

            if (Folder?.InternalType == FolderInternalType.FilterView
                || Folder?.InternalType == FolderInternalType.Static
                || Folder?.InternalType == FolderInternalType.Worktray)
            {
                menu.Add(Menu.None, MenuItemActions.MoveToFolder, MenuItemActions.MoveToFolder, Resource.String.move_to_folder);
            }

            menu.Add(Menu.None, MenuItemActions.SetPriority, MenuItemActions.SetPriority, Resource.String.set_priority);
            menu.Add(Menu.None, MenuItemActions.Categories, MenuItemActions.Categories, Resource.String.categories);

            if (Document != null)
            {
                menu.Add(Menu.None, MenuItemActions.Comments, MenuItemActions.Comments, Resource.String.comments);
            }

            menu.Add(Menu.None, MenuItemActions.Actions, MenuItemActions.Actions, Resource.String.actions);
            menu.Add(Menu.None, MenuItemActions.Links, MenuItemActions.Links, Resource.String.links);

            if (Folder?.InternalType == FolderInternalType.FilterView
                || Folder?.InternalType == FolderInternalType.Static
                || Folder?.InternalType == FolderInternalType.Worktray)
            {
                menu.Add(Menu.None, MenuItemActions.DeleteFromFolder, MenuItemActions.DeleteFromFolder, Resource.String.delete_from_folder);
            }

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator
                || ServerConfig.SystemSettings.DocumentsModuleInfo.Permissions.DeleteAllowed)
            {
                menu.Add(Menu.None, MenuItemActions.Delete, MenuItemActions.Delete, Resource.String.delete);
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == MenuItemActions.MarkAsRead)
            {
                MarkAsRead();
                return true;
            }

            if (item.ItemId == MenuItemActions.MarkAsUnread)
            {
                MarkAsUnread();
                return true;
            }

            if (item.ItemId == MenuItemActions.Reply)
            {
            }

            if (item.ItemId == MenuItemActions.ReplyAll)
            {
            }

            if (item.ItemId == MenuItemActions.Forward)
            {
            }

            if (item.ItemId == MenuItemActions.CopyToWorktray)
            {
                CopyToWorktrayAction();
                return true;
            }

            if (item.ItemId == MenuItemActions.CopyToFolder)
            {
                var i = new Intent(Activity, typeof(FolderListSelectionActivity));
                i.PutExtra(FolderListSelectionActivity.ModeIntentKey, (int)FolderListSelectionActivity.ModeType.CopyToFolderMode);
                i.PutExtra(FolderListSelectionActivity.ModuleIntentKey, SerializationUtils.Serialize(ModuleType.Documents));
                i.PutExtra(FolderListSelectionActivity.BusinessEntitiesIntentKey, SerializationUtils.Serialize(new List<IBusinessEntity> { DocumentPreview }));
                StartActivity(i);

                return true;
            }

            if (item.ItemId == MenuItemActions.MoveToFolder)
            {
                var i = new Intent(Activity, typeof(FolderListSelectionActivity));
                i.PutExtra(FolderListSelectionActivity.ModeIntentKey, (int)FolderListSelectionActivity.ModeType.MoveToFolderMode);
                i.PutExtra(FolderListSelectionActivity.ModuleIntentKey, SerializationUtils.Serialize(ModuleType.Documents));
                i.PutExtra(FolderListSelectionActivity.BusinessEntitiesIntentKey, SerializationUtils.Serialize(new List<IBusinessEntity> { DocumentPreview }));
                i.PutExtra(FolderListSelectionActivity.FromFolderIntentKey, SerializationUtils.Serialize(Folder));
                StartActivity(i);

                return true;
            }

            if (item.ItemId == MenuItemActions.SetPriority)
            {
                SetPriority();
                return true;
            }

            if (item.ItemId == MenuItemActions.Categories)
            {
                var i = new Intent(Activity, typeof(CategoriesListActivity));
                i.PutExtra(CategoriesListActivity.BusinessEntityPreviewIntentKey, SerializationUtils.Serialize(DocumentPreview));
                Activity.StartActivityForResult(i, RequestCodes.CategoriesRequest);

                return true;
            }

            if (item.ItemId == MenuItemActions.Comments)
            {
                var i = new Intent(Activity, typeof(CommentsListActivity));
                i.PutExtra(CommentsListActivity.EntityIntentKey, SerializationUtils.Serialize(Document));
                Activity.StartActivityForResult(i, RequestCodes.CommentsRequest);

                return true;
            }

            if (item.ItemId == MenuItemActions.Actions)
            {
                var i = new Intent(Activity, typeof(ObjectActionsActivity));
                i.PutExtra(ObjectActionsActivity.BusinessEntityIntentKey, SerializationUtils.Serialize(DocumentPreview as IBusinessEntity));
                StartActivity(i);

                return true;
            }

            if (item.ItemId == MenuItemActions.Links)
            {
                var i = new Intent(Activity, typeof(ObjectLinksActivity));
                i.PutExtra(ObjectLinksActivity.BusinessEntityIntentKey, SerializationUtils.Serialize(DocumentPreview as IBusinessEntity));
                StartActivity(i);

                return true;
            }

            if (item.ItemId == MenuItemActions.DeleteFromFolder)
            {
                DeleteFromFolderAction();
                return true;
            }

            if (item.ItemId == MenuItemActions.Delete)
            {
                DeleteAction();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        async void MarkAsRead()
        {
            CommonConfig.Logger.Info($"Attempting to mark as read [documentPreview={DocumentPreview}]...");

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.marking_as_read, Resource.String.please_wait);

            try
            {
                await Managers.DocumentsManager.SetDocumentReadStatusAsync(DocumentPreview, Document, true, ServerConfig.SystemSettings.UserInfo.User);

                RefreshView<RecipentsView>();
                PlatformConfig.MessengerHub.Publish(new DocumentPreviewReadStatusChangedMessage(this, DocumentPreview.Id, DocumentPreview.IsReadByCurrent, DocumentPreview.IsReadByAnyone));

                dismissAction();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Marking as read failed [documentPreview={DocumentPreview}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        async void MarkAsUnread()
        {
            CommonConfig.Logger.Info($"Attempting to mark as unread [documentPreview={DocumentPreview}]...");

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.marking_as_unread, Resource.String.please_wait);

            try
            {
                await Managers.DocumentsManager.SetDocumentReadStatusAsync(DocumentPreview, Document, false, ServerConfig.SystemSettings.UserInfo.User);

                RefreshView<RecipentsView>();
                PlatformConfig.MessengerHub.Publish(new DocumentPreviewReadStatusChangedMessage(this, DocumentPreview.Id, DocumentPreview.IsReadByCurrent, DocumentPreview.IsReadByAnyone));

                dismissAction();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Marking as unread failed [documentPreview={DocumentPreview}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        async void CopyToWorktrayAction()
        {
            var option = await Dialogs.ShowListDialog(Context, Resource.String.copy_to_worktray, Resource.Array.copy_to_worktray_options);

            if (option == 0)
            {
                CommonConfig.Logger.Info($"Attempting copy to worktray [documentPreview={DocumentPreview}]...");

                var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.copying_to_worktray, Resource.String.please_wait);

                try
                {
                    await Managers.CommonActionsManager.CopyToWorktray(new List<IBusinessEntity> { DocumentPreview });

                    dismissAction();
                }
                catch (Exception ex)
                {
                    dismissAction();

                    CommonConfig.Logger.Error($"Copying to worktray failed [documentPreview={DocumentPreview}]", ex);

                    await Dialogs.ShowErrorDialogAsync(Activity, ex);
                }
            }

            if (option == 1)
            {
                StartActivity(CopyToUserWorktrayActivity.CreateIntent(Activity, new List<IBusinessEntity> { DocumentPreview }));
            }
        }

        async void SetPriority()
        {
            var priority = await Dialogs.ShowSingleSelectDialogAsync(Context, Resource.String.set_priority, new List<Priority> { Priority.Urgent, Priority.Normal, Priority.Low }, DocumentPreview.Priority);
            if (priority == default(Priority) || priority == DocumentPreview.Priority)
            {
                return;
            }

            CommonConfig.Logger.Info($"Attempting to set priority [documentPreview={DocumentPreview}]...");

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.setting_priority, Resource.String.please_wait);

            try
            {
                await Managers.DocumentsManager.SetDocumentsPriorityAsync(new List<DocumentPreview> { DocumentPreview }, priority);

                RefreshView<PriorityView>();
                PlatformConfig.MessengerHub.Publish(new DocumentPreviewPriorityChangedMessage(this, DocumentPreview.Id, DocumentPreview.Priority));

                dismissAction();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Setting priority failed [documentPreview={DocumentPreview}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        async void DeleteFromFolderAction()
        {
            var yesNo = await Dialogs.ShowYesNoDialogAsync(Context, Resource.String.delete_from_folder, Resource.String.delete_from_folder_are_you_sure);
            if (!yesNo)
            {
                return;
            }

            CommonConfig.Logger.Info($"Attempting to delete from folder [documentPreview={DocumentPreview}]...");

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.deleting_from_folder, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.RemoveFromFolder(new List<IBusinessEntity> { DocumentPreview }, Folder);

                PlatformConfig.MessengerHub.Publish(new EntityRemovedFromFolderMessage(this, ObjectType.Document, FolderId ?? Folder.Id, new List<int> { DocumentPreview.Id }));

                dismissAction();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Deleting from folder failed [documentPreview={DocumentPreview}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        async void DeleteAction()
        {
            var yesNo = await Dialogs.ShowYesNoDialogAsync(Context, Resource.String.delete, Resource.String.delete_are_you_sure);
            if (!yesNo)
            {
                return;
            }

            CommonConfig.Logger.Info($"Attempting to delete [documentPreview={DocumentPreview}]...");

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.deleting, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.Delete(new List<IBusinessEntity> { DocumentPreview });

                PlatformConfig.MessengerHub.Publish(new EntityRemovedMessage(this, ObjectType.Document, new List<int> { DocumentPreview.Id }));

                dismissAction();
                if (CloseRequest != null) CloseRequest();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Deleting failed [documentPreview={DocumentPreview}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        async void AttachmentsView_AttachmentClicked(object sender, AttachmentDescription attachmentDescription)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.opening_attachment, Resource.String.please_wait);

            try
            {
                string path = null;

                if (Folder != null)
                {
                    path = await Managers.DocumentsManager.GetAttachmentAsync(attachmentDescription, Folder, Document, false, SourceType.Local);
                }
                if (SearchId <= -999)
                {
                    path = await Managers.DocumentsManager.GetAttachmentAsync(attachmentDescription, SearchId, Document, false, SourceType.Local);
                }

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

                    if (Folder != null)
                    {
                        path = await Managers.DocumentsManager.GetAttachmentAsync(attachmentDescription, Folder, Document, false, SourceType.Remote);
                    }
                    if (SearchId <= -999)
                    {
                        path = await Managers.DocumentsManager.GetAttachmentAsync(attachmentDescription, SearchId, Document, false, SourceType.Remote);
                    }
                }

                if (string.IsNullOrWhiteSpace(path))
                {
                    throw new Exception("Unable to get attachment path.");
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
                CommonConfig.Logger.Error($"Failed to view attachment [document.Id={Document.Id}, attachment.Id={attachmentDescription?.Id}, attachment.Name={attachmentDescription?.Name}", ex);

                dismissAction();
                await Dialogs.ShowErrorDialogAsync(Activity, ex);
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
                string path = null;

                if (Folder != null)
                {
                    path = await Managers.DocumentsManager.GetAttachmentAsync(attachmentDescription, Folder, Document, false, SourceType.Local);
                }
                if (SearchId <= -999)
                {
                    path = await Managers.DocumentsManager.GetAttachmentAsync(attachmentDescription, SearchId, Document, false, SourceType.Local);
                }

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

                    if (Folder != null)
                    {
                        path = await Managers.DocumentsManager.GetAttachmentAsync(attachmentDescription, Folder, Document, false, SourceType.Remote);
                    }
                    if (SearchId <= -999)
                    {
                        path = await Managers.DocumentsManager.GetAttachmentAsync(attachmentDescription, SearchId, Document, false, SourceType.Remote);
                    }
                }

                if (string.IsNullOrWhiteSpace(path))
                {
                    throw new Exception("Unable to get attachment path.");
                }

                var uri = FileProvider.GetUriForFile(Context, Context.PackageName + ".fileprovider", new Java.IO.File(path));
                var mimeType = Context.ContentResolver.GetType(uri);

                ShareCompat.IntentBuilder.From(Activity).SetType(mimeType).SetStream(uri).StartChooser();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Failed to share attachment [document.Id={Document.Id}, attachment.Id={attachmentDescription?.Id}, attachment.Name={attachmentDescription?.Name}", ex);

                dismissAction();
                await Dialogs.ShowErrorDialogAsync(Activity, ex);
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
                if (NotificationGuid != default(Guid))
                {
                    await Managers.NotificationsManager.MarkAsRead(NotificationGuid);
                }

                if (Folder != null || FolderId.HasValue)
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
                }

                if (SearchId <= -999)
                {
                    if (DocumentPreview != null && Document == null)
                    {
                        Document = await Managers.SearchManager.GetDocumentAsync(SearchId, DocumentPreview);
                    }
                }

                RefreshView();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Downloading document failed [folder.name={Folder?.Name}, searchId={SearchId}, folder.id={FolderId ?? Folder?.Id}, documentId={DocumentId ?? DocumentPreview?.Id}, readOnlyMode={ReadOnlyMode}]", ex);

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

            Activity.InvalidateOptionsMenu();
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

            Activity.InvalidateOptionsMenu();
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
                        if (dp == null) return;

                        RefreshView<RecipentsView>();
                        PlatformConfig.MessengerHub.Publish(new DocumentPreviewReadStatusChangedMessage(this, dp.Id, dp.IsReadByCurrent, dp.IsReadByAnyone));
                    });
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error($"Marking document as read failed [folder.name={f?.Name}, folder.id={f?.Id}, documentPreviewId={dp?.Id}]", ex);
                }
            });
        }

        void UpdateCategories(List<Category> categories)
        {
            DocumentPreview?.Categories.Clear();
            DocumentPreview?.Categories.AddRange(categories);
        }

        void UpdateComments(List<Comment> comments)
        {
            if (Document != null)
            {
                Document.Comments.Clear();
                Document.Comments.AddRange(comments);
            }

            if (DocumentPreview != null)
            {
                DocumentPreview.CommentsCount = comments.Count;
            }
        }

        public override IRetainableState OnRetainInstanceState()
        {
            return new DocumentFragmentState
            {
                FolderId = FolderId,
                Folder = Folder,
                SearchId = SearchId,
                DocumentId = DocumentId,
                DocumentPreview = DocumentPreview,
                Document = Document,
                ReadOnlyMode = ReadOnlyMode
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var dfs = restoredState as DocumentFragmentState;
            if (dfs != null)
            {
                FolderId = dfs.FolderId;
                Folder = dfs.Folder;
                SearchId = dfs.SearchId;
                DocumentId = dfs.DocumentId;
                DocumentPreview = dfs.DocumentPreview;
                Document = dfs.Document;
                ReadOnlyMode = dfs.ReadOnlyMode;
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(DocumentFragment)} [DocumentId={DocumentPreview?.Id ?? Document?.Id ?? DocumentId}]";
        }

        class DocumentFragmentState : IRetainableState
        {

            public int? FolderId { get; set; }

            public Folder Folder { get; set; }

            public int SearchId { get; set; }

            public int? DocumentId { get; set; }

            public DocumentPreview DocumentPreview { get; set; }

            public Document Document { get; set; }

            public bool ReadOnlyMode { get; set; }
        }
    }
}
