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
            barButtonItemWeakReference = barButtonItem.Wrap();
        }

        public PopoverPresentationControllerDelegate(UIView sourceView)
        {
            sourceViewWeakReference = sourceView.Wrap();
        }

        public PopoverPresentationControllerDelegate(UITableView tableView, UITableViewCell cell)
        {
            tableViewWeakReference = tableView.Wrap();
            cellWeakReference = cell.Wrap();
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