using System;
using System.Linq;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Utilities;
using ObjCRuntime;
using UIKit;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Utilities;

namespace Mark5.Mobile.IOS.Ui.Common
{
    public class ReachabilityBar : UILabel
    {
        const double AnimationDuration = .25d;
        const float VisibleHeight = 20f;
        const float HiddenHeight = 0f;

        WeakReference<UITableViewController> weakViewController;

        public static void Attach(UITableViewController viewController)
        {
            var view = viewController.View;

            var rb = new ReachabilityBar(viewController);
            view.AddSubview(rb);
        }

        public static void Detach(UITableViewController controller)
        {
            var view = controller.View;

            foreach (var rb in view.Subviews.OfType<ReachabilityBar>().ToArray())
                rb.RemoveFromSuperview();
        }

        public ReachabilityBar(UITableViewController viewController)
        {
            weakViewController = viewController.Wrap();

            Layer.ZPosition = nfloat.MaxValue;
            TextAlignment = UITextAlignment.Center;
            Font = Theme.DefaultFont.WithSize(12f);
        }

        public override void MovedToSuperview()
        {
            base.MovedToSuperview();

            var r = CommonConfig.Reachability;

            if (Superview != null)
            {
                TranslatesAutoresizingMaskIntoConstraints = false;

                NSLayoutConstraint c1;
                if (Integration.IsRunningAtLeast(11))
                    c1 = BottomAnchor.ConstraintEqualTo(Superview.SafeAreaLayoutGuide.TopAnchor);
                else
                    c1 = TopAnchor.ConstraintEqualTo(Superview.TopAnchor);
                c1.SetIdentifier("reachabilitybar.top");
                var c2 = LeftAnchor.ConstraintEqualTo(Superview.LeftAnchor);
                c2.SetIdentifier("reachabilitybar.left");
                var c3 = WidthAnchor.ConstraintEqualTo(Superview.WidthAnchor);
                c3.SetIdentifier("reachabilitybar.width");
                Superview.AddConstraints(new[] { c1, c2, c3 });

                var c4 = HeightAnchor.ConstraintEqualTo(0f);
                c4.SetIdentifier("reachabilitybar.height");
                AddConstraint(c4);

                r.RefreshingReachability += Reachability_RefreshingReachability;
                r.ReachabilityRefreshed += Reachability_ReachabilityRefreshed;

                AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("tapped:")));

                if (CommonConfig.Reachability.IsCheckingReachability)
                    ShowConnecting(false);
                else if (!CommonConfig.Reachability.IsReachable)
                    ShowDisconnected(false);
                else
                    HideConnected(false);
            }
            else
            {
                Layer.RemoveAllAnimations();

                r.RefreshingReachability -= Reachability_RefreshingReachability;
                r.ReachabilityRefreshed -= Reachability_ReachabilityRefreshed;

                GestureRecognizers?.ForEach(RemoveGestureRecognizer);
            }
        }

        void Reachability_RefreshingReachability(object sender, EventArgs e) => BeginInvokeOnMainThread(() => ShowConnecting(true));

        void Reachability_ReachabilityRefreshed(object sender, ReachabilityRefreshedEventArgs e)
        {
            if (e.IsReachable)
                BeginInvokeOnMainThread(() => HideConnected(true));
            else
                BeginInvokeOnMainThread(() => ShowDisconnected(true));
        }

        void ShowConnecting(bool animate)
        {
            UserInteractionEnabled = false;

            TextColor = Theme.White;
            Text = Localization.GetString("connecting___");
            BackgroundColor = Theme.LightBrown;

            var heightConstraint = Constraints.First(nslc => nslc.GetIdentifier() == "reachabilitybar.height");
            heightConstraint.Constant = VisibleHeight;
            LayoutIfNeeded();

            Action action = () =>
            {
                Alpha = 1f;

                if (Integration.IsRunningAtLeast(11))
                {
                    var viewController = weakViewController.Unwrap();
                    if (viewController != null)
                    {
                        var asai = viewController.AdditionalSafeAreaInsets;
                        asai = new UIEdgeInsets(VisibleHeight, asai.Left, asai.Bottom, asai.Right);
                        viewController.AdditionalSafeAreaInsets = asai;
                    }
                }
            };

            UICompletionHandler completion = finished => UserInteractionEnabled = true;

            if (animate)
                AnimateNotify(AnimationDuration, 0d, UIViewAnimationOptions.CurveEaseIn | UIViewAnimationOptions.BeginFromCurrentState, action, completion);
            else
            {
                action();
                completion(true);
            }
        }

        void ShowDisconnected(bool animate)
        {
            UserInteractionEnabled = false;

            TextColor = Theme.White;
            Text = Localization.GetString("disconnected");
            BackgroundColor = Theme.Brown;

            var heightConstraint = Constraints.First(nslc => nslc.GetIdentifier() == "reachabilitybar.height");
            heightConstraint.Constant = VisibleHeight;
            LayoutIfNeeded();

            Action action = () =>
            {
                Alpha = 1f;

                if (Integration.IsRunningAtLeast(11))
                {
                    var viewController = weakViewController.Unwrap();
                    if (viewController != null)
                    {
                        var asai = viewController.AdditionalSafeAreaInsets;
                        asai = new UIEdgeInsets(VisibleHeight, asai.Left, asai.Bottom, asai.Right);
                        viewController.AdditionalSafeAreaInsets = asai;
                    }
                }
            };

            UICompletionHandler completion = finished => UserInteractionEnabled = true;

            if (animate)
                AnimateNotify(AnimationDuration, 0d, UIViewAnimationOptions.CurveEaseIn | UIViewAnimationOptions.BeginFromCurrentState, action, completion);
            else
            {
                action();
                completion(true);
            }
        }

        void HideConnected(bool animate)
        {
            UserInteractionEnabled = false;

            Action action = () =>
            {
                Alpha = 0f;

                if (Integration.IsRunningAtLeast(11))
                {
                    var viewController = weakViewController.Unwrap();
                    if (viewController != null)
                    {
                        var asai = viewController.AdditionalSafeAreaInsets;
                        asai = new UIEdgeInsets(HiddenHeight, asai.Left, asai.Bottom, asai.Right);
                        viewController.AdditionalSafeAreaInsets = asai;
                    }
                }
            };

            UICompletionHandler completion = finished =>
            {
                var heightConstraint = Constraints.First(nslc => nslc.GetIdentifier() == "reachabilitybar.height");
                heightConstraint.Constant = HiddenHeight;
                LayoutIfNeeded();

                TextColor = Theme.Clear;
                Text = null;
                BackgroundColor = Theme.Clear;

                UserInteractionEnabled = true;
            };

            if (animate)
                AnimateNotify(AnimationDuration, 0d, UIViewAnimationOptions.CurveEaseIn | UIViewAnimationOptions.BeginFromCurrentState, action, completion);
            else
            {
                action();
                completion(true);
            }
        }

        [Export("tapped:")]
        public void Tapped(UITapGestureRecognizer recognizer) => CommonConfig.Reachability.Refresh();
    }
}
