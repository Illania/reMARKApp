//
// Project: Mark5.Mobile.IOS
// File: ContactsSearchCriteriaViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.IO;
using System.Linq;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using ObjCRuntime;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{

    public class ContactsSearchCriteriaViewController : AbstractViewController
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

        SearchContactsCriteria criteria = new SearchContactsCriteria();

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
                TranslatesAutoresizingMaskIntoConstraints = false
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
                NSLayoutConstraint.Create(searchButton, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, bottomView, NSLayoutAttribute.Bottom, 1f, -12f)
            });

            stackView.AddArrangedSubview(new ContactTypesSearchView());
            stackView.AddArrangedSubview(new NameSearchView());
            stackView.AddArrangedSubview(new AddressSearchView());
            stackView.AddArrangedSubview(new ShortIdDescriptionPhysicalAddressView());
            stackView.AddArrangedSubview(new CountryCategoriesView(this));

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
            criteria = new SearchContactsCriteria();

            foreach (var view in stackView.Subviews.OfType<AbstractSearchView>())
                view.SetCriteria(criteria);
        }

        void SearchButton_TouchUpInside(object sender, EventArgs e)
        {
            searchButton.TouchUpInside -= SearchButton_TouchUpInside;

            criteria.MaxToFetch = PlatformConfig.Preferences.ContactsToSearch;

            NavigationController.PushViewController(new ContactsSearchResultsViewController { Criteria = criteria }, true);
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

            protected SearchContactsCriteria Criteria;

            protected AbstractSearchView()
            {
                AddConstraint(NSLayoutConstraint.Create(this, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 50f));

                Axis = UILayoutConstraintAxis.Horizontal;
                Alignment = UIStackViewAlignment.Fill;
                Distribution = UIStackViewDistribution.FillEqually;
                Spacing = InnerMargin;
            }

            public void SetCriteria(SearchContactsCriteria criteria)
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

        class ContactTypesSearchView : AbstractSearchView
        {

            readonly UILabel personView;
            readonly UILabel departmentView;
            readonly UILabel companyView;

            public ContactTypesSearchView()
            {
                personView = new UILabel
                {
                    Text = Localization.GetString("search_person").ToUpper(),
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };
                personView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));
                personView.Layer.CornerRadius = CornerRadius;
                personView.Layer.MasksToBounds = true;
                AddArrangedSubview(personView);

                departmentView = new UILabel
                {
                    Text = Localization.GetString("search_department").ToUpper(),
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };
                departmentView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));
                departmentView.Layer.CornerRadius = CornerRadius;
                departmentView.Layer.MasksToBounds = true;
                AddArrangedSubview(departmentView);

                companyView = new UILabel
                {
                    Text = Localization.GetString("search_company").ToUpper(),
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };
                companyView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));
                companyView.Layer.CornerRadius = CornerRadius;
                companyView.Layer.MasksToBounds = true;
                AddArrangedSubview(companyView);
            }

            protected override void UpdateRow()
            {
                var types = Criteria.ContactTypes;

                if (!types.Any())
                    types.UnionWith(new[] { ContactType.Person, ContactType.Department, ContactType.Company });

                SetLabelActive(personView, types.Contains(ContactType.Person));
                SetLabelActive(departmentView, types.Contains(ContactType.Department));
                SetLabelActive(companyView, types.Contains(ContactType.Company));
            }

            [Export("tapped:")]
            void Tapped(UITapGestureRecognizer recognizer)
            {
                var types = Criteria.ContactTypes;

                if (recognizer.View == personView)
                {
                    if (types.Contains(ContactType.Person))
                        types.Remove(ContactType.Person);
                    else
                        types.Add(ContactType.Person);
                }
                else if (recognizer.View == departmentView)
                {
                    if (types.Contains(ContactType.Department))
                        types.Remove(ContactType.Department);
                    else
                        types.Add(ContactType.Department);
                }
                else if (recognizer.View == companyView)
                {
                    if (types.Contains(ContactType.Company))
                        types.Remove(ContactType.Company);
                    else
                        types.Add(ContactType.Company);
                }


                UpdateRow();
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
                    Text = Localization.GetString("search_email_number_etc"),
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
                text.Text = Criteria.ComAddress;
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
                Criteria.ComAddress = textField.Text;
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

        class ShortIdDescriptionPhysicalAddressView : AbstractSearchView
        {

            readonly UIView shortIdView;
            readonly UILabel shortIdLabel;
            readonly UITextField shortIdTextField;
            readonly UIView descriptionView;
            readonly UILabel descriptionLabel;
            readonly UITextField descriptionTextField;
            readonly UIView physicalAddressView;
            readonly UILabel physicalAddressLabel;
            readonly UITextField physicalAddressTextField;

            public ShortIdDescriptionPhysicalAddressView()
            {
                shortIdView = new UIView
                {
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true
                };
                shortIdView.Layer.CornerRadius = CornerRadius;
                shortIdView.Layer.MasksToBounds = true;
                shortIdView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));

                shortIdLabel = new UILabel
                {
                    Text = Localization.GetString("search_short_id"),
                    TextColor = LabelTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };

                shortIdTextField = new UITextField
                {
                    AttributedPlaceholder = new NSAttributedString(Localization.GetString("search_type"), new UIStringAttributes { ForegroundColor = Theme.LightGray }),
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TintColor = Theme.LightGray,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false,
                    WeakDelegate = this
                };
                shortIdTextField.AddTarget(this, new Selector("textFieldDidChange:"), UIControlEvent.EditingChanged);
                shortIdView.Add(shortIdLabel);
                shortIdView.Add(shortIdTextField);
                shortIdView.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(shortIdLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, shortIdView, NSLayoutAttribute.Top, 1f, 4f),
                    NSLayoutConstraint.Create(shortIdLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, shortIdView, NSLayoutAttribute.Left, 1f, 4f),
                    NSLayoutConstraint.Create(shortIdLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, shortIdView, NSLayoutAttribute.Right, 1f, -4f),
                    NSLayoutConstraint.Create(shortIdTextField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, shortIdLabel, NSLayoutAttribute.Bottom, 1f, 2f),
                    NSLayoutConstraint.Create(shortIdTextField, NSLayoutAttribute.Left, NSLayoutRelation.Equal, shortIdView, NSLayoutAttribute.Left, 1f, 4f),
                    NSLayoutConstraint.Create(shortIdTextField, NSLayoutAttribute.Right, NSLayoutRelation.Equal, shortIdView, NSLayoutAttribute.Right, 1f, -4f),
                    NSLayoutConstraint.Create(shortIdTextField, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, shortIdView, NSLayoutAttribute.Bottom, 1f, -4f),
                    NSLayoutConstraint.Create(shortIdLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, shortIdTextField, NSLayoutAttribute.Height, 1f, 0f)
                });

                AddArrangedSubview(shortIdView);

                descriptionView = new UIView
                {
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true
                };
                descriptionView.Layer.CornerRadius = CornerRadius;
                descriptionView.Layer.MasksToBounds = true;
                descriptionView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));

                descriptionLabel = new UILabel
                {
                    Text = Localization.GetString("search_description"),
                    TextColor = LabelTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };

                descriptionTextField = new UITextField
                {
                    AttributedPlaceholder = new NSAttributedString(Localization.GetString("search_type"), new UIStringAttributes { ForegroundColor = Theme.LightGray }),
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TintColor = Theme.LightGray,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false,
                    WeakDelegate = this
                };
                descriptionTextField.AddTarget(this, new Selector("textFieldDidChange:"), UIControlEvent.EditingChanged);
                descriptionView.Add(descriptionLabel);
                descriptionView.Add(descriptionTextField);
                descriptionView.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(descriptionLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, descriptionView, NSLayoutAttribute.Top, 1f, 4f),
                    NSLayoutConstraint.Create(descriptionLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, descriptionView, NSLayoutAttribute.Left, 1f, 4f),
                    NSLayoutConstraint.Create(descriptionLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, descriptionView, NSLayoutAttribute.Right, 1f, -4f),
                    NSLayoutConstraint.Create(descriptionTextField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, descriptionLabel, NSLayoutAttribute.Bottom, 1f, 2f),
                    NSLayoutConstraint.Create(descriptionTextField, NSLayoutAttribute.Left, NSLayoutRelation.Equal, descriptionView, NSLayoutAttribute.Left, 1f, 4f),
                    NSLayoutConstraint.Create(descriptionTextField, NSLayoutAttribute.Right, NSLayoutRelation.Equal, descriptionView, NSLayoutAttribute.Right, 1f, -4f),
                    NSLayoutConstraint.Create(descriptionTextField, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, descriptionView, NSLayoutAttribute.Bottom, 1f, -4f),
                    NSLayoutConstraint.Create(descriptionLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, descriptionTextField, NSLayoutAttribute.Height, 1f, 0f)
                });

                AddArrangedSubview(descriptionView);

                physicalAddressView = new UIView
                {
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true
                };
                physicalAddressView.Layer.CornerRadius = CornerRadius;
                physicalAddressView.Layer.MasksToBounds = true;
                physicalAddressView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));

                physicalAddressLabel = new UILabel
                {
                    Text = Localization.GetString("search_physical_address"),
                    TextColor = LabelTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };

                physicalAddressTextField = new UITextField
                {
                    AttributedPlaceholder = new NSAttributedString(Localization.GetString("search_type"), new UIStringAttributes { ForegroundColor = Theme.LightGray }),
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TintColor = Theme.LightGray,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false,
                    WeakDelegate = this
                };
                physicalAddressTextField.AddTarget(this, new Selector("textFieldDidChange:"), UIControlEvent.EditingChanged);
                physicalAddressView.Add(physicalAddressLabel);
                physicalAddressView.Add(physicalAddressTextField);
                physicalAddressView.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(physicalAddressLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, physicalAddressView, NSLayoutAttribute.Top, 1f, 4f),
                    NSLayoutConstraint.Create(physicalAddressLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, physicalAddressView, NSLayoutAttribute.Left, 1f, 4f),
                    NSLayoutConstraint.Create(physicalAddressLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, physicalAddressView, NSLayoutAttribute.Right, 1f, -4f),
                    NSLayoutConstraint.Create(physicalAddressTextField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, physicalAddressLabel, NSLayoutAttribute.Bottom, 1f, 2f),
                    NSLayoutConstraint.Create(physicalAddressTextField, NSLayoutAttribute.Left, NSLayoutRelation.Equal, physicalAddressView, NSLayoutAttribute.Left, 1f, 4f),
                    NSLayoutConstraint.Create(physicalAddressTextField, NSLayoutAttribute.Right, NSLayoutRelation.Equal, physicalAddressView, NSLayoutAttribute.Right, 1f, -4f),
                    NSLayoutConstraint.Create(physicalAddressTextField, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, physicalAddressView, NSLayoutAttribute.Bottom, 1f, -4f),
                    NSLayoutConstraint.Create(physicalAddressLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, physicalAddressTextField, NSLayoutAttribute.Height, 1f, 0f)
                });

                AddArrangedSubview(physicalAddressView);
            }

            protected override void UpdateRow()
            {
                shortIdTextField.Text = Criteria.ShortId;
                descriptionTextField.Text = Criteria.Description;
                physicalAddressTextField.Text = Criteria.PostAddress;
            }

            [Export("tapped:")]
            void Tapped(UITapGestureRecognizer recognizer)
            {
                if (recognizer.View == shortIdView)
                {
                    AnimateNotify(AnimationLength, () =>
                    {
                        descriptionView.Hidden = true;
                        physicalAddressView.Hidden = true;
                    }, ch =>
                    {
                        shortIdTextField.UserInteractionEnabled = true;
                        shortIdTextField.BecomeFirstResponder();
                    });
                }

                if (recognizer.View == descriptionView)
                {
                    AnimateNotify(AnimationLength, () =>
                    {
                        shortIdView.Hidden = true;
                        physicalAddressView.Hidden = true;
                    }, ch =>
                    {
                        descriptionTextField.UserInteractionEnabled = true;
                        descriptionTextField.BecomeFirstResponder();
                    });
                }

                if (recognizer.View == physicalAddressView)
                {
                    AnimateNotify(AnimationLength, () =>
                    {
                        shortIdView.Hidden = true;
                        descriptionView.Hidden = true;
                    }, ch =>
                    {
                        physicalAddressTextField.UserInteractionEnabled = true;
                        physicalAddressTextField.BecomeFirstResponder();
                    });
                }

                UpdateRow();
            }

            [Export("textFieldDidChange:")]
            void TextFieldDidChange(UITextField textField)
            {
                if (textField == shortIdTextField)
                    Criteria.ShortId = textField.Text;
                if (textField == descriptionTextField)
                    Criteria.Description = textField.Text;
                if (textField == physicalAddressTextField)
                    Criteria.PostAddress = textField.Text;
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
                if (textField == shortIdTextField)
                {
                    AnimateNotify(AnimationLength, () =>
                    {
                        descriptionView.Hidden = false;
                        physicalAddressView.Hidden = false;
                    }, ch =>
                    {
                        shortIdTextField.ResignFirstResponder();
                        shortIdTextField.UserInteractionEnabled = false;
                    });
                }

                if (textField == descriptionTextField)
                {
                    AnimateNotify(AnimationLength, () =>
                    {
                        shortIdView.Hidden = false;
                        physicalAddressView.Hidden = false;
                    }, ch =>
                    {
                        descriptionTextField.ResignFirstResponder();
                        descriptionTextField.UserInteractionEnabled = false;
                    });
                }

                if (textField == physicalAddressTextField)
                {
                    AnimateNotify(AnimationLength, () =>
                    {
                        shortIdView.Hidden = false;
                        descriptionView.Hidden = false;
                    }, ch =>
                    {
                        physicalAddressTextField.ResignFirstResponder();
                        physicalAddressTextField.UserInteractionEnabled = false;
                    });
                }
            }
        }

        class CountryCategoriesView : AbstractSearchView
        {

            readonly UIViewController parentViewController;

            readonly UIView countryView;
            readonly UILabel countryLabel;
            readonly UITextField countryValue;
            readonly UIView categoriesView;
            readonly UILabel categoriesLabel;
            readonly UILabel categoriesValue;

            readonly UIToolbar countryPickerToolbar;
            readonly Source countrySource;
            readonly UIPickerView countryPicker;

            public CountryCategoriesView(UIViewController parentViewController)
            {
                this.parentViewController = parentViewController;

                countryView = new UIView
                {
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true
                };
                countryView.Layer.CornerRadius = CornerRadius;
                countryView.Layer.MasksToBounds = true;
                countryView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));

                countryPickerToolbar = new UIToolbar(new CGRect(0f, 0f, 0f, 44f))
                {
                    Items = new[]
                    {
                        new UIBarButtonItem(UIBarButtonSystemItem.Cancel, this, new Selector("cancelTapped:")) { TintColor = Theme.DarkerBlue },
                        new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                        new UIBarButtonItem(UIBarButtonSystemItem.Done, this, new Selector("doneTapped:")) { TintColor = Theme.DarkerBlue }
                    }
                };

                countryLabel = new UILabel
                {
                    Text = Localization.GetString("search_country"),
                    TextColor = LabelTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };

                countrySource = new Source();
                countryPicker = new UIPickerView
                {
                    DataSource = countrySource,
                    Delegate = countrySource
                };

                countryValue = new UITextField
                {
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TintColor = UIColor.Clear,
                    TextAlignment = UITextAlignment.Center,
                    InputView = countryPicker,
                    InputAccessoryView = countryPickerToolbar,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false
                };
                countryView.Add(countryLabel);
                countryView.Add(countryValue);
                countryView.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(countryLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, countryView, NSLayoutAttribute.Top, 1f, 4f),
                    NSLayoutConstraint.Create(countryLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, countryView, NSLayoutAttribute.Left, 1f, 4f),
                    NSLayoutConstraint.Create(countryLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, countryView, NSLayoutAttribute.Right, 1f, -4f),
                    NSLayoutConstraint.Create(countryValue, NSLayoutAttribute.Top, NSLayoutRelation.Equal, countryLabel, NSLayoutAttribute.Bottom, 1f, 2f),
                    NSLayoutConstraint.Create(countryValue, NSLayoutAttribute.Left, NSLayoutRelation.Equal, countryView, NSLayoutAttribute.Left, 1f, 4f),
                    NSLayoutConstraint.Create(countryValue, NSLayoutAttribute.Right, NSLayoutRelation.Equal, countryView, NSLayoutAttribute.Right, 1f, -4f),
                    NSLayoutConstraint.Create(countryValue, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, countryView, NSLayoutAttribute.Bottom, 1f, -4f),
                    NSLayoutConstraint.Create(countryLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, countryValue, NSLayoutAttribute.Height, 1f, 0f)
                });

                AddArrangedSubview(countryView);

                categoriesView = new UIView
                {
                    BackgroundColor = InactiveBackgroundColor,
                    UserInteractionEnabled = true
                };
                categoriesView.Layer.CornerRadius = CornerRadius;
                categoriesView.Layer.MasksToBounds = true;
                categoriesView.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));

                categoriesLabel = new UILabel
                {
                    Text = Localization.GetString("search_categories"),
                    TextColor = LabelTextColor,
                    Font = Font,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };

                categoriesValue = new UILabel
                {
                    TextColor = InactiveTextColor,
                    Font = Font,
                    TintColor = Theme.LightGray,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    UserInteractionEnabled = false,
                    Lines = 1,
                    MinimumScaleFactor = .8f,
                    AdjustsFontSizeToFitWidth = true
                };
                categoriesView.Add(categoriesLabel);
                categoriesView.Add(categoriesValue);
                categoriesView.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(categoriesLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, categoriesView, NSLayoutAttribute.Top, 1f, 4f),
                    NSLayoutConstraint.Create(categoriesLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, categoriesView, NSLayoutAttribute.Left, 1f, 4f),
                    NSLayoutConstraint.Create(categoriesLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, categoriesView, NSLayoutAttribute.Right, 1f, -4f),
                    NSLayoutConstraint.Create(categoriesValue, NSLayoutAttribute.Top, NSLayoutRelation.Equal, categoriesLabel, NSLayoutAttribute.Bottom, 1f, 2f),
                    NSLayoutConstraint.Create(categoriesValue, NSLayoutAttribute.Left, NSLayoutRelation.Equal, categoriesView, NSLayoutAttribute.Left, 1f, 4f),
                    NSLayoutConstraint.Create(categoriesValue, NSLayoutAttribute.Right, NSLayoutRelation.Equal, categoriesView, NSLayoutAttribute.Right, 1f, -4f),
                    NSLayoutConstraint.Create(categoriesValue, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, categoriesView, NSLayoutAttribute.Bottom, 1f, -4f),
                    NSLayoutConstraint.Create(categoriesLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, categoriesValue, NSLayoutAttribute.Height, 1f, 0f)
                });

                AddArrangedSubview(categoriesView);
            }

            protected override void UpdateRow()
            {
                if (Criteria.CountryPrefix == -1)
                {
                    countryValue.Text = Localization.GetString("search_any");
                }
                else
                {
                    var ci = countrySource.CountryByPrefix(Criteria.CountryPrefix);
                    if (string.IsNullOrWhiteSpace(ci.CCode))
                        countryValue.Text = ci.Name;
                    else
                        countryValue.Text = ci.CCode;
                }

                categoriesValue.Text = Criteria.CategoryIds.Count < 1 ? Localization.GetString("search_any") : Criteria.CategoryIds.Count.ToString();
            }

            [Export("tapped:")]
#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
            async void Tapped(UITapGestureRecognizer recognizer)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
            {
                if (recognizer.View == countryView)
                {
                    countryPicker.ReloadAllComponents();

                    if (Criteria.CountryPrefix >= 0)
                        countrySource.SelectCountryByFaxPrefix(countryPicker, Criteria.CountryPrefix);
                    else
                        countryPicker.Select(0, 0, false);

                    countryValue.UserInteractionEnabled = true;
                    countryValue.BecomeFirstResponder();
                }

                if (recognizer.View == categoriesView)
                {
                    var vc = new CategoriesSelectListViewController(ModuleType.Contacts);
                    parentViewController.PresentViewController(new NavigationController(vc, UIModalPresentationStyle.FormSheet), true, null);

                    var result = await vc.Task;

                    if (result == null)
                        return;

                    Criteria.CategoryIds = result.Select(c => c.Id).ToList();
                }

                UpdateRow();
            }

            [Export("doneTapped:")]
            void DoneTapped(UIBarButtonItem sender)
            {
                countryValue.UserInteractionEnabled = false;
                Criteria.CountryPrefix = countrySource.SelectedCountryFaxPrefix(countryPicker);

                UpdateRow();
            }

            [Export("cancelTapped:")]
            void CancelTapped(UIBarButtonItem sender)
            {
                countryValue.UserInteractionEnabled = false;
            }

            class Source : UIPickerViewDataSource, IUIPickerViewDelegate
            {

                readonly CountryInfo[] countries = ServerConfig.SystemSettings.ContactsModuleInfo.Countries.OrderBy(ci => ci.Name).ToArray();

                public override nint GetComponentCount(UIPickerView pickerView)
                {
                    return 1;
                }

                public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
                {
                    return countries.Length;
                }

                [Export("pickerView:titleForRow:forComponent:")]
                public string GetTitle(UIPickerView picker, nint row, nint component)
                {
                    var ci = countries[row];

                    if (string.IsNullOrWhiteSpace(ci.CCode))
                        return ci.Name;

                    return $"{ci.Name} [{ci.CCode}/{ci.CCode3}]";
                }

                public void SelectCountryByFaxPrefix(UIPickerView picker, int faxPrefix)
                {
                    var index = 0;
                    for (int i = 0; i < countries.Length; i++)
                    {
                        if (countries[i].FaxPrefix == faxPrefix)
                        {
                            index = i;
                            break;
                        }
                    }

                    picker.Select(index, 0, true);
                }

                public int SelectedCountryFaxPrefix(UIPickerView picker)
                {
                    var selectedIndex = picker.SelectedRowInComponent(0);
                    return countries[selectedIndex].FaxPrefix;
                }

                public CountryInfo CountryByPrefix(int faxPrefix)
                {
                    for (int i = 0; i < countries.Length; i++)
                        if (countries[i].FaxPrefix == faxPrefix)
                            return countries[i];

                    return null;
                }
            }
        }
    }
}