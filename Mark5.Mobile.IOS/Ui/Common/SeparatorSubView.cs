using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
{
    public class SeparatorSubView : UIView
    {
        static readonly UIColor backgroundColor = new UITableView().SeparatorColor;

        public SeparatorSubView()
        {
            Initialize();
        }

        void Initialize()
        {
            var line = new UIView
            {
                BackgroundColor = backgroundColor,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            AddSubview(line);
            var constraints = new[]
            {
                line.TopAnchor.ConstraintEqualTo(this.TopAnchor),
                line.LeftAnchor.ConstraintEqualTo(this.LeftAnchor, 15f),
                line.RightAnchor.ConstraintEqualTo(this.RightAnchor),
                line.BottomAnchor.ConstraintEqualTo(this.BottomAnchor),
                line.HeightAnchor.ConstraintEqualTo(1f)
            };
            foreach (var constraint in constraints)
                constraint.Priority = 500;

            AddConstraints(constraints);
        }
    }
}