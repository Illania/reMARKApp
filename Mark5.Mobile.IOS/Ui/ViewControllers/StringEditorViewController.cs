using System;
using System.Threading.Tasks;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class StringEditorViewController: AbstractViewController
    {
        protected readonly TaskCompletionSource<string> tcs = new();
        public Task<string> Result => tcs.Task;

        protected UITextField valueTextField;
        protected UIView containerView;
        UIButtonScalable okButton;
        UIButtonScalable cancelButton;
        UIView verticalLine;
        UIView horizontalLine;

        NSLayoutConstraint[] sharedConstraints;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            InitializeView();
        }

        virtual protected void InitializeView()
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

            valueTextField = new UITextField
            {
                TranslatesAutoresizingMaskIntoConstraints = false
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

            okButton = new UIButtonScalable
            {
                ContentEdgeInsets = new UIEdgeInsets(7f, 7f, 7f, 7f),
                TranslatesAutoresizingMaskIntoConstraints = false,
                Enabled = true
            };
            okButton.TitleLabel.Font = Theme.DefaultFont.CustomFont();
            okButton.SetTitle(Localization.GetString("ok"), UIControlState.Normal);
            okButton.SetTitleColor(Theme.DarkBlue, UIControlState.Normal);
            okButton.TitleLabel.Lines = 0;
            okButton.TitleLabel.LineBreakMode = UILineBreakMode.WordWrap;

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

            containerView.AddSubview(valueTextField);
            containerView.AddSubview(horizontalLine);
            containerView.AddSubview(cancelButton);
            containerView.AddSubview(verticalLine);
            containerView.AddSubview(okButton);

            containerView.AddConstraints(new NSLayoutConstraint[]
            {
                    valueTextField.TopAnchor.ConstraintEqualTo(containerView.TopAnchor, Integration.IsIPad() ? 30 : 20),
                    valueTextField.LeftAnchor.ConstraintEqualTo(containerView.LeftAnchor, Integration.IsIPad() ? 30 : 20),
                    valueTextField.RightAnchor.ConstraintEqualTo(containerView.RightAnchor, Integration.IsIPad() ? -30 : -20),
                    valueTextField.CenterXAnchor.ConstraintEqualTo(containerView.CenterXAnchor),

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

                    okButton.TopAnchor.ConstraintEqualTo(horizontalLine.BottomAnchor),
                    okButton.LeftAnchor.ConstraintEqualTo(verticalLine.RightAnchor),
                    okButton.RightAnchor.ConstraintEqualTo(containerView.RightAnchor),

                    containerView.BottomAnchor.ConstraintEqualTo(cancelButton.BottomAnchor)
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            okButton.TouchUpInside += OkButton_TouchedUpInside;
            cancelButton.TouchUpInside += CancelButton_TouchedUpInside;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            okButton.TouchUpInside -= OkButton_TouchedUpInside;
            cancelButton.TouchUpInside -= CancelButton_TouchedUpInside;
        }

        protected override void Recycle()
        {
            base.Recycle();

            valueTextField = null;
            okButton = null;
            cancelButton = null;
        }

        virtual protected async void OkButton_TouchedUpInside(object sender, EventArgs e)
        {
            containerView.RemoveFromSuperview();

            View.BackgroundColor = Theme.Clear;

        
            var updateConfirmed = await Dialogs.ShowYesNoAlertAsync(this, Localization.GetString("confirm_extra_field_update_title"),
                                                                        string.Format(Localization.GetString("confirm_extra_field_content"),
                                                                        valueTextField.Text));

            if (updateConfirmed)
                tcs.SetResult(valueTextField.Text);
            else
                tcs.SetCanceled();
            

            DismissViewController(true, null);
        }

        void CancelButton_TouchedUpInside(object sender, EventArgs e)
        {
            tcs.SetCanceled();
            DismissViewController(true, null);
        }
    }
}


            
