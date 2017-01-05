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
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.Common.StackView;
using Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.Subviews;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{

    public class DocumentViewController : StackViewController, ISecondaryViewController
    {
        public bool Empty { get { return true; } }

        public int? FolderId { get; set; }
        public Folder Folder { get; set; }
        public int? DocumentId { get; set; }
        public DocumentPreview DocumentPreview { get; set; }
        public Document Document { get; set; }
        public Guid NotificationGuid { get; set; }

        const long MaxAttachmentSize = 25 * 1024 * 1024;

        UIBarButtonItem doneButtonItem;
        UIBarButtonItem previousDocumentButtonItem;
        UIBarButtonItem nextDocumentButtonItem;
        UIBarButtonItem editDocumentButtonItem;

        FromView from;
        ToView to;
        CcView cc;
        BccView bcc;
        OriginatorView originator;
        SubjectView subject;
        DateReceivedView dateReceived;
        PriorityView priority;
        ContentView content;
        AttachmentsView attachmentsList;
        CreatorView creator;
        ReadByView readBy;
        ReferenceNumberView referenceNumber;

        UIBarButtonItem flag;
        UIBarButtonItem fileTo;
        UIButton commentsButton;
        BadgeBarButtonItem comments;
        UIBarButtonItem replyActions;
        UIBarButtonItem userActions;

        UIToolbar toolbar;
        NSLayoutConstraint toolbarBottomConstraint;

        static UIBarButtonItem FlexibleSpace
        {
            get
            {
                return new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
            }
        }

        #region UIViewController overrides

        public override void LoadView()
        {
            base.LoadView();

            InitNavigationBar();
            InitSubViews();
            InitToolbar();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            InitializeHandlers();
            CorrectToolbar();


            RefreshData();
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

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
            nextDocumentButtonItem = null;
            previousDocumentButtonItem = null;

            var rightButtons = new UIBarButtonItem[2];

            nextDocumentButtonItem = new UIBarButtonItem();
            nextDocumentButtonItem.Image = UIImage.FromBundle(Path.Combine("Icons", "arrow-down.png"));
            nextDocumentButtonItem.Clicked += (sender, e) => GoToNextDocument();
            nextDocumentButtonItem.Enabled = false;
            rightButtons[0] = nextDocumentButtonItem;

            previousDocumentButtonItem = new UIBarButtonItem();
            previousDocumentButtonItem.Image = UIImage.FromBundle(Path.Combine("Icons", "arrow-up.png"));
            previousDocumentButtonItem.Clicked += (sender, e) => GoToPreviousDocument();
            previousDocumentButtonItem.Enabled = false;
            rightButtons[1] = previousDocumentButtonItem;

            editDocumentButtonItem = new UIBarButtonItem();
            editDocumentButtonItem.Image = UIImage.FromBundle(Path.Combine("Icons", "pencil.png"));
            editDocumentButtonItem.Clicked += (sender, e) => EditDocument();
            editDocumentButtonItem.Enabled = true;

            NavigationItem.SetRightBarButtonItems(rightButtons, false);
        }

        void InitSubViews()
        {
            var views = new List<DocumentSubView>();

            from = new FromView();
            views.Add(from);

            to = new ToView();
            views.Add(to);

            cc = new CcView();
            views.Add(cc);

            bcc = new BccView();
            views.Add(bcc);

            subject = new SubjectView();
            views.Add(subject);

            dateReceived = new DateReceivedView();
            views.Add(dateReceived);

            priority = new PriorityView();
            views.Add(priority);

            content = new ContentView();
            views.Add(content);

            attachmentsList = new AttachmentsView();
            views.Add(attachmentsList);

            referenceNumber = new ReferenceNumberView();
            views.Add(referenceNumber);

            readBy = new ReadByView();
            views.Add(readBy);

            creator = new CreatorView();
            views.Add(creator);

            originator = new OriginatorView();
            views.Add(originator);

            AddArrangedViewsWithSeparators(views);

            StackView.ArrangedSubviews.OfType<DocumentSubView>().ForEach(v => v.UpdateVisibility());

            StackView.Alpha = 0.0f;
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
            ScrollView.ContentInset = new UIEdgeInsets(ScrollView.ContentInset.Top, 0.0f, ScrollView.ContentInset.Bottom + toolbar.Frame.Height, 0.0f);
            ScrollView.ScrollIndicatorInsets = new UIEdgeInsets(ScrollView.ScrollIndicatorInsets.Top, 0.0f, ScrollView.ScrollIndicatorInsets.Bottom + toolbar.Frame.Height, 0.0f);
        }

        void InitializeHandlers()
        {
            from.RecipentTapped += HandleRecipentTapped;
            to.RecipentTapped += HandleRecipentTapped;
            cc.RecipentTapped += HandleRecipentTapped;
            bcc.RecipentTapped += HandleRecipentTapped;

            flag.Clicked += Flag_Clicked;
            fileTo.Clicked += FileTo_Clicked;
            replyActions.Clicked += ReplyActions_Clicked;
            userActions.Clicked += DoShowUserActions;
            commentsButton.TouchUpInside += DoShowComments;
        }

        void DeInitializeHandlers()
        {
            from.RecipentTapped -= HandleRecipentTapped;
            to.RecipentTapped -= HandleRecipentTapped;
            cc.RecipentTapped -= HandleRecipentTapped;
            bcc.RecipentTapped -= HandleRecipentTapped;

            flag.Clicked -= Flag_Clicked;
            fileTo.Clicked -= FileTo_Clicked;
            replyActions.Clicked -= ReplyActions_Clicked; //TODO uniform naming
            userActions.Clicked -= DoShowUserActions;
            commentsButton.TouchUpInside -= DoShowComments;
        }

        #endregion

        #region Refresh methods

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

                //TODO need to close the view controller eventually
            }
        }

        void RefreshView()
        {
            if (Document == null | DocumentPreview == null)
            {
                return;
            }

            Title = DocumentPreview.Subject;

            StackView.ArrangedSubviews.OfType<DocumentSubView>().ForEach(v =>
            {
                v.Document = Document;
                v.DocumentPreview = DocumentPreview;
                v.RefreshView();
            });

            StackView.ArrangedSubviews.OfType<DocumentSubView>().ForEach(v => v.UpdateVisibility());
            CorrectSeparators();

            UIView.Animate(0.075d, StackView.LayoutIfNeeded);
            UIView.Animate(0.1d, () => StackView.Alpha = 1.0f);
        }

        #endregion

        #region Handlers

        void HandleRecipentTapped(object sender, RecipentTappedEventArgs e)
        {
            //TODO
        }

        #endregion

        #region Actions

        void Flag_Clicked(object sender, EventArgs e) //TODO use dialogs? what about the popover
        {
            var isRead = DocumentPreview.IsReadByCurrent;

            var actions = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            actions.AddAction(UIAlertAction.Create(string.Format("Mark as {0}", isRead ? "unread" : "read"), UIAlertActionStyle.Default, a => DoChangeReadStatus()));
            actions.AddAction(UIAlertAction.Create("Categories", UIAlertActionStyle.Default, a => DoAssignCategory()));
            actions.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));
            if (actions.PopoverPresentationController != null)
            {
                actions.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate(flag);
            }
            PresentViewController(actions, true, null);
        }

        void ReplyActions_Clicked(object sender, EventArgs e)
        {
            var replyActionSheet = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            replyActionSheet.AddAction(UIAlertAction.Create("Reply", UIAlertActionStyle.Default, a => DoReply(DocumentCreationModeFlag.Reply)));
            replyActionSheet.AddAction(UIAlertAction.Create("Reply all", UIAlertActionStyle.Default, a => DoReply(DocumentCreationModeFlag.ReplyAll)));
            replyActionSheet.AddAction(UIAlertAction.Create("Forward", UIAlertActionStyle.Default, a => DoReply(DocumentCreationModeFlag.Forward)));
            replyActionSheet.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));
            if (replyActionSheet.PopoverPresentationController != null)
            {
                replyActionSheet.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate(replyActions);
            }
            PresentViewController(replyActionSheet, true, null);
        }

        void FileTo_Clicked(object sender, EventArgs e)
        {
            var fileToActionSheet = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            fileToActionSheet.AddAction(UIAlertAction.Create("Copy to worktray", UIAlertActionStyle.Default, a => DoFileToWorktray()));
            fileToActionSheet.AddAction(UIAlertAction.Create("Copy to folder", UIAlertActionStyle.Default, a => DoFileToFolder(false)));
            //fileToActionSheet.AddAction(UIAlertAction.Create("Move to folder", UIAlertActionStyle.Default, a => DoFileToFolder(true)));
            fileToActionSheet.AddAction(UIAlertAction.Create("Delete from folder", UIAlertActionStyle.Destructive, a => DoDeleteFromFolder()));
            fileToActionSheet.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));
            if (fileToActionSheet.PopoverPresentationController != null)
            {
                fileToActionSheet.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate(fileTo);
            }
            PresentViewController(fileToActionSheet, true, null);
        }


        void DoChangeReadStatus()
        {
            //TODO
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

        void GoToNextDocument()
        {
            //TODO
        }

        void GoToPreviousDocument()
        {
            //TODO
        }

        void EditDocument()
        {
            //TODO
        }

        #endregion
    }
}
