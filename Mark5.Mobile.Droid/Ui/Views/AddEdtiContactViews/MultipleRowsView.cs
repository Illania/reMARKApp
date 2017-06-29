using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public abstract class MultipleRowsView<T> : AddEditContactView where T : class  //TODO rename
    {
        AppCompatEditText titleEditText;
        AppCompatTextView titleTextView;
        AppCompatImageButton addButton;

        bool singleRow;

        List<Row> rows = new List<Row>();

        protected MultipleRowsView(Context context, int titleResourceId, bool singleRow) : base(context)
        {
            this.singleRow = singleRow;

            SetPadding(DistanceLarge, DistanceNormal, DistanceLarge, DistanceNormal);

            titleEditText = new AppCompatEditText(context)
            {
                KeyListener = null,
            };
            titleEditText.SetHint(titleResourceId);

            titleEditText.Focusable = false;
            titleEditText.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            titleEditText.Click += HintEditText_Click;
            var hintEditTextLp = new LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1.0f)
            {
                Gravity = (int)GravityFlags.CenterVertical,
                RightMargin = DistanceNormal,
            };

            TopLayout.AddView(titleEditText, hintEditTextLp);

            titleTextView = new AppCompatTextView(context);
            titleTextView.SetText(titleResourceId);
            var titleTextViewLp = new LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1.0f)
            {
                Gravity = (int)GravityFlags.CenterVertical,
                RightMargin = DistanceNormal,
            };
            titleTextView.Visibility = ViewStates.Gone;
            titleTextView.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            titleTextView.SetPadding(titleEditText.PaddingLeft, titleEditText.PaddingTop, titleEditText.PaddingRight, titleEditText.PaddingBottom);
            TopLayout.AddView(titleTextView, titleTextViewLp);

            addButton = GetButton(context, true);
            addButton.Click += AddButton_Click;
            TopLayout.AddView(addButton);
        }

        void AddButton_Click(object sender, EventArgs e)
        {
            AddRow();
        }

        void HintEditText_Click(object sender, EventArgs e)
        {
            AddRow();
        }

        virtual protected void AddRow(T content = null)
        {
            if (singleRow)
                addButton.Visibility = ViewStates.Gone;

            titleEditText.Visibility = ViewStates.Gone;
            titleTextView.Visibility = ViewStates.Visible;

            var row = GetNewRow(content);
            row.DeleteButton.Click += (sender, e) => RemoveRow(sender as Row);

            rows.Add(row);
            ContentLayout.AddView(row.Layout);
        }

        void RemoveRow(Row row)
        {
            ContentLayout.RemoveView(row.Layout);
            rows.Remove(row);

            if (!rows.Any())
            {
                titleEditText.Visibility = ViewStates.Visible;
                titleTextView.Visibility = ViewStates.Gone;
                addButton.Visibility = ViewStates.Visible;
            }
        }

        abstract protected Row GetNewRow(T content = null);

        #region Utilities

        static public AppCompatImageButton GetButton(Context context, bool plus = true)
        {
            var button = new AppCompatImageButton(context);

            button.SetImageResource(Resource.Drawable.add);
            button.SetColorFilter(plus ? new Color(ContextCompat.GetColor(context, Resource.Color.blue)) : Color.Red);

            var addButtonLp = new LayoutParams(ConversionUtils.ConvertDpToPixels(24), ConversionUtils.ConvertDpToPixels(24))
            {
                LeftMargin = DistanceNormal,
                Gravity = (int)GravityFlags.CenterVertical,
            };
            button.LayoutParameters = addButtonLp;
            return button;
        }

        #endregion

        abstract protected class Row
        {
            public LinearLayoutCompat Layout { get => layout; }
            public AppCompatImageButton DeleteButton { get => deleteButton; }

            protected LinearLayoutCompat layout;
            protected AppCompatImageButton deleteButton;
            protected T content;

            protected Row(Context context, T content)
            {
                this.content = content;

                layout = new LinearLayoutCompat(context)
                {
                    LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                };

                deleteButton = GetButton(context, false);
                Layout.AddView(deleteButton);
            }

            public abstract T GetContent();
        }
    }
}
