using System;
using Android.Content;
using Android.Support.Design.Widget;
using Android.Views;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public abstract class AbstractSimpleFieldView : AddEditContactView
    {
        TextInputEditText contentEditText;

        protected string Content { get => contentEditText.Text; set => contentEditText.Text = value; }

        protected AbstractSimpleFieldView(Context context, int hintResourceId, bool floatingHint, bool editable = true)
            : base(context)
        {
            var layout = new TextInputLayout(Context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
            };
            AddView(layout);

            contentEditText = new TextInputEditText(Context)
            {
                LayoutParameters = new Android.Widget.LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                InputType = Android.Text.InputTypes.TextFlagNoSuggestions,
            };
            contentEditText.SetHint(hintResourceId);

            contentEditText.TextChanged += ContentChanged;
            contentEditText.Click += ContentClicked;
            contentEditText.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);

            if (!editable)
            {
                contentEditText.Focusable = false;
                contentEditText.KeyListener = null;
            }

            layout.AddView(contentEditText);
            layout.HintEnabled = floatingHint;

            var topBottomDistance = floatingHint ? 0 : DistanceSmall;
            var leftDistance = DistanceLarge;
            var rightDistance = DistanceLarge + ConversionUtils.ConvertDpToPixels(24) + DistanceSmall;

            SetPadding(leftDistance, topBottomDistance, rightDistance, topBottomDistance);
        }

        protected virtual void ContentClicked(object sender, EventArgs e) { }

        protected virtual void ContentChanged(object sender, Android.Text.TextChangedEventArgs e) { }
    }
}
