using System;
using System.Threading.Tasks;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.SearchView;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class SearchByReferenceViewController: AbstractViewController
    {
        protected readonly TaskCompletionSource<int> tcs = new();
        public Task<int> Result => tcs.Task;

        protected UITextFieldScalable valueTextField;
        protected UILabelScalable label;
        protected UIView containerView;
        UIButtonScalable searchButton;
        UIButtonScalable cancelButton;
        UIView verticalLine;
        UIView horizontalLine;

        NSLayoutConstraint[] sharedConstraints;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            InitializeView();
        }

        protected virtual void InitializeView()
        {
            View.BackgroundColor = UIColor.FromWhiteAlpha(0.3f, 0.5f);

            containerView = new UIView
            {
                BackgroundColor = Theme.White,
                LayoutMargins = new UIEdgeInsets(50f, 50f, 50f, 50f),
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            containerView.Layer.CornerRadius = 20;
            containerView.Layer.MasksToBounds = true;

            View.Add(containerView);

            sharedConstraints = new[]
            {
                    containerView.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
                    containerView.CenterYAnchor.ConstraintEqualTo(View.CenterYAnchor),
                    containerView.WidthAnchor.ConstraintEqualTo(Integration.IsIPhone() ? 300f : 350f),
                };

            View.AddConstraints(sharedConstraints);

            valueTextField = new UITextFieldScalable
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            label = new UILabelScalable
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Text = "Reference number: "
            };

            
            cancelButton = new UIButtonScalable
            {
                ContentEdgeInsets = new UIEdgeInsets(7f, 7f, 7f, 7f),
                TranslatesAutoresizingMaskIntoConstraints = false,
                Enabled = true
            };
            cancelButton.TitleLabel.Font = Theme.DefaultBoldFont.CustomFont();
            cancelButton.SetTitle(Localization.GetString("cancel"), UIControlState.Normal);
            cancelButton.SetTitleColor(Theme.DarkBlue, UIControlState.Normal);
            cancelButton.TitleLabel.Lines = 0;
            cancelButton.TitleLabel.LineBreakMode = UILineBreakMode.WordWrap;

            searchButton = new UIButtonScalable
            {
                ContentEdgeInsets = new UIEdgeInsets(7f, 7f, 7f, 7f),
                TranslatesAutoresizingMaskIntoConstraints = false,
                Enabled = true
            };
            searchButton.TitleLabel.Font = Theme.DefaultFont.CustomFont();
            searchButton.SetTitle(Localization.GetString("search"), UIControlState.Normal);
            searchButton.SetTitleColor(Theme.DarkBlue, UIControlState.Normal);
            searchButton.TitleLabel.Lines = 0;
            searchButton.TitleLabel.LineBreakMode = UILineBreakMode.WordWrap;

            horizontalLine = new UIView
            {
                BackgroundColor = Theme.OpaqueLightGray,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            verticalLine = new UIView
            {
                BackgroundColor = Theme.OpaqueLightGray,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            containerView.AddSubview(label);
            containerView.AddSubview(valueTextField);
            containerView.AddSubview(horizontalLine);
            containerView.AddSubview(cancelButton);
            containerView.AddSubview(verticalLine);
            containerView.AddSubview(searchButton);

            containerView.AddConstraints(new NSLayoutConstraint[]
            {
                    label.TopAnchor.ConstraintEqualTo(containerView.TopAnchor, Integration.IsIPad() ? 30 : 20),
                    label.LeftAnchor.ConstraintEqualTo(containerView.LeftAnchor, Integration.IsIPad() ? 30 : 20),

                    valueTextField.TopAnchor.ConstraintEqualTo(containerView.TopAnchor, Integration.IsIPad() ? 30 : 20),
                    valueTextField.LeftAnchor.ConstraintEqualTo(label.RightAnchor),
                    valueTextField.RightAnchor.ConstraintEqualTo(containerView.RightAnchor, Integration.IsIPad() ? -30 : -20),

                    horizontalLine.TopAnchor.ConstraintEqualTo(valueTextField.BottomAnchor),
                    horizontalLine.LeftAnchor.ConstraintEqualTo(containerView.LeftAnchor),
                    horizontalLine.RightAnchor.ConstraintEqualTo(containerView.RightAnchor),
                    horizontalLine.HeightAnchor.ConstraintEqualTo(1f),

                    verticalLine.TopAnchor.ConstraintEqualTo(horizontalLine.BottomAnchor),
                    verticalLine.CenterXAnchor.ConstraintEqualTo(containerView.CenterXAnchor),
                    verticalLine.BottomAnchor.ConstraintEqualTo(containerView.BottomAnchor),
                    verticalLine.WidthAnchor.ConstraintEqualTo(1f),

                    cancelButton.TopAnchor.ConstraintEqualTo(horizontalLine.BottomAnchor),
                    cancelButton.LeftAnchor.ConstraintEqualTo(containerView.LeftAnchor),
                    cancelButton.RightAnchor.ConstraintEqualTo(verticalLine.LeftAnchor),

                    searchButton.TopAnchor.ConstraintEqualTo(horizontalLine.BottomAnchor),
                    searchButton.LeftAnchor.ConstraintEqualTo(verticalLine.RightAnchor),
                    searchButton.RightAnchor.ConstraintEqualTo(containerView.RightAnchor),

                    containerView.BottomAnchor.ConstraintEqualTo(cancelButton.BottomAnchor)
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            searchButton.TouchUpInside += SearchButton_TouchedUpInside;
            cancelButton.TouchUpInside += CancelButton_TouchedUpInside;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            searchButton.TouchUpInside -= SearchButton_TouchedUpInside;
            cancelButton.TouchUpInside -= CancelButton_TouchedUpInside;
        }

        protected override void Recycle()
        {
            base.Recycle();

            valueTextField = null;
            searchButton = null;
            cancelButton = null;
        }

        protected virtual async void SearchButton_TouchedUpInside(object sender, EventArgs e)
        {
            containerView.RemoveFromSuperview();

            View.BackgroundColor = Theme.Clear;

            var criteria = new Mobile.Common.Model.SearchDocumentsCriteria
            {
                Reference = valueTextField.Text,
                PartialWordSearch = PlatformConfig.Preferences.PartialWordSearch,
                MaxToFetch = PlatformConfig.Preferences.DocumentsToSearch
            };

            CommonConfig.Logger.Info($"Starting search... [criteria={Serializer.Serialize(criteria)}]");
            try
            {
                var vc = new DocumentsSearchByReferenceResultsViewController { Criteria = criteria };
                PresentViewController(new NavigationController(vc, UIModalPresentationStyle.Automatic), true, null);
                var docId = await vc.Result;
                tcs.SetResult(docId);
                DismissViewController(false, null);
            }
            catch (TaskCanceledException ex)
            {
                tcs.SetCanceled();
                DismissViewController(false, null);
            }
             
        }

        void CancelButton_TouchedUpInside(object sender, EventArgs e)
        {
            tcs.SetCanceled();
            DismissViewController(true, null);
        }
    }
}


            
