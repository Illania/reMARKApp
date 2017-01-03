//
// Project: Mark5.Mobile.IOS
// File: DocumentView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.ViewControllers.Common.StackView;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.Subviews
{
    public abstract class DocumentView : StackSubView
    {
        public DocumentPreview DocumentPreview { get; set; }
        public Document Document { get; set; }

        protected DocumentView()
        {
            Initialize();
        }

        void Initialize()
        {
            Opaque = false;
            TranslatesAutoresizingMaskIntoConstraints = false;
            SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
        }

        public abstract void RefreshView();

        public abstract void UpdateVisibility();

    }
}
