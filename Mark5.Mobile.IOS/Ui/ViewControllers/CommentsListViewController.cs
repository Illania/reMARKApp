using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
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

            if (Entity != null)
                CommonConfig.UsageAnalytics.LogEvent(new OpenCommentsEvent(Entity.ModuleType));

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

            ((DataSource)tableView?.Source)?.Reset();

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

            NSLayoutConstraint tableViewBottomConstraint = null;

            if (Integration.IsRunningAtLeast(11))
                tableViewBottomConstraint = tableView.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor);
            else
                tableViewBottomConstraint = tableView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor);

            View.AddConstraints(new[]
            {
                tableView.LeftAnchor.ConstraintEqualTo(View.LeftAnchor),
                tableView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                tableView.RightAnchor.ConstraintEqualTo(View.RightAnchor),
                tableViewBottomConstraint
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

            if (Integration.IsRunningAtLeast(11))
                commentViewBottomConstraint = commentView.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor);
            else
                commentViewBottomConstraint = commentView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor);

            View.AddSubview(commentView);
            View.AddConstraints(new[]
            {
                commentView.LeftAnchor.ConstraintEqualTo(View.LeftAnchor),
                commentView.RightAnchor.ConstraintEqualTo(View.RightAnchor),
                commentViewBottomConstraint,
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
                borderView.TopAnchor.ConstraintEqualTo(commentView.TopAnchor),
                borderView.LeftAnchor.ConstraintEqualTo(commentView.LeftAnchor),
                borderView.RightAnchor.ConstraintEqualTo(commentView.RightAnchor),
                borderView.HeightAnchor.ConstraintEqualTo(.75f)
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

            commentTextScrollView = new UIScrollView
            {
                ScrollEnabled = false,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            if (Integration.IsRunningAtLeast(11))
                commentTextScrollView.ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Never;
            commentTextScrollView.Layer.BorderColor = Theme.DarkGray.CGColor;
            commentTextScrollView.Layer.BorderWidth = .7f;
            commentTextScrollView.Layer.CornerRadius = 5f;
            commentView.AddSubview(commentTextScrollView);
            commentView.AddConstraints(new[]
            {
                commentTextScrollView.TopAnchor.ConstraintEqualTo(commentView.TopAnchor,7f),
                commentTextScrollView.LeftAnchor.ConstraintEqualTo(commentView.LeftAnchor,7f),
                commentTextScrollView.RightAnchor.ConstraintEqualTo(addCommentButton.LeftAnchor,-7f),
                commentTextScrollView.BottomAnchor.ConstraintEqualTo(commentView.BottomAnchor,-7f),
                commentTextScrollView.HeightAnchor.ConstraintGreaterThanOrEqualTo(0f),
                commentTextScrollViewHeightConstraint = commentTextScrollView.HeightAnchor.ConstraintEqualTo(0f),
                   
                addCommentButton.RightAnchor.ConstraintEqualTo(commentView.RightAnchor,-7f),
                addCommentButton.CenterYAnchor.ConstraintEqualTo(commentTextScrollView.CenterYAnchor)
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
                commentTextView.TopAnchor.ConstraintEqualTo(commentTextScrollView.TopAnchor),
                commentTextView.LeftAnchor.ConstraintEqualTo(commentTextScrollView.LeftAnchor),
                commentTextView.RightAnchor.ConstraintEqualTo(commentTextScrollView.RightAnchor),
                commentTextView.BottomAnchor.ConstraintEqualTo(commentTextScrollView.BottomAnchor),
                commentTextView.WidthAnchor.ConstraintEqualTo(commentTextScrollView.WidthAnchor)
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
                    CommonConfig.Logger.Error($"Failed to delete comment from entity [objectType={Entity?.ObjectType}, entity.Id={Entity?.Id}, comment.Id={comment.Id}, comment.Content={comment.Content}] "
                                              , t.Exception.InnerException);
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

            if (Integration.IsRunningAtLeast(11))
                commentViewBottomConstraint.Constant = height == 0 ? 0 : -height + (View.SafeAreaInsets.Bottom);
            else
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
            tableView.ContentInset = new UIEdgeInsets(tableView.ContentInset.Top, 0f, offset, 0f);
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

                if (indexPath.Row < 0 || indexPath.Row >= Items.Count)
                    return actions.ToArray();

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