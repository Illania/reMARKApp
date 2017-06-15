using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Database;
using Android.Provider;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Support;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.Common;
using Mark5.Mobile.Droid.Ui.Views.ComposeDocumentViews;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class ComposeDocumentFragment : RetainableStateFragment
    {
        const int LargeAttachmentSizeInBytes = 20 * 1024 * 1024; // 20MB
        const int AttachmentRequestCode = 111;

        public DocumentDirection PreviousDocumentDirection { get; set; }
        public DocumentCreationModeFlag CreationModeFlag { get; set; }
        public DocumentCreationModeFlag OutgoingDocumentOriginalCreationModeFlag { get; set; }
        public Guid OutgoingDocumentGuid { get; set; }
        public OutgoingDocumentState OutgoingDocumentState { get; set; }

        public List<OutgoingDocumentAttachmentDescription> OutgoingDocumentInitialAttachments { get; set; } = new List<OutgoingDocumentAttachmentDescription>();

        public bool LocalDocument { get; set; }
        public int? PreviousDocumentFolderId { get; set; }
        public int? PreviousDocumentId { get; set; }
        public string[] PreconfiguredEmailToAddresses { get; set; }
        public string[] PreconfiguredEmailCcAddresses { get; set; }
        public string[] PreconfiguredEmailBccAddresses { get; set; }
        public CopyToNewOption CopyToNewOption { get; set; } //Ignored if CreationModeFlag != New
        public Action CloseRequest { get; set; }

        Document PreviousDocument { get; set; }
        DocumentPreview PreviousDocumentPreview { get; set; }

        Document Document { get; set; } = new Document();
        DocumentPreview DocumentPreview { get; set; } = new DocumentPreview();

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
        FloatingActionButton fab;

        List<ComposeDocumentView> subViews = new List<ComposeDocumentView>();

        AutoSaveWorker autoSaveWorker;
        int autoSaveInterval = 5 * 1000; //5 seconds

        bool documentShown;
        bool templateLoaded;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Android.OS.Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"{nameof(ComposeDocumentFragment)} [PreviousDocument.Id={PreviousDocument?.Id}, PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={CreationModeFlag}]");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_with_progress, container, false);

            progress = rootView.FindViewById<ProgressBar>(Resource.Id.progress);
            progress.Visibility = ViewStates.Gone;
            scrollView = rootView.FindViewById<ScrollView>(Resource.Id.scroll_view);
            scrollView.Visibility = ViewStates.Visible;
            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);

            toView = new ToView(Context);
            toView.Edited += Subview_Edited;
            subViews.Add(toView);

            ccView = new CcView(Context);
            ccView.Edited += Subview_Edited;
            subViews.Add(ccView);

            bccView = new BccView(Context);
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
            attachmentsView.AttachmentClicked += AttachmentsView_AttachmentClicked;
            subViews.Add(attachmentsView);

            contentView = new ContentView(Context);
            subViews.Add(contentView);

            foreach (var subview in subViews)
            {
                linearLayout.AddView(subview);
                if (subview != attachmentsView && subview != contentView)
                    linearLayout.AddView(new Divider(Context));
            }

            fab = ((View) container.Parent.Parent).FindViewById<FloatingActionButton>(Resource.Id.fab);
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

            if (OutgoingDocumentGuid == Guid.Empty)
                OutgoingDocumentGuid = Guid.NewGuid();

            await LoadDocument();

            UpdateSendButtonState();

            if (!LocalDocument || LocalDocument && OutgoingDocumentState == OutgoingDocumentState.AutoSaved)
            {
                autoSaveWorker?.Stop();
                autoSaveWorker = new AutoSaveWorker(AutoSaveAction, autoSaveInterval);
                autoSaveWorker.Start();
            }

            CommonConfig.Logger.Info($"Resumed {nameof(ComposeDocumentFragment)}...");
        }

        public override void OnPause()
        {
            base.OnPause();

            CommonConfig.Logger.Info($"Pausing {nameof(ComposeDocumentFragment)}...");

            autoSaveWorker?.Stop();

            CommonConfig.Logger.Info($"Paused {nameof(ComposeDocumentFragment)}");
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            if (requestCode == AttachmentRequestCode && resultCode == (int) Result.Ok)
                HandleLocalAttachment(data);
        }

        async Task LoadDocument()
        {
            if (PreviousDocument != null || (CreationModeFlag == DocumentCreationModeFlag.New && CopyToNewOption == CopyToNewOption.None))
            {
                await ShowDocument();
                return;
            }

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.loading_document, Resource.String.please_wait);

            try
            {
                if (LocalDocument)
                {
                    var outgoingContainer = await Managers.DocumentsManager.GetOutgoingDocumentContainerAsync(OutgoingDocumentGuid, true);
                    PreviousDocument = outgoingContainer.Document;
                    PreviousDocumentPreview = outgoingContainer.DocumentPreview;
                    PreviousDocumentId = outgoingContainer.Info.PreviousDocumentId;
                    PreviousDocumentFolderId = outgoingContainer.Info.PreviousDocumentdFolderId;
                    OutgoingDocumentState = outgoingContainer.Info.State;
                    OutgoingDocumentOriginalCreationModeFlag = outgoingContainer.Info.Flag;

                    if (outgoingContainer.Info.State == OutgoingDocumentState.Failed)
                        await Dialogs.ShowErrorDialogAsync(Activity, new Exception(Resources.GetString(Resource.String.error_while_sending_document)));

                    if (outgoingContainer.LocalAttachments != null)
                        OutgoingDocumentInitialAttachments.AddRange(outgoingContainer.LocalAttachments);
                }
                else
                {
                    var container = await Managers.DocumentsManager.GetDocumentWithPreviewAsync(PreviousDocumentFolderId, PreviousDocumentId.Value, PreviousDocumentFolderId == null ? SourceType.Auto : SourceType.Local);
                    PreviousDocument = container.Document;
                    PreviousDocumentPreview = container.DocumentPreview;

                    if (CreationModeFlag == DocumentCreationModeFlag.Edit && PreviousDocumentPreview.Direction == DocumentDirection.Draft)
                        Document.Id = DocumentPreview.Id = PreviousDocument.Id;
                }

                dismissAction();

                await ShowDocument();
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
            if (documentShown)
                return;

            documentShown = true;

            foreach (var subView in subViews)
            {
                subView.Document = Document;
                subView.DocumentPreview = DocumentPreview;
                subView.PreviousDocument = PreviousDocument;
                subView.PreviousDocumentPreview = PreviousDocumentPreview;
                subView.CreationModeFlag = CreationModeFlag;
                subView.CopyToNewOptions = CopyToNewOption;
                await subView.RefreshView();
            }

            OutgoingDocumentInitialAttachments.ForEach(attachmentsView.AddAttachment);

            if (CreationModeFlag == DocumentCreationModeFlag.New)
            {
                if (PreconfiguredEmailToAddresses != null)
                    toView.SetEmails(PreconfiguredEmailToAddresses);
                if (PreconfiguredEmailCcAddresses != null)
                    ccView.SetEmails(PreconfiguredEmailCcAddresses);
                if (PreconfiguredEmailBccAddresses != null)
                    bccView.SetEmails(PreconfiguredEmailBccAddresses);
            }

            UpdateSendButtonState();

            await AskIfShouldUseTemplates();
        }

        #region Subviews event handlers

        void Subview_Edited(object sender, EventArgs e)
        {
            ((AppCompatActivity) Activity).SupportActionBar.Title = !subjectView.Empty ? subjectView.Subject : GetString(Resource.String.new_document);
            ((AppCompatActivity) Activity).SupportActionBar.Subtitle = null;

            UpdateSendButtonState();

            if (sender is LineView && PlatformConfig.Preferences.RemoveLine && CreationModeFlag == DocumentCreationModeFlag.ReplyAll && PreviousDocumentPreview != null && PreviousDocumentPreview.Direction == DocumentDirection.Incoming)
                if (!lineView.LineSelectedIsAmbiguous && !string.IsNullOrEmpty(lineView.GetLine().FromAddress))
                {
                    toView.RemoveAddressFromLine(lineView.GetLine().FromAddress);
                    ccView.RemoveAddressFromLine(lineView.GetLine().FromAddress);
                    bccView.RemoveAddressFromLine(lineView.GetLine().FromAddress);
                }
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void AttachmentsView_AttachmentClicked(object sender, IAttachmentDescription attachment)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            var option = await Dialogs.ShowListDialog(Context, attachment.Name, Resource.Array.attachment_clicked_options, true);

            if (option == 0) //Open attachment
            {
                var dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.opening_attachment, Resource.String.please_wait);

                string path = null;

                var outgoingAttachment = attachment as OutgoingDocumentAttachmentDescription;
                if (outgoingAttachment != null)
                {
                    path = outgoingAttachment.Path;
                }
                else
                {
                    var remoteAttachment = attachment as AttachmentDescription;

                    path = await Managers.DocumentsManager.GetAttachmentAsync(remoteAttachment, Document, false, SourceType.Local);

                    if (string.IsNullOrWhiteSpace(path))
                    {
                        if (remoteAttachment.SizeInBytes > LargeAttachmentSizeInBytes && PlatformConfig.Preferences.LargeAttachmentWarning && Integration.IsConnectedToMeteredConnection() && !await Dialogs.ShowYesNoDialogAsync(Context, Resource.String.warning, Resource.String.large_attachment))
                        {
                            dismissAction();
                            return;
                        }

                        path = await Managers.DocumentsManager.GetAttachmentAsync(remoteAttachment, PreviousDocument, false, SourceType.Remote);
                    }
                }

                if (string.IsNullOrWhiteSpace(path))
                    throw new Exception("Unable to get attachment path.");

                try
                {
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
                    CommonConfig.Logger.Error($"Failed to view attachment [AttachmentName={outgoingAttachment?.Name}, PreviousDocument.Id={PreviousDocument?.Id}, PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={CreationModeFlag}]", ex);
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
                var outgoingAttachment = attachment as OutgoingDocumentAttachmentDescription;
                if (outgoingAttachment != null)
                {
                    try
                    {
                        if (!LocalDocument)
                            await Managers.DocumentsManager.RemoveOutgoingAttachmentAsync(OutgoingDocumentGuid, outgoingAttachment.Name);

                        attachmentsView.RemoveAttachment(sender, outgoingAttachment);
                    }
                    catch (Exception ex)
                    {
                        CommonConfig.Logger.Error($"Error while removing attachment [AttachmentName={outgoingAttachment?.Name}, PreviousDocument.Id={PreviousDocument?.Id}, PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={CreationModeFlag}]", ex);
                        await Dialogs.ShowErrorDialogAsync(Activity, new Exception(Resources.GetString(Resource.String.error_removing_local_attachment)));
                    }
                }
                else
                {
                    var remoteAttachment = attachment as AttachmentDescription;
                    attachmentsView.RemoveAttachment(sender, remoteAttachment);
                }
            }
        }

        #endregion

        #region Actions

        void SendDocument(bool draft = false)
        {
            Action sendAction = () =>
            {
                var dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, draft ? Resource.String.saving_draft : Resource.String.sending_document, Resource.String.please_wait);

                Task.Run(async () =>
                    {
                        foreach (var subView in subViews)
                            await subView.UpdateDocument();

                        DocumentPreview.Direction = draft ? DocumentDirection.Draft : DocumentDirection.Outgoing;
                        if (LocalDocument)
                            await SynchOutgoingAttachments(false);
                        await Managers.DocumentsManager.InsertDocumentInOutgoingAsync(OutgoingDocumentGuid, Document, DocumentPreview, LocalDocument ? OutgoingDocumentOriginalCreationModeFlag : CreationModeFlag, PreviousDocumentId ?? -1, PreviousDocumentFolderId ?? -1, 0, false, false);
                    })
                    .ContinueWith(async t =>
                        {
                            dismissAction();

                            if (t.IsFaulted)
                            {
                                CommonConfig.Logger.Error($"Failed to insert document in outgoing [isDraft={draft}, PreviousDocument.Id={PreviousDocument?.Id}, PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={CreationModeFlag}] ", t.Exception.InnerException);
                                await Dialogs.ShowErrorDialogAsync(Activity, t.Exception.InnerException);
                            }
                            else
                            {
                                Activity.Finish();
                            }
                        },
                        TaskScheduler.FromCurrentSynchronizationContext());
            };

            if (new RecipientsView[]
            {
                toView,
                ccView,
                bccView
            }.All(rv => rv.AllEmailsValid))
                sendAction();
            else
                Dialogs.ShowYesNoDialog(Context, Resource.String.invalid_emails_title, Resource.String.invalid_emails_content, sendAction, null);
        }

        void SaveModifiedOutgoingDocument()
        {
            if (!LocalDocument)
                return;

            Task.Run(async () =>
                {
                    foreach (var subView in subViews)
                        await subView.UpdateDocument();

                    await SynchOutgoingAttachments(false);
                    await Managers.DocumentsManager.SaveOutgoingDocumentAsync(OutgoingDocumentGuid, Document, DocumentPreview, LocalDocument ? OutgoingDocumentOriginalCreationModeFlag : CreationModeFlag, PreviousDocumentId ?? -1, PreviousDocumentFolderId ?? -1, 0, false, false);
                })
                .ContinueWith(async t =>
                    {
                        if (t.IsFaulted)
                        {
                            CommonConfig.Logger.Error($"Failed to save modified outgoing document [PreviousDocument.Id={PreviousDocument?.Id}, PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={CreationModeFlag}] ", t.Exception.InnerException);
                            await Dialogs.ShowErrorDialogAsync(Activity, t.Exception.InnerException);
                        }
                        else
                        {
                            Activity.Finish();
                        }
                    },
                    TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void AskIfShouldSave()
        {
            if (LocalDocument && OutgoingDocumentState != OutgoingDocumentState.AutoSaved)
                Dialogs.ShowYesNoDialog(Context, Resource.String.save_modifications, Resource.String.confirm_save_modified_document, SaveModifiedOutgoingDocument, SaveAndCloseComposeActivity);
            else if (PreviousDocumentDirection == DocumentDirection.Draft)
                Dialogs.ShowYesNoDialog(Context, Resource.String.save_draft, Resource.String.confirm_change_draft, () => SendDocument(true), SaveAndCloseComposeActivity);
            else
                Dialogs.ShowYesNoDialog(Context, Resource.String.save_draft, Resource.String.confirm_save_as_draft, () => SendDocument(true), SaveAndCloseComposeActivity);
        }

        void SaveAndCloseComposeActivity()
        {
            Task.Run(async () =>
                {
                    if (!LocalDocument)
                    {
                        await Managers.DocumentsManager.DeleteOutgoingDocumentFolder(OutgoingDocumentGuid);
                    }
                    else
                    {
                        await SynchOutgoingAttachments(true);
                        await Managers.DocumentsManager.UnlockOutgoingDocumentAsync(OutgoingDocumentGuid);
                        Managers.OutgoingDocumentsManager.Notify(OutgoingDocumentGuid);
                    }
                })
                .ContinueWith(t => { Activity.Finish(); }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public async Task SynchOutgoingAttachments(bool restoreState)
        {
            if (restoreState) //We need to remove all the newly added attachments
            {
                var currentAttachments = attachmentsView.GetOutgoingAttachments();
                var initialAttachmentsNames = OutgoingDocumentInitialAttachments.Select(a => a.Name).ToList();

                var attachmentsToRemove = currentAttachments.Where(a => !initialAttachmentsNames.Contains(a.Name));

                foreach (var attachment in attachmentsToRemove)
                    await Managers.DocumentsManager.RemoveOutgoingAttachmentAsync(OutgoingDocumentGuid, attachment.Name);
            }
            else //We need to remove all the attachments that are not there already
            {
                var currentAttachmentsNames = attachmentsView.GetOutgoingAttachments().Select(a => a.Name).ToList();
                var initialAttachments = OutgoingDocumentInitialAttachments;

                var attachmentsToRemove = initialAttachments.Where(a => !currentAttachmentsNames.Contains(a.Name));

                foreach (var attachment in attachmentsToRemove)
                    await Managers.DocumentsManager.RemoveOutgoingAttachmentAsync(OutgoingDocumentGuid, attachment.Name);
            }
        }

        void HandleLocalAttachment(Intent data)
        {
            OutgoingDocumentAttachmentDescription attachment = null;
            var attachmentTooBig = false;
            Stream stream = null;

            Task.Run(async () =>
                {
                    var uri = data.Data;
                    stream = Activity.ContentResolver.OpenInputStream(uri);

                    string filename;

                    if (uri.Scheme == "file")
                    {
                        filename = uri.LastPathSegment;
                    }
                    else
                    {
                        ICursor cursor = null;
                        try
                        {
                            cursor = Activity.ContentResolver.Query(uri, null, null, null, null);
                            var nameIndex = cursor.GetColumnIndex(OpenableColumns.DisplayName);
                            cursor.MoveToFirst();

                            filename = cursor.GetString(nameIndex);
                        }
                        finally
                        {
                            cursor.Close();
                        }
                    }

                    var path = await Managers.DocumentsManager.SaveOutgoingAttachmentAsync(OutgoingDocumentGuid, filename, stream);
                    var size = new Java.IO.File(path).Length();

                    if (size > ServerConfig.SystemSettings.DocumentsModuleInfo.MaximumAttachmentSizeBytes)
                    {
                        attachmentTooBig = true;
                        await Managers.DocumentsManager.RemoveOutgoingAttachmentAsync(OutgoingDocumentGuid, filename);
                        throw new Exception();
                    }

                    attachment = new OutgoingDocumentAttachmentDescription
                    {
                        Name = filename,
                        SizeInBytes = size,
                        Path = path
                    };
                })
                .ContinueWith(async t =>
                    {
                        stream?.Dispose();

                        if (t.IsFaulted)
                        {
                            CommonConfig.Logger.Error($"Failed to save attachment to memory [AttachmentName={attachment?.Name}, PreviousDocument.Id={PreviousDocument?.Id}, PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={CreationModeFlag}]", t.Exception.InnerException);
                            var resourceStringId = attachmentTooBig ? Resource.String.attachment_too_big : Resource.String.error_saving_local_attachment;
                            await Dialogs.ShowErrorDialogAsync(Activity, new Exception(Resources.GetString(resourceStringId)));
                        }
                        else
                        {
                            attachmentsView.AddAttachment(attachment);
                        }
                    },
                    TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void DeleteAutoSavedDocument()
        {
            Task.Run(async () => { await Managers.DocumentsManager.DeleteAutoSavedDocumentAsync(); })
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                        CommonConfig.Logger.Error("Error while deleting autosaved document", t.Exception);
                });
        }

        async Task AutoSaveAction()
        {
            foreach (var subView in subViews)
                await subView.UpdateDocument();

            await SynchOutgoingAttachments(false);

            DocumentPreview.Direction = DocumentDirection.Outgoing;
            await Managers.DocumentsManager.AutoSaveDocumentAsync(OutgoingDocumentGuid, Document, DocumentPreview, CreationModeFlag, PreviousDocumentId ?? -1, PreviousDocumentFolderId ?? -1, 0, false, false);
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
            StartActivityForResult(i, AttachmentRequestCode);
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
            foreach (var recipientView in new List<RecipientsView>
            {
                toView,
                ccView,
                bccView
            })
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

            if (CreationModeFlag == DocumentCreationModeFlag.Edit)
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
                        await GetAllTemplates();
                        break;
                }
            }

            templateLoaded = true;
        }

        async Task GetAllTemplates()
        {

            var intent = new Intent(Context, typeof(TemplatesListActivity));
            StartActivity(intent);
            //var dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.loading_templates, Resource.String.please_wait);
            //List<TemplatePreview> templatesPreviews = null;

            //try
            //{
            //    templatesPreviews = await Managers.DocumentsManager.GetTemplatePreviewsAsync();

            //    templatesPreviews = templatesPreviews.Where(t => t.CreationMode.HasFlag(CreationModeFlag) || t.CreationMode == DocumentCreationModeFlag.None)
            //                                         .OrderByDescending(tp => tp.Private).ThenBy(tp => tp.Name)
            //                                         .ToList();

            //    dismissAction();

            //    if (templatesPreviews.Any())
            //    {
            //        var templateNames = templatesPreviews.Select(t => (t.Private ? "[Private] " : "[Public] ") + t.Name).ToArray();

            //        var result = await Dialogs.ShowListDialog(Context, Resource.String.template_question, templateNames, true);
            //        var selectedPreview = templatesPreviews[result];
            //        await GetTemplate(selectedPreview);
            //    }
            //    else
            //    {
            //        await Dialogs.ShowConfirmDialogAsync(Context, Resource.String.no_templates_title, Resource.String.no_templates_content);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    CommonConfig.Logger.Error($"Error while getting default template [PreviousDocument.Id={PreviousDocument?.Id}, PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={CreationModeFlag}] ", ex);

            //    dismissAction();
            //    await Dialogs.ShowErrorDialogAsync(Activity, ex);
            //}
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
                var template = await Managers.DocumentsManager.GetDefaultTemplateAsync(CreationModeFlag);
                if (template != null)
                    await ApplyTemplate(template);
                else if (errorMessageIfNull)
                    throw new Exception(Resources.GetString(Resource.String.template_null));
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while getting default template [PreviousDocument.Id={PreviousDocument?.Id}, PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={CreationModeFlag}] ", ex);

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
                CommonConfig.Logger.Error($"Error while getting template [template.Id={templatePreview?.Id}, PreviousDocument.Id={PreviousDocument?.Id}, PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={CreationModeFlag}] ", ex);

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

            lineView.SetLineFromGuid(template.LineGuid);
        }

        void ProcessTemplate(Template template)
        {
            var templateContent = template.Content;

            var currentTime = DateTime.Now;
            var dateString = currentTime.ToString("dd-MM-yyyy");
            var timeString = currentTime.ToString("HH:mm");

            var fromNameString = string.Empty;
            if (PreviousDocumentPreview != null && PreviousDocumentPreview.Addresses != null)
                fromNameString = PreviousDocumentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.From).Select(da => da.Name).FirstOrDefault() ?? string.Empty;

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
                Document = Document,
                DocumentPreview = DocumentPreview,
                PreviousDocument = PreviousDocument,
                PreviousDocumentPreview = PreviousDocumentPreview,
                PreviousDocumentFolderId = PreviousDocumentFolderId,
                PreviousDocumentId = PreviousDocumentId,
                OutgoingDocumentGuid = OutgoingDocumentGuid,
                CreationModeFlag = CreationModeFlag,
                TemplateLoaded = templateLoaded,
                ToState = toView.ReturnState(),
                CcState = ccView.ReturnState(),
                BccState = bccView.ReturnState(),
                PriorityState = priorityView.ReturnState(),
                LineState = lineView.ReturnState(),
                SubjectState = subjectView.ReturnState(),
                AttachmentsState = attachmentsView.ReturnState(),
                ContentState = contentView.ReturnState(),
                LocalDocument = LocalDocument,
                OutgoingDocumentOriginalCreationModeFlag = OutgoingDocumentOriginalCreationModeFlag,
                OutgoingDocumentState = OutgoingDocumentState,
                OutgoingDocumentInitialAttachments = OutgoingDocumentInitialAttachments,
                CopyToNewOptions = CopyToNewOption,
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var cfs = restoredState as ComposeDocumentFragmentState;
            if (cfs != null)
            {
                Document = cfs.Document;
                DocumentPreview = cfs.DocumentPreview;
                PreviousDocument = cfs.PreviousDocument;
                PreviousDocumentPreview = cfs.PreviousDocumentPreview;
                PreviousDocumentFolderId = cfs.PreviousDocumentFolderId;
                PreviousDocumentId = cfs.PreviousDocumentId;
                OutgoingDocumentGuid = cfs.OutgoingDocumentGuid;
                CreationModeFlag = cfs.CreationModeFlag;
                templateLoaded = cfs.TemplateLoaded;
                toView.State = cfs.ToState;
                ccView.State = cfs.CcState;
                bccView.State = cfs.BccState;
                priorityView.State = cfs.PriorityState;
                lineView.State = cfs.LineState;
                subjectView.State = cfs.SubjectState;
                attachmentsView.State = cfs.AttachmentsState;
                contentView.State = cfs.ContentState;
                LocalDocument = cfs.LocalDocument;
                OutgoingDocumentOriginalCreationModeFlag = cfs.OutgoingDocumentOriginalCreationModeFlag;
                OutgoingDocumentState = cfs.OutgoingDocumentState;
                OutgoingDocumentInitialAttachments = cfs.OutgoingDocumentInitialAttachments;
                CopyToNewOption = cfs.CopyToNewOptions;
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(ComposeDocumentFragment)} [CreationModeFlag={CreationModeFlag}, PreviousDocument.Id={PreviousDocument?.Id ?? -1}]";
        }

        class ComposeDocumentFragmentState : IRetainableState
        {
            public Document Document { get; set; }
            public DocumentPreview DocumentPreview { get; set; }
            public Document PreviousDocument { get; set; }
            public DocumentPreview PreviousDocumentPreview { get; set; }
            public int? PreviousDocumentFolderId { get; set; }
            public int? PreviousDocumentId { get; set; }
            public Guid OutgoingDocumentGuid { get; set; }
            public bool TemplateLoaded { get; set; }
            public bool LocalDocument { get; set; }
            public bool PermissionsAsked { get; set; }
            public OutgoingDocumentState OutgoingDocumentState { get; set; }
            public DocumentCreationModeFlag CreationModeFlag { get; set; }
            public List<OutgoingDocumentAttachmentDescription> OutgoingDocumentInitialAttachments { get; set; }
            public DocumentCreationModeFlag OutgoingDocumentOriginalCreationModeFlag { get; set; }
            public CopyToNewOption CopyToNewOptions { get; set; }
            public IComposeDocumentViewState ToState { get; set; }
            public IComposeDocumentViewState CcState { get; set; }
            public IComposeDocumentViewState BccState { get; set; }
            public IComposeDocumentViewState PriorityState { get; set; }
            public IComposeDocumentViewState LineState { get; set; }
            public IComposeDocumentViewState SubjectState { get; set; }
            public IComposeDocumentViewState AttachmentsState { get; set; }
            public IComposeDocumentViewState ContentState { get; set; }
        }

        #endregion
    }

    public class AutoSaveWorker
    {
        CancellationTokenSource cts;
        Func<Task> autoSaveAction;
        int delay;

        public AutoSaveWorker(Func<Task> autoSaveAction, int delay)
        {
            this.autoSaveAction = autoSaveAction;
            this.delay = delay;
        }

        public void Start()
        {
            cts?.Cancel();
            cts = new CancellationTokenSource();

            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(delay);
                    if (cts.IsCancellationRequested)
                        return;

                    await autoSaveAction();
                }
            });
        }

        public void Stop()
        {
            cts?.Cancel();
            cts = null;
        }
    }
}