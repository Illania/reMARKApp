
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class RecentAddressesListFragment : RetainableStateFragment
    {
        RecyclerView recyclerView;

        RecentAddressesListAdapter adapter;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(RecentAddressesListFragment)}");

            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            var emptyView = rootView.FindViewById<AppCompatTextView>(Resource.Id.empty_view);
            emptyView.SetText(Resource.String.no_categories);

            var refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            refreshLayout.Enabled = false;

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));

            adapter = new RecentAddressesListAdapter();
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

            ((AppCompatActivity) Activity).SupportActionBar.Title = GetString(Resource.String.recent_addresses);
            ((AppCompatActivity) Activity).SupportActionBar.Subtitle = null;

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

        async Task RefreshView()
        {
            try
            {
                var addresses = await Managers.DocumentsManager.GetRecentAddressesAsync();
                adapter.SetItems(addresses);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while retrieving recent addresses", ex);
                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        #region Retainable State

        public override string GenerateTag()
        {
            return $"{nameof(RecentAddressesListFragment)}";
        }

        #endregion

        class RecentAddressesListAdapter : RecyclerView.Adapter
        {
            public override int ItemCount => Items.Count;

            public List<RecentAddress> Items { get; } = new List<RecentAddress>();

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var viewHolder = holder as RecentAddressViewHolder;
                var ra = Items[position];

                viewHolder.Address = ra.Address;
                viewHolder.Name = ra.Name;
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_recent_addresses, parent, false);
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
                    addressTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_recent_addresses_address);
                    nameTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_recent_addresses_name);
                }
            }
        }

    }
}
