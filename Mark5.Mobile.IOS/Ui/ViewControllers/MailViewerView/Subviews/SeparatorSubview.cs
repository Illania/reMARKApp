using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.MailViewerView.Subviews
{
    public class SeparatorSubView : MailViewerSubview
    {
        const float SeparatorVerticalMargin = 12f;

        public SeparatorSubView()
        {
            Initialize();
        }

        void Initialize()
        {
            TranslatesAutoresizingMaskIntoConstraints = false;

            var line = new UIView
            {
                BackgroundColor = Theme.DarkBlue,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            AddSubview(line);
            var constraints = new[]
            {
                line.TopAnchor.ConstraintEqualTo(this.TopAnchor, SeparatorVerticalMargin),
                line.LeftAnchor.ConstraintEqualTo(this.LeftAnchor),
                line.RightAnchor.ConstraintEqualTo(this.RightAnchor),
                line.BottomAnchor.ConstraintEqualTo(this.BottomAnchor, -SeparatorVerticalMargin),
                line.HeightAnchor.ConstraintEqualTo(1f)
            };
            foreach (var constraint in constraints)
                constraint.Priority = 500;

            AddConstraints(constraints);
        }

        public override void RefreshView()
        {
        }

        public override void UpdateVisibility()
        {
            Hidden = false;
        }

    }
}
