using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;

namespace Mark5.Mobile.Droid.Views.Common
{

    public class DividerItemDecorator : RecyclerView.ItemDecoration
    {

        readonly Drawable divider;

        public DividerItemDecorator(Context context)
        {
            divider = ContextCompat.GetDrawable(context, Resource.Drawable.line_divider);
        }

        public override void OnDrawOver(Canvas c, RecyclerView parent, RecyclerView.State state)
        {
            var left = parent.PaddingLeft;
            var right = parent.Width - parent.PaddingRight;

            for (var i = 0; i < parent.ChildCount; i++)
            {
                var child = parent.GetChildAt(i);
                var p = (RecyclerView.LayoutParams)child.LayoutParameters;

                var top = child.Bottom + p.BottomMargin;
                var bottom = top + divider.IntrinsicHeight;

                divider.SetBounds(left, top, right, bottom);
                divider.Draw(c);
            }
        }
    }
}