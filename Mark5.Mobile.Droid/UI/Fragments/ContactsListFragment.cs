//
// Project: Mark5.Mobile.Droid
// File: ContactsListFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V4.View;
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
using Mark5.Mobile.Droid.Ui.Common.BusMesseges;

namespace Mark5.Mobile.Droid.Ui.Fragments
{

    public class ContactsListFragment : RetainableStateFragment, ActionMode.ICallback, MenuItemCompat.IOnActionExpandListener, SearchView.IOnQueryTextListener
    {

        public Folder Folder
        {
            get;
            set;
        }

        bool refreshing;

        SwipeRefreshLayout refreshLayout;
        RecyclerView recyclerView;
        ContactsListAdapter adapter;
        ContactsListAdapter searchAdapter;
        ActionMode actionMode;
        SearchView searchView;

        bool shouldNotifyAdapter;
        bool shouldNotifySearchAdapter;

        ContactsListAdapter CurrentAdapter
        {
            get { return (ContactsListAdapter)recyclerView.GetAdapter(); }
        }

        CancellationTokenSource cts;

        readonly Handler searchHandler = new Handler();

        #region Fragment overrides

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(ContactsListFragment)} [folder.id={Folder?.Id}, folder.name={Folder?.Name}]...");

            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            refreshLayout.SetColorSchemeResources(Resource.Color.lightbrown, Resource.Color.brown);
            refreshLayout.Refresh += (sender, e) =>
            {
                actionMode?.Finish();
                actionMode = null;

                RefreshData(force: true);
            };

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));
            recyclerView.HasFixedSize = true;

            adapter = new ContactsListAdapter();
            adapter.ItemClicked += Adapter_ItemClicked;
            adapter.ItemLongClicked += Adapter_ItemLongClicked;
            recyclerView.SetAdapter(adapter);

            searchAdapter = new ContactsListAdapter();
            searchAdapter.ItemClicked += Adapter_ItemClicked;
            searchAdapter.ItemLongClicked += Adapter_ItemLongClicked;

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = Folder?.Name;
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = GetString(Resource.String.contacts);

            CommonConfig.Logger.Info($"Created {nameof(ContactsListFragment)} [folder.id={Folder?.Id}, folder.name={Folder?.Name}]");
        }

        public override void OnResume()
        {
            base.OnResume();

            CommonConfig.Logger.Info($"Resuming {nameof(ContactsListFragment)} [folder.id={Folder?.Id}, folder.name={Folder?.Name}]...");

            if (adapter.ItemCount < 1)
            {
                CommonConfig.Logger.Info($"No elements - will refresh...");

                RefreshData();
            }

            if (shouldNotifyAdapter)
            {
                shouldNotifyAdapter = false;
                actionMode?.Finish();
                actionMode = null;
                adapter.NotifyDataSetChanged();
            }
            if (shouldNotifySearchAdapter)
            {
                shouldNotifySearchAdapter = false;
                actionMode?.Finish();
                actionMode = null;
                searchAdapter.NotifyDataSetChanged();
            }
        }

        public override void OnPause()
        {
            base.OnPause();

            CommonConfig.Logger.Info($"Pausing {nameof(ContactsListFragment)} [folder.id={Folder?.Id}, folder.name={Folder?.Name}]...");

            cts?.Cancel();
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.menu_main, menu);

            var searchItem = menu.FindItem(Resource.Id.action_search);
            MenuItemCompat.SetOnActionExpandListener(searchItem, this);
            searchView = (SearchView)MenuItemCompat.GetActionView(searchItem);
            searchView.QueryHint = GetString(Resource.String.filter);
            searchView.SetOnQueryTextListener(this);
        }

        #endregion

        #region RetainableStateFragment overrides

        public override IRetainableState OnRetainInstanceState()
        {
            CommonConfig.Logger.Info($"Retaining state [folder.id={Folder?.Id}, folder.name={Folder?.Name}, contactPreviews.Count={adapter?.ItemCount}/{adapter?.SelectedItemCount}, refreshing={refreshing}]...");

            return new ContactsListFragmentState
            {
                Folder = Folder,
                ContactPreviews = adapter.Items,
                SelectedContactPreviews = adapter.SelectedItems,
                RefreshInProgress = refreshing
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var dlfs = restoredState as ContactsListFragmentState;
            if (dlfs != null)
            {
                CommonConfig.Logger.Info($"Restoring state [dlfs.folder.id={dlfs.Folder?.Id}, dlfs.items.count={dlfs.ContactPreviews?.Count}, dlfs.selectedItems.count={dlfs.SelectedContactPreviews?.Count}]...");

                Folder = dlfs.Folder;
                adapter.AppendItems(dlfs.ContactPreviews);

                if (dlfs.RefreshInProgress)
                {
                    CommonConfig.Logger.Info($"Refresh was in progress before - will continue...");

                    RefreshData(dlfs.ContactPreviews[dlfs.ContactPreviews.Count - 1].RowId);
                }

                if (dlfs.SelectedContactPreviews.Count > 0)
                {
                    actionMode?.Finish();
                    actionMode = Activity.StartActionMode(this);

                    adapter.SetSelected(dlfs.SelectedContactPreviews, true);
                    actionMode.Title = adapter.SelectedItemCount.ToString();
                    actionMode.Invalidate();
                }
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(ContactsListFragment)} [folder.id={Folder.Id}, folder.name={Folder.Name}]";
        }

        #endregion

        #region Refreshing

        void RefreshData(int startRowId = -1, bool force = false)
        {
            CommonConfig.Logger.Info($"Attempting refresh [startRowId={startRowId}, force={force}]...");

            if (refreshing) return;

            refreshing = true;
            refreshLayout.Post(() => refreshLayout.Refreshing = true); //Bug: fixed in support library v 24.2.0 (issue 77712)

            CommonConfig.Logger.Info($"Refresh running...");

            cts?.Cancel();
            cts = new CancellationTokenSource();

            if (force)
            {
                adapter.Clear();
            }

            Managers.ContactsManager.GetAllContactPreviews(Folder, cps =>
            {
                CommonConfig.Logger.Debug($"Retrieved {cps?.Count} contacts");

                Managers.DownloadManager.Notify(ObjectType.Contact, Folder.Id);
                Activity.RunOnUiThread(() => adapter.AppendItems(cps));
            }, () =>
            {
                refreshLayout.Post(() => refreshLayout.Refreshing = false); //Bug: fixed in support library v 24.2.0 (issue 77712)
                refreshing = false;

                CommonConfig.Logger.Info($"Refresh finished");
            }, ex =>
            {
                CommonConfig.Logger.Error($"Downloading contacts failed [folder.name={Folder?.Name}, folder.id={Folder?.Id}, startRowId={startRowId}, force={force}]", ex);

                Dialogs.ShowErrorDialog(Activity, ex);
            }, startRowId, cts.Token);
        }

        #endregion

        #region Messanger Handlers

        public void UpdateCategories(ContactPreviewCategoriesChangedMessage m)
        {
            var position = adapter.GetPosition(m.ContactPreviewId);
            if (position >= 0)
            {
                shouldNotifyAdapter = true;
                var cp = adapter.Items[position];
                cp.Categories.Clear();
                cp.Categories.AddRange(m.Categories);
            }

            position = searchAdapter.GetPosition(m.ContactPreviewId);
            if (position >= 0)
            {
                shouldNotifySearchAdapter = true;
                var cp = searchAdapter.Items[position];
                cp.Categories.Clear();
                cp.Categories.AddRange(m.Categories);
            }
        }

        public void RemoveMovedEntities(EntityMovedFromFolderMessage m)
        {
            foreach (var entityId in m.EntitiesId)
            {
                var position = adapter.GetPosition(entityId);
                if (position >= 0)
                {
                    shouldNotifyAdapter = true;
                    adapter.RemoveItemsAtIndex(position);
                    adapter.ClearSelections(false);
                }

                position = searchAdapter.GetPosition(entityId);
                if (position >= 0)
                {
                    shouldNotifySearchAdapter = true;
                    searchAdapter.RemoveItemsAtIndex(position);
                    adapter.ClearSelections(false);
                }
            }

        }

        #endregion

        #region Adapter callbacks

        void Adapter_ItemClicked(object sender, ContactPreview contactPreview)
        {
            if (actionMode == null)
            {
                var i = new Intent(Activity, typeof(ContactActivity));
                i.PutExtra(ContactActivity.ContactPreviewIntentKey, SerializationUtils.Serialize(contactPreview));
                i.PutExtra(ContactActivity.FolderIntentKey, SerializationUtils.Serialize(Folder));
                StartActivity(i);
            }
            else
            {
                var currentAdapter = (ContactsListAdapter)recyclerView.GetAdapter();
                currentAdapter.SetSelected(contactPreview, !currentAdapter.IsSelected(contactPreview));

                if (currentAdapter.SelectedItemCount < 1)
                {
                    actionMode.Finish();
                }
                else
                {
                    actionMode.Title = currentAdapter.SelectedItemCount.ToString();
                    actionMode.Invalidate();
                }
            }
        }

        void Adapter_ItemLongClicked(object sender, ContactPreview contactPreview)
        {
            if (actionMode == null)
            {
                actionMode = Activity.StartActionMode(this);
            }

            Adapter_ItemClicked(sender, contactPreview);
        }

        #endregion

        #region Action mode

        bool ActionMode.ICallback.OnPrepareActionMode(ActionMode mode, IMenu menu)
        {
            menu.Clear();

            menu.Add(Menu.None, MenuItemActions.CopyToWorktray, MenuItemActions.CopyToWorktray, Resource.String.copy_to_worktray);
            menu.Add(Menu.None, MenuItemActions.CopyToFolder, MenuItemActions.CopyToFolder, Resource.String.copy_to_folder);

            if (Folder.InternalType == FolderInternalType.FilterView
                || Folder.InternalType == FolderInternalType.Static
                || Folder.InternalType == FolderInternalType.Worktray)
            {
                menu.Add(Menu.None, MenuItemActions.MoveToFolder, MenuItemActions.MoveToFolder, Resource.String.move_to_folder);
            }

            if (CurrentAdapter.SelectedItemCount == 1)
            {
                menu.Add(Menu.None, MenuItemActions.Categories, MenuItemActions.Categories, Resource.String.categories);
            }

            if (Folder.InternalType == FolderInternalType.FilterView
                || Folder.InternalType == FolderInternalType.Static
                || Folder.InternalType == FolderInternalType.Worktray)
            {
                menu.Add(Menu.None, MenuItemActions.DeleteFromFolder, MenuItemActions.DeleteFromFolder, Resource.String.delete_from_folder);
            }

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator
                || ServerConfig.SystemSettings.ContactsModuleInfo.Permissions.DeleteAllowed)
            {
                menu.Add(Menu.None, MenuItemActions.Delete, MenuItemActions.Delete, Resource.String.delete);
            }

            return true;
        }

        static class MenuItemActions
        {
            public const int CopyToWorktray = 30;
            public const int CopyToFolder = 40;
            public const int MoveToFolder = 41;
            public const int Categories = 50;
            public const int DeleteFromFolder = 70;
            public const int Delete = 71;
        }

        public bool OnCreateActionMode(ActionMode mode, IMenu menu)
        {
            return true;
        }

        bool ActionMode.ICallback.OnActionItemClicked(ActionMode mode, IMenuItem item)
        {
            var selectedContacts = CurrentAdapter.SelectedItems;

            if (item.ItemId == MenuItemActions.Categories)
            {
                var i = new Intent(Activity, typeof(CategoriesListActivity));
                i.PutExtra(CategoriesListActivity.BusinessEntityPreviewIntentKey, SerializationUtils.Serialize(selectedContacts.First()));
                StartActivity(i);

                return true;
            }
            if (item.ItemId == MenuItemActions.CopyToFolder)
            {
                var i = new Intent(Activity, typeof(FolderListSelectionActivity));
                i.PutExtra(FolderListSelectionActivity.ModeIntentKey, (int)FolderListSelectionActivity.ModeType.CopyToFolderMode);
                i.PutExtra(FolderListSelectionActivity.ModuleIntentKey, SerializationUtils.Serialize(ModuleType.Contacts));
                i.PutExtra(FolderListSelectionActivity.BusinessEntitiesIntentKey, SerializationUtils.Serialize(selectedContacts.Select(sp => sp).Cast<IBusinessEntity>().ToList()));
                StartActivity(i);

                return true;
            }
            if (item.ItemId == MenuItemActions.MoveToFolder)
            {
                var i = new Intent(Activity, typeof(FolderListSelectionActivity));
                i.PutExtra(FolderListSelectionActivity.ModeIntentKey, (int)FolderListSelectionActivity.ModeType.MoveToFolderMode);
                i.PutExtra(FolderListSelectionActivity.ModuleIntentKey, SerializationUtils.Serialize(ModuleType.Contacts));
                i.PutExtra(FolderListSelectionActivity.BusinessEntitiesIntentKey, SerializationUtils.Serialize(selectedContacts.Select(sp => sp).Cast<IBusinessEntity>().ToList()));
                i.PutExtra(FolderListSelectionActivity.FromFolderIntentKey, SerializationUtils.Serialize(Folder));
                StartActivity(i);

                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        void ActionMode.ICallback.OnDestroyActionMode(ActionMode mode)
        {
            var currentAdapter = (ContactsListAdapter)recyclerView.GetAdapter();
            currentAdapter.ClearSelections();
            actionMode = null;
        }

        #endregion

        #region Filtering

        bool MenuItemCompat.IOnActionExpandListener.OnMenuItemActionExpand(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_search)
            {
                refreshLayout.Enabled = false;
                adapter.ClearSelections();
                recyclerView.SwapAdapter(searchAdapter, true);
                return true;
            }

            return false;
        }

        bool MenuItemCompat.IOnActionExpandListener.OnMenuItemActionCollapse(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_search)
            {
                searchHandler.RemoveCallbacksAndMessages(null);
                searchAdapter.Clear();
                recyclerView.SwapAdapter(adapter, true);
                refreshLayout.Enabled = true;
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
                {
                    searchAdapter.Clear();
                }
                else
                {
                    searchAdapter.ReplaceItems(adapter.Items.Where(dp => MatchesQuery(dp, newText)).ToList());
                }
            }, 500);
            return false;
        }

        bool SearchView.IOnQueryTextListener.OnQueryTextSubmit(string query)
        {
            return false;
        }

        static bool MatchesQuery(ContactPreview cp, string query)
        {
            if (cp.Name.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) > 0)
            {
                return true;
            }
            if (cp.CompanyName.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) > 0)
            {
                return true;
            }
            if (cp.ShortId.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) > 0)
            {
                return true;
            }
            if (cp.Description.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) > 0)
            {
                return true;
            }
            if (cp.PrimaryAddress?.Address?.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) > 0)
            {
                return true;
            }
            if (cp.Categories.Any(da => da.Name.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) > 0))
            {
                return true;
            }

            return false;
        }

        #endregion

        #region State

        class ContactsListFragmentState : IRetainableState
        {

            public Folder Folder { get; set; }

            public List<ContactPreview> ContactPreviews { get; set; }

            public List<ContactPreview> SelectedContactPreviews { get; set; }

            public bool RefreshInProgress { get; set; }
        }

        #endregion

        #region RecyclerView Adapter/ViewHolder

        class ContactsListAdapter : RecyclerView.Adapter
        {

            public List<ContactPreview> Items
            {
                get
                {
                    return contactPreviewsInView.ToList();
                }
            }

            public List<ContactPreview> SelectedItems
            {
                get
                {
                    return selectedContactsInView.Values.ToList();
                }
            }

            public override int ItemCount
            {
                get
                {
                    return contactPreviewsInView.Count;
                }
            }

            public int SelectedItemCount
            {
                get
                {
                    return selectedContactsInView.Count;
                }
            }

            readonly List<ContactPreview> contactPreviewsInView = new List<ContactPreview>(1000);
            readonly Dictionary<int, ContactPreview> selectedContactsInView = new Dictionary<int, ContactPreview>();

            public event EventHandler<ContactPreview> ItemClicked = delegate { };
            public event EventHandler<ContactPreview> ItemLongClicked = delegate { };

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var cpvh = holder as ContactPreviewViewHolder;
                if (cpvh == null) return;

                var cp = contactPreviewsInView[position];

                cpvh.ItemView.SetOnClickListener(new ActionOnClickListener(() => ItemClicked(this, cp)));
                cpvh.ItemView.SetOnLongClickListener(new ActionOnLongClickListener(() => ItemLongClicked(this, cp)));

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

            public void PrependItems(List<ContactPreview> items)
            {
                var count = items.Count;
                contactPreviewsInView.InsertRange(0, items);
                NotifyItemRangeInserted(0, count);
            }

            public void AppendItems(List<ContactPreview> items)
            {
                var count = contactPreviewsInView.Count;
                contactPreviewsInView.AddRange(items);
                NotifyItemRangeInserted(count, items.Count);
            }

            public void ReplaceItems(List<ContactPreview> items)
            {
                Clear();
                AppendItems(items);
            }

            public void Clear()
            {
                var size = contactPreviewsInView.Count;
                contactPreviewsInView.Clear();
                selectedContactsInView.Clear();
                NotifyItemRangeRemoved(0, size);
            }

            public bool IsSelected(ContactPreview contactPreview)
            {
                return selectedContactsInView.ContainsKey(contactPreview.Id);
            }

            public void SetSelected(List<ContactPreview> contactPreviews, bool selected)
            {
                foreach (var contact in contactPreviews)
                {
                    SetSelected(contact, selected);
                }
            }

            public void SetSelected(ContactPreview contactPreview, bool selected)
            {
                var position = GetPosition(contactPreview);
                if (position < 0) return;

                if (selected)
                {
                    selectedContactsInView[contactPreview.Id] = contactPreview;
                }
                else
                {
                    selectedContactsInView.Remove(contactPreview.Id);
                }
                NotifyItemChanged(position);
            }

            public void ClearSelections(bool notify = true)
            {
                var contacts = selectedContactsInView.Values.ToArray();
                selectedContactsInView.Clear();
                if (notify)
                {
                    foreach (var contact in contacts)
                    {
                        NotifyItemChanged(GetPosition(contact));
                    }
                }

            }

            public void RemoveItemsAtIndex(int index)
            {
                contactPreviewsInView.RemoveAt(index);
            }

            public int GetPosition(int contactPreviewId)
            {
                var position = -1;
                for (var i = 0; i < contactPreviewsInView.Count; i++)
                {
                    if (contactPreviewsInView[i].Id == contactPreviewId)
                    {
                        position = i;
                        break;
                    }
                }
                return position;
            }

            public int GetPosition(ContactPreview contactPreview)
            {
                var position = -1;
                for (var i = 0; i < contactPreviewsInView.Count; i++)
                {
                    if (contactPreviewsInView[i].Id == contactPreview.Id)
                    {
                        position = i;
                        break;
                    }
                }
                return position;
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

