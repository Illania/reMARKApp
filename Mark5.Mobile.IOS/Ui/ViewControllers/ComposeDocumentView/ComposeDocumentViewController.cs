using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Foundation;
using HtmlAgilityPack;
using MailBee.Html;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Model;
using Mark5.Mobile.IOS.Model.HubMessages;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentViews.Subviews;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;
using Mark5.Mobile.IOS.Ui.ViewControllers.MailViewerView;
using Mark5.Mobile.IOS.Utilities;
using MobileCoreServices;
using Photos;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentView
{
    public class ComposeDocumentViewController : AbstractWebViewController
    {
        const int LargeAttachmentSizeInBytes = 20 * 1024 * 1024; // 20MB
        const int AutoSaveWorkingCopyInterval = 5000; // 5 seconds

        string DefaultTitle = Localization.GetString("new_document");

        public bool RestoreWorkingCopy { get; set; }

        public DocumentCreationModeFlag DocumentCreationModeFlag { get; set; } = DocumentCreationModeFlag.New;
        public CopyToNewOption CopyToNewOption { get; set; }

        public DocumentDirection PreviousDocumentDirection { get; set; }
        public int? PreviousDocumentFolderId { get; set; }
        public int? PreviousDocumentId { get; set; }
        public Dictionary<DocumentAddressType, string[]> PreconfiguredEmailAddresses { get; set; }

        DocumentPreview documentPreview = new DocumentPreview();
        Document document = new Document();

        DocumentPreview previousDocumentPreview;
        Document previousDocument;

        bool documentLoaded;
        bool templateLoaded;
        string previousDocumentContent;

        UIBarButtonItem cancelButtonItem;
        UIBarButtonItem insertButtonItem;
        UIBarButtonItem sendButtonItem;

        UIStackView headerStackView;
        ToView toView;
        CcView ccView;
        BccView bccView;
        LineView lineView;
        PriorityView priorityView;
        SubjectView subjectView;
        AttachmentsView attachmentsView;

        SuggestionsListView suggestionsListView;
        Worker autoSaveWorkingCopyWorker;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            if (CopyToNewOption != CopyToNewOption.None)
                CommonConfig.UsageAnalytics.LogEvent(new ComposeCopyToNewEvent());
            else if (DocumentCreationModeFlag == DocumentCreationModeFlag.Edit)
                CommonConfig.UsageAnalytics.LogEvent(new ComposeEditDraftEvent());
            else if (DocumentCreationModeFlag == DocumentCreationModeFlag.Reply)
                CommonConfig.UsageAnalytics.LogEvent(new ComposeReplyEvent());
            else if (DocumentCreationModeFlag == DocumentCreationModeFlag.ReplyAll)
                CommonConfig.UsageAnalytics.LogEvent(new ComposeReplyAllEvent());
            else if (DocumentCreationModeFlag == DocumentCreationModeFlag.Forward)
                CommonConfig.UsageAnalytics.LogEvent(new ComposeForwardEvent());
            else if (DocumentCreationModeFlag == DocumentCreationModeFlag.New)
                CommonConfig.UsageAnalytics.LogEvent(new ComposeNewDocumentEvent());

            InitNavigationBar();
            InitializeView();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            InitializeHandlers();
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info("Appeared");

            RefreshData();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeinitializeHandlers();
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning("Received memory warning!");

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        protected override void Recycle()
        {
            base.Recycle();

            cancelButtonItem = null;
            sendButtonItem = null;
            insertButtonItem = null;

            toView?.RemoveFromSuperview();
            ccView?.RemoveFromSuperview();
            bccView?.RemoveFromSuperview();
            lineView?.RemoveFromSuperview();
            priorityView?.RemoveFromSuperview();
            subjectView?.RemoveFromSuperview();
            attachmentsView?.RemoveFromSuperview();

            suggestionsListView?.RemoveFromSuperview();

            toView = null;
            ccView = null;
            bccView = null;
            lineView = null;
            priorityView = null;
            subjectView = null;
            attachmentsView = null;
            suggestionsListView = null;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (CommonConfig.Logger.IsDebugEnabled())
                CommonConfig.Logger.Debug("Disposed");
        }

        void InitNavigationBar()
        {
            Title = DefaultTitle;

            cancelButtonItem = new UIBarButtonItem
            {
                Title = Localization.GetString("cancel")
            };
            sendButtonItem = new UIBarButtonItem
            {
                Title = Localization.GetString("send"),
                Enabled = false
            };
            insertButtonItem = new UIBarButtonItem
            {
                Image = UIImage.FromBundle("Attachment").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
                Enabled = false
            };

            NavigationItem.SetLeftBarButtonItem(cancelButtonItem, false);
            NavigationItem.SetRightBarButtonItems(new[] { sendButtonItem, insertButtonItem }, false);
        }

        void InitializeView()
        {
            View.BackgroundColor = Theme.White;

            headerStackView = new UIStackView
            {
                BackgroundColor = Theme.White,
                Axis = UILayoutConstraintAxis.Vertical,
                Alignment = UIStackViewAlignment.Fill,
                Distribution = UIStackViewDistribution.Fill,
                Spacing = 0f,
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            headerStackView.AddArrangedSubview(toView = new ToView());
            headerStackView.AddArrangedSubview(ccView = new CcView());
            headerStackView.AddArrangedSubview(bccView = new BccView());
            headerStackView.AddArrangedSubview(lineView = new LineView(this));
            if (PlatformConfig.Preferences.ComposePriorityEnabled)
                headerStackView.AddArrangedSubview(priorityView = new PriorityView(this));
            headerStackView.AddArrangedSubview(subjectView = new SubjectView());
            headerStackView.AddArrangedSubview(attachmentsView = new AttachmentsView());

            var containerView = new UIView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = Theme.LightGray,
            };

            containerView.AddSubview(headerStackView);
            containerView.AddConstraints(new[]
            {
                headerStackView.TopAnchor.ConstraintEqualTo(containerView.TopAnchor),
                headerStackView.LeftAnchor.ConstraintEqualTo(containerView.LeftAnchor),
                headerStackView.RightAnchor.ConstraintEqualTo(containerView.RightAnchor),
                headerStackView.BottomAnchor.ConstraintEqualTo(containerView.BottomAnchor),
            });

            SetHeaderView(containerView);
        }

        void InitializeHandlers()
        {
            cancelButtonItem.Clicked += CancelButtonItem_Clicked;
            insertButtonItem.Clicked += InsertButtonItem_Clicked;
            sendButtonItem.Clicked += SendButtonItem_Clicked;

            toView.AddButtonTapped += RecipientView_AddButtonTapped;
            ccView.AddButtonTapped += RecipientView_AddButtonTapped;
            bccView.AddButtonTapped += RecipientView_AddButtonTapped;

            toView.SearchRequested += RecipientView_SearchRequested;
            ccView.SearchRequested += RecipientView_SearchRequested;
            bccView.SearchRequested += RecipientView_SearchRequested;

            toView.Edited += Subview_Edited;
            ccView.Edited += Subview_Edited;
            bccView.Edited += Subview_Edited;
            lineView.Edited += Subview_Edited;
            subjectView.Edited += Subview_Edited;

            attachmentsView.Tapped += AttachmentsView_Tapped;
            attachmentsView.DeleteTapped += AttachmentsView_DeleteTapped;
        }

        void DeinitializeHandlers()
        {
            cancelButtonItem.Clicked -= CancelButtonItem_Clicked;
            insertButtonItem.Clicked -= InsertButtonItem_Clicked;
            sendButtonItem.Clicked -= SendButtonItem_Clicked;

            toView.AddButtonTapped -= RecipientView_AddButtonTapped;
            ccView.AddButtonTapped -= RecipientView_AddButtonTapped;
            bccView.AddButtonTapped -= RecipientView_AddButtonTapped;

            toView.SearchRequested -= RecipientView_SearchRequested;
            ccView.SearchRequested -= RecipientView_SearchRequested;
            bccView.SearchRequested -= RecipientView_SearchRequested;

            toView.Edited -= Subview_Edited;
            ccView.Edited -= Subview_Edited;
            bccView.Edited -= Subview_Edited;
            lineView.Edited -= Subview_Edited;
            subjectView.Edited -= Subview_Edited;

            attachmentsView.Tapped -= AttachmentsView_Tapped;
            attachmentsView.DeleteTapped -= AttachmentsView_DeleteTapped;
        }

        async void RefreshData()
        {
            await LoadDocument();
            await LoadTemplate();

            insertButtonItem.Enabled = true;
            sendButtonItem.Enabled = true;

            autoSaveWorkingCopyWorker?.Stop();
            autoSaveWorkingCopyWorker = new Worker(SaveWorkingCopy, AutoSaveWorkingCopyInterval);
            autoSaveWorkingCopyWorker.Start();
        }

        async Task LoadDocument()
        {
            if (documentLoaded)
                return;

            try
            {
                await StartRefreshing();

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

                if (DocumentCreationModeFlag == DocumentCreationModeFlag.New && CopyToNewOption != CopyToNewOption.None ||
                    DocumentCreationModeFlag == DocumentCreationModeFlag.Reply && CopyToNewOption == CopyToNewOption.None ||
                    DocumentCreationModeFlag == DocumentCreationModeFlag.ReplyAll && CopyToNewOption == CopyToNewOption.None ||
                    DocumentCreationModeFlag == DocumentCreationModeFlag.Forward && CopyToNewOption == CopyToNewOption.None)
                {
                    var result = await Managers.DocumentsManager.GetDocumentWithPreviewAsync(PreviousDocumentFolderId ?? -1, PreviousDocumentId.Value, SourceType.Local);
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

                var subViews = headerStackView.Subviews.OfType<ComposeDocumentSubView>().ToArray();
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

                    await subView.InitializeView();
                }

                if (RestoreWorkingCopy)
                {
                    var files = await Managers.DocumentsManager.GetDocumentWorkingCopyAttachmentsAsync();
                    attachmentsView.InitializeFileDescriptions(files.Select(f => new FileDescription(f)).ToArray());
                }

                if (RestoreWorkingCopy)
                    await LoadHtmlString(document.HtmlBody, HtmlProcessingConfiguration.DefaultForEditing);
                else if (previousDocumentPreview != null && PreviousDocumentDirection == DocumentDirection.Draft ||
                         (DocumentCreationModeFlag == DocumentCreationModeFlag.New && CopyToNewOption.HasFlag(CopyToNewOption.Content)))
                {
                    previousDocumentContent = null;

                    if (!string.IsNullOrWhiteSpace(previousDocument?.HtmlBody))
                        await LoadHtmlString(previousDocument.HtmlBody, HtmlProcessingConfiguration.DefaultForEditing);
                    else if (!string.IsNullOrWhiteSpace(previousDocument?.PlainTextBody))
                        await LoadPlainText(previousDocument.PlainTextBody, PlainTextProcessingConfiguration.DefaultForEditing);
                }
                else
                {
                    LoadEditor();

                    if (previousDocumentPreview != null &&
                           (DocumentCreationModeFlag == DocumentCreationModeFlag.Reply && CopyToNewOption == CopyToNewOption.None ||
                            DocumentCreationModeFlag == DocumentCreationModeFlag.ReplyAll && CopyToNewOption == CopyToNewOption.None ||
                            DocumentCreationModeFlag == DocumentCreationModeFlag.Forward && CopyToNewOption == CopyToNewOption.None))
                    {
                        if (!string.IsNullOrWhiteSpace(previousDocument?.HtmlBody))
                        {
                            var config = HtmlProcessingConfiguration.DefaultForEditing;
                            config.InjectReplyHeader = true;
                            config.ReplyHeaderParameters = GetReplyHeaderParameters(previousDocumentPreview);
                            previousDocumentContent = await ProcessHtml(previousDocument.HtmlBody, config);
                        }
                        else if (!string.IsNullOrWhiteSpace(previousDocument?.PlainTextBody))
                        {
                            var config = PlainTextProcessingConfiguration.DefaultForEditing;
                            config.InjectReplyHeader = true;
                            config.ReplyHeaderParameters = GetReplyHeaderParameters(previousDocumentPreview);
                            previousDocumentContent = await ProcessPlainText(previousDocument.PlainTextBody, config);
                        }
                        else
                            previousDocumentContent = null;
                    }

                    if (previousDocumentContent != null)
                    {
                        ToolbarItems = new[]
                        {
                            new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                            new UIBarButtonItem(Localization.GetString("edit_original_email"), UIBarButtonItemStyle.Plain, async (sender, e) =>
                            {
                                var vc = new EditOriginalDocumentViewController { Content = previousDocumentContent };
                                PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
                                var editedContent = await vc.Result;
                                if (editedContent != null)
                                    previousDocumentContent = editedContent;
                            }),
                            new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace)
                        };
                        NavigationController.SetToolbarHidden(false, false);
                    }
                }

                await EndRefreshing();

                documentLoaded = true;
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Failed to load email into editor", ex);

                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        async Task LoadTemplate()
        {
            if (templateLoaded)
                return;

            if (DocumentCreationModeFlag == DocumentCreationModeFlag.Edit)
                return;

            if (CopyToNewOption.HasAnyFlag(CopyToNewOption.Content, CopyToNewOption.Attachments))
                return;

            switch (PlatformConfig.Preferences.UseTemplate)
            {
                case Preferences.TemplateUsageMode.Default:
                    CommonConfig.UsageAnalytics.LogEvent(new ComposeAddTemplateEvent(TemplateType.Default));

                    await InsertDefaultTemplate();
                    break;
                case Preferences.TemplateUsageMode.Local:
                    CommonConfig.UsageAnalytics.LogEvent(new ComposeAddTemplateEvent(TemplateType.Local));

                    await InsertLocalTemplate();
                    break;
                case Preferences.TemplateUsageMode.AlwaysAsk:
                    CommonConfig.UsageAnalytics.LogEvent(new ComposeAddTemplateEvent(TemplateType.Another));

                    var templateListStrings = new[]
                    {
                        Localization.GetString("template_selection_default"),
                        Localization.GetString("template_selection_local"),
                        Localization.GetString("template_selection_another")
                    };

                    var result = await Dialogs.ShowListActionSheetAsync(this, templateListStrings);
                    switch (result)
                    {
                        case 0:
                            await InsertDefaultTemplate();
                            break;
                        case 1:
                            await InsertLocalTemplate();
                            break;
                        case 2:
                            await InsertTemplate();
                            break;
                    }
                    break;
            }

            templateLoaded = true;
        }

        #region Handlers

        async void CancelButtonItem_Clicked(object sender, EventArgs e)
        {
            var d = new PopoverPresentationControllerDelegate((UIBarButtonItem)sender);
            var source = await Dialogs.ShowListActionSheetAsync(this, new[] { Localization.GetString("save_draft"), Localization.GetString("delete_draft") }, d);
            if (source < 0)
                return;

            if (autoSaveWorkingCopyWorker != null)
            {
                autoSaveWorkingCopyWorker.Stop();
                await autoSaveWorkingCopyWorker.Finished();
            }

            if (source == 0)
            {
                var saved = await SaveDraft();
                if (!saved)
                    return;

                DismissViewController(true, null);
            }

            if (source == 1)
            {
                await Managers.DocumentsManager.DeleteDocumentWorkingCopyAsync();
                DismissViewController(true, null);
            }
        }

        async void InsertButtonItem_Clicked(object sender, EventArgs e)
        {
            var d = new PopoverPresentationControllerDelegate((UIBarButtonItem)sender);
            var source = await Dialogs.ShowListActionSheetAsync(this, new[] { Localization.GetString("insert_template"), Localization.GetString("take_photo"), Localization.GetString("existing_photo"), Localization.GetString("browse_files") }, d);
            if (source < 0)
                return;

            if (source == 0)
            {
                CommonConfig.UsageAnalytics.LogEvent(new ComposeInsertTemplateEvent());
                await InsertTemplate();
            }

            if (source == 1)
            {
                CommonConfig.UsageAnalytics.LogEvent(new ComposeAddAttachmentEvent(AddAttachmentType.TakePhoto));
                InsertNewPhoto(d);
            }

            if (source == 2)
            {
                CommonConfig.UsageAnalytics.LogEvent(new ComposeAddAttachmentEvent(AddAttachmentType.PickPhoto));
                InsertExistingPhoto(d);
            }

            if (source == 3)
            {
                CommonConfig.UsageAnalytics.LogEvent(new ComposeAddAttachmentEvent(AddAttachmentType.Local));
                InsertFile(d);
            }
        }

        async void SendButtonItem_Clicked(object sender, EventArgs e)
        {
            if (autoSaveWorkingCopyWorker != null)
            {
                autoSaveWorkingCopyWorker.Stop();
                await autoSaveWorkingCopyWorker.Finished();
            }

            var sent = await SendDocument();
            if (!sent)
                return;

            DismissViewController(true, null);
        }

        void Subview_Edited(object sender, EventArgs e)
        {
            if (sender is LineView
                && PlatformConfig.Preferences.RemoveLine
                && DocumentCreationModeFlag == DocumentCreationModeFlag.ReplyAll
                && previousDocumentPreview != null
                && previousDocumentPreview.Direction == DocumentDirection.Incoming
                && !lineView.LineSelectedIsAmbiguous
                && !string.IsNullOrEmpty(lineView.GetLine().FromAddress))
            {
                toView.RemoveAddressFromLine(lineView.GetLine().FromAddress);
                ccView.RemoveAddressFromLine(lineView.GetLine().FromAddress);
                bccView.RemoveAddressFromLine(lineView.GetLine().FromAddress);
            }
        }

        void RecipientView_SearchRequested(object sender, string initialSearchString)
        {
            if (string.IsNullOrEmpty(initialSearchString))
                return;

            if (suggestionsListView == null)
            {
                suggestionsListView = new SuggestionsListView(this);

                View.AddSubview(suggestionsListView);
                View.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(suggestionsListView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1f, 0f),
                    NSLayoutConstraint.Create(suggestionsListView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1f, 0f),
                    NSLayoutConstraint.Create(suggestionsListView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1f, 0f),
                    NSLayoutConstraint.Create(suggestionsListView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1f, 0f)
                });

                View.SendSubviewToBack(suggestionsListView);
            }

            suggestionsListView.Initialize((RecipientsView)sender, initialSearchString);
            suggestionsListView.ShouldDisappear -= SuggestionsListView_ShouldDisappear;
            suggestionsListView.ShouldDisappear += SuggestionsListView_ShouldDisappear;

            View.BringSubviewToFront(suggestionsListView);
        }

        async void RecipientView_AddButtonTapped(object sender, EventArgs e)
        {
            var strings = new[]
            {
                Localization.GetString("contact_picker_recent_addresses"),
                Localization.GetString("contact_picker_contacts"),
                Localization.GetString("contact_picker_shortcodes"),
                Localization.GetString("contact_picker_phonebook")
            };

            var choice = await Dialogs.ShowListActionSheetAsync(this, strings, (UIView)sender);
            switch (choice)
            {
                case 0:
                    await DoOpenRecents(sender as RecipientsView);
                    break;
                case 1:
                    await DoOpenContacts(sender as RecipientsView);
                    break;
                case 2:
                    await DoOpenShortcodes();
                    break;
                case 3:
                    await DoOpenPhonebook(sender as RecipientsView);
                    break;
                default:
                    return;
            }
        }

        async void AttachmentsView_Tapped(object sender, AttachmentsView.TappedEventArgs e)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("opening_attachment___"));

            CommonConfig.UsageAnalytics.LogEvent(new ComposeOpenAttachmentEvent());

            try
            {
                string path = null;

                if (e.AttachmentDescription != null)
                {
                    path = await Managers.DocumentsManager.GetAttachmentAsync(e.AttachmentDescription, previousDocument, false, SourceType.Local);

                    if (string.IsNullOrWhiteSpace(path))
                    {
                        if (PlatformConfig.Preferences.LargeAttachmentWarning &&
                            e.AttachmentDescription.SizeInBytes > LargeAttachmentSizeInBytes &&
                            !await Dialogs.ShowYesNoAlertAsync(this, Localization.GetString("warning"), string.Format(Localization.GetString("big_attachment_warning"), UI.PrettyFileSize(e.AttachmentDescription.SizeInBytes))))
                        {
                            dismissAction();
                            return;
                        }

                        path = await Managers.DocumentsManager.GetAttachmentAsync(e.AttachmentDescription, previousDocument, false, SourceType.Remote);
                    }
                }

                if (e.FileDescription != null)
                    path = e.FileDescription.Path;

                if (string.IsNullOrWhiteSpace(path))
                    throw new Exception("Unable to open attachment.");

                var url = NSUrl.FromFilename(path);

                if (MailViewerViewController.CanOpen(url))
                {
                    PresentViewController(new NavigationController(new MailViewerViewController(url), UIModalPresentationStyle.PageSheet), true, null);
                    return;
                }

                var attachmentInteractionController = UIDocumentInteractionController.FromUrl(url);
                attachmentInteractionController.Delegate = new DocumentInteractionControllerDelegate(this);

                var success = attachmentInteractionController.PresentPreview(true);
                if (!success)
                {
                    CommonConfig.Logger.Info($"Failed to present preview for attachment. Presenting open with instead [documentId={document.Id}, previousDocumentId={previousDocument.Id}, e.AttachmentDescription.Name={e.AttachmentDescription?.Name}, e.FileDescription.Name={e.FileDescription?.Name}]");

                    success = attachmentInteractionController.PresentOptionsMenu(View.Frame, View, true);
                    if (!success)
                    {
                        CommonConfig.Logger.Warning($"Failed to present open in view - there is no app that can open this type of attachment installed [documentId={document.Id}, previousDocumentId={previousDocument.Id}, e.AttachmentDescription.Name={e.AttachmentDescription?.Name}, e.FileDescription.Name={e.FileDescription?.Name}]");

                        await Dialogs.ShowConfirmAlertAsync(this, Localization.GetString("cannot_open_attachment_title"), Localization.GetString("cannot_open_attachment_content"));
                    }
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Failed to view attachment [document.Id={document.Id}, previousDocumentId={previousDocument.Id}, e.AttachmentDescription.Name={e.AttachmentDescription?.Name}, e.FileDescription.Name={e.FileDescription?.Name}]", ex);

                dismissAction();
                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
            finally
            {
                dismissAction();
            }
        }

        async void AttachmentsView_DeleteTapped(object sender, AttachmentsView.DeleteTappedEventArgs e)
        {
            CommonConfig.UsageAnalytics.LogEvent(new ComposeRemoveAttachmentEvent());

            try
            {
                if (e.AttachmentDescription != null)
                    attachmentsView.RemoveAttachment(e.AttachmentDescription);

                if (e.FileDescription != null)
                {
                    await Managers.DocumentsManager.DeleteDocumentWorkingCopyAttachmentAsync(e.FileDescription.Name);
                    attachmentsView.RemoveFileDescription(e.FileDescription);
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Failed to remove attachment [document.Id={document.Id}, e.AttachmentDescription.Name={e.AttachmentDescription?.Name}, e.FileDescription.Name={e.FileDescription?.Name}]", ex);
                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        #endregion

        #region Sending/saving/deleting

        async Task<bool> SendDocument()
        {
            if (!ContainsValidEmails(toView, ccView, bccView))
            {
                await Dialogs.ShowConfirmAlertAsync(this, Localization.GetString("warning"), Localization.GetString("no_email_addresses_added"));
                return false;
            }

            if (lineView.LineSelectedIsAmbiguous)
            {
                await Dialogs.ShowConfirmAlertAsync(this, Localization.GetString("warning"), Localization.GetString("no_line_selected"));
                return false;
            }

            if (ContainsInvalidEmails(toView, ccView, bccView))
            {
                var result = await Dialogs.ShowYesNoAlertAsync(this, Localization.GetString("warning"), Localization.GetString("incorrect_email_addresses_added"));
                if (!result)
                    return false;
            }

            if (subjectView.Empty)
            {
                var result = await Dialogs.ShowYesNoAlertAsync(this, Localization.GetString("warning"), Localization.GetString("no_subject_added"));
                if (!result)
                    return false;
            }

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("sending_document___"));

            try
            {
                var subViews = headerStackView.Subviews.OfType<ComposeDocumentSubView>().ToArray();
                foreach (var subView in subViews)
                    await subView.UpdateDocument();

                document.HtmlBody = await GetContent();

                documentPreview.Direction = DocumentDirection.Outgoing;

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

                dismissAction();

                return true;
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Failed to queue document for upload [previousDocumentId={PreviousDocumentId}, previousDocumentFolderId={PreviousDocumentFolderId}, creationModeFlag={DocumentCreationModeFlag}]", ex);

                await Dialogs.ShowErrorAlertAsync(this, ex);

                return false;
            }
        }

        async Task<bool> SaveDraft()
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("saving_draft___"));

            CommonConfig.UsageAnalytics.LogEvent(new ComposeSaveDraftEvent());

            try
            {
                var subViews = headerStackView.Subviews.OfType<ComposeDocumentSubView>().ToArray();
                foreach (var subView in subViews)
                    await subView.UpdateDocument();

                document.HtmlBody = await GetContent();

                documentPreview.Direction = DocumentDirection.Draft;

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

                if (previousDocumentPreview != null)
                    CommonConfig.MessengerHub.PublishAsync(new DraftSentMessage(this, previousDocumentPreview.Id));

                dismissAction();

                return true;
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Failed to queue document draft for upload [previousDocumentId={PreviousDocumentId}, previousDocumentFolderId={PreviousDocumentFolderId}, creationModeFlag={DocumentCreationModeFlag}] ", ex);

                await Dialogs.ShowErrorAlertAsync(this, ex);

                return false;
            }
        }

        protected override async Task<string> GetContent()
        {
            var newContent = await base.GetContent();
            newContent = await CleanContent(newContent);

            var oldContent = previousDocumentContent;
            if (!string.IsNullOrWhiteSpace(oldContent))
            {
                oldContent = await CleanContent(oldContent);
                var mergedContent = await MergeContent(newContent, oldContent);
                return mergedContent;
            }

            return newContent;
        }

        Task<string> CleanContent(string content)
        {
            return Task.Run(() =>
            {
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(content);

                var headNode = htmlDocument.DocumentNode.SelectSingleNode("//head");
                headNode?.ChildNodes.FirstOrDefault(n => n.Name == "link" && n.Attributes.Any(attr => attr.Name == "id" && attr.Value == "fonts"))?.Remove();
                headNode?.ChildNodes.FirstOrDefault(n => n.Name == "meta" && n.Attributes.Any(attr => attr.Name == "id" && attr.Value == "viewport"))?.Remove();
                headNode?.ChildNodes.FirstOrDefault(n => n.Name == "style" && n.Attributes.Any(attr => attr.Name == "id" && attr.Value == "style1"))?.Remove();

                var bodyNode = htmlDocument.DocumentNode.SelectSingleNode("//body");
                bodyNode?.Attributes.FirstOrDefault(attr => attr.Name == "contentEditable")?.Remove();
                bodyNode?.ChildNodes.FirstOrDefault(n => n.Name == "div" && n.Attributes.Any(attr => attr.Name == "id" && attr.Value == "headerpadding"))?.Remove();

                var editorNode = bodyNode?.SelectSingleNode("//div[@id='editor']");
                editorNode?.Attributes.FirstOrDefault(attr => attr.Name == "contentEditable")?.Remove();

                var html = htmlDocument.DocumentNode.OuterHtml;

                html = PreMailer.Net.PreMailer.MoveCssInline(html, true, null, null, true, true).Html;

                var p = new Processor();
                p.Dom.OuterHtml = html;
                html = p.Dom.ProcessToString(RuleSet.GetSafeHtmlRules(), null);

                return html;
            });
        }

        Task<string> MergeContent(string newContent, string oldContent)
        {
            return Task.Run(() =>
            {
                var newHtmlDocument = new HtmlDocument();
                newHtmlDocument.LoadHtml(newContent);
                var oldHtmlDocument = new HtmlDocument();
                oldHtmlDocument.LoadHtml(oldContent);

                var html = File.ReadAllText(NSBundle.MainBundle.PathForResource("html/blank", "html"));
                var mergedHtmlDocument = new HtmlDocument();
                mergedHtmlDocument.LoadHtml(html);

                var newNode = mergedHtmlDocument.DocumentNode.SelectSingleNode("//div[@id='new']");
                newNode.AppendChildren(newHtmlDocument.DocumentNode.SelectSingleNode("//body").ChildNodes);
                newNode.Attributes.Remove("id");
                var oldNode = mergedHtmlDocument.DocumentNode.SelectSingleNode("//div[@id='old']");
                oldNode.AppendChildren(oldHtmlDocument.DocumentNode.SelectSingleNode("//body").ChildNodes);
                oldNode.Attributes.Remove("id");

                return mergedHtmlDocument.DocumentNode.OuterHtml;
            });
        }

        #endregion

        #region Templates loading

        async Task InsertDefaultTemplate()
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("loading_template___"));

            try
            {
                var template = await Managers.DocumentsManager.GetDefaultTemplateAsync(DocumentCreationModeFlag);
                if (template == null)
                    return;

                ProcessTemplate(template, previousDocumentPreview);

                var insertTemplateJs = File.ReadAllText(NSBundle.MainBundle.PathForResource("html/insertTemplate", "js"));
                if (template.ContentType == ContentType.PlainText)
                {
                    var templateText = Regex.Replace(template.Content, @"\r\n?|\n", "\\n", RegexOptions.Multiline);
                    insertTemplateJs = ProcessWebTemplate(insertTemplateJs, "text", template.Id, templateText);
                }
                if (template.ContentType == ContentType.Html)
                {
                    var templateText = Regex.Replace(template.Content, @"\r\n?|\n", " ", RegexOptions.Multiline);
                    insertTemplateJs = ProcessWebTemplate(insertTemplateJs, "html", template.Id, templateText);
                }

                var result = await EvaluateJavaScriptAsync(insertTemplateJs);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while getting default template [PreviousDocumentId={PreviousDocumentId}, PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={DocumentCreationModeFlag}] ", ex);

                dismissAction();
                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
            finally
            {
                dismissAction();
            }
        }

        async Task InsertTemplate()
        {
            var tp = new TemplatesListViewController();
            PresentViewController(new NavigationController(tp, UIModalPresentationStyle.PageSheet), true, null);

            var templatePreview = await tp.Result;
            if (templatePreview == null)
                return;

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("loading_template___"));

            try
            {
                var template = await Managers.DocumentsManager.GetTemplateAsync(templatePreview.Id);
                if (template == null)
                    return;

                ProcessTemplate(template, previousDocumentPreview);

                var insertTemplateJs = File.ReadAllText(NSBundle.MainBundle.PathForResource("html/insertTemplate", "js"));
                if (template.ContentType == ContentType.PlainText)
                {
                    var templateText = Regex.Replace(template.Content, @"\r\n?|\n", "\\n", RegexOptions.Multiline);
                    insertTemplateJs = ProcessWebTemplate(insertTemplateJs, "text", template.Id, templateText);
                }
                if (template.ContentType == ContentType.Html)
                {
                    var templateText = Regex.Replace(template.Content, @"\r\n?|\n", " ", RegexOptions.Multiline);
                    insertTemplateJs = ProcessWebTemplate(insertTemplateJs, "html", template.Id, templateText);
                }

                var result = await EvaluateJavaScriptAsync(insertTemplateJs);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while getting template [templatePreview.Id={templatePreview?.Id}, PreviousDocumentId={PreviousDocumentId}, PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={DocumentCreationModeFlag}] ", ex);

                dismissAction();
                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
            finally
            {
                dismissAction();
            }
        }

        async Task InsertLocalTemplate()
        {
            if (string.IsNullOrEmpty(PlatformConfig.Preferences.LocalTemplate))
                return;

            var insertTemplateJs = File.ReadAllText(NSBundle.MainBundle.PathForResource("html/insertTemplate", "js"));
            var localTemplateText = Regex.Replace(PlatformConfig.Preferences.LocalTemplate, @"\r\n?|\n", "\\n", RegexOptions.Multiline);
            insertTemplateJs = ProcessWebTemplate(insertTemplateJs, "text", "local", localTemplateText);

            var result = await EvaluateJavaScriptAsync(insertTemplateJs);
        }

        #endregion

        #region Suggestions methods

        void SuggestionsListView_ShouldDisappear(object sender, EventArgs e)
        {
            View.SendSubviewToBack(suggestionsListView);
            ((SuggestionsListView)sender).ShouldDisappear -= SuggestionsListView_ShouldDisappear;
        }

        #endregion

        #region Picker methods

        async Task DoOpenPhonebook(RecipientsView recipientsView)
        {
            CommonConfig.UsageAnalytics.LogEvent(new ComposeContactPickerEvent(ContactPickerChoice.Phonebook));

            var vc = new PhonebookContactsListViewController();
            PresentViewController(new NavigationController(vc), true, null);

            var pa = await vc.Result;
            if (pa != null)
                recipientsView.AddRecipent(pa.Name, pa.Address);
        }

        async Task DoOpenShortcodes()
        {
            CommonConfig.UsageAnalytics.LogEvent(new ComposeContactPickerEvent(ContactPickerChoice.Shortcodes));

            var vc = new PickerShortcodesFoldersListViewController();
            PresentViewController(new NavigationController(vc), true, null);

            var sc = await vc.Result;
            if (sc != null && sc.Addresses != null && sc.Addresses.Any())
            {
                var addresses = sc.Addresses;
                toView.AddEmails(addresses.Where(da => da.Type == CommunicationAddressType.Email && da.AddressType == DocumentAddressType.To).Select(da => da.Address));
                ccView.AddEmails(addresses.Where(da => da.Type == CommunicationAddressType.Email && da.AddressType == DocumentAddressType.Cc).Select(da => da.Address));
                bccView.AddEmails(addresses.Where(da => da.Type == CommunicationAddressType.Email && da.AddressType == DocumentAddressType.Bcc).Select(da => da.Address));
            }
        }

        async Task DoOpenContacts(RecipientsView recipientsView)
        {
            CommonConfig.UsageAnalytics.LogEvent(new ComposeContactPickerEvent(ContactPickerChoice.Contacts));

            var vc = new PickerContactsFoldersListViewController();
            PresentViewController(new NavigationController(vc), true, null);

            var pa = await vc.Result;
            if (pa != null)
                recipientsView.AddRecipent(pa.Name, pa.Address);
        }

        async Task DoOpenRecents(RecipientsView recipientsView)
        {
            CommonConfig.UsageAnalytics.LogEvent(new ComposeContactPickerEvent(ContactPickerChoice.Recents));

            var vc = new RecentAddressesListViewController();
            PresentViewController(new NavigationController(vc), true, null);

            var pa = await vc.Result;
            if (pa != null)
                recipientsView.AddRecipent(pa.Name, pa.Address);
        }

        #endregion

        #region Attachments

        void InsertNewPhoto(PopoverPresentationControllerDelegate d)
        {
            var picker = new UIImagePickerController
            {
                AllowsEditing = false,
                SourceType = UIImagePickerControllerSourceType.Camera,
                CameraCaptureMode = UIImagePickerControllerCameraCaptureMode.Photo,
                CameraDevice = UIImagePickerControllerCameraDevice.Rear,
                Delegate = new ImagePickerControllerDelegate(this, HandleAttachmentImage),
                ModalPresentationStyle = UIModalPresentationStyle.PageSheet
            };
            if (picker.PopoverPresentationController != null)
                picker.PopoverPresentationController.Delegate = d;
            PresentViewController(picker, true, null);
        }

        void InsertExistingPhoto(PopoverPresentationControllerDelegate d)
        {
            var picker = new UIImagePickerController
            {
                AllowsEditing = false,
                SourceType = UIImagePickerControllerSourceType.SavedPhotosAlbum,
                MediaTypes = new[] { UTType.Image.ToString() },
                Delegate = new ImagePickerControllerDelegate(this, HandleAttachmentImage),
                ModalPresentationStyle = UIModalPresentationStyle.PageSheet
            };
            if (picker.PopoverPresentationController != null)
                picker.PopoverPresentationController.Delegate = d;
            PresentViewController(picker, true, null);
        }

        void InsertFile(PopoverPresentationControllerDelegate d)
        {
            var picker = new UIDocumentPickerViewController(new[]
            {
                "public.content",
                "public.data",
                "public.msg",
                "public.eml"
            }, UIDocumentPickerMode.Import)
            {
                Delegate = new DocumentMenuDelegate(this, HandleAttachmentUrl)
            };
            if (picker.PopoverPresentationController != null)
                picker.PopoverPresentationController.Delegate = d;
            PresentViewController(picker, true, null);
        }

        async void HandleAttachmentUrl(NSUrl url)
        {
            Stream stream = null;

            try
            {
                var filename = url.LastPathComponent;
                stream = new FileStream(url.Path, FileMode.Open, FileAccess.Read);
                var result = url.TryGetResource(NSUrl.FileSizeKey, out NSObject sizeObject, out NSError _error);

                if (!result)
                    throw new Exception(_error.ToString());

                var sizeInBytes = int.Parse(sizeObject.ToString());

                if (sizeInBytes > ServerConfig.SystemSettings.DocumentsModuleInfo.MaximumAttachmentSizeBytes)
                {
                    await Dialogs.ShowErrorAlertAsync(this, new Exception(Localization.GetString("attachment_too_big")));
                    return;
                }

                var file = await Managers.DocumentsManager.SaveDocumentWorkingCopyAttachmentAsync(filename, stream);
                attachmentsView.AddFileDescription(new FileDescription(file));
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Failed to save attachment [Url={url}, PreviousDocumentId={PreviousDocumentId}, PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={DocumentCreationModeFlag}]", ex);

                await Dialogs.ShowErrorAlertAsync(this, new Exception(Localization.GetString("error_saving_local_attachment")));
            }
            finally
            {
                stream?.Dispose();
            }
        }

        async void HandleAttachmentImage(string filename, NSData jpegData)
        {
            Stream stream = null;

            try
            {
                var sizeInBytes = (long)jpegData.Length;
                stream = jpegData.AsStream();

                if (sizeInBytes > ServerConfig.SystemSettings.DocumentsModuleInfo.MaximumAttachmentSizeBytes)
                {
                    await Dialogs.ShowErrorAlertAsync(this, new Exception(Localization.GetString("attachment_too_big")));
                    return;
                }

                var file = await Managers.DocumentsManager.SaveDocumentWorkingCopyAttachmentAsync(filename, stream);
                attachmentsView.AddFileDescription(new FileDescription(file));
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Failed to save image [FileName={filename}, PreviousDocumentId={PreviousDocumentId}, PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={DocumentCreationModeFlag}]", ex);

                await Dialogs.ShowErrorAlertAsync(this, new Exception(Localization.GetString("error_saving_local_attachment")));
            }
            finally
            {
                stream?.Dispose();
            }
        }

        #endregion

        #region Auto save

        async Task SaveWorkingCopy()
        {
            try
            {
                if (CommonConfig.Logger.IsDebugEnabled())
                    CommonConfig.Logger.Debug("Saving working copy...");

                await AsyncHelpers.InvokeOnMainThreadAsync(this, async () =>
                {
                    var subViews = headerStackView.Subviews.OfType<ComposeDocumentSubView>().ToArray();

                    foreach (var subView in subViews)
                        await subView.UpdateDocument();

                    document.HtmlBody = await GetContent();
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

        #region Utilities

        static bool ContainsValidEmails(params RecipientsView[] rvs)
        {
            var containsValidEmails = false;

            foreach (var rv in rvs)
                containsValidEmails |= !rv.Empty;

            return containsValidEmails;
        }

        static bool ContainsInvalidEmails(params RecipientsView[] rvs)
        {
            foreach (var rv in rvs)
                if (rv.ContainsInvalidEmail())
                    return true;

            return false;
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

        static string[] GetReplyHeaderParameters(DocumentPreview documentPreview)
        {
            var from = GetAddressTextFromPreviousDocument(documentPreview, DocumentAddressType.From);
            var date = documentPreview.DateReceivedTimestamp
                                      .ConvertTimestampMillisecondsToDateTime()
                                      .ConvertUtcToUserTime()
                                      .ConvertDateTimeToTimestampMilliseconds()
                                      .FormatUserTimestampAsTimeAndDateString();
            var to = GetAddressTextFromPreviousDocument(documentPreview, DocumentAddressType.To, DocumentAddressType.Cc);
            var subject = documentPreview.Subject;

            return new[] { from, date, to, subject };
        }

        static string GetAddressTextFromPreviousDocument(DocumentPreview documentPreview, params DocumentAddressType[] addressTypes)
        {
            var sb = new StringBuilder();
            var addresses = documentPreview.Addresses.Where(da => addressTypes.Contains(da.AddressType)).ToArray();
            for (var i = 0; i < addresses.Length; i++)
            {
                var hasName = !string.IsNullOrWhiteSpace(addresses[i].Name);
                if (hasName)
                    sb.Append(addresses[i].Name).Append(" &lt;");
                sb.Append(addresses[i].Address);
                if (hasName)
                    sb.Append("&gt;");
                if (i < addresses.Length - 1)
                    sb.Append(", ");
            }

            return sb.ToString();
        }

        class ImagePickerControllerDelegate : UIImagePickerControllerDelegate
        {
            readonly WeakReference<ComposeDocumentViewController> viewControllerWeakReference;
            readonly Action<string, NSData> handler;

            public ImagePickerControllerDelegate(ComposeDocumentViewController vc, Action<string, NSData> handler)
            {
                viewControllerWeakReference = vc.Wrap();
                this.handler = handler;
            }

            public override void FinishedPickingMedia(UIImagePickerController picker, NSDictionary info)
            {
                try
                {
                    NSData jpegImage;
                    using (var image = (UIImage)info[UIImagePickerController.OriginalImage])
                    {
                        jpegImage = image.AsJPEG();
                    }

                    string filename = null;
                    PHAsset asset = null;

                    if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
                        asset = (PHAsset)info[UIImagePickerController.PHAsset];
                    else
                    {
                        var referenceUrl = (NSUrl)info[UIImagePickerController.ReferenceUrl];
                        if (referenceUrl != null)
                        {
                            var results = PHAsset.FetchAssets(new[] { referenceUrl }, null);
                            asset = (PHAsset)results.firstObject;
                        }
                    }

                    if (asset != null)
                        filename = PHAssetResource.GetAssetResources(asset)[0].OriginalFilename;
                    else
                        filename = "photo_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".jpg";

                    picker.DismissViewController(true, null);

                    handler(filename, jpegImage);
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error("Could not pick media", ex);

                    var vc = viewControllerWeakReference.Unwrap();
                    if (vc != null)
                        Dialogs.ShowErrorAlert(vc, ex);

                    picker.DismissViewController(true, null);
                }
            }
        }

        class DocumentMenuDelegate : UIDocumentPickerDelegate, IUIDocumentPickerDelegate
        {
            readonly WeakReference<ComposeDocumentViewController> vcWeak;
            readonly Action<NSUrl> handler;

            public DocumentMenuDelegate(ComposeDocumentViewController vc, Action<NSUrl> handler)
            {
                vcWeak = vc.Wrap();
                this.handler = handler;
            }

            public override void DidPickDocument(UIDocumentPickerViewController controller, NSUrl url)
            {
                handler(url);
            }
        }

        #endregion

    }
}