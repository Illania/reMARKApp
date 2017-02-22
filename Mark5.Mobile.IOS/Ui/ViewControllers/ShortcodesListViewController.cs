//
// Project: Mark5.Mobile.IOS
// File: ShortcodesListViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;
using ObjCRuntime;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{

    public class ShortcodesListViewController : AbstractViewController, IPrimaryViewController, IUISearchResultsUpdating, IUIGestureRecognizerDelegate
    {

        public Folder Folder { get; set; }

        UIBarButtonItem exitEditItem;
        UIBarButtonItem editItem;

        UIRefreshControl refreshControl;
        UITableView shortcodesTableView;
        UISearchController searchController;
        UITableViewController searchResultsController;
        DataSource searchResultsDataSource;

        CancellationTokenSource searchCancellationTokenSource;
        readonly List<CancellationTokenSource> searchCancellationTokenSourceList = new List<CancellationTokenSource>();

        bool refreshing;

        CancellationTokenSource cts;

        #region UIViewController overrides

        public override void LoadView()
        {
            base.LoadView();

            InitializeNavigationBar();
            InitializeView();
            InitializeSearchBar();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            InitializeNavigationBarTitle();
            InitializeHandlers();
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info($"{nameof(ShortcodesListViewController)} appeared");

            var ds = (DataSource)shortcodesTableView.Source;
            if (ds.Empty)
                RefreshData();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeinitializeHandlers();

            cts?.Cancel();
        }

        public override void WillMoveToParentViewController(UIViewController parent)
        {
            base.WillMoveToParentViewController(parent);

            if (parent == null && SplitViewController != null && !SplitViewController.Collapsed)
            {
                var nc = (UINavigationController)SplitViewController.ViewControllers[1];
                nc.PopToRootViewController(false);

                var vc = (ShortcodeViewController)nc.ViewControllers[0];
                vc.ClearData();
            }
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning($"{nameof(ShortcodesListViewController)} received memory warning!");

            var ds = shortcodesTableView?.Source as DataSource;
            ds?.Reset();

            base.DidReceiveMemoryWarning();
        }

        #endregion

        #region Initialization

        void InitializeNavigationBar()
        {
            exitEditItem = new UIBarButtonItem(UIBarButtonSystemItem.Done);
            editItem = new UIBarButtonItem(UIBarButtonSystemItem.Edit);
        }

        void InitializeView()
        {
            AutomaticallyAdjustsScrollViewInsets = true;

            shortcodesTableView = new UITableView();
            shortcodesTableView.ClipsToBounds = false;
            shortcodesTableView.Source = new DataSource(this, shortcodesTableView, Localization.GetString("folder_empty"));
            shortcodesTableView.AllowsSelectionDuringEditing = false;
            shortcodesTableView.AllowsMultipleSelectionDuringEditing = true;
            shortcodesTableView.TranslatesAutoresizingMaskIntoConstraints = false;
            View.AddSubview(shortcodesTableView);
            View.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(shortcodesTableView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(shortcodesTableView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(shortcodesTableView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(shortcodesTableView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1.0f, 0.0f)
                });

            var longPressRecognizer = new UILongPressGestureRecognizer(this, new Selector("longPressed:"))
            {
                MinimumPressDuration = 1f,
                Delegate = this
            };
            shortcodesTableView.AddGestureRecognizer(longPressRecognizer);

            refreshControl = new UIRefreshControl();
            refreshControl.BackgroundColor = UIColor.White;
            refreshControl.AttributedTitle = Localization.GetNSAttributedString("pull_to_refresh");
            shortcodesTableView.AddSubview(refreshControl);
        }

        void InitializeSearchBar()
        {
            DefinesPresentationContext = true;

            searchResultsController = new UITableViewController();
            searchResultsDataSource = new DataSource(this, searchResultsController.TableView, Localization.GetString("no_matching_shortcodes"));
            searchResultsController.TableView.Source = searchResultsDataSource;

            searchController = new UISearchController(searchResultsController)
            {
                HidesNavigationBarDuringPresentation = true,
                DimsBackgroundDuringPresentation = true,
                ObscuresBackgroundDuringPresentation = true,
                SearchResultsUpdater = this
            };
            searchController.SearchBar.Placeholder = Localization.GetString("filter");

            shortcodesTableView.TableHeaderView = searchController.SearchBar;
        }

        void InitializeNavigationBarTitle()
        {
            NavigationItem.Title = Folder.Name;
        }

        void InitializeHandlers()
        {
            if (exitEditItem != null)
                exitEditItem.Clicked += ExitEditItem_Clicked;

            if (editItem != null)
                editItem.Clicked += EditItem_Clicked;

            if (refreshControl != null)
                refreshControl.ValueChanged += RefreshControl_ValueChanged;
        }

        void DeinitializeHandlers()
        {
            if (exitEditItem != null)
                exitEditItem.Clicked -= ExitEditItem_Clicked;

            if (editItem != null)
                editItem.Clicked -= EditItem_Clicked;

            if (refreshControl != null)
                refreshControl.ValueChanged -= RefreshControl_ValueChanged;
        }

        #endregion

        #region Actions

        public void ShortcodeSelected(UITableView tableView, ShortcodePreview shortcodePreview)
        {
            if (tableView == searchResultsController.TableView)
            {
                var ds = (DataSource)shortcodesTableView.Source;
                var index = ds.Items.FindIndex(sp => sp.Id == shortcodePreview.Id);
                if (index >= 0) shortcodesTableView.SelectRow(NSIndexPath.FromRowSection(index, 0), false, UITableViewScrollPosition.Middle);
            }

            if (SplitViewController != null && !SplitViewController.Collapsed)
            {
                var nc = (UINavigationController)SplitViewController.ViewControllers[1];
                nc.PopToRootViewController(false);

                var vc = (ShortcodeViewController)nc.ViewControllers[0];
                vc.ClearData();
                vc.SetData(Folder, shortcodePreview);
                vc.RefreshData();
            }
            else
            {
                var vc = new ShortcodeViewController();
                vc.SetData(Folder, shortcodePreview);
                vc.SetRefreshDataOnAppear();
                NavigationController.PushViewController(vc, true);
            }
        }

        [Export("longPressed:")]
        public void LongPressed(UILongPressGestureRecognizer recognizer)
        {
            if (shortcodesTableView.Editing) return;

            StartEditing();

            var point = recognizer.LocationInView(shortcodesTableView);
            var indexPath = shortcodesTableView.IndexPathForRowAtPoint(point);

            shortcodesTableView.SelectRow(indexPath, true, UITableViewScrollPosition.None);
        }

        void StartEditing()
        {
            shortcodesTableView.SetEditing(true, true);
            NavigationItem.SetRightBarButtonItem(exitEditItem, true);
            NavigationItem.SetLeftBarButtonItem(editItem, true);

            searchController.SearchBar.UserInteractionEnabled = false;
            searchController.SearchBar.Alpha = .5f;

            if (SplitViewController != null && !SplitViewController.Collapsed)
            {
                var nc = (UINavigationController)SplitViewController.ViewControllers[1];
                nc.PopToRootViewController(false);

                var vc = (ShortcodeViewController)nc.ViewControllers[0];
                vc.ClearData();
            }
        }

        void ExitEditItem_Clicked(object sender, EventArgs e) => EndEditing();

        void EndEditing()
        {
            shortcodesTableView.SetEditing(false, true);
            NavigationItem.SetRightBarButtonItem(null, true);
            NavigationItem.SetLeftBarButtonItem(NavigationItem.BackBarButtonItem, true);

            searchController.SearchBar.UserInteractionEnabled = true;
            searchController.SearchBar.Alpha = 1f;
        }

        void EditItem_Clicked(object sender, EventArgs e)
        {
            if (shortcodesTableView.IndexPathsForSelectedRows == null || shortcodesTableView.IndexPathsForSelectedRows.Length < 1) return;

            var eas = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);

            var rows = shortcodesTableView.IndexPathsForSelectedRows.ToArray();
            var selectedShortcodes = rows.Select(ip => ((DataSource)shortcodesTableView.Source).Items[ip.Row]).ToList();

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_worktray"), UIAlertActionStyle.Default, null)); // TODO
            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_folder"), UIAlertActionStyle.Default, a =>
            {
                var vc = new CopyMoveToFolderListViewController(selectedShortcodes.Cast<IBusinessEntity>().ToList());
                NavigationController.PresentViewController(new NavigationController(vc), true, null);
            }));

            if (Folder.InternalType == FolderInternalType.FilterView
                || Folder.InternalType == FolderInternalType.Static
                || Folder.InternalType == FolderInternalType.Worktray)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("move_to_folder"), UIAlertActionStyle.Default, a =>
            {
                var vc = new CopyMoveToFolderListViewController(selectedShortcodes.Cast<IBusinessEntity>().ToList(), Folder);
                NavigationController.PresentViewController(new NavigationController(vc), true, null);
            }));

            if (Folder.InternalType == FolderInternalType.FilterView
                || Folder.InternalType == FolderInternalType.Static
                || Folder.InternalType == FolderInternalType.Worktray)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete_from_folder"), UIAlertActionStyle.Default, null)); // TODO

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator
                || ServerConfig.SystemSettings.ShortcodesModuleInfo.Permissions.DeleteAllowed)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete"), UIAlertActionStyle.Destructive, null)); // TODO

            eas.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, a => exitEditItem.Enabled = true));

            if (eas.PopoverPresentationController != null)
                eas.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate((UIBarButtonItem)sender);

            exitEditItem.Enabled = false;
            PresentViewController(eas, true, null);
        }

        #endregion

        #region Refreshing

        void RefreshControl_ValueChanged(object sender, EventArgs e) => RefreshData(forceClear: true);

        void RefreshData(int startRowId = -1, bool forceClear = false)
        {
            if (refreshing) return;

            refreshing = true;
            refreshControl.ValueChanged -= RefreshControl_ValueChanged;

            CommonConfig.Logger.Info($"Refreshing shortcodes list [folder={Folder?.Name}, startRowId={startRowId}, forceClear={forceClear}]");

            cts?.Cancel();
            cts = new CancellationTokenSource();

            if (forceClear)
            {
                var ds = (DataSource)shortcodesTableView.Source;
                ds.Reset();
            }

            Managers.ShortcodesManager.GetAllShortcodePreviews(Folder, sps =>
            {
                Managers.DownloadManager.Notify(ObjectType.Shortcode, Folder.Id);
                InvokeOnMainThread(() =>
                {
                    var ds = (DataSource)shortcodesTableView.Source;
                    ds.AppendItems(sps);
                });
            }, () =>
            {
                refreshControl.EndRefreshing();
                refreshControl.ValueChanged += RefreshControl_ValueChanged;

                refreshing = false;

                CommonConfig.Logger.Info($"Refresh finished");
#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
            }, async ex =>
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
            {
                refreshControl.EndRefreshing();
                refreshControl.ValueChanged += RefreshControl_ValueChanged;

                refreshing = false;

                CommonConfig.Logger.Error($"Could not refresh folders [folder={Folder?.Name}, startRowId={startRowId}, forceClear={forceClear}]", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);

                NavigationController?.PopViewController(true);
            }, startRowId, cts.Token);
        }

        #endregion

        #region Searching

        void IUISearchResultsUpdating.UpdateSearchResultsForSearchController(UISearchController searchController)
        {
            var searchText = searchController.SearchBar.Text;

            if (!searchController.Active || string.IsNullOrWhiteSpace(searchText))
            {
                searchCancellationTokenSourceList.ForEach(cts => cts?.Cancel());
                searchCancellationTokenSourceList.Clear();
                searchResultsDataSource.Reset();
            }
            else
            {
                if (searchCancellationTokenSource != null)
                {
                    searchCancellationTokenSource.Cancel();
                    searchCancellationTokenSourceList.Remove(searchCancellationTokenSource);
                    searchCancellationTokenSource = null;
                }

                searchCancellationTokenSource = new CancellationTokenSource();
                searchCancellationTokenSourceList.Add(searchCancellationTokenSource);

                DoSearchShortcodes(searchText, searchCancellationTokenSource.Token);
            }
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void DoSearchShortcodes(string searchText, CancellationToken ct)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            searchResultsDataSource.Reset();

            await Task.Delay(500);

            if (ct.IsCancellationRequested) return;

            var ds = (DataSource)shortcodesTableView.Source;
            var filteredShortcodes = ds.Items.Where(sp => MatchesQuery(sp, searchText)).ToList();

            if (ct.IsCancellationRequested) return;

            searchResultsDataSource.AppendItems(filteredShortcodes);
        }

        static bool MatchesQuery(ShortcodePreview sp, string query)
        {
            if (sp.Name.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0)
                return true;

            if (sp.Description.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0)
                return true;

            return false;
        }

        #endregion

        class DataSource : UITableViewSource, IDisposable
        {

            static readonly nfloat Height = 65f;

            public bool Empty
            {
                get
                {
                    return shortcodePreviewsInView.Count < 1;
                }
            }

            public List<ShortcodePreview> Items
            {
                get
                {
                    return shortcodePreviewsInView;
                }
            }

            ShortcodesListViewController viewController;
            UITableView shortcodesTableView;
            readonly string emptyText;

            bool loading = true;
            List<ShortcodePreview> shortcodePreviewsInView = new List<ShortcodePreview>(1000);

            public DataSource(ShortcodesListViewController viewController, UITableView shortcodesTableView, string emptyText)
            {
                this.viewController = viewController;
                this.shortcodesTableView = shortcodesTableView;
                this.emptyText = emptyText;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                {
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as WaitTableViewCell ?? WaitTableViewCell.Create();
                }

                if (shortcodePreviewsInView.Count < 1)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                    emptyCell.Initialize(emptyText);
                    return emptyCell;
                }

                var sp = shortcodePreviewsInView[indexPath.Row];

                var cell = tableView.DequeueReusableCell(ShortcodesTableViewCell.Key) as ShortcodesTableViewCell ?? ShortcodesTableViewCell.Create();
                cell.Initialize(sp);
                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading)
                    return 1;

                if (shortcodePreviewsInView.Count < 1)
                    return 1;

                return shortcodePreviewsInView.Count;
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return Height;
            }

            public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
            {
                return true;
            }

            public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
            {
                var actions = new List<UITableViewRowAction>();

                var shortcodePreview = shortcodePreviewsInView[indexPath.Row];

                var moreAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("more"), (a, ip) => { viewController.EndEditing(); }); // TODO
                moreAction.BackgroundColor = Theme.Blue;
                actions.Add(moreAction);

                var copyToWorktrayAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("copy_to_worktray"), (a, ip) => { viewController.EndEditing(); }); // TODO
                copyToWorktrayAction.BackgroundColor = Theme.DarkBlue;
                actions.Add(copyToWorktrayAction);

                return actions.ToArray();
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                if (tableView.Editing) return;

                var sp = shortcodePreviewsInView[indexPath.Row];
                viewController.ShortcodeSelected(tableView, sp);
            }

            public void AppendItems(List<ShortcodePreview> shortcodePreviews)
            {
                loading = false;

                shortcodePreviewsInView.AddRange(shortcodePreviews);
                shortcodesTableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Automatic);
            }

            public void Reset()
            {
                loading = true;

                shortcodePreviewsInView.Clear();
                shortcodesTableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Automatic);
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                viewController = null;
                shortcodesTableView = null;
                shortcodePreviewsInView = null;
            }
        }
    }
}
