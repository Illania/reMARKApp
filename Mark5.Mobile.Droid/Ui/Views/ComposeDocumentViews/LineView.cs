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
        readonly Line fakeLine;

        public bool LineSelectedIsAmbiguous => GetLine().Guid == Guid.Empty;

        public LineView(Context context)
            : base(context)
        {
            defaultOutgoingLine = ServerConfig.SystemSettings.DocumentsModuleInfo.DefaultOutgoingLine;
            availableOutgoingLines = ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines.ToList();

            SetPadding(DistanceNormal + DistanceSmall, DistanceNormal + DistanceSmall, DistanceSmall, DistanceNormal + DistanceSmall);

            var titleTextView = new AppCompatTextView(context)
            {
                LayoutParameters = new LayoutParams(DistanceVeryLarge, ViewGroup.LayoutParams.WrapContent),
            };
            titleTextView.SetTextAppearanceCompat(context, Resource.Style.fontPrimaryLight);
            titleTextView.SetText(Resource.String.line);
            AddView(titleTextView);

            fakeLine = new Line
            {
                Name = Resources.GetString(Resource.String.select_a_line),
                Guid = Guid.Empty,
            };

            availableOutgoingLines.Add(fakeLine);

            lineSpinner = new AppCompatSpinner(context);
            var spinnerLayoutParams = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent) { Weight = 1 };
            lineSpinner.LayoutParameters = spinnerLayoutParams;

            var adapter = new CustomAdapter(context, Android.Resource.Layout.SimpleSpinnerItem, availableOutgoingLines);
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

            SetLine(LineUtilities.GetLineForCreationModeFlag(DocumentCreationModeFlag, PreviousDocument, PlatformConfig.Preferences.AlwaysUseDefaultLine));
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
            readonly public List<Line> lines;

            public CustomAdapter(Context context, int textViewResourceId, List<Line> lines)
                : base(context, textViewResourceId, lines)
            {
                this.lines = lines;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                var v = base.GetView(position, convertView, parent);

                if (v is TextView textView)
                {
                    textView.Text = lines[position]?.Name;

                    if (lines[position].Guid == Guid.Empty)
                        textView.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.lightgray)));
                    else
                        textView.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.black)));
                }

                return v;
            }

            public override View GetDropDownView(int position, View convertView, ViewGroup parent)
            {
                View v = null;

                if (lines[position].Guid == Guid.Empty)
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