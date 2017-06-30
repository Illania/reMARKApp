using System;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public abstract class AbstractStringSingleRowView : AbstractMultipleRowsView<string>
    {
        protected AbstractStringSingleRowView(Context context, int titleResourceId)
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
                editText.Text = content;
                Layout.AddView(editText, 0, editTextLp);
            }

            public override string GetContent() => editText.Text;

            public override bool ContainsValidContent() => true;
        }

    }
}
