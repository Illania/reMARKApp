//
// Project: Mark5.Mobile.Droid
// File: PriorityView.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
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
        readonly AppCompatSpinner prioritySpinner;
        readonly List<Priority> priorities = new List<Priority> { Priority.Ignore, Priority.Low, Priority.Normal, Priority.System, Priority.Urgent };

        public PriorityView(Context context)
            : base(context)
        {
            Orientation = Horizontal;
            SetPadding(DistanceNormal, DistanceNormal, DistanceNormal, DistanceNormal);

            var titleTextView = new AppCompatTextView(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent),
            };
            titleTextView.SetTextAppearanceCompat(context, Resource.Style.fontPrimaryBold);
            titleTextView.SetText(Resource.String.priority);
            AddView(titleTextView);

            prioritySpinner = new AppCompatSpinner(context);
            var spinnerLayoutParams = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            spinnerLayoutParams.Weight = 1;
            prioritySpinner.LayoutParameters = spinnerLayoutParams;
            var adapter = new ArrayAdapter(context, Android.Resource.Layout.SimpleSpinnerItem, priorities);
            adapter.SetDropDownViewResource(Resource.Layout.support_simple_spinner_dropdown_item);
            prioritySpinner.Adapter = adapter;
            prioritySpinner.SetSelection(priorities.IndexOf(Priority.Normal));
            AddView(prioritySpinner);
        }

        public override void RefreshView()
        {
            throw new NotImplementedException();
        }

        public override void UpdateDocument()
        {
            throw new NotImplementedException();
        }
    }
}
