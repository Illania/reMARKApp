
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using FastScrollRecycler;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class PhonebookContactsListFragment : RetainableStateFragment
    {
        RecyclerView recyclerView;
        PhonebookContactsListAdapter adapter;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(PhonebookContactsListFragment)}");

            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            var emptyView = rootView.FindViewById<AppCompatTextView>(Resource.Id.empty_view);
            emptyView.SetText(Resource.String.no_phonebook);

            var refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            refreshLayout.Enabled = false;

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));

            adapter = new PhonebookContactsListAdapter();
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

            ((AppCompatActivity) Activity).SupportActionBar.Title = GetString(Resource.String.phonebook);
            ((AppCompatActivity) Activity).SupportActionBar.Subtitle = null;

            CommonConfig.Logger.Info($"Created {nameof(PhonebookContactsListFragment)}");
        }

        public override void OnResume()
        {
            base.OnResume();

            if (adapter.ItemCount < 1)
            {
                CommonConfig.Logger.Info($"Refreshing {nameof(PhonebookContactsListFragment)}");
                RefreshView();
            }
        }

        void RefreshView()
        {
            List<Recipient> contacts = null;

            Task.Run(() =>
              {
                  contacts = CommonConfig.Phonebook.GetPhonebookContacts();
              }).ContinueWith(t =>
               {
                   Activity.RunOnUiThread(async () =>
                   {
                       if (t.IsFaulted)
                       {
                           var ex = t.Exception.InnerException;
                           CommonConfig.Logger.Error($"Error while retrieving phonebook contacts", ex);
                           await Dialogs.ShowErrorDialogAsync(Activity, ex);
                       }

                       if (contacts == null)
                       {
                           await Dialogs.ShowConfirmDialogAsync(Activity, Resource.String.phonebook_contacts_no_access_title,
                                                                Resource.String.phonebook_contacts_no_access_content);
                           Activity?.Finish();
                       }
                       else
                       {
                           adapter.SetItems(contacts.OrderBy(c => c.Name.SafeSubstring(0, 1)));
                       }
                   });
               });
        }

        void Adapter_ItemClicked(object sender, Recipient r)
        {
            var intent = new Intent();
            intent.PutExtra(PhonebookContactsListActivity.RecipientResultKey, Serializer.Serialize(r));
            Activity.SetResult(Result.Ok, intent);
            Activity?.Finish();
        }

        #region Retainable state

        public override string GenerateTag()
        {
            return $"{nameof(PhonebookContactsListFragment)}";
        }

        #endregion

        class PhonebookContactsListAdapter : RecyclerView.Adapter, ISectionedAdapter
        {
            public List<Recipient> Items { get; } = new List<Recipient>();
            public override int ItemCount => Items.Count;

            public event EventHandler<Recipient> ItemClicked = delegate { };

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var viewHolder = holder as PhonebookContactViewHolder;
                var recipient = Items[position];

                viewHolder.ItemView.SetOnClickListener(new ActionOnClickListener(() => ItemClicked(this, recipient)));

                viewHolder.Address = recipient.Address;
                viewHolder.Name = recipient.Name;
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var itemView = LayoutInflater.FromContext(parent.Context).Inflate(Resource.Layout.list_item_recipients, parent, false);
                return new PhonebookContactViewHolder(itemView);
            }

            public void SetItems(IEnumerable<Recipient> recipients)
            {
                Items.Clear();
                Items.AddRange(recipients);

                NotifyItemRangeInserted(0, Items.Count);
            }

            string ISectionedAdapter.GetSectionName(int position)
            {
                return Items[position].Name?.SafeSubstring(0, 1)?.ToUpper() ?? "";
            }

            class PhonebookContactViewHolder : RecyclerView.ViewHolder
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

                public PhonebookContactViewHolder(View itemView) : base(itemView)
                {
                    addressTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_recipients_address);
                    nameTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_recipients_name);
                }
            }
        }

    }
}
