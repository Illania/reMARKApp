//
// Project: Mark5.Mobile.Droid
// File: OutgoingDocumentListFragment.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;


namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class OutgoingDocumentsListFragment : RetainableStateFragment, ActionMode.ICallback
    {
        bool refreshing;

        public Action CloseRequest { get; set; }

        SwipeRefreshLayout refreshLayout;
        RecyclerView recyclerView;
        OutgoingDocumentsListAdapter adapter;
        ActionMode actionMode;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(OutgoingDocumentsListFragment)} ...");

            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            refreshLayout.SetColorSchemeResources(Resource.Color.lightbrown, Resource.Color.brown);
            refreshLayout.Refresh += async (sender, e) =>
            {
                actionMode?.Finish();
                actionMode = null;

                await RefreshData();
            };

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));

            adapter = new OutgoingDocumentsListAdapter(Activity);
            adapter.ItemClicked += Adapter_ItemClicked;
            adapter.ItemLongClicked += Adapter_ItemLongClicked;
            recyclerView.SetAdapter(adapter);

            HasOptionsMenu = true;

            return rootView;
        }

        public override async void OnResume()
        {
            base.OnResume();

            CommonConfig.Logger.Info($"Resuming {nameof(OutgoingDocumentsListFragment)} ...");

            CommonConfig.Logger.Info($"Will refresh...");
            await RefreshData();

            if (!IsAdded || IsDetached || IsRemoving) return;

            Managers.OutgoingDocumentsManager.DocumentBeingSent += OutgoingDocumentsManager_DocumentBeingSent;
            Managers.OutgoingDocumentsManager.DocumentSendingFailed += OutgoingDocumentsManager_DocumentSendingFailed;
            Managers.OutgoingDocumentsManager.DocumentSendingSuccessful += OutgoingDocumentsManager_DocumentSendingSuccessful;
        }

        public override void OnPause()
        {
            base.OnPause();

            CommonConfig.Logger.Info($"Resuming {nameof(OutgoingDocumentsListFragment)} ...");

            Managers.OutgoingDocumentsManager.DocumentBeingSent -= OutgoingDocumentsManager_DocumentBeingSent;
            Managers.OutgoingDocumentsManager.DocumentSendingFailed -= OutgoingDocumentsManager_DocumentSendingFailed;
            Managers.OutgoingDocumentsManager.DocumentSendingSuccessful -= OutgoingDocumentsManager_DocumentSendingSuccessful;
        }

        protected async Task RefreshData()
        {
            try
            {
                CommonConfig.Logger.Info($"Attempting refresh of outgoing folder...");

                if (refreshing) return;

                refreshing = true;
                refreshLayout.Post(() => refreshLayout.Refreshing = true); //Bug: fixed in support library v 24.2.0 (issue 77712)

                CommonConfig.Logger.Info($"Refresh running...");

                var outgoingDocumentPreviews = await Managers.DocumentsManager.GetOutgoingDocumentPreviewsAsync(); //TODO need to sort them by date
                adapter.ReplaceItems(outgoingDocumentPreviews);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while retrieving local documents", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);

                if (CloseRequest != null) CloseRequest();
            }
            finally
            {
                refreshLayout.Post(() => refreshLayout.Refreshing = false); //Bug: fixed in support library v 24.2.0 (issue 77712)
                refreshing = false;

                CommonConfig.Logger.Info($"Refresh finished");
            }
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            var newItem = menu.Add(Menu.None, 10, 10, "New"); //TODO an icon should be here
            newItem.SetShowAsAction(ShowAsAction.Always);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == 10)
            {
                StartActivity(ComposeDocumentActivity.CreateIntent(Context, DocumentCreationModeFlag.New, DocumentDirection.None));
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        void Adapter_ItemClicked(object sender, OutgoingDocumentPreview e)
        {
            if (actionMode != null)
            {
                adapter.SetSelected(e, !adapter.IsSelected(e));

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
            else
            {
                StartActivity(ComposeDocumentActivity.CreateIntent(Context, DocumentCreationModeFlag.Edit, e.Direction, outgoingDocumentGuid: e.Identifier));
            }
        }

        void Adapter_ItemLongClicked(object sender, OutgoingDocumentPreview e)
        {
            if (actionMode == null)
            {
                actionMode = Activity.StartActionMode(this);
            }

            Adapter_ItemClicked(sender, e);
        }

        #region Action mode

        bool ActionMode.ICallback.OnPrepareActionMode(ActionMode mode, IMenu menu)
        {
            return false;
        }

        bool ActionMode.ICallback.OnCreateActionMode(ActionMode mode, IMenu menu)
        {
            menu.Add(Menu.None, 10, 10, Resource.String.delete);
            return true;
        }

        bool ActionMode.ICallback.OnActionItemClicked(ActionMode mode, IMenuItem item)
        {
            var selectedOutgoingDocuments = adapter.SelectedItems;

            if (item.ItemId == 10)
            {

                DeleteOutgoingDocuments(selectedOutgoingDocuments);
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        void ActionMode.ICallback.OnDestroyActionMode(ActionMode mode)
        {
            adapter.ClearSelections();
            actionMode = null;
        }

        void DeleteOutgoingDocuments(List<OutgoingDocumentPreview> selectedOutgoingDocuments)
        {
            Task.Run(async () =>
           {
               foreach (var selectedOutgoingDocument in selectedOutgoingDocuments)
               {
                   await Managers.DocumentsManager.DeleteOutgoingDocumentFolder(selectedOutgoingDocument.Identifier);
               }
           }).ContinueWith(async t =>
          {
              await RefreshData();
              actionMode.Finish();
          }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        #endregion

        #region OutgoingDocumentManager event handlers

        void OutgoingDocumentsManager_DocumentBeingSent(object sender, OutgoingDocumentContainer outgoingDocumentContainer)
        {
            var position = adapter.GetPosition(outgoingDocumentContainer.Info.Identifier);
            if (position >= 0)
            {
                var dp = adapter.Items[position];
                dp.State = OutgoingDocumentState.Sending;
                Activity.RunOnUiThread(() => adapter.NotifyItemChanged(position));
            }
        }

        void OutgoingDocumentsManager_DocumentSendingFailed(object sender, OutgoingDocumentContainer outgoingDocumentContainer)
        {
            var position = adapter.GetPosition(outgoingDocumentContainer.Info.Identifier);
            if (position >= 0)
            {
                var dp = adapter.Items[position];
                dp.State = OutgoingDocumentState.Failed;
                Activity.RunOnUiThread(() => adapter.NotifyItemChanged(position));
            }
        }

        void OutgoingDocumentsManager_DocumentSendingSuccessful(object sender, OutgoingDocumentContainer outgoingDocumentContainer)
        {
            var position = adapter.GetPosition(outgoingDocumentContainer.Info.Identifier);
            if (position >= 0)
            {
                Activity.RunOnUiThread(() =>
                {
                    adapter.RemoveItemsAtIndex(position);
                    adapter.NotifyItemRemoved(position);
                });
            }
        }

        #endregion

        #region RetainableStateFragment overrides

        public override IRetainableState OnRetainInstanceState()
        {
            CommonConfig.Logger.Info($"Retaining state [Count={adapter?.ItemCount}/{adapter?.SelectedItemCount}]...");

            return new OutgoingDocumentsListFragmentState
            {
                OutgoingDocumentPreviews = adapter.Items,
                OutgoingSelectedDocumentPreviews = adapter.SelectedItems
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var dlfs = restoredState as OutgoingDocumentsListFragmentState;
            if (dlfs != null)
            {
                CommonConfig.Logger.Info($"Restoring state [dlfs.folder.id={dlfs.Folder?.Id}, dlfs.items.count={dlfs.OutgoingDocumentPreviews?.Count}, dlfs.selectedItems.count={dlfs.OutgoingSelectedDocumentPreviews?.Count}]...");

                adapter.AppendItems(dlfs.OutgoingDocumentPreviews);

                if (dlfs.OutgoingSelectedDocumentPreviews.Count > 0)
                {
                    actionMode?.Finish();
                    actionMode = Activity.StartActionMode((this));

                    adapter.SetSelected(dlfs.OutgoingSelectedDocumentPreviews, true);
                    actionMode.Title = adapter.SelectedItemCount.ToString();
                    actionMode.Invalidate();
                }
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(OutgoingDocumentsListFragment)}]";
        }

        class OutgoingDocumentsListFragmentState : IRetainableState
        {

            public Folder Folder { get; set; }

            public List<OutgoingDocumentPreview> OutgoingDocumentPreviews { get; set; }

            public List<OutgoingDocumentPreview> OutgoingSelectedDocumentPreviews { get; set; }
        }

        #endregion

        protected class OutgoingDocumentsListAdapter : RecyclerView.Adapter
        {

            public static class ViewType
            {
                public const int DocumentView = 0;
                public const int ExternalDocumentView = 1;
                public const int OutgoingDocumentView = 2;
            }

            public List<OutgoingDocumentPreview> Items
            {
                get
                {
                    return ougoingDocumentPreviewsInView.ToList();
                }
            }

            public List<OutgoingDocumentPreview> SelectedItems
            {
                get
                {
                    return selectedOutgoingDocumentsInView.Values.ToList();
                }
            }

            public override int ItemCount
            {
                get
                {
                    return ougoingDocumentPreviewsInView.Count;
                }
            }

            public int SelectedItemCount
            {
                get
                {
                    return selectedOutgoingDocumentsInView.Count;
                }
            }

            readonly List<OutgoingDocumentPreview> ougoingDocumentPreviewsInView = new List<OutgoingDocumentPreview>(1000); //TODO modify
            readonly Dictionary<Guid, OutgoingDocumentPreview> selectedOutgoingDocumentsInView = new Dictionary<Guid, OutgoingDocumentPreview>();
            readonly Context context;

            public event EventHandler<OutgoingDocumentPreview> ItemClicked = delegate { };
            public event EventHandler<OutgoingDocumentPreview> ItemLongClicked = delegate { };

            public OutgoingDocumentsListAdapter(Context context)
            {
                this.context = context;
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var odpvh = holder as OutgoingDocumentPreviewViewHolder;
                var dp = ougoingDocumentPreviewsInView[position];

                odpvh.ItemView.SetOnClickListener(new ActionOnClickListener(() => ItemClicked(this, dp)));
                odpvh.ItemView.SetOnLongClickListener(new ActionOnLongClickListener(() => ItemLongClicked(this, dp)));

                var address = dp.Addresses.Where(da => da.AddressType == DocumentAddressType.To || da.AddressType == DocumentAddressType.Cc || da.AddressType == DocumentAddressType.Bcc).OrderBy(da => da.AddressType).FirstOrDefault();
                odpvh.Recipient = address == null ? string.Empty : string.IsNullOrWhiteSpace(address.Name) ? address.Address : address.Name;

                odpvh.Subject = string.IsNullOrWhiteSpace(dp.Subject) ? context.GetString(Resource.String.no_subject) : dp.Subject;
                odpvh.Date = dp.DateReceivedTimestamp
                .ConvertTimestampMillisecondsToDateTime()
                .ConvertUtcToServerTime()
                .ConvertDateTimeToTimestampMilliseconds()
                .FormatServerTimestampAsCompactShortDateTimeString(context);
                odpvh.Preview = string.IsNullOrWhiteSpace(dp.Preview) ? context.GetString(Resource.String.no_content) : Regex.Replace(dp.Preview, @"^\s+$[\r\n]*", "", RegexOptions.Multiline);
                odpvh.AttachmentIndicator = dp.AttachmentsCount > 0;
                odpvh.WaitingIndicator = dp.State == OutgoingDocumentState.Waiting;
                odpvh.SendingIndicator = dp.State == OutgoingDocumentState.Sending;
                odpvh.FailedIndicator = dp.State == OutgoingDocumentState.Failed;

                odpvh.Selected = selectedOutgoingDocumentsInView.ContainsKey(dp.Identifier);
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_documents_outgoing, parent, false);
                return new OutgoingDocumentPreviewViewHolder(itemView);
            }

            public void AppendItems(List<OutgoingDocumentPreview> items)
            {
                var count = ougoingDocumentPreviewsInView.Count;
                ougoingDocumentPreviewsInView.AddRange(items);
                NotifyItemRangeInserted(count, items.Count);
            }

            public void RemoveItemsAtIndex(int index)
            {
                ougoingDocumentPreviewsInView.RemoveAt(index);
            }

            public void ReplaceItems(List<OutgoingDocumentPreview> items)
            {
                Clear();
                AppendItems(items.OrderBy(i => i.DateReceivedTimestamp).ToList());
            }

            public void Clear()
            {
                var size = ougoingDocumentPreviewsInView.Count;
                ougoingDocumentPreviewsInView.Clear();
                selectedOutgoingDocumentsInView.Clear();
                NotifyItemRangeRemoved(0, size);
            }

            public void SetSelected(List<OutgoingDocumentPreview> outgoingDocumentPrevivews, bool selected)
            {
                foreach (var outgoingDocumentPreview in outgoingDocumentPrevivews)
                {
                    SetSelected(outgoingDocumentPreview, selected);
                }
            }

            public void SetSelected(OutgoingDocumentPreview documentPreview, bool selected)
            {
                var position = GetPosition(documentPreview);
                if (position < 0) return;

                if (selected)
                {
                    selectedOutgoingDocumentsInView[documentPreview.Identifier] = documentPreview;
                }
                else
                {
                    selectedOutgoingDocumentsInView.Remove(documentPreview.Identifier);
                }
                NotifyItemChanged(position);
            }

            public void ClearSelections(bool notify = true)
            {
                var outgoingDocuments = selectedOutgoingDocumentsInView.Values.ToArray();
                selectedOutgoingDocumentsInView.Clear();
                if (notify)
                {
                    foreach (var document in outgoingDocuments)
                    {
                        NotifyItemChanged(GetPosition(document));
                    }
                }
            }

            public bool IsSelected(OutgoingDocumentPreview outgoingDocumentPreview)
            {
                return selectedOutgoingDocumentsInView.ContainsKey(outgoingDocumentPreview.Identifier);
            }

            public int GetPosition(Guid identifier)
            {
                var position = -1;
                for (var i = 0; i < ougoingDocumentPreviewsInView.Count; i++)
                {
                    if (ougoingDocumentPreviewsInView[i].Identifier == identifier)
                    {
                        position = i;
                        break;
                    }
                }
                return position;
            }

            public int GetPosition(OutgoingDocumentPreview outgoingDocumentPreview)
            {
                return GetPosition(outgoingDocumentPreview.Identifier);
            }
        }

        class OutgoingDocumentPreviewViewHolder : RecyclerView.ViewHolder
        {
            public string Recipient
            {
                set
                {
                    recipentTextView.Text = value;
                }
            }

            public string Subject
            {
                set
                {
                    subjectTextView.Text = value;
                }
            }

            public string Date
            {
                set
                {
                    dateTextView.Text = value;
                }
            }

            public string Preview
            {
                set
                {
                    previewTextView.Text = value;
                }
            }


            public bool AttachmentIndicator
            {
                set
                {
                    attachmentImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
                }
            }


            public bool WaitingIndicator
            {
                set
                {
                    waitingImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
                }
            }


            public bool FailedIndicator
            {
                set
                {
                    failedImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
                }
            }


            public bool SendingIndicator
            {
                set
                {
                    sendingImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
                }
            }


            public bool Selected
            {
                set
                {
                    selectedOverlay.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
                }
            }

            readonly AppCompatTextView recipentTextView;
            readonly AppCompatTextView dateTextView;
            readonly AppCompatTextView subjectTextView;
            readonly AppCompatTextView previewTextView;
            readonly AppCompatImageView waitingImageView;
            readonly AppCompatImageView failedImageView;
            readonly AppCompatImageView sendingImageView;
            readonly AppCompatImageView attachmentImageView;
            readonly View selectedOverlay;

            public OutgoingDocumentPreviewViewHolder(View itemView)
                    : base(itemView)
            {
                recipentTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_document_outgoing_recipent); //TODO need to correct the icons for the states
                subjectTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_document_outgoing_subject);
                dateTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_document_outgoing_date);
                previewTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_document_outgoing_preview);
                waitingImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_outgoing_waiting);
                failedImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_outgoing_error);
                sendingImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_outgoing_sending);
                attachmentImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_outgoing_attachment);
                selectedOverlay = itemView.FindViewById<View>(Resource.Id.selected_overlay);
            }
        }


    }
}
