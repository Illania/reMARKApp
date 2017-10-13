using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;
using ObjCRuntime;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentViews.Subviews
{
    public class LineView : ComposeDocumentSubView
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

        public bool LineSelectedIsAmbiguous => selectedLine == null;

        public LineView(UIViewController viewController)
        {
            this.viewController = viewController;

            defaultOutgoingLine = ServerConfig.SystemSettings.DocumentsModuleInfo.DefaultOutgoingLine;
            availableOutgoingLines = ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines;

            Initialize();
        }

        void Initialize()
        {
            label = new UILabel()
            {
                Text = Localization.GetString("line") + ": ",
                Font = Theme.DefaultFont,
                TextColor = Theme.DarkGray,
                Opaque = false,
                Lines = 0,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            label.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            label.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            label.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContainerView.AddSubview(label);
            ContainerView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(label, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Top, 1f, VerticalMargin),
                NSLayoutConstraint.Create(label, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1f, HorizontalMargin),
                NSLayoutConstraint.Create(label, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Bottom, 1f, -VerticalMargin)
            });

            selectedLineLabel = new UILabel()
            {
                Text = selectedLine == null ? defaultMessage : selectedLine.Name,
                Font = Theme.DefaultFont,
                Opaque = false,
                Lines = 1,
                UserInteractionEnabled = true,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            selectedLineLabel.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("LineLabelTapped")));
            ContainerView.AddSubview(selectedLineLabel);
            ContainerView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(selectedLineLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Top, 1f, VerticalMargin),
                NSLayoutConstraint.Create(selectedLineLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, label, NSLayoutAttribute.Right, 1f, InnerMargin),
                NSLayoutConstraint.Create(selectedLineLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Right, 1f, -HorizontalMargin),
                NSLayoutConstraint.Create(selectedLineLabel, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Bottom, 1f, -VerticalMargin)
            });
        }

        #region Public methods

        public override Task InitializeView()
        {
            if (RestoreWorkingCopy)
            {
                SetLine(Document.Lines.FirstOrDefault());
                return Task.CompletedTask;
            }

            if (DocumentCreationModeFlag == DocumentCreationModeFlag.New)
                SetLine(defaultOutgoingLine);

            if (DocumentCreationModeFlag == DocumentCreationModeFlag.Edit)
                SetLine(PreviousDocument.Lines.FirstOrDefault());

            if (DocumentCreationModeFlag == DocumentCreationModeFlag.Reply ||
                DocumentCreationModeFlag == DocumentCreationModeFlag.ReplyAll ||
                DocumentCreationModeFlag == DocumentCreationModeFlag.Forward)
            {
                if (availableOutgoingLines.Count == 1)
                {
                    SetLine(availableOutgoingLines.FirstOrDefault());
                    return Task.CompletedTask;
                }

                if (PreviousDocument.Lines.FirstOrDefault(l => l.Guid == defaultOutgoingLine?.Guid) != null)
                    SetLine(defaultOutgoingLine);
                else
                {
                    var intersection = PreviousDocument.Lines.Intersect(availableOutgoingLines, LambdaEqualityComparer<Line>.Create(l => l.Guid)).ToArray();
                    if (intersection.Length == 1)
                        SetLine(intersection.FirstOrDefault());
                    else
                        SetLine(null);
                }
            }

            return Task.CompletedTask;
        }

        public override Task UpdateDocument()
        {
            Document.Lines.Clear();
            Document.Lines.Add(selectedLine);
            return Task.CompletedTask;
        }

        public void SetLineFromGuid(Guid lineGuid)
        {
            var line = availableOutgoingLines.FirstOrDefault(l => l.Guid == lineGuid);
            if (line != null)
                SetLine(line);
        }

        public Line GetLine()
        {
            return selectedLine;
        }

        #endregion

        #region Helper methods

        void SetLine(Line line)
        {
            selectedLineLabel.TextColor = Theme.Black;

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
        async void LineLabelTapped()
        {
            selectedLineLabel.TextColor = Theme.TintColor;

            HandleScrollToView(this, EventArgs.Empty);
            ActionSheetWillAppear(this, EventArgs.Empty);

            var lineNames = availableOutgoingLines.Select(l => l.Name).ToArray();
            var result = await Dialogs.ShowListDialogAsync(viewController, null, lineNames, selectedLineLabel);

            if (result >= 0)
                SetLine(availableOutgoingLines[result]);
        }

        #endregion
    }
}