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
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public abstract class AbstractDropdownSearchView<T> : AbstractSearchView<T>
    {
        readonly protected MultiSelectSpinner Spinner;
        readonly protected AppCompatTextView TextView;
        readonly protected DocumentSearchCriteriaFragment ParentFragment;

        protected AbstractDropdownSearchView(Context context, int titleResId, int emptyResId, DocumentSearchCriteriaFragment f) : base(context)
        {
            ParentFragment = f;

            Orientation = Vertical;
            SetBackgroundColor(BackgroundColorNormalState);

            TextView = new AppCompatTextView(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Gravity = GravityFlags.Center
            };
            TextView.Text = context.GetString(titleResId);
            TextView.SetTextAppearanceCompat(context, TextStyleTopLineResourceId);
            AddView(TextView);

            Spinner = new MultiSelectSpinner(context, context.GetString(emptyResId), ClickAction)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
                {
                    Gravity = (int)GravityFlags.CenterHorizontal
                }
            };

            AddView(Spinner);
        }

        public void UpdateSpinnerText(int selectedCount)
        {
            if (selectedCount == 0)
            {
                Spinner.Reset();
            }
            else
            {
                Spinner.SetText(Resources.GetQuantityString(Resource.Plurals.search_dropdown_selected, selectedCount, selectedCount));
            }
        }

        protected abstract void ClickAction();

        protected class MultiSelectSpinner : AppCompatSpinner
        {
            readonly ArrayAdapter adapter;
            readonly Action clickAction;
            readonly string initialText;

            public MultiSelectSpinner(Context context, string initialText, Action clickAction) : base(context)
            {
                this.clickAction = clickAction;
                this.initialText = initialText;

                adapter = new ArrayAdapter(context, Resource.Layout.search_spinner_item_dropdown, new List<string> { initialText });
                adapter.SetDropDownViewResource(Resource.Layout.support_simple_spinner_dropdown_item);
                SetAdapter(adapter);
            }

            public override bool PerformClick()
            {
                clickAction();
                return true;
            }

            public void SetText(string text)
            {
                adapter.Clear();
                adapter.Add(text);
            }

            public void Reset()
            {
                SetText(initialText);
            }
        }
    }
}