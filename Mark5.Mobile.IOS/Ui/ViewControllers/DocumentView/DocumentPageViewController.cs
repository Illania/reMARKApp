using System;
using System.Collections.Generic;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView
{
    public class DocumentPageViewController : AbstractPageViewController, IUIPageViewControllerDataSource
    {
        public Folder Folder { get; set; }
        public DocumentPreview CurrentDocumentPreview { get; set; }
        public List<DocumentPreview> DocumentPreviews { get; set; }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            if (Integration.IsRunningAtLeast(11))
                NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Never;

            WeakDataSource = this;

            SetViewControllers(new [] { GetDocumentViewController(Folder, CurrentDocumentPreview) }, UIPageViewControllerNavigationDirection.Forward, false, null);
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning("Received memory warning!");

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        protected override void Recycle()
        {
            base.Recycle();

            WeakDataSource = null;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (CommonConfig.Logger.IsDebugEnabled())
                CommonConfig.Logger.Debug("Disposed");
        }

        UIViewController IUIPageViewControllerDataSource.GetNextViewController(UIPageViewController pageViewController, UIViewController referenceViewController)
        {
            var referenceDocumentViewController = (DocumentViewController)referenceViewController;
            var referenceDocumentPreview = referenceDocumentViewController.DocumentPreview;
            var index = DocumentPreviews.FindIndex(dp => dp.Id == referenceDocumentPreview.Id);
            if (index < 0 || index >= DocumentPreviews.Count)
                return null;

            var nextDocumentPreview = DocumentPreviews[index + 1];
            return GetDocumentViewController(Folder, nextDocumentPreview);
        }

        UIViewController IUIPageViewControllerDataSource.GetPreviousViewController(UIPageViewController pageViewController, UIViewController referenceViewController)
        {
            var referenceDocumentViewController = (DocumentViewController)referenceViewController;
            var referenceDocumentPreview = referenceDocumentViewController.DocumentPreview;
            var index = DocumentPreviews.FindIndex(dp => dp.Id == referenceDocumentPreview.Id);
            if (index < 1)
                return null;

            var nextDocumentPreview = DocumentPreviews[index - 1];
            return GetDocumentViewController(Folder, nextDocumentPreview);
        }

        static DocumentViewController GetDocumentViewController(Folder folder, DocumentPreview documentPreview)
        {
            var vc = new DocumentViewController();
            vc.SetData(folder, documentPreview);
            vc.SetRefreshDataOnAppear();
            return vc;
        }
    }
}