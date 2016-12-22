//
// Project: Mark5.Mobile.IOS
// File: LineView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;
using ObjCRuntime;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentViews.Subviews
{
    public class LineView : ComposeDocumentView
    {
        string defaultMessage = Localization.GetString("tap_select_line");

        Line selectedLine;

        public event EventHandler Edited = delegate { };
        public event EventHandler ActionSheetWillAppear = delegate { };

        UILabel label;
        UILabel selectedLineLabel;

        readonly UIViewController viewController;
        readonly Line defaultOutgoingLine;
        readonly List<Line> availableOutgoingLines;

        public LineView(UIViewController viewController)
        {
            this.viewController = viewController;

            defaultOutgoingLine = ServerConfig.SystemSettings.DocumentsModuleInfo.DefaultOutgoingLine;
            availableOutgoingLines = ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines;

            Initialize();
        }

        void Initialize()
        {
            label = new UILabel();
            label.Text = Localization.GetString("line") + ": ";
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
                    NSLayoutConstraint.Create(label, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this, NSLayoutAttribute.Bottom, 1.0f, -VerticalMargin)
                });

            selectedLineLabel = new UILabel();
            selectedLineLabel.Text = selectedLine == null ? defaultMessage : selectedLine.Name;
            selectedLineLabel.Font = Theme.DefaultFont;
            selectedLineLabel.Opaque = false;
            selectedLineLabel.Lines = 1;
            selectedLineLabel.UserInteractionEnabled = true;
            selectedLineLabel.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("LineLabelTapped")));
            selectedLineLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            AddSubview(selectedLineLabel);
            AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(selectedLineLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1.0f, VerticalMargin),
                    NSLayoutConstraint.Create(selectedLineLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, label, NSLayoutAttribute.Right, 1.0f, InnerMargin),
                    NSLayoutConstraint.Create(selectedLineLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1.0f, -HorizontalMargin),
                    NSLayoutConstraint.Create(selectedLineLabel, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this, NSLayoutAttribute.Bottom, 1.0f, -VerticalMargin)
                });
        }

        #region Overrides

        public override Task RefreshView()
        {
            if (CreationModeFlag == DocumentCreationModeFlag.New)
            {
                if (defaultOutgoingLine != null)
                {
                    SetLine(defaultOutgoingLine);
                }
                else
                {
                    SetLine(null);
                }
                return Task.CompletedTask;
            }

            if (CreationModeFlag == DocumentCreationModeFlag.None)
            {
                return Task.CompletedTask;
            }

            if (CreationModeFlag == DocumentCreationModeFlag.Edit)
            {
                SetLine(PreviousDocument.Lines.First());
            }

            if (availableOutgoingLines.Count == 1)
            {
                SetLine(availableOutgoingLines.First());
                return Task.CompletedTask;
            }

            var previousDocumentLines = PreviousDocument.Lines;
            if (previousDocumentLines.Contains(defaultOutgoingLine))
            {
                SetLine(defaultOutgoingLine);
            }
            else
            {
                var intersection = previousDocumentLines.Intersect(availableOutgoingLines, LambdaEqualityComparer<Line>.Create(l => l.Guid)).ToList();
                if (intersection.Count() == 1)
                {
                    SetLine(intersection.First());
                }
                else
                {
                    SetLine(null);
                }
            }

            return Task.CompletedTask;
        }

        public override Task UpdateDocument()
        {
            Document.Lines.Add(selectedLine);
            return Task.CompletedTask;  
        }

        #endregion

        #region Helpet methods

        void SetLine(Line line)
        {
            selectedLineLabel.TextColor = UIColor.DarkTextColor;

            if (line != null && availableOutgoingLines.Select(l => l.Guid).Contains(line.Guid))
            {
                selectedLine = line;
                selectedLineLabel.Text = line.Name;
            }
            else
            {
                selectedLine = null;
                selectedLineLabel.Text = defaultMessage;
            }

            Edited(this, EventArgs.Empty);
        }

        #endregion

        #region Event handlers

        [Export("LineLabelTapped")]
        void LineLabelTapped()
        {
            selectedLineLabel.TextColor = Theme.TintColor;

            HandleScrollToView(this, EventArgs.Empty);
            ActionSheetWillAppear(this, EventArgs.Empty);

            var linesActionSheet = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            foreach (var line in availableOutgoingLines)
            {
                linesActionSheet.AddAction(UIAlertAction.Create(line.Name, UIAlertActionStyle.Default, a => SetLine(line)));
            }
            linesActionSheet.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, a => selectedLineLabel.TextColor = UIColor.DarkTextColor));
            if (linesActionSheet.PopoverPresentationController != null)
            {
                //linesActionSheet.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate(selectedLineLabel); //TODO
            }
            viewController.PresentViewController(linesActionSheet, true, null);
        }

        #endregion

    }
}
