using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.OS;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using FastScrollRecycler;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public abstract class AbstractUserSelectionFragment : RetainableStateFragment, MenuItemCompat.IOnActionExpandListener, SearchView.IOnQueryTextListener
    {
        public List<int> PreselectedUserIds { get; set; }

        UserSelectionAdapter CurrentAdapter => (UserSelectionAdapter)recyclerView.GetAdapter();

        SwipeRefreshLayout refreshLayout;
        RecyclerView recyclerView;
        SearchView searchView;
        protected UserSelectionAdapter Adapter;
        protected UserSelectionAdapter SearchAdapter;
        AppCompatButton actionButton;

        protected readonly Dictionary<int, SystemUser> SelectedSystemUsers = new Dictionary<int, SystemUser>();

        readonly Handler searchHandler = new Handler();

        int actionButtonTextResId;
        bool includeCurrentUser;
        bool allowNoUserSelected;

        protected AbstractUserSelectionFragment(int actionButtonTextResId, bool includeCurrentUser, bool allowNoUserSelected = false)
        {
            this.actionButtonTextResId = actionButtonTextResId;
            this.includeCurrentUser = includeCurrentUser;
            this.allowNoUserSelected = allowNoUserSelected;
        }

        #region Fragment overrides

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(AbstractUserSelectionFragment)} {GetInfo()}");

            var rootView = inflater.Inflate(Resource.Layout.list_with_button, container, false);

            refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            refreshLayout.SetColorSchemeResources(Resource.Color.blue, Resource.Color.darkerblue);
            refreshLayout.Enabled = false;

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));

            Adapter = new UserSelectionAdapter(SelectedSystemUsers);
            Adapter.ItemClicked += Adapter_ItemClicked;
            recyclerView.SetAdapter(Adapter);

            SearchAdapter = new UserSelectionAdapter(SelectedSystemUsers);
            SearchAdapter.ItemClicked += Adapter_ItemClicked;

            actionButton = rootView.FindViewById<AppCompatButton>(Resource.Id.button);
            actionButton.SetText(actionButtonTextResId);
            actionButton.Enabled = false;
            actionButton.Click += ActionButton_Click;

            HasOptionsMenu = true;

            return rootView;
        }

        protected abstract string GetInfo();

        protected abstract void ActionButton_Click(object sender, EventArgs e);

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.select_users);
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = null;

            CommonConfig.Logger.Info($"Created {nameof(AbstractUserSelectionFragment)} {GetInfo()}");
        }

        public override async void OnResume()
        {
            base.OnResume();

            CommonConfig.Logger.Info($"Resuming {nameof(AbstractUserSelectionFragment)} {GetInfo()}...");

            if (Adapter.ItemCount < 1)
            {
                CommonConfig.Logger.Info($"No elements - will refresh...");

                await RefreshData();
            }
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.menu_main, menu);

            var searchItem = menu.FindItem(Resource.Id.action_filter);
            MenuItemCompat.SetOnActionExpandListener(searchItem, this);
            searchView = (SearchView)MenuItemCompat.GetActionView(searchItem);
            searchView.QueryHint = GetString(Resource.String.filter);
            searchView.SetOnQueryTextListener(this);
        }

        void Adapter_ItemClicked(object sender, SystemUser e)
        {
            ToggleSelected(e);
            UpdateControls();
        }

        #endregion

        #region Local methods

        async Task RefreshData()
        {
            try
            {
                CommonConfig.Logger.Info($"Refresh running...");

                refreshLayout.Refreshing = true;

                var userDepartments = await Managers.SystemManager.GetSystemUsersDepartmentsAsync();
                if (includeCurrentUser)
                {
                    Adapter.SetItems(userDepartments.Users.OrderBy(su => su.Username).ToList());
                }
                else
                {
                    Adapter.SetItems(userDepartments.Users.Where(su => su.Username != ServerConfig.SystemSettings.UserInfo.User.Username).OrderBy(su => su.Username).ToList()); //TODO need to fix this part
                }

                if (PreselectedUserIds != null && PreselectedUserIds.Any())
                {
                    foreach (var userId in PreselectedUserIds)
                    {
                        var user = Adapter.Items.FirstOrDefault(u => u.Id == userId);
                        if (user != null)
                        {
                            SelectedSystemUsers.Add(userId, user);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Downloading system users failed", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
            finally
            {
                refreshLayout.Refreshing = false;

                CommonConfig.Logger.Info($"Refresh finished");
            }
        }

        void ToggleSelected(SystemUser systemUser)
        {
            var isSelected = SelectedSystemUsers.ContainsKey(systemUser.Id);
            if (isSelected)
                SelectedSystemUsers.Remove(systemUser.Id);
            else
                SelectedSystemUsers.Add(systemUser.Id, systemUser);

            var position = CurrentAdapter.GetPosition(systemUser);
            if (position >= 0)
                CurrentAdapter.NotifyItemChanged(position);
        }

        protected void UpdateControls()
        {
            if (!IsAdded || IsDetached || IsRemoving)
                return;

            if (SelectedSystemUsers.Count < 1)
            {
                ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.select_users);
                ((AppCompatActivity)Activity).SupportActionBar.Subtitle = null;

                if (!allowNoUserSelected)
                {
                    actionButton.Enabled = false;
                }
            }
            else
            {
                ((AppCompatActivity)Activity).SupportActionBar.Title = Resources.GetQuantityString(Resource.Plurals.users_selected, SelectedSystemUsers.Count, SelectedSystemUsers.Count);
                ((AppCompatActivity)Activity).SupportActionBar.Subtitle = null;

                actionButton.Enabled = true;
            }
        }

        #endregion

        #region Filtering

        bool MenuItemCompat.IOnActionExpandListener.OnMenuItemActionExpand(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_filter)
            {
                recyclerView.SwapAdapter(SearchAdapter, true);
                (this as SearchView.IOnQueryTextListener).OnQueryTextChange(string.Empty);
                return true;
            }

            return false;
        }

        bool MenuItemCompat.IOnActionExpandListener.OnMenuItemActionCollapse(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_filter)
            {
                searchHandler.RemoveCallbacksAndMessages(null);
                SearchAdapter.Clear();
                recyclerView.SwapAdapter(Adapter, true);
                return true;
            }

            return false;
        }

        bool SearchView.IOnQueryTextListener.OnQueryTextChange(string newText)
        {
            searchHandler.RemoveCallbacksAndMessages(null);
            searchHandler.PostDelayed(() =>
                {
                    if (string.IsNullOrWhiteSpace(newText))
                        SearchAdapter.ReplaceItems(Adapter.Items);
                    else
                        SearchAdapter.ReplaceItems(Adapter.Items.Where(su => MatchesQuery(su, newText)).ToList());
                },
                500);
            return false;
        }

        bool SearchView.IOnQueryTextListener.OnQueryTextSubmit(string newText)
        {
            return false;
        }

        static bool MatchesQuery(SystemUser su, string query)
        {
            if (su.Username.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0)
                return true;
            if (su.FirstName.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0)
                return true;
            if (su.LastName.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0)
                return true;
            if (su.PatronymicName.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0)
                return true;

            return false;
        }

        #endregion


        #region RetainableStateFragment overrides

        public override IRetainableState OnRetainInstanceState()
        {
            CommonConfig.Logger.Info($"Retaining state {GetInfo()}[systemUsers.Count={Adapter?.ItemCount}, selectedSystemUsers.Count={SelectedSystemUsers.Count}]...");

            return new AbstractUserSelectionFragmentState
            {
                SystemUsers = Adapter.Items,
                SelectedSystemUsers = SelectedSystemUsers,
                AllowNoUserSelected = allowNoUserSelected,
                IncludeCurrentUser = includeCurrentUser,
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            if (restoredState is AbstractUserSelectionFragmentState dlfs)
            {
                CommonConfig.Logger.Info($"Restoring state {GetInfo()}[dlfs.systemUsers.Count={dlfs.SystemUsers?.Count}, dlfs.selectedSystemUsers.Cound={dlfs.SelectedSystemUsers?.Count}]...");

                allowNoUserSelected = dlfs.AllowNoUserSelected;
                includeCurrentUser = dlfs.IncludeCurrentUser;

                Adapter.SetItems(dlfs.SystemUsers);

                SelectedSystemUsers.Clear();
                foreach (var kv in dlfs.SelectedSystemUsers)
                    SelectedSystemUsers.Add(kv.Key, kv.Value);

                UpdateControls();
            }
        }

        #endregion

        #region State

        protected class AbstractUserSelectionFragmentState : IRetainableState
        {
            public List<SystemUser> SystemUsers { get; set; }
            public Dictionary<int, SystemUser> SelectedSystemUsers { get; set; }
            public bool IncludeCurrentUser { get; set; }
            public bool AllowNoUserSelected { get; set; }
        }

        #endregion


        #region RecyclerView Adapter/ViewHolder

        protected class UserSelectionAdapter : RecyclerView.Adapter, ISectionedAdapter
        {
            readonly Dictionary<int, SystemUser> selectedSystemUsersInView;

            public override int ItemCount => Items.Count;

            public List<SystemUser> Items { get; } = new List<SystemUser>(100);

            public UserSelectionAdapter(Dictionary<int, SystemUser> selectedSystemUsersInView)
            {
                this.selectedSystemUsersInView = selectedSystemUsersInView;
            }

            public event EventHandler<SystemUser> ItemClicked = delegate { };

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var suvh = holder as UserViewHolder;
                if (suvh == null)
                    return;

                var su = Items[position];

                suvh.ItemView.SetOnClickListener(new ActionOnClickListener(() => ItemClicked(this, su)));

                suvh.FullName = su.FirstName + " " + su.LastName;
                suvh.Username = su.Username;

                suvh.Selected = selectedSystemUsersInView.ContainsKey(su.Id);
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_system_user, parent, false);
                return new UserViewHolder(itemView);
            }

            public void SetItems(List<SystemUser> items)
            {
                var count = Items.Count;
                Items.AddRange(items);
                NotifyItemRangeInserted(count, items.Count);
            }

            public void Clear()
            {
                var count = Items.Count;
                Items.Clear();
                NotifyItemRangeRemoved(0, count);
            }

            public void ReplaceItems(List<SystemUser> items)
            {
                Clear();
                SetItems(items);
            }

            public int GetPosition(SystemUser systemUser)
            {
                var position = -1;
                for (var i = 0; i < Items.Count; i++)
                    if (Items[i].Id == systemUser.Id)
                    {
                        position = i;
                        break;
                    }

                return position;
            }

            string ISectionedAdapter.GetSectionName(int position)
            {
                return Items[position].Username?.SafeSubstring(0, 1)?.ToUpper() ?? "";
            }
        }

        class UserViewHolder : RecyclerView.ViewHolder
        {
            public string FullName { set => fullnameTextView.Text = value; }

            public string Username { set => username.Text = value; }

            public bool Selected { set => selectedOverlay.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }

            readonly AppCompatTextView fullnameTextView;
            readonly AppCompatTextView username;
            readonly View selectedOverlay;

            public UserViewHolder(View itemView)
                : base(itemView)
            {
                fullnameTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_system_user_full_name);
                username = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_system_user_name);
                selectedOverlay = itemView.FindViewById<View>(Resource.Id.selected_overlay);
            }
        }

        #endregion
    }
}