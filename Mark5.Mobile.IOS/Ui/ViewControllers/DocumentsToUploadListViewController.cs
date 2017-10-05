using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.HubMessages;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
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

            if (NavigationController != null)
                NavigationController.NavigationBar.PrefersLargeTitles = true;
            NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Automatic;

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

        public override void Recycle()
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
        }

        #endregion

        #region Subscribe/unsubscribe

        void SubscribeToMessages()
        {
            documentUploadStatusChangedToken = CommonConfig.MessengerHub.Subscribe<DocumentUploadStatusChanged>(DocumentUploadStatusChanged);
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

                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        #endregion

        #region List handlers

        void FailedDocumentSelected(Guid guid)
        {
            var vc = new DocumentViewController
            {
                Modal = true
            };
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

        void DocumentUploadStatusChanged(DocumentUploadStatusChanged m)
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

            public bool Empty => !documentsToUploadInView.SelectMany(kv => kv.Value).Any();

            readonly WeakReference<DocumentsToUploadListViewController> viewControllerWeakReference;
            readonly WeakReference<UITableView> tableViewWeakReference;

            bool loading = true;

            readonly Dictionary<nint, List<(Guid Guid, DocumentPreview DocumentPreview)>> documentsToUploadInView = new Dictionary<nint, List<(Guid Guid, DocumentPreview DocumentPreview)>>
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

                if (documentsToUploadInView[indexPath.Section].Count < 1)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.DefaultId) as EmptyTableViewCell ?? new EmptyTableViewCell();
                    if (indexPath.Section == Section.Pending)
                        emptyCell.Initialize(Localization.GetString("no_documents_pending"));
                    if (indexPath.Section == Section.Failed)
                        emptyCell.Initialize(Localization.GetString("no_documents_failed"));
                    return emptyCell;
                }

                var dp = documentsToUploadInView[indexPath.Section][indexPath.Row];

                var cell = tableView.DequeueReusableCell(DocumentToUploadTableViewCell.Key) as DocumentToUploadTableViewCell ?? DocumentToUploadTableViewCell.Create();
                cell.Initialize(dp, indexPath.Section);
                return cell;
            }

            public override nint NumberOfSections(UITableView tableView) => 2;

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading)
                    return 1;

                if (documentsToUploadInView[section].Count < 1)
                    return 1;

                return documentsToUploadInView[section].Count;
            }

            public override string TitleForHeader(UITableView tableView, nint section)
            {
                if (section == Section.Pending)
                    return Localization.GetString("pending");

                if (section == Section.Failed)
                    return Localization.GetString("failed");

                return null;
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath) => DocumentToUploadTableViewCell.Height;

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                if (indexPath.Section == Section.Failed)
                    viewControllerWeakReference.Unwrap()?.FailedDocumentSelected(documentsToUploadInView[Section.Failed][indexPath.Row].Guid);
            }

            public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
            {
                return indexPath.Section == Section.Failed;
            }

            public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
            {
                if (indexPath.Section == Section.Pending)
                    return new UITableViewRowAction[0];

                var actions = new UITableViewRowAction[2];
                actions[0] = UITableViewRowAction.Create(UITableViewRowActionStyle.Destructive,
                                                         Localization.GetString("delete"),
                                                         (a, nsip) =>
                {
                    viewControllerWeakReference.Unwrap()?.DeleteFailedDocumentToUpload(documentsToUploadInView[indexPath.Section][indexPath.Row]);
                });
                actions[0].BackgroundColor = Theme.Brown;
                actions[1] = UITableViewRowAction.Create(UITableViewRowActionStyle.Default,
                                                         Localization.GetString("resend"),
                                                         (a, nsip) =>
                {
                    viewControllerWeakReference.Unwrap()?.ResendFailedDocumentToUpload(documentsToUploadInView[indexPath.Section][indexPath.Row]);
                });
                actions[1].BackgroundColor = Theme.DarkBlue;
                return actions;
            }

            public void ReplaceItems(List<(Guid, DocumentPreview)> queue, List<(Guid, DocumentPreview)> failed)
            {
                loading = false;

                documentsToUploadInView[Section.Pending].Clear();
                documentsToUploadInView[Section.Pending].AddRange(queue);
                documentsToUploadInView[Section.Failed].Clear();
                documentsToUploadInView[Section.Failed].AddRange(failed);

                tableViewWeakReference.Unwrap()?.BeginUpdates();
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(Section.Pending), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(Section.Failed), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.EndUpdates();
            }

            public void Reset()
            {
                loading = true;

                documentsToUploadInView[Section.Pending].Clear();
                documentsToUploadInView[Section.Failed].Clear();

                tableViewWeakReference.Unwrap()?.BeginUpdates();
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(Section.Pending), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(Section.Failed), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.EndUpdates();
            }
        }

        #endregion

    }
}