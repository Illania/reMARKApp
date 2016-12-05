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
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.ComposeDocumentViews
{
    public class LineView : ComposeDocumentView
    {
        public event EventHandler Edited = delegate { };

        readonly AppCompatSpinner lineSpinner;
        readonly AppCompatImageView lineAmbiguityIndicator;
        readonly List<LineInView> availableOutgoingLinesInView;
        readonly Line defaultOutgoingLine;

        bool selectionChangedProgrammatically;

        public bool LineSelectedIsAmbiguous
        {
            get { return lineAmbiguityIndicator.Visibility == ViewStates.Visible; }
        }

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

            lineSpinner = new AdaptedSpinner(context);
            var spinnerLayoutParams = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            spinnerLayoutParams.Weight = 1;
            lineSpinner.LayoutParameters = spinnerLayoutParams;
            var adapter = new ArrayAdapter(context, Android.Resource.Layout.SimpleSpinnerItem, availableOutgoingLinesInView);
            adapter.SetDropDownViewResource(Resource.Layout.support_simple_spinner_dropdown_item);
            lineSpinner.Adapter = adapter;
            lineSpinner.ItemSelected += LineSpinner_ItemSelected;
            AddView(lineSpinner);

            lineAmbiguityIndicator = new AppCompatImageView(context);
            var layoutParams = new LayoutParams(ConversionUtils.ConvertDpToPixels(18), ConversionUtils.ConvertDpToPixels(18));
            layoutParams.Gravity = (int)GravityFlags.CenterVertical;
            lineAmbiguityIndicator.LayoutParameters = layoutParams;
            lineAmbiguityIndicator.SetImageResource(Resource.Drawable.error);
            lineAmbiguityIndicator.SetColorFilter(Android.Graphics.Color.Red);
            lineAmbiguityIndicator.Visibility = ViewStates.Gone;
            AddView(lineAmbiguityIndicator);
        }

        void LineSpinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            if (selectionChangedProgrammatically) //Otherwise there is no way of knowing if event was fired due to the user changing the selection
            {
                selectionChangedProgrammatically = false;
            }
            else
            {
                lineAmbiguityIndicator.Visibility = ViewStates.Gone;
            }

            Edited(this, EventArgs.Empty);
        }

        #region Public methods

        public override Task RefreshView()
        {
            if (State != null)
            {
                RestoreState();
                State = null;
                return Task.CompletedTask;
            }

            if (CreationModeFlag == DocumentCreationModeFlag.New)
            {
                SetLine(defaultOutgoingLine);
                return Task.CompletedTask;
            }

            if (CreationModeFlag == DocumentCreationModeFlag.None)
            {
                return Task.CompletedTask;
            }

            if (CreationModeFlag == DocumentCreationModeFlag.Edit)
            {
                SetLine(PreviousDocument.Lines.First());
            }

            if (availableOutgoingLinesInView.Count == 1)
            {
                SetLine(availableOutgoingLinesInView.First().Line);
                return Task.CompletedTask;
            }

            var previousDocumentLines = PreviousDocument.Lines;
            if (previousDocumentLines.Contains(defaultOutgoingLine))
            {
                SetLine(defaultOutgoingLine);
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
                    SetLine(availableOutgoingLinesInView.First().Line);
                    lineAmbiguityIndicator.Visibility = ViewStates.Visible;
                }
            }

            return Task.CompletedTask;

        }

        public override Task UpdateDocument()
        {
            Document.Lines.Add(GetLine());
            return Task.CompletedTask;
        }

        public void SetLineFromGuid(Guid lineGuid)
        {
            var index = availableOutgoingLinesInView.FindIndex(l => l.Line.Guid == lineGuid);
            if (index > 0)
            {
                lineSpinner.SetSelection(index);
            }
            selectionChangedProgrammatically = true;
        }

        #endregion

        #region Utilities

        void SetLine(Line line)
        {
            SetLineFromGuid(line.Guid);
        }

        Line GetLine()
        {
            return availableOutgoingLinesInView[lineSpinner.SelectedItemPosition].Line;
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

        #region Spinner

        class AdaptedSpinner : AppCompatSpinner
        {
            public AdaptedSpinner(Context context) : base(context)
            {
            }

            public override void SetSelection(int position)
            {
                //The base spinner does not fire the onItemSelected event when selecting the same item
                if (position == SelectedItemPosition)
                {
                    OnItemSelectedListener.OnItemSelected(this, SelectedView, position, SelectedItemId);
                    return;
                }

                base.SetSelection(position);
            }
        }

        #endregion

        #region State related

        void RestoreState()
        {
            var lineViewState = State as LineViewState;
            SetLine(lineViewState.SelectedLine);
            lineAmbiguityIndicator.Visibility = lineViewState.LineAmbiguityIndicatorVisibility;
        }

        public override IComposeDocumentViewState ReturnState()
        {
            return new LineViewState
            {
                SelectedLine = GetLine(),
                LineAmbiguityIndicatorVisibility = lineAmbiguityIndicator.Visibility,
            };
        }

        class LineViewState : IComposeDocumentViewState
        {
            public Line SelectedLine { get; set; }
            public ViewStates LineAmbiguityIndicatorVisibility { get; set; }
        }

        #endregion
    }
}
