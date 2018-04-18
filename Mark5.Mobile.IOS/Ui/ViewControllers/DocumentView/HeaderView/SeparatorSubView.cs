using System;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.HeaderView
{
    public class SeparatorSubView : UIView
    {
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
                NSLayoutConstraint.Create(line, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(line, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(line, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(line, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(line, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, 1f),
            };
            foreach (var constraint in constraints)
                constraint.Priority = 500;

            AddConstraints(constraints);
        }
    }
}
