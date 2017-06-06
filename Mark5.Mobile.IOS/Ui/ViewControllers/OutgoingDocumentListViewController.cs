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
using Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentView;
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
        UITableView tableView;

        bool refreshing;

        #region UIViewController overrides

        public override void LoadView()
        {
            base.LoadView();

            InitializeNavigationBar();
            InitializeView();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ExtendedLayoutIncludesOpaqueBars = true;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            InitializeNavigationBarTitle();
            InitializeHandlers();

            if (tableView?.IndexPathForSelectedRow != null)
                tableView.DeselectRow(tableView.IndexPathForSelectedRow, true);

            if (tableView?.IndexPathsForSelectedRows?.Length > 0)
                foreach (var selectedIndexPath in tableView?.IndexPathsForSelectedRows)
                    tableView.DeselectRow(selectedIndexPath, true);

            ReachabilityBar.Attach(View, tableView, (float) NavigationController.BottomLayoutGuide.Length, UITextAlignment.Left);
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public override async void ViewDidAppear(bool animated)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info($"{nameof(OutgoingDocumentListViewController)} appeared");

            await RefreshData();

            if (IsBeingDismissed)
                return;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeinitializeHandlers();
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning($"{nameof(DocumentsListViewController)} received memory warning!");

            var ds = tableView?.DataSource as DataSource;
            ds?.Reset();

            GC.Collect();
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

            tableView = new UITableView();
            tableView.ClipsToBounds = false;

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
            tableView.Source = new DataSource(this, tableView, Localization.GetString("folder_empty"));
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void

            tableView.RowHeight = UITableView.AutomaticDimension;
            tableView.EstimatedRowHeight = 75f;
            tableView.AllowsSelectionDuringEditing = false;
            tableView.AllowsMultipleSelectionDuringEditing = true;
            tableView.TranslatesAutoresizingMaskIntoConstraints = false;
            View.AddSubview(tableView);
            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1f, 0f)
            });

            var longPressRecognizer = new UILongPressGestureRecognizer(this, new Selector("longPressed:"))
            {
                MinimumPressDuration = 1f,
                Delegate = this
            };
            tableView.AddGestureRecognizer(longPressRecognizer);

            refreshControl = new UIRefreshControl();
            refreshControl.BackgroundColor = UIColor.White;
            tableView.AddSubview(refreshControl);
        }

        void InitializeNavigationBarTitle()
        {
            UIView.AnimationsEnabled = false;
            NavigationItem.Title = outgoingFolder.Name;
            NavigationItem.Prompt = null;
            UIView.AnimationsEnabled = true;
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

        public void DocumentSelected(OutgoingDocumentContainer container)
        {
            if (tableView.Editing || container.Info.State == OutgoingDocumentState.Sending)
                return;

            if (SplitViewController != null && !SplitViewController.Collapsed)
            {
                var ds = (DataSource) tableView.Source;

                var nc = (UINavigationController) SplitViewController.ViewControllers[1];
                nc.PopToViewController(nc.ViewControllers[0], false);

                var vc = (DocumentViewController) nc.ViewControllers[0];

                if (vc.IsShowingOutgoingDocumentWithGuid(container.Info.Identifier))
                    return;

                vc.HidesBottomBarWhenPushed = false;

                vc.ClearData();
                vc.SetData(container.Info.Identifier);
                vc.RefreshData();
            }
            else
            {
                var vc = new DocumentViewController();
                vc.SetData(container.Info.Identifier);
                vc.SetRefreshDataOnAppear();

                NavigationController.PushViewController(vc, true);
            }
        }

        [Export("longPressed:")]
        public void LongPressed(UILongPressGestureRecognizer recognizer)
        {
            if (tableView.Editing)
                return;

            StartEditing();

            var point = recognizer.LocationInView(tableView);
            var indexPath = tableView.IndexPathForRowAtPoint(point);

            tableView.SelectRow(indexPath, true, UITableViewScrollPosition.None);
        }

        void StartEditing()
        {
            tableView.SetEditing(true, true);
            NavigationItem.SetRightBarButtonItem(exitEditItem, true);
            NavigationItem.SetLeftBarButtonItem(editItem, true);
        }

        void ComposeDocumentItem_Clicked(object sender, EventArgs e)
        {
            var vc = new ComposeDocumentViewController
            {
                CreationModeFlag = DocumentCreationModeFlag.New,
                PreviousDocumentDirection = DocumentDirection.None
            };

            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        void ExitEditItem_Clicked(object sender, EventArgs e)
        {
            EndEditing();
        }

        void EndEditing()
        {
            tableView.SetEditing(false, true);
            NavigationItem.SetRightBarButtonItem(composeDocumentItem, true);
            NavigationItem.SetLeftBarButtonItem(NavigationItem.BackBarButtonItem, true);
        }

        async void EditItem_Clicked(object sender, EventArgs e)
        {
            if (tableView.IndexPathsForSelectedRows == null || tableView.IndexPathsForSelectedRows.Length < 1)
                return;

            var rows = tableView.IndexPathsForSelectedRows.ToArray();
            var selectedDocuments = rows.Select(ip => ((DataSource) tableView.Source).Items[ip.Row]).ToList();

            var result = await Dialogs.ShowListDialogAsync(this, null, new string[]
            {
                Localization.GetString("delete")
            }, editItem);

            if (result == 0)
                await Delete(selectedDocuments);
        }

        async void Delete(OutgoingDocumentContainer container)
        {
            await Delete(new List<OutgoingDocumentContainer>
            {
                container
            });
        }

        async Task Delete(List<OutgoingDocumentContainer> containers)
        {
            foreach (var container in containers)
                await Managers.DocumentsManager.DeleteOutgoingDocumentFolder(container.Info.Identifier);

            await RefreshData();
        }

        #endregion

        #region Refreshing

        async void RefreshControl_ValueChanged(object sender, EventArgs e)
        {
            await RefreshData(true);
        }

        async Task RefreshData(bool forceClear = false)
        {
            if (refreshing)
                return;

            refreshing = true;
            refreshControl.ValueChanged -= RefreshControl_ValueChanged;

            CommonConfig.Logger.Info($"Refreshing outgoing documents list");

            try
            {
                var ds = (DataSource) tableView.Source;

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
            BeginInvokeOnMainThread(() =>
            {
                var ds = (DataSource) tableView.Source;
                var row = ds.GetPosition(outgoingDocumentContainer.Info.Identifier);
                if (row >= 0)
                {
                    var container = ds.Items[row];
                    container.Info.State = OutgoingDocumentState.Sending;
                    ds.UpdateRow(row);
                }
            });
        }

        void OutgoingDocumentsManager_DocumentSendingFailed(object sender, OutgoingDocumentContainer outgoingDocumentContainer)
        {
            BeginInvokeOnMainThread(() =>
            {
                var ds = (DataSource) tableView.Source;
                var row = ds.GetPosition(outgoingDocumentContainer.Info.Identifier);
                if (row >= 0)
                {
                    var container = ds.Items[row];
                    container.Info.State = OutgoingDocumentState.Failed;
                    ds.UpdateRow(row);
                }
            });
        }

        void OutgoingDocumentsManager_DocumentSendingSuccessful(object sender, OutgoingDocumentContainer outgoingDocumentContainer)
        {
            BeginInvokeOnMainThread(() =>
            {
                var ds = (DataSource) tableView.Source;
                var row = ds.GetPosition(outgoingDocumentContainer.Info.Identifier);
                if (row >= 0)
                {
                    ds.Items.RemoveAt(row);
                    ds.RemoveRow(row);
                }
            });
        }

        #endregion

        class DataSource : UITableViewSource, IDisposable
        {
            static readonly nfloat Height = 100f;

            public bool Empty => Items.Count < 1;

            public List<OutgoingDocumentContainer> Items { get; private set; } = new List<OutgoingDocumentContainer>(1000);

            OutgoingDocumentListViewController viewController;
            UITableView documentsTableView;
            readonly string emptyText;

            bool loading = true;

            public DataSource(OutgoingDocumentListViewController viewController, UITableView documentsTableView, string emptyText)
            {
                this.viewController = viewController;
                this.documentsTableView = documentsTableView;
                this.emptyText = emptyText;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as WaitTableViewCell ?? WaitTableViewCell.Create();

                if (Items.Count < 1)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                    emptyCell.Initialize(emptyText);
                    return emptyCell;
                }

                var dp = Items[indexPath.Row];

                var cell = tableView.DequeueReusableCell(DocumentsTableViewCell.Key) as DocumentsTableViewCell ?? DocumentsTableViewCell.Create();
                cell.Initialize(dp);
                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading)
                    return 1;

                if (Items.Count < 1)
                    return 1;

                return Items.Count;
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

                var documentPreview = Items[indexPath.Row];

                var deleteAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("delete"), (a, ip) =>
                {
                    viewController.Delete(documentPreview);
                    viewController.EndEditing();
                });
                deleteAction.BackgroundColor = Theme.DarkBlue;
                actions.Add(deleteAction);

                return actions.ToArray();
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var dp = Items[indexPath.Row];
                viewController.DocumentSelected(dp);
            }

            public int GetPosition(Guid identifier)
            {
                return Items.FindIndex(o => o.Info.Identifier == identifier);
            }

            public void ReplaceItems(List<OutgoingDocumentContainer> containers)
            {
                loading = false;

                Items.Clear();
                Items.AddRange(containers);
                documentsTableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            public void Reset()
            {
                loading = true;

                Items.Clear();
                documentsTableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                viewController = null;
                documentsTableView = null;
                Items = null;
            }

            public void UpdateRow(int row)
            {
                documentsTableView.ReloadRows(new NSIndexPath[]
                {
                    NSIndexPath.FromRowSection(row, 0)
                }, UITableViewRowAnimation.Fade);
            }

            public void RemoveRow(int row)
            {
                if (Items.Count < 1 && row == 0)
                    UpdateRow(0); //We always keep a row for the empty table cell
                else
                    documentsTableView.DeleteRows(new NSIndexPath[]
                    {
                        NSIndexPath.FromRowSection(row, 0)
                    }, UITableViewRowAnimation.Fade);
            }
        }
    }
}