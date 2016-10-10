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
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class CommentsFragment : RetainableStateFragment
    {
        public BusinessEntity Entity { get; set; }

        RecyclerView recyclerView;
        CommentsListAdapter adapter;

        AppCompatEditText addCommentEditText;
        AppCompatImageButton addCommentButton;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(CommentsFragment)} [entity.Id={Entity?.Id}]...");

            var rootView = inflater.Inflate(Resource.Layout.list_comments, container, false);

            var refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            refreshLayout.Enabled = false;

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));

            adapter = new CommentsListAdapter(Context); //TODO need to add events for deletion, editing and so on
            recyclerView.SetAdapter(adapter);

            addCommentEditText = rootView.FindViewById<AppCompatEditText>(Resource.Id.add_comment_edit_text);
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

            CommonConfig.Logger.Info($"Created {nameof(CommentsFragment)} [entity.Id={Entity?.Id}]");
        }

        public override void OnResume()
        {
            base.OnResume();

            CommonConfig.Logger.Info($"Resuming {nameof(CommentsFragment)} [entity.Id={Entity?.Id}]");

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
                        newComment = await Managers.DocumentsManager.AddComment(Entity as Document, newCommentContent);
                        break;
                    case ObjectType.Contact:
                        newComment = await Managers.ContactsManager.AddComment(Entity as Contact, newCommentContent);
                        break;
                    default:
                        throw new ArgumentException("The input business entity does not have comments defined in the model");
                }

                adapter.AppendItem(newComment);
                addCommentEditText.Text = string.Empty;
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
            return $"{nameof(CommentsFragment)} [businessEntity.Id={Entity.Id}]";
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

            readonly List<Comment> commentsInView = new List<Comment>();
            readonly Context context;

            public CommentsListAdapter(Context context)
            {
                this.context = context;
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var cvh = holder as CommentViewHolder;
                if (cvh == null) return;

                var comment = commentsInView[position];
                cvh.Username = comment.UserName; //TODO need to put ME if the same user

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
