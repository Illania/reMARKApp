using System;
using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public class ExpandableView : AddEditContactView
    {
        AppCompatEditText titleEditText;
        AppCompatTextView titleTextView;
        AppCompatImageButton addButton;

        bool singleAdd;

        public ExpandableView(Context context, bool singleAdd = false) : base(context)
        {
            this.singleAdd = singleAdd;

            SetPadding(DistanceLarge, DistanceNormal, DistanceLarge, DistanceNormal);

            titleEditText = new AppCompatEditText(context)
            {
                KeyListener = null,
                Hint = "Test"
            };

            titleEditText.Focusable = false;
            titleEditText.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            titleEditText.Click += HintEditText_Click;
            var hintEditTextLp = new LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1.0f)
            {
                Gravity = (int)GravityFlags.CenterVertical,
                RightMargin = DistanceNormal,
            };

            TopLayout.AddView(titleEditText, hintEditTextLp);

            titleTextView = new AppCompatTextView(context)
            {
                Text = "Test"
            };
            var titleTextViewLp = new LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1.0f)
            {
                Gravity = (int)GravityFlags.CenterVertical,
                RightMargin = DistanceNormal,
            };
            titleTextView.Visibility = ViewStates.Gone;
            titleTextView.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            titleTextView.SetPadding(titleEditText.PaddingLeft, titleEditText.PaddingTop, titleEditText.PaddingRight, titleEditText.PaddingBottom);
            TopLayout.AddView(titleTextView, titleTextViewLp);

            addButton = GetButton(true);
            addButton.Click += AddButton_Click;
            TopLayout.AddView(addButton);
        }

        public override void RefreshView()
        {
            throw new NotImplementedException();
        }

        public override void UpdateContact()
        {
            throw new NotImplementedException();
        }

        void AddButton_Click(object sender, EventArgs e)
        {
            AddRow();
        }

        void HintEditText_Click(object sender, EventArgs e)
        {
            AddRow();
        }

        void AddRow()
        {
            if (singleAdd)
                addButton.Visibility = ViewStates.Gone;

            titleEditText.Visibility = ViewStates.Gone;
            titleTextView.Visibility = ViewStates.Visible;

            var layout = new LinearLayoutCompat(Context);
            layout.Orientation = Horizontal;
            ContentLayout.AddView(layout, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

            var editText = new AppCompatEditText(Context);

            var editTextLp = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 1.0f)
            {
                Gravity = (int)GravityFlags.CenterVertical,
            };

            editText.RequestFocus();
            layout.AddView(editText, editTextLp);

            var button = GetButton(false);
            button.Click += (s, e) => RemoveRow(layout);
            layout.AddView(button);
        }

        #region Utilities

        AppCompatImageButton GetButton(bool addButton = true)
        {
            var button = new AppCompatImageButton(Context);

            var drawable = ContextCompat.GetDrawable(Context, Resource.Drawable.circle).GetConstantState().NewDrawable();
            drawable.SetColorFilter(addButton ? new Color(ContextCompat.GetColor(Context, Resource.Color.blue)) : Color.Red, PorterDuff.Mode.SrcAtop);

            button.SetImageDrawable(drawable);

            var addButtonLp = new LayoutParams(ConversionUtils.ConvertDpToPixels(20), ConversionUtils.ConvertDpToPixels(20))
            {
                LeftMargin = DistanceNormal,
                Gravity = (int)GravityFlags.CenterVertical,
            };
            button.LayoutParameters = addButtonLp;
            return button;
        }

        #endregion

        void RemoveRow(LinearLayoutCompat layout)
        {
            ContentLayout.RemoveView(layout);

            if (ContentLayout.ChildCount == 0)
            {
                titleEditText.Visibility = ViewStates.Visible;
                titleTextView.Visibility = ViewStates.Gone;
                addButton.Visibility = ViewStates.Visible;
            }
        }
    }
}
