using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Model;
using reMark.Mobile.IOS.Ui.Common;
using reMark.Mobile.IOS.Ui.ViewControllers;
using reMark.Mobile.IOS.Ui.ViewControllers.DocumentView;
using reMark.Mobile.Common.Extensions;
using UIKit;
using Foundation;
using reMark.Mobile.IOS.Utilities;
using System.Collections.Generic;
using reMark.Mobile.Common.Utilities;

namespace reMark.Mobile.IOS.Ui.ViewControllers.SearchView
{
    public class DocumentsSearchResultsFilterController:UITableViewController
    {
        public DocumentsSearchResultsViewController DocumentSearchResultsController { get; set; }
        bool selectAllEnabled;
       
        public override void LoadView()
        {
            base.LoadView();
            InitializeView();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            DocumentSearchResultsController.DeinitializeEditModeHandlers();
            DocumentSearchResultsController.InitializeEditModeActions(EditItem_Clicked, ExitEditItem_Clicked);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            DocumentSearchResultsController.DeinitializeEditModeActions(EditItem_Clicked, ExitEditItem_Clicked);
            DocumentSearchResultsController.InitializeEditModeHandlers();
        }


        void InitializeView()
        {
            var searchResultsDataSource = new DocumentsSearchResultsDataSource(DocumentSearchResultsController, TableView, PlatformConfig.Preferences.CompactDocumentsList);
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
            if (Editing || ((DocumentsSearchResultsDataSource)TableView.Source).Empty)
                return;

            DocumentSearchResultsController.StartEditing(TableView);

            var point = recognizer.LocationInView(TableView);
            var indexPath = TableView.IndexPathForRowAtPoint(point);

            if (indexPath == null || (!TableView.CellAt(indexPath)?.UserInteractionEnabled ?? true))
                return;

            TableView.SelectRow(indexPath, true, UITableViewScrollPosition.None);
        }


        void EndEditing()
        {
            TableView.SetEditing(false, true);
            DocumentSearchResultsController.NavigationItem.SetLeftBarButtonItem(NavigationItem.BackBarButtonItem, true);
        }
      
        void ExitEditItem_Clicked(object sender, EventArgs e) => DocumentSearchResultsController.EndEditing(TableView);

        void EditItem_Clicked(object sender, EventArgs e)
        {
            if (TableView.IndexPathsForSelectedRows == null || TableView.IndexPathsForSelectedRows.Length < 1)
                return;

            var eas = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            var d = new PopoverPresentationControllerDelegate((UIBarButtonItem)sender);

            var rows = TableView.IndexPathsForSelectedRows.ToArray();
            var selectedDocuments = rows.Select(ip => ((DocumentsSearchResultsDataSource)TableView.Source).Items[ip.Row]).ToList();

            if (selectedDocuments.Any(dp => !dp.IsReadByCurrent))
                eas.AddAction(UIAlertAction.Create(Localization.GetString("mark_as_read"),
                    UIAlertActionStyle.Default,
                    a =>
                    {
                        DocumentSearchResultsController.MarkAsRead(selectedDocuments, rows);
                        EndEditing();
                    }));

            if (selectedDocuments.Any(dp => dp.IsReadByCurrent))
                eas.AddAction(UIAlertAction.Create(Localization.GetString("mark_as_unread"),
                    UIAlertActionStyle.Default,
                    a =>
                    {
                        DocumentSearchResultsController.MarkAsUnread(selectedDocuments, rows);
                        EndEditing();
                    }));

            if (ServerConfig.SystemSettings.DocumentsModuleInfo.WorktrayEnabled ?? true)
            {
                eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_worktray"),
                                                   UIAlertActionStyle.Default,
                                                   a =>
                                                   {

                                                       DocumentSearchResultsController.CopyToWorktray(
                                                          selectedDocuments.Select(be => (IBusinessEntity)be).ToList());
                                                       EndEditing();
                                                   }));
            }

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_folder"),
                UIAlertActionStyle.Default,
                a =>
                {
                    DocumentSearchResultsController.CopyToFolder(selectedDocuments.Select(be => (IBusinessEntity)be).ToList());
                    EndEditing();
                }));


            eas.AddAction(UIAlertAction.Create(Localization.GetString("set_priority"), UIAlertActionStyle.Default,
                a => DocumentSearchResultsController.ShowPriorityActionSheet(selectedDocuments, (UIBarButtonItem)sender, rows)));

   
            if (DocumentsDeleteChecker.CanDeleteDocuments(selectedDocuments))
            {
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete"),
                    UIAlertActionStyle.Destructive,
                    a => DocumentSearchResultsController.Delete(selectedDocuments, d)));
            }


            eas.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel,
                a => DocumentSearchResultsController.SetExitEditItemEnabled(true)));

            if (eas.PopoverPresentationController != null)
                eas.PopoverPresentationController.Delegate = d;

            PresentViewController(eas, true, null);
        }


        public async void SearchDocuments(string searchText, CancellationToken ct)
        {        
            var dataSource = TableView?.Source as DocumentsSearchResultsDataSource;
            dataSource?.Reset();

            await Task.Delay(500);

            if (ct.IsCancellationRequested)
                return;

            var ds = (DocumentsSearchResultsDataSource)DocumentSearchResultsController.TableView.Source;
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
