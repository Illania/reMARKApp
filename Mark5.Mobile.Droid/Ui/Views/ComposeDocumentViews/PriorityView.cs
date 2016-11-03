//
// Project: Mark5.Mobile.Droid
// File: PriorityView.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Views.ComposeDocumentViews
{
    public class PriorityView : ComposeDocumentView
    {
        protected readonly AppCompatSpinner PrioritySpinner;
        readonly List<Priority> priorities = new List<Priority> { Priority.Ignore, Priority.Low, Priority.Normal, Priority.System, Priority.Urgent };
        public PriorityView(Context context)
            : base(context)
        {
            Orientation = Horizontal;
            SetPadding(DistanceNormal, DistanceSmall, DistanceNormal, DistanceSmall);

            var titleTextView = new AppCompatTextView(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent),
            };
            titleTextView.SetTextAppearanceCompat(context, Resource.Style.fontPrimaryBold);
            titleTextView.Text = "Priority"; //TODO need to use resources
            AddView(titleTextView);

            PrioritySpinner = new AppCompatSpinner(context);
            var spinnerLayoutParams = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            spinnerLayoutParams.Weight = 1;
            PrioritySpinner.LayoutParameters = spinnerLayoutParams;
            var adapter = new ArrayAdapter(context, Android.Resource.Layout.SimpleSpinnerItem, priorities);
            adapter.SetDropDownViewResource(Resource.Layout.support_simple_spinner_dropdown_item);
            PrioritySpinner.Adapter = adapter;
            PrioritySpinner.SetSelection(priorities.IndexOf(Priority.Normal));
            AddView(PrioritySpinner);
        }
    }
}
