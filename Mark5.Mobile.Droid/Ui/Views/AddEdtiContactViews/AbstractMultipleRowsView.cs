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
    public abstract class AbstractMultipleRowsView<T> : AddEditContactView
    {
        AppCompatEditText titleEditText;
        AppCompatTextView titleTextView;
        AppCompatImageButton addButton;

        bool isSingleRow;

        protected List<Row> Rows = new List<Row>();

        protected AbstractMultipleRowsView(Context context, int titleResourceId, bool isSingleRow) : base(context)
        {
            this.isSingleRow = isSingleRow;

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

        virtual protected void AddRow(T content = default(T))
        {
            if (isSingleRow)
                addButton.Visibility = ViewStates.Gone;

            titleEditText.Visibility = ViewStates.Gone;
            titleTextView.Visibility = ViewStates.Visible;

            var row = GetNewRow(content);
            row.DeleteClicked += (sender, e) => RemoveRow(sender as Row);

            Rows.Add(row);
            ContentLayout.AddView(row.Layout);
        }

        void RemoveRow(Row row)
        {
            ContentLayout.RemoveView(row.Layout);
            Rows.Remove(row);

            if (!Rows.Any())
            {
                titleEditText.Visibility = ViewStates.Visible;
                titleTextView.Visibility = ViewStates.Gone;
                addButton.Visibility = ViewStates.Visible;
            }
        }

        abstract protected Row GetNewRow(T content);

        #region Utilities

        static public AppCompatImageButton GetButton(Context context, bool plus = true)
        {
            var button = new AppCompatImageButton(context);

            button.SetImageResource(Resource.Drawable.add);
            button.SetColorFilter(plus ? new Color(ContextCompat.GetColor(context, Resource.Color.blue)) : Color.Red);

            var addButtonLp = new LayoutParams(ConversionUtils.ConvertDpToPixels(24), ConversionUtils.ConvertDpToPixels(24))
            {
                LeftMargin = DistanceNormal,
                Gravity = (int)GravityFlags.Top,
            };
            button.LayoutParameters = addButtonLp;
            return button;
        }

        #endregion

        abstract protected class Row
        {
            public event EventHandler DeleteClicked = delegate { };

            public LinearLayoutCompat Layout { get => LinearLayout; }

            readonly AppCompatImageButton deleteButton;

            protected readonly LinearLayoutCompat LinearLayout;
            protected readonly T Content;
            protected readonly Context Context;

            protected Row(Context context, T content)
            {
                Context = context;
                Content = content;

                LinearLayout = new LinearLayoutCompat(context)
                {
                    LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                };

                deleteButton = GetButton(context, false);
                deleteButton.Click += (sender, e) => DeleteClicked(this, EventArgs.Empty);
                LinearLayout.AddView(deleteButton);
            }

            public abstract T GetContent();

            public abstract bool ContainsValidContent();
        }
    }
}
