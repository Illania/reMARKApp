using System;
using Android.Content;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Views;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public abstract class AbstractSimpleFieldView : AddEditContactView
    {
        AppCompatEditText contentEditText;

        protected string Content { get => contentEditText.Text; set => contentEditText.Text = value; }

        protected AbstractSimpleFieldView(Context context, int hintResourceId, bool floatingHint, bool editable = true)
            : base(context)
        {
            if (floatingHint)
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
                layout.AddView(contentEditText);
            }
            else
            {
                contentEditText = new AppCompatEditText(Context)
                {
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                    InputType = Android.Text.InputTypes.TextFlagNoSuggestions,
                };
                contentEditText.SetHint(hintResourceId);
                AddView(contentEditText);
            }

            contentEditText.TextChanged += ContentChanged;
            contentEditText.Click += ContentClicked;

            if (!editable)
            {
                contentEditText.Focusable = false;
                contentEditText.KeyListener = null;
            }
        }

        protected virtual void ContentClicked(object sender, EventArgs e) { }

        protected virtual void ContentChanged(object sender, Android.Text.TextChangedEventArgs e) { }
    }
}
