//
// Project: Mark5.Mobile.Droid
// File: ShortcodeSearchResultsFragment.cs
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

    public class ShortcodesSearchResultsFragment : RetainableStateFragment
    {

        public SearchShortcodesCriteria Criteria { get; set; }

        int searchId;

        SwipeRefreshLayout refreshLayout;
        RecyclerView recyclerView;
        ShortcodeSearchResultsAdapter adapter;

        #region Fragment overrides

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(ShortcodesSearchResultsFragment)} [criteria={Criteria}]...");

            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            refreshLayout.SetColorSchemeResources(Resource.Color.lightbrown, Resource.Color.brown);
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

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.search_shortcodes_result);

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
                SearchId = searchId,
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
                searchId = dlfs.SearchId;
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

                refreshLayout.Post(() => refreshLayout.Refreshing = true); //Bug: fixed in support library v 24.2.0 (issue 77712)

                var searchResults = await Managers.SearchManager.SearchShortcodesAsync(Criteria);
                searchId = searchResults.SearchId;
                adapter.AppendItems(searchResults.ShortcodePreviews);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Downloading shortcodes failed [criteria={Criteria}]", ex);

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

        void Adapter_ItemClicked(object sender, ShortcodePreview shortcodePreview)
        {
            //var i = new Intent(Activity, typeof(ShortcodeActivity));
            //i.PutExtra(ShortcodeActivity.SearchIdIntentKey, searchId);
            //i.PutExtra(ShortcodeActivity.ShortcodePreviewIntentKey, SerializationUtils.Serialize(shortcodePreview));
            //i.PutExtra(ShortcodeActivity.ReadOnlyModeIntentKey, true);
            //StartActivity(i);
        }

        #endregion

        #region State

        class ShortcodeSearchResultsFragmentState : IRetainableState
        {

            public int SearchId { get; set; }

            public SearchShortcodesCriteria Criteria { get; set; }

            public List<ShortcodePreview> ShortcodePreviews { get; set; }
        }

        #endregion

        #region RecyclerView Adapter/ViewHolder

        class ShortcodeSearchResultsAdapter : RecyclerView.Adapter
        {

            public List<ShortcodePreview> Items
            {
                get
                {
                    return shortcodePreviewsInView.ToList();
                }
            }

            public override int ItemCount
            {
                get
                {
                    return shortcodePreviewsInView.Count;
                }
            }

            readonly List<ShortcodePreview> shortcodePreviewsInView = new List<ShortcodePreview>(1000);
            readonly Dictionary<int, ShortcodePreview> selectedShortcodesInView = new Dictionary<int, ShortcodePreview>();

            public event EventHandler<ShortcodePreview> ItemClicked = delegate { };

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var cpvh = holder as ShortcodePreviewViewHolder;
                if (cpvh == null) return;

                var cp = shortcodePreviewsInView[position];

                cpvh.ItemView.SetOnClickListener(new ActionOnClickListener(() => ItemClicked(this, cp)));

                cpvh.Name = cp.Name;
                cpvh.Description = cp.Description;

                cpvh.Selected = selectedShortcodesInView.ContainsKey(cp.Id);
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_shortcodes, parent, false);
                return new ShortcodePreviewViewHolder(itemView);
            }

            public void AppendItems(List<ShortcodePreview> items)
            {
                var count = shortcodePreviewsInView.Count;
                shortcodePreviewsInView.AddRange(items);
                NotifyItemRangeInserted(count, items.Count);
            }
        }

        class ShortcodePreviewViewHolder : RecyclerView.ViewHolder
        {

            static readonly int[] colors = { Resource.Color.darkerblue, Resource.Color.darkblue, Resource.Color.blue };

            public string Name
            {
                set
                {
                    nameTextView.Text = value;
                    letterTextView.Text = value.SafeSubstring(0, 1).ToUpper();

                    var sd = new ShapeDrawable(new OvalShape());
                    sd.Paint.Color = new Color(ContextCompat.GetColor(ItemView.Context, colors[Math.Abs(value.GetHashCode() % colors.Length)]));
                    letterTextView.Background = sd;
                }
            }

            public string Description
            {
                set
                {
                    descTextView.Text = value;
                    descTextView.Visibility = string.IsNullOrWhiteSpace(value) ? ViewStates.Gone : ViewStates.Visible;
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
            readonly AppCompatTextView nameTextView;
            readonly AppCompatTextView descTextView;
            readonly View selectedOverlay;

            public ShortcodePreviewViewHolder(View itemView)
                    : base(itemView)
            {
                letterTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_shortcode_letter);
                nameTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_shortcode_name);
                descTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_shortcode_desc);
                selectedOverlay = itemView.FindViewById<View>(Resource.Id.selected_overlay);
            }
        }

        #endregion
    }
}

