//
// Project: Mark5.Mobile.IOS
// File: LoginViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.IO;
using System.Threading.Tasks;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Authenticator;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Services;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{

    public class LoginViewController : UIViewController
    {

        #region Animation and layout controls

        const double AnimationDuration = 0.50d;
        const double AnimationInitialDelay = 0.25d;
        const double AnimationDelay = 0.05d;
        const float Damping = 0.9f;
        const float SpringVelocity = 1.2f;

        const float LogoImageViewToViewInitialDistance = 0.0f;
        const float TextFieldToLogoImageViewInitialDistance = 100.0f;
        const float TextFieldToTextFieldInitialDistance = 50.0f;
        const float LoginButtonToTextFieldInitialDistance = 50.0f;

        const float LogoImageViewToViewDistance = -140.0f;
        const float TextFieldToLogoImageViewDistance = -140.0f;
        const float TextFieldToTextFieldDistance = 10.0f;
        const float LoginButtonToTextFieldDistance = 20.0f;

        const float LogoImageViewToViewDistanceWithKeyboard = -300.0f;

        const float TextFieldWidth = 180.0f;
        const float TextFieldHeight = 28.0f;

        const float LoginButtonWidth = 100.0f;
        const float LoginButtonHeight = 24.0f;

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

            authenticator = AuthenticatorFactory.Create();

            InitializeHandlers();

            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.DidChangeFrameNotification, OnKeyboardDidChangeFrameNotification);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info($"{nameof(LoginViewController)} appeared");

            StartLogoScaleAnimation();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            usernameTextField.ResignFirstResponder();
            passwordTextField.ResignFirstResponder();
            hostnameTextField.ResignFirstResponder();
            portTextField.ResignFirstResponder();

            authenticator = null;

            DeinitializeHandlers();

            NSNotificationCenter.DefaultCenter.RemoveObservers(new[]
                {
                    UIKeyboard.DidChangeFrameNotification
                });
        }

        #endregion

        #region Initialize methods

        void InitializeView()
        {
            string logoFileName;
            string backgroundFileName;

            var screenSize = Integration.GetScreenSizeInPixels();
            if (screenSize == Integration.IPhone6PlusScreenSize || screenSize == Integration.IPhone6PlusZoomScreenSize)
            { // iPhone 6 Plus
                logoFileName = Path.Combine("startscreens", "start-screen-logo-retinahd55.png");
                backgroundFileName = Path.Combine("startscreens", "start-screen-bg-retinahd55.png");
            }
            else if (screenSize == Integration.IPhone6ScreenSize || screenSize == Integration.IPhone6ZoomScreenSize)
            { // iPhone 6
                logoFileName = Path.Combine("startscreens", "start-screen-logo-retinahd47.png");
                backgroundFileName = Path.Combine("startscreens", "start-screen-bg-retinahd47.png");
            }
            else if (screenSize == Integration.IPhone5ScreenSize)
            { //iPhone 5, iPhone 5s, iPod Touch 5G
                logoFileName = Path.Combine("startscreens", "start-screen-logo-retina4.png");
                backgroundFileName = Path.Combine("startscreens", "start-screen-bg-retina4.png");
            }
            else if (screenSize == Integration.IPhone4SScreenSize)
            { //iPhone 4S
                logoFileName = Path.Combine("startscreens", "start-screen-logo-retina35.png");
                backgroundFileName = Path.Combine("startscreens", "start-screen-bg-retina35.png");
            }
            else if (screenSize == Integration.IPadScreenSize)
            {
                logoFileName = Path.Combine("startscreens", "start-screen-logo-ipad-portrait.png");
                backgroundFileName = Path.Combine("startscreens", "start-screen-bg-ipad-portrait.png");
            }
            else if (screenSize == Integration.IPadRetinaScreenSize)
            {
                logoFileName = Path.Combine("startscreens", "start-screen-logo-ipadretina-portrait.png");
                backgroundFileName = Path.Combine("startscreens", "start-screen-bg-ipadretina-portrait.png");
            }
            else if (screenSize == Integration.IPadProScreenSize)
            {
                logoFileName = Path.Combine("startscreens", "start-screen-logo-ipadretina-portrait.png");
                backgroundFileName = Path.Combine("startscreens", "start-screen-bg-ipadretina-portrait.png");
            }
            else
            { // unknown devices

                CommonConfig.Logger.Error($"Unknown device detected. [screenSize={screenSize}]");

                logoFileName = Path.Combine("startscreens", "start-screen-logo-retinahd47.png");
                backgroundFileName = Path.Combine("startscreens", "start-screen-bg-retinahd47.png");
            }

            logoContainer = new UIView();
            logoContainer.Opaque = false;
            logoContainer.TranslatesAutoresizingMaskIntoConstraints = false;
            logoContainer.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            logoContainer.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            logoContainer.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            logoContainer.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            View.AddSubview(logoContainer);
            logoContainerCenterYConstraint = NSLayoutConstraint.Create(logoContainer, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, View, NSLayoutAttribute.CenterY, 1.0f, LogoImageViewToViewInitialDistance);
            View.AddConstraints(new[]
                {
                    logoContainerCenterYConstraint,
                    NSLayoutConstraint.Create(logoContainer, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, View, NSLayoutAttribute.CenterX, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(logoContainer, NSLayoutAttribute.Width, NSLayoutRelation.Equal, View, NSLayoutAttribute.Width, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(logoContainer, NSLayoutAttribute.Height, NSLayoutRelation.Equal, View, NSLayoutAttribute.Height, 1.0f, 0.0f)
                });

            logoImageView = new UIImageView();
            logoImageView.Image = UIImage.FromBundle(logoFileName);
            logoImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
            logoImageView.Opaque = false;
            logoImageView.TranslatesAutoresizingMaskIntoConstraints = false;
            logoImageView.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            logoImageView.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            logoImageView.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            logoImageView.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            logoContainer.AddSubview(logoImageView);
            logoContainer.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(logoImageView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, logoContainer, NSLayoutAttribute.Top, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(logoImageView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, logoContainer, NSLayoutAttribute.Left, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(logoImageView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, logoContainer, NSLayoutAttribute.Width, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(logoImageView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, logoContainer, NSLayoutAttribute.Height, 1.0f, 0.0f)
                });


            var backgroundImageView = new UIImageView();
            backgroundImageView.Image = UIImage.FromBundle(backgroundFileName);
            backgroundImageView.ContentMode = UIViewContentMode.ScaleAspectFill;
            backgroundImageView.Opaque = false;
            backgroundImageView.TranslatesAutoresizingMaskIntoConstraints = false;
            View.AddSubview(backgroundImageView);
            View.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(backgroundImageView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(backgroundImageView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(backgroundImageView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, View, NSLayoutAttribute.Width, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(backgroundImageView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, View, NSLayoutAttribute.Height, 1.0f, 0.0f)
                });
            View.SendSubviewToBack(backgroundImageView);

            settingsButton = new UIButton();
            settingsButton.TitleLabel.Font = Theme.DefaultBoldFont;
            settingsButton.SetTitle(Localization.GetString("settings"), UIControlState.Normal);
            settingsButton.TranslatesAutoresizingMaskIntoConstraints = false;
            View.AddSubview(settingsButton);
            View.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(settingsButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1.0f, 20.0f),
                    NSLayoutConstraint.Create(settingsButton, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1.0f, -5.0f)
                });
        }

        void InitializeSubViews()
        {
            usernameTextField = new UITextField();
            usernameTextField.BorderStyle = UITextBorderStyle.RoundedRect;
            usernameTextField.Font = Theme.DefaultFont;
            usernameTextField.AutocapitalizationType = UITextAutocapitalizationType.AllCharacters;
            usernameTextField.AutocorrectionType = UITextAutocorrectionType.No;
            usernameTextField.ClearButtonMode = UITextFieldViewMode.WhileEditing;
            usernameTextField.ReturnKeyType = UIReturnKeyType.Next;
            usernameTextField.AttributedPlaceholder = new NSAttributedString(Localization.GetString("username"));
            usernameTextField.TranslatesAutoresizingMaskIntoConstraints = false;
            View.AddSubview(usernameTextField);
            usernameTextFieldTopConstraint = NSLayoutConstraint.Create(usernameTextField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, logoImageView, NSLayoutAttribute.Bottom, 1.0f, TextFieldToLogoImageViewInitialDistance);
            View.AddConstraints(new[]
                {
                    usernameTextFieldTopConstraint,
                    NSLayoutConstraint.Create(usernameTextField, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, View, NSLayoutAttribute.CenterX, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(usernameTextField, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1.0f, TextFieldWidth),
                    NSLayoutConstraint.Create(usernameTextField, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1.0f, TextFieldHeight)
                });

            passwordTextField = new UITextField();
            passwordTextField.BorderStyle = UITextBorderStyle.RoundedRect;
            passwordTextField.Font = Theme.DefaultFont;
            passwordTextField.AutocapitalizationType = UITextAutocapitalizationType.None;
            passwordTextField.AutocorrectionType = UITextAutocorrectionType.No;
            passwordTextField.ClearsOnBeginEditing = true;
            passwordTextField.ClearButtonMode = UITextFieldViewMode.WhileEditing;
            passwordTextField.SecureTextEntry = true;
            passwordTextField.ReturnKeyType = UIReturnKeyType.Next;
            passwordTextField.AttributedPlaceholder = new NSAttributedString(Localization.GetString("password"));
            passwordTextField.TranslatesAutoresizingMaskIntoConstraints = false;
            View.AddSubview(passwordTextField);
            passwordTextFieldTopConstraint = NSLayoutConstraint.Create(passwordTextField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, usernameTextField, NSLayoutAttribute.Bottom, 1.0f, 50.0f);
            View.AddConstraints(new[]
                {
                    passwordTextFieldTopConstraint,
                    NSLayoutConstraint.Create(passwordTextField, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, View, NSLayoutAttribute.CenterX, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(passwordTextField, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1.0f, TextFieldWidth),
                    NSLayoutConstraint.Create(passwordTextField, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1.0f, TextFieldHeight)
                });

            hostnameTextField = new UITextField();
            hostnameTextField.BorderStyle = UITextBorderStyle.RoundedRect;
            hostnameTextField.Font = Theme.DefaultFont;
            hostnameTextField.AutocapitalizationType = UITextAutocapitalizationType.None;
            hostnameTextField.AutocorrectionType = UITextAutocorrectionType.No;
            hostnameTextField.ClearButtonMode = UITextFieldViewMode.WhileEditing;
            hostnameTextField.ReturnKeyType = UIReturnKeyType.Next;
            hostnameTextField.AttributedPlaceholder = new NSAttributedString(Localization.GetString("hostname"));
            hostnameTextField.TranslatesAutoresizingMaskIntoConstraints = false;
            View.AddSubview(hostnameTextField);
            hostnameTextFieldTopConstraint = NSLayoutConstraint.Create(hostnameTextField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, passwordTextField, NSLayoutAttribute.Bottom, 1.0f, TextFieldToTextFieldInitialDistance);
            View.AddConstraints(new[]
                {
                    hostnameTextFieldTopConstraint,
                    NSLayoutConstraint.Create(hostnameTextField, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, View, NSLayoutAttribute.CenterX, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(hostnameTextField, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1.0f, TextFieldWidth),
                    NSLayoutConstraint.Create(hostnameTextField, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1.0f, TextFieldHeight)
                });

            portTextField = new UITextField();
            portTextField.BorderStyle = UITextBorderStyle.RoundedRect;
            portTextField.Font = Theme.DefaultFont;
            portTextField.AutocapitalizationType = UITextAutocapitalizationType.None;
            portTextField.AutocorrectionType = UITextAutocorrectionType.No;
            portTextField.ClearButtonMode = UITextFieldViewMode.WhileEditing;
            portTextField.KeyboardType = UIKeyboardType.NumberPad;
            portTextField.ReturnKeyType = UIReturnKeyType.Go;
            portTextField.AttributedPlaceholder = new NSAttributedString(Localization.GetString("port"));
            portTextField.TranslatesAutoresizingMaskIntoConstraints = false;
            View.AddSubview(portTextField);
            portTextFieldTopConstraint = NSLayoutConstraint.Create(portTextField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, hostnameTextField, NSLayoutAttribute.Bottom, 1.0f, TextFieldToTextFieldInitialDistance);
            View.AddConstraints(new[]
                {
                    portTextFieldTopConstraint,
                    NSLayoutConstraint.Create(portTextField, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, View, NSLayoutAttribute.CenterX, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(portTextField, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1.0f, TextFieldWidth),
                    NSLayoutConstraint.Create(portTextField, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1.0f, TextFieldHeight)
                });

            loginButton = new UIButton();
            loginButton.SetTitle(Localization.GetString("login"), UIControlState.Normal);
            loginButton.TitleLabel.Font = Theme.DefaultBoldFont;
            loginButton.TranslatesAutoresizingMaskIntoConstraints = false;
            loginButton.Enabled = false;
            View.AddSubview(loginButton);
            loginButtonTopConstraint = NSLayoutConstraint.Create(loginButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, portTextField, NSLayoutAttribute.Bottom, 1.0f, LoginButtonToTextFieldInitialDistance);
            View.AddConstraints(new[]
                {
                    loginButtonTopConstraint,
                    NSLayoutConstraint.Create(loginButton, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, View, NSLayoutAttribute.CenterX, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(loginButton, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1.0f, LoginButtonWidth),
                    NSLayoutConstraint.Create(loginButton, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1.0f, LoginButtonHeight)
                });
        }

        #endregion

        #region Animation methods

        void StartLogoScaleAnimation()
        {
            if (startLogoScaleAnimationDone)
            {
                return;
            }

            startLogoScaleAnimationDone = true;

            var bounceAnimation = CAKeyFrameAnimation.GetFromKeyPath("transform");
            bounceAnimation.FillMode = CAFillMode.Both;
            bounceAnimation.RemovedOnCompletion = false;
            bounceAnimation.Duration = 0.4d;
            bounceAnimation.Values = new[]
            {
                NSValue.FromCATransform3D(CATransform3D.MakeScale(1.0f, 1.0f, 1.0f)),
                NSValue.FromCATransform3D(CATransform3D.MakeScale(1.2f, 1.2f, 1.2f)),
                NSValue.FromCATransform3D(CATransform3D.MakeScale(1.0f, 1.0f, 1.0f)),
                NSValue.FromCATransform3D(Integration.IsIPhone() ? CATransform3D.MakeScale(0.6f, 0.6f, 0.6f) : CATransform3D.MakeScale(0.8f, 0.8f, 0.8f))
            };
            bounceAnimation.KeyTimes = new[]
            {
                NSNumber.FromFloat(0.0f),
                NSNumber.FromFloat(0.5f),
                NSNumber.FromFloat(0.75f),
                NSNumber.FromFloat(1.0f)
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
            UIView.AnimateNotify(AnimationDuration, AnimationInitialDelay + (AnimationDelay * 2), Damping, SpringVelocity, UIViewAnimationOptions.CurveEaseOut, View.LayoutIfNeeded, null);

            hostnameTextFieldTopConstraint.Constant = TextFieldToTextFieldDistance;
            UIView.AnimateNotify(AnimationDuration, AnimationInitialDelay + (AnimationDelay * 3), Damping, SpringVelocity, UIViewAnimationOptions.CurveEaseOut, View.LayoutIfNeeded, null);

            portTextFieldTopConstraint.Constant = TextFieldToTextFieldDistance;
            UIView.AnimateNotify(AnimationDuration, AnimationInitialDelay + (AnimationDelay * 4), Damping, SpringVelocity, UIViewAnimationOptions.CurveEaseOut, View.LayoutIfNeeded, null);

            loginButtonTopConstraint.Constant = LoginButtonToTextFieldDistance;
            UIView.AnimateNotify(AnimationDuration, AnimationInitialDelay + (AnimationDelay * 5), Damping, SpringVelocity, UIViewAnimationOptions.CurveEaseOut, View.LayoutIfNeeded, null);
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
            var rsv = new LoginSettingsViewController.RestrictedSettingsValues { SslMode = sslMode };
            var loginSettingsViewController = new LoginSettingsViewController(rsv);
            loginSettingsViewController.RestrictedSettingsValuesUpdated += LoginSettingsViewController_RestrictedSettingsValuesUpdated;
            PresentViewController(new UINavigationController(loginSettingsViewController), true, null);
        }

        void TextField_EditingChanged(object sender, EventArgs e) => ValidateForm();

        void LoginSettingsViewController_RestrictedSettingsValuesUpdated(object sender, LoginSettingsViewController.RestrictedSettingsValues values)
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

            Func<Task> dismissAction = null;

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
                    await Dialogs.ShowConfirmDialogAsync(this, Localization.GetString("wrong_username_title"), Localization.GetString("wrong_username_summary"));
                }
                else if (!Validator.IsPasswordValid(password))
                {
                    CommonConfig.Logger.Info($"Invalid password was entered: {password}");

                    errors = true;
                    await Dialogs.ShowConfirmDialogAsync(this, Localization.GetString("wrong_password_title"), Localization.GetString("wrong_password_summary"));
                }
                else if (!Validator.IsHostNameValid(hostname))
                {
                    CommonConfig.Logger.Info($"Invalid hostname was entered: {hostname}");

                    errors = true;
                    await Dialogs.ShowConfirmDialogAsync(this, Localization.GetString("wrong_hostname_title"), Localization.GetString("wrong_hostname_summary"));
                }
                else if (!Validator.IsPortValid(port))
                {
                    CommonConfig.Logger.Info($"Invalid port was entered: {port}");

                    errors = true;
                    await Dialogs.ShowConfirmDialogAsync(this, Localization.GetString("wrong_port_title"), Localization.GetString("wrong_port_summary"));
                }

                if (errors)
                {
                    loginButton.TouchUpInside += LoginButton_TouchUpInside;
                    return;
                }
                
                if (sslMode == SslMode.Off && !await Dialogs.ShowYesNoDialogAsync(this, Localization.GetString("warning"), Localization.GetString("warning_ssl_off"), Localization.GetString("continue"), Localization.GetString("cancel")))
                {
                    loginButton.TouchUpInside += LoginButton_TouchUpInside;
                    return;
                }

                if (sslMode == SslMode.AllowSelfSigned && !await Dialogs.ShowYesNoDialogAsync(this, Localization.GetString("warning"), Localization.GetString("warning_selfsigned_on"), Localization.GetString("continue"), Localization.GetString("cancel")))
                {
                    loginButton.TouchUpInside += LoginButton_TouchUpInside;
                    return;
                }

                CommonConfig.Logger.Info("Logging in...");

                dismissAction = Dialogs.ShowInfiniteProgressDialog(this, Localization.GetString("logging_in"), Localization.GetString("please_wait___"));

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
                var policies = Managers.DownloadManager.DownloadPolicies;
                policies[ObjectType.Document] = new DownloadFoldersPolicy();
                if (PlatformConfig.Preferences.SynchroniseContacts)
                {
                    policies[ObjectType.Contact] = new DownloadAllPolicy();
                }
                if (PlatformConfig.Preferences.SynchroniseShortcodes)
                {
                    policies[ObjectType.Shortcode] = new DownloadAllPolicy();
                }

                CommonConfig.Logger.Info("Retrieving system settings...");

                ServerConfig.SystemSettings = await Managers.SystemManager.GetSystemSettingsAsync();

                CommonConfig.Logger.Info($"Starting {nameof(IDownloadManager)} and {nameof(IOutgoingDocumentsManager)}...");

                await Managers.DownloadManager.Start();
                await Managers.OutgoingDocumentsManager.Start();

                CommonConfig.Logger.Info($"Refreshing reachability status...");
                await CommonConfig.ReachabilityService.Refresh();

                CommonConfig.Logger.Info($"Registering {nameof(ReachabilityReceiver)}...");
                PlatformConfig.ReachabilityReceiver.Register();

                CommonConfig.Logger.Info($"Logged in - will present {nameof(MainViewController)}");

                if (dismissAction != null) await dismissAction();

                PresentViewController(new MainViewController
                {
                    ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve
                }, true, null);
            }
            catch (Exception ex)
            {
                if (dismissAction != null) await dismissAction();

                CommonConfig.Logger.Error("Log in failed", ex);

                await Dialogs.ShowConfirmDialogAsync(this, "failed", "login failed");

                loginButton.TouchUpInside += LoginButton_TouchUpInside;
            }
        }

        #endregion

        #region Notification receivers

        void OnKeyboardDidChangeFrameNotification(NSNotification notification)
        {
            if (IsViewLoaded)
            {
                SlideViewOverKeyboard(notification, true);
            }
        }

        #endregion

        #region Helper methods

        void SlideViewOverKeyboard(NSNotification notification, bool up)
        {
            View.LayoutIfNeeded();

            var remainingScreenHeight = View.Frame.Height - KeyboardUtilities.KeyboardHeightFromNotification(notification);
            var formHeight = loginButton.Frame.GetMaxY() - usernameTextField.Frame.GetMinY();
            var distanceFromTopOfTheScreen = (remainingScreenHeight - formHeight) / 1.5f;
            var requiredMovement = usernameTextField.Frame.GetMinY() - distanceFromTopOfTheScreen;

            logoContainerCenterYConstraint.Constant = up ? logoContainer.Frame.Top - requiredMovement : LogoImageViewToViewDistance;
            UIView.AnimateNotify(AnimationDuration, AnimationInitialDelay, Damping, SpringVelocity, UIViewAnimationOptions.CurveEaseInOut, () =>
                {
                    logoContainer.Alpha = up ? 0.0f : 1.0f;
                }, (finished) =>
                {
                    View.LayoutIfNeeded();
                });
        }

        void ValidateForm()
        {
            var result = true;
            result &= Validator.IsUsernameValid(usernameTextField.Text);
            result &= Validator.IsPasswordValid(passwordTextField.Text);
            result &= Validator.IsHostNameValid(hostnameTextField.Text);
            result &= Validator.IsPortValid(portTextField.Text);

            loginButton.Enabled = result;
        }

        #endregion
    }
}
