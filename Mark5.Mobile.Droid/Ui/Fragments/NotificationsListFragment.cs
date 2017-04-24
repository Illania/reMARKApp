//
// Project: Mark5.Mobile.Droid
// File: NotificationsListFragment.cs
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
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Common.HubMessages;
using Mark5.Mobile.Droid.Utilities;
using TinyMessenger;

namespace Mark5.Mobile.Droid.Ui.Fragments
{

    public class NotificationsListFragment : RetainableStateFragment
    {

        public ObjectType[] ObjectTypes { get; set; }

        SwipeRefreshLayout refreshLayout;
        RecyclerView recyclerView;
        NotificationsAdapter adapter;

        TinyMessageSubscriptionToken newNotificationsToken;

        #region Fragment overrides

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(NotificationsListFragment)}...");

            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            var emptyView = rootView.FindViewById<AppCompatTextView>(Resource.Id.empty_view);
            emptyView.SetText(Resource.String.no_notifications);

            refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            refreshLayout.SetColorSchemeResources(Resource.Color.blue, Resource.Color.darkerblue);
            refreshLayout.Refresh += async (sender, e) => { await RefreshData(); };

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));

            adapter = new NotificationsAdapter(Context);
            adapter.RegisterAdapterDataObserver(new LambdaEmptyAdapterObserver(() =>
            {
                if (recyclerView.GetAdapter() != adapter) return;

                emptyView.Visibility = adapter.ItemCount < 1 ? ViewStates.Visible : ViewStates.Gone;
                recyclerView.Visibility = adapter.ItemCount > 0 ? ViewStates.Visible : ViewStates.Gone;
            }));
            adapter.ItemClicked += Adapter_ItemClicked;
            recyclerView.SetAdapter(adapter);

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            if (!(view.Parent is ViewPager))
            {
                ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.notifications);
                ((AppCompatActivity)Activity).SupportActionBar.Subtitle = null;
            }

            CommonConfig.Logger.Info($"Created {nameof(NotificationsListFragment)}");
        }

        public override async void OnResume()
        {
            base.OnResume();

            CommonConfig.Logger.Info($"Resuming {nameof(NotificationsListFragment)}...");

            await RefreshData();

            newNotificationsToken = PlatformConfig.MessengerHub.Subscribe<NewNotificationsReceived>(m => Activity.RunOnUiThread(async () => await RefreshData()));
        }

        public override void OnPause()
        {
            base.OnPause();

            CommonConfig.Logger.Info($"Pausing {nameof(NotificationsListFragment)}...");

            if (newNotificationsToken != null)
                PlatformConfig.MessengerHub.Unsubscribe<NewNotificationsReceived>(newNotificationsToken);
        }

        #endregion

        #region Options menu

        static class MenuItemActions
        {
            public const int MarkAllAsRead = 10;
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            menu.Add(Menu.None, MenuItemActions.MarkAllAsRead, MenuItemActions.MarkAllAsRead, Resource.String.mark_all_as_read);
        }

        public override void OnPrepareOptionsMenu(IMenu menu)
        {
            menu.FindItem(MenuItemActions.MarkAllAsRead)?.SetEnabled(adapter.Items.Any(n => !n.IsRead));
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == MenuItemActions.MarkAllAsRead)
            {
                MarkAllAsRead();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region RetainableStateFragment overrides

        public override IRetainableState OnRetainInstanceState()
        {
            CommonConfig.Logger.Info($"Retaining state [notifications.Count={adapter?.ItemCount}]...");

            return new NotificationsFragmentState
            {
                ObjectTypes = ObjectTypes,
                Notifications = adapter.Items
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var dlfs = restoredState as NotificationsFragmentState;
            if (dlfs != null)
            {
                CommonConfig.Logger.Info($"Restoring state [dlfs.items.count={dlfs.Notifications?.Count}]...");

                ObjectTypes = dlfs.ObjectTypes;
                adapter.AppendItems(dlfs.Notifications);
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(NotificationsListFragment)}]";
        }

        #endregion

        #region Refreshing

        async Task RefreshData()
        {
            try
            {
                CommonConfig.Logger.Info($"Refresh running...");

                refreshLayout.Refreshing = true;

                var notifications = await Managers.NotificationsManager.GetNotificationsAsync(DeviceType.Android, PlatformConfig.Preferences.PushNotificationToken);
                notifications = notifications.Where(n => ObjectTypes.Contains(n.ObjectType)).ToList();

                adapter.Clear();
                adapter.AppendItems(notifications);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Downloading notifications failed", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
            finally
            {
                refreshLayout.Refreshing = false;

                CommonConfig.Logger.Info($"Refresh finished");
            }
        }

        #endregion

        #region Adapter callbacks

        async void Adapter_ItemClicked(object sender, Notification notification)
        {
            await Managers.NotificationsManager.MarkAsRead(notification);

            var position = adapter.GetPosition(notification);
            if (position >= 0)
            {
                adapter.NotifyItemChanged(position);
            }

            if (notification.ObjectType == ObjectType.Document)
            {
                var i = new Intent(Activity, typeof(DocumentActivity));
                i.PutExtra(DocumentActivity.FolderIdIntentKey, notification.FolderId);
                i.PutExtra(DocumentActivity.DocumentIdIntentKey, notification.ObjectId);
                StartActivity(i);
            }
            if (notification.ObjectType == ObjectType.Contact)
            {
                var i = new Intent(Activity, typeof(ContactActivity));
                i.PutExtra(ContactActivity.FolderIdIntentKey, notification.FolderId);
                i.PutExtra(ContactActivity.ContactIdIntentKey, notification.ObjectId);
                StartActivity(i);
            }
            if (notification.ObjectType == ObjectType.Shortcode)
            {
                var i = new Intent(Activity, typeof(ShortcodeActivity));
                i.PutExtra(ShortcodeActivity.FolderIdIntentKey, notification.FolderId);
                i.PutExtra(ShortcodeActivity.ShortcodeIdIntentKey, notification.ObjectId);
                StartActivity(i);
            }
        }

        #endregion

        #region Private methods

        async void MarkAllAsRead()
        {
            try
            {
                await Managers.NotificationsManager.MarkAsRead(adapter.Items);
                adapter.NotifyItemRangeChanged(0, adapter.ItemCount);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Marking notifications as read failed", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        #endregion

        #region State

        class NotificationsFragmentState : IRetainableState
        {

            public ObjectType[] ObjectTypes { get; set; }

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
                    return notificationsInView;
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

                cpvh.Title = n.Type == EventType.NewObjectCreated ? string.Empty : n.Title;

                var splitMessage = n.Message.Split('\n');

                cpvh.MessageFirstLine = splitMessage.ElementAtOrDefault(0);
                cpvh.MessageSecondLine = splitMessage.ElementAtOrDefault(1);

                cpvh.DateTime = n.DateTimeTimestamp
                    .ConvertTimestampMillisecondsToDateTime()
                    .ConvertUtcToUserTime()
                    .ConvertDateTimeToTimestampMilliseconds()
                    .FormatUserTimestampAsCompactShortDateTimeString(context);
                cpvh.UnreadIndicator = !n.IsRead;
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

            public int GetPosition(Notification notification)
            {
                return GetPosition(notification.Id);
            }

            public int GetPosition(int notificationId)
            {
                var position = -1;
                for (var i = 0; i < notificationsInView.Count; i++)
                {
                    if (notificationsInView[i].Id == notificationId)
                    {
                        position = i;
                        break;
                    }
                }
                return position;
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
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        titleTextView.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        titleTextView.Visibility = ViewStates.Visible;
                        titleTextView.Text = value;
                    }
                }
            }

            public string MessageFirstLine
            {
                set
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        messageFirstLine.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        messageFirstLine.Visibility = ViewStates.Visible;
                        messageFirstLine.Text = value;
                    }
                }
            }

            public string MessageSecondLine
            {
                set
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        messageSecondLine.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        messageSecondLine.Visibility = ViewStates.Visible;
                        messageSecondLine.Text = value;
                    }
                }
            }

            public string DateTime
            {
                set
                {
                    dateTimeTextView.Text = value;
                }
            }

            public bool UnreadIndicator
            {
                set
                {
                    unreadImageView.Visibility = value ? ViewStates.Visible : ViewStates.Invisible;
                }
            }

            readonly AppCompatImageView unreadImageView;
            readonly AppCompatTextView titleTextView;
            readonly AppCompatTextView messageFirstLine;
            readonly AppCompatTextView messageSecondLine;
            readonly AppCompatTextView dateTimeTextView;

            public NotificationViewHolder(View itemView)
                    : base(itemView)
            {
                unreadImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_notification_unread);
                titleTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_notification_title);
                messageFirstLine = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_notification_message_first_line);
                messageSecondLine = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_notification_message_second_line);
                dateTimeTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_notification_date);
            }
        }

        #endregion
    }
}

