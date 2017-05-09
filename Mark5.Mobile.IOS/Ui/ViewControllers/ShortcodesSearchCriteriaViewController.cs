//
// Project: Mark5.Mobile.IOS
// File: ShortcodesSearchCriteriaViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.IO;
using System.Linq;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using ObjCRuntime;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{

    public class ShortcodesSearchCriteriaViewController : AbstractViewController
    {

        const float BottomViewSize = 64f;

        UIBarButtonItem closeItem;
        UIBarButtonItem resetItem;

        UIView bottomView;
        UIScrollView scrollView;
        UIStackView stackView;
        UIButton searchButton;

        NSObject didShowNotificationObserver;
        NSObject willChangeFrameNotificationObserver;
        NSObject willHideNotification;

        SearchShortcodesCriteria criteria = new SearchShortcodesCriteria();

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

            stackView = new UIStackView
            {
                Axis = UILayoutConstraintAxis.Vertical,
                Alignment = UIStackViewAlignment.Fill,
                Distribution = UIStackViewDistribution.Fill,
                LayoutMargins = new UIEdgeInsets(10f, 10f, 10f, 10f),
                LayoutMarginsRelativeArrangement = true,
                Spacing = 10f,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            scrollView.AddSubview(stackView);

            var const1 = NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, scrollView, NSLayoutAttribute.Width, 1f, 0f);
            const1.Priority = 999f;
            var const2 = NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Width, NSLayoutRelation.LessThanOrEqual, 1f, 500f);
            const2.Priority = 1000f;

            scrollView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, scrollView, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(stackView, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, scrollView, NSLayoutAttribute.CenterX, 1f, 0f),
                NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, scrollView, NSLayoutAttribute.Bottom, 1f, 0f),
                const1,
                const2
            });

            bottomView = new TouchTransparentView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
            };
            View.AddSubview(bottomView);
            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(bottomView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, BottomViewSize),
                NSLayoutConstraint.Create(bottomView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, BottomViewSize),
                NSLayoutConstraint.Create(bottomView, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, View, NSLayoutAttribute.CenterX, 1f, 0f),
                NSLayoutConstraint.Create(bottomView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1f, 0f)
            });

            searchButton = new UIButton
            {
                TintColor = Theme.DarkerBlue,
                BackgroundColor = Theme.LightBlue,
                TranslatesAutoresizingMaskIntoConstraints = false,
                ClipsToBounds = true,
                ContentEdgeInsets = new UIEdgeInsets(14f, 14f, 14f, 14f)
            };
            searchButton.SetImage(UIImage.FromBundle(Path.Combine("icons", "search_large.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
            searchButton.Layer.CornerRadius = 27.5f;
            bottomView.AddSubview(searchButton);
            bottomView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(searchButton, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 55f),
                NSLayoutConstraint.Create(searchButton, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, 55f),
                NSLayoutConstraint.Create(searchButton, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, bottomView, NSLayoutAttribute.CenterX, 1f, 0f),
                NSLayoutConstraint.Create(searchButton, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, bottomView, NSLayoutAttribute.Bottom, 1f, -8f)
            });

            stackView.AddArrangedSubview(new NameSearchView());
            stackView.AddArrangedSubview(new DescritpionSearchView());
            stackView.AddArrangedSubview(new AddressSearchView());

            foreach (var view in stackView.Subviews.OfType<AbstractSearchView>())
                view.SetCriteria(criteria);
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
            searchButton.TouchUpInside += SearchButton_TouchUpInside;

            didShowNotificationObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.DidShowNotification, OnKeyboardDidShowNotification);
            willChangeFrameNotificationObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillChangeFrameNotification, OnKeyboardWillChangeFrameNotification);
            willHideNotification = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, OnKeyboardWillHideNotification);
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
            searchButton.TouchUpInside -= SearchButton_TouchUpInside;

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

        void CloseItem_Clicked(object sender, EventArgs e) => DismissViewController(true, null);

        void ResetItem_Clicked(object sender, EventArgs e)
        {
            View.EndEditing(true);

            criteria = new SearchShortcodesCriteria();

            foreach (var view in stackView.Subviews.OfType<AbstractSearchView>())
                view.SetCriteria(criteria);
        }

        void SearchButton_TouchUpInside(object sender, EventArgs e)
        {
            searchButton.TouchUpInside -= SearchButton_TouchUpInside;

            criteria.MaxToFetch = PlatformConfig.Preferences.ShortcodesToSearch;

            CommonConfig.Logger.Info($"Starting search... [criteria={SerializationUtils.Serialize(criteria)}]");

            NavigationController.PushViewController(new ShortcodesSearchResultsViewController { Criteria = criteria }, true);
        }

        void OnKeyboardDidShowNotification(NSNotification notification) => AdjustViewToKeyboard(UI.KeyboardHeightFromNotification(notification), notification);

        void OnKeyboardWillChangeFrameNotification(NSNotification notification) => AdjustViewToKeyboard(UI.KeyboardHeightFromNotification(notification), notification);

        void OnKeyboardWillHideNotification(NSNotification notification) => AdjustViewToKeyboard(0f, notification);

        void AdjustViewToKeyboard(float keyboardHeight, NSNotification notification)
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
            UIView.AnimateNotify(duration, 0.0d, options, View.LayoutIfNeeded, null);
        }

        abstract class AbstractSearchView : UIStackView
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

            protected SearchShortcodesCriteria Criteria;

            protected AbstractSearchView()
            {
                AddConstraint(NSLayoutConstraint.Create(this, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 50f));

                Axis = UILayoutConstraintAxis.Horizontal;
                Alignment = UIStackViewAlignment.Fill;
                Distribution = UIStackViewDistribution.FillEqually;
                Spacing = InnerMargin;
            }

            public void SetCriteria(SearchShortcodesCriteria criteria)
            {
                Criteria = criteria;
                UpdateRow();
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
        }

        class NameSearchView : AbstractSearchView
        {

            readonly UIView view;
            readonly UILabel label;
            readonly UITextField text;

            public NameSearchView()
            {
                view = new UIView
                {
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true
                };
                view.Layer.CornerRadius = CornerRadius;
                view.Layer.MasksToBounds = true;
                view.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));

                label = new UILabel
                {
                    Text = Localization.GetString("search_name"),
                    TextColor = LabelTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Left,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };

                text = new UITextField
                {
                    AttributedPlaceholder = new NSAttributedString(Localization.GetString("search_enter_search_text"), new UIStringAttributes { ForegroundColor = Theme.LightGray }),
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TintColor = Theme.LightGray,
                    TextAlignment = UITextAlignment.Left,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false,
                    WeakDelegate = this
                };
                text.AddTarget(this, new Selector("textFieldDidChange:"), UIControlEvent.EditingChanged);
                view.Add(label);
                view.Add(text);
                view.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(label, NSLayoutAttribute.Top, NSLayoutRelation.Equal, view, NSLayoutAttribute.Top, 1f, 4f),
                    NSLayoutConstraint.Create(label, NSLayoutAttribute.Left, NSLayoutRelation.Equal, view, NSLayoutAttribute.Left, 1f, 12f),
                    NSLayoutConstraint.Create(label, NSLayoutAttribute.Right, NSLayoutRelation.Equal, view, NSLayoutAttribute.Right, 1f, -8f),
                    NSLayoutConstraint.Create(text, NSLayoutAttribute.Top, NSLayoutRelation.Equal, label, NSLayoutAttribute.Bottom, 1f, 2f),
                    NSLayoutConstraint.Create(text, NSLayoutAttribute.Left, NSLayoutRelation.Equal, view, NSLayoutAttribute.Left, 1f, 12f),
                    NSLayoutConstraint.Create(text, NSLayoutAttribute.Right, NSLayoutRelation.Equal, view, NSLayoutAttribute.Right, 1f, -8f),
                    NSLayoutConstraint.Create(text, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, view, NSLayoutAttribute.Bottom, 1f, -4f),
                    NSLayoutConstraint.Create(label, NSLayoutAttribute.Height, NSLayoutRelation.Equal, text, NSLayoutAttribute.Height, 1f, 0f)
                });

                AddArrangedSubview(view);
            }

            protected override void UpdateRow()
            {
                text.Text = Criteria.Name;
            }

            [Export("tapped:")]
            void Tapped(UITapGestureRecognizer recognizer)
            {
                text.UserInteractionEnabled = true;
                text.BecomeFirstResponder();

                UpdateRow();
            }

            [Export("textFieldDidChange:")]
            void TextFieldDidChange(UITextField textField)
            {
                Criteria.Name = textField.Text;
            }


            [Export("textFieldShouldReturn:")]
            bool TextFieldShouldReturn(UITextField textField)
            {
                textField.ResignFirstResponder();
                return true;
            }

            [Export("textFieldDidEndEditing:")]
            void TextFieldDidEndEditing(UITextField textField)
            {
                text.ResignFirstResponder();
                text.UserInteractionEnabled = false;
            }
        }

        class DescritpionSearchView : AbstractSearchView
        {

            readonly UIView view;
            readonly UILabel label;
            readonly UITextField text;

            public DescritpionSearchView()
            {
                view = new UIView
                {
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true
                };
                view.Layer.CornerRadius = CornerRadius;
                view.Layer.MasksToBounds = true;
                view.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));

                label = new UILabel
                {
                    Text = Localization.GetString("search_search_description"),
                    TextColor = LabelTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Left,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };

                text = new UITextField
                {
                    AttributedPlaceholder = new NSAttributedString(Localization.GetString("search_enter_search_text"), new UIStringAttributes { ForegroundColor = Theme.LightGray }),
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TintColor = Theme.LightGray,
                    TextAlignment = UITextAlignment.Left,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false,
                    WeakDelegate = this
                };
                text.AddTarget(this, new Selector("textFieldDidChange:"), UIControlEvent.EditingChanged);
                view.Add(label);
                view.Add(text);
                view.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(label, NSLayoutAttribute.Top, NSLayoutRelation.Equal, view, NSLayoutAttribute.Top, 1f, 4f),
                    NSLayoutConstraint.Create(label, NSLayoutAttribute.Left, NSLayoutRelation.Equal, view, NSLayoutAttribute.Left, 1f, 12f),
                    NSLayoutConstraint.Create(label, NSLayoutAttribute.Right, NSLayoutRelation.Equal, view, NSLayoutAttribute.Right, 1f, -8f),
                    NSLayoutConstraint.Create(text, NSLayoutAttribute.Top, NSLayoutRelation.Equal, label, NSLayoutAttribute.Bottom, 1f, 2f),
                    NSLayoutConstraint.Create(text, NSLayoutAttribute.Left, NSLayoutRelation.Equal, view, NSLayoutAttribute.Left, 1f, 12f),
                    NSLayoutConstraint.Create(text, NSLayoutAttribute.Right, NSLayoutRelation.Equal, view, NSLayoutAttribute.Right, 1f, -8f),
                    NSLayoutConstraint.Create(text, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, view, NSLayoutAttribute.Bottom, 1f, -4f),
                    NSLayoutConstraint.Create(label, NSLayoutAttribute.Height, NSLayoutRelation.Equal, text, NSLayoutAttribute.Height, 1f, 0f)
                });

                AddArrangedSubview(view);
            }

            protected override void UpdateRow()
            {
                text.Text = Criteria.Description;
            }

            [Export("tapped:")]
            void Tapped(UITapGestureRecognizer recognizer)
            {
                text.UserInteractionEnabled = true;
                text.BecomeFirstResponder();

                UpdateRow();
            }

            [Export("textFieldDidChange:")]
            void TextFieldDidChange(UITextField textField)
            {
                Criteria.Description = textField.Text;
            }


            [Export("textFieldShouldReturn:")]
            bool TextFieldShouldReturn(UITextField textField)
            {
                textField.ResignFirstResponder();
                return true;
            }

            [Export("textFieldDidEndEditing:")]
            void TextFieldDidEndEditing(UITextField textField)
            {
                text.ResignFirstResponder();
                text.UserInteractionEnabled = false;
            }
        }

        class AddressSearchView : AbstractSearchView
        {

            readonly UIView view;
            readonly UILabel label;
            readonly UITextField text;

            public AddressSearchView()
            {
                view = new UIView
                {
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true
                };
                view.Layer.CornerRadius = CornerRadius;
                view.Layer.MasksToBounds = true;
                view.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));

                label = new UILabel
                {
                    Text = Localization.GetString("search_emails"),
                    TextColor = LabelTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Left,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };

                text = new UITextField
                {
                    AttributedPlaceholder = new NSAttributedString(Localization.GetString("search_enter_search_text"), new UIStringAttributes { ForegroundColor = Theme.LightGray }),
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TintColor = Theme.LightGray,
                    TextAlignment = UITextAlignment.Left,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false,
                    WeakDelegate = this
                };
                text.AddTarget(this, new Selector("textFieldDidChange:"), UIControlEvent.EditingChanged);
                view.Add(label);
                view.Add(text);
                view.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(label, NSLayoutAttribute.Top, NSLayoutRelation.Equal, view, NSLayoutAttribute.Top, 1f, 4f),
                    NSLayoutConstraint.Create(label, NSLayoutAttribute.Left, NSLayoutRelation.Equal, view, NSLayoutAttribute.Left, 1f, 12f),
                    NSLayoutConstraint.Create(label, NSLayoutAttribute.Right, NSLayoutRelation.Equal, view, NSLayoutAttribute.Right, 1f, -8f),
                    NSLayoutConstraint.Create(text, NSLayoutAttribute.Top, NSLayoutRelation.Equal, label, NSLayoutAttribute.Bottom, 1f, 2f),
                    NSLayoutConstraint.Create(text, NSLayoutAttribute.Left, NSLayoutRelation.Equal, view, NSLayoutAttribute.Left, 1f, 12f),
                    NSLayoutConstraint.Create(text, NSLayoutAttribute.Right, NSLayoutRelation.Equal, view, NSLayoutAttribute.Right, 1f, -8f),
                    NSLayoutConstraint.Create(text, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, view, NSLayoutAttribute.Bottom, 1f, -4f),
                    NSLayoutConstraint.Create(label, NSLayoutAttribute.Height, NSLayoutRelation.Equal, text, NSLayoutAttribute.Height, 1f, 0f)
                });

                AddArrangedSubview(view);
            }

            protected override void UpdateRow()
            {
                text.Text = Criteria.Address;
            }

            [Export("tapped:")]
            void Tapped(UITapGestureRecognizer recognizer)
            {
                text.UserInteractionEnabled = true;
                text.BecomeFirstResponder();

                UpdateRow();
            }

            [Export("textFieldDidChange:")]
            void TextFieldDidChange(UITextField textField)
            {
                Criteria.Address = textField.Text;
            }


            [Export("textFieldShouldReturn:")]
            bool TextFieldShouldReturn(UITextField textField)
            {
                textField.ResignFirstResponder();
                return true;
            }

            [Export("textFieldDidEndEditing:")]
            void TextFieldDidEndEditing(UITextField textField)
            {
                text.ResignFirstResponder();
                text.UserInteractionEnabled = false;
            }
        }
    }
}