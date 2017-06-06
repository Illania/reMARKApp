//
// Project: Mark5.Mobile.Droid
// File: LineView.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.ComposeDocumentViews
{
    public class LineView : ComposeDocumentView
    {
        public event EventHandler Edited = delegate { };

        readonly AppCompatSpinner lineSpinner;
        readonly List<LineInView> availableOutgoingLinesInView;
        readonly Line defaultOutgoingLine;
        readonly Line ambiguousFakeLine;
        readonly Guid ambiguousFakeLineGuid = Guid.Parse("175012b3-abee-48ff-9973-2bd84f67e5fd");

        public bool LineSelectedIsAmbiguous
        {
            get { return GetLine().Guid == ambiguousFakeLineGuid; }
        }

        public LineView(Context context)
            : base(context)
        {
            SetPadding(DistanceNormal + DistanceSmall, DistanceNormal + DistanceSmall, DistanceNormal + DistanceSmall, DistanceNormal + DistanceSmall);

            var titleTextView = new AppCompatTextView(context)
            {
                LayoutParameters = new LayoutParams(DistanceVeryLarge, ViewGroup.LayoutParams.WrapContent),
            };
            titleTextView.SetTextAppearanceCompat(context, Resource.Style.fontPrimaryLight);
            titleTextView.SetText(Resource.String.line);
            AddView(titleTextView);

            defaultOutgoingLine = ServerConfig.SystemSettings.DocumentsModuleInfo.DefaultOutgoingLine;
            availableOutgoingLinesInView = ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines.Select(l => new LineInView(l)).ToList();

            ambiguousFakeLine = new Line
            {
                Name = Resources.GetString(Resource.String.select_a_line),
                Guid = ambiguousFakeLineGuid,
            };

            availableOutgoingLinesInView.Add(new LineInView(ambiguousFakeLine));

            lineSpinner = new AppCompatSpinner(context);
            var spinnerLayoutParams = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            spinnerLayoutParams.Weight = 1;
            lineSpinner.LayoutParameters = spinnerLayoutParams;

            var adapter = new CustomAdapter(context, Android.Resource.Layout.SimpleSpinnerItem, availableOutgoingLinesInView, availableOutgoingLinesInView.Count - 1);
            adapter.SetDropDownViewResource(Resource.Layout.support_simple_spinner_dropdown_item);
            lineSpinner.Adapter = adapter;
            lineSpinner.ItemSelected += LineSpinner_ItemSelected;
            AddView(lineSpinner);
        }

        void LineSpinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
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
                if (defaultOutgoingLine != null)
                {
                    SetLine(defaultOutgoingLine);
                }
                else
                {
                    SetSelectLine();
                }
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
                var intersection = previousDocumentLines.Intersect(availableOutgoingLinesInView.Select(l => l.Line), LambdaEqualityComparer<Line>.Create(l => l.Guid)).ToList();
                if (intersection.Count() == 1)
                {
                    SetLine(intersection.First());
                }
                else
                {
                    SetSelectLine();
                }
            }

            return Task.CompletedTask;
        }

        public override Task UpdateDocument()
        {
            Document.Lines.Clear();
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
        }

        public Line GetLine()
        {
            return availableOutgoingLinesInView[lineSpinner.SelectedItemPosition].Line;
        }

        #endregion

        #region Utilities

        void SetLine(Line line)
        {
            SetLineFromGuid(line.Guid);
        }

        void SetSelectLine()
        {
            SetLineFromGuid(ambiguousFakeLineGuid);
        }

        class LineInView
        {
            readonly Line line;

            public Line Line
            {
                get { return line; }
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

        #region CustomAdapter

        public class CustomAdapter : ArrayAdapter
        {
            readonly int hiddenItemIndex;

            public CustomAdapter(Context context, int textViewResourceId, IList objects, int hiddenItemIndex)
                : base(context, textViewResourceId, objects)
            {
                this.hiddenItemIndex = hiddenItemIndex;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                var v = base.GetView(position, convertView, parent);

                var textView = v as TextView;
                if (textView != null)
                {
                    if (position == hiddenItemIndex)
                    {
                        textView.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.lightgray)));
                    }
                    else
                    {
                        textView.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.black)));
                    }
                }

                return v;
            }

            override public View GetDropDownView(int position, View convertView, ViewGroup parent)
            {
                View v = null;
                if (position == hiddenItemIndex)
                {
                    var tv = new TextView(Context);
                    tv.SetHeight(0);
                    v = tv;
                }
                else
                {
                    v = base.GetDropDownView(position, null, parent);
                }

                parent.VerticalScrollBarEnabled = false;

                return v;
            }
        }

        #endregion

        #region State related

        void RestoreState()
        {
            var lineViewState = State as LineViewState;
            SetLine(lineViewState.SelectedLine);
        }

        public override IComposeDocumentViewState ReturnState()
        {
            return new LineViewState
            {
                SelectedLine = GetLine(),
            };
        }

        class LineViewState : IComposeDocumentViewState
        {
            public Line SelectedLine { get; set; }
        }

        #endregion
    }
}