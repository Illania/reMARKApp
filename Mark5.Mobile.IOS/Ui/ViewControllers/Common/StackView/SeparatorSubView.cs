//
// Project: Mark5.Mobile.IOS
// File: SeparatorSubView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.Common.StackView
{
    public class SeparatorSubView : UIView
    {
        readonly static UIColor backgroundColor = new UITableView().SeparatorColor;

        public SeparatorSubView()
        {
            Initialize();
        }

        void Initialize()
        {
            var line = new UIView();
            line.BackgroundColor = backgroundColor;
            line.TranslatesAutoresizingMaskIntoConstraints = false;
            AddSubview(line);
            var constraints = new[]
            {
                NSLayoutConstraint.Create(line, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1.0f, 0.0f),
                NSLayoutConstraint.Create(line, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1.0f, 15.0f),
                NSLayoutConstraint.Create(line, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1.0f, 0.0f),
                NSLayoutConstraint.Create(line, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this, NSLayoutAttribute.Bottom, 1.0f, 0.0f),
                NSLayoutConstraint.Create(line, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1.0f, 0.5f),
            };
            foreach (var constraint in constraints)
            {
                constraint.Priority = 500;
            }
            AddConstraints(constraints);
        }
    }
}
