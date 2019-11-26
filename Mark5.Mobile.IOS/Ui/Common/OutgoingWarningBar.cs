using UIKit;
using Foundation;
using TinyMessenger;
using System;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.Common.Model.HubMessages;

namespace Mark5.Mobile.IOS.Ui.Common
{
    public class OutgoingWarningBar : UILabel
    {
        const double AnimationDuration = .25d;
        const float VisibleHeight = 20f;
        const float HiddenHeight = 0f;

        TinyMessageSubscriptionToken outgoingDocumentCountChangedToken;
        WeakReference<UITableViewController> weakViewController;
        NSLayoutConstraint topConstraint;

        public static void Attach(UITableViewController viewController)
        {
            var view = viewController.View;
            view.AddSubview(new OutgoingWarningBar(viewController));
        }

        public static void Detach(UITableViewController controller)
        {
            var view = controller.View;

            foreach (var rb in view.Subviews.OfType<OutgoingWarningBar>().ToArray())
                rb.RemoveFromSuperview();
        }

        public OutgoingWarningBar(UITableViewController viewController)
        {
            weakViewController = viewController.Wrap();

            Layer.ZPosition = float.MaxValue;
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

                var c2 = LeftAnchor.ConstraintEqualTo(Superview.LeftAnchor);
                c2.SetIdentifier("toastbar.left");
                var c3 = WidthAnchor.ConstraintEqualTo(Superview.WidthAnchor);
                c3.SetIdentifier("toastbar.width");
                Superview.AddConstraints(new[] { c2, c3 });

                var c4 = HeightAnchor.ConstraintEqualTo(0f);
                c4.SetIdentifier("toastbar.height");
                AddConstraint(c4);

                r.RefreshingReachability += Reachability_RefreshingReachability;
                r.ReachabilityRefreshed += Reachability_ReachabilityRefreshed;

                SubscribeToMessages();
            }
            else
            {
                Layer.RemoveAllAnimations();
                r.RefreshingReachability -= Reachability_RefreshingReachability;
                r.ReachabilityRefreshed -= Reachability_ReachabilityRefreshed;
                UnsubscribeFromMessages();
            }
        }

        void ShowMessage()
        {
            TextColor = Theme.White;
            Text = Localization.GetString("outgoing_warning");
            BackgroundColor = UIColor.Red;

            var heightConstraint = Constraints.First(nslc => nslc.GetIdentifier() == "toastbar.height");
            heightConstraint.Constant = VisibleHeight;
            LayoutIfNeeded();

            if (CommonConfig.Reachability.IsReachable)
                SetConstraintOverReachabilityBar();
            else
                SetConstraintBelowReachabilityBar();

            Action action = () =>
            {
                Alpha = 1f;

                var viewController = weakViewController.Unwrap();
                if (viewController != null)
                {
                    var asai = viewController.AdditionalSafeAreaInsets;
                    asai = new UIEdgeInsets(VisibleHeight, asai.Left, asai.Bottom, asai.Right);
                    viewController.AdditionalSafeAreaInsets = asai;
                }
            };

            AnimateNotify(AnimationDuration, 0d, UIViewAnimationOptions.CurveEaseIn | UIViewAnimationOptions.BeginFromCurrentState, action, null);
        }

        void HideMessage()
        {
            Action action = () =>
            {
                Alpha = 1f;

                var viewController = weakViewController.Unwrap();
                if (viewController != null)
                {
                    var asai = viewController.AdditionalSafeAreaInsets;
                    asai = new UIEdgeInsets(VisibleHeight, asai.Left, asai.Bottom, asai.Right);
                    viewController.AdditionalSafeAreaInsets = asai;
                }
            };

            AnimateNotify(AnimationDuration, 0d, UIViewAnimationOptions.CurveEaseOut | UIViewAnimationOptions.BeginFromCurrentState, action, null);
        }

        void SetConstraintBelowReachabilityBar()
        {
            if (Superview != null)
            {
                if (topConstraint != null)
                    RemoveConstraint(topConstraint);

                topConstraint = BottomAnchor.ConstraintEqualTo(Superview.SafeAreaLayoutGuide.TopAnchor, 20f);
                topConstraint.SetIdentifier("toastbar.top");
                Superview.AddConstraint(topConstraint);
            }
        }

        void SetConstraintOverReachabilityBar()
        {
            if (Superview != null)
            {
                if (topConstraint != null)
                    RemoveConstraint(topConstraint);

                topConstraint = BottomAnchor.ConstraintEqualTo(Superview.SafeAreaLayoutGuide.TopAnchor);
                topConstraint.SetIdentifier("toastbar.top");
                Superview.AddConstraint(topConstraint);
            }
        }

        #region Subscribe/unsubscribe
        void SubscribeToMessages()
        {
            outgoingDocumentCountChangedToken = CommonConfig.MessengerHub.Subscribe<OugoingDocumentCountMessage>(HandleOutgoingDocumentCountChange);
        }

        void UnsubscribeFromMessages()
        {
            outgoingDocumentCountChangedToken?.Dispose();
        }
        #endregion

        #region handlers
        void HandleOutgoingDocumentCountChange(OugoingDocumentCountMessage ougoingMessageCount)
        {
            BeginInvokeOnMainThread(() =>
            {
                if (ougoingMessageCount.HasFailedDocuments)
                    ShowMessage();
                else
                    HideMessage();
            });
        }

        void Reachability_RefreshingReachability(object sender, EventArgs e) => BeginInvokeOnMainThread(() => SetConstraintBelowReachabilityBar());

        void Reachability_ReachabilityRefreshed(object sender, ReachabilityRefreshedEventArgs e)
        {
            if (e.IsReachable)
                BeginInvokeOnMainThread(() => SetConstraintOverReachabilityBar());
            else
                BeginInvokeOnMainThread(() => SetConstraintBelowReachabilityBar());
        }

        #endregion
    }
}