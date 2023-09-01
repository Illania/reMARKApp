using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using AndroidX.RecyclerView.Widget;
using AndroidX.Core.Content;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using Mark5.Mobile.Common.Manager;
using System;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class PickShortcodeSavedSearchesFragment : BaseFragment
    {
        public Task<SavedShortcodesSearch> Task => tcs.Task;

        readonly TaskCompletionSource<SavedShortcodesSearch> tcs = new TaskCompletionSource<SavedShortcodesSearch>();

        RecyclerView recyclerView;
        ShortcodeSavedSearchesListViewAdapter adapter;
        Action dismissAction;

        public static (PickShortcodeSavedSearchesFragment fragment, string tag) NewInstance()
        {
            var fragment = new PickShortcodeSavedSearchesFragment();
            var tag = $"{nameof(PickShortcodeSavedSearchesFragment)}";

            return (fragment, tag);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(PickShortcodeSavedSearchesFragment)}]");

            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkerblue)));

            adapter = new ShortcodeSavedSearchesListViewAdapter(this, CreateContextMenu);
            recyclerView.SetAdapter(adapter);

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = GetString(Resource.String.saved_searches);

            CommonConfig.Logger.Info($"Created {nameof(PickShortcodeSavedSearchesFragment)}");
        }

        public override void OnResume()
        {
            base.OnResume();

            if (adapter.ItemCount < 1)
            {
                CommonConfig.Logger.Info($"Refreshing {nameof(PickShortcodeSavedSearchesFragment)}");
                RefreshData();
            }
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
        }

        public async void RefreshData()
        {
            var searches = await Managers.SearchManager.GetSavedShortcodesSearchesAsync();
            if (searches != null)
                adapter.SetItems(searches);
        }


        public void SavedSearchSelected(SavedShortcodesSearch savedSearch)
        {
            tcs.SetResult(savedSearch);
            ((AppCompatActivity)Activity).OnBackPressed();
        }

        async void DeleteSavedSearch(SavedShortcodesSearch savedSearch)
        {
            dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.deleting_saved_search, Resource.String.please_wait);

            try
            {

                await Managers.SearchManager.DeleteSavedSearchAsync(savedSearch.Id);

            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Failed to delete SavedSearch from entity [savedSearch.Id={savedSearch.Id}", ex);
                await Dialogs.ShowErrorDialogAsync(Activity, ex);

                return;
            }

            dismissAction();
            adapter.RemoveItem(savedSearch);
            adapter.SortItems();
        }

        #region Options menu

        public override bool OnContextItemSelected(IMenuItem item)
        {
            var savedSearch = adapter.GetSelectedItem();

            if (item.ItemId == MenuItemActions.DeleteSavedSearch)
                Dialogs.ShowYesNoDialog(Context, Resource.String.confirm_saved_search_deletion_title, Resource.String.confirm_saved_search_deletion_content,
                    () => DeleteSavedSearch(savedSearch),
                    null, Resource.String.confirm, Resource.String.cancel);

            return true;
        }

        public void CreateContextMenu(IContextMenu menu, View view, IContextMenuContextMenuInfo menuInfo)
        {
            var position = recyclerView.GetChildAdapterPosition(view);
            adapter.SelectedPosition = position;

            menu.Add(IMenu.None, MenuItemActions.DeleteSavedSearch, MenuItemActions.DeleteSavedSearch, Resource.String.delete);
        }

        static class MenuItemActions
        {
            public const int DeleteSavedSearch = 10;
        }

        #endregion


        class ShortcodeSavedSearchesListViewAdapter : RecyclerView.Adapter
        {
            public override int ItemCount => Items.Count;

            public SavedShortcodesSearch SelectedSavedSearch { get; }

            public int SelectedPosition { get; set; }

            public List<SavedShortcodesSearch> Items { get; } = new List<SavedShortcodesSearch>();

            readonly PickShortcodeSavedSearchesFragment parent;

            readonly Action<IContextMenu, View, IContextMenuContextMenuInfo> action;

            public ShortcodeSavedSearchesListViewAdapter(PickShortcodeSavedSearchesFragment parent, Action<IContextMenu, View, IContextMenuContextMenuInfo> action)
            {
                this.parent = parent;
                this.action = action;
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var c = Items[position];
                var lvh = holder as ShortcodeSavedSearchViewHolder;

                lvh.ItemView.SetOnClickListener(new ActionOnClickListener(() => HandleClick(c)));
                lvh.ItemView.SetOnCreateContextMenuListener(new ActionOnCreateContextMenuListener(action));
                lvh.Name = c.Name;
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.search_list_item_country, parent, false);
                return new ShortcodeSavedSearchViewHolder(itemView);
            }

            public void SetItems(List<SavedShortcodesSearch> savedSearches)
            {
                var count = Items.Count;
                Items.AddRange(savedSearches);
                NotifyItemRangeInserted(count, savedSearches.Count);
            }

            void HandleClick(SavedShortcodesSearch savedSearch)
            {
                parent.SavedSearchSelected(savedSearch);
            }

            public void SortItems()
            {
                Items.Sort();
                NotifyItemRangeChanged(0, Items.Count);
            }

            public void RemoveItem(SavedShortcodesSearch item)
            {
                var position = Items.FindIndex(c => c.Id == item.Id);
                if (position >= 0)
                {
                    Items.RemoveAt(position);
                    NotifyItemRemoved(position);
                }
            }

            public SavedShortcodesSearch GetItemAtPosition(int position)
            {
                return Items[position];
            }

            public SavedShortcodesSearch GetSelectedItem()
            {
                return Items[SelectedPosition];
            }
        }


        class ShortcodeSavedSearchViewHolder : RecyclerView.ViewHolder
        {
            public string Name { set => nameTextView.Text = value; }

            readonly AppCompatTextView nameTextView;

            public ShortcodeSavedSearchViewHolder(View itemView)
                : base(itemView)
            {
                nameTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.search_list_item_country_name);
            }
        }
    }
}