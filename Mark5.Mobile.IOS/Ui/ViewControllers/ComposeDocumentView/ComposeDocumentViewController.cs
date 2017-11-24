using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
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

        UIBarButtonItem cancelButtonItem;
        UIBarButtonItem sendButtonItem;
        UIBarButtonItem attachmentButtonItem;

        UIStackView headerStackView;
        ToView toView;
        CcView ccView;
        BccView bccView;
        LineView lineView;
        PriorityView priorityView;
        SubjectView subjectView;
        AttachmentsView attachmentsView;

        UIDocumentInteractionController attachmentInteractionController;

        Worker autoSaveWorkingCopyWorker;

        #region UIViewController overrides

        public override void LoadView()
        {
            base.LoadView();

            InitNavigationBar();
            InitializeView();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ExtendedLayoutIncludesOpaqueBars = true;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            InitializeHandlers();
        }

        public override async void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info("Appeared");

            await RefreshData();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeInitializeHandlers();
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
            attachmentButtonItem = null;

            headerStackView?.RemoveFromSuperview();

            toView?.RemoveFromSuperview();
            ccView?.RemoveFromSuperview();
            bccView?.RemoveFromSuperview();
            lineView?.RemoveFromSuperview();
            priorityView?.RemoveFromSuperview();
            subjectView?.RemoveFromSuperview();
            attachmentsView?.RemoveFromSuperview();

            headerStackView = null;
            toView = null;
            ccView = null;
            bccView = null;
            lineView = null;
            priorityView = null;
            subjectView = null;
            attachmentsView = null;
            attachmentInteractionController = null;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (CommonConfig.Logger.IsDebugEnabled())
                CommonConfig.Logger.Debug("Disposed");
        }

        #endregion

        #region Init methods

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
            attachmentButtonItem = new UIBarButtonItem
            {
                Image = UIImage.FromBundle(Path.Combine("icons", "attachment.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
            };

            NavigationItem.SetLeftBarButtonItem(cancelButtonItem, false);
            NavigationItem.SetRightBarButtonItems(new[] { sendButtonItem, attachmentButtonItem }, false);
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
            };

            headerStackView.AddArrangedSubview(toView = new ToView());
            headerStackView.AddArrangedSubview(ccView = new CcView());
            headerStackView.AddArrangedSubview(bccView = new BccView());
            headerStackView.AddArrangedSubview(lineView = new LineView(this));
            if (PlatformConfig.Preferences.ComposePriorityEnabled)
                headerStackView.AddArrangedSubview(priorityView = new PriorityView(this));
            headerStackView.AddArrangedSubview(subjectView = new SubjectView());
            headerStackView.AddArrangedSubview(attachmentsView = new AttachmentsView());

            SetHeaderView(headerStackView);
        }

        void InitializeHandlers()
        {
            cancelButtonItem.Clicked += CancelButtonItem_Clicked;
            sendButtonItem.Clicked += SendButtonItem_Clicked;
            attachmentButtonItem.Clicked += AttachmentButtonItem_Clicked;

            toView.AddButtonTapped += RecipientView_AddButtonTapped;
            toView.Edited += Subview_Edited;

            ccView.AddButtonTapped += RecipientView_AddButtonTapped;
            ccView.Edited += Subview_Edited;

            bccView.AddButtonTapped += RecipientView_AddButtonTapped;
            bccView.Edited += Subview_Edited;

            lineView.Edited += Subview_Edited;

            subjectView.Edited += Subview_Edited;

            attachmentsView.Tapped += AttachmentsView_Tapped;
            attachmentsView.DeleteTapped += AttachmentsView_DeleteTapped;
        }

        void DeInitializeHandlers()
        {
            cancelButtonItem.Clicked -= CancelButtonItem_Clicked;
            sendButtonItem.Clicked -= SendButtonItem_Clicked;
            attachmentButtonItem.Clicked -= AttachmentButtonItem_Clicked;

            toView.AddButtonTapped -= RecipientView_AddButtonTapped;
            toView.Edited -= Subview_Edited;

            ccView.AddButtonTapped -= RecipientView_AddButtonTapped;
            ccView.Edited -= Subview_Edited;

            bccView.AddButtonTapped -= RecipientView_AddButtonTapped;
            bccView.Edited -= Subview_Edited;

            lineView.Edited -= Subview_Edited;

            subjectView.Edited -= Subview_Edited;

            attachmentsView.Tapped -= AttachmentsView_Tapped;
            attachmentsView.DeleteTapped -= AttachmentsView_DeleteTapped;
        }

        #endregion

        async Task RefreshData()
        {
            if (documentLoaded)
                return;

            documentLoaded = true;

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

                var files = await Managers.DocumentsManager.GetDocumentWorkingCopyAttachmentsAsync();
                attachmentsView.InitializeFileDescriptions(files.Select(f => new FileDescription(f)).ToArray());

                sendButtonItem.Enabled = IsFormValid();

                LoadEditor();

                if (!RestoreWorkingCopy)
                    await AskIfShouldUseTemplates();

                await EndRefreshing();

                autoSaveWorkingCopyWorker?.Stop();
                autoSaveWorkingCopyWorker = new Worker(SaveWorkingCopy, AutoSaveWorkingCopyInterval);
                autoSaveWorkingCopyWorker.Start();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Failed to load editor", ex);

                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        bool IsFormValid()
        {
            var recipientAdded = false;
            foreach (var recipientView in new RecipientsView[] { toView, ccView, bccView })
                recipientAdded |= !recipientView.Empty;

            return recipientAdded && !lineView.LineSelectedIsAmbiguous;
        }

        #region NavigationBar Event Handlers

        async void AttachmentButtonItem_Clicked(object sender, EventArgs e)
        {
            var d = new PopoverPresentationControllerDelegate((UIBarButtonItem)sender);
            var source = await Dialogs.ShowListActionSheetAsync(this, new[] { Localization.GetString("take_photo"), Localization.GetString("existing_photo"), Localization.GetString("browse_files") }, d);
            if (source < 0)
                return;

            if (source == 0)
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

            if (source == 1)
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

            if (source == 2)
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

        async void SendButtonItem_Clicked(object sender, EventArgs e)
        {
            await SendDocument(false);
        }

        void CancelButtonItem_Clicked(object sender, EventArgs e)
        {
            var actionSheet = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            actionSheet.AddAction(UIAlertAction.Create(Localization.GetString("save_draft"), UIAlertActionStyle.Default, async a => await SendDocument(true)));
            actionSheet.AddAction(UIAlertAction.Create(Localization.GetString("delete_draft"), UIAlertActionStyle.Destructive, async a => await DiscardAndCloseComposeViewController()));
            actionSheet.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, null));
            if (actionSheet.PopoverPresentationController != null)
                actionSheet.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate((UIBarButtonItem)sender);
            PresentViewController(actionSheet, true, null);
        }

        async Task DiscardAndCloseComposeViewController()
        {
            autoSaveWorkingCopyWorker?.Stop();
            await autoSaveWorkingCopyWorker?.Finished();
            await Managers.DocumentsManager.DeleteDocumentWorkingCopyAsync();

            if (PresentingViewController == null)
                NavigationController?.PopViewController(true);
            else
                DismissViewController(true, null);
        }

        #endregion

        #region Actions

        async Task SendDocument(bool saveDraft)
        {
            var containsInvalidEmails = false;
            foreach (var recipientView in new RecipientsView[] { toView, ccView, bccView })
                containsInvalidEmails |= recipientView.ContainsInvalidEmail();

            if (containsInvalidEmails)
            {
                var result = await Dialogs.ShowYesNoAlertAsync(this, Localization.GetString("warning"), Localization.GetString("incorrect_email_addresses"));
                if (!result)
                    return;
            }

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(saveDraft ? Localization.GetString("saving_draft___") : Localization.GetString("sending_document___"));

            try
            {
                if (autoSaveWorkingCopyWorker != null)
                {
                    autoSaveWorkingCopyWorker.Stop();
                    await autoSaveWorkingCopyWorker.Finished();
                }

                var subViews = headerStackView.Subviews.OfType<ComposeDocumentSubView>().ToArray();
                foreach (var subView in subViews)
                    await subView.UpdateDocument();

                documentPreview.Direction = saveDraft ? DocumentDirection.Draft : DocumentDirection.Outgoing;

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

                if (previousDocumentPreview?.Direction == DocumentDirection.Draft)
                    CommonConfig.MessengerHub.PublishAsync(new DraftSentMessage(this, previousDocumentPreview.Id));

                dismissAction();

                if (PresentingViewController == null)
                    NavigationController?.PopViewController(true);
                else
                    DismissViewController(true, null);
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Failed to queue document for upload [saveDraft={saveDraft}, PreviousDocumentId={PreviousDocumentId}, PreviousDocumentFolderId={PreviousDocumentFolderId}, CreationModeFlag={DocumentCreationModeFlag}] ", ex);

                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        async Task SaveWorkingCopy()
        {
            try
            {
                if (CommonConfig.Logger.IsDebugEnabled())
                    CommonConfig.Logger.Debug("Saving working copy...");

                ComposeDocumentSubView[] subViews = null;

                InvokeOnMainThread(() => subViews = headerStackView.Subviews.OfType<ComposeDocumentSubView>().ToArray());

                foreach (var subView in subViews)
                    await subView.UpdateDocument();

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

        #region Subviews Event Handlers

        void Subview_Edited(object sender, EventArgs e)
        {
            Title = !subjectView.Empty ? subjectView.Subject : DefaultTitle;
            sendButtonItem.Enabled = IsFormValid();

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

        async void RecipientView_AddButtonTapped(object sender, EventArgs e)
        {
            var strings = new[] { Localization.GetString("contact_picker_recent_addresses"),
                Localization.GetString("contact_picker_contacts"),
                Localization.GetString("contact_picker_shortcodes"),
                Localization.GetString("contact_picker_phonebook"),
            };

            var choice = await Dialogs.ShowListActionSheetAsync(this, strings, sender as UIView);

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

            sendButtonItem.Enabled = IsFormValid();
        }

        async void AttachmentsView_Tapped(object sender, AttachmentsView.TappedEventArgs e)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("opening_attachment___"));

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
                }
                else
                {
                    attachmentInteractionController = UIDocumentInteractionController.FromUrl(url);
                    attachmentInteractionController.Delegate = new DocumentInteractionControllerDelegate(this);

                    var previewSuccessful = attachmentInteractionController.PresentPreview(true);

                    if (!previewSuccessful)
                    {
                        CommonConfig.Logger.Info($"Failed to present preview for attachment. Presenting open with instead [documentId={document.Id}, previousDocumentId={previousDocument.Id}, e.AttachmentDescription.Name={e.AttachmentDescription?.Name}, e.FileDescription.Name={e.FileDescription?.Name}]");

                        var openInSuccessful = attachmentInteractionController.PresentOptionsMenu(View.Frame, View, true);
                        if (!openInSuccessful)
                        {
                            CommonConfig.Logger.Warning($"Failed to present open in view - there is no app that can open this type of attachment installed [documentId={document.Id}, previousDocumentId={previousDocument.Id}, e.AttachmentDescription.Name={e.AttachmentDescription?.Name}, e.FileDescription.Name={e.FileDescription?.Name}]");

                            await Dialogs.ShowConfirmAlertAsync(this, Localization.GetString("cannot_open_attachment_title"), Localization.GetString("cannot_open_attachment_content"));
                        }
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

        #region Picker methods

        async Task DoOpenPhonebook(RecipientsView recipientsView)
        {
            var vc = new PhonebookContactsListViewController();
            PresentViewController(new NavigationController(vc), true, null);

            var pa = await vc.Result;
            if (pa != null)
                recipientsView.AddRecipent(pa.Name, pa.Address);
        }

        async Task DoOpenShortcodes()
        {
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
            var vc = new PickerContactsFoldersListViewController();
            PresentViewController(new NavigationController(vc), true, null);

            var pa = await vc.Result;
            if (pa != null)
                recipientsView.AddRecipent(pa.Name, pa.Address);
        }

        async Task DoOpenRecents(RecipientsView recipientsView)
        {
            var vc = new RecentAddressesListViewController();
            PresentViewController(new NavigationController(vc), true, null);

            var pa = await vc.Result;
            if (pa != null)
                recipientsView.AddRecipent(pa.Name, pa.Address);
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
                CommonConfig.Logger.Info("Documeny copied from new with text and attachments, no need to have templates");
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
                var templateListStrings = new string[]
                {
                    Localization.GetString("template_selection_default"),
                    Localization.GetString("template_selection_local"),
                    Localization.GetString("template_selection_another")
                };
                //var result = await Dialogs.ShowListActionSheetAsync(this, templateListStrings, contentView);
                var result = -1;
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
            var tp = new TemplatesListViewController();
            PresentViewController(new NavigationController(tp, UIModalPresentationStyle.PageSheet), true, null);

            var templatePreview = await tp.Result;
            if (templatePreview != null)
                await GetTemplate(templatePreview);
        }

        async Task GetLocalTemplate()
        {
            var localTemplate = PlatformConfig.Preferences.LocalTemplate;
            //await contentView.InsertLocalTemplate(localTemplate);
        }

        async Task GetDefaultTemplate(bool errorMessageIfNull = false)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("loading_template___"));

            try
            {
                var template = await Managers.DocumentsManager.GetDefaultTemplateAsync(DocumentCreationModeFlag);
                if (template != null)
                    await ApplyTemplate(template);
                else if (errorMessageIfNull)
                    throw new Exception(Localization.GetString("template_null"));
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

        async Task GetTemplate(TemplatePreview templatePreview)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("loading_template___"));

            try
            {
                var template = await Managers.DocumentsManager.GetTemplateAsync(templatePreview.Id);
                if (template != null)
                    await ApplyTemplate(template);
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

        async Task ApplyTemplate(Template template)
        {
            ProcessTemplate(template);

            //await contentView.InsertTemplate(template);

            if (!string.IsNullOrEmpty(template.Subject))
                subjectView.Subject = template.Subject;

            lineView.SetLineFromGuid(template.LineGuid);
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

                    PHAsset asset = null;

                    if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
                    {
                        asset = (PHAsset)info[UIImagePickerController.PHAsset];
                    }
                    else
                    {
                        var referenceUrl = (NSUrl)info[UIImagePickerController.ReferenceUrl];

                        if (referenceUrl != null)
                        {
                            var results = PHAsset.FetchAssets(new[]
                                {
                                referenceUrl
                            },
                                null);
                            asset = (PHAsset)results.firstObject;
                        }
                    }

                    string filename = null;

                    if (asset != null)
                    {
                        var assetResources = PHAssetResource.GetAssetResources(asset);
                        filename = assetResources[0].OriginalFilename;
                    }
                    else
                    {
                        filename = "photo_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".jpg";
                    }

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
    }
}