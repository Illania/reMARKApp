using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Model.HubMessages;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentView;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class ShortcodeViewController : AbstractViewController, ISecondaryViewController, IUIViewControllerRestoration
    {
        public bool Modal { get; set; }

        public bool Empty => folderId == null && folder == null && shortcodeId == null && shortcodePreview == null && shortcode == null;

        int? folderId;
        Folder folder;

        int? shortcodeId;
        ShortcodePreview shortcodePreview;
        Shortcode shortcode;

        bool refreshDataOnAppear;

        UIBarButtonItem composeButton;
        UIBarButtonItem doneButtonItem;

        UITableView tableView;
        UIToolbar toolbar;
        UIBarButtonItem fileToButton;

        CancellationTokenSource cts;

        public override void LoadView()
        {
            base.LoadView();

            InitializeView();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            RestorationIdentifier = nameof(ShortcodeViewController);
            RestorationClass = Class;

            ExtendedLayoutIncludesOpaqueBars = true;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            InitializeNavigationBar();
            InitializeNavigationBarTitle();
            InitializeHandlers();
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info("Appeared");

            tableView.ContentInset = new UIEdgeInsets(NavigationController.NavigationBar.Frame.Bottom, 0f, 45f + (TabBarController?.TabBar?.Frame.Height ?? 0f), 0f);
            tableView.ScrollIndicatorInsets = new UIEdgeInsets(NavigationController.NavigationBar.Frame.Bottom, 0f, 45f + (TabBarController?.TabBar?.Frame.Height ?? 0f), 0f);

            if (refreshDataOnAppear)
            {
                refreshDataOnAppear = false;
                RefreshData();
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeinitializeHandlers();
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning("Received memory warning!");

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        public override void ViewWillTransitionToSize(CGSize toSize, IUIViewControllerTransitionCoordinator coordinator)
        {
            base.ViewWillTransitionToSize(toSize, coordinator);

            coordinator.AnimateAlongsideTransition(ctx => { },
                ctx =>
                {
                    if (tableView == null)
                        return;

                    tableView.ContentInset = new UIEdgeInsets(NavigationController.NavigationBar.Frame.Bottom, 0f, 45f + (TabBarController?.TabBar?.Frame.Height ?? 0f), 0f);
                    tableView.ScrollIndicatorInsets = new UIEdgeInsets(NavigationController.NavigationBar.Frame.Bottom, 0f, 45f + (TabBarController?.TabBar?.Frame.Height ?? 0f), 0f);
                });
        }

        void InitializeNavigationBar()
        {
            if (Modal)
            {
                doneButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Done);
                NavigationItem.SetRightBarButtonItem(doneButtonItem, false);
            }
            else
            {
                composeButton = new UIBarButtonItem();
                composeButton.Image = UIImage.FromBundle(Path.Combine("icons", "compose.png"));
                composeButton.Enabled = false;
                NavigationItem.SetRightBarButtonItem(composeButton, false);
            }
        }

        void InitializeView()
        {
            AutomaticallyAdjustsScrollViewInsets = false;

            tableView = new UITableView(CGRect.Empty, UITableViewStyle.Grouped)
            {
                ClipsToBounds = false,
                TranslatesAutoresizingMaskIntoConstraints = false,
                RowHeight = UITableView.AutomaticDimension,
                EstimatedRowHeight = 60f
            };
            tableView.Source = new DataSource(this, tableView);
            tableView.AddGestureRecognizer(new UILongPressGestureRecognizer(RowLongPressed)
            {
                MinimumPressDuration = 1f
            });
            if (NavigationController != null)
            {
                tableView.ContentInset = new UIEdgeInsets(NavigationController.NavigationBar.Frame.Bottom, 0f, 45f + (TabBarController?.TabBar?.Frame.Height ?? 0f), 0f);
                tableView.ScrollIndicatorInsets = new UIEdgeInsets(NavigationController.NavigationBar.Frame.Bottom, 0f, 45f + (TabBarController?.TabBar?.Frame.Height ?? 0f), 0f);
            }
            View.AddSubview(tableView);
            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1f, 0f)
            });

            fileToButton = new UIBarButtonItem();
            fileToButton.Image = UIImage.FromBundle(Path.Combine("icons", "worktray.png"));
            fileToButton.Enabled = false;

            toolbar = new UIToolbar();
            toolbar.BarStyle = UIBarStyle.Default;
            toolbar.Items = new[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                fileToButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace)
            };
            toolbar.BarTintColor = Theme.Gray;
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            toolbar.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            View.AddSubview(toolbar);
            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(toolbar, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 45f),
                NSLayoutConstraint.Create(toolbar, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(toolbar, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(toolbar, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1f, -(TabBarController?.TabBar?.Frame.Height ?? 0f))
            });
        }

        void InitializeNavigationBarTitle()
        {
            NavigationItem.Title = shortcodePreview?.Name;
        }

        void InitializeHandlers()
        {
            if (fileToButton != null)
                fileToButton.Clicked += FileToButton_Clicked;

            if (composeButton != null)
                composeButton.Clicked += ComposeButton_Clicked;

            if (doneButtonItem != null)
                doneButtonItem.Clicked += DoneButtonItem_Clicked;
        }

        void DeinitializeHandlers()
        {
            if (fileToButton != null)
                fileToButton.Clicked -= FileToButton_Clicked;

            if (composeButton != null)
                composeButton.Clicked -= ComposeButton_Clicked;

            if (doneButtonItem != null)
                doneButtonItem.Clicked -= DoneButtonItem_Clicked;
        }

        void RowLongPressed(UILongPressGestureRecognizer gr)
        {
            if (gr.State != UIGestureRecognizerState.Began)
                return;

            var location = gr.LocationInView(tableView);
            var indexPath = tableView?.IndexPathForRowAtPoint(location);
            var cell = tableView?.CellAt(indexPath);
            var dataSource = tableView?.Source as DataSource;
            var da = dataSource?.DocumentAddessAtRow(indexPath);
            if (cell != null && da != null)
                CopyToClipboard(tableView, cell, da.Address);
        }

        public void DocumentAddressClicked(DocumentAddress documentAddress)
        {
            var vc = new ComposeDocumentViewController
            {
                DocumentCreationModeFlag = DocumentCreationModeFlag.New,
                PreconfiguredEmailAddresses = new Dictionary<DocumentAddressType, string[]>
                {
                    { DocumentAddressType.To, new [] { documentAddress.FullAddress } }
                }
            };
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        public void CopyToClipboard(UITableView tableView, UITableViewCell cell, string text)
        {
            Integration.CopyToClipboard(this, tableView, cell, text);
        }

        void FileToButton_Clicked(object sender, EventArgs e)
        {
            var eas = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_worktray"),
                UIAlertActionStyle.Default,
                a =>
                {
                    var vc = new CopyToWorktrayViewController
                    {
                        BusinessEntities = new List<IBusinessEntity>
                        {
                            shortcode
                        }
                    };
                    PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
                }));
            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_folder"),
                UIAlertActionStyle.Default,
                a =>
                {
                    var vc = new CopyMoveToFolderListViewController(new List<IBusinessEntity>
                    {
                        shortcodePreview
                    });
                    PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
                }));

            if (folder?.InternalType == FolderInternalType.FilterView || folder?.InternalType == FolderInternalType.Static || folder?.InternalType == FolderInternalType.Worktray)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("move_to_folder"),
                    UIAlertActionStyle.Default,
                    a =>
                    {
                        var vc = new CopyMoveToFolderListViewController(new List<IBusinessEntity>
                            {
                                shortcodePreview
                            },
                            folder);
                        PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
                    }));

            if (folder?.InternalType == FolderInternalType.FilterView || folder?.InternalType == FolderInternalType.Static || folder?.InternalType == FolderInternalType.Worktray)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete_from_folder"), UIAlertActionStyle.Default, RemoveFromFolder));

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator || ServerConfig.SystemSettings.ShortcodesModuleInfo.Permissions.DeleteAllowed)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete"), UIAlertActionStyle.Destructive, Delete));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, null));

            if (eas.PopoverPresentationController != null)
                eas.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate((UIBarButtonItem)sender);

            PresentViewController(eas, true, null);
        }

        void ComposeButton_Clicked(object sender, EventArgs e)
        {
            var vc = new ComposeDocumentViewController
            {
                DocumentCreationModeFlag = DocumentCreationModeFlag.New,
                PreconfiguredEmailAddresses = new Dictionary<DocumentAddressType, string[]>
                {
                    { DocumentAddressType.To, shortcode.Addresses.Where(da => da.Type == CommunicationAddressType.Email && da.AddressType == DocumentAddressType.To).Select(da => da.FullAddress).ToArray() },
                    { DocumentAddressType.Cc, shortcode.Addresses.Where(da => da.Type == CommunicationAddressType.Email && da.AddressType == DocumentAddressType.Cc).Select(da => da.FullAddress).ToArray() },
                    { DocumentAddressType.Bcc, shortcode.Addresses.Where(da => da.Type == CommunicationAddressType.Email && da.AddressType == DocumentAddressType.Bcc).Select(da => da.FullAddress).ToArray() }
                }
            };
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        void DoneButtonItem_Clicked(object sender, EventArgs e)
        {
            DismissViewController(true, null);
        }

        public void SetData(int folderId, int shortcodeId)
        {
            folder = null;
            shortcodePreview = null;
            shortcode = null;

            this.folderId = folderId;
            this.shortcodeId = shortcodeId;
        }

        public void SetData(Folder folder, ShortcodePreview shortcodePreview)
        {
            folderId = null;
            shortcodeId = null;
            shortcode = null;

            this.folder = folder;
            this.shortcodePreview = shortcodePreview;
        }

        public void SetData(ShortcodePreview shortcodePreview)
        {
            folderId = null;
            folder = null;
            shortcodeId = null;
            shortcode = null;

            this.shortcodePreview = shortcodePreview;
        }

        public void SetData(int shortcodeId)
        {
            folderId = null;
            folder = null;
            shortcode = null;
            shortcodePreview = null;

            this.shortcodeId = shortcodeId;
        }

        public bool IsShowingShortcodeWithId(int shortcodeId)
        {
            return shortcodePreview?.Id == shortcodeId || this.shortcodeId == shortcodeId;
        }

        public async void RefreshData()
        {
            cts?.Cancel();
            cts = new CancellationTokenSource();
            var token = cts.Token;

            var folderId = this.folderId;
            var folder = this.folder;
            var shortcodeId = this.shortcodeId;
            var shortcodePreview = this.shortcodePreview;
            var shortcode = this.shortcode;

            CommonConfig.Logger.Info("Loading shortcode...");

            var ds = (DataSource)tableView?.Source;

            try
            {
                ds.StartRefresh();

                if (folderId != null && shortcodeId != null)
                {
                    var swp = await Managers.ShortcodesManager.GetShortcodeWithPreviewAsync(folderId.Value, shortcodeId.Value);
                    shortcodePreview = swp.ShortcodePreview;
                    shortcode = swp.Shortcode;
                }

                if (folder != null && shortcodePreview != null)
                    shortcode = await Managers.ShortcodesManager.GetShortcodeAsync(folder, shortcodePreview.Id);

                if (folderId == null && folder == null && shortcodePreview != null)
                    shortcode = await Managers.ShortcodesManager.GetShortcodeAsync(-1, shortcodePreview.Id);

                if (folderId == null && folder == null && shortcodePreview == null)
                {
                    var swp = await Managers.ShortcodesManager.GetShortcodeWithPreviewAsync(-1, shortcodeId.Value);
                    shortcodePreview = swp.ShortcodePreview;
                    shortcode = swp.Shortcode;
                }

                this.folderId = folderId;
                this.folder = folder;
                this.shortcodeId = shortcodeId;
                this.shortcodePreview = shortcodePreview;
                this.shortcode = shortcode;

                var description = shortcodePreview?.Description;
                var toAddresses = shortcode?.Addresses?.Where(da => da.AddressType == DocumentAddressType.To).OrderBy(da => da.Name).ThenBy(da => da.FullAddress).ToArray();
                var ccAddresses = shortcode?.Addresses?.Where(da => da.AddressType == DocumentAddressType.Cc).OrderBy(da => da.Name).ThenBy(da => da.FullAddress).ToArray();
                var bccAddresses = shortcode?.Addresses?.Where(da => da.AddressType == DocumentAddressType.Bcc).OrderBy(da => da.Name).ThenBy(da => da.FullAddress).ToArray();

                if (token.IsCancellationRequested)
                    return;

                InitializeNavigationBarTitle();

                if (composeButton != null)
                    composeButton.Enabled = shortcode?.Addresses?.Any() ?? false;

                if (fileToButton != null)
                    fileToButton.Enabled = true;

                ds.EndRefresh(description, toAddresses, ccAddresses, bccAddresses);
            }
            catch (Exception ex)
            {
                if (token.IsCancellationRequested)
                    return;

                CommonConfig.Logger.Error($"Could not load shortcode", ex);

                ds.Clear();

                await Dialogs.ShowErrorDialogAsync(this, ex);

                if (SplitViewController == null)
                    NavigationController.PopViewController(true);
            }
        }

        public void SetRefreshDataOnAppear()
        {
            refreshDataOnAppear = true;
        }

        public void ClearData()
        {
            cts?.Cancel();

            folderId = null;
            folder = null;
            shortcodeId = null;
            shortcodePreview = null;
            shortcode = null;

            InitializeNavigationBarTitle();

            if (composeButton != null)
                composeButton.Enabled = false;

            if (fileToButton != null)
                fileToButton.Enabled = false;

            var ds = tableView?.Source as DataSource;
            ds?.Clear();
        }

        #region Actions

        async void RemoveFromFolder(UIAlertAction a)
        {
            var result = await Dialogs.ShowYesNoDialogAsync(this, Localization.GetString("delete_from_folder"), Localization.GetString("confirm_delete_from_folder_shortcode"));

            if (!result)
                return;

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("deleting_from_folder___"));

            try
            {
                CommonConfig.Logger.Info($"Attempting to remove shortcode from folder [shortcodeId={shortcode.Id}, folderId={folder.Id}]");

                await Managers.CommonActionsManager.RemoveFromFolder(new List<IBusinessEntity>
                    {
                        shortcode
                    },
                    folder);

                CommonConfig.MessengerHub.Publish(new EntityRemovedFromFolderMessage(this,
                    ObjectType.Shortcode,
                    folder.Id,
                    new List<int>
                    {
                        shortcode.Id
                    }));

                dismissAction();

                if (SplitViewController != null && !SplitViewController.Collapsed)
                    ClearData();
                else
                    NavigationController.PopViewController(true);
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Error while removing shortcode from folder [shortcodeId={shortcode.Id}, folderId={folder.Id}]", ex);
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        async void Delete(UIAlertAction a)
        {
            var result = await Dialogs.ShowYesNoDialogAsync(this, Localization.GetString("delete"), Localization.GetString("confirm_delete_shortcode"));

            if (!result)
                return;

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("deleting___"));

            try
            {
                CommonConfig.Logger.Info($"Attempting to delete shortcode [shortcodeId={shortcode.Id}]");

                await Managers.CommonActionsManager.Delete(new List<IBusinessEntity>
                {
                    shortcode
                });

                CommonConfig.MessengerHub.Publish(new EntityDeletedMessage(this,
                    ObjectType.Shortcode,
                    new List<int>
                    {
                        shortcode.Id
                    }));

                dismissAction();

                if (SplitViewController != null && !SplitViewController.Collapsed)
                    ClearData();
                else
                    NavigationController.PopViewController(true);
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Error while deleting shortcode [shortcodeId={shortcode.Id}]", ex);
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        #endregion

        class DataSource : UITableViewSource, IDisposable
        {
            ShortcodeViewController viewController;
            UITableView tableView;

            bool empty = true;
            bool loading = true;

            string description = string.Empty;
            DocumentAddress[] toAddresses = new DocumentAddress[0];
            DocumentAddress[] ccAddresses = new DocumentAddress[0];
            DocumentAddress[] bccAddresses = new DocumentAddress[0];

            public DataSource(ShortcodeViewController viewController, UITableView tableView)
            {
                this.viewController = viewController;
                this.tableView = tableView;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (empty)
                    return null;

                if (loading)
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as WaitTableViewCell ?? WaitTableViewCell.Create();

                if (indexPath.Section == 0)
                {
                    if (string.IsNullOrWhiteSpace(description))
                    {
                        var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                        emptyCell.Initialize(Localization.GetString("no_description"));
                        return emptyCell;
                    }

                    var cell = tableView.DequeueReusableCell(DescriptionTableViewCell.Key) as DescriptionTableViewCell ?? DescriptionTableViewCell.Create();
                    cell.Initialize(description);
                    return cell;
                }

                if (indexPath.Section == 1)
                {
                    if (toAddresses.Length < 1)
                    {
                        var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                        emptyCell.Initialize(Localization.GetString("no_addresses"));
                        return emptyCell;
                    }

                    var address = toAddresses[indexPath.Row];
                    return GetInitializedDocumentAddressCell(address);
                }

                if (indexPath.Section == 2)
                {
                    if (ccAddresses.Length < 1)
                    {
                        var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                        emptyCell.Initialize(Localization.GetString("no_addresses"));
                        return emptyCell;
                    }

                    var address = ccAddresses[indexPath.Row];
                    return GetInitializedDocumentAddressCell(address);
                }

                if (indexPath.Section == 3)
                {
                    if (bccAddresses.Length < 1)
                    {
                        var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                        emptyCell.Initialize(Localization.GetString("no_addresses"));
                        return emptyCell;
                    }

                    var address = bccAddresses[indexPath.Row];
                    return GetInitializedDocumentAddressCell(address);
                }

                return null;
            }

            UITableViewCell GetInitializedDocumentAddressCell(DocumentAddress documentAddress)
            {
                if (string.IsNullOrWhiteSpace(documentAddress.Name))
                {
                    var compactCell = tableView.DequeueReusableCell(DocumentAddressesCompactTableViewCell.Key) as DocumentAddressesCompactTableViewCell ?? DocumentAddressesCompactTableViewCell.Create();
                    compactCell.Initialize(documentAddress);
                    return compactCell;
                }

                var cell = tableView.DequeueReusableCell(DocumentAddressesTableViewCell.Key) as DocumentAddressesTableViewCell ?? DocumentAddressesTableViewCell.Create();
                cell.Initialize(documentAddress);
                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (empty)
                    return 0;

                if (loading)
                    return 1;

                if (section == 0)
                    return 1;

                if (section == 1)
                    return toAddresses.Length > 0 ? toAddresses.Length : 1;

                if (section == 2)
                    return ccAddresses.Length > 0 ? ccAddresses.Length : 1;

                if (section == 3)
                    return bccAddresses.Length > 0 ? bccAddresses.Length : 1;

                return 0;
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                if (empty)
                    return 0;

                if (loading)
                    return 1;

                return 4;
            }

            public override string TitleForHeader(UITableView tableView, nint section)
            {
                if (empty)
                    return string.Empty;
                if (loading)
                    return string.Empty;

                if (section == 0)
                    return Localization.GetString("description");
                if (section == 1)
                    return Localization.GetString("to");
                if (section == 2)
                    return Localization.GetString("cc");
                if (section == 3)
                    return Localization.GetString("bcc");

                return string.Empty;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                if (tableView.CellAt(indexPath).SelectionStyle == UITableViewCellSelectionStyle.None)
                    return;

                DocumentAddress documentAddress = null;

                if (indexPath.Section == 1)
                    documentAddress = toAddresses[indexPath.Row];
                if (indexPath.Section == 2)
                    documentAddress = ccAddresses[indexPath.Row];
                if (indexPath.Section == 3)
                    documentAddress = bccAddresses[indexPath.Row];

                if (documentAddress == null)
                    return;

                viewController.DocumentAddressClicked(documentAddress);

                if (tableView?.IndexPathForSelectedRow != null)
                    tableView.DeselectRow(tableView.IndexPathForSelectedRow, true);
            }

            public void StartRefresh()
            {
                empty = false;
                loading = true;

                tableView.InsertSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            public void EndRefresh(string description, DocumentAddress[] toAddresses, DocumentAddress[] ccAddresses, DocumentAddress[] bccAddresses)
            {
                empty = false;
                loading = false;

                this.description = description;
                this.toAddresses = toAddresses;
                this.ccAddresses = ccAddresses;
                this.bccAddresses = bccAddresses;

                tableView.BeginUpdates();
                tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                tableView.InsertSections(NSIndexSet.FromNSRange(new NSRange(1, 3)), UITableViewRowAnimation.Fade);
                tableView.EndUpdates();
            }

            public void Clear()
            {
                var sections = NumberOfSections(tableView);

                empty = true;
                loading = true;

                description = string.Empty;
                toAddresses = new DocumentAddress[0];
                ccAddresses = new DocumentAddress[0];
                bccAddresses = new DocumentAddress[0];

                tableView.DeleteSections(NSIndexSet.FromNSRange(new NSRange(0, sections)), UITableViewRowAnimation.Fade);
            }

            public DocumentAddress DocumentAddessAtRow(NSIndexPath indexPath)
            {
                DocumentAddress documentAddress = null;

                if (indexPath.Section == 1)
                    documentAddress = toAddresses[indexPath.Row];
                if (indexPath.Section == 2)
                    documentAddress = ccAddresses[indexPath.Row];
                if (indexPath.Section == 3)
                    documentAddress = bccAddresses[indexPath.Row];

                return documentAddress;
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                tableView = null;
                viewController = null;

                description = null;
                toAddresses = null;
                ccAddresses = null;
                bccAddresses = null;
            }
        }

        #region State restoration

        public override void EncodeRestorableState(NSCoder coder)
        {
            base.EncodeRestorableState(coder);

            coder.Encode(Modal, "modal");

            if (folderId.HasValue)
                coder.Encode(folderId.Value, "folderId");
            if (folder != null)
                coder.Encode(Serializer.SerializeToByteArray(folder.ShallowCopy()), "folder");
            if (shortcodeId.HasValue)
                coder.Encode(shortcodeId.Value, "shortcodeId");
            if (shortcodePreview != null)
                coder.Encode(Serializer.SerializeToByteArray(shortcodePreview), "shortcodePreview");
            if (shortcode != null)
                coder.Encode(Serializer.SerializeToByteArray(shortcode), "shortcode");
        }

        public override void DecodeRestorableState(NSCoder coder)
        {
            base.DecodeRestorableState(coder);

            if (coder.ContainsKey("folderId"))
                folderId = coder.DecodeInt("folderId");
            if (folder != null)
                folder = Serializer.DeserializeFromByteArray<Folder>(coder.DecodeBytes("folder"));
            if (coder.ContainsKey("shortcodeId"))
                shortcodeId = coder.DecodeInt("shortcodeId");
            if (coder.ContainsKey("shortcodePreview"))
                shortcodePreview = Serializer.DeserializeFromByteArray<ShortcodePreview>(coder.DecodeBytes("shortcodePreview"));
            if (coder.ContainsKey("shortcode"))
                shortcode = Serializer.DeserializeFromByteArray<Shortcode>(coder.DecodeBytes("shortcode"));

            refreshDataOnAppear = true;
        }

        [Export("viewControllerWithRestorationIdentifierPath:coder:")]
        public static UIViewController Restore(string[] identifierComponents, NSCoder coder)
        {
            if (coder.DecodeBool("modal"))
                return null;

            return new ShortcodeViewController();
        }

        #endregion

    }
}