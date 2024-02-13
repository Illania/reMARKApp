using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CoreGraphics;
using Foundation;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Extensions;
using reMark.Mobile.Common.Manager;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Model.HubMessages;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.Common.Utilities.Extensions;
using reMark.Mobile.IOS.Ui.Common;
using reMark.Mobile.IOS.Ui.TableViewCells;
using reMark.Mobile.IOS.Utilities;
using reMark.Mobile.IOS.Utilities.Extensions;
using TinyMessenger;
using UIKit;
using Contact = reMark.Mobile.Common.Model.Contact;

namespace reMark.Mobile.IOS.Ui.ViewControllers
{
    public abstract class AbstractContactViewController : AbstractTableViewController, ISecondaryViewController, IUIViewControllerRestoration
    {
        public bool Empty => FolderId == null && Folder == null && ContactId == null && ContactPreview == null && Contact == null;

        protected int? FolderId;
        protected Folder Folder;

        protected int? ContactId;
        protected ContactPreview ContactPreview;
        protected Contact Contact;

        protected bool HideDoneButton;

        protected UIBarButtonItem FileToButton;
        protected UIBarButtonItem ActionsLinksButton;
        protected UIBarButtonItem EditButtonItem;
        protected UIButton EmailButton;
        protected UIButton MobileButton;
        protected UIButton SmsButton;
        protected UIButton MapButton;

        bool refreshDataOnAppear;

        UIView headerView;
        UILabel nameLabel;
        UILabel nameSubLabel;

        UIBarButtonItem assignCategoryButton;
        UIBarButtonItem commentsButton;
        UIBarButtonItem doneButtonItem;

        CancellationTokenSource cts;

        TinyMessageSubscriptionToken contactChangedToken;

        protected abstract void EmailButton_TouchUpInside(object sender, EventArgs e);
        protected abstract void MobileButton_TouchUpInside(object sender, EventArgs e);
        protected abstract void SmsButton_TouchUpInside(object sender, EventArgs e);
        protected abstract void MapButton_TouchUpInside(object sender, EventArgs e);
        protected abstract void EditButtonItem_Clicked(object sender, EventArgs e);
        protected abstract void AssignCategoryButton_Clicked(object sender, EventArgs e);
        protected abstract void FileToButton_Clicked(object sender, EventArgs e);
        protected abstract void CommentsButton_Clicked(object sender, EventArgs e);
        protected abstract void ActionsLinksButton_Clicked(object sender, EventArgs e);
        protected abstract void DoneButtonItem_Clicked(object sender, EventArgs e);
        protected abstract void CommunicationAddressClicked(UITableView tv, UITableViewCell cell, CommunicationAddress ca);
        protected abstract void LinkedContactClicked(ContactPreview contactPreview);
        protected abstract void PhysicalAddressClicked(UITableView tv, UITableViewCell cell, PhysicalAddress pa);

        public AbstractContactViewController()
            : base(UITableViewStyle.Grouped)
        {
            HidesBottomBarWhenPushed = true;
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

            RestorationIdentifier = nameof(AbstractContactViewController);
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

            if (NavigationController != null)
                NavigationController.ToolbarHidden = Integration.IsIPad();
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
            FileToButton = null;
            EditButtonItem = null;

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
            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 40f;
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
                nameLabel.TopAnchor.ConstraintEqualTo(headerView.TopAnchor,10f),
                nameLabel.LeftAnchor.ConstraintEqualTo(headerView.LeftAnchor),
                nameLabel.RightAnchor.ConstraintEqualTo(headerView.RightAnchor),
                nameLabel.HeightAnchor.ConstraintEqualTo(25f)
            });

            nameSubLabel = new UILabel
            {
                Font = Theme.DefaultFont.WithRelativeSize(-2f),
                TextColor = Theme.DarkerBlue,
                TextAlignment = UITextAlignment.Center,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            headerView.AddSubview(nameSubLabel);
            headerView.AddConstraints(new[]
            {
                nameSubLabel.TopAnchor.ConstraintEqualTo(nameLabel.BottomAnchor,5f),
                nameSubLabel.LeftAnchor.ConstraintEqualTo(headerView.LeftAnchor),
                nameSubLabel.RightAnchor.ConstraintEqualTo(headerView.RightAnchor),
                nameSubLabel.HeightAnchor.ConstraintEqualTo(20f)
            });

            var buttonsView = new UIView
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            headerView.AddSubview(buttonsView);
            headerView.AddConstraints(new[]
            {
                buttonsView.TopAnchor.ConstraintEqualTo(nameSubLabel.BottomAnchor,10f),
                buttonsView.CenterXAnchor.ConstraintEqualTo(headerView.CenterXAnchor)
            });

            EmailButton = new UIButton
            {
                Enabled = false,
                Alpha = 0f,
                TintColor = Theme.DarkerBlue,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            EmailButton.SetImage(UIImage.FromBundle("Email-Large").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
            buttonsView.AddSubview(EmailButton);
            buttonsView.AddConstraints(new[]
            {
                EmailButton.TopAnchor.ConstraintEqualTo(buttonsView.TopAnchor),
                EmailButton.LeftAnchor.ConstraintEqualTo(buttonsView.LeftAnchor),
                EmailButton.BottomAnchor.ConstraintEqualTo(buttonsView.BottomAnchor),
                EmailButton.HeightAnchor.ConstraintEqualTo(60f),
                EmailButton.WidthAnchor.ConstraintEqualTo(40f)
            });

            MobileButton = new UIButton
            {
                Enabled = false,
                Alpha = 0f,
                TintColor = Theme.DarkerBlue,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            MobileButton.SetImage(UIImage.FromBundle("Mobile-Large").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
            buttonsView.AddSubview(MobileButton);
            buttonsView.AddConstraints(new[]
            {
                MobileButton.TopAnchor.ConstraintEqualTo(buttonsView.TopAnchor),
                MobileButton.LeftAnchor.ConstraintEqualTo(EmailButton.RightAnchor,15f),
                MobileButton.BottomAnchor.ConstraintEqualTo(buttonsView.BottomAnchor),
                MobileButton.HeightAnchor.ConstraintEqualTo(60f),
                MobileButton.WidthAnchor.ConstraintEqualTo(40f)
            });

            SmsButton = new UIButton
            {
                Enabled = false,
                Alpha = 0f,
                TintColor = Theme.DarkerBlue,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            SmsButton.SetImage(UIImage.FromBundle("Sms-Large").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
            buttonsView.AddSubview(SmsButton);
            buttonsView.AddConstraints(new[]
            {
                SmsButton.TopAnchor.ConstraintEqualTo(buttonsView.TopAnchor),
                SmsButton.LeftAnchor.ConstraintEqualTo(MobileButton.RightAnchor,15f),
                SmsButton.BottomAnchor.ConstraintEqualTo(buttonsView.BottomAnchor),
                SmsButton.HeightAnchor.ConstraintEqualTo(60f),
                SmsButton.WidthAnchor.ConstraintEqualTo(40f)
            });

            MapButton = new UIButton
            {
                Enabled = false,
                Alpha = 0f,
                TintColor = Theme.DarkerBlue,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            MapButton.SetImage(UIImage.FromBundle("Map-Large").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
            buttonsView.AddSubview(MapButton);
            buttonsView.AddConstraints(new[]
            {
                MapButton.TopAnchor.ConstraintEqualTo(buttonsView.TopAnchor),
                MapButton.LeftAnchor.ConstraintEqualTo(SmsButton.RightAnchor,15f),
                MapButton.RightAnchor.ConstraintEqualTo(buttonsView.RightAnchor),
                MapButton.BottomAnchor.ConstraintEqualTo(buttonsView.BottomAnchor),
                MapButton.HeightAnchor.ConstraintEqualTo(60f),
                MapButton.WidthAnchor.ConstraintEqualTo(40f)
            });

            TableView.TableHeaderView = headerView;

            var buttons = new[]
                {
                    assignCategoryButton = new UIBarButtonItem
                    {
                        Image = UIImage.FromBundle("Flag"),
                        Enabled = false
                    },
                    new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                    FileToButton = new UIBarButtonItem
                    {
                        Image = UIImage.FromBundle("Worktray"),
                        Enabled = false
                    },
                    new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                    commentsButton = new UIBarButtonItem
                    {
                        Image = UIImage.FromBundle("Comments"),
                        Enabled = false
                    },
                    new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                    ActionsLinksButton = new UIBarButtonItem
                    {
                        Image = UIImage.FromBundle("Actions"),
                        Enabled = false
                    }
                };

            if (!Integration.IsIPad())
            {
                ToolbarItems = buttons;
            }
            else
            {
                NavigationController.ToolbarHidden = true;
                NavigationItem.SetLeftBarButtonItems(buttons, false);
            }
        }

        void InitializeNavigationBar()
        {
            if (PresentingViewController == null)
            {
                if (ServerConfig.SystemSettings.ContactsModuleInfo.Permissions.CreateAllowed || ServerConfig.SystemSettings.ContactsModuleInfo.Permissions.EditAllowed)
                {
                    EditButtonItem = new UIBarButtonItem
                    {
                        Image = UIImage.FromBundle("Edit"),
                        Enabled = false
                    };
                    NavigationItem.SetRightBarButtonItem(EditButtonItem, false);
                }
            }
            else if (!HideDoneButton)
            {
                doneButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Done);
                NavigationItem.SetRightBarButtonItem(doneButtonItem, false);
            }
        }

        void InitializeHandlers()
        {
            if (EmailButton != null)
                EmailButton.TouchUpInside += EmailButton_TouchUpInside;

            if (MobileButton != null)
                MobileButton.TouchUpInside += MobileButton_TouchUpInside;

            if (SmsButton != null)
                SmsButton.TouchUpInside += SmsButton_TouchUpInside;

            if (MapButton != null)
                MapButton.TouchUpInside += MapButton_TouchUpInside;

            if (assignCategoryButton != null)
                assignCategoryButton.Clicked += AssignCategoryButton_Clicked;

            if (FileToButton != null)
                FileToButton.Clicked += FileToButton_Clicked;

            if (commentsButton != null)
                commentsButton.Clicked += CommentsButton_Clicked;

            if (ActionsLinksButton != null)
                ActionsLinksButton.Clicked += ActionsLinksButton_Clicked;

            if (doneButtonItem != null)
                doneButtonItem.Clicked += DoneButtonItem_Clicked;

            if (EditButtonItem != null)
                EditButtonItem.Clicked += EditButtonItem_Clicked;
        }

        void DeinitializeHandlers()
        {
            if (EmailButton != null)
                EmailButton.TouchUpInside -= EmailButton_TouchUpInside;

            if (MobileButton != null)
                MobileButton.TouchUpInside -= MobileButton_TouchUpInside;

            if (SmsButton != null)
                SmsButton.TouchUpInside -= SmsButton_TouchUpInside;

            if (MapButton != null)
                MapButton.TouchUpInside -= MapButton_TouchUpInside;

            if (assignCategoryButton != null)
                assignCategoryButton.Clicked -= AssignCategoryButton_Clicked;

            if (FileToButton != null)
                FileToButton.Clicked -= FileToButton_Clicked;

            if (commentsButton != null)
                commentsButton.Clicked -= CommentsButton_Clicked;

            if (ActionsLinksButton != null)
                ActionsLinksButton.Clicked -= ActionsLinksButton_Clicked;

            if (doneButtonItem != null)
                doneButtonItem.Clicked -= DoneButtonItem_Clicked;

            if (EditButtonItem != null)
                EditButtonItem.Clicked -= EditButtonItem_Clicked;
        }

        void SubscribeToMessages()
        {
            contactChangedToken = CommonConfig.MessengerHub.Subscribe<EntityChangedMessage>(HandleContactChangedMessage, m => m.ObjectType == ObjectType.Contact && ContactPreview?.Id == m.EntityId);
        }

        void UnsubscribeFromMessages()
        {
            contactChangedToken?.Dispose();
        }

        void HandleContactChangedMessage(EntityChangedMessage obj) => RefreshAllOnAppear();

        void RefreshAllOnAppear()
        {
            ((DataSource)TableView.Source)?.Clear();
            ContactId = ContactPreview.Id;
            ContactPreview = null;

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

        public void WebPageClicked(UITableView tableView, UITableViewCell cell, string webPageAddress) => Integration.OpenUrl(this, tableView, cell, webPageAddress);

        public void CopyToClipboard(UITableView tableView, UITableViewCell cell, string text) => Integration.CopyToClipboard(this, tableView, cell, text);

        public void SetData(int folderId, int contactId)
        {
            CommonConfig.UsageAnalytics.LogEvent(new OpenContactEvent());

            Folder = null;
            ContactPreview = null;
            Contact = null;

            FolderId = folderId;
            ContactId = contactId;
        }

        public void SetData(Folder folder, ContactPreview contactPreview, bool hideDoneButton = false)
        {
            CommonConfig.UsageAnalytics.LogEvent(new OpenContactEvent());

            FolderId = null;
            ContactId = null;
            Contact = null;

            Folder = folder;
            ContactPreview = contactPreview;
            HideDoneButton = hideDoneButton;
        }

        public void SetData(ContactPreview contactPreview, bool hideDoneButton)
        {
            CommonConfig.UsageAnalytics.LogEvent(new OpenContactEvent());

            FolderId = null;
            Folder = null;
            ContactId = null;
            Contact = null;

            HideDoneButton = hideDoneButton;
            ContactPreview = contactPreview;
        }

        public void SetData(int contactId)
        {
            CommonConfig.UsageAnalytics.LogEvent(new OpenContactEvent());

            FolderId = null;
            Folder = null;
            ContactPreview = null;
            Contact = null;

            ContactId = contactId;
        }

        public bool IsShowingContactWithId(int contactId) => ContactPreview?.Id == contactId || ContactId == contactId;

        public async void RefreshData()
        {
            cts?.Cancel();
            cts = new CancellationTokenSource();
            var token = cts.Token;

            var folderId = FolderId;
            var folder = Folder;
            var contactId = ContactId;
            var contactPreview = ContactPreview;

            CommonConfig.Logger.Info("Loading contact...");

            var ds = (DataSource)TableView.Source;

            try
            {
                ds.StartRefresh();

                if ((folderId != null || folder != null) && contactId != null)
                {
                    var swp = await Managers.ContactsManager.GetContactWithPreviewAsync(folder?.Id ?? folderId, contactId.Value);

                    ContactPreview = swp.ContactPreview;
                    Contact = swp.Contact;
                }

                if (folder != null && contactPreview != null)
                    Contact = await Managers.ContactsManager.GetContactAsync(folder, contactPreview.Id);

                if (folderId == null && folder == null && contactPreview != null)
                    Contact = await Managers.ContactsManager.GetContactAsync(-1, contactPreview.Id);

                if (folderId == null && folder == null && contactPreview == null)
                {
                    var swp = await Managers.ContactsManager.GetContactWithPreviewAsync(-1, contactId.Value);
                    ContactPreview = swp.ContactPreview;
                    Contact = swp.Contact;
                }

                Contact.CommunicationAddresses = Contact.CommunicationAddresses.OrderBy(ca => ca.Address).ToList();

                if (token.IsCancellationRequested)
                    return;

                if (ContactPreview.Type == ContactType.Person)
                {
                    nameLabel.Text = ContactPreview.Name;
                    if (!string.IsNullOrEmpty(ContactPreview.CompanyName) && !string.IsNullOrEmpty(Contact.Position))
                    {
                        nameSubLabel.Text = $"{Contact.Position} @ {ContactPreview.CompanyName}";
                    }
                    else
                    {
                        string subtitle = string.Empty;
                        subtitle += Contact.Position;
                        subtitle += ContactPreview.CompanyName;

                        nameSubLabel.Text = subtitle;
                    }

                }
                else if (ContactPreview.Type == ContactType.Department)
                {
                    nameLabel.Text = ContactPreview.Name;
                    nameSubLabel.Text = ContactPreview.CompanyName;
                }
                else
                {
                    nameLabel.Text = ContactPreview.Name;
                    nameSubLabel.Text = string.Empty;
                }

                SetAlphaForHeaderButtons();

                if (assignCategoryButton != null)
                    assignCategoryButton.Enabled = true;

                if (FileToButton != null)
                    FileToButton.Enabled = true;

                if (commentsButton != null)
                    commentsButton.Enabled = Contact != null;

                if (ActionsLinksButton != null)
                    ActionsLinksButton.Enabled = true;

                if (EditButtonItem != null)
                {
                    EditButtonItem.Enabled = true;
                    NavigationItem.SetRightBarButtonItem(EditButtonItem, false);
                }

                if (doneButtonItem != null)
                    NavigationItem.SetRightBarButtonItem(doneButtonItem, false);

                ds.EndRefresh(ContactPreview, Contact);
            }
            catch (Exception ex)
            {
                if (token.IsCancellationRequested)
                    return;

                CommonConfig.Logger.Error($"Could not load contact", ex);

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

            FolderId = null;
            Folder = null;
            ContactId = null;
            ContactPreview = null;
            Contact = null;

            nameLabel.Text = string.Empty;
            nameSubLabel.Text = string.Empty;

            EmailButton.Alpha = 0f;
            MobileButton.Alpha = 0f;
            SmsButton.Alpha = 0f;
            MapButton.Alpha = 0f;
            EmailButton.Enabled = false;
            MobileButton.Enabled = false;
            SmsButton.Enabled = false;
            MapButton.Enabled = false;

            if (assignCategoryButton != null)
                assignCategoryButton.Enabled = false;

            if (FileToButton != null)
                FileToButton.Enabled = false;

            if (commentsButton != null)
                commentsButton.Enabled = false;

            if (ActionsLinksButton != null)
                ActionsLinksButton.Enabled = false;

            NavigationItem.SetRightBarButtonItem(null, false);

            ((DataSource)TableView.Source)?.Clear();
        }

        protected virtual void SetAlphaForHeaderButtons()
        {
            EmailButton.Enabled = Contact.CommunicationAddresses.Any(ca => ca.Type == CommunicationAddressType.Email && ca.IsPrimary);
            MobileButton.Enabled = Contact.CommunicationAddresses.Any(ca => (ca.Type == CommunicationAddressType.Mobile || ca.Type == CommunicationAddressType.Phone) && ca.IsPrimary);
            SmsButton.Enabled = Contact.CommunicationAddresses.Any(ca => ca.Type == CommunicationAddressType.Mobile && ca.IsPrimary);
            MapButton.Enabled = Contact.PhysicalAddresses.Any();

            EmailButton.Alpha = 1f;
            MobileButton.Alpha = 1f;
            SmsButton.Alpha = 1f;
            MapButton.Alpha = 1f;
        }

        void UpdateTitle(bool show)
        {
            var title = show ? nameLabel.Text : null;
            NavigationItem.Title = title;
        }

        protected class DataSource : UITableViewSource
        {
            readonly WeakReference<AbstractContactViewController> viewControllerWeakReference;
            readonly WeakReference<UITableView> tableViewWeakReference;

            bool empty = true;
            bool loading = true;

            readonly SectionCollection sections = new SectionCollection();

            public DataSource(AbstractContactViewController viewController, UITableView tableView)
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
                if (Integration.IsRunningAtLeast(11))
                {
                    var offset = scrollView.ContentOffset.Y;
                    var inset = -scrollView.SafeAreaInsets.Top;
                    var show = offset > inset + 30;

                    viewControllerWeakReference.Unwrap()?.UpdateTitle(show);
                }
            }

            public void StartRefresh()
            {
                empty = false;
                loading = true;

                tableViewWeakReference.Unwrap()?.InsertSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            public void EndRefresh(ContactPreview contactPreview, Contact contact)
            {
                var allSections = new AbstractSection[]
                {
                    new CommunicationAddressSection(),
                    new PhysicalAddressSection(),
                    new LinkedContactSection(),
                    new ExtraSection()
                };

                foreach (var section in allSections)
                {
                    section.ContactPreview = contactPreview;
                    section.Contact = contact;

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
                WeakReference<ContactPreview> weakContactPreview;
                WeakReference<Contact> weakContact;

                public ContactPreview ContactPreview
                {
                    get => weakContactPreview.Unwrap();
                    set => weakContactPreview = value.Wrap();
                }

                public Contact Contact
                {
                    get => weakContact.Unwrap();
                    set => weakContact = value.Wrap();
                }

                public abstract bool Empty { get; }
                public RowCollection Rows { get; } = new RowCollection();
                public abstract void InitializeRows();
            }

            public class CommunicationAddressSection : AbstractSection
            {
                readonly CommunicationAddressType[] supportedSections =
                {
                    CommunicationAddressType.Mobile,
                    CommunicationAddressType.Phone,
                    CommunicationAddressType.Email,
                    CommunicationAddressType.Fax,
                    CommunicationAddressType.IM,
                    CommunicationAddressType.Telex,
                    CommunicationAddressType.Internal
                };


                public override bool Empty { get { return !Contact?.CommunicationAddresses?.Any(ca => supportedSections.Contains(ca.Type)) ?? true; } }

                public override void InitializeRows()
                {
                    if (Empty)
                        return;

                    var cas = Contact.CommunicationAddresses.Where(ca => ca.Type == CommunicationAddressType.Mobile);
                    foreach (var ca in cas)
                        Rows.Add(new CommunicationAddressRow(ContactPreview, Contact, ca));

                    cas = Contact.CommunicationAddresses.Where(ca => ca.Type == CommunicationAddressType.Phone);
                    foreach (var ca in cas)
                        Rows.Add(new CommunicationAddressRow(ContactPreview, Contact, ca));

                    cas = Contact.CommunicationAddresses.Where(ca => ca.Type == CommunicationAddressType.Email);
                    foreach (var ca in cas)
                        Rows.Add(new CommunicationAddressRow(ContactPreview, Contact, ca));

                    cas = Contact.CommunicationAddresses.Where(ca => ca.Type == CommunicationAddressType.Fax);
                    foreach (var ca in cas)
                        Rows.Add(new CommunicationAddressRow(ContactPreview, Contact, ca));

                    cas = Contact.CommunicationAddresses.Where(ca => ca.Type == CommunicationAddressType.IM);
                    foreach (var ca in cas)
                        Rows.Add(new CommunicationAddressRow(ContactPreview, Contact, ca));

                    cas = Contact.CommunicationAddresses.Where(ca => ca.Type == CommunicationAddressType.Telex);
                    foreach (var ca in cas)
                        Rows.Add(new CommunicationAddressRow(ContactPreview, Contact, ca));

                    cas = Contact.CommunicationAddresses.Where(ca => ca.Type == CommunicationAddressType.Internal);
                    foreach (var ca in cas)
                        Rows.Add(new CommunicationAddressRow(ContactPreview, Contact, ca));
                }
            }

            public class PhysicalAddressSection : AbstractSection
            {
                public override bool Empty => !Contact?.PhysicalAddresses?.Any() ?? true;

                public override void InitializeRows()
                {
                    if (Empty)
                        return;

                    var pas = Contact.PhysicalAddresses.ToArray();
                    foreach (var pa in pas)
                        Rows.Add(new PhysicalAddressRow(ContactPreview, Contact, pa));
                }
            }

            public class LinkedContactSection : AbstractSection
            {
                public override bool Empty => Contact?.PrimaryPerson == null && (!Contact?.Children?.Any() ?? true);

                public override void InitializeRows()
                {
                    if (Empty)
                        return;

                    if (Contact.PrimaryPerson != null)
                        Rows.Add(new LinkedContactRow(ContactPreview, Contact, Contact.PrimaryPerson));

                    var cps = Contact.Children.Where(cp => cp.Type == ContactType.Person).OrderBy(cp => cp.Name);
                    foreach (var cp in cps)
                        Rows.Add(new LinkedContactRow(ContactPreview, Contact, cp));

                    cps = Contact.Children.Where(cp => cp.Type == ContactType.Department).OrderBy(cp => cp.Name);
                    foreach (var cp in cps)
                        Rows.Add(new LinkedContactRow(ContactPreview, Contact, cp));

                    cps = Contact.Children.Where(cp => cp.Type == ContactType.Company).OrderBy(cp => cp.Name);
                    foreach (var cp in cps)
                        Rows.Add(new LinkedContactRow(ContactPreview, Contact, cp));
                }
            }

            public class ExtraSection : AbstractSection
            {
                public override bool Empty => string.IsNullOrWhiteSpace(ContactPreview?.Description)
                                                    && string.IsNullOrWhiteSpace(ContactPreview?.ShortId) && (!Contact?.ResponsibleUsers?.Any() ?? true)
                                                    && (Contact?.BirthDateTimestamp == -1)
                                                    && string.IsNullOrWhiteSpace(Contact?.WebPageAddress) && string.IsNullOrWhiteSpace(Contact?.Vat) && string.IsNullOrWhiteSpace(Contact?.Ledger) && string.IsNullOrWhiteSpace(Contact?.Account);

                public override void InitializeRows()
                {
                    if (!string.IsNullOrWhiteSpace(ContactPreview?.Description))
                        Rows.Add(new DescriptionRow(ContactPreview, Contact));
                    if (!string.IsNullOrWhiteSpace(ContactPreview?.ShortId))
                        Rows.Add(new ShortIdRow(ContactPreview, Contact));
                    if (!(!Contact?.ResponsibleUsers?.Any() ?? true))
                        Rows.Add(new ResponsibleUsersRow(ContactPreview, Contact));
                    if (Contact?.BirthDateTimestamp != -1)
                        Rows.Add(new BirthdateRow(ContactPreview, Contact));
                    if (!string.IsNullOrWhiteSpace(Contact?.WebPageAddress))
                        Rows.Add(new WebPageRow(ContactPreview, Contact));
                    if (!string.IsNullOrWhiteSpace(Contact?.Vat))
                        Rows.Add(new VatRow(ContactPreview, Contact));
                    if (!string.IsNullOrWhiteSpace(Contact?.Ledger))
                        Rows.Add(new LedgerRow(ContactPreview, Contact));
                    if (!string.IsNullOrWhiteSpace(Contact?.Account))
                        Rows.Add(new AccountRow(ContactPreview, Contact));
                }
            }

            public class RowCollection : List<AbstractRow>
            {
            }

            public abstract class AbstractRow
            {
                WeakReference<ContactPreview> weakContactPreview;
                WeakReference<Contact> weakContact;

                public ContactPreview ContactPreview
                {
                    get => weakContactPreview.Unwrap();
                    set => weakContactPreview = value.Wrap();
                }

                public Contact Contact
                {
                    get => weakContact.Unwrap();
                    set => weakContact = value.Wrap();
                }

                protected AbstractRow(ContactPreview contactPreview, Contact contact)
                {
                    ContactPreview = contactPreview;
                    Contact = contact;
                }

                public virtual string Id => ContactInfoTableViewCell.DefaultId;
                public virtual UITableViewCell CreateCell() => new ContactInfoTableViewCell();
                public abstract void Bind(UITableViewCell cell);
                public virtual void OnClicked(WeakReference<AbstractContactViewController> viewControllerWeakReference, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath) { }
                public virtual void OnLongClicked(WeakReference<AbstractContactViewController> viewControllerWeakReference, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath) { }
            }

            public class DescriptionRow : AbstractRow
            {
                public DescriptionRow(ContactPreview contactPreview, Contact contact)
                    : base(contactPreview, contact)
                {
                }

                public override string Id { get => base.Id + "_Description"; }

                public override void Bind(UITableViewCell cell)
                {
                    var cic = (ContactInfoTableViewCell)cell;
                    cic.Accessory = UITableViewCellAccessory.None;
                    cic.SelectionStyle = UITableViewCellSelectionStyle.Default;
                    cic.Initialize(Localization.GetString("description").ToUpper(), ContactPreview.Description, true);
                }

                public override void OnLongClicked(WeakReference<AbstractContactViewController> viewControllerWeakReference, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewControllerWeakReference.Unwrap()?.CopyToClipboard(tableView, cell, ContactPreview.Description);
                }
            }

            public class CommunicationAddressRow : AbstractRow
            {
                readonly WeakReference<CommunicationAddress> weakCommunicationAddress;

                public CommunicationAddressRow(ContactPreview contactPreview, Contact contact, CommunicationAddress communicationAddress)
                    : base(contactPreview, contact)
                {
                    weakCommunicationAddress = communicationAddress.Wrap();
                }

                public override string Id
                {
                    get
                    {
                        if (string.IsNullOrWhiteSpace(weakCommunicationAddress.Unwrap()?.Description))
                            return CommunicationAddressTableViewCell.CompactId;

                        return CommunicationAddressTableViewCell.DefaultId;
                    }
                }

                public override UITableViewCell CreateCell()
                {
                    if (string.IsNullOrWhiteSpace(weakCommunicationAddress.Unwrap()?.Description))
                        return new CommunicationAddressTableViewCell(CommunicationAddressTableViewCell.CompactId);

                    return new CommunicationAddressTableViewCell(CommunicationAddressTableViewCell.DefaultId);
                }

                public override void Bind(UITableViewCell cell)
                {
                    var ca = weakCommunicationAddress.Unwrap();

                    var catcv = (CommunicationAddressTableViewCell)cell;
                    catcv.Initialize(ca);
                }

                public override void OnClicked(WeakReference<AbstractContactViewController> viewControllerWeakReference, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewControllerWeakReference.Unwrap()?.CommunicationAddressClicked(tableView, cell, weakCommunicationAddress.Unwrap());
                }

                public override void OnLongClicked(WeakReference<AbstractContactViewController> viewControllerWeakReference, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    if (cell is CommunicationAddressTableViewCell caCell)
                        viewControllerWeakReference.Unwrap()?.CopyToClipboard(tableView, cell, caCell.Content);
                }
            }

            public class PhysicalAddressRow : AbstractRow
            {
                readonly WeakReference<PhysicalAddress> weakPhysicalAddress;

                public PhysicalAddressRow(ContactPreview contactPreview, Contact contact, PhysicalAddress physicalAddress)
                    : base(contactPreview, contact)
                {
                    weakPhysicalAddress = physicalAddress.Wrap();
                }

                public override string Id => PhysicalAddressTableViewCell.DefaultId;

                public override UITableViewCell CreateCell() => new PhysicalAddressTableViewCell();

                public override void Bind(UITableViewCell cell)
                {
                    var patvc = (PhysicalAddressTableViewCell)cell;
                    patvc.Initialize(weakPhysicalAddress.Unwrap());
                }

                public override void OnClicked(WeakReference<AbstractContactViewController> viewControllerWeakReference, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewControllerWeakReference.Unwrap()?.PhysicalAddressClicked(tableView, cell, weakPhysicalAddress.Unwrap());
                }

                public override void OnLongClicked(WeakReference<AbstractContactViewController> viewControllerWeakReference, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    if (cell is PhysicalAddressTableViewCell paCell)
                        viewControllerWeakReference.Unwrap()?.CopyToClipboard(tableView, cell, paCell.Content);
                }
            }

            public class LinkedContactRow : AbstractRow
            {
                readonly WeakReference<ContactPreview> weakLinkedContactPreview;

                public LinkedContactRow(ContactPreview contactPreview, Contact contact, ContactPreview linkedContactPreview)
                    : base(contactPreview, contact)
                {
                    weakLinkedContactPreview = linkedContactPreview.Wrap();
                }

                public override string Id => base.Id + "_LinkedContact";

                public override void Bind(UITableViewCell cell)
                {
                    var cp = weakLinkedContactPreview.Unwrap();

                    string type;
                    if (Contact?.PrimaryPerson?.Id == cp.Id)
                        type = Localization.GetString("primary_person");
                    else if (cp.Type == ContactType.Person)
                        type = Localization.GetString("person");
                    else if (cp.Type == ContactType.Department)
                        type = Localization.GetString("department");
                    else
                        type = Localization.GetString("company");

                    var cic = (ContactInfoTableViewCell)cell;
                    cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
                    cell.SelectionStyle = UITableViewCellSelectionStyle.Default;
                    cic.Initialize(type.ToUpper(), cp.Name);
                }

                public override void OnClicked(WeakReference<AbstractContactViewController> viewControllerWeakReference, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewControllerWeakReference?.Unwrap().LinkedContactClicked(weakLinkedContactPreview.Unwrap());
                }
            }

            public class WebPageRow : AbstractRow
            {
                public WebPageRow(ContactPreview contactPreview, Contact contact)
                    : base(contactPreview, contact)
                {
                }

                public override string Id => base.Id + "_WebPage";

                public override void Bind(UITableViewCell cell)
                {
                    var cic = (ContactInfoTableViewCell)cell;
                    cic.Accessory = UITableViewCellAccessory.DisclosureIndicator;
                    cic.SelectionStyle = UITableViewCellSelectionStyle.Default;
                    cic.Initialize(Localization.GetString("webpage").ToUpper(), Contact.WebPageAddress);
                }

                public override void OnClicked(WeakReference<AbstractContactViewController> viewControllerWeakReference, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewControllerWeakReference.Unwrap()?.WebPageClicked(tableView, cell, Contact.WebPageAddress);
                }

                public override void OnLongClicked(WeakReference<AbstractContactViewController> viewControllerWeakReference, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewControllerWeakReference.Unwrap()?.CopyToClipboard(tableView, cell, Contact.WebPageAddress);
                }
            }

            public class BirthdateRow : AbstractRow
            {
                public BirthdateRow(ContactPreview contactPreview, Contact contact)
                    : base(contactPreview, contact)
                {
                }

                public override string Id => base.Id + "_Birthdate";

                public override void Bind(UITableViewCell cell)
                {
                    var cic = (ContactInfoTableViewCell)cell;
                    cic.Accessory = UITableViewCellAccessory.None;
                    cic.SelectionStyle = UITableViewCellSelectionStyle.None;
                    cic.Initialize(Localization.GetString("birthdate").ToUpper(), Contact.BirthDateTimestamp.ConvertTimestampMillisecondsToDateTime()
                                   .ConvertUtcToServerTime()
                                   .ConvertDateTimeToTimestampMilliseconds()
                                   .FormatUserTimestampAsLongDateString());
                }
            }

            public class AccountRow : AbstractRow
            {
                public AccountRow(ContactPreview contactPreview, Contact contact)
                    : base(contactPreview, contact)
                {
                }

                public override string Id => base.Id + "_Account";

                public override void Bind(UITableViewCell cell)
                {
                    var cic = (ContactInfoTableViewCell)cell;
                    cic.Accessory = UITableViewCellAccessory.None;
                    cic.SelectionStyle = UITableViewCellSelectionStyle.Default;
                    cic.Initialize(Localization.GetString("account").ToUpper(), Contact.Account);
                }

                public override void OnLongClicked(WeakReference<AbstractContactViewController> viewControllerWeakReference, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewControllerWeakReference.Unwrap()?.CopyToClipboard(tableView, cell, Contact.Account);
                }
            }

            public class LedgerRow : AbstractRow
            {
                public LedgerRow(ContactPreview contactPreview, Contact contact)
                    : base(contactPreview, contact)
                {
                }

                public override string Id => base.Id + "_Ledger";

                public override void Bind(UITableViewCell cell)
                {
                    var cic = (ContactInfoTableViewCell)cell;
                    cic.Accessory = UITableViewCellAccessory.None;
                    cic.SelectionStyle = UITableViewCellSelectionStyle.Default;
                    cic.Initialize(Localization.GetString("ledger").ToUpper(), Contact.Ledger);
                }

                public override void OnLongClicked(WeakReference<AbstractContactViewController> viewControllerWeakReference, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewControllerWeakReference.Unwrap()?.CopyToClipboard(tableView, cell, Contact.Ledger);
                }
            }

            public class VatRow : AbstractRow
            {
                public VatRow(ContactPreview contactPreview, Contact contact)
                    : base(contactPreview, contact)
                {
                }

                public override string Id => base.Id + "_Vat";

                public override void Bind(UITableViewCell cell)
                {
                    var cic = (ContactInfoTableViewCell)cell;
                    cic.Accessory = UITableViewCellAccessory.None;
                    cic.SelectionStyle = UITableViewCellSelectionStyle.Default;
                    cic.Initialize(Localization.GetString("vat").ToUpper(), Contact.Vat);
                }

                public override void OnLongClicked(WeakReference<AbstractContactViewController> viewControllerWeakReference, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewControllerWeakReference.Unwrap()?.CopyToClipboard(tableView, cell, Contact.Vat);
                }
            }

            public class ResponsibleUsersRow : AbstractRow
            {
                public ResponsibleUsersRow(ContactPreview contactPreview, Contact contact)
                    : base(contactPreview, contact)
                {
                }

                public override string Id => base.Id + "_ResponsibleUsers";

                public override void Bind(UITableViewCell cell)
                {
                    var cic = (ContactInfoTableViewCell)cell;
                    cic.Accessory = UITableViewCellAccessory.None;
                    cic.SelectionStyle = UITableViewCellSelectionStyle.None;
                    cic.Initialize(Localization.GetString("responsible_users").ToUpper(), string.Join(", ", Contact.ResponsibleUsers.Values.OrderBy(s => s)));
                }
            }

            public class ShortIdRow : AbstractRow
            {
                public ShortIdRow(ContactPreview contactPreview, Contact contact)
                    : base(contactPreview, contact)
                {
                }

                public override string Id => base.Id + "_ShortId";

                public override void Bind(UITableViewCell cell)
                {
                    var cic = (ContactInfoTableViewCell)cell;
                    cic.Accessory = UITableViewCellAccessory.None;
                    cic.SelectionStyle = UITableViewCellSelectionStyle.None;
                    cic.Initialize(Localization.GetString("short_id").ToUpper(), ContactPreview.ShortId);
                }
            }
        }

        public override void EncodeRestorableState(NSCoder coder)
        {
            base.EncodeRestorableState(coder);

            coder.Encode(PresentingViewController != null, "doNotRestore");

            if (FolderId.HasValue)
                coder.Encode(FolderId.Value, "folderId");
            if (Folder != null)
                coder.Encode(Serializer.SerializeToByteArray(Folder.ShallowCopy()), "folder");
            if (ContactId.HasValue)
                coder.Encode(ContactId.Value, "contactId");
            if (ContactPreview != null)
                coder.Encode(Serializer.SerializeToByteArray(ContactPreview), "contactPreview");
            if (Contact != null)
                coder.Encode(Serializer.SerializeToByteArray(Contact), "contact");
        }

        public override void DecodeRestorableState(NSCoder coder)
        {
            base.DecodeRestorableState(coder);

            if (coder.ContainsKey("folderId"))
                FolderId = coder.DecodeInt("folderId");
            if (Folder != null)
                Folder = Serializer.DeserializeFromByteArray<Folder>(coder.DecodeBytes("folder"));
            if (coder.ContainsKey("contactId"))
                ContactId = coder.DecodeInt("contactId");
            if (coder.ContainsKey("contactPreview"))
                ContactPreview = Serializer.DeserializeFromByteArray<ContactPreview>(coder.DecodeBytes("contactPreview"));
            if (coder.ContainsKey("contact"))
                Contact = Serializer.DeserializeFromByteArray<Contact>(coder.DecodeBytes("contact"));

            refreshDataOnAppear = true;
        }
    }
}