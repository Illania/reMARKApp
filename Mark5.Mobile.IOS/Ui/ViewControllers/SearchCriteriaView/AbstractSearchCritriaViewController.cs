
//
// Project: Mark5.Mobile.IOS
// File: AbstractSearchCritriaViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.IO;
using System.Linq;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.SearchCriteriaView
{

    public abstract class AbstractSearchCriteriaViewController : AbstractViewController
    {

        const float BottomViewSize = 64f;

        UIBarButtonItem closeItem;
        UIBarButtonItem resetItem;

        UIView bottomView;
        UIScrollView scrollView;
        protected UIStackView StackView;
        protected UIButton SearchButton;

        NSObject didShowNotificationObserver;
        NSObject willChangeFrameNotificationObserver;
        NSObject willHideNotification;

        UIView activeField;

        NSLayoutConstraint bottomLayoutConstraint;

        bool firstRun = true;

        public override void LoadView()
        {
            base.LoadView();

            AutomaticallyAdjustsScrollViewInsets = false;

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
                ShowsVerticalScrollIndicator = false,
                ShowsHorizontalScrollIndicator = false,
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = Theme.DarkerBlue,
                ContentInset = new UIEdgeInsets(NavigationController.NavigationBar.Frame.Bottom, 0f, BottomViewSize, 0f),
                ScrollIndicatorInsets = new UIEdgeInsets(NavigationController.NavigationBar.Frame.Bottom, 0f, BottomViewSize, 0f)
            };
            View.AddSubview(scrollView);
            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(scrollView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(scrollView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(scrollView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(scrollView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1f, 0f)
            });

            StackView = new UIStackView
            {
                Axis = UILayoutConstraintAxis.Vertical,
                Alignment = UIStackViewAlignment.Fill,
                Distribution = UIStackViewDistribution.Fill,
                LayoutMargins = new UIEdgeInsets(10f, 10f, 10f, 10f),
                LayoutMarginsRelativeArrangement = true,
                Spacing = 10f,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            scrollView.AddSubview(StackView);

            var const1 = NSLayoutConstraint.Create(StackView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, scrollView, NSLayoutAttribute.Width, 1f, 0f);
            const1.Priority = 999f;
            var const2 = NSLayoutConstraint.Create(StackView, NSLayoutAttribute.Width, NSLayoutRelation.LessThanOrEqual, 1f, 500f);
            const2.Priority = 1000f;

            scrollView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(StackView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, scrollView, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(StackView, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, scrollView, NSLayoutAttribute.CenterX, 1f, 0f),
                NSLayoutConstraint.Create(StackView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, scrollView, NSLayoutAttribute.Bottom, 1f, 0f),
                const1,
                const2
            });

            bottomView = new TouchTransparentView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
            };
            View.AddSubview(bottomView);

            bottomLayoutConstraint = NSLayoutConstraint.Create(bottomView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1f, 0f);

            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(bottomView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, BottomViewSize),
                NSLayoutConstraint.Create(bottomView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, BottomViewSize),
                NSLayoutConstraint.Create(bottomView, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, View, NSLayoutAttribute.CenterX, 1f, 0f),
                bottomLayoutConstraint,
            });

            SearchButton = new UIButton
            {
                TintColor = Theme.DarkerBlue,
                BackgroundColor = Theme.LightBlue,
                TranslatesAutoresizingMaskIntoConstraints = false,
                ClipsToBounds = true,
                ContentEdgeInsets = new UIEdgeInsets(14f, 14f, 14f, 14f)
            };
            SearchButton.SetImage(UIImage.FromBundle(Path.Combine("icons", "search_large.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
            SearchButton.Layer.CornerRadius = 27.5f;
            bottomView.AddSubview(SearchButton);
            bottomView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(SearchButton, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 55f),
                NSLayoutConstraint.Create(SearchButton, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, 55f),
                NSLayoutConstraint.Create(SearchButton, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, bottomView, NSLayoutAttribute.CenterX, 1f, 0f),
                NSLayoutConstraint.Create(SearchButton, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, bottomView, NSLayoutAttribute.Bottom, 1f, -8f)
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ExtendedLayoutIncludesOpaqueBars = true;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            scrollView.ContentInset = new UIEdgeInsets(NavigationController.NavigationBar.Frame.Bottom, 0f, BottomViewSize, 0f);
            scrollView.ScrollIndicatorInsets = new UIEdgeInsets(NavigationController.NavigationBar.Frame.Bottom, 0f, BottomViewSize, 0f);

            closeItem.Clicked += CloseItem_Clicked;
            resetItem.Clicked += ResetItem_Clicked;
            SearchButton.TouchUpInside += SearchButton_TouchUpInside;

            foreach (var view in StackView.Subviews.OfType<AbstractSearchView>())
                view.Activated += View_Activated;

            didShowNotificationObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.DidShowNotification, OnKeyboardDidShowNotification);
            willChangeFrameNotificationObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillChangeFrameNotification, OnKeyboardWillChangeFrameNotification);
            willHideNotification = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, OnKeyboardWillHideNotification);

            if (firstRun)
            {
                firstRun = false;
                RestoreCriteria();
            }
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            scrollView.ContentInset = new UIEdgeInsets(NavigationController.NavigationBar.Frame.Bottom, 0f, BottomViewSize, 0f);
            scrollView.ScrollIndicatorInsets = new UIEdgeInsets(NavigationController.NavigationBar.Frame.Bottom, 0f, BottomViewSize, 0f);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            closeItem.Clicked -= CloseItem_Clicked;
            resetItem.Clicked -= ResetItem_Clicked;
            SearchButton.TouchUpInside -= SearchButton_TouchUpInside;

            foreach (var view in StackView.Subviews.OfType<AbstractSearchView>())
                view.Activated -= View_Activated;

            NSNotificationCenter.DefaultCenter.RemoveObservers(new[] { didShowNotificationObserver, willChangeFrameNotificationObserver, willHideNotification });
        }

        public override void ViewWillTransitionToSize(CGSize toSize, IUIViewControllerTransitionCoordinator coordinator)
        {
            base.ViewWillTransitionToSize(toSize, coordinator);

            coordinator.AnimateAlongsideTransition(ctx => { }, ctx =>
            {
                if (scrollView == null) return;

                scrollView.ContentInset = new UIEdgeInsets(NavigationController.NavigationBar.Frame.Bottom, 0f, BottomViewSize, 0f);
                scrollView.ScrollIndicatorInsets = new UIEdgeInsets(NavigationController.NavigationBar.Frame.Bottom, 0f, BottomViewSize, 0f);
            });
        }

        void View_Activated(object sender, EventArgs e) => activeField = sender as UIView;

        void CloseItem_Clicked(object sender, EventArgs e)
        {
            DismissViewController(true, null);

            SaveCriteria();
        }

        protected virtual void ResetItem_Clicked(object sender, EventArgs e)
        {
            View.EndEditing(true);
        }

        protected abstract void SearchButton_TouchUpInside(object sender, EventArgs e);

        protected abstract void SaveCriteria();

        protected abstract void RestoreCriteria();

        void OnKeyboardDidShowNotification(NSNotification notification) => AdjustViewToKeyboard(UI.KeyboardHeightFromNotification(notification), notification, true);

        void OnKeyboardWillChangeFrameNotification(NSNotification notification) => AdjustViewToKeyboard(UI.KeyboardHeightFromNotification(notification), notification);

        void OnKeyboardWillHideNotification(NSNotification notification) => AdjustViewToKeyboard(0f, notification);

        void AdjustViewToKeyboard(float keyboardHeight, NSNotification notification, bool correctOffset = false)
        {
            scrollView.ContentInset = new UIEdgeInsets(NavigationController.NavigationBar.Frame.Bottom, 0f, BottomViewSize + keyboardHeight, 0f);
            scrollView.ScrollIndicatorInsets = new UIEdgeInsets(NavigationController.NavigationBar.Frame.Bottom, 0f, BottomViewSize + keyboardHeight, 0f);

            if (notification == null)
            {
                View.LayoutIfNeeded();
                return;
            }

            var duration = UI.KeyboardAnimationDurationFromNotification(notification);
            var options = UI.KeyboardAnimationOptionsFromNotification(notification);
            UIView.AnimateNotify(duration, 0.0d, options, () =>
            {
                bottomLayoutConstraint.Constant = -keyboardHeight;
                View.LayoutIfNeeded();
            }, null);

            if (correctOffset && activeField != null)
            {
                var difference = activeField.Frame.Bottom - scrollView.ContentOffset.Y - (View.Frame.Height - keyboardHeight - BottomViewSize) + 10;

                if (difference > 0)
                {
                    var co = scrollView.ContentOffset;
                    co.Y += difference;
                    scrollView.SetContentOffset(co, true);
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

            public event EventHandler Activated = delegate { };

            protected AbstractSearchView()
            {
                AddConstraint(NSLayoutConstraint.Create(this, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 50f));

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

            protected void SetAsActive()
            {
                Activated(this, EventArgs.Empty);
            }
        }
    }
}
