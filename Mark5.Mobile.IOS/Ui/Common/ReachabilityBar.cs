//
// Project: Mark5.Mobile.IOS
// File: ReachabilityBar.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.Linq;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Services;
using ObjCRuntime;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
{

    public class ReachabilityBar : UILabel
    {

        const float AnimationDuration = 0.25f;
        const float Height = 25.0f;

        readonly UIScrollView scrollView;
        readonly float offset;

        bool registered;
        UIView parent;
        CGRect parentFrame;

        public static void Attach(UIView parent, UIScrollView scrollView = null, float offset = 0f)
        {
            if (parent.Subviews.Any(v => v is ReachabilityBar))
                return;

            parent.AddSubview(new ReachabilityBar(parent, scrollView, offset));
        }

        ReachabilityBar(UIView parent, UIScrollView scrollView, float offset)
        {
            this.scrollView = scrollView;
            this.offset = offset;

            AutoresizingMask = UIViewAutoresizing.None;

            TextAlignment = UITextAlignment.Center;
            Font = Theme.DefaultFont.WithSize(12f);

            AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));
            AddGestureRecognizer(new UILongPressGestureRecognizer(this, new Selector("longPressed:")));
        }

        public override void WillMoveToSuperview(UIView newsuper)
        {
            base.WillMoveToSuperview(newsuper);

            if (parent != null)
                parent.RemoveObserver(this, "frame");

            parent = newsuper;

            if (parent != null)
                parent.AddObserver(this, "frame", NSKeyValueObservingOptions.OldNew, IntPtr.Zero);

            if (newsuper != null)
            {
                if (!registered)
                {
                    registered = true;

                    CommonConfig.ReachabilityService.RefreshingReachability += ReachabilityService_RefreshingReachability;
                    CommonConfig.ReachabilityService.ReachabilityRefreshed += ReachabilityService_ReachabilityRefreshed;
                }
            }
            else
            {
                CommonConfig.ReachabilityService.RefreshingReachability -= ReachabilityService_RefreshingReachability;
                CommonConfig.ReachabilityService.ReachabilityRefreshed -= ReachabilityService_ReachabilityRefreshed;

                Layer.RemoveAllAnimations();
            }
        }

        public override void MovedToSuperview()
        {
            base.MovedToSuperview();

            if (Superview == null)
                return;

            parentFrame = Superview.Frame;

            if (CommonConfig.ReachabilityService.IsCheckingReachability)
                ShowConnecting(false);
            else if (!CommonConfig.ReachabilityService.IsReachable)
                ShowDisconnected(false);
            else
                Hide(false);
        }

        [Export("tapped:")]
        public void Tapped(UILongPressGestureRecognizer recognizer) => CommonConfig.ReachabilityService.Refresh();

        [Export("longPressed:")]
        public void LongPressed(UILongPressGestureRecognizer recognizer)
        {
        }

        void ShowDisconnected() => ShowDisconnected(true);

        void ShowDisconnected(bool animate)
        {
            Action action = () =>
            {
                Layer.BackgroundColor = Theme.Brown.CGColor;
                Text = Localization.GetString("disconnected");
                TextColor = Theme.White;

                Frame = new CGRect(0f, parentFrame.Height - offset - Height, parentFrame.Width, Height);
                Alpha = 1f;

                if (scrollView != null)
                {
                    scrollView.ContentInset = new UIEdgeInsets(scrollView.ContentInset.Top, scrollView.ContentInset.Left, offset + Height, scrollView.ContentInset.Right);
                    scrollView.ScrollIndicatorInsets = new UIEdgeInsets(scrollView.ScrollIndicatorInsets.Top, scrollView.ScrollIndicatorInsets.Left, offset + Height, scrollView.ScrollIndicatorInsets.Right);
                }
            };

            UICompletionHandler completion = finished =>
            {
                UserInteractionEnabled = true;
            };

            if (animate)
                AnimateNotify(AnimationDuration, 0f, UIViewAnimationOptions.CurveEaseIn | UIViewAnimationOptions.BeginFromCurrentState, action, completion);
            else
            {
                action();
                completion(true);
            }
        }

        void ShowConnecting() => ShowConnecting(true);

        void ShowConnecting(bool animate)
        {
            UserInteractionEnabled = false;

            Action action = () =>
            {
                Layer.BackgroundColor = Theme.LightBrown.CGColor;
                Text = Localization.GetString("connecting___");
                TextColor = Theme.White;

                Frame = new CGRect(0f, parentFrame.Height - offset - Height, parentFrame.Width, Height);
                Alpha = 1f;

                if (scrollView != null)
                {
                    scrollView.ContentInset = new UIEdgeInsets(scrollView.ContentInset.Top, scrollView.ContentInset.Left, offset + Height, scrollView.ContentInset.Right);
                    scrollView.ScrollIndicatorInsets = new UIEdgeInsets(scrollView.ScrollIndicatorInsets.Top, scrollView.ScrollIndicatorInsets.Left, offset + Height, scrollView.ScrollIndicatorInsets.Right);
                }
            };

            if (animate)
                AnimateNotify(AnimationDuration, 0f, UIViewAnimationOptions.CurveEaseIn | UIViewAnimationOptions.BeginFromCurrentState, action, null);
            else
                action();
        }

        void Hide() => Hide(true);

        void Hide(bool animate)
        {
            UserInteractionEnabled = false;

            Action action = () =>
            {
                Layer.BackgroundColor = UIColor.Clear.CGColor;
                Text = string.Empty;
                TextColor = UIColor.Clear;

                Frame = new CGRect(0f, parentFrame.Height - offset, parentFrame.Width, 0f);
                Alpha = 0f;

                if (scrollView != null)
                {
                    scrollView.ContentInset = new UIEdgeInsets(scrollView.ContentInset.Top, scrollView.ContentInset.Left, offset, scrollView.ContentInset.Right);
                    scrollView.ScrollIndicatorInsets = new UIEdgeInsets(scrollView.ScrollIndicatorInsets.Top, scrollView.ScrollIndicatorInsets.Left, offset, scrollView.ScrollIndicatorInsets.Right);
                }
            };

            if (animate)
                AnimateNotify(AnimationDuration, 0f, UIViewAnimationOptions.CurveEaseIn | UIViewAnimationOptions.BeginFromCurrentState, action, null);
            else
                action();
        }

        public override void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
        {
            if (keyPath == "frame" && ofObject == Superview)
                Position();
        }

        void Position()
        {
            if (Superview == null)
                return;
            
            parentFrame = Superview.Frame;

            if (Alpha > 0f)
                Frame = new CGRect(0f, parentFrame.Height - offset - Height, parentFrame.Width, Height);
            else
                Frame = new CGRect(0f, parentFrame.Height - offset, parentFrame.Width, 0f);
        }

        void ReachabilityService_RefreshingReachability(object sender, EventArgs e)
        {
            BeginInvokeOnMainThread(ShowConnecting);
        }

        void ReachabilityService_ReachabilityRefreshed(object sender, ReachabilityRefreshedEventArgs e)
        {
            if (e.IsReachable)
                BeginInvokeOnMainThread(Hide);
            else
                BeginInvokeOnMainThread(ShowDisconnected);
        }
    }
}
