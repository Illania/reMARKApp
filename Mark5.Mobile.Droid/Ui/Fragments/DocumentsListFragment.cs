using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Support.V7.Widget.Helper;
using Android.Text;
using Android.Util;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.HubMessages;
using Mark5.Mobile.Common.Service;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class DocumentsListFragment : AbstractDocumentsListFragment, ActionMode.ICallback
    {
        const string SelectedDocumentPreviewsKey = "SelectedDocumentPreviews_9d33e0b7-9791-4ee9-82bd-73af5c0b5716";

        const int AutoRefreshIntervalMs = 5 * 1000; // 5 seconds

        protected List<DocumentPreview> savedSelectedDocumentPreviews;

        bool selectEnabled = true;

        SwipeHelperCallback swipeHelperCallback;
        ItemTouchHelper itemTouchHelper;
        SearchView searchView;
        FloatingActionButton fab;

        AutoRefreshWorker autoRefreshWorker;

        public static (DocumentsListFragment fragment, string tag) NewInstance(Folder folder)
        {
            var args = new Bundle();

            if (folder != null)
                args.PutString(FolderBundleKey, Serializer.Serialize(folder));

            var fragment = new DocumentsListFragment();
            fragment.Arguments = args;

            var tag = $"{nameof(DocumentsListFragment)} [folder.id={folder.Id}, folder.name={folder.Name}]";

            return (fragment, tag);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (savedInstanceState?.ContainsKey(SelectedDocumentPreviewsKey) == true)
                savedSelectedDocumentPreviews = Serializer.Deserialize<List<DocumentPreview>>(savedInstanceState.GetString(SelectedDocumentPreviewsKey));
        }

        #region Fragment overrides

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var rootView = base.OnCreateView(inflater, container, savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(DocumentsListFragment)} [folder.id={Folder?.Id}, folder.name={Folder?.Name}]...");

            adapter.ItemLongClicked += Adapter_ItemLongClicked;

            swipeHelperCallback = new SwipeHelperCallback(Context, this, adapter, refreshLayout, Folder);
            itemTouchHelper = new ItemTouchHelper(swipeHelperCallback);
            itemTouchHelper.AttachToRecyclerView(recyclerView);

            searchAdapter.ItemLongClicked += Adapter_ItemLongClicked;

            fab = ((BaseAppCompatActivity)Activity).Fab;
            fab.SetImageResource(Resource.Drawable.action_new);
            fab.SetOnClickListener(new ActionOnClickListener(ComposeDocument));
            fab.Visibility = ViewStates.Visible;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            CommonConfig.Logger.Info($"Created {nameof(DocumentsListFragment)} [folder.id={Folder?.Id}, folder.name={Folder?.Name}]");
        }

        public override void OnResume()
        {
            base.OnResume();

            CommonConfig.Logger.Info($"Resuming {nameof(DocumentsListFragment)} [folder.id={Folder?.Id}, folder.name={Folder?.Name}]...");

            CommonConfig.Logger.Info($"Starting automatic refresh...");

            autoRefreshWorker?.Stop();
            autoRefreshWorker = new AutoRefreshWorker(AutoRefreshData, () => { return adapter?.Items?.FirstOrDefault(); }, AutoRefreshIntervalMs);
            autoRefreshWorker.Start();

            CommonConfig.Logger.Info($"Started automatic refresh");
        }

        public override void OnPause()
        {
            base.OnPause();

            CommonConfig.Logger.Info($"Pausing {nameof(DocumentsListFragment)} [folder.id={Folder?.Id}, folder.name={Folder?.Name}]...");

            CommonConfig.Logger.Info($"Stopping automatic refresh...");

            autoRefreshWorker?.Stop();

            CommonConfig.Logger.Info($"Stopped automatic refresh");
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            if (adapter?.SelectedItems != null || savedSelectedDocumentPreviews != null)
                outState.PutString(SelectedDocumentPreviewsKey, Serializer.Serialize(adapter?.SelectedItems ?? savedSelectedDocumentPreviews));
        }

        #endregion

        #region Refreshing

        virtual protected async Task AutoRefreshData(int endId)
        {
            try
            {
                CommonConfig.Logger.Debug($"Attempting automatic refresh [endId={endId}, !isAdded={!IsAdded}, isDetached={IsDetached}, isRemoving={IsRemoving}, refreshing={refreshing}]...");

                if (!IsAdded || IsDetached || IsRemoving)
                    return;
                if (refreshing)
                    return;

                refreshing = true;

                CommonConfig.Logger.Debug($"Automatic refresh running...");

                var documents = await Managers.DocumentsManager.GetDocumentPreviewsAsync(Folder, endId: endId);

                if (documents.Count > 0)
                {
                    CommonConfig.Logger.Info($"Received {documents?.Count} new documents");

                    var snackbar = Snackbar.Make(coordinatorLayout, Resources.GetQuantityString(Resource.Plurals.new_documents_received, documents.Count, documents.Count), Snackbar.LengthShort);
                    snackbar.View.SetBackgroundColor(new Color(ContextCompat.GetColor(Activity, Resource.Color.darkerblue)));
                    snackbar.Show();

                    Activity?.RunOnUiThread(() => { adapter?.PrependItems(documents); });

                    Services.DocumentsDownloadService.Notify();
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Automatic refresh failed [endId={endId}]", ex);
            }
            finally
            {
                refreshing = false;

                CommonConfig.Logger.Debug($"Automatic refresh finished");
            }
        }

        protected override async Task RefreshData(int startId = -1, int endId = -1, bool forceClear = false)
        {
            await base.RefreshData(startId, endId, forceClear);

            if (savedSelectedDocumentPreviews?.Count > 0)
            {
                actionMode?.Finish();
                actionMode = Activity.StartActionMode(this);

                adapter.SetSelected(savedSelectedDocumentPreviews, true);
                actionMode.Title = adapter.SelectedItemCount.ToString();
                actionMode.Invalidate();

                savedSelectedDocumentPreviews = null;
            }
        }

        #endregion

        #region Adapter callbacks

        protected void Adapter_ItemLongClicked(object sender, DocumentPreview documentPreview)
        {
            if (actionMode == null)
                actionMode = Activity.StartActionMode(this);

            Adapter_ItemClicked(sender, documentPreview);
        }

        #endregion

        #region Action mode

        public void ShowCategories(DocumentPreview documentPreview)
        {
            StartActivity(CategoriesListActivity.CreateIntent(Context, documentPreview));
        }

        void ComposeDocument()
        {
            if (!ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines.Any())
            {
                Dialogs.ShowConfirmDialog(Activity, Resource.String.no_lines_error_title, Resource.String.no_lines_error_content);
                return;
            }

            StartActivity(ComposeDocumentActivity.CreateIntent(Context, DocumentCreationModeFlag.New, CopyToNewOption.None));
        }

        bool ActionMode.ICallback.OnCreateActionMode(ActionMode mode, IMenu menu)
        {
            return true;
        }

        bool ActionMode.ICallback.OnPrepareActionMode(ActionMode mode, IMenu menu)
        {
            Activity.Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
            Activity.Window.SetStatusBarColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));

            swipeHelperCallback.Enabled = false;
            fab?.Hide();

            menu.Clear();

            var selectAllItem = menu.Add(Menu.None, MenuItemActions.SelectDeselectAll, MenuItemActions.SelectDeselectAll, "");
            if (selectEnabled)
            {
                selectAllItem.SetTitle(Resource.String.select_all);
                selectAllItem.SetIcon(Resource.Drawable.action_select_all);
            }
            else
            {
                selectAllItem.SetTitle(Resource.String.deselect_all);
                selectAllItem.SetIcon(Resource.Drawable.action_deselect_all);
            }

            if (CurrentAdapter.SelectedItems.Any(dp => !dp.IsReadByCurrent) || !CurrentAdapter.SelectedItems.Any())
                menu.Add(MenuItemGroup.Actions, MenuItemActions.MarkAsRead, MenuItemActions.MarkAsRead, Resource.String.mark_as_read);

            if (CurrentAdapter.SelectedItems.Any(dp => dp.IsReadByCurrent))
                menu.Add(MenuItemGroup.Actions, MenuItemActions.MarkAsUnread, MenuItemActions.MarkAsUnread, Resource.String.marks_as_unread);

            if (ServerConfig.SystemSettings.DocumentsModuleInfo.WorktrayEnabled ?? true)
                menu.Add(MenuItemGroup.Actions, MenuItemActions.CopyToWorktray, MenuItemActions.CopyToWorktray, Resource.String.copy_to_worktray);

            menu.Add(MenuItemGroup.Actions, MenuItemActions.CopyToFolder, MenuItemActions.CopyToFolder, Resource.String.copy_to_folder);

            if (Folder.InternalType == FolderInternalType.FilterView || Folder.InternalType == FolderInternalType.Static || Folder.InternalType == FolderInternalType.Worktray)
                menu.Add(MenuItemGroup.Actions, MenuItemActions.MoveToFolder, MenuItemActions.MoveToFolder, Resource.String.move_to_folder);

            menu.Add(MenuItemGroup.Actions, MenuItemActions.SetPriority, MenuItemActions.SetPriority, Resource.String.set_priority);

            if (CurrentAdapter.SelectedItemCount == 1)
                menu.Add(MenuItemGroup.Actions, MenuItemActions.Categories, MenuItemActions.Categories, Resource.String.categories);

            if (Folder.InternalType == FolderInternalType.FilterView || Folder.InternalType == FolderInternalType.Static || Folder.InternalType == FolderInternalType.Worktray)
                menu.Add(MenuItemGroup.Actions, MenuItemActions.DeleteFromFolder, MenuItemActions.DeleteFromFolder, Resource.String.delete_from_folder);

            if (ServerConfig.SystemSettings.DocumentsModuleInfo.Permissions.DeleteAllowed || CurrentAdapter.SelectedItems.All(dp => dp.Direction == DocumentDirection.Draft))
                menu.Add(MenuItemGroup.Actions, MenuItemActions.Delete, MenuItemActions.Delete, Resource.String.delete);

            menu.SetGroupEnabled(MenuItemGroup.Actions, CurrentAdapter.SelectedItems.Any());

            return true;
        }

        bool ActionMode.ICallback.OnActionItemClicked(ActionMode mode, IMenuItem item)
        {
            if (item.ItemId == MenuItemActions.MarkAsRead)
            {
                MarkAsRead(CurrentAdapter.SelectedItems);
                return true;
            }

            if (item.ItemId == MenuItemActions.MarkAsUnread)
            {
                MarkAsUnread(CurrentAdapter.SelectedItems);
                return true;
            }

            if (item.ItemId == MenuItemActions.CopyToWorktray)
            {
                CopyToWorktrayAction(CurrentAdapter.SelectedItems);
                return true;
            }

            if (item.ItemId == MenuItemActions.CopyToFolder)
            {
                StartActivity(CopyMoveToFolderListActivity.CreateIntent(Context, CopyMoveToFolderListActivity.ModeType.Copy, ModuleType.Documents,
                                                                        CurrentAdapter.SelectedItems.Select(sp => sp).Cast<IBusinessEntity>().ToList()));
                actionMode?.Finish();
                return true;
            }

            if (item.ItemId == MenuItemActions.MoveToFolder)
            {
                StartActivity(CopyMoveToFolderListActivity.CreateIntent(Context, CopyMoveToFolderListActivity.ModeType.Move, ModuleType.Documents,
                                                                        CurrentAdapter.SelectedItems.Select(sp => sp).Cast<IBusinessEntity>().ToList(), Folder));
                actionMode?.Finish();
                return true;
            }

            if (item.ItemId == MenuItemActions.SetPriority)
            {
                SetPriority(CurrentAdapter.SelectedItems);
                return true;
            }

            if (item.ItemId == MenuItemActions.SelectDeselectAll)
            {
                SelectDeselectAll(item);
                return true;
            }

            if (item.ItemId == MenuItemActions.Categories)
            {
                ShowCategories(CurrentAdapter.SelectedItems.First());

                actionMode?.Finish();
                return true;
            }

            if (item.ItemId == MenuItemActions.DeleteFromFolder)
            {
                DeleteFromFolderAction(CurrentAdapter.SelectedItems);
                return true;
            }

            if (item.ItemId == MenuItemActions.Delete)
            {
                DeleteAction(CurrentAdapter.SelectedItems);
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        void ActionMode.ICallback.OnDestroyActionMode(ActionMode mode)
        {
            Activity.Window.AddFlags(WindowManagerFlags.TranslucentStatus);
            Activity.Window.SetStatusBarColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));

            swipeHelperCallback.Enabled = true;
            fab?.Show();

            CurrentAdapter.ClearSelections();
            actionMode = null;
            selectEnabled = true;
        }

        public async Task CopyToOwnWorktray(List<DocumentPreview> documentPreviews)
        {
            CommonConfig.Logger.Info($"Attempting copy to worktray [businessEntities.Count={documentPreviews.Count}]...");

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.copying_to_worktray, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.CopyToWorktray(documentPreviews.OfType<IBusinessEntity>().ToList());

                dismissAction();
                actionMode?.Finish();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Copying to worktray failed [businessEntities.Count={CurrentAdapter.SelectedItemCount}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        async void MarkAsRead(List<DocumentPreview> items)
        {
            CommonConfig.Logger.Info($"Attempting to mark as read [businessEntities.Count={items.Count}]...");

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.marking_as_read, Resource.String.please_wait);

            try
            {
                CommonConfig.UsageAnalytics.LogEvent(new SetReadStatusEvent(items.Count));

                await Managers.DocumentsManager.SetDocumentsReadStatusAsync(items, true);
                adapter.RefreshItems(items);
                searchAdapter.RefreshItems(items);

                dismissAction();
                actionMode?.Finish();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Marking as read failed [businessEntities.Count={items.Count}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        async void MarkAsUnread(List<DocumentPreview> items)
        {
            CommonConfig.Logger.Info($"Attempting to mark as unread [businessEntities.Count={items.Count}]...");

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.marking_as_unread, Resource.String.please_wait);

            try
            {
                CommonConfig.UsageAnalytics.LogEvent(new SetReadStatusEvent(items.Count));
                await Managers.DocumentsManager.SetDocumentsReadStatusAsync(items, false);
                adapter.RefreshItems(items);
                searchAdapter.RefreshItems(items);

                dismissAction();
                actionMode?.Finish();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Marking as unread failed [businessEntities.Count={items.Count}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        async void CopyToWorktrayAction(List<DocumentPreview> documentPreviews)
        {
            var option = await Dialogs.ShowListDialog(Context, Resource.String.copy_to_worktray, Resource.Array.copy_to_worktray_options, true);

            if (option == 0)
                await CopyToOwnWorktray(documentPreviews);

            if (option == 1)
            {
                StartActivity(CopyToUserWorktrayActivity.CreateIntent(Context, documentPreviews.Cast<IBusinessEntity>().ToList()));
            }
        }

        async void CopyToOwnWorktray(DocumentPreview documentPreview)
        {
            await CopyToOwnWorktray(new List<DocumentPreview>
            {
                documentPreview
            });
        }

        async void SetPriority(List<DocumentPreview> items)
        {
            var possiblePriorities = new List<Priority>
            {
                Priority.Urgent,
                Priority.Normal,
                Priority.Low
            };

            var selectedPriority = items.All(dp => dp.Priority == items[0].Priority) ? items[0].Priority : Priority.None;

            if (!possiblePriorities.Contains(selectedPriority))
                selectedPriority = Priority.Normal;

            var priority = await Dialogs.ShowSingleSelectDialogAsync(Context, Resource.String.set_priority, possiblePriorities, selectedPriority);
            if (priority == default(Priority) || priority == selectedPriority)
                return;

            CommonConfig.Logger.Info($"Attempting to set priority [businessEntities.Count={items.Count}]...");

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.setting_priority, Resource.String.please_wait);

            try
            {
                await Managers.DocumentsManager.SetDocumentsPriorityAsync(items, priority);

                dismissAction();
                actionMode?.Finish();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Setting priority failed [businessEntities.Count={items.Count}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }

        }

        void SelectDeselectAll(IMenuItem item)
        {
            if (selectEnabled)
                CurrentAdapter.SetSelected(CurrentAdapter.Items, true);
            else
                CurrentAdapter.SetSelected(CurrentAdapter.Items, false);

            actionMode.Title = CurrentAdapter.SelectedItemCount.ToString();
            selectEnabled = !selectEnabled;
            actionMode.Invalidate();
        }

        async void DeleteFromFolderAction(List<DocumentPreview> items)
        {
            var yesNo = await Dialogs.ShowYesNoDialogAsync(Context, Resource.String.delete_from_folder, Resource.String.delete_from_folder_are_you_sure);
            if (!yesNo)
                return;

            CommonConfig.Logger.Info($"Attempting to delete from folder [businessEntities.Count={items.Count}]...");

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.deleting_from_folder, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.RemoveFromFolder(items.OfType<IBusinessEntity>().ToList(), Folder);

                dismissAction();
                actionMode?.Finish();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Deleting from folder failed [businessEntities.Count={items.Count}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        async void DeleteAction(List<DocumentPreview> items)
        {
            var yesNo = await Dialogs.ShowYesNoDialogAsync(Context, Resource.String.delete, Resource.String.delete_are_you_sure);
            if (!yesNo)
                return;

            CommonConfig.Logger.Info($"Attempting to delete [businessEntities.Count={items.Count}]...");

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.deleting, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.Delete(items.OfType<IBusinessEntity>().ToList());

                dismissAction();
                actionMode?.Finish();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Deleting failed [businessEntities.Count={items.Count}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        static class MenuItemActions
        {
            public const int SelectDeselectAll = 05;
            public const int MarkAsRead = 10;
            public const int MarkAsUnread = 11;
            public const int CopyToWorktray = 20;
            public const int CopyToFolder = 30;
            public const int MoveToFolder = 31;
            public const int SetPriority = 40;
            public const int Categories = 50;
            public const int DeleteFromFolder = 60;
            public const int Delete = 61;
        }

        static class MenuItemGroup
        {
            public const int Actions = 1;
        }

        #endregion

        #region Messenger hub related

        public void UpdateReadStatus(DocumentPreviewReadStatusChangedMessage m)
        {
            var position = adapter.GetPosition(m.DocumentPreviewId);
            if (position >= 0)
            {
                shouldNotifyAdapter = true;
                var dp = adapter.Items[position];
                dp.IsReadByCurrent = m.IsReadByCurrent;
                dp.IsReadByAnyone = m.IsReadByAnyone;
            }

            position = searchAdapter.GetPosition(m.DocumentPreviewId);
            if (position >= 0)
            {
                shouldNotifySearchAdapter = true;
                var dp = searchAdapter.Items[position];
                dp.IsReadByCurrent = m.IsReadByCurrent;
                dp.IsReadByAnyone = m.IsReadByAnyone;
            }
        }

        public void UpdatePriority(DocumentPreviewPriorityChangedMessage m)
        {
            var position = adapter.GetPosition(m.DocumentPreviewId);
            if (position >= 0)
            {
                shouldNotifyAdapter = true;
                var dp = adapter.Items[position];
                dp.Priority = m.Priority;
            }

            position = searchAdapter.GetPosition(m.DocumentPreviewId);
            if (position >= 0)
            {
                shouldNotifySearchAdapter = true;
                var dp = searchAdapter.Items[position];
                dp.Priority = m.Priority;
            }
        }

        public void UpdateCategories(EntityCategoriesChangedMessage m)
        {
            var position = adapter.GetPosition(m.EntityId);
            if (position >= 0)
            {
                shouldNotifyAdapter = true;
                var dp = adapter.Items[position];
                dp.Categories.Clear();
                dp.Categories.AddRange(m.Categories);
            }

            position = searchAdapter.GetPosition(m.EntityId);
            if (position >= 0)
            {
                shouldNotifySearchAdapter = true;
                var dp = searchAdapter.Items[position];
                dp.Categories.Clear();
                dp.Categories.AddRange(m.Categories);
            }
        }

        public void UpdateCommentsCount(EntityPreviewCommentCountChangedMessage m)
        {
            var position = adapter.GetPosition(m.EntityId);
            if (position >= 0)
            {
                shouldNotifyAdapter = true;
                var dp = adapter.Items[position];
                dp.CommentsCount = m.CommentsCount;
            }

            position = searchAdapter.GetPosition(m.EntityId);
            if (position >= 0)
            {
                shouldNotifySearchAdapter = true;
                var dp = searchAdapter.Items[position];
                dp.CommentsCount = m.CommentsCount;
            }
        }

        public void UpdateMovedFromFolderEntities(EntityMovedFromFolderMessage m)
        {
            foreach (var entityId in m.EntitiesId)
            {
                var position = adapter.GetPosition(entityId);
                if (position >= 0)
                {
                    shouldNotifyAdapter = true;
                    adapter.RemoveItemAtPosition(position);
                }

                position = searchAdapter.GetPosition(entityId);
                if (position >= 0)
                {
                    shouldNotifySearchAdapter = true;
                    searchAdapter.RemoveItemAtPosition(position);
                }
            }
        }

        public void UpdateRemovedFromFolderEntities(EntityRemovedFromFolderMessage m)
        {
            foreach (var entityId in m.EntitiesId)
            {
                var position = adapter.GetPosition(entityId);
                if (position >= 0)
                {
                    shouldNotifyAdapter = true;
                    adapter.RemoveItemAtPosition(position);
                }

                position = searchAdapter.GetPosition(entityId);
                if (position >= 0)
                {
                    shouldNotifySearchAdapter = true;
                    searchAdapter.RemoveItemAtPosition(position);
                }
            }
        }

        public void UpdateRemovedEntities(EntityRemovedMessage m)
        {
            foreach (var entityId in m.EntitiesId)
            {
                var position = adapter.GetPosition(entityId);
                if (position >= 0)
                {
                    shouldNotifyAdapter = true;
                    adapter.RemoveItemAtPosition(position);
                }

                position = searchAdapter.GetPosition(entityId);
                if (position >= 0)
                {
                    shouldNotifySearchAdapter = true;
                    searchAdapter.RemoveItemAtPosition(position);
                }
            }
        }

        #endregion

        #region RecyclerView Adapter/ViewHolder

        class SwipeHelperCallback : ItemTouchHelper.Callback
        {
            public bool Enabled { get; set; } = true;

            readonly Context context;
            readonly DocumentsListAdapter adapter;
            readonly DocumentsListFragment fragment;
            readonly SwipeRefreshLayout refreshLayout;
            readonly Folder folder;

            Drawable leftBackground;
            Drawable rightBackground;

            public SwipeHelperCallback(Context context, DocumentsListFragment fragment, DocumentsListAdapter adapter, SwipeRefreshLayout refreshLayout, Folder folder)
            {
                this.context = context;
                this.fragment = fragment;
                this.adapter = adapter;
                this.refreshLayout = refreshLayout;
                this.folder = folder;
            }

            public override int GetMovementFlags(RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder)
            {
                if (!Enabled)
                    return MakeMovementFlags(0, 0);

                return MakeMovementFlags(0, ItemTouchHelper.Left | ItemTouchHelper.Right);
            }

            public override bool OnMove(RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder, RecyclerView.ViewHolder target)
            {
                return false;
            }

            public override void OnSelectedChanged(RecyclerView.ViewHolder viewHolder, int actionState)
            {
                base.OnSelectedChanged(viewHolder, actionState);

                refreshLayout.Enabled = actionState == ItemTouchHelper.ActionStateIdle;
            }

            public override void OnChildDraw(Canvas c, RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder, float dX, float dY, int actionState, bool isCurrentlyActive)
            {

                if (actionState != ItemTouchHelper.ActionStateSwipe || viewHolder.AdapterPosition == -1) //Sometimes it gets called for viewHolders that are already gone
                    return;

                var itemView = viewHolder.ItemView;
                var itemViewHeight = itemView.Bottom - itemView.Top;

                var paint = new TextPaint();
                paint.TextSize = (int)TypedValue.ApplyDimension(ComplexUnitType.Sp, 14, Android.App.Application.Context.Resources.DisplayMetrics);
                paint.Color = Color.White;
                paint.TextAlign = Paint.Align.Left;
                paint.SetTypeface(Typeface.Create(Typeface.Default, TypefaceStyle.Normal));

                var iconMargin = Conversion.ConvertDpToPixels(30);

                var baseline = -paint.Ascent();
                var textHeight = (int)(baseline + paint.Descent() + 0.5f);

                if (dX > 0) //Swiping to right
                {
                    Preferences.EmailSwipeAction action = PlatformConfig.Preferences.EmailLeadingSwipeAction;
                    int bgColor = SwipeActionAllowed(action) ? Resource.Color.brown : Resource.Color.lightgray;
                    leftBackground = new ColorDrawable(new Color(ContextCompat.GetColor(context, bgColor)));
                    string text = GetSwipeActionTitle(action, viewHolder.AdapterPosition);
                    leftBackground.SetBounds(itemView.Left, itemView.Top, (int)dX, itemView.Bottom);
                    leftBackground.Draw(c);
                    var textLayout = new StaticLayout(text, paint, c.Width, Layout.Alignment.AlignNormal, 1, 0, false);
                    var textLeft = itemView.Left + iconMargin;
                    var textTop = itemView.Top + (itemViewHeight - textHeight) / 2;

                    c.Save();
                    c.Translate(textLeft, textTop);
                    textLayout.Draw(c);
                    c.Restore();
                }
                else if (dX < 0)
                {
                    Preferences.EmailSwipeAction action = PlatformConfig.Preferences.EmailTrailingSwipeAction;
                    int bgColor = SwipeActionAllowed(action) ? Resource.Color.darkblue : Resource.Color.lightgray;
                    rightBackground = new ColorDrawable(new Color(ContextCompat.GetColor(context, bgColor)));
                    string text = GetSwipeActionTitle(action, viewHolder.AdapterPosition);
                    rightBackground.SetBounds(itemView.Right + (int)dX, itemView.Top, itemView.Right, itemView.Bottom);
                    rightBackground.Draw(c);
                    var textLayout = new StaticLayout(text, paint, c.Width, Layout.Alignment.AlignNormal, 1, 0, false);
                    var iconWidth = text.Split(new string[]
                            {
                                "\n"
                            },
                            StringSplitOptions.None)
                        .Select(s => (int)(paint.MeasureText(s) + 0.5f))
                        .Max();

                    var textRight = itemView.Right - iconMargin;
                    var textLeft = textRight - iconWidth;
                    var textTop = itemView.Top + itemViewHeight / 2 - textHeight;

                    c.Save();
                    c.Translate(textLeft, textTop);
                    textLayout.Draw(c);
                    c.Restore();
                }

                base.OnChildDraw(c, recyclerView, viewHolder, dX, dY, actionState, isCurrentlyActive);
            }

            public override void OnSwiped(RecyclerView.ViewHolder viewHolder, int direction)
            {
                ResetViewHolder(viewHolder, direction);
                if (direction == ItemTouchHelper.Left)
                {
                    SwipeActionSelected(PlatformConfig.Preferences.EmailTrailingSwipeAction, viewHolder);

                }
                else if (direction == ItemTouchHelper.Right)
                {
                    SwipeActionSelected(PlatformConfig.Preferences.EmailLeadingSwipeAction, viewHolder);

                }
            }

            void ResetViewHolder(RecyclerView.ViewHolder viewHolder, int direction)
            {
                var position = viewHolder.AdapterPosition;
                var view = viewHolder.ItemView;

                viewHolder.ItemView.TranslationX = 0;
                viewHolder.ItemView.TranslationY = 0;

                adapter.SetSwipedState(position, 0);
                adapter.NotifyItemChanged(position);
            }

            async void SwipeActionSelected(Preferences.EmailSwipeAction action, RecyclerView.ViewHolder viewHolder)
            {
                CommonConfig.UsageAnalytics.LogEvent(new SwipeActionUsedEvent());

                if (SwipeActionAllowed(action))
                {
                    switch (action)
                    {
                        case Preferences.EmailSwipeAction.Delete:
                            fragment.DeleteAction(new List<DocumentPreview>() { adapter.Items[viewHolder.AdapterPosition] });
                            break;
                        case Preferences.EmailSwipeAction.CopyToFolder:
                            fragment.StartActivity(CopyMoveToFolderListActivity.CreateIntent(context, CopyMoveToFolderListActivity.ModeType.Copy, ModuleType.Documents,
                                                                                             new List<DocumentPreview> { adapter.Items[viewHolder.AdapterPosition] }.Cast<IBusinessEntity>().ToList(), folder));
                            break;
                        case Preferences.EmailSwipeAction.More:
                            var index = await Dialogs.ShowListDialog(context, Resource.String.pref_email_swipe_dialog_title, Resource.Array.pref_email_swipe_actions_entries_without_more, true);
                            if (index >= 0)
                            {
                                SwipeActionSelected(PlatformConfig.Preferences.GetAllAvailableActions()[index], viewHolder);
                            }
                            break;
                        case Preferences.EmailSwipeAction.CopyToWorkTray:
                            fragment.CopyToWorktrayAction(new List<DocumentPreview>() { adapter.Items[viewHolder.AdapterPosition] });
                            break;
                        case Preferences.EmailSwipeAction.Categories:
                            fragment.ShowCategories(adapter.Items[viewHolder.AdapterPosition]);
                            break;
                        case Preferences.EmailSwipeAction.MarkAsReadUnread:
                            if (!adapter.Items[viewHolder.AdapterPosition].IsReadByCurrent)
                            {
                                fragment.MarkAsRead(new List<DocumentPreview>() { adapter.Items[viewHolder.AdapterPosition] });
                            }
                            if (adapter.Items[viewHolder.AdapterPosition].IsReadByCurrent)
                            {
                                fragment.MarkAsUnread(new List<DocumentPreview>() { adapter.Items[viewHolder.AdapterPosition] });
                            }
                            break;
                        case Preferences.EmailSwipeAction.RemoveFromFolder:
                            fragment.DeleteFromFolderAction(new List<DocumentPreview>() { adapter.Items[viewHolder.AdapterPosition] });
                            break;
                        case Preferences.EmailSwipeAction.Priorities:
                            fragment.SetPriority(new List<DocumentPreview>() { adapter.Items[viewHolder.AdapterPosition] });
                            break;
                        case Preferences.EmailSwipeAction.MoveToFolder:
                            fragment.StartActivity(CopyMoveToFolderListActivity.CreateIntent(context, CopyMoveToFolderListActivity.ModeType.Move, ModuleType.Documents,
                                                                                             new List<DocumentPreview> { adapter.Items[viewHolder.AdapterPosition] }.Cast<IBusinessEntity>().ToList(), folder));
                            break;

                    }
                }

            }

            bool SwipeActionAllowed(Preferences.EmailSwipeAction action)
            {
                switch (action)
                {
                    case Preferences.EmailSwipeAction.CopyToWorkTray:
                        return ServerConfig.SystemSettings.DocumentsModuleInfo.WorktrayEnabled ?? true;
                    case Preferences.EmailSwipeAction.Delete:
                        return ServerConfig.SystemSettings.DocumentsModuleInfo.Permissions.DeleteAllowed || adapter.SelectedItems.All(dp => dp.Direction == DocumentDirection.Draft);
                    case Preferences.EmailSwipeAction.MoveToFolder:
                        return folder.InternalType == FolderInternalType.FilterView || folder.InternalType == FolderInternalType.Static || folder.InternalType == FolderInternalType.Worktray;
                    case Preferences.EmailSwipeAction.RemoveFromFolder:
                        return folder.InternalType == FolderInternalType.FilterView || folder.InternalType == FolderInternalType.Static || folder.InternalType == FolderInternalType.Worktray;
                    default:
                        return true;
                }
            }

            string GetSwipeActionTitle(Preferences.EmailSwipeAction action, int position)
            {
                switch (action)
                {
                    case Preferences.EmailSwipeAction.Delete:
                        return context.Resources.GetString(Resource.String.delete);
                    case Preferences.EmailSwipeAction.More:
                        return context.Resources.GetString(Resource.String.more);
                    case Preferences.EmailSwipeAction.MoveToFolder:
                        return context.Resources.GetString(Resource.String.move_to_folder);
                    case Preferences.EmailSwipeAction.MarkAsReadUnread:
                        if (!adapter.Items[position].IsReadByCurrent)
                        {
                            return context.Resources.GetString(Resource.String.mark_as_read);
                        }
                        if (adapter.Items[position].IsReadByCurrent)
                        {
                            return context.Resources.GetString(Resource.String.marks_as_unread);
                        }
                        return "";
                    case Preferences.EmailSwipeAction.Categories:
                        return context.Resources.GetString(Resource.String.categories);
                    case Preferences.EmailSwipeAction.RemoveFromFolder:
                        return context.Resources.GetString(Resource.String.remove_from_folder);
                    case Preferences.EmailSwipeAction.CopyToWorkTray:
                        return context.Resources.GetString(Resource.String.copy_to_worktray);
                    case Preferences.EmailSwipeAction.CopyToFolder:
                        return context.Resources.GetString(Resource.String.copy_to_folder);
                    case Preferences.EmailSwipeAction.Priorities:
                        return context.Resources.GetString(Resource.String.priority);
                    default:
                        return "Forgot case ?";
                }
            }
        }

        #endregion

        class AutoRefreshWorker
        {
            CancellationTokenSource cts;

            readonly Func<int, Task> work;
            readonly Func<DocumentPreview> firstOrDefaultItem;
            readonly int intervalMs;

            readonly object lockObj = new object();

            public AutoRefreshWorker(Func<int, Task> work, Func<DocumentPreview> firstOrDefaultItem, int intervalMs)
            {
                this.work = work;
                this.firstOrDefaultItem = firstOrDefaultItem;
                this.intervalMs = intervalMs;
            }

            public void Start()
            {
                lock (lockObj)
                {
                    cts?.Cancel();
                    cts = new CancellationTokenSource();
                    Task.Run(async () =>
                    {
                        while (true)
                        {
                            await Task.Delay(intervalMs);
                            if (cts.IsCancellationRequested)
                                break;

                            var first = firstOrDefaultItem();
                            if (first != null)
                                await work(first.Id);
                        }
                    });
                }
            }

            public void Stop()
            {
                lock (lockObj)
                {
                    cts?.Cancel();
                }
            }
        }
    }
}