using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.Common.Utilities.Extensions;
using reMark.Mobile.IOS.Ui.Common;
using reMark.Mobile.IOS.Ui.TableViewCells;
using reMark.Mobile.IOS.Ui.ViewControllers;
using reMark.Mobile.IOS.Utilities;
using UIKit;
using reMark.Mobile.Common.Extensions;

namespace reMark.Mobile.IOS.Ui
{
    #region DataSource

    public class DocumentListDataSource : UITableViewSource
    {
        public bool Empty => Items.Count < 1;
        public List<DocumentPreview> Items { get; } = new List<DocumentPreview>(1000);
        public bool LoadMoreEnabled { get; set; }

        public readonly WeakReference<DocumentsListViewController> viewControllerWeakReference;
        public readonly WeakReference<UITableView> tableViewWeakReference;
        readonly string emptyText;
        readonly bool compactList;
        readonly bool disableRowActions;

        bool loading = true;

        public DocumentListDataSource(DocumentsListViewController viewController, UITableView tableView, string emptyText, bool compactList,
            bool? disableRowActions = null)
        {
            viewControllerWeakReference = viewController.Wrap();
            tableViewWeakReference = tableView.Wrap();
            this.emptyText = emptyText;
            this.compactList = compactList;
            this.disableRowActions = disableRowActions.Value;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            if (loading)
                return tableView.DequeueReusableCell(WaitTableViewCell.DefaultId) as WaitTableViewCell ?? new WaitTableViewCell();

            if (Empty)
            {
                var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.DefaultId) as EmptyTableViewCell ?? new EmptyTableViewCell();
                emptyCell.Initialize(emptyText);
                return emptyCell;
            }

            var dp = Items[indexPath.Row];

            var folderId = viewControllerWeakReference.Unwrap()?.Folder.Id ?? -1;

            if (LoadMoreEnabled && dp.Id == Items.Last().Id)
            {
                CommonConfig.UsageAnalytics.LogEvent(new GetMoreDocumentsEvent());
                AsyncHelpers.FireAndForget(viewControllerWeakReference.Unwrap()?.RefreshData(dp.Id));
            }

            if (dp.Direction == DocumentDirection.External)
            {
                var cell = tableView.DequeueReusableCell(DocumentsTableViewCell.ExternalId) as DocumentsTableViewCell ?? new DocumentsTableViewCell(DocumentsTableViewCell.ExternalId);
                cell.Initialize(dp);
                InitializeBookmarkAppearance(dp, folderId, cell);
                return cell;
            }

            if (compactList)
            {
                var cell = tableView.DequeueReusableCell(DocumentsTableViewCell.CompactId) as DocumentsTableViewCell ?? new DocumentsTableViewCell(DocumentsTableViewCell.CompactId);
                cell.Initialize(dp);
                InitializeBookmarkAppearance(dp, folderId, cell);
                return cell;
            }
            else
            {
                var cell = tableView.DequeueReusableCell(DocumentsTableViewCell.DefaultId) as DocumentsTableViewCell ?? new DocumentsTableViewCell(DocumentsTableViewCell.DefaultId);
                cell.Initialize(dp);
                InitializeBookmarkAppearance(dp, folderId, cell);
                return cell;
            }
        }

        void InitializeBookmarkAppearance(DocumentPreview dp, int folderId, DocumentsTableViewCell cell)
        {
            if (PlatformConfig.Preferences.HasBookmarkForFolder(folderId, dp.Id))
                cell.BackgroundColor = Theme.Bookmark;
            else
                cell.BackgroundColor = Theme.White;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return loading || Empty ? 1 : (nint)Items.Count;
        }

        public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
        {
            if (loading || Empty || disableRowActions)
                return false;

            return true;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            if (tableView.Editing)
                return;

            var dp = Items[indexPath.Row];
            viewControllerWeakReference.Unwrap()?.DocumentSelected(dp);
        }

        public void PrependItems(IEnumerable<DocumentPreview> documentPreviews)
        {
            loading = false;

            if (Empty)
            {
                Items.InsertRange(0, documentPreviews);

                tableViewWeakReference.Unwrap()?.BeginUpdates();

                tableViewWeakReference.Unwrap()?.ReloadRows(new[] { NSIndexPath.FromRowSection(0, 0) }, UITableViewRowAnimation.Automatic);

                if (documentPreviews.Count() > 1)
                {
                    var indexes = Enumerable.Range(1, documentPreviews.Count() - 1).Select(i => NSIndexPath.FromRowSection(i, 0)).ToArray();
                    tableViewWeakReference.Unwrap()?.InsertRows(indexes, UITableViewRowAnimation.Fade);
                }

                tableViewWeakReference.Unwrap()?.EndUpdates();
            }
            else
            {
                if (PlatformConfig.Preferences.SortByDate)
                {
                    Items.Sort();
                    foreach (var dp in documentPreviews)
                        if(!Items.Exists(k=> k.Id == dp.Id))
                            Items.AddSorted(dp);
                }
                else
                    Items.InsertRange(0, documentPreviews);

                var indexes = Enumerable.Range(0, documentPreviews.Count()).Select(i => NSIndexPath.FromRowSection(i, 0)).ToArray();
                tableViewWeakReference.Unwrap()?.InsertRows(indexes, UITableViewRowAnimation.Fade);
            }

        }

        public void AppendItems(IEnumerable<DocumentPreview> documentPreviews)
        {
            loading = false;

            Items.AddRange(documentPreviews);
            tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
        }

        public void InsertItems(IEnumerable<DocumentPreview> documentPreviews)
        {
            loading = false;
            Items.Sort();
            foreach (var documentPreview in documentPreviews)
                if (!Items.Exists(k => k.Id == documentPreview.Id))
                    Items.AddSorted(documentPreview);
            tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
        }

        public void RemoveItems(IEnumerable<int> documentIds)
        {
            var indices = Items.Select((d, i) => new { d, i })
                               .Where(x => documentIds.Contains(x.d.Id))
                               .Select(x => x.i)
                               .OrderByDescending(i => i)
                               .ToArray();
            indices.ForEach(Items.RemoveAt);

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

        public void UpdateItems(IEnumerable<int> documentPreviewIds)
        {
            if (documentPreviewIds == null || !documentPreviewIds.Any())
                return;

            tableViewWeakReference.Unwrap()?.BeginUpdates();
            documentPreviewIds.ForEach(UpdateItem);
            tableViewWeakReference.Unwrap()?.EndUpdates();
        }

        public void UpdateItem(int documentPreviewId)
        {
            var documentRow = Items?.IndexOf(d => d.Id == documentPreviewId);
            if (documentRow < 0)
                return;

            tableViewWeakReference.Unwrap()?.ReloadRows(new NSIndexPath[] { NSIndexPath.FromRowSection(documentRow.Value, 0) }, UITableViewRowAnimation.Fade);
        }

        public void Reset()
        {
            loading = true;

            Items.Clear();
            tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
        }

        public void ScrollTo(int row)
        {
            var selectedIndexPaths = tableViewWeakReference.Unwrap()?.IndexPathsForSelectedRows;
            if (selectedIndexPaths != null)
                foreach (var indexPath in selectedIndexPaths)
                    tableViewWeakReference.Unwrap()?.DeselectRow(indexPath, true);

            tableViewWeakReference.Unwrap()?.SelectRow(NSIndexPath.FromRowSection(row, 0), true, UITableViewScrollPosition.Middle);
        }

        #region SwipeRelated

        class SwipeActionUIWrapper
        {
            public UITableViewRowAction Action { get; set; }
            public bool Disabled;
        }

        UIContextualAction BuildLeadingContextualAction(UITableView tableView, NSIndexPath indexPath)
        {
            if (indexPath == null)
                return null;

            var documentPreview = Items[indexPath.Row];

            EmailSwipeAction leadingAction = PlatformConfig.Preferences.EmailLeadingSwipeActions.First();
            if (!CheckActionEnabled(leadingAction))
                return null;

            var folder = viewControllerWeakReference.Unwrap()?.Folder;

            string title = viewControllerWeakReference.Unwrap()?.SwipeActionTitle(leadingAction.Action, documentPreview);

            var contextualAction = UIContextualAction.FromContextualActionStyle(UIContextualActionStyle.Normal, title, (someAction, view, success) =>
            {
                viewControllerWeakReference.Unwrap()?.OnSwipeActionClick(leadingAction, indexPath, documentPreview, folder, tableView);
            });

            contextualAction.BackgroundColor = DocumentsListViewController.SwipeActionAllowed(leadingAction.Action, documentPreview, folder) ? Theme.LightBrown : Theme.LightGray;

            return contextualAction;
        }

        private bool CheckActionEnabled(EmailSwipeAction emailSwipeAction)
            => emailSwipeAction.Action != EmailSwipeAction.SwipeAction.MoveToFolder || PlatformConfig.Preferences.EnableMoveToFolder;

        public override UISwipeActionsConfiguration GetLeadingSwipeActionsConfiguration(UITableView tableView, NSIndexPath indexPath)
        {
            var leadingSwipe = UISwipeActionsConfiguration.FromActions(new UIContextualAction[] { BuildLeadingContextualAction(tableView, indexPath) });

            leadingSwipe.PerformsFirstActionWithFullSwipe = true;

            var actions = leadingSwipe.Actions[0];
            if (!leadingSwipe.Actions.Any(a => a != null))
                return null;

            return leadingSwipe;
        }

        public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
        {
            if (indexPath == null)
            {
                CommonConfig.Logger.Warning($"IndexPath in DocumentListViewController.EditActionsForRow() was null.");
                return null;
            }

            var actionWrappers = new List<SwipeActionUIWrapper>();

            if (indexPath.Row < 0 || indexPath.Row >= Items.Count)
                return actionWrappers.Select(a => a.Action).ToArray();

            var documentPreview = Items[indexPath.Row];

            if (documentPreview == null)
            {
                CommonConfig.Logger.Warning($"DocumentPreview in DocumentListViewController.EditActionsForRow() was null");
                return null;
            }

            var trailingSwipeActions = FilterActions(PlatformConfig.Preferences.EmailTrailingSwipeActions);
            trailingSwipeActions.Reverse();
            var folder = viewControllerWeakReference.Unwrap()?.Folder;

            if (folder == null)
            {
                CommonConfig.Logger.Warning($"Folder in DocumentListViewController.EditActionsForRow() was null");
                return null;
            }

            foreach (EmailSwipeAction swipeAction in trailingSwipeActions)
            {
                SwipeActionUIWrapper actionWrapper = new SwipeActionUIWrapper();

                switch (swipeAction.Action)
                {
                    case EmailSwipeAction.SwipeAction.MarkAsRead:
                        if (documentPreview.IsReadByCurrent)
                        {
                            actionWrapper.Action = UITableViewRowAction.Create(
                                UITableViewRowActionStyle.Default,
                                viewControllerWeakReference.Unwrap()?.SwipeActionTitle(swipeAction.Action, documentPreview),
                                (a, ip) =>
                                {
                                    viewControllerWeakReference.Unwrap()?.OnSwipeActionClick(swipeAction, indexPath, documentPreview, folder, tableView);
                                });
                        }
                        else
                        {
                            actionWrapper.Action = UITableViewRowAction.Create(
                                UITableViewRowActionStyle.Default,
                                viewControllerWeakReference.Unwrap()?.SwipeActionTitle(swipeAction.Action, documentPreview),
                                (a, ip) =>
                                {
                                    viewControllerWeakReference.Unwrap()?.OnSwipeActionClick(swipeAction, indexPath, documentPreview, folder, tableView);
                                });
                        }
                        break;
                    case EmailSwipeAction.SwipeAction.CopyToWorkTray:
                        actionWrapper.Action = UITableViewRowAction.Create(
                            UITableViewRowActionStyle.Default,
                            viewControllerWeakReference.Unwrap()?.SwipeActionTitle(swipeAction.Action, documentPreview),
                            (a, ip) =>
                            {
                                viewControllerWeakReference.Unwrap()?.OnSwipeActionClick(swipeAction, indexPath, documentPreview, folder, tableView);
                            });
                        actionWrapper.Disabled = !DocumentsListViewController.SwipeActionAllowed(swipeAction.Action, documentPreview, folder);
                        break;
                    case EmailSwipeAction.SwipeAction.AddBookmark:
                        actionWrapper.Action = UITableViewRowAction.Create(
                            UITableViewRowActionStyle.Default,
                            viewControllerWeakReference.Unwrap()?.SwipeActionTitle(swipeAction.Action, documentPreview),
                            (a, ip) =>
                            {
                                viewControllerWeakReference.Unwrap()?.OnSwipeActionClick(swipeAction, indexPath, documentPreview, folder, tableView);
                            });
                        actionWrapper.Disabled = !DocumentsListViewController.SwipeActionAllowed(swipeAction.Action, documentPreview, folder);
                        break;
                    case EmailSwipeAction.SwipeAction.SetPresetCategory:
                        actionWrapper.Action = UITableViewRowAction.Create(
                            UITableViewRowActionStyle.Default,
                            viewControllerWeakReference.Unwrap()?.SwipeActionTitle(swipeAction.Action, documentPreview),
                            (a, ip) =>
                            {
                                viewControllerWeakReference.Unwrap()?.OnSwipeActionClick(swipeAction, indexPath, documentPreview, folder, tableView);
                            });
                        actionWrapper.Disabled = !DocumentsListViewController.SwipeActionAllowed(swipeAction.Action, documentPreview, folder);
                        break;
                    case EmailSwipeAction.SwipeAction.More:
                        actionWrapper.Action = UITableViewRowAction.Create(
                            UITableViewRowActionStyle.Default,
                            viewControllerWeakReference.Unwrap()?.SwipeActionTitle(swipeAction.Action, documentPreview),
                            (a, ip) =>
                            {
                                viewControllerWeakReference.Unwrap()?.OnSwipeActionClick(swipeAction, indexPath, documentPreview, folder, tableView);
                            });
                        break;
                    case EmailSwipeAction.SwipeAction.CopyToFolder:
                        actionWrapper.Action = UITableViewRowAction.Create(
                            UITableViewRowActionStyle.Default,
                            viewControllerWeakReference.Unwrap()?.SwipeActionTitle(swipeAction.Action, documentPreview),
                            (a, ip) =>
                            {
                                viewControllerWeakReference.Unwrap()?.OnSwipeActionClick(swipeAction, indexPath, documentPreview, folder, tableView);
                            });
                        break;
                    case EmailSwipeAction.SwipeAction.Categories:
                        actionWrapper.Action = UITableViewRowAction.Create(
                            UITableViewRowActionStyle.Default,
                            viewControllerWeakReference.Unwrap()?.SwipeActionTitle(swipeAction.Action, documentPreview),
                            (a, ip) =>
                            {
                                viewControllerWeakReference.Unwrap()?.OnSwipeActionClick(swipeAction, indexPath, documentPreview, folder, tableView);
                            });
                        break;
                    case EmailSwipeAction.SwipeAction.SetPriority:
                        actionWrapper.Action = UITableViewRowAction.Create(
                            UITableViewRowActionStyle.Default,
                            viewControllerWeakReference.Unwrap()?.SwipeActionTitle(swipeAction.Action, documentPreview),
                            (a, ip) =>
                            {
                                viewControllerWeakReference.Unwrap()?.OnSwipeActionClick(swipeAction, indexPath, documentPreview, folder, tableView);
                            });
                        break;
                    case EmailSwipeAction.SwipeAction.Delete:
                        actionWrapper.Action = UITableViewRowAction.Create(
                            UITableViewRowActionStyle.Default,
                            viewControllerWeakReference.Unwrap()?.SwipeActionTitle(swipeAction.Action, documentPreview),
                            (a, ip) =>
                            {
                                viewControllerWeakReference.Unwrap()?.OnSwipeActionClick(swipeAction, indexPath, documentPreview, folder, tableView);
                            });

                        actionWrapper.Disabled = !DocumentsListViewController.SwipeActionAllowed(swipeAction.Action, documentPreview, folder);
                        break;
                    case EmailSwipeAction.SwipeAction.MoveToFolder:
                        actionWrapper.Action = UITableViewRowAction.Create(
                            UITableViewRowActionStyle.Default,
                            viewControllerWeakReference.Unwrap()?.SwipeActionTitle(swipeAction.Action, documentPreview),
                            (a, ip) => {
                                viewControllerWeakReference.Unwrap()?.OnSwipeActionClick(swipeAction, indexPath, documentPreview, folder, tableView);
                            });

                        actionWrapper.Disabled = !DocumentsListViewController.SwipeActionAllowed(swipeAction.Action, documentPreview, folder);
                        break;
                    case EmailSwipeAction.SwipeAction.RemoveFromFolder:
                        actionWrapper.Action = UITableViewRowAction.Create(
                            UITableViewRowActionStyle.Default,
                            viewControllerWeakReference.Unwrap()?.SwipeActionTitle(swipeAction.Action, documentPreview),
                            (a, ip) =>
                            {
                                viewControllerWeakReference.Unwrap()?.OnSwipeActionClick(swipeAction, indexPath, documentPreview, folder, tableView);
                            });
                        actionWrapper.Disabled = !DocumentsListViewController.SwipeActionAllowed(swipeAction.Action, documentPreview, folder);
                        break;
                    case EmailSwipeAction.SwipeAction.DeliveryReport:
                        actionWrapper.Action = UITableViewRowAction.Create(
                            UITableViewRowActionStyle.Default,
                            viewControllerWeakReference.Unwrap()?.SwipeActionTitle(swipeAction.Action, documentPreview),
                            (a, ip) => {
                                viewControllerWeakReference.Unwrap()?.OnSwipeActionClick(swipeAction, indexPath, documentPreview, folder, tableView);
                            });
                        actionWrapper.Disabled = !DocumentsListViewController.SwipeActionAllowed(swipeAction.Action, documentPreview, folder);
                        break;
           
                    default:
                        break;
                }

                actionWrappers.Add(actionWrapper);
            }

            for (int i = 0; i < actionWrappers.Count; i++)
            {
                if (actionWrappers[i].Disabled)
                {
                    actionWrappers[i].Action.BackgroundColor = Theme.LightGray;
                }
                else
                {
                    if (i == 0)
                    {
                        actionWrappers[i].Action.BackgroundColor = Theme.Brown;
                    }
                    else if (i == 1)
                    {
                        actionWrappers[i].Action.BackgroundColor = Theme.DarkBlue;
                    }
                    else
                    {
                        actionWrappers[i].Action.BackgroundColor = Theme.DarkerBlue;
                    }
                }
            }

            UITableViewRowAction[] returnActions = actionWrappers.Select(a => a.Action).ToArray();

            return returnActions;
        }

        private List<EmailSwipeAction> FilterActions(List<EmailSwipeAction> emailSwipeActions)
        {
            var actionsToRemove = new List<EmailSwipeAction>();
            foreach (var action in emailSwipeActions)
            {
                if (action.Action == EmailSwipeAction.SwipeAction.MoveToFolder && !PlatformConfig.Preferences.EnableMoveToFolder)
                    actionsToRemove.Add(action);
            }
            return emailSwipeActions.Except(actionsToRemove.ToList()).ToList();
        }
        #endregion
    }

    #endregion

}
