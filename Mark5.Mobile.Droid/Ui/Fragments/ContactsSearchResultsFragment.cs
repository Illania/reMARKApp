//
// Project: Mark5.Mobile.Droid
// File: ContactSearchResultsFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
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
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{

    public class ContactsSearchResultsFragment : RetainableStateFragment
    {

        public SearchContactsCriteria Criteria { get; set; }
        public Action CloseRequest { get; set; }

        int searchId;

        SwipeRefreshLayout refreshLayout;
        RecyclerView recyclerView;
        ContactSearchResultsAdapter adapter;

        #region Fragment overrides

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(ContactsSearchResultsFragment)} [criteria={Criteria}]...");

            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            refreshLayout.SetColorSchemeResources(Resource.Color.lightbrown, Resource.Color.brown);
            refreshLayout.Enabled = false;

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));

            adapter = new ContactSearchResultsAdapter();
            adapter.ItemClicked += Adapter_ItemClicked;
            recyclerView.SetAdapter(adapter);

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.search_contacts_result);

            CommonConfig.Logger.Info($"Created {nameof(ContactsSearchResultsFragment)} [criteria={Criteria}]");
        }

        public override async void OnResume()
        {
            base.OnResume();

            CommonConfig.Logger.Info($"Resuming {nameof(ContactsSearchResultsFragment)} [criteria={Criteria}]...");

            if (adapter.ItemCount < 1)
            {
                CommonConfig.Logger.Info($"No elements - will refresh...");

                await RefreshData();
            }
        }

        public override void OnPause()
        {
            base.OnPause();

            CommonConfig.Logger.Info($"Pausing {nameof(ContactsSearchResultsFragment)} [criteria={Criteria}]...");
        }

        #endregion

        #region RetainableStateFragment overrides

        public override IRetainableState OnRetainInstanceState()
        {
            CommonConfig.Logger.Info($"Retaining state [criteria={Criteria}, contactPreviews.Count={adapter?.ItemCount}]...");

            return new ContactSearchResultsFragmentState
            {
                Criteria = Criteria,
                SearchId = searchId,
                ContactPreviews = adapter.Items
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var dlfs = restoredState as ContactSearchResultsFragmentState;
            if (dlfs != null)
            {
                CommonConfig.Logger.Info($"Restoring state [dlfs.criteria={dlfs.Criteria}, dlfs.items.count={dlfs.ContactPreviews?.Count}]...");

                Criteria = dlfs.Criteria;
                searchId = dlfs.SearchId;
                adapter.AppendItems(dlfs.ContactPreviews);
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(ContactsSearchResultsFragment)}]";
        }

        #endregion

        #region Refreshing

        async Task RefreshData()
        {
            try
            {
                CommonConfig.Logger.Info($"Refresh running...");

                refreshLayout.Post(() => refreshLayout.Refreshing = true); //Bug: fixed in support library v 24.2.0 (issue 77712)

                var searchResults = await Managers.SearchManager.SearchContactsAsync(Criteria);
                searchId = searchResults.SearchId;
                adapter.AppendItems(searchResults.ContactPreviews);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Downloading contacts failed [criteria={Criteria}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);

                if (CloseRequest != null) CloseRequest();
            }
            finally
            {
                refreshLayout.Post(() => refreshLayout.Refreshing = false); //Bug: fixed in support library v 24.2.0 (issue 77712)

                CommonConfig.Logger.Info($"Refresh finished");
            }
        }

        #endregion

        #region Adapter callbacks

        void Adapter_ItemClicked(object sender, ContactPreview contactPreview)
        {
            var i = new Intent(Activity, typeof(ContactActivity));
            i.PutExtra(ContactActivity.SearchIdIntentKey, searchId);
            i.PutExtra(ContactActivity.ContactPreviewIntentKey, SerializationUtils.Serialize(contactPreview));
            i.PutExtra(ContactActivity.ReadOnlyModeIntentKey, true);
            StartActivity(i);
        }

        #endregion

        #region State

        class ContactSearchResultsFragmentState : IRetainableState
        {

            public int SearchId { get; set; }

            public SearchContactsCriteria Criteria { get; set; }

            public List<ContactPreview> ContactPreviews { get; set; }
        }

        #endregion

        #region RecyclerView Adapter/ViewHolder

        class ContactSearchResultsAdapter : RecyclerView.Adapter
        {

            public List<ContactPreview> Items
            {
                get
                {
                    return contactPreviewsInView.ToList();
                }
            }

            public override int ItemCount
            {
                get
                {
                    return contactPreviewsInView.Count;
                }
            }

            readonly List<ContactPreview> contactPreviewsInView = new List<ContactPreview>(1000);
            readonly Dictionary<int, ContactPreview> selectedContactsInView = new Dictionary<int, ContactPreview>();

            public event EventHandler<ContactPreview> ItemClicked = delegate { };

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var cpvh = holder as ContactPreviewViewHolder;
                if (cpvh == null) return;

                var cp = contactPreviewsInView[position];

                cpvh.ItemView.SetOnClickListener(new ActionOnClickListener(() => ItemClicked(this, cp)));

                cpvh.Name = cp.Name;
                cpvh.Description = cp.Description;
                cpvh.Categories = cp.Categories;

                cpvh.Selected = selectedContactsInView.ContainsKey(cp.Id);
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_contacts, parent, false);
                return new ContactPreviewViewHolder(itemView);
            }

            public void AppendItems(List<ContactPreview> items)
            {
                var count = contactPreviewsInView.Count;
                contactPreviewsInView.AddRange(items);
                NotifyItemRangeInserted(count, items.Count);
            }
        }

        class ContactPreviewViewHolder : RecyclerView.ViewHolder
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

            public List<Category> Categories
            {
                set
                {
                    categoriesLayout.RemoveAllViews();

                    foreach (var hexColor in value.Select(c => c.HexColor))
                    {
                        var view = new View(ItemView.Context)
                        {
                            LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent, 1f),
                            Background = new ColorDrawable(Color.ParseColor(hexColor))
                        };
                        categoriesLayout.AddView(view);
                    }
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
            readonly LinearLayoutCompat categoriesLayout;
            readonly View selectedOverlay;

            public ContactPreviewViewHolder(View itemView)
                    : base(itemView)
            {
                letterTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_contact_letter);
                nameTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_contact_name);
                descTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_contact_desc);
                categoriesLayout = itemView.FindViewById<LinearLayoutCompat>(Resource.Id.list_item_contact_categories);
                selectedOverlay = itemView.FindViewById<View>(Resource.Id.selected_overlay);
            }
        }

        #endregion
    }
}

