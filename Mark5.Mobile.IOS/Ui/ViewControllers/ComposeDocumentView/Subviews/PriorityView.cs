//
// Project: Mark5.Mobile.IOS
// File: PriorityView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using ObjCRuntime;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentViews.Subviews
{
    public class PriorityView : ComposeDocumentSubView
    {
        Priority selectedPriority;

        public event EventHandler Edited = delegate { };
        public event EventHandler ActionSheetWillAppear = delegate { };

        UIViewController viewController;

        UILabel label;
        UILabel selectedPriorityLabel;

        public PriorityView(UIViewController viewController)
        {
            this.viewController = viewController;
            Initialize();
        }

        void Initialize()
        {
            label = new UILabel();
            label.Text = Localization.GetString("priority") + ": ";
            label.Font = Theme.DefaultFont;
            label.TextColor = UIColor.LightGray;
            label.Opaque = false;
            label.Lines = 0;
            label.TranslatesAutoresizingMaskIntoConstraints = false;
            label.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            label.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            label.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContainerView.AddSubview(label);
            ContainerView.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(label, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Top, 1f, VerticalMargin),
                    NSLayoutConstraint.Create(label, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1f, HorizontalMargin)
                });

            selectedPriorityLabel = new UILabel();
            selectedPriorityLabel.Text = UI.PriorityString(Priority.Normal);
            selectedPriorityLabel.Font = Theme.DefaultFont;
            selectedPriorityLabel.Opaque = false;
            selectedPriorityLabel.Lines = 1;
            selectedPriorityLabel.UserInteractionEnabled = true;
            selectedPriorityLabel.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("PriorityLabelTapped")));
            selectedPriorityLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            ContainerView.AddSubview(selectedPriorityLabel);
            ContainerView.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(selectedPriorityLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Top, 1f, VerticalMargin),
                    NSLayoutConstraint.Create(selectedPriorityLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, label, NSLayoutAttribute.Right, 1f, InnerMargin),
                    NSLayoutConstraint.Create(selectedPriorityLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Right, 1f, -HorizontalMargin),
                    NSLayoutConstraint.Create(selectedPriorityLabel, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Bottom, 1f, -VerticalMargin)
                });
        }

        #region Overrides

        public override Task RefreshView()
        {
            if (CreationModeFlag == DocumentCreationModeFlag.Edit)
            {
                SelectPriority(PreviousDocumentPreview.Priority);
            }

            return Task.CompletedTask;
        }

        public override Task UpdateDocument()
        {
            DocumentPreview.Priority = selectedPriority;
            return Task.CompletedTask;
        }

        #endregion

        #region Event handlers

        [Export("PriorityLabelTapped")]
#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void PriorityLabelTapped()
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            selectedPriorityLabel.TextColor = Theme.TintColor;

            HandleScrollToView(this, EventArgs.Empty);
            ActionSheetWillAppear(this, EventArgs.Empty);

            var templateListStrings = new string[] { UI.PriorityString(Priority.Urgent), UI.PriorityString(Priority.Normal), UI.PriorityString(Priority.Low) };

            var result = await Dialogs.ShowListDialogAsync(viewController, null, templateListStrings, selectedPriorityLabel);
            switch (result)
            {
                case -1:
                    break;
                case 0:
                    SelectPriority(Priority.Urgent);
                    break;
                case 1:
                    SelectPriority(Priority.Normal);
                    break;
                case 2:
                    SelectPriority(Priority.Low);
                    break;
            }
        }

        #endregion

        #region Helper methods

        void SelectPriority(Priority priority)
        {
            selectedPriorityLabel.TextColor = UIColor.DarkTextColor;

            selectedPriority = priority;
            selectedPriorityLabel.Text = UI.PriorityString(priority);

            Edited(this, EventArgs.Empty);
        }

        #endregion

    }
}
