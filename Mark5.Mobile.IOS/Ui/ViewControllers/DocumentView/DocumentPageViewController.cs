using System;
using System.Collections.Generic;
using System.Linq;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView
{
    public class DocumentPageViewController : AbstractPageViewController, IUIPageViewControllerDataSource
    {
        const int CacheCapacity = 5; // Going below 5 might cause issues with internal caching of
                                     // UIPageViewController

        public Folder Folder { get; set; }
        public DocumentPreview InitialDocumentPreview { get; set; }
        public List<DocumentPreview> DocumentPreviews { get; set; }

        readonly List<DocumentViewController> viewControllerCache = new List<DocumentViewController>(CacheCapacity + 1);

        public DocumentPageViewController()
        {
            HidesBottomBarWhenPushed = true;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            if (Integration.IsRunningAtLeast(11))
                NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Never;

            WeakDataSource = this;

            WillTransition += DocumentPageViewController_WillTransition;
            DidFinishAnimating += DocumentPageViewController_DidFinishAnimating;

            var vc = GetDocumentViewController(Folder, InitialDocumentPreview);
            SetViewControllers(new[] { vc }, UIPageViewControllerNavigationDirection.Forward, false, null);
            ToolbarItems = vc.ToolbarItems;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (NavigationController != null)
                NavigationController.ToolbarHidden = false;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            if (NavigationController != null)
                NavigationController.ToolbarHidden = true;
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

            WillTransition -= DocumentPageViewController_WillTransition;
            DidFinishAnimating -= DocumentPageViewController_DidFinishAnimating;

            foreach (var cachedViewController in viewControllerCache)
                cachedViewController.RecycleIfNeeded();
            viewControllerCache.Clear();

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

            var previousDocumentPreview = DocumentPreviews[index - 1];
            return GetDocumentViewController(Folder, previousDocumentPreview);
        }

        void DocumentPageViewController_WillTransition(object sender, UIPageViewControllerTransitionEventArgs e)
        {
            SetToolbarItems(null, true);
        }

        void DocumentPageViewController_DidFinishAnimating(object sender, UIPageViewFinishedAnimationEventArgs e)
        {
            var vc = ViewControllers.FirstOrDefault()?.ToolbarItems;
            SetToolbarItems(vc, true);
        }

        DocumentViewController GetDocumentViewController(Folder folder, DocumentPreview documentPreview)
        {
            var cachedViewController = viewControllerCache.FirstOrDefault(dvc => dvc.DocumentPreview.Id == documentPreview.Id);
            if (cachedViewController != null)
                return cachedViewController;

            var vc = new DocumentViewController();
            vc.SetData(folder, documentPreview);
            vc.SetRefreshDataOnAppear();
            viewControllerCache.Add(vc);

            if (viewControllerCache.Count > CacheCapacity)
            {
                viewControllerCache[0].RecycleIfNeeded();
                viewControllerCache.RemoveAt(0);
            }

            return vc;
        }
    }
}