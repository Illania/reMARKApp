using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.HubMessages;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Model.HubMessages;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentView;
using Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView;
using Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.HeaderView;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;
using Mark5.Mobile.IOS.Ui.ViewControllers.MailViewerView;
using Mark5.Mobile.IOS.Utilities;
using TinyMessenger;
using UIKit;
using WebKit;

using static Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.DocumentPageViewController;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class DocumentViewController : AbstractWebViewController, ISecondaryViewController, IUIViewControllerRestoration
    {
        const int LargeAttachmentSizeInBytes = 20 * 1024 * 1024; // 20MB

        public bool Empty => document == null && documentPreview == null && folderId == null && folder == null && documentId == null;

        public DocumentPreview DocumentPreview => documentPreview;

        WeakReference<IDocumentPageViewControllerDelegate> weakRefDocumentPageViewControllerDelegate;

        public IDocumentPageViewControllerDelegate DocumentPageViewControllerDelegate
        {
            get
            {
                return weakRefDocumentPageViewControllerDelegate.TryGetTarget(out IDocumentPageViewControllerDelegate documentPageViewControllerDelegate) ? documentPageViewControllerDelegate : null;
            }
            set
            {
                weakRefDocumentPageViewControllerDelegate = new WeakReference<IDocumentPageViewControllerDelegate>(value);
            }
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
        UIButton commentsButton;
        BadgeBarButtonItem commentsBadgeButton;
        UIBarButtonItem replyActionsButton;
        UIBarButtonItem userActionsButton;

        CancellationTokenSource readStatusCts;
        CancellationTokenSource loadCts;

        bool refreshDataOnAppear;
        bool hideDoneButton;

        TinyMessageSubscriptionToken readStatusChangedToken;
        TinyMessageSubscriptionToken draftSentToken;
        TinyMessageSubscriptionToken commentsCountChangedToken;

        public DocumentViewController()
        {
            HidesBottomBarWhenPushed = true;
        }

        #region UIViewController overrides

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            InitNavigationBar();
            InitHeaderView();
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
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info("Appeared");

            if (refreshDataOnAppear)
            {
                refreshDataOnAppear = false;
                RefreshData();
            }
            else
                MarkAsReadIfNecessary();
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

            if (NavigationController != null && !(ParentViewController is DocumentPageViewController))
                NavigationController.ToolbarHidden = true;
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

        void InitNavigationBar()
        {
            if (PresentingViewController == null)
            {
                editDocumentButtonItem = new UIBarButtonItem
                {
                    Image = UIImage.FromBundle("Edit")
                };
            }
            else if (!hideDoneButton)
            {
                doneButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Done);
                NavigationItem.SetRightBarButtonItem(doneButtonItem, false);
            }
        }

        void InitHeaderView()
        {
            SetHeaderView(headerView = new HeaderView());
        }

        void InitToolbar()
        {
            commentsButton = new UIButton
            {
                Frame = new CoreGraphics.CGRect(0f, 0f, 25f, 25f),
                Enabled = false,
            };
            commentsButton.SetImage(UIImage.FromBundle("Comments").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);

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

        void InitializeHandlers()
        {
            if (headerView != null)
            {
                headerView.RecipientTapped += HeaderView_RecipientTapped;
                headerView.AttachmentTapped += HeaderView_AttachmentTapped;

                headerView.BeginAnimating += HeaderView_BeginAnimating;
                headerView.Animating += HeaderView_Animating;
                headerView.EndAnimating += HeaderView_EndAnimating;
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

        void DeinitializeHandlers()
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

        void SubscribeToMessages()
        {
            readStatusChangedToken = CommonConfig.MessengerHub.Subscribe<DocumentPreviewReadStatusChangedMessage>(ReadStatusChangedHandler, m => m.DocumentPreviewId == document?.Id);
            draftSentToken = CommonConfig.MessengerHub.Subscribe<DraftSentMessage>(DraftSentHandler, m => m.DocumentId == document?.Id);
            commentsCountChangedToken = CommonConfig.MessengerHub.Subscribe<EntityPreviewCommentCountChangedMessage>(CommentsCountChangedHandler, m => m.EntityId == document?.Id);

        }

        void UnsubscribeFromMessages()
        {
            readStatusChangedToken?.Dispose();
            draftSentToken?.Dispose();
        }

        public void SetData(Folder folder, DocumentPreview documentPreview)
        {
            CommonConfig.UsageAnalytics.LogEvent(new OpenDocumentEvent(documentPreview?.Direction == DocumentDirection.External));

            failedDocumentToUploadGuid = Guid.Empty;
            document = null;
            documentId = null;
            folderId = null;
            notificationGuid = default(Guid);

            this.documentPreview = documentPreview;
            this.folder = folder;
        }

        public void SetData(DocumentPreview documentPreview, bool hideDoneButton)
        {
            CommonConfig.UsageAnalytics.LogEvent(new OpenDocumentEvent(documentPreview?.Direction == DocumentDirection.External));

            failedDocumentToUploadGuid = Guid.Empty;
            folder = null;
            document = null;
            documentId = null;
            folderId = null;
            notificationGuid = default(Guid);

            this.documentPreview = documentPreview;
            this.hideDoneButton = hideDoneButton;
        }

        public void SetData(DocumentPreview documentPreview)
        {
            CommonConfig.UsageAnalytics.LogEvent(new OpenDocumentEvent(documentPreview?.Direction == DocumentDirection.External));

            failedDocumentToUploadGuid = Guid.Empty;
            document = null;
            documentId = null;
            folderId = null;
            folder = null;
            notificationGuid = default(Guid);

            this.documentPreview = documentPreview;
        }

        public void SetData(int documentId)
        {
            CommonConfig.UsageAnalytics.LogEvent(new OpenDocumentEvent(false));

            failedDocumentToUploadGuid = Guid.Empty;
            document = null;
            folder = null;
            folderId = null;
            documentPreview = null;
            notificationGuid = default(Guid);

            this.documentId = documentId;
        }

        public void SetData(int documentId, Guid notificationGuid)
        {
            CommonConfig.UsageAnalytics.LogEvent(new OpenDocumentEvent(false));

            failedDocumentToUploadGuid = Guid.Empty;
            document = null;
            folder = null;
            folderId = null;
            documentPreview = null;
            this.documentId = documentId;
            this.notificationGuid = notificationGuid;
        }

        public void SetData(Guid failedDocumentToUploadGuid)
        {
            CommonConfig.UsageAnalytics.LogEvent(new OpenDocumentEvent(false));

            document = null;
            documentPreview = null;
            folder = null;
            documentId = null;
            notificationGuid = default(Guid);

            this.failedDocumentToUploadGuid = failedDocumentToUploadGuid;
        }

        public void ClearData()
        {
            loadCts?.Cancel();

            failedDocumentToUploadGuid = Guid.Empty;
            document = null;
            documentPreview = null;
            folder = null;
            documentId = null;
            notificationGuid = default(Guid);

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

            Clear();
        }

        public bool IsShowingDocumentWithId(int documentId)
        {
            return documentPreview?.Id == documentId || this.documentId == documentId;
        }

        public void SetRefreshDataOnAppear()
        {
            refreshDataOnAppear = true;
        }

        #endregion

        #region Refresh methods

        public async void RefreshData()
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
            if (folder == null || document == null || documentPreview == null)
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

                    await Managers.DocumentsManager.SetDocumentReadStatusAsync(dp, d, true, ServerConfig.SystemSettings.UserInfo.User);
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error($"Marking document as read failed [folder.name={f?.Name}, folder.id={f?.Id}, documentPreviewId={dp?.Id}]", ex);
                }
            });
        }

        public void RefreshNavigationBar()
        {
            if (PresentingViewController == null && document != null && documentPreview.Direction == DocumentDirection.Draft)
            {
                var rightButtons = new UIBarButtonItem[1];
                rightButtons[0] = editDocumentButtonItem;
                NavigationItem.SetRightBarButtonItems(rightButtons, true);
            }
        }

        public void RefreshToolbar()
        {
            var enableBottomActions = failedDocumentToUploadGuid == Guid.Empty;
            if (enableBottomActions)
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

        async void HeaderView_AttachmentTapped(object sender, AttachmentButtonTappedEventArgs e)
        {
            var attachmentDescription = e.Attachment;
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("opening_attachment___"));

            CommonConfig.UsageAnalytics.LogEvent(new DocumentOpenAttachmentEvent());

            try
            {
                var path = await Managers.DocumentsManager.GetAttachmentAsync(attachmentDescription, document, false, SourceType.Local);

                if (string.IsNullOrWhiteSpace(path))
                {
                    if (PlatformConfig.Preferences.LargeAttachmentWarning
                        && attachmentDescription.SizeInBytes > LargeAttachmentSizeInBytes
                        && !await Dialogs.ShowYesNoAlertAsync(this, Localization.GetString("warning"), string.Format(Localization.GetString("big_attachment_warning"), UI.PrettyFileSize(attachmentDescription.SizeInBytes))))
                    {
                        dismissAction();
                        return;
                    }

                    path = await Managers.DocumentsManager.GetAttachmentAsync(attachmentDescription, document, false, SourceType.Remote);
                }

                if (string.IsNullOrWhiteSpace(path))
                    throw new Exception("Unable to open attachment.");

                var url = NSUrl.FromFilename(path);

                if (MailViewerViewController.CanOpen(url))
                {
                    PresentViewController(new NavigationController(new MailViewerViewController(url), UIModalPresentationStyle.PageSheet), true, null);
                }
                else
                {
                    var attachmentInteractionController = UIDocumentInteractionController.FromUrl(url);
                    attachmentInteractionController.Delegate = new DocumentInteractionControllerDelegate(this);

                    var previewSuccessful = attachmentInteractionController.PresentPreview(true);
                    if (!previewSuccessful)
                    {
                        CommonConfig.Logger.Info($"Failed to present preview for attachment. Presenting open with instead. [documentId={document.Id}, attachment={attachmentDescription}]");
                        var openInSuccessful = attachmentInteractionController.PresentOptionsMenu(View.Frame, View, true);
                        if (!openInSuccessful)
                        {
                            CommonConfig.Logger.Warning($"Failed to present open in view - there is no app that can open this type of attachment installed. [documentId={document.Id}, attachment={attachmentDescription}]");
                            await Dialogs.ShowConfirmAlertAsync(this, Localization.GetString("cannot_open_attachment_title"), Localization.GetString("cannot_open_attachment_content"));
                        }
                    }
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

        void HeaderView_RecipientTapped(object sender, RecipientTappedEventArgs e)
        {
            PresentComposeViewWithPreconfiguredAddresses(new string[] { e.Recipent });
        }

        void PresentComposeViewWithPreconfiguredAddresses(string[] preconfiguredEmailAddresses)
        {
            var vc = new ComposeDocumentViewController
            {
                DocumentCreationModeFlag = DocumentCreationModeFlag.New,
                PreconfiguredEmailAddresses = new Dictionary<DocumentAddressType, string[]>
                {
                    { DocumentAddressType.To, preconfiguredEmailAddresses }
                }
            };

            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        protected override void OnWebViewLoaded()
        {
            base.OnWebViewLoaded();
            if (weakRefDocumentPageViewControllerDelegate != null)
                DocumentPageViewControllerDelegate?.AddViewControllerToCache(this);
        }

        #endregion

        #region Toolbar event handlers

        async void FlagButton_Clicked(object sender, EventArgs e)
        {
            var isRead = documentPreview.IsReadByCurrent;
            var flagListStrings = new string[]
            {
                Localization.GetString(isRead ? "mark_as_unread" : "mark_as_read"),
                Localization.GetString("categories")
            };

            var result = await Dialogs.ShowListActionSheetAsync(this, flagListStrings, flagButton);

            if (result < 0)
                return;

            switch (result)
            {
                case 0:
                    await DoChangeReadStatus();
                    break;
                case 1:
                    DoAssignCategory();
                    break;
            }
        }

        async void ReplyActionsButton_Clicked(object sender, EventArgs e)
        {
            var replyListStrings = new string[]
            {
                Localization.GetString("reply"),
                Localization.GetString("reply_all"),
                Localization.GetString("forward"),
                Localization.GetString("copy_to_new")};

            var result = await Dialogs.ShowListActionSheetAsync(this, replyListStrings, replyActionsButton);

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

        async void ShowPriorityActionSheet(UIBarButtonItem barButtonItem)
        {
            var priorities = new List<Priority>
            {
                Priority.Low,
                Priority.Normal,
                Priority.Urgent
            };
            var priorityStrings = priorities.Select(p => UI.PrettyPriorityString(p));
            var result = await Dialogs.ShowListActionSheetAsync(this, priorityStrings.ToArray(), barButtonItem);

            if (result < 0)
                return;

            var priority = priorities[result];

            await SetPriority(priority);
        }

        async Task SetPriority(Priority priority)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("setting_priority___"));

            try
            {
                CommonConfig.Logger.Info($"Attempting to setting priority for document [documentId={document.Id}]");
                await Managers.DocumentsManager.SetDocumentsPriorityAsync(new List<DocumentPreview> { documentPreview }, priority);

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

        void FileToButton_Clicked(object sender, EventArgs e)
        {
            var eas = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);

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
                    PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
                }));
            }

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_folder"),
                UIAlertActionStyle.Default,
                a =>
                {
                    var vc = new CopyMoveToFolderListViewController(ModuleType.Documents, new List<IBusinessEntity> { document });
                    PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
                }));

            if (folder?.InternalType == FolderInternalType.FilterView || folder?.InternalType == FolderInternalType.Static || folder?.InternalType == FolderInternalType.Worktray)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("move_to_folder"),
                    UIAlertActionStyle.Default,
                    a =>
                    {
                        var vc = new CopyMoveToFolderListViewController(ModuleType.Documents, new List<IBusinessEntity> { document }, folder);
                        PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
                    }));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("set_priority"), UIAlertActionStyle.Default, a => ShowPriorityActionSheet((UIBarButtonItem)sender)));

            if (folder?.InternalType == FolderInternalType.FilterView || folder?.InternalType == FolderInternalType.Static || folder?.InternalType == FolderInternalType.Worktray)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete_from_folder"), UIAlertActionStyle.Default, RemoveFromFolder));

            if (ServerConfig.SystemSettings.DocumentsModuleInfo.Permissions.DeleteAllowed)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete"), UIAlertActionStyle.Destructive, Delete));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, null));

            if (eas.PopoverPresentationController != null)
                eas.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate((UIBarButtonItem)sender);

            PresentViewController(eas, true, null);
        }

        #endregion

        #region NavigationBar event handlers

        void EditDocumentButtonItem_Clicked(object sender, EventArgs e) => PresentEditing();

        void DoneButtonItem_Clicked(object sender, EventArgs e) => DismissViewController(true, null);

        #endregion

        #region Actions

        public void PresentEditing()
        {
            var vc = new ComposeDocumentViewController
            {
                DocumentCreationModeFlag = DocumentCreationModeFlag.Edit,
                PreviousDocumentDirection = documentPreview.Direction,
                PreviousDocumentFolderId = folder.Id,
                PreviousDocumentId = documentPreview.Id
            };

            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        async Task DoChangeReadStatus()
        {
            var isReadByCurrent = documentPreview.IsReadByCurrent;

            CommonConfig.Logger.Info($"Attempting to mark as {(isReadByCurrent ? "unread" : "read")} [documentPreview={documentPreview}]...");

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString(isReadByCurrent ? "marking_document_as_unread___" : "marking_document_as_read___"));

            try
            {
                CommonConfig.UsageAnalytics.LogEvent(new SetReadStatusEvent(1));

                await Managers.DocumentsManager.SetDocumentReadStatusAsync(documentPreview, document, !isReadByCurrent, ServerConfig.SystemSettings.UserInfo.User);

                dismissAction();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Marking as {(isReadByCurrent ? "unread" : "read")}  failed [documentPreview={documentPreview}]", ex);

                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        void DoAssignCategory()
        {
            var vc = new CategoriesListViewController(documentPreview);
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        async void UserActionsButton_Clicked(object sender, EventArgs e)
        {
            var actionLinksListString = new string[]
            {
                Localization.GetString("actions"),
                Localization.GetString("links")
            };

            var result = await Dialogs.ShowListActionSheetAsync(this, actionLinksListString, userActionsButton);

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

        void CommentsButton_Clicked(object sender, EventArgs e)
        {
            var vc = new CommentsListViewController
            {
                Entity = document
            };
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        void DoRespond(DocumentCreationModeFlag creationModeFlag)
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

        async void CopyToNew()
        {
            if (document == null || documentPreview == null)
                return;

            CopyToNewOption[] data;

            var hasAttachments = document.Attachments.Any();
            if (hasAttachments)
                data = new[] { CopyToNewOption.Addresses, CopyToNewOption.Content, CopyToNewOption.Attachments };
            else
                data = new[] { CopyToNewOption.Addresses, CopyToNewOption.Content };

            var selections = await Dialogs.ShowMultiSelectViewControllerAsync(this, Localization.GetString("copy_to_new"), data, data, UI.PrettyCopyToNewString, LambdaEqualityComparer<CopyToNewOption>.Create(ctno => ctno), true);
            if (selections == null || selections.Length < 1)
                return;

            CopyToNewOption copyToNewOption = CopyToNewOption.None;
            for (int i = 0; i < selections.Length; i++)
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

        void CopyToWorktray()
        {
            var vc = new CopyToWorktrayViewController
            {
                BusinessEntities = new List<IBusinessEntity> { document }
            };
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        async void RemoveFromFolder(UIAlertAction a)
        {
            var d = new PopoverPresentationControllerDelegate(fileToButton);
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

        async void Delete(UIAlertAction a)
        {
            var d = new PopoverPresentationControllerDelegate(fileToButton);
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
                    var address = action.Request.Url.ResourceSpecifier;
                    PresentComposeViewWithPreconfiguredAddresses(new string[] { address });
                }
                else
                    Integration.OpenLink(action.Request.Url,
                                         async () => await Dialogs.ShowConfirmAlertAsync(this,
                                                                                         Localization.GetString("unable_open_link_title"),
                                                                                         Localization.GetString("unable_open_link_content") + action.Request.Url.Scheme));

                return false;
            }

            if (action.NavigationType == WKNavigationType.Reload)
                return false;

            return true;
        }

        #endregion

        #region Messages handlers

        void ReadStatusChangedHandler(DocumentPreviewReadStatusChangedMessage obj)
        {
            BeginInvokeOnMainThread(headerView.UpdateReadBy);
        }

        void DraftSentHandler(DraftSentMessage message)
        {
            BeginInvokeOnMainThread(() =>
            {
                if (PresentingViewController == null)
                    NavigationController?.PopViewController(true);
                else
                    DismissViewController(true, null);
            });
        }

        void CommentsCountChangedHandler(EntityPreviewCommentCountChangedMessage m)
        {
            BeginInvokeOnMainThread(() => commentsBadgeButton.SetBadgeValue(document.Comments.Count().ToString(), false));
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
        public static UIViewController Restore(string[] identifierComponents, NSCoder coder)
        {
            if (coder.DecodeBool("doNotRestore"))
                return null;

            return new DocumentViewController();
        }

        #endregion

    }
}