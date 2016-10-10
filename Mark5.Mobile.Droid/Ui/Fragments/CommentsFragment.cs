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
using Android.Support.V7.Widget;
using Android.Text.Format;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class CommentsFragment : RetainableStateFragment
    {
        List<Comment> Comments;

        RecyclerView recyclerView;
        CommentsListAdapter adapter;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);

            return base.OnCreateView(inflater, container, savedInstanceState);
        }

        public override IRetainableState OnRetainInstanceState()
        {
            throw new NotImplementedException();
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            throw new NotImplementedException();
        }

        public override string GenerateTag()
        {
            throw new NotImplementedException();
        }

        class CommentsFragmentState : IRetainableState
        {
        }

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
                cvh.Username = comment.UserName;

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
