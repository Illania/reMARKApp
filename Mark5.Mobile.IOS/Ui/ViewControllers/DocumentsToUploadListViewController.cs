using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using UIKit;
using CoreGraphics;
using System.Linq;
using Mark5.Mobile.Common.Model.HubMessages;
using TinyMessenger;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class DocumentsToUploadListViewController : AbstractViewController, IPrimaryViewController
    {
        UITableView tableView;

        bool refreshing;

        TinyMessageSubscriptionToken documentUploadStatusChangedToken;

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

        public override async void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info($"{nameof(DocumentsToUploadListViewController)} appeared");
            await RefreshData();

            if (IsBeingDismissed)
                return;

            documentUploadStatusChangedToken = CommonConfig.MessengerHub.Subscribe<DocumentUploadStatusChanged>(m =>
            {
                BeginInvokeOnMainThread(async () =>
                {
                    await RefreshData();
                });
            });
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            documentUploadStatusChangedToken?.Dispose();
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

            tableView = new UITableView(CGRect.Empty, UITableViewStyle.Grouped)
            {
                ClipsToBounds = false,
                RowHeight = UITableView.AutomaticDimension,
                EstimatedRowHeight = 75f,
                AllowsSelectionDuringEditing = false,
                AllowsMultipleSelectionDuringEditing = true,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            tableView.Source = new DataSource(this, tableView, Localization.GetString("no_documents_pending"), Localization.GetString("no_documents_failed"));
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

        #region Refreshing

        async Task RefreshData()
        {
            if (refreshing)
                return;

            refreshing = true;

            CommonConfig.Logger.Info($"Refreshing outgoing documents list");

            try
            {
                var ds = (DataSource)tableView.Source;

                var pendingDocs = await Managers.DocumentsManager.GetDocumentsToUploadDocumentPreviews();
                var failedDocs = await Managers.DocumentsManager.GetFailedDocumentsToUploadDocumentPreviews();
                ds.ReplaceItems(pendingDocs, failedDocs);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Could not refresh outgoing document list", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);
            }

            refreshing = false;
        }

        #endregion

        class DataSource : UITableViewSource, IDisposable
        {
            public static class Section
            {
                public static readonly nint Pending = 0;
                public static readonly nint Failed = 1;
            }

            static readonly nfloat Height = 68f;

            public bool Empty => !Items.SelectMany(kv => kv.Value).Any();

            public Dictionary<nint, List<(Guid Guid, DocumentPreview DocumentPreview)>> Items { get; } = new Dictionary<nint, List<(Guid Guid, DocumentPreview DocumentPreview)>>
            {
                { Section.Pending, new List<(Guid, DocumentPreview)>(25) },
                { Section.Failed, new List<(Guid, DocumentPreview)>(25) }
            };

            DocumentsToUploadListViewController viewController;
            UITableView tableView;
            readonly string pendingEmptyText;
            readonly string failedEmptyText;

            bool loading = true;

            public DataSource(DocumentsToUploadListViewController viewController, UITableView tableView, string pendingEmptyText, string failedEmptyText)
            {
                this.viewController = viewController;
                this.tableView = tableView;
                this.pendingEmptyText = pendingEmptyText;
                this.failedEmptyText = failedEmptyText;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as WaitTableViewCell ?? WaitTableViewCell.Create();

                if (Items[indexPath.Section].Count < 1)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                    if (indexPath.Section == Section.Pending)
                        emptyCell.Initialize(pendingEmptyText);
                    if (indexPath.Section == Section.Failed)
                        emptyCell.Initialize(failedEmptyText);
                    return emptyCell;
                }

                var dp = Items[indexPath.Section][indexPath.Row];

                var cell = tableView.DequeueReusableCell(DocumentToUploadTableViewCell.Key) as DocumentToUploadTableViewCell ?? DocumentToUploadTableViewCell.Create();
                cell.Initialize(dp, indexPath.Section);
                return cell;
            }

            public override nint NumberOfSections(UITableView tableView) => 2;

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading)
                    return 1;

                if (Items[section].Count < 1)
                    return 1;

                return Items[section].Count;
            }

            public override string TitleForHeader(UITableView tableView, nint section)
            {
                if (section == Section.Pending)
                    return Localization.GetString("pending");

                if (section == Section.Failed)
                    return Localization.GetString("failed");

                return null;
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath) => Height;

            public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
            {
                return indexPath.Section == Section.Failed;
            }

            public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
            {
                if (indexPath.Section == Section.Pending)
                    return new UITableViewRowAction[0];

                var actions = new UITableViewRowAction[2];
                actions[0] = UITableViewRowAction.Create(UITableViewRowActionStyle.Destructive, "Delete", (a, nsip) => { viewController.DeleteFailedDocumentToUpload(Items[indexPath.Section][indexPath.Row]); });
                actions[0].BackgroundColor = Theme.Brown;
                actions[1] = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, "Resend", (a, nsip) => { viewController.ResendFailedDocumentToUpload(Items[indexPath.Section][indexPath.Row]); });
                actions[1].BackgroundColor = Theme.DarkBlue;
                return actions;
            }

            public void ReplaceItems(List<(Guid, DocumentPreview)> queue, List<(Guid, DocumentPreview)> failed)
            {
                loading = false;

                Items[Section.Pending].Clear();
                Items[Section.Pending].AddRange(queue);
                Items[Section.Failed].Clear();
                Items[Section.Failed].AddRange(failed);
                tableView.BeginUpdates();
                tableView.ReloadSections(NSIndexSet.FromIndex(Section.Pending), UITableViewRowAnimation.Fade);
                tableView.ReloadSections(NSIndexSet.FromIndex(Section.Failed), UITableViewRowAnimation.Fade);
                tableView.EndUpdates();
            }

            public void Reset()
            {
                loading = true;

                Items[Section.Pending].Clear();
                Items[Section.Failed].Clear();
                tableView.BeginUpdates();
                tableView.ReloadSections(NSIndexSet.FromIndex(Section.Pending), UITableViewRowAnimation.Fade);
                tableView.ReloadSections(NSIndexSet.FromIndex(Section.Failed), UITableViewRowAnimation.Fade);
                tableView.EndUpdates();
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                viewController = null;
                tableView = null;
                Items[Section.Pending].Clear();
                Items[Section.Failed].Clear();
            }
        }
    }
}