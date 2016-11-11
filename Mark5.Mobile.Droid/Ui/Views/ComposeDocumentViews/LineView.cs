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
using System.Threading.Tasks;
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
        readonly List<LineInView> availableOutgoingLinesInView;
        readonly Line defaultLine; //TODO remember to set;
        Line defaultOutgoingLine;

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

            defaultOutgoingLine = ServerConfig.SystemSettings.DocumentsModuleInfo.DefaultOutgoingLine;
            availableOutgoingLinesInView = ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines.Select(l => new LineInView(l)).ToList();

            lineSpinner = new AppCompatSpinner(context);
            var spinnerLayoutParams = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            spinnerLayoutParams.Weight = 1;
            lineSpinner.LayoutParameters = spinnerLayoutParams;
            var adapter = new ArrayAdapter(context, Android.Resource.Layout.SimpleSpinnerItem, availableOutgoingLinesInView);
            adapter.SetDropDownViewResource(Resource.Layout.support_simple_spinner_dropdown_item);
            lineSpinner.Adapter = adapter;
            AddView(lineSpinner);

        }

        #region Public methods

        public override async Task RefreshView()
        {
            if (CreationModeFlag == DocumentCreationModeFlag.None || CreationModeFlag == DocumentCreationModeFlag.Edit)
            {
                return;
            }

            if (availableOutgoingLinesInView.Count == 1)
            {
                SetLine(availableOutgoingLinesInView.First().Line);
                return;
            }

            var previousDocumentLines = PreviousDocument.Lines;
            if (previousDocumentLines.Contains(defaultLine))
            {
                SetLine(defaultLine);
            }
            else
            {
                var intersection = previousDocumentLines.Intersect(availableOutgoingLinesInView.Select(l => l.Line)).ToList();
                if (intersection.Count() == 1)
                {
                    SetLine(intersection.First());
                }
                else
                {
                    //TODO need to notify the user somehow (not easy to put empty text)
                }
            }
        }

        public override Task UpdateDocument()
        {
            throw new NotImplementedException();
        }

        public void SetLineFromGuid(Guid lineGuid)
        {
            var index = availableOutgoingLinesInView.FindIndex(l => l.Line.Guid == lineGuid);
            if (index > 0)
            {
                lineSpinner.SetSelection(index);
            }
        }

        #endregion

        #region Utilities

        void SetLine(Line line)
        {
            SetLineFromGuid(line.Guid);
        }

        class LineInView
        {
            readonly Line line;

            public Line Line
            {
                get
                {
                    return line;
                }
            }
            public LineInView(Line line)
            {
                this.line = line;
            }

            public override string ToString()
            {
                return Line.Name;
            }
        }

        #endregion
    }
}
