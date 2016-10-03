//
// Project: Mark5.Mobile.Droid
// File: ShortcodesListFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{

    public class ShortcodesListFragment : RetainableStateFragment, ActionMode.ICallback, View.IOnClickListener, SearchView.IOnQueryTextListener, SearchView.IOnCloseListener
    {

        public Folder Folder
        {
            get;
            set;
        }

        bool refreshing;

        SwipeRefreshLayout refreshLayout;
        RecyclerView recyclerView;
        ShortcodesListAdapter adapter;
        ShortcodesListAdapter searchAdapter;
        ActionMode actionMode;
        SearchView searchView;

        CancellationTokenSource cts;

        readonly Handler searchHandler = new Handler();

        #region Fragment overrides

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(ShortcodesListFragment)} [folder.id={Folder?.Id}, folder.name={Folder?.Name}]...");

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

            adapter = new ShortcodesListAdapter();
            adapter.ItemClicked += Adapter_ItemClicked;
            adapter.ItemLongClicked += Adapter_ItemLongClicked;
            recyclerView.SetAdapter(adapter);

            searchAdapter = new ShortcodesListAdapter();
            searchAdapter.ItemClicked += Adapter_ItemClicked;
            searchAdapter.ItemLongClicked += Adapter_ItemLongClicked;

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = Folder?.Name;
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = GetString(Resource.String.shortcodes);

            CommonConfig.Logger.Info($"Created {nameof(ShortcodesListFragment)} [folder.id={Folder?.Id}, folder.name={Folder?.Name}]");
        }

        public override void OnResume()
        {
            base.OnResume();

            CommonConfig.Logger.Info($"Resuming {nameof(ShortcodesListFragment)} [folder.id={Folder?.Id}, folder.name={Folder?.Name}]...");

            if (adapter.ItemCount < 1)
            {
                RefreshData();
            }
        }

        public override void OnPause()
        {
            base.OnPause();

            CommonConfig.Logger.Info($"Pausing {nameof(ShortcodesListFragment)} [folder.id={Folder?.Id}, folder.name={Folder?.Name}]...");

            cts?.Cancel();
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.menu_main, menu);

            var searchItem = menu.FindItem(Resource.Id.action_search);
            searchView = (SearchView)MenuItemCompat.GetActionView(searchItem);
            searchView.QueryHint = GetString(Resource.String.filter);
            searchView.SetOnSearchClickListener(this);
            searchView.SetOnQueryTextListener(this);
            searchView.SetOnCloseListener(this);
        }

        #endregion

        #region RetainableStateFragment overrides

        public override IRetainableState OnRetainInstanceState()
        {
            CommonConfig.Logger.Info($"Retaining state [folder.id={Folder?.Id}, folder.name={Folder?.Name}, shortcodePreviews.Count={adapter?.ItemCount}/{adapter?.SelectedItemCount}, refreshing={refreshing}]...");

            return new ShortcodesListFragmentState
            {
                Folder = Folder,
                ShortcodePreviews = adapter.Items,
                SelectedShortcodePreviews = adapter.SelectedItems,
                RefreshInProgress = refreshing
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var dlfs = restoredState as ShortcodesListFragmentState;
            if (dlfs != null)
            {
                CommonConfig.Logger.Info($"Restoring state [dlfs.folder.id={dlfs.Folder.Id}, dlfs.items.count={dlfs.ShortcodePreviews.Count}, dlfs.selectedItems.count={dlfs.SelectedShortcodePreviews.Count}]...");

                Folder = dlfs.Folder;
                adapter.AppendItems(dlfs.ShortcodePreviews);

                if (dlfs.RefreshInProgress)
                {
                    CommonConfig.Logger.Info($"Refresh was in progress before - will continue...");

                    RefreshData(dlfs.ShortcodePreviews[dlfs.ShortcodePreviews.Count - 1].RowId);
                }

                if (dlfs.SelectedShortcodePreviews.Count > 0)
                {
                    actionMode?.Finish();
                    actionMode = Activity.StartActionMode(this);

                    adapter.SetSelected(dlfs.SelectedShortcodePreviews, true);
                    actionMode.Title = adapter.SelectedItemCount.ToString();
                    actionMode.Invalidate();
                }
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(ShortcodesListFragment)} [folder.id={Folder.Id}, folder.name={Folder.Name}]";
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

            Managers.ShortcodesManager.GetAllShortcodePreviews(Folder, cps =>
            {
                CommonConfig.Logger.Debug($"Retrieved {cps?.Count} contacts");

                Managers.DownloadManager.Notify(ObjectType.Shortcode, Folder.Id);
                Activity.RunOnUiThread(() => adapter.AppendItems(cps));
            }, () =>
            {
                refreshLayout.Post(() => refreshLayout.Refreshing = false); //Bug: fixed in support library v 24.2.0 (issue 77712)
                refreshing = false;

                CommonConfig.Logger.Info($"Refresh finished");
            }, ex =>
            {
                CommonConfig.Logger.Error($"Downloading shortcodes failed [folder.name={Folder?.Name}, folder.id={Folder?.Id}, startRowId={startRowId}, force={force}]", ex);

                Dialogs.ShowErrorDialog(Activity, ex);
            }, startRowId, cts.Token);
        }

        #endregion

        #region Adapter callbacks

        void Adapter_ItemClicked(object sender, ShortcodePreview shortcodePreview)
        {
            if (actionMode == null)
            {
                Android.Widget.Toast.MakeText(Activity, "Shortcode clicked!", Android.Widget.ToastLength.Short).Show();
            }
            else
            {
                var currentAdapter = (ShortcodesListAdapter)recyclerView.GetAdapter();
                currentAdapter.SetSelected(shortcodePreview, !currentAdapter.IsSelected(shortcodePreview));

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

        #endregion

        #region Action mode

        void Adapter_ItemLongClicked(object sender, ShortcodePreview shortcodePreview)
        {
            if (actionMode == null)
            {
                actionMode = Activity.StartActionMode(this);
            }

            Adapter_ItemClicked(sender, shortcodePreview);
        }

        public bool OnPrepareActionMode(ActionMode mode, IMenu menu)
        {
            menu.Clear();

            menu.Add(Menu.None, 30, 30, Resource.String.copy_to_worktray);
            menu.Add(Menu.None, 40, 40, Resource.String.copy_to_folder);

            if (Folder.InternalType == FolderInternalType.FilterView
                || Folder.InternalType == FolderInternalType.Static
                || Folder.InternalType == FolderInternalType.Worktray)
            {
                menu.Add(Menu.None, 41, 41, Resource.String.move_to_folder);
            }

            if (Folder.InternalType == FolderInternalType.FilterView
                || Folder.InternalType == FolderInternalType.Static
                || Folder.InternalType == FolderInternalType.Worktray)
            {
                menu.Add(Menu.None, 60, 60, Resource.String.delete_from_folder);
            }

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator
                || ServerConfig.SystemSettings.ShortcodesModuleInfo.Permissions.DeleteAllowed)
            {
                menu.Add(Menu.None, 61, 61, Resource.String.delete);
            }

            return true;
        }

        public bool OnCreateActionMode(ActionMode mode, IMenu menu)
        {
            return true;
        }

        public bool OnActionItemClicked(ActionMode mode, IMenuItem item)
        {
            return true;
        }

        public void OnDestroyActionMode(ActionMode mode)
        {
            var currentAdapter = (ShortcodesListAdapter)recyclerView.GetAdapter();
            currentAdapter.ClearSelections();
            actionMode = null;
        }

        #endregion

        #region Filtering

        void View.IOnClickListener.OnClick(View v)
        {
            if (v == searchView)
            {
                refreshLayout.Enabled = false;
                adapter.ClearSelections();
                recyclerView.SwapAdapter(searchAdapter, true);
            }
        }

        public bool OnQueryTextChange(string newText)
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

        public bool OnQueryTextSubmit(string query)
        {
            return false;
        }

        public bool OnClose()
        {
            searchHandler.RemoveCallbacksAndMessages(null);
            searchAdapter.Clear();
            recyclerView.SwapAdapter(adapter, true);
            refreshLayout.Enabled = true;
            return false;
        }

        static bool MatchesQuery(ShortcodePreview cp, string query)
        {
            if (cp.Name.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) > 0)
            {
                return true;
            }
            if (cp.Description.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) > 0)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region State

        class ShortcodesListFragmentState : IRetainableState
        {

            public Folder Folder { get; set; }

            public List<ShortcodePreview> ShortcodePreviews { get; set; }

            public List<ShortcodePreview> SelectedShortcodePreviews { get; set; }

            public bool RefreshInProgress { get; set; }
        }

        #endregion

        #region RecyclerView Adapter/ViewHolder

        class ShortcodesListAdapter : RecyclerView.Adapter
        {

            public List<ShortcodePreview> Items
            {
                get
                {
                    return shortcodePreviewsInView.ToList();
                }
            }

            public List<ShortcodePreview> SelectedItems
            {
                get
                {
                    return selectedShortcodesInView.Values.ToList();
                }
            }

            public override int ItemCount
            {
                get
                {
                    return shortcodePreviewsInView.Count;
                }
            }

            public int SelectedItemCount
            {
                get
                {
                    return selectedShortcodesInView.Count;
                }
            }

            readonly List<ShortcodePreview> shortcodePreviewsInView = new List<ShortcodePreview>(1000);
            readonly Dictionary<int, ShortcodePreview> selectedShortcodesInView = new Dictionary<int, ShortcodePreview>();

            public event EventHandler<ShortcodePreview> ItemClicked = delegate { };
            public event EventHandler<ShortcodePreview> ItemLongClicked = delegate { };

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var cpvh = holder as ShortcodePreviewViewHolder;
                if (cpvh == null) return;

                var cp = shortcodePreviewsInView[position];

                cpvh.ItemView.SetOnClickListener(new ActionOnClickListener(() => ItemClicked(this, cp)));
                cpvh.ItemView.SetOnLongClickListener(new ActionOnLongClickListener(() => ItemLongClicked(this, cp)));

                cpvh.Name = cp.Name;
                cpvh.Description = cp.Description;

                cpvh.Selected = selectedShortcodesInView.ContainsKey(cp.Id);
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_shortcodes, parent, false);
                return new ShortcodePreviewViewHolder(itemView);
            }

            public void PrependItems(List<ShortcodePreview> items)
            {
                var count = items.Count;
                shortcodePreviewsInView.InsertRange(0, items);
                NotifyItemRangeInserted(0, count);
            }

            public void AppendItems(List<ShortcodePreview> items)
            {
                var count = shortcodePreviewsInView.Count;
                shortcodePreviewsInView.AddRange(items);
                NotifyItemRangeInserted(count, items.Count);
            }

            public void ReplaceItems(List<ShortcodePreview> items)
            {
                Clear();
                AppendItems(items);
            }

            public void Clear()
            {
                var size = shortcodePreviewsInView.Count;
                shortcodePreviewsInView.Clear();
                selectedShortcodesInView.Clear();
                NotifyItemRangeRemoved(0, size);
            }

            public bool IsSelected(ShortcodePreview shortcodePreview)
            {
                return selectedShortcodesInView.ContainsKey(shortcodePreview.Id);
            }

            public void SetSelected(List<ShortcodePreview> shortcodePreviews, bool selected)
            {
                foreach (var shortcode in shortcodePreviews)
                {
                    SetSelected(shortcode, selected);
                }
            }

            public void SetSelected(ShortcodePreview shortcodePreview, bool selected)
            {
                var position = GetPosition(shortcodePreview);
                if (position < 0) return;

                if (selected)
                {
                    selectedShortcodesInView[shortcodePreview.Id] = shortcodePreview;
                }
                else
                {
                    selectedShortcodesInView.Remove(shortcodePreview.Id);
                }
                NotifyItemChanged(position);
            }

            public void ClearSelections()
            {
                var shortcodes = selectedShortcodesInView.Values.ToArray();
                selectedShortcodesInView.Clear();
                foreach (var shortcode in shortcodes)
                {
                    NotifyItemChanged(GetPosition(shortcode));
                }
            }

            int GetPosition(ShortcodePreview shortcodePreview)
            {
                var position = -1;
                for (var i = 0; i < shortcodePreviewsInView.Count; i++)
                {
                    if (shortcodePreviewsInView[i].Id == shortcodePreview.Id)
                    {
                        position = i;
                        break;
                    }
                }
                return position;
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
                    letterTextView.Text = value.Substring(0, 1).ToUpper();

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

