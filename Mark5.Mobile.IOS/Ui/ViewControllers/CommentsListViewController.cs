//
// Project: ${Project}
// File: CommentsListViewController.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class CommentsListViewController : AbstractViewController
    {
        const int CommentContentMaximumLinesNumber = 5;
        const float CommentEditViewInnerMargin = 7.0f;

        UIBarButtonItem doneButtonItem;
        UITableView commentsTableView;
        UIView commentEditView;
        UIButton addComment;
        UITextView commentContent;
        NSLayoutConstraint commentEditViewBottomConstraint;
        NSLayoutConstraint commentContentMaximumHeightConstraint;

        public BusinessEntity Entity { get; set; }

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

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.DidShowNotification, OnKeyboardDidShowNotification);
            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillChangeFrameNotification, OnKeyboardWillChangeFrameNotification);
            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, OnKeyboardWillHideNotification);

            InitializeHandlers();
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info($"{nameof(CommentsListViewController)} appeared");

            var ds = (DataSource)commentsTableView.Source;
            if (ds.Empty)
                RefreshView();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

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

            commentsTableView = new UITableView();
            commentsTableView.Source = new DataSource(commentsTableView, Localization.GetString("no_comments"));
            commentsTableView.TranslatesAutoresizingMaskIntoConstraints = false;
            commentsTableView.Bounces = true;
            commentsTableView.CellLayoutMarginsFollowReadableWidth = false;
            commentsTableView.KeyboardDismissMode = UIScrollViewKeyboardDismissMode.OnDrag;
            commentsTableView.ClipsToBounds = false;

            View.AddSubview(commentsTableView);
            View.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(commentsTableView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(commentsTableView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(commentsTableView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1.0f, 0.0f),
                });
        }

        void InitializeEditView()
        {
            commentEditView = new UIView();
            commentEditView.BackgroundColor = UIColor.FromRGB(248.0f / 255.0f, 248.0f / 255.0f, 248.0f / 255.0f); //TODO should we take it from official color?
            commentEditView.TranslatesAutoresizingMaskIntoConstraints = false;
            commentEditView.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            commentEditViewBottomConstraint = NSLayoutConstraint.Create(commentEditView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1.0f, 0.0f);
            View.AddSubview(commentEditView);
            View.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(commentEditView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(commentEditView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(commentsTableView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, commentEditView, NSLayoutAttribute.Top, 1.0f, 0.0f),
                    commentEditViewBottomConstraint
                });

            var borderView = new UIView();
            borderView.BackgroundColor = UIColor.LightGray;
            borderView.TranslatesAutoresizingMaskIntoConstraints = false;
            borderView.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            commentEditView.AddSubview(borderView);
            commentEditView.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(borderView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, commentEditView, NSLayoutAttribute.Top, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(borderView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, commentEditView, NSLayoutAttribute.Left, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(borderView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, commentEditView, NSLayoutAttribute.Right, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(borderView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1.0f, 1.0f)
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
                    NSLayoutConstraint.Create(addComment, NSLayoutAttribute.Right, NSLayoutRelation.Equal, commentEditView, NSLayoutAttribute.Right, 1.0f, -CommentEditViewInnerMargin),
                    NSLayoutConstraint.Create(addComment, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, commentEditView, NSLayoutAttribute.CenterY, 1.0f, 0.0f)
                });

            commentContent = new UITextView();
            commentContent.AutocapitalizationType = UITextAutocapitalizationType.Sentences;
            commentContent.AutocorrectionType = UITextAutocorrectionType.Yes;
            commentContent.ScrollEnabled = false;
            commentContent.Font = Theme.DefaultFont;
            commentContent.Layer.BorderColor = UIColor.Gray.ColorWithAlpha(0.5f).CGColor;
            commentContent.Layer.BorderWidth = 1.0f;
            commentContent.Layer.CornerRadius = 5.0f;
            commentContent.ClipsToBounds = true;
            commentContent.TranslatesAutoresizingMaskIntoConstraints = false;
            commentEditView.AddSubview(commentContent);
            commentContentMaximumHeightConstraint = NSLayoutConstraint.Create(commentContent, NSLayoutAttribute.Height, NSLayoutRelation.LessThanOrEqual, null, NSLayoutAttribute.NoAttribute, 1.0f, commentContent.SizeThatFits(commentContent.Frame.Size).Height);
            commentEditView.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(commentContent, NSLayoutAttribute.Top, NSLayoutRelation.Equal, commentEditView, NSLayoutAttribute.Top, 1.0f, CommentEditViewInnerMargin),
                    NSLayoutConstraint.Create(commentContent, NSLayoutAttribute.Left, NSLayoutRelation.Equal, commentEditView, NSLayoutAttribute.Left, 1.0f, CommentEditViewInnerMargin),
                    NSLayoutConstraint.Create(commentContent, NSLayoutAttribute.Right, NSLayoutRelation.Equal, addComment, NSLayoutAttribute.Left, 1.0f, -CommentEditViewInnerMargin),
                    NSLayoutConstraint.Create(commentContent, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, commentEditView, NSLayoutAttribute.Bottom, 1.0f, -CommentEditViewInnerMargin),
                    commentContentMaximumHeightConstraint,
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
            //TODO
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

            var ds = (commentsTableView.Source as DataSource);
            ds.RefreshData(comments);
        }

        #endregion

        #region Event handlers

        void DoneButtonItem_Clicked(object sender, EventArgs e)
        {
            DismissViewController(true, null); //TODO need to add check if a comment has not been sent
        }

        void AddComment_TouchUpInside(object sender, EventArgs e)
        {

        }

        void CommentContent_Changed(object sender, EventArgs e)
        {
            UpdateCommentContentHeight();
            UpdateAddCommentEnabled();
        }

        #endregion

        #region Keyboard Related

        void OnKeyboardDidShowNotification(NSNotification notification)
        {
            AdjustViewToKeyboard(UI.KeyboardHeightFromNotification(notification), notification);
        }

        void OnKeyboardWillChangeFrameNotification(NSNotification notification)
        {
            AdjustViewToKeyboard(UI.KeyboardHeightFromNotification(notification), notification);
        }

        void OnKeyboardWillHideNotification(NSNotification notification)
        {
            AdjustViewToKeyboard(0.0f, notification);
        }

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

            if (offset > 0.0f)
            {
                ScrollCommentsToTop(true);
            }
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
                if (targetHeight >= 0.0f)
                {
                    commentContentMaximumHeightConstraint.Constant = (nfloat)targetHeight;
                    commentContent.ScrollEnabled = heightThatFits > targetHeight;
                }
            }
        }

        void ScrollCommentsToTop(bool animated)
        {
            var commentsCount = (commentsTableView.Source as DataSource).Items.Count;
            if (commentsCount > 0)
            {
                commentsTableView.ScrollToRow(NSIndexPath.FromRowSection(0, 0), UITableViewScrollPosition.Top, animated);
            }
        }

        #endregion

        class DataSource : UITableViewSource, IDisposable
        {
            const float CellHeight = 58.0f;
            const float TextViewHeight = 27.0f;

            UITableView tableView;
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

            public DataSource(UITableView tableView, string emptyText)
            {
                this.tableView = tableView;
                this.emptyText = emptyText;
            }

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

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                if (Empty)
                {
                    return CellHeight;
                }

                var tempView = new UITextView();
                tempView.Text = commentsInView[indexPath.Row].Content;
                var textHeight = tempView.SizeThatFits(new CGSize(tableView.Frame.Width, float.MaxValue)).Height;

                return CellHeight - TextViewHeight + textHeight;
            }

            public void RefreshData(List<Comment> newComments)
            {
                if (newComments == null)
                {
                    return;
                }

                commentsInView.AddRange(newComments);
                tableView.ReloadData();
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing); //TODO finish
            }
        }

    }
}
