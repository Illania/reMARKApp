using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Foundation;
using reMark.Mobile.Classes.Enum;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Manager;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Model.HubMessages;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.Common.Utilities.Extensions;
using reMark.Mobile.IOS.Model;
using reMark.Mobile.IOS.Model.HubMessages;
using reMark.Mobile.IOS.Ui.Common;
using reMark.Mobile.IOS.Ui.ViewControllers.ComposeDocumentView;
using reMark.Mobile.IOS.Ui.ViewControllers.DocumentView;
using reMark.Mobile.IOS.Ui.ViewControllers.DocumentView.HeaderView;
using reMark.Mobile.IOS.Ui.ViewControllers.FoldersList;
using reMark.Mobile.IOS.Utilities;
using TinyMessenger;
using UIKit;
using WebKit;

namespace reMark.Mobile.IOS.Ui.ViewControllers
{
    public class DocumentViewController : AbstractWebViewController, ISecondaryViewController, IUIViewControllerRestoration
    {
        const int LargeAttachmentSizeInBytes = 20 * 1024 * 1024; // 20MB

        public bool Empty => document == null && documentPreview == null && folderId == null && folder == null && documentId == null;

        public DocumentPreview DocumentPreview => documentPreview;

        WeakReference<IDocumentPageViewControllerDelegate> weakRefDocumentPageViewControllerDelegate;

        public IDocumentPageViewControllerDelegate DocumentPageViewControllerDelegate
        {
            get => weakRefDocumentPageViewControllerDelegate?.Unwrap();
            set => weakRefDocumentPageViewControllerDelegate = value.Wrap();
        }
       
        Guid failedDocumentToUploadGuid;
        int? folderId;
        Folder folder;
        int? documentId;
        DocumentPreview documentPreview;
        Document document;
        Guid notificationGuid;

        UIBarButtonItem doneButtonItem;
        UIBarButtonItem editDocumentButtonItem;

        HeaderView headerView;

        UIBarButtonItem flagButton;
        UIBarButtonItem fileToButton;
        UIButtonScalable commentsButton;
        BadgeBarButtonItem commentsBadgeButton;
        UIBarButtonItem replyActionsButton;
        UIBarButtonItem userActionsButton;

        CancellationTokenSource readStatusCts;
        CancellationTokenSource loadCts;

        bool refreshDataOnAppear;
        bool hideDoneButton;
        bool forceShowActionBar;
        bool finishedLoading;

        TinyMessageSubscriptionToken readStatusChangedToken;
        TinyMessageSubscriptionToken draftSentToken;
        TinyMessageSubscriptionToken commentsCountChangedToken;

        // EventHandlers for IPad
        public EventHandler FlagClicked => FlagButton_Clicked;
        public EventHandler ReplyClicked => ReplyActionsButton_Clicked;
        public EventHandler FileToClicked => FileToButton_Clicked;
        public EventHandler CommentsClicked => CommentsButton_Clicked;
        public EventHandler UserActionsClicked => UserActionsButton_Clicked;

        public DocumentViewController(bool forceShowActionBar = false)
        {
            HidesBottomBarWhenPushed = true;
            this.forceShowActionBar = forceShowActionBar;
        }

        #region UIViewController overrides

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            InitNavigationBar();
            InitHeaderView();

            if (forceShowActionBar || !Integration.IsIPad())
                InitToolbar();

            if (Integration.IsRunningAtLeast(11))
                NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Never;

            RestorationIdentifier = nameof(DocumentViewController);
            RestorationClass = Class;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            InitializeHandlers();
            SubscribeToMessages();

            if (NavigationController != null && !(ParentViewController is DocumentPageViewController))
                NavigationController.ToolbarHidden = false;

            if (!forceShowActionBar && Integration.IsIPad())
                NavigationController.ToolbarHidden = true;

            if(!Integration.IsIPad())
                SendStatusBanner.Attach(this);
        }

        public override async void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info("Appeared");

            if (IsRefreshing)
            {
                Clear();
                refreshDataOnAppear = true;
            }

            if (refreshDataOnAppear)
            {
                refreshDataOnAppear = false;
                RefreshData();
            }
            else
                MarkAsReadIfNecessary();

            if (PlatformConfig.Preferences.SyncUserActivities)
            {
                await Managers.DocumentsManager
                    .ExecuteUserActivity(UserActivityType.Open, documentPreview, null);
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            loadCts?.Cancel();
            loadCts = null;

            readStatusCts?.Cancel();
            readStatusCts = null;

            DeinitializeHandlers();
            UnsubscribeFromMessages();

            if (NavigationController != null && ParentViewController is not DocumentPageViewController)
                NavigationController.ToolbarHidden = true;

            SendStatusBanner.Detach(this);
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning("Received memory warning!");

            UnsubscribeFromMessages();

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        protected override void Recycle()
        {
            base.Recycle();

            doneButtonItem = null;
            editDocumentButtonItem = null;

            headerView.RemoveFromSuperview();
            headerView = null;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (CommonConfig.Logger.IsDebugEnabled())
                CommonConfig.Logger.Debug("Disposed");
        }

        #endregion

        #region Init methods

        private void InitNavigationBar()
        {
            editDocumentButtonItem = new UIBarButtonItem
            {
                Image = UIImage.FromBundle("Edit")
            };

            if (PresentingViewController == null || hideDoneButton) 
                return;
            
            doneButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Done);
            NavigationItem.SetRightBarButtonItem(doneButtonItem, false);
        }

        private void InitHeaderView()
        {
            SetHeaderView(headerView = new HeaderView());
        }

        private void InitToolbar()
        {
            commentsButton = new UIButtonScalable
            {
                Frame = new CoreGraphics.CGRect(0f, 0f, 25f, 25f),
                Enabled = false,
            };
            commentsButton.SetImage(
                UIImage.FromBundle("Comments")
                    ?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);

            ToolbarItems = new[]
            {
                flagButton = new UIBarButtonItem
                {
                    Image = UIImage.FromBundle("Flag"),
                    Enabled = false
                },
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                replyActionsButton = new UIBarButtonItem
                {
                    Image = UIImage.FromBundle("Reply"),
                    Enabled = false
                },
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                fileToButton = new UIBarButtonItem
                {
                    Image = UIImage.FromBundle("Worktray"),
                    Enabled = false
                },
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                commentsBadgeButton = new BadgeBarButtonItem(commentsButton)
                {
                    Enabled = false
                },
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                userActionsButton = new UIBarButtonItem
                {
                    Image = UIImage.FromBundle("Actions"),
                    Enabled = false
                }
            };
        }

        private void InitializeHandlers()
        {
            if (headerView != null)
            {
                headerView.RecipientTapped += HeaderView_RecipientTapped;
                headerView.AttachmentTapped += HeaderView_AttachmentTapped;

                headerView.BeginAnimating += HeaderView_BeginAnimating;
                headerView.Animating += HeaderView_Animating;
                headerView.EndAnimating += HeaderView_EndAnimating;
                headerView.AppointmentReplyTapped += HeaderView_AppointmentReplyTapped;
            }

            if (flagButton != null)
                flagButton.Clicked += FlagButton_Clicked;
            if (fileToButton != null)
                fileToButton.Clicked += FileToButton_Clicked;
            if (replyActionsButton != null)
                replyActionsButton.Clicked += ReplyActionsButton_Clicked;
            if (commentsButton != null)
                commentsButton.TouchUpInside += CommentsButton_Clicked;
            if (userActionsButton != null)
                userActionsButton.Clicked += UserActionsButton_Clicked;

            if (doneButtonItem != null)
                doneButtonItem.Clicked += DoneButtonItem_Clicked;

            if (editDocumentButtonItem != null)
                editDocumentButtonItem.Clicked += EditDocumentButtonItem_Clicked;
        }

        private async void HeaderView_AppointmentReplyTapped(object sender, EventArgs e)
        {
            var invitation = document.Invitations.First();
            var line = LineUtilities.GetLineForCreationModeFlag(DocumentCreationModeFlag.Reply, document, PlatformConfig.Preferences.AlwaysUseDefaultLine);
            var appointmentReplyVc = new InvitationReplyViewController(invitation.Status, line)
            {
                ModalPresentationStyle = UIModalPresentationStyle.OverFullScreen,
                ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve
            };

            NavigationController?.PresentViewController(appointmentReplyVc, true, null);

            var detailsModel = await appointmentReplyVc.Result;

            if (detailsModel != null)
                await SendInvitationReply(sender as CalendarInvitationView, invitation, detailsModel);
        }

        private void DeinitializeHandlers()
        {
            if (headerView != null)
            {
                headerView.RecipientTapped -= HeaderView_RecipientTapped;
                headerView.AttachmentTapped -= HeaderView_AttachmentTapped;

                headerView.BeginAnimating -= HeaderView_BeginAnimating;
                headerView.Animating -= HeaderView_Animating;
                headerView.EndAnimating -= HeaderView_EndAnimating;
            }

            if (flagButton != null)
                flagButton.Clicked -= FlagButton_Clicked;
            if (fileToButton != null)
                fileToButton.Clicked -= FileToButton_Clicked;
            if (replyActionsButton != null)
                replyActionsButton.Clicked -= ReplyActionsButton_Clicked;
            if (commentsButton != null)
                commentsButton.TouchUpInside -= CommentsButton_Clicked;
            if (userActionsButton != null)
                userActionsButton.Clicked -= UserActionsButton_Clicked;

            if (doneButtonItem != null)
                doneButtonItem.Clicked -= DoneButtonItem_Clicked;

            if (editDocumentButtonItem != null)
                editDocumentButtonItem.Clicked -= EditDocumentButtonItem_Clicked;
        }

        private void SubscribeToMessages()
        {
            readStatusChangedToken = CommonConfig.MessengerHub.Subscribe<DocumentPreviewReadStatusChangedMessage>(
                ReadStatusChangedHandler, m => m.DocumentPreviewId == document?.Id);
            
            draftSentToken = CommonConfig.MessengerHub.Subscribe<DraftSentMessage>(
                DraftSentHandler, m => m.DocumentId == document?.Id);
            
            commentsCountChangedToken = CommonConfig.MessengerHub.Subscribe<EntityPreviewCommentCountChangedMessage>(
                CommentsCountChangedHandler, m => m.EntityId == document?.Id);
        }

        private void UnsubscribeFromMessages()
        {
            readStatusChangedToken?.Dispose();
            draftSentToken?.Dispose();
        }

        public void SetData(Folder folderCurrent, DocumentPreview documentPreviewCurrent)
        {
            CommonConfig.UsageAnalytics.LogEvent(new OpenDocumentEvent(
                documentPreviewCurrent?.Direction == DocumentDirection.External));

            failedDocumentToUploadGuid = Guid.Empty;
            document = null;
            documentId = null;
            folderId = null;
            notificationGuid = default;

            documentPreview = documentPreviewCurrent;
            folder = folderCurrent;
        }

        public void SetData(DocumentPreview documentPreviewCurrent, bool hideDoneButtonCurrent)
        {
            CommonConfig.UsageAnalytics.LogEvent(new OpenDocumentEvent(
                documentPreview?.Direction == DocumentDirection.External));

            failedDocumentToUploadGuid = Guid.Empty;
            folder = null;
            document = null;
            documentId = null;
            folderId = null;
            notificationGuid = default;

            documentPreview = documentPreviewCurrent;
            hideDoneButton = hideDoneButtonCurrent;
        }

        public void SetData(int documentIdCurrent)
        {
            CommonConfig.UsageAnalytics.LogEvent(new OpenDocumentEvent(false));

            failedDocumentToUploadGuid = Guid.Empty;
            document = null;
            folder = null;
            folderId = null;
            documentPreview = null;
            notificationGuid = default;

            documentId = documentIdCurrent;
        }

        public void SetData(int documentIdCurrent, Guid notification)
        {
            CommonConfig.UsageAnalytics.LogEvent(new OpenDocumentEvent(false));

            failedDocumentToUploadGuid = Guid.Empty;
            document = null;
            folder = null;
            folderId = null;
            documentPreview = null;
            documentId = documentIdCurrent;
            notificationGuid = notification;
        }

        public void SetData(Guid failedDocumentToUpload)
        {
            CommonConfig.UsageAnalytics.LogEvent(new OpenDocumentEvent(false));

            document = null;
            documentPreview = null;
            folder = null;
            documentId = null;
            notificationGuid = default;

            failedDocumentToUploadGuid = failedDocumentToUpload;
        }

        private void ClearData()
        {
            loadCts?.Cancel();

            failedDocumentToUploadGuid = Guid.Empty;
            document = null;
            documentPreview = null;
            folder = null;
            documentId = null;
            notificationGuid = default;

            if (flagButton != null)
                flagButton.Enabled = false;

            if (fileToButton != null)
                fileToButton.Enabled = false;

            if (replyActionsButton != null)
                replyActionsButton.Enabled = false;

            if (commentsBadgeButton != null)
            {
                commentsBadgeButton.SetBadgeValue("0", false);
                commentsBadgeButton.Enabled = false;
            }

            if (commentsButton != null)
                commentsButton.Enabled = false;

            if (userActionsButton != null)
                userActionsButton.Enabled = false;

            if (Integration.IsIPad())
                DocumentPageViewControllerDelegate?.UpdateIPadNavigationButtons(false, "0");

            Clear();
        }

        public void SetRefreshDataOnAppear()
        {
            refreshDataOnAppear = true;
        }

        #endregion

        #region Refresh methods

        private async void RefreshData()
        {
            loadCts?.Cancel();
            loadCts = new CancellationTokenSource();
            var token = loadCts.Token;

            try
            {
                await StartRefreshing();

                if (notificationGuid != default(Guid))
                    await Managers.NotificationsManager.MarkAsRead(notificationGuid);

                if (failedDocumentToUploadGuid != Guid.Empty)
                {
                    (documentPreview, document) = await Managers.DocumentsManager.GetFailedDocumentToUpload(failedDocumentToUploadGuid);
                    documentId = document.Id;
                }

                if (documentId.HasValue && documentPreview == null && document == null)
                {
                    var documentContainer = await Managers.DocumentsManager.GetDocumentWithPreviewAsync(folderId ?? folder?.Id, documentId.Value);
                    documentPreview = documentContainer.DocumentPreview;
                    document = documentContainer.Document;
                }

                if (documentPreview != null && document == null)
                    document = await Managers.DocumentsManager.GetDocumentAsync(folderId ?? folder?.Id, documentPreview.Id);

                if (token.IsCancellationRequested)
                    return;

                headerView.Document = document;
                headerView.DocumentPreview = DocumentPreview;
                headerView.RefreshHeader();

                if (document != null)
                {
                    if (!string.IsNullOrWhiteSpace(document.HtmlBody))
                        await LoadHtmlString(document.HtmlBody, HtmlProcessingConfiguration.DefaultForViewing);
                    else if (!string.IsNullOrWhiteSpace(document.PlainTextBody))
                        await LoadPlainText(document.PlainTextBody, PlainTextProcessingConfiguration.DefaultForViewing);
                    else
                        LoadNoContentString();
                }
                else
                    LoadEmpty();

                await EndRefreshing();

                RefreshNavigationBar();
                RefreshToolbar();

                MarkAsReadIfNecessary();

            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Downloading document failed [folder.name={folder?.Name}, folder.id={folderId ?? folder?.Id}, documentId={documentId ?? documentPreview?.Id}]", ex);

                await Dialogs.ShowErrorAlertAsync(this, ex);

                if (SplitViewController == null || SplitViewController.Collapsed)
                {
                    if (PresentingViewController == null)
                        NavigationController?.PopViewController(true);
                    else
                        DismissViewController(true, null);
                }
                else
                {
                    ClearData();
                }
            }
        }

        void MarkAsReadIfNecessary()
        {
            if (document == null || documentPreview == null)
                return;

            readStatusCts?.Cancel();
            readStatusCts = new CancellationTokenSource();

            Task.Run(async () =>
            {
                var f = folder;
                var d = document;
                var dp = documentPreview;
                var token = readStatusCts.Token;

                try
                {
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
                        await Managers.DocumentsManager.ExecuteUserActivity(Mobile.Common.Model.UserActivityType.Read, DocumentPreview, null);
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error($"Marking document as read failed [folder.name={f?.Name}, folder.id={f?.Id}, documentPreviewId={dp?.Id}]", ex);
                }
            });
        }

        private void RefreshNavigationBar()
        {
            if (document != null && documentPreview.Direction == DocumentDirection.Draft)
                NavigationItem.SetRightBarButtonItem(editDocumentButtonItem, true);
        }

        public void RefreshToolbar()
        {
            var enableBottomActions = failedDocumentToUploadGuid == Guid.Empty;
            if (!enableBottomActions) 
                return;
            
            if (!forceShowActionBar && Integration.IsIPad())
            {
                DocumentPageViewControllerDelegate?.UpdateIPadNavigationButtons(document != null, document?.Comments?.Count.ToString());
            }
            else
            {
                flagButton.Enabled = document != null;
                fileToButton.Enabled = document != null;
                replyActionsButton.Enabled = document != null;
                commentsBadgeButton.BadgeValue = document?.Comments?.Count.ToString();
                commentsBadgeButton.Enabled = document != null;
                commentsButton.Enabled = document != null;
                userActionsButton.Enabled = document != null;
            }
        }

        #endregion

        #region Subviews event handlers

        private async void HeaderView_AttachmentTapped(object sender, AttachmentButtonTappedEventArgs e)
        {
            var attachmentDescription = e.Attachment;
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("opening_attachment___"));

            CommonConfig.UsageAnalytics.LogEvent(new DocumentOpenAttachmentEvent());

            try
            {
                if (!Integration.IsIPad())
                {
                    var path = await Managers.DocumentsManager
                        .GetAttachmentAsync(attachmentDescription, document, false, SourceType.Local);

                    if (string.IsNullOrWhiteSpace(path))
                    {
                        if (PlatformConfig.Preferences.LargeAttachmentWarning
                            && attachmentDescription.SizeInBytes > LargeAttachmentSizeInBytes
                            && !await Dialogs.ShowYesNoAlertAsync(this, 
                                Localization.GetString("warning"), 
                                string.Format(Localization.GetString("big_attachment_warning"), 
                                    UI.PrettyFileSize(attachmentDescription.SizeInBytes))))
                        {
                            dismissAction();
                            return;
                        }

                        path = await Managers.DocumentsManager.GetAttachmentAsync(attachmentDescription, document, false, SourceType.Remote);
                    }

                    if (string.IsNullOrWhiteSpace(path))
                        throw new Exception("Unable to open attachment.");

                    await AttachmentsUtilities.OpenAttachment(path, this, document.Id, attachmentDescription?.Name ?? string.Empty);
                }
                else
                {
                    var currentItemIndex = document.Attachments.IndexOf(attachmentDescription);
                    var attachmentsList = await Managers.DocumentsManager.GetAttachmentsAsync(document);
                    AttachmentsUtilities.OpenAttachmentsInQuickLook(attachmentsList, currentItemIndex, this);
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Failed to view attachment [document.Id={document.Id}, attachment.Name={attachmentDescription?.Name}", ex);

                dismissAction();
                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
            finally
            {
                dismissAction();
            }
        }

        private async void HeaderView_RecipientTapped(object sender, RecipientTappedEventArgs e)
        {
            await ShowMenuForRecipientTappedAction(sender, e.Recipent);
        }

        private async Task ShowMenuForRecipientTappedAction(object sender, string recipient )
        {
            var d = new PopoverPresentationControllerDelegate(doneButtonItem);
            var source = await Dialogs.ShowListActionSheetAsync(this, new[]
            {
                Localization.GetString("new_document"), Localization.GetString("new_contact")
            }, d);
            if (source < 0)
                return;

            if (source == 0)
            {
                PresentComposeViewWithPreconfiguredFields(preconfiguredToAddresses: new[] { recipient });
            }

            if (source == 1)
                PresentContactViewWithPreconfiguredFields(recipient);
        }

        private void PresentComposeViewWithPreconfiguredFields(string subject = null, string body = null,
            string[] preconfiguredToAddresses = null, string[] preconfiguredCcAddresses = null, string[] preconfiguredBccAddresses = null)
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

            var vc = new ComposeDocumentViewController
            {
                DocumentCreationModeFlag = DocumentCreationModeFlag.New,
                PreconfiguredEmailAddresses = preconfiguredAddresses,
                PreconfiguredContent = body,
                PreconfiguredSubject = subject,
            };

            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }


        private static DocumentAddress GetEmail(string email) => 
            Validator.ContainsValidEmails(email, out var addresses)
                ? addresses.First() 
                : new DocumentAddress();

        private async void PresentContactViewWithPreconfiguredFields(string preconfiguredEmailAddress)
        {
            var choice = await Dialogs.ShowListActionSheetAsync(this, new[] 
            {
                Localization.GetString("add_company"),
                Localization.GetString("add_department"),
                Localization.GetString("add_person") 
            });

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

            var vc = new AddEditContactViewController
            {
                CreationModeFlag = ContactCreationModeFlag.New,
                ContactType = type,
                PreconfiguredEmailAddress = GetEmail(preconfiguredEmailAddress)
            };
            PresentViewController(new NavigationController(vc),true, null);
         }

        protected override void OnWebViewLoaded()
        {
            base.OnWebViewLoaded();
            DocumentPageViewControllerDelegate?.AddViewControllerToCache(this);
        }
        
        #endregion

        #region Toolbar event handlers

        private async void FlagButton_Clicked(object sender, EventArgs e)
        {
            var isRead = documentPreview.IsReadByCurrent;
            var flagListStrings = new []
            {
                Localization.GetString(isRead ? "mark_as_unread" : "mark_as_read"),
                Localization.GetString("categories")
            };

            var result = await Dialogs.ShowListActionSheetAsync(this, flagListStrings, (UIBarButtonItem)sender);

            if (result < 0)
                return;

            switch (result)
            {
                case 0:
                    await DoChangeReadStatus(isRead);
                    break;
                case 1:
                    DoAssignCategory();
                    break;
            }
        }

        private async void ReplyActionsButton_Clicked(object sender, EventArgs e)
        {
            var replyListStrings = new []
            {
                Localization.GetString("reply"),
                Localization.GetString("reply_all"),
                Localization.GetString("forward"),
                Localization.GetString("copy_to_new")};

            var result = await Dialogs.ShowListActionSheetAsync(this, replyListStrings, (UIBarButtonItem)sender);

            if (result < 0)
                return;

            switch (result)
            {
                case 0:
                    DoRespond(DocumentCreationModeFlag.Reply);
                    break;
                case 1:
                    DoRespond(DocumentCreationModeFlag.ReplyAll);
                    break;
                case 2:
                    DoRespond(DocumentCreationModeFlag.Forward);
                    break;
                case 3:
                    CopyToNew();
                    break;
            }
        }

        private async void ShowPriorityActionSheet(UIBarButtonItem barButtonItem)
        {
            var priorities = new List<Priority>
            {
                Priority.Low,
                Priority.Normal,
                Priority.Urgent
            };
            var priorityStrings = priorities.Select(UI.PrettyPriorityString);
            var result = await Dialogs.ShowListActionSheetAsync(this, priorityStrings.ToArray(), barButtonItem);

            if (result < 0)
                return;

            var priority = priorities[result];

            await SetPriority(priority);
        }

        private async Task SetPriority(Priority priority)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("setting_priority___"));

            try
            {
                CommonConfig.Logger.Info($"Attempting to setting priority for document [documentId={document.Id}]");
                await Managers.DocumentsManager.SetDocumentsPriorityAsync(
                    new List<DocumentPreview> { documentPreview }, priority);

                UpdatePriority();

                dismissAction();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Error while setting priority for document [documentId={document.Id}]", ex);
                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        public void UpdatePriority()
        {
            headerView.UpdatePriority();
        }

        private void FileToButton_Clicked(object sender, EventArgs e)
        {
            var eas = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);

            var button = (UIBarButtonItem)sender;

            if (ServerConfig.SystemSettings.DocumentsModuleInfo.WorktrayEnabled ?? true)
            {
                eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_worktray"),
                                                   UIAlertActionStyle.Default,
                                                   a =>
                {
                    var vc = new CopyToWorktrayViewController
                    {
                        BusinessEntities = new List<IBusinessEntity>
                        {
                            document
                        }
                    };
                    PresentViewController(new NavigationController(
                        vc, UIModalPresentationStyle.PageSheet), true, null);
                }));
            }

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_folder"),
                UIAlertActionStyle.Default,
                a =>
                {
                    var vc = new CopyMoveToFolderListViewController(ModuleType.Documents, new List<IBusinessEntity> { document });
                    PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
                }));

            if (PlatformConfig.Preferences.EnableMoveToFolder &&
                (folder?.InternalType == FolderInternalType.FilterView
                || folder?.InternalType == FolderInternalType.Static
                || folder?.InternalType == FolderInternalType.Worktray))
            {
                eas.AddAction(UIAlertAction.Create(Localization.GetString("move_to_folder"),
                      UIAlertActionStyle.Default,
                      a =>
                      {
                          var vc = new CopyMoveToFolderListViewController(ModuleType.Documents, new List<IBusinessEntity> { document }, folder);
                          PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
                      }));
            }

            eas.AddAction(UIAlertAction.Create(Localization.GetString("set_priority"),
                UIAlertActionStyle.Default, a => ShowPriorityActionSheet((UIBarButtonItem)sender)));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("print"), 
                UIAlertActionStyle.Default, a => Print()));

            if (folder?.InternalType == FolderInternalType.FilterView
                || folder?.InternalType == FolderInternalType.Static
                || folder?.InternalType == FolderInternalType.Worktray)
            {
                eas.AddAction(UIAlertAction.Create(
                    Localization.GetString("delete_from_folder"), 
                    UIAlertActionStyle.Default, (a) => RemoveFromFolder(button)));
            }

            var documents = new List<DocumentPreview>
            {
                documentPreview
            };
            if (DocumentsDeleteChecker.CanDeleteDocuments(documents))
            {
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete"), 
                    UIAlertActionStyle.Destructive, (a) => Delete(button)));
            }

            eas.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, null));

            if (eas.PopoverPresentationController != null)
                eas.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate((UIBarButtonItem)sender);

            PresentViewController(eas, true, null);
        }

        #endregion

        #region NavigationBar event handlers

        private void EditDocumentButtonItem_Clicked(object sender, EventArgs e) => PresentEditing();

        private void DoneButtonItem_Clicked(object sender, EventArgs e) => DismissViewController(true, null);

        #endregion

        #region Actions

        public void PresentEditing()
        {
            var vc = new ComposeDocumentViewController
            {
                DocumentCreationModeFlag = DocumentCreationModeFlag.Edit,
                PreviousDocumentDirection = documentPreview.Direction,
                PreviousDocumentFolderId = folder?.Id,
                PreviousDocumentId = documentPreview.Id
            };

            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        private async Task DoChangeReadStatus(bool isReadByCurrent)
        {
            CommonConfig.Logger.Info($"Attempting to mark as {(isReadByCurrent ? "unread" : "read")} [documentPreview={documentPreview}]...");

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString(isReadByCurrent ? "marking_document_as_unread___" : "marking_document_as_read___"));

            try
            {
                CommonConfig.UsageAnalytics.LogEvent(new SetReadStatusEvent(1));

                await Managers.DocumentsManager.SetDocumentReadStatusAsync(documentPreview, document, !isReadByCurrent);

                dismissAction();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Marking as {(isReadByCurrent ? "unread" : "read")}  failed [documentPreview={documentPreview}]", ex);

                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        private void DoAssignCategory()
        {
            if (!ServerConfig.SystemSettings.SystemInfo.FavoriteCategoriesAvailable)
            {
                var vcOld = new CategoriesListOldViewController(documentPreview);
                PresentViewController(new NavigationController(vcOld, UIModalPresentationStyle.PageSheet), true, null);
            }
            else
            {
                var vc = new CategoriesListViewController(documentPreview);
                PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
            }
        }

        async void UserActionsButton_Clicked(object sender, EventArgs e)
        {
            var actionLinksListString = new []
            {
                Localization.GetString("history"),
                Localization.GetString("overview")
            };

            var result = await Dialogs.ShowListActionSheetAsync(this, actionLinksListString, (UIBarButtonItem)sender);

            if (result < 0)
                return;

            UIViewController vc = null;
            switch (result)
            {
                case 0:
                    vc = new ObjectActionsListViewController(document);
                    break;
                case 1:
                    vc = new ObjectLinksListViewController(document);
                    break;
            }

            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        private async void CommentsButton_Clicked(object sender, EventArgs e)
        {
             var vc = new CommentsListViewController
             {
                 Entity = document
             };
             PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        private void DoRespond(DocumentCreationModeFlag creationModeFlag)
        {
            var vc = new ComposeDocumentViewController
            {
                DocumentCreationModeFlag = creationModeFlag,
                PreviousDocumentDirection = documentPreview.Direction,
                PreviousDocumentFolderId = folderId ?? folder?.Id,
                PreviousDocumentId = documentPreview.Id
            };

            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        private async void CopyToNew()
        {
            if (document == null || documentPreview == null)
                return;

            var hasAttachments = document.Attachments.Any();
            var data = hasAttachments 
                ? new[] { CopyToNewOption.Addresses, CopyToNewOption.Content, CopyToNewOption.Attachments } 
                : new[] { CopyToNewOption.Addresses, CopyToNewOption.Content };

            var selections = await Dialogs.ShowMultiSelectViewControllerAsync(this, 
                Localization.GetString("copy_to_new"), data, data, UI.PrettyCopyToNewString, 
                LambdaEqualityComparer<CopyToNewOption>.Create(option => option), true);
            
            if (selections == null || selections.Length < 1)
                return;

            CopyToNewOption copyToNewOption = CopyToNewOption.None;
            for (var i = 0; i < selections.Length; i++)
                copyToNewOption |= selections[i];

            var vc = new ComposeDocumentViewController
            {
                DocumentCreationModeFlag = DocumentCreationModeFlag.New,
                CopyToNewOption = copyToNewOption,
                PreviousDocumentDirection = documentPreview.Direction,
                PreviousDocumentFolderId = folderId ?? folder?.Id,
                PreviousDocumentId = documentPreview.Id
            };

            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        private async void RemoveFromFolder(UIBarButtonItem button)
        {
            var d = new PopoverPresentationControllerDelegate(button);
            var result = await Dialogs.ShowDestructiveActionSheetAsync(this, Localization.GetString("delete_from_folder"), d);
            if (!result)
                return;

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("deleting_from_folder___"));

            try
            {
                CommonConfig.Logger.Info($"Attempting to remove documnet from folder [documentId={document.Id}, folderId={folder.Id}]");

                await Managers.CommonActionsManager.RemoveFromFolder(new List<IBusinessEntity> { document }, folder);

                dismissAction();

                if (SplitViewController != null && !SplitViewController.Collapsed)
                    ClearData();
                else
                    NavigationController?.PopViewController(true);
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Error while removing document from folder [documentId={document.Id}, folderId={folder.Id}]", ex);
                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        private async void Delete(UIBarButtonItem button)
        {
            var d = new PopoverPresentationControllerDelegate(button);
            var result = await Dialogs.ShowDestructiveActionSheetAsync(this, Localization.GetString("delete"), d);
            if (!result)
                return;

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("deleting___"));

            try
            {
                CommonConfig.Logger.Info($"Attempting to delete document [documentId={document.Id}]");

                await Managers.CommonActionsManager.Delete(new List<IBusinessEntity>
                {
                    document
                });

                dismissAction();

                if (SplitViewController != null && !SplitViewController.Collapsed)
                    ClearData();
                else
                    NavigationController?.PopViewController(true);
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Error while deleting document [documentId={document.Id}]", ex);
                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        private async Task SendInvitationReply(CalendarInvitationView cv, CalendarInvitation invitation, InvitationReplyDetailViewModel vm)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("sending_reply___"));

            try
            {
                CommonConfig.Logger.Info($"Attempting to reply to calendar invitation for document [documentId={document.Id}]");


                var responseDocument = new Document();
                var responseDocumentPreview = new DocumentPreview();

                //Line
                responseDocument.Lines = new List<Line> { vm.Line };

                //Body
                var previousDocumentContent = string.Empty;

                if (!string.IsNullOrWhiteSpace(document?.HtmlBody))
                {
                    var config = HtmlProcessingConfiguration.DefaultForEditing;
                    config.InjectReplyHeader = true;
                    config.ReplyHeaderParameters = HtmlUtilities.GetReplyHeaderParameters(documentPreview, document);
                    previousDocumentContent = await HtmlUtilities.ProcessHtml(document.HtmlBody, config);
                }
                else if (!string.IsNullOrWhiteSpace(document?.PlainTextBody))
                {
                    var config = PlainTextProcessingConfiguration.DefaultForEditing;
                    config.InjectReplyHeader = true;
                    config.ReplyHeaderParameters = HtmlUtilities.GetReplyHeaderParameters(documentPreview, document);
                    previousDocumentContent = await HtmlUtilities.ProcessPlainText(document.PlainTextBody, config);
                }

                responseDocument.HtmlBody = await HtmlUtilities.MergeReplyWithPreviousDocument(vm.Message, previousDocumentContent);

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

                responseDocumentPreview.Subject = responseSubjectString + documentPreview.Subject;

                //Addresses
                documentPreview.Addresses.Where(x => x.AddressType == DocumentAddressType.From).ToList().ForEach(da =>
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

                if (Managers.MicrosoftGraphClient == null || !Managers.MicrosoftGraphClient.IsAuthenticated())
                { 
                    Managers.MicrosoftGraphClient = new MicrosoftGraphClient();
                    await Managers.MicrosoftGraphClient.Authenticate(this, forceInteractive: false);
                }

                await Managers.DocumentsManager.ReplyToCalendarInvitationAsync(responseDocument, responseDocumentPreview,
                    invitation, vm.Status, string.IsNullOrEmpty(vm.Message), document.Id, folder?.Id ?? folderId ?? 0);

                invitation.Status = vm.Status;
                cv.RefreshView();

                dismissAction();
            }
            catch (Exception ex)
            {
                dismissAction();

                var id = document == null ? string.Empty : document.Id.ToString();
                CommonConfig.Logger.Error($"Error while replying to calendar invitation for document [documentId={id}]", ex);
                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        #endregion

        #region Utilities

        protected override bool CanNavigate(WKNavigationAction action)
        {
            if (action.NavigationType == WKNavigationType.LinkActivated
                || action.NavigationType == WKNavigationType.BackForward
                || action.NavigationType == WKNavigationType.FormSubmitted
                || action.NavigationType == WKNavigationType.FormResubmitted)
            {
                if (action.Request.Url.Scheme == "mailto")
                {
                    try
                    {
                        if (action.Request.Url.AbsoluteString != null)
                        {
                            var parts = action.Request.Url.AbsoluteString.Split("?");
                            var to = parts[0].Split(",");

                            if (parts.Length > 1)
                            {
                                var parsed = HttpUtility.ParseQueryString(parts[1]);
                                var subject = parsed["subject"];
                                var body = parsed["body"];
                                var cc = parsed["cc"]?.Split(",");
                                var bcc = parsed["bcc"]?.Split(",");

                                PresentComposeViewWithPreconfiguredFields(subject, body, to, cc, bcc);
                            }
                            else
                                PresentComposeViewWithPreconfiguredFields(preconfiguredToAddresses: to);
                        }
                    }
                    catch (Exception ex)
                    {
                        CommonConfig.Logger.Error("Request.Url.AbsoluteString : " + action.Request.Url.AbsoluteString, ex);
                    }
                }
                else
                {
                    Integration.OpenLink(action.Request.Url,
                        async () => await Dialogs.ShowConfirmAlertAsync(
                            this,
                            Localization.GetString("unable_open_link_title"),
                            Localization.GetString("unable_open_link_content") + action.Request.Url.Scheme));
                }
                return false;
            }

            return action.NavigationType != WKNavigationType.Reload;
        }

        #endregion

        #region Messages handlers

        private void ReadStatusChangedHandler(DocumentPreviewReadStatusChangedMessage obj)
        {
            BeginInvokeOnMainThread(headerView.UpdateReadBy);
        }

        private void DraftSentHandler(DraftSentMessage message)
        {
            BeginInvokeOnMainThread(() =>
            {
                if (PresentingViewController == null)
                    NavigationController?.PopViewController(true);
                else
                    DismissViewController(true, null);
            });
        }

        private void CommentsCountChangedHandler(EntityPreviewCommentCountChangedMessage m)
        {
            if (Integration.IsIPad())
            {
                BeginInvokeOnMainThread(() => DocumentPageViewControllerDelegate?.UpdateIPadNavigationButtons(
                    true, document.Comments.Count().ToString()));
            }
            else
            {
                BeginInvokeOnMainThread(() => commentsBadgeButton.SetBadgeValue(
                    document.Comments.Count().ToString(), false));
            }
        }

        #endregion

        #region State restoration

        public override void EncodeRestorableState(NSCoder coder)
        {
            base.EncodeRestorableState(coder);

            coder.Encode(PresentingViewController != null, "doNotRestore");

            coder.Encode(failedDocumentToUploadGuid.ToByteArray(), "failedDocumentToUploadGuid");
            if (folderId.HasValue)
                coder.Encode(folderId.Value, "folderId");
            if (folder != null)
                coder.Encode(Serializer.SerializeToByteArray(folder.ShallowCopy()), "folder");
            if (documentId.HasValue)
                coder.Encode(documentId.Value, "documentId");
            if (documentPreview != null)
                coder.Encode(Serializer.SerializeToByteArray(documentPreview), "documentPreview");
            if (document != null)
                coder.Encode(Serializer.SerializeToByteArray(document), "document");
        }

        public override void DecodeRestorableState(NSCoder coder)
        {
            base.DecodeRestorableState(coder);

            failedDocumentToUploadGuid = new Guid(coder.DecodeBytes("failedDocumentToUploadGuid"));
            if (coder.ContainsKey("folderId"))
                folderId = coder.DecodeInt("folderId");
            if (folder != null)
                folder = Serializer.DeserializeFromByteArray<Folder>(coder.DecodeBytes("folder"));
            if (coder.ContainsKey("documentId"))
                documentId = coder.DecodeInt("documentId");
            if (coder.ContainsKey("documentPreview"))
                documentPreview = Serializer.DeserializeFromByteArray<DocumentPreview>(coder.DecodeBytes("documentPreview"));
            if (coder.ContainsKey("document"))
                document = Serializer.DeserializeFromByteArray<Document>(coder.DecodeBytes("document"));

            refreshDataOnAppear = true;
        }

        [Export("viewControllerWithRestorationIdentifierPath:coder:")]
        public static UIViewController Restore(string[] identifierComponents, NSCoder coder) => 
            coder.DecodeBool("doNotRestore") ? null : new DocumentViewController();

        #endregion
    }
}