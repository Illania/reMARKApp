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
    public class PickContactSavedSearchesFragment : BaseFragment
    {
        public Task<SavedContactsSearch> Task => tcs.Task;

        readonly TaskCompletionSource<SavedContactsSearch> tcs = new TaskCompletionSource<SavedContactsSearch>();

        RecyclerView recyclerView;
        ContactSavedSearchesListViewAdapter adapter;
        Action dismissAction;

        public static (PickContactSavedSearchesFragment fragment, string tag) NewInstance()
        {
            var fragment = new PickContactSavedSearchesFragment();
            var tag = $"{nameof(PickContactSavedSearchesFragment)}";

            return (fragment, tag);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(PickContactSavedSearchesFragment)}]");

            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkerblue)));

            adapter = new ContactSavedSearchesListViewAdapter(this, CreateContextMenu);
            recyclerView.SetAdapter(adapter);

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = GetString(Resource.String.saved_searches);

            CommonConfig.Logger.Info($"Created {nameof(PickContactSavedSearchesFragment)}");
        }

        public override void OnResume()
        {
            base.OnResume();

            if (adapter.ItemCount < 1)
            {
                CommonConfig.Logger.Info($"Refreshing {nameof(PickContactSavedSearchesFragment)}");
                RefreshData();
            }
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
        }

        public async void RefreshData()
        {
            var searches = await Managers.SearchManager.GetSavedContactsSearchesAsync();
            if (searches != null)
                adapter.SetItems(searches);
        }


        public void SavedSearchSelected(SavedContactsSearch savedSearch)
        {
            tcs.SetResult(savedSearch);
            ((AppCompatActivity)Activity).OnBackPressed();
        }

        async void DeleteSavedSearch(SavedContactsSearch savedSearch)
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


        class ContactSavedSearchesListViewAdapter : RecyclerView.Adapter
        {
            public override int ItemCount => Items.Count;

            public SavedContactsSearch SelectedSavedSearch { get; }

            public int SelectedPosition { get; set; }

            public List<SavedContactsSearch> Items { get; } = new List<SavedContactsSearch>();

            readonly PickContactSavedSearchesFragment parent;

            readonly Action<IContextMenu, View, IContextMenuContextMenuInfo> action;

            public ContactSavedSearchesListViewAdapter(PickContactSavedSearchesFragment parent, Action<IContextMenu, View, IContextMenuContextMenuInfo> action)
            {
                this.parent = parent;
                this.action = action;
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var c = Items[position];
                var lvh = holder as ContactSavedSearchViewHolder;

                lvh.ItemView.SetOnClickListener(new ActionOnClickListener(() => HandleClick(c)));
                lvh.ItemView.SetOnCreateContextMenuListener(new ActionOnCreateContextMenuListener(action));
                lvh.Name = c.Name;
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.search_list_item_country, parent, false);
                return new ContactSavedSearchViewHolder(itemView);
            }

            public void SetItems(List<SavedContactsSearch> savedSearches)
            {
                var count = Items.Count;
                Items.AddRange(savedSearches);
                NotifyItemRangeInserted(count, savedSearches.Count);
            }

            void HandleClick(SavedContactsSearch savedSearch)
            {
                parent.SavedSearchSelected(savedSearch);
            }

            public void SortItems()
            {
                Items.Sort();
                NotifyItemRangeChanged(0, Items.Count);
            }

            public void RemoveItem(SavedContactsSearch item)
            {
                var position = Items.FindIndex(c => c.Id == item.Id);
                if (position >= 0)
                {
                    Items.RemoveAt(position);
                    NotifyItemRemoved(position);
                }
            }

            public SavedContactsSearch GetItemAtPosition(int position)
            {
                return Items[position];
            }

            public SavedContactsSearch GetSelectedItem()
            {
                return Items[SelectedPosition];
            }
        }


        class ContactSavedSearchViewHolder : RecyclerView.ViewHolder
        {
            public string Name { set => nameTextView.Text = value; }

            readonly AppCompatTextView nameTextView;

            public ContactSavedSearchViewHolder(View itemView)
                : base(itemView)
            {
                nameTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.search_list_item_country_name);
            }
        }
    }
}