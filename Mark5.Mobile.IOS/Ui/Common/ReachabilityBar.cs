using System;
using System.Linq;
using System.Text;
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

        public static void Attach(UIView parent, UIScrollView scrollView = null, float offset = 0f, UITextAlignment alignment = UITextAlignment.Center)
        {
            if (parent == null)
                return;

            if (parent.Subviews.Any(v => v is ReachabilityBar))
                return;

            parent.AddSubview(new ReachabilityBar(scrollView, offset, alignment));
        }

        ReachabilityBar(UIScrollView scrollView, float offset, UITextAlignment alignment)
        {
            this.scrollView = scrollView;
            this.offset = offset;

            AutoresizingMask = UIViewAutoresizing.None;

            TextAlignment = alignment;
            Font = Theme.DefaultFont.WithSize(12f);


            AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));
            AddGestureRecognizer(new UILongPressGestureRecognizer(this, new Selector("longPressed:"))
            {
                MinimumPressDuration = 2d
            });
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

                    CommonConfig.Reachability.RefreshingReachability += ReachabilityService_RefreshingReachability;
                    CommonConfig.Reachability.ReachabilityRefreshed += ReachabilityService_ReachabilityRefreshed;
                }
            }
            else
            {
                CommonConfig.Reachability.RefreshingReachability -= ReachabilityService_RefreshingReachability;
                CommonConfig.Reachability.ReachabilityRefreshed -= ReachabilityService_ReachabilityRefreshed;

                Layer.RemoveAllAnimations();
            }
        }

        public override void MovedToSuperview()
        {
            base.MovedToSuperview();

            if (Superview == null)
                return;

            parentFrame = Superview.Frame;

            if (CommonConfig.Reachability.IsCheckingReachability)
                ShowConnecting(false);
            else if (!CommonConfig.Reachability.IsReachable)
                ShowDisconnected(false);
            else
                Hide(false);
        }

        public override void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
        {
            if (keyPath == "frame" && ofObject == Superview)
                Position();
        }

        public override void DrawText(CGRect rect)
        {
            base.DrawText(new UIEdgeInsets(0f, 5f, 0f, 5f).InsetRect(rect));
        }

        [Export("tapped:")]
        public void Tapped(UILongPressGestureRecognizer recognizer)
        {
            CommonConfig.Reachability.Refresh();
        }

        [Export("longPressed:")]
#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public async void LongPressed(UILongPressGestureRecognizer recognizer)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            UserInteractionEnabled = false;

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("testing_connection___"));

            try
            {
                var network = await CommonConfig.Reachability.Refresh(ReachabilityMode.NetworkAvailability, true);
                var google = await CommonConfig.Reachability.Refresh(ReachabilityMode.Google, true);
                var serviceConnection = await CommonConfig.Reachability.Refresh(ReachabilityMode.ServiceConnection, true);
                var service = await CommonConfig.Reachability.Refresh(ReachabilityMode.Service, true);

                var title = Localization.GetString("connection_status");

                var messageSb = new StringBuilder();
                messageSb.Append(Localization.GetString("network_interface"));
                messageSb.Append(" ");
                messageSb.AppendLine(network ? Localization.GetString("ok") : Localization.GetString("unavailable"));
                messageSb.Append(Localization.GetString("internet_access"));
                messageSb.Append(" ");
                messageSb.AppendLine(google ? Localization.GetString("ok") : Localization.GetString("unavailable"));
                messageSb.Append(Localization.GetString("mark5_server_reachability"));
                messageSb.Append(" ");
                messageSb.AppendLine(serviceConnection ? Localization.GetString("ok") : Localization.GetString("unavailable"));
                messageSb.Append(Localization.GetString("mark5_service_reachability"));
                messageSb.Append(" ");
                messageSb.AppendLine(service ? Localization.GetString("ok") : Localization.GetString("unavailable"));

                dismissAction();

                await Dialogs.ShowConfirmDialogAsync(ParentViewController(), title, messageSb.ToString());
            }
            catch (Exception ex)
            {
                dismissAction();

                await Dialogs.ShowErrorDialogAsync(ParentViewController(), ex);
            }

            UserInteractionEnabled = true;
        }

        void ShowDisconnected()
        {
            ShowDisconnected(true);
        }

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

            UICompletionHandler completion = finished => { UserInteractionEnabled = true; };

            if (animate)
            {
                AnimateNotify(AnimationDuration, 0f, UIViewAnimationOptions.CurveEaseIn | UIViewAnimationOptions.BeginFromCurrentState, action, completion);
            }
            else
            {
                action();
                completion(true);
            }
        }

        void ShowConnecting()
        {
            ShowConnecting(true);
        }

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

        void Hide()
        {
            Hide(true);
        }

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

        UIViewController ParentViewController()
        {
            return ParentViewController(this);
        }

        UIViewController ParentViewController(UIView view)
        {
            if (view.NextResponder is UIViewController)
                return (UIViewController) view.NextResponder;
            if (view.NextResponder is UIView)
                return ParentViewController((UIView) view.NextResponder);

            return null;
        }
    }
}