using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Provider;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Model;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.Common;
using Mark5.Mobile.Droid.Ui.Views.ComposeDocumentViews;
using Mark5.Mobile.Droid.Utilities;
using PCLStorage;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class ComposeDocumentFragment : RetainableStateFragment
    {
        static class RequestCodes
        {
            public const int AttachmentRequestCode = 111;
            public const int RecentAddressesRequestCode = 222;
            public const int ContactsRequestCode = 333;
            public const int ShortcodesRequestCode = 444;
            public const int PhonebookRequestCode = 555;
            public const int TemplatePreviewRequestCode = 666;
        }

        const int LargeAttachmentSizeInBytes = 20 * 1024 * 1024; // 20MB
        const int AutoSaveWorkingCopyInterval = 2500; // 2.5 seconds

        public bool RestoreWorkingCopy { get; set; }

        public DocumentCreationModeFlag DocumentCreationModeFlag { get; set; } = DocumentCreationModeFlag.New;
        public CopyToNewOption CopyToNewOption { get; set; }

        public DocumentDirection PreviousDocumentDirection { get; set; }
        public int? PreviousDocumentFolderId { get; set; }
        public int? PreviousDocumentId { get; set; }
        public Dictionary<DocumentAddressType, string[]> PreconfiguredEmailAddresses { get; set; }

        public Action CloseRequest { get; set; }

        DocumentPreview documentPreview = new DocumentPreview();
        Document document = new Document();

        DocumentPreview previousDocumentPreview;
        Document previousDocument;

        bool documentLoaded;
        bool templateLoaded;

        ProgressBar progress;
        ScrollView scrollView;
        LinearLayoutCompat linearLayout;

        ToView toView;
        CcView ccView;
        BccView bccView;
        PriorityView priorityView;
        LineView lineView;
        SubjectView subjectView;
        AttachmentsView attachmentsView;
        ContentView contentView;

        readonly List<ComposeDocumentView> subViews = new List<ComposeDocumentView>(10);

        RecipientsView focusedRecipientView;

        FloatingActionButton fab;

        Worker autoSaveWorkingCopyWorker;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Android.OS.Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"{nameof(ComposeDocumentFragment)} [restoreWorkingCopy={RestoreWorkingCopy}, documentCreationModeFlag={DocumentCreationModeFlag}, copyToNewOption={CopyToNewOption}, previousDocumentFolderId={PreviousDocumentFolderId}, previousDocumentId={PreviousDocumentId}]");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_with_progress, container, false);

            progress = rootView.FindViewById<ProgressBar>(Resource.Id.progress);
            progress.Visibility = ViewStates.Gone;
            scrollView = rootView.FindViewById<ScrollView>(Resource.Id.scroll_view);
            scrollView.Visibility = ViewStates.Visible;
            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);

            toView = new ToView(Context);
            toView.AddButtonClicked += RecipientView_AddButtonClicked;
            toView.Edited += Subview_Edited;
            subViews.Add(toView);

            ccView = new CcView(Context);
            ccView.AddButtonClicked += RecipientView_AddButtonClicked;
            ccView.Edited += Subview_Edited;
            subViews.Add(ccView);

            bccView = new BccView(Context);
            bccView.AddButtonClicked += RecipientView_AddButtonClicked;
            bccView.Edited += Subview_Edited;
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

            contentView = new ContentView(Context);
            subViews.Add(contentView);

            foreach (var subview in subViews)
            {
                linearLayout.AddView(subview);
                if (subview != attachmentsView && subview != contentView)
                    linearLayout.AddView(new Divider(Context));
            }

            fab = ((View)container.Parent.Parent).FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.SetImageResource(Resource.Drawable.action_send);
            fab.SetOnClickListener(new ActionOnClickListener(() => SendDocument()));
            fab.Enabled = false;
            fab.Alpha = 0.6f;
            fab.Visibility = ViewStates.Visible;

            HasOptionsMenu = true;

            return rootView;
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
                focusedRecipientView.AddRecipent(recipient.Name, recipient.Address);
                UpdateSendButtonState();
            }
            if (requestCode == RequestCodes.PhonebookRequestCode && resultCode == (int)Result.Ok)
            {
                var recipient = Serializer.Deserialize<Recipient>(data.GetStringExtra(PhonebookContactsListActivity.RecipientResultKey));
                focusedRecipientView.AddRecipent(recipient.Name, recipient.Address);
                UpdateSendButtonState();
            }
            if (requestCode == RequestCodes.ContactsRequestCode && resultCode == (int)Result.Ok)
            {
                var recipient = Serializer.Deserialize<Recipient>(data.GetStringExtra(PickerContactFolderListActivity.RecipientResultKey));
                focusedRecipientView.AddRecipent(recipient.Name, recipient.Address);
                UpdateSendButtonState();
            }
            if (requestCode == RequestCodes.ShortcodesRequestCode && resultCode == (int)Result.Ok)
            {
                var shortcode = Serializer.Deserialize<Shortcode>(data.GetStringExtra(PickerShortcodesFolderListActivity.ShortcodesResultKey));
                AddAddressesFromShortcode(shortcode);
                UpdateSendButtonState();
            }
            if (requestCode == RequestCodes.TemplatePreviewRequestCode && resultCode == (int)Result.Ok)
            {
                var template = Serializer.Deserialize<TemplatePreview>(data.GetStringExtra(TemplatesListActivity.TemplatePreviewResultKey));
                await GetTemplate(template);
            }
        }

        void AddAddressesFromShortcode(Shortcode shortcode)
        {
            if (shortcode == null || shortcode.Addresses == null || !shortcode.Addresses.Any())
                return;

            var addresses = shortcode.Addresses;
            toView.AddEmails(addresses.Where(da => da.Type == CommunicationAddressType.Email && da.AddressType == DocumentAddressType.To).Select(da => da.Address));
            ccView.AddEmails(addresses.Where(da => da.Type == CommunicationAddressType.Email && da.AddressType == DocumentAddressType.Cc).Select(da => da.Address));
            bccView.AddEmails(addresses.Where(da => da.Type == CommunicationAddressType.Email && da.AddressType == DocumentAddressType.Bcc).Select(da => da.Address));
        }

        async Task LoadDocument()
        {

            if (documentLoaded)
                return;

            documentLoaded = true;

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.loading_document, Resource.String.please_wait);

            try
            {
                if (RestoreWorkingCopy)
                {
                    var wc = await Managers.DocumentsManager.GetDocumentWorkingCopyAsync();

                    DocumentCreationModeFlag = wc.DocumentCreationModeFlag;
                    CopyToNewOption = wc.CopyToNewOption;
                    PreviousDocumentFolderId = wc.PreviousDocumentFolderId;
                    PreviousDocumentId = wc.PreviousDocumentId;
                    PreviousDocumentDirection = wc.PreviousDocumentDirection;
                    documentPreview = wc.DocumentPreview;
                    document = wc.Document;
                }

                if (DocumentCreationModeFlag == DocumentCreationModeFlag.New && CopyToNewOption == CopyToNewOption.KeepOnlyAddresses ||
                    DocumentCreationModeFlag == DocumentCreationModeFlag.New && CopyToNewOption == CopyToNewOption.KeepTextAndAttachments ||
                    DocumentCreationModeFlag == DocumentCreationModeFlag.New && CopyToNewOption == CopyToNewOption.KeepOnlyAttachments ||
                    DocumentCreationModeFlag == DocumentCreationModeFlag.Reply && CopyToNewOption == CopyToNewOption.None ||
                    DocumentCreationModeFlag == DocumentCreationModeFlag.ReplyAll && CopyToNewOption == CopyToNewOption.None ||
                    DocumentCreationModeFlag == DocumentCreationModeFlag.Forward && CopyToNewOption == CopyToNewOption.None)
                {
                    var result = await Managers.DocumentsManager.GetDocumentWithPreviewAsync(PreviousDocumentFolderId ?? -1, PreviousDocumentId.Value);
                    previousDocumentPreview = result.DocumentPreview;
                    previousDocument = result.Document;
                }
                else if (DocumentCreationModeFlag == DocumentCreationModeFlag.Edit &&
                         PreviousDocumentDirection == DocumentDirection.Draft &&
                         CopyToNewOption == CopyToNewOption.None)
                {
                    var result = await Managers.DocumentsManager.GetDocumentWithPreviewAsync(PreviousDocumentFolderId ?? -1, PreviousDocumentId.Value);
                    previousDocumentPreview = result.DocumentPreview;
                    previousDocument = result.Document;

                    document.Id = PreviousDocumentId.Value;
                    documentPreview.Id = PreviousDocumentId.Value;
                }

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

                CloseRequest?.Invoke();
            }
        }

        async Task ShowDocument()
        {
            foreach (var subView in subViews)
            {
                subView.RestoreWorkingCopy = RestoreWorkingCopy;
                subView.DocumentCreationModeFlag = DocumentCreationModeFlag;
                subView.CopyToNewOption = CopyToNewOption;
                subView.Document = document;
                subView.DocumentPreview = documentPreview;
                subView.PreviousDocumentDirection = PreviousDocumentDirection;
                subView.PreviousDocument = previousDocument;
                subView.PreviousDocumentPreview = previousDocumentPreview;
                subView.PreconfiguredEmailAddresses = PreconfiguredEmailAddresses;

                await subView.RefreshView();
            }

            var files = await Managers.DocumentsManager.GetDocumentWorkingCopyAttachmentsAsync();
            attachmentsView.InitializeFileDescriptions(files.Select(f => new FileDescription(f)).ToArray());

            UpdateSendButtonState();

            if (RestoreWorkingCopy)
                return;

            await AskIfShouldUseTemplates();
        }

        #region Subviews event handlers

        async void RecipientView_AddButtonClicked(object sender, EventArgs e)
        {
            var choice = await Dialogs.ShowListDialog(Context, Resource.String.picker_title, Resource.Array.picker_choice, true);

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
            }

            if (choice != 2)
                focusedRecipientView.RequestEditorFocus();
        }

        void DoOpenRecentAddresses()
        {
            var i = new Intent(Activity, typeof(RecentAddressesListActivity));
            StartActivityForResult(i, RequestCodes.RecentAddressesRequestCode);
        }

        void DoOpenContacts()
        {
            var i = new Intent(Activity, typeof(PickerContactFolderListActivity));
            StartActivityForResult(i, RequestCodes.ContactsRequestCode);
        }

        void DoOpenShortcodes()
        {
            var i = new Intent(Activity, typeof(PickerShortcodesFolderListActivity));
            StartActivityForResult(i, RequestCodes.ShortcodesRequestCode);
        }

        void DoOpenPhonebook()
        {
            var i = new Intent(Activity, typeof(PhonebookContactsListActivity));
            StartActivityForResult(i, RequestCodes.PhonebookRequestCode);
        }

        void Subview_Edited(object sender, EventArgs e)
        {
            ((AppCompatActivity)Activity).SupportActionBar.Title = !subjectView.Empty ? subjectView.Subject : GetString(Resource.String.new_document);
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = null;

            UpdateSendButtonState();

            if (sender is LineView &&
                PlatformConfig.Preferences.RemoveLine &&
                DocumentCreationModeFlag == DocumentCreationModeFlag.ReplyAll &&
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
                        throw new Exception("Unable to opent attachment");

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
                    CommonConfig.Logger.Error($"Failed to view attachment [restoreWorkingCopy={RestoreWorkingCopy}, documentCreationModeFlag={DocumentCreationModeFlag}, copyToNewOption={CopyToNewOption}, previousDocumentFolderId={PreviousDocumentFolderId}, previousDocumentId={PreviousDocumentId}, e.AttachmentDescription.Name={e.AttachmentDescription?.Name}, e.FileDescription.Name={e.FileDescription?.Name}]", ex);
                    dismissAction();
                    await Dialogs.ShowErrorDialogAsync(Activity, ex);
                }
                finally
                {
                    dismissAction();
                }
            }

            if (option == 1) //Remove attachment
            {
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
                    CommonConfig.Logger.Error($"Failed to remove attachment [restoreWorkingCopy={RestoreWorkingCopy}, documentCreationModeFlag={DocumentCreationModeFlag}, copyToNewOption={CopyToNewOption}, previousDocumentFolderId={PreviousDocumentFolderId}, previousDocumentId={PreviousDocumentId}, e.AttachmentDescription.Name={e.AttachmentDescription?.Name}, e.FileDescription.Name={e.FileDescription?.Name}]", ex);
                    await Dialogs.ShowErrorDialogAsync(Activity, new Exception(Resources.GetString(Resource.String.error_removing_local_attachment)));
                }
            }
        }

        #endregion

        #region Actions

        void SendDocument(bool saveDraft = false)
        {
            Action sendAction = () =>
            {
                var dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, saveDraft ? Resource.String.saving_draft : Resource.String.sending_document, Resource.String.please_wait);

                Task.Run(async () =>
                {
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
                        DocumentCreationModeFlag = DocumentCreationModeFlag,
                        CopyToNewOption = CopyToNewOption,
                        PreviousDocumentFolderId = PreviousDocumentFolderId,
                        PreviousDocumentId = PreviousDocumentId,
                        PreviousDocumentDirection = PreviousDocumentDirection,
                        DocumentPreview = documentPreview,
                        Document = document
                    });
                    await Managers.DocumentsManager.QueueWorkingCopyToUpload();
                })
                .ContinueWith(async t =>
                {
                    dismissAction();

                    if (t.IsFaulted)
                    {
                        CommonConfig.Logger.Error($"Failed to queue document for upload [saveDraft={saveDraft}, restoreWorkingCopy={RestoreWorkingCopy}, documentCreationModeFlag={DocumentCreationModeFlag}, copyToNewOption={CopyToNewOption}, previousDocumentFolderId={PreviousDocumentFolderId}, previousDocumentId={PreviousDocumentId}]", t.Exception.InnerException);
                        await Dialogs.ShowErrorDialogAsync(Activity, t.Exception.InnerException);
                    }
                    else
                        Activity?.Finish();
                }, TaskScheduler.FromCurrentSynchronizationContext());
            };

            if (new RecipientsView[] { toView, ccView, bccView }.All(rv => rv.AllEmailsValid))
                sendAction();
            else
                Dialogs.ShowYesNoDialog(Context, Resource.String.invalid_emails_title, Resource.String.invalid_emails_content, sendAction, null);
        }

        public void AskIfShouldSave()
        {
            if (PreviousDocumentDirection == DocumentDirection.Draft)
                Dialogs.ShowYesNoDialog(Context, Resource.String.save_draft, Resource.String.confirm_change_draft, () => SendDocument(false), DeleteAutoSavedDocumentAndClose);
            else
                Dialogs.ShowYesNoDialog(Context, Resource.String.save_draft, Resource.String.confirm_save_as_draft, () => SendDocument(true), DeleteAutoSavedDocumentAndClose);
        }

        public void DeleteAutoSavedDocumentAndClose()
        {
            AsyncHelpers.RunSync(async () =>
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
            });

            CloseRequest?.Invoke();
        }

        void HandleAddAttachment(Intent data)
        {
            var attachmentTooBig = false;
            IFile file = null;
            Stream stream = null;

            Task.Run(async () =>
            {
                var uri = data.Data;
                stream = Activity.ContentResolver.OpenInputStream(uri);

                string filename;

                if (uri.Scheme == "file")
                    filename = uri.LastPathSegment;
                else
                {
                    using (var cursor = Activity.ContentResolver.Query(uri, null, null, null, null))
                    {
                        var nameIndex = cursor.GetColumnIndex(OpenableColumns.DisplayName);
                        cursor.MoveToFirst();
                        filename = cursor.GetString(nameIndex);
                    }
                }

                file = await Managers.DocumentsManager.SaveDocumentWorkingCopyAttachmentAsync(filename, stream);
                var size = new Java.IO.File(file.Path).Length();

                if (size > ServerConfig.SystemSettings.DocumentsModuleInfo.MaximumAttachmentSizeBytes)
                {
                    attachmentTooBig = true;
                    await Managers.DocumentsManager.DeleteDocumentWorkingCopyAttachmentAsync(filename);
                    throw new Exception();
                }
            })
            .ContinueWith(async t =>
            {
                stream?.Dispose();

                if (t.IsFaulted)
                {
                    CommonConfig.Logger.Error($"Failed to save attachment", t.Exception.InnerException);
                    var resourceStringId = attachmentTooBig ? Resource.String.attachment_too_big : Resource.String.error_saving_local_attachment;
                    await Dialogs.ShowErrorDialogAsync(Activity, new Exception(Resources.GetString(resourceStringId)));
                }
                else
                {
                    attachmentsView.AddFileDescription(new FileDescription(file));
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        async Task SaveWorkingCopy()
        {
            try
            {
                if (CommonConfig.Logger.IsDebugEnabled())
                    CommonConfig.Logger.Debug("Saving working copy...");

                await AsyncHelpers.RunOnUiThreadAsync(Activity, async () =>
                {
                    foreach (var subView in subViews)
                        await subView.UpdateDocument();
                });

                await Managers.DocumentsManager.SaveDocumentWorkingCopyAsync(new DocumentWorkingCopy
                {
                    DocumentCreationModeFlag = DocumentCreationModeFlag,
                    CopyToNewOption = CopyToNewOption,
                    PreviousDocumentFolderId = PreviousDocumentFolderId,
                    PreviousDocumentId = PreviousDocumentId,
                    PreviousDocumentDirection = PreviousDocumentDirection,
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

        #endregion

        #region Options menu related

        static class MenuItemActions
        {
            public const int AddAttachment = 10;
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            menu.Clear();

            var attachmentItem = menu.Add(Menu.None, MenuItemActions.AddAttachment, MenuItemActions.AddAttachment, Resource.String.add_attachment);
            attachmentItem.SetIcon(Resource.Drawable.action_attachment);
            attachmentItem.SetShowAsAction(ShowAsAction.Always);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
                AskIfShouldSave();

            if (item.ItemId == MenuItemActions.AddAttachment)
                AddAttachment();

            return true;
        }

        void AddAttachment()
        {
            var intent = new Intent(Intent.ActionGetContent);
            intent.SetType("*/*");
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
            foreach (var recipientView in new RecipientsView[] {toView,ccView,bccView})
                recipientAdded |= !recipientView.Empty;

            if (!recipientAdded)
                return false;

            if (subjectView.Empty)
                return false;

            return !lineView.LineSelectedIsAmbiguous;
        }

        #endregion

        #region Template methods

        async Task AskIfShouldUseTemplates()
        {
            if (templateLoaded)
                return;

            if (DocumentCreationModeFlag == DocumentCreationModeFlag.Edit)
            {
                CommonConfig.Logger.Info("Document opened in edit mode, no need to add template");
                return;
            }

            if (CopyToNewOption == CopyToNewOption.KeepTextAndAttachments)
            {
                CommonConfig.Logger.Info("Document copied as new with text and attachments, no need to add template");
                return;
            }

            var useTemplate = PlatformConfig.Preferences.UseTemplate;
            if (useTemplate == Preferences.TemplateUsageMode.DontUse)
                return;

            if (useTemplate == Preferences.TemplateUsageMode.Local)
            {
                await GetLocalTemplate();
            }
            else if (useTemplate == Preferences.TemplateUsageMode.Default)
            {
                await GetDefaultTemplate();
            }
            else if (useTemplate == Preferences.TemplateUsageMode.AlwaysAsk)
            {
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
                        GetAllTemplates();
                        break;
                }
            }

            templateLoaded = true;
        }

        void GetAllTemplates()
        {
            var i = new Intent(Context, typeof(TemplatesListActivity));
            StartActivityForResult(i, RequestCodes.TemplatePreviewRequestCode);
        }

        async Task GetLocalTemplate()
        {
            var localTemplate = PlatformConfig.Preferences.LocalTemplate;
            localTemplate = "\n\n\n" + localTemplate;
            await contentView.InsertLocalTemplate(localTemplate);
        }

        async Task GetDefaultTemplate(bool errorMessageIfNull = false)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.loading_template, Resource.String.please_wait);

            try
            {
                var template = await Managers.DocumentsManager.GetDefaultTemplateAsync(DocumentCreationModeFlag);
                if (template != null)
                    await ApplyTemplate(template);
                else if (errorMessageIfNull)
                    throw new Exception(Resources.GetString(Resource.String.template_null));
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while getting default template [restoreWorkingCopy={RestoreWorkingCopy}, documentCreationModeFlag={DocumentCreationModeFlag}, copyToNewOption={CopyToNewOption}, previousDocumentFolderId={PreviousDocumentFolderId}, previousDocumentId={PreviousDocumentId}]", ex);

                dismissAction();
                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
            finally
            {
                dismissAction();
            }
        }

        async Task GetTemplate(TemplatePreview templatePreview)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.loading_template, Resource.String.please_wait);

            try
            {
                var template = await Managers.DocumentsManager.GetTemplateAsync(templatePreview.Id);
                if (template != null)
                    await ApplyTemplate(template);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while getting template [template.Id={templatePreview?.Id}, restoreWorkingCopy={RestoreWorkingCopy}, documentCreationModeFlag={DocumentCreationModeFlag}, copyToNewOption={CopyToNewOption}, previousDocumentFolderId={PreviousDocumentFolderId}, previousDocumentId={PreviousDocumentId}]", ex);

                dismissAction();
                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
            finally
            {
                dismissAction();
            }
        }

        async Task ApplyTemplate(Template template)
        {
            ProcessTemplate(template);

            await contentView.InsertTemplate(template);

            if (!string.IsNullOrEmpty(template.Subject))
                subjectView.SetSubject(template.Subject);

            lineView.SetLine(template.LineGuid);
        }

        void ProcessTemplate(Template template)
        {
            var templateContent = template.Content;

            var currentTime = DateTime.Now;
            var dateString = currentTime.ToString("dd-MM-yyyy");
            var timeString = currentTime.ToString("HH:mm");

            var fromNameString = string.Empty;

            if (previousDocumentPreview != null && previousDocumentPreview.Addresses != null)
                fromNameString = previousDocumentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.From).Select(da => da.Name).FirstOrDefault() ?? string.Empty;

            if (template.ContentType == ContentType.Html)
            {
                templateContent = templateContent.Replace("&lt;FROMNAME&gt;", fromNameString);
                templateContent = templateContent.Replace("&lt;DATE&gt;", dateString);
                templateContent = templateContent.Replace("&lt;TIME&gt;", timeString);
            }
            else
            {
                templateContent = templateContent.Replace("<FROMNAME>", fromNameString);
                templateContent = templateContent.Replace("<DATE>", dateString);
                templateContent = templateContent.Replace("<TIME>", timeString);
            }

            if (template.ContentType == ContentType.Html)
            {
                templateContent = templateContent.Replace("&lt;CUR&gt;", string.Empty);
                templateContent = templateContent.Replace("&lt;SOURCETEXT&gt;", string.Empty);
                templateContent = templateContent.Replace("&lt;COMPANYNAME&gt;", string.Empty);
                templateContent = templateContent.Replace("&lt;FROMNAMEWITHCOMPANY&gt;", string.Empty);
            }
            else
            {
                templateContent = templateContent.Replace("<CUR>", string.Empty);
                templateContent = templateContent.Replace("<SOURCETEXT>", string.Empty);
                templateContent = templateContent.Replace("<COMPANYNAME>", string.Empty);
                templateContent = templateContent.Replace("<FROMNAMEWITHCOMPANY>", string.Empty);
            }

            template.Content = templateContent;
        }

        #endregion

        #region Retained State methods

        public override IRetainableState OnRetainInstanceState()
        {
            return new ComposeDocumentFragmentState
            {
                DocumentCreationModeFlag = DocumentCreationModeFlag,
                CopyToNewOptions = CopyToNewOption,
                DocumentPreview = documentPreview,
                Document = document,
                PreviousDocumentDirection = PreviousDocumentDirection,
                PreviousDocumentFolderId = PreviousDocumentFolderId,
                PreviousDocumentId = PreviousDocumentId,
                PreviousDocumentPreview = previousDocumentPreview,
                PreviousDocument = previousDocument,
                PreconfiguredEmailAddresses = PreconfiguredEmailAddresses,
                DocumentLoaded = documentLoaded,
                TemplateLoaded = templateLoaded,
                ToState = toView.GetState(),
                CcState = ccView.GetState(),
                BccState = bccView.GetState(),
                PriorityState = priorityView.GetState(),
                LineState = lineView.GetState(),
                SubjectState = subjectView.GetState(),
                AttachmentsState = attachmentsView.GetState(),
                ContentState = contentView.GetState()
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            if (restoredState is ComposeDocumentFragmentState cfs)
            {
                DocumentCreationModeFlag = cfs.DocumentCreationModeFlag;
                CopyToNewOption = cfs.CopyToNewOptions;
                documentPreview = cfs.DocumentPreview;
                document = cfs.Document;
                PreviousDocumentDirection = cfs.PreviousDocumentDirection;
                PreviousDocumentFolderId = cfs.PreviousDocumentFolderId;
                PreviousDocumentId = cfs.PreviousDocumentId;
                previousDocumentPreview = cfs.PreviousDocumentPreview;
                previousDocument = cfs.PreviousDocument;
                PreconfiguredEmailAddresses = cfs.PreconfiguredEmailAddresses;
                documentLoaded = cfs.DocumentLoaded;
                templateLoaded = cfs.TemplateLoaded;
                toView.State = cfs.ToState;
                ccView.State = cfs.CcState;
                bccView.State = cfs.BccState;
                priorityView.State = cfs.PriorityState;
                lineView.State = cfs.LineState;
                subjectView.State = cfs.SubjectState;
                attachmentsView.State = cfs.AttachmentsState;
                contentView.State = cfs.ContentState;
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(ComposeDocumentFragment)} [restoreWorkingCopy={RestoreWorkingCopy}, documentCreationModeFlag={DocumentCreationModeFlag}, copyToNewOption={CopyToNewOption}, previousDocumentFolderId={PreviousDocumentFolderId}, previousDocumentId={PreviousDocumentId}]";
        }

        class ComposeDocumentFragmentState : IRetainableState
        {
            public DocumentCreationModeFlag DocumentCreationModeFlag { get; set; }
            public CopyToNewOption CopyToNewOptions { get; set; }
            public DocumentPreview DocumentPreview { get; set; }
            public Document Document { get; set; }
            public DocumentDirection PreviousDocumentDirection { get; set; }
            public int? PreviousDocumentFolderId { get; set; }
            public int? PreviousDocumentId { get; set; }
            public DocumentPreview PreviousDocumentPreview { get; set; }
            public Document PreviousDocument { get; set; }
            public Dictionary<DocumentAddressType, string[]> PreconfiguredEmailAddresses { get; set; }
            public bool DocumentLoaded { get; set; }
            public bool TemplateLoaded { get; set; }
            public ComposeDocumentView.IComposeDocumentViewState ToState { get; set; }
            public ComposeDocumentView.IComposeDocumentViewState CcState { get; set; }
            public ComposeDocumentView.IComposeDocumentViewState BccState { get; set; }
            public ComposeDocumentView.IComposeDocumentViewState PriorityState { get; set; }
            public ComposeDocumentView.IComposeDocumentViewState LineState { get; set; }
            public ComposeDocumentView.IComposeDocumentViewState SubjectState { get; set; }
            public ComposeDocumentView.IComposeDocumentViewState AttachmentsState { get; set; }
            public ComposeDocumentView.IComposeDocumentViewState ContentState { get; set; }
        }

        #endregion
    }
}