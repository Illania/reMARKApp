using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using Mark5.Mobile.IOS.Utilities.Extensions;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.SearchView
{
    public abstract class AbstractSearchCriteriaViewController : AbstractViewController
    {
        const float BottomViewSize = 64f;

        UIBarButtonItem closeItem;
        UIBarButtonItem resetItem;

        UIScrollView scrollView;
        protected UIStackView StackView;
        protected UIButton SearchButton;

        NSLayoutConstraint searchButtonBottomConstraint1;
        NSLayoutConstraint searchButtonBottomConstraint2;

        NSObject didShowNotificationObserver;
        NSObject willChangeFrameNotificationObserver;
        NSObject willHideNotificationObserver;

        WeakReference<UIView> activeViewWeakReference;

        protected bool RestoreCriteriaFromStorage;

        public override void LoadView()
        {
            base.LoadView();

            RestoreCriteriaFromStorage = true;

            Title = Localization.GetString("search");

            closeItem = new UIBarButtonItem
            {
                Title = Localization.GetString("close")
            };
            NavigationItem.SetLeftBarButtonItem(closeItem, false);

            resetItem = new UIBarButtonItem
            {
                Title = Localization.GetString("reset")
            };
            NavigationItem.SetRightBarButtonItem(resetItem, false);

            scrollView = new UIScrollView
            {
                BackgroundColor = Theme.DarkerBlue,
                ShowsVerticalScrollIndicator = false,
                ShowsHorizontalScrollIndicator = false,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            View.AddSubview(scrollView);
            View.AddConstraints(new[]
            {
                scrollView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                scrollView.LeftAnchor.ConstraintEqualTo(View.LeftAnchor),
                scrollView.RightAnchor.ConstraintEqualTo(View.RightAnchor),
                scrollView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor)
            });

            StackView = new UIStackView
            {
                Axis = UILayoutConstraintAxis.Vertical,
                Alignment = UIStackViewAlignment.Fill,
                Distribution = UIStackViewDistribution.EqualSpacing,
                LayoutMargins = new UIEdgeInsets(10f, 10f, 10f, 10f),
                LayoutMarginsRelativeArrangement = true,
                Spacing = 10f,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            scrollView.AddSubview(StackView);

            if (Integration.IsIPadOrMac())
            {
                scrollView.AddConstraints(new[]
                {
                    StackView.TopAnchor.ConstraintEqualTo(scrollView.TopAnchor),
                    StackView.BottomAnchor.ConstraintEqualTo(scrollView.BottomAnchor),
                    StackView.CenterXAnchor.ConstraintEqualTo(scrollView.CenterXAnchor),
                    StackView.WidthAnchor.ConstraintEqualTo(500f)
                });
            }
            else
            {
                scrollView.AddConstraints(new[]
                {
                    StackView.LeftAnchor.ConstraintEqualTo(scrollView.LeftAnchor),
                    StackView.TopAnchor.ConstraintEqualTo(scrollView.TopAnchor),
                    StackView.RightAnchor.ConstraintEqualTo(scrollView.RightAnchor),
                    StackView.BottomAnchor.ConstraintEqualTo(scrollView.BottomAnchor),
                    StackView.WidthAnchor.ConstraintEqualTo(scrollView.WidthAnchor)
               });
            }

            SearchButton = new UIButton
            {
                TintColor = Theme.DarkerBlue,
                BackgroundColor = Theme.LightBlue,
                ClipsToBounds = true,
                ContentEdgeInsets = new UIEdgeInsets(14f, 14f, 14f, 14f),
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            SearchButton.SetTitle(Localization.GetString("search").ToUpper(), UIControlState.Normal);
            SearchButton.SetTitleColor(Theme.DarkBlue, UIControlState.Normal);
            SearchButton.TitleLabel.Font = Theme.DefaultLightFont;
            SearchButton.Layer.CornerRadius = 4f;
            View.AddSubview(SearchButton);
            View.AddConstraints(new[]
            {
                SearchButton.HeightAnchor.ConstraintEqualTo(55f),
                SearchButton.WidthAnchor.ConstraintEqualTo(150f),
                SearchButton.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor)
            });

            if (Integration.IsRunningAtLeast(11))
                searchButtonBottomConstraint1 = SearchButton.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor, -8f);
            else
                searchButtonBottomConstraint1 = SearchButton.BottomAnchor.ConstraintEqualTo(BottomLayoutGuide.GetTopAnchor(), -8f);

            searchButtonBottomConstraint2 = SearchButton.BottomAnchor.ConstraintEqualTo(View.BottomAnchor);
            searchButtonBottomConstraint2.Active = false;

            View.AddConstraints(new[]
            {
                searchButtonBottomConstraint1,
                searchButtonBottomConstraint2
            });
        }

        public override async void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (Integration.IsRunningAtLeast(11))
            {
                if (NavigationController != null)
                    NavigationController.NavigationBar.PrefersLargeTitles = false;
                NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Automatic;
            }

            closeItem.Clicked += CloseItem_Clicked;
            resetItem.Clicked += ResetItem_Clicked;
            SearchButton.TouchUpInside += SearchButton_TouchUpInside;

            foreach (var view in StackView.Subviews.OfType<AbstractSearchView>())
                view.Activated += View_Activated;

            didShowNotificationObserver = UIKeyboard.Notifications.ObserveDidShow(OnKeyboardDidShowNotification);
            willChangeFrameNotificationObserver = UIKeyboard.Notifications.ObserveWillChangeFrame(OnKeyboardWillChangeFrameNotification);
            willHideNotificationObserver = UIKeyboard.Notifications.ObserveWillHide(OnKeyboardWillHideNotification);

            if (RestoreCriteriaFromStorage)
            {
                RestoreCriteriaFromStorage = false;
                await RestoreCriteria();
            }

            RefreshView();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            closeItem.Clicked -= CloseItem_Clicked;
            resetItem.Clicked -= ResetItem_Clicked;
            SearchButton.TouchUpInside -= SearchButton_TouchUpInside;

            foreach (var view in StackView.Subviews.OfType<AbstractSearchView>())
                view.Activated -= View_Activated;

            didShowNotificationObserver?.Dispose();
            willChangeFrameNotificationObserver?.Dispose();
            willHideNotificationObserver?.Dispose();
        }

        protected override void Recycle()
        {
            base.Recycle();

            closeItem = null;
            resetItem = null;

            scrollView = null;
            StackView = null;
            SearchButton = null;

            searchButtonBottomConstraint1 = null;
            searchButtonBottomConstraint2 = null;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (CommonConfig.Logger.IsDebugEnabled())
                CommonConfig.Logger.Debug("Disposed");
        }

        async void CloseItem_Clicked(object sender, EventArgs e)
        {
            await SaveCriteria();

            if (NavigationController != null)
                NavigationController.ModalTransitionStyle = UIModalTransitionStyle.CoverVertical;

            DismissViewController(true, null);
        }

        protected virtual void ResetItem_Clicked(object sender, EventArgs e) => View.EndEditing(true);

        protected abstract void SearchButton_TouchUpInside(object sender, EventArgs e);

        protected abstract Task SaveCriteria();

        protected abstract Task RestoreCriteria();

        protected abstract void RefreshView();

        void View_Activated(object sender, EventArgs e) => activeViewWeakReference = ((UIView)sender).Wrap();

        void OnKeyboardDidShowNotification(object sender, UIKeyboardEventArgs e) => AdjustViewToKeyboard(e);
        void OnKeyboardWillChangeFrameNotification(object sender, UIKeyboardEventArgs e) => AdjustViewToKeyboard(e);
        void OnKeyboardWillHideNotification(object sender, UIKeyboardEventArgs e) => AdjustViewToKeyboard(e);

        void AdjustViewToKeyboard(UIKeyboardEventArgs e)
        {
            var keyboardOffset = scrollView.Frame.Bottom - e.FrameEnd.Top;

            if (Integration.IsRunningAtLeast(11))
            {
                var safeAreaOffset = keyboardOffset;

                if (keyboardOffset > 0)
                {
                    safeAreaOffset += 75f;

                    searchButtonBottomConstraint1.Active = false;
                    searchButtonBottomConstraint2.Active = true;
                    searchButtonBottomConstraint2.Constant = -keyboardOffset - 8f;
                }
                else
                {
                    searchButtonBottomConstraint1.Active = true;
                    searchButtonBottomConstraint2.Active = false;
                    searchButtonBottomConstraint2.Constant = 0f;
                }

                UIView.AnimateNotify(e.AnimationDuration, 0d, e.GetAimationOptions(), () =>
                {
                    var asai = AdditionalSafeAreaInsets;
                    asai = new UIEdgeInsets(asai.Top, asai.Left, safeAreaOffset, asai.Right);
                    AdditionalSafeAreaInsets = asai;
                    View.LayoutIfNeeded();
                }, null);
            }
            else
            {
                scrollView.ContentInset = new UIEdgeInsets(NavigationController.NavigationBar.Frame.Bottom, 0f, BottomViewSize + keyboardOffset, 0f);
                scrollView.ScrollIndicatorInsets = new UIEdgeInsets(NavigationController.NavigationBar.Frame.Bottom, 0f, BottomViewSize + keyboardOffset, 0f);

                UIView.AnimateNotify(e.AnimationDuration, 0d, e.GetAimationOptions(), () =>
                {
                    searchButtonBottomConstraint1.Constant = -keyboardOffset - 8f;
                    View.LayoutIfNeeded();
                }, null);

                var activeField = activeViewWeakReference.Unwrap();

                if (activeField != null)
                {
                    var difference = activeField.Frame.Bottom - scrollView.ContentOffset.Y - (View.Frame.Height - keyboardOffset - BottomViewSize) + 10;

                    if (difference > 0)
                    {
                        var co = scrollView.ContentOffset;
                        co.Y += difference;
                        scrollView.ContentOffset = co;
                    }
                }
            }
        }

        protected abstract class AbstractSearchView : UIStackView
        {
            protected const float CornerRadius = 4f;
            protected const float InnerMargin = 2f;
            protected const float AnimationLength = .1f;

            protected static readonly UIColor LabelTextColor = Theme.LightBlue;
            protected static readonly UIColor InactiveTextColor = Theme.LightGray;
            protected static readonly UIColor ActiveTextColor = Theme.DarkerBlue;
            protected static readonly UIColor InactiveBackgroundColor = Theme.DarkBlue;
            protected static readonly UIColor ActiveBackgroundColor = Theme.LightBlue;
            protected static readonly UIFont Font = Theme.DefaultFont;

            public event EventHandler Activated;

            protected AbstractSearchView()
            {
                AddConstraint(this.HeightAnchor.ConstraintEqualTo(50f));

                Axis = UILayoutConstraintAxis.Horizontal;
                Alignment = UIStackViewAlignment.Fill;
                Distribution = UIStackViewDistribution.FillEqually;
                Spacing = InnerMargin;
            }

            protected abstract void UpdateRow();

            protected void SetLabelActive(UILabel label, bool active)
            {
                TransitionNotify(label, AnimationLength, UIViewAnimationOptions.TransitionCrossDissolve, () =>
                {
                    label.TextColor = active ? ActiveTextColor : InactiveTextColor;
                    label.BackgroundColor = active ? ActiveBackgroundColor : InactiveBackgroundColor;
                }, null);
            }

            protected void SetAsActive() => Activated?.Invoke(this, EventArgs.Empty);
        }
    }
}