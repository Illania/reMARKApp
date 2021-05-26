using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Airbnb.Lottie;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Authenticator;
using Mark5.Mobile.Common.Azure;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Azure;
using Mark5.Mobile.Common.Service;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Service;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using Mark5.ServiceReference.Exceptions;
using Microsoft.Extensions.Logging;
using Sentry;
using UIKit;
using UserNotifications;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class LoginViewController : AbstractViewController
    {
        #region Animation and layout controls

        const float TextFieldToAnimationViewDistance = 50;
        const float TextFieldToTextFieldDistance = 10f;
        const float LoginButtonToTextFieldDistance = 20f;
        const float OrLabelToLoginButtonDistance = 15f;
        const float LoginWithMicrosoftButtonToOrLabelDistance = 20f;

        const float TextFieldWidth = 180f;
        const float TextFieldHeight = 28f;

        const float LoginButtonWidth = 100f;
        const float LoginButtonHeight = 24f;

        #endregion

        CancellationTokenSource cts;
        Action dismissAction;

        LOTAnimationView animationView;

        UIButton settingsButton;

        UITextField usernameTextField;
        UITextField hostnameTextField;
        UITextField passwordTextField;
        UITextField portTextField;
        UILabel orLabel;
        UIButton showPasswordButton;
        UIButton loginButton;
        UIButton loginWithMicrosoftButton;

        NSLayoutConstraint containerCenter;

        bool startLogoScaleAnimationDone;

        IAuthenticator authenticator;
        MicrosoftAuthService microsoftAuthService;

        SslMode sslMode = SslMode.On;

        NSObject keyboardWillAppearObsever;

        ConnectionInfo retainedConnectionInfo;

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

            keyboardWillAppearObsever = UIKeyboard.Notifications.ObserveWillChangeFrame(OnKeyboardWillChangeFrame);
            UIKeyboard.Notifications.ObserveWillHide(OnKeyboardWillHide);
        }

        public override async void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info("Appeared");

            StartLogoScaleAnimation();

            await RefreshData();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            usernameTextField.ResignFirstResponder();
            passwordTextField.ResignFirstResponder();
            hostnameTextField.ResignFirstResponder();
            portTextField.ResignFirstResponder();

            DeinitializeHandlers();

            authenticator = null;

            keyboardWillAppearObsever?.Dispose();
        }

        protected override void Recycle()
        {
            base.Recycle();

            animationView = null;

            settingsButton = null;

            usernameTextField = null;
            hostnameTextField = null;
            passwordTextField = null;
            portTextField = null;
            loginButton = null;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (CommonConfig.Logger.IsDebugEnabled())
                CommonConfig.Logger.Debug("Disposed");
        }

        #endregion

        #region Initialize methods

        UIView containerView;

        void InitializeView()
        {
            View.BackgroundColor = Theme.LightGray;

            containerView = new UIView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Opaque = false,
            };
            containerView.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            containerView.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            containerView.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            containerView.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            View.AddSubview(containerView);
            View.AddConstraints(new[]
            {
                containerView.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
                containerCenter = containerView.CenterYAnchor.ConstraintEqualTo(View.CenterYAnchor),
            });

            animationView = LOTAnimationView.AnimationNamed("splash");
            animationView.ContentMode = UIViewContentMode.ScaleAspectFit;
            animationView.TranslatesAutoresizingMaskIntoConstraints = false;
            containerView.AddSubview(animationView);

            View.AddConstraints(new[]
            {
                animationView.CenterXAnchor.ConstraintEqualTo(containerView.CenterXAnchor),
                animationView.TopAnchor.ConstraintEqualTo(containerView.TopAnchor),
                animationView.LeftAnchor.ConstraintEqualTo(containerView.LeftAnchor),
                animationView.RightAnchor.ConstraintEqualTo(containerView.RightAnchor),
                animationView.WidthAnchor.ConstraintLessThanOrEqualTo(View.WidthAnchor, 0.8f),
                animationView.WidthAnchor.ConstraintLessThanOrEqualTo(450f),
                animationView.HeightAnchor.ConstraintEqualTo(animationView.WidthAnchor),
            });

            settingsButton = new UIButton();
            settingsButton.TitleLabel.Font = Theme.DefaultBoldFont;
            settingsButton.SetTitle(Localization.GetString("settings"), UIControlState.Normal);
            settingsButton.SetTitleColor(Theme.DarkBlue, UIControlState.Normal);
            settingsButton.TranslatesAutoresizingMaskIntoConstraints = false;
            settingsButton.Alpha = 0;
            View.AddSubview(settingsButton);

            if (Integration.IsRunningAtLeast(11))
            {
                View.AddConstraints(new[]
                {
                    settingsButton.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                    settingsButton.RightAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.RightAnchor, -10f)
                });
            }
            else
            {
                View.AddConstraints(new[]
                {
                    settingsButton.TopAnchor.ConstraintEqualTo(View.TopAnchor, 20f),
                    settingsButton.RightAnchor.ConstraintEqualTo(View.RightAnchor, -5f)
                });
            }

        }

        void InitializeSubViews()
        {
            var placeholderAttributes = new UIStringAttributes
            {
                ForegroundColor = Theme.LightGray
            };

            usernameTextField = new UITextField
            {
                BorderStyle = UITextBorderStyle.RoundedRect,
                Font = Theme.DefaultFont,
                TextColor = Theme.White,
                TintColor = Theme.White,
                AutocorrectionType = UITextAutocorrectionType.No,
                ClearButtonMode = UITextFieldViewMode.WhileEditing,
                ReturnKeyType = UIReturnKeyType.Next,
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = Theme.DarkBlue,
                AttributedPlaceholder = new NSMutableAttributedString(Localization.GetString("username"), placeholderAttributes),
                Alpha = 0,
            };

            containerView.AddSubview(usernameTextField);
            containerView.AddConstraints(new[]
            {
                usernameTextField.TopAnchor.ConstraintEqualTo(animationView.BottomAnchor, TextFieldToAnimationViewDistance),
                usernameTextField.CenterXAnchor.ConstraintEqualTo(containerView.CenterXAnchor),
                usernameTextField.WidthAnchor.ConstraintEqualTo(TextFieldWidth),
                usernameTextField.HeightAnchor.ConstraintEqualTo(TextFieldHeight)
            });

            passwordTextField = new UITextField
            {
                BorderStyle = UITextBorderStyle.RoundedRect,
                Font = Theme.DefaultFont,
                TextColor = Theme.White,
                TintColor = Theme.White,
                AutocapitalizationType = UITextAutocapitalizationType.None,
                AutocorrectionType = UITextAutocorrectionType.No,
                ClearsOnBeginEditing = true,
                ClearButtonMode = UITextFieldViewMode.WhileEditing,
                SecureTextEntry = true,
                ReturnKeyType = UIReturnKeyType.Next,
                AttributedPlaceholder = new NSMutableAttributedString(Localization.GetString("password"), placeholderAttributes),
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = Theme.DarkBlue,
                Alpha = 0,
            };
            containerView.AddSubview(passwordTextField);
            containerView.AddConstraints(new[]
            {
                passwordTextField.TopAnchor.ConstraintEqualTo(usernameTextField.BottomAnchor, TextFieldToTextFieldDistance),
                passwordTextField.CenterXAnchor.ConstraintEqualTo(usernameTextField.CenterXAnchor),
                passwordTextField.WidthAnchor.ConstraintEqualTo(TextFieldWidth),
                passwordTextField.HeightAnchor.ConstraintEqualTo(TextFieldHeight)
            });

            showPasswordButton = new UIButton()
            {
                Frame = new CGRect(0, 0, 30, 30),
            };
            FlipShowHidePasswordImage(passwordTextField.SecureTextEntry);
            showPasswordButton.ContentEdgeInsets = new UIEdgeInsets(top: 0, left: 0, bottom: 0, right: 5);
            showPasswordButton.ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;

            passwordTextField.RightViewMode = UITextFieldViewMode.Always;
            passwordTextField.RightView = showPasswordButton;

            hostnameTextField = new UITextField
            {
                BorderStyle = UITextBorderStyle.RoundedRect,
                Font = Theme.DefaultFont,
                TextColor = Theme.White,
                TintColor = Theme.White,
                AutocapitalizationType = UITextAutocapitalizationType.None,
                AutocorrectionType = UITextAutocorrectionType.No,
                ClearButtonMode = UITextFieldViewMode.WhileEditing,
                ReturnKeyType = UIReturnKeyType.Next,
                AttributedPlaceholder = new NSMutableAttributedString(Localization.GetString("hostname"), placeholderAttributes),
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = Theme.DarkBlue,
                Alpha = 0,
            };
            containerView.AddSubview(hostnameTextField);
            containerView.AddConstraints(new[]
            {
                hostnameTextField.TopAnchor.ConstraintEqualTo(passwordTextField.BottomAnchor, TextFieldToTextFieldDistance),
                hostnameTextField.CenterXAnchor.ConstraintEqualTo(usernameTextField.CenterXAnchor),
                hostnameTextField.WidthAnchor.ConstraintEqualTo(TextFieldWidth),
                hostnameTextField.HeightAnchor.ConstraintEqualTo(TextFieldHeight)
            });

            portTextField = new UITextField
            {
                BorderStyle = UITextBorderStyle.RoundedRect,
                Font = Theme.DefaultFont,
                TextColor = Theme.White,
                TintColor = Theme.White,
                AutocapitalizationType = UITextAutocapitalizationType.None,
                AutocorrectionType = UITextAutocorrectionType.No,
                ClearButtonMode = UITextFieldViewMode.WhileEditing,
                KeyboardType = UIKeyboardType.NumberPad,
                ReturnKeyType = UIReturnKeyType.Go,
                AttributedPlaceholder = new NSMutableAttributedString(Localization.GetString("port"), placeholderAttributes),
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = Theme.DarkBlue,
                Alpha = 0,
            };
            containerView.AddSubview(portTextField);
            containerView.AddConstraints(new[]
            {
                portTextField.TopAnchor.ConstraintEqualTo(hostnameTextField.BottomAnchor, TextFieldToTextFieldDistance),
                portTextField.CenterXAnchor.ConstraintEqualTo(usernameTextField.CenterXAnchor),
                portTextField.WidthAnchor.ConstraintEqualTo(TextFieldWidth),
                portTextField.HeightAnchor.ConstraintEqualTo(TextFieldHeight)
            });

            loginButton = new UIButton { Enabled = false, Alpha = 0 };
            loginButton.TitleLabel.Font = Theme.DefaultBoldFont;
            loginButton.TranslatesAutoresizingMaskIntoConstraints = false;
            loginButton.SetTitle(Localization.GetString("login"), UIControlState.Normal);
            loginButton.SetTitleColor(Theme.DarkBlue, UIControlState.Normal);

            containerView.AddSubview(loginButton);
            containerView.AddConstraints(new[]
            {
                loginButton.TopAnchor.ConstraintEqualTo(portTextField.BottomAnchor, LoginButtonToTextFieldDistance),
                loginButton.CenterXAnchor.ConstraintEqualTo(usernameTextField.CenterXAnchor),
                loginButton.WidthAnchor.ConstraintEqualTo(LoginButtonWidth),
                loginButton.HeightAnchor.ConstraintEqualTo(LoginButtonHeight),
            });

            orLabel = new UILabel
            {
                Text = "or",
                Font = Theme.DefaultFont,
                TranslatesAutoresizingMaskIntoConstraints = false,
                TextColor = Theme.DarkBlue,
                TextAlignment = UITextAlignment.Center,
                Alpha = 0
            };
            containerView.AddSubview(orLabel);
            containerView.AddConstraints(new[]
            {
                orLabel.TopAnchor.ConstraintEqualTo(loginButton.BottomAnchor, OrLabelToLoginButtonDistance),
                orLabel.CenterXAnchor.ConstraintEqualTo(usernameTextField.CenterXAnchor),
                orLabel.WidthAnchor.ConstraintEqualTo(LoginButtonWidth),
            });

            loginWithMicrosoftButton = new UIButton { Alpha = 0, Enabled = true };
            loginWithMicrosoftButton.SetImage(UIImage.FromBundle("AzureLogin"), UIControlState.Normal);
            loginWithMicrosoftButton.TranslatesAutoresizingMaskIntoConstraints = false;

            containerView.AddSubview(loginWithMicrosoftButton);
            containerView.AddConstraints(new[]
            {
                loginWithMicrosoftButton.TopAnchor.ConstraintEqualTo(orLabel.BottomAnchor, LoginWithMicrosoftButtonToOrLabelDistance),
                loginWithMicrosoftButton.CenterXAnchor.ConstraintEqualTo(usernameTextField.CenterXAnchor),
                loginWithMicrosoftButton.BottomAnchor.ConstraintEqualTo(containerView.BottomAnchor),
            });
        }

        private void FlipShowHidePasswordImage(bool securedTextShown)
        {
            var imgPasswordShow = UIImage.FromBundle("Password-Show").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            var imgPasswordHide = UIImage.FromBundle("Password-Hide").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);

            var isIos13Plus = UIDevice.CurrentDevice.CheckSystemVersion(13, 0);
            if (isIos13Plus)
            {
                imgPasswordShow.ApplyTintColor(UIColor.White);
                imgPasswordHide.ApplyTintColor(UIColor.White);
            }
            else
            {
                imgPasswordShow = GetTintedImageForiOS12(imgPasswordShow);
                imgPasswordHide = GetTintedImageForiOS12(imgPasswordHide);
            }

            showPasswordButton.SetImage(securedTextShown
              ? imgPasswordShow
              : imgPasswordHide, UIControlState.Normal);
        }

        UIImage GetTintedImageForiOS12(UIImage originalImage)
        {
            var drawRectT = new CGRect(0, 0, originalImage.Size.Width, originalImage.Size.Height);
            var tmpImg = new UIImageView(drawRectT)
            {
                Image = originalImage.ImageWithRenderingMode(UIKit.UIImageRenderingMode.AlwaysTemplate),
                TintColor = UIColor.White
            };

            UIGraphics.BeginImageContextWithOptions(tmpImg.Bounds.Size, tmpImg.Opaque, 0.0f);
            tmpImg.Layer.RenderInContext(UIGraphics.GetCurrentContext());
            var convertedImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            return convertedImage;
        }

        async Task RefreshData()
        {
            try
            {
                retainedConnectionInfo = await authenticator.GetRetainedConnectionInfoAsync();
                if (retainedConnectionInfo != null && string.IsNullOrEmpty(usernameTextField.Text + hostnameTextField.Text))
                {
                    usernameTextField.Text = retainedConnectionInfo.Username;
                    hostnameTextField.Text = retainedConnectionInfo.Hostname;
                    portTextField.Text = retainedConnectionInfo.Port.ToString();
                    sslMode = retainedConnectionInfo.SslMode;
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while restoring retained connection info", ex);
            }
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
        {
            return Integration.IsIPad() ? base.GetSupportedInterfaceOrientations() : UIInterfaceOrientationMask.Portrait;
        }

        #endregion

        #region Animation methods

        void StartLogoScaleAnimation()
        {
            if (startLogoScaleAnimationDone)
                return;

            startLogoScaleAnimationDone = true;

            animationView.PlayToProgress(0.5f, (animationFinished) =>
             {
                 animationView.Play();
                 UIView.Animate(0.25, () =>
                 {
                     settingsButton.Alpha = 1;
                     usernameTextField.Alpha = 1;
                     hostnameTextField.Alpha = 1;
                     passwordTextField.Alpha = 1;
                     portTextField.Alpha = 1;
                     loginButton.Alpha = 0.7f;
                     orLabel.Alpha = 1;
                     loginWithMicrosoftButton.Alpha = 1f;
                 });
             });
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
            loginWithMicrosoftButton.TouchUpInside += LoginWithMicrosoftButton_TouchUpInside;
            showPasswordButton.TouchUpInside += ShowPasswordButton_TouchUpInside;
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
            loginWithMicrosoftButton.TouchUpInside -= LoginWithMicrosoftButton_TouchUpInside;
            showPasswordButton.TouchUpInside -= ShowPasswordButton_TouchUpInside;
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

        void ShowPasswordButton_TouchUpInside(object sender, EventArgs e)
        {
            var updatedIsPassword = !passwordTextField.SecureTextEntry;
            passwordTextField.SecureTextEntry = updatedIsPassword;
            FlipShowHidePasswordImage(updatedIsPassword);
        }

        #endregion

        #region Actions

        private async void LoginWithMicrosoftButton_TouchUpInside(object sender, EventArgs e)
        {
            loginWithMicrosoftButton.TouchUpInside -= LoginWithMicrosoftButton_TouchUpInside;

            usernameTextField.ResignFirstResponder();
            passwordTextField.ResignFirstResponder();
            hostnameTextField.ResignFirstResponder();
            portTextField.ResignFirstResponder();

            CancellationToken token;

            var hapticGenerator = new UINotificationFeedbackGenerator();
            hapticGenerator.Prepare();

            CommonConfig.Logger.Info($"Attempting login with Azure...");

            try
            {
                microsoftAuthService = new MicrosoftAuthService();
                await microsoftAuthService.Authenticate(this);

                var azureUser = await microsoftAuthService.GetAzureUser();
                var endpointList = await microsoftAuthService.GetAzureEndpointInfoList();

                if (!endpointList.Any())
                    throw new Exception("No connection info was found on Azure");

                AzureEndpointInfo endpointInfo = null;

                if (endpointList.Count > 1)
                {
                    var cInfoNamesList = endpointList.Select(c => c.Name).ToArray();
                    var index = await Dialogs.ShowListActionSheetWithTitleAsync(this, cInfoNamesList, loginWithMicrosoftButton, "Select system to connect to");
                    if (index == -1)
                        return;

                    endpointInfo = endpointList[index];
                }
                else
                    endpointInfo = endpointList.First();

                //We assume that all the connection details are correct (no need to validate or confirm hostname, port, SSL)
                var azureUserId = azureUser.Id;
                var hostname = endpointInfo.Hostname;
                var port = endpointInfo.Port > 0 ? endpointInfo.Port.ToString() : string.Empty;
                var sslMode = endpointInfo.UseSsl ? SslMode.On : SslMode.Off;

                CommonConfig.Logger.Info($"Logging in with Azure Id... [azureUserId={azureUserId}, hostname={hostname}, port={port}, ssl={sslMode}]");

                cts = new CancellationTokenSource();
                token = cts.Token;

                dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("logging_in___"), OnCancelLogin);

                CommonConfig.Logger.Info("Authenticating with Azure Id...");

                var ci = await authenticator.AuthenticateWithAzureAsync(azureUser, sslMode, hostname, port, token);

                await InitializeApplication(ci, token);
            }
            catch (Exception ex)
            {
                await ManageLoginException(ex, token, true);

                loginWithMicrosoftButton.TouchUpInside += LoginWithMicrosoftButton_TouchUpInside;
            }
        }

        async void LoginButton_TouchUpInside(object sender, EventArgs e)
        {
            loginButton.TouchUpInside -= LoginButton_TouchUpInside;

            CommonConfig.Logger.Info($"Attempting login...");

            var hapticGenerator = new UINotificationFeedbackGenerator();
            hapticGenerator.Prepare();

            CancellationToken token;

            try
            {
                var username = usernameTextField.Text.TrimEnd();
                var password = passwordTextField.Text.TrimEnd();
                var hostname = hostnameTextField.Text.TrimEnd();
                var port = portTextField.Text.TrimEnd();

                var inputsAreValid = await ValidateInputs(username, password, hostname, port);
                if (!inputsAreValid)
                {
                    loginButton.TouchUpInside += LoginButton_TouchUpInside;
                    return;
                }

                var sslModeConfirmed = await ConfirmSSLMode(sslMode);
                if (!sslModeConfirmed)
                {
                    loginButton.TouchUpInside += LoginButton_TouchUpInside;
                    return;
                }

                CommonConfig.Logger.Info($"Logging in... [username={username}, hostname={hostname}, port={port}, ssl={sslMode}]");

                usernameTextField.ResignFirstResponder();
                passwordTextField.ResignFirstResponder();
                hostnameTextField.ResignFirstResponder();
                portTextField.ResignFirstResponder();

                cts = new CancellationTokenSource();
                token = cts.Token;

                dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("logging_in___"), OnCancelLogin);

                CommonConfig.Logger.Info("Authenticating...");


                var ci = await authenticator.AuthenticateAsync(username, password, sslMode, hostname, port, token);

                await InitializeApplication(ci, token);
            }
            catch (Exception ex)
            {
                await ManageLoginException(ex, token, false);

                usernameTextField.BecomeFirstResponder();

                loginButton.TouchUpInside += LoginButton_TouchUpInside;
            }
        }

        async Task ManageLoginException(Exception ex, CancellationToken token, bool loginFromAzure)
        {
            var hapticGenerator = new UINotificationFeedbackGenerator();
            hapticGenerator.Prepare();

            if (token.IsCancellationRequested)
                return;

            dismissAction?.Invoke();

            CommonConfig.Logger.Error("Log in failed - exception", ex);

            if (ex.InnerException != null)
                CommonConfig.Logger.Error("Log in failed - inner exception", ex.InnerException);

            if (Dialogs.IsAccessDisabled(ex))
                await Dialogs.ShowConfirmAlertAsync(this, Localization.GetString("login_failed"), Localization.GetString("login_failed_access_disabled"));
            else if (IsAcountLocked(ex))
                await Dialogs.ShowConfirmAlertAsync(this, Localization.GetString("login_failed"), Localization.GetString("login_failed_account_locked"));
            else
                await Dialogs.ShowConfirmAlertAsync(this, Localization.GetString("login_failed"), loginFromAzure ? Localization.GetString("login_failed_azure_desc") : Localization.GetString("login_failed_desc"));

            hapticGenerator.NotificationOccurred(UINotificationFeedbackType.Error);
        }

        async Task<bool> ConfirmSSLMode(SslMode sslMode)
        {
            var hapticGenerator = new UINotificationFeedbackGenerator();
            hapticGenerator.Prepare();

            if (sslMode == SslMode.Off)
            {
                hapticGenerator.NotificationOccurred(UINotificationFeedbackType.Warning);

                if (!await Dialogs.ShowYesNoAlertAsync(this, Localization.GetString("warning"), Localization.GetString("warning_ssl_off"), Localization.GetString("continue"), Localization.GetString("cancel")))
                {
                    loginButton.TouchUpInside += LoginButton_TouchUpInside;
                    return false;
                }
            }
          
            return true;
        }

        async Task<bool> ValidateInputs(string username, string password, string hostname, string port)
        {
            var hapticGenerator = new UINotificationFeedbackGenerator();
            hapticGenerator.Prepare();

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

            return !errors;
        }

        async Task InitializeApplication(ConnectionInfo ci, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                CommonConfig.Logger.Info($"Authentication was cancelled...");
                cts = null;
                return;
            }

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

            await Managers.SystemManager.GetSystemUsersDepartmentsAsync();

            CommonConfig.Logger.Info($"Starting services...");
            Services.DocumentsUploadService.Start();
            Services.DocumentPreviewsDownloadService.Start();
            Services.DocumentsDownloadService.Start();

            LocalNotificationsListener.Initialize();

            CommonConfig.Logger.Info($"Refreshing reachability status...");
            await CommonConfig.Reachability.Refresh();

            CommonConfig.Logger.Info($"Registering {nameof(ReachabilityReceiver)}...");
            PlatformConfig.ReachabilityReceiver.Register();

            using var loggerFactory = new LoggerFactory().AddSentry(o =>
            {
                o.DiagnosticLevel = SentryLevel.Debug;
                o.Dsn = "https://7005b8e24f4b45d68b5c1d789da957ed@o588553.ingest.sentry.io/5779906";
                o.Release = $"{CommonConfig.DeviceInfoProvider.GetAppVersionString()}";
                o.MinimumEventLevel = Microsoft.Extensions.Logging.LogLevel.Information;
                o.ConfigureScope(s => {
                    s.SetTag("RootScope", "sent with all events");
                    s.SetTag("DeviceName", ci.FriendlyDeviceName);
                    s.SetTag("ServerName", $"{ci.Hostname}:{ci.Port}");
                    s.SetTag("SslEnabled", $"{ci.SslMode}");
                    s.User = new User { Username = ci.Username };
                });
            });
            CommonConfig.Sentry = loggerFactory.CreateLogger("base");

            CommonConfig.Logger.Info($"Logged in - will present {nameof(AbstractMainViewController)}");

            dismissAction?.Invoke();

            TryRequestAuthorization();

            CommonConfig.UsageAnalytics.SetUserProperty(UserProperty.Hostname, ci.Hostname);
            CommonConfig.UsageAnalytics.SetUserProperty(UserProperty.SSL, ci.SslMode.ToString());

            if (!String.IsNullOrEmpty(ServerConfig.SystemSettings.SystemInfo.CustomerName))
                CommonConfig.UsageAnalytics.SetUserProperty(UserProperty.CustomerName, ServerConfig.SystemSettings.SystemInfo.CustomerName);


            UIViewController vc;
            if (Integration.IsIPad())
            {
                vc = new SplitMainViewController { ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve };
            }             
            else
            {
                vc = new SimpleMainViewController { ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve };
            }
                
            var window = ((AppDelegate)UIApplication.SharedApplication.Delegate).Window;
            UIView.TransitionNotify(window, 0.25, UIViewAnimationOptions.TransitionCrossDissolve, () => window.RootViewController = vc, null);
        }

        private void TryRequestAuthorization()
        {
            try
            {
                UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Alert
                | UNAuthorizationOptions.Badge
                | UNAuthorizationOptions.Sound, (result, error) =>
                {
                    ((AppDelegate)UIApplication.SharedApplication.Delegate)?.OnAuthorizationRequestCompleted(result, error);
                });
            }
            catch (NSErrorException nex)
            {
                CommonConfig.Logger.Error($"Error while requesting authorization", nex);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while requesting authorization", ex);
            }
        }

        #endregion

        #region Notification receivers

        void OnKeyboardWillChangeFrame(object sender, UIKeyboardEventArgs e)
        {
            var loginButtonBottom = View.ConvertRectFromView(loginButton.Frame, containerView).GetMaxY();
            var usernameTextFieldTop = View.ConvertRectFromView(usernameTextField.Frame, containerView).GetMinY();

            var remainingScreenHeight = View.Frame.Height - e.FrameEnd.Height;
            var formHeight = loginButtonBottom - usernameTextFieldTop;
            var distanceFromTopOfTheScreen = (remainingScreenHeight - formHeight) / 1.1f;
            var requiredMovement = usernameTextFieldTop - distanceFromTopOfTheScreen;

            if (requiredMovement > 1)
            {
                UIView.BeginAnimations(string.Empty);
                UIView.SetAnimationDuration(e.AnimationDuration);
                UIView.SetAnimationCurve(e.AnimationCurve);

                containerCenter.Constant += -requiredMovement;
                View.LayoutIfNeeded();

                UIView.CommitAnimations();
            }
        }

        void OnKeyboardWillHide(object sender, UIKeyboardEventArgs e)
        {
            UIView.BeginAnimations(string.Empty);
            UIView.SetAnimationDuration(e.AnimationDuration);
            UIView.SetAnimationCurve(e.AnimationCurve);

            containerCenter.Constant = 0;
            View.LayoutIfNeeded();

            UIView.CommitAnimations();
        }

        void OnCancelLogin()
        {
            dismissAction?.Invoke();
            cts?.Cancel();
            if (loginButton != null)
                loginButton.TouchUpInside += LoginButton_TouchUpInside;
        }

        #endregion

        #region Helper methods

        void ValidateForm()
        {
            var result = true;
            result &= Validator.IsUsernameValid(usernameTextField.Text);
            result &= Validator.IsPasswordValid(passwordTextField.Text);
            result &= Validator.IsHostNameValid(hostnameTextField.Text);

            loginButton.Enabled = result;
            loginButton.Alpha = result ? 1f : 0.7f;
        }

        public static bool IsAcountLocked(Exception ex)
        {
            if (ex is HttpAppServiceException httpEx)
            {
                var code = httpEx?.Detail?.Code;

                return code == AppServiceFaultCode.PasswordPolicyError;
            }

            return false;
        }

        #endregion
    }
}