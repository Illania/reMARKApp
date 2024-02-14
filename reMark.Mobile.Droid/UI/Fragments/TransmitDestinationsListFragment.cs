using System.Collections.Generic;
using System.Linq;
using Android.Graphics;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Content;
using AndroidX.RecyclerView.Widget;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Manager;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.Droid.Ui.Activities;
using reMark.Mobile.Droid.Ui.Common;
using Color = Android.Graphics.Color;
using View = Android.Views.View;

namespace reMark.Mobile.Droid.Ui.Fragments
{
    public class TransmitDestinationsListFragment : BaseFragment
    {
        private const string DocumentIdBundleKey = "DocumentId_d3ded4d4-be9a-49e6-8626-84cb175c12bb";
        private const string ReferenceNumberBundleKey = "ReferenceNumber_d3ded4d4-be9a-49e6-8626-84cb175c12bb";


        public int DocumentId { get; set; }
        public string ReferenceNumber { get; set; }

        RecyclerView recyclerView;
        TransmitDestinationsListViewAdapter adapter;

        public static (TransmitDestinationsListFragment fragment, string tag) NewInstance(int? docId, string reference)
        {
            var args = new Bundle();

            if(docId != null)
                args.PutString(DocumentIdBundleKey, Serializer.Serialize(docId));

            if (reference != null)
                args.PutString(ReferenceNumberBundleKey, Serializer.Serialize(reference));

            var fragment = new TransmitDestinationsListFragment();
            var tag = $"{nameof(TransmitDestinationsListFragment)} [document.id={docId}, document.reference={reference}]";

            fragment.Arguments = args;

            return (fragment, tag);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Arguments.ContainsKey(DocumentIdBundleKey))
                DocumentId = Serializer.Deserialize<int>(Arguments.GetString(DocumentIdBundleKey));


            if (Arguments.ContainsKey(ReferenceNumber))
                ReferenceNumber = Serializer.Deserialize<string>(Arguments.GetString(ReferenceNumberBundleKey));

        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(TransmitDestinationsListFragment)}]");

            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.white)));

            adapter = new TransmitDestinationsListViewAdapter(this);
            recyclerView.SetAdapter(adapter);

            HasOptionsMenu = false;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = GetString(Resource.String.transmit_destinations);

            CommonConfig.Logger.Info($"Created {nameof(TransmitDestinationsListFragment)}");
        }

        public override void OnResume()
        {
            base.OnResume();

            if (adapter.ItemCount < 1)
            {
                CommonConfig.Logger.Info($"Refreshing {nameof(TransmitDestinationsListFragment)}");
                RefreshData();
            }
        }

        public async void RefreshData()
        {
            var transmits = await Managers.DocumentsManager.GetDocumentTransmitInfoAsync(DocumentId);
            var transmit = transmits.FirstOrDefault();
            if (transmit != null)
                adapter.SetItems(transmit.Destinations);

        }

        public void DestinationSelected(TransmitDestination destination)
        {
            StartActivity(DeliveryReportActivity.CreateIntent(Context, destination, ReferenceNumber));
        }

        class TransmitDestinationsListViewAdapter : RecyclerView.Adapter
        {
            public override int ItemCount => Items.Count;

            public List<TransmitDestination> Items { get; } = new List<TransmitDestination>(500);

            readonly TransmitDestinationsListFragment parent;

            public TransmitDestinationsListViewAdapter(TransmitDestinationsListFragment parent)
            {
                this.parent = parent;
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var transmitDestination = Items[position];
                var status = transmitDestination.Status.StatusDetail;
                var lvh = holder as TransmitDestinationViewHolder;
                var isFailed = status == DestinationStatusDetail.Cancelled || status == DestinationStatusDetail.CancelRequested
                    || status == DestinationStatusDetail.FailedBounced || status == DestinationStatusDetail.SystemError;
        
                lvh.ItemView.SetOnClickListener(new ActionOnClickListener(() => HandleClick(transmitDestination)));
                lvh.FailIndicator = isFailed;
                lvh.OutgoingIndicator = !isFailed;
                lvh.Name = transmitDestination.Address;
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_transmit_destination, parent, false);
                return new TransmitDestinationViewHolder(itemView);
            }

            public void SetItems(List<TransmitDestination> destinations)
            {
                var count = Items.Count;
                Items.AddRange(destinations);
                NotifyItemRangeInserted(count, destinations.Count);
            }

            void HandleClick(TransmitDestination destination)
            {
                parent.DestinationSelected(destination);
            }
        }

        class TransmitDestinationViewHolder : RecyclerView.ViewHolder
        {
            readonly AppCompatTextView addressView;
            readonly AppCompatImageView outgoingImageView;
            readonly AppCompatImageView failImageView;

            public bool OutgoingIndicator { set => outgoingImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }
            public bool FailIndicator { set => failImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }
            public string Name { set => addressView.Text = value; }

            public TransmitDestinationViewHolder(View itemView)
                : base(itemView)
            {
               addressView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_transmit_destination);
               outgoingImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_status_outgoing);
               failImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_status_failed);

            }
        }
    }
}
