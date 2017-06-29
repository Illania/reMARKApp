using System;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public abstract class StringSingleRow : MultipleRowsView<string>
    {
        protected StringSingleRow(Context context, int titleResourceId)
            : base(context, titleResourceId, true)
        {
        }

        protected override Row GetNewRow(string content = null)
        {
            return new SimpleRow(Context, content);
        }

        protected class SimpleRow : Row
        {
            readonly AppCompatEditText editText;

            public SimpleRow(Context context, string content)
                : base(context, content)
            {
                editText = new AppCompatEditText(context);

                var editTextLp = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 1.0f)
                {
                    Gravity = (int)GravityFlags.CenterVertical,
                };

                editText.RequestFocus();
                Layout.AddView(editText, 0, editTextLp);
            }

            public override string GetContent()
            {
                return editText.Text;
            }
        }

    }
}
