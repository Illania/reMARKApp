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
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class TemplatesListFragment : RetainableStateFragment, MenuItemCompat.IOnActionExpandListener, SearchView.IOnQueryTextListener
    {
        RecyclerView recyclerView;
        SearchView searchView;

        TemplatesListAdapter adapter;
        TemplatesListAdapter searchAdapter;

        readonly Handler searchHandler = new Handler();

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(TemplatesListFragment)}");

            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            var emptyView = rootView.FindViewById<AppCompatTextView>(Resource.Id.empty_view);
            emptyView.SetText(Resource.String.no_templates);

            var refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            refreshLayout.Enabled = false;

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));

            adapter = new TemplatesListAdapter();
            adapter.ItemClicked += Adapter_ItemClicked;
            adapter.RegisterAdapterDataObserver(new LambdaEmptyAdapterObserver(() =>
            {
                if (recyclerView.GetAdapter() != adapter)
                    return;

                emptyView.Visibility = adapter.ItemCount < 1 ? ViewStates.Visible : ViewStates.Gone;
                recyclerView.Visibility = adapter.ItemCount > 0 ? ViewStates.Visible : ViewStates.Gone;
            }));
            recyclerView.SetAdapter(adapter);

            searchAdapter = new TemplatesListAdapter();
            searchAdapter.ItemClicked += Adapter_ItemClicked;

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.templates);
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = null;

            CommonConfig.Logger.Info($"Created {nameof(TemplatesListFragment)}");
        }

        public async override void OnResume()
        {
            base.OnResume();

            if (adapter.Empty)
            {
                CommonConfig.Logger.Info($"Refreshing {nameof(TemplatesListFragment)}");
                await RefreshView();
            }
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.menu_main, menu);

            var filterItem = menu.FindItem(Resource.Id.action_filter);
            MenuItemCompat.SetOnActionExpandListener(filterItem, this);
            searchView = (SearchView)MenuItemCompat.GetActionView(filterItem);
            searchView.QueryHint = GetString(Resource.String.filter);
            searchView.SetOnQueryTextListener(this);
        }

        #region Refresh

        async Task RefreshView()
        {
            try
            {
                CommonConfig.Logger.Info($"Refresh running...");

                var previews = await Managers.DocumentsManager.GetTemplatePreviewsAsync();

                adapter.RefreshData(previews);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while retrieving template previews", ex);
                await Dialogs.ShowErrorDialogAsync(Activity, ex);
                Activity?.Finish();
            }
            finally
            {
                CommonConfig.Logger.Info($"Refresh finished");
            }
        }

        #endregion

        #region Actions

        void Adapter_ItemClicked(object sender, TemplatePreview tp)
        {
            var data = new Intent();
            data.PutExtra(TemplatesListActivity.TemplatePreviewResultKey, Serializer.Serialize(tp));
            Activity.SetResult(Android.App.Result.Ok, data);
            Activity?.Finish();
        }

        #endregion

        #region Filtering

        static bool MatchesQuery(TemplatePreview sp, string query)
        {
            if (sp.Name?.ContainsCaseInsensitive(query) ?? false)
                return true;

            return false;
        }

        bool MenuItemCompat.IOnActionExpandListener.OnMenuItemActionExpand(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_filter)
            {
                recyclerView.SwapAdapter(searchAdapter, true);
                (this as SearchView.IOnQueryTextListener).OnQueryTextChange(string.Empty);
                return true;
            }

            return false;
        }

        bool MenuItemCompat.IOnActionExpandListener.OnMenuItemActionCollapse(IMenuItem item)
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
                        searchAdapter.RefreshData(adapter.Items);
                    else
                        searchAdapter.RefreshData(adapter.Items.Where(dp => MatchesQuery(dp, newText)).ToList());
                },
                500);
            return false;
        }

        bool SearchView.IOnQueryTextListener.OnQueryTextSubmit(string query)
        {
            return false;
        }

        #endregion

        #region Retainable State

        public override IRetainableState OnRetainInstanceState()
        {
            CommonConfig.Logger.Info($"Retaining state for {nameof(TemplatesListFragment)}");
            return new TemplateListFragmentState
            {
                TemplatePreviews = adapter.Items.ToList()
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            CommonConfig.Logger.Info($"Restoring state for {nameof(TemplatesListFragment)}");

            if (restoredState is TemplateListFragmentState tlfs)
            {
                adapter.RefreshData(tlfs.TemplatePreviews);
            }
        }

        public override string GenerateTag() => $"{nameof(TemplatesListFragment)}";

        class TemplateListFragmentState : IRetainableState
        {
            public List<TemplatePreview> TemplatePreviews { get; set; }
        }

        #endregion

        class TemplatesListAdapter : RecyclerView.Adapter
        {
            public override int ItemCount => templatesInView.Sum(f => f.Count) + 2;

            public event EventHandler<TemplatePreview> ItemClicked = delegate { };

            readonly int sectionHeight = Conversion.ConvertDpToPixels(56);

            List<List<TemplatePreview>> templatesInView = new List<List<TemplatePreview>>(2)
            {
                new List<TemplatePreview>(),
                new List<TemplatePreview>(),
            };

            public bool Empty => !Items.Any();

            public IEnumerable<TemplatePreview> Items => templatesInView.SelectMany(t => t);

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                if (holder is SectionViewHolder sectionViewHolder)
                {
                    var section = GetSectionAtPosition(position);
                    var title = section == Section.Private ? holder.ItemView.Context.GetString(Resource.String.private_header)
                                                  : holder.ItemView.Context.GetString(Resource.String.public_header);

                    sectionViewHolder.SectionTitle = title;

                    if (templatesInView[section].Any())
                    {
                        sectionViewHolder.ItemView.Visibility = ViewStates.Visible;
                        sectionViewHolder.ItemView.LayoutParameters.Height = sectionHeight;
                    }
                    else
                    {
                        sectionViewHolder.ItemView.Visibility = ViewStates.Gone;
                        sectionViewHolder.ItemView.LayoutParameters.Height = 0;
                    }
                }

                if (holder is TemplateViewHolder templateViewHolder)
                {
                    var preview = GetItemAtPosition(position);
                    templateViewHolder.ItemView.SetOnClickListener(new ActionOnClickListener(() => ItemClicked(this, preview)));
                    templateViewHolder.Name = preview.Name;
                }
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                if (viewType == ViewType.SectionView)
                {
                    var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_section, parent, false);
                    return new SectionViewHolder(itemView);
                }
                else
                {
                    var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_templates, parent, false);
                    return new TemplateViewHolder(itemView);
                }
            }

            public override int GetItemViewType(int position)
            {
                if (position == 0 || position == templatesInView[Section.Private].Count + 1)
                    return ViewType.SectionView;

                return ViewType.TemplateView;
            }

            public void RefreshData(IEnumerable<TemplatePreview> previews)
            {
                NotifyItemRangeRemoved(0, ItemCount);

                templatesInView[Section.Private].Clear();
                templatesInView[Section.Public].Clear();

                templatesInView[Section.Private].AddRange(previews.Where(p => p.Private).OrderBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase));
                templatesInView[Section.Public].AddRange(previews.Where(p => !p.Private).OrderBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase));

                NotifyItemRangeInserted(0, ItemCount);
            }

            public void Clear()
            {
                var size = ItemCount;

                templatesInView[Section.Private].Clear();
                templatesInView[Section.Public].Clear();

                NotifyItemRangeRemoved(2, size - 2);
            }

            TemplatePreview GetItemAtPosition(int position)
            {
                var privateCount = templatesInView[Section.Private].Count;
                var publicCount = templatesInView[Section.Public].Count;

                if (position < privateCount + 1)
                {
                    return templatesInView[Section.Private][position - 1];
                }

                return templatesInView[Section.Public][position - 2 - privateCount];
            }

            int GetSectionAtPosition(int position)
            {
                return position == 0 ? Section.Private : Section.Public;
            }

            #region RecyclerView ViewHolders

            class TemplateViewHolder : RecyclerView.ViewHolder
            {
                public string Name { set => nameTextView.Text = value; }

                readonly AppCompatTextView nameTextView;

                public TemplateViewHolder(View itemView)
                    : base(itemView)
                {
                    nameTextView = itemView as AppCompatTextView;
                }
            }

            class SectionViewHolder : RecyclerView.ViewHolder
            {
                public string SectionTitle { set => sectionTitleTextView.Text = value; }

                public AppCompatTextView sectionTitleTextView;

                public SectionViewHolder(View itemView)
                    : base(itemView)
                {
                    sectionTitleTextView = itemView as AppCompatTextView;
                }
            }

            #endregion

            public static class ViewType
            {
                public const int TemplateView = 0;
                public const int SectionView = 1;
            }

            //If we decide to change the order of public and private some of the functions need to be modified slightly
            public static class Section
            {
                public const int Private = 0;
                public const int Public = 1;
            }
        }

    }

}

