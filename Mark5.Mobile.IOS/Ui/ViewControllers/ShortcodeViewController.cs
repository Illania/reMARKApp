using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.HubMessages;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentView;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;
using Mark5.Mobile.IOS.Utilities;
using Mark5.Mobile.IOS.Utilities.Extensions;
using TinyMessenger;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class ShortcodeViewController : AbstractTableViewController, ISecondaryViewController, IUIViewControllerRestoration
    {
        public bool Empty => folderId == null && folder == null && shortcodeId == null && shortcodePreview == null && shortcode == null;

        int? folderId;
        Folder folder;

        int? shortcodeId;
        ShortcodePreview shortcodePreview;
        Shortcode shortcode;

        bool refreshDataOnAppear;

        UIView headerView;
        UILabel nameLabel;
        UIButton button1;

        UIBarButtonItem fileToButton;
        UIBarButtonItem doneButtonItem;
        UIBarButtonItem editButtonItem;

        CancellationTokenSource cts;

        TinyMessageSubscriptionToken shortcodeChangedToken;

        public ShortcodeViewController()
            : base(UITableViewStyle.Grouped)
        {
        }

        public override void LoadView()
        {
            base.LoadView();

            InitializeView();
            InitializeNavigationBar();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            RestorationIdentifier = nameof(ShortcodeViewController);
            RestorationClass = Class;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (Integration.IsRunningAtLeast(11))
            {
                if (NavigationController != null)
                    NavigationController.NavigationBar.PrefersLargeTitles = true;
                NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Never;
            }

            InitializeHandlers();
            SubscribeToMessages();
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info("Appeared");

            if (refreshDataOnAppear)
            {
                refreshDataOnAppear = false;
                RefreshData();
            }

            if (NavigationController != null)
                NavigationController.ToolbarHidden = false;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeinitializeHandlers();

            if (NavigationController != null)
                NavigationController.ToolbarHidden = true;
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning("Received memory warning!");

            UnsubscribeFromMessages();

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        protected override void Recycle()
        {
            base.Recycle();

            doneButtonItem = null;
            fileToButton = null;
            editButtonItem = null;

            TableView.GestureRecognizers.ForEach(TableView.RemoveGestureRecognizer);
            ((DataSource)TableView.Source)?.Clear();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (CommonConfig.Logger.IsDebugEnabled())
                CommonConfig.Logger.Debug("Disposed");
        }

        void InitializeView()
        {
            TableView.Source = new DataSource(this, TableView);
            TableView.BackgroundColor = Theme.White;
            TableView.AddGestureRecognizer(new UILongPressGestureRecognizer(RowLongPressed));

            headerView = new UIView(new CGRect(0f, 0f, 0f, 160f))
            {
                BackgroundColor = Theme.White
            };

            nameLabel = new UILabel
            {
                Font = Theme.DefaultFont.WithRelativeSize(6f),
                TextColor = Theme.DarkerBlue,
                TextAlignment = UITextAlignment.Center,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            headerView.AddSubview(nameLabel);
            headerView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(nameLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, headerView, NSLayoutAttribute.Top, 1f, 10f),
                NSLayoutConstraint.Create(nameLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, headerView, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(nameLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, headerView, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(nameLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 25f)
            });

            var buttonsView = new UIView
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            headerView.AddSubview(buttonsView);
            headerView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(buttonsView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, nameLabel, NSLayoutAttribute.Bottom, 1f, 35f),
                NSLayoutConstraint.Create(buttonsView, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, headerView, NSLayoutAttribute.CenterX, 1f, 0f)
            });

            button1 = new UIButton
            {
                Enabled = false,
                Alpha = 0f,
                TintColor = Theme.DarkerBlue,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            button1.SetImage(UIImage.FromBundle(Path.Combine("icons", "large_email.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
            buttonsView.AddSubview(button1);
            buttonsView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(button1, NSLayoutAttribute.Top, NSLayoutRelation.Equal, buttonsView, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(button1, NSLayoutAttribute.Left, NSLayoutRelation.Equal, buttonsView, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(button1, NSLayoutAttribute.Right, NSLayoutRelation.Equal, buttonsView, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(button1, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, buttonsView, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(button1, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 60),
                NSLayoutConstraint.Create(button1, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, 40f)
            });

            TableView.TableHeaderView = headerView;

            ToolbarItems = new[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                fileToButton = new UIBarButtonItem
                {
                    Image = UIImage.FromBundle(Path.Combine("icons", "worktray.png")),
                    Enabled = false
                },
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace)
            };
        }

        void InitializeNavigationBar()
        {
            if (PresentingViewController == null)
            {
                if (ServerConfig.SystemSettings.ShortcodesModuleInfo.Permissions.CreateAllowed || ServerConfig.SystemSettings.ShortcodesModuleInfo.Permissions.EditAllowed)
                {
                    editButtonItem = new UIBarButtonItem
                    {
                        Image = UIImage.FromBundle(Path.Combine("icons", "pencil")),
                        Enabled = false
                    };
                    NavigationItem.SetRightBarButtonItem(editButtonItem, false);
                }
            }
            else
            {
                doneButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Done);
                NavigationItem.SetRightBarButtonItem(doneButtonItem, false);
            }
        }

        void InitializeHandlers()
        {
            if (button1 != null)
                button1.TouchUpInside += Button1_TouchUpInside;

            if (fileToButton != null)
                fileToButton.Clicked += FileToButton_Clicked;

            if (doneButtonItem != null)
                doneButtonItem.Clicked += DoneButtonItem_Clicked;

            if (editButtonItem != null)
                editButtonItem.Clicked += EditButtonItem_Clicked;
        }

        void DeinitializeHandlers()
        {
            if (button1 != null)
                button1.TouchUpInside -= Button1_TouchUpInside;

            if (fileToButton != null)
                fileToButton.Clicked -= FileToButton_Clicked;

            if (doneButtonItem != null)
                doneButtonItem.Clicked -= DoneButtonItem_Clicked;

            if (editButtonItem != null)
                editButtonItem.Clicked -= EditButtonItem_Clicked;
        }

        void SubscribeToMessages()
        {
            shortcodeChangedToken = CommonConfig.MessengerHub.Subscribe<EntityPreviewChangedMessage>(HandleShortcodeChangedMessage, arg => arg.EntityPreview.ObjectType == ObjectType.Shortcode && arg.EntityPreview.Id == shortcodePreview?.Id);
        }

        void UnsubscribeFromMessages()
        {
            shortcodeChangedToken?.Dispose();
        }

        void HandleShortcodeChangedMessage(EntityPreviewChangedMessage obj) => RefreshAllOnAppear();

        void RefreshAllOnAppear()
        {
            ((DataSource)TableView.Source)?.Clear();
            shortcodeId = shortcodePreview.Id;
            shortcodePreview = null;

            if (SplitViewController == null || SplitViewController.Collapsed)
                refreshDataOnAppear = true;
            else
                RefreshData();
        }

        void RowLongPressed(UILongPressGestureRecognizer gr)
        {
            if (gr.State != UIGestureRecognizerState.Began)
                return;

            var location = gr.LocationInView(TableView);
            var indexPath = TableView?.IndexPathForRowAtPoint(location);

            if (indexPath == null)
                return;

            var cell = TableView?.CellAt(indexPath);
            var dataSource = TableView?.Source as DataSource;
            var row = dataSource?.RowAt(indexPath);
            if (cell != null && row != null)
                row.OnLongClicked(this.Wrap(), TableView, cell, indexPath);
        }

        void Button1_TouchUpInside(object sender, EventArgs e)
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

        void EditButtonItem_Clicked(object sender, EventArgs e)
        {
            var vc = new AddEditShortcodeViewController
            {
                ShortcodePreview = shortcodePreview,
                Shortcode = shortcode,
                CreationModeFlag = ShortcodeCreationModeFlag.Edit
            };
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        void FileToButton_Clicked(object sender, EventArgs e)
        {
            var eas = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_worktray"),
                                               UIAlertActionStyle.Default,
                                               a =>
            {
                var vc = new CopyToWorktrayViewController { BusinessEntities = new List<IBusinessEntity> { shortcode } };
                PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
            }));
            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_folder"),
                                               UIAlertActionStyle.Default,
                                               a =>
            {
                var vc = new CopyMoveToFolderListViewController(ModuleType.Shortcodes, new List<IBusinessEntity> { shortcodePreview });
                PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
            }));

            if (folder?.InternalType == FolderInternalType.FilterView || folder?.InternalType == FolderInternalType.Static || folder?.InternalType == FolderInternalType.Worktray)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("move_to_folder"),
                                                   UIAlertActionStyle.Default,
                                                   a =>
            {
                var vc = new CopyMoveToFolderListViewController(ModuleType.Shortcodes, new List<IBusinessEntity> { shortcodePreview }, folder);
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

        void DoneButtonItem_Clicked(object sender, EventArgs e) => DismissViewController(true, null);

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

        public void CopyToClipboard(UITableView tableView, UITableViewCell cell, string text) => Integration.CopyToClipboard(this, tableView, cell, text);

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

        public bool IsShowingShortcodeWithId(int shortcodeId) => shortcodePreview?.Id == shortcodeId || this.shortcodeId == shortcodeId;

        public async void RefreshData()
        {
            cts?.Cancel();
            cts = new CancellationTokenSource();
            var token = cts.Token;

            var folderId = this.folderId;
            var folder = this.folder;
            var shortcodeId = this.shortcodeId;
            var shortcodePreview = this.shortcodePreview;

            CommonConfig.Logger.Info("Loading shortcode...");

            var ds = (DataSource)TableView.Source;

            try
            {
                ds.StartRefresh();

                if ((folderId != null || folder != null) && shortcodeId != null)
                {
                    var swp = await Managers.ShortcodesManager.GetShortcodeWithPreviewAsync(folder?.Id ?? folderId.Value, shortcodeId.Value);
                    this.shortcodePreview = swp.ShortcodePreview;
                    shortcode = swp.Shortcode;
                }

                if (folder != null && shortcodePreview != null)
                    shortcode = await Managers.ShortcodesManager.GetShortcodeAsync(folder, shortcodePreview.Id);

                if (folderId == null && folder == null && shortcodePreview != null)
                    shortcode = await Managers.ShortcodesManager.GetShortcodeAsync(-1, shortcodePreview.Id);

                if (folderId == null && folder == null && shortcodePreview == null)
                {
                    var swp = await Managers.ShortcodesManager.GetShortcodeWithPreviewAsync(-1, shortcodeId.Value);
                    this.shortcodePreview = swp.ShortcodePreview;
                    shortcode = swp.Shortcode;
                }

                if (token.IsCancellationRequested)
                    return;

                nameLabel.Text = this.shortcodePreview.Name;

                button1.Enabled = shortcode.Addresses.Any();
                button1.Alpha = 1f;

                if (fileToButton != null)
                    fileToButton.Enabled = true;

                if (editButtonItem != null)
                {
                    editButtonItem.Enabled = true;
                    NavigationItem.SetRightBarButtonItem(editButtonItem, false);
                }

                if (doneButtonItem != null)
                    NavigationItem.SetRightBarButtonItem(doneButtonItem, false);

                ds.EndRefresh(this.shortcodePreview, shortcode);
            }
            catch (Exception ex)
            {
                if (token.IsCancellationRequested)
                    return;

                CommonConfig.Logger.Error($"Could not load shortcode", ex);

                ds.Clear();

                await Dialogs.ShowErrorAlertAsync(this, ex);

                if (SplitViewController == null || SplitViewController.Collapsed)
                {
                    if (PresentingViewController == null)
                        NavigationController?.PopViewController(true);
                    else
                        DismissViewController(true, null);
                }
            }
        }

        public void SetRefreshDataOnAppear() => refreshDataOnAppear = true;

        public void ClearData()
        {
            cts?.Cancel();

            folderId = null;
            folder = null;
            shortcodeId = null;
            shortcodePreview = null;
            shortcode = null;

            nameLabel.Text = string.Empty;

            button1.Alpha = 0f;
            button1.Enabled = false;

            if (fileToButton != null)
                fileToButton.Enabled = false;

            NavigationItem.SetRightBarButtonItem(null, false);

            ((DataSource)TableView.Source)?.Clear();
        }

        async void RemoveFromFolder(UIAlertAction a)
        {
            var d = new PopoverPresentationControllerDelegate(fileToButton);
            var result = await Dialogs.ShowDestructiveActionSheetAsync(this, Localization.GetString("delete_from_folder"), d);
            if (!result)
                return;

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("deleting_from_folder___"));

            try
            {
                CommonConfig.Logger.Info($"Attempting to remove shortcode from folder [shortcodeId={shortcode.Id}, folderId={folder.Id}]");

                await Managers.CommonActionsManager.RemoveFromFolder(new List<IBusinessEntity> { shortcode }, folder);

                dismissAction();

                if (SplitViewController != null && !SplitViewController.Collapsed)
                    ClearData();
                else
                    NavigationController?.PopViewController(true);
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Error while removing shortcode from folder [shortcodeId={shortcode.Id}, folderId={folder.Id}]", ex);
                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        async void Delete(UIAlertAction a)
        {
            var d = new PopoverPresentationControllerDelegate(fileToButton);
            var result = await Dialogs.ShowDestructiveActionSheetAsync(this, Localization.GetString("delete"), d);
            if (!result)
                return;

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("deleting___"));

            try
            {
                CommonConfig.Logger.Info($"Attempting to delete shortcode [shortcodeId={shortcode.Id}]");

                await Managers.CommonActionsManager.Delete(new List<IBusinessEntity> { shortcode });

                dismissAction();

                if (SplitViewController != null && !SplitViewController.Collapsed)
                    ClearData();
                else
                    NavigationController?.PopViewController(true);
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Error while deleting shortcode [shortcodeId={shortcode.Id}]", ex);
                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        void UpdateTitle(bool show)
        {
            var title = show ? nameLabel.Text : null;
            NavigationItem.Title = title;
        }

        class DataSource : UITableViewSource
        {
            readonly WeakReference<ShortcodeViewController> viewControllerWeakReference;
            readonly WeakReference<UITableView> tableViewWeakReference;

            bool empty = true;
            bool loading = true;

            readonly SectionCollection sections = new SectionCollection();

            public DataSource(ShortcodeViewController viewController, UITableView tableView)
            {
                viewControllerWeakReference = viewController.Wrap();
                tableViewWeakReference = tableView.Wrap();
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (empty)
                    return null;

                if (loading)
                    return tableView.DequeueReusableCell(WaitTableViewCell.DefaultId) as WaitTableViewCell ?? new WaitTableViewCell();

                var row = sections[indexPath.Section].Rows[indexPath.Row];
                var cell = tableView.DequeueReusableCell(row.Id) ?? row.CreateCell();
                row.Bind(cell);
                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (empty)
                    return 0;

                if (loading)
                    return 1;

                return sections[(int)section].Rows.Count;
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                if (empty)
                    return 0;

                if (loading)
                    return 1;

                return sections.Count;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.CellAt(indexPath);
                if (cell.SelectionStyle == UITableViewCellSelectionStyle.None)
                    return;

                sections[indexPath.Section].Rows[indexPath.Row].OnClicked(viewControllerWeakReference, tableView, cell, indexPath);

                if (tableView?.IndexPathForSelectedRow != null)
                    tableView.DeselectRow(tableView.IndexPathForSelectedRow, true);
            }

            public override void Scrolled(UIScrollView scrollView) => ScrollChanged(scrollView);
            public override void DraggingStarted(UIScrollView scrollView) => ScrollChanged(scrollView);
            public override void DraggingEnded(UIScrollView scrollView, bool willDecelerate) => ScrollChanged(scrollView);
            public override void DecelerationEnded(UIScrollView scrollView) => ScrollChanged(scrollView);

            void ScrollChanged(UIScrollView scrollView)
            {
                var offset = scrollView.ContentOffset.Y;
                var inset = -scrollView.SafeAreaInsets.Top;
                var show = offset > inset + 30;
                viewControllerWeakReference.Unwrap()?.UpdateTitle(show);
            }

            public void StartRefresh()
            {
                empty = false;
                loading = true;

                tableViewWeakReference.Unwrap()?.InsertSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            public void EndRefresh(ShortcodePreview shortcodePreview, Shortcode shortcode)
            {
                var allSections = new AbstractSection[]
                {
                    new DescriptionSection(),
                    new DocumentAddressSection()
                };

                foreach (var section in allSections)
                {
                    section.ShortcodePreview = shortcodePreview;
                    section.Shortcode = shortcode;

                    if (!section.Empty)
                    {
                        section.InitializeRows();
                        sections.Add(section);
                    }
                }

                allSections = null;

                if (sections.Count < 1)
                {
                    empty = true;
                    loading = false;

                    tableViewWeakReference.Unwrap()?.DeleteSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                }
                else if (sections.Count == 1)
                {
                    empty = false;
                    loading = false;

                    tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                }
                else if (sections.Count > 1)
                {
                    empty = false;
                    loading = false;

                    tableViewWeakReference.Unwrap()?.BeginUpdates();
                    tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                    tableViewWeakReference.Unwrap()?.InsertSections(NSIndexSet.FromNSRange(new NSRange(1, sections.Count - 1)), UITableViewRowAnimation.Fade);
                    tableViewWeakReference.Unwrap()?.EndUpdates();
                }
            }

            public void Clear()
            {
                var numberOfSections = tableViewWeakReference.Unwrap()?.NumberOfSections() ?? 0;

                empty = true;
                loading = true;

                sections.Clear();

                tableViewWeakReference.Unwrap()?.BeginUpdates();
                tableViewWeakReference.Unwrap()?.DeleteSections(NSIndexSet.FromNSRange(new NSRange(0, numberOfSections)), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.EndUpdates();
            }

            public AbstractRow RowAt(NSIndexPath indexPath) => sections[indexPath.Section].Rows[indexPath.Row];

            public class SectionCollection : List<AbstractSection>
            {
            }

            public abstract class AbstractSection
            {
                WeakReference<ShortcodePreview> weakShortcodePreview;
                WeakReference<Shortcode> weakShortcode;

                public ShortcodePreview ShortcodePreview
                {
                    get => weakShortcodePreview.Unwrap();
                    set => weakShortcodePreview = value.Wrap();
                }

                public Shortcode Shortcode
                {
                    get => weakShortcode.Unwrap();
                    set => weakShortcode = value.Wrap();
                }

                public abstract bool Empty { get; }
                public RowCollection Rows { get; } = new RowCollection();
                public abstract void InitializeRows();
            }

            public class DescriptionSection : AbstractSection
            {
                public override bool Empty => string.IsNullOrWhiteSpace(ShortcodePreview?.Description);

                public override void InitializeRows()
                {
                    Rows.Add(new DescriptionRow(ShortcodePreview, Shortcode));
                }
            }

            public class DocumentAddressSection : AbstractSection
            {
                public override bool Empty => !Shortcode?.Addresses?.Any() ?? true;

                public override void InitializeRows()
                {
                    var toAddresses = Shortcode.Addresses?.Where(da => da.AddressType == DocumentAddressType.To).OrderBy(da => da.Name).ThenBy(da => da.FullAddress).ToArray();
                    foreach (var a in toAddresses)
                        Rows.Add(new DocumentAddressRow(ShortcodePreview, Shortcode, a));

                    var ccAddresses = Shortcode.Addresses?.Where(da => da.AddressType == DocumentAddressType.Cc).OrderBy(da => da.Name).ThenBy(da => da.FullAddress).ToArray();
                    foreach (var a in ccAddresses)
                        Rows.Add(new DocumentAddressRow(ShortcodePreview, Shortcode, a));

                    var bccAddresses = Shortcode.Addresses?.Where(da => da.AddressType == DocumentAddressType.Bcc).OrderBy(da => da.Name).ThenBy(da => da.FullAddress).ToArray();
                    foreach (var a in bccAddresses)
                        Rows.Add(new DocumentAddressRow(ShortcodePreview, Shortcode, a));
                }
            }

            public class RowCollection : List<AbstractRow>
            {
            }

            public abstract class AbstractRow
            {
                WeakReference<ShortcodePreview> weakShortcodePreview;
                WeakReference<Shortcode> weakShortcode;

                public ShortcodePreview ShortcodePreview
                {
                    get => weakShortcodePreview.Unwrap();
                    set => weakShortcodePreview = value.Wrap();
                }

                public Shortcode Shortcode
                {
                    get => weakShortcode.Unwrap();
                    set => weakShortcode = value.Wrap();
                }

                protected AbstractRow(ShortcodePreview shortcodePreview, Shortcode shortcode)
                {
                    ShortcodePreview = shortcodePreview;
                    Shortcode = shortcode;
                }

                public virtual string Id => ShortcodeInfoTableViewCell.DefaultId;
                public virtual UITableViewCell CreateCell() => new ShortcodeInfoTableViewCell();
                public abstract void Bind(UITableViewCell cell);
                public virtual void OnClicked(WeakReference<ShortcodeViewController> viewControllerWeakReference, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath) { }
                public virtual void OnLongClicked(WeakReference<ShortcodeViewController> viewControllerWeakReference, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath) { }
            }

            public class DescriptionRow : AbstractRow
            {
                public DescriptionRow(ShortcodePreview shortcodePreview, Shortcode shortcode)
                    : base(shortcodePreview, shortcode)
                {
                }

                public override string Id { get => base.Id + "_Description"; }

                public override void Bind(UITableViewCell cell)
                {
                    var cic = (ShortcodeInfoTableViewCell)cell;
                    cic.Accessory = UITableViewCellAccessory.None;
                    cic.SelectionStyle = UITableViewCellSelectionStyle.Default;
                    cic.Initialize(Localization.GetString("description").ToUpper(), ShortcodePreview.Description, true);
                }

                public override void OnLongClicked(WeakReference<ShortcodeViewController> viewControllerWeakReference, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewControllerWeakReference.Unwrap()?.CopyToClipboard(tableView, cell, ShortcodePreview.Description);
                }
            }

            public class DocumentAddressRow : AbstractRow
            {
                readonly WeakReference<DocumentAddress> weakDocumentAddress;
                readonly bool compact;

                public DocumentAddressRow(ShortcodePreview shortcodePreview, Shortcode shortcode, DocumentAddress documentAddress)
                    : base(shortcodePreview, shortcode)
                {
                    weakDocumentAddress = documentAddress.Wrap();
                    compact = string.IsNullOrWhiteSpace(documentAddress?.FullAttention);
                }

                public override string Id
                {
                    get
                    {
                        if (compact)
                            return DocumentAddressTableViewCell.CompactId;

                        return DocumentAddressTableViewCell.DefaultId;
                    }
                }

                public override UITableViewCell CreateCell()
                {
                    if (compact)
                        return new DocumentAddressTableViewCell(DocumentAddressTableViewCell.CompactId);

                    return new DocumentAddressTableViewCell(DocumentAddressTableViewCell.DefaultId);
                }

                public override void Bind(UITableViewCell cell)
                {
                    var da = weakDocumentAddress.Unwrap();
                    var c = (DocumentAddressTableViewCell)cell;
                    c.Initialize(da);
                }

                public override void OnClicked(WeakReference<ShortcodeViewController> viewControllerWeakReference, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewControllerWeakReference.Unwrap()?.DocumentAddressClicked(weakDocumentAddress.Unwrap());
                }

                public override void OnLongClicked(WeakReference<ShortcodeViewController> viewControllerWeakReference, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewControllerWeakReference.Unwrap()?.CopyToClipboard(tableView, cell, cell.TextLabel.Text);
                }
            }
        }

        public override void EncodeRestorableState(NSCoder coder)
        {
            base.EncodeRestorableState(coder);

            coder.Encode(PresentingViewController != null, "doNotRestore");

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
            if (coder.DecodeBool("doNotRestore"))
                return null;

            return new ShortcodeViewController();
        }
    }
}