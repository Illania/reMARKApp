//
// Project: Mark5.Mobile.IOS
// File: CommentsListViewController.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Model.HubMessages;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{

    public class CommentsListViewController : AbstractViewController
    {

        const int CommentContentMaximumLinesNumber = 5;
        const float CommentEditViewInnerMargin = 7f;
        const int SecondsToEdit = 60;

        public BusinessEntity Entity { get; set; }

        UIBarButtonItem doneButtonItem;
        UITableView tableView;
        UIView commentEditView;
        UIButton addComment;
        UITextView commentContent;
        NSLayoutConstraint commentEditViewBottomConstraint;
        NSLayoutConstraint commentContentMaximumHeightConstraint;

        NSObject didShowNotificationObserver;
        NSObject willChangeFrameNotificationObserver;
        NSObject willHideNotification;

        public CommentsListViewController()
        {
            Title = Localization.GetString("comments");
        }

        #region UIViewControllers overrides

        public override void LoadView()
        {
            base.LoadView();

            InitializeNavigationBar();
            InitializeListView();
            InitializeEditView();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ExtendedLayoutIncludesOpaqueBars = true;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            didShowNotificationObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.DidShowNotification, OnKeyboardDidShowNotification);
            willChangeFrameNotificationObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillChangeFrameNotification, OnKeyboardWillChangeFrameNotification);
            willHideNotification = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, OnKeyboardWillHideNotification);

            InitializeHandlers();

            ReachabilityBar.Attach(View, tableView, (float)(NavigationController.BottomLayoutGuide.Length + commentEditView.Frame.Height));
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info($"{nameof(CommentsListViewController)} appeared");

            var ds = (DataSource)tableView.Source;
            if (ds.Empty)
                RefreshView();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            NSNotificationCenter.DefaultCenter.RemoveObservers(new[] { didShowNotificationObserver, willChangeFrameNotificationObserver, willHideNotification });

            DeInitializeHandlers();
        }

        #endregion

        #region Init methods

        void InitializeNavigationBar()
        {
            doneButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Done);
            NavigationItem.SetRightBarButtonItem(doneButtonItem, false);
        }

        void InitializeListView()
        {
            AutomaticallyAdjustsScrollViewInsets = true;

            tableView = new UITableView();
            tableView.Source = new DataSource(this, tableView, Localization.GetString("no_comments"));
            tableView.EstimatedRowHeight = CommentsTableViewCell.Height;
            tableView.RowHeight = UITableView.AutomaticDimension;
            tableView.ClipsToBounds = false;
            tableView.KeyboardDismissMode = UIScrollViewKeyboardDismissMode.OnDrag;
            tableView.TranslatesAutoresizingMaskIntoConstraints = false;
            View.AddSubview(tableView);
            View.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1f, 0f),
                    NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1f, 0f),
                    NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1f, 0f)
                });
        }

        void InitializeEditView()
        {
            commentEditView = new UIView();
            commentEditView.BackgroundColor = UIColor.FromRGB(248f / 255f, 248f / 255f, 248f / 255f);
            commentEditView.TranslatesAutoresizingMaskIntoConstraints = false;
            commentEditView.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            commentEditViewBottomConstraint = NSLayoutConstraint.Create(commentEditView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1f, 0f);
            View.AddSubview(commentEditView);
            View.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(commentEditView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1f, 0f),
                    NSLayoutConstraint.Create(commentEditView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1f, 0f),
                    NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, commentEditView, NSLayoutAttribute.Top, 1f, 0f),
                    commentEditViewBottomConstraint
                });

            var borderView = new UIView();
            borderView.BackgroundColor = UIColor.LightGray;
            borderView.TranslatesAutoresizingMaskIntoConstraints = false;
            borderView.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            commentEditView.AddSubview(borderView);
            commentEditView.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(borderView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, commentEditView, NSLayoutAttribute.Top, 1f, 0f),
                    NSLayoutConstraint.Create(borderView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, commentEditView, NSLayoutAttribute.Left, 1f, 0f),
                    NSLayoutConstraint.Create(borderView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, commentEditView, NSLayoutAttribute.Right, 1f, 0f),
                    NSLayoutConstraint.Create(borderView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, 1f)
                });

            addComment = new UIButton(UIButtonType.System);
            addComment.TitleLabel.Font = Theme.DefaultBoldFont;
            addComment.SetTitle(Localization.GetString("add"), UIControlState.Normal);
            addComment.Enabled = false;
            addComment.Opaque = false;
            addComment.TranslatesAutoresizingMaskIntoConstraints = false;
            addComment.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            addComment.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            addComment.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            addComment.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            commentEditView.AddSubview(addComment);
            commentEditView.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(addComment, NSLayoutAttribute.Right, NSLayoutRelation.Equal, commentEditView, NSLayoutAttribute.Right, 1f, -CommentEditViewInnerMargin),
                    NSLayoutConstraint.Create(addComment, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, commentEditView, NSLayoutAttribute.CenterY, 1f, 0f)
                });

            commentContent = new UITextView();
            commentContent.AutocapitalizationType = UITextAutocapitalizationType.Sentences;
            commentContent.AutocorrectionType = UITextAutocorrectionType.Yes;
            commentContent.ScrollEnabled = false;
            commentContent.Font = Theme.DefaultFont;
            commentContent.Layer.BorderColor = UIColor.Gray.ColorWithAlpha(0.5f).CGColor;
            commentContent.Layer.BorderWidth = 1f;
            commentContent.Layer.CornerRadius = 5f;
            commentContent.ClipsToBounds = true;
            commentContent.TranslatesAutoresizingMaskIntoConstraints = false;
            commentEditView.AddSubview(commentContent);
            commentContentMaximumHeightConstraint = NSLayoutConstraint.Create(commentContent, NSLayoutAttribute.Height, NSLayoutRelation.LessThanOrEqual, null, NSLayoutAttribute.NoAttribute, 1f, commentContent.SizeThatFits(commentContent.Frame.Size).Height);
            commentEditView.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(commentContent, NSLayoutAttribute.Top, NSLayoutRelation.Equal, commentEditView, NSLayoutAttribute.Top, 1f, CommentEditViewInnerMargin),
                    NSLayoutConstraint.Create(commentContent, NSLayoutAttribute.Left, NSLayoutRelation.Equal, commentEditView, NSLayoutAttribute.Left, 1f, CommentEditViewInnerMargin),
                    NSLayoutConstraint.Create(commentContent, NSLayoutAttribute.Right, NSLayoutRelation.Equal, addComment, NSLayoutAttribute.Left, 1f, -CommentEditViewInnerMargin),
                    NSLayoutConstraint.Create(commentContent, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, commentEditView, NSLayoutAttribute.Bottom, 1f, -CommentEditViewInnerMargin),
                    commentContentMaximumHeightConstraint
                });

            View.AddGestureRecognizer(new UITapGestureRecognizer(() => commentContent.EndEditing(true)));
        }

        void InitializeHandlers()
        {
            if (doneButtonItem != null)
                doneButtonItem.Clicked += DoneButtonItem_Clicked;

            if (addComment != null)
                addComment.TouchUpInside += AddComment_TouchUpInside;

            if (commentContent != null)
                commentContent.Changed += CommentContent_Changed;
        }

        void DeInitializeHandlers()
        {
            if (doneButtonItem != null)
                doneButtonItem.Clicked -= DoneButtonItem_Clicked;

            if (addComment != null)
                addComment.TouchUpInside -= AddComment_TouchUpInside;

            if (commentContent != null)
                commentContent.Changed -= CommentContent_Changed;
        }

        #endregion

        #region Refresh

        void RefreshView()
        {
            if (Entity == null)
            {
                return;
            }

            List<Comment> comments;

            switch (Entity.ObjectType)
            {
                case ObjectType.Document:
                    comments = (Entity as Document).Comments;
                    break;
                case ObjectType.Contact:
                    comments = (Entity as Contact).Comments;
                    break;
                default:
                    throw new ArgumentException("The input business entity does not have comments defined in the model");
            }

            var ds = (tableView.Source as DataSource);
            ds.RefreshData(comments);
        }

        #endregion

        #region Event handlers

        void DoneButtonItem_Clicked(object sender, EventArgs e) => DismissViewController(true, null);

        async void AddComment_TouchUpInside(object sender, EventArgs e)
        {
            commentContent.EndEditing(true);

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("add_comment___"));
            var newCommentContent = commentContent.Text;

            try
            {
                Comment newComment;

                switch (Entity.ObjectType)
                {
                    case ObjectType.Document:
                        var document = Entity as Document;
                        newComment = await Managers.DocumentsManager.AddComment(document, newCommentContent);
                        PlatformConfig.MessengerHub.Publish(new DocumentPreviewCommentsCountChangedMessage(this, document.Id, document.Comments.Count));
                        break;
                    case ObjectType.Contact:
                        var contact = Entity as Contact;
                        newComment = await Managers.ContactsManager.AddComment(contact, newCommentContent);
                        break;
                    default:
                        throw new ArgumentException("The input business entity does not have comments defined in the model");
                }

                var ds = tableView.Source as DataSource;
                ds.AddComment(newComment);
                tableView.ScrollToRow(NSIndexPath.FromRowSection(ds.Items.Count - 1, 0), UITableViewScrollPosition.Middle, true);
                commentContent.Text = string.Empty;
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Failed to add comment attachment [entity.Id={Entity?.Id}, commentContent={newCommentContent}] ", ex);

                dismissAction();
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
            finally
            {
                dismissAction();
            }
        }

        void CommentContent_Changed(object sender, EventArgs e)
        {
            UpdateCommentContentHeight();
            UpdateAddCommentEnabled();
        }

        #endregion

        #region Actions

        void DeleteComment(Comment comment)
        {
            tableView.Editing = false;

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("deleting_comment___"));

            Task.Run(async () =>
             {
                 switch (Entity.ObjectType)
                 {
                     case ObjectType.Document:
                         var document = Entity as Document;
                         await Managers.DocumentsManager.DeleteComment(document, comment);
                         PlatformConfig.MessengerHub.Publish(new DocumentPreviewCommentsCountChangedMessage(this, document.Id, document.Comments.Count));
                         break;
                     case ObjectType.Contact:
                         var contact = Entity as Contact;
                         await Managers.ContactsManager.DeleteComment(contact, comment);
                         break;
                     default:
                         throw new ArgumentException("The input business entity does not have comments defined in the model");
                 }

             }).ContinueWith(async t =>
             {
                 dismissAction();

                 if (t.IsFaulted)
                 {
                     CommonConfig.Logger.Error($"Failed to delete comment from entity [objectType={Entity?.ObjectType}, entity.Id={Entity?.Id}, comment.Id={comment.Id}, comment.Content={comment.Content}] ", t.Exception.InnerException);
                     await Dialogs.ShowErrorDialogAsync(this, t.Exception.InnerException);
                 }
                 else
                 {
                     (tableView.Source as DataSource).RemoveComment(comment);
                 }

             }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        void StartEditComment(Comment comment)
        {
            tableView.Editing = false;

            var isEditable = DateTime.UtcNow.Subtract(comment.DateAddedTimestamp.ConvertTimestampMillisecondsToDateTime()).TotalSeconds <= SecondsToEdit;
            if (!isEditable)
            {
                Dialogs.ShowConfirmDialogAsync(this, Localization.GetString("error"), Localization.GetString("edit_not_possible"));
                return;
            }

            var alert = UIAlertController.Create(Localization.GetString("edit_message"), string.Empty, UIAlertControllerStyle.Alert);
            var cancelAction = UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, null);
            var confirmAction = UIAlertAction.Create(Localization.GetString("confirm"), UIAlertActionStyle.Default, obj => FinalizeEditComment(comment, alert.TextFields[0].Text));
            alert.AddTextField(tf => tf.Text = comment.Content);
            alert.AddAction(cancelAction);
            alert.AddAction(confirmAction);

            PresentViewController(alert, true, null);
        }

        void FinalizeEditComment(Comment oldComment, string newContent)
        {
            if (oldComment == null || newContent == null)
            {
                return;
            }

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("editing_comment___"));
            var newComment = oldComment.ShallowCopy();
            newComment.Content = newContent;

            Task.Run(async () =>
             {
                 bool result;

                 switch (Entity.ObjectType)
                 {
                     case ObjectType.Document:
                         var document = Entity as Document;
                         result = await Managers.DocumentsManager.EditComment(document, newComment);
                         break;
                     case ObjectType.Contact:
                         var contact = Entity as Contact;
                         result = await Managers.ContactsManager.EditComment(contact, newComment);
                         break;
                     default:
                         throw new ArgumentException("The input business entity does not have comments defined in the model");
                 }

                 return result;
             }).ContinueWith(async t =>
             {
                 dismissAction();

                 if (t.IsFaulted)
                 {
                     CommonConfig.Logger.Error($"Failed to edit comment for entity [objectType={Entity?.ObjectType}, entity.Id={Entity?.Id}, comment.Id={oldComment.Id}, comment.Content={oldComment.Content}] ", t.Exception.InnerException);
                     await Dialogs.ShowErrorDialogAsync(this, t.Exception.InnerException);
                 }
                 else if (t.Result)
                 {
                     (tableView.Source as DataSource).EditComment(newComment);
                 }
                 else
                 {
                     await Dialogs.ShowConfirmDialogAsync(this, Localization.GetString("error"), Localization.GetString("edit_not_possible"));
                 }
             }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        #endregion

        #region Keyboard Related

        void OnKeyboardDidShowNotification(NSNotification notification) => AdjustViewToKeyboard(UI.KeyboardHeightFromNotification(notification), notification);

        void OnKeyboardWillChangeFrameNotification(NSNotification notification) => AdjustViewToKeyboard(UI.KeyboardHeightFromNotification(notification), notification);

        void OnKeyboardWillHideNotification(NSNotification notification) => AdjustViewToKeyboard(0f, notification);

        void AdjustViewToKeyboard(float offset, NSNotification notification = null)
        {
            UpdateCommentContentHeight();

            commentEditViewBottomConstraint.Constant = -offset;

            if (notification != null)
            {
                var duration = UI.KeyboardAnimationDurationFromNotification(notification);
                var options = UI.KeyboardAnimationOptionsFromNotification(notification);
                UIView.AnimateNotify(duration, 0.0d, options, View.LayoutIfNeeded, null);
            }
            else
            {
                View.LayoutIfNeeded();
            }

            if (offset > 0f)
                ScrollCommentsToTop(true);
        }

        #endregion

        #region Utilities

        void UpdateAddCommentEnabled()
        {
            addComment.Enabled = !string.IsNullOrEmpty(commentContent.Text);
        }

        void UpdateCommentContentHeight()
        {
            if (NavigationController != null)
            {
                var heightThatFits = commentContent.SizeThatFits(commentContent.Frame.Size).Height;
                var targetHeight = Math.Min(heightThatFits, commentContent.Font.LineHeight * CommentContentMaximumLinesNumber);
                targetHeight = Math.Min(targetHeight, commentEditView.Frame.Bottom - NavigationController.NavigationBar.Frame.Bottom - 2 * CommentEditViewInnerMargin);
                if (targetHeight >= 0f)
                {
                    commentContentMaximumHeightConstraint.Constant = (nfloat)targetHeight;
                    commentContent.ScrollEnabled = heightThatFits > targetHeight;
                }
            }
        }

        void ScrollCommentsToTop(bool animated)
        {
            var commentsCount = (tableView.Source as DataSource).Items.Count;
            if (commentsCount > 0)
                tableView.ScrollToRow(NSIndexPath.FromRowSection(0, 0), UITableViewScrollPosition.Top, animated);
        }

        #endregion

        class DataSource : UITableViewSource, IDisposable
        {

            UITableView tableView;
            CommentsListViewController viewController;
            List<Comment> commentsInView = new List<Comment>();

            readonly string emptyText;

            public bool Empty
            {
                get
                {
                    return !Items.Any();
                }
            }

            public List<Comment> Items
            {
                get
                {
                    return commentsInView;
                }
            }

            public DataSource(CommentsListViewController viewController, UITableView tableView, string emptyText)
            {
                this.tableView = tableView;
                this.viewController = viewController;
                this.emptyText = emptyText;
            }

            #region Overrides

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (Empty)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                    emptyCell.Initialize(emptyText);
                    return emptyCell;
                }

                var cell = tableView.DequeueReusableCell(CommentsTableViewCell.Key) as CommentsTableViewCell ?? CommentsTableViewCell.Create();
                cell.Initialize(commentsInView[indexPath.Row]);
                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return Empty ? 1 : commentsInView.Count;
            }

            public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
            {
                var actions = new List<UITableViewRowAction>();
                var comment = commentsInView[indexPath.Row];

                if (comment.UserId == ServerConfig.SystemSettings.UserInfo.User.Id)
                {
                    var deleteAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Destructive, Localization.GetString("delete"), (a, ip) => viewController.DeleteComment(comment));
                    deleteAction.BackgroundColor = Theme.DarkerBlue;
                    actions.Add(deleteAction);

                    var isEditable = DateTime.Now.ToUniversalTime().Subtract(comment.DateAddedTimestamp.ConvertTimestampMillisecondsToDateTime()).TotalSeconds <= SecondsToEdit;

                    if (isEditable)
                    {
                        var editAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Destructive, Localization.GetString("edit"), (a, ip) => viewController.StartEditComment(comment));
                        editAction.BackgroundColor = Theme.DarkBlue;
                        actions.Add(editAction);
                    }
                }

                return actions.ToArray();
            }

            #endregion

            public void RefreshData(List<Comment> newComments)
            {
                if (newComments == null)
                    return;

                commentsInView.AddRange(newComments);
                tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            public void AddComment(Comment newComment)
            {
                if (newComment == null)
                    return;

                commentsInView.Add(newComment);

                if (commentsInView.Count == 1)
                    tableView.ReloadRows(new NSIndexPath[] { NSIndexPath.FromRowSection(commentsInView.Count - 1, 0) }, UITableViewRowAnimation.Fade);
                else
                    tableView.InsertRows(new NSIndexPath[] { NSIndexPath.FromRowSection(commentsInView.Count - 1, 0) }, UITableViewRowAnimation.Fade);
            }

            public void RemoveComment(Comment comment)
            {
                var position = commentsInView.FindIndex(c => c.Id == comment.Id);
                if (position >= 0)
                {
                    commentsInView.RemoveAt(position);

                    if (commentsInView.Count == 0)
                        tableView.ReloadRows(new NSIndexPath[] { NSIndexPath.FromRowSection(position, 0) }, UITableViewRowAnimation.Fade);
                    else
                        tableView.DeleteRows(new NSIndexPath[] { NSIndexPath.FromRowSection(position, 0) }, UITableViewRowAnimation.Fade);
                }
            }

            public void EditComment(Comment editedContent)
            {
                var position = commentsInView.FindIndex(c => c.Id == editedContent.Id);
                if (position >= 0)
                {
                    commentsInView[position].Content = editedContent.Content;

                    tableView.ReloadRows(new NSIndexPath[] { NSIndexPath.FromRowSection(position, 0) }, UITableViewRowAnimation.Fade);
                }
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                tableView = null;
                commentsInView = null;
            }
        }
    }
}
