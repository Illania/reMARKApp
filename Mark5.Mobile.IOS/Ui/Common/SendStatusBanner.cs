using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using AudioToolbox;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model.HubMessages;
using TinyMessenger;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
{
    public class SendStatusBanner : UIView
    {
        readonly double animationDuration = 0.25;
        readonly double bannerDuration = 2;
        readonly BannerView bannerView;

        TinyMessageSubscriptionToken documentUploadStatusChangedToken;
        NSLayoutConstraint bottomConstraint;
        NSLayoutConstraint topConstraint;
        ConcurrentQueue<BannerInfo> queue = new ConcurrentQueue<BannerInfo>();

        public static void Attach(UIViewController viewController)
        {
            var view = viewController.View;

            var rb = new SendStatusBanner();
            view.AddSubview(rb);
        }

        public static void Detach(UIViewController controller)
        {
            var view = controller.View;

            foreach (var rb in view.Subviews.OfType<SendStatusBanner>().ToArray())
                rb.RemoveFromSuperview();
        }

        public SendStatusBanner()
        {
            TranslatesAutoresizingMaskIntoConstraints = false;
            Layer.ZPosition = float.MaxValue;
            Opaque = true;
            BackgroundColor = UIColor.Clear;
            Hidden = true;

            bannerView = new BannerView();
            AddSubview(bannerView);
            AddConstraints(new[] {
                bannerView.CenterXAnchor.ConstraintEqualTo(CenterXAnchor),
                bannerView.BottomAnchor.ConstraintEqualTo(BottomAnchor, -25),
                bannerView.TopAnchor.ConstraintEqualTo(TopAnchor),
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
                queue.Clear();
            }
        }

        void SubscribeToMessages()
        {
            documentUploadStatusChangedToken = CommonConfig.MessengerHub.Subscribe<DocumentUploadStatusChangedMessage>(DocumentUploadStatusChanged);
        }

        void UnsubscribeFromMessages()
        {
            documentUploadStatusChangedToken?.Dispose();
        }

        void ShowBanner(BannerInfo info)
        {
            BeginInvokeOnMainThread(() =>
            {
                if (Superview == null)
                    return;

                bannerView.SetData(info);

                void action()
                {
                    bottomConstraint.Active = true;
                    topConstraint.Active = false;

                    Superview?.LayoutIfNeeded();
                }

                Hidden = false;
                Superview?.LayoutIfNeeded();
                AnimateNotify(animationDuration, 0d, 0.7f, 0, UIViewAnimationOptions.CurveEaseInOut, action, null);
                Task.Delay(TimeSpan.FromSeconds(animationDuration + bannerDuration)).ContinueWith((task) => HideBanner()); ;
                SystemSound.Vibrate.PlayAlertSound();
            });
        }

        void HideBanner()
        {
            BeginInvokeOnMainThread(() =>
            {
                if (Superview == null)
                    return;

                void action()
                {
                    bottomConstraint.Active = false;
                    topConstraint.Active = true;

                    Superview?.LayoutIfNeeded();
                }

                Superview?.LayoutIfNeeded();
                AnimateNotify(animationDuration, 0d, 0.7f, 0, UIViewAnimationOptions.CurveEaseInOut, action, (c) =>
                {
                    Hidden = true;
                    OnBannerHidden();
                });
            });
        }

        void QueueBanner(BannerInfo info)
        {
            if (queue.IsEmpty)
                ShowBanner(info);

            queue.Enqueue(info);
        }

        void OnBannerHidden()
        {
            queue.TryDequeue(out _);

            if (queue.TryPeek(out var info))
                ShowBanner(info);
        }

        void DocumentUploadStatusChanged(DocumentUploadStatusChangedMessage obj)
        {
            if (obj.Change != DocumentUploadStatusChangedMessage.Status.DocumentSent && obj.Change != DocumentUploadStatusChangedMessage.Status.DocumentSentFailed)
                return;

            QueueBanner(BannerInfo.FromStatus(obj.Change));
        }

        class BannerInfo
        {
            public string Text { get; private set; }
            public UIColor Color { get; private set; }

            private BannerInfo() { }

            public static BannerInfo FromStatus(DocumentUploadStatusChangedMessage.Status status)
            {
                var bannerInfo = new BannerInfo();
                if (status == DocumentUploadStatusChangedMessage.Status.DocumentSent)
                {
                    bannerInfo.Text = Localization.GetString("email_sent");
                    bannerInfo.Color = Theme.TintColor;
                }
                else if (status == DocumentUploadStatusChangedMessage.Status.DocumentSentFailed)
                {
                    bannerInfo.Text = Localization.GetString("email_error");
                    bannerInfo.Color = UIColor.Red;
                }

                return bannerInfo;
            }
        }

        class BannerView : UIView
        {
            readonly UILabel bannerLabel;

            public BannerView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false;
                ClipsToBounds = false;
                Layer.CornerRadius = 10;

                bannerLabel = new UILabel
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    TextColor = UIColor.White
                };
                bannerLabel.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
                bannerLabel.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);

                nfloat internalPadding = 8;

                AddSubview(bannerLabel);
                AddConstraints(new[] {
                    bannerLabel.LeftAnchor.ConstraintEqualTo(LeftAnchor, internalPadding),
                    bannerLabel.RightAnchor.ConstraintEqualTo(RightAnchor, -internalPadding),
                    bannerLabel.BottomAnchor.ConstraintEqualTo(BottomAnchor, -internalPadding),
                    bannerLabel.TopAnchor.ConstraintEqualTo(TopAnchor, internalPadding),
                });
            }

            public void SetData(BannerInfo info)
            {
                BackgroundColor = info.Color;
                bannerLabel.Text = info.Text;
            }
        }
    }
}
