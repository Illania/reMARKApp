//
// Project: Mark5.Mobile.Droid
// File: CommentsFragment.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Common.HubMessages;
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

                RefreshView();
            }
        }

        public void RefreshView()
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

        #region Options menu

        static class MenuItemActions
        {
            public const int EditComment = 10;
            public const int DeleteComment = 20;
        }

        public void CreateContextMenu(IContextMenu menu, View view, IContextMenuContextMenuInfo menuInfo)
        {
            menu.Add(Menu.None, MenuItemActions.EditComment, MenuItemActions.EditComment, Resource.String.edit);
            menu.Add(Menu.None, MenuItemActions.DeleteComment, MenuItemActions.DeleteComment, Resource.String.delete);

            var position = recyclerView.GetChildAdapterPosition(view);
            adapter.SelectedPosition = position;
        }

        public override bool OnContextItemSelected(IMenuItem item)
        {
            var comment = adapter.GetSelectedItem();

            if (item.ItemId == MenuItemActions.EditComment)
            {
                Dialogs.ShowEditTextDialog(Context, Resource.String.edit_comment_message, comment.Content, (text) => EditComment(comment, text), null,
                                          Resource.String.confirm, Resource.String.cancel);
            }

            if (item.ItemId == MenuItemActions.DeleteComment)
            {
                Dialogs.ShowYesNoDialog(Context, Resource.String.confirm_comment_deletion_title, Resource.String.confirm_comment_deletion_content,
                                             () => DeleteComment(comment), null,
                                        Resource.String.confirm, Resource.String.cancel);
            }

            return true;
        }

        #endregion

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
                        PlatformConfig.MessengerHub.Publish(new DocumentPreviewCommentCountChangedMessage(this, document.Id, document.Comments.Count));
                        break;
                    case ObjectType.Contact:
                        var contact = Entity as Contact;
                        newComment = await Managers.ContactsManager.AddComment(contact, newCommentContent);
                        break;
                    default:
                        throw new ArgumentException("The input business entity does not have comments defined in the model");
                }

                adapter.AppendItem(newComment);
                recyclerView.SmoothScrollToPosition(adapter.ItemCount);
                addCommentEditText.Text = string.Empty;
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Failed to add comment attachment [entity.Id={Entity?.Id}, commentContent={newCommentContent}] ", ex);

                dismissAction();
                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
            finally
            {
                dismissAction();
            }
        }

        void DeleteComment(Comment comment)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.deleting_comment, Resource.String.please_wait);

            Task.Run(async () =>
             {
                 switch (Entity.ObjectType)
                 {
                     case ObjectType.Document:
                         var document = Entity as Document;
                         await Managers.DocumentsManager.DeleteComment(document, comment);
                         PlatformConfig.MessengerHub.Publish(new DocumentPreviewCommentCountChangedMessage(this, document.Id, document.Comments.Count));
                         break;
                     case ObjectType.Contact:
                         var contact = Entity as Contact;
                         await Managers.ContactsManager.DeleteComment(contact, comment);
                         break;
                     default:
                         throw new ArgumentException("The input business entity does not have comments defined in the model");
                 }
             }).ContinueWith(async t =>
             {
                 dismissAction();

                 if (t.IsFaulted)
                 {
                     CommonConfig.Logger.Error($"Failed to delete comment from entity [objectType={Entity?.ObjectType}, entity.Id={Entity?.Id}, comment.Id={comment.Id}, comment.Content={comment.Content}] ", t.Exception.InnerException);
                     await Dialogs.ShowErrorDialogAsync(Activity, t.Exception.InnerException);
                 }
                 else
                 {
                     adapter.RemoveItem(comment);
                 }

             }, TaskScheduler.FromCurrentSynchronizationContext());

        }

        void EditComment(Comment comment, string newContent)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.editing_comment, Resource.String.please_wait);
            var newComment = comment.ShallowCopy();
            newComment.Content = newContent;

            Task.Run(async () =>
             {
                 switch (Entity.ObjectType)
                 {
                     case ObjectType.Document:
                         var document = Entity as Document;
                         await Managers.DocumentsManager.EditComment(document, newComment);
                         PlatformConfig.MessengerHub.Publish(new DocumentPreviewCommentCountChangedMessage(this, document.Id, document.Comments.Count));
                         break;
                     case ObjectType.Contact:
                         var contact = Entity as Contact;
                         await Managers.ContactsManager.EditComment(contact, newComment);
                         break;
                     default:
                         throw new ArgumentException("The input business entity does not have comments defined in the model");
                 }
             }).ContinueWith(async t =>
             {
                 dismissAction();

                 if (t.IsFaulted)
                 {
                     CommonConfig.Logger.Error($"Failed to edit comment for entity [objectType={Entity?.ObjectType}, entity.Id={Entity?.Id}, comment.Id={comment.Id}, comment.Content={comment.Content}] ", t.Exception.InnerException);
                     await Dialogs.ShowErrorDialogAsync(Activity, t.Exception.InnerException);
                 }
                 else
                 {
                     adapter.EditItem(newComment);
                 }
             }, TaskScheduler.FromCurrentSynchronizationContext());
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
                AddCommentText = addCommentEditText.Text
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var cfs = restoredState as CommentsFragmentState;
            if (cfs != null)
            {
                Entity = cfs.Entity;
                addCommentEditText.Text = cfs.AddCommentText;

                RefreshView();
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
                    return commentsInView;
                }
            }

            public override int ItemCount
            {
                get
                {
                    return commentsInView.Count;
                }
            }

            public int SelectedPosition { get; set; }

            readonly Action<IContextMenu, View, IContextMenuContextMenuInfo> action;
            readonly List<Comment> commentsInView = new List<Comment>();
            readonly Context context;

            public CommentsListAdapter(Context context, Action<IContextMenu, View, IContextMenuContextMenuInfo> action)
            {
                this.context = context;
                this.action = action;
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var cvh = holder as CommentViewHolder;
                if (cvh == null) return;

                var comment = commentsInView[position];
                var commentFromCurrentUser = ServerConfig.SystemSettings.UserInfo.User.Id == comment.UserId;

                if (commentFromCurrentUser)
                {
                    cvh.ItemView.SetOnCreateContextMenuListener(new ActionOnCreateContextMenuListener(action));
                }

                cvh.Username = commentFromCurrentUser ? "Me" : comment.UserName;
                cvh.Date = comment.DateAddedTimestamp
                    .ConvertTimestampMillisecondsToDateTime()
                    .ConvertUtcToServerTime()
                    .ConvertDateTimeToTimestampMilliseconds()
                    .FormatServerTimestampAsCompactShortDateTimeString(context);
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

            public void RemoveItem(Comment item)
            {
                var position = commentsInView.FindIndex(c => c.Id == item.Id);
                if (position >= 0)
                {
                    commentsInView.RemoveAt(position);
                    NotifyItemRemoved(position);
                }
            }

            public void EditItem(Comment item)
            {
                var position = commentsInView.FindIndex(c => c.Id == item.Id);
                if (position >= 0)
                {
                    commentsInView[position].Content = item.Content;
                    NotifyItemChanged(position);
                }
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
