//
// Project: Mark5.Mobile.IOS
// File: PriorityView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
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
            AddSubview(label);
            AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(label, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1.0f, VerticalMargin),
                    NSLayoutConstraint.Create(label, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1.0f, HorizontalMargin),
                });

            selectedPriorityLabel = new UILabel();
            selectedPriorityLabel.Text = GetPriorityText(Priority.Normal);
            selectedPriorityLabel.Font = Theme.DefaultFont;
            selectedPriorityLabel.Opaque = false;
            selectedPriorityLabel.Lines = 1;
            selectedPriorityLabel.UserInteractionEnabled = true;
            selectedPriorityLabel.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("PriorityLabelTapped")));
            selectedPriorityLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            AddSubview(selectedPriorityLabel);
            AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(selectedPriorityLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1.0f, VerticalMargin),
                    NSLayoutConstraint.Create(selectedPriorityLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, label, NSLayoutAttribute.Right, 1.0f, InnerMargin),
                    NSLayoutConstraint.Create(selectedPriorityLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1.0f, -HorizontalMargin),
                    NSLayoutConstraint.Create(selectedPriorityLabel, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this, NSLayoutAttribute.Bottom, 1.0f, -VerticalMargin),
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
        void PriorityLabelTapped()
        {
            selectedPriorityLabel.TextColor = Theme.TintColor;

            HandleScrollToView(this, EventArgs.Empty);
            ActionSheetWillAppear(this, EventArgs.Empty);

            var selectPriorityActionSheet = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            selectPriorityActionSheet.AddAction(UIAlertAction.Create(GetPriorityText(Priority.Urgent), UIAlertActionStyle.Default, a => SelectPriority(Priority.Urgent)));
            selectPriorityActionSheet.AddAction(UIAlertAction.Create(GetPriorityText(Priority.Normal), UIAlertActionStyle.Default, a => SelectPriority(Priority.Normal)));
            selectPriorityActionSheet.AddAction(UIAlertAction.Create(GetPriorityText(Priority.Low), UIAlertActionStyle.Default, a => SelectPriority(Priority.Low)));
            selectPriorityActionSheet.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, a => selectedPriorityLabel.TextColor = UIColor.DarkTextColor));
            if (selectPriorityActionSheet.PopoverPresentationController != null)
            {
                //selectPriorityActionSheet.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate(selectedPriorityLabel); //TODO
            }
            viewController.PresentViewController(selectPriorityActionSheet, true, null);
        }

        #endregion

        #region Helper methods

        static string GetPriorityText(Priority priority)
        {
            switch (priority)
            {
                case Priority.Low:
                    return Localization.GetString("low");
                case Priority.Normal:
                    return Localization.GetString("normal");
                case Priority.Urgent:
                    return Localization.GetString("urgent");
                default:
                    throw new ArgumentException(string.Format("Unknown priority. [priority={0}]", priority));
            }
        }

        void SelectPriority(Priority priority)
        {
            selectedPriorityLabel.TextColor = UIColor.DarkTextColor;

            selectedPriority = priority;
            selectedPriorityLabel.Text = GetPriorityText(priority);

            Edited(this, EventArgs.Empty);
        }

        #endregion

    }
}
