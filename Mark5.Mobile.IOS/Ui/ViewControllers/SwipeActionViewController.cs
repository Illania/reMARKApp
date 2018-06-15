using System;
using CoreGraphics;
using Foundation;
using System.Collections.Generic;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using Mark5.Mobile.IOS.Utilities.Extensions;
using UIKit;
using System.Linq;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class SwipeActionViewController : AbstractViewController
    {

        NSObject observer;

        protected UIScrollView scrollView;
        protected UIStackView stackView;
        protected float innerMargin = 5f;
        protected float swipeContainerHeight = 160f;

        UIButton leadingSwipeButton;
        UIButton middleSwipeButton;
        UIButton lastSwipeButton;

        static nint leadingBtnTag = 0;
        static nint trailingMiddleBtnTag = 1;
        static nint trailingLastBtnTag = 2;

        private nfloat swipeActionBtnWidth = 80f;

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            observer = NSNotificationCenter.DefaultCenter.AddObserver((NSString)"NSUserDefaultsDidChangeNotification", DefaultsChanged);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            if (observer != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(observer);
                observer = null;
            }
        }

        void DefaultsChanged(NSNotification obj)
        {
            if (leadingSwipeButton != null)
            {
                var action = PlatformConfig.Preferences.EmailLeadingSwipeActions.First();
                if (action != null) leadingSwipeButton.SetTitle(action.GetName(), UIControlState.Normal);
            }

            if (middleSwipeButton != null)
            {
                var action = PlatformConfig.Preferences.EmailTrailingSwipeActions.ElementAt(1);
                if (action != null) middleSwipeButton.SetTitle(action.GetName(), UIControlState.Normal);
            }

            if (lastSwipeButton != null)
            {
                var action = PlatformConfig.Preferences.EmailTrailingSwipeActions.Last();
                if (action != null) lastSwipeButton.SetTitle(action.GetName(), UIControlState.Normal);
            }
        }

        public override void LoadView()
        {
            base.LoadView();

            Title = Localization.GetString("email_swipe_actions");

            scrollView = new UIScrollView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = UIColor.GroupTableViewBackgroundColor,
                ShowsVerticalScrollIndicator = false,
                ShowsHorizontalScrollIndicator = false,
            };

            View.AddSubview(scrollView);

            scrollView.TopAnchor.ConstraintEqualTo(View.TopAnchor).Active = true;
            scrollView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;
            scrollView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            scrollView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;

            scrollView.ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Always;

            UILabel topLabel = new UILabel()
            {
                Font = Theme.DefaultFont,
                TextColor = Theme.DarkGray,
                LineBreakMode = UILineBreakMode.WordWrap,
                Lines = 0,
                BackgroundColor = Theme.Clear,
                Text = Localization.GetString("swipe_settings_info"),
                TextAlignment = UITextAlignment.Left,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            scrollView.AddSubview(topLabel);

            if (Integration.IsIPad())
            {
                scrollView.AddConstraint(NSLayoutConstraint.Create(topLabel, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, scrollView.ReadableContentGuide, NSLayoutAttribute.Leading, 1f, 0f));
            }
            else
            {
                scrollView.AddConstraint(NSLayoutConstraint.Create(topLabel, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, scrollView.ReadableContentGuide, NSLayoutAttribute.Leading, 2f, 0f));
            }

            scrollView.AddConstraints(new[] {
                NSLayoutConstraint.Create(topLabel, NSLayoutAttribute.Trailing,  NSLayoutRelation.Equal, scrollView.ReadableContentGuide, NSLayoutAttribute.Trailing, 1f, 0f),
                NSLayoutConstraint.Create(topLabel, NSLayoutAttribute.Top,  NSLayoutRelation.Equal, scrollView, NSLayoutAttribute.Top, 1f, 10f),
            });

            #region Leading Swipe
            UIView leadingSwipeBg = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = Theme.White
            };

            scrollView.AddSubview(leadingSwipeBg);
            scrollView.AddConstraints(new[] {
                NSLayoutConstraint.Create(leadingSwipeBg, NSLayoutAttribute.Width,  NSLayoutRelation.Equal, scrollView, NSLayoutAttribute.Width, 1f, 0f),
                NSLayoutConstraint.Create(leadingSwipeBg, NSLayoutAttribute.CenterX,  NSLayoutRelation.Equal, scrollView, NSLayoutAttribute.CenterX, 1f, 0f),
                NSLayoutConstraint.Create(leadingSwipeBg, NSLayoutAttribute.Top,  NSLayoutRelation.Equal, topLabel, NSLayoutAttribute.Bottom, 1f, 10f),
                NSLayoutConstraint.Create(leadingSwipeBg, NSLayoutAttribute.Height,  NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, swipeContainerHeight),
            });

            UIView leadingSwipe = BuildLeadingSwipe();
            scrollView.AddSubview(leadingSwipe);

            if (Integration.IsIPad())
            {
                scrollView.AddConstraint(NSLayoutConstraint.Create(leadingSwipe, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, scrollView.ReadableContentGuide, NSLayoutAttribute.Leading, 1f, 0f));
            }
            else
            {
                scrollView.AddConstraint(NSLayoutConstraint.Create(leadingSwipe, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, scrollView.ReadableContentGuide, NSLayoutAttribute.Leading, 2f, 0f));
            }

            scrollView.AddConstraints(new[] {
                NSLayoutConstraint.Create(leadingSwipe, NSLayoutAttribute.Trailing,  NSLayoutRelation.Equal, scrollView.ReadableContentGuide, NSLayoutAttribute.Trailing, 1f, 0f),
                NSLayoutConstraint.Create(leadingSwipe, NSLayoutAttribute.Top,  NSLayoutRelation.Equal, topLabel, NSLayoutAttribute.Bottom, 1f, 10f),
                NSLayoutConstraint.Create(leadingSwipe, NSLayoutAttribute.Height,  NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, swipeContainerHeight),
            });

            #endregion

            #region Trailing Swipe Middle
            UIView trailingSwipeMiddleBg = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = Theme.White
            };

            scrollView.AddSubview(trailingSwipeMiddleBg);
            scrollView.AddConstraints(new[] {
                NSLayoutConstraint.Create(trailingSwipeMiddleBg, NSLayoutAttribute.Width,  NSLayoutRelation.Equal, scrollView, NSLayoutAttribute.Width, 1f, 0f),
                NSLayoutConstraint.Create(trailingSwipeMiddleBg, NSLayoutAttribute.CenterX,  NSLayoutRelation.Equal, scrollView, NSLayoutAttribute.CenterX, 1f, 0f),
                NSLayoutConstraint.Create(trailingSwipeMiddleBg, NSLayoutAttribute.Top,  NSLayoutRelation.Equal, leadingSwipeBg, NSLayoutAttribute.Bottom, 1f, 2f),
                NSLayoutConstraint.Create(trailingSwipeMiddleBg, NSLayoutAttribute.Height,  NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, swipeContainerHeight),
            });

            UIView trailingSwipeMiddle = BuildTrailingSwipeMiddleAction();
            scrollView.AddSubview(trailingSwipeMiddleBg);

            scrollView.AddSubview(trailingSwipeMiddle);

            if (Integration.IsIPad())
            {
                scrollView.AddConstraint(NSLayoutConstraint.Create(trailingSwipeMiddle, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, scrollView.ReadableContentGuide, NSLayoutAttribute.Leading, 1f, 0f));
            }
            else
            {
                scrollView.AddConstraint(NSLayoutConstraint.Create(trailingSwipeMiddle, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, scrollView.ReadableContentGuide, NSLayoutAttribute.Leading, 2f, 0f));
            }

            scrollView.AddConstraints(new[] {
                NSLayoutConstraint.Create(trailingSwipeMiddle, NSLayoutAttribute.Trailing,  NSLayoutRelation.Equal, scrollView.ReadableContentGuide, NSLayoutAttribute.Trailing, 1f, 0f),
                NSLayoutConstraint.Create(trailingSwipeMiddle, NSLayoutAttribute.Top,  NSLayoutRelation.Equal, leadingSwipeBg, NSLayoutAttribute.Bottom, 1f, 2f),
                NSLayoutConstraint.Create(trailingSwipeMiddle, NSLayoutAttribute.Height,  NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, swipeContainerHeight),
            });

            #endregion
            #region Trailing Swipe Middle
            UIView trailingSwipeLastBg = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = Theme.White
            };

            scrollView.AddSubview(trailingSwipeLastBg);
            scrollView.AddConstraints(new[] {
                NSLayoutConstraint.Create(trailingSwipeLastBg, NSLayoutAttribute.Width,  NSLayoutRelation.Equal, scrollView, NSLayoutAttribute.Width, 1f, 0f),
                NSLayoutConstraint.Create(trailingSwipeLastBg, NSLayoutAttribute.CenterX,  NSLayoutRelation.Equal, scrollView, NSLayoutAttribute.CenterX, 1f, 0f),
                NSLayoutConstraint.Create(trailingSwipeLastBg, NSLayoutAttribute.Top,  NSLayoutRelation.Equal, trailingSwipeMiddleBg, NSLayoutAttribute.Bottom, 1f, 2f),
                NSLayoutConstraint.Create(trailingSwipeLastBg, NSLayoutAttribute.Height,  NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, swipeContainerHeight),
            });

            UIView trailingSwipeLast = BuildTrailingSwipeLastAction();
            scrollView.AddSubview(trailingSwipeLast);

            scrollView.AddSubview(trailingSwipeLast);

            if (Integration.IsIPad())
            {
                scrollView.AddConstraint(NSLayoutConstraint.Create(trailingSwipeLast, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, scrollView.ReadableContentGuide, NSLayoutAttribute.Leading, 1f, 0f));
            }
            else
            {
                scrollView.AddConstraint(NSLayoutConstraint.Create(trailingSwipeLast, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, scrollView.ReadableContentGuide, NSLayoutAttribute.Leading, 2f, 0f));
            }

            scrollView.AddConstraints(new[] {
                NSLayoutConstraint.Create(trailingSwipeLast, NSLayoutAttribute.Trailing,  NSLayoutRelation.Equal, scrollView.ReadableContentGuide, NSLayoutAttribute.Trailing, 1f, 0f),
                NSLayoutConstraint.Create(trailingSwipeLast, NSLayoutAttribute.Top,  NSLayoutRelation.Equal, trailingSwipeMiddleBg, NSLayoutAttribute.Bottom, 1f, 2f),
                NSLayoutConstraint.Create(trailingSwipeLast, NSLayoutAttribute.Height,  NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, swipeContainerHeight),
            });


            #endregion

            UIButton defaultsBtn = new UIButton()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = Theme.White,
                UserInteractionEnabled = true
            };

            defaultsBtn.SetTitleColor(Theme.DarkBlue, UIControlState.Normal);
            defaultsBtn.TitleLabel.LineBreakMode = UILineBreakMode.WordWrap;
            defaultsBtn.SetTitle(Localization.GetString("swipe_reset_default"), UIControlState.Normal);

            scrollView.AddSubview(defaultsBtn);

            defaultsBtn.TopAnchor.ConstraintEqualTo(trailingSwipeLast.BottomAnchor, 2f).Active = true;
            defaultsBtn.WidthAnchor.ConstraintEqualTo(scrollView.WidthAnchor).Active = true;
            defaultsBtn.CenterXAnchor.ConstraintEqualTo(scrollView.CenterXAnchor).Active = true;
            defaultsBtn.HeightAnchor.ConstraintEqualTo(70f).Active = true;
            defaultsBtn.BottomAnchor.ConstraintEqualTo(scrollView.BottomAnchor).Active = true;

            defaultsBtn.TouchUpInside += (object sender, EventArgs e) =>
            {
                PlatformConfig.Preferences.ResetSwipeActions();
            };
        }

        UIView BuildTrailingSwipeMiddleAction()
        {
            UIView containerView = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = Theme.White
            };

            UILabel titleLbl = new UILabel()
            {
                Font = Theme.DefaultFont,
                TextColor = Theme.DarkBlue,
                Lines = 1,
                BackgroundColor = Theme.Clear,
                Text = Localization.GetString("swipe_settings_middle_title"),
                TextAlignment = UITextAlignment.Left,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            titleLbl.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);

            containerView.AddSubview(titleLbl);

            titleLbl.LeadingAnchor.ConstraintEqualTo(containerView.LeadingAnchor).Active = true;
            titleLbl.TrailingAnchor.ConstraintEqualTo(containerView.TrailingAnchor).Active = true;
            titleLbl.TopAnchor.ConstraintEqualTo(containerView.TopAnchor, 10f).Active = true;

            UIView btnContainer = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = Theme.Gray
            };

            middleSwipeButton = BuildActionSelectionButton(trailingMiddleBtnTag);

            var action = PlatformConfig.Preferences.EmailTrailingSwipeActions.ElementAt(1);
            if (action != null) middleSwipeButton.SetTitle(action.GetName(), UIControlState.Normal);

            containerView.AddSubview(btnContainer);

            btnContainer.WidthAnchor.ConstraintEqualTo(3 * swipeActionBtnWidth).Active = true;
            btnContainer.TopAnchor.ConstraintEqualTo(titleLbl.BottomAnchor, 10f).Active = true;
            btnContainer.BottomAnchor.ConstraintEqualTo(containerView.BottomAnchor, -10f).Active = true;
            btnContainer.TrailingAnchor.ConstraintEqualTo(containerView.TrailingAnchor).Active = true;

            UIView emailContent = BuildLeadingSwipeEmai();

            containerView.AddSubview(emailContent);

            emailContent.LeadingAnchor.ConstraintEqualTo(containerView.LeadingAnchor).Active = true;
            emailContent.TrailingAnchor.ConstraintEqualTo(btnContainer.LeadingAnchor).Active = true;
            emailContent.TopAnchor.ConstraintEqualTo(titleLbl.BottomAnchor, 10f).Active = true;
            emailContent.BottomAnchor.ConstraintEqualTo(containerView.BottomAnchor, -10f).Active = true;

            containerView.AddSubview(middleSwipeButton);

            middleSwipeButton.WidthAnchor.ConstraintEqualTo(swipeActionBtnWidth).Active = true;
            middleSwipeButton.TopAnchor.ConstraintEqualTo(btnContainer.TopAnchor).Active = true;
            middleSwipeButton.BottomAnchor.ConstraintEqualTo(containerView.BottomAnchor, -10f).Active = true;
            middleSwipeButton.CenterXAnchor.ConstraintEqualTo(btnContainer.CenterXAnchor).Active = true;

            middleSwipeButton.BackgroundColor = Theme.DarkBlue;

            return containerView;
        }

        UIView BuildTrailingSwipeLastAction()
        {
            UIView containerView = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = Theme.White
            };

            UILabel titleLbl = new UILabel()
            {
                Font = Theme.DefaultFont,
                TextColor = Theme.DarkBlue,
                Lines = 1,
                BackgroundColor = Theme.Clear,
                Text = Localization.GetString("swipe_settings_last_title"),
                TextAlignment = UITextAlignment.Left,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            titleLbl.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);

            containerView.AddSubview(titleLbl);

            titleLbl.LeadingAnchor.ConstraintEqualTo(containerView.LeadingAnchor).Active = true;
            titleLbl.TrailingAnchor.ConstraintEqualTo(containerView.TrailingAnchor).Active = true;
            titleLbl.TopAnchor.ConstraintEqualTo(containerView.TopAnchor, 10f).Active = true;

            UIView btnContainer = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = Theme.Gray
            };

            lastSwipeButton = BuildActionSelectionButton(trailingLastBtnTag);

            var action = PlatformConfig.Preferences.EmailTrailingSwipeActions.Last();
            if (action != null) lastSwipeButton.SetTitle(action.GetName(), UIControlState.Normal);

            containerView.AddSubview(btnContainer);

            btnContainer.WidthAnchor.ConstraintEqualTo(3 * swipeActionBtnWidth).Active = true;
            btnContainer.TopAnchor.ConstraintEqualTo(titleLbl.BottomAnchor, 10f).Active = true;
            btnContainer.BottomAnchor.ConstraintEqualTo(containerView.BottomAnchor, -10f).Active = true;
            btnContainer.TrailingAnchor.ConstraintEqualTo(containerView.TrailingAnchor).Active = true;

            UIView seperator = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = Theme.White
            };

            containerView.AddSubview(seperator);

            seperator.WidthAnchor.ConstraintEqualTo(2f).Active = true;
            seperator.TopAnchor.ConstraintEqualTo(btnContainer.TopAnchor).Active = true;
            seperator.BottomAnchor.ConstraintEqualTo(btnContainer.BottomAnchor).Active = true;
            seperator.LeadingAnchor.ConstraintEqualTo(btnContainer.LeadingAnchor, swipeActionBtnWidth).Active = true;

            UIView emailContent = BuildLeadingSwipeEmai();

            containerView.AddSubview(emailContent);

            emailContent.LeadingAnchor.ConstraintEqualTo(containerView.LeadingAnchor).Active = true;
            emailContent.TrailingAnchor.ConstraintEqualTo(btnContainer.LeadingAnchor).Active = true;
            emailContent.TopAnchor.ConstraintEqualTo(titleLbl.BottomAnchor, 10f).Active = true;
            emailContent.BottomAnchor.ConstraintEqualTo(containerView.BottomAnchor, -10f).Active = true;

            containerView.AddSubview(lastSwipeButton);

            lastSwipeButton.WidthAnchor.ConstraintEqualTo(swipeActionBtnWidth).Active = true;
            lastSwipeButton.TopAnchor.ConstraintEqualTo(btnContainer.TopAnchor).Active = true;
            lastSwipeButton.BottomAnchor.ConstraintEqualTo(containerView.BottomAnchor, -10f).Active = true;
            lastSwipeButton.TrailingAnchor.ConstraintEqualTo(btnContainer.TrailingAnchor).Active = true;

            lastSwipeButton.BackgroundColor = Theme.Brown;

            return containerView;
        }

        UIButton BuildActionSelectionButton(nint tag)
        {
            var button = new UIButton()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                LineBreakMode = UILineBreakMode.WordWrap,
                HorizontalAlignment = UIControlContentHorizontalAlignment.Center,
            };

            button.SetTitleColor(Theme.White, UIControlState.Normal);
            button.HorizontalAlignment = UIControlContentHorizontalAlignment.Center;
            button.ContentEdgeInsets = new UIEdgeInsets(10f, 10f, 10f, 10f);
            button.TitleLabel.Lines = 5;
            button.TitleLabel.ContentScaleFactor = 0.7f;

            button.TitleLabel.AdjustsFontSizeToFitWidth = true;
            button.TitleLabel.Font = Theme.DefaultFont;
            button.TitleLabel.TextAlignment = UITextAlignment.Center;
            button.Tag = Convert.ToInt16(tag);
            button.TouchUpInside += SelectAction_Clicked;

            return button;
        }

        UIView BuildLeadingSwipe()
        {
            UIView containerView = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = Theme.White
            };

            UILabel titleLbl = new UILabel()
            {
                Font = Theme.DefaultFont,
                TextColor = Theme.DarkBlue,
                Lines = 1,
                BackgroundColor = Theme.Clear,
                Text = Localization.GetString("swipe_settings_leading_title"),
                TextAlignment = UITextAlignment.Left,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            titleLbl.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);

            containerView.AddSubview(titleLbl);

            titleLbl.LeadingAnchor.ConstraintEqualTo(containerView.LeadingAnchor).Active = true;
            titleLbl.TrailingAnchor.ConstraintEqualTo(containerView.TrailingAnchor).Active = true;
            titleLbl.TopAnchor.ConstraintEqualTo(containerView.TopAnchor, 10f).Active = true;

            UIView emailContent = BuildLeadingSwipeEmai();

            leadingSwipeButton = BuildActionSelectionButton(leadingBtnTag);

            var leadingAction = PlatformConfig.Preferences.EmailLeadingSwipeActions.First();

            leadingSwipeButton.SetTitle(leadingAction.GetName(), UIControlState.Normal);
            leadingSwipeButton.BackgroundColor = Theme.LightBrown;

            containerView.AddSubview(leadingSwipeButton);

            leadingSwipeButton.WidthAnchor.ConstraintEqualTo(swipeActionBtnWidth).Active = true;
            leadingSwipeButton.LeadingAnchor.ConstraintEqualTo(containerView.LeadingAnchor).Active = true;
            leadingSwipeButton.TopAnchor.ConstraintEqualTo(titleLbl.BottomAnchor, 10f).Active = true;
            leadingSwipeButton.BottomAnchor.ConstraintEqualTo(containerView.BottomAnchor, -10f).Active = true;

            containerView.AddSubview(emailContent);

            emailContent.TrailingAnchor.ConstraintEqualTo(containerView.TrailingAnchor).Active = true;
            emailContent.LeadingAnchor.ConstraintEqualTo(leadingSwipeButton.TrailingAnchor).Active = true;
            emailContent.TopAnchor.ConstraintEqualTo(titleLbl.BottomAnchor, 10f).Active = true;
            emailContent.BottomAnchor.ConstraintEqualTo(containerView.BottomAnchor, -10f).Active = true;

            /* commnet out if UI/UX wants color mask
            UIView leftContainerColorMask = new UIView()
            {
                BackgroundColor = Theme.Gray,
                Alpha = 0.7f,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            containerView.AddSubview(leftContainerColorMask);

            containerView.AddConstraints(new[] {
                NSLayoutConstraint.Create(leftContainerColorMask, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, leadingSwipeButton, NSLayoutAttribute.Trailing, 1f, 0f),
                NSLayoutConstraint.Create(leftContainerColorMask, NSLayoutAttribute.Top, NSLayoutRelation.Equal, titleLbl, NSLayoutAttribute.Bottom, 1f, 10f),
                NSLayoutConstraint.Create(leftContainerColorMask, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, containerView, NSLayoutAttribute.Bottom, 1f, -10f)
            });

            if (Integration.IsIPad())
            {
                containerView.AddConstraint(NSLayoutConstraint.Create(leftContainerColorMask, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, containerView.ReadableContentGuide, NSLayoutAttribute.Trailing, 1f, 0f));
            }
            else
            {
                containerView.AddConstraint(NSLayoutConstraint.Create(leftContainerColorMask, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, containerView.ReadableContentGuide, NSLayoutAttribute.Trailing, 1f, 40f));

            }
            */
            return containerView;
        }

        UIView BuildLeadingSwipeEmai()
        {

            UIView container = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            var leadingMarginGuide = new UILayoutGuide();
            container.AddLayoutGuide(leadingMarginGuide);

            leadingMarginGuide.LeadingAnchor.ConstraintEqualTo(container.ReadableContentGuide.LeadingAnchor).Active = true;
            var leadingMarginWidthAnchor = leadingMarginGuide.WidthAnchor.ConstraintEqualTo(0f);
            leadingMarginWidthAnchor.SetIdentifier("leadingMarginWidth");
            leadingMarginWidthAnchor.Active = true;

            UILabel topLabel = new UILabel
            {
                Text = "Anne Mortensen",
                Font = Theme.DefaultBoldFont,
                TextColor = Theme.DarkGray,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            container.AddSubview(topLabel);
            container.AddConstraints(new[]
            {
                topLabel.TopAnchor.ConstraintEqualTo(container.TopAnchor, 8f),
                topLabel.LeadingAnchor.ConstraintEqualTo(leadingMarginGuide.TrailingAnchor, 15f + 8f),
                topLabel.HeightAnchor.ConstraintGreaterThanOrEqualTo(Theme.MinimumLabelSize),
            });

            UILabel dateLabel = new UILabel
            {
                Text = "9.35, today",
                Font = Theme.DefaultLightFont.WithRelativeSize(-2f),
                TextColor = Theme.DarkGray,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            dateLabel.SetContentHuggingPriority(1000f, UILayoutConstraintAxis.Horizontal);
            dateLabel.SetContentCompressionResistancePriority(1000f, UILayoutConstraintAxis.Horizontal);

            container.AddSubview(dateLabel);
            container.AddConstraints(new[]
                {
                    dateLabel.LeadingAnchor.ConstraintEqualTo(topLabel.TrailingAnchor, 8f),
                    dateLabel.TrailingAnchor.ConstraintEqualTo(container.ReadableContentGuide.TrailingAnchor),
                    dateLabel.CenterYAnchor.ConstraintEqualTo(topLabel.CenterYAnchor)
                });

            UILabel middleLabel = new UILabel
            {
                Text = "Hvad skal du lave til nytår?",
                Font = Theme.DefaultFont,
                TextColor = Theme.DarkGray,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            container.AddSubview(middleLabel);
            container.AddConstraints(new[]
            {
                middleLabel.LeadingAnchor.ConstraintEqualTo(topLabel.LeadingAnchor),
                middleLabel.TopAnchor.ConstraintEqualTo(topLabel.BottomAnchor, 4f),
                middleLabel.TrailingAnchor.ConstraintEqualTo(container.ReadableContentGuide.TrailingAnchor),
            });

            UITextView bottomLabel = new UITextView
            {
                Text = "Hej Anne nytår står for døren så jeg ville høre hvad dine planer er. Vi tænker at tage på musling og spise en masse...",
                Font = Theme.DefaultFont.WithRelativeSize(-2f),
                TextColor = Theme.DarkGray,
                Selectable = false,
                Editable = false,
                ScrollEnabled = false,
                ClipsToBounds = false,
                TextContainerInset = UIEdgeInsets.Zero,
                UserInteractionEnabled = false,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            bottomLabel.TextContainer.MaximumNumberOfLines = 3;
            bottomLabel.TextContainer.LineFragmentPadding = 0f;

            container.AddSubview(bottomLabel);
            container.AddConstraints(new[]
            {
                bottomLabel.LeadingAnchor.ConstraintEqualTo(topLabel.LeadingAnchor),
                bottomLabel.TopAnchor.ConstraintEqualTo(middleLabel.BottomAnchor, 4f),
                bottomLabel.TrailingAnchor.ConstraintEqualTo(container.ReadableContentGuide.TrailingAnchor),
                bottomLabel.BottomAnchor.ConstraintEqualTo(container.BottomAnchor),
            });

            UIImageView directionIndicatorImageView = new UIImageView
            {
                ContentMode = UIViewContentMode.ScaleToFill,
                TranslatesAutoresizingMaskIntoConstraints = false,
                Image = UIImage.FromBundle("Incoming").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
                TintColor = Theme.DarkGray
            };

            container.AddSubview(directionIndicatorImageView);
            container.AddConstraints(new[]
                {
                    directionIndicatorImageView.LeadingAnchor.ConstraintEqualTo(leadingMarginGuide.TrailingAnchor),
                    directionIndicatorImageView.CenterYAnchor.ConstraintEqualTo(topLabel.CenterYAnchor),
                    directionIndicatorImageView.WidthAnchor.ConstraintEqualTo(15f),
                    directionIndicatorImageView.HeightAnchor.ConstraintEqualTo(15f),
                });

            UIImageView unreadIndicatorImageView = new UIImageView
            {
                ContentMode = UIViewContentMode.ScaleToFill,
                Image = UIImage.FromBundle("Full-Dot").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
                TranslatesAutoresizingMaskIntoConstraints = false,
                TintColor = Theme.DarkGray
            };

            container.AddSubview(unreadIndicatorImageView);
            container.AddConstraints(new[]
                {
                    unreadIndicatorImageView.LeadingAnchor.ConstraintEqualTo(leadingMarginGuide.TrailingAnchor),
                    unreadIndicatorImageView.TopAnchor.ConstraintEqualTo(directionIndicatorImageView.BottomAnchor, 4f),
                    unreadIndicatorImageView.WidthAnchor.ConstraintEqualTo(15f),
                    unreadIndicatorImageView.HeightAnchor.ConstraintEqualTo(15f),
                });

            container.Layer.BorderWidth = 1f;
            container.Layer.BorderColor = Theme.Gray.CGColor;

            return container;
        }

        void SetAction(nint tag, EmailSwipeAction.SwipeAction action)
        {
            switch (tag)
            {
                case 0:
                    PlatformConfig.Preferences.SetEmailLeadingSwipeAction(action);
                    break;
                case 2:
                    PlatformConfig.Preferences.SetEmailTrailingLastAction(action);
                    break;
                case 1:
                    PlatformConfig.Preferences.SetEmailTrailingMiddleAction(action);
                    break;
            }
        }

        void SelectAction_Clicked(object sender, EventArgs e)
        {
            var eas = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);

            UIButton senderBtn = (UIButton)sender;

            eas.AddAction(UIAlertAction.Create(
                Localization.GetString("mark_as_read_unread"),
                UIAlertActionStyle.Default,
                a =>
                {
                    SetAction(senderBtn.Tag, EmailSwipeAction.SwipeAction.MarkAsRead);
                }));

            eas.AddAction(UIAlertAction.Create(
                Localization.GetString("copy_to_worktray"),
                UIAlertActionStyle.Default,
                a =>
                {
                    SetAction(senderBtn.Tag, EmailSwipeAction.SwipeAction.CopyToWorkTray);
                }));
            eas.AddAction(UIAlertAction.Create(
                Localization.GetString("copy_to_folder"),
                UIAlertActionStyle.Default,
                a =>
                {
                    SetAction(senderBtn.Tag, EmailSwipeAction.SwipeAction.CopyToFolder);
                }));


            eas.AddAction(UIAlertAction.Create(
                Localization.GetString("move_to_folder"),
                UIAlertActionStyle.Default,
                a =>
                {
                    SetAction(senderBtn.Tag, EmailSwipeAction.SwipeAction.MoveToFolder);
                }));

            eas.AddAction(UIAlertAction.Create(
                Localization.GetString("set_priority"),
                UIAlertActionStyle.Default,
                a =>
                {
                    SetAction(senderBtn.Tag, EmailSwipeAction.SwipeAction.SetPriority);
                }));

            eas.AddAction(UIAlertAction.Create(
                Localization.GetString("delete_from_folder"), 
                UIAlertActionStyle.Default,
                a =>
                {
                    SetAction(senderBtn.Tag, EmailSwipeAction.SwipeAction.RemoveFromFolder);
                }));


            eas.AddAction(UIAlertAction.Create(
                Localization.GetString("delete"), 
                UIAlertActionStyle.Default,
                a =>
                {
                    SetAction(senderBtn.Tag, EmailSwipeAction.SwipeAction.Delete);
                }));

            eas.AddAction(UIAlertAction.Create(
                Localization.GetString("cancel"),
                UIAlertActionStyle.Cancel,
                a =>
                {
                    DismissViewController(true, null);
                }));

            UIButton senRef = (UIButton)sender;

            if (eas.PopoverPresentationController != null)
            {
                eas.PopoverPresentationController.SourceView = senRef;
                eas.PopoverPresentationController.SourceRect = senRef.Bounds;
            }
            PresentViewController(eas, true, null);
        }
    }
}
