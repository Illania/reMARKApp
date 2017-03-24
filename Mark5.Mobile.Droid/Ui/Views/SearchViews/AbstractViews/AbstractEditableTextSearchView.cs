//
// Project: Mark5.Mobile.Droid
// File: AbstractEditTextView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public abstract class AbstractEditableTextSearchView<T> : AbstractSearchView<T>
    {
        readonly protected AppCompatTextView TopTextView;
        readonly protected AppCompatTextView BottomTextView;

        string EmptyText = "Enter text:";
        bool empty = true;

        protected AbstractEditableTextSearchView(Context context) : base(context)
        {
            Orientation = Vertical;
            SetBackgroundColor(BackgroundColorNormalState);

            TopTextView = new AppCompatTextView(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Gravity = GravityFlags.CenterHorizontal
            };
            TopTextView.Text = "TEST TEXT";
            TopTextView.SetTextAppearanceCompat(context, TextStyleTopLineResourceId);
            AddView(TopTextView);

            BottomTextView = new AppCompatTextView(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Gravity = GravityFlags.CenterHorizontal
            };
            BottomTextView.SetTextAppearanceCompat(context, TextStyleBottomLineResourceId);
            BottomTextView.Click += BottomTextView_Click;
            AddView(BottomTextView);

            ResetTextView();
        }

        void ResetTextView()
        {
            BottomTextView.Text = EmptyText;
            empty = true;
        }

        void BottomTextView_Click(object sender, EventArgs e)
        {
            Dialogs.ShowEditTextDialog(Context, Resource.String.send, empty ? string.Empty : BottomTextView.Text, HandleChangeText);
        }

        void HandleChangeText(string newText)
        {
            if (string.IsNullOrWhiteSpace(newText))
            {
                ResetTextView();
            }
            else
            {
                BottomTextView.Text = newText;
                empty = false;
            }
        }
    }
}