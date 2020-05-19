using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Support.Design.Widget;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.Droid.Model;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.Common;
using Mark5.Mobile.Droid.Ui.Views.ComposeDocumentViews;
using Mark5.Mobile.Droid.Utilities;
using PCLStorage;
using TinyMessenger;
using Uri = Android.Net.Uri;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class ComposeDocumentFragment : BaseFragment
    {
        readonly List<ComposeDocumentView> subViews = new List<ComposeDocumentView>(10);

        const string RestoreWorkingCopyBundleKey = "RestoreWorkingCopy_a6c252fc-09b9-44a9-941f-ea3785c0864d";
        const string DocumentCreationModeFlagBundleKey = "DocumentCreationModeFlag_b181c281-54bd-4c21-a476-a69ea0f83872";
        const string CopyToNewOptionBundleKey = "CopyToNewOption_e3d9e971-7873-497c-9239-838e14286d2e";
        const string PreviousDocumentDirectionBundleKey = "PreviousDocumentDirection_4a80d080-aaf2-497a-a1b2-b18d62e3d817";
        const string PreviousDocumentFolderIdBundleKey = "PreviousDocumentFolderId_def12a0b-0156-4189-9c67-e41ed7c944a5";
        const string PreviousDocumentIdBundleKey = "PreviousDocumentId_b8e521b8-8a8a-42ce-9d67-99b61abdafd2";
        const string PreconfiguredEmailAddressesBundleKey = "PreconfiguredEmailAdresses_d5a9b692-2f14-4865-bf25-d317f0f4abd2";
        const string PreconfiguredContentBundleKey = "PreconfiguredContent_5ca57487-4b87-483f-a712-94c648c0495c";
        const string PreconfiguredSubjectBundleKey = "PreconfiguredSubject_f68ab59f-6b38-4b59-ace3-92c5c38626f1";

        const int LargeAttachmentSizeInBytes = 20 * 1024 * 1024; // 20MB
        const int AutoSaveWorkingCopyInterval = 5000; // 2.5 seconds

        DocumentDirection previousDocumentDirection;
        int? previousDocumentFolderId;
        int? previousDocumentId;
        Dictionary<DocumentAddressType, string[]> preconfiguredEmailAddresses;
        string preconfiguredContent;
        string preconfiguredSubject;

        bool restoreWorkingCopy;

        DocumentCreationModeFlag documentCreationModeFlag = DocumentCreationModeFlag.New;
        CopyToNewOption copyToNewOption;

        DocumentPreview documentPreview = new DocumentPreview();
        Document document = new Document();

        DocumentPreview previousDocumentPreview;
        Document previousDocument;

        bool documentLoaded;
        bool templateLoaded;

        ProgressBar progress;
        NestedScrollView scrollView;
        LinearLayoutCompat linearLayout;

        View rootView;
        ToView toView;
        CcView ccView;
        BccView bccView;
        PriorityView priorityView;
        LineView lineView;
        SubjectView subjectView;
        AttachmentsView attachmentsView;
        ContentView contentView;

        RecipientsView focusedRecipientView;

        FloatingActionButton fab;

        Worker autoSaveWorkingCopyWorker;
        Rect visibleRect = new Rect();

        public static (ComposeDocumentFragment fragment, string tag) NewInstance(DocumentCreationModeFlag documentCreationModeFlag, CopyToNewOption? copyToNewOption, bool? restoreWorkingCopy,
                                                                                 DocumentDirection? previousDocumentDirection, int? previousDocumentFolderId, int? previousDocumentId,
                                                                                 Dictionary<DocumentAddressType, string[]> preconfiguredEmailAddresses, string preconfiguredContent,
                                                                                 string preconfiguredSubject)
        {
            if (copyToNewOption != null && copyToNewOption != CopyToNewOption.None)
                CommonConfig.UsageAnalytics.LogEvent(new ComposeCopyToNewEvent());
            else if (documentCreationModeFlag == DocumentCreationModeFlag.Edit)
                CommonConfig.UsageAnalytics.LogEvent(new ComposeEditDraftEvent());
            else if (documentCreationModeFlag == DocumentCreationModeFlag.Reply)
                CommonConfig.UsageAnalytics.LogEvent(new ComposeReplyEvent());
            else if (documentCreationModeFlag == DocumentCreationModeFlag.ReplyAll)
                CommonConfig.UsageAnalytics.LogEvent(new ComposeReplyAllEvent());
            else if (documentCreationModeFlag == DocumentCreationModeFlag.Forward)
                CommonConfig.UsageAnalytics.LogEvent(new ComposeForwardEvent());
            else if (documentCreationModeFlag == DocumentCreationModeFlag.New)
                CommonConfig.UsageAnalytics.LogEvent(new ComposeNewDocumentEvent());

            var args = new Bundle();

            if (documentCreationModeFlag != DocumentCreationModeFlag.None)
                args.PutInt(DocumentCreationModeFlagBundleKey, (int)documentCreationModeFlag);

            if (copyToNewOption != null)
                args.PutString(CopyToNewOptionBundleKey, Serializer.Serialize(copyToNewOption.Value));

            if (restoreWorkingCopy != null)
                args.PutBoolean(RestoreWorkingCopyBundleKey, restoreWorkingCopy.Value);

            if (previousDocumentDirection != null)
                args.PutString(PreviousDocumentDirectionBundleKey, Serializer.Serialize(previousDocumentDirection.Value));

            if (previousDocumentFolderId != null)
                args.PutInt(PreviousDocumentFolderIdBundleKey, previousDocumentFolderId.Value);

            if (previousDocumentId != null)
                args.PutInt(PreviousDocumentIdBundleKey, previousDocumentId.Value);

            if (preconfiguredEmailAddresses != null)
                args.PutString(PreconfiguredEmailAddressesBundleKey, Serializer.Serialize(preconfiguredEmailAddresses));

            if (preconfiguredContent != null)
                args.PutString(PreconfiguredContentBundleKey, preconfiguredContent);

            if (preconfiguredSubject != null)
                args.PutString(PreconfiguredSubjectBundleKey, preconfiguredSubject);

            var fragment = new ComposeDocumentFragment();
            fragment.Arguments = args;

            var tag = $"{nameof(ComposeDocumentFragment)} [restoreWorkingCopy={restoreWorkingCopy}, documentCreationModeFlag={documentCreationModeFlag}, copyToNewOption={copyToNewOption}, previousDocumentFolderId={previousDocumentFolderId}, previousDocumentId={previousDocumentId}]";

            return (fragment, tag);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Arguments.ContainsKey(DocumentCreationModeFlagBundleKey))
                documentCreationModeFlag = (DocumentCreationModeFlag)Arguments.GetInt(DocumentCreationModeFlagBundleKey);

            if (Arguments.ContainsKey(CopyToNewOptionBundleKey))
                copyToNewOption = Serializer.Deserialize<CopyToNewOption>(Arguments.GetString(CopyToNewOptionBundleKey));

            if (Arguments.ContainsKey(RestoreWorkingCopyBundleKey))
                restoreWorkingCopy = Arguments.GetBoolean(RestoreWorkingCopyBundleKey);

            if (Arguments.ContainsKey(PreviousDocumentDirectionBundleKey))
                previousDocumentDirection = Serializer.Deserialize<DocumentDirection>(Arguments.GetString(PreviousDocumentDirectionBundleKey));

            if (Arguments.ContainsKey(PreviousDocumentFolderIdBundleKey))
                previousDocumentFolderId = Arguments.GetInt(PreviousDocumentFolderIdBundleKey);

            if (Arguments.ContainsKey(PreviousDocumentIdBundleKey))
                previousDocumentId = Arguments.GetInt(PreviousDocumentIdBundleKey);

            if (Arguments.ContainsKey(PreconfiguredEmailAddressesBundleKey))
                preconfiguredEmailAddresses = Serializer.Deserialize<Dictionary<DocumentAddressType, string[]>>(Arguments.GetString(PreconfiguredEmailAddressesBundleKey));

            if (Arguments.ContainsKey(PreconfiguredContentBundleKey))
                preconfiguredContent = Arguments.GetString(PreconfiguredContentBundleKey);

            if (Arguments.ContainsKey(PreconfiguredSubjectBundleKey))
                preconfiguredSubject = Arguments.GetString(PreconfiguredSubjectBundleKey);

            restoreWorkingCopy = restoreWorkingCopy || Restored;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"{nameof(ComposeDocumentFragment)} [restoreWorkingCopy={restoreWorkingCopy}, documentCreationModeFlag={documentCreationModeFlag}, copyToNewOption={copyToNewOption}, previousDocumentFolderId={previousDocumentFolderId}, previousDocumentId={previousDocumentId}]");

            rootView = inflater.Inflate(Resource.Layout.linear_layout_with_progress, container, false);
            rootView.ViewTreeObserver.GlobalLayout += RootView_OnGlobalLayout;

            progress = rootView.FindViewById<ProgressBar>(Resource.Id.progress);
            progress.Visibility = ViewStates.Gone;
            scrollView = rootView.FindViewById<NestedScrollView>(Resource.Id.scroll_view);
            scrollView.Visibility = ViewStates.Visible;
            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);

            toView = new ToView(Context);
            toView.AddButtonClicked += RecipientView_AddButtonClicked;
            toView.Edited += Subview_Edited;
            toView.ShortcodeClicked += RecipientView_ShortcodeClicked;
            subViews.Add(toView);

            ccView = new CcView(Context);
            ccView.AddButtonClicked += RecipientView_AddButtonClicked;
            ccView.Edited += Subview_Edited;
            ccView.ShortcodeClicked += RecipientView_ShortcodeClicked;
            subViews.Add(ccView);

            bccView = new BccView(Context);
            bccView.AddButtonClicked += RecipientView_AddButtonClicked;
            bccView.Edited += Subview_Edited;
            bccView.ShortcodeClicked += RecipientView_ShortcodeClicked;
            subViews.Add(bccView);

            priorityView = new PriorityView(Context);
            if (PlatformConfig.Preferences.ComposePriorityEnabled)
                subViews.Add(priorityView);

            lineView = new LineView(Context);
            lineView.Edited += Subview_Edited;
            subViews.Add(lineView);

            subjectView = new SubjectView(Context);
            subjectView.Edited += Subview_Edited;
            subViews.Add(subjectView);

            attachmentsView = new AttachmentsView(Context);
            attachmentsView.Clicked += AttachmentsView_Clicked;
            subViews.Add(attachmentsView);

            contentView = new ContentView(Context, MoveViewToCaret);
            subViews.Add(contentView);

            foreach (var subview in subViews)
            {
                linearLayout.AddView(subview);
                if (subview != attachmentsView && subview != contentView)
                    linearLayout.AddView(new Divider(Context));
            }

            fab = ((BaseAppCompatActivity)Activity).Fab;
            fab.SetImageResource(Resource.Drawable.action_send);
            fab.SetOnClickListener(new ActionOnClickListener(() => SendDocument()));
            fab.Enabled = false;
            fab.Alpha = 0.6f;
            fab.Visibility = ViewStates.Visible;

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();
            rootView.ViewTreeObserver.GlobalLayout -= RootView_OnGlobalLayout;
        }

        public override async void OnResume()
        {
            base.OnResume();

            CommonConfig.Logger.Info($"Resuming {nameof(ComposeDocumentFragment)}...");

            await LoadDocument();

            CommonConfig.Logger.Info($"Resumed {nameof(ComposeDocumentFragment)}...");
        }

        public override void OnPause()
        {
            base.OnPause();

            CommonConfig.Logger.Info($"Paused {nameof(ComposeDocumentFragment)}");
        }

        public override async void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            if (requestCode == RequestCodes.AttachmentRequestCode && resultCode == (int)Result.Ok)
                HandleAddAttachment(data);

            if (requestCode == RequestCodes.RecentAddressesRequestCode && resultCode == (int)Result.Ok)
            {
                var recipient = Serializer.Deserialize<Recipient>(data.GetStringExtra(RecentAddressesListActivity.RecipientResultKey));
                focusedRecipientView.AddRecipient(recipient.Name, recipient.Address);
                UpdateSendButtonState();
            }
            if (requestCode == RequestCodes.PhonebookRequestCode && resultCode == (int)Result.Ok)
            {
                var recipient = Serializer.Deserialize<Recipient>(data.GetStringExtra(PhonebookContactsListActivity.RecipientResultKey));
                focusedRecipientView.AddRecipient(recipient.Name, recipient.Address);
                UpdateSendButtonState();
            }
            if (requestCode == RequestCodes.ContactsRequestCode && resultCode == (int)Result.Ok)
            {
                var recipient = Serializer.Deserialize<Recipient>(data.GetStringExtra(PickerContactFolderListActivity.RecipientResultKey));
                focusedRecipientView.AddRecipient(recipient.Name, recipient.Address);
                UpdateSendButtonState();
            }
            if (requestCode == RequestCodes.InternalContactsRequestCode && resultCode == (int)Result.Ok)
            {
                var users = Serializer.Deserialize<List<SystemUser>>(data.GetStringExtra(PickerInternalContactsListActivity.RecipientResultKey));
                foreach (var user in users)
                {
                    focusedRecipientView.AddRecipient("", user.Username);
                }
                UpdateSendButtonState();
            }
            if (requestCode == RequestCodes.ShortcodesRequestCode && resultCode == (int)Result.Ok)
            {
                var shortcodeId = data.GetIntExtra(PickerShortcodesFolderListActivity.ShortcodeIdResultKey, -1);
                var folderId = data.GetIntExtra(PickerShortcodesFolderListActivity.FolderIdResultKey, -1);
                await RetrieveAndAddShortcode(shortcodeId, folderId);
                UpdateSendButtonState();
            }
            if (requestCode == RequestCodes.TemplatePreviewRequestCode && resultCode == (int)Result.Ok)
            {
                var template = Serializer.Deserialize<TemplatePreview>(data.GetStringExtra(TemplatesListActivity.TemplatePreviewResultKey));
                await GetTemplate(template, false);
            }
            if (requestCode == RequestCodes.TemplatePreviewInitialRequestCode && resultCode == (int)Result.Ok)
            {
                var template = Serializer.Deserialize<TemplatePreview>(data.GetStringExtra(TemplatesListActivity.TemplatePreviewResultKey));
                await GetTemplate(template, true);
            }
        }

        async Task RetrieveAndAddShortcode(int shortcodeId, int folderId)
        {
            try
            {
                var shortcode = await Managers.ShortcodesManager.GetShortcodeAsync(folderId, shortcodeId, SourceType.Local);

                AddAddressesFromShortcode(shortcode);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while retrieving shortcode from db [FolderId = {folderId}, ShortcodeId = {shortcodeId}]");
                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        void AddAddressesFromShortcode(Shortcode shortcode)
        {
            if (shortcode == null || shortcode.Addresses == null || !shortcode.Addresses.Any())
                return;

            var addresses = shortcode.Addresses;
            toView.AddEmails(addresses.Where(da => da.Type == CommunicationAddressType.Email && da.AddressType == DocumentAddressType.To).Select(da => da.Address), true);
            ccView.AddEmails(addresses.Where(da => da.Type == CommunicationAddressType.Email && da.AddressType == DocumentAddressType.Cc).Select(da => da.Address), true);
            bccView.AddEmails(addresses.Where(da => da.Type == CommunicationAddressType.Email && da.AddressType == DocumentAddressType.Bcc).Select(da => da.Address), true);
        }

        async Task LoadDocument()
        {
            if (documentLoaded)
                return;

            documentLoaded = true;

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.loading_document, Resource.String.please_wait);

            try
            {
                if (restoreWorkingCopy || Restored)
                {
                    var wc = await Managers.DocumentsManager.GetDocumentWorkingCopyAsync();

                    if (wc != null)
                    {
                        documentCreationModeFlag = wc.DocumentCreationModeFlag;
                        copyToNewOption = wc.CopyToNewOption;
                        previousDocumentFolderId = wc.PreviousDocumentFolderId;
                        previousDocumentId = wc.PreviousDocumentId;
                        previousDocumentDirection = wc.PreviousDocumentDirection;
                        documentPreview = wc.DocumentPreview;
                        document = wc.Document;
                    }
                }

                if (documentCreationModeFlag == DocumentCreationModeFlag.New && copyToNewOption != CopyToNewOption.None ||
                    documentCreationModeFlag == DocumentCreationModeFlag.Reply && copyToNewOption == CopyToNewOption.None ||
                    documentCreationModeFlag == DocumentCreationModeFlag.ReplyAll && copyToNewOption == CopyToNewOption.None ||
                    documentCreationModeFlag == DocumentCreationModeFlag.Forward && copyToNewOption == CopyToNewOption.None)
                {
                    var result = await Managers.DocumentsManager.GetDocumentWithPreviewAsync(previousDocumentFolderId ?? -1, previousDocumentId.Value, SourceType.Local);
                    previousDocumentPreview = result.DocumentPreview;
                    previousDocument = result.Document;
                }
                else if (documentCreationModeFlag == DocumentCreationModeFlag.Edit &&
                         previousDocumentDirection == DocumentDirection.Draft &&
                         copyToNewOption == CopyToNewOption.None)
                {
                    var result = await Managers.DocumentsManager.GetDocumentWithPreviewAsync(previousDocumentFolderId ?? -1, previousDocumentId.Value);
                    previousDocumentPreview = result.DocumentPreview;
                    previousDocument = result.Document;

                    document.Id = previousDocumentId.Value;
                    documentPreview.Id = previousDocumentId.Value;
                }

                document.Guid = Guid.NewGuid();

                await ShowDocument();
                dismissAction();

                autoSaveWorkingCopyWorker?.Stop();
                autoSaveWorkingCopyWorker = new Worker(SaveWorkingCopy, AutoSaveWorkingCopyInterval);
                autoSaveWorkingCopyWorker.Start();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error("Failed to load document", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);

                Activity?.OnBackPressed();
            }
        }

        async Task ShowDocument()
        {
            foreach (var subView in subViews)
            {
                subView.RestoreWorkingCopy = restoreWorkingCopy;
                subView.DocumentCreationModeFlag = documentCreationModeFlag;
                subView.CopyToNewOption = copyToNewOption;
                subView.Document = document;
                subView.DocumentPreview = documentPreview;
                subView.PreviousDocumentDirection = previousDocumentDirection;
                subView.PreviousDocument = previousDocument;
                subView.PreviousDocumentPreview = previousDocumentPreview;
                subView.PreconfiguredEmailAddresses = preconfiguredEmailAddresses;

                await subView.RefreshView();
            }

            if (preconfiguredSubject != null)
                subjectView.SetSubject(preconfiguredSubject);

            if (preconfiguredContent != null)
                await contentView.InsertPlainText(preconfiguredContent);

            var files = await Managers.DocumentsManager.GetDocumentWorkingCopyAttachmentsAsync();
            attachmentsView.InitializeFileDescriptions(files.Select(f => new FileDescription(f)).ToArray());

            UpdateSendButtonState();

            if (restoreWorkingCopy)
                return;

            await AskIfShouldUseTemplates();
        }

        static class RequestCodes
        {
            public const int AttachmentRequestCode = 111;
            public const int RecentAddressesRequestCode = 222;
            public const int ContactsRequestCode = 333;
            public const int InternalContactsRequestCode = 334;
            public const int ShortcodesRequestCode = 444;
            public const int PhonebookRequestCode = 555;
            public const int TemplatePreviewRequestCode = 666;
            public const int TemplatePreviewInitialRequestCode = 777;

        }

        #region Subviews event handlers

        void DoOpenRecentAddresses()
        {
            CommonConfig.UsageAnalytics.LogEvent(new ComposeContactPickerEvent(ContactPickerChoice.Recents));

            StartActivityForResult(RecentAddressesListActivity.CreateIntent(Context), RequestCodes.RecentAddressesRequestCode);
        }

        void DoOpenContacts()
        {
            CommonConfig.UsageAnalytics.LogEvent(new ComposeContactPickerEvent(ContactPickerChoice.Contacts));

            StartActivityForResult(PickerContactFolderListActivity.CreateIntent(Context), RequestCodes.ContactsRequestCode);
        }

        void DoOpenInternalContacts()
        {
            CommonConfig.UsageAnalytics.LogEvent(new ComposeContactPickerEvent(ContactPickerChoice.Internal));

            StartActivityForResult(PickerInternalContactsListActivity.CreateIntent(Context), RequestCodes.InternalContactsRequestCode);
        }

        void DoOpenShortcodes()
        {
            CommonConfig.UsageAnalytics.LogEvent(new ComposeContactPickerEvent(ContactPickerChoice.Shortcodes));

            StartActivityForResult(PickerShortcodesFolderListActivity.CreateIntent(Context), RequestCodes.ShortcodesRequestCode);
        }

        void DoOpenPhonebook()
        {
            CommonConfig.UsageAnalytics.LogEvent(new ComposeContactPickerEvent(ContactPickerChoice.Phonebook));

            StartActivityForResult(PhonebookContactsListActivity.CreateIntent(Context), RequestCodes.PhonebookRequestCode);
        }

        void Subview_Edited(object sender, EventArgs e)
        {
            ((AppCompatActivity)Activity).SupportActionBar.Title = !subjectView.Empty ? subjectView.Subject : GetString(Resource.String.new_document);
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = null;

            UpdateSendButtonState();

            if (sender is LineView &&
                PlatformConfig.Preferences.RemoveLine &&
                documentCreationModeFlag == DocumentCreationModeFlag.ReplyAll &&
                previousDocumentPreview != null &&
                previousDocumentPreview.Direction == DocumentDirection.Incoming &&
                !lineView.LineSelectedIsAmbiguous &&
                !string.IsNullOrEmpty(lineView.GetLine().FromAddress))
            {
                toView.RemoveAddressFromLine(lineView.GetLine().FromAddress);
                ccView.RemoveAddressFromLine(lineView.GetLine().FromAddress);
                bccView.RemoveAddressFromLine(lineView.GetLine().FromAddress);
            }
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void AttachmentsView_Clicked(object sender, AttachmentsView.ClickedEventArgs e)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            var option = await Dialogs.ShowListDialog(Context, e?.AttachmentDescription?.Name ?? e?.FileDescription?.Name, Resource.Array.attachment_clicked_options, true);

            if (option == 0) //Open attachment
            {
                if (e.AttachmentDescription?.FromTemplate == true)
                {
                    await Dialogs.ShowConfirmDialogAsync(Context, Resource.String.template_attachment_title, Resource.String.template_attachment_content);
                    return;
                }

                CommonConfig.UsageAnalytics.LogEvent(new ComposeOpenAttachmentEvent());

                var dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.opening_attachment, Resource.String.please_wait);

                string path = null;

                if (e.AttachmentDescription != null)
                {
                    path = await Managers.DocumentsManager.GetAttachmentAsync(e.AttachmentDescription, previousDocument, false, SourceType.Local);

                    if (string.IsNullOrWhiteSpace(path))
                    {
                        if (e.AttachmentDescription.SizeInBytes > LargeAttachmentSizeInBytes &&
                            PlatformConfig.Preferences.LargeAttachmentWarning &&
                            Integration.IsConnectedToMeteredConnection() &&
                            !await Dialogs.ShowYesNoDialogAsync(Context, Resource.String.warning, Resource.String.large_attachment))
                        {
                            dismissAction();
                            return;
                        }

                        path = await Managers.DocumentsManager.GetAttachmentAsync(e.AttachmentDescription, previousDocument, false, SourceType.Remote);
                    }
                }

                if (e.FileDescription != null)
                    path = e.FileDescription.Path;

                try
                {
                    if (string.IsNullOrWhiteSpace(path))
                        throw new Exception("Unable to open attachment");

                    var uri = Android.Support.V4.Content.FileProvider.GetUriForFile(Context, Context.PackageName + ".fileprovider", new Java.IO.File(path));
                    var mimeType = MimeTypeMap.GetMimeType(System.IO.Path.GetExtension(path));

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
                    CommonConfig.Logger.Error($"Failed to view attachment [restoreWorkingCopy={restoreWorkingCopy}, documentCreationModeFlag={documentCreationModeFlag}, copyToNewOption={copyToNewOption}, previousDocumentFolderId={previousDocumentFolderId}, previousDocumentId={previousDocumentId}, e.AttachmentDescription.Name={e.AttachmentDescription?.Name}, e.FileDescription.Name={e.FileDescription?.Name}]", ex);
                    dismissAction();
                    await Dialogs.ShowErrorDialogAsync(Activity, ex);
                }
                finally
                {
                    dismissAction();
                }
            }
            else if (option == 1) //Remove attachment
            {
                CommonConfig.UsageAnalytics.LogEvent(new ComposeRemoveAttachmentEvent());

                try
                {
                    if (e.AttachmentDescription != null)
                        attachmentsView.RemoveAttachment(sender, e.AttachmentDescription);

                    if (e.FileDescription != null)
                    {
                        await Managers.DocumentsManager.DeleteDocumentWorkingCopyAttachmentAsync(e.FileDescription.Name);
                        attachmentsView.RemoveFileDescription(sender, e.FileDescription);
                    }
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error($"Failed to remove attachment [restoreWorkingCopy={restoreWorkingCopy}, documentCreationModeFlag={documentCreationModeFlag}, copyToNewOption={copyToNewOption}, previousDocumentFolderId={previousDocumentFolderId}, previousDocumentId={previousDocumentId}, e.AttachmentDescription.Name={e.AttachmentDescription?.Name}, e.FileDescription.Name={e.FileDescription?.Name}]", ex);
                    await Dialogs.ShowErrorDialogAsync(Activity, new Exception(Resources.GetString(Resource.String.error_removing_local_attachment)));
                }
            }
        }


        async void RecipientView_AddButtonClicked(object sender, EventArgs e)
        {
            var choice = ServerConfig.SystemSettings.SystemInfo.InternalMailsAvailable
                                     ? await Dialogs.ShowListDialog(Context, Resource.String.picker_title, Resource.Array.picker_choice_with_internal_contacts, true)
                                     : await Dialogs.ShowListDialog(Context, Resource.String.picker_title, Resource.Array.picker_choice, true);

            if (choice < 0)
                return;

            focusedRecipientView = sender as RecipientsView;

            switch (choice)
            {
                case 0:
                    DoOpenRecentAddresses();
                    break;
                case 1:
                    DoOpenContacts();
                    break;
                case 2:
                    DoOpenShortcodes();
                    break;
                case 3:
                    DoOpenPhonebook();
                    break;
                case 4:
                    DoOpenInternalContacts();
                    break;
                default:
                    return;
            }

            if (choice != 2)
                focusedRecipientView.RequestEditorFocus();
        }

        private void RecipientView_ShortcodeClicked(object sender, List<DocumentAddress> addresses)
        {
            (RecipientsView view, DocumentAddressType addressType)[] addressControls = {
                (toView, DocumentAddressType.To),
                (ccView, DocumentAddressType.Cc),
                (bccView, DocumentAddressType.Bcc),
            };

            foreach (var addressControlInfo in addressControls)
                addressControlInfo.view.AddEmails(addresses.Where(da => da.AddressType == addressControlInfo.addressType)
                    .Select(da => da.Address), true);
        }

        #endregion

        #region Scrolling related

        void RootView_OnGlobalLayout(object sender, EventArgs e)
        {
            if (View == null)
                return;

            int[] windowCoordinates = new int[2];
            View.GetLocationOnScreen(windowCoordinates);

            visibleRect.Top = windowCoordinates[1];
            visibleRect.Bottom = windowCoordinates[1] + View.Height;
            visibleRect.Left = windowCoordinates[0];
            visibleRect.Right = windowCoordinates[0] + View.Width;
        }

        void MoveViewToCaret(View webView, int relativeCaretPositionDp)
        {
            if (relativeCaretPositionDp <= 0)
                return;

            int[] webViewWindowLocation = new int[2];

            webView.GetLocationOnScreen(webViewWindowLocation);
            var webViewYPosition = webViewWindowLocation[1];

            var relativeCaretPositionPx = Conversion.ConvertDpToPixels(relativeCaretPositionDp);

            var absoluteCaretPositionTop = relativeCaretPositionPx + webViewYPosition - 10; //Added a little bit of padding
            var caretSize = 50;
            var absoluteCaretPositionBottom = absoluteCaretPositionTop + caretSize;

            int delta = 0;

            if (absoluteCaretPositionBottom > visibleRect.Bottom)
                delta = absoluteCaretPositionBottom - visibleRect.Bottom;
            else if (absoluteCaretPositionTop < visibleRect.Top)
                delta = absoluteCaretPositionTop - visibleRect.Top;

            if (delta != 0)
                Activity.RunOnUiThread(() => scrollView.ScrollBy(0, delta));
        }

        #endregion

        #region Actions

        public void AskIfShouldSave()
        {
            if (previousDocumentDirection == DocumentDirection.Draft)
                Dialogs.ShowYesNoDialog(Context, Resource.String.save_draft, Resource.String.confirm_change_draft, () => SendDocument(true), DeleteAutoSavedDocumentAndClose);
            else
                Dialogs.ShowYesNoDialog(Context, Resource.String.save_draft, Resource.String.confirm_save_as_draft, () => SendDocument(true), DeleteAutoSavedDocumentAndClose, cancelable: true);
        }

        public async void DeleteAutoSavedDocumentAndClose()
        {
            try
            {
                autoSaveWorkingCopyWorker.Stop();
                await autoSaveWorkingCopyWorker.Finished();
                await Managers.DocumentsManager.DeleteDocumentWorkingCopyAsync();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while deleting autosaved document", ex);
            }

            Activity?.Finish();
        }

        void SendDocument(bool saveDraft = false)
        {
            fab.Enabled = false;

            if (saveDraft)
                CommonConfig.UsageAnalytics.LogEvent(new ComposeSaveDraftEvent());

            async void sendAction()
            {
                var dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, saveDraft ? Resource.String.saving_draft : Resource.String.sending_document, Resource.String.please_wait);

                foreach (var subView in subViews)
                    await subView.UpdateDocument();

                documentPreview.Direction = saveDraft ? DocumentDirection.Draft : DocumentDirection.Outgoing;

                if (autoSaveWorkingCopyWorker != null)
                {
                    autoSaveWorkingCopyWorker.Stop();
                    await autoSaveWorkingCopyWorker.Finished();
                }

                await Managers.DocumentsManager.SaveDocumentWorkingCopyAsync(new DocumentWorkingCopy
                {
                    DocumentCreationModeFlag = documentCreationModeFlag,
                    CopyToNewOption = copyToNewOption,
                    PreviousDocumentFolderId = previousDocumentFolderId,
                    PreviousDocumentId = previousDocumentId,
                    PreviousDocumentDirection = previousDocumentDirection,
                    DocumentPreview = documentPreview,
                    Document = document
                });

                try
                {
                    await Managers.DocumentsManager.QueueWorkingCopyToUpload();
                }
                catch (Exception ex)
                {
                    dismissAction();

                    CommonConfig.Logger.Error($"Failed to queue document for upload [saveDraft={saveDraft}, restoreWorkingCopy={restoreWorkingCopy}, documentCreationModeFlag={documentCreationModeFlag}, copyToNewOption={copyToNewOption}, previousDocumentFolderId={previousDocumentFolderId}, previousDocumentId={previousDocumentId}]", ex.InnerException);
                    await Dialogs.ShowErrorDialogAsync(Activity, ex.InnerException);
                    fab.Enabled = true;
                }

                dismissAction();
                Activity?.Finish();
            }

            var allRecipientsValid = new RecipientsView[] { toView, ccView, bccView }.All(rv => rv.AllRecipientsValid);

            if (saveDraft && lineView.LineSelectedIsAmbiguous)
            {
                Dialogs.ShowConfirmDialog(Context, Resource.String.invalid_line_draft_title, Resource.String.invalid_line_draft_content, () => fab.Enabled = true);
                return;
            }

            if (!saveDraft)
            {
                if (!allRecipientsValid && subjectView.Empty)
                    Dialogs.ShowYesNoDialog(Context, Resource.String.invalid_recipients_and_subject_title, Resource.String.invalid_recipients_and_subject_content, sendAction, () => fab.Enabled = true);
                else if (!allRecipientsValid)
                    Dialogs.ShowYesNoDialog(Context, Resource.String.invalid_recipients_title, Resource.String.invalid_recipients_content, sendAction, () => fab.Enabled = true);
                else if (subjectView.Empty)
                    Dialogs.ShowYesNoDialog(Context, Resource.String.invalid_subject_title, Resource.String.invalid_subject_content, sendAction, () => fab.Enabled = true);
                else
                    sendAction();
            }
            else
                sendAction();
        }

        async void HandleAddAttachment(Intent data)
        {
            if (data.Data != null)
            {
                var uri = data.Data;
                await HandleOneAttachment(uri);
            }
            else
            {
                if (data.ClipData == null) return;

                var mClipData = data.ClipData;

                for (var i = 0; i < mClipData.ItemCount; i++)
                {
                    var item = mClipData.GetItemAt(i);
                    await HandleOneAttachment(item.Uri);
                }
            }
        }

        async Task SaveWorkingCopy()
        {
            try
            {
                if (CommonConfig.Logger.IsDebugEnabled())
                    CommonConfig.Logger.Debug("Saving working copy...");

                foreach (var subView in subViews)
                    await subView.UpdateDocument();

                await Managers.DocumentsManager.SaveDocumentWorkingCopyAsync(new DocumentWorkingCopy
                {
                    DocumentCreationModeFlag = documentCreationModeFlag,
                    CopyToNewOption = copyToNewOption,
                    PreviousDocumentFolderId = previousDocumentFolderId,
                    PreviousDocumentId = previousDocumentId,
                    PreviousDocumentDirection = previousDocumentDirection,
                    DocumentPreview = documentPreview,
                    Document = document
                });

                CommonConfig.Logger.Info("Saved working copy");
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Failed to save working copy!", ex);
            }
        }

        private async Task HandleOneAttachment(Uri uri)
        {
            var attachmentTooBig = false;
            IFile file = null;

            var stream = Activity.ContentResolver.OpenInputStream(uri);

            string filename;

            if (uri.Scheme == "file")
                filename = uri.LastPathSegment;
            else
            {
                using var cursor = Activity.ContentResolver.Query(uri, null, null, null, null);
                var nameIndex = cursor.GetColumnIndex(OpenableColumns.DisplayName);
                cursor.MoveToFirst();
                filename = cursor.GetString(nameIndex);
            }

            try
            {
                file = await Managers.DocumentsManager.SaveDocumentWorkingCopyAttachmentAsync(filename, stream);
                var size = new Java.IO.File(file.Path).Length();

                if (size > ServerConfig.SystemSettings.DocumentsModuleInfo.MaximumAttachmentSizeBytes)
                {
                    attachmentTooBig = true;
                    await Managers.DocumentsManager.DeleteDocumentWorkingCopyAttachmentAsync(filename);
                    throw new Exception();
                }

                stream?.Dispose();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Failed to save attachment", ex.InnerException);
                var resourceStringId = attachmentTooBig
                    ? Resource.String.attachment_too_big
                    : Resource.String.error_saving_local_attachment;
                await Dialogs.ShowErrorDialogAsync(Activity, new Exception(Resources.GetString(resourceStringId)));
            }

            attachmentsView.AddFileDescription(new FileDescription(file));
        }

        #endregion

        #region Options menu related

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            menu.Clear();

            var insertTemplateItem = menu.Add(Menu.None, MenuItemActions.InsertTemplate, MenuItemActions.InsertTemplate, Resource.String.insert_template);
            insertTemplateItem.SetIcon(Resource.Drawable.action_add);
            insertTemplateItem.SetShowAsAction(ShowAsAction.Always);

            var attachmentItem = menu.Add(Menu.None, MenuItemActions.AddAttachment, MenuItemActions.AddAttachment, Resource.String.add_attachment);
            attachmentItem.SetIcon(Resource.Drawable.action_attachment);
            attachmentItem.SetShowAsAction(ShowAsAction.Always);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
                AskIfShouldSave();

            if (item.ItemId == MenuItemActions.InsertTemplate)
            {
                CommonConfig.UsageAnalytics.LogEvent(new ComposeInsertTemplateEvent());
                GetAllTemplates(false);
            }

            if (item.ItemId == MenuItemActions.AddAttachment)
            {
                CommonConfig.UsageAnalytics.LogEvent(new ComposeAddAttachmentEvent(AddAttachmentType.Local));
                AddAttachment();
            }

            return true;
        }

        void AddAttachment()
        {
            var intent = new Intent(Intent.ActionGetContent);
            intent.SetType("*/*");
            intent.PutExtra(Intent.ExtraAllowMultiple, true);
            intent.AddCategory(Intent.CategoryOpenable);
            var i = Intent.CreateChooser(intent, "File");
            StartActivityForResult(i, RequestCodes.AttachmentRequestCode);
        }

        void UpdateSendButtonState()
        {
            var isFormValid = IsFormValid();

            fab.Enabled = isFormValid;
            fab.Alpha = isFormValid ? 1f : 0.6f;
        }

        bool IsFormValid()
        {
            var recipientAdded = false;
            foreach (var recipientView in new RecipientsView[] { toView, ccView, bccView })
                recipientAdded |= !recipientView.Empty;

            if (!recipientAdded)
                return false;

            return !lineView.LineSelectedIsAmbiguous;
        }

        static class MenuItemActions
        {
            public const int InsertTemplate = 10;
            public const int AddAttachment = 20;
        }

        #endregion

        #region Template methods

        async Task AskIfShouldUseTemplates()
        {
            if (templateLoaded)
                return;

            if (documentCreationModeFlag == DocumentCreationModeFlag.Edit)
                return;

            if (copyToNewOption.HasAnyFlag(CopyToNewOption.Content, CopyToNewOption.Attachments))
                return;

            switch (PlatformConfig.Preferences.UseTemplate)
            {
                case Preferences.TemplateUsageMode.Default:
                    CommonConfig.UsageAnalytics.LogEvent(new ComposeAddTemplateEvent(TemplateType.Default));

                    await GetDefaultTemplate(true);
                    break;
                case Preferences.TemplateUsageMode.Local:
                    CommonConfig.UsageAnalytics.LogEvent(new ComposeAddTemplateEvent(TemplateType.Local));

                    await GetLocalTemplate();
                    break;
                case Preferences.TemplateUsageMode.AlwaysAsk:
                    CommonConfig.UsageAnalytics.LogEvent(new ComposeAddTemplateEvent(TemplateType.Another));

                    var result = await Dialogs.ShowListDialog(Context, Resource.String.template_question, Resource.Array.template_question_options, true);
                    switch (result)
                    {
                        case 0:
                            await GetDefaultTemplate(true);
                            break;
                        case 1:
                            await GetLocalTemplate();
                            break;
                        case 2:
                            GetAllTemplates(true);
                            break;
                        default:
                            CommonConfig.UsageAnalytics.LogEvent(new ComposeAddTemplateEvent(null));
                            break;
                    }
                    break;
            }

            templateLoaded = true;
        }

        async Task GetDefaultTemplate(bool initializing)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.loading_template, Resource.String.please_wait);

            try
            {
                var template = await Managers.DocumentsManager.GetDefaultTemplateAsync(documentCreationModeFlag);
                if (template != null)
                    await ApplyTemplate(template, initializing);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while getting default template [restoreWorkingCopy={restoreWorkingCopy}, documentCreationModeFlag={documentCreationModeFlag}, copyToNewOption={copyToNewOption}, previousDocumentFolderId={previousDocumentFolderId}, previousDocumentId={previousDocumentId}]", ex);

                dismissAction();
                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
            finally
            {
                dismissAction();
            }
        }

        void GetAllTemplates(bool initializing)
        {
            StartActivityForResult(TemplatesListActivity.CreateIntent(Context), initializing ? RequestCodes.TemplatePreviewInitialRequestCode
                                   : RequestCodes.TemplatePreviewRequestCode);
        }

        async Task GetTemplate(TemplatePreview templatePreview, bool initializing)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.loading_template, Resource.String.please_wait);

            try
            {
                var template = await Managers.DocumentsManager.GetTemplateAsync(templatePreview.Id);
                if (template != null)
                    await ApplyTemplate(template, initializing);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while getting template [template.Id={templatePreview?.Id}, restoreWorkingCopy={restoreWorkingCopy}, documentCreationModeFlag={documentCreationModeFlag}, copyToNewOption={copyToNewOption}, previousDocumentFolderId={previousDocumentFolderId}, previousDocumentId={previousDocumentId}]", ex);

                dismissAction();
                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
            finally
            {
                dismissAction();
            }
        }

        async Task GetLocalTemplate()
        {
            var localTemplate = PlatformConfig.Preferences.LocalTemplate;
            await contentView.InsertLocalTemplate(localTemplate);
        }

        async Task ApplyTemplate(Template template, bool initializing)
        {
            ProcessTemplate(template, documentPreview);

            await contentView.InsertTemplate(template, initializing);

            if (!string.IsNullOrEmpty(template.Subject))
                subjectView.SetSubject(template.Subject);

            if (template.LineGuid != Guid.Empty)
                lineView.SetLine(template.LineGuid);

            if (template.Attachments.Any())
                template.Attachments.ForEach(attachmentsView.AddAttachment);
        }

        static void ProcessTemplate(Template template, DocumentPreview documentPreview)
        {
            var templateContent = template.Content;

            var currentTime = DateTime.Now;
            var dateString = currentTime.ToString("dd-MM-yyyy");
            var timeString = currentTime.ToString("HH:mm");

            var fromNameString = string.Empty;
            if (documentPreview?.Addresses != null)
                fromNameString = documentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.From).Select(da => da.Name).FirstOrDefault() ?? string.Empty;

            if (template.ContentType == ContentType.Html)
            {
                templateContent = templateContent.Replace("&lt;FROMNAME&gt;", fromNameString);
                templateContent = templateContent.Replace("&lt;DATE&gt;", dateString);
                templateContent = templateContent.Replace("&lt;TIME&gt;", timeString);

                templateContent = templateContent.Replace("&lt;CUR&gt;", string.Empty);
                templateContent = templateContent.Replace("&lt;SOURCETEXT&gt;", string.Empty);
                templateContent = templateContent.Replace("&lt;COMPANYNAME&gt;", string.Empty);
                templateContent = templateContent.Replace("&lt;FROMNAMEWITHCOMPANY&gt;", string.Empty);
            }
            else
            {
                templateContent = templateContent.Replace("<FROMNAME>", fromNameString);
                templateContent = templateContent.Replace("<DATE>", dateString);
                templateContent = templateContent.Replace("<TIME>", timeString);

                templateContent = templateContent.Replace("<CUR>", string.Empty);
                templateContent = templateContent.Replace("<SOURCETEXT>", string.Empty);
                templateContent = templateContent.Replace("<COMPANYNAME>", string.Empty);
                templateContent = templateContent.Replace("<FROMNAMEWITHCOMPANY>", string.Empty);
            }

            template.Content = templateContent;
        }

        #endregion
    }
}