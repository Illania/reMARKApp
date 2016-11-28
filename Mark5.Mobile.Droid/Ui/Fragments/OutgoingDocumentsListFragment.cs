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
    public class OutgoingDocumentsListFragment : RetainableStateFragment, Android.Views.ActionMode.ICallback
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

                await RefreshData(true);
            };

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));

            adapter = new OutgoingDocumentsListAdapter(Activity);
            adapter.ItemClicked += Adapter_ItemClicked; ;
            //adapter.ItemLongClicked += Adapter_ItemLongClicked; //TODO which actions do we need on long click?
            recyclerView.SetAdapter(adapter);

            HasOptionsMenu = true;

            return rootView;
        }

        protected async Task RefreshData(bool force = false)
        {
            try
            {
                CommonConfig.Logger.Info($"Attempting refresh of outgoing folder...");

                if (refreshing) return;

                refreshing = true;
                refreshLayout.Post(() => refreshLayout.Refreshing = true); //Bug: fixed in support library v 24.2.0 (issue 77712)

                CommonConfig.Logger.Info($"Refresh running...");

                if (force)
                {
                    adapter.Clear();
                }

                var outgoingDocumentPreviews = await Managers.DocumentsManager.GetOutgoingDocumentPreviewsAsync(); //TODO need to sort them by date
                adapter.AppendItems(outgoingDocumentPreviews);
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

        public override async void OnResume()
        {
            base.OnResume();

            CommonConfig.Logger.Info($"Resuming {nameof(OutgoingDocumentsListFragment)} ...");

            if (adapter.ItemCount < 1)
            {
                CommonConfig.Logger.Info($"No elements - will refresh...");
                await RefreshData();
            }

            if (!IsAdded || IsDetached || IsRemoving) return;

            Managers.OutgoingDocumentsManager.DocumentAddedToQueue += OutgoingDocumentsManager_DocumentAddedToQueue;
            Managers.OutgoingDocumentsManager.DocumentBeingSent += OutgoingDocumentsManager_DocumentBeingSent;
            Managers.OutgoingDocumentsManager.DocumentSendingFailed += OutgoingDocumentsManager_DocumentSendingFailed;
            Managers.OutgoingDocumentsManager.DocumentSendingSuccessful += OutgoingDocumentsManager_DocumentSendingSuccessful;
        }

        public override void OnPause()
        {
            base.OnPause();

            CommonConfig.Logger.Info($"Resuming {nameof(OutgoingDocumentsListFragment)} ...");

            Managers.OutgoingDocumentsManager.DocumentAddedToQueue -= OutgoingDocumentsManager_DocumentAddedToQueue;
            Managers.OutgoingDocumentsManager.DocumentBeingSent -= OutgoingDocumentsManager_DocumentBeingSent;
            Managers.OutgoingDocumentsManager.DocumentSendingFailed -= OutgoingDocumentsManager_DocumentSendingFailed;
            Managers.OutgoingDocumentsManager.DocumentSendingSuccessful -= OutgoingDocumentsManager_DocumentSendingSuccessful;
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
            StartActivity(ComposeDocumentActivity.CreateIntent(Context, DocumentCreationModeFlag.Edit, e.Direction, outgoingDocumentGuid: e.Identifier));
        }

        #region OutgoingDocumentManager event handlers

        void OutgoingDocumentsManager_DocumentAddedToQueue(object sender, Guid e)
        {
            //Take the document from file storage and add it at the end of the queue

            //Maybe not necessary.
            //If we are in another folder, then we don't need it
            //If we are in this folder this is not called, and we need something like tiny messenger or whatever
        }

        void OutgoingDocumentsManager_DocumentBeingSent(object sender, OutgoingDocumentContainer e)
        {
            var position = adapter.GetPosition(e.Info.Identifier);
            if (position >= 0)
            {
                var dp = adapter.Items[position];
                dp.State = OutgoingDocumentState.Sending;
                adapter.NotifyItemChanged(position);
            }
        }

        void OutgoingDocumentsManager_DocumentSendingFailed(object sender, OutgoingDocumentContainer e)
        {
            var position = adapter.GetPosition(e.Info.Identifier);
            if (position >= 0)
            {
                var dp = adapter.Items[position];
                dp.State = OutgoingDocumentState.Failed;
                adapter.NotifyItemChanged(position);
            }
        }

        void OutgoingDocumentsManager_DocumentSendingSuccessful(object sender, OutgoingDocumentContainer e)
        {
            var position = adapter.GetPosition(e.Info.Identifier);
            if (position >= 0)
            {
                adapter.RemoveItemsAtIndex(position);
                adapter.NotifyItemRemoved(position);
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

        #region Action Mode related

        bool ActionMode.ICallback.OnActionItemClicked(ActionMode mode, IMenuItem item)
        {
            throw new NotImplementedException();
        }

        bool ActionMode.ICallback.OnCreateActionMode(ActionMode mode, IMenu menu)
        {
            throw new NotImplementedException();
        }

        void ActionMode.ICallback.OnDestroyActionMode(ActionMode mode)
        {
            throw new NotImplementedException();
        }

        bool ActionMode.ICallback.OnPrepareActionMode(ActionMode mode, IMenu menu)
        {
            throw new NotImplementedException();
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
                    return selectedDocumentsInView.Values.ToList();
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
                    return selectedDocumentsInView.Count;
                }
            }

            readonly List<OutgoingDocumentPreview> ougoingDocumentPreviewsInView = new List<OutgoingDocumentPreview>(1000); //TODO modify
            readonly Dictionary<int, OutgoingDocumentPreview> selectedDocumentsInView = new Dictionary<int, OutgoingDocumentPreview>();
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

                odpvh.Selected = selectedDocumentsInView.ContainsKey(dp.Id);
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_documents_outgoing, parent, false);
                return new OutgoingDocumentPreviewViewHolder(itemView);
            }

            public void PrependItems(List<OutgoingDocumentPreview> items) //TODO check at the end which of those functions are used
            {
                var count = items.Count;
                ougoingDocumentPreviewsInView.InsertRange(0, items);
                NotifyItemRangeInserted(0, count);
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
                AppendItems(items);
            }

            public void Clear()
            {
                var size = ougoingDocumentPreviewsInView.Count;
                ougoingDocumentPreviewsInView.Clear();
                selectedDocumentsInView.Clear();
                NotifyItemRangeRemoved(0, size);
            }

            public bool IsSelected(OutgoingDocumentPreview outgoingDocumentPreview)
            {
                return selectedDocumentsInView.ContainsKey(outgoingDocumentPreview.Id);
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
                    selectedDocumentsInView[documentPreview.Id] = documentPreview;
                }
                else
                {
                    selectedDocumentsInView.Remove(documentPreview.Id);
                }
                NotifyItemChanged(position);
            }

            public void ClearSelections(bool notify = true)
            {
                var documents = selectedDocumentsInView.Values.ToArray();
                selectedDocumentsInView.Clear();
                if (notify)
                {
                    foreach (var document in documents)
                    {
                        NotifyItemChanged(GetPosition(document));
                    }
                }
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
