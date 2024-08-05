using reMark.Mobile.Common;
using reMark.Mobile.Common.Manager;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Model.HubMessages;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.IOS.Ui.Common;
using reMark.Mobile.IOS.Ui.ViewControllers.DocumentView;
using reMark.Mobile.IOS.Utilities;
using UIKit;

namespace reMark.Mobile.IOS.Ui.ViewControllers
{
    public class NotificationPageViewController : AbstractPageViewController, IDocumentPageViewControllerDelegate, ISecondaryViewController
    {
        UIBarButtonItem previousDocumentButtonItem;
        UIBarButtonItem nextDocumentButtonItem;
        UIBarButtonItem editDocumentButtonItem;

        // Buttons for iPad only
        UIBarButtonItem flagButton;
        UIBarButtonItem fileToButton;
        UIButtonScalable commentsButton;
        BadgeBarButtonItem commentsBadgeButton;
        UIBarButtonItem replyActionsButton;
        UIBarButtonItem userActionsButton;

        const int CacheCapacity = 5; // Going below 5 might cause issues with internal caching of
                                     // UIPageViewController

        public Folder Folder { get; set; }
        public DocumentPreview InitialDocumentPreview { get; set; }
        public Guid InitialNotificationGuid { get; set; }
        public List<(Notification, DocumentPreview)> Notifications { get; set; }

        public bool Empty => !IsShowingAnyDocument();

        readonly List<DocumentViewController> viewControllerCache = new(CacheCapacity + 1);

        public NotificationPageViewController()
        {
            HidesBottomBarWhenPushed = true;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            if (Integration.IsRunningAtLeast(11))
                NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Never;

            InitNavigationBar();

            if (InitialDocumentPreview != null && InitialNotificationGuid != null)
                SetPage(InitialDocumentPreview, InitialNotificationGuid, false);

            Delegate = new DocumentPageDelegate();

            DataSource = new NotificationPageDataSource();
            ((NotificationPageDataSource)DataSource).OnNextViewControllerLoaded += PageDelegate_OnNextViewControllerLoaded;
        }
        
        private void PageDelegate_OnNextViewControllerLoaded(DocumentViewController viewController, UIPageViewControllerNavigationDirection direction)
        {
            if (viewController != null)
            {
                // This method should run on the UI thread.
                InvokeOnMainThread(() =>
                {
                    SetViewControllers(new[] { viewController }, direction, false, 
                        (finished) => UpdateToolBar(viewController));
                });
            }
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (NavigationController != null)
            {
                NavigationController.ToolbarHidden = Integration.IsIPad();
            }
            InitializeHandlers();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            if (NavigationController != null)
            {
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

        async void PreviousDocumentButton_Clicked(object sender, EventArgs e)
        {
            var vc = (DocumentViewController)ViewControllers.FirstOrDefault();
            var documentPreview = vc.DocumentPreview;
            

            var index = Notifications.FindIndex(dp => dp.Item1.ObjectId == documentPreview.Id);
            if (index < 1)
                return;

            var previousDocumentPreviewId = Notifications[index - 1].Item1.ObjectId;
            var previousDocumentPreviewFolderId = Notifications[index - 1].Item1.FolderId;
            var previousDocumentNotificationGuid = Notifications[index - 1].Item1.Guid;

            var documentContainer = await Managers.DocumentsManager.GetDocumentWithPreviewAsync(previousDocumentPreviewFolderId, previousDocumentPreviewId);
            GoToPage(documentContainer.DocumentPreview, previousDocumentNotificationGuid, UIPageViewControllerNavigationDirection.Reverse);
        }

        async void NextDocumentButton_Clicked(object sender, EventArgs e)
        {
            var vc = (DocumentViewController)ViewControllers.FirstOrDefault();
            var documentPreview = vc.DocumentPreview;

            var index = Notifications.FindIndex(dp => dp.Item1.ObjectId == documentPreview.Id);
            if (index < 0 || index >= Notifications.Count - 1)
                return;

            var nextDocumentPreviewId = Notifications[index + 1].Item1.ObjectId;
            var nextDocumentPreviewFolderId = Notifications[index + 1].Item1.FolderId;
            var nextDocumentNotificationGuid = Notifications[index + 1].Item1.Guid;

            var documentContainer = await Managers.DocumentsManager.GetDocumentWithPreviewAsync( nextDocumentPreviewFolderId, nextDocumentPreviewId);
            GoToPage(documentContainer.DocumentPreview, nextDocumentNotificationGuid, UIPageViewControllerNavigationDirection.Forward);
        }

        #endregion

        #region Public methods

        public void SetPage(DocumentPreview documentPreview, Guid notificationGuid, bool isSearchActive)
        {
            ChangePage(documentPreview, notificationGuid, UIPageViewControllerNavigationDirection.Forward, isSearchActive);
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
            NavigationItem.SetLeftBarButtonItems(new UIBarButtonItem[0], false);
        }

        public bool IsShowingDocumentWithId(int id)
        {
            var vc = (DocumentViewController)ViewControllers.FirstOrDefault();
            return vc?.IsRecycled() == false && vc.DocumentPreview?.Id == id;
        }

        #endregion

        #region Utilities

        bool IsShowingAnyDocument()
        {
            var vc = (DocumentViewController)ViewControllers.FirstOrDefault();
            return vc?.IsRecycled() == false && vc.DocumentPreview?.Id != null;
        }

        bool HasNext(DocumentPreview documentPreview)
        {
            var index = Notifications.FindIndex(dp => dp.Item2.Id == documentPreview.Id);
            return index >= 0 && index < Notifications.Count - 1;
        }

        bool HasPrevious(DocumentPreview documentPreview)
        {
            var index = Notifications.FindIndex(dp => dp.Item2.Id == documentPreview.Id);
            return index >= 1;
        }

        void GoToPage(DocumentPreview documentPreview, Guid notificationGuid, UIPageViewControllerNavigationDirection direction)
        {
            CommonConfig.UsageAnalytics.LogEvent(new DocumentQuickSwitchEvent());
            ChangePage(documentPreview, notificationGuid, direction);
        }

        void ChangePage(DocumentPreview documentPreview, Guid notificationGuid, UIPageViewControllerNavigationDirection direction, bool isSearchActive = false)
        {
            SetToolbarItems(null, true);

            var vc = GetDocumentViewController(documentPreview, notificationGuid);
            SetViewControllers(new[] { vc }, direction, false, (finished) => UpdateToolBar(vc));
            UpdateNavigationBar(documentPreview, isSearchActive);
            CommonConfig.MessengerHub.Publish(new GoToDocumentMessage(this, documentPreview.Id));
        }

        DocumentViewController GetDocumentViewController(DocumentPreview documentPreview, Guid notificationGuid)
        {
            var cachedViewController = viewControllerCache.FirstOrDefault(dvc => dvc.DocumentPreview?.Id == documentPreview.Id);
            if (cachedViewController != null)
                return cachedViewController;

            DocumentViewController vc = new DocumentViewController
            {
                DocumentPageViewControllerDelegate = this
            };
            
            vc.SetData(documentPreview.Id, notificationGuid);
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
            if (Integration.IsIPad())
                vc?.RefreshToolbar();
            else
            {
                var ti = vc?.ToolbarItems;
                SetToolbarItems(ti, true);
            }
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
        //Both the delegate and the datasource are used only when swiping

        DocumentViewController GoToPageAndReturnVC(DocumentPreview documentPreview, Guid notificationGuid)
        {
            CommonConfig.UsageAnalytics.LogEvent(new DocumentQuickSwitchEvent());
            var vc = GetDocumentViewController(documentPreview, notificationGuid);
            return vc;
        }

        #region Delegate
        protected class DocumentPageDelegate : UIPageViewControllerDelegate
        {
            
            public override void DidFinishAnimating(UIPageViewController pageViewController, bool finished, 
                UIViewController[] previousViewControllers, bool completed)
            {
                if (pageViewController == null) 
                    return;
                var vc = (DocumentViewController)pageViewController.ViewControllers.FirstOrDefault();
                if (vc == null) return;
                var documentPreview = vc.DocumentPreview;
                if (documentPreview == null)
                    return;
                var pageVC = (NotificationPageViewController)pageViewController;
                var index = pageVC.Notifications.FindIndex(dp => dp.Item2.Id == documentPreview.Id);
                if (index < 0 || index > pageVC.Notifications.Count - 1)
                    return;
                CommonConfig.MessengerHub.Publish(new GoToDocumentMessage(this, documentPreview.Id));

                pageVC.UpdateToolBar(vc);
                pageVC.UpdateNavigationBar(documentPreview);

                var vcPrevious = (DocumentViewController)previousViewControllers?.FirstOrDefault();
                vcPrevious?.ResetOffset();
            }
        }
        #endregion

        #region DataSource
        public delegate void DocumentViewControllerLoadedHandler(DocumentViewController viewController, 
            UIPageViewControllerNavigationDirection direction);
        
        protected class NotificationPageDataSource : UIPageViewControllerDataSource
        {
            public event DocumentViewControllerLoadedHandler OnNextViewControllerLoaded;
    
            public override UIViewController GetNextViewController(UIPageViewController pageViewController, UIViewController referenceViewController)
            {
                try
                {
                    var vc = (DocumentViewController)pageViewController.ViewControllers.FirstOrDefault();
                    var documentPreview = vc.DocumentPreview;
                    var pageVC = (NotificationPageViewController)pageViewController;
                    

                  
                    var index = pageVC.Notifications.FindIndex(dp => dp.Item1.ObjectId== documentPreview.Id);

                    if (index < 0 || index >= pageVC.Notifications.Count - 1)
                        return null;

                    var nextDocumentPreviewId = pageVC.Notifications[index + 1].Item1.ObjectId;
                    var nextDocumentPreviewFolderId = pageVC.Notifications[index + 1].Item1.FolderId;
                    var nextDocumentNotificationGuid = pageVC.Notifications[index + 1].Item1.Guid;
                    var nextDocumentPreview = pageVC.Notifications[index + 1].Item2;
                    return pageVC.GoToPageAndReturnVC(nextDocumentPreview, nextDocumentNotificationGuid);
         
                    // Asynchronously fetch the next view controller
                  /*  FetchNextViewController(pageVC, nextDocumentPreviewFolderId, nextDocumentPreviewId, nextDocumentNotificationGuid,
                        UIPageViewControllerNavigationDirection.Forward,
                        OnNextViewControllerLoaded);
*/
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error("Could not swipe to next DocumentViewController", ex);
                    return null;
                }
            }
            
            private async void FetchNextViewController(NotificationPageViewController pageViewController, 
                int nextDocumentPreviewFolderId, int nextDocumentPreviewId, 
                Guid nextDocumentNotificationGuid,
                UIPageViewControllerNavigationDirection direction,
                DocumentViewControllerLoadedHandler callback)
            {
                var nextDocument = await Managers.DocumentsManager.GetDocumentWithPreviewAsync(nextDocumentPreviewFolderId, nextDocumentPreviewId);
                var nextViewController = pageViewController.GetDocumentViewController(nextDocument.DocumentPreview, nextDocumentNotificationGuid);

                // Invoke the callback when the task is complete.
                callback?.Invoke(nextViewController, direction);
            }
           
            public override UIViewController GetPreviousViewController(UIPageViewController pageViewController, UIViewController referenceViewController)
            {
                try
                {
                    var vc = (DocumentViewController)pageViewController.ViewControllers.FirstOrDefault();
                    var documentPreview = vc.DocumentPreview;
                    var pageVC = (NotificationPageViewController)pageViewController;
                    pageVC.PreviousDocumentButton_Clicked(null, new EventArgs());
      

                    var index = pageVC.Notifications.FindIndex(dp => dp.Item1.ObjectId == documentPreview.Id);

                    if (index < 1) 
                        return null;
                    
                    var previousDocumentPreviewId = pageVC.Notifications[index - 1].Item1.ObjectId;
                    var previousDocumentPreviewFolderId = pageVC.Notifications[index - 1].Item1.FolderId;
                    var previousDocumentNotificationGuid = pageVC.Notifications[index - 1].Item1.Guid;
                    var previousDocumentPreview = pageVC.Notifications[index - 1].Item2;
                    return pageVC.GoToPageAndReturnVC(previousDocumentPreview, previousDocumentNotificationGuid);
/*
                    // Asynchronously fetch the next view controller
                    FetchNextViewController(pageVC, previousDocumentPreviewFolderId, previousDocumentPreviewId, 
                        previousDocumentNotificationGuid, UIPageViewControllerNavigationDirection.Reverse, OnNextViewControllerLoaded);
*/
                    return null;  // Return null initially, as the async operation is in progress.
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

        #region iPad related
        public void UpdateIPadNavigationButtons(bool enabled, string commentBadgeValue)
        {
            flagButton = new UIBarButtonItem
            {
                Image = UIImage.FromBundle("Flag"),
                Enabled = enabled
            };

            flagButton.Clicked += FlagButton_Clicked;

            replyActionsButton = new UIBarButtonItem
            {
                Image = UIImage.FromBundle("Reply"),
                Enabled = enabled
            };

            replyActionsButton.Clicked += ReplyButton_Clicked;

            fileToButton = new UIBarButtonItem
            {
                Image = UIImage.FromBundle("Worktray"),
                Enabled = enabled
            };

            fileToButton.Clicked += FileToButton_Clicked;

            commentsButton = new UIButtonScalable
            {
                Frame = new CoreGraphics.CGRect(0f, 0f, 25f, 25f),
                Enabled = enabled,
            };

            commentsButton.SetImage(UIImage.FromBundle("Comments").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);

            commentsButton.TouchUpInside += CommentsButton_TouchUpInside;

            commentsBadgeButton = new BadgeBarButtonItem(commentsButton)
            {
                Enabled = enabled,
            };

            commentsBadgeButton.SetBadgeValue(commentBadgeValue);

            userActionsButton = new UIBarButtonItem
            {
                Image = UIImage.FromBundle("Actions"),
                Enabled = enabled
            };

            userActionsButton.Clicked += UserActionsButton_Clicked;

            var leftButtons = new[]
            {
                flagButton,
                replyActionsButton,
                fileToButton,
                commentsBadgeButton,
                userActionsButton
            };

            NavigationItem.SetLeftBarButtonItems(leftButtons, false);
        }

        void UserActionsButton_Clicked(object sender, EventArgs e)
        {
            var vc = (DocumentViewController)ViewControllers.FirstOrDefault();
            vc.UserActionsClicked(sender, e);
        }

        void CommentsButton_TouchUpInside(object sender, EventArgs e)
        {
            var vc = (DocumentViewController)ViewControllers.FirstOrDefault();
            vc.CommentsClicked(sender, e);
        }

        void FlagButton_Clicked(object sender, EventArgs e)
        {
            var vc = (DocumentViewController)ViewControllers.FirstOrDefault();
            vc.FlagClicked(sender, e);
        }

        void ReplyButton_Clicked(object sender, EventArgs e)
        {
            var vc = (DocumentViewController)ViewControllers.FirstOrDefault();
            vc.ReplyClicked(sender, e);
        }

        void FileToButton_Clicked(object sender, EventArgs e)
        {
            var vc = (DocumentViewController)ViewControllers.FirstOrDefault();
            vc.FileToClicked(sender, e);
        }
        #endregion
    }


}
