using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.HubMessages;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;
using TinyMessenger;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class DocumentsToUploadListFragment : RetainableStateFragment, ActionMode.ICallback
    {
        bool refreshing;

        RecyclerView recyclerView;
        DocumentsToUploadListAdapter adapter;
        ActionMode actionMode;

        TinyMessageSubscriptionToken documentUploadStatusChangedToken;

        public static (DocumentsToUploadListFragment fragment, string var) NewInstance()
        {
            var fragment = new DocumentsToUploadListFragment();
            var tag = $"{nameof(DocumentsToUploadListFragment)}]";

            return (fragment, tag);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(DocumentsToUploadListFragment)} ...");

            var rootView = inflater.Inflate(Resource.Layout.list_no_refresh, container, false);

            var emptyView = rootView.FindViewById<AppCompatTextView>(Resource.Id.empty_view);
            emptyView.SetText(Resource.String.empty_folder);

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));

            adapter = new DocumentsToUploadListAdapter(Activity);
            adapter.RegisterAdapterDataObserver(new LambdaEmptyAdapterObserver(() =>
            {
                if (recyclerView.GetAdapter() != adapter)
                    return;

                emptyView.Visibility = adapter.ItemCount < 1 ? ViewStates.Visible : ViewStates.Gone;
                recyclerView.Visibility = adapter.ItemCount > 0 ? ViewStates.Visible : ViewStates.Gone;
            }));
            adapter.ItemClicked += Adapter_ItemClicked;
            adapter.ItemLongClicked += Adapter_ItemLongClicked;
            recyclerView.SetAdapter(adapter);

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.documents);
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = GetString(Resource.String.outgoing);
        }

        public override async void OnResume()
        {
            base.OnResume();

            CommonConfig.Logger.Info($"Resuming {nameof(DocumentsToUploadListFragment)} ...");

            CommonConfig.Logger.Info($"Will refresh...");
            await RefreshData();

            if (!IsAdded || IsDetached || IsRemoving)
                return;

            documentUploadStatusChangedToken = CommonConfig.MessengerHub.Subscribe<DocumentUploadStatusChanged>(m =>
            {
                Activity.RunOnUiThread(async () => { await RefreshData(); });
            });
        }

        public override void OnPause()
        {
            base.OnPause();

            documentUploadStatusChangedToken?.Dispose();

            CommonConfig.Logger.Info($"Resuming {nameof(DocumentsToUploadListFragment)} ...");
        }

        protected async Task RefreshData()
        {
            try
            {
                CommonConfig.Logger.Info($"Attempting refresh of outgoing folder...");

                if (refreshing)
                    return;

                refreshing = true;

                CommonConfig.Logger.Info($"Refresh running...");

                var pendingDocs = await Managers.DocumentsManager.GetDocumentsToUploadDocumentPreviews();
                var failedDocs = await Managers.DocumentsManager.GetFailedDocumentsToUploadDocumentPreviews();
                adapter.ReplaceItems(pendingDocs, failedDocs);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while retrieving local documents", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);

                Activity?.OnBackPressed();
            }
            finally
            {
                refreshing = false;

                CommonConfig.Logger.Info($"Refresh finished");
            }
        }

        void Adapter_ItemClicked(object sender, (Guid Guid, DocumentPreview DocumentPreview) data)
        {
            if (actionMode == null &&
                adapter.FailedGuids.Contains(data.Guid))
            {
                var i = new Intent(Activity, typeof(DocumentActivity));
                i.PutExtra(DocumentActivity.FailedDocumentToUploadGuidIntentKey, data.Guid.ToString());
                StartActivity(i);
            }

            if (actionMode != null &&
                adapter.FailedGuids.Contains(data.Guid))
            {
                adapter.SetSelected(data, !adapter.IsSelected(data));

                if (adapter.SelectedItemCount < 1)
                {
                    actionMode.Finish();
                }
                else
                {
                    actionMode.Title = adapter.SelectedItemCount.ToString();
                    actionMode.Invalidate();
                }
            }
        }

        void Adapter_ItemLongClicked(object sender, (Guid Guid, DocumentPreview DocumentPreview) data)
        {
            if (adapter.PendingGuids.Contains(data.Guid))
                return;

            if (actionMode == null)
                actionMode = Activity.StartActionMode(this);

            Adapter_ItemClicked(sender, data);
        }

        #region Action mode

        bool ActionMode.ICallback.OnCreateActionMode(ActionMode mode, IMenu menu)
        {
            return true;
        }

        bool ActionMode.ICallback.OnPrepareActionMode(ActionMode mode, IMenu menu)
        {
            Activity.Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
            Activity.Window.SetStatusBarColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));

            menu.Clear();

            if (adapter.SelectedItems.All(d => adapter.FailedGuids.Contains(d.Guid)))
            {
                menu.Add(Menu.None, 10, 10, Resource.String.resend);
                menu.Add(Menu.None, 20, 20, Resource.String.delete);
            }

            return false;
        }

        bool ActionMode.ICallback.OnActionItemClicked(ActionMode mode, IMenuItem item)
        {
            var selectedItems = adapter.SelectedItems;

            if (item.ItemId == 10)
            {
                Task.Run(async () =>
                {
                    foreach (var selectedItem in selectedItems)
                    {
                        await Managers.DocumentsManager.RequeueFailedToUpload(selectedItem.Guid);
                    }
                }).ContinueWith(async t =>
                {
                    actionMode?.Finish();
                    await RefreshData();
                }, TaskScheduler.FromCurrentSynchronizationContext());
                return true;
            }

            if (item.ItemId == 20)
            {
                Task.Run(async () =>
                {
                    foreach (var selectedItem in selectedItems)
                    {
                        await Managers.DocumentsManager.DeleteFailedDocumentToUpload(selectedItem.Guid);
                    }
                }).ContinueWith(async t =>
                {
                    actionMode?.Finish();
                    await RefreshData();
                }, TaskScheduler.FromCurrentSynchronizationContext());
                return true;
            }

            return OnOptionsItemSelected(item);
        }

        void ActionMode.ICallback.OnDestroyActionMode(ActionMode mode)
        {
            Activity.Window.AddFlags(WindowManagerFlags.TranslucentStatus);
            Activity.Window.SetStatusBarColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));

            adapter.ClearSelections();
            actionMode = null;
        }

        #endregion

        protected class DocumentsToUploadListAdapter : RecyclerView.Adapter
        {
            public override int ItemCount => itemsInView.Count;
            public int SelectedItemCount => selectedItemsInView.Count;

            public List<(Guid Guid, DocumentPreview DocumentPreview)> Items => itemsInView.ToList();
            public List<(Guid Guid, DocumentPreview DocumentPreview)> SelectedItems => selectedItemsInView.ToList();
            public HashSet<Guid> PendingGuids => pendingGuids.ToHashSet();
            public HashSet<Guid> FailedGuids => failedGuids.ToHashSet();

            List<(Guid Guid, DocumentPreview DocumentPreview)> itemsInView = new List<(Guid, DocumentPreview)>(25);
            List<(Guid Guid, DocumentPreview DocumentPreview)> selectedItemsInView = new List<(Guid, DocumentPreview)>(25);
            HashSet<Guid> pendingGuids = new HashSet<Guid>();
            HashSet<Guid> failedGuids = new HashSet<Guid>();

            readonly Context context;

            public event EventHandler<(Guid Guid, DocumentPreview DocumentPreview)> ItemClicked = delegate { };
            public event EventHandler<(Guid Guid, DocumentPreview DocumentPreview)> ItemLongClicked = delegate { };

            public DocumentsToUploadListAdapter(Context context)
            {
                this.context = context;
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var dtuvh = holder as DocumentToUploadViewHolder;
                var data = itemsInView[position];
                var dp = data.DocumentPreview;

                dtuvh.ItemView.SetOnClickListener(new ActionOnClickListener(() => ItemClicked(this, data)));
                dtuvh.ItemView.SetOnLongClickListener(new ActionOnLongClickListener(() => ItemLongClicked(this, data)));

                var address = dp.Addresses.Where(da => da.AddressType == DocumentAddressType.To || da.AddressType == DocumentAddressType.Cc || da.AddressType == DocumentAddressType.Bcc).OrderBy(da => da.AddressType).FirstOrDefault();
                dtuvh.Recipient = address == null ? string.Empty : string.IsNullOrWhiteSpace(address.Name) ? address.Address : address.Name;

                dtuvh.Subject = string.IsNullOrWhiteSpace(dp.Subject) ? context.GetString(Resource.String.no_subject) : dp.Subject;
                dtuvh.WaitingIndicator = pendingGuids.Contains(data.Guid);
                dtuvh.FailedIndicator = failedGuids.Contains(data.Guid);

                dtuvh.Selected = selectedItemsInView.Any(d => d.Guid == data.Guid);
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_documents_to_upload, parent, false);
                return new DocumentToUploadViewHolder(itemView);
            }

            public void AppendItems(List<(Guid Guid, DocumentPreview DocumentPreview)> pending, List<(Guid Guid, DocumentPreview DocumentPreview)> failed)
            {
                var count = itemsInView.Count;
                itemsInView.AddRange(pending);
                itemsInView.AddRange(failed);
                pendingGuids.UnionWith(pending.Select(d => d.Guid));
                failedGuids.UnionWith(failed.Select(d => d.Guid));
                NotifyItemRangeInserted(count, pending.Count + failed.Count);
            }

            public void Clear()
            {
                var size = itemsInView.Count;
                itemsInView.Clear();
                selectedItemsInView.Clear();
                pendingGuids.Clear();
                failedGuids.Clear();
                NotifyItemRangeRemoved(0, size);
            }

            public void ReplaceItems(List<(Guid Guid, DocumentPreview DocumentPreview)> pending, List<(Guid Guid, DocumentPreview DocumentPreview)> failed)
            {
                Clear();
                AppendItems(pending, failed);
            }

            public void SetSelected(List<(Guid Guid, DocumentPreview DocumentPreview)> documentsToUpload, bool selected)
            {
                foreach (var documentToUpload in documentsToUpload)
                    SetSelected(documentToUpload, selected);
            }

            public void SetSelected((Guid Guid, DocumentPreview DocumentPreview) data, bool selected)
            {
                var position = GetPosition(data);
                if (position < 0)
                    return;

                if (selected)
                    selectedItemsInView.Add(data);
                else
                    selectedItemsInView.Remove(data);
                NotifyItemChanged(position);
            }

            public void ClearSelections(bool notify = true)
            {
                var selectedItems = selectedItemsInView.ToArray();
                selectedItemsInView.Clear();
                if (notify)
                    foreach (var selectedItem in selectedItems)
                        NotifyItemChanged(GetPosition(selectedItem));
            }

            public bool IsSelected(Guid guid) => selectedItemsInView.Any(d => d.Guid == guid);
            public bool IsSelected((Guid Guid, DocumentPreview DocumentPreview) data) => IsSelected(data.Guid);

            public int GetPosition(Guid guid) => itemsInView.FindIndex(d => d.Guid == guid);
            public int GetPosition((Guid Guid, DocumentPreview DocumentPreview) data) => GetPosition(data.Guid);
        }

        class DocumentToUploadViewHolder : RecyclerView.ViewHolder
        {
            public string Recipient { set => recipentTextView.Text = value; }
            public string Subject { set => subjectTextView.Text = value; }
            public bool WaitingIndicator { set => waitingImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }
            public bool FailedIndicator { set => failedImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }
            public bool Selected { set => selectedOverlay.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }

            readonly AppCompatTextView recipentTextView;
            readonly AppCompatTextView subjectTextView;
            readonly AppCompatImageView waitingImageView;
            readonly AppCompatImageView failedImageView;
            readonly View selectedOverlay;

            public DocumentToUploadViewHolder(View itemView)
                : base(itemView)
            {
                recipentTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_document_to_upload_recipent);
                subjectTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_document_to_upload_subject);
                waitingImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_to_upload_waiting);
                failedImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_to_upload_error);
                selectedOverlay = itemView.FindViewById<View>(Resource.Id.selected_overlay);
            }
        }
    }
}