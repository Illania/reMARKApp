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

        protected override Row GetNewRow()
        {
            return new SimpleRow(Context, this);
        }

        protected override void AddButton_Click(object sender, EventArgs e)
        {
            AddRow();
        }

        abstract protected void TextChanged(string newText);

        protected class SimpleRow : Row
        {
            readonly AppCompatEditText editText;

            public SimpleRow(Context context, AbstractStringSingleRowView stringSingleRowView)
                : base(context, stringSingleRowView)
            {
                editText = new AppCompatEditText(context);

                var editTextLp = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 1.0f)
                {
                    Gravity = (int)GravityFlags.CenterVertical,
                };

                editText.RequestFocus();
                editText.TextChanged += EditText_TextChanged;
                Layout.AddView(editText, 0, editTextLp);
            }

            void EditText_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
            {
                (ParentView as AbstractStringSingleRowView).TextChanged(editText.Text);
            }

            protected override void UpdateRow()
            {
                editText.Text = Content;
            }

            public override bool ContainsValidContent() => true;

        }

    }
}
