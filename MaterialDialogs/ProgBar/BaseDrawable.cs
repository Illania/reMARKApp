using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.Annotation;

namespace MaterialDialogs.ProgBar
{
    public abstract class BaseDrawable : Drawable, ITintableDrawable
    {
        int mAlpha = 0xFF;
        ColorFilter colorFilter;
        ColorStateList tintList;
        PorterDuff.Mode tintMode = PorterDuff.Mode.SrcIn;
        PorterDuffColorFilter tintFilter;

        DummyConstantState mConstantState = new DummyConstantState();

        public override int Alpha
        {
            get
            {
                return mAlpha;
            }
        }

		public override void SetAlpha(int alpha)
		{
			if (mAlpha != alpha)
			{
				mAlpha = alpha;
				InvalidateSelf();
			}
		}

        public override ColorFilter ColorFilter
        {
            get
            {
                return colorFilter;
            }
        }

        public override void SetColorFilter([Nullable] ColorFilter colorFilter)
        {
			this.colorFilter = colorFilter;
			InvalidateSelf();
   		}

        public override void SetTint([ColorInt] int tintColor)
        {
            SetTintList(ColorStateList.ValueOf(new Color(tintColor)));
        }

        public override void SetTintList([Nullable] ColorStateList tint)
        {
			tintList = tint;
			if (UpdateTintFilter())
			    InvalidateSelf();
        }

        public override void SetTintMode([NonNull] PorterDuff.Mode tintMode)
        {
			this.tintMode = tintMode;
			if (UpdateTintFilter())
				InvalidateSelf();	
        }

        public override bool IsStateful
        {
            get
            {
                return tintList != null && tintList.IsStateful;
            }
        }

        protected override bool OnStateChange(int[] state)
        {
            return UpdateTintFilter();
        }

		bool UpdateTintFilter()
		{
			if (tintList == null || tintMode == null)
			{
				var hadTintFilter = tintFilter != null;
				tintFilter = null;
				return hadTintFilter;
			}

            var tintColor = tintList.GetColorForState(GetState(), Color.Transparent);
			// They made PorterDuffColorFilter.setColor() and setMode() @hide.
			tintFilter = new PorterDuffColorFilter(new Color(tintColor), tintMode);
			return true;
		}

		public override int Opacity => (int)Format.Translucent;

		public override void Draw(Canvas canvas)
		{
            var bounds = Bounds;
            if (bounds.Width() == 0 || bounds.Height() == 0)
			    return;
			
            var saveCount = canvas.Save();
            canvas.Translate(bounds.Left, bounds.Top);
            OnDraw(canvas, bounds.Width(), bounds.Height());
			canvas.RestoreToCount(saveCount);
		}

		internal ColorFilter GetColorFilterForDrawing()
		{
			return colorFilter ?? tintFilter;
		}

		protected abstract void OnDraw(Canvas canvas, int width, int height);

		// Workaround LayerDrawable.ChildDrawable which calls getConstantState().newDrawable()
		// without checking for null.
		// We are never inflated from XML so the protocol of ConstantState does not apply to us. In
		// order to make LayerDrawable happy, we return ourselves from DummyConstantState.newDrawable().
	    public override ConstantState GetConstantState()
        {
            return mConstantState;
        }

        class DummyConstantState : ConstantState
        {
            public override ConfigChanges ChangingConfigurations => 0;

            public override Drawable NewDrawable()
            {
                return null;
            }
        }
    }
}
