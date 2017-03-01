//
// Project: Mark5.Mobile.IOS
// File: ProgressHUD.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.Linq;
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

        public const string DidReceiveTouchEventNotification = "SVProgressHUDDidReceiveTouchEventNotification";
        public const string DidTouchDownInsideNotification = "SVProgressHUDDidTouchDownInsideNotification";
        public const string WillDisappearNotification = "SVProgressHUDWillDisappearNotification";
        public const string DidDisappearNotification = "SVProgressHUDDidDisappearNotification";
        public const string WillAppearNotification = "SVProgressHUDWillAppearNotification";
        public const string DidAppearNotification = "SVProgressHUDDidAppearNotification";

        public const string StatusUserInfoKey = "SVProgressHUDStatusUserInfoKey";

        const float ParallaxDepthPoints = 10f;
        const float UndefinedProgress = -1;
        const float DefaultAnimationDuration = 0.15f;
        const float VerticalSpacing = 12f;
        const float HorizontalSpacing = 12f;
        const float LabelSpacing = 8f;

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

        public static ProgressHUD Instance { get; private set; }

        public static void Initialize()
        {
#if SV_APP_EXTENSIONS
            Instance = new ProgressHUD(UIApplication.SharedApplication.Delegate.GetWindow().Bounds);
#else
            Instance = new ProgressHUD(UIScreen.MainScreen.Bounds);
#endif
        }

        NSTimer fadeOutTimer;
        UIControl controlView;
        UIView backgroundView;
        RadialGradientLayer backroundRadialGradientLayer;
        UIVisualEffectView hudView;
        UIVisualEffectView hudVibrancyView;
        UILabel statusLabel;
        UIImageView imageView;

        UIView indefiniteAnimatedView;
        ProgressAnimatedView ringView;
        ProgressAnimatedView backgroundRingView;

        float progress;
        int activityCount;

        float visibleKeyboardHeight;
        UIWindow fronWindow;

        ProgressHUD(CGRect frame)
            : base(frame)
        {
            UserInteractionEnabled = false;
        }

        #region Show methods

        public void Show(string status = null, float progress = UndefinedProgress)
        {
            var weakThis = new WeakReference<ProgressHUD>(this);
            NSOperationQueue.MainQueue.AddOperation(() =>
            {
                ProgressHUD strongThis;
                if (!weakThis.TryGetTarget(out strongThis)) return;

                strongThis.UpdateViewHierarchy();

                strongThis.imageView.Hidden = true;
                strongThis.imageView.Image = null;

                if (strongThis.fadeOutTimer != null)
                    strongThis.activityCount = 0;
                strongThis.fadeOutTimer = null;

                strongThis.statusLabel.Text = status;
                strongThis.progress = progress;

                if (progress >= 0)
                {
                    strongThis.CancelIndefiniteAnimatedViewAnimation();

                    if (strongThis.ringView.Superview == null)
                        strongThis.hudVibrancyView.ContentView.AddSubview(strongThis.ringView);

                    if (strongThis.backgroundRingView.Superview == null)
                        strongThis.hudVibrancyView.ContentView.AddSubview(strongThis.backgroundRingView);

                    CATransaction.Begin();
                    CATransaction.DisableActions = true;
                    strongThis.ringView.StrokeEnd = progress;
                    CATransaction.Commit();

                    if (progress <= 0)
                        strongThis.activityCount++;
                }
                else
                {
                    strongThis.CancelRingLayerAnimation();

                    strongThis.hudVibrancyView.ContentView.AddSubview(strongThis.indefiniteAnimatedView);

                    if (strongThis.indefiniteAnimatedView.RespondsToSelector(new Selector("startAnimating")))
                        strongThis.indefiniteAnimatedView.PerformSelector(new Selector("startAnimating"));

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
                strongThis.imageView.TintColor = tintColor;
                strongThis.imageView.Image = tintedImage;
                strongThis.imageView.Hidden = false;

                strongThis.statusLabel.Text = status;

                strongThis.ShowInternal();

                var duration = GetDisplayDurationForString(status);

                strongThis.fadeOutTimer = NSTimer.CreateTimer(duration, nsTimer => Dismiss());
                NSRunLoop.Main.AddTimer(strongThis.fadeOutTimer, NSRunLoopMode.Common);
            });
        }

        void ShowInternal()
        {
            UpdateHudFrame();
            PositionHud(null);

            controlView.UserInteractionEnabled = DefaultMaskType != MaskType.None;

#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
            if (hudView.ContentView.Alpha != 1.0f)
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator
            {
                NSNotificationCenter.DefaultCenter.PostNotificationName(WillAppearNotification, this, GetNotificationUserInfo());
                hudView.Transform = CGAffineTransform.Scale(hudView.Transform, 1.3f, 1.3f);
                if (DefaultStyle != Style.Custom)
                    AddBlur();

                Action animation = () =>
                {
                    hudView.Transform = CGAffineTransform.Scale(hudView.Transform, 1 / 1.3f, 1 / 1.3f);

                    if (DefaultStyle != Style.Custom)
                        AddBlur();
                    else
                        hudView.Alpha = 1f;

                    hudView.ContentView.Alpha = 1f;
                    backgroundView.Alpha = 1f;
                };

                UICompletionHandler completion = completed =>
                {
#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                    if (hudView.ContentView.Alpha == 1.0f)
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

        void Dismiss(double delay = 0, Action completionHandler = null)
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
                    strongThis.hudView.Transform = CGAffineTransform.Scale(strongThis.hudView.Transform, 1 / 1.3f, 1 / 1.3f);

                    if (DefaultStyle == Style.Custom)
                        strongThis.hudView.Alpha = 0f;
                    else
                    {
                        strongThis.hudView.Effect = null;
                        strongThis.hudVibrancyView = null;
                    }

                    strongThis.hudView.ContentView.Alpha = 0f;
                    strongThis.backgroundView.Alpha = 0f;
                };

                UICompletionHandler completion = completed =>
                {
#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                    if (strongThis.hudView.ContentView.Alpha == 0.0f)
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator
                    {
                        strongThis.controlView.RemoveFromSuperview();
                        strongThis.backgroundView.RemoveFromSuperview();
                        strongThis.hudView.RemoveFromSuperview();
                        strongThis.RemoveFromSuperview();

                        strongThis.progress = 0f;
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
            if (controlView.Superview == null)
                if (ContainerView != null)
                    ContainerView.AddSubview(controlView);
                else
#if !SV_APP_EXTENSIONS
                    GetFrontWindow().AddSubview(controlView);
#else
                    if (ViewForExtension != null)
                        ViewForExtension.AddSubview(controlView);
#endif
            else
                controlView.Superview.BringSubviewToFront(controlView);

            if (Superview == null)
                controlView.AddSubview(this);
        }

        void UpdateHudFrame()
        {
            throw new NotImplementedException();
        }

        [Export("positionHud:")]
        void PositionHud(object p)
        {
            throw new NotImplementedException();
        }

        void AddBlur()
        {
            var blurEffectStyle = DefaultStyle == Style.Dark ? UIBlurEffectStyle.Dark : UIBlurEffectStyle.ExtraLight;
            var blurEffect = UIBlurEffect.FromStyle(blurEffectStyle);

            hudView.Effect = blurEffect;
            hudVibrancyView.Effect = UIVibrancyEffect.FromBlurEffect(blurEffect);
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

        void CancelRingLayerAnimation()
        {
            CATransaction.Begin();
            CATransaction.DisableActions = true;
            hudView.Layer.RemoveAllAnimations();
            ringView.StrokeEnd = 0f;
            CATransaction.Commit();

            ringView.RemoveFromSuperview();
            backgroundRingView.RemoveFromSuperview();
        }

        void CancelIndefiniteAnimatedViewAnimation()
        {
            if (indefiniteAnimatedView.RespondsToSelector(new Selector("stopAnimating")))
                indefiniteAnimatedView.PerformSelector(new Selector("stopAnimating"));

            indefiniteAnimatedView.RemoveFromSuperview();
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

                if (windowOnMainScreen && windowVisible && windowLevelSupported)
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

        double GetDisplayDurationForString(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return MinimumDismissInterval;

            var min = Math.Max(str.Length * 0.06 + 5, MinimumDismissInterval);
            return Math.Min(min, MaximumDismissInterval);
        }

        NSDictionary GetNotificationUserInfo()
        {
            return string.IsNullOrWhiteSpace(statusLabel.Text) ? null : NSDictionary.FromObjectAndKey(FromObject(statusLabel.Text), FromObject(StatusUserInfoKey));
        }

        #endregion

    }
}
