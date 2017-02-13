//
// Project: Mark5.Mobile.IOS
// File: OutgoingDocumentListViewController.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using ObjCRuntime;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class OutgoingDocumentListViewController : AbstractViewController, IPrimaryViewController, IUIGestureRecognizerDelegate
    {
        readonly Folder outgoingFolder = Folder.DocumentsOutgoingFolder;

        UIBarButtonItem composeDocumentItem;
        UIBarButtonItem exitEditItem;
        UIBarButtonItem editItem;

        UIRefreshControl refreshControl;
        UITableView documentsTableView;

        bool refreshing;

        #region UIViewController overrides

        public override void LoadView()
        {
            base.LoadView();

            InitializeNavigationBar();
            InitializeView();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            InitializeNavigationBarTitle();
            InitializeHandlers();
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public override async void ViewDidAppear(bool animated)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info($"{nameof(DocumentsListViewController)} appeared");

            await RefreshData();

            if (IsBeingDismissed) return;

            CommonConfig.Logger.Info($"Starting automatic refresh...");
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeinitializeHandlers();
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning($"{nameof(DocumentsListViewController)} received memory warning!");

            var ds = documentsTableView?.DataSource as DataSource;
            ds?.Reset();

            base.DidReceiveMemoryWarning();
        }

        #endregion

        #region Initialization

        void InitializeNavigationBar()
        {
            composeDocumentItem = new UIBarButtonItem();
            composeDocumentItem.Image = UIImage.FromBundle(Path.Combine("icons", "compose.png"));
            NavigationItem.SetRightBarButtonItem(composeDocumentItem, false);

            exitEditItem = new UIBarButtonItem(UIBarButtonSystemItem.Done);
            editItem = new UIBarButtonItem(UIBarButtonSystemItem.Edit);
        }

        void InitializeView()
        {
            AutomaticallyAdjustsScrollViewInsets = true;

            documentsTableView = new UITableView();
            documentsTableView.ClipsToBounds = false;

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
            documentsTableView.Source = new DataSource(this, documentsTableView, Localization.GetString("folder_empty"));
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void

            documentsTableView.RowHeight = UITableView.AutomaticDimension;
            documentsTableView.EstimatedRowHeight = 75f;
            documentsTableView.AllowsSelectionDuringEditing = false;
            documentsTableView.AllowsMultipleSelectionDuringEditing = true;
            documentsTableView.TranslatesAutoresizingMaskIntoConstraints = false;
            View.AddSubview(documentsTableView);
            View.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(documentsTableView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(documentsTableView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(documentsTableView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(documentsTableView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1.0f, 0.0f)
                });

            var longPressRecognizer = new UILongPressGestureRecognizer(this, new Selector("longPressed:"))
            {
                MinimumPressDuration = 1f,
                Delegate = this
            };
            documentsTableView.AddGestureRecognizer(longPressRecognizer);

            refreshControl = new UIRefreshControl();
            refreshControl.BackgroundColor = UIColor.White;
            refreshControl.AttributedTitle = Localization.GetNSAttributedString("pull_to_refresh");
            documentsTableView.AddSubview(refreshControl);
        }

        void InitializeNavigationBarTitle()
        {
            NavigationItem.Title = outgoingFolder.Name;
        }

        void InitializeHandlers()
        {
            if (composeDocumentItem != null)
                composeDocumentItem.Clicked += ComposeDocumentItem_Clicked;

            if (exitEditItem != null)
                exitEditItem.Clicked += ExitEditItem_Clicked;

            if (editItem != null)
                editItem.Clicked += EditItem_Clicked;

            if (refreshControl != null)
                refreshControl.ValueChanged += RefreshControl_ValueChanged;

            Managers.OutgoingDocumentsManager.DocumentBeingSent += OutgoingDocumentsManager_DocumentBeingSent;
            Managers.OutgoingDocumentsManager.DocumentSendingFailed += OutgoingDocumentsManager_DocumentSendingFailed;
            Managers.OutgoingDocumentsManager.DocumentSendingSuccessful += OutgoingDocumentsManager_DocumentSendingSuccessful;
        }

        void DeinitializeHandlers()
        {
            if (composeDocumentItem != null)
                composeDocumentItem.Clicked -= ComposeDocumentItem_Clicked;

            if (exitEditItem != null)
                exitEditItem.Clicked -= ExitEditItem_Clicked;

            if (editItem != null)
                editItem.Clicked -= EditItem_Clicked;

            if (refreshControl != null)
                refreshControl.ValueChanged -= RefreshControl_ValueChanged;

            Managers.OutgoingDocumentsManager.DocumentBeingSent -= OutgoingDocumentsManager_DocumentBeingSent;
            Managers.OutgoingDocumentsManager.DocumentSendingFailed -= OutgoingDocumentsManager_DocumentSendingFailed;
            Managers.OutgoingDocumentsManager.DocumentSendingSuccessful -= OutgoingDocumentsManager_DocumentSendingSuccessful;
        }

        #endregion

        #region Actions

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public async void DocumentSelected(OutgoingDocumentContainer container)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            if (SplitViewController == null || SplitViewController.Collapsed)
            {
                var ds = (DataSource)documentsTableView.Source;

                var documentViewController = new DocumentViewController();
                documentViewController.OutgoingDocumentIdentifier = container.Info.Identifier;
                documentViewController.Folder = outgoingFolder;
                documentViewController.HidesBottomBarWhenPushed = true;

                NavigationController.PushViewController(documentViewController, true);
            }
            else
            {
                var ds = (DataSource)documentsTableView.Source;

                var documentNavigationController = ((UINavigationController)SplitViewController.ViewControllers[1]);
                documentNavigationController.PopToViewController(documentNavigationController.ViewControllers[0], false);

                var documentViewController = (DocumentViewController)documentNavigationController.ViewControllers[0];

                if (documentViewController.Folder != outgoingFolder || documentViewController.DocumentPreview != container.DocumentPreview)
                {
                    documentViewController.GetNextDocumentPreview = null;
                    documentViewController.GetPreviousDocumentPreview = null;

                    documentViewController.FolderId = null;
                    documentViewController.DocumentId = null;

                    documentViewController.OutgoingDocumentIdentifier = container.Info.Identifier;
                    documentViewController.Folder = outgoingFolder;
                    documentViewController.HidesBottomBarWhenPushed = false;

                    await documentViewController.Reload();
                }
            }
        }

        [Export("longPressed:")]
        public void LongPressed(UILongPressGestureRecognizer recognizer)
        {
            if (documentsTableView.Editing) return;

            StartEditing();

            var point = recognizer.LocationInView(documentsTableView);
            var indexPath = documentsTableView.IndexPathForRowAtPoint(point);

            documentsTableView.SelectRow(indexPath, true, UITableViewScrollPosition.None);
        }

        void StartEditing()
        {
            documentsTableView.SetEditing(true, true);
            NavigationItem.SetRightBarButtonItem(exitEditItem, true);
            NavigationItem.SetLeftBarButtonItem(editItem, true);
        }

        void ComposeDocumentItem_Clicked(object sender, EventArgs e)
        {
            var composeDocumentViewController = new ComposeDocumentViewController
            {
                CreationModeFlag = DocumentCreationModeFlag.New,
                PreviousDocumentDirection = DocumentDirection.None
            };

            var composeDocumentNavigationController = new UINavigationController(composeDocumentViewController);
            composeDocumentNavigationController.ModalPresentationStyle = UIModalPresentationStyle.PageSheet;
            PresentViewController(composeDocumentNavigationController, true, null);
        }

        void ExitEditItem_Clicked(object sender, EventArgs e) => EndEditing();

        void EndEditing()
        {
            documentsTableView.SetEditing(false, true);
            NavigationItem.SetRightBarButtonItem(composeDocumentItem, true);
            NavigationItem.SetLeftBarButtonItem(NavigationItem.BackBarButtonItem, true);
        }

        async void EditItem_Clicked(object sender, EventArgs e)
        {
            if (documentsTableView.IndexPathsForSelectedRows == null || documentsTableView.IndexPathsForSelectedRows.Length < 1) return;

            var rows = documentsTableView.IndexPathsForSelectedRows.ToArray();
            var selectedDocuments = rows.Select(ip => ((DataSource)documentsTableView.Source).Items[ip.Row]).ToList();

            var result = await Dialogs.ShowListDialogAsync(this, null, new string[] { Localization.GetString("delete") }, editItem);

            if (result == 0)
            {
                await Delete(selectedDocuments);
            }
        }

        async void Delete(OutgoingDocumentContainer container) => await Delete(new List<OutgoingDocumentContainer> { container });

        async Task Delete(List<OutgoingDocumentContainer> containers)
        {
            foreach (var container in containers)
            {
                await Managers.DocumentsManager.DeleteOutgoingDocumentFolder(container.Info.Identifier);
            }

            await RefreshData();
        }

        #endregion

        #region Refreshing

        async void RefreshControl_ValueChanged(object sender, EventArgs e) => await RefreshData(forceClear: true);

        async Task RefreshData(bool forceClear = false)
        {
            if (refreshing) return;

            refreshing = true;
            refreshControl.ValueChanged -= RefreshControl_ValueChanged;

            CommonConfig.Logger.Info($"Refreshing outgoing documents list");

            try
            {
                var ds = (DataSource)documentsTableView.Source;

                if (forceClear)
                    ds.Reset();

                var outgoingDocumentContainers = await Managers.DocumentsManager.GetOutgoingDocumentContainersPreviewAsync();
                ds.ReplaceItems(outgoingDocumentContainers);

            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Could not refresh outgoing document list", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);
            }

            refreshControl.EndRefreshing();
            refreshControl.ValueChanged += RefreshControl_ValueChanged;

            refreshing = false;
        }

        #endregion

        #region OutgoingDocumentManager event handlers

        void OutgoingDocumentsManager_DocumentBeingSent(object sender, OutgoingDocumentContainer outgoingDocumentContainer)
        {
            var ds = (DataSource)documentsTableView.DataSource;
            var row = ds.GetPosition(outgoingDocumentContainer.Info.Identifier);
            if (row >= 0)
            {
                var container = ds.Items[row];
                container.Info.State = OutgoingDocumentState.Sending;
                ds.UpdateRow(row);
            }
        }

        void OutgoingDocumentsManager_DocumentSendingFailed(object sender, OutgoingDocumentContainer outgoingDocumentContainer)
        {
            var ds = (DataSource)documentsTableView.DataSource;
            var row = ds.GetPosition(outgoingDocumentContainer.Info.Identifier);
            if (row >= 0)
            {
                var container = ds.Items[row];
                container.Info.State = OutgoingDocumentState.Failed;
                ds.UpdateRow(row);
            }
        }

        void OutgoingDocumentsManager_DocumentSendingSuccessful(object sender, OutgoingDocumentContainer outgoingDocumentContainer)
        {
            var ds = (DataSource)documentsTableView.DataSource;
            var row = ds.GetPosition(outgoingDocumentContainer.Info.Identifier);
            if (row >= 0)
            {
                ds.Items.RemoveAt(row);
                ds.RemoveRow(row);
            }
        }

        #endregion

        class DataSource : UITableViewSource, IDisposable
        {
            static readonly nfloat Height = 100f;

            public bool Empty
            {
                get
                {
                    return outgoingDocumentPreviewsInView.Count < 1;
                }
            }

            public List<OutgoingDocumentContainer> Items
            {
                get
                {
                    return outgoingDocumentPreviewsInView;
                }
            }

            OutgoingDocumentListViewController viewController;
            UITableView documentsTableView;
            readonly string emptyText;

            bool loading = true;
            List<OutgoingDocumentContainer> outgoingDocumentPreviewsInView = new List<OutgoingDocumentContainer>(1000);

            public DataSource(OutgoingDocumentListViewController viewController, UITableView documentsTableView, string emptyText)
            {
                this.viewController = viewController;
                this.documentsTableView = documentsTableView;
                this.emptyText = emptyText;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                {
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as WaitTableViewCell ?? WaitTableViewCell.Create();
                }

                if (outgoingDocumentPreviewsInView.Count < 1)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                    emptyCell.Initialize(emptyText);
                    return emptyCell;
                }

                var dp = outgoingDocumentPreviewsInView[indexPath.Row];

                var cell = tableView.DequeueReusableCell(DocumentsTableViewCell.Key) as DocumentsTableViewCell ?? DocumentsTableViewCell.Create();
                cell.Initialize(dp);
                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading)
                    return 1;

                if (outgoingDocumentPreviewsInView.Count < 1)
                    return 1;

                return outgoingDocumentPreviewsInView.Count;
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

                var documentPreview = outgoingDocumentPreviewsInView[indexPath.Row];

                var deleteAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("delete"), (a, ip) => { viewController.Delete(documentPreview); viewController.EndEditing(); });
                deleteAction.BackgroundColor = Theme.DarkBlue;
                actions.Add(deleteAction);

                return actions.ToArray();
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var dp = outgoingDocumentPreviewsInView[indexPath.Row];
                viewController.DocumentSelected(dp);
            }

            public int GetPosition(Guid identifier)
            {
                return outgoingDocumentPreviewsInView.FindIndex(o => o.Info.Identifier == identifier);
            }

            public void ReplaceItems(List<OutgoingDocumentContainer> containers)
            {
                loading = false;

                outgoingDocumentPreviewsInView.Clear();
                outgoingDocumentPreviewsInView.AddRange(containers);
                documentsTableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Automatic);
            }

            public void Reset()
            {
                loading = true;

                outgoingDocumentPreviewsInView.Clear();
                documentsTableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Automatic);
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                viewController = null;
                documentsTableView = null;
                outgoingDocumentPreviewsInView = null;
            }

            public void UpdateRow(int row)
            {
                documentsTableView.ReloadRows(new NSIndexPath[] { NSIndexPath.FromRowSection(row, 0) }, UITableViewRowAnimation.Automatic);
            }

            public void RemoveRow(int row)
            {
                documentsTableView.DeleteRows(new NSIndexPath[] { NSIndexPath.FromRowSection(row, 0) }, UITableViewRowAnimation.Automatic);
            }

        }
    }
}
