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
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Utilities;
using System.Text.RegularExpressions;
using Android.Graphics;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class OutgoingDocumentListFragment : DocumentsListFragment
    {
        protected override async Task RefreshData(int startId = -1, int endId = -1, bool force = false)
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

                var outgoingDocumentPreviews = await Managers.DocumentsManager.GetOutgoingDocumentPreviewsAsync();
                adapter.AppendItems(outgoingDocumentPreviews.Cast<DocumentPreview>().ToList());
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

        //#region RecyclerView Adapter/ViewHolder

        //protected class OutgoingDocumentsListAdapter : DocumentsListAdapter
        //{

        //    public static class ViewType
        //    {
        //        public const int DocumentView = 0;
        //        public const int ExternalDocumentView = 1;
        //    }

        //    public List<DocumentPreview> Items
        //    {
        //        get
        //        {
        //            return documentPreviewsInView.ToList();
        //        }
        //    }

        //    public List<DocumentPreview> SelectedItems
        //    {
        //        get
        //        {
        //            return selectedDocumentsInView.Values.ToList();
        //        }
        //    }

        //    public override int ItemCount
        //    {
        //        get
        //        {
        //            return documentPreviewsInView.Count;
        //        }
        //    }

        //    public int SelectedItemCount
        //    {
        //        get
        //        {
        //            return selectedDocumentsInView.Count;
        //        }
        //    }

        //    readonly List<DocumentPreview> documentPreviewsInView = new List<DocumentPreview>(1000);
        //    readonly Dictionary<int, DocumentPreview> selectedDocumentsInView = new Dictionary<int, DocumentPreview>();
        //    readonly Action<int> loadMoreAction;
        //    readonly Context context;

        //    public event EventHandler<DocumentPreview> ItemClicked = delegate { };
        //    public event EventHandler<DocumentPreview> ItemLongClicked = delegate { };

        //    bool unreadIndicatorMe = PlatformConfig.Preferences.UnreadIndicatorMe;
        //    bool compactList = PlatformConfig.Preferences.CompactDocumentsList;

        //    public OutgoingDocumentsListAdapter(Context context) : base(context)
        //    {
        //        this.context = context;
        //    }

        //    public override int GetItemViewType(int position)
        //    {
        //        return documentPreviewsInView[position].Direction == DocumentDirection.External ? ViewType.ExternalDocumentView : ViewType.DocumentView;
        //    }

        //    public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        //    {
        //        if (holder is DocumentPreviewViewHolder)
        //        {
        //            var dpvh = holder as DocumentPreviewViewHolder;
        //            var dp = documentPreviewsInView[position];

        //            dpvh.ItemView.SetOnClickListener(new ActionOnClickListener(() => ItemClicked(this, dp)));
        //            dpvh.ItemView.SetOnLongClickListener(new ActionOnLongClickListener(() => ItemLongClicked(this, dp)));

        //            if (dp.Direction == DocumentDirection.Incoming)
        //            {
        //                var address = dp.Addresses.FirstOrDefault(da => da.AddressType == DocumentAddressType.From);
        //                dpvh.Recipent = address == null ? string.Empty : string.IsNullOrWhiteSpace(address.Name) ? address.Address : address.Name;
        //            }
        //            else
        //            {
        //                var address = dp.Addresses.Where(da => da.AddressType == DocumentAddressType.To || da.AddressType == DocumentAddressType.Cc || da.AddressType == DocumentAddressType.Bcc).OrderBy(da => da.AddressType).FirstOrDefault();
        //                dpvh.Recipent = address == null ? string.Empty : string.IsNullOrWhiteSpace(address.Name) ? address.Address : address.Name;
        //            }

        //            dpvh.Subject = string.IsNullOrWhiteSpace(dp.Subject) ? context.GetString(Resource.String.no_subject) : dp.Subject;
        //            dpvh.Date = dp.DateReceivedTimestamp
        //                .ConvertTimestampMillisecondsToDateTime()
        //                .ConvertUtcToServerTime()
        //                .ConvertDateTimeToTimestampMilliseconds()
        //                .FormatServerTimestampAsCompactShortDateTimeString(context);
        //            dpvh.Preview = string.IsNullOrWhiteSpace(dp.Preview) ? context.GetString(Resource.String.no_content) : Regex.Replace(dp.Preview, @"^\s+$[\r\n]*", "", RegexOptions.Multiline);
        //            dpvh.Categories = dp.Categories;
        //            dpvh.IncomingIndicator = dp.Direction == DocumentDirection.Incoming;
        //            dpvh.OutgoingIndicator = dp.Direction == DocumentDirection.Outgoing;
        //            dpvh.DraftIndicator = dp.Direction == DocumentDirection.Draft;
        //            dpvh.UnreadIndicator = unreadIndicatorMe ? !dp.IsReadByCurrent : !dp.IsReadByAnyone;
        //            dpvh.AttachmentIndicator = dp.AttachmentsCount > 0;
        //            dpvh.CommentIndicator = dp.CommentsCount > 0;

        //            dpvh.Compact = compactList;
        //            dpvh.Selected = selectedDocumentsInView.ContainsKey(dp.Id);

        //            if (loadMoreAction != null && position == ItemCount - 1)
        //            {
        //                loadMoreAction(dp.Id);
        //            }
        //        }
        //    }

        //    public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        //    {
        //        if (viewType == ViewType.DocumentView)
        //        {
        //            var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_documents, parent, false);
        //            return new DocumentPreviewViewHolder(itemView);
        //        }

        //        return null;
        //    }

        //    public void PrependItems(List<DocumentPreview> items)
        //    {
        //        var count = items.Count;
        //        documentPreviewsInView.InsertRange(0, items);
        //        NotifyItemRangeInserted(0, count);
        //    }

        //    public void AppendItems(List<DocumentPreview> items)
        //    {
        //        var count = documentPreviewsInView.Count;
        //        documentPreviewsInView.AddRange(items);
        //        NotifyItemRangeInserted(count, items.Count);
        //    }

        //    public void RemoveItemsAtIndex(int index)
        //    {
        //        documentPreviewsInView.RemoveAt(index);
        //    }

        //    public void ReplaceItems(List<DocumentPreview> items)
        //    {
        //        Clear();
        //        AppendItems(items);
        //    }

        //    public void Clear()
        //    {
        //        var size = documentPreviewsInView.Count;
        //        documentPreviewsInView.Clear();
        //        selectedDocumentsInView.Clear();
        //        NotifyItemRangeRemoved(0, size);
        //    }

        //    public bool IsSelected(DocumentPreview documentPreview)
        //    {
        //        return selectedDocumentsInView.ContainsKey(documentPreview.Id);
        //    }

        //    public void SetSelected(List<DocumentPreview> documentPreviews, bool selected)
        //    {
        //        foreach (var document in documentPreviews)
        //        {
        //            SetSelected(document, selected);
        //        }
        //    }

        //    public void SetSelected(DocumentPreview documentPreview, bool selected)
        //    {
        //        var position = GetPosition(documentPreview);
        //        if (position < 0) return;

        //        if (selected)
        //        {
        //            selectedDocumentsInView[documentPreview.Id] = documentPreview;
        //        }
        //        else
        //        {
        //            selectedDocumentsInView.Remove(documentPreview.Id);
        //        }
        //        NotifyItemChanged(position);
        //    }

        //    public void ClearSelections(bool notify = true)
        //    {
        //        var documents = selectedDocumentsInView.Values.ToArray();
        //        selectedDocumentsInView.Clear();
        //        if (notify)
        //        {
        //            foreach (var document in documents)
        //            {
        //                NotifyItemChanged(GetPosition(document));
        //            }
        //        }
        //    }

        //    public int GetPosition(int documentPreviewId)
        //    {
        //        var position = -1;
        //        for (var i = 0; i < documentPreviewsInView.Count; i++)
        //        {
        //            if (documentPreviewsInView[i].Id == documentPreviewId)
        //            {
        //                position = i;
        //                break;
        //            }
        //        }
        //        return position;
        //    }

        //    public int GetPosition(DocumentPreview documentPreview)
        //    {
        //        return GetPosition(documentPreview.Id);
        //    }
        //}

        //class DocumentPreviewViewHolder : RecyclerView.ViewHolder
        //{

        //    public string Recipent
        //    {
        //        set
        //        {
        //            recipentTextView.Text = value;
        //        }
        //    }

        //    public string Date
        //    {
        //        set
        //        {
        //            dateTextView.Text = value;
        //        }
        //    }

        //    public string Subject
        //    {
        //        set
        //        {
        //            subjectTextView.Text = value;
        //        }
        //    }

        //    public string Preview
        //    {
        //        set
        //        {
        //            previewTextView.Text = value;
        //        }
        //    }

        //    public List<Category> Categories
        //    {
        //        set
        //        {
        //            categoriesLayout.RemoveAllViews();

        //            foreach (var hexColor in value.Select(c => c.HexColor))
        //            {
        //                var view = new View(ItemView.Context)
        //                {
        //                    LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent, 1f),
        //                    Background = new ColorDrawable(Color.ParseColor(hexColor))
        //                };
        //                categoriesLayout.AddView(view);
        //            }
        //        }
        //    }

        //    public bool IncomingIndicator
        //    {
        //        set
        //        {
        //            incomingImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
        //        }
        //    }

        //    public bool OutgoingIndicator
        //    {
        //        set
        //        {
        //            outgoingImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
        //        }
        //    }

        //    public bool DraftIndicator
        //    {
        //        set
        //        {
        //            draftImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
        //        }
        //    }

        //    public bool UnreadIndicator
        //    {
        //        set
        //        {
        //            unreadImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
        //        }
        //    }

        //    public bool AttachmentIndicator
        //    {
        //        set
        //        {
        //            attachmentImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
        //        }
        //    }

        //    public bool CommentIndicator
        //    {
        //        set
        //        {
        //            commentImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
        //        }
        //    }

        //    public bool Compact
        //    {
        //        set
        //        {
        //            attachmentImageView.Visibility = value ? ViewStates.Gone : attachmentImageView.Visibility;
        //            commentImageView.Visibility = value ? ViewStates.Gone : commentImageView.Visibility;
        //            previewTextView.Visibility = value ? ViewStates.Gone : previewTextView.Visibility;
        //        }
        //    }

        //    public bool Selected
        //    {
        //        set
        //        {
        //            selectedOverlay.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
        //        }
        //    }

        //    readonly AppCompatTextView recipentTextView;
        //    readonly AppCompatTextView dateTextView;
        //    readonly AppCompatTextView subjectTextView;
        //    readonly AppCompatTextView previewTextView;
        //    readonly LinearLayoutCompat categoriesLayout;
        //    readonly AppCompatImageView incomingImageView;
        //    readonly AppCompatImageView outgoingImageView;
        //    readonly AppCompatImageView draftImageView;
        //    readonly AppCompatImageView unreadImageView;
        //    readonly AppCompatImageView attachmentImageView;
        //    readonly AppCompatImageView commentImageView;
        //    readonly View selectedOverlay;

        //    public DocumentPreviewViewHolder(View itemView)
        //            : base(itemView)
        //    {
        //        recipentTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_document_recipent);
        //        dateTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_document_date);
        //        subjectTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_document_subject);
        //        previewTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_document_preview);
        //        categoriesLayout = itemView.FindViewById<LinearLayoutCompat>(Resource.Id.list_item_document_categories);
        //        incomingImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_direction_incoming);
        //        outgoingImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_direction_outgoing);
        //        draftImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_direction_draft);
        //        unreadImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_unread);
        //        attachmentImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_attachment);
        //        commentImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_comment);
        //        selectedOverlay = itemView.FindViewById<View>(Resource.Id.selected_overlay);
        //    }
        //}

        //#endregion

    }
}
