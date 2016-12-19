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
using Mark5.Mobile.IOS.Ui.ViewControllers.Common.StackView;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentViews.Subviews
{
    public abstract class ComposeDocumentView : StackSubView
    {
        public DocumentPreview DocumentPreview { get; set; }
        public Document Document { get; set; }
        public DocumentPreview PreviousDocumentPreview { get; set; }
        public Document PreviousDocument { get; set; }
        public DocumentCreationModeFlag CreationModeFlag { get; set; }

        protected float MinimumHeight = 21.0f;

        protected ComposeDocumentView()
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
            //TODO this was for the form view, don't know if it'll work with the stackview
            //var parentScrollView = Superview as UIScrollView;
            //if (parentScrollView != null)
            //{
            //    var frame = Frame;
            //    frame.Height += HorizontalMargin;
            //    if (frame.Height > parentScrollView.Frame.Height + parentScrollView.ContentOffset.Y)
            //    {
            //        frame.Height = parentScrollView.Frame.Height + parentScrollView.ContentOffset.Y;
            //    }

            //    parentScrollView.ScrollRectToVisible(frame, true);
            //}
        }

        #endregion

        public abstract Task RefreshView();

        public abstract Task UpdateDocument();
    }
}
