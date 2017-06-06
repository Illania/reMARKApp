//
// Project: Mark5.Mobile.IOS
// File: DocumentsListViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Model.HubMessages;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentView;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;
using Mark5.Mobile.IOS.Utilities;
using ObjCRuntime;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class DocumentsListViewController : AbstractViewController, IPrimaryViewController, IUISearchResultsUpdating, IUIGestureRecognizerDelegate
    {
        const int AutoRefreshIntervalMs = 5 * 1000; // 5 seconds

        public Folder Folder { get; set; }

        UIBarButtonItem composeDocumentItem;
        UIBarButtonItem exitEditItem;
        UIBarButtonItem editItem;

        UIRefreshControl refreshControl;
        UITableView tableView;
        UISearchController searchController;
        UITableViewController searchResultsController;
        DataSource searchResultsDataSource;

        CancellationTokenSource searchCancellationTokenSource;
        readonly List<CancellationTokenSource> searchCancellationTokenSourceList = new List<CancellationTokenSource>();

        AutoRefreshWorker autoRefreshWorker;
        Action newDocumentsAvailableAction;

        bool refreshing;

        #region UIViewController overrides

        public override void LoadView()
        {
            base.LoadView();

            InitializeNavigationBar();
            InitializeView();
            InitializeSearchBar();
            SubscribeToMessages();
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

            CommonConfig.Logger.Info($"{nameof(DocumentsListViewController)} appeared");

            var ds = (DataSource) tableView.Source;
            if (ds.Empty)
                await RefreshData();

            if (IsBeingDismissed)
                return;

            CommonConfig.Logger.Info($"Starting automatic refresh...");

            autoRefreshWorker?.Stop();
            autoRefreshWorker = new AutoRefreshWorker(AutoRefreshData, () => { return ds?.Items?.FirstOrDefault(); }, AutoRefreshIntervalMs);
            autoRefreshWorker.Start();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeinitializeHandlers();

            autoRefreshWorker?.Stop();
            autoRefreshWorker = null;
        }

        public override void WillMoveToParentViewController(UIViewController parent)
        {
            base.WillMoveToParentViewController(parent);

            if (parent == null && SplitViewController != null && !SplitViewController.Collapsed)
            {
                var nc = (UINavigationController) SplitViewController.ViewControllers[1];
                nc.PopToRootViewController(false);

                var vc = (DocumentViewController) nc.ViewControllers[0];
                vc.ClearData();
            }
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning($"{nameof(DocumentsListViewController)} received memory warning!");

            autoRefreshWorker?.Stop();
            autoRefreshWorker = null;

            var ds = tableView?.Source as DataSource;
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
            tableView.Source = new DataSource(this, tableView, async (startId) => await RefreshData(startId), Localization.GetString("folder_empty"), PlatformConfig.Preferences.CompactDocumentsList);
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void

            tableView.RowHeight = UITableView.AutomaticDimension;
            tableView.EstimatedRowHeight = DocumentsTableViewCell.Height;
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

        void InitializeSearchBar()
        {
            DefinesPresentationContext = true;

            searchResultsController = new UITableViewController();
            searchResultsDataSource = new DataSource(this, searchResultsController.TableView, null, Localization.GetString("no_matching_documents"), PlatformConfig.Preferences.CompactDocumentsList);
            searchResultsController.TableView.Source = searchResultsDataSource;

            searchController = new UISearchController(searchResultsController)
            {
                HidesNavigationBarDuringPresentation = true,
                DimsBackgroundDuringPresentation = true,
                ObscuresBackgroundDuringPresentation = true,
                SearchResultsUpdater = this
            };
            searchController.SearchBar.Placeholder = Localization.GetString("filter");

            tableView.TableHeaderView = searchController.SearchBar;
        }

        void InitializeNavigationBarTitle()
        {
            UIView.AnimationsEnabled = false;
            NavigationItem.Title = Folder.Name;
            NavigationItem.Prompt = null;
            UIView.AnimationsEnabled = true;
        }

        void SubscribeToMessages()
        {
            PlatformConfig.MessengerHub.Subscribe<DocumentPreviewCommentsCountChangedMessage>(CommentsCountChangedHandler);
            PlatformConfig.MessengerHub.Subscribe<EntityCategoriesChangedMessage>(CategoriesChangedHandler, m => m.ObjectType == ObjectType.Document);

            PlatformConfig.MessengerHub.Subscribe<EntityRemovedFromFolderMessage>(HandleRemovedFromFolder, m => m.ObjectType == ObjectType.Document);
            PlatformConfig.MessengerHub.Subscribe<EntityMovedFromFolderMessage>(HandleMovedFromFolder, m => m.ObjectType == ObjectType.Document);
            PlatformConfig.MessengerHub.Subscribe<EntityDeletedMessage>(HandleDeleted, m => m.ObjectType == ObjectType.Document);
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
        }

        #endregion

        #region Actions

        public void DocumentSelected(DocumentPreview documentPreview)
        {
            if (tableView.Editing)
                return;

            if (SplitViewController != null && !SplitViewController.Collapsed)
            {
                var ds = (DataSource) tableView.Source;

                var nc = ((UINavigationController) SplitViewController.ViewControllers[1]);
                nc.PopToViewController(nc.ViewControllers[0], false);

                var vc = (DocumentViewController) nc.ViewControllers[0];

                if (vc.IsShowingDocumentWithId(documentPreview.Id))
                    return;

                vc.HidesBottomBarWhenPushed = false;

                vc.ClearData();
                vc.ReadStatusUpdated -= DocumentViewController_ReadStatusUpdated;
                vc.ReadStatusUpdated += DocumentViewController_ReadStatusUpdated;

                if (!searchController.Active)
                {
                    vc.SetData(Folder, documentPreview, ds.GetNextDocumentPreview, ds.GetPreviousDocumentPreview);
                    newDocumentsAvailableAction = vc.RefreshNavigationBar;
                }
                else
                {
                    vc.SetData(Folder, documentPreview);
                    newDocumentsAvailableAction = null;
                }

                vc.RefreshData();
            }
            else
            {
                var ds = (DataSource) tableView.Source;

                var vc = new DocumentViewController();
                vc.ReadStatusUpdated += DocumentViewController_ReadStatusUpdated;
                if (!searchController.Active)
                {
                    vc.SetData(Folder, documentPreview, ds.GetNextDocumentPreview, ds.GetPreviousDocumentPreview);
                }
                else
                {
                    vc.SetData(Folder, documentPreview);
                }
                vc.SetRefreshDataOnAppear();

                newDocumentsAvailableAction = vc.RefreshNavigationBar;
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
            if (SplitViewController != null && !SplitViewController.Collapsed)
            {
                var nc = (UINavigationController) SplitViewController.ViewControllers[1];
                nc.PopToRootViewController(false);

                var vc = (DocumentViewController) nc.ViewControllers[0];
                vc.ClearData();
            }

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

        void ExitEditItem_Clicked(object sender, EventArgs e) => EndEditing();

        void EndEditing()
        {
            tableView.SetEditing(false, true);
            NavigationItem.SetRightBarButtonItem(composeDocumentItem, false);
            NavigationItem.SetLeftBarButtonItem(NavigationItem.BackBarButtonItem, true);
        }

        void EditItem_Clicked(object sender, EventArgs e)
        {
            if (tableView.IndexPathsForSelectedRows == null || tableView.IndexPathsForSelectedRows.Length < 1)
                return;

            var eas = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);

            var rows = tableView.IndexPathsForSelectedRows.ToArray();
            var selectedDocuments = rows.Select(ip => ((DataSource) tableView.Source).Items[ip.Row]).ToList();

            if (selectedDocuments.Any(dp => !dp.IsReadByCurrent))
                eas.AddAction(UIAlertAction.Create(Localization.GetString("mark_as_read"), UIAlertActionStyle.Default, a =>
                {
                    MarkAsRead(selectedDocuments, rows);
                    EndEditing();
                }));

            if (selectedDocuments.Any(dp => dp.IsReadByCurrent))
                eas.AddAction(UIAlertAction.Create(Localization.GetString("mark_as_unread"), UIAlertActionStyle.Default, a =>
                {
                    MarkAsUnread(selectedDocuments, rows);
                    EndEditing();
                }));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_worktray"), UIAlertActionStyle.Default, a =>
            {
                CopyToWorktray(selectedDocuments);
                EndEditing();
            }));
            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_folder"), UIAlertActionStyle.Default, a =>
            {
                CopyToFolder(selectedDocuments);
                EndEditing();
            }));

            if (Folder.InternalType == FolderInternalType.FilterView || Folder.InternalType == FolderInternalType.Static || Folder.InternalType == FolderInternalType.Worktray)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("move_to_folder"), UIAlertActionStyle.Default, a =>
                {
                    MoveToFolder(selectedDocuments);
                    EndEditing();
                }));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("set_priority"), UIAlertActionStyle.Default, a => ShowPriorityActionSheet(selectedDocuments, (UIBarButtonItem) sender)));

            if (Folder.InternalType == FolderInternalType.FilterView || Folder.InternalType == FolderInternalType.Static || Folder.InternalType == FolderInternalType.Worktray)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete_from_folder"), UIAlertActionStyle.Default, a => RemoveFromFolder(selectedDocuments)));

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator || ServerConfig.SystemSettings.DocumentsModuleInfo.Permissions.DeleteAllowed || selectedDocuments.All(dp => dp.Direction == DocumentDirection.Draft))
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete"), UIAlertActionStyle.Destructive, a => Delete(selectedDocuments)));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, a => exitEditItem.Enabled = true));

            if (eas.PopoverPresentationController != null)
                eas.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate((UIBarButtonItem) sender);

            PresentViewController(eas, true, null);
        }


#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void ShowPriorityActionSheet(List<DocumentPreview> selectedDocuments, UIBarButtonItem barButtonItem)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            var priorities = new List<Priority>
            {
                Priority.Low,
                Priority.Normal,
                Priority.Urgent
            };
            var priorityStrings = priorities.Select(p => UI.PriorityString(p));
            var result = await Dialogs.ShowListDialogAsync(this, Localization.GetString("select_priority"), priorityStrings.ToArray(), barButtonItem);

            if (result < 0)
                return;

            var priority = priorities[result];

            await SetPriority(selectedDocuments, priority);
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void ShowPriorityActionSheet(DocumentPreview selectedDocument, UITableView tv, UITableViewCell cell)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            var priorities = new List<Priority>
            {
                Priority.Low,
                Priority.Normal,
                Priority.Urgent
            };
            var priorityStrings = priorities.Select(p => UI.PriorityString(p));
            var result = await Dialogs.ShowListDialogAsync(this, Localization.GetString("select_priority"), priorityStrings.ToArray(), tv, cell);

            if (result < 0)
                return;

            var priority = priorities[result];

            await SetPriority(new List<DocumentPreview>
            {
                selectedDocument
            }, priority);
        }

        async Task SetPriority(List<DocumentPreview> selectedDocuments, Priority priority)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("setting_priority___"));

            try
            {
                CommonConfig.Logger.Info($"Attempting to setting priority for documents");
                await Managers.DocumentsManager.SetDocumentsPriorityAsync(selectedDocuments, priority);

                EndEditing();

                UpdatePriorityForDocument(selectedDocuments.Select(d => d.Id));

                dismissAction();
            }
            catch (Exception ex)
            {
                EndEditing();
                dismissAction();

                CommonConfig.Logger.Error($"Error while setting priority for documents", ex);
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        void RemoveFromFolder(DocumentPreview selectedDocument) => RemoveFromFolder(new List<DocumentPreview>
        {
            selectedDocument
        });

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void RemoveFromFolder(List<DocumentPreview> selectedDocuments)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            var result = await Dialogs.ShowYesNoDialogAsync(this, Localization.GetString("delete_from_folder"), Localization.GetString("confirm_delete_from_folder_documents"));

            if (!result)
            {
                EndEditing();
                return;
            }

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("deleting_from_folder___"));

            try
            {
                CommonConfig.Logger.Info($"Attempting to remove documents from folder [folderId={Folder.Id}]");
                await Managers.CommonActionsManager.RemoveFromFolder(selectedDocuments.Cast<IBusinessEntity>().ToList(), Folder);

                RemoveDocumentsFromList(selectedDocuments.Select(s => s.Id));
                EndEditing();

                dismissAction();
            }
            catch (Exception ex)
            {
                EndEditing();
                dismissAction();

                CommonConfig.Logger.Error($"Error while removing documents from folder [folderId={Folder.Id}]", ex);
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        void Delete(DocumentPreview selectedDocument) => Delete(new List<DocumentPreview>
        {
            selectedDocument
        });

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void Delete(List<DocumentPreview> selectedDocuments)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            var result = await Dialogs.ShowYesNoDialogAsync(this, Localization.GetString("delete"), Localization.GetString("confirm_delete_documents"));

            if (!result)
            {
                EndEditing();
                return;
            }

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("deleting___"));

            try
            {
                CommonConfig.Logger.Info($"Attempting to delete documents");

                await Managers.CommonActionsManager.Delete(selectedDocuments.Cast<IBusinessEntity>().ToList());

                RemoveDocumentsFromList(selectedDocuments.Select(s => s.Id));
                EndEditing();

                dismissAction();
            }
            catch (Exception ex)
            {
                EndEditing();
                dismissAction();

                CommonConfig.Logger.Error($"Error while deleting documents", ex);
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        void Reply(DocumentPreview documentPreview, DocumentCreationModeFlag creationModeFlag)
        {
            var vc = new ComposeDocumentViewController
            {
                PreviousDocumentId = documentPreview.Id,
                CreationModeFlag = creationModeFlag,
                PreviousDocumentFolderId = Folder.Id,
                PreviousDocumentDirection = documentPreview.Direction,
                PreviousDocumentPreview = documentPreview
            };

            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        void ShowCategories(DocumentPreview selectedDocument)
        {
            var vc = new CategoriesListViewController
            {
                BusinessEntityPreview = selectedDocument
            };
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        void CopyToFolder(DocumentPreview selectedDocument) => CopyToFolder(new List<DocumentPreview>
        {
            selectedDocument
        });

        void CopyToFolder(List<DocumentPreview> selectedDocument)
        {
            var vc = new CopyMoveToFolderListViewController(selectedDocument.Cast<IBusinessEntity>().ToList());
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        void MoveToFolder(DocumentPreview selectedDocument) => MoveToFolder(new List<DocumentPreview>
        {
            selectedDocument
        });

        void MoveToFolder(List<DocumentPreview> selectedDocument)
        {
            var vc = new CopyMoveToFolderListViewController(selectedDocument.Cast<IBusinessEntity>().ToList(), Folder);
            vc.ModalPresentationStyle = UIModalPresentationStyle.PageSheet;
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        void CopyToWorktray(DocumentPreview selectedDocument) => CopyToWorktray(new List<DocumentPreview>
        {
            selectedDocument
        });

        void CopyToWorktray(List<DocumentPreview> selectedDocuments)
        {
            var vc = new CopyToWorktrayViewController
            {
                BusinessEntities = selectedDocuments.Cast<IBusinessEntity>().ToList()
            };
            vc.ModalPresentationStyle = UIModalPresentationStyle.PageSheet;
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        void MarkAsRead(DocumentPreview documentPreview, NSIndexPath row) => MarkAsRead(new List<DocumentPreview>
        {
            documentPreview
        }, new[]
        {
            row
        });

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void MarkAsRead(List<DocumentPreview> documentPreviews, NSIndexPath[] rows)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            CommonConfig.Logger.Info($"Attempting to mark as read [documentPreviews={documentPreviews.Count}]...");

            try
            {
                await Managers.DocumentsManager.SetDocumentsReadStatusAsync(documentPreviews, true);
                tableView.ReloadRows(rows, UITableViewRowAnimation.Fade);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Marking as read failed [documentPreviews.Count={documentPreviews.Count}]", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        void MarkAsUnread(DocumentPreview documentPreview, NSIndexPath row) => MarkAsUnread(new List<DocumentPreview>
        {
            documentPreview
        }, new[]
        {
            row
        });

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void MarkAsUnread(List<DocumentPreview> documentPreviews, NSIndexPath[] rows)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            CommonConfig.Logger.Info($"Attempting to mark as unread [documentPreviews={documentPreviews.Count}]...");

            try
            {
                await Managers.DocumentsManager.SetDocumentsReadStatusAsync(documentPreviews, false);
                tableView.ReloadRows(rows, UITableViewRowAnimation.Fade);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Marking as unread failed [documentPreviews.Count={documentPreviews.Count}]", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        void DoShowMoreActionSheet(NSIndexPath indexPath, DocumentPreview selectedDocument)
        {
            var eas = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);

            eas.AddAction(UIAlertAction.Create(Localization.GetString("categories"), UIAlertActionStyle.Default, a =>
            {
                ShowCategories(selectedDocument);
                EndEditing();
            }));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("reply"), UIAlertActionStyle.Default, a =>
            {
                Reply(selectedDocument, DocumentCreationModeFlag.Reply);
                EndEditing();
            }));
            eas.AddAction(UIAlertAction.Create(Localization.GetString("reply_all"), UIAlertActionStyle.Default, a =>
            {
                Reply(selectedDocument, DocumentCreationModeFlag.ReplyAll);
                EndEditing();
            }));
            eas.AddAction(UIAlertAction.Create(Localization.GetString("forward"), UIAlertActionStyle.Default, a =>
            {
                Reply(selectedDocument, DocumentCreationModeFlag.Forward);
                EndEditing();
            }));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_folder"), UIAlertActionStyle.Default, a =>
            {
                CopyToFolder(selectedDocument);
                EndEditing();
            }));

            if (Folder.InternalType == FolderInternalType.FilterView || Folder.InternalType == FolderInternalType.Static || Folder.InternalType == FolderInternalType.Worktray)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("move_to_folder"), UIAlertActionStyle.Default, a =>
                {
                    MoveToFolder(selectedDocument);
                    EndEditing();
                }));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("set_priority"), UIAlertActionStyle.Default, a => ShowPriorityActionSheet(selectedDocument, tableView, tableView.CellAt(indexPath))));

            if (Folder.InternalType == FolderInternalType.FilterView || Folder.InternalType == FolderInternalType.Static || Folder.InternalType == FolderInternalType.Worktray)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete_from_folder"), UIAlertActionStyle.Default, a => RemoveFromFolder(selectedDocument)));

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator || ServerConfig.SystemSettings.DocumentsModuleInfo.Permissions.DeleteAllowed || selectedDocument.Direction == DocumentDirection.Draft)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete"), UIAlertActionStyle.Destructive, a => Delete(selectedDocument)));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, null));

            if (eas.PopoverPresentationController != null)
                eas.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate(tableView, tableView.CellAt(indexPath));

            PresentViewController(eas, true, null);
        }

        #endregion

        #region Utilities

        void RemoveDocumentsFromList(IEnumerable<int> ids)
        {
            BeginInvokeOnMainThread(() =>
            {
                if (searchController.Active)
                    searchResultsDataSource.RemoveItems(ids.ToList());

                var ds = (DataSource) tableView.Source;
                ds.RemoveItems(ids.ToList());

                if (SplitViewController != null && !SplitViewController.Collapsed)
                {
                    var nc = (UINavigationController) SplitViewController.ViewControllers[1];
                    var vc = (DocumentViewController) nc.ViewControllers[0];
                    if (ids.Select(id => vc.IsShowingDocumentWithId(id)).Any(v => v))
                        vc.ClearData();
                }
            });
        }

        void UpdatePriorityForDocument(IEnumerable<int> ids)
        {
            if (SplitViewController != null && !SplitViewController.Collapsed)
            {
                var nc = (UINavigationController) SplitViewController.ViewControllers[1];
                var vc = (DocumentViewController) nc.ViewControllers[0];
                if (ids.Select(id => vc.IsShowingDocumentWithId(id)).Any(v => v))
                {
                    vc.UpdatePriority();
                }
            }
        }

        #endregion

        #region Refreshing

        async void RefreshControl_ValueChanged(object sender, EventArgs e) => await RefreshData(forceClear: true);

        async Task RefreshData(int startId = -1, int endId = -1, bool forceClear = false)
        {
            if (refreshing)
                return;

            refreshing = true;
            refreshControl.ValueChanged -= RefreshControl_ValueChanged;

            CommonConfig.Logger.Info($"Refreshing documents list [folder={Folder?.Name}, startId={startId}, endId={endId}, forceClear={forceClear}]");

            try
            {
                var ds = (DataSource) tableView.Source;

                if (forceClear)
                    ds.Reset();

                var documentPreviews = await Managers.DocumentsManager.GetDocumentPreviewsAsync(Folder, startId, endId);
                ds.LoadMoreEnabled = documentPreviews.Count >= PlatformConfig.Preferences.DocumentsToDownload;
                CommonConfig.Logger.Info($"Enable load more documents set to {ds.LoadMoreEnabled}");

                Managers.DownloadManager.Notify(ObjectType.Document, Folder.Id);
                ds.AppendItems(documentPreviews);

                if (documentPreviews.Any() && newDocumentsAvailableAction != null)
                    newDocumentsAvailableAction();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Could not refresh folders [folder={Folder?.Name}, startId={startId}, endId={endId}, forceClear={forceClear}]", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);

                NavigationController?.PopViewController(true);
            }

            refreshControl.EndRefreshing();
            refreshControl.ValueChanged += RefreshControl_ValueChanged;

            refreshing = false;
        }

        async Task AutoRefreshData(int endId)
        {
            if (refreshing)
                return;

            refreshing = true;

            refreshControl.Enabled = false;

            try
            {
                CommonConfig.Logger.Debug($"Attempting automatic refresh [endId={endId}, isBeingDismissed={IsBeingDismissed}]...");

                if (IsBeingDismissed)
                    return;

                CommonConfig.Logger.Debug($"Automatic refresh running...");

                var documents = await Managers.DocumentsManager.GetDocumentPreviewsAsync(Folder, endId: endId);

                if (documents.Count > 0)
                {
                    CommonConfig.Logger.Info($"Received {documents?.Count} new documents");

                    Managers.DownloadManager.Notify(ObjectType.Document, Folder.Id);

                    var ds = tableView.Source as DataSource;
                    ds?.PrependItems(documents);

                    if (newDocumentsAvailableAction != null)
                        newDocumentsAvailableAction();
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Automatic refresh failed [endId={endId}]", ex);
            }
            finally
            {
                CommonConfig.Logger.Debug($"Automatic refresh finished");
            }

            refreshControl.Enabled = true;
            refreshing = false;
        }

        #endregion

        #region Events

        void DocumentViewController_ReadStatusUpdated(object sender, ReadStatusUpdatedEventArgs e)
        {
            BeginInvokeOnMainThread(() =>
            {
                var selectedRow = tableView.IndexPathForSelectedRow;

                (tableView.Source as DataSource).UpdateDocumentPreview(e.DocumentPreview);
                tableView.ReloadData();

                if (selectedRow != null)
                {
                    tableView.SelectRow(selectedRow, false, UITableViewScrollPosition.None);
                }
            });
        }

        void CommentsCountChangedHandler(DocumentPreviewCommentsCountChangedMessage message)
        {
            BeginInvokeOnMainThread(() =>
            {
                var ds = tableView.Source as DataSource;
                var index = ds.Items.FindIndex(dp => dp.Id == message.DocumentPreviewId);

                if (index >= 0)
                {
                    var documentPreview = ds.Items[index];
                    documentPreview.CommentsCount = message.CommentsCount;

                    var selectedRow = tableView.IndexPathForSelectedRow;

                    tableView.ReloadRows(new NSIndexPath[]
                    {
                        NSIndexPath.FromRowSection(index, 0)
                    }, UITableViewRowAnimation.Fade);

                    if (selectedRow != null)
                    {
                        tableView.SelectRow(selectedRow, false, UITableViewScrollPosition.None);
                    }
                }
            });
        }

        void CategoriesChangedHandler(EntityCategoriesChangedMessage message)
        {
            BeginInvokeOnMainThread(() =>
            {
                var ds = tableView.Source as DataSource;
                var index = ds.Items.FindIndex(dp => dp.Id == message.EntityId);

                if (index >= 0)
                {
                    var documentPreview = ds.Items[index];
                    documentPreview.Categories.Clear();
                    documentPreview.Categories.AddRange(message.Categories);

                    var selectedRow = tableView.IndexPathForSelectedRow;

                    tableView.ReloadRows(new NSIndexPath[]
                    {
                        NSIndexPath.FromRowSection(index, 0)
                    }, UITableViewRowAnimation.Fade);

                    if (selectedRow != null)
                    {
                        tableView.SelectRow(selectedRow, false, UITableViewScrollPosition.None);
                    }
                }
            });
        }

        #endregion

        #region Searching

        void IUISearchResultsUpdating.UpdateSearchResultsForSearchController(UISearchController searchController)
        {
            var searchText = searchController.SearchBar.Text;

            if (!searchController.Active || string.IsNullOrWhiteSpace(searchText))
            {
                searchCancellationTokenSourceList.ForEach(cts => cts?.Cancel());
                searchCancellationTokenSourceList.Clear();
                searchResultsDataSource.Reset();
            }
            else
            {
                if (searchCancellationTokenSource != null)
                {
                    searchCancellationTokenSource.Cancel();
                    searchCancellationTokenSourceList.Remove(searchCancellationTokenSource);
                    searchCancellationTokenSource = null;
                }

                searchCancellationTokenSource = new CancellationTokenSource();
                searchCancellationTokenSourceList.Add(searchCancellationTokenSource);

                DoSearchDocuments(searchText, searchCancellationTokenSource.Token);
            }
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void DoSearchDocuments(string searchText, CancellationToken ct)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            searchResultsDataSource.Reset();

            await Task.Delay(500);

            if (ct.IsCancellationRequested)
                return;

            var ds = (DataSource) tableView.Source;
            var filteredDocuments = ds.Items.Where(dp => MatchesQuery(dp, searchText)).ToList();

            if (ct.IsCancellationRequested)
                return;

            searchResultsDataSource.AppendItems(filteredDocuments);
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

        #endregion

        #region Messages handlers

        void HandleRemovedFromFolder(EntityRemovedFromFolderMessage m) => RemoveDocumentsFromList(m.EntitiesId);

        void HandleMovedFromFolder(EntityMovedFromFolderMessage m) => RemoveDocumentsFromList(m.EntitiesId);

        void HandleDeleted(EntityDeletedMessage m) => RemoveDocumentsFromList(m.EntitiesId);

        #endregion

        class DataSource : UITableViewSource, IDisposable
        {
            public bool Empty
            {
                get { return documentPreviewsInView.Count < 1; }
            }

            public List<DocumentPreview> Items
            {
                get { return documentPreviewsInView; }
            }

            public bool LoadMoreEnabled { get; set; }

            DocumentsListViewController viewController;
            UITableView documentsTableView;
            readonly Action<int> loadMoreAction;
            readonly string emptyText;
            readonly bool compact;

            bool loading = true;
            List<DocumentPreview> documentPreviewsInView = new List<DocumentPreview>(1000);

            public DataSource(DocumentsListViewController viewController, UITableView documentsTableView, Action<int> loadMoreAction, string emptyText, bool compact)
            {
                this.viewController = viewController;
                this.documentsTableView = documentsTableView;
                this.loadMoreAction = loadMoreAction;
                this.emptyText = emptyText;
                this.compact = compact;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                {
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as WaitTableViewCell ?? WaitTableViewCell.Create();
                }

                if (documentPreviewsInView.Count < 1)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                    emptyCell.Initialize(emptyText);
                    return emptyCell;
                }

                var dp = documentPreviewsInView[indexPath.Row];


                if (LoadMoreEnabled && loadMoreAction != null && dp.Id == documentPreviewsInView.Last().Id)
                    loadMoreAction(dp.Id);

                if (dp.Direction == DocumentDirection.External)
                {
                    var cell = tableView.DequeueReusableCell(ExternalDocumentsTableViewCell.Key) as ExternalDocumentsTableViewCell ?? ExternalDocumentsTableViewCell.Create();
                    cell.Initialize(dp);
                    return cell;
                }

                if (compact)
                {
                    var cell = tableView.DequeueReusableCell(DocumentsCompactTableViewCell.Key) as DocumentsCompactTableViewCell ?? DocumentsCompactTableViewCell.Create();
                    cell.Initialize(dp);
                    return cell;
                }
                else
                {
                    var cell = tableView.DequeueReusableCell(DocumentsTableViewCell.Key) as DocumentsTableViewCell ?? DocumentsTableViewCell.Create();
                    cell.Initialize(dp);
                    return cell;
                }
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading)
                    return 1;

                if (documentPreviewsInView.Count < 1)
                    return 1;

                return documentPreviewsInView.Count;
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                if (documentPreviewsInView.Count > 0 && documentPreviewsInView[indexPath.Row]?.Direction == DocumentDirection.External)
                    return ExternalDocumentsTableViewCell.Height;

                return compact ? DocumentsCompactTableViewCell.Height : DocumentsTableViewCell.Height;
            }

            public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
            {
                return true;
            }

            public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
            {
                var actions = new List<UITableViewRowAction>();

                var documentPreview = documentPreviewsInView[indexPath.Row];

                var moreAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("more"), (a, ip) => { viewController.DoShowMoreActionSheet(indexPath, documentPreview); });
                moreAction.BackgroundColor = Theme.DarkerBlue;
                actions.Add(moreAction);

                var copyToWorktrayAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("copy_to_worktray_ml"), (a, ip) =>
                {
                    viewController.CopyToWorktray(documentPreview);
                    viewController.EndEditing();
                });
                copyToWorktrayAction.BackgroundColor = Theme.DarkBlue;
                actions.Add(copyToWorktrayAction);

                if (documentPreview.IsReadByCurrent)
                {
                    var markAsUnreadAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("mark_as_unread_ml"), (a, ip) =>
                    {
                        viewController.MarkAsUnread(documentPreview, indexPath);
                        viewController.EndEditing();
                    });
                    markAsUnreadAction.BackgroundColor = Theme.Brown;
                    actions.Add(markAsUnreadAction);
                }
                else
                {
                    var markAsReadAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("mark_as_read_ml"), (a, ip) =>
                    {
                        viewController.MarkAsRead(documentPreview, indexPath);
                        viewController.EndEditing();
                    });
                    markAsReadAction.BackgroundColor = Theme.Brown;
                    actions.Add(markAsReadAction);
                }

                return actions.ToArray();
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var dp = documentPreviewsInView[indexPath.Row];
                viewController.DocumentSelected(dp);
            }

            public void PrependItems(List<DocumentPreview> documentPreviews)
            {
                loading = false;

                documentPreviewsInView.InsertRange(0, documentPreviews);
                var indexes = Enumerable.Range(0, documentPreviews.Count).Select(i => NSIndexPath.FromRowSection(i, 0)).ToArray();
                documentsTableView.InsertRows(indexes, UITableViewRowAnimation.Fade);
            }

            public void AppendItems(List<DocumentPreview> documentPreviews)
            {
                loading = false;

                documentPreviewsInView.AddRange(documentPreviews);
                documentsTableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            public void RemoveItems(List<int> documentIds)
            {
                var indices = documentPreviewsInView.Select((d, i) => new
                    {
                        d,
                        i
                    })
                    .Where(x => documentIds.Contains(x.d.Id))
                    .Select(x => x.i)
                    .ToList();
                indices.OrderByDescending(i => i).ForEach(documentPreviewsInView.RemoveAt);

                documentsTableView.BeginUpdates();

                if (!documentPreviewsInView.Any())
                {
                    documentsTableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Automatic);
                }
                else
                {
                    var indexPaths = indices.Select(i => NSIndexPath.FromRowSection(i, 0)).ToArray();
                    documentsTableView.DeleteRows(indexPaths, UITableViewRowAnimation.Automatic);
                }

                documentsTableView.EndUpdates();
            }

            public DocumentPreview GetNextDocumentPreview(DocumentPreview documentPreview, out bool previousDocumentAvailable, out bool nextDocumentAvailable, bool scrollToDocument = false)
            {
                try
                {
                    var currentDocumentRow = documentPreviewsInView.IndexOf(d => d.Id == documentPreview.Id);
                    if (currentDocumentRow < 0)
                    {
                        previousDocumentAvailable = false;
                        nextDocumentAvailable = false;
                        return null;
                    }

                    var nextDocumentRow = currentDocumentRow + 1;
                    previousDocumentAvailable = true;
                    nextDocumentAvailable = nextDocumentRow < documentPreviewsInView.Count - 1;

                    if (!nextDocumentAvailable && LoadMoreEnabled && loadMoreAction != null)
                        loadMoreAction(documentPreviewsInView.Last().Id);

                    if (scrollToDocument)
                    {
                        ScrollAndSelect(nextDocumentRow);
                    }

                    return documentPreviewsInView.ElementAtOrDefault(nextDocumentRow);
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error("Could not switch to next document", ex);

                    previousDocumentAvailable = false;
                    nextDocumentAvailable = false;
                    return null;
                }
            }

            public DocumentPreview GetPreviousDocumentPreview(DocumentPreview documentPreview, out bool previousDocumentAvailable, out bool nextDocumentAvailable, bool scrollToDocument = false)
            {
                try
                {
                    var currentDocumentRow = documentPreviewsInView.IndexOf(d => d.Id == documentPreview.Id);
                    if (currentDocumentRow < 0)
                    {
                        previousDocumentAvailable = false;
                        nextDocumentAvailable = false;
                        return null;
                    }

                    var previousDocumentRow = currentDocumentRow - 1;
                    previousDocumentAvailable = previousDocumentRow > 0;
                    nextDocumentAvailable = previousDocumentRow < documentPreviewsInView.Count - 1;

                    if (scrollToDocument)
                    {
                        ScrollAndSelect(previousDocumentRow);
                    }

                    return documentPreviewsInView.ElementAtOrDefault(previousDocumentRow);
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error("Could not switch to previous document", ex);

                    previousDocumentAvailable = false;
                    nextDocumentAvailable = false;
                    return null;
                }
            }

            void ScrollAndSelect(int row)
            {
                var selectedIndexPaths = documentsTableView.IndexPathsForSelectedRows;
                if (selectedIndexPaths != null)
                {
                    foreach (var indexPath in selectedIndexPaths)
                    {
                        documentsTableView.DeselectRow(indexPath, true);
                    }
                }

                documentsTableView.SelectRow(NSIndexPath.FromRowSection(row, 0), true, UITableViewScrollPosition.Middle);
            }

            public void Reset()
            {
                loading = true;

                documentPreviewsInView.Clear();
                documentsTableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                viewController = null;
                documentsTableView = null;
                documentPreviewsInView = null;
            }

            public void UpdateDocumentPreview(DocumentPreview documentPreview)
            {
                var documentRow = documentPreviewsInView.IndexOf(d => d.Id == documentPreview.Id);
                if (documentRow < 0)
                    return;

                documentsTableView.ReloadRows(new NSIndexPath[]
                {
                    NSIndexPath.FromRowSection(documentRow, 0)
                }, UITableViewRowAnimation.Fade);
            }
        }

        class AutoRefreshWorker : NSObject
        {
            CancellationTokenSource cts;

            readonly Func<int, Task> work;
            readonly Func<DocumentPreview> firstOrDefaultItem;
            readonly int intervalMs;

            readonly object lockObj = new object();

            public AutoRefreshWorker(Func<int, Task> work, Func<DocumentPreview> firstOrDefaultItem, int intervalMs)
            {
                this.work = work;
                this.firstOrDefaultItem = firstOrDefaultItem;
                this.intervalMs = intervalMs;
            }

            public void Start()
            {
                lock (lockObj)
                {
                    cts?.Cancel();
                    cts = new CancellationTokenSource();
                    Task.Run(async () =>
                    {
                        while (true)
                        {
                            await Task.Delay(intervalMs);
                            if (cts.IsCancellationRequested)
                                break;

                            var first = firstOrDefaultItem();
                            if (first != null)
#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
                                InvokeOnMainThread(async () => await work(first.Id));
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
                        }
                    });
                }
            }

            public void Stop()
            {
                lock (lockObj)
                    cts?.Cancel();
            }
        }
    }
}