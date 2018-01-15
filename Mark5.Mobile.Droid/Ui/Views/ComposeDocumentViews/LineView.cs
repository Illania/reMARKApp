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
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Views.ComposeDocumentViews
{
    public class LineView : ComposeDocumentView
    {
        public event EventHandler Edited = delegate { };

        readonly AppCompatSpinner lineSpinner;
        readonly Line defaultOutgoingLine;
        readonly List<Line> availableOutgoingLines;

        public bool LineSelectedIsAmbiguous => GetLine().Guid == Guid.Empty;

        public LineView(Context context)
            : base(context)
        {
            defaultOutgoingLine = ServerConfig.SystemSettings.DocumentsModuleInfo.DefaultOutgoingLine;
            availableOutgoingLines = ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines;

            SetPadding(DistanceNormal + DistanceSmall, DistanceNormal + DistanceSmall, DistanceSmall, DistanceNormal + DistanceSmall);

            var titleTextView = new AppCompatTextView(context)
            {
                LayoutParameters = new LayoutParams(DistanceVeryLarge, ViewGroup.LayoutParams.WrapContent),
            };
            titleTextView.SetTextAppearanceCompat(context, Resource.Style.fontPrimaryLight);
            titleTextView.SetText(Resource.String.line);
            AddView(titleTextView);

            availableOutgoingLines.Add(new Line
            {
                Name = Resources.GetString(Resource.String.select_a_line),
                Guid = Guid.Empty,
            });

            lineSpinner = new AppCompatSpinner(context);
            var spinnerLayoutParams = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent) { Weight = 1 };
            lineSpinner.LayoutParameters = spinnerLayoutParams;

            var adapter = new CustomAdapter(context, Android.Resource.Layout.SimpleSpinnerItem, availableOutgoingLines, availableOutgoingLines.Count - 1);
            adapter.SetDropDownViewResource(Resource.Layout.support_simple_spinner_dropdown_item);
            lineSpinner.Adapter = adapter;
            lineSpinner.ItemSelected += LineSpinner_ItemSelected;
            AddView(lineSpinner);
        }

        void LineSpinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e) => Edited(this, EventArgs.Empty);

        #region Public methods

        public override Task RefreshView()
        {
            if (RestoreWorkingCopy)
            {
                SetLine(Document.Lines.FirstOrDefault());
                return Task.CompletedTask;
            }

            if (DocumentCreationModeFlag == DocumentCreationModeFlag.New)
                SetLine(defaultOutgoingLine);

            if (DocumentCreationModeFlag == DocumentCreationModeFlag.Edit)
                SetLine(PreviousDocument.Lines.FirstOrDefault());

            if (DocumentCreationModeFlag == DocumentCreationModeFlag.Reply ||
                DocumentCreationModeFlag == DocumentCreationModeFlag.ReplyAll ||
                DocumentCreationModeFlag == DocumentCreationModeFlag.Forward)
            {
                if (PlatformConfig.Preferences.AlwaysUseDefaultLine && defaultOutgoingLine != null)
                {
                    SetLine(defaultOutgoingLine);
                    return Task.CompletedTask;
                }

                if (availableOutgoingLines.Count == 1)
                {
                    SetLine(availableOutgoingLines.FirstOrDefault());
                    return Task.CompletedTask;
                }

                if (PreviousDocument.Lines.FirstOrDefault(l => l.Guid == defaultOutgoingLine?.Guid) != null)
                    SetLine(defaultOutgoingLine);
                else
                {
                    var intersection = PreviousDocument.Lines.Intersect(availableOutgoingLines, LambdaEqualityComparer<Line>.Create(l => l.Guid)).ToArray();
                    if (intersection.Length == 1)
                        SetLine(intersection.FirstOrDefault());
                    else
                        SetLine(null);
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

        public void SetLine(Line line)
        {
            if (line == null)
                SetLine(Guid.Empty);
            else
                SetLine(line.Guid);
        }

        public void SetLine(Guid lineGuid)
        {
            var index = availableOutgoingLines.FindIndex(l => l.Guid == lineGuid);
            if (index > 0)
                lineSpinner.SetSelection(index);
        }

        public Line GetLine() => availableOutgoingLines[lineSpinner.SelectedItemPosition];

        #endregion

        #region CustomAdapter

        public class CustomAdapter : ArrayAdapter
        {
            readonly int hiddenItemPosition;
            readonly public List<Line> lines;

            public CustomAdapter(Context context, int textViewResourceId, List<Line> lines, int hiddenItemPosition)
                : base(context, textViewResourceId, lines)
            {
                this.lines = lines;
                this.hiddenItemPosition = hiddenItemPosition;
            }

            //TODO there is a problem here, and we have "select mailbox" among the choices

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                var v = base.GetView(position, convertView, parent);

                if (v is TextView textView)
                {
                    textView.Text = lines[position]?.Name;

                    if (position == hiddenItemPosition)
                        textView.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.lightgray)));
                    else
                        textView.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.black)));
                }

                return v;
            }

            public override View GetDropDownView(int position, View convertView, ViewGroup parent)
            {
                View v = null;

                if (position == hiddenItemPosition)
                {
                    var tv = new TextView(Context);
                    tv.SetHeight(0);
                    v = tv;
                }
                else
                {
                    v = base.GetDropDownView(position, null, parent);

                    if (v is TextView textView)
                        textView.Text = lines[position]?.Name;
                }

                parent.VerticalScrollBarEnabled = false;
                return v;
            }
        }

        #endregion
    }
}