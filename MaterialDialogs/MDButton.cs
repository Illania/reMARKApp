using Android.Content;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace MaterialDialogs
{
    public class MDButton : TextView
    {
        bool stacked;
        GravityEnum stackedGravity;

        int stackedEndPadding;
        Drawable stackedBackground;
        Drawable defaultBackground;

        public MDButton(Context context, IAttributeSet attrs) 
            : base(context, attrs)
        {
            Init(context);
        }

        public MDButton(Context context, IAttributeSet attrs, int defStyleAttr) 
            : base(context, attrs, defStyleAttr)
        {
            Init(context);
        }

        void Init(Context context)
        {
            stackedEndPadding = context.Resources.GetDimensionPixelSize(Resource.Dimension.md_dialog_frame_margin);
            stackedGravity = GravityEnum.End;
        }

        /*
         * Set if button should be displayed in stack mode. Only to be called
         * from MDRootLayout's onMeasure method.
         */
        public void SetStacked(bool stacked, bool force)
        {
            if (this.stacked != stacked || force)
                base.Gravity = stacked ? (GravityFlags.CenterVertical | stackedGravity.GetGravityInt()) : GravityFlags.Center;

            TextAlignment = stacked ? stackedGravity.GetTextAlignment() : TextAlignment.Center;
            Background = stacked ? stackedBackground : defaultBackground;
            if (stacked)
                SetPadding(stackedEndPadding, PaddingTop, stackedEndPadding, PaddingBottom);

            this.stacked = stacked;
        }

        public void SetStackedGravity(GravityEnum gravity)
        {
            stackedGravity = gravity;
        }

        public void SetStackedSelector(Drawable d)
        {
            stackedBackground = d;
            if (stacked)
                SetStacked(true, true);
        }

        public void SetDefaultSelector(Drawable d)
        {
            defaultBackground = d;
            if (!stacked)
                SetStacked(false, true);
        }
    }
}