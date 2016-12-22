//
// Project: Mark5.Mobile.IOS
// File: PopoverPresentationControllerDelegate.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
{
    public class PopoverPresentationControllerDelegate : UIPopoverPresentationControllerDelegate
    {

        readonly UIBarButtonItem barButtonItem;
        readonly UIView sourceView;
        readonly UITableView tableView;
        readonly UITableViewCell cell;

        public PopoverPresentationControllerDelegate(UIBarButtonItem barButtonItem)
        {
            this.barButtonItem = barButtonItem;
        }

        public PopoverPresentationControllerDelegate(UIView sourceView)
        {
            this.sourceView = sourceView;
        }

        public PopoverPresentationControllerDelegate(UITableView tableView, UITableViewCell cell)
        {
            this.tableView = tableView;
            this.cell = cell;
        }

        public override void PrepareForPopoverPresentation(UIPopoverPresentationController popoverPresentationController)
        {
            if (barButtonItem != null)
            {
                popoverPresentationController.BarButtonItem = barButtonItem;
            }

            if (sourceView != null)
            {
                popoverPresentationController.SourceView = sourceView;
                popoverPresentationController.SourceRect = sourceView.Frame;
            }

            if (tableView != null && cell != null)
            {
                var indexPath = tableView.IndexPathForCell(cell);
                var rect = tableView.RectForRowAtIndexPath(indexPath);

                popoverPresentationController.SourceView = tableView;
                popoverPresentationController.SourceRect = rect;
            }
        }
    }
}
