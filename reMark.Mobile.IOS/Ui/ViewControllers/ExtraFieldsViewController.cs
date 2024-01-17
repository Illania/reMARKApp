using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Manager;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Utilities.Extensions;
using reMark.Mobile.IOS.Ui.Common;
using reMark.Mobile.IOS.Ui.TableViewCells;
using reMark.Mobile.IOS.Utilities;
using UIKit;

namespace reMark.Mobile.IOS.Ui.ViewControllers
{
    public class ExtraFieldsViewController : AbstractViewController, IUITextViewDelegate
    {
        const float MinTextViewHeight = 38f; // 1 line
        const float MaxCommentTextViewHeight = 125.5f; // 5 lines
        bool refreshing;
        public bool IsEditingAvailable = false;
        UIBarButtonItem doneButtonItem;

        UITableView TableView;
        UIView extraFieldView;
        UIButtonScalable addExtraFieldButton;
        UITextViewScalable extraFieldTextView;
        UIScrollView extraFieldTextScrollView;

        NSLayoutConstraint extraFieldViewBottomConstraint;
        NSLayoutConstraint extraFieldTextScrollViewHeightConstraint;

        #region UIViewController overrides

        public override void LoadView()
        {
            base.LoadView();

            InitializeNavigationBar();
            InitializeView();

            if (IsEditingAvailable)
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
            UpdateExtraFieldTextViewHeight();
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info("Appeared");

            if (((DataSource)TableView.Source).Empty)
                RefreshData();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeInitializeHandlers();
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning("Received memory warning!");

            ((DataSource)TableView?.Source)?.Reset();

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        protected override void Recycle()
        {
            base.Recycle();

            doneButtonItem = null;

            ((DataSource)TableView.Source)?.Reset();
            TableView = null;

            if (extraFieldView != null)
                extraFieldView = null;

            if (extraFieldTextScrollView != null)
                extraFieldTextScrollView = null;

            if (extraFieldViewBottomConstraint != null) 
                extraFieldViewBottomConstraint = null;

            if (extraFieldTextView != null)
            {
                extraFieldTextView.Delegate = null;
                extraFieldTextView = null;
            }
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
            Title = Localization.GetString("extra_fields");

            doneButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Done);
            NavigationItem.SetRightBarButtonItem(doneButtonItem, false);
        }

        void InitializeView()
        {

            TableView = new UITableView();
            TableView.Source = new DataSource(this, TableView);
            TableView.KeyboardDismissMode = UIScrollViewKeyboardDismissMode.Interactive;
            TableView.TranslatesAutoresizingMaskIntoConstraints = false;
            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 20f;
            TableView.RefreshControl = new UIRefreshControl();
            View.AddSubview(TableView);

            NSLayoutConstraint tableViewBottomConstraint;

            if (Integration.IsRunningAtLeast(11))
                tableViewBottomConstraint = TableView.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor);
            else
                tableViewBottomConstraint = TableView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor);

            View.AddConstraints(new[]
            {
                TableView.LeftAnchor.ConstraintEqualTo(View.LeftAnchor),
                TableView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                TableView.RightAnchor.ConstraintEqualTo(View.RightAnchor),
                tableViewBottomConstraint
            });
        }

        void InitializeEditView()
        {
            extraFieldView = new UIView
            {
                Opaque = true,
                BackgroundColor = Theme.White,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            extraFieldView.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);

            if (Integration.IsRunningAtLeast(11))
                extraFieldViewBottomConstraint = extraFieldView.BottomAnchor.ConstraintEqualTo(TableView.BottomAnchor);

            View.AddSubview(extraFieldView);
            View.AddConstraints(new[]
            {
                extraFieldView.LeftAnchor.ConstraintEqualTo(View.LeftAnchor),
                extraFieldView.RightAnchor.ConstraintEqualTo(View.RightAnchor),
                extraFieldViewBottomConstraint,
            });

            var borderView = new UIView
            {
                BackgroundColor = Theme.DarkGray,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            borderView.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            extraFieldView.AddSubview(borderView);
            extraFieldView.AddConstraints(new[]
            {
                borderView.TopAnchor.ConstraintEqualTo(extraFieldView.TopAnchor),
                borderView.LeftAnchor.ConstraintEqualTo(extraFieldView.LeftAnchor),
                borderView.RightAnchor.ConstraintEqualTo(extraFieldView.RightAnchor),
                borderView.HeightAnchor.ConstraintEqualTo(.75f)
            });

            addExtraFieldButton = new UIButtonScalable(UIButtonType.System)
            {
                Enabled = false,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            addExtraFieldButton.ApplyTheme();
            addExtraFieldButton.SetTitle(Localization.GetString("add"), UIControlState.Normal);
            addExtraFieldButton.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            addExtraFieldButton.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            addExtraFieldButton.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            addExtraFieldButton.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            extraFieldView.AddSubview(addExtraFieldButton);
            extraFieldTextScrollView = new UIScrollView
            {
                ScrollEnabled = false,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            if (Integration.IsRunningAtLeast(11))
                extraFieldTextScrollView.ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Never;
            extraFieldTextScrollView.Layer.BorderColor = Theme.DarkGray.CGColor;
            extraFieldTextScrollView.Layer.BorderWidth = .7f;
            extraFieldTextScrollView.Layer.CornerRadius = 5f;
            extraFieldView.AddSubview(extraFieldTextScrollView);
            extraFieldView.AddConstraints(new[]
            {
                extraFieldTextScrollView.TopAnchor.ConstraintEqualTo(extraFieldView.TopAnchor,7f),
                extraFieldTextScrollView.LeftAnchor.ConstraintEqualTo(extraFieldView.LeftAnchor,7f),
                extraFieldTextScrollView.RightAnchor.ConstraintEqualTo(addExtraFieldButton.LeftAnchor,-7f),
                extraFieldTextScrollView.BottomAnchor.ConstraintEqualTo(extraFieldView.BottomAnchor,-7f),
                extraFieldTextScrollView.HeightAnchor.ConstraintGreaterThanOrEqualTo(0f),
                extraFieldTextScrollViewHeightConstraint = extraFieldTextScrollView.HeightAnchor.ConstraintEqualTo(0f),

                addExtraFieldButton.RightAnchor.ConstraintEqualTo(extraFieldView.RightAnchor,-7f),
                addExtraFieldButton.CenterYAnchor.ConstraintEqualTo(extraFieldTextScrollView.CenterYAnchor)
            });


            extraFieldTextView = new UITextViewScalable
            {
                AutocapitalizationType = UITextAutocapitalizationType.Sentences,
                AutocorrectionType = UITextAutocorrectionType.Yes,
                ScrollEnabled = false,
                ClipsToBounds = true,
                Delegate = this,
                TranslatesAutoresizingMaskIntoConstraints = false,
                InputAccessoryView = new KeyboardObserverInputAccessoryView()
            };
            extraFieldTextView.ApplyTheme();
            extraFieldTextScrollView.AddSubview(extraFieldTextView);
            extraFieldTextScrollView.AddConstraints(new[]
            {
                extraFieldTextView.TopAnchor.ConstraintEqualTo(extraFieldTextScrollView.TopAnchor),
                extraFieldTextView.LeftAnchor.ConstraintEqualTo(extraFieldTextScrollView.LeftAnchor),
                extraFieldTextView.RightAnchor.ConstraintEqualTo(extraFieldTextScrollView.RightAnchor),
                extraFieldTextView.BottomAnchor.ConstraintEqualTo(extraFieldTextScrollView.BottomAnchor),
                extraFieldTextView.WidthAnchor.ConstraintEqualTo(extraFieldTextScrollView.WidthAnchor)
            });
        }

        void InitializeHandlers()
        {
            if (doneButtonItem != null)
                doneButtonItem.Clicked += DoneButtonItem_Clicked;

            if (addExtraFieldButton != null)
                addExtraFieldButton.TouchUpInside += AddExtraField_TouchUpInside;

            if (extraFieldTextView?.InputAccessoryView is KeyboardObserverInputAccessoryView keyboardObserverInputAccessoryView)
                keyboardObserverInputAccessoryView.KeyboardChanged += KeyboardObserverInputAccessoryView_KeyboardChanged;

            TableView.RefreshControl.ValueChanged += RefreshControl_ValueChanged;

        }

        void DeInitializeHandlers()
        {
            if (doneButtonItem != null)
                doneButtonItem.Clicked -= DoneButtonItem_Clicked;

            if (addExtraFieldButton != null)
                addExtraFieldButton.TouchUpInside -= AddExtraField_TouchUpInside;

            if (extraFieldTextView != null && extraFieldTextView.InputAccessoryView is KeyboardObserverInputAccessoryView keyboardObserverInputAccessoryView)
                keyboardObserverInputAccessoryView.KeyboardChanged -= KeyboardObserverInputAccessoryView_KeyboardChanged;

            TableView.RefreshControl.ValueChanged -= RefreshControl_ValueChanged;
        }

        #endregion

        #region Refresh
        void RefreshControl_ValueChanged(object sender, EventArgs e) => RefreshData();


        async void RefreshData()
        {
            if (refreshing)
                return;

            refreshing = true;
            TableView.RefreshControl.ValueChanged -= RefreshControl_ValueChanged;

            CommonConfig.Logger.Info($"Refreshing list of extraFields");

            try
            {
                var extraFields = await Managers.DocumentsManager.GetExtraFieldsAsync();
                ((DataSource)TableView.Source).SetItems(extraFields);
                ((DataSource)TableView.Source).SortItems();

            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Could not refresh list of extra fields", ex);

                await Dialogs.ShowErrorAlertAsync(this, ex);
            }

            TableView.RefreshControl.EndRefreshing();
            TableView.RefreshControl.ValueChanged += RefreshControl_ValueChanged;

            refreshing = false;

        }

        #endregion

        #region Event handlers

        async void DoneButtonItem_Clicked(object sender, EventArgs e)
        {
            var extraFieldsList = ((DataSource)TableView.Source).Items;
            await Managers.DocumentsManager.UpdateExtraFieldsAsync(extraFieldsList);
            DismissViewController(true, null);
        }

        async void AddExtraField_TouchUpInside(object sender, EventArgs e)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("adding_extra_field___"));

            try
            {

                var newField = await Managers.DocumentsManager.AddExtraFieldAsync(extraFieldTextView.Text);

                ((DataSource)TableView.Source).AddExtraField(newField);
                ((DataSource)TableView.Source).SortItems();
                extraFieldTextView.Text = null;
                extraFieldTextView.EndEditing(true);

                ScrollTextScrollToBottom(true);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Failed to add extra field [name={extraFieldTextView.Text}] ", ex);

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

        async void DeleteExtraField(ExtraField extraField)
        {
            TableView.Editing = false;

            try
            {

                var deleteConfirmed = await Dialogs.ShowYesNoAlertAsync(this, Localization.GetString("confirm_extra_field_deletion_title"),
                                                                      string.Format(Localization.GetString("confirm_extra_field_deletion_content")));
                if (!deleteConfirmed)
                    return;

                var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("deleting_extra_field___"));
                await Managers.DocumentsManager.DeleteExtraFieldAsync(extraField.FieldId);
                dismissAction();

                ((DataSource)TableView.Source).RemoveExtraField(extraField);
                ((DataSource)TableView.Source).SortItems();

            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Failed to delete extra field [Id={extraField.FieldId}] "
                                             , ex.InnerException);
                await Dialogs.ShowErrorAlertAsync(this, ex.InnerException);
            }

        }

        async void EditExtraField(ExtraField extraField)
        {
            TableView.Editing = false;

            try
            {
                var dp = new StringEditorViewController
                {
                    ModalPresentationStyle = UIModalPresentationStyle.OverCurrentContext,
                    ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve
                };
                PresentViewController(dp, false, null);

                var newName = await dp.Result;
                var updatedExtraField = new ExtraField { FieldId = extraField.FieldId, FieldName = newName, Enabled = extraField.Enabled };

                var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("editing_extra_field___"));
                await Managers.DocumentsManager.UpdateExtraFieldAsync(updatedExtraField);
                dismissAction();

                ((DataSource)TableView.Source).UpdateItem(updatedExtraField);
                ((DataSource)TableView.Source).SortItems();
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Failed to update extra field [Id={extraField.FieldId}] "
                                             , ex.InnerException);
                await Dialogs.ShowErrorAlertAsync(this, ex.InnerException);
            }

        }

        #endregion

        #region Keyboard handling

        void KeyboardObserverInputAccessoryView_KeyboardChanged(object sender, CGRect frame)
        {
            var height = KeyboardObserverInputAccessoryView.GetVisibleKeyboardHeight(View, frame);

            if (Integration.IsRunningAtLeast(11))
                extraFieldViewBottomConstraint.Constant = height <= 0 ? 0 : -height + (View.SafeAreaInsets.Bottom);
            else
                extraFieldViewBottomConstraint.Constant = -height;

            extraFieldView.LayoutIfNeeded();

            AdjustSafeAreaInsets();
        }

        #endregion

        #region IUITextViewDelegate

        [Export("textViewDidChange:")]
        public void Changed(UITextViewScalable textView)
        {
            UpdateAddExtraFieldEnabled();
            UpdateExtraFieldTextViewHeight();
        }

        [Export("textViewDidEndEditing:")]
        public void EditingEnded(UITextViewScalable textView)
        {
            UpdateAddExtraFieldEnabled();
            UpdateExtraFieldTextViewHeight();
        }

        #endregion

        #region Utilities

        void UpdateAddExtraFieldEnabled()
        {
            if (addExtraFieldButton == null)
                return;
            addExtraFieldButton.Enabled = !string.IsNullOrEmpty(extraFieldTextView.Text);
        }


        void UpdateExtraFieldTextViewHeight()
        {
            if (NavigationController == null || extraFieldTextView == null)
                return;

            var requiredHeight = extraFieldTextView.SizeThatFits(new CGSize(extraFieldTextView.ContentSize.Width,
                nfloat.MaxValue)).Height;
            var targetHeight = Math.Min(Math.Max(requiredHeight, MinTextViewHeight), MaxCommentTextViewHeight);

            extraFieldTextScrollViewHeightConstraint.Constant = (nfloat)targetHeight;
            extraFieldTextScrollView.ScrollEnabled = requiredHeight > targetHeight;

            extraFieldView.LayoutIfNeeded();

            AdjustSafeAreaInsets();
        }

        void AdjustSafeAreaInsets()
        {
            var offset = -extraFieldViewBottomConstraint.Constant + extraFieldView.Frame.Height;
            TableView.ContentInset = new UIEdgeInsets(TableView.ContentInset.Top, 0f, offset, 0f);
        }

        void ScrollTextScrollToBottom(bool animated)
        {
            if (!((DataSource)TableView.Source).Empty)
                TableView.ScrollToRow(NSIndexPath.FromRowSection(((DataSource)TableView.Source).Items.Count - 1, 0),
                                      UITableViewScrollPosition.Bottom,
                                      animated);
        }
        #endregion

        #region DataSource

        class DataSource : UITableViewSource
        {
            public bool Empty => !Items.Any();
            public List<ExtraField> Items { get; } = new List<ExtraField>();

            readonly WeakReference<ExtraFieldsViewController> viewControllerWeakReference;
            readonly WeakReference<UITableView> tableViewWeakReference;

            bool loading = true;

            public DataSource(ExtraFieldsViewController viewController, UITableView tableView)
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
                    emptyCell.Initialize(Localization.GetString("no_extra_fields"));
                    return emptyCell;
                }

                var cell = tableView.DequeueReusableCell(ExtraFieldsTableViewCell.DefaultId) as ExtraFieldsTableViewCell ?? new ExtraFieldsTableViewCell();
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
                if (!(viewControllerWeakReference.Unwrap().IsEditingAvailable))
                    return null;

                var actions = new List<UITableViewRowAction>();

                if (indexPath.Row < 0 || indexPath.Row >= Items.Count)
                    return actions.ToArray();

                var extraField = Items[indexPath.Row];

   
                var deleteAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Destructive,
                                                                Localization.GetString("delete"),
                                                                (a, ip) => viewControllerWeakReference.Unwrap()?.DeleteExtraField(extraField));

                var editAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Destructive,
                                                                Localization.GetString("edit"),
                                                                (a, ip) => viewControllerWeakReference.Unwrap()?.EditExtraField(extraField));
                deleteAction.BackgroundColor = Theme.DarkerBlue;
                editAction.BackgroundColor = Theme.DarkBlue;
                actions.Add(deleteAction);
                actions.Add(editAction);
                

                return actions.ToArray();
            }

            public void SetItems(List<ExtraField> extraFields)
            {
                loading = false;

                Items.Clear();
                Items.AddRange(extraFields);
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            public void UpdateItem(ExtraField extraField)
            {
                var indexPath = Items.FindIndex(c => c.FieldId == extraField.FieldId);
    
                Items[indexPath] = extraField;
                tableViewWeakReference.Unwrap()?.ReloadRows(new[] { NSIndexPath.FromRowSection(indexPath, 0) }, UITableViewRowAnimation.Fade);

            }

            public void Reset()
            {
                loading = true;

                Items.Clear();
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            public void AddExtraField(ExtraField extraField)
            {
                Items.Add(extraField);

                if (Items.Count == 1)
                    tableViewWeakReference.Unwrap()?.ReloadRows(new[] { NSIndexPath.FromRowSection(Items.Count - 1, 0) }, UITableViewRowAnimation.Fade);
                else
                    tableViewWeakReference.Unwrap()?.InsertRows(new[] { NSIndexPath.FromRowSection(Items.Count - 1, 0) }, UITableViewRowAnimation.Fade);
            }

            public void RemoveExtraField(ExtraField extraField)
            {
                var position = Items.FindIndex(c => c.FieldId == extraField.FieldId);
                if (position >= 0)
                {
                    Items.RemoveAt(position);

                    if (Items.Count == 0)
                        tableViewWeakReference.Unwrap()?.ReloadRows(new[] { NSIndexPath.FromRowSection(position, 0) }, UITableViewRowAnimation.Fade);
                    else
                        tableViewWeakReference.Unwrap()?.DeleteRows(new[] { NSIndexPath.FromRowSection(position, 0) }, UITableViewRowAnimation.Fade);
                }
            }

            public void SortItems()
            {
                Items.Sort();
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }
        }

        #endregion

    }
}