using System;
using Foundation;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Utilities.Extensions;
using reMark.Mobile.IOS.Ui.Common;
using reMark.Mobile.IOS.Ui.TableViewCells;
using System.Collections.Generic;
using UIKit;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Extensions;
using System.Linq;
using reMark.Mobile.IOS.Utilities;

namespace reMark.Mobile.IOS.Ui.ViewControllers.SearchView
{

    public class DocumentsSearchResultsDataSource : UITableViewSource
    {
        public bool Empty => Items.Count < 1;
        public List<DocumentPreview> Items { get; } = new List<DocumentPreview>(1000);

        readonly WeakReference<DocumentsSearchResultsViewController> viewControllerWeakReference;
        readonly WeakReference<UITableView> tableViewWeakReference;
        readonly bool compactList;

        bool loading = true;

        public DocumentsSearchResultsDataSource(DocumentsSearchResultsViewController viewController, UITableView tableView, bool compactList)
        {
            viewControllerWeakReference = viewController.Wrap();
            tableViewWeakReference = tableView.Wrap();
            this.compactList = compactList;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            if (loading)
                return tableView.DequeueReusableCell(WaitTableViewCell.DefaultId) as WaitTableViewCell ?? new WaitTableViewCell();

            if (Empty)
            {
                var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.DefaultId) as EmptyTableViewCell ?? new EmptyTableViewCell();
                emptyCell.Initialize(Localization.GetString("no_documents_found"));
                return emptyCell;
            }

            var dp = Items[indexPath.Row];
            if (dp.Direction == DocumentDirection.External)
            {
                var cell = tableView.DequeueReusableCell(DocumentsTableViewCell.ExternalId) as DocumentsTableViewCell ?? new DocumentsTableViewCell(DocumentsTableViewCell.ExternalId);
                cell.Initialize(dp);
                return cell;
            }

            if (compactList)
            {
                var cell = tableView.DequeueReusableCell(DocumentsTableViewCell.CompactId) as DocumentsTableViewCell ?? new DocumentsTableViewCell(DocumentsTableViewCell.CompactId);
                cell.Initialize(dp);
                return cell;
            }
            else
            {
                var cell = tableView.DequeueReusableCell(DocumentsTableViewCell.DefaultId) as DocumentsTableViewCell ?? new DocumentsTableViewCell(DocumentsTableViewCell.DefaultId);
                cell.Initialize(dp);
                return cell;
            }
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            if (loading || Empty)
                return 1;

            return Items.Count;
        }

        public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
        {
            if (loading || Empty)
                return false;

            return true;
        }

        public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
        {
            var actions = new List<UITableViewRowAction>();

            if (indexPath.Row < 0 || indexPath.Row >= Items.Count)
                return actions.ToArray();

            var documentPreview = Items[indexPath.Row];

            if (documentPreview.IsReadByCurrent)
            {
                var markAsUnreadAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Default,
                    Localization.GetString("mark_as_unread_ml"),
                    (a, ip) =>
                    {
                        viewControllerWeakReference.Unwrap()?.MarkAsUnread(documentPreview, indexPath);
                    });
                markAsUnreadAction.BackgroundColor = Theme.Brown;
                actions.Add(markAsUnreadAction);
            }
            else
            {
                var markAsReadAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Default,
                    Localization.GetString("mark_as_read_ml"),
                    (a, ip) =>
                    {
                        viewControllerWeakReference.Unwrap()?.MarkAsRead(documentPreview, indexPath);
                    });
                markAsReadAction.BackgroundColor = Theme.Brown;
                actions.Add(markAsReadAction);
            }

            if (ServerConfig.SystemSettings.DocumentsModuleInfo.WorktrayEnabled ?? true)
            {
                var copyToWorktrayAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Default,
                                                                       Localization.GetString("copy_to_worktray_ml"),
                                                                       (a, ip) =>
                                                                       {
                                                                           viewControllerWeakReference.Unwrap()?.CopyToWorktray(documentPreview);
                                                                       });
                copyToWorktrayAction.BackgroundColor = Theme.DarkBlue;
                actions.Add(copyToWorktrayAction);
            }

            var moreAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Default,
                                                         Localization.GetString("more"),
                                                         (a, ip) =>
                                                         {
                                                             viewControllerWeakReference.Unwrap()?.ShowMoreActionSheet(indexPath, documentPreview);
                                                         });
            moreAction.BackgroundColor = Theme.DarkerBlue;
            actions.Add(moreAction);

            return actions.ToArray();
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            if (tableView.Editing)
                return;

            var dp = Items[indexPath.Row];
            viewControllerWeakReference.Unwrap()?.DocumentSelected(dp);
        }

        public void AppendItems(List<DocumentPreview> documentPreviews)
        {
            loading = false;

            Items.AddRange(documentPreviews);
            tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
        }

        public void RemoveItems(List<int> documentIds)
        {
            var indices = Items.Select((d, i) => new { d, i })
            .Where(x => documentIds.Contains(x.d.Id))
                               .Select(x => x.i)
            .ToList();
            indices.OrderByDescending(i => i).ForEach(Items.RemoveAt);

            tableViewWeakReference.Unwrap()?.BeginUpdates();

            if (!Items.Any())
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Automatic);
            else
            {
                var indexPaths = indices.Select(i => NSIndexPath.FromRowSection(i, 0)).ToArray();
                tableViewWeakReference.Unwrap()?.DeleteRows(indexPaths, UITableViewRowAnimation.Automatic);
            }
            tableViewWeakReference.Unwrap()?.EndUpdates();
        }

        public void Reset()
        {
            loading = true;

            Items.Clear();
            tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
        }

        internal void UpdateItems(IEnumerable<int> documentPreviewIds)
        {
            if (documentPreviewIds == null || !documentPreviewIds.Any())
                return;

            tableViewWeakReference.Unwrap()?.BeginUpdates();
            documentPreviewIds.ForEach(UpdateItem);
            tableViewWeakReference.Unwrap()?.EndUpdates();
        }


        private void UpdateItem(int documentPreviewId)
        {
            var documentRow = Items?.IndexOf(d => d.Id == documentPreviewId);
            if (documentRow < 0)
                return;

            tableViewWeakReference.Unwrap()?.ReloadRows(new NSIndexPath[] { NSIndexPath.FromRowSection(documentRow.Value, 0) }, UITableViewRowAnimation.Fade);
        }
    }

}

