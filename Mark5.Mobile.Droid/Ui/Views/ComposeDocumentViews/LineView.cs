//
// Project: Mark5.Mobile.Droid
// File: LineView.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Views.ComposeDocumentViews
{
    public class LineView : ComposeDocumentView
    {
        readonly AppCompatSpinner lineSpinner;
        readonly List<LineInView> availableOutgoingLines;

        public LineView(Context context)
            : base(context)
        {
            Orientation = Horizontal;
            SetPadding(DistanceNormal, DistanceNormal, DistanceNormal, DistanceNormal);

            var titleTextView = new AppCompatTextView(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent),
            };
            titleTextView.SetTextAppearanceCompat(context, Resource.Style.fontPrimaryBold);
            titleTextView.SetText(Resource.String.line);
            AddView(titleTextView);

            availableOutgoingLines = ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines.Select(l => new LineInView(l)).ToList();

            lineSpinner = new AppCompatSpinner(context);
            var spinnerLayoutParams = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            spinnerLayoutParams.Weight = 1;
            lineSpinner.LayoutParameters = spinnerLayoutParams;
            var adapter = new ArrayAdapter(context, Android.Resource.Layout.SimpleSpinnerItem, availableOutgoingLines);
            adapter.SetDropDownViewResource(Resource.Layout.support_simple_spinner_dropdown_item);
            lineSpinner.Adapter = adapter;
            AddView(lineSpinner);
        }

        class LineInView
        {
            readonly Line line;

            public LineInView(Line line)
            {
                this.line = line;
            }

            public override string ToString()
            {
                return line.Name;
            }
        }

        public override void RefreshView()
        {
            throw new NotImplementedException();
        }
    }
}
