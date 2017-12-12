using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using FastScrollRecycler;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public abstract class AbstractUserSelectionFragment : BaseFragment, IMenuItemOnActionExpandListener, SearchView.IOnQueryTextListener
    {
        UserSelectionAdapter CurrentAdapter => (UserSelectionAdapter)recyclerView.GetAdapter();

        protected readonly Dictionary<int, SystemUser> SelectedSystemUsers = new Dictionary<int, SystemUser>();

        readonly Handler searchHandler = new Handler();

        protected const string PreselectedUserIdsBundleKey = "PreselectedUserIds_a8a78647-2a7f-4e36-9ada-b9ff9b727b80";
        protected const string ActionButtonTextResIdBundleKey = "ActionButtonTextResId_0482d7d6-a109-4a78-8075-f69455052af2";
        protected const string IncludeCurrentUserBundleKey = "IncludeCurrentUser_470297d6-812a-4ae8-8528-e85355aa7da7";
        protected const string AllowNoUserSelectedBundleKey = "AllowNoUserSelected_fca35857-47a0-45c7-b8e1-d5bd4ff8bf39";
        protected const string SystemUsersKey = "SystemUsers_cf15bc94-215a-48dc-bce5-26c3b4ec5903";
        protected const string SelectedSystemUsersKey = "SelectedSystemUsers_124e4282-1191-4d17-9615-c7a6c3d8d0a4";

        protected UserSelectionAdapter Adapter;
        protected UserSelectionAdapter SearchAdapter;

        SwipeRefreshLayout refreshLayout;
        RecyclerView recyclerView;
        SearchView searchView;
        AppCompatButton actionButton;

        protected List<int> preselectedUserIds;
        protected int actionButtonTextResId;
        protected bool includeCurrentUser;
        protected bool allowNoUserSelected;

        #region Fragment overrides

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (Arguments.ContainsKey(PreselectedUserIdsBundleKey))
                preselectedUserIds = Serializer.Deserialize<List<int>>(Arguments.GetString(PreselectedUserIdsBundleKey));

            if (Arguments.ContainsKey(ActionButtonTextResIdBundleKey))
                actionButtonTextResId = Arguments.GetInt(ActionButtonTextResIdBundleKey);

            if (Arguments.ContainsKey(IncludeCurrentUserBundleKey))
                includeCurrentUser = Arguments.GetBoolean(IncludeCurrentUserBundleKey);

            if (Arguments.ContainsKey(AllowNoUserSelectedBundleKey))
                allowNoUserSelected = Arguments.GetBoolean(AllowNoUserSelectedBundleKey);

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

            if (savedInstanceState?.ContainsKey(SystemUsersKey) == true)
            {
                CommonConfig.Logger.Info($"Restoring state...");

                Adapter.SetItems(Serializer.Deserialize<List<SystemUser>>(savedInstanceState.GetString(SystemUsersKey)));

                if (savedInstanceState.ContainsKey(SelectedSystemUsersKey))
                {
                    var selected = Serializer.Deserialize<List<SystemUser>>(savedInstanceState.GetString(SelectedSystemUsersKey));
                    SelectedSystemUsers.Clear();
                    foreach (var kv in selected)
                        SelectedSystemUsers.Add(kv.Id, kv);
                }

                UpdateControls();
            }

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

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            if (Adapter?.Items != null)
                outState.PutString(SystemUsersKey, Serializer.Serialize(Adapter.Items));

            if (SelectedSystemUsers != null)
                outState.PutString(SelectedSystemUsersKey, Serializer.Serialize(SelectedSystemUsers.Values.ToList()));
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.menu_main, menu);

            var searchItem = menu.FindItem(Resource.Id.action_filter);
            searchItem.SetOnActionExpandListener(this);
            searchView = (SearchView)searchItem.ActionView;
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
                    Adapter.SetItems(userDepartments.Users.OrderBy(su => su.Username).ToList());
                else
                    Adapter.SetItems(userDepartments.Users.Where(su => su.Username != ServerConfig.SystemSettings.UserInfo.User.Username).OrderBy(su => su.Username).ToList());

                if (preselectedUserIds != null && preselectedUserIds.Any())
                {
                    foreach (var userId in preselectedUserIds)
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

        bool IMenuItemOnActionExpandListener.OnMenuItemActionExpand(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_filter)
            {
                recyclerView.SwapAdapter(SearchAdapter, true);
                (this as SearchView.IOnQueryTextListener).OnQueryTextChange(string.Empty);
                return true;
            }

            return false;
        }

        bool IMenuItemOnActionExpandListener.OnMenuItemActionCollapse(IMenuItem item)
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