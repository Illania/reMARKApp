using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers;
using Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView;
using Mark5.Mobile.Common.Extensions;

using UIKit;
using Foundation;
using Mark5.Mobile.IOS.Utilities;
using System.Collections.Generic;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.IOS.Ui
{
    public class SearchResultsController: UITableViewController
    {
        public bool DisableRowActions { get; set; }
        public DocumentsListViewController DocumentListViewController { get; set; }
        public Folder Folder { get; set; }
        bool selectAllEnabled;
       
        public override void LoadView()
        {
            base.LoadView();
            InitializeView();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            DocumentListViewController.DeinitializeEditModeHandlers();
            DocumentListViewController.InitializeEditModeActions(EditItem_Clicked, ExitEditItem_Clicked, SelectAllItem_Clicked);
            DocumentListViewController.HideBookmarkButton();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            DocumentListViewController.DeinitializeEditModeActions(EditItem_Clicked, ExitEditItem_Clicked, SelectAllItem_Clicked);
            DocumentListViewController.InitializeEditModeHandlers();
            DocumentListViewController.ShowBookmarkButton();
        }


        void InitializeView()
        {
            var searchResultsDataSource = new DocumentListDataSource(DocumentListViewController, TableView, Localization.GetString("no_matching_documents"),
                PlatformConfig.Preferences.CompactDocumentsList, DisableRowActions);
            TableView.Source = searchResultsDataSource;
            TableView.EstimatedRowHeight = 60f;
            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.AllowsSelection = true;
            TableView.UserInteractionEnabled = true;      
            TableView.AllowsSelectionDuringEditing = true;
            TableView.AllowsMultipleSelectionDuringEditing = true;
            TableView.AddGestureRecognizer(new UILongPressGestureRecognizer(SearchControllerDocumentPreviewLongPressed));
        }

        void SearchControllerDocumentPreviewLongPressed(UILongPressGestureRecognizer recognizer)
        {
            if (Editing || ((DocumentListDataSource)TableView.Source).Empty || DisableRowActions)
                return;

            DocumentListViewController.StartEditing(TableView);

            var point = recognizer.LocationInView(TableView);
            var indexPath = TableView.IndexPathForRowAtPoint(point);

            if (indexPath == null || (!TableView.CellAt(indexPath)?.UserInteractionEnabled ?? true))
                return;

            TableView.SelectRow(indexPath, true, UITableViewScrollPosition.None);
        }


        void EndEditing()
        {
            TableView.SetEditing(false, true);
            DocumentListViewController.NavigationItem.SetLeftBarButtonItem(NavigationItem.BackBarButtonItem, true);
        }
      
        void ExitEditItem_Clicked(object sender, EventArgs e) => DocumentListViewController.EndEditing(TableView);

        void EditItem_Clicked(object sender, EventArgs e)
        {
            if (TableView.IndexPathsForSelectedRows == null || TableView.IndexPathsForSelectedRows.Length < 1)
                return;

            var eas = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            var d = new PopoverPresentationControllerDelegate((UIBarButtonItem)sender);

            var rows = TableView.IndexPathsForSelectedRows.ToArray();
            var selectedDocuments = rows.Select(ip => ((DocumentListDataSource)TableView.Source).Items[ip.Row]).ToList();

            if (selectedDocuments.Any(dp => !dp.IsReadByCurrent))
                eas.AddAction(UIAlertAction.Create(Localization.GetString("mark_as_read"),
                    UIAlertActionStyle.Default,
                    a =>
                    {
                        DocumentListViewController.MarkAsRead(selectedDocuments);
                        EndEditing();
                    }));

            if (selectedDocuments.Any(dp => dp.IsReadByCurrent))
                eas.AddAction(UIAlertAction.Create(Localization.GetString("mark_as_unread"),
                    UIAlertActionStyle.Default,
                    a =>
                    {
                        DocumentListViewController.MarkAsUnread(selectedDocuments);
                        EndEditing();
                    }));

            if (ServerConfig.SystemSettings.DocumentsModuleInfo.WorktrayEnabled ?? true)
            {
                eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_worktray"),
                                                   UIAlertActionStyle.Default,
                                                   a =>
                                                   {

                                                       DocumentListViewController.CopyToWorktray(
                                                          selectedDocuments.Select(be => (IBusinessEntity)be).ToList());
                                                       EndEditing();
                                                   }));
            }

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_folder"),
                UIAlertActionStyle.Default,
                a =>
                {
                    DocumentListViewController.CopyToFolder(selectedDocuments.Select(be => (IBusinessEntity)be).ToList());
                    EndEditing();
                }));

            if (Folder.InternalType == FolderInternalType.FilterView || Folder.InternalType == FolderInternalType.Static || Folder.InternalType == FolderInternalType.Worktray)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("move_to_folder"),
                    UIAlertActionStyle.Default,
                    a =>
                    {
                        DocumentListViewController.MoveToFolder(selectedDocuments.Select(be => (IBusinessEntity)be).ToList(), DocumentListViewController.Folder);
                        EndEditing();
                    }));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("set_priority"), UIAlertActionStyle.Default, a => DocumentListViewController.ShowPriorityActionSheet(selectedDocuments, (UIBarButtonItem)sender)));

            if (Folder.InternalType == FolderInternalType.FilterView || Folder.InternalType == FolderInternalType.Static || Folder.InternalType == FolderInternalType.Worktray)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete_from_folder"),
                    UIAlertActionStyle.Default,
                    a => DocumentListViewController.RemoveFromFolder(selectedDocuments, d)));

            if (DocumentsDeleteChecker.CanDeleteDocuments(selectedDocuments))
            {
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete"),
                    UIAlertActionStyle.Destructive,
                    a => DocumentListViewController.Delete(selectedDocuments, d)));
            }

            if (ServerConfig.SystemSettings?.SystemInfo?.DelaySendAvailable == true && selectedDocuments.Any(dp => dp.TransmitStatus == TransmitStatus.Delayed))
            {
                eas.AddAction(UIAlertAction.Create(Localization.GetString("send_now"),
                   UIAlertActionStyle.Default,
                   a =>
                   {
                       DocumentListViewController.ForceSend(selectedDocuments);
                       EndEditing();
                   }));

                eas.AddAction(UIAlertAction.Create(Localization.GetString("cancel_send"),
                  UIAlertActionStyle.Default,
                  a =>
                  {
                      DocumentListViewController.CancelSend(selectedDocuments);
                      EndEditing();
                  }));
            }


            eas.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, a => DocumentListViewController.SetExitEditItemEnabled(true)));

            if (eas.PopoverPresentationController != null)
                eas.PopoverPresentationController.Delegate = d;

            PresentViewController(eas, true, null);
        }

        void SelectAllItem_Clicked(object sender, EventArgs e)
        {
            if (selectAllEnabled)
            {
                DocumentListViewController.SelectAll(TableView);
 
            }
            else
            {
                DocumentListViewController.DeselectAll(TableView);
            }

            selectAllEnabled = !selectAllEnabled;
        }

        public async void SearchDocuments(string searchText, CancellationToken ct)
        {        
            var dataSource = TableView?.Source as DocumentListDataSource;
            dataSource?.Reset();

            await Task.Delay(500);

            if (ct.IsCancellationRequested)
                return;

            var ds = (DocumentListDataSource)DocumentListViewController.TableView.Source;
            var filteredDocuments = ds.Items.Where(dp => MatchesQuery(dp, searchText)).ToList();

            if (ct.IsCancellationRequested)
                return;

            dataSource?.AppendItems(filteredDocuments);
        }

        static bool MatchesQuery(DocumentPreview dp, string query)
        {
#if DEBUG
            if (dp.Id.ToString() == query)
                return true;
#endif

            if (dp.ReferenceNumber?.ContainsCaseInsensitive(query) ?? false)
                return true;

            if (dp.Subject?.ContainsCaseInsensitive(query) ?? false)
                return true;

            if (dp.Preview?.ContainsCaseInsensitive(query) ?? false)
                return true;

            if (dp.Addresses.Any(da => da.Name?.ContainsCaseInsensitive(query) ?? false))
                return true;

            if (dp.Addresses.Any(da => da.Address?.ContainsCaseInsensitive(query) ?? false))
                return true;

            if (dp.Categories.Any(da => da.Name?.ContainsCaseInsensitive(query) ?? false))
                return true;

            if (dp.Creator?.ContainsCaseInsensitive(query) ?? false)
                return true;

            return false;
        }

    }
}
