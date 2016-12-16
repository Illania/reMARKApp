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
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentView.Subviews
{
    public abstract class ComposeDocumentSubview : UIView
    {
        public DocumentPreview DocumentPreview { get; set; }
        public Document Document { get; set; }
        public DocumentPreview PreviousDocumentPreview { get; set; }
        public Document PreviousDocument { get; set; }
        public DocumentCreationModeFlag CreationModeFlag { get; set; }

        protected float MinimumHeight = 21.0f;
        protected float HorizontalMargin = 15.0f;
        protected float VerticalMargin = 12.0f;
        protected float InnerMargin = 5.0f;

        protected ComposeDocumentSubview()
        {
            Initialize();
        }

        void Initialize()
        {
            Opaque = false;
            TranslatesAutoresizingMaskIntoConstraints = false;
            SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
        }

        #region Event handlers

        protected void HandleScrollToView(object sender, EventArgs e)
        {
            var parentScrollView = Superview as UIScrollView;
            if (parentScrollView != null)
            {
                var frame = Frame;
                frame.Height += HorizontalMargin;
                if (frame.Height > parentScrollView.Frame.Height + parentScrollView.ContentOffset.Y)
                {
                    frame.Height = parentScrollView.Frame.Height + parentScrollView.ContentOffset.Y;
                }

                parentScrollView.ScrollRectToVisible(frame, true);
            }
        }

        #endregion

        //TODO PinView

        public abstract Task RefreshView();

        public abstract Task UpdateDocument();
    }
}
