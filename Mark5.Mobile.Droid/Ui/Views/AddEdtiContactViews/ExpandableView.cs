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

        public ExpandableView(Context context) : base(context)
        {
            SetPadding(DistanceNormal, DistanceNormal, DistanceNormal, DistanceNormal);

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

            addButton = new AppCompatImageButton(Context);
            addButton.SetImageResource(Resource.Drawable.add);
            addButton.SetColorFilter(new Color(ContextCompat.GetColor(Context, Resource.Color.blue)));

            var addButtonLp = new LayoutParams(ConversionUtils.ConvertDpToPixels(24), ConversionUtils.ConvertDpToPixels(24))
            {
                Gravity = (int)GravityFlags.CenterVertical,
            };
            TopLayout.AddView(addButton, addButtonLp);
        }

        public override void RefreshView()
        {
            throw new NotImplementedException();
        }

        public override void UpdateContact()
        {
            throw new NotImplementedException();
        }

        void HintEditText_Click(object sender, EventArgs e)
        {
            titleEditText.Visibility = ViewStates.Gone;
            titleTextView.Visibility = ViewStates.Visible;

            var editText = new AppCompatEditText(Context);

            var hintEditTextLp = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 1.0f)
            {
                Gravity = (int)GravityFlags.CenterVertical,
            };

            editText.RequestFocus();
            ContentLayout.AddView(editText, hintEditTextLp);
        }
    }
}
