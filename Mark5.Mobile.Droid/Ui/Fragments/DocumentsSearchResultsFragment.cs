//
// Project: Mark5.Mobile.Droid
// File: DocumentSearchResultsFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
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
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{

    public class DocumentsSearchResultsFragment : RetainableStateFragment
    {

        public SearchDocumentsCriteria Criteria { get; set; }
        public Action CloseRequest { get; set; }

        int searchId;

        SwipeRefreshLayout refreshLayout;
        RecyclerView recyclerView;
        DocumentSearchResultsAdapter adapter;

        #region Fragment overrides

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(DocumentsSearchResultsFragment)} [criteria={Criteria}]...");

            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            refreshLayout.SetColorSchemeResources(Resource.Color.lightbrown, Resource.Color.brown);
            refreshLayout.Enabled = false;

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));

            adapter = new DocumentSearchResultsAdapter(Activity);
            adapter.ItemClicked += Adapter_ItemClicked;
            recyclerView.SetAdapter(adapter);

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.search_documents_result);

            CommonConfig.Logger.Info($"Created {nameof(DocumentsSearchResultsFragment)} [criteria={Criteria}]");
        }

        public override async void OnResume()
        {
            base.OnResume();

            CommonConfig.Logger.Info($"Resuming {nameof(DocumentsSearchResultsFragment)} [criteria={Criteria}]...");

            if (adapter.ItemCount < 1)
            {
                CommonConfig.Logger.Info($"No elements - will refresh...");

                await RefreshData();
            }
        }

        public override void OnPause()
        {
            base.OnPause();

            CommonConfig.Logger.Info($"Pausing {nameof(DocumentsSearchResultsFragment)} [criteria={Criteria}]...");
        }

        #endregion

        #region RetainableStateFragment overrides

        public override IRetainableState OnRetainInstanceState()
        {
            CommonConfig.Logger.Info($"Retaining state [criteria={Criteria}, documentPreviews.Count={adapter?.ItemCount}]...");

            return new DocumentSearchResultsFragmentState
            {
                Criteria = Criteria,
                SearchId = searchId,
                DocumentPreviews = adapter.Items
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var dlfs = restoredState as DocumentSearchResultsFragmentState;
            if (dlfs != null)
            {
                CommonConfig.Logger.Info($"Restoring state [dlfs.criteria={dlfs.Criteria}, dlfs.items.count={dlfs.DocumentPreviews?.Count}]...");

                Criteria = dlfs.Criteria;
                searchId = dlfs.SearchId;
                adapter.AppendItems(dlfs.DocumentPreviews);
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(DocumentsSearchResultsFragment)}]";
        }

        #endregion

        #region Refreshing

        async Task RefreshData()
        {
            try
            {
                CommonConfig.Logger.Info($"Refresh running...");

                refreshLayout.Refreshing = true;

                var searchResults = await Managers.SearchManager.SearchDocumentsAsync(Criteria);
                searchId = searchResults.SearchId;
                adapter.AppendItems(searchResults.DocumentPreviews);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Downloading documents failed [criteria={Criteria}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);

                if (CloseRequest != null) CloseRequest();
            }
            finally
            {
                refreshLayout.Refreshing = false;

                CommonConfig.Logger.Info($"Refresh finished");
            }
        }

        #endregion

        #region Adapter callbacks

        void Adapter_ItemClicked(object sender, DocumentPreview documentPreview)
        {
            var i = new Intent(Activity, typeof(DocumentActivity));
            i.PutExtra(DocumentActivity.SearchIdIntentKey, searchId);
            i.PutExtra(DocumentActivity.DocumentPreviewIntentKey, SerializationUtils.Serialize(documentPreview));
            i.PutExtra(DocumentActivity.ReadOnlyModeIntentKey, true);
            StartActivity(i);
        }

        #endregion

        #region State

        class DocumentSearchResultsFragmentState : IRetainableState
        {

            public int SearchId { get; set; }

            public SearchDocumentsCriteria Criteria { get; set; }

            public List<DocumentPreview> DocumentPreviews { get; set; }
        }

        #endregion

        #region RecyclerView Adapter/ViewHolder

        class DocumentSearchResultsAdapter : RecyclerView.Adapter
        {

            public static class ViewType
            {
                public const int DocumentView = 0;
                public const int ExternalDocumentView = 1;
            }

            public List<DocumentPreview> Items
            {
                get
                {
                    return documentPreviewsInView;
                }
            }

            public override int ItemCount
            {
                get
                {
                    return documentPreviewsInView.Count;
                }
            }

            readonly List<DocumentPreview> documentPreviewsInView = new List<DocumentPreview>(1000);
            readonly Context context;

            public event EventHandler<DocumentPreview> ItemClicked = delegate { };

            public DocumentSearchResultsAdapter(Context context)
            {
                this.context = context;
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                if (holder is DocumentPreviewViewHolder)
                {
                    var dpvh = holder as DocumentPreviewViewHolder;
                    var dp = documentPreviewsInView[position];

                    dpvh.ItemView.SetOnClickListener(new ActionOnClickListener(() => ItemClicked(this, dp)));

                    if (dp.Direction == DocumentDirection.Incoming)
                    {
                        var address = dp.Addresses.FirstOrDefault(da => da.AddressType == DocumentAddressType.From);
                        dpvh.Recipent = address == null ? string.Empty : string.IsNullOrWhiteSpace(address.Name) ? address.Address : address.Name;
                    }
                    else
                    {
                        var address = dp.Addresses.Where(da => da.AddressType == DocumentAddressType.To || da.AddressType == DocumentAddressType.Cc || da.AddressType == DocumentAddressType.Bcc).OrderBy(da => da.AddressType).FirstOrDefault();
                        dpvh.Recipent = address == null ? string.Empty : string.IsNullOrWhiteSpace(address.Name) ? address.Address : address.Name;
                    }

                    dpvh.Subject = string.IsNullOrWhiteSpace(dp.Subject) ? context.GetString(Resource.String.no_subject) : dp.Subject;
                    dpvh.Date = dp.DateReceivedTimestamp
                        .ConvertTimestampMillisecondsToDateTime()
                        .ConvertUtcToServerTime()
                        .ConvertDateTimeToTimestampMilliseconds()
                        .FormatServerTimestampAsCompactShortDateTimeString(context);
                    dpvh.Preview = string.IsNullOrWhiteSpace(dp.Preview) ? context.GetString(Resource.String.no_content) : Regex.Replace(dp.Preview, @"^\s+$[\r\n]*", "", RegexOptions.Multiline);
                    dpvh.Categories = dp.Categories;
                    dpvh.IncomingIndicator = dp.Direction == DocumentDirection.Incoming;
                    dpvh.OutgoingIndicator = dp.Direction == DocumentDirection.Outgoing;
                    dpvh.DraftIndicator = dp.Direction == DocumentDirection.Draft;
                    dpvh.UnreadIndicator = PlatformConfig.Preferences.UnreadIndicatorMe ? !dp.IsReadByCurrent : !dp.IsReadByAnyone;
                    dpvh.AttachmentIndicator = dp.AttachmentsCount > 0;
                    dpvh.CommentIndicator = dp.CommentsCount > 0;
                }
                else if (holder is ExternalDocumentPreviewViewHolder)
                {
                    var edpvh = holder as ExternalDocumentPreviewViewHolder;
                    var dp = documentPreviewsInView[position];

                    edpvh.ItemView.SetOnClickListener(new ActionOnClickListener(() => ItemClicked(this, dp)));

                    edpvh.Date = dp.DateReceivedTimestamp
                    .ConvertTimestampMillisecondsToDateTime()
                    .ConvertUtcToServerTime()
                    .ConvertDateTimeToTimestampMilliseconds()
                    .FormatServerTimestampAsCompactShortDateTimeString(context);
                    edpvh.Name = string.IsNullOrWhiteSpace(dp.Subject) ? context.GetString(Resource.String.no_subject) : dp.Subject;
                    edpvh.Preview = string.IsNullOrWhiteSpace(dp.Preview) ? context.GetString(Resource.String.no_content) : Regex.Replace(dp.Preview, @"^\s+$[\r\n]*", "", RegexOptions.Multiline);
                    edpvh.Categories = dp.Categories;
                    edpvh.CommentIndicator = dp.CommentsCount > 0;
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

            public void AppendItems(List<DocumentPreview> items)
            {
                var count = documentPreviewsInView.Count;
                documentPreviewsInView.AddRange(items);
                NotifyItemRangeInserted(count, items.Count);
            }
        }

        class DocumentPreviewViewHolder : RecyclerView.ViewHolder
        {

            public string Recipent
            {
                set
                {
                    recipentTextView.Text = value;
                }
            }

            public string Date
            {
                set
                {
                    dateTextView.Text = value;
                }
            }

            public string Subject
            {
                set
                {
                    subjectTextView.Text = value;
                }
            }

            public string Preview
            {
                set
                {
                    previewTextView.Text = value;
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

            public bool IncomingIndicator
            {
                set
                {
                    incomingImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
                }
            }

            public bool OutgoingIndicator
            {
                set
                {
                    outgoingImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
                }
            }

            public bool DraftIndicator
            {
                set
                {
                    draftImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
                }
            }

            public bool UnreadIndicator
            {
                set
                {
                    unreadImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
                }
            }

            public bool AttachmentIndicator
            {
                set
                {
                    attachmentImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
                }
            }

            public bool CommentIndicator
            {
                set
                {
                    commentImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
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
            readonly LinearLayoutCompat categoriesLayout;
            readonly AppCompatImageView incomingImageView;
            readonly AppCompatImageView outgoingImageView;
            readonly AppCompatImageView draftImageView;
            readonly AppCompatImageView unreadImageView;
            readonly AppCompatImageView attachmentImageView;
            readonly AppCompatImageView commentImageView;
            readonly View selectedOverlay;

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
                selectedOverlay = itemView.FindViewById<View>(Resource.Id.selected_overlay);
            }
        }

        class ExternalDocumentPreviewViewHolder : RecyclerView.ViewHolder
        {

            public string Name
            {
                set
                {
                    nameTextView.Text = value;
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

            public bool CommentIndicator
            {
                set
                {
                    commentImageView.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
                }
            }

            public bool Selected
            {
                set
                {
                    selectedOverlay.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
                }
            }

            readonly AppCompatTextView nameTextView;
            readonly AppCompatTextView dateTextView;
            readonly AppCompatTextView previewTextView;
            readonly LinearLayoutCompat categoriesLayout;
            readonly AppCompatImageView commentImageView;
            readonly View selectedOverlay;

            public ExternalDocumentPreviewViewHolder(View itemView)
                    : base(itemView)
            {
                nameTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_document_external_name);
                dateTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_document_external_date);
                previewTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_document_external_preview);
                categoriesLayout = itemView.FindViewById<LinearLayoutCompat>(Resource.Id.list_item_document_external_categories);
                commentImageView = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_document_external_comment);
                selectedOverlay = itemView.FindViewById<View>(Resource.Id.selected_overlay);
            }
        }

        #endregion
    }
}

