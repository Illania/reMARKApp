using System;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Mark5.Mobile.Droid.Ui.Fragments;
using System.Linq;

namespace Mark5.Mobile.Droid.Ui.Common
{
    public class DividerItemDecorator : RecyclerView.ItemDecoration
    {
        readonly Drawable divider;
        readonly int[] idToSkip;

        public DividerItemDecorator(Context context, params int[] idToSkip)
        {
            this.idToSkip = idToSkip;
            divider = ContextCompat.GetDrawable(context, Resource.Drawable.line_divider);
        }

        public override void OnDrawOver(Canvas c, RecyclerView parent, RecyclerView.State state)
        {
            var left = parent.PaddingLeft;
            var right = parent.Width - parent.PaddingRight;

            for (var i = 0; i < parent.ChildCount - 1; i++)
            {
                var child = parent.GetChildAt(i);

                if (child != null && idToSkip.Contains(child.Id))
                    continue;

                var nextChild = parent.GetChildAt(i + 1);

                if (nextChild != null && idToSkip.Contains(nextChild.Id))
                    continue;

                var p = (RecyclerView.LayoutParams) child.LayoutParameters;

                var top = child.Bottom + p.BottomMargin;
                var bottom = top + divider.IntrinsicHeight;

                divider.SetBounds(left, top, right, bottom);
                divider.Draw(c);
            }
        }
    }
}