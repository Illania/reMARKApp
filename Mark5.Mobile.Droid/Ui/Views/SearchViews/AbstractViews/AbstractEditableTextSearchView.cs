//
// Project: Mark5.Mobile.Droid
// File: AbstractEditTextView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Android.Content;
using Android.Graphics;
using Android.Support.Transitions;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public abstract class AbstractEditableTextSearchView<T> : AbstractSearchView<T>
    {
        readonly protected AppCompatTextView TopTextView;
        readonly protected AppCompatTextView BottomTextView;

        readonly protected AppCompatEditText BottomEditText;
        readonly protected LinearLayoutCompat containerLayout;
        readonly protected LinearLayoutCompat cancelIconLayout;

        readonly string emptyText;

        protected AbstractEditableTextSearchView(Context context, int topTextResId, LinearLayoutCompat containerLayout) : base(context)
        {
            this.containerLayout = containerLayout;
            emptyText = context.GetString(Resource.String.search_editable_empty);

            Orientation = Horizontal;
            SetBackgroundColor(BackgroundColorNormalState);

            Clickable = true;
            Click += (sender, e) => PrepareViewsExpansion();

            var leftLayout = new LinearLayoutCompat(context)
            {
                Orientation = Vertical,
                LayoutParameters = new LayoutParams(0, ViewGroup.LayoutParams.WrapContent)
                {
                    Weight = 1.0f
                }
            };

            TopTextView = new AppCompatTextView(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Gravity = GravityFlags.CenterHorizontal
            };
            TopTextView.Text = context.GetString(topTextResId);
            TopTextView.SetTextAppearanceCompat(context, TextStyleTopLineResourceId);
            leftLayout.AddView(TopTextView);

            BottomEditText = LayoutInflater.From(context).Inflate(Resource.Layout.search_edit_text_layout, null).FindViewById<AppCompatEditText>(Resource.Id.search_edit_text);
            BottomEditText.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            BottomEditText.SetTextAppearanceCompat(context, TextStyleBottomLineResourceId);
            BottomEditText.SetBackgroundColor(Color.Transparent);
            BottomEditText.SetPadding(0, 0, 0, 0);
            BottomEditText.Hint = context.GetString(Resource.String.search_editable_empty);
            BottomEditText.SetHintTextColor(ViewUtilities.GetColorStateList(context, Resource.Drawable.search_edit_text_selector));
            BottomEditText.FocusChange += BottomEditText_FocusChange;
            BottomEditText.Visibility = ViewStates.Gone;

            leftLayout.AddView(BottomEditText);

            BottomTextView = new AppCompatTextView(context);
            BottomTextView.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            BottomTextView.Gravity = GravityFlags.CenterHorizontal;
            BottomTextView.Text = emptyText;
            BottomTextView.SetTextAppearanceCompat(context, TextStyleBottomLineResourceId);
            BottomTextView.Visibility = ViewStates.Visible;
            BottomTextView.Ellipsize = TextUtils.TruncateAt.End;
            BottomTextView.SetLines(1);
            leftLayout.AddView(BottomTextView);

            cancelIconLayout = new LinearLayoutCompat(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
                {
                    Gravity = (int)GravityFlags.CenterVertical,
                }
            };

            var cancelIconSize = ConversionUtils.ConvertDpToPixels(20f);
            var cancelIconView = new AppCompatImageView(context)
            {
                LayoutParameters = new LayoutParams(cancelIconSize, cancelIconSize)
            };
            cancelIconView.SetImageResource(Resource.Drawable.cross);
            cancelIconView.SetColorFilter(Color.White);
            cancelIconLayout.AddView(cancelIconView);
            cancelIconLayout.Clickable = true;
            cancelIconLayout.Click += CancelIconLayout_Click;
            cancelIconLayout.Visibility = ViewStates.Gone;

            AddView(leftLayout);
            AddView(cancelIconLayout);
        }

        //TODO 
        // - What to do with the back button?

        void BottomEditText_FocusChange(object sender, FocusChangeEventArgs e)
        {
            if (!e.HasFocus)
            {
                ((InputMethodManager)Context.GetSystemService(Context.InputMethodService)).HideSoftInputFromWindow((sender as View).WindowToken, HideSoftInputFlags.None);
            }
            else
            {
                PrepareViewsExpansion();
            }
        }

        void CancelIconLayout_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < containerLayout.ChildCount; i++)
            {
                var view = containerLayout.GetChildAt(i) as AbstractEditableTextSearchView<T>;
                if (view != null)
                {
                    view.Visibility = ViewStates.Visible;
                }
            }

            Collapse();
        }

        void PrepareViewsExpansion()
        {
            BottomEditText.Visibility = ViewStates.Visible;
            BottomTextView.Visibility = ViewStates.Gone;

            if (BottomTextView.Text != emptyText)
            {
                BottomEditText.Text = BottomTextView.Text;
            }

            for (int i = 0; i < containerLayout.ChildCount; i++)
            {
                var view = containerLayout.GetChildAt(i) as AbstractEditableTextSearchView<T>;
                if (view != null)
                {
                    if (view == this)

                    {
                        view.Expand();
                    }
                    else
                    {
                        view.Visibility = ViewStates.Gone;
                    }
                }
            }
        }

        public void Expand()
        {
            BottomEditText.RequestFocus();
            BottomEditText.SetSelection(BottomEditText.Text.Length);

            ((InputMethodManager)Context.GetSystemService(Context.InputMethodService)).ShowSoftInput(BottomEditText, ShowFlags.Implicit);

            cancelIconLayout.Visibility = ViewStates.Visible;
        }

        public void Collapse()
        {
            cancelIconLayout.Visibility = ViewStates.Gone;

            BottomEditText.Visibility = ViewStates.Gone;
            BottomTextView.Visibility = ViewStates.Visible;

            BottomTextView.Text = string.IsNullOrWhiteSpace(BottomEditText.Text) ? emptyText : BottomEditText.Text;

            BottomEditText.ClearFocus();
        }
    }
}