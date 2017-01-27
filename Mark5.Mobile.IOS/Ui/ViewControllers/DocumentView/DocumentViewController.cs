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
using System.Threading;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.Subviews;
using Mark5.Mobile.IOS.Utilities;
using UIKit;
using WebKit;

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

        public bool Modal { get; set; }

        public GetPreviousDocumentPreviewDelegate GetPreviousDocumentPreview { get; set; }
        public GetNextDocumentPreviewDelegate GetNextDocumentPreview { get; set; }

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
        UIBarButtonItem editDocumentButtonItem;

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

        public event EventHandler<ReadStatusUpdatedEventArgs> ReadStatusUpdated;

        CancellationTokenSource setReadStatusCancellationTokenSource;

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

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public override async void ViewDidAppear(bool animated)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void 
        {
            base.ViewDidAppear(animated);

            await RefreshData();

            CorrectScrollViewInsets();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            setReadStatusCancellationTokenSource?.Cancel();
            setReadStatusCancellationTokenSource = null;

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

                editDocumentButtonItem = new UIBarButtonItem();
                editDocumentButtonItem.Image = UIImage.FromBundle(Path.Combine("Icons", "pencil.png"));
                editDocumentButtonItem.Enabled = true;

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
                ShowsHorizontalScrollIndicator = false,
                ScrollEnabled = true,
                ScrollsToTop = true,
                UserInteractionEnabled = true,
                ClipsToBounds = false,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            mainScrollView.LayoutSubviewsAction = HandleScrollViewLayoutSubviewsAction;
            View.AddSubview(mainScrollView);
            View.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(mainScrollView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(mainScrollView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1.0f, 0.0f),
                NSLayoutConstraint.Create(mainScrollView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(mainScrollView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1.0f, 0.0f)
                });

            stackViewBeforeContent = new UIStackView
            {
                BackgroundColor = UIColor.White,
                Axis = UILayoutConstraintAxis.Vertical,
                Alignment = UIStackViewAlignment.Fill,
                Distribution = UIStackViewDistribution.Fill,
                Spacing = 0.0f,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            mainScrollView.AddSubview(stackViewBeforeContent);
            View.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(stackViewBeforeContent, NSLayoutAttribute.Top, NSLayoutRelation.Equal, mainScrollView, NSLayoutAttribute.Top, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(stackViewBeforeContent, NSLayoutAttribute.Left, NSLayoutRelation.Equal, mainScrollView, NSLayoutAttribute.Left, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(stackViewBeforeContent, NSLayoutAttribute.Width, NSLayoutRelation.Equal, mainScrollView, NSLayoutAttribute.Width, 1.0f, 0.0f)
                });

            contentView = new ContentView(mainScrollView, DecidePolicyForNavigationAction);
            mainScrollView.AddSubview(contentView);
            mainScrollView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(contentView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, stackViewBeforeContent, NSLayoutAttribute.Bottom, 1.0f, 0.0f),
                NSLayoutConstraint.Create(contentView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, mainScrollView, NSLayoutAttribute.Left, 1.0f, 0.0f),
                NSLayoutConstraint.Create(contentView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, mainScrollView, NSLayoutAttribute.Right, 1.0f, 0.0f),
                NSLayoutConstraint.Create(contentView, NSLayoutAttribute.Width, NSLayoutRelation.GreaterThanOrEqual, mainScrollView, NSLayoutAttribute.Width, 1.0f, 0.0f)
            });

            stackViewAfterContent = new UIStackView
            {
                BackgroundColor = UIColor.White,
                Axis = UILayoutConstraintAxis.Vertical,
                Alignment = UIStackViewAlignment.Fill,
                Distribution = UIStackViewDistribution.Fill,
                Spacing = 0.0f,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            mainScrollView.AddSubview(stackViewAfterContent);
            View.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(stackViewAfterContent, NSLayoutAttribute.Top, NSLayoutRelation.Equal, contentView, NSLayoutAttribute.Bottom, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(stackViewAfterContent, NSLayoutAttribute.Left, NSLayoutRelation.Equal, mainScrollView, NSLayoutAttribute.Left, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(stackViewAfterContent, NSLayoutAttribute.Width, NSLayoutRelation.Equal, mainScrollView, NSLayoutAttribute.Width, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(stackViewAfterContent, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, mainScrollView, NSLayoutAttribute.Bottom, 1.0f, 0.0f)
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

            comments = new BadgeBarButtonItem(commentsButton);
            comments.BadgeBackgroundColor = Theme.Brown;
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
                userActions
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
                    toolbarBottomConstraint
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
            fromView.RecipentTapped += RecipeintsView_RecipientTapped;
            toView.RecipentTapped += RecipeintsView_RecipientTapped;
            ccView.RecipentTapped += RecipeintsView_RecipientTapped;
            bccView.RecipentTapped += RecipeintsView_RecipientTapped;
            attachmentsListView.AttachmentTapped += AttachmentsList_AttachmentTapped;

            //Toolbar
            flag.Clicked += Flag_Clicked;
            fileTo.Clicked += FileTo_Clicked;
            replyActions.Clicked += ReplyActions_Clicked;
            userActions.Clicked += UserActions_Clicked;
            commentsButton.TouchUpInside += CommentsButton_TouchUpInside;

            //NavigationBar
            if (Modal)
            {
                doneButtonItem.Clicked += DoneButtonItem_Clicked;
            }
            else
            {
                nextDocumentButtonItem.Clicked += NextDocumentButton_Clicked;
                previousDocumentButtonItem.Clicked += PreviousDocumentButton_Clicked;
                editDocumentButtonItem.Clicked += EditDocumentButtonItem_Clicked;
            }
        }

        void DeInitializeHandlers()
        {
            //Subviews
            fromView.RecipentTapped -= RecipeintsView_RecipientTapped;
            toView.RecipentTapped -= RecipeintsView_RecipientTapped;
            ccView.RecipentTapped -= RecipeintsView_RecipientTapped;
            bccView.RecipentTapped -= RecipeintsView_RecipientTapped;
            attachmentsListView.AttachmentTapped -= AttachmentsList_AttachmentTapped;

            //Toolbar
            flag.Clicked -= Flag_Clicked;
            fileTo.Clicked -= FileTo_Clicked;
            replyActions.Clicked -= ReplyActions_Clicked;
            userActions.Clicked -= UserActions_Clicked;
            commentsButton.TouchUpInside -= CommentsButton_TouchUpInside;

            //NavigationBar
            if (Modal)
            {
                doneButtonItem.Clicked -= DoneButtonItem_Clicked;
            }
            else
            {
                nextDocumentButtonItem.Clicked -= NextDocumentButton_Clicked;
                previousDocumentButtonItem.Clicked -= PreviousDocumentButton_Clicked;
            }
        }

        #endregion

        #region Refresh methods

        public async Task Reload()
        {
            Reset();
            await RefreshData();
        }

        void Reset()
        {
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
                MarkAsReadIfNecessary();
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

        void MarkAsReadIfNecessary()
        {
            setReadStatusCancellationTokenSource?.Cancel();
            setReadStatusCancellationTokenSource = new CancellationTokenSource();

            Task.Run(async () =>
            {
                var f = Folder;
                var d = Document;
                var dp = DocumentPreview;
                var token = setReadStatusCancellationTokenSource.Token;

                try
                {
                    if (dp.IsReadByCurrent)
                    {
                        return;
                    }

                    var delaySeconds = PlatformConfig.Preferences.MarkAsReadDelaySeconds;
                    if (delaySeconds < 0) return;

                    await Task.Delay(delaySeconds * 1000);

                    if (token.IsCancellationRequested) return;
                    await Managers.DocumentsManager.SetDocumentReadStatusAsync(dp, d, true, ServerConfig.SystemSettings.UserInfo.User);

                    BeginInvokeOnMainThread(() =>
                    {
                        if (token.IsCancellationRequested) return;
                        if (dp == null) return;

                        readByView.RefreshView();
                        readByView.UpdateVisibility();

                        ReadStatusUpdated(this, new ReadStatusUpdatedEventArgs(dp));
                    });
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error($"Marking document as read failed [folder.name={f?.Name}, folder.id={f?.Id}, documentPreviewId={dp?.Id}]", ex);
                }
            });
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

            RefreshNavigationBar();

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

        public void RefreshNavigationBar()
        {
            if (Modal)
            {
                return;
            }

            bool _na;
            bool _pa;

            if (GetNextDocumentPreview != null)
            {
                nextDocumentButtonItem.Enabled = GetNextDocumentPreview(DocumentPreview, out _na, out _pa) != null;
            }
            else
            {
                nextDocumentButtonItem.Enabled = false;
            }

            if (GetPreviousDocumentPreview != null)
            {
                previousDocumentButtonItem.Enabled = GetPreviousDocumentPreview(DocumentPreview, out _na, out _pa) != null;
            }
            else
            {
                previousDocumentButtonItem.Enabled = false;
            }

            if (Document == null || (DocumentPreview.Direction != DocumentDirection.Draft))
            {
                var rightButtons = new UIBarButtonItem[2];
                rightButtons[0] = nextDocumentButtonItem;
                rightButtons[1] = previousDocumentButtonItem;
                NavigationItem.SetRightBarButtonItems(rightButtons, true);
            }
            else
            {
                var rightButtons = new UIBarButtonItem[3];
                rightButtons[0] = nextDocumentButtonItem;
                rightButtons[1] = previousDocumentButtonItem;
                rightButtons[2] = editDocumentButtonItem;
                NavigationItem.SetRightBarButtonItems(rightButtons, true);
            }
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


                var url = NSUrl.FromFilename(path);
                attachmentInteractionController = UIDocumentInteractionController.FromUrl(url);
                attachmentInteractionController.Delegate = new AttachmentInteractionControllerDelegate(this, attachmentDescription);

                var previewSuccessful = attachmentInteractionController.PresentPreview(true); //TODO check on the phone if it works, because it does not on the simulator

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
                CommonConfig.Logger.Error($"Failed to view attachment [document.Id={Document.Id}, attachment.Id={attachmentDescription?.Id}, attachment.Name={attachmentDescription?.Name}", ex);

                dismissAction();
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
            finally
            {
                dismissAction();
            }
        }

        void RecipeintsView_RecipientTapped(object sender, RecipentTappedEventArgs e)
        {
            //TODO
        }

        WKNavigationActionPolicy DecidePolicyForNavigationAction(WKNavigationAction navigationAction)
        {
            if (navigationAction.NavigationType == WKNavigationType.LinkActivated
            || navigationAction.NavigationType == WKNavigationType.BackForward
            || navigationAction.NavigationType == WKNavigationType.FormSubmitted
            || navigationAction.NavigationType == WKNavigationType.FormResubmitted)
            {
                if (navigationAction.Request.Url.Scheme == "mailto")
                {
                    var address = navigationAction.Request.Url.ResourceSpecifier;
                    //TODO open compose view controller with address
                }
                else
                {
                    Integration.OpenLink(navigationAction.Request.Url,
                                                      async () => await Dialogs.ShowConfirmDialogAsync(this, Localization.GetString("unable_open_link_title"), Localization.GetString("unable_open_link_content") + navigationAction.Request.Url.Scheme));
                }

                return WKNavigationActionPolicy.Cancel;
            }

            if (navigationAction.NavigationType == WKNavigationType.Reload)
            {
                return WKNavigationActionPolicy.Cancel;
            }

            return WKNavigationActionPolicy.Allow;
        }

        #endregion

        #region Toolbar event handlers

        async void Flag_Clicked(object sender, EventArgs e)
        {
            var isRead = DocumentPreview.IsReadByCurrent;
            var flagListStrings = new string[] { Localization.GetString(isRead ? "mark_as_unread" : "mark_as_read"),
                    Localization.GetString("categories") };

            var result = await Dialogs.ShowListDialogAsync(this, null, flagListStrings, flag);

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

        async void ReplyActions_Clicked(object sender, EventArgs e)
        {
            var replyListStrings = new string[] { Localization.GetString("reply"),
                Localization.GetString("reply_all"),
                Localization.GetString("forward")};

            var result = await Dialogs.ShowListDialogAsync(this, null, replyListStrings, replyActions);

            if (result < 0)
                return;

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
            var fileToListStrings = new string[] { Localization.GetString("copy_to_worktray"),
                Localization.GetString("copy_to_folder"),
                Localization.GetString("move_to_folder"),
                Localization.GetString("delete_from_folder")};

            var result = await Dialogs.ShowListDialogAsync(this, null, fileToListStrings, fileTo);

            if (result < 0)
                return;

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

        async void NextDocumentButton_Clicked(object sender, EventArgs args)
        {
            Document = null;
            DocumentId = null;

            bool previousAvailable, nextAvailable;
            DocumentPreview = GetNextDocumentPreview(DocumentPreview, out previousAvailable, out nextAvailable, true);

            if (DocumentPreview == null)
            {
                nextDocumentButtonItem.Enabled = false;
                previousDocumentButtonItem.Enabled = false;
                return;
            }

            await Reload();
        }

        async void PreviousDocumentButton_Clicked(object sender, EventArgs args)
        {
            Document = null;
            DocumentId = null;

            bool previousAvailable, nextAvailable;
            DocumentPreview = GetPreviousDocumentPreview(DocumentPreview, out previousAvailable, out nextAvailable, true);

            if (DocumentPreview == null)
            {
                nextDocumentButtonItem.Enabled = false;
                previousDocumentButtonItem.Enabled = false;
                return;
            }

            await Reload();
        }

        void EditDocumentButtonItem_Clicked(object sender, EventArgs e)
        {
            var composeDocumentViewController = new ComposeDocumentViewController()
            {
                PreviousDocumentPreview = DocumentPreview,
                PreviousDocument = Document,
                CreationModeFlag = DocumentCreationModeFlag.Edit,
                PreviousDocumentDirection = DocumentPreview.Direction,
                PreviousDocumentFolderId = Folder.Id
            };

            var composeDocumentNavigationController = new UINavigationController(composeDocumentViewController);
            composeDocumentNavigationController.ModalPresentationStyle = UIModalPresentationStyle.PageSheet;
            PresentViewController(composeDocumentNavigationController, true, null);
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

                ReadStatusUpdated(this, new ReadStatusUpdatedEventArgs(DocumentPreview));

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

        void UserActions_Clicked(object sender, EventArgs e)
        {
            //TODO
        }

        void DoFileToFolder(bool move)
        {
            //TODO
        }

        void CommentsButton_TouchUpInside(object sender, EventArgs e)
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
            var composeDocumentViewController = new ComposeDocumentViewController()
            {
                PreviousDocumentId = DocumentPreview.Id,
                CreationModeFlag = creationModeFlag,
                PreviousDocumentFolderId = FolderId ?? Folder?.Id,
                PreviousDocumentDirection = DocumentPreview.Direction,
                PreviousDocument = Document,
                PreviousDocumentPreview = DocumentPreview
            };

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

    public class ReadStatusUpdatedEventArgs : EventArgs
    {
        public DocumentPreview DocumentPreview
        {
            get;
            private set;
        }

        public ReadStatusUpdatedEventArgs(DocumentPreview documentPreview)
        {
            DocumentPreview = documentPreview;
        }
    }
}
