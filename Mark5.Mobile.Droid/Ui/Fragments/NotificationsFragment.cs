//
// Project: Mark5.Mobile.Droid
// File: NotificationsFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{

    public class NotificationsFragment : RetainableStateFragment
    {

        SwipeRefreshLayout refreshLayout;
        RecyclerView recyclerView;
        NotificationsAdapter adapter;

        #region Fragment overrides

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(NotificationsFragment)}...");

            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            refreshLayout.SetColorSchemeResources(Resource.Color.lightbrown, Resource.Color.brown);
            refreshLayout.Refresh += async (sender, e) => { await RefreshData(true); };

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));

            adapter = new NotificationsAdapter(Context);
            adapter.ItemClicked += Adapter_ItemClicked;
            recyclerView.SetAdapter(adapter);

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.notifications);

            CommonConfig.Logger.Info($"Created {nameof(NotificationsFragment)}");
        }

        public override async void OnResume()
        {
            base.OnResume();

            CommonConfig.Logger.Info($"Resuming {nameof(NotificationsFragment)}...");

            if (adapter.ItemCount < 1)
            {
                CommonConfig.Logger.Info($"No elements - will refresh...");

                await RefreshData();
            }
        }

        public override void OnPause()
        {
            base.OnPause();

            CommonConfig.Logger.Info($"Pausing {nameof(NotificationsFragment)}...");
        }

        #endregion

        #region RetainableStateFragment overrides

        public override IRetainableState OnRetainInstanceState()
        {
            CommonConfig.Logger.Info($"Retaining state [notifications.Count={adapter?.ItemCount}]...");

            return new NotificationsFragmentState
            {
                Notifications = adapter.Items
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var dlfs = restoredState as NotificationsFragmentState;
            if (dlfs != null)
            {
                CommonConfig.Logger.Info($"Restoring state [dlfs.items.count={dlfs.Notifications?.Count}]...");

                adapter.AppendItems(dlfs.Notifications);
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(NotificationsFragment)}]";
        }

        #endregion

        #region Refreshing

        async Task RefreshData(bool force = false)
        {
            try
            {
                CommonConfig.Logger.Info($"Refresh running...");

                refreshLayout.Post(() => refreshLayout.Refreshing = true); //Bug: fixed in support library v 24.2.0 (issue 77712)

                if (force)
                {
                    adapter.Clear();
                }

                var notifications = await Managers.NotificationsManager.GetNotificationsAsync(DeviceType.Android, PlatformConfig.Preferences.PushNotificationToken);
                adapter.AppendItems(notifications);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Downloading notifications failed", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
            finally
            {
                refreshLayout.Post(() => refreshLayout.Refreshing = false); //Bug: fixed in support library v 24.2.0 (issue 77712)

                CommonConfig.Logger.Info($"Refresh finished");
            }
        }

        #endregion

        #region Adapter callbacks

        void Adapter_ItemClicked(object sender, Notification notification)
        {
            // TODO
        }

        #endregion

        #region State

        class NotificationsFragmentState : IRetainableState
        {

            public List<Notification> Notifications { get; set; }
        }

        #endregion

        #region RecyclerView Adapter/ViewHolder

        class NotificationsAdapter : RecyclerView.Adapter
        {

            public List<Notification> Items
            {
                get
                {
                    return notificationsInView.ToList();
                }
            }

            public override int ItemCount
            {
                get
                {
                    return notificationsInView.Count;
                }
            }

            readonly List<Notification> notificationsInView = new List<Notification>(200);
            readonly Context context;

            public event EventHandler<Notification> ItemClicked = delegate { };

            public NotificationsAdapter(Context context)
            {
                this.context = context;
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var cpvh = holder as NotificationViewHolder;
                if (cpvh == null) return;

                var n = notificationsInView[position];

                cpvh.ItemView.SetOnClickListener(new ActionOnClickListener(() => ItemClicked(this, n)));

                cpvh.Title = n.Title;
                cpvh.Message = n.Message;
                cpvh.DateTime = n.DateTimeTimestamp
                    .ConvertTimestampMillisecondsToDateTime()
                    .ConvertUtcToServerTime()
                    .ConvertDateTimeToTimestampMilliseconds()
                    .FormatServerTimestampAsCompactShortDateTimeString(context);
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_notifications, parent, false);
                return new NotificationViewHolder(itemView);
            }

            public void AppendItems(List<Notification> items)
            {
                var count = notificationsInView.Count;
                notificationsInView.AddRange(items);
                NotifyItemRangeInserted(count, items.Count);
            }

            public void Clear()
            {
                var size = notificationsInView.Count;
                notificationsInView.Clear();
                NotifyItemRangeRemoved(0, size);
            }
        }

        class NotificationViewHolder : RecyclerView.ViewHolder
        {

            public string Title
            {
                set
                {
                    titleTextView.Text = value;
                }
            }

            public string Message
            {
                set
                {
                    messageTextView.Text = value;
                }
            }

            public string DateTime
            {
                set
                {
                    dateTimeTextView.Text = value;
                }
            }

            readonly AppCompatTextView titleTextView;
            readonly AppCompatTextView messageTextView;
            readonly AppCompatTextView dateTimeTextView;

            public NotificationViewHolder(View itemView)
                    : base(itemView)
            {
                titleTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_notification_title);
                messageTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_notification_message);
                dateTimeTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_notification_date);
            }
        }

        #endregion
    }
}

