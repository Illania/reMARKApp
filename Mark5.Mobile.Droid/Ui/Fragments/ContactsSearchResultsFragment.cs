using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using FastScrollRecycler;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class ContactsSearchResultsFragment : BaseFragment
    {
        const string CriteriaBundleKey = "Criteria_cc2b48a4-affd-48a8-bb0c-4a6cec17975a";
        const string ContactPreviewsKey = "ContactPreviews_dbe9d50f-3e9e-4e20-a0d8-981cc0511e39";

        SearchContactsCriteria criteria;
        List<ContactPreview> savedResults;

        SwipeRefreshLayout refreshLayout;
        RecyclerView recyclerView;
        ContactSearchResultsAdapter adapter;

        public static (ContactsSearchResultsFragment fragment, string tag) NewInstance(SearchContactsCriteria criteria)
        {
            var args = new Bundle();

            if (criteria != null)
                args.PutString(CriteriaBundleKey, Serializer.Serialize(criteria));

            var fragment = new ContactsSearchResultsFragment();
            fragment.Arguments = args;

            var tag = $"{nameof(ContactsSearchResultsFragment)}]";

            return (fragment, tag);
        }

        #region Fragment overrides

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Arguments.ContainsKey(CriteriaBundleKey))
                criteria = Serializer.Deserialize<SearchContactsCriteria>(Arguments.GetString(CriteriaBundleKey));

            if (savedInstanceState?.ContainsKey(ContactPreviewsKey) == true)
                savedResults = Serializer.Deserialize<List<ContactPreview>>(savedInstanceState.GetString(ContactPreviewsKey));
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(ContactsSearchResultsFragment)} [criteria={criteria}]...");

            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            refreshLayout.SetColorSchemeResources(Resource.Color.blue, Resource.Color.darkerblue);
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

            if (savedResults != null)
            {
                CommonConfig.Logger.Info($"Restoring state...");
                adapter.AppendItems(savedResults);
            }

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.search_contacts_result);
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = null;

            CommonConfig.Logger.Info($"Created {nameof(ContactsSearchResultsFragment)} [criteria={criteria}]");
        }

        public override async void OnResume()
        {
            base.OnResume();

            CommonConfig.Logger.Info($"Resuming {nameof(ContactsSearchResultsFragment)} [criteria={criteria}]...");

            if (adapter.ItemCount < 1)
            {
                CommonConfig.Logger.Info($"No elements - will refresh...");

                await RefreshData();
            }
        }

        public override void OnPause()
        {
            base.OnPause();

            CommonConfig.Logger.Info($"Pausing {nameof(ContactsSearchResultsFragment)} [criteria={criteria}]...");
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            //if (adapter?.Items != null || savedResults != null) // Cannot be used because the size of the results could be too big
            //outState.PutString(ContactPreviewsKey, Serializer.Serialize(adapter?.Items ?? savedResults));
        }

        #endregion

        #region Refreshing

        async Task RefreshData()
        {
            try
            {
                CommonConfig.Logger.Info($"Refresh running...");

                refreshLayout.Refreshing = true;

                var contactPreviews = await Managers.SearchManager.SearchContactsAsync(criteria);

                if (contactPreviews.Count < 1)
                {
                    await Dialogs.ShowConfirmDialogAsync(Activity, Resource.String.no_results, Resource.String.no_results_contacts);
                    Activity?.OnBackPressed();
                    return;
                }

                if (CommonConfig.Logger.IsDebugEnabled())
                    CommonConfig.Logger.Debug($"Retrieved {contactPreviews.Count} items");

                adapter.AppendItems(contactPreviews);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Downloading contacts failed [criteria={criteria}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);

                Activity?.OnBackPressed();
            }
            finally
            {
                refreshLayout.Refreshing = false;

                CommonConfig.Logger.Info($"Refresh finished");
            }
        }

        #endregion

        #region Adapter callbacks

        void Adapter_ItemClicked(object sender, ContactPreview contactPreview)
        {
            var i = new Intent(Activity, typeof(ContactActivity));
            i.PutExtra(ContactActivity.ContactPreviewIntentKey, Serializer.Serialize(contactPreview));
            StartActivity(i);
        }

        #endregion

        #region RecyclerView Adapter/ViewHolder

        class ContactSearchResultsAdapter : RecyclerView.Adapter, ISectionedAdapter
        {
            public List<ContactPreview> Items { get; } = new List<ContactPreview>(1000);

            public override int ItemCount => Items.Count;

            readonly Dictionary<int, ContactPreview> selectedContactsInView = new Dictionary<int, ContactPreview>();

            public event EventHandler<ContactPreview> ItemClicked = delegate { };

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var cpvh = holder as ContactPreviewViewHolder;
                if (cpvh == null)
                    return;

                var cp = Items[position];

                cpvh.ItemView.SetOnClickListener(new ActionOnClickListener(() => ItemClicked(this, cp)));

                cpvh.Type = cp.Type;
                cpvh.Name = cp.Name;
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
                var count = Items.Count;
                Items.AddRange(items);
                NotifyItemRangeInserted(count, items.Count);
            }

            string ISectionedAdapter.GetSectionName(int position)
            {
                return Items[position].Name?.SafeSubstring(0, 1)?.ToUpper() ?? "";
            }
        }

        class ContactPreviewViewHolder : RecyclerView.ViewHolder
        {
            public ContactType Type
            {
                set
                {
                    switch (value)
                    {
                        case ContactType.Person:
                            iconImageView.SetImageResource(Resource.Drawable.large_person);
                            break;
                        case ContactType.Department:
                            iconImageView.SetImageResource(Resource.Drawable.large_department);
                            break;
                        case ContactType.Company:
                            iconImageView.SetImageResource(Resource.Drawable.large_company);
                            break;
                        default:
                            iconImageView.SetImageDrawable(null);
                            break;
                    }
                }
            }

            public string Name { set => nameTextView.Text = value; }

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

            public bool Selected { set => selectedOverlay.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }

            readonly AppCompatImageView iconImageView;
            readonly AppCompatTextView nameTextView;
            readonly LinearLayoutCompat categoriesLayout;
            readonly View selectedOverlay;

            public ContactPreviewViewHolder(View itemView)
                : base(itemView)
            {
                iconImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_contact_icon);
                nameTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_contact_name);
                categoriesLayout = itemView.FindViewById<LinearLayoutCompat>(Resource.Id.list_item_contact_categories);
                selectedOverlay = itemView.FindViewById<View>(Resource.Id.selected_overlay);
            }
        }

        #endregion
    }
}