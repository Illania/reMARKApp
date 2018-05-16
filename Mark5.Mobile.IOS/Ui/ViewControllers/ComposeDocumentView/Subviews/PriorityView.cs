using System;
using System.Linq;
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
        Priority selectedPriority = Priority.Normal;

        public event EventHandler Edited = delegate { };
        public event EventHandler ActionSheetWillAppear = delegate { };

        readonly WeakReference<UIViewController> weakViewController;

        UILabel label;
        UILabel selectedPriorityLabel;

        public PriorityView(UIViewController viewController)
        {
            weakViewController = new WeakReference<UIViewController>(viewController);
            Initialize();
        }

        void Initialize()
        {
            label = new UILabel
            {
                Text = Localization.GetString("priority") + ": ",
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
                NSLayoutConstraint.Create(label, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1f, HorizontalMargin)
            });

            selectedPriorityLabel = new UILabel
            {
                BackgroundColor = UIColor.Clear,
                Text = UI.PrettyPriorityString(selectedPriority),
                Font = Theme.DefaultFont,
                Opaque = false,
                Lines = 1,
                UserInteractionEnabled = true,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            selectedPriorityLabel.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("PriorityLabelTapped")));
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

        public override Task InitializeView()
        {
            if (RestoreWorkingCopy)
            {
                SelectPriority(DocumentPreview.Priority);
                return Task.CompletedTask;
            }

            if (DocumentCreationModeFlag == DocumentCreationModeFlag.Edit)
            {
                var previousDocumentPriority = PreviousDocumentPreview.Priority;

                if (previousDocumentPriority != Priority.Low && previousDocumentPriority != Priority.Normal && previousDocumentPriority != Priority.Urgent)
                    previousDocumentPriority = Priority.Normal;

                SelectPriority(previousDocumentPriority);
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
        async void PriorityLabelTapped()
        {
            selectedPriorityLabel.TextColor = Theme.TintColor;

            HandleScrollToView(this, EventArgs.Empty);
            ActionSheetWillAppear(this, EventArgs.Empty);

            var priorityStrings = new[]
            {
                UI.PrettyPriorityString(Priority.Urgent),
                UI.PrettyPriorityString(Priority.Normal),
                UI.PrettyPriorityString(Priority.Low)
            };

            if (!weakViewController.TryGetTarget(out UIViewController viewController))
                return;

            var result = await Dialogs.ShowListActionSheetAsync(viewController, priorityStrings, selectedPriorityLabel);
            switch (result)
            {
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
            selectedPriorityLabel.TextColor = Theme.Black;

            selectedPriority = priority;
            selectedPriorityLabel.Text = UI.PrettyPriorityString(priority);

            Edited(this, EventArgs.Empty);
        }

        #endregion
    }
}