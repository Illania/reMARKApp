using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class CommentsListFragment : RetainableStateFragment
    {
        public List<Comment> Comments => adapter.Items;

        const string BusinessEntityBundleKey = "BusinessEntity_d475f087-b641-494d-b56b-152e945b0823";

        const int SecondsToEdit = 60;

        BusinessEntity entity;

        RecyclerView recyclerView;
        CommentsListAdapter adapter;

        AppCompatEditText addCommentEditText;
        AppCompatImageButton addCommentButton;

        public static (CommentsListFragment fragment, string tag) NewInstance(BusinessEntity be)
        {
            var args = new Bundle();

            if (be != null)
                args.PutString(BusinessEntityBundleKey, Serializer.Serialize(be));

            var fragment = new CommentsListFragment();
            fragment.Arguments = args;

            var tag = $"{nameof(CommentsListFragment)} [businessEntity.Id={be.Id}]";

            return (fragment, tag);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (Arguments.ContainsKey(BusinessEntityBundleKey))
                entity = Serializer.Deserialize<BusinessEntity>(Arguments.GetString(BusinessEntityBundleKey));

            CommonConfig.Logger.Info($"Creating {nameof(CommentsListFragment)} [entity.Id={entity?.Id}]...");

            var rootView = inflater.Inflate(Resource.Layout.list_comments, container, false);

            var emptyView = rootView.FindViewById<AppCompatTextView>(Resource.Id.empty_view);
            emptyView.SetText(Resource.String.no_comments);

            var refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            refreshLayout.Enabled = false;

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));
            RegisterForContextMenu(recyclerView);

            adapter = new CommentsListAdapter(Context, CreateContextMenu);
            adapter.RegisterAdapterDataObserver(new LambdaEmptyAdapterObserver(() =>
            {
                if (recyclerView.GetAdapter() != adapter)
                    return;

                emptyView.Visibility = adapter.ItemCount < 1 ? ViewStates.Visible : ViewStates.Gone;
                recyclerView.Visibility = adapter.ItemCount > 0 ? ViewStates.Visible : ViewStates.Gone;
            }));
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

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.comments);
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = null;

            CommonConfig.Logger.Info($"Created {nameof(CommentsListFragment)} [entity.Id={entity?.Id}]");
        }

        public override void OnResume()
        {
            base.OnResume();

            CommonConfig.Logger.Info($"Resuming {nameof(CommentsListFragment)} [entity.Id={entity?.Id}]");

            if (adapter.ItemCount < 1)
            {
                CommonConfig.Logger.Info($"No elements - will refresh...");

                RefreshView();
            }
        }

        public void RefreshView()
        {
            switch (entity.ObjectType)
            {
                case ObjectType.Document:
                    adapter.AppendItems((entity as Document).Comments);
                    break;
                case ObjectType.Contact:
                    adapter.AppendItems((entity as Contact).Comments);
                    break;
                default:
                    throw new ArgumentException("The input business entity does not have comments defined in the model");
            }

            recyclerView.SmoothScrollToPosition(adapter.ItemCount);
        }

        #region Options menu

        public override bool OnContextItemSelected(IMenuItem item)
        {
            var comment = adapter.GetSelectedItem();

            if (item.ItemId == MenuItemActions.EditComment)
            {
                var isEditable = DateTime.UtcNow.Subtract(comment.DateAddedTimestamp.ConvertTimestampMillisecondsToDateTime()).TotalSeconds <= SecondsToEdit;
                if (!isEditable)
                    Dialogs.ShowConfirmDialog(Context, Resource.String.cannot_edit_comment_title, Resource.String.cannot_edit_comment_content);
                else
                    Dialogs.ShowEditTextDialog(Context, Resource.String.edit_comment_message, comment.Content, (text) => EditComment(comment, text), null, Resource.String.confirm, Resource.String.cancel);
            }

            if (item.ItemId == MenuItemActions.DeleteComment)
                Dialogs.ShowYesNoDialog(Context, Resource.String.confirm_comment_deletion_title, Resource.String.confirm_comment_deletion_content, () => DeleteComment(comment), null, Resource.String.confirm, Resource.String.cancel);

            return true;
        }

        public void CreateContextMenu(IContextMenu menu, View view, IContextMenuContextMenuInfo menuInfo)
        {
            var position = recyclerView.GetChildAdapterPosition(view);
            adapter.SelectedPosition = position;

            var comment = adapter.GetSelectedItem();
            var isEditable = DateTime.UtcNow.Subtract(comment.DateAddedTimestamp.ConvertTimestampMillisecondsToDateTime()).TotalSeconds <= SecondsToEdit;

            if (isEditable)
                menu.Add(Menu.None, MenuItemActions.EditComment, MenuItemActions.EditComment, Resource.String.edit);

            menu.Add(Menu.None, MenuItemActions.DeleteComment, MenuItemActions.DeleteComment, Resource.String.delete);
        }

        static class MenuItemActions
        {
            public const int EditComment = 10;
            public const int DeleteComment = 20;
        }

        #endregion

        #region Event handlers

        async void DeleteComment(Comment comment)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.deleting_comment, Resource.String.please_wait);
            Task t;

            switch (entity.ObjectType)
            {
                case ObjectType.Document:
                    var document = entity as Document;
                    t = Managers.DocumentsManager.DeleteComment(document, comment);
                    await t;
                    break;
                case ObjectType.Contact:
                    var contact = entity as Contact;
                    t = Managers.ContactsManager.DeleteComment(contact, comment);
                    await t;
                    break;

                default:
                    throw new ArgumentException("The input business entity does not have comments defined in the model");
            }

            dismissAction();

            if (t.IsFaulted)
            {
                CommonConfig.Logger.Error($"Failed to delete comment from entity [objectType={entity?.ObjectType}, entity.Id={entity?.Id}, comment.Id={comment.Id}, comment.Content={comment.Content}] ", t.Exception.InnerException);
                await Dialogs.ShowErrorDialogAsync(Activity, t.Exception.InnerException);
            }
            else
            {
                adapter.RemoveItem(comment);
            }
        }

        async void EditComment(Comment comment, string newContent)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.editing_comment, Resource.String.please_wait);
            var newComment = comment.ShallowCopy();
            newComment.Content = newContent;

            Task<bool> t;

            switch (entity.ObjectType)
            {
                case ObjectType.Document:
                    var document = entity as Document;
                    t = Managers.DocumentsManager.EditComment(document, newComment);
                    await t;
                    break;
                case ObjectType.Contact:
                    var contact = entity as Contact;
                    t = Managers.ContactsManager.EditComment(contact, newComment);
                    await t;
                    break;

                default:
                    throw new ArgumentException("The input business entity does not have comments defined in the model");
            }

            dismissAction();

            if (t.IsFaulted)
            {
                CommonConfig.Logger.Error($"Failed to edit comment for entity [objectType={entity?.ObjectType}, entity.Id={entity?.Id}, comment.Id={comment.Id}, comment.Content={comment.Content}] ", t.Exception.InnerException);
                await Dialogs.ShowErrorDialogAsync(Activity, t.Exception.InnerException);
            }
            else
            {
                adapter.EditItem(newComment);
            }
        }

        void AddCommentEditText_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            addCommentButton.Enabled = !string.IsNullOrEmpty(addCommentEditText.Text);
        }

        async void AddCommentButton_Click(object sender, EventArgs e)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.adding_comment, Resource.String.please_wait);
            var newCommentContent = addCommentEditText.Text;

            try
            {
                Comment newComment;

                switch (entity.ObjectType)
                {
                    case ObjectType.Document:
                        var document = entity as Document;
                        newComment = await Managers.DocumentsManager.AddComment(document, newCommentContent);
                        break;
                    case ObjectType.Contact:
                        var contact = entity as Contact;
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
                CommonConfig.Logger.Error($"Failed to add comment attachment [entity.Id={entity?.Id}, commentContent={newCommentContent}] ", ex);

                dismissAction();
                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
            finally
            {
                dismissAction();
            }
        }

        #endregion

        #region Retained State methods

        public override IRetainableState OnRetainInstanceState()
        {
            CommonConfig.Logger.Info($"Retaining state [entity.Id={entity?.Id}, addCommentText={addCommentEditText?.Text}");

            return new CommentsFragmentState
            {
                Entity = entity,
                AddCommentText = addCommentEditText.Text
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            if (restoredState is CommentsFragmentState cfs)
            {
                entity = cfs.Entity;
                addCommentEditText.Text = cfs.AddCommentText;

                RefreshView();
            }
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
            public override int ItemCount => Items.Count;

            public List<Comment> Items { get; } = new List<Comment>();

            public int SelectedPosition { get; set; }

            readonly Action<IContextMenu, View, IContextMenuContextMenuInfo> action;
            readonly Context context;

            public CommentsListAdapter(Context context, Action<IContextMenu, View, IContextMenuContextMenuInfo> action)
            {
                this.context = context;
                this.action = action;
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var cvh = holder as CommentViewHolder;
                if (cvh == null)
                    return;

                var comment = Items[position];
                var commentFromCurrentUser = ServerConfig.SystemSettings.UserInfo.User.Id == comment.UserId;

                if (commentFromCurrentUser)
                    cvh.ItemView.SetOnCreateContextMenuListener(new ActionOnCreateContextMenuListener(action));

                cvh.Username = commentFromCurrentUser ? "Me" : comment.UserName;
                cvh.Date = comment.DateAddedTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTime().ConvertDateTimeToTimestampMilliseconds().FormatUserTimestampAsCompactShortDateTimeString(context);
                cvh.Content = comment.Content;
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_comments, parent, false);
                return new CommentViewHolder(itemView);
            }

            public void AppendItems(List<Comment> items)
            {
                var count = Items.Count;
                Items.AddRange(items);
                NotifyItemRangeInserted(count, items.Count);
            }

            public void AppendItem(Comment item)
            {
                Items.Add(item);
                NotifyItemInserted(Items.Count - 1);
            }

            public void RemoveItem(Comment item)
            {
                var position = Items.FindIndex(c => c.Id == item.Id);
                if (position >= 0)
                {
                    Items.RemoveAt(position);
                    NotifyItemRemoved(position);
                }
            }

            public void EditItem(Comment item)
            {
                var position = Items.FindIndex(c => c.Id == item.Id);
                if (position >= 0)
                {
                    Items[position].Content = item.Content;
                    NotifyItemChanged(position);
                }
            }

            public Comment GetItemAtPosition(int position)
            {
                return Items[position];
            }

            public Comment GetSelectedItem()
            {
                return Items[SelectedPosition];
            }
        }

        class CommentViewHolder : RecyclerView.ViewHolder
        {

            public string Username { set => usernameTextView.Text = value; }
            public string Date { set => dateTextView.Text = value; }
            public string Content { set => contentTextView.Text = value; }

            readonly AppCompatTextView usernameTextView;
            readonly AppCompatTextView dateTextView;
            readonly AppCompatTextView contentTextView;

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