using UIKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Presenters.CalendarModule;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews
{
    public class AppointmentParticipantsViewController : AbstractTableViewController, IUITableViewDelegate
    {
        readonly TaskCompletionSource<List<ParticipantsViewModel>> tcs = new TaskCompletionSource<List<ParticipantsViewModel>>();
        public Task<List<ParticipantsViewModel>> Result => tcs.Task;

        UIBarButtonItem addParticipantsItem;

        UITextField field;
        UIButton addButton;

        AddEditAppointmentViewModel viewModel;

        public AppointmentParticipantsViewController(AddEditAppointmentViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public override void LoadView()
        {
            base.LoadView();
            InitializeNavigationBar();
            InitializeView();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (NavigationController != null)
                NavigationController.NavigationBar.PrefersLargeTitles = false;
            NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Never;

            InitializeHandlers();
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info("Appeared");

            RefreshData();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            DeinitializeHandlers();
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning("Received memory warning!");

            ((DataSource)TableView.Source)?.Reset();

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        protected override void Recycle()
        {
            base.Recycle();

            addParticipantsItem = null;

            ((DataSource)TableView.Source)?.Reset();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (CommonConfig.Logger.IsDebugEnabled())
                CommonConfig.Logger.Debug("Disposed");
        }

        void InitializeNavigationBar()
        {
            Title = Localization.GetString("participants");

            addParticipantsItem = new UIBarButtonItem
            {
                Title = Localization.GetString("add"),
                Image = UIImage.FromBundle("Create")
            };

            NavigationItem.SetRightBarButtonItem(addParticipantsItem, false);
        }

        void InitializeView()
        {
            TableView.Source = new DataSource(TableView);
            TableView.AllowsSelection = true;
            TableView.AllowsMultipleSelection = true;
            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 40f;
            TableView.TableHeaderView = GetHeader();
            TableView.Delegate = this;

            EdgesForExtendedLayout = UIRectEdge.None;
        }

        UIView GetHeader()
        {
            var headerView = new UIView { TranslatesAutoresizingMaskIntoConstraints = false };

            field = new UITextField
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BorderStyle = UITextBorderStyle.RoundedRect,
            };
            field.EditingChanged += Field_EditingChanged;

            addButton = new UIButton(UIButtonType.System)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
            };
            addButton.Enabled = false;
            addButton.SetTitle("ADD", UIControlState.Normal);
            addButton.TouchUpInside += AddButton_TouchUpInside;

            var separator = new UIView { TranslatesAutoresizingMaskIntoConstraints = false };
            separator.BackgroundColor = TableView.SeparatorColor;

            headerView.AddSubview(field);
            headerView.AddSubview(addButton);
            headerView.AddSubview(separator);

            headerView.AddConstraints(new[]
            {
                    field.LeadingAnchor.ConstraintEqualTo(headerView.LeadingAnchor, TableView.SeparatorInset.Left),
                    field.TopAnchor.ConstraintEqualTo(headerView.TopAnchor, 10f),

                    addButton.LeadingAnchor.ConstraintEqualTo(field.TrailingAnchor, 25f),
                    addButton.TrailingAnchor.ConstraintEqualTo(headerView.TrailingAnchor, -15f),
                    addButton.CenterYAnchor.ConstraintEqualTo(field.CenterYAnchor),

                    separator.WidthAnchor.ConstraintEqualTo(headerView.WidthAnchor),
                    separator.HeightAnchor.ConstraintEqualTo(1f),
                    separator.BottomAnchor.ConstraintEqualTo(headerView.BottomAnchor),
                    separator.TopAnchor.ConstraintEqualTo(field.BottomAnchor, 10f),

             });

            TableView.AddConstraint(headerView.WidthAnchor.ConstraintEqualTo(TableView.WidthAnchor));
            TableView.AddConstraint(headerView.HeightAnchor.ConstraintEqualTo(60f));

            return headerView;
        }

        void AddButton_TouchUpInside(object sender, EventArgs e)
        {
            if (!Validator.IsEmailValid(field.Text))
                return;

            viewModel.Participants.Add(new ParticipantsViewModel
            {
                Email = field.Text,
                Status = Mobile.Common.Model.ParticipantStatus.NeedAction,
                Type = Mobile.Common.Model.ParticipantType.ComAddress
            });

            RefreshData();

            field.Text = string.Empty;
            addButton.Enabled = false;
        }

        void Field_EditingChanged(object sender, EventArgs e)
        {
            addButton.Enabled = Validator.IsEmailValid(field.Text);
        }

        public override UISwipeActionsConfiguration GetTrailingSwipeActionsConfiguration(UITableView tableView, NSIndexPath indexPath)
        {
            var deleteAction = UIContextualAction.FromContextualActionStyle(UIContextualActionStyle.Destructive, "Delete", (someAction, view, success) =>
            {
                viewModel.Participants.RemoveAt(indexPath.Row);
                if (!viewModel.Participants.Any())
                    viewModel.Participants = new List<ParticipantsViewModel>();

                RefreshData();
            });

            var trailingSwipe = UISwipeActionsConfiguration.FromActions(new[] { deleteAction });
            return trailingSwipe;
        }

        void InitializeHandlers()
        {
            if (addParticipantsItem != null)
                addParticipantsItem.Clicked += DoneItem_Clicked;
        }

        void DeinitializeHandlers()
        {
            if (addParticipantsItem != null)
                addParticipantsItem.Clicked -= DoneItem_Clicked;
        }

        async void DoneItem_Clicked(object sender, EventArgs e)
        {
            var strings = new List<string>
                    {
                        Localization.GetString("contact_picker_recent_addresses"),
                        Localization.GetString("contact_picker_contacts"),
                        Localization.GetString("contact_picker_phonebook"),
                        Localization.GetString("contact_picker_internal_contacts"),
                    };

            var choice = await Dialogs.ShowListActionSheetAsync(this, strings.ToArray(), addParticipantsItem);

            switch (choice)
            {
                case 0:
                    await DoOpenRecents();
                    break;
                case 1:
                    await DoOpenContacts();
                    break;
                case 2:
                    await DoOpenPhonebook();
                    break;
                case 3:
                    await DoOpenInternalContacts();
                    break;
                default:
                    return;
            }
        }

        async Task DoOpenPhonebook()
        {
            var vc = new PhonebookContactsListViewController();
            PresentViewController(new NavigationController(vc), true, null);

            var pa = await vc.Result;
            if (pa != null)
            {
                viewModel.Participants.Add(new ParticipantsViewModel
                {
                    Name = pa.Name,
                    Email = pa.Address,
                    Status = Mobile.Common.Model.ParticipantStatus.NeedAction,
                    Type = Mobile.Common.Model.ParticipantType.ComAddress
                });
                RefreshData();
            }
        }

        async Task DoOpenRecents()
        {
            var vc = new RecentAddressesListViewController();
            PresentViewController(new NavigationController(vc), true, null);
            var pa = await vc.Result;
            if (pa != null)
            {
                viewModel.Participants.Add(new ParticipantsViewModel
                {
                    Id = pa.Id,
                    Name = pa.Name,
                    Email = pa.Address,
                    Status = Mobile.Common.Model.ParticipantStatus.NeedAction,
                    Type = Mobile.Common.Model.ParticipantType.ComAddress
                });
                RefreshData();
            }
        }

        async Task DoOpenContacts()
        {
            var vc = new PickerContactsFoldersListViewController();
            PresentViewController(new NavigationController(vc), true, null);
            var pa = await vc.Result;
            if (pa != null)
            {
                viewModel.Participants.Add(new ParticipantsViewModel
                {
                    Id = pa.Id,
                    Name = pa.Name,
                    Email = pa.Address,
                    Status = Mobile.Common.Model.ParticipantStatus.NeedAction,
                    Type = Mobile.Common.Model.ParticipantType.Client
                });
                RefreshData();
            }
        }

        async Task DoOpenInternalContacts()
        {
            var vc = new MultipleUserSelectionViewController();
            vc.IncludeCurrentUser = false;
            PresentViewController(new NavigationController(vc), true, null);
            var pa = await vc.Result;
            if (pa != null)
            {
                foreach (var su in pa)
                    viewModel.Participants.Add(new ParticipantsViewModel
                    {
                        Id = su.Id,
                        Name = su.Username,
                        Email = string.Empty,
                        Status = Mobile.Common.Model.ParticipantStatus.NeedAction,
                        Type = Mobile.Common.Model.ParticipantType.User
                    });
                RefreshData();
            }
        }

        void RefreshData()
        {
            ((DataSource)TableView.Source).SetItems(viewModel.Participants);
        }

        class DataSource : UITableViewSource
        {
            List<ParticipantsViewModel> items = new List<ParticipantsViewModel>();
            readonly WeakReference<UITableView> tableViewWeakReference;

            bool Empty => (items == null || items.Count == 0);

            public DataSource(UITableView tableView)
            {
                tableViewWeakReference = tableView.Wrap();
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var c = items[indexPath.Row];
                var cell = tableView.DequeueReusableCell(ParticipantsTableViewCell.DefaultId) as ParticipantsTableViewCell ?? new ParticipantsTableViewCell();
                cell.Editing = true;
                cell.Initialize(c);

                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (Empty)
                    return 0;

                return items.Count;
            }

            public void SetItems(List<ParticipantsViewModel> participants)
            {
                items.Clear();
                if (participants != null)
                    items.AddRange(participants.OrderBy(c => c.Name));

                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            public void Reset()
            {
                items.Clear();
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }
        }
    }
}
