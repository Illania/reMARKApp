//
// Project: Mark5.Mobile.IOS
// File: ProgressHUD.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.Linq;
using System.Threading;
using CoreAnimation;
using CoreFoundation;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace SVProgressHUD
{

    public class ProgressHUD : UIView
    {

        #region Constants

        public const string DidReceiveTouchEventNotification = "ProgressHUDDidReceiveTouchEventNotification";
        public const string DidTouchDownInsideNotification = "ProgressHUDDidTouchDownInsideNotification";
        public const string WillDisappearNotification = "ProgressHUDWillDisappearNotification";
        public const string DidDisappearNotification = "ProgressHUDDidDisappearNotification";
        public const string WillAppearNotification = "ProgressHUDWillAppearNotification";
        public const string DidAppearNotification = "ProgressHUDDidAppearNotification";

        public const string StatusUserInfoKey = "ProgressHUDStatusUserInfoKey";

        const float ParallaxDepthPoints = 10f;
        const float UndefinedProgress = -1;
        const float DefaultAnimationDuration = 0.15f;
        const float VerticalSpacing = 14f;
        const float HorizontalSpacing = 14f;
        const float LabelSpacing = 8f;

        #endregion

        #region Defaults

        public static Style DefaultStyle { get; set; } = Style.Light;
        public static MaskType DefaultMaskType { get; set; } = MaskType.Black;
        public static AnimationType DefaultAnimationType { get; set; } = AnimationType.Flat;
        public static UIView ContainerView { get; set; } = null;
        public static CGSize MinimumSize { get; set; } = CGSize.Empty;
        public static float RingThickness { get; set; } = 2f;
        public static float RingRadius { get; set; } = 18f;
        public static float RingNoTextRadius { get; set; } = 24f;
        public static float CornerRadius { get; set; } = 18f;
        public static UIFont Font { get; set; } = UIFont.PreferredSubheadline;
        public static UIColor ForegroundColor { get; set; } = UIColor.White;
        public static new UIColor BackgroundColor { get; set; } = UIColor.Black;
        public static UIColor BackgroundLayerColor { get; set; } = UIColor.FromWhiteAlpha(0f, 0.4f);
        public static UIImage InfoImage { get; set; } = UIImage.FromBundle("info.png");
        public static UIImage SuccessImage { get; set; } = UIImage.FromBundle("success.png");
        public static UIImage ErrorImage { get; set; } = UIImage.FromBundle("error.png");
        public static UIView ViewForExtension { get; set; } = null;
        public static double MinimumDismissInterval { get; set; } = 5f;
        public static double MaximumDismissInterval { get; set; } = float.MaxValue;
        public static UIOffset CenterOffset { get; set; } = UIOffset.Zero;
        public static double FadeInAnimationDuration { get; set; } = 0.15d;
        public static double FadeOutAnimationDuration { get; set; } = 0.15d;
        public static nfloat MaxSupportedWindowLevel { get; set; } = UIWindowLevel.Normal;

        #endregion

        #region Initialization

        static Lazy<ProgressHUD> _instanceLazy;
        public static ProgressHUD Instance
        {
            get
            {
                if (_instanceLazy == null)
                    throw new InvalidOperationException("ProgressHUD was not initialized yet.");
                return _instanceLazy.Value;
            }
        }

        public static void Initialize()
        {
            if (_instanceLazy != null)
                throw new InvalidOperationException("ProgressHUD was already initialized.");

#if !SV_APP_EXTENSIONS
            _instanceLazy = new Lazy<ProgressHUD>(() => { return new ProgressHUD(UIApplication.SharedApplication.Delegate.GetWindow().Bounds); }, LazyThreadSafetyMode.ExecutionAndPublication);
#else
            _instanceLazy = new Lazy<ProgressHUD>(() => { return new ProgressHUD(UIScreen.MainScreen.Bounds); }, LazyThreadSafetyMode.ExecutionAndPublication);
#endif
        }

        #endregion

        #region Public properties

        public bool Visible
        {
            get
            {
                return HudView.ContentView.Alpha > 0f;
            }
        }

        #endregion

        #region Private properties

        // BEGIN: These fields should not be used
        // anywhere else then in private properties getters/setters
        NSTimer _fadeOutTimer;
        UIControl _controlView;
        UIView _backgroundView;
        RadialGradientLayer _backgroundRadialGradientLayer;
        UIVisualEffectView _hudView;
        UIVisualEffectView _hudVibrancyView;
        UILabel _statusLabel;
        UIImageView _imageView;
        UIView _indefiniteAnimatedView;
        ProgressAnimatedView _ringView;
        ProgressAnimatedView _backgroundRingView;
        // END

        float progress;
        int activityCount;

        NSTimer FadeOutTimer
        {
            get { return _fadeOutTimer; }
            set
            {
                _fadeOutTimer?.Invalidate();

                if (value != null)
                    _fadeOutTimer = value;
            }
        }

        UIControl ControlView
        {
            get
            {
                if (_controlView == null)
                {
                    _controlView = new UIControl
                    {
                        AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight,
                        BackgroundColor = UIColor.Clear
                    };
                    _controlView.AddTarget(this, new Selector("controlViewDidReceiveTouchEvent:forEvent:"), UIControlEvent.TouchDown);
                }

#if !SV_APP_EXTENSIONS
                _controlView.Frame = UIApplication.SharedApplication.Delegate.GetWindow().Bounds;
#else
                _controlView.Frame = UIScreen.MainScreen.Bounds;
#endif

                return _controlView;
            }
        }

        UIView BackgroundView
        {
            get
            {
                if (_backgroundView == null)
                {
                    _backgroundView = new UIView
                    {
                        AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight
                    };
                }

                if (_backgroundView.Superview == null)
                    InsertSubviewBelow(_backgroundView, HudView);

                switch (DefaultMaskType)
                {
                    case MaskType.Custom:
                    case MaskType.Black:
                        if (_backgroundRadialGradientLayer != null && _backgroundRadialGradientLayer.SuperLayer != null)
                            _backgroundRadialGradientLayer.RemoveFromSuperLayer();
                        _backgroundView.BackgroundColor = DefaultMaskType == MaskType.Custom ? BackgroundLayerColor : UIColor.FromWhiteAlpha(0f, 0.4f);
                        break;
                    case MaskType.Gradient:
                        if (_backgroundRadialGradientLayer == null)
                            _backgroundRadialGradientLayer = new RadialGradientLayer();
                        if (_backgroundRadialGradientLayer.SuperLayer == null)
                            _backgroundView.Layer.InsertSublayer(_backgroundRadialGradientLayer, 0);
                        break;
                    default:
                        _backgroundView.BackgroundColor = UIColor.Clear;
                        break;
                }

                if (_backgroundView != null)
                    _backgroundView.Frame = Bounds;

                if (_backgroundRadialGradientLayer != null)
                {
                    _backgroundRadialGradientLayer.Frame = Bounds;

                    var gradientCenter = Center;
                    gradientCenter.Y = (Bounds.Size.Height - GetVisibleKeyboardHeight()) / 2;
                    _backgroundRadialGradientLayer.GradientCenter = gradientCenter;
                }

                return _backgroundView;
            }
        }

        UIVisualEffectView HudView
        {
            get
            {
                if (_hudView == null)
                {
                    _hudView = new UIVisualEffectView
                    {
                        AutoresizingMask = UIViewAutoresizing.FlexibleBottomMargin | UIViewAutoresizing.FlexibleTopMargin | UIViewAutoresizing.FlexibleRightMargin | UIViewAutoresizing.FlexibleLeftMargin
                    };
                    _hudView.Layer.MasksToBounds = true;
                }

                if (_hudView.Superview == null)
                    AddSubview(_hudView);

                _hudView.Layer.CornerRadius = CornerRadius;
                _hudView.BackgroundColor = GetBackgroundColorForStyle();

                return _hudView;
            }
        }

        UIVisualEffectView HudVibrancyView
        {
            get
            {
                if (_hudVibrancyView == null)
                {
                    _hudVibrancyView = new UIVisualEffectView
                    {
                        AutoresizingMask = UIViewAutoresizing.FlexibleBottomMargin | UIViewAutoresizing.FlexibleTopMargin | UIViewAutoresizing.FlexibleRightMargin | UIViewAutoresizing.FlexibleLeftMargin
                    };
                    _hudVibrancyView.Layer.MasksToBounds = true;
                }

                if (_hudVibrancyView.Superview == null)
                    HudView.ContentView.AddSubview(_hudVibrancyView);

                return _hudVibrancyView;
            }
            set { _hudVibrancyView = value; }
        }

        UILabel StatusLabel
        {
            get
            {
                if (_statusLabel == null)
                    _statusLabel = new UILabel
                    {
                        BackgroundColor = UIColor.Clear,
                        AdjustsFontSizeToFitWidth = true,
                        TextAlignment = UITextAlignment.Center,
                        BaselineAdjustment = UIBaselineAdjustment.AlignCenters,
                        Lines = 0
                    };

                if (_statusLabel.Superview == null)
                    HudVibrancyView.ContentView.AddSubview(_statusLabel);

                _statusLabel.TextColor = GetForegroundColorForStyle();
                _statusLabel.Font = Font;

                return _statusLabel;
            }
        }

        UIImageView ImageView
        {
            get
            {
                if (_imageView == null)
                    _imageView = new UIImageView(new CGRect(0f, 0f, 28f, 28f));

                if (_imageView.Superview == null)
                    HudVibrancyView.ContentView.AddSubview(_imageView);

                return _imageView;
            }
        }

        UIView IndefiniteAnimatedView
        {
            get
            {
                if (DefaultAnimationType == AnimationType.Flat)
                {
                    if (_indefiniteAnimatedView != null && !_indefiniteAnimatedView.IsKindOfClass(new Class("IndefiniteAnimatedView")))
                    {
                        _indefiniteAnimatedView.RemoveFromSuperview();
                        _indefiniteAnimatedView = null;
                    }

                    if (_indefiniteAnimatedView == null)
                        _indefiniteAnimatedView = new IndefiniteAnimatedView();

                    var iav = (IndefiniteAnimatedView)_indefiniteAnimatedView;
                    iav.StrokeColor = GetForegroundColorForStyle();
                    iav.StrokeThickness = RingThickness;
                    iav.Radius = string.IsNullOrWhiteSpace(StatusLabel.Text) ? RingRadius : RingNoTextRadius;
                }
                else
                {
                    if (_indefiniteAnimatedView != null && !_indefiniteAnimatedView.IsKindOfClass(new Class("UIActivityIndicatorView")))
                    {
                        _indefiniteAnimatedView.RemoveFromSuperview();
                        _indefiniteAnimatedView = null;
                    }

                    if (_indefiniteAnimatedView == null)
                        _indefiniteAnimatedView = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge);

                    var aiv = (UIActivityIndicatorView)_indefiniteAnimatedView;
                    aiv.Color = GetForegroundColorForStyle();
                }

                _indefiniteAnimatedView.SizeToFit();

                return _indefiniteAnimatedView;
            }
        }

        ProgressAnimatedView RingView
        {
            get
            {
                if (_ringView == null)
                    _ringView = new ProgressAnimatedView();

                _ringView.StrokeColor = GetForegroundColorForStyle();
                _ringView.StrokeThickness = RingThickness;
                _ringView.Radius = string.IsNullOrWhiteSpace(StatusLabel.Text) ? RingRadius : RingNoTextRadius;

                return _ringView;
            }
        }

        ProgressAnimatedView BackgroundRingView
        {
            get
            {
                if (_backgroundRingView == null)
                    _backgroundRingView = new ProgressAnimatedView();

                _backgroundRingView.StrokeColor = GetForegroundColorForStyle().ColorWithAlpha(0.1f);
                _backgroundRingView.StrokeThickness = RingThickness;
                _backgroundRingView.Radius = string.IsNullOrWhiteSpace(StatusLabel.Text) ? RingRadius : RingNoTextRadius;

                return _backgroundRingView;
            }
        }

        #endregion

        #region Private constructors

        ProgressHUD(CGRect frame)
            : base(frame)
        {
            UserInteractionEnabled = false;
            activityCount = 0;

            HudView.ContentView.Alpha = 0f;
            BackgroundView.Alpha = 0f;
        }

        #endregion

        #region Show methods

        public void Show(string status = null, float progress = UndefinedProgress)
        {
            var weakThis = new WeakReference<ProgressHUD>(this);
            NSOperationQueue.MainQueue.AddOperation(() =>
            {
                ProgressHUD strongThis;
                if (!weakThis.TryGetTarget(out strongThis)) return;

                strongThis.UpdateViewHierarchy();

                strongThis.ImageView.Hidden = true;
                strongThis.ImageView.Image = null;

                if (strongThis.FadeOutTimer != null)
                    strongThis.activityCount = 0;
                strongThis.FadeOutTimer = null;

                strongThis.StatusLabel.Text = status;
                strongThis.progress = progress;

                if (progress >= 0)
                {
                    strongThis.CancelIndefiniteAnimatedViewAnimation();

                    if (strongThis.RingView.Superview == null)
                        strongThis.HudVibrancyView.ContentView.AddSubview(strongThis.RingView);

                    if (strongThis.BackgroundRingView.Superview == null)
                        strongThis.HudVibrancyView.ContentView.AddSubview(strongThis.BackgroundRingView);

                    CATransaction.Begin();
                    CATransaction.DisableActions = true;
                    strongThis.RingView.StrokeEnd = progress;
                    CATransaction.Commit();

                    if (progress <= 0)
                        strongThis.activityCount++;
                }
                else
                {
                    strongThis.CancelRingLayerAnimation();

                    strongThis.HudVibrancyView.ContentView.AddSubview(strongThis.IndefiniteAnimatedView);

                    if (strongThis.IndefiniteAnimatedView.RespondsToSelector(new Selector("startAnimating")))
                        strongThis.IndefiniteAnimatedView.PerformSelector(new Selector("startAnimating"));

                    strongThis.activityCount++;
                }

                ShowInternal();
            });
        }

        public void ShowInfo(string status = null) => ShowImage(InfoImage, status);
        public void ShowSuccess(string status = null) => ShowImage(SuccessImage, status);
        public void ShowError(string status = null) => ShowImage(ErrorImage, status);

        public void ShowImage(UIImage image, string status = null)
        {
            var weakThis = new WeakReference<ProgressHUD>(this);
            NSOperationQueue.MainQueue.AddOperation(() =>
            {
                ProgressHUD strongThis;
                if (!weakThis.TryGetTarget(out strongThis)) return;

                strongThis.UpdateViewHierarchy();

                strongThis.progress = UndefinedProgress;
                strongThis.CancelRingLayerAnimation();
                strongThis.CancelIndefiniteAnimatedViewAnimation();

                var tintColor = strongThis.GetForegroundColorForStyle();
                var tintedImage = image;
                if (image.RenderingMode != UIImageRenderingMode.AlwaysTemplate)
                    tintedImage = image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                strongThis.ImageView.TintColor = tintColor;
                strongThis.ImageView.Image = tintedImage;
                strongThis.ImageView.Hidden = false;

                strongThis.StatusLabel.Text = status;

                strongThis.ShowInternal();

                var duration = GetDisplayDurationForString(status);

                strongThis.FadeOutTimer = NSTimer.CreateTimer(duration, nsTimer => Dismiss());
                NSRunLoop.Main.AddTimer(strongThis.FadeOutTimer, NSRunLoopMode.Common);
            });
        }

        void ShowInternal()
        {
            UpdateHudFrame();
            PositionHud(null);

            ControlView.UserInteractionEnabled = DefaultMaskType != MaskType.None;

#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
            if (HudView.ContentView.Alpha != 1.0f)
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator
            {
                NSNotificationCenter.DefaultCenter.PostNotificationName(WillAppearNotification, this, GetNotificationUserInfo());
                HudView.Transform = CGAffineTransform.Scale(HudView.Transform, 1.3f, 1.3f);

                Action animation = () =>
                {
                    HudView.Transform = CGAffineTransform.Scale(HudView.Transform, 1 / 1.3f, 1 / 1.3f);

                    if (DefaultStyle != Style.Custom)
                        AddBlur();
                    else
                        HudView.Alpha = 1f;

                    HudView.ContentView.Alpha = 1f;
                    BackgroundView.Alpha = 1f;
                };

                UICompletionHandler completion = completed =>
                {
#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                    if (HudView.ContentView.Alpha == 1.0f)
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator
                    {
                        RegisterNotifications();

                        NSNotificationCenter.DefaultCenter.PostNotificationName(DidAppearNotification, this, GetNotificationUserInfo());
                    }

                };

                if (FadeInAnimationDuration > 0)
                    AnimateNotify(FadeInAnimationDuration,
                                  0d,
                                  UIViewAnimationOptions.AllowUserInteraction | UIViewAnimationOptions.CurveEaseIn | UIViewAnimationOptions.BeginFromCurrentState,
                                  animation,
                                  completion);
                else
                {
                    animation();
                    completion(true);
                }

                SetNeedsDisplay();
            }
        }

        #endregion

        #region Dismiss methods

        public void Dismiss() => Dismiss(0);

        public void Dismiss(double delay, Action completionHandler = null)
        {
            var weakThis = new WeakReference<ProgressHUD>(this);
            NSOperationQueue.MainQueue.AddOperation(() =>
            {
                ProgressHUD strongThis;
                if (!weakThis.TryGetTarget(out strongThis))
                {
                    if (completionHandler != null)
                        completionHandler();
                    return;
                }

                NSNotificationCenter.DefaultCenter.PostNotificationName(WillDisappearNotification, this, GetNotificationUserInfo());

                strongThis.activityCount = 0;

                Action animation = () =>
                {
                    strongThis.HudView.Transform = CGAffineTransform.Scale(strongThis.HudView.Transform, 1 / 1.3f, 1 / 1.3f);

                    if (DefaultStyle == Style.Custom)
                        strongThis.HudView.Alpha = 0f;
                    else
                    {
                        strongThis.HudView.Effect = null;
                        strongThis.HudVibrancyView = null;
                    }

                    strongThis.HudView.ContentView.Alpha = 0f;
                    strongThis.BackgroundView.Alpha = 0f;
                };

                UICompletionHandler completion = completed =>
                {
#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                    if (strongThis.HudView.ContentView.Alpha == 0.0f)
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator
                    {
                        strongThis.ControlView.RemoveFromSuperview();
                        strongThis.BackgroundView.RemoveFromSuperview();
                        strongThis.HudView.RemoveFromSuperview();
                        strongThis.RemoveFromSuperview();

                        strongThis.progress = UndefinedProgress;
                        strongThis.CancelRingLayerAnimation();
                        strongThis.CancelIndefiniteAnimatedViewAnimation();

                        NSNotificationCenter.DefaultCenter.RemoveObserver(strongThis);

                        NSNotificationCenter.DefaultCenter.PostNotificationName(DidDisappearNotification, this, GetNotificationUserInfo());

#if !SV_APP_EXTENSIONS
                        var vc = UIApplication.SharedApplication.KeyWindow.RootViewController;
                        vc.SetNeedsStatusBarAppearanceUpdate();
#endif

                        if (completionHandler != null)
                            completionHandler();
                    }
                };

                var dispatchTime = new DispatchTime(DispatchTime.Now, TimeSpan.FromSeconds(delay));
                DispatchQueue.MainQueue.DispatchAfter(dispatchTime, () =>
                {
                    if (FadeOutAnimationDuration > 0)
                        AnimateNotify(FadeOutAnimationDuration,
                                      0d,
                                      UIViewAnimationOptions.AllowUserInteraction | UIViewAnimationOptions.CurveEaseOut | UIViewAnimationOptions.BeginFromCurrentState,
                                      animation,
                                      completion);
                    else
                    {
                        animation();
                        completion(true);
                    }
                });

                strongThis.SetNeedsDisplay();
            });
        }

        #endregion

        #region Helper methods

        void UpdateViewHierarchy()
        {
            if (ControlView.Superview == null)
                if (ContainerView != null)
                    ContainerView.AddSubview(ControlView);
                else
#if !SV_APP_EXTENSIONS
                    GetFrontWindow().AddSubview(ControlView);
#else
                    if (ViewForExtension != null)
                        ViewForExtension.AddSubview(ControlView);
#endif
            else
                ControlView.Superview.BringSubviewToFront(ControlView);

            if (Superview == null)
                ControlView.AddSubview(this);
        }

        void UpdateHudFrame()
        {
            var imageUsed = ImageView.Image != null && !ImageView.Hidden;
            var progressUsed = ImageView.Hidden;

            var labelRect = CGRect.Empty;
            var labelHeight = 0f;
            var labelWidth = 0f;

            if (!string.IsNullOrWhiteSpace(StatusLabel.Text))
            {
                var constraintSize = new CGSize(200f, 300f);
                labelRect = new NSString(StatusLabel.Text).GetBoundingRect(constraintSize,
                                                                               NSStringDrawingOptions.UsesFontLeading | NSStringDrawingOptions.TruncatesLastVisibleLine | NSStringDrawingOptions.UsesLineFragmentOrigin,
                                                                               new UIStringAttributes { Font = StatusLabel.Font },
                                                                               null);
                labelHeight = (float)Math.Ceiling(labelRect.Height);
                labelWidth = (float)Math.Ceiling(labelRect.Width);
            }

            var hudHeight = 0f;
            var hudWidth = 0f;

            var contentHeight = 0f;
            var contentWidth = 0f;

            if (imageUsed || progressUsed)
            {
                contentHeight = (float)(imageUsed ? ImageView.Frame.Height : IndefiniteAnimatedView.Frame.Height);
                contentWidth = (float)(imageUsed ? ImageView.Frame.Width : IndefiniteAnimatedView.Frame.Width);
            }

            hudWidth = HorizontalSpacing + Math.Max(labelWidth, contentWidth) + HorizontalSpacing;
            hudHeight = VerticalSpacing + labelHeight + contentHeight + VerticalSpacing;

            if (!string.IsNullOrWhiteSpace(StatusLabel.Text) && (imageUsed || progressUsed))
                hudHeight += LabelSpacing;

            HudView.Bounds = new CGRect(0f, 0f, Math.Max(MinimumSize.Width, hudWidth), Math.Max(MinimumSize.Height, hudHeight));
            HudVibrancyView.Bounds = new CGRect(0f, 0f, Math.Max(MinimumSize.Width, hudWidth), Math.Max(MinimumSize.Height, hudHeight));
            HudVibrancyView.Frame = new CGRect(0f, 0f, HudVibrancyView.Frame.Width, HudVibrancyView.Frame.Height);

            CATransaction.Begin();
            CATransaction.DisableActions = true;

            float centerY;
            if (!string.IsNullOrWhiteSpace(StatusLabel.Text))
            {
                var yOffset = (float)Math.Max(VerticalSpacing, (MinimumSize.Height - contentHeight - LabelSpacing - labelHeight) / 2f);
                centerY = yOffset + contentHeight / 2f;
            }
            else
            {
                centerY = (float)HudView.Bounds.GetMidY();
            }

            IndefiniteAnimatedView.Center = new CGPoint(HudView.Bounds.GetMidX(), centerY);
#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
            if (progress != UndefinedProgress)
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator
                BackgroundRingView.Center = new CGPoint(HudView.Bounds.GetMidX(), centerY);
            ImageView.Center = new CGPoint(HudView.Bounds.GetMidX(), centerY);

            if (imageUsed || progressUsed)
                centerY = (float)((imageUsed ? ImageView.Frame : IndefiniteAnimatedView.Frame).GetMaxY() + LabelSpacing + labelHeight / 2f);
            else
                centerY = (float)HudView.Bounds.GetMidY();

            StatusLabel.Frame = labelRect;
            StatusLabel.Center = new CGPoint(HudView.Bounds.GetMidX(), centerY);
            StatusLabel.Hidden = string.IsNullOrWhiteSpace(StatusLabel.Text);

            CATransaction.Commit();
        }

        [Export("positionHud:")]
        void PositionHud(NSNotification notification)
        {
            var keyboardHeight = 0f;
            var animationDuration = 0d;

#if !SV_APP_EXTENSIONS
            Frame = UIApplication.SharedApplication.Delegate.GetWindow().Bounds;
            var orientation = UIApplication.SharedApplication.StatusBarOrientation;
#else
            if (ViewForExtension != null)
                Frame = ViewForExtension.Frame;
            else
                Frame = UIScreen.MainScreen.Bounds;
            var orientation = Frame.Width > Frame.Height
                                   ? UIInterfaceOrientation.LandscapeLeft
                                   : UIInterfaceOrientation.Portrait;
#endif

            if (notification != null)
            {
                if (notification.Name == UIKeyboard.WillShowNotification || notification.Name == UIKeyboard.DidShowNotification)
                {
                    var keyboardInfo = notification.UserInfo;
                    var keyboardFrame = ((NSValue)keyboardInfo[UIKeyboard.FrameBeginUserInfoKey]).CGRectValue;
                    animationDuration = ((NSNumber)keyboardInfo[UIKeyboard.AnimationDurationUserInfoKey]).DoubleValue;

                    keyboardHeight = (float)keyboardFrame.Height;
                }
            }
            else
                keyboardHeight = GetVisibleKeyboardHeight();

            var orientationFrame = Bounds;
#if !SV_APP_EXTENSIONS
            var statusBarFrame = UIApplication.SharedApplication.StatusBarFrame;
#else
            var statusBarFrame = CGRect.Empty;
#endif

            UpdateMotionEffectForOrientation(orientation);

            var activeHeight = orientationFrame.Height;
            if (keyboardHeight > 0)
                activeHeight += statusBarFrame.Height * 2;
            activeHeight -= keyboardHeight;

            var posX = orientationFrame.GetMidX();
            var posY = (float)Math.Floor(activeHeight * 0.45f);

            var rotateAngle = 0f;
            var newCenter = new CGPoint(posX, posY);

            if (notification != null)
                AnimateNotify(animationDuration,
                             0f,
                             UIViewAnimationOptions.AllowUserInteraction | UIViewAnimationOptions.BeginFromCurrentState,
                             () =>
                             {
                                 MoveToPoint(newCenter, rotateAngle);
                                 HudView.SetNeedsDisplay();
                             },
                             null);
            else
                MoveToPoint(newCenter, rotateAngle);
        }

        void MoveToPoint(CGPoint newCenter, float angle)
        {
            HudView.Transform = CGAffineTransform.MakeRotation(angle);
            if (ContainerView != null)
                HudView.Center = ContainerView.Center;
            else
                HudView.Center = new CGPoint(newCenter.X + CenterOffset.Horizontal, newCenter.Y + CenterOffset.Vertical);
        }

        void AddBlur()
        {
            var blurEffectStyle = DefaultStyle == Style.Dark ? UIBlurEffectStyle.Dark : UIBlurEffectStyle.ExtraLight;
            var blurEffect = UIBlurEffect.FromStyle(blurEffectStyle);

            HudView.Effect = blurEffect;
            HudVibrancyView.Effect = UIVibrancyEffect.FromBlurEffect(blurEffect);
        }

        void UpdateMotionEffectForOrientation(UIInterfaceOrientation orientation)
        {
            var xEffectType = orientation.IsPortrait()
                                         ? UIInterpolatingMotionEffectType.TiltAlongHorizontalAxis
                                         : UIInterpolatingMotionEffectType.TiltAlongVerticalAxis;
            var yEffectType = orientation.IsPortrait()
                                         ? UIInterpolatingMotionEffectType.TiltAlongVerticalAxis
                                         : UIInterpolatingMotionEffectType.TiltAlongHorizontalAxis;
            UpdateMotionEffectForXMotionEffectType(xEffectType, yEffectType);
        }

        void UpdateMotionEffectForXMotionEffectType(UIInterpolatingMotionEffectType xEffectType, UIInterpolatingMotionEffectType yEffectType)
        {
            var xEffect = new UIInterpolatingMotionEffect("center.x", xEffectType)
            {
                MinimumRelativeValue = FromObject(-ParallaxDepthPoints),
                MaximumRelativeValue = FromObject(ParallaxDepthPoints)
            };

            var yEffect = new UIInterpolatingMotionEffect("center.y", yEffectType)
            {
                MinimumRelativeValue = FromObject(-ParallaxDepthPoints),
                MaximumRelativeValue = FromObject(ParallaxDepthPoints)
            };

            var effectGroup = new UIMotionEffectGroup
            {
                MotionEffects = new[] { xEffect, yEffect }
            };

            HudView.MotionEffects = new UIMotionEffectGroup[0];
            HudView.AddMotionEffect(effectGroup);
        }

        void CancelRingLayerAnimation()
        {
            CATransaction.Begin();
            CATransaction.DisableActions = true;
            HudView.Layer.RemoveAllAnimations();
            RingView.StrokeEnd = 0f;
            CATransaction.Commit();

            RingView.RemoveFromSuperview();
            BackgroundRingView.RemoveFromSuperview();
        }

        void CancelIndefiniteAnimatedViewAnimation()
        {
            if (IndefiniteAnimatedView.RespondsToSelector(new Selector("stopAnimating")))
                IndefiniteAnimatedView.PerformSelector(new Selector("stopAnimating"));

            IndefiniteAnimatedView.RemoveFromSuperview();
        }

        float GetVisibleKeyboardHeight()
        {
#if !SV_APP_EXTENSIONS
            UIWindow keyboardWindow = null;
            foreach (var testWindow in UIApplication.SharedApplication.Windows)
            {
                if (testWindow.Class != new Class("UIWindow"))
                {
                    keyboardWindow = testWindow;
                    break;
                }
            }

            foreach (var possibleKeyboard in keyboardWindow.Subviews)
            {
                if (possibleKeyboard.IsKindOfClass(new Class("UIPeripheralHostView")) || possibleKeyboard.IsKindOfClass(new Class("UIKeyboard")))
                    return (float)possibleKeyboard.Bounds.Height;
                if (possibleKeyboard.IsKindOfClass(new Class("UIInputSetContainerView")))
                {
                    foreach (var possibleKeyboardSubview in possibleKeyboard.Subviews)
                        if (possibleKeyboardSubview.IsKindOfClass(new Class("UIInputSetHostView")))
                            return (float)possibleKeyboardSubview.Bounds.Height;
                }
            }
#endif
            return 0f;
        }

        UIWindow GetFrontWindow()
        {
#if !SV_APP_EXTENSIONS
            var frontToBackWindows = UIApplication.SharedApplication.Windows.ToArray();
            Array.Reverse(frontToBackWindows);

            foreach (var window in frontToBackWindows)
            {
                var windowOnMainScreen = window.Screen == UIScreen.MainScreen;
                var windowVisible = !window.Hidden && window.Alpha > 0f;
                var windowLevelSupported = window.WindowLevel >= UIWindowLevel.Normal && window.WindowLevel <= MaxSupportedWindowLevel;
                var windowsKeyWindow = window.IsKeyWindow;

                if (windowOnMainScreen && windowVisible && windowLevelSupported && windowsKeyWindow)
                    return window;
            }
#endif
            return null;
        }

        UIColor GetForegroundColorForStyle()
        {
            if (DefaultStyle == Style.Light)
                return UIColor.Black;

            if (DefaultStyle == Style.Dark)
                return UIColor.White;

            return ForegroundColor;
        }

        UIColor GetBackgroundColorForStyle()
        {
            return DefaultStyle == Style.Custom ? BackgroundColor : UIColor.Clear;
        }

        double GetDisplayDurationForString(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return MinimumDismissInterval;

            var min = Math.Max(str.Length * 0.06 + 5, MinimumDismissInterval);
            return Math.Min(min, MaximumDismissInterval);
        }

        void RegisterNotifications()
        {
            NSNotificationCenter.DefaultCenter.AddObserver(this, new Selector("positionHud:"), UIApplication.DidChangeStatusBarOrientationNotification, null);
            NSNotificationCenter.DefaultCenter.AddObserver(this, new Selector("positionHud:"), UIApplication.DidBecomeActiveNotification, null);
            NSNotificationCenter.DefaultCenter.AddObserver(this, new Selector("positionHud:"), UIKeyboard.WillShowNotification, null);
            NSNotificationCenter.DefaultCenter.AddObserver(this, new Selector("positionHud:"), UIKeyboard.DidShowNotification, null);
            NSNotificationCenter.DefaultCenter.AddObserver(this, new Selector("positionHud:"), UIKeyboard.WillHideNotification, null);
            NSNotificationCenter.DefaultCenter.AddObserver(this, new Selector("positionHud:"), UIKeyboard.DidHideNotification, null);
        }

        [Export("controlViewDidReceiveTouchEvent:forEvent:")]
        void ControlViewDidReceiveTouchEvent(NSObject sender, UIEvent e)
        {
            NSNotificationCenter.DefaultCenter.PostNotificationName(DidReceiveTouchEventNotification, this, GetNotificationUserInfo());

            var touch = (UITouch)e.AllTouches.AnyObject;
            var touchLocation = touch.LocationInView(this);

            if (HudView.Frame.Contains(touchLocation))
                NSNotificationCenter.DefaultCenter.PostNotificationName(DidTouchDownInsideNotification, this, GetNotificationUserInfo());
        }

        NSDictionary GetNotificationUserInfo()
        {
            return string.IsNullOrWhiteSpace(StatusLabel.Text) ? null : NSDictionary.FromObjectAndKey(FromObject(StatusLabel.Text), FromObject(StatusUserInfoKey));
        }

        #endregion

    }
}
