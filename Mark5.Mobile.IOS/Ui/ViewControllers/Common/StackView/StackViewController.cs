//
// Project: Mark5.Mobile.IOS
// File: StackViewController.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.Common.StackView
{
    public class StackViewController : UIViewController
    {

        protected UIScrollView ScrollView;
        protected UIStackView StackView;

        #region UIViewController overrides

        public override void LoadView()
        {
            base.LoadView();

            Initialize();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            NavigationController.HidesBarsOnSwipe = true;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            NavigationController.HidesBarsOnSwipe = false;
        }

        #endregion

        void Initialize()
        {
            View.BackgroundColor = UIColor.White;

            ScrollView = new UIScrollView
            {
                BackgroundColor = UIColor.White,
                ShowsVerticalScrollIndicator = true,
                ShowsHorizontalScrollIndicator = false,
                ScrollEnabled = true,
                ScrollsToTop = true,
                UserInteractionEnabled = true,
                ClipsToBounds = false,
                TranslatesAutoresizingMaskIntoConstraints = false,
            };
            View.AddSubview(ScrollView);
            View.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(ScrollView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(ScrollView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(ScrollView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, View, NSLayoutAttribute.Width, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(ScrollView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1.0f, 0.0f),
                });

            StackView = new UIStackView
            {
                BackgroundColor = UIColor.White,
                Axis = UILayoutConstraintAxis.Vertical,
                Alignment = UIStackViewAlignment.Fill,
                Distribution = UIStackViewDistribution.Fill,
                Spacing = 0.0f,
                TranslatesAutoresizingMaskIntoConstraints = false,
            };
            ScrollView.AddSubview(StackView);
            View.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(StackView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ScrollView, NSLayoutAttribute.Top, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(StackView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ScrollView, NSLayoutAttribute.Left, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(StackView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ScrollView, NSLayoutAttribute.Right, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(StackView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ScrollView, NSLayoutAttribute.Bottom, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(StackView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, ScrollView, NSLayoutAttribute.Width, 1.0f, 0.0f),
                });
        }

        protected void AddArrangedViewsWithSeparators(IEnumerable<UIView> views)
        {
            var viewsArray = views as UIView[] ?? views.ToArray();
            for (int i = 0; i < viewsArray.Count(); i++)
            {
                if (i != 0)
                {
                    StackView.AddArrangedSubview(new SeparatorSubView());
                }

                StackView.AddArrangedSubview(viewsArray[i]);
            }
        }

        protected void CorrectSeparators()
        {
            var views = StackView.ArrangedSubviews;
            for (int i = 0; i < views.Length - 1; i++)
            {
                if (views[i] is StackSubView && views[i + 1] is SeparatorSubView)
                {
                    views[i + 1].Hidden = views[i].Hidden;
                }
            }
        }
    }
}
