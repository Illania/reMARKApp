using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class DocumentsToUploadListViewController : AbstractViewController, IPrimaryViewController
    {
        UITableView tableView;

        bool refreshing;

        #region UIViewController overrides

        public override void LoadView()
        {
            base.LoadView();

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

            if (tableView?.IndexPathForSelectedRow != null)
                tableView.DeselectRow(tableView.IndexPathForSelectedRow, true);

            if (tableView?.IndexPathsForSelectedRows?.Length > 0)
                foreach (var selectedIndexPath in tableView?.IndexPathsForSelectedRows)
                    tableView.DeselectRow(selectedIndexPath, true);

            ReachabilityBar.Attach(View, tableView, (float)NavigationController.BottomLayoutGuide.Length, UITextAlignment.Left);
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public override async void ViewDidAppear(bool animated)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info($"{nameof(DocumentsToUploadListViewController)} appeared");

            await RefreshData();

            if (IsBeingDismissed)
                return;
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

        void InitializeView()
        {
            AutomaticallyAdjustsScrollViewInsets = true;

            tableView = new UITableView
            {
                ClipsToBounds = false,
                RowHeight = UITableView.AutomaticDimension,
                EstimatedRowHeight = 75f,
                AllowsSelectionDuringEditing = false,
                AllowsMultipleSelectionDuringEditing = true,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            tableView.Source = new DataSource(this, tableView, Localization.GetString("folder_empty"));
            View.AddSubview(tableView);
            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1f, 0f)
            });
        }

        void InitializeNavigationBarTitle()
        {
            UIView.AnimationsEnabled = false;
            NavigationItem.Title = Localization.GetString("outgoing");
            NavigationItem.Prompt = null;
            UIView.AnimationsEnabled = true;
        }

        #endregion

        #region Actions

        public void DocumentSelected(DocumentToUploadContainer container)
        {
            if (tableView.Editing || container.Info.State == OutgoingDocumentState.Sending)
                return;

            if (SplitViewController != null && !SplitViewController.Collapsed)
            {
                var ds = (DataSource)tableView.Source;

                var nc = (UINavigationController)SplitViewController.ViewControllers[1];
                nc.PopToViewController(nc.ViewControllers[0], false);

                var vc = (DocumentViewController)nc.ViewControllers[0];

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

        void EndEditing()
        {
            tableView.SetEditing(false, true);
            NavigationItem.SetLeftBarButtonItem(NavigationItem.BackBarButtonItem, true);
        }

        async void Delete(DocumentToUploadContainer container)
        {
            await Delete(new List<DocumentToUploadContainer> { container });
        }

        async Task Delete(List<DocumentToUploadContainer> containers)
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

            CommonConfig.Logger.Info($"Refreshing outgoing documents list");

            try
            {
                var ds = (DataSource)tableView.Source;

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

            refreshing = false;
        }

        #endregion

        #region OutgoingDocumentManager event handlers

        void OutgoingDocumentsManager_DocumentBeingSent(object sender, DocumentToUploadContainer outgoingDocumentContainer)
        {
            BeginInvokeOnMainThread(() =>
            {
                var ds = (DataSource)tableView.Source;
                var row = ds.GetPosition(outgoingDocumentContainer.Info.Identifier);
                if (row >= 0)
                {
                    var container = ds.Items[row];
                    container.Info.State = OutgoingDocumentState.Sending;
                    ds.UpdateRow(row);
                }
            });
        }

        void OutgoingDocumentsManager_DocumentSendingFailed(object sender, DocumentToUploadContainer outgoingDocumentContainer)
        {
            BeginInvokeOnMainThread(() =>
            {
                var ds = (DataSource)tableView.Source;
                var row = ds.GetPosition(outgoingDocumentContainer.Info.Identifier);
                if (row >= 0)
                {
                    var container = ds.Items[row];
                    container.Info.State = OutgoingDocumentState.Failed;
                    ds.UpdateRow(row);
                }
            });
        }

        void OutgoingDocumentsManager_DocumentSendingSuccessful(object sender, DocumentToUploadContainer outgoingDocumentContainer)
        {
            BeginInvokeOnMainThread(() =>
            {
                var ds = (DataSource)tableView.Source;
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

            public List<DocumentToUploadContainer> Items { get; private set; } = new List<DocumentToUploadContainer>(1000);

            DocumentsToUploadListViewController viewController;
            UITableView documentsTableView;
            readonly string emptyText;

            bool loading = true;

            public DataSource(DocumentsToUploadListViewController viewController, UITableView documentsTableView, string emptyText)
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

                var deleteAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Default,
                    Localization.GetString("delete"),
                    (a, ip) =>
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

            public void ReplaceItems(List<DocumentToUploadContainer> containers)
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
                documentsTableView.ReloadRows(new[] { NSIndexPath.FromRowSection(row, 0) }, UITableViewRowAnimation.Fade);
            }

            public void RemoveRow(int row)
            {
                if (Items.Count < 1 && row == 0)
                    UpdateRow(0); //We always keep a row for the empty table cell
                else
                    documentsTableView.DeleteRows(new NSIndexPath[] { NSIndexPath.FromRowSection(row, 0) }, UITableViewRowAnimation.Fade);
            }
        }
    }
}