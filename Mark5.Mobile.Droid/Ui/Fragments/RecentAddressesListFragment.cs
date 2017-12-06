
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class RecentAddressesListFragment : BaseFragment
    {
        const string RecentAddressesKey = "RecentAddresses_a0d328b3-7c18-4a18-b62e-f713cc528cc1";

        RecyclerView recyclerView;

        RecentAddressesListAdapter adapter;

        List<RecentAddress> recentAddresses;

        public static (RecentAddressesListFragment fragment, string tag) NewInstance()
        {
            var fragment = new RecentAddressesListFragment();
            var tag = $"{nameof(RecentAddressesListFragment)}";

            return (fragment, tag);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (savedInstanceState?.ContainsKey(RecentAddressesKey) == true)
                recentAddresses = Serializer.Deserialize<List<RecentAddress>>(savedInstanceState.GetString(RecentAddressesKey));

            CommonConfig.Logger.Info($"Creating {nameof(RecentAddressesListFragment)}");

            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            var emptyView = rootView.FindViewById<AppCompatTextView>(Resource.Id.empty_view);
            emptyView.SetText(Resource.String.no_recent_addresses);

            var refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            refreshLayout.Enabled = false;

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));

            adapter = new RecentAddressesListAdapter();
            adapter.ItemClicked += Adapter_ItemClicked;
            adapter.RegisterAdapterDataObserver(new LambdaEmptyAdapterObserver(() =>
            {
                if (recyclerView.GetAdapter() != adapter)
                    return;

                emptyView.Visibility = adapter.ItemCount < 1 ? ViewStates.Visible : ViewStates.Gone;
                recyclerView.Visibility = adapter.ItemCount > 0 ? ViewStates.Visible : ViewStates.Gone;
            }));
            recyclerView.SetAdapter(adapter);

            HasOptionsMenu = false;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.recent_addresses);
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = null;

            CommonConfig.Logger.Info($"Created {nameof(RecentAddressesListFragment)}");
        }

        public override async void OnResume()
        {
            base.OnResume();

            if (adapter.ItemCount < 1)
            {
                CommonConfig.Logger.Info($"Refreshing {nameof(RecentAddressesListFragment)}");
                await RefreshView();
            }
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            if (recentAddresses != null)
                outState.PutString(RecentAddressesKey, Serializer.Serialize(recentAddresses));
        }

        async Task RefreshView()
        {
            try
            {
                if (recentAddresses == null)
                    recentAddresses = await Managers.DocumentsManager.GetRecentAddressesAsync();

                adapter.SetItems(recentAddresses);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while retrieving recent addresses", ex);
                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        void Adapter_ItemClicked(object sender, RecentAddress ra)
        {
            var intent = new Intent();
            intent.PutExtra(RecentAddressesListActivity.RecipientResultKey, Serializer.Serialize(new Recipient(ra)));
            Activity.SetResult(Result.Ok, intent);
            Activity?.Finish();
        }

        class RecentAddressesListAdapter : RecyclerView.Adapter
        {
            public override int ItemCount => Items.Count;

            public List<RecentAddress> Items { get; } = new List<RecentAddress>();

            public event EventHandler<RecentAddress> ItemClicked = delegate { };

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var viewHolder = holder as RecentAddressViewHolder;
                var ra = Items[position];

                viewHolder.ItemView.SetOnClickListener(new ActionOnClickListener(() => ItemClicked(this, ra)));

                viewHolder.Address = ra.Address;
                viewHolder.Name = ra.Name;
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_recipients, parent, false);
                return new RecentAddressViewHolder(itemView);
            }

            public void SetItems(List<RecentAddress> recentAddresses)
            {
                Items.Clear();
                Items.AddRange(recentAddresses);

                NotifyItemRangeInserted(0, ItemCount);
            }

            class RecentAddressViewHolder : RecyclerView.ViewHolder
            {
                public string Address { set => addressTextView.Text = value; }

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

                readonly AppCompatTextView addressTextView;
                readonly AppCompatTextView nameTextView;

                public RecentAddressViewHolder(View itemView) : base(itemView)
                {
                    addressTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_recipients_address);
                    nameTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_recipients_name);
                }
            }
        }

    }
}
