using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class CommentsListViewController : AbstractViewController, IUITextViewDelegate
    {
        const float MinCommentTextViewHeight = 38f; // 1 line
        const float MaxCommentTextViewHeight = 125.5f; // 5 lines

        public BusinessEntity Entity { get; set; }

        UIBarButtonItem doneButtonItem;

        UITableView tableView;
        UIView commentView;
        UIButton addCommentButton;
        UIScrollView commentTextScrollView;
        UITextView commentTextView;

        NSLayoutConstraint commentViewBottomConstraint;
        NSLayoutConstraint commentTextScrollViewHeightConstraint;

        #region UIViewController overrides

        public override void LoadView()
        {
            base.LoadView();

            InitializeNavigationBar();
            InitializeView();
            InitializeEditView();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (Integration.IsRunningAtLeast(11))
            {
                if (NavigationController != null)
                    NavigationController.NavigationBar.PrefersLargeTitles = true;
                NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Automatic;
            }

            InitializeHandlers();
            UpdateCommentTextViewHeight();
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info("Appeared");

            if (((DataSource)tableView.Source).Empty)
                RefreshView();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeInitializeHandlers();
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning("Received memory warning!");

            ((DataSource)tableView.Source)?.Reset();

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        protected override void Recycle()
        {
            base.Recycle();

            doneButtonItem = null;

            ((DataSource)tableView.Source)?.Reset();
            tableView = null;
            commentView = null;
            commentTextScrollView = null;
            commentTextView.Delegate = null;
            commentTextView = null;

            commentViewBottomConstraint = null;
            commentTextScrollViewHeightConstraint = null;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (CommonConfig.Logger.IsDebugEnabled())
                CommonConfig.Logger.Debug("Disposed");
        }

        #endregion

        #region Initialize/deinitialize

        void InitializeNavigationBar()
        {
            Title = Localization.GetString("comments");

            doneButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Done);
            NavigationItem.SetRightBarButtonItem(doneButtonItem, false);
        }

        void InitializeView()
        {
            tableView = new UITableView();
            tableView.Source = new DataSource(this, tableView);
            tableView.KeyboardDismissMode = UIScrollViewKeyboardDismissMode.Interactive;
            tableView.TranslatesAutoresizingMaskIntoConstraints = false;
            tableView.RowHeight = UITableView.AutomaticDimension;
            tableView.EstimatedRowHeight = 20f;
            View.AddSubview(tableView);
            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1f, 0f)
            });
        }

        void InitializeEditView()
        {
            commentView = new UIView
            {
                Opaque = true,
                BackgroundColor = Theme.White,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            commentView.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            View.AddSubview(commentView);
            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(commentView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(commentView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1f, 0f),
                commentViewBottomConstraint = NSLayoutConstraint.Create(commentView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1f, 0f)
            });

            var borderView = new UIView
            {
                BackgroundColor = Theme.DarkGray,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            borderView.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            commentView.AddSubview(borderView);
            commentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(borderView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, commentView, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(borderView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, commentView, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(borderView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, commentView, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(borderView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, .75f)
            });

            addCommentButton = new UIButton(UIButtonType.System)
            {
                Enabled = false,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            addCommentButton.ApplyTheme();
            addCommentButton.SetTitle(Localization.GetString("add"), UIControlState.Normal);
            addCommentButton.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            addCommentButton.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            addCommentButton.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            addCommentButton.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            commentView.AddSubview(addCommentButton);
            commentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(addCommentButton, NSLayoutAttribute.Right, NSLayoutRelation.Equal, commentView, NSLayoutAttribute.Right, 1f, -7f),
                NSLayoutConstraint.Create(addCommentButton, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, commentView, NSLayoutAttribute.CenterY, 1f, 0f)
            });

            commentTextScrollView = new UIScrollView
            {
                ScrollEnabled = false,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            if (Integration.IsRunningAtLeast(11))
                commentTextScrollView.ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Never;
            commentTextScrollView.Layer.BorderColor = Theme.DarkGray.CGColor;
            commentTextScrollView.Layer.BorderWidth = .75f;
            commentTextScrollView.Layer.CornerRadius = 5f;
            commentView.AddSubview(commentTextScrollView);
            commentView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(commentTextScrollView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, commentView, NSLayoutAttribute.Top, 1f, 7f),
                NSLayoutConstraint.Create(commentTextScrollView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, commentView, NSLayoutAttribute.Left, 1f, 7f),
                NSLayoutConstraint.Create(commentTextScrollView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, addCommentButton, NSLayoutAttribute.Left, 1f, -7f),
                NSLayoutConstraint.Create(commentTextScrollView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, commentView, NSLayoutAttribute.Bottom, 1f, -7f),
                NSLayoutConstraint.Create(commentTextScrollView, NSLayoutAttribute.Height, NSLayoutRelation.GreaterThanOrEqual, null, NSLayoutAttribute.NoAttribute, 1f, 0f),
                commentTextScrollViewHeightConstraint = NSLayoutConstraint.Create(commentTextScrollView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, 0f)
            });

            commentTextView = new UITextView
            {
                AutocapitalizationType = UITextAutocapitalizationType.Sentences,
                AutocorrectionType = UITextAutocorrectionType.Yes,
                ScrollEnabled = false,
                ClipsToBounds = true,
                Delegate = this,
                TranslatesAutoresizingMaskIntoConstraints = false,
                InputAccessoryView = new KeyboardObserverInputAccessoryView()
            };
            commentTextView.ApplyTheme();
            commentTextScrollView.AddSubview(commentTextView);
            commentTextScrollView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(commentTextView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, commentTextScrollView, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(commentTextView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, commentTextScrollView, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(commentTextView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, commentTextScrollView, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(commentTextView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, commentTextScrollView, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(commentTextView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, commentTextScrollView, NSLayoutAttribute.Width, 1f, 0f)
            });
        }

        void InitializeHandlers()
        {
            if (doneButtonItem != null)
                doneButtonItem.Clicked += DoneButtonItem_Clicked;

            if (addCommentButton != null)
                addCommentButton.TouchUpInside += AddComment_TouchUpInside;

            if (commentTextView.InputAccessoryView is KeyboardObserverInputAccessoryView keyboardObserverInputAccessoryView)
                keyboardObserverInputAccessoryView.KeyboardChanged += KeyboardObserverInputAccessoryView_KeyboardChanged;
        }

        void DeInitializeHandlers()
        {
            if (doneButtonItem != null)
                doneButtonItem.Clicked -= DoneButtonItem_Clicked;

            if (addCommentButton != null)
                addCommentButton.TouchUpInside -= AddComment_TouchUpInside;

            if (commentTextView.InputAccessoryView is KeyboardObserverInputAccessoryView keyboardObserverInputAccessoryView)
                keyboardObserverInputAccessoryView.KeyboardChanged -= KeyboardObserverInputAccessoryView_KeyboardChanged;
        }

        #endregion

        #region Refresh

        void RefreshView()
        {
            if (Entity == null)
                return;

            List<Comment> comments = null;

            if (Entity is Document d)
                comments = d.Comments;
            if (Entity is Contact c)
                comments = c.Comments;

            ((DataSource)tableView.Source).SetItems(comments);
        }

        #endregion

        #region Event handlers

        void DoneButtonItem_Clicked(object sender, EventArgs e) => DismissViewController(true, null);

        async void AddComment_TouchUpInside(object sender, EventArgs e)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("adding_comment___"));
            var newCommentContent = commentTextView.Text;

            try
            {
                Comment newComment = null;

                if (Entity is Document d)
                    newComment = await Managers.DocumentsManager.AddComment(d, newCommentContent);
                if (Entity is Contact c)
                    newComment = await Managers.ContactsManager.AddComment(c, newCommentContent);

                ((DataSource)tableView.Source).AddComment(newComment);
                commentTextView.Text = null;
                commentTextView.EndEditing(true);

                ScrollCommentsToBottom(true);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Failed to add comment attachment [entity.Id={Entity?.Id}, commentContent={newCommentContent}] ", ex);

                dismissAction();
                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
            finally
            {
                dismissAction();
            }
        }

        #endregion

        #region Actions

        void DeleteComment(Comment comment)
        {
            tableView.Editing = false;

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("deleting_comment___"));

            Task.Run(async () =>
            {
                if (Entity is Document d)
                    await Managers.DocumentsManager.DeleteComment(d, comment);
                if (Entity is Contact c)
                    await Managers.ContactsManager.DeleteComment(c, comment);
            }).ContinueWith(async t =>
            {
                dismissAction();

                if (t.IsFaulted)
                {
                    CommonConfig.Logger.Error($"Failed to delete comment from entity [objectType={Entity?.ObjectType}, entity.Id={Entity?.Id}, comment.Id={comment.Id}, comment.Content={comment.Content}] ", t.Exception.InnerException);
                    await Dialogs.ShowErrorAlertAsync(this, t.Exception.InnerException);
                }
                else
                    ((DataSource)tableView.Source).RemoveComment(comment);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        #endregion

        #region Keyboard handling

        void KeyboardObserverInputAccessoryView_KeyboardChanged(object sender, CGRect frame)
        {
            var height = KeyboardObserverInputAccessoryView.GetVisibleKeyboardHeight(View, frame);

            commentViewBottomConstraint.Constant = -height;
            commentView.LayoutIfNeeded();

            AdjustSafeAreaInsets();
        }

        #endregion

        #region IUITextViewDelegate

        [Export("textViewDidChange:")]
        public void Changed(UITextView textView)
        {
            UpdateAddCommentEnabled();
            UpdateCommentTextViewHeight();
        }

        [Export("textViewDidEndEditing:")]
        public void EditingEnded(UITextView textView)
        {
            UpdateAddCommentEnabled();
            UpdateCommentTextViewHeight();
        }

        #endregion

        #region Utilities

        void UpdateAddCommentEnabled()
        {
            addCommentButton.Enabled = !string.IsNullOrEmpty(commentTextView.Text);
        }

        void UpdateCommentTextViewHeight()
        {
            if (NavigationController == null)
                return;

            var requiredHeight = commentTextView.SizeThatFits(new CGSize(commentTextView.ContentSize.Width, nfloat.MaxValue)).Height;
            var targetHeight = Math.Min(Math.Max(requiredHeight, MinCommentTextViewHeight), MaxCommentTextViewHeight);

            commentTextScrollViewHeightConstraint.Constant = (nfloat)targetHeight;
            commentTextScrollView.ScrollEnabled = requiredHeight > targetHeight;

            commentView.LayoutIfNeeded();

            AdjustSafeAreaInsets();
        }

        void AdjustSafeAreaInsets()
        {
            var offset = -commentViewBottomConstraint.Constant + commentView.Frame.Height;
            if (Integration.IsRunningAtLeast(11))
            {
                AdditionalSafeAreaInsets = new UIEdgeInsets(0f, 0f, offset, 0f);
            }
            else
            {
                tableView.ContentInset = new UIEdgeInsets(NavigationController.NavigationBar.Frame.Bottom, 0f, offset, 0f);
            }
        }

        void ScrollCommentsToBottom(bool animated)
        {
            if (!((DataSource)tableView.Source).Empty)
                tableView.ScrollToRow(NSIndexPath.FromRowSection(((DataSource)tableView.Source).Items.Count - 1, 0),
                                      UITableViewScrollPosition.Bottom,
                                      animated);
        }

        #endregion

        #region DataSource

        class DataSource : UITableViewSource
        {
            public bool Empty => !Items.Any();
            public List<Comment> Items { get; } = new List<Comment>();

            readonly WeakReference<CommentsListViewController> viewControllerWeakReference;
            readonly WeakReference<UITableView> tableViewWeakReference;

            bool loading = true;

            public DataSource(CommentsListViewController viewController, UITableView tableView)
            {
                viewControllerWeakReference = viewController.Wrap();
                tableViewWeakReference = tableView.Wrap();
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                    return tableView.DequeueReusableCell(WaitTableViewCell.DefaultId) as WaitTableViewCell ?? new WaitTableViewCell();

                if (Empty)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.DefaultId) as EmptyTableViewCell ?? new EmptyTableViewCell();
                    emptyCell.Initialize(Localization.GetString("no_comments"));
                    return emptyCell;
                }

                var cell = tableView.DequeueReusableCell(CommentsTableViewCell.DefaultId) as CommentsTableViewCell ?? new CommentsTableViewCell();
                cell.Initialize(Items[indexPath.Row]);
                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading || Empty)
                    return 1;

                return Items.Count;
            }

            public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath) => tableView.CellAt(indexPath)?.UserInteractionEnabled ?? false;

            public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
            {
                var actions = new List<UITableViewRowAction>();
                var comment = Items[indexPath.Row];

                if (comment.UserId == ServerConfig.SystemSettings.UserInfo.User.Id)
                {
                    var deleteAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Destructive,
                                                                   Localization.GetString("delete"),
                                                                   (a, ip) => viewControllerWeakReference.Unwrap()?.DeleteComment(comment));
                    deleteAction.BackgroundColor = Theme.DarkerBlue;
                    actions.Add(deleteAction);
                }

                return actions.ToArray();
            }

            public void SetItems(List<Comment> newComments)
            {
                loading = false;

                Items.Clear();
                Items.AddRange(newComments);
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            public void Reset()
            {
                loading = true;

                Items.Clear();
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            public void AddComment(Comment newComment)
            {
                Items.Add(newComment);

                if (Items.Count == 1)
                    tableViewWeakReference.Unwrap()?.ReloadRows(new[] { NSIndexPath.FromRowSection(Items.Count - 1, 0) }, UITableViewRowAnimation.Fade);
                else
                    tableViewWeakReference.Unwrap()?.InsertRows(new[] { NSIndexPath.FromRowSection(Items.Count - 1, 0) }, UITableViewRowAnimation.Fade);
            }

            public void RemoveComment(Comment comment)
            {
                var position = Items.FindIndex(c => c.Id == comment.Id);
                if (position >= 0)
                {
                    Items.RemoveAt(position);

                    if (Items.Count == 0)
                        tableViewWeakReference.Unwrap()?.ReloadRows(new[] { NSIndexPath.FromRowSection(position, 0) }, UITableViewRowAnimation.Fade);
                    else
                        tableViewWeakReference.Unwrap()?.DeleteRows(new[] { NSIndexPath.FromRowSection(position, 0) }, UITableViewRowAnimation.Fade);
                }
            }
        }

        #endregion

    }
}