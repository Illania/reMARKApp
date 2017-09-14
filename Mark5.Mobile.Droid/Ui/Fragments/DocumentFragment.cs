using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Model.HubMessages;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.Common;
using Mark5.Mobile.Droid.Ui.Views.DocumentViews;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class DocumentFragment : RetainableStateFragment
    {
        const string FolderIdBundleKey = "FolderId_7215e56b-5ec3-436d-b4e3-900449cb1ad0";
        const string FolderBundleKey = "Folder_592db8d9-d212-4476-ac83-fd4bb11cc8d9";
        const string DocumentPreviewBundleKey = "DocumentPreview_83a5daa4-7ede-453b-9b19-07362f644ad1";
        const string DocumentIdBundleKey = "DocumentId_e9409d8a-9212-4483-b819-ff5ac3487c87";
        const string CloseRequestBundleKey = "CloseRequest_d45a15b4-dadb-40ae-aab1-c565e9446bd0";
        const string NotificationGuidBundleKey = "NotificationGuid_fb411b05-9d4b-46db-aa81-7348bf069a38";
        const string FailedDocumentToUploadGuidBundleKey = "FailedDocumentToUploadGuid_7bf6c332-083d-4f2b-950e-d3bdc3992e2b";

        const int LargeAttachmentSizeInBytes = 20 * 1024 * 1024; // 20MB

        Guid FailedDocumentToUploadGuid;
        int? FolderId;
        Folder Folder;
        int? DocumentId;
        DocumentPreview DocumentPreview;
        Document Document;
        Guid NotificationGuid;

        ProgressBar progress;
        RelativeLayout relativeLayout;
        LinearLayoutCompat linearLayout;
        AppCompatImageView button1;
        AppCompatImageView button2;
        AppCompatImageView button3;

        CancellationTokenSource setReadStatusCancellationTokenSource;

        public static (DocumentFragment fragment, string tag) NewInstance(Folder folder = null, int? folderId = null, DocumentPreview dp = null, int? docId = null, Guid? notificationGuid = null, Guid? failDocToUploadGuid = null)
        {
            var args = new Bundle();

            if (folder != null)
                args.PutString(FolderBundleKey, Serializer.Serialize(folder));

            if (folderId != null)
                args.PutInt(FolderIdBundleKey, folderId.Value);

            if (dp != null)
                args.PutString(DocumentPreviewBundleKey, Serializer.Serialize(dp));

            if (docId != null)
                args.PutInt(DocumentIdBundleKey, docId.Value);

            if (notificationGuid != null)
                args.PutString(NotificationGuidBundleKey, Serializer.Serialize(notificationGuid));

            if (failDocToUploadGuid != null)
                args.PutString(FailedDocumentToUploadGuidBundleKey, Serializer.Serialize(failDocToUploadGuid));

            var fragment = new DocumentFragment();
            fragment.Arguments = args;

            //old tag = $"{nameof(DocumentFragment)} [DocumentId={dp?.Id ?? Document?.Id ?? docId}]"; -> Document only changed in this fragment after instantiation.
            var tag = $"{nameof(DocumentFragment)} [DocumentId={dp?.Id ?? docId}]";

            return (fragment, tag);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (Arguments.ContainsKey(FolderBundleKey))
                Folder = Serializer.Deserialize<Folder>(Arguments.GetString(FolderBundleKey));

            if (Arguments.ContainsKey(FolderIdBundleKey))
                FolderId = Arguments.GetInt(FolderIdBundleKey);

            if (Arguments.ContainsKey(DocumentPreviewBundleKey))
                DocumentPreview = Serializer.Deserialize<DocumentPreview>(Arguments.GetString(DocumentPreviewBundleKey));

            if (Arguments.ContainsKey(DocumentIdBundleKey))
                DocumentId = Arguments.GetInt(DocumentIdBundleKey);

            if (Arguments.ContainsKey(NotificationGuidBundleKey))
                NotificationGuid = Serializer.Deserialize<Guid>(Arguments.GetString(NotificationGuidBundleKey));

            if (Arguments.ContainsKey(FailedDocumentToUploadGuidBundleKey))
                FailedDocumentToUploadGuid = Serializer.Deserialize<Guid>(Arguments.GetString(FailedDocumentToUploadGuidBundleKey));

            CommonConfig.Logger.Info($"Creating {nameof(DocumentFragment)} [folder.id={FolderId ?? Folder?.Id}, document.id={DocumentId ?? DocumentPreview?.Id ?? Document?.Id}]...");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_with_buttons_and_progress, container, false);

            progress = rootView.FindViewById<ProgressBar>(Resource.Id.progress);
            relativeLayout = rootView.FindViewById<RelativeLayout>(Resource.Id.relative_layout);
            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);
            button1 = rootView.FindViewById<AppCompatImageView>(Resource.Id.button1);
            button2 = rootView.FindViewById<AppCompatImageView>(Resource.Id.button2);
            button3 = rootView.FindViewById<AppCompatImageView>(Resource.Id.button3);

            button1.SetImageResource(Resource.Drawable.reply);
            button1.SetColorFilter(new Android.Graphics.Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));
            button1.Enabled = false;
            button1.Clickable = true;
            button1.Click += (sender, e) =>
            {
                if (DocumentPreview == null || Document == null)
                    return;

                if (!ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines.Any())
                {
                    Dialogs.ShowConfirmDialog(Activity, Resource.String.no_lines_error_title, Resource.String.no_lines_error_content);
                    return;
                }

                StartActivity(ComposeDocumentActivity.CreateIntent(Context,
                                                                   DocumentCreationModeFlag.Reply,
                                                                   CopyToNewOption.None,
                                                                   previousDocumentDirection: DocumentPreview.Direction,
                                                                   previousDocumentFolderId: Folder?.Id ?? FolderId,
                                                                   previousDocumentId: DocumentPreview.Id));
            };
            button1.LongClickable = true;
            button1.LongClick += (sender, e) => Toast.MakeText(Context, Resource.String.reply, ToastLength.Short).Show();

            button2.SetImageResource(Resource.Drawable.replyall);
            button2.SetColorFilter(new Android.Graphics.Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));
            button2.Enabled = false;
            button2.Clickable = true;
            button2.Click += (sender, e) =>
            {
                if (DocumentPreview == null || Document == null)
                    return;

                if (!ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines.Any())
                {
                    Dialogs.ShowConfirmDialog(Activity, Resource.String.no_lines_error_title, Resource.String.no_lines_error_content);
                    return;
                }
                StartActivity(ComposeDocumentActivity.CreateIntent(Context,
                                                                   DocumentCreationModeFlag.ReplyAll,
                                                                   CopyToNewOption.None,
                                                                   previousDocumentDirection: DocumentPreview.Direction,
                                                                   previousDocumentFolderId: Folder?.Id ?? FolderId,
                                                                   previousDocumentId: DocumentPreview.Id));
            };
            button2.LongClickable = true;
            button2.LongClick += (sender, e) => Toast.MakeText(Context, Resource.String.reply_all, ToastLength.Short).Show();

            button3.SetImageResource(Resource.Drawable.forward);
            button3.SetColorFilter(new Android.Graphics.Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));
            button3.Enabled = false;
            button3.Clickable = true;
            button3.Click += (sender, e) =>
            {
                if (DocumentPreview == null || Document == null)
                    return;

                if (!ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines.Any())
                {
                    Dialogs.ShowConfirmDialog(Activity, Resource.String.no_lines_error_title, Resource.String.no_lines_error_content);
                    return;
                }

                StartActivity(ComposeDocumentActivity.CreateIntent(Context,
                                                                   DocumentCreationModeFlag.Forward,
                                                                   CopyToNewOption.None,
                                                                   previousDocumentDirection: DocumentPreview.Direction,
                                                                   previousDocumentFolderId: Folder?.Id ?? FolderId,
                                                                   previousDocumentId: DocumentPreview.Id));
            };
            button3.LongClickable = true;
            button3.LongClick += (sender, e) => Toast.MakeText(Context, Resource.String.forward, ToastLength.Short).Show();

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
            linearLayout.AddView(new ContentView(Context));

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = string.Empty;
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = null;

            CommonConfig.Logger.Info($"Created {nameof(DocumentFragment)} [folder.id={FolderId ?? Folder?.Id}, document.id={DocumentId ?? DocumentPreview?.Id ?? Document?.Id}]");
        }

        public override async void OnResume()
        {
            base.OnResume();

            await RefreshData();

            if (!IsAdded || IsDetached || IsRemoving)
                return;
            if ((Activity is SwipeDocumentActivity) && !UserVisibleHint)
                return;

            await MarkAsReadIfNecessary();
        }

        public override async void OnUserVisibilityHintChanged()
        {
            base.OnUserVisibilityHintChanged();

            if (UserVisibleHint)
            {
                await MarkAsReadIfNecessary();
            }
            else
            {
                setReadStatusCancellationTokenSource?.Cancel();
                setReadStatusCancellationTokenSource = null;
            }
        }

        public override void OnDestroyedByUser()
        {
            base.OnDestroyedByUser();

            setReadStatusCancellationTokenSource?.Cancel();
            setReadStatusCancellationTokenSource = null;
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            if (resultCode == (int)Result.Ok)
                if (requestCode == RequestCodes.CommentsRequest)
                {
                    var comments = Serializer.Deserialize<List<Comment>>(data.GetStringExtra(CommentsListActivity.CommentsResultKey));
                    UpdateComments(comments);
                }
                else if (requestCode == RequestCodes.CategoriesRequest)
                {
                    var categories = Serializer.Deserialize<List<Category>>(data.GetStringExtra(CategoriesListActivity.CategoriesResultKey));
                    UpdateCategories(categories);
                }
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            if (DocumentPreview == null)
                return;

            if (FailedDocumentToUploadGuid != Guid.Empty)
                return;

            if (Activity is SwitchDocumentActivity && Folder != null)
            {
                var goToPreviousItem = menu.Add(Menu.None, MenuItemActions.GoToPrevious, MenuItemActions.GoToPrevious, Resource.String.document_previous);
                goToPreviousItem.SetShowAsAction(ShowAsAction.Always);

                var goToNextItem = menu.Add(Menu.None, MenuItemActions.GoToNext, MenuItemActions.GoToNext, Resource.String.document_next);
                goToNextItem.SetShowAsAction(ShowAsAction.Always);
            }

            if (!DocumentPreview.IsReadByCurrent)
                menu.Add(Menu.None, MenuItemActions.MarkAsRead, MenuItemActions.MarkAsRead, Resource.String.mark_as_read);

            if (DocumentPreview.IsReadByCurrent)
                menu.Add(Menu.None, MenuItemActions.MarkAsUnread, MenuItemActions.MarkAsUnread, Resource.String.marks_as_unread);

            menu.Add(Menu.None, MenuItemActions.CopyToWorktray, MenuItemActions.CopyToWorktray, Resource.String.copy_to_worktray);
            menu.Add(Menu.None, MenuItemActions.CopyToFolder, MenuItemActions.CopyToFolder, Resource.String.copy_to_folder);

            if (Folder?.InternalType == FolderInternalType.FilterView || Folder?.InternalType == FolderInternalType.Static || Folder?.InternalType == FolderInternalType.Worktray)
                menu.Add(Menu.None, MenuItemActions.MoveToFolder, MenuItemActions.MoveToFolder, Resource.String.move_to_folder);

            menu.Add(Menu.None, MenuItemActions.SetPriority, MenuItemActions.SetPriority, Resource.String.set_priority);
            menu.Add(Menu.None, MenuItemActions.Categories, MenuItemActions.Categories, Resource.String.categories);

            if (Document != null)
                menu.Add(Menu.None, MenuItemActions.Comments, MenuItemActions.Comments, Resource.String.comments);

            menu.Add(Menu.None, MenuItemActions.Actions, MenuItemActions.Actions, Resource.String.actions);
            menu.Add(Menu.None, MenuItemActions.Links, MenuItemActions.Links, Resource.String.links);

            if (Folder?.InternalType == FolderInternalType.FilterView || Folder?.InternalType == FolderInternalType.Static || Folder?.InternalType == FolderInternalType.Worktray)
                menu.Add(Menu.None, MenuItemActions.DeleteFromFolder, MenuItemActions.DeleteFromFolder, Resource.String.delete_from_folder);

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator || ServerConfig.SystemSettings.DocumentsModuleInfo.Permissions.DeleteAllowed || DocumentPreview.Direction == DocumentDirection.Draft)
                menu.Add(Menu.None, MenuItemActions.Delete, MenuItemActions.Delete, Resource.String.delete);

            menu.Add(Menu.None, MenuItemActions.CopyToNew, MenuItemActions.CopyToNew, Resource.String.copy_to_new);

        }

        public override async void OnPrepareOptionsMenu(IMenu menu)
        {
            var isDocumentReady = Document != null;

            var menuItemIds = new List<int>
            {
                MenuItemActions.Comments
            };
            foreach (var itemId in menuItemIds)
            {
                var menuItem = menu.FindItem(itemId);
                menuItem?.SetEnabled(isDocumentReady);
            }

            if (Activity is SwitchDocumentActivity && Folder != null)
            {
                var goToPreviousItem = menu.FindItem(MenuItemActions.GoToPrevious);
                if (goToPreviousItem != null)
                    goToPreviousItem.SetEnabled(await ((SwitchDocumentActivity)Activity).HasPrevious(DocumentId ?? DocumentPreview.Id));

                var goToNextItem = menu.FindItem(MenuItemActions.GoToNext);
                if (goToNextItem != null)
                    goToNextItem.SetEnabled(await ((SwitchDocumentActivity)Activity).HasNext(DocumentId ?? DocumentPreview.Id));
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (Activity is SwitchDocumentActivity && item.ItemId == MenuItemActions.GoToPrevious)
                ((SwitchDocumentActivity)Activity).GoToPrevious(DocumentId ?? DocumentPreview.Id);

            if (Activity is SwitchDocumentActivity && item.ItemId == MenuItemActions.GoToNext)
                ((SwitchDocumentActivity)Activity).GoToNext(DocumentId ?? DocumentPreview.Id);

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

            if (item.ItemId == MenuItemActions.CopyToWorktray)
            {
                CopyToWorktrayAction();
                return true;
            }

            if (item.ItemId == MenuItemActions.CopyToFolder)
            {
                StartActivity(CopyMoveToFolderListActivity.CreateIntent(Context, 
                                                                        (int)CopyMoveToFolderListActivity.ModeType.Copy, 
                                                                        ModuleType.Documents,
                                                                        new List<IBusinessEntity>
                                                                        {
                                                                            DocumentPreview
                                                                        }));
                return true;
            }

            if (item.ItemId == MenuItemActions.MoveToFolder)
            {
                StartActivity(CopyMoveToFolderListActivity.CreateIntent(Context,
                                                                        (int)CopyMoveToFolderListActivity.ModeType.Move,
                                                                        ModuleType.Documents,
                                                                        new List<IBusinessEntity>
                                                                        {
                                                                            DocumentPreview
                                                                        },
                                                                        Folder));
                return true;
            }

            if (item.ItemId == MenuItemActions.SetPriority)
            {
                SetPriority();
                return true;
            }

            if (item.ItemId == MenuItemActions.Categories)
            {
                StartActivityForResult(CategoriesListActivity.CreateIntent(Context, documentPreview:DocumentPreview), RequestCodes.CategoriesRequest);
                return true;
            }

            if (item.ItemId == MenuItemActions.Comments)
            {
                StartActivityForResult(CommentsListActivity.CreateIntent(Context, document:Document), RequestCodes.CommentsRequest);
                return true;
            }

            if (item.ItemId == MenuItemActions.Actions)
            {
                StartActivity(ObjectActionsActivity.CreateIntent(Context, DocumentPreview as IBusinessEntity));
                return true;
            }

            if (item.ItemId == MenuItemActions.Links)
            {
                StartActivity(ObjectLinksActivity.CreateIntent(Context, DocumentPreview as IBusinessEntity));
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

            if (item.ItemId == MenuItemActions.CopyToNew)
            {
                CopyToNew();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        public override IRetainableState OnRetainInstanceState()
        {
            return new DocumentFragmentState
            {
                FolderId = FolderId,
                Folder = Folder,
                DocumentId = DocumentId,
                DocumentPreview = DocumentPreview
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            if (restoredState is DocumentFragmentState dfs)
            {
                FolderId = dfs.FolderId;
                Folder = dfs.Folder;
                DocumentId = dfs.DocumentId;
                DocumentPreview = dfs.DocumentPreview;
            }
        }

        void RefreshView()
        {
            var activateButtons = FailedDocumentToUploadGuid == Guid.Empty;
            if (activateButtons)
            {
                button1.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = true;
                button1.Alpha = 1f;
                button2.Alpha = 1f;
                button3.Alpha = 1f;
            }
            else
            {
                button1.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = false;
                button1.Alpha = .5f;
                button2.Alpha = .5f;
                button3.Alpha = .5f;
            }

            progress.Visibility = ViewStates.Gone;
            relativeLayout.Visibility = ViewStates.Visible;

            for (var i = 0; i < linearLayout.ChildCount; i++)
            {
                if (linearLayout.GetChildAt(i) is DocumentView dv)
                {
                    dv.DocumentPreview = DocumentPreview;
                    dv.Document = Document;
                    dv.RefreshView();

                    if (linearLayout.GetChildAt(i + 1) is Divider d)
                    {
                        d.Visibility = dv.Visibility;
                        i++;
                    }
                }
            }

            linearLayout.Invalidate();
            linearLayout.RequestLayout();

            Activity?.InvalidateOptionsMenu();
        }

        void RefreshView<T>() where T : DocumentView
        {
            progress.Visibility = ViewStates.Gone;
            relativeLayout.Visibility = ViewStates.Visible;

            for (var i = 0; i < linearLayout.ChildCount; i++)
            {
                if (linearLayout.GetChildAt(i) is T dv)
                {
                    dv.DocumentPreview = DocumentPreview;
                    dv.Document = Document;
                    dv.RefreshView();

                    if (linearLayout.GetChildAt(i + 1) is Divider d)
                    {
                        d.Visibility = dv.Visibility;
                        i++;
                    }
                }
            }

            linearLayout.Invalidate();
            linearLayout.RequestLayout();

            Activity?.InvalidateOptionsMenu();
        }

        async Task MarkAsReadIfNecessary()
        {
            setReadStatusCancellationTokenSource?.Cancel();
            setReadStatusCancellationTokenSource = new CancellationTokenSource();

            var d = Document;
            var dp = DocumentPreview;
            var token = setReadStatusCancellationTokenSource.Token;

            try
            {
                if (dp == null || d == null)
                    return;

                if (dp.IsReadByCurrent)
                    return;

                var delaySeconds = PlatformConfig.Preferences.MarkAsReadDelaySeconds;
                if (delaySeconds < 0)
                    return;

                await Task.Delay(delaySeconds * 1000);

                if (token.IsCancellationRequested)
                    return;

                await Managers.DocumentsManager.SetDocumentReadStatusAsync(dp, d, true, ServerConfig.SystemSettings.UserInfo.User);
         
                if (token.IsCancellationRequested)
                    return;

                if (!IsAdded || IsDetached || IsRemoving)
                    return;

                if (dp == null)
                    return;

                RefreshView<RecipentsView>();
                CommonConfig.MessengerHub.Publish(new DocumentPreviewReadStatusChangedMessage(this, dp.Id, dp.IsReadByCurrent, dp.IsReadByAnyone));          
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Marking document as read failed [documentPreviewId={dp?.Id}]", ex);
            }
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
                DocumentPreview.CommentsCount = comments.Count;
        }

        async void MarkAsRead()
        {
            CommonConfig.Logger.Info($"Attempting to mark as read [documentPreview={DocumentPreview}]...");

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.marking_as_read, Resource.String.please_wait);

            try
            {
                await Managers.DocumentsManager.SetDocumentReadStatusAsync(DocumentPreview, Document, true, ServerConfig.SystemSettings.UserInfo.User);

                RefreshView<RecipentsView>();
                CommonConfig.MessengerHub.Publish(new DocumentPreviewReadStatusChangedMessage(this, DocumentPreview.Id, DocumentPreview.IsReadByCurrent, DocumentPreview.IsReadByAnyone));

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
                CommonConfig.MessengerHub.Publish(new DocumentPreviewReadStatusChangedMessage(this, DocumentPreview.Id, DocumentPreview.IsReadByCurrent, DocumentPreview.IsReadByAnyone));

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
            var option = await Dialogs.ShowListDialog(Context, Resource.String.copy_to_worktray, Resource.Array.copy_to_worktray_options, true);

            if (option == 0)
            {
                CommonConfig.Logger.Info($"Attempting copy to worktray [documentPreview={DocumentPreview}]...");

                var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.copying_to_worktray, Resource.String.please_wait);

                try
                {
                    await Managers.CommonActionsManager.CopyToWorktray(new List<IBusinessEntity>
                    {
                        DocumentPreview
                    });

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
                StartActivity(CopyToUserWorktrayActivity.CreateIntent(Context,new List<IBusinessEntity> { DocumentPreview }));
            }
        }

        async void SetPriority()
        {
            var possiblePriorities = new List<Priority> { Priority.Urgent, Priority.Normal, Priority.Low };
            var documentPriority = DocumentPreview.Priority;

            if (!possiblePriorities.Contains(documentPriority))
                documentPriority = Priority.Normal;

            var priority = await Dialogs.ShowSingleSelectDialogAsync(Context, Resource.String.set_priority, possiblePriorities, documentPriority);
            if (priority == default(Priority) || priority == DocumentPreview.Priority)
                return;

            CommonConfig.Logger.Info($"Attempting to set priority [documentPreview={DocumentPreview}]...");

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.setting_priority, Resource.String.please_wait);

            try
            {
                await Managers.DocumentsManager.SetDocumentsPriorityAsync(new List<DocumentPreview>
                    {
                        DocumentPreview
                    },
                    priority);

                RefreshView<PriorityView>();
                CommonConfig.MessengerHub.Publish(new DocumentPreviewPriorityChangedMessage(this, DocumentPreview.Id, DocumentPreview.Priority));

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
                return;

            CommonConfig.Logger.Info($"Attempting to delete from folder [documentPreview={DocumentPreview}]...");

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.deleting_from_folder, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.RemoveFromFolder(new List<IBusinessEntity>
                    {
                        DocumentPreview
                    },
                    Folder);

                CommonConfig.MessengerHub.Publish(new EntityRemovedFromFolderMessage(this,
                    ObjectType.Document,
                    Folder.Id,
                    new List<int>
                    {
                        DocumentPreview.Id
                    }));

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
                return;

            CommonConfig.Logger.Info($"Attempting to delete [documentPreview={DocumentPreview}]...");

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.deleting, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.Delete(new List<IBusinessEntity>
                {
                    DocumentPreview
                });

                CommonConfig.MessengerHub.Publish(new EntityRemovedMessage(this,
                    ObjectType.Document,
                    new List<int>
                    {
                        DocumentPreview.Id
                    }));

                dismissAction();
                Activity?.OnBackPressed();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Deleting failed [documentPreview={DocumentPreview}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        async void CopyToNew()
        {
            if (Document == null || DocumentPreview == null)
                return;

            var hasAttachments = Document.Attachments.Any();

            var choice = await Dialogs.ShowListDialog(Context, Resource.String.copy_to_new_mode_title,
                                                      hasAttachments ? Resource.Array.copy_to_new_modes : Resource.Array.copy_to_new_modes_no_attachments, true);

            if (choice < 0)
                return;

            CopyToNewOption option = CopyToNewOption.None;
            switch (choice)
            {
                case 0:
                    option = CopyToNewOption.KeepOnlyAddresses;
                    break;
                case 1:
                    option = CopyToNewOption.KeepTextAndAttachments;
                    break;
                case 2:
                    option = CopyToNewOption.KeepOnlyAttachments;
                    break;
            }

            StartActivity(ComposeDocumentActivity.CreateIntent(Context,
                                                               DocumentCreationModeFlag.New,
                                                               option,
                                                               previousDocumentDirection: DocumentPreview.Direction,
                                                               previousDocumentFolderId: Folder?.Id ?? FolderId,
                                                               previousDocumentId: DocumentPreview.Id));
        }

        async void AttachmentsView_AttachmentClicked(object sender, AttachmentDescription attachmentDescription)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.opening_attachment, Resource.String.please_wait);

            try
            {
                var path = await Managers.DocumentsManager.GetAttachmentAsync(attachmentDescription, Document, false, SourceType.Local);

                if (string.IsNullOrWhiteSpace(path))
                {
                    if (attachmentDescription.SizeInBytes > LargeAttachmentSizeInBytes && PlatformConfig.Preferences.LargeAttachmentWarning && Integration.IsConnectedToMeteredConnection() && !await Dialogs.ShowYesNoDialogAsync(Context, Resource.String.warning, Resource.String.large_attachment))
                    {
                        dismissAction();
                        return;
                    }

                    path = await Managers.DocumentsManager.GetAttachmentAsync(attachmentDescription, Document, false, SourceType.Remote);
                }

                if (string.IsNullOrWhiteSpace(path))
                    throw new Exception("Unable to open attachment");

                var uri = FileProvider.GetUriForFile(Context, Context.PackageName + ".fileprovider", new Java.IO.File(path));
                var mimeType = MimeTypeMap.GetMimeType(Path.GetExtension(path));

                var openFileIntent = new Intent(Intent.ActionView);
                openFileIntent.SetDataAndType(uri, mimeType);
                openFileIntent.AddFlags(ActivityFlags.NewTask);
                openFileIntent.AddFlags(ActivityFlags.GrantReadUriPermission);

                var canOpen = Context.PackageManager.QueryIntentActivities(openFileIntent, 0).Any();
                if (canOpen)
                    Context.StartActivity(openFileIntent);
                else
                    await Dialogs.ShowConfirmDialogAsync(Context, Resource.String.attachment_cannot_be_opened_title, Resource.String.attachment_cannot_be_opened_summary);
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
                var path = await Managers.DocumentsManager.GetAttachmentAsync(attachmentDescription, Document, false, SourceType.Local);

                if (string.IsNullOrWhiteSpace(path))
                {
                    if (attachmentDescription.SizeInBytes > LargeAttachmentSizeInBytes && PlatformConfig.Preferences.LargeAttachmentWarning && Integration.IsConnectedToMeteredConnection() && !await Dialogs.ShowYesNoDialogAsync(Context, Resource.String.warning, Resource.String.large_attachment))
                    {
                        dismissAction();
                        return;
                    }

                    path = await Managers.DocumentsManager.GetAttachmentAsync(attachmentDescription, Document, false, SourceType.Remote);
                }

                if (string.IsNullOrWhiteSpace(path))
                    throw new Exception("Unable to get attachment path.");

                var uri = FileProvider.GetUriForFile(Context, Context.PackageName + ".fileprovider", new Java.IO.File(path));
                var mimeType = MimeTypeMap.GetMimeType(Path.GetExtension(path));

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
                    await Managers.NotificationsManager.MarkAsRead(NotificationGuid);

                if (FailedDocumentToUploadGuid != Guid.Empty)
                {
                    (DocumentPreview, Document) = await Managers.DocumentsManager.GetFailedDocumentToUpload(FailedDocumentToUploadGuid);
                    DocumentId = Document.Id;
                }

                if (DocumentId.HasValue && DocumentPreview == null && Document == null)
                {
                    var container = await Managers.DocumentsManager.GetDocumentWithPreviewAsync(FolderId ?? Folder?.Id, DocumentId.Value);
                    DocumentPreview = container.DocumentPreview;
                    Document = container.Document;
                }

                if (DocumentPreview != null && Document == null)
                    Document = await Managers.DocumentsManager.GetDocumentAsync(FolderId ?? Folder?.Id, DocumentPreview.Id);

                RefreshView();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Downloading document failed [folder.name={Folder?.Name}, folder.id={FolderId ?? Folder?.Id}, documentId={DocumentId ?? DocumentPreview?.Id}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);

                Activity?.OnBackPressed();
            }
        }

        static class MenuItemActions
        {
            public const int GoToPrevious = 5;
            public const int GoToNext = 6;
            public const int MarkAsRead = 10;
            public const int MarkAsUnread = 11;
            public const int CopyToNew = 20;
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

        static class RequestCodes
        {
            public static int CommentsRequest = 1;
            public static int CategoriesRequest = 2;
        }

        class DocumentFragmentState : IRetainableState
        {
            public int? FolderId { get; set; }

            public Folder Folder { get; set; }

            public int? DocumentId { get; set; }

            public DocumentPreview DocumentPreview { get; set; }
        }
    }
}