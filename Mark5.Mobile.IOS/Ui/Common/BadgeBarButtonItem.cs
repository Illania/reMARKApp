using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
{
    public sealed class BadgeBarButtonItem : UIBarButtonItem
    {
        public string BadgeValue
        {
            get => badgeValue;
            set
            {
                badgeValue = value;
                SetBadgeValue(value);
            }
        }

        public UIColor BadgeBackgroundColor
        {
            get => badgeBackgroundColor;
            set
            {
                badgeBackgroundColor = value;

                if (badge != null)
                    RefreshBadge();
            }
        }

        public UIColor BadgeTextColor
        {
            get => badgeTextColor;
            set
            {
                badgeTextColor = value;

                if (badge != null)
                    RefreshBadge();
            }
        }

        public UIFont BadgeFont
        {
            get => badgeFont;
            set
            {
                badgeFont = value;

                if (badge != null)
                    RefreshBadge();
            }
        }

        public nfloat BadgePadding
        {
            get => badgePadding;
            set
            {
                badgePadding = value;

                if (badge != null)
                    UpdateBadgeFrame();
            }
        }

        public nfloat BadgeMinSize
        {
            get => badgeMinSize;
            set
            {
                badgeMinSize = value;

                if (badge != null)
                    UpdateBadgeFrame();
            }
        }

        public nfloat BadgeOriginX
        {
            get => badgeOriginX;
            set
            {
                badgeOriginX = value;

                if (badge != null)
                    UpdateBadgeFrame();
            }
        }

        public nfloat BadgeOriginY
        {
            get => badgeOriginY;
            set
            {
                badgeOriginY = value;

                if (badge != null)
                    UpdateBadgeFrame();
            }
        }

        public bool ShouldHideBadgeAtZero
        {
            get => shouldHideBadgeAtZero;
            set
            {
                shouldHideBadgeAtZero = value;
                if (badge != null)
                    UpdateBadgeFrame();
            }
        }

        public bool ShouldAnimateBadge
        {
            get => shouldAnimateBadge;
            set
            {
                shouldAnimateBadge = value;

                if (badge != null)
                    UpdateBadgeFrame();
            }
        }

        UILabel badge;
        string badgeValue;
        UIColor badgeBackgroundColor;
        UIColor badgeTextColor;
        UIFont badgeFont;
        nfloat badgePadding;
        nfloat badgeMinSize;
        nfloat badgeOriginX;
        nfloat badgeOriginY;
        bool shouldHideBadgeAtZero;
        bool shouldAnimateBadge;

        public BadgeBarButtonItem(UIButton customButton)
        {
            CustomView = customButton;
            if (CustomView != null)
                Initialize();
        }

        void Initialize()
        {
            BadgeBackgroundColor = UIColor.Red;
            BadgeTextColor = UIColor.White;
            BadgeFont = UIFont.SystemFontOfSize(10);
            BadgePadding = 5;
            BadgeMinSize = 8;
            BadgeOriginX = 15;
            BadgeOriginY = -10;
            ShouldHideBadgeAtZero = true;
            ShouldAnimateBadge = true;
            CustomView.ClipsToBounds = false;
        }

        public void SetBadgeValue(string newBadgeValue, bool animate = true)
        {
            badgeValue = newBadgeValue;

            if (string.IsNullOrEmpty(newBadgeValue) || newBadgeValue == @"0" && ShouldHideBadgeAtZero)
            {
                RemoveBadge();
            }
            else if (badge == null)
            {
                badge = new UILabel(new CGRect(BadgeOriginX, BadgeOriginY, 20, 20))
                {
                    TextColor = BadgeTextColor,
                    BackgroundColor = BadgeBackgroundColor,
                    Font = BadgeFont,
                    TextAlignment = UITextAlignment.Center,
                };

                CustomView.AddSubview(badge);
                UpdateBadgeValue(animate);
            }
            else
            {
                UpdateBadgeValue(animate);
            }
        }


        void RefreshBadge()
        {
            badge.TextColor = BadgeTextColor;
            badge.BackgroundColor = BadgeBackgroundColor;
            badge.Font = BadgeFont;
        }

        void UpdateBadgeFrame()
        {
            var frameLabel = DuplicateLabel(badge);

            frameLabel.SizeToFit();

            var expectedLabelSize = frameLabel.Frame.Size;

            var minHeight = expectedLabelSize.Height;

            minHeight = minHeight < BadgeMinSize ? BadgeMinSize : expectedLabelSize.Height;
            var minWidth = expectedLabelSize.Width;
            var padding = BadgePadding;

            minWidth = minWidth < minHeight ? minHeight : expectedLabelSize.Width;
            badge.ClipsToBounds = true;
            badge.Frame = new CGRect(BadgeOriginX, BadgeOriginY, minWidth + padding, minHeight + padding);
            badge.Layer.CornerRadius = (minHeight + padding) / 2;
            badge.Layer.MasksToBounds = true;
        }

        void UpdateBadgeValue(bool animated)
        {
            if (animated && ShouldAnimateBadge && badge.Text != BadgeValue)
            {
                var animation = new CABasicAnimation
                {
                    KeyPath = @"transform.scale",
                    From = NSObject.FromObject(1.5),
                    To = NSObject.FromObject(1),
                    Duration = 0.2,
                    TimingFunction = new CAMediaTimingFunction(0.4f, 1.3f, 1f, 1f)
                };
                badge.Layer.AddAnimation(animation, @"bounceAnimation");
            }

            badge.Text = BadgeValue;

            var duration = animated ? 0.2 : 0;
            UIView.Animate(duration, UpdateBadgeFrame);
        }

        void RemoveBadge()
        {
            if (badge != null)
                UIView.AnimateNotify(0.15f, 0f, UIViewAnimationOptions.CurveEaseIn, () => { badge.Transform = CGAffineTransform.MakeScale(0.1f, 0.1f); }, completed =>
                {
                    if (badge != null)
                    {
                        badge.RemoveFromSuperview();
                        badge = null;
                    }
                });
        }

        static UILabel DuplicateLabel(UILabel labelToCopy)
        {
            var duplicateLabel = new UILabel(labelToCopy.Frame)
            {
                Text = labelToCopy.Text,
                Font = labelToCopy.Font
            };

            return duplicateLabel;
        }
    }
}