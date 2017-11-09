using System;
using Android.Content;
using Android.Graphics;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.AddEditContactViews
{
    public abstract class AbstractSimpleFieldView : AddEditContactView
    {
        TextInputEditText contentEditText;
        AppCompatImageButton deleteButton;

        int errorResourceId;

        protected string Content { get => contentEditText.Text; set => contentEditText.Text = value; }

        protected AbstractSimpleFieldView(Context context, int hintResourceId = -1, bool floatingHint = false, bool editable = true,
                                          int errorResourceId = -1,
                                          InputTypes inputType = InputTypes.TextFlagNoSuggestions
                                          | InputTypes.TextFlagCapSentences
                                          | InputTypes.ClassText)
                                         : base(context)
        {
            Orientation = Horizontal;

            this.errorResourceId = errorResourceId;

            var textInputLayout = new TextInputLayout(Context)
            {
                LayoutParameters = new LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1.0f),
            };
            AddView(textInputLayout);

            contentEditText = new TextInputEditText(Context)
            {
                LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                InputType = inputType,
            };

            if (hintResourceId != -1)
            {
                contentEditText.SetHint(hintResourceId);
            }

            contentEditText.TextChanged += ContentChanged;
            contentEditText.Click += ContentClicked;
            contentEditText.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);

            if (!editable)
            {
                contentEditText.Focusable = false;
                contentEditText.KeyListener = null;
            }

            textInputLayout.AddView(contentEditText);
            textInputLayout.HintEnabled = floatingHint;

            var topBottomDistance = floatingHint ? 0 : DistanceSmall;
            var leftDistance = DistanceLarge;
            var rightDistance = DistanceLarge;

            SetPadding(leftDistance, topBottomDistance, rightDistance, topBottomDistance);

            deleteButton = new AppCompatImageButton(Context);

            deleteButton.SetImageResource(Resource.Drawable.remove);
            deleteButton.SetColorFilter(new Color(ContextCompat.GetColor(context, Resource.Color.brown)));

            var addButtonLp = new LayoutParams(Conversion.ConvertDpToPixels(24), Conversion.ConvertDpToPixels(24))
            {
                TopMargin = DistanceSmall,
                LeftMargin = DistanceNormal,
                Gravity = (int)GravityFlags.Top,
            };
            deleteButton.LayoutParameters = addButtonLp;
            deleteButton.Click += DeleteButtonClicked;
            deleteButton.Visibility = ViewStates.Invisible;
            AddView(deleteButton);
        }

        protected void SetHintResId(int resId)
        {
            contentEditText.SetHint(resId);
        }

        protected void SetError(bool errorValue)
        {
            if (errorResourceId <= 0)
            {
                throw new InvalidOperationException("Need to set the resource id for error before using it!");
            }

            contentEditText.Error = errorValue ? Context.GetString(errorResourceId) : null;
        }

        protected void ShowDeleteButton()
        {
            deleteButton.Visibility = ViewStates.Visible;
        }

        protected void HideDeleteButton()
        {
            deleteButton.Visibility = ViewStates.Invisible;
        }

        protected virtual void DeleteButtonClicked(object sender, EventArgs e) { }

        protected virtual void ContentClicked(object sender, EventArgs e) { }

        protected virtual void ContentChanged(object sender, TextChangedEventArgs e) { }
    }
}
