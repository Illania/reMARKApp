using System;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;

namespace Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews
{
    public abstract class SingleRowView<T> : ExpandableView<T> where T : class
    {
        protected SingleRowView(Context context, int titleResourceId)
            : base(context, titleResourceId, true)
        {
        }

        protected abstract class SimpleRow : Row
        {
            protected SimpleRow(Context context, T content)
                : base(context, content)
            {
                var editText = new AppCompatEditText(context);

                var editTextLp = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 1.0f)
                {
                    Gravity = (int)GravityFlags.CenterVertical,
                };

                editText.RequestFocus();
                Layout.AddView(editText, 0, editTextLp);
            }
        }

    }
}
