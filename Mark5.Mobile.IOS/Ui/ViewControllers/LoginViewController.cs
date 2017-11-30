using System;
using System.IO;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Authenticator;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Service;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Service;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;
using UserNotifications;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class LoginViewController : AbstractViewController
    {
        #region Animation and layout controls

        const double AnimationDuration = 0.50d;
        const double AnimationInitialDelay = 0.25d;
        const double AnimationDelay = 0.05d;
        const float Damping = 0.9f;
        const float SpringVelocity = 1.2f;

        const float LogoImageViewToViewInitialDistance = 0f;
        const float TextFieldToLogoImageViewInitialDistance = 100f;
        const float TextFieldToTextFieldInitialDistance = 50f;
        const float LoginButtonToTextFieldInitialDistance = 50f;

        const float LogoImageViewToViewDistance = -140f;
        const float TextFieldToLogoImageViewDistance = -140f;
        const float TextFieldToTextFieldDistance = 10f;
        const float LoginButtonToTextFieldDistance = 20f;

        const float LogoImageViewToViewDistanceWithKeyboard = -300f;

        const float TextFieldWidth = 180f;
        const float TextFieldHeight = 28f;

        const float LoginButtonWidth = 100f;
        const float LoginButtonHeight = 24f;

        #endregion

        UIView logoContainer;
        UIImageView logoImageView;

        UIButton settingsButton;

        UITextField usernameTextField;
        UITextField hostnameTextField;
        UITextField passwordTextField;
        UITextField portTextField;
        UIButton loginButton;

        NSLayoutConstraint logoContainerCenterYConstraint;
        NSLayoutConstraint usernameTextFieldTopConstraint;
        NSLayoutConstraint passwordTextFieldTopConstraint;
        NSLayoutConstraint hostnameTextFieldTopConstraint;
        NSLayoutConstraint portTextFieldTopConstraint;
        NSLayoutConstraint loginButtonTopConstraint;

        bool startLogoScaleAnimationDone;

        IAuthenticator authenticator;

        SslMode sslMode = SslMode.On;

        NSObject didChangeFrameNotificationObserver;

        #region UIViewController overrides

        public override void LoadView()
        {
            base.LoadView();

            InitializeView();
            InitializeSubViews();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.LightContent;

            authenticator = AuthenticatorFactory.Create();

            InitializeHandlers();

            didChangeFrameNotificationObserver = UIKeyboard.Notifications.ObserveDidChangeFrame(OnKeyboardDidChangeFrameNotification);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info("Appeared");

            StartLogoScaleAnimation();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.Default;

            usernameTextField.ResignFirstResponder();
            passwordTextField.ResignFirstResponder();
            hostnameTextField.ResignFirstResponder();
            portTextField.ResignFirstResponder();

            DeinitializeHandlers();

            authenticator = null;

            didChangeFrameNotificationObserver?.Dispose();
        }

        protected override void Recycle()
        {
            base.Recycle();

            logoContainer = null;
            logoImageView = null;

            settingsButton = null;

            usernameTextField = null;
            hostnameTextField = null;
            passwordTextField = null;
            portTextField = null;
            loginButton = null;

            logoContainerCenterYConstraint = null;
            usernameTextFieldTopConstraint = null;
            passwordTextFieldTopConstraint = null;
            hostnameTextFieldTopConstraint = null;
            portTextFieldTopConstraint = null;
            loginButtonTopConstraint = null;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (CommonConfig.Logger.IsDebugEnabled())
                CommonConfig.Logger.Debug("Disposed");
        }

        #endregion

        #region Initialize methods

        void InitializeView()
        {
            string logoFileName;
            string backgroundFileName;

            var screenSize = Integration.GetScreenSizeInPixels();
            if (screenSize == Integration.IPhoneRetina55Resolution || screenSize == Integration.IPhoneRetina55ZoomResolution)
            {
                logoFileName = Path.Combine("startscreens", "start-screen-logo-retinahd55.png");
                backgroundFileName = Path.Combine("startscreens", "start-screen-bg-retinahd55.png");
            }
            else if (screenSize == Integration.IPhoneRetina47Resolution || screenSize == Integration.IPhoneRetina47ZoomResolution)
            {
                logoFileName = Path.Combine("startscreens", "start-screen-logo-retinahd47.png");
                backgroundFileName = Path.Combine("startscreens", "start-screen-bg-retinahd47.png");
            }
            else if (screenSize == Integration.IPhoneRetina40Resolution)
            {
                logoFileName = Path.Combine("startscreens", "start-screen-logo-retina4.png");
                backgroundFileName = Path.Combine("startscreens", "start-screen-bg-retina4.png");
            }
            else if (screenSize == Integration.IPadProRetina105ProScreenSize || screenSize == Integration.IPadProRetina129ProScreenSize)
            {
                logoFileName = Path.Combine("startscreens", "start-screen-logo-ipadretina-portrait.png");
                backgroundFileName = Path.Combine("startscreens", "start-screen-bg-ipadretina-portrait.png");
            }
            else if (screenSize == Integration.IPadRetina79ScreenSize || screenSize == Integration.IPadRetina97ScreenSize)
            {
                logoFileName = Path.Combine("startscreens", "start-screen-logo-ipadretina-portrait.png");
                backgroundFileName = Path.Combine("startscreens", "start-screen-bg-ipadretina-portrait.png");
            }
            else
            {
                CommonConfig.Logger.Error($"Unknown device detected. [screenSize={screenSize}]");

                logoFileName = Path.Combine("startscreens", "start-screen-logo-retinahd47.png");
                backgroundFileName = Path.Combine("startscreens", "start-screen-bg-retinahd47.png");
            }

            logoContainer = new UIView
            {
                Opaque = false,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            logoContainer.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            logoContainer.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            logoContainer.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            logoContainer.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            View.AddSubview(logoContainer);
            View.AddConstraints(new[]
            {
                logoContainerCenterYConstraint = NSLayoutConstraint.Create(logoContainer, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, View, NSLayoutAttribute.CenterY, 1f, LogoImageViewToViewInitialDistance),
                NSLayoutConstraint.Create(logoContainer, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, View, NSLayoutAttribute.CenterX, 1f, 0f),
                NSLayoutConstraint.Create(logoContainer, NSLayoutAttribute.Width, NSLayoutRelation.Equal, View, NSLayoutAttribute.Width, 1f, 0f),
                NSLayoutConstraint.Create(logoContainer, NSLayoutAttribute.Height, NSLayoutRelation.Equal, View, NSLayoutAttribute.Height, 1f, 0f)
            });

            logoImageView = new UIImageView
            {
                Image = UIImage.FromBundle(logoFileName),
                ContentMode = UIViewContentMode.ScaleAspectFit,
                Opaque = false,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            logoImageView.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            logoImageView.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            logoImageView.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            logoImageView.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            logoContainer.AddSubview(logoImageView);
            logoContainer.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(logoImageView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, logoContainer, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(logoImageView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, logoContainer, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(logoImageView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, logoContainer, NSLayoutAttribute.Width, 1f, 0f),
                NSLayoutConstraint.Create(logoImageView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, logoContainer, NSLayoutAttribute.Height, 1f, 0f)
            });

            var backgroundImageView = new UIImageView
            {
                Image = UIImage.FromBundle(backgroundFileName),
                ContentMode = UIViewContentMode.ScaleAspectFill,
                Opaque = false,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            View.AddSubview(backgroundImageView);
            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(backgroundImageView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(backgroundImageView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(backgroundImageView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, View, NSLayoutAttribute.Width, 1f, 0f),
                NSLayoutConstraint.Create(backgroundImageView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, View, NSLayoutAttribute.Height, 1f, 0f)
            });
            View.SendSubviewToBack(backgroundImageView);

            settingsButton = new UIButton();
            settingsButton.TitleLabel.Font = Theme.DefaultBoldFont;
            settingsButton.SetTitle(Localization.GetString("settings"), UIControlState.Normal);
            settingsButton.TranslatesAutoresizingMaskIntoConstraints = false;
            View.AddSubview(settingsButton);

            if (Integration.IsRunningAtLeast(11))
            {
                View.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(settingsButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View.SafeAreaLayoutGuide, NSLayoutAttribute.Top, 1f, 0f),
                    NSLayoutConstraint.Create(settingsButton, NSLayoutAttribute.Right, NSLayoutRelation.Equal,  View.SafeAreaLayoutGuide, NSLayoutAttribute.Right, 1f, -10f)
                });
            }
            else
            {
                View.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(settingsButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1f, 20f),
                    NSLayoutConstraint.Create(settingsButton, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1f, -5f)
                });
            }
        }

        void InitializeSubViews()
        {
            usernameTextField = new UITextField
            {
                BorderStyle = UITextBorderStyle.RoundedRect,
                Font = Theme.DefaultFont,
                AutocorrectionType = UITextAutocorrectionType.No,
                ClearButtonMode = UITextFieldViewMode.WhileEditing,
                ReturnKeyType = UIReturnKeyType.Next,
                AttributedPlaceholder = new NSAttributedString(Localization.GetString("username")),
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            View.AddSubview(usernameTextField);
            View.AddConstraints(new[]
            {
                usernameTextFieldTopConstraint = NSLayoutConstraint.Create(usernameTextField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, logoImageView, NSLayoutAttribute.Bottom, 1f, TextFieldToLogoImageViewInitialDistance),
                NSLayoutConstraint.Create(usernameTextField, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, View, NSLayoutAttribute.CenterX, 1f, 0f),
                NSLayoutConstraint.Create(usernameTextField, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, TextFieldWidth),
                NSLayoutConstraint.Create(usernameTextField, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, TextFieldHeight)
            });

            passwordTextField = new UITextField
            {
                BorderStyle = UITextBorderStyle.RoundedRect,
                Font = Theme.DefaultFont,
                AutocapitalizationType = UITextAutocapitalizationType.None,
                AutocorrectionType = UITextAutocorrectionType.No,
                ClearsOnBeginEditing = true,
                ClearButtonMode = UITextFieldViewMode.WhileEditing,
                SecureTextEntry = true,
                ReturnKeyType = UIReturnKeyType.Next,
                AttributedPlaceholder = new NSAttributedString(Localization.GetString("password")),
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            View.AddSubview(passwordTextField);
            View.AddConstraints(new[]
            {
                passwordTextFieldTopConstraint = NSLayoutConstraint.Create(passwordTextField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, usernameTextField, NSLayoutAttribute.Bottom, 1f, 50f),
                NSLayoutConstraint.Create(passwordTextField, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, View, NSLayoutAttribute.CenterX, 1f, 0f),
                NSLayoutConstraint.Create(passwordTextField, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, TextFieldWidth),
                NSLayoutConstraint.Create(passwordTextField, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, TextFieldHeight)
            });

            hostnameTextField = new UITextField
            {
                BorderStyle = UITextBorderStyle.RoundedRect,
                Font = Theme.DefaultFont,
                AutocapitalizationType = UITextAutocapitalizationType.None,
                AutocorrectionType = UITextAutocorrectionType.No,
                ClearButtonMode = UITextFieldViewMode.WhileEditing,
                ReturnKeyType = UIReturnKeyType.Next,
                AttributedPlaceholder = new NSAttributedString(Localization.GetString("hostname")),
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            View.AddSubview(hostnameTextField);
            View.AddConstraints(new[]
            {
                hostnameTextFieldTopConstraint = NSLayoutConstraint.Create(hostnameTextField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, passwordTextField, NSLayoutAttribute.Bottom, 1f, TextFieldToTextFieldInitialDistance),
                NSLayoutConstraint.Create(hostnameTextField, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, View, NSLayoutAttribute.CenterX, 1f, 0f),
                NSLayoutConstraint.Create(hostnameTextField, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, TextFieldWidth),
                NSLayoutConstraint.Create(hostnameTextField, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, TextFieldHeight)
            });

            portTextField = new UITextField
            {
                BorderStyle = UITextBorderStyle.RoundedRect,
                Font = Theme.DefaultFont,
                AutocapitalizationType = UITextAutocapitalizationType.None,
                AutocorrectionType = UITextAutocorrectionType.No,
                ClearButtonMode = UITextFieldViewMode.WhileEditing,
                KeyboardType = UIKeyboardType.NumberPad,
                ReturnKeyType = UIReturnKeyType.Go,
                AttributedPlaceholder = new NSAttributedString(Localization.GetString("port")),
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            View.AddSubview(portTextField);
            View.AddConstraints(new[]
            {
                portTextFieldTopConstraint = NSLayoutConstraint.Create(portTextField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, hostnameTextField, NSLayoutAttribute.Bottom, 1f, TextFieldToTextFieldInitialDistance),
                NSLayoutConstraint.Create(portTextField, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, View, NSLayoutAttribute.CenterX, 1f, 0f),
                NSLayoutConstraint.Create(portTextField, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, TextFieldWidth),
                NSLayoutConstraint.Create(portTextField, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, TextFieldHeight)
            });

            loginButton = new UIButton();
            loginButton.SetTitle(Localization.GetString("login"), UIControlState.Normal);
            loginButton.TitleLabel.Font = Theme.DefaultBoldFont;
            loginButton.TranslatesAutoresizingMaskIntoConstraints = false;
            loginButton.Enabled = false;
            loginButton.Alpha = 0.7f;
            View.AddSubview(loginButton);
            View.AddConstraints(new[]
            {
                loginButtonTopConstraint = NSLayoutConstraint.Create(loginButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, portTextField, NSLayoutAttribute.Bottom, 1f, LoginButtonToTextFieldInitialDistance),
                NSLayoutConstraint.Create(loginButton, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, View, NSLayoutAttribute.CenterX, 1f, 0f),
                NSLayoutConstraint.Create(loginButton, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, LoginButtonWidth),
                NSLayoutConstraint.Create(loginButton, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, LoginButtonHeight)
            });
        }

        #endregion

        #region Animation methods

        void StartLogoScaleAnimation()
        {
            if (startLogoScaleAnimationDone)
                return;

            startLogoScaleAnimationDone = true;

            var bounceAnimation = CAKeyFrameAnimation.GetFromKeyPath("transform");
            bounceAnimation.FillMode = CAFillMode.Both;
            bounceAnimation.RemovedOnCompletion = false;
            bounceAnimation.Duration = 0.4d;
            bounceAnimation.Values = new[]
            {
                NSValue.FromCATransform3D(CATransform3D.MakeScale(1f, 1f, 1f)),
                NSValue.FromCATransform3D(CATransform3D.MakeScale(1.2f, 1.2f, 1.2f)),
                NSValue.FromCATransform3D(CATransform3D.MakeScale(1f, 1f, 1f)),
                NSValue.FromCATransform3D(Integration.IsIPhone() ? CATransform3D.MakeScale(0.6f, 0.6f, 0.6f) : CATransform3D.MakeScale(0.8f, 0.8f, 0.8f))
            };
            bounceAnimation.KeyTimes = new[]
            {
                NSNumber.FromFloat(0f),
                NSNumber.FromFloat(0.5f),
                NSNumber.FromFloat(0.75f),
                NSNumber.FromFloat(1f)
            };
            bounceAnimation.TimingFunctions = new[]
            {
                CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseInEaseOut),
                CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseInEaseOut),
                CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseInEaseOut),
                CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseInEaseOut)
            };
            bounceAnimation.AnimationStopped += BounceAnimation_AnimationStopped;
            logoContainer.Layer.AddAnimation(bounceAnimation, "bounceAnimation");
        }

        void StartShowFieldsAnimation()
        {
            logoContainerCenterYConstraint.Constant = LogoImageViewToViewDistance;
            UIView.AnimateNotify(AnimationDuration, AnimationInitialDelay, Damping, SpringVelocity, UIViewAnimationOptions.CurveEaseInOut, View.LayoutIfNeeded, null);

            usernameTextFieldTopConstraint.Constant = TextFieldToLogoImageViewDistance;
            UIView.AnimateNotify(AnimationDuration, AnimationInitialDelay + AnimationDelay, Damping, SpringVelocity, UIViewAnimationOptions.CurveEaseOut, View.LayoutIfNeeded, null);

            passwordTextFieldTopConstraint.Constant = TextFieldToTextFieldDistance;
            UIView.AnimateNotify(AnimationDuration, AnimationInitialDelay + AnimationDelay * 2, Damping, SpringVelocity, UIViewAnimationOptions.CurveEaseOut, View.LayoutIfNeeded, null);

            hostnameTextFieldTopConstraint.Constant = TextFieldToTextFieldDistance;
            UIView.AnimateNotify(AnimationDuration, AnimationInitialDelay + AnimationDelay * 3, Damping, SpringVelocity, UIViewAnimationOptions.CurveEaseOut, View.LayoutIfNeeded, null);

            portTextFieldTopConstraint.Constant = TextFieldToTextFieldDistance;
            UIView.AnimateNotify(AnimationDuration, AnimationInitialDelay + AnimationDelay * 4, Damping, SpringVelocity, UIViewAnimationOptions.CurveEaseOut, View.LayoutIfNeeded, null);

            loginButtonTopConstraint.Constant = LoginButtonToTextFieldDistance;
            UIView.AnimateNotify(AnimationDuration, AnimationInitialDelay + AnimationDelay * 5, Damping, SpringVelocity, UIViewAnimationOptions.CurveEaseOut, View.LayoutIfNeeded, null);
        }

        void InitializeHandlers()
        {
            settingsButton.TouchUpInside += SettingsButton_TouchUpInside;
            usernameTextField.EditingChanged += TextField_EditingChanged;
            usernameTextField.ShouldReturn = (textField) =>
            {
                passwordTextField.BecomeFirstResponder();
                return true;
            };
            passwordTextField.EditingChanged += TextField_EditingChanged;
            passwordTextField.ShouldReturn = (textField) =>
            {
                hostnameTextField.BecomeFirstResponder();
                return true;
            };
            hostnameTextField.EditingChanged += TextField_EditingChanged;
            hostnameTextField.ShouldReturn = (textField) =>
            {
                portTextField.BecomeFirstResponder();
                return true;
            };
            portTextField.EditingChanged += TextField_EditingChanged;
            portTextField.ShouldReturn = (textField) =>
            {
                portTextField.ResignFirstResponder();
                loginButton.SendActionForControlEvents(UIControlEvent.TouchUpInside);
                return true;
            };
            loginButton.TouchUpInside += LoginButton_TouchUpInside;
        }

        void DeinitializeHandlers()
        {
            settingsButton.TouchUpInside -= SettingsButton_TouchUpInside;
            usernameTextField.EditingChanged -= TextField_EditingChanged;
            usernameTextField.ShouldReturn = null;
            passwordTextField.EditingChanged -= TextField_EditingChanged;
            passwordTextField.ShouldReturn = null;
            hostnameTextField.EditingChanged -= TextField_EditingChanged;
            hostnameTextField.ShouldReturn = null;
            portTextField.EditingChanged -= TextField_EditingChanged;
            portTextField.ShouldReturn = null;
            loginButton.TouchUpInside -= LoginButton_TouchUpInside;
        }

        #endregion

        #region EventHandlers

        void SettingsButton_TouchUpInside(object sender, EventArgs e)
        {
            var sv = new LoginSettingsViewController.SettingsValues { SslMode = sslMode };
            var vc = new LoginSettingsViewController(sv);
            vc.RestrictedSettingsValuesUpdated += LoginSettingsViewController_RestrictedSettingsValuesUpdated;
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        void TextField_EditingChanged(object sender, EventArgs e) => ValidateForm();

        void LoginSettingsViewController_RestrictedSettingsValuesUpdated(object sender, LoginSettingsViewController.SettingsValues values)
        {
            sslMode = values.SslMode;

            var loginSettingsViewController = (LoginSettingsViewController)sender;
            loginSettingsViewController.RestrictedSettingsValuesUpdated -= LoginSettingsViewController_RestrictedSettingsValuesUpdated;
        }

        void BounceAnimation_AnimationStopped(object sender, CAAnimationStateEventArgs e)
        {
            View.LayoutIfNeeded();
            StartShowFieldsAnimation();

            var bounceAnimation = (CAKeyFrameAnimation)sender;
            bounceAnimation.AnimationStopped -= BounceAnimation_AnimationStopped;
        }

        #endregion

        #region Actions

        async void LoginButton_TouchUpInside(object sender, EventArgs e)
        {
            loginButton.TouchUpInside -= LoginButton_TouchUpInside;

            CommonConfig.Logger.Info($"Attempting login...");

            Action dismissAction = null;

            var hapticGenerator = new UINotificationFeedbackGenerator();
            hapticGenerator.Prepare();

            try
            {
                var username = usernameTextField.Text;
                var password = passwordTextField.Text;
                var hostname = hostnameTextField.Text;
                var port = portTextField.Text;

                var errors = false;
                if (!Validator.IsUsernameValid(username))
                {
                    CommonConfig.Logger.Info($"Invalid username was entered: {username}");

                    errors = true;
                    await Dialogs.ShowConfirmAlertAsync(this, Localization.GetString("wrong_username_title"), Localization.GetString("wrong_username_summary"));

                    hapticGenerator.NotificationOccurred(UINotificationFeedbackType.Warning);
                }
                else if (!Validator.IsPasswordValid(password))
                {
                    CommonConfig.Logger.Info($"Invalid password was entered: {password}");

                    errors = true;
                    await Dialogs.ShowConfirmAlertAsync(this, Localization.GetString("wrong_password_title"), Localization.GetString("wrong_password_summary"));

                    hapticGenerator.NotificationOccurred(UINotificationFeedbackType.Warning);
                }
                else if (!Validator.IsHostNameValid(hostname))
                {
                    CommonConfig.Logger.Info($"Invalid hostname was entered: {hostname}");

                    errors = true;
                    await Dialogs.ShowConfirmAlertAsync(this, Localization.GetString("wrong_hostname_title"), Localization.GetString("wrong_hostname_summary"));

                    hapticGenerator.NotificationOccurred(UINotificationFeedbackType.Warning);
                }
                else if (!Validator.IsPortValid(port))
                {
                    CommonConfig.Logger.Info($"Invalid port was entered: {port}");

                    errors = true;
                    await Dialogs.ShowConfirmAlertAsync(this, Localization.GetString("wrong_port_title"), Localization.GetString("wrong_port_summary"));

                    hapticGenerator.NotificationOccurred(UINotificationFeedbackType.Warning);
                }

                if (errors)
                {
                    loginButton.TouchUpInside += LoginButton_TouchUpInside;
                    return;
                }

                if (sslMode == SslMode.Off)
                {
                    hapticGenerator.NotificationOccurred(UINotificationFeedbackType.Warning);

                    if (!await Dialogs.ShowYesNoAlertAsync(this, Localization.GetString("warning"), Localization.GetString("warning_ssl_off"), Localization.GetString("continue"), Localization.GetString("cancel")))
                    {
                        loginButton.TouchUpInside += LoginButton_TouchUpInside;
                        return;
                    }
                }

                if (sslMode == SslMode.AllowSelfSigned)
                {
                    hapticGenerator.NotificationOccurred(UINotificationFeedbackType.Warning);

                    if (!await Dialogs.ShowYesNoAlertAsync(this, Localization.GetString("warning"), Localization.GetString("warning_selfsigned_on"), Localization.GetString("continue"), Localization.GetString("cancel")))
                    {
                        loginButton.TouchUpInside += LoginButton_TouchUpInside;
                        return;
                    }
                }

                CommonConfig.Logger.Info($"Logging in... [username={username}, hostname={hostname}, port={port}, ssl={sslMode}]");

                usernameTextField.ResignFirstResponder();
                passwordTextField.ResignFirstResponder();
                hostnameTextField.ResignFirstResponder();
                portTextField.ResignFirstResponder();

                dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("logging_in___"));

                switch (sslMode)
                {
                    case SslMode.AllowSelfSigned:
                        PlatformConfig.SSLCertificateVerificationManager.EnableSelfSignedCertificates();
                        break;
                    default:
                        PlatformConfig.SSLCertificateVerificationManager.DisableSelfSignedCertificates();
                        break;
                }

                CommonConfig.Logger.Info("Authenticating...");

                var ci = await authenticator.AuthenticateAsync(username, password, sslMode, hostname, int.Parse(port));

                CommonConfig.Logger.Info($"Authenticated - saving connection info {ci}...");

                await authenticator.SaveConnectionInfoAsync(ci);

                CommonConfig.Logger.Info($"Initializing {nameof(Managers)}...");

                Managers.Initialize(ci);
                Managers.DocumentsManager.MaxToFetch = PlatformConfig.Preferences.DocumentsToDownload;
                Managers.DocumentsManager.DocumentBodyTypeRequest = PlatformConfig.Preferences.DocumentBodyRequestType;
                Managers.NotificationsManager.DocumentBodyTypeRequest = PlatformConfig.Preferences.DocumentBodyRequestType;
                Managers.SearchManager.DocumentBodyTypeRequest = PlatformConfig.Preferences.DocumentBodyRequestType;

                CommonConfig.Logger.Info("Retrieving system settings...");

                ServerConfig.SystemSettings = await Managers.SystemManager.GetSystemSettingsAsync();

                CommonConfig.Logger.Info($"Starting services...");
                Services.DocumentsUploadService.Start();
                Services.DocumentPreviewsDownloadService.Start();
                Services.DocumentsDownloadService.Start();

                LocalNotificationsListener.Initialize();

                CommonConfig.Logger.Info($"Refreshing reachability status...");
                await CommonConfig.Reachability.Refresh();

                CommonConfig.Logger.Info($"Registering {nameof(ReachabilityReceiver)}...");
                PlatformConfig.ReachabilityReceiver.Register();

                CommonConfig.Logger.Info($"Logged in - will present {nameof(SplitMainViewController)}");

                dismissAction?.Invoke();

                UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound, (result, error) => { });

                CommonConfig.UsageAnalytics.SetUserProperty(UserProperty.Hostname, hostname);
                CommonConfig.UsageAnalytics.SetUserProperty(UserProperty.Username, username.ToLowerInvariant());
                CommonConfig.UsageAnalytics.SetUserProperty(UserProperty.SSL, sslMode.ToString());

                UIViewController vc;
                if (Integration.IsIPad())
                    vc = new SplitMainViewController { ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve };
                else
                    vc = new SimpleMainViewController { ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve };

                var window = ((AppDelegate)UIApplication.SharedApplication.Delegate).Window;
                UIView.TransitionNotify(window, 0.25, UIViewAnimationOptions.TransitionCrossDissolve, () => window.RootViewController = vc, null);

            }
            catch (Exception ex)
            {
                dismissAction?.Invoke();

                CommonConfig.Logger.Error("Log in failed - exception", ex);

                if (ex.InnerException != null)
                    CommonConfig.Logger.Error("Log in failed - inner exception", ex.InnerException);

                await Dialogs.ShowConfirmAlertAsync(this, Localization.GetString("login_failed"), Localization.GetString("login_failed_desc"));

                hapticGenerator.NotificationOccurred(UINotificationFeedbackType.Error);

                usernameTextField.BecomeFirstResponder();

                loginButton.TouchUpInside += LoginButton_TouchUpInside;
            }
        }

        #endregion

        #region Notification receivers

        void OnKeyboardDidChangeFrameNotification(object sender, UIKeyboardEventArgs e)
        {
            if (IsViewLoaded)
                SlideViewOverKeyboard(e, true);
        }

        #endregion

        #region Helper methods

        void SlideViewOverKeyboard(UIKeyboardEventArgs e, bool up)
        {
            View.LayoutIfNeeded();

            var remainingScreenHeight = View.Frame.Height - e.FrameEnd.Height;
            var formHeight = loginButton.Frame.GetMaxY() - usernameTextField.Frame.GetMinY();
            var distanceFromTopOfTheScreen = (remainingScreenHeight - formHeight) / 1.5f;
            var requiredMovement = usernameTextField.Frame.GetMinY() - distanceFromTopOfTheScreen;

            logoContainerCenterYConstraint.Constant = up ? logoContainer.Frame.Top - requiredMovement : LogoImageViewToViewDistance;
            UIView.AnimateNotify(AnimationDuration, AnimationInitialDelay, Damping, SpringVelocity, UIViewAnimationOptions.CurveEaseInOut, () => { logoContainer.Alpha = up ? 0f : 1f; }, (finished) => { View.LayoutIfNeeded(); });
        }

        void ValidateForm()
        {
            var result = true;
            result &= Validator.IsUsernameValid(usernameTextField.Text);
            result &= Validator.IsPasswordValid(passwordTextField.Text);
            result &= Validator.IsHostNameValid(hostnameTextField.Text);
            result &= Validator.IsPortValid(portTextField.Text);

            loginButton.Enabled = result;
            loginButton.Alpha = result ? 1f : 0.7f;
        }

        #endregion
    }
}