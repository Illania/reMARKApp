using System;
using System.Linq;
using System.Threading.Tasks;
using AudioToolbox;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model.HubMessages;
using Mark5.Mobile.Common.Utilities.Extensions;
using TinyMessenger;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
{
    public class SendStatusBanner : UIView
    {
        readonly double animationDuration = 0.25;
        readonly double bannerDuration = 3;

        WeakReference<UITableViewController> weakViewController;
        TinyMessageSubscriptionToken documentUploadStatusChangedToken;
        NSLayoutConstraint bottomConstraint;
        NSLayoutConstraint topConstraint;

        public static void Attach(UITableViewController viewController)
        {
            var view = viewController.View;

            var rb = new SendStatusBanner(viewController);
            view.AddSubview(rb);
        }

        public static void Detach(UITableViewController controller)
        {
            var view = controller.View;

            foreach (var rb in view.Subviews.OfType<SendStatusBanner>().ToArray())
                rb.RemoveFromSuperview();
        }

        void SubscribeToMessages()
        {
            documentUploadStatusChangedToken = CommonConfig.MessengerHub.Subscribe<DocumentUploadStatusChangedMessage>(DocumentUploadStatusChanged);
        }

        void UnsubscribeFromMessages()
        {
            documentUploadStatusChangedToken?.Dispose();
        }

        public SendStatusBanner(UITableViewController viewController)
        {
            weakViewController = viewController.Wrap();

            TranslatesAutoresizingMaskIntoConstraints = false;
            Layer.ZPosition = float.MaxValue;
            Opaque = true;
            BackgroundColor = UIColor.Clear;
            Hidden = true;

            CreateBanner();
        }

        void CreateBanner()
        {
            var backgroundView = new UIView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = Theme.TintColor,
                ClipsToBounds = false
            };
            backgroundView.Layer.CornerRadius = 12;

            AddSubview(backgroundView);
            AddConstraints(new[] {
                backgroundView.LeftAnchor.ConstraintEqualTo(LeftAnchor, 10),
                backgroundView.RightAnchor.ConstraintEqualTo(RightAnchor, -10),
                backgroundView.BottomAnchor.ConstraintEqualTo(BottomAnchor, -25),
                backgroundView.TopAnchor.ConstraintEqualTo(TopAnchor),
            });

            var banner = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Text = "Test text",
                TextColor = UIColor.White
            };
            banner.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            banner.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);

            nfloat internalPadding = 15;

            backgroundView.AddSubview(banner);
            backgroundView.AddConstraints(new[] {
                banner.LeftAnchor.ConstraintEqualTo(backgroundView.LeftAnchor, internalPadding),
                banner.RightAnchor.ConstraintEqualTo(backgroundView.RightAnchor, -internalPadding),
                banner.BottomAnchor.ConstraintEqualTo(backgroundView.BottomAnchor, -internalPadding),
                banner.TopAnchor.ConstraintEqualTo(backgroundView.TopAnchor, internalPadding),
            });
        }

        public override void MovedToSuperview()
        {
            base.MovedToSuperview();

            if (Superview != null)
            {
                SubscribeToMessages();

                Superview.AddConstraints(new[] {
                    WidthAnchor.ConstraintEqualTo(Superview.WidthAnchor),
                    bottomConstraint = BottomAnchor.ConstraintEqualTo(Superview.SafeAreaLayoutGuide.BottomAnchor),
                    CenterXAnchor.ConstraintEqualTo(Superview.CenterXAnchor),
                    topConstraint = TopAnchor.ConstraintEqualTo(Superview.SafeAreaLayoutGuide.BottomAnchor, 100),
                });

                bottomConstraint.Active = false;
            }
            else
            {
                UnsubscribeFromMessages();
            }


        }

        void ShowBanner()
        {
            void action()
            {
                bottomConstraint.Active = true;
                topConstraint.Active = false;

                Superview.LayoutIfNeeded();
            }

            Hidden = false;
            Superview.LayoutIfNeeded();
            AnimateNotify(animationDuration, 0d, 0.7f, 0, UIViewAnimationOptions.CurveEaseInOut, action, null);
            Task.Delay(TimeSpan.FromSeconds(animationDuration + bannerDuration)).ContinueWith((task) => BeginInvokeOnMainThread(HideBanner));
            SystemSound.Vibrate.PlayAlertSound();
        }

        void HideBanner()
        {
            void action()
            {
                bottomConstraint.Active = false;
                topConstraint.Active = true;

                Superview.LayoutIfNeeded();
            }

            Superview.LayoutIfNeeded();
            AnimateNotify(animationDuration, 0d, 0.7f, 0, UIViewAnimationOptions.CurveEaseInOut, action, (c) => { Hidden = true; });
        }

        void DocumentUploadStatusChanged(DocumentUploadStatusChangedMessage obj)
        {
            ShowBanner();
        }
    }
}
