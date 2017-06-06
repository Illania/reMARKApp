using System.Collections.Generic;
using System.Linq;
using UIKit;

namespace Mark5.Mobile.IOS.Utilities
{
    public static class UiViewExtension
    {
        public static Dictionary<UIView, NSLayoutConstraint[]> BackupConstaints(this UIView view)
        {
            var backup = new Dictionary<UIView, NSLayoutConstraint[]>();
            if (view != null)
            {
                if (view.Constraints != null && view.Constraints.Length > 0)
                {
                    var relevant = view.Constraints.Where(IsConstraintRelevant).ToArray();
                    if (relevant.Length > 0)
                    {
                        backup.Add(view, relevant.UniversalDeepCopy());
                        view.RemoveConstraints(relevant);
                    }
                }

                if (view.Subviews != null && view.Subviews.Length > 0)
                    foreach (var subview in view.Subviews)
                    {
                        var subviewBackup = subview.BackupConstaints();
                        backup = backup.Concat(subviewBackup).ToDictionary(entry => entry.Key, entry => entry.Value);
                    }
            }

            return backup;
        }

        public static void RestoreConstaints(this UIView view, Dictionary<UIView, NSLayoutConstraint[]> constraints)
        {
            if (view != null)
            {
                NSLayoutConstraint[] subviewConstraints;
                if (constraints.TryGetValue(view, out subviewConstraints))
                {
                    if (view.Constraints != null && view.Constraints.Length > 0)
                        view.RemoveConstraints(view.Constraints.Where(IsConstraintRelevant).ToArray());

                    view.AddConstraints(subviewConstraints);
                }

                if (view.Subviews != null && view.Subviews.Length > 0)
                    foreach (var subview in view.Subviews)
                        subview.RestoreConstaints(constraints);
            }
        }

        public static bool IsVisible(this UIView view)
        {
            return !view.Hidden && view.Alpha > 0f;
        }

        #region Helper methods

        static bool IsConstraintRelevant(NSLayoutConstraint constraint)
        {
            var attributes = new[]
            {
                NSLayoutAttribute.Top,
                NSLayoutAttribute.Bottom,
                NSLayoutAttribute.Height,
                NSLayoutAttribute.CenterY,
                NSLayoutAttribute.Baseline
            };
            return attributes.Contains(constraint.FirstAttribute) || attributes.Contains(constraint.SecondAttribute);
        }

        #endregion
    }
}