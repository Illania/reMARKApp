//
// Project: Mark5.Mobile.Droid
// File: AbstractButtonsSearchView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Android.Content;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Droid.Ui.Common;


namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public abstract class AbstractButtonsSearchView<T> : AbstractSearchView<T>
    {

        protected AbstractButtonsSearchView(Context context) : base(context)
        {
            Orientation = Horizontal;
            SetPadding(0, 0, 0, 0);

            DividerDrawable = ContextCompat.GetDrawable(Context, Resource.Drawable.search_divider_vertical);
            ShowDividers = ShowDividerMiddle;
        }

        protected void AddButtons(params CustomButton[] buttons)
        {
            foreach (var button in buttons)
            {
                AddView(button, new LayoutParams(0, ViewGroup.LayoutParams.MatchParent, 1));
            }
        }

        public class CustomButton : AppCompatButton
        {
            int buttonTextStyleNormalResourceId = Resource.Style.searchViewButtonNormal;
            int buttonTextStyleSelectedResourceId = Resource.Style.searchViewButtonSelected;

            readonly Context context;
            readonly Func<CustomButton, bool> clickedAction; //Returns true if the button should change state

            public CustomButton(Context context, int stringResourceId, Func<CustomButton, bool> clickedAction = null)
                : base(context)
            {
                this.context = context;
                this.clickedAction = clickedAction;

                Text = context.GetString(stringResourceId);

                Background = ContextCompat.GetDrawable(Context, Resource.Drawable.search_button_background);

                Click += CustomButton_Click;

                UpdateStyle();
            }

            void CustomButton_Click(object sender, EventArgs e)
            {
                if (clickedAction == null || clickedAction(this))
                {
                    Selected = !Selected;
                }
                UpdateStyle();
            }

            void UpdateStyle()
            {
                this.SetTextAppearanceCompat(context, Selected ? buttonTextStyleSelectedResourceId : buttonTextStyleNormalResourceId);
            }

            public void UpdateSelectedState(bool selected)
            {
                Selected = selected;
                UpdateStyle();
            }

        }
    }

}