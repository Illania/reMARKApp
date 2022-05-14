using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Support.V7.Widget.Helper;
using Android.Views;
using FastScrollRecycler;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Service;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;
using System.Collections.Generic;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public abstract class AbstractDocumentsListFragment : BaseFragment, IMenuItemOnActionExpandListener, SearchView.IOnQueryTextListener
    {
        public Folder Folder { get; set; }

        protected DocumentsListAdapter CurrentAdapter => (DocumentsListAdapter)recyclerView.GetAdapter();

        readonly Handler searchHandler = new Handler();

        protected const string FolderBundleKey = "Folder_5ab3effc-9a60-4b26-805e-72a0c3527b0d";
        protected const string HideSearchBundleKey = "HideSearchBundle_4ec1a10c-f9e5-43f8-8e73-c555f7679b43";
        protected const string OnlyShowExternalDocumentsBundleKey = "OnlyShowExternalDocuments_119623bc-74c6-4763-898a-319ea8fc9591";
        const string FirstRowIdKey = "FirstRowId_ab73aa33-930f-4139-94b1-b7828d5f4de7";
        const string LastRowIdKey = "LastRowId_a92f8e84-7274-48e3-9296-3d52a9b3231c";

        protected bool refreshing;

        protected IMenu menu;
        protected CoordinatorLayout coordinatorLayout;
        protected SwipeRefreshLayout refreshLayout;
        protected RecyclerView recyclerView;
        protected DocumentsListAdapter adapter;
        protected DocumentsListAdapter searchAdapter;
        protected ActionMode actionMode;

        protected int savedFirstRowId = -1;
        protected int savedLastRowId = -1;

        protected bool shouldNotifyAdapter;
        protected bool shouldNotifySearchAdapter;
        protected bool hideSearch;
        protected bool onlyShowExternalDocuments;

        SearchView searchView;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Arguments.ContainsKey(FolderBundleKey))
                Folder = Serializer.Deserialize<Folder>(Arguments.GetString(FolderBundleKey));

            if (savedInstanceState?.ContainsKey(FirstRowIdKey) == true)
                savedFirstRowId = savedInstanceState.GetInt(FirstRowIdKey);

            if (savedInstanceState?.ContainsKey(LastRowIdKey) == true)
                savedLastRowId = savedInstanceState.GetInt(LastRowIdKey);

            if (Arguments.ContainsKey(HideSearchBundleKey))
                hideSearch = Arguments.GetBoolean(HideSearchBundleKey);

            if (Arguments.ContainsKey(OnlyShowExternalDocumentsBundleKey))
                onlyShowExternalDocuments = Arguments.GetBoolean(OnlyShowExternalDocumentsBundleKey);
        }

        #region Fragment overrides

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            coordinatorLayout = (CoordinatorLayout)container.Parent.Parent;

            var emptyView = rootView.FindViewById<AppCompatTextView>(Resource.Id.empty_view);
            emptyView.SetText(Resource.String.empty_folder);

            refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            refreshLayout.SetColorSchemeResources(Resource.Color.blue, Resource.Color.darkerblue);
            refreshLayout.Refresh += async (sender, e) =>
            {
                CommonConfig.UsageAnalytics.LogEvent(new PullToRefreshEvent(false, module: ModuleType.Documents));

                actionMode?.Finish();
                actionMode = null;

                await RefreshData(forceClear: true);
            };

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));

            adapter = new DocumentsListAdapter(Activity, recyclerView, async (startId) => await RefreshData(startId));
            adapter.ItemClicked += Adapter_ItemClicked;
            adapter.RegisterAdapterDataObserver(new LambdaEmptyAdapterObserver(() =>
            {
                if (recyclerView.GetAdapter() != adapter)
                    return;

                emptyView.Visibility = adapter.ItemCount < 1 ? ViewStates.Visible : ViewStates.Gone;
                recyclerView.Visibility = adapter.ItemCount > 0 ? ViewStates.Visible : ViewStates.Gone;
                menu?.FindItem(Resource.Id.action_filter)?.SetEnabled(adapter.ItemCount > 0);
            }));
            recyclerView.SetAdapter(adapter);

            searchAdapter = new DocumentsListAdapter(Activity);
            searchAdapter.ItemClicked += Adapter_ItemClicked;

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.documents);
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = Folder?.Name;
        }

        public override async void OnResume()
        {
            base.OnResume();

            if (adapter.ItemCount < 1)
            {
                CommonConfig.Logger.Info($"No elements - will refresh...");
                await RefreshData();
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

            if (!IsAdded || IsDetached || IsRemoving)
                return;
        }

        public override void OnPause()
        {
            base.OnPause();
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            if (adapter?.Items?.Any() == true)
            {
                outState.PutInt(FirstRowIdKey, adapter.Items.First().Id + 1); //To comply with the DB query we use
                outState.PutInt(LastRowIdKey, adapter.Items.Last().Id - 1);
            }
            else
            {
                outState.PutInt(FirstRowIdKey, savedFirstRowId);
                outState.PutInt(LastRowIdKey, savedLastRowId);
            }
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            this.menu = menu;

            inflater.Inflate(Resource.Menu.menu_main, menu);

            var filterItem = menu.FindItem(Resource.Id.action_filter);
            filterItem.SetOnActionExpandListener(this);
            searchView = (SearchView)filterItem.ActionView;
            searchView.QueryHint = GetString(Resource.String.filter);
            searchView.SetOnQueryTextListener(this);

            if (!hideSearch)
            {
                var searchItem = menu.Add(Menu.None, 10, Menu.None, Resource.String.search);
                searchItem.SetIcon(Resource.Drawable.action_search_server);
                searchItem.SetShowAsAction(ShowAsAction.Always);
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == 10)
            {
                StartActivity(SearchActivity.CreateIntent(Context, ModuleType.Documents));
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Refreshing

        virtual protected async Task RefreshData(int startId = -1, int endId = -1, bool forceClear = false)
        {
            try
            {
                CommonConfig.Logger.Info($"Attempting refresh [startId={startId}, endId={endId}, force={forceClear}]...");

                if (refreshing)
                    return;

                refreshing = true;
                refreshLayout.Refreshing = true;

                CommonConfig.Logger.Info($"Refresh running...");

                List<DocumentPreview> documentPreviews;

                if (Restored && savedLastRowId != -1 && savedFirstRowId != -1)
                {
                    documentPreviews = await Managers.DocumentsManager.GetDocumentPreviewsAsync(Folder, savedFirstRowId, savedLastRowId, SourceType.Local);
                    savedLastRowId = savedFirstRowId = -1;
                }
                else
                {
                    documentPreviews = await Managers.DocumentsManager.GetDocumentPreviewsAsync(Folder, startId, endId);
                }

                if (onlyShowExternalDocuments)
                    documentPreviews = documentPreviews.FindAll((DocumentPreview dp) => dp.Direction == DocumentDirection.External);

                adapter.EnableLoadMore = documentPreviews.Count >= PlatformConfig.Preferences.DocumentsToDownload;
                CommonConfig.Logger.Info($"Enable load more documents set to {adapter.EnableLoadMore}");

                if (forceClear)
                    adapter.Clear();

                adapter.AppendItems(documentPreviews);

                Services.DocumentsDownloadService.Notify();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Downloading documents failed [folder.name={Folder?.Name}, folder.id={Folder?.Id}, startId={startId}, endId={endId}, force={forceClear}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);

                Activity?.OnBackPressed();
            }
            finally
            {
                refreshLayout.Refreshing = false;
                refreshing = false;

                CommonConfig.Logger.Info($"Refresh finished");
            }
        }

        #endregion

        #region Adapter callbacks

        virtual protected void Adapter_ItemClicked(object sender, DocumentPreview documentPreview)
        {
            if (actionMode == null)
            {
                if (documentPreview.Direction == DocumentDirection.Draft)
                {
                    if (!ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines.Any())
                    {
                        Dialogs.ShowConfirmDialog(Activity, Resource.String.no_lines_error_title, Resource.String.no_lines_error_content);
                        return;
                    }

                    StartActivity(ComposeDocumentActivity.CreateIntent(Context,
                                                                       DocumentCreationModeFlag.Edit,
                                                                       CopyToNewOption.None,
                                                                       previousDocumentDirection: documentPreview.Direction,
                                                                       previousDocumentFolderId: Folder.Id,
                                                                       previousDocumentId: documentPreview.Id));
                }
                else
                {
                    StartActivity(SwipeDocumentActivity.CreateIntent(Context, Folder, documentPreview));
                }
            }
            else
            {
                CurrentAdapter.SetSelected(documentPreview, !CurrentAdapter.IsSelected(documentPreview));

                if (CurrentAdapter.SelectedItemCount < 1)
                {
                    actionMode.Finish();
                }
                else
                {
                    actionMode.Title = CurrentAdapter.SelectedItemCount.ToString();
                    actionMode.Invalidate();
                }
            }
        }
        #endregion

        #region Filtering

        bool IMenuItemOnActionExpandListener.OnMenuItemActionExpand(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_filter)
            {
                CommonConfig.UsageAnalytics.LogEvent(new FilterEvent(false, module: ModuleType.Documents));

                menu?.FindItem(10)?.SetVisible(false);

                refreshLayout.Enabled = false;
                adapter.ClearSelections();
                recyclerView.SwapAdapter(searchAdapter, true);
                (this as SearchView.IOnQueryTextListener).OnQueryTextChange(string.Empty);
                return true;
            }

            return false;
        }

        bool IMenuItemOnActionExpandListener.OnMenuItemActionCollapse(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_filter)
            {
                menu?.FindItem(10)?.SetVisible(true);

                searchHandler.RemoveCallbacksAndMessages(null);
                searchAdapter.Clear();
                searchAdapter.ClearSelections();
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
                    searchAdapter.ReplaceItems(adapter.Items);
                else
                    searchAdapter.ReplaceItems(adapter.Items.Where(dp => MatchesQuery(dp, newText)).ToList());
            },
                500);
            return false;
        }

        bool SearchView.IOnQueryTextListener.OnQueryTextSubmit(string query)
        {
            return false;
        }

        static bool MatchesQuery(DocumentPreview dp, string query)
        {
#if DEBUG
            if (dp.Id.ToString() == query)
                return true;
#endif

            if (dp.ReferenceNumber?.ContainsCaseInsensitive(query) ?? false)
                return true;

            if (dp.Subject?.ContainsCaseInsensitive(query) ?? false)
                return true;

            if (dp.Preview?.ContainsCaseInsensitive(query) ?? false)
                return true;

            if (dp.Addresses.Any(da => da.Name?.ContainsCaseInsensitive(query) ?? false))
                return true;

            if (dp.Addresses.Any(da => da.Address?.ContainsCaseInsensitive(query) ?? false))
                return true;

            if (dp.Categories.Any(da => da.Name?.ContainsCaseInsensitive(query) ?? false))
                return true;

            if (dp.Creator?.ContainsCaseInsensitive(query) ?? false)
                return true;

            return false;
        }

        #endregion

        #region RecyclerView Adapter/ViewHolder

        protected class DocumentsListAdapter : RecyclerView.Adapter, ISectionedAdapter
        {
            public override int ItemCount => Items.Count;

            public List<DocumentPreview> Items { get; } = new List<DocumentPreview>(1000);

            public List<DocumentPreview> SelectedItems => selectedDocumentsInView.Values.ToList();

            public int SelectedItemCount => selectedDocumentsInView.Count;

            public bool EnableLoadMore { get; set; }

            public event EventHandler<DocumentPreview> ItemClicked = delegate { };
            public event EventHandler<DocumentPreview> ItemLongClicked = delegate { };

            readonly Dictionary<int, DocumentPreview> selectedDocumentsInView = new Dictionary<int, DocumentPreview>();
            readonly Context context;
            readonly RecyclerView recyclerView;
            readonly Action<int> loadMoreAction;

            bool unreadIndicatorMe = PlatformConfig.Preferences.UnreadIndicatorMe;
            bool compactList = PlatformConfig.Preferences.CompactDocumentsList;
            bool showCreatorOutgoing = PlatformConfig.Preferences.ShowCreatorOutgoing;

            int? swipedPosition;
            int swipedDirection;

            public DocumentsListAdapter(Context context)
            {
                this.context = context;
            }

            public DocumentsListAdapter(Context context, RecyclerView recyclerView, Action<int> loadMoreAction)
            {
                this.recyclerView = recyclerView;
                this.context = context;
                this.loadMoreAction = loadMoreAction;
            }

            public override int GetItemViewType(int position)
            {
                return Items[position].Direction == DocumentDirection.External ? ViewType.ExternalDocumentView : ViewType.DocumentView;
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var dp = Items[position];

                if (holder is DocumentPreviewViewHolder)
                {
                    var dpvh = holder as DocumentPreviewViewHolder;

                    dpvh.SwipedDirection = position == swipedPosition ? swipedDirection : 0;

                    dpvh.ItemView.SetOnClickListener(new ActionOnClickListener(() => ItemClicked(this, dp)));
                    dpvh.ItemView.SetOnLongClickListener(new ActionOnLongClickListener(() => ItemLongClicked(this, dp)));

                    if (dp.Direction == DocumentDirection.Incoming)
                    {
                        var address = dp.Addresses.FirstOrDefault(da => da.AddressType == DocumentAddressType.From);
                        dpvh.Recipent = address == null ? string.Empty : string.IsNullOrWhiteSpace(address.Name) ? address.Address : address.Name;
                    }
                    else
                    {
                        if (showCreatorOutgoing)
                        {
                            dpvh.Recipent = dp.Creator;
                        }
                        else
                        {
                            var address = dp.Addresses.Where(da => da.AddressType == DocumentAddressType.To || da.AddressType == DocumentAddressType.Cc || da.AddressType == DocumentAddressType.Bcc).OrderBy(da => da.AddressType).FirstOrDefault();
                            dpvh.Recipent = address == null ? string.Empty : string.IsNullOrWhiteSpace(address.Name) ? address.Address : address.Name;
                        }
                    }

                    dpvh.Subject = string.IsNullOrWhiteSpace(dp.Subject) ? context.GetString(Resource.String.no_subject) : dp.Subject;
                    var d = dp.DateReceivedTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTime().ConvertDateTimeToTimestampMilliseconds();
                    dpvh.Date = PlatformConfig.Preferences.ShowTimeOlderEmails ? d.FormatUserTimestampAsCompactMediumDateTimeString(context) : d.FormatUserTimestampAsCompactShortDateTimeString(context);
                    dpvh.BubbleDate = d.FormatUserTimestampAsCompactLongDateTimeString(context);
                    dpvh.Preview = string.IsNullOrWhiteSpace(dp.Preview) ? context.GetString(Resource.String.no_content) : Regex.Replace(dp.Preview, @"^\s+$[\r\n]*", "", RegexOptions.Multiline);
                    dpvh.Categories = dp.Categories;
                    dpvh.IncomingIndicator = dp.Direction == DocumentDirection.Incoming;
                    dpvh.OutgoingIndicator = dp.Direction == DocumentDirection.Outgoing;
                    dpvh.DraftIndicator = dp.Direction == DocumentDirection.Draft;
                    dpvh.UnreadIndicator = unreadIndicatorMe ? !dp.IsReadByCurrent : !dp.IsReadByAnyone;
                    dpvh.AttachmentIndicator = dp.AttachmentsCount > 0;
                    dpvh.CommentIndicator = dp.CommentsCount > 0;

                    if (compactList)
                    {
                        dpvh.Preview = null;
                        dpvh.AttachmentIndicator = false;
                        dpvh.CommentIndicator = false;
                    }

                    dpvh.Selected = selectedDocumentsInView.ContainsKey(dp.Id);
                }
                else if (holder is ExternalDocumentPreviewViewHolder)
                {
                    var edpvh = holder as ExternalDocumentPreviewViewHolder;

                    edpvh.SwipedDirection = position == swipedPosition ? swipedDirection : 0;

                    edpvh.ItemView.SetOnClickListener(new ActionOnClickListener(() => ItemClicked(this, dp)));
                    edpvh.ItemView.SetOnLongClickListener(new ActionOnLongClickListener(() => ItemLongClicked(this, dp)));

                    var d = dp.DateReceivedTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTime().ConvertDateTimeToTimestampMilliseconds();
                    edpvh.Date = PlatformConfig.Preferences.ShowTimeOlderEmails ? d.FormatUserTimestampAsCompactMediumDateTimeString(context) : d.FormatUserTimestampAsCompactShortDateTimeString(context);
                    edpvh.BubbleDate = d.FormatUserTimestampAsCompactLongDateTimeString(context);
                    edpvh.Name = string.IsNullOrWhiteSpace(dp.Subject) ? context.GetString(Resource.String.no_subject) : dp.Subject;
                    edpvh.Preview = string.IsNullOrWhiteSpace(dp.Preview) ? context.GetString(Resource.String.no_content) : Regex.Replace(dp.Preview, @"^\s+$[\r\n]*", "", RegexOptions.Multiline);
                    edpvh.Categories = dp.Categories;
                    edpvh.CommentIndicator = dp.CommentsCount > 0;

                    edpvh.Selected = selectedDocumentsInView.ContainsKey(dp.Id);
                }

                if (recyclerView != null && loadMoreAction != null && position == ItemCount - 1 && EnableLoadMore)
                {
                    CommonConfig.UsageAnalytics.LogEvent(new GetMoreDocumentsEvent());

                    loadMoreAction(dp.Id);
                }
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                if (viewType == ViewType.DocumentView)
                {
                    var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_documents, parent, false);
                    return new DocumentPreviewViewHolder(itemView);
                }

                if (viewType == ViewType.ExternalDocumentView)
                {
                    var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_documents_external, parent, false);
                    return new ExternalDocumentPreviewViewHolder(itemView);
                }

                return null;
            }

            public void PrependItems(List<DocumentPreview> items)
            {
                var count = items.Count;
                Items.InsertRange(0, items);
                NotifyItemRangeInserted(0, count);
            }

            public void AppendItems(List<DocumentPreview> items)
            {
                var count = Items.Count;
                Items.AddRange(items);
                NotifyItemRangeInserted(count, items.Count);
            }

            public void ReplaceItems(List<DocumentPreview> items)
            {
                Clear();
                AppendItems(items);
            }

            public void RefreshItems(List<DocumentPreview> items)
            {
                foreach (var item in items)
                {
                    var position = GetPosition(item);
                    if (position >= 0)
                        NotifyItemChanged(position);
                }
            }

            public void RemoveItems(List<DocumentPreview> items)
            {
                foreach (var item in items)
                {
                    var position = GetPosition(item);
                    if (position >= 0)
                    {
                        Items.RemoveAt(position);
                        NotifyItemRemoved(position);
                    }
                }
            }

            public void RemoveItemAtPosition(int position)
            {
                Items.RemoveAt(position);
                NotifyItemRemoved(position);
            }

            public bool IsSelected(DocumentPreview documentPreview)
            {
                return selectedDocumentsInView.ContainsKey(documentPreview.Id);
            }

            public void SetSelected(List<DocumentPreview> documentPreviews, bool selected)
            {
                foreach (var document in documentPreviews)
                    SetSelected(document, selected);
            }

            public void SetSelected(DocumentPreview documentPreview, bool selected)
            {
                var position = GetPosition(documentPreview);
                if (position < 0)
                    return;

                if (selected)
                    selectedDocumentsInView[documentPreview.Id] = documentPreview;
                else
                    selectedDocumentsInView.Remove(documentPreview.Id);
                NotifyItemChanged(position);
            }

            public void ClearSelections()
            {
                var documents = selectedDocumentsInView.Values.ToArray();
                selectedDocumentsInView.Clear();

                foreach (var document in documents)
                {
                    var position = GetPosition(document);
                    if (position >= 0)
                        NotifyItemChanged(position);
                }
            }

            public void Clear()
            {
                var size = Items.Count;
                Items.Clear();
                selectedDocumentsInView.Clear();
                NotifyItemRangeRemoved(0, size);
            }

            public int GetPosition(int documentPreviewId)
            {
                var position = -1;
                for (var i = 0; i < Items.Count; i++)
                    if (Items[i].Id == documentPreviewId)
                    {
                        position = i;
                        break;
                    }

                return position;
            }

            public int GetPosition(DocumentPreview documentPreview)
            {
                return GetPosition(documentPreview.Id);
            }

            public void SetSwipedState(int position, int direction)
            {
                swipedPosition = position;
                swipedDirection = direction;
            }

            public void ResetSwipedState()
            {
                swipedPosition = null;
            }

            string ISectionedAdapter.GetSectionName(int position)
            {
                var vh = recyclerView?.FindViewHolderForAdapterPosition(position);

                if (vh != null)
                {
                    var dpvh = vh as DocumentPreviewViewHolder;
                    if (dpvh != null)
                        return dpvh.BubbleDate;

                    var edpvh = vh as ExternalDocumentPreviewViewHolder;
                    if (edpvh != null)
                        return edpvh.BubbleDate;
                }

                return string.Empty;
            }

            public static class ViewType
            {
                public const int DocumentView = 0;
                public const int ExternalDocumentView = 1;
            }
        }

        class DocumentPreviewViewHolder : RecyclerView.ViewHolder
        {
            public string Recipent { set => recipentTextView.Text = value; }

            public string Date { set => dateTextView.Text = value; }

            public string BubbleDate { get; set; }

            public string Subject { set => subjectTextView.Text = value; }

            public string Preview
            {
                set
                {
                    if (value == null)
                    {
                        previewTextView.Text = string.Empty;
                        previewTextView.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        previewTextView.Text = value;
                        previewTextView.Visibility = ViewStates.Visible;
                    }
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

            public bool IncomingIndicator { set => incomingImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }

            public bool OutgoingIndicator { set => outgoingImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }

            public bool DraftIndicator { set => draftImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }

            public bool UnreadIndicator { set => unreadImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }

            public bool AttachmentIndicator { set => attachmentImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }

            public bool CommentIndicator { set => commentImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }

            public bool Compact
            {
                set
                {
                    attachmentImageView.Visibility = value ? ViewStates.Gone : attachmentImageView.Visibility;
                    commentImageView.Visibility = value ? ViewStates.Gone : commentImageView.Visibility;
                    previewTextView.Visibility = value ? ViewStates.Gone : previewTextView.Visibility;
                }
            }

            public bool Selected { set => selectedOverlay.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }

            public int SwipedDirection
            {
                set
                {
                    if (value != 0)
                    {
                        swipedBackground.Visibility = ViewStates.Visible;
                        itemContent.Visibility = ViewStates.Invisible; //Otherwisw the view collapses

                        var colorId = value == ItemTouchHelper.Left ? Resource.Color.darkerblue : Resource.Color.brown;

                        swipedBackground.SetBackgroundColor(new Color(ContextCompat.GetColor(ItemView.Context, colorId)));
                    }
                    else
                    {
                        swipedBackground.Visibility = ViewStates.Gone;
                        itemContent.Visibility = ViewStates.Visible;
                    }
                }
            }

            readonly AppCompatTextView recipentTextView;
            readonly AppCompatTextView dateTextView;
            readonly AppCompatTextView subjectTextView;
            readonly AppCompatTextView previewTextView;
            readonly LinearLayoutCompat categoriesLayout;
            readonly AppCompatImageView incomingImageView;
            readonly AppCompatImageView outgoingImageView;
            readonly AppCompatImageView draftImageView;
            readonly AppCompatImageView unreadImageView;
            readonly AppCompatImageView attachmentImageView;
            readonly AppCompatImageView commentImageView;
            readonly LinearLayoutCompat itemContent;
            readonly View selectedOverlay;
            readonly View swipedBackground;

            public DocumentPreviewViewHolder(View itemView)
                : base(itemView)
            {
                recipentTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_document_recipent);
                dateTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_document_date);
                subjectTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_document_subject);
                previewTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_document_preview);
                categoriesLayout = itemView.FindViewById<LinearLayoutCompat>(Resource.Id.list_item_document_categories);
                incomingImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_direction_incoming);
                outgoingImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_direction_outgoing);
                draftImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_direction_draft);
                unreadImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_unread);
                attachmentImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_attachment);
                commentImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_comment);
                itemContent = itemView.FindViewById<LinearLayoutCompat>(Resource.Id.list_item_document_internal_layout);
                selectedOverlay = itemView.FindViewById<View>(Resource.Id.selected_overlay);
                swipedBackground = itemView.FindViewById<View>(Resource.Id.swiped_background);
            }
        }

        class ExternalDocumentPreviewViewHolder : RecyclerView.ViewHolder
        {
            public string Name { set => nameTextView.Text = value; }

            public string Date { set => dateTextView.Text = value; }

            public string BubbleDate { get; set; }

            public string Preview { set => previewTextView.Text = value; }

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

            public bool CommentIndicator { set => commentImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }

            public bool Selected { set => selectedOverlay.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }

            public int SwipedDirection
            {
                set
                {
                    if (value != 0)
                    {
                        swipedBackground.Visibility = ViewStates.Visible;
                        itemContent.Visibility = ViewStates.Invisible;

                        var colorId = value == ItemTouchHelper.Left ? Resource.Color.darkerblue : Resource.Color.brown;

                        swipedBackground.SetBackgroundColor(new Color(ContextCompat.GetColor(ItemView.Context, colorId)));
                    }
                    else
                    {
                        swipedBackground.Visibility = ViewStates.Gone;
                        itemContent.Visibility = ViewStates.Visible;
                    }
                }
            }

            readonly AppCompatTextView nameTextView;
            readonly AppCompatTextView dateTextView;
            readonly AppCompatTextView previewTextView;
            readonly LinearLayoutCompat categoriesLayout;
            readonly AppCompatImageView commentImageView;
            readonly LinearLayoutCompat itemContent;
            readonly View selectedOverlay;
            readonly View swipedBackground;

            public ExternalDocumentPreviewViewHolder(View itemView)
                : base(itemView)
            {
                nameTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_document_external_name);
                dateTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_document_external_date);
                previewTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_document_external_preview);
                categoriesLayout = itemView.FindViewById<LinearLayoutCompat>(Resource.Id.list_item_document_external_categories);
                commentImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_external_comment);
                itemContent = itemView.FindViewById<LinearLayoutCompat>(Resource.Id.list_item_document_external_internal_layout);
                selectedOverlay = itemView.FindViewById<View>(Resource.Id.selected_overlay);
                swipedBackground = itemView.FindViewById<View>(Resource.Id.swiped_background);
            }
        }

        #endregion
    }
}