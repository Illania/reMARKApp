using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.HubMessages;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView
{
    public class DocumentPageViewController : AbstractPageViewController, IDocumentPageViewControllerDelegate
    {
        UIBarButtonItem previousDocumentButtonItem;
        UIBarButtonItem nextDocumentButtonItem;
        UIBarButtonItem editDocumentButtonItem;

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

            InitNavigationBar();

            if (InitialDocumentPreview != null && Folder != null)
                SetPage(Folder, InitialDocumentPreview, false);

            Delegate = new DocumentPageDelegate();

            DataSource = new DocumentPageDataSource();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (NavigationController != null)
            {
                if (NavigationController.NavigationBar != null)
                {
                    NavigationController.NavigationBar.Translucent = false;
                }
                NavigationController.ToolbarHidden = false;
            }
            InitializeHandlers();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            if (NavigationController != null)
            {
                if (NavigationController.NavigationBar != null)
                {
                    NavigationController.NavigationBar.Translucent = true;
                }
                NavigationController.ToolbarHidden = true;
            }
            DeinitializeHandlers();
        }

        void InitNavigationBar()
        {
            nextDocumentButtonItem = new UIBarButtonItem
            {
                Image = UIImage.FromBundle("Arrow-Down"),
            };

            previousDocumentButtonItem = new UIBarButtonItem
            {
                Image = UIImage.FromBundle("Arrow-Up"),
            };

            editDocumentButtonItem = new UIBarButtonItem
            {
                Image = UIImage.FromBundle("Edit"),
            };
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

        protected override void Recycle()
        {
            base.Recycle();

            foreach (var cachedViewController in viewControllerCache)
                cachedViewController.RecycleIfNeeded();
            viewControllerCache.Clear();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (CommonConfig.Logger.IsDebugEnabled())
                CommonConfig.Logger.Debug("Disposed");
        }

        #region Navigation Bar handlers

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
            GoToPage(previousDocumentPreview, UIPageViewControllerNavigationDirection.Reverse);
        }

        void NextDocumentButton_Clicked(object sender, EventArgs e)
        {
            var vc = (DocumentViewController)ViewControllers.FirstOrDefault();
            var documentPreview = vc.DocumentPreview;

            var index = DocumentPreviews.FindIndex(dp => dp.Id == documentPreview.Id);
            if (index < 0 || index >= DocumentPreviews.Count - 1)
                return;

            var nextDocumentPreview = DocumentPreviews[index + 1];
            GoToPage(nextDocumentPreview, UIPageViewControllerNavigationDirection.Forward);
        }

        #endregion

        #region Public methods

        public void SetPage(Folder folder, DocumentPreview documentPreview, bool isSearchActive)
        {
            Folder = folder;
            ChangePage(folder, documentPreview, UIPageViewControllerNavigationDirection.Forward, isSearchActive);
        }

        public void ClearPage()
        {
            var vc = (DocumentViewController)ViewControllers?.FirstOrDefault();

            if (vc == null || viewControllerCache == null)
                return;

            var index = viewControllerCache?.FindIndex(v => v != null && v.DocumentPreview?.Id == vc.DocumentPreview?.Id);
            if (index >= 0)
            {
                viewControllerCache[index.Value].RecycleIfNeeded();
                viewControllerCache.RemoveAt(index.Value);
            }

            SetToolbarItems(null, true);
            NavigationItem.SetRightBarButtonItems(new UIBarButtonItem[0], false);
        }

        public bool IsShowingDocumentWithId(int id)
        {
            var vc = (DocumentViewController)ViewControllers.FirstOrDefault();
            return vc?.IsRecycled() == false && vc.DocumentPreview?.Id == id;
        }

        #endregion

        #region Utilities

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

        void GoToPage(DocumentPreview documentPreview, UIPageViewControllerNavigationDirection direction)
        {
            CommonConfig.UsageAnalytics.LogEvent(new DocumentQuickSwitchEvent());
            ChangePage(Folder, documentPreview, direction);
        }

        void ChangePage(Folder folder, DocumentPreview documentPreview, UIPageViewControllerNavigationDirection direction, bool isSearchActive = false)
        {
            SetToolbarItems(null, true);

            var vc = GetDocumentViewController(folder, documentPreview);
            SetViewControllers(new[] { vc }, direction, false, (finished) => UpdateToolBar(vc));
            UpdateNavigationBar(documentPreview, isSearchActive);
            CommonConfig.MessengerHub.Publish(new GoToDocumentMessage(this, documentPreview.Id));
        }

        DocumentViewController GetDocumentViewController(Folder folder, DocumentPreview documentPreview)
        {
            var cachedViewController = viewControllerCache.FirstOrDefault(dvc => dvc.DocumentPreview?.Id == documentPreview.Id);
            if (cachedViewController != null)
                return cachedViewController;

            DocumentViewController vc = new DocumentViewController
            {
                DocumentPageViewControllerDelegate = this
            };

            vc.SetData(folder, documentPreview);
            vc.SetRefreshDataOnAppear();

            return vc;
        }

        public void AddViewControllerToCache(DocumentViewController vc)
        {
            vc.DisableRecyclingOnDisappear();
            viewControllerCache.Add(vc);
            if (viewControllerCache.Count > CacheCapacity)
            {
                viewControllerCache[0].RecycleIfNeeded();
                viewControllerCache.RemoveAt(0);
            }
        }

        void UpdateToolBar(DocumentViewController vc)
        {
            var ti = vc?.ToolbarItems;
            SetToolbarItems(ti, true);
        }

        void UpdateNavigationBar(DocumentPreview documentPreview, bool isSearchActive = false)
        {
            nextDocumentButtonItem.Enabled = HasNext(documentPreview);
            previousDocumentButtonItem.Enabled = HasPrevious(documentPreview);

            if (isSearchActive && SplitViewController != null && !SplitViewController.Collapsed)
            {
                if (documentPreview.Direction == DocumentDirection.Draft)
                {
                    var rightButtons = new UIBarButtonItem[1];
                    rightButtons[0] = editDocumentButtonItem;

                    NavigationItem.SetRightBarButtonItems(rightButtons, false);
                }
                else
                {
                    var rightButtons = new UIBarButtonItem[0];
                    NavigationItem.SetRightBarButtonItems(rightButtons, false);
                }
            }
            else
            {
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
        }

        public void UpdatePriority()
        {
            var vc = (DocumentViewController)ViewControllers.FirstOrDefault();
            vc?.UpdatePriority();
        }

        #endregion

        #region Swipe Related
        //Both the delegate and the datasource are used only when swiping

        DocumentViewController GoToPageAndReturnVC(DocumentPreview documentPreview, UIPageViewControllerNavigationDirection direction)
        {
            CommonConfig.UsageAnalytics.LogEvent(new DocumentQuickSwitchEvent());
            var vc = GetDocumentViewController(Folder, documentPreview);
            return vc;
        }

        #region Delegate
        protected class DocumentPageDelegate : UIPageViewControllerDelegate
        {
            public override void DidFinishAnimating(UIPageViewController pageViewController, bool finished, UIViewController[] previousViewControllers, bool completed)
            {
                var vc = (DocumentViewController)pageViewController.ViewControllers.FirstOrDefault();
                var documentPreview = vc.DocumentPreview;
                var pageVC = (DocumentPageViewController)pageViewController;
                var index = pageVC.DocumentPreviews.FindIndex(dp => dp.Id == documentPreview.Id);
                if (index < 0 || index > pageVC.DocumentPreviews.Count - 1)
                    return;
                CommonConfig.MessengerHub.Publish(new GoToDocumentMessage(this, documentPreview.Id));

                ((DocumentPageViewController)pageViewController).UpdateNavigationBar(documentPreview);

                var vcPrevious = (DocumentViewController)previousViewControllers?.FirstOrDefault();
                vcPrevious?.ResetOffset();
            }
        }
        #endregion

        #region DataSource
        protected class DocumentPageDataSource : UIPageViewControllerDataSource
        {
            public override UIViewController GetNextViewController(UIPageViewController pageViewController, UIViewController referenceViewController)
            {
                try
                {
                    var vc = (DocumentViewController)pageViewController.ViewControllers.FirstOrDefault();
                    var documentPreview = vc.DocumentPreview;
                    var pageVC = (DocumentPageViewController)pageViewController;
                    var index = pageVC.DocumentPreviews.FindIndex(dp => dp.Id == documentPreview.Id);
                    if (index < 0 || index >= pageVC.DocumentPreviews.Count - 1)
                        return null;
                    var nextDocumentPreview = pageVC.DocumentPreviews[index + 1];
                    return pageVC.GoToPageAndReturnVC(nextDocumentPreview, UIPageViewControllerNavigationDirection.Forward);
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error("Could not swipe to next DocumentViewController", ex);
                    return null;
                }
            }

            public override UIViewController GetPreviousViewController(UIPageViewController pageViewController, UIViewController referenceViewController)
            {
                try
                {
                    var vc = (DocumentViewController)pageViewController.ViewControllers.FirstOrDefault();
                    var documentPreview = vc.DocumentPreview;
                    var pageVC = (DocumentPageViewController)pageViewController;
                    var index = pageVC.DocumentPreviews.FindIndex(dp => dp.Id == documentPreview.Id);
                    if (index < 1)
                        return null;
                    var previousDocumentPreview = pageVC.DocumentPreviews[index - 1];
                    return pageVC.GoToPageAndReturnVC(previousDocumentPreview, UIPageViewControllerNavigationDirection.Reverse);
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error("Could not swipe to previous DocumentViewController", ex);
                    return null;
                }
            }
        }
        #endregion

        #endregion

    }

    public interface IDocumentPageViewControllerDelegate
    {
        void AddViewControllerToCache(DocumentViewController documentViewController);
    }
}