using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Extensions;
using reMark.Mobile.Common.Manager;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.Droid.Ui.Activities;
using reMark.Mobile.Droid.Ui.Common;
using FastScrollRecycler;
using View = Android.Views.View;
using Contact = reMark.Mobile.Common.Model.Contact;
using reMark.Mobile.Droid.UI;

namespace reMark.Mobile.Droid.Ui.Fragments
{
    public class LinkedEmailListFragment : BaseFragment, IMenuItemOnActionExpandListener, SearchView.IOnQueryTextListener
    {
        readonly Handler searchHandler = new Handler();

        public Folder Folder { get; set; }
        public Contact Contact { get; set; }
        public ContactPreview ContactPreview { get; set; }

        RecyclerView recyclerView;
        LinkedEmailListAdapter adapter;
        LinkedEmailListAdapter searchAdapter;
        SearchView searchView;
        IMenu menu;

        public static (LinkedEmailListFragment fragment, string tag) NewInstance(Folder folder = null, Contact contact = null, ContactPreview contactPreview = null)
        {
            var fragment = new LinkedEmailListFragment();
            var tag = $"{nameof(LinkedEmailListFragment)}";

            return (fragment, tag);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(LinkedEmailListFragment)}");

            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            var emptyView = rootView.FindViewById<AppCompatTextView>(Resource.Id.empty_view);
            emptyView.SetText(Resource.String.no_phonebook);

            var refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            refreshLayout.Enabled = false;

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));

            adapter = new LinkedEmailListAdapter();
            adapter.ItemClicked += Adapter_ItemClicked;
            adapter.RegisterAdapterDataObserver(new LambdaEmptyAdapterObserver(() =>
            {
                if (recyclerView.GetAdapter() != adapter)
                    return;

                emptyView.Visibility = adapter.ItemCount < 1 ? ViewStates.Visible : ViewStates.Gone;
                recyclerView.Visibility = adapter.ItemCount > 0 ? ViewStates.Visible : ViewStates.Gone;
            }));
            recyclerView.SetAdapter(adapter);

            searchAdapter = new LinkedEmailListAdapter();
            searchAdapter.ItemClicked += Adapter_ItemClicked;

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.phonebook);
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = null;

            CommonConfig.Logger.Info($"Created {nameof(LinkedEmailListFragment)}");
        }

        public override async void OnResume()
        {
            base.OnResume();

            if (adapter.ItemCount < 1)
            {
                CommonConfig.Logger.Info($"Refreshing {nameof(PhonebookContactsListFragment)}");
                await RefreshView();
            }
        }

        async Task RefreshView()
        {
                if (Contact == null)
                    Contact = await Managers.ContactsManager.GetContactAsync(Folder, ContactPreview.Id);
                if (Contact.CommunicationAddresses.Any())
                    adapter.SetItems(Contact.CommunicationAddresses.OrderBy(c => c.Address.SafeSubstring(0, 1)));
        }

        void Adapter_ItemClicked(object sender, CommunicationAddress ca)
        {
            var intent = new Intent();
            intent.PutExtra(LinkedEmailListActivity.RecipientResultKey, Serializer.Serialize(ca));
            Activity.SetResult(Result.Ok, intent);
            Activity?.Finish();
        }

        #region Options Menu / Filtering
        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            this.menu = menu;

            inflater.Inflate(Resource.Menu.menu_main, menu);
            var filterItem = menu.FindItem(Resource.Id.action_filter);
            filterItem.ApplyColor(filterItem.TitleFormatted.ToString(), this);
            filterItem.SetOnActionExpandListener(this);

            searchView = (SearchView)filterItem.ActionView;
            searchView.QueryHint = GetString(Resource.String.filter);
            searchView.SetOnQueryTextListener(this);
        }

        public bool OnMenuItemActionExpand(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_filter)
            {
                recyclerView.SwapAdapter(searchAdapter, true);
                (this as SearchView.IOnQueryTextListener).OnQueryTextChange(string.Empty);
                return true;
            }

            return false;
        }

        public bool OnMenuItemActionCollapse(IMenuItem item)
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
                    searchAdapter.ReplaceItems(adapter.Items.Where(recipient => MatchesQuery(recipient, newText)).ToList());
            }, 500);

            return false;
        }

        bool SearchView.IOnQueryTextListener.OnQueryTextSubmit(string query)
        {
            return false;
        }

        static bool MatchesQuery(CommunicationAddress ca, string query)
        {
            if (ca.Address?.ContainsCaseInsensitive(query) ?? false)
                return true;

            return false;
        }

        #endregion

        class LinkedEmailListAdapter : RecyclerView.Adapter, ISectionedAdapter
        {
            public override int ItemCount => Items.Count;
            public List<CommunicationAddress> Items { get; } = new List<CommunicationAddress>();

            public event EventHandler<CommunicationAddress> ItemClicked = delegate { };

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var viewHolder = holder as LinkedEmailListViewHolder;
                var recipient = Items[position];

                viewHolder.ItemView.SetOnClickListener(new ActionOnClickListener(() => ItemClicked(this, recipient)));
                viewHolder.Address = recipient.Address;
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var itemView = LayoutInflater.FromContext(parent.Context).Inflate(Resource.Layout.list_item_recipients, parent, false);
                return new LinkedEmailListViewHolder(itemView);
            }

            string ISectionedAdapter.GetSectionName(int position)
            {
                return Items[position].Address?.SafeSubstring(0, 1)?.ToUpper() ?? "";
            }

            public void SetItems(IEnumerable<CommunicationAddress> recipients)
            {
                Items.Clear();
                Items.AddRange(recipients);

                NotifyItemRangeInserted(0, Items.Count);
            }

            public void Clear()
            {
                var size = Items.Count;
                Items.Clear();
                NotifyItemRangeRemoved(0, size);
            }

            public void ReplaceItems(List<CommunicationAddress> items)
            {
                Clear();
                AppendItems(items);
            }

            public void AppendItems(List<CommunicationAddress> items)
            {
                var count = Items.Count;
                Items.AddRange(items);
                NotifyItemRangeInserted(count, items.Count);
            }

            class LinkedEmailListViewHolder : RecyclerView.ViewHolder
            {
                public string Address { set => addressTextView.Text = value; }
                readonly AppCompatTextView addressTextView;
                public LinkedEmailListViewHolder(View itemView) : base(itemView)
                {
                    addressTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_recipients_address);
                }
            }
        }

    }
}
