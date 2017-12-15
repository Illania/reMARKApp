using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.HubMessages;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using Mark5.Mobile.IOS.Utilities;
using TinyMessenger;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class DocumentsToUploadListViewController : AbstractTableViewController, IPrimaryViewController
    {
        TinyMessageSubscriptionToken documentUploadStatusChangedToken;

        public DocumentsToUploadListViewController()
            : base(UITableViewStyle.Grouped)
        {
        }

        #region UIViewController overrides

        public override void LoadView()
        {
            base.LoadView();

            InitializeNavigationBar();
            InitializeView();
            SubscribeToMessages();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (Integration.IsRunningAtLeast(11))
            {
                if (NavigationController != null)
                    NavigationController.NavigationBar.PrefersLargeTitles = true;
                NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Automatic;
            }

            if (TableView?.IndexPathForSelectedRow != null)
                TableView.DeselectRow(TableView.IndexPathForSelectedRow, true);

            if (TableView?.IndexPathsForSelectedRows?.Length > 0)
                foreach (var selectedIndexPath in TableView?.IndexPathsForSelectedRows)
                    TableView.DeselectRow(selectedIndexPath, true);
        }

        public override async void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info("Appeared");
            await RefreshData();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            documentUploadStatusChangedToken?.Dispose();
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning("Received memory warning!");

            ((DataSource)TableView.Source)?.Reset();

            UnsubscribeFromMessages();

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        protected override void Recycle()
        {
            base.Recycle();

            ((DataSource)TableView.Source)?.Reset();

            documentUploadStatusChangedToken?.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (CommonConfig.Logger.IsDebugEnabled())
                CommonConfig.Logger.Debug("Disposed");
        }

        #endregion

        #region Initialize/deinitialize

        void InitializeNavigationBar()
        {
            NavigationItem.Title = Localization.GetString("outgoing");
        }

        void InitializeView()
        {
            TableView.Source = new DataSource(this, TableView);
            TableView.AllowsMultipleSelectionDuringEditing = true;
            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 60f;
        }

        #endregion

        #region Subscribe/unsubscribe

        void SubscribeToMessages()
        {
            documentUploadStatusChangedToken = CommonConfig.MessengerHub.Subscribe<DocumentUploadStatusChangedMessage>(DocumentUploadStatusChanged);
        }

        void UnsubscribeFromMessages()
        {
            documentUploadStatusChangedToken?.Dispose();
        }

        #endregion

        #region Refreshing

        async Task RefreshData()
        {
            CommonConfig.Logger.Info($"Refreshing outgoing documents list");

            try
            {
                var pendingDocs = await Managers.DocumentsManager.GetDocumentsToUploadDocumentPreviews();
                var failedDocs = await Managers.DocumentsManager.GetFailedDocumentsToUploadDocumentPreviews();
                ((DataSource)TableView.Source).ReplaceItems(pendingDocs, failedDocs);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Could not refresh outgoing document list", ex);

                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        #endregion

        #region List handlers

        void FailedDocumentSelected(Guid guid)
        {
            var vc = new DocumentViewController();
            vc.SetData(guid);
            vc.SetRefreshDataOnAppear();
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        #endregion

        #region Actions

        async void ResendFailedDocumentToUpload((Guid Guid, DocumentPreview DocumentPreview) data)
        {
            await Managers.DocumentsManager.RequeueFailedToUpload(data.Guid);
            await RefreshData();
        }

        async void DeleteFailedDocumentToUpload((Guid Guid, DocumentPreview DocumentPreview) data)
        {
            await Managers.DocumentsManager.DeleteFailedDocumentToUpload(data.Guid);
            await RefreshData();
        }

        #endregion

        #region Message handlers

        void DocumentUploadStatusChanged(DocumentUploadStatusChangedMessage m)
        {
            BeginInvokeOnMainThread(async () =>
            {
                await RefreshData();
            });
        }

        #endregion

        #region DataSource

        class DataSource : UITableViewSource
        {
            public static class Section
            {
                public static readonly nint Pending = 0;
                public static readonly nint Failed = 1;
            }

            readonly WeakReference<DocumentsToUploadListViewController> viewControllerWeakReference;
            readonly WeakReference<UITableView> tableViewWeakReference;

            bool loading = true;
            readonly Dictionary<nint, List<(Guid Guid, DocumentPreview DocumentPreview)>> items = new Dictionary<nint, List<(Guid Guid, DocumentPreview DocumentPreview)>>
            {
                { Section.Pending, new List<(Guid, DocumentPreview)>(25) },
                { Section.Failed, new List<(Guid, DocumentPreview)>(25) }
            };

            public DataSource(DocumentsToUploadListViewController viewController, UITableView tableView)
            {
                viewControllerWeakReference = viewController.Wrap();
                tableViewWeakReference = tableView.Wrap();
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                    return tableView.DequeueReusableCell(WaitTableViewCell.DefaultId) as WaitTableViewCell ?? new WaitTableViewCell();

                if (items[indexPath.Section].Count < 1)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.DefaultId) as EmptyTableViewCell ?? new EmptyTableViewCell();
                    if (indexPath.Section == Section.Pending)
                        emptyCell.Initialize(Localization.GetString("no_documents_pending"));
                    if (indexPath.Section == Section.Failed)
                        emptyCell.Initialize(Localization.GetString("no_documents_failed"));
                    return emptyCell;
                }

                var dp = items[indexPath.Section][indexPath.Row];

                var cell = tableView.DequeueReusableCell(DocumentsTableViewCell.UploadId) as DocumentsTableViewCell ?? new DocumentsTableViewCell(DocumentsTableViewCell.UploadId);
                cell.Initialize(dp.DocumentPreview);

                cell.SelectionStyle = indexPath.Section == Section.Failed
                    ? UITableViewCellSelectionStyle.Default
                    : UITableViewCellSelectionStyle.None;

                return cell;
            }

            public override nint NumberOfSections(UITableView tableView) => 2;

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading || items[section].Count < 1)
                    return 1;

                return items[section].Count;
            }

            public override string TitleForHeader(UITableView tableView, nint section)
            {
                if (section == Section.Pending)
                    return Localization.GetString("pending");

                if (section == Section.Failed)
                    return Localization.GetString("failed");

                return null;
            }

            public override void WillDisplayHeaderView(UITableView tableView, UIView headerView, nint section) => headerView.ApplyTheme();

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                if (tableView.Editing)
                    return;

                var cell = tableView.CellAt(indexPath);
                if (cell?.SelectionStyle == UITableViewCellSelectionStyle.None)
                    return;

                viewControllerWeakReference.Unwrap()?.FailedDocumentSelected(items[Section.Failed][indexPath.Row].Guid);
            }

            public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading || items[indexPath.Section].Count < 1)
                    return false;

                return true;
            }

            public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
            {
                var actions = new UITableViewRowAction[2];
                actions[0] = UITableViewRowAction.Create(UITableViewRowActionStyle.Destructive,
                                                         Localization.GetString("delete"),
                                                         (a, nsip) =>
                {
                    viewControllerWeakReference.Unwrap()?.DeleteFailedDocumentToUpload(items[indexPath.Section][indexPath.Row]);
                });
                actions[0].BackgroundColor = Theme.Brown;
                actions[1] = UITableViewRowAction.Create(UITableViewRowActionStyle.Default,
                                                         Localization.GetString("resend"),
                                                         (a, nsip) =>
                {
                    viewControllerWeakReference.Unwrap()?.ResendFailedDocumentToUpload(items[indexPath.Section][indexPath.Row]);
                });
                actions[1].BackgroundColor = Theme.DarkBlue;
                return actions;
            }

            public void ReplaceItems(List<(Guid, DocumentPreview)> queue, List<(Guid, DocumentPreview)> failed)
            {
                loading = false;

                items[Section.Pending].Clear();
                items[Section.Pending].AddRange(queue);
                items[Section.Failed].Clear();
                items[Section.Failed].AddRange(failed);

                tableViewWeakReference.Unwrap()?.BeginUpdates();
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(Section.Pending), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(Section.Failed), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.EndUpdates();
            }

            public void Reset()
            {
                loading = true;

                items[Section.Pending].Clear();
                items[Section.Failed].Clear();

                tableViewWeakReference.Unwrap()?.BeginUpdates();
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(Section.Pending), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(Section.Failed), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.EndUpdates();
            }
        }

        #endregion

    }
}