using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Model.HubMessages;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentView;
using Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.Subviews;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;
using Mark5.Mobile.IOS.Ui.ViewControllers.MailViewerView;
using Mark5.Mobile.IOS.Utilities;
using UIKit;
using WebKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class DocumentViewController : AbstractViewController, ISecondaryViewController, IUIViewControllerRestoration
    {
        const int LargeAttachmentSizeInBytes = 20 * 1024 * 1024; // 20MB

        public bool Modal { get; set; }
        public Action OnComplete { get; set; }

        public bool Empty => document == null && documentPreview == null && folderId == null && folder == null && documentId == null;

        Guid failedDocumentToUploadGuid;
        int? folderId;
        Folder folder;
        int? documentId;
        DocumentPreview documentPreview;
        Document document;
        Guid notificationGuid;

        UIBarButtonItem doneButtonItem;
        UIBarButtonItem previousDocumentButtonItem;
        UIBarButtonItem nextDocumentButtonItem;
        UIBarButtonItem editDocumentButtonItem;

        UIScrollView mainScrollView;
        UIStackView stackViewBeforeContent;
        UIStackView stackViewAfterContent;

        FromView fromView;
        ToView toView;
        CcView ccView;
        BccView bccView;
        ExtraFieldsView extraFieldsView;
        OriginatorView originatorView;
        SubjectView subjectView;
        DateReceivedView dateReceivedView;
        PriorityView priorityView;
        ContentView contentView;
        AttachmentsView attachmentsListView;
        CreatorView creatorView;
        ReadByView readByView;
        ReferenceNumberView referenceNumberView;

        UIView backgroundView;
        UIActivityIndicatorView spinner;

        UIBarButtonItem flag;
        UIBarButtonItem fileTo;
        UIButton commentsButton;
        BadgeBarButtonItem comments;
        UIBarButtonItem replyActions;
        UIBarButtonItem userActions;

        UIToolbar toolbar;

        DocumentSubView[] subViews;

        UIDocumentInteractionController attachmentInteractionController;

        public delegate DocumentPreview GetPreviousDocumentPreviewDelegate(DocumentPreview documentPreview, out bool nextDocumentAvailable, out bool previousDocumentAvailable, bool scrollAndSelect = false);
        public delegate DocumentPreview GetNextDocumentPreviewDelegate(DocumentPreview documentPreview, out bool nextDocumentAvailable, out bool previousDocumentAvailable, bool scrollAndSelect = false);

        GetPreviousDocumentPreviewDelegate GetPreviousDocumentPreview { get; set; }
        GetNextDocumentPreviewDelegate GetNextDocumentPreview { get; set; }

        public event EventHandler<ReadStatusUpdatedEventArgs> ReadStatusUpdated;

        CancellationTokenSource readStatusCts;
        CancellationTokenSource loadCts;

        bool refreshDataOnAppear;

        #region UIViewController overrides

        public override void LoadView()
        {
            base.LoadView();

            InitNavigationBar();
            InitStackViews();
            InitSubViews();
            InitToolbar();
            InitBackgroundView();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Never;

            RestorationIdentifier = nameof(DocumentViewController);
            RestorationClass = Class;
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

            if (refreshDataOnAppear)
            {
                refreshDataOnAppear = false;
                RefreshData();
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            readStatusCts?.Cancel();
            readStatusCts = null;

            if (IsMovingFromParentViewController || IsBeingDismissed)
                OnComplete?.Invoke();

            DeinitializeHandlers();
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning("Received memory warning!");

            GC.Collect();
            base.DidReceiveMemoryWarning();
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

                nextDocumentButtonItem = new UIBarButtonItem
                {
                    Image = UIImage.FromBundle(Path.Combine("icons", "arrow-down.png")),
                    Enabled = false
                };

                previousDocumentButtonItem = new UIBarButtonItem
                {
                    Image = UIImage.FromBundle(Path.Combine("icons", "arrow-up.png")),
                    Enabled = false
                };

                editDocumentButtonItem = new UIBarButtonItem
                {
                    Image = UIImage.FromBundle(Path.Combine("icons", "pencil.png"))
                };

                var rightButtons = new UIBarButtonItem[2];
                rightButtons[0] = nextDocumentButtonItem;
                rightButtons[1] = previousDocumentButtonItem;
                NavigationItem.SetRightBarButtonItems(rightButtons, false);
            }
        }

        void InitStackViews()
        {
            mainScrollView = new UIScrollView
            {
                BackgroundColor = Theme.White,
                ShowsVerticalScrollIndicator = true,
                ShowsHorizontalScrollIndicator = false,
                ScrollEnabled = true,
                ScrollsToTop = true,
                UserInteractionEnabled = true,
                ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Always,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            View.AddSubview(mainScrollView);
            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(mainScrollView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(mainScrollView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(mainScrollView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(mainScrollView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1f, 0f),
            });

            stackViewBeforeContent = new UIStackView
            {
                Axis = UILayoutConstraintAxis.Vertical,
                Alignment = UIStackViewAlignment.Fill,
                Distribution = UIStackViewDistribution.Fill,
                Spacing = 0f,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            mainScrollView.AddSubview(stackViewBeforeContent);
            mainScrollView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(stackViewBeforeContent, NSLayoutAttribute.Top, NSLayoutRelation.Equal, mainScrollView, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(stackViewBeforeContent, NSLayoutAttribute.Left, NSLayoutRelation.Equal, mainScrollView, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(stackViewBeforeContent, NSLayoutAttribute.Width, NSLayoutRelation.Equal, mainScrollView, NSLayoutAttribute.Width, 1f, 0f)
            });

            contentView = new ContentView(DecidePolicyForNavigationAction);
            mainScrollView.AddSubview(contentView);
            mainScrollView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(contentView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, stackViewBeforeContent, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(contentView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, mainScrollView, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(contentView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, mainScrollView, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(contentView, NSLayoutAttribute.Width, NSLayoutRelation.GreaterThanOrEqual, mainScrollView, NSLayoutAttribute.Width, 1f, 0f)
            });

            stackViewAfterContent = new UIStackView
            {
                Axis = UILayoutConstraintAxis.Vertical,
                Alignment = UIStackViewAlignment.Fill,
                Distribution = UIStackViewDistribution.Fill,
                Spacing = 0f,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            mainScrollView.AddSubview(stackViewAfterContent);
            mainScrollView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(stackViewAfterContent, NSLayoutAttribute.Top, NSLayoutRelation.Equal, contentView, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(stackViewAfterContent, NSLayoutAttribute.Left, NSLayoutRelation.Equal, mainScrollView, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(stackViewAfterContent, NSLayoutAttribute.Width, NSLayoutRelation.Equal, mainScrollView, NSLayoutAttribute.Width, 1f, 0f),
                NSLayoutConstraint.Create(stackViewAfterContent, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, mainScrollView, NSLayoutAttribute.Bottom, 1f, 0f)
            });
        }

        void InitSubViews()
        {
            var viewsBeforeContent = new List<DocumentSubView>();
            var viewsAfterContent = new List<DocumentSubView>();

            viewsBeforeContent.Add(subjectView = new SubjectView());
            viewsBeforeContent.Add(fromView = new FromView());
            viewsBeforeContent.Add(toView = new ToView());
            viewsBeforeContent.Add(ccView = new CcView());
            viewsBeforeContent.Add(bccView = new BccView());
            viewsBeforeContent.Add(dateReceivedView = new DateReceivedView());
            viewsBeforeContent.Add(extraFieldsView = new ExtraFieldsView());
            viewsBeforeContent.Add(priorityView = new PriorityView());
            viewsBeforeContent.Add(attachmentsListView = new AttachmentsView());
            viewsAfterContent.Add(referenceNumberView = new ReferenceNumberView());
            viewsAfterContent.Add(readByView = new ReadByView());
            viewsAfterContent.Add(creatorView = new CreatorView());
            viewsAfterContent.Add(originatorView = new OriginatorView());

            viewsBeforeContent.ForEach(stackViewBeforeContent.AddArrangedSubview);
            viewsAfterContent.ForEach(stackViewAfterContent.AddArrangedSubview);

            subViews = viewsBeforeContent.Append(contentView).Concat(viewsAfterContent).ToArray();

            subViews.ForEach(v => v.UpdateVisibility());
        }

        void InitToolbar()
        {
            flag = new UIBarButtonItem
            {
                Image = UIImage.FromBundle(Path.Combine("icons", "flag.png")),
                Enabled = false
            };
            fileTo = new UIBarButtonItem
            {
                Image = UIImage.FromBundle(Path.Combine("icons", "worktray.png")),
                Enabled = false
            };
            commentsButton = new UIButton(UIButtonType.System)
            {
                Frame = new CGRect(0f, 0f, 25f, 25f),
                Enabled = false
            };
            commentsButton.SetImage(UIImage.FromBundle(Path.Combine("icons", "comments.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
            comments = new BadgeBarButtonItem(commentsButton)
            {
                BadgeBackgroundColor = Theme.Brown,
                Enabled = false
            };
            replyActions = new UIBarButtonItem
            {
                Image = UIImage.FromBundle(Path.Combine("icons", "reply.png")),
                Enabled = false
            };
            userActions = new UIBarButtonItem
            {
                Image = UIImage.FromBundle(Path.Combine("icons", "actions.png")),
                Enabled = false
            };
            toolbar = new UIToolbar
            {
                BarStyle = UIBarStyle.Default,
                Items = new[]
                {
                    flag,
                    new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                    replyActions,
                    new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                    fileTo,
                    new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                    comments,
                    new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                    userActions
                },
                BarTintColor = Theme.Gray,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            toolbar.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            toolbar.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            View.AddSubview(toolbar);
            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(toolbar, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 45f),
                NSLayoutConstraint.Create(toolbar, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(toolbar, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(toolbar, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1f, -(TabBarController?.TabBar?.Frame.Height ?? 0f))
            });

            AdditionalSafeAreaInsets = new UIEdgeInsets(0f, 0f, 45f, 0f);
        }

        void InitBackgroundView()
        {
            backgroundView = new UIView
            {
                BackgroundColor = UIColor.GroupTableViewBackgroundColor,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            View.AddSubview(backgroundView);
            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(backgroundView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(backgroundView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(backgroundView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(backgroundView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1f, 0f)
            });
            View.BringSubviewToFront(backgroundView);

            spinner = new UIActivityIndicatorView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.Gray
            };
            backgroundView.AddSubview(spinner);
            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(spinner, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, backgroundView, NSLayoutAttribute.CenterX, 1f, 0f),
                NSLayoutConstraint.Create(spinner, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, backgroundView, NSLayoutAttribute.CenterY, 1f, 0f)
            });
        }

        void InitializeHandlers()
        {
            fromView.RecipientTapped += RecipientsView_RecipientTapped;
            toView.RecipientTapped += RecipientsView_RecipientTapped;
            ccView.RecipientTapped += RecipientsView_RecipientTapped;
            bccView.RecipientTapped += RecipientsView_RecipientTapped;
            attachmentsListView.AttachmentTapped += AttachmentsList_AttachmentTapped;

            flag.Clicked += Flag_Clicked;
            fileTo.Clicked += FileTo_Clicked;
            replyActions.Clicked += ReplyActions_Clicked;
            commentsButton.TouchUpInside += CommentsButton_TouchUpInside;
            userActions.Clicked += UserActions_Clicked;

            if (Modal)
                doneButtonItem.Clicked += DoneButtonItem_Clicked;
            else
            {
                nextDocumentButtonItem.Clicked += NextDocumentButton_Clicked;
                previousDocumentButtonItem.Clicked += PreviousDocumentButton_Clicked;
                editDocumentButtonItem.Clicked += EditDocumentButtonItem_Clicked;
            }
        }

        void DeinitializeHandlers()
        {
            fromView.RecipientTapped -= RecipientsView_RecipientTapped;
            toView.RecipientTapped -= RecipientsView_RecipientTapped;
            ccView.RecipientTapped -= RecipientsView_RecipientTapped;
            bccView.RecipientTapped -= RecipientsView_RecipientTapped;
            attachmentsListView.AttachmentTapped -= AttachmentsList_AttachmentTapped;

            flag.Clicked -= Flag_Clicked;
            fileTo.Clicked -= FileTo_Clicked;
            replyActions.Clicked -= ReplyActions_Clicked;
            commentsButton.TouchUpInside -= CommentsButton_TouchUpInside;
            userActions.Clicked -= UserActions_Clicked;

            if (Modal)
                doneButtonItem.Clicked -= DoneButtonItem_Clicked;
            else
            {
                nextDocumentButtonItem.Clicked -= NextDocumentButton_Clicked;
                previousDocumentButtonItem.Clicked -= PreviousDocumentButton_Clicked;
                editDocumentButtonItem.Clicked -= EditDocumentButtonItem_Clicked;
            }
        }

        public void SetData(Folder folder, DocumentPreview documentPreview, GetNextDocumentPreviewDelegate getNextDocumentPreview, GetPreviousDocumentPreviewDelegate getPreviousDocumentPreview)
        {
            failedDocumentToUploadGuid = Guid.Empty;
            documentId = null;
            document = null;
            folderId = null;
            notificationGuid = default(Guid);

            this.documentPreview = documentPreview;
            this.folder = folder;
            GetNextDocumentPreview = getNextDocumentPreview;
            GetPreviousDocumentPreview = getPreviousDocumentPreview;
        }

        public void SetData(int documentId)
        {
            failedDocumentToUploadGuid = Guid.Empty;
            document = null;
            folder = null;
            folderId = null;
            documentPreview = null;
            GetNextDocumentPreview = null;
            GetPreviousDocumentPreview = null;
            notificationGuid = default(Guid);

            this.documentId = documentId;
        }

        public void SetData(int documentId, Guid notificationGuid)
        {
            failedDocumentToUploadGuid = Guid.Empty;
            document = null;
            folder = null;
            folderId = null;
            documentPreview = null;
            GetNextDocumentPreview = null;
            GetPreviousDocumentPreview = null;

            this.documentId = documentId;
            this.notificationGuid = notificationGuid;
        }

        public void SetData(DocumentPreview documentPreview, GetNextDocumentPreviewDelegate getNextDocumentPreview, GetPreviousDocumentPreviewDelegate getPreviousDocumentPreview)
        {
            failedDocumentToUploadGuid = Guid.Empty;
            document = null;
            documentId = null;
            folderId = null;
            folder = null;
            notificationGuid = default(Guid);

            this.documentPreview = documentPreview;
            GetNextDocumentPreview = getNextDocumentPreview;
            GetPreviousDocumentPreview = getPreviousDocumentPreview;
        }

        public void SetData(Folder folder, DocumentPreview documentPreview)
        {
            failedDocumentToUploadGuid = Guid.Empty;
            document = null;
            documentId = null;
            folderId = null;
            notificationGuid = default(Guid);

            this.documentPreview = documentPreview;
            this.folder = folder;
        }

        public void SetData(Guid failedDocumentToUploadGuid)
        {
            document = null;
            documentPreview = null;
            folder = null;
            documentId = null;
            GetPreviousDocumentPreview = null;
            GetNextDocumentPreview = null;
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
            GetPreviousDocumentPreview = null;
            GetNextDocumentPreview = null;
            notificationGuid = default(Guid);

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

            RefreshView();
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
                StartRefreshing();

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

                RefreshView();

                if (NavigationController != null)
                    mainScrollView.SetContentOffset(new CGPoint(-NavigationController.NavigationBar.Frame.Bottom, 0), false);

                EndRefreshing();

                MarkAsReadIfNecessary();
            }
            catch (Exception ex)
            {
                EndRefreshing(true);

                CommonConfig.Logger.Error($"Downloading document failed [folder.name={folder?.Name}, folder.id={folderId ?? folder?.Id}, documentId={documentId ?? documentPreview?.Id}]", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);

                if (Modal)
                    DismissViewController(true, null);
                else
                    NavigationController?.PopViewController(true);
            }
        }

        void StartRefreshing()
        {
            spinner.StartAnimating();

            View.BringSubviewToFront(backgroundView);
            UIView.Animate(0.25,
                () =>
                {
                    backgroundView.Alpha = 1f;
                    mainScrollView.Alpha = 0f;
                });
        }

        void EndRefreshing(bool withError = false)
        {
            spinner.StopAnimating();

            if (withError)
                return;

            View.SendSubviewToBack(backgroundView);
            UIView.Animate(0.25,
                () =>
                {
                    backgroundView.Alpha = 0f;
                    mainScrollView.Alpha = 1f;
                });
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

                    BeginInvokeOnMainThread(() =>
                    {
                        if (token.IsCancellationRequested)
                            return;
                        if (dp == null)
                            return;

                        readByView.RefreshView();
                        readByView.UpdateVisibility();

                        ReadStatusUpdated?.Invoke(this, new ReadStatusUpdatedEventArgs(dp));
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
            subViews.ForEach(v =>
            {
                v.Document = document;
                v.DocumentPreview = documentPreview;
                v.RefreshView();
            });

            subViews.ForEach(v => v.UpdateVisibility());

            RefreshNavigationBar();

            var enableBottomActions = failedDocumentToUploadGuid == Guid.Empty;
            if (enableBottomActions)
            {
                flag.Enabled = document != null;
                fileTo.Enabled = document != null;
                replyActions.Enabled = document != null;
                comments.BadgeValue = document?.Comments?.Count.ToString();
                comments.Enabled = document != null;
                commentsButton.Enabled = document != null;
                userActions.Enabled = document != null;
            }

            UIView.Animate(0.075d, stackViewBeforeContent.LayoutIfNeeded);
            UIView.Animate(0.1d, () => stackViewBeforeContent.Alpha = 1f);
            UIView.Animate(0.075d, stackViewAfterContent.LayoutIfNeeded);
            UIView.Animate(0.1d, () => stackViewAfterContent.Alpha = 1f);
        }

        public void RefreshNavigationBar()
        {
            if (Modal)
                return;

            bool _na;
            bool _pa;

            if (GetNextDocumentPreview != null)
                nextDocumentButtonItem.Enabled = GetNextDocumentPreview(documentPreview, out _na, out _pa) != null;
            else
                nextDocumentButtonItem.Enabled = false;

            if (GetPreviousDocumentPreview != null)
                previousDocumentButtonItem.Enabled = GetPreviousDocumentPreview(documentPreview, out _na, out _pa) != null;
            else
                previousDocumentButtonItem.Enabled = false;

            if (document == null || documentPreview.Direction != DocumentDirection.Draft)
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
                var path = await Managers.DocumentsManager.GetAttachmentAsync(attachmentDescription, document, false, SourceType.Local);

                if (string.IsNullOrWhiteSpace(path))
                {
                    if (PlatformConfig.Preferences.LargeAttachmentWarning &&
                        attachmentDescription.SizeInBytes > LargeAttachmentSizeInBytes &&
                        !await Dialogs.ShowYesNoDialogAsync(this, Localization.GetString("big_attachment_title"), string.Format(Localization.GetString("big_attachment_warning"), UI.PrettyFileSize(attachmentDescription.SizeInBytes))))
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
                    attachmentInteractionController = UIDocumentInteractionController.FromUrl(url);
                    attachmentInteractionController.Delegate = new DocumentInteractionControllerDelegate(this);

                    var previewSuccessful = attachmentInteractionController.PresentPreview(true);
                    if (!previewSuccessful)
                    {
                        CommonConfig.Logger.Info($"Failed to present preview for attachment. Presenting open with instead. [documentId={document.Id}, attachment={attachmentDescription}]");
                        var openInSuccessful = attachmentInteractionController.PresentOptionsMenu(View.Frame, View, true);
                        if (!openInSuccessful)
                        {
                            CommonConfig.Logger.Warning($"Failed to present open in view - there is no app that can open this type of attachment installed. [documentId={document.Id}, attachment={attachmentDescription}]");
                            await Dialogs.ShowConfirmDialogAsync(this, Localization.GetString("cannot_open_attachment_title"), Localization.GetString("cannot_open_attachment_content"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Failed to view attachment [document.Id={document.Id}, attachment.Name={attachmentDescription?.Name}", ex);

                dismissAction();
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
            finally
            {
                dismissAction();
            }
        }

        void RecipientsView_RecipientTapped(object sender, RecipientTappedEventArgs e)
        {
            PresentComposeViewWithPreconfiguredAddresses(new string[]
            {
                e.Recipent
            });
        }

        WKNavigationActionPolicy DecidePolicyForNavigationAction(WKNavigationAction navigationAction)
        {
            if (navigationAction.NavigationType == WKNavigationType.LinkActivated || navigationAction.NavigationType == WKNavigationType.BackForward || navigationAction.NavigationType == WKNavigationType.FormSubmitted || navigationAction.NavigationType == WKNavigationType.FormResubmitted)
            {
                if (navigationAction.Request.Url.Scheme == "mailto")
                {
                    var address = navigationAction.Request.Url.ResourceSpecifier;
                    PresentComposeViewWithPreconfiguredAddresses(new string[] { address });
                }
                else
                {
                    Integration.OpenLink(navigationAction.Request.Url, async () => await Dialogs.ShowConfirmDialogAsync(this, Localization.GetString("unable_open_link_title"), Localization.GetString("unable_open_link_content") + navigationAction.Request.Url.Scheme));
                }

                return WKNavigationActionPolicy.Cancel;
            }

            if (navigationAction.NavigationType == WKNavigationType.Reload)
                return WKNavigationActionPolicy.Cancel;

            return WKNavigationActionPolicy.Allow;
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

        #endregion

        #region Toolbar event handlers

        async void Flag_Clicked(object sender, EventArgs e)
        {
            var isRead = documentPreview.IsReadByCurrent;
            var flagListStrings = new string[]
            {
                Localization.GetString(isRead ? "mark_as_unread" : "mark_as_read"),
                Localization.GetString("categories")
            };

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
            var replyListStrings = new string[]
            {
                Localization.GetString("reply"),
                Localization.GetString("reply_all"),
                Localization.GetString("forward"),
                Localization.GetString("copy_to_new")};

            var result = await Dialogs.ShowListDialogAsync(this, null, replyListStrings, replyActions);

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
            var result = await Dialogs.ShowListDialogAsync(this, Localization.GetString("select_priority"), priorityStrings.ToArray(), barButtonItem);

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
                await Managers.DocumentsManager.SetDocumentsPriorityAsync(new List<DocumentPreview>
                    {
                        documentPreview
                    },
                    priority);

                UpdatePriority();

                dismissAction();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Error while setting priority for document [documentId={document.Id}]", ex);
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        public void UpdatePriority()
        {
            priorityView.RefreshView();
            priorityView.UpdateVisibility();
        }

        void FileTo_Clicked(object sender, EventArgs e)
        {
            var eas = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);

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
            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_folder"),
                UIAlertActionStyle.Default,
                a =>
                {
                    var vc = new CopyMoveToFolderListViewController(new List<IBusinessEntity>
                    {
                        document
                    });
                    PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
                }));

            if (folder?.InternalType == FolderInternalType.FilterView || folder?.InternalType == FolderInternalType.Static || folder?.InternalType == FolderInternalType.Worktray)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("move_to_folder"),
                    UIAlertActionStyle.Default,
                    a =>
                    {
                        var vc = new CopyMoveToFolderListViewController(new List<IBusinessEntity>
                            {
                                document
                            },
                            folder);
                        PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
                    }));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("set_priority"), UIAlertActionStyle.Default, a => ShowPriorityActionSheet((UIBarButtonItem)sender)));

            if (folder?.InternalType == FolderInternalType.FilterView || folder?.InternalType == FolderInternalType.Static || folder?.InternalType == FolderInternalType.Worktray)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete_from_folder"), UIAlertActionStyle.Default, RemoveFromFolder));

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator || ServerConfig.SystemSettings.DocumentsModuleInfo.Permissions.DeleteAllowed)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete"), UIAlertActionStyle.Destructive, Delete));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, null));

            if (eas.PopoverPresentationController != null)
                eas.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate((UIBarButtonItem)sender);

            PresentViewController(eas, true, null);
        }

        #endregion

        #region NavigationBar event handlers

        void NextDocumentButton_Clicked(object sender, EventArgs args)
        {
            document = null;
            documentId = null;

            documentPreview = GetNextDocumentPreview(documentPreview, out bool previousAvailable, out bool nextAvailable, true);

            if (documentPreview == null)
            {
                nextDocumentButtonItem.Enabled = false;
                previousDocumentButtonItem.Enabled = false;
                return;
            }

            RefreshData();
        }

        void PreviousDocumentButton_Clicked(object sender, EventArgs args)
        {
            document = null;
            documentId = null;

            documentPreview = GetPreviousDocumentPreview(documentPreview, out bool previousAvailable, out bool nextAvailable, true);

            if (documentPreview == null)
            {
                nextDocumentButtonItem.Enabled = false;
                previousDocumentButtonItem.Enabled = false;
                return;
            }

            RefreshData();
        }

        void EditDocumentButtonItem_Clicked(object sender, EventArgs e)
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

        void DoneButtonItem_Clicked(object sender, EventArgs e)
        {
            DismissViewController(true, null);
        }

        #endregion

        #region Actions

        async Task DoChangeReadStatus()
        {
            var isReadByCurrent = documentPreview.IsReadByCurrent;

            CommonConfig.Logger.Info($"Attempting to mark as {(isReadByCurrent ? "unread" : "read")} [documentPreview={documentPreview}]...");

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString(isReadByCurrent ? "marking_document_as_unread___" : "marking_document_as_read___"));

            try
            {
                await Managers.DocumentsManager.SetDocumentReadStatusAsync(documentPreview, document, !isReadByCurrent, ServerConfig.SystemSettings.UserInfo.User);

                readByView.RefreshView();

                ReadStatusUpdated?.Invoke(this, new ReadStatusUpdatedEventArgs(documentPreview));

                dismissAction();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Marking as {(isReadByCurrent ? "unread" : "read")}  failed [documentPreview={documentPreview}]", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        void DoAssignCategory()
        {
            var vc = new CategoriesListViewController
            {
                BusinessEntityPreview = documentPreview
            };
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        async void UserActions_Clicked(object sender, EventArgs e)
        {
            var actionLinksListString = new string[]
            {
                Localization.GetString("actions"),
                Localization.GetString("links")
            };

            var result = await Dialogs.ShowListDialogAsync(this, null, actionLinksListString, userActions);

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

        void CommentsButton_TouchUpInside(object sender, EventArgs e)
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

            var hasAttachments = document.Attachments.Any();

            string[] modes = null;

            if (hasAttachments)
            {
                modes = new[] { Localization.GetString("copy_to_new_addresses"),
                Localization.GetString("copy_to_new_text_and_attachments"), Localization.GetString("copy_to_new_attachments") };
            }
            else
            {
                modes = new[] { Localization.GetString("copy_to_new_addresses"),
                Localization.GetString("copy_to_new_text") };
            }

            var result = await Dialogs.ShowListDialogAsync(this, Localization.GetString("copy_to_new_title"), modes, replyActions);

            if (result < 0)
                return;

            CopyToNewOption option = CopyToNewOption.None;
            switch (result)
            {
                case 0:
                    option = CopyToNewOption.KeepOnlyAddresses;
                    break;
                case 1:
                    option = CopyToNewOption.KeepTextAndAttachments;
                    break;
                case 2:
                    option = CopyToNewOption.KeepOnlyAttachments;
                    break;
            }

            var vc = new ComposeDocumentViewController
            {
                DocumentCreationModeFlag = DocumentCreationModeFlag.New,
                CopyToNewOption = option,
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
            var result = await Dialogs.ShowYesNoDialogAsync(this, Localization.GetString("delete_from_folder"), Localization.GetString("confirm_delete_from_folder_document"));

            if (!result)
                return;

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("deleting_from_folder___"));

            try
            {
                CommonConfig.Logger.Info($"Attempting to remove documnet from folder [documentId={document.Id}, folderId={folder.Id}]");

                await Managers.CommonActionsManager.RemoveFromFolder(new List<IBusinessEntity> { document }, folder);

                CommonConfig.MessengerHub.Publish(new EntityRemovedFromFolderMessage(this,
                    ObjectType.Document,
                    folder.Id,
                    new List<int> { document.Id }));

                dismissAction();

                if (SplitViewController != null && !SplitViewController.Collapsed)
                    ClearData();
                else
                    NavigationController.PopViewController(true);
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Error while removing document from folder [documentId={document.Id}, folderId={folder.Id}]", ex);
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        async void Delete(UIAlertAction a)
        {
            var result = await Dialogs.ShowYesNoDialogAsync(this, Localization.GetString("delete"), Localization.GetString("confirm_delete_document"));

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

                CommonConfig.MessengerHub.Publish(new EntityDeletedMessage(this,
                    ObjectType.Document,
                    new List<int>
                    {
                        document.Id
                    }));

                dismissAction();

                if (SplitViewController != null && !SplitViewController.Collapsed)
                    ClearData();
                else
                    NavigationController.PopViewController(true);
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Error while deleting document [documentId={document.Id}]", ex);
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        #endregion

        #region State restoration

        public override void EncodeRestorableState(NSCoder coder)
        {
            base.EncodeRestorableState(coder);

            coder.Encode(Modal, "modal");

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
            if (coder.DecodeBool("modal"))
                return null;
            
            return new DocumentViewController();
        }

        #endregion

    }

    public class ReadStatusUpdatedEventArgs : EventArgs
    {
        public DocumentPreview DocumentPreview { get; }

        public ReadStatusUpdatedEventArgs(DocumentPreview documentPreview)
        {
            DocumentPreview = documentPreview;
        }
    }
}