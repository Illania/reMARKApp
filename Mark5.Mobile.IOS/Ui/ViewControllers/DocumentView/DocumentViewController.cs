//
// Project: Mark5.Mobile.IOS
// File: DocumentViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.Subviews;
using Mark5.Mobile.IOS.Utilities.UserInterface;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{

    public class DocumentViewController : UIViewController, ISecondaryViewController
    {
        static UIBarButtonItem FlexibleSpace
        {
            get
            {
                return new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
            }
        }

        public bool Modal
        {
            get;
            set;
        }

        public GetPreviousDocumentPreviewDelegate GetPreviousDocumentId
        {
            get;
            set;
        }

        public GetNextDocumentPreviewDelegate GetNextDocumentId
        {
            get;
            set;
        }

        public bool Empty { get { return true; } }

        public int? FolderId { get; set; }
        public Folder Folder { get; set; }
        public int? DocumentId { get; set; }
        public DocumentPreview DocumentPreview { get; set; }
        public Document Document { get; set; }
        public Guid NotificationGuid { get; set; }

        const int LargeAttachmentSizeInBytes = 20 * 1024 * 1024; // 20MB

        UIBarButtonItem doneButtonItem;
        UIBarButtonItem previousDocumentButtonItem;
        UIBarButtonItem nextDocumentButtonItem;

        ActionableLayoutScrollView mainScrollView;
        UIStackView stackViewBeforeContent;
        UIStackView stackViewAfterContent;

        FromView fromView;
        ToView toView;
        CcView ccView;
        BccView bccView;
        OriginatorView originatorView;
        SubjectView subjectView;
        DateReceivedView dateReceivedView;
        PriorityView priorityView;
        ContentView contentView;
        AttachmentsView attachmentsListView;
        CreatorView creatorView;
        ReadByView readByView;
        ReferenceNumberView referenceNumberView;

        UIBarButtonItem flag;
        UIBarButtonItem fileTo;
        UIButton commentsButton;
        BadgeBarButtonItem comments;
        UIBarButtonItem replyActions;
        UIBarButtonItem userActions;

        UIToolbar toolbar;
        NSLayoutConstraint toolbarBottomConstraint;

        List<DocumentSubView> subViews;

        UIDocumentInteractionController attachmentInteractionController;

        public delegate DocumentPreview GetPreviousDocumentPreviewDelegate(DocumentPreview documentPreview, out bool nextDocumentAvailable, out bool previousDocumentAvailable, bool scrollAndSelect = false);
        public delegate DocumentPreview GetNextDocumentPreviewDelegate(DocumentPreview documentPreview, out bool nextDocumentAvailable, out bool previousDocumentAvailable, bool scrollAndSelect = false);

        #region UIViewController overrides

        public override void LoadView()
        {
            base.LoadView();

            InitNavigationBar();
            InitStackViews();
            InitSubViews();
            InitToolbar();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            InitializeHandlers();
            CorrectToolbar();
        }

        public override async void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            await RefreshData();

            CorrectScrollViewInsets();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeInitializeHandlers();
        }

        #endregion

        #region Init methods

        void InitNavigationBar()
        {
            if (Modal)
            {
                doneButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Done);
                NavigationItem.SetRightBarButtonItem(doneButtonItem, false);
            }
            else
            {
                nextDocumentButtonItem = null;
                previousDocumentButtonItem = null;

                var rightButtons = new UIBarButtonItem[2];

                nextDocumentButtonItem = new UIBarButtonItem();
                nextDocumentButtonItem.Image = UIImage.FromBundle(Path.Combine("Icons", "arrow-down.png"));
                nextDocumentButtonItem.Enabled = false;
                rightButtons[0] = nextDocumentButtonItem;

                previousDocumentButtonItem = new UIBarButtonItem();
                previousDocumentButtonItem.Image = UIImage.FromBundle(Path.Combine("Icons", "arrow-up.png"));
                previousDocumentButtonItem.Enabled = false;
                rightButtons[1] = previousDocumentButtonItem;

                //TODO I removed the edit button, because I think we should do it as in Android. So if we tap on a local document, we get the compose view directly

                NavigationItem.SetRightBarButtonItems(rightButtons, false);
            }
        }

        void InitStackViews()
        {
            View.BackgroundColor = UIColor.White;

            mainScrollView = new ActionableLayoutScrollView
            {
                BackgroundColor = UIColor.White,
                ShowsVerticalScrollIndicator = true,
                ScrollEnabled = true,
                ScrollsToTop = true,
                UserInteractionEnabled = true,
                ClipsToBounds = false,
                TranslatesAutoresizingMaskIntoConstraints = false,
            };
            mainScrollView.LayoutSubviewsAction = HandleScrollViewLayoutSubviewsAction;
            View.AddSubview(mainScrollView);
            View.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(mainScrollView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(mainScrollView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(mainScrollView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, View, NSLayoutAttribute.Width, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(mainScrollView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1.0f, 0.0f),
                });

            stackViewBeforeContent = new UIStackView
            {
                BackgroundColor = UIColor.White,
                Axis = UILayoutConstraintAxis.Vertical,
                Alignment = UIStackViewAlignment.Fill,
                Distribution = UIStackViewDistribution.Fill,
                Spacing = 0.0f,
                TranslatesAutoresizingMaskIntoConstraints = false,
            };
            mainScrollView.AddSubview(stackViewBeforeContent);
            View.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(stackViewBeforeContent, NSLayoutAttribute.Top, NSLayoutRelation.Equal, mainScrollView, NSLayoutAttribute.Top, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(stackViewBeforeContent, NSLayoutAttribute.Left, NSLayoutRelation.Equal, mainScrollView, NSLayoutAttribute.Left, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(stackViewBeforeContent, NSLayoutAttribute.Width, NSLayoutRelation.Equal, mainScrollView, NSLayoutAttribute.Width, 1.0f, 0.0f),
                });

            contentView = new ContentView(mainScrollView);
            mainScrollView.AddSubview(contentView);
            mainScrollView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(contentView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, stackViewBeforeContent, NSLayoutAttribute.Bottom, 1.0f, 0.0f),
                NSLayoutConstraint.Create(contentView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, mainScrollView, NSLayoutAttribute.Left, 1.0f, 0.0f),
                NSLayoutConstraint.Create(contentView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, mainScrollView, NSLayoutAttribute.Right, 1.0f, 0.0f),
            });

            stackViewAfterContent = new UIStackView
            {
                BackgroundColor = UIColor.White,
                Axis = UILayoutConstraintAxis.Vertical,
                Alignment = UIStackViewAlignment.Fill,
                Distribution = UIStackViewDistribution.Fill,
                Spacing = 0.0f,
                TranslatesAutoresizingMaskIntoConstraints = false,
            };
            mainScrollView.AddSubview(stackViewAfterContent);
            View.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(stackViewAfterContent, NSLayoutAttribute.Top, NSLayoutRelation.Equal, contentView, NSLayoutAttribute.Bottom, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(stackViewAfterContent, NSLayoutAttribute.Left, NSLayoutRelation.Equal, mainScrollView, NSLayoutAttribute.Left, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(stackViewAfterContent, NSLayoutAttribute.Width, NSLayoutRelation.Equal, mainScrollView, NSLayoutAttribute.Width, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(stackViewAfterContent, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, mainScrollView, NSLayoutAttribute.Bottom, 1.0f, 0.0f),
                });
        }

        void InitSubViews()
        {
            var viewsBeforeContent = new List<DocumentSubView>();
            var viewsAfterContent = new List<DocumentSubView>();

            fromView = new FromView();
            viewsBeforeContent.Add(fromView);

            toView = new ToView();
            viewsBeforeContent.Add(toView);

            ccView = new CcView();
            viewsBeforeContent.Add(ccView);

            bccView = new BccView();
            viewsBeforeContent.Add(bccView);

            subjectView = new SubjectView();
            viewsBeforeContent.Add(subjectView);

            dateReceivedView = new DateReceivedView();
            viewsBeforeContent.Add(dateReceivedView);

            priorityView = new PriorityView();
            viewsBeforeContent.Add(priorityView);

            attachmentsListView = new AttachmentsView();
            viewsAfterContent.Add(attachmentsListView);

            referenceNumberView = new ReferenceNumberView();
            viewsAfterContent.Add(referenceNumberView);

            readByView = new ReadByView();
            viewsAfterContent.Add(readByView);

            creatorView = new CreatorView();
            viewsAfterContent.Add(creatorView);

            originatorView = new OriginatorView();
            viewsAfterContent.Add(originatorView);

            viewsBeforeContent.ForEach(stackViewBeforeContent.AddArrangedSubview);
            viewsAfterContent.ForEach(stackViewAfterContent.AddArrangedSubview);

            subViews = viewsBeforeContent.Append(contentView).Concat(viewsAfterContent).ToList();

            subViews.ForEach(v => v.UpdateVisibility());
        }

        void InitToolbar()
        {
            flag = new UIBarButtonItem();
            flag.Image = UIImage.FromBundle(Path.Combine("Icons", "flag.png"));
            flag.Enabled = false;

            fileTo = new UIBarButtonItem();
            fileTo.Image = UIImage.FromBundle(Path.Combine("Icons", "worktray.png"));
            fileTo.Enabled = false;

            commentsButton = new UIButton(UIButtonType.System);
            commentsButton.Frame = new CGRect(0.0f, 0.0f, 25.0f, 25.0f);
            commentsButton.SetImage(UIImage.FromBundle(Path.Combine("Icons", "comments.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
            commentsButton.Enabled = false;

            comments = new BadgeBarButtonItem(commentsButton)
            {
                BadgeBackgroundColor = Theme.Brown,
            };
            comments.Enabled = false;

            replyActions = new UIBarButtonItem();
            replyActions.Image = UIImage.FromBundle(Path.Combine("Icons", "reply.png"));
            replyActions.Enabled = false;

            userActions = new UIBarButtonItem();
            userActions.Image = UIImage.FromBundle(Path.Combine("Icons", "actions.png"));
            userActions.Enabled = false;

            toolbar = new UIToolbar();
            toolbar.BarStyle = UIBarStyle.Default;
            toolbar.Items = new[]
            {
                flag,
                FlexibleSpace,
                fileTo,
                FlexibleSpace,
                replyActions,
                FlexibleSpace,
                comments,
                FlexibleSpace,
                userActions,
            };
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            toolbar.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            View.AddSubview(toolbar);
            toolbarBottomConstraint = NSLayoutConstraint.Create(toolbar, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1.0f, 0.0f);
            View.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(toolbar, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(toolbar, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1.0f, 0.0f),
                    toolbarBottomConstraint,
                });
        }

        void CorrectToolbar()
        {
            toolbarBottomConstraint.Constant = SplitViewController != null ? -49.0f : 0.0f;
        }

        void CorrectScrollViewInsets()
        {
            mainScrollView.ContentInset = new UIEdgeInsets(mainScrollView.ContentInset.Top, 0.0f, mainScrollView.ContentInset.Bottom + toolbar.Frame.Height, 0.0f);
            mainScrollView.ScrollIndicatorInsets = new UIEdgeInsets(mainScrollView.ScrollIndicatorInsets.Top, 0.0f, mainScrollView.ScrollIndicatorInsets.Bottom + toolbar.Frame.Height, 0.0f);
        }

        void InitializeHandlers()
        {
            //Subviews
            fromView.RecipentTapped += HandleRecipentTapped; //TODO uniform naming
            toView.RecipentTapped += HandleRecipentTapped;
            ccView.RecipentTapped += HandleRecipentTapped;
            bccView.RecipentTapped += HandleRecipentTapped;
            attachmentsListView.AttachmentTapped += AttachmentsList_AttachmentTapped;

            //Toolbar
            flag.Clicked += Flag_Clicked;
            fileTo.Clicked += FileTo_Clicked;
            replyActions.Clicked += ReplyActions_Clicked;
            userActions.Clicked += DoShowUserActions;
            commentsButton.TouchUpInside += DoShowComments;

            //NavigationBar
            if (Modal)
            {
                doneButtonItem.Clicked += DoneButtonItem_Clicked;
            }
            else
            {
                nextDocumentButtonItem.Clicked += GoToNextDocument;
                previousDocumentButtonItem.Clicked += GoToPreviousDocument;
            }
        }

        void DeInitializeHandlers()
        {
            //Subviews
            fromView.RecipentTapped -= HandleRecipentTapped;
            toView.RecipentTapped -= HandleRecipentTapped;
            ccView.RecipentTapped -= HandleRecipentTapped;
            bccView.RecipentTapped -= HandleRecipentTapped;
            attachmentsListView.AttachmentTapped -= AttachmentsList_AttachmentTapped;

            //Toolbar
            flag.Clicked -= Flag_Clicked;
            fileTo.Clicked -= FileTo_Clicked;
            replyActions.Clicked -= ReplyActions_Clicked;
            userActions.Clicked -= DoShowUserActions;
            commentsButton.TouchUpInside -= DoShowComments;

            //NavigationBar
            if (Modal)
            {
                doneButtonItem.Clicked -= DoneButtonItem_Clicked;
            }
            else
            {
                nextDocumentButtonItem.Clicked -= GoToNextDocument;
                previousDocumentButtonItem.Clicked -= GoToPreviousDocument;
            }
        }

        #endregion

        #region Refresh methods

        public async void Reload()
        {
            Reset();
            await RefreshData();
        }

        void Reset()
        {
            //TODO what about cancellation tokens

            NavigationController.SetNavigationBarHidden(false, true);
            mainScrollView.SetContentOffset(new CGPoint(0, -mainScrollView.ContentInset.Top), false);

            nextDocumentButtonItem.Enabled = false;
            previousDocumentButtonItem.Enabled = false;

            var rightButtons = new UIBarButtonItem[2];
            rightButtons[0] = nextDocumentButtonItem;
            rightButtons[1] = previousDocumentButtonItem;
            NavigationItem.SetRightBarButtonItems(rightButtons, true);

            flag.Enabled = false;
            fileTo.Enabled = false;
            replyActions.Enabled = false;
            comments.SetBadgeValue("0", false);
            comments.Enabled = false;
            commentsButton.Enabled = false;
            userActions.Enabled = false;
        }

        async Task RefreshData()
        {
            try
            {
                if (NotificationGuid != default(Guid))
                {
                    await Managers.NotificationsManager.MarkAsRead(NotificationGuid);
                }

                if (DocumentId.HasValue && DocumentPreview == null && Document == null)
                {
                    var container = await Managers.DocumentsManager.GetDocumentWithPreviewAsync(FolderId ?? Folder?.Id, DocumentId.Value);
                    DocumentPreview = container.DocumentPreview;
                    Document = container.Document;
                }

                if (DocumentPreview != null && Document == null)
                {
                    Document = await Managers.DocumentsManager.GetDocumentAsync(FolderId ?? Folder?.Id, DocumentPreview.Id);
                }

                RefreshView();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Downloading document failed [folder.name={Folder?.Name}, folder.id={FolderId ?? Folder?.Id}, documentId={DocumentId ?? DocumentPreview?.Id}]", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);

                if (Modal)
                {
                    DismissViewController(true, null);
                }
                else
                {
                    NavigationController.PopViewController(true);
                }
            }
        }

        void RefreshView()
        {
            if (Document == null | DocumentPreview == null)
            {
                return;
            }

            Title = DocumentPreview.Subject;

            subViews.ForEach(v =>
            {
                v.Document = Document;
                v.DocumentPreview = DocumentPreview;
                v.RefreshView();
            });

            subViews.ForEach(v => v.UpdateVisibility());

            flag.Enabled = true;
            fileTo.Enabled = true;
            replyActions.Enabled = true;
            comments.BadgeValue = DocumentPreview.CommentsCount.ToString();
            comments.Enabled = true;
            commentsButton.Enabled = true;
            userActions.Enabled = true;

            UIView.Animate(0.075d, stackViewBeforeContent.LayoutIfNeeded);
            UIView.Animate(0.1d, () => stackViewBeforeContent.Alpha = 1.0f);
            UIView.Animate(0.075d, stackViewAfterContent.LayoutIfNeeded);
            UIView.Animate(0.1d, () => stackViewAfterContent.Alpha = 1.0f);
        }

        #endregion

        #region Subviews event handlers

        async void AttachmentsList_AttachmentTapped(object sender, AttachmentButtonTappedEventArgs e)
        {
            var attachmentDescription = e.Attachment;
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("opening_attachment___"));

            try
            {
                var path = await Managers.DocumentsManager.GetAttachmentAsync(attachmentDescription, Document, false, SourceType.Local);

                if (string.IsNullOrWhiteSpace(path))
                {
                    if (attachmentDescription.SizeInBytes > LargeAttachmentSizeInBytes
                        && PlatformConfig.Preferences.LargeAttachmentWarning
                        && !await Dialogs.ShowYesNoDialogAsync(this, Localization.GetString("big_attachment_title"),
                                                               string.Format(Localization.GetString("big_attachment_warning"), UI.PrettyFileSize(e.Attachment.SizeInBytes))))
                    {
                        dismissAction();
                        return;
                    }

                    path = await Managers.DocumentsManager.GetAttachmentAsync(attachmentDescription, Document, false, SourceType.Remote);
                }

                if (string.IsNullOrWhiteSpace(path))
                {
                    throw new Exception("Unable to get attachment path.");
                }

                attachmentInteractionController = UIDocumentInteractionController.FromUrl(NSUrl.FromFilename(path));
                attachmentInteractionController.Delegate = new AttachmentInteractionControllerDelegate(this, attachmentDescription);

                var previewSuccessful = attachmentInteractionController.PresentPreview(true);

                if (!previewSuccessful)
                {
                    CommonConfig.Logger.Info(string.Format("Failed to present preview for attachment. Presenting open with instead. [documentId={0}, attachment={1}]", Document.Id, attachmentDescription));
                }
                var openInSuccessful = attachmentInteractionController.PresentOpenInMenu(View.Frame, View, true);
                if (!openInSuccessful)
                {
                    CommonConfig.Logger.Warning(string.Format("Failed to present open in view - there is no app that can open this type of attachment installed. [documentId={0}, attachment={1}]", Document.Id, attachmentDescription));
                    await Dialogs.ShowConfirmDialogAsync(this, Localization.GetString("cannot_open_attachment_title"), Localization.GetString("cannot_open_attachment_content"));
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Failed to view attachment [document.Id={Document.Id}, attachment.Id={attachmentDescription?.Id}, attachment.Name={attachmentDescription?.Name}", ex);

                dismissAction();
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
            finally
            {
                dismissAction();
            }
        }

        void HandleRecipentTapped(object sender, RecipentTappedEventArgs e)
        {
            //TODO
        }

        #endregion

        #region Toolbar event handlers

        async void Flag_Clicked(object sender, EventArgs e)
        {
            var isRead = DocumentPreview.IsReadByCurrent;
            var flagListStrings = new string[] { Localization.GetString(isRead ? "mark_as_unread" : "mark_as_read"),
                    Localization.GetString("categories") };

            var result = await Dialogs.ShowListDialogAsync(this, null, flagListStrings, flag);
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

        async void ReplyActions_Clicked(object sender, EventArgs e)
        {
            var replyListStrings = new string[] { "reply",
                "reply_all",
                "forward" };

            var result = await Dialogs.ShowListDialogAsync(this, null, replyListStrings, replyActions);
            switch (result)
            {
                case 0:
                    DoReply(DocumentCreationModeFlag.Reply);
                    break;
                case 1:
                    DoReply(DocumentCreationModeFlag.ReplyAll);
                    break;
                case 2:
                    DoReply(DocumentCreationModeFlag.Forward);
                    break;
            }
        }

        async void FileTo_Clicked(object sender, EventArgs e)
        {
            var fileToListStrings = new string[] { "copy_to_worktray",
                "copy_to_folder",
                "move_to_folder",
                "delete_from_folder" };

            var result = await Dialogs.ShowListDialogAsync(this, null, fileToListStrings, fileTo);
            switch (result)
            {
                case 0:
                    DoFileToWorktray();
                    break;
                case 1:
                    DoFileToFolder(false);
                    break;
                case 2:
                    DoFileToFolder(true);
                    break;
                case 3:
                    DoDeleteFromFolder();
                    break;
            }
        }

        #endregion

        #region NavigationBar event handlers

        void GoToNextDocument(object sender, EventArgs args)
        {
            DocumentPreview = null;
            Document = null;
            DocumentId = null;

            bool previousAvailable, nextAvailable;
            DocumentPreview = GetNextDocumentId(DocumentPreview, out previousAvailable, out nextAvailable, true);

            if (DocumentPreview == null)
            {
                nextDocumentButtonItem.Enabled = false;
                previousDocumentButtonItem.Enabled = false;
                return;
            }

            Reload();
        }

        void GoToPreviousDocument(object sender, EventArgs args)
        {
            //TODO
        }

        void EditDocument(object sender, EventArgs args)
        {
            //TODO
        }

        void DoneButtonItem_Clicked(object sender, EventArgs e)
        {
            DismissViewController(true, null);
        }

        #endregion

        #region Actions

        async Task DoChangeReadStatus()
        {
            var isReadByCurrent = DocumentPreview.IsReadByCurrent;

            CommonConfig.Logger.Info($"Attempting to mark as {(isReadByCurrent ? "unread" : "read")} [documentPreview={DocumentPreview}]...");

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString(isReadByCurrent ? "marking_document_as_unread___" : "marking_document_as_read___"));

            try
            {
                await Managers.DocumentsManager.SetDocumentReadStatusAsync(DocumentPreview, Document, !isReadByCurrent, ServerConfig.SystemSettings.UserInfo.User);

                readByView.RefreshView();

                dismissAction();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Marking as {(isReadByCurrent ? "unread" : "read")}  failed [documentPreview={DocumentPreview}]", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        void DoAssignCategory()
        {
            //TODO
        }

        void DoShowUserActions(object sender, EventArgs e)
        {
            //TODO
        }

        void DoFileToFolder(bool move)
        {
            //TODO
        }

        void DoShowComments(object sender, EventArgs e)
        {
            //TODO
        }

        void DoFileToWorktray()
        {
            //TODO
        }

        void DoDeleteFromFolder()
        {
            //TODO
        }

        void DoReply(DocumentCreationModeFlag creationModeFlag)
        {
            var composeDocumentViewController = new ComposeDocumentViewController();
            composeDocumentViewController.PreviousDocumentId = DocumentPreview.Id;
            composeDocumentViewController.CreationModeFlag = creationModeFlag;
            composeDocumentViewController.PreviousDocumentFolderId = FolderId ?? Folder?.Id;
            composeDocumentViewController.PreviousDocumentDirection = DocumentPreview.Direction;

            var composeDocumentNavigationController = new UINavigationController(composeDocumentViewController);
            composeDocumentNavigationController.ModalPresentationStyle = UIModalPresentationStyle.PageSheet;
            PresentViewController(composeDocumentNavigationController, true, null);
        }

        #endregion

        #region ScrollView LayoutSubViews Action

        void HandleScrollViewLayoutSubviewsAction(UIScrollView scrollView)
        {
            //Used to keep the views before and after the content anchored to the scrollView
            var minimumVisibleX = scrollView.ContentOffset.X;

            var views = new UIView[] { stackViewBeforeContent, stackViewAfterContent };

            foreach (var item in views)
            {
                var actualFrame = item.Frame;
                actualFrame.X = minimumVisibleX;
                item.Frame = actualFrame;
            }
        }

        #endregion

    }

    public class ActionableLayoutScrollView : UIScrollView
    {
        public Action<UIScrollView> LayoutSubviewsAction
        {
            get;
            set;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            if (LayoutSubviewsAction != null)
            {
                LayoutSubviewsAction(this);
            }
        }
    }

    class AttachmentInteractionControllerDelegate : UIDocumentInteractionControllerDelegate
    {

        readonly UIViewController parentController;
        readonly AttachmentDescription attachmentDescription;

        public AttachmentInteractionControllerDelegate(UIViewController parentController, AttachmentDescription attachmentDescription)
        {
            this.attachmentDescription = attachmentDescription;
            this.parentController = parentController;
        }

        #region UIDocumentInteractionControllerDelegate overrides

        public override UIViewController ViewControllerForPreview(UIDocumentInteractionController controller)
        {
            return parentController;
        }

        public override UIView ViewForPreview(UIDocumentInteractionController controller)
        {
            return parentController.View;
        }

        public override CGRect RectangleForPreview(UIDocumentInteractionController controller)
        {
            return parentController.View.Frame;
        }

        public override void WillBeginSendingToApplication(UIDocumentInteractionController controller, string application)
        {
            CommonConfig.Logger.Info(string.Format("Sending atttachment to {0} app. [attachment={1}", attachmentDescription, application));
        }

        public override void DidEndSendingToApplication(UIDocumentInteractionController controller, string application)
        {
            CommonConfig.Logger.Info(string.Format("Sent atttachment to {0} app. [attachment={1}", attachmentDescription, application));
        }

        #endregion

    }
}
