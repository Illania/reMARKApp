using System;
using reMark.Mobile.Common.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using UIKit;
using reMark.Mobile.IOS.Ui.Common;

namespace reMark.Mobile.IOS.Ui.ViewControllers.AutoReply
{
    public abstract class AutoReplySubView : UIStackView
    {
        protected UIView ContainerView;
        public AutoReplyRule AutoReplyRule { get; set; }
       
        protected float MinimumHeight = 21f;
        protected float HorizontalMargin = 15f;
        protected float VerticalMargin = 10f;
        protected float InnerMargin = 5f;

        protected AutoReplySubView()
        {
            Initialize();
        }

        void Initialize()
        {
            Opaque = false;
            Axis = UILayoutConstraintAxis.Vertical;
            Alignment = UIStackViewAlignment.Fill;
            Distribution = UIStackViewDistribution.Fill;
            Spacing = 0f;
            TranslatesAutoresizingMaskIntoConstraints = false;

            ContainerView = new UIView();
            AddArrangedSubview(ContainerView);
            AddArrangedSubview(new SeparatorSubView());
        }

        #region Event handlers

        protected void HandleScrollToView(object sender, EventArgs e)
        {
            if (Superview.Superview is UIScrollView parentScrollView)
            {
                var frame = Frame;
                frame.Height -= 2 * VerticalMargin;
                if (frame.Height > parentScrollView.Frame.Height + parentScrollView.ContentOffset.Y)
                    frame.Height = parentScrollView.Frame.Height + parentScrollView.ContentOffset.Y;

                parentScrollView.ScrollRectToVisible(frame, true);
            }
        }

        #endregion

        public abstract Task InitializeView();

        public abstract Task UpdateAutoReplyRule();
    }

}

