//
// Project: Mark5.Mobile.IOS
// File: AbstractFoldersListViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList
{
    
    public abstract class AbstractFoldersListViewController : ViewController
    {

        protected bool isRootOfFoldersList;
        protected Folder folder { get; private set; }

        protected UIBarButtonItem EditModeItem;
        protected UIBarButtonItem ComposeDocumentItem;

        protected UIRefreshControl RefreshControl;
        protected UITableView FoldersListView;

        protected AbstractFoldersListViewController(ModuleType module)
        {
            isRootOfFoldersList = true;
            folder = Folder.RootForModule(module);
        }

        protected AbstractFoldersListViewController(Folder folder)
        {
            isRootOfFoldersList = false;
            this.folder = folder;
        }

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

            RefreshData();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            ClearNavigationBarTitle();
            DeinitializeHandlers();
        }

        public override void DidReceiveMemoryWarning()
        {
            var ds = FoldersListView?.DataSource as GrouppedDataSource;
            ds?.Clear();
            
            base.DidReceiveMemoryWarning();
        }

        void InitializeNavigationBarTitle()
        {
            if (isRootOfFoldersList)
            {
                NavigationItem.Title = Localization.GetString("documents");
            }
            else
            {
                NavigationItem.Title = folder.Name;
                NavigationItem.Prompt = Localization.GetString("documents");
            }
        }

        void ClearNavigationBarTitle()
        {
            if (isRootOfFoldersList)
            {
                NavigationItem.Title = Localization.GetString("back");
            }
        }

        void InitializeNavigationBar()
        {
            if (folder.Module == ModuleType.Documents)
            {
                ComposeDocumentItem = new UIBarButtonItem();
                ComposeDocumentItem.Image = UIImage.FromBundle(Path.Combine("icons", "compose.png"));
                NavigationItem.SetRightBarButtonItem(ComposeDocumentItem, false);

                if (isRootOfFoldersList)
                {
                    EditModeItem = new UIBarButtonItem();
                    EditModeItem.Title = Localization.GetString("edit");
                    NavigationItem.SetLeftBarButtonItem(EditModeItem, false);
                }

                return;
            }

            if (folder.Module == ModuleType.Contacts || folder.Module == ModuleType.Shortcodes)
            {
                if (isRootOfFoldersList)
                {
                    EditModeItem = new UIBarButtonItem();
                    EditModeItem.Title = Localization.GetString("edit");
                    NavigationItem.SetLeftBarButtonItem(EditModeItem, false);
                }

                return;
            }

            throw new ArgumentException(nameof(folder.Module));
        }

        void InitializeView()
        {
            AutomaticallyAdjustsScrollViewInsets = true;

            FoldersListView = new UITableView(CGRect.Empty, UITableViewStyle.Grouped);
            FoldersListView.ClipsToBounds = false;
            if (isRootOfFoldersList)
                FoldersListView.Source = new GrouppedDataSource(this, FoldersListView, folder.Module);
            else
                FoldersListView.Source = new DataSource(this, FoldersListView, folder.Module);
            FoldersListView.AllowsSelectionDuringEditing = false;
            FoldersListView.TranslatesAutoresizingMaskIntoConstraints = false;
            View.AddSubview(FoldersListView);
            View.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(FoldersListView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(FoldersListView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(FoldersListView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(FoldersListView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1.0f, 0.0f)
                });

            RefreshControl = new UIRefreshControl();
            RefreshControl.BackgroundColor = UIColor.White;
            RefreshControl.AttributedTitle = Localization.GetNSAttributedString("pull_to_refresh");
            FoldersListView.AddSubview(RefreshControl);
        }

        void InitializeSearchBar()
        {
            // TODO
        }

        void ComposeDocumentItem_Clicked(object sender, EventArgs e)
        {
            // TODO
        }

        void EditModeItem_Clicked(object sender, EventArgs e)
        {
            // TODO
        }

        void RefreshControl_ValueChanged(object sender, EventArgs e) => RefreshData();

        void InitializeHandlers()
        {
            if (ComposeDocumentItem != null)
                ComposeDocumentItem.Clicked += ComposeDocumentItem_Clicked;

            if (EditModeItem != null)
                EditModeItem.Clicked += EditModeItem_Clicked;

            RefreshControl.ValueChanged += RefreshControl_ValueChanged;
        }

        void DeinitializeHandlers()
        {
            if (ComposeDocumentItem != null)
                ComposeDocumentItem.Clicked -= ComposeDocumentItem_Clicked;

            if (EditModeItem != null)
                EditModeItem.Clicked -= EditModeItem_Clicked;

            RefreshControl.ValueChanged -= RefreshControl_ValueChanged;
        }

        async void RefreshData()
        {
            RefreshControl.ValueChanged -= RefreshControl_ValueChanged;

            try
            {
                if (isRootOfFoldersList)
                {
                    var ds = FoldersListView.Source as GrouppedDataSource;

                    var favorites = await Managers.FoldersManager.GetFavoriteFoldersAsync(folder.Module);
                    var folders = await Managers.FoldersManager.GetFoldersAsync(folder);

                    var favoriteIds = favorites.Select(f => f.Id);
                    var folderIds = folders.Select(f => f.Id);
                    var allIds = favoriteIds.Union(folderIds).Distinct();

                    var favoritesStatus = new Dictionary<int, bool>();
                    var offlineStatus = new Dictionary<int, bool>();

                    foreach (var id in allIds)
                    {
                        favoritesStatus[id] = await Managers.FoldersManager.IsFolderFavouriteAsync(folder.Module, id);
                        offlineStatus[id] = await Managers.FoldersManager.IsFolderOfflineAsync(folder.Module, id);
                    }

                    ds.FavoriteStatus = favoritesStatus;
                    ds.OfflineStatus = offlineStatus;

                    ds.SetFolders(GrouppedDataSource.Section.Favorites, favorites);
                    ds.SetFolders(GrouppedDataSource.Section.Local, Folder.LocalRootForModule(ModuleType.Documents).SubFolders);
                    ds.SetFolders(GrouppedDataSource.Section.Folders, folders);
                }
                else
                {
                    var ds = FoldersListView.Source as DataSource;

                    var folders = await Managers.FoldersManager.GetFoldersAsync(folder);

                    var folderIds = folders.Select(f => f.Id).Distinct();

                    var favoritesStatus = new Dictionary<int, bool>();
                    var offlineStatus = new Dictionary<int, bool>();

                    foreach (var id in folderIds)
                    {
                        favoritesStatus[id] = await Managers.FoldersManager.IsFolderFavouriteAsync(folder.Module, id);
                        offlineStatus[id] = await Managers.FoldersManager.IsFolderOfflineAsync(folder.Module, id);
                    }

                    ds.FavoriteStatus = favoritesStatus;
                    ds.OfflineStatus = offlineStatus;

                    ds.SetFolders(folders); 
                }
            }
            catch (Exception ex)
            {
            }

            RefreshControl.EndRefreshing();
            RefreshControl.ValueChanged += RefreshControl_ValueChanged;
        }

        protected virtual void FolderSelected(Folder folder)
        {
        }

        protected virtual void FolderDeselected(Folder folder)
        {
        }

        protected virtual void FolderExpand(Folder folder)
        {
        }

        protected class DataSource : UITableViewSource, IDisposable
        {

            public bool Empty
            {
                get
                {
                    return foldersInView.Count < 1;
                }
            }

            public Dictionary<int, bool> FavoriteStatus { get; set; }
            public Dictionary<int, bool> OfflineStatus { get; set; }

            AbstractFoldersListViewController viewController;
            UITableView tableView;

            bool firstLoad;
            List<Folder> foldersInView;


            public DataSource(AbstractFoldersListViewController viewController, UITableView tableView, ModuleType module)
            {
                this.tableView = tableView;
                this.viewController = viewController;

                firstLoad = true;
                foldersInView = new List<Folder>();
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.DequeueReusableCell(FoldersListViewCell.Key) as FoldersListViewCell;
                if (cell == null)
                {
                    cell = FoldersListViewCell.Create();
                    cell.ExpandCollapseClicked += Cell_ExpandCollapseClicked;
                }

                var f = foldersInView[indexPath.Row];
                var isFavorite = false;
                FavoriteStatus.TryGetValue(f.Id, out isFavorite);
                var isOffline = false;
                OfflineStatus.TryGetValue(f.Id, out isOffline);

                cell.Initialize(f, isFavorite, isOffline);

                return cell;
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return 44f;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return foldersInView.Count;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var f = foldersInView[indexPath.Row];
                viewController.FolderSelected(f);
            }

            public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
            {
                var f = foldersInView[indexPath.Row];
                viewController.FolderDeselected(f);
            }

            void Cell_ExpandCollapseClicked(object sender, Folder f)
            {
                viewController.FolderExpand(f);
            }

            protected override void Dispose(bool disposing)
            {
                viewController = null;
                tableView = null;

                firstLoad = true;
                foldersInView = null;
            }

            public void SetFolders(List<Folder> folders)
            {
                foldersInView.Clear();
                foldersInView.AddRange(folders);
                tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Automatic);
            }

            public void Clear()
            {
                firstLoad = true;
                foldersInView.Clear();
            }
        }

        protected class GrouppedDataSource : UITableViewSource, IDisposable
        {

            public static class Section
            {
                public static readonly nint Favorites = 0;
                public static readonly nint Folders = 1;
                public static readonly nint Local = 2;
            }

            public bool Empty
            {
                get
                {
                    return foldersInView.All(kv => kv.Value.Count < 1);
                }
            }

            public Dictionary<int, bool> FavoriteStatus { get; set; }
            public Dictionary<int, bool> OfflineStatus { get; set; }

            AbstractFoldersListViewController viewController;
            UITableView tableView;

            bool[] firstLoad;
            Dictionary<nint, List<Folder>> foldersInView;


            public GrouppedDataSource(AbstractFoldersListViewController viewController, UITableView tableView, ModuleType module)
            {
                this.tableView = tableView;
                this.viewController = viewController;

                if (module == ModuleType.Documents)
                {
                    firstLoad = new []{ true, true, true};
                    foldersInView = new Dictionary<nint, List<Folder>>
                    {
                        [Section.Favorites] = new List<Folder>(),
                        [Section.Folders] = new List<Folder>(),
                        [Section.Local] = new List<Folder>()
                    };

                    return;
                }

                if (module == ModuleType.Contacts || module == ModuleType.Shortcodes)
                {
                    firstLoad = new[] { true, true };
                    foldersInView = new Dictionary<nint, List<Folder>>
                    {
                        [Section.Favorites] = new List<Folder>(),
                        [Section.Folders] = new List<Folder>()
                    };

                    return;
                }

                throw new ArgumentException(nameof(module));
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.DequeueReusableCell(FoldersListViewCell.Key) as FoldersListViewCell;
                if (cell == null)
                {
                    cell = FoldersListViewCell.Create();
                    cell.ExpandCollapseClicked += Cell_ExpandCollapseClicked;
                }

                var f = foldersInView[indexPath.LongSection][indexPath.Row];
                var isFavorite = false;
                FavoriteStatus.TryGetValue(f.Id, out isFavorite);
                var isOffline = false;
                OfflineStatus.TryGetValue(f.Id, out isOffline);

                cell.Initialize(f, isFavorite, isOffline);

                return cell;
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return 44f;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return foldersInView[section].Count;
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                return foldersInView.Keys.Count;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var f = foldersInView[indexPath.LongSection][indexPath.Row];
                viewController.FolderSelected(f);
            }

            public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
            {
                var f = foldersInView[indexPath.LongSection][indexPath.Row];
                viewController.FolderDeselected(f);
            }

            void Cell_ExpandCollapseClicked(object sender, Folder f)
            {
                viewController.FolderExpand(f);
            }

            public override string TitleForHeader(UITableView tableView, nint section)
            {
                if (section == Section.Favorites)
                    return Localization.GetString("favorites").ToUpper(CultureInfo.CurrentCulture);

                if (section == Section.Folders)
                    return Localization.GetString("folders").ToUpper(CultureInfo.CurrentCulture);

                if (section == Section.Local)
                    return Localization.GetString("local_folders").ToUpper(CultureInfo.CurrentCulture);

                throw new ArgumentException(nameof(section));
            }

            public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
            {
                var actions = new List<UITableViewRowAction>();

                if (indexPath.Section == Section.Favorites)
                {
                    var rff = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("remove_from_favorites"), (arg1, arg2) => { });
                    rff.BackgroundColor = Theme.Brown;
                    actions.Add(rff);
                }

                if (indexPath.Section == Section.Folders)
                {
                    var rff = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("remove_from_favorites"), (arg1, arg2) => { });
                    rff.BackgroundColor = Theme.Brown;
                    actions.Add(rff);

                    var ec = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("enable_caching"), (arg1, arg2) => { });
                    ec.BackgroundColor = Theme.DarkBlue;
                    actions.Add(ec);

                    var n = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("notify"), (arg1, arg2) => { });
                    n.BackgroundColor = Theme.Blue;
                    actions.Add(n);
                }

                return actions.ToArray();
            }

            protected override void Dispose(bool disposing)
            {
                viewController = null;
                tableView = null;

                firstLoad = null;
                foldersInView = null;
            }

            public void SetFolders(nint section, List<Folder> folders)
            {
                foldersInView[section].Clear();
                foldersInView[section].AddRange(folders);
                tableView.ReloadSections(NSIndexSet.FromIndex(section), UITableViewRowAnimation.Automatic);
            }

            public void Clear()
            {
                for (var i = 0; i < firstLoad.Length; i++)
                    firstLoad[i] = true;

                foreach (var kv in foldersInView)
                    kv.Value.Clear();
            }
       }
   }
}
