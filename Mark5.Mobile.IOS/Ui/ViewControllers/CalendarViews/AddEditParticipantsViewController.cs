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
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews
{
    public class AddEditParticipantsViewController : AbstractTableViewController, IUITableViewDelegate
    {
        readonly TaskCompletionSource<List<ParticipantsViewModel>> tcs = new TaskCompletionSource<List<ParticipantsViewModel>>();
        public Task<List<ParticipantsViewModel>> Result => tcs.Task;

        UIBarButtonItem addParticipantsItem;

        AddEditAppointmentViewModel viewModel;

        Action<ParticipantsViewModel> participantSelectedAction;

        public AddEditParticipantsViewController(AddEditAppointmentViewModel viewModel)
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

            if (Integration.IsRunningAtLeast(11))
            {
                if (NavigationController != null)
                    NavigationController.NavigationBar.PrefersLargeTitles = true;
                NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Automatic;
            }

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
            TableView.Delegate = this;
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
                        Localization.GetString("contact_picker_shortcodes"),
                        Localization.GetString("contact_picker_phonebook"),
                    };

            if (ServerConfig.SystemSettings.SystemInfo.InternalMailsAvailable)
                strings.Add(Localization.GetString("contact_picker_internal_contacts"));

            var choice = await Dialogs.ShowListActionSheetAsync(this, strings.ToArray(), (UIView)this.View);

            switch (choice)
            {
                case 0:
                    await DoOpenRecents();
                    break;
                case 1:
                    await DoOpenContacts();
                    break;
                case 2:
                    //await DoOpenShortcodes();
                    break;
                case 3:
                    await DoOpenPhonebook();
                    break;
                case 4:
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
                viewModel.Participants.Add(new ParticipantsViewModel() { Name = pa.Name, Email = pa.Address });
        }

        async Task DoOpenRecents()
        {
            var vc = new RecentAddressesListViewController();
            PresentViewController(new NavigationController(vc), true, null);
            var result = await vc.Result;
            if (result != null)
            {
                participantSelectedAction(new ParticipantsViewModel());
            }

            var pa = await vc.Result;
            if (pa != null)
                viewModel.Participants.Add(new ParticipantsViewModel() { Name = pa.Name, Email = pa.Address });
        }

        async Task DoOpenContacts()
        {
            var vc = new PickerContactsFoldersListViewController();
            PresentViewController(new NavigationController(vc), true, null);
            var result = await vc.Result;
            if (result != null)
            {
                participantSelectedAction(new ParticipantsViewModel());
            }

            var pa = await vc.Result;
            if (pa != null)
                viewModel.Participants.Add(new ParticipantsViewModel() { Name = pa.Name, Email = pa.Address });
        }

        async Task DoOpenInternalContacts()
        {
            var vc = new MultipleUserSelectionViewController();
            vc.IncludeCurrentUser = false;
            PresentViewController(new NavigationController(vc), true, null);
            var result = await vc.Result;
            if (result != null)
            {
                participantSelectedAction(new ParticipantsViewModel());
            }
            var pa = await vc.Result;
            if (pa != null)
                foreach (var su in pa)
                    viewModel.Participants.Add(new ParticipantsViewModel() { Name = string.Empty, Email = su.Username });
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
