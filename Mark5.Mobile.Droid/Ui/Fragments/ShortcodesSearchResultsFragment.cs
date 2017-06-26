using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using FastScrollRecycler;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class ShortcodesSearchResultsFragment : RetainableStateFragment
    {
        public SearchShortcodesCriteria Criteria { get; set; }
        public Action CloseRequest { get; set; }

        SwipeRefreshLayout refreshLayout;
        RecyclerView recyclerView;
        ShortcodeSearchResultsAdapter adapter;

        #region Fragment overrides

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(ShortcodesSearchResultsFragment)} [criteria={Criteria}]...");

            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            refreshLayout.SetColorSchemeResources(Resource.Color.blue, Resource.Color.darkerblue);
            refreshLayout.Enabled = false;

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));

            adapter = new ShortcodeSearchResultsAdapter();
            adapter.ItemClicked += Adapter_ItemClicked;
            recyclerView.SetAdapter(adapter);

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity) Activity).SupportActionBar.Title = GetString(Resource.String.search_shortcodes_result);
            ((AppCompatActivity) Activity).SupportActionBar.Subtitle = null;

            CommonConfig.Logger.Info($"Created {nameof(ShortcodesSearchResultsFragment)} [criteria={Criteria}]");
        }

        public override async void OnResume()
        {
            base.OnResume();

            CommonConfig.Logger.Info($"Resuming {nameof(ShortcodesSearchResultsFragment)} [criteria={Criteria}]...");

            if (adapter.ItemCount < 1)
            {
                CommonConfig.Logger.Info($"No elements - will refresh...");

                await RefreshData();
            }
        }

        public override void OnPause()
        {
            base.OnPause();

            CommonConfig.Logger.Info($"Pausing {nameof(ShortcodesSearchResultsFragment)} [criteria={Criteria}]...");
        }

        #endregion

        #region RetainableStateFragment overrides

        public override IRetainableState OnRetainInstanceState()
        {
            CommonConfig.Logger.Info($"Retaining state [criteria={Criteria}, shortcodePreviews.Count={adapter?.ItemCount}]...");

            return new ShortcodeSearchResultsFragmentState
            {
                Criteria = Criteria,
                ShortcodePreviews = adapter.Items
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var dlfs = restoredState as ShortcodeSearchResultsFragmentState;
            if (dlfs != null)
            {
                CommonConfig.Logger.Info($"Restoring state [dlfs.criteria={dlfs.Criteria}, dlfs.items.count={dlfs.ShortcodePreviews?.Count}]...");

                Criteria = dlfs.Criteria;
                adapter.AppendItems(dlfs.ShortcodePreviews);
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(ShortcodesSearchResultsFragment)}]";
        }

        #endregion

        #region Refreshing

        async Task RefreshData()
        {
            try
            {
                CommonConfig.Logger.Info($"Refresh running...");

                refreshLayout.Refreshing = true;

                var shortcodePreviews = await Managers.SearchManager.SearchShortcodesAsync(Criteria);

                if (shortcodePreviews.Count < 1)
                {
                    await Dialogs.ShowConfirmDialogAsync(Activity, Resource.String.no_results, Resource.String.no_results_shortcodes);
                    if (CloseRequest != null)
                        CloseRequest();
                    return;
                }

                if (CommonConfig.Logger.IsDebugEnabled())
                    CommonConfig.Logger.Debug($"Retrieved {shortcodePreviews.Count} items");

                adapter.AppendItems(shortcodePreviews);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Downloading shortcodes failed [criteria={Criteria}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);

                if (CloseRequest != null)
                    CloseRequest();
            }
            finally
            {
                refreshLayout.Refreshing = false;

                CommonConfig.Logger.Info($"Refresh finished");
            }
        }

        #endregion

        #region Adapter callbacks

        void Adapter_ItemClicked(object sender, ShortcodePreview shortcodePreview)
        {
            var i = new Intent(Activity, typeof(ShortcodeActivity));
            i.PutExtra(ShortcodeActivity.ShortcodePreviewIntentKey, Serializer.Serialize(shortcodePreview));
            StartActivity(i);
        }

        #endregion

        #region State

        class ShortcodeSearchResultsFragmentState : IRetainableState
        {
            public SearchShortcodesCriteria Criteria { get; set; }

            public List<ShortcodePreview> ShortcodePreviews { get; set; }
        }

        #endregion

        #region RecyclerView Adapter/ViewHolder

        class ShortcodeSearchResultsAdapter : RecyclerView.Adapter, ISectionedAdapter
        {
            public List<ShortcodePreview> Items { get; } = new List<ShortcodePreview>(1000);

            public override int ItemCount => Items.Count;

            readonly Dictionary<int, ShortcodePreview> selectedShortcodesInView = new Dictionary<int, ShortcodePreview>();

            public event EventHandler<ShortcodePreview> ItemClicked = delegate { };

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var cpvh = holder as ShortcodePreviewViewHolder;
                if (cpvh == null)
                    return;

                var cp = Items[position];

                cpvh.ItemView.SetOnClickListener(new ActionOnClickListener(() => ItemClicked(this, cp)));

                cpvh.Name = cp.Name;

                cpvh.Selected = selectedShortcodesInView.ContainsKey(cp.Id);
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_shortcodes, parent, false);
                return new ShortcodePreviewViewHolder(itemView);
            }

            public void AppendItems(List<ShortcodePreview> items)
            {
                var count = Items.Count;
                Items.AddRange(items);
                NotifyItemRangeInserted(count, items.Count);
            }

            string ISectionedAdapter.GetSectionName(int position)
            {
                return Items[position].Name?.SafeSubstring(0, 1)?.ToUpper() ?? "";
            }
        }

        class ShortcodePreviewViewHolder : RecyclerView.ViewHolder
        {
            public string Name { set => nameTextView.Text = value; }

            public bool Selected { set => selectedOverlay.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }

            readonly AppCompatTextView nameTextView;
            readonly View selectedOverlay;

            public ShortcodePreviewViewHolder(View itemView)
                : base(itemView)
            {
                nameTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_shortcode_name);
                selectedOverlay = itemView.FindViewById<View>(Resource.Id.selected_overlay);
            }
        }

        #endregion
    }
}