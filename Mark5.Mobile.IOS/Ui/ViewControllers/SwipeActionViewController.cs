using System;
using Foundation;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using Mark5.Mobile.IOS.Utilities.Extensions;
using UIKit;
using System.Linq;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class SwipeActionViewController : AbstractViewController
    {
        NSObject observer;

        const float swipeContainerHeight = 160f;
        const float swipeActionBtnWidth = 80f;

        UIButton leadingSwipeButton;
        UIButton middleSwipeButton;
        UIButton lastSwipeButton;

        static nint leadingBtnTag = 0;
        static nint trailingMiddleBtnTag = 1;
        static nint trailingLastBtnTag = 2;

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
                if (action != null) {
                    BeginInvokeOnMainThread(() =>
                    {
                        leadingSwipeButton.SetTitle(action.GetName(), UIControlState.Normal);
                    });
                }
            }

            if (middleSwipeButton != null)
            {
                var action = PlatformConfig.Preferences.EmailTrailingSwipeActions.ElementAt(1);
                if (action != null) {
                    BeginInvokeOnMainThread(() =>
                    {
                        middleSwipeButton.SetTitle(action.GetName(), UIControlState.Normal);
                    });
                } 
            }

            if (lastSwipeButton != null)
            {
                var action = PlatformConfig.Preferences.EmailTrailingSwipeActions.Last();
                if (action != null) BeginInvokeOnMainThread(() =>
                {
                    lastSwipeButton.SetTitle(action.GetName(), UIControlState.Normal);
                });
            }
        }

        public override void LoadView()
        {
            base.LoadView();

            Title = Localization.GetString("email_swipe_actions");

            UIScrollView scrollView = new UIScrollView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = UIColor.GroupTableViewBackgroundColor,
                ShowsVerticalScrollIndicator = false,
                ShowsHorizontalScrollIndicator = false,
            };

            View.AddSubview(scrollView);

            View.AddConstraints(new[]
            {
                scrollView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                scrollView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                scrollView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                scrollView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });

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

            scrollView.AddConstraints(new[]
            {
                topLabel.TrailingAnchor.ConstraintEqualTo(scrollView.ReadableContentGuide.TrailingAnchor),
                topLabel.TopAnchor.ConstraintEqualTo(scrollView.TopAnchor,10f)
            });

            var leadingSwipeElement = BuildSwipeContainer(scrollView, BuildLeadingSwipe(), topLabel, 10f);

            var trailingSwipeMiddleElement = BuildSwipeContainer(scrollView, BuildTrailingSwipeMiddleAction(), leadingSwipeElement, 1f);

            var trailingSwipeLastElement = BuildSwipeContainer(scrollView, BuildTrailingSwipeLastAction(), trailingSwipeMiddleElement, 1f);

            BuildDefaultsButton(scrollView, trailingSwipeLastElement);
        }

        #region UI elements

        UIView BuildSwipeContainer(UIScrollView scrollView, UIView content, UIView topView, float topMargin)
        {
            UIView background = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = Theme.White
            };

            scrollView.AddSubview(background);

            scrollView.AddConstraints(new[]
            {
                background.WidthAnchor.ConstraintEqualTo(scrollView.WidthAnchor),
                background.CenterXAnchor.ConstraintEqualTo(scrollView.CenterXAnchor),
                background.TopAnchor.ConstraintEqualTo(topView.BottomAnchor,topMargin),
                background.HeightAnchor.ConstraintEqualTo(swipeContainerHeight)
            });

            scrollView.AddSubview(content);

            if (Integration.IsIPad())
            {
                scrollView.AddConstraint(NSLayoutConstraint.Create(content, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, scrollView.ReadableContentGuide, NSLayoutAttribute.Leading, 1f, 0f));
            }
            else
            {
                scrollView.AddConstraint(NSLayoutConstraint.Create(content, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, scrollView.ReadableContentGuide, NSLayoutAttribute.Leading, 2f, 0f));
            }

            scrollView.AddConstraints(new[]
            {
                content.TrailingAnchor.ConstraintEqualTo(scrollView.ReadableContentGuide.TrailingAnchor),
                content.TopAnchor.ConstraintEqualTo(topView.BottomAnchor, topMargin),
                content.HeightAnchor.ConstraintEqualTo(swipeContainerHeight)
            });

            return background;
        }

        UIButton BuildActionSelectionButton()
        {
            var button = new UIButton()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                ContentEdgeInsets = new UIEdgeInsets(5f, 5f, 5f, 5f),
                HorizontalAlignment = UIControlContentHorizontalAlignment.Center,
            };

            button.SetTitleColor(Theme.White, UIControlState.Normal);
            button.TitleLabel.Lines = 0;
            button.TitleLabel.AdjustsFontSizeToFitWidth = false;
            button.TitleLabel.Font = Theme.DefaultActionsFont;
            button.TitleLabel.TextAlignment = UITextAlignment.Center;
            button.TitleLabel.LineBreakMode = UILineBreakMode.WordWrap;

            return button;
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
                TextColor = Theme.LightGray,
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
                TextColor = Theme.LightGray,
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
                TextColor = Theme.LightGray,
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
                TextColor = Theme.LightGray,
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
                TintColor = Theme.LightGray
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
                TintColor = Theme.LightGray
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

        void BuildMaskButton(UIView containerView, nint tag)
        {

            UIButton maskBtn = new UIButton()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                UserInteractionEnabled = true,
                Alpha = 1
            };

            maskBtn.Tag = Convert.ToInt16(tag);

            maskBtn.SetTitleColor(Theme.Black, UIControlState.Normal);

            maskBtn.TouchUpInside += SelectAction_Clicked;

            containerView.AddSubview(maskBtn);

            containerView.AddConstraints(new[]
            {
                maskBtn.TopAnchor.ConstraintEqualTo(containerView.TopAnchor),
                maskBtn.WidthAnchor.ConstraintEqualTo(containerView.WidthAnchor),
                maskBtn.HeightAnchor.ConstraintEqualTo(containerView.HeightAnchor),
                maskBtn.LeadingAnchor.ConstraintEqualTo(containerView.LeadingAnchor),
                maskBtn.TrailingAnchor.ConstraintEqualTo(containerView.TrailingAnchor)
            });
        }

        void BuildDefaultsButton(UIScrollView scrollView, UIView topView)
        {
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
            scrollView.AddConstraints(new[]
            {
                defaultsBtn.TopAnchor.ConstraintEqualTo(topView.BottomAnchor, 1f),
                defaultsBtn.WidthAnchor.ConstraintEqualTo(scrollView.WidthAnchor),
                defaultsBtn.CenterXAnchor.ConstraintEqualTo(scrollView.CenterXAnchor),
                defaultsBtn.HeightAnchor.ConstraintEqualTo(70f),
                defaultsBtn.BottomAnchor.ConstraintEqualTo(scrollView.BottomAnchor)
            });

            defaultsBtn.TouchUpInside += (object sender, EventArgs e) =>
            {
                CommonConfig.UsageAnalytics.LogEvent(new SwipeActionChangedEvent());
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

            containerView.AddConstraints(new[]
            {
                titleLbl.LeadingAnchor.ConstraintEqualTo(containerView.LeadingAnchor),
                titleLbl.TrailingAnchor.ConstraintEqualTo(containerView.TrailingAnchor),
                titleLbl.TopAnchor.ConstraintEqualTo(containerView.TopAnchor, 10f)
            });

            UIView btnContainer = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = Theme.Gray
            };

            middleSwipeButton = BuildActionSelectionButton();

            var action = PlatformConfig.Preferences.EmailTrailingSwipeActions.ElementAt(1);
            if (action != null) middleSwipeButton.SetTitle(action.GetName(), UIControlState.Normal);

            containerView.AddSubview(btnContainer);

            containerView.AddConstraints(new[]
            {
                btnContainer.WidthAnchor.ConstraintEqualTo(3 * swipeActionBtnWidth),
                btnContainer.TopAnchor.ConstraintEqualTo(titleLbl.BottomAnchor, 10f),
                btnContainer.BottomAnchor.ConstraintEqualTo(containerView.BottomAnchor, -10f),
                btnContainer.TrailingAnchor.ConstraintEqualTo(containerView.TrailingAnchor)
            });

            UIView emailContent = BuildLeadingSwipeEmai();

            containerView.AddSubview(emailContent);

            containerView.AddConstraints(new[]
            {
                emailContent.LeadingAnchor.ConstraintEqualTo(containerView.LeadingAnchor),
                emailContent.TrailingAnchor.ConstraintEqualTo(btnContainer.LeadingAnchor),
                emailContent.TopAnchor.ConstraintEqualTo(titleLbl.BottomAnchor, 10f),
                emailContent.BottomAnchor.ConstraintEqualTo(containerView.BottomAnchor, -10f)
            });

            containerView.AddSubview(middleSwipeButton);

            containerView.AddConstraints(new[]
            {
                middleSwipeButton.WidthAnchor.ConstraintEqualTo(swipeActionBtnWidth),
                middleSwipeButton.TopAnchor.ConstraintEqualTo(btnContainer.TopAnchor),
                middleSwipeButton.BottomAnchor.ConstraintEqualTo(containerView.BottomAnchor, -10f),
                middleSwipeButton.CenterXAnchor.ConstraintEqualTo(btnContainer.CenterXAnchor)
            });

            middleSwipeButton.BackgroundColor = Theme.DarkBlue;

            BuildMaskButton(containerView, trailingMiddleBtnTag);

            return containerView;
        }

        UIView BuildTrailingSwipeLastAction()
        {
            UIButton containerView = new UIButton()
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

            containerView.AddConstraints(new[]
            {
                titleLbl.LeadingAnchor.ConstraintEqualTo(containerView.LeadingAnchor),
                titleLbl.TrailingAnchor.ConstraintEqualTo(containerView.TrailingAnchor),
                titleLbl.TopAnchor.ConstraintEqualTo(containerView.TopAnchor, 10f)
            });

            UIView btnContainer = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = Theme.Gray
            };

            lastSwipeButton = BuildActionSelectionButton();

            var action = PlatformConfig.Preferences.EmailTrailingSwipeActions.Last();
            if (action != null) lastSwipeButton.SetTitle(action.GetName(), UIControlState.Normal);

            containerView.AddSubview(btnContainer);

            containerView.AddConstraints(new[]
            {
                btnContainer.WidthAnchor.ConstraintEqualTo(3 * swipeActionBtnWidth),
                btnContainer.TopAnchor.ConstraintEqualTo(titleLbl.BottomAnchor, 10f),
                btnContainer.BottomAnchor.ConstraintEqualTo(containerView.BottomAnchor, -10f),
                btnContainer.TrailingAnchor.ConstraintEqualTo(containerView.TrailingAnchor)
            });

            UIView seperator = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = Theme.White
            };

            containerView.AddSubview(seperator);
            containerView.AddConstraints(new[]
            {
                seperator.WidthAnchor.ConstraintEqualTo(2f),
                seperator.TopAnchor.ConstraintEqualTo(btnContainer.TopAnchor),
                seperator.BottomAnchor.ConstraintEqualTo(btnContainer.BottomAnchor),
                seperator.LeadingAnchor.ConstraintEqualTo(btnContainer.LeadingAnchor, swipeActionBtnWidth)
            });

            UIView emailContent = BuildLeadingSwipeEmai();

            containerView.AddSubview(emailContent);

            containerView.AddConstraints(new[]
            {
                emailContent.LeadingAnchor.ConstraintEqualTo(containerView.LeadingAnchor),
                emailContent.TrailingAnchor.ConstraintEqualTo(btnContainer.LeadingAnchor),
                emailContent.TopAnchor.ConstraintEqualTo(titleLbl.BottomAnchor, 10f),
                emailContent.BottomAnchor.ConstraintEqualTo(containerView.BottomAnchor, -10f)
            });

            containerView.AddSubview(lastSwipeButton);

            containerView.AddConstraints(new[]
            {
                lastSwipeButton.WidthAnchor.ConstraintEqualTo(swipeActionBtnWidth),
                lastSwipeButton.TopAnchor.ConstraintEqualTo(btnContainer.TopAnchor),
                lastSwipeButton.BottomAnchor.ConstraintEqualTo(containerView.BottomAnchor, -10f),
                lastSwipeButton.TrailingAnchor.ConstraintEqualTo(btnContainer.TrailingAnchor)
            });

            lastSwipeButton.BackgroundColor = Theme.Brown;

            BuildMaskButton(containerView, trailingLastBtnTag);

            return containerView;
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

            containerView.AddConstraints(new[] {
                titleLbl.LeadingAnchor.ConstraintEqualTo(containerView.LeadingAnchor),
                titleLbl.TrailingAnchor.ConstraintEqualTo(containerView.TrailingAnchor),
                titleLbl.TopAnchor.ConstraintEqualTo(containerView.TopAnchor, 10f)
            });

            UIView emailContent = BuildLeadingSwipeEmai();

            leadingSwipeButton = BuildActionSelectionButton();

            var leadingAction = PlatformConfig.Preferences.EmailLeadingSwipeActions.First();

            leadingSwipeButton.SetTitle(leadingAction.GetName(), UIControlState.Normal);
            leadingSwipeButton.BackgroundColor = Theme.LightBrown;

            containerView.AddSubview(leadingSwipeButton);

            containerView.AddConstraints(new[] {
                leadingSwipeButton.WidthAnchor.ConstraintEqualTo(swipeActionBtnWidth),
                leadingSwipeButton.LeadingAnchor.ConstraintEqualTo(containerView.LeadingAnchor),
                leadingSwipeButton.TopAnchor.ConstraintEqualTo(titleLbl.BottomAnchor, 10f),
                leadingSwipeButton.BottomAnchor.ConstraintEqualTo(containerView.BottomAnchor, -10f)
            });

            containerView.AddSubview(emailContent);

            containerView.AddConstraints(new[] {
                emailContent.TrailingAnchor.ConstraintEqualTo(containerView.TrailingAnchor),
                emailContent.LeadingAnchor.ConstraintEqualTo(leadingSwipeButton.TrailingAnchor),
                emailContent.TopAnchor.ConstraintEqualTo(titleLbl.BottomAnchor, 10f),
                emailContent.BottomAnchor.ConstraintEqualTo(containerView.BottomAnchor, -10f)
            });

            BuildMaskButton(containerView, leadingBtnTag);

            return containerView;
        }

        #endregion

        #region Actions
        void SetAction(nint tag, EmailSwipeAction.SwipeAction action)
        {

            CommonConfig.UsageAnalytics.LogEvent(new SwipeActionChangedEvent());

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

        #endregion
    }
}
