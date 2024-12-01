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
        UIBarButtonItem _previousDocumentButtonItem;
        UIBarButtonItem _nextDocumentButtonItem;
        UIBarButtonItem _editDocumentButtonItem;

        // Buttons for iPad only
        UIBarButtonItem _flagButton;
        UIBarButtonItem _fileToButton;
        UIButtonScalable _commentsButton;
        BadgeBarButtonItem _commentsBadgeButton;
        UIBarButtonItem _replyActionsButton;
        UIBarButtonItem _userActionsButton;

        const int CacheCapacity = 5; // Going below 5 might cause issues with internal caching of
                                     // UIPageViewController

        public Folder Folder { get; set; }
        public DocumentPreview InitialDocumentPreview { get; set; }
        public Guid InitialNotificationGuid { get; set; }
        public List<(Notification, DocumentPreview)> Notifications { get; set; }

        public bool Empty => !IsShowingAnyDocument();

        readonly List<DocumentViewController> _viewControllerCache = new(CacheCapacity + 1);

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

            Delegate = new NotificationPageDelegate();

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
            _nextDocumentButtonItem = new UIBarButtonItem
            {
                Image = UIImage.FromBundle("Arrow-Down"),
            };

            _previousDocumentButtonItem = new UIBarButtonItem
            {
                Image = UIImage.FromBundle("Arrow-Up"),
            };

            _editDocumentButtonItem = new UIBarButtonItem
            {
                Image = UIImage.FromBundle("Edit"),
            };
        }

        void InitializeHandlers()
        {
            if (_nextDocumentButtonItem != null)
                _nextDocumentButtonItem.Clicked += NextDocumentButton_Clicked;
            if (_previousDocumentButtonItem != null)
                _previousDocumentButtonItem.Clicked += PreviousDocumentButton_Clicked;
            if (_editDocumentButtonItem != null)
                _editDocumentButtonItem.Clicked += EditDocumentButtonItem_Clicked;
        }

        void DeinitializeHandlers()
        {
            if (_nextDocumentButtonItem != null)
                _nextDocumentButtonItem.Clicked -= NextDocumentButton_Clicked;
            if (_previousDocumentButtonItem != null)
                _previousDocumentButtonItem.Clicked -= PreviousDocumentButton_Clicked;
            if (_editDocumentButtonItem != null)
                _editDocumentButtonItem.Clicked -= EditDocumentButtonItem_Clicked;
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

            foreach (var cachedViewController in _viewControllerCache)
                cachedViewController.RecycleIfNeeded();
            _viewControllerCache.Clear();
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

            if (vc == null || _viewControllerCache == null)
                return;

            var index = _viewControllerCache?.FindIndex(v => v != null && v.DocumentPreview?.Id == vc.DocumentPreview?.Id);
            if (index >= 0)
            {
                _viewControllerCache[index.Value].RecycleIfNeeded();
                _viewControllerCache.RemoveAt(index.Value);
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
            var cachedViewController = _viewControllerCache.FirstOrDefault(dvc => dvc.DocumentPreview?.Id == documentPreview.Id);
            if (cachedViewController != null)
                return cachedViewController;

            DocumentViewController vc = new DocumentViewController
            {
                DocumentPageViewControllerDelegate = this
            };
            
            vc.SetData(Folder,documentPreview, notificationGuid);
            vc.SetRefreshDataOnAppear();
            vc.SetRefreshDataOnAppear();

            return vc;
        }

        public void AddViewControllerToCache(DocumentViewController vc)
        {
            vc.DisableRecyclingOnDisappear();
            _viewControllerCache.Add(vc);
            if (_viewControllerCache.Count > CacheCapacity)
            {
                _viewControllerCache[0].RecycleIfNeeded();
                _viewControllerCache.RemoveAt(0);
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
            _nextDocumentButtonItem.Enabled = HasNext(documentPreview);
            _previousDocumentButtonItem.Enabled = HasPrevious(documentPreview);

            if (isSearchActive && SplitViewController != null && !SplitViewController.Collapsed)
            {
                if (documentPreview.Direction == DocumentDirection.Draft)
                {
                    var rightButtons = new UIBarButtonItem[1];
                    rightButtons[0] = _editDocumentButtonItem;

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
                    rightButtons[0] = _nextDocumentButtonItem;
                    rightButtons[1] = _previousDocumentButtonItem;
                    rightButtons[2] = _editDocumentButtonItem;

                    NavigationItem.SetRightBarButtonItems(rightButtons, false);
                }
                else
                {
                    var rightButtons = new UIBarButtonItem[2];
                    rightButtons[0] = _nextDocumentButtonItem;
                    rightButtons[1] = _previousDocumentButtonItem;
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

        DocumentViewController GoToPageAndReturnVc(DocumentPreview documentPreview, Guid notificationGuid)
        {
            CommonConfig.UsageAnalytics.LogEvent(new DocumentQuickSwitchEvent());
            var vc = GetDocumentViewController(documentPreview, notificationGuid);
            return vc;
        }

        #region Delegate
        protected class NotificationPageDelegate : UIPageViewControllerDelegate
        {
            
            public override void DidFinishAnimating(UIPageViewController pageViewController, bool finished, 
                UIViewController[] previousViewControllers, bool completed)
            {
                var vc = (DocumentViewController)pageViewController.ViewControllers.FirstOrDefault();
                var documentPreview = vc.DocumentPreview;
                var pageVc = (NotificationPageViewController)pageViewController;
                var index = pageVc.Notifications.FindIndex(dp => dp.Item1.ObjectId == documentPreview.Id);
                if (index < 0 || index > pageVc.Notifications.Count - 1)
                    return;
                CommonConfig.MessengerHub.Publish(new GoToDocumentMessage(this, documentPreview.Id));

                pageVc.UpdateToolBar(vc);
                pageVc.UpdateNavigationBar(documentPreview);

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
                    var pageVc = (NotificationPageViewController)pageViewController;

                    var index = pageVc.Notifications.FindIndex(dp => dp.Item1.ObjectId== documentPreview.Id);

                    if (index < 0 || index >= pageVc.Notifications.Count - 1)
                        return null!;
                    
                    var nextDocumentNotificationGuid = pageVc.Notifications[index + 1].Item1.Guid;
                    var nextDocumentPreview = pageVc.Notifications[index + 1].Item2;
                    return pageVc.GoToPageAndReturnVc(nextDocumentPreview, nextDocumentNotificationGuid);
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error("Could not swipe to next DocumentViewController", ex);
                    return null!;
                }
            }
           
            public override UIViewController GetPreviousViewController(UIPageViewController pageViewController, UIViewController referenceViewController)
            {
                try
                {
                    var vc = (DocumentViewController)pageViewController.ViewControllers.FirstOrDefault();
                    var documentPreview = vc.DocumentPreview;
                    var pageVc = (NotificationPageViewController)pageViewController;
                    
                    var index = pageVc.Notifications.FindIndex(dp => dp.Item1.ObjectId == documentPreview.Id);

                    if (index < 1) 
                        return null!;
                    
                    var previousDocumentNotificationGuid = pageVc.Notifications[index - 1].Item1.Guid;
                    var previousDocumentPreview = pageVc.Notifications[index - 1].Item2;
                    return pageVc.GoToPageAndReturnVc(previousDocumentPreview, previousDocumentNotificationGuid);

                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error("Could not swipe to previous DocumentViewController", ex);
                    return null!;
                }
            }
        }
        #endregion

        #endregion

        #region iPad related
        public void UpdateIPadNavigationButtons(bool enabled, string commentBadgeValue)
        {
            _flagButton = new UIBarButtonItem
            {
                Image = UIImage.FromBundle("Flag"),
                Enabled = enabled
            };

            _flagButton.Clicked += FlagButton_Clicked;

            _replyActionsButton = new UIBarButtonItem
            {
                Image = UIImage.FromBundle("Reply"),
                Enabled = enabled
            };

            _replyActionsButton.Clicked += ReplyButton_Clicked;

            _fileToButton = new UIBarButtonItem
            {
                Image = UIImage.FromBundle("Worktray"),
                Enabled = enabled
            };

            _fileToButton.Clicked += FileToButton_Clicked;

            _commentsButton = new UIButtonScalable
            {
                Frame = new CoreGraphics.CGRect(0f, 0f, 25f, 25f),
                Enabled = enabled,
            };

            _commentsButton.SetImage(UIImage.FromBundle("Comments").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);

            _commentsButton.TouchUpInside += CommentsButton_TouchUpInside;

            _commentsBadgeButton = new BadgeBarButtonItem(_commentsButton)
            {
                Enabled = enabled,
            };

            _commentsBadgeButton.SetBadgeValue(commentBadgeValue);

            _userActionsButton = new UIBarButtonItem
            {
                Image = UIImage.FromBundle("Actions"),
                Enabled = enabled
            };

            _userActionsButton.Clicked += UserActionsButton_Clicked;

            var leftButtons = new[]
            {
                _flagButton,
                _replyActionsButton,
                _fileToButton,
                _commentsBadgeButton,
                _userActionsButton
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
