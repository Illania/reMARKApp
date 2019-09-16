using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Android.OS;
using Android.App;
using Android.Views;
using Android.Content;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Support.V4.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Presenters.CalendarModule;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Activities;
using Android.Graphics;
using Android.Support.V4.Content;

namespace Mark5.Mobile.Droid.Ui.Fragments.Calendar
{
    public class AddEditParticipantsFragment : BaseFragment, IMenuItemOnMenuItemClickListener
    {
        const string ParticipantsKey = "Participants_a0d328b3-7c18-4a18-b62e-f713cc528cc1";

        RecyclerView recyclerView;
        ParticipantsListAdapter adapter;
        List<ParticipantsViewModel> participants = new List<ParticipantsViewModel>();

        AppCompatButton addButton;
        AppCompatEditText participantTextView;

        readonly TaskCompletionSource<List<ParticipantsViewModel>> tcs = new TaskCompletionSource<List<ParticipantsViewModel>>();
        public Task<List<ParticipantsViewModel>> TaskResult => tcs.Task;

        public static (AddEditParticipantsFragment fragment, string tag) NewInstance(List<ParticipantsViewModel> participants)
        {
            Bundle args = new Bundle();
            var fragment = new AddEditParticipantsFragment
            {
                Arguments = args
            };

            var tag = $"{nameof(AddEditParticipantsFragment)}";

            if (participants != null)
                args.PutString(ParticipantsKey, Serializer.Serialize(participants));

            return (fragment, tag);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Arguments.ContainsKey(ParticipantsKey) == true)
                participants = Serializer.Deserialize<List<ParticipantsViewModel>>(Arguments.GetString(ParticipantsKey));
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(AddEditParticipantsFragment)}");

            HasOptionsMenu = true;

            var rootView = inflater.Inflate(Resource.Layout.list_participants, container, false);

            addButton = rootView.FindViewById<AppCompatButton>(Resource.Id.add_participant_btn);
            participantTextView = rootView.FindViewById<AppCompatEditText>(Resource.Id.participant_text);

            addButton.Click += AddButton_Click;
            addButton.Enabled = false;
            UpdateAddButton();
            participantTextView.TextChanged += ParticipantTextView_TextChanged;

            var emptyView = rootView.FindViewById<AppCompatTextView>(Resource.Id.empty_view);
            emptyView.SetText(Resource.String.no_participants);

            var refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout_participants);
            refreshLayout.Enabled = false;

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));

            adapter = new ParticipantsListAdapter();

            adapter.RegisterAdapterDataObserver(new LambdaEmptyAdapterObserver(() =>
            {
                if (recyclerView.GetAdapter() != adapter)
                    return;

                emptyView.Visibility = adapter.ItemCount < 1 ? ViewStates.Visible : ViewStates.Gone;
                recyclerView.Visibility = adapter.ItemCount > 0 ? ViewStates.Visible : ViewStates.Gone;
            }));

            recyclerView.SetAdapter(adapter);

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.participants);
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = null;

            RefreshView();

            CommonConfig.Logger.Info($"Created {nameof(AddEditParticipantsFragment)}");
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            tcs.TrySetResult(null);
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            if (participants != null)
                outState.PutString(ParticipantsKey, Serializer.Serialize(participants));
        }

        public override async void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            if (requestCode == RequestCodes.RecentAddressesRequestCode && resultCode == (int)Result.Ok)
            {
                var recipient = Serializer.Deserialize<Recipient>(data.GetStringExtra(RecentAddressesListActivity.RecipientResultKey));
                participants.Add(new ParticipantsViewModel
                {
                    Id = recipient.Id,
                    Email = recipient.Address,
                    Name = recipient.Name,
                    Status = ParticipantStatus.NeedAction,
                    Type = ParticipantType.ComAddress,
                });
            }
            else if (requestCode == RequestCodes.PhonebookRequestCode && resultCode == (int)Result.Ok)
            {
                var recipient = Serializer.Deserialize<Recipient>(data.GetStringExtra(PhonebookContactsListActivity.RecipientResultKey));
                participants.Add(new ParticipantsViewModel
                {
                    Name = recipient.Name,
                    Email = recipient.Address,
                    Status = ParticipantStatus.NeedAction,
                    Type = ParticipantType.ComAddress
                });
            }
            else if (requestCode == RequestCodes.ContactsRequestCode && resultCode == (int)Result.Ok)
            {
                var recipient = Serializer.Deserialize<Recipient>(data.GetStringExtra(PickerContactFolderListActivity.RecipientResultKey));
                participants.Add(new ParticipantsViewModel
                {
                    Id = recipient.Id,
                    Name = recipient.Name,
                    Email = recipient.Address,
                    Status = ParticipantStatus.NeedAction,
                    Type = ParticipantType.Client
                });
            }
            else if (requestCode == RequestCodes.InternalContactsRequestCode && resultCode == (int)Result.Ok)
            {
                var users = Serializer.Deserialize<List<SystemUser>>(data.GetStringExtra(PickerInternalContactsListActivity.RecipientResultKey));
                foreach (var user in users)
                {
                    participants.Add(new ParticipantsViewModel
                    {
                        Id = user.Id,
                        Name = user.Username,
                        Email = string.Empty,
                        Status = ParticipantStatus.NeedAction,
                        Type = ParticipantType.User
                    });
                }
            }

            adapter.SetItems(participants);
        }

        void RefreshView()
        {
            adapter.SetItems(participants);
        }

        #region IMenu
        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            menu.Clear();

            var addItem = menu.Add(Menu.None, MenuItemActions.AddParticipants, MenuItemActions.AddParticipants, Resource.String.participants);
            addItem.SetIcon(Resource.Drawable.add_appointment);  //TODO need to rename to just add after refactoring
            addItem.SetShowAsAction(ShowAsAction.Always);
            addItem.SetOnMenuItemClickListener(this);

            var saveItem = menu.Add(Menu.None, MenuItemActions.SaveParticipants, MenuItemActions.SaveParticipants, Resource.String.save);
            saveItem.SetShowAsAction(ShowAsAction.Always);  //Either we use an icon here too, or we change it in the previous view
            saveItem.SetOnMenuItemClickListener(this);
        }

        static class MenuItemActions
        {
            public const int AddParticipants = 10;
            public const int SaveParticipants = 11;
        }

        public bool OnMenuItemClick(IMenuItem item)
        {
            if (item.ItemId == MenuItemActions.AddParticipants)
                OnAddContactClicked();
            if (item.ItemId == MenuItemActions.SaveParticipants)
                OnSaveParticipantsClicked();

            return true;
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            if (!Validator.IsEmailValid(participantTextView.Text))
                return;

            participants.Add(
                    new ParticipantsViewModel
                    {
                        Email = participantTextView.Text,
                        Status = ParticipantStatus.NeedAction,
                        Type = ParticipantType.ComAddress,
                    });


            adapter.SetItems(participants);

            participantTextView.Text = string.Empty;
            addButton.Enabled = false;
            UpdateAddButton();
        }

        void ParticipantTextView_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            addButton.Enabled = Validator.IsEmailValid(participantTextView.Text);
            UpdateAddButton();
        }

        void UpdateAddButton()
        {
            addButton.SetTextColor(new Color(ContextCompat.GetColor(Context, addButton.Enabled ? Resource.Color.black : Resource.Color.lightgray)));
        }

        private void OnSaveParticipantsClicked()
        {
            tcs.TrySetResult(participants);
            ((AppCompatActivity)Activity).SupportFragmentManager.PopBackStack();
        }

        async void OnAddContactClicked()
        {
            var choice = await Dialogs.ShowListDialog(Context, Resource.String.picker_title, Resource.Array.picker_choice_appointments, true);

            switch (choice)
            {
                case 0:
                    DoOpenRecentAddresses();
                    break;
                case 1:
                    DoOpenContacts();
                    break;
                case 2:
                    DoOpenPhonebook();
                    break;
                case 3:
                    DoOpenInternalContacts();
                    break;
                default:
                    return;
            }
        }

        static class RequestCodes
        {
            public const int RecentAddressesRequestCode = 222;
            public const int ContactsRequestCode = 333;
            public const int InternalContactsRequestCode = 334;
            public const int PhonebookRequestCode = 555;
        }

        void DoOpenPhonebook()
        {
            CommonConfig.UsageAnalytics.LogEvent(new ComposeContactPickerEvent(ContactPickerChoice.Phonebook));

            StartActivityForResult(PhonebookContactsListActivity.CreateIntent(Context), RequestCodes.PhonebookRequestCode);
        }

        void DoOpenContacts()
        {
            CommonConfig.UsageAnalytics.LogEvent(new ComposeContactPickerEvent(ContactPickerChoice.Contacts));

            StartActivityForResult(PickerContactFolderListActivity.CreateIntent(Context), RequestCodes.ContactsRequestCode);
        }

        void DoOpenRecentAddresses()
        {
            CommonConfig.UsageAnalytics.LogEvent(new ComposeContactPickerEvent(ContactPickerChoice.Recents));

            StartActivityForResult(RecentAddressesListActivity.CreateIntent(Context), RequestCodes.RecentAddressesRequestCode);
        }

        void DoOpenInternalContacts()
        {
            CommonConfig.UsageAnalytics.LogEvent(new ComposeContactPickerEvent(ContactPickerChoice.Internal));

            StartActivityForResult(PickerInternalContactsListActivity.CreateIntent(Context), RequestCodes.InternalContactsRequestCode);
        }

        #endregion

        class ParticipantsListAdapter : RecyclerView.Adapter
        {
            public List<ParticipantsViewModel> Items { get; } = new List<ParticipantsViewModel>();

            public override int ItemCount => Items.Count;

            public event EventHandler<ParticipantsViewModel> ItemClicked = delegate { };

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var viewHolder = holder as ParticipantViewHolder;
                var participantViewModel = Items[position];
                viewHolder.Address = participantViewModel.Name;
                viewHolder.Name = participantViewModel.Email;
                viewHolder.Status = participantViewModel.Status;
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                return new ParticipantViewHolder(LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_participant, parent, false));
            }

            public void SetItems(List<ParticipantsViewModel> participants)
            {
                var count = Items.Count;
                Items.Clear();
                Items.AddRange(participants);
                NotifyItemRangeRemoved(0, count);
                NotifyItemRangeInserted(count, participants.Count);
            }

            class ParticipantViewHolder : RecyclerView.ViewHolder
            {
                readonly AppCompatImageView iconView;
                readonly AppCompatTextView addressTextView;
                readonly AppCompatTextView nameTextView;

                public ParticipantStatus Status
                {
                    set
                    {
                        switch (value)
                        {
                            case ParticipantStatus.Accepted:
                                iconView.SetImageResource(Resource.Drawable.icon_check);
                                break;
                            case ParticipantStatus.Declined:
                                iconView.SetImageResource(Resource.Drawable.icon_cross);
                                break;
                            default:
                                iconView.SetImageResource(Resource.Drawable.icon_question);
                                break;
                        }
                    }
                }

                public string Name
                {
                    set
                    {
                        if (string.IsNullOrEmpty(value))
                        {
                            nameTextView.Visibility = ViewStates.Gone;
                        }
                        else
                        {
                            nameTextView.Visibility = ViewStates.Visible;
                            nameTextView.Text = value;
                        }
                    }
                }

                public string Address
                {
                    set
                    {
                        if (string.IsNullOrEmpty(value))
                        {
                            addressTextView.Visibility = ViewStates.Gone;
                        }
                        else
                        {
                            addressTextView.Visibility = ViewStates.Visible;
                            addressTextView.Text = value;
                        }
                    }
                }

                public ParticipantViewHolder(View itemView) : base(itemView)
                {
                    addressTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_recipients_address);
                    nameTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_recipients_name);
                    iconView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_participant_icon);
                }
            }
        }
    }
}
