//
// Project: Mark5.Mobile.IOS
// File: StackSubView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.Common.StackView
{
    public abstract class StackSubView : UIView
    {

        protected float HorizontalMargin = 15.0f;
        protected float VerticalMargin = 12.0f;
        protected float InnerMargin = 5.0f;

        protected StackSubView()
        {
            Initialize();
        }

        void Initialize()
        {
            Opaque = false;
            TranslatesAutoresizingMaskIntoConstraints = false;
        }

        public override void AddConstraint(NSLayoutConstraint constraint)
        {
            constraint.Priority = 999;

            base.AddConstraint(constraint);
        }

        public override void AddConstraints(NSLayoutConstraint[] constraints)
        {
            foreach (var constraint in constraints)
            {
                constraint.Priority = 999;
            }

            base.AddConstraints(constraints);
        }
    }
}
