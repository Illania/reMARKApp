using System;
using System.Collections.Generic;
using CoreGraphics;
using Mark5.Mobile.Common;
using Mark5.Mobile.IOS.Model;
using Mark5.Mobile.IOS.Model.HubMessages;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
{
    public class ModuleNavigationController : UIViewController
    {
        const string mailBtnConstraintIdentifier = "mailBtnConstraintIdentifier";
        const string contactsBtnConstraintIdentifier = "contactsBtnConstraintIdentifier";
        const string searchBtnConstraintIdentifier = "searchBtnHorizontalConstraint";
        const string settingsBtnConstraintIdentifier = "settingsBtnConstraintIdentifier";

        nint seperatorTag = 1;
        nint mailBtnTag = 2;
        nint shortCodesBtnTag = 3;
        nint settingsBtnTag = 4;
        nint contactsBtnTag = 5;
        nint searcBtnTag = 6;
        nint titleTag = 7;

        UIButton closeButton;
        UIView searchButtonContainer;
        UIView seperatorView;
        UILabel titleLabel;

        ReMarkNavigationButton searchBtn;
        ReMarkNavigationButton contactsBtn;
        ReMarkNavigationButton settingsBtn;
        ReMarkNavigationButton mailBtn;
        ReMarkNavigationButton shortCodesBtn;

        NavigationModule.NavigationModuleType currentModule;

        readonly List<string> constraintIdentifiers = new List<string> { mailBtnConstraintIdentifier, contactsBtnConstraintIdentifier, searchBtnConstraintIdentifier, settingsBtnConstraintIdentifier };

        public ModuleNavigationController(NavigationModule.NavigationModuleType currentModule)
        {
            this.currentModule = currentModule;

            closeButton = new UIButton
            {
                TintColor = Theme.White,
                BackgroundColor = Theme.DarkBlue,
                TranslatesAutoresizingMaskIntoConstraints = false,
                ClipsToBounds = true,
                ContentEdgeInsets = new UIEdgeInsets(10f, 10f, 10f, 10f)
            };

            closeButton.Layer.ZPosition.Equals(999f);
            closeButton.SetImage(UIImage.FromBundle("Failed").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
            closeButton.Layer.BorderColor = Theme.DarkBlue.CGColor;
            closeButton.Layer.BorderWidth = .7f;
            closeButton.Layer.CornerRadius = 27.5f;

            closeButton.TouchUpInside += (object sender, EventArgs e) => { DismissViewController(true, null); };

            mailBtn = new ReMarkNavigationButton(new NavigationModule(NavigationModule.NavigationModuleType.Mail), BtnClicked, mailBtnTag, currentModule == NavigationModule.NavigationModuleType.Mail);
            shortCodesBtn = new ReMarkNavigationButton(new NavigationModule(NavigationModule.NavigationModuleType.Shortcodes), BtnClicked, shortCodesBtnTag, currentModule == NavigationModule.NavigationModuleType.Shortcodes);
            contactsBtn = new ReMarkNavigationButton(new NavigationModule(NavigationModule.NavigationModuleType.Contacts), BtnClicked, contactsBtnTag, currentModule == NavigationModule.NavigationModuleType.Contacts);
            settingsBtn = new ReMarkNavigationButton(new NavigationModule(NavigationModule.NavigationModuleType.Settings), BtnClicked, settingsBtnTag, currentModule == NavigationModule.NavigationModuleType.Settings);
            searchBtn = new ReMarkNavigationButton(new NavigationModule(NavigationModule.NavigationModuleType.Search), BtnClicked, settingsBtnTag, currentModule == NavigationModule.NavigationModuleType.Search);

            seperatorView = new UIView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = Theme.DarkBlue,
                Tag = seperatorTag
            };

            seperatorView.Layer.CornerRadius = 1.5f;

            titleLabel = new UILabel
            {
                Font = Theme.DefaultFont,
                TextColor = Theme.DarkerBlue,
                TranslatesAutoresizingMaskIntoConstraints = false,
                Text = "Choose",
                TextAlignment = UITextAlignment.Center,
                Tag = titleTag
            };

            searchButtonContainer = new TouchTransparentView
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            float verticalSpacingFirstBtns = 60f;
            float verticalSpacing2ndRow = 115f;

            View.AddSubviews(new UIView[] {
                searchButtonContainer,
                seperatorView,
                shortCodesBtn,
                mailBtn,
                contactsBtn,
                searchBtn,
                settingsBtn
            });

            View.AddConstraints(new[]
            {
                View.WidthAnchor.ConstraintLessThanOrEqualTo(300),
                searchButtonContainer.HeightAnchor.ConstraintEqualTo(65f),
                searchButtonContainer.WidthAnchor.ConstraintEqualTo(65f),
                searchButtonContainer.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
                searchButtonContainer.BottomAnchor.ConstraintEqualTo(Integration.IsRunningAtLeast(11) ? View.SafeAreaLayoutGuide.BottomAnchor : BottomLayoutGuide.GetTopAnchor(), 2),
            });

            searchButtonContainer.AddSubview(closeButton);
            searchButtonContainer.AddConstraints(new[]
            {
                closeButton.HeightAnchor.ConstraintEqualTo(55f),
                closeButton.WidthAnchor.ConstraintEqualTo(55f),
                closeButton.CenterXAnchor.ConstraintEqualTo(searchButtonContainer.CenterXAnchor),
                closeButton.BottomAnchor.ConstraintEqualTo(searchButtonContainer.BottomAnchor, -10f),
            });

            View.AddConstraints(new[]
            {
                seperatorView.HeightAnchor.ConstraintEqualTo(3f),
                seperatorView.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
                seperatorView.CenterYAnchor.ConstraintEqualTo(View.CenterYAnchor, 60),
                seperatorView.LeftAnchor.ConstraintEqualTo(View.LeftAnchor, 20f),
                seperatorView.RightAnchor.ConstraintEqualTo(View.RightAnchor, -20f)
            });

            if (!UIDevice.CurrentDevice.Orientation.IsLandscape())
            {
                View.AddSubview(titleLabel);
                View.AddConstraints(new[]
                {
                    titleLabel.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
                    titleLabel.TopAnchor.ConstraintEqualTo(View.TopAnchor, 40f)
                });
            }

            View.AddConstraints(new[]
            {
                shortCodesBtn.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
                shortCodesBtn.BottomAnchor.ConstraintEqualTo(seperatorView.CenterYAnchor, -verticalSpacing2ndRow),
                mailBtn.CenterYAnchor.ConstraintEqualTo(shortCodesBtn.CenterYAnchor),
                contactsBtn.CenterYAnchor.ConstraintEqualTo(shortCodesBtn.CenterYAnchor),
                searchBtn.CenterYAnchor.ConstraintEqualTo(seperatorView.CenterYAnchor, -verticalSpacingFirstBtns),
                settingsBtn.CenterYAnchor.ConstraintEqualTo(seperatorView.CenterYAnchor, verticalSpacingFirstBtns),
            });

            SetButtonGridHorizontalConstraints();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            View.BackgroundColor = Theme.White;
        }

        public override void ViewWillTransitionToSize(CGSize toSize, IUIViewControllerTransitionCoordinator coordinator)
        {
            base.ViewWillTransitionToSize(toSize, coordinator);

            UpdateLayoutOnRotation();
        }

        void UpdateLayoutOnRotation()
        {
            foreach (var constraint in View.Constraints)
                if (constraintIdentifiers.Contains(constraint.GetIdentifier()))
                    View.RemoveConstraint(constraint);

            foreach (var view in View.Subviews)
                if (view.Tag == titleTag)
                    view.RemoveFromSuperview();

            if (!UIDevice.CurrentDevice.Orientation.IsLandscape())
            {
                View.AddSubview(titleLabel);
                View.AddConstraints(new[]
                {
                    titleLabel.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
                    titleLabel.TopAnchor.ConstraintEqualTo(View.TopAnchor, 40f)
                });
            }

            SetButtonGridHorizontalConstraints();
        }

        void SetButtonGridHorizontalConstraints()
        {
            float horizontalSpacing = 85f;

            if (UIDevice.CurrentDevice.Orientation.IsLandscape())
                horizontalSpacing = 180f;

            NSLayoutConstraint mailBtnHorizontalConstraint = mailBtn.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor, -horizontalSpacing);
            mailBtnHorizontalConstraint.SetIdentifier(mailBtnConstraintIdentifier);

            NSLayoutConstraint contactsBtnHorizontalConstraint = contactsBtn.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor, +horizontalSpacing);
            contactsBtnHorizontalConstraint.SetIdentifier(contactsBtnConstraintIdentifier);

            NSLayoutConstraint searchBtnHorizontalConstraint = searchBtn.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor, -horizontalSpacing);
            searchBtnHorizontalConstraint.SetIdentifier(searchBtnConstraintIdentifier);

            NSLayoutConstraint settingsBtnConstraint = settingsBtn.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor, -horizontalSpacing);
            settingsBtnConstraint.SetIdentifier(settingsBtnConstraintIdentifier);

            View.AddConstraints(new[]
            {
                mailBtnHorizontalConstraint,
                contactsBtnHorizontalConstraint,
                searchBtnHorizontalConstraint,
                settingsBtnConstraint
            });
        }

        void BtnClicked()
        {
            DismissViewController(false, null);
        }

        class ReMarkNavigationButton : UIView
        {
            public bool Selected
            {
                set
                {
                    if (value)
                    {
                        Button.BackgroundColor = Theme.DarkBlue;
                        TintColor = Theme.White;
                    }
                    else
                    {
                        Button.BackgroundColor = Theme.White;
                        Button.TintColor = Theme.DarkBlue;
                    }
                }
            }

            readonly UIButton Button = new UIButton
            {
                TintColor = Theme.White,
                BackgroundColor = Theme.White,
                TranslatesAutoresizingMaskIntoConstraints = false,
                ClipsToBounds = true,
                ContentEdgeInsets = new UIEdgeInsets(10f, 10f, 10f, 10f)
            };

            readonly UILabel Title = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                TextColor = Theme.DarkBlue,
                TextAlignment = UITextAlignment.Center,
                Font = Theme.DefaultLightFont,
                MinimumScaleFactor = 0.6f
            };

            public ReMarkNavigationButton(NavigationModule module, Action clicked, nint tag, bool isSelected)
            {
                Tag = tag;

                Selected = isSelected;

                TranslatesAutoresizingMaskIntoConstraints = false;

                AddConstraints(new[]
                {
                    HeightAnchor.ConstraintEqualTo(95f),
                    WidthAnchor.ConstraintEqualTo(85f)
                });

                Button.SetImage(UIImage.FromBundle(module.Image).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
                Button.Layer.BorderColor = Theme.DarkBlue.CGColor;
                Button.Layer.BorderWidth = .7f;
                Button.Layer.CornerRadius = 32.5f;

                AddSubview(Button);
                AddConstraints(new[]
                {
                    Button.HeightAnchor.ConstraintEqualTo(65f),
                    Button.WidthAnchor.ConstraintEqualTo(65f),
                    Button.CenterXAnchor.ConstraintEqualTo(CenterXAnchor),
                    Button.TopAnchor.ConstraintEqualTo(TopAnchor),
                });

                Title.Text = module.Title;

                AddSubview(Title);
                AddConstraints(new[]
                {
                    Title.TopAnchor.ConstraintEqualTo(Button.BottomAnchor, 5f),
                    Title.CenterXAnchor.ConstraintEqualTo(CenterXAnchor),
                    Title.WidthAnchor.ConstraintEqualTo(WidthAnchor)
                });

                if (module.Type == NavigationModule.NavigationModuleType.Dummy)
                {
                    Button.Alpha = 0;
                    Title.Alpha = 0;
                }

                Button.TouchUpInside += (object sender, EventArgs e) =>
                {
                    clicked.Invoke();
                    CommonConfig.MessengerHub.Publish(new NavigationModuleChangedMessage(this, module));
                };
            }
        }
    }
}
