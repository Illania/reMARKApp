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
        LayoutParams buttonsLayoutParams;

        protected AbstractButtonsSearchView(Context context) : base(context)
        {
            Orientation = Horizontal;
            SetPadding(0, 0, 0, 0);

            buttonsLayoutParams = new LayoutParams(0, ViewGroup.LayoutParams.MatchParent, 1);

            DividerDrawable = ContextCompat.GetDrawable(Context, Resource.Drawable.search_divider_vertical);
            ShowDividers = ShowDividerMiddle;
        }

        protected void AddButtons(params StyledButton[] buttons)
        {
            foreach (var button in buttons)
            {
                AddView(button, buttonsLayoutParams);
            }
        }

        public class StyledButton : AppCompatButton
        {
            int buttonTextStyleNormalResourceId = Resource.Style.searchViewButtonNormal;
            int buttonTextStyleSelectedResourceId = Resource.Style.searchViewButtonSelected;

            readonly Context context;
            readonly Func<StyledButton, bool> clickedAction; //Returns true if the button should change state

            public StyledButton(Context context, int stringResourceId, Func<StyledButton, bool> clickedAction = null)
                : base(context)
            {
                this.context = context;
                this.clickedAction = clickedAction;

                Text = context.GetString(stringResourceId);

                Background = ContextCompat.GetDrawable(Context, Resource.Drawable.search_button_background);

                Click += StyledButton_Click;

                UpdateStyle();
            }

            void StyledButton_Click(object sender, EventArgs e)
            {
                if (clickedAction == null || clickedAction(this))
                {
                    Selected = !Selected;
                }
                UpdateStyle();
            }

            void UpdateStyle()
            {
                //SetBackgroundColor(Selected ? BackgroundColorSelectedState : BackgroundColorNormalState);
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