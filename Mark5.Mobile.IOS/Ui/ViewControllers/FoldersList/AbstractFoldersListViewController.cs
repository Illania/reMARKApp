//
// Project: Mark5.Mobile.IOS
// File: AbstractFoldersListViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
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
        
        protected ModuleType Module { get; private set; }

        UIBarButtonItem editModeItem;
        UIBarButtonItem composeDocumentItem;

        UIRefreshControl RefreshControl;
        UITableView FoldersListView;

        UITableViewController SearchResultsController;

        protected AbstractFoldersListViewController(ModuleType module)
        {
            Module = module;
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

            DeinitializeHandlers();
        }

        public override void DidReceiveMemoryWarning()
        {
            var ds = FoldersListView?.DataSource as DataSource;
            ds?.Clear();
            
            base.DidReceiveMemoryWarning();
        }

        void InitializeNavigationBar()
        {
            if (Module == ModuleType.Documents)
            {
                composeDocumentItem = new UIBarButtonItem();
                composeDocumentItem.Image = UIImage.FromBundle(Path.Combine("icons", "compose.png"));
                NavigationItem.SetRightBarButtonItem(composeDocumentItem, false);

                editModeItem = new UIBarButtonItem();
                editModeItem.Title = Localization.GetString("edit");
                NavigationItem.SetLeftBarButtonItem(editModeItem, false);

                return;
            }

            if (Module == ModuleType.Contacts || Module == ModuleType.Shortcodes)
            {
                editModeItem = new UIBarButtonItem();
                editModeItem.Title = Localization.GetString("edit");
                NavigationItem.SetLeftBarButtonItem(editModeItem, false);

                return;
            }

            throw new ArgumentException(nameof(Module));
        }

        void InitializeView()
        {
            AutomaticallyAdjustsScrollViewInsets = true;

            FoldersListView = new UITableView(CGRect.Empty, UITableViewStyle.Grouped);
            FoldersListView.CellLayoutMarginsFollowReadableWidth = false;
            FoldersListView.ClipsToBounds = false;
            FoldersListView.Source = new DataSource(this, FoldersListView, Module);
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
            if (composeDocumentItem != null)
                composeDocumentItem.Clicked += ComposeDocumentItem_Clicked;

            if (editModeItem != null)
                editModeItem.Clicked += EditModeItem_Clicked;

            RefreshControl.ValueChanged += RefreshControl_ValueChanged;
        }

        void DeinitializeHandlers()
        {
            if (composeDocumentItem != null)
                composeDocumentItem.Clicked -= ComposeDocumentItem_Clicked;

            if (editModeItem != null)
                editModeItem.Clicked -= EditModeItem_Clicked;

            RefreshControl.ValueChanged -= RefreshControl_ValueChanged;
        }

        async void RefreshData()
        {
            RefreshControl.ValueChanged -= RefreshControl_ValueChanged;

            try
            {
                var favorites = await Managers.FoldersManager.GetFavoriteFoldersAsync(Module);
                var folders = await Managers.FoldersManager.GetFoldersAsync(Folder.RootForModule(Module));

                var ds = FoldersListView.Source as DataSource;
                ds.SetFolders(DataSource.Section.Favorites, favorites);
                ds.SetFolders(DataSource.Section.Folders, folders);
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
                    return foldersInView.All(kv => kv.Value.Count < 1);
                }
            }

            public static class Section
            {
                public static readonly nint Favorites = 0;
                public static readonly nint Folders = 1;
                public static readonly nint Local = 2;
            }

            AbstractFoldersListViewController viewController;
            UITableView tableView;

            bool[] firstLoad;
            Dictionary<nint, List<Folder>> foldersInView;


            public DataSource(AbstractFoldersListViewController viewController, UITableView tableView, ModuleType module)
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

                cell.Initialize(foldersInView[indexPath.LongSection][indexPath.Row]);

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
                var folder = foldersInView[indexPath.LongSection][indexPath.Row];
                viewController.FolderSelected(folder);
            }

            public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
            {
                var folder = foldersInView[indexPath.LongSection][indexPath.Row];
                viewController.FolderDeselected(folder);
            }

            void Cell_ExpandCollapseClicked(object sender, Folder f)
            {
                viewController.FolderExpand(f);
            }

            public override string TitleForHeader(UITableView tableView, nint section)
            {
                if (section == Section.Favorites)
                    return Localization.GetString("favorites");

                if (section == Section.Folders)
                    return Localization.GetString("folders");

                if (section == Section.Local)
                    return Localization.GetString("local");

                throw new ArgumentException(nameof(section));
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
