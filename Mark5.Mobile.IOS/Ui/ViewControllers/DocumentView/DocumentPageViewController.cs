using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView
{
    public class DocumentPageViewController : AbstractPageViewController
    {
        UIBarButtonItem previousDocumentButtonItem;
        UIBarButtonItem nextDocumentButtonItem;
        UIBarButtonItem editDocumentButtonItem;

        const int CacheCapacity = 5; // Going below 5 might cause issues with internal caching of
                                     // UIPageViewController

        public Folder Folder { get; set; }
        public DocumentPreview InitialDocumentPreview { get; set; }
        public List<DocumentPreview> DocumentPreviews { get; set; }

        int currentDocumentId;

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

            WillTransition += DocumentPageViewController_WillTransition;
            DidFinishAnimating += DocumentPageViewController_DidFinishAnimating;

            var vc = GetDocumentViewController(Folder, InitialDocumentPreview);
            currentDocumentId = InitialDocumentPreview.Id;
            SetViewControllers(new[] { vc }, UIPageViewControllerNavigationDirection.Forward, false, null);
            ToolbarItems = vc.ToolbarItems;

            InitNavigationBar();
            UpdateNavigationBar(vc, InitialDocumentPreview);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (NavigationController != null)
                NavigationController.ToolbarHidden = false;

            InitializeHandlers();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            if (NavigationController != null)
                NavigationController.ToolbarHidden = true;

            DeinitializeHandlers();
        }

        void InitializeHandlers()
        {
            if (nextDocumentButtonItem != null)
                nextDocumentButtonItem.Clicked += NextDocumentButton_Clicked;
            if (previousDocumentButtonItem != null)
                previousDocumentButtonItem.Clicked += PreviousDocumentButton_Clicked;
            if (editDocumentButtonItem != null)
                editDocumentButtonItem.Clicked += EditDocumentButtonItem_Clicked;
        }

        void DeinitializeHandlers()
        {
            if (nextDocumentButtonItem != null)
                nextDocumentButtonItem.Clicked -= NextDocumentButton_Clicked;
            if (previousDocumentButtonItem != null)
                previousDocumentButtonItem.Clicked -= PreviousDocumentButton_Clicked;
            if (editDocumentButtonItem != null)
                editDocumentButtonItem.Clicked -= EditDocumentButtonItem_Clicked;
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning("Received memory warning!");

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        void InitNavigationBar()
        {
            nextDocumentButtonItem = null;
            previousDocumentButtonItem = null;

            nextDocumentButtonItem = new UIBarButtonItem
            {
                Image = UIImage.FromBundle(Path.Combine("icons", "arrow-down.png")),
                Enabled = true
            };

            previousDocumentButtonItem = new UIBarButtonItem
            {
                Image = UIImage.FromBundle(Path.Combine("icons", "arrow-up.png")),
                Enabled = true
            };

            editDocumentButtonItem = new UIBarButtonItem
            {
                Image = UIImage.FromBundle(Path.Combine("icons", "edit.png"))
            };
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

        void DocumentPageViewController_WillTransition(object sender, UIPageViewControllerTransitionEventArgs e)
        {
            CommonConfig.UsageAnalytics.LogEvent(new DocumentQuickSwitchEvent());
            SetToolbarItems(null, true);
        }

        void DocumentPageViewController_DidFinishAnimating(object sender, UIPageViewFinishedAnimationEventArgs e)
        {
            var vc = (DocumentViewController)ViewControllers.FirstOrDefault();
            var ti = vc?.ToolbarItems;
            currentDocumentId = vc.DocumentPreview.Id;
            SetToolbarItems(ti, true);
        }

        DocumentViewController GetDocumentViewController(Folder folder, DocumentPreview documentPreview)
        {
            var cachedViewController = viewControllerCache.FirstOrDefault(dvc => dvc.DocumentPreview.Id == documentPreview.Id);
            if (cachedViewController != null)
            {
                //cachedViewController.SetRefreshDataOnAppear();
                return cachedViewController;
            }

            var vc = new DocumentViewController();
            vc.DisableRecyclingOnDisappear();
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

        #region Navigation Bar handlers

        void UpdateNavigationBar(DocumentViewController dvc, DocumentPreview documentPreview)
        {
            nextDocumentButtonItem.Enabled = HasNext(documentPreview);
            previousDocumentButtonItem.Enabled = HasPrevious(documentPreview);

            if (documentPreview.Direction == DocumentDirection.Draft)
            {
                var rightButtons = new UIBarButtonItem[3];
                rightButtons[0] = nextDocumentButtonItem;
                rightButtons[1] = previousDocumentButtonItem;
                rightButtons[2] = editDocumentButtonItem;

                NavigationItem.SetRightBarButtonItems(rightButtons, false);
            }
            else
            {
                var rightButtons = new UIBarButtonItem[2];
                rightButtons[0] = nextDocumentButtonItem;
                rightButtons[1] = previousDocumentButtonItem;
                NavigationItem.SetRightBarButtonItems(rightButtons, false);
            }
        }

        bool HasNext(DocumentPreview documentPreview)
        {
            var index = DocumentPreviews.FindIndex(dp => dp.Id == documentPreview.Id);
            return index >= 0 && index < DocumentPreviews.Count - 1;
        }

        bool HasPrevious(DocumentPreview documentPreview)
        {
            var index = DocumentPreviews.FindIndex(dp => dp.Id == documentPreview.Id);
            return index >= 1;
        }

        void EditDocumentButtonItem_Clicked(object sender, EventArgs e)
        {
            var vc = (DocumentViewController)ViewControllers.FirstOrDefault();
            vc.PresentEditing();
        }

        void PreviousDocumentButton_Clicked(object sender, EventArgs e)
        {
            var vc = (DocumentViewController)ViewControllers.FirstOrDefault();
            var documentPreview = vc.DocumentPreview;

            var index = DocumentPreviews.FindIndex(dp => dp.Id == documentPreview.Id);
            if (index < 1)
                return;

            var previousDocumentPreview = DocumentPreviews[index - 1];
            var previousVc = GetDocumentViewController(Folder, previousDocumentPreview);
            SetViewControllers(new[] { previousVc }, UIPageViewControllerNavigationDirection.Reverse, false, null);
            UpdateNavigationBar(previousVc, previousDocumentPreview);
        }

        void NextDocumentButton_Clicked(object sender, EventArgs e)
        {
            var vc = (DocumentViewController)ViewControllers.FirstOrDefault();
            var documentPreview = vc.DocumentPreview;

            var index = DocumentPreviews.FindIndex(dp => dp.Id == documentPreview.Id);
            if (index < 0 || index >= DocumentPreviews.Count - 1)
                return;

            var nextDocumentPreview = DocumentPreviews[index + 1];
            var nextVc = GetDocumentViewController(Folder, nextDocumentPreview);
            SetViewControllers(new[] { nextVc }, UIPageViewControllerNavigationDirection.Forward, false, null);
            UpdateNavigationBar(nextVc, nextDocumentPreview);
        }

        #endregion
    }
}