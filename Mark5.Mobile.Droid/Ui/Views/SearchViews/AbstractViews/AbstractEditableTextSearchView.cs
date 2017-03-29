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
using Android.Support.V7.Widget;
using Android.Views;
using Android.Views.InputMethods;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public abstract class AbstractEditableTextSearchView<T> : AbstractSearchView<T>
    {
        readonly protected AppCompatTextView TopTextView;
        readonly protected AppCompatEditText BottomEditText;
        readonly protected LinearLayoutCompat containerLayout;
        readonly protected LinearLayoutCompat cancelIconLayout;

        protected AbstractEditableTextSearchView(Context context, int topTextResId, LinearLayoutCompat containerLayout) : base(context)
        {
            this.containerLayout = containerLayout;

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
            BottomEditText.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
            {
                Gravity = (int)GravityFlags.CenterHorizontal,
            };
            BottomEditText.SetTextAppearanceCompat(context, TextStyleBottomLineResourceId);
            BottomEditText.SetBackgroundColor(Color.Transparent);
            BottomEditText.SetPadding(0, 0, 0, 0);
            BottomEditText.Hint = context.GetString(Resource.String.search_editable_empty);
            BottomEditText.FocusChange += BottomEditText_FocusChange;

            leftLayout.AddView(BottomEditText);

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
            cancelIconView.SetImageResource(Resource.Drawable.failed); //TODO new icon?
            cancelIconView.SetColorFilter(Color.White);
            cancelIconLayout.AddView(cancelIconView);
            cancelIconLayout.Clickable = true;
            cancelIconLayout.Click += CancelIconLayout_Click;
            cancelIconLayout.Visibility = ViewStates.Gone;

            AddView(leftLayout);
            AddView(cancelIconLayout);
        }

        //TODO Open questions (more or less)
        // - What about ellipsize?
        // - What happens when the user presses back?
        // - What happens when we click on something else in the search?

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
            BottomEditText.ClearFocus();
        }
    }
}