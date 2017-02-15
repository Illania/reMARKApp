//
// Project: Mark5.Mobile.IOS
// File: ComposeDocumentViewController.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.Common.StackView;
using Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentView.SuggestionsView;
using Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentViews.Subviews;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class ComposeDocumentViewController : StackViewController
    {
        const int LargeAttachmentSizeInBytes = 20 * 1024 * 1024; // 20MB

        string DefaultTitle = Localization.GetString("new_document");

        public DocumentDirection PreviousDocumentDirection { get; set; }
        public DocumentCreationModeFlag CreationModeFlag { get; set; }
        public DocumentCreationModeFlag OutgoingDocumentOriginalCreationModeFlag { get; set; }
        public Guid OutgoingDocumentGuid { get; set; }
        public OutgoingDocumentState OutgoingDocumentState { get; set; }
        public List<OutgoingDocumentAttachmentDescription> OutgoingDocumentInitialAttachments { get; set; } = new List<OutgoingDocumentAttachmentDescription>();
        public bool LocalDocument { get; set; }
        public int? PreviousDocumentFolderId { get; set; }
        public int? PreviousDocumentId { get; set; }
        public string[] PreconfiguredEmailAddresses { get; set; }
        public Document PreviousDocument { get; set; }

        public DocumentPreview PreviousDocumentPreview { get; set; }

        Document Document { get; set; } = new Document();
        DocumentPreview DocumentPreview { get; set; } = new DocumentPreview();

        ToView toView;
        CcView ccView;
        BccView bccView;
        LineView lineView;
        PriorityView priorityView;
        SubjectView subjectView;
        AttachmentsView attachmentsView;
        ContentView contentView;
        readonly List<ComposeDocumentSubView> subViews = new List<ComposeDocumentSubView>();

        SuggestionsListView suggestionsListView;

        bool templateLoaded;
        bool documentShown;

        UIBarButtonItem cancelButtonItem;
        UIBarButtonItem sendButtonItem;
        UIBarButtonItem attachmentButtonItem;

        UIDocumentInteractionController attachmentInteractionController;

        // This value will be later updated from notification.
        float keyboardHeight = 216.0f;

        public ComposeDocumentViewController()
        {
            Title = DefaultTitle;
        }

        #region UIViewController overrides

        public override void LoadView()
        {
            base.LoadView();

            InitNavigationBar();
            InitSubViews();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            InitializeHandlers();

            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.DidShowNotification, OnKeyboardDidShowNotification);
            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillChangeFrameNotification, OnKeyboardDidShowNotification);
            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, OnKeyboardWillHideNotification);
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public override async void ViewDidAppear(bool animated)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info($"{typeof(ComposeDocumentViewController)} appeared");

            if (OutgoingDocumentGuid == Guid.Empty)
            {
                OutgoingDocumentGuid = Guid.NewGuid();
            }

            await LoadDocument();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            DeInitializeHandlers();

            NSNotificationCenter.DefaultCenter.RemoveObservers(new[]
            {
                    UIKeyboard.DidShowNotification,
                    UIKeyboard.WillChangeFrameNotification,
                    UIKeyboard.WillHideNotification
                });
        }

        #endregion

        #region Init methods

        void InitNavigationBar()
        {
            cancelButtonItem = new UIBarButtonItem();
            cancelButtonItem.Title = Localization.GetString("cancel");
            NavigationItem.SetLeftBarButtonItem(cancelButtonItem, false);

            sendButtonItem = new UIBarButtonItem();
            sendButtonItem.Title = Localization.GetString("send");
            sendButtonItem.Enabled = false;

            attachmentButtonItem = new UIBarButtonItem();
            attachmentButtonItem.Title = "ATT"; //TODO need to put an icon here
            attachmentButtonItem.Enabled = true;

            if (LocalDocument)
            {
                NavigationItem.SetRightBarButtonItems(new UIBarButtonItem[] { attachmentButtonItem }, false);
            }
            else
            {
                NavigationItem.SetRightBarButtonItems(new UIBarButtonItem[] { sendButtonItem, attachmentButtonItem }, false);
            }
        }

        void InitSubViews()
        {
            toView = new ToView();
            subViews.Add(toView);

            ccView = new CcView();
            subViews.Add(ccView);

            bccView = new BccView();
            subViews.Add(bccView);

            lineView = new LineView(this);
            subViews.Add(lineView);

            priorityView = new PriorityView(this);
            if (PlatformConfig.Preferences.ComposePriorityEnabled)
                subViews.Add(priorityView);

            subjectView = new SubjectView();
            subViews.Add(subjectView);

            attachmentsView = new AttachmentsView();
            subViews.Add(attachmentsView);

            contentView = new ContentView();
            subViews.Add(contentView);

            AddArrangedViewsWithSeparators(subViews);
        }

        void InitializeHandlers()
        {
            //Navigation Bar
            cancelButtonItem.Clicked += CancelButtonItem_Clicked;
            sendButtonItem.Clicked += SendButtonItem_Clicked;
            attachmentButtonItem.Clicked += AttachmentButtonItem_Clicked;

            //Subviews
            toView.SearchRequested += RecipientView_SearchRequested;
            toView.Edited += Subview_Edited;

            ccView.SearchRequested += RecipientView_SearchRequested;
            ccView.Edited += Subview_Edited;

            bccView.SearchRequested += RecipientView_SearchRequested;
            bccView.Edited += Subview_Edited;

            lineView.Edited += Subview_Edited;

            subjectView.Edited += Subview_Edited;

            attachmentsView.AttachmentClicked += AttachmentsView_AttachmentClicked;
            attachmentsView.DeleteAttachmentClicked += AttachmentsView_DeleteAttachmentClicked;
        }

        void DeInitializeHandlers()
        {
            //Navigation Bar
            cancelButtonItem.Clicked -= CancelButtonItem_Clicked;
            sendButtonItem.Clicked -= SendButtonItem_Clicked;
            attachmentButtonItem.Clicked -= AttachmentButtonItem_Clicked;

            //Subviews
            toView.SearchRequested -= RecipientView_SearchRequested;
            toView.Edited -= Subview_Edited;

            ccView.SearchRequested -= RecipientView_SearchRequested;
            ccView.Edited -= Subview_Edited;

            bccView.SearchRequested -= RecipientView_SearchRequested;
            bccView.Edited -= Subview_Edited;

            lineView.Edited -= Subview_Edited;

            subjectView.Edited -= Subview_Edited;

            attachmentsView.AttachmentClicked -= AttachmentsView_AttachmentClicked;
            attachmentsView.DeleteAttachmentClicked -= AttachmentsView_DeleteAttachmentClicked;


            if (suggestionsListView != null)
            {
                suggestionsListView.ShouldDisappear -= SuggestionsListView_ShouldDisappear;
            }
        }

        #endregion

        async Task LoadDocument()
        {
            if (PreviousDocument != null || CreationModeFlag == DocumentCreationModeFlag.New)
            {
                await ShowDocument();
                return;
            }

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("loading_document___"));

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
                    {
                        await Dialogs.ShowErrorDialogAsync(this, new Exception(Localization.GetString("error_while_sending_document")));
                        NavigationItem.SetRightBarButtonItems(new UIBarButtonItem[] { sendButtonItem, attachmentButtonItem }, false);
                    }
                    if (outgoingContainer.LocalAttachments != null)
                    {
                        OutgoingDocumentInitialAttachments.AddRange(outgoingContainer.LocalAttachments);
                    }
                }
                else
                {
                    var sourceType = SourceType.Auto;
                    PreviousDocument = await Managers.DocumentsManager.GetDocumentAsync(PreviousDocumentFolderId.Value, PreviousDocumentId.Value, sourceType);
                    if (CreationModeFlag == DocumentCreationModeFlag.Edit && PreviousDocumentPreview.Direction == DocumentDirection.Draft)
                    {
                        Document.Id = DocumentPreview.Id = PreviousDocument.Id;
                    }
                }

                dismissAction();
                await ShowDocument();
            }
            catch (Exception ex)
            {
                dismissAction();
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        async Task ShowDocument()
        {
            if (documentShown)
            {
                return;
            }

            foreach (var subView in subViews)
            {
                subView.Document = Document;
                subView.DocumentPreview = DocumentPreview;
                subView.PreviousDocument = PreviousDocument;
                subView.PreviousDocumentPreview = PreviousDocumentPreview;
                subView.CreationModeFlag = CreationModeFlag;
                await subView.RefreshView();
            }

            OutgoingDocumentInitialAttachments.ForEach(attachmentsView.AddAttachment);

            if (CreationModeFlag == DocumentCreationModeFlag.New && PreconfiguredEmailAddresses != null)
            {
                toView.SetEmails(PreconfiguredEmailAddresses);
            }

            sendButtonItem.Enabled = IsFormValid();

            await AskIfShouldUseTemplates();

            documentShown = true;
        }

        bool IsFormValid()
        {
            if (subjectView.Empty)
            {
                return false;
            }

            var recipientAdded = false;
            foreach (var recipientView in new List<RecipientsView> { toView, ccView, bccView })
            {
                recipientAdded |= !recipientView.Empty;
            }

            if (!recipientAdded)
            {
                return false;
            }

            return !lineView.LineSelectedIsAmbiguous;
        }

        #region Keyboard Notifications

        void OnKeyboardDidShowNotification(NSNotification notification)
        {
            keyboardHeight = UI.KeyboardHeightFromNotification(notification);

            var insets = ScrollView.ContentInset;
            insets.Bottom = keyboardHeight;
            ScrollView.ContentInset = insets;
        }

        void OnKeyboardWillHideNotification(NSNotification notification)
        {
            var insets = ScrollView.ContentInset;
            insets.Bottom = 0.0f;
            ScrollView.ContentInset = insets;
        }

        #endregion

        #region Actions

        async Task SendDocument(bool draft)
        {
            var containtsIncompleteEmail = false;
            foreach (var recipientView in new List<RecipientsView> { toView, ccView, bccView })
            {
                containtsIncompleteEmail |= recipientView.ContainsInvalidEmail();
            }

            if (containtsIncompleteEmail)
            {
                var result = await Dialogs.ShowYesNoDialogAsync(this, Localization.GetString("warining"), Localization.GetString("incorrect_email_addresses"));
                if (!result)
                {
                    return;
                }
            }

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(draft ? Localization.GetString("saving_draft___") : Localization.GetString("sending_document___"));

            try
            {
                foreach (var subView in subViews)
                {
                    await subView.UpdateDocument();
                }

                DocumentPreview.Direction = draft ? DocumentDirection.Draft : DocumentDirection.Outgoing;

                if (LocalDocument)
                {
                    await SynchOutgoingAttachments(false);
                }

                await Managers.DocumentsManager.InsertDocumentInOutgoingAsync(OutgoingDocumentGuid, Document, DocumentPreview, LocalDocument ? OutgoingDocumentOriginalCreationModeFlag : CreationModeFlag,
                                                            PreviousDocumentId ?? -1, PreviousDocumentFolderId ?? -1,
                                                           0, false, false);
                dismissAction();

                PopOrDismissViewController();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Failed to insert document in outgoing [isDraft={draft}, PreviousDocument.Id={PreviousDocument?.Id}, PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={CreationModeFlag}] ", ex);
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        void PopOrDismissViewController()
        {
            if (PresentingViewController != null)
            {
                DismissViewController(true, null);
            }
            else
            {
                NavigationController.PopViewController(true);
            }
        }

        public async Task SynchOutgoingAttachments(bool restoreState)
        {
            if (restoreState) //We need to remove all the newly added attachments - Restoring state before modifications
            {
                var currentAttachments = attachmentsView.GetOutgoingAttachments();
                var initialAttachmentsNames = OutgoingDocumentInitialAttachments.Select(a => a.Name).ToList();

                var attachmentsToRemove = currentAttachments.Where(a => !initialAttachmentsNames.Contains(a.Name));

                foreach (var attachment in attachmentsToRemove)
                {
                    await Managers.DocumentsManager.RemoveOutgoingAttachmentAsync(OutgoingDocumentGuid, attachment.Name);
                }
            }
            else //We need to remove all the attachments that are not there already
            {
                var currentAttachmentsNames = attachmentsView.GetOutgoingAttachments().Select(a => a.Name).ToList();
                var initialAttachments = OutgoingDocumentInitialAttachments;

                var attachmentsToRemove = initialAttachments.Where(a => !currentAttachmentsNames.Contains(a.Name));

                foreach (var attachment in attachmentsToRemove)
                {
                    await Managers.DocumentsManager.RemoveOutgoingAttachmentAsync(OutgoingDocumentGuid, attachment.Name);
                }
            }
        }

        #endregion

        #region Navigation Bar Event Handlers

        void AttachmentButtonItem_Clicked(object sender, EventArgs e)
        {
            var c = new UIDocumentMenuViewController(new string[] { "public.content" }, UIDocumentPickerMode.Import);
            c.Delegate = new DocumentMenuDelegate(this, HandleAttachmentUrl);
            PresentViewController(c, true, null);
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void HandleAttachmentUrl(NSUrl url)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            OutgoingDocumentAttachmentDescription attachment = null;
            Stream stream = null;

            try
            {
                //The url points to a temporary file in the app's sandbox
                var filename = url.LastPathComponent;
                stream = new FileStream(url.Path, FileMode.Open, FileAccess.Read);
                NSError _error;
                NSObject sizeObject;
                var result = url.TryGetResource(NSUrl.FileSizeKey, out sizeObject, out _error);

                if (!result)
                {
                    throw new Exception(_error.ToString());
                }

                var sizeInBytes = int.Parse(sizeObject.ToString());

                if (sizeInBytes > ServerConfig.SystemSettings.DocumentsModuleInfo.MaximumAttachmentSizeBytes)
                {
                    await Dialogs.ShowErrorDialogAsync(this, new Exception(Localization.GetString("attachment_too_big")));
                    return;
                    //TODO should remove file from common container ourselves? (otherwise it gets deleted automatically when the app closes)
                }

                var path = await Managers.DocumentsManager.SaveOutgoingAttachmentAsync(OutgoingDocumentGuid, filename, stream);

                attachment = new OutgoingDocumentAttachmentDescription
                {
                    Name = filename,
                    SizeInBytes = sizeInBytes,
                    Stream = stream,
                    Path = path
                };
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Failed to save attachment to memory [Url={url}, PreviousDocument.Id={PreviousDocument?.Id}, PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={CreationModeFlag}]", ex);
                await Dialogs.ShowErrorDialogAsync(this, new Exception(Localization.GetString("error_saving_local_attachment")));
            }

            stream?.Dispose();

            if (attachment != null)
            {
                attachmentsView.AddAttachment(attachment);
            }

        }

        async void SendButtonItem_Clicked(object sender, EventArgs e)
        {
            await SendDocument(false);
        }

        async void CancelButtonItem_Clicked(object sender, EventArgs e)
        {
            if (LocalDocument)
            {
                var confirm = await Dialogs.ShowYesNoDialogAsync(this, Localization.GetString("save_modifications"), Localization.GetString("confirm_save_modified_document"));
                if (confirm)
                {
                    await SaveModifiedOutgoingDocument();
                }
                else
                {
                    await SaveAndCloseComposeViewController();
                }
            }
            else
            {
                var confirm = await Dialogs.ShowYesNoDialogAsync(this, Localization.GetString("save_draft"), Localization.GetString("confirm_save_as_draft"));
                if (confirm)
                {
                    await SendDocument(true);
                }
                else
                {
                    await SaveAndCloseComposeViewController();
                }
            }
        }

        async Task SaveAndCloseComposeViewController()
        {
            if (!LocalDocument)
            {
                await Managers.DocumentsManager.DeleteOutgoingDocumentFolder(OutgoingDocumentGuid);
            }
            else
            {
                await SynchOutgoingAttachments(true);
            }

            PopOrDismissViewController();
        }

        async Task SaveModifiedOutgoingDocument()
        {
            if (!LocalDocument)
            {
                return;
            }

            try
            {
                foreach (var subView in subViews)
                {
                    await subView.UpdateDocument();
                }

                await SynchOutgoingAttachments(false);
                await Managers.DocumentsManager.SaveOutgoingDocumentAsync(OutgoingDocumentGuid, Document, DocumentPreview, LocalDocument ? OutgoingDocumentOriginalCreationModeFlag : CreationModeFlag,
                                                                        PreviousDocumentId ?? -1, PreviousDocumentFolderId ?? -1,
                                                                          0, false, false);

                PopOrDismissViewController();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Failed to save modified outgoing document [PreviousDocument.Id={PreviousDocument?.Id}, PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={CreationModeFlag}] ", ex);
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        #endregion

        #region Subviews Event Handlers

        void Subview_Edited(object sender, EventArgs e)
        {
            Title = !subjectView.Empty ? subjectView.Subject : DefaultTitle;
            sendButtonItem.Enabled = IsFormValid();

            if (sender is LineView && PlatformConfig.Preferences.RemoveLine && CreationModeFlag == DocumentCreationModeFlag.ReplyAll
                && PreviousDocumentPreview != null && PreviousDocumentPreview.Direction == DocumentDirection.Incoming)
            {
                if (!lineView.LineSelectedIsAmbiguous && !string.IsNullOrEmpty(lineView.GetLine().FromAddress))
                {
                    toView.RemoveAddressFromLine(lineView.GetLine().FromAddress);
                    ccView.RemoveAddressFromLine(lineView.GetLine().FromAddress);
                    bccView.RemoveAddressFromLine(lineView.GetLine().FromAddress);
                }
            }
        }

        void RecipientView_SearchRequested(object sender, string initialSearchString)
        {
            if (string.IsNullOrEmpty(initialSearchString))
            {
                return;
            }

            if (suggestionsListView == null)
            {
                suggestionsListView = new SuggestionsListView(this);
                suggestionsListView.ShouldDisappear += SuggestionsListView_ShouldDisappear;

                View.AddSubview(suggestionsListView);
                View.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(suggestionsListView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(suggestionsListView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(suggestionsListView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(suggestionsListView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1.0f, 0.0f),
                });

                View.SendSubviewToBack(suggestionsListView);
            }

            var recipientView = (RecipientsView)sender;
            suggestionsListView.Initialize(recipientView, initialSearchString);

            View.BringSubviewToFront(suggestionsListView);
        }

        void SuggestionsListView_ShouldDisappear(object sender, EventArgs e)
        {
            View.SendSubviewToBack(suggestionsListView);
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void AttachmentsView_AttachmentClicked(object sender, IAttachmentDescription attachmentDescription)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("opening_attachment___"));

            try
            {
                string path = null;

                var remoteAttachment = attachmentDescription as AttachmentDescription;
                if (remoteAttachment != null)
                {
                    path = await Managers.DocumentsManager.GetAttachmentAsync(remoteAttachment, Document, false, SourceType.Local);

                    if (string.IsNullOrWhiteSpace(path))
                    {
                        if (attachmentDescription.SizeInBytes > LargeAttachmentSizeInBytes
                            && PlatformConfig.Preferences.LargeAttachmentWarning
                            && !await Dialogs.ShowYesNoDialogAsync(this, Localization.GetString("big_attachment_title"),
                                                                   string.Format(Localization.GetString("big_attachment_warning"), UI.PrettyFileSize(remoteAttachment.SizeInBytes))))
                        {
                            dismissAction();
                            return;
                        }

                        path = await Managers.DocumentsManager.GetAttachmentAsync(remoteAttachment, Document, false, SourceType.Remote);
                    }
                }
                else
                {
                    var outgoingAttachment = attachmentDescription as OutgoingDocumentAttachmentDescription;
                    path = outgoingAttachment.Path;
                }

                if (string.IsNullOrWhiteSpace(path))
                {
                    throw new Exception("Unable to get attachment path.");
                }


                var url = NSUrl.FromFilename(path);
                attachmentInteractionController = UIDocumentInteractionController.FromUrl(url);
                attachmentInteractionController.Delegate = new AttachmentInteractionControllerDelegate(this, attachmentDescription);

                var previewSuccessful = attachmentInteractionController.PresentPreview(true);

                if (!previewSuccessful)
                {
                    CommonConfig.Logger.Info(string.Format("Failed to present preview for attachment. Presenting open with instead. [documentId={0}, attachment={1}]", Document.Id, attachmentDescription));
                    var openInSuccessful = attachmentInteractionController.PresentOptionsMenu(View.Frame, View, true);
                    if (!openInSuccessful)
                    {
                        CommonConfig.Logger.Warning(string.Format("Failed to present open in view - there is no app that can open this type of attachment installed. [documentId={0}, attachment={1}]", Document.Id, attachmentDescription));
                        await Dialogs.ShowConfirmDialogAsync(this, Localization.GetString("cannot_open_attachment_title"), Localization.GetString("cannot_open_attachment_content"));
                    }
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Failed to view attachment [document.Id={Document.Id}, attachment.Name={attachmentDescription?.Name}", ex);

                dismissAction();
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
            finally
            {
                dismissAction();
            }
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void AttachmentsView_DeleteAttachmentClicked(object sender, IAttachmentDescription attachment)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            var outgoingAttachment = attachment as OutgoingDocumentAttachmentDescription;
            if (outgoingAttachment != null)
            {
                try
                {
                    if (!LocalDocument)
                    {
                        await Managers.DocumentsManager.RemoveOutgoingAttachmentAsync(OutgoingDocumentGuid, outgoingAttachment.Name);
                    }

                    attachmentsView.RemoveAttachment(sender, outgoingAttachment);
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error($"Error while removing attachment [AttachmentName={outgoingAttachment?.Name}, PreviousDocument.Id={PreviousDocument?.Id}," +
                                              $" PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={CreationModeFlag}]", ex);
                    await Dialogs.ShowErrorDialogAsync(this, new Exception(Localization.GetString("error_removing_local_attachment")));
                }
            }
            else
            {
                var remoteAttachment = attachment as AttachmentDescription;
                attachmentsView.RemoveAttachment(sender, remoteAttachment);
            }
        }

        #endregion

        #region Template methods

        async Task AskIfShouldUseTemplates()
        {
            if (templateLoaded)
            {
                return;
            }

            if (CreationModeFlag == DocumentCreationModeFlag.Edit)
            {
                CommonConfig.Logger.Info("Document opened in edit mode, no need to add template");
                return;
            }

            var useTemplate = PlatformConfig.Preferences.UseTemplate;

            if (useTemplate == Preferences.TemplateUsageMode.DontUse)
            {
                return;
            }

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
                var templateListStrings = new string[] { Localization.GetString("template_selection_default"),
                    Localization.GetString("template_selection_local"),
                    Localization.GetString("template_selection_another") };
                var result = await Dialogs.ShowListDialogAsync(this, Localization.GetString("template_selection_title"), templateListStrings, contentView);
                switch (result)
                {
                    case -1:
                        break;
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
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("loading_templates___"));
            List<TemplatePreview> templatesPreviews = null;

            try
            {
                templatesPreviews = await Managers.DocumentsManager.GetTemplatePreviewsAsync();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while getting default template [PreviousDocument.Id={PreviousDocument?.Id}, PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={CreationModeFlag}] ", ex);

                dismissAction();
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
            finally
            {
                dismissAction();
            }

            if (templatesPreviews != null)
            {
                var templatesForCreationMode = templatesPreviews.Where(t => t.CreationMode.HasFlag(CreationModeFlag));
                if (templatesForCreationMode.Any())
                {
                    var templateNames = templatesForCreationMode.Select(t => (t.Private ? "[Private] " : "[Public] ") + t.Name).ToArray();

                    var result = await Dialogs.ShowListDialogAsync(this, Localization.GetString("template_question"), templateNames, contentView);
                    var selectedPreview = templatesPreviews[result];
                    await GetTemplate(selectedPreview);
                }
                else
                {
                    await Dialogs.ShowConfirmDialogAsync(this, Localization.GetString("no_templates_title"), Localization.GetString("no_templates_content"));
                }
            }
        }

        async Task GetLocalTemplate()
        {
            var localTemplate = PlatformConfig.Preferences.LocalTemplate;
            await contentView.InsertLocalTemplate(localTemplate);
        }

        async Task GetDefaultTemplate(bool errorMessageIfNull = false)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("loading_template___"));

            try
            {
                var template = await Managers.DocumentsManager.GetDefaultTemplateAsync(CreationModeFlag);
                if (template != null)
                {
                    await ApplyTemplate(template);
                }
                else if (errorMessageIfNull)
                {
                    throw new Exception(Localization.GetString("template_null"));
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while getting default template [PreviousDocument.Id={PreviousDocument?.Id}, PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={CreationModeFlag}] ", ex);

                dismissAction();
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
            finally
            {
                dismissAction();
            }
        }

        async Task GetTemplate(TemplatePreview templatePreview)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("loading_template___"));

            try
            {
                var template = await Managers.DocumentsManager.GetTemplateAsync(templatePreview.Id);
                if (template != null)
                {
                    await ApplyTemplate(template);
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while getting template [template.Id={templatePreview?.Id}, PreviousDocument.Id={PreviousDocument?.Id}, PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={CreationModeFlag}] ", ex);

                dismissAction();
                await Dialogs.ShowErrorDialogAsync(this, ex);
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
            {
                subjectView.Subject = template.Subject;
            }

            lineView.SetLineFromGuid(template.LineGuid);
        }

        void ProcessTemplate(Template template)
        {
            var templateContent = template.Content;

            var currentTime = DateTime.Now;
            var dateString = currentTime.ToString("dd-MM-yyyy");
            var timeString = currentTime.ToString("HH:mm");

            string fromNameString = string.Empty;
            if (PreviousDocumentPreview != null && PreviousDocumentPreview.Addresses != null)
            {
                fromNameString = PreviousDocumentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.From).Select(da => da.Name).FirstOrDefault() ?? string.Empty;
            }

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

        class DocumentMenuDelegate : UIDocumentMenuDelegate, IUIDocumentPickerDelegate
        {
            ComposeDocumentViewController viewController;
            Action<NSUrl> urlHanlder;

            public DocumentMenuDelegate(ComposeDocumentViewController vc, Action<NSUrl> handler)
            {
                viewController = vc;
                urlHanlder = handler;
            }

            public void DidPickDocument(UIDocumentPickerViewController controller, NSUrl url)
            {
                urlHanlder(url);
            }

            public override void DidPickDocumentPicker(UIDocumentMenuViewController documentMenu, UIDocumentPickerViewController documentPicker)
            {
                documentPicker.Delegate = this;
                documentPicker.ModalPresentationStyle = UIModalPresentationStyle.FormSheet;
                viewController.PresentViewController(documentPicker, true, null);
            }

            public override void WasCancelled(UIDocumentMenuViewController documentMenu)
            {
            }
        }

    }
}
