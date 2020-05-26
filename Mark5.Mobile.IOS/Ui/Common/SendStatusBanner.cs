using System;
using System.Linq;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model.HubMessages;
using Mark5.Mobile.Common.Utilities.Extensions;
using TinyMessenger;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
{
    public class SendStatusBanner : UIView
    {
        WeakReference<UITableViewController> weakViewController;
        TinyMessageSubscriptionToken documentUploadStatusChangedToken;

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

        public SendStatusBanner(UITableViewController viewController)
        {
            weakViewController = viewController.Wrap();

            TranslatesAutoresizingMaskIntoConstraints = false;
            Layer.ZPosition = float.MaxValue;
            Opaque = true;
            BackgroundColor = UIColor.Clear;

            CreateBanner();
        }

        void CreateBanner()
        {
            var backgroundView = new UIView();
            backgroundView.TranslatesAutoresizingMaskIntoConstraints = false;
            backgroundView.BackgroundColor = Theme.TintColor;
            backgroundView.ClipsToBounds = false;
            backgroundView.Layer.CornerRadius = 12;

            AddSubview(backgroundView);
            AddConstraints(new[] {
                backgroundView.LeftAnchor.ConstraintEqualTo(LeftAnchor, 10),
                backgroundView.RightAnchor.ConstraintEqualTo(RightAnchor, -10),
                backgroundView.BottomAnchor.ConstraintEqualTo(BottomAnchor, -25),
                backgroundView.TopAnchor.ConstraintEqualTo(TopAnchor),
            });

            var banner = new UILabel();
            banner.TranslatesAutoresizingMaskIntoConstraints = false;
            banner.Text = "Test text";
            banner.TextColor = UIColor.White;
            banner.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            //banner.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);

            banner.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            //banner.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);

            nfloat internalPadding = 15;

            backgroundView.AddSubview(banner);
            backgroundView.AddConstraints(new[] {
                banner.LeftAnchor.ConstraintEqualTo(backgroundView.LeftAnchor, internalPadding),
                banner.RightAnchor.ConstraintEqualTo(backgroundView.RightAnchor, -internalPadding),
                banner.BottomAnchor.ConstraintEqualTo(backgroundView.BottomAnchor, -internalPadding),
                banner.TopAnchor.ConstraintEqualTo(backgroundView.TopAnchor, internalPadding),
            });

        }

        NSLayoutConstraint bottomConstraint;
        NSLayoutConstraint topConstraint;

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
                    topConstraint = TopAnchor.ConstraintEqualTo(Superview.SafeAreaLayoutGuide.BottomAnchor),
                });

                bottomConstraint.Active = false;
            }
            else
            {
                UnsubscribeFromMessages();
            }
            Superview.LayoutIfNeeded();

            Animate(5, () =>
            {
                bottomConstraint.Active = true;
                topConstraint.Active = false;

                Superview.LayoutIfNeeded();
            });
        }

        void SubscribeToMessages()
        {
            documentUploadStatusChangedToken = CommonConfig.MessengerHub.Subscribe<DocumentUploadStatusChangedMessage>(DocumentUploadStatusChanged);
        }

        void UnsubscribeFromMessages()
        {
            documentUploadStatusChangedToken?.Dispose();
        }

        void DocumentUploadStatusChanged(DocumentUploadStatusChangedMessage obj)
        {
            throw new NotImplementedException();
        }
    }
}
