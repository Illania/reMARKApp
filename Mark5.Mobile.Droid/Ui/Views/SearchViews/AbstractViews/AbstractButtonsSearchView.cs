using System;
using System.Globalization;
using Android.Content;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Views;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public abstract class AbstractButtonsSearchView<T> : AbstractSearchView<T>
    {
        protected AbstractButtonsSearchView(Context context)
            : base(context)
        {
            Orientation = Horizontal;
            SetPadding(0, 0, 0, 0);

            DividerDrawable = ContextCompat.GetDrawable(Context, Resource.Drawable.search_divider_vertical);
            ShowDividers = ShowDividerMiddle;
        }

        protected void AddButtons(params CustomButton[] buttons)
        {
            foreach (var button in buttons)
                AddView(button, new LayoutParams(0, ViewGroup.LayoutParams.MatchParent, 1));
        }

        public class CustomButton : AppCompatTextView, IOnLayoutChangeListener
        {
            int buttonTextStyleNormalResourceId = Resource.Style.searchViewButtonNormal;
            int buttonTextStyleSelectedResourceId = Resource.Style.searchViewButtonSelected;

            readonly Context context;
            readonly Func<CustomButton, bool> clickedAction; //Returns true if the button should change selected state

            public CustomButton(Context context, int stringResourceId, Func<CustomButton, bool> clickedAction = null)
                : base(context)
            {
                AddOnLayoutChangeListener(this);
                Gravity = GravityFlags.Center;

                var horizontalPaddingValue = Conversion.ConvertDpToPixels(10);
                var verticalPaddingValue = Conversion.ConvertDpToPixels(16);

                SetPadding(horizontalPaddingValue, verticalPaddingValue, horizontalPaddingValue, verticalPaddingValue);

                this.context = context;
                this.clickedAction = clickedAction;

                Text = context.GetString(stringResourceId).ToUpper(CultureInfo.CurrentCulture);

                Background = ContextCompat.GetDrawable(Context, Resource.Drawable.search_button_background);

                Click += CustomButton_Click;
                UpdateTextAppearance();
            }

            void CustomButton_Click(object sender, EventArgs e)
            {
                if (clickedAction == null || clickedAction(this))
                    Selected = !Selected;
                UpdateTextAppearance();
            }

            void UpdateTextAppearance()
            {
                var previousTextSize = TextSize;
                this.SetTextAppearanceCompat(context, Selected ? buttonTextStyleSelectedResourceId : buttonTextStyleNormalResourceId);
                SetTextSize(Android.Util.ComplexUnitType.Px, previousTextSize);
            }

            public void UpdateSelectedState(bool selected)
            {
                Selected = selected;
                UpdateTextAppearance();
            }

            void IOnLayoutChangeListener.OnLayoutChange(View v, int left, int top, int right, int bottom, int oldLeft, int oldTop, int oldRight, int oldBottom)
            {
                var maxTextSize = TextSize;
                var targetWidth = Width - PaddingLeft - PaddingRight;
                var paint = new TextPaint();

                paint.Set(Paint);
                paint.TextSize = maxTextSize;

                var newTextSize = maxTextSize;

                while (paint.MeasureText(Text, 0, Text.Length) > targetWidth)
                {
                    newTextSize -= 1;
                    paint.TextSize = newTextSize;
                }

                SetTextSize(Android.Util.ComplexUnitType.Px, newTextSize);
            }
        }
    }
}