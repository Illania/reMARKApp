//
// Project: ${Project}
// File: AbstractTabButtonsView.cs
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
    public class AbstractButtonsView : AbstractSearchView
    {
        public int value;

        public AbstractButtonsView(Context context) : base(context)
        {
            Orientation = Horizontal;
            SetPadding(0, 0, 0, 0);

            var lp = new LayoutParams(0, ViewGroup.LayoutParams.MatchParent);
            lp.Weight = 1.0f;

            DividerDrawable = ContextCompat.GetDrawable(Context, Resource.Drawable.search_divider_vertical);
            ShowDividers = ShowDividerMiddle;

            var button1 = new StyledButton(context);
            button1.Text = "INBOX";

            var button2 = new StyledButton(context);
            button2.Text = "OUTBOX";

            var button3 = new StyledButton(context);
            button3.Text = "DRAFT";

            AddView(button1, lp);
            AddView(button2, lp);
            AddView(button3, lp);
        }

        public class StyledButton : AppCompatButton
        {
            int buttonTextStyleNormalResourceId = Resource.Style.searchViewButtonNormal;
            int buttonTextStyleSelectedResourceId = Resource.Style.searchViewButtonSelected;

            bool selected;
            readonly Context context;

            public StyledButton(Context context)
                : base(context)
            {
                this.context = context;
                this.SetTextAppearanceCompat(context, buttonTextStyleNormalResourceId);

                //var typedArray = Context.ObtainStyledAttributes(new int[] { Resource.Attribute.selectableItemBackground });
                //SetBackgroundResource(typedArray.GetResourceId(0, 0));
                //typedArray.Recycle(); //TODO add ripple effect

                SetBackgroundColor(BackgroundColorNormalState);

                Click += StyledButton_Click;
            }

            void StyledButton_Click(object sender, EventArgs e)
            {
                selected = !selected;

                SetBackgroundColor(selected ? BackgroundColorSelectedState : BackgroundColorNormalState);
                this.SetTextAppearanceCompat(context, selected ? buttonTextStyleSelectedResourceId : buttonTextStyleNormalResourceId);
            }
        }
    }

}