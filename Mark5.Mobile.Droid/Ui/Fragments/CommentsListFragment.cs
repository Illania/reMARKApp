//
// Project: 
// File: CommentsFragment.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//

using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Text.Format;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Common.BusMesseges;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class CommentsListFragment : RetainableStateFragment
    {
        public BusinessEntity Entity { get; set; }
        public List<Comment> Comments
        {
            get
            {
                return adapter.Items;
            }
        }

        RecyclerView recyclerView;
        CommentsListAdapter adapter;

        AppCompatEditText addCommentEditText;
        AppCompatImageButton addCommentButton;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(CommentsListFragment)} [entity.Id={Entity?.Id}]...");

            var rootView = inflater.Inflate(Resource.Layout.list_comments, container, false);

            var refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            refreshLayout.Enabled = false;

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));
            RegisterForContextMenu(recyclerView);

            adapter = new CommentsListAdapter(Context, CreateContextMenu);
            adapter.ItemLongClicked += Adapter_ItemLongClicked;
            recyclerView.SetAdapter(adapter);

            addCommentEditText = rootView.FindViewById<AppCompatEditText>(Resource.Id.add_comment_edit_text);
            addCommentEditText.Hint = Resources.GetString(Resource.String.add_comment_hint);
            addCommentEditText.TextChanged += AddCommentEditText_TextChanged;

            addCommentButton = rootView.FindViewById<AppCompatImageButton>(Resource.Id.add_comment_button);
            addCommentButton.Enabled = false;
            addCommentButton.Click += AddCommentButton_Click;

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = "Comments";

            CommonConfig.Logger.Info($"Created {nameof(CommentsListFragment)} [entity.Id={Entity?.Id}]");
        }

        public override void OnResume()
        {
            base.OnResume();

            CommonConfig.Logger.Info($"Resuming {nameof(CommentsListFragment)} [entity.Id={Entity?.Id}]");

            if (adapter.ItemCount < 1)
            {
                CommonConfig.Logger.Info($"No elements - will refresh...");

                RefreshData();
            }
        }

        public void RefreshData()
        {
            switch (Entity.ObjectType)
            {
                case ObjectType.Document:
                    adapter.AppendItems((Entity as Document).Comments);
                    break;
                case ObjectType.Contact:
                    adapter.AppendItems((Entity as Contact).Comments);
                    break;
                default:
                    throw new ArgumentException("The input business entity does not have comments defined in the model");
            }

            recyclerView.SmoothScrollToPosition(adapter.ItemCount);
        }

        public override bool OnContextItemSelected(IMenuItem item)
        {
            var comment = adapter.GetSelectedItem();
            return true;
        }

        public void CreateContextMenu(IContextMenu menu, View view, IContextMenuContextMenuInfo menuInfo)
        {
            menu.Add(Menu.None, 20, 20, Resource.String.edit);
            menu.Add(Menu.None, 21, 21, Resource.String.delete);

            var position = recyclerView.GetChildAdapterPosition(view);
            adapter.SelectedPosition = position;
        }

        #region Event handlers

        async void AddCommentButton_Click(object sender, EventArgs e)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.adding_comment, Resource.String.please_wait);
            var newCommentContent = addCommentEditText.Text;

            try
            {
                Comment newComment;

                switch (Entity.ObjectType)
                {
                    case ObjectType.Document:
                        var document = Entity as Document;
                        newComment = await Managers.DocumentsManager.AddComment(document, newCommentContent);
                        document.Comments.Add(newComment); //TODO decide where to do this, here or in the manager
                        PlatformConfig.MessengerHub.Publish(new DocumentPreviewCommentCountChangedMessage(this, document.Id, document.Comments.Count));
                        break;
                    case ObjectType.Contact:
                        var contact = Entity as Contact;
                        newComment = await Managers.ContactsManager.AddComment(contact, newCommentContent);
                        contact.Comments.Add(newComment);
                        break;
                    default:
                        throw new ArgumentException("The input business entity does not have comments defined in the model");
                }

                Activity.RunOnUiThread(() =>
                {
                    adapter.AppendItem(newComment);
                    recyclerView.SmoothScrollToPosition(adapter.ItemCount);
                    addCommentEditText.Text = string.Empty;
                });
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Failed to add comment attachment [entity.Id={Entity?.Id}, commentContent={newCommentContent}] ", ex);

                dismissAction();
                await Dialogs.ShowErrorDialogAsync(Context, ex);
            }
            finally
            {
                dismissAction();
            }
        }

        void AddCommentEditText_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            addCommentButton.Enabled = !string.IsNullOrEmpty(addCommentEditText.Text);
        }

        void Adapter_ItemLongClicked(object sender, Comment e)
        {

        }

        #endregion

        #region Retained State methods

        public override IRetainableState OnRetainInstanceState()
        {
            CommonConfig.Logger.Info($"Retaining state [entity.Id={Entity?.Id}, addCommentText={addCommentEditText?.Text}");

            return new CommentsFragmentState
            {
                Entity = Entity,
                AddCommentText = addCommentEditText.Text,
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var cfs = restoredState as CommentsFragmentState;
            if (cfs != null)
            {
                Entity = cfs.Entity;
                addCommentEditText.Text = cfs.AddCommentText;

                RefreshData();
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(CommentsListFragment)} [businessEntity.Id={Entity.Id}]";
        }

        class CommentsFragmentState : IRetainableState
        {
            public BusinessEntity Entity { get; set; }
            public string AddCommentText { get; set; }
        }

        #endregion

        #region RecyclerView Adapter/ViewHolder

        class CommentsListAdapter : RecyclerView.Adapter
        {
            public List<Comment> Items
            {
                get
                {
                    return commentsInView.ToList();
                }
            }

            public override int ItemCount
            {
                get
                {
                    return commentsInView.Count();
                }
            }

            public int SelectedPosition { get; set; }

            readonly Action<IContextMenu, View, IContextMenuContextMenuInfo> action;
            readonly List<Comment> commentsInView = new List<Comment>();
            readonly Context context;

            public event EventHandler<Comment> ItemLongClicked = delegate { }; //TODO remove

            public CommentsListAdapter(Context context, Action<IContextMenu, View, IContextMenuContextMenuInfo> action)
            {
                this.context = context;
                this.action = action;
            }

            //TODO think if we need remove of listener on recycling

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var cvh = holder as CommentViewHolder;
                if (cvh == null) return;

                var comment = commentsInView[position];

                cvh.ItemView.SetOnCreateContextMenuListener(new ActionOnCreateContextMenuListener(action));

                cvh.Username = ServerConfig.SystemSettings.UserInfo.User.Id == comment.UserId ? "Me" : comment.UserName;

                var dateReceived = comment.DateAdded.ToServerTime();
                if (DateTime.Now.Date == dateReceived.Date)
                {
                    cvh.Date = DateFormat.Is24HourFormat(context) ? dateReceived.ToString("HH:mm") : dateReceived.ToString("hh:mm tt");
                }
                else if (DateTime.Now.AddDays(-1).Date == dateReceived.Date)
                {
                    cvh.Date = context.GetString(Resource.String.yesterday);
                }
                else
                {
                    var dfo = DateFormat.GetDateFormatOrder(context);
                    cvh.Date = dateReceived.ToString($"{dfo[0]}{dfo[0]}/{dfo[1]}{dfo[1]}/{dfo[2]}{dfo[2]}{dfo[2]}{dfo[2]}"); //TODO copied from documents, should we put this conversion in a common place?
                }

                cvh.Content = comment.Content;
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_comments, parent, false);
                return new CommentViewHolder(itemView);
            }

            public void AppendItems(List<Comment> items)
            {
                var count = commentsInView.Count;
                commentsInView.AddRange(items);
                NotifyItemRangeInserted(count, items.Count);
            }

            public void AppendItem(Comment item)
            {
                commentsInView.Add(item);
                NotifyItemInserted(commentsInView.Count - 1);
            }

            public Comment GetItemAtPosition(int position)
            {
                return commentsInView[position];
            }

            public Comment GetSelectedItem()
            {
                return commentsInView[SelectedPosition];
            }
        }

        class CommentViewHolder : RecyclerView.ViewHolder
        {
            readonly AppCompatTextView usernameTextView;
            readonly AppCompatTextView dateTextView;
            readonly AppCompatTextView contentTextView;

            public string Username
            {
                set
                {
                    usernameTextView.Text = value;
                }
            }

            public string Date
            {
                set
                {
                    dateTextView.Text = value;
                }
            }

            public string Content
            {
                set
                {
                    contentTextView.Text = value;
                }
            }

            public CommentViewHolder(View itemView)
                    : base(itemView)
            {
                usernameTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_comment_username);
                dateTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_comment_date);
                contentTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_comment_content);
            }
        }

        #endregion


    }

}
