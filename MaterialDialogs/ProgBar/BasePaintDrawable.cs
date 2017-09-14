using Android.Graphics;

namespace MaterialDialogs.ProgBar
{
    public abstract class BasePaintDrawable : BaseDrawable
    {
        Paint mPaint;

        protected override void OnDraw(Canvas canvas, int width, int height)
        {
			if (mPaint == null)
			{
                mPaint = new Paint()
                {
                    AntiAlias = true,
                    Color = Color.Black
                };
                OnPreparePaint(mPaint);
			}
            mPaint.Alpha = Alpha;
            mPaint.SetColorFilter(GetColorFilterForDrawing());
			OnDraw(canvas, width, height, mPaint);
        }

        protected abstract void OnPreparePaint(Paint paint);
        protected abstract void OnDraw(Canvas canvas, int width, int height, Paint paint);
    }
}
