//
// Project: Mark5.Mobile.Droid
// File: DocumentsListFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Views.Common;

namespace Mark5.Mobile.Droid.Views.Fragments
{

    public class DocumentsListFragment : RetainableStateFragment
    {

        public Folder Folder
        {
            get;
            set;
        }

        bool refreshing;

        SwipeRefreshLayout refreshLayout;
        RecyclerView recyclerView;
        DocumentsListAdapter adapter;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);
            refreshLayout.SetColorSchemeResources(Resource.Color.lightbrown, Resource.Color.brown);
            refreshLayout.Refresh += async (sender, e) => await RefreshData(force: true);

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recyclerView);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));

            adapter = new DocumentsListAdapter(async (endId) => await RefreshData(endId: endId));
            adapter.ItemClicked += Adapter_ItemClicked;
            adapter.ItemLongClicked += Adapter_ItemLongClicked;
            recyclerView.SetAdapter(adapter);

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = Folder?.Name;
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = GetString(Resource.String.documents);
        }

        public override async void OnResume()
        {
            base.OnResume();

            if (adapter.ItemCount < 1)
            {
                await RefreshData();
            }
        }

        public override IRetainableState OnRetainInstanceState()
        {
            return new DocumentsListFragmentState
            {
                Folder = Folder,
                DocumentPreviews = adapter.Items
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var dlfs = restoredState as DocumentsListFragmentState;
            if (dlfs != null)
            {
                Folder = dlfs.Folder;
                adapter.AddItems(dlfs.DocumentPreviews);
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(DocumentsListFragment)} [FolderId={Folder.Id}, FolderName={Folder.Name}]";
        }

        async Task RefreshData(int startId = -1, int endId = -1, bool force = false)
        {
            if (refreshing) return;

            refreshing = true;
            refreshLayout.Post(() => refreshLayout.Refreshing = true); //Bug: fixed in support library v 24.2.0 (issue 77712)

            if (force)
            {
                adapter.Clear();
            }

            var documents = await Managers.DocumentsManager.GetDocumentPreviewsAsync(Folder, startId, endId);
            adapter.AddItems(documents);

            refreshLayout.Post(() => refreshLayout.Refreshing = false); //Bug: fixed in support library v 24.2.0 (issue 77712)
            refreshing = false;
        }

        void Adapter_ItemClicked(object sender, DocumentPreview e)
        {
            Toast.MakeText(Activity, "Document clicked!", ToastLength.Short).Show();
        }

        void Adapter_ItemLongClicked(object sender, DocumentPreview e)
        {
            Toast.MakeText(Activity, "Document long clicked!", ToastLength.Short).Show();
        }

        class DocumentsListFragmentState : IRetainableState
        {

            public Folder Folder { get; set; }

            public List<DocumentPreview> DocumentPreviews { get; set; }
        }

        #region RecyclerView Adapter/ViewHolder

        class DocumentsListAdapter : RecyclerView.Adapter
        {

            public List<DocumentPreview> Items
            {
                get
                {
                    return documentsInView;
                }
            }

            public override int ItemCount
            {
                get
                {
                    return documentsInView.Count;
                }
            }

            readonly List<DocumentPreview> documentsInView = new List<DocumentPreview>(1000);
            readonly Action<int> loadMoreAction;

            public event EventHandler<DocumentPreview> ItemClicked = delegate { };
            public event EventHandler<DocumentPreview> ItemLongClicked = delegate { };

            public DocumentsListAdapter(Action<int> loadMoreAction)
            {
                this.loadMoreAction = loadMoreAction;
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var dvh = holder as DocumentViewHolder;
                if (dvh == null) return;

                var d = documentsInView[position];

                dvh.ItemView.Click += (sender, e) => ItemClicked(this, d);
                dvh.ItemView.LongClick += (sender, e) => ItemLongClicked(this, d);

                dvh.Subject = d.Id + " " + d.Subject;

                if (loadMoreAction != null && position == ItemCount - 1)
                {
                    loadMoreAction(d.Id);
                }
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_documents, parent, false);
                return new DocumentViewHolder(itemView);
            }

            public void AddItems(IEnumerable<DocumentPreview> items)
            {
                var count = documentsInView.Count;
                documentsInView.AddRange(items);
                NotifyItemRangeInserted(count, count - documentsInView.Count);
            }

            public void Clear()
            {
                var size = documentsInView.Count;
                documentsInView.Clear();
                NotifyItemRangeRemoved(0, size);
            }
        }

        class DocumentViewHolder : RecyclerView.ViewHolder
        {

            public string Subject
            {
                get
                {
                    return subjectTextView.Text;
                }
                set
                {
                    subjectTextView.Text = value;
                }
            }

            readonly TextView subjectTextView;

            public DocumentViewHolder(View itemView)
                : base(itemView)
            {
                subjectTextView = itemView.FindViewById<TextView>(Resource.Id.list_item_document_subject);
            }
        }

        #endregion

    }
}

