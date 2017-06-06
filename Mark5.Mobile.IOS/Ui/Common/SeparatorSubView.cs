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
            var line = new UIView();
            line.BackgroundColor = backgroundColor;
            line.TranslatesAutoresizingMaskIntoConstraints = false;
            AddSubview(line);
            var constraints = new[]
            {
                NSLayoutConstraint.Create(line, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(line, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1f, 15f),
                NSLayoutConstraint.Create(line, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(line, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(line, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, 0.5f),
            };
            foreach (var constraint in constraints)
                constraint.Priority = 500;

            AddConstraints(constraints);
        }
    }
}