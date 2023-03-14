using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Mark5.Mobile.Classes.Enum;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.HubMessages;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Model;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.Common;
using Mark5.Mobile.Droid.Ui.Views.DocumentViews;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class DocumentFragment : BaseFragment
    {
        private const string FolderIdBundleKey = "FolderId_7215e56b-5ec3-436d-b4e3-900449cb1ad0";
        private const string FolderBundleKey = "Folder_592db8d9-d212-4476-ac83-fd4bb11cc8d9";
        private const string DocumentPreviewBundleKey = "DocumentPreview_83a5daa4-7ede-453b-9b19-07362f644ad1";
        private const string DocumentIdBundleKey = "DocumentId_e9409d8a-9212-4483-b819-ff5ac3487c87";
        private const string CloseRequestBundleKey = "CloseRequest_d45a15b4-dadb-40ae-aab1-c565e9446bd0";
        private const string NotificationGuidBundleKey = "NotificationGuid_fb411b05-9d4b-46db-aa81-7348bf069a38";
        private const string FailedDocumentToUploadGuidBundleKey = "FailedDocumentToUploadGuid_7bf6c332-083d-4f2b-950e-d3bdc3992e2b";

        private const int LargeAttachmentSizeInBytes = 20 * 1024 * 1024; // 20MB

        private Guid _failedDocumentToUploadGuid;
        private int? _folderId;
        private Folder _folder;
        private int? _documentId;
        private DocumentPreview _documentPreview;
        private Document _document;
        private Guid _notificationGuid; 

        private ProgressBar _progress;
        private RelativeLayout _relativeLayout;
        private LinearLayoutCompat _linearLayout;
        private AppCompatImageView _button1;
        private AppCompatImageView _button2;
        private AppCompatImageView _button3;
        private ContentView contentView;

        private CancellationTokenSource _setReadStatusCancellationTokenSource;

        private Action _dismissAction;

        public static (DocumentFragment fragment, string tag) NewInstance(Folder folder = null, int? folderId = null,
            DocumentPreview dp = null, int? docId = null, Guid? notificationGuid = null, Guid? failDocToUploadGuid = null)
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

            var tag = $"{nameof(DocumentFragment)} [DocumentId={dp?.Id ?? docId}]";

            return (fragment, tag);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Arguments == null)
                return;
            
            if (Arguments.ContainsKey(FolderBundleKey))
                _folder = Serializer.Deserialize<Folder>(Arguments.GetString(FolderBundleKey));

            if (Arguments.ContainsKey(FolderIdBundleKey))
                _folderId = Arguments.GetInt(FolderIdBundleKey);

            if (Arguments.ContainsKey(DocumentPreviewBundleKey))
                _documentPreview = Serializer.Deserialize<DocumentPreview>(Arguments.GetString(DocumentPreviewBundleKey));

            if (Arguments.ContainsKey(DocumentIdBundleKey))
                _documentId = Arguments.GetInt(DocumentIdBundleKey);

            if (Arguments.ContainsKey(NotificationGuidBundleKey))
                _notificationGuid = Serializer.Deserialize<Guid>(Arguments.GetString(NotificationGuidBundleKey));

            if (Arguments.ContainsKey(FailedDocumentToUploadGuidBundleKey))
                _failedDocumentToUploadGuid = Serializer.Deserialize<Guid>(Arguments.GetString(FailedDocumentToUploadGuidBundleKey));
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info(
                $"Creating {nameof(DocumentFragment)} [folder.id={_folderId ?? _folder?.Id}, document.id={_documentId ?? _documentPreview?.Id ?? _document?.Id}]...");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_with_buttons_and_progress, container, false);

            var scrollView = rootView?.FindViewById<ScrollView>(Resource.Id.scroll_view);
            if (scrollView == null)
                return null;

            scrollView.Focusable = true;
            scrollView.FocusableInTouchMode = true;
            scrollView.DescendantFocusability = DescendantFocusability.BeforeDescendants;

            _progress = rootView.FindViewById<ProgressBar>(Resource.Id.progress);
            _relativeLayout = rootView.FindViewById<RelativeLayout>(Resource.Id.relative_layout);
            _linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);
            _button1 = rootView.FindViewById<AppCompatImageView>(Resource.Id.button1);
            _button2 = rootView.FindViewById<AppCompatImageView>(Resource.Id.button2);
            _button3 = rootView.FindViewById<AppCompatImageView>(Resource.Id.button3);

            if (_button1 != null)
            {
                _button1.SetImageResource(Resource.Drawable.reply);
                if (Context != null)
                {
                    _button1.SetColorFilter(
                        new Android.Graphics.Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));
                    _button1.Enabled = false;
                    _button1.Clickable = true;
                    _button1.Click += (sender, e) =>
                    {
                        if (_documentPreview == null || _document == null)
                            return;

                        if (!ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines.Any())
                        {
                            Dialogs.ShowConfirmDialog(Activity, Resource.String.no_lines_error_title,
                                Resource.String.no_lines_error_content);
                            return;
                        }

                        StartActivity(ComposeDocumentActivity.CreateIntent(Context,
                            DocumentCreationModeFlag.Reply,
                            CopyToNewOption.None,
                            previousDocumentDirection: _documentPreview.Direction,
                            previousDocumentFolderId: _folder?.Id ?? _folderId,
                            previousDocumentId: _documentPreview.Id));
                    };
                    _button1.LongClickable = true;
                    _button1.LongClick += (sender, e) =>
                        Toast.MakeText(Context, Resource.String.reply, ToastLength.Short)?.Show();
                }
            }

            if (_button2 != null)
            {
                _button2.SetImageResource(Resource.Drawable.replyall);
                if (Context != null)
                {
                    _button2.SetColorFilter(
                        new Android.Graphics.Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));
                    _button2.Enabled = false;
                    _button2.Clickable = true;
                    _button2.Click += (sender, e) =>
                    {
                        if (_documentPreview == null || _document == null)
                            return;

                        if (!ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines.Any())
                        {
                            Dialogs.ShowConfirmDialog(Activity, Resource.String.no_lines_error_title,
                                Resource.String.no_lines_error_content);
                            return;
                        }

                        StartActivity(ComposeDocumentActivity.CreateIntent(Context,
                            DocumentCreationModeFlag.ReplyAll,
                            CopyToNewOption.None,
                            previousDocumentDirection: _documentPreview.Direction,
                            previousDocumentFolderId: _folder?.Id ?? _folderId,
                            previousDocumentId: _documentPreview.Id));
                    };
                    _button2.LongClickable = true;
                    _button2.LongClick += (sender, e) =>
                        Toast.MakeText(Context, Resource.String.reply_all, ToastLength.Short)?.Show();
                }
            }

            if (_button3 != null)
            {
                _button3.SetImageResource(Resource.Drawable.forward);
                if (Context != null)
                {
                    _button3.SetColorFilter(
                        new Android.Graphics.Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));
                    _button3.Enabled = false;
                    _button3.Clickable = true;
                    _button3.Click += (sender, e) =>
                    {
                        if (_documentPreview == null || _document == null)
                            return;

                        if (!ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines.Any())
                        {
                            Dialogs.ShowConfirmDialog(Activity, Resource.String.no_lines_error_title,
                                Resource.String.no_lines_error_content);
                            return;
                        }

                        StartActivity(ComposeDocumentActivity.CreateIntent(Context,
                            DocumentCreationModeFlag.Forward,
                            CopyToNewOption.None,
                            previousDocumentDirection: _documentPreview.Direction,
                            previousDocumentFolderId: _folder?.Id ?? _folderId,
                            previousDocumentId: _documentPreview.Id));
                    };
                    _button3.LongClickable = true;
                    _button3.LongClick += (sender, e) =>
                        Toast.MakeText(Context, Resource.String.forward, ToastLength.Short)?.Show();
                }
            }

            if (_linearLayout != null)
            {
                _linearLayout.AddView(new SubjectView(Context));
                _linearLayout.AddView(new Divider(Context));
                _linearLayout.AddView(new RecipentsView(Context) { RecipientClickHandler = FromValue_Click });
                _linearLayout.AddView(new Divider(Context));
                _linearLayout.AddView(new PriorityView(Context));
                _linearLayout.AddView(new Divider(Context));
                var av = new AttachmentsView(Context);
                av.AttachmentClicked += AttachmentsView_AttachmentClicked;
                av.AttachmentLongClicked += AttachmentsView_AttachmentLongClicked;
                _linearLayout.AddView(av);

                var civ = new CalendarInvitationView(Context);
                civ.ReplySelected += CalendarInvitationView_ReplySelected;
                _linearLayout.AddView(civ);

                contentView = new ContentView(Context);
                contentView.MailToLinkClicked += ContentView_MailToLinkClicked;
                _linearLayout.AddView(contentView);
            }

            HasOptionsMenu = true;

            return rootView;
        }

        private void ContentView_MailToLinkClicked(object sender, string url)
        {
            var preconfiguredEmailAddresses = new Dictionary<DocumentAddressType, string[]>();
            var parts = url.Split("?");

            preconfiguredEmailAddresses.Add(DocumentAddressType.To, parts[0].Split(","));

            if (parts.Length > 1)
            {
                var parsed = HttpUtility.ParseQueryString(parts[1]);
                var subject = parsed["subject"];
                var body = parsed["body"];
                var cc = parsed["cc"]?.Split(",");
                var bcc = parsed["bcc"]?.Split(",");

                if (cc != null && cc.Any())
                    preconfiguredEmailAddresses.Add(DocumentAddressType.Cc, cc);

                if (bcc != null && bcc.Any())
                    preconfiguredEmailAddresses.Add(DocumentAddressType.Bcc, bcc);

                StartActivity(ComposeDocumentActivity.CreateIntent(Context, DocumentCreationModeFlag.New, 
                    CopyToNewOption.None, preconfiguredContent: body, preconfiguredSubject: subject,
                    preconfiguredEmailAddresses: preconfiguredEmailAddresses));
            }
            else
            {
                StartActivity(ComposeDocumentActivity.CreateIntent(Context, DocumentCreationModeFlag.New, 
                    CopyToNewOption.None, preconfiguredEmailAddresses: preconfiguredEmailAddresses));
            }
        }
 
        private async void FromValue_Click(object sender, RecipientClickEventArgs e)
        {
            if (Context == null) 
                return;
            
            var result = await Dialogs.ShowListDialog(Context, 
                Resource.String.select_action, new [] { 
                    Context.GetString(Resource.String.new_document),
                    Context.GetString(Resource.String.new_contact)
                }, 
                true);

            if (result < 0) 
                return;
            
            switch (result)
            {
                case 0:
                    await PresentComposeViewWithPreconfiguredFields(
                        preconfiguredToAddresses: new[] { e.Recipient });
                    break;
                case 1:
                    await PresentContactViewWithPreconfiguredFields(e.Recipient);
                    break;
            }
        }

        private static DocumentAddress GetEmail(string email) => 
            Validator.ContainsValidEmails(email, out var addresses) 
                ? addresses.First() 
                : new DocumentAddress();

        private async Task PresentComposeViewWithPreconfiguredFields(string subject = null, string body = null,
            string[] preconfiguredToAddresses = null, string[] preconfiguredCcAddresses = null, 
            string[] preconfiguredBccAddresses = null)
        {
            var preconfiguredAddresses = new Dictionary<DocumentAddressType, string[]>
            {
                {
                    DocumentAddressType.To, preconfiguredToAddresses
                }
            };

            if (preconfiguredCcAddresses != null)
                preconfiguredAddresses.Add(DocumentAddressType.Cc, preconfiguredCcAddresses);

            if (preconfiguredBccAddresses != null)
                preconfiguredAddresses.Add(DocumentAddressType.Bcc, preconfiguredBccAddresses);

            StartActivity(ComposeDocumentActivity.CreateIntent(Context,
                DocumentCreationModeFlag.New, CopyToNewOption.None, preconfiguredContent: body, 
                preconfiguredSubject: subject, preconfiguredEmailAddresses: preconfiguredAddresses));
        }

        private async Task PresentContactViewWithPreconfiguredFields(string preconfiguredEmailAddress)
        {
            if (Context == null)
                return;
            
            var choice = await Dialogs.ShowListDialog(Context, 
                Resource.String.select_action, new[] 
                {
                    Context.GetString(Resource.String.add_company),
                    Context.GetString(Resource.String.add_department),
                    Context.GetString(Resource.String.add_person)
                }, true);

            if (choice < 0)
                return;

            var type = ContactType.None;
            switch (choice)
            {
                case 0:
                    type = ContactType.Company;
                    break;
                case 1:
                    type = ContactType.Department;
                    break;
                case 2:
                    type = ContactType.Person;
                    break;
            }

            StartActivity(AddEditContactActivity.CreateIntent(Context, 
                contactCreationModeFlag: (int)ContactCreationModeFlag.New,
                contactType: (int)type, preconfiguredAddress: GetEmail(preconfiguredEmailAddress)));
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            if (Activity is AppCompatActivity activity 
                && activity.SupportActionBar != null)
            {
                activity.SupportActionBar.Title = string.Empty;
                activity.SupportActionBar.Subtitle = null;
            }

            CommonConfig.Logger.Info(
                $"Created {nameof(DocumentFragment)} [folder.id={_folderId ?? _folder?.Id}, document.id={_documentId ?? _documentPreview?.Id ?? _document?.Id}]");
        }

        public override void OnDestroyView()
        {
            _dismissAction?.Invoke();
            base.OnDestroyView();
        }

        public override async void OnResume()
        {
            base.OnResume();

            await RefreshData();

            if (!IsAdded || IsDetached || IsRemoving
                || (Activity is SwipeDocumentActivity && !UserVisibleHint))
            {
                return;
            }

            await MarkAsReadIfNecessary();

            if (PlatformConfig.Preferences.SyncUserActivities)
            {
                await Managers.DocumentsManager.ExecuteUserActivity(UserActivityType.Open, 
                    _documentPreview, null);
            }
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
                _setReadStatusCancellationTokenSource?.Cancel();
                _setReadStatusCancellationTokenSource = null;
            }
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            if (resultCode != (int)Result.Ok) 
                return;
            
            if (requestCode == RequestCodes.CommentsRequest)
            {
                var comments = Serializer.Deserialize<List<Comment>>(
                    data.GetStringExtra(CommentsListActivity.CommentsResultKey));
                UpdateComments(comments);
            }
            else if (requestCode == RequestCodes.CategoriesRequest)
            {
                var categories = Serializer.Deserialize<List<Category>>(
                    data.GetStringExtra(CategoriesListActivity.CategoriesResultKey));
                UpdateCategories(categories);
            }
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            if (_documentPreview == null)
                return;

            if (_failedDocumentToUploadGuid != Guid.Empty)
                return;

            if (Activity is SwitchDocumentActivity && _folder != null)
            {
                var goToPreviousItem = menu.Add(Menu.None, 
                    MenuItemActions.GoToPrevious, MenuItemActions.GoToPrevious, 
                    Resource.String.document_previous);
                goToPreviousItem?.SetShowAsAction(ShowAsAction.Always);

                var goToNextItem = menu.Add(Menu.None, 
                    MenuItemActions.GoToNext, MenuItemActions.GoToNext, 
                    Resource.String.document_next);
                goToNextItem?.SetShowAsAction(ShowAsAction.Always);
            }

            if (!_documentPreview.IsReadByCurrent)
            {
                menu.Add(Menu.None, MenuItemActions.MarkAsRead, 
                    MenuItemActions.MarkAsRead, Resource.String.mark_as_read);
            }

            if (_documentPreview.IsReadByCurrent)
            {
                menu.Add(Menu.None, MenuItemActions.MarkAsUnread, 
                    MenuItemActions.MarkAsUnread, Resource.String.marks_as_unread);
            }

            if (ServerConfig.SystemSettings.DocumentsModuleInfo.WorktrayEnabled ?? true)
            {
                menu.Add(Menu.None, MenuItemActions.CopyToWorktray, 
                    MenuItemActions.CopyToWorktray, Resource.String.copy_to_worktray);
            }

            menu.Add(Menu.None, MenuItemActions.CopyToFolder, 
                MenuItemActions.CopyToFolder, Resource.String.copy_to_folder);

            if (PlatformConfig.Preferences.EnableMoveToFolder &&
                (_folder?.InternalType == FolderInternalType.FilterView
                || _folder?.InternalType == FolderInternalType.Static
                || _folder?.InternalType == FolderInternalType.Worktray))
            {
                menu.Add(Menu.None, MenuItemActions.MoveToFolder, 
                    MenuItemActions.MoveToFolder, Resource.String.move_to_folder);
            }

            menu.Add(Menu.None, MenuItemActions.SetPriority, 
                MenuItemActions.SetPriority, Resource.String.set_priority);
            menu.Add(Menu.None, MenuItemActions.Categories, 
                MenuItemActions.Categories, Resource.String.categories);

            if (_document != null)
            {
                menu.Add(Menu.None, MenuItemActions.Comments, 
                    MenuItemActions.Comments, Resource.String.comments);
            }

            menu.Add(Menu.None, MenuItemActions.Actions, 
                MenuItemActions.Actions, Resource.String.history);
            menu.Add(Menu.None, MenuItemActions.Links, 
                MenuItemActions.Links, Resource.String.overview);

            if (_folder?.InternalType == FolderInternalType.FilterView ||
                _folder?.InternalType == FolderInternalType.Static ||
                _folder?.InternalType == FolderInternalType.Worktray)
            {
                menu.Add(Menu.None, MenuItemActions.DeleteFromFolder, 
                    MenuItemActions.DeleteFromFolder, Resource.String.delete_from_folder);
            }

            var documents = new List<DocumentPreview>
            {
                _documentPreview
            };
            if (DocumentsDeleteChecker.CanDeleteDocuments(documents))
            {
                menu.Add(Menu.None, MenuItemActions.Delete, 
                    MenuItemActions.Delete, Resource.String.delete);
            }

            menu.Add(Menu.None, MenuItemActions.CopyToNew, 
                MenuItemActions.CopyToNew, Resource.String.copy_to_new);

            if (ServerConfig.SystemSettings.SystemInfo.DeliveryReportAvailable)
            {
                menu.Add(Menu.None, MenuItemActions.DeliveryReport, 
                    MenuItemActions.DeliveryReport, Resource.String.delivery_report);
            }

            menu.Add(Menu.None, MenuItemActions.Print,
                MenuItemActions.Print, Resource.String.print);
        }

        public override async void OnPrepareOptionsMenu(IMenu menu)
        {
            var isDocumentReady = _document != null;

            var menuItemIds = new List<int>
            {
                MenuItemActions.Comments
            };
            foreach (var menuItem in menuItemIds.Select(menu.FindItem))
            {
                menuItem?.SetEnabled(isDocumentReady);
            }

            if (!(Activity is SwitchDocumentActivity) || _folder == null) 
                return;
            
            var goToPreviousItem = menu.FindItem(MenuItemActions.GoToPrevious);
            goToPreviousItem?.SetEnabled(
                await ((SwitchDocumentActivity)Activity).HasPrevious(_documentId ?? _documentPreview.Id));

            var goToNextItem = menu.FindItem(MenuItemActions.GoToNext);
            goToNextItem?.SetEnabled(
                await ((SwitchDocumentActivity)Activity).HasNext(_documentId ?? _documentPreview.Id));
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (Activity is SwitchDocumentActivity && item.ItemId == MenuItemActions.GoToPrevious)
                ((SwitchDocumentActivity)Activity).GoToPrevious(_documentId ?? _documentPreview.Id);

            if (Activity is SwitchDocumentActivity && item.ItemId == MenuItemActions.GoToNext)
                ((SwitchDocumentActivity)Activity).GoToNext(_documentId ?? _documentPreview.Id);

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
                StartActivity(CopyMoveToFolderListActivity
                    .CreateIntent(Context,
                        CopyMoveToFolderListActivity.ModeType.Copy,
                        ModuleType.Documents,
                        new List<IBusinessEntity>
                        {
                            _documentPreview
                        }));
                
                return true;
            }

            if (item.ItemId == MenuItemActions.MoveToFolder)
            {
                StartActivity(CopyMoveToFolderListActivity
                    .CreateIntent(Context,
                        CopyMoveToFolderListActivity.ModeType.Move,
                        ModuleType.Documents,
                        new List<IBusinessEntity>
                        {
                            _documentPreview
                        },
                        _folder));
                
                return true;
            }

            if (item.ItemId == MenuItemActions.SetPriority)
            {
                SetPriority();
                return true;
            }

            if (item.ItemId == MenuItemActions.Categories)
            {
                StartActivityForResult(CategoriesListActivity.CreateIntent(Context, _documentPreview), 
                    RequestCodes.CategoriesRequest);
                return true;
            }

            if (item.ItemId == MenuItemActions.Comments)
            {
                StartActivityForResult(CommentsListActivity.CreateIntent(Context, _document), 
                    RequestCodes.CommentsRequest);
                return true;
            }

            if (item.ItemId == MenuItemActions.Actions)
            {
                StartActivity(ObjectActionsActivity.CreateIntent(Context, _documentPreview));
                return true;
            }

            if (item.ItemId == MenuItemActions.Links)
            {
                StartActivity(ObjectLinksActivity.CreateIntent(Context, _documentPreview));
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

            if (item.ItemId == MenuItemActions.DeliveryReport)
            {
                StartActivity(TransmitDestinationsListActivity.CreateIntent(Context, _documentPreview.Id, 
                    _documentPreview.ReferenceNumber));
                return true;
            }

            if (item.ItemId == MenuItemActions.Print)
            {
                Print();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void Print()
        {
            contentView.Print();
        }

        private async Task RefreshView()
        {
            var activateButtons = _failedDocumentToUploadGuid == Guid.Empty;
            if (activateButtons)
            {
                _button1.Enabled = true;
                _button2.Enabled = true;
                _button3.Enabled = true;
                _button1.Alpha = 1f;
                _button2.Alpha = 1f;
                _button3.Alpha = 1f;
            }
            else
            {
                _button1.Enabled = false;
                _button2.Enabled = false;
                _button3.Enabled = false;
                _button1.Alpha = .5f;
                _button2.Alpha = .5f;
                _button3.Alpha = .5f;
            }

            _progress.Visibility = ViewStates.Gone;
            _relativeLayout.Visibility = ViewStates.Visible;

            for (var i = 0; i < _linearLayout.ChildCount; i++)
            {
                if (!(_linearLayout.GetChildAt(i) is DocumentView dv)) 
                    continue;
                
                dv.DocumentPreview = _documentPreview;
                dv.Document = _document;
                await dv.RefreshView();

                if (!(_linearLayout.GetChildAt(i + 1) is Divider d)) 
                    continue;
                
                d.Visibility = dv.Visibility;
                i++;
            }

            _linearLayout.Invalidate();
            _linearLayout.RequestLayout();

            Activity?.InvalidateOptionsMenu();
        }

        private async Task RefreshView<T>() where T : DocumentView
        {
            _progress.Visibility = ViewStates.Gone;
            _relativeLayout.Visibility = ViewStates.Visible;

            for (var i = 0; i < _linearLayout.ChildCount; i++)
            {
                if (!(_linearLayout.GetChildAt(i) is T dv)) 
                    continue;
                
                dv.DocumentPreview = _documentPreview;
                dv.Document = _document;
                await dv.RefreshView();

                if (!(_linearLayout.GetChildAt(i + 1) is Divider d)) 
                    continue;
                
                d.Visibility = dv.Visibility;
                i++;
            }

            _linearLayout.Invalidate();
            _linearLayout.RequestLayout();

            Activity?.InvalidateOptionsMenu();
        }

        private async Task MarkAsReadIfNecessary()
        {
            _setReadStatusCancellationTokenSource?.Cancel();
            _setReadStatusCancellationTokenSource = new CancellationTokenSource();

            var d = _document;
            var dp = _documentPreview;
            var token = _setReadStatusCancellationTokenSource.Token;

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

                await Managers.DocumentsManager.SetDocumentReadStatusAsync(dp, d, true);

                if (PlatformConfig.Preferences.SyncUserActivities)
                {
                    await Managers.DocumentsManager.ExecuteUserActivity(UserActivityType.Read, 
                        dp, null);
                }

                if (token.IsCancellationRequested)
                    return;

                if (!IsAdded || IsDetached || IsRemoving)
                    return;

                await RefreshView<RecipentsView>();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error(
                    $"Marking document as read failed [documentPreviewId={dp?.Id}]", ex);
            }
        }

        private void UpdateCategories(List<Category> categories)
        {
            _documentPreview?.Categories.Clear();
            _documentPreview?.Categories.AddRange(categories);
        }

        private void UpdateComments(List<Comment> comments)
        {
            if (_document != null)
            {
                _document.Comments.Clear();
                _document.Comments.AddRange(comments);
            }

            if (_documentPreview != null)
                _documentPreview.CommentsCount = comments.Count;
        }

        private async void MarkAsRead()
        {
            CommonConfig.Logger.Info(
                $"Attempting to mark as read [documentPreview={_documentPreview}]...");

            _dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, 
                Resource.String.marking_as_read, Resource.String.please_wait);

            try
            {
                CommonConfig.UsageAnalytics.LogEvent(new SetReadStatusEvent(1));

                await Managers.DocumentsManager.SetDocumentReadStatusAsync(
                    _documentPreview, _document, true);

                await RefreshView<RecipentsView>();

                _dismissAction();
            }
            catch (Exception ex)
            {
                _dismissAction();

                CommonConfig.Logger.Error($"Marking as read failed [documentPreview={_documentPreview}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        private async void MarkAsUnread()
        {
            CommonConfig.Logger.Info(
                $"Attempting to mark as unread [documentPreview={_documentPreview}]...");

            _dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, 
                Resource.String.marking_as_unread, Resource.String.please_wait);

            try
            {
                CommonConfig.UsageAnalytics.LogEvent(new SetReadStatusEvent(1));

                await Managers.DocumentsManager.SetDocumentReadStatusAsync(_documentPreview, _document, false);

                await RefreshView<RecipentsView>();

                _dismissAction();
            }
            catch (Exception ex)
            {
                _dismissAction();

                CommonConfig.Logger.Error(
                    $"Marking as unread failed [documentPreview={_documentPreview}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        private async void CopyToWorktrayAction()
        {
            var option = await Dialogs.ShowListDialog(Context, Resource.String.copy_to_worktray, Resource.Array.copy_to_worktray_options, true);

            if (option == 0)
            {
                CommonConfig.Logger.Info(
                    $"Attempting copy to worktray [documentPreview={_documentPreview}]...");

                _dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, 
                    Resource.String.copying_to_worktray, Resource.String.please_wait);

                try
                {
                    await Managers.CommonActionsManager.CopyToWorktray(new List<IBusinessEntity>
                    {
                        _documentPreview
                    });

                    _dismissAction();
                }
                catch (Exception ex)
                {
                    _dismissAction();

                    CommonConfig.Logger.Error(
                        $"Copying to worktray failed [documentPreview={_documentPreview}]", ex);

                    await Dialogs.ShowErrorDialogAsync(Activity, ex);
                }
            }

            if (option == 1)
            {
                StartActivity(CopyToUserWorktrayActivity.CreateIntent(Context, 
                    new List<IBusinessEntity> { _documentPreview }));
            }
        }

        private async void SetPriority()
        {
            var possiblePriorities = new List<Priority> { Priority.Urgent, Priority.Normal, Priority.Low };
            var documentPriority = _documentPreview.Priority;

            if (!possiblePriorities.Contains(documentPriority))
                documentPriority = Priority.Normal;

            var priority = await Dialogs.ShowSingleSelectDialogAsync(Context, 
                Resource.String.set_priority, possiblePriorities, documentPriority);
            if (priority == default || priority == _documentPreview.Priority)
                return;

            CommonConfig.Logger.Info($"Attempting to set priority [documentPreview={_documentPreview}]...");

            _dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, 
                Resource.String.setting_priority, Resource.String.please_wait);

            try
            {
                await Managers.DocumentsManager.SetDocumentsPriorityAsync(
                    new List<DocumentPreview>
                    {
                        _documentPreview
                    },
                    priority);

                await RefreshView<PriorityView>();
                _dismissAction();
            }
            catch (Exception ex)
            {
                _dismissAction();

                CommonConfig.Logger.Error(
                    $"Setting priority failed [documentPreview={_documentPreview}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        private async void DeleteFromFolderAction()
        {
            var yesNo = await Dialogs.ShowYesNoDialogAsync(Context, Resource.String.delete_from_folder, Resource.String.delete_from_folder_are_you_sure);
            if (!yesNo)
                return;

            CommonConfig.Logger.Info($"Attempting to delete from folder [documentPreview={_documentPreview}]...");

            _dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.deleting_from_folder, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.RemoveFromFolder(
                    new List<IBusinessEntity>
                    {
                        _documentPreview
                    },
                    _folder);

                _dismissAction();
            }
            catch (Exception ex)
            {
                _dismissAction();

                CommonConfig.Logger.Error(
                    $"Deleting from folder failed [documentPreview={_documentPreview}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        private async void DeleteAction()
        {
            var yesNo = await Dialogs.ShowYesNoDialogAsync(Context, 
                Resource.String.delete, Resource.String.delete_are_you_sure);
            
            if (!yesNo)
                return;

            CommonConfig.Logger.Info($"Attempting to delete [documentPreview={_documentPreview}]...");

            _dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, 
                Resource.String.deleting, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.Delete(new List<IBusinessEntity>
                {
                    _documentPreview
                });

                _dismissAction();
                Activity?.OnBackPressed();
            }
            catch (Exception ex)
            {
                _dismissAction();

                CommonConfig.Logger.Error($"Deleting failed [documentPreview={_documentPreview}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        private async void CopyToNew()
        {
            if (_document == null || _documentPreview == null)
                return;

            var data = new List<CopyToNewOption>
            {
                CopyToNewOption.Addresses, CopyToNewOption.Content
            };

            if (_document.Attachments.Any())
                data.Add(CopyToNewOption.Attachments);

            string DisplayText(CopyToNewOption option)
            {
                switch (option)
                {
                    case CopyToNewOption.Addresses:
                        return GetString(Resource.String.copy_to_new_addresses);
                    case CopyToNewOption.Content:
                        return GetString(Resource.String.copy_to_new_content);
                    case CopyToNewOption.Attachments:
                        return GetString(Resource.String.copy_to_new_attachments);
                    default:
                        return string.Empty;
                }
            }

            var selections = 
                await Dialogs.ShowMultiSelectDialogAsync(Context, 
                    Resource.String.copy_to_new, data, data, displayText: DisplayText);
            
            if (selections == null || selections.Count < 1)
                return;

            var copyToNewOption = CopyToNewOption.None;
            for (var i = 0; i < selections.Count; i++)
                copyToNewOption |= selections[i];

            StartActivity(ComposeDocumentActivity.CreateIntent(Context,
                DocumentCreationModeFlag.New,
                copyToNewOption,
                previousDocumentDirection: _documentPreview.Direction,
                previousDocumentFolderId: _folder?.Id ?? _folderId,
                previousDocumentId: _documentPreview.Id));
        }

        private async void AttachmentsView_AttachmentClicked(object sender, 
            AttachmentDescription attachmentDescription)
        {
            CommonConfig.UsageAnalytics.LogEvent(new DocumentOpenAttachmentEvent());

            _dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, 
                Resource.String.opening_attachment, Resource.String.please_wait);

            try
            {
                var path = await Managers.DocumentsManager
                    .GetAttachmentAsync(attachmentDescription, _document, 
                        false, SourceType.Local);

                if (string.IsNullOrWhiteSpace(path))
                {
                    if (attachmentDescription.SizeInBytes > LargeAttachmentSizeInBytes 
                        && PlatformConfig.Preferences.LargeAttachmentWarning 
                        && Integration.IsConnectedToMeteredConnection() 
                        && !await Dialogs.ShowYesNoDialogAsync(Context, 
                            Resource.String.warning, 
                            Resource.String.large_attachment))
                    {
                        _dismissAction();
                        return;
                    }

                    path = await Managers.DocumentsManager
                        .GetAttachmentAsync(attachmentDescription, _document, 
                            false, SourceType.Remote);
                }

                if (string.IsNullOrWhiteSpace(path))
                    throw new Exception("Unable to open attachment");

                if (Context == null) 
                    return;
                
                var uri = FileProvider.GetUriForFile(Context, Context.PackageName + ".fileprovider", new Java.IO.File(path));
                var mimeType = MimeTypeMap.GetMimeType(Path.GetExtension(path));

                var openFileIntent = new Intent(Intent.ActionView);
                openFileIntent.SetDataAndType(uri, mimeType);
                openFileIntent.AddFlags(ActivityFlags.NewTask);
                openFileIntent.AddFlags(ActivityFlags.GrantReadUriPermission);

                var canOpen = Context.PackageManager != null 
                              && Context.PackageManager.QueryIntentActivities(openFileIntent, 0).Any();
                if (canOpen)
                    Context.StartActivity(openFileIntent);
                else
                    await Dialogs.ShowConfirmDialogAsync(Context, Resource.String.attachment_cannot_be_opened_title, Resource.String.attachment_cannot_be_opened_summary);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error(
                    $"Failed to view attachment [document.Id={_document.Id}, attachment.Id={attachmentDescription?.Id}, attachment.Name={attachmentDescription?.Name}", ex);

                _dismissAction();
                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
            finally
            {
                _dismissAction();
            }
        }

        private async void AttachmentsView_AttachmentLongClicked(object sender, AttachmentDescription attachmentDescription)
        {
            _dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, 
                Resource.String.opening_attachment, Resource.String.please_wait);

            try
            {
                var path = await Managers.DocumentsManager
                    .GetAttachmentAsync(attachmentDescription, _document, 
                        false, SourceType.Local);

                if (string.IsNullOrWhiteSpace(path))
                {
                    if (attachmentDescription.SizeInBytes > LargeAttachmentSizeInBytes 
                        && PlatformConfig.Preferences.LargeAttachmentWarning 
                        && Integration.IsConnectedToMeteredConnection() 
                        && !await Dialogs.ShowYesNoDialogAsync(Context, Resource.String.warning, 
                            Resource.String.large_attachment))
                    {
                        _dismissAction();
                        return;
                    }

                    path = await Managers.DocumentsManager
                        .GetAttachmentAsync(attachmentDescription, _document, false, SourceType.Remote);
                }

                if (string.IsNullOrWhiteSpace(path))
                    throw new Exception("Unable to get attachment path.");

                if (Context == null) 
                    return;
                
                var uri = FileProvider.GetUriForFile(Context, Context.PackageName + ".fileprovider", new Java.IO.File(path));
                var mimeType = MimeTypeMap.GetMimeType(Path.GetExtension(path));

                if (Activity != null)
                    ShareCompat.IntentBuilder.From(Activity).SetType(mimeType).SetStream(uri).StartChooser();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error(
                    $"Failed to share attachment [document.Id={_document.Id}, attachment.Id={attachmentDescription?.Id}, attachment.Name={attachmentDescription?.Name}", ex);

                _dismissAction();
                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
            finally
            {
                _dismissAction();
            }
        }

        private async void CalendarInvitationView_ReplySelected(object sender, 
            InvitationReplyDetailViewModel vm)
        {
            var civ = sender as CalendarInvitationView;
            var invitation = _document?.Invitations?.FirstOrDefault();

            if (invitation == null)
                return;

            await SendInvitationReply(civ, invitation, vm);
        }

        private async Task SendInvitationReply(CalendarInvitationView cv, 
            CalendarInvitation invitation, InvitationReplyDetailViewModel vm)
        {
            _dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, 
                Resource.String.sending_appointment_response, 
                Resource.String.please_wait);

            try
            {
                CommonConfig.Logger.Info(
                    $"Attempting to reply to calendar invitation for document [documentId={_document.Id}]");

                var responseDocument = new Document();
                var responseDocumentPreview = new DocumentPreview();

                //Line
                responseDocument.Lines = new List<Line> { vm.Line };

                //Body
                var previousDocumentContent = string.Empty;

                if (!string.IsNullOrWhiteSpace(_document?.HtmlBody))
                {
                    var config = HtmlProcessingConfiguration.DefaultForEditing;
                    config.InjectReplyHeader = true;
                    config.ReplyHeaderParameters = 
                        HtmlUtilities.GetReplyHeaderParameters(Context, _documentPreview, _document);
                    previousDocumentContent = 
                        await HtmlUtilities.ProcessHtml(Context, _document.HtmlBody, config);
                }
                else if (!string.IsNullOrWhiteSpace(_document?.PlainTextBody))
                {
                    var config = PlainTextProcessingConfiguration.DefaultForEditing;
                    config.InjectReplyHeader = true;
                    config.ReplyHeaderParameters = 
                        HtmlUtilities.GetReplyHeaderParameters(Context, _documentPreview, _document);
                    previousDocumentContent = 
                        await HtmlUtilities.ProcessPlainText(Context, _document.PlainTextBody, config);
                }

                responseDocument.HtmlBody = 
                    await HtmlUtilities.MergeReplyWithPreviousDocument(Context, vm.Message, previousDocumentContent);

                //Subject
                var responseSubjectString = string.Empty;
                switch (vm.Status)
                {
                    case ParticipantStatus.Accepted:
                        responseSubjectString = "ACCEPTED: ";
                        break;
                    case ParticipantStatus.Declined:
                        responseSubjectString = "DECLINED: ";
                        break;
                    case ParticipantStatus.Tentative:
                        responseSubjectString = "TENTATIVE: ";
                        break;
                }

                responseDocumentPreview.Subject = responseSubjectString + _documentPreview.Subject;

                //Addresses
                _documentPreview.Addresses.Where(
                    x => x.AddressType == DocumentAddressType.From)
                    .ToList().ForEach(da =>
                {
                    var address = new DocumentAddress
                    {
                        Address = da.Address,
                        Name = da.Name,
                        Type = CommunicationAddressType.Email,
                        AddressType = DocumentAddressType.To
                    };
                    responseDocumentPreview.Addresses.Add(address);
                });

                responseDocumentPreview.Direction = DocumentDirection.Outgoing;

                if (_document != null)
                {
                    await Managers.DocumentsManager.ReplyToCalendarInvitationAsync(
                        responseDocument, responseDocumentPreview,
                        invitation, vm.Status, string.IsNullOrEmpty(vm.Message),
                        _document.Id, _folder?.Id ?? _folderId ?? 0);
                }

                invitation.Status = vm.Status;
                await cv.RefreshView();

                //notify to update calendar datasource
                if (invitation.Status == ParticipantStatus.Accepted 
                    || invitation.Status == ParticipantStatus.Tentative)
                {
                    CommonConfig.MessengerHub.Publish(new EntityAddedMessage(this, 
                        ObjectType.CalendarAppointment, invitation.AppointmentId));
                }

                _dismissAction();
            }
            catch (Exception ex)
            {
                _dismissAction();

                CommonConfig.Logger.Error($"Error while replying to calendar invitation for document [documentId={_document.Id}]", ex);
                await Dialogs.ShowErrorDialogAsync(Context, ex);
            }
        }

        private async Task RefreshData()
        {
            try
            {
                if (_notificationGuid != default)
                    await Managers.NotificationsManager.MarkAsRead(_notificationGuid);

                if (_failedDocumentToUploadGuid != Guid.Empty)
                {
                    (_documentPreview, _document) = await Managers.DocumentsManager
                        .GetFailedDocumentToUpload(_failedDocumentToUploadGuid);
                    _documentId = _document.Id;
                }

                if (_documentId.HasValue && _documentPreview == null && _document == null)
                {
                    var container = await Managers.DocumentsManager
                        .GetDocumentWithPreviewAsync(_folderId ?? _folder?.Id, _documentId.Value, 
                            Restored ? SourceType.Local : SourceType.Auto);
                    _documentPreview = container.DocumentPreview;
                    _document = container.Document;
                }

                if (_documentPreview != null && _document == null)
                {
                    _document = await Managers.DocumentsManager
                        .GetDocumentAsync(_folderId ?? _folder?.Id, _documentPreview.Id, 
                            Restored ? SourceType.Local : SourceType.Auto);
                }

                await RefreshView();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error(
                    $"Downloading document failed [folder.name={_folder?.Name}, folder.id={_folderId ?? _folder?.Id}, documentId={_documentId ?? _documentPreview?.Id}]", ex);

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
            public const int DeliveryReport = 102;
            public const int Print = 103;
        }

        static class RequestCodes
        {
            public static int CommentsRequest = 1;
            public static int CategoriesRequest = 2;
        }
    }
}