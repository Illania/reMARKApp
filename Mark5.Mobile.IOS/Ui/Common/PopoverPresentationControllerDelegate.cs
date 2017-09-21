using UIKit;
using System;
using Mark5.Mobile.Common.Utilities.Extensions;
using CoreGraphics;

namespace Mark5.Mobile.IOS.Ui.Common
{
    public class PopoverPresentationControllerDelegate : UIPopoverPresentationControllerDelegate
    {
        readonly WeakReference<UIBarButtonItem> barButtonItemWeakReference;
        readonly WeakReference<UIView> sourceViewWeakReference;
        readonly WeakReference<UITableView> tableViewWeakReference;
        readonly WeakReference<UITableViewCell> cellWeakReference;

        public PopoverPresentationControllerDelegate(UIBarButtonItem barButtonItem)
        {
            barButtonItemWeakReference = new WeakReference<UIBarButtonItem>(barButtonItem);
        }

        public PopoverPresentationControllerDelegate(UIView sourceView)
        {
            sourceViewWeakReference = new WeakReference<UIView>(sourceView);
        }

        public PopoverPresentationControllerDelegate(UITableView tableView, UITableViewCell cell)
        {
            tableViewWeakReference = new WeakReference<UITableView>(tableView);
            cellWeakReference = new WeakReference<UITableViewCell>(cell);
        }

        public override void PrepareForPopoverPresentation(UIPopoverPresentationController popoverPresentationController)
        {
            if (barButtonItemWeakReference != null)
                popoverPresentationController.BarButtonItem = barButtonItemWeakReference.Unwrap();

            if (sourceViewWeakReference != null)
            {
                popoverPresentationController.SourceView = sourceViewWeakReference.Unwrap();
                popoverPresentationController.SourceRect = sourceViewWeakReference.Unwrap()?.Frame ?? CGRect.Empty;
            }

            if (tableViewWeakReference != null && cellWeakReference != null)
            {
                var indexPath = tableViewWeakReference.Unwrap()?.IndexPathForCell(cellWeakReference.Unwrap());
                var rect = tableViewWeakReference.Unwrap()?.RectForRowAtIndexPath(indexPath);

                popoverPresentationController.SourceView = tableViewWeakReference.Unwrap();
                popoverPresentationController.SourceRect = rect ?? CGRect.Empty;
            }
        }
    }
}