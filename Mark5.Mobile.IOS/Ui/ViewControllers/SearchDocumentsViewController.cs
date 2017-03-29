//
// Project: Mark5.Mobile.IOS
// File: SearchDocumentsViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{

    public class SearchDocumentsViewController : AbstractViewController
    {

        UIBarButtonItem closeItem;
        UIBarButtonItem resetItem;
        UIScrollView scrollView;
        UIStackView stackView;

        public override void LoadView()
        {
            base.LoadView();

            AutomaticallyAdjustsScrollViewInsets = true;

            Title = Localization.GetString("search");

            closeItem = new UIBarButtonItem
            {
                Title = Localization.GetString("close")
            };
            NavigationItem.SetLeftBarButtonItem(closeItem, false);

            resetItem = new UIBarButtonItem
            {
                Title = Localization.GetString("reset")
            };
            NavigationItem.SetRightBarButtonItem(resetItem, false);

            scrollView = new UIScrollView
            {
                ShowsVerticalScrollIndicator = false,
                ShowsHorizontalScrollIndicator = false,
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = Theme.DarkerBlue
            };
            View.AddSubview(scrollView);
            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(scrollView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(scrollView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(scrollView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(scrollView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1f, 0f)
            });

            stackView = new UIStackView
            {
                Axis = UILayoutConstraintAxis.Vertical,
                Alignment = UIStackViewAlignment.Top,
                Distribution = UIStackViewDistribution.Fill,
                Spacing = 0f,
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = Theme.DarkerBlue
            };
            scrollView.AddSubview(stackView);
            scrollView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, scrollView, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, scrollView, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, scrollView, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, scrollView, NSLayoutAttribute.Bottom, 1f, 0f)
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ExtendedLayoutIncludesOpaqueBars = true;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            closeItem.Clicked += CloseItem_Clicked;
            resetItem.Clicked += ResetItem_Clicked;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            closeItem.Clicked -= CloseItem_Clicked;
            resetItem.Clicked -= ResetItem_Clicked;
        }

        void CloseItem_Clicked(object sender, EventArgs e) => DismissViewController(true, null);

        void ResetItem_Clicked(object sender, EventArgs e)
        {

        }
    }
}
