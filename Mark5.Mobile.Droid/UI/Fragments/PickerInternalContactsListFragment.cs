using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using FastScrollRecycler;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Activities;
using Android.Content;
using Android.App;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class PickerInternalContactsListFragment : BaseFragment, IMenuItemOnActionExpandListener, SearchView.IOnQueryTextListener
    {
        readonly Dictionary<int, SystemUser> selectedSystemUsers = new Dictionary<int, SystemUser>();

        readonly Handler searchHandler = new Handler();

        const string SelectedSystemUsersKey = "SelectedSystemUsers_a5882c46-4209-48d8-9759-139286f1e476";

        SelectInternalUsersAdapter CurrentAdapter => (SelectInternalUsersAdapter)recyclerView.GetAdapter();

        SwipeRefreshLayout refreshLayout;
        RecyclerView recyclerView;
        SearchView searchView;
        SelectInternalUsersAdapter adapter;
        SelectInternalUsersAdapter searchAdapter;

        public static (PickerInternalContactsListFragment fragment, string tag) NewInstance()
        {
            var fragment = new PickerInternalContactsListFragment();

            var tag = $"{nameof(PickerInternalContactsListFragment)}]";

            return (fragment, tag);
        }

        #region Fragment overrides

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (savedInstanceState?.ContainsKey(SelectedSystemUsersKey) == true)
            {
                var selectedUsers = Serializer.Deserialize<List<SystemUser>>(savedInstanceState.GetString(SelectedSystemUsersKey));

                selectedSystemUsers.Clear();
                foreach (var s in selectedUsers)
                    selectedSystemUsers.Add(s.Id, s);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(CopyToUserWorktrayFragment)} [businessEntities.Count={businessEntities?.Count}]...");

            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            refreshLayout.SetColorSchemeResources(Resource.Color.blue, Resource.Color.darkerblue);
            refreshLayout.Enabled = false;

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));

            adapter = new SelectInternalUsersAdapter(selectedSystemUsers);
            adapter.ItemClicked += Adapter_ItemClicked;
            recyclerView.SetAdapter(adapter);

            searchAdapter = new SelectInternalUsersAdapter(selectedSystemUsers);
            searchAdapter.ItemClicked += Adapter_ItemClicked;

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            CommonConfig.Logger.Info($"Created {nameof(PickerInternalContactsListFragment)} [[businessEntities.Count={businessEntities?.Count}]");
        }

        public override async void OnResume()
        {
            base.OnResume();

            CommonConfig.Logger.Info($"Resuming {nameof(PickerInternalContactsListFragment)} [businessEntities.Count={businessEntities?.Count}]...");

            if (adapter.ItemCount < 1)
            {
                CommonConfig.Logger.Info($"No elements - will refresh...");

                await RefreshData();
            }
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            if (selectedSystemUsers != null)
                outState.PutString(SelectedSystemUsersKey, Serializer.Serialize(selectedSystemUsers.Values.ToList()));
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
            var intent = new Intent();
            intent.PutExtra(PickerInternalContactsListActivity.RecipientResultKey, Serializer.Serialize(e));
            Activity.SetResult(Result.Ok, intent);
            Activity?.Finish();
        }

        #endregion

        #region Local methods

        async Task RefreshData()
        {
            try
            {
                CommonConfig.Logger.Info($"Refresh running...");

                if (!Restored)
                    refreshLayout.Refreshing = true;

                var userDepartments = await Managers.SystemManager.GetSystemUsersDepartmentsAsync(Restored ? SourceType.Local : SourceType.Auto);
                adapter.SetItems(userDepartments.Users.Where(su => su.Username != ServerConfig.SystemSettings.UserInfo.User.Username).OrderBy(su => su.Username).ToList());
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

        #endregion

        #region Filtering

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

        bool IMenuItemOnActionExpandListener.OnMenuItemActionExpand(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_filter)
            {
                recyclerView.SwapAdapter(searchAdapter, true);
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
                searchAdapter.Clear();
                recyclerView.SwapAdapter(adapter, true);
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
                    searchAdapter.ReplaceItems(adapter.Items);
                else
                    searchAdapter.ReplaceItems(adapter.Items.Where(su => MatchesQuery(su, newText)).ToList());
            },
                500);
            return false;
        }

        bool SearchView.IOnQueryTextListener.OnQueryTextSubmit(string newText)
        {
            return false;
        }

        #endregion

        #region RecyclerView Adapter/ViewHolder

        class SelectInternalUsersAdapter : RecyclerView.Adapter, ISectionedAdapter
        {
            readonly Dictionary<int, SystemUser> selectedSystemUsersInView;

            public override int ItemCount => Items.Count;

            public List<SystemUser> Items { get; } = new List<SystemUser>(100);

            public SelectInternalUsersAdapter(Dictionary<int, SystemUser> selectedSystemUsersInView)
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
