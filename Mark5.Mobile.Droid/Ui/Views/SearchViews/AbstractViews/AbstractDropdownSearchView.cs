//
// Project: Mark5.Mobile.Droid
// File: AbstractDropdownSearchView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.Collections.Generic;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public abstract class AbstractDropdownSearchView<T> : AbstractSearchView<T>
    {
        readonly protected MultiSelectSpinner Spinner;
        readonly protected AppCompatTextView TextView;

        protected AbstractDropdownSearchView(Context context) : base(context)
        {
            Orientation = Vertical;
            SetBackgroundColor(BackgroundColorNormalState);

            TextView = new AppCompatTextView(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Gravity = GravityFlags.Center
            };
            TextView.Text = "TEST";
            TextView.SetTextAppearanceCompat(context, TextStyleTopLineResourceId);
            AddView(TextView);

            Spinner = new MultiSelectSpinner(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
                {
                    Gravity = (int)GravityFlags.CenterHorizontal
                }
            };

            AddView(Spinner);
        }

        protected class MultiSelectSpinner : AppCompatSpinner
        {
            readonly ArrayAdapter adapter;
            public MultiSelectSpinner(Context context) : base(context)
            {
                var initialText = "BlaBla";
                adapter = new ArrayAdapter(context, Resource.Layout.search_spinner_item_dropdown, new List<string> { initialText });
                adapter.SetDropDownViewResource(Resource.Layout.support_simple_spinner_dropdown_item);
                SetAdapter(adapter);
            }

            public override bool PerformClick()
            {
                var choices = new List<string> { "val1", "val2", "val3" };
                Dialogs.ShowMultiSelectDialog(Context, Resource.String.search, choices, HandleAction);
                return true;
            }

            void HandleAction(List<string> selectedItems)
            {
                adapter.Clear();
                adapter.Add($"{selectedItems.Count} selected");
            }
        }
    }
}