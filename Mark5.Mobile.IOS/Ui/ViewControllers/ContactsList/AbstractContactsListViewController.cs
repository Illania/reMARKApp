using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Model.HubMessages;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;
using ObjCRuntime;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.ContactsList
{
    public abstract class AbstractContactsListViewController : AbstractViewController, IPrimaryViewController, IUISearchResultsUpdating, IUIGestureRecognizerDelegate
    {
        protected readonly bool DisableRowActions;

        public Folder Folder { get; set; }

        protected UIBarButtonItem ExitEditItem;
        protected UIBarButtonItem EditItem;

        protected UIRefreshControl RefreshControl;
        protected UITableView TableView;
        protected UISearchController SearchController;
        protected UITableViewController SearchResultsController;
        protected DataSource SearchResultsDataSource;

        protected CancellationTokenSource searchCancellationTokenSource;
        readonly List<CancellationTokenSource> searchCancellationTokenSourceList = new List<CancellationTokenSource>();

        bool refreshing;

        CancellationTokenSource cts;

        protected AbstractContactsListViewController(bool disableRowActions)
        {
            DisableRowActions = disableRowActions;
        }

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

            if (TableView?.IndexPathForSelectedRow != null)
                TableView.DeselectRow(TableView.IndexPathForSelectedRow, true);

            if (TableView?.IndexPathsForSelectedRows?.Length > 0)
                foreach (var selectedIndexPath in TableView?.IndexPathsForSelectedRows)
                    TableView.DeselectRow(selectedIndexPath, true);

            ReachabilityBar.Attach(View, TableView, (float)NavigationController.BottomLayoutGuide.Length, UITextAlignment.Left);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info($"{nameof(ContactsListViewController)} appeared");

            var ds = (DataSource)TableView.Source;
            if (ds.Empty)
                RefreshData();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeinitializeHandlers();

            cts?.Cancel();
        }

        public override void WillMoveToParentViewController(UIViewController parent)
        {
            base.WillMoveToParentViewController(parent);

            if (parent == null && SplitViewController != null && !SplitViewController.Collapsed)
            {
                var nc = (UINavigationController)SplitViewController.ViewControllers[1];
                nc.PopToRootViewController(false);

                var vc = (ContactViewController)nc.ViewControllers[0];
                vc.ClearData();
            }
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning($"{nameof(ContactsListViewController)} received memory warning!");

            var ds = TableView?.Source as DataSource;
            ds?.Reset();

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        #endregion

        #region Initialization

        void InitializeNavigationBar()
        {
            ExitEditItem = new UIBarButtonItem(UIBarButtonSystemItem.Done);
            EditItem = new UIBarButtonItem(UIBarButtonSystemItem.Edit);
        }

        void InitializeView()
        {
            AutomaticallyAdjustsScrollViewInsets = true;

            TableView = new UITableView();
            TableView.ClipsToBounds = false;
            TableView.Source = new DataSource(this, TableView, Localization.GetString("folder_empty"), DisableRowActions);
            TableView.AllowsSelectionDuringEditing = false;
            TableView.AllowsMultipleSelectionDuringEditing = true;
            TableView.TranslatesAutoresizingMaskIntoConstraints = false;
            View.AddSubview(TableView);
            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(TableView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(TableView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(TableView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(TableView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1f, 0f)
            });

            var longPressRecognizer = new UILongPressGestureRecognizer(this, new Selector("longPressed:"))
            {
                MinimumPressDuration = 1f,
                Delegate = this
            };
            TableView.AddGestureRecognizer(longPressRecognizer);

            RefreshControl = new UIRefreshControl();
            RefreshControl.BackgroundColor = UIColor.White;
            TableView.AddSubview(RefreshControl);
        }

        void InitializeSearchBar()
        {
            DefinesPresentationContext = true;

            SearchResultsController = new UITableViewController();
            SearchResultsDataSource = new DataSource(this, SearchResultsController.TableView, Localization.GetString("no_matching_contacts"), DisableRowActions);
            SearchResultsController.TableView.Source = SearchResultsDataSource;

            SearchController = new UISearchController(SearchResultsController)
            {
                HidesNavigationBarDuringPresentation = true,
                DimsBackgroundDuringPresentation = true,
                ObscuresBackgroundDuringPresentation = true,
                SearchResultsUpdater = this
            };
            SearchController.SearchBar.Placeholder = Localization.GetString("filter");

            TableView.TableHeaderView = SearchController.SearchBar;
        }

        void SubscribeToMessages()
        {
            PlatformConfig.MessengerHub.Subscribe<EntityCategoriesChangedMessage>(HandleCategoriesChanged, m => m.ObjectType == ObjectType.Contact);
            PlatformConfig.MessengerHub.Subscribe<EntityRemovedFromFolderMessage>(HandleRemovedFromFolder, m => m.ObjectType == ObjectType.Contact);
            PlatformConfig.MessengerHub.Subscribe<EntityMovedFromFolderMessage>(HandleMovedFromFolder, m => m.ObjectType == ObjectType.Contact);
            PlatformConfig.MessengerHub.Subscribe<EntityDeletedMessage>(HandleDeleted, m => m.ObjectType == ObjectType.Contact);
        }

        void InitializeNavigationBarTitle()
        {
            UIView.AnimationsEnabled = false;
            NavigationItem.Title = Folder.Name;
            NavigationItem.Prompt = null;
            UIView.AnimationsEnabled = true;
        }

        void InitializeHandlers()
        {
            if (ExitEditItem != null)
                ExitEditItem.Clicked += ExitEditItem_Clicked;

            if (EditItem != null)
                EditItem.Clicked += EditItem_Clicked;

            if (RefreshControl != null)
                RefreshControl.ValueChanged += RefreshControl_ValueChanged;
        }

        void DeinitializeHandlers()
        {
            if (ExitEditItem != null)
                ExitEditItem.Clicked -= ExitEditItem_Clicked;

            if (EditItem != null)
                EditItem.Clicked -= EditItem_Clicked;

            if (RefreshControl != null)
                RefreshControl.ValueChanged -= RefreshControl_ValueChanged;
        }

        #endregion

        #region Actions

        public virtual void ContactSelected(UITableView tableView, ContactPreview contactPreview, NSIndexPath indexPath)
        {
        }

        [Export("longPressed:")]
        public void LongPressed(UILongPressGestureRecognizer recognizer)
        {
            if (TableView.Editing)
                return;

            StartEditing();

            var point = recognizer.LocationInView(TableView);
            var indexPath = TableView.IndexPathForRowAtPoint(point);

            TableView.SelectRow(indexPath, true, UITableViewScrollPosition.None);
        }

        void StartEditing()
        {
            TableView.SetEditing(true, true);
            NavigationItem.SetRightBarButtonItem(ExitEditItem, true);
            NavigationItem.SetLeftBarButtonItem(EditItem, true);

            SearchController.SearchBar.UserInteractionEnabled = false;
            SearchController.SearchBar.Alpha = .5f;

            if (SplitViewController != null && !SplitViewController.Collapsed)
            {
                var nc = (UINavigationController)SplitViewController.ViewControllers[1];
                nc.PopToRootViewController(false);

                var vc = (ContactViewController)nc.ViewControllers[0];
                vc.ClearData();
            }
        }

        void ExitEditItem_Clicked(object sender, EventArgs e)
        {
            EndEditing();
        }

        void EndEditing()
        {
            TableView.SetEditing(false, true);
            NavigationItem.SetRightBarButtonItem(null, true);
            NavigationItem.SetLeftBarButtonItem(NavigationItem.BackBarButtonItem, true);

            SearchController.SearchBar.UserInteractionEnabled = true;
            SearchController.SearchBar.Alpha = 1f;
        }

        void EditItem_Clicked(object sender, EventArgs e)
        {
            if (TableView.IndexPathsForSelectedRows == null || TableView.IndexPathsForSelectedRows.Length < 1)
                return;

            var eas = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);

            var rows = TableView.IndexPathsForSelectedRows.ToArray();
            var selectedContacts = rows.Select(ip => ((DataSource)TableView.Source).FindItemAtIndexPath(ip)).ToList();

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_worktray"),
                UIAlertActionStyle.Default,
                a =>
                {
                    CopyToWorktray(selectedContacts);
                    EndEditing();
                }));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_folder"),
                UIAlertActionStyle.Default,
                a =>
                {
                    CopyToFolder(selectedContacts);
                    EndEditing();
                }));

            if (Folder.InternalType == FolderInternalType.FilterView || Folder.InternalType == FolderInternalType.Static || Folder.InternalType == FolderInternalType.Worktray)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("move_to_folder"),
                    UIAlertActionStyle.Default,
                    a =>
                    {
                        MoveToFolder(selectedContacts);
                        EndEditing();
                    }));

            if (Folder.InternalType == FolderInternalType.FilterView || Folder.InternalType == FolderInternalType.Static || Folder.InternalType == FolderInternalType.Worktray)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete_from_folder"), UIAlertActionStyle.Default, a => RemoveFromFolder(selectedContacts)));

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator || ServerConfig.SystemSettings.ContactsModuleInfo.Permissions.DeleteAllowed)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete"), UIAlertActionStyle.Destructive, a => Delete(selectedContacts)));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, a => ExitEditItem.Enabled = true));

            if (eas.PopoverPresentationController != null)
                eas.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate((UIBarButtonItem)sender);

            ExitEditItem.Enabled = false;
            PresentViewController(eas, true, null);
        }

        #endregion

        #region Refreshing

        void RefreshControl_ValueChanged(object sender, EventArgs e)
        {
            RefreshData(forceClear: true);
        }

        void RefreshData(int startRowId = -1, bool forceClear = false)
        {
            if (refreshing)
                return;

            refreshing = true;
            RefreshControl.ValueChanged -= RefreshControl_ValueChanged;

            CommonConfig.Logger.Info($"Refreshing contacts list [folder={Folder?.Name}, startRowId={startRowId}, forceClear={forceClear}]");

            cts?.Cancel();
            cts = new CancellationTokenSource();

            if (forceClear)
            {
                var ds = (DataSource)TableView.Source;
                ds.Reset();
            }

            Managers.ContactsManager.GetAllContactPreviews(Folder,
                cps =>
                {
                    Managers.DownloadManager.Notify(ObjectType.Contact, Folder.Id);
                    InvokeOnMainThread(() =>
                    {
                        var ds = (DataSource)TableView.Source;
                        ds.AppendItems(cps);
                    });
                },
                () =>
                {
                    RefreshControl.EndRefreshing();
                    RefreshControl.ValueChanged += RefreshControl_ValueChanged;

                    refreshing = false;

                    CommonConfig.Logger.Info($"Refresh finished");
#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
                },
                async ex =>
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
                {
                    RefreshControl.EndRefreshing();
                    RefreshControl.ValueChanged += RefreshControl_ValueChanged;

                    refreshing = false;

                    CommonConfig.Logger.Error($"Could not refresh folders [folder={Folder?.Name}, startRowId={startRowId}, forceClear={forceClear}]", ex);

                    await Dialogs.ShowErrorDialogAsync(this, ex);

                    NavigationController?.PopViewController(true);
                },
                startRowId,
                cts.Token);
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
                SearchResultsDataSource.Reset();
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

                DoSearchContacts(searchText, searchCancellationTokenSource.Token);
            }
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void DoSearchContacts(string searchText, CancellationToken ct)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            SearchResultsDataSource.Reset();

            await Task.Delay(500);

            if (ct.IsCancellationRequested)
                return;

            var ds = (DataSource)TableView.Source;
            var filteredContacts = ds.Items.Where(cp => MatchesQuery(cp, searchText)).ToList();

            if (ct.IsCancellationRequested)
                return;

            SearchResultsDataSource.AppendItems(filteredContacts);
        }

        static bool MatchesQuery(ContactPreview cp, string query)
        {
            if (cp.Name?.ContainsCaseInsensitive(query) ?? false)
                return true;

            if (cp.CompanyName?.ContainsCaseInsensitive(query) ?? false)
                return true;

            if (cp.ShortId?.ContainsCaseInsensitive(query) ?? false)
                return true;

            if (cp.Description?.ContainsCaseInsensitive(query) ?? false)
                return true;

            if (cp.PrimaryAddress?.Address?.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0)
                return true;

            if (cp.Categories.Any(da => da.Name?.ContainsCaseInsensitive(query) ?? false))
                return true;

            return false;
        }

        #endregion

        #region Actions

        void RemoveFromFolder(ContactPreview selectedContact)
        {
            RemoveFromFolder(new List<ContactPreview>
            {
                selectedContact
            });
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void RemoveFromFolder(List<ContactPreview> selectedContacts)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            var result = await Dialogs.ShowYesNoDialogAsync(this, Localization.GetString("delete_from_folder"), Localization.GetString("confirm_delete_from_folder_contacts"));

            if (!result)
            {
                EndEditing();
                return;
            }

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("deleting_from_folder___"));

            try
            {
                CommonConfig.Logger.Info($"Attempting to remove contacts from folder [folderId={Folder.Id}]");

                await Managers.CommonActionsManager.RemoveFromFolder(selectedContacts.Cast<IBusinessEntity>().ToList(), Folder);

                RemoveContactsFromList(selectedContacts.Select(s => s.Id));
                EndEditing();

                dismissAction();
            }
            catch (Exception ex)
            {
                EndEditing();
                dismissAction();

                CommonConfig.Logger.Error($"Error while removing contacts from folder [folderId={Folder.Id}]", ex);
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        void Delete(ContactPreview selectedContact)
        {
            Delete(new List<ContactPreview>
            {
                selectedContact
            });
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void Delete(List<ContactPreview> selectedContacts)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            var result = await Dialogs.ShowYesNoDialogAsync(this, Localization.GetString("delete"), Localization.GetString("confirm_delete_contacts"));

            if (!result)
            {
                EndEditing();
                return;
            }

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("deleting___"));

            try
            {
                CommonConfig.Logger.Info($"Attempting to delete contacts");

                await Managers.CommonActionsManager.Delete(selectedContacts.Cast<IBusinessEntity>().ToList());

                RemoveContactsFromList(selectedContacts.Select(s => s.Id));
                EndEditing();

                dismissAction();
            }
            catch (Exception ex)
            {
                EndEditing();
                dismissAction();

                CommonConfig.Logger.Error($"Error while deleting contacts", ex);
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        void CopyToWorktray(ContactPreview selectedContact)
        {
            CopyToWorktray(new List<ContactPreview>
            {
                selectedContact
            });
        }

        void CopyToWorktray(List<ContactPreview> selectedContacts)
        {
            var vc = new CopyToWorktrayViewController
            {
                BusinessEntities = selectedContacts.Cast<IBusinessEntity>().ToList()
            };
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        void CopyToFolder(ContactPreview selecteContact)
        {
            CopyToFolder(new List<ContactPreview>
            {
                selecteContact
            });
        }

        void CopyToFolder(List<ContactPreview> selectedContacts)
        {
            var vc = new CopyMoveToFolderListViewController(selectedContacts.Cast<IBusinessEntity>().ToList());
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        void ShowCategories(ContactPreview selectedContact)
        {
            var vc = new CategoriesListViewController
            {
                BusinessEntityPreview = selectedContact
            };
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        void MoveToFolder(ContactPreview selectedContact)
        {
            MoveToFolder(new List<ContactPreview>
            {
                selectedContact
            });
        }

        void MoveToFolder(List<ContactPreview> selectedContacts)
        {
            var vc = new CopyMoveToFolderListViewController(selectedContacts.Cast<IBusinessEntity>().ToList(), Folder);
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        void DoShowMoreActionSheet(NSIndexPath indexPath, ContactPreview selectedContact)
        {
            var eas = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);


            eas.AddAction(UIAlertAction.Create(Localization.GetString("categories"),
                UIAlertActionStyle.Default,
                a =>
                {
                    ShowCategories(selectedContact);
                    EndEditing();
                }));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_folder"),
                UIAlertActionStyle.Default,
                a =>
                {
                    CopyToFolder(selectedContact);
                    EndEditing();
                }));

            if (Folder.InternalType == FolderInternalType.FilterView || Folder.InternalType == FolderInternalType.Static || Folder.InternalType == FolderInternalType.Worktray)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("move_to_folder"),
                    UIAlertActionStyle.Default,
                    a =>
                    {
                        MoveToFolder(selectedContact);
                        EndEditing();
                    }));

            if (Folder.InternalType == FolderInternalType.FilterView || Folder.InternalType == FolderInternalType.Static || Folder.InternalType == FolderInternalType.Worktray)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete_from_folder"), UIAlertActionStyle.Default, a => RemoveFromFolder(selectedContact)));

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator || ServerConfig.SystemSettings.ContactsModuleInfo.Permissions.DeleteAllowed)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete"), UIAlertActionStyle.Destructive, a => Delete(selectedContact)));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, a => EndEditing()));

            if (eas.PopoverPresentationController != null)
                eas.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate(TableView, TableView.CellAt(indexPath));

            PresentViewController(eas, true, null);
        }

        #endregion

        #region Messages handlers

        void HandleRemovedFromFolder(EntityRemovedFromFolderMessage m)
        {
            RemoveContactsFromList(m.EntitiesId);
        }

        void HandleMovedFromFolder(EntityMovedFromFolderMessage m)
        {
            RemoveContactsFromList(m.EntitiesId);
        }

        void HandleDeleted(EntityDeletedMessage m)
        {
            RemoveContactsFromList(m.EntitiesId);
        }

        void HandleCategoriesChanged(EntityCategoriesChangedMessage message)
        {
            BeginInvokeOnMainThread(() =>
            {
                var ds = TableView.Source as DataSource;
                var indexPath = ds.FindItemIndexPath(message.EntityId);

                if (indexPath != null)
                {
                    var contactPreview = ds.FindItemAtIndexPath(indexPath);
                    contactPreview.Categories.Clear();
                    contactPreview.Categories.AddRange(message.Categories);

                    var selectedRow = TableView.IndexPathForSelectedRow;

                    TableView.ReloadRows(new[]
                        {
                            indexPath
                        },
                        UITableViewRowAnimation.Fade);

                    if (selectedRow != null)
                        TableView.SelectRow(selectedRow, false, UITableViewScrollPosition.None);
                }
            });
        }

        #endregion

        #region Utilities

        void RemoveContactsFromList(IEnumerable<int> ids)
        {
            if (SearchController.Active)
                SearchResultsDataSource.RemoveItems(ids.ToList());

            var ds = (DataSource)TableView.Source;
            ds.RemoveItems(ids.ToList());
            if (SplitViewController != null && !SplitViewController.Collapsed)
            {
                var nc = (UINavigationController)SplitViewController.ViewControllers[1];
                var vc = (ContactViewController)nc.ViewControllers[0];
                if (ids.Select(id => vc.IsShowingContactWithId(id)).Any(v => v))
                    vc.ClearData();
            }
        }

        #endregion

        protected class DataSource : UITableViewSource, IDisposable
        {
            public bool Empty { get { return !contactPreviewsInView.SelectMany(v => v).Any(); } }

            public IEnumerable<ContactPreview> Items { get { return contactPreviewsInView.SelectMany(i => i); } }

            AbstractContactsListViewController viewController;
            UITableView tableView;
            readonly string emptyText;

            bool loading = true;
            bool disableRowActions;
            List<List<ContactPreview>> contactPreviewsInView = new List<List<ContactPreview>>(25);

            public DataSource(AbstractContactsListViewController viewController, UITableView tableView, string emptyText, bool disableRowActions)
            {
                this.viewController = viewController;
                this.tableView = tableView;
                this.emptyText = emptyText;
                this.disableRowActions = disableRowActions;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as WaitTableViewCell ?? WaitTableViewCell.Create();

                if (!contactPreviewsInView.SelectMany(v => v).Any())
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                    emptyCell.Initialize(emptyText);
                    return emptyCell;
                }

                var cp = contactPreviewsInView[indexPath.Section][indexPath.Row];

                var cell = tableView.DequeueReusableCell(ContactsTableViewCell.Key) as ContactsTableViewCell ?? ContactsTableViewCell.Create();
                cell.Initialize(cp);

                return cell;
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                if (loading)
                    return 1;

                if (!contactPreviewsInView.SelectMany(v => v).Any())
                    return 1;

                return contactPreviewsInView.Count;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading)
                    return 1;

                if (!contactPreviewsInView.SelectMany(v => v).Any())
                    return 1;

                return contactPreviewsInView[(int)section].Count;
            }

            public override string[] SectionIndexTitles(UITableView tableView)
            {
                return contactPreviewsInView.SelectMany(i => i).Select(cp => cp.Name.SafeSubstring(0, 1).ToUpper()).Distinct().ToArray();
            }

            public override nint SectionFor(UITableView tableView, string title, nint atIndex)
            {
                for (var section = 0; section < contactPreviewsInView.Count; section++)
                {
                    var row = contactPreviewsInView[section].FindIndex(cp => cp.Name.SafeSubstring(0, 1).ToUpper() == title);
                    if (row >= 0)
                    {
                        tableView.ScrollToRow(NSIndexPath.FromRowSection(row, section), UITableViewScrollPosition.Top, true);
                        break;
                    }
                }

                return -1;
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return ContactsTableViewCell.Height;
            }

            public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
            {
                return !disableRowActions;
            }

            public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
            {
                var actions = new List<UITableViewRowAction>();

                var contactPreview = contactPreviewsInView[indexPath.Section][indexPath.Row];

                var moreAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("more"), (a, ip) => { viewController.DoShowMoreActionSheet(indexPath, contactPreview); });
                moreAction.BackgroundColor = Theme.DarkerBlue;
                actions.Add(moreAction);

                var copyToWorktrayAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Default,
                    Localization.GetString("copy_to_worktray_ml"),
                    (a, ip) =>
                    {
                        viewController.CopyToWorktray(contactPreview);
                        viewController.EndEditing();
                    });
                copyToWorktrayAction.BackgroundColor = Theme.DarkBlue;
                actions.Add(copyToWorktrayAction);

                return actions.ToArray();
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                if (tableView.Editing)
                    return;

                var cp = contactPreviewsInView[indexPath.Section][indexPath.Row];
                viewController.ContactSelected(tableView, cp, indexPath);
            }

            public void AppendItems(List<ContactPreview> contactPreviews)
            {
                loading = false;

                var count = contactPreviewsInView.Count;
                var isInputListPopulated = contactPreviews.Any();

                if (isInputListPopulated)
                    contactPreviewsInView.Add(contactPreviews);

                if (count == 0)
                    tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                else if (isInputListPopulated)
                    tableView.InsertSections(NSIndexSet.FromIndex(contactPreviewsInView.Count - 1), UITableViewRowAnimation.Fade);
            }

            public void RemoveItems(List<int> contactsId)
            {
                tableView.BeginUpdates();

                var indexPaths = contactsId.Select(id => FindItemIndexPath(id)).Where(idx => idx != null).OrderByDescending(idx => idx.Section).ThenByDescending(idx => idx.Row).ToList();
                foreach (var indexPath in indexPaths)
                {
                    contactPreviewsInView[indexPath.Section].RemoveAt(indexPath.Row);
                    if (!contactPreviewsInView[indexPath.Section].Any())
                    {
                        contactPreviewsInView.RemoveAt(indexPath.Section);
                        if (contactPreviewsInView.Count == 0)
                            tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                        else
                            tableView.DeleteSections(NSIndexSet.FromIndex(indexPath.Section), UITableViewRowAnimation.Automatic);
                    }
                    else
                    {
                        tableView.DeleteRows(new NSIndexPath[]
                            {
                                indexPath
                            },
                            UITableViewRowAnimation.Automatic);
                    }
                }

                tableView.EndUpdates();
            }

            public void Reset()
            {
                loading = true;

                var count = contactPreviewsInView.Count;

                contactPreviewsInView.Clear();

                tableView.BeginUpdates();
                tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);

                if (count > 1)
                    tableView.DeleteSections(NSIndexSet.FromNSRange(new NSRange(1, count - 1)), UITableViewRowAnimation.Fade);

                tableView.EndUpdates();
            }

            public NSIndexPath FindItemIndexPath(ContactPreview cp)
            {
                return FindItemIndexPath(cp.Id);
            }

            public NSIndexPath FindItemIndexPath(int id)
            {
                for (var section = 0; section < contactPreviewsInView.Count; section++)
                    for (var row = 0; row < contactPreviewsInView[section].Count; row++)
                        if (contactPreviewsInView[section][row].Id == id)
                            return NSIndexPath.FromRowSection(row, section);

                return null;
            }

            public ContactPreview FindItemAtIndexPath(NSIndexPath indexPath)
            {
                return contactPreviewsInView[indexPath.Section][indexPath.Row];
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                viewController = null;
                tableView = null;
                contactPreviewsInView = null;
            }
        }
    }
}
