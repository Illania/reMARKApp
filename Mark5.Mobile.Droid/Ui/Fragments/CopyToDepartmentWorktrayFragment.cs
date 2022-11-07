using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.OS;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Android.Content;
using Mark5.Mobile.Droid.Ui.Activities;
using AndroidX.AppCompat.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.AppCompat.App;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class CopyToDepartmentWorktrayFragment : BaseFragment, IMenuItemOnActionExpandListener, SearchView.IOnQueryTextListener
    {
        readonly Dictionary<int, SystemDepartment> selectedSystemDepartments = new Dictionary<int, SystemDepartment>();

        readonly Handler searchHandler = new Handler();

        const string IdsIntentKey = "IdsIntentKey";
        const string ObjectTypeIntentKey = "ObjectTypeIntentKey";
        const string SelectedSystemDepartmentsKey = "SelectedSystemDepartments_f2f7fa81-a3c0-4b3d-9b24-c2ecd360ae93";
        const string DelayedCopyBundleKey = "DelayedCopy_ed011f46-e180-462c-9d49-5dc047f3c328";

        List<int> businessEntitiesIds;
        ObjectType objectType;
        bool delayedCopy;

        CopyToDepartmentWorktrayAdapter CurrentAdapter => (CopyToDepartmentWorktrayAdapter)recyclerView.GetAdapter();

        SwipeRefreshLayout refreshLayout;
        RecyclerView recyclerView;
        SearchView searchView;
        CopyToDepartmentWorktrayAdapter adapter;
        CopyToDepartmentWorktrayAdapter searchAdapter;
        AppCompatButton copyButton;

        Action dismissAction;

        public static (CopyToDepartmentWorktrayFragment fragment, string tag) NewInstance(List<int> ids, ObjectType ot, bool? delayedCopy = false)
        {
            var args = new Bundle();

            if (ids != null)
                args.PutString(IdsIntentKey, Serializer.Serialize(ids));

            args.PutInt(ObjectTypeIntentKey, (int)ot);


            if (delayedCopy != null)
                args.PutBoolean(DelayedCopyBundleKey, delayedCopy.Value);

            var fragment = new CopyToDepartmentWorktrayFragment();
            fragment.Arguments = args;

            var tag = $"{nameof(CopyToDepartmentWorktrayFragment)}]";

            return (fragment, tag);
        }

        #region Fragment overrides

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Arguments.ContainsKey(IdsIntentKey))
                businessEntitiesIds = Serializer.Deserialize<List<int>>(Arguments.GetString(IdsIntentKey));

            objectType = (ObjectType)Arguments.GetInt(ObjectTypeIntentKey);

            if (Arguments.ContainsKey(DelayedCopyBundleKey))
                delayedCopy = Arguments.GetBoolean(DelayedCopyBundleKey);

            if (savedInstanceState?.ContainsKey(SelectedSystemDepartmentsKey) == true)
            {
                var selectedUsers = Serializer.Deserialize<List<SystemDepartment>>(savedInstanceState.GetString(SelectedSystemDepartmentsKey));

                selectedSystemDepartments.Clear();
                foreach (var s in selectedUsers)
                    selectedSystemDepartments.Add(s.Id, s);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(CopyToDepartmentWorktrayFragment)} [businessEntities.Count={businessEntitiesIds?.Count}]...");

            var rootView = inflater.Inflate(Resource.Layout.list_with_button, container, false);

            refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            refreshLayout.SetColorSchemeResources(Resource.Color.blue, Resource.Color.darkerblue);
            refreshLayout.Enabled = false;

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));

            adapter = new CopyToDepartmentWorktrayAdapter(selectedSystemDepartments);
            adapter.ItemClicked += Adapter_ItemClicked;
            recyclerView.SetAdapter(adapter);

            searchAdapter = new CopyToDepartmentWorktrayAdapter(selectedSystemDepartments);
            searchAdapter.ItemClicked += Adapter_ItemClicked;

            copyButton = rootView.FindViewById<AppCompatButton>(Resource.Id.button);
            copyButton.Text = GetString(Resource.String.copy_to_worktray);
            copyButton.Enabled = false;
            copyButton.Click += async (sender, e) =>
            {
                CommonConfig.Logger.Info($"Attempting copy to worktray [businessEntities.Count={businessEntitiesIds.Count}, selectedUsers.Count={selectedSystemDepartments.Count}]...");

                dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.copying_to_worktray, Resource.String.please_wait);

                try
                {
                    if (delayedCopy == false)
                    {
                        await Managers.CommonActionsManager.CopyToUserWorktray(businessEntitiesIds, objectType,
                        selectedSystemDepartments.SelectMany(d => d.Value.UserIds).ToList());
                        Activity?.OnBackPressed();
                    }
                    else
                    {
                        var data = new Intent();
                        var selectDepartmentUsersIds = selectedSystemDepartments.SelectMany(d => d.Value.UserIds).ToList();
                        data.PutExtra(CopyToUserWorktrayActivity.SelectedUsersResultKey, Serializer.Serialize(selectDepartmentUsersIds));
                        Activity.SetResult(Android.App.Result.Ok, data);
                        Activity?.OnBackPressed();
                    }
                   
                    Activity?.SetResult(Android.App.Result.Ok);
                    Activity?.OnBackPressed();
                }
                catch (Exception ex)
                {
                    dismissAction();

                    CommonConfig.Logger.Error($"Copying to worktray failed [businessEntities.Count={businessEntitiesIds.Count}, selectedUsers.Count={selectedSystemDepartments.Count}]", ex);

                    await Dialogs.ShowErrorDialogAsync(Activity, ex);
                }
            };

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.select_users);
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = null;

            CommonConfig.Logger.Info($"Created {nameof(CopyToDepartmentWorktrayFragment)} [[businessEntities.Count={businessEntitiesIds?.Count}]");
        }

        public override void OnDestroyView()
        {
            dismissAction?.Invoke();
            base.OnDestroyView();
        }

        public override async void OnResume()
        {
            base.OnResume();

            CommonConfig.Logger.Info($"Resuming {nameof(CopyToDepartmentWorktrayFragment)} [businessEntities.Count={businessEntitiesIds?.Count}]...");

            if (adapter.ItemCount < 1)
            {
                CommonConfig.Logger.Info($"No elements - will refresh...");

                await RefreshData();
            }
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            if (selectedSystemDepartments != null)
                outState.PutString(SelectedSystemDepartmentsKey, Serializer.Serialize(selectedSystemDepartments.Values.ToList()));
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

        void Adapter_ItemClicked(object sender, SystemDepartment e)
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

                if (!Restored)
                    refreshLayout.Refreshing = true;

                var userDepartments = await Managers.SystemManager.GetSystemUsersDepartmentsAsync(Restored ? SourceType.Local : SourceType.Auto);
                adapter.SetItems(userDepartments.Departments.OrderBy(d=>d.Name).ToList());
                UpdateControls();
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

        void ToggleSelected(SystemDepartment systemUser)
        {
            var isSelected = selectedSystemDepartments.ContainsKey(systemUser.Id);
            if (isSelected)
                selectedSystemDepartments.Remove(systemUser.Id);
            else
                selectedSystemDepartments.Add(systemUser.Id, systemUser);

            var position = CurrentAdapter.GetPosition(systemUser);
            if (position >= 0)
                CurrentAdapter.NotifyItemChanged(position);
        }

        void UpdateControls()
        {
            if (!IsAdded || IsDetached || IsRemoving)
                return;

            if (selectedSystemDepartments.Count < 1)
            {
                ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.select_users);
                ((AppCompatActivity)Activity).SupportActionBar.Subtitle = null;

                copyButton.Enabled = false;
            }
            else
            {
                ((AppCompatActivity)Activity).SupportActionBar.Title = Resources.GetQuantityString(Resource.Plurals.users_selected, selectedSystemDepartments.Count, selectedSystemDepartments.Count);
                ((AppCompatActivity)Activity).SupportActionBar.Subtitle = null;

                copyButton.Enabled = true;
            }
        }

        #endregion

        #region Filtering

        static bool MatchesQuery(SystemDepartment su, string query)
        {
            if (su.Name.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0)
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

        class CopyToDepartmentWorktrayAdapter : RecyclerView.Adapter
        {
            readonly Dictionary<int, SystemDepartment> selectedSystemDepartmentsInView;

            public override int ItemCount => Items.Count;

            public List<SystemDepartment> Items { get; } = new List<SystemDepartment>(100);

            public CopyToDepartmentWorktrayAdapter(Dictionary<int, SystemDepartment> selectedSystemDepartmentsInView)
            {
                this.selectedSystemDepartmentsInView = selectedSystemDepartmentsInView;
            }

            public event EventHandler<SystemDepartment> ItemClicked = delegate { };

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var suvh = holder as DepartmentViewHolder;
                if (suvh == null)
                    return;

                var su = Items[position];

                suvh.ItemView.SetOnClickListener(new ActionOnClickListener(() => ItemClicked(this, su)));

                suvh.Name = su.Name;

                suvh.Selected = selectedSystemDepartmentsInView.ContainsKey(su.Id);
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_system_user, parent, false);
                return new DepartmentViewHolder(itemView);
            }

            public void SetItems(List<SystemDepartment> items)
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

            public void ReplaceItems(List<SystemDepartment> items)
            {
                Clear();
                SetItems(items);
            }

            public int GetPosition(SystemDepartment systemUser)
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

        }

        class DepartmentViewHolder : RecyclerView.ViewHolder
        {
            public string Name { set => fullnameTextView.Text = value; }

            public bool Selected { set => selectedOverlay.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }

            readonly AppCompatTextView fullnameTextView;
            readonly View selectedOverlay;

            public DepartmentViewHolder(View itemView)
                : base(itemView)
            {
                fullnameTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_system_user_full_name);
                selectedOverlay = itemView.FindViewById<View>(Resource.Id.selected_overlay);
            }
        }

        #endregion
    }
}