//
// Project: Mark5.Mobile.Droid
// File: CopyToUserWorktrayFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{

    public class CopyToUserWorktrayFragment : RetainableStateFragment
    {

        public List<IBusinessEntity> BusinessEntities { get; set; }
        public Action CloseRequest { get; set; }

        SwipeRefreshLayout refreshLayout;
        RecyclerView recyclerView;
        CopyToUserWorktrayAdapter adapter;
        AppCompatButton copyButton;

        #region Fragment overrides

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(CopyToUserWorktrayFragment)} [businessEntities.Count={BusinessEntities?.Count}]...");

            var rootView = inflater.Inflate(Resource.Layout.list_with_button, container, false);

            refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            refreshLayout.SetColorSchemeResources(Resource.Color.lightbrown, Resource.Color.brown);
            refreshLayout.Enabled = false;

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));

            adapter = new CopyToUserWorktrayAdapter();
            adapter.ItemClicked += Adapter_ItemClicked;
            recyclerView.SetAdapter(adapter);

            copyButton = rootView.FindViewById<AppCompatButton>(Resource.Id.button);
            copyButton.Text = GetString(Resource.String.copy_to_worktray);
            copyButton.Enabled = false;
            copyButton.Click += async (sender, e) =>
            {
                CommonConfig.Logger.Info($"Attempting copy to worktray [businessEntities.Count={BusinessEntities.Count}, adapter.selectedItemCount={adapter.SelectedItemCount}]...");

                var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.copying_to_worktray, Resource.String.please_wait);

                try
                {
                    await Managers.CommonActionsManager.CopyToUserWorktray(BusinessEntities, adapter.SelectedItems);

                    if (CloseRequest != null) CloseRequest();
                }
                catch (Exception ex)
                {
                    dismissAction();

                    CommonConfig.Logger.Error($"Copying to worktray failed [businessEntities.Count={BusinessEntities.Count}, adapter.selectedItemCount={adapter.SelectedItemCount}]", ex);

                    await Dialogs.ShowErrorDialogAsync(Activity, ex);
                }
            };

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.select_users);

            CommonConfig.Logger.Info($"Created {nameof(CopyToUserWorktrayFragment)} [[businessEntities.Count={BusinessEntities?.Count}]");
        }

        public override async void OnResume()
        {
            base.OnResume();

            CommonConfig.Logger.Info($"Resuming {nameof(CopyToUserWorktrayFragment)} [businessEntities.Count={BusinessEntities?.Count}]...");

            if (adapter.ItemCount < 1)
            {
                CommonConfig.Logger.Info($"No elements - will refresh...");

                await RefreshData();
            }
        }

        public override void OnPause()
        {
            base.OnPause();

            CommonConfig.Logger.Info($"Pausing {nameof(CopyToUserWorktrayFragment)} [businessEntities.Count={BusinessEntities?.Count}]...");
        }

        #endregion

        #region RetainableStateFragment overrides

        public override IRetainableState OnRetainInstanceState()
        {
            CommonConfig.Logger.Info($"Retaining state [businessEntities.Count={BusinessEntities?.Count}, systemUsers.Count={adapter?.ItemCount}, selectedSystemUsers.Cound={adapter?.SelectedItemCount}]...");

            return new CopyToUserWorktrayFragmentState
            {
                BusinessEntities = BusinessEntities,
                SystemUsers = adapter.Items,
                SelectedSystemUsers = adapter.SelectedItems
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var dlfs = restoredState as CopyToUserWorktrayFragmentState;
            if (dlfs != null)
            {
                CommonConfig.Logger.Info($"Restoring state [dlfs.businessEntities.Count={dlfs.BusinessEntities?.Count}, dlfs.systemUsers.Count={dlfs.SystemUsers?.Count}, dlfs.selectedSystemUsers.Cound={dlfs.SelectedSystemUsers?.Count}]...");

                BusinessEntities = dlfs.BusinessEntities;
                adapter.AppendItems(dlfs.SystemUsers);
                adapter.SetSelected(dlfs.SelectedSystemUsers, true);

                UpdateControls();
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(CopyToUserWorktrayFragment)}]";
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
                adapter.AppendItems(userDepartments.Users.Where(su => su.Username != ServerConfig.SystemSettings.UserInfo.User.Username).OrderBy(su => su.Username).ToList());
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

        void UpdateControls()
        {
            if (!IsAdded || IsDetached || IsRemoving) return;

            if (adapter.SelectedItemCount < 1)
            {
                ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.select_users);
                copyButton.Enabled = false;
            }
            else
            {
                ((AppCompatActivity)Activity).SupportActionBar.Title = Resources.GetQuantityString(Resource.Plurals.users_selected, adapter.SelectedItemCount, adapter.SelectedItemCount);
                copyButton.Enabled = true;
            }
        }

        #endregion

        #region Adapter callbacks

        void Adapter_ItemClicked(object sender, SystemUser systemUser)
        {
            adapter.SetSelected(systemUser, !adapter.IsSelected(systemUser));
            UpdateControls();
        }

        #endregion

        #region State

        class CopyToUserWorktrayFragmentState : IRetainableState
        {

            public List<IBusinessEntity> BusinessEntities { get; set; }

            public List<SystemUser> SystemUsers { get; set; }

            public List<SystemUser> SelectedSystemUsers { get; set; }
        }

        #endregion

        #region RecyclerView Adapter/ViewHolder

        class CopyToUserWorktrayAdapter : RecyclerView.Adapter
        {

            public List<SystemUser> Items
            {
                get
                {
                    return systemUsersInView.ToList();
                }
            }

            public List<SystemUser> SelectedItems
            {
                get
                {
                    return selectedSystemUsersInView.Values.ToList();
                }
            }

            public override int ItemCount
            {
                get
                {
                    return systemUsersInView.Count;
                }
            }

            public int SelectedItemCount
            {
                get
                {
                    return selectedSystemUsersInView.Count;
                }
            }

            readonly List<SystemUser> systemUsersInView = new List<SystemUser>(100);
            readonly Dictionary<int, SystemUser> selectedSystemUsersInView = new Dictionary<int, SystemUser>();

            public event EventHandler<SystemUser> ItemClicked = delegate { };

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var suvh = holder as UserViewHolder;
                if (suvh == null) return;

                var su = systemUsersInView[position];

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

            public void AppendItems(List<SystemUser> items)
            {
                var count = systemUsersInView.Count;
                systemUsersInView.AddRange(items);
                NotifyItemRangeInserted(count, items.Count);
            }

            public bool IsSelected(SystemUser systemUser)
            {
                return selectedSystemUsersInView.ContainsKey(systemUser.Id);
            }

            public void SetSelected(List<SystemUser> systemUsers, bool selected)
            {
                foreach (var contact in systemUsers)
                {
                    SetSelected(contact, selected);
                }
            }

            public void SetSelected(SystemUser systemUser, bool selected)
            {
                var position = GetPosition(systemUser);
                if (position < 0) return;

                if (selected)
                {
                    selectedSystemUsersInView[systemUser.Id] = systemUser;
                }
                else
                {
                    selectedSystemUsersInView.Remove(systemUser.Id);
                }
                NotifyItemChanged(position);
            }

            public int GetPosition(int systemUserId)
            {
                var position = -1;
                for (var i = 0; i < systemUsersInView.Count; i++)
                {
                    if (systemUsersInView[i].Id == systemUserId)
                    {
                        position = i;
                        break;
                    }
                }
                return position;
            }

            public int GetPosition(SystemUser systemUser)
            {
                var position = -1;
                for (var i = 0; i < systemUsersInView.Count; i++)
                {
                    if (systemUsersInView[i].Id == systemUser.Id)
                    {
                        position = i;
                        break;
                    }
                }
                return position;
            }
        }

        class UserViewHolder : RecyclerView.ViewHolder
        {

            static readonly int[] colors = { Resource.Color.darkerblue, Resource.Color.darkblue, Resource.Color.blue };

            public string FullName
            {
                set
                {
                    fullnameTextView.Text = value;
                    letterTextView.Text = value.SafeSubstring(0, 1).ToUpper();

                    var sd = new ShapeDrawable(new OvalShape());
                    sd.Paint.Color = new Color(ContextCompat.GetColor(ItemView.Context, colors[Math.Abs(value.GetHashCode() % colors.Length)]));
                    letterTextView.Background = sd;
                }
            }

            public string Username
            {
                set
                {
                    username.Text = value;
                }
            }

            public bool Selected
            {
                set
                {
                    selectedOverlay.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
                }
            }

            readonly AppCompatTextView letterTextView;
            readonly AppCompatTextView fullnameTextView;
            readonly AppCompatTextView username;
            readonly View selectedOverlay;

            public UserViewHolder(View itemView)
                    : base(itemView)
            {
                letterTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_system_user_letter);
                fullnameTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_system_user_full_name);
                username = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_system_user_name);
                selectedOverlay = itemView.FindViewById<View>(Resource.Id.selected_overlay);
            }
        }

        #endregion
    }
}

