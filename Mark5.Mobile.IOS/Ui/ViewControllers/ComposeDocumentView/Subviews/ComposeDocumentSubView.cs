//
// Project: Mark5.Mobile.IOS
// File: ComposeDocumentSubview.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Support;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentViews.Subviews
{
    public abstract class ComposeDocumentSubView : UIStackView
    {
        protected UIView ContainerView;

        public DocumentPreview DocumentPreview { get; set; }
        public Document Document { get; set; }
        public DocumentPreview PreviousDocumentPreview { get; set; }
        public Document PreviousDocument { get; set; }
        public DocumentCreationModeFlag CreationModeFlag { get; set; }
        public CopyToNewOptions CopyToNewOptions { get; set; }

        protected float MinimumHeight = 21f;
        protected float HorizontalMargin = 15f;
        protected float VerticalMargin = 12f;
        protected float InnerMargin = 5f;

        protected ComposeDocumentSubView()
        {
            Initialize();
        }

        void Initialize()
        {
            BackgroundColor = UIColor.White;
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
            var parentScrollView = Superview.Superview as UIScrollView;
            if (parentScrollView != null)
            {
                var frame = Frame;
                frame.Height -= 2 * VerticalMargin;
                if (frame.Height > parentScrollView.Frame.Height + parentScrollView.ContentOffset.Y)
                {
                    frame.Height = parentScrollView.Frame.Height + parentScrollView.ContentOffset.Y;
                }

                parentScrollView.ScrollRectToVisible(frame, true);
            }
        }

        #endregion

        public abstract Task RefreshView();

        public abstract Task UpdateDocument();
    }
}
